using BaseCore.Common.Auth;
using BaseCore.DTO.Common;
using BaseCore.ViPhamService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.ViPhamService.Controllers;

/// <summary>SPEC 6.1.2 Danh mục khoản thu. Quyền SPEC 3.2 (B02 KHOANTHU): R N2,N4 · C,E,D N4.</summary>
[ApiController]
[Route("api/quan-tri/khoan-thu")]
public sealed class KhoanThuController(KhoanThuService service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("KHOANTHU_R")]
    public Task<KetQuaTrang<KhoanThuDto>> DanhSach([FromQuery] ThamSoLuoi ts, CancellationToken ct) => service.DanhSachAsync(ts, ct);

    [HttpPost]
    [YeuCauQuyen("KHOANTHU_C")]
    public Task<KhoanThuDto> Them(KhoanThuNhap n, CancellationToken ct) => service.ThemAsync(n, User.MaNd(), ct);

    [HttpPut("{ma}")]
    [YeuCauQuyen("KHOANTHU_E")]
    public Task<KhoanThuDto> Sua(string ma, KhoanThuNhap n, CancellationToken ct) => service.SuaAsync(ma, n, User.MaNd(), ct);

    [HttpDelete("{ma}")]
    [YeuCauQuyen("KHOANTHU_D")]
    public async Task<IActionResult> Xoa(string ma, CancellationToken ct)
    {
        await service.XoaAsync(ma, User.MaNd(), ct);
        return NoContent();
    }
}
