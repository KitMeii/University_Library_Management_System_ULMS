using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.KhoSachService.DuLieu;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.KhoSachService.NghiepVu;

/// <summary>Một kệ có thể chọn ở nút lên/xuống (file 05 mục 1).</summary>
public sealed record MucKe(string Ma, string Ten, int SoSach);

/// <summary>Một đầu sách trên kệ coverflow (file 05 mục 2). <see cref="Hang"/> chỉ có ở kệ “Mượn nhiều nhất”.</summary>
public sealed record SachTrenKe(string MaDauSach, string TenSach, string? TacGia, int? NamXb, string? MaTheLoai, string? TenTheLoai,
    string? AnhBiaNho, string? AnhBiaLon, string? MauGay, int SoBanCoTheMuon, int SoLuotMuon, int? Hang);

/// <summary>Trục thanh thời gian: “nam” (năm xuất bản) hoặc “hang” (thứ hạng – kệ Mượn nhiều nhất).</summary>
public sealed record KeSachDto(string Ma, string Ten, string Truc, IReadOnlyList<SachTrenKe> Sach);

/// <summary>
/// Kệ sách nổi bật ở cổng bạn đọc (file 04 khu 3, file 05): “Sách mới về” (đầu sách thêm gần nhất), “Mượn nhiều nhất”
/// (đếm thật CT_PHIEUMUON 90 ngày qua DAUSACH_BANSACH) và mỗi thể loại. Chỉ đầu sách có ít nhất 1 bản sách.
/// Kệ năm sắp theo năm xuất bản (thanh thời gian), kệ Mượn nhiều sắp theo thứ hạng.
/// </summary>
public sealed class KeSachService(KhoSachDbContext db, IClock clock)
{
    public const int SoNgayMuonNhieu = 90;
    public const int GioiHanMacDinh = 41;
    public const string KeMoi = "moi", KeMuonNhieu = "muon-nhieu", TienToTheLoai = "the-loai:";

    private DateOnly TuNgayMuonNhieu => clock.HomNay.AddDays(-(SoNgayMuonNhieu - 1));

    /// <summary>Danh sách kệ có dữ liệu – không hiện kệ rỗng (file 04 mục 1).</summary>
    public async Task<List<MucKe>> DanhSachKeAsync(CancellationToken ct)
    {
        var coBan = db.DauSach.Where(d => db.DauSachBanSach.Any(x => x.MaDauSach == d.MaDauSach));
        var ds = new List<MucKe>();
        var soMoi = await coBan.CountAsync(ct);
        if (soMoi > 0) ds.Add(new MucKe(KeMoi, "Sách mới về", Math.Min(soMoi, GioiHanMacDinh)));
        var tu = TuNgayMuonNhieu;
        var soMuon = await (from cm in db.CtPhieuMuon
                            join x in db.DauSachBanSach on cm.MaBanSach equals x.MaBanSach
                            where cm.NgayMuon >= tu
                            select x.MaDauSach).Distinct().CountAsync(ct);
        if (soMuon > 0) ds.Add(new MucKe(KeMuonNhieu, "Mượn nhiều nhất", Math.Min(soMuon, GioiHanMacDinh)));
        var theLoai = await db.TheLoai.AsNoTracking().OrderBy(t => t.ThuTuHienThi).ThenBy(t => t.TenTheLoai)
            .Select(t => new { t.MaTheLoai, t.TenTheLoai, So = coBan.Count(d => d.MaTheLoai == t.MaTheLoai) }).ToListAsync(ct);
        ds.AddRange(theLoai.Where(t => t.So > 0).Select(t => new MucKe(TienToTheLoai + t.MaTheLoai, t.TenTheLoai, Math.Min(t.So, GioiHanMacDinh))));
        return ds;
    }

    public async Task<KeSachDto> KeAsync(string? ke, int? gioiHan, CancellationToken ct)
    {
        var n = Math.Clamp(gioiHan ?? GioiHanMacDinh, 1, 81);
        ke = (ke ?? KeMoi).Trim();
        var tu = TuNgayMuonNhieu;
        // Hai truy vấn gộp nối trái một lần thay cho đếm tương quan từng đầu sách (Pha 8 hiệu năng)
        var coThe = BanCoTheMuon.TheoDauSach(db, clock.BayGio);
        var luot = BanCoTheMuon.LuotMuonTheoDauSach(db, tu);
        var goc = from d in db.DauSach.AsNoTracking()
                  where db.DauSachBanSach.Any(x => x.MaDauSach == d.MaDauSach)
                  join t in db.TheLoai on d.MaTheLoai equals t.MaTheLoai into tl
                  from t in tl.DefaultIfEmpty()
                  join c in coThe on d.MaDauSach equals c.MaDauSach into gc
                  from c in gc.DefaultIfEmpty()
                  join l in luot on d.MaDauSach equals l.MaDauSach into gl
                  from l in gl.DefaultIfEmpty()
                  select new
                  {
                      d.MaDauSach, d.TenSach, d.TacGia, d.NamXb, d.MaTheLoai, t.TenTheLoai, d.AnhBia, d.MauGay, d.NgayTao,
                      SoBanCoTheMuon = (int?)c.So ?? 0,
                      SoLuotMuon = (int?)l.So ?? 0
                  };

        string ten, truc = "nam";
        if (ke == KeMoi)
        {
            ten = "Sách mới về";
            goc = goc.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaDauSach).Take(n);
        }
        else if (ke == KeMuonNhieu)
        {
            ten = "Mượn nhiều nhất";
            truc = "hang";
            goc = goc.Where(x => x.SoLuotMuon > 0).OrderByDescending(x => x.SoLuotMuon).ThenBy(x => x.TenSach).Take(n);
        }
        else if (ke.StartsWith(TienToTheLoai, StringComparison.Ordinal) && ke.Length > TienToTheLoai.Length)
        {
            var ma = ke[TienToTheLoai.Length..];
            ten = await db.TheLoai.Where(t => t.MaTheLoai == ma).Select(t => t.TenTheLoai).SingleOrDefaultAsync(ct)
                  ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_THE_LOAI", $"Không có thể loại {ma}.");
            goc = goc.Where(x => x.MaTheLoai == ma).OrderByDescending(x => x.NgayTao).Take(n);
        }
        else
        {
            new KiemTraDuLieu().Them("ke", "Kệ không hợp lệ (moi, muon-nhieu hoặc the-loai:{mã}).").NemNeuCoLoi();
            throw new InvalidOperationException();
        }

        var ds = await goc.ToListAsync(ct);
        var sach = ds.Select((x, i) => new SachTrenKe(x.MaDauSach, x.TenSach, x.TacGia, x.NamXb, x.MaTheLoai, x.TenTheLoai,
            BanCoTheMuon.AnhNho(x.AnhBia), x.AnhBia, x.MauGay, x.SoBanCoTheMuon, x.SoLuotMuon, truc == "hang" ? i + 1 : null));
        // Kệ năm: sắp theo năm xuất bản để thanh thời gian chạy từ trái sang phải (file 05 mục 2)
        if (truc == "nam") sach = sach.OrderBy(x => x.NamXb ?? int.MaxValue).ThenBy(x => x.TenSach, StringComparer.Ordinal);
        return new KeSachDto(ke, ten, truc, sach.ToList());
    }

    /// <summary>
    /// Dữ liệu Book Preview (file 06 mục 1): thông tin đầu sách + số bản có thể mượn + khu vực của các bản (Kệ mở/Kho).
    /// Phần “đặt chỗ của tôi” lấy ở muontra (GET /api/cong/dat-cho/dau-sach/{ma}) – GIA_DINH D62.
    /// </summary>
    public async Task<SachPreviewDto> SachAsync(string? maDauSach, CancellationToken ct)
    {
        var ma = (maDauSach ?? string.Empty).Trim().ToUpperInvariant();
        var d = await (from x in db.DauSach.AsNoTracking()
                       join t in db.TheLoai on x.MaTheLoai equals t.MaTheLoai into tl
                       from t in tl.DefaultIfEmpty()
                       where x.MaDauSach == ma
                       select new { x, t.TenTheLoai }).SingleOrDefaultAsync(ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_DAU_SACH", "Không tìm thấy sách.");
        var coTheMuon = BanCoTheMuon.Query(db, clock.BayGio);
        var ban = await (from b in db.DauSachBanSach
                         join s in db.Sach on b.MaBanSach equals s.MaBanSach
                         join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
                         where b.MaDauSach == ma
                         select new { kv.LoaiKhuVuc, kv.ViTri, CoThe = coTheMuon.Contains(s.MaBanSach) }).ToListAsync(ct);
        var khuVuc = ban.Where(x => x.CoThe).GroupBy(x => new { x.LoaiKhuVuc, x.ViTri })
            .Select(g => new KhuVucCoSach(g.Key.LoaiKhuVuc, g.Key.ViTri, g.Count()))
            .OrderBy(k => k.LoaiKhuVuc == BaseCore.Entities.MienGiaTri.LoaiKhuVuc.KeMo ? 0 : 1).ThenBy(k => k.ViTri).ToList();
        var tuKhoa = (d.x.TuKhoa ?? string.Empty).Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).Take(12).ToList();
        return new SachPreviewDto(d.x.MaDauSach, d.x.TenSach, d.x.TacGia, d.x.NhaXuatBan, d.x.NamXb, d.x.Isbn, d.x.MaTheLoai, d.TenTheLoai,
            d.x.GioiThieu, tuKhoa, d.x.AnhBia, BanCoTheMuon.AnhNho(d.x.AnhBia), d.x.MauGay, ban.Count, ban.Count(x => x.CoThe), khuVuc);
    }
}

public sealed record KhuVucCoSach(string LoaiKhuVuc, string ViTri, int SoBan);

public sealed record SachPreviewDto(string MaDauSach, string TenSach, string? TacGia, string? NhaXuatBan, int? NamXb, string? Isbn,
    string? MaTheLoai, string? TenTheLoai, string? GioiThieu, IReadOnlyList<string> TuKhoa, string? AnhBiaLon, string? AnhBiaNho,
    string? MauGay, int TongSoBan, int SoBanCoTheMuon, IReadOnlyList<KhuVucCoSach> KhuVuc);
