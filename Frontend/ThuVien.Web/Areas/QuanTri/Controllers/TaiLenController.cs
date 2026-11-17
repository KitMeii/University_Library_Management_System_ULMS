using BaseCore.Common.Auth;
using BaseCore.Common.Loi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using ThuVien.Web.HaTang;

namespace ThuVien.Web.Areas.QuanTri.Controllers;

/// <summary>
/// Tải ảnh thẻ Bạn đọc (BANDOC.anh – đường dẫn tệp ảnh, C(100)). File 11: kiểm tra loại tệp thật (chữ ký byte),
/// giới hạn 2 MB, tên tệp ngẫu nhiên; lưu ở volume thuvien-uploads.
/// GIA_DINH D35: giải mã bằng SkiaSharp, xoay theo EXIF, thu nhỏ vừa khung 600×800 rồi lưu WebP (bỏ siêu dữ liệu EXIF/GPS).
/// </summary>
[Area("QuanTri")]
[Authorize(Policy = ChinhSach.NhanVien)]
public sealed class TaiLenController(IWebHostEnvironment env) : Controller
{
    private const long ToiDa = 2 * 1024 * 1024;
    private const int RongToiDa = 600, CaoToiDa = 800, ChatLuong = 82;
    /// <summary>Chặn “bom giải nén”: ảnh nhỏ về dung lượng nhưng kích thước điểm ảnh khổng lồ.</summary>
    private const long DiemAnhToiDa = 40_000_000;

    [HttpPost("quan-tri/tai-len/anh-ban-doc")]
    [QuyenTrang("BANDOC_C", "BANDOC_E")]
    [RequestSizeLimit(ToiDa + 64 * 1024)]
    public async Task<IActionResult> AnhBanDoc(IFormFile? tep, CancellationToken ct)
    {
        if (tep is null || tep.Length == 0)
            return BadRequest(new LoiApi("THIEU_TEP", "Vui lòng chọn tệp ảnh."));
        if (tep.Length > ToiDa)
            return BadRequest(new LoiApi("TEP_QUA_LON", "Ảnh tối đa 2 MB."));

        byte[] du;
        using (var ms = new MemoryStream((int)tep.Length))
        {
            await tep.CopyToAsync(ms, ct);
            du = ms.ToArray();
        }
        if (!LaAnhHopLe(du))
            return BadRequest(new LoiApi("SAI_LOAI_TEP", "Chỉ nhận ảnh JPG, PNG hoặc WebP."));

        var webp = ChuyenWebP(du);
        if (webp is null)
            return BadRequest(new LoiApi("ANH_HONG", "Không đọc được ảnh (tệp hỏng hoặc kích thước quá lớn)."));

        var thuMuc = Path.Combine(env.WebRootPath, "uploads", "ban-doc");
        Directory.CreateDirectory(thuMuc);
        var ten = Guid.NewGuid().ToString("N") + ".webp";
        await System.IO.File.WriteAllBytesAsync(Path.Combine(thuMuc, ten), webp, ct);
        return Ok(new { duongDan = "/uploads/ban-doc/" + ten }); // 50 ký tự ≤ C(100)
    }

    /// <summary>
    /// Ảnh bìa đầu sách (DAUSACH.anhbia – mở rộng): cùng kiểm tra như ảnh thẻ; lưu 2 bản WebP – lớn 600×900 (cuốn giữa kệ,
    /// Book Preview) và nhỏ 240×360 “_nho” (gáy/cuốn ngoài tâm, lưới sách – file 05 mục 7).
    /// </summary>
    [HttpPost("quan-tri/tai-len/anh-bia")]
    [QuyenTrang("DAUSACH_C", "DAUSACH_E")]
    [RequestSizeLimit(ToiDa + 64 * 1024)]
    public async Task<IActionResult> AnhBia(IFormFile? tep, CancellationToken ct)
    {
        if (tep is null || tep.Length == 0) return BadRequest(new LoiApi("THIEU_TEP", "Vui lòng chọn tệp ảnh."));
        if (tep.Length > ToiDa) return BadRequest(new LoiApi("TEP_QUA_LON", "Ảnh tối đa 2 MB."));
        byte[] du;
        using (var ms = new MemoryStream((int)tep.Length))
        {
            await tep.CopyToAsync(ms, ct);
            du = ms.ToArray();
        }
        if (!LaAnhHopLe(du)) return BadRequest(new LoiApi("SAI_LOAI_TEP", "Chỉ nhận ảnh JPG, PNG hoặc WebP."));
        var lon = ChuyenWebP(du, 600, 900);
        var nho = ChuyenWebP(du, 240, 360);
        if (lon is null || nho is null) return BadRequest(new LoiApi("ANH_HONG", "Không đọc được ảnh (tệp hỏng hoặc kích thước quá lớn)."));

        var thuMuc = Path.Combine(env.WebRootPath, "uploads", "bia");
        Directory.CreateDirectory(thuMuc);
        var id = Guid.NewGuid().ToString("N");
        await System.IO.File.WriteAllBytesAsync(Path.Combine(thuMuc, id + ".webp"), lon, ct);
        await System.IO.File.WriteAllBytesAsync(Path.Combine(thuMuc, id + "_nho.webp"), nho, ct);
        return Ok(new { duongDan = $"/uploads/bia/{id}.webp", duongDanNho = $"/uploads/bia/{id}_nho.webp" });
    }

    private static bool LaAnhHopLe(byte[] b) =>
        b is [0xFF, 0xD8, 0xFF, ..]
        || b is [0x89, 0x50, 0x4E, 0x47, ..]
        || b is [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x45, 0x42, 0x50, ..];

    /// <summary>Giải mã → xoay theo EXIF → thu nhỏ (giữ tỉ lệ, không phóng to) → WebP. Trả null nếu ảnh không giải mã được.</summary>
    public static byte[]? ChuyenWebP(byte[] du) => ChuyenWebP(du, RongToiDa, CaoToiDa);

    public static byte[]? ChuyenWebP(byte[] du, int rongToiDa, int caoToiDa)
    {
        using var data = SKData.CreateCopy(du);
        using var codec = SKCodec.Create(data);
        if (codec is null || (long)codec.Info.Width * codec.Info.Height > DiemAnhToiDa) return null;
        using var goc = SKBitmap.Decode(codec);
        if (goc is null) return null;
        using var thang = XoayTheoExif(goc, codec.EncodedOrigin);
        var src = thang ?? goc;

        var tiLe = Math.Min(1d, Math.Min((double)rongToiDa / src.Width, (double)caoToiDa / src.Height));
        var rong = Math.Max(1, (int)Math.Round(src.Width * tiLe));
        var cao = Math.Max(1, (int)Math.Round(src.Height * tiLe));
        using var nho = tiLe < 1
            ? src.Resize(new SKImageInfo(rong, cao, SKColorType.Rgba8888, SKAlphaType.Premul), new SKSamplingOptions(SKCubicResampler.Mitchell))
            : null;
        using var anh = SKImage.FromBitmap(nho ?? src);
        using var kq = anh.Encode(SKEncodedImageFormat.Webp, ChatLuong);
        return kq?.ToArray();
    }

    private static SKBitmap? XoayTheoExif(SKBitmap b, SKEncodedOrigin goc)
    {
        var (doXoay, doiChieu) = goc switch
        {
            SKEncodedOrigin.BottomRight => (180, false),
            SKEncodedOrigin.RightTop => (90, true),
            SKEncodedOrigin.LeftBottom => (270, true),
            _ => (0, false)
        };
        if (doXoay == 0) return null;
        var kq = new SKBitmap(doiChieu ? b.Height : b.Width, doiChieu ? b.Width : b.Height);
        using var c = new SKCanvas(kq);
        c.Translate(kq.Width / 2f, kq.Height / 2f);
        c.RotateDegrees(doXoay);
        c.Translate(-b.Width / 2f, -b.Height / 2f);
        c.DrawBitmap(b, 0, 0);
        return kq;
    }
}
