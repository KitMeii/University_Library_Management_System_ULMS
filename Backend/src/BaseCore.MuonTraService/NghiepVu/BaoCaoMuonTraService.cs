using BaseCore.Common.Loi;
using BaseCore.Common.NghiepVu;
using BaseCore.DTO.Common;
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
    IReadOnlyList<DongSachChuaTra> SachQuaHan,
    int SoDongLuotMuon = 0, int SoDongLuotGiaHan = 0, int SoDongLuotTra = 0, int SoDongSachQuaHan = 0, int CoTrang = 0);

/// <summary>Dòng lượt trả rút gọn (không nối bảng) – dùng nội bộ để đếm/gộp/phân trang.</summary>
public sealed class LuotTraGon
{
    public string SoPTra { get; init; } = null!;
    public DateOnly NgayTra { get; init; }
    public string SoPMuon { get; init; } = null!;
    public string MaBanSach { get; init; } = null!;
    public string TinhTrangTra { get; init; } = null!;
    public int SoNgayQuaHan { get; init; }
}

public sealed record KyBaoCaoNhap(DateOnly? TuNgay, DateOnly? DenNgay);
public sealed record KetQuaLapBaoCao(DateOnly NgayLap, string ThongBao);

/// <summary>
/// 01.04.02 Tổng hợp tình hình mượn trả sách (TT057–TT068) và 01.04.03 Lập Báo cáo tình hình mượn trả sách (TT069–TT073).
/// Công thức SPEC 6.4.5: lượt mượn = COUNT(CT_PHIEUMUON) có ngaymuon trong kỳ; lượt gia hạn = COUNT(CT_PHIEUGIAHAN “Đồng ý”)
/// có ngaylap_pgiahan trong kỳ; lượt trả = COUNT(CT_PHIEUTRA) có PHIEUTRA.ngaytra trong kỳ; sách quá hạn = COUNT(sách chưa trả
/// có hạn trả &lt; ngày cuối kỳ) – hạn trả hiện hành gồm các lần gia hạn “Đồng ý” lập đến ngày cuối kỳ (GIA_DINH D45, D51).
/// </summary>
public sealed class BaoCaoMuonTraService(MuonTraDbContext db, IClock clock, DbContextOptions<MuonTraDbContext> tuyChonCsdl)
{
    /// <summary>Cỡ trang mặc định của các bảng chi tiết khi XEM TRƯỚC (GIA_DINH D82 – chủ dự án duyệt 18/11/2026).</summary>
    private MuonTraDbContext Db => db;

    public const int CoTrangMacDinh = 50;
    public static readonly string[] BangChiTiet = ["luot-muon", "luot-gia-han", "luot-tra", "sach-qua-han"];

    private IQueryable<DongLuotMuon> QLuotMuon(DateOnly tu, DateOnly den) =>
        from cm in db.CtPhieuMuon.AsNoTracking()
        join pm in db.PhieuMuon on cm.SoPMuon equals pm.SoPMuon
        where cm.NgayMuon >= tu && cm.NgayMuon <= den
        orderby cm.NgayMuon, cm.SoPMuon, cm.MaBanSach
        select new DongLuotMuon(cm.SoPMuon, pm.MaBanDoc, pm.HoTen, cm.MaBanSach, cm.TenSach, cm.NgayMuon, cm.HanTra);

    /// <summary>Một trang lượt mượn: phân trang trên CT_PHIEUMUON (chỉ mục ngaymuon) rồi mới nối PHIEUMUON cho đúng các dòng của trang.</summary>
    private async Task<List<DongLuotMuon>> TrangLuotMuonAsync(DateOnly tu, DateOnly den, int boQua, int lay, CancellationToken ct)
    {
        var trang = db.CtPhieuMuon.AsNoTracking().Where(c => c.NgayMuon >= tu && c.NgayMuon <= den)
            .OrderBy(c => c.NgayMuon).ThenBy(c => c.SoPMuon).ThenBy(c => c.MaBanSach).Skip(boQua).Take(lay);
        var ds = await (from cm in trang
                        join pm in db.PhieuMuon on cm.SoPMuon equals pm.SoPMuon
                        select new DongLuotMuon(cm.SoPMuon, pm.MaBanDoc, pm.HoTen, cm.MaBanSach, cm.TenSach, cm.NgayMuon, cm.HanTra))
            .ToListAsync(ct);
        return ds.OrderBy(x => x.NgayMuon).ThenBy(x => x.SoPMuon, StringComparer.Ordinal).ThenBy(x => x.MaBanSach, StringComparer.Ordinal).ToList();
    }

    private IQueryable<DongLuotGiaHan> QLuotGiaHan(DateOnly tu, DateOnly den) =>
        from cg in db.CtPhieuGiaHan.AsNoTracking()
        join pg in db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
        join pm in db.PhieuMuon on pg.SoPMuon equals pm.SoPMuon
        where pg.NgayLapPGiaHan >= tu && pg.NgayLapPGiaHan <= den
        orderby pg.NgayLapPGiaHan, pg.SoPGiaHan, cg.MaBanSach
        select new DongLuotGiaHan(pg.SoPGiaHan, pg.NgayLapPGiaHan, pg.SoPMuon, pm.HoTen, cg.MaBanSach, cg.TenSach, cg.KetQuaGiaHan, cg.HanTraMoi);

    private IQueryable<DongLuotTra> QLuotTra(DateOnly tu, DateOnly den) =>
        from x in db.CtPhieuTra.AsNoTracking()
        join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
        join pm in db.PhieuMuon on pt.SoPMuon equals pm.SoPMuon
        join cm in db.CtPhieuMuon on new { pt.SoPMuon, x.MaBanSach } equals new { cm.SoPMuon, cm.MaBanSach }
        where pt.NgayTra >= tu && pt.NgayTra <= den
        orderby pt.NgayTra, pt.SoPTra, x.MaBanSach
        select new DongLuotTra(pt.SoPTra, pt.NgayTra, pt.SoPMuon, pm.HoTen, x.MaBanSach, cm.TenSach, x.TinhTrangTra, x.SoNgayQuaHan);

    /// <summary>Lượt trả trong kỳ – chỉ CT_PHIEUTRA ⋈ PHIEUTRA (đúng công thức SPEC 6.4.5: COUNT(CT_PHIEUTRA) có ngaytra trong kỳ).</summary>
    private IQueryable<LuotTraGon> QLuotTraGon(DateOnly tu, DateOnly den) =>
        from x in db.CtPhieuTra.AsNoTracking()
        join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
        where pt.NgayTra >= tu && pt.NgayTra <= den
        select new LuotTraGon { SoPTra = pt.SoPTra, NgayTra = pt.NgayTra, SoPMuon = pt.SoPMuon, MaBanSach = x.MaBanSach,
            TinhTrangTra = x.TinhTrangTra, SoNgayQuaHan = x.SoNgayQuaHan };

    /// <summary>Một trang lượt trả: phân trang trên phần gọn rồi nối họ tên, tên sách cho đúng các dòng của trang (Pha 8 hiệu năng).</summary>
    private async Task<List<DongLuotTra>> TrangLuotTraAsync(DateOnly tu, DateOnly den, int boQua, int lay, CancellationToken ct)
    {
        var trang = QLuotTraGon(tu, den).OrderBy(x => x.NgayTra).ThenBy(x => x.SoPTra).ThenBy(x => x.MaBanSach).Skip(boQua).Take(lay);
        var ds = await (from x in trang
                        join pm in db.PhieuMuon on x.SoPMuon equals pm.SoPMuon
                        join cm in db.CtPhieuMuon on new { x.SoPMuon, x.MaBanSach } equals new { cm.SoPMuon, cm.MaBanSach }
                        select new DongLuotTra(x.SoPTra, x.NgayTra, x.SoPMuon, pm.HoTen, x.MaBanSach, cm.TenSach, x.TinhTrangTra, x.SoNgayQuaHan))
            .ToListAsync(ct);
        return ds.OrderBy(x => x.NgayTra).ThenBy(x => x.SoPTra, StringComparer.Ordinal).ThenBy(x => x.MaBanSach, StringComparer.Ordinal).ToList();
    }

    private async Task<List<DongSachChuaTra>> QuaHanAsync(DateOnly den, CancellationToken ct) =>
        (await SachChuaTraAsync(den, ct)).Where(x => x.SoNgayQuaHan > 0).OrderByDescending(x => x.SoNgayQuaHan).ThenBy(x => x.SoPMuon).ToList();

    /// <summary>
    /// btnTongHop (01.04.02) – XEM TRƯỚC: số liệu tổng hợp và biểu đồ tính trên TOÀN BỘ kỳ (đếm/gộp theo ngày bằng SQL),
    /// mỗi bảng chi tiết chỉ trả trang đầu (<paramref name="coTrang"/> dòng, mặc định 50) kèm tổng số dòng; trang sau lấy qua
    /// <see cref="ChiTietAsync"/>. Bản in lấy đủ dòng qua <see cref="DayDuAsync"/>.
    /// </summary>
    public async Task<BaoCaoMuonTraDto> TongHopAsync(DateOnly? tuNgay, DateOnly? denNgay, int? coTrang, CancellationToken ct)
    {
        var (tu, den) = KyBaoCao.KiemTra(tuNgay, denNgay, clock.HomNay);
        var n = Math.Clamp(coTrang ?? CoTrangMacDinh, 1, 200);

        // Bốn phần độc lập (mượn, gia hạn, trả, chưa trả) chạy SONG SONG, mỗi phần một DbContext riêng (DbContext không dùng chung
        // được giữa các luồng) – thời gian = phần chậm nhất thay vì tổng ~9 lượt hỏi CSDL (Pha 8 hiệu năng, GIA_DINH D82)
        var tMuon = RiengAsync(async s => (
            TheoNgay: await s.Db.CtPhieuMuon.AsNoTracking().Where(c => c.NgayMuon >= tu && c.NgayMuon <= den)
                .GroupBy(c => c.NgayMuon).Select(g => new { g.Key, So = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.So, ct),
            Trang: await s.TrangLuotMuonAsync(tu, den, 0, n, ct)));
        var tGiaHan = RiengAsync(async s => (
            TheoNgay: await (from cg in s.Db.CtPhieuGiaHan.AsNoTracking()
                             join pg in s.Db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
                             where pg.NgayLapPGiaHan >= tu && pg.NgayLapPGiaHan <= den && cg.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY
                             group cg by pg.NgayLapPGiaHan into g
                             select new { g.Key, So = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.So, ct),
            SoDong: await s.QLuotGiaHan(tu, den).CountAsync(ct),
            Trang: await s.QLuotGiaHan(tu, den).Take(n).ToListAsync(ct)));
        var tTra = RiengAsync(async s => (
            TheoNgay: await s.QLuotTraGon(tu, den).TagWith(GoiYTruyVanInterceptor.LoopJoin).GroupBy(x => x.NgayTra)
                .Select(g => new { g.Key, So = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.So, ct),
            Trang: await s.TrangLuotTraAsync(tu, den, 0, n, ct)));
        var tChuaTra = RiengAsync(s => s.SachChuaTraAsync(den, ct));
        await Task.WhenAll(tMuon, tGiaHan, tTra, tChuaTra);
        var (muonTheoNgay, trangMuon) = await tMuon;
        var (giaHanTheoNgay, soDongGiaHan, trangGiaHan) = await tGiaHan;
        var (traTheoNgay, trangTra) = await tTra;
        var chuaTra = await tChuaTra;
        var quaHan = chuaTra.Where(x => x.SoNgayQuaHan > 0).OrderByDescending(x => x.SoNgayQuaHan).ThenBy(x => x.SoPMuon).ToList();

        var tongMuon = muonTheoNgay.Values.Sum();
        var tongTra = traTheoNgay.Values.Sum();
        return new BaoCaoMuonTraDto(tu, den, tongMuon, giaHanTheoNgay.Values.Sum(), tongTra, chuaTra.Count, quaHan.Count,
            TheoNgay(tu, den, muonTheoNgay, giaHanTheoNgay, traTheoNgay),
            trangMuon, trangGiaHan, trangTra, quaHan.Take(n).ToList(),
            tongMuon, soDongGiaHan, tongTra, quaHan.Count, n);
    }

    /// <summary>Chạy một phần truy vấn trên DbContext riêng (cùng cấu hình) để chạy song song an toàn.</summary>
    private async Task<T> RiengAsync<T>(Func<BaoCaoMuonTraService, Task<T>> phan)
    {
        await using var c = new MuonTraDbContext(tuyChonCsdl);
        return await phan(new BaoCaoMuonTraService(c, clock, tuyChonCsdl));
    }

    /// <summary>Một trang của một bảng chi tiết (lưới dgvLuotMuon / dgvLuotGiaHan / dgvLuotTra / dgvSachQuaHan khi xem trước).</summary>
    public async Task<KetQuaTrang<object>> ChiTietAsync(string bang, DateOnly? tuNgay, DateOnly? denNgay, int? trang, int? coTrang, CancellationToken ct)
    {
        var (tu, den) = KyBaoCao.KiemTra(tuNgay, denNgay, clock.HomNay);
        if (!BangChiTiet.Contains(bang))
            new KiemTraDuLieu().Them("bang", "Bảng chi tiết không hợp lệ (luot-muon, luot-gia-han, luot-tra, sach-qua-han).").NemNeuCoLoi();
        var n = Math.Clamp(coTrang ?? CoTrangMacDinh, 1, 200);
        var t = Math.Max(1, trang ?? 1);
        async Task<KetQuaTrang<object>> TrangSql<T>(IQueryable<T> q) where T : class =>
            new(await q.CountAsync(ct), t, n, (await q.Skip((t - 1) * n).Take(n).ToListAsync(ct)).Cast<object>().ToList());
        switch (bang)
        {
            case "luot-muon":
                return new KetQuaTrang<object>(await db.CtPhieuMuon.CountAsync(c => c.NgayMuon >= tu && c.NgayMuon <= den, ct), t, n,
                    (await TrangLuotMuonAsync(tu, den, (t - 1) * n, n, ct)).Cast<object>().ToList());
            case "luot-gia-han": return await TrangSql(QLuotGiaHan(tu, den));
            case "luot-tra":
                return new KetQuaTrang<object>(await QLuotTraGon(tu, den).TagWith(GoiYTruyVanInterceptor.LoopJoin).CountAsync(ct), t, n,
                    (await TrangLuotTraAsync(tu, den, (t - 1) * n, n, ct)).Cast<object>().ToList());
            default:
                var quaHan = await QuaHanAsync(den, ct);
                return new KetQuaTrang<object>(quaHan.Count, t, n, quaHan.Skip((t - 1) * n).Take(n).Cast<object>().ToList());
        }
    }

    /// <summary>Đủ toàn bộ dòng của kỳ – dữ liệu trang in MB06 (01.04.03 – TT070, TT071) và khung xem bản in.</summary>
    public async Task<BaoCaoMuonTraDto> DayDuAsync(DateOnly? tuNgay, DateOnly? denNgay, CancellationToken ct)
    {
        var (tu, den) = KyBaoCao.KiemTra(tuNgay, denNgay, clock.HomNay);
        var luotMuon = await QLuotMuon(tu, den).ToListAsync(ct);
        var luotGiaHan = await QLuotGiaHan(tu, den).ToListAsync(ct);
        var luotTra = await QLuotTra(tu, den).ToListAsync(ct);
        var chuaTra = await SachChuaTraAsync(den, ct);
        var quaHan = chuaTra.Where(x => x.SoNgayQuaHan > 0).OrderByDescending(x => x.SoNgayQuaHan).ThenBy(x => x.SoPMuon).ToList();
        var dongY = luotGiaHan.Where(x => x.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY).ToList();

        var muonTheoNgay = luotMuon.GroupBy(x => x.NgayMuon).ToDictionary(g => g.Key, g => g.Count());
        var giaHanTheoNgay = dongY.GroupBy(x => x.NgayLapPGiaHan).ToDictionary(g => g.Key, g => g.Count());
        var traTheoNgay = luotTra.GroupBy(x => x.NgayTra).ToDictionary(g => g.Key, g => g.Count());
        return new BaoCaoMuonTraDto(tu, den, luotMuon.Count, dongY.Count, luotTra.Count, chuaTra.Count, quaHan.Count,
            TheoNgay(tu, den, muonTheoNgay, giaHanTheoNgay, traTheoNgay), luotMuon, luotGiaHan, luotTra, quaHan,
            luotMuon.Count, luotGiaHan.Count, luotTra.Count, quaHan.Count, 0);
    }

    private static List<DiemMuonTra> TheoNgay(DateOnly tu, DateOnly den, Dictionary<DateOnly, int> muon, Dictionary<DateOnly, int> giaHan,
        Dictionary<DateOnly, int> tra) =>
        Enumerable.Range(0, den.DayNumber - tu.DayNumber + 1).Select(i => tu.AddDays(i))
            .Select(d => new DiemMuonTra(d, muon.GetValueOrDefault(d), giaHan.GetValueOrDefault(d), tra.GetValueOrDefault(d))).ToList();

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
