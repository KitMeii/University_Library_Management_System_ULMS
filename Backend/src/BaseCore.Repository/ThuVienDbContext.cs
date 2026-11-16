using BaseCore.Entities;
using BaseCore.Entities.KhoSach;
using BaseCore.Entities.MuonTra;
using BaseCore.Entities.QuanTri;
using BaseCore.Entities.ViPham;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseCore.Repository;

/// <summary>
/// Mô hình EF Core đầy đủ của CSDL ThuVien (1 database, 4 schema – GIA_DINH D10, D14).
/// Migrator dùng để tạo lược đồ; mỗi service kế thừa thành DbContext riêng. Ranh giới ghi được ép bằng
/// login SQL của từng service (chỉ ghi schema của mình), nên service đọc được bảng của schema khác
/// qua cùng mô hình mà không thể ghi.
/// </summary>
public class ThuVienDbContext(DbContextOptions options) : DbContext(options)
{
    public const string SeqNhatKyHoatDong = "SEQ_NHATKY_HOATDONG";
    public const string SeqNhatKyDangNhap = "SEQ_NHATKY_DANGNHAP";

    // quantri
    public DbSet<NhomNguoiDung> NhomNguoiDung => Set<NhomNguoiDung>();
    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();
    public DbSet<Quyen> Quyen => Set<Quyen>();
    public DbSet<PhanQuyen> PhanQuyen => Set<PhanQuyen>();
    public DbSet<NhatKyDangNhap> NhatKyDangNhap => Set<NhatKyDangNhap>();
    public DbSet<NhatKyHoatDong> NhatKyHoatDong => Set<NhatKyHoatDong>();
    // khosach
    public DbSet<Sach> Sach => Set<Sach>();
    public DbSet<KhuVuc> KhuVuc => Set<KhuVuc>();
    public DbSet<TheLoai> TheLoai => Set<TheLoai>();
    public DbSet<DauSach> DauSach => Set<DauSach>();
    public DbSet<DauSachBanSach> DauSachBanSach => Set<DauSachBanSach>();
    public DbSet<XuLyChuyenDen> XuLyChuyenDen => Set<XuLyChuyenDen>();
    // muontra
    public DbSet<BanDoc> BanDoc => Set<BanDoc>();
    public DbSet<PhieuMuon> PhieuMuon => Set<PhieuMuon>();
    public DbSet<CtPhieuMuon> CtPhieuMuon => Set<CtPhieuMuon>();
    public DbSet<PhieuGiaHan> PhieuGiaHan => Set<PhieuGiaHan>();
    public DbSet<CtPhieuGiaHan> CtPhieuGiaHan => Set<CtPhieuGiaHan>();
    public DbSet<PhieuTra> PhieuTra => Set<PhieuTra>();
    public DbSet<CtPhieuTra> CtPhieuTra => Set<CtPhieuTra>();
    public DbSet<TaiKhoanBanDoc> TaiKhoanBanDoc => Set<TaiKhoanBanDoc>();
    public DbSet<PhieuDatCho> PhieuDatCho => Set<PhieuDatCho>();
    public DbSet<YeuThich> YeuThich => Set<YeuThich>();
    public DbSet<ThongBao> ThongBao => Set<ThongBao>();
    public DbSet<SoChungTuDem> SoChungTu => Set<SoChungTuDem>();
    // vipham
    public DbSet<KhoanThu> KhoanThu => Set<KhoanThu>();
    public DbSet<BienBan> BienBan => Set<BienBan>();
    public DbSet<CtBienBan> CtBienBan => Set<CtBienBan>();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        // date (D) và datetime2(0) (thời điểm) theo file 02 mục 1
        b.Properties<DateOnly>().HaveColumnType("date");
        b.Properties<DateTime>().HaveColumnType("datetime2(0)");
        b.Properties<decimal>().HavePrecision(18, 0);
    }

    protected override void OnModelCreating(ModelBuilder m)
    {
        CauHinhQuanTri(m);
        CauHinhKhoSach(m);
        CauHinhMuonTra(m);
        CauHinhViPham(m);
    }

    private static string Trong(string cot, IEnumerable<string> giaTri) =>
        $"[{cot}] IN ({string.Join(", ", giaTri.Select(v => "N'" + v.Replace("'", "''") + "'"))})";

    private static void Check<T>(EntityTypeBuilder<T> e, string ten, string sql) where T : class =>
        e.ToTable(t => t.HasCheckConstraint(ten, sql));

    private static void CauHinhQuanTri(ModelBuilder m)
    {
        m.HasSequence<long>(SeqNhatKyHoatDong, LuocDo.QuanTri).StartsAt(1);
        m.HasSequence<long>(SeqNhatKyDangNhap, LuocDo.QuanTri).StartsAt(1);

        m.Entity<NguoiDung>(e =>
        {
            e.HasIndex(x => x.TenDangNhap).IsUnique();
            e.HasOne<NhomNguoiDung>().WithMany().HasForeignKey(x => x.MaNhom).OnDelete(DeleteBehavior.Restrict);
            Check(e, "CK_NGUOIDUNG_tinhtrang", Trong("tinhtrang", MienGiaTri.TinhTrangNguoiDung.TatCa));
        });
        m.Entity<PhanQuyen>(e =>
        {
            e.HasKey(x => new { x.MaNhom, x.MaQuyen });
            e.HasOne<NhomNguoiDung>().WithMany().HasForeignKey(x => x.MaNhom).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Quyen>().WithMany().HasForeignKey(x => x.MaQuyen).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<NhatKyDangNhap>(e =>
        {
            e.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.MaNd).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.TgDangNhap);
        });
        m.Entity<NhatKyHoatDong>(e =>
        {
            e.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.MaNd).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.TgThucHien);
        });
    }

    private static void CauHinhKhoSach(ModelBuilder m)
    {
        m.Entity<KhuVuc>(e => Check(e, "CK_KHUVUC_loai_khuvuc", Trong("loai_khuvuc", MienGiaTri.LoaiKhuVuc.TatCa)));
        m.Entity<Sach>(e =>
        {
            e.HasOne<KhuVuc>().WithMany().HasForeignKey(x => x.MaKhuVuc).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.TinhTrang).HasDefaultValue(MienGiaTri.TinhTrangSach.SanSang);
            e.HasIndex(x => x.TenSach);
            e.HasIndex(x => x.TacGia);
            e.HasIndex(x => x.TinhTrang);
            Check(e, "CK_SACH_tinhtrang", Trong("tinhtrang", MienGiaTri.TinhTrangSach.TatCa));
        });
        m.Entity<TheLoai>(e =>
        {
            e.HasIndex(x => x.TenTheLoai).IsUnique();
            e.Property(x => x.ThuTuHienThi).HasDefaultValue(0);
        });
        m.Entity<DauSach>(e =>
        {
            e.HasOne<TheLoai>().WithMany().HasForeignKey(x => x.MaTheLoai).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.Isbn).IsUnique().HasFilter("[isbn] IS NOT NULL");
            e.HasIndex(x => x.TenSach);
            e.HasIndex(x => x.TacGia);
            e.HasIndex(x => x.NamXb);
            Check(e, "CK_DAUSACH_namxb", "[namxb] IS NULL OR ([namxb] >= 1900 AND [namxb] <= YEAR(GETDATE()))");
            Check(e, "CK_DAUSACH_mau_gay", "[mau_gay] IS NULL OR [mau_gay] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'");
        });
        m.Entity<DauSachBanSach>(e =>
        {
            e.HasOne<Sach>().WithOne().HasForeignKey<DauSachBanSach>(x => x.MaBanSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<DauSach>().WithMany().HasForeignKey(x => x.MaDauSach).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<XuLyChuyenDen>(e =>
        {
            e.HasKey(x => new { x.MaBanSach, x.SoChungTu, x.TienTrinh });
            e.HasOne<Sach>().WithMany().HasForeignKey(x => x.MaBanSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.MaNd).OnDelete(DeleteBehavior.Restrict);
            Check(e, "CK_XULY_CHUYENDEN_tinhtrang_moi", Trong("tinhtrang_moi", MienGiaTri.TinhTrangSach.TatCa));
        });
    }

    private static void CauHinhMuonTra(ModelBuilder m)
    {
        m.Entity<BanDoc>(e =>
        {
            e.HasIndex(x => x.MaSvHvCb).IsUnique();
            e.HasIndex(x => x.SoTheBd).IsUnique();
            e.HasIndex(x => x.HoTen);
            Check(e, "CK_BANDOC_doituong", Trong("doituong", MienGiaTri.DoiTuong.TatCa));
            Check(e, "CK_BANDOC_hsd", "[hsd_denngay] > [hsd_tungay]");
        });
        m.Entity<PhieuMuon>(e =>
        {
            e.HasOne<BanDoc>().WithMany().HasForeignKey(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.NgayLapPMuon);
        });
        m.Entity<CtPhieuMuon>(e =>
        {
            e.HasKey(x => new { x.MaBanSach, x.SoPMuon });
            e.HasOne<Sach>().WithMany().HasForeignKey(x => x.MaBanSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PhieuMuon>().WithMany().HasForeignKey(x => x.SoPMuon).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.SoPMuon);
            e.HasIndex(x => x.NgayMuon);
            e.HasIndex(x => x.HanTra);
        });
        m.Entity<PhieuGiaHan>(e =>
        {
            e.HasOne<PhieuMuon>().WithMany().HasForeignKey(x => x.SoPMuon).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<BanDoc>().WithMany().HasForeignKey(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.NgayLapPGiaHan);
        });
        m.Entity<CtPhieuGiaHan>(e =>
        {
            e.HasKey(x => new { x.MaBanSach, x.SoPGiaHan });
            e.HasOne<Sach>().WithMany().HasForeignKey(x => x.MaBanSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PhieuGiaHan>().WithMany().HasForeignKey(x => x.SoPGiaHan).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.SoPGiaHan);
            Check(e, "CK_CT_PHIEUGIAHAN_ketqua", Trong("ketqua_giahan", MienGiaTri.KetQuaGiaHan.TatCa));
        });
        m.Entity<PhieuTra>(e =>
        {
            e.HasOne<PhieuMuon>().WithMany().HasForeignKey(x => x.SoPMuon).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<BanDoc>().WithMany().HasForeignKey(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            // SPEC 6.2.3: mỗi so_pmuon chỉ có một Phiếu trả
            e.HasIndex(x => x.SoPMuon).IsUnique();
            e.HasIndex(x => x.NgayTra);
        });
        m.Entity<CtPhieuTra>(e =>
        {
            e.HasKey(x => new { x.MaBanSach, x.SoPTra });
            e.HasOne<Sach>().WithMany().HasForeignKey(x => x.MaBanSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PhieuTra>().WithMany().HasForeignKey(x => x.SoPTra).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.SoPTra);
            Check(e, "CK_CT_PHIEUTRA_tinhtrang_tra", Trong("tinhtrang_tra", MienGiaTri.TinhTrangTra.TatCa));
            Check(e, "CK_CT_PHIEUTRA_so_ngay_quahan", "[so_ngay_quahan] >= 0");
        });
        m.Entity<TaiKhoanBanDoc>(e =>
        {
            e.HasOne<BanDoc>().WithOne().HasForeignKey<TaiKhoanBanDoc>(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.SoLanSai).HasDefaultValue(0);
            Check(e, "CK_TAIKHOAN_BANDOC_tinhtrang", Trong("tinhtrang", MienGiaTri.TinhTrangTaiKhoanBanDoc.TatCa));
        });
        m.Entity<PhieuDatCho>(e =>
        {
            e.HasOne<BanDoc>().WithMany().HasForeignKey(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<DauSach>().WithMany().HasForeignKey(x => x.MaDauSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Sach>().WithMany().HasForeignKey(x => x.MaBanSach).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.MaNdXuLy).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PhieuMuon>().WithMany().HasForeignKey(x => x.SoPMuon).OnDelete(DeleteBehavior.Restrict);
            Check(e, "CK_PHIEUDATCHO_trangthai", Trong("trangthai", MienGiaTri.TrangThaiDatCho.TatCa));
            // File 02 X05: một bản sách chỉ giữ cho một người; mỗi bạn đọc giữ tối đa 1 bản của một đầu sách
            e.HasIndex(x => x.MaBanSach).IsUnique().HasDatabaseName("UX_DATCHO_BANSACH_DANGGIU")
                .HasFilter("[trangthai] = N'" + MienGiaTri.TrangThaiDatCho.DangGiu + "'");
            e.HasIndex(x => new { x.MaBanDoc, x.MaDauSach }).IsUnique().HasDatabaseName("UX_DATCHO_BANDOC_DAUSACH_DANGGIU")
                .HasFilter("[trangthai] = N'" + MienGiaTri.TrangThaiDatCho.DangGiu + "'");
            e.HasIndex(x => new { x.TrangThai, x.TgHetHan });
        });
        m.Entity<YeuThich>(e =>
        {
            e.HasKey(x => new { x.MaBanDoc, x.MaDauSach });
            e.HasOne<BanDoc>().WithMany().HasForeignKey(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<DauSach>().WithMany().HasForeignKey(x => x.MaDauSach).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<ThongBao>(e =>
        {
            e.Property(x => x.MaThongBao).UseIdentityColumn();
            e.HasOne<BanDoc>().WithMany().HasForeignKey(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.DaDoc).HasDefaultValue(false);
            e.HasIndex(x => new { x.MaBanDoc, x.DaDoc });
            Check(e, "CK_THONGBAO_loai", Trong("loai", MienGiaTri.LoaiThongBao.TatCa));
        });
        m.Entity<SoChungTuDem>(e => e.HasKey(x => new { x.Loai, x.Ngay }));
    }

    private static void CauHinhViPham(ModelBuilder m)
    {
        m.Entity<KhoanThu>(e => Check(e, "CK_KHOANTHU_dongia", "[dongia] > 0"));
        m.Entity<BienBan>(e =>
        {
            e.HasOne<PhieuMuon>().WithMany().HasForeignKey(x => x.SoPMuon).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<BanDoc>().WithMany().HasForeignKey(x => x.MaBanDoc).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.ThoiGianLap);
            Check(e, "CK_BIENBAN_ht_thanhtoan", Trong("ht_thanhtoan", MienGiaTri.HinhThucThanhToan.TatCa));
        });
        m.Entity<CtBienBan>(e =>
        {
            e.HasKey(x => new { x.MaKhoanThu, x.SoBienBan, x.MaBanSach });
            e.HasOne<KhoanThu>().WithMany().HasForeignKey(x => x.MaKhoanThu).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<BienBan>().WithMany().HasForeignKey(x => x.SoBienBan).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.SoBienBan);
            e.HasIndex(x => x.MaBanSach);
            Check(e, "CK_CT_BIENBAN_soluong", "[soluong] > 0");
        });
    }
}
