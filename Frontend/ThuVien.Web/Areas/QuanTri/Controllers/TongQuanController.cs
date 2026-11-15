using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuVien.Web.Areas.QuanTri.Models;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>Trang tổng quan theo nhóm (biểu đồ ECharts ở Pha 4) và trang chức năng chưa hoàn thiện.</summary>
[Area("QuanTri")]
[Authorize(Policy = ChinhSach.NhanVien)]
[YeuCauDoiMatKhauFilter]
public sealed class TongQuanController : Controller
{
    [HttpGet("quan-tri")]
    public IActionResult Index() => View();

    /// <summary>
    /// Mọi đường dẫn trong menu chưa có giao diện ở pha hiện tại → trang “đang xây dựng” ghi rõ pha hoàn thiện
    /// (không link chết, không dữ liệu giả). Kiểm tra quyền giống menu → ngoài quyền trả 403.
    /// </summary>
    [HttpGet("quan-tri/{**duongDan}", Order = 1000)]
    public IActionResult ChucNang(string duongDan)
    {
        var tienTrinh = Request.Query["tienTrinh"].ToString();
        var day = "/quan-tri/" + duongDan + (string.IsNullOrEmpty(tienTrinh) ? "" : "?tienTrinh=" + tienTrinh);
        var tim = MenuQuanTri.Tim(day) ?? MenuQuanTri.Tim("/quan-tri/" + duongDan);
        if (tim is null) return NotFound();
        if (!tim.Value.Muc.DuocThay(User)) return Forbid(XacThucWeb.NhanVien);
        return View("DangXayDung", tim.Value);
    }
}
