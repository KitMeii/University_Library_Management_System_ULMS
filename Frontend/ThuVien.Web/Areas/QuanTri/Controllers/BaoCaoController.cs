using System.Text.RegularExpressions;
using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThuVien.Web.Areas.QuanTri.Models;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>
/// Thống kê – báo cáo (SPEC 6.4.5–6.4.6, file 09 mục 3): 01.04.02 / 02.05.01 Tổng hợp (bộ lọc kỳ → btnTongHop → bảng + biểu đồ),
/// 01.04.03 / 02.05.02 Lập báo cáo (xem trước MB06/MB07 → btnIn), trang in A4 01 bản dựng biểu đồ SVG.
/// </summary>
[Area("QuanTri")]
[Authorize(Policy = ChinhSach.NhanVien)]
[YeuCauDoiMatKhauFilter]
public sealed partial class BaoCaoController(GatewayClient gateway, IOptions<ThuVienOptions> opt) : Controller
{
    /// <summary>01.04.02 Tổng hợp tình hình mượn trả sách (TT057).</summary>
    [HttpGet("quan-tri/bao-cao/muon-tra")]
    [QuyenTrang("TT057")]
    public IActionResult TongHopMuonTra() => View("TongHop", new BaoCaoVm("muon-tra", null, null));

    /// <summary>01.04.03 Lập Báo cáo tình hình mượn trả sách (TT069).</summary>
    [HttpGet("quan-tri/bao-cao/muon-tra/lap")]
    [QuyenTrang("TT069")]
    public IActionResult LapMuonTra(string? tuNgay, string? denNgay) => View("Lap", new BaoCaoVm("muon-tra", Ngay(tuNgay), Ngay(denNgay)));

    /// <summary>02.05.01 Tổng hợp tình hình vi phạm mượn trả sách (TT090).</summary>
    [HttpGet("quan-tri/bao-cao/vi-pham")]
    [QuyenTrang("TT090")]
    public IActionResult TongHopViPham() => View("TongHop", new BaoCaoVm("vi-pham", null, null));

    /// <summary>02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách (TT095).</summary>
    [HttpGet("quan-tri/bao-cao/vi-pham/lap")]
    [QuyenTrang("TT095")]
    public IActionResult LapViPham(string? tuNgay, string? denNgay) => View("Lap", new BaoCaoVm("vi-pham", Ngay(tuNgay), Ngay(denNgay)));

    /// <summary>MB06 – A4, 01 bản (TT071 hiển thị chi tiết, TT073 in).</summary>
    [HttpGet("quan-tri/in/bao-cao-muon-tra")]
    [QuyenTrang("TT073")]
    public Task<IActionResult> InMuonTra(string? tuNgay, string? denNgay, CancellationToken ct) =>
        InAsync<BaoCaoMuonTraIn>("/api/quan-tri/bao-cao/muon-tra", tuNgay, denNgay, "MB06", "BÁO CÁO TÌNH HÌNH MƯỢN TRẢ SÁCH", "InMuonTra", ct);

    /// <summary>MB07 – A4, 01 bản (TT097 hiển thị chi tiết, TT098 in).</summary>
    [HttpGet("quan-tri/in/bao-cao-vi-pham")]
    [QuyenTrang("TT098")]
    public Task<IActionResult> InViPham(string? tuNgay, string? denNgay, CancellationToken ct) =>
        InAsync<BaoCaoViPhamIn>("/api/quan-tri/bao-cao/vi-pham", tuNgay, denNgay, "MB07", "BÁO CÁO TÌNH HÌNH VI PHẠM MƯỢN TRẢ SÁCH", "InViPham", ct);

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex NgayIso();

    private static string? Ngay(string? v) => v is not null && NgayIso().IsMatch(v) ? v : null;

    private async Task<IActionResult> InAsync<T>(string api, string? tuNgay, string? denNgay, string maMau, string tenMau, string view, CancellationToken ct)
    {
        var token = await HttpContext.GetTokenAsync(XacThucWeb.NhanVien, XacThucWeb.TokenTen);
        if (token is null) return Challenge(XacThucWeb.NhanVien);
        var url = $"{api}?tuNgay={Uri.EscapeDataString(Ngay(tuNgay) ?? "")}&denNgay={Uri.EscapeDataString(Ngay(denNgay) ?? "")}";
        var kq = await gateway.LayAsync<T>(url, token, ct);
        if (kq.MaHttp == StatusCodes.Status401Unauthorized) return Challenge(XacThucWeb.NhanVien);
        if (!kq.ThanhCong || kq.DuLieu is null)
        {
            Response.StatusCode = kq.MaHttp is >= 400 and < 600 ? kq.MaHttp : 502;
            var chiTiet = kq.Loi?.Truong is { Count: > 0 } t ? " " + string.Join(" ", t.Values) : "";
            return View("~/Areas/QuanTri/Views/ChungTu/KhongIn.cshtml", (kq.Loi?.ThongBao ?? "Không lấy được số liệu báo cáo.") + chiTiet);
        }
        ViewData["NgayLap"] = DateOnly.FromDateTime(DateTime.Now);
        ViewData["NguoiLap"] = User.TenNd();
        return View(view, new KhungIn(maMau, tenMau, "A4", "", opt.Value.ThongTin.TenTruong, ["Nơi nhận: Ban giám đốc thư viện"], kq.DuLieu));
    }
}
