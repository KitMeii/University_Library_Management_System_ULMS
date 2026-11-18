using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>Chạy riêng (không song song): số liệu thống kê phải khớp truy vấn kiểm chứng tại cùng thời điểm.</summary>
[CollectionDefinition("ThongKe", DisableParallelization = true)]
public sealed class BoThongKe;

/// <summary>
/// Pha 4 – báo cáo MB06/MB07 (SPEC 6.4.5–6.4.6) và tổng quan N1–N4 (file 09): mỗi API so với truy vấn kiểm chứng viết độc lập
/// bằng SQL trên CSDL kiểm thử; kiểm soát kỳ báo cáo (422), quyền theo SPEC 3.4 (403), ghi nhật ký khi lập báo cáo.
/// </summary>
[Collection("ThongKe")]
public class Pha4ApiTests
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

    private static async Task<object?> ScalarAsync(string sql, params (string, object)[] ts)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var (k, v) in ts) cmd.Parameters.AddWithValue(k, v);
        return await cmd.ExecuteScalarAsync();
    }

    private static async Task<int> DemAsync(string sql, params (string, object)[] ts) => Convert.ToInt32(await ScalarAsync(sql, ts));
    private static async Task<decimal> TienAsync(string sql, params (string, object)[] ts) =>
        await ScalarAsync(sql, ts) is { } v and not DBNull ? Convert.ToDecimal(v) : 0m;

    /// <summary>Hôm nay theo giờ Việt Nam (IClock của service) – container kiểm thử chạy giờ UTC.</summary>
    private static DateOnly HomNay => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Ho_Chi_Minh"));
    private static string Iso(DateOnly d) => d.ToString("yyyy-MM-dd");

    // ---- Truy vấn kiểm chứng (viết độc lập với code service) ----
    private const string SqlChuaTra = """
        FROM muontra.CT_PHIEUMUON cm
        WHERE cm.ngaymuon <= @den
          AND NOT EXISTS (SELECT 1 FROM muontra.CT_PHIEUTRA x JOIN muontra.PHIEUTRA pt ON pt.so_ptra = x.so_ptra
                          WHERE pt.so_pmuon = cm.so_pmuon AND x.ma_bansach = cm.ma_bansach AND pt.ngaytra <= @den)
        """;
    private const string SqlHanHienHanh = """
        (SELECT MAX(h) FROM (VALUES (cm.hantra), ((SELECT MAX(cg.hantra_moi) FROM muontra.CT_PHIEUGIAHAN cg
            JOIN muontra.PHIEUGIAHAN pg ON pg.so_pgiahan = cg.so_pgiahan
            WHERE pg.so_pmuon = cm.so_pmuon AND cg.ma_bansach = cm.ma_bansach AND cg.ketqua_giahan = N'Đồng ý' AND pg.ngaylap_pgiahan <= @den))) v(h))
        """;

    // ---------- MB06 ----------
    [Fact]
    public async Task MB06_KhopTruyVanKiemChung()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var den = HomNay;
        var tu = den.AddDays(-59);
        var d = await JsonAsync(await n1.GetAsync($"/api/quan-tri/bao-cao/muon-tra?tuNgay={Iso(tu)}&denNgay={Iso(den)}"));
        (string, object)[] ky = [("@tu", tu.ToDateTime(TimeOnly.MinValue)), ("@den", den.ToDateTime(TimeOnly.MinValue))];

        var muon = await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUMUON WHERE ngaymuon BETWEEN @tu AND @den", ky);
        var giaHan = await DemAsync("""
            SELECT COUNT(*) FROM muontra.CT_PHIEUGIAHAN cg JOIN muontra.PHIEUGIAHAN pg ON pg.so_pgiahan = cg.so_pgiahan
            WHERE cg.ketqua_giahan = N'Đồng ý' AND pg.ngaylap_pgiahan BETWEEN @tu AND @den
            """, ky);
        var tra = await DemAsync("""
            SELECT COUNT(*) FROM muontra.CT_PHIEUTRA x JOIN muontra.PHIEUTRA pt ON pt.so_ptra = x.so_ptra WHERE pt.ngaytra BETWEEN @tu AND @den
            """, ky);
        var dangMuon = await DemAsync("SELECT COUNT(*) " + SqlChuaTra, ky);
        var quaHan = await DemAsync($"SELECT COUNT(*) {SqlChuaTra} AND {SqlHanHienHanh} < @den", ky);

        Assert.True(muon > 0, "Dữ liệu mẫu phải có lượt mượn trong 60 ngày");
        Assert.Equal(muon, d.GetProperty("tongLuotMuon").GetInt32());
        Assert.Equal(giaHan, d.GetProperty("tongLuotGiaHan").GetInt32());
        Assert.Equal(tra, d.GetProperty("tongLuotTra").GetInt32());
        Assert.Equal(dangMuon, d.GetProperty("soSachDangMuon").GetInt32());
        Assert.Equal(quaHan, d.GetProperty("soSachQuaHan").GetInt32());
        Assert.True(quaHan > 0, "Dữ liệu mẫu có bạn đọc đang mượn quá hạn");

        // Chi tiết và biểu đồ theo ngày khớp các tổng
        var theoNgay = d.GetProperty("theoNgay").EnumerateArray().ToList();
        Assert.Equal(60, theoNgay.Count);
        Assert.Equal(muon, theoNgay.Sum(x => x.GetProperty("muon").GetInt32()));
        Assert.Equal(giaHan, theoNgay.Sum(x => x.GetProperty("giaHan").GetInt32()));
        Assert.Equal(tra, theoNgay.Sum(x => x.GetProperty("tra").GetInt32()));
        // Xem trước (D82): tổng số dòng của từng bảng chi tiết = truy vấn kiểm chứng; trang đầu tối đa 50 dòng
        Assert.Equal(muon, d.GetProperty("soDongLuotMuon").GetInt32());
        Assert.Equal(Math.Min(50, muon), d.GetProperty("luotMuon").GetArrayLength());
        Assert.Equal(tra, d.GetProperty("soDongLuotTra").GetInt32());
        Assert.Equal(Math.Min(50, tra), d.GetProperty("luotTra").GetArrayLength());
        Assert.Equal(quaHan, d.GetProperty("soDongSachQuaHan").GetInt32());
        Assert.Equal(Math.Min(50, quaHan), d.GetProperty("sachQuaHan").GetArrayLength());
        Assert.All(d.GetProperty("sachQuaHan").EnumerateArray(), x => Assert.True(x.GetProperty("soNgayQuaHan").GetInt32() > 0));
    }

    /// <summary>Kỳ quá khứ: đang mượn / quá hạn tính đến ngày cuối kỳ, không phải hôm nay.</summary>
    [Fact]
    public async Task MB06_KyQuaKhu_TinhDenNgayCuoiKy()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var den = HomNay.AddDays(-30);
        var tu = den.AddDays(-20);
        var d = await JsonAsync(await n1.GetAsync($"/api/quan-tri/bao-cao/muon-tra?tuNgay={Iso(tu)}&denNgay={Iso(den)}"));
        (string, object)[] ky = [("@den", den.ToDateTime(TimeOnly.MinValue))];
        Assert.Equal(await DemAsync("SELECT COUNT(*) " + SqlChuaTra, ky), d.GetProperty("soSachDangMuon").GetInt32());
        Assert.Equal(await DemAsync($"SELECT COUNT(*) {SqlChuaTra} AND {SqlHanHienHanh} < @den", ky), d.GetProperty("soSachQuaHan").GetInt32());
    }

    [Theory]
    [InlineData("tuNgay=2026-01-10&denNgay=2026-01-01", "tuNgay")]   // từ > đến
    [InlineData("tuNgay=2020-01-01&denNgay=2021-06-01", "denNgay")]  // quá 366 ngày
    [InlineData("denNgay=2026-01-01", "tuNgay")]                      // thiếu từ ngày
    public async Task KyBaoCao_KhongHopLe_422(string truyVan, string truong)
    {
        using var n1 = await HttpCuaAsync("muontra");
        using var n2 = await HttpCuaAsync("vipham");
        foreach (var (http, url) in new[] { (n1, "/api/quan-tri/bao-cao/muon-tra?"), (n2, "/api/quan-tri/bao-cao/vi-pham?") })
        {
            var r = await http.GetAsync(url + truyVan);
            Assert.Equal((HttpStatusCode)422, r.StatusCode);
            Assert.Contains(truong, await r.Content.ReadAsStringAsync());
        }
        var tuongLai = await n1.GetAsync($"/api/quan-tri/bao-cao/muon-tra?tuNgay={Iso(HomNay)}&denNgay={Iso(HomNay.AddDays(1))}");
        Assert.Equal((HttpStatusCode)422, tuongLai.StatusCode);
    }

    [Fact]
    public async Task LapBaoCao_GhiNhatKy()
    {
        using var n1 = await HttpCuaAsync("muontra");
        using var n2 = await HttpCuaAsync("vipham");
        var tu = HomNay.AddDays(-6);
        var ky = new { tuNgay = Iso(tu), denNgay = Iso(HomNay) };
        var kyChu = $"{tu:dd/MM/yyyy} – {HomNay:dd/MM/yyyy}";

        var truoc = await DemAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung = @n", ("@n", $"01.04.03 Lập Báo cáo tình hình mượn trả sách (MB06) kỳ {kyChu}"));
        var kq = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/bao-cao/muon-tra/lap", ky));
        Assert.Equal(Iso(HomNay), kq.GetProperty("ngayLap").GetString()); // TT072 ngày lập tự sinh
        Assert.Equal(truoc + 1, await DemAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung = @n", ("@n", $"01.04.03 Lập Báo cáo tình hình mượn trả sách (MB06) kỳ {kyChu}")));

        var truoc2 = await DemAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung = @n", ("@n", $"02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách (MB07) kỳ {kyChu}"));
        await JsonAsync(await n2.PostAsJsonAsync("/api/quan-tri/bao-cao/vi-pham/lap", ky));
        Assert.Equal(truoc2 + 1, await DemAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung = @n", ("@n", $"02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách (MB07) kỳ {kyChu}")));
    }

    // ---------- MB07 ----------
    [Fact]
    public async Task MB07_KhopTruyVanKiemChung()
    {
        using var n2 = await HttpCuaAsync("vipham");
        var den = HomNay;
        var tu = den.AddDays(-89);
        var d = await JsonAsync(await n2.GetAsync($"/api/quan-tri/bao-cao/vi-pham?tuNgay={Iso(tu)}&denNgay={Iso(den)}"));
        (string, object)[] ky = [("@tu", tu.ToDateTime(TimeOnly.MinValue)), ("@sau", den.AddDays(1).ToDateTime(TimeOnly.MinValue))];
        const string trongKy = "bb.thoigian_lap >= @tu AND bb.thoigian_lap < @sau";

        Assert.Equal(await DemAsync($"SELECT COUNT(*) FROM vipham.BIENBAN bb WHERE {trongKy}", ky), d.GetProperty("soBienBan").GetInt32());
        Assert.True(d.GetProperty("soBienBan").GetInt32() >= 5, "Dữ liệu mẫu có ≥ 5 biên bản");
        Assert.Equal(await DemAsync($"SELECT COUNT(*) FROM vipham.CT_BIENBAN cb JOIN vipham.BIENBAN bb ON bb.so_bienban = cb.so_bienban WHERE {trongKy}", ky),
            d.GetProperty("soTruongHop").GetInt32());
        Assert.Equal(await TienAsync($"SELECT SUM(tong_tien) FROM vipham.BIENBAN bb WHERE {trongKy} AND ht_thanhtoan <> N'Chưa thanh toán'", ky),
            d.GetProperty("tongTienDaThu").GetDecimal());
        Assert.Equal(await TienAsync($"SELECT SUM(tong_tien) FROM vipham.BIENBAN bb WHERE {trongKy} AND ht_thanhtoan = N'Chưa thanh toán'", ky),
            d.GetProperty("tongTienChuaThu").GetDecimal());

        foreach (var k in d.GetProperty("theoKhoanThu").EnumerateArray())
        {
            (string, object)[] ts = [.. ky, ("@kt", k.GetProperty("maKhoanThu").GetString()!)];
            Assert.Equal(await DemAsync($"SELECT COUNT(cb.ma_bansach) FROM vipham.CT_BIENBAN cb JOIN vipham.BIENBAN bb ON bb.so_bienban = cb.so_bienban WHERE {trongKy} AND cb.ma_khoanthu = @kt", ts),
                k.GetProperty("soTruongHop").GetInt32());
            Assert.Equal(await TienAsync($"SELECT SUM(cb.thanh_tien) FROM vipham.CT_BIENBAN cb JOIN vipham.BIENBAN bb ON bb.so_bienban = cb.so_bienban WHERE {trongKy} AND cb.ma_khoanthu = @kt", ts),
                k.GetProperty("tongTien").GetDecimal());
            Assert.Equal(k.GetProperty("soTruongHop").GetInt32(), k.GetProperty("soTruongHopDaThanhToan").GetInt32() + k.GetProperty("soTruongHopChuaThanhToan").GetInt32());
        }
        Assert.Equal(d.GetProperty("tongTienViPham").GetDecimal(), d.GetProperty("theoKhoanThu").EnumerateArray().Sum(k => k.GetProperty("tongTien").GetDecimal()));
        Assert.Equal(d.GetProperty("soBienBan").GetInt32(), d.GetProperty("soDongBienBan").GetInt32());
        Assert.Equal(Math.Min(50, d.GetProperty("soBienBan").GetInt32()), d.GetProperty("bienBan").GetArrayLength());
    }

    // ---------- Quyền SPEC 3.4 ----------
    [Fact]
    public async Task Quyen_BaoCaoVaTongQuan_TheoNhom()
    {
        using var n1 = await HttpCuaAsync("muontra");
        using var n2 = await HttpCuaAsync("vipham");
        using var n3 = await HttpCuaAsync("khosach");
        using var n4 = await HttpCuaAsync("admin");
        var ky = $"?tuNgay={Iso(HomNay.AddDays(-7))}&denNgay={Iso(HomNay)}";
        // MB06: chỉ N1 (TT064–TT073); MB07: chỉ N2 (TT092–TT098)
        foreach (var h in new[] { n2, n3, n4 }) Assert.Equal(HttpStatusCode.Forbidden, (await h.GetAsync("/api/quan-tri/bao-cao/muon-tra" + ky)).StatusCode);
        foreach (var h in new[] { n1, n3, n4 }) Assert.Equal(HttpStatusCode.Forbidden, (await h.GetAsync("/api/quan-tri/bao-cao/vi-pham" + ky)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n2.PostAsJsonAsync("/api/quan-tri/bao-cao/muon-tra/lap", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n1.PostAsJsonAsync("/api/quan-tri/bao-cao/vi-pham/lap", new { })).StatusCode);
        // Tổng quan: mỗi nhóm một trang
        var api = new[] { "/api/quan-tri/thong-ke/n1", "/api/quan-tri/thong-ke/n2", "/api/quan-tri/thong-ke/n3", "/api/quan-tri/thong-ke/n4" };
        var http = new[] { n1, n2, n3, n4 };
        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                Assert.Equal(i == j ? HttpStatusCode.OK : HttpStatusCode.Forbidden, (await http[j].GetAsync(api[i])).StatusCode);
    }

    // ---------- Tổng quan N1–N4 ----------
    [Fact]
    public async Task TongQuanN1_KhopTruyVanKiemChung()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var d = await JsonAsync(await n1.GetAsync("/api/quan-tri/thong-ke/n1"));
        var homNay = HomNay;
        (string, object)[] ky = [("@den", homNay.ToDateTime(TimeOnly.MinValue))];
        Assert.Equal(Iso(homNay), d.GetProperty("homNay").GetString());
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUMUON WHERE ngaymuon = @den", ky), d.GetProperty("luotMuonHomNay").GetInt32());
        Assert.Equal(await DemAsync("SELECT COUNT(*) " + SqlChuaTra, ky), d.GetProperty("dangMuon").GetInt32());
        Assert.Equal(await DemAsync($"SELECT COUNT(*) {SqlChuaTra} AND {SqlHanHienHanh} < @den", ky), d.GetProperty("quaHan").GetInt32());
        Assert.Equal(d.GetProperty("quaHan").GetInt32(), (await JsonAsync(await n1.GetAsync("/api/quan-tri/thong-ke/n1/sach?loai=qua-han"))).GetArrayLength());

        // Top 10 gộp theo SACH.ten_sach, 12 tháng (GIA_DINH D50)
        var top = d.GetProperty("top10").EnumerateArray().ToList();
        Assert.InRange(top.Count, 1, 10);
        var dau = await ScalarAsync("""
            SELECT TOP 1 COUNT(*) FROM muontra.CT_PHIEUMUON cm JOIN khosach.SACH s ON s.ma_bansach = cm.ma_bansach
            WHERE cm.ngaymuon BETWEEN DATEADD(day, -364, @den) AND @den GROUP BY s.ten_sach ORDER BY COUNT(*) DESC
            """, ky);
        Assert.Equal(Convert.ToInt32(dau), top[0].GetProperty("soLuot").GetInt32());
        Assert.Equal(30, d.GetProperty("muonTra30Ngay").GetArrayLength());
        Assert.Equal(365, d.GetProperty("theoNgayTrongNam").GetArrayLength());
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUMUON WHERE ngaymuon BETWEEN DATEADD(day, -364, @den) AND @den", ky),
            d.GetProperty("theoNgayTrongNam").EnumerateArray().Sum(x => x.GetProperty("soLuot").GetInt32()));
    }

    [Fact]
    public async Task TongQuanN2_N3_N4_KhopTruyVanKiemChung()
    {
        using var n2 = await HttpCuaAsync("vipham");
        var d2 = await JsonAsync(await n2.GetAsync("/api/quan-tri/thong-ke/n2"));
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM vipham.BIENBAN WHERE ht_thanhtoan = N'Chưa thanh toán'"), d2.GetProperty("chuaThanhToan").GetInt32());
        Assert.Equal(await TienAsync("SELECT SUM(tong_tien) FROM vipham.BIENBAN WHERE ht_thanhtoan = N'Chưa thanh toán'"), d2.GetProperty("tienChuaThanhToan").GetDecimal());
        Assert.Equal(d2.GetProperty("chuaThanhToan").GetInt32(), (await JsonAsync(await n2.GetAsync("/api/quan-tri/thong-ke/n2/chua-thanh-toan"))).GetArrayLength());
        var dauThang = new DateOnly(HomNay.Year, HomNay.Month, 1).ToDateTime(TimeOnly.MinValue);
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM vipham.BIENBAN WHERE thoigian_lap >= @t", ("@t", dauThang)), d2.GetProperty("bienBanThangNay").GetInt32());
        Assert.Equal(12, d2.GetProperty("thang").GetArrayLength());

        using var n3 = await HttpCuaAsync("khosach");
        var d3 = await JsonAsync(await n3.GetAsync("/api/quan-tri/thong-ke/n3"));
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM khosach.SACH"), d3.GetProperty("tongBanSach").GetInt32());
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM khosach.SACH WHERE tinhtrang = N'Sẵn sàng'"), d3.GetProperty("sanSang").GetInt32());
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM khosach.SACH WHERE tinhtrang = N'Đang được mượn'"), d3.GetProperty("dangDuocMuon").GetInt32());
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM khosach.SACH WHERE tinhtrang IN (N'Hư hỏng', N'Mất')"),
            d3.GetProperty("huHong").GetInt32() + d3.GetProperty("mat").GetInt32());
        Assert.Equal(d3.GetProperty("tongBanSach").GetInt32(), d3.GetProperty("theoKhuVuc").EnumerateArray().Sum(x => x.GetProperty("soSach").GetInt32()));

        using var n4 = await HttpCuaAsync("admin");
        var d4 = await JsonAsync(await n4.GetAsync("/api/quan-tri/thong-ke/n4"));
        var dauNgay = HomNay.ToDateTime(TimeOnly.MinValue);
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM quantri.NGUOIDUNG WHERE tinhtrang = N'Hoạt động'"), d4.GetProperty("taiKhoanHoatDong").GetInt32());
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM quantri.NHATKY_DANGNHAP WHERE tg_dangnhap >= @t", ("@t", dauNgay)), d4.GetProperty("dangNhapHomNay").GetInt32());
        Assert.Equal(await DemAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE tg_thuchien >= @t", ("@t", dauNgay)), d4.GetProperty("thaoTacHomNay").GetInt32());
        Assert.Equal(30, d4.GetProperty("dangNhap30Ngay").GetArrayLength());
    }
}
