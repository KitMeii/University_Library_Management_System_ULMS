using BaseCore.Common.HeThong;
using BaseCore.Common.Auth;
using BaseCore.KhoSachService.NghiepVu;
using Microsoft.AspNetCore.Mvc;

namespace BaseCore.KhoSachService.Controllers;

/// <summary>Trang tổng quan N3 (file 09 mục 3) – nhóm có quyền 03.04.01 Tổng hợp số sách theo tình trạng (TT117).</summary>
[ApiController]
[Route("api/quan-tri/thong-ke/n3")]
public sealed class ThongKeN3Controller(ThongKeN3Service service, BoNhoDemNgan nhoDem) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("TT117")]
    public Task<TongQuanN3Dto> TongQuan(CancellationToken ct) => nhoDem.LayAsync("tong-quan-n3", service.TongQuanAsync);
}
