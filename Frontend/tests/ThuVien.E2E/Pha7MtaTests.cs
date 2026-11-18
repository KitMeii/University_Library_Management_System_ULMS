namespace ThuVien.E2E;

/// <summary>
/// Pha 7 – các điểm điều hướng/chuyển động học từ trang tham khảo (THAM_KHAO_MTA.md, đã duyệt): menu thả xuống bằng bàn phím,
/// nhóm thu gọn trên điện thoại, mũi tên Hero (chuột + bàn phím), nút lên đầu trang, header thu gọn trên máy tính,
/// CSP không 'unsafe-hashes'/'unsafe-inline', chế độ giảm chuyển động.
/// </summary>
[Collection("TrinhDuyet")]
public class Pha7MtaTests(TrinhDuyet td)
{
    private static async Task<IPage> MoTrangChuAsync(IBrowserContext ctx)
    {
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        return p;
    }

    private static Task<bool> DangMoAsync(IPage p, string id) =>
        p.EvaluateAsync<bool>($"() => getComputedStyle(document.getElementById('{id}')).visibility === 'visible'");

    [Fact]
    public async Task MenuThaXuong_BanPhim_MuiTenXuong_Esc_NapTheLoai()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoTrangChuAsync(ctx);
        var nut = p.Locator("[aria-controls='dd-danh-muc']");
        Assert.False(await DangMoAsync(p, "dd-danh-muc"));
        await nut.FocusAsync();
        await p.Keyboard.PressAsync("ArrowDown");
        // Thể loại nạp từ API công khai vào bảng con; focus vào liên kết đầu
        await p.WaitForFunctionAsync("() => document.querySelectorAll('#dd-danh-muc a[data-loc-the-loai]').length > 0");
        Assert.True(await DangMoAsync(p, "dd-danh-muc"));
        Assert.Equal("true", await nut.GetAttributeAsync("aria-expanded"));
        Assert.True(await p.EvaluateAsync<bool>("() => document.activeElement?.closest('#dd-danh-muc') !== null"), "Focus nằm trong bảng con");
        var soTheLoai = await p.EvaluateAsync<int>("async () => (await (await fetch('/api/cong/the-loai')).json()).length");
        Assert.Equal(soTheLoai, await p.Locator("#dd-danh-muc a[data-loc-the-loai]").CountAsync());
        // Chỉ chèn vào danh sách của menu – ô thể loại khu Danh mục và khu Khám phá giữ nguyên
        Assert.Equal(0, await p.Locator("#danh-muc a[data-loc-the-loai], #kham-pha a[data-loc-the-loai]").CountAsync());
        await p.Keyboard.PressAsync("ArrowDown");
        Assert.True(await p.EvaluateAsync<bool>("() => document.activeElement?.closest('#dd-danh-muc') !== null"));
        // Esc đóng và trả focus về nút mở
        await p.Keyboard.PressAsync("Escape");
        await p.WaitForTimeoutAsync(400);
        Assert.False(await DangMoAsync(p, "dd-danh-muc"));
        Assert.Equal("false", await nut.GetAttributeAsync("aria-expanded"));
        Assert.True(await p.EvaluateAsync<bool>("() => document.activeElement?.getAttribute('aria-controls') === 'dd-danh-muc'"));

        // Bấm một thể loại trong menu → khu Khám phá lọc theo thể loại đó, không tải lại trang
        await p.EvaluateAsync("() => { window.__khongTaiLai = true; }");
        await p.HoverAsync(".cong-nav__muc--con:has(#dd-danh-muc)");
        var lk = p.Locator("#dd-danh-muc a[data-loc-the-loai]").First;
        var ma = await lk.GetAttributeAsync("data-loc-the-loai");
        await lk.ClickAsync();
        await p.WaitForFunctionAsync($"() => document.getElementById('cboTheLoai').value === '{ma}'");
        Assert.True(await p.EvaluateAsync<bool>("() => window.__khongTaiLai === true"), "Không tải lại trang");

        // Menu Hướng dẫn: mở bằng hover, đủ mục
        await p.HoverAsync(".cong-nav__muc--con:has(#dd-huong-dan)");
        await p.WaitForTimeoutAsync(350);
        Assert.True(await DangMoAsync(p, "dd-huong-dan"));
        Assert.True(await p.Locator("#dd-huong-dan a").CountAsync() >= 3);
    }

    [Fact]
    public async Task DienThoai_MenuNhomThuGon_KhongCuonNgang()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await MoTrangChuAsync(ctx);
        await p.ClickAsync(".cong-header__menu");
        await p.WaitForSelectorAsync("#congMenu.show");
        var nhom = p.Locator("#congMenu details.cong-offcanvas__nhom");
        Assert.Equal(2, await nhom.CountAsync());
        Assert.False(await nhom.First.EvaluateAsync<bool>("(d) => d.open"));
        await nhom.First.Locator("summary").ClickAsync();
        Assert.True(await nhom.First.EvaluateAsync<bool>("(d) => d.open"));
        await p.WaitForFunctionAsync("() => document.querySelectorAll('#congMenu details a[data-loc-the-loai]').length > 0");
        Assert.True(await nhom.First.Locator("a").First.IsVisibleAsync());
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));
        // Điện thoại: header không thu gọn (hàng trên giữ nút menu)
        await p.EvaluateAsync("() => window.scrollTo(0, 1500)");
        await p.WaitForTimeoutAsync(400);
        Assert.False(await p.EvaluateAsync<bool>("() => document.querySelector('.cong-header').classList.contains('thu-gon')"));
    }

    [Fact]
    public async Task Hero_MuiTen_Chuot_BanPhim()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoTrangChuAsync(ctx);
        const string chiSo = "() => [...document.querySelectorAll('.hero__slide')].findIndex((e) => e.classList.contains('dang-hien'))";
        var tong = await p.Locator(".hero__slide").CountAsync();
        var dau = await p.EvaluateAsync<int>(chiSo);
        await p.ClickAsync("#btnHeroSau");
        await p.WaitForFunctionAsync($"() => ({chiSo})() === {(dau + 1) % tong}");
        await p.ClickAsync("#btnHeroTruoc");
        await p.WaitForFunctionAsync($"() => ({chiSo})() === {dau}");
        // Bàn phím: ← → khi focus trong khung Hero; chấm đồng bộ aria-current
        await p.FocusAsync("#btnHeroSau");
        await p.Keyboard.PressAsync("ArrowRight");
        await p.WaitForFunctionAsync($"() => ({chiSo})() === {(dau + 1) % tong}");
        await p.Keyboard.PressAsync("ArrowLeft");
        await p.WaitForFunctionAsync($"() => ({chiSo})() === {dau}");
        Assert.Equal("true", await p.Locator(".hero__cham-nut").Nth(dau).GetAttributeAsync("aria-current"));
        Assert.Equal("Ảnh giới thiệu trước", await p.GetAttributeAsync("#btnHeroTruoc", "aria-label"));
        // Gợi ý “Cuộn xuống” ẩn khi đã cuộn
        Assert.True(await p.IsVisibleAsync("#lnkCuonXuong"));
        await p.EvaluateAsync("() => window.scrollTo(0, 300)");
        await p.WaitForFunctionAsync("() => document.getElementById('lnkCuonXuong').classList.contains('an')");
    }

    [Fact]
    public async Task NutLenDau_HeaderThuGon_MayTinh()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoTrangChuAsync(ctx);
        Assert.False(await p.IsVisibleAsync("#btnLenDau"));
        await p.Mouse.WheelAsync(0, 1600);
        await p.WaitForFunctionAsync("() => document.querySelector('.cong-header').classList.contains('thu-gon')");
        await p.WaitForTimeoutAsync(400);
        // Hàng tiện ích trượt lên khuất; thanh điều hướng còn trong khung nhìn
        var navTop = await p.EvaluateAsync<double>("() => document.querySelector('.cong-nav').getBoundingClientRect().top");
        Assert.InRange(navTop, -2, 4);
        await p.WaitForSelectorAsync("#btnLenDau", new() { State = WaitForSelectorState.Visible });
        // Cuộn lên → header mở lại
        await p.Mouse.WheelAsync(0, -200);
        await p.WaitForFunctionAsync("() => !document.querySelector('.cong-header').classList.contains('thu-gon')");
        await p.ClickAsync("#btnLenDau");
        await p.WaitForFunctionAsync("() => window.scrollY < 10", null, new() { Timeout = 5000 });
        await p.WaitForSelectorAsync("#btnLenDau", new() { State = WaitForSelectorState.Hidden });
    }

    [Fact]
    public async Task Csp_KhongUnsafeHashes_KhongUnsafeInline()
    {
        await using var ctx = await td.MayTinhCspThatAsync();
        var p = await ctx.NewPageAsync();
        var res = await p.GotoAsync(TrinhDuyet.Web + "/");
        var csp = await res!.HeaderValueAsync("content-security-policy") ?? "";
        Assert.Contains("script-src 'self'", csp);
        Assert.DoesNotContain("unsafe-hashes", csp);
        Assert.DoesNotContain("unsafe-inline", csp);
    }

    [Fact]
    public async Task GiamChuyenDong_HienNgay_KhongNhapNho()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        // Mọi khối [data-xuat-hien] hiện ngay (không chờ cuộn)
        Assert.True(await p.EvaluateAsync<bool>("() => [...document.querySelectorAll('[data-xuat-hien]')].every((e) => e.classList.contains('da-hien'))"));
        Assert.Equal("none", await p.EvaluateAsync<string>("() => getComputedStyle(document.querySelector('.hero__chuot span')).animationName"));
        // Menu thả xuống mở không dịch chuyển
        await p.FocusAsync("[aria-controls='dd-huong-dan']");
        await p.Keyboard.PressAsync("ArrowDown");
        Assert.Equal("none", await p.EvaluateAsync<string>("() => getComputedStyle(document.getElementById('dd-huong-dan')).transform"));
    }
    [Fact]
    public async Task ThongTinThuVien_Khu8_CotLienHe_NoiQuyTuQuyDinh()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoTrangChuAsync(ctx);
        // Bộ kiểm thử dùng .env có THUVIEN_* (D75): khu 8 + cột Liên hệ footer hiện
        Assert.Equal(1, await p.Locator("#thong-tin-thu-vien").CountAsync());
        Assert.Equal(1, await p.Locator("#cotLienHe").CountAsync());
        Assert.True(await p.Locator("#cotLienHe .cong-footer__lien-he li").CountAsync() >= 1);
        // Giờ mở cửa gộp ngày, đúng một dòng “hôm nay”
        Assert.Equal(1, await p.Locator("#lstGioMoCua li.hom-nay").CountAsync());
        Assert.Equal(1, await p.Locator("#cotLienHe .cong-footer__gio li.hom-nay").CountAsync());
        // Nội quy lấy số liệu thật từ /api/cong/quy-dinh
        var qd = await p.EvaluateAsync<int[]>("async () => { const q = await (await fetch('/api/cong/quy-dinh')).json(); return [q.soGioGiu, q.toiDaDangGiu, q.soNgayGiaHan]; }");
        var noiQuy = await p.TextContentAsync("#lstNoiQuy") ?? "";
        Assert.Contains($"{qd[0]} giờ", noiQuy);
        Assert.Contains($"{qd[1]} đặt chỗ", noiQuy);
        Assert.Contains($"Gia hạn thêm {qd[2]} ngày", noiQuy);
    }
}
