using System.Security.Claims;
using BaseCore.Common.Auth;

namespace ThuVien.Web.HaTang;

/// <summary>
/// Một mục menu dashboard. Hiện khi người dùng có MỘT trong <see cref="Quyen"/> (PHANQUYEN) và không có
/// <see cref="AnNeuCo"/>. <see cref="Pha"/> = pha hoàn thiện giao diện (trước đó hiển thị trang “đang xây dựng”).
/// </summary>
public sealed record MucMenu(string Ten, string DuongDan, string[] Quyen, string Icon, int Pha, string? AnNeuCo = null);

public sealed record NhomMenu(string Ten, string Icon, MucMenu[] Muc);

/// <summary>
/// Menu theo SPEC 3.3 (quy ước: chức năng nghiệp vụ có mã, danh mục không mã) và nhóm sidebar file 08.
/// Mục tiến trình 03.xx của N3 đều mở Danh mục sách (SPEC 6.1.1) với tham số tiến trình.
/// </summary>
public static class MenuQuanTri
{
    public static readonly NhomMenu[] TatCa =
    [
        new("Mượn trả sách", "bi-arrow-left-right",
        [
            new("01.01.01 Lưu thông tin Thẻ Bạn đọc", "/quan-tri/ban-doc", ["TT001"], "bi-person-vcard", 2),
            new("01.01.03 Lập Phiếu mượn sách", "/quan-tri/phieu-muon/lap", ["TT012"], "bi-journal-arrow-up", 3),
            new("01.02.01 Lập Phiếu gia hạn sách", "/quan-tri/phieu-gia-han/lap", ["TT030"], "bi-calendar-plus", 3),
            new("01.02.02 Lập Phiếu trả sách", "/quan-tri/phieu-tra/lap", ["TT043"], "bi-journal-arrow-down", 3),
        ]),
        new("Xử lý vi phạm", "bi-exclamation-octagon",
        [
            new("02.03.01 Lập Biên bản vi phạm mượn trả sách", "/quan-tri/bien-ban/lap", ["TT074"], "bi-file-earmark-ruled", 3),
        ]),
        new("Kho sách", "bi-box-seam",
        [
            new("03.01.02 Kiểm tra khả năng đáp ứng sách", "/quan-tri/danh-muc/sach?tienTrinh=03.01.02", ["TT099"], "bi-search", 2),
            new("03.01.04 Cập nhật tình trạng sách đang được mượn", "/quan-tri/danh-muc/sach?tienTrinh=03.01.04", ["TT106"], "bi-arrow-repeat", 3),
            new("03.02.03 Cập nhật sách sau trả", "/quan-tri/danh-muc/sach?tienTrinh=03.02.03", ["TT109"], "bi-box-arrow-in-down", 3),
            new("03.03.02 Cập nhật tình trạng sách bị mất, hư hỏng", "/quan-tri/danh-muc/sach?tienTrinh=03.03.02", ["TT113"], "bi-bandaid", 3),
            new("03.04.01 Tổng hợp số sách theo tình trạng", "/quan-tri/danh-muc/sach?tienTrinh=03.04.01", ["TT117"], "bi-pie-chart", 2),
        ]),
        new("Thống kê", "bi-bar-chart-line",
        [
            new("01.04.02 Tổng hợp tình hình mượn trả sách", "/quan-tri/bao-cao/muon-tra", ["TT057"], "bi-clipboard-data", 4),
            new("01.04.03 Lập Báo cáo tình hình mượn trả sách", "/quan-tri/bao-cao/muon-tra/lap", ["TT069"], "bi-printer", 4),
            new("02.05.01 Tổng hợp tình hình vi phạm mượn trả sách", "/quan-tri/bao-cao/vi-pham", ["TT090"], "bi-clipboard-data", 4),
            new("02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách", "/quan-tri/bao-cao/vi-pham/lap", ["TT095"], "bi-printer", 4),
        ]),
        new("Danh mục", "bi-collection",
        [
            new("Danh mục sách", "/quan-tri/danh-muc/sach", ["SACH_R"], "bi-book", 2),
            new("Danh mục khoản thu", "/quan-tri/danh-muc/khoan-thu", ["KHOANTHU_R"], "bi-cash-coin", 2),
            // N1 dùng “01.01.01 Lưu thông tin Thẻ Bạn đọc” (cùng giao diện 6.1.3) nên không lặp mục danh mục
            new("Danh mục Bạn đọc", "/quan-tri/danh-muc/ban-doc", ["BANDOC_R"], "bi-people", 2, AnNeuCo: "TT001"),
            new("Danh mục khu vực lưu trữ", "/quan-tri/danh-muc/khu-vuc", ["KHUVUC_R"], "bi-geo-alt", 2),
        ]),
        new("Quản trị", "bi-shield-lock",
        [
            new("04.00.01 Quản lý tài khoản người dùng", "/quan-tri/tai-khoan", ["QT040001"], "bi-person-gear", 2),
            new("04.00.02 Cấp quyền cho tài khoản", "/quan-tri/phan-quyen", ["QT040002"], "bi-key", 2),
            new("04.00.03 Theo dõi lịch sử đăng nhập", "/quan-tri/nhat-ky/dang-nhap", ["QT040003"], "bi-clock-history", 2),
            new("04.00.08 Theo dõi nhật ký hoạt động", "/quan-tri/nhat-ky/hoat-dong", ["QT040008"], "bi-list-check", 2),
            new("04.00.09 Sao lưu và phục hồi dữ liệu", "/quan-tri/sao-luu", ["QT040009"], "bi-database-down", 2),
        ]),
    ];

    public static bool DuocThay(this MucMenu m, ClaimsPrincipal u) =>
        m.Quyen.Any(u.CoQuyen) && (m.AnNeuCo is null || !u.CoQuyen(m.AnNeuCo));

    public static IEnumerable<NhomMenu> ChoNguoiDung(ClaimsPrincipal u) =>
        TatCa.Select(n => n with { Muc = n.Muc.Where(m => m.DuocThay(u)).ToArray() }).Where(n => n.Muc.Length > 0);

    /// <summary>Tìm mục menu theo đường dẫn (không tính query) – dùng cho trang “đang xây dựng” và breadcrumb.</summary>
    public static (NhomMenu Nhom, MucMenu Muc)? Tim(string duongDanDayDu) =>
        TatCa.SelectMany(n => n.Muc.Select(m => (n, m)))
            .Where(x => string.Equals(x.m.DuongDan, duongDanDayDu, StringComparison.OrdinalIgnoreCase))
            .Select(x => ((NhomMenu, MucMenu)?)(x.n, x.m))
            .FirstOrDefault();
}
