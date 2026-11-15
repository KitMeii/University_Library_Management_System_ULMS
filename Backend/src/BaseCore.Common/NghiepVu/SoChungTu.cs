using System.Globalization;
using System.Text.RegularExpressions;

namespace BaseCore.Common.NghiepVu;

/// <summary>
/// Số chứng từ theo GIA_DINH D4 (SPEC mục 4): {loại 2 chữ}{yy}{MM}{4 số} = đúng 10 ký tự,
/// đánh lại từ 0001 mỗi tháng. Bộ đếm lưu ở SOCHUNGTU (ngay = ngày đầu tháng).
/// </summary>
public static partial class SoChungTu
{
    public const string PhieuMuon = "PM";
    public const string PhieuGiaHan = "GH";
    public const string PhieuTra = "PT";
    public const string BienBan = "BB";
    public const int SoToiDaMoiThang = 9999;

    public static readonly string[] LoaiTheoThang = [PhieuMuon, PhieuGiaHan, PhieuTra, BienBan];

    /// <summary>Ngày đầu tháng dùng làm khóa SOCHUNGTU.ngay.</summary>
    public static DateOnly NgayKhoa(DateOnly ngay) => new(ngay.Year, ngay.Month, 1);

    public static string Tao(string loai, DateOnly ngay, int soThuTu)
    {
        if (!LoaiTheoThang.Contains(loai)) throw new ArgumentException($"Loại chứng từ không hợp lệ: {loai}", nameof(loai));
        if (soThuTu is < 1 or > SoToiDaMoiThang)
            throw new ArgumentOutOfRangeException(nameof(soThuTu), "Đã hết số chứng từ trong tháng (tối đa 9999).");
        return string.Create(CultureInfo.InvariantCulture, $"{loai}{ngay:yyMM}{soThuTu:D4}");
    }

    public static bool HopLe(string? so) => so is not null && MauSo().IsMatch(so);

    [GeneratedRegex(@"^(PM|GH|PT|BB)\d{2}(0[1-9]|1[0-2])\d{4}$")]
    private static partial Regex MauSo();
}
