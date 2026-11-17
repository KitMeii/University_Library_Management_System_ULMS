using System.Text.RegularExpressions;
using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.DTO.Common;
using BaseCore.Entities;
using BaseCore.Entities.KhoSach;
using BaseCore.KhoSachService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.KhoSachService.NghiepVu;

public sealed record DauSachDto
{
    public string MaDauSach { get; init; } = null!;
    public string TenSach { get; init; } = null!;
    public string? TacGia { get; init; }
    public string? NhaXuatBan { get; init; }
    public int? NamXb { get; init; }
    public string? Isbn { get; init; }
    public string? MaTheLoai { get; init; }
    public string? TenTheLoai { get; init; }
    public string? GioiThieu { get; init; }
    public string? TuKhoa { get; init; }
    public string? AnhBia { get; init; }
    public string? AnhBiaNho { get; init; }
    public string? MauGay { get; init; }
    public DateTime NgayTao { get; init; }
    public int SoBanSach { get; init; }
    public int SoBanCoTheMuon { get; init; }
}

public sealed record DauSachNhap(string? MaDauSach, string? TenSach, string? TacGia, string? NhaXuatBan, int? NamXb, string? Isbn,
    string? MaTheLoai, string? GioiThieu, string? TuKhoa, string? AnhBia, string? MauGay);

public sealed record BanSachCuaDauSach(string MaBanSach, string TenSach, string TacGia, string MaKhuVuc, string ViTri, string TinhTrang);
public sealed record GanBanSachNhap(List<string>? MaBanSach);

/// <summary>
/// Quản lý đầu sách (X02 DAUSACH – mở rộng, file 02) và gắn bản sách vào đầu sách (X03 DAUSACH_BANSACH – GIA_DINH D55).
/// Quyền mở rộng DAUSACH_C/E/R (N3, N4), DAUSACH_D (N4). Đầu sách hiển thị ở cổng bạn đọc (kệ sách, khám phá).
/// </summary>
public sealed partial class DauSachService(KhoSachDbContext db, IClock clock)
{
    private static readonly string[] SapXep =
        [nameof(DauSachDto.MaDauSach), nameof(DauSachDto.TenSach), nameof(DauSachDto.TacGia), nameof(DauSachDto.NamXb), nameof(DauSachDto.NgayTao)];

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")] private static partial Regex MauHopLe();
    [GeneratedRegex(@"^[0-9Xx\-]{10,20}$")] private static partial Regex IsbnHopLe();
    [GeneratedRegex("^/uploads/bia/[0-9a-f]{32}\\.webp$")] private static partial Regex AnhBiaHopLe();

    private IQueryable<DauSachDto> Query()
    {
        var coTheMuon = BanCoTheMuon.Query(db, clock.BayGio);
        return from d in db.DauSach.AsNoTracking()
               join t in db.TheLoai on d.MaTheLoai equals t.MaTheLoai into tl
               from t in tl.DefaultIfEmpty()
               select new DauSachDto
               {
                   MaDauSach = d.MaDauSach, TenSach = d.TenSach, TacGia = d.TacGia, NhaXuatBan = d.NhaXuatBan, NamXb = d.NamXb, Isbn = d.Isbn,
                   MaTheLoai = d.MaTheLoai, TenTheLoai = t.TenTheLoai, GioiThieu = d.GioiThieu, TuKhoa = d.TuKhoa, AnhBia = d.AnhBia,
                   MauGay = d.MauGay, NgayTao = d.NgayTao,
                   SoBanSach = db.DauSachBanSach.Count(x => x.MaDauSach == d.MaDauSach),
                   SoBanCoTheMuon = db.DauSachBanSach.Count(x => x.MaDauSach == d.MaDauSach && coTheMuon.Contains(x.MaBanSach))
               };
    }

    private static DauSachDto VoiAnhNho(DauSachDto d) => d with { AnhBiaNho = BanCoTheMuon.AnhNho(d.AnhBia) };

    public async Task<KetQuaTrang<DauSachDto>> DanhSachAsync(ThamSoLuoi ts, string? maTheLoai, CancellationToken ct)
    {
        var q = Query();
        if (!string.IsNullOrWhiteSpace(maTheLoai)) q = q.Where(x => x.MaTheLoai == maTheLoai);
        var k = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (k is not null)
            q = q.Where(x => x.MaDauSach.Contains(k) || x.TenSach.Contains(k) || (x.TacGia != null && x.TacGia.Contains(k))
                             || (x.Isbn != null && x.Isbn.Contains(k)) || (x.TuKhoa != null && x.TuKhoa.Contains(k)));
        var kq = await q.LayTrangAsync(ts, SapXep, nameof(DauSachDto.MaDauSach), x => x, ct);
        return kq with { DuLieu = kq.DuLieu.Select(VoiAnhNho).ToList() };
    }

    public async Task<DauSachDto> MotAsync(string ma, CancellationToken ct) =>
        VoiAnhNho(await Query().SingleOrDefaultAsync(x => x.MaDauSach == ma, ct)
                  ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy đầu sách {ma}."));

    /// <summary>Gợi ý mã đầu sách kế tiếp DS#### khi bấm Thêm.</summary>
    public async Task<string> MaKeTiepAsync(CancellationToken ct)
    {
        var ds = await db.DauSach.AsNoTracking().Where(x => x.MaDauSach.StartsWith("DS")).Select(x => x.MaDauSach).ToListAsync(ct);
        var max = ds.Select(m => int.TryParse(m[2..], out var so) ? so : 0).DefaultIfEmpty(0).Max();
        return $"DS{max + 1:D4}";
    }

    public async Task<DauSachDto> ThemAsync(DauSachNhap n, string maNd, CancellationToken ct)
    {
        var kt = await KiemTraAsync(n, null, ct);
        if (await db.DauSach.AnyAsync(x => x.MaDauSach == n.MaDauSach, ct)) kt.Them("maDauSach", $"Mã đầu sách {n.MaDauSach} đã tồn tại.");
        kt.NemNeuCoLoi();
        var d = new DauSach { MaDauSach = n.MaDauSach!.Trim().ToUpperInvariant(), NgayTao = clock.BayGio };
        Gan(d, n);
        db.DauSach.Add(d);
        await NhatKyGhi.ThemAsync(db, maNd, $"Thêm đầu sách {d.MaDauSach} – {d.TenSach}", clock.BayGio, ct);
        await LuuAsync(ct);
        return await MotAsync(d.MaDauSach, ct);
    }

    public async Task<DauSachDto> SuaAsync(string ma, DauSachNhap n, string maNd, CancellationToken ct)
    {
        var d = await db.DauSach.SingleOrDefaultAsync(x => x.MaDauSach == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy đầu sách {ma}.");
        (await KiemTraAsync(n, ma, ct)).NemNeuCoLoi();
        var cu = d.TenSach;
        Gan(d, n);
        await NhatKyGhi.ThemAsync(db, maNd, $"Sửa đầu sách {ma}: {cu} → {d.TenSach}", clock.BayGio, ct);
        await LuuAsync(ct);
        return await MotAsync(ma, ct);
    }

    /// <summary>Không xóa đầu sách còn bản sách gắn vào, đã có đặt chỗ hoặc có bạn đọc yêu thích (bảng của service khác).</summary>
    public async Task XoaAsync(string ma, string maNd, CancellationToken ct)
    {
        var d = await db.DauSach.SingleOrDefaultAsync(x => x.MaDauSach == ma, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy đầu sách {ma}.");
        var soBan = await db.DauSachBanSach.CountAsync(x => x.MaDauSach == ma, ct);
        if (soBan > 0) throw LoiNghiepVu.XungDot("DAU_SACH_CON_BAN_SACH", $"Đầu sách {ma} còn {soBan} bản sách gắn vào – bỏ gắn trước khi xóa.");
        if (await db.PhieuDatCho.AnyAsync(x => x.MaDauSach == ma, ct))
            throw LoiNghiepVu.XungDot("DA_CO_DAT_CHO", $"Đầu sách {ma} đã phát sinh đặt chỗ của bạn đọc nên không thể xóa.");
        if (await db.YeuThich.AnyAsync(x => x.MaDauSach == ma, ct))
            throw LoiNghiepVu.XungDot("DA_CO_YEU_THICH", $"Đầu sách {ma} đang có trong danh sách yêu thích của bạn đọc nên không thể xóa.");
        db.DauSach.Remove(d);
        await NhatKyGhi.ThemAsync(db, maNd, $"Xóa đầu sách {ma} – {d.TenSach}", clock.BayGio, ct);
        await LuuAsync(ct);
    }

    // ---------- Bản sách của đầu sách (X03 – D55) ----------
    public async Task<List<BanSachCuaDauSach>> BanSachAsync(string ma, CancellationToken ct)
    {
        if (!await db.DauSach.AnyAsync(x => x.MaDauSach == ma, ct)) throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy đầu sách {ma}.");
        return await (from x in db.DauSachBanSach.AsNoTracking()
                      join s in db.Sach on x.MaBanSach equals s.MaBanSach
                      join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
                      where x.MaDauSach == ma
                      orderby s.MaBanSach
                      select new BanSachCuaDauSach(s.MaBanSach, s.TenSach, s.TacGia, s.MaKhuVuc, kv.ViTri, s.TinhTrang)).ToListAsync(ct);
    }

    /// <summary>Bản sách chưa gắn đầu sách nào (tìm theo mã, tên, tác giả) – tối đa 50 dòng.</summary>
    public Task<List<BanSachCuaDauSach>> BanSachChuaGanAsync(string? tuKhoa, CancellationToken ct)
    {
        var k = KiemTraDuLieu.Gon(tuKhoa);
        var q = from s in db.Sach.AsNoTracking()
                join kv in db.KhuVuc on s.MaKhuVuc equals kv.MaKhuVuc
                where !db.DauSachBanSach.Any(x => x.MaBanSach == s.MaBanSach)
                select new { s, kv.ViTri };
        if (k is not null) q = q.Where(x => x.s.MaBanSach.Contains(k) || x.s.TenSach.Contains(k) || x.s.TacGia.Contains(k));
        return q.OrderBy(x => x.s.MaBanSach).Take(50)
            .Select(x => new BanSachCuaDauSach(x.s.MaBanSach, x.s.TenSach, x.s.TacGia, x.s.MaKhuVuc, x.ViTri, x.s.TinhTrang)).ToListAsync(ct);
    }

    public async Task<int> GanBanSachAsync(string ma, GanBanSachNhap n, string maNd, CancellationToken ct)
    {
        var ds = (n.MaBanSach ?? []).Select(x => x?.Trim().ToUpperInvariant()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        if (ds.Count == 0) new KiemTraDuLieu().Them("maBanSach", "Chọn ít nhất một bản sách.").NemNeuCoLoi();
        if (!await db.DauSach.AnyAsync(x => x.MaDauSach == ma, ct)) throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy đầu sách {ma}.");
        var coSach = await db.Sach.Where(s => ds.Contains(s.MaBanSach)).Select(s => s.MaBanSach).ToListAsync(ct);
        var thieu = ds.Except(coSach).ToList();
        if (thieu.Count > 0) throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BAN_SACH", "Không có bản sách: " + string.Join(", ", thieu));
        var daGan = await db.DauSachBanSach.Where(x => ds.Contains(x.MaBanSach)).Select(x => x.MaBanSach + " (" + x.MaDauSach + ")").ToListAsync(ct);
        if (daGan.Count > 0) throw LoiNghiepVu.XungDot("DA_GAN_DAU_SACH", "Bản sách đã gắn đầu sách khác: " + string.Join(", ", daGan));
        foreach (var m in ds) db.DauSachBanSach.Add(new DauSachBanSach { MaDauSach = ma, MaBanSach = m! });
        await NhatKyGhi.ThemAsync(db, maNd, $"Gắn {ds.Count} bản sách vào đầu sách {ma}: {string.Join(", ", ds)}", clock.BayGio, ct);
        await LuuAsync(ct);
        return ds.Count;
    }

    public async Task BoGanBanSachAsync(string ma, string maBanSach, string maNd, CancellationToken ct)
    {
        var x = await db.DauSachBanSach.SingleOrDefaultAsync(k => k.MaDauSach == ma && k.MaBanSach == maBanSach, ct)
                ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Bản sách {maBanSach} không gắn vào đầu sách {ma}.");
        if (await db.PhieuDatCho.AnyAsync(d => d.MaBanSach == maBanSach && d.TrangThai == MienGiaTri.TrangThaiDatCho.DangGiu, ct))
            throw LoiNghiepVu.XungDot("DANG_GIU_CHO", $"Bản sách {maBanSach} đang được giữ chỗ cho bạn đọc nên chưa bỏ gắn được.");
        db.DauSachBanSach.Remove(x);
        await NhatKyGhi.ThemAsync(db, maNd, $"Bỏ gắn bản sách {maBanSach} khỏi đầu sách {ma}", clock.BayGio, ct);
        await db.SaveChangesAsync(ct);
    }

    private static void Gan(DauSach d, DauSachNhap n)
    {
        d.TenSach = n.TenSach!.Trim();
        d.TacGia = KiemTraDuLieu.Gon(n.TacGia);
        d.NhaXuatBan = KiemTraDuLieu.Gon(n.NhaXuatBan);
        d.NamXb = n.NamXb;
        d.Isbn = KiemTraDuLieu.Gon(n.Isbn);
        d.MaTheLoai = KiemTraDuLieu.Gon(n.MaTheLoai);
        d.GioiThieu = KiemTraDuLieu.Gon(n.GioiThieu);
        d.TuKhoa = KiemTraDuLieu.Gon(n.TuKhoa);
        d.AnhBia = KiemTraDuLieu.Gon(n.AnhBia);
        d.MauGay = KiemTraDuLieu.Gon(n.MauGay)?.ToUpperInvariant();
    }

    private async Task<KiemTraDuLieu> KiemTraAsync(DauSachNhap n, string? truMa, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        if (truMa is null) kt.Ma("maDauSach", n.MaDauSach, 10, "Mã đầu sách");
        kt.BatBuoc("tenSach", n.TenSach, 200, "Tên sách");
        kt.ToiDa("tacGia", n.TacGia, 100, "Tác giả");
        kt.ToiDa("nhaXuatBan", n.NhaXuatBan, 150, "Nhà xuất bản");
        if (n.NamXb is { } nam && (nam < 1900 || nam > clock.HomNay.Year)) kt.Them("namXb", $"Năm xuất bản từ 1900 đến {clock.HomNay.Year}.");
        var isbn = KiemTraDuLieu.Gon(n.Isbn);
        if (isbn is not null)
        {
            if (!IsbnHopLe().IsMatch(isbn)) kt.Them("isbn", "ISBN gồm 10–20 ký tự chữ số, “-” hoặc “X”.");
            else if (await db.DauSach.AnyAsync(x => x.Isbn == isbn && x.MaDauSach != truMa, ct)) kt.Them("isbn", $"ISBN {isbn} đã có ở đầu sách khác.");
        }
        var tl = KiemTraDuLieu.Gon(n.MaTheLoai);
        if (tl is not null && !await db.TheLoai.AnyAsync(x => x.MaTheLoai == tl, ct)) kt.Them("maTheLoai", "Thể loại không tồn tại.");
        kt.ToiDa("gioiThieu", n.GioiThieu, 2000, "Giới thiệu");
        kt.ToiDa("tuKhoa", n.TuKhoa, 300, "Từ khóa");
        var anh = KiemTraDuLieu.Gon(n.AnhBia);
        if (anh is not null && !AnhBiaHopLe().IsMatch(anh)) kt.Them("anhBia", "Ảnh bìa không hợp lệ – vui lòng tải ảnh lên lại.");
        var mau = KiemTraDuLieu.Gon(n.MauGay);
        if (mau is not null && !MauHopLe().IsMatch(mau)) kt.Them("mauGay", "Màu gáy dạng #RRGGBB.");
        return kt;
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.LaTrungKhoa())
        {
            throw LoiNghiepVu.XungDot("TRUNG_MA", "Mã đầu sách hoặc ISBN đã tồn tại.");
        }
    }
}
