namespace ThuVien.E2E;

/// <summary>
/// ScreenshotTests (file 11 mục 4): 1366×768 và 390×844, tên theo số mục báo cáo (SPEC 6.3.1 ↔ 4.6.1).
/// Bộ ảnh Bài 4 chụp khi web chạy với ThuVien__HienNhanDuLieuMau=false (GIA_DINH D23).
/// </summary>
[Collection("TrinhDuyet")]
public class ChupAnhTests(TrinhDuyet td)
{
    [Fact]
    public async Task Chup_DangNhap_461()
    {
        await using (var ctx = await td.MayTinhAsync())
        {
            var page = await ctx.NewPageAsync();
            await page.GotoAsync(TrinhDuyet.Web + "/quan-tri/dang-nhap");
            await TrinhDuyet.ChupAsync(page, "4.6.1_Dang_nhap");
            await page.FillAsync("#txtTenDangNhap", "muontra");
            await page.FillAsync("#txtMatKhau", "SaiMatKhau1");
            await page.ClickAsync("#btnDangNhap");
            await page.WaitForSelectorAsync("#thongBaoLoi");
            await TrinhDuyet.ChupAsync(page, "4.6.1_Dang_nhap_Sai_mat_khau");
        }
        await using (var ctx = await td.DienThoaiAsync())
        {
            var page = await ctx.NewPageAsync();
            await page.GotoAsync(TrinhDuyet.Web + "/quan-tri/dang-nhap");
            await TrinhDuyet.ChupAsync(page, "4.6.1_Dang_nhap_mobile", toanTrang: true);
        }
    }

    [Theory]
    [InlineData("muontra", "N1")]
    [InlineData("vipham", "N2")]
    [InlineData("khosach", "N3")]
    [InlineData("admin", "N4")]
    public async Task Chup_DashboardTheoNhom(string ten, string nhom)
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(page, ten);
        await TrinhDuyet.ChupAsync(page, $"Pha1_Man_hinh_mac_dinh_{nhom}");
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri");
        await TrinhDuyet.ChupAsync(page, $"Pha1_Tong_quan_{nhom}");
    }

    [Fact]
    public async Task Chup_Dashboard_DienThoai()
    {
        await using var ctx = await td.DienThoaiAsync();
        var page = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(page, "admin");
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri");
        await TrinhDuyet.ChupAsync(page, "Pha1_Tong_quan_N4_mobile");
        await page.ClickAsync("[aria-controls='qtSidebar']");
        await page.WaitForSelectorAsync("#qtSidebar.show");
        await TrinhDuyet.ChupAsync(page, "Pha1_Menu_N4_mobile");
    }

    [Fact]
    public async Task Chup_Cong()
    {
        foreach (var (dt, hauTo) in new[] { (false, ""), (true, "_mobile") })
        {
            await using var ctx = dt ? await td.DienThoaiAsync() : await td.MayTinhAsync();
            var page = await ctx.NewPageAsync();
            await page.GotoAsync(TrinhDuyet.Web + "/");
            await TrinhDuyet.ChupAsync(page, "Cong_Trang_chu_khung" + hauTo, toanTrang: true);
            await page.GotoAsync(TrinhDuyet.Web + "/dang-nhap");
            await TrinhDuyet.ChupAsync(page, "Cong_Dang_nhap" + hauTo, toanTrang: true);
        }
    }
}
