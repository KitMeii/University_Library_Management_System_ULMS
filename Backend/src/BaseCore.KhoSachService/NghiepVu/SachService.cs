using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.DTO.Common;
using BaseCore.Entities;
using BaseCore.Entities.KhoSach;
using BaseCore.KhoSachService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using TT = BaseCore.Entities.MienGiaTri.TinhTrangSach;

namespace BaseCore.KhoSachService.NghiepVu;

public sealed record SachDto
{
    public string MaBanSach { get; init; } = null!;
    public string TenSach { get; init; } = null!;
    public string TacGia { get; init; } = null!;
    public string MaKhuVuc { get; init; } = null!;
    public string ViTri { get; init; } = null!;
    public string LoaiKhuVuc { get; init; } = null!;
    public string TinhTrang { get; init; } = null!;
}
public sealed record SachNhap(string? MaBanSach, string? MaKhuVuc, string? TenSach, string? TacGia, string? TinhTrang);
public sealed record ThongKeTinhTrang(string TinhTrang, int SoSach);

/// <summary>Loại thông tin bộ phận khác chuyển đến (SPEC 6.1.1 dgvChuyenDen, GIA_DINH D15).</summary>
public static class LoaiChuyenDen
{
    public const string DaChoMuon = "Sách đã cho mượn";      // 03.01.04 ← 01.01.03
    public const string SachTra = "Sách trả";                 // 03.02.03 ← 01.02.02
    public const string MatHuHong = "Sách bị mất, hư hỏng";   // 03.03.02 ← 02.03.01
}

public sealed record ChuyenDenDto(
    string MaBanSach, string TenSach, string Loai, string ChungTu, DateOnly? Ngay, string TinhTrangHienTai, string TinhTrangDeXuat, string TienTrinh);

/// <summary>SPEC 6.1.1 Danh mục sách – kèm các tiến trình 03.01.02, 03.01.04, 03.02.03, 03.03.02, 03.04.01 của N3.</summary>
public sealed class SachService(KhoSachDbContext db, IClock clock)
{
    private static readonly string[] SapXep =
        [nameof(SachDto.MaBanSach), nameof(SachDto.TenSach), nameof(SachDto.TacGia), nameof(SachDto.MaKhuVuc), nameof(SachDto.TinhTrang)];

    /// <summary>Bản sách thuộc phiếu mượn chưa trả (chưa có dòng CT_PHIEUTRA của phiếu trả cùng phiếu mượn).</summary>
    private IQueryable<string> BanDangMuon() =>
        from cm in db.CtPhieuMuon
        where !(from ct in db.CtPhieuTra
                join pt in db.PhieuTra on ct.SoPTra equals pt.SoPTra
                where pt.SoPMuon == cm.SoPMuon && ct.MaBanSach == cm.MaBanSach
                select 1).Any()
        select cm.MaBanSach;

    /// <summary>
    /// frmDanhMucSach (Load), cboLocTinhTrang (03.04.01), txtTimKiem + btnTimKiem (03.01.02: lọc theo mã, tên, tác giả,
    /// tình trạng, khu vực; loại bản sách đang thuộc phiếu mượn chưa trả; kèm vị trí lưu trữ).
    /// </summary>
    public Task<KetQuaTrang<SachDto>> DanhSachAsync(ThamSoLuoi ts, string? tinhTrang, CancellationToken ct)
    {
        var q = from s in db.Sach.AsNoTracking()
                join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
                select new SachDto { MaBanSach = s.MaBanSach, TenSach = s.TenSach, TacGia = s.TacGia, MaKhuVuc = s.MaKhuVuc, ViTri = kv.ViTri, LoaiKhuVuc = kv.LoaiKhuVuc, TinhTrang = s.TinhTrang };
        if (!string.IsNullOrWhiteSpace(tinhTrang)) q = q.Where(x => x.TinhTrang == tinhTrang);
        var k = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (k is not null)
        {
            q = q.Where(x => x.MaBanSach.Contains(k) || x.TenSach.Contains(k) || x.TacGia.Contains(k) ||
                             x.TinhTrang.Contains(k) || x.MaKhuVuc.Contains(k) || x.ViTri.Contains(k));
            var dangMuon = BanDangMuon();
            q = q.Where(x => !dangMuon.Contains(x.MaBanSach));
        }
        return q.LayTrangAsync(ts, SapXep, nameof(SachDto.MaBanSach), x => x, ct);
    }

    /// <summary>03.04.01: Số sách = COUNT(ma_bansach) theo tinhtrang.</summary>
    public async Task<IReadOnlyList<ThongKeTinhTrang>> ThongKeAsync(CancellationToken ct)
    {
        var dem = await db.Sach.AsNoTracking().GroupBy(s => s.TinhTrang)
            .Select(g => new { g.Key, SoSach = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.SoSach, ct);
        return TT.TatCa.Select(t => new ThongKeTinhTrang(t, dem.GetValueOrDefault(t))).ToList();
    }

    public async Task<SachDto> ThemAsync(SachNhap n, string maNd, CancellationToken ct)
    {
        var kt = await KiemTraAsync(n, true, ct);
        if (await db.Sach.AnyAsync(x => x.MaBanSach == n.MaBanSach, ct)) kt.Them("maBanSach", $"Mã bản sách {n.MaBanSach} đã tồn tại.");
        kt.NemNeuCoLoi();
        var s = new Sach
        {
            MaBanSach = n.MaBanSach!.Trim(), MaKhuVuc = n.MaKhuVuc!, TenSach = n.TenSach!.Trim(), TacGia = n.TacGia!.Trim(),
            TinhTrang = string.IsNullOrEmpty(n.TinhTrang) ? TT.SanSang : n.TinhTrang
        };
        db.Sach.Add(s);
        await NhatKyGhi.ThemAsync(db, maNd, $"Thêm bản sách {s.MaBanSach} – {s.TenSach} ({s.TacGia}), khu vực {s.MaKhuVuc}, {s.TinhTrang}", clock.BayGio, ct);
        await LuuAsync(ct);
        return await MotAsync(s.MaBanSach, ct);
    }

    /// <summary>btnSua → btnLuu: sửa ma_khuvuc, ten_sach, tacgia, tinhtrang (không sửa ma_bansach).</summary>
    public async Task<SachDto> SuaAsync(string ma, SachNhap n, string maNd, CancellationToken ct)
    {
        var s = await db.Sach.SingleOrDefaultAsync(x => x.MaBanSach == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy bản sách {ma}.");
        (await KiemTraAsync(n, false, ct)).NemNeuCoLoi();
        var cu = $"{s.TenSach} ({s.TacGia}), {s.MaKhuVuc}, {s.TinhTrang}";
        s.MaKhuVuc = n.MaKhuVuc!;
        s.TenSach = n.TenSach!.Trim();
        s.TacGia = n.TacGia!.Trim();
        s.TinhTrang = n.TinhTrang!;
        await NhatKyGhi.ThemAsync(db, maNd, $"Sửa bản sách {ma}: {cu} → {s.TenSach} ({s.TacGia}), {s.MaKhuVuc}, {s.TinhTrang}", clock.BayGio, ct);
        await LuuAsync(ct);
        return await MotAsync(ma, ct);
    }

    /// <summary>btnXoa: bản sách đã có trong phiếu mượn (CT_PHIEUMUON) hoặc biên bản vi phạm (CT_BIENBAN) thì không cho xóa.</summary>
    public async Task XoaAsync(string ma, string maNd, CancellationToken ct)
    {
        var s = await db.Sach.SingleOrDefaultAsync(x => x.MaBanSach == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy bản sách {ma}.");
        if (await db.CtPhieuMuon.AnyAsync(x => x.MaBanSach == ma, ct))
            throw LoiNghiepVu.XungDot("DA_CO_PHIEU_MUON", $"Bản sách {ma} đã có trong phiếu mượn nên không thể xóa.");
        if (await db.CtBienBan.AnyAsync(x => x.MaBanSach == ma, ct))
            throw LoiNghiepVu.XungDot("DA_CO_BIEN_BAN", $"Bản sách {ma} đã có trong biên bản vi phạm nên không thể xóa.");
        if (await db.PhieuDatCho.AnyAsync(x => x.MaBanSach == ma, ct))
            throw LoiNghiepVu.XungDot("DA_CO_DAT_CHO", $"Bản sách {ma} đã phát sinh đặt chỗ của bạn đọc nên không thể xóa.");

        var tenSach = s.TenSach;
        await db.ChayAsync(async c =>
        {
            // bảng mở rộng X03 cùng service
            await db.DauSachBanSach.Where(x => x.MaBanSach == ma).ExecuteDeleteAsync(c);
            await db.Sach.Where(x => x.MaBanSach == ma).ExecuteDeleteAsync(c);
            await NhatKyGhi.ThemAsync(db, maNd, $"Xóa bản sách {ma} – {tenSach}", clock.BayGio, c);
            await db.SaveChangesAsync(c);
        }, ct);
    }

    /// <summary>
    /// dgvChuyenDen: thông tin bộ phận khác chuyển đến, suy ra từ dữ liệu (GIA_DINH D15). Dòng tự hết khi
    /// SACH.tinhtrang đã khớp (“đánh dấu đã xử lý”).
    /// </summary>
    public async Task<IReadOnlyList<ChuyenDenDto>> ChuyenDenAsync(CancellationToken ct)
    {
        var dangMuon = BanDangMuon();

        // 03.01.04: sách đã cho mượn, phiếu mượn chưa trả, SACH còn “Sẵn sàng”
        var daChoMuon = await (from cm in db.CtPhieuMuon.AsNoTracking()
                               join s in db.Sach on cm.MaBanSach equals s.MaBanSach
                               where s.TinhTrang == TT.SanSang && dangMuon.Contains(cm.MaBanSach)
                                     && !(from ct2 in db.CtPhieuTra join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                                          where pt.SoPMuon == cm.SoPMuon && ct2.MaBanSach == cm.MaBanSach select 1).Any()
                               select new ChuyenDenDto(cm.MaBanSach, s.TenSach, LoaiChuyenDen.DaChoMuon, cm.SoPMuon, cm.NgayMuon,
                                   s.TinhTrang, TT.DangDuocMuon, "03.01.04")).ToListAsync(ct);

        // 03.02.03: sách trả bình thường, SACH còn “Đang được mượn”, không bị cho mượn lại
        var sachTra = await (from ct2 in db.CtPhieuTra.AsNoTracking()
                             join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                             join s in db.Sach on ct2.MaBanSach equals s.MaBanSach
                             where ct2.TinhTrangTra == MienGiaTri.TinhTrangTra.BinhThuong && s.TinhTrang == TT.DangDuocMuon
                                   && !dangMuon.Contains(ct2.MaBanSach)
                             select new ChuyenDenDto(ct2.MaBanSach, s.TenSach, LoaiChuyenDen.SachTra, pt.SoPTra, pt.NgayTra,
                                 s.TinhTrang, TT.SanSang, "03.02.03")).ToListAsync(ct);

        // 03.03.02: sách mất/hư hỏng khi trả, đã lập biên bản vi phạm, SACH chưa cập nhật tương ứng
        var matHong = await (from ct2 in db.CtPhieuTra.AsNoTracking()
                             join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                             join s in db.Sach on ct2.MaBanSach equals s.MaBanSach
                             join cb in db.CtBienBan on ct2.MaBanSach equals cb.MaBanSach
                             join bb in db.BienBan on cb.SoBienBan equals bb.SoBienBan
                             where bb.SoPMuon == pt.SoPMuon
                                   && (ct2.TinhTrangTra == MienGiaTri.TinhTrangTra.Mat || ct2.TinhTrangTra == MienGiaTri.TinhTrangTra.HuHong)
                                   && s.TinhTrang != ct2.TinhTrangTra && !dangMuon.Contains(ct2.MaBanSach)
                             select new ChuyenDenDto(ct2.MaBanSach, s.TenSach, LoaiChuyenDen.MatHuHong, bb.SoBienBan,
                                 DateOnly.FromDateTime(bb.ThoiGianLap), s.TinhTrang, ct2.TinhTrangTra, "03.03.02")).ToListAsync(ct);

        return daChoMuon.Concat(sachTra).Concat(matHong.DistinctBy(x => x.MaBanSach))
            .OrderBy(x => x.Ngay).ThenBy(x => x.MaBanSach).ToList();
    }

    /// <summary>btnCapNhatTT: cập nhật SACH.tinhtrang theo dòng chuyển đến (tình trạng mới do server xác định lại).</summary>
    public async Task<SachDto> CapNhatTinhTrangAsync(string ma, string loai, Func<string, bool> coQuyen, string maNd, CancellationToken ct)
    {
        var dong = (await ChuyenDenAsync(ct)).FirstOrDefault(x => x.MaBanSach == ma && x.Loai == loai)
                   ?? throw LoiNghiepVu.XungDot("DA_XU_LY", $"Bản sách {ma} không còn trong danh sách chuyển đến (có thể đã được cập nhật).");
        var quyen = dong.TienTrinh switch { "03.01.04" => "TT108", "03.02.03" => "TT112", _ => "TT116" };
        if (!coQuyen(quyen))
            throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "KHONG_CO_QUYEN", "Tài khoản không có quyền thực hiện tiến trình " + dong.TienTrinh + ".");

        var s = await db.Sach.SingleAsync(x => x.MaBanSach == ma, ct);
        var cu = s.TinhTrang;
        s.TinhTrang = dong.TinhTrangDeXuat;
        await NhatKyGhi.ThemAsync(db, maNd,
            $"{dong.TienTrinh} Cập nhật tình trạng bản sách {ma}: {cu} → {s.TinhTrang} (theo {dong.Loai.ToLowerInvariant()} {dong.ChungTu})",
            clock.BayGio, ct);
        await db.SaveChangesAsync(ct);
        return await MotAsync(ma, ct);
    }

    private Task<SachDto> MotAsync(string ma, CancellationToken ct) =>
        (from s in db.Sach.AsNoTracking()
         join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
         where s.MaBanSach == ma
         select new SachDto { MaBanSach = s.MaBanSach, TenSach = s.TenSach, TacGia = s.TacGia, MaKhuVuc = s.MaKhuVuc, ViTri = kv.ViTri, LoaiKhuVuc = kv.LoaiKhuVuc, TinhTrang = s.TinhTrang }).SingleAsync(ct);

    private async Task<KiemTraDuLieu> KiemTraAsync(SachNhap n, bool laThem, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        if (laThem) kt.Ma("maBanSach", n.MaBanSach, 10, "Mã bản sách");
        if (string.IsNullOrEmpty(n.MaKhuVuc)) kt.Them("maKhuVuc", "Vui lòng chọn khu vực lưu trữ.");
        else if (!await db.KhuVuc.AnyAsync(x => x.MaKhuVuc == n.MaKhuVuc, ct)) kt.Them("maKhuVuc", "Khu vực không tồn tại trong danh mục khu vực.");
        kt.BatBuoc("tenSach", n.TenSach, 200, "Tên sách");
        kt.BatBuoc("tacGia", n.TacGia, 100, "Tác giả");
        if (!laThem || !string.IsNullOrEmpty(n.TinhTrang)) kt.ThuocMien("tinhTrang", n.TinhTrang, TT.TatCa, "Tình trạng");
        return kt;
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.LaTrungKhoa())
        {
            throw LoiNghiepVu.XungDot("TRUNG_MA", "Mã bản sách đã tồn tại.");
        }
    }
}
