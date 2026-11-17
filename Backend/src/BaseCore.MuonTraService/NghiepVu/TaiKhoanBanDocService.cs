using BaseCore.Common.Auth;
using BaseCore.Common.Loi;
using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TTTK = BaseCore.Entities.MienGiaTri.TinhTrangTaiKhoanBanDoc;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record KichHoatNhap(string? MaSvHvCb, string? SoTheBd, DateOnly? NgaySinh, string? MatKhau, string? NhapLai);
public sealed record DoiMatKhauBanDocNhap(string? MatKhauCu, string? MatKhauMoi, string? NhapLai);

public sealed record TheBanDocDto(string MaBanDoc, string HoTen, string DoiTuong, string MaSvHvCb, string LopDonVi, string SoTheBd,
    DateOnly HsdTuNgay, DateOnly HsdDenNgay, int SoNgayConHanThe, int SoBanToiDa);
public sealed record TongQuanBanDocDto(TheBanDocDto The, int DangMuon, int SapDenHan, int QuaHan, int DatChoDangGiu,
    int ThongBaoChuaDoc, IReadOnlyList<string> ViecCanLam, IReadOnlyList<DiemThangBanDoc> MuonTheoThang);
/// <summary>Số bản sách đã mượn trong tháng (biểu đồ cột nhỏ trang cá nhân – file 09 mục 3).</summary>
public sealed record DiemThangBanDoc(int Nam, int Thang, int SoBan);
public sealed record SachDangMuonCongDto(string SoPMuon, string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTra,
    int SoNgayConLai, int SoLanGiaHan, string? AnhBia, string? MaDauSach);
public sealed record LichSuDong(string Loai, string SoChungTu, DateOnly Ngay, string NoiDung, IReadOnlyList<string> Sach);
public sealed record ViPhamCongDto(string SoBienBan, string SoPMuon, DateTime ThoiGianLap, string HtThanhToan, decimal TongTien,
    IReadOnlyList<ViPhamDongDto> ChiTiet);
public sealed record ViPhamDongDto(string MaBanSach, string TenSach, string NoiDung, int SoLuong, decimal ThanhTien);
public sealed record YeuThichDto(string MaDauSach, string TenSach, string? TacGia, string? AnhBia, string? MauGay, DateTime TgThem, int SoBanCoTheMuon);
public sealed record TinhTrangTaiKhoanCongDto(string MaBanDoc, string TinhTrang, DateTime? TgDangNhapCuoi, DateTime? TgKhoaDen, DateTime TgTao);
public sealed record ThongBaoDto(long MaThongBao, string Loai, string NoiDung, string? LienKet, DateTime TgTao, bool DaDoc);

/// <summary>
/// Tài khoản và trang cá nhân bạn đọc (file 07): kích hoạt / đặt lại mật khẩu, đổi mật khẩu, tổng quan, sách đang mượn,
/// lịch sử, vi phạm, yêu thích, thông báo. Chỉ ĐỌC dữ liệu gốc – bạn đọc không tự gia hạn, trả hay thanh toán.
/// </summary>
public sealed class TaiKhoanBanDocService(MuonTraDbContext db, MatKhauHasher hasher, IClock clock, TraCuuMuonTra traCuu,
    ChungTuChung chung, IOptions<QuyDinhOptions> quyDinh, IOptions<DatChoOptions> datCho, ILogger<TaiKhoanBanDocService> logger)
{
    public const int SoLanSaiKichHoat = 5;
    public static readonly TimeSpan KhoaKichHoat = TimeSpan.FromMinutes(15);
    private const string ThongBaoSaiKichHoat = "Thông tin không khớp với thẻ bạn đọc. Vui lòng kiểm tra mã SV/HV/CB, số thẻ và ngày sinh.";

    // ───────────────────────────── Kích hoạt / quên mật khẩu (file 07 mục 1) ─────────────────────────────

    /// <summary>
    /// POST /api/cong/kich-hoat: ma_svhvcb + so_thebd + ngaysinh khớp BANDOC và thẻ còn hạn → đặt mật khẩu → “Hoạt động”.
    /// Dùng cả cho quên mật khẩu (đặt lại) với tài khoản đang “Hoạt động”. Sai 5 lần → tạm khóa 15 phút (tg_khoa_den).
    /// </summary>
    public async Task<string> KichHoatAsync(KichHoatNhap n, CancellationToken ct)
    {
        var maSv = (n.MaSvHvCb ?? string.Empty).Trim();
        var soThe = (n.SoTheBd ?? string.Empty).Trim();
        var kt = new KiemTraDuLieu();
        kt.BatBuoc("maSvHvCb", maSv, 15, "Mã SV/HV/CB");
        kt.BatBuoc("soTheBd", soThe, 15, "Số thẻ bạn đọc");
        if (n.NgaySinh is null) kt.Them("ngaySinh", "Vui lòng chọn ngày sinh.");
        if (!MatKhauHasher.DatChinhSach(n.MatKhau)) kt.Them("matKhau", "Mật khẩu phải có ít nhất 8 ký tự, gồm cả chữ và số.");
        else if (n.MatKhau != n.NhapLai) kt.Them("nhapLai", "Mật khẩu nhập lại không khớp.");
        kt.NemNeuCoLoi();

        var bayGio = clock.BayGio;
        var bd = await db.BanDoc.AsNoTracking().SingleOrDefaultAsync(x => x.MaSvHvCb == maSv, ct);
        var tk = bd is null ? null : await db.TaiKhoanBanDoc.SingleOrDefaultAsync(x => x.MaBanDoc == bd.MaBanDoc, ct);
        if (bd is null || tk is null)
        {
            hasher.KiemTra(n.MatKhau!, new string('A', MatKhauHasher.DoDaiChuoi)); // cân thời gian, không lộ mã tồn tại
            throw LoiNghiepVu.KhongHopLe("SAI_THONG_TIN_KICH_HOAT", ThongBaoSaiKichHoat);
        }
        if (tk.TinhTrang == TTTK.Khoa)
            throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAI_KHOAN_KHOA", "Tài khoản cổng bạn đọc đã bị khóa. Vui lòng liên hệ quầy mượn trả.");
        if (tk.TgKhoaDen is { } den && den > bayGio)
            throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAM_KHOA",
                $"Nhập sai quá {SoLanSaiKichHoat} lần. Vui lòng thử lại sau {den:HH:mm dd/MM/yyyy}.");

        if (bd.SoTheBd != soThe || bd.NgaySinh != n.NgaySinh)
        {
            tk.SoLanSai++;
            if (tk.SoLanSai >= SoLanSaiKichHoat)
            {
                tk.SoLanSai = 0;
                tk.TgKhoaDen = bayGio.Add(KhoaKichHoat);
                await db.SaveChangesAsync(ct);
                throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAM_KHOA",
                    $"Nhập sai {SoLanSaiKichHoat} lần. Chức năng kích hoạt tạm khóa đến {tk.TgKhoaDen:HH:mm dd/MM/yyyy}.");
            }
            await db.SaveChangesAsync(ct);
            throw LoiNghiepVu.KhongHopLe("SAI_THONG_TIN_KICH_HOAT", $"{ThongBaoSaiKichHoat} (còn {SoLanSaiKichHoat - tk.SoLanSai} lần thử).");
        }
        var homNay = clock.HomNay;
        if (!(bd.HsdTuNgay <= homNay && homNay <= bd.HsdDenNgay))
            throw LoiNghiepVu.XungDot("THE_HET_HAN", $"Thẻ bạn đọc không còn hiệu lực (hạn {bd.HsdTuNgay:dd/MM/yyyy} – {bd.HsdDenNgay:dd/MM/yyyy}). Vui lòng gia hạn thẻ tại quầy.");

        var laKichHoat = tk.TinhTrang == TTTK.ChuaKichHoat;
        tk.MatKhau = hasher.Bam(n.MatKhau!);
        tk.TinhTrang = TTTK.HoatDong;
        tk.SoLanSai = 0;
        tk.TgKhoaDen = null;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Bạn đọc {MaBanDoc} {Viec} tài khoản cổng", bd.MaBanDoc, laKichHoat ? "kích hoạt" : "đặt lại mật khẩu");
        return laKichHoat ? "Kích hoạt tài khoản thành công. Bạn có thể đăng nhập bằng mã SV/HV/CB và mật khẩu vừa đặt."
                          : "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.";
    }

    /// <summary>POST /api/cong/tai-khoan/doi-mat-khau.</summary>
    public async Task DoiMatKhauAsync(string maBanDoc, DoiMatKhauBanDocNhap n, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        if (string.IsNullOrEmpty(n.MatKhauCu)) kt.Them("matKhauCu", "Vui lòng nhập mật khẩu hiện tại.");
        if (!MatKhauHasher.DatChinhSach(n.MatKhauMoi)) kt.Them("matKhauMoi", "Mật khẩu mới phải có ít nhất 8 ký tự, gồm cả chữ và số.");
        else if (n.MatKhauMoi != n.NhapLai) kt.Them("nhapLai", "Mật khẩu nhập lại không khớp.");
        else if (n.MatKhauMoi == n.MatKhauCu) kt.Them("matKhauMoi", "Mật khẩu mới phải khác mật khẩu hiện tại.");
        kt.NemNeuCoLoi();
        var tk = await db.TaiKhoanBanDoc.SingleAsync(x => x.MaBanDoc == maBanDoc, ct);
        if (!hasher.KiemTra(n.MatKhauCu!, tk.MatKhau))
            throw LoiNghiepVu.KhongHopLe("SAI_MAT_KHAU", "Mật khẩu hiện tại không đúng.", new Dictionary<string, string> { ["matKhauCu"] = "Mật khẩu hiện tại không đúng." });
        tk.MatKhau = hasher.Bam(n.MatKhauMoi!);
        await db.SaveChangesAsync(ct);
    }

    public async Task<TinhTrangTaiKhoanCongDto> TinhTrangAsync(string maBanDoc, CancellationToken ct) =>
        await db.TaiKhoanBanDoc.AsNoTracking().Where(x => x.MaBanDoc == maBanDoc)
            .Select(x => new TinhTrangTaiKhoanCongDto(x.MaBanDoc, x.TinhTrang, x.TgDangNhapCuoi, x.TgKhoaDen, x.TgTao)).SingleOrDefaultAsync(ct)
        ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_TAI_KHOAN", $"Bạn đọc {maBanDoc} chưa có tài khoản cổng.");

    /// <summary>N4 khóa/mở khóa tài khoản cổng ở Danh mục Bạn đọc (file 07 mục 1 – mở rộng).</summary>
    public async Task<string> DoiTinhTrangAsync(string maBanDoc, bool khoa, string maNd, CancellationToken ct)
    {
        var tk = await db.TaiKhoanBanDoc.SingleOrDefaultAsync(x => x.MaBanDoc == maBanDoc, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_TAI_KHOAN", $"Bạn đọc {maBanDoc} chưa có tài khoản cổng.");
        var moi = khoa ? TTTK.Khoa : tk.MatKhau is null ? TTTK.ChuaKichHoat : TTTK.HoatDong;
        if (tk.TinhTrang == moi) return moi;
        await db.ChayAsync(async c =>
        {
            var t = await db.TaiKhoanBanDoc.SingleAsync(x => x.MaBanDoc == maBanDoc, c);
            t.TinhTrang = moi;
            t.SoLanSai = 0;
            t.TgKhoaDen = null;
            await NhatKyGhi.ThemAsync(db, maNd, $"Danh mục Bạn đọc – {(khoa ? "khóa" : "mở khóa")} tài khoản cổng bạn đọc {maBanDoc}", clock.BayGio, c);
            await db.SaveChangesAsync(c);
        }, ct);
        return moi;
    }

    // ───────────────────────────── Trang cá nhân (file 07 mục 3) ─────────────────────────────

    public async Task<TongQuanBanDocDto> TongQuanAsync(string maBanDoc, CancellationToken ct)
    {
        var bd = await chung.BanDocAsync(maBanDoc, ct);
        var homNay = clock.HomNay;
        var bayGio = clock.BayGio;
        var dangMuon = await traCuu.SachDangMuonAsync(maBanDoc, homNay, ct);
        var sapDenHan = dangMuon.Count(x => x.SoNgayQuaHan == 0 && x.HanTra.DayNumber - homNay.DayNumber is >= 0 and <= 3);
        var quaHan = dangMuon.Count(x => x.SoNgayQuaHan > 0);
        var giu = await chung.DatChoConGiu(bayGio).Where(d => d.MaBanDoc == maBanDoc).Select(d => d.TgHetHan).ToListAsync(ct);
        var chuaDoc = await db.ThongBao.CountAsync(x => x.MaBanDoc == maBanDoc && !x.DaDoc, ct);
        var viPham = await db.BienBan.CountAsync(b => b.MaBanDoc == maBanDoc && b.HtThanhToan == MienGiaTri.HinhThucThanhToan.ChuaThanhToan, ct);

        var viec = new List<string>();
        if (quaHan > 0) viec.Add($"Trả {quaHan} sách quá hạn tại quầy mượn trả.");
        if (viPham > 0) viec.Add($"Thanh toán {viPham} biên bản vi phạm tại quầy mượn trả.");
        if (giu.Count > 0) viec.Add($"Nhận {giu.Count} sách đặt chỗ trước {giu.Min():HH:mm dd/MM}.");
        if (sapDenHan > 0) viec.Add($"{sapDenHan} sách sắp đến hạn trả trong 3 ngày tới.");
        var conHan = bd.HsdDenNgay.DayNumber - homNay.DayNumber;
        if (conHan is >= 0 and <= 30) viec.Add($"Thẻ bạn đọc hết hạn sau {conHan} ngày – liên hệ quầy để gia hạn.");
        if (conHan < 0) viec.Add("Thẻ bạn đọc đã hết hạn – liên hệ quầy để gia hạn.");

        // 12 tháng gần nhất (kể cả tháng này)
        var dauKy = new DateOnly(homNay.Year, homNay.Month, 1).AddMonths(-11);
        var theoThang = await (from cm in db.CtPhieuMuon
                               join pm in db.PhieuMuon on cm.SoPMuon equals pm.SoPMuon
                               where pm.MaBanDoc == maBanDoc && cm.NgayMuon >= dauKy
                               group cm by new { cm.NgayMuon.Year, cm.NgayMuon.Month } into g
                               select new { g.Key.Year, g.Key.Month, So = g.Count() }).ToListAsync(ct);
        var muonTheoThang = Enumerable.Range(0, 12).Select(i => dauKy.AddMonths(i))
            .Select(t => new DiemThangBanDoc(t.Year, t.Month, theoThang.FirstOrDefault(x => x.Year == t.Year && x.Month == t.Month)?.So ?? 0)).ToList();

        return new TongQuanBanDocDto(TheDto(bd, homNay), dangMuon.Count, sapDenHan, quaHan, giu.Count, chuaDoc, viec, muonTheoThang);
    }

    private TheBanDocDto TheDto(BanDoc bd, DateOnly homNay) =>
        new(bd.MaBanDoc, bd.HoTen, bd.DoiTuong, bd.MaSvHvCb, bd.LopDonVi, bd.SoTheBd, bd.HsdTuNgay, bd.HsdDenNgay,
            bd.HsdDenNgay.DayNumber - homNay.DayNumber, quyDinh.Value.Cua(bd.DoiTuong).SoBanToiDa);

    /// <summary>Sách đang mượn: CT_PHIEUMUON chưa có trong CT_PHIEUTRA, hạn trả đã tính gia hạn (CT_PHIEUGIAHAN).</summary>
    public async Task<List<SachDangMuonCongDto>> DangMuonAsync(string maBanDoc, CancellationToken ct)
    {
        var homNay = clock.HomNay;
        var ds = await traCuu.SachDangMuonAsync(maBanDoc, homNay, ct);
        var ma = ds.Select(x => x.MaBanSach).ToList();
        var bia = await (from b in db.DauSachBanSach join d in db.DauSach on b.MaDauSach equals d.MaDauSach
                         where ma.Contains(b.MaBanSach) select new { b.MaBanSach, d.MaDauSach, d.AnhBia }).ToListAsync(ct);
        var soPm = ds.Select(x => x.SoPMuon).Distinct().ToList();
        var giaHan = await (from cg in db.CtPhieuGiaHan join pg in db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
                            where soPm.Contains(pg.SoPMuon) && cg.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY
                            select new { pg.SoPMuon, cg.MaBanSach }).ToListAsync(ct);
        return ds.Select(x =>
        {
            var b = bia.FirstOrDefault(k => k.MaBanSach == x.MaBanSach);
            return new SachDangMuonCongDto(x.SoPMuon, x.MaBanSach, x.TenSach, x.NgayMuon, x.HanTra, x.HanTra.DayNumber - homNay.DayNumber,
                giaHan.Count(g => g.SoPMuon == x.SoPMuon && g.MaBanSach == x.MaBanSach),
                BanCoTheMuonAnh(b?.AnhBia), b?.MaDauSach);
        }).ToList();
    }

    /// <summary>Lịch sử phiếu mượn, gia hạn, trả của tôi (lọc theo năm lập phiếu).</summary>
    public async Task<object> LichSuAsync(string maBanDoc, int? nam, CancellationToken ct)
    {
        var namChon = nam ?? clock.HomNay.Year;
        var tu = new DateOnly(namChon, 1, 1);
        var den = new DateOnly(namChon, 12, 31);
        var muon = await (from pm in db.PhieuMuon where pm.MaBanDoc == maBanDoc && pm.NgayLapPMuon >= tu && pm.NgayLapPMuon <= den
                          select new { pm.SoPMuon, pm.NgayLapPMuon, Sach = db.CtPhieuMuon.Where(c => c.SoPMuon == pm.SoPMuon).Select(c => c.TenSach).ToList() })
            .ToListAsync(ct);
        var giaHan = await (from pg in db.PhieuGiaHan where pg.MaBanDoc == maBanDoc && pg.NgayLapPGiaHan >= tu && pg.NgayLapPGiaHan <= den
                            select new
                            {
                                pg.SoPGiaHan, pg.SoPMuon, pg.NgayLapPGiaHan,
                                Sach = db.CtPhieuGiaHan.Where(c => c.SoPGiaHan == pg.SoPGiaHan)
                                    .Select(c => c.TenSach + " – " + c.KetQuaGiaHan + (c.HanTraMoi != null ? " (hạn mới " + c.HanTraMoi.ToString() + ")" : "")).ToList()
                            }).ToListAsync(ct);
        var tra = await (from pt in db.PhieuTra where pt.MaBanDoc == maBanDoc && pt.NgayTra >= tu && pt.NgayTra <= den
                         select new
                         {
                             pt.SoPTra, pt.SoPMuon, pt.NgayTra,
                             Sach = (from c in db.CtPhieuTra join s in db.Sach on c.MaBanSach equals s.MaBanSach
                                     where c.SoPTra == pt.SoPTra select s.TenSach + " – " + c.TinhTrangTra).ToList()
                         }).ToListAsync(ct);
        var dong = muon.Select(x => new LichSuDong("Phiếu mượn sách", x.SoPMuon, x.NgayLapPMuon, $"Mượn {x.Sach.Count} bản sách", x.Sach))
            .Concat(giaHan.Select(x => new LichSuDong("Phiếu gia hạn sách", x.SoPGiaHan, x.NgayLapPGiaHan, $"Gia hạn theo phiếu mượn {x.SoPMuon}", x.Sach)))
            .Concat(tra.Select(x => new LichSuDong("Phiếu trả sách", x.SoPTra, x.NgayTra, $"Trả {x.Sach.Count} bản sách của phiếu mượn {x.SoPMuon}", x.Sach)))
            .OrderByDescending(x => x.Ngay).ThenByDescending(x => x.SoChungTu).ToList();
        var cacNam = await db.PhieuMuon.Where(x => x.MaBanDoc == maBanDoc).Select(x => x.NgayLapPMuon.Year).Distinct().ToListAsync(ct);
        if (!cacNam.Contains(namChon)) cacNam.Add(namChon);
        return new { nam = namChon, cacNam = cacNam.OrderDescending(), dong };
    }

    /// <summary>Vi phạm và khoản phải nộp: BIENBAN + CT_BIENBAN của tôi.</summary>
    public async Task<List<ViPhamCongDto>> ViPhamAsync(string maBanDoc, CancellationToken ct)
    {
        var bb = await db.BienBan.AsNoTracking().Where(b => b.MaBanDoc == maBanDoc).OrderByDescending(b => b.ThoiGianLap).ToListAsync(ct);
        var so = bb.Select(b => b.SoBienBan).ToList();
        var ct2 = await (from c in db.CtBienBan join kt in db.KhoanThu on c.MaKhoanThu equals kt.MaKhoanThu
                         join s in db.Sach on c.MaBanSach equals s.MaBanSach
                         where so.Contains(c.SoBienBan)
                         select new { c.SoBienBan, c.MaBanSach, s.TenSach, NoiDung = kt.NoiDungTt, c.SoLuong, c.ThanhTien }).ToListAsync(ct);
        return bb.Select(b => new ViPhamCongDto(b.SoBienBan, b.SoPMuon, b.ThoiGianLap, b.HtThanhToan, b.TongTien,
            ct2.Where(c => c.SoBienBan == b.SoBienBan).Select(c => new ViPhamDongDto(c.MaBanSach, c.TenSach, c.NoiDung, c.SoLuong, c.ThanhTien)).ToList())).ToList();
    }

    // ───────────────────────────── Yêu thích ─────────────────────────────

    public async Task<List<YeuThichDto>> YeuThichAsync(string maBanDoc, CancellationToken ct)
    {
        var bayGio = clock.BayGio;
        var coThe = BanCoTheMuonQuery(bayGio);
        var ds = await (from y in db.YeuThich
                        join d in db.DauSach on y.MaDauSach equals d.MaDauSach
                        where y.MaBanDoc == maBanDoc
                        orderby y.TgThem descending
                        select new
                        {
                            d.MaDauSach, d.TenSach, d.TacGia, d.AnhBia, d.MauGay, y.TgThem,
                            SoBan = db.DauSachBanSach.Count(b => b.MaDauSach == d.MaDauSach && coThe.Contains(b.MaBanSach))
                        }).ToListAsync(ct);
        return ds.Select(x => new YeuThichDto(x.MaDauSach, x.TenSach, x.TacGia, BanCoTheMuonAnh(x.AnhBia), x.MauGay, x.TgThem, x.SoBan)).ToList();
    }

    public async Task<bool> DatYeuThichAsync(string maBanDoc, string maDauSach, bool them, CancellationToken ct)
    {
        maDauSach = (maDauSach ?? string.Empty).Trim().ToUpperInvariant();
        if (!await db.DauSach.AnyAsync(x => x.MaDauSach == maDauSach, ct))
            throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_DAU_SACH", "Không tìm thấy đầu sách.");
        var co = await db.YeuThich.SingleOrDefaultAsync(x => x.MaBanDoc == maBanDoc && x.MaDauSach == maDauSach, ct);
        if (them && co is null) db.YeuThich.Add(new YeuThich { MaBanDoc = maBanDoc, MaDauSach = maDauSach, TgThem = clock.BayGio });
        else if (!them && co is not null) db.YeuThich.Remove(co);
        else return them;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) when (them) { /* bấm hai lần cùng lúc – đã có */ }
        return them;
    }

    // ───────────────────────────── Thông báo (file 07 mục 4) ─────────────────────────────

    public async Task<object> ThongBaoAsync(string maBanDoc, int soLuong, CancellationToken ct)
    {
        soLuong = Math.Clamp(soLuong, 1, 200);
        var ds = await db.ThongBao.AsNoTracking().Where(x => x.MaBanDoc == maBanDoc)
            .OrderByDescending(x => x.TgTao).ThenByDescending(x => x.MaThongBao).Take(soLuong)
            .Select(x => new ThongBaoDto(x.MaThongBao, x.Loai, x.NoiDung, x.LienKet, x.TgTao, x.DaDoc)).ToListAsync(ct);
        var chuaDoc = await db.ThongBao.CountAsync(x => x.MaBanDoc == maBanDoc && !x.DaDoc, ct);
        return new { chuaDoc, ds };
    }

    /// <summary>Đánh dấu đã đọc một thông báo (hoặc tất cả khi <paramref name="ma"/> = null).</summary>
    public async Task<int> DanhDauDaDocAsync(string maBanDoc, long? ma, CancellationToken ct) =>
        await db.ThongBao.Where(x => x.MaBanDoc == maBanDoc && !x.DaDoc && (ma == null || x.MaThongBao == ma))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.DaDoc, true), ct);

    // ───────────────────────────── Dùng chung ─────────────────────────────

    /// <summary>Bản sách có thể mượn (cùng quy tắc với khosach.BanCoTheMuon và 6.2.1).</summary>
    private IQueryable<string> BanCoTheMuonQuery(DateTime bayGio)
    {
        var dangMuon = chung.BanDangMuon();
        var choKho = chung.BanChoKhoXuLy();
        var dangGiu = chung.DatChoConGiu(bayGio).Select(d => d.MaBanSach);
        return db.Sach.Where(s => s.TinhTrang == MienGiaTri.TinhTrangSach.SanSang && !dangMuon.Contains(s.MaBanSach)
                                  && !choKho.Contains(s.MaBanSach) && !dangGiu.Contains(s.MaBanSach)).Select(s => s.MaBanSach);
    }

    /// <summary>Ảnh bìa bản nhỏ (“_nho.webp”) cho lưới thẻ.</summary>
    private static string? BanCoTheMuonAnh(string? anhBia) => anhBia is { Length: > 5 } ? anhBia[..^5] + "_nho.webp" : null;

    public int SoGioGiu => datCho.Value.SoGioGiu;
}
