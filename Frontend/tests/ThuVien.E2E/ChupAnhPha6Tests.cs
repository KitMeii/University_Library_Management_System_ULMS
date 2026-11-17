namespace ThuVien.E2E;

/// <summary>
/// Ảnh Pha 6 (không thuộc mục IV Bài 4 – phần mở rộng): Book Preview (mở sách, hộp đăng nhập, vé giữ chỗ, điện thoại), trang cá nhân
/// kính mờ, chuông thông báo, kích hoạt; phía nhân viên: Quầy đặt chỗ, dgvDatCho trong Lập Phiếu mượn, tổng quan N1 có đặt chỗ,
/// khóa tài khoản cổng. Ảnh nhân viên bật cookie kiểm thử mở rộng (bộ kiểm thử ở chế độ chụp ảnh Bài 4 – D61, D65).
/// </summary>
[Collection("TrinhDuyet")]
public class ChupAnhPha6Tests(TrinhDuyet td)
{
    private static string Duong(string ten) => Path.Combine(TrinhDuyet.ThuMucAnh, ten + ".png");

    [Fact]
    public async Task Chup_BookPreview_MayTinh()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#ke-sach.cf-3d .book[aria-current='true']");
        await p.EvaluateAsync("() => document.getElementById('ke-sach').scrollIntoView({ block: 'start' })");
        await p.WaitForTimeoutAsync(1500);

        // Đang mở: bìa bay ra rồi mở quanh gáy (khung hình giữa chừng)
        await p.ClickAsync("#ke-sach .book[aria-current='true'] .book__body");
        await p.WaitForTimeoutAsync(820);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Book_Preview_Dang_mo") });
        await p.WaitForSelectorAsync(".bp.bp--da-mo");
        await p.WaitForTimeoutAsync(500);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Book_Preview") });

        // Chưa đăng nhập → bấm Đặt chỗ → hộp đăng nhập ngay trong trang sách
        await p.ClickAsync("#btnDatCho");
        await p.WaitForSelectorAsync("#frmDangNhapNhanh");
        await p.WaitForTimeoutAsync(300);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Book_Preview_Dang_nhap") });
    }

    [Fact]
    public async Task Chup_BookPreview_VeGiuCho_KhongDuDieuKien()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(p, "SV22000003");
        var ma = await Pha6Tests.DauSachConBanAsync(p);
        await Pha6Tests.MoSachAsync(p, ma);
        await p.ClickAsync("#btnDatCho");
        await p.WaitForSelectorAsync("#veGiuCho");
        await p.WaitForTimeoutAsync(1400); // tem “ĐÃ GIỮ CHỖ” + tia
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Book_Preview_Ve_giu_cho") });
        p.Dialog += async (_, d) => await d.AcceptAsync();
        await p.ClickAsync("#btnHuyDatCho");
        await p.WaitForSelectorAsync("#btnDatCho");

        await using var ctx2 = await td.MayTinhAsync();
        var q = await ctx2.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(q, "SV22000008");
        await Pha6Tests.MoSachAsync(q, ma);
        await q.ClickAsync("#btnDatCho");
        await q.WaitForSelectorAsync("#bpLyDo");
        await q.WaitForTimeoutAsync(300);
        await q.ScreenshotAsync(new() { Path = Duong("Cong_Book_Preview_Khong_du_dieu_kien") });
    }

    [Fact]
    public async Task Chup_BookPreview_DienThoai()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        var ma = await Pha6Tests.DauSachConBanAsync(p);
        await Pha6Tests.MoSachAsync(p, ma);
        await p.WaitForTimeoutAsync(500);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Book_Preview_mobile") });
        await p.ClickAsync("#btnDatChoGhim");
        await p.WaitForSelectorAsync("#frmDangNhapNhanh");
        await p.WaitForTimeoutAsync(1400);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Book_Preview_mobile_trang_2") });
    }

    [Fact]
    public async Task Chup_TrangCaNhan_ThongBao_KichHoat()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(p, "SV22000007");
        foreach (var (trang, ten) in new[] { ("", "Cong_Tai_khoan_Tong_quan"), ("dat-cho", "Cong_Tai_khoan_Dat_cho"), ("thong-bao", "Cong_Tai_khoan_Thong_bao"),
                                             ("yeu-thich", "Cong_Tai_khoan_Yeu_thich"), ("lich-su", "Cong_Tai_khoan_Lich_su") })
        {
            await p.GotoAsync(TrinhDuyet.Web + "/tai-khoan" + (trang == "" ? "" : "/" + trang));
            await p.WaitForSelectorAsync("#tkNoiDung[aria-busy='false']");
            await p.WaitForTimeoutAsync(900);
            await TrinhDuyet.ChupAsync(p, ten, toanTrang: true);
        }
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.ClickAsync("#btnChuongCong");
        await p.WaitForSelectorAsync("#dsThongBaoCong .tb-dong");
        await p.WaitForTimeoutAsync(400);
        await p.ScreenshotAsync(new() { Path = Duong("Cong_Chuong_thong_bao") });

        await using var ctx2 = await td.MayTinhAsync();
        var q = await ctx2.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(q, "SV22000002");
        await q.GotoAsync(TrinhDuyet.Web + "/tai-khoan/dang-muon");
        await q.WaitForSelectorAsync("#tkNoiDung[aria-busy='false']");
        await q.WaitForTimeoutAsync(900);
        await TrinhDuyet.ChupAsync(q, "Cong_Tai_khoan_Dang_muon", toanTrang: true);

        await using var ctx3 = await td.MayTinhAsync();
        var k = await ctx3.NewPageAsync();
        await k.GotoAsync(TrinhDuyet.Web + "/kich-hoat");
        await TrinhDuyet.ChupAsync(k, "Cong_Kich_hoat");
    }

    [Fact]
    public async Task Chup_TrangCaNhan_DienThoai()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(p, "SV22000007");
        await p.GotoAsync(TrinhDuyet.Web + "/tai-khoan/dat-cho");
        await p.WaitForSelectorAsync("#tkNoiDung[aria-busy='false']");
        await p.WaitForTimeoutAsync(900);
        await TrinhDuyet.ChupAsync(p, "Cong_Tai_khoan_Dat_cho_mobile");
    }

    [Fact]
    public async Task Chup_NhanVien_QuayDatCho_LapPhieuMuon_TongQuan_TaiKhoanCong()
    {
        await using var ctx = await td.MayTinhAsync();
        await TrinhDuyet.BatMoRongAsync(ctx);
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, "muontra");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri");
        await p.WaitForSelectorAsync("#bdDatCho");
        await p.WaitForTimeoutAsync(1600);
        await TrinhDuyet.ChupAsync(p, "Pha6_Tong_quan_N1_dat_cho", toanTrang: true);

        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/dat-cho");
        await p.WaitForSelectorAsync("#dgvDatChoQuay tbody tr[data-chi-so]");
        await TrinhDuyet.ChupAsync(p, "Pha6_Quay_dat_cho", toanTrang: true);

        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/phieu-muon/lap");
        await p.FillAsync("#txtMaSV", "SV22000007");
        await p.ClickAsync("#btnTimBanDoc");
        await p.WaitForSelectorAsync("#khuDatCho:not([hidden]) #dgvDatCho tbody tr[data-chi-so]");
        await p.ClickAsync("#btnNhanDatCho");
        await p.WaitForSelectorAsync("#dgvChiTiet tbody tr[data-chi-so]");
        await TrinhDuyet.ChupAsync(p, "Pha6_Lap_phieu_muon_Nhan_dat_cho", toanTrang: true);
        await p.ClickAsync("#btnHuy");

        var inSoan = await ctx.NewPageAsync();
        await inSoan.GotoAsync(TrinhDuyet.Web + "/quan-tri/in/phieu-soan-sach");
        await TrinhDuyet.ChupAsync(inSoan, "Pha6_Phieu_soan_sach_A4", toanTrang: true);

        await using var ctx4 = await td.MayTinhAsync();
        await TrinhDuyet.BatMoRongAsync(ctx4);
        var a = await ctx4.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(a, "admin");
        await a.GotoAsync(TrinhDuyet.Web + "/quan-tri/danh-muc/ban-doc");
        await a.WaitForSelectorAsync("#dgvBanDoc tbody tr[data-chi-so]");
        await a.ClickAsync("#dgvBanDoc tbody tr[data-chi-so='0']");
        await a.WaitForSelectorAsync("#khuTaiKhoanCong:not([hidden])");
        await TrinhDuyet.ChupAsync(a, "Pha6_Danh_muc_ban_doc_Tai_khoan_cong", toanTrang: true);
    }
}
