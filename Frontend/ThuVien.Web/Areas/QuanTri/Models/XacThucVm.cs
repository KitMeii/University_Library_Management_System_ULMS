using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ThuVien.Web.Areas.QuanTri.Models;

public sealed class DangNhapVm
{
    public string? TenDangNhap { get; set; }
    public string? MatKhau { get; set; }
    public string? ReturnUrl { get; set; }
    public string? ThongBaoLoi { get; set; }
    public string? ThongBao { get; set; }
}

public sealed class DoiMatKhauVm
{
    public string? MatKhauCu { get; set; }
    public string? MatKhauMoi { get; set; }
    public string? NhapLai { get; set; }
    public bool BatBuoc { get; set; }
    public string? ThongBaoLoi { get; set; }
    public Dictionary<string, string> LoiTruong { get; } = new(StringComparer.OrdinalIgnoreCase);

    public DoiMatKhauVm XoaMatKhau()
    {
        MatKhauCu = MatKhauMoi = NhapLai = null;
        return this;
    }
}

/// <summary>D22: tài khoản phải đổi mật khẩu lần đầu thì mọi trang dashboard chuyển về trang Đổi mật khẩu.</summary>
public sealed class YeuCauDoiMatKhauFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.User.HasClaim(Controllers.XacThucController.ClaimPhaiDoiMatKhau, "1") &&
            context.HttpContext.User.HasClaim(ThuVienClaims.Loai, LoaiTaiKhoan.NhanVien))
        {
            context.Result = new RedirectResult("/quan-tri/doi-mat-khau");
        }
    }
}
