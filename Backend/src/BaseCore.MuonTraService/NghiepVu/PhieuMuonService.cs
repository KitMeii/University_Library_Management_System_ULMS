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
using TT = BaseCore.Entities.MienGiaTri.TinhTrangSach;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record BanSachMuonDto(string MaBanSach, string TenSach, string TacGia, string MaKhuVuc, string ViTri, string TinhTrang);
public sealed record KetQuaKiemTraBanSach(bool DapUng, string? LyDo, BanSachMuonDto? BanSach, DateOnly NgayMuon, DateOnly HanTra,
    IReadOnlyList<BanSachMuonDto> BanThayThe);
public sealed record DongPhieuMuonDto(string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTra, string TinhTrangMuon,
    string? GhiChuMuon, string? ViTri, bool DaTra);
public sealed record PhieuMuonDto(string SoPMuon, DateOnly NgayLapPMuon, BanDocTomTat BanDoc, string TenNd, IReadOnlyList<DongPhieuMuonDto> ChiTiet)
{
    /// <summary>MB02: Tổng số bản sách mượn = COUNT(CT_PHIEUMUON.ma_bansach) của phiếu.</summary>
    public int TongSoBan => ChiTiet.Count;
}
public sealed record KetQuaTimBanDocMuon(BanDocTomTat BanDoc, DieuKienMuonDto DieuKien, int SoNgayMuon, PhieuMuonDto? PhieuTrongNgay);
public sealed record DongMuonNhap(string? MaBanSach, string? TinhTrangMuon, string? GhiChuMuon);
public sealed record PhieuMuonNhap(string? MaBanDoc, string? SoPMuon, List<DongMuonNhap>? ChiTiet);

/// <summary>SPEC 6.2.1 Lập Phiếu mượn sách (01.01.03) – MB02.</summary>
public sealed class PhieuMuonService(MuonTraDbContext db, IClock clock, ChungTuChung chung, BanDocService banDoc, IOptions<QuyDinhOptions> quyDinh)
{
    /// <summary>
    /// txtMaSV, btnTimBanDoc: tìm BANDOC theo ma_svhvcb, kiểm tra điều kiện mượn (TT007–TT011), tìm Phiếu mượn sách lập
    /// trong ngày của Bạn đọc (TT013) để nạp ở chế độ bổ sung.
    /// </summary>
    public async Task<KetQuaTimBanDocMuon> TimBanDocAsync(string? maSv, CancellationToken ct)
    {
        maSv = (maSv ?? string.Empty).Trim();
        if (maSv.Length == 0) new KiemTraDuLieu().Them("maSv", "Vui lòng nhập mã SV/HV/CB.").NemNeuCoLoi();
        var bd = await db.BanDoc.AsNoTracking().SingleOrDefaultAsync(x => x.MaSvHvCb == maSv, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BAN_DOC", $"Không tìm thấy Bạn đọc có mã SV/HV/CB “{maSv}”.");
        var dk = await banDoc.KiemTraDieuKienAsync(bd.MaBanDoc, ct);
        var homNay = clock.HomNay;
        var soPhieu = await db.PhieuMuon.AsNoTracking()
            .Where(x => x.MaBanDoc == bd.MaBanDoc && x.NgayLapPMuon == homNay)
            .OrderByDescending(x => x.SoPMuon).Select(x => x.SoPMuon).FirstOrDefaultAsync(ct);
        return new KetQuaTimBanDocMuon(ChungTuChung.TomTat(bd), dk, quyDinh.Value.Cua(bd.DoiTuong).SoNgayMuon,
            soPhieu is null ? null : await ChiTietAsync(soPhieu, ct));
    }

    /// <summary>
    /// txtMaBanSach, btnThemSach: TT014 tìm bản sách, TT015 vị trí, TT021 hạn trả. Không “Sẵn sàng” hoặc đang thuộc phiếu
    /// mượn chưa trả → không đáp ứng (TT019) kèm bản sách thay thế cùng tên, cùng tác giả còn sẵn trong kho (TT016, TT017).
    /// </summary>
    public async Task<KetQuaKiemTraBanSach> KiemTraBanSachAsync(string? maBanSach, string? maBanDoc, CancellationToken ct)
    {
        var ma = (maBanSach ?? string.Empty).Trim().ToUpperInvariant();
        if (ma.Length == 0) new KiemTraDuLieu().Them("maBanSach", "Vui lòng nhập mã bản sách.").NemNeuCoLoi();
        var bd = await chung.BanDocAsync(maBanDoc ?? string.Empty, ct);
        var homNay = clock.HomNay;
        var han = homNay.AddDays(quyDinh.Value.Cua(bd.DoiTuong).SoNgayMuon);
        var s = await TimBanSachAsync(ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BAN_SACH", $"Không có bản sách {ma} trong danh mục sách.");
        var dangMuon = await chung.BanDangMuon().AnyAsync(x => x == ma, ct);
        var choKho = !dangMuon && await chung.BanChoKhoXuLy().AnyAsync(x => x == ma, ct);
        string? lyDo = s.TinhTrang != TT.SanSang ? $"Bản sách {ma} đang ở tình trạng “{s.TinhTrang}”, không đáp ứng được."
                     : dangMuon ? $"Bản sách {ma} đang được cho mượn theo phiếu khác, không đáp ứng được."
                     : choKho ? $"Bản sách {ma} được trả ở tình trạng mất/hư hỏng, chờ Bộ phận kho sách cập nhật – không đáp ứng được."
                     : null;
        IReadOnlyList<BanSachMuonDto> thayThe = [];
        if (lyDo is not null)
        {
            var banMuon = chung.BanDangMuon();
            var choKhoXuLy = chung.BanChoKhoXuLy();
            thayThe = await (from x in db.Sach.AsNoTracking()
                             join kv in db.KhuVuc on x.MaKhuVuc equals kv.MaKhuVuc
                             where x.MaBanSach != ma && x.TenSach == s.TenSach && x.TacGia == s.TacGia
                                   && x.TinhTrang == TT.SanSang && !banMuon.Contains(x.MaBanSach) && !choKhoXuLy.Contains(x.MaBanSach)
                             orderby x.MaBanSach
                             select new BanSachMuonDto(x.MaBanSach, x.TenSach, x.TacGia, x.MaKhuVuc, kv.ViTri, x.TinhTrang)).Take(5).ToListAsync(ct);
        }
        return new KetQuaKiemTraBanSach(lyDo is null, lyDo, s, homNay, han, thayThe);
    }

    /// <summary>btnTimPhieu (TT022): nạp Phiếu mượn sách đã lưu lên form ở chế độ sửa; cũng là dữ liệu mẫu in MB02.</summary>
    public async Task<PhieuMuonDto> ChiTietAsync(string? soPMuon, CancellationToken ct)
    {
        var pm = await chung.PhieuMuonAsync(soPMuon, ct);
        var bd = await chung.BanDocAsync(pm.MaBanDoc, ct);
        var chuaTra = (await chung.DongPhieuMuonAsync(pm.SoPMuon, chiChuaTra: true, ct)).Select(x => x.MaBanSach).ToHashSet();
        var dong = await (from cm in db.CtPhieuMuon.AsNoTracking()
                          join s in db.Sach on cm.MaBanSach equals s.MaBanSach
                          join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
                          where cm.SoPMuon == pm.SoPMuon
                          orderby cm.NgayMuon, cm.MaBanSach
                          select new { cm, kv.ViTri }).ToListAsync(ct);
        return new PhieuMuonDto(pm.SoPMuon, pm.NgayLapPMuon, ChungTuChung.TomTat(bd), pm.TenNd,
            dong.Select(x => new DongPhieuMuonDto(x.cm.MaBanSach, x.cm.TenSach, x.cm.NgayMuon, x.cm.HanTra, x.cm.TinhTrangMuon,
                x.cm.GhiChuMuon, x.ViTri, !chuaTra.Contains(x.cm.MaBanSach))).ToList());
    }

    /// <summary>
    /// btnIn: kiểm soát dữ liệu nhập (TT020); phiếu mới → sinh so_pmuon, ngaylap_pmuon (TT024), lưu (TT025);
    /// Bạn đọc đã có phiếu lập trong ngày → bổ sung vào phiếu đó, không cấp số mới (TT023); phiếu đã lưu → cập nhật (TT026).
    /// Sách đã cho mượn tự xuất hiện ở dgvChuyenDen của Bộ phận kho sách (TT027 – GIA_DINH D15). Ghi nhật ký.
    /// </summary>
    public async Task<KetQuaLuuChungTu> LuuAsync(PhieuMuonNhap n, string maNd, string tenNd, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        var dongNhap = (n.ChiTiet ?? []).Select(d => d with
        {
            MaBanSach = d.MaBanSach?.Trim().ToUpperInvariant(), TinhTrangMuon = d.TinhTrangMuon?.Trim(), GhiChuMuon = KiemTraDuLieu.Gon(d.GhiChuMuon)
        }).ToList();
        kt.Dung(dongNhap.Count > 0, "chiTiet", "Phiếu mượn sách phải có ít nhất một bản sách.");
        kt.Dung(dongNhap.Select(d => d.MaBanSach).Distinct().Count() == dongNhap.Count, "chiTiet", "Bản sách bị trùng trong phiếu.");
        foreach (var d in dongNhap)
        {
            kt.Ma("chiTiet", d.MaBanSach, 10, "Mã bản sách");
            kt.BatBuoc("chiTiet", d.TinhTrangMuon, 50, $"Tình trạng khi mượn của bản sách {d.MaBanSach}");
            kt.ToiDa("chiTiet", d.GhiChuMuon, 200, $"Ghi chú của bản sách {d.MaBanSach}");
        }
        kt.NemNeuCoLoi();

        var bd = await chung.BanDocAsync(n.MaBanDoc ?? string.Empty, ct);
        var homNay = clock.HomNay;
        var soNgay = quyDinh.Value.Cua(bd.DoiTuong).SoNgayMuon;

        return await db.ChayAsync(async c =>
        {
            // Xác định phiếu: sửa phiếu đã lưu / bổ sung vào phiếu lập trong ngày / phiếu mới
            PhieuMuon? pm = null;
            if (!string.IsNullOrWhiteSpace(n.SoPMuon))
            {
                var so = n.SoPMuon.Trim().ToUpperInvariant();
                pm = await db.PhieuMuon.SingleOrDefaultAsync(x => x.SoPMuon == so, c)
                     ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_PHIEU_MUON", $"Không tìm thấy Phiếu mượn sách {so}.");
                if (pm.MaBanDoc != bd.MaBanDoc) throw LoiNghiepVu.KhongHopLe("SAI_BAN_DOC", "Phiếu mượn sách không thuộc Bạn đọc này.");
            }
            pm ??= await db.PhieuMuon.Where(x => x.MaBanDoc == bd.MaBanDoc && x.NgayLapPMuon == homNay)
                .OrderByDescending(x => x.SoPMuon).FirstOrDefaultAsync(c);
            var cheDo = pm is null ? "moi" : !string.IsNullOrWhiteSpace(n.SoPMuon) ? "sua" : "bosung";

            var daCo = pm is null ? [] : await db.CtPhieuMuon.Where(x => x.SoPMuon == pm.SoPMuon).ToListAsync(c);
            var maNhap = dongNhap.Select(d => d.MaBanSach!).ToHashSet();
            // Dòng đã lưu không được bỏ khỏi phiếu (SPEC: chỉ bỏ dòng chưa lưu)
            if (cheDo == "sua")
            {
                var bo = daCo.Where(x => !maNhap.Contains(x.MaBanSach)).Select(x => x.MaBanSach).ToList();
                if (bo.Count > 0) throw LoiNghiepVu.KhongHopLe("KHONG_BO_DONG_DA_LUU", "Không được bỏ bản sách đã lưu khỏi phiếu: " + string.Join(", ", bo));
            }
            var moi = dongNhap.Where(d => daCo.All(x => x.MaBanSach != d.MaBanSach)).ToList();
            if (moi.Count > 0 && pm is not null && pm.NgayLapPMuon != homNay)
                throw LoiNghiepVu.KhongHopLe("PHIEU_KHAC_NGAY", $"Chỉ bổ sung bản sách vào Phiếu mượn sách lập trong ngày; phiếu {pm.SoPMuon} lập ngày {pm.NgayLapPMuon:dd/MM/yyyy}. Vui lòng lập phiếu mới.");

            if (moi.Count > 0)
            {
                // Điều kiện mượn (SPEC mục 4) – kiểm tra lại ở server
                var dk = await banDoc.KiemTraDieuKienAsync(bd.MaBanDoc, c);
                var lyDo = dk.LyDo.Where(l => !l.StartsWith("Đã mượn đủ", StringComparison.Ordinal)).ToList();
                if (lyDo.Count > 0) throw LoiNghiepVu.XungDot("KHONG_DU_DIEU_KIEN", "Bạn đọc không đủ điều kiện mượn: " + string.Join(" ", lyDo));
                if (dk.SoDangMuon + moi.Count > dk.SoBanToiDa)
                    throw LoiNghiepVu.XungDot("VUOT_SO_BAN_TOI_DA",
                        $"Vượt số bản tối đa: đang mượn {dk.SoDangMuon}, thêm {moi.Count}, tối đa {dk.SoBanToiDa} bản đối với {bd.DoiTuong.ToLowerInvariant()}.");

                await chung.KhoaBanSachAsync(moi.Select(d => d.MaBanSach!), c);
                var banMuon = chung.BanDangMuon();
                var choKhoXuLy = chung.BanChoKhoXuLy();
                var sach = await db.Sach.Where(x => maNhap.Contains(x.MaBanSach))
                    .Select(x => new { x.MaBanSach, x.TenSach, x.TinhTrang, DangMuon = banMuon.Contains(x.MaBanSach) || choKhoXuLy.Contains(x.MaBanSach) }).ToListAsync(c);
                var loi = new KiemTraDuLieu();
                foreach (var d in moi)
                {
                    var s = sach.FirstOrDefault(x => x.MaBanSach == d.MaBanSach);
                    if (s is null) loi.Them("chiTiet", $"Không có bản sách {d.MaBanSach} trong danh mục sách.");
                    else if (s.TinhTrang != TT.SanSang || s.DangMuon) loi.Them("chiTiet", $"Bản sách {d.MaBanSach} không còn sẵn sàng để cho mượn.");
                }
                loi.NemNeuCoLoi();

                if (pm is null)
                {
                    var so = SoChungTu.Tao(SoChungTu.PhieuMuon, homNay, await SoChungTuGhi.CapAsync(db, SoChungTu.PhieuMuon, homNay, c));
                    pm = new PhieuMuon { SoPMuon = so, MaBanDoc = bd.MaBanDoc, NgayLapPMuon = homNay, HoTen = bd.HoTen, MaNd = maNd, TenNd = tenNd };
                    db.PhieuMuon.Add(pm);
                }
                foreach (var d in moi)
                    db.CtPhieuMuon.Add(new CtPhieuMuon
                    {
                        SoPMuon = pm.SoPMuon, MaBanSach = d.MaBanSach!, NgayMuon = homNay, HanTra = homNay.AddDays(soNgay),
                        TinhTrangMuon = d.TinhTrangMuon!, GhiChuMuon = d.GhiChuMuon,
                        TenSach = sach.First(x => x.MaBanSach == d.MaBanSach).TenSach
                    });
            }

            // Sửa tình trạng/ghi chú khi mượn của dòng đã lưu (TT026)
            var sua = 0;
            foreach (var x in daCo)
            {
                var d = dongNhap.FirstOrDefault(k => k.MaBanSach == x.MaBanSach);
                if (d is null || (x.TinhTrangMuon == d.TinhTrangMuon && x.GhiChuMuon == d.GhiChuMuon)) continue;
                x.TinhTrangMuon = d.TinhTrangMuon!;
                x.GhiChuMuon = d.GhiChuMuon;
                sua++;
            }
            if (moi.Count == 0 && sua == 0)
                throw LoiNghiepVu.KhongHopLe("KHONG_CO_THAY_DOI", "Phiếu mượn sách không có thay đổi nào để lưu.");

            var dsMoi = string.Join(", ", moi.Select(d => d.MaBanSach));
            var noiDung = cheDo switch
            {
                "moi" => $"01.01.03 Lập Phiếu mượn sách {pm!.SoPMuon} – Bạn đọc {bd.MaBanDoc} {bd.HoTen}: {moi.Count} bản ({dsMoi})",
                "bosung" => $"01.01.03 Bổ sung {moi.Count} bản sách ({dsMoi}) vào Phiếu mượn sách {pm!.SoPMuon} lập trong ngày",
                _ => $"01.01.03 Cập nhật Phiếu mượn sách {pm!.SoPMuon}: thêm {moi.Count} bản{(moi.Count > 0 ? " (" + dsMoi + ")" : "")}, sửa {sua} dòng"
            };
            await NhatKyGhi.ThemAsync(db, maNd, noiDung, clock.BayGio, c);
            await db.SaveChangesAsync(c);

            var thongBao = cheDo switch
            {
                "moi" => $"Đã lưu Phiếu mượn sách {pm.SoPMuon} ({moi.Count} bản).",
                "bosung" => $"Đã bổ sung {moi.Count} bản sách vào Phiếu mượn sách {pm.SoPMuon} lập trong ngày.",
                _ => $"Đã cập nhật Phiếu mượn sách {pm.SoPMuon}."
            };
            if (moi.Count > 0) thongBao += " Thông tin sách đã cho mượn đã chuyển cho Bộ phận kho sách.";
            return new KetQuaLuuChungTu(pm.SoPMuon, thongBao, cheDo == "moi");
        }, ct);
    }

    private Task<BanSachMuonDto?> TimBanSachAsync(string ma, CancellationToken ct) =>
        (from s in db.Sach.AsNoTracking()
         join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
         where s.MaBanSach == ma
         select new BanSachMuonDto(s.MaBanSach, s.TenSach, s.TacGia, s.MaKhuVuc, kv.ViTri, s.TinhTrang)).SingleOrDefaultAsync(ct);
}
