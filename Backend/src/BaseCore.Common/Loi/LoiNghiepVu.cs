using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BaseCore.Common.Loi;

/// <summary>Định dạng lỗi chung của mọi API (file 01 mục 5): { ma, thongBao, truong }.</summary>
public sealed record LoiApi(
    [property: JsonPropertyName("ma")] string Ma,
    [property: JsonPropertyName("thongBao")] string ThongBao,
    [property: JsonPropertyName("truong")] IReadOnlyDictionary<string, string>? Truong = null);

/// <summary>Lỗi nghiệp vụ có mã HTTP rõ ràng (400, 401, 403, 404, 409, 422).</summary>
public class LoiNghiepVu : Exception
{
    public LoiNghiepVu(int maHttp, string ma, string thongBao, IReadOnlyDictionary<string, string>? truong = null)
        : base(thongBao)
    {
        MaHttp = maHttp;
        Ma = ma;
        Truong = truong;
    }

    public int MaHttp { get; }
    public string Ma { get; }
    public IReadOnlyDictionary<string, string>? Truong { get; }

    public static LoiNghiepVu KhongHopLe(string ma, string thongBao, IReadOnlyDictionary<string, string>? truong = null)
        => new(StatusCodes.Status422UnprocessableEntity, ma, thongBao, truong);
    public static LoiNghiepVu KhongTimThay(string ma, string thongBao) => new(StatusCodes.Status404NotFound, ma, thongBao);
    public static LoiNghiepVu XungDot(string ma, string thongBao) => new(StatusCodes.Status409Conflict, ma, thongBao);
}

/// <summary>Bắt ngoại lệ, trả JSON đúng định dạng; ẩn chi tiết lỗi ngoài Development (kế thừa ý tưởng ExceptionMiddleware của basecore).</summary>
public sealed class XuLyLoiMiddleware(RequestDelegate next, ILogger<XuLyLoiMiddleware> logger, IHostEnvironment env)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (LoiNghiepVu ex)
        {
            await GhiAsync(context, ex.MaHttp, new LoiApi(ex.Ma, ex.Message, ex.Truong));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Trình duyệt hủy request – không ghi lỗi.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi chưa xử lý tại {Path}", context.Request.Path);
            var thongBao = env.IsDevelopment() ? ex.Message : "Hệ thống gặp sự cố, vui lòng thử lại sau.";
            await GhiAsync(context, StatusCodes.Status500InternalServerError, new LoiApi("LOI_HE_THONG", thongBao));
        }
    }

    public static Task GhiAsync(HttpContext context, int maHttp, LoiApi loi)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;
        context.Response.Clear();
        context.Response.StatusCode = maHttp;
        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(loi, Json));
    }
}
