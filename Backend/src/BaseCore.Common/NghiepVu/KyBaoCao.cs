using BaseCore.Common.Loi;

namespace BaseCore.Common.NghiepVu;

/// <summary>
/// Kiểm soát dữ liệu nhập kỳ báo cáo MB06/MB07 (TT058 – SPEC 6.4: chọn ngày, không gõ tay): bắt buộc từ ngày, đến ngày;
/// từ ngày ≤ đến ngày; đến ngày không sau hôm nay; tối đa 366 ngày (GIA_DINH D51).
/// </summary>
public static class KyBaoCao
{
    public const int SoNgayToiDa = 366;

    public static (DateOnly TuNgay, DateOnly DenNgay) KiemTra(DateOnly? tuNgay, DateOnly? denNgay, DateOnly homNay)
    {
        var kt = new KiemTraDuLieu();
        if (tuNgay is null) kt.Them("tuNgay", "Vui lòng chọn từ ngày của kỳ báo cáo.");
        if (denNgay is null) kt.Them("denNgay", "Vui lòng chọn đến ngày của kỳ báo cáo.");
        if (tuNgay is { } tu && denNgay is { } den)
        {
            if (tu > den) kt.Them("tuNgay", "Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");
            else if (den > homNay) kt.Them("denNgay", "Đến ngày không được sau ngày hiện tại.");
            else if (den.DayNumber - tu.DayNumber + 1 > SoNgayToiDa) kt.Them("denNgay", $"Kỳ báo cáo tối đa {SoNgayToiDa} ngày.");
        }
        kt.NemNeuCoLoi();
        return (tuNgay!.Value, denNgay!.Value);
    }
}
