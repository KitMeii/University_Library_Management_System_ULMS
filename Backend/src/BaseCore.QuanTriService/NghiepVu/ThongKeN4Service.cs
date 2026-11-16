using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.QuanTriService.DuLieu;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.QuanTriService.NghiepVu;

public sealed record DiemNgayN4(DateOnly Ngay, int SoLuot);
public sealed record ChuoiNhom(string MaNhom, string TenNhom, IReadOnlyList<int> SoTheoNgay);

/// <summary>Trang tổng quan N4 – Quản trị (file 09 mục 3).</summary>
public sealed record TongQuanN4Dto(
    int TaiKhoanHoatDong, int TongTaiKhoan, int DangNhapHomNay, int ThaoTacHomNay, DateTime? SaoLuuGanNhat, string? SaoLuuNoiDung,
    IReadOnlyList<DiemNgayN4> DangNhap30Ngay, IReadOnlyList<DateOnly> Ngay14, IReadOnlyList<ChuoiNhom> ThaoTacTheoNhom);

/// <summary>
/// KPI: tài khoản “Hoạt động”, lượt đăng nhập và thao tác hôm nay, lần sao lưu gần nhất (dòng NHATKY_HOATDONG “04.00.09 Sao lưu…”
/// – thủ công hoặc tự động; login svc_quantri không đọc msdb). Lượt đăng nhập 30 ngày; thao tác 14 ngày theo nhóm người dùng.
/// </summary>
public sealed class ThongKeN4Service(QuanTriDbContext db, IClock clock)
{
    public async Task<TongQuanN4Dto> TongQuanAsync(CancellationToken ct)
    {
        var homNay = clock.HomNay;
        var dauHomNay = homNay.ToDateTime(TimeOnly.MinValue);
        var tu30 = homNay.AddDays(-29);
        var tu14 = homNay.AddDays(-13);

        var tk = await db.NguoiDung.AsNoTracking().GroupBy(x => x.TinhTrang).Select(g => new { g.Key, So = g.Count() }).ToListAsync(ct);
        var dangNhap = await db.NhatKyDangNhap.AsNoTracking().Where(x => x.TgDangNhap >= tu30.ToDateTime(TimeOnly.MinValue))
            .Select(x => x.TgDangNhap).ToListAsync(ct);
        var thaoTac = await (from h in db.NhatKyHoatDong.AsNoTracking()
                             join n in db.NguoiDung on h.MaNd equals n.MaNd
                             where h.TgThucHien >= tu14.ToDateTime(TimeOnly.MinValue)
                             select new { h.TgThucHien, n.MaNhom }).ToListAsync(ct);
        var thaoTacHomNay = await db.NhatKyHoatDong.AsNoTracking().CountAsync(x => x.TgThucHien >= dauHomNay, ct);
        var saoLuu = await db.NhatKyHoatDong.AsNoTracking()
            .Where(x => x.NoiDung.StartsWith("04.00.09 Sao lưu"))
            .OrderByDescending(x => x.TgThucHien).Select(x => new { x.TgThucHien, x.NoiDung }).FirstOrDefaultAsync(ct);
        var nhom = await db.NhomNguoiDung.AsNoTracking().OrderBy(x => x.MaNhom).ToListAsync(ct);
        var ngay14 = Enumerable.Range(0, 14).Select(i => tu14.AddDays(i)).ToList();

        return new TongQuanN4Dto(
            tk.Where(x => x.Key == MienGiaTri.TinhTrangNguoiDung.HoatDong).Sum(x => x.So), tk.Sum(x => x.So),
            dangNhap.Count(t => t >= dauHomNay), thaoTacHomNay, saoLuu?.TgThucHien, saoLuu?.NoiDung,
            Enumerable.Range(0, 30).Select(i => tu30.AddDays(i))
                .Select(n => new DiemNgayN4(n, dangNhap.Count(t => DateOnly.FromDateTime(t) == n))).ToList(),
            ngay14,
            nhom.Select(g => new ChuoiNhom(g.MaNhom, g.TenNhom,
                ngay14.Select(n => thaoTac.Count(x => x.MaNhom == g.MaNhom && DateOnly.FromDateTime(x.TgThucHien) == n)).ToList())).ToList());
    }
}
