using BaseCore.Common.Auth;
using BaseCore.QuanTriService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.QuanTriService.Controllers;

/// <summary>Trang tổng quan N4 (file 09 mục 3) – nhóm có quyền theo dõi nhật ký hệ thống (04.00.03 / 04.00.08).</summary>
[ApiController]
[Route("api/quan-tri/thong-ke/n4")]
public sealed class ThongKeN4Controller(ThongKeN4Service service) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("QT040003", "QT040008")]
    public Task<TongQuanN4Dto> TongQuan(CancellationToken ct) => service.TongQuanAsync(ct);
}
