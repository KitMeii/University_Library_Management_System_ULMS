namespace ThuVien.E2E;

/// <summary>Một trình duyệt Chromium dùng chung cho cả lớp kiểm thử.</summary>
public sealed class TrinhDuyet : IAsyncLifetime
{
    public static string Web => (Environment.GetEnvironmentVariable("ULMS_WEB") ?? "http://web:8080").TrimEnd('/');
    public static string ThuMucAnh => Environment.GetEnvironmentVariable("ULMS_ANH") ?? Path.Combine(Path.GetTempPath(), "anh_giao_dien");
    public const string MatKhauMau = "Thuvien@123";

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new() { Headless = true });
        Directory.CreateDirectory(ThuMucAnh);
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    /// <summary>Máy tính 1366×768 (file 11 mục 4).</summary>
    public Task<IBrowserContext> MayTinhAsync() => Browser.NewContextAsync(new()
    {
        ViewportSize = new() { Width = 1366, Height = 768 }, Locale = "vi-VN", TimezoneId = "Asia/Ho_Chi_Minh"
    });

    /// <summary>Điện thoại: thiết bị mô phỏng của Playwright (iPhone 13 = 390×844, Pixel 7).</summary>
    public Task<IBrowserContext> DienThoaiAsync(string thietBi = "iPhone 13")
    {
        var d = Playwright.Devices[thietBi];
        return Browser.NewContextAsync(new()
        {
            ViewportSize = d.ViewportSize, UserAgent = d.UserAgent, DeviceScaleFactor = d.DeviceScaleFactor,
            IsMobile = d.IsMobile, HasTouch = d.HasTouch, Locale = "vi-VN", TimezoneId = "Asia/Ho_Chi_Minh"
        });
    }

    public static async Task DangNhapNhanVienAsync(IPage page, string ten, string mk = MatKhauMau)
    {
        await page.GotoAsync(Web + "/quan-tri/dang-nhap");
        await page.FillAsync("#txtTenDangNhap", ten);
        await page.FillAsync("#txtMatKhau", mk);
        await page.ClickAsync("#btnDangNhap");
        await page.WaitForURLAsync(u => !u.Contains("/dang-nhap"));
    }

    /// <summary>Chụp ảnh sau khi hiệu ứng vào trang kết thúc; tên theo số mục báo cáo (file 11).</summary>
    public static async Task ChupAsync(IPage page, string ten, bool toanTrang = false)
    {
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.EvaluateAsync("() => document.fonts.ready");
        await page.WaitForTimeoutAsync(700);
        await page.ScreenshotAsync(new() { Path = Path.Combine(ThuMucAnh, ten + ".png"), FullPage = toanTrang });
    }
}

[CollectionDefinition("TrinhDuyet")]
public sealed class BoTrinhDuyet : ICollectionFixture<TrinhDuyet>;
