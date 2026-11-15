using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseCore.Entities.ViPham;

// Schema "vipham" – service vipham sở hữu (GIA_DINH D11).

/// <summary>B02. KHOANTHU</summary>
[Table("KHOANTHU", Schema = LuocDo.ViPham)]
public class KhoanThu
{
    [Key, Column("ma_khoanthu"), MaxLength(10)] public string MaKhoanThu { get; set; } = null!;
    [Column("noidung_tt"), MaxLength(100), Required] public string NoiDungTt { get; set; } = null!;
    [Column("dongia", TypeName = "decimal(18,0)")] public decimal DonGia { get; set; }
}

/// <summary>B08. BIENBAN</summary>
[Table("BIENBAN", Schema = LuocDo.ViPham)]
public class BienBan
{
    [Key, Column("so_bienban"), MaxLength(10)] public string SoBienBan { get; set; } = null!;
    [Column("so_pmuon"), MaxLength(10), Required] public string SoPMuon { get; set; } = null!;
    [Column("ma_bandoc"), MaxLength(10), Required] public string MaBanDoc { get; set; } = null!;
    [Column("thoigian_lap")] public DateTime ThoiGianLap { get; set; }
    [Column("ht_thanhtoan"), MaxLength(20), Required] public string HtThanhToan { get; set; } = null!;
    [Column("ma_nd"), MaxLength(10), Required] public string MaNd { get; set; } = null!;
    [Column("ten_nd"), MaxLength(50), Required] public string TenNd { get; set; } = null!;
    [Column("tong_tien", TypeName = "decimal(18,0)")] public decimal TongTien { get; set; }
}

/// <summary>B08.1. CT_BIENBAN – khóa (ma_khoanthu, so_bienban, ma_bansach).</summary>
[Table("CT_BIENBAN", Schema = LuocDo.ViPham)]
public class CtBienBan
{
    [Column("ma_khoanthu"), MaxLength(10)] public string MaKhoanThu { get; set; } = null!;
    [Column("so_bienban"), MaxLength(10)] public string SoBienBan { get; set; } = null!;
    [Column("ma_bansach"), MaxLength(10)] public string MaBanSach { get; set; } = null!;
    [Column("soluong")] public int SoLuong { get; set; }
    [Column("thanh_tien", TypeName = "decimal(18,0)")] public decimal ThanhTien { get; set; }
}
