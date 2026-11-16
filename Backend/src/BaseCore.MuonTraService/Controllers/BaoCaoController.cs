using BaseCore.Common.Auth;
using BaseCore.MuonTraService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.MuonTraService.Controllers;

/// <summary>
/// 01.04.02 Tổng hợp tình hình mượn trả sách và 01.04.03 Lập Báo cáo tình hình mượn trả sách (MB06 – SPEC 6.4.5).
/// Quyền SPEC 3.4: TT058 kiểm soát kỳ, TT064–TT068 tính và hiển thị số liệu (N1); TT071 hiển thị chi tiết, TT073 in báo cáo (N1).
/// </summary>
[ApiController]
[Route("api/quan-tri/bao-cao/muon-tra")]
public sealed class BaoCaoMuonTraController(BaoCaoMuonTraService service) : ControllerBase
{
    /// <summary>btnTongHop (01.04.02) – cũng là dữ liệu trang in MB06 (01.04.03 – TT070, TT071).</summary>
    [HttpGet]
    [YeuCauQuyen("TT068", "TT071")]
    public Task<BaoCaoMuonTraDto> TongHop([FromQuery] DateOnly? tuNgay, [FromQuery] DateOnly? denNgay, CancellationToken ct) =>
        service.TongHopAsync(tuNgay, denNgay, ct);

    /// <summary>btnIn (01.04.03): ngày lập báo cáo tự sinh (TT072), ghi nhật ký in báo cáo (TT073).</summary>
    [HttpPost("lap")]
    [YeuCauQuyen("TT073")]
    public Task<KetQuaLapBaoCao> Lap(KyBaoCaoNhap n, CancellationToken ct) => service.LapAsync(n, User.MaNd(), ct);
}

/// <summary>Trang tổng quan N1 (file 09 mục 3) – chỉ nhóm có quyền mở form tổng hợp tình hình mượn trả sách (TT057).</summary>
[ApiController]
[Route("api/quan-tri/thong-ke/n1")]
public sealed class ThongKeN1Controller(ThongKeN1Service service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("TT057")]
    public Task<TongQuanN1Dto> TongQuan(CancellationToken ct) => service.TongQuanAsync(ct);

    /// <summary>Danh sách chi tiết khi bấm thẻ KPI: loai = dang-muon | qua-han.</summary>
    [HttpGet("sach")]
    [YeuCauQuyen("TT057")]
    public Task<List<DongSachChuaTra>> Sach([FromQuery] string? loai, CancellationToken ct) => service.SachAsync(loai == "qua-han", ct);
}
