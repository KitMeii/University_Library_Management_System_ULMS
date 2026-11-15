using System.Security.Claims;
using BaseCore.Common.Auth;
using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.QuanTri;
using BaseCore.QuanTriService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BaseCore.QuanTriService.NghiepVu;

public sealed record KetQuaDangNhapNhanVien(
    string Token, string MaDangNhap, string MaNd, string TenNd, string TenDangNhap,
    string MaNhom, string TenNhom, IReadOnlyList<string> Quyen, bool PhaiDoiMatKhau, string ManHinhMacDinh);

/// <summary>
/// SPEC 6.3.1 Đăng nhập (btnDangNhap) + đăng xuất + đổi mật khẩu.
/// Đếm số lần sai liên tiếp trong bộ nhớ của service (NGUOIDUNG không có cột đếm – GIA_DINH D27);
/// trạng thái “Khóa” được lưu bền vào NGUOIDUNG.tinhtrang.
/// </summary>
public sealed class XacThucNhanVienService(
    QuanTriDbContext db, MatKhauHasher hasher, IClock clock, IMemoryCache cache,
    IOptions<JwtOptions> jwt, IHostEnvironment env, ILogger<XacThucNhanVienService> logger)
{
    public const int SoLanSaiToiDa = 5;
    public const string ThongBaoSai = "Tên đăng nhập hoặc mật khẩu không đúng";

    /// <summary>Màn hình mở ngay sau đăng nhập theo nhóm (SPEC 3.3, 6.3.1).</summary>
    public static string ManHinhMacDinh(string maNhom) => maNhom switch
    {
        NhomNguoiDungMa.N1 => "/quan-tri/phieu-muon/lap",
        NhomNguoiDungMa.N2 => "/quan-tri/bien-ban/lap",
        NhomNguoiDungMa.N3 => "/quan-tri/danh-muc/sach",
        NhomNguoiDungMa.N4 => "/quan-tri/tai-khoan",
        _ => "/quan-tri"
    };

    public async Task<KetQuaDangNhapNhanVien> DangNhapAsync(string tenDangNhap, string matKhau, CancellationToken ct)
    {
        tenDangNhap = (tenDangNhap ?? string.Empty).Trim();
        if (tenDangNhap.Length == 0 || string.IsNullOrEmpty(matKhau))
            throw LoiNghiepVu.KhongHopLe("THIEU_THONG_TIN", "Vui lòng nhập tên đăng nhập và mật khẩu.");

        var nd = await db.NguoiDung.SingleOrDefaultAsync(x => x.TenDangNhap == tenDangNhap, ct);
        if (nd is null)
        {
            // Vẫn băm để thời gian phản hồi không lộ việc tài khoản có tồn tại hay không
            hasher.KiemTra(matKhau, new string('A', MatKhauHasher.DoDaiChuoi));
            throw new LoiNghiepVu(StatusCodes.Status401Unauthorized, "SAI_THONG_TIN", ThongBaoSai);
        }
        if (nd.TinhTrang == MienGiaTri.TinhTrangNguoiDung.Khoa)
            throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAI_KHOAN_KHOA",
                "Tài khoản đã bị khóa. Vui lòng liên hệ Quản trị để mở khóa.");

        var khoaDem = "sai:" + nd.MaNd;
        if (!hasher.KiemTra(matKhau, nd.MatKhau))
        {
            var soLan = cache.GetOrCreate(khoaDem, e => { e.SlidingExpiration = TimeSpan.FromHours(12); return 0; }) + 1;
            cache.Set(khoaDem, soLan, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(12) });
            if (soLan >= SoLanSaiToiDa)
            {
                nd.TinhTrang = MienGiaTri.TinhTrangNguoiDung.Khoa;
                await NhatKyGhi.ThemAsync(db, nd.MaNd,
                    $"Khóa tài khoản {nd.TenDangNhap} ({nd.MaNd}) do nhập sai mật khẩu {SoLanSaiToiDa} lần liên tiếp", clock.BayGio, ct);
                await db.SaveChangesAsync(ct);
                cache.Remove(khoaDem);
                logger.LogWarning("Khóa tài khoản {TenDangNhap} sau {SoLan} lần sai", nd.TenDangNhap, soLan);
                throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAI_KHOAN_KHOA",
                    $"Nhập sai mật khẩu {SoLanSaiToiDa} lần liên tiếp. Tài khoản đã bị khóa, vui lòng liên hệ Quản trị.");
            }
            throw new LoiNghiepVu(StatusCodes.Status401Unauthorized, "SAI_THONG_TIN",
                $"{ThongBaoSai} (còn {SoLanSaiToiDa - soLan} lần thử trước khi tài khoản bị khóa).");
        }
        cache.Remove(khoaDem);

        var nhom = await db.NhomNguoiDung.AsNoTracking().SingleAsync(x => x.MaNhom == nd.MaNhom, ct);
        var quyen = await db.PhanQuyen.AsNoTracking().Where(x => x.MaNhom == nd.MaNhom)
            .Select(x => x.MaQuyen).OrderBy(x => x).ToListAsync(ct);

        // D22: Production bắt admin đổi mật khẩu ở lần đăng nhập đầu tiên (chưa có dòng NHATKY_DANGNHAP nào)
        var lanDau = !await db.NhatKyDangNhap.AnyAsync(x => x.MaNd == nd.MaNd, ct);
        var phaiDoi = env.IsProduction() && nd.TenDangNhap == "admin" && lanDau;

        var bayGio = clock.BayGio;
        var maDangNhap = await NhatKyGhi.SinhMaAsync(db, "DN", ThuVienDbContext.SeqNhatKyDangNhap, bayGio, ct);
        db.NhatKyDangNhap.Add(new NhatKyDangNhap { MaDangNhap = maDangNhap, MaNd = nd.MaNd, TgDangNhap = bayGio });
        await db.SaveChangesAsync(ct);

        var claims = new List<Claim>
        {
            new(ThuVienClaims.Loai, LoaiTaiKhoan.NhanVien),
            new(ThuVienClaims.MaNd, nd.MaNd),
            new(ThuVienClaims.Ten, nd.TenNd),
            new(ThuVienClaims.TenDangNhap, nd.TenDangNhap),
            new(ThuVienClaims.MaNhom, nd.MaNhom),
            new(ThuVienClaims.TenNhom, nhom.TenNhom),
            new(ThuVienClaims.MaDangNhap, maDangNhap),
        };
        claims.AddRange(quyen.Select(q => new Claim(ThuVienClaims.Quyen, q)));
        var token = TokenHelper.TaoToken(jwt.Value, claims, clock.BayGioUtc);

        logger.LogInformation("Nhân viên {TenDangNhap} ({MaNhom}) đăng nhập", nd.TenDangNhap, nd.MaNhom);
        return new KetQuaDangNhapNhanVien(token, maDangNhap, nd.MaNd, nd.TenNd, nd.TenDangNhap,
            nd.MaNhom, nhom.TenNhom, quyen, phaiDoi, ManHinhMacDinh(nd.MaNhom));
    }

    /// <summary>Cập nhật NHATKY_DANGNHAP.tg_dangxuat (SPEC 6.3.1 – kết quả đầu ra).</summary>
    public async Task DangXuatAsync(string maNd, string maDangNhap, CancellationToken ct)
    {
        var dong = await db.NhatKyDangNhap.SingleOrDefaultAsync(x => x.MaDangNhap == maDangNhap && x.MaNd == maNd, ct);
        if (dong is { TgDangXuat: null })
        {
            dong.TgDangXuat = clock.BayGio;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task DoiMatKhauAsync(string maNd, string matKhauCu, string matKhauMoi, CancellationToken ct)
    {
        var nd = await db.NguoiDung.SingleOrDefaultAsync(x => x.MaNd == maNd, ct)
                 ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY", "Không tìm thấy tài khoản.");
        if (!hasher.KiemTra(matKhauCu ?? string.Empty, nd.MatKhau))
            throw LoiNghiepVu.KhongHopLe("SAI_MAT_KHAU_CU", "Mật khẩu hiện tại không đúng.",
                new Dictionary<string, string> { ["matKhauCu"] = "Mật khẩu hiện tại không đúng." });
        if (!MatKhauHasher.DatChinhSach(matKhauMoi))
            throw LoiNghiepVu.KhongHopLe("MAT_KHAU_YEU", "Mật khẩu mới phải có tối thiểu 8 ký tự, gồm cả chữ và số.",
                new Dictionary<string, string> { ["matKhauMoi"] = "Tối thiểu 8 ký tự, gồm cả chữ và số." });
        if (matKhauMoi == matKhauCu)
            throw LoiNghiepVu.KhongHopLe("MAT_KHAU_TRUNG", "Mật khẩu mới phải khác mật khẩu hiện tại.",
                new Dictionary<string, string> { ["matKhauMoi"] = "Mật khẩu mới phải khác mật khẩu hiện tại." });

        nd.MatKhau = hasher.Bam(matKhauMoi);
        await NhatKyGhi.ThemAsync(db, nd.MaNd, $"Đổi mật khẩu tài khoản {nd.TenDangNhap} ({nd.MaNd})", clock.BayGio, ct);
        await db.SaveChangesAsync(ct);
    }
}
