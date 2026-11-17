using System.Text.RegularExpressions;
using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.DTO.Common;
using BaseCore.Entities.KhoSach;
using BaseCore.KhoSachService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.KhoSachService.NghiepVu;

public sealed record TheLoaiDto(string MaTheLoai, string TenTheLoai, string? MoTa, string? BieuTuong, int ThuTuHienThi, int SoDauSach);
public sealed record TheLoaiNhap(string? MaTheLoai, string? TenTheLoai, string? MoTa, string? BieuTuong, int? ThuTuHienThi);

/// <summary>
/// Quản lý thể loại (X01 THELOAI – mở rộng, file 02): dùng cho danh mục và các kệ thể loại ở cổng bạn đọc.
/// Quyền mở rộng THELOAI_C/E/R (N3, N4), THELOAI_D (N4). Thể loại đang có đầu sách thì không xóa.
/// </summary>
public sealed partial class TheLoaiService(KhoSachDbContext db, IClock clock)
{
    private static readonly string[] SapXep = [nameof(TheLoai.MaTheLoai), nameof(TheLoai.TenTheLoai), nameof(TheLoai.ThuTuHienThi)];

    [GeneratedRegex("^bi-[a-z0-9-]{1,47}$")]
    private static partial Regex BieuTuongHopLe();

    public Task<KetQuaTrang<TheLoaiDto>> DanhSachAsync(ThamSoLuoi ts, CancellationToken ct)
    {
        var q = db.TheLoai.AsNoTracking();
        var k = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (k is not null) q = q.Where(x => x.MaTheLoai.Contains(k) || x.TenTheLoai.Contains(k) || (x.MoTa != null && x.MoTa.Contains(k)));
        return q.LayTrangAsync(ts, SapXep, nameof(TheLoai.ThuTuHienThi),
            x => new TheLoaiDto(x.MaTheLoai, x.TenTheLoai, x.MoTa, x.BieuTuong, x.ThuTuHienThi, db.DauSach.Count(d => d.MaTheLoai == x.MaTheLoai)), ct);
    }

    /// <summary>Danh sách gọn cho ô chọn thể loại (form đầu sách).</summary>
    public Task<List<TheLoaiDto>> TatCaAsync(CancellationToken ct) =>
        db.TheLoai.AsNoTracking().OrderBy(x => x.ThuTuHienThi).ThenBy(x => x.TenTheLoai)
            .Select(x => new TheLoaiDto(x.MaTheLoai, x.TenTheLoai, x.MoTa, x.BieuTuong, x.ThuTuHienThi, db.DauSach.Count(d => d.MaTheLoai == x.MaTheLoai)))
            .ToListAsync(ct);

    public async Task<TheLoaiDto> ThemAsync(TheLoaiNhap n, string maNd, CancellationToken ct)
    {
        var kt = KiemTra(n, laThem: true);
        if (await db.TheLoai.AnyAsync(x => x.MaTheLoai == n.MaTheLoai, ct)) kt.Them("maTheLoai", $"Mã thể loại {n.MaTheLoai} đã tồn tại.");
        await KiemTenTrungAsync(kt, n.TenTheLoai, null, ct);
        kt.NemNeuCoLoi();
        var t = new TheLoai
        {
            MaTheLoai = n.MaTheLoai!.Trim().ToUpperInvariant(), TenTheLoai = n.TenTheLoai!.Trim(), MoTa = KiemTraDuLieu.Gon(n.MoTa),
            BieuTuong = KiemTraDuLieu.Gon(n.BieuTuong), ThuTuHienThi = n.ThuTuHienThi ?? 0
        };
        db.TheLoai.Add(t);
        await NhatKyGhi.ThemAsync(db, maNd, $"Thêm thể loại {t.MaTheLoai} – {t.TenTheLoai}", clock.BayGio, ct);
        await LuuAsync(ct);
        return new TheLoaiDto(t.MaTheLoai, t.TenTheLoai, t.MoTa, t.BieuTuong, t.ThuTuHienThi, 0);
    }

    public async Task<TheLoaiDto> SuaAsync(string ma, TheLoaiNhap n, string maNd, CancellationToken ct)
    {
        var t = await db.TheLoai.SingleOrDefaultAsync(x => x.MaTheLoai == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy thể loại {ma}.");
        var kt = KiemTra(n, laThem: false);
        await KiemTenTrungAsync(kt, n.TenTheLoai, ma, ct);
        kt.NemNeuCoLoi();
        var cu = t.TenTheLoai;
        t.TenTheLoai = n.TenTheLoai!.Trim();
        t.MoTa = KiemTraDuLieu.Gon(n.MoTa);
        t.BieuTuong = KiemTraDuLieu.Gon(n.BieuTuong);
        t.ThuTuHienThi = n.ThuTuHienThi ?? 0;
        await NhatKyGhi.ThemAsync(db, maNd, $"Sửa thể loại {ma}: {cu} → {t.TenTheLoai}", clock.BayGio, ct);
        await LuuAsync(ct);
        return new TheLoaiDto(t.MaTheLoai, t.TenTheLoai, t.MoTa, t.BieuTuong, t.ThuTuHienThi, await db.DauSach.CountAsync(d => d.MaTheLoai == ma, ct));
    }

    public async Task XoaAsync(string ma, string maNd, CancellationToken ct)
    {
        var t = await db.TheLoai.SingleOrDefaultAsync(x => x.MaTheLoai == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy thể loại {ma}.");
        var so = await db.DauSach.CountAsync(d => d.MaTheLoai == ma, ct);
        if (so > 0) throw LoiNghiepVu.XungDot("THE_LOAI_CON_DAU_SACH", $"Thể loại {ma} còn {so} đầu sách nên không thể xóa.");
        db.TheLoai.Remove(t);
        await NhatKyGhi.ThemAsync(db, maNd, $"Xóa thể loại {ma} – {t.TenTheLoai}", clock.BayGio, ct);
        await LuuAsync(ct);
    }

    private static KiemTraDuLieu KiemTra(TheLoaiNhap n, bool laThem)
    {
        var kt = new KiemTraDuLieu();
        if (laThem) kt.Ma("maTheLoai", n.MaTheLoai, 10, "Mã thể loại");
        kt.BatBuoc("tenTheLoai", n.TenTheLoai, 100, "Tên thể loại");
        kt.ToiDa("moTa", n.MoTa, 500, "Mô tả");
        var bt = KiemTraDuLieu.Gon(n.BieuTuong);
        if (bt is not null && !BieuTuongHopLe().IsMatch(bt)) kt.Them("bieuTuong", "Biểu tượng phải là tên Bootstrap Icons dạng bi-…");
        if (n.ThuTuHienThi is < 0 or > 999) kt.Them("thuTuHienThi", "Thứ tự hiển thị từ 0 đến 999.");
        return kt;
    }

    private async Task KiemTenTrungAsync(KiemTraDuLieu kt, string? ten, string? truMa, CancellationToken ct)
    {
        var t = KiemTraDuLieu.Gon(ten);
        if (t is not null && await db.TheLoai.AnyAsync(x => x.TenTheLoai == t && x.MaTheLoai != truMa, ct))
            kt.Them("tenTheLoai", $"Tên thể loại “{t}” đã có.");
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.LaTrungKhoa())
        {
            throw LoiNghiepVu.XungDot("TRUNG_MA", "Mã hoặc tên thể loại đã tồn tại.");
        }
    }
}
