using System.ComponentModel.DataAnnotations;
using BaseCore.Common.Auth;
using BaseCore.Common.HeThong;
using BaseCore.QuanTriService.NghiepVu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BaseCore.QuanTriService.Controllers;

/// <summary>SPEC 6.3.1 Đăng nhập; menu Hệ thống: Đăng xuất, Đổi mật khẩu.</summary>
[ApiController]
[Route("api/quan-tri/xac-thuc")]
public sealed class XacThucController(XacThucNhanVienService service) : ControllerBase
{
    public sealed record YeuCauDangNhap([Required] string TenDangNhap, [Required] string MatKhau);
    public sealed record YeuCauDoiMatKhau([Required] string MatKhauCu, [Required] string MatKhauMoi);

    /// <summary>btnDangNhap</summary>
    [HttpPost("dang-nhap")]
    [AllowAnonymous]
    [EnableRateLimiting(GioiHanTanSuat.DangNhap)]
    public Task<KetQuaDangNhapNhanVien> DangNhap(YeuCauDangNhap yc, CancellationToken ct) =>
        service.DangNhapAsync(yc.TenDangNhap, yc.MatKhau, ct);

    [HttpPost("dang-xuat")]
    [Authorize(Policy = ChinhSach.NhanVien)]
    public async Task<IActionResult> DangXuat(CancellationToken ct)
    {
        await service.DangXuatAsync(User.FindFirst(ThuVienClaims.MaNd)!.Value,
            User.FindFirst(ThuVienClaims.MaDangNhap)?.Value ?? string.Empty, ct);
        return NoContent();
    }

    [HttpPost("doi-mat-khau")]
    [Authorize(Policy = ChinhSach.NhanVien)]
    public async Task<IActionResult> DoiMatKhau(YeuCauDoiMatKhau yc, CancellationToken ct)
    {
        await service.DoiMatKhauAsync(User.FindFirst(ThuVienClaims.MaNd)!.Value, yc.MatKhauCu, yc.MatKhauMoi, ct);
        return NoContent();
    }
}
