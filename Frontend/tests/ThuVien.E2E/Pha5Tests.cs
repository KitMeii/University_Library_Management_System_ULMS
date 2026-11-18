using System.Text.Json;
using Deque.AxeCore.Playwright;

namespace ThuVien.E2E;

/// <summary>
/// Pha 5 – cổng bạn đọc: header, hero (file 04 mục 2), kệ sách coverflow (file 05 mục 5–7): bàn phím, nút, bấm gáy, bấm liên tục,
/// kéo, thanh thời gian, đổi kệ (yêu cầu mới nhất thắng), mở cuốn giữa, giảm chuyển động, không JS, điện thoại, axe, CSP thật,
/// số đo khung hình khi kéo liên tục (CPU chậm 4×).
/// </summary>
[Collection("TrinhDuyet")]
public class Pha5Tests(TrinhDuyet td)
{
    private static async Task<IPage> TrangChuAsync(IBrowserContext ctx)
    {
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#ke-sach.cf-3d .book[aria-current='true']");
        await p.EvaluateAsync("() => document.getElementById('ke-sach').scrollIntoView({ block: 'start' })");
        await p.WaitForTimeoutAsync(300);
        return p;
    }

    private static Task<int> GiuaAsync(IPage p) =>
        p.EvaluateAsync<int>("() => Number(document.querySelector('#ke-sach .book[aria-current=\"true\"]').dataset.index)");
    private static Task ChoSanSangAsync(IPage p) =>
        p.WaitForFunctionAsync("() => document.getElementById('ke-sach').dataset.trangThai === 'KE_SAN_SANG'");
    private static Task<int> SoSachAsync(IPage p) => p.Locator("#ke-sach .book").CountAsync();

    [Fact]
    public async Task Header_Hero_TamDungPhat_ChamDieuHuong()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        var nav = await p.Locator(".cong-nav__muc > a").AllInnerTextsAsync();
        Assert.Equal(["Trang chủ", "Khám phá sách", "Danh mục", "Kệ sách nổi bật", "Hướng dẫn mượn sách", "Thông tin thư viện"], nav.Select(x => x.Trim()).ToArray());
        Assert.Equal("Tri thức mở lối – Tương lai rộng mở.", (await p.InnerTextAsync("h1")).Trim());
        // Ảnh đầu không lazy-load, ảnh sau chưa tải trước khi cần
        Assert.Null(await p.GetAttributeAsync(".hero__slide >> nth=0", "loading"));
        Assert.True(await p.Locator(".hero__slide").CountAsync() >= 4);
        // Nút Tạm dừng/Phát
        await p.ClickAsync("#btnHeroDung");
        Assert.Equal("true", await p.GetAttributeAsync("#btnHeroDung", "aria-pressed"));
        Assert.Equal("Phát trình chiếu", await p.GetAttributeAsync("#btnHeroDung", "aria-label"));
        // Chấm điều hướng
        await p.ClickAsync(".hero__cham-nut[data-chi-so='2']");
        await p.WaitForFunctionAsync("() => document.querySelector('.hero__cham-nut[data-chi-so=\"2\"]').getAttribute('aria-current') === 'true'");
        Assert.Equal(1, await p.Locator(".hero__cham-nut[aria-current='true']").CountAsync());
        // Đang tạm dừng: không tự chuyển sau chu kỳ 4.5s
        await p.WaitForTimeoutAsync(5200);
        Assert.Equal("true", await p.GetAttributeAsync(".hero__cham-nut[data-chi-so='2']", "aria-current"));
        // Liên kết neo tới kệ sách
        await p.ClickAsync(".hero__noi-dung a[href='/#ke-sach']");
        // Tới kệ (chừa chiều cao header) hoặc tới cuối trang nếu trang chưa đủ dài (các khu Pha 7 chưa có)
        await p.WaitForFunctionAsync("""
            () => { const r = document.getElementById('ke-sach').getBoundingClientRect(), h = document.querySelector('.cong-header').offsetHeight;
              return Math.abs(r.top - h - 8) < 40 || scrollY >= document.documentElement.scrollHeight - innerHeight - 2; }
            """);
        Assert.Equal("#ke-sach", await p.EvaluateAsync<string>("() => location.hash"));
    }

    [Fact]
    public async Task Coverflow_BanPhim_Nut_BamGay_ChuThich()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var n = await SoSachAsync(p);
        var giua = await GiuaAsync(p);
        Assert.Equal((n - 1) / 2, giua);
        Assert.Equal(1, await p.Locator("#ke-sach .book[aria-current='true']").CountAsync());
        // Chỉ cuốn giữa nhận Tab
        Assert.Equal(1, await p.Locator("#ke-sach .book__body[tabindex='0']").CountAsync());

        // Phím → khi khu vực có focus
        await p.FocusAsync("#ke-sach .book[aria-current='true'] .book__body");
        await p.Keyboard.PressAsync("ArrowRight");
        await ChoSanSangAsync(p);
        Assert.Equal(giua + 1, await GiuaAsync(p));
        var ten = await p.EvaluateAsync<string>("(i) => JSON.parse(document.getElementById('duLieuKe').textContent).ke.sach[i].tenSach", giua + 1);
        await p.WaitForFunctionAsync("(t) => (document.getElementById('cfTieuDe').getAttribute('aria-label') || document.getElementById('cfTieuDe').textContent) === t", ten);
        Assert.Contains(ten, await p.InnerTextAsync("#cfDocTen"));

        // Nút ←
        await p.ClickAsync("#cfTruoc");
        await ChoSanSangAsync(p);
        Assert.Equal(giua, await GiuaAsync(p));

        // Bấm gáy cách 3 cuốn → xoay về giữa (sự kiện con trỏ trên đúng cuốn đó – gáy bị cuốn kế bên che một phần)
        await p.EvaluateAsync("""
            (i) => { const b = document.querySelector(`#ke-sach .book[data-index="${i}"] .book__body`);
              const o = { bubbles: true, pointerId: 3, button: 0, isPrimary: true };
              b.dispatchEvent(new PointerEvent('pointerdown', o)); b.dispatchEvent(new PointerEvent('pointerup', o)); }
            """, giua + 3);
        await ChoSanSangAsync(p);
        Assert.Equal(giua + 3, await GiuaAsync(p));

        // Thanh thời gian: bấm đầu thanh → cuốn đầu kệ (năm nhỏ nhất)
        var hop = (await p.Locator("#cfTruc").BoundingBoxAsync())!;
        await p.Mouse.ClickAsync(hop.X + 2, hop.Y + hop.Height / 2);
        await ChoSanSangAsync(p);
        Assert.Equal(0, await GiuaAsync(p));
        Assert.Equal("1", await p.GetAttributeAsync("#cfTruc", "aria-valuenow"));
        Assert.True(await p.IsDisabledAsync("#cfTruoc"));
    }

    [Fact]
    public async Task Coverflow_BamLienTuc10Lan_KhongKetTrangThai()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var n = await SoSachAsync(p);
        var giua = await GiuaAsync(p);
        for (var i = 0; i < 10; i++) { await p.ClickAsync("#cfSau"); await p.WaitForTimeoutAsync(90); }
        await ChoSanSangAsync(p);
        Assert.Equal(Math.Min(giua + 10, n - 1), await GiuaAsync(p));
        // Đổi kệ trong lúc đang xoay: không lỗi
        await p.ClickAsync("#cfTruoc");
        await p.ClickAsync("#cfKeSau");
        await ChoSanSangAsync(p);
        await p.WaitForFunctionAsync("() => document.getElementById('tenKeHienTai').textContent.trim() === 'Mượn nhiều nhất'");
        Assert.Equal(1, await p.Locator("#ke-sach .book[aria-current='true']").CountAsync());
    }

    [Fact]
    public async Task Coverflow_DoiKeLienTuc_YeuCauMoiNhatThang()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var danhSach = JsonDocument.Parse(await p.InnerTextAsync("#duLieuKe")).RootElement.GetProperty("danhSach").EnumerateArray()
            .Select(k => k.GetProperty("ten").GetString()!).ToList();
        Assert.True(danhSach.Count >= 3);
        await p.ClickAsync("#cfKeSau");
        await p.ClickAsync("#cfKeSau");
        await p.ClickAsync("#cfKeSau");
        await ChoSanSangAsync(p);
        await p.WaitForTimeoutAsync(400);
        await ChoSanSangAsync(p);
        // Lần bấm cuối cùng thắng: kệ thứ 2 tính từ kệ đang hiện khi lệnh chờ chạy → không kẹt, đúng một kệ hợp lệ, có sách
        var ten = (await p.TextContentAsync("#tenKeHienTai"))!.Trim(); // textContent: không bị text-transform viết hoa
        Assert.Contains(ten, danhSach);
        Assert.NotEqual(danhSach[0], ten);
        Assert.True(await SoSachAsync(p) > 0);
        Assert.Equal("true", await p.GetAttributeAsync($".coverflow__chip:has-text('{ten}')", "aria-pressed"));
    }

    [Fact]
    public async Task Coverflow_KeoChuot_QuanTinh_BamCuonGiua_MoBookPreview()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var giua = await GiuaAsync(p);
        var san = (await p.Locator("#cfSanKhau").BoundingBoxAsync())!;
        var y = san.Y + san.Height / 2;
        await p.Mouse.MoveAsync(san.X + san.Width * 0.6f, y);
        await p.Mouse.DownAsync();
        for (var i = 1; i <= 12; i++) await p.Mouse.MoveAsync(san.X + san.Width * 0.6f - i * 30, y);
        await p.Mouse.UpAsync();
        await ChoSanSangAsync(p);
        var sau = await GiuaAsync(p);
        Assert.True(sau > giua, $"Kéo sang trái phải tới sách sau ({giua} → {sau})");
        Assert.True(sau - giua <= 5 + 4, "Quán tính có giới hạn");

        // Bấm cuốn giữa → Book Preview (Pha 6); Esc đóng, focus về cuốn giữa
        await p.Locator("#ke-sach .book[aria-current='true'] .book__body").ClickAsync();
        await p.WaitForSelectorAsync(".bp.bp--da-mo");
        // Book Preview đã mở xong (bp--da-mo) – Esc đóng ngay
        Assert.Equal("MO_SACH", await p.EvaluateAsync<string>("() => document.getElementById('ke-sach').dataset.trangThai"));
        Assert.True(await p.IsVisibleAsync(".bp-tinh-trang__so"), "Book Preview hiện tình trạng còn bản có thể mượn");
        await p.Keyboard.PressAsync("Escape");
        await p.WaitForSelectorAsync(".bp", new() { State = WaitForSelectorState.Detached });
        await ChoSanSangAsync(p);
        Assert.True(await p.EvaluateAsync<bool>("() => document.activeElement.closest('.book')?.getAttribute('aria-current') === 'true'"));
    }

    [Fact]
    public async Task Coverflow_GiamChuyenDong_VaKhongJavaScript()
    {
        await using var ctx = await td.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1366, Height = 768 }, Locale = "vi-VN", ReducedMotion = ReducedMotion.Reduce, BypassCSP = true });
        var p = await TrangChuAsync(ctx);
        var giua = await GiuaAsync(p);
        await p.ClickAsync("#cfSau");
        await ChoSanSangAsync(p);
        Assert.Equal(giua + 1, await GiuaAsync(p));
        Assert.Equal("1", await p.EvaluateAsync<string>("() => getComputedStyle(document.getElementById('cfKe')).opacity"));
        // Không có con trỏ “Mở”, Hero không tự chạy
        Assert.Equal("0", await p.EvaluateAsync<string>("() => getComputedStyle(document.getElementById('cfConTro')).opacity"));

        await using var ctxKhongJs = await td.Browser.NewContextAsync(new() { JavaScriptEnabled = false, ViewportSize = new() { Width = 1366, Height = 768 } });
        var k = await ctxKhongJs.NewPageAsync();
        await k.GotoAsync(TrinhDuyet.Web + "/");
        Assert.True(await k.Locator(".coverflow__luoi li").CountAsync() > 0, "Không có JS: lưới bìa đơn giản");
        Assert.Equal("Tri thức mở lối – Tương lai rộng mở.", (await k.InnerTextAsync("h1")).Trim());
    }

    [Theory]
    [InlineData("iPhone 13")]
    [InlineData("Pixel 7")]
    public async Task DienThoai_KhongTranNgang_ChipKe_VuotNgang(string thietBi)
    {
        await using var ctx = await td.DienThoaiAsync(thietBi);
        var p = await TrangChuAsync(ctx);
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"), "Trang tràn ngang");
        Assert.False(await p.Locator("#cfKeSau").IsVisibleAsync());
        Assert.True(await p.Locator(".coverflow__chip").First.IsVisibleAsync());
        // 3–4 gáy mỗi bên hiển thị trong khung nhìn
        var soHien = await p.EvaluateAsync<int>("""
            () => [...document.querySelectorAll('#ke-sach .book')].filter((b) => {
              if (getComputedStyle(b).visibility !== 'visible') return false;
              const r = b.getBoundingClientRect(); return r.right > 0 && r.left < innerWidth; }).length
            """);
        Assert.True(soHien >= 5, $"Chỉ thấy {soHien} cuốn trên điện thoại");
        // Vuốt ngang (Pointer Events chạm)
        var giua = await GiuaAsync(p);
        await p.EvaluateAsync("""
            async () => {
              const s = document.getElementById('cfSanKhau'); const r = s.getBoundingClientRect(); const y = r.top + r.height / 2;
              const ev = (t, x) => s.dispatchEvent(new PointerEvent(t, { pointerId: 7, pointerType: 'touch', clientX: x, clientY: y, bubbles: true, button: 0, isPrimary: true }));
              ev('pointerdown', r.width * 0.8);
              for (let i = 1; i <= 10; i++) { await new Promise((q) => setTimeout(q, 16)); ev('pointermove', r.width * 0.8 - i * 22); }
              ev('pointerup', r.width * 0.8 - 220);
            }
            """);
        await ChoSanSangAsync(p);
        Assert.True(await GiuaAsync(p) > giua);
        // Chip đổi kệ
        await p.Locator(".coverflow__chip").Nth(1).ClickAsync();
        await ChoSanSangAsync(p);
        Assert.Equal("true", await p.GetAttributeAsync(".coverflow__chip >> nth=1", "aria-pressed"));
    }

    [Fact]
    public async Task Axe_TrangChu_VaCspThat()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        await p.WaitForTimeoutAsync(1200);
        var kq = await p.RunAxe();
        var loi = kq.Violations.Where(v => v.Impact is "critical" or "serious").ToList();
        Assert.True(loi.Count == 0, string.Join("\n", loi.Select(v => $"{v.Id}: {v.Help} – " + string.Join("; ", v.Nodes.Select(n => n.Target)))));

        await using var csp = await td.MayTinhCspThatAsync();
        await csp.AddInitScriptAsync("(() => document.addEventListener('securitypolicyviolation', (e) => (window.__csp ||= []).push(e.violatedDirective + ' ' + e.blockedURI + ' ' + (e.sample || ''))))()");
        var c = await csp.NewPageAsync();
        var viPham = new List<string>();
        c.Console += (_, m) => { if (m.Text.Contains("Content Security Policy") || m.Text.Contains("Refused to")) viPham.Add(m.Text); };
        await c.GotoAsync(TrinhDuyet.Web + "/");
        await c.WaitForSelectorAsync("#ke-sach.cf-3d .book[aria-current='true']");
        await c.ClickAsync("#cfSau");
        await c.ClickAsync("#cfKeSau");
        await c.WaitForTimeoutAsync(1200);
        await c.Locator("#ke-sach .book[aria-current='true'] .book__body").ClickAsync();
        await c.WaitForSelectorAsync(".bp.bp--da-mo");
        viPham.AddRange(await c.EvaluateAsync<string[]>("() => window.__csp || []"));
        Assert.True(viPham.Count == 0, "Vi phạm CSP: " + string.Join(" | ", viPham.Distinct()));
    }

    /// <summary>
    /// File 05 mục 7: kéo liên tục, đo khung hình bằng requestAnimationFrame ở CPU bình thường và chậm 4× (CDP).
    /// Ghi số đo ra artifacts/test-results/so_do_coverflow.json cho báo cáo. Container kiểm thử không có GPU (vẽ 3D bằng CPU) nên
    /// ngưỡng ở đây chỉ chặn suy giảm; tiêu chí ≥ 55 fps (CPU chậm 4×) đo trên máy thật có GPU (file 05 mục 5b, 7).
    /// </summary>
    [Fact]
    public async Task SoDo_KhungHinh_KeoLienTuc_CpuCham4Lan()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var cdp = await ctx.NewCDPSessionAsync(p);
        var san = (await p.Locator("#cfSanKhau").BoundingBoxAsync())!;
        var y = san.Y + san.Height / 2;

        async Task<(double Fps, double Cham, int SoKhung)> DoAsync(int cham)
        {
            await cdp.SendAsync("Emulation.setCPUThrottlingRate", new Dictionary<string, object> { ["rate"] = cham });
            await p.EvaluateAsync("() => { window.__khung = []; let t0 = performance.now(); const vong = (t) => { window.__khung.push(t - t0); t0 = t; if (window.__do) requestAnimationFrame(vong); }; window.__do = true; requestAnimationFrame(vong); }");
            for (var lan = 0; lan < 3; lan++)
            {
                await p.Mouse.MoveAsync(san.X + san.Width * 0.75f, y);
                await p.Mouse.DownAsync();
                for (var i = 1; i <= 40; i++) await p.Mouse.MoveAsync(san.X + san.Width * 0.75f - i * 14, y);
                for (var i = 1; i <= 40; i++) await p.Mouse.MoveAsync(san.X + san.Width * 0.75f - 560 + i * 14, y);
                await p.Mouse.UpAsync();
            }
            await p.EvaluateAsync("() => { window.__do = false; }");
            var khung = (await p.EvaluateAsync<double[]>("() => window.__khung.slice(2)")).Where(x => x > 0).ToArray();
            await cdp.SendAsync("Emulation.setCPUThrottlingRate", new Dictionary<string, object> { ["rate"] = 1 });
            await p.WaitForTimeoutAsync(1200);
            return (1000 / khung.Average(), khung.Count(x => x > 33.4) * 100.0 / khung.Length, khung.Length);
        }

        var thuong = await DoAsync(1);
        var cham4 = await DoAsync(4);
        var thuMuc = Path.Combine(Environment.GetEnvironmentVariable("ULMS_ROOT") ?? "/src", "artifacts", "test-results");
        Directory.CreateDirectory(thuMuc);
        await File.WriteAllTextAsync(Path.Combine(thuMuc, "so_do_coverflow.json"), JsonSerializer.Serialize(new
        {
            moiTruong = "Chromium headless trong container, không GPU (vẽ 3D bằng CPU)",
            cpuBinhThuong = new { fpsTrungBinh = Math.Round(thuong.Fps, 1), phanTramKhungTren33ms = Math.Round(thuong.Cham, 1), soKhung = thuong.SoKhung },
            cpuCham4 = new { fpsTrungBinh = Math.Round(cham4.Fps, 1), phanTramKhungTren33ms = Math.Round(cham4.Cham, 1), soKhung = cham4.SoKhung },
            ngay = DateTime.UtcNow
        }));
        Assert.True(thuong.Fps >= 40, $"FPS (CPU bình thường) {thuong.Fps:F1} – suy giảm hiệu năng kệ sách");
        Assert.True(cham4.Fps >= 20, $"FPS (CPU chậm 4×) {cham4.Fps:F1} – suy giảm hiệu năng kệ sách");
    }
}