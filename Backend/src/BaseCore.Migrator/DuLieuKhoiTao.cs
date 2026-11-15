using BaseCore.Common.Auth;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.QuanTri;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseCore.Migrator;

/// <summary>Dữ liệu khởi tạo mọi môi trường (file 02 mục 3). Chạy lại nhiều lần không trùng, không ghi đè thay đổi của N4.</summary>
public static class DuLieuKhoiTao
{
    public static readonly (string Ma, string Ten, string MoTa)[] Nhom =
    [
        (NhomNguoiDungMa.N1, "Mượn trả sách", "Bộ phận mượn trả sách: thẻ bạn đọc, phiếu mượn, gia hạn, trả, báo cáo mượn trả"),
        (NhomNguoiDungMa.N2, "Xử lý vi phạm", "Bộ phận xử lý vi phạm: biên bản vi phạm, báo cáo vi phạm"),
        (NhomNguoiDungMa.N3, "Kho sách", "Bộ phận kho sách: danh mục sách, khu vực lưu trữ, cập nhật tình trạng sách"),
        (NhomNguoiDungMa.N4, "Quản trị", "Quản trị hệ thống: tài khoản, phân quyền, nhật ký, danh mục, sao lưu – phục hồi"),
    ];

    public static async Task NapAsync(ThuVienDbContext db, IConfiguration config, MatKhauHasher hasher, IClock clock)
    {
        foreach (var (ma, ten, moTa) in Nhom)
        {
            if (!await db.NhomNguoiDung.AnyAsync(x => x.MaNhom == ma))
                db.NhomNguoiDung.Add(new NhomNguoiDung { MaNhom = ma, TenNhom = ten, MoTa = moTa });
        }
        await db.SaveChangesAsync();

        // QUYEN: cập nhật tên/mô tả; quyền MỚI thì cấp cho nhóm theo SPEC. Quyền đã có thì không đụng
        // PHANQUYEN (giữ nguyên thay đổi N4 làm ở 04.00.02).
        var daCo = await db.Quyen.ToDictionaryAsync(x => x.MaQuyen);
        foreach (var q in DuLieuQuyen.TatCa)
        {
            if (daCo.TryGetValue(q.MaQuyen, out var cu))
            {
                cu.TenQuyen = q.TenQuyen;
                cu.MoTa = q.MoTa;
                continue;
            }
            db.Quyen.Add(new Quyen { MaQuyen = q.MaQuyen, TenQuyen = q.TenQuyen, MoTa = q.MoTa });
            foreach (var n in q.Nhom)
                db.PhanQuyen.Add(new PhanQuyen { MaNhom = n, MaQuyen = q.MaQuyen });
        }
        await db.SaveChangesAsync();

        // Tài khoản quản trị N4 "admin" – mật khẩu từ biến môi trường (file 02 mục 3)
        if (!await db.NguoiDung.AnyAsync(x => x.TenDangNhap == "admin"))
        {
            var mk = config["ThuVien:KhoiTao:MatKhauAdmin"];
            if (string.IsNullOrWhiteSpace(mk) || !MatKhauHasher.DatChinhSach(mk))
                throw new InvalidOperationException(
                    "Thiếu/không đạt chính sách mật khẩu admin (ThuVien__KhoiTao__MatKhauAdmin: ≥ 8 ký tự, có chữ và số).");
            db.NguoiDung.Add(new NguoiDung
            {
                MaNd = "ND0001",
                TenNd = "Quản trị hệ thống",
                TenDangNhap = "admin",
                MatKhau = hasher.Bam(mk),
                ChucVu = "Quản trị viên",
                TinhTrang = MienGiaTri.TinhTrangNguoiDung.HoatDong,
                MaNhom = NhomNguoiDungMa.N4
            });
            await db.SaveChangesAsync();
        }
    }
}
