using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Controllers;

public sealed class DangNhapBanDocVm
{
    public string? MaSvHvCb { get; set; }
    public string? MatKhau { get; set; }
    public bool GhiNho { get; set; }
    public string? ReturnUrl { get; set; }
    public string? ThongBaoLoi { get; set; }
}

/// <summary>Đăng nhập cổng bạn đọc (file 07 mục 2). Kích hoạt, trang cá nhân: Pha 6.</summary>
public sealed class TaiKhoanController(GatewayClient gateway) : Controller
{
    [HttpGet("/dang-nhap")]
    [AllowAnonymous]
    public IActionResult DangNhap(string? returnUrl = null) => View(new DangNhapBanDocVm { ReturnUrl = returnUrl });

    [HttpPost("/dang-nhap")]
    [AllowAnonymous]
    public async Task<IActionResult> DangNhap(DangNhapBanDocVm vm, CancellationToken ct)
    {
        var kq = await gateway.DangNhapBanDocAsync(vm.MaSvHvCb ?? string.Empty, vm.MatKhau ?? string.Empty, ct);
        if (!kq.ThanhCong || kq.DuLieu is null)
        {
            ModelState.Clear();
            return View(new DangNhapBanDocVm { MaSvHvCb = vm.MaSvHvCb, GhiNho = vm.GhiNho, ReturnUrl = vm.ReturnUrl, ThongBaoLoi = kq.Loi?.ThongBao });
        }
        var d = kq.DuLieu;
        var claims = new[]
        {
            new Claim(ThuVienClaims.Loai, LoaiTaiKhoan.BanDoc),
            new Claim(ThuVienClaims.MaBanDoc, d.MaBanDoc),
            new Claim(ThuVienClaims.Ten, d.HoTen),
            new Claim(ThuVienClaims.MaSvHvCb, d.MaSvHvCb),
        };
        var hetHan = new JwtSecurityTokenHandler().ReadJwtToken(d.Token).ValidTo;
        await HttpContext.SignInAsync(XacThucWeb.BanDoc, XacThucWeb.TaoNguoiDung(XacThucWeb.BanDoc, claims),
            XacThucWeb.TaoThuocTinh(d.Token, new DateTimeOffset(hetHan, TimeSpan.Zero), vm.GhiNho));
        return Redirect(Url.IsLocalUrl(vm.ReturnUrl) ? vm.ReturnUrl! : "/");
    }

    [HttpPost("/dang-xuat")]
    [Authorize(Policy = ChinhSach.BanDoc)]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync(XacThucWeb.BanDoc);
        return Redirect("/");
    }
}
