using BaseCore.Common.Auth;
using BaseCore.KhoSachService.DuLieu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.KhoSachService.Controllers;

/// <summary>SPEC 6.1.4 Danh mục khu vực lưu trữ – Pha 1 mới có danh sách (đọc); thêm/sửa/xóa ở Pha 2.</summary>
[ApiController]
[Route("api/quan-tri/khu-vuc")]
public sealed class KhuVucController(KhoSachDbContext db) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("KHUVUC_R")]
    public async Task<IActionResult> DanhSach(CancellationToken ct) =>
        Ok(await db.KhuVuc.AsNoTracking().OrderBy(x => x.MaKhuVuc)
            .Select(x => new { maKhuVuc = x.MaKhuVuc, viTri = x.ViTri, loaiKhuVuc = x.LoaiKhuVuc }).ToListAsync(ct));
}
