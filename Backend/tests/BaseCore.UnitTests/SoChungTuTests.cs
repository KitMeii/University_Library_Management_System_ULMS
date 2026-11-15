using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;

namespace BaseCore.UnitTests;

/// <summary>GIA_DINH D4: {PM|GH|PT|BB}yyMM#### = 10 ký tự, đánh lại mỗi tháng.</summary>
public class SoChungTuTests
{
    [Theory]
    [InlineData("PM", 2026, 11, 3, 1, "PM26110001")]
    [InlineData("GH", 2026, 1, 31, 25, "GH26010025")]
    [InlineData("PT", 2027, 12, 1, 9999, "PT27129999")]
    [InlineData("BB", 2026, 11, 30, 120, "BB26110120")]
    public void Tao_DungDinhDang(string loai, int y, int m, int d, int so, string mong)
    {
        var s = SoChungTu.Tao(loai, new DateOnly(y, m, d), so);
        Assert.Equal(mong, s);
        Assert.Equal(10, s.Length); // khớp C(10) của so_pmuon, so_pgiahan, so_ptra, so_bienban
        Assert.True(SoChungTu.HopLe(s));
    }

    [Fact]
    public void NgayKhoa_LaNgayDauThang_DanhLaiMoiThang()
    {
        Assert.Equal(new DateOnly(2026, 11, 1), SoChungTu.NgayKhoa(new DateOnly(2026, 11, 29)));
        Assert.NotEqual(SoChungTu.NgayKhoa(new DateOnly(2026, 11, 30)), SoChungTu.NgayKhoa(new DateOnly(2026, 12, 1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10000)]
    public void SoThuTuNgoaiKhoang_BaoLoi(int so) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => SoChungTu.Tao("PM", new DateOnly(2026, 11, 1), so));

    [Fact]
    public void LoaiKhongHopLe_BaoLoi() =>
        Assert.Throws<ArgumentException>(() => SoChungTu.Tao("XX", new DateOnly(2026, 11, 1), 1));

    [Theory]
    [InlineData("PM2611-001")]
    [InlineData("PM26130001")]
    [InlineData("PMyyyyMMdd")]
    [InlineData(null)]
    public void HopLe_TuChoiSaiDinhDang(string? s) => Assert.False(SoChungTu.HopLe(s));

    [Fact]
    public void DongHo_GioVietNam_LechUtc7Gio()
    {
        var gia = new GiaThoiGian(new DateTimeOffset(2026, 11, 15, 20, 30, 0, TimeSpan.Zero));
        var c = new DongHoHeThong(gia);
        Assert.Equal(new DateTime(2026, 11, 16, 3, 30, 0), c.BayGio);
        Assert.Equal(new DateOnly(2026, 11, 16), ((IClock)c).HomNay);
    }

    private sealed class GiaThoiGian(DateTimeOffset t) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => t;
    }
}
