using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace ThuVien.Web.HaTang;

/// <summary>
/// BFF (GIA_DINH D17): JavaScript gọi cùng nguồn /api/** kèm cookie + AntiForgery; web chuyển tới gateway và
/// gắn "Authorization: Bearer" lấy từ cookie tương ứng (/api/quan-tri → NhanVien, /api/cong → BanDoc).
/// Cookie của trình duyệt KHÔNG được chuyển xuống backend.
/// </summary>
public static class BffProxy
{
    public static IServiceCollection AddBffProxy(this IServiceCollection services, IConfiguration config)
    {
        var gateway = config["Gateway:Url"] ?? "http://gateway:8080";
        services.AddHttpContextAccessor();
        services.AddReverseProxy()
            .LoadFromMemory(
                [
                    new RouteConfig
                    {
                        RouteId = "api",
                        ClusterId = "gateway",
                        Match = new RouteMatch { Path = "/api/{**duongDan}" }
                    }
                ],
                [
                    new ClusterConfig
                    {
                        ClusterId = "gateway",
                        Destinations = new Dictionary<string, DestinationConfig> { ["gw"] = new() { Address = gateway } }
                    }
                ])
            .AddTransforms(ctx =>
            {
                ctx.AddRequestHeaderRemove("Cookie");
                ctx.AddRequestTransform(async t =>
                {
                    var path = t.HttpContext.Request.Path;
                    var scheme = path.StartsWithSegments("/api/quan-tri") ? XacThucWeb.NhanVien
                        : path.StartsWithSegments("/api/cong") ? XacThucWeb.BanDoc
                        : null;
                    t.ProxyRequest.Headers.Remove("Authorization");
                    if (scheme is null) return;
                    var kq = await t.HttpContext.AuthenticateAsync(scheme);
                    var token = kq.Succeeded ? kq.Properties?.GetTokenValue(XacThucWeb.TokenTen) : null;
                    if (token is not null)
                        t.ProxyRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
                });
                // D36: backend từ chối token (phiên bị hủy/hết hạn) → xóa luôn cookie để trang đăng nhập không quay vòng
                ctx.AddResponseTransform(async t =>
                {
                    if (t.ProxyResponse?.StatusCode != System.Net.HttpStatusCode.Unauthorized) return;
                    var path = t.HttpContext.Request.Path;
                    var scheme = path.StartsWithSegments("/api/quan-tri") ? XacThucWeb.NhanVien
                        : path.StartsWithSegments("/api/cong") ? XacThucWeb.BanDoc
                        : null;
                    if (scheme is not null && path != "/api/quan-tri/xac-thuc/dang-nhap" && path != "/api/cong/dang-nhap")
                        await t.HttpContext.SignOutAsync(scheme);
                });
            });
        return services;
    }
}
