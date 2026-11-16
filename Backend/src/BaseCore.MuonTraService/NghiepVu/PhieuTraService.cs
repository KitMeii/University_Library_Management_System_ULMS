using BaseCore.Common.Loi;
using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.Entities.KhoSach;
using BaseCore.Entities.ViPham;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using TR = BaseCore.Entities.MienGiaTri.TinhTrangTra;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record DongTraDto(string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTra, string? TinhTrangTra, string? GhiChuTra,
    int SoNgayQuaHan, bool DaLuu, bool DaLapBienBan)
{
    /// <summary>Trường hợp vi phạm chuyển Bộ phận xử lý vi phạm: quá hạn, mất, hư hỏng (SPEC 6.2.3).</summary>
    public bool ViPham => DaLuu && (SoNgayQuaHan > 0 || TinhTrangTra is TR.Mat or TR.HuHong);
}
public sealed record PhieuTraDto(string? SoPTra, DateOnly? NgayLapPTra, string SoPMuon, BanDocTomTat BanDoc, DateOnly NgayTra, string? TenNd,
    IReadOnlyList<DongTraDto> ChiTiet);
public sealed record DongTraNhap(string? MaBanSach, string? TinhTrangTra, string? GhiChuTra);
public sealed record PhieuTraNhap(string? SoPMuon, string? SoPTra, DateOnly? NgayTra, List<DongTraNhap>? ChiTiet);

/// <summary>
/// SPEC 6.2.3 Lập Phiếu trả sách (01.02.02) – MB04. Mỗi Phiếu mượn có một Phiếu trả; trả nhiều lần thì bổ sung dòng.
/// PHIEUTRA.ngaytra = ngày trả gần nhất; so_ngay_quahan của từng dòng tính theo ngày trả của lần đó (GIA_DINH D46).
/// </summary>
public sealed class PhieuTraService(MuonTraDbContext db, IClock clock, ChungTuChung chung)
{
    /// <summary>
    /// txtSoPhieuMuon, btnTimPhieuMuon (TT032): Bạn đọc + bản sách chưa trả; Phiếu trả đã lập của phiếu mượn (TT044) nạp ở
    /// chế độ bổ sung (dòng đã lưu chỉ hiển thị).
    /// </summary>
    public async Task<PhieuTraDto> TimPhieuMuonAsync(string? soPMuon, CancellationToken ct)
    {
        var pm = await chung.PhieuMuonAsync(soPMuon, ct);
        var bd = await chung.BanDocAsync(pm.MaBanDoc, ct);
        var chuaTra = await chung.DongPhieuMuonAsync(pm.SoPMuon, chiChuaTra: true, ct);
        if (chuaTra.Count == 0) throw LoiNghiepVu.XungDot("DA_TRA_HET", $"Phiếu mượn sách {pm.SoPMuon} đã trả đủ sách.");
        var pt = await db.PhieuTra.AsNoTracking().SingleOrDefaultAsync(x => x.SoPMuon == pm.SoPMuon, ct);
        var daLuu = pt is null ? [] : await DongDaLuuAsync(pt, ct);
        return new PhieuTraDto(pt?.SoPTra, pt?.NgayLapPTra, pm.SoPMuon, ChungTuChung.TomTat(bd), clock.HomNay, pt?.TenNd,
            daLuu.Concat(chuaTra.Select(d => new DongTraDto(d.MaBanSach, d.TenSach, d.NgayMuon, d.HanHienHanh, null, null, 0, false, false))).ToList());
    }

    /// <summary>btnTimPhieu (TT044, TT048): nạp Phiếu trả sách đã lưu ở chế độ sửa; dữ liệu in MB04.</summary>
    public async Task<PhieuTraDto> ChiTietAsync(string? soPTra, CancellationToken ct)
    {
        var so = (soPTra ?? string.Empty).Trim().ToUpperInvariant();
        var pt = await db.PhieuTra.AsNoTracking().SingleOrDefaultAsync(x => x.SoPTra == so, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_PHIEU_TRA", $"Không tìm thấy Phiếu trả sách {so}.");
        var bd = await chung.BanDocAsync(pt.MaBanDoc, ct);
        return new PhieuTraDto(pt.SoPTra, pt.NgayLapPTra, pt.SoPMuon, ChungTuChung.TomTat(bd), pt.NgayTra, pt.TenNd, await DongDaLuuAsync(pt, ct));
    }

    private async Task<List<DongTraDto>> DongDaLuuAsync(PhieuTra pt, CancellationToken ct)
    {
        var han = (await chung.DongPhieuMuonAsync(pt.SoPMuon, chiChuaTra: false, ct)).ToDictionary(x => x.MaBanSach);
        var daBb = await (from cb in db.CtBienBan join bb in db.BienBan on cb.SoBienBan equals bb.SoBienBan
                          where bb.SoPMuon == pt.SoPMuon select cb.MaBanSach).Distinct().ToListAsync(ct);
        var dong = await db.CtPhieuTra.AsNoTracking().Where(x => x.SoPTra == pt.SoPTra).OrderBy(x => x.MaBanSach).ToListAsync(ct);
        return dong.Select(x =>
        {
            var h = han.GetValueOrDefault(x.MaBanSach);
            return new DongTraDto(x.MaBanSach, h?.TenSach ?? x.MaBanSach, h?.NgayMuon ?? default, h?.HanHienHanh ?? default, x.TinhTrangTra,
                x.GhiChuTra, x.SoNgayQuaHan, true, daBb.Contains(x.MaBanSach));
        }).ToList();
    }

    /// <summary>
    /// btnIn: TT045 kiểm soát dữ liệu nhập, TT046 sai lệch, TT047 số ngày quá hạn; phiếu mới → TT050 sinh so_ptra, ngaylap_ptra,
    /// TT051 lưu; đã có Phiếu trả → TT049 bổ sung; phiếu đã lưu → TT052 cập nhật. Sách trả (TT055) và trường hợp vi phạm (TT053)
    /// tự xuất hiện ở danh sách của Bộ phận kho sách / xử lý vi phạm (GIA_DINH D15). Ghi nhật ký.
    /// </summary>
    public async Task<KetQuaLuuChungTu> LuuAsync(PhieuTraNhap n, string maNd, string tenNd, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        var homNay = clock.HomNay;
        var nhap = (n.ChiTiet ?? []).Select(d => d with { MaBanSach = d.MaBanSach?.Trim().ToUpperInvariant(), GhiChuTra = KiemTraDuLieu.Gon(d.GhiChuTra) }).ToList();
        kt.Dung(nhap.Count > 0, "chiTiet", "Vui lòng chọn ít nhất một bản sách trả.");
        kt.Dung(nhap.Select(d => d.MaBanSach).Distinct().Count() == nhap.Count, "chiTiet", "Bản sách bị trùng trong phiếu.");
        foreach (var d in nhap)
        {
            kt.ThuocMien("chiTiet", d.TinhTrangTra, TR.TatCa, $"Tình trạng khi trả của bản sách {d.MaBanSach}");
            kt.ToiDa("chiTiet", d.GhiChuTra, 200, $"Ghi chú của bản sách {d.MaBanSach}");
        }
        if (n.NgayTra is null) kt.Them("ngayTra", "Vui lòng chọn ngày trả.");
        else if (n.NgayTra > homNay) kt.Them("ngayTra", "Ngày trả không được sau ngày hiện tại.");
        kt.NemNeuCoLoi();
        var ngayTra = n.NgayTra!.Value;

        return await db.ChayAsync(async c =>
        {
            PhieuTra? pt;
            string cheDo;
            if (!string.IsNullOrWhiteSpace(n.SoPTra))
            {
                var so = n.SoPTra.Trim().ToUpperInvariant();
                pt = await db.PhieuTra.SingleOrDefaultAsync(x => x.SoPTra == so, c)
                     ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_PHIEU_TRA", $"Không tìm thấy Phiếu trả sách {so}.");
                cheDo = "sua";
            }
            else
            {
                var pm0 = await chung.PhieuMuonAsync(n.SoPMuon, c);
                pt = await db.PhieuTra.SingleOrDefaultAsync(x => x.SoPMuon == pm0.SoPMuon, c);
                cheDo = pt is null ? "moi" : "bosung";
            }
            var pm = await chung.PhieuMuonAsync(pt?.SoPMuon ?? n.SoPMuon, c);
            var dongPm = (await chung.DongPhieuMuonAsync(pm.SoPMuon, chiChuaTra: false, c)).ToDictionary(x => x.MaBanSach);
            var chuaTra = (await chung.DongPhieuMuonAsync(pm.SoPMuon, chiChuaTra: true, c)).Select(x => x.MaBanSach).ToHashSet();
            var daCo = pt is null ? [] : await db.CtPhieuTra.Where(x => x.SoPTra == pt.SoPTra).ToListAsync(c);
            var daBb = await (from cb in db.CtBienBan join bb in db.BienBan on cb.SoBienBan equals bb.SoBienBan
                              where bb.SoPMuon == pm.SoPMuon select cb.MaBanSach).Distinct().ToListAsync(c);

            // TT046: bản sách không thuộc phiếu mượn / đã trả trước đó → sai lệch
            var loi = new KiemTraDuLieu();
            foreach (var d in nhap)
            {
                var laDongDaLuu = daCo.Any(x => x.MaBanSach == d.MaBanSach);
                if (!dongPm.ContainsKey(d.MaBanSach!)) loi.Them("chiTiet", $"Sai lệch: bản sách {d.MaBanSach} không thuộc Phiếu mượn sách {pm.SoPMuon}.");
                else if ((!laDongDaLuu || cheDo != "sua") && !chuaTra.Contains(d.MaBanSach!))
                    loi.Them("chiTiet", $"Sai lệch: bản sách {d.MaBanSach} đã được trả trước đó{(laDongDaLuu ? $" (Phiếu trả sách {pt!.SoPTra}; sửa bằng “Tìm phiếu”)" : "")}.");
                else if (ngayTra < dongPm[d.MaBanSach!].NgayMuon)
                    loi.Them("ngayTra", $"Ngày trả phải từ ngày mượn ({dongPm[d.MaBanSach!].NgayMuon:dd/MM/yyyy}) trở đi.");
            }
            loi.NemNeuCoLoi();

            if (pt is null)
            {
                var so = SoChungTu.Tao(SoChungTu.PhieuTra, homNay, await SoChungTuGhi.CapAsync(db, SoChungTu.PhieuTra, homNay, c));
                pt = new PhieuTra { SoPTra = so, SoPMuon = pm.SoPMuon, MaBanDoc = pm.MaBanDoc, NgayLapPTra = homNay, NgayTra = ngayTra, MaNd = maNd, TenNd = tenNd };
                db.PhieuTra.Add(pt);
            }
            else if (cheDo == "sua" || ngayTra > pt.NgayTra) pt.NgayTra = ngayTra;

            int them = 0, sua = 0, viPham = 0;
            foreach (var d in nhap)
            {
                var quaHan = Math.Max(0, ngayTra.DayNumber - dongPm[d.MaBanSach!].HanHienHanh.DayNumber);
                var x = daCo.FirstOrDefault(k => k.MaBanSach == d.MaBanSach);
                if (x is null)
                {
                    db.CtPhieuTra.Add(new CtPhieuTra { MaBanSach = d.MaBanSach!, SoPTra = pt.SoPTra, TinhTrangTra = d.TinhTrangTra!, GhiChuTra = d.GhiChuTra, SoNgayQuaHan = quaHan });
                    them++;
                }
                else
                {
                    if (x.TinhTrangTra == d.TinhTrangTra && x.GhiChuTra == d.GhiChuTra && (cheDo != "sua" || x.SoNgayQuaHan == quaHan)) continue;
                    if (daBb.Contains(x.MaBanSach))
                        throw LoiNghiepVu.XungDot("DA_LAP_BIEN_BAN", $"Bản sách {x.MaBanSach} đã được lập biên bản vi phạm, không sửa được dòng trả.");
                    x.TinhTrangTra = d.TinhTrangTra!;
                    x.GhiChuTra = d.GhiChuTra;
                    if (cheDo == "sua") x.SoNgayQuaHan = quaHan;
                    sua++;
                }
                if (quaHan > 0 || d.TinhTrangTra is TR.Mat or TR.HuHong) viPham++;
            }
            // Dòng đã lưu không gửi lên thì giữ nguyên (không bỏ dòng đã lưu – SPEC)
            if (them == 0 && sua == 0) throw LoiNghiepVu.KhongHopLe("KHONG_CO_THAY_DOI", "Phiếu trả sách không có thay đổi nào để lưu.");

            var noiDung = cheDo switch
            {
                "moi" => $"01.02.02 Lập Phiếu trả sách {pt.SoPTra} (Phiếu mượn {pm.SoPMuon}): trả {them} bản, {viPham} trường hợp vi phạm",
                "bosung" => $"01.02.02 Bổ sung {them} bản sách trả vào Phiếu trả sách {pt.SoPTra} (Phiếu mượn {pm.SoPMuon}), {viPham} trường hợp vi phạm",
                _ => $"01.02.02 Cập nhật Phiếu trả sách {pt.SoPTra}: thêm {them}, sửa {sua} dòng"
            };
            await NhatKyGhi.ThemAsync(db, maNd, noiDung, clock.BayGio, c);
            await db.SaveChangesAsync(c);

            var tb = cheDo switch
            {
                "moi" => $"Đã lưu Phiếu trả sách {pt.SoPTra} ({them} bản).",
                "bosung" => $"Đã bổ sung {them} bản sách vào Phiếu trả sách {pt.SoPTra}.",
                _ => $"Đã cập nhật Phiếu trả sách {pt.SoPTra}."
            };
            if (them > 0) tb += " Sách trả đã chuyển cho Bộ phận kho sách.";
            if (viPham > 0) tb += $" Đã chuyển {viPham} trường hợp vi phạm cho Bộ phận xử lý vi phạm.";
            return new KetQuaLuuChungTu(pt.SoPTra, tb, cheDo == "moi");
        }, ct);
    }
}
