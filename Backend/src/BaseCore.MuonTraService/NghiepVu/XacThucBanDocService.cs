using System.Security.Claims;
using BaseCore.Common.Auth;
using BaseCore.Common.Loi;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.MuonTraService.DuLieu;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BaseCore.MuonTraService.NghiepVu;

public sealed record KetQuaDangNhapBanDoc(string Token, string MaBanDoc, string HoTen, string MaSvHvCb, string DoiTuong);

/// <summary>
/// Đăng nhập cổng bạn đọc (file 07 mục 2): ma_svhvcb + mật khẩu; sai 5 lần → tạm khóa 15 phút (tg_khoa_den).
/// Kích hoạt tài khoản, quên mật khẩu: Pha 6.
/// </summary>
public sealed class XacThucBanDocService(MuonTraDbContext db, MatKhauHasher hasher, IClock clock,
    IOptions<JwtOptions> jwt, ILogger<XacThucBanDocService> logger)
{
    public const int SoLanSaiToiDa = 5;
    public static readonly TimeSpan ThoiGianKhoa = TimeSpan.FromMinutes(15);
    private const string ThongBaoSai = "Mã sinh viên/học viên/cán bộ hoặc mật khẩu không đúng";

    public async Task<KetQuaDangNhapBanDoc> DangNhapAsync(string maSvHvCb, string matKhau, CancellationToken ct)
    {
        maSvHvCb = (maSvHvCb ?? string.Empty).Trim();
        if (maSvHvCb.Length == 0 || string.IsNullOrEmpty(matKhau))
            throw LoiNghiepVu.KhongHopLe("THIEU_THONG_TIN", "Vui lòng nhập mã SV/HV/CB và mật khẩu.");

        var bd = await db.BanDoc.AsNoTracking().SingleOrDefaultAsync(x => x.MaSvHvCb == maSvHvCb, ct);
        var tk = bd is null ? null : await db.TaiKhoanBanDoc.SingleOrDefaultAsync(x => x.MaBanDoc == bd.MaBanDoc, ct);
        if (bd is null || tk is null)
        {
            hasher.KiemTra(matKhau, new string('A', MatKhauHasher.DoDaiChuoi));
            throw new LoiNghiepVu(StatusCodes.Status401Unauthorized, "SAI_THONG_TIN", ThongBaoSai);
        }

        var bayGio = clock.BayGio;
        if (tk.TinhTrang == MienGiaTri.TinhTrangTaiKhoanBanDoc.ChuaKichHoat)
            throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "CHUA_KICH_HOAT",
                "Tài khoản chưa được kích hoạt. Vui lòng kích hoạt bằng mã SV/HV/CB, số thẻ và ngày sinh.");
        if (tk.TinhTrang == MienGiaTri.TinhTrangTaiKhoanBanDoc.Khoa)
            throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAI_KHOAN_KHOA",
                "Tài khoản cổng bạn đọc đã bị khóa. Vui lòng liên hệ quầy mượn trả.");
        if (tk.TgKhoaDen is { } den && den > bayGio)
            throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAM_KHOA",
                $"Đăng nhập sai quá {SoLanSaiToiDa} lần. Tài khoản tạm khóa đến {den:HH:mm dd/MM/yyyy}.");

        if (!hasher.KiemTra(matKhau, tk.MatKhau))
        {
            tk.SoLanSai++;
            if (tk.SoLanSai >= SoLanSaiToiDa)
            {
                tk.SoLanSai = 0;
                tk.TgKhoaDen = bayGio.Add(ThoiGianKhoa);
                await db.SaveChangesAsync(ct);
                throw new LoiNghiepVu(StatusCodes.Status403Forbidden, "TAM_KHOA",
                    $"Đăng nhập sai {SoLanSaiToiDa} lần. Tài khoản tạm khóa đến {tk.TgKhoaDen:HH:mm dd/MM/yyyy}.");
            }
            await db.SaveChangesAsync(ct);
            throw new LoiNghiepVu(StatusCodes.Status401Unauthorized, "SAI_THONG_TIN",
                $"{ThongBaoSai} (còn {SoLanSaiToiDa - tk.SoLanSai} lần thử).");
        }

        tk.SoLanSai = 0;
        tk.TgKhoaDen = null;
        tk.TgDangNhapCuoi = bayGio;
        await db.SaveChangesAsync(ct);

        var token = TokenHelper.TaoToken(jwt.Value,
        [
            new Claim(ThuVienClaims.Loai, LoaiTaiKhoan.BanDoc),
            new Claim(ThuVienClaims.MaBanDoc, bd.MaBanDoc),
            new Claim(ThuVienClaims.Ten, bd.HoTen),
            new Claim(ThuVienClaims.MaSvHvCb, bd.MaSvHvCb),
        ], clock.BayGioUtc);
        logger.LogInformation("Bạn đọc {MaBanDoc} đăng nhập cổng", bd.MaBanDoc);
        return new KetQuaDangNhapBanDoc(token, bd.MaBanDoc, bd.HoTen, bd.MaSvHvCb, bd.DoiTuong);
    }
}
