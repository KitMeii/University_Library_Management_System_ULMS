using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>Chạy riêng, sau các lớp song song: đặt mốc thu hồi làm mọi token cũ mất hiệu lực.</summary>
[CollectionDefinition("ThuHoiPhien", DisableParallelization = true)]
public sealed class BoThuHoiPhien;

/// <summary>
/// GIA_DINH D36: sau phục hồi dữ liệu, mọi phiên đăng nhập khác (nhân viên và bạn đọc) bị hủy ngay; phiên của người
/// phục hồi vẫn dùng được; đăng nhập lại thì dùng bình thường. Kiểm thử đặt mốc thu hồi giống bước cuối của 04.00.09
/// (không chạy RESTORE thật để không ghi đè dữ liệu của các kiểm thử khác).
/// </summary>
[Collection("ThuHoiPhien")]
public class ThuHoiPhienTests
{
    private const string TenThuocTinh = "ULMS_MocThuHoiPhien";

    private static async Task ChayAsync(string sql, params (string, object)[] ts)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var (k, v) in ts) cmd.Parameters.AddWithValue(k, v);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<(HttpClient Http, string MaDangNhap)> NhanVienAsync(string ten)
    {
        var http = MoiTruong.Http();
        var res = await http.PostAsJsonAsync("/api/quan-tri/xac-thuc/dang-nhap", new { tenDangNhap = ten, matKhau = MoiTruong.MatKhauMau });
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doc.RootElement.GetProperty("token").GetString());
        return (http, doc.RootElement.GetProperty("maDangNhap").GetString()!);
    }

    private static async Task<HttpClient> BanDocAsync()
    {
        var http = MoiTruong.Http();
        var res = await http.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = "SV22000001", matKhau = MoiTruong.MatKhauBanDocMau });
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", doc.RootElement.GetProperty("token").GetString());
        return http;
    }

    private static async Task DatMocAsync(string maDangNhapMienTru) => await ChayAsync($"""
        DECLARE @v nvarchar(100) = CONCAT(DATEDIFF_BIG(SECOND, '1970-01-01', SYSUTCDATETIME()) + 1, N'|', @mt);
        IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = N'{TenThuocTinh}')
            EXEC sys.sp_updateextendedproperty @name = N'{TenThuocTinh}', @value = @v;
        ELSE
            EXEC sys.sp_addextendedproperty @name = N'{TenThuocTinh}', @value = @v;
        """, ("@mt", maDangNhapMienTru));

    [Fact]
    public async Task SauPhucHoi_HuyMoiPhienKhac_TruNguoiPhucHoi()
    {
        var (admin, maDnAdmin) = await NhanVienAsync("admin");
        var (n1, _) = await NhanVienAsync("muontra");
        var bd = await BanDocAsync();
        try
        {
            Assert.Equal(HttpStatusCode.NoContent, (await n1.GetAsync("/api/quan-tri/xac-thuc/kiem-tra-phien")).StatusCode);
            await DatMocAsync(maDnAdmin);
            await Task.Delay(TimeSpan.FromSeconds(7)); // service nhớ tạm mốc tối đa 5 giây

            // Nhân viên khác (qua nhiều service) và bạn đọc: 401 PHIEN_DA_BI_HUY
            foreach (var url in new[] { "/api/quan-tri/xac-thuc/kiem-tra-phien", "/api/quan-tri/ban-doc", "/api/quan-tri/sach" })
            {
                var r = await n1.GetAsync(url);
                Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
                Assert.Contains("PHIEN_DA_BI_HUY", await r.Content.ReadAsStringAsync());
            }
            Assert.Equal(HttpStatusCode.Unauthorized, (await bd.GetAsync("/api/quan-tri/xac-thuc/kiem-tra-phien")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await bd.GetAsync("/api/cong/tai-khoan/toi")).StatusCode);

            // Người thực hiện phục hồi vẫn làm việc tiếp
            Assert.Equal(HttpStatusCode.NoContent, (await admin.GetAsync("/api/quan-tri/xac-thuc/kiem-tra-phien")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/quan-tri/tai-khoan")).StatusCode);

            // Đăng nhập lại → dùng bình thường
            var (n1Moi, _) = await NhanVienAsync("muontra");
            Assert.Equal(HttpStatusCode.OK, (await n1Moi.GetAsync("/api/quan-tri/ban-doc")).StatusCode);
            var bdMoi = await BanDocAsync();
            Assert.Equal(HttpStatusCode.NoContent, (await bdMoi.GetAsync("/api/quan-tri/xac-thuc/kiem-tra-phien")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await bdMoi.GetAsync("/api/cong/tai-khoan/toi")).StatusCode);
        }
        finally
        {
            await ChayAsync($"""
                IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE class = 0 AND name = N'{TenThuocTinh}')
                    EXEC sys.sp_dropextendedproperty @name = N'{TenThuocTinh}';
                """);
            await Task.Delay(TimeSpan.FromSeconds(6));
            admin.Dispose(); n1.Dispose(); bd.Dispose();
        }
    }
}
