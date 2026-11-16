namespace ThuVien.E2E;

/// <summary>
/// Bộ ảnh Bài 4 – Pha 4: 6.4.5 ↔ 4.5.1 (MB06), 6.4.6 ↔ 4.5.2 (MB07) gồm form tổng hợp, form lập báo cáo và trang in;
/// trang tổng quan N1–N4 (máy tính) và N1 (điện thoại). Kỳ báo cáo 60/90 ngày gần nhất để có dữ liệu mẫu 2 tháng.
/// </summary>
[Collection("TrinhDuyet")]
public class ChupAnhPha4Tests(TrinhDuyet td)
{
    private static async Task<IPage> MoAsync(IBrowserContext ctx, string ten, string duongDan)
    {
        var p = await ctx.NewPageAsync();
        await TrinhDuyet.DangNhapNhanVienAsync(p, ten);
        await p.GotoAsync(TrinhDuyet.Web + duongDan);
        await p.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return p;
    }

    private static string Iso(DateOnly d) => d.ToString("yyyy-MM-dd");
    private static DateOnly HomNay => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Ho_Chi_Minh"));

    private async Task ChupBaoCaoAsync(string ten, string loai, int soNgay, string so, string tenAnh)
    {
        var ky = $"tuNgay={Iso(HomNay.AddDays(-(soNgay - 1)))}&denNgay={Iso(HomNay)}";
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, ten, $"/quan-tri/bao-cao/{loai}");
        await TrinhDuyet.ChupAsync(p, $"{so}_Tong_hop_{tenAnh}_TT1", toanTrang: true);
        await p.GotoAsync(TrinhDuyet.Web + $"/quan-tri/bao-cao/{loai}?{ky}");
        await p.WaitForSelectorAsync("#khuKetQua:not([hidden])");
        await Pha4Tests.ChoBieuDoAsync(p);
        await Pha4Tests.ChoKpiAsync(p);
        await TrinhDuyet.ChupAsync(p, $"{so}_Tong_hop_{tenAnh}", toanTrang: true);

        await p.ClickAsync("#btnLapBaoCao");
        await p.WaitForURLAsync(u => u.Contains("/lap"));
        await p.WaitForFunctionAsync("() => !document.getElementById('btnIn').disabled");
        await TrinhDuyet.ChupAsync(p, $"{so}_Lap_bao_cao_{tenAnh}");

        var trang = await ctx.NewPageAsync();
        await trang.GotoAsync(TrinhDuyet.Web + $"/quan-tri/in/bao-cao-{loai}?{ky}");
        await trang.WaitForSelectorAsync(".in-bieu-do svg");
        await TrinhDuyet.ChupAsync(trang, $"{so}_Bao_cao_{tenAnh}_{(loai == "muon-tra" ? "MB06" : "MB07")}", toanTrang: true);

        await using var dt = await td.DienThoaiAsync();
        var m = await MoAsync(dt, ten, $"/quan-tri/bao-cao/{loai}?{ky}");
        await m.WaitForSelectorAsync("#khuKetQua:not([hidden])");
        await Pha4Tests.ChoBieuDoAsync(m);
        await Pha4Tests.ChoKpiAsync(m);
        await TrinhDuyet.ChupAsync(m, $"{so}_Tong_hop_{tenAnh}_mobile");
    }

    [Fact]
    public Task Chup_451_BaoCaoMuonTra_MB06() => ChupBaoCaoAsync("muontra", "muon-tra", 60, "4.5.1", "muon_tra");

    [Fact]
    public Task Chup_452_BaoCaoViPham_MB07() => ChupBaoCaoAsync("vipham", "vi-pham", 90, "4.5.2", "vi_pham");

    [Theory]
    [InlineData("muontra", "N1")]
    [InlineData("vipham", "N2")]
    [InlineData("khosach", "N3")]
    [InlineData("admin", "N4")]
    public async Task Chup_TongQuan(string ten, string nhom)
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await MoAsync(ctx, ten, "/quan-tri");
        await Pha4Tests.ChoBieuDoAsync(p);
        await Pha4Tests.ChoKpiAsync(p);
        await TrinhDuyet.ChupAsync(p, $"Tong_quan_{nhom}", toanTrang: true);
        if (nhom == "N1")
        {
            await using var dt = await td.DienThoaiAsync();
            var m = await MoAsync(dt, ten, "/quan-tri");
            await Pha4Tests.ChoBieuDoAsync(m);
            await Pha4Tests.ChoKpiAsync(m);
            await TrinhDuyet.ChupAsync(m, "Tong_quan_N1_mobile");
        }
    }
}
