using Deque.AxeCore.Playwright;

namespace ThuVien.E2E;

/// <summary>
/// Pha 4: tổng quan N1–N4 (KPI đếm lên, biểu đồ ECharts, KPI bấm được), 01.04.02/01.04.03 và 02.05.01/02.05.02
/// (kỳ báo cáo → Tổng hợp → Lập báo cáo → xem trước MB06/MB07 → In), trang in A4 có biểu đồ SVG, quyền, điện thoại, axe.
/// </summary>
[Collection("TrinhDuyet")]
public class Pha4Tests(TrinhDuyet td)
{
    private static async Task<IPage> MoAsync(IBrowserContext ctx, string ten, string duongDan)
    {
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, ten);
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        await p.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return p;
    }

    /// <summary>Mọi biểu đồ trên trang đã vẽ xong (có canvas) hoặc báo rỗng; không còn skeleton.</summary>
    internal static Task ChoBieuDoAsync(IPage p) => p.WaitForFunctionAsync("""
        () => [...document.querySelectorAll('.qt-bieu-do')].every((h) => !h.classList.contains('dang-tai')
          && (h.querySelector('.qt-bieu-do__khung canvas') || !h.querySelector('.qt-bieu-do__rong').hidden))
        """);

    /// <summary>KPI đếm lên xong (hết hiệu ứng countUp).</summary>
    internal static async Task ChoKpiAsync(IPage p)
    {
        await p.WaitForFunctionAsync("() => [...document.querySelectorAll('.qt-kpi__so')].every((e) => e.textContent.trim() !== '–')");
        await p.WaitForTimeoutAsync(1400);
    }

    private static string Iso(DateOnly d) => d.ToString("yyyy-MM-dd");
    private static DateOnly HomNay => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Ho_Chi_Minh"));
    internal static string Ky60 => $"tuNgay={Iso(HomNay.AddDays(-59))}&denNgay={Iso(HomNay)}";

    [Theory]
    [InlineData("muontra", "kpiDangMuon", new[] { "bdMuonTra30", "bdTop10", "bdLichMuon" })]
    [InlineData("vipham", "kpiChuaThanhToan", new[] { "bdViPhamTheoThang", "bdTienThu", "bdHinhThuc" })]
    [InlineData("khosach", "kpiTongBanSach", new[] { "bdTinhTrang", "bdKhuVuc", "bdHuHongMat" })]
    [InlineData("admin", "kpiTaiKhoan", new[] { "bdDangNhap30", "bdThaoTacNhom" })]
    public async Task TongQuan_TheoNhom_KpiVaBieuDo(string ten, string kpi, string[] bieuDo)
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, ten, "/quan-tri");
        await ChoBieuDoAsync(p);
        await ChoKpiAsync(p);
        Assert.Matches(@"^\d[\d.]*$", (await p.InnerTextAsync("#" + kpi)).Trim());
        foreach (var id in bieuDo) Assert.True(await p.Locator($"#{id}").IsVisibleAsync(), id);
        // Biểu đồ đầu tiên có dữ liệu mẫu → vẽ canvas; “Xem bảng số liệu” mở bảng thay thế
        await p.WaitForSelectorAsync($"#{bieuDo[0]} .qt-bieu-do__khung canvas");
        await p.ClickAsync($"#{bieuDo[0]} [data-hanh-dong='bang']");
        Assert.True(await p.Locator($"#{bieuDo[0]} .qt-bieu-do__bang table").IsVisibleAsync());
        Assert.Equal("true", await p.GetAttributeAsync($"#{bieuDo[0]} [data-hanh-dong='bang']", "aria-expanded"));
        // Trang tổng quan của nhóm khác không hiện
        Assert.Equal(1, await p.Locator(".qt-luoi-kpi").CountAsync());
    }

    [Fact]
    public async Task TongQuanN1_KpiQuaHan_MoDanhSachChiTiet()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri");
        await ChoKpiAsync(p);
        var so = int.Parse((await p.InnerTextAsync("#kpiQuaHan")).Replace(".", ""));
        Assert.True(so > 0, "Dữ liệu mẫu có sách quá hạn");
        await p.ClickAsync("[data-chi-tiet='qua-han']");
        await p.WaitForSelectorAsync("#dgvChiTietKpi tbody tr[data-chi-so]");
        Assert.Equal(so, await p.Locator("#dgvChiTietKpi tbody tr[data-chi-so]").CountAsync());
        Assert.Contains($"({so})", await p.InnerTextAsync("#chiTietKpiTieuDe"));
        // N3: KPI mở danh mục sách lọc theo tình trạng
        await using var ctx3 = await td.MayTinhAsync();
        var n3 = await MoAsync(ctx3, "khosach", "/quan-tri");
        await n3.ClickAsync("a.qt-kpi:has(#kpiHuHongMat)");
        await n3.WaitForURLAsync(u => u.Contains("/quan-tri/danh-muc/sach"));
        await n3.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Equal("Hư hỏng", await n3.InputValueAsync("#cboLocTinhTrang"));
    }

    [Fact]
    public async Task BaoCaoMuonTra_TongHop_LapBaoCao_XemTruoc_In()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/bao-cao/muon-tra");
        Assert.True(await p.IsDisabledAsync("#btnLapBaoCao"));
        Assert.True(await p.Locator("#khuKetQua").IsHiddenAsync());
        // Kỳ không hợp lệ: từ ngày > đến ngày → báo lỗi, không tổng hợp
        await p.EvaluateAsync("() => { document.getElementById('dtpTuNgay').value = '20/11/2026'; document.getElementById('dtpDenNgay').value = '01/11/2026'; }");
        await p.ClickAsync("#btnTongHop");
        await p.WaitForSelectorAsync(".qt-toast--loi");
        Assert.Equal("true", await p.GetAttributeAsync("#dtpTuNgay", "aria-invalid"));
        Assert.True(await p.IsDisabledAsync("#btnLapBaoCao"));

        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/bao-cao/muon-tra?" + Ky60);
        await p.WaitForSelectorAsync("#khuKetQua:not([hidden])");
        await ChoBieuDoAsync(p);
        await ChoKpiAsync(p);
        var tongMuon = int.Parse((await p.InnerTextAsync("#lblTongLuotMuon")).Replace(".", ""));
        Assert.True(tongMuon > 0);
        Assert.Equal(tongMuon, await p.Locator("#dgvLuotMuon tbody tr[data-chi-so]").CountAsync()); // chi tiết = tổng (SPEC 6.4.5)
        // Thẻ chi tiết: bàn phím → chuyển sang lượt gia hạn
        await p.FocusAsync("#tabLuotMuon");
        await p.Keyboard.PressAsync("ArrowRight");
        Assert.Equal("true", await p.GetAttributeAsync("#tabLuotGiaHan", "aria-selected"));
        Assert.True(await p.Locator("#khuLuotGiaHan").IsVisibleAsync());

        // Lập báo cáo (01.04.03) cùng kỳ → xem trước MB06
        Assert.False(await p.IsDisabledAsync("#btnLapBaoCao"));
        await p.ClickAsync("#btnLapBaoCao");
        await p.WaitForURLAsync(u => u.Contains("/quan-tri/bao-cao/muon-tra/lap"));
        await p.WaitForFunctionAsync("() => !document.getElementById('btnIn').disabled");
        var khung = p.FrameLocator("#khungXemTruoc");
        Assert.Contains("BÁO CÁO TÌNH HÌNH MƯỢN TRẢ SÁCH", await khung.Locator("h1.in-tieu-de").InnerTextAsync());
        Assert.Equal(tongMuon.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")),
            (await khung.Locator(".in-tong-hop strong").First.InnerTextAsync()).Trim());
        Assert.True(await khung.Locator("#inBieuDoTheoNgay svg").CountAsync() > 0, "Biểu đồ in dựng bằng SVG");
        // btnIn: ghi nhật ký (TT073) rồi in khung xem trước
        await p.EvaluateAsync("() => { const f = document.getElementById('khungXemTruoc'); f.contentWindow.print = () => { window.__daIn = true; }; }");
        await p.ClickAsync("#btnIn");
        await p.WaitForSelectorAsync(".qt-toast--ok");
        Assert.True(await p.EvaluateAsync<bool>("() => window.__daIn === true"));
    }

    [Fact]
    public async Task BaoCaoViPham_TongHop_TrangInMB07()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "vipham", "/quan-tri/bao-cao/vi-pham?ky=thang-nay");
        Assert.Equal("true", await p.GetAttributeAsync("[data-ky='thang-nay']", "aria-pressed"));
        await p.WaitForSelectorAsync("#khuKetQua:not([hidden])");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/bao-cao/vi-pham?tuNgay=" + Iso(HomNay.AddDays(-89)) + "&denNgay=" + Iso(HomNay));
        await ChoBieuDoAsync(p);
        await ChoKpiAsync(p);
        Assert.True(await p.Locator("#dgvTheoKhoanThu tbody tr[data-chi-so]").CountAsync() >= 4);
        await p.ClickAsync("#tabBienBan");
        Assert.True(await p.Locator("#dgvBienBan tbody tr[data-chi-so]").CountAsync() >= 5);

        var trang = await ctx.NewPageAsync();
        var r = await trang.GotoAsync(TrinhDuyet.Web + "/quan-tri/in/bao-cao-vi-pham?tuNgay=" + Iso(HomNay.AddDays(-89)) + "&denNgay=" + Iso(HomNay));
        Assert.Equal(200, r!.Status);
        Assert.Contains("BÁO CÁO TÌNH HÌNH VI PHẠM MƯỢN TRẢ SÁCH", await trang.InnerTextAsync("h1"));
        Assert.Contains("Nơi nhận: Ban giám đốc thư viện", await trang.InnerTextAsync("body"));
        await trang.WaitForSelectorAsync("#inBieuDoKhoanThu svg");
        await trang.WaitForSelectorAsync("#inBieuDoTyLe svg");
        // Kỳ sai → trang báo lỗi (422), không in số liệu sai
        var sai = await trang.GotoAsync(TrinhDuyet.Web + "/quan-tri/in/bao-cao-vi-pham?tuNgay=2026-02-01&denNgay=2026-01-01");
        Assert.Equal(422, sai!.Status);
    }

    [Fact]
    public async Task BaoCao_NgoaiQuyen_KhongVaoDuoc()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "vipham", "/quan-tri");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/bao-cao/muon-tra");
        Assert.Contains("/quan-tri/khong-co-quyen", p.Url);
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/in/bao-cao-muon-tra?" + Ky60);
        Assert.Contains("/quan-tri/khong-co-quyen", p.Url);
        await using var ctx1 = await td.MayTinhAsync();
        var n1 = await MoAsync(ctx1, "muontra", "/quan-tri");
        await n1.GotoAsync(TrinhDuyet.Web + "/quan-tri/bao-cao/vi-pham/lap");
        Assert.Contains("/quan-tri/khong-co-quyen", n1.Url);
    }

    /// <summary>
    /// CSP thật (không bỏ qua): trang in /quan-tri/in/… chỉ cho chính trang web nhúng khung (SAMEORIGIN, frame-ancestors 'self') –
    /// xem trước MB06 và in trong khung của btnIn hiển thị được; trang khác vẫn chống nhúng (DENY).
    /// Tổng quan có biểu đồ + tooltip, xem trước báo cáo, trang in: không vi phạm CSP nào.
    /// </summary>
    [Fact]
    public async Task CspThat_TrangIn_NhungKhungCungNguon_KhongViPham()
    {
        await using var ctx = await td.MayTinhCspThatAsync();
        await ctx.AddInitScriptAsync("() => document.addEventListener('securitypolicyviolation', (e) => (window.__csp ||= []).push(e.violatedDirective + ' ' + e.blockedURI + ' ' + (e.sample || '')))");
        var viPham = new List<string>();
        var p = await ctx.NewPageAsync();
        p.Console += (_, m) => { if (m.Text.Contains("Content Security Policy") || m.Text.Contains("Refused to")) viPham.Add(m.Text); };
        await TrinhDuyet.DangNhapNhanVienAsync(p, "muontra");

        var trangChu = await p.GotoAsync(TrinhDuyet.Web + "/quan-tri");
        Assert.Equal("DENY", trangChu!.Headers["x-frame-options"]);
        Assert.Contains("frame-ancestors 'none'", trangChu.Headers["content-security-policy"]);
        await ChoBieuDoAsync(p);
        await p.HoverAsync("#bdMuonTra30 .qt-bieu-do__khung canvas", new() { Position = new() { X = 300, Y = 150 } });
        await p.WaitForTimeoutAsync(500);
        viPham.AddRange(await p.EvaluateAsync<string[]>("() => window.__csp || []"));

        var trangIn = await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/in/bao-cao-muon-tra?" + Ky60);
        Assert.Equal("SAMEORIGIN", trangIn!.Headers["x-frame-options"]);
        Assert.Contains("frame-ancestors 'self'", trangIn.Headers["content-security-policy"]);
        await p.WaitForSelectorAsync("#inBieuDoTheoNgay svg");
        viPham.AddRange(await p.EvaluateAsync<string[]>("() => window.__csp || []"));

        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/bao-cao/muon-tra/lap?" + Ky60);
        await p.WaitForFunctionAsync("() => !document.getElementById('btnIn').disabled");
        var khung = p.FrameLocator("#khungXemTruoc");
        Assert.Contains("BÁO CÁO TÌNH HÌNH MƯỢN TRẢ SÁCH", await khung.Locator("h1.in-tieu-de").InnerTextAsync(new() { Timeout = 10_000 }));
        await khung.Locator("#inBieuDoTheoNgay svg").First.WaitForAsync(new() { Timeout = 10_000 });

        foreach (var f in p.Frames)
            viPham.AddRange(await f.EvaluateAsync<string[]>("() => window.__csp || []"));
        Assert.True(viPham.Count == 0, "Vi phạm CSP: " + string.Join(" | ", viPham.Distinct()));
    }

    [Theory]
    [InlineData("muontra", "/quan-tri")]
    [InlineData("vipham", "/quan-tri")]
    [InlineData("khosach", "/quan-tri")]
    [InlineData("muontra", "/quan-tri/bao-cao/muon-tra?ky=thang-nay")]
    [InlineData("vipham", "/quan-tri/bao-cao/vi-pham/lap?ky=thang-nay")]
    public async Task DienThoai_KhongTranNgang(string ten, string duongDan)
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await MoAsync(ctx, ten, duongDan);
        await p.WaitForTimeoutAsync(800);
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"),
            "Trang tràn ngang trên điện thoại");
    }

    [Theory]
    [InlineData("muontra", "/quan-tri")]
    [InlineData("vipham", "/quan-tri")]
    [InlineData("khosach", "/quan-tri")]
    [InlineData("muontra", "/quan-tri/bao-cao/muon-tra?ky=thang-nay")]
    [InlineData("vipham", "/quan-tri/bao-cao/vi-pham?ky=thang-nay")]
    [InlineData("muontra", "/quan-tri/bao-cao/muon-tra/lap?ky=thang-nay")]
    public async Task Axe_TongQuanVaBaoCao(string ten, string duongDan)
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, ten, duongDan);
        await ChoBieuDoAsync(p);
        await p.WaitForTimeoutAsync(1500);
        var kq = await p.RunAxe();
        var loi = kq.Violations.Where(v => v.Impact is "critical" or "serious").ToList();
        Assert.True(loi.Count == 0, string.Join("\n", loi.Select(v => $"{v.Id}: {v.Help} – " + string.Join("; ", v.Nodes.Select(n => n.Target)))));
    }
}
