using Deque.AxeCore.Playwright;

namespace ThuVien.E2E;

/// <summary>
/// Pha 8 – nghiệm thu file 11: axe cho MỌI trang (theo đúng nhóm có quyền), điện thoại Pixel 7 + iPhone 13 không tràn ngang trên mọi trang
/// dashboard và cổng, chỉ số trang chủ LCP &lt; 2,5 s và CLS &lt; 0,05 (file 11 mục 3).
/// </summary>
[Collection("TrinhDuyet")]
public class Pha8Tests(TrinhDuyet td)
{
    /// <summary>Mọi giao diện dashboard theo nhóm có quyền (SPEC 3.3) – dùng chung cho axe, điện thoại và ảnh.</summary>
    public static readonly (string NhanVien, string DuongDan, string TenAnh)[] TrangNghiepVu =
    [
        ("khosach", "/quan-tri/danh-muc/sach", "4.2.1_Danh_muc_sach"),
        ("vipham", "/quan-tri/danh-muc/khoan-thu", "4.2.2_Danh_muc_khoan_thu"),
        ("muontra", "/quan-tri/danh-muc/ban-doc", "4.2.3_Danh_muc_ban_doc"),
        ("khosach", "/quan-tri/danh-muc/khu-vuc", "4.2.4_Danh_muc_khu_vuc"),
        ("muontra", "/quan-tri/phieu-muon/lap", "4.3.1_Lap_phieu_muon"),
        ("muontra", "/quan-tri/phieu-gia-han/lap", "4.3.2_Lap_phieu_gia_han"),
        ("muontra", "/quan-tri/phieu-tra/lap", "4.3.3_Lap_phieu_tra"),
        ("vipham", "/quan-tri/bien-ban/lap", "4.3.4_Lap_bien_ban_vi_pham"),
        ("muontra", "/quan-tri/bao-cao/muon-tra", "4.5.1_Tong_hop_muon_tra"),
        ("vipham", "/quan-tri/bao-cao/vi-pham", "4.5.2_Tong_hop_vi_pham"),
        ("admin", "/quan-tri/tai-khoan", "4.6.2_Quan_ly_tai_khoan"),
        ("admin", "/quan-tri/phan-quyen", "4.6.3_Cap_quyen"),
        ("admin", "/quan-tri/nhat-ky/hoat-dong", "4.6.4_Nhat_ky_Hoat_dong"),
        ("admin", "/quan-tri/nhat-ky/dang-nhap", "4.6.4_Nhat_ky_Lich_su_dang_nhap"),
        ("admin", "/quan-tri/sao-luu", "4.6.5_Sao_luu_phuc_hoi"),
        ("muontra", "/quan-tri", "Tong_quan_N1"),
        ("vipham", "/quan-tri", "Tong_quan_N2"),
        ("khosach", "/quan-tri", "Tong_quan_N3"),
        ("admin", "/quan-tri", "Tong_quan_N4"),
        ("khosach", "/quan-tri/danh-muc/dau-sach", "Pha5_Quan_ly_dau_sach"),
        ("khosach", "/quan-tri/danh-muc/the-loai", "Pha5_Quan_ly_the_loai"),
        ("muontra", "/quan-tri/dat-cho", "Pha6_Quay_dat_cho"),
    ];

    /// <summary>Trang cổng bạn đọc: (cần đăng nhập bạn đọc, đường dẫn, tên ảnh).</summary>
    public static readonly (bool BanDoc, string DuongDan, string TenAnh)[] TrangCong =
    [
        (false, "/", "Cong_Trang_chu_hoan_chinh"),
        (false, "/kham-pha", "Cong_Kham_pha_toan_trang"),
        (false, "/huong-dan", "Cong_Huong_dan"),
        (false, "/dang-nhap", "Cong_Dang_nhap"),
        (false, "/kich-hoat", "Cong_Kich_hoat"),
        (true, "/tai-khoan", "Cong_Tai_khoan_Tong_quan"),
        (true, "/tai-khoan/dang-muon", "Cong_Tai_khoan_Dang_muon"),
        (true, "/tai-khoan/dat-cho", "Cong_Tai_khoan_Dat_cho"),
        (true, "/tai-khoan/lich-su", "Cong_Tai_khoan_Lich_su"),
        (true, "/tai-khoan/vi-pham", "Cong_Tai_khoan_Vi_pham"),
        (true, "/tai-khoan/yeu-thich", "Cong_Tai_khoan_Yeu_thich"),
        (true, "/tai-khoan/thong-bao", "Cong_Tai_khoan_Thong_bao"),
        (true, "/tai-khoan/doi-mat-khau", "Cong_Tai_khoan_Doi_mat_khau"),
    ];

    private const string BanDocMau = "SV22000001";

    internal static async Task MoTrangAsync(IPage p, string duongDan)
    {
        var res = await p.GotoAsync(TrinhDuyet.Web + duongDan);
        Assert.True(res is { Ok: true }, $"{duongDan}: HTTP {res?.Status}");
        var duongThat = new Uri(p.Url).AbsolutePath;
        Assert.DoesNotContain("/khong-co-quyen", duongThat);
        // Không bị chuyển về trang đăng nhập (trừ khi chính trang cần mở là trang đăng nhập)
        if (!duongDan.Contains("dang-nhap")) Assert.DoesNotContain("dang-nhap", duongThat);
        await p.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task<List<string>> AxeLoiAsync(IPage p, string ten)
    {
        var kq = await p.RunAxe();
        return kq.Violations.Where(v => v.Impact is "critical" or "serious")
            .Select(v => $"{ten}: {v.Id} – " + string.Join("; ", v.Nodes.Take(3).Select(n => n.Target))).ToList();
    }

    [Fact]
    public async Task Axe_MoiTrangNghiepVu_DungNhomQuyen_KhongLoiNghiemTrong()
    {
        var loi = new List<string>();
        foreach (var nhom in TrangNghiepVu.GroupBy(t => t.NhanVien))
        {
            await using var ctx = await td.MayTinhAsync();
            await TrinhDuyet.BatMoRongAsync(ctx);
            var p = await ctx.NewPageAsync();
            await TrinhDuyet.DangNhapNhanVienAsync(p, nhom.Key);
            foreach (var (_, duong, ten) in nhom)
            {
                await MoTrangAsync(p, duong);
                await p.WaitForTimeoutAsync(600);
                loi.AddRange(await AxeLoiAsync(p, ten));
            }
        }
        Assert.True(loi.Count == 0, string.Join("\n", loi));
    }

    [Fact]
    public async Task Axe_MoiTrangCong_KhongLoiNghiemTrong()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        var loi = new List<string>();
        var daDangNhap = false;
        foreach (var (banDoc, duong, ten) in TrangCong)
        {
            if (banDoc && !daDangNhap) { await TrinhDuyet.DangNhapBanDocAsync(p, BanDocMau); daDangNhap = true; }
            await MoTrangAsync(p, duong);
            await p.WaitForTimeoutAsync(900);
            loi.AddRange(await AxeLoiAsync(p, ten));
        }
        Assert.True(loi.Count == 0, string.Join("\n", loi));
    }

    [Theory]
    [InlineData("Pixel 7")]
    [InlineData("iPhone 13")]
    public async Task DienThoai_MoiTrang_KhongTranNgang_SidebarOffcanvas(string thietBi)
    {
        var tran = new List<string>();
        const string doTran = "() => document.documentElement.scrollWidth - window.innerWidth";
        foreach (var nhom in TrangNghiepVu.GroupBy(t => t.NhanVien))
        {
            await using var ctx = await td.DienThoaiAsync(thietBi);
            await TrinhDuyet.BatMoRongAsync(ctx);
            var p = await ctx.NewPageAsync();
            await TrinhDuyet.DangNhapNhanVienAsync(p, nhom.Key);
            foreach (var (_, duong, ten) in nhom)
            {
                await MoTrangAsync(p, duong);
                if (await p.EvaluateAsync<int>(doTran) > 1) tran.Add(ten);
            }
            // Sidebar thành Offcanvas: ẩn mặc định, mở bằng nút menu
            Assert.False(await p.IsVisibleAsync("#qtSidebar"));
            await p.ClickAsync("[aria-controls='qtSidebar']");
            await p.WaitForSelectorAsync("#qtSidebar.show");
        }
        await using (var ctx = await td.DienThoaiAsync(thietBi))
        {
            var p = await ctx.NewPageAsync();
            var daDangNhap = false;
            foreach (var (banDoc, duong, ten) in TrangCong)
            {
                if (banDoc && !daDangNhap) { await TrinhDuyet.DangNhapBanDocAsync(p, BanDocMau); daDangNhap = true; }
                await MoTrangAsync(p, duong);
                if (await p.EvaluateAsync<int>(doTran) > 1) tran.Add(ten);
            }
        }
        Assert.True(tran.Count == 0, $"{thietBi} – tràn ngang: " + string.Join(", ", tran));
    }

    [Fact]
    public async Task DienThoaiPixel7_CongBanDoc_KeVuot_BookPreviewMotTrang_TabDay()
    {
        await using var ctx = await td.DienThoaiAsync("Pixel 7");
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#ke-sach .book", new() { State = WaitForSelectorState.Attached });
        // Kệ trên điện thoại: chip đổi kệ
        Assert.True(await p.Locator(".coverflow__chip").First.IsVisibleAsync());
        // Book Preview 1 trang trên điện thoại (liên kết sâu)
        await p.GotoAsync(TrinhDuyet.Web + "/sach/DS0001");
        await p.WaitForSelectorAsync(".bp .stf__parent");
        await p.WaitForTimeoutAsync(1500);
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));
        // Tài khoản: thanh tab đáy
        await TrinhDuyet.DangNhapBanDocAsync(p, BanDocMau);
        await p.GotoAsync(TrinhDuyet.Web + "/tai-khoan");
        Assert.Equal("fixed", await p.EvaluateAsync<string>("() => getComputedStyle(document.querySelector('.tk-dieu-huong')).position"));
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));
    }

    [Fact]
    public async Task TrangChu_Lcp_Duoi2Giay5_Cls_Duoi0_05()
    {
        await using var ctx = await td.MayTinhAsync();
        await ctx.AddInitScriptAsync("""
            (() => {
              window.__lcp = 0; window.__cls = 0;
              new PerformanceObserver((l) => { for (const e of l.getEntries()) window.__lcp = e.startTime; })
                .observe({ type: 'largest-contentful-paint', buffered: true });
              new PerformanceObserver((l) => { for (const e of l.getEntries()) if (!e.hadRecentInput) window.__cls += e.value; })
                .observe({ type: 'layout-shift', buffered: true });
            })();
            """);
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/");
        await p.WaitForSelectorAsync("#kpLuoi .the-sach");
        await p.WaitForTimeoutAsync(2500);
        // LCP chốt khi người dùng tương tác – đọc trước khi cuộn; CLS đo cả khi cuộn hết trang
        var lcp = await p.EvaluateAsync<double>("() => window.__lcp");
        await p.EvaluateAsync("""
            async () => { for (let y = 0; y < document.body.scrollHeight; y += 600) { window.scrollTo(0, y); await new Promise((r) => setTimeout(r, 150)); } }
            """);
        await p.WaitForTimeoutAsync(1000);
        var cls = await p.EvaluateAsync<double>("() => window.__cls");
        Assert.True(lcp > 0 && lcp < 2500, $"LCP = {lcp:0} ms (ngưỡng 2500)");
        Assert.True(cls < 0.05, $"CLS = {cls:0.000} (ngưỡng 0,05)");
    }
}
