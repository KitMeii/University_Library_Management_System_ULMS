namespace BaseCore.Common.ThoiGian;

/// <summary>
/// Nguồn thời gian duy nhất của hệ thống (file 01 mục 5). Trả giờ Việt Nam (Asia/Ho_Chi_Minh).
/// Kiểm thử thay bằng đồng hồ giả để tua thời gian (đặt chỗ 24 giờ).
/// </summary>
public interface IClock
{
    /// <summary>Thời điểm hiện tại theo giờ Việt Nam (Kind = Unspecified, lưu vào datetime2).</summary>
    DateTime BayGio { get; }
    DateTime BayGioUtc { get; }
    DateOnly HomNay => DateOnly.FromDateTime(BayGio);
}

public sealed class DongHoHeThong : IClock
{
    public static readonly TimeZoneInfo MuiGioVietNam = TimNguiGio();

    private readonly TimeProvider _time;
    public DongHoHeThong(TimeProvider? time = null) => _time = time ?? TimeProvider.System;

    public DateTime BayGioUtc => _time.GetUtcNow().UtcDateTime;

    public DateTime BayGio => DateTime.SpecifyKind(
        TimeZoneInfo.ConvertTimeFromUtc(BayGioUtc, MuiGioVietNam), DateTimeKind.Unspecified);

    private static TimeZoneInfo TimNguiGio()
    {
        foreach (var id in new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz)) return tz;
        }
        return TimeZoneInfo.CreateCustomTimeZone("ICT", TimeSpan.FromHours(7), "ICT", "ICT");
    }
}
