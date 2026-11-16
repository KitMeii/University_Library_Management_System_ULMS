using BaseCore.Repository;
using BaseCore.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BaseCore.UnitTests;

/// <summary>
/// Mô hình EF (Fluent API) khớp SPEC mục 2: đúng 18 bảng, đúng tên cột, kiểu, độ dài, khóa chính.
/// (Bản đối chiếu với CSDL thật nằm ở IntegrationTests/SchemaMatchesSpecTests.)
/// </summary>
public class MoHinhKhopSpecTests
{
    private static readonly IModel Model = TaoModel();

    private static IModel TaoModel()
    {
        var o = new DbContextOptionsBuilder<ThuVienDbContext>().UseSqlServer("Server=.;Database=x").Options;
        using var db = new ThuVienDbContext(o);
        return db.GetService<IDesignTimeModel>().Model;
    }

    public static TheoryData<string> Bang18 => new(SpecDoc.BangGoc().Keys);

    [Fact]
    public void Spec_Co18Bang() => Assert.Equal(18, SpecDoc.BangGoc().Count);

    [Theory]
    [MemberData(nameof(Bang18))]
    public void BangGoc_DungCotKieuKhoa(string bang)
    {
        var cot = SpecDoc.BangGoc()[bang];
        var et = Model.GetEntityTypes().SingleOrDefault(e => e.GetTableName() == bang);
        Assert.True(et is not null, $"Thiếu bảng {bang}");
        var bangSql = StoreObjectIdentifier.Table(bang, et!.GetSchema());

        var cotModel = et.GetProperties().Select(p => p.GetColumnName(bangSql)!).OrderBy(x => x, StringComparer.Ordinal);
        Assert.Equal(cot.Select(c => c.Ten).OrderBy(x => x, StringComparer.Ordinal), cotModel);

        foreach (var c in cot)
        {
            var p = et.GetProperties().Single(x => x.GetColumnName(bangSql) == c.Ten);
            var (kieu, doDai) = SpecDoc.KieuSql(c);
            var kieuModel = p.GetColumnType(bangSql);
            if (kieu == "nvarchar") Assert.Equal($"nvarchar({doDai})", kieuModel);
            else if (kieu == "decimal") Assert.Equal("decimal(18,0)", kieuModel);
            else if (kieu == "datetime2") Assert.Equal("datetime2(0)", kieuModel);
            else Assert.Equal(kieu, kieuModel);
        }

        var khoa = et.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName(bangSql)).OrderBy(x => x, StringComparer.Ordinal);
        Assert.Equal(cot.Where(c => c.LaKhoa).Select(c => c.Ten).OrderBy(x => x, StringComparer.Ordinal), khoa);
    }

    /// <summary>8 bảng mở rộng file 02 (X01–X08) + X09 XULY_CHUYENDEN (GIA_DINH D47, file 02 X09) → 27 bảng.</summary>
    [Fact]
    public void Co9BangMoRong_File02_D47()
    {
        var mo = new[] { "THELOAI", "DAUSACH", "DAUSACH_BANSACH", "TAIKHOAN_BANDOC", "PHIEUDATCHO", "YEUTHICH", "THONGBAO", "SOCHUNGTU", "XULY_CHUYENDEN" };
        var bang = Model.GetEntityTypes().Select(e => e.GetTableName()).ToHashSet();
        Assert.All(mo, b => Assert.Contains(b, bang));
        Assert.Equal(27, bang.Count);
    }
}
