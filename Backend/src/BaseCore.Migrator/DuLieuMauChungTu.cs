using BaseCore.Common.NghiepVu;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.Entities.ViPham;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using TT = BaseCore.Entities.MienGiaTri.TinhTrangSach;
using TR = BaseCore.Entities.MienGiaTri.TinhTrangTra;
using HT = BaseCore.Entities.MienGiaTri.HinhThucThanhToan;
using KQ = BaseCore.Entities.MienGiaTri.KetQuaGiaHan;

namespace BaseCore.Migrator;

/// <summary>
/// DỮ LIỆU MẪU – lịch sử chứng từ 2 tháng (SPEC mục 7): 25 phiếu mượn, 8 phiếu gia hạn, 18 phiếu trả, 5 biên bản (quá hạn,
/// mất, hư hỏng; 1 biên bản chưa thanh toán; 1 trường hợp vi phạm chờ lập biên bản; vài dòng “chuyển đến” chờ Bộ phận kho sách).
/// Ngày tính lùi từ hôm nay; số chứng từ cấp theo thứ tự ngày, bộ đếm SOCHUNGTU đồng bộ để chứng từ mới không trùng số.
/// Chỉ Development; chạy khi chưa có phiếu mượn nào.
/// </summary>
public static class DuLieuMauChungTu
{
    private enum KichBan { TraDung, TraQuaHan, Mat, HuHong, QuaHanChuaLapBb, GiaHanRoiTra, GiaHanTuChoi, TraMotPhan, DangMuon, DangMuonQuaHan, DangMuonGiaHan }

    /// <summary>(bạn đọc, ngày mượn lùi, số bản, kịch bản, ngày trả lùi hoặc null, ngày gia hạn lùi hoặc null, biên bản: hình thức thanh toán)</summary>
    private sealed record Dong(string BanDoc, int NgayMuon, int SoBan, KichBan KichBan, int? NgayTra = null, int? NgayGiaHan = null, string? ThanhToan = null);

    private static readonly Dong[] KichBanMau =
    [
        new("BD0002", 60, 2, KichBan.TraDung, NgayTra: 50),
        new("BD0003", 58, 1, KichBan.TraQuaHan, NgayTra: 38, ThanhToan: HT.TienMat),
        new("BD0005", 56, 2, KichBan.GiaHanRoiTra, NgayTra: 37, NgayGiaHan: 45),
        new("BD0007", 55, 1, KichBan.TraDung, NgayTra: 47),
        new("BD0009", 53, 2, KichBan.Mat, NgayTra: 45, ThanhToan: HT.ChuyenKhoan),
        new("BD0010", 52, 3, KichBan.GiaHanRoiTra, NgayTra: 35, NgayGiaHan: 40),
        new("BD0013", 50, 2, KichBan.GiaHanRoiTra, NgayTra: 15, NgayGiaHan: 25),
        new("BD0014", 48, 1, KichBan.HuHong, NgayTra: 25, ThanhToan: HT.TienMat),
        new("BD0015", 46, 2, KichBan.GiaHanRoiTra, NgayTra: 12, NgayGiaHan: 20),
        new("BD0008", 44, 1, KichBan.TraQuaHan, NgayTra: 27, ThanhToan: HT.ChuaThanhToan),
        new("BD0003", 42, 2, KichBan.TraDung, NgayTra: 33),
        new("BD0005", 40, 1, KichBan.GiaHanTuChoi, NgayTra: 22, NgayGiaHan: 24, ThanhToan: HT.ChuyenKhoan),
        new("BD0006", 38, 2, KichBan.TraDung, NgayTra: 30),
        new("BD0007", 36, 1, KichBan.QuaHanChuaLapBb, NgayTra: 17),
        new("BD0012", 34, 2, KichBan.TraDung, NgayTra: 20),
        new("BD0013", 30, 3, KichBan.TraMotPhan, NgayTra: 12),
        new("BD0009", 28, 1, KichBan.TraDung, NgayTra: 20),
        new("BD0010", 26, 2, KichBan.GiaHanRoiTra, NgayTra: 3, NgayGiaHan: 10),
        new("BD0001", 10, 1, KichBan.DangMuon),
        new("BD0006", 25, 1, KichBan.DangMuonQuaHan),
        new("BD0012", 30, 1, KichBan.DangMuonQuaHan),
        new("BD0014", 20, 2, KichBan.DangMuonGiaHan, NgayGiaHan: 2),
        new("BD0002", 12, 1, KichBan.DangMuonGiaHan, NgayGiaHan: 1),
        new("BD0003", 5, 2, KichBan.DangMuon),
        new("BD0005", 3, 1, KichBan.DangMuon),
    ];

    private const string MaN1 = "ND0002", TenN1 = "Nguyễn Thị Mượn Trả", MaN2 = "ND0003", TenN2 = "Trần Văn Vi Phạm", MaN3 = "ND0004";

    public static async Task NapAsync(ThuVienDbContext db, IClock clock)
    {
        if (await db.PhieuMuon.AnyAsync()) return;
        var homNay = clock.HomNay;
        var quyDinh = new QuyDinhOptions();
        var banDoc = await db.BanDoc.AsNoTracking().ToDictionaryAsync(x => x.MaBanDoc);
        var sach = await db.Sach.Where(x => x.TinhTrang == TT.SanSang).OrderBy(x => x.MaBanSach).ToListAsync();
        var khoanThu = await db.KhoanThu.AsNoTracking().ToDictionaryAsync(x => x.MaKhoanThu);
        if (sach.Count < 50 || khoanThu.Count < 4 || KichBanMau.Any(k => !banDoc.ContainsKey(k.BanDoc))) return;

        var dem = new Dictionary<(string, DateOnly), int>();
        string Cap(string loai, DateOnly ngay)
        {
            var khoa = (loai, SoChungTu.NgayKhoa(ngay));
            dem[khoa] = dem.GetValueOrDefault(khoa) + 1;
            return SoChungTu.Tao(loai, ngay, dem[khoa]);
        }
        DateOnly Ngay(int lui) => homNay.AddDays(-lui);

        // Cấp bản sách: lấy cách quãng để rải đều các đầu sách
        var iSach = 0;
        Entities.KhoSach.Sach LaySach() { var s = sach[iSach * 2 % sach.Count + iSach * 2 / sach.Count]; iSach++; return s; }

        var kichBan = KichBanMau.OrderByDescending(k => k.NgayMuon).ThenBy(k => k.BanDoc).ToList();
        var phieu = new List<(Dong K, PhieuMuon Pm, List<CtPhieuMuon> Ct)>();
        foreach (var k in kichBan)
        {
            var bd = banDoc[k.BanDoc];
            var ngayMuon = Ngay(k.NgayMuon);
            var pm = new PhieuMuon { SoPMuon = Cap(SoChungTu.PhieuMuon, ngayMuon), MaBanDoc = bd.MaBanDoc, NgayLapPMuon = ngayMuon, HoTen = bd.HoTen, MaNd = MaN1, TenNd = TenN1 };
            var ct = Enumerable.Range(0, k.SoBan).Select(i =>
            {
                var s = LaySach();
                return new CtPhieuMuon
                {
                    SoPMuon = pm.SoPMuon, MaBanSach = s.MaBanSach, NgayMuon = ngayMuon, HanTra = ngayMuon.AddDays(quyDinh.Cua(bd.DoiTuong).SoNgayMuon),
                    TinhTrangMuon = i % 3 == 2 ? "Bìa hơi cũ" : "Bình thường", GhiChuMuon = i % 4 == 3 ? "Bạn đọc đã kiểm tra sách" : null, TenSach = s.TenSach
                };
            }).ToList();
            db.PhieuMuon.Add(pm);
            db.CtPhieuMuon.AddRange(ct);
            phieu.Add((k, pm, ct));
        }

        // Gia hạn (đồng ý, trừ kịch bản từ chối vì đã quá hạn)
        var hanHienHanh = phieu.SelectMany(p => p.Ct).ToDictionary(c => (c.SoPMuon, c.MaBanSach), c => c.HanTra);
        foreach (var p in phieu.Where(p => p.K.NgayGiaHan is not null).OrderByDescending(p => p.K.NgayGiaHan))
        {
            var ngay = Ngay(p.K.NgayGiaHan!.Value);
            var pg = new PhieuGiaHan { SoPGiaHan = Cap(SoChungTu.PhieuGiaHan, ngay), SoPMuon = p.Pm.SoPMuon, MaBanDoc = p.Pm.MaBanDoc, NgayLapPGiaHan = ngay, NgayDeNghi = ngay, MaNd = MaN1, TenNd = TenN1 };
            db.PhieuGiaHan.Add(pg);
            foreach (var c in p.Ct)
            {
                var cu = hanHienHanh[(c.SoPMuon, c.MaBanSach)];
                var dongY = p.K.KichBan != KichBan.GiaHanTuChoi && ngay <= cu;
                var moi = cu.AddDays(quyDinh.SoNgayGiaHan);
                db.CtPhieuGiaHan.Add(new CtPhieuGiaHan
                {
                    MaBanSach = c.MaBanSach, SoPGiaHan = pg.SoPGiaHan, HanTraCu = cu, KetQuaGiaHan = dongY ? KQ.DongY : KQ.KhongDongY,
                    HanTraMoi = dongY ? moi : null, LyDoKhongGh = dongY ? null : $"Đã quá hạn trả ({cu:dd/MM/yyyy}).", TenSach = c.TenSach, SoLanGiaHan = dongY ? 1 : 0
                });
                if (dongY) hanHienHanh[(c.SoPMuon, c.MaBanSach)] = moi;
            }
        }

        // Trả sách + tình trạng khi trả
        var viPham = new List<(Dong K, PhieuMuon Pm, PhieuTra Pt, List<CtPhieuTra> Ct)>();
        foreach (var p in phieu.Where(p => p.K.NgayTra is not null).OrderByDescending(p => p.K.NgayTra))
        {
            var ngayTra = Ngay(p.K.NgayTra!.Value);
            var pt = new PhieuTra { SoPTra = Cap(SoChungTu.PhieuTra, ngayTra), SoPMuon = p.Pm.SoPMuon, MaBanDoc = p.Pm.MaBanDoc, NgayLapPTra = ngayTra, NgayTra = ngayTra, MaNd = MaN1, TenNd = TenN1 };
            db.PhieuTra.Add(pt);
            var tra = p.K.KichBan == KichBan.TraMotPhan ? p.Ct.Take(p.Ct.Count - 1) : p.Ct;
            var ctTra = tra.Select((c, i) => new CtPhieuTra
            {
                MaBanSach = c.MaBanSach, SoPTra = pt.SoPTra,
                TinhTrangTra = p.K.KichBan == KichBan.Mat && i == p.Ct.Count - 1 ? TR.Mat : p.K.KichBan == KichBan.HuHong ? TR.HuHong : TR.BinhThuong,
                GhiChuTra = p.K.KichBan == KichBan.HuHong ? "Rách bìa, ố nước nhiều trang" : p.K.KichBan == KichBan.Mat && i == p.Ct.Count - 1 ? "Bạn đọc báo mất sách" : null,
                SoNgayQuaHan = Math.Max(0, ngayTra.DayNumber - hanHienHanh[(c.SoPMuon, c.MaBanSach)].DayNumber)
            }).ToList();
            db.CtPhieuTra.AddRange(ctTra);
            if (ctTra.Any(c => c.SoNgayQuaHan > 0 || c.TinhTrangTra != TR.BinhThuong)) viPham.Add((p.K, p.Pm, pt, ctTra));
        }

        // Biên bản vi phạm (trừ kịch bản “chờ lập biên bản”)
        foreach (var v in viPham.Where(v => v.K.KichBan != KichBan.QuaHanChuaLapBb).OrderBy(v => v.Pt.NgayTra))
        {
            var thoiGian = v.Pt.NgayTra.AddDays(1).ToDateTime(new TimeOnly(9, 30));
            var bb = new BienBan
            {
                SoBienBan = Cap(SoChungTu.BienBan, DateOnly.FromDateTime(thoiGian)), SoPMuon = v.Pm.SoPMuon, MaBanDoc = v.Pm.MaBanDoc,
                ThoiGianLap = thoiGian, HtThanhToan = v.K.ThanhToan ?? HT.TienMat, MaNd = MaN2, TenNd = TenN2
            };
            var ct = new List<CtBienBan>();
            foreach (var c in v.Ct)
            {
                if (c.SoNgayQuaHan > 0) ct.Add(new CtBienBan { MaKhoanThu = "KT01", SoBienBan = bb.SoBienBan, MaBanSach = c.MaBanSach, SoLuong = c.SoNgayQuaHan });
                if (c.TinhTrangTra == TR.Mat) ct.Add(new CtBienBan { MaKhoanThu = "KT02", SoBienBan = bb.SoBienBan, MaBanSach = c.MaBanSach, SoLuong = 1 });
                if (c.TinhTrangTra == TR.HuHong) ct.Add(new CtBienBan { MaKhoanThu = "KT04", SoBienBan = bb.SoBienBan, MaBanSach = c.MaBanSach, SoLuong = 1 });
            }
            foreach (var x in ct) x.ThanhTien = x.SoLuong * khoanThu[x.MaKhoanThu].DonGia;
            bb.TongTien = ct.Sum(x => x.ThanhTien);
            db.BienBan.Add(bb);
            db.CtBienBan.AddRange(ct);
        }

        // Tình trạng bản sách sau khi Bộ phận kho sách đã xử lý – chừa lại vài dòng “chuyển đến” (03.01.04, 03.02.03, 03.03.02)
        var dangMuon = phieu.SelectMany(p => p.K.NgayTra is null ? p.Ct : p.K.KichBan == KichBan.TraMotPhan ? p.Ct.TakeLast(1) : []).ToList();
        var choMuonMoiNhat = phieu.Where(p => p.K.NgayTra is null).OrderBy(p => p.K.NgayMuon).First().Ct.First().MaBanSach;
        foreach (var c in dangMuon.Where(c => c.MaBanSach != choMuonMoiNhat))
            sach.First(s => s.MaBanSach == c.MaBanSach).TinhTrang = TT.DangDuocMuon;
        var traChoXuLy = phieu.Where(p => p.K.KichBan == KichBan.GiaHanRoiTra).OrderBy(p => p.K.NgayTra).First().Ct.First().MaBanSach;
        sach.First(s => s.MaBanSach == traChoXuLy).TinhTrang = TT.DangDuocMuon;
        foreach (var v in viPham.Where(v => v.K.KichBan == KichBan.Mat))
            foreach (var c in v.Ct.Where(c => c.TinhTrangTra == TR.Mat))
            {
                sach.First(s => s.MaBanSach == c.MaBanSach).TinhTrang = TT.Mat;
                // N3 đã xử lý 03.03.02 (GIA_DINH D47)
                db.XuLyChuyenDen.Add(new Entities.KhoSach.XuLyChuyenDen
                {
                    MaBanSach = c.MaBanSach, SoChungTu = c.SoPTra, TienTrinh = Entities.KhoSach.XuLyChuyenDen.MatHuHong,
                    TinhTrangMoi = TT.Mat, MaNd = MaN3, ThoiGianXuLy = v.Pt.NgayTra.AddDays(2).ToDateTime(new TimeOnly(8, 15))
                });
            }
        // Sách hư hỏng: biên bản đã lập, kho chưa cập nhật → chờ 03.03.02

        foreach (var ((loai, thang), so) in dem)
        {
            var dong = await db.SoChungTu.SingleOrDefaultAsync(x => x.Loai == loai && x.Ngay == thang);
            if (dong is null) db.SoChungTu.Add(new SoChungTuDem { Loai = loai, Ngay = thang, SoCuoi = so });
            else dong.SoCuoi = Math.Max(dong.SoCuoi, so);
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"[migrator] Đã nạp lịch sử chứng từ mẫu: {phieu.Count} phiếu mượn, {phieu.Count(p => p.K.NgayGiaHan is not null)} phiếu gia hạn, " +
                          $"{phieu.Count(p => p.K.NgayTra is not null)} phiếu trả, {viPham.Count(v => v.K.KichBan != KichBan.QuaHanChuaLapBb)} biên bản.");
    }
}
