using System.Threading.RateLimiting;
using BaseCore.Common.Loi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseCore.Common.HeThong;

/// <summary>Giới hạn tần suất có sẵn của .NET cho đăng nhập, kích hoạt, đặt chỗ (file 11 mục 1).</summary>
public static class GioiHanTanSuat
{
    public const string DangNhap = "dang-nhap";

    /// <summary>Số lần đăng nhập mỗi phút cho một IP: cấu hình "GioiHanTanSuat:DangNhapMoiPhut" (mặc định 10).</summary>
    public static IServiceCollection AddThuVienRateLimit(this IServiceCollection services, IConfiguration config)
    {
        var moiPhut = config.GetValue("GioiHanTanSuat:DangNhapMoiPhut", 10);
        return services.AddRateLimiter(o =>
        {
            o.OnRejected = (ctx, _) => new ValueTask(XuLyLoiMiddleware.GhiAsync(ctx.HttpContext,
                StatusCodes.Status429TooManyRequests,
                new LoiApi("QUA_NHIEU_YEU_CAU", "Bạn thao tác quá nhiều lần, vui lòng thử lại sau 1 phút.")));
            o.AddPolicy(DangNhap, http => RateLimitPartition.GetFixedWindowLimiter(
                DiaChiNguoiDung(http),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = moiPhut, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
    }

    /// <summary>IP người dùng thật: web/gateway chuyển tiếp qua X-Forwarded-For.</summary>
    public static string DiaChiNguoiDung(HttpContext http)
    {
        var xff = http.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(xff)) return xff.Split(',')[0].Trim();
        return http.Connection.RemoteIpAddress?.ToString() ?? "khong-ro";
    }
}
