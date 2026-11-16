using Microsoft.Data.SqlClient;

namespace BaseCore.Migrator;

/// <summary>
/// Hỗ trợ bộ kiểm thử (scripts/test-docker.sh – GIA_DINH D49): kiểm thử chạy trên CSDL riêng (tên kết thúc “_Test”),
/// không bao giờ đụng CSDL ThuVien. Mọi thao tác xóa đều bị chặn nếu tên CSDL không phải CSDL kiểm thử.
/// </summary>
public static class CsdlKiemThu
{
    public static bool LaCsdlKiemThu(string ten) => ten.EndsWith("_Test", StringComparison.OrdinalIgnoreCase);

    private static (string Ten, string ChuoiMaster) TachChuoi(string chuoiKetNoi)
    {
        var b = new SqlConnectionStringBuilder(chuoiKetNoi);
        var ten = b.InitialCatalog;
        if (!LaCsdlKiemThu(ten))
            throw new InvalidOperationException($"Chỉ được xóa/tạo lại CSDL kiểm thử (tên kết thúc “_Test”); từ chối thao tác trên CSDL “{ten}”.");
        b.InitialCatalog = "master";
        return (ten, b.ConnectionString);
    }

    /// <summary>Xóa CSDL kiểm thử (nếu có) để migrator tạo lại sạch + dữ liệu mẫu; chuẩn bị thư mục sao lưu kiểm thử.</summary>
    public static async Task TaoLaiAsync(string chuoiKetNoi, string? thuMucSaoLuu)
    {
        var (ten, chuoi) = TachChuoi(chuoiKetNoi);
        await using var cn = new SqlConnection(chuoi);
        await cn.OpenAsync();
        await using (var cmd = new SqlCommand("""
            IF DB_ID(@ten) IS NOT NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(@ten) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@ten) + N';';
                EXEC (@sql);
            END
            """, cn) { CommandTimeout = 120 })
        {
            cmd.Parameters.AddWithValue("@ten", ten);
            await cmd.ExecuteNonQueryAsync();
        }
        Console.WriteLine($"[migrator] Đã xóa CSDL kiểm thử {ten} (tạo lại ở bước migration).");
        if (!string.IsNullOrWhiteSpace(thuMucSaoLuu))
        {
            await using var tao = new SqlCommand("EXEC master.sys.xp_create_subdir @d;", cn);
            tao.Parameters.AddWithValue("@d", thuMucSaoLuu.Trim());
            await tao.ExecuteNonQueryAsync();
            await XoaTepAsync(cn, ten, thuMucSaoLuu.Trim());
        }
    }

    /// <summary>
    /// Dọn tệp .bak do kiểm thử tạo trong thư mục sao lưu kiểm thử. Chỉ xóa tệp mang tên CSDL kiểm thử
    /// (…ThuVien_Test_….bak) và tệp KiemTra_*.bak – kể cả khi cấu hình nhầm sang thư mục khác cũng không xóa tệp thật.
    /// </summary>
    public static async Task DonSaoLuuAsync(string chuoiKetNoi, string? thuMucSaoLuu)
    {
        var (ten, chuoi) = TachChuoi(chuoiKetNoi);
        if (string.IsNullOrWhiteSpace(thuMucSaoLuu))
        {
            Console.WriteLine("[migrator] SaoLuu__ThuMuc trống – không dọn tệp sao lưu kiểm thử.");
            return;
        }
        await using var cn = new SqlConnection(chuoi);
        await cn.OpenAsync();
        await XoaTepAsync(cn, ten, thuMucSaoLuu.Trim());
    }

    private static async Task XoaTepAsync(SqlConnection cn, string ten, string thuMuc)
    {
        var tach = thuMuc.Contains('\\') ? '\\' : '/';
        var goc = thuMuc.TrimEnd('/', '\\') + tach;
        foreach (var mau in new[] { $"*{ten}_*.bak", "KiemTra_*.bak" })
        {
            try
            {
                await using var cmd = new SqlCommand("EXEC master.sys.xp_delete_files @mau;", cn);
                cmd.Parameters.AddWithValue("@mau", goc + mau);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[migrator] Không dọn được {goc + mau}: {ex.Message}");
            }
        }
        Console.WriteLine($"[migrator] Đã dọn tệp sao lưu kiểm thử trong {thuMuc}.");
    }
}
