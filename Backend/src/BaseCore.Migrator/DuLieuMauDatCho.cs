using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.MuonTra;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using TTDC = BaseCore.Entities.MienGiaTri.TrangThaiDatCho;

namespace BaseCore.Migrator;

/// <summary>
/// DỮ LIỆU MẪU – đặt chỗ đủ 4 trạng thái, trong đó 2 đặt chỗ “Đang giữ” còn dưới 2 giờ (file 02 mục 4); kèm THONGBAO và
/// YEUTHICH tương ứng. Bạn đọc giữ chỗ đều đủ điều kiện (thẻ còn hạn, không quá hạn, không nợ biên bản). BD0001
/// (SV22000001 – tài khoản dùng trong kiểm thử E2E) không có đặt chỗ mẫu để kiểm thử bắt đầu từ trạng thái sạch.
/// Chỉ Development; chạy khi chưa có đặt chỗ nào và đã có chứng từ mẫu.
/// </summary>
public static class DuLieuMauDatCho
{
    public static async Task NapAsync(ThuVienDbContext db, IClock clock)
    {
        if (await db.PhieuDatCho.AnyAsync() || !await db.PhieuMuon.AnyAsync()) return;
        var bayGio = clock.BayGio;

        // Bản sách chưa từng xuất hiện trong phiếu mượn mẫu, “Sẵn sàng”, đã gắn đầu sách – mỗi đầu sách một bản
        var daMuon = db.CtPhieuMuon.Select(c => c.MaBanSach);
        var ban = await (from b in db.DauSachBanSach
                         join s in db.Sach on b.MaBanSach equals s.MaBanSach
                         where s.TinhTrang == MienGiaTri.TinhTrangSach.SanSang && !daMuon.Contains(s.MaBanSach)
                         orderby b.MaDauSach, s.MaBanSach
                         select new { b.MaDauSach, s.MaBanSach }).ToListAsync();
        var moiDau = ban.GroupBy(x => x.MaDauSach).Select(g => g.First()).ToList();
        if (moiDau.Count < 8) return;
        var ten = await db.DauSach.ToDictionaryAsync(x => x.MaDauSach, x => x.TenSach);

        // Đã nhận: bản sách của phiếu mượn đang mượn gần nhất của BD0003 (đặt chỗ trước ngày mượn 1 ngày)
        var nhan = await (from c in db.CtPhieuMuon
                          join p in db.PhieuMuon on c.SoPMuon equals p.SoPMuon
                          join b in db.DauSachBanSach on c.MaBanSach equals b.MaBanSach
                          where p.MaBanDoc == "BD0003"
                          orderby p.NgayLapPMuon descending
                          select new { c.MaBanSach, b.MaDauSach, p.SoPMuon, p.NgayLapPMuon }).FirstOrDefaultAsync();

        var dem = new Dictionary<DateOnly, int>();
        string Ma(DateTime tg)
        {
            var ngay = DateOnly.FromDateTime(tg);
            dem[ngay] = dem.GetValueOrDefault(ngay) + 1;
            return VongDoiDatCho.TaoMa(ngay, dem[ngay]);
        }
        var i = 0;
        PhieuDatCho Tao(string maBanDoc, DateTime tgDat, string trangThai, DateTime? tgXuLy = null, string? lyDo = null)
        {
            var x = moiDau[i++];
            return new PhieuDatCho
            {
                MaDatCho = Ma(tgDat), MaBanDoc = maBanDoc, MaDauSach = x.MaDauSach, MaBanSach = x.MaBanSach, TgDat = tgDat,
                TgHetHan = VongDoiDatCho.HetHan(tgDat, 24), TrangThai = trangThai, TgXuLy = tgXuLy, LyDoHuy = lyDo
            };
        }

        var ds = new List<PhieuDatCho>
        {
            // Đã hủy, Hết hạn (quá khứ)
            Tao("BD0010", bayGio.AddDays(-8).AddHours(-3), TTDC.DaHuy, bayGio.AddDays(-8).AddHours(-1), "Bạn đọc hủy trên cổng"),
            Tao("BD0005", bayGio.AddDays(-10), TTDC.HetHan, bayGio.AddDays(-9).AddMinutes(4)),
            Tao("BD0009", bayGio.AddDays(-4).AddHours(-5), TTDC.HetHan, bayGio.AddDays(-3).AddHours(-5).AddMinutes(3)),
            // Đang giữ: 2 cái còn dưới 2 giờ (đếm ngược đỏ), 2 cái còn nhiều giờ
            Tao("BD0007", bayGio.AddHours(-22).AddMinutes(-35), TTDC.DangGiu),
            Tao("BD0009", bayGio.AddHours(-23).AddMinutes(-20), TTDC.DangGiu),
            Tao("BD0002", bayGio.AddHours(-3), TTDC.DangGiu),
            Tao("BD0007", bayGio.AddHours(-1).AddMinutes(-10), TTDC.DangGiu),
        };
        if (nhan is not null)
        {
            var tgDat = nhan.NgayLapPMuon.ToDateTime(new TimeOnly(15, 20)).AddDays(-1);
            ds.Add(new PhieuDatCho
            {
                MaDatCho = Ma(tgDat), MaBanDoc = "BD0003", MaDauSach = nhan.MaDauSach, MaBanSach = nhan.MaBanSach, TgDat = tgDat,
                TgHetHan = VongDoiDatCho.HetHan(tgDat, 24), TrangThai = TTDC.DaNhan,
                TgXuLy = nhan.NgayLapPMuon.ToDateTime(new TimeOnly(9, 5)), MaNdXuLy = "ND0002", SoPMuon = nhan.SoPMuon
            });
        }
        db.PhieuDatCho.AddRange(ds);

        foreach (var d in ds)
        {
            var t = ten[d.MaDauSach];
            void TB(string loai, string nd, DateTime tg, bool daDoc) => db.ThongBao.Add(new ThongBao
            {
                MaBanDoc = d.MaBanDoc, Loai = loai, NoiDung = nd, LienKet = "/tai-khoan/dat-cho?ma=" + d.MaDatCho, TgTao = tg, DaDoc = daDoc
            });
            TB("Đặt chỗ thành công", $"Đã giữ chỗ “{t}” (mã {d.MaDatCho}). Vui lòng nhận sách tại Quầy mượn trả – Tầng 1 trước {d.TgHetHan:HH:mm dd/MM/yyyy}.",
                d.TgDat, d.TrangThai != TTDC.DangGiu);
            if (d.TrangThai == TTDC.HetHan)
                TB("Hết hạn giữ", $"Đặt chỗ “{t}” (mã {d.MaDatCho}) đã hết hạn giữ lúc {d.TgHetHan:HH:mm dd/MM/yyyy}. Bản sách đã trở lại kệ.", d.TgXuLy!.Value, true);
            if (d.TrangThai == TTDC.DaNhan)
                TB("Đã nhận sách", $"Bạn đã nhận sách đặt chỗ “{t}” (mã {d.MaDatCho}) theo Phiếu mượn sách {d.SoPMuon}.", d.TgXuLy!.Value, true);
        }

        // Yêu thích mẫu cho vài bạn đọc (không gồm BD0001)
        foreach (var (bd, n) in new[] { ("BD0002", 4), ("BD0007", 3), ("BD0009", 2) })
            foreach (var x in ban.Select(b => b.MaDauSach).Distinct().Skip(n * 3).Take(n))
                db.YeuThich.Add(new YeuThich { MaBanDoc = bd, MaDauSach = x, TgThem = bayGio.AddDays(-n * 2) });

        foreach (var (ngay, so) in dem)
        {
            var dong = await db.SoChungTu.SingleOrDefaultAsync(x => x.Loai == VongDoiDatCho.LoaiSoChungTu && x.Ngay == ngay);
            if (dong is null) db.SoChungTu.Add(new SoChungTuDem { Loai = VongDoiDatCho.LoaiSoChungTu, Ngay = ngay, SoCuoi = so });
            else dong.SoCuoi = Math.Max(dong.SoCuoi, so);
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"[migrator] Đã nạp {ds.Count} đặt chỗ mẫu ({ds.Count(d => d.TrangThai == TTDC.DangGiu)} đang giữ).");
    }
}
