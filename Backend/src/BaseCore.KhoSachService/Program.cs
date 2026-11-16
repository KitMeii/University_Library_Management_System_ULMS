using BaseCore.Common.HeThong;
using BaseCore.KhoSachService.DuLieu;
using Microsoft.EntityFrameworkCore;

// Service khosach (GIA_DINH D11): danh mục sách, khu vực, đầu sách, thể loại, dữ liệu kệ sách cho cổng.
if (HealthProbe.LaYeuCauKiemTra(args)) return await HealthProbe.ChayAsync();

var builder = WebApplication.CreateBuilder(args);
builder.AddThuVienService("khosach").AddDbContextCheck<KhoSachDbContext>("csdl");
builder.Services.AddDbContext<KhoSachDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("ThuVien"), sql => sql.EnableRetryOnFailure()));
builder.Services.AddScoped<BaseCore.KhoSachService.NghiepVu.KhuVucService>();
builder.Services.AddScoped<BaseCore.KhoSachService.NghiepVu.SachService>();
builder.Services.AddScoped<BaseCore.KhoSachService.NghiepVu.ThongKeN3Service>();
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
