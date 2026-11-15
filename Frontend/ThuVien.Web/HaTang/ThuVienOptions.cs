namespace ThuVien.Web.HaTang;

/// <summary>Mục cấu hình "ThuVien" của web (GIA_DINH D21, D23).</summary>
public sealed class ThuVienOptions
{
    public const string Muc = "ThuVien";

    public ThongTinThuVien ThongTin { get; set; } = new();

    /// <summary>Hiện nhãn “DỮ LIỆU MẪU” (Development). Đặt false khi chụp bộ ảnh Bài 4 (GIA_DINH D23).</summary>
    public bool HienNhanDuLieuMau { get; set; }
}

/// <summary>Thông tin thư viện – chủ dự án bổ sung sau (D21). Trường để trống thì không hiển thị.</summary>
public sealed class ThongTinThuVien
{
    public string TenTruong { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DienThoai { get; set; } = string.Empty;
    public string NoiQuy { get; set; } = string.Empty;
    /// <summary>Giờ mở cửa theo thứ: khóa "T2".."CN", giá trị ví dụ "7:30 – 17:00".</summary>
    public Dictionary<string, string> GioMoCua { get; set; } = new();

    public string TenThuVien => string.IsNullOrWhiteSpace(TenTruong) ? "Thư viện" : $"Thư viện {TenTruong}";
}
