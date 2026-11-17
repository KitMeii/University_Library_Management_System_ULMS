namespace ThuVien.Web.Models;

// Dữ liệu trang chủ cổng bạn đọc (file 04). Khớp JSON API /api/cong/ke-sach của khosach.

public sealed record MucKeCong(string Ma, string Ten, int SoSach);

public sealed record SachTrenKeCong(string MaDauSach, string TenSach, string? TacGia, int? NamXb, string? MaTheLoai, string? TenTheLoai,
    string? AnhBiaNho, string? AnhBiaLon, string? MauGay, int SoBanCoTheMuon, int SoLuotMuon, int? Hang);

public sealed record KeSachCong(string Ma, string Ten, string Truc, List<SachTrenKeCong> Sach);

/// <param name="AnhHero">Ảnh Hero theo thứ tự tên tệp trong wwwroot/img/hero (GIA_DINH D56).</param>
/// <param name="Ke">Kệ đầu tiên (render phía server – không có JS vẫn xem được lưới bìa, file 05 mục 6); null nếu không có dữ liệu.</param>
public sealed record TrangChuVm(IReadOnlyList<string> AnhHero, IReadOnlyList<MucKeCong> DanhSachKe, KeSachCong? Ke, KhamPhaVm KhamPha);

/// <summary>Phần tối thiểu của /api/cong/sach/{ma} dùng cho liên kết sâu (tiêu đề trang).</summary>
public sealed record SachTomTatCong(string MaDauSach, string TenSach);

// ---------- Pha 7: các khu còn lại của trang chủ (file 04 mục 1) ----------
public sealed record TheLoaiCongVm(string MaTheLoai, string TenTheLoai, string? BieuTuong, string? MoTa, int SoDauSach);
public sealed record TuKhoaCongVm(string TuKhoa, int SoDauSach);
public sealed record SoLieuCongVm(int SoDauSach, int SoBanSach, int SoBanDoc, int LuotMuonThangNay);
public sealed record QuyDinhDoiTuongVm(string DoiTuong, int SoNgayMuon, int SoBanToiDa);
public sealed record QuyDinhCongVm(List<QuyDinhDoiTuongVm> DoiTuong, int SoNgayGiaHan, int SoLanGiaHanToiDa, int SoGioGiu, int ToiDaDangGiu,
    int NhacTruocGio, string QuayNhan);

/// <summary>Dữ liệu chung khu Khám phá sách (trang chủ khu 5 và trang /kham-pha) + các khu 4, 6, 7.</summary>
public sealed record KhamPhaVm(IReadOnlyList<TheLoaiCongVm> TheLoai, IReadOnlyList<TuKhoaCongVm> TuKhoa, SoLieuCongVm? SoLieu, QuyDinhCongVm? QuyDinh,
    string? Q = null, string? TheLoaiChon = null, bool ToanTrang = false);
