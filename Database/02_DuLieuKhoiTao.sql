-- Sinh tự động bởi BaseCore.Migrator --xuat-sql (scripts/xuat-sql.sh). Không sửa tay.
-- Dữ liệu khởi tạo (file 02 mục 3): 4 nhóm người dùng N1–N4, danh mục QUYEN (SPEC 3.2, 3.4) và PHANQUYEN.
-- Tài khoản quản trị "admin" do migrator tạo (mật khẩu băm PBKDF2 từ ADMIN_PASSWORD) – xem Database/README.md.
USE [ThuVien];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;
IF NOT EXISTS (SELECT 1 FROM quantri.NHOMNGUOIDUNG WHERE ma_nhom = N'N1') INSERT quantri.NHOMNGUOIDUNG (ma_nhom, ten_nhom, mota) VALUES (N'N1', N'Mượn trả sách', N'Bộ phận mượn trả sách: thẻ bạn đọc, phiếu mượn, gia hạn, trả, báo cáo mượn trả');
IF NOT EXISTS (SELECT 1 FROM quantri.NHOMNGUOIDUNG WHERE ma_nhom = N'N2') INSERT quantri.NHOMNGUOIDUNG (ma_nhom, ten_nhom, mota) VALUES (N'N2', N'Xử lý vi phạm', N'Bộ phận xử lý vi phạm: biên bản vi phạm, báo cáo vi phạm');
IF NOT EXISTS (SELECT 1 FROM quantri.NHOMNGUOIDUNG WHERE ma_nhom = N'N3') INSERT quantri.NHOMNGUOIDUNG (ma_nhom, ten_nhom, mota) VALUES (N'N3', N'Kho sách', N'Bộ phận kho sách: danh mục sách, khu vực lưu trữ, cập nhật tình trạng sách');
IF NOT EXISTS (SELECT 1 FROM quantri.NHOMNGUOIDUNG WHERE ma_nhom = N'N4') INSERT quantri.NHOMNGUOIDUNG (ma_nhom, ten_nhom, mota) VALUES (N'N4', N'Quản trị', N'Quản trị hệ thống: tài khoản, phân quyền, nhật ký, danh mục, sao lưu – phục hồi');
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT001')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form Thẻ thông tin Bạn đọc', mota = N'Mở form Thẻ thông tin Bạn đọc' WHERE ma_quyen = N'TT001';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT001', N'Mở form Thẻ thông tin Bạn đọc', N'Mở form Thẻ thông tin Bạn đọc');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT001');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT002')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ma_svhvcb)', mota = N'Kiểm soát dữ liệu nhập (ma_svhvcb)' WHERE ma_quyen = N'TT002';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT002', N'Kiểm soát dữ liệu nhập (ma_svhvcb)', N'Kiểm soát dữ liệu nhập (ma_svhvcb)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT002');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT003')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm thông tin Bạn đọc', mota = N'Tìm kiếm thông tin Bạn đọc' WHERE ma_quyen = N'TT003';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT003', N'Tìm kiếm thông tin Bạn đọc', N'Tìm kiếm thông tin Bạn đọc');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT003');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT003');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT003');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT004')
    UPDATE quantri.QUYEN SET ten_quyen = N'Đối chiếu thông tin Thẻ Bạn đọc', mota = N'Đối chiếu thông tin Thẻ Bạn đọc' WHERE ma_quyen = N'TT004';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT004', N'Đối chiếu thông tin Thẻ Bạn đọc', N'Đối chiếu thông tin Thẻ Bạn đọc');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT004');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT005')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ma_bandoc, hoten, doituong, ma_svhvcb, lop_donvi, ngaysinh, so_thebd, …)', mota = N'Kiểm soát dữ liệu nhập (ma_bandoc, hoten, doituong, ma_svhvcb, lop_donvi, ngaysinh, so_thebd, dienthoai_email, hsd_tungay, hsd_denngay)' WHERE ma_quyen = N'TT005';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT005', N'Kiểm soát dữ liệu nhập (ma_bandoc, hoten, doituong, ma_svhvcb, lop_donvi, ngaysinh, so_thebd, …)', N'Kiểm soát dữ liệu nhập (ma_bandoc, hoten, doituong, ma_svhvcb, lop_donvi, ngaysinh, so_thebd, dienthoai_email, hsd_tungay, hsd_denngay)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT005');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT006')
    UPDATE quantri.QUYEN SET ten_quyen = N'Lưu thông tin Bạn đọc mới', mota = N'Lưu thông tin Bạn đọc mới' WHERE ma_quyen = N'TT006';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT006', N'Lưu thông tin Bạn đọc mới', N'Lưu thông tin Bạn đọc mới');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT006');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT007')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm tra hạn sử dụng Thẻ', mota = N'Kiểm tra hạn sử dụng Thẻ' WHERE ma_quyen = N'TT007';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT007', N'Kiểm tra hạn sử dụng Thẻ', N'Kiểm tra hạn sử dụng Thẻ');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT007');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT008')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách sách Bạn đọc đang mượn', mota = N'Truy xuất danh sách sách Bạn đọc đang mượn' WHERE ma_quyen = N'TT008';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT008', N'Truy xuất danh sách sách Bạn đọc đang mượn', N'Truy xuất danh sách sách Bạn đọc đang mượn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT008');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT008');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT008');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT008');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT009')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm tra sách mượn quá hạn', mota = N'Kiểm tra sách mượn quá hạn' WHERE ma_quyen = N'TT009';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT009', N'Kiểm tra sách mượn quá hạn', N'Kiểm tra sách mượn quá hạn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT009');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT010')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách vi phạm chưa thanh toán', mota = N'Truy xuất danh sách vi phạm chưa thanh toán' WHERE ma_quyen = N'TT010';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT010', N'Truy xuất danh sách vi phạm chưa thanh toán', N'Truy xuất danh sách vi phạm chưa thanh toán');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT010');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT010');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT010');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT010');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT011')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thông báo kết quả kiểm tra điều kiện mượn', mota = N'Thông báo kết quả kiểm tra điều kiện mượn' WHERE ma_quyen = N'TT011';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT011', N'Thông báo kết quả kiểm tra điều kiện mượn', N'Thông báo kết quả kiểm tra điều kiện mượn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT011');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT012')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form Phiếu mượn sách', mota = N'Mở form Phiếu mượn sách' WHERE ma_quyen = N'TT012';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT012', N'Mở form Phiếu mượn sách', N'Mở form Phiếu mượn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT012');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT013')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm Phiếu mượn sách lập trong ngày', mota = N'Tìm kiếm Phiếu mượn sách lập trong ngày' WHERE ma_quyen = N'TT013';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT013', N'Tìm kiếm Phiếu mượn sách lập trong ngày', N'Tìm kiếm Phiếu mượn sách lập trong ngày');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT013');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT013');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT013');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT013');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT014')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm thông tin bản sách', mota = N'Tìm kiếm thông tin bản sách' WHERE ma_quyen = N'TT014';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT014', N'Tìm kiếm thông tin bản sách', N'Tìm kiếm thông tin bản sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT014');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT014');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT014');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT015')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm vị trí bản sách', mota = N'Tìm kiếm vị trí bản sách' WHERE ma_quyen = N'TT015';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT015', N'Tìm kiếm vị trí bản sách', N'Tìm kiếm vị trí bản sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT015');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT015');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT015');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT016')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển Phiếu yêu cầu lấy sách trong kho cho Bộ phận kho sách', mota = N'Chuyển Phiếu yêu cầu lấy sách trong kho cho Bộ phận kho sách' WHERE ma_quyen = N'TT016';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT016', N'Chuyển Phiếu yêu cầu lấy sách trong kho cho Bộ phận kho sách', N'Chuyển Phiếu yêu cầu lấy sách trong kho cho Bộ phận kho sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT016');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT017')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển yêu cầu đổi bản sách cho Bộ phận kho sách', mota = N'Chuyển yêu cầu đổi bản sách cho Bộ phận kho sách' WHERE ma_quyen = N'TT017';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT017', N'Chuyển yêu cầu đổi bản sách cho Bộ phận kho sách', N'Chuyển yêu cầu đổi bản sách cho Bộ phận kho sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT017');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT018')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận kết quả đáp ứng sách', mota = N'Tiếp nhận kết quả đáp ứng sách' WHERE ma_quyen = N'TT018';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT018', N'Tiếp nhận kết quả đáp ứng sách', N'Tiếp nhận kết quả đáp ứng sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT018');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT019')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thông báo không đáp ứng sách', mota = N'Thông báo không đáp ứng sách' WHERE ma_quyen = N'TT019';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT019', N'Thông báo không đáp ứng sách', N'Thông báo không đáp ứng sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT019');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT020')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ma_bandoc, ma_bansach, ngaymuon, tinhtrang_muon, ghichu_muon)', mota = N'Kiểm soát dữ liệu nhập (ma_bandoc, ma_bansach, ngaymuon, tinhtrang_muon, ghichu_muon)' WHERE ma_quyen = N'TT020';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT020', N'Kiểm soát dữ liệu nhập (ma_bandoc, ma_bansach, ngaymuon, tinhtrang_muon, ghichu_muon)', N'Kiểm soát dữ liệu nhập (ma_bandoc, ma_bansach, ngaymuon, tinhtrang_muon, ghichu_muon)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT020');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT021')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính hạn trả = ngaymuon + số ngày được mượn', mota = N'Tính hạn trả = ngaymuon + số ngày được mượn' WHERE ma_quyen = N'TT021';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT021', N'Tính hạn trả = ngaymuon + số ngày được mượn', N'Tính hạn trả = ngaymuon + số ngày được mượn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT021');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT022')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị chi tiết thông tin Phiếu mượn sách', mota = N'Hiển thị chi tiết thông tin Phiếu mượn sách' WHERE ma_quyen = N'TT022';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT022', N'Hiển thị chi tiết thông tin Phiếu mượn sách', N'Hiển thị chi tiết thông tin Phiếu mượn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT022');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT023')
    UPDATE quantri.QUYEN SET ten_quyen = N'Bổ sung bản sách vào Phiếu mượn sách đã lập trong ngày', mota = N'Bổ sung bản sách vào Phiếu mượn sách đã lập trong ngày' WHERE ma_quyen = N'TT023';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT023', N'Bổ sung bản sách vào Phiếu mượn sách đã lập trong ngày', N'Bổ sung bản sách vào Phiếu mượn sách đã lập trong ngày');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT023');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT024')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tự động sinh so_pmuon, ngaylap_pmuon khi lưu', mota = N'Tự động sinh so_pmuon, ngaylap_pmuon khi lưu' WHERE ma_quyen = N'TT024';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT024', N'Tự động sinh so_pmuon, ngaylap_pmuon khi lưu', N'Tự động sinh so_pmuon, ngaylap_pmuon khi lưu');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT024');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT025')
    UPDATE quantri.QUYEN SET ten_quyen = N'Lưu thông tin Phiếu mượn sách', mota = N'Lưu thông tin Phiếu mượn sách' WHERE ma_quyen = N'TT025';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT025', N'Lưu thông tin Phiếu mượn sách', N'Lưu thông tin Phiếu mượn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT025');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT026')
    UPDATE quantri.QUYEN SET ten_quyen = N'Cập nhật thông tin Phiếu mượn sách', mota = N'Cập nhật thông tin Phiếu mượn sách' WHERE ma_quyen = N'TT026';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT026', N'Cập nhật thông tin Phiếu mượn sách', N'Cập nhật thông tin Phiếu mượn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT026');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT027')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển thông tin sách đã cho mượn cho Bộ phận kho sách', mota = N'Chuyển thông tin sách đã cho mượn cho Bộ phận kho sách' WHERE ma_quyen = N'TT027';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT027', N'Chuyển thông tin sách đã cho mượn cho Bộ phận kho sách', N'Chuyển thông tin sách đã cho mượn cho Bộ phận kho sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT027');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT028')
    UPDATE quantri.QUYEN SET ten_quyen = N'In Phiếu mượn sách', mota = N'In Phiếu mượn sách' WHERE ma_quyen = N'TT028';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT028', N'In Phiếu mượn sách', N'In Phiếu mượn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT028');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT029')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thông báo điều kiện sử dụng sách', mota = N'Thông báo điều kiện sử dụng sách' WHERE ma_quyen = N'TT029';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT029', N'Thông báo điều kiện sử dụng sách', N'Thông báo điều kiện sử dụng sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT029');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT030')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form Phiếu gia hạn sách', mota = N'Mở form Phiếu gia hạn sách' WHERE ma_quyen = N'TT030';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT030', N'Mở form Phiếu gia hạn sách', N'Mở form Phiếu gia hạn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT030');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT031')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (so_pmuon)', mota = N'Kiểm soát dữ liệu nhập (so_pmuon)' WHERE ma_quyen = N'TT031';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT031', N'Kiểm soát dữ liệu nhập (so_pmuon)', N'Kiểm soát dữ liệu nhập (so_pmuon)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT031');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT031');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT031');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT032')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm thông tin Phiếu mượn sách', mota = N'Tìm kiếm thông tin Phiếu mượn sách' WHERE ma_quyen = N'TT032';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT032', N'Tìm kiếm thông tin Phiếu mượn sách', N'Tìm kiếm thông tin Phiếu mượn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT032');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT032');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT032');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT032');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT033')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ma_bansach, ngay_denghi)', mota = N'Kiểm soát dữ liệu nhập (ma_bansach, ngay_denghi)' WHERE ma_quyen = N'TT033';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT033', N'Kiểm soát dữ liệu nhập (ma_bansach, ngay_denghi)', N'Kiểm soát dữ liệu nhập (ma_bansach, ngay_denghi)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT033');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT034')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính số lần đã gia hạn = COUNT(lần gia hạn được đồng ý của bản sách)', mota = N'Tính số lần đã gia hạn = COUNT(lần gia hạn được đồng ý của bản sách)' WHERE ma_quyen = N'TT034';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT034', N'Tính số lần đã gia hạn = COUNT(lần gia hạn được đồng ý của bản sách)', N'Tính số lần đã gia hạn = COUNT(lần gia hạn được đồng ý của bản sách)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT034');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT035')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm tra điều kiện gia hạn', mota = N'Kiểm tra điều kiện gia hạn' WHERE ma_quyen = N'TT035';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT035', N'Kiểm tra điều kiện gia hạn', N'Kiểm tra điều kiện gia hạn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT035');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT036')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thông báo lý do từ chối gia hạn', mota = N'Thông báo lý do từ chối gia hạn' WHERE ma_quyen = N'TT036';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT036', N'Thông báo lý do từ chối gia hạn', N'Thông báo lý do từ chối gia hạn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT036');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT037')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính hạn trả mới = hantra_cu + số ngày được gia hạn', mota = N'Tính hạn trả mới = hantra_cu + số ngày được gia hạn' WHERE ma_quyen = N'TT037';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT037', N'Tính hạn trả mới = hantra_cu + số ngày được gia hạn', N'Tính hạn trả mới = hantra_cu + số ngày được gia hạn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT037');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT038')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị chi tiết thông tin Phiếu gia hạn sách', mota = N'Hiển thị chi tiết thông tin Phiếu gia hạn sách' WHERE ma_quyen = N'TT038';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT038', N'Hiển thị chi tiết thông tin Phiếu gia hạn sách', N'Hiển thị chi tiết thông tin Phiếu gia hạn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT038');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT039')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tự động sinh so_pgiahan, ngaylap_pgiahan khi lưu', mota = N'Tự động sinh so_pgiahan, ngaylap_pgiahan khi lưu' WHERE ma_quyen = N'TT039';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT039', N'Tự động sinh so_pgiahan, ngaylap_pgiahan khi lưu', N'Tự động sinh so_pgiahan, ngaylap_pgiahan khi lưu');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT039');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT040')
    UPDATE quantri.QUYEN SET ten_quyen = N'Lưu thông tin Phiếu gia hạn sách', mota = N'Lưu thông tin Phiếu gia hạn sách' WHERE ma_quyen = N'TT040';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT040', N'Lưu thông tin Phiếu gia hạn sách', N'Lưu thông tin Phiếu gia hạn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT040');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT041')
    UPDATE quantri.QUYEN SET ten_quyen = N'Cập nhật thông tin Phiếu gia hạn sách', mota = N'Cập nhật thông tin Phiếu gia hạn sách' WHERE ma_quyen = N'TT041';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT041', N'Cập nhật thông tin Phiếu gia hạn sách', N'Cập nhật thông tin Phiếu gia hạn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT041');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT042')
    UPDATE quantri.QUYEN SET ten_quyen = N'In Phiếu gia hạn sách', mota = N'In Phiếu gia hạn sách' WHERE ma_quyen = N'TT042';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT042', N'In Phiếu gia hạn sách', N'In Phiếu gia hạn sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT042');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT043')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form Phiếu trả sách', mota = N'Mở form Phiếu trả sách' WHERE ma_quyen = N'TT043';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT043', N'Mở form Phiếu trả sách', N'Mở form Phiếu trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT043');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT044')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm Phiếu trả sách đã lập', mota = N'Tìm kiếm Phiếu trả sách đã lập' WHERE ma_quyen = N'TT044';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT044', N'Tìm kiếm Phiếu trả sách đã lập', N'Tìm kiếm Phiếu trả sách đã lập');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT044');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT044');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT044');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT045')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ma_bansach, ngaytra, tinhtrang_tra, ghichu_tra)', mota = N'Kiểm soát dữ liệu nhập (ma_bansach, ngaytra, tinhtrang_tra, ghichu_tra)' WHERE ma_quyen = N'TT045';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT045', N'Kiểm soát dữ liệu nhập (ma_bansach, ngaytra, tinhtrang_tra, ghichu_tra)', N'Kiểm soát dữ liệu nhập (ma_bansach, ngaytra, tinhtrang_tra, ghichu_tra)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT045');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT046')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thông báo sai lệch sách trả', mota = N'Thông báo sai lệch sách trả' WHERE ma_quyen = N'TT046';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT046', N'Thông báo sai lệch sách trả', N'Thông báo sai lệch sách trả');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT046');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT047')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính số ngày quá hạn = ngaytra − hantra', mota = N'Tính số ngày quá hạn = ngaytra − hantra' WHERE ma_quyen = N'TT047';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT047', N'Tính số ngày quá hạn = ngaytra − hantra', N'Tính số ngày quá hạn = ngaytra − hantra');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT047');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT048')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị chi tiết thông tin Phiếu trả sách', mota = N'Hiển thị chi tiết thông tin Phiếu trả sách' WHERE ma_quyen = N'TT048';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT048', N'Hiển thị chi tiết thông tin Phiếu trả sách', N'Hiển thị chi tiết thông tin Phiếu trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT048');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT049')
    UPDATE quantri.QUYEN SET ten_quyen = N'Bổ sung bản sách vào Phiếu trả sách đã lập', mota = N'Bổ sung bản sách vào Phiếu trả sách đã lập' WHERE ma_quyen = N'TT049';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT049', N'Bổ sung bản sách vào Phiếu trả sách đã lập', N'Bổ sung bản sách vào Phiếu trả sách đã lập');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT049');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT050')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tự động sinh so_ptra, ngaylap_ptra khi lưu', mota = N'Tự động sinh so_ptra, ngaylap_ptra khi lưu' WHERE ma_quyen = N'TT050';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT050', N'Tự động sinh so_ptra, ngaylap_ptra khi lưu', N'Tự động sinh so_ptra, ngaylap_ptra khi lưu');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT050');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT051')
    UPDATE quantri.QUYEN SET ten_quyen = N'Lưu thông tin Phiếu trả sách', mota = N'Lưu thông tin Phiếu trả sách' WHERE ma_quyen = N'TT051';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT051', N'Lưu thông tin Phiếu trả sách', N'Lưu thông tin Phiếu trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT051');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT052')
    UPDATE quantri.QUYEN SET ten_quyen = N'Cập nhật thông tin Phiếu trả sách', mota = N'Cập nhật thông tin Phiếu trả sách' WHERE ma_quyen = N'TT052';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT052', N'Cập nhật thông tin Phiếu trả sách', N'Cập nhật thông tin Phiếu trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT052');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT053')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển trường hợp vi phạm cho Bộ phận xử lý vi phạm', mota = N'Chuyển trường hợp vi phạm cho Bộ phận xử lý vi phạm' WHERE ma_quyen = N'TT053';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT053', N'Chuyển trường hợp vi phạm cho Bộ phận xử lý vi phạm', N'Chuyển trường hợp vi phạm cho Bộ phận xử lý vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT053');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT054')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận kết quả xử lý vi phạm', mota = N'Tiếp nhận kết quả xử lý vi phạm' WHERE ma_quyen = N'TT054';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT054', N'Tiếp nhận kết quả xử lý vi phạm', N'Tiếp nhận kết quả xử lý vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT054');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT055')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển sách trả cho Bộ phận kho sách', mota = N'Chuyển sách trả cho Bộ phận kho sách' WHERE ma_quyen = N'TT055';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT055', N'Chuyển sách trả cho Bộ phận kho sách', N'Chuyển sách trả cho Bộ phận kho sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT055');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT056')
    UPDATE quantri.QUYEN SET ten_quyen = N'In Phiếu trả sách', mota = N'In Phiếu trả sách' WHERE ma_quyen = N'TT056';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT056', N'In Phiếu trả sách', N'In Phiếu trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT056');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT057')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form tổng hợp tình hình mượn trả sách', mota = N'Mở form tổng hợp tình hình mượn trả sách' WHERE ma_quyen = N'TT057';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT057', N'Mở form tổng hợp tình hình mượn trả sách', N'Mở form tổng hợp tình hình mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT057');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT058')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (kỳ báo cáo)', mota = N'Kiểm soát dữ liệu nhập (kỳ báo cáo)' WHERE ma_quyen = N'TT058';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT058', N'Kiểm soát dữ liệu nhập (kỳ báo cáo)', N'Kiểm soát dữ liệu nhập (kỳ báo cáo)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT058');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT058');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT059')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách lượt mượn trong kỳ', mota = N'Truy xuất danh sách lượt mượn trong kỳ' WHERE ma_quyen = N'TT059';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT059', N'Truy xuất danh sách lượt mượn trong kỳ', N'Truy xuất danh sách lượt mượn trong kỳ');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT059');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT059');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT059');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT059');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT060')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách lượt gia hạn trong kỳ', mota = N'Truy xuất danh sách lượt gia hạn trong kỳ' WHERE ma_quyen = N'TT060';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT060', N'Truy xuất danh sách lượt gia hạn trong kỳ', N'Truy xuất danh sách lượt gia hạn trong kỳ');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT060');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT060');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT060');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT060');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT061')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách lượt trả trong kỳ', mota = N'Truy xuất danh sách lượt trả trong kỳ' WHERE ma_quyen = N'TT061';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT061', N'Truy xuất danh sách lượt trả trong kỳ', N'Truy xuất danh sách lượt trả trong kỳ');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT061');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT061');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT061');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT062')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển yêu cầu đối chiếu tình trạng sách cho Bộ phận kho sách', mota = N'Chuyển yêu cầu đối chiếu tình trạng sách cho Bộ phận kho sách' WHERE ma_quyen = N'TT062';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT062', N'Chuyển yêu cầu đối chiếu tình trạng sách cho Bộ phận kho sách', N'Chuyển yêu cầu đối chiếu tình trạng sách cho Bộ phận kho sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT062');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT063')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận dữ liệu tình trạng sách', mota = N'Tiếp nhận dữ liệu tình trạng sách' WHERE ma_quyen = N'TT063';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT063', N'Tiếp nhận dữ liệu tình trạng sách', N'Tiếp nhận dữ liệu tình trạng sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT063');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT064')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính tổng số lượt mượn = COUNT(lượt mượn trong kỳ)', mota = N'Tính tổng số lượt mượn = COUNT(lượt mượn trong kỳ)' WHERE ma_quyen = N'TT064';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT064', N'Tính tổng số lượt mượn = COUNT(lượt mượn trong kỳ)', N'Tính tổng số lượt mượn = COUNT(lượt mượn trong kỳ)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT064');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT065')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính tổng số lượt gia hạn = COUNT(lượt gia hạn được đồng ý trong kỳ)', mota = N'Tính tổng số lượt gia hạn = COUNT(lượt gia hạn được đồng ý trong kỳ)' WHERE ma_quyen = N'TT065';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT065', N'Tính tổng số lượt gia hạn = COUNT(lượt gia hạn được đồng ý trong kỳ)', N'Tính tổng số lượt gia hạn = COUNT(lượt gia hạn được đồng ý trong kỳ)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT065');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT066')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính tổng số lượt trả = COUNT(lượt trả trong kỳ)', mota = N'Tính tổng số lượt trả = COUNT(lượt trả trong kỳ)' WHERE ma_quyen = N'TT066';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT066', N'Tính tổng số lượt trả = COUNT(lượt trả trong kỳ)', N'Tính tổng số lượt trả = COUNT(lượt trả trong kỳ)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT066');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT067')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính số sách quá hạn = COUNT(sách chưa trả có hantra < ngày cuối kỳ báo cáo)', mota = N'Tính số sách quá hạn = COUNT(sách chưa trả có hantra < ngày cuối kỳ báo cáo)' WHERE ma_quyen = N'TT067';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT067', N'Tính số sách quá hạn = COUNT(sách chưa trả có hantra < ngày cuối kỳ báo cáo)', N'Tính số sách quá hạn = COUNT(sách chưa trả có hantra < ngày cuối kỳ báo cáo)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT067');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT068')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị số liệu tổng hợp tình hình mượn trả sách', mota = N'Hiển thị số liệu tổng hợp tình hình mượn trả sách' WHERE ma_quyen = N'TT068';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT068', N'Hiển thị số liệu tổng hợp tình hình mượn trả sách', N'Hiển thị số liệu tổng hợp tình hình mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT068');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT069')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form Báo cáo tình hình mượn trả sách', mota = N'Mở form Báo cáo tình hình mượn trả sách' WHERE ma_quyen = N'TT069';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT069', N'Mở form Báo cáo tình hình mượn trả sách', N'Mở form Báo cáo tình hình mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT069');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT070')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận số liệu tổng hợp tình hình mượn trả sách', mota = N'Tiếp nhận số liệu tổng hợp tình hình mượn trả sách' WHERE ma_quyen = N'TT070';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT070', N'Tiếp nhận số liệu tổng hợp tình hình mượn trả sách', N'Tiếp nhận số liệu tổng hợp tình hình mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT070');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT071')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị chi tiết Báo cáo tình hình mượn trả sách', mota = N'Hiển thị chi tiết Báo cáo tình hình mượn trả sách' WHERE ma_quyen = N'TT071';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT071', N'Hiển thị chi tiết Báo cáo tình hình mượn trả sách', N'Hiển thị chi tiết Báo cáo tình hình mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT071');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT072')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tự động sinh ngày lập báo cáo', mota = N'Tự động sinh ngày lập báo cáo' WHERE ma_quyen = N'TT072';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT072', N'Tự động sinh ngày lập báo cáo', N'Tự động sinh ngày lập báo cáo');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT072');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT072');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT073')
    UPDATE quantri.QUYEN SET ten_quyen = N'In Báo cáo tình hình mượn trả sách', mota = N'In Báo cáo tình hình mượn trả sách' WHERE ma_quyen = N'TT073';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT073', N'In Báo cáo tình hình mượn trả sách', N'In Báo cáo tình hình mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT073');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT074')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form Biên bản vi phạm', mota = N'Mở form Biên bản vi phạm' WHERE ma_quyen = N'TT074';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT074', N'Mở form Biên bản vi phạm', N'Mở form Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT074');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT075')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận trường hợp vi phạm từ Bộ phận mượn trả sách', mota = N'Tiếp nhận trường hợp vi phạm từ Bộ phận mượn trả sách' WHERE ma_quyen = N'TT075';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT075', N'Tiếp nhận trường hợp vi phạm từ Bộ phận mượn trả sách', N'Tiếp nhận trường hợp vi phạm từ Bộ phận mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT075');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT076')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm thông tin khoản thu', mota = N'Tìm kiếm thông tin khoản thu' WHERE ma_quyen = N'TT076';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT076', N'Tìm kiếm thông tin khoản thu', N'Tìm kiếm thông tin khoản thu');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT076');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT076');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT077')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ma_khoanthu, ma_bansach, soluong)', mota = N'Kiểm soát dữ liệu nhập (ma_khoanthu, ma_bansach, soluong)' WHERE ma_quyen = N'TT077';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT077', N'Kiểm soát dữ liệu nhập (ma_khoanthu, ma_bansach, soluong)', N'Kiểm soát dữ liệu nhập (ma_khoanthu, ma_bansach, soluong)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT077');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT078')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính thành tiền = soluong * dongia', mota = N'Tính thành tiền = soluong * dongia' WHERE ma_quyen = N'TT078';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT078', N'Tính thành tiền = soluong * dongia', N'Tính thành tiền = soluong * dongia');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT078');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT079')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính tổng tiền cho Biên bản vi phạm', mota = N'Tính tổng tiền cho Biên bản vi phạm' WHERE ma_quyen = N'TT079';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT079', N'Tính tổng tiền cho Biên bản vi phạm', N'Tính tổng tiền cho Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT079');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT080')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị chi tiết thông tin Biên bản vi phạm', mota = N'Hiển thị chi tiết thông tin Biên bản vi phạm' WHERE ma_quyen = N'TT080';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT080', N'Hiển thị chi tiết thông tin Biên bản vi phạm', N'Hiển thị chi tiết thông tin Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT080');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT081')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thông báo khoản thu', mota = N'Thông báo khoản thu' WHERE ma_quyen = N'TT081';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT081', N'Thông báo khoản thu', N'Thông báo khoản thu');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT081');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT082')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ht_thanhtoan)', mota = N'Kiểm soát dữ liệu nhập (ht_thanhtoan)' WHERE ma_quyen = N'TT082';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT082', N'Kiểm soát dữ liệu nhập (ht_thanhtoan)', N'Kiểm soát dữ liệu nhập (ht_thanhtoan)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT082');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT083')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tự động sinh so_bienban, thoigian_lap khi lưu', mota = N'Tự động sinh so_bienban, thoigian_lap khi lưu' WHERE ma_quyen = N'TT083';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT083', N'Tự động sinh so_bienban, thoigian_lap khi lưu', N'Tự động sinh so_bienban, thoigian_lap khi lưu');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT083');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT084')
    UPDATE quantri.QUYEN SET ten_quyen = N'Lưu thông tin Biên bản vi phạm', mota = N'Lưu thông tin Biên bản vi phạm' WHERE ma_quyen = N'TT084';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT084', N'Lưu thông tin Biên bản vi phạm', N'Lưu thông tin Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT084');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT085')
    UPDATE quantri.QUYEN SET ten_quyen = N'Ghi nhận thanh toán vào Biên bản vi phạm', mota = N'Ghi nhận thanh toán vào Biên bản vi phạm' WHERE ma_quyen = N'TT085';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT085', N'Ghi nhận thanh toán vào Biên bản vi phạm', N'Ghi nhận thanh toán vào Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT085');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT086')
    UPDATE quantri.QUYEN SET ten_quyen = N'Cập nhật thông tin Biên bản vi phạm', mota = N'Cập nhật thông tin Biên bản vi phạm' WHERE ma_quyen = N'TT086';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT086', N'Cập nhật thông tin Biên bản vi phạm', N'Cập nhật thông tin Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT086');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT087')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển kết quả xử lý vi phạm cho Bộ phận mượn trả sách', mota = N'Chuyển kết quả xử lý vi phạm cho Bộ phận mượn trả sách' WHERE ma_quyen = N'TT087';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT087', N'Chuyển kết quả xử lý vi phạm cho Bộ phận mượn trả sách', N'Chuyển kết quả xử lý vi phạm cho Bộ phận mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT087');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT088')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển thông tin sách bị mất, hư hỏng cho Bộ phận kho sách', mota = N'Chuyển thông tin sách bị mất, hư hỏng cho Bộ phận kho sách' WHERE ma_quyen = N'TT088';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT088', N'Chuyển thông tin sách bị mất, hư hỏng cho Bộ phận kho sách', N'Chuyển thông tin sách bị mất, hư hỏng cho Bộ phận kho sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT088');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT089')
    UPDATE quantri.QUYEN SET ten_quyen = N'In Biên bản vi phạm', mota = N'In Biên bản vi phạm' WHERE ma_quyen = N'TT089';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT089', N'In Biên bản vi phạm', N'In Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT089');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT090')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form tổng hợp tình hình vi phạm', mota = N'Mở form tổng hợp tình hình vi phạm' WHERE ma_quyen = N'TT090';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT090', N'Mở form tổng hợp tình hình vi phạm', N'Mở form tổng hợp tình hình vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT090');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT091')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách Biên bản vi phạm trong kỳ', mota = N'Truy xuất danh sách Biên bản vi phạm trong kỳ' WHERE ma_quyen = N'TT091';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT091', N'Truy xuất danh sách Biên bản vi phạm trong kỳ', N'Truy xuất danh sách Biên bản vi phạm trong kỳ');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT091');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT091');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT091');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT091');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT092')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính số trường hợp vi phạm = COUNT(Biên bản vi phạm theo khoản thu)', mota = N'Tính số trường hợp vi phạm = COUNT(Biên bản vi phạm theo khoản thu)' WHERE ma_quyen = N'TT092';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT092', N'Tính số trường hợp vi phạm = COUNT(Biên bản vi phạm theo khoản thu)', N'Tính số trường hợp vi phạm = COUNT(Biên bản vi phạm theo khoản thu)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT092');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT093')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính tổng tiền vi phạm = SUM(soluong * dongia theo khoản thu)', mota = N'Tính tổng tiền vi phạm = SUM(soluong * dongia theo khoản thu)' WHERE ma_quyen = N'TT093';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT093', N'Tính tổng tiền vi phạm = SUM(soluong * dongia theo khoản thu)', N'Tính tổng tiền vi phạm = SUM(soluong * dongia theo khoản thu)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT093');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT094')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị số liệu tổng hợp tình hình vi phạm', mota = N'Hiển thị số liệu tổng hợp tình hình vi phạm' WHERE ma_quyen = N'TT094';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT094', N'Hiển thị số liệu tổng hợp tình hình vi phạm', N'Hiển thị số liệu tổng hợp tình hình vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT094');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT095')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form Báo cáo tình hình vi phạm', mota = N'Mở form Báo cáo tình hình vi phạm' WHERE ma_quyen = N'TT095';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT095', N'Mở form Báo cáo tình hình vi phạm', N'Mở form Báo cáo tình hình vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT095');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT096')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận số liệu tổng hợp tình hình vi phạm', mota = N'Tiếp nhận số liệu tổng hợp tình hình vi phạm' WHERE ma_quyen = N'TT096';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT096', N'Tiếp nhận số liệu tổng hợp tình hình vi phạm', N'Tiếp nhận số liệu tổng hợp tình hình vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT096');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT097')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị chi tiết Báo cáo tình hình vi phạm', mota = N'Hiển thị chi tiết Báo cáo tình hình vi phạm' WHERE ma_quyen = N'TT097';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT097', N'Hiển thị chi tiết Báo cáo tình hình vi phạm', N'Hiển thị chi tiết Báo cáo tình hình vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT097');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT098')
    UPDATE quantri.QUYEN SET ten_quyen = N'In Báo cáo tình hình vi phạm', mota = N'In Báo cáo tình hình vi phạm' WHERE ma_quyen = N'TT098';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT098', N'In Báo cáo tình hình vi phạm', N'In Báo cáo tình hình vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT098');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT099')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form kiểm tra khả năng đáp ứng sách', mota = N'Mở form kiểm tra khả năng đáp ứng sách' WHERE ma_quyen = N'TT099';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT099', N'Mở form kiểm tra khả năng đáp ứng sách', N'Mở form kiểm tra khả năng đáp ứng sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT099');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT100')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận Phiếu yêu cầu lấy sách, yêu cầu đổi bản sách', mota = N'Tiếp nhận Phiếu yêu cầu lấy sách, yêu cầu đổi bản sách' WHERE ma_quyen = N'TT100';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT100', N'Tiếp nhận Phiếu yêu cầu lấy sách, yêu cầu đổi bản sách', N'Tiếp nhận Phiếu yêu cầu lấy sách, yêu cầu đổi bản sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT100');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT101')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (ten_sach, tacgia)', mota = N'Kiểm soát dữ liệu nhập (ten_sach, tacgia)' WHERE ma_quyen = N'TT101';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT101', N'Kiểm soát dữ liệu nhập (ten_sach, tacgia)', N'Kiểm soát dữ liệu nhập (ten_sach, tacgia)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT101');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT102')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách bản sách đang được mượn', mota = N'Truy xuất danh sách bản sách đang được mượn' WHERE ma_quyen = N'TT102';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT102', N'Truy xuất danh sách bản sách đang được mượn', N'Truy xuất danh sách bản sách đang được mượn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT102');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT102');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT102');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT102');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT103')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm tra bản sách sẵn sàng cho mượn', mota = N'Kiểm tra bản sách sẵn sàng cho mượn' WHERE ma_quyen = N'TT103';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT103', N'Kiểm tra bản sách sẵn sàng cho mượn', N'Kiểm tra bản sách sẵn sàng cho mượn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT103');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT104')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị bản sách đáp ứng', mota = N'Hiển thị bản sách đáp ứng' WHERE ma_quyen = N'TT104';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT104', N'Hiển thị bản sách đáp ứng', N'Hiển thị bản sách đáp ứng');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT104');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT105')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển kết quả đáp ứng sách cho Bộ phận mượn trả sách', mota = N'Chuyển kết quả đáp ứng sách cho Bộ phận mượn trả sách' WHERE ma_quyen = N'TT105';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT105', N'Chuyển kết quả đáp ứng sách cho Bộ phận mượn trả sách', N'Chuyển kết quả đáp ứng sách cho Bộ phận mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT105');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT106')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form cập nhật tình trạng sách', mota = N'Mở form cập nhật tình trạng sách' WHERE ma_quyen = N'TT106';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT106', N'Mở form cập nhật tình trạng sách', N'Mở form cập nhật tình trạng sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT106');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT107')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận thông tin sách đã cho mượn', mota = N'Tiếp nhận thông tin sách đã cho mượn' WHERE ma_quyen = N'TT107';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT107', N'Tiếp nhận thông tin sách đã cho mượn', N'Tiếp nhận thông tin sách đã cho mượn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT107');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT108')
    UPDATE quantri.QUYEN SET ten_quyen = N'Cập nhật tình trạng sách đang được mượn', mota = N'Cập nhật tình trạng sách đang được mượn' WHERE ma_quyen = N'TT108';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT108', N'Cập nhật tình trạng sách đang được mượn', N'Cập nhật tình trạng sách đang được mượn');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT108');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT108');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT109')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận sách trả', mota = N'Tiếp nhận sách trả' WHERE ma_quyen = N'TT109';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT109', N'Tiếp nhận sách trả', N'Tiếp nhận sách trả');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT109');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT110')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (so_ptra)', mota = N'Kiểm soát dữ liệu nhập (so_ptra)' WHERE ma_quyen = N'TT110';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT110', N'Kiểm soát dữ liệu nhập (so_ptra)', N'Kiểm soát dữ liệu nhập (so_ptra)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT110');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT111')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm thông tin Phiếu trả sách', mota = N'Tìm kiếm thông tin Phiếu trả sách' WHERE ma_quyen = N'TT111';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT111', N'Tìm kiếm thông tin Phiếu trả sách', N'Tìm kiếm thông tin Phiếu trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT111');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT111');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT111');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT112')
    UPDATE quantri.QUYEN SET ten_quyen = N'Cập nhật tình trạng sách sau trả', mota = N'Cập nhật tình trạng sách sau trả' WHERE ma_quyen = N'TT112';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT112', N'Cập nhật tình trạng sách sau trả', N'Cập nhật tình trạng sách sau trả');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT112');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT112');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT113')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận thông tin sách bị mất, hư hỏng', mota = N'Tiếp nhận thông tin sách bị mất, hư hỏng' WHERE ma_quyen = N'TT113';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT113', N'Tiếp nhận thông tin sách bị mất, hư hỏng', N'Tiếp nhận thông tin sách bị mất, hư hỏng');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT113');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT114')
    UPDATE quantri.QUYEN SET ten_quyen = N'Kiểm soát dữ liệu nhập (so_bienban)', mota = N'Kiểm soát dữ liệu nhập (so_bienban)' WHERE ma_quyen = N'TT114';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT114', N'Kiểm soát dữ liệu nhập (so_bienban)', N'Kiểm soát dữ liệu nhập (so_bienban)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT114');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT115')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tìm kiếm thông tin Biên bản vi phạm', mota = N'Tìm kiếm thông tin Biên bản vi phạm' WHERE ma_quyen = N'TT115';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT115', N'Tìm kiếm thông tin Biên bản vi phạm', N'Tìm kiếm thông tin Biên bản vi phạm');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT115');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'TT115');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT115');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT115');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT116')
    UPDATE quantri.QUYEN SET ten_quyen = N'Cập nhật tình trạng sách bị mất, hư hỏng', mota = N'Cập nhật tình trạng sách bị mất, hư hỏng' WHERE ma_quyen = N'TT116';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT116', N'Cập nhật tình trạng sách bị mất, hư hỏng', N'Cập nhật tình trạng sách bị mất, hư hỏng');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT116');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT116');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT117')
    UPDATE quantri.QUYEN SET ten_quyen = N'Mở form tổng hợp số sách theo tình trạng', mota = N'Mở form tổng hợp số sách theo tình trạng' WHERE ma_quyen = N'TT117';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT117', N'Mở form tổng hợp số sách theo tình trạng', N'Mở form tổng hợp số sách theo tình trạng');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT117');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT118')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tiếp nhận yêu cầu đối chiếu tình trạng sách', mota = N'Tiếp nhận yêu cầu đối chiếu tình trạng sách' WHERE ma_quyen = N'TT118';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT118', N'Tiếp nhận yêu cầu đối chiếu tình trạng sách', N'Tiếp nhận yêu cầu đối chiếu tình trạng sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT118');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT119')
    UPDATE quantri.QUYEN SET ten_quyen = N'Truy xuất danh sách bản sách theo tình trạng', mota = N'Truy xuất danh sách bản sách theo tình trạng' WHERE ma_quyen = N'TT119';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT119', N'Truy xuất danh sách bản sách theo tình trạng', N'Truy xuất danh sách bản sách theo tình trạng');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'TT119');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT119');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'TT119');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT120')
    UPDATE quantri.QUYEN SET ten_quyen = N'Tính số sách theo tình trạng = COUNT(bản sách theo tinhtrang)', mota = N'Tính số sách theo tình trạng = COUNT(bản sách theo tinhtrang)' WHERE ma_quyen = N'TT120';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT120', N'Tính số sách theo tình trạng = COUNT(bản sách theo tinhtrang)', N'Tính số sách theo tình trạng = COUNT(bản sách theo tinhtrang)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT120');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT121')
    UPDATE quantri.QUYEN SET ten_quyen = N'Hiển thị số liệu tổng hợp số sách theo tình trạng', mota = N'Hiển thị số liệu tổng hợp số sách theo tình trạng' WHERE ma_quyen = N'TT121';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT121', N'Hiển thị số liệu tổng hợp số sách theo tình trạng', N'Hiển thị số liệu tổng hợp số sách theo tình trạng');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT121');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'TT122')
    UPDATE quantri.QUYEN SET ten_quyen = N'Chuyển dữ liệu tình trạng sách cho Bộ phận mượn trả sách', mota = N'Chuyển dữ liệu tình trạng sách cho Bộ phận mượn trả sách' WHERE ma_quyen = N'TT122';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'TT122', N'Chuyển dữ liệu tình trạng sách cho Bộ phận mượn trả sách', N'Chuyển dữ liệu tình trạng sách cho Bộ phận mượn trả sách');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'TT122');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'SACH_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Sách (B01. SACH)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'SACH_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'SACH_C', N'Thêm Sách (B01. SACH)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'SACH_C');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'SACH_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'SACH_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Sách (B01. SACH)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'SACH_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'SACH_E', N'Sửa Sách (B01. SACH)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'SACH_E');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'SACH_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'SACH_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Sách (B01. SACH)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'SACH_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'SACH_R', N'Xem Sách (B01. SACH)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'SACH_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'SACH_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'SACH_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'SACH_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Sách (B01. SACH)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'SACH_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'SACH_D', N'Xóa Sách (B01. SACH)', N'Quyền D trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'SACH_D');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHOANTHU_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Khoản thu (B02. KHOANTHU)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHOANTHU_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHOANTHU_C', N'Thêm Khoản thu (B02. KHOANTHU)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHOANTHU_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHOANTHU_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Khoản thu (B02. KHOANTHU)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHOANTHU_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHOANTHU_E', N'Sửa Khoản thu (B02. KHOANTHU)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHOANTHU_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHOANTHU_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Khoản thu (B02. KHOANTHU)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHOANTHU_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHOANTHU_R', N'Xem Khoản thu (B02. KHOANTHU)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'KHOANTHU_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHOANTHU_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHOANTHU_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Khoản thu (B02. KHOANTHU)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHOANTHU_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHOANTHU_D', N'Xóa Khoản thu (B02. KHOANTHU)', N'Quyền D trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHOANTHU_D');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BANDOC_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Bạn đọc (B03. BANDOC)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'BANDOC_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BANDOC_C', N'Thêm Bạn đọc (B03. BANDOC)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'BANDOC_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BANDOC_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Bạn đọc (B03. BANDOC)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'BANDOC_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BANDOC_E', N'Sửa Bạn đọc (B03. BANDOC)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'BANDOC_E');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'BANDOC_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BANDOC_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Bạn đọc (B03. BANDOC)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'BANDOC_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BANDOC_R', N'Xem Bạn đọc (B03. BANDOC)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'BANDOC_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'BANDOC_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'BANDOC_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BANDOC_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Bạn đọc (B03. BANDOC)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'BANDOC_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BANDOC_D', N'Xóa Bạn đọc (B03. BANDOC)', N'Quyền D trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'BANDOC_D');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'BANDOC_D');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHUVUC_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Khu vực lưu trữ (B04. KHUVUC)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHUVUC_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHUVUC_C', N'Thêm Khu vực lưu trữ (B04. KHUVUC)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'KHUVUC_C');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHUVUC_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHUVUC_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Khu vực lưu trữ (B04. KHUVUC)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHUVUC_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHUVUC_E', N'Sửa Khu vực lưu trữ (B04. KHUVUC)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'KHUVUC_E');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHUVUC_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHUVUC_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Khu vực lưu trữ (B04. KHUVUC)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHUVUC_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHUVUC_R', N'Xem Khu vực lưu trữ (B04. KHUVUC)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'KHUVUC_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'KHUVUC_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHUVUC_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'KHUVUC_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Khu vực lưu trữ (B04. KHUVUC)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'KHUVUC_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'KHUVUC_D', N'Xóa Khu vực lưu trữ (B04. KHUVUC)', N'Quyền D trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'KHUVUC_D');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PMUON_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'PMUON_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PMUON_C', N'Thêm Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PMUON_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PMUON_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'PMUON_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PMUON_E', N'Sửa Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PMUON_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PMUON_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'PMUON_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PMUON_R', N'Xem Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PMUON_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'PMUON_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'PMUON_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'PMUON_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PMUON_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'PMUON_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PMUON_D', N'Xóa Phiếu mượn sách (B05. PHIEUMUON + CT_PHIEUMUON)', N'Quyền D trên bảng – SPEC 3.2');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PGHAN_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'PGHAN_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PGHAN_C', N'Thêm Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PGHAN_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PGHAN_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'PGHAN_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PGHAN_E', N'Sửa Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PGHAN_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PGHAN_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'PGHAN_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PGHAN_R', N'Xem Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PGHAN_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'PGHAN_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'PGHAN_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'PGHAN_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PGHAN_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'PGHAN_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PGHAN_D', N'Xóa Phiếu gia hạn sách (B06. PHIEUGIAHAN + CT_PHIEUGIAHAN)', N'Quyền D trên bảng – SPEC 3.2');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PTRA_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'PTRA_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PTRA_C', N'Thêm Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PTRA_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PTRA_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'PTRA_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PTRA_E', N'Sửa Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PTRA_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PTRA_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'PTRA_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PTRA_R', N'Xem Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'PTRA_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'PTRA_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'PTRA_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'PTRA_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'PTRA_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'PTRA_D', N'Xóa Phiếu trả sách (B07. PHIEUTRA + CT_PHIEUTRA)', N'Quyền D trên bảng – SPEC 3.2');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BIENBAN_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', mota = N'Quyền C trên bảng – SPEC 3.2' WHERE ma_quyen = N'BIENBAN_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BIENBAN_C', N'Thêm Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', N'Quyền C trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'BIENBAN_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BIENBAN_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', mota = N'Quyền E trên bảng – SPEC 3.2' WHERE ma_quyen = N'BIENBAN_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BIENBAN_E', N'Sửa Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', N'Quyền E trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'BIENBAN_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BIENBAN_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', mota = N'Quyền R trên bảng – SPEC 3.2' WHERE ma_quyen = N'BIENBAN_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BIENBAN_R', N'Xem Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', N'Quyền R trên bảng – SPEC 3.2');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'BIENBAN_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N2', N'BIENBAN_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'BIENBAN_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'BIENBAN_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'BIENBAN_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', mota = N'Quyền D trên bảng – SPEC 3.2' WHERE ma_quyen = N'BIENBAN_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'BIENBAN_D', N'Xóa Biên bản vi phạm (B08. BIENBAN + CT_BIENBAN)', N'Quyền D trên bảng – SPEC 3.2');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040001')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.01 Quản lý tài khoản người dùng', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040001';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040001', N'04.00.01 Quản lý tài khoản người dùng', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040001');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040002')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.02 Cấp quyền cho tài khoản', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040002';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040002', N'04.00.02 Cấp quyền cho tài khoản', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040002');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040003')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.03 Theo dõi lịch sử đăng nhập', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040003';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040003', N'04.00.03 Theo dõi lịch sử đăng nhập', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040003');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040004')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.04 Quản lý danh mục sách', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040004';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040004', N'04.00.04 Quản lý danh mục sách', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040004');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040005')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.05 Quản lý danh mục khu vực lưu trữ', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040005';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040005', N'04.00.05 Quản lý danh mục khu vực lưu trữ', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040005');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040006')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.06 Quản lý danh mục xử lý vi phạm', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040006';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040006', N'04.00.06 Quản lý danh mục xử lý vi phạm', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040006');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040007')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.07 Quản lý danh mục mượn trả sách', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040007';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040007', N'04.00.07 Quản lý danh mục mượn trả sách', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040007');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040008')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.08 Theo dõi nhật ký hoạt động', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040008';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040008', N'04.00.08 Theo dõi nhật ký hoạt động', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040008');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'QT040009')
    UPDATE quantri.QUYEN SET ten_quyen = N'04.00.09 Sao lưu và phục hồi dữ liệu', mota = N'Chức năng quản trị – SPEC 3.1 (N4)' WHERE ma_quyen = N'QT040009';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'QT040009', N'04.00.09 Sao lưu và phục hồi dữ liệu', N'Chức năng quản trị – SPEC 3.1 (N4)');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'QT040009');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'DATCHO_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem đặt chỗ (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'DATCHO_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'DATCHO_R', N'Xem đặt chỗ (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'DATCHO_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'DATCHO_GH')
    UPDATE quantri.QUYEN SET ten_quyen = N'Giao sách đặt chỗ, hủy đặt chỗ tại quầy (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'DATCHO_GH';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'DATCHO_GH', N'Giao sách đặt chỗ, hủy đặt chỗ tại quầy (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N1', N'DATCHO_GH');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'DAUSACH_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm đầu sách (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'DAUSACH_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'DAUSACH_C', N'Thêm đầu sách (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'DAUSACH_C');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'DAUSACH_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'DAUSACH_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa đầu sách (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'DAUSACH_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'DAUSACH_E', N'Sửa đầu sách (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'DAUSACH_E');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'DAUSACH_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'DAUSACH_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem đầu sách (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'DAUSACH_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'DAUSACH_R', N'Xem đầu sách (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'DAUSACH_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'DAUSACH_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'DAUSACH_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa đầu sách (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'DAUSACH_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'DAUSACH_D', N'Xóa đầu sách (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'DAUSACH_D');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'THELOAI_C')
    UPDATE quantri.QUYEN SET ten_quyen = N'Thêm thể loại (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'THELOAI_C';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'THELOAI_C', N'Thêm thể loại (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'THELOAI_C');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'THELOAI_C');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'THELOAI_E')
    UPDATE quantri.QUYEN SET ten_quyen = N'Sửa thể loại (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'THELOAI_E';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'THELOAI_E', N'Sửa thể loại (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'THELOAI_E');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'THELOAI_E');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'THELOAI_R')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xem thể loại (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'THELOAI_R';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'THELOAI_R', N'Xem thể loại (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N3', N'THELOAI_R');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'THELOAI_R');
END
IF EXISTS (SELECT 1 FROM quantri.QUYEN WHERE ma_quyen = N'THELOAI_D')
    UPDATE quantri.QUYEN SET ten_quyen = N'Xóa thể loại (mở rộng)', mota = N'Quyền mở rộng – file 02 mục 3' WHERE ma_quyen = N'THELOAI_D';
ELSE
BEGIN
    INSERT quantri.QUYEN (ma_quyen, ten_quyen, mota) VALUES (N'THELOAI_D', N'Xóa thể loại (mở rộng)', N'Quyền mở rộng – file 02 mục 3');
    INSERT quantri.PHANQUYEN (ma_nhom, ma_quyen) VALUES (N'N4', N'THELOAI_D');
END
COMMIT;
GO
