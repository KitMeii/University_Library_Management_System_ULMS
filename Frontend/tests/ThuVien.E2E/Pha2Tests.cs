namespace ThuVien.E2E;

/// <summary>Pha 2: SPEC 6.1.1–6.1.3, 6.3.2–6.3.5 – trạng thái nút, luồng chính, quyền.</summary>
[Collection("TrinhDuyet")]
public class Pha2Tests(TrinhDuyet td)
{
    private static async Task<bool> Bat(IPage p, string id) => await p.IsEnabledAsync("#" + id);
    private static async Task<bool> Co(IPage p, string id) => await p.Locator("#" + id).CountAsync() > 0;

    private async Task<IPage> MoAsync(IBrowserContext ctx, string ten, string duongDan, string? luoi = null)
    {
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, ten);
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        if (luoi is not null)
        {
            await p.WaitForSelectorAsync($"#{luoi} tbody tr");
            await p.WaitForFunctionAsync($"() => !document.querySelector('#{luoi}').hasAttribute('aria-busy')");
        }
        return p;
    }

    // ---------- 6.1.1 Danh mục sách ----------
    [Fact]
    public async Task Sach_N3_ThongKe_Loc_ChuyenDen()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "khosach", "/quan-tri/danh-muc/sach", "dgvSach");
        // N3: C, E, R – không có quyền D → btnXoa ẩn
        Assert.True(await Co(p, "btnThem"));
        Assert.False(await Co(p, "btnXoa"));
        // 03.04.01: thống kê theo tình trạng
        await p.WaitForSelectorAsync("#thongKeTinhTrang >> text=Tổng số");
        // cboLocTinhTrang
        await p.SelectOptionAsync("#cboLocTinhTrang", "Mất");
        await p.WaitForFunctionAsync("() => !document.querySelector('#dgvSach').hasAttribute('aria-busy')");
        var tinhTrang = await p.Locator("#dgvSach tbody tr[data-chi-so] td:nth-child(5)").AllInnerTextsAsync();
        Assert.All(tinhTrang, t => Assert.Equal("Mất", t.Trim()));
        // dgvChuyenDen hiện với N3; btnCapNhatTT chỉ bật khi chọn 1 dòng chuyển đến
        Assert.True(await Co(p, "dgvChuyenDen"));
        Assert.False(await Bat(p, "btnCapNhatTT"));
    }

    [Fact]
    public async Task Sach_N1_ChiXem_KhongCoChuyenDen()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/danh-muc/sach", "dgvSach");
        foreach (var n in new[] { "btnThem", "btnSua", "btnXoa", "btnLuu" }) Assert.False(await Co(p, n), n);
        Assert.False(await Co(p, "dgvChuyenDen"));
    }

    [Fact]
    public async Task Sach_ThemSuaXoa_N4()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/danh-muc/sach", "dgvSach");
        var ma = "T" + Random.Shared.Next(100000, 999999);
        await p.ClickAsync("#btnThem");
        Assert.Equal("Sẵn sàng", await p.InputValueAsync("#cboTinhTrang")); // tinhtrang mặc định “Sẵn sàng”
        await p.FillAsync("#txtMaBanSach", ma);
        await p.FillAsync("#txtTenSach", "Sách kiểm thử E2E");
        await p.FillAsync("#txtTacGia", "Tác giả E2E");
        await p.ClickAsync("#btnLuu");
        await p.WaitForSelectorAsync(".qt-toast--ok");
        await p.FillAsync("#txtTimKiem", ma);
        await p.ClickAsync("#btnTimKiem");
        await p.ClickAsync($"#dgvSach tbody tr[data-chi-so]:has-text('{ma}')");
        await p.ClickAsync("#btnXoa");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync(".qt-toast--ok >> text=Đã xóa");
    }

    // ---------- 6.1.3 Danh mục Bạn đọc ----------
    [Fact]
    public async Task BanDoc_TimThay_KiemTraDieuKien()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/ban-doc", "dgvBanDoc");
        Assert.False(await Bat(p, "btnKiemTraDK")); // (Load): btnKiemTraDK → False
        await p.FillAsync("#txtMaSV", "SV22000001");
        await p.ClickAsync("#btnTimKiem");
        await p.WaitForFunctionAsync("() => document.querySelector('#txtHoTen').value !== ''");
        Assert.Equal("Nguyễn Minh Anh", await p.InputValueAsync("#txtHoTen"));
        foreach (var n in new[] { "btnKiemTraDK", "btnSua", "btnXoa" }) Assert.True(await Bat(p, n), n);
        await p.ClickAsync("#btnKiemTraDK");
        await p.WaitForSelectorAsync("#khuDieuKien >> text=Đủ điều kiện mượn");
    }

    [Fact]
    public async Task BanDoc_TheHetHan_KhongDuDieuKien()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/ban-doc", "dgvBanDoc");
        await p.FillAsync("#txtMaSV", "SV22000004"); // thẻ hết hạn trong dữ liệu mẫu
        await p.ClickAsync("#btnTimKiem");
        await p.WaitForFunctionAsync("() => document.querySelector('#txtHoTen').value !== ''");
        await p.ClickAsync("#btnKiemTraDK");
        await p.WaitForSelectorAsync("#khuDieuKien >> text=Không đủ điều kiện mượn");
        Assert.Contains("hết hạn", await p.InnerTextAsync("#khuDieuKien"));
    }

    [Fact]
    public async Task BanDoc_KhongThay_MoCheDoThem_LuuTaoTaiKhoan()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/ban-doc", "dgvBanDoc");
        var ma = "E2E" + Random.Shared.Next(100000, 999999);
        await p.FillAsync("#txtMaSV", ma);
        await p.ClickAsync("#btnTimKiem");
        // Không thấy → mở chế độ thêm: btnLuu, btnHuy → True; btnKiemTraDK, btnSua, btnXoa → False
        await p.WaitForFunctionAsync("() => !document.querySelector('#btnLuu').disabled");
        Assert.Equal(ma, await p.InputValueAsync("#txtMaSvHvCb"));
        foreach (var n in new[] { "btnKiemTraDK", "btnSua", "btnXoa" }) Assert.False(await Bat(p, n), n);
        await p.FillAsync("#txtHoTen", "Bạn đọc kiểm thử");
        await p.FillAsync("#txtLopDonVi", "K26-E2E");
        await p.FillAsync("#dtpNgaySinh", "2004-05-06");
        await p.FillAsync("#txtSoTheBD", "T" + ma);
        await p.ClickAsync("#btnLuu");
        await p.WaitForSelectorAsync(".qt-toast--ok >> text=chờ kích hoạt");
        // Dọn dữ liệu kiểm thử
        await p.FillAsync("#txtMaSV", ma);
        await p.ClickAsync("#btnTimKiem");
        await p.WaitForFunctionAsync("() => !document.querySelector('#btnXoa').disabled");
        await p.ClickAsync("#btnXoa");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync(".qt-toast--ok >> text=Đã xóa");
    }

    /// <summary>GIA_DINH D35: ảnh thẻ tải lên được chuyển sang WebP và thu nhỏ vừa khung 600×800; tệp không phải ảnh bị từ chối.</summary>
    [Fact]
    public async Task BanDoc_AnhThe_ChuyenWebP_ThuNho()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/ban-doc", "dgvBanDoc");
        await p.FillAsync("#txtMaSV", "E2EANH" + Random.Shared.Next(1000, 9999));
        await p.ClickAsync("#btnTimKiem");
        await p.WaitForFunctionAsync("() => !document.querySelector('#fileAnh').disabled");

        // Ảnh PNG 1200×1600 vẽ bằng canvas
        var base64 = await p.EvaluateAsync<string>("""
            () => { const c = document.createElement('canvas'); c.width = 1200; c.height = 1600;
              const g = c.getContext('2d'); g.fillStyle = '#c8102e'; g.fillRect(0, 0, 1200, 1600);
              g.fillStyle = '#fff'; g.fillRect(300, 400, 600, 800);
              return c.toDataURL('image/png').split(',')[1]; }
            """);
        await p.SetInputFilesAsync("#fileAnh", new FilePayload { Name = "the.png", MimeType = "image/png", Buffer = Convert.FromBase64String(base64) });
        await p.WaitForFunctionAsync("() => /\\.webp$/.test(document.querySelector('#txtAnh').value)");
        await p.WaitForFunctionAsync("() => document.querySelector('#imgAnh').complete && document.querySelector('#imgAnh').naturalWidth > 0");
        var kichThuoc = await p.EvaluateAsync<int[]>("() => [document.querySelector('#imgAnh').naturalWidth, document.querySelector('#imgAnh').naturalHeight]");
        Assert.Equal(600, kichThuoc[0]);
        Assert.Equal(800, kichThuoc[1]);

        // Tệp đổi đuôi .png nhưng không phải ảnh → từ chối
        await p.SetInputFilesAsync("#fileAnh", new FilePayload { Name = "gia.png", MimeType = "image/png", Buffer = "không phải ảnh"u8.ToArray() });
        await p.WaitForSelectorAsync(".qt-toast >> text=Chỉ nhận ảnh JPG, PNG hoặc WebP");
    }

    [Fact]
    public async Task BanDoc_N2_ChiXem()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "vipham", "/quan-tri/danh-muc/ban-doc", "dgvBanDoc");
        foreach (var n in new[] { "btnThem", "btnSua", "btnXoa", "btnLuu" }) Assert.False(await Co(p, n), n);
    }

    // ---------- 6.3.2 Quản lý tài khoản ----------
    [Fact]
    public async Task TaiKhoan_Them_Khoa_MoKhoa_DatLaiMK()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/tai-khoan", "dgvNguoiDung");
        foreach (var n in new[] { "btnSua", "btnKhoa", "btnDatLaiMK", "btnLuu", "btnHuy" }) Assert.False(await Bat(p, n), n);
        Assert.False(await Co(p, "btnXoa")); // không có chức năng xóa
        var tdn = "e2e" + Random.Shared.Next(10000, 99999);
        await p.ClickAsync("#btnThem");
        await p.WaitForFunctionAsync("() => document.querySelector('#txtMaND').value.startsWith('ND')"); // tự sinh ma_nd
        Assert.True(await p.Locator("#txtMaND").GetAttributeAsync("readonly") is not null);
        await p.FillAsync("#txtTenND", "Nhân viên E2E");
        await p.FillAsync("#txtTenDangNhap", tdn);
        await p.SelectOptionAsync("#cboNhom", "N3");
        await p.ClickAsync("#btnLuu");
        await p.WaitForSelectorAsync(".qt-toast--ok");
        await p.FillAsync("#txtTimKiem", tdn);
        await p.ClickAsync("#btnTimKiem");
        await p.ClickAsync($"#dgvNguoiDung tbody tr[data-chi-so]:has-text('{tdn}')");
        Assert.Equal("Khóa", (await p.InnerTextAsync("#btnKhoa")).Trim());
        await p.ClickAsync("#btnKhoa");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync(".qt-toast--ok >> text=Đã khóa");
        await p.ClickAsync($"#dgvNguoiDung tbody tr[data-chi-so]:has-text('{tdn}')");
        Assert.Equal("Mở khóa", (await p.InnerTextAsync("#btnKhoa")).Trim());
        await p.ClickAsync("#btnDatLaiMK");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync(".qt-toast--ok >> text=Đã đặt lại mật khẩu");
    }

    [Fact]
    public async Task TaiKhoan_KhongTuKhoaChinhMinh()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/tai-khoan", "dgvNguoiDung");
        await p.ClickAsync("#dgvNguoiDung tbody tr[data-chi-so]:has-text('admin')");
        await p.ClickAsync("#btnKhoa");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync(".qt-toast--loi >> text=chính tài khoản");
    }

    // ---------- 6.3.3 Cấp quyền ----------
    [Fact]
    public async Task PhanQuyen_TrangThaiNut_QuyenQuanTriN4KhongBo()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/phan-quyen", "dgvQuyen");
        Assert.False(await Bat(p, "btnLuu"));
        Assert.False(await Bat(p, "btnHuy"));
        await p.SelectOptionAsync("#cboNhom", "N4");
        await p.WaitForSelectorAsync("#tomTatQuyen >> text=Nhóm N4");
        await p.FillAsync("#txtLocQuyen", "QT040001");
        Assert.True(await p.IsDisabledAsync("#dgvQuyen tbody tr[data-chi-so] input[type=checkbox]"));
        await p.FillAsync("#txtLocQuyen", "");
        await p.SelectOptionAsync("#cboNhom", "N2");
        await p.WaitForSelectorAsync("#tomTatQuyen >> text=Nhóm N2");
        await p.FillAsync("#txtLocQuyen", "TT001");
        await p.ClickAsync("#dgvQuyen tbody tr[data-chi-so] input[type=checkbox]");
        // Đánh dấu → btnLuu, btnHuy True; cboNhom False
        Assert.True(await Bat(p, "btnLuu"));
        Assert.True(await Bat(p, "btnHuy"));
        Assert.True(await p.IsDisabledAsync("#cboNhom"));
        await p.ClickAsync("#btnHuy"); // nạp lại, không thay đổi PHANQUYEN
        await p.WaitForFunctionAsync("() => document.querySelector('#btnLuu').disabled");
        Assert.True(await p.IsEnabledAsync("#cboNhom"));
    }

    // ---------- 6.3.4 Nhật ký ----------
    [Fact]
    public async Task NhatKy_HaiThe_LocKhoangNgay()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/nhat-ky/dang-nhap", "dgvDangNhap");
        Assert.Equal("true", await p.GetAttributeAsync("#tabDangNhap", "aria-selected"));
        await p.WaitForFunctionAsync("() => document.querySelector('#soLuot').textContent.trim() !== '0'");
        await p.FillAsync("#dtpTuNgay", "2030-01-02");
        await p.FillAsync("#dtpDenNgay", "2030-01-01");
        await p.ClickAsync("#btnLoc");
        await p.WaitForSelectorAsync(".qt-toast--loi >> text=Từ ngày phải nhỏ hơn");
        await p.ClickAsync("#tabHoatDong");
        await p.WaitForSelectorAsync("#dgvHoatDong tbody tr");
        Assert.Contains("/quan-tri/nhat-ky/hoat-dong", p.Url);
    }

    // ---------- 6.3.5 Sao lưu ----------
    [Fact]
    public async Task SaoLuu_ChonThuMuc_ThucHien_100PhanTram()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/sao-luu");
        Assert.True(await p.IsCheckedAsync("#rdoSaoLuu"));
        Assert.True(await p.Locator("#txtDuongDan").GetAttributeAsync("readonly") is not null);
        Assert.False(await Bat(p, "btnThucHien"));
        await p.ClickAsync("#btnChon");
        await p.WaitForSelectorAsync("#modalChon.show");
        await p.ClickAsync("#btnChonXong");
        Assert.True(await Bat(p, "btnThucHien"));
        await p.ClickAsync("#btnThucHien");
        await p.WaitForSelectorAsync("#phanTram >> text=100%", new() { Timeout = 120_000 });
        await p.WaitForSelectorAsync(".qt-toast--ok >> text=Sao lưu thành công");
        Assert.False(await Bat(p, "btnThucHien"));
        // Đổi sang phục hồi: xóa đường dẫn, btnThucHien False
        await p.CheckAsync("#rdoPhucHoi");
        Assert.Equal("", await p.InputValueAsync("#txtDuongDan"));
        Assert.Contains("tệp", await p.InnerTextAsync("#btnChon"));
    }

    // ---------- Điện thoại: không cuộn ngang trang (lưới tự cuộn trong khung – file 11) ----------
    [Theory]
    [InlineData("khosach", "/quan-tri/danh-muc/sach")]
    [InlineData("muontra", "/quan-tri/ban-doc")]
    [InlineData("admin", "/quan-tri/danh-muc/khoan-thu")]
    [InlineData("admin", "/quan-tri/danh-muc/khu-vuc")]
    [InlineData("admin", "/quan-tri/tai-khoan")]
    [InlineData("admin", "/quan-tri/phan-quyen")]
    [InlineData("admin", "/quan-tri/nhat-ky/hoat-dong")]
    [InlineData("admin", "/quan-tri/sao-luu")]
    public async Task DienThoai_KhongTranNgang(string ten, string duongDan)
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, ten);
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        await p.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var tran = await p.EvaluateAsync<int>("() => document.documentElement.scrollWidth - document.documentElement.clientWidth");
        Assert.True(tran <= 1, $"Trang {duongDan} tràn ngang {tran}px trên điện thoại");
    }
}
