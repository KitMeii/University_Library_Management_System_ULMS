using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using BaseCore.Common.Auth;
using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;

namespace BaseCore.Common.HeThong;

/// <summary>Cấu hình dùng chung cho mọi Web API nghiệp vụ của ULMS.</summary>
public static class ThuVienServiceExtensions
{
    /// <summary>Serilog: console + file xoay vòng theo ngày (thư mục <c>Logging:ThuMuc</c>, mặc định ./logs).</summary>
    public static WebApplicationBuilder AddThuVienLogging(this WebApplicationBuilder builder, string tenUngDung)
    {
        var thuMuc = builder.Configuration["Logging:ThuMuc"] ?? Path.Combine(AppContext.BaseDirectory, "logs");
        builder.Host.UseSerilog((ctx, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("UngDung", tenUngDung)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {UngDung} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(Path.Combine(thuMuc, tenUngDung + "-.log"), rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30, shared: true));
        return builder;
    }

    /// <summary>Đăng ký JWT, phân quyền theo mã quyền, IClock, hasher, controller và health check.</summary>
    public static IHealthChecksBuilder AddThuVienService(this WebApplicationBuilder builder, string tenService)
    {
        builder.AddThuVienLogging(tenService);
        var services = builder.Services;

        services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Muc));
        services.Configure<MatKhauOptions>(builder.Configuration.GetSection(MatKhauOptions.Muc));
        services.Configure<BaseCore.Common.NghiepVu.QuyDinhOptions>(builder.Configuration.GetSection(BaseCore.Common.NghiepVu.QuyDinhOptions.Muc));
        services.Configure<BaseCore.Common.NghiepVu.DatChoOptions>(builder.Configuration.GetSection(BaseCore.Common.NghiepVu.DatChoOptions.Muc));
        services.AddSingleton<IClock, DongHoHeThong>();
        services.AddSingleton<MocThuHoiPhien>();
        services.AddSingleton(sp => new MatKhauHasher(sp.GetRequiredService<IOptions<MatKhauOptions>>().Value.SoVongLap));

        var jwt = builder.Configuration.GetSection(JwtOptions.Muc).Get<JwtOptions>() ?? new JwtOptions();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = jwt.TaoThamSoKiemTra();
                o.Events = new JwtBearerEvents
                {
                    // D36: từ chối token cấp trước mốc thu hồi phiên (sau phục hồi dữ liệu)
                    OnTokenValidated = async ctx =>
                    {
                        var moc = await ctx.HttpContext.RequestServices.GetRequiredService<MocThuHoiPhien>().LayAsync(ctx.HttpContext.RequestAborted);
                        if (MocThuHoiPhien.BiThuHoi(moc, ctx.Principal!, MocThuHoiPhien.CapLuc(ctx.Principal!)))
                            ctx.Fail(MocThuHoiPhien.MaLoi);
                    },
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        await XuLyLoiMiddleware.GhiAsync(ctx.HttpContext, StatusCodes.Status401Unauthorized,
                            ctx.AuthenticateFailure?.Message == MocThuHoiPhien.MaLoi
                                ? new LoiApi(MocThuHoiPhien.MaLoi, MocThuHoiPhien.ThongBao)
                                : new LoiApi("CHUA_DANG_NHAP", "Phiên đăng nhập không hợp lệ hoặc đã hết hạn, vui lòng đăng nhập lại."));
                    },
                    OnForbidden = ctx => XuLyLoiMiddleware.GhiAsync(ctx.HttpContext, StatusCodes.Status403Forbidden,
                        new LoiApi("KHONG_CO_QUYEN", "Tài khoản không có quyền thực hiện chức năng này."))
                };
            });

        services.AddSingleton<IAuthorizationPolicyProvider, QuyenPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, CoQuyenHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy(ChinhSach.NhanVien, p => p.RequireClaim(ThuVienClaims.Loai, LoaiTaiKhoan.NhanVien))
            .AddPolicy(ChinhSach.BanDoc, p => p.RequireClaim(ThuVienClaims.Loai, LoaiTaiKhoan.BanDoc));

        services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Encoder =
                System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping)
            .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ctx =>
            {
                var truong = ctx.ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors[0].ErrorMessage);
                return new ObjectResult(new LoiApi("DU_LIEU_KHONG_HOP_LE", "Dữ liệu nhập chưa hợp lệ.", truong))
                    { StatusCode = StatusCodes.Status400BadRequest };
            });

        return services.AddHealthChecks();
    }

    public static WebApplication UseThuVienService(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseMiddleware<XuLyLoiMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapThuVienHealth();
        app.MapControllers();
        return app;
    }

    /// <summary>/health: trạng thái sống + từng kiểm tra (CSDL…) dạng JSON.</summary>
    public static IEndpointConventionBuilder MapThuVienHealth(this IEndpointRouteBuilder app) =>
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (ctx, report) =>
            {
                ctx.Response.ContentType = "application/json; charset=utf-8";
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    trangThai = report.Status.ToString(),
                    kiemTra = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString())
                }));
            }
        }).AllowAnonymous();
}
