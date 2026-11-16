using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record DongLuotMuon(string SoPMuon, string MaBanDoc, string HoTen, string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTra);
public sealed record DongLuotGiaHan(string SoPGiaHan, DateOnly NgayLapPGiaHan, string SoPMuon, string HoTen, string MaBanSach, string TenSach,
    string KetQuaGiaHan, DateOnly? HanTraMoi);
public sealed record DongLuotTra(string SoPTra, DateOnly NgayTra, string SoPMuon, string HoTen, string MaBanSach, string TenSach,
    string TinhTrangTra, int SoNgayQuaHan);
public sealed record DongSachChuaTra(string SoPMuon, string MaBanDoc, string HoTen, string MaBanSach, string TenSach, DateOnly NgayMuon,
    DateOnly HanTra, int SoNgayQuaHan);
public sealed record DiemMuonTra(DateOnly Ngay, int Muon, int GiaHan, int Tra);

/// <summary>Số liệu tổng hợp 01.04.02 – cũng là dữ liệu Báo cáo tình hình mượn trả sách MB06 (SPEC 6.4.5).</summary>
public sealed record BaoCaoMuonTraDto(
    DateOnly TuNgay, DateOnly DenNgay,
    int TongLuotMuon, int TongLuotGiaHan, int TongLuotTra, int SoSachDangMuon, int SoSachQuaHan,
    IReadOnlyList<DiemMuonTra> TheoNgay,
    IReadOnlyList<DongLuotMuon> LuotMuon, IReadOnlyList<DongLuotGiaHan> LuotGiaHan, IReadOnlyList<DongLuotTra> LuotTra,
    IReadOnlyList<DongSachChuaTra> SachQuaHan);

public sealed record KyBaoCaoNhap(DateOnly? TuNgay, DateOnly? DenNgay);
public sealed record KetQuaLapBaoCao(DateOnly NgayLap, string ThongBao);

/// <summary>
/// 01.04.02 Tổng hợp tình hình mượn trả sách (TT057–TT068) và 01.04.03 Lập Báo cáo tình hình mượn trả sách (TT069–TT073).
/// Công thức SPEC 6.4.5: lượt mượn = COUNT(CT_PHIEUMUON) có ngaymuon trong kỳ; lượt gia hạn = COUNT(CT_PHIEUGIAHAN “Đồng ý”)
/// có ngaylap_pgiahan trong kỳ; lượt trả = COUNT(CT_PHIEUTRA) có PHIEUTRA.ngaytra trong kỳ; sách quá hạn = COUNT(sách chưa trả
/// có hạn trả &lt; ngày cuối kỳ) – hạn trả hiện hành gồm các lần gia hạn “Đồng ý” lập đến ngày cuối kỳ (GIA_DINH D45, D51).
/// </summary>
public sealed class BaoCaoMuonTraService(MuonTraDbContext db, IClock clock)
{
    public async Task<BaoCaoMuonTraDto> TongHopAsync(DateOnly? tuNgay, DateOnly? denNgay, CancellationToken ct)
    {
        var (tu, den) = KyBaoCao.KiemTra(tuNgay, denNgay, clock.HomNay);

        var luotMuon = await (from cm in db.CtPhieuMuon.AsNoTracking()
                              join pm in db.PhieuMuon on cm.SoPMuon equals pm.SoPMuon
                              where cm.NgayMuon >= tu && cm.NgayMuon <= den
                              orderby cm.NgayMuon, cm.SoPMuon, cm.MaBanSach
                              select new DongLuotMuon(cm.SoPMuon, pm.MaBanDoc, pm.HoTen, cm.MaBanSach, cm.TenSach, cm.NgayMuon, cm.HanTra))
            .ToListAsync(ct);

        var luotGiaHan = await (from cg in db.CtPhieuGiaHan.AsNoTracking()
                                join pg in db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
                                join pm in db.PhieuMuon on pg.SoPMuon equals pm.SoPMuon
                                where pg.NgayLapPGiaHan >= tu && pg.NgayLapPGiaHan <= den
                                orderby pg.NgayLapPGiaHan, pg.SoPGiaHan, cg.MaBanSach
                                select new DongLuotGiaHan(pg.SoPGiaHan, pg.NgayLapPGiaHan, pg.SoPMuon, pm.HoTen, cg.MaBanSach, cg.TenSach,
                                    cg.KetQuaGiaHan, cg.HanTraMoi))
            .ToListAsync(ct);

        var luotTra = await (from x in db.CtPhieuTra.AsNoTracking()
                             join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
                             join pm in db.PhieuMuon on pt.SoPMuon equals pm.SoPMuon
                             join cm in db.CtPhieuMuon on new { pt.SoPMuon, x.MaBanSach } equals new { cm.SoPMuon, cm.MaBanSach }
                             where pt.NgayTra >= tu && pt.NgayTra <= den
                             orderby pt.NgayTra, pt.SoPTra, x.MaBanSach
                             select new DongLuotTra(pt.SoPTra, pt.NgayTra, pt.SoPMuon, pm.HoTen, x.MaBanSach, cm.TenSach, x.TinhTrangTra, x.SoNgayQuaHan))
            .ToListAsync(ct);

        var chuaTra = await SachChuaTraAsync(den, ct);
        var quaHan = chuaTra.Where(x => x.SoNgayQuaHan > 0).OrderByDescending(x => x.SoNgayQuaHan).ThenBy(x => x.SoPMuon).ToList();
        var dongY = luotGiaHan.Where(x => x.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY).ToList();

        var muonTheoNgay = luotMuon.GroupBy(x => x.NgayMuon).ToDictionary(g => g.Key, g => g.Count());
        var giaHanTheoNgay = dongY.GroupBy(x => x.NgayLapPGiaHan).ToDictionary(g => g.Key, g => g.Count());
        var traTheoNgay = luotTra.GroupBy(x => x.NgayTra).ToDictionary(g => g.Key, g => g.Count());
        var theoNgay = Enumerable.Range(0, den.DayNumber - tu.DayNumber + 1).Select(i => tu.AddDays(i))
            .Select(n => new DiemMuonTra(n, muonTheoNgay.GetValueOrDefault(n), giaHanTheoNgay.GetValueOrDefault(n), traTheoNgay.GetValueOrDefault(n)))
            .ToList();

        return new BaoCaoMuonTraDto(tu, den, luotMuon.Count, dongY.Count, luotTra.Count, chuaTra.Count, quaHan.Count,
            theoNgay, luotMuon, luotGiaHan, luotTra, quaHan);
    }

    /// <summary>
    /// Bản sách đã mượn đến <paramref name="ngay"/> mà chưa trả tính đến ngày đó (không có dòng CT_PHIEUTRA của phiếu trả cùng
    /// phiếu mượn có ngaytra ≤ ngày), kèm hạn trả hiện hành và số ngày quá hạn tính tới <paramref name="ngay"/>.
    /// </summary>
    public async Task<List<DongSachChuaTra>> SachChuaTraAsync(DateOnly ngay, CancellationToken ct)
    {
        var dong = await (from cm in db.CtPhieuMuon.AsNoTracking()
                          join pm in db.PhieuMuon on cm.SoPMuon equals pm.SoPMuon
                          where cm.NgayMuon <= ngay
                                && !(from x in db.CtPhieuTra
                                     join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
                                     where pt.SoPMuon == cm.SoPMuon && x.MaBanSach == cm.MaBanSach && pt.NgayTra <= ngay
                                     select 1).Any()
                          select new { cm.SoPMuon, pm.MaBanDoc, pm.HoTen, cm.MaBanSach, cm.TenSach, cm.NgayMuon, cm.HanTra })
            .ToListAsync(ct);
        var giaHan = await (from cg in db.CtPhieuGiaHan.AsNoTracking()
                            join pg in db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
                            where cg.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY && cg.HanTraMoi != null && pg.NgayLapPGiaHan <= ngay
                            select new { pg.SoPMuon, cg.MaBanSach, HanTraMoi = cg.HanTraMoi!.Value })
            .ToListAsync(ct);
        var hanMoi = giaHan.GroupBy(g => (g.SoPMuon, g.MaBanSach)).ToDictionary(g => g.Key, g => g.Max(x => x.HanTraMoi));
        return dong.Select(x =>
            {
                var han = hanMoi.TryGetValue((x.SoPMuon, x.MaBanSach), out var h) && h > x.HanTra ? h : x.HanTra;
                return new DongSachChuaTra(x.SoPMuon, x.MaBanDoc, x.HoTen, x.MaBanSach, x.TenSach, x.NgayMuon, han,
                    Math.Max(0, ngay.DayNumber - han.DayNumber));
            })
            .OrderBy(x => x.HanTra).ThenBy(x => x.SoPMuon).ThenBy(x => x.MaBanSach).ToList();
    }

    /// <summary>01.04.03 btnIn: kiểm tra kỳ, tự sinh ngày lập báo cáo (TT072), ghi nhật ký việc lập/in báo cáo MB06 (TT073).</summary>
    public async Task<KetQuaLapBaoCao> LapAsync(KyBaoCaoNhap n, string maNd, CancellationToken ct)
    {
        var (tu, den) = KyBaoCao.KiemTra(n.TuNgay, n.DenNgay, clock.HomNay);
        await NhatKyGhi.ThemAsync(db, maNd, $"01.04.03 Lập Báo cáo tình hình mượn trả sách (MB06) kỳ {tu:dd/MM/yyyy} – {den:dd/MM/yyyy}", clock.BayGio, ct);
        await db.SaveChangesAsync(ct);
        return new KetQuaLapBaoCao(clock.HomNay, $"Đã lập Báo cáo tình hình mượn trả sách kỳ {tu:dd/MM/yyyy} – {den:dd/MM/yyyy}.");
    }
}
