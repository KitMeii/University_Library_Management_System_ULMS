using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.MuonTraService.DuLieu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TTDC = BaseCore.Entities.MienGiaTri.TrangThaiDatCho;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record DiemMuonTra30(DateOnly Ngay, int Muon, int Tra);
public sealed record DongTopSach(string MaDauSach, string TenSach, string TacGia, int SoLuot);
public sealed record DiemNgay(DateOnly Ngay, int SoLuot);
public sealed record DatChoTheoKetQua(string KetQua, int So);

/// <summary>Trang tổng quan N1 – Mượn trả sách (file 09 mục 3). Đặt chỗ bật lại từ Pha 6 (GIA_DINH D50).</summary>
public sealed record TongQuanN1Dto(
    DateOnly HomNay, int LuotMuonHomNay, int DangMuon, int QuaHan, int DatChoDangGiu, int DatChoSapHetHan,
    IReadOnlyList<DiemMuonTra30> MuonTra30Ngay, IReadOnlyList<DongTopSach> Top10, IReadOnlyList<DiemNgay> TheoNgayTrongNam,
    IReadOnlyList<DatChoTheoKetQua> DatChoThangNay);

/// <summary>
/// Số liệu trang tổng quan Bộ phận mượn trả sách: KPI (lượt mượn hôm nay, đang mượn, quá hạn, đặt chỗ đang giữ + sắp hết
/// hạn), lượt mượn – trả 30 ngày, Top 10 đầu sách mượn nhiều 12 tháng (CT_PHIEUMUON ⨝ DAUSACH_BANSACH – GIA_DINH D64),
/// đặt chỗ theo kết quả trong tháng (Đã nhận / Hết hạn / Đã hủy, theo tg_xuly), lượt mượn theo ngày 365 ngày (heatmap lịch).
/// Đang mượn/quá hạn dùng cùng quy tắc với báo cáo MB06 (BaoCaoMuonTraService.SachChuaTraAsync).
/// </summary>
public sealed class ThongKeN1Service(MuonTraDbContext db, BaoCaoMuonTraService baoCao, ChungTuChung chung, IClock clock,
    IOptions<DatChoOptions> datCho)
{
    public async Task<TongQuanN1Dto> TongQuanAsync(CancellationToken ct)
    {
        var homNay = clock.HomNay;
        var bayGio = clock.BayGio;
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
                         join b in db.DauSachBanSach on cm.MaBanSach equals b.MaBanSach
                         where cm.NgayMuon >= tu365 && cm.NgayMuon <= homNay
                         group cm by b.MaDauSach into g
                         select new { MaDauSach = g.Key, SoLuot = g.Count() })
            .Join(db.DauSach, x => x.MaDauSach, d => d.MaDauSach, (x, d) => new { x.MaDauSach, d.TenSach, d.TacGia, x.SoLuot })
            .OrderByDescending(x => x.SoLuot).ThenBy(x => x.TenSach).Take(10).ToListAsync(ct);

        var giu = chung.DatChoConGiu(bayGio);
        var mocNhac = bayGio.AddHours(datCho.Value.NhacTruocGio);
        var dauThang = new DateOnly(homNay.Year, homNay.Month, 1).ToDateTime(TimeOnly.MinValue);
        var dauThangSau = dauThang.AddMonths(1);
        // Đặt chỗ kết thúc trong tháng (tg_xuly); “Đang giữ” đã quá hạn mà tác vụ nền chưa quét tính là Hết hạn tại tg_hethan
        var ketQua = await db.PhieuDatCho.AsNoTracking()
            .Where(d => (d.TrangThai != TTDC.DangGiu && d.TgXuLy >= dauThang && d.TgXuLy < dauThangSau)
                        || (d.TrangThai == TTDC.DangGiu && d.TgHetHan <= bayGio && d.TgHetHan >= dauThang && d.TgHetHan < dauThangSau))
            .GroupBy(d => d.TrangThai == TTDC.DangGiu ? TTDC.HetHan : d.TrangThai)
            .Select(g => new { g.Key, So = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.So, ct);

        return new TongQuanN1Dto(homNay, muon.GetValueOrDefault(homNay), chuaTra.Count, chuaTra.Count(x => x.SoNgayQuaHan > 0),
            await giu.CountAsync(ct), await giu.CountAsync(x => x.TgHetHan <= mocNhac, ct),
            Enumerable.Range(0, 30).Select(i => tu30.AddDays(i)).Select(n => new DiemMuonTra30(n, muon.GetValueOrDefault(n), tra.GetValueOrDefault(n))).ToList(),
            top.Select(x => new DongTopSach(x.MaDauSach, x.TenSach, x.TacGia ?? string.Empty, x.SoLuot)).ToList(),
            Enumerable.Range(0, 365).Select(i => tu365.AddDays(i)).Select(n => new DiemNgay(n, muon.GetValueOrDefault(n))).ToList(),
            new[] { TTDC.DaNhan, TTDC.HetHan, TTDC.DaHuy }.Select(k => new DatChoTheoKetQua(k, ketQua.GetValueOrDefault(k))).ToList());
    }

    /// <summary>Thẻ KPI “Đang mượn” / “Quá hạn” bấm được → danh sách chi tiết (file 08 mục 4).</summary>
    public async Task<List<DongSachChuaTra>> SachAsync(bool chiQuaHan, CancellationToken ct)
    {
        var ds = await baoCao.SachChuaTraAsync(clock.HomNay, ct);
        return chiQuaHan ? ds.Where(x => x.SoNgayQuaHan > 0).OrderByDescending(x => x.SoNgayQuaHan).ToList() : ds;
    }
}
