using BaseCore.Common.Auth;
using BaseCore.ViPhamService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.ViPhamService.Controllers;

/// <summary>
/// 02.05.01 Tổng hợp tình hình vi phạm mượn trả sách và 02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách (MB07 – SPEC 6.4.6).
/// Quyền SPEC 3.4: TT092–TT094 tính và hiển thị số liệu (N2); TT097 hiển thị chi tiết, TT098 in báo cáo (N2).
/// </summary>
[ApiController]
[Route("api/quan-tri/bao-cao/vi-pham")]
public sealed class BaoCaoViPhamController(BaoCaoViPhamService service) : ControllerBase
{
    /// <summary>btnTongHop (02.05.01) – cũng là dữ liệu trang in MB07 (02.05.02 – TT096, TT097).</summary>
    [HttpGet]
    [YeuCauQuyen("TT094", "TT097")]
    public Task<BaoCaoViPhamDto> TongHop([FromQuery] DateOnly? tuNgay, [FromQuery] DateOnly? denNgay, CancellationToken ct) =>
        service.TongHopAsync(tuNgay, denNgay, ct);

    /// <summary>btnIn (02.05.02): ngày lập tự sinh, ghi nhật ký in báo cáo (TT098).</summary>
    [HttpPost("lap")]
    [YeuCauQuyen("TT098")]
    public Task<KetQuaLapBaoCao> Lap(KyBaoCaoNhap n, CancellationToken ct) => service.LapAsync(n, User.MaNd(), ct);
}

/// <summary>Trang tổng quan N2 (file 09 mục 3) – nhóm có quyền mở form tổng hợp tình hình vi phạm (TT090).</summary>
[ApiController]
[Route("api/quan-tri/thong-ke/n2")]
public sealed class ThongKeN2Controller(ThongKeN2Service service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("TT090")]
    public Task<TongQuanN2Dto> TongQuan(CancellationToken ct) => service.TongQuanAsync(ct);

    [HttpGet("chua-thanh-toan")]
    [YeuCauQuyen("TT090")]
    public Task<List<DongBienBanBaoCao>> ChuaThanhToan(CancellationToken ct) => service.ChuaThanhToanAsync(ct);
}
