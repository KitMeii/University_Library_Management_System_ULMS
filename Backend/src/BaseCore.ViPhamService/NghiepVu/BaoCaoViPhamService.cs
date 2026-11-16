using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Repository;
using BaseCore.ViPhamService.DuLieu;
using Microsoft.EntityFrameworkCore;
using HT = BaseCore.Entities.MienGiaTri.HinhThucThanhToan;

namespace BaseCore.ViPhamService.NghiepVu;

/// <summary>Một dòng tổng hợp theo khoản thu (MB07).</summary>
public sealed record DongTheoKhoanThu(string MaKhoanThu, string NoiDungTt, decimal DonGia, int SoTruongHop, int SoLuong, decimal TongTien,
    int SoTruongHopDaThanhToan, int SoTruongHopChuaThanhToan);

/// <summary>Bạn đọc vi phạm và tình trạng thanh toán (MB07 – liệt kê chi tiết).</summary>
public sealed record DongBienBanBaoCao(string SoBienBan, DateTime ThoiGianLap, string SoPMuon, string MaBanDoc, string HoTen, string LopDonVi,
    int SoTruongHop, decimal TongTien, string HtThanhToan, bool DaThanhToan);

public sealed record DongHinhThuc(string HtThanhToan, int SoBienBan, decimal TongTien);

/// <summary>Số liệu tổng hợp 02.05.01 – cũng là dữ liệu Báo cáo tình hình vi phạm mượn trả sách MB07 (SPEC 6.4.6).</summary>
public sealed record BaoCaoViPhamDto(
    DateOnly TuNgay, DateOnly DenNgay,
    int SoBienBan, int SoTruongHop, decimal TongTienViPham, decimal TongTienDaThu, decimal TongTienChuaThu,
    IReadOnlyList<DongTheoKhoanThu> TheoKhoanThu, IReadOnlyList<DongHinhThuc> TheoHinhThuc, IReadOnlyList<DongBienBanBaoCao> BienBan);

public sealed record KyBaoCaoNhap(DateOnly? TuNgay, DateOnly? DenNgay);
public sealed record KetQuaLapBaoCao(DateOnly NgayLap, string ThongBao);

/// <summary>
/// 02.05.01 Tổng hợp tình hình vi phạm (TT090–TT094) và 02.05.02 Lập Báo cáo tình hình vi phạm (TT095–TT098).
/// Kỳ báo cáo theo ngày của BIENBAN.thoigian_lap. Công thức SPEC 6.4.6: số trường hợp vi phạm = COUNT(CT_BIENBAN.ma_bansach)
/// theo ma_khoanthu; tổng tiền theo khoản thu = SUM(thanh_tien) theo ma_khoanthu; tổng tiền thu trong kỳ = SUM(tong_tien)
/// của các biên bản đã thanh toán (ht_thanhtoan khác “Chưa thanh toán”).
/// </summary>
public sealed class BaoCaoViPhamService(ViPhamDbContext db, IClock clock)
{
    public async Task<BaoCaoViPhamDto> TongHopAsync(DateOnly? tuNgay, DateOnly? denNgay, CancellationToken ct)
    {
        var (tu, den) = KyBaoCao.KiemTra(tuNgay, denNgay, clock.HomNay);
        var tuTg = tu.ToDateTime(TimeOnly.MinValue);
        var denTg = den.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var bienBan = await (from bb in db.BienBan.AsNoTracking()
                             join bd in db.BanDoc on bb.MaBanDoc equals bd.MaBanDoc
                             where bb.ThoiGianLap >= tuTg && bb.ThoiGianLap < denTg
                             orderby bb.ThoiGianLap, bb.SoBienBan
                             select new { bb.SoBienBan, bb.ThoiGianLap, bb.SoPMuon, bb.MaBanDoc, bd.HoTen, bd.LopDonVi, bb.TongTien, bb.HtThanhToan })
            .ToListAsync(ct);
        var chiTiet = await (from cb in db.CtBienBan.AsNoTracking()
                             join bb in db.BienBan on cb.SoBienBan equals bb.SoBienBan
                             where bb.ThoiGianLap >= tuTg && bb.ThoiGianLap < denTg
                             select new { cb.SoBienBan, cb.MaKhoanThu, cb.SoLuong, cb.ThanhTien, DaThanhToan = bb.HtThanhToan != HT.ChuaThanhToan })
            .ToListAsync(ct);
        var khoanThu = await db.KhoanThu.AsNoTracking().OrderBy(x => x.MaKhoanThu).ToListAsync(ct);

        var theoKhoanThu = khoanThu.Select(k =>
        {
            var ds = chiTiet.Where(c => c.MaKhoanThu == k.MaKhoanThu).ToList();
            return new DongTheoKhoanThu(k.MaKhoanThu, k.NoiDungTt, k.DonGia, ds.Count, ds.Sum(c => c.SoLuong), ds.Sum(c => c.ThanhTien),
                ds.Count(c => c.DaThanhToan), ds.Count(c => !c.DaThanhToan));
        }).ToList();
        var soTruongHop = chiTiet.GroupBy(c => c.SoBienBan).ToDictionary(g => g.Key, g => g.Count());
        var dsBienBan = bienBan.Select(b => new DongBienBanBaoCao(b.SoBienBan, b.ThoiGianLap, b.SoPMuon, b.MaBanDoc, b.HoTen, b.LopDonVi,
            soTruongHop.GetValueOrDefault(b.SoBienBan), b.TongTien, b.HtThanhToan, b.HtThanhToan != HT.ChuaThanhToan)).ToList();
        var theoHinhThuc = HT.TatCa.Select(h => new DongHinhThuc(h, dsBienBan.Count(b => b.HtThanhToan == h),
            dsBienBan.Where(b => b.HtThanhToan == h).Sum(b => b.TongTien))).ToList();

        return new BaoCaoViPhamDto(tu, den, dsBienBan.Count, chiTiet.Count, chiTiet.Sum(c => c.ThanhTien),
            dsBienBan.Where(b => b.DaThanhToan).Sum(b => b.TongTien), dsBienBan.Where(b => !b.DaThanhToan).Sum(b => b.TongTien),
            theoKhoanThu, theoHinhThuc, dsBienBan);
    }

    /// <summary>02.05.02 btnIn: kiểm tra kỳ, ngày lập tự sinh, ghi nhật ký in báo cáo MB07 (TT098).</summary>
    public async Task<KetQuaLapBaoCao> LapAsync(KyBaoCaoNhap n, string maNd, CancellationToken ct)
    {
        var (tu, den) = KyBaoCao.KiemTra(n.TuNgay, n.DenNgay, clock.HomNay);
        await NhatKyGhi.ThemAsync(db, maNd, $"02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách (MB07) kỳ {tu:dd/MM/yyyy} – {den:dd/MM/yyyy}", clock.BayGio, ct);
        await db.SaveChangesAsync(ct);
        return new KetQuaLapBaoCao(clock.HomNay, $"Đã lập Báo cáo tình hình vi phạm mượn trả sách kỳ {tu:dd/MM/yyyy} – {den:dd/MM/yyyy}.");
    }
}
