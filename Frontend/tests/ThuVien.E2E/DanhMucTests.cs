namespace ThuVien.E2E;

/// <summary>Giao diện danh mục SPEC 6.1.x: trạng thái nút từng bước, quyền (Visible = False), luồng thêm/sửa/xóa.</summary>
[Collection("TrinhDuyet")]
public class DanhMucTests(TrinhDuyet td)
{
    private static async Task<bool> BatAsync(IPage p, string id) => await p.IsEnabledAsync("#" + id);
    private static async Task<bool> HienAsync(IPage p, string id) => await p.Locator("#" + id).CountAsync() > 0;

    private static async Task ChoLuoiAsync(IPage p, string luoi)
    {
        await p.WaitForSelectorAsync($"#{luoi} tbody tr[data-chi-so]");
        await p.WaitForFunctionAsync($"() => !document.querySelector('#{luoi}').hasAttribute('aria-busy')");
    }

    /// <summary>Trạng thái nút chuẩn của 6.1.x (Load → chọn dòng → Thêm → Hủy).</summary>
    private static async Task KiemTraTrangThaiChuanAsync(IPage p, string luoi, string oMa, string oKhac)
    {
        // (Load): ô ReadOnly; btnThem, btnTimKiem True; btnSua, btnXoa, btnLuu, btnHuy False
        Assert.True(await p.Locator("#" + oMa).GetAttributeAsync("readonly") is not null);
        Assert.True(await BatAsync(p, "btnThem"));
        Assert.True(await BatAsync(p, "btnTimKiem"));
        foreach (var n in new[] { "btnSua", "btnXoa", "btnLuu", "btnHuy" }) Assert.False(await BatAsync(p, n), n);

        // Click dòng: đổ lên ô; btnSua, btnXoa True
        await p.ClickAsync($"#{luoi} tbody tr[data-chi-so='0']");
        Assert.NotEqual("", await p.InputValueAsync("#" + oMa));
        Assert.True(await BatAsync(p, "btnSua"));
        Assert.True(await BatAsync(p, "btnXoa"));

        // btnThem: xóa trắng, mở ô; btnLuu, btnHuy True; btnThem, btnSua, btnXoa, btnTimKiem False
        await p.ClickAsync("#btnThem");
        Assert.Equal("", await p.InputValueAsync("#" + oMa));
        Assert.True(await p.Locator("#" + oKhac).GetAttributeAsync("readonly") is null);
        Assert.True(await BatAsync(p, "btnLuu"));
        Assert.True(await BatAsync(p, "btnHuy"));
        foreach (var n in new[] { "btnThem", "btnSua", "btnXoa", "btnTimKiem" }) Assert.False(await BatAsync(p, n), n);

        // btnHuy: về trạng thái khi mở form
        await p.ClickAsync("#btnHuy");
        Assert.True(await BatAsync(p, "btnThem"));
        Assert.False(await BatAsync(p, "btnLuu"));
    }

    [Fact]
    public async Task KhuVuc_TrangThaiNut_ThemSuaXoa()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "admin");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/danh-muc/khu-vuc");
        await ChoLuoiAsync(p, "dgvKhuVuc");
        await KiemTraTrangThaiChuanAsync(p, "dgvKhuVuc", "txtMaKhuVuc", "txtViTri");

        var ma = "KVT" + Random.Shared.Next(1000, 9999);
        // Thêm: kiểm soát dữ liệu nhập (vị trí rỗng → lỗi dưới ô)
        await p.ClickAsync("#btnThem");
        await p.FillAsync("#txtMaKhuVuc", ma);
        await p.ClickAsync("#btnLuu");
        await p.WaitForSelectorAsync("#txtViTriLoi");
        Assert.Equal("true", await p.GetAttributeAsync("#txtViTri", "aria-invalid"));

        await p.FillAsync("#txtViTri", "Phòng thử nghiệm E2E");
        await p.SelectOptionAsync("#cboLoaiKhuVuc", "Kho");
        await p.ClickAsync("#btnLuu");
        await p.WaitForSelectorAsync(".qt-toast--ok");
        await p.FillAsync("#txtTimKiem", ma);
        await p.ClickAsync("#btnTimKiem");
        // Chỉ thao tác khi lưới đã lọc xong đúng 1 dòng của mã vừa thêm (tránh sửa nhầm dữ liệu khác)
        await p.WaitForFunctionAsync($"() => {{ const r = document.querySelectorAll('#dgvKhuVuc tbody tr[data-chi-so]'); return r.length === 1 && r[0].textContent.includes('{ma}'); }}");

        // Sửa: không sửa được mã
        await p.ClickAsync($"#dgvKhuVuc tbody tr[data-chi-so]:has-text('{ma}')");
        await p.ClickAsync("#btnSua");
        Assert.True(await p.Locator("#txtMaKhuVuc").GetAttributeAsync("readonly") is not null);
        await p.FillAsync("#txtViTri", "Phòng thử nghiệm E2E (đã sửa)");
        await p.ClickAsync("#btnLuu");
        await p.WaitForSelectorAsync("#dgvKhuVuc >> text=Phòng thử nghiệm E2E (đã sửa)");

        // Xóa: hỏi xác nhận (modal tự làm, không dùng confirm mặc định)
        await p.FillAsync("#txtTimKiem", ma);
        await p.ClickAsync("#btnTimKiem");
        await p.WaitForFunctionAsync($"() => {{ const r = document.querySelectorAll('#dgvKhuVuc tbody tr[data-chi-so]'); return r.length === 1 && r[0].textContent.includes('{ma}'); }}");
        await p.ClickAsync($"#dgvKhuVuc tbody tr[data-chi-so]:has-text('{ma}')");
        await p.ClickAsync("#btnXoa");
        await p.WaitForSelectorAsync(".modal.show");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync($".qt-toast--ok >> text=Đã xóa");
    }

    [Fact]
    public async Task KhuVuc_ConSach_KhongChoXoa()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "admin");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/danh-muc/khu-vuc");
        await ChoLuoiAsync(p, "dgvKhuVuc");
        await p.ClickAsync("#dgvKhuVuc tbody tr[data-chi-so]:has-text('KV01')");
        await p.ClickAsync("#btnXoa");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync(".qt-toast--loi >> text=còn");
    }

    [Fact]
    public async Task KhuVuc_N1ChiXem_AnNutGhi()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "muontra");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/danh-muc/khu-vuc");
        await ChoLuoiAsync(p, "dgvKhuVuc");
        foreach (var n in new[] { "btnThem", "btnSua", "btnXoa", "btnLuu" }) Assert.False(await HienAsync(p, n), n);
        Assert.True(await HienAsync(p, "btnDong"));
    }
}
