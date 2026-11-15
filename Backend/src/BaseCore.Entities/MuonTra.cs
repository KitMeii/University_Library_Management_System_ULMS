using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseCore.Entities.MuonTra;

// Schema "muontra" – service muontra sở hữu (GIA_DINH D11, D12).

/// <summary>B03. BANDOC</summary>
[Table("BANDOC", Schema = LuocDo.MuonTra)]
public class BanDoc
{
    [Key, Column("ma_bandoc"), MaxLength(10)] public string MaBanDoc { get; set; } = null!;
    [Column("hoten"), MaxLength(50), Required] public string HoTen { get; set; } = null!;
    [Column("doituong"), MaxLength(20), Required] public string DoiTuong { get; set; } = null!;
    [Column("ma_svhvcb"), MaxLength(15), Required] public string MaSvHvCb { get; set; } = null!;
    [Column("lop_donvi"), MaxLength(50), Required] public string LopDonVi { get; set; } = null!;
    [Column("ngaysinh")] public DateOnly NgaySinh { get; set; }
    [Column("so_thebd"), MaxLength(15), Required] public string SoTheBd { get; set; } = null!;
    [Column("dienthoai_email"), MaxLength(50)] public string? DienThoaiEmail { get; set; }
    [Column("hsd_tungay")] public DateOnly HsdTuNgay { get; set; }
    [Column("hsd_denngay")] public DateOnly HsdDenNgay { get; set; }
    [Column("anh"), MaxLength(100)] public string? Anh { get; set; }
}

/// <summary>B05. PHIEUMUON</summary>
[Table("PHIEUMUON", Schema = LuocDo.MuonTra)]
public class PhieuMuon
{
    [Key, Column("so_pmuon"), MaxLength(10)] public string SoPMuon { get; set; } = null!;
    [Column("ma_bandoc"), MaxLength(10), Required] public string MaBanDoc { get; set; } = null!;
    [Column("ngaylap_pmuon")] public DateOnly NgayLapPMuon { get; set; }
    [Column("hoten"), MaxLength(50), Required] public string HoTen { get; set; } = null!;
    [Column("ma_nd"), MaxLength(10), Required] public string MaNd { get; set; } = null!;
    [Column("ten_nd"), MaxLength(50), Required] public string TenNd { get; set; } = null!;
}

/// <summary>B05.1. CT_PHIEUMUON – khóa (ma_bansach, so_pmuon).</summary>
[Table("CT_PHIEUMUON", Schema = LuocDo.MuonTra)]
public class CtPhieuMuon
{
    [Column("ma_bansach"), MaxLength(10)] public string MaBanSach { get; set; } = null!;
    [Column("so_pmuon"), MaxLength(10)] public string SoPMuon { get; set; } = null!;
    [Column("ngaymuon")] public DateOnly NgayMuon { get; set; }
    [Column("hantra")] public DateOnly HanTra { get; set; }
    [Column("tinhtrang_muon"), MaxLength(50), Required] public string TinhTrangMuon { get; set; } = null!;
    [Column("ghichu_muon"), MaxLength(200)] public string? GhiChuMuon { get; set; }
    [Column("ten_sach"), MaxLength(200), Required] public string TenSach { get; set; } = null!;
}

/// <summary>B06. PHIEUGIAHAN</summary>
[Table("PHIEUGIAHAN", Schema = LuocDo.MuonTra)]
public class PhieuGiaHan
{
    [Key, Column("so_pgiahan"), MaxLength(10)] public string SoPGiaHan { get; set; } = null!;
    [Column("so_pmuon"), MaxLength(10), Required] public string SoPMuon { get; set; } = null!;
    [Column("ma_bandoc"), MaxLength(10), Required] public string MaBanDoc { get; set; } = null!;
    [Column("ngaylap_pgiahan")] public DateOnly NgayLapPGiaHan { get; set; }
    [Column("ngay_denghi")] public DateOnly NgayDeNghi { get; set; }
    [Column("ma_nd"), MaxLength(10), Required] public string MaNd { get; set; } = null!;
    [Column("ten_nd"), MaxLength(50), Required] public string TenNd { get; set; } = null!;
}

/// <summary>B06.1. CT_PHIEUGIAHAN – khóa (ma_bansach, so_pgiahan).</summary>
[Table("CT_PHIEUGIAHAN", Schema = LuocDo.MuonTra)]
public class CtPhieuGiaHan
{
    [Column("ma_bansach"), MaxLength(10)] public string MaBanSach { get; set; } = null!;
    [Column("so_pgiahan"), MaxLength(10)] public string SoPGiaHan { get; set; } = null!;
    [Column("hantra_cu")] public DateOnly HanTraCu { get; set; }
    [Column("ketqua_giahan"), MaxLength(20), Required] public string KetQuaGiaHan { get; set; } = null!;
    [Column("hantra_moi")] public DateOnly? HanTraMoi { get; set; }
    [Column("lydo_khonggh"), MaxLength(200)] public string? LyDoKhongGh { get; set; }
    [Column("ten_sach"), MaxLength(200), Required] public string TenSach { get; set; } = null!;
    [Column("so_lan_giahan")] public int SoLanGiaHan { get; set; }
}

/// <summary>B07. PHIEUTRA</summary>
[Table("PHIEUTRA", Schema = LuocDo.MuonTra)]
public class PhieuTra
{
    [Key, Column("so_ptra"), MaxLength(10)] public string SoPTra { get; set; } = null!;
    [Column("so_pmuon"), MaxLength(10), Required] public string SoPMuon { get; set; } = null!;
    [Column("ma_bandoc"), MaxLength(10), Required] public string MaBanDoc { get; set; } = null!;
    [Column("ngaylap_ptra")] public DateOnly NgayLapPTra { get; set; }
    [Column("ngaytra")] public DateOnly NgayTra { get; set; }
    [Column("ma_nd"), MaxLength(10), Required] public string MaNd { get; set; } = null!;
    [Column("ten_nd"), MaxLength(50), Required] public string TenNd { get; set; } = null!;
}

/// <summary>B07.1. CT_PHIEUTRA – khóa (ma_bansach, so_ptra).</summary>
[Table("CT_PHIEUTRA", Schema = LuocDo.MuonTra)]
public class CtPhieuTra
{
    [Column("ma_bansach"), MaxLength(10)] public string MaBanSach { get; set; } = null!;
    [Column("so_ptra"), MaxLength(10)] public string SoPTra { get; set; } = null!;
    [Column("tinhtrang_tra"), MaxLength(50), Required] public string TinhTrangTra { get; set; } = null!;
    [Column("ghichu_tra"), MaxLength(200)] public string? GhiChuTra { get; set; }
    [Column("so_ngay_quahan")] public int SoNgayQuaHan { get; set; }
}

/// <summary>X04. TAIKHOAN_BANDOC (mở rộng, GIA_DINH D12)</summary>
[Table("TAIKHOAN_BANDOC", Schema = LuocDo.MuonTra)]
public class TaiKhoanBanDoc
{
    [Key, Column("ma_bandoc"), MaxLength(10)] public string MaBanDoc { get; set; } = null!;
    [Column("matkhau"), MaxLength(200)] public string? MatKhau { get; set; }
    [Column("tinhtrang"), MaxLength(20), Required] public string TinhTrang { get; set; } = MienGiaTri.TinhTrangTaiKhoanBanDoc.ChuaKichHoat;
    [Column("so_lan_sai")] public int SoLanSai { get; set; }
    [Column("tg_khoa_den")] public DateTime? TgKhoaDen { get; set; }
    [Column("tg_dangnhap_cuoi")] public DateTime? TgDangNhapCuoi { get; set; }
    [Column("tg_tao")] public DateTime TgTao { get; set; }
}

/// <summary>X05. PHIEUDATCHO (mở rộng)</summary>
[Table("PHIEUDATCHO", Schema = LuocDo.MuonTra)]
public class PhieuDatCho
{
    [Key, Column("ma_datcho"), MaxLength(20)] public string MaDatCho { get; set; } = null!;
    [Column("ma_bandoc"), MaxLength(10), Required] public string MaBanDoc { get; set; } = null!;
    [Column("ma_dausach"), MaxLength(10), Required] public string MaDauSach { get; set; } = null!;
    [Column("ma_bansach"), MaxLength(10), Required] public string MaBanSach { get; set; } = null!;
    [Column("tg_dat")] public DateTime TgDat { get; set; }
    [Column("tg_hethan")] public DateTime TgHetHan { get; set; }
    [Column("trangthai"), MaxLength(20), Required] public string TrangThai { get; set; } = MienGiaTri.TrangThaiDatCho.DangGiu;
    [Column("tg_xuly")] public DateTime? TgXuLy { get; set; }
    [Column("ma_nd_xuly"), MaxLength(10)] public string? MaNdXuLy { get; set; }
    /// <summary>File 02 ghi nvarchar(20); dùng nvarchar(10) để khớp kiểu khóa ngoại PHIEUMUON.so_pmuon C(10) (GIA_DINH D25).</summary>
    [Column("so_pmuon"), MaxLength(10)] public string? SoPMuon { get; set; }
    [Column("lydo_huy"), MaxLength(200)] public string? LyDoHuy { get; set; }
}

/// <summary>X06. YEUTHICH (mở rộng) – khóa (ma_bandoc, ma_dausach).</summary>
[Table("YEUTHICH", Schema = LuocDo.MuonTra)]
public class YeuThich
{
    [Column("ma_bandoc"), MaxLength(10)] public string MaBanDoc { get; set; } = null!;
    [Column("ma_dausach"), MaxLength(10)] public string MaDauSach { get; set; } = null!;
    [Column("tg_them")] public DateTime TgThem { get; set; }
}

/// <summary>X07. THONGBAO (mở rộng) – mọi service được INSERT (GIA_DINH D13).</summary>
[Table("THONGBAO", Schema = LuocDo.MuonTra)]
public class ThongBao
{
    [Key, Column("ma_thongbao")] public long MaThongBao { get; set; }
    [Column("ma_bandoc"), MaxLength(10), Required] public string MaBanDoc { get; set; } = null!;
    [Column("loai"), MaxLength(30), Required] public string Loai { get; set; } = null!;
    [Column("noidung"), MaxLength(500), Required] public string NoiDung { get; set; } = null!;
    [Column("lienket"), MaxLength(260)] public string? LienKet { get; set; }
    [Column("tg_tao")] public DateTime TgTao { get; set; }
    [Column("da_doc")] public bool DaDoc { get; set; }
}

/// <summary>X08. SOCHUNGTU (mở rộng) – khóa (loai, ngay); ngay = ngày đầu tháng (GIA_DINH D4).</summary>
[Table("SOCHUNGTU", Schema = LuocDo.MuonTra)]
public class SoChungTuDem
{
    [Column("loai"), MaxLength(4)] public string Loai { get; set; } = null!;
    [Column("ngay")] public DateOnly Ngay { get; set; }
    [Column("so_cuoi")] public int SoCuoi { get; set; }
}
