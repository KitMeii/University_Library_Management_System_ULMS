using BaseCore.Common.Loi;
using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.ViPham;
using BaseCore.Repository;
using BaseCore.ViPhamService.DuLieu;
using Microsoft.EntityFrameworkCore;
using HT = BaseCore.Entities.MienGiaTri.HinhThucThanhToan;
using TR = BaseCore.Entities.MienGiaTri.TinhTrangTra;

namespace BaseCore.ViPhamService.NghiepVu;

public sealed record BanDocBienBan(string MaBanDoc, string HoTen, string LopDonVi, string MaSvHvCb);
public sealed record SachViPhamDto(string MaBanSach, string TenSach, string TinhTrangTra, int SoNgayQuaHan, string NoiDung);
public sealed record TruongHopViPhamDto(string SoPMuon, string MaBanDoc, string HoTen, string SoPTra, DateOnly NgayTra, int SoSach, string NoiDung);
public sealed record DongBienBanDto(string MaKhoanThu, string NoiDungTt, decimal DonGia, string MaBanSach, string TenSach, int SoLuong,
    decimal ThanhTien, bool DaLuu);
public sealed record BienBanDto(string? SoBienBan, DateTime? ThoiGianLap, string SoPMuon, BanDocBienBan BanDoc, string HtThanhToan,
    string? TenNd, IReadOnlyList<SachViPhamDto> SachViPham, IReadOnlyList<DongBienBanDto> ChiTiet)
{
    /// <summary>tong_tien = SUM(thanh_tien) của biên bản (SPEC 6.2.4, MB05).</summary>
    public decimal TongTien => ChiTiet.Sum(x => x.ThanhTien);
}
public sealed record DongBienBanNhap(string? MaKhoanThu, string? MaBanSach, int? SoLuong);
public sealed record BienBanNhap(string? SoPMuon, string? SoBienBan, string? HtThanhToan, List<DongBienBanNhap>? ChiTiet);
public sealed record KetQuaLuuBienBan(string So, string ThongBao, bool LaPhieuMoi);

/// <summary>SPEC 6.2.4 Lập Biên bản vi phạm mượn trả sách (02.03.01) – MB05.</summary>
public sealed class BienBanService(ViPhamDbContext db, IClock clock)
{
    /// <summary>Lớp có thuộc tính init (không dùng record vị trí) để EF dịch được truy cập thành viên trong truy vấn ghép.</summary>
    private static readonly System.Globalization.CultureInfo Vn = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");

    private sealed class DongViPham
    {
        public string SoPMuon { get; init; } = null!;
        public string SoPTra { get; init; } = null!;
        public DateOnly NgayTra { get; init; }
        public string MaBanSach { get; init; } = null!;
        public string TinhTrangTra { get; init; } = null!;
        public int SoNgayQuaHan { get; init; }
    }

    private static string MoTa(string tinhTrangTra, int soNgayQuaHan) =>
        string.Join(", ", new[]
        {
            soNgayQuaHan > 0 ? $"Quá hạn {soNgayQuaHan} ngày" : null,
            tinhTrangTra is TR.Mat ? "Mất sách" : tinhTrangTra is TR.HuHong ? "Hư hỏng" : null
        }.Where(x => x is not null));

    /// <summary>
    /// Trường hợp vi phạm Bộ phận mượn trả chuyển sang (TT053 → TT075, GIA_DINH D15): dòng CT_PHIEUTRA quá hạn, mất, hư hỏng
    /// chưa có trong CT_BIENBAN của cùng phiếu mượn.
    /// </summary>
    private IQueryable<DongViPham> ChuaLapBienBan() =>
        from ct in db.CtPhieuTra
        join pt in db.PhieuTra on ct.SoPTra equals pt.SoPTra
        where (ct.SoNgayQuaHan > 0 || ct.TinhTrangTra == TR.Mat || ct.TinhTrangTra == TR.HuHong)
              && !(from cb in db.CtBienBan join bb in db.BienBan on cb.SoBienBan equals bb.SoBienBan
                   where bb.SoPMuon == pt.SoPMuon && cb.MaBanSach == ct.MaBanSach select 1).Any()
        select new DongViPham { SoPMuon = pt.SoPMuon, SoPTra = pt.SoPTra, NgayTra = pt.NgayTra, MaBanSach = ct.MaBanSach, TinhTrangTra = ct.TinhTrangTra, SoNgayQuaHan = ct.SoNgayQuaHan };

    /// <summary>dgvTruongHop (TT075 Tiếp nhận trường hợp vi phạm): danh sách chờ lập biên bản, theo phiếu mượn.</summary>
    public async Task<IReadOnlyList<TruongHopViPhamDto>> TruongHopAsync(CancellationToken ct)
    {
        var dong = await (from v in ChuaLapBienBan()
                          join pm in db.PhieuMuon on v.SoPMuon equals pm.SoPMuon
                          select new { v.SoPMuon, pm.MaBanDoc, pm.HoTen, v.SoPTra, v.NgayTra, v.TinhTrangTra, v.SoNgayQuaHan }).ToListAsync(ct);
        return dong.GroupBy(x => x.SoPMuon)
            .Select(g => new TruongHopViPhamDto(g.Key, g.First().MaBanDoc, g.First().HoTen, g.First().SoPTra, g.Max(x => x.NgayTra), g.Count(),
                string.Join("; ", g.Select(x => MoTa(x.TinhTrangTra, x.SoNgayQuaHan)).Distinct())))
            .OrderBy(x => x.NgayTra).ThenBy(x => x.SoPMuon).ToList();
    }

    /// <summary>cboKhoanThu (TT076, TT081).</summary>
    public Task<List<KhoanThuDto>> KhoanThuAsync(CancellationToken ct) =>
        db.KhoanThu.AsNoTracking().OrderBy(x => x.MaKhoanThu).Select(x => new KhoanThuDto(x.MaKhoanThu, x.NoiDungTt, x.DonGia)).ToListAsync(ct);

    /// <summary>txtSoPhieuMuon, btnTimPhieuMuon (TT075): Bạn đọc + các bản sách vi phạm chưa lập biên bản.</summary>
    public async Task<BienBanDto> TimPhieuMuonAsync(string? soPMuon, CancellationToken ct)
    {
        var (pm, banDoc) = await PhieuMuonAsync(soPMuon, ct);
        var sach = await SachViPhamAsync(pm.SoPMuon, null, ct);
        if (sach.Count == 0)
            throw LoiNghiepVu.XungDot("KHONG_CO_VI_PHAM", $"Phiếu mượn sách {pm.SoPMuon} không có trường hợp vi phạm chờ lập biên bản.");
        return new BienBanDto(null, null, pm.SoPMuon, banDoc, HT.ChuaThanhToan, null, sach, []);
    }

    /// <summary>btnTimBienBan (TT080): nạp Biên bản vi phạm đã lưu ở chế độ sửa; dữ liệu in MB05.</summary>
    public async Task<BienBanDto> ChiTietAsync(string? soBienBan, CancellationToken ct)
    {
        var so = (soBienBan ?? string.Empty).Trim().ToUpperInvariant();
        var bb = await db.BienBan.AsNoTracking().SingleOrDefaultAsync(x => x.SoBienBan == so, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BIEN_BAN", $"Không tìm thấy Biên bản vi phạm {so}.");
        var (_, banDoc) = await PhieuMuonAsync(bb.SoPMuon, ct);
        var dong = await (from cb in db.CtBienBan.AsNoTracking()
                          join kt in db.KhoanThu on cb.MaKhoanThu equals kt.MaKhoanThu
                          join cm in db.CtPhieuMuon on new { cb.MaBanSach, bb.SoPMuon } equals new { cm.MaBanSach, cm.SoPMuon }
                          where cb.SoBienBan == so
                          orderby cb.MaBanSach, cb.MaKhoanThu
                          select new { cb, kt.NoiDungTt, kt.DonGia, cm.TenSach }).ToListAsync(ct);
        return new BienBanDto(bb.SoBienBan, bb.ThoiGianLap, bb.SoPMuon, banDoc, bb.HtThanhToan, bb.TenNd,
            await SachViPhamAsync(bb.SoPMuon, bb.SoBienBan, ct),
            dong.Select(x => new DongBienBanDto(x.cb.MaKhoanThu, x.NoiDungTt, x.DonGia, x.cb.MaBanSach, x.TenSach, x.cb.SoLuong, x.cb.ThanhTien, true)).ToList());
    }

    /// <summary>
    /// btnIn: TT077 kiểm soát dữ liệu nhập, TT078 thành tiền, TT079 tổng tiền, TT082 hình thức thanh toán; biên bản mới → TT083
    /// sinh so_bienban, thoigian_lap, TT084 lưu; đã lưu → TT086 cập nhật, TT085 ghi nhận thanh toán. Kết quả xử lý (TT087) và sách
    /// mất/hư hỏng (TT088) tự đến Bộ phận mượn trả / kho sách (GIA_DINH D15). Ghi nhật ký.
    /// </summary>
    public async Task<KetQuaLuuBienBan> LuuAsync(BienBanNhap n, string maNd, string tenNd, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        kt.ThuocMien("htThanhToan", n.HtThanhToan, HT.TatCa, "Hình thức thanh toán");
        var nhap = (n.ChiTiet ?? []).Select(d => d with { MaKhoanThu = d.MaKhoanThu?.Trim().ToUpperInvariant(), MaBanSach = d.MaBanSach?.Trim().ToUpperInvariant() }).ToList();
        kt.Dung(nhap.Count > 0, "chiTiet", "Biên bản vi phạm phải có ít nhất một khoản thu.");
        kt.Dung(nhap.Select(d => (d.MaKhoanThu, d.MaBanSach)).Distinct().Count() == nhap.Count, "chiTiet", "Khoản thu của một bản sách bị trùng.");
        foreach (var d in nhap)
        {
            if (string.IsNullOrEmpty(d.MaKhoanThu)) kt.Them("chiTiet", "Vui lòng chọn khoản thu.");
            if (string.IsNullOrEmpty(d.MaBanSach)) kt.Them("chiTiet", "Vui lòng chọn bản sách vi phạm.");
            if (d.SoLuong is null or <= 0 or > 99999) kt.Them("chiTiet", $"Số lượng của bản sách {d.MaBanSach} phải là số nguyên lớn hơn 0 (tối đa 5 chữ số).");
        }
        kt.NemNeuCoLoi();

        return await db.ChayAsync(async c =>
        {
            BienBan? bb = null;
            if (!string.IsNullOrWhiteSpace(n.SoBienBan))
            {
                var so = n.SoBienBan.Trim().ToUpperInvariant();
                bb = await db.BienBan.SingleOrDefaultAsync(x => x.SoBienBan == so, c)
                     ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BIEN_BAN", $"Không tìm thấy Biên bản vi phạm {so}.");
            }
            var (pm, banDoc) = await PhieuMuonAsync(bb?.SoPMuon ?? n.SoPMuon, c);
            var hopLe = (await SachViPhamAsync(pm.SoPMuon, bb?.SoBienBan, c)).Select(x => x.MaBanSach).ToHashSet();
            var khoanThu = await db.KhoanThu.AsNoTracking().ToDictionaryAsync(x => x.MaKhoanThu, c);
            var loi = new KiemTraDuLieu();
            foreach (var d in nhap)
            {
                if (!khoanThu.ContainsKey(d.MaKhoanThu!)) loi.Them("chiTiet", $"Khoản thu {d.MaKhoanThu} không có trong danh mục khoản thu.");
                if (!hopLe.Contains(d.MaBanSach!)) loi.Them("chiTiet", $"Bản sách {d.MaBanSach} không thuộc trường hợp vi phạm của Phiếu mượn sách {pm.SoPMuon}.");
            }
            loi.NemNeuCoLoi();

            var moi = bb is null;
            var thanhToanCu = bb?.HtThanhToan;
            if (bb is null)
            {
                var bayGio = clock.BayGio;
                var ngay = DateOnly.FromDateTime(bayGio);
                var so = SoChungTu.Tao(SoChungTu.BienBan, ngay, await SoChungTuGhi.CapAsync(db, SoChungTu.BienBan, ngay, c));
                bb = new BienBan { SoBienBan = so, SoPMuon = pm.SoPMuon, MaBanDoc = pm.MaBanDoc, ThoiGianLap = bayGio, MaNd = maNd, TenNd = tenNd };
                db.BienBan.Add(bb);
            }
            bb.HtThanhToan = n.HtThanhToan!;

            var daCo = moi ? [] : await db.CtBienBan.Where(x => x.SoBienBan == bb.SoBienBan).ToListAsync(c);
            var bo = daCo.Where(x => nhap.All(d => d.MaKhoanThu != x.MaKhoanThu || d.MaBanSach != x.MaBanSach)).ToList();
            if (bo.Count > 0)
                throw LoiNghiepVu.KhongHopLe("KHONG_BO_DONG_DA_LUU", "Không được bỏ dòng đã lưu khỏi biên bản: " + string.Join(", ", bo.Select(x => $"{x.MaBanSach}/{x.MaKhoanThu}")));
            foreach (var d in nhap)
            {
                var x = daCo.FirstOrDefault(k => k.MaKhoanThu == d.MaKhoanThu && k.MaBanSach == d.MaBanSach);
                if (x is null)
                {
                    x = new CtBienBan { MaKhoanThu = d.MaKhoanThu!, SoBienBan = bb.SoBienBan, MaBanSach = d.MaBanSach! };
                    db.CtBienBan.Add(x);
                }
                x.SoLuong = d.SoLuong!.Value;
                x.ThanhTien = d.SoLuong.Value * khoanThu[d.MaKhoanThu!].DonGia;
            }
            bb.TongTien = nhap.Sum(d => d.SoLuong!.Value * khoanThu[d.MaKhoanThu!].DonGia);

            await NhatKyGhi.ThemAsync(db, maNd,
                $"02.03.01 {(moi ? "Lập" : "Cập nhật")} Biên bản vi phạm {bb.SoBienBan} (Phiếu mượn {pm.SoPMuon}, Bạn đọc {banDoc.MaBanDoc}): {nhap.Count} khoản, tổng {bb.TongTien.ToString("N0", Vn)} đ, {bb.HtThanhToan}",
                clock.BayGio, c);
            if (!moi && thanhToanCu == HT.ChuaThanhToan && bb.HtThanhToan != HT.ChuaThanhToan)
                await NhatKyGhi.ThemAsync(db, maNd, $"02.03.01 Ghi nhận thanh toán Biên bản vi phạm {bb.SoBienBan}: {bb.HtThanhToan} {bb.TongTien.ToString("N0", Vn)} đ", clock.BayGio, c);
            await db.SaveChangesAsync(c);

            var tb = $"Đã {(moi ? "lưu" : "cập nhật")} Biên bản vi phạm {bb.SoBienBan}, tổng tiền {bb.TongTien.ToString("N0", Vn)} đ ({bb.HtThanhToan.ToLowerInvariant()}).";
            tb += " Kết quả xử lý đã chuyển cho Bộ phận mượn trả sách";
            var matHong = await (from ct2 in db.CtPhieuTra join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                                 where pt.SoPMuon == pm.SoPMuon && (ct2.TinhTrangTra == TR.Mat || ct2.TinhTrangTra == TR.HuHong)
                                 select ct2.MaBanSach).ToListAsync(c);
            tb += nhap.Any(d => matHong.Contains(d.MaBanSach!)) ? "; sách bị mất, hư hỏng đã chuyển cho Bộ phận kho sách." : ".";
            return new KetQuaLuuBienBan(bb.SoBienBan, tb, moi);
        }, ct);
    }

    private async Task<(Entities.MuonTra.PhieuMuon Pm, BanDocBienBan BanDoc)> PhieuMuonAsync(string? soPMuon, CancellationToken ct)
    {
        var so = (soPMuon ?? string.Empty).Trim().ToUpperInvariant();
        if (so.Length == 0) new KiemTraDuLieu().Them("soPMuon", "Vui lòng nhập số Phiếu mượn sách.").NemNeuCoLoi();
        var pm = await db.PhieuMuon.AsNoTracking().SingleOrDefaultAsync(x => x.SoPMuon == so, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_PHIEU_MUON", $"Không tìm thấy Phiếu mượn sách {so}.");
        var bd = await db.BanDoc.AsNoTracking().Where(x => x.MaBanDoc == pm.MaBanDoc)
            .Select(x => new BanDocBienBan(x.MaBanDoc, x.HoTen, x.LopDonVi, x.MaSvHvCb)).SingleAsync(ct);
        return (pm, bd);
    }

    /// <summary>Bản sách vi phạm của phiếu mượn: chưa lập biên bản, cộng các bản đã có trong chính biên bản đang sửa.</summary>
    private async Task<List<SachViPhamDto>> SachViPhamAsync(string soPMuon, string? soBienBan, CancellationToken ct)
    {
        var trongBienBan = soBienBan is null ? [] :
            await db.CtBienBan.Where(x => x.SoBienBan == soBienBan).Select(x => x.MaBanSach).Distinct().ToListAsync(ct);
        var dong = await (from ct2 in db.CtPhieuTra.AsNoTracking()
                          join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                          join cm in db.CtPhieuMuon on new { ct2.MaBanSach, pt.SoPMuon } equals new { cm.MaBanSach, cm.SoPMuon }
                          where pt.SoPMuon == soPMuon
                                && (trongBienBan.Contains(ct2.MaBanSach)
                                    || ((ct2.SoNgayQuaHan > 0 || ct2.TinhTrangTra == TR.Mat || ct2.TinhTrangTra == TR.HuHong)
                                        && !(from cb in db.CtBienBan join bb in db.BienBan on cb.SoBienBan equals bb.SoBienBan
                                             where bb.SoPMuon == soPMuon && cb.MaBanSach == ct2.MaBanSach select 1).Any()))
                          orderby ct2.MaBanSach
                          select new { ct2.MaBanSach, cm.TenSach, ct2.TinhTrangTra, ct2.SoNgayQuaHan }).ToListAsync(ct);
        return dong.Select(x => new SachViPhamDto(x.MaBanSach, x.TenSach, x.TinhTrangTra, x.SoNgayQuaHan, MoTa(x.TinhTrangTra, x.SoNgayQuaHan))).ToList();
    }
}
