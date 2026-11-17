namespace ThuVien.E2E;

/// <summary>
/// Mã vạch Code128 trên vé giữ chỗ (ma-vach.js – GIA_DINH D69): vẽ SVG trong trình duyệt rồi GIẢI MÃ LẠI ở đây bằng bảng chuẩn
/// Code128 (ISO/IEC 15417) viết độc lập – kiểm tra vùng yên tĩnh, Start B, từng ký hiệu, checksum mod 103, Stop.
/// </summary>
[Collection("TrinhDuyet")]
public class MaVachTests(TrinhDuyet td)
{
    // Bảng độ rộng vạch/khoảng của 107 ký hiệu Code128 (giá trị 0–106) theo tiêu chuẩn
    private static readonly string[] Bang =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213",
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132",
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211",
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331",
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111",
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141",
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141",
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112"
    ];

    /// <summary>Giải mã chuỗi độ rộng (vạch, khoảng xen kẽ, bắt đầu bằng vạch) thành chuỗi Code128B.</summary>
    private static string GiaiMa(List<int> doRong)
    {
        var chuoi = string.Concat(doRong);
        Assert.Equal(1, chuoi.Length % 6); // 6 phần tử × n ký hiệu + Stop 7 phần tử
        var kyHieu = new List<int>();
        for (var i = 0; i + 6 <= chuoi.Length - 7; i += 6)
        {
            var v = Array.IndexOf(Bang, chuoi.Substring(i, 6));
            Assert.True(v >= 0, $"Ký hiệu không hợp lệ ở vị trí {i / 6}: {chuoi.Substring(i, 6)}");
            kyHieu.Add(v);
        }
        Assert.Equal("2331112", chuoi[^7..]);              // Stop
        Assert.Equal(104, kyHieu[0]);                        // Start B
        var duLieu = kyHieu.Skip(1).Take(kyHieu.Count - 2).ToList();
        var tong = 104 + duLieu.Select((v, i) => v * (i + 1)).Sum();
        Assert.Equal(tong % 103, kyHieu[^1]);                // checksum
        // Mỗi ký hiệu dữ liệu = 11 mô-đun, Stop = 13 mô-đun (đúng chuẩn)
        Assert.All(Enumerable.Range(0, kyHieu.Count), k => Assert.Equal(11, chuoi.Substring(k * 6, 6).Sum(c => c - '0')));
        return new string(duLieu.Select(v => (char)(v + 32)).ToArray());
    }

    [Theory]
    [InlineData("DC20261117-0001")]
    [InlineData("DC20261231-9999")]
    [InlineData("Thu vien ~!@#")]
    public async Task VeSvg_GiaiMaLai_DungChuoiGoc(string chuoi)
    {
        await using var ctx = await td.MayTinhAsync();
        var p = await ctx.NewPageAsync();
        await p.GotoAsync(TrinhDuyet.Web + "/kich-hoat");
        var hinh = await p.EvaluateAsync<double[][]>("""
            async (s) => {
              const { veMaVach } = await import('/js/cong/ma-vach.js');
              const svg = veMaVach(s);
              const vb = svg.getAttribute('viewBox').split(' ').map(Number);
              return [[vb[2]], ...[...svg.querySelectorAll('rect')].map((r) => [Number(r.getAttribute('x')), Number(r.getAttribute('width'))])];
            }
            """, chuoi);
        var rongTong = (int)hinh[0][0];
        var vach = hinh.Skip(1).Select(x => (X: (int)x[0], W: (int)x[1])).ToList();
        Assert.Equal(10, vach[0].X);                         // vùng yên tĩnh trái 10 mô-đun
        Assert.Equal(10, rongTong - (vach[^1].X + vach[^1].W)); // vùng yên tĩnh phải 10 mô-đun

        // Dựng lại dãy độ rộng vạch/khoảng từ vị trí các hình chữ nhật
        var doRong = new List<int>();
        for (var i = 0; i < vach.Count; i++)
        {
            doRong.Add(vach[i].W);
            if (i + 1 < vach.Count) doRong.Add(vach[i + 1].X - (vach[i].X + vach[i].W));
        }
        Assert.Equal(chuoi, GiaiMa(doRong));
    }
}
