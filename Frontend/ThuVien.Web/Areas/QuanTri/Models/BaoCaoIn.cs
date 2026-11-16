namespace ThuVien.Web.Areas.QuanTri.Models;

// Dữ liệu trang in MB06/MB07 (SPEC 6.4.5–6.4.6) – khớp JSON của API báo cáo muontra / vipham.

public sealed record DongLuotMuonIn(string SoPMuon, string MaBanDoc, string HoTen, string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTra);
public sealed record DongLuotGiaHanIn(string SoPGiaHan, DateOnly NgayLapPGiaHan, string SoPMuon, string HoTen, string MaBanSach, string TenSach,
    string KetQuaGiaHan, DateOnly? HanTraMoi);
public sealed record DongLuotTraIn(string SoPTra, DateOnly NgayTra, string SoPMuon, string HoTen, string MaBanSach, string TenSach,
    string TinhTrangTra, int SoNgayQuaHan);
public sealed record DongSachChuaTraIn(string SoPMuon, string MaBanDoc, string HoTen, string MaBanSach, string TenSach, DateOnly NgayMuon,
    DateOnly HanTra, int SoNgayQuaHan);
public sealed record DiemMuonTraIn(DateOnly Ngay, int Muon, int GiaHan, int Tra);

/// <summary>MB06 Báo cáo tình hình mượn trả sách.</summary>
public sealed record BaoCaoMuonTraIn(DateOnly TuNgay, DateOnly DenNgay, int TongLuotMuon, int TongLuotGiaHan, int TongLuotTra,
    int SoSachDangMuon, int SoSachQuaHan, List<DiemMuonTraIn> TheoNgay, List<DongLuotMuonIn> LuotMuon, List<DongLuotGiaHanIn> LuotGiaHan,
    List<DongLuotTraIn> LuotTra, List<DongSachChuaTraIn> SachQuaHan);

public sealed record DongTheoKhoanThuIn(string MaKhoanThu, string NoiDungTt, decimal DonGia, int SoTruongHop, int SoLuong, decimal TongTien,
    int SoTruongHopDaThanhToan, int SoTruongHopChuaThanhToan);
public sealed record DongBienBanBaoCaoIn(string SoBienBan, DateTime ThoiGianLap, string SoPMuon, string MaBanDoc, string HoTen, string LopDonVi,
    int SoTruongHop, decimal TongTien, string HtThanhToan, bool DaThanhToan);
public sealed record DongHinhThucIn(string HtThanhToan, int SoBienBan, decimal TongTien);

/// <summary>MB07 Báo cáo tình hình vi phạm mượn trả sách.</summary>
public sealed record BaoCaoViPhamIn(DateOnly TuNgay, DateOnly DenNgay, int SoBienBan, int SoTruongHop, decimal TongTienViPham,
    decimal TongTienDaThu, decimal TongTienChuaThu, List<DongTheoKhoanThuIn> TheoKhoanThu, List<DongHinhThucIn> TheoHinhThuc,
    List<DongBienBanBaoCaoIn> BienBan);
