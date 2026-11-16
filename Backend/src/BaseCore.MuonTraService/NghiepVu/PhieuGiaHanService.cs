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
using Microsoft.Extensions.Options;
using KQ = BaseCore.Entities.MienGiaTri.KetQuaGiaHan;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record DongGiaHanDto(string MaBanSach, string TenSach, DateOnly HanTraCu, int SoLanDaGiaHan, string? KetQuaGiaHan,
    DateOnly? HanTraMoi, string? LyDoKhongGh, int SoLanGiaHan, bool DaLuu);
public sealed record PhieuGiaHanDto(string? SoPGiaHan, DateOnly? NgayLapPGiaHan, string SoPMuon, BanDocTomTat BanDoc, DateOnly NgayDeNghi,
    string? TenNd, IReadOnlyList<DongGiaHanDto> ChiTiet, int SoNgayGiaHan);
public sealed record DongGiaHanNhap(string? MaBanSach, string? KetQuaGiaHan, string? LyDoKhongGh);
public sealed record PhieuGiaHanNhap(string? SoPMuon, string? SoPGiaHan, DateOnly? NgayDeNghi, List<DongGiaHanNhap>? ChiTiet);

/// <summary>SPEC 6.2.2 Lập Phiếu gia hạn sách (01.02.01) – MB03. Điều kiện gia hạn: GIA_DINH D45.</summary>
public sealed class PhieuGiaHanService(MuonTraDbContext db, IClock clock, ChungTuChung chung, IOptions<QuyDinhOptions> quyDinh)
{
    private QuyDinhOptions Q => quyDinh.Value;

    /// <summary>txtSoPhieuMuon, btnTimPhieuMuon (TT031, TT032): Bạn đọc + các bản sách chưa trả của phiếu mượn.</summary>
    public async Task<PhieuGiaHanDto> TimPhieuMuonAsync(string? soPMuon, CancellationToken ct)
    {
        var pm = await chung.PhieuMuonAsync(soPMuon, ct);
        var bd = await chung.BanDocAsync(pm.MaBanDoc, ct);
        var dong = await chung.DongPhieuMuonAsync(pm.SoPMuon, chiChuaTra: true, ct);
        if (dong.Count == 0) throw LoiNghiepVu.XungDot("DA_TRA_HET", $"Phiếu mượn sách {pm.SoPMuon} không còn bản sách chưa trả để gia hạn.");
        return new PhieuGiaHanDto(null, null, pm.SoPMuon, ChungTuChung.TomTat(bd), clock.HomNay, null,
            dong.Select(d => new DongGiaHanDto(d.MaBanSach, d.TenSach, d.HanHienHanh, d.SoLanGiaHan, null, null, null, d.SoLanGiaHan, false)).ToList(),
            Q.SoNgayGiaHan);
    }

    /// <summary>
    /// dgvChiTiet (chọn bản sách + ngay_denghi): TT034 số lần đã gia hạn, TT035 kiểm tra điều kiện, TT036 lý do từ chối,
    /// TT037 hạn trả mới = hantra_cu + số ngày được gia hạn.
    /// </summary>
    public async Task<IReadOnlyList<DongGiaHanDto>> KiemTraAsync(PhieuGiaHanNhap n, CancellationToken ct)
    {
        var (_, _, dong) = await TinhAsync(n, null, ct);
        return dong;
    }

    /// <summary>btnTimPhieu (TT038): nạp Phiếu gia hạn sách đã lưu ở chế độ sửa; dữ liệu in MB03.</summary>
    public async Task<PhieuGiaHanDto> ChiTietAsync(string? soPGiaHan, CancellationToken ct)
    {
        var so = (soPGiaHan ?? string.Empty).Trim().ToUpperInvariant();
        var pg = await db.PhieuGiaHan.AsNoTracking().SingleOrDefaultAsync(x => x.SoPGiaHan == so, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_PHIEU_GIA_HAN", $"Không tìm thấy Phiếu gia hạn sách {so}.");
        var bd = await chung.BanDocAsync(pg.MaBanDoc, ct);
        var truoc = (await chung.DongPhieuMuonAsync(pg.SoPMuon, chiChuaTra: false, ct, truSoPGiaHan: pg.SoPGiaHan)).ToDictionary(x => x.MaBanSach);
        var ct2 = await db.CtPhieuGiaHan.AsNoTracking().Where(x => x.SoPGiaHan == so).OrderBy(x => x.MaBanSach).ToListAsync(ct);
        return new PhieuGiaHanDto(pg.SoPGiaHan, pg.NgayLapPGiaHan, pg.SoPMuon, ChungTuChung.TomTat(bd), pg.NgayDeNghi, pg.TenNd,
            ct2.Select(x => new DongGiaHanDto(x.MaBanSach, x.TenSach, x.HanTraCu, truoc.TryGetValue(x.MaBanSach, out var t) ? t.SoLanGiaHan : 0,
                x.KetQuaGiaHan, x.HanTraMoi, x.LyDoKhongGh, x.SoLanGiaHan, true)).ToList(),
            Q.SoNgayGiaHan);
    }

    /// <summary>
    /// btnIn: TT033 kiểm soát dữ liệu nhập; phiếu mới → TT039 sinh so_pgiahan, ngaylap_pgiahan, TT040 lưu; phiếu đã lưu → TT041
    /// cập nhật. Ghi nhật ký.
    /// </summary>
    public async Task<KetQuaLuuChungTu> LuuAsync(PhieuGiaHanNhap n, string maNd, string tenNd, CancellationToken ct)
    {
        return await db.ChayAsync(async c =>
        {
            PhieuGiaHan? pg = null;
            if (!string.IsNullOrWhiteSpace(n.SoPGiaHan))
            {
                var so = n.SoPGiaHan.Trim().ToUpperInvariant();
                pg = await db.PhieuGiaHan.SingleOrDefaultAsync(x => x.SoPGiaHan == so, c)
                     ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_PHIEU_GIA_HAN", $"Không tìm thấy Phiếu gia hạn sách {so}.");
                n = n with { SoPMuon = pg.SoPMuon };
            }
            var (pm, ngayDeNghi, dong) = await TinhAsync(n, pg, c);
            var homNay = clock.HomNay;
            if (pg is null)
            {
                var so = SoChungTu.Tao(SoChungTu.PhieuGiaHan, homNay, await SoChungTuGhi.CapAsync(db, SoChungTu.PhieuGiaHan, homNay, c));
                pg = new PhieuGiaHan { SoPGiaHan = so, SoPMuon = pm.SoPMuon, MaBanDoc = pm.MaBanDoc, NgayLapPGiaHan = homNay, NgayDeNghi = ngayDeNghi, MaNd = maNd, TenNd = tenNd };
                db.PhieuGiaHan.Add(pg);
            }
            else pg.NgayDeNghi = ngayDeNghi;

            var daCo = await db.CtPhieuGiaHan.Where(x => x.SoPGiaHan == pg.SoPGiaHan).ToListAsync(c);
            var bo = daCo.Where(x => dong.All(d => d.MaBanSach != x.MaBanSach)).Select(x => x.MaBanSach).ToList();
            if (bo.Count > 0) throw LoiNghiepVu.KhongHopLe("KHONG_BO_DONG_DA_LUU", "Không được bỏ bản sách đã lưu khỏi phiếu: " + string.Join(", ", bo));
            foreach (var d in dong)
            {
                var x = daCo.FirstOrDefault(k => k.MaBanSach == d.MaBanSach);
                if (x is null)
                {
                    x = new CtPhieuGiaHan { MaBanSach = d.MaBanSach, SoPGiaHan = pg.SoPGiaHan, TenSach = d.TenSach };
                    db.CtPhieuGiaHan.Add(x);
                }
                x.HanTraCu = d.HanTraCu;
                x.KetQuaGiaHan = d.KetQuaGiaHan!;
                x.HanTraMoi = d.HanTraMoi;
                x.LyDoKhongGh = d.LyDoKhongGh;
                x.SoLanGiaHan = d.SoLanGiaHan;
            }
            var dongY = dong.Count(d => d.KetQuaGiaHan == KQ.DongY);
            await NhatKyGhi.ThemAsync(db, maNd,
                $"01.02.01 {(daCo.Count == 0 ? "Lập" : "Cập nhật")} Phiếu gia hạn sách {pg.SoPGiaHan} (Phiếu mượn {pm.SoPMuon}): đồng ý {dongY}, không đồng ý {dong.Count - dongY}",
                clock.BayGio, c);
            await db.SaveChangesAsync(c);
            return new KetQuaLuuChungTu(pg.SoPGiaHan,
                $"Đã {(daCo.Count == 0 ? "lưu" : "cập nhật")} Phiếu gia hạn sách {pg.SoPGiaHan}: đồng ý {dongY}, không đồng ý {dong.Count - dongY} bản.", daCo.Count == 0);
        }, ct);
    }

    /// <summary>Tính kết quả từng dòng theo quy định (server luôn tính lại – không tin dữ liệu gửi lên).</summary>
    private async Task<(PhieuMuon Pm, DateOnly NgayDeNghi, List<DongGiaHanDto> Dong)> TinhAsync(PhieuGiaHanNhap n, PhieuGiaHan? daLuu, CancellationToken ct)
    {
        var pm = await chung.PhieuMuonAsync(n.SoPMuon, ct);
        var bd = await chung.BanDocAsync(pm.MaBanDoc, ct);
        var homNay = clock.HomNay;
        var kt = new KiemTraDuLieu();
        var ngay = n.NgayDeNghi ?? default;
        if (n.NgayDeNghi is null) kt.Them("ngayDeNghi", "Vui lòng chọn ngày đề nghị gia hạn.");
        else if (ngay < (daLuu?.NgayLapPGiaHan ?? homNay)) kt.Them("ngayDeNghi", "Ngày đề nghị phải từ ngày hiện tại trở đi.");
        var nhap = (n.ChiTiet ?? []).Select(d => d with { MaBanSach = d.MaBanSach?.Trim().ToUpperInvariant(), LyDoKhongGh = KiemTraDuLieu.Gon(d.LyDoKhongGh) }).ToList();
        kt.Dung(nhap.Count > 0, "chiTiet", "Vui lòng chọn ít nhất một bản sách cần gia hạn.");
        kt.Dung(nhap.Select(d => d.MaBanSach).Distinct().Count() == nhap.Count, "chiTiet", "Bản sách bị trùng trong phiếu.");
        foreach (var d in nhap)
        {
            if (d.KetQuaGiaHan is not null) kt.ThuocMien("chiTiet", d.KetQuaGiaHan, KQ.TatCa, "Kết quả gia hạn");
            if (d.KetQuaGiaHan == KQ.KhongDongY) kt.BatBuoc("chiTiet", d.LyDoKhongGh, 200, $"Lý do không gia hạn bản sách {d.MaBanSach}");
        }
        kt.NemNeuCoLoi();

        // Hạn trả cũ, số lần đã gia hạn: không tính chính phiếu đang sửa
        var nguon = (await chung.DongPhieuMuonAsync(pm.SoPMuon, chiChuaTra: daLuu is null, ct, truSoPGiaHan: daLuu?.SoPGiaHan)).ToDictionary(x => x.MaBanSach);
        // Dòng hợp lệ: bản sách chưa trả của phiếu mượn, hoặc (khi sửa) dòng đã có trong chính phiếu gia hạn này
        var hopLe = (await chung.DongPhieuMuonAsync(pm.SoPMuon, chiChuaTra: true, ct)).Select(x => x.MaBanSach).ToHashSet();
        if (daLuu is not null)
            hopLe.UnionWith(await db.CtPhieuGiaHan.Where(x => x.SoPGiaHan == daLuu.SoPGiaHan).Select(x => x.MaBanSach).ToListAsync(ct));
        var coBienBanNo = await db.BienBan.AnyAsync(b => b.MaBanDoc == bd.MaBanDoc && b.HtThanhToan == MienGiaTri.HinhThucThanhToan.ChuaThanhToan, ct);
        var ketQua = new List<DongGiaHanDto>();
        var sai = new KiemTraDuLieu();
        foreach (var d in nhap)
        {
            if (!hopLe.Contains(d.MaBanSach!) || !nguon.TryGetValue(d.MaBanSach!, out var g))
            {
                sai.Them("chiTiet", $"Bản sách {d.MaBanSach} không thuộc Phiếu mượn sách {pm.SoPMuon} hoặc đã trả.");
                continue;
            }
            var hanMoi = g.HanHienHanh.AddDays(Q.SoNgayGiaHan);
            var lyDo = new List<string>();
            if (g.SoLanGiaHan >= Q.SoLanGiaHanToiDa) lyDo.Add($"Bản sách đã gia hạn đủ {Q.SoLanGiaHanToiDa} lần.");
            if (ngay > g.HanHienHanh) lyDo.Add($"Đã quá hạn trả ({g.HanHienHanh:dd/MM/yyyy}).");
            if (bd.HsdDenNgay < hanMoi) lyDo.Add($"Thẻ bạn đọc hết hạn {bd.HsdDenNgay:dd/MM/yyyy}, trước hạn trả mới.");
            if (coBienBanNo) lyDo.Add("Bạn đọc còn biên bản vi phạm chưa thanh toán.");
            var dongY = lyDo.Count == 0 && d.KetQuaGiaHan != KQ.KhongDongY;
            var lyDoGhi = dongY ? null : (d.KetQuaGiaHan == KQ.KhongDongY && d.LyDoKhongGh is not null ? d.LyDoKhongGh : string.Join(" ", lyDo));
            ketQua.Add(new DongGiaHanDto(g.MaBanSach, g.TenSach, g.HanHienHanh, g.SoLanGiaHan, dongY ? KQ.DongY : KQ.KhongDongY,
                dongY ? hanMoi : null, lyDoGhi?.Length > 200 ? lyDoGhi[..200] : lyDoGhi, g.SoLanGiaHan + (dongY ? 1 : 0), daLuu is not null));
        }
        sai.NemNeuCoLoi();
        return (pm, ngay, ketQua);
    }
}
