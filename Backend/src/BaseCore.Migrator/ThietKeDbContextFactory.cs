using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BaseCore.Migrator;

/// <summary>Cho công cụ `dotnet ef migrations add` (không cần kết nối CSDL thật).</summary>
public sealed class ThietKeDbContextFactory : IDesignTimeDbContextFactory<ThuVienDbContext>
{
    public ThuVienDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ThuVienDbContext>()
            .UseSqlServer("Server=localhost;Database=ThuVien;Trusted_Connection=False;TrustServerCertificate=True",
                sql => sql.MigrationsAssembly(typeof(ThietKeDbContextFactory).Assembly.GetName().Name))
            .Options;
        return new ThuVienDbContext(options);
    }
}
