using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// Pha 8 – GIA_DINH D82 (chủ dự án duyệt 18/11/2026): XEM TRƯỚC báo cáo MB06/MB07 phân trang bảng chi tiết (mặc định 50 dòng/trang);
/// số liệu tổng hợp và biểu đồ vẫn tính toàn kỳ; BẢN IN (/in) đủ toàn bộ dòng. Dùng cỡ trang nhỏ để dữ liệu mẫu có nhiều trang.
/// </summary>
[Collection("ThongKe")]
public class Pha8ApiTests
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
        return JsonDocument.Parse(t).RootElement.Clone();
    }

    private static async Task<int> DemAsync(string sql, params (string, object)[] ts)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var (k, v) in ts) cmd.Parameters.AddWithValue(k, v);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private static DateOnly HomNay => DateOnly.FromDateTime(GioVietNam.HomNay);
    private static string Iso(DateOnly d) => d.ToString("yyyy-MM-dd");

    /// <summary>Đọc lần lượt mọi trang của một bảng chi tiết; kiểm tra tổng số không đổi giữa các trang và mỗi trang đúng cỡ.</summary>
    private static async Task<List<string>> DocMoiTrangAsync(HttpClient http, string url, int coTrang, int tongMongDoi, Func<JsonElement, string> khoa)
    {
        var tatCa = new List<string>();
        var soTrang = Math.Max(1, (int)Math.Ceiling(tongMongDoi / (double)coTrang));
        for (var t = 1; t <= soTrang + 1; t++) // đọc thêm 1 trang sau trang cuối: phải rỗng
        {
            var p = await JsonAsync(await http.GetAsync($"{url}&trang={t}&coTrang={coTrang}"));
            Assert.Equal(tongMongDoi, p.GetProperty("tongSo").GetInt32()); // tổng không đổi giữa các trang
            Assert.Equal(t, p.GetProperty("trang").GetInt32());
            var dong = p.GetProperty("duLieu").EnumerateArray().ToList();
            var mongDoi = t < soTrang ? coTrang : t == soTrang ? tongMongDoi - (soTrang - 1) * coTrang : 0;
            Assert.Equal(mongDoi, dong.Count);
            tatCa.AddRange(dong.Select(khoa));
        }
        return tatCa;
    }

    private static string K(JsonElement x, params string[] ten) => string.Join("|", ten.Select(t => x.GetProperty(t).ToString()));

    [Fact]
    public async Task MB06_XemTruoc_PhanTrang_TongKhongDoi_BanInDuDong()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var den = HomNay;
        var tu = den.AddDays(-365);
        var ky = $"tuNgay={Iso(tu)}&denNgay={Iso(den)}";
        const int coTrang = 4;

        var mau = await JsonAsync(await n1.GetAsync($"/api/quan-tri/bao-cao/muon-tra?{ky}"));          // cỡ trang mặc định
        var nho = await JsonAsync(await n1.GetAsync($"/api/quan-tri/bao-cao/muon-tra?{ky}&coTrang={coTrang}"));
        var banIn = await JsonAsync(await n1.GetAsync($"/api/quan-tri/bao-cao/muon-tra/in?{ky}"));
        Assert.Equal(50, mau.GetProperty("coTrang").GetInt32());

        // Số liệu tổng hợp + biểu đồ tính trên TOÀN kỳ – không phụ thuộc cỡ trang, giống bản in
        foreach (var ten in new[] { "tongLuotMuon", "tongLuotGiaHan", "tongLuotTra", "soSachDangMuon", "soSachQuaHan",
                     "soDongLuotMuon", "soDongLuotGiaHan", "soDongLuotTra", "soDongSachQuaHan" })
        {
            Assert.Equal(banIn.GetProperty(ten).GetInt32(), mau.GetProperty(ten).GetInt32());
            Assert.Equal(banIn.GetProperty(ten).GetInt32(), nho.GetProperty(ten).GetInt32());
        }
        Assert.Equal(banIn.GetProperty("theoNgay").ToString(), nho.GetProperty("theoNgay").ToString());

        // Bản in đủ dòng = truy vấn kiểm chứng
        (string, object)[] ts = [("@tu", tu.ToDateTime(TimeOnly.MinValue)), ("@den", den.ToDateTime(TimeOnly.MinValue))];
        var muon = await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUMUON WHERE ngaymuon BETWEEN @tu AND @den", ts);
        var tra = await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUTRA x JOIN muontra.PHIEUTRA pt ON pt.so_ptra = x.so_ptra WHERE pt.ngaytra BETWEEN @tu AND @den", ts);
        var giaHan = await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUGIAHAN cg JOIN muontra.PHIEUGIAHAN pg ON pg.so_pgiahan = cg.so_pgiahan WHERE pg.ngaylap_pgiahan BETWEEN @tu AND @den", ts);
        Assert.True(muon > coTrang, $"Dữ liệu mẫu cần > {coTrang} lượt mượn trong 365 ngày để có nhiều trang (có {muon})");
        Assert.Equal(muon, banIn.GetProperty("luotMuon").GetArrayLength());
        Assert.Equal(tra, banIn.GetProperty("luotTra").GetArrayLength());
        Assert.Equal(giaHan, banIn.GetProperty("luotGiaHan").GetArrayLength());
        Assert.Equal(banIn.GetProperty("soSachQuaHan").GetInt32(), banIn.GetProperty("sachQuaHan").GetArrayLength());

        // Mỗi bảng: trang đầu trong kết quả tổng hợp = trang 1 của /chi-tiet; ghép mọi trang = đúng thứ tự bản in
        var bang = new (string Bang, string TruongDs, string TruongSo, string[] Khoa)[]
        {
            ("luot-muon", "luotMuon", "soDongLuotMuon", ["soPMuon", "maBanSach"]),
            ("luot-gia-han", "luotGiaHan", "soDongLuotGiaHan", ["soPGiaHan", "maBanSach"]),
            ("luot-tra", "luotTra", "soDongLuotTra", ["soPTra", "maBanSach"]),
            ("sach-qua-han", "sachQuaHan", "soDongSachQuaHan", ["soPMuon", "maBanSach"]),
        };
        foreach (var (b, ds, soDong, khoa) in bang)
        {
            var tong = nho.GetProperty(soDong).GetInt32();
            var trangDau = nho.GetProperty(ds).EnumerateArray().Select(x => K(x, khoa)).ToList();
            Assert.Equal(Math.Min(coTrang, tong), trangDau.Count);
            var moiTrang = await DocMoiTrangAsync(n1, $"/api/quan-tri/bao-cao/muon-tra/chi-tiet/{b}?{ky}", coTrang, tong, x => K(x, khoa));
            Assert.Equal(trangDau, moiTrang.Take(trangDau.Count));
            Assert.Equal(banIn.GetProperty(ds).EnumerateArray().Select(x => K(x, khoa)).ToList(), moiTrang);
        }

        // Kiểm soát: bảng không hợp lệ → 422; nhóm khác không có quyền → 403 (cả /chi-tiet và /in)
        Assert.Equal((HttpStatusCode)422, (await n1.GetAsync($"/api/quan-tri/bao-cao/muon-tra/chi-tiet/khong-co?{ky}")).StatusCode);
        using var n2 = await HttpCuaAsync("vipham");
        Assert.Equal(HttpStatusCode.Forbidden, (await n2.GetAsync($"/api/quan-tri/bao-cao/muon-tra/chi-tiet/luot-muon?{ky}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n2.GetAsync($"/api/quan-tri/bao-cao/muon-tra/in?{ky}")).StatusCode);
    }

    [Fact]
    public async Task MB07_XemTruoc_PhanTrang_TongKhongDoi_BanInDuDong()
    {
        using var n2 = await HttpCuaAsync("vipham");
        var den = HomNay;
        var tu = den.AddDays(-365);
        var ky = $"tuNgay={Iso(tu)}&denNgay={Iso(den)}";
        const int coTrang = 2;

        var mau = await JsonAsync(await n2.GetAsync($"/api/quan-tri/bao-cao/vi-pham?{ky}"));
        var nho = await JsonAsync(await n2.GetAsync($"/api/quan-tri/bao-cao/vi-pham?{ky}&coTrang={coTrang}"));
        var banIn = await JsonAsync(await n2.GetAsync($"/api/quan-tri/bao-cao/vi-pham/in?{ky}"));
        Assert.Equal(50, mau.GetProperty("coTrang").GetInt32());
        foreach (var ten in new[] { "soBienBan", "soTruongHop", "soDongBienBan" })
            Assert.Equal(banIn.GetProperty(ten).GetInt32(), nho.GetProperty(ten).GetInt32());
        foreach (var ten in new[] { "tongTienViPham", "tongTienDaThu", "tongTienChuaThu" })
            Assert.Equal(banIn.GetProperty(ten).GetDecimal(), nho.GetProperty(ten).GetDecimal());
        Assert.Equal(banIn.GetProperty("theoKhoanThu").ToString(), nho.GetProperty("theoKhoanThu").ToString());
        Assert.Equal(banIn.GetProperty("theoHinhThuc").ToString(), nho.GetProperty("theoHinhThuc").ToString());

        (string, object)[] ts = [("@tu", tu.ToDateTime(TimeOnly.MinValue)), ("@sau", den.AddDays(1).ToDateTime(TimeOnly.MinValue))];
        var soBb = await DemAsync("SELECT COUNT(*) FROM vipham.BIENBAN WHERE thoigian_lap >= @tu AND thoigian_lap < @sau", ts);
        Assert.True(soBb > coTrang, $"Dữ liệu mẫu cần > {coTrang} biên bản để có nhiều trang (có {soBb})");
        Assert.Equal(soBb, banIn.GetProperty("bienBan").GetArrayLength());   // bản in đủ dòng
        Assert.Equal(soBb, nho.GetProperty("soDongBienBan").GetInt32());
        Assert.Equal(coTrang, nho.GetProperty("bienBan").GetArrayLength());

        var moiTrang = await DocMoiTrangAsync(n2, $"/api/quan-tri/bao-cao/vi-pham/chi-tiet/bien-ban?{ky}", coTrang, soBb, x => K(x, "soBienBan"));
        Assert.Equal(banIn.GetProperty("bienBan").EnumerateArray().Select(x => K(x, "soBienBan")).ToList(), moiTrang);

        Assert.Equal((HttpStatusCode)422, (await n2.GetAsync($"/api/quan-tri/bao-cao/vi-pham/chi-tiet/khong-co?{ky}")).StatusCode);
        using var n1 = await HttpCuaAsync("muontra");
        Assert.Equal(HttpStatusCode.Forbidden, (await n1.GetAsync($"/api/quan-tri/bao-cao/vi-pham/chi-tiet/bien-ban?{ky}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n1.GetAsync($"/api/quan-tri/bao-cao/vi-pham/in?{ky}")).StatusCode);
    }
}
