using System.Net.Http.Headers;
using System.Net.Http.Json;
using BaseCore.Common.Loi;

namespace ThuVien.Web.HaTang;

/// <summary>Kết quả gọi gateway: hoặc dữ liệu, hoặc lỗi đúng định dạng { ma, thongBao, truong }.</summary>
public sealed record KetQuaApi<T>(T? DuLieu, LoiApi? Loi, int MaHttp)
{
    public bool ThanhCong => Loi is null;
}

public sealed record DangNhapNhanVienKq(
    string Token, string MaDangNhap, string MaNd, string TenNd, string TenDangNhap,
    string MaNhom, string TenNhom, List<string> Quyen, bool PhaiDoiMatKhau, string ManHinhMacDinh);

public sealed record DangNhapBanDocKq(string Token, string MaBanDoc, string HoTen, string MaSvHvCb, string DoiTuong);

/// <summary>Gọi gateway từ phía server của web (đăng nhập, đăng xuất…). Chuyển tiếp IP người dùng.</summary>
public sealed class GatewayClient(HttpClient http, IHttpContextAccessor? accessor = null)
{
    public Task<KetQuaApi<DangNhapNhanVienKq>> DangNhapNhanVienAsync(string tenDangNhap, string matKhau, CancellationToken ct) =>
        GuiAsync<DangNhapNhanVienKq>(HttpMethod.Post, "/api/quan-tri/xac-thuc/dang-nhap", null, new { tenDangNhap, matKhau }, ct);

    public Task<KetQuaApi<object>> DangXuatNhanVienAsync(string token, CancellationToken ct) =>
        GuiAsync<object>(HttpMethod.Post, "/api/quan-tri/xac-thuc/dang-xuat", token, null, ct);

    /// <summary>D36: phiên (token) còn hiệu lực không – 401 khi đã bị thu hồi sau phục hồi dữ liệu.</summary>
    public Task<KetQuaApi<object>> KiemTraPhienAsync(string token, CancellationToken ct) =>
        GuiAsync<object>(HttpMethod.Get, "/api/quan-tri/xac-thuc/kiem-tra-phien", token, null, ct);

    public Task<KetQuaApi<object>> DoiMatKhauAsync(string token, string matKhauCu, string matKhauMoi, CancellationToken ct) =>
        GuiAsync<object>(HttpMethod.Post, "/api/quan-tri/xac-thuc/doi-mat-khau", token, new { matKhauCu, matKhauMoi }, ct);

    public Task<KetQuaApi<DangNhapBanDocKq>> DangNhapBanDocAsync(string maSvHvCb, string matKhau, CancellationToken ct) =>
        GuiAsync<DangNhapBanDocKq>(HttpMethod.Post, "/api/cong/dang-nhap", null, new { maSvHvCb, matKhau }, ct);

    /// <summary>GET bất kỳ API nào bằng token của người dùng (trang in chứng từ render phía server).</summary>
    public Task<KetQuaApi<T>> LayAsync<T>(string url, string token, CancellationToken ct) =>
        GuiAsync<T>(HttpMethod.Get, url, token, null, ct);

    private async Task<KetQuaApi<T>> GuiAsync<T>(HttpMethod method, string url, string? token, object? body, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, url);
        if (token is not null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var ip = accessor?.HttpContext?.Connection.RemoteIpAddress?.ToString();
        if (ip is not null) req.Headers.TryAddWithoutValidation("X-Forwarded-For", ip);
        if (body is not null) req.Content = JsonContent.Create(body);

        HttpResponseMessage res;
        try
        {
            res = await http.SendAsync(req, ct);
        }
        catch (HttpRequestException)
        {
            return new KetQuaApi<T>(default, new LoiApi("MAT_KET_NOI", "Không kết nối được máy chủ nghiệp vụ. Vui lòng thử lại sau."), 503);
        }

        using (res)
        {
            if (res.IsSuccessStatusCode)
            {
                var duLieu = res.Content.Headers.ContentLength is 0 || res.StatusCode == System.Net.HttpStatusCode.NoContent
                    ? default
                    : await res.Content.ReadFromJsonAsync<T>(ct);
                return new KetQuaApi<T>(duLieu, null, (int)res.StatusCode);
            }
            LoiApi? loi = null;
            try { loi = await res.Content.ReadFromJsonAsync<LoiApi>(ct); } catch { /* không phải JSON */ }
            return new KetQuaApi<T>(default, loi ?? new LoiApi("LOI_HE_THONG", "Máy chủ trả lỗi không xác định."), (int)res.StatusCode);
        }
    }
}
