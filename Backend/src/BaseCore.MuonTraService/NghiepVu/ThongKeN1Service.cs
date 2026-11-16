using BaseCore.Common.ThoiGian;
using BaseCore.MuonTraService.DuLieu;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record DiemMuonTra30(DateOnly Ngay, int Muon, int Tra);
public sealed record DongTopSach(string TenSach, string TacGia, int SoLuot);
public sealed record DiemNgay(DateOnly Ngay, int SoLuot);

/// <summary>Trang tổng quan N1 – Mượn trả sách (file 09 mục 3). Đặt chỗ ẩn đến Pha 6 (GIA_DINH D50).</summary>
public sealed record TongQuanN1Dto(
    DateOnly HomNay, int LuotMuonHomNay, int DangMuon, int QuaHan,
    IReadOnlyList<DiemMuonTra30> MuonTra30Ngay, IReadOnlyList<DongTopSach> Top10, IReadOnlyList<DiemNgay> TheoNgayTrongNam);

/// <summary>
/// Số liệu trang tổng quan Bộ phận mượn trả sách: KPI (lượt mượn hôm nay, đang mượn, quá hạn), lượt mượn – trả 30 ngày,
/// Top 10 sách mượn nhiều 12 tháng (tạm gộp theo SACH.ten_sach – GIA_DINH D50), lượt mượn theo ngày 365 ngày (heatmap lịch).
/// Đang mượn/quá hạn dùng cùng quy tắc với báo cáo MB06 (BaoCaoMuonTraService.SachChuaTraAsync).
/// </summary>
public sealed class ThongKeN1Service(MuonTraDbContext db, BaoCaoMuonTraService baoCao, IClock clock)
{
    public async Task<TongQuanN1Dto> TongQuanAsync(CancellationToken ct)
    {
        var homNay = clock.HomNay;
        var tu30 = homNay.AddDays(-29);
        var tu365 = homNay.AddDays(-364);

        var chuaTra = await baoCao.SachChuaTraAsync(homNay, ct);

        var muon = await db.CtPhieuMuon.AsNoTracking().Where(x => x.NgayMuon >= tu365 && x.NgayMuon <= homNay)
            .GroupBy(x => x.NgayMuon).Select(g => new { Ngay = g.Key, So = g.Count() }).ToDictionaryAsync(x => x.Ngay, x => x.So, ct);
        var tra = await (from x in db.CtPhieuTra.AsNoTracking()
                         join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
                         where pt.NgayTra >= tu30 && pt.NgayTra <= homNay
                         group x by pt.NgayTra into g
                         select new { Ngay = g.Key, So = g.Count() }).ToDictionaryAsync(x => x.Ngay, x => x.So, ct);

        var top = await (from cm in db.CtPhieuMuon.AsNoTracking()
                         join s in db.Sach on cm.MaBanSach equals s.MaBanSach
                         where cm.NgayMuon >= tu365 && cm.NgayMuon <= homNay
                         group s by s.TenSach into g
                         orderby g.Count() descending, g.Key
                         select new { TenSach = g.Key, TacGia = g.Max(x => x.TacGia), SoLuot = g.Count() })
            .Take(10).ToListAsync(ct);

        return new TongQuanN1Dto(homNay, muon.GetValueOrDefault(homNay), chuaTra.Count, chuaTra.Count(x => x.SoNgayQuaHan > 0),
            Enumerable.Range(0, 30).Select(i => tu30.AddDays(i)).Select(n => new DiemMuonTra30(n, muon.GetValueOrDefault(n), tra.GetValueOrDefault(n))).ToList(),
            top.Select(x => new DongTopSach(x.TenSach, x.TacGia, x.SoLuot)).ToList(),
            Enumerable.Range(0, 365).Select(i => tu365.AddDays(i)).Select(n => new DiemNgay(n, muon.GetValueOrDefault(n))).ToList());
    }

    /// <summary>Thẻ KPI “Đang mượn” / “Quá hạn” bấm được → danh sách chi tiết (file 08 mục 4).</summary>
    public async Task<List<DongSachChuaTra>> SachAsync(bool chiQuaHan, CancellationToken ct)
    {
        var ds = await baoCao.SachChuaTraAsync(clock.HomNay, ct);
        return chiQuaHan ? ds.Where(x => x.SoNgayQuaHan > 0).OrderByDescending(x => x.SoNgayQuaHan).ToList() : ds;
    }
}
