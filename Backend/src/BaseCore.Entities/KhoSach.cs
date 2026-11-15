using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseCore.Entities.KhoSach;

// Schema "khosach" – service khosach sở hữu (GIA_DINH D11).

/// <summary>B01. SACH (bản sách)</summary>
[Table("SACH", Schema = LuocDo.KhoSach)]
public class Sach
{
    [Key, Column("ma_bansach"), MaxLength(10)] public string MaBanSach { get; set; } = null!;
    [Column("ma_khuvuc"), MaxLength(10), Required] public string MaKhuVuc { get; set; } = null!;
    [Column("ten_sach"), MaxLength(200), Required] public string TenSach { get; set; } = null!;
    [Column("tacgia"), MaxLength(100), Required] public string TacGia { get; set; } = null!;
    [Column("tinhtrang"), MaxLength(30), Required] public string TinhTrang { get; set; } = MienGiaTri.TinhTrangSach.SanSang;
}

/// <summary>B04. KHUVUC</summary>
[Table("KHUVUC", Schema = LuocDo.KhoSach)]
public class KhuVuc
{
    [Key, Column("ma_khuvuc"), MaxLength(10)] public string MaKhuVuc { get; set; } = null!;
    [Column("vitri"), MaxLength(50), Required] public string ViTri { get; set; } = null!;
    [Column("loai_khuvuc"), MaxLength(20), Required] public string LoaiKhuVuc { get; set; } = null!;
}

/// <summary>X01. THELOAI (mở rộng)</summary>
[Table("THELOAI", Schema = LuocDo.KhoSach)]
public class TheLoai
{
    [Key, Column("ma_theloai"), MaxLength(10)] public string MaTheLoai { get; set; } = null!;
    [Column("ten_theloai"), MaxLength(100), Required] public string TenTheLoai { get; set; } = null!;
    [Column("mota"), MaxLength(500)] public string? MoTa { get; set; }
    [Column("bieutuong"), MaxLength(50)] public string? BieuTuong { get; set; }
    [Column("thutu_hienthi")] public int ThuTuHienThi { get; set; }
}

/// <summary>X02. DAUSACH (mở rộng)</summary>
[Table("DAUSACH", Schema = LuocDo.KhoSach)]
public class DauSach
{
    [Key, Column("ma_dausach"), MaxLength(10)] public string MaDauSach { get; set; } = null!;
    [Column("ten_sach"), MaxLength(200), Required] public string TenSach { get; set; } = null!;
    [Column("tacgia"), MaxLength(100)] public string? TacGia { get; set; }
    [Column("nhaxuatban"), MaxLength(150)] public string? NhaXuatBan { get; set; }
    [Column("namxb")] public int? NamXb { get; set; }
    [Column("isbn"), MaxLength(20)] public string? Isbn { get; set; }
    [Column("ma_theloai"), MaxLength(10)] public string? MaTheLoai { get; set; }
    [Column("gioithieu"), MaxLength(2000)] public string? GioiThieu { get; set; }
    [Column("tukhoa"), MaxLength(300)] public string? TuKhoa { get; set; }
    [Column("anhbia"), MaxLength(260)] public string? AnhBia { get; set; }
    [Column("mau_gay", TypeName = "char(7)")] public string? MauGay { get; set; }
    [Column("ngaytao")] public DateTime NgayTao { get; set; }
}

/// <summary>X03. DAUSACH_BANSACH (mở rộng)</summary>
[Table("DAUSACH_BANSACH", Schema = LuocDo.KhoSach)]
public class DauSachBanSach
{
    [Key, Column("ma_bansach"), MaxLength(10)] public string MaBanSach { get; set; } = null!;
    [Column("ma_dausach"), MaxLength(10), Required] public string MaDauSach { get; set; } = null!;
}
