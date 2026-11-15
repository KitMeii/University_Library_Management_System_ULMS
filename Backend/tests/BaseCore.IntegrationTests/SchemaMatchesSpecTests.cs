using BaseCore.Tests.Shared;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// File 02 mục 1: đọc metadata SQL Server, so với danh sách bảng/cột trích từ SPEC mục 2 → khớp 100%.
/// CSDL có 4 schema (GIA_DINH D10) → so trên HỢP các schema.
/// </summary>
public class SchemaMatchesSpecTests
{
    private sealed record CotDb(string Schema, string Bang, string Cot, string Kieu, int? DoDai, int? DoChinhXac, int? Thang);

    private static async Task<List<CotDb>> DocCotAsync()
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand("""
            SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA IN ('quantri','khosach','muontra','vipham')
            """, cn);
        var ds = new List<CotDb>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            ds.Add(new CotDb(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
                r.IsDBNull(4) ? null : r.GetInt32(4), r.IsDBNull(5) ? null : r.GetByte(5), r.IsDBNull(6) ? null : r.GetInt32(6)));
        return ds;
    }

    [Fact]
    public async Task Du18BangGoc_DungTenCotKieuDoDai()
    {
        var db = await DocCotAsync();
        var spec = SpecDoc.BangGoc();
        Assert.Equal(18, spec.Count);
        foreach (var (bang, cotSpec) in spec)
        {
            var cotDb = db.Where(c => c.Bang == bang).ToList();
            Assert.True(cotDb.Count > 0, $"CSDL thiếu bảng {bang}");
            Assert.Equal(cotSpec.Select(c => c.Ten).Order(StringComparer.Ordinal), cotDb.Select(c => c.Cot).Order(StringComparer.Ordinal));
            foreach (var c in cotSpec)
            {
                var d = cotDb.Single(x => x.Cot == c.Ten);
                var (kieu, doDai) = SpecDoc.KieuSql(c);
                Assert.True(d.Kieu == kieu, $"{bang}.{c.Ten}: kiểu {d.Kieu}, mong {kieu}");
                if (doDai is not null) Assert.True(d.DoDai == doDai, $"{bang}.{c.Ten}: độ dài {d.DoDai}, mong {doDai}");
                if (kieu == "decimal") Assert.True(d.DoChinhXac == 18 && d.Thang == 0, $"{bang}.{c.Ten} phải decimal(18,0)");
            }
        }
    }

    [Fact]
    public async Task Co8BangMoRong_VaKhongCoBangLa()
    {
        var bang = (await DocCotAsync()).Select(c => c.Bang).ToHashSet();
        var mong = SpecDoc.BangGoc().Keys
            .Concat(["THELOAI", "DAUSACH", "DAUSACH_BANSACH", "TAIKHOAN_BANDOC", "PHIEUDATCHO", "YEUTHICH", "THONGBAO", "SOCHUNGTU"])
            .ToHashSet();
        Assert.Equal(mong.Order(), bang.Order());
    }

    [Fact]
    public async Task KhoaNgoaiGiuaSchema_TonTai()
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand("""
            SELECT COUNT(*) FROM sys.foreign_keys fk
            WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) <> OBJECT_SCHEMA_NAME(fk.referenced_object_id)
            """, cn);
        var so = (int)(await cmd.ExecuteScalarAsync())!;
        Assert.True(so >= 8, $"Chỉ có {so} khóa ngoại giữa các schema (GIA_DINH D14 giữ đủ FK SPEC)");
    }

    [Fact]
    public async Task ChiMucLoc_DatCho_TonTai()
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand("""
            SELECT name FROM sys.indexes WHERE has_filter = 1 AND is_unique = 1
              AND name IN ('UX_DATCHO_BANSACH_DANGGIU','UX_DATCHO_BANDOC_DAUSACH_DANGGIU')
            """, cn);
        var ds = new List<string>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) ds.Add(r.GetString(0));
        Assert.Equal(2, ds.Count);
    }
}
