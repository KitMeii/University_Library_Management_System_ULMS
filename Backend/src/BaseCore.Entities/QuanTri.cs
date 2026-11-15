using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseCore.Entities.QuanTri;

// Schema "quantri" – service quantri sở hữu (GIA_DINH D11). Tên bảng/cột đúng SPEC mục 2.

/// <summary>B09. NHOMNGUOIDUNG</summary>
[Table("NHOMNGUOIDUNG", Schema = LuocDo.QuanTri)]
public class NhomNguoiDung
{
    [Key, Column("ma_nhom"), MaxLength(10)] public string MaNhom { get; set; } = null!;
    [Column("ten_nhom"), MaxLength(50), Required] public string TenNhom { get; set; } = null!;
    [Column("mota"), MaxLength(200)] public string? MoTa { get; set; }
}

/// <summary>B10. NGUOIDUNG</summary>
[Table("NGUOIDUNG", Schema = LuocDo.QuanTri)]
public class NguoiDung
{
    [Key, Column("ma_nd"), MaxLength(10)] public string MaNd { get; set; } = null!;
    [Column("ten_nd"), MaxLength(50), Required] public string TenNd { get; set; } = null!;
    [Column("tendangnhap"), MaxLength(30), Required] public string TenDangNhap { get; set; } = null!;
    /// <summary>Đã băm theo GIA_DINH D5 – đúng 64 ký tự.</summary>
    [Column("matkhau"), MaxLength(64), Required] public string MatKhau { get; set; } = null!;
    [Column("sdt"), MaxLength(15)] public string? Sdt { get; set; }
    [Column("chucvu"), MaxLength(50)] public string? ChucVu { get; set; }
    [Column("tinhtrang"), MaxLength(20), Required] public string TinhTrang { get; set; } = MienGiaTri.TinhTrangNguoiDung.HoatDong;
    [Column("ma_nhom"), MaxLength(10), Required] public string MaNhom { get; set; } = null!;
}

/// <summary>B11. QUYEN</summary>
[Table("QUYEN", Schema = LuocDo.QuanTri)]
public class Quyen
{
    [Key, Column("ma_quyen"), MaxLength(10)] public string MaQuyen { get; set; } = null!;
    [Column("ten_quyen"), MaxLength(100), Required] public string TenQuyen { get; set; } = null!;
    [Column("mota"), MaxLength(200)] public string? MoTa { get; set; }
}

/// <summary>B12. PHANQUYEN – khóa (ma_nhom, ma_quyen).</summary>
[Table("PHANQUYEN", Schema = LuocDo.QuanTri)]
public class PhanQuyen
{
    [Column("ma_nhom"), MaxLength(10)] public string MaNhom { get; set; } = null!;
    [Column("ma_quyen"), MaxLength(10)] public string MaQuyen { get; set; } = null!;
}

/// <summary>B13. NHATKY_DANGNHAP</summary>
[Table("NHATKY_DANGNHAP", Schema = LuocDo.QuanTri)]
public class NhatKyDangNhap
{
    /// <summary>DN + yyMMdd + 7 số = 15 ký tự (GIA_DINH D19).</summary>
    [Key, Column("ma_dangnhap"), MaxLength(15)] public string MaDangNhap { get; set; } = null!;
    [Column("ma_nd"), MaxLength(10), Required] public string MaNd { get; set; } = null!;
    [Column("tg_dangnhap")] public DateTime TgDangNhap { get; set; }
    [Column("tg_dangxuat")] public DateTime? TgDangXuat { get; set; }
}

/// <summary>B14. NHATKY_HOATDONG – mọi service được INSERT (GIA_DINH D13).</summary>
[Table("NHATKY_HOATDONG", Schema = LuocDo.QuanTri)]
public class NhatKyHoatDong
{
    /// <summary>HD + yyMMdd + 7 số = 15 ký tự (GIA_DINH D19).</summary>
    [Key, Column("ma_hoatdong"), MaxLength(15)] public string MaHoatDong { get; set; } = null!;
    [Column("ma_nd"), MaxLength(10), Required] public string MaNd { get; set; } = null!;
    [Column("tg_thuchien")] public DateTime TgThucHien { get; set; }
    [Column("noidung"), MaxLength(500), Required] public string NoiDung { get; set; } = null!;
}
