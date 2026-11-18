#nullable disable
// Mã basecore gốc (BASECORE_PHAN_TICH mục 6/6a), chưa chú thích nullable; tắt cảnh báo nullable cho tệp này (Pha 8 – build sạch).

namespace BaseCore.DTO.Response
{
    public class GridResponse
    {
        public int TotalRecords { get; set; }
        public object Records { get; set; }
    }
}
