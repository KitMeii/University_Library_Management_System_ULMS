using BaseCore.Common.Auth;
using BaseCore.DTO.Common;
using BaseCore.MuonTraService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.MuonTraService.Controllers;

/// <summary>
/// SPEC 6.1.3 Danh mục Bạn đọc (01.01.01). Quyền SPEC 3.2 (B03 BANDOC): R N1,N2,N4 · C N1 · E,D N1,N4.
/// Tiến trình: TT003 tìm kiếm Bạn đọc, TT007/TT009/TT010 kiểm tra điều kiện mượn.
/// </summary>
[ApiController]
[Route("api/quan-tri/ban-doc")]
public sealed class BanDocController(BanDocService service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("BANDOC_R")]
    public Task<KetQuaTrang<BanDocDto>> DanhSach([FromQuery] ThamSoLuoi ts, CancellationToken ct) => service.DanhSachAsync(ts, ct);

    /// <summary>txtMaSV, btnTimKiem.</summary>
    [HttpGet("tim")]
    [YeuCauQuyen("TT003")]
    public Task<KetQuaTimBanDoc> Tim([FromQuery] string maSv, CancellationToken ct) => service.TimTheoMaSvAsync(maSv, ct);

    /// <summary>btnKiemTraDK.</summary>
    [HttpGet("{ma}/dieu-kien-muon")]
    [YeuCauQuyen("TT007", "TT008")]
    public Task<DieuKienMuonDto> DieuKien(string ma, CancellationToken ct) => service.KiemTraDieuKienAsync(ma, ct);

    [HttpGet("ma-ke-tiep")]
    [YeuCauQuyen("BANDOC_C")]
    public async Task<IActionResult> MaKeTiep(CancellationToken ct) => Ok(new { maBanDoc = await service.MaKeTiepAsync(ct) });

    [HttpPost]
    [YeuCauQuyen("BANDOC_C")]
    public Task<BanDocDto> Them(BanDocNhap n, CancellationToken ct) => service.ThemAsync(n, User.MaNd(), ct);

    [HttpPut("{ma}")]
    [YeuCauQuyen("BANDOC_E")]
    public Task<BanDocDto> Sua(string ma, BanDocNhap n, CancellationToken ct) => service.SuaAsync(ma, n, User.MaNd(), ct);

    [HttpDelete("{ma}")]
    [YeuCauQuyen("BANDOC_D")]
    public async Task<IActionResult> Xoa(string ma, CancellationToken ct)
    {
        await service.XoaAsync(ma, User.MaNd(), ct);
        return NoContent();
    }
}
