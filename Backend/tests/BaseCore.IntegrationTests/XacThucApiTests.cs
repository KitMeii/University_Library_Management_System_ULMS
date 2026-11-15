using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BaseCore.Common.Auth;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// Hợp đồng gateway ↔ service: SPEC 6.3.1 (đăng nhập, khóa 5 lần, NHATKY_DANGNHAP, quyền theo nhóm),
/// file 07 (đăng nhập bạn đọc, khóa tạm 15 phút), file 11 (API ngoài quyền → 403, token bạn đọc ↔ nhân viên tách biệt).
/// </summary>
public class XacThucApiTests
{
    private static async Task<JsonElement> DangNhapAsync(HttpClient http, string ten, string mk = MoiTruong.MatKhauMau)
    {
        var res = await http.PostAsJsonAsync("/api/quan-tri/xac-thuc/dang-nhap", new { tenDangNhap = ten, matKhau = mk });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        return JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static async Task<HttpStatusCode> GoiAsync(HttpClient http, string url, string? token)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        if (token is not null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var res = await http.SendAsync(req);
        return res.StatusCode;
    }

    [Theory]
    [InlineData("muontra", "N1", "/quan-tri/phieu-muon/lap")]
    [InlineData("vipham", "N2", "/quan-tri/bien-ban/lap")]
    [InlineData("khosach", "N3", "/quan-tri/danh-muc/sach")]
    [InlineData("admin", "N4", "/quan-tri/tai-khoan")]
    public async Task DangNhap_DungNhom_ManHinhMacDinh_GhiNhatKy(string ten, string nhom, string manHinh)
    {
        using var http = MoiTruong.Http();
        var kq = await DangNhapAsync(http, ten);
        Assert.Equal(nhom, kq.GetProperty("maNhom").GetString());
        Assert.Equal(manHinh, kq.GetProperty("manHinhMacDinh").GetString());
        var ma = kq.GetProperty("maDangNhap").GetString()!;
        Assert.Matches(@"^DN\d{13}$", ma); // 15 ký tự (GIA_DINH D19)

        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand("SELECT ma_nd FROM quantri.NHATKY_DANGNHAP WHERE ma_dangnhap = @ma", cn);
        cmd.Parameters.AddWithValue("@ma", ma);
        Assert.NotNull(await cmd.ExecuteScalarAsync());
    }

    [Fact]
    public async Task DangXuat_GhiTgDangXuat()
    {
        using var http = MoiTruong.Http();
        var kq = await DangNhapAsync(http, "khosach");
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/quan-tri/xac-thuc/dang-xuat");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", kq.GetProperty("token").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await http.SendAsync(req)).StatusCode);

        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand("SELECT tg_dangxuat FROM quantri.NHATKY_DANGNHAP WHERE ma_dangnhap = @ma", cn);
        cmd.Parameters.AddWithValue("@ma", kq.GetProperty("maDangNhap").GetString());
        Assert.IsType<DateTime>(await cmd.ExecuteScalarAsync());
    }

    [Theory]
    // (tài khoản, API, mã HTTP mong đợi) – theo PHANQUYEN nạp từ SPEC 3.2
    [InlineData("muontra", "/api/quan-tri/khoan-thu", 403)]
    [InlineData("vipham", "/api/quan-tri/khoan-thu", 200)]
    [InlineData("khosach", "/api/quan-tri/khoan-thu", 403)]
    [InlineData("admin", "/api/quan-tri/khoan-thu", 200)]
    [InlineData("muontra", "/api/quan-tri/khu-vuc", 200)]
    [InlineData("vipham", "/api/quan-tri/khu-vuc", 403)]
    [InlineData("khosach", "/api/quan-tri/khu-vuc", 200)]
    [InlineData("muontra", "/api/quan-tri/ban-doc", 200)]
    [InlineData("khosach", "/api/quan-tri/ban-doc", 403)]
    [InlineData("vipham", "/api/quan-tri/ban-doc", 200)]
    [InlineData("muontra", "/api/quan-tri/nhom-nguoi-dung", 403)]
    [InlineData("admin", "/api/quan-tri/nhom-nguoi-dung", 200)]
    [InlineData("muontra", "/api/cong/tai-khoan/toi", 403)]
    public async Task ApiTheoQuyen(string ten, string url, int mong)
    {
        using var http = MoiTruong.Http();
        var token = (await DangNhapAsync(http, ten)).GetProperty("token").GetString();
        Assert.Equal((HttpStatusCode)mong, await GoiAsync(http, url, token));
    }

    [Theory]
    [InlineData("/api/quan-tri/khu-vuc")]
    [InlineData("/api/quan-tri/khoan-thu")]
    [InlineData("/api/cong/tai-khoan/toi")]
    public async Task KhongCoToken_401(string url)
    {
        using var http = MoiTruong.Http();
        Assert.Equal(HttpStatusCode.Unauthorized, await GoiAsync(http, url, null));
    }

    [Fact]
    public async Task TokenBanDoc_KhongVaoDuocApiQuanTri()
    {
        using var http = MoiTruong.Http();
        var res = await http.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = "SV22000001", matKhau = MoiTruong.MatKhauBanDocMau });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var token = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString();
        Assert.Equal(HttpStatusCode.OK, await GoiAsync(http, "/api/cong/tai-khoan/toi", token));
        Assert.Equal(HttpStatusCode.Forbidden, await GoiAsync(http, "/api/quan-tri/khu-vuc", token));
        Assert.Equal(HttpStatusCode.Forbidden, await GoiAsync(http, "/api/quan-tri/ban-doc", token));
    }

    [Fact]
    public async Task SaiMatKhau_ThongBaoDungSpec_KhongLoTaiKhoan()
    {
        using var http = MoiTruong.Http();
        var r1 = await http.PostAsJsonAsync("/api/quan-tri/xac-thuc/dang-nhap", new { tenDangNhap = "khong-ton-tai", matKhau = "x1234567" });
        Assert.Equal(HttpStatusCode.Unauthorized, r1.StatusCode);
        var loi = JsonDocument.Parse(await r1.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("Tên đăng nhập hoặc mật khẩu không đúng", loi.GetProperty("thongBao").GetString());
        Assert.Equal("SAI_THONG_TIN", loi.GetProperty("ma").GetString());
    }

    /// <summary>SPEC 6.3.1: sai 5 lần liên tiếp → NGUOIDUNG.tinhtrang = 'Khóa'. Dùng tài khoản tạm.</summary>
    [Fact]
    public async Task Sai5Lan_KhoaTaiKhoan()
    {
        var ten = "kt" + Random.Shared.Next(100000, 999999);
        var maNd = "NDT" + ten[^6..];
        var soVong = int.Parse(Environment.GetEnvironmentVariable("MATKHAU_SO_VONG_LAP") ?? "600000");
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using (var them = new SqlCommand("""
            INSERT INTO quantri.NGUOIDUNG (ma_nd, ten_nd, tendangnhap, matkhau, tinhtrang, ma_nhom)
            VALUES (@ma, N'Tài khoản kiểm thử', @ten, @mk, N'Hoạt động', 'N3')
            """, cn))
        {
            them.Parameters.AddWithValue("@ma", maNd);
            them.Parameters.AddWithValue("@ten", ten);
            them.Parameters.AddWithValue("@mk", new MatKhauHasher(soVong).Bam("Dung12345"));
            await them.ExecuteNonQueryAsync();
        }
        try
        {
            using var http = MoiTruong.Http();
            for (var i = 1; i <= 4; i++)
            {
                var r = await http.PostAsJsonAsync("/api/quan-tri/xac-thuc/dang-nhap", new { tenDangNhap = ten, matKhau = "Sai" + i + "xxxxx" });
                Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
            }
            var r5 = await http.PostAsJsonAsync("/api/quan-tri/xac-thuc/dang-nhap", new { tenDangNhap = ten, matKhau = "Sai5xxxxx" });
            Assert.Equal(HttpStatusCode.Forbidden, r5.StatusCode);
            // Đã khóa: kể cả đúng mật khẩu cũng không vào được
            var rDung = await http.PostAsJsonAsync("/api/quan-tri/xac-thuc/dang-nhap", new { tenDangNhap = ten, matKhau = "Dung12345" });
            Assert.Equal(HttpStatusCode.Forbidden, rDung.StatusCode);
            Assert.Equal("TAI_KHOAN_KHOA", JsonDocument.Parse(await rDung.Content.ReadAsStringAsync()).RootElement.GetProperty("ma").GetString());

            await using var tt = new SqlCommand("SELECT tinhtrang FROM quantri.NGUOIDUNG WHERE ma_nd = @ma", cn);
            tt.Parameters.AddWithValue("@ma", maNd);
            Assert.Equal("Khóa", await tt.ExecuteScalarAsync());
        }
        finally
        {
            await using var xoa = new SqlCommand("""
                DELETE FROM quantri.NHATKY_HOATDONG WHERE ma_nd = @ma;
                DELETE FROM quantri.NHATKY_DANGNHAP WHERE ma_nd = @ma;
                DELETE FROM quantri.NGUOIDUNG WHERE ma_nd = @ma;
                """, cn);
            xoa.Parameters.AddWithValue("@ma", maNd);
            await xoa.ExecuteNonQueryAsync();
        }
    }

    /// <summary>File 07: bạn đọc sai 5 lần → tạm khóa 15 phút (tg_khoa_den).</summary>
    [Fact]
    public async Task BanDoc_Sai5Lan_TamKhoa15Phut()
    {
        const string ma = "SV22000009"; // BD0009 – tài khoản mẫu đã kích hoạt
        using var http = MoiTruong.Http();
        try
        {
            for (var i = 1; i <= 4; i++)
                Assert.Equal(HttpStatusCode.Unauthorized,
                    (await http.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = ma, matKhau = "sai" + i + "xxxx" })).StatusCode);
            var r5 = await http.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = ma, matKhau = "sai5xxxx" });
            Assert.Equal(HttpStatusCode.Forbidden, r5.StatusCode);
            var rDung = await http.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = ma, matKhau = MoiTruong.MatKhauBanDocMau });
            Assert.Equal("TAM_KHOA", JsonDocument.Parse(await rDung.Content.ReadAsStringAsync()).RootElement.GetProperty("ma").GetString());
        }
        finally
        {
            await using var cn = MoiTruong.KetNoiSa();
            await cn.OpenAsync();
            await using var mo = new SqlCommand(
                "UPDATE muontra.TAIKHOAN_BANDOC SET so_lan_sai = 0, tg_khoa_den = NULL WHERE ma_bandoc = 'BD0009'", cn);
            await mo.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task QuyenTrongToken_DungPhanQuyenCuaNhom()
    {
        using var http = MoiTruong.Http();
        var kq = await DangNhapAsync(http, "muontra");
        var quyen = kq.GetProperty("quyen").EnumerateArray().Select(x => x.GetString()).ToHashSet();
        Assert.Contains("TT012", quyen);   // Mở form Phiếu mượn sách (N1: A)
        Assert.DoesNotContain("TT074", quyen); // Mở form Biên bản vi phạm (N1: nA)
        Assert.Contains("SACH_R", quyen);
        Assert.DoesNotContain("SACH_C", quyen);
    }
}
