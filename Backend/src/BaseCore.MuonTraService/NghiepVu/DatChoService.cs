using BaseCore.Common.Loi;
using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TTDC = BaseCore.Entities.MienGiaTri.TrangThaiDatCho;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record DatChoDto(string MaDatCho, string MaDauSach, string TenSach, string? TacGia, string? AnhBia, string MaBanSach,
    string MaKhuVuc, string ViTri, string LoaiKhuVuc, DateTime TgDat, DateTime TgHetHan, string TrangThai, DateTime? TgXuLy,
    string? SoPMuon, string? LyDoHuy, string QuayNhan);

public sealed record DatChoQuayDto(string MaDatCho, string MaBanDoc, string HoTen, string MaSvHvCb, string MaDauSach, string TenSach,
    string MaBanSach, string MaKhuVuc, string ViTri, string LoaiKhuVuc, DateTime TgDat, DateTime TgHetHan, string TrangThai,
    DateTime? TgXuLy, string? SoPMuon, string? LyDoHuy, string? MaNdXuLy);

/// <summary>Một điều kiện đặt chỗ không đạt (file 06 mục 2): mã lỗi riêng, câu thông báo và hướng xử lý.</summary>
public sealed record DieuKienKhongDat(string Ma, string ThongBao, string HuongXuLy, int MaHttp = StatusCodes.Status409Conflict);

/// <summary>Trạng thái đặt chỗ của bạn đọc đang đăng nhập đối với một đầu sách – phần “đặt chỗ của tôi” của Book Preview.</summary>
public sealed record TrangThaiDauSachCuaToi(DatChoDto? DatCho, bool YeuThich, DieuKienKhongDat? KhongDat);

public sealed record KetQuaDatCho(DatChoDto DatCho, string ThongBao);

/// <summary>
/// Nghiệp vụ đặt chỗ 24 giờ (file 06 mục 2–3, 5–7). Mọi ghi trong MỘT transaction của muontra: PHIEUDATCHO + THONGBAO
/// (+ NHATKY_HOATDONG khi nhân viên thao tác). Không đổi SACH.tinhtrang (giữ nguyên 18 bảng gốc).
/// </summary>
public sealed class DatChoService(MuonTraDbContext db, IClock clock, ChungTuChung chung, BanDocService banDoc,
    IOptions<DatChoOptions> tuyChon, ILogger<DatChoService> logger)
{
    private DatChoOptions Opt => tuyChon.Value;

    // ───────────────────────────── Điều kiện (file 06 mục 2) ─────────────────────────────

    /// <summary>Kiểm tra điều kiện 1–5 (và 6 nếu có <paramref name="maDauSach"/>); trả điều kiện đầu tiên không đạt, hoặc null.</summary>
    public async Task<DieuKienKhongDat?> KiemTraDieuKienAsync(string maBanDoc, string? maDauSach, CancellationToken ct)
    {
        var bayGio = clock.BayGio;
        // 1. Tài khoản bạn đọc “Hoạt động”
        var tk = await db.TaiKhoanBanDoc.AsNoTracking().SingleOrDefaultAsync(x => x.MaBanDoc == maBanDoc, ct);
        if (tk is null || tk.TinhTrang != MienGiaTri.TinhTrangTaiKhoanBanDoc.HoatDong)
            return new("TAI_KHOAN_KHONG_HOAT_DONG", "Tài khoản cổng bạn đọc không ở trạng thái “Hoạt động”.",
                "Vui lòng kích hoạt tài khoản hoặc liên hệ quầy mượn trả.", StatusCodes.Status403Forbidden);

        // 2–4. Dùng lại kiểm tra điều kiện mượn của 01.01.01 (thẻ còn hạn, quá hạn, vi phạm chưa thanh toán)
        var dk = await banDoc.KiemTraDieuKienAsync(maBanDoc, ct);
        if (!dk.TheConHan)
            return new("THE_HET_HAN", dk.LyDo.First(l => l.StartsWith("Thẻ", StringComparison.Ordinal)),
                "Vui lòng gia hạn thẻ bạn đọc tại quầy mượn trả trước khi đặt chỗ.");
        var quaHan = dk.SachDangMuon.Count(x => x.SoNgayQuaHan > 0);
        if (quaHan > 0)
            return new("CO_SACH_QUA_HAN", $"Bạn đang có {quaHan} sách quá hạn chưa trả.",
                "Vui lòng trả sách quá hạn tại quầy mượn trả trước khi đặt chỗ.");
        if (dk.ViPhamChuaThanhToan.Count > 0)
            return new("CO_VI_PHAM_CHUA_THANH_TOAN", $"Bạn còn {dk.ViPhamChuaThanhToan.Count} biên bản vi phạm chưa thanh toán.",
                "Vui lòng thanh toán khoản vi phạm tại quầy mượn trả trước khi đặt chỗ.");

        var dangGiu = await chung.DatChoConGiu(bayGio).Where(d => d.MaBanDoc == maBanDoc)
            .Select(d => d.MaDauSach).ToListAsync(ct);
        // 4. đang mượn + đang giữ < tối đa theo đối tượng (SPEC mục 4)
        if (dk.SoDangMuon + dangGiu.Count >= dk.SoBanToiDa)
            return new("VUOT_SO_BAN_TOI_DA",
                $"Bạn đang mượn {dk.SoDangMuon} bản và giữ chỗ {dangGiu.Count} bản – đã đủ số bản tối đa ({dk.SoBanToiDa} bản).",
                "Vui lòng trả bớt sách hoặc hủy một đặt chỗ trước khi đặt thêm.");
        // 5. số đặt chỗ đang giữ < ToiDaDangGiu; chưa giữ bản khác của cùng đầu sách
        if (maDauSach is not null && dangGiu.Contains(maDauSach))
            return new("DA_GIU_DAU_SACH", "Bạn đang giữ chỗ một bản của đầu sách này.",
                "Xem vé giữ chỗ ở mục Tài khoản → Đặt chỗ.");
        if (dangGiu.Count >= Opt.ToiDaDangGiu)
            return new("VUOT_SO_DAT_CHO", $"Bạn đang giữ chỗ {dangGiu.Count} sách – tối đa {Opt.ToiDaDangGiu} đặt chỗ cùng lúc.",
                "Vui lòng nhận sách tại quầy hoặc hủy một đặt chỗ trước khi đặt thêm.");

        // 6. Đầu sách còn bản có thể đặt
        if (maDauSach is not null)
        {
            if (!await db.DauSach.AnyAsync(x => x.MaDauSach == maDauSach, ct))
                return new("KHONG_TIM_THAY_DAU_SACH", "Không tìm thấy đầu sách.", "Vui lòng chọn sách khác.", StatusCodes.Status404NotFound);
            if (!await BanCoTheDat(maDauSach, bayGio).AnyAsync(ct))
                return new("HET_BAN_CO_THE_DAT", "Đầu sách hiện không còn bản nào có thể đặt chỗ.",
                    "Vui lòng quay lại sau hoặc chọn sách khác.");
        }
        return null;
    }

    /// <summary>Bản sách của đầu sách có thể đặt: “Sẵn sàng”, không đang mượn, không chờ kho xử lý (D47), không đang được giữ.</summary>
    private IQueryable<BanUngVien> BanCoTheDat(string maDauSach, DateTime bayGio)
    {
        var dangMuon = chung.BanDangMuon();
        var choKho = chung.BanChoKhoXuLy();
        var dangGiu = chung.DatChoConGiu(bayGio).Select(d => d.MaBanSach);
        return from b in db.DauSachBanSach
               join s in db.Sach on b.MaBanSach equals s.MaBanSach
               join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
               where b.MaDauSach == maDauSach && s.TinhTrang == MienGiaTri.TinhTrangSach.SanSang
                     && !dangMuon.Contains(s.MaBanSach) && !choKho.Contains(s.MaBanSach) && !dangGiu.Contains(s.MaBanSach)
               // Ưu tiên khu vực Kệ mở, rồi mã nhỏ nhất (file 06 mục 2)
               orderby kv.LoaiKhuVuc == MienGiaTri.LoaiKhuVuc.KeMo ? 0 : 1, s.MaBanSach
               select new BanUngVien { MaBanSach = s.MaBanSach, MaKhuVuc = s.MaKhuVuc };
    }

    // Lớp có thuộc tính init (không dùng record khởi tạo theo tham số) để EF Core ghép thêm Where/Any phía sau phép chiếu
    private sealed class BanUngVien
    {
        public string MaBanSach { get; init; } = null!;
        public string MaKhuVuc { get; init; } = null!;
    }

    // ───────────────────────────── Bạn đọc ─────────────────────────────

    /// <summary>POST /api/cong/dat-cho: đặt chỗ – một transaction, khóa BANDOC → DAUSACH → SACH, chỉ mục lọc chặn trùng.</summary>
    public async Task<KetQuaDatCho> DatAsync(string maBanDoc, string? maDauSach, CancellationToken ct)
    {
        maDauSach = (maDauSach ?? string.Empty).Trim().ToUpperInvariant();
        if (maDauSach.Length == 0) new KiemTraDuLieu().Them("maDauSach", "Vui lòng chọn đầu sách.").NemNeuCoLoi();

        string maDatCho;
        try
        {
            maDatCho = await db.ChayAsync(async c =>
            {
                await chung.KhoaAsync(["ULMS:BANDOC:" + maBanDoc, "ULMS:DAUSACH:" + maDauSach], c);
                var bayGio = clock.BayGio;
                // Đặt chỗ quá hạn mà tác vụ nền chưa quét: chuyển “Hết hạn” ngay để giải phóng chỉ mục lọc “Đang giữ”
                if (await HetHanAsync(db.PhieuDatCho.Where(d => d.MaDauSach == maDauSach || d.MaBanDoc == maBanDoc), bayGio, c) > 0)
                    await db.SaveChangesAsync(c); // lưu trước khi INSERT để chỉ mục lọc không còn thấy dòng cũ

                var loi = await KiemTraDieuKienAsync(maBanDoc, maDauSach, c);
                if (loi is not null) throw Nem(loi);

                var ungVien = await BanCoTheDat(maDauSach, bayGio).Take(5).ToListAsync(c);
                foreach (var ban in ungVien)
                {
                    // Lập Phiếu mượn cũng khóa theo bản sách → không cho mượn và đặt chỗ cùng một bản cùng lúc
                    await chung.KhoaBanSachAsync([ban.MaBanSach], c);
                    if (!await BanCoTheDat(maDauSach, bayGio).AnyAsync(x => x.MaBanSach == ban.MaBanSach, c)) continue;

                    var ngay = DateOnly.FromDateTime(bayGio);
                    var ma = VongDoiDatCho.TaoMa(ngay, await SoChungTuGhi.CapAsync(db, VongDoiDatCho.LoaiSoChungTu, ngay, c));
                    var dc = new PhieuDatCho
                    {
                        MaDatCho = ma, MaBanDoc = maBanDoc, MaDauSach = maDauSach, MaBanSach = ban.MaBanSach,
                        TgDat = bayGio, TgHetHan = VongDoiDatCho.HetHan(bayGio, Opt.SoGioGiu), TrangThai = TTDC.DangGiu
                    };
                    db.PhieuDatCho.Add(dc);
                    var ten = await db.DauSach.Where(x => x.MaDauSach == maDauSach).Select(x => x.TenSach).SingleAsync(c);
                    ThemThongBao(maBanDoc, "Đặt chỗ thành công",
                        $"Đã giữ chỗ “{ten}” (mã {ma}). Vui lòng nhận sách tại {Opt.QuayNhan} trước {dc.TgHetHan:HH:mm dd/MM/yyyy}.",
                        LienKetDatCho(ma), bayGio);
                    await db.SaveChangesAsync(c);
                    return ma;
                }
                throw LoiNghiepVu.XungDot("VUA_DUOC_DAT", "Sách vừa được người khác đặt. Vui lòng thử lại sau.");
            }, ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Chỉ mục lọc UX_DATCHO_* chặn trùng (lưới an toàn cuối cùng)
            throw LoiNghiepVu.XungDot("VUA_DUOC_DAT", "Sách vừa được người khác đặt. Vui lòng thử lại sau.");
        }

        logger.LogInformation("Bạn đọc {MaBanDoc} đặt chỗ {MaDatCho} đầu sách {MaDauSach}", maBanDoc, maDatCho, maDauSach);
        var dto = (await DanhSachCuaToiAsync(maBanDoc, ct)).First(x => x.MaDatCho == maDatCho);
        return new KetQuaDatCho(dto, $"Đặt chỗ thành công. Sách được giữ đến {dto.TgHetHan:HH:mm dd/MM/yyyy}.");
    }

    /// <summary>DELETE /api/cong/dat-cho/{ma}: bạn đọc hủy đặt chỗ đang giữ của chính mình.</summary>
    public Task<DatChoDto> HuyCuaBanDocAsync(string maBanDoc, string maDatCho, CancellationToken ct) =>
        HuyAsync(maDatCho, maBanDoc, null, "Bạn đọc hủy trên cổng", ct);

    /// <summary>GET /api/cong/dat-cho/cua-toi: mọi đặt chỗ của bạn đọc, “Đang giữ” lên đầu theo hạn gần nhất.</summary>
    public async Task<List<DatChoDto>> DanhSachCuaToiAsync(string maBanDoc, CancellationToken ct)
    {
        var bayGio = clock.BayGio;
        var ds = await TruyVan().Where(x => x.d.MaBanDoc == maBanDoc).ToListAsync(ct);
        return ds.Select(x => Dto(x.d, x.ds, x.kv, bayGio))
            .OrderBy(x => x.TrangThai == TTDC.DangGiu ? 0 : 1)
            .ThenBy(x => x.TrangThai == TTDC.DangGiu ? x.TgHetHan : DateTime.MaxValue)
            .ThenByDescending(x => x.TgDat).ToList();
    }

    /// <summary>Phần “đặt chỗ của tôi” của Book Preview: đặt chỗ đang giữ, yêu thích, lý do chưa đặt được.</summary>
    public async Task<TrangThaiDauSachCuaToi> TrangThaiDauSachAsync(string maBanDoc, string maDauSach, CancellationToken ct)
    {
        var bayGio = clock.BayGio;
        var giu = await TruyVan().Where(x => x.d.MaBanDoc == maBanDoc && x.d.MaDauSach == maDauSach
                                              && x.d.TrangThai == TTDC.DangGiu && x.d.TgHetHan > bayGio).FirstOrDefaultAsync(ct);
        var yt = await db.YeuThich.AnyAsync(x => x.MaBanDoc == maBanDoc && x.MaDauSach == maDauSach, ct);
        var loi = giu is null ? await KiemTraDieuKienAsync(maBanDoc, maDauSach, ct) : null;
        return new TrangThaiDauSachCuaToi(giu is null ? null : Dto(giu.d, giu.ds, giu.kv, bayGio), yt, loi);
    }

    // ───────────────────────────── Nhân viên – Quầy đặt chỗ (file 06 mục 5) ─────────────────────────────

    /// <summary>GET /api/quan-tri/dat-cho: lọc trạng thái, khoảng ngày đặt, mã SV/HV/CB; “Đang giữ” sắp theo hạn gần nhất.</summary>
    public async Task<List<DatChoQuayDto>> DanhSachQuayAsync(string? trangThai, DateOnly? tuNgay, DateOnly? denNgay, string? maSv, CancellationToken ct)
    {
        var bayGio = clock.BayGio;
        var q = from x in TruyVan()
                join bd in db.BanDoc on x.d.MaBanDoc equals bd.MaBanDoc
                select new { x.d, x.ds, x.kv, bd };
        trangThai = string.IsNullOrWhiteSpace(trangThai) ? TTDC.DangGiu : trangThai.Trim();
        if (trangThai == TTDC.DangGiu) q = q.Where(x => x.d.TrangThai == TTDC.DangGiu && x.d.TgHetHan > bayGio);
        else if (trangThai == TTDC.HetHan) q = q.Where(x => x.d.TrangThai == TTDC.HetHan || (x.d.TrangThai == TTDC.DangGiu && x.d.TgHetHan <= bayGio));
        else if (trangThai != "Tất cả")
        {
            if (!TTDC.TatCa.Contains(trangThai)) new KiemTraDuLieu().Them("trangThai", "Trạng thái đặt chỗ không hợp lệ.").NemNeuCoLoi();
            q = q.Where(x => x.d.TrangThai == trangThai);
        }
        if (tuNgay is { } tu) { var t = tu.ToDateTime(TimeOnly.MinValue); q = q.Where(x => x.d.TgDat >= t); }
        if (denNgay is { } den) { var d = den.AddDays(1).ToDateTime(TimeOnly.MinValue); q = q.Where(x => x.d.TgDat < d); }
        if (!string.IsNullOrWhiteSpace(maSv)) { var m = maSv.Trim(); q = q.Where(x => x.bd.MaSvHvCb.Contains(m)); }

        var ds = await q.ToListAsync(ct);
        return ds.Select(x => new DatChoQuayDto(x.d.MaDatCho, x.bd.MaBanDoc, x.bd.HoTen, x.bd.MaSvHvCb, x.d.MaDauSach, x.ds.TenSach,
                x.d.MaBanSach, x.kv.MaKhuVuc, x.kv.ViTri, x.kv.LoaiKhuVuc, x.d.TgDat, x.d.TgHetHan,
                VongDoiDatCho.TrangThaiHieuLuc(x.d.TrangThai, x.d.TgHetHan, bayGio), x.d.TgXuLy, x.d.SoPMuon, x.d.LyDoHuy, x.d.MaNdXuLy))
            .OrderBy(x => x.TrangThai == TTDC.DangGiu ? x.TgHetHan : DateTime.MaxValue).ThenByDescending(x => x.TgDat).ToList();
    }

    /// <summary>Chuông topbar dashboard: số đặt chỗ đang giữ còn ≤ NhacTruocGio giờ.</summary>
    public async Task<object> TomTatQuayAsync(CancellationToken ct)
    {
        var bayGio = clock.BayGio;
        var moc = bayGio.AddHours(Opt.NhacTruocGio);
        var giu = chung.DatChoConGiu(bayGio);
        return new { dangGiu = await giu.CountAsync(ct), sapHetHan = await giu.CountAsync(x => x.TgHetHan <= moc, ct), nhacTruocGio = Opt.NhacTruocGio };
    }

    /// <summary>POST /api/quan-tri/dat-cho/{ma}/huy: nhân viên hủy, bắt buộc lý do; ghi nhật ký.</summary>
    public Task<DatChoDto> HuyCuaNhanVienAsync(string maDatCho, string? lyDo, string maNd, CancellationToken ct)
    {
        lyDo = KiemTraDuLieu.Gon(lyDo);
        var kt = new KiemTraDuLieu();
        kt.BatBuoc("lyDo", lyDo, 200, "Lý do hủy");
        kt.NemNeuCoLoi();
        return HuyAsync(maDatCho, null, maNd, lyDo!, ct);
    }

    /// <summary>
    /// Đặt chỗ đang giữ của bạn đọc (dgvDatCho ở 01.01.03 – mở rộng). Dùng khi nhân viên tìm Bạn đọc ở Lập Phiếu mượn.
    /// </summary>
    public async Task<List<DatChoQuayDto>> DangGiuCuaBanDocAsync(string maBanDoc, CancellationToken ct) =>
        (await DanhSachQuayAsync(TTDC.DangGiu, null, null, null, ct)).Where(x => x.MaBanDoc == maBanDoc).ToList();

    // ───────────────────────────── Dùng chung ─────────────────────────────

    private async Task<DatChoDto> HuyAsync(string maDatCho, string? maBanDoc, string? maNd, string lyDo, CancellationToken ct)
    {
        maDatCho = (maDatCho ?? string.Empty).Trim().ToUpperInvariant();
        await db.ChayAsync(async c =>
        {
            var bayGio = clock.BayGio;
            var dc = await db.PhieuDatCho.FromSqlInterpolated(
                    $"SELECT * FROM [muontra].[PHIEUDATCHO] WITH (UPDLOCK, ROWLOCK) WHERE [ma_datcho] = {maDatCho}")
                .SingleOrDefaultAsync(c);
            if (dc is null || (maBanDoc is not null && dc.MaBanDoc != maBanDoc))
                throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_DAT_CHO", $"Không tìm thấy đặt chỗ {maDatCho}.");
            var hieuLuc = VongDoiDatCho.TrangThaiHieuLuc(dc.TrangThai, dc.TgHetHan, bayGio);
            if (!VongDoiDatCho.ChuyenHopLe(hieuLuc, TTDC.DaHuy))
                throw LoiNghiepVu.XungDot("DAT_CHO_KHONG_CON_GIU", $"Đặt chỗ {maDatCho} đang ở trạng thái “{hieuLuc}”, không hủy được.");

            dc.TrangThai = TTDC.DaHuy;
            dc.TgXuLy = bayGio;
            dc.LyDoHuy = lyDo;
            dc.MaNdXuLy = maNd;
            var ten = await db.DauSach.Where(x => x.MaDauSach == dc.MaDauSach).Select(x => x.TenSach).SingleAsync(c);
            if (maNd is not null)
            {
                // Loại THONGBAO “Đặt chỗ bị hủy” (bổ sung X07 – GIA_DINH D63) – chỉ khi nhân viên hủy
                ThemThongBao(dc.MaBanDoc, "Đặt chỗ bị hủy", $"Đặt chỗ “{ten}” (mã {dc.MaDatCho}) đã bị thư viện hủy. Lý do: {lyDo}.",
                    LienKetDatCho(dc.MaDatCho), bayGio);
                await NhatKyGhi.ThemAsync(db, maNd, $"Quầy đặt chỗ – hủy đặt chỗ {dc.MaDatCho} ({ten}, bản {dc.MaBanSach}) của Bạn đọc {dc.MaBanDoc}. Lý do: {lyDo}", bayGio, c);
            }
            await db.SaveChangesAsync(c);
        }, ct);
        var bayGioSau = clock.BayGio;
        var x = await TruyVan().SingleAsync(x => x.d.MaDatCho == maDatCho, ct);
        return Dto(x.d, x.ds, x.kv, bayGioSau);
    }

    /// <summary>
    /// Chuyển “Đang giữ” đã quá hạn (trong <paramref name="pham"/>) sang “Hết hạn” + THONGBAO “Hết hạn giữ”.
    /// Dùng bởi tác vụ nền <see cref="DatChoNenService"/> và trước khi đặt chỗ mới. Chưa SaveChanges.
    /// </summary>
    public async Task<int> HetHanAsync(IQueryable<PhieuDatCho> pham, DateTime bayGio, CancellationToken ct)
    {
        var het = await (from d in pham
                         join ds in db.DauSach on d.MaDauSach equals ds.MaDauSach
                         where d.TrangThai == TTDC.DangGiu && d.TgHetHan <= bayGio
                         select new { d, ds.TenSach }).ToListAsync(ct);
        foreach (var x in het)
        {
            x.d.TrangThai = TTDC.HetHan;
            x.d.TgXuLy = bayGio;
            ThemThongBao(x.d.MaBanDoc, "Hết hạn giữ",
                $"Đặt chỗ “{x.TenSach}” (mã {x.d.MaDatCho}) đã hết hạn giữ lúc {x.d.TgHetHan:HH:mm dd/MM/yyyy}. Bản sách đã trở lại kệ.",
                LienKetDatCho(x.d.MaDatCho), bayGio);
        }
        return het.Count;
    }

    /// <summary>Thông báo “Sắp hết hạn giữ” – một lần cho mỗi đặt chỗ (khóa theo liên kết). Chưa SaveChanges.</summary>
    public async Task<int> NhacSapHetHanAsync(DateTime bayGio, CancellationToken ct)
    {
        var moc = bayGio.AddHours(Opt.NhacTruocGio);
        var ds = await (from d in db.PhieuDatCho
                        join ds2 in db.DauSach on d.MaDauSach equals ds2.MaDauSach
                        where d.TrangThai == TTDC.DangGiu && d.TgHetHan > bayGio && d.TgHetHan <= moc
                        select new { d.MaDatCho, d.MaBanDoc, d.TgHetHan, ds2.TenSach }).ToListAsync(ct);
        var n = 0;
        foreach (var x in ds)
        {
            var lk = LienKetDatCho(x.MaDatCho);
            if (await db.ThongBao.AnyAsync(t => t.MaBanDoc == x.MaBanDoc && t.Loai == "Sắp hết hạn giữ" && t.LienKet == lk, ct)) continue;
            ThemThongBao(x.MaBanDoc, "Sắp hết hạn giữ",
                $"Đặt chỗ “{x.TenSach}” (mã {x.MaDatCho}) sẽ hết hạn lúc {x.TgHetHan:HH:mm dd/MM/yyyy}. Vui lòng đến {Opt.QuayNhan} để nhận sách.",
                lk, bayGio);
            n++;
        }
        return n;
    }

    public static string LienKetDatCho(string maDatCho) => "/tai-khoan/dat-cho?ma=" + maDatCho;

    private void ThemThongBao(string maBanDoc, string loai, string noiDung, string? lienKet, DateTime bayGio) =>
        db.ThongBao.Add(new ThongBao
        {
            MaBanDoc = maBanDoc, Loai = loai, NoiDung = noiDung.Length > 500 ? noiDung[..500] : noiDung,
            LienKet = lienKet, TgTao = bayGio, DaDoc = false
        });

    private static LoiNghiepVu Nem(DieuKienKhongDat l) =>
        new(l.MaHttp, l.Ma, l.ThongBao, new Dictionary<string, string> { ["huongXuLy"] = l.HuongXuLy });

    private IQueryable<DatChoDong> TruyVan() =>
        from d in db.PhieuDatCho.AsNoTracking()
        join ds in db.DauSach on d.MaDauSach equals ds.MaDauSach
        join s in db.Sach on d.MaBanSach equals s.MaBanSach
        join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
        select new DatChoDong { d = d, ds = ds, kv = kv };

    private sealed class DatChoDong
    {
#pragma warning disable IDE1006 // tên ngắn cho truy vấn
        public PhieuDatCho d { get; init; } = null!;
        public BaseCore.Entities.KhoSach.DauSach ds { get; init; } = null!;
        public BaseCore.Entities.KhoSach.KhuVuc kv { get; init; } = null!;
#pragma warning restore IDE1006
    }

    private DatChoDto Dto(PhieuDatCho d, BaseCore.Entities.KhoSach.DauSach ds, BaseCore.Entities.KhoSach.KhuVuc kv, DateTime bayGio) =>
        new(d.MaDatCho, d.MaDauSach, ds.TenSach, ds.TacGia, ds.AnhBia, d.MaBanSach, kv.MaKhuVuc, kv.ViTri, kv.LoaiKhuVuc,
            d.TgDat, d.TgHetHan, VongDoiDatCho.TrangThaiHieuLuc(d.TrangThai, d.TgHetHan, bayGio), d.TgXuLy, d.SoPMuon, d.LyDoHuy, Opt.QuayNhan);
}
