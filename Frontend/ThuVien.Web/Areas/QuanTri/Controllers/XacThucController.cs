using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuVien.Web.Areas.QuanTri.Models;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>SPEC 6.3.1 Đăng nhập (frmDangNhap) + menu Hệ thống: Đăng xuất, Đổi mật khẩu.</summary>
[Area("QuanTri")]
[Route("quan-tri")]
public sealed class XacThucController(GatewayClient gateway) : Controller
{
    public const string ClaimPhaiDoiMatKhau = "phai_doi_mk";

    [HttpGet("dang-nhap")]
    [AllowAnonymous]
    public IActionResult DangNhap(string? returnUrl = null, string? thongBao = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.HasClaim(ThuVienClaims.Loai, LoaiTaiKhoan.NhanVien))
            return Redirect("/quan-tri");
        return View(new DangNhapVm { ReturnUrl = returnUrl, ThongBao = thongBao });
    }

    /// <summary>btnDangNhap</summary>
    [HttpPost("dang-nhap")]
    [AllowAnonymous]
    public async Task<IActionResult> DangNhap(DangNhapVm vm, CancellationToken ct)
    {
        var kq = await gateway.DangNhapNhanVienAsync(vm.TenDangNhap ?? string.Empty, vm.MatKhau ?? string.Empty, ct);
        if (!kq.ThanhCong || kq.DuLieu is null)
        {
            // SPEC 6.3.1: thông báo, xóa trắng ô mật khẩu
            ModelState.Clear();
            return View(new DangNhapVm { TenDangNhap = vm.TenDangNhap, ReturnUrl = vm.ReturnUrl, ThongBaoLoi = kq.Loi?.ThongBao });
        }

        var d = kq.DuLieu;
        var claims = new List<Claim>
        {
            new(ThuVienClaims.Loai, LoaiTaiKhoan.NhanVien),
            new(ThuVienClaims.MaNd, d.MaNd),
            new(ThuVienClaims.Ten, d.TenNd),
            new(ThuVienClaims.TenDangNhap, d.TenDangNhap),
            new(ThuVienClaims.MaNhom, d.MaNhom),
            new(ThuVienClaims.TenNhom, d.TenNhom),
            new(ThuVienClaims.MaDangNhap, d.MaDangNhap),
        };
        claims.AddRange(d.Quyen.Select(q => new Claim(ThuVienClaims.Quyen, q)));
        if (d.PhaiDoiMatKhau) claims.Add(new Claim(ClaimPhaiDoiMatKhau, "1"));

        var hetHan = new JwtSecurityTokenHandler().ReadJwtToken(d.Token).ValidTo;
        await HttpContext.SignInAsync(XacThucWeb.NhanVien, XacThucWeb.TaoNguoiDung(XacThucWeb.NhanVien, claims),
            XacThucWeb.TaoThuocTinh(d.Token, new DateTimeOffset(hetHan, TimeSpan.Zero), ghiNho: false));

        if (d.PhaiDoiMatKhau) return Redirect("/quan-tri/doi-mat-khau");
        // SPEC 3.3: mở màn hình mặc định của nhóm (trừ khi đang quay lại một trang cụ thể)
        return Redirect(Url.IsLocalUrl(vm.ReturnUrl) && vm.ReturnUrl != "/quan-tri" ? vm.ReturnUrl! : d.ManHinhMacDinh);
    }

    [HttpPost("dang-xuat")]
    [Authorize(Policy = ChinhSach.NhanVien)]
    public async Task<IActionResult> DangXuat(CancellationToken ct)
    {
        var kq = await HttpContext.AuthenticateAsync(XacThucWeb.NhanVien);
        var token = kq.Properties?.GetTokenValue(XacThucWeb.TokenTen);
        if (token is not null) await gateway.DangXuatNhanVienAsync(token, ct); // ghi NHATKY_DANGNHAP.tg_dangxuat
        await HttpContext.SignOutAsync(XacThucWeb.NhanVien);
        return Redirect("/quan-tri/dang-nhap?thongBao=" + Uri.EscapeDataString("Bạn đã đăng xuất."));
    }

    [HttpGet("doi-mat-khau")]
    [Authorize(Policy = ChinhSach.NhanVien)]
    public IActionResult DoiMatKhau() => View(new DoiMatKhauVm { BatBuoc = User.HasClaim(ClaimPhaiDoiMatKhau, "1") });

    [HttpPost("doi-mat-khau")]
    [Authorize(Policy = ChinhSach.NhanVien)]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauVm vm, CancellationToken ct)
    {
        vm.BatBuoc = User.HasClaim(ClaimPhaiDoiMatKhau, "1");
        if (vm.MatKhauMoi != vm.NhapLai)
        {
            vm.LoiTruong["nhapLai"] = "Mật khẩu nhập lại không khớp.";
            return View(vm.XoaMatKhau());
        }
        var dn = await HttpContext.AuthenticateAsync(XacThucWeb.NhanVien);
        var kq = await gateway.DoiMatKhauAsync(dn.Properties?.GetTokenValue(XacThucWeb.TokenTen) ?? string.Empty,
            vm.MatKhauCu ?? string.Empty, vm.MatKhauMoi ?? string.Empty, ct);
        if (!kq.ThanhCong)
        {
            vm.ThongBaoLoi = kq.Loi?.ThongBao;
            foreach (var (k, v) in kq.Loi?.Truong ?? new Dictionary<string, string>()) vm.LoiTruong[k] = v;
            return View(vm.XoaMatKhau());
        }
        // Đổi xong: đăng nhập lại để nạp phiên mới
        await gateway.DangXuatNhanVienAsync(dn.Properties?.GetTokenValue(XacThucWeb.TokenTen) ?? string.Empty, ct);
        await HttpContext.SignOutAsync(XacThucWeb.NhanVien);
        return Redirect("/quan-tri/dang-nhap?thongBao=" + Uri.EscapeDataString("Đổi mật khẩu thành công. Vui lòng đăng nhập lại."));
    }

    [HttpGet("khong-co-quyen")]
    [AllowAnonymous]
    public IActionResult KhongCoQuyen()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
