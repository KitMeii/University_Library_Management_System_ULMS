using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.KhoSachService.DuLieu;

/// <summary>DbContext của service khosach – kết nối bằng login svc_khosach (chỉ ghi schema khosach).</summary>
public sealed class KhoSachDbContext(DbContextOptions<KhoSachDbContext> options) : ThuVienDbContext(options);
