using BaseCore.Common.Auth;
using BaseCore.DTO.Common;
using BaseCore.KhoSachService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.KhoSachService.Controllers;

/// <summary>
/// SPEC 6.1.1 Danh mục sách. Quyền SPEC 3.2 (B01 SACH): R N1,N3,N4 · C,E N3,N4 · D N4.
/// Tiến trình 3.4: 03.01.02 tra cứu (TT103), 03.04.01 thống kê (TT119/TT120), cập nhật tình trạng TT108/TT112/TT116.
/// </summary>
[ApiController]
[Route("api/quan-tri/sach")]
public sealed class SachController(SachService service) : ControllerBase
{
    public sealed record YeuCauCapNhat(string Loai);

    [HttpGet]
    [YeuCauQuyen("SACH_R")]
    public Task<KetQuaTrang<SachDto>> DanhSach([FromQuery] ThamSoLuoi ts, [FromQuery] string? tinhTrang, CancellationToken ct) =>
        service.DanhSachAsync(ts, tinhTrang, ct);

    /// <summary>cboLocTinhTrang – 03.04.01 Tổng hợp số sách theo tình trạng.</summary>
    [HttpGet("thong-ke-tinh-trang")]
    [YeuCauQuyen("SACH_R")]
    public Task<IReadOnlyList<ThongKeTinhTrang>> ThongKe(CancellationToken ct) => service.ThongKeAsync(ct);

    [HttpPost]
    [YeuCauQuyen("SACH_C")]
    public Task<SachDto> Them(SachNhap n, CancellationToken ct) => service.ThemAsync(n, User.MaNd(), ct);

    [HttpPut("{ma}")]
    [YeuCauQuyen("SACH_E")]
    public Task<SachDto> Sua(string ma, SachNhap n, CancellationToken ct) => service.SuaAsync(ma, n, User.MaNd(), ct);

    [HttpDelete("{ma}")]
    [YeuCauQuyen("SACH_D")]
    public async Task<IActionResult> Xoa(string ma, CancellationToken ct)
    {
        await service.XoaAsync(ma, User.MaNd(), ct);
        return NoContent();
    }

    /// <summary>dgvChuyenDen.</summary>
    [HttpGet("chuyen-den")]
    [YeuCauQuyen("TT108", "TT112", "TT116")]
    public Task<IReadOnlyList<ChuyenDenDto>> ChuyenDen(CancellationToken ct) => service.ChuyenDenAsync(ct);

    /// <summary>btnCapNhatTT.</summary>
    [HttpPost("{ma}/cap-nhat-tinh-trang")]
    [YeuCauQuyen("TT108", "TT112", "TT116")]
    public Task<SachDto> CapNhatTinhTrang(string ma, YeuCauCapNhat yc, CancellationToken ct) =>
        service.CapNhatTinhTrangAsync(ma, yc.Loai, User.CoQuyen, User.MaNd(), ct);
}
