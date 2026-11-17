using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThuVien.Web.Areas.QuanTri.Models;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>
/// Quầy đặt chỗ của Bộ phận mượn trả sách (file 06 mục 5 – mở rộng): danh sách đặt chỗ, hủy có lý do, in phiếu soạn sách.
/// Không có nút “Nhận”: nhận sách chỉ qua 01.01.03 Lập Phiếu mượn sách (btnNhanDatCho).
/// </summary>
[Area("QuanTri")]
[Authorize(Policy = ChinhSach.NhanVien)]
[YeuCauDoiMatKhauFilter]
public sealed class DatChoController(GatewayClient gateway, IOptions<ThuVienOptions> opt) : Controller
{
    [HttpGet("quan-tri/dat-cho")]
    [QuyenTrang("DATCHO_R")]
    public IActionResult Index() => View();

    /// <summary>Phiếu soạn sách (A4): bản sách “Đang giữ” cần lấy từ kệ/kho để chuẩn bị trước, nhóm theo khu vực.</summary>
    [HttpGet("quan-tri/in/phieu-soan-sach")]
    [QuyenTrang("DATCHO_R")]
    public async Task<IActionResult> InPhieuSoanSach(CancellationToken ct)
    {
        var token = await HttpContext.GetTokenAsync(XacThucWeb.NhanVien, XacThucWeb.TokenTen);
        if (token is null) return Challenge(XacThucWeb.NhanVien);
        var kq = await gateway.LayAsync<List<DatChoQuayIn>>("/api/quan-tri/dat-cho?trangThai=" + Uri.EscapeDataString("Đang giữ"), token, ct);
        if (kq.MaHttp == StatusCodes.Status401Unauthorized) return Challenge(XacThucWeb.NhanVien);
        if (!kq.ThanhCong || kq.DuLieu is null)
        {
            Response.StatusCode = kq.MaHttp is >= 400 and < 600 ? kq.MaHttp : 502;
            return View("~/Areas/QuanTri/Views/ChungTu/KhongIn.cshtml", kq.Loi?.ThongBao ?? "Không lấy được danh sách đặt chỗ.");
        }
        ViewData["NguoiLap"] = User.TenNd();
        return View(new KhungIn("Mở rộng", "PHIẾU SOẠN SÁCH ĐẶT CHỖ", "A4", DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            opt.Value.ThongTin.TenTruong, ["Bộ phận mượn trả sách"], kq.DuLieu));
    }
}
