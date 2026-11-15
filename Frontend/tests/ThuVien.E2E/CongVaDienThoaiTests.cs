using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;

namespace ThuVien.E2E;

/// <summary>Cổng bạn đọc (đăng nhập, tách cookie với nhân viên), điện thoại, accessibility (axe-core).</summary>
[Collection("TrinhDuyet")]
public class CongVaDienThoaiTests(TrinhDuyet td)
{
    [Fact]
    public async Task BanDoc_DangNhap_KhongVaoDuocQuanTri()
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await page.GotoAsync(TrinhDuyet.Web + "/dang-nhap");
        await page.FillAsync("#maSvHvCb", "SV22000001");
        await page.FillAsync("#matKhau", "Docgia@123");
        await page.ClickAsync("button[type=submit]");
        await page.WaitForURLAsync(TrinhDuyet.Web + "/");
        Assert.Contains("Nguyễn Minh Anh", await page.InnerTextAsync("main"));

        // Cookie BanDoc không mở được /quan-tri (file 07 mục 5)
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri");
        Assert.Contains("/quan-tri/dang-nhap", page.Url);
    }

    [Theory]
    [InlineData("iPhone 13")]
    [InlineData("Pixel 7")]
    public async Task DienThoai_DashboardSidebarThanhOffcanvas(string thietBi)
    {
        await using var ctx = await td.DienThoaiAsync(thietBi);
        var page = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(page, "admin");
        Assert.False(await page.IsVisibleAsync("#qtSidebar"));
        await page.ClickAsync("[aria-controls='qtSidebar']");
        await page.WaitForSelectorAsync("#qtSidebar.show");
        Assert.True(await page.IsVisibleAsync("#qtSidebar .qt-nav"));
        // Không cuộn ngang trang
        Assert.True(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));
    }

    [Theory]
    [InlineData("/quan-tri/dang-nhap")]
    [InlineData("/")]
    [InlineData("/dang-nhap")]
    public async Task Axe_KhongCoLoiNghiemTrong(string duongDan)
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await page.GotoAsync(TrinhDuyet.Web + duongDan);
        await page.WaitForTimeoutAsync(600);
        KhongCoLoi(await page.RunAxe());
    }

    [Fact]
    public async Task Axe_Dashboard_KhongCoLoiNghiemTrong()
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(page, "admin");
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri");
        await page.WaitForTimeoutAsync(800);
        KhongCoLoi(await page.RunAxe());
    }

    /// <summary>File 11: không lỗi mức nghiêm trọng (critical, serious).</summary>
    private static void KhongCoLoi(AxeResult kq)
    {
        var loi = kq.Violations.Where(v => v.Impact is "critical" or "serious").ToList();
        var moTa = string.Join(Environment.NewLine, loi.Select(v =>
            $"{v.Impact}: {v.Id} – {v.Help}: " + string.Join(" ; ",
                v.Nodes.Select(n => n.Target + " → " + string.Join(", ", n.Any.Select(c => c.Message))))));
        Assert.True(loi.Count == 0, moTa);
    }
}
