using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BaseCore.MuonTraService.DuLieu;

/// <summary>
/// Gợi ý kế hoạch truy vấn cho đúng các truy vấn được gắn thẻ (EF Core không hỗ trợ query hint trực tiếp).
/// Pha 8 hiệu năng (GIA_DINH D82): đếm/gộp lượt trả theo ngày trong kỳ (CT_PHIEUTRA ⋈ PHIEUTRA) – SQL Server chọn hash join quét
/// 200.000 dòng (~100 ms); nested loop qua chỉ mục IX_PHIEUTRA_ngaytra + IX_CT_PHIEUTRA_so_ptra chỉ ~20 ms.
/// Chỉ áp dụng khi CommandText có thẻ <see cref="LoopJoin"/> (gắn bằng <c>TagWith</c>); mọi truy vấn khác giữ nguyên.
/// </summary>
public sealed class GoiYTruyVanInterceptor : DbCommandInterceptor
{
    public const string LoopJoin = "ULMS:LOOP_JOIN";

    private static void ThemGoiY(DbCommand lenh)
    {
        if (lenh.CommandText.Contains(LoopJoin, StringComparison.Ordinal) && !lenh.CommandText.Contains("OPTION (", StringComparison.Ordinal))
            lenh.CommandText = lenh.CommandText.TrimEnd().TrimEnd(';') + " OPTION (LOOP JOIN)";
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ThemGoiY(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        ThemGoiY(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        ThemGoiY(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        ThemGoiY(command);
        return ValueTask.FromResult(result);
    }
}
