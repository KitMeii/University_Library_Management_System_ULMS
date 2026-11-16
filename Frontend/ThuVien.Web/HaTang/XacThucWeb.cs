using System.Globalization;
using System.Security.Claims;
using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

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
        services.AddMemoryCache();
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
        var token = ctx.Properties.GetTokenValue(TokenTen);
        if (token is null || hetHan is null
            || !DateTimeOffset.TryParse(hetHan, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var t)
            || t <= DateTimeOffset.UtcNow
            || await PhienBiThuHoiAsync(ctx.HttpContext, token))
        {
            ctx.RejectPrincipal();
            await ctx.HttpContext.SignOutAsync(ctx.Scheme.Name);
        }
    }

    /// <summary>
    /// D36: hỏi backend phiên còn hiệu lực không (nhớ tạm 15 giây cho mỗi token). Chỉ coi là bị thu hồi khi backend trả 401;
    /// mất kết nối thì không đăng xuất người dùng (backend vẫn tự chặn token bị thu hồi).
    /// </summary>
    private static async Task<bool> PhienBiThuHoiAsync(HttpContext http, string token)
    {
        var cache = http.RequestServices.GetRequiredService<IMemoryCache>();
        var khoa = "phien:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
        if (cache.TryGetValue(khoa, out _)) return false;
        var kq = await http.RequestServices.GetRequiredService<GatewayClient>().KiemTraPhienAsync(token, http.RequestAborted);
        if (kq.MaHttp == StatusCodes.Status401Unauthorized) return true;
        if (kq.ThanhCong) cache.Set(khoa, true, TimeSpan.FromSeconds(15));
        return false;
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

}
