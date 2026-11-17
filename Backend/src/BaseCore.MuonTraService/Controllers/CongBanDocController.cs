using System.ComponentModel.DataAnnotations;
using BaseCore.Common.Auth;
using BaseCore.Common.HeThong;
using BaseCore.MuonTraService.NghiepVu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BaseCore.MuonTraService.Controllers;

/// <summary>API cổng bạn đọc: đăng nhập, kích hoạt (file 07 mục 1–2).</summary>
[ApiController]
[Route("api/cong")]
public sealed class CongBanDocController(XacThucBanDocService service, TaiKhoanBanDocService taiKhoan) : ControllerBase
{
    public sealed record YeuCauDangNhap([Required] string MaSvHvCb, [Required] string MatKhau);

    [HttpPost("dang-nhap")]
    [AllowAnonymous]
    [EnableRateLimiting(GioiHanTanSuat.DangNhap)]
    public Task<KetQuaDangNhapBanDoc> DangNhap(YeuCauDangNhap yc, CancellationToken ct) =>
        service.DangNhapAsync(yc.MaSvHvCb, yc.MatKhau, ct);

    /// <summary>Kích hoạt tài khoản / quên mật khẩu (đặt lại bằng mã SV/HV/CB + số thẻ + ngày sinh).</summary>
    [HttpPost("kich-hoat")]
    [AllowAnonymous]
    [EnableRateLimiting(GioiHanTanSuat.DangNhap)]
    public async Task<IActionResult> KichHoat(KichHoatNhap n, CancellationToken ct) =>
        Ok(new { thongBao = await taiKhoan.KichHoatAsync(n, ct) });
}

/// <summary>Trang cá nhân bạn đọc (file 07 mục 3) – chỉ đọc dữ liệu gốc của chính bạn đọc đang đăng nhập.</summary>
[ApiController]
[Route("api/cong/tai-khoan")]
[Authorize(Policy = ChinhSach.BanDoc)]
public sealed class TaiKhoanCongController(TaiKhoanBanDocService service) : ControllerBase
{
    private string Ma => User.FindFirst(ThuVienClaims.MaBanDoc)!.Value;

    [HttpGet("toi")]
    public async Task<IActionResult> Toi(CancellationToken ct) => Ok((await service.TongQuanAsync(Ma, ct)).The);

    [HttpGet("tong-quan")]
    public Task<TongQuanBanDocDto> TongQuan(CancellationToken ct) => service.TongQuanAsync(Ma, ct);

    [HttpGet("dang-muon")]
    public Task<List<SachDangMuonCongDto>> DangMuon(CancellationToken ct) => service.DangMuonAsync(Ma, ct);

    [HttpGet("lich-su")]
    public Task<object> LichSu([FromQuery] int? nam, CancellationToken ct) => service.LichSuAsync(Ma, nam, ct);

    [HttpGet("vi-pham")]
    public Task<List<ViPhamCongDto>> ViPham(CancellationToken ct) => service.ViPhamAsync(Ma, ct);

    [HttpPost("doi-mat-khau")]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauBanDocNhap n, CancellationToken ct)
    {
        await service.DoiMatKhauAsync(Ma, n, ct);
        return Ok(new { thongBao = "Đã đổi mật khẩu." });
    }
}

/// <summary>Yêu thích (YEUTHICH) và thông báo (THONGBAO) của bạn đọc.</summary>
[ApiController]
[Route("api/cong")]
[Authorize(Policy = ChinhSach.BanDoc)]
public sealed class YeuThichThongBaoController(TaiKhoanBanDocService service) : ControllerBase
{
    private string Ma => User.FindFirst(ThuVienClaims.MaBanDoc)!.Value;

    [HttpGet("yeu-thich")]
    public Task<List<YeuThichDto>> YeuThich(CancellationToken ct) => service.YeuThichAsync(Ma, ct);

    [HttpPut("yeu-thich/{maDauSach}")]
    public async Task<IActionResult> Them(string maDauSach, CancellationToken ct) =>
        Ok(new { yeuThich = await service.DatYeuThichAsync(Ma, maDauSach, true, ct) });

    [HttpDelete("yeu-thich/{maDauSach}")]
    public async Task<IActionResult> Bo(string maDauSach, CancellationToken ct) =>
        Ok(new { yeuThich = await service.DatYeuThichAsync(Ma, maDauSach, false, ct) });

    [HttpGet("thong-bao")]
    public Task<object> ThongBao([FromQuery] int soLuong = 10, CancellationToken ct = default) => service.ThongBaoAsync(Ma, soLuong, ct);

    [HttpPost("thong-bao/{ma:long}/da-doc")]
    public async Task<IActionResult> DaDoc(long ma, CancellationToken ct) => Ok(new { soDong = await service.DanhDauDaDocAsync(Ma, ma, ct) });

    [HttpPost("thong-bao/da-doc-tat-ca")]
    public async Task<IActionResult> DaDocTatCa(CancellationToken ct) => Ok(new { soDong = await service.DanhDauDaDocAsync(Ma, null, ct) });
}

/// <summary>Đặt chỗ phía bạn đọc (file 06 mục 6–7).</summary>
[ApiController]
[Route("api/cong/dat-cho")]
[Authorize(Policy = ChinhSach.BanDoc)]
public sealed class DatChoCongController(DatChoService service) : ControllerBase
{
    public sealed record YeuCauDatCho(string? MaDauSach);
    private string Ma => User.FindFirst(ThuVienClaims.MaBanDoc)!.Value;

    [HttpPost]
    public Task<KetQuaDatCho> Dat(YeuCauDatCho yc, CancellationToken ct) => service.DatAsync(Ma, yc.MaDauSach, ct);

    [HttpDelete("{maDatCho}")]
    public Task<DatChoDto> Huy(string maDatCho, CancellationToken ct) => service.HuyCuaBanDocAsync(Ma, maDatCho, ct);

    [HttpGet("cua-toi")]
    public Task<List<DatChoDto>> CuaToi(CancellationToken ct) => service.DanhSachCuaToiAsync(Ma, ct);

    /// <summary>Phần “đặt chỗ của tôi” của Book Preview (đặt chỗ đang giữ, yêu thích, lý do chưa đặt được).</summary>
    [HttpGet("dau-sach/{maDauSach}")]
    public Task<TrangThaiDauSachCuaToi> TheoDauSach(string maDauSach, CancellationToken ct) =>
        service.TrangThaiDauSachAsync(Ma, maDauSach.Trim().ToUpperInvariant(), ct);
}

/// <summary>Quầy đặt chỗ của N1 (file 06 mục 5 – mở rộng): DATCHO_R xem, DATCHO_GH hủy.</summary>
[ApiController]
[Route("api/quan-tri/dat-cho")]
public sealed class DatChoQuayController(DatChoService service) : ControllerBase
{
    public sealed record YeuCauHuy(string? LyDo);

    [HttpGet]
    [YeuCauQuyen("DATCHO_R")]
    public Task<List<DatChoQuayDto>> DanhSach([FromQuery] string? trangThai, [FromQuery] DateOnly? tuNgay, [FromQuery] DateOnly? denNgay,
        [FromQuery] string? maSv, CancellationToken ct) => service.DanhSachQuayAsync(trangThai, tuNgay, denNgay, maSv, ct);

    [HttpGet("tom-tat")]
    [YeuCauQuyen("DATCHO_R")]
    public Task<object> TomTat(CancellationToken ct) => service.TomTatQuayAsync(ct);

    /// <summary>dgvDatCho ở 01.01.03 Lập Phiếu mượn sách (mở rộng).</summary>
    [HttpGet("ban-doc/{maBanDoc}")]
    [YeuCauQuyen("DATCHO_GH")]
    public Task<List<DatChoQuayDto>> CuaBanDoc(string maBanDoc, CancellationToken ct) => service.DangGiuCuaBanDocAsync(maBanDoc, ct);

    [HttpPost("{ma}/huy")]
    [YeuCauQuyen("DATCHO_GH")]
    public Task<DatChoDto> Huy(string ma, YeuCauHuy yc, CancellationToken ct) => service.HuyCuaNhanVienAsync(ma, yc.LyDo, User.MaNd(), ct);
}
