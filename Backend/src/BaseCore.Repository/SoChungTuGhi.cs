using System.Data;
using BaseCore.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BaseCore.Repository;

/// <summary>
/// Cấp số chứng từ (SPEC mục 4, GIA_DINH D4) KHI LƯU/IN, trong transaction nghiệp vụ hiện tại: gọi
/// <c>muontra.usp_CapSoChungTu</c> (khóa dòng bộ đếm UPDLOCK, HOLDLOCK) → hai người lập cùng lúc không trùng số;
/// transaction rollback thì số cũng được trả lại (không “mất số”).
/// </summary>
public static class SoChungTuGhi
{
    /// <returns>Số thứ tự trong kỳ (tháng); ghép thành số chứng từ bằng <c>SoChungTu.Tao</c>.</returns>
    public static async Task<int> CapAsync(DbContext db, string loai, DateOnly ngay, CancellationToken ct = default)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Cấp số chứng từ phải nằm trong transaction của nghiệp vụ lập chứng từ.");
        var cn = db.Database.GetDbConnection();
        await using var cmd = cn.CreateCommand();
        cmd.CommandText = $"[{LuocDo.MuonTra}].[usp_CapSoChungTu]";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Transaction = db.Database.CurrentTransaction.GetDbTransaction();
        cmd.Parameters.Add(new SqlParameter("@loai", SqlDbType.NVarChar, 4) { Value = loai });
        cmd.Parameters.Add(new SqlParameter("@ngay", SqlDbType.Date) { Value = ngay.ToDateTime(TimeOnly.MinValue) });
        var so = new SqlParameter("@so", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(so);
        await cmd.ExecuteNonQueryAsync(ct);
        return (int)so.Value;
    }
}
