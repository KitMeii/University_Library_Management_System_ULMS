using System.Globalization;
using TTDC = BaseCore.Entities.MienGiaTri.TrangThaiDatCho;

namespace BaseCore.Entities;

/// <summary>
/// Quy tắc vòng đời đặt chỗ (file 06 mục 3) – hàm thuần, mọi thời điểm truyền vào từ <c>IClock</c> để kiểm thử tua thời gian.
/// <code>
/// (không) ──đặt chỗ──► Đang giữ ──nhân viên giao sách (Lập Phiếu mượn)──► Đã nhận
///                         ├──bạn đọc hủy / nhân viên hủy (ghi lý do)──► Đã hủy
///                         └──quá tg_hethan (tác vụ nền)──────────────► Hết hạn
/// </code>
/// </summary>
public static class VongDoiDatCho
{
    /// <summary>Đặt chỗ còn giữ thật sự: “Đang giữ” và chưa quá hạn. Truy vấn số bản có thể mượn luôn dùng điều kiện này
    /// (không phụ thuộc tác vụ nền chạy đúng giờ – file 06 mục 3).</summary>
    public static bool ConGiu(string trangThai, DateTime tgHetHan, DateTime bayGio) =>
        trangThai == TTDC.DangGiu && tgHetHan > bayGio;

    /// <summary>Trạng thái hiển thị: “Đang giữ” đã quá hạn mà tác vụ nền chưa quét vẫn hiện “Hết hạn”.</summary>
    public static string TrangThaiHieuLuc(string trangThai, DateTime tgHetHan, DateTime bayGio) =>
        trangThai == TTDC.DangGiu && tgHetHan <= bayGio ? TTDC.HetHan : trangThai;

    /// <summary>Chỉ “Đang giữ” mới chuyển được, và chỉ sang Đã nhận / Đã hủy / Hết hạn.</summary>
    public static bool ChuyenHopLe(string tu, string den) =>
        tu == TTDC.DangGiu && den is TTDC.DaNhan or TTDC.DaHuy or TTDC.HetHan;

    /// <summary>Đã tới lúc nhắc “Sắp hết hạn giữ”: còn giữ và còn ≤ <paramref name="nhacTruocGio"/> giờ.</summary>
    public static bool CanNhac(string trangThai, DateTime tgHetHan, DateTime bayGio, int nhacTruocGio) =>
        ConGiu(trangThai, tgHetHan, bayGio) && tgHetHan - bayGio <= TimeSpan.FromHours(nhacTruocGio);

    public static DateTime HetHan(DateTime tgDat, int soGioGiu) => tgDat.AddHours(soGioGiu);

    /// <summary>Mã đặt chỗ dạng DCyyyyMMdd-#### (file 02 X05) – đánh số theo ngày đặt.</summary>
    public static string TaoMa(DateOnly ngay, int soThuTu)
    {
        if (soThuTu is < 1 or > 9999) throw new ArgumentOutOfRangeException(nameof(soThuTu), "Đã hết mã đặt chỗ trong ngày (tối đa 9999).");
        return string.Create(CultureInfo.InvariantCulture, $"DC{ngay:yyyyMMdd}-{soThuTu:D4}");
    }

    public const string LoaiSoChungTu = "DC";
}
