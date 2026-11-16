using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.DTO.Common;
using BaseCore.Entities;
using BaseCore.Entities.KhoSach;
using BaseCore.KhoSachService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.KhoSachService.NghiepVu;

public sealed record KhuVucDto(string MaKhuVuc, string ViTri, string LoaiKhuVuc, int SoBanSach);
public sealed record KhuVucNhap(string? MaKhuVuc, string? ViTri, string? LoaiKhuVuc);

/// <summary>SPEC 6.1.4 Danh mục khu vực lưu trữ (04.00.05 với N4).</summary>
public sealed class KhuVucService(KhoSachDbContext db, IClock clock)
{
    private static readonly string[] SapXep = [nameof(KhuVuc.MaKhuVuc), nameof(KhuVuc.ViTri), nameof(KhuVuc.LoaiKhuVuc)];

    /// <summary>frmDanhMucKhuVuc (Load) và txtTimKiem, btnTimKiem: lọc theo mã, vị trí, loại.</summary>
    public Task<KetQuaTrang<KhuVucDto>> DanhSachAsync(ThamSoLuoi ts, CancellationToken ct)
    {
        var q = db.KhuVuc.AsNoTracking();
        var k = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (k is not null) q = q.Where(x => x.MaKhuVuc.Contains(k) || x.ViTri.Contains(k) || x.LoaiKhuVuc.Contains(k));
        return q.LayTrangAsync(ts, SapXep, nameof(KhuVuc.MaKhuVuc),
            x => new KhuVucDto(x.MaKhuVuc, x.ViTri, x.LoaiKhuVuc, db.Sach.Count(s => s.MaKhuVuc == x.MaKhuVuc)), ct);
    }

    public async Task<KhuVucDto> ThemAsync(KhuVucNhap n, string maNd, CancellationToken ct)
    {
        var kt = KiemTra(n, laThem: true);
        if (await db.KhuVuc.AnyAsync(x => x.MaKhuVuc == n.MaKhuVuc, ct))
            kt.Them("maKhuVuc", $"Mã khu vực {n.MaKhuVuc} đã tồn tại.");
        kt.NemNeuCoLoi();

        var kv = new KhuVuc { MaKhuVuc = n.MaKhuVuc!.Trim(), ViTri = n.ViTri!.Trim(), LoaiKhuVuc = n.LoaiKhuVuc! };
        db.KhuVuc.Add(kv);
        await NhatKyGhi.ThemAsync(db, maNd, $"Thêm khu vực lưu trữ {kv.MaKhuVuc} – {kv.ViTri} ({kv.LoaiKhuVuc})", clock.BayGio, ct);
        await LuuAsync(ct);
        return new KhuVucDto(kv.MaKhuVuc, kv.ViTri, kv.LoaiKhuVuc, 0);
    }

    /// <summary>btnSua → btnLuu: sửa vitri, loai_khuvuc (không sửa ma_khuvuc).</summary>
    public async Task<KhuVucDto> SuaAsync(string ma, KhuVucNhap n, string maNd, CancellationToken ct)
    {
        var kv = await db.KhuVuc.SingleOrDefaultAsync(x => x.MaKhuVuc == ma, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy khu vực {ma}.");
        KiemTra(n, laThem: false).NemNeuCoLoi();
        var cu = $"{kv.ViTri} ({kv.LoaiKhuVuc})";
        kv.ViTri = n.ViTri!.Trim();
        kv.LoaiKhuVuc = n.LoaiKhuVuc!;
        await NhatKyGhi.ThemAsync(db, maNd, $"Sửa khu vực lưu trữ {kv.MaKhuVuc}: {cu} → {kv.ViTri} ({kv.LoaiKhuVuc})", clock.BayGio, ct);
        await LuuAsync(ct);
        return new KhuVucDto(kv.MaKhuVuc, kv.ViTri, kv.LoaiKhuVuc, await db.Sach.CountAsync(s => s.MaKhuVuc == ma, ct));
    }

    /// <summary>btnXoa: khu vực còn bản sách trong SACH thì không cho xóa.</summary>
    public async Task XoaAsync(string ma, string maNd, CancellationToken ct)
    {
        var kv = await db.KhuVuc.SingleOrDefaultAsync(x => x.MaKhuVuc == ma, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy khu vực {ma}.");
        var soBan = await db.Sach.CountAsync(s => s.MaKhuVuc == ma, ct);
        if (soBan > 0)
            throw LoiNghiepVu.XungDot("KHU_VUC_CON_SACH", $"Khu vực {ma} còn {soBan} bản sách nên không thể xóa.");
        db.KhuVuc.Remove(kv);
        await NhatKyGhi.ThemAsync(db, maNd, $"Xóa khu vực lưu trữ {kv.MaKhuVuc} – {kv.ViTri}", clock.BayGio, ct);
        await LuuAsync(ct);
    }

    private static KiemTraDuLieu KiemTra(KhuVucNhap n, bool laThem)
    {
        var kt = new KiemTraDuLieu();
        if (laThem) kt.Ma("maKhuVuc", n.MaKhuVuc, 10, "Mã khu vực");
        kt.BatBuoc("viTri", n.ViTri, 50, "Vị trí");
        kt.ThuocMien("loaiKhuVuc", n.LoaiKhuVuc, MienGiaTri.LoaiKhuVuc.TatCa, "Loại khu vực");
        return kt;
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.LaTrungKhoa())
        {
            throw LoiNghiepVu.XungDot("TRUNG_MA", "Mã khu vực đã tồn tại.");
        }
    }
}
