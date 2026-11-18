using ThuVien.Web.HaTang;

namespace BaseCore.UnitTests;

/// <summary>D75: giờ mở cửa gộp các ngày liền nhau cùng giờ theo thứ tự trong tuần; trường trống không hiện.</summary>
public class ThongTinThuVienTests
{
    private static ThongTinThuVien Mau() => new()
    {
        // Thứ tự khóa cố ý xáo trộn (cấu hình từ biến môi trường được sắp theo tên khóa)
        GioMoCua = new()
        {
            ["CN"] = "Nghỉ", ["T7"] = "7:30 – 17:00", ["T2"] = "7:00 – 20:00", ["T3"] = "7:00 – 20:00",
            ["T4"] = "7:00 – 20:00", ["T5"] = "7:00 – 20:00", ["T6"] = "7:00 – 20:00"
        }
    };

    [Fact]
    public void GioGop_GopNgayLienNhau_DungThuTu_DanhDauHomNay()
    {
        var kq = Mau().GioGop("T3");
        Assert.Equal(3, kq.Count);
        Assert.Equal(("Thứ 2 – Thứ 6", "7:00 – 20:00", true), kq[0]);
        Assert.Equal(("Thứ 7", "7:30 – 17:00", false), kq[1]);
        Assert.Equal(("Chủ nhật", "Nghỉ", false), kq[2]);
    }

    [Fact]
    public void GioGop_BoNgayTrong_KhongGopQuaNgayTrong()
    {
        var tt = new ThongTinThuVien { GioMoCua = new() { ["T2"] = "8:00 – 17:00", ["T3"] = " ", ["T4"] = "8:00 – 17:00" } };
        var kq = tt.GioGop("CN");
        Assert.Equal(["Thứ 2", "Thứ 4"], kq.Select(x => x.Nhan));
        Assert.True(tt.CoGioMoCua);
        Assert.False(new ThongTinThuVien { GioMoCua = new() { ["T2"] = "" } }.CoGioMoCua);
        Assert.Null(tt.GioCua("T3"));
    }

    [Fact]
    public void CoLienHe_ChiKhiCoTruong()
    {
        Assert.False(new ThongTinThuVien { TenTruong = "X" }.CoLienHe);
        Assert.True(new ThongTinThuVien { Email = "thuvien@truong.edu.vn" }.CoLienHe);
        Assert.Contains(ThongTinThuVien.ThuHomNay(), new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" });
    }
}
