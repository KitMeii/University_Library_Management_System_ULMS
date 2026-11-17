namespace BaseCore.Common.NghiepVu;

/// <summary>Tham số đặt chỗ (file 06 mục 2) – mục cấu hình "ThuVien:DatCho".</summary>
public sealed class DatChoOptions
{
    public const string Muc = "ThuVien:DatCho";

    /// <summary>Số giờ giữ sách kể từ lúc đặt.</summary>
    public int SoGioGiu { get; set; } = 24;
    /// <summary>Số đặt chỗ “Đang giữ” tối đa cùng lúc của một bạn đọc.</summary>
    public int ToiDaDangGiu { get; set; } = 3;
    /// <summary>Gửi thông báo “Sắp hết hạn giữ” khi còn ≤ số giờ này.</summary>
    public int NhacTruocGio { get; set; } = 2;
    /// <summary>Chu kỳ quét của tác vụ nền (phút, cho phép số lẻ – môi trường kiểm thử đặt nhỏ để không phải chờ).</summary>
    public double ChuKyQuetPhut { get; set; } = 5;
    /// <summary>Giờ trong ngày chạy nhắc hạn trả (file 07 mục 4: 7:00).</summary>
    public int GioNhacHanTra { get; set; } = 7;
    /// <summary>Nhắc “Sắp đến hạn trả” khi còn đúng số ngày này.</summary>
    public int NhacTruocHanTraNgay { get; set; } = 2;
    /// <summary>Quầy nhận sách hiển thị trên vé giữ chỗ.</summary>
    public string QuayNhan { get; set; } = "Quầy mượn trả – Tầng 1";
}
