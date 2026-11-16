using BaseCore.Common.Loi;
using Microsoft.AspNetCore.Antiforgery;

namespace ThuVien.Web.HaTang;

/// <summary>
/// Đặt HttpContext.User theo đúng cookie của khu vực: /quan-tri, /api/quan-tri → NhanVien; còn lại → BanDoc (mặc định).
/// Cần cho AntiForgery (token gắn với danh tính khi sinh) và cho layout biết người đang đăng nhập.
/// </summary>
public sealed class ChonDanhTinhMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var p = ctx.Request.Path;
        if (p.StartsWithSegments("/quan-tri") || p.StartsWithSegments("/api/quan-tri"))
        {
            var kq = await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.AuthenticateAsync(ctx, XacThucWeb.NhanVien);
            ctx.User = kq.Succeeded ? kq.Principal! : new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
        }
        await next(ctx);
    }
}

/// <summary>Mọi request ghi tới /api/** phải có AntiForgery token ở header "RequestVerificationToken" (file 11).</summary>
public sealed class KiemTraAntiforgeryApiMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.Request.Path.StartsWithSegments("/api") &&
            !HttpMethods.IsGet(ctx.Request.Method) && !HttpMethods.IsHead(ctx.Request.Method) &&
            !HttpMethods.IsOptions(ctx.Request.Method))
        {
            if (!await antiforgery.IsRequestValidAsync(ctx))
            {
                await XuLyLoiMiddleware.GhiAsync(ctx, StatusCodes.Status400BadRequest,
                    new LoiApi("ANTIFORGERY", "Phiên làm việc không hợp lệ, vui lòng tải lại trang."));
                return;
            }
        }
        await next(ctx);
    }
}

/// <summary>
/// Header bảo mật (file 11): CSP chỉ nguồn nội bộ, chống nhúng khung, không đoán kiểu tệp.
/// Ngoại lệ duy nhất: trang in chứng từ/báo cáo <c>/quan-tri/in/…</c> được nhúng khung bởi CHÍNH trang web này
/// (in trong khung ẩn của btnIn, xem trước MB06/MB07) – frame-ancestors 'self', X-Frame-Options SAMEORIGIN.
/// </summary>
public sealed class HeaderBaoMatMiddleware(RequestDelegate next)
{
    private const string CspGoc =
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data: blob:; font-src 'self'; " +
        "connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; ";
    private const string Csp = CspGoc + "frame-ancestors 'none'";
    private const string CspTrangIn = CspGoc + "frame-ancestors 'self'";

    public static bool LaTrangIn(PathString p) => p.StartsWithSegments("/quan-tri/in");

    public Task InvokeAsync(HttpContext ctx)
    {
        var h = ctx.Response.Headers;
        var trangIn = LaTrangIn(ctx.Request.Path);
        h["Content-Security-Policy"] = trangIn ? CspTrangIn : Csp;
        h["X-Content-Type-Options"] = "nosniff";
        h["Referrer-Policy"] = "strict-origin-when-cross-origin";
        h["X-Frame-Options"] = trangIn ? "SAMEORIGIN" : "DENY";
        h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        return next(ctx);
    }
}
