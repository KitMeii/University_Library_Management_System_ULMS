using System.Data;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.Repository;

/// <summary>
/// Chạy một nghiệp vụ ghi trong MỘT transaction (file 02 mục 5) qua execution strategy của EF
/// (bắt buộc khi bật EnableRetryOnFailure: cả khối được thử lại nguyên vẹn nếu lỗi kết nối tạm thời).
/// </summary>
public static class GiaoDich
{
    public static Task<T> ChayAsync<T>(this DbContext db, Func<CancellationToken, Task<T>> viec, CancellationToken ct,
        IsolationLevel mucCachLy = IsolationLevel.ReadCommitted) =>
        db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(mucCachLy, ct);
            var kq = await viec(ct);
            await tx.CommitAsync(ct);
            return kq;
        });

    public static Task ChayAsync(this DbContext db, Func<CancellationToken, Task> viec, CancellationToken ct,
        IsolationLevel mucCachLy = IsolationLevel.ReadCommitted) =>
        db.ChayAsync<bool>(async c => { await viec(c); return true; }, ct, mucCachLy);
}
