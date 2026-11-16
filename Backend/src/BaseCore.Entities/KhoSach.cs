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

/// <summary>
/// X09. XULY_CHUYENDEN (mở rộng – GIA_DINH D47): Bộ phận kho sách đã xử lý một dòng “chuyển đến” của dgvChuyenDen.
/// 03.03.02: so_chungtu = so_ptra của dòng CT_PHIEUTRA trả Mất/Hư hỏng. Có dòng này thì bản sách không còn bị chặn
/// cho mượn vì lần trả đó, kể cả khi sau này N3 sửa tình trạng về “Sẵn sàng” (sách hư hỏng đã sửa).
/// </summary>
[Table("XULY_CHUYENDEN", Schema = LuocDo.KhoSach)]
public class XuLyChuyenDen
{
    /// <summary>Tiến trình 03.03.02 – cập nhật tình trạng sách bị mất, hư hỏng.</summary>
    public const string MatHuHong = "03.03.02";

    [Column("ma_bansach"), MaxLength(10)] public string MaBanSach { get; set; } = null!;
    [Column("so_chungtu"), MaxLength(10)] public string SoChungTu { get; set; } = null!;
    [Column("tientrinh"), MaxLength(10)] public string TienTrinh { get; set; } = null!;
    [Column("tinhtrang_moi"), MaxLength(30), Required] public string TinhTrangMoi { get; set; } = null!;
    [Column("ma_nd"), MaxLength(10), Required] public string MaNd { get; set; } = null!;
    [Column("thoigian_xuly")] public DateTime ThoiGianXuLy { get; set; }
}
