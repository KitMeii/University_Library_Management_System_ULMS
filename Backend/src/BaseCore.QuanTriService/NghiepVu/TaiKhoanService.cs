using BaseCore.Common.Auth;
using BaseCore.Common.Loi;
using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.DTO.Common;
using BaseCore.Entities;
using BaseCore.Entities.QuanTri;
using BaseCore.QuanTriService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TT = BaseCore.Entities.MienGiaTri.TinhTrangNguoiDung;

namespace BaseCore.QuanTriService.NghiepVu;

public sealed record TaiKhoanDto
{
    public string MaNd { get; init; } = null!;
    public string TenNd { get; init; } = null!;
    public string TenDangNhap { get; init; } = null!;
    public string? Sdt { get; init; }
    public string? ChucVu { get; init; }
    public string TinhTrang { get; init; } = null!;
    public string MaNhom { get; init; } = null!;
    public string TenNhom { get; init; } = null!;
}
public sealed record TaiKhoanNhap(string? TenNd, string? TenDangNhap, string? Sdt, string? ChucVu, string? TinhTrang, string? MaNhom);

/// <summary>SPEC 6.3.2 Quản lý tài khoản người dùng (04.00.01). Không có chức năng xóa – chỉ khóa.</summary>
public sealed class TaiKhoanService(QuanTriDbContext db, IClock clock, MatKhauHasher hasher, IOptions<QuyDinhOptions> quyDinh)
{
    private static readonly string[] SapXep =
        [nameof(TaiKhoanDto.MaNd), nameof(TaiKhoanDto.TenNd), nameof(TaiKhoanDto.TenDangNhap), nameof(TaiKhoanDto.MaNhom), nameof(TaiKhoanDto.TinhTrang)];

    private IQueryable<TaiKhoanDto> TruyVan() =>
        from n in db.NguoiDung.AsNoTracking()
        join g in db.NhomNguoiDung on n.MaNhom equals g.MaNhom
        select new TaiKhoanDto { MaNd = n.MaNd, TenNd = n.TenNd, TenDangNhap = n.TenDangNhap, Sdt = n.Sdt, ChucVu = n.ChucVu, TinhTrang = n.TinhTrang, MaNhom = n.MaNhom, TenNhom = g.TenNhom };

    /// <summary>frmTaiKhoan (Load); txtTimKiem, btnTimKiem: lọc theo ma_nd, ten_nd, tendangnhap, nhóm, tinhtrang.</summary>
    public Task<KetQuaTrang<TaiKhoanDto>> DanhSachAsync(ThamSoLuoi ts, CancellationToken ct)
    {
        var q = TruyVan();
        var k = KiemTraDuLieu.Gon(ts.TuKhoa);
        if (k is not null)
            q = q.Where(x => x.MaNd.Contains(k) || x.TenNd.Contains(k) || x.TenDangNhap.Contains(k) ||
                             x.MaNhom.Contains(k) || x.TenNhom.Contains(k) || x.TinhTrang.Contains(k));
        return q.LayTrangAsync(ts, SapXep, nameof(TaiKhoanDto.MaNd), x => x, ct);
    }

    /// <summary>btnThem: tự sinh ma_nd (ND####).</summary>
    public async Task<string> MaKeTiepAsync(CancellationToken ct)
    {
        var ds = await db.NguoiDung.AsNoTracking().Where(x => x.MaNd.StartsWith("ND")).Select(x => x.MaNd).ToListAsync(ct);
        var max = ds.Select(m => int.TryParse(m[2..], out var so) ? so : 0).DefaultIfEmpty(0).Max();
        return $"ND{max + 1:D4}";
    }

    /// <summary>btnLuu (thêm): tinhtrang mặc định “Hoạt động”, matkhau = mật khẩu mặc định đã băm.</summary>
    public async Task<TaiKhoanDto> ThemAsync(TaiKhoanNhap n, string maNdLap, CancellationToken ct)
    {
        var kt = await KiemTraAsync(n, true, ct);
        if (!string.IsNullOrWhiteSpace(n.TenDangNhap) && await db.NguoiDung.AnyAsync(x => x.TenDangNhap == n.TenDangNhap.Trim(), ct))
            kt.Them("tenDangNhap", $"Tên đăng nhập {n.TenDangNhap} đã tồn tại.");
        kt.NemNeuCoLoi();

        var matKhau = hasher.Bam(quyDinh.Value.MatKhauMacDinh);
        // ma_nd tự sinh: trùng do hai quản trị thêm cùng lúc → lỗi khóa chính, báo người dùng lưu lại
        var maNd = await db.ChayAsync(async c =>
        {
            var nd = new NguoiDung
            {
                MaNd = await MaKeTiepAsync(c),
                TenNd = n.TenNd!.Trim(), TenDangNhap = n.TenDangNhap!.Trim(), MatKhau = matKhau,
                Sdt = KiemTraDuLieu.Gon(n.Sdt), ChucVu = KiemTraDuLieu.Gon(n.ChucVu),
                TinhTrang = string.IsNullOrEmpty(n.TinhTrang) ? TT.HoatDong : n.TinhTrang, MaNhom = n.MaNhom!
            };
            db.NguoiDung.Add(nd);
            await NhatKyGhi.ThemAsync(db, maNdLap, $"04.00.01 Thêm tài khoản {nd.MaNd} – {nd.TenDangNhap} ({nd.TenNd}), nhóm {nd.MaNhom}", clock.BayGio, c);
            await LuuAsync(c);
            return nd.MaNd;
        }, ct);
        return await MotAsync(maNd, ct);
    }

    /// <summary>btnSua → btnLuu: sửa ten_nd, sdt, chucvu, ma_nhom, tinhtrang (không sửa ma_nd, tendangnhap).</summary>
    public async Task<TaiKhoanDto> SuaAsync(string ma, TaiKhoanNhap n, string maNdLap, CancellationToken ct)
    {
        var nd = await TimAsync(ma, ct);
        (await KiemTraAsync(n, false, ct)).NemNeuCoLoi();
        var tinhTrangMoi = string.IsNullOrEmpty(n.TinhTrang) ? nd.TinhTrang : n.TinhTrang;
        await BaoVeQuanTriAsync(nd, n.MaNhom!, tinhTrangMoi, maNdLap, ct);
        var cu = $"{nd.TenNd}, nhóm {nd.MaNhom}, {nd.TinhTrang}";
        nd.TenNd = n.TenNd!.Trim();
        nd.Sdt = KiemTraDuLieu.Gon(n.Sdt);
        nd.ChucVu = KiemTraDuLieu.Gon(n.ChucVu);
        nd.MaNhom = n.MaNhom!;
        nd.TinhTrang = tinhTrangMoi;
        await NhatKyGhi.ThemAsync(db, maNdLap, $"04.00.01 Sửa tài khoản {nd.MaNd} ({nd.TenDangNhap}): {cu} → {nd.TenNd}, nhóm {nd.MaNhom}, {nd.TinhTrang}", clock.BayGio, ct);
        await LuuAsync(ct);
        return await MotAsync(ma, ct);
    }

    /// <summary>btnKhoa: đổi tinhtrang giữa “Hoạt động” và “Khóa”.</summary>
    public async Task<TaiKhoanDto> DoiKhoaAsync(string ma, string maNdLap, CancellationToken ct)
    {
        var nd = await TimAsync(ma, ct);
        var moi = nd.TinhTrang == TT.Khoa ? TT.HoatDong : TT.Khoa;
        await BaoVeQuanTriAsync(nd, nd.MaNhom, moi, maNdLap, ct);
        nd.TinhTrang = moi;
        await NhatKyGhi.ThemAsync(db, maNdLap, $"04.00.01 {(moi == TT.Khoa ? "Khóa" : "Mở khóa")} tài khoản {nd.MaNd} ({nd.TenDangNhap})", clock.BayGio, ct);
        await LuuAsync(ct);
        return await MotAsync(ma, ct);
    }

    /// <summary>btnDatLaiMK: matkhau = mật khẩu mặc định (đã băm).</summary>
    public async Task DatLaiMatKhauAsync(string ma, string maNdLap, CancellationToken ct)
    {
        var nd = await TimAsync(ma, ct);
        nd.MatKhau = hasher.Bam(quyDinh.Value.MatKhauMacDinh);
        await NhatKyGhi.ThemAsync(db, maNdLap, $"04.00.01 Đặt lại mật khẩu mặc định cho tài khoản {nd.MaNd} ({nd.TenDangNhap})", clock.BayGio, ct);
        await LuuAsync(ct);
    }

    /// <summary>
    /// Giữ hệ thống luôn còn tài khoản quản trị (cùng tinh thần SPEC 6.3.3): không tự khóa / tự bỏ nhóm N4 của chính mình,
    /// không khóa / chuyển nhóm tài khoản N4 hoạt động cuối cùng.
    /// </summary>
    private async Task BaoVeQuanTriAsync(NguoiDung nd, string nhomMoi, string tinhTrangMoi, string maNdLap, CancellationToken ct)
    {
        var mat = nd.MaNhom == NhomNguoiDungMa.N4 && nd.TinhTrang == TT.HoatDong &&
                  (nhomMoi != NhomNguoiDungMa.N4 || tinhTrangMoi != TT.HoatDong);
        if (!mat) return;
        if (nd.MaNd == maNdLap)
            throw LoiNghiepVu.XungDot("TU_KHOA_MINH", "Không thể khóa hoặc bỏ quyền quản trị của chính tài khoản đang đăng nhập.");
        var conLai = await db.NguoiDung.CountAsync(x => x.MaNhom == NhomNguoiDungMa.N4 && x.TinhTrang == TT.HoatDong && x.MaNd != nd.MaNd, ct);
        if (conLai == 0)
            throw LoiNghiepVu.XungDot("QUAN_TRI_CUOI", "Đây là tài khoản quản trị hoạt động cuối cùng, không thể khóa hoặc chuyển nhóm.");
    }

    private async Task<NguoiDung> TimAsync(string ma, CancellationToken ct) =>
        await db.NguoiDung.SingleOrDefaultAsync(x => x.MaNd == ma, ct)
        ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", $"Không tìm thấy tài khoản {ma}.");

    private Task<TaiKhoanDto> MotAsync(string ma, CancellationToken ct) => TruyVan().SingleAsync(x => x.MaNd == ma, ct);

    /// <summary>Kiểm soát dữ liệu nhập theo điều kiện toàn vẹn SPEC 6.3.2.</summary>
    private async Task<KiemTraDuLieu> KiemTraAsync(TaiKhoanNhap n, bool laThem, CancellationToken ct)
    {
        var kt = new KiemTraDuLieu();
        kt.BatBuoc("tenNd", n.TenNd, 50, "Tên người dùng");
        if (laThem) kt.Ma("tenDangNhap", n.TenDangNhap, 30, "Tên đăng nhập");
        var sdt = KiemTraDuLieu.Gon(n.Sdt);
        if (sdt is not null) kt.Dung(sdt.Length <= 15 && KiemTraDuLieu.ChiSo().IsMatch(sdt), "sdt", "Số điện thoại chỉ gồm chữ số, tối đa 15 ký tự.");
        kt.ToiDa("chucVu", n.ChucVu, 50, "Chức vụ");
        if (!laThem || !string.IsNullOrEmpty(n.TinhTrang)) kt.ThuocMien("tinhTrang", n.TinhTrang, TT.TatCa, "Tình trạng");
        if (string.IsNullOrEmpty(n.MaNhom) || !await db.NhomNguoiDung.AnyAsync(x => x.MaNhom == n.MaNhom, ct))
            kt.Them("maNhom", "Vui lòng chọn nhóm người dùng có trong danh mục nhóm.");
        return kt;
    }

    private async Task LuuAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.LaTrungKhoa())
        {
            throw LoiNghiepVu.XungDot("TRUNG_MA", "Mã người dùng hoặc tên đăng nhập đã tồn tại.");
        }
    }
}
