using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThuVien.Web.Areas.QuanTri.Models;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>Giao diện lập chứng từ SPEC 6.2.1–6.2.4 và trang in MB02–MB05 (SPEC 6.4.1–6.4.4, file 08 mục 3).</summary>
[Area("QuanTri")]
[Authorize(Policy = ChinhSach.NhanVien)]
[YeuCauDoiMatKhauFilter]
public sealed class ChungTuController(GatewayClient gateway, IOptions<ThuVienOptions> opt) : Controller
{
    /// <summary>6.2.1 Lập Phiếu mượn sách (01.01.03) – màn hình mặc định của N1.</summary>
    [HttpGet("quan-tri/phieu-muon/lap")]
    [QuyenTrang("TT012")]
    public IActionResult LapPhieuMuon() => View();

    /// <summary>6.2.2 Lập Phiếu gia hạn sách (01.02.01).</summary>
    [HttpGet("quan-tri/phieu-gia-han/lap")]
    [QuyenTrang("TT030")]
    public IActionResult LapPhieuGiaHan() => View();

    /// <summary>6.2.3 Lập Phiếu trả sách (01.02.02).</summary>
    [HttpGet("quan-tri/phieu-tra/lap")]
    [QuyenTrang("TT043")]
    public IActionResult LapPhieuTra() => View();

    /// <summary>6.2.4 Lập Biên bản vi phạm mượn trả sách (02.03.01) – màn hình mặc định của N2.</summary>
    [HttpGet("quan-tri/bien-ban/lap")]
    [QuyenTrang("TT074")]
    public IActionResult LapBienBan() => View();

    /// <summary>MB02 Phiếu mượn sách – A5, 02 bản (TT028).</summary>
    [HttpGet("quan-tri/in/phieu-muon/{so}")]
    [QuyenTrang("TT028")]
    public Task<IActionResult> InPhieuMuon(string so, CancellationToken ct) =>
        InAsync<PhieuMuonIn>($"/api/quan-tri/phieu-muon/{Uri.EscapeDataString(so)}", "MB02", "PHIẾU MƯỢN SÁCH", "A5", so,
            ["Liên 1: Giao Bạn đọc", "Liên 2: Lưu tại Bộ phận mượn trả sách"], "InPhieuMuon", ct);

    /// <summary>MB03 Phiếu gia hạn sách – A5, 02 bản (TT042).</summary>
    [HttpGet("quan-tri/in/phieu-gia-han/{so}")]
    [QuyenTrang("TT042")]
    public Task<IActionResult> InPhieuGiaHan(string so, CancellationToken ct) =>
        InAsync<PhieuGiaHanIn>($"/api/quan-tri/phieu-gia-han/{Uri.EscapeDataString(so)}", "MB03", "PHIẾU GIA HẠN SÁCH", "A5", so,
            ["Liên 1: Giao Bạn đọc", "Liên 2: Lưu tại Bộ phận mượn trả sách"], "InPhieuGiaHan", ct);

    /// <summary>MB04 Phiếu trả sách – A5, 02 bản (TT056).</summary>
    [HttpGet("quan-tri/in/phieu-tra/{so}")]
    [QuyenTrang("TT056")]
    public Task<IActionResult> InPhieuTra(string so, CancellationToken ct) =>
        InAsync<PhieuTraIn>($"/api/quan-tri/phieu-tra/{Uri.EscapeDataString(so)}", "MB04", "PHIẾU TRẢ SÁCH", "A5", so,
            ["Liên 1: Giao Bạn đọc", "Liên 2: Lưu tại Bộ phận mượn trả sách"], "InPhieuTra", ct);

    /// <summary>MB05 Biên bản vi phạm mượn trả sách – A4, 02 bản (TT089).</summary>
    [HttpGet("quan-tri/in/bien-ban/{so}")]
    [QuyenTrang("TT089")]
    public Task<IActionResult> InBienBan(string so, CancellationToken ct) =>
        InAsync<BienBanIn>($"/api/quan-tri/bien-ban/{Uri.EscapeDataString(so)}", "MB05", "BIÊN BẢN VI PHẠM MƯỢN TRẢ SÁCH", "A4", so,
            ["Liên 1: Giao Bạn đọc", "Liên 2: Lưu tại Bộ phận xử lý vi phạm"], "InBienBan", ct);

    private async Task<IActionResult> InAsync<T>(string url, string maMau, string tenMau, string khoGiay, string so, string[] lien, string view, CancellationToken ct)
    {
        var token = await HttpContext.GetTokenAsync(XacThucWeb.NhanVien, XacThucWeb.TokenTen);
        if (token is null) return Challenge(XacThucWeb.NhanVien);
        var kq = await gateway.LayAsync<T>(url, token, ct);
        if (kq.MaHttp == StatusCodes.Status401Unauthorized) return Challenge(XacThucWeb.NhanVien);
        if (!kq.ThanhCong || kq.DuLieu is null)
        {
            Response.StatusCode = kq.MaHttp is >= 400 and < 600 ? kq.MaHttp : 502;
            return View("KhongIn", kq.Loi?.ThongBao ?? "Không lấy được dữ liệu chứng từ.");
        }
        var tenTruong = opt.Value.ThongTin.TenTruong;
        return View(view, new KhungIn(maMau, tenMau, khoGiay, so.ToUpperInvariant(), tenTruong, lien, kq.DuLieu));
    }
}
