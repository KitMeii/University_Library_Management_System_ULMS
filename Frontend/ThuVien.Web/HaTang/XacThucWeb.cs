using System.Globalization;
using System.Security.Claims;
using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ThuVien.Web.HaTang;

/// <summary>
/// Hai cookie scheme tách biệt (file 11 mục 1): NhanVien (Area QuanTri, trượt 30 phút) và BanDoc (cổng, ghi nhớ ≤ 14 ngày).
/// JWT do service cấp được lưu TRONG cookie đã mã hóa (HttpOnly) – không bao giờ lộ ra JavaScript/LocalStorage.
/// </summary>
public static class XacThucWeb
{
    public const string NhanVien = "NhanVien";
    public const string BanDoc = "BanDoc";
    public const string TokenTen = "access_token";

    public static IServiceCollection AddThuVienXacThuc(this IServiceCollection services)
    {
        services.AddAuthentication(BanDoc)
            .AddCookie(NhanVien, o =>
            {
                o.Cookie.Name = ".ULMS.NhanVien";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                o.LoginPath = "/quan-tri/dang-nhap";
                o.LogoutPath = "/quan-tri/dang-xuat";
                o.AccessDeniedPath = "/quan-tri/khong-co-quyen";
                o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                o.SlidingExpiration = true;
                o.Events.OnValidatePrincipal = TuChoiKhiTokenHetHan;
                o.Events.OnRedirectToLogin = ctx => TraVe401NeuLaApi(ctx);
            })
            .AddCookie(BanDoc, o =>
            {
                o.Cookie.Name = ".ULMS.BanDoc";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Lax;
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                o.LoginPath = "/dang-nhap";
                o.LogoutPath = "/dang-xuat";
                o.ExpireTimeSpan = TimeSpan.FromDays(14);
                o.SlidingExpiration = true;
                o.Events.OnValidatePrincipal = TuChoiKhiTokenHetHan;
                o.Events.OnRedirectToLogin = ctx => TraVe401NeuLaApi(ctx);
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(ChinhSach.NhanVien, p => p.AddAuthenticationSchemes(NhanVien).RequireAuthenticatedUser()
                .RequireClaim(ThuVienClaims.Loai, LoaiTaiKhoan.NhanVien))
            .AddPolicy(ChinhSach.BanDoc, p => p.AddAuthenticationSchemes(BanDoc).RequireAuthenticatedUser()
                .RequireClaim(ThuVienClaims.Loai, LoaiTaiKhoan.BanDoc));
        return services;
    }

    public static AuthenticationProperties TaoThuocTinh(string token, DateTimeOffset hetHanToken, bool ghiNho)
    {
        var p = new AuthenticationProperties { IsPersistent = ghiNho, AllowRefresh = true };
        p.StoreTokens(
        [
            new AuthenticationToken { Name = TokenTen, Value = token },
            new AuthenticationToken { Name = "expires_at", Value = hetHanToken.ToString("o", CultureInfo.InvariantCulture) }
        ]);
        return p;
    }

    public static ClaimsPrincipal TaoNguoiDung(string scheme, IEnumerable<Claim> claims) =>
        new(new ClaimsIdentity(claims, scheme, ThuVienClaims.Ten, ThuVienClaims.Loai));

    private static async Task TuChoiKhiTokenHetHan(CookieValidatePrincipalContext ctx)
    {
        var hetHan = ctx.Properties.GetTokenValue("expires_at");
        if (hetHan is null || !DateTimeOffset.TryParse(hetHan, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var t)
            || t <= DateTimeOffset.UtcNow)
        {
            ctx.RejectPrincipal();
            await ctx.HttpContext.SignOutAsync(ctx.Scheme.Name);
        }
    }

    private static Task TraVe401NeuLaApi(RedirectContext<CookieAuthenticationOptions> ctx)
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    }

    public static bool CoQuyen(this ClaimsPrincipal u, string maQuyen) => u.HasClaim(ThuVienClaims.Quyen, maQuyen);
}
