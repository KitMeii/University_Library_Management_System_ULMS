namespace ThuVien.E2E;

/// <summary>
/// Ảnh Pha 5 (không thuộc mục IV Bài 4): trang chủ cổng bạn đọc (header, hero, kệ sách coverflow, footer) máy tính và điện thoại,
/// ảnh đặt cạnh ảnh tham khảo kệ sách (file 05 mục 7), trang quản lý đầu sách và thể loại.
/// </summary>
[Collection("TrinhDuyet")]
public class ChupAnhPha5Tests(TrinhDuyet td)
{
    private static string ThuMucThamKhao =>
        Path.Combine(Environment.GetEnvironmentVariable("ULMS_ROOT") ?? "/src", "Prompt/Prompt_ClaudeCode_HeThong/docs/tham_khao");

    /// <summary>Kệ đã dựng 3D, hết hiệu ứng vào.</summary>
    internal static async Task ChoKeAsync(IPage p)
    {
        await p.WaitForSelectorAsync("#ke-sach.cf-3d .book[aria-current='true']");
        await p.EvaluateAsync("() => document.getElementById('ke-sach').scrollIntoView({ block: 'start' })");
        await p.WaitForTimeoutAsync(1600);
    }

    [Fact]
    public async Task Chup_TrangChu_MayTinh()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await TrinhDuyet.ChupAsync(p, "Cong_Trang_chu_Hero");
        await ChoKeAsync(p);
        await p.Locator("#ke-sach").ScreenshotAsync(new() { Path = Path.Combine(TrinhDuyet.ThuMucAnh, "Cong_Ke_sach.png") });
        await p.EvaluateAsync("() => window.scrollTo(0, 0)");
        await TrinhDuyet.ChupAsync(p, "Cong_Trang_chu", toanTrang: true);

        // Đổi kệ: Mượn nhiều nhất (thanh thứ hạng)
        await p.EvaluateAsync("() => document.getElementById('ke-sach').scrollIntoView({ block: 'start' })");
        await p.ClickAsync("#cfKeSau");
        await p.WaitForFunctionAsync("() => document.getElementById('ke-sach').dataset.trangThai === 'KE_SAN_SANG' && document.getElementById('tenKeHienTai').textContent !== 'Sách mới về'");
        await p.WaitForTimeoutAsync(900);
        await p.Locator("#ke-sach").ScreenshotAsync(new() { Path = Path.Combine(TrinhDuyet.ThuMucAnh, "Cong_Ke_sach_Muon_nhieu.png") });

        // Đặt cạnh ảnh tham khảo (file 05 mục 7)
        var thamKhao = Path.Combine(ThuMucThamKhao, "hieu_ung_ke_sach_coverflow.png");
        if (File.Exists(thamKhao))
        {
            var a = Convert.ToBase64String(await File.ReadAllBytesAsync(thamKhao));
            var b = Convert.ToBase64String(await File.ReadAllBytesAsync(Path.Combine(TrinhDuyet.ThuMucAnh, "Cong_Ke_sach.png")));
            var ss = await ctx.NewPageAsync();
            await ss.SetViewportSizeAsync(1366, 1500);
            await ss.SetContentAsync($$"""
                <html><body style="margin:0;font:600 15px system-ui;background:#fff">
                <div style="padding:10px 16px">Ảnh tham khảo (docs/tham_khao/hieu_ung_ke_sach_coverflow.png)</div>
                <img style="display:block;width:100%" src="data:image/png;base64,{{a}}">
                <div style="padding:10px 16px">Kệ sách nổi bật – cổng bạn đọc ULMS (1366×768)</div>
                <img style="display:block;width:100%" src="data:image/png;base64,{{b}}">
                </body></html>
                """);
            await ss.ScreenshotAsync(new() { Path = Path.Combine(TrinhDuyet.ThuMucAnh, "Cong_Ke_sach_doi_chieu_tham_khao.png"), FullPage = true });
        }
    }

    [Fact]
    public async Task Chup_TrangChu_DienThoai()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await TrinhDuyet.ChupAsync(p, "Cong_Trang_chu_Hero_mobile");
        await ChoKeAsync(p);
        await TrinhDuyet.ChupAsync(p, "Cong_Ke_sach_mobile");
        await p.EvaluateAsync("() => window.scrollTo(0, 0)");
        await TrinhDuyet.ChupAsync(p, "Cong_Trang_chu_mobile", toanTrang: true);
        await p.ClickAsync("[aria-controls='congMenu']");
        await p.WaitForSelectorAsync("#congMenu.show");
        await p.WaitForTimeoutAsync(400);
        await TrinhDuyet.ChupAsync(p, "Cong_Menu_mobile");
    }

    [Fact]
    public async Task Chup_QuanLyDauSach_TheLoai()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "khosach");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/danh-muc/dau-sach");
        await p.WaitForSelectorAsync("#dgvDauSach tbody tr[data-chi-so]");
        await p.ClickAsync("#dgvDauSach tbody tr[data-chi-so='0']");
        await p.WaitForSelectorAsync("#khuBanSach:not([hidden]) #dgvBanSachDauSach tbody tr");
        await p.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await TrinhDuyet.ChupAsync(p, "Pha5_Quan_ly_dau_sach", toanTrang: true);
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/danh-muc/the-loai");
        await p.WaitForSelectorAsync("#dgvTheLoai tbody tr[data-chi-so]");
        await p.ClickAsync("#dgvTheLoai tbody tr[data-chi-so='0']");
        await TrinhDuyet.ChupAsync(p, "Pha5_Quan_ly_the_loai", toanTrang: true);
    }
}
