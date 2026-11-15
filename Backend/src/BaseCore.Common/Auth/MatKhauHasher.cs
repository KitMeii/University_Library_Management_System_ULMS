using System.Security.Cryptography;

namespace BaseCore.Common.Auth;

public sealed class MatKhauOptions
{
    public const string Muc = "MatKhau";
    /// <summary>Số vòng lặp PBKDF2 (GIA_DINH D5, mặc định 600.000).</summary>
    public int SoVongLap { get; set; } = 600_000;
}

/// <summary>
/// Băm mật khẩu theo GIA_DINH D5: PBKDF2 HMAC-SHA256, salt 16 byte ngẫu nhiên, khóa 32 byte,
/// lưu Base64(salt ‖ hash) = 48 byte = đúng 64 ký tự (khớp NGUOIDUNG.matkhau C(64)).
/// So sánh bằng <see cref="CryptographicOperations.FixedTimeEquals"/>.
/// </summary>
public sealed class MatKhauHasher
{
    public const int DoDaiSalt = 16;
    public const int DoDaiKhoa = 32;
    public const int DoDaiChuoi = 64;

    private readonly int _soVongLap;

    public MatKhauHasher(int soVongLap)
    {
        if (soVongLap < 1) throw new ArgumentOutOfRangeException(nameof(soVongLap));
        _soVongLap = soVongLap;
    }

    public string Bam(string matKhau)
    {
        ArgumentNullException.ThrowIfNull(matKhau);
        Span<byte> buf = stackalloc byte[DoDaiSalt + DoDaiKhoa];
        RandomNumberGenerator.Fill(buf[..DoDaiSalt]);
        Rfc2898DeriveBytes.Pbkdf2(matKhau, buf[..DoDaiSalt], buf[DoDaiSalt..], _soVongLap, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(buf);
    }

    public bool KiemTra(string matKhau, string? chuoiDaBam)
    {
        if (matKhau is null || chuoiDaBam is null || chuoiDaBam.Length != DoDaiChuoi) return false;
        Span<byte> luu = stackalloc byte[DoDaiSalt + DoDaiKhoa];
        if (!Convert.TryFromBase64String(chuoiDaBam, luu, out var n) || n != luu.Length) return false;
        Span<byte> tinh = stackalloc byte[DoDaiKhoa];
        Rfc2898DeriveBytes.Pbkdf2(matKhau, luu[..DoDaiSalt], tinh, _soVongLap, HashAlgorithmName.SHA256);
        return CryptographicOperations.FixedTimeEquals(tinh, luu[DoDaiSalt..]);
    }

    /// <summary>Chính sách mật khẩu (file 11): ≥ 8 ký tự, có chữ và có số.</summary>
    public static bool DatChinhSach(string? matKhau) =>
        matKhau is { Length: >= 8 } && matKhau.Any(char.IsLetter) && matKhau.Any(char.IsDigit);
}
