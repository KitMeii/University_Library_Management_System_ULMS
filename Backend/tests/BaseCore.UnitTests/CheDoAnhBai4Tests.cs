using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using ThuVien.Web.HaTang;

namespace BaseCore.UnitTests;

/// <summary>
/// Chế độ chụp ảnh Bài 4 (GIA_DINH D61) và cookie kiểm thử mở rộng (D65): cookie CHỈ có tác dụng ở Development;
/// Production luôn hiện đủ phần mở rộng (file 08), có hay không có cookie, có hay không ẩn nhãn DỮ LIỆU MẪU.
/// </summary>
public class CheDoAnhBai4Tests
{
    private sealed class MoiTruongGia(string ten) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = ten;
        public string ApplicationName { get; set; } = "ThuVien.Web";
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static HttpContext Ctx(string moiTruong, bool hienNhan, bool coCookie)
    {
        var sp = new ServiceCollection()
            .AddSingleton<IWebHostEnvironment>(new MoiTruongGia(moiTruong))
            .AddSingleton(Options.Create(new ThuVienOptions { HienNhanDuLieuMau = hienNhan }))
            .BuildServiceProvider();
        var ctx = new DefaultHttpContext { RequestServices = sp };
        if (coCookie) ctx.Request.Headers.Cookie = CheDoAnhBai4.CookieKiemThuMoRong + "=1";
        return ctx;
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Production_LuonHienPhanMoRong_CookieKhongCoTacDung(bool hienNhan, bool coCookie) =>
        Assert.False(CheDoAnhBai4.AnMoRong(Ctx("Production", hienNhan, coCookie)));

    [Fact]
    public void Development_AnNhan_AnPhanMoRong_TruKhiCoCookieKiemThu()
    {
        Assert.True(CheDoAnhBai4.AnMoRong(Ctx("Development", hienNhan: false, coCookie: false)));
        Assert.False(CheDoAnhBai4.AnMoRong(Ctx("Development", hienNhan: false, coCookie: true)));
        Assert.False(CheDoAnhBai4.AnMoRong(Ctx("Development", hienNhan: true, coCookie: false)));
    }

    [Fact]
    public void MenuMoRong_AnTheoCheDo()
    {
        var admin = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            new[] { "DAUSACH_R", "THELOAI_R", "DATCHO_R", "SACH_R" }.Select(q => new System.Security.Claims.Claim(BaseCore.Common.Auth.ThuVienClaims.Quyen, q)), "kt"));
        var day = MenuQuanTri.ChoNguoiDung(admin).SelectMany(n => n.Muc).ToList();
        var an = MenuQuanTri.ChoNguoiDung(admin, anMoRong: true).SelectMany(n => n.Muc).ToList();
        Assert.Contains(day, m => m.MoRong);
        Assert.DoesNotContain(an, m => m.MoRong);
        Assert.Contains(an, m => m.DuongDan == "/quan-tri/danh-muc/sach");
    }
}
