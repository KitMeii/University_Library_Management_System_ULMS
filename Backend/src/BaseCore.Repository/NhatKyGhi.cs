using System.Globalization;
using BaseCore.Entities;
using BaseCore.Entities.QuanTri;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BaseCore.Repository;

/// <summary>
/// Sinh mã nhật ký (GIA_DINH D19) và thêm dòng NHATKY_HOATDONG trong transaction nghiệp vụ hiện tại.
/// Mọi service được INSERT vào NHATKY_HOATDONG (GIA_DINH D13).
/// </summary>
public static class NhatKyGhi
{
    public static async Task<string> SinhMaAsync(DbContext db, string tienTo, string sequence, DateTime bayGio, CancellationToken ct = default)
    {
        // NEXT VALUE FOR không được phép trong subquery (SqlQueryRaw bọc subquery) → dùng lệnh ADO.NET trực tiếp,
        // tham gia transaction hiện tại của DbContext. Tên sequence là hằng số nội bộ nên ghép chuỗi an toàn.
        var cn = db.Database.GetDbConnection();
        var tuMo = cn.State != System.Data.ConnectionState.Open;
        if (tuMo) await db.Database.OpenConnectionAsync(ct);
        long so;
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = $"SELECT NEXT VALUE FOR [{LuocDo.QuanTri}].[{sequence}]";
            cmd.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
            so = Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));
        }
        finally
        {
            if (tuMo) await db.Database.CloseConnectionAsync();
        }
        // 2 + 6 + 7 = 15 ký tự = C(15)
        return string.Create(CultureInfo.InvariantCulture, $"{tienTo}{bayGio:yyMMdd}{so % 10_000_000:D7}");
    }

    /// <summary>Thêm 1 dòng nhật ký (chưa SaveChanges – lưu cùng thao tác nghiệp vụ).</summary>
    public static async Task<NhatKyHoatDong> ThemAsync(ThuVienDbContext db, string maNd, string noiDung, DateTime bayGio, CancellationToken ct = default)
    {
        var dong = new NhatKyHoatDong
        {
            MaHoatDong = await SinhMaAsync(db, "HD", ThuVienDbContext.SeqNhatKyHoatDong, bayGio, ct),
            MaNd = maNd,
            TgThucHien = bayGio,
            NoiDung = noiDung.Length > 500 ? noiDung[..500] : noiDung
        };
        db.NhatKyHoatDong.Add(dong);
        return dong;
    }
}
