using System.Security.Claims;
using BaseCore.Common.Auth;

namespace BaseCore.UnitTests;

/// <summary>GIA_DINH D36 – quy tắc thu hồi phiên sau phục hồi dữ liệu.</summary>
public class MocThuHoiPhienTests
{
    private static ClaimsPrincipal NguoiDung(string? maDangNhap) =>
        new(new ClaimsIdentity(maDangNhap is null ? [] : [new Claim(ThuVienClaims.MaDangNhap, maDangNhap)], "test"));

    [Fact]
    public void MaHoa_GiaiMa_KhuHoi()
    {
        var moc = new DateTimeOffset(2026, 11, 16, 3, 0, 0, TimeSpan.Zero);
        var tt = MocThuHoiPhien.GiaiMa(MocThuHoiPhien.MaHoa(moc, "DN2611160000001"));
        Assert.Equal(moc.ToUnixTimeSeconds(), tt!.MocUnix);
        Assert.Equal("DN2611160000001", tt.MaDangNhapMienTru);
        Assert.Null(MocThuHoiPhien.GiaiMa(MocThuHoiPhien.MaHoa(moc, null))!.MaDangNhapMienTru);
        Assert.Null(MocThuHoiPhien.GiaiMa(null));
        Assert.Null(MocThuHoiPhien.GiaiMa("rác"));
    }

    [Fact]
    public void TokenCapTruocMoc_BiThuHoi_TruPhienMienTru()
    {
        var moc = new MocThuHoiPhien.ThongTin(1_000, "DN_ADMIN");
        Assert.True(MocThuHoiPhien.BiThuHoi(moc, NguoiDung("DN_KHAC"), 999));
        Assert.True(MocThuHoiPhien.BiThuHoi(moc, NguoiDung(null), 999));      // bạn đọc (không có ma_dangnhap)
        Assert.True(MocThuHoiPhien.BiThuHoi(moc, NguoiDung("DN_KHAC"), null)); // token không có iat
        Assert.False(MocThuHoiPhien.BiThuHoi(moc, NguoiDung("DN_ADMIN"), 999)); // người thực hiện phục hồi
        Assert.False(MocThuHoiPhien.BiThuHoi(moc, NguoiDung("DN_KHAC"), 1_000)); // đăng nhập lại sau mốc
        Assert.False(MocThuHoiPhien.BiThuHoi(null, NguoiDung("DN_KHAC"), 1));    // chưa từng phục hồi
    }
}
