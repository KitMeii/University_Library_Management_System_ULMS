using Deque.AxeCore.Playwright;

namespace ThuVien.E2E;

/// <summary>
/// Pha 7 – trang chủ hoàn chỉnh (file 04 mục 5): đủ khu vực theo thứ tự, tìm kiếm Hero/khu 5 không đổi route và không tải lại,
/// lọc thể loại, yêu cầu mới nhất thắng, Xem thêm, thẻ sách mở Book Preview, từ khóa trong Book Preview → /kham-pha, /huong-dan,
/// điện thoại, axe, CSP thật, không CLS.
/// </summary>
[Collection("TrinhDuyet")]
public class Pha7Tests(TrinhDuyet td)
{
    private static async Task<IPage> TrangChuAsync(IBrowserContext ctx)
    {
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        // Đánh dấu cửa sổ: nếu trang tải lại thì dấu này mất
        await p.EvaluateAsync("() => { window.__khongTaiLai = true; }");
        return p;
    }

    private static Task ChoXongAsync(IPage p) =>
        p.WaitForFunctionAsync("() => document.getElementById('kpLuoi').getAttribute('aria-busy') === 'false'");

    [Fact]
    public async Task TrangChu_DuKhuVuc_DungThuTu_SoLieuThat()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var thuTu = await p.EvaluateAsync<string[]>("""
            () => [...document.querySelectorAll('header.cong-header, section.hero, #ke-sach, #danh-muc, #kham-pha, #huong-dan, #so-lieu, footer.cong-footer')]
                  .map((e) => e.id || e.className.split(' ')[0])
            """);
        Assert.Equal(["cong-header", "hero", "ke-sach", "danh-muc", "kham-pha", "huong-dan", "so-lieu", "thong-tin"], thuTu);
        // Nav header đủ mục file 04
        var nav = await p.Locator(".cong-nav__muc > a").AllInnerTextsAsync();
        Assert.Equal(["Trang chủ", "Khám phá sách", "Danh mục", "Kệ sách nổi bật", "Hướng dẫn mượn sách", "Thông tin thư viện"], nav.Select(x => x.Trim()));
        // Thư viện bằng số: số đích lấy từ API thật
        var api = await p.EvaluateAsync<int[]>("async () => { const s = await (await fetch('/api/cong/sach/so-lieu')).json(); return [s.soDauSach, s.soBanSach, s.soBanDoc, s.luotMuonThangNay]; }");
        var dich = await p.EvaluateAsync<int[]>("() => [...document.querySelectorAll('[data-dem-len]')].map((e) => Number(e.dataset.demLen))");
        Assert.Equal(api[..3], dich[..3]);
        // Hướng dẫn lấy quy định thật: 24 giờ, tối đa 3 đặt chỗ
        // textContent: khu chưa tới khung nhìn không được vẽ (content-visibility) nên innerText rỗng
        var hd = await p.TextContentAsync("#huong-dan") ?? "";
        Assert.Contains("24 giờ", hd);
        Assert.Contains("3 đặt chỗ", hd);
    }

    [Fact]
    public async Task TimHero_CuonToiKhamPha_KhongDoiRoute_KhongTaiLai()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var url = p.Url;
        await p.FillAsync("#txtTimHero", "lap trinh");
        await p.PressAsync("#txtTimHero", "Enter");
        await p.WaitForFunctionAsync("() => document.getElementById('kpKetQua').textContent.includes('lap trinh')");
        await ChoXongAsync(p);
        Assert.Equal(url, p.Url);
        Assert.True(await p.EvaluateAsync<bool>("() => window.__khongTaiLai === true"), "Không tải lại trang");
        Assert.Equal("lap trinh", await p.InputValueAsync("#txtTimSach"));
        var ten = await p.Locator("#kpLuoi .the-sach__ten").AllInnerTextsAsync();
        Assert.NotEmpty(ten);
        Assert.Contains(ten, t => t.Contains("Lập trình"));
        await p.WaitForTimeoutAsync(900);
        var top = await p.EvaluateAsync<double>("() => document.getElementById('kham-pha').getBoundingClientRect().top");
        Assert.InRange(top, -40, 260); // đã cuộn tới khu 5

        // Gợi ý từ khóa dưới Hero
        await p.EvaluateAsync("() => window.scrollTo(0, 0)");
        var chip = p.Locator("#goiYTuKhoa [data-tu-khoa]").First;
        var tk = await chip.GetAttributeAsync("data-tu-khoa");
        await chip.ClickAsync();
        await p.WaitForFunctionAsync($"() => document.getElementById('txtTimSach').value === {System.Text.Json.JsonSerializer.Serialize(tk)}");
        await ChoXongAsync(p);
        Assert.Equal(url, p.Url);
    }

    [Fact]
    public async Task DanhMuc_BamTheLoai_LocKhu5_ThanhCongCu_XemThem()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        var o = p.Locator(".danh-muc__o").First;
        var ma = await o.GetAttributeAsync("data-the-loai");
        var so = int.Parse((await o.Locator(".danh-muc__so").InnerTextAsync()).Split(' ')[0]);
        await o.ClickAsync();
        await p.WaitForFunctionAsync($"() => document.getElementById('cboTheLoai').value === '{ma}'");
        await ChoXongAsync(p);
        await p.WaitForFunctionAsync($"() => document.getElementById('kpKetQua').textContent.startsWith('{so} đầu sách')");
        Assert.True(await p.EvaluateAsync<bool>("() => window.__khongTaiLai === true"));
        Assert.Equal(TrinhDuyet.Web + "/", p.Url);

        // Chỉ sách còn có thể mượn + sắp xếp
        await p.SelectOptionAsync("#cboTheLoai", "");
        await p.CheckAsync("#chkConSach");
        await ChoXongAsync(p);
        var nhan = await p.Locator("#kpLuoi .the-sach__nhan").AllInnerTextsAsync();
        Assert.All(nhan, n => Assert.StartsWith("Còn", n));

        // Xem thêm: thêm thẻ, không thay thẻ cũ
        await p.UncheckAsync("#chkConSach");
        await ChoXongAsync(p);
        Assert.True(await p.IsVisibleAsync("#btnXemThem"));
        var truoc = await p.Locator("#kpLuoi .the-sach").CountAsync();
        var dau = await p.GetAttributeAsync("#kpLuoi .the-sach >> nth=0", "data-ma");
        await p.ClickAsync("#btnXemThem");
        await p.WaitForFunctionAsync($"() => document.querySelectorAll('#kpLuoi .the-sach').length > {truoc}");
        Assert.Equal(dau, await p.GetAttributeAsync("#kpLuoi .the-sach >> nth=0", "data-ma"));
    }

    [Fact]
    public async Task YeuCauMoiNhatThang_GoNhanhNhieuLan()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        // Làm chậm phản hồi đầu tiên để nó về SAU phản hồi cuối – kết quả phải là của yêu cầu cuối
        await p.RouteAsync("**/api/cong/sach?*", async (r) =>
        {
            if (r.Request.Url.Contains("q=co&")) await Task.Delay(1500);
            await r.ContinueAsync();
        });
        await p.FillAsync("#txtTimSach", "co");
        await p.ClickAsync("#btnTimSach");
        await p.FillAsync("#txtTimSach", "mang");
        await p.ClickAsync("#btnTimSach");
        await p.WaitForTimeoutAsync(2500); // phản hồi “co” (bị làm chậm) đã về hoặc đã bị hủy
        await ChoXongAsync(p);
        Assert.Contains("mang", await p.InnerTextAsync("#kpKetQua"));
        var mong = await p.EvaluateAsync<string[]>("async () => (await (await fetch('/api/cong/sach?q=mang&sapXep=moi&trang=1&coTrang=12')).json()).sach.map((s) => s.maDauSach)");
        var luoi = await p.EvaluateAsync<string[]>("() => [...document.querySelectorAll('#kpLuoi .the-sach')].map((e) => e.dataset.ma)");
        Assert.Equal(mong, luoi); // lưới là kết quả của yêu cầu cuối
    }

    [Fact]
    public async Task TheSach_MoBookPreview_TuKhoaTrongPreview_SangKhamPha()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await TrangChuAsync(ctx);
        // Đầu sách dữ liệu mẫu (có từ khóa) – kiểm thử tích hợp có thể thêm đầu sách không từ khóa
        await p.SelectOptionAsync("#cboSapXep", "ten");
        await ChoXongAsync(p);
        var the = p.Locator("#kpLuoi .the-sach[data-ma^='DS']").First;
        var ma = await the.GetAttributeAsync("data-ma");
        await the.Locator(".the-sach__mo").ClickAsync();
        await p.WaitForSelectorAsync(".bp.bp--da-mo");
        Assert.EndsWith("/sach/" + ma, p.Url);
        var chip = p.Locator(".bp-tu-khoa a.bp-chip").First;
        var tk = await chip.InnerTextAsync();
        await chip.ClickAsync();
        await p.WaitForURLAsync(u => u.Contains("/kham-pha?q="));
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        Assert.Equal(tk, await p.InputValueAsync("#txtTimSach"));
        Assert.Contains(tk, await p.InnerTextAsync("#kpKetQua"));
    }

    [Fact]
    public async Task KhamPhaToanTrang_LocNam_HuongDan_TrangCon()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/kham-pha");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        await p.FillAsync("#txtNamTu", "2010");
        await p.FillAsync("#txtNamDen", "2019");
        await p.PressAsync("#txtNamDen", "Enter");
        await ChoXongAsync(p);
        var nam = await p.Locator("#kpLuoi .the-sach__phu").AllInnerTextsAsync();
        Assert.All(nam, x => Assert.InRange(int.Parse(x.Split('·').Last().Trim()), 2010, 2019));

        await p.GotoAsync(TrinhDuyet.Web + "/huong-dan");
        var hd = await p.InnerTextAsync("main");
        Assert.Contains("Sinh viên", hd);
        Assert.Contains("14 ngày", hd);
        Assert.Contains("24 giờ", hd);
        Assert.Contains("/kich-hoat", await p.GetAttributeAsync("#tai-khoan a", "href"));
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/kham-pha")]
    [InlineData("/huong-dan")]
    public async Task Axe_CspThat_KhongCls(string duongDan)
    {
        await using var ctx = await td.MayTinhCspThatAsync();
        await ctx.AddInitScriptAsync("""
            (() => {
              document.addEventListener('securitypolicyviolation', (e) => (window.__csp ||= []).push(e.violatedDirective + ' ' + e.blockedURI));
              window.__cls = 0;
              new PerformanceObserver((l) => { for (const e of l.getEntries()) if (!e.hadRecentInput) window.__cls += e.value; })
                .observe({ type: 'layout-shift', buffered: true });
            })();
            """);
        var p = await ctx.NewPageAsync();
        var viPham = new List<string>();
        p.Console += (_, m) => { if (m.Text.Contains("Content Security Policy") || m.Text.Contains("Refused to")) viPham.Add(m.Text); };
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        if (duongDan != "/huong-dan") await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        await p.WaitForTimeoutAsync(1200);
        viPham.AddRange(await p.EvaluateAsync<string[]>("() => window.__csp || []"));
        Assert.True(viPham.Count == 0, "Vi phạm CSP: " + string.Join(" | ", viPham.Distinct()));
        var cls = await p.EvaluateAsync<double>("() => window.__cls");
        Assert.True(cls < 0.1, $"CLS = {cls:0.000} (ngưỡng tốt < 0.1)");
        var kq = await p.RunAxe();
        var loi = kq.Violations.Where(v => v.Impact is "critical" or "serious").ToList();
        Assert.True(loi.Count == 0, string.Join("\n", loi.Select(v => $"{v.Id}: {v.Help} – " + string.Join("; ", v.Nodes.Select(n => n.Target)))));
    }

    [Fact]
    public async Task DienThoai_LuoiHaiCot_KhongCuonNgang()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        Assert.Equal(2, await p.EvaluateAsync<int>("() => getComputedStyle(document.getElementById('kpLuoi')).gridTemplateColumns.split(' ').length"));
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));
    }
}
