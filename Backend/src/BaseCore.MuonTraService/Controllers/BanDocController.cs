using BaseCore.Common.Auth;
using BaseCore.MuonTraService.DuLieu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.MuonTraService.Controllers;

/// <summary>SPEC 6.1.3 Danh mục Bạn đọc – Pha 1 mới có danh sách (đọc); 01.01.01 đầy đủ ở Pha 2.</summary>
[ApiController]
[Route("api/quan-tri/ban-doc")]
public sealed class BanDocController(MuonTraDbContext db) : ControllerBase
{
    [HttpGet]
    [YeuCauQuyen("BANDOC_R")]
    public async Task<IActionResult> DanhSach([FromQuery] int trang = 1, [FromQuery] int coTrang = 20, CancellationToken ct = default)
    {
        coTrang = Math.Clamp(coTrang, 1, 100);
        var q = db.BanDoc.AsNoTracking().OrderBy(x => x.MaBanDoc);
        var tong = await q.CountAsync(ct);
        var ds = await q.Skip((Math.Max(trang, 1) - 1) * coTrang).Take(coTrang)
            .Select(x => new { maBanDoc = x.MaBanDoc, hoTen = x.HoTen, doiTuong = x.DoiTuong, maSvHvCb = x.MaSvHvCb, hsdDenNgay = x.HsdDenNgay })
            .ToListAsync(ct);
        return Ok(new { tongSo = tong, trang, coTrang, duLieu = ds });
    }
}
