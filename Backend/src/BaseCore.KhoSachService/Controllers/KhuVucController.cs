using BaseCore.Common.Auth;
using BaseCore.DTO.Common;
using BaseCore.KhoSachService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.KhoSachService.Controllers;

/// <summary>SPEC 6.1.4 Danh mục khu vực lưu trữ. Quyền theo SPEC 3.2 (B04 KHUVUC): R N1,N3,N4 · C,E N3,N4 · D N4.</summary>
[ApiController]
[Route("api/quan-tri/khu-vuc")]
public sealed class KhuVucController(KhuVucService service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("KHUVUC_R")]
    public Task<KetQuaTrang<KhuVucDto>> DanhSach([FromQuery] ThamSoLuoi ts, CancellationToken ct) => service.DanhSachAsync(ts, ct);

    [HttpPost]
    [YeuCauQuyen("KHUVUC_C")]
    public Task<KhuVucDto> Them(KhuVucNhap n, CancellationToken ct) => service.ThemAsync(n, User.MaNd(), ct);

    [HttpPut("{ma}")]
    [YeuCauQuyen("KHUVUC_E")]
    public Task<KhuVucDto> Sua(string ma, KhuVucNhap n, CancellationToken ct) => service.SuaAsync(ma, n, User.MaNd(), ct);

    [HttpDelete("{ma}")]
    [YeuCauQuyen("KHUVUC_D")]
    public async Task<IActionResult> Xoa(string ma, CancellationToken ct)
    {
        await service.XoaAsync(ma, User.MaNd(), ct);
        return NoContent();
    }
}
