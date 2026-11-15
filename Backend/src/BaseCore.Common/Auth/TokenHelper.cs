using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BaseCore.Common.Auth;

/// <summary>Cấu hình JWT dùng chung cho các service và gateway (mục cấu hình "Jwt").</summary>
public sealed class JwtOptions
{
    public const string Muc = "Jwt";

    /// <summary>Khóa ký HS256, tối thiểu 32 byte, lấy từ biến môi trường <c>Jwt__SecretKey</c>.</summary>
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ulms";
    public string Audience { get; set; } = "ulms";
    /// <summary>Thời hạn token (phút). Mặc định bằng hạn tối đa của cookie bạn đọc (14 ngày).</summary>
    public int ThoiHanPhut { get; set; } = 14 * 24 * 60;

    public SymmetricSecurityKey LayKhoaKy()
    {
        var bytes = Encoding.UTF8.GetBytes(SecretKey ?? string.Empty);
        if (bytes.Length < 32)
            throw new InvalidOperationException("Jwt:SecretKey phải dài tối thiểu 32 byte (biến môi trường Jwt__SecretKey).");
        return new SymmetricSecurityKey(bytes);
    }

    public TokenValidationParameters TaoThamSoKiemTra() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = LayKhoaKy(),
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        NameClaimType = ThuVienClaims.Ten,
        RoleClaimType = ThuVienClaims.Loai
    };
}

/// <summary>Sinh JWT (HS256). Giữ vai trò như TokenHelper của basecore.</summary>
public static class TokenHelper
{
    public static string TaoToken(JwtOptions options, IEnumerable<Claim> claims, DateTime bayGioUtc)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = options.Issuer,
            Audience = options.Audience,
            NotBefore = bayGioUtc.AddMinutes(-1),
            Expires = bayGioUtc.AddMinutes(options.ThoiHanPhut),
            SigningCredentials = new SigningCredentials(options.LayKhoaKy(), SecurityAlgorithms.HmacSha256)
        };
        var handler = new JwtSecurityTokenHandler { OutboundClaimTypeMap = new Dictionary<string, string>() };
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
