using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.DTO.Common;
using BaseCore.Entities.ViPham;
using BaseCore.Repository;
using BaseCore.ViPhamService.DuLieu;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.ViPhamService.NghiepVu;

public sealed record KhoanThuDto(string MaKhoanThu, string NoiDungTt, decimal DonGia);
public sealed record KhoanThuNhap(string? MaKhoanThu, string? NoiDungTt, decimal? DonGia);

/// <summary>SPEC 6.1.2 Danh mục khoản thu (04.00.06 với N4; N2 chỉ xem).</summary>
public sealed class KhoanThuService(ViPhamDbContext db, IClock clock)
{
    private static readonly string[] SapXep = [nameof(KhoanThu.MaKhoanThu), nameof(KhoanThu.NoiDungTt), nameof(KhoanThu.DonGia)];

    /// <summary>frmDanhMucKhoanThu (Load); txtTimKiem, btnTimKiem: lọc theo mã, nội dung.</summary>
    public Task<KetQuaTrang<KhoanThuDto>> DanhSachAsync(ThamSoLuoi ts, CancellationToken ct)
    {
        var q = db.KhoanThu.AsNoTracking();
        var k = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (k is not null) q = q.Where(x => x.MaKhoanThu.Contains(k) || x.NoiDungTt.Contains(k));
        return q.LayTrangAsync(ts, SapXep, nameof(KhoanThu.MaKhoanThu), x => new KhoanThuDto(x.MaKhoanThu, x.NoiDungTt, x.DonGia), ct);
    }

    public async Task<KhoanThuDto> ThemAsync(KhoanThuNhap n, string maNd, CancellationToken ct)
    {
        var kt = KiemTra(n, true);
        if (await db.KhoanThu.AnyAsync(x => x.MaKhoanThu == n.MaKhoanThu, ct)) kt.Them("maKhoanThu", $"Mã khoản thu {n.MaKhoanThu} đã tồn tại.");
        kt.NemNeuCoLoi();
        var kt2 = new KhoanThu { MaKhoanThu = n.MaKhoanThu!.Trim(), NoiDungTt = n.NoiDungTt!.Trim(), DonGia = n.DonGia!.Value };
        db.KhoanThu.Add(kt2);
        await NhatKyGhi.ThemAsync(db, maNd, $"Thêm khoản thu {kt2.MaKhoanThu} – {kt2.NoiDungTt}, đơn giá {kt2.DonGia:N0} đ", clock.BayGio, ct);
        await LuuAsync(ct);
        return new KhoanThuDto(kt2.MaKhoanThu, kt2.NoiDungTt, kt2.DonGia);
    }

    /// <summary>btnSua → btnLuu: sửa noidung_tt, dongia.</summary>
    public async Task<KhoanThuDto> SuaAsync(string ma, KhoanThuNhap n, string maNd, CancellationToken ct)
    {
        var x = await db.KhoanThu.SingleOrDefaultAsync(k => k.MaKhoanThu == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy khoản thu {ma}.");
        KiemTra(n, false).NemNeuCoLoi();
        var cu = $"{x.NoiDungTt}, {x.DonGia:N0} đ";
        x.NoiDungTt = n.NoiDungTt!.Trim();
        x.DonGia = n.DonGia!.Value;
        await NhatKyGhi.ThemAsync(db, maNd, $"Sửa khoản thu {ma}: {cu} → {x.NoiDungTt}, {x.DonGia:N0} đ", clock.BayGio, ct);
        await LuuAsync(ct);
        return new KhoanThuDto(x.MaKhoanThu, x.NoiDungTt, x.DonGia);
    }

    /// <summary>btnXoa: khoản thu đã dùng trong biên bản vi phạm (CT_BIENBAN) thì không cho xóa.</summary>
    public async Task XoaAsync(string ma, string maNd, CancellationToken ct)
    {
        var x = await db.KhoanThu.SingleOrDefaultAsync(k => k.MaKhoanThu == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy khoản thu {ma}.");
        if (await db.CtBienBan.AnyAsync(c => c.MaKhoanThu == ma, ct))
            throw LoiNghiepVu.XungDot("DA_DUNG_TRONG_BIEN_BAN", $"Khoản thu {ma} đã được dùng trong biên bản vi phạm nên không thể xóa.");
        db.KhoanThu.Remove(x);
        await NhatKyGhi.ThemAsync(db, maNd, $"Xóa khoản thu {ma} – {x.NoiDungTt}", clock.BayGio, ct);
        await LuuAsync(ct);
    }

    private static KiemTraDuLieu KiemTra(KhoanThuNhap n, bool laThem)
    {
        var kt = new KiemTraDuLieu();
        if (laThem) kt.Ma("maKhoanThu", n.MaKhoanThu, 10, "Mã khoản thu");
        kt.BatBuoc("noiDungTt", n.NoiDungTt, 100, "Nội dung thanh toán");
        // N(10), chữ số, > 0
        kt.Dung(n.DonGia is > 0 && n.DonGia <= 9_999_999_999m && decimal.Truncate(n.DonGia.Value) == n.DonGia,
            "donGia", "Đơn giá phải là số nguyên dương, tối đa 10 chữ số.");
        return kt;
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.LaTrungKhoa())
        {
            throw LoiNghiepVu.XungDot("TRUNG_MA", "Mã khoản thu đã tồn tại.");
        }
    }
}
