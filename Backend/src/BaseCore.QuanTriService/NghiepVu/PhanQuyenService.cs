using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.QuanTri;
using BaseCore.QuanTriService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.QuanTriService.NghiepVu;

public sealed record QuyenDto(string MaQuyen, string TenQuyen, string? MoTa, string Loai, bool CoQuyen, bool KhoaBo);
public sealed record KetQuaLuuQuyen(int SoThem, int SoBo);

/// <summary>SPEC 6.3.3 Cấp quyền cho tài khoản (04.00.02): cấp/thu hồi quyền theo nhóm người dùng.</summary>
public sealed class PhanQuyenService(QuanTriDbContext db, IClock clock)
{
    /// <summary>Quyền quản trị của N4 không cho bỏ (tránh hệ thống không còn tài khoản quản trị – SPEC 6.3.3).</summary>
    public static bool LaQuyenQuanTriBatBuoc(string maNhom, string maQuyen) =>
        maNhom == NhomNguoiDungMa.N4 && maQuyen.StartsWith("QT", StringComparison.Ordinal);

    public static string PhanLoai(string ma) =>
        ma.StartsWith("TT") ? "Tiến trình hệ thống (2.3)"
        : ma.StartsWith("QT") ? "Chức năng quản trị"
        : ma.StartsWith("DATCHO") || ma.StartsWith("DAUSACH") || ma.StartsWith("THELOAI") ? "Mở rộng"
        : "Quyền trên bảng dữ liệu (2.2)";

    /// <summary>cboNhom: đánh dấu trên dgvQuyen các ma_quyen nhóm đang có trong PHANQUYEN.</summary>
    public async Task<IReadOnlyList<QuyenDto>> QuyenCuaNhomAsync(string maNhom, CancellationToken ct)
    {
        if (!await db.NhomNguoiDung.AnyAsync(x => x.MaNhom == maNhom, ct))
            throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy nhóm {maNhom}.");
        var co = (await db.PhanQuyen.AsNoTracking().Where(x => x.MaNhom == maNhom).Select(x => x.MaQuyen).ToListAsync(ct)).ToHashSet();
        var ds = await db.Quyen.AsNoTracking().ToListAsync(ct);
        return ds.OrderBy(q => ThuTu(q.MaQuyen)).ThenBy(q => q.MaQuyen)
            .Select(q => new QuyenDto(q.MaQuyen, q.TenQuyen, q.MoTa, PhanLoai(q.MaQuyen), co.Contains(q.MaQuyen),
                LaQuyenQuanTriBatBuoc(maNhom, q.MaQuyen)))
            .ToList();
    }

    /// <summary>Thứ tự như hồ sơ thiết kế: quyền trên bảng (2.2) → tiến trình (2.3) → quản trị → mở rộng.</summary>
    private static int ThuTu(string ma) => ma.StartsWith("TT") ? 1 : ma.StartsWith("QT") ? 2 : PhanLoai(ma) == "Mở rộng" ? 3 : 0;

    /// <summary>
    /// btnLuu: thêm vào PHANQUYEN các cặp mới đánh dấu, xóa các cặp bị bỏ đánh dấu; ghi nhật ký.
    /// Quyền mới có hiệu lực từ lần đăng nhập tiếp theo (quyền nạp vào token lúc đăng nhập).
    /// </summary>
    public async Task<KetQuaLuuQuyen> LuuAsync(string maNhom, IReadOnlyCollection<string> maQuyenChon, string maNd, CancellationToken ct)
    {
        if (!await db.NhomNguoiDung.AnyAsync(x => x.MaNhom == maNhom, ct))
            throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy nhóm {maNhom}.");
        var tatCa = (await db.Quyen.AsNoTracking().Select(x => x.MaQuyen).ToListAsync(ct)).ToHashSet();
        var la = maQuyenChon.Where(m => !tatCa.Contains(m)).ToList();
        if (la.Count > 0) throw LoiNghiepVu.KhongHopLe("QUYEN_KHONG_TON_TAI", "Mã quyền không có trong danh mục QUYEN: " + string.Join(", ", la));
        var chon = maQuyenChon.ToHashSet();
        var thieuQuanTri = tatCa.Where(m => LaQuyenQuanTriBatBuoc(maNhom, m) && !chon.Contains(m)).ToList();
        if (thieuQuanTri.Count > 0)
            throw LoiNghiepVu.KhongHopLe("KHONG_BO_QUYEN_QUAN_TRI", "Không được bỏ quyền quản trị của nhóm N4: " + string.Join(", ", thieuQuanTri));

        return await db.ChayAsync(async c =>
        {
            var hienCo = await db.PhanQuyen.Where(x => x.MaNhom == maNhom).ToListAsync(c);
            var maHienCo = hienCo.Select(x => x.MaQuyen).ToHashSet();
            var them = chon.Except(maHienCo).OrderBy(x => x).ToList();
            var bo = hienCo.Where(x => !chon.Contains(x.MaQuyen)).ToList();
            if (them.Count == 0 && bo.Count == 0) return new KetQuaLuuQuyen(0, 0);
            db.PhanQuyen.AddRange(them.Select(m => new PhanQuyen { MaNhom = maNhom, MaQuyen = m }));
            db.PhanQuyen.RemoveRange(bo);
            var noiDung = $"04.00.02 Cấp quyền nhóm {maNhom}: thêm {them.Count} ({string.Join(", ", them)}); thu hồi {bo.Count} ({string.Join(", ", bo.Select(b => b.MaQuyen))})";
            await NhatKyGhi.ThemAsync(db, maNd, noiDung, clock.BayGio, c);
            await db.SaveChangesAsync(c);
            return new KetQuaLuuQuyen(them.Count, bo.Count);
        }, ct);
    }
}
