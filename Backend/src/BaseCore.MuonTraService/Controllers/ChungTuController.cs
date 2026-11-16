using BaseCore.Common.Auth;
using BaseCore.MuonTraService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.MuonTraService.Controllers;

/// <summary>SPEC 6.2.1 Lập Phiếu mượn sách (01.01.03). Mỗi nút gọi một API ứng với tiến trình SPEC 3.4.</summary>
[ApiController]
[Route("api/quan-tri/phieu-muon")]
public sealed class PhieuMuonController(PhieuMuonService service) : ControllerBase
{
    /// <summary>txtMaSV, btnTimBanDoc – TT013 (kèm TT003, TT007–TT011).</summary>
    [HttpGet("tim-ban-doc")]
    [YeuCauQuyen("TT012")]
    [YeuCauQuyen("TT013")]
    public Task<KetQuaTimBanDocMuon> TimBanDoc([FromQuery] string? maSv, CancellationToken ct) => service.TimBanDocAsync(maSv, ct);

    /// <summary>txtMaBanSach, btnThemSach – TT014, TT015, TT021.</summary>
    [HttpGet("ban-sach")]
    [YeuCauQuyen("TT012")]
    [YeuCauQuyen("TT014")]
    public Task<KetQuaKiemTraBanSach> BanSach([FromQuery] string? maBanSach, [FromQuery] string? maBanDoc, CancellationToken ct) =>
        service.KiemTraBanSachAsync(maBanSach, maBanDoc, ct);

    /// <summary>btnTimPhieu, trang in MB02 – TT022.</summary>
    [HttpGet("{so}")]
    [YeuCauQuyen("TT022")]
    public Task<PhieuMuonDto> ChiTiet(string so, CancellationToken ct) => service.ChiTietAsync(so, ct);

    /// <summary>btnIn – LapPhieuMuon_LuuVaIn: TT020, TT023–TT027.</summary>
    [HttpPost]
    [YeuCauQuyen("TT025")]
    public Task<KetQuaLuuChungTu> LuuVaIn(PhieuMuonNhap n, CancellationToken ct) => service.LuuAsync(n, User.MaNd(), User.TenNd(), ct);
}

/// <summary>SPEC 6.2.2 Lập Phiếu gia hạn sách (01.02.01).</summary>
[ApiController]
[Route("api/quan-tri/phieu-gia-han")]
public sealed class PhieuGiaHanController(PhieuGiaHanService service) : ControllerBase
{
    /// <summary>txtSoPhieuMuon, btnTimPhieuMuon – TT031, TT032.</summary>
    [HttpGet("phieu-muon/{soPMuon}")]
    [YeuCauQuyen("TT030")]
    [YeuCauQuyen("TT032")]
    public Task<PhieuGiaHanDto> TimPhieuMuon(string soPMuon, CancellationToken ct) => service.TimPhieuMuonAsync(soPMuon, ct);

    /// <summary>dgvChiTiet (chọn bản sách, ngay_denghi) – TT033–TT037.</summary>
    [HttpPost("kiem-tra")]
    [YeuCauQuyen("TT035")]
    public Task<IReadOnlyList<DongGiaHanDto>> KiemTra(PhieuGiaHanNhap n, CancellationToken ct) => service.KiemTraAsync(n, ct);

    /// <summary>btnTimPhieu, trang in MB03 – TT038.</summary>
    [HttpGet("{so}")]
    [YeuCauQuyen("TT038")]
    public Task<PhieuGiaHanDto> ChiTiet(string so, CancellationToken ct) => service.ChiTietAsync(so, ct);

    /// <summary>btnIn – LapPhieuGiaHan_LuuVaIn: TT039–TT041.</summary>
    [HttpPost]
    [YeuCauQuyen("TT040")]
    public Task<KetQuaLuuChungTu> LuuVaIn(PhieuGiaHanNhap n, CancellationToken ct) => service.LuuAsync(n, User.MaNd(), User.TenNd(), ct);
}

/// <summary>SPEC 6.2.3 Lập Phiếu trả sách (01.02.02).</summary>
[ApiController]
[Route("api/quan-tri/phieu-tra")]
public sealed class PhieuTraController(PhieuTraService service) : ControllerBase
{
    /// <summary>txtSoPhieuMuon, btnTimPhieuMuon – TT032 (+ TT044 Phiếu trả đã lập).</summary>
    [HttpGet("phieu-muon/{soPMuon}")]
    [YeuCauQuyen("TT043")]
    [YeuCauQuyen("TT032")]
    public Task<PhieuTraDto> TimPhieuMuon(string soPMuon, CancellationToken ct) => service.TimPhieuMuonAsync(soPMuon, ct);

    /// <summary>btnTimPhieu, trang in MB04 – TT044, TT048.</summary>
    [HttpGet("{so}")]
    [YeuCauQuyen("TT044", "TT048")]
    public Task<PhieuTraDto> ChiTiet(string so, CancellationToken ct) => service.ChiTietAsync(so, ct);

    /// <summary>btnIn – LapPhieuTra_LuuVaIn: TT045–TT055.</summary>
    [HttpPost]
    [YeuCauQuyen("TT051")]
    public Task<KetQuaLuuChungTu> LuuVaIn(PhieuTraNhap n, CancellationToken ct) => service.LuuAsync(n, User.MaNd(), User.TenNd(), ct);
}
