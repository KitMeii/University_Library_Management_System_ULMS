using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>GIA_DINH D10, D13, D14: login SQL của mỗi service chỉ ghi schema của mình.</summary>
public class RanhGioiServiceTests
{
    private const int KhongCoQuyen = 229; // The %ls permission was denied on the object…
    private const int KhongCoQuyenCot = 230;

    private static async Task<int?> ThuChayAsync(string schemaDichVu, string sql)
    {
        await using var cn = MoiTruong.KetNoi("svc_" + schemaDichVu, MoiTruong.MatKhauDichVu(schemaDichVu));
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
        try
        {
            await using var cmd = new SqlCommand(sql, cn, tx);
            await cmd.ExecuteNonQueryAsync();
            return null;
        }
        catch (SqlException ex)
        {
            return ex.Number;
        }
        finally
        {
            if (tx.Connection is not null) await tx.RollbackAsync();
        }
    }

    [Theory]
    [InlineData("khosach", "UPDATE muontra.BANDOC SET hoten = hoten")]
    [InlineData("vipham", "UPDATE khosach.SACH SET tinhtrang = tinhtrang")]
    [InlineData("muontra", "DELETE FROM vipham.KHOANTHU WHERE 1 = 0")]
    [InlineData("quantri", "UPDATE muontra.PHIEUMUON SET hoten = hoten")]
    [InlineData("muontra", "UPDATE quantri.NGUOIDUNG SET tinhtrang = tinhtrang")]
    public async Task KhongGhiDuocSchemaKhac(string dichVu, string sql) =>
        Assert.Equal(KhongCoQuyen, await ThuChayAsync(dichVu, sql));

    [Theory]
    [InlineData("khosach", "UPDATE khosach.SACH SET tinhtrang = tinhtrang")]
    [InlineData("muontra", "UPDATE muontra.BANDOC SET hoten = hoten")]
    [InlineData("vipham", "UPDATE vipham.KHOANTHU SET dongia = dongia")]
    [InlineData("quantri", "UPDATE quantri.NHOMNGUOIDUNG SET mota = mota")]
    [InlineData("khosach", "SELECT TOP 1 ma_bandoc FROM muontra.BANDOC")]
    [InlineData("vipham", "SELECT TOP 1 so_pmuon FROM muontra.PHIEUMUON")]
    public async Task GhiDuocSchemaMinh_DocDuocSchemaKhac(string dichVu, string sql) =>
        Assert.Null(await ThuChayAsync(dichVu, sql));

    [Theory]
    [InlineData("khosach", "SELECT matkhau FROM quantri.NGUOIDUNG")]
    [InlineData("vipham", "SELECT matkhau FROM muontra.TAIKHOAN_BANDOC")]
    public async Task KhongDocDuocCotMatKhauCuaServiceKhac(string dichVu, string sql) =>
        Assert.Contains(await ThuChayAsync(dichVu, sql), new int?[] { KhongCoQuyen, KhongCoQuyenCot });

    [Theory]
    [InlineData("khosach")]
    [InlineData("muontra")]
    [InlineData("vipham")]
    public async Task MoiServiceGhiDuocNhatKyHoatDong(string dichVu) =>
        Assert.Null(await ThuChayAsync(dichVu, """
            DECLARE @so bigint = NEXT VALUE FOR quantri.SEQ_NHATKY_HOATDONG;
            INSERT INTO quantri.NHATKY_HOATDONG (ma_hoatdong, ma_nd, tg_thuchien, noidung)
            VALUES (CONCAT('HDTEST', RIGHT(CONCAT('000000000', @so), 9)), 'ND0001', SYSDATETIME(), N'Kiểm thử ranh giới');
            """));

    [Fact]
    public async Task CapSoChungTu_TangDan_TrongTransaction()
    {
        await using var cn = MoiTruong.KetNoi("svc_vipham", MoiTruong.MatKhauDichVu("vipham"));
        await cn.OpenAsync();
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync();
        async Task<int> CapAsync()
        {
            await using var cmd = new SqlCommand("muontra.usp_CapSoChungTu", cn, tx) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@loai", "BB");
            cmd.Parameters.AddWithValue("@ngay", new DateTime(2099, 1, 15));
            var so = cmd.Parameters.Add("@so", System.Data.SqlDbType.Int);
            so.Direction = System.Data.ParameterDirection.Output;
            await cmd.ExecuteNonQueryAsync();
            return (int)so.Value;
        }
        var a = await CapAsync();
        var b = await CapAsync();
        Assert.Equal(a + 1, b);
        await tx.RollbackAsync();
    }
}
