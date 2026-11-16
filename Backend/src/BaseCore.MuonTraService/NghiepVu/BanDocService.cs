using BaseCore.Common.Loi;
using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.DTO.Common;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record BanDocDto(
    string MaBanDoc, string HoTen, string DoiTuong, string MaSvHvCb, string LopDonVi, DateOnly NgaySinh, string SoTheBd,
    string? DienThoaiEmail, DateOnly HsdTuNgay, DateOnly HsdDenNgay, string? Anh, bool TheConHan);

public sealed record BanDocNhap(
    string? MaBanDoc, string? HoTen, string? DoiTuong, string? MaSvHvCb, string? LopDonVi, DateOnly? NgaySinh, string? SoTheBd,
    string? DienThoaiEmail, DateOnly? HsdTuNgay, DateOnly? HsdDenNgay, string? Anh);

public sealed record KetQuaTimBanDoc(BanDocDto BanDoc, IReadOnlyList<SachDangMuonDto> SachDangMuon);

public sealed record DieuKienMuonDto(
    bool DuDieuKien, IReadOnlyList<string> LyDo, bool TheConHan, int SoDangMuon, int SoBanToiDa,
    IReadOnlyList<SachDangMuonDto> SachDangMuon, IReadOnlyList<ViPhamChuaThanhToanDto> ViPhamChuaThanhToan);

public sealed record ViPhamChuaThanhToanDto(string SoBienBan, DateTime ThoiGianLap, decimal TongTien);

/// <summary>SPEC 6.1.3 Danh mục Bạn đọc – tiến trình nghiệp vụ 01.01.01 Lưu thông tin Thẻ Bạn đọc.</summary>
public sealed class BanDocService(MuonTraDbContext db, IClock clock, TraCuuMuonTra traCuu, IOptions<QuyDinhOptions> quyDinh)
{
    private static readonly string[] SapXep =
        [nameof(BanDoc.MaBanDoc), nameof(BanDoc.HoTen), nameof(BanDoc.DoiTuong), nameof(BanDoc.MaSvHvCb), nameof(BanDoc.LopDonVi), nameof(BanDoc.HsdDenNgay)];

    private BanDocDto Dto(BanDoc x) => new(x.MaBanDoc, x.HoTen, x.DoiTuong, x.MaSvHvCb, x.LopDonVi, x.NgaySinh, x.SoTheBd,
        x.DienThoaiEmail, x.HsdTuNgay, x.HsdDenNgay, x.Anh, x.HsdTuNgay <= clock.HomNay && clock.HomNay <= x.HsdDenNgay);

    /// <summary>frmDanhMucBanDoc (Load) – nạp BANDOC lên dgvBanDoc.</summary>
    public async Task<KetQuaTrang<BanDocDto>> DanhSachAsync(ThamSoLuoi ts, CancellationToken ct)
    {
        var q = db.BanDoc.AsNoTracking();
        var k = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (k is not null) q = q.Where(x => x.MaBanDoc.Contains(k) || x.HoTen.Contains(k) || x.MaSvHvCb.Contains(k) || x.SoTheBd.Contains(k));
        var trang = await q.LayTrangAsync(ts, SapXep, nameof(BanDoc.MaBanDoc), x => x, ct);
        return new KetQuaTrang<BanDocDto>(trang.TongSo, trang.Trang, trang.CoTrang, trang.DuLieu.Select(Dto).ToList());
    }

    /// <summary>txtMaSV, btnTimKiem: tìm theo ma_svhvcb; thấy → thông tin + sách đang mượn; không thấy → 404 (giao diện mở chế độ thêm).</summary>
    public async Task<KetQuaTimBanDoc> TimTheoMaSvAsync(string maSv, CancellationToken ct)
    {
        maSv = (maSv ?? string.Empty).Trim();
        var bd = await db.BanDoc.AsNoTracking().SingleOrDefaultAsync(x => x.MaSvHvCb == maSv, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BAN_DOC", $"Không tìm thấy Bạn đọc có mã SV/HV/CB “{maSv}”.");
        return new KetQuaTimBanDoc(Dto(bd), await traCuu.SachDangMuonAsync(bd.MaBanDoc, clock.HomNay, ct));
    }

    /// <summary>
    /// btnKiemTraDK: hsd_denngay ≥ ngày hiện tại; sách đang mượn, sách quá hạn; vi phạm chưa thanh toán; số bản tối đa (SPEC mục 4).
    /// </summary>
    public async Task<DieuKienMuonDto> KiemTraDieuKienAsync(string maBanDoc, CancellationToken ct)
    {
        var bd = await db.BanDoc.AsNoTracking().SingleOrDefaultAsync(x => x.MaBanDoc == maBanDoc, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BAN_DOC", $"Không tìm thấy Bạn đọc {maBanDoc}.");
        var homNay = clock.HomNay;
        var dangMuon = await traCuu.SachDangMuonAsync(maBanDoc, homNay, ct);
        var viPham = await db.BienBan.AsNoTracking()
            .Where(b => b.MaBanDoc == maBanDoc && b.HtThanhToan == MienGiaTri.HinhThucThanhToan.ChuaThanhToan)
            .OrderBy(b => b.ThoiGianLap).Select(b => new ViPhamChuaThanhToanDto(b.SoBienBan, b.ThoiGianLap, b.TongTien)).ToListAsync(ct);
        var toiDa = quyDinh.Value.Cua(bd.DoiTuong).SoBanToiDa;

        var lyDo = new List<string>();
        var conHan = bd.HsdTuNgay <= homNay && homNay <= bd.HsdDenNgay;
        if (!conHan)
            lyDo.Add(bd.HsdDenNgay < homNay ? $"Thẻ bạn đọc đã hết hạn từ {bd.HsdDenNgay:dd/MM/yyyy}." : $"Thẻ bạn đọc chỉ có hiệu lực từ {bd.HsdTuNgay:dd/MM/yyyy}.");
        var quaHan = dangMuon.Where(x => x.SoNgayQuaHan > 0).ToList();
        if (quaHan.Count > 0) lyDo.Add($"Còn {quaHan.Count} sách mượn quá hạn chưa trả.");
        if (viPham.Count > 0) lyDo.Add($"Còn {viPham.Count} biên bản vi phạm chưa thanh toán ({viPham.Sum(v => v.TongTien):N0} đ).");
        if (dangMuon.Count >= toiDa) lyDo.Add($"Đã mượn đủ số bản tối đa ({toiDa} bản đối với {bd.DoiTuong.ToLowerInvariant()}).");

        return new DieuKienMuonDto(lyDo.Count == 0, lyDo, conHan, dangMuon.Count, toiDa, dangMuon, viPham);
    }

    /// <summary>
    /// btnLuu (thêm): lưu Bạn đọc mới; tạo TAIKHOAN_BANDOC “Chưa kích hoạt” trong cùng transaction (file 07 mục 1, GIA_DINH D12).
    /// </summary>
    public async Task<BanDocDto> ThemAsync(BanDocNhap n, string maNd, CancellationToken ct)
    {
        var kt = KiemTra(n, true);
        if (await db.BanDoc.AnyAsync(x => x.MaBanDoc == n.MaBanDoc, ct)) kt.Them("maBanDoc", $"Mã bạn đọc {n.MaBanDoc} đã tồn tại.");
        await KiemTraTrungAsync(kt, n, null, ct);
        kt.NemNeuCoLoi();

        var bd = new BanDoc();
        Gan(bd, n);
        bd.MaBanDoc = n.MaBanDoc!.Trim();
        var bayGio = clock.BayGio;
        await db.ChayAsync(async c =>
        {
            db.BanDoc.Add(bd);
            db.TaiKhoanBanDoc.Add(new TaiKhoanBanDoc
            {
                MaBanDoc = bd.MaBanDoc, TinhTrang = MienGiaTri.TinhTrangTaiKhoanBanDoc.ChuaKichHoat, TgTao = bayGio
            });
            await NhatKyGhi.ThemAsync(db, maNd, $"01.01.01 Lưu thông tin Thẻ Bạn đọc mới {bd.MaBanDoc} – {bd.HoTen} ({bd.MaSvHvCb}); tạo tài khoản cổng chờ kích hoạt", bayGio, c);
            await LuuAsync(c);
        }, ct);
        return Dto(bd);
    }

    /// <summary>btnSua → btnLuu: cập nhật các trường đã sửa (trừ ma_bandoc).</summary>
    public async Task<BanDocDto> SuaAsync(string ma, BanDocNhap n, string maNd, CancellationToken ct)
    {
        var bd = await db.BanDoc.SingleOrDefaultAsync(x => x.MaBanDoc == ma, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy Bạn đọc {ma}.");
        var kt = KiemTra(n, false);
        await KiemTraTrungAsync(kt, n, ma, ct);
        kt.NemNeuCoLoi();
        Gan(bd, n);
        await NhatKyGhi.ThemAsync(db, maNd, $"Sửa thông tin Bạn đọc {ma} – {bd.HoTen} ({bd.MaSvHvCb}), hạn thẻ {bd.HsdTuNgay:dd/MM/yyyy}–{bd.HsdDenNgay:dd/MM/yyyy}", clock.BayGio, ct);
        await LuuAsync(ct);
        return Dto(bd);
    }

    /// <summary>btnXoa: Bạn đọc đã có phiếu mượn thì không cho xóa.</summary>
    public async Task XoaAsync(string ma, string maNd, CancellationToken ct)
    {
        var bd = await db.BanDoc.SingleOrDefaultAsync(x => x.MaBanDoc == ma, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy Bạn đọc {ma}.");
        // GIA_DINH D41: chỉ xóa (kèm dữ liệu mở rộng) khi bạn đọc chưa phát sinh chứng từ nào
        var chungTu = new List<string>();
        if (await db.PhieuMuon.AnyAsync(x => x.MaBanDoc == ma, ct)) chungTu.Add("phiếu mượn");
        if (await db.PhieuGiaHan.AnyAsync(x => x.MaBanDoc == ma, ct)) chungTu.Add("phiếu gia hạn");
        if (await db.PhieuTra.AnyAsync(x => x.MaBanDoc == ma, ct)) chungTu.Add("phiếu trả");
        if (await db.BienBan.AnyAsync(x => x.MaBanDoc == ma, ct)) chungTu.Add("biên bản vi phạm");
        if (chungTu.Count > 0)
            throw LoiNghiepVu.XungDot("DA_CO_CHUNG_TU",
                $"Không thể xóa Bạn đọc {ma} vì đã có {string.Join(", ", chungTu)}. Chứng từ phải được lưu giữ; hãy cập nhật thông tin thay vì xóa.");
        if (await db.PhieuDatCho.AnyAsync(x => x.MaBanDoc == ma, ct))
            throw LoiNghiepVu.XungDot("DA_CO_DAT_CHO", $"Bạn đọc {ma} đã phát sinh đặt chỗ nên không thể xóa.");

        var moTa = $"{bd.HoTen} ({bd.MaSvHvCb})";
        await db.ChayAsync(async c =>
        {
            // Dữ liệu mở rộng cùng service gắn với bạn đọc
            await db.ThongBao.Where(x => x.MaBanDoc == ma).ExecuteDeleteAsync(c);
            await db.YeuThich.Where(x => x.MaBanDoc == ma).ExecuteDeleteAsync(c);
            await db.TaiKhoanBanDoc.Where(x => x.MaBanDoc == ma).ExecuteDeleteAsync(c);
            await db.BanDoc.Where(x => x.MaBanDoc == ma).ExecuteDeleteAsync(c);
            await NhatKyGhi.ThemAsync(db, maNd, $"Xóa Bạn đọc {ma} – {moTa}", clock.BayGio, c);
            await db.SaveChangesAsync(c);
        }, ct);
    }

    /// <summary>Gợi ý mã bạn đọc kế tiếp (BD####) khi bấm btnThem.</summary>
    public async Task<string> MaKeTiepAsync(CancellationToken ct)
    {
        var ds = await db.BanDoc.AsNoTracking().Where(x => x.MaBanDoc.StartsWith("BD")).Select(x => x.MaBanDoc).ToListAsync(ct);
        var max = ds.Select(m => int.TryParse(m[2..], out var so) ? so : 0).DefaultIfEmpty(0).Max();
        return $"BD{max + 1:D4}";
    }

    private static void Gan(BanDoc bd, BanDocNhap n)
    {
        bd.HoTen = n.HoTen!.Trim();
        bd.DoiTuong = n.DoiTuong!;
        bd.MaSvHvCb = n.MaSvHvCb!.Trim();
        bd.LopDonVi = n.LopDonVi!.Trim();
        bd.NgaySinh = n.NgaySinh!.Value;
        bd.SoTheBd = n.SoTheBd!.Trim();
        bd.DienThoaiEmail = KiemTraDuLieu.Gon(n.DienThoaiEmail);
        bd.HsdTuNgay = n.HsdTuNgay!.Value;
        bd.HsdDenNgay = n.HsdDenNgay!.Value;
        bd.Anh = KiemTraDuLieu.Gon(n.Anh);
    }

    /// <summary>Kiểm soát dữ liệu nhập theo “Điều kiện toàn vẹn” SPEC 6.1.3 (TT005).</summary>
    private KiemTraDuLieu KiemTra(BanDocNhap n, bool laThem)
    {
        var kt = new KiemTraDuLieu();
        if (laThem) kt.Ma("maBanDoc", n.MaBanDoc, 10, "Mã bạn đọc");
        kt.BatBuoc("hoTen", n.HoTen, 50, "Họ và tên");
        kt.ThuocMien("doiTuong", n.DoiTuong, MienGiaTri.DoiTuong.TatCa, "Đối tượng");
        kt.Ma("maSvHvCb", n.MaSvHvCb, 15, "Mã SV/HV/CB");
        kt.BatBuoc("lopDonVi", n.LopDonVi, 50, "Lớp/Đơn vị");
        if (n.NgaySinh is null) kt.Them("ngaySinh", "Vui lòng chọn ngày sinh.");
        else kt.Dung(n.NgaySinh < clock.HomNay, "ngaySinh", "Ngày sinh phải nhỏ hơn ngày hiện tại.");
        kt.Ma("soTheBd", n.SoTheBd, 15, "Số thẻ bạn đọc");
        kt.ToiDa("dienThoaiEmail", n.DienThoaiEmail, 50, "Điện thoại/Email");
        if (n.HsdTuNgay is null) kt.Them("hsdTuNgay", "Vui lòng chọn ngày bắt đầu hạn sử dụng.");
        if (n.HsdDenNgay is null) kt.Them("hsdDenNgay", "Vui lòng chọn ngày hết hạn sử dụng.");
        else if (n.HsdTuNgay is not null) kt.Dung(n.HsdDenNgay > n.HsdTuNgay, "hsdDenNgay", "Hạn sử dụng đến ngày phải lớn hơn từ ngày.");
        kt.ToiDa("anh", n.Anh, 100, "Đường dẫn ảnh");
        return kt;
    }

    private async Task KiemTraTrungAsync(KiemTraDuLieu kt, BanDocNhap n, string? boQuaMa, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(n.MaSvHvCb) &&
            await db.BanDoc.AnyAsync(x => x.MaSvHvCb == n.MaSvHvCb.Trim() && x.MaBanDoc != boQuaMa, ct))
            kt.Them("maSvHvCb", $"Mã SV/HV/CB {n.MaSvHvCb} đã thuộc Bạn đọc khác.");
        if (!string.IsNullOrWhiteSpace(n.SoTheBd) &&
            await db.BanDoc.AnyAsync(x => x.SoTheBd == n.SoTheBd.Trim() && x.MaBanDoc != boQuaMa, ct))
            kt.Them("soTheBd", $"Số thẻ {n.SoTheBd} đã được cấp cho Bạn đọc khác.");
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.LaTrungKhoa())
        {
            throw LoiNghiepVu.XungDot("TRUNG_MA", "Mã bạn đọc, mã SV/HV/CB hoặc số thẻ đã tồn tại.");
        }
    }
}
