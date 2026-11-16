-- Sinh tự động bởi BaseCore.Migrator --xuat-sql (scripts/xuat-sql.sh). Không sửa tay.
-- Tạo CSDL ThuVien, 4 schema (quantri, khosach, muontra, vipham), bảng, ràng buộc, sequence, thủ tục.
-- Idempotent: chạy lại nhiều lần không lỗi, chỉ áp dụng migration còn thiếu (bảng __EFMigrationsHistory).
IF DB_ID(N'ThuVien') IS NULL
    CREATE DATABASE [ThuVien];
GO
USE [ThuVien];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    IF SCHEMA_ID(N'muontra') IS NULL EXEC(N'CREATE SCHEMA [muontra];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    IF SCHEMA_ID(N'vipham') IS NULL EXEC(N'CREATE SCHEMA [vipham];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    IF SCHEMA_ID(N'khosach') IS NULL EXEC(N'CREATE SCHEMA [khosach];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    IF SCHEMA_ID(N'quantri') IS NULL EXEC(N'CREATE SCHEMA [quantri];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE SEQUENCE [quantri].[SEQ_NHATKY_DANGNHAP] START WITH 1 INCREMENT BY 1 NO CYCLE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE SEQUENCE [quantri].[SEQ_NHATKY_HOATDONG] START WITH 1 INCREMENT BY 1 NO CYCLE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[BANDOC] (
        [ma_bandoc] nvarchar(10) NOT NULL,
        [hoten] nvarchar(50) NOT NULL,
        [doituong] nvarchar(20) NOT NULL,
        [ma_svhvcb] nvarchar(15) NOT NULL,
        [lop_donvi] nvarchar(50) NOT NULL,
        [ngaysinh] date NOT NULL,
        [so_thebd] nvarchar(15) NOT NULL,
        [dienthoai_email] nvarchar(50) NULL,
        [hsd_tungay] date NOT NULL,
        [hsd_denngay] date NOT NULL,
        [anh] nvarchar(100) NULL,
        CONSTRAINT [PK_BANDOC] PRIMARY KEY ([ma_bandoc]),
        CONSTRAINT [CK_BANDOC_doituong] CHECK ([doituong] IN (N'Sinh viên', N'Học viên', N'Cán bộ')),
        CONSTRAINT [CK_BANDOC_hsd] CHECK ([hsd_denngay] > [hsd_tungay])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [vipham].[KHOANTHU] (
        [ma_khoanthu] nvarchar(10) NOT NULL,
        [noidung_tt] nvarchar(100) NOT NULL,
        [dongia] decimal(18,0) NOT NULL,
        CONSTRAINT [PK_KHOANTHU] PRIMARY KEY ([ma_khoanthu]),
        CONSTRAINT [CK_KHOANTHU_dongia] CHECK ([dongia] > 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [khosach].[KHUVUC] (
        [ma_khuvuc] nvarchar(10) NOT NULL,
        [vitri] nvarchar(50) NOT NULL,
        [loai_khuvuc] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_KHUVUC] PRIMARY KEY ([ma_khuvuc]),
        CONSTRAINT [CK_KHUVUC_loai_khuvuc] CHECK ([loai_khuvuc] IN (N'Kệ mở', N'Kho'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [quantri].[NHOMNGUOIDUNG] (
        [ma_nhom] nvarchar(10) NOT NULL,
        [ten_nhom] nvarchar(50) NOT NULL,
        [mota] nvarchar(200) NULL,
        CONSTRAINT [PK_NHOMNGUOIDUNG] PRIMARY KEY ([ma_nhom])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [quantri].[QUYEN] (
        [ma_quyen] nvarchar(10) NOT NULL,
        [ten_quyen] nvarchar(100) NOT NULL,
        [mota] nvarchar(200) NULL,
        CONSTRAINT [PK_QUYEN] PRIMARY KEY ([ma_quyen])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[SOCHUNGTU] (
        [loai] nvarchar(4) NOT NULL,
        [ngay] date NOT NULL,
        [so_cuoi] int NOT NULL,
        CONSTRAINT [PK_SOCHUNGTU] PRIMARY KEY ([loai], [ngay])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [khosach].[THELOAI] (
        [ma_theloai] nvarchar(10) NOT NULL,
        [ten_theloai] nvarchar(100) NOT NULL,
        [mota] nvarchar(500) NULL,
        [bieutuong] nvarchar(50) NULL,
        [thutu_hienthi] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_THELOAI] PRIMARY KEY ([ma_theloai])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[PHIEUMUON] (
        [so_pmuon] nvarchar(10) NOT NULL,
        [ma_bandoc] nvarchar(10) NOT NULL,
        [ngaylap_pmuon] date NOT NULL,
        [hoten] nvarchar(50) NOT NULL,
        [ma_nd] nvarchar(10) NOT NULL,
        [ten_nd] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_PHIEUMUON] PRIMARY KEY ([so_pmuon]),
        CONSTRAINT [FK_PHIEUMUON_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[TAIKHOAN_BANDOC] (
        [ma_bandoc] nvarchar(10) NOT NULL,
        [matkhau] nvarchar(200) NULL,
        [tinhtrang] nvarchar(20) NOT NULL,
        [so_lan_sai] int NOT NULL DEFAULT 0,
        [tg_khoa_den] datetime2(0) NULL,
        [tg_dangnhap_cuoi] datetime2(0) NULL,
        [tg_tao] datetime2(0) NOT NULL,
        CONSTRAINT [PK_TAIKHOAN_BANDOC] PRIMARY KEY ([ma_bandoc]),
        CONSTRAINT [CK_TAIKHOAN_BANDOC_tinhtrang] CHECK ([tinhtrang] IN (N'Chưa kích hoạt', N'Hoạt động', N'Khóa')),
        CONSTRAINT [FK_TAIKHOAN_BANDOC_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[THONGBAO] (
        [ma_thongbao] bigint NOT NULL IDENTITY,
        [ma_bandoc] nvarchar(10) NOT NULL,
        [loai] nvarchar(30) NOT NULL,
        [noidung] nvarchar(500) NOT NULL,
        [lienket] nvarchar(260) NULL,
        [tg_tao] datetime2(0) NOT NULL,
        [da_doc] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_THONGBAO] PRIMARY KEY ([ma_thongbao]),
        CONSTRAINT [CK_THONGBAO_loai] CHECK ([loai] IN (N'Đặt chỗ thành công', N'Sắp hết hạn giữ', N'Hết hạn giữ', N'Đã nhận sách', N'Sắp đến hạn trả', N'Quá hạn trả', N'Biên bản vi phạm')),
        CONSTRAINT [FK_THONGBAO_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [khosach].[SACH] (
        [ma_bansach] nvarchar(10) NOT NULL,
        [ma_khuvuc] nvarchar(10) NOT NULL,
        [ten_sach] nvarchar(200) NOT NULL,
        [tacgia] nvarchar(100) NOT NULL,
        [tinhtrang] nvarchar(30) NOT NULL DEFAULT N'Sẵn sàng',
        CONSTRAINT [PK_SACH] PRIMARY KEY ([ma_bansach]),
        CONSTRAINT [CK_SACH_tinhtrang] CHECK ([tinhtrang] IN (N'Sẵn sàng', N'Đang được mượn', N'Mất', N'Hư hỏng')),
        CONSTRAINT [FK_SACH_KHUVUC_ma_khuvuc] FOREIGN KEY ([ma_khuvuc]) REFERENCES [khosach].[KHUVUC] ([ma_khuvuc]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [quantri].[NGUOIDUNG] (
        [ma_nd] nvarchar(10) NOT NULL,
        [ten_nd] nvarchar(50) NOT NULL,
        [tendangnhap] nvarchar(30) NOT NULL,
        [matkhau] nvarchar(64) NOT NULL,
        [sdt] nvarchar(15) NULL,
        [chucvu] nvarchar(50) NULL,
        [tinhtrang] nvarchar(20) NOT NULL,
        [ma_nhom] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_NGUOIDUNG] PRIMARY KEY ([ma_nd]),
        CONSTRAINT [CK_NGUOIDUNG_tinhtrang] CHECK ([tinhtrang] IN (N'Hoạt động', N'Khóa')),
        CONSTRAINT [FK_NGUOIDUNG_NHOMNGUOIDUNG_ma_nhom] FOREIGN KEY ([ma_nhom]) REFERENCES [quantri].[NHOMNGUOIDUNG] ([ma_nhom]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [quantri].[PHANQUYEN] (
        [ma_nhom] nvarchar(10) NOT NULL,
        [ma_quyen] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_PHANQUYEN] PRIMARY KEY ([ma_nhom], [ma_quyen]),
        CONSTRAINT [FK_PHANQUYEN_NHOMNGUOIDUNG_ma_nhom] FOREIGN KEY ([ma_nhom]) REFERENCES [quantri].[NHOMNGUOIDUNG] ([ma_nhom]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PHANQUYEN_QUYEN_ma_quyen] FOREIGN KEY ([ma_quyen]) REFERENCES [quantri].[QUYEN] ([ma_quyen]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [khosach].[DAUSACH] (
        [ma_dausach] nvarchar(10) NOT NULL,
        [ten_sach] nvarchar(200) NOT NULL,
        [tacgia] nvarchar(100) NULL,
        [nhaxuatban] nvarchar(150) NULL,
        [namxb] int NULL,
        [isbn] nvarchar(20) NULL,
        [ma_theloai] nvarchar(10) NULL,
        [gioithieu] nvarchar(2000) NULL,
        [tukhoa] nvarchar(300) NULL,
        [anhbia] nvarchar(260) NULL,
        [mau_gay] char(7) NULL,
        [ngaytao] datetime2(0) NOT NULL,
        CONSTRAINT [PK_DAUSACH] PRIMARY KEY ([ma_dausach]),
        CONSTRAINT [CK_DAUSACH_mau_gay] CHECK ([mau_gay] IS NULL OR [mau_gay] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'),
        CONSTRAINT [CK_DAUSACH_namxb] CHECK ([namxb] IS NULL OR ([namxb] >= 1900 AND [namxb] <= YEAR(GETDATE()))),
        CONSTRAINT [FK_DAUSACH_THELOAI_ma_theloai] FOREIGN KEY ([ma_theloai]) REFERENCES [khosach].[THELOAI] ([ma_theloai]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [vipham].[BIENBAN] (
        [so_bienban] nvarchar(10) NOT NULL,
        [so_pmuon] nvarchar(10) NOT NULL,
        [ma_bandoc] nvarchar(10) NOT NULL,
        [thoigian_lap] datetime2(0) NOT NULL,
        [ht_thanhtoan] nvarchar(20) NOT NULL,
        [ma_nd] nvarchar(10) NOT NULL,
        [ten_nd] nvarchar(50) NOT NULL,
        [tong_tien] decimal(18,0) NOT NULL,
        CONSTRAINT [PK_BIENBAN] PRIMARY KEY ([so_bienban]),
        CONSTRAINT [CK_BIENBAN_ht_thanhtoan] CHECK ([ht_thanhtoan] IN (N'Tiền mặt', N'Chuyển khoản', N'Chưa thanh toán')),
        CONSTRAINT [FK_BIENBAN_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BIENBAN_PHIEUMUON_so_pmuon] FOREIGN KEY ([so_pmuon]) REFERENCES [muontra].[PHIEUMUON] ([so_pmuon]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[PHIEUGIAHAN] (
        [so_pgiahan] nvarchar(10) NOT NULL,
        [so_pmuon] nvarchar(10) NOT NULL,
        [ma_bandoc] nvarchar(10) NOT NULL,
        [ngaylap_pgiahan] date NOT NULL,
        [ngay_denghi] date NOT NULL,
        [ma_nd] nvarchar(10) NOT NULL,
        [ten_nd] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_PHIEUGIAHAN] PRIMARY KEY ([so_pgiahan]),
        CONSTRAINT [FK_PHIEUGIAHAN_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PHIEUGIAHAN_PHIEUMUON_so_pmuon] FOREIGN KEY ([so_pmuon]) REFERENCES [muontra].[PHIEUMUON] ([so_pmuon]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[PHIEUTRA] (
        [so_ptra] nvarchar(10) NOT NULL,
        [so_pmuon] nvarchar(10) NOT NULL,
        [ma_bandoc] nvarchar(10) NOT NULL,
        [ngaylap_ptra] date NOT NULL,
        [ngaytra] date NOT NULL,
        [ma_nd] nvarchar(10) NOT NULL,
        [ten_nd] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_PHIEUTRA] PRIMARY KEY ([so_ptra]),
        CONSTRAINT [FK_PHIEUTRA_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PHIEUTRA_PHIEUMUON_so_pmuon] FOREIGN KEY ([so_pmuon]) REFERENCES [muontra].[PHIEUMUON] ([so_pmuon]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[CT_PHIEUMUON] (
        [ma_bansach] nvarchar(10) NOT NULL,
        [so_pmuon] nvarchar(10) NOT NULL,
        [ngaymuon] date NOT NULL,
        [hantra] date NOT NULL,
        [tinhtrang_muon] nvarchar(50) NOT NULL,
        [ghichu_muon] nvarchar(200) NULL,
        [ten_sach] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_CT_PHIEUMUON] PRIMARY KEY ([ma_bansach], [so_pmuon]),
        CONSTRAINT [FK_CT_PHIEUMUON_PHIEUMUON_so_pmuon] FOREIGN KEY ([so_pmuon]) REFERENCES [muontra].[PHIEUMUON] ([so_pmuon]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CT_PHIEUMUON_SACH_ma_bansach] FOREIGN KEY ([ma_bansach]) REFERENCES [khosach].[SACH] ([ma_bansach]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [quantri].[NHATKY_DANGNHAP] (
        [ma_dangnhap] nvarchar(15) NOT NULL,
        [ma_nd] nvarchar(10) NOT NULL,
        [tg_dangnhap] datetime2(0) NOT NULL,
        [tg_dangxuat] datetime2(0) NULL,
        CONSTRAINT [PK_NHATKY_DANGNHAP] PRIMARY KEY ([ma_dangnhap]),
        CONSTRAINT [FK_NHATKY_DANGNHAP_NGUOIDUNG_ma_nd] FOREIGN KEY ([ma_nd]) REFERENCES [quantri].[NGUOIDUNG] ([ma_nd]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [quantri].[NHATKY_HOATDONG] (
        [ma_hoatdong] nvarchar(15) NOT NULL,
        [ma_nd] nvarchar(10) NOT NULL,
        [tg_thuchien] datetime2(0) NOT NULL,
        [noidung] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_NHATKY_HOATDONG] PRIMARY KEY ([ma_hoatdong]),
        CONSTRAINT [FK_NHATKY_HOATDONG_NGUOIDUNG_ma_nd] FOREIGN KEY ([ma_nd]) REFERENCES [quantri].[NGUOIDUNG] ([ma_nd]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [khosach].[DAUSACH_BANSACH] (
        [ma_bansach] nvarchar(10) NOT NULL,
        [ma_dausach] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_DAUSACH_BANSACH] PRIMARY KEY ([ma_bansach]),
        CONSTRAINT [FK_DAUSACH_BANSACH_DAUSACH_ma_dausach] FOREIGN KEY ([ma_dausach]) REFERENCES [khosach].[DAUSACH] ([ma_dausach]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DAUSACH_BANSACH_SACH_ma_bansach] FOREIGN KEY ([ma_bansach]) REFERENCES [khosach].[SACH] ([ma_bansach]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[PHIEUDATCHO] (
        [ma_datcho] nvarchar(20) NOT NULL,
        [ma_bandoc] nvarchar(10) NOT NULL,
        [ma_dausach] nvarchar(10) NOT NULL,
        [ma_bansach] nvarchar(10) NOT NULL,
        [tg_dat] datetime2(0) NOT NULL,
        [tg_hethan] datetime2(0) NOT NULL,
        [trangthai] nvarchar(20) NOT NULL,
        [tg_xuly] datetime2(0) NULL,
        [ma_nd_xuly] nvarchar(10) NULL,
        [so_pmuon] nvarchar(10) NULL,
        [lydo_huy] nvarchar(200) NULL,
        CONSTRAINT [PK_PHIEUDATCHO] PRIMARY KEY ([ma_datcho]),
        CONSTRAINT [CK_PHIEUDATCHO_trangthai] CHECK ([trangthai] IN (N'Đang giữ', N'Đã nhận', N'Đã hủy', N'Hết hạn')),
        CONSTRAINT [FK_PHIEUDATCHO_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PHIEUDATCHO_DAUSACH_ma_dausach] FOREIGN KEY ([ma_dausach]) REFERENCES [khosach].[DAUSACH] ([ma_dausach]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PHIEUDATCHO_NGUOIDUNG_ma_nd_xuly] FOREIGN KEY ([ma_nd_xuly]) REFERENCES [quantri].[NGUOIDUNG] ([ma_nd]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PHIEUDATCHO_PHIEUMUON_so_pmuon] FOREIGN KEY ([so_pmuon]) REFERENCES [muontra].[PHIEUMUON] ([so_pmuon]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PHIEUDATCHO_SACH_ma_bansach] FOREIGN KEY ([ma_bansach]) REFERENCES [khosach].[SACH] ([ma_bansach]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[YEUTHICH] (
        [ma_bandoc] nvarchar(10) NOT NULL,
        [ma_dausach] nvarchar(10) NOT NULL,
        [tg_them] datetime2(0) NOT NULL,
        CONSTRAINT [PK_YEUTHICH] PRIMARY KEY ([ma_bandoc], [ma_dausach]),
        CONSTRAINT [FK_YEUTHICH_BANDOC_ma_bandoc] FOREIGN KEY ([ma_bandoc]) REFERENCES [muontra].[BANDOC] ([ma_bandoc]) ON DELETE NO ACTION,
        CONSTRAINT [FK_YEUTHICH_DAUSACH_ma_dausach] FOREIGN KEY ([ma_dausach]) REFERENCES [khosach].[DAUSACH] ([ma_dausach]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [vipham].[CT_BIENBAN] (
        [ma_khoanthu] nvarchar(10) NOT NULL,
        [so_bienban] nvarchar(10) NOT NULL,
        [ma_bansach] nvarchar(10) NOT NULL,
        [soluong] int NOT NULL,
        [thanh_tien] decimal(18,0) NOT NULL,
        CONSTRAINT [PK_CT_BIENBAN] PRIMARY KEY ([ma_khoanthu], [so_bienban], [ma_bansach]),
        CONSTRAINT [CK_CT_BIENBAN_soluong] CHECK ([soluong] > 0),
        CONSTRAINT [FK_CT_BIENBAN_BIENBAN_so_bienban] FOREIGN KEY ([so_bienban]) REFERENCES [vipham].[BIENBAN] ([so_bienban]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CT_BIENBAN_KHOANTHU_ma_khoanthu] FOREIGN KEY ([ma_khoanthu]) REFERENCES [vipham].[KHOANTHU] ([ma_khoanthu]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[CT_PHIEUGIAHAN] (
        [ma_bansach] nvarchar(10) NOT NULL,
        [so_pgiahan] nvarchar(10) NOT NULL,
        [hantra_cu] date NOT NULL,
        [ketqua_giahan] nvarchar(20) NOT NULL,
        [hantra_moi] date NULL,
        [lydo_khonggh] nvarchar(200) NULL,
        [ten_sach] nvarchar(200) NOT NULL,
        [so_lan_giahan] int NOT NULL,
        CONSTRAINT [PK_CT_PHIEUGIAHAN] PRIMARY KEY ([ma_bansach], [so_pgiahan]),
        CONSTRAINT [CK_CT_PHIEUGIAHAN_ketqua] CHECK ([ketqua_giahan] IN (N'Đồng ý', N'Không đồng ý')),
        CONSTRAINT [FK_CT_PHIEUGIAHAN_PHIEUGIAHAN_so_pgiahan] FOREIGN KEY ([so_pgiahan]) REFERENCES [muontra].[PHIEUGIAHAN] ([so_pgiahan]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CT_PHIEUGIAHAN_SACH_ma_bansach] FOREIGN KEY ([ma_bansach]) REFERENCES [khosach].[SACH] ([ma_bansach]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE TABLE [muontra].[CT_PHIEUTRA] (
        [ma_bansach] nvarchar(10) NOT NULL,
        [so_ptra] nvarchar(10) NOT NULL,
        [tinhtrang_tra] nvarchar(50) NOT NULL,
        [ghichu_tra] nvarchar(200) NULL,
        [so_ngay_quahan] int NOT NULL,
        CONSTRAINT [PK_CT_PHIEUTRA] PRIMARY KEY ([ma_bansach], [so_ptra]),
        CONSTRAINT [CK_CT_PHIEUTRA_so_ngay_quahan] CHECK ([so_ngay_quahan] >= 0),
        CONSTRAINT [CK_CT_PHIEUTRA_tinhtrang_tra] CHECK ([tinhtrang_tra] IN (N'Bình thường', N'Hư hỏng', N'Mất')),
        CONSTRAINT [FK_CT_PHIEUTRA_PHIEUTRA_so_ptra] FOREIGN KEY ([so_ptra]) REFERENCES [muontra].[PHIEUTRA] ([so_ptra]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CT_PHIEUTRA_SACH_ma_bansach] FOREIGN KEY ([ma_bansach]) REFERENCES [khosach].[SACH] ([ma_bansach]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_BANDOC_hoten] ON [muontra].[BANDOC] ([hoten]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BANDOC_ma_svhvcb] ON [muontra].[BANDOC] ([ma_svhvcb]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BANDOC_so_thebd] ON [muontra].[BANDOC] ([so_thebd]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_BIENBAN_ma_bandoc] ON [vipham].[BIENBAN] ([ma_bandoc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_BIENBAN_so_pmuon] ON [vipham].[BIENBAN] ([so_pmuon]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_BIENBAN_thoigian_lap] ON [vipham].[BIENBAN] ([thoigian_lap]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_CT_BIENBAN_ma_bansach] ON [vipham].[CT_BIENBAN] ([ma_bansach]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_CT_BIENBAN_so_bienban] ON [vipham].[CT_BIENBAN] ([so_bienban]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_CT_PHIEUGIAHAN_so_pgiahan] ON [muontra].[CT_PHIEUGIAHAN] ([so_pgiahan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_CT_PHIEUMUON_hantra] ON [muontra].[CT_PHIEUMUON] ([hantra]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_CT_PHIEUMUON_ngaymuon] ON [muontra].[CT_PHIEUMUON] ([ngaymuon]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_CT_PHIEUMUON_so_pmuon] ON [muontra].[CT_PHIEUMUON] ([so_pmuon]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_CT_PHIEUTRA_so_ptra] ON [muontra].[CT_PHIEUTRA] ([so_ptra]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DAUSACH_isbn] ON [khosach].[DAUSACH] ([isbn]) WHERE [isbn] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_DAUSACH_ma_theloai] ON [khosach].[DAUSACH] ([ma_theloai]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_DAUSACH_namxb] ON [khosach].[DAUSACH] ([namxb]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_DAUSACH_tacgia] ON [khosach].[DAUSACH] ([tacgia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_DAUSACH_ten_sach] ON [khosach].[DAUSACH] ([ten_sach]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_DAUSACH_BANSACH_ma_dausach] ON [khosach].[DAUSACH_BANSACH] ([ma_dausach]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_NGUOIDUNG_ma_nhom] ON [quantri].[NGUOIDUNG] ([ma_nhom]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NGUOIDUNG_tendangnhap] ON [quantri].[NGUOIDUNG] ([tendangnhap]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_NHATKY_DANGNHAP_ma_nd] ON [quantri].[NHATKY_DANGNHAP] ([ma_nd]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_NHATKY_DANGNHAP_tg_dangnhap] ON [quantri].[NHATKY_DANGNHAP] ([tg_dangnhap]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_NHATKY_HOATDONG_ma_nd] ON [quantri].[NHATKY_HOATDONG] ([ma_nd]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_NHATKY_HOATDONG_tg_thuchien] ON [quantri].[NHATKY_HOATDONG] ([tg_thuchien]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHANQUYEN_ma_quyen] ON [quantri].[PHANQUYEN] ([ma_quyen]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUDATCHO_ma_dausach] ON [muontra].[PHIEUDATCHO] ([ma_dausach]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUDATCHO_ma_nd_xuly] ON [muontra].[PHIEUDATCHO] ([ma_nd_xuly]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUDATCHO_so_pmuon] ON [muontra].[PHIEUDATCHO] ([so_pmuon]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUDATCHO_trangthai_tg_hethan] ON [muontra].[PHIEUDATCHO] ([trangthai], [tg_hethan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_DATCHO_BANDOC_DAUSACH_DANGGIU] ON [muontra].[PHIEUDATCHO] ([ma_bandoc], [ma_dausach]) WHERE [trangthai] = N''Đang giữ''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_DATCHO_BANSACH_DANGGIU] ON [muontra].[PHIEUDATCHO] ([ma_bansach]) WHERE [trangthai] = N''Đang giữ''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUGIAHAN_ma_bandoc] ON [muontra].[PHIEUGIAHAN] ([ma_bandoc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUGIAHAN_ngaylap_pgiahan] ON [muontra].[PHIEUGIAHAN] ([ngaylap_pgiahan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUGIAHAN_so_pmuon] ON [muontra].[PHIEUGIAHAN] ([so_pmuon]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUMUON_ma_bandoc] ON [muontra].[PHIEUMUON] ([ma_bandoc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUMUON_ngaylap_pmuon] ON [muontra].[PHIEUMUON] ([ngaylap_pmuon]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUTRA_ma_bandoc] ON [muontra].[PHIEUTRA] ([ma_bandoc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_PHIEUTRA_ngaytra] ON [muontra].[PHIEUTRA] ([ngaytra]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PHIEUTRA_so_pmuon] ON [muontra].[PHIEUTRA] ([so_pmuon]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_SACH_ma_khuvuc] ON [khosach].[SACH] ([ma_khuvuc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_SACH_tacgia] ON [khosach].[SACH] ([tacgia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_SACH_ten_sach] ON [khosach].[SACH] ([ten_sach]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_SACH_tinhtrang] ON [khosach].[SACH] ([tinhtrang]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE UNIQUE INDEX [IX_THELOAI_ten_theloai] ON [khosach].[THELOAI] ([ten_theloai]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_THONGBAO_ma_bandoc_da_doc] ON [muontra].[THONGBAO] ([ma_bandoc], [da_doc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    CREATE INDEX [IX_YEUTHICH_ma_dausach] ON [muontra].[YEUTHICH] ([ma_dausach]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115172104_KhoiTao'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261115172104_KhoiTao', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115180000_ThuTucCapSoChungTu'
)
BEGIN
    EXEC(N'CREATE OR ALTER PROCEDURE [muontra].[usp_CapSoChungTu]
        @loai nvarchar(4),
        @ngay date,
        @so int OUTPUT
    AS
    BEGIN
        SET NOCOUNT ON;
        IF @@TRANCOUNT = 0
            THROW 50001, N''usp_CapSoChungTu phải được gọi trong transaction của nghiệp vụ lập chứng từ.'', 1;

        DECLARE @khoa date = CASE WHEN @loai = N''DC'' THEN @ngay
                                  ELSE DATEFROMPARTS(YEAR(@ngay), MONTH(@ngay), 1) END;

        UPDATE [muontra].[SOCHUNGTU] WITH (UPDLOCK, HOLDLOCK)
           SET @so = [so_cuoi] = [so_cuoi] + 1
         WHERE [loai] = @loai AND [ngay] = @khoa;

        IF @@ROWCOUNT = 0
        BEGIN
            INSERT INTO [muontra].[SOCHUNGTU] ([loai], [ngay], [so_cuoi]) VALUES (@loai, @khoa, 1);
            SET @so = 1;
        END

        IF @loai <> N''DC'' AND @so > 9999
            THROW 50002, N''Đã hết số chứng từ trong tháng (tối đa 9999).'', 1;
    END');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115180000_ThuTucCapSoChungTu'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261115180000_ThuTucCapSoChungTu', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261116064648_XuLyChuyenDen'
)
BEGIN
    CREATE TABLE [khosach].[XULY_CHUYENDEN] (
        [ma_bansach] nvarchar(10) NOT NULL,
        [so_chungtu] nvarchar(10) NOT NULL,
        [tientrinh] nvarchar(10) NOT NULL,
        [tinhtrang_moi] nvarchar(30) NOT NULL,
        [ma_nd] nvarchar(10) NOT NULL,
        [thoigian_xuly] datetime2(0) NOT NULL,
        CONSTRAINT [PK_XULY_CHUYENDEN] PRIMARY KEY ([ma_bansach], [so_chungtu], [tientrinh]),
        CONSTRAINT [CK_XULY_CHUYENDEN_tinhtrang_moi] CHECK ([tinhtrang_moi] IN (N'Sẵn sàng', N'Đang được mượn', N'Mất', N'Hư hỏng')),
        CONSTRAINT [FK_XULY_CHUYENDEN_NGUOIDUNG_ma_nd] FOREIGN KEY ([ma_nd]) REFERENCES [quantri].[NGUOIDUNG] ([ma_nd]) ON DELETE NO ACTION,
        CONSTRAINT [FK_XULY_CHUYENDEN_SACH_ma_bansach] FOREIGN KEY ([ma_bansach]) REFERENCES [khosach].[SACH] ([ma_bansach]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261116064648_XuLyChuyenDen'
)
BEGIN
    CREATE INDEX [IX_XULY_CHUYENDEN_ma_nd] ON [khosach].[XULY_CHUYENDEN] ([ma_nd]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261116064648_XuLyChuyenDen'
)
BEGIN
    INSERT INTO [khosach].[XULY_CHUYENDEN] (ma_bansach, so_chungtu, tientrinh, tinhtrang_moi, ma_nd, thoigian_xuly)
    SELECT ct.ma_bansach, ct.so_ptra, N'03.03.02', s.tinhtrang, pt.ma_nd, CAST(SYSDATETIME() AS datetime2(0))
    FROM [muontra].[CT_PHIEUTRA] ct
    JOIN [muontra].[PHIEUTRA] pt ON pt.so_ptra = ct.so_ptra
    JOIN [khosach].[SACH] s ON s.ma_bansach = ct.ma_bansach
    WHERE ct.tinhtrang_tra IN (N'Mất', N'Hư hỏng') AND s.tinhtrang = ct.tinhtrang_tra;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261116064648_XuLyChuyenDen'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261116064648_XuLyChuyenDen', N'10.0.14');
END;

COMMIT;
GO

