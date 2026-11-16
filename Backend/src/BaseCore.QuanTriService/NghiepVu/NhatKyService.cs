using BaseCore.Common.Loi;
using BaseCore.DTO.Common;
using BaseCore.QuanTriService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.QuanTriService.NghiepVu;

public sealed record DongDangNhap
{
    public string MaDangNhap { get; init; } = null!;
    public string MaNd { get; init; } = null!;
    public string TenNd { get; init; } = null!;
    public string TenDangNhap { get; init; } = null!;
    public DateTime TgDangNhap { get; init; }
    public DateTime? TgDangXuat { get; init; }
}
public sealed record DongHoatDong
{
    public string MaHoatDong { get; init; } = null!;
    public string MaNd { get; init; } = null!;
    public string TenNd { get; init; } = null!;
    public string TenDangNhap { get; init; } = null!;
    public DateTime TgThucHien { get; init; }
    public string NoiDung { get; init; } = null!;
}
public sealed record NguoiDungLoc(string MaNd, string TenNd, string TenDangNhap);

/// <summary>SPEC 6.3.4 Theo dõi nhật ký hệ thống (04.00.03, 04.00.08) – giao diện chỉ đọc.</summary>
public sealed class NhatKyService(QuanTriDbContext db)
{
    /// <summary>cboNguoiDung: “Tất cả” + danh sách người dùng.</summary>
    public Task<List<NguoiDungLoc>> NguoiDungAsync(CancellationToken ct) =>
        db.NguoiDung.AsNoTracking().OrderBy(x => x.TenDangNhap).Select(x => new NguoiDungLoc(x.MaNd, x.TenNd, x.TenDangNhap)).ToListAsync(ct);

    private static (DateTime Tu, DateTime Den) KhoangNgay(DateOnly? tu, DateOnly? den)
    {
        var kt = new KiemTraDuLieu();
        if (tu is null) kt.Them("tuNgay", "Vui lòng chọn từ ngày.");
        if (den is null) kt.Them("denNgay", "Vui lòng chọn đến ngày.");
        if (tu is not null && den is not null) kt.Dung(tu <= den, "denNgay", "Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");
        kt.NemNeuCoLoi();
        return (tu!.Value.ToDateTime(TimeOnly.MinValue), den!.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));
    }

    /// <summary>Thẻ Lịch sử đăng nhập: lọc theo ma_nd và khoảng thời gian; số lượt = COUNT(ma_dangnhap).</summary>
    public Task<KetQuaTrang<DongDangNhap>> DangNhapAsync(DateOnly? tuNgay, DateOnly? denNgay, string? maNd, ThamSoLuoi ts, CancellationToken ct)
    {
        var (tu, den) = KhoangNgay(tuNgay, denNgay);
        var q = from k in db.NhatKyDangNhap.AsNoTracking()
                join n in db.NguoiDung on k.MaNd equals n.MaNd
                where k.TgDangNhap >= tu && k.TgDangNhap < den && (maNd == null || maNd == "" || k.MaNd == maNd)
                select new DongDangNhap { MaDangNhap = k.MaDangNhap, MaNd = k.MaNd, TenNd = n.TenNd, TenDangNhap = n.TenDangNhap, TgDangNhap = k.TgDangNhap, TgDangXuat = k.TgDangXuat };
        ts.Tang = ts.SapXep is null ? false : ts.Tang;
        return q.LayTrangAsync(ts, [nameof(DongDangNhap.TgDangNhap), nameof(DongDangNhap.MaNd), nameof(DongDangNhap.TgDangXuat)],
            nameof(DongDangNhap.TgDangNhap), x => x, ct);
    }

    /// <summary>Thẻ Nhật ký hoạt động: lọc theo ma_nd và khoảng thời gian; số lượt = COUNT(ma_hoatdong).</summary>
    public Task<KetQuaTrang<DongHoatDong>> HoatDongAsync(DateOnly? tuNgay, DateOnly? denNgay, string? maNd, ThamSoLuoi ts, CancellationToken ct)
    {
        var (tu, den) = KhoangNgay(tuNgay, denNgay);
        var q = from k in db.NhatKyHoatDong.AsNoTracking()
                join n in db.NguoiDung on k.MaNd equals n.MaNd
                where k.TgThucHien >= tu && k.TgThucHien < den && (maNd == null || maNd == "" || k.MaNd == maNd)
                select new DongHoatDong { MaHoatDong = k.MaHoatDong, MaNd = k.MaNd, TenNd = n.TenNd, TenDangNhap = n.TenDangNhap, TgThucHien = k.TgThucHien, NoiDung = k.NoiDung };
        var tuKhoa = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (tuKhoa is not null) q = q.Where(x => x.NoiDung.Contains(tuKhoa));
        ts.Tang = ts.SapXep is null ? false : ts.Tang;
        return q.LayTrangAsync(ts, [nameof(DongHoatDong.TgThucHien), nameof(DongHoatDong.MaNd)], nameof(DongHoatDong.TgThucHien), x => x, ct);
    }
}
