using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BaseCore.Common.Auth;
using Microsoft.AspNetCore.Antiforgery;
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

public sealed class KichHoatVm
{
    public string? MaSvHvCb { get; set; }
    public string? SoTheBd { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string? MatKhau { get; set; }
    public string? NhapLai { get; set; }
    public string? ThongBaoLoi { get; set; }
    public string? ThongBaoThanhCong { get; set; }
    public IReadOnlyDictionary<string, string>? LoiTruong { get; set; }
}

public sealed record DangNhapNhanhYc(string? MaSvHvCb, string? MatKhau);

/// <summary>
/// Đăng nhập, kích hoạt (file 07 mục 1–2) và trang cá nhân bạn đọc (file 07 mục 3). Trang cá nhân render khung phía server,
/// dữ liệu nạp qua /api/cong/** (BFF gắn token từ cookie BanDoc).
/// </summary>
public sealed class TaiKhoanController(GatewayClient gateway, IAntiforgery antiforgery) : Controller
{
    /// <summary>Các trang con của /tai-khoan (thanh viên thuốc dọc – file 07 mục 3).</summary>
    public static readonly (string Ma, string Ten, string Icon)[] TrangCon =
    [
        ("", "Tổng quan", "bi-grid-1x2"),
        ("dang-muon", "Sách đang mượn", "bi-book"),
        ("dat-cho", "Đặt chỗ", "bi-bookmark-check"),
        ("lich-su", "Lịch sử", "bi-clock-history"),
        ("vi-pham", "Vi phạm và khoản phải nộp", "bi-receipt"),
        ("yeu-thich", "Yêu thích", "bi-heart"),
        ("thong-bao", "Thông báo", "bi-bell"),
        ("doi-mat-khau", "Đổi mật khẩu", "bi-key"),
    ];

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
        await DangNhapAsync(kq.DuLieu, vm.GhiNho);
        return Redirect(Url.IsLocalUrl(vm.ReturnUrl) ? vm.ReturnUrl! : "/");
    }

    /// <summary>
    /// Hộp đăng nhập nhúng trong Book Preview (file 07 mục 2): cùng API, không rời trang. Trả token AntiForgery mới gắn với
    /// danh tính bạn đọc để trang tiếp tục gọi API ghi (đặt chỗ) ngay sau khi đăng nhập.
    /// </summary>
    [HttpPost("/dang-nhap/nhanh")]
    [AllowAnonymous]
    public async Task<IActionResult> DangNhapNhanh([FromBody] DangNhapNhanhYc yc, CancellationToken ct)
    {
        var kq = await gateway.DangNhapBanDocAsync(yc.MaSvHvCb ?? string.Empty, yc.MatKhau ?? string.Empty, ct);
        if (!kq.ThanhCong || kq.DuLieu is null)
            return StatusCode(kq.MaHttp is >= 400 and < 600 ? kq.MaHttp : 400, kq.Loi);
        HttpContext.User = await DangNhapAsync(kq.DuLieu, false);
        var csrf = antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
        return Json(new { hoTen = kq.DuLieu.HoTen, csrf });
    }

    [HttpGet("/kich-hoat")]
    [AllowAnonymous]
    public IActionResult KichHoat() => View(new KichHoatVm());

    [HttpPost("/kich-hoat")]
    [AllowAnonymous]
    public async Task<IActionResult> KichHoat(KichHoatVm vm, CancellationToken ct)
    {
        var kq = await gateway.KichHoatBanDocAsync(new { vm.MaSvHvCb, vm.SoTheBd, vm.NgaySinh, vm.MatKhau, vm.NhapLai }, ct);
        ModelState.Clear();
        return View(kq.ThanhCong
            ? new KichHoatVm { MaSvHvCb = vm.MaSvHvCb, ThongBaoThanhCong = kq.DuLieu?.ThongBao }
            : new KichHoatVm { MaSvHvCb = vm.MaSvHvCb, SoTheBd = vm.SoTheBd, NgaySinh = vm.NgaySinh, ThongBaoLoi = kq.Loi?.ThongBao, LoiTruong = kq.Loi?.Truong });
    }

    [HttpGet("/tai-khoan/{trang?}")]
    [Authorize(Policy = ChinhSach.BanDoc)]
    public IActionResult TrangCaNhan(string? trang)
    {
        trang ??= string.Empty;
        var muc = TrangCon.FirstOrDefault(x => x.Ma == trang);
        if (muc.Ten is null) return NotFound();
        ViewData["Title"] = muc.Ten;
        ViewData["TrangCon"] = trang;
        return View("TrangCaNhan");
    }

    [HttpPost("/dang-xuat")]
    [Authorize(Policy = ChinhSach.BanDoc)]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync(XacThucWeb.BanDoc);
        return Redirect("/");
    }

    private async Task<ClaimsPrincipal> DangNhapAsync(DangNhapBanDocKq d, bool ghiNho)
    {
        var claims = new[]
        {
            new Claim(ThuVienClaims.Loai, LoaiTaiKhoan.BanDoc),
            new Claim(ThuVienClaims.MaBanDoc, d.MaBanDoc),
            new Claim(ThuVienClaims.Ten, d.HoTen),
            new Claim(ThuVienClaims.MaSvHvCb, d.MaSvHvCb),
        };
        var hetHan = new JwtSecurityTokenHandler().ReadJwtToken(d.Token).ValidTo;
        var nguoi = XacThucWeb.TaoNguoiDung(XacThucWeb.BanDoc, claims);
        await HttpContext.SignInAsync(XacThucWeb.BanDoc, nguoi, XacThucWeb.TaoThuocTinh(d.Token, new DateTimeOffset(hetHan, TimeSpan.Zero), ghiNho));
        return nguoi;
    }
}
