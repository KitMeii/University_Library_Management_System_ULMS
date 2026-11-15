using BaseCore.Common.Auth;

namespace BaseCore.UnitTests;

/// <summary>GIA_DINH D5: PBKDF2-SHA256, salt 16 + khóa 32 → Base64 đúng 64 ký tự (NGUOIDUNG.matkhau C(64)).</summary>
public class MatKhauHasherTests
{
    private readonly MatKhauHasher _h = new(1_000); // số vòng nhỏ cho kiểm thử (cấu hình được)

    [Fact]
    public void Bam_TraVeDung64KyTu()
    {
        var s = _h.Bam("Thuvien@123");
        Assert.Equal(64, s.Length);
        Assert.Equal(48, Convert.FromBase64String(s).Length);
    }

    [Fact]
    public void KiemTra_DungMatKhau_True_SaiMatKhau_False()
    {
        var s = _h.Bam("Thuvien@123");
        Assert.True(_h.KiemTra("Thuvien@123", s));
        Assert.False(_h.KiemTra("thuvien@123", s));
        Assert.False(_h.KiemTra("", s));
    }

    [Fact]
    public void MoiLanBam_SaltKhacNhau()
    {
        Assert.NotEqual(_h.Bam("abc12345"), _h.Bam("abc12345"));
    }

    [Fact]
    public void KhacSoVongLap_KhongKhop()
    {
        var s = _h.Bam("abc12345");
        Assert.False(new MatKhauHasher(1_001).KiemTra("abc12345", s));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("khong-phai-base64")]
    [InlineData("QUJD")]
    public void ChuoiLuuKhongHopLe_False(string? luu) => Assert.False(_h.KiemTra("abc12345", luu));

    [Fact]
    public void ChuoiBiSua_False()
    {
        var s = _h.Bam("abc12345").ToCharArray();
        s[40] = s[40] == 'A' ? 'B' : 'A';
        Assert.False(_h.KiemTra("abc12345", new string(s)));
    }

    [Theory]
    [InlineData("Thuvien@123", true)]
    [InlineData("abcd1234", true)]
    [InlineData("abcdefgh", false)]
    [InlineData("12345678", false)]
    [InlineData("abc123", false)]
    [InlineData(null, false)]
    public void ChinhSachMatKhau(string? mk, bool dat) => Assert.Equal(dat, MatKhauHasher.DatChinhSach(mk));

    [Fact]
    public void MacDinh600000VongLap() => Assert.Equal(600_000, new MatKhauOptions().SoVongLap);
}
