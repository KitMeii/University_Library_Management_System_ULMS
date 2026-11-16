using BaseCore.Common.Loi;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.Entities.KhoSach;
using BaseCore.Entities.ViPham;
using BaseCore.MuonTraService.DuLieu;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.MuonTraService.NghiepVu;

/// <summary>Thông tin Bạn đọc in trên chứng từ (MB02–MB04) và hiển thị ở form lập chứng từ.</summary>
public sealed record BanDocTomTat(string MaBanDoc, string HoTen, string DoiTuong, string MaSvHvCb, string LopDonVi, string SoTheBd);

/// <summary>Kết quả btnIn: số chứng từ (để mở trang in) + thông báo kết quả.</summary>
public sealed record KetQuaLuuChungTu(string So, string ThongBao, bool LaPhieuMoi);

/// <summary>Một dòng CT_PHIEUMUON chưa trả kèm hạn trả hiện hành và số lần gia hạn được đồng ý.</summary>
public sealed record DongChuaTra(string SoPMuon, string MaBanSach, string TenSach, DateOnly NgayMuon, DateOnly HanTraGoc,
    DateOnly HanHienHanh, int SoLanGiaHan, string TinhTrangMuon);

/// <summary>Truy vấn dùng chung cho 6.2.1–6.2.3.</summary>
public sealed class ChungTuChung(MuonTraDbContext db)
{
    public static BanDocTomTat TomTat(BanDoc b) => new(b.MaBanDoc, b.HoTen, b.DoiTuong, b.MaSvHvCb, b.LopDonVi, b.SoTheBd);

    /// <summary>Mã bản sách đang thuộc một phiếu mượn chưa trả (của bất kỳ bạn đọc nào).</summary>
    public IQueryable<string> BanDangMuon() =>
        from cm in db.CtPhieuMuon
        where !(from ct in db.CtPhieuTra
                join pt in db.PhieuTra on ct.SoPTra equals pt.SoPTra
                where pt.SoPMuon == cm.SoPMuon && ct.MaBanSach == cm.MaBanSach
                select 1).Any()
        select cm.MaBanSach;

    /// <summary>
    /// Bản sách có lần trả gần nhất ở tình trạng Mất/Hư hỏng mà Bộ phận kho sách chưa xử lý (03.03.02 – chưa có dòng
    /// XULY_CHUYENDEN cho lần trả đó) → không cho mượn (GIA_DINH D47). N3 xử lý xong – kể cả sau đó sửa tình trạng
    /// về “Sẵn sàng” (sách hư hỏng đã sửa) – thì cho mượn lại bình thường.
    /// </summary>
    public IQueryable<string> BanChoKhoXuLy() =>
        from ct in db.CtPhieuTra
        join pt in db.PhieuTra on ct.SoPTra equals pt.SoPTra
        where (ct.TinhTrangTra == MienGiaTri.TinhTrangTra.Mat || ct.TinhTrangTra == MienGiaTri.TinhTrangTra.HuHong)
              && !db.XuLyChuyenDen.Any(x => x.MaBanSach == ct.MaBanSach && x.SoChungTu == ct.SoPTra && x.TienTrinh == XuLyChuyenDen.MatHuHong)
              && !(from ct2 in db.CtPhieuTra join pt2 in db.PhieuTra on ct2.SoPTra equals pt2.SoPTra
                   where ct2.MaBanSach == ct.MaBanSach && pt2.NgayTra > pt.NgayTra select 1).Any()
        select ct.MaBanSach;

    public async Task<PhieuMuon> PhieuMuonAsync(string? soPMuon, CancellationToken ct)
    {
        var so = (soPMuon ?? string.Empty).Trim().ToUpperInvariant();
        if (so.Length == 0) new KiemTraDuLieu().Them("soPMuon", "Vui lòng nhập số Phiếu mượn sách.").NemNeuCoLoi();
        return await db.PhieuMuon.AsNoTracking().SingleOrDefaultAsync(x => x.SoPMuon == so, ct)
               ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_PHIEU_MUON", $"Không tìm thấy Phiếu mượn sách {so}.");
    }

    public async Task<BanDoc> BanDocAsync(string maBanDoc, CancellationToken ct) =>
        await db.BanDoc.AsNoTracking().SingleOrDefaultAsync(x => x.MaBanDoc == maBanDoc, ct)
        ?? throw LoiNghiepVu.KhongTimThay("KHONG_TIM_THAY_BAN_DOC", $"Không tìm thấy Bạn đọc {maBanDoc}.");

    /// <summary>
    /// Các dòng của phiếu mượn (chưa trả nếu <paramref name="chiChuaTra"/>), hạn trả hiện hành = hạn mới nhất của các lần
    /// gia hạn “Đồng ý” (SPEC 6.2.2), số lần gia hạn = COUNT(lần gia hạn được đồng ý) – TT034.
    /// </summary>
    public async Task<List<DongChuaTra>> DongPhieuMuonAsync(string soPMuon, bool chiChuaTra, CancellationToken ct, string? truSoPGiaHan = null)
    {
        var dong = await (from cm in db.CtPhieuMuon.AsNoTracking()
                          where cm.SoPMuon == soPMuon
                                && (!chiChuaTra || !(from ct2 in db.CtPhieuTra join pt in db.PhieuTra on ct2.SoPTra equals pt.SoPTra
                                                     where pt.SoPMuon == cm.SoPMuon && ct2.MaBanSach == cm.MaBanSach select 1).Any())
                          select new { cm.SoPMuon, cm.MaBanSach, cm.TenSach, cm.NgayMuon, cm.HanTra, cm.TinhTrangMuon }).ToListAsync(ct);
        // Các lần gia hạn “Đồng ý” của phiếu mượn (trừ phiếu gia hạn đang sửa)
        var giaHan = await (from cg in db.CtPhieuGiaHan.AsNoTracking()
                            join pg in db.PhieuGiaHan on cg.SoPGiaHan equals pg.SoPGiaHan
                            where pg.SoPMuon == soPMuon && cg.KetQuaGiaHan == MienGiaTri.KetQuaGiaHan.DongY
                                  && (truSoPGiaHan == null || cg.SoPGiaHan != truSoPGiaHan)
                            select new { cg.MaBanSach, cg.HanTraMoi }).ToListAsync(ct);
        return dong.Select(x =>
            {
                var gh = giaHan.Where(g => g.MaBanSach == x.MaBanSach).ToList();
                var hanMoi = gh.Max(g => g.HanTraMoi);
                return new DongChuaTra(x.SoPMuon, x.MaBanSach, x.TenSach, x.NgayMuon, x.HanTra,
                    hanMoi is { } h && h > x.HanTra ? h : x.HanTra, gh.Count, x.TinhTrangMuon);
            })
            .OrderBy(x => x.NgayMuon).ThenBy(x => x.MaBanSach).ToList();
    }

    /// <summary>Khóa ứng dụng theo bản sách trong transaction hiện tại – hai người không cùng cho mượn một bản sách.</summary>
    public async Task KhoaBanSachAsync(IEnumerable<string> maBanSach, CancellationToken ct)
    {
        var cn = (SqlConnection)db.Database.GetDbConnection();
        var tx = (SqlTransaction)Microsoft.EntityFrameworkCore.Storage.DbContextTransactionExtensions.GetDbTransaction(db.Database.CurrentTransaction!);
        foreach (var ma in maBanSach.Order(StringComparer.Ordinal))
        {
            await using var cmd = new SqlCommand("EXEC sp_getapplock @Resource = @r, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;", cn, tx);
            cmd.Parameters.AddWithValue("@r", "ULMS:SACH:" + ma);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
