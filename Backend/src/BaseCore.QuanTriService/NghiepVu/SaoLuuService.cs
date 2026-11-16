using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BaseCore.Common.Auth;
using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.QuanTriService.DuLieu;
using BaseCore.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.QuanTriService.NghiepVu;

public sealed class SaoLuuOptions
{
    public const string Muc = "SaoLuu";
    /// <summary>
    /// Thư mục sao lưu trên máy chủ SQL (SQL Server container: /var/opt/mssql/backup – volume thuvien-backup).
    /// Để trống: dùng thư mục sao lưu mặc định của SQL Server (InstanceDefaultBackupPath) – GIA_DINH D44.
    /// </summary>
    public string ThuMuc { get; set; } = string.Empty;
    public string TenCsdl { get; set; } = "ThuVien";
    /// <summary>Giờ sao lưu tự động hằng ngày (file 11 mục 2).</summary>
    public TimeOnly GioTuDong { get; set; } = new(23, 0);
    public int GiuLai { get; set; } = 14;
}

public sealed record MucThuMuc(string Ten, string DuongDan, bool LaThuMuc, long KichThuoc, DateTime? NgaySua);
public sealed record TrangThaiCongViec(string Ma, string Loai, int PhanTram, bool Xong, bool ThanhCong, string ThongBao);
public sealed record PhienDangMo(string MaNd, string TenDangNhap, DateTime TgDangNhap);

/// <summary>
/// SPEC 6.3.5 Sao lưu và phục hồi dữ liệu (04.00.09). Chạy bằng kết nối quản trị CSDL riêng (ConnectionStrings:QuanTriCsdl –
/// quyền BACKUP/RESTORE, đọc thư mục sao lưu). Công việc chạy nền; tiến trình % lấy từ thông báo STATS của SQL Server.
/// </summary>
public sealed partial class SaoLuuService(IConfiguration config, Microsoft.Extensions.Options.IOptions<SaoLuuOptions> opt,
    IServiceScopeFactory scopes, IClock clock, MocThuHoiPhien mocThuHoi, ILogger<SaoLuuService> logger)
{
    private readonly ConcurrentDictionary<string, TrangThaiCongViec> _congViec = new();
    private int _dangChay;

    private string ChuoiQuanTri => config.GetConnectionString("QuanTriCsdl")
        ?? throw new InvalidOperationException("Thiếu ConnectionStrings:QuanTriCsdl cho 04.00.09.");
    private SaoLuuOptions O => opt.Value;

    private string? _goc;

    /// <summary>Kiểm tra đường dẫn tương đối (phân cách “/”), không cho thoát ra ngoài thư mục gốc (chống “../”).</summary>
    public static string KiemTraTuongDoi(string? tuongDoi)
    {
        tuongDoi = (tuongDoi ?? string.Empty).Trim().Replace('\\', '/').Trim('/');
        if (tuongDoi.Length > 0 && (!DuongDanHopLe().IsMatch(tuongDoi) || tuongDoi.Split('/').Any(p => p is "." or "..")))
            throw LoiNghiepVu.KhongHopLe("DUONG_DAN_KHONG_HOP_LE", "Đường dẫn không hợp lệ.");
        return tuongDoi;
    }

    /// <summary>Ghép theo dấu phân cách của máy chủ SQL: Windows “\”, Linux “/”.</summary>
    public static string Ghep(string goc, string tuongDoi)
    {
        var tach = goc.Contains('\\') ? '\\' : '/';
        goc = goc.TrimEnd('/', '\\');
        return tuongDoi.Length == 0 ? goc : goc + tach + tuongDoi.Replace('/', tach);
    }

    private static string TenTep(string duongDan) => duongDan[(duongDan.LastIndexOfAny(['/', '\\']) + 1)..];

    /// <summary>Thư mục gốc: SaoLuu:ThuMuc hoặc thư mục sao lưu mặc định của SQL Server đang dùng.</summary>
    private async Task<string> GocAsync(CancellationToken ct)
    {
        if (_goc is not null) return _goc;
        if (!string.IsNullOrWhiteSpace(O.ThuMuc)) return _goc = O.ThuMuc.Trim();
        await using var cn = new SqlConnection(ChuoiQuanTri);
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400))", cn);
        return _goc = await cmd.ExecuteScalarAsync(ct) as string
            ?? throw new InvalidOperationException("Không xác định được thư mục sao lưu của SQL Server – đặt SaoLuu__ThuMuc.");
    }

    private async Task<string> DuongDanTuyetDoiAsync(string tuongDoiDaKiemTra, CancellationToken ct) =>
        Ghep(await GocAsync(ct), tuongDoiDaKiemTra);

    /// <summary>btnChon: duyệt thư mục con / tệp .bak trong thư mục sao lưu của máy chủ (GIA_DINH D20).</summary>
    public async Task<IReadOnlyList<MucThuMuc>> DuyetAsync(string? tuongDoi, CancellationToken ct)
    {
        var goc = KiemTraTuongDoi(tuongDoi);
        var tuyetDoi = await DuongDanTuyetDoiAsync(goc, ct);
        await using var cn = new SqlConnection(ChuoiQuanTri);
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("""
            SELECT file_or_directory_name, is_directory, size_in_bytes, last_write_time
            FROM sys.dm_os_enumerate_filesystem(@p, N'*')
            WHERE is_directory = 1 OR file_or_directory_name LIKE N'%' + @csdl + N'%.bak'
            """, cn);
        // Thư mục Backup của SQL Server cài sẵn dùng chung cho mọi CSDL (D44): chỉ liệt kê tệp mang tên CSDL ThuVien
        // (ThuVien_…, TuDong_ThuVien_…, TruocPhucHoi_ThuVien_…); phục hồi còn kiểm tra nội dung bằng RESTORE HEADERONLY.
        cmd.Parameters.AddWithValue("@csdl", O.TenCsdl.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]"));
        cmd.Parameters.AddWithValue("@p", tuyetDoi);
        var ds = new List<MucThuMuc>();
        try
        {
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                var ten = r.GetString(0);
                ds.Add(new MucThuMuc(ten, goc.Length == 0 ? ten : goc + "/" + ten, Convert.ToBoolean(r.GetValue(1)),
                    r.IsDBNull(2) ? 0 : r.GetInt64(2), r.IsDBNull(3) ? null : r.GetDateTime(3)));
            }
        }
        catch (SqlException ex)
        {
            throw LoiNghiepVu.KhongTimThay("KHONG_DOC_DUOC_THU_MUC", "Không đọc được thư mục sao lưu: " + ex.Message);
        }
        return ds.OrderByDescending(x => x.LaThuMuc).ThenByDescending(x => x.NgaySua).ToList();
    }

    /// <summary>Tài khoản khác chưa đăng xuất (tiền điều kiện phục hồi: các tài khoản khác đã đăng xuất).</summary>
    public async Task<IReadOnlyList<PhienDangMo>> PhienDangMoAsync(string maNdHienTai, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuanTriDbContext>();
        var tu = clock.BayGio.AddHours(-12);
        return await (from k in db.NhatKyDangNhap.AsNoTracking()
                      join n in db.NguoiDung on k.MaNd equals n.MaNd
                      where k.TgDangXuat == null && k.TgDangNhap >= tu && k.MaNd != maNdHienTai
                      orderby k.TgDangNhap descending
                      select new PhienDangMo(k.MaNd, n.TenDangNhap, k.TgDangNhap)).ToListAsync(ct);
    }

    public TrangThaiCongViec? TrangThai(string ma) => _congViec.GetValueOrDefault(ma);

    /// <summary>btnThucHien (rdoSaoLuu): tạo tệp sao lưu toàn bộ CSDL tại thư mục đã chọn, tên kèm ngày giờ.</summary>
    public string BatDauSaoLuu(string? thuMucTuongDoi, string maNd)
    {
        var thuMuc = KiemTraTuongDoi(thuMucTuongDoi);
        return ChayNen("SaoLuu", async (capNhat, ct) =>
        {
            var tep = Ghep(await DuongDanTuyetDoiAsync(thuMuc, ct), $"{O.TenCsdl}_{clock.BayGio:yyyyMMdd_HHmmss}.bak");
            await SaoLuuAsync(tep, (p) => capNhat(p, "Đang sao lưu…"), ct);
            await GhiNhatKyAsync(maNd, $"04.00.09 Sao lưu dữ liệu ra tệp {tep}", ct);
            return $"Sao lưu thành công: {TenTep(tep)}";
        });
    }

    /// <summary>
    /// btnThucHien (rdoPhucHoi): tự sao lưu dữ liệu hiện tại → SINGLE_USER → RESTORE → MULTI_USER; ghi nhật ký;
    /// hủy mọi phiên đăng nhập khác (D36).
    /// </summary>
    public string BatDauPhucHoi(string tepTuongDoi, string maNd, string? maDangNhap)
    {
        var tuongDoi = KiemTraTuongDoi(tepTuongDoi);
        if (!tuongDoi.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
            throw LoiNghiepVu.KhongHopLe("SAI_LOAI_TEP", "Vui lòng chọn tệp sao lưu .bak.");
        return ChayNen("PhucHoi", async (capNhat, ct) =>
        {
            var tep = await DuongDanTuyetDoiAsync(tuongDoi, ct);
            capNhat(0, "Đang kiểm tra tệp sao lưu…");
            await KiemTraTepCuaCsdlAsync(tep, ct);
            var duPhong =Ghep(await GocAsync(ct), $"TruocPhucHoi_{O.TenCsdl}_{clock.BayGio:yyyyMMdd_HHmmss}.bak");
            await SaoLuuAsync(duPhong, (p) => capNhat(p * 30 / 100, "Đang tự sao lưu dữ liệu hiện tại…"), ct);
            await GhiNhatKyAsync(maNd, $"04.00.09 Tự sao lưu dữ liệu hiện tại trước khi phục hồi: {duPhong}", ct);

            await using (var cn = new SqlConnection(ChuoiQuanTri))
            {
                await cn.OpenAsync(ct);
                cn.ChangeDatabase("master");
                cn.InfoMessage += (_, e) => { if (PhanTram(e.Message) is { } p) capNhat(30 + p * 70 / 100, "Đang phục hồi…"); };
                var ten = O.TenCsdl.Replace("]", "]]");
                await ChayLenhAsync(cn, $"ALTER DATABASE [{ten}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", null, ct);
                try
                {
                    await ChayLenhAsync(cn, $"RESTORE DATABASE [{ten}] FROM DISK = @tep WITH REPLACE, STATS = 5;", tep, ct);
                }
                finally
                {
                    await ChayLenhAsync(cn, $"ALTER DATABASE [{ten}] SET MULTI_USER;", null, ct);
                }
            }
            SqlConnection.ClearAllPools();
            await GhiNhatKyAsync(maNd, $"04.00.09 Phục hồi dữ liệu từ tệp {tep} (bản dự phòng trước khi phục hồi: {TenTep(duPhong)})", ct);
            var soPhien = await ThuHoiPhienKhacAsync(maDangNhap, ct);
            await GhiNhatKyAsync(maNd, $"04.00.09 Hủy mọi phiên đăng nhập khác (nhân viên và bạn đọc) sau phục hồi; đóng {soPhien} lượt đăng nhập đang mở", ct);
            return $"Phục hồi thành công từ {TenTep(tep)}. Mọi phiên đăng nhập khác đã bị hủy, người dùng phải đăng nhập lại.";
        });
    }

    /// <summary>Sao lưu tự động hằng ngày; giữ lại <see cref="SaoLuuOptions.GiuLai"/> bản gần nhất.</summary>
    public async Task SaoLuuTuDongAsync(CancellationToken ct)
    {
        var tep = Ghep(await GocAsync(ct), $"TuDong_{O.TenCsdl}_{clock.BayGio:yyyyMMdd_HHmmss}.bak");
        await SaoLuuAsync(tep, _ => { }, ct);
        var maNd = await QuanTriHoatDongAsync(ct);
        if (maNd is not null) await GhiNhatKyAsync(maNd, $"04.00.09 Sao lưu tự động hằng ngày (hệ thống): {tep}", ct);

        var cu = (await DuyetAsync(null, ct)).Where(x => !x.LaThuMuc && x.Ten.StartsWith("TuDong_"))
            .OrderByDescending(x => x.NgaySua).Skip(O.GiuLai).ToList();
        if (cu.Count == 0) return;
        await using var cn = new SqlConnection(ChuoiQuanTri);
        await cn.OpenAsync(ct);
        foreach (var x in cu) await ChayLenhAsync(cn, "EXEC master.sys.xp_delete_files @tep;", await DuongDanTuyetDoiAsync(KiemTraTuongDoi(x.DuongDan), ct), ct);
        logger.LogInformation("Đã xóa {SoTep} bản sao lưu tự động cũ", cu.Count);
    }

    private string ChayNen(string loai, Func<Action<int, string>, CancellationToken, Task<string>> viec)
    {
        if (Interlocked.CompareExchange(ref _dangChay, 1, 0) != 0)
            throw LoiNghiepVu.XungDot("DANG_CHAY", "Đang có thao tác sao lưu/phục hồi khác, vui lòng chờ.");
        var ma = Guid.NewGuid().ToString("N");
        _congViec[ma] = new TrangThaiCongViec(ma, loai, 0, false, false, "Đang chuẩn bị…");
        _ = Task.Run(async () =>
        {
            try
            {
                var kq = await viec((p, tb) => _congViec[ma] = _congViec[ma] with { PhanTram = Math.Clamp(p, 0, 99), ThongBao = tb },
                    CancellationToken.None);
                _congViec[ma] = _congViec[ma] with { PhanTram = 100, Xong = true, ThanhCong = true, ThongBao = kq };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "04.00.09 {Loai} thất bại", loai);
                _congViec[ma] = _congViec[ma] with { Xong = true, ThanhCong = false, ThongBao = "Thất bại: " + ex.Message };
            }
            finally
            {
                Interlocked.Exchange(ref _dangChay, 0);
            }
        });
        return ma;
    }

    /// <summary>BACKUP … WITH CHECKSUM. Thông báo STATS (mức thông tin) đến qua InfoMessage; lỗi thật ném SqlException.</summary>
    private async Task SaoLuuAsync(string tep, Action<int> tienTrinh, CancellationToken ct)
    {
        await using var cn = new SqlConnection(ChuoiQuanTri);
        cn.InfoMessage += (_, e) => { if (PhanTram(e.Message) is { } p) tienTrinh(p); };
        await cn.OpenAsync(ct);
        var ten = O.TenCsdl.Replace("]", "]]");
        await ChayLenhAsync(cn, $"BACKUP DATABASE [{ten}] TO DISK = @tep WITH INIT, CHECKSUM, STATS = 5, NAME = N'ULMS {ten}';", tep, ct);
    }

    private static async Task ChayLenhAsync(SqlConnection cn, string sql, string? tep, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, cn) { CommandTimeout = 0 };
        if (tep is not null) cmd.Parameters.AddWithValue("@tep", tep);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// Thư mục sao lưu dùng chung của SQL Server cài sẵn có thể chứa .bak của CSDL khác (D44) → chỉ cho phục hồi tệp
    /// mà RESTORE HEADERONLY xác nhận là bản sao lưu toàn phần của CSDL ThuVien.
    /// </summary>
    private async Task KiemTraTepCuaCsdlAsync(string tep, CancellationToken ct)
    {
        await using var cn = new SqlConnection(ChuoiQuanTri);
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("RESTORE HEADERONLY FROM DISK = @tep;", cn) { CommandTimeout = 0 };
        cmd.Parameters.AddWithValue("@tep", tep);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        var coBanToanPhan = false;
        while (await r.ReadAsync(ct))
        {
            var tenCsdl = r["DatabaseName"] as string;
            if (!string.Equals(tenCsdl, O.TenCsdl, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Tệp {TenTep(tep)} là bản sao lưu của CSDL “{tenCsdl}”, không phải {O.TenCsdl}.");
            if (Convert.ToInt32(r["BackupType"]) == 1) coBanToanPhan = true;
        }
        if (!coBanToanPhan)
            throw new InvalidOperationException($"Tệp {TenTep(tep)} không chứa bản sao lưu toàn phần của {O.TenCsdl}.");
    }

    /// <summary>
    /// D36: đặt mốc thu hồi phiên (extended property CSDL) – mọi JWT/cookie cấp trước mốc bị từ chối, trừ phiên của
    /// người thực hiện phục hồi; đóng các lượt NHATKY_DANGNHAP đang mở (tg_dangxuat = bây giờ).
    /// </summary>
    private async Task<int> ThuHoiPhienKhacAsync(string? maDangNhapMienTru, CancellationToken ct)
    {
        await using (var cn = new SqlConnection(ChuoiQuanTri))
        {
            await cn.OpenAsync(ct);
            cn.ChangeDatabase(O.TenCsdl);
            await using var cmd = new SqlCommand($"""
                IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = N'{MocThuHoiPhien.TenThuocTinh}')
                    EXEC sys.sp_updateextendedproperty @name = N'{MocThuHoiPhien.TenThuocTinh}', @value = @giaTri;
                ELSE
                    EXEC sys.sp_addextendedproperty @name = N'{MocThuHoiPhien.TenThuocTinh}', @value = @giaTri;
                """, cn);
            cmd.Parameters.AddWithValue("@giaTri", MocThuHoiPhien.MaHoa(new DateTimeOffset(clock.BayGioUtc, TimeSpan.Zero), maDangNhapMienTru));
            await cmd.ExecuteNonQueryAsync(ct);
        }
        mocThuHoi.XoaBoNho();
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuanTriDbContext>();
        var bayGio = clock.BayGio;
        return await db.NhatKyDangNhap.Where(x => x.TgDangXuat == null && x.MaDangNhap != maDangNhapMienTru)
            .ExecuteUpdateAsync(x => x.SetProperty(k => k.TgDangXuat, bayGio), ct);
    }

    private async Task GhiNhatKyAsync(string maNd, string noiDung, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuanTriDbContext>();
        await NhatKyGhi.ThemAsync(db, maNd, noiDung, clock.BayGio, ct);
        await db.SaveChangesAsync(ct);
    }

    private async Task<string?> QuanTriHoatDongAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuanTriDbContext>();
        return await db.NguoiDung.AsNoTracking()
            .Where(x => x.MaNhom == NhomNguoiDungMa.N4 && x.TinhTrang == MienGiaTri.TinhTrangNguoiDung.HoatDong)
            .OrderBy(x => x.MaNd).Select(x => x.MaNd).FirstOrDefaultAsync(ct);
    }

    private static int? PhanTram(string thongBao) =>
        SoPhanTram().Match(thongBao) is { Success: true } m ? int.Parse(m.Groups[1].Value) : null;

    [GeneratedRegex(@"(\d{1,3}) percent processed")] private static partial Regex SoPhanTram();
    [GeneratedRegex(@"^[A-Za-z0-9_\-./]+$")] private static partial Regex DuongDanHopLe();
}

/// <summary>SaoLuuTuDongService (file 11 mục 2): sao lưu hằng ngày lúc 23:00, giữ 14 bản gần nhất.</summary>
public sealed class SaoLuuTuDongService(SaoLuuService saoLuu, IClock clock, Microsoft.Extensions.Options.IOptions<SaoLuuOptions> opt,
    ILogger<SaoLuuTuDongService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var bayGio = clock.BayGio;
            var lanToi = bayGio.Date.Add(opt.Value.GioTuDong.ToTimeSpan());
            if (lanToi <= bayGio) lanToi = lanToi.AddDays(1);
            try { await Task.Delay(lanToi - bayGio, ct); }
            catch (OperationCanceledException) { return; }
            try
            {
                await saoLuu.SaoLuuTuDongAsync(ct);
                logger.LogInformation("Sao lưu tự động hoàn tất");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Sao lưu tự động thất bại");
            }
        }
    }
}
