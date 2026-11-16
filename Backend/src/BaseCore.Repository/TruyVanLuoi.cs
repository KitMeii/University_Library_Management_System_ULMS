using System.Linq.Expressions;
using BaseCore.DTO.Common;
using BaseCore.Libs.Utils;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.Repository;

/// <summary>Phân trang + sắp xếp phía server cho mọi lưới (file 11 mục 3).</summary>
public static class TruyVanLuoi
{
    /// <summary>
    /// Sắp xếp theo <paramref name="ts"/>.SapXep nếu nằm trong danh sách cho phép (tên thuộc tính C#), ngược lại theo
    /// <paramref name="macDinh"/>; rồi lấy một trang.
    /// </summary>
    public static async Task<KetQuaTrang<TKq>> LayTrangAsync<T, TKq>(
        this IQueryable<T> q, ThamSoLuoi ts, string[] choPhepSapXep, string macDinh,
        Expression<Func<T, TKq>> chieu, CancellationToken ct)
    {
        var tong = await q.CountAsync(ct);
        var cot = choPhepSapXep.FirstOrDefault(c => string.Equals(c, ts.SapXep, StringComparison.OrdinalIgnoreCase)) ?? macDinh;
        var ds = await q.OrderByField(cot, ts.Tang).Skip(ts.BoQua).Take(ts.CoTrangHopLe).Select(chieu).ToListAsync(ct);
        return new KetQuaTrang<TKq>(tong, ts.TrangHopLe, ts.CoTrangHopLe, ds);
    }

    /// <summary>Lỗi trùng khóa chính/duy nhất của SQL Server (2627, 2601).</summary>
    public static bool LaTrungKhoa(this DbUpdateException ex) =>
        ex.InnerException is Microsoft.Data.SqlClient.SqlException s && s.Number is 2627 or 2601;
}
