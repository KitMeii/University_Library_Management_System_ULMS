namespace ThuVien.Web.Areas.QuanTri.Models;

// Dữ liệu mẫu in SPEC 6.4.1–6.4.4 (MB02–MB05) – đọc từ API của service (JSON camelCase).

public sealed record BanDocIn(string MaBanDoc, string HoTen, string? DoiTuong, string MaSvHvCb, string LopDonVi, string? SoTheBd);

public sealed record DongPhieuMuonIn(string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTra, string TinhTrangMuon, string? GhiChuMuon);
public sealed record PhieuMuonIn(string SoPMuon, DateOnly NgayLapPMuon, BanDocIn BanDoc, string TenNd, List<DongPhieuMuonIn> ChiTiet, int TongSoBan);

public sealed record DongGiaHanIn(string MaBanSach, string TenSach, DateOnly HanTraCu, string? KetQuaGiaHan, DateOnly? HanTraMoi, string? LyDoKhongGh, int SoLanGiaHan);
public sealed record PhieuGiaHanIn(string SoPGiaHan, DateOnly NgayLapPGiaHan, string SoPMuon, BanDocIn BanDoc, DateOnly NgayDeNghi, string? TenNd,
    List<DongGiaHanIn> ChiTiet, int SoNgayGiaHan);

public sealed record DongTraIn(string MaBanSach, string TenSach, DateOnly HanTra, string? TinhTrangTra, string? GhiChuTra, int SoNgayQuaHan);
public sealed record PhieuTraIn(string SoPTra, DateOnly NgayLapPTra, string SoPMuon, BanDocIn BanDoc, DateOnly NgayTra, string? TenNd, List<DongTraIn> ChiTiet);

public sealed record DongBienBanIn(string MaKhoanThu, string NoiDungTt, decimal DonGia, string MaBanSach, string TenSach, int SoLuong, decimal ThanhTien);
public sealed record BienBanIn(string SoBienBan, DateTime ThoiGianLap, string SoPMuon, BanDocIn BanDoc, string HtThanhToan, decimal TongTien, string? TenNd,
    List<DongBienBanIn> ChiTiet);

/// <summary>Khung chung của một mẫu in: tiêu đề, mã mẫu, khổ giấy, số liên.</summary>
public sealed record KhungIn(string MaMau, string TenMau, string KhoGiay, string So, string TenTruong, string[] Lien, object DuLieu);

/// <summary>Đầu tờ in của một liên.</summary>
public sealed record DauIn(KhungIn Khung, int Lien, string BoPhan, string DongNgay);

/// <summary>Một đặt chỗ ở Quầy đặt chỗ (mở rộng – file 06 mục 5), khớp JSON /api/quan-tri/dat-cho.</summary>
public sealed record DatChoQuayIn(string MaDatCho, string MaBanDoc, string HoTen, string MaSvHvCb, string MaDauSach, string TenSach,
    string MaBanSach, string MaKhuVuc, string ViTri, string LoaiKhuVuc, DateTime TgDat, DateTime TgHetHan, string TrangThai);
