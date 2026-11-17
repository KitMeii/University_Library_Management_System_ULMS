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
        var coTheMuon = BanCoTheMuon.Query(db);
        var tu = TuNgayMuonNhieu;
        var goc = from d in db.DauSach.AsNoTracking()
                  where db.DauSachBanSach.Any(x => x.MaDauSach == d.MaDauSach)
                  join t in db.TheLoai on d.MaTheLoai equals t.MaTheLoai into tl
                  from t in tl.DefaultIfEmpty()
                  select new
                  {
                      d.MaDauSach, d.TenSach, d.TacGia, d.NamXb, d.MaTheLoai, t.TenTheLoai, d.AnhBia, d.MauGay, d.NgayTao,
                      SoBanCoTheMuon = db.DauSachBanSach.Count(x => x.MaDauSach == d.MaDauSach && coTheMuon.Contains(x.MaBanSach)),
                      SoLuotMuon = (from cm in db.CtPhieuMuon
                                    join x in db.DauSachBanSach on cm.MaBanSach equals x.MaBanSach
                                    where x.MaDauSach == d.MaDauSach && cm.NgayMuon >= tu
                                    select 1).Count()
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
}
