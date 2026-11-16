using BaseCore.Common.ThoiGian;
using BaseCore.ViPhamService.DuLieu;
using Microsoft.EntityFrameworkCore;
using HT = BaseCore.Entities.MienGiaTri.HinhThucThanhToan;

namespace BaseCore.ViPhamService.NghiepVu;

/// <summary>Một chuỗi “số trường hợp vi phạm theo tháng” của một khoản thu (bar chồng).</summary>
public sealed record ChuoiKhoanThu(string MaKhoanThu, string NoiDungTt, IReadOnlyList<int> SoTheoThang);

/// <summary>Trang tổng quan N2 – Xử lý vi phạm (file 09 mục 3). Tháng dạng “yyyy-MM”.</summary>
public sealed record TongQuanN2Dto(
    int BienBanThangNay, decimal TienThuThangNay, int ChuaThanhToan, decimal TienChuaThanhToan,
    IReadOnlyList<string> Thang, IReadOnlyList<ChuoiKhoanThu> ViPhamTheoThang, IReadOnlyList<decimal> TienThuTheoThang,
    IReadOnlyList<DongHinhThuc> HinhThucThanhToan);

/// <summary>
/// KPI: biên bản lập trong tháng này, tiền đã thu của biên bản lập trong tháng này (ht_thanhtoan khác “Chưa thanh toán”),
/// biên bản chưa thanh toán (mọi thời điểm). Biểu đồ 12 tháng gần nhất theo tháng của thoigian_lap (GIA_DINH D52).
/// </summary>
public sealed class ThongKeN2Service(ViPhamDbContext db, IClock clock)
{
    public const int SoThang = 12;

    public async Task<TongQuanN2Dto> TongQuanAsync(CancellationToken ct)
    {
        var homNay = clock.HomNay;
        var dauThang = new DateOnly(homNay.Year, homNay.Month, 1);
        var thang = Enumerable.Range(0, SoThang).Select(i => dauThang.AddMonths(i - SoThang + 1)).ToList();
        var tu = thang[0].ToDateTime(TimeOnly.MinValue);

        var bienBan = await db.BienBan.AsNoTracking().Where(x => x.ThoiGianLap >= tu)
            .Select(x => new { x.SoBienBan, x.ThoiGianLap, x.TongTien, x.HtThanhToan }).ToListAsync(ct);
        var chiTiet = await (from cb in db.CtBienBan.AsNoTracking()
                             join bb in db.BienBan on cb.SoBienBan equals bb.SoBienBan
                             where bb.ThoiGianLap >= tu
                             select new { cb.MaKhoanThu, bb.ThoiGianLap }).ToListAsync(ct);
        var khoanThu = await db.KhoanThu.AsNoTracking().OrderBy(x => x.MaKhoanThu).ToListAsync(ct);
        var chuaTT = await db.BienBan.AsNoTracking().Where(x => x.HtThanhToan == HT.ChuaThanhToan)
            .GroupBy(_ => 1).Select(g => new { So = g.Count(), Tien = g.Sum(x => x.TongTien) }).SingleOrDefaultAsync(ct);

        static DateOnly Thang(DateTime t) => new(t.Year, t.Month, 1);
        var thangNay = bienBan.Where(b => Thang(b.ThoiGianLap) == dauThang).ToList();

        return new TongQuanN2Dto(
            thangNay.Count, thangNay.Where(b => b.HtThanhToan != HT.ChuaThanhToan).Sum(b => b.TongTien),
            chuaTT?.So ?? 0, chuaTT?.Tien ?? 0,
            thang.Select(t => t.ToString("yyyy-MM")).ToList(),
            khoanThu.Select(k => new ChuoiKhoanThu(k.MaKhoanThu, k.NoiDungTt,
                thang.Select(t => chiTiet.Count(c => c.MaKhoanThu == k.MaKhoanThu && Thang(c.ThoiGianLap) == t)).ToList())).ToList(),
            thang.Select(t => bienBan.Where(b => Thang(b.ThoiGianLap) == t && b.HtThanhToan != HT.ChuaThanhToan).Sum(b => b.TongTien)).ToList(),
            HT.TatCa.Select(h => new DongHinhThuc(h, bienBan.Count(b => b.HtThanhToan == h), bienBan.Where(b => b.HtThanhToan == h).Sum(b => b.TongTien))).ToList());
    }

    /// <summary>Thẻ KPI “Chưa thanh toán” → danh sách biên bản chưa thanh toán (mọi thời điểm).</summary>
    public async Task<List<DongBienBanBaoCao>> ChuaThanhToanAsync(CancellationToken ct)
    {
        var ds = await (from bb in db.BienBan.AsNoTracking()
                        join bd in db.BanDoc on bb.MaBanDoc equals bd.MaBanDoc
                        where bb.HtThanhToan == HT.ChuaThanhToan
                        orderby bb.ThoiGianLap
                        select new { bb.SoBienBan, bb.ThoiGianLap, bb.SoPMuon, bb.MaBanDoc, bd.HoTen, bd.LopDonVi, bb.TongTien, bb.HtThanhToan,
                            SoTruongHop = db.CtBienBan.Count(c => c.SoBienBan == bb.SoBienBan) })
            .ToListAsync(ct);
        return ds.Select(b => new DongBienBanBaoCao(b.SoBienBan, b.ThoiGianLap, b.SoPMuon, b.MaBanDoc, b.HoTen, b.LopDonVi, b.SoTruongHop,
            b.TongTien, b.HtThanhToan, false)).ToList();
    }
}
