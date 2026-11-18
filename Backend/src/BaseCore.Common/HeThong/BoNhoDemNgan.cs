using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace BaseCore.Common.HeThong;

/// <summary>
/// Bộ nhớ đệm ngắn cho dữ liệu kệ sách và thống kê tổng quan (file 11 mục 3 – mặc định 60 giây).
/// Cấu hình "ThuVien:BoNhoDem:Giay"; 0 = tắt (bộ kiểm thử chức năng so số liệu ngay sau khi ghi – scripts/test-docker.sh).
/// Chỉ dùng cho dữ liệu hiển thị chung mọi người; điều kiện nghiệp vụ (đặt chỗ, cho mượn) luôn kiểm tra trực tiếp trên CSDL.
/// </summary>
public sealed class BoNhoDemNgan(IMemoryCache cache, IConfiguration config)
{
    private readonly TimeSpan _thoiHan = TimeSpan.FromSeconds(Math.Max(0, config.GetValue("ThuVien:BoNhoDem:Giay", 60)));

    public bool Bat => _thoiHan > TimeSpan.Zero;

    /// <summary>Lấy từ bộ nhớ đệm hoặc tạo mới. Hàm tạo chạy không gắn CancellationToken của yêu cầu đầu tiên (kết quả dùng chung).</summary>
    public async Task<T> LayAsync<T>(string khoa, Func<CancellationToken, Task<T>> tao)
    {
        if (!Bat) return await tao(CancellationToken.None);
        if (cache.TryGetValue(khoa, out T? co) && co is not null) return co;
        var moi = await tao(CancellationToken.None);
        cache.Set(khoa, moi, _thoiHan);
        return moi;
    }
}
