using BaseCore.Common.HeThong;
using BaseCore.QuanTriService.DuLieu;
using BaseCore.QuanTriService.NghiepVu;
using Microsoft.EntityFrameworkCore;

// Service quantri (GIA_DINH D11): đăng nhập nhân viên, tài khoản, phân quyền, nhật ký, sao lưu – phục hồi.
if (HealthProbe.LaYeuCauKiemTra(args)) return await HealthProbe.ChayAsync();

var builder = WebApplication.CreateBuilder(args);
builder.AddThuVienService("quantri").AddDbContextCheck<QuanTriDbContext>("csdl");
builder.Services.AddDbContext<QuanTriDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("ThuVien"), sql => sql.EnableRetryOnFailure()));
builder.Services.AddMemoryCache();
builder.Services.AddThuVienRateLimit(builder.Configuration);
builder.Services.AddScoped<XacThucNhanVienService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRateLimiter();
app.UseThuVienService();
await app.RunAsync();
return 0;
