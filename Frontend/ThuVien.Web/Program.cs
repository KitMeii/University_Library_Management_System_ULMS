using BaseCore.Common.HeThong;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using ThuVien.Web.HaTang;

// Ứng dụng web ULMS: Cổng bạn đọc (/) + Hệ thống nghiệp vụ (/quan-tri). Gọi backend qua gateway.
if (HealthProbe.LaYeuCauKiemTra(args)) return await HealthProbe.ChayAsync();

var builder = WebApplication.CreateBuilder(args);
builder.AddThuVienLogging("web");

builder.Services.Configure<ThuVienOptions>(builder.Configuration.GetSection(ThuVienOptions.Muc));
builder.Services.AddHttpClient<GatewayClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Gateway:Url"] ?? "http://gateway:8080");
    c.Timeout = TimeSpan.FromSeconds(30);
});

// Khóa bảo vệ cookie lưu bền (volume) để khởi động lại container không đăng xuất mọi người
var thuMucKhoa = builder.Configuration["DataProtection:ThuMuc"];
var dp = builder.Services.AddDataProtection().SetApplicationName("ULMS");
if (!string.IsNullOrWhiteSpace(thuMucKhoa)) dp.PersistKeysToFileSystem(new DirectoryInfo(thuMucKhoa));

builder.Services.AddThuVienXacThuc();
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "RequestVerificationToken";
    o.Cookie.Name = ".ULMS.Antiforgery";
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);
builder.Services.AddHealthChecks();
builder.Services.AddBffProxy(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/loi");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/loi/{0}");
app.UseResponseCompression();
app.UseMiddleware<HeaderBaoMatMiddleware>();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Thư viện có phiên bản trong đường dẫn → cache dài hạn
        if (ctx.Context.Request.Path.StartsWithSegments("/lib") || ctx.Context.Request.Path.StartsWithSegments("/fonts"))
            ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
    }
});
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<ChonDanhTinhMiddleware>();
app.UseAuthorization();
app.UseMiddleware<KiemTraAntiforgeryApiMiddleware>();

app.MapThuVienHealth();
app.MapReverseProxy();
app.MapControllerRoute("areas", "{area:exists}/{controller=TongQuan}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
return 0;

public partial class Program;
