using BaseCore.Entities;
using BaseCore.Tests.Shared;

namespace BaseCore.UnitTests;

/// <summary>Dữ liệu QUYEN/PHANQUYEN nạp vào CSDL phải khớp SPEC 3.2, 3.4 (đọc trực tiếp từ SPEC).</summary>
public class DuLieuQuyenTests
{
    private static readonly Dictionary<string, QuyenKhoiTao> Theo = DuLieuQuyen.TatCa.ToDictionary(q => q.MaQuyen);

    [Fact]
    public void MaQuyen_KhongTrung_VaVuaCot()
    {
        Assert.Equal(DuLieuQuyen.TatCa.Length, Theo.Count);
        Assert.All(DuLieuQuyen.TatCa, q =>
        {
            Assert.InRange(q.MaQuyen.Length, 1, 10);   // QUYEN.ma_quyen C(10)
            Assert.InRange(q.TenQuyen.Length, 1, 100); // ten_quyen C(100)
            Assert.InRange(q.MoTa.Length, 0, 200);     // mota C(200)
        });
    }

    [Fact]
    public void Du122TienTrinh_DungNhomTheoSpec34()
    {
        var spec = SpecDoc.QuyenTienTrinh();
        Assert.Equal(122, spec.Count);
        foreach (var (stt, nhom) in spec)
        {
            var ma = $"TT{stt:D3}";
            Assert.True(Theo.ContainsKey(ma), $"Thiếu {ma}");
            Assert.Equal(nhom.OrderBy(x => x), Theo[ma].Nhom.OrderBy(x => x));
        }
    }

    [Fact]
    public void QuyenTrenBang_DungSpec32()
    {
        var tienTo = new Dictionary<string, string>
        {
            ["B01"] = "SACH", ["B02"] = "KHOANTHU", ["B03"] = "BANDOC", ["B04"] = "KHUVUC",
            ["B05"] = "PMUON", ["B06"] = "PGHAN", ["B07"] = "PTRA", ["B08"] = "BIENBAN"
        };
        foreach (var (bang, theoNhom) in SpecDoc.QuyenBang())
        {
            foreach (var tt in new[] { "C", "E", "R", "D" })
            {
                var ma = $"{tienTo[bang]}_{tt}";
                var mong = theoNhom.Where(kv => kv.Value.Contains(tt)).Select(kv => kv.Key).OrderBy(x => x);
                Assert.Equal(mong, Theo[ma].Nhom.OrderBy(x => x));
            }
        }
    }

    [Fact]
    public void QuyenQuanTri_ChiN4()
    {
        for (var i = 1; i <= 9; i++) Assert.Equal(["N4"], Theo[$"QT04000{i}"].Nhom);
    }
}
