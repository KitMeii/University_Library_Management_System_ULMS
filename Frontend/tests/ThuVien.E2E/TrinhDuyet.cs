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
        ViewportSize = new() { Width = 1366, Height = 768 }, Locale = "vi-VN", TimezoneId = "Asia/Ho_Chi_Minh", BypassCSP = true
    });

    /// <summary>Máy tính, KHÔNG bỏ qua CSP – kiểm tra trang chạy đúng dưới chính sách bảo mật thật (file 11).</summary>
    public Task<IBrowserContext> MayTinhCspThatAsync() => Browser.NewContextAsync(new()
    {
        ViewportSize = new() { Width = 1366, Height = 768 }, Locale = "vi-VN", TimezoneId = "Asia/Ho_Chi_Minh", BypassCSP = false
    });

    /// <summary>Điện thoại: thiết bị mô phỏng của Playwright (iPhone 13 = 390×844, Pixel 7).</summary>
    public Task<IBrowserContext> DienThoaiAsync(string thietBi = "iPhone 13")
    {
        var d = Playwright.Devices[thietBi];
        return Browser.NewContextAsync(new()
        {
            ViewportSize = d.ViewportSize, UserAgent = d.UserAgent, DeviceScaleFactor = d.DeviceScaleFactor,
            IsMobile = d.IsMobile, HasTouch = d.HasTouch, Locale = "vi-VN", TimezoneId = "Asia/Ho_Chi_Minh", BypassCSP = true
        });
    }

    public const string MatKhauBanDoc = "Docgia@123";

    /// <summary>
    /// Bộ kiểm thử chạy ở chế độ chụp ảnh Bài 4 (ẩn nhãn DỮ LIỆU MẪU → ẩn phần mở rộng – GIA_DINH D61). Kiểm thử chức năng mở rộng
    /// (đặt chỗ, Quầy đặt chỗ, btnNhanDatCho…) đặt cookie này để web hiện phần mở rộng (chỉ có tác dụng ở Development – D65).
    /// </summary>
    public static async Task BatMoRongAsync(IBrowserContext ctx)
    {
        // Đặt bằng document.cookie trên chính trang web (host “web” một nhãn – AddCookiesAsync theo Url không áp dụng được)
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(Web + "/kich-hoat");
        await p.EvaluateAsync("() => { document.cookie = 'ulms-kiem-thu-mo-rong=1; path=/; SameSite=Lax'; }");
        await p.CloseAsync();
    }

    public static async Task DangNhapBanDocAsync(IPage page, string maSv, string mk = MatKhauBanDoc)
    {
        await page.GotoAsync(Web + "/dang-nhap");
        await page.FillAsync("#maSvHvCb", maSv);
        await page.FillAsync("#matKhau", mk);
        await page.ClickAsync("#frmDangNhapBanDoc button[type=submit]");
        await page.WaitForURLAsync(u => !u.Contains("/dang-nhap"));
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
        // Ảnh toàn trang: hàng nút “dính đáy” được đặt về vị trí tĩnh để không đè lên lưới trong ảnh ghép
        if (toanTrang)
        {
            // content-visibility:auto (khu dưới màn hình chưa vẽ – Pha 7) không áp dụng cho ảnh ghép toàn trang: buộc vẽ hết
            await page.AddStyleTagAsync(new() { Content = ".qt-hang-nut{position:static!important} *{content-visibility:visible!important}" });
            // Thanh trên/sidebar cố định theo khung nhìn: cuộn về đầu trang để ảnh ghép không lệch
            await page.EvaluateAsync("() => { document.activeElement?.blur(); window.scrollTo(0, 0); }");
        }
        await page.WaitForTimeoutAsync(700);
        await page.ScreenshotAsync(new() { Path = Path.Combine(ThuMucAnh, ten + ".png"), FullPage = toanTrang });
    }
}

[CollectionDefinition("TrinhDuyet")]
public sealed class BoTrinhDuyet : ICollectionFixture<TrinhDuyet>;
