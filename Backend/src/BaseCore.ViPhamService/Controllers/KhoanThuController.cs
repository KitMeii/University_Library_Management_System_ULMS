using BaseCore.Common.Auth;
using BaseCore.ViPhamService.DuLieu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.ViPhamService.Controllers;

/// <summary>SPEC 6.1.2 Danh mục khoản thu – Pha 1 mới có danh sách (đọc); thêm/sửa/xóa ở Pha 2.</summary>
[ApiController]
[Route("api/quan-tri/khoan-thu")]
public sealed class KhoanThuController(ViPhamDbContext db) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("KHOANTHU_R")]
    public async Task<IActionResult> DanhSach(CancellationToken ct) =>
        Ok(await db.KhoanThu.AsNoTracking().OrderBy(x => x.MaKhoanThu)
            .Select(x => new { maKhoanThu = x.MaKhoanThu, noiDungTt = x.NoiDungTt, donGia = x.DonGia }).ToListAsync(ct));
}
