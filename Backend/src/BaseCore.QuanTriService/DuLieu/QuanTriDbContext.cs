using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.QuanTriService.DuLieu;

/// <summary>DbContext của service quantri – kết nối bằng login svc_quantri (chỉ ghi schema quantri).</summary>
public sealed class QuanTriDbContext(DbContextOptions<QuanTriDbContext> options) : ThuVienDbContext(options);
