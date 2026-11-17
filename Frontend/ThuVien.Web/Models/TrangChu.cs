namespace ThuVien.Web.Models;

// Dữ liệu trang chủ cổng bạn đọc (file 04). Khớp JSON API /api/cong/ke-sach của khosach.

public sealed record MucKeCong(string Ma, string Ten, int SoSach);

public sealed record SachTrenKeCong(string MaDauSach, string TenSach, string? TacGia, int? NamXb, string? MaTheLoai, string? TenTheLoai,
    string? AnhBiaNho, string? AnhBiaLon, string? MauGay, int SoBanCoTheMuon, int SoLuotMuon, int? Hang);

public sealed record KeSachCong(string Ma, string Ten, string Truc, List<SachTrenKeCong> Sach);

/// <param name="AnhHero">Ảnh Hero theo thứ tự tên tệp trong wwwroot/img/hero (GIA_DINH D56).</param>
/// <param name="Ke">Kệ đầu tiên (render phía server – không có JS vẫn xem được lưới bìa, file 05 mục 6); null nếu không có dữ liệu.</param>
public sealed record TrangChuVm(IReadOnlyList<string> AnhHero, IReadOnlyList<MucKeCong> DanhSachKe, KeSachCong? Ke);
