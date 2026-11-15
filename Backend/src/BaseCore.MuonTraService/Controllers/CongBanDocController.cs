using System.ComponentModel.DataAnnotations;
using BaseCore.Common.Auth;
using BaseCore.Common.HeThong;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.MuonTraService.NghiepVu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.MuonTraService.Controllers;

/// <summary>API cổng bạn đọc: đăng nhập, thông tin tài khoản (file 07).</summary>
[ApiController]
[Route("api/cong")]
public sealed class CongBanDocController(XacThucBanDocService service, MuonTraDbContext db) : ControllerBase
{
    public sealed record YeuCauDangNhap([Required] string MaSvHvCb, [Required] string MatKhau);

    [HttpPost("dang-nhap")]
    [AllowAnonymous]
    [EnableRateLimiting(GioiHanTanSuat.DangNhap)]
    public Task<KetQuaDangNhapBanDoc> DangNhap(YeuCauDangNhap yc, CancellationToken ct) =>
        service.DangNhapAsync(yc.MaSvHvCb, yc.MatKhau, ct);

    [HttpGet("tai-khoan/toi")]
    [Authorize(Policy = ChinhSach.BanDoc)]
    public async Task<IActionResult> Toi(CancellationToken ct)
    {
        var ma = User.FindFirst(ThuVienClaims.MaBanDoc)!.Value;
        var bd = await db.BanDoc.AsNoTracking().Where(x => x.MaBanDoc == ma)
            .Select(x => new
            {
                maBanDoc = x.MaBanDoc, hoTen = x.HoTen, doiTuong = x.DoiTuong, maSvHvCb = x.MaSvHvCb,
                lopDonVi = x.LopDonVi, soTheBd = x.SoTheBd, hsdTuNgay = x.HsdTuNgay, hsdDenNgay = x.HsdDenNgay
            }).SingleOrDefaultAsync(ct);
        return bd is null ? NotFound() : Ok(bd);
    }
}
