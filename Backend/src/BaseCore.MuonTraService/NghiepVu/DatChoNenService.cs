using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.MuonTraService.DuLieu;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BaseCore.MuonTraService.NghiepVu;

/// <summary>
/// Tác vụ nền của muontra (file 06 mục 3, file 07 mục 4), chạy mỗi <c>ChuKyQuetPhut</c>:
/// <list type="bullet">
/// <item><c>DatChoHetHan</c>: “Đang giữ” quá tg_hethan → “Hết hạn” (tg_xuly) + THONGBAO “Hết hạn giữ”.</item>
/// <item><c>NhacHan</c> (đặt chỗ): còn ≤ NhacTruocGio giờ → THONGBAO “Sắp hết hạn giữ” (một lần).</item>
/// <item><c>NhacHan</c> (hạn trả): từ <c>GioNhacHanTra</c> giờ mỗi ngày → “Sắp đến hạn trả” (còn 2 ngày), “Quá hạn trả”
/// (mỗi dòng mượn một lần cho mỗi hạn trả).</item>
/// </list>
/// Mọi thời điểm qua IClock. Hai bản service chạy song song cũng an toàn: mỗi lượt quét giữ khóa ứng dụng “ULMS:NEN:DATCHO”.
/// </summary>
public sealed class DatChoNenService(IServiceScopeFactory scopes, IOptions<DatChoOptions> tuyChon, ILogger<DatChoNenService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var chuKy = TimeSpan.FromMinutes(Math.Max(0.02, tuyChon.Value.ChuKyQuetPhut));
        logger.LogInformation("Tác vụ nền đặt chỗ/nhắc hạn chạy mỗi {ChuKy}", chuKy);
        // Chờ CSDL sẵn sàng sau khi container khởi động
        try { await Task.Delay(TimeSpan.FromSeconds(Math.Min(10, chuKy.TotalSeconds)), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<TacVuNen>().ChayMotLuotAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi chạy tác vụ nền đặt chỗ/nhắc hạn");
            }
            try { await Task.Delay(chuKy, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }
}

/// <summary>Một lượt quét của tác vụ nền (scoped – tách riêng để kiểm thử và gọi lại được).</summary>
public sealed class TacVuNen(MuonTraDbContext db, IClock clock, DatChoService datCho, ChungTuChung chung,
    IOptions<DatChoOptions> tuyChon, ILogger<TacVuNen> logger)
{
    public async Task<(int HetHan, int NhacGiu, int NhacTra)> ChayMotLuotAsync(CancellationToken ct)
    {
        var kq = await db.ChayAsync(async c =>
        {
            await chung.KhoaAsync(["ULMS:NEN:DATCHO"], c);
            var bayGio = clock.BayGio;
            var hetHan = await datCho.HetHanAsync(db.PhieuDatCho, bayGio, c);
            var nhacGiu = await datCho.NhacSapHetHanAsync(bayGio, c);
            var nhacTra = bayGio.Hour >= tuyChon.Value.GioNhacHanTra ? await NhacHanTraAsync(DateOnly.FromDateTime(bayGio), bayGio, c) : 0;
            await db.SaveChangesAsync(c);
            return (hetHan, nhacGiu, nhacTra);
        }, ct);
        if (kq.hetHan + kq.nhacGiu + kq.nhacTra > 0)
            logger.LogInformation("Tác vụ nền: {HetHan} đặt chỗ hết hạn, {NhacGiu} nhắc sắp hết hạn giữ, {NhacTra} nhắc hạn trả",
                kq.hetHan, kq.nhacGiu, kq.nhacTra);
        return kq;
    }

    /// <summary>
    /// “Sắp đến hạn trả” khi hạn trả hiện hành (đã tính gia hạn được đồng ý) = hôm nay + NhacTruocHanTraNgay;
    /// “Quá hạn trả” khi đã quá hạn. Khóa chống lặp = liên kết chứa số phiếu, mã bản sách và hạn trả.
    /// </summary>
    private async Task<int> NhacHanTraAsync(DateOnly homNay, DateTime bayGio, CancellationToken ct)
    {
        var nguong = homNay.AddDays(tuyChon.Value.NhacTruocHanTraNgay);
        var dong = await (from cm in db.CtPhieuMuon
                          join pm in db.PhieuMuon on cm.SoPMuon equals pm.SoPMuon
                          where !(from ct2 in db.CtPhieuTra join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                                  where pt.SoPMuon == cm.SoPMuon && ct2.MaBanSach == cm.MaBanSach select 1).Any()
                          select new
                          {
                              pm.MaBanDoc, cm.SoPMuon, cm.MaBanSach, cm.TenSach, cm.HanTra,
                              HanGiaHan = (from cg in db.CtPhieuGiaHan
                                           join pg in db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
                                           where pg.SoPMuon == cm.SoPMuon && cg.MaBanSach == cm.MaBanSach
                                                 && cg.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY
                                           select cg.HanTraMoi).Max()
                          }).ToListAsync(ct);
        var n = 0;
        foreach (var x in dong)
        {
            var han = x.HanGiaHan is { } h && h > x.HanTra ? h : x.HanTra;
            string? loai = han == nguong ? "Sắp đến hạn trả" : han < homNay ? "Quá hạn trả" : null;
            if (loai is null) continue;
            var lk = $"/tai-khoan/dang-muon?pm={x.SoPMuon}&bs={x.MaBanSach}&han={han:yyyyMMdd}";
            if (await db.ThongBao.AnyAsync(t => t.MaBanDoc == x.MaBanDoc && t.Loai == loai && t.LienKet == lk, ct)) continue;
            db.ThongBao.Add(new ThongBao
            {
                MaBanDoc = x.MaBanDoc, Loai = loai, LienKet = lk, TgTao = bayGio,
                NoiDung = loai == "Sắp đến hạn trả"
                    ? $"Sách “{x.TenSach}” (bản {x.MaBanSach}) đến hạn trả ngày {han:dd/MM/yyyy}. Vui lòng trả hoặc gia hạn tại quầy mượn trả."
                    : $"Sách “{x.TenSach}” (bản {x.MaBanSach}) đã quá hạn trả từ ngày {han:dd/MM/yyyy}. Vui lòng trả sách tại quầy mượn trả."
            });
            n++;
        }
        return n;
    }
}
