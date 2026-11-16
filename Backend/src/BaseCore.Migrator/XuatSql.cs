using System.Text;
using BaseCore.Entities;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BaseCore.Migrator;

/// <summary>
/// GIA_DINH D44 – xuất bộ tệp .sql (thư mục Database/) để tạo CSDL ThuVien bằng SSMS trên SQL Server cài sẵn.
/// Sinh từ chính migration / dữ liệu khởi tạo / quyền SQL mà migrator dùng nên luôn khớp mã nguồn.
/// Chạy: sh scripts/xuat-sql.sh
/// </summary>
public static class XuatSql
{
    private const string DauTep = "-- Sinh tự động bởi BaseCore.Migrator --xuat-sql (scripts/xuat-sql.sh). Không sửa tay.\n";

    public static async Task ChayAsync(string thuMuc)
    {
        Directory.CreateDirectory(thuMuc);
        var options = new DbContextOptionsBuilder<ThuVienDbContext>()
            .UseSqlServer("Server=khong-ket-noi;Database=ThuVien", sql => sql.MigrationsAssembly(typeof(XuatSql).Assembly.GetName().Name))
            .Options;
        await using var db = new ThuVienDbContext(options);

        var luocDo = db.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
        await GhiAsync(thuMuc, "01_CSDL_LuocDo.sql", $"""
            {DauTep}-- Tạo CSDL ThuVien, 4 schema (quantri, khosach, muontra, vipham), bảng, ràng buộc, sequence, thủ tục.
            -- Idempotent: chạy lại nhiều lần không lỗi, chỉ áp dụng migration còn thiếu (bảng __EFMigrationsHistory).
            IF DB_ID(N'ThuVien') IS NULL
                CREATE DATABASE [ThuVien];
            GO
            USE [ThuVien];
            GO
            SET ANSI_NULLS ON;
            SET QUOTED_IDENTIFIER ON;
            GO
            {luocDo}
            """);

        await GhiAsync(thuMuc, "02_DuLieuKhoiTao.sql", DuLieuKhoiTaoSql());

        var login = BaoMatSql.TaoSql(schema => "$(" + BaoMatSql.BienSqlCmd(schema) + ")");
        await GhiAsync(thuMuc, "03_LoginDichVu.sql", $"""
            {DauTep}-- Login SQL riêng của từng service + quyền theo schema (GIA_DINH D10, D13, D14). SID cố định để phục hồi
            -- CSDL không bị "orphan user". Chạy ở SQLCMD Mode (SSMS: Query → SQLCMD Mode).
            -- Điền 4 mật khẩu GIỐNG HỆT các biến cùng tên trong .env (mật khẩu service dùng để kết nối).
            -- :setvar trong tệp ghi đè tham số -v của sqlcmd – sửa trực tiếp các dòng dưới. KHÔNG commit tệp đã điền.
            :setvar SVC_QUANTRI_PASSWORD ""
            :setvar SVC_KHOSACH_PASSWORD ""
            :setvar SVC_MUONTRA_PASSWORD ""
            :setvar SVC_VIPHAM_PASSWORD ""
            -- Tài khoản quản trị "admin" (tùy chọn – migrator cũng tự tạo): mật khẩu ĐÃ BĂM (64 ký tự), lấy bằng
            --   docker compose run --rm migrator --bam-mat-khau "<mật khẩu admin>"
            :setvar MATKHAU_ADMIN_BAM ""
            USE [ThuVien];
            GO
            IF N'$(SVC_QUANTRI_PASSWORD)' = N'' OR N'$(SVC_KHOSACH_PASSWORD)' = N'' OR N'$(SVC_MUONTRA_PASSWORD)' = N'' OR N'$(SVC_VIPHAM_PASSWORD)' = N''
            BEGIN
                RAISERROR(N'Chưa điền đủ 4 mật khẩu SVC_*_PASSWORD ở đầu tệp (lấy từ .env).', 16, 1);
                SET NOEXEC ON;
            END
            GO
            {login}
            GO
            IF LEN(N'$(MATKHAU_ADMIN_BAM)') = 64 AND NOT EXISTS (SELECT 1 FROM quantri.NGUOIDUNG WHERE tendangnhap = N'admin')
                INSERT quantri.NGUOIDUNG (ma_nd, ten_nd, tendangnhap, matkhau, chucvu, tinhtrang, ma_nhom)
                VALUES (N'ND0001', N'Quản trị hệ thống', N'admin', N'$(MATKHAU_ADMIN_BAM)', N'Quản trị viên', {N(MienGiaTri.TinhTrangNguoiDung.HoatDong)}, N'N4');
            GO
            SET NOEXEC OFF;
            GO
            """);
        Console.WriteLine($"[migrator] Đã xuất 01_CSDL_LuocDo.sql, 02_DuLieuKhoiTao.sql, 03_LoginDichVu.sql vào {thuMuc}");
    }

    private static Task GhiAsync(string thuMuc, string ten, string noiDung) =>
        File.WriteAllTextAsync(Path.Combine(thuMuc, ten), noiDung.Replace("\r\n", "\n"), new UTF8Encoding(true));

    private static string N(string? s) => s is null ? "NULL" : "N'" + s.Replace("'", "''") + "'";

    /// <summary>Cùng quy tắc với DuLieuKhoiTao.NapAsync: thêm nhóm còn thiếu; quyền đã có chỉ cập nhật tên/mô tả;
    /// quyền mới thì cấp cho nhóm theo SPEC 3.2 (không đụng phân quyền N4 đã chỉnh).</summary>
    private static string DuLieuKhoiTaoSql()
    {
        var sb = new StringBuilder(DauTep);
        sb.AppendLine("-- Dữ liệu khởi tạo (file 02 mục 3): 4 nhóm người dùng N1–N4, danh mục QUYEN (SPEC 3.2, 3.4) và PHANQUYEN.");
        sb.AppendLine("-- Tài khoản quản trị \"admin\" do migrator tạo (mật khẩu băm PBKDF2 từ ADMIN_PASSWORD) – xem Database/README.md.");
        sb.AppendLine("USE [ThuVien];\nGO\nSET ANSI_NULLS ON;\nSET QUOTED_IDENTIFIER ON;\nSET NOCOUNT ON;\nSET XACT_ABORT ON;\nBEGIN TRAN;");
        foreach (var (ma, ten, moTa) in DuLieuKhoiTao.Nhom)
            sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM quantri.NHOMNGUOIDUNG WHERE ma_nhom = {N(ma)}) INSERT quantri.NHOMNGUOIDUNG (ma_nhom, ten_nhom, mota) VALUES ({N(ma)}, {N(ten)}, {N(moTa)});");
        foreach (var q in DuLieuQuyen.TatCa)
        {
            sb.AppendLine($"IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = {N(q.MaQuyen)})");
            sb.AppendLine($"    UPDATE quantri.QUYEN SET ten_quyen = {N(q.TenQuyen)}, mota = {N(q.MoTa)} WHERE ma_quyen = {N(q.MaQuyen)};");
            sb.AppendLine("ELSE");
            sb.AppendLine("BEGIN");
            sb.AppendLine($"    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES ({N(q.MaQuyen)}, {N(q.TenQuyen)}, {N(q.MoTa)});");
            foreach (var n in q.Nhom)
                sb.AppendLine($"    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES ({N(n)}, {N(q.MaQuyen)});");
            sb.AppendLine("END");
        }
        sb.AppendLine("COMMIT;\nGO");
        return sb.ToString();
    }
}
