namespace BaseCore.Common.HeThong;

/// <summary>
/// Cho HEALTHCHECK của Docker: chạy <c>dotnet App.dll --healthcheck</c> → gọi http://localhost:8080/health,
/// thoát 0 nếu Healthy, 1 nếu không. Không cần cài curl vào image.
/// </summary>
public static class HealthProbe
{
    public static bool LaYeuCauKiemTra(string[] args) => args.Contains("--healthcheck");

    public static async Task<int> ChayAsync(string? url = null)
    {
        url ??= Environment.GetEnvironmentVariable("HEALTHCHECK_URL") ?? "http://localhost:8080/health";
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            using var res = await http.GetAsync(url);
            return res.IsSuccessStatusCode ? 0 : 1;
        }
        catch
        {
            return 1;
        }
    }
}
