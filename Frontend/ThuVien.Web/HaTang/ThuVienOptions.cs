namespace ThuVien.Web.HaTang;

/// <summary>Mục cấu hình "ThuVien" của web (GIA_DINH D21, D23).</summary>
public sealed class ThuVienOptions
{
    public const string Muc = "ThuVien";

    public ThongTinThuVien ThongTin { get; set; } = new();

    /// <summary>Hiện nhãn “DỮ LIỆU MẪU” (Development). Đặt false khi chụp bộ ảnh Bài 4 (GIA_DINH D23).</summary>
    public bool HienNhanDuLieuMau { get; set; }
}

/// <summary>
/// Thông tin thư viện (D21, D75) – đặt qua biến môi trường THUVIEN_* trong .env (docker-compose chuyển thành ThuVien__ThongTin__*).
/// Trường để trống thì không hiển thị.
/// </summary>
public sealed class ThongTinThuVien
{
    private static readonly (string Khoa, string Nhan)[] ThuTrongTuan =
        [("T2", "Thứ 2"), ("T3", "Thứ 3"), ("T4", "Thứ 4"), ("T5", "Thứ 5"), ("T6", "Thứ 6"), ("T7", "Thứ 7"), ("CN", "Chủ nhật")];

    public string TenTruong { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DienThoai { get; set; } = string.Empty;
    /// <summary>Ghi chú nội quy thêm (tùy chọn). Quy định mượn trả/đặt chỗ lấy từ GET /api/cong/quy-dinh, không lặp ở đây (D74).</summary>
    public string NoiQuy { get; set; } = string.Empty;
    /// <summary>Giờ mở cửa theo thứ: khóa "T2".."CN", giá trị ví dụ "7:30 – 17:00" hoặc "Nghỉ".</summary>
    public Dictionary<string, string> GioMoCua { get; set; } = new();

    public string TenThuVien => string.IsNullOrWhiteSpace(TenTruong) ? "Thư viện" : $"Thư viện {TenTruong}";

    public bool CoLienHe => !string.IsNullOrWhiteSpace(DiaChi) || !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(DienThoai);

    public bool CoGioMoCua => GioMoCua.Values.Any(v => !string.IsNullOrWhiteSpace(v));

    /// <summary>Khóa thứ hôm nay theo giờ Việt Nam (container chạy UTC – không dùng DateTime.Now).</summary>
    public static string ThuHomNay()
    {
        var bayGio = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BaseCore.Common.ThoiGian.DongHoHeThong.MuiGioVietNam);
        return bayGio.DayOfWeek switch
        {
            DayOfWeek.Monday => "T2", DayOfWeek.Tuesday => "T3", DayOfWeek.Wednesday => "T4", DayOfWeek.Thursday => "T5",
            DayOfWeek.Friday => "T6", DayOfWeek.Saturday => "T7", _ => "CN"
        };
    }

    public string? GioCua(string thu) => GioMoCua.TryGetValue(thu, out var g) && !string.IsNullOrWhiteSpace(g) ? g.Trim() : null;

    /// <summary>Giờ mở cửa gộp các ngày liền nhau cùng giờ, theo thứ tự trong tuần: “Thứ 2 – Thứ 6: 7:00 – 20:00”.</summary>
    public IReadOnlyList<(string Nhan, string Gio, bool CoHomNay)> GioGop(string thuHomNay)
    {
        var kq = new List<(string, string, bool)>();
        var i = 0;
        while (i < ThuTrongTuan.Length)
        {
            var gio = GioCua(ThuTrongTuan[i].Khoa);
            if (gio is null) { i++; continue; }
            var j = i;
            while (j + 1 < ThuTrongTuan.Length && GioCua(ThuTrongTuan[j + 1].Khoa) == gio) j++;
            var nhan = i == j ? ThuTrongTuan[i].Nhan : $"{ThuTrongTuan[i].Nhan} – {ThuTrongTuan[j].Nhan}";
            var coHomNay = ThuTrongTuan[i..(j + 1)].Any(t => t.Khoa == thuHomNay);
            kq.Add((nhan, gio, coHomNay));
            i = j + 1;
        }
        return kq;
    }
}
