using System.Globalization;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BaseCore.Common.Auth;

/// <summary>
/// GIA_DINH D36 – Mốc thu hồi phiên: sau khi phục hồi dữ liệu (04.00.09) mọi phiên đăng nhập cấp trước mốc
/// (nhân viên và bạn đọc) bị từ chối, trừ phiên của người vừa thực hiện phục hồi.
/// Lưu bằng extended property của CSDL (không thêm bảng ngoài SPEC): giá trị "unixGiây|ma_dangnhap miễn trừ".
/// </summary>
public sealed class MocThuHoiPhien(IConfiguration config, ILogger<MocThuHoiPhien> logger)
{
    public const string TenThuocTinh = "ULMS_MocThuHoiPhien";
    public const string MaLoi = "PHIEN_DA_BI_HUY";
    public const string ThongBao = "Phiên đăng nhập đã bị hủy do dữ liệu hệ thống vừa được phục hồi, vui lòng đăng nhập lại.";
    private static readonly TimeSpan ThoiGianNho = TimeSpan.FromSeconds(5);

    public sealed record ThongTin(long MocUnix, string? MaDangNhapMienTru);

    private readonly SemaphoreSlim _khoa = new(1, 1);
    private ThongTin? _giaTri;
    private DateTime _hetHanNho = DateTime.MinValue;

    public static string MaHoa(DateTimeOffset moc, string? maDangNhapMienTru) =>
        moc.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture) + "|" + (maDangNhapMienTru ?? string.Empty);

    public static ThongTin? GiaiMa(string? giaTri)
    {
        if (string.IsNullOrWhiteSpace(giaTri)) return null;
        var p = giaTri.Split('|', 2);
        if (!long.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var moc)) return null;
        return new ThongTin(moc, p.Length > 1 && p[1].Length > 0 ? p[1] : null);
    }

    /// <summary>Phiên (JWT/cookie) cấp trước mốc và không phải phiên miễn trừ thì bị thu hồi.</summary>
    public static bool BiThuHoi(ThongTin? moc, ClaimsPrincipal nguoiDung, long? capLucUnix)
    {
        if (moc is null) return false;
        if (moc.MaDangNhapMienTru is { } mt && nguoiDung.FindFirst(ThuVienClaims.MaDangNhap)?.Value == mt) return false;
        return capLucUnix is null || capLucUnix < moc.MocUnix;
    }

    /// <summary>Thời điểm cấp token theo claim "iat" (giây Unix).</summary>
    public static long? CapLuc(ClaimsPrincipal nguoiDung) =>
        long.TryParse(nguoiDung.FindFirst("iat")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    public void XoaBoNho() => _hetHanNho = DateTime.MinValue;

    /// <summary>Đọc mốc (nhớ tạm 5 giây). Lỗi CSDL: giữ giá trị cũ, không chặn mọi người dùng.</summary>
    public async Task<ThongTin?> LayAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow < _hetHanNho) return _giaTri;
        await _khoa.WaitAsync(ct);
        try
        {
            if (DateTime.UtcNow < _hetHanNho) return _giaTri;
            var chuoi = config.GetConnectionString("ThuVien");
            if (string.IsNullOrEmpty(chuoi)) return null;
            await using var cn = new SqlConnection(chuoi);
            await cn.OpenAsync(ct);
            await using var cmd = new SqlCommand(
                "SELECT CAST(value AS nvarchar(100)) FROM sys.extended_properties WHERE class = 0 AND name = @ten", cn);
            cmd.Parameters.AddWithValue("@ten", TenThuocTinh);
            _giaTri = GiaiMa(await cmd.ExecuteScalarAsync(ct) as string);
            _hetHanNho = DateTime.UtcNow + ThoiGianNho;
            return _giaTri;
        }
        catch (SqlException ex)
        {
            logger.LogWarning(ex, "Không đọc được mốc thu hồi phiên");
            return _giaTri;
        }
        finally
        {
            _khoa.Release();
        }
    }
}
