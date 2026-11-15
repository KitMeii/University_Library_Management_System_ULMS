namespace BaseCore.Common.Auth;

/// <summary>Tên claim trong JWT/cookie của ULMS.</summary>
public static class ThuVienClaims
{
    /// <summary>Loại tài khoản: <see cref="LoaiTaiKhoan.NhanVien"/> hoặc <see cref="LoaiTaiKhoan.BanDoc"/>.</summary>
    public const string Loai = "loai";
    public const string Ten = "ten";
    public const string MaNd = "ma_nd";
    public const string TenDangNhap = "tendangnhap";
    public const string MaNhom = "ma_nhom";
    public const string TenNhom = "ten_nhom";
    /// <summary>Mã quyền (một claim cho mỗi mã), nạp từ PHANQUYEN lúc đăng nhập (SPEC 6.3.3).</summary>
    public const string Quyen = "quyen";
    /// <summary>Mã lượt đăng nhập (NHATKY_DANGNHAP.ma_dangnhap) để ghi tg_dangxuat.</summary>
    public const string MaDangNhap = "ma_dangnhap";
    public const string MaBanDoc = "ma_bandoc";
    public const string MaSvHvCb = "ma_svhvcb";
}

public static class LoaiTaiKhoan
{
    public const string NhanVien = "NV";
    public const string BanDoc = "BD";
}

/// <summary>Tên policy phân quyền.</summary>
public static class ChinhSach
{
    public const string NhanVien = "NhanVien";
    public const string BanDoc = "BanDoc";
    /// <summary>Tiền tố policy kiểm tra một mã quyền, ví dụ "Quyen:TT025".</summary>
    public const string TienToQuyen = "Quyen:";
}
