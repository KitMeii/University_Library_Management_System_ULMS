using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// Pha 5 – kệ sách nổi bật ở cổng bạn đọc (file 05 mục 2) so với truy vấn kiểm chứng; quản lý đầu sách, thể loại (X01–X03,
/// GIA_DINH D55): kiểm soát dữ liệu nhập, ràng buộc xóa, gắn bản sách, quyền mở rộng DAUSACH_*/THELOAI_*.
/// </summary>
[Collection("ThongKe")]
public class Pha5ApiTests
{
    private static async Task<HttpClient> HttpCuaAsync(string ten)
    {
        var http = MoiTruong.Http();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await MoiTruong.DangNhapNhanVienAsync(http, ten));
        return http;
    }

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage r)
    {
        var t = await r.Content.ReadAsStringAsync();
        Assert.True(r.IsSuccessStatusCode, $"{(int)r.StatusCode} {t}");
        return string.IsNullOrEmpty(t) ? default : JsonDocument.Parse(t).RootElement.Clone();
    }

    private static async Task<object?> ScalarAsync(string sql, params (string, object)[] ts)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var (k, v) in ts) cmd.Parameters.AddWithValue(k, v);
        return await cmd.ExecuteScalarAsync();
    }

    private static async Task<int> DemAsync(string sql, params (string, object)[] ts) => Convert.ToInt32(await ScalarAsync(sql, ts));
    private static string Ngau(string tienTo, int doDai) => (tienTo + Guid.NewGuid().ToString("N").ToUpperInvariant())[..doDai];
    private static DateOnly HomNay => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Ho_Chi_Minh"));

    /// <summary>Bản sách có thể mượn của một đầu sách – viết độc lập (file 04 mục 4, GIA_DINH D47).</summary>
    private const string SqlCoTheMuon = """
        SELECT COUNT(*) FROM khosach.DAUSACH_BANSACH x JOIN khosach.SACH s ON s.ma_bansach = x.ma_bansach
        WHERE x.ma_dausach = @ds AND s.tinhtrang = N'Sẵn sàng'
          AND NOT EXISTS (SELECT 1 FROM muontra.CT_PHIEUMUON cm WHERE cm.ma_bansach = s.ma_bansach
                AND NOT EXISTS (SELECT 1 FROM muontra.CT_PHIEUTRA t JOIN muontra.PHIEUTRA pt ON pt.so_ptra = t.so_ptra
                                WHERE pt.so_pmuon = cm.so_pmuon AND t.ma_bansach = cm.ma_bansach))
          AND NOT EXISTS (SELECT 1 FROM muontra.PHIEUDATCHO d WHERE d.ma_bansach = s.ma_bansach AND d.trangthai = N'Đang giữ')
          AND NOT EXISTS (SELECT 1 FROM muontra.CT_PHIEUTRA t JOIN muontra.PHIEUTRA pt ON pt.so_ptra = t.so_ptra
                WHERE t.ma_bansach = s.ma_bansach AND t.tinhtrang_tra IN (N'Mất', N'Hư hỏng')
                  AND NOT EXISTS (SELECT 1 FROM khosach.XULY_CHUYENDEN k WHERE k.ma_bansach = t.ma_bansach AND k.so_chungtu = t.so_ptra AND k.tientrinh = '03.03.02')
                  AND NOT EXISTS (SELECT 1 FROM muontra.CT_PHIEUTRA t2 JOIN muontra.PHIEUTRA p2 ON p2.so_ptra = t2.so_ptra
                                  WHERE t2.ma_bansach = t.ma_bansach AND p2.ngaytra > pt.ngaytra))
        """;

    // ---------- Kệ sách (công khai) ----------
    [Fact]
    public async Task KeSach_CongKhai_DanhSachKe_KhongRong()
    {
        using var http = MoiTruong.Http(); // không đăng nhập
        var ds = (await JsonAsync(await http.GetAsync("/api/cong/ke-sach/danh-sach"))).EnumerateArray().ToList();
        Assert.Equal("moi", ds[0].GetProperty("ma").GetString());
        Assert.Contains(ds, k => k.GetProperty("ma").GetString() == "muon-nhieu");
        Assert.Contains(ds, k => k.GetProperty("ma").GetString() == "the-loai:TL01");
        Assert.All(ds, k => Assert.True(k.GetProperty("soSach").GetInt32() > 0));
        // Không hiện kệ thể loại rỗng (file 04 mục 1)
        var theLoaiCoSach = await DemAsync("SELECT COUNT(DISTINCT d.ma_theloai) FROM khosach.DAUSACH d WHERE EXISTS (SELECT 1 FROM khosach.DAUSACH_BANSACH x WHERE x.ma_dausach = d.ma_dausach)");
        Assert.Equal(theLoaiCoSach, ds.Count(k => k.GetProperty("ma").GetString()!.StartsWith("the-loai:")));
    }

    [Fact]
    public async Task KeSach_Moi_SapTheoNam_SoBanCoTheMuonKhopTruyVan()
    {
        using var http = MoiTruong.Http();
        var ke = await JsonAsync(await http.GetAsync("/api/cong/ke-sach?ke=moi"));
        Assert.Equal("nam", ke.GetProperty("truc").GetString());
        var sach = ke.GetProperty("sach").EnumerateArray().ToList();
        Assert.InRange(sach.Count, 1, 41);
        var nam = sach.Select(s => s.GetProperty("namXb").ValueKind == JsonValueKind.Null ? int.MaxValue : s.GetProperty("namXb").GetInt32()).ToList();
        Assert.Equal(nam.Order(), nam);
        foreach (var s in sach.Take(12))
            Assert.Equal(await DemAsync(SqlCoTheMuon, ("@ds", s.GetProperty("maDauSach").GetString()!)), s.GetProperty("soBanCoTheMuon").GetInt32());
        // Kệ mới = 41 đầu sách thêm gần nhất (có bản sách)
        var moiNhat = (string)(await ScalarAsync("""
            SELECT TOP 1 d.ma_dausach FROM khosach.DAUSACH d WHERE EXISTS (SELECT 1 FROM khosach.DAUSACH_BANSACH x WHERE x.ma_dausach = d.ma_dausach)
            ORDER BY d.ngaytao DESC, d.ma_dausach DESC
            """))!;
        Assert.Contains(sach, s => s.GetProperty("maDauSach").GetString() == moiNhat);
    }

    [Fact]
    public async Task KeSach_MuonNhieu_DemThat90Ngay_XepHang()
    {
        using var http = MoiTruong.Http();
        var ke = await JsonAsync(await http.GetAsync("/api/cong/ke-sach?ke=muon-nhieu"));
        Assert.Equal("hang", ke.GetProperty("truc").GetString());
        var sach = ke.GetProperty("sach").EnumerateArray().ToList();
        Assert.NotEmpty(sach);
        var tu = HomNay.AddDays(-89).ToDateTime(TimeOnly.MinValue);
        for (var i = 0; i < sach.Count; i++)
        {
            var s = sach[i];
            Assert.Equal(i + 1, s.GetProperty("hang").GetInt32());
            Assert.Equal(await DemAsync("""
                SELECT COUNT(*) FROM muontra.CT_PHIEUMUON cm JOIN khosach.DAUSACH_BANSACH x ON x.ma_bansach = cm.ma_bansach
                WHERE x.ma_dausach = @ds AND cm.ngaymuon >= @tu
                """, ("@ds", s.GetProperty("maDauSach").GetString()!), ("@tu", tu)), s.GetProperty("soLuotMuon").GetInt32());
            if (i > 0) Assert.True(sach[i - 1].GetProperty("soLuotMuon").GetInt32() >= s.GetProperty("soLuotMuon").GetInt32());
            Assert.True(s.GetProperty("soLuotMuon").GetInt32() > 0);
        }
    }

    [Fact]
    public async Task KeSach_TheLoai_VaThamSoSai()
    {
        using var http = MoiTruong.Http();
        var ke = await JsonAsync(await http.GetAsync("/api/cong/ke-sach?ke=the-loai:TL01"));
        Assert.Equal("Công nghệ thông tin", ke.GetProperty("ten").GetString());
        Assert.All(ke.GetProperty("sach").EnumerateArray(), s => Assert.Equal("TL01", s.GetProperty("maTheLoai").GetString()));
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync("/api/cong/ke-sach?ke=the-loai:KHONGCO")).StatusCode);
        Assert.Equal((HttpStatusCode)422, (await http.GetAsync("/api/cong/ke-sach?ke=bat-ky")).StatusCode);
    }

    // ---------- Quản lý đầu sách ----------
    [Fact]
    public async Task DauSach_Them_KiemSoatNhap_GanBanSach_Xoa()
    {
        using var n3 = await HttpCuaAsync("khosach");
        using var n4 = await HttpCuaAsync("admin");
        var ma = Ngau("DK", 10);
        var hop = new { maDauSach = ma, tenSach = "Sách kiểm thử Pha 5", tacGia = "Tác giả thử", nhaXuatBan = "NXB Thử", namXb = 2020,
            isbn = (string?)null, maTheLoai = "TL01", gioiThieu = "Giới thiệu", tuKhoa = "kiểm thử", anhBia = (string?)null, mauGay = "#8B2E2E" };

        // Kiểm soát dữ liệu nhập: năm, ISBN, màu gáy, tên rỗng, thể loại không có, ảnh bìa ngoài thư mục bìa
        var sai = await n3.PostAsJsonAsync("/api/quan-tri/dau-sach", hop with { namXb = 1800, mauGay = "đỏ", tenSach = "", maTheLoai = "KHONGCO", anhBia = "/etc/passwd" });
        Assert.Equal((HttpStatusCode)422, sai.StatusCode);
        var loi = (await sai.Content.ReadAsStringAsync());
        foreach (var t in new[] { "namXb", "mauGay", "tenSach", "maTheLoai", "anhBia" }) Assert.Contains(t, loi);

        var ds = await JsonAsync(await n3.PostAsJsonAsync("/api/quan-tri/dau-sach", hop));
        Assert.Equal(ma, ds.GetProperty("maDauSach").GetString());
        Assert.Equal(0, ds.GetProperty("soBanSach").GetInt32());
        Assert.Equal(1, await DemAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung LIKE @n", ("@n", $"Thêm đầu sách {ma}%")));
        // Trùng mã → 422
        Assert.Equal((HttpStatusCode)422, (await n3.PostAsJsonAsync("/api/quan-tri/dau-sach", hop)).StatusCode);

        // Gắn bản sách mới (chưa gắn đầu sách nào) – D55
        var maBs = Ngau("BK", 10);
        await JsonAsync(await n3.PostAsJsonAsync("/api/quan-tri/sach", new { maBanSach = maBs, maKhuVuc = "KV01", tenSach = "Sách kiểm thử Pha 5", tacGia = "Tác giả thử", tinhTrang = "Sẵn sàng" }));
        var chuaGan = (await JsonAsync(await n3.GetAsync("/api/quan-tri/dau-sach/ban-sach-chua-gan?tuKhoa=" + maBs))).EnumerateArray().ToList();
        Assert.Single(chuaGan);
        Assert.Equal(1, (await JsonAsync(await n3.PostAsJsonAsync($"/api/quan-tri/dau-sach/{ma}/ban-sach", new { maBanSach = new[] { maBs } }))).GetProperty("soGan").GetInt32());
        // Bản sách đã gắn đầu sách khác → 409
        Assert.Equal(HttpStatusCode.Conflict, (await n3.PostAsJsonAsync("/api/quan-tri/dau-sach/DS0001/ban-sach", new { maBanSach = new[] { maBs } })).StatusCode);
        var sauGan = await JsonAsync(await n3.GetAsync($"/api/quan-tri/dau-sach/{ma}"));
        Assert.Equal(1, sauGan.GetProperty("soBanSach").GetInt32());
        Assert.Equal(1, sauGan.GetProperty("soBanCoTheMuon").GetInt32());
        // Cổng: đầu sách mới nhất có mặt ở kệ “Sách mới về” với 1 bản có thể mượn
        using var cong = MoiTruong.Http();
        var keMoi = await JsonAsync(await cong.GetAsync("/api/cong/ke-sach?ke=moi"));
        Assert.Contains(keMoi.GetProperty("sach").EnumerateArray(), s => s.GetProperty("maDauSach").GetString() == ma && s.GetProperty("soBanCoTheMuon").GetInt32() == 1);

        // Xóa: N3 không có quyền D; N4 bị chặn khi còn bản sách gắn → bỏ gắn → xóa được
        Assert.Equal(HttpStatusCode.Forbidden, (await n3.DeleteAsync($"/api/quan-tri/dau-sach/{ma}")).StatusCode);
        var chan = await n4.DeleteAsync($"/api/quan-tri/dau-sach/{ma}");
        Assert.Equal(HttpStatusCode.Conflict, chan.StatusCode);
        Assert.Contains("DAU_SACH_CON_BAN_SACH", await chan.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NoContent, (await n3.DeleteAsync($"/api/quan-tri/dau-sach/{ma}/ban-sach/{maBs}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await n4.DeleteAsync($"/api/quan-tri/dau-sach/{ma}")).StatusCode);
        Assert.Equal(0, await DemAsync("SELECT COUNT(*) FROM khosach.DAUSACH WHERE ma_dausach = @m", ("@m", ma)));
        Assert.Equal(HttpStatusCode.NoContent, (await n4.DeleteAsync($"/api/quan-tri/sach/{maBs}")).StatusCode);
    }

    [Fact]
    public async Task DauSach_TrungIsbn_VaSua()
    {
        using var n3 = await HttpCuaAsync("khosach");
        var isbnDs1 = (string)(await ScalarAsync("SELECT isbn FROM khosach.DAUSACH WHERE ma_dausach = 'DS0001'"))!;
        var ma = Ngau("DI", 10);
        var r = await n3.PostAsJsonAsync("/api/quan-tri/dau-sach", new { maDauSach = ma, tenSach = "Trùng ISBN", isbn = isbnDs1 });
        Assert.Equal((HttpStatusCode)422, r.StatusCode);
        Assert.Contains("isbn", await r.Content.ReadAsStringAsync());
        await JsonAsync(await n3.PostAsJsonAsync("/api/quan-tri/dau-sach", new { maDauSach = ma, tenSach = "Bản đầu" }));
        var sua = await JsonAsync(await n3.PutAsJsonAsync($"/api/quan-tri/dau-sach/{ma}", new { tenSach = "Bản đã sửa", namXb = 2001, mauGay = "#123f35" }));
        Assert.Equal("Bản đã sửa", sua.GetProperty("tenSach").GetString());
        Assert.Equal("#123F35", sua.GetProperty("mauGay").GetString());
        using var n4 = await HttpCuaAsync("admin");
        Assert.Equal(HttpStatusCode.NoContent, (await n4.DeleteAsync($"/api/quan-tri/dau-sach/{ma}")).StatusCode);
    }

    // ---------- Quản lý thể loại ----------
    [Fact]
    public async Task TheLoai_Them_TrungTen_XoaKhiConDauSach()
    {
        using var n3 = await HttpCuaAsync("khosach");
        using var n4 = await HttpCuaAsync("admin");
        var ma = Ngau("TK", 8);
        await JsonAsync(await n3.PostAsJsonAsync("/api/quan-tri/the-loai", new { maTheLoai = ma, tenTheLoai = "Thể loại thử " + ma, bieuTuong = "bi-flask", thuTuHienThi = 99 }));
        Assert.Equal((HttpStatusCode)422, (await n3.PostAsJsonAsync("/api/quan-tri/the-loai", new { maTheLoai = Ngau("TK", 8), tenTheLoai = "Công nghệ thông tin" })).StatusCode);
        Assert.Equal((HttpStatusCode)422, (await n3.PostAsJsonAsync("/api/quan-tri/the-loai", new { maTheLoai = Ngau("TK", 8), tenTheLoai = "X", bieuTuong = "<script>" })).StatusCode);
        var conDauSach = await n4.DeleteAsync("/api/quan-tri/the-loai/TL01");
        Assert.Equal(HttpStatusCode.Conflict, conDauSach.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n3.DeleteAsync($"/api/quan-tri/the-loai/{ma}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await n4.DeleteAsync($"/api/quan-tri/the-loai/{ma}")).StatusCode);
    }

    [Fact]
    public async Task Quyen_MoRong_ChiN3N4()
    {
        using var n1 = await HttpCuaAsync("muontra");
        using var n2 = await HttpCuaAsync("vipham");
        foreach (var h in new[] { n1, n2 })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await h.GetAsync("/api/quan-tri/dau-sach")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await h.GetAsync("/api/quan-tri/the-loai")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await h.PostAsJsonAsync("/api/quan-tri/dau-sach/DS0001/ban-sach", new { maBanSach = new[] { "BS00001" } })).StatusCode);
        }
        using var anDanh = MoiTruong.Http();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anDanh.GetAsync("/api/quan-tri/dau-sach")).StatusCode);
    }
}
