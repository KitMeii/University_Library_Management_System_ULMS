using BaseCore.Entities;
using BaseCore.Entities.KhoSach;
using BaseCore.KhoSachService.DuLieu;

namespace BaseCore.KhoSachService.NghiepVu;

/// <summary>
/// Bản sách “có thể mượn” hiển thị ở cổng bạn đọc (file 04 mục 4): SACH “Sẵn sàng”, không thuộc phiếu mượn chưa trả,
/// không đang được giữ chỗ (PHIEUDATCHO “Đang giữ”), không trả Mất/Hư hỏng mà Bộ phận kho sách chưa xử lý (GIA_DINH D47).
/// Cùng quy tắc chặn cho mượn ở 6.2.1.
/// </summary>
public static class BanCoTheMuon
{
    public static IQueryable<string> Query(KhoSachDbContext db)
    {
        var dangMuon = from cm in db.CtPhieuMuon
                       where !(from x in db.CtPhieuTra
                               join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
                               where pt.SoPMuon == cm.SoPMuon && x.MaBanSach == cm.MaBanSach
                               select 1).Any()
                       select cm.MaBanSach;
        var dangGiu = db.PhieuDatCho.Where(d => d.TrangThai == MienGiaTri.TrangThaiDatCho.DangGiu).Select(d => d.MaBanSach);
        var choKho = from x in db.CtPhieuTra
                     join pt in db.PhieuTra on x.SoPTra equals pt.SoPTra
                     where (x.TinhTrangTra == MienGiaTri.TinhTrangTra.Mat || x.TinhTrangTra == MienGiaTri.TinhTrangTra.HuHong)
                           && !db.XuLyChuyenDen.Any(k => k.MaBanSach == x.MaBanSach && k.SoChungTu == x.SoPTra && k.TienTrinh == XuLyChuyenDen.MatHuHong)
                           && !(from x2 in db.CtPhieuTra join pt2 in db.PhieuTra on x2.SoPTra equals pt2.SoPTra
                                where x2.MaBanSach == x.MaBanSach && pt2.NgayTra > pt.NgayTra select 1).Any()
                     select x.MaBanSach;
        return db.Sach.Where(s => s.TinhTrang == MienGiaTri.TinhTrangSach.SanSang
                                  && !dangMuon.Contains(s.MaBanSach) && !dangGiu.Contains(s.MaBanSach) && !choKho.Contains(s.MaBanSach))
            .Select(s => s.MaBanSach);
    }

    /// <summary>Ảnh bìa: AnhBia lưu bản lớn “/uploads/bia/{id}.webp”, bản nhỏ cùng tên thêm “_nho”.</summary>
    public static string? AnhNho(string? anhBia) => anhBia is null ? null : anhBia[..^5] + "_nho.webp";
}
