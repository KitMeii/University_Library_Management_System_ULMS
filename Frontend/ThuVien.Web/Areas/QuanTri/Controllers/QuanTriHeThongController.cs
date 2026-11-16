using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuVien.Web.Areas.QuanTri.Models;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>Giao diện quản trị hệ thống SPEC 6.3.2–6.3.5 (N4).</summary>
[Area("QuanTri")]
[Authorize(Policy = ChinhSach.NhanVien)]
[YeuCauDoiMatKhauFilter]
public sealed class QuanTriHeThongController : Controller
{
    /// <summary>6.3.2 Quản lý tài khoản người dùng (04.00.01).</summary>
    [HttpGet("quan-tri/tai-khoan")]
    [QuyenTrang("QT040001")]
    public IActionResult TaiKhoan() => View();

    /// <summary>6.3.3 Cấp quyền cho tài khoản (04.00.02).</summary>
    [HttpGet("quan-tri/phan-quyen")]
    [QuyenTrang("QT040002")]
    public IActionResult PhanQuyen() => View();

    /// <summary>6.3.4 Theo dõi nhật ký hệ thống – mở đúng thẻ theo chức năng chọn trên menu (04.00.03 / 04.00.08).</summary>
    [HttpGet("quan-tri/nhat-ky/{the:regex(^(dang-nhap|hoat-dong)$)}")]
    [QuyenTrang("QT040003", "QT040008")]
    public IActionResult NhatKy(string the) => View(new NhatKyVm(the,
        User.CoQuyen("QT040003"), User.CoQuyen("QT040008")));

    /// <summary>6.3.5 Sao lưu và phục hồi dữ liệu (04.00.09).</summary>
    [HttpGet("quan-tri/sao-luu")]
    [QuyenTrang("QT040009")]
    public IActionResult SaoLuu() => View();
}

public sealed record NhatKyVm(string The, bool XemDangNhap, bool XemHoatDong);
