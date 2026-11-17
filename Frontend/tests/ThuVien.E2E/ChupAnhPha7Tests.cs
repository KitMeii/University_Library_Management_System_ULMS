namespace ThuVien.E2E;

/// <summary>Ảnh Pha 7 (không thuộc mục IV Bài 4): trang chủ hoàn chỉnh 9 khu vực, khu Khám phá sách, trang /kham-pha, /huong-dan.</summary>
[Collection("TrinhDuyet")]
public class ChupAnhPha7Tests(TrinhDuyet td)
{
    private static string Duong(string ten) => Path.Combine(TrinhDuyet.ThuMucAnh, ten + ".png");

    /// <summary>Cuộn hết trang để mọi hiệu ứng xuất hiện khi cuộn (ScrollTrigger) chạy xong trước khi chụp toàn trang.</summary>
    private static async Task CuonHetAsync(IPage p)
    {
        await p.EvaluateAsync("""
            async () => {
              for (let y = 0; y < document.body.scrollHeight; y += 500) { window.scrollTo(0, y); await new Promise((r) => setTimeout(r, 120)); }
              window.scrollTo(0, 0);
            }
            """);
        await p.WaitForTimeoutAsync(1500);
    }

    [Fact]
    public async Task Chup_TrangChu_HoanChinh_MayTinh()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        await CuonHetAsync(p);
        await TrinhDuyet.ChupAsync(p, "Cong_Trang_chu_hoan_chinh", toanTrang: true);
        await p.Locator("#danh-muc").ScreenshotAsync(new() { Path = Duong("Cong_Danh_muc_the_loai") });

        // Tìm từ Hero → kết quả ở khu Khám phá sách
        await p.FillAsync("#txtTimHero", "co so du lieu");
        await p.PressAsync("#txtTimHero", "Enter");
        await p.WaitForFunctionAsync("() => document.getElementById('kpKetQua').textContent.includes('co so du lieu')");
        await p.WaitForTimeoutAsync(1200);
        await p.Locator("#kham-pha").ScreenshotAsync(new() { Path = Duong("Cong_Kham_pha_Tim_kiem") });

        // Bấm ô thể loại → lọc
        await p.Locator(".danh-muc__o").Nth(1).ClickAsync();
        await p.WaitForTimeoutAsync(1500);
        await p.Locator("#kham-pha").ScreenshotAsync(new() { Path = Duong("Cong_Kham_pha_Loc_the_loai") });
        await p.Locator("#huong-dan").ScreenshotAsync(new() { Path = Duong("Cong_Huong_dan_dat_cho") });
        await p.Locator("#so-lieu").ScrollIntoViewIfNeededAsync();
        await p.WaitForTimeoutAsync(1600);
        await p.Locator("#so-lieu").ScreenshotAsync(new() { Path = Duong("Cong_Thu_vien_bang_so") });
    }

    [Fact]
    public async Task Chup_TrangChu_DienThoai_KhamPha_HuongDan()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        await CuonHetAsync(p);
        await TrinhDuyet.ChupAsync(p, "Cong_Trang_chu_hoan_chinh_mobile", toanTrang: true);

        await using var ctx2 = await td.MayTinhAsync();
        var q = await ctx2.NewPageAsync();
        await q.GotoAsync(TrinhDuyet.Web + "/kham-pha?q=" + Uri.EscapeDataString("kinh tế"));
        await q.WaitForSelectorAsync("#kpLuoi .the-sach");
        await q.WaitForTimeoutAsync(900);
        await TrinhDuyet.ChupAsync(q, "Cong_Kham_pha_toan_trang", toanTrang: true);
        await q.GotoAsync(TrinhDuyet.Web + "/huong-dan");
        await TrinhDuyet.ChupAsync(q, "Cong_Huong_dan", toanTrang: true);
    }
    /// <summary>Ảnh các điểm học từ trang tham khảo (THAM_KHAO_MTA.md): menu thả xuống, header thu gọn, Hero có mũi tên, footer, menu điện thoại.</summary>
    [Fact]
    public async Task Chup_DieuHuong_ThamKhao()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        await p.WaitForTimeoutAsync(1200);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Hero_mui_ten") });
        await p.FocusAsync("[aria-controls='dd-danh-muc']");
        await p.Keyboard.PressAsync("ArrowDown");
        await p.WaitForFunctionAsync("() => document.querySelectorAll('#dd-danh-muc a[data-loc-the-loai]').length > 0");
        await p.WaitForTimeoutAsync(500);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Menu_tha_xuong_Danh_muc") });
        await p.Keyboard.PressAsync("Escape");
        await p.EvaluateAsync("() => document.activeElement?.blur()");
        await p.Mouse.MoveAsync(700, 600);
        await p.Mouse.WheelAsync(0, 1600);
        await p.WaitForFunctionAsync("() => document.querySelector('.cong-header').classList.contains('thu-gon')");
        await p.WaitForTimeoutAsync(900);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Header_thu_gon") });
        // Footer: trang riêng ở chế độ giảm chuyển động (Lenis tắt) để cuộn thẳng xuống đáy
        var r = await ctx.NewPageAsync();
        await r.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await r.GotoAsync(TrinhDuyet.Web + "/");
        await r.WaitForSelectorAsync("#kpLuoi .the-sach");
        await r.EvaluateAsync("() => window.scrollTo(0, document.documentElement.scrollHeight)");
        await r.WaitForTimeoutAsync(800);
        await r.ScreenshotAsync(new() { Path = Duong("Cong_Footer_ba_cot") });

        await using var ctx2 = await td.DienThoaiAsync();
        var q = await ctx2.NewPageAsync();
        await q.GotoAsync(TrinhDuyet.Web + "/");
        await q.WaitForSelectorAsync("#kpLuoi .the-sach");
        await q.ClickAsync(".cong-header__menu");
        await q.WaitForSelectorAsync("#congMenu.show");
        await q.Locator("#congMenu details.cong-offcanvas__nhom summary").First.ClickAsync();
        await q.WaitForFunctionAsync("() => document.querySelectorAll('#congMenu details a[data-loc-the-loai]').length > 0");
        await q.WaitForTimeoutAsync(500);
        await q.ScreenshotAsync(new() { Path = Duong("Cong_Menu_dien_thoai_nhom") });
    }
}
