using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.KhoSachService.DuLieu;
using Microsoft.EntityFrameworkCore;
using TT = BaseCore.Entities.MienGiaTri.TinhTrangSach;

namespace BaseCore.KhoSachService.NghiepVu;

public sealed record DongKhuVuc(string MaKhuVuc, string ViTri, string LoaiKhuVuc, int SoSach);

/// <summary>Trang tổng quan N3 – Kho sách (file 09 mục 3). Tháng dạng “yyyy-MM”.</summary>
public sealed record TongQuanN3Dto(
    int TongBanSach, int SanSang, int DangDuocMuon, int HuHong, int Mat,
    IReadOnlyList<ThongKeTinhTrang> TheoTinhTrang, IReadOnlyList<DongKhuVuc> TheoKhuVuc,
    IReadOnlyList<string> Thang, IReadOnlyList<int> HuHongTheoThang, IReadOnlyList<int> MatTheoThang);

/// <summary>
/// KPI và biểu đồ theo SACH.tinhtrang (03.04.01), SACH ⨝ KHUVUC; sách hư hỏng, mất khi trả theo tháng của PHIEUTRA.ngaytra
/// (CT_PHIEUTRA.tinhtrang_tra), 12 tháng gần nhất.
/// </summary>
public sealed class ThongKeN3Service(KhoSachDbContext db, IClock clock)
{
    public const int SoThang = 12;

    public async Task<TongQuanN3Dto> TongQuanAsync(CancellationToken ct)
    {
        var dem = await db.Sach.AsNoTracking().GroupBy(s => s.TinhTrang)
            .Select(g => new { g.Key, So = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.So, ct);
        var theoKhuVuc = await (from kv in db.KhuVuc.AsNoTracking()
                                orderby kv.MaKhuVuc
                                select new DongKhuVuc(kv.MaKhuVuc, kv.ViTri, kv.LoaiKhuVuc, db.Sach.Count(s => s.MaKhuVuc == kv.MaKhuVuc)))
            .ToListAsync(ct);

        var homNay = clock.HomNay;
        var dauThang = new DateOnly(homNay.Year, homNay.Month, 1);
        var thang = Enumerable.Range(0, SoThang).Select(i => dauThang.AddMonths(i - SoThang + 1)).ToList();
        var tu = thang[0];
        var tra = await (from x in db.CtPhieuTra.AsNoTracking()
                         join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
                         where pt.NgayTra >= tu && (x.TinhTrangTra == MienGiaTri.TinhTrangTra.HuHong || x.TinhTrangTra == MienGiaTri.TinhTrangTra.Mat)
                         select new { pt.NgayTra, x.TinhTrangTra }).ToListAsync(ct);
        int Dem(DateOnly t, string tinhTrang) => tra.Count(x => x.TinhTrangTra == tinhTrang && x.NgayTra.Year == t.Year && x.NgayTra.Month == t.Month);

        return new TongQuanN3Dto(dem.Values.Sum(), dem.GetValueOrDefault(TT.SanSang), dem.GetValueOrDefault(TT.DangDuocMuon),
            dem.GetValueOrDefault(TT.HuHong), dem.GetValueOrDefault(TT.Mat),
            TT.TatCa.Select(t => new ThongKeTinhTrang(t, dem.GetValueOrDefault(t))).ToList(), theoKhuVuc,
            thang.Select(t => t.ToString("yyyy-MM")).ToList(),
            thang.Select(t => Dem(t, MienGiaTri.TinhTrangTra.HuHong)).ToList(),
            thang.Select(t => Dem(t, MienGiaTri.TinhTrangTra.Mat)).ToList());
    }
}
