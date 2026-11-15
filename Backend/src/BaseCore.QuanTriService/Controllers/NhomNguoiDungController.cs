using BaseCore.Common.Auth;
using BaseCore.QuanTriService.DuLieu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.QuanTriService.Controllers;

/// <summary>Danh mục nhóm người dùng (dùng cho cboNhom ở 04.00.01, 04.00.02 – hoàn thiện ở Pha 2).</summary>
[ApiController]
[Route("api/quan-tri/nhom-nguoi-dung")]
public sealed class NhomNguoiDungController(QuanTriDbContext db) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("QT040001", "QT040002")]
    public async Task<IActionResult> DanhSach(CancellationToken ct) =>
        Ok(await db.NhomNguoiDung.AsNoTracking().OrderBy(x => x.MaNhom)
            .Select(x => new { maNhom = x.MaNhom, tenNhom = x.TenNhom, moTa = x.MoTa }).ToListAsync(ct));
}
