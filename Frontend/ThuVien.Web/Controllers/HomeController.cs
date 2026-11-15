using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ThuVien.Web.Controllers;

/// <summary>Trang chủ cổng bạn đọc. Pha 1: khung header/footer; hero, kệ sách, các khu vực ở Pha 5–7.</summary>
[AllowAnonymous]
public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();

    [HttpGet("/loi/{ma:int?}")]
    [IgnoreAntiforgeryToken]
    public IActionResult Loi(int? ma)
    {
        var goc = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath ?? string.Empty;
        ViewData["MaLoi"] = ma ?? 500;
        ViewData["LaQuanTri"] = goc.StartsWith("/quan-tri", StringComparison.OrdinalIgnoreCase);
        return View();
    }
}
