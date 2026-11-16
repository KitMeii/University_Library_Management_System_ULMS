using BaseCore.Common.Auth;
using BaseCore.DTO.Common;
using BaseCore.QuanTriService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.QuanTriService.Controllers;

/// <summary>SPEC 6.3.2 Quản lý tài khoản người dùng (04.00.01).</summary>
[ApiController]
[Route("api/quan-tri/tai-khoan")]
[YeuCauQuyen("QT040001")]
public sealed class TaiKhoanController(TaiKhoanService service) : ControllerBase
{
    [HttpGet] public Task<KetQuaTrang<TaiKhoanDto>> DanhSach([FromQuery] ThamSoLuoi ts, CancellationToken ct) => service.DanhSachAsync(ts, ct);
    [HttpGet("ma-ke-tiep")] public async Task<IActionResult> MaKeTiep(CancellationToken ct) => Ok(new { maNd = await service.MaKeTiepAsync(ct) });
    [HttpPost] public Task<TaiKhoanDto> Them(TaiKhoanNhap n, CancellationToken ct) => service.ThemAsync(n, User.MaNd(), ct);
    [HttpPut("{ma}")] public Task<TaiKhoanDto> Sua(string ma, TaiKhoanNhap n, CancellationToken ct) => service.SuaAsync(ma, n, User.MaNd(), ct);
    /// <summary>btnKhoa.</summary>
    [HttpPost("{ma}/khoa")] public Task<TaiKhoanDto> Khoa(string ma, CancellationToken ct) => service.DoiKhoaAsync(ma, User.MaNd(), ct);
    /// <summary>btnDatLaiMK.</summary>
    [HttpPost("{ma}/dat-lai-mat-khau")]
    public async Task<IActionResult> DatLaiMatKhau(string ma, CancellationToken ct)
    {
        await service.DatLaiMatKhauAsync(ma, User.MaNd(), ct);
        return NoContent();
    }
}

/// <summary>SPEC 6.3.3 Cấp quyền cho tài khoản (04.00.02).</summary>
[ApiController]
[Route("api/quan-tri/phan-quyen")]
[YeuCauQuyen("QT040002")]
public sealed class PhanQuyenController(PhanQuyenService service) : ControllerBase
{
    public sealed record YeuCauLuu(List<string> MaQuyen);

    [HttpGet("{maNhom}")] public Task<IReadOnlyList<QuyenDto>> CuaNhom(string maNhom, CancellationToken ct) => service.QuyenCuaNhomAsync(maNhom, ct);
    [HttpPut("{maNhom}")] public Task<KetQuaLuuQuyen> Luu(string maNhom, YeuCauLuu yc, CancellationToken ct) =>
        service.LuuAsync(maNhom, yc.MaQuyen ?? [], User.MaNd(), ct);
}

/// <summary>SPEC 6.3.4 Theo dõi nhật ký hệ thống (04.00.03, 04.00.08).</summary>
[ApiController]
[Route("api/quan-tri/nhat-ky")]
public sealed class NhatKyController(NhatKyService service) : ControllerBase
{
    [HttpGet("nguoi-dung")]
    [YeuCauQuyen("QT040003", "QT040008")]
    public Task<List<NguoiDungLoc>> NguoiDung(CancellationToken ct) => service.NguoiDungAsync(ct);

    [HttpGet("dang-nhap")]
    [YeuCauQuyen("QT040003")]
    public Task<KetQuaTrang<DongDangNhap>> DangNhap([FromQuery] DateOnly? tuNgay, [FromQuery] DateOnly? denNgay, [FromQuery] string? maNd,
        [FromQuery] ThamSoLuoi ts, CancellationToken ct) => service.DangNhapAsync(tuNgay, denNgay, maNd, ts, ct);

    [HttpGet("hoat-dong")]
    [YeuCauQuyen("QT040008")]
    public Task<KetQuaTrang<DongHoatDong>> HoatDong([FromQuery] DateOnly? tuNgay, [FromQuery] DateOnly? denNgay, [FromQuery] string? maNd,
        [FromQuery] ThamSoLuoi ts, CancellationToken ct) => service.HoatDongAsync(tuNgay, denNgay, maNd, ts, ct);
}

/// <summary>SPEC 6.3.5 Sao lưu và phục hồi dữ liệu (04.00.09).</summary>
[ApiController]
[Route("api/quan-tri/sao-luu")]
[YeuCauQuyen("QT040009")]
public sealed class SaoLuuController(SaoLuuService service) : ControllerBase
{
    public sealed record YeuCauThucHien(string? DuongDan);

    [HttpGet("duyet")] public Task<IReadOnlyList<MucThuMuc>> Duyet([FromQuery] string? duongDan, CancellationToken ct) => service.DuyetAsync(duongDan, ct);
    [HttpGet("phien-dang-mo")] public Task<IReadOnlyList<PhienDangMo>> PhienDangMo(CancellationToken ct) => service.PhienDangMoAsync(User.MaNd(), ct);
    [HttpPost("sao-luu")] public IActionResult SaoLuu(YeuCauThucHien yc) => Accepted(new { ma = service.BatDauSaoLuu(yc.DuongDan, User.MaNd()) });
    [HttpPost("phuc-hoi")] public IActionResult PhucHoi(YeuCauThucHien yc) => Accepted(new { ma = service.BatDauPhucHoi(yc.DuongDan ?? string.Empty, User.MaNd(),
        User.FindFirst(ThuVienClaims.MaDangNhap)?.Value) });
    [HttpGet("tien-trinh/{ma}")]
    public IActionResult TienTrinh(string ma) => service.TrangThai(ma) is { } t ? Ok(t) : NotFound();
}
