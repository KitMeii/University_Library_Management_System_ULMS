using BaseCore.Common.HeThong;
using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.KhoSachService.DuLieu;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.KhoSachService.NghiepVu;

public sealed record TheLoaiCong(string MaTheLoai, string TenTheLoai, string? BieuTuong, string? MoTa, int SoDauSach);
public sealed record KetQuaKhamPha(int TongSo, int Trang, int CoTrang, IReadOnlyList<SachTrenKe> Sach);
public sealed record TuKhoaPhoBien(string TuKhoa, int SoDauSach);
public sealed record SoLieuThuVien(int SoDauSach, int SoBanSach, int SoBanDoc, int LuotMuonThangNay);

/// <summary>
/// Trang chủ cổng bạn đọc (file 04): khu 4 danh mục thể loại, khu 5 khám phá/tìm kiếm sách, gợi ý từ khóa của thanh tìm kiếm Hero,
/// khu 7 “Thư viện bằng số”. Chỉ đầu sách có ít nhất 1 bản sách; số bản có thể mượn tính như kệ sách (BanCoTheMuon).
/// Tìm không phân biệt hoa thường và dấu (collation Latin1_General_100_CI_AI, đ/Đ quy về d/D ở cả hai phía – “lap trinh” khớp
/// “Lập trình”, “dien” khớp “điện”) trên tên sách, tác giả, ISBN, từ khóa;
/// nhiều từ thì mỗi từ phải khớp ít nhất một trường.
/// </summary>
public sealed class KhamPhaService(KhoSachDbContext db, IClock clock, BoNhoDemNgan nhoDem)
{
    public const string CollationTim = "Latin1_General_100_CI_AI";
    public const int SoNgayMuonNhieu = 365;
    public static readonly string[] SapXepHopLe = ["moi", "ten", "muon-nhieu", "nam"];

    public async Task<KetQuaKhamPha> TimAsync(string? q, string? theLoai, int? namTu, int? namDen, bool conSach, string? sapXep,
        int? trang, int? coTrang, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        sapXep = string.IsNullOrWhiteSpace(sapXep) ? "moi" : sapXep.Trim();
        if (!SapXepHopLe.Contains(sapXep)) kt.Them("sapXep", "Cách sắp xếp không hợp lệ.");
        if (namTu is { } a && namDen is { } b && a > b) kt.Them("namTu", "Năm bắt đầu phải nhỏ hơn hoặc bằng năm kết thúc.");
        q = (q ?? string.Empty).Trim();
        if (q.Length > 100) kt.Them("q", "Từ khóa tìm kiếm tối đa 100 ký tự.");
        kt.NemNeuCoLoi();
        var t = Math.Max(1, trang ?? 1);
        var n = Math.Clamp(coTrang ?? 12, 1, 48);

        // Số bản có thể mượn và lượt mượn 365 ngày theo đầu sách: BẢN ĐỒ dùng chung (hai truy vấn gộp, bộ nhớ đệm ngắn 60 giây như
        // dữ liệu kệ sách – file 11 mục 3). SQL chỉ còn lọc chữ/thể loại/năm; “chỉ còn sách” và “mượn nhiều” xử lý trên bản đồ
        // (Pha 8 hiệu năng: 50.000 bản sách, 200.000 dòng mượn – trước đây tính lại anti-join trong cả truy vấn đếm lẫn truy vấn trang).
        var banDo = await BanDoDauSachAsync(ct);
        var goc = from d in db.DauSach.AsNoTracking()
                  where db.DauSachBanSach.Any(x => x.MaDauSach == d.MaDauSach)
                  join tl in db.TheLoai on d.MaTheLoai equals tl.MaTheLoai into g
                  from tl in g.DefaultIfEmpty()
                  select new
                  {
                      d.MaDauSach, d.TenSach, d.TacGia, d.NamXb, d.MaTheLoai, TenTheLoai = tl.TenTheLoai, d.AnhBia, d.MauGay, d.NgayTao,
                      d.Isbn, d.TuKhoa
                  };

        foreach (var tuDon in q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(6))
        {
            var mau = "%" + ThoatLike(tuDon.Replace('đ', 'd').Replace('Đ', 'D')) + "%";
            goc = goc.Where(x =>
                EF.Functions.Like(EF.Functions.Collate((x.TenSach).Replace("đ", "d").Replace("Đ", "D"), CollationTim), mau, "\\")
                || EF.Functions.Like(EF.Functions.Collate((x.TacGia ?? "").Replace("đ", "d").Replace("Đ", "D"), CollationTim), mau, "\\")
                || EF.Functions.Like(EF.Functions.Collate((x.Isbn ?? "").Replace("đ", "d").Replace("Đ", "D"), CollationTim), mau, "\\")
                || EF.Functions.Like(EF.Functions.Collate((x.TuKhoa ?? "").Replace("đ", "d").Replace("Đ", "D"), CollationTim), mau, "\\"));
        }
        if (!string.IsNullOrWhiteSpace(theLoai)) { var ma = theLoai.Trim(); goc = goc.Where(x => x.MaTheLoai == ma); }
        if (namTu is { } nt) goc = goc.Where(x => x.NamXb >= nt);
        if (namDen is { } nd) goc = goc.Where(x => x.NamXb <= nd);

        // “Mượn nhiều”: SQL xếp theo khóa phụ (tên sách, mã) rồi sắp xếp ỔN ĐỊNH theo lượt mượn giảm dần trên bản đồ
        // = ORDER BY lượt DESC, tên, mã như trước
        goc = sapXep switch
        {
            "ten" => goc.OrderBy(x => EF.Functions.Collate(x.TenSach, "Vietnamese_100_CI_AS")).ThenBy(x => x.MaDauSach),
            "muon-nhieu" => goc.OrderBy(x => x.TenSach).ThenBy(x => x.MaDauSach),
            "nam" => goc.OrderByDescending(x => x.NamXb).ThenBy(x => x.TenSach).ThenBy(x => x.MaDauSach),
            _ => goc.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaDauSach)
        };

        int CoThe(string ma) => banDo.TryGetValue(ma, out var v) ? v.CoThe : 0;
        int Luot(string ma) => banDo.TryGetValue(ma, out var v) ? v.Luot : 0;
        SachTrenKe Ra(string ma, string ten, string? tg, int? nam, string? maTl, string? tenTl, string? anh, string? mau) =>
            new(ma, ten, tg, nam, maTl, tenTl, BanCoTheMuon.AnhNho(anh), anh, mau, CoThe(ma), Luot(ma), null);

        // Có từ khóa: quét LIKE không dấu chỉ MỘT lần (lấy mã theo thứ tự rồi phân trang trong bộ nhớ) thay vì hai lần COUNT + trang
        if (!conSach && sapXep != "muon-nhieu" && q.Length == 0)
        {
            // Đường nhanh: lọc, sắp xếp, phân trang hoàn toàn trong SQL
            var tong = await goc.CountAsync(ct);
            var trangDs = await goc.Skip((t - 1) * n).Take(n).ToListAsync(ct);
            return new KetQuaKhamPha(tong, t, n, trangDs.Select(x => Ra(x.MaDauSach, x.TenSach, x.TacGia, x.NamXb, x.MaTheLoai, x.TenTheLoai, x.AnhBia, x.MauGay)).ToList());
        }

        // Cần bản đồ để lọc/sắp xếp: lấy mã các đầu sách khớp (đã theo thứ tự SQL), lọc + sắp xếp ổn định trong bộ nhớ, rồi nạp trang
        var dsKhop = await goc.Select(x => x.MaDauSach).ToListAsync(ct);
        IEnumerable<string> loc = dsKhop;
        if (conSach) loc = loc.Where(m => CoThe(m) > 0);
        if (sapXep == "muon-nhieu") loc = loc.OrderByDescending(Luot);
        var dsMa = loc.ToList();
        var maTrang = dsMa.Skip((t - 1) * n).Take(n).ToList();
        var chiTiet = (await goc.Where(x => maTrang.Contains(x.MaDauSach)).ToListAsync(ct)).ToDictionary(x => x.MaDauSach);
        return new KetQuaKhamPha(dsMa.Count, t, n, maTrang.Select(m => chiTiet[m])
            .Select(x => Ra(x.MaDauSach, x.TenSach, x.TacGia, x.NamXb, x.MaTheLoai, x.TenTheLoai, x.AnhBia, x.MauGay)).ToList());
    }

    /// <summary>Bản đồ mã đầu sách → (số bản có thể mượn, lượt mượn 365 ngày) – bộ nhớ đệm ngắn dùng chung.</summary>
    private Task<Dictionary<string, (int CoThe, int Luot)>> BanDoDauSachAsync(CancellationToken ct) =>
        nhoDem.LayAsync("ban-do-dau-sach", async c =>
        {
            var coThe = await BanCoTheMuon.TheoDauSach(db, clock.BayGio).ToDictionaryAsync(x => x.MaDauSach, x => x.So, c);
            var luot = await BanCoTheMuon.LuotMuonTheoDauSach(db, clock.HomNay.AddDays(-(SoNgayMuonNhieu - 1)))
                .ToDictionaryAsync(x => x.MaDauSach, x => x.So, c);
            return coThe.Keys.Union(luot.Keys)
                .ToDictionary(k => k, k => (coThe.GetValueOrDefault(k), luot.GetValueOrDefault(k)));
        });

    /// <summary>Khu 4: thể loại có ít nhất 1 đầu sách (không hiện ô rỗng – file 04 mục 1).</summary>
    public async Task<List<TheLoaiCong>> TheLoaiAsync(CancellationToken ct)
    {
        var coBan = db.DauSach.Where(d => db.DauSachBanSach.Any(x => x.MaDauSach == d.MaDauSach));
        var ds = await db.TheLoai.AsNoTracking().OrderBy(t => t.ThuTuHienThi).ThenBy(t => t.TenTheLoai)
            .Select(t => new TheLoaiCong(t.MaTheLoai, t.TenTheLoai, t.BieuTuong, t.MoTa, coBan.Count(d => d.MaTheLoai == t.MaTheLoai)))
            .ToListAsync(ct);
        return ds.Where(x => x.SoDauSach > 0).ToList();
    }

    /// <summary>Gợi ý từ khóa dưới thanh tìm kiếm Hero: từ khóa xuất hiện ở nhiều đầu sách nhất (DAUSACH.tukhoa).</summary>
    public async Task<List<TuKhoaPhoBien>> TuKhoaPhoBienAsync(int soLuong, CancellationToken ct)
    {
        var ds = await db.DauSach.AsNoTracking().Where(d => d.TuKhoa != null && db.DauSachBanSach.Any(x => x.MaDauSach == d.MaDauSach))
            .Select(d => d.TuKhoa!).ToListAsync(ct);
        return ds.SelectMany(t => t.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            .Where(t => t.Length >= 3)
            .GroupBy(t => t.ToLowerInvariant())
            .Select(g => new TuKhoaPhoBien(g.First(), g.Count()))
            .OrderByDescending(x => x.SoDauSach).ThenBy(x => x.TuKhoa, StringComparer.Ordinal)
            .Take(Math.Clamp(soLuong, 1, 20)).ToList();
    }

    /// <summary>Khu 7 “Thư viện bằng số”: đầu sách, bản sách (trừ Mất), bạn đọc (thẻ còn hạn), lượt mượn tháng này – truy vấn thật.</summary>
    public async Task<SoLieuThuVien> SoLieuAsync(CancellationToken ct)
    {
        var homNay = clock.HomNay;
        var dauThang = new DateOnly(homNay.Year, homNay.Month, 1);
        return new SoLieuThuVien(
            await db.DauSach.CountAsync(d => db.DauSachBanSach.Any(x => x.MaDauSach == d.MaDauSach), ct),
            await db.Sach.CountAsync(s => s.TinhTrang != BaseCore.Entities.MienGiaTri.TinhTrangSach.Mat, ct),
            await db.BanDoc.CountAsync(b => b.HsdTuNgay <= homNay && homNay <= b.HsdDenNgay, ct),
            await db.CtPhieuMuon.CountAsync(c => c.NgayMuon >= dauThang && c.NgayMuon <= homNay, ct));
    }

    private static string ThoatLike(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
}
