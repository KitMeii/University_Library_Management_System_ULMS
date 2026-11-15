using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.MuonTraService.DuLieu;

/// <summary>DbContext của service muontra – kết nối bằng login svc_muontra (chỉ ghi schema muontra).</summary>
public sealed class MuonTraDbContext(DbContextOptions<MuonTraDbContext> options) : ThuVienDbContext(options);
