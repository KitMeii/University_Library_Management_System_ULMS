using BaseCore.Common.Auth;
using BaseCore.ViPhamService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.ViPhamService.Controllers;

/// <summary>SPEC 6.2.4 Lập Biên bản vi phạm mượn trả sách (02.03.01). Mỗi nút gọi một API ứng với tiến trình SPEC 3.4.</summary>
[ApiController]
[Route("api/quan-tri/bien-ban")]
public sealed class BienBanController(BienBanService service) : ControllerBase
{
    /// <summary>dgvTruongHop – TT075 Tiếp nhận trường hợp vi phạm từ Bộ phận mượn trả sách.</summary>
    [HttpGet("truong-hop")]
    [YeuCauQuyen("TT075")]
    public Task<IReadOnlyList<TruongHopViPhamDto>> TruongHop(CancellationToken ct) => service.TruongHopAsync(ct);

    /// <summary>cboKhoanThu – TT076, TT081.</summary>
    [HttpGet("khoan-thu")]
    [YeuCauQuyen("TT076")]
    public Task<List<KhoanThuDto>> KhoanThu(CancellationToken ct) => service.KhoanThuAsync(ct);

    /// <summary>txtSoPhieuMuon, btnTimPhieuMuon – TT075.</summary>
    [HttpGet("phieu-muon/{soPMuon}")]
    [YeuCauQuyen("TT074")]
    [YeuCauQuyen("TT075")]
    public Task<BienBanDto> TimPhieuMuon(string soPMuon, CancellationToken ct) => service.TimPhieuMuonAsync(soPMuon, ct);

    /// <summary>btnTimBienBan, trang in MB05 – TT080.</summary>
    [HttpGet("{so}")]
    [YeuCauQuyen("TT080")]
    public Task<BienBanDto> ChiTiet(string so, CancellationToken ct) => service.ChiTietAsync(so, ct);

    /// <summary>btnIn – LapBienBan_LuuVaIn: TT077–TT088.</summary>
    [HttpPost]
    [YeuCauQuyen("TT084")]
    public Task<KetQuaLuuBienBan> LuuVaIn(BienBanNhap n, CancellationToken ct) => service.LuuAsync(n, User.MaNd(), User.TenNd(), ct);
}
