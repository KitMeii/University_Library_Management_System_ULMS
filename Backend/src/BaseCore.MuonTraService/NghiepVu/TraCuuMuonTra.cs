using BaseCore.Entities;
using BaseCore.MuonTraService.DuLieu;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.MuonTraService.NghiepVu;

/// <summary>Một bản sách bạn đọc đang mượn (chưa trả) với hạn trả hiện hành (đã tính gia hạn được đồng ý).</summary>
public sealed record SachDangMuonDto(string SoPMuon, string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTra, int SoNgayQuaHan);

/// <summary>Truy vấn dùng chung của muontra: TT008 Truy xuất danh sách sách Bạn đọc đang mượn, TT009 Kiểm tra sách mượn quá hạn.</summary>
public sealed class TraCuuMuonTra(MuonTraDbContext db)
{
    /// <summary>Dòng CT_PHIEUMUON của bạn đọc chưa có trong CT_PHIEUTRA (cùng phiếu mượn).</summary>
    public async Task<List<SachDangMuonDto>> SachDangMuonAsync(string maBanDoc, DateOnly homNay, CancellationToken ct)
    {
        var dong = await (from cm in db.CtPhieuMuon.AsNoTracking()
                          join pm in db.PhieuMuon on cm.SoPMuon equals pm.SoPMuon
                          where pm.MaBanDoc == maBanDoc
                                && !(from ct2 in db.CtPhieuTra join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                                     where pt.SoPMuon == cm.SoPMuon && ct2.MaBanSach == cm.MaBanSach select 1).Any()
                          select new
                          {
                              cm.SoPMuon, cm.MaBanSach, cm.TenSach, cm.NgayMuon, cm.HanTra,
                              // Hạn trả hiện hành = hạn mới nhất của các lần gia hạn được đồng ý (SPEC 6.2.2)
                              HanGiaHan = (from cg in db.CtPhieuGiaHan
                                           join pg in db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
                                           where pg.SoPMuon == cm.SoPMuon && cg.MaBanSach == cm.MaBanSach
                                                 && cg.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY
                                           select cg.HanTraMoi).Max()
                          }).ToListAsync(ct);
        return dong.Select(x =>
        {
            var han = x.HanGiaHan is { } h && h > x.HanTra ? h : x.HanTra;
            return new SachDangMuonDto(x.SoPMuon, x.MaBanSach, x.TenSach, x.NgayMuon, han, Math.Max(0, homNay.DayNumber - han.DayNumber));
        }).OrderBy(x => x.HanTra).ToList();
    }
}
