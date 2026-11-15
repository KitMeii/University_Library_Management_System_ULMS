using BaseCore.Common.Auth;
using BaseCore.Common.HeThong;
using BaseCore.Common.Loi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// API Gateway (Ocelot – giữ của basecore): định tuyến /api/** tới 4 service, kiểm JWT trước khi chuyển.
if (HealthProbe.LaYeuCauKiemTra(args)) return await HealthProbe.ChayAsync();

var builder = WebApplication.CreateBuilder(args);
builder.AddThuVienLogging("gateway");
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var jwt = builder.Configuration.GetSection(JwtOptions.Muc).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", o =>
    {
        o.MapInboundClaims = false;
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = jwt.TaoThamSoKiemTra();
        o.Events = new JwtBearerEvents
        {
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                await XuLyLoiMiddleware.GhiAsync(ctx.HttpContext, StatusCodes.Status401Unauthorized,
                    new LoiApi("CHUA_DANG_NHAP", "Phiên đăng nhập không hợp lệ hoặc đã hết hạn, vui lòng đăng nhập lại."));
            }
        };
    });
builder.Services.AddHealthChecks();
builder.Services.AddOcelot();

var app = builder.Build();
app.UseMiddleware<XuLyLoiMiddleware>();
// /health trả trước, phần còn lại do Ocelot xử lý
app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/health"), b =>
{
    b.UseRouting();
    b.UseEndpoints(e => e.MapThuVienHealth());
});
await app.UseOcelot();
await app.RunAsync();
return 0;
