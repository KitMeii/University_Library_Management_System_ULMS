using System.Security.Claims;

namespace BaseCore.Common.Auth;

/// <summary>Đọc thông tin nhân viên đang đăng nhập từ JWT (dùng ghi NHATKY_HOATDONG, ma_nd/ten_nd chứng từ).</summary>
public static class NguoiDungHienTai
{
    public static string MaNd(this ClaimsPrincipal u) =>
        u.FindFirst(ThuVienClaims.MaNd)?.Value ?? throw new InvalidOperationException("Thiếu claim ma_nd.");

    public static string TenNd(this ClaimsPrincipal u) => u.FindFirst(ThuVienClaims.Ten)?.Value ?? string.Empty;

    public static bool CoQuyen(this ClaimsPrincipal u, string maQuyen) => u.HasClaim(ThuVienClaims.Quyen, maQuyen);
}
