-- Sinh tự động bởi BaseCore.Migrator --xuat-sql (scripts/xuat-sql.sh). Không sửa tay.
-- Login SQL riêng của từng service + quyền theo schema (GIA_DINH D10, D13, D14). SID cố định để phục hồi
-- CSDL không bị "orphan user". Chạy ở SQLCMD Mode (SSMS: Query → SQLCMD Mode).
-- Điền 4 mật khẩu GIỐNG HỆT các biến cùng tên trong .env (mật khẩu service dùng để kết nối).
-- :setvar trong tệp ghi đè tham số -v của sqlcmd – sửa trực tiếp các dòng dưới. KHÔNG commit tệp đã điền.
:setvar SVC_QUANTRI_PASSWORD ""
:setvar SVC_KHOSACH_PASSWORD ""
:setvar SVC_MUONTRA_PASSWORD ""
:setvar SVC_VIPHAM_PASSWORD ""
-- Tài khoản quản trị "admin" (tùy chọn – migrator cũng tự tạo): mật khẩu ĐÃ BĂM (64 ký tự), lấy bằng
--   docker compose run --rm migrator --bam-mat-khau "<mật khẩu admin>"
:setvar MATKHAU_ADMIN_BAM ""
USE [ThuVien];
GO
IF N'$(SVC_QUANTRI_PASSWORD)' = N'' OR N'$(SVC_KHOSACH_PASSWORD)' = N'' OR N'$(SVC_MUONTRA_PASSWORD)' = N'' OR N'$(SVC_VIPHAM_PASSWORD)' = N''
BEGIN
    RAISERROR(N'Chưa điền đủ 4 mật khẩu SVC_*_PASSWORD ở đầu tệp (lấy từ .env).', 16, 1);
    SET NOEXEC ON;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'svc_quantri')
    CREATE LOGIN [svc_quantri] WITH PASSWORD = N'$(SVC_QUANTRI_PASSWORD)', SID = 0x554C4D53000000000000000000000001, DEFAULT_DATABASE = [ThuVien], CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [svc_quantri] WITH PASSWORD = N'$(SVC_QUANTRI_PASSWORD)';
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'svc_quantri')
    CREATE USER [svc_quantri] FOR LOGIN [svc_quantri];
ELSE
    ALTER USER [svc_quantri] WITH LOGIN = [svc_quantri];
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA::[quantri] TO [svc_quantri];
GRANT SELECT ON SCHEMA::[khosach] TO [svc_quantri];
GRANT SELECT ON SCHEMA::[muontra] TO [svc_quantri];
GRANT SELECT ON SCHEMA::[vipham] TO [svc_quantri];
GRANT INSERT ON [muontra].[THONGBAO] TO [svc_quantri];
DENY SELECT ON [muontra].[TAIKHOAN_BANDOC] ([matkhau]) TO [svc_quantri];
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'svc_khosach')
    CREATE LOGIN [svc_khosach] WITH PASSWORD = N'$(SVC_KHOSACH_PASSWORD)', SID = 0x554C4D53000000000000000000000002, DEFAULT_DATABASE = [ThuVien], CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [svc_khosach] WITH PASSWORD = N'$(SVC_KHOSACH_PASSWORD)';
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'svc_khosach')
    CREATE USER [svc_khosach] FOR LOGIN [svc_khosach];
ELSE
    ALTER USER [svc_khosach] WITH LOGIN = [svc_khosach];
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA::[khosach] TO [svc_khosach];
GRANT SELECT ON SCHEMA::[quantri] TO [svc_khosach];
GRANT SELECT ON SCHEMA::[muontra] TO [svc_khosach];
GRANT SELECT ON SCHEMA::[vipham] TO [svc_khosach];
GRANT INSERT ON [quantri].[NHATKY_HOATDONG] TO [svc_khosach];
GRANT UPDATE ON OBJECT::[quantri].[SEQ_NHATKY_HOATDONG] TO [svc_khosach];
DENY SELECT ON [quantri].[NGUOIDUNG] ([matkhau]) TO [svc_khosach];
GRANT INSERT ON [muontra].[THONGBAO] TO [svc_khosach];
DENY SELECT ON [muontra].[TAIKHOAN_BANDOC] ([matkhau]) TO [svc_khosach];
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'svc_muontra')
    CREATE LOGIN [svc_muontra] WITH PASSWORD = N'$(SVC_MUONTRA_PASSWORD)', SID = 0x554C4D53000000000000000000000003, DEFAULT_DATABASE = [ThuVien], CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [svc_muontra] WITH PASSWORD = N'$(SVC_MUONTRA_PASSWORD)';
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'svc_muontra')
    CREATE USER [svc_muontra] FOR LOGIN [svc_muontra];
ELSE
    ALTER USER [svc_muontra] WITH LOGIN = [svc_muontra];
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA::[muontra] TO [svc_muontra];
GRANT SELECT ON SCHEMA::[quantri] TO [svc_muontra];
GRANT SELECT ON SCHEMA::[khosach] TO [svc_muontra];
GRANT SELECT ON SCHEMA::[vipham] TO [svc_muontra];
GRANT INSERT ON [quantri].[NHATKY_HOATDONG] TO [svc_muontra];
GRANT UPDATE ON OBJECT::[quantri].[SEQ_NHATKY_HOATDONG] TO [svc_muontra];
DENY SELECT ON [quantri].[NGUOIDUNG] ([matkhau]) TO [svc_muontra];
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'svc_vipham')
    CREATE LOGIN [svc_vipham] WITH PASSWORD = N'$(SVC_VIPHAM_PASSWORD)', SID = 0x554C4D53000000000000000000000004, DEFAULT_DATABASE = [ThuVien], CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [svc_vipham] WITH PASSWORD = N'$(SVC_VIPHAM_PASSWORD)';
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'svc_vipham')
    CREATE USER [svc_vipham] FOR LOGIN [svc_vipham];
ELSE
    ALTER USER [svc_vipham] WITH LOGIN = [svc_vipham];
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA::[vipham] TO [svc_vipham];
GRANT SELECT ON SCHEMA::[quantri] TO [svc_vipham];
GRANT SELECT ON SCHEMA::[khosach] TO [svc_vipham];
GRANT SELECT ON SCHEMA::[muontra] TO [svc_vipham];
GRANT INSERT ON [quantri].[NHATKY_HOATDONG] TO [svc_vipham];
GRANT UPDATE ON OBJECT::[quantri].[SEQ_NHATKY_HOATDONG] TO [svc_vipham];
DENY SELECT ON [quantri].[NGUOIDUNG] ([matkhau]) TO [svc_vipham];
GRANT INSERT ON [muontra].[THONGBAO] TO [svc_vipham];
DENY SELECT ON [muontra].[TAIKHOAN_BANDOC] ([matkhau]) TO [svc_vipham];
GRANT EXECUTE ON [muontra].[usp_CapSoChungTu] TO [svc_vipham];
GRANT BACKUP DATABASE TO [svc_quantri];

GO
IF LEN(N'$(MATKHAU_ADMIN_BAM)') = 64 AND NOT EXISTS (SELECT 1 FROM quantri.NGUOIDUNG WHERE tendangnhap = N'admin')
    INSERT quantri.NGUOIDUNG (ma_nd, ten_nd, tendangnhap, matkhau, chucvu, tinhtrang, ma_nhom)
    VALUES (N'ND0001', N'Quản trị hệ thống', N'admin', N'$(MATKHAU_ADMIN_BAM)', N'Quản trị viên', N'Hoạt động', N'N4');
GO
SET NOEXEC OFF;
GO