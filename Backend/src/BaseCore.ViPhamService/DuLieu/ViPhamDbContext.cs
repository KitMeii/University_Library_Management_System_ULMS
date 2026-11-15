using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.ViPhamService.DuLieu;

/// <summary>DbContext của service vipham – kết nối bằng login svc_vipham (chỉ ghi schema vipham).</summary>
public sealed class ViPhamDbContext(DbContextOptions<ViPhamDbContext> options) : ThuVienDbContext(options);
