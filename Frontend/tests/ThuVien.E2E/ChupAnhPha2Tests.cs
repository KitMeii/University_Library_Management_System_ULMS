namespace ThuVien.E2E;

/// <summary>
/// Bộ ảnh Bài 4 – Pha 2: 6.1.k ↔ 4.2.k (danh mục), 6.3.k ↔ 4.6.k (quản trị). Chụp toàn trang 1366×768 + điện thoại.
/// </summary>
[Collection("TrinhDuyet")]
public class ChupAnhPha2Tests(TrinhDuyet td)
{
    private static async Task ChoLuoiAsync(IPage p, string luoi)
    {
        await p.WaitForSelectorAsync($"#{luoi} tbody tr");
        await p.WaitForFunctionAsync($"() => !document.querySelector('#{luoi}').hasAttribute('aria-busy')");
    }

    private async Task<IPage> MoAsync(IBrowserContext ctx, string ten, string duongDan, string? luoi)
    {
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, ten);
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        if (luoi is not null) await ChoLuoiAsync(p, luoi);
        return p;
    }

    [Fact]
    public async Task Chup_421_DanhMucSach()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "khosach", "/quan-tri/danh-muc/sach", "dgvSach");
        await p.ClickAsync("#dgvSach tbody tr[data-chi-so='1']");
        await TrinhDuyet.ChupAsync(p, "4.2.1_Danh_muc_sach", toanTrang: true);
        await using var dt = await td.DienThoaiAsync();
        var m = await MoAsync(dt, "khosach", "/quan-tri/danh-muc/sach", "dgvSach");
        await TrinhDuyet.ChupAsync(m, "4.2.1_Danh_muc_sach_mobile");
    }

    [Fact]
    public async Task Chup_422_DanhMucKhoanThu()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/danh-muc/khoan-thu", "dgvKhoanThu");
        await p.ClickAsync("#dgvKhoanThu tbody tr[data-chi-so='0']");
        await TrinhDuyet.ChupAsync(p, "4.2.2_Danh_muc_khoan_thu", toanTrang: true);
    }

    [Fact]
    public async Task Chup_423_DanhMucBanDoc()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/ban-doc", "dgvBanDoc");
        await TrinhDuyet.ChupAsync(p, "4.2.3_Danh_muc_ban_doc", toanTrang: true);
        await p.FillAsync("#txtMaSV", "SV22000001");
        await p.ClickAsync("#btnTimKiem");
        await p.WaitForFunctionAsync("() => document.querySelector('#txtHoTen').value !== ''");
        await p.ClickAsync("#btnKiemTraDK");
        await p.WaitForSelectorAsync("#khuDieuKien >> text=Đủ điều kiện mượn");
        await TrinhDuyet.ChupAsync(p, "4.2.3_Danh_muc_ban_doc_Kiem_tra_dieu_kien", toanTrang: true);
    }

    [Fact]
    public async Task Chup_424_DanhMucKhuVuc()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/danh-muc/khu-vuc", "dgvKhuVuc");
        await p.ClickAsync("#dgvKhuVuc tbody tr[data-chi-so='0']");
        await TrinhDuyet.ChupAsync(p, "4.2.4_Danh_muc_khu_vuc", toanTrang: true);
        await p.ClickAsync("#btnThem");
        await TrinhDuyet.ChupAsync(p, "4.2.4_Danh_muc_khu_vuc_Them", toanTrang: true);
        await p.ClickAsync("#btnHuy");
    }

    [Fact]
    public async Task Chup_462_QuanLyTaiKhoan()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/tai-khoan", "dgvNguoiDung");
        await p.ClickAsync("#dgvNguoiDung tbody tr[data-chi-so]:has-text('muontra')");
        await TrinhDuyet.ChupAsync(p, "4.6.2_Quan_ly_tai_khoan", toanTrang: true);
        await using var dt = await td.DienThoaiAsync();
        var m = await MoAsync(dt, "admin", "/quan-tri/tai-khoan", "dgvNguoiDung");
        await TrinhDuyet.ChupAsync(m, "4.6.2_Quan_ly_tai_khoan_mobile");
    }

    [Fact]
    public async Task Chup_463_CapQuyen()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/phan-quyen", "dgvQuyen");
        await p.SelectOptionAsync("#cboNhom", "N1");
        await p.WaitForSelectorAsync("#tomTatQuyen >> text=Nhóm N1");
        await TrinhDuyet.ChupAsync(p, "4.6.3_Cap_quyen");
    }

    [Fact]
    public async Task Chup_464_NhatKy()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/nhat-ky/dang-nhap", "dgvDangNhap");
        await TrinhDuyet.ChupAsync(p, "4.6.4_Nhat_ky_Lich_su_dang_nhap", toanTrang: true);
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/nhat-ky/hoat-dong");
        await ChoLuoiAsync(p, "dgvHoatDong");
        await TrinhDuyet.ChupAsync(p, "4.6.4_Nhat_ky_Hoat_dong", toanTrang: true);
    }

    [Fact]
    public async Task Chup_465_SaoLuu()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "admin", "/quan-tri/sao-luu", null);
        await p.ClickAsync("#btnChon");
        await p.WaitForSelectorAsync("#modalChon.show");
        await p.WaitForTimeoutAsync(500);
        await TrinhDuyet.ChupAsync(p, "4.6.5_Sao_luu_Chon_thu_muc");
        await p.ClickAsync("#btnChonXong");
        await p.ClickAsync("#btnThucHien");
        await p.WaitForSelectorAsync("#phanTram >> text=100%", new() { Timeout = 120_000 });
        await TrinhDuyet.ChupAsync(p, "4.6.5_Sao_luu_phuc_hoi");
    }
}
