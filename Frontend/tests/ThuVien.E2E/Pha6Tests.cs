using Deque.AxeCore.Playwright;

namespace ThuVien.E2E;

/// <summary>
/// Pha 6 – Book Preview (file 06 mục 1), đặt chỗ phía bạn đọc (mục 6), trang cá nhân + thông báo + kích hoạt (file 07),
/// Quầy đặt chỗ + dgvDatCho/btnNhanDatCho (file 06 mục 4–5), chế độ chụp ảnh Bài 4 ẩn phần mở rộng (D61).
/// </summary>
[Collection("TrinhDuyet")]
public class Pha6Tests(TrinhDuyet td)
{
    /// <summary>Một đầu sách trên kệ “Sách mới về” còn bản có thể đặt (đọc từ API công khai).</summary>
    internal static Task<string> DauSachConBanAsync(IPage p) => p.EvaluateAsync<string>("""
        async () => {
          const ke = await (await fetch('/api/cong/ke-sach?ke=moi')).json();
          return ke.sach.filter((s) => s.soBanCoTheMuon >= 2).at(-1).maDauSach;
        }
        """);

    internal static async Task MoSachAsync(IPage p, string ma)
    {
        await p.GotoAsync(TrinhDuyet.Web + "/sach/" + ma);
        await p.WaitForSelectorAsync(".bp.bp--da-mo");
    }

    [Fact]
    public async Task BookPreview_TuKeSach_BackDong_LienKetSau_GiuFocus()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#ke-sach.cf-3d .book[aria-current='true']");
        await p.EvaluateAsync("() => document.getElementById('ke-sach').scrollIntoView({ block: 'start' })");
        await p.WaitForTimeoutAsync(400);
        var ma = await p.GetAttributeAsync("#ke-sach .book[aria-current='true']", "data-ma-dau-sach");

        // Mở từ cuốn giữa: URL /sach/{ma}, hai trang (StPageFlip), tiêu đề sách
        await p.ClickAsync("#ke-sach .book[aria-current='true'] .book__body");
        await p.WaitForSelectorAsync(".bp.bp--da-mo");
        Assert.EndsWith("/sach/" + ma, p.Url);
        Assert.True(await p.Locator(".bp .stf__parent").CountAsync() == 1, "StPageFlip đã dựng trang");
        Assert.False(string.IsNullOrWhiteSpace(await p.InnerTextAsync("#bpTieuDe")));
        Assert.True(await p.IsVisibleAsync("#btnDatCho"));
        // Focus bị giữ trong preview
        for (var i = 0; i < 12; i++) await p.Keyboard.PressAsync("Tab");
        Assert.True(await p.EvaluateAsync<bool>("() => !!document.activeElement.closest('.bp')"));

        // Nút Back của trình duyệt đóng sách, URL về trang chủ, focus về cuốn nguồn
        await p.GoBackAsync();
        await p.WaitForSelectorAsync(".bp", new() { State = WaitForSelectorState.Detached });
        Assert.Equal(TrinhDuyet.Web + "/", p.Url);

        // Liên kết sâu: mở thẳng sách; × đóng về trang chủ
        await MoSachAsync(p, ma!);
        await p.ClickAsync("#btnDongSach");
        await p.WaitForSelectorAsync(".bp", new() { State = WaitForSelectorState.Detached });
        Assert.Equal(TrinhDuyet.Web + "/", p.Url);
        // Bấm nền cũng đóng
        await MoSachAsync(p, ma!);
        await p.Mouse.ClickAsync(8, 400);
        await p.WaitForSelectorAsync(".bp", new() { State = WaitForSelectorState.Detached });
        // Mã không tồn tại → 404
        var r = await p.GotoAsync(TrinhDuyet.Web + "/sach/KHONGCO999");
        Assert.Equal(404, r!.Status);
    }

    [Fact]
    public async Task DatCho_ChuaDangNhap_DangNhapTrongTrang_VeGiuCho_HuyDatCho()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        var ma = await DauSachConBanAsync(p);
        await MoSachAsync(p, ma);

        // Chưa đăng nhập → hộp đăng nhập ngay trong trang sách, đăng nhập xong tự đặt chỗ
        await p.ClickAsync("#btnDatCho");
        await p.WaitForSelectorAsync("#frmDangNhapNhanh");
        await p.FillAsync("#bpMaSv", "SV22000001");
        await p.FillAsync("#bpMatKhau", TrinhDuyet.MatKhauBanDoc);
        await p.ClickAsync("#btnDangNhapNhanh");
        await p.WaitForSelectorAsync("#veGiuCho");
        Assert.Matches(@"^DC\d{8}-\d{4}$", (await p.InnerTextAsync(".bp-ve__ma")).Trim());
        Assert.True(await p.Locator("#veGiuCho .ma-vach rect").CountAsync() > 20, "Mã vạch Code128 vẽ bằng SVG");
        Assert.Contains("giờ", await p.InnerTextAsync(".bp-ve__dem"));
        Assert.EndsWith("/sach/" + ma, p.Url);

        // Hủy đặt chỗ (xác nhận) → quay lại nút Đặt chỗ
        p.Dialog += async (_, d) => await d.AcceptAsync();
        await p.ClickAsync("#btnHuyDatCho");
        await p.WaitForSelectorAsync("#btnDatCho");
        // Đóng → trang tải lại để header hiện tên bạn đọc
        await p.ClickAsync("#btnDongSach");
        await p.WaitForSelectorAsync(".cong-header a[href='/tai-khoan']");
        Assert.Contains("Nguyễn Minh Anh", await p.InnerTextAsync(".cong-header"));
    }

    [Fact]
    public async Task DatCho_KhongDuDieuKien_HienLyDoVaHuongXuLy()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(p, "SV22000008"); // dữ liệu mẫu: biên bản vi phạm chưa thanh toán
        var ma = await DauSachConBanAsync(p);
        await MoSachAsync(p, ma);
        await p.ClickAsync("#btnDatCho");
        await p.WaitForSelectorAsync("#bpLyDo");
        var lyDo = await p.InnerTextAsync("#bpLyDo");
        Assert.Contains("chưa thanh toán", lyDo);
        Assert.Contains("thanh toán khoản vi phạm tại quầy", lyDo);
    }

    [Fact]
    public async Task BookPreview_DienThoai_MotTrang_NutDatChoGhimDay_LatSangTrangHanhDong()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        var ma = await DauSachConBanAsync(p);
        await MoSachAsync(p, ma);
        Assert.True(await p.IsVisibleAsync("#btnDatChoGhim"), "Nút Đặt chỗ ghim ở đáy màn hình");
        var hop = (await p.Locator("#btnDatChoGhim").BoundingBoxAsync())!;
        Assert.True(hop.Y + hop.Height > p.ViewportSize!.Height - 80, "Nút ghim sát đáy màn hình");
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));
        // Bấm nút ghim khi chưa đăng nhập → lật sang trang 2 (khối hành động) với hộp đăng nhập
        await p.ClickAsync("#btnDatChoGhim");
        await p.WaitForSelectorAsync("#frmDangNhapNhanh");
        await p.WaitForTimeoutAsync(1200);
        Assert.True(await p.IsVisibleAsync("#bpMaSv"));
    }

    [Fact]
    public async Task BookPreview_GiamChuyenDong_MoNhanh_CspThat_Axe()
    {
        await using var ctx = await td.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1366, Height = 768 }, Locale = "vi-VN", ReducedMotion = ReducedMotion.Reduce, BypassCSP = false
        });
        await ctx.AddInitScriptAsync("(() => document.addEventListener('securitypolicyviolation', (e) => (window.__csp ||= []).push(e.violatedDirective + ' ' + e.blockedURI + ' ' + (e.sample || ''))))()");
        var p = await ctx.NewPageAsync();
        var viPham = new List<string>();
        p.Console += (_, m) => { if (m.Text.Contains("Content Security Policy") || m.Text.Contains("Refused to")) viPham.Add(m.Text); };
        await p.GotoAsync(TrinhDuyet.Web + "/");
        var ma = await DauSachConBanAsync(p);
        var batDau = DateTime.UtcNow;
        await MoSachAsync(p, ma);
        Assert.True(DateTime.UtcNow - batDau < TimeSpan.FromSeconds(4), "Giảm chuyển động: mở bằng mờ dần ngắn");
        viPham.AddRange(await p.EvaluateAsync<string[]>("() => window.__csp || []"));
        Assert.True(viPham.Count == 0, "Vi phạm CSP: " + string.Join(" | ", viPham.Distinct()));
        // D69: thẻ <style> page-flip tự chèn được CSP cho phép bằng băm SHA-256 (không sửa thư viện) và thực sự có hiệu lực
        Assert.True(await p.EvaluateAsync<bool>("""
            () => [...document.querySelectorAll('style')].some((s) => s.textContent.includes('.stf__parent'))
                  && getComputedStyle(document.querySelector('.stf__parent')).position === 'relative'
                  && getComputedStyle(document.querySelector('.stf__block')).position === 'absolute'
            """), "CSS của page-flip phải được áp dụng dưới CSP thật");

        var kq = await p.RunAxe();
        var loi = kq.Violations.Where(v => v.Impact is "critical" or "serious").ToList();
        Assert.True(loi.Count == 0, string.Join("\n", loi.Select(v => $"{v.Id}: {v.Help} – " + string.Join("; ", v.Nodes.Select(n => n.Target)))));
    }

    [Theory]
    [InlineData("")]
    [InlineData("dang-muon")]
    [InlineData("dat-cho")]
    [InlineData("lich-su")]
    [InlineData("vi-pham")]
    [InlineData("yeu-thich")]
    [InlineData("thong-bao")]
    [InlineData("doi-mat-khau")]
    public async Task TrangCaNhan_TatCaTrang_TaiDuLieu_KhongLoi_Axe(string trang)
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(p, "SV22000007");
        await p.GotoAsync(TrinhDuyet.Web + "/tai-khoan" + (trang == "" ? "" : "/" + trang));
        await p.WaitForSelectorAsync("#tkNoiDung[aria-busy='false']");
        Assert.Equal(0, await p.Locator("#tkThan .cong-loi:not([hidden])").CountAsync());
        Assert.True(await p.Locator("#tkThan .tk-the").CountAsync() >= 1);
        Assert.Equal("page", await p.GetAttributeAsync(".tk-vien-thuoc[aria-current]", "aria-current"));
        await p.WaitForTimeoutAsync(700);
        var kq = await p.RunAxe();
        var loi = kq.Violations.Where(v => v.Impact is "critical" or "serious").ToList();
        Assert.True(loi.Count == 0, string.Join("\n", loi.Select(v => $"{v.Id}: {v.Help} – " + string.Join("; ", v.Nodes.Select(n => n.Target)))));
        if (trang == "dat-cho") Assert.True(await p.Locator("#tkThan .ma-vach").CountAsync() >= 1, "Đặt chỗ đang giữ có mã vạch");
    }

    [Fact]
    public async Task TrangCaNhan_DienThoai_ThanhTabDuoiDay_ChuongThongBao()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(p, "SV22000007");
        await p.GotoAsync(TrinhDuyet.Web + "/tai-khoan");
        await p.WaitForSelectorAsync("#tkNoiDung[aria-busy='false']");
        Assert.Equal("fixed", await p.EvaluateAsync<string>("() => getComputedStyle(document.querySelector('.tk-dieu-huong')).position"));
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));

        // Chuông: số chưa đọc + danh sách thông báo
        await p.WaitForSelectorAsync("#soThongBaoChuaDoc:not([hidden])");
        await p.ClickAsync("#btnChuongCong");
        await p.WaitForSelectorAsync("#dsThongBaoCong .tb-dong");
        Assert.True(await p.Locator("#dsThongBaoCong .tb-dong").CountAsync() >= 1);
    }

    [Fact]
    public async Task KichHoat_SaiThongTin_BaoLoi_LienKetTuTrangDangNhap()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/dang-nhap");
        await p.ClickAsync("#lnkKichHoat");
        await p.WaitForURLAsync(TrinhDuyet.Web + "/kich-hoat");
        await p.FillAsync("#maSvHvCb", "CB0015");
        await p.FillAsync("#soTheBd", "SAI000000");
        await p.FillAsync("#ngaySinh", "1985-03-01");
        await p.FillAsync("#matKhauMoi", "Matkhau123");
        await p.FillAsync("#nhapLai", "Matkhau123");
        await p.ClickAsync("#btnKichHoat");
        await p.WaitForSelectorAsync("#thongBaoLoi");
        Assert.Contains("không khớp", await p.InnerTextAsync("#thongBaoLoi"));
    }

    // ───────────── Nghiệp vụ N1 (mở rộng) ─────────────

    [Fact]
    public async Task N1_LapPhieuMuon_DgvDatCho_BtnNhanDatCho_DuaVaoPhieu()
    {
        await using var ctx = await td.MayTinhAsync();
        await TrinhDuyet.BatMoRongAsync(ctx);
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "muontra");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/phieu-muon/lap");
        await p.FillAsync("#txtMaSV", "SV22000007"); // dữ liệu mẫu: 2 đặt chỗ đang giữ
        await p.ClickAsync("#btnTimBanDoc");
        await p.WaitForSelectorAsync("#khuDatCho:not([hidden]) #dgvDatCho tbody tr[data-chi-so]");
        var soGiu = await p.Locator("#dgvDatCho tbody tr[data-chi-so]").CountAsync();
        Assert.True(soGiu >= 1);
        Assert.True(await p.IsDisabledAsync("#btnIn"), "Trạng thái 2: chưa có dòng");
        await p.ClickAsync("#btnNhanDatCho");
        await p.WaitForFunctionAsync($"() => document.querySelectorAll('#dgvChiTiet tbody tr[data-chi-so]').length === {soGiu}");
        Assert.True(await p.IsEnabledAsync("#btnIn"), "Trạng thái 3 sau khi đưa sách đặt chỗ vào phiếu");
        await p.ClickAsync("#btnHuy"); // không lưu – giữ dữ liệu mẫu
    }

    [Fact]
    public async Task N1_QuayDatCho_DanhSach_Chuong_InPhieuSoan_TongQuan()
    {
        await using var ctx = await td.MayTinhAsync();
        await TrinhDuyet.BatMoRongAsync(ctx);
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "muontra");
        Assert.True(await p.Locator("#qtSidebar a[href='/quan-tri/dat-cho']").CountAsync() == 1, "Menu có Quầy đặt chỗ");
        Assert.True(await p.IsVisibleAsync("#lnkChuongDatCho"));
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri"); // màn hình mặc định của N1 là Lập Phiếu mượn
        await p.WaitForSelectorAsync("#kpiDatChoDangGiu");
        await p.WaitForSelectorAsync("#bdDatCho");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/dat-cho");
        await p.WaitForSelectorAsync("#dgvDatChoQuay tbody tr[data-chi-so]");
        Assert.True(int.Parse(await p.InnerTextAsync("#soDatCho")) >= 1);
        // Sắp theo hạn gần nhất
        var han = await p.EvaluateAsync<string[]>("() => [...document.querySelectorAll('#dgvDatChoQuay .qt-dem-nguoc')].map((e) => e.dataset.hetHan)");
        Assert.Equal(han.OrderBy(x => x).ToArray(), han);
        // Phiếu soạn sách in A4
        var r = await p.Context.APIRequest.GetAsync(TrinhDuyet.Web + "/quan-tri/in/phieu-soan-sach");
        Assert.Equal(200, r.Status);
        Assert.Contains("PHIẾU SOẠN SÁCH", await r.TextAsync());
    }

    /// <summary>D61: chế độ chụp ảnh Bài 4 (ẩn nhãn DỮ LIỆU MẪU) ẩn mọi mục menu và control mở rộng; bật cookie kiểm thử thì hiện.</summary>
    [Fact]
    public async Task CheDoAnhBai4_AnPhanMoRong()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "muontra");
        Assert.Equal(0, await p.Locator("#qtSidebar a[href='/quan-tri/dat-cho']").CountAsync());
        Assert.Equal(0, await p.Locator("#lnkChuongDatCho").CountAsync());
        Assert.Equal(0, await p.Locator("#kpiDatChoDangGiu, #bdDatCho").CountAsync());
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/phieu-muon/lap");
        Assert.Equal(0, await p.Locator("#khuDatCho, #btnNhanDatCho").CountAsync());

        await using var ctx3 = await td.MayTinhAsync();
        var p3 = await ctx3.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p3, "khosach");
        Assert.Equal(0, await p3.Locator("#qtSidebar a[href='/quan-tri/danh-muc/dau-sach'], #qtSidebar a[href='/quan-tri/danh-muc/the-loai']").CountAsync());

        await using var ctx4 = await td.MayTinhAsync();
        var p4 = await ctx4.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p4, "admin");
        await p4.GotoAsync(TrinhDuyet.Web + "/quan-tri/danh-muc/ban-doc");
        Assert.Equal(0, await p4.Locator("#khuTaiKhoanCong").CountAsync());
        // Cookie kiểm thử (chỉ Development) → hiện phần mở rộng
        await TrinhDuyet.BatMoRongAsync(ctx4);
        await p4.ReloadAsync();
        Assert.Equal(1, await p4.Locator("#khuTaiKhoanCong").CountAsync());
        Assert.True(await p4.Locator("#qtSidebar a[href='/quan-tri/danh-muc/dau-sach']").CountAsync() == 1);
    }
}
