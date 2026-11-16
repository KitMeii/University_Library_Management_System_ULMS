using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ThuVien.Web.Areas.QuanTri.Models;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>Trang chỉ mở cho người có một trong các mã quyền (menu và trang kiểm tra cùng quy tắc – SPEC 3.3).</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class QuyenTrangAttribute(params string[] maQuyen) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!maQuyen.Any(context.HttpContext.User.CoQuyen))
            context.Result = new ForbidResult(XacThucWeb.NhanVien);
    }
}

/// <summary>Quyền C/E/D trên bảng (SPEC 3.2) để hiện/ẩn nút (Visible = False khi chỉ có quyền R).</summary>
public sealed record QuyenBang(bool Them, bool Sua, bool Xoa)
{
    public static QuyenBang Tu(System.Security.Claims.ClaimsPrincipal u, string tienTo) =>
        new(u.CoQuyen(tienTo + "_C"), u.CoQuyen(tienTo + "_E"), u.CoQuyen(tienTo + "_D"));
    public bool ChiXem => !Them && !Sua && !Xoa;
}

/// <summary>Giao diện danh mục SPEC 6.1.1–6.1.4.</summary>
[Area("QuanTri")]
[Authorize(Policy = ChinhSach.NhanVien)]
[YeuCauDoiMatKhauFilter]
public sealed class DanhMucController : Controller
{
    /// <summary>6.1.4 Danh mục khu vực lưu trữ (04.00.05).</summary>
    [HttpGet("quan-tri/danh-muc/khu-vuc")]
    [QuyenTrang("KHUVUC_R")]
    public IActionResult KhuVuc() => View(QuyenBang.Tu(User, "KHUVUC"));

    /// <summary>6.1.2 Danh mục khoản thu (04.00.06).</summary>
    [HttpGet("quan-tri/danh-muc/khoan-thu")]
    [QuyenTrang("KHOANTHU_R")]
    public IActionResult KhoanThu() => View(QuyenBang.Tu(User, "KHOANTHU"));

    /// <summary>6.1.3 Danh mục Bạn đọc – N1: 01.01.01 Lưu thông tin Thẻ Bạn đọc; N4: 04.00.07; N2: chỉ xem.</summary>
    [HttpGet("quan-tri/ban-doc")]
    [HttpGet("quan-tri/danh-muc/ban-doc")]
    [QuyenTrang("BANDOC_R")]
    public IActionResult BanDoc() => View(QuyenBang.Tu(User, "BANDOC"));

    /// <summary>6.1.1 Danh mục sách (04.00.04) – N3 thực hiện 03.01.02, 03.01.04, 03.02.03, 03.03.02, 03.04.01.</summary>
    [HttpGet("quan-tri/danh-muc/sach")]
    [QuyenTrang("SACH_R")]
    public IActionResult Sach(string? tienTrinh) => View(new DanhMucSachVm(QuyenBang.Tu(User, "SACH"), tienTrinh,
        CapNhatTinhTrang: User.CoQuyen("TT108") || User.CoQuyen("TT112") || User.CoQuyen("TT116")));
}

public sealed record DanhMucSachVm(QuyenBang Quyen, string? TienTrinh, bool CapNhatTinhTrang);
