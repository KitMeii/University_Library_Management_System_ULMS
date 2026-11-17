using BaseCore.Common.Auth;
using BaseCore.DTO.Common;
using BaseCore.KhoSachService.NghiepVu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.KhoSachService.Controllers;

/// <summary>Quản lý đầu sách (X02, X03 – mở rộng). Quyền mở rộng DAUSACH_C/E/R (N3, N4), DAUSACH_D (N4).</summary>
[ApiController]
[Route("api/quan-tri/dau-sach")]
public sealed class DauSachController(DauSachService service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("DAUSACH_R")]
    public Task<KetQuaTrang<DauSachDto>> DanhSach([FromQuery] ThamSoLuoi ts, [FromQuery] string? maTheLoai, CancellationToken ct) =>
        service.DanhSachAsync(ts, maTheLoai, ct);

    [HttpGet("ma-ke-tiep")]
    [YeuCauQuyen("DAUSACH_C")]
    public async Task<object> MaKeTiep(CancellationToken ct) => new { ma = await service.MaKeTiepAsync(ct) };

    [HttpGet("ban-sach-chua-gan")]
    [YeuCauQuyen("DAUSACH_E")]
    public Task<List<BanSachCuaDauSach>> BanSachChuaGan([FromQuery] string? tuKhoa, CancellationToken ct) => service.BanSachChuaGanAsync(tuKhoa, ct);

    [HttpGet("{ma}")]
    [YeuCauQuyen("DAUSACH_R")]
    public Task<DauSachDto> Mot(string ma, CancellationToken ct) => service.MotAsync(ma, ct);

    [HttpPost]
    [YeuCauQuyen("DAUSACH_C")]
    public Task<DauSachDto> Them(DauSachNhap n, CancellationToken ct) => service.ThemAsync(n, User.MaNd(), ct);

    [HttpPut("{ma}")]
    [YeuCauQuyen("DAUSACH_E")]
    public Task<DauSachDto> Sua(string ma, DauSachNhap n, CancellationToken ct) => service.SuaAsync(ma, n, User.MaNd(), ct);

    [HttpDelete("{ma}")]
    [YeuCauQuyen("DAUSACH_D")]
    public async Task<IActionResult> Xoa(string ma, CancellationToken ct)
    {
        await service.XoaAsync(ma, User.MaNd(), ct);
        return NoContent();
    }

    [HttpGet("{ma}/ban-sach")]
    [YeuCauQuyen("DAUSACH_R")]
    public Task<List<BanSachCuaDauSach>> BanSach(string ma, CancellationToken ct) => service.BanSachAsync(ma, ct);

    [HttpPost("{ma}/ban-sach")]
    [YeuCauQuyen("DAUSACH_E")]
    public async Task<object> GanBanSach(string ma, GanBanSachNhap n, CancellationToken ct) => new { soGan = await service.GanBanSachAsync(ma, n, User.MaNd(), ct) };

    [HttpDelete("{ma}/ban-sach/{maBanSach}")]
    [YeuCauQuyen("DAUSACH_E")]
    public async Task<IActionResult> BoGanBanSach(string ma, string maBanSach, CancellationToken ct)
    {
        await service.BoGanBanSachAsync(ma, maBanSach, User.MaNd(), ct);
        return NoContent();
    }
}

/// <summary>Quản lý thể loại (X01 – mở rộng). Quyền mở rộng THELOAI_C/E/R (N3, N4), THELOAI_D (N4).</summary>
[ApiController]
[Route("api/quan-tri/the-loai")]
public sealed class TheLoaiController(TheLoaiService service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("THELOAI_R")]
    public Task<KetQuaTrang<TheLoaiDto>> DanhSach([FromQuery] ThamSoLuoi ts, CancellationToken ct) => service.DanhSachAsync(ts, ct);

    /// <summary>Ô chọn thể loại ở form đầu sách.</summary>
    [HttpGet("tat-ca")]
    [YeuCauQuyen("THELOAI_R", "DAUSACH_R")]
    public Task<List<TheLoaiDto>> TatCa(CancellationToken ct) => service.TatCaAsync(ct);

    [HttpPost]
    [YeuCauQuyen("THELOAI_C")]
    public Task<TheLoaiDto> Them(TheLoaiNhap n, CancellationToken ct) => service.ThemAsync(n, User.MaNd(), ct);

    [HttpPut("{ma}")]
    [YeuCauQuyen("THELOAI_E")]
    public Task<TheLoaiDto> Sua(string ma, TheLoaiNhap n, CancellationToken ct) => service.SuaAsync(ma, n, User.MaNd(), ct);

    [HttpDelete("{ma}")]
    [YeuCauQuyen("THELOAI_D")]
    public async Task<IActionResult> Xoa(string ma, CancellationToken ct)
    {
        await service.XoaAsync(ma, User.MaNd(), ct);
        return NoContent();
    }
}

/// <summary>Kệ sách nổi bật ở cổng bạn đọc (file 05 mục 2) – công khai, không cần đăng nhập.</summary>
[ApiController]
[Route("api/cong/ke-sach")]
[AllowAnonymous]
public sealed class KeSachController(KeSachService service) : ControllerBase
{
    [HttpGet]
    public Task<KeSachDto> Ke([FromQuery] string? ke, [FromQuery] int? gioiHan, CancellationToken ct) => service.KeAsync(ke, gioiHan, ct);

    [HttpGet("danh-sach")]
    public Task<List<MucKe>> DanhSach(CancellationToken ct) => service.DanhSachKeAsync(ct);
}
