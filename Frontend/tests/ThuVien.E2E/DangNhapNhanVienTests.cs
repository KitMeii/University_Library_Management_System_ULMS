namespace ThuVien.E2E;

/// <summary>SPEC 6.3.1 frmDangNhap: control đúng id, trạng thái nút, thông báo, màn hình mặc định & menu theo nhóm (SPEC 3.3).</summary>
[Collection("TrinhDuyet")]
public class DangNhapNhanVienTests(TrinhDuyet td)
{
    [Fact]
    public async Task Load_TrangThaiNutDungSpec()
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri/dang-nhap");

        // (Load): xóa trắng, con trỏ tại txtTenDangNhap; btnDangNhap Enable = False; btnThoat Enable = True
        Assert.Equal("", await page.InputValueAsync("#txtTenDangNhap"));
        Assert.Equal("", await page.InputValueAsync("#txtMatKhau"));
        Assert.Equal("txtTenDangNhap", await page.EvaluateAsync<string>("() => document.activeElement.id"));
        Assert.True(await page.IsDisabledAsync("#btnDangNhap"));
        Assert.True(await page.IsEnabledAsync("#btnThoat"));

        // Nhập đủ cả hai ô → Enable = True; thiếu một ô → Enable = False
        await page.FillAsync("#txtTenDangNhap", "admin");
        Assert.True(await page.IsDisabledAsync("#btnDangNhap"));
        await page.FillAsync("#txtMatKhau", "x");
        Assert.True(await page.IsEnabledAsync("#btnDangNhap"));
        await page.FillAsync("#txtTenDangNhap", "");
        Assert.True(await page.IsDisabledAsync("#btnDangNhap"));
    }

    [Fact]
    public async Task SaiMatKhau_ThongBao_XoaTrangOMatKhau()
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri/dang-nhap");
        await page.FillAsync("#txtTenDangNhap", "khongtontai");
        await page.FillAsync("#txtMatKhau", "SaiMatKhau1");
        await page.ClickAsync("#btnDangNhap");
        await page.WaitForSelectorAsync("#thongBaoLoi");
        Assert.Contains("Tên đăng nhập hoặc mật khẩu không đúng", await page.InnerTextAsync("#thongBaoLoi"));
        Assert.Equal("", await page.InputValueAsync("#txtMatKhau"));
        Assert.Equal("khongtontai", await page.InputValueAsync("#txtTenDangNhap"));
    }

    [Theory]
    [InlineData("muontra", "/quan-tri/phieu-muon/lap", "01.01.03 Lập Phiếu mượn sách", "02.03.01 Lập Biên bản vi phạm mượn trả sách")]
    [InlineData("vipham", "/quan-tri/bien-ban/lap", "02.03.01 Lập Biên bản vi phạm mượn trả sách", "01.01.03 Lập Phiếu mượn sách")]
    [InlineData("khosach", "/quan-tri/danh-muc/sach", "03.01.04 Cập nhật tình trạng sách đang được mượn", "04.00.01 Quản lý tài khoản người dùng")]
    [InlineData("admin", "/quan-tri/tai-khoan", "04.00.01 Quản lý tài khoản người dùng", "01.01.03 Lập Phiếu mượn sách")]
    public async Task DangNhap_ManHinhMacDinh_MenuChiCuaNhom(string ten, string manHinh, string coMuc, string khongCoMuc)
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(page, ten);
        Assert.EndsWith(manHinh, new Uri(page.Url).PathAndQuery);
        var menu = await page.InnerTextAsync(".qt-nav");
        Assert.Contains(coMuc, menu);
        Assert.DoesNotContain(khongCoMuc, menu);
        // Mục đang chọn đánh dấu bằng aria-current, không dùng chữ “ACTIVE” (nguyên tắc 9)
        Assert.Equal(1, await page.Locator(".qt-nav [aria-current='page']").CountAsync());
        Assert.DoesNotContain("ACTIVE", await page.InnerTextAsync("body"));
    }

    [Fact]
    public async Task ChucNangNgoaiQuyen_ChuyenTrangKhongCoQuyen()
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(page, "muontra");
        var res = await page.GotoAsync(TrinhDuyet.Web + "/quan-tri/tai-khoan");
        Assert.Contains("/quan-tri/khong-co-quyen", page.Url);
        Assert.Equal(403, res!.Status);
    }

    [Fact]
    public async Task Thoat_HoiXacNhan_RoiVeTrangChu()
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri/dang-nhap");
        await page.ClickAsync("#btnThoat");
        await page.WaitForSelectorAsync("#modalThoat.show");
        await page.ClickAsync("#btnXacNhanThoat");
        await page.WaitForURLAsync(TrinhDuyet.Web + "/");
    }

    [Fact]
    public async Task DangXuat_VeManDangNhap()
    {
        await using var ctx = await td.MayTinhAsync();
        var page = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(page, "khosach");
        await page.ClickAsync(".qt-nguoi-dung");
        await page.ClickAsync("#btnDangXuat");
        await page.WaitForURLAsync(u => u.Contains("/quan-tri/dang-nhap"));
        Assert.Contains("Bạn đã đăng xuất", await page.InnerTextAsync("body"));
        await page.GotoAsync(TrinhDuyet.Web + "/quan-tri");
        Assert.Contains("/quan-tri/dang-nhap", page.Url);
    }
}
