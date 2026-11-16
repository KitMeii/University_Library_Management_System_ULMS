using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>Thông số kết nối tới hệ thống đang chạy (biến môi trường do scripts/test-docker.sh đặt).</summary>
public static class MoiTruong
{
    public static string Gateway => Environment.GetEnvironmentVariable("ULMS_GATEWAY") ?? "http://gateway:8080";
    /// <summary>Máy chủ SQL đang dùng (DB_SERVER trong .env – GIA_DINH D44): "sqlserver" (container) hoặc "host.docker.internal,1433".</summary>
    public static string SqlServer => Environment.GetEnvironmentVariable("ULMS_SQL_SERVER") ?? Environment.GetEnvironmentVariable("DB_SERVER") ?? "sqlserver";
    public static string MatKhauSa => Bat("MSSQL_SA_PASSWORD");
    public static string MatKhauDichVu(string schema) => Bat("SVC_" + schema.ToUpperInvariant() + "_PASSWORD");
    public const string MatKhauMau = "Thuvien@123";
    public const string MatKhauBanDocMau = "Docgia@123";

    public static SqlConnection KetNoi(string user, string matKhau) =>
        new($"Server={SqlServer};Database=ThuVien;User Id={user};Password={matKhau};TrustServerCertificate=True;Connect Timeout=10");

    public static SqlConnection KetNoiSa() => KetNoi("sa", MatKhauSa);

    public static HttpClient Http() => new() { BaseAddress = new Uri(Gateway), Timeout = TimeSpan.FromSeconds(30) };

    public static async Task<string> DangNhapNhanVienAsync(HttpClient http, string ten, string mk = MatKhauMau)
    {
        var res = await http.PostAsJsonAsync("/api/quan-tri/xac-thuc/dang-nhap", new { tenDangNhap = ten, matKhau = mk });
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    private static string Bat(string ten) =>
        Environment.GetEnvironmentVariable(ten) ?? throw new InvalidOperationException("Thiếu biến môi trường " + ten);
}
