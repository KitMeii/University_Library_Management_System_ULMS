namespace BaseCore.Entities;

/// <summary>Tên schema theo service (GIA_DINH D10, D11).</summary>
public static class LuocDo
{
    public const string QuanTri = "quantri";
    public const string KhoSach = "khosach";
    public const string MuonTra = "muontra";
    public const string ViPham = "vipham";
    public static readonly string[] TatCa = [QuanTri, KhoSach, MuonTra, ViPham];
}

/// <summary>Miền giá trị (SPEC mục 2 “Miền giá trị” + file 02). Tạo thành CHECK constraint.</summary>
public static class MienGiaTri
{
    public static class DoiTuong
    {
        public const string SinhVien = "Sinh viên", HocVien = "Học viên", CanBo = "Cán bộ";
        public static readonly string[] TatCa = [SinhVien, HocVien, CanBo];
    }

    public static class TinhTrangSach
    {
        public const string SanSang = "Sẵn sàng", DangDuocMuon = "Đang được mượn", Mat = "Mất", HuHong = "Hư hỏng";
        public static readonly string[] TatCa = [SanSang, DangDuocMuon, Mat, HuHong];
    }

    public static class KetQuaGiaHan
    {
        public const string DongY = "Đồng ý", KhongDongY = "Không đồng ý";
        public static readonly string[] TatCa = [DongY, KhongDongY];
    }

    public static class TinhTrangTra
    {
        public const string BinhThuong = "Bình thường", HuHong = "Hư hỏng", Mat = "Mất";
        public static readonly string[] TatCa = [BinhThuong, HuHong, Mat];
    }

    public static class HinhThucThanhToan
    {
        public const string TienMat = "Tiền mặt", ChuyenKhoan = "Chuyển khoản", ChuaThanhToan = "Chưa thanh toán";
        public static readonly string[] TatCa = [TienMat, ChuyenKhoan, ChuaThanhToan];
    }

    public static class TinhTrangNguoiDung
    {
        public const string HoatDong = "Hoạt động", Khoa = "Khóa";
        public static readonly string[] TatCa = [HoatDong, Khoa];
    }

    public static class LoaiKhuVuc
    {
        public const string KeMo = "Kệ mở", Kho = "Kho";
        public static readonly string[] TatCa = [KeMo, Kho];
    }

    public static class TinhTrangTaiKhoanBanDoc
    {
        public const string ChuaKichHoat = "Chưa kích hoạt", HoatDong = "Hoạt động", Khoa = "Khóa";
        public static readonly string[] TatCa = [ChuaKichHoat, HoatDong, Khoa];
    }

    public static class TrangThaiDatCho
    {
        public const string DangGiu = "Đang giữ", DaNhan = "Đã nhận", DaHuy = "Đã hủy", HetHan = "Hết hạn";
        public static readonly string[] TatCa = [DangGiu, DaNhan, DaHuy, HetHan];
    }

    public static class LoaiThongBao
    {
        public static readonly string[] TatCa =
        [
            "Đặt chỗ thành công", "Sắp hết hạn giữ", "Hết hạn giữ", "Đã nhận sách",
            "Sắp đến hạn trả", "Quá hạn trả", "Biên bản vi phạm",
            "Đặt chỗ bị hủy" // bổ sung 17/11/2026 – GIA_DINH D63: nhân viên hủy đặt chỗ tại quầy
        ];
    }
}

/// <summary>Nhóm người dùng (SPEC 3.1).</summary>
public static class NhomNguoiDungMa
{
    public const string N1 = "N1", N2 = "N2", N3 = "N3", N4 = "N4";
}
