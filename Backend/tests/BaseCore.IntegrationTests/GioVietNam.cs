namespace BaseCore.IntegrationTests;

/// <summary>
/// “Hôm nay” theo giờ Việt Nam – trùng IClock của các service. Container kiểm thử chạy giờ UTC nên <c>DateTime.Today</c>
/// lệch một ngày trong khoảng 00:00–07:00 giờ Việt Nam (lỗi phát hiện khi chạy bộ kiểm thử lúc 03:17 ngày 18/11/2026).
/// </summary>
internal static class GioVietNam
{
    public static DateTime HomNay => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BaseCore.Common.ThoiGian.DongHoHeThong.MuiGioVietNam).Date;
}
