using System.Text.RegularExpressions;

namespace BaseCore.Common.Loi;

/// <summary>
/// Kiểm soát dữ liệu nhập theo cột “Điều kiện toàn vẹn” của SPEC mục 6. Gom lỗi theo trường rồi ném một
/// <see cref="LoiNghiepVu"/> 422 duy nhất (định dạng { ma, thongBao, truong } – file 01 mục 5).
/// Tên trường dùng đúng tên thuộc tính JSON để giao diện hiển thị lỗi ngay dưới ô (SPEC mục 5).
/// </summary>
public sealed partial class KiemTraDuLieu
{
    private readonly Dictionary<string, string> _loi = new(StringComparer.OrdinalIgnoreCase);

    public bool CoLoi => _loi.Count > 0;

    public KiemTraDuLieu Them(string truong, string thongBao)
    {
        _loi.TryAdd(truong, thongBao);
        return this;
    }

    /// <summary>Không rỗng, tối đa n ký tự.</summary>
    public KiemTraDuLieu BatBuoc(string truong, string? giaTri, int toiDa, string nhan)
    {
        if (string.IsNullOrWhiteSpace(giaTri)) return Them(truong, $"{nhan} không được để trống.");
        if (giaTri.Trim().Length > toiDa) return Them(truong, $"{nhan} tối đa {toiDa} ký tự.");
        return this;
    }

    /// <summary>Không bắt buộc, tối đa n ký tự.</summary>
    public KiemTraDuLieu ToiDa(string truong, string? giaTri, int toiDa, string nhan)
    {
        if (giaTri is not null && giaTri.Trim().Length > toiDa) Them(truong, $"{nhan} tối đa {toiDa} ký tự.");
        return this;
    }

    /// <summary>Mã: không rỗng, chỉ chữ cái (không dấu) + chữ số, tối đa n ký tự.</summary>
    public KiemTraDuLieu Ma(string truong, string? giaTri, int toiDa, string nhan)
    {
        if (string.IsNullOrWhiteSpace(giaTri)) return Them(truong, $"{nhan} không được để trống.");
        if (giaTri.Length > toiDa) return Them(truong, $"{nhan} tối đa {toiDa} ký tự.");
        if (!ChuSo().IsMatch(giaTri)) return Them(truong, $"{nhan} chỉ gồm chữ cái (không dấu) và chữ số.");
        return this;
    }

    public KiemTraDuLieu ThuocMien(string truong, string? giaTri, IEnumerable<string> mien, string nhan)
    {
        var ds = mien.ToArray();
        if (giaTri is null || !ds.Contains(giaTri)) Them(truong, $"{nhan} phải thuộc: {string.Join(", ", ds)}.");
        return this;
    }

    public KiemTraDuLieu Dung(bool dieuKien, string truong, string thongBao)
    {
        if (!dieuKien) Them(truong, thongBao);
        return this;
    }

    /// <summary>Ném lỗi 422 nếu có ít nhất một trường sai.</summary>
    public void NemNeuCoLoi()
    {
        if (CoLoi)
            throw LoiNghiepVu.KhongHopLe("DU_LIEU_KHONG_HOP_LE",
                _loi.Count == 1 ? _loi.Values.First() : "Dữ liệu nhập chưa hợp lệ, vui lòng kiểm tra các ô được đánh dấu.", _loi);
    }

    public static string? Gon(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    [GeneratedRegex("^[A-Za-z0-9]+$")] private static partial Regex ChuSo();
    [GeneratedRegex(@"^[0-9]+$")] public static partial Regex ChiSo();
}
