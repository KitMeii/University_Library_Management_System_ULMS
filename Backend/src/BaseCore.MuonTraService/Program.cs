using BaseCore.Common.HeThong;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.MuonTraService.NghiepVu;
using Microsoft.EntityFrameworkCore;

// Service muontra (GIA_DINH D11, D12): bạn đọc, phiếu mượn/gia hạn/trả, đặt chỗ, tài khoản bạn đọc, thông báo.
if (HealthProbe.LaYeuCauKiemTra(args)) return await HealthProbe.ChayAsync();

var builder = WebApplication.CreateBuilder(args);
builder.AddThuVienService("muontra").AddDbContextCheck<MuonTraDbContext>("csdl");
builder.Services.AddDbContext<MuonTraDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("ThuVien"), sql => sql.EnableRetryOnFailure())
     .AddInterceptors(new BaseCore.MuonTraService.DuLieu.GoiYTruyVanInterceptor()));
builder.Services.AddThuVienRateLimit(builder.Configuration);
builder.Services.AddScoped<XacThucBanDocService>();
builder.Services.AddScoped<TraCuuMuonTra>();
builder.Services.AddScoped<BanDocService>();
builder.Services.AddScoped<ChungTuChung>();
builder.Services.AddScoped<PhieuMuonService>();
builder.Services.AddScoped<PhieuGiaHanService>();
builder.Services.AddScoped<PhieuTraService>();
builder.Services.AddScoped<BaoCaoMuonTraService>();
builder.Services.AddScoped<ThongKeN1Service>();
// Pha 6: đặt chỗ 24 giờ, tài khoản bạn đọc, thông báo, tác vụ nền (file 06, 07)
builder.Services.AddScoped<DatChoService>();
builder.Services.AddScoped<TaiKhoanBanDocService>();
builder.Services.AddScoped<TacVuNen>();
if (builder.Configuration.GetValue("ThuVien:DatCho:ChayTacVuNen", true))
    builder.Services.AddHostedService<DatChoNenService>();
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
