namespace ThuVien.E2E;

/// <summary>
/// Bộ ảnh Bài 4 – Pha 3: 6.2.k ↔ 4.3.k (mỗi form TT1/TT2/TT3), 6.4.1–6.4.4 ↔ 4.4.1–4.4.4 (mẫu in).
/// Chạy trên CSDL sạch có dữ liệu mẫu (lịch sử chứng từ SPEC mục 7).
/// </summary>
[Collection("TrinhDuyet")]
public class ChupAnhPha3Tests(TrinhDuyet td)
{
    private static async Task<IPage> MoAsync(IBrowserContext ctx, string ten, string duongDan)
    {
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, ten);
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        await p.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return p;
    }

    /// <summary>Đánh dấu mọi dòng (mỗi lần đánh dấu lưới vẽ lại → tìm lại ô theo vị trí).</summary>
    private static async Task ChonTatCaAsync(IPage p)
    {
        var o = p.Locator("#dgvChiTiet tbody tr input[type=checkbox]");
        var n = await o.CountAsync();
        for (var i = 0; i < n; i++) await o.Nth(i).CheckAsync();
    }

    /// <summary>Phiếu mượn đang mượn (chưa trả) đầu tiên của bạn đọc mẫu.</summary>
    private static async Task<string> PhieuDangMuonAsync(IPage p, string maSv)
    {
        await p.EvaluateAsync(Pha3Tests.JsHoTro);
        var so = await p.EvaluateAsync<string?>("async (sv) => (await window.__goi('GET', '/api/quan-tri/ban-doc/tim?maSv=' + sv)).sachDangMuon[0]?.soPMuon ?? null", maSv);
        if (so is not null) return so;
        // CSDL không còn phiếu mẫu đang mượn (đã chạy kiểm thử trước đó) → lập phiếu mới cho bạn đọc kiểm thử
        var (bd, _) = await Pha3Tests.TaoBanDocAsync(p);
        return await Pha3Tests.LapPhieuMuonApiAsync(p, bd, await Pha3Tests.SachSanSangAsync(p, bd, 2));
    }

    private static async Task<string> InVaChupAsync(IBrowserContext ctx, IPage p, string anhIn)
    {
        await p.ClickAsync("#btnIn");
        await p.WaitForSelectorAsync("#khuInGanNhat:not([hidden])");
        var href = (await p.GetAttributeAsync("#khuInGanNhat a", "href"))!.Replace("?tuIn=1", "");
        var trang = await ctx.NewPageAsync();
        await trang.GotoAsync(TrinhDuyet.Web + href);
        await TrinhDuyet.ChupAsync(trang, anhIn, toanTrang: true);
        return href;
    }

    [Fact]
    public async Task Chup_431_LapPhieuMuon_441_MB02()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-muon/lap");
        await TrinhDuyet.ChupAsync(p, "4.3.1_Lap_phieu_muon_TT1", toanTrang: true);
        await p.FillAsync("#txtMaSV", "SV22000001");
        await p.ClickAsync("#btnTimBanDoc");
        await p.WaitForSelectorAsync("#khuDieuKien >> text=đủ điều kiện mượn");
        await TrinhDuyet.ChupAsync(p, "4.3.1_Lap_phieu_muon_TT2", toanTrang: true);
        var sach = await Pha3Tests.SachSanSangAsync(p, "BD0001", 2);
        foreach (var ma in sach)
        {
            await p.FillAsync("#txtMaBanSach", ma);
            await p.ClickAsync("#btnThemSach");
            await p.WaitForSelectorAsync($"#dgvChiTiet tbody tr:has-text('{ma}')");
        }
        await TrinhDuyet.ChupAsync(p, "4.3.1_Lap_phieu_muon_TT3", toanTrang: true);
        await InVaChupAsync(ctx, p, "4.4.1_Phieu_muon_sach_MB02");

        await using var dt = await td.DienThoaiAsync();
        var m = await MoAsync(dt, "muontra", "/quan-tri/phieu-muon/lap");
        await TrinhDuyet.ChupAsync(m, "4.3.1_Lap_phieu_muon_mobile");
    }

    [Fact]
    public async Task Chup_432_LapPhieuGiaHan_442_MB03()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-gia-han/lap");
        await TrinhDuyet.ChupAsync(p, "4.3.2_Lap_phieu_gia_han_TT1", toanTrang: true);
        var soPm = await PhieuDangMuonAsync(p, "SV22000003");
        await p.FillAsync("#txtSoPhieuMuon", soPm);
        await p.ClickAsync("#btnTimPhieuMuon");
        await p.WaitForSelectorAsync("#dgvChiTiet tbody tr input[type=checkbox]");
        await TrinhDuyet.ChupAsync(p, "4.3.2_Lap_phieu_gia_han_TT2", toanTrang: true);
        await ChonTatCaAsync(p);
        await p.WaitForSelectorAsync("#dgvChiTiet tbody >> text=Đồng ý");
        await p.WaitForFunctionAsync("() => !document.getElementById('btnIn').disabled");
        await TrinhDuyet.ChupAsync(p, "4.3.2_Lap_phieu_gia_han_TT3", toanTrang: true);
        await InVaChupAsync(ctx, p, "4.4.2_Phieu_gia_han_sach_MB03");
    }

    [Fact]
    public async Task Chup_433_LapPhieuTra_443_MB04()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-tra/lap");
        await TrinhDuyet.ChupAsync(p, "4.3.3_Lap_phieu_tra_TT1", toanTrang: true);
        var soPm = await PhieuDangMuonAsync(p, "SV22000006"); // bạn đọc mẫu đang mượn quá hạn
        await p.FillAsync("#txtSoPhieuMuon", soPm);
        await p.ClickAsync("#btnTimPhieuMuon");
        await p.WaitForSelectorAsync("#dgvChiTiet tbody tr input[type=checkbox]");
        await TrinhDuyet.ChupAsync(p, "4.3.3_Lap_phieu_tra_TT2", toanTrang: true);
        await ChonTatCaAsync(p);
        await p.WaitForFunctionAsync("() => !document.getElementById('btnIn').disabled");
        await TrinhDuyet.ChupAsync(p, "4.3.3_Lap_phieu_tra_TT3", toanTrang: true);
        await InVaChupAsync(ctx, p, "4.4.3_Phieu_tra_sach_MB04");
    }

    [Fact]
    public async Task Chup_434_LapBienBan_444_MB05()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "vipham", "/quan-tri/bien-ban/lap");
        await p.WaitForFunctionAsync("() => !document.querySelector('#dgvTruongHop').hasAttribute('aria-busy') && document.querySelector('#dgvTruongHop tbody tr')");
        if (await p.Locator("#dgvTruongHop tbody tr[data-chi-so]").CountAsync() == 0)
        {
            // CSDL không còn trường hợp mẫu (đã chạy kiểm thử trước đó) → N1 tạo một trường hợp: trả sách hư hỏng
            await using var ctx1 = await td.MayTinhAsync();
            var n1 = await MoAsync(ctx1, "muontra", "/quan-tri/phieu-tra/lap");
            var (bd, _) = await Pha3Tests.TaoBanDocAsync(n1);
            var sach = await Pha3Tests.SachSanSangAsync(n1, bd, 1);
            var soPm = await Pha3Tests.LapPhieuMuonApiAsync(n1, bd, sach);
            await n1.EvaluateAsync("""
                async ([pm, ma]) => window.__goi('POST', '/api/quan-tri/phieu-tra', { soPMuon: pm, ngayTra: new Date().toISOString().slice(0, 10),
                  chiTiet: [{ maBanSach: ma, tinhTrangTra: 'Hư hỏng', ghiChuTra: 'Rách bìa' }] })
                """, new[] { soPm, sach[0] });
            await p.ReloadAsync();
        }
        await p.WaitForSelectorAsync("#dgvTruongHop tbody tr[data-chi-so]");
        await TrinhDuyet.ChupAsync(p, "4.3.4_Lap_bien_ban_vi_pham_TT1", toanTrang: true);
        // Trường hợp mẫu: bạn đọc BD0007 trả quá hạn, chưa lập biên bản (nếu còn), nếu không thì dòng đầu tiên
        var dong = p.Locator("#dgvTruongHop tbody tr:has-text('Đặng Ngọc Hà')");
        if (await dong.CountAsync() == 0) dong = p.Locator("#dgvTruongHop tbody tr[data-chi-so]").First;
        await dong.Locator("td:nth-child(2)").DblClickAsync();
        await p.WaitForSelectorAsync("#dgvSachViPham tbody tr[data-chi-so]");
        await p.SelectOptionAsync("#cboKhoanThu", await p.Locator("#dgvSachViPham tbody tr", new() { HasText = "Quá hạn" }).CountAsync() > 0 ? "KT01" : "KT03");
        await TrinhDuyet.ChupAsync(p, "4.3.4_Lap_bien_ban_vi_pham_TT2", toanTrang: true);
        await p.ClickAsync("#btnThemCT");
        await p.WaitForSelectorAsync("#dgvChiTiet tbody tr[data-chi-so]");
        await p.SelectOptionAsync("#cboHTThanhToan", "Tiền mặt");
        await TrinhDuyet.ChupAsync(p, "4.3.4_Lap_bien_ban_vi_pham_TT3", toanTrang: true);
        await InVaChupAsync(ctx, p, "4.4.4_Bien_ban_vi_pham_MB05");
    }
}
