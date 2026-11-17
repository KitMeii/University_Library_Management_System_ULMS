using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ThuVien.Web.HaTang;
using ThuVien.Web.Models;

namespace ThuVien.Web.Controllers;

/// <summary>
/// Trang chủ cổng bạn đọc (file 04). Pha 5: header, hero, kệ sách nổi bật (coverflow – file 05), footer;
/// các khu danh mục, khám phá, hướng dẫn, số liệu ở Pha 7.
/// </summary>
[AllowAnonymous]
public sealed class HomeController(GatewayClient gateway, IWebHostEnvironment env) : Controller
{
    private static readonly string[] DuoiAnh = [".svg", ".webp", ".jpg", ".jpeg", ".png", ".avif"];

    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Kệ đầu tiên render phía server: không có JS vẫn xem được (file 05 mục 6), JS nhận dữ liệu không cần gọi lại API
        var dsKe = await gateway.LayCongKhaiAsync<List<MucKeCong>>("/api/cong/ke-sach/danh-sach", ct);
        var danhSach = dsKe.DuLieu ?? [];
        KeSachCong? ke = null;
        if (danhSach.Count > 0)
            ke = (await gateway.LayCongKhaiAsync<KeSachCong>("/api/cong/ke-sach?ke=" + Uri.EscapeDataString(danhSach[0].Ma), ct)).DuLieu;
        return View(new TrangChuVm(AnhHero(), danhSach, ke is { Sach.Count: > 0 } ? ke : null));
    }

    /// <summary>Ảnh Hero: mọi tệp ảnh trong wwwroot/img/hero theo thứ tự tên (GIA_DINH D56 – thay ảnh thật không sửa code).</summary>
    private List<string> AnhHero()
    {
        var thuMuc = Path.Combine(env.WebRootPath, "img", "hero");
        if (!Directory.Exists(thuMuc)) return [];
        return Directory.EnumerateFiles(thuMuc)
            .Where(f => DuoiAnh.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .Order(StringComparer.Ordinal)
            .Select(f => "/img/hero/" + Path.GetFileName(f))
            .ToList();
    }

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
