namespace ThuVien.Web.Areas.QuanTri.Models;

/// <summary>Khung một biểu đồ (partial _BieuDo – file 09 mục 2): tiêu đề, chú thích kỳ, nút bảng số liệu / tải ảnh.</summary>
public sealed record BieuDoVm(string Id, string TieuDe, string? Ky = null, string Lop = "", string? TenTep = null);

/// <summary>
/// Một thẻ KPI (partial _Kpi – file 08 mục 4, file 10 mục 4): số đếm lên; bấm được → <see cref="Href"/> (trang chi tiết)
/// hoặc <see cref="ChiTiet"/> (mở danh sách chi tiết trong hộp thoại).
/// </summary>
public sealed record KpiVm(string Id, string Nhan, string Icon, string? Href = null, string? ChiTiet = null, string? MoTa = null);

/// <summary>Trang báo cáo MB06/MB07 (01.04.02/01.04.03, 02.05.01/02.05.02).</summary>
public sealed record BaoCaoVm(string Loai, string? TuNgay, string? DenNgay);
