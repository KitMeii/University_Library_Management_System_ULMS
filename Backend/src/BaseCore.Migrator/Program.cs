using BaseCore.Common.Auth;
using BaseCore.Common.ThoiGian;
using BaseCore.Migrator;
using BaseCore.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Migrator: chạy một lần khi `docker compose up` (các service chờ nó hoàn tất – file 14).
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Công cụ phụ (GIA_DINH D44): xuất bộ .sql cho SSMS; băm mật khẩu admin cho Database/03_LoginDichVu.sql
if (args is ["--xuat-sql", var thuMucSql])
{
    await XuatSql.ChayAsync(thuMucSql);
    return 0;
}
// GIA_DINH D77: sinh script một lần cập nhật từ khóa mẫu cho CSDL đang dùng
if (args is ["--sql-tu-khoa-mau", var tepSql])
{
    await File.WriteAllTextAsync(tepSql, DuLieuMau.SqlCapNhatTuKhoaMau(), new System.Text.UTF8Encoding(true));
    Console.WriteLine($"Đã ghi {tepSql}");
    return 0;
}
if (args is ["--bam-mat-khau", var mkCanBam])
{
    if (!MatKhauHasher.DatChinhSach(mkCanBam))
    {
        Console.Error.WriteLine("Mật khẩu chưa đạt chính sách (≥ 8 ký tự, có chữ và số).");
        return 1;
    }
    Console.WriteLine(new MatKhauHasher(config.GetValue("MatKhau:SoVongLap", 600_000)).Bam(mkCanBam));
    return 0;
}

var moiTruong = config["DOTNET_ENVIRONMENT"] ?? config["ASPNETCORE_ENVIRONMENT"] ?? "Production";
var chuoiKetNoi = config.GetConnectionString("QuanTriCsdl")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings__QuanTriCsdl (kết nối quyền sa tới CSDL ThuVien).");

Console.WriteLine($"[migrator] Môi trường: {moiTruong}");
await ChoSqlServerAsync(chuoiKetNoi);

// Bộ kiểm thử (GIA_DINH D49): dọn .bak kiểm thử khi kết thúc; xóa – tạo lại CSDL kiểm thử khi bắt đầu
if (args is ["--don-sao-luu-kiem-thu"])
{
    await CsdlKiemThu.DonSaoLuuAsync(chuoiKetNoi, config["SaoLuu:ThuMuc"]);
    return 0;
}
if (config.GetValue("ThuVien:TaoLaiCsdl", false))
    await CsdlKiemThu.TaoLaiAsync(chuoiKetNoi, config["SaoLuu:ThuMuc"]);

var options = new DbContextOptionsBuilder<ThuVienDbContext>()
    .UseSqlServer(chuoiKetNoi, sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name))
    .Options;

await using (var db = new ThuVienDbContext(options))
{
    Console.WriteLine("[migrator] Áp dụng migration...");
    await db.Database.MigrateAsync();

    Console.WriteLine("[migrator] Tạo login/quyền SQL cho từng service...");
    await BaoMatSql.ApDungAsync(db, config);

    var hasher = new MatKhauHasher(config.GetValue("MatKhau:SoVongLap", 600_000));
    var clock = new DongHoHeThong();

    Console.WriteLine("[migrator] Nạp dữ liệu khởi tạo (nhóm, quyền, tài khoản quản trị)...");
    await DuLieuKhoiTao.NapAsync(db, config, hasher, clock);

    if (config.GetValue("ThuVien:NapDuLieuMau", false))
    {
        if (string.Equals(moiTruong, "Production", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Không được nạp dữ liệu mẫu ở Production (nguyên tắc 4, file 01).");
        Console.WriteLine("[migrator] Nạp DỮ LIỆU MẪU (Development)...");
        await DuLieuMau.NapAsync(db, hasher, clock);
        await DuLieuMauChungTu.NapAsync(db, clock);
        await DuLieuMauDatCho.NapAsync(db, clock);
    }
}

Console.WriteLine("[migrator] Hoàn tất.");
return 0;

static async Task ChoSqlServerAsync(string chuoiKetNoi)
{
    var b = new SqlConnectionStringBuilder(chuoiKetNoi) { InitialCatalog = "master", ConnectTimeout = 5 };
    for (var lan = 1; ; lan++)
    {
        try
        {
            await using var cn = new SqlConnection(b.ConnectionString);
            await cn.OpenAsync();
            return;
        }
        catch (SqlException) when (lan < 60)
        {
            Console.WriteLine($"[migrator] Chờ SQL Server sẵn sàng ({lan})...");
            await Task.Delay(2000);
        }
    }
}
