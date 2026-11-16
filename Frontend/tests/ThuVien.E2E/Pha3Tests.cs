using System.Text.RegularExpressions;
using BaseCore.Tests.Shared;
using Deque.AxeCore.Playwright;

namespace ThuVien.E2E;

/// <summary>
/// Pha 3: SPEC 6.2.1–6.2.4 – 3 trạng thái form, trạng thái nút đọc TRỰC TIẾP từ bảng “Control và xử lý” của SPEC,
/// luồng mượn → gia hạn → trả → biên bản, trang in MB02–MB05.
/// </summary>
[Collection("TrinhDuyet")]
public class Pha3Tests(TrinhDuyet td)
{
    /// <summary>Hỗ trợ dữ liệu kiểm thử qua chính BFF của trang (cookie + AntiForgery) – không cần quyền CSDL.</summary>
    internal const string JsHoTro = """
        () => { window.__goi = async (method, url, body) => {
          const r = await fetch(url, { method, credentials: 'same-origin', headers: { 'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('meta[name="csrf-token"]').content }, body: body ? JSON.stringify(body) : undefined });
          const t = await r.text(); if (!r.ok) throw new Error(r.status + ' ' + t); return t ? JSON.parse(t) : null; }; }
        """;

    internal static async Task<(string Ma, string MaSv)> TaoBanDocAsync(IPage p)
    {
        await p.EvaluateAsync(JsHoTro);
        var ma = "BE" + Random.Shared.Next(10_000_000, 99_999_999);
        var sv = "CBE" + Random.Shared.Next(10_000_000, 99_999_999);
        await p.EvaluateAsync("""
            async ([ma, sv]) => { const d = new Date(); const iso = (x) => x.toISOString().slice(0, 10);
              const sau = new Date(d); sau.setFullYear(d.getFullYear() + 2); const truoc = new Date(d); truoc.setDate(d.getDate() - 1);
              await window.__goi('POST', '/api/quan-tri/ban-doc', { maBanDoc: ma, hoTen: 'Bạn đọc kiểm thử E2E', doiTuong: 'Cán bộ', maSvHvCb: sv,
                lopDonVi: 'Phòng thử nghiệm', ngaySinh: '1990-01-01', soTheBd: 'E' + sv, hsdTuNgay: iso(truoc), hsdDenNgay: iso(sau) }); }
            """, new[] { ma, sv });
        return (ma, sv);
    }

    internal static async Task<string[]> SachSanSangAsync(IPage p, string maBanDoc, int soLuong)
    {
        await p.EvaluateAsync(JsHoTro);
        return await p.EvaluateAsync<string[]>("""
            async ([bd, n]) => { const ds = []; const dau = Math.floor(Math.random() * 100);
              for (let k = 0; k < 112 && ds.length < n; k++) { const ma = 'BS' + String((dau + k) % 112 + 1).padStart(5, '0');
                try { const r = await window.__goi('GET', `/api/quan-tri/phieu-muon/ban-sach?maBanSach=${ma}&maBanDoc=${bd}`); if (r.dapUng) ds.push(ma); } catch { } }
              return ds; }
            """, new object[] { maBanDoc, soLuong });
    }

    internal static async Task<string> LapPhieuMuonApiAsync(IPage p, string maBanDoc, string[] sach) =>
        await p.EvaluateAsync<string>("""
            async ([bd, ds]) => (await window.__goi('POST', '/api/quan-tri/phieu-muon',
              { maBanDoc: bd, chiTiet: ds.map((m) => ({ maBanSach: m, tinhTrangMuon: 'Bình thường' })) })).so
            """, new object[] { maBanDoc, sach });

    private static Task<bool> Bat(IPage p, string id) => p.EvaluateAsync<bool>(
        "(id) => { const e = document.getElementById(id); return !!e && !e.disabled && e.getAttribute('aria-disabled') !== 'true'; }", id);

    /// <summary>So trạng thái nút trên trang với một dòng của bảng “Control và xử lý” SPEC.</summary>
    private static async Task DungSpecAsync(IPage p, string muc, string control)
    {
        var (bat, tat) = SpecDoc.TrangThaiNut(muc)[control];
        foreach (var id in bat) Assert.True(await Bat(p, id), $"{muc} {control}: {id} phải Enable = True");
        foreach (var id in tat) Assert.False(await Bat(p, id), $"{muc} {control}: {id} phải Enable = False");
    }

    private static async Task<IPage> MoAsync(IBrowserContext ctx, string ten, string duongDan)
    {
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, ten);
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        await p.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return p;
    }

    private static async Task<string> InVaLaySoAsync(IPage p, string tienTo)
    {
        await p.ClickAsync("#btnIn");
        await p.WaitForSelectorAsync(".qt-toast--ok");
        await p.WaitForSelectorAsync("#khuInGanNhat:not([hidden])");
        var href = await p.GetAttributeAsync("#khuInGanNhat a", "href");
        var m = Regex.Match(href!, $@"/({tienTo}\d{{8}})\?");
        Assert.True(m.Success, href);
        return m.Groups[1].Value;
    }

    private static async Task KiemTraTrangInAsync(IBrowserContext ctx, string duongDan, string tenMau, string maMau, string khoGiay, params string[] chua)
    {
        var p = await ctx.NewPageAsync();
        var r = await p.GotoAsync(TrinhDuyet.Web + duongDan);
        Assert.Equal(200, r!.Status);
        Assert.Equal(2, await p.Locator(".in-to").CountAsync()); // 02 bản in
        Assert.Equal(maMau, await p.GetAttributeAsync("body", "data-ma-mau"));
        Assert.Contains("in--" + khoGiay, await p.GetAttributeAsync("body", "class"));
        var noiDung = await p.InnerTextAsync("main");
        Assert.Contains(tenMau, noiDung);
        Assert.Contains("THƯ VIỆN TRƯỜNG ĐẠI HỌC", noiDung);
        foreach (var c in chua) Assert.Contains(c, noiDung);
        await p.CloseAsync();
    }

    // ---------- 6.2.1 Lập Phiếu mượn sách ----------
    [Fact]
    public async Task PhieuMuon_BaTrangThai_DungSpec_VaIn()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-muon/lap");
        await DungSpecAsync(p, "6.2.1", "frmLapPhieuMuon (Load)");
        Assert.Equal("TT1", await p.GetAttributeAsync(".qt-buoc", "data-trang-thai-form"));

        var (bd, sv) = await TaoBanDocAsync(p);
        var sach = await SachSanSangAsync(p, bd, 2);
        await p.FillAsync("#txtMaSV", sv);
        await p.ClickAsync("#btnTimBanDoc");
        await p.WaitForSelectorAsync("#khuDieuKien >> text=đủ điều kiện mượn");
        await DungSpecAsync(p, "6.2.1", "txtMaSV, btnTimBanDoc");
        Assert.Equal("Bạn đọc kiểm thử E2E", await p.InputValueAsync("#txtHoTen"));

        await p.FillAsync("#txtMaBanSach", sach[0]);
        await p.ClickAsync("#btnThemSach");
        await p.WaitForSelectorAsync($"#dgvChiTiet tbody tr:has-text('{sach[0]}')");
        await DungSpecAsync(p, "6.2.1", "txtMaBanSach, btnThemSach");
        Assert.Equal("TT3", await p.GetAttributeAsync(".qt-buoc", "data-trang-thai-form"));

        // dgvChiTiet: double click → bỏ dòng chưa lưu; lưới trống → btnIn = False
        await p.DblClickAsync($"#dgvChiTiet tbody tr:has-text('{sach[0]}') td:nth-child(2)");
        await p.CheckAsync(".modal.show #dlgBoDong");
        await p.ClickAsync(".modal.show [data-nut='dong-y']");
        await p.WaitForSelectorAsync("#dgvChiTiet .qt-bang__rong");
        Assert.False(await Bat(p, "btnIn"));

        foreach (var ma in sach)
        {
            await p.FillAsync("#txtMaBanSach", ma);
            await p.PressAsync("#txtMaBanSach", "Enter");
            await p.WaitForSelectorAsync($"#dgvChiTiet tbody tr:has-text('{ma}')");
        }
        Assert.Equal("2", await p.InnerTextAsync("#lblTongSoBan"));
        var so = await InVaLaySoAsync(p, "PM");
        await DungSpecAsync(p, "6.2.1", "btnIn"); // in xong: xóa trắng, về trạng thái 1
        Assert.Equal("", await p.InputValueAsync("#txtMaSV"));
        await KiemTraTrangInAsync(ctx, "/quan-tri/in/phieu-muon/" + so, "PHIẾU MƯỢN SÁCH", "MB02", "a5", so, sach[0], sach[1], "Tổng số bản sách mượn: 2");

        // btnTimPhieu: mở phiếu đã lưu ở chế độ sửa → trạng thái 3; btnHuy → trạng thái 1
        await p.FillAsync("#txtSoPhieu", so);
        await p.ClickAsync("#btnTimPhieu");
        await p.WaitForSelectorAsync($"#dgvChiTiet tbody tr:has-text('{sach[1]}')");
        await DungSpecAsync(p, "6.2.1", "btnTimPhieu");
        await p.ClickAsync("#btnHuy");
        await DungSpecAsync(p, "6.2.1", "btnHuy");
    }

    [Fact]
    public async Task PhieuMuon_BanDocKhongDuDieuKien_GiuTrangThai1()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-muon/lap");
        await p.FillAsync("#txtMaSV", "SV22000004"); // thẻ hết hạn (dữ liệu mẫu)
        await p.ClickAsync("#btnTimBanDoc");
        await p.WaitForSelectorAsync("#khuDieuKien >> text=chưa đủ điều kiện");
        await DungSpecAsync(p, "6.2.1", "frmLapPhieuMuon (Load)");
    }

    // ---------- 6.2.2 Lập Phiếu gia hạn sách ----------
    [Fact]
    public async Task PhieuGiaHan_BaTrangThai_DungSpec_VaIn()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-gia-han/lap");
        await DungSpecAsync(p, "6.2.2", "frmLapPhieuGiaHan (Load)");
        var (bd, _) = await TaoBanDocAsync(p);
        var sach = await SachSanSangAsync(p, bd, 1);
        var soPm = await LapPhieuMuonApiAsync(p, bd, sach);

        await p.FillAsync("#txtSoPhieuMuon", soPm);
        await p.ClickAsync("#btnTimPhieuMuon");
        await p.WaitForSelectorAsync($"#dgvChiTiet tbody tr:has-text('{sach[0]}')");
        await DungSpecAsync(p, "6.2.2", "txtSoPhieuMuon, btnTimPhieuMuon");

        await p.CheckAsync("#dgvChiTiet tbody tr input[type=checkbox]");
        await p.WaitForSelectorAsync("#dgvChiTiet tbody >> text=Đồng ý");
        Assert.True(await Bat(p, "btnIn")); // dòng hợp lệ → trạng thái 3
        Assert.Equal("1", await p.InnerTextAsync("#lblDongY"));
        var so = await InVaLaySoAsync(p, "GH");
        await DungSpecAsync(p, "6.2.2", "btnIn");
        await KiemTraTrangInAsync(ctx, "/quan-tri/in/phieu-gia-han/" + so, "PHIẾU GIA HẠN SÁCH", "MB03", "a5", so, soPm, "Đồng ý");
    }

    // ---------- 6.2.3 Lập Phiếu trả sách + 6.2.4 Lập Biên bản vi phạm ----------
    [Fact]
    public async Task PhieuTra_ViPham_ChuyenSangBienBan_VaIn()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-tra/lap");
        await DungSpecAsync(p, "6.2.3", "frmLapPhieuTra (Load)");
        var (bd, _) = await TaoBanDocAsync(p);
        var sach = await SachSanSangAsync(p, bd, 2);
        var soPm = await LapPhieuMuonApiAsync(p, bd, sach);

        await p.FillAsync("#txtSoPhieuMuon", soPm);
        await p.ClickAsync("#btnTimPhieuMuon");
        await p.WaitForSelectorAsync($"#dgvChiTiet tbody tr:has-text('{sach[1]}')");
        await DungSpecAsync(p, "6.2.3", "txtSoPhieuMuon, btnTimPhieuMuon");
        await p.CheckAsync($"#dgvChiTiet tbody tr:has-text('{sach[0]}') input[type=checkbox]");
        Assert.True(await Bat(p, "btnIn"));
        await p.CheckAsync($"#dgvChiTiet tbody tr:has-text('{sach[1]}') input[type=checkbox]");
        await p.SelectOptionAsync($"#dgvChiTiet tbody tr:has-text('{sach[1]}') select", "Mất");
        Assert.Equal("1", await p.InnerTextAsync("#lblViPham"));
        var soPt = await InVaLaySoAsync(p, "PT");
        await DungSpecAsync(p, "6.2.3", "btnIn");
        await KiemTraTrangInAsync(ctx, "/quan-tri/in/phieu-tra/" + soPt, "PHIẾU TRẢ SÁCH", "MB04", "a5", soPt, soPm, "Mất", "Tổng số bản sách trả: 2");

        // N2 nhận trường hợp vi phạm (dgvTruongHop), lập biên bản, ghi nhận thanh toán, in MB05 (A4)
        await using var ctx2 = await td.MayTinhAsync();
        var b = await MoAsync(ctx2, "vipham", "/quan-tri/bien-ban/lap");
        await DungSpecAsync(b, "6.2.4", "frmLapBienBan (Load)");
        await b.WaitForSelectorAsync($"#dgvTruongHop tbody tr:has-text('{soPm}')");
        await b.DblClickAsync($"#dgvTruongHop tbody tr:has-text('{soPm}') td:nth-child(2)");
        await b.WaitForSelectorAsync($"#dgvSachViPham tbody tr:has-text('{sach[1]}')");
        await DungSpecAsync(b, "6.2.4", "txtSoPhieuMuon, btnTimPhieuMuon");
        await b.SelectOptionAsync("#cboKhoanThu", "KT02");
        await b.SelectOptionAsync("#cboBanSach", sach[1]);
        await b.FillAsync("#txtSoLuong", "1");
        await b.ClickAsync("#btnThemCT");
        await b.WaitForSelectorAsync("#dgvChiTiet tbody tr:has-text('KT02')");
        await DungSpecAsync(b, "6.2.4", "btnThemCT");
        Assert.Equal("200.000 đ", (await b.InnerTextAsync("#lblTongTien")).Trim());
        await b.SelectOptionAsync("#cboHTThanhToan", "Tiền mặt");
        var soBb = await InVaLaySoAsync(b, "BB");
        await DungSpecAsync(b, "6.2.4", "btnIn");
        await KiemTraTrangInAsync(ctx2, "/quan-tri/in/bien-ban/" + soBb, "BIÊN BẢN VI PHẠM MƯỢN TRẢ SÁCH", "MB05", "a4", soBb, soPm, "Tiền mặt", "200.000");

        // btnTimBienBan → trạng thái 3
        await b.FillAsync("#txtSoBienBan", soBb);
        await b.ClickAsync("#btnTimBienBan");
        await b.WaitForSelectorAsync("#dgvChiTiet tbody tr:has-text('KT02')");
        await DungSpecAsync(b, "6.2.4", "btnTimBienBan");
    }

    [Fact]
    public async Task TrangIn_NgoaiQuyen_403_VaKhongTimThay()
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, "khosach", "/quan-tri");
        await p.GotoAsync(TrinhDuyet.Web + "/quan-tri/in/bien-ban/BB00000000");
        Assert.Contains("/quan-tri/khong-co-quyen", p.Url); // N3 không có TT089
        await using var ctx1 = await td.MayTinhAsync();
        var n1 = await MoAsync(ctx1, "muontra", "/quan-tri");
        var r2 = await n1.GotoAsync(TrinhDuyet.Web + "/quan-tri/in/phieu-muon/PM00000000");
        Assert.Equal(404, r2!.Status);
        Assert.Contains("Không tìm thấy", await n1.InnerTextAsync("main"));
    }

    [Theory]
    [InlineData("muontra", "/quan-tri/phieu-muon/lap")]
    [InlineData("muontra", "/quan-tri/phieu-tra/lap")]
    [InlineData("vipham", "/quan-tri/bien-ban/lap")]
    public async Task Axe_FormChungTu(string ten, string duongDan)
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, ten, duongDan);
        await p.WaitForTimeoutAsync(600);
        var kq = await p.RunAxe();
        var loi = kq.Violations.Where(v => v.Impact is "critical" or "serious").ToList();
        Assert.True(loi.Count == 0, string.Join("\n", loi.Select(v => $"{v.Id}: {v.Help} – " + string.Join("; ", v.Nodes.Select(n => n.Target)))));
    }

    [Fact]
    public async Task DienThoai_FormChungTu_KhongTranNgang()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await MoAsync(ctx, "muontra", "/quan-tri/phieu-muon/lap");
        Assert.True(await p.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= window.innerWidth + 1"));
        Assert.True(await p.IsVisibleAsync("#btnTimBanDoc"));
    }
}
