using BaseCore.Common.NghiepVu;
using BaseCore.Entities;
using TTDC = BaseCore.Entities.MienGiaTri.TrangThaiDatCho;

namespace BaseCore.UnitTests;

/// <summary>Vòng đời đặt chỗ (file 06 mục 3) với đồng hồ giả – tua thời gian qua 24 giờ.</summary>
public class VongDoiDatChoTests
{
    private static readonly DateTime Dat = new(2026, 11, 17, 9, 30, 0);

    [Fact]
    public void HetHan_SauDung24Gio()
    {
        Assert.Equal(new DateTime(2026, 11, 18, 9, 30, 0), VongDoiDatCho.HetHan(Dat, new DatChoOptions().SoGioGiu));
        Assert.Equal(24, new DatChoOptions().SoGioGiu);
        Assert.Equal(3, new DatChoOptions().ToiDaDangGiu);
        Assert.Equal(2, new DatChoOptions().NhacTruocGio);
        Assert.Equal(5, new DatChoOptions().ChuKyQuetPhut);
    }

    [Fact]
    public void TuaQua24Gio_DangGiuThanhHetHan_KhongPhuThuocTacVuNen()
    {
        var het = VongDoiDatCho.HetHan(Dat, 24);
        Assert.True(VongDoiDatCho.ConGiu(TTDC.DangGiu, het, Dat.AddHours(23).AddMinutes(59)));
        Assert.Equal(TTDC.DangGiu, VongDoiDatCho.TrangThaiHieuLuc(TTDC.DangGiu, het, Dat.AddHours(23)));
        // Đúng mốc hết hạn và sau đó: coi như đã hết dù tác vụ nền chưa quét
        Assert.False(VongDoiDatCho.ConGiu(TTDC.DangGiu, het, het));
        Assert.Equal(TTDC.HetHan, VongDoiDatCho.TrangThaiHieuLuc(TTDC.DangGiu, het, Dat.AddHours(25)));
        // Trạng thái kết thúc không bị đổi bởi thời gian
        Assert.Equal(TTDC.DaNhan, VongDoiDatCho.TrangThaiHieuLuc(TTDC.DaNhan, het, Dat.AddDays(5)));
        Assert.False(VongDoiDatCho.ConGiu(TTDC.DaHuy, het, Dat));
    }

    [Theory]
    [InlineData(TTDC.DangGiu, TTDC.DaNhan, true)]
    [InlineData(TTDC.DangGiu, TTDC.DaHuy, true)]
    [InlineData(TTDC.DangGiu, TTDC.HetHan, true)]
    [InlineData(TTDC.DangGiu, TTDC.DangGiu, false)]
    [InlineData(TTDC.DaNhan, TTDC.DaHuy, false)]
    [InlineData(TTDC.HetHan, TTDC.DaNhan, false)]
    [InlineData(TTDC.DaHuy, TTDC.HetHan, false)]
    public void ChiDangGiuMoiChuyenDuoc(string tu, string den, bool hopLe) =>
        Assert.Equal(hopLe, VongDoiDatCho.ChuyenHopLe(tu, den));

    [Fact]
    public void NhacSapHetHan_KhiConToiDa2Gio_VaConGiu()
    {
        var het = VongDoiDatCho.HetHan(Dat, 24);
        Assert.False(VongDoiDatCho.CanNhac(TTDC.DangGiu, het, het.AddHours(-3), 2));
        Assert.True(VongDoiDatCho.CanNhac(TTDC.DangGiu, het, het.AddHours(-2), 2));
        Assert.True(VongDoiDatCho.CanNhac(TTDC.DangGiu, het, het.AddMinutes(-1), 2));
        Assert.False(VongDoiDatCho.CanNhac(TTDC.DangGiu, het, het, 2));       // đã hết hạn → không nhắc nữa
        Assert.False(VongDoiDatCho.CanNhac(TTDC.DaNhan, het, het.AddHours(-1), 2));
    }

    [Fact]
    public void MaDatCho_DangDCyyyyMMdd_4So_TheoNgay()
    {
        Assert.Equal("DC20261117-0001", VongDoiDatCho.TaoMa(new DateOnly(2026, 11, 17), 1));
        Assert.Equal("DC20261231-9999", VongDoiDatCho.TaoMa(new DateOnly(2026, 12, 31), 9999));
        Assert.Equal(15, VongDoiDatCho.TaoMa(new DateOnly(2026, 1, 1), 42).Length); // ≤ PHIEUDATCHO.ma_datcho nvarchar(20)
        Assert.Throws<ArgumentOutOfRangeException>(() => VongDoiDatCho.TaoMa(new DateOnly(2026, 1, 1), 10000));
        Assert.Throws<ArgumentOutOfRangeException>(() => VongDoiDatCho.TaoMa(new DateOnly(2026, 1, 1), 0));
    }
}
