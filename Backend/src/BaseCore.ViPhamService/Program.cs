using BaseCore.Common.HeThong;
using BaseCore.ViPhamService.DuLieu;
using Microsoft.EntityFrameworkCore;

// Service vipham (GIA_DINH D11): khoản thu, biên bản vi phạm, báo cáo vi phạm.
if (HealthProbe.LaYeuCauKiemTra(args)) return await HealthProbe.ChayAsync();

var builder = WebApplication.CreateBuilder(args);
builder.AddThuVienService("vipham").AddDbContextCheck<ViPhamDbContext>("csdl");
builder.Services.AddDbContext<ViPhamDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("ThuVien"), sql => sql.EnableRetryOnFailure()));
builder.Services.AddScoped<BaseCore.ViPhamService.NghiepVu.KhoanThuService>();
builder.Services.AddScoped<BaseCore.ViPhamService.NghiepVu.BienBanService>();
builder.Services.AddScoped<BaseCore.ViPhamService.NghiepVu.BaoCaoViPhamService>();
builder.Services.AddScoped<BaseCore.ViPhamService.NghiepVu.ThongKeN2Service>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseThuVienService();
await app.RunAsync();
return 0;
