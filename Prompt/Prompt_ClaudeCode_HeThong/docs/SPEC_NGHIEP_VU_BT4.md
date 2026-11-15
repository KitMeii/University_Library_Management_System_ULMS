# SPEC NGHIỆP VỤ (NGUỒN CHUẨN) – QUẢN LÝ MƯỢN TRẢ SÁCH THƯ VIỆN ĐẠI HỌC

> Tài liệu nguồn: Bài tập 4 – Thiết kế hệ thống (mục 2.1–2.3 Kiểm soát, mục III CSDL, mục IV Giao diện). Mục tiêu: một **hệ thống hoàn chỉnh, dùng được thật** cho thư viện (không phải bản demo). Mọi tên bảng, tên trường, tên control, mã tiến trình trong tài liệu này là **bắt buộc giữ nguyên** để hệ thống khớp với hồ sơ thiết kế.

## 0. Nguyên tắc bắt buộc khi code
- Giữ nguyên **tên bảng, tên trường** như mục 2 (không thêm/bớt trường, không đổi tên). Phần mở rộng (đặt chỗ, cổng bạn đọc) chỉ thêm bảng mới theo `02_CSDL_VA_MO_RONG.md`, không sửa 18 bảng này.
- Giữ nguyên **tên control** như mục 6 làm `id` của phần tử HTML trong Razor View (ví dụ `txtMaSV`, `btnTimBanDoc`, `dgvChiTiet`). Nhãn hiển thị bằng tiếng Việt có dấu.
- **Tại mỗi thời điểm chỉ bật các nút lệnh của trạng thái hiện tại** (trừ nút Đóng/Thoát) – làm đúng phần “Trạng thái nút” ghi trong cột Xử lý của từng giao diện.
- **Một nút lệnh – một công việc**, một API – một tác vụ; mỗi thao tác thêm/sửa/xóa ghi 1 dòng `NHATKY_HOATDONG`.
- Phân quyền kiểm tra **cả ở menu (ẩn chức năng) và ở API (trả 403)** theo mục 3.
- Ngày hiển thị `dd/MM/yyyy`, lưu `date`/`datetime2` trong SQL Server; tiền lưu `decimal(18,0)`, hiển thị `1.234.000 đ`.
- Mọi thông báo, nhãn, tiêu đề form, menu bằng **tiếng Việt có dấu**. Không dùng `alert()` mặc định – dùng hộp thoại tự làm (modal) để chụp ảnh đẹp.
- Không tự ý thêm chức năng nghiệp vụ ngoài đặc tả này và phần mở rộng ở `02_CSDL_VA_MO_RONG.md`. Chỗ nào chưa rõ: hỏi lại trong kế hoạch; quyết định đã duyệt ghi vào `docs/GIA_DINH.md`.

## 2. Cơ sở dữ liệu (đúng đặc tả mục 3.7 – 18 bảng)
Ánh xạ kiểu sang SQL Server: `C(n)` → `nvarchar(n)`, `N` → `int` (tiền: `decimal(18,0)`), `D` → `date` (thời điểm: `datetime2(0)`). Khóa chính in **đậm**, khóa ngoại ghi ở dòng FK.

### B01. SACH
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_bansach** | Mã bản sách | C(10) | Chữ cái + chữ số |
| ma_khuvuc | Mã khu vực | C(10) | Chữ cái + chữ số |
| ten_sach | Tên sách | C(200) | Chữ cái |
| tacgia | Tác giả | C(100) | Chữ cái |
| tinhtrang | Tình trạng | C(30) | Chữ cái |
FK: ma_khuvuc → KHUVUC

### B02. KHOANTHU
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_khoanthu** | Mã khoản thu | C(10) | Chữ cái + chữ số |
| noidung_tt | Nội dung thanh toán | C(100) | Chữ cái |
| dongia | Đơn giá | N(10) | Chữ số |

### B03. BANDOC
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_bandoc** | Mã bạn đọc | C(10) | Chữ cái + chữ số |
| hoten | Họ và tên | C(50) | Chữ cái |
| doituong | Đối tượng | C(20) | Chữ cái |
| ma_svhvcb | Mã SV/HV/CB | C(15) | Chữ cái + chữ số |
| lop_donvi | Lớp/Đơn vị | C(50) | Chữ cái |
| ngaysinh | Ngày sinh | D | dd/mm/yyyy |
| so_thebd | Số thẻ bạn đọc | C(15) | Chữ cái + chữ số |
| dienthoai_email | Điện thoại/Email | C(50) | Chữ cái + chữ số |
| hsd_tungay | Hạn sử dụng từ ngày | D | dd/mm/yyyy |
| hsd_denngay | Hạn sử dụng đến ngày | D | dd/mm/yyyy |
| anh | Ảnh | C(100) | Chữ cái + chữ số |

### B04. KHUVUC
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_khuvuc** | Mã khu vực | C(10) | Chữ cái + chữ số |
| vitri | Vị trí | C(50) | Chữ cái |
| loai_khuvuc | Loại khu vực | C(20) | Chữ cái |

### B05. PHIEUMUON
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **so_pmuon** | Số phiếu mượn | C(10) | Chữ cái + chữ số |
| ma_bandoc | Mã bạn đọc | C(10) | Chữ cái + chữ số |
| ngaylap_pmuon | Ngày lập phiếu mượn | D | dd/mm/yyyy |
| hoten | Họ và tên Bạn đọc (lấy từ BANDOC) | C(50) | Chữ cái |
| ma_nd | Mã người dùng lập giao dịch | C(10) | Chữ cái + chữ số |
| ten_nd | Tên người dùng lập giao dịch | C(50) | Chữ cái |
FK: ma_bandoc → BANDOC

### B05.1. CT_PHIEUMUON
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_bansach** | Mã bản sách | C(10) | Chữ cái + chữ số |
| **so_pmuon** | Số phiếu mượn | C(10) | Chữ cái + chữ số |
| ngaymuon | Ngày mượn | D | dd/mm/yyyy |
| hantra | Hạn trả | D | dd/mm/yyyy |
| tinhtrang_muon | Tình trạng khi mượn | C(50) | Chữ cái |
| ghichu_muon | Ghi chú mượn | C(200) | Chữ cái |
| ten_sach | Tên sách (lấy từ SACH) | C(200) | Chữ cái |
FK: ma_bansach → SACH; so_pmuon → PHIEUMUON

### B06. PHIEUGIAHAN
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **so_pgiahan** | Số phiếu gia hạn | C(10) | Chữ cái + chữ số |
| so_pmuon | Số phiếu mượn | C(10) | Chữ cái + chữ số |
| ma_bandoc | Mã bạn đọc | C(10) | Chữ cái + chữ số |
| ngaylap_pgiahan | Ngày lập phiếu gia hạn | D | dd/mm/yyyy |
| ngay_denghi | Ngày đề nghị gia hạn | D | dd/mm/yyyy |
| ma_nd | Mã người dùng lập giao dịch | C(10) | Chữ cái + chữ số |
| ten_nd | Tên người dùng lập giao dịch | C(50) | Chữ cái |
FK: so_pmuon → PHIEUMUON; ma_bandoc → BANDOC

### B06.1. CT_PHIEUGIAHAN
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_bansach** | Mã bản sách | C(10) | Chữ cái + chữ số |
| **so_pgiahan** | Số phiếu gia hạn | C(10) | Chữ cái + chữ số |
| hantra_cu | Hạn trả cũ | D | dd/mm/yyyy |
| ketqua_giahan | Kết quả gia hạn | C(20) | Chữ cái |
| hantra_moi | Hạn trả mới | D | dd/mm/yyyy |
| lydo_khonggh | Lý do không gia hạn | C(200) | Chữ cái |
| ten_sach | Tên sách (lấy từ SACH) | C(200) | Chữ cái |
| so_lan_giahan | Số lần đã gia hạn của bản sách | N(2) | Chữ số |
FK: ma_bansach → SACH; so_pgiahan → PHIEUGIAHAN

### B07. PHIEUTRA
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **so_ptra** | Số phiếu trả | C(10) | Chữ cái + chữ số |
| so_pmuon | Số phiếu mượn | C(10) | Chữ cái + chữ số |
| ma_bandoc | Mã bạn đọc | C(10) | Chữ cái + chữ số |
| ngaylap_ptra | Ngày lập phiếu trả | D | dd/mm/yyyy |
| ngaytra | Ngày trả | D | dd/mm/yyyy |
| ma_nd | Mã người dùng lập giao dịch | C(10) | Chữ cái + chữ số |
| ten_nd | Tên người dùng lập giao dịch | C(50) | Chữ cái |
FK: so_pmuon → PHIEUMUON; ma_bandoc → BANDOC

### B07.1. CT_PHIEUTRA
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_bansach** | Mã bản sách | C(10) | Chữ cái + chữ số |
| **so_ptra** | Số phiếu trả | C(10) | Chữ cái + chữ số |
| tinhtrang_tra | Tình trạng khi trả | C(50) | Chữ cái |
| ghichu_tra | Ghi chú trả | C(200) | Chữ cái |
| so_ngay_quahan | Số ngày trả quá hạn | N(4) | Chữ số |
FK: ma_bansach → SACH; so_ptra → PHIEUTRA

### B08. BIENBAN
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **so_bienban** | Số biên bản | C(10) | Chữ cái + chữ số |
| so_pmuon | Số phiếu mượn | C(10) | Chữ cái + chữ số |
| ma_bandoc | Mã bạn đọc | C(10) | Chữ cái + chữ số |
| thoigian_lap | Thời gian lập | D | dd/mm/yyyy hh:mm |
| ht_thanhtoan | Hình thức thanh toán | C(20) | Chữ cái |
| ma_nd | Mã người dùng lập giao dịch | C(10) | Chữ cái + chữ số |
| ten_nd | Tên người dùng lập giao dịch | C(50) | Chữ cái |
| tong_tien | Tổng tiền của Biên bản | N(12) | Chữ số |
FK: so_pmuon → PHIEUMUON; ma_bandoc → BANDOC

### B08.1. CT_BIENBAN
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_khoanthu** | Mã khoản thu | C(10) | Chữ cái + chữ số |
| **so_bienban** | Số biên bản | C(10) | Chữ cái + chữ số |
| **ma_bansach** | Mã bản sách | C(10) | Chữ cái + chữ số |
| soluong | Số lượng | N(5) | Chữ số |
| thanh_tien | Thành tiền của dòng vi phạm | N(12) | Chữ số |
FK: ma_khoanthu → KHOANTHU; so_bienban → BIENBAN

### B09. NHOMNGUOIDUNG
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_nhom** | Mã nhóm người dùng | C(10) | Chữ cái + chữ số |
| ten_nhom | Tên nhóm người dùng | C(50) | Chữ cái |
| mota | Mô tả | C(200) | Chữ cái |

### B10. NGUOIDUNG
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_nd** | Mã người dùng | C(10) | Chữ cái + chữ số |
| ten_nd | Tên người dùng | C(50) | Chữ cái |
| tendangnhap | Tên đăng nhập | C(30) | Chữ cái + chữ số |
| matkhau | Mật khẩu (đã băm) | C(64) | Trừ ký tự đặc biệt |
| sdt | Số điện thoại | C(15) | Chữ số |
| chucvu | Chức vụ | C(50) | Chữ cái |
| tinhtrang | Tình trạng hoạt động | C(20) | Chữ cái |
| ma_nhom | Mã nhóm người dùng | C(10) | Chữ cái + chữ số |
FK: ma_nhom → NHOMNGUOIDUNG

### B11. QUYEN
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_quyen** | Mã quyền | C(10) | Chữ cái + chữ số |
| ten_quyen | Tên quyền | C(100) | Chữ cái |
| mota | Mô tả | C(200) | Chữ cái |

### B12. PHANQUYEN
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_nhom** | Mã nhóm người dùng | C(10) | Chữ cái + chữ số |
| **ma_quyen** | Mã quyền | C(10) | Chữ cái + chữ số |
FK: ma_nhom → NHOMNGUOIDUNG; ma_quyen → QUYEN

### B13. NHATKY_DANGNHAP
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_dangnhap** | Mã lượt đăng nhập | C(15) | Chữ cái + chữ số |
| ma_nd | Mã người dùng | C(10) | Chữ cái + chữ số |
| tg_dangnhap | Thời gian đăng nhập | D | dd/mm/yyyy hh:mm |
| tg_dangxuat | Thời gian đăng xuất | D | dd/mm/yyyy hh:mm |
FK: ma_nd → NGUOIDUNG

### B14. NHATKY_HOATDONG
| Trường | Mô tả | Kiểu | Khuôn dạng / ràng buộc |
|---|---|---|---|
| **ma_hoatdong** | Mã hoạt động | C(15) | Chữ cái + chữ số |
| ma_nd | Mã người dùng | C(10) | Chữ cái + chữ số |
| tg_thuchien | Thời gian thực hiện | D | dd/mm/yyyy hh:mm |
| noidung | Nội dung thao tác | C(500) | Chữ cái |
FK: ma_nd → NGUOIDUNG

Miền giá trị dùng đúng như cột “Điều kiện toàn vẹn” ở mục 6, cụ thể: `BANDOC.doituong` ∈ {Sinh viên, Học viên, Cán bộ}; `SACH.tinhtrang` ∈ {Sẵn sàng, Đang được mượn, Mất, Hư hỏng}; `CT_PHIEUGIAHAN.ketqua_giahan` ∈ {Đồng ý, Không đồng ý}; `CT_PHIEUTRA.tinhtrang_tra` ∈ {Bình thường, Hư hỏng, Mất}; `BIENBAN.ht_thanhtoan` ∈ {Tiền mặt, Chuyển khoản, Chưa thanh toán}; `NGUOIDUNG.tinhtrang` ∈ {Hoạt động, Khóa}; `KHUVUC.loai_khuvuc` ∈ {Kệ mở, Kho}.

## 3. Kiểm soát: nhóm người dùng và phân quyền
### 3.1 Nhóm người dùng (mục 2.1)
- **N1 – Mượn trả sách**: 01.01.01 Lưu thông tin Thẻ Bạn đọc; 01.01.03 Lập Phiếu mượn sách; 01.02.01 Lập Phiếu gia hạn sách; 01.02.02 Lập Phiếu trả sách; 01.04.02 Tổng hợp tình hình mượn trả sách; 01.04.03 Lập Báo cáo tình hình mượn trả sách
- **N2 – Xử lý vi phạm**: 02.03.01 Lập Biên bản vi phạm mượn trả sách; 02.05.01 Tổng hợp tình hình vi phạm mượn trả sách; 02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách
- **N3 – Kho sách**: 03.01.02 Kiểm tra khả năng đáp ứng sách; 03.01.04 Cập nhật tình trạng sách đang được mượn; 03.02.03 Cập nhật sách sau trả; 03.03.02 Cập nhật tình trạng sách bị mất, hư hỏng; 03.04.01 Tổng hợp số sách theo tình trạng
- **N4 – Quản trị**: 04.00.01 Quản lý tài khoản người dùng; 04.00.02 Cấp quyền cho tài khoản; 04.00.03 Theo dõi lịch sử đăng nhập; 04.00.04 Quản lý danh mục sách; 04.00.05 Quản lý danh mục khu vực lưu trữ; 04.00.06 Quản lý danh mục xử lý vi phạm; 04.00.07 Quản lý danh mục mượn trả sách; 04.00.08 Theo dõi nhật ký hoạt động; 04.00.09 Sao lưu và phục hồi dữ liệu

### 3.2 Quyền trên bảng dữ liệu (mục 2.2) – C: thêm, E: sửa, R: xem, D: xóa, –: không có quyền
| Bảng | N1 – Mượn trả sách | N2 – Xử lý vi phạm | N3 – Kho sách | N4 – Quản trị |
|---|---|---|---|---|
| B01. SACH | R | – | C, E, R | C, E, R, D |
| B02. KHOANTHU | – | R | – | C, E, R, D |
| B03. BANDOC | C, E, R, D | R | – | E, R, D |
| B04. KHUVUC | R | – | C, E, R | C, E, R, D |
| B05. PHIEUMUON + CT_PHIEUMUON | C, E, R | R | R | R |
| B06. PHIEUGIAHAN + CT_PHIEUGIAHAN | C, E, R | R | R | R |
| B07. PHIEUTRA + CT_PHIEUTRA | C, E, R | – | R | R |
| B08. BIENBAN + CT_BIENBAN | R | C, E, R | R | R |

Các bảng B09–B14 chỉ N4 thao tác (qua các giao diện 6.3); mọi nhóm được ghi NHATKY_* gián tiếp qua hệ thống.

### 3.3 Menu theo người dùng (mục 4.1) – mỗi nhóm chỉ thấy menu của mình
| Nhóm | Menu | Chức năng hiển thị | Màn hình mở ngay sau đăng nhập |
|---|---|---|---|
| N1 – Mượn trả sách | Menu Mượn trả sách | 01.01.01 Lưu thông tin Thẻ Bạn đọc; 01.01.03 Lập Phiếu mượn sách; 01.02.01 Lập Phiếu gia hạn sách; 01.02.02 Lập Phiếu trả sách; 01.04.02 Tổng hợp tình hình mượn trả sách; 01.04.03 Lập Báo cáo tình hình mượn trả sách; Danh mục sách, Danh mục khu vực lưu trữ (chỉ xem) | Lập Phiếu mượn sách |
| N2 – Xử lý vi phạm | Menu Xử lý vi phạm | 02.03.01 Lập Biên bản vi phạm mượn trả sách; 02.05.01 Tổng hợp tình hình vi phạm mượn trả sách; 02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách; Danh mục khoản thu, Danh mục Bạn đọc (chỉ xem) | Lập Biên bản vi phạm mượn trả sách |
| N3 – Kho sách | Menu Kho sách | 03.01.02 Kiểm tra khả năng đáp ứng sách; 03.01.04 Cập nhật tình trạng sách đang được mượn; 03.02.03 Cập nhật sách sau trả; 03.03.02 Cập nhật tình trạng sách bị mất, hư hỏng; 03.04.01 Tổng hợp số sách theo tình trạng; Danh mục sách (thực hiện các tiến trình 03.xx); Danh mục khu vực lưu trữ | Danh mục sách |
| N4 – Quản trị | Menu Quản trị | 04.00.01 đến 04.00.09 (tài khoản, phân quyền, nhật ký, danh mục sách, khu vực lưu trữ, khoản thu, Bạn đọc, sao lưu – phục hồi) | Quản lý tài khoản người dùng |

Menu theo chức năng (để đặt tên mục menu): 
- **Hệ thống**: Đăng nhập; Đăng xuất; Đổi mật khẩu
- **Quản lý danh mục**: Danh mục sách; Danh mục khoản thu; Danh mục Bạn đọc; Danh mục khu vực lưu trữ
- **Quản lý nghiệp vụ**: 01.01.01 Lưu thông tin Thẻ Bạn đọc; 01.01.03 Lập Phiếu mượn sách; 01.02.01 Lập Phiếu gia hạn sách; 01.02.02 Lập Phiếu trả sách; 02.03.01 Lập Biên bản vi phạm mượn trả sách; 03.01.02 Kiểm tra khả năng đáp ứng sách; 03.01.04 Cập nhật tình trạng sách đang được mượn; 03.02.03 Cập nhật sách sau trả; 03.03.02 Cập nhật tình trạng sách bị mất, hư hỏng
- **Quản lý thống kê**: 01.04.02 Tổng hợp tình hình mượn trả sách; 01.04.03 Lập Báo cáo tình hình mượn trả sách; 02.05.01 Tổng hợp tình hình vi phạm mượn trả sách; 02.05.02 Lập Báo cáo tình hình vi phạm mượn trả sách; 03.04.01 Tổng hợp số sách theo tình trạng
- **Quản trị**: 04.00.01 Quản lý tài khoản người dùng; 04.00.02 Cấp quyền cho tài khoản; 04.00.03 Theo dõi lịch sử đăng nhập; 04.00.04 Quản lý danh mục sách; 04.00.05 Quản lý danh mục khu vực lưu trữ; 04.00.06 Quản lý danh mục xử lý vi phạm; 04.00.07 Quản lý danh mục mượn trả sách; 04.00.08 Theo dõi nhật ký hoạt động; 04.00.09 Sao lưu và phục hồi dữ liệu

Quy ước tên mục menu: chức năng nghiệp vụ/giao dịch **có mã** (vd “01.01.03 Lập Phiếu mượn sách”); danh mục **không mã** (vd “Danh mục sách”).

### 3.4 Quyền thực hiện tiến trình hệ thống (mục 2.3, A = được phép, nA = không)
Nạp bảng này vào `QUYEN` (mã quyền dạng `TT001`…) và `PHANQUYEN`; middleware kiểm tra quyền theo mã tiến trình của từng API. Quyền trên bảng ở 3.2 nạp thành mã dạng `SACH_C`, `SACH_E`…

| STT | Tiến trình hệ thống | N1 | N2 | N3 | N4 |
|---|---|---|---|---|---|
| 1 | Mở form Thẻ thông tin Bạn đọc | A | nA | nA | nA |
| 2 | Kiểm soát dữ liệu nhập (ma_svhvcb) | A | nA | nA | nA |
| 3 | Tìm kiếm thông tin Bạn đọc | A | A | nA | A |
| 4 | Đối chiếu thông tin Thẻ Bạn đọc | A | nA | nA | nA |
| 5 | Kiểm soát dữ liệu nhập (ma_bandoc, hoten, doituong, ma_svhvcb, lop_donvi, ngaysinh, so_thebd, dienthoai_email, hsd_tungay, hsd_denngay) | A | nA | nA | nA |
| 6 | Lưu thông tin Bạn đọc mới | A | nA | nA | nA |
| 7 | Kiểm tra hạn sử dụng Thẻ | A | nA | nA | nA |
| 8 | Truy xuất danh sách sách Bạn đọc đang mượn | A | A | A | A |
| 9 | Kiểm tra sách mượn quá hạn | A | nA | nA | nA |
| 10 | Truy xuất danh sách vi phạm chưa thanh toán | A | A | A | A |
| 11 | Thông báo kết quả kiểm tra điều kiện mượn | A | nA | nA | nA |
| 12 | Mở form Phiếu mượn sách | A | nA | nA | nA |
| 13 | Tìm kiếm Phiếu mượn sách lập trong ngày | A | A | A | A |
| 14 | Tìm kiếm thông tin bản sách | A | nA | A | A |
| 15 | Tìm kiếm vị trí bản sách | A | nA | A | A |
| 16 | Chuyển Phiếu yêu cầu lấy sách trong kho cho Bộ phận kho sách | A | nA | nA | nA |
| 17 | Chuyển yêu cầu đổi bản sách cho Bộ phận kho sách | A | nA | nA | nA |
| 18 | Tiếp nhận kết quả đáp ứng sách | A | nA | nA | nA |
| 19 | Thông báo không đáp ứng sách | A | nA | nA | nA |
| 20 | Kiểm soát dữ liệu nhập (ma_bandoc, ma_bansach, ngaymuon, tinhtrang_muon, ghichu_muon) | A | nA | nA | nA |
| 21 | Tính hạn trả = ngaymuon + số ngày được mượn | A | nA | nA | nA |
| 22 | Hiển thị chi tiết thông tin Phiếu mượn sách | A | nA | nA | nA |
| 23 | Bổ sung bản sách vào Phiếu mượn sách đã lập trong ngày | A | nA | nA | nA |
| 24 | Tự động sinh so_pmuon, ngaylap_pmuon khi lưu | A | nA | nA | nA |
| 25 | Lưu thông tin Phiếu mượn sách | A | nA | nA | nA |
| 26 | Cập nhật thông tin Phiếu mượn sách | A | nA | nA | nA |
| 27 | Chuyển thông tin sách đã cho mượn cho Bộ phận kho sách | A | nA | nA | nA |
| 28 | In Phiếu mượn sách | A | nA | nA | nA |
| 29 | Thông báo điều kiện sử dụng sách | A | nA | nA | nA |
| 30 | Mở form Phiếu gia hạn sách | A | nA | nA | nA |
| 31 | Kiểm soát dữ liệu nhập (so_pmuon) | A | A | A | nA |
| 32 | Tìm kiếm thông tin Phiếu mượn sách | A | A | A | A |
| 33 | Kiểm soát dữ liệu nhập (ma_bansach, ngay_denghi) | A | nA | nA | nA |
| 34 | Tính số lần đã gia hạn = COUNT(lần gia hạn được đồng ý của bản sách) | A | nA | nA | nA |
| 35 | Kiểm tra điều kiện gia hạn | A | nA | nA | nA |
| 36 | Thông báo lý do từ chối gia hạn | A | nA | nA | nA |
| 37 | Tính hạn trả mới = hantra_cu + số ngày được gia hạn | A | nA | nA | nA |
| 38 | Hiển thị chi tiết thông tin Phiếu gia hạn sách | A | nA | nA | nA |
| 39 | Tự động sinh so_pgiahan, ngaylap_pgiahan khi lưu | A | nA | nA | nA |
| 40 | Lưu thông tin Phiếu gia hạn sách | A | nA | nA | nA |
| 41 | Cập nhật thông tin Phiếu gia hạn sách | A | nA | nA | nA |
| 42 | In Phiếu gia hạn sách | A | nA | nA | nA |
| 43 | Mở form Phiếu trả sách | A | nA | nA | nA |
| 44 | Tìm kiếm Phiếu trả sách đã lập | A | nA | A | A |
| 45 | Kiểm soát dữ liệu nhập (ma_bansach, ngaytra, tinhtrang_tra, ghichu_tra) | A | nA | nA | nA |
| 46 | Thông báo sai lệch sách trả | A | nA | nA | nA |
| 47 | Tính số ngày quá hạn = ngaytra − hantra | A | nA | nA | nA |
| 48 | Hiển thị chi tiết thông tin Phiếu trả sách | A | nA | nA | nA |
| 49 | Bổ sung bản sách vào Phiếu trả sách đã lập | A | nA | nA | nA |
| 50 | Tự động sinh so_ptra, ngaylap_ptra khi lưu | A | nA | nA | nA |
| 51 | Lưu thông tin Phiếu trả sách | A | nA | nA | nA |
| 52 | Cập nhật thông tin Phiếu trả sách | A | nA | nA | nA |
| 53 | Chuyển trường hợp vi phạm cho Bộ phận xử lý vi phạm | A | nA | nA | nA |
| 54 | Tiếp nhận kết quả xử lý vi phạm | A | nA | nA | nA |
| 55 | Chuyển sách trả cho Bộ phận kho sách | A | nA | nA | nA |
| 56 | In Phiếu trả sách | A | nA | nA | nA |
| 57 | Mở form tổng hợp tình hình mượn trả sách | A | nA | nA | nA |
| 58 | Kiểm soát dữ liệu nhập (kỳ báo cáo) | A | A | nA | nA |
| 59 | Truy xuất danh sách lượt mượn trong kỳ | A | A | A | A |
| 60 | Truy xuất danh sách lượt gia hạn trong kỳ | A | A | A | A |
| 61 | Truy xuất danh sách lượt trả trong kỳ | A | nA | A | A |
| 62 | Chuyển yêu cầu đối chiếu tình trạng sách cho Bộ phận kho sách | A | nA | nA | nA |
| 63 | Tiếp nhận dữ liệu tình trạng sách | A | nA | nA | nA |
| 64 | Tính tổng số lượt mượn = COUNT(lượt mượn trong kỳ) | A | nA | nA | nA |
| 65 | Tính tổng số lượt gia hạn = COUNT(lượt gia hạn được đồng ý trong kỳ) | A | nA | nA | nA |
| 66 | Tính tổng số lượt trả = COUNT(lượt trả trong kỳ) | A | nA | nA | nA |
| 67 | Tính số sách quá hạn = COUNT(sách chưa trả có hantra < ngày cuối kỳ báo cáo) | A | nA | nA | nA |
| 68 | Hiển thị số liệu tổng hợp tình hình mượn trả sách | A | nA | nA | nA |
| 69 | Mở form Báo cáo tình hình mượn trả sách | A | nA | nA | nA |
| 70 | Tiếp nhận số liệu tổng hợp tình hình mượn trả sách | A | nA | nA | nA |
| 71 | Hiển thị chi tiết Báo cáo tình hình mượn trả sách | A | nA | nA | nA |
| 72 | Tự động sinh ngày lập báo cáo | A | A | nA | nA |
| 73 | In Báo cáo tình hình mượn trả sách | A | nA | nA | nA |
| 74 | Mở form Biên bản vi phạm | nA | A | nA | nA |
| 75 | Tiếp nhận trường hợp vi phạm từ Bộ phận mượn trả sách | nA | A | nA | nA |
| 76 | Tìm kiếm thông tin khoản thu | nA | A | nA | A |
| 77 | Kiểm soát dữ liệu nhập (ma_khoanthu, ma_bansach, soluong) | nA | A | nA | nA |
| 78 | Tính thành tiền = soluong * dongia | nA | A | nA | nA |
| 79 | Tính tổng tiền cho Biên bản vi phạm | nA | A | nA | nA |
| 80 | Hiển thị chi tiết thông tin Biên bản vi phạm | nA | A | nA | nA |
| 81 | Thông báo khoản thu | nA | A | nA | nA |
| 82 | Kiểm soát dữ liệu nhập (ht_thanhtoan) | nA | A | nA | nA |
| 83 | Tự động sinh so_bienban, thoigian_lap khi lưu | nA | A | nA | nA |
| 84 | Lưu thông tin Biên bản vi phạm | nA | A | nA | nA |
| 85 | Ghi nhận thanh toán vào Biên bản vi phạm | nA | A | nA | nA |
| 86 | Cập nhật thông tin Biên bản vi phạm | nA | A | nA | nA |
| 87 | Chuyển kết quả xử lý vi phạm cho Bộ phận mượn trả sách | nA | A | nA | nA |
| 88 | Chuyển thông tin sách bị mất, hư hỏng cho Bộ phận kho sách | nA | A | nA | nA |
| 89 | In Biên bản vi phạm | nA | A | nA | nA |
| 90 | Mở form tổng hợp tình hình vi phạm | nA | A | nA | nA |
| 91 | Truy xuất danh sách Biên bản vi phạm trong kỳ | A | A | A | A |
| 92 | Tính số trường hợp vi phạm = COUNT(Biên bản vi phạm theo khoản thu) | nA | A | nA | nA |
| 93 | Tính tổng tiền vi phạm = SUM(soluong * dongia theo khoản thu) | nA | A | nA | nA |
| 94 | Hiển thị số liệu tổng hợp tình hình vi phạm | nA | A | nA | nA |
| 95 | Mở form Báo cáo tình hình vi phạm | nA | A | nA | nA |
| 96 | Tiếp nhận số liệu tổng hợp tình hình vi phạm | nA | A | nA | nA |
| 97 | Hiển thị chi tiết Báo cáo tình hình vi phạm | nA | A | nA | nA |
| 98 | In Báo cáo tình hình vi phạm | nA | A | nA | nA |
| 99 | Mở form kiểm tra khả năng đáp ứng sách | nA | nA | A | nA |
| 100 | Tiếp nhận Phiếu yêu cầu lấy sách, yêu cầu đổi bản sách | nA | nA | A | nA |
| 101 | Kiểm soát dữ liệu nhập (ten_sach, tacgia) | nA | nA | A | nA |
| 102 | Truy xuất danh sách bản sách đang được mượn | A | A | A | A |
| 103 | Kiểm tra bản sách sẵn sàng cho mượn | nA | nA | A | nA |
| 104 | Hiển thị bản sách đáp ứng | nA | nA | A | nA |
| 105 | Chuyển kết quả đáp ứng sách cho Bộ phận mượn trả sách | nA | nA | A | nA |
| 106 | Mở form cập nhật tình trạng sách | nA | nA | A | nA |
| 107 | Tiếp nhận thông tin sách đã cho mượn | nA | nA | A | nA |
| 108 | Cập nhật tình trạng sách đang được mượn | nA | nA | A | A |
| 109 | Tiếp nhận sách trả | nA | nA | A | nA |
| 110 | Kiểm soát dữ liệu nhập (so_ptra) | nA | nA | A | nA |
| 111 | Tìm kiếm thông tin Phiếu trả sách | A | nA | A | A |
| 112 | Cập nhật tình trạng sách sau trả | nA | nA | A | A |
| 113 | Tiếp nhận thông tin sách bị mất, hư hỏng | nA | nA | A | nA |
| 114 | Kiểm soát dữ liệu nhập (so_bienban) | nA | nA | A | nA |
| 115 | Tìm kiếm thông tin Biên bản vi phạm | A | A | A | A |
| 116 | Cập nhật tình trạng sách bị mất, hư hỏng | nA | nA | A | A |
| 117 | Mở form tổng hợp số sách theo tình trạng | nA | nA | A | nA |
| 118 | Tiếp nhận yêu cầu đối chiếu tình trạng sách | nA | nA | A | nA |
| 119 | Truy xuất danh sách bản sách theo tình trạng | A | nA | A | A |
| 120 | Tính số sách theo tình trạng = COUNT(bản sách theo tinhtrang) | nA | nA | A | nA |
| 121 | Hiển thị số liệu tổng hợp số sách theo tình trạng | nA | nA | A | nA |
| 122 | Chuyển dữ liệu tình trạng sách cho Bộ phận mượn trả sách | nA | nA | A | nA |

## 4. Quy tắc nghiệp vụ và tham số
- Tham số nghiệp vụ đặt tập trung trong `appsettings.json` mục `ThuVien` (đọc qua IOptions) (giá trị mặc định đề xuất – thư viện điều chỉnh theo quy định thật trước khi đưa vào dùng): số ngày được mượn và số bản tối đa theo `doituong` – Sinh viên 14 ngày/5 bản, Học viên 21 ngày/7 bản, Cán bộ 30 ngày/10 bản; gia hạn thêm 7 ngày, mỗi bản tối đa 1 lần; mật khẩu mặc định khi tạo/đặt lại tài khoản `Thuvien@123`.
- Điều kiện mượn: thẻ còn hạn (`hsd_tungay` ≤ hôm nay ≤ `hsd_denngay`), không có sách quá hạn chưa trả, không có biên bản chưa thanh toán, chưa vượt số bản tối đa.
- `hantra = ngaymuon + số ngày được mượn theo BANDOC.doituong`; `hantra_moi = hantra_cu + số ngày gia hạn`; `so_ngay_quahan = max(0, ngaytra − hantra)`; `thanh_tien = soluong × dongia`; `tong_tien = SUM(thanh_tien)`.
- Số chứng từ sinh **khi lưu/in** (không sinh khi mở form để tránh mất số): `PMyyMM####`, `GHyyMM####`, `PTyyMM####`, `BByyMM####` – đúng 10 ký tự cho khớp C(10): 2 chữ loại + 2 số năm + 2 số tháng + 4 số thứ tự, **đánh lại từ 0001 mỗi tháng** (ví dụ `PM26110001` là Phiếu mượn đầu tiên tháng 11/2026). *(Cập nhật 15/11/2026 theo quyết định chủ dự án – xem `GIA_DINH.md` D4.)*
- Bạn đọc đã có Phiếu mượn lập trong ngày thì mượn thêm được **bổ sung** vào phiếu đó, không cấp số mới.
- Cập nhật `SACH.tinhtrang` theo giao dịch: cho mượn → “Đang được mượn”; trả bình thường → “Sẵn sàng”; mất → “Mất”; hư hỏng → “Hư hỏng”. Các bước này là tiến trình 03.xx của Bộ phận kho sách, thực hiện ở Danh mục sách (6.1.1) qua danh sách “sách chuyển đến”.
- Không xóa chứng từ đã in; không xóa tài khoản đã phát sinh chứng từ (chỉ khóa).

## 5. Quy tắc giao diện chung
- Khung chính: thanh tiêu đề (tên hệ thống, tên + nhóm người dùng, nút Đăng xuất, Đổi mật khẩu), menu trái theo nhóm, vùng nội dung.
- Mỗi giao diện có tiêu đề đúng “Tên giao diện” ở mục 6, khu nhập liệu phía trên, lưới dữ liệu (`dgv…`) phía dưới, hàng nút lệnh cố định.
- Ô chỉ đọc có nền xám nhạt; ô bắt buộc có dấu *; lỗi hiển thị ngay dưới ô theo đúng “Điều kiện toàn vẹn”.
- Hộp thoại: xác nhận trước khi Xóa/Khóa/Phục hồi/Lưu quyền; thông báo kết quả sau mỗi thao tác.
- Giao diện chứng từ (6.2.x) theo **3 trạng thái**: (1) chỉ nhập mã và tìm; (2) thêm dòng chi tiết; (3) khi đủ thông tin mới bật nút In. In xong form xóa trắng và về trạng thái 1.
- Phong cách: theo Design System Dashboard trắng – đỏ ở `03_DESIGN_SYSTEM.md`.

## 6. Đặc tả từng giao diện (mục IV)
Đối chiếu số mục với báo cáo: 6.1.k ↔ 4.2.k; 6.2.k ↔ 4.3.k; 6.3.k ↔ 4.6.k; 6.4.1–6.4.4 ↔ 4.4.1–4.4.4; 6.4.5–6.4.6 ↔ 4.5.1–4.5.2. Ảnh chụp đặt tên theo số mục báo cáo.

### 6.1 Giao diện danh mục

### 6.1.1 Danh mục sách
- **Người sử dụng:** Nhân viên Bộ phận kho sách (N3: thêm, sửa, tìm kiếm); Quản trị (N4: thêm, sửa, xóa, tìm kiếm); Bộ phận mượn trả sách (N1: chỉ xem)
- **Nhiệm vụ:** Xem danh sách bản sách của thư viện và các chức năng: thêm, sửa, xóa, tìm kiếm bản sách. Bộ phận kho sách dùng giao diện này để thực hiện: 03.01.02 Kiểm tra khả năng đáp ứng sách (tìm bản sách sẵn sàng, vị trí lưu trữ); 03.01.04, 03.02.03, 03.03.02 Cập nhật tình trạng sách theo thông tin Bộ phận mượn trả sách, Bộ phận xử lý vi phạm chuyển sang; 03.04.01 Tổng hợp số sách theo tình trạng.
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N3 – Kho sách hoặc N4 – Quản trị; Từ màn hình chính chọn Menu Kho sách (N3) hoặc Menu Quản trị (N4) → Danh mục sách; Danh mục khu vực lưu trữ (KHUVUC) đã có dữ liệu; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi thêm bản sách mới: hiển thị thông báo (thành công hoặc thất bại); danh sách bản sách hiển thị bản sách mới với tình trạng mặc định “Sẵn sàng”.
  - Sau khi sửa: hiển thị thông báo; danh sách cập nhật thông tin đã sửa.
  - Sau khi xóa: hỏi xác nhận; nếu bản sách đã có trong phiếu mượn hoặc biên bản vi phạm thì không cho xóa và thông báo lý do; ngược lại xóa và cập nhật danh sách.
  - Sau khi tìm kiếm: danh sách chỉ hiển thị các bản sách phù hợp từ khóa; không tìm thấy thì thông báo.
  - Sau khi cập nhật tình trạng sách theo thông tin chuyển đến: SACH.tinhtrang được cập nhật (Đang được mượn, Sẵn sàng, Mất, Hư hỏng); thông tin chuyển đến được đánh dấu đã xử lý.
  - Sau khi lọc theo tình trạng: hiển thị danh sách và tổng số bản sách theo từng tình trạng.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | SACH | ma_bansach | C, R, D | C(10), chữ cái + chữ số, không trùng, không rỗng |
  | SACH | ma_khuvuc | C, E, R, D | Chọn từ KHUVUC (phải tồn tại trong KHUVUC) |
  | SACH | ten_sach | C, E, R, D | C(200), chữ cái, không rỗng |
  | SACH | tacgia | C, E, R, D | C(100), chữ cái, không rỗng |
  | SACH | tinhtrang | C, E, R, D | Mặc định “Sẵn sàng”; thuộc {Sẵn sàng, Đang được mượn, Mất, Hư hỏng} |
  | KHUVUC | ma_khuvuc | R | – |
  | KHUVUC | vitri | R | – |
  | CT_PHIEUMUON | ma_bansach | R | – |
  | CT_PHIEUMUON | hantra | R | – |
  | CT_PHIEUTRA | ma_bansach | R | – |
  | CT_PHIEUTRA | tinhtrang_tra | R | – |
  | CT_BIENBAN | ma_bansach | R | – |
  | CT_BIENBAN | ma_khoanthu | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmDanhMucSach (Load)` | Nếu mở form / Thì nạp SACH lên dgvSach; các ô nhập ReadOnly = True; btnThem, btnTimKiem → Enable = True; btnSua, btnXoa, btnLuu, btnHuy → Enable = False; nhóm chỉ có quyền R (theo PHANQUYEN) thì btnThem, btnSua, btnXoa, btnLuu → Visible = False; btnCapNhatTT → Enable = False (chỉ bật khi chọn 1 dòng ở dgvChuyenDen) | SACH |
  | `dgvSach` | Lặp / Nếu click vào dòng i của dgvSach / Thì đổ ma_bansach, ten_sach, tacgia, tinhtrang, khu vực của dòng i lên các ô nhập (chỉ đọc). btnSua, btnXoa → Enable = True / Cho đến khi đóng form | SACH, KHUVUC |
  | `btnThem` | Lặp / Nếu click nút này / Thì xóa trắng các ô, mở các ô nhập, tinhtrang mặc định “Sẵn sàng”, đặt con trỏ tại ô mã bản sách. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnSua` | Lặp / Nếu click nút này (đã chọn 1 dòng) / Thì mở các ô cho sửa, trừ ma_bansach. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnXoa` | Lặp / Nếu click nút này (đã chọn 1 dòng) / Thì hỏi xác nhận; kiểm tra bản sách có trong CT_PHIEUMUON, CT_BIENBAN; nếu không có thì xóa khỏi SACH, tải lại dgvSach. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form | SACH, CT_PHIEUMUON, CT_BIENBAN |
  | `btnLuu` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập theo điều kiện toàn vẹn; thêm mới hoặc cập nhật SACH; ghi nhật ký; thông báo kết quả; tải lại dgvSach. Các ô nhập ReadOnly = True; các nút trở về trạng thái khi mở form / Cho đến khi đóng form | SACH, NHATKY_HOATDONG |
  | `btnHuy` | Lặp / Nếu click nút này / Thì bỏ thao tác đang làm, trả các ô về trạng thái chỉ đọc. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form |  |
  | `txtTimKiem, btnTimKiem` | Lặp / Nếu nhập từ khóa và click nút tìm / Thì lọc SACH theo mã, tên sách, tác giả, tình trạng, khu vực; loại các bản sách đang thuộc phiếu mượn chưa trả; hiển thị lên dgvSach kèm vị trí lưu trữ (03.01.02) / Cho đến khi đóng form | SACH, KHUVUC, CT_PHIEUMUON |
  | `dgvChuyenDen` | Lặp / Nếu chọn dòng sách do bộ phận khác chuyển đến (sách đã cho mượn, sách trả, sách bị mất, hư hỏng) / Thì nạp bản sách, đề xuất tình trạng mới tương ứng (03.01.04, 03.02.03, 03.03.02). btnCapNhatTT → Enable = True / Cho đến khi đóng form | CT_PHIEUMUON, CT_PHIEUTRA, CT_BIENBAN |
  | `btnCapNhatTT` | Lặp / Nếu click nút này / Thì cập nhật SACH.tinhtrang của bản sách đã chọn; ghi nhật ký; tải lại dgvSach. btnCapNhatTT → Enable = False / Cho đến khi đóng form | SACH, NHATKY_HOATDONG |
  | `cboLocTinhTrang` | Lặp / Nếu thay đổi tình trạng cần lọc / Thì hiển thị bản sách theo tình trạng và Số sách = COUNT(ma_bansach) theo tinhtrang (03.04.01) / Cho đến khi đóng form | SACH |
  | `btnDong` | Lặp / Nếu click nút này / Thì đóng form, trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B01 | SACH | Thêm (insert toàn bộ bản ghi) | Bản ghi mới: ma_bansach, ma_khuvuc, ten_sach, tacgia, tinhtrang = “Sẵn sàng” |
  | B01 | SACH | Sửa (update) | ma_khuvuc, ten_sach, tacgia, tinhtrang (gồm cập nhật tình trạng theo 03.01.04, 03.02.03, 03.03.02) |
  | B01 | SACH | Xóa (delete bản ghi) | Xóa bản ghi |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung (ghi lại thao tác) |

### 6.1.2 Danh mục khoản thu
- **Người sử dụng:** Quản trị (N4: thêm, sửa, xóa, tìm kiếm); Bộ phận xử lý vi phạm (N2: chỉ xem)
- **Nhiệm vụ:** Xem danh sách các khoản thu khi xử lý vi phạm (phạt quá hạn, bồi thường mất, hư hỏng sách) và các chức năng: thêm, sửa, xóa, tìm kiếm khoản thu.
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N4 – Quản trị (N2 chỉ xem); Từ màn hình chính chọn Menu Quản trị (N4) hoặc Menu Xử lý vi phạm (N2, chỉ xem) → Danh mục khoản thu; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi thêm, sửa: hiển thị thông báo; danh sách khoản thu cập nhật.
  - Sau khi xóa: hỏi xác nhận; khoản thu đã dùng trong biên bản vi phạm thì không cho xóa.
  - Sau khi tìm kiếm: hiển thị các khoản thu phù hợp từ khóa.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | KHOANTHU | ma_khoanthu | C, R, D | C(10), chữ cái + chữ số, không trùng, không rỗng |
  | KHOANTHU | noidung_tt | C, E, R, D | C(100), chữ cái, không rỗng |
  | KHOANTHU | dongia | C, E, R, D | N(10), chữ số, > 0 |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmDanhMucKhoanThu (Load)` | Nếu mở form / Thì nạp KHOANTHU lên dgvKhoanThu; các ô nhập ReadOnly = True; btnThem, btnTimKiem → Enable = True; btnSua, btnXoa, btnLuu, btnHuy → Enable = False; nhóm chỉ có quyền R (theo PHANQUYEN) thì btnThem, btnSua, btnXoa, btnLuu → Visible = False | KHOANTHU |
  | `dgvKhoanThu` | Lặp / Nếu click vào dòng i / Thì đổ ma_khoanthu, noidung_tt, dongia lên các ô (chỉ đọc). btnSua, btnXoa → Enable = True / Cho đến khi đóng form | KHOANTHU |
  | `btnThem` | Lặp / Nếu click nút này / Thì xóa trắng, mở các ô nhập. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnSua` | Lặp / Nếu click nút này / Thì mở các ô cho sửa, trừ ma_khoanthu. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnXoa` | Lặp / Nếu click nút này / Thì hỏi xác nhận; kiểm tra khoản thu có trong CT_BIENBAN; nếu không có thì xóa. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form | KHOANTHU, CT_BIENBAN |
  | `btnLuu` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập; thêm mới hoặc cập nhật KHOANTHU; ghi nhật ký; thông báo. Các ô nhập ReadOnly = True; các nút trở về trạng thái khi mở form / Cho đến khi đóng form | KHOANTHU, NHATKY_HOATDONG |
  | `btnHuy` | Lặp / Nếu click nút này / Thì bỏ thao tác đang làm. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form |  |
  | `txtTimKiem, btnTimKiem` | Lặp / Nếu click nút tìm / Thì lọc KHOANTHU theo mã, nội dung / Cho đến khi đóng form | KHOANTHU |
  | `btnDong` | Lặp / Nếu click nút này / Thì trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B02 | KHOANTHU | Thêm (insert toàn bộ bản ghi) | Bản ghi mới: ma_khoanthu, noidung_tt, dongia |
  | B02 | KHOANTHU | Sửa (update) | noidung_tt, dongia |
  | B02 | KHOANTHU | Xóa (delete bản ghi) | Xóa bản ghi |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.1.3 Danh mục Bạn đọc
- **Người sử dụng:** Nhân viên Bộ phận mượn trả sách (N1: thêm, sửa, xóa, tìm kiếm); Quản trị (N4: sửa, xóa, tìm kiếm); Bộ phận xử lý vi phạm (N2: chỉ xem)
- **Nhiệm vụ:** Thực hiện tiến trình nghiệp vụ 01.01.01 Lưu thông tin Thẻ Bạn đọc: tìm Bạn đọc theo mã sinh viên, học viên, cán bộ; lưu thông tin Thẻ Bạn đọc mới; sửa, xóa, tìm kiếm Bạn đọc; xem điều kiện mượn của Bạn đọc (hạn thẻ, sách đang mượn, sách quá hạn, vi phạm chưa thanh toán).
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N1 – Mượn trả sách hoặc N4 – Quản trị; Từ màn hình chính chọn Menu Mượn trả sách → 01.01.01 Lưu thông tin Thẻ Bạn đọc (N1), hoặc Menu Quản trị → Danh mục Bạn đọc (N4); Bạn đọc xuất trình Thẻ thông tin Bạn đọc (MB01); Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi tìm kiếm theo ma_svhvcb: tìm thấy thì hiển thị thông tin Bạn đọc và danh sách sách đang mượn; không thấy thì thông báo và mở chế độ thêm mới.
  - Sau khi thêm Bạn đọc mới: hiển thị thông báo; Bạn đọc mới xuất hiện trong danh sách.
  - Sau khi kiểm tra điều kiện mượn: hiển thị kết quả “Đủ điều kiện mượn” hoặc lý do không đủ (thẻ hết hạn, còn sách quá hạn, còn vi phạm chưa thanh toán).
  - Sau khi sửa, xóa: hiển thị thông báo; Bạn đọc đã có phiếu mượn thì không cho xóa.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | BANDOC | ma_bandoc | C, R, D | C(10), chữ cái + chữ số, không trùng, không rỗng |
  | BANDOC | hoten | C, E, R, D | C(50), chữ cái, không rỗng |
  | BANDOC | doituong | C, E, R, D | Thuộc {Sinh viên, Học viên, Cán bộ} |
  | BANDOC | ma_svhvcb | C, E, R, D | C(15), chữ cái + chữ số, không trùng, không rỗng |
  | BANDOC | lop_donvi | C, E, R, D | C(50), không rỗng |
  | BANDOC | ngaysinh | C, E, R, D | dd/mm/yyyy, nhỏ hơn ngày hiện tại |
  | BANDOC | so_thebd | C, E, R, D | C(15), chữ cái + chữ số, không trùng |
  | BANDOC | dienthoai_email | C, E, R, D | C(50), không bắt buộc |
  | BANDOC | hsd_tungay | C, E, R, D | dd/mm/yyyy |
  | BANDOC | hsd_denngay | C, E, R, D | dd/mm/yyyy, lớn hơn hsd_tungay |
  | BANDOC | anh | C, E, R, D | Đường dẫn tệp ảnh, không bắt buộc |
  | PHIEUMUON | so_pmuon, ma_bandoc | R | – |
  | CT_PHIEUMUON | ma_bansach, hantra | R | – |
  | BIENBAN | so_bienban, ma_bandoc, ht_thanhtoan | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmDanhMucBanDoc (Load)` | Nếu mở form / Thì nạp BANDOC lên dgvBanDoc; các ô nhập ReadOnly = True; btnThem, btnTimKiem → Enable = True; btnSua, btnXoa, btnLuu, btnHuy → Enable = False; nhóm chỉ có quyền R (theo PHANQUYEN) thì btnThem, btnSua, btnXoa, btnLuu → Visible = False; btnKiemTraDK → Enable = False | BANDOC |
  | `txtMaSV, btnTimKiem` | Lặp / Nếu nhập ma_svhvcb và click nút tìm / Thì tìm trong BANDOC; nếu thấy thì hiển thị thông tin Bạn đọc; nếu không thấy thì thông báo và mở chế độ thêm mới. Nếu tìm thấy thì btnKiemTraDK, btnSua, btnXoa → Enable = True; nếu không thấy thì btnLuu, btnHuy → Enable = True; btnKiemTraDK, btnSua, btnXoa → Enable = False / Cho đến khi đóng form | BANDOC |
  | `btnKiemTraDK` | Lặp / Nếu click nút này (đã tìm thấy Bạn đọc) / Thì kiểm tra hsd_denngay ≥ ngày hiện tại; truy xuất sách đang mượn, sách quá hạn (CT_PHIEUMUON.hantra < ngày hiện tại); truy xuất vi phạm chưa thanh toán; hiển thị kết quả điều kiện mượn / Cho đến khi đóng form | BANDOC, PHIEUMUON, CT_PHIEUMUON, BIENBAN |
  | `dgvBanDoc` | Lặp / Nếu click vào dòng i / Thì đổ thông tin Bạn đọc dòng i lên các ô (chỉ đọc). btnSua, btnXoa → Enable = True / Cho đến khi đóng form | BANDOC |
  | `btnThem` | Lặp / Nếu click nút này / Thì xóa trắng, mở các ô nhập. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnSua` | Lặp / Nếu click nút này / Thì mở các ô cho sửa, trừ ma_bandoc. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnXoa` | Lặp / Nếu click nút này / Thì hỏi xác nhận; kiểm tra Bạn đọc có trong PHIEUMUON; nếu không có thì xóa. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form | BANDOC, PHIEUMUON |
  | `btnLuu` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập; thêm mới hoặc cập nhật BANDOC; ghi nhật ký; thông báo. Các ô nhập ReadOnly = True; các nút trở về trạng thái khi mở form / Cho đến khi đóng form | BANDOC, NHATKY_HOATDONG |
  | `btnHuy` | Lặp / Nếu click nút này / Thì bỏ thao tác đang làm. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form |  |
  | `btnDong` | Lặp / Nếu click nút này / Thì trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B03 | BANDOC | Thêm (insert toàn bộ bản ghi) | Bản ghi mới: ma_bandoc, hoten, doituong, ma_svhvcb, lop_donvi, ngaysinh, so_thebd, dienthoai_email, hsd_tungay, hsd_denngay, anh |
  | B03 | BANDOC | Sửa (update) | Các trường đã sửa (trừ ma_bandoc) |
  | B03 | BANDOC | Xóa (delete bản ghi) | Xóa bản ghi |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.1.4 Danh mục khu vực lưu trữ
- **Người sử dụng:** Nhân viên Bộ phận kho sách (N3: thêm, sửa, tìm kiếm); Quản trị (N4: thêm, sửa, xóa, tìm kiếm); Bộ phận mượn trả sách (N1: chỉ xem)
- **Nhiệm vụ:** Xem danh sách khu vực lưu trữ sách (kệ mở, kho) và các chức năng: thêm, sửa, xóa, tìm kiếm khu vực.
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N3 – Kho sách hoặc N4 – Quản trị; Từ màn hình chính chọn Menu Kho sách (N3) hoặc Menu Quản trị (N4) → Danh mục khu vực lưu trữ; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi thêm, sửa: hiển thị thông báo; danh sách khu vực cập nhật.
  - Sau khi xóa: hỏi xác nhận; khu vực còn bản sách thì không cho xóa.
  - Sau khi tìm kiếm: hiển thị các khu vực phù hợp.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | KHUVUC | ma_khuvuc | C, R, D | C(10), chữ cái + chữ số, không trùng, không rỗng |
  | KHUVUC | vitri | C, E, R, D | C(50), không rỗng |
  | KHUVUC | loai_khuvuc | C, E, R, D | Thuộc {Kệ mở, Kho} |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmDanhMucKhuVuc (Load)` | Nếu mở form / Thì nạp KHUVUC lên dgvKhuVuc; các ô nhập ReadOnly = True; btnThem, btnTimKiem → Enable = True; btnSua, btnXoa, btnLuu, btnHuy → Enable = False; nhóm chỉ có quyền R (theo PHANQUYEN) thì btnThem, btnSua, btnXoa, btnLuu → Visible = False | KHUVUC |
  | `dgvKhuVuc` | Lặp / Nếu click vào dòng i / Thì đổ ma_khuvuc, vitri, loai_khuvuc lên các ô (chỉ đọc). btnSua, btnXoa → Enable = True / Cho đến khi đóng form | KHUVUC |
  | `btnThem` | Lặp / Nếu click nút này / Thì xóa trắng, mở các ô nhập. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnSua` | Lặp / Nếu click nút này / Thì mở các ô cho sửa, trừ ma_khuvuc. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnXoa, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnXoa` | Lặp / Nếu click nút này / Thì hỏi xác nhận; kiểm tra khu vực còn bản sách trong SACH; nếu không còn thì xóa. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form | KHUVUC, SACH |
  | `btnLuu` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập; thêm mới hoặc cập nhật KHUVUC; ghi nhật ký; thông báo. Các ô nhập ReadOnly = True; các nút trở về trạng thái khi mở form / Cho đến khi đóng form | KHUVUC, NHATKY_HOATDONG |
  | `btnHuy` | Lặp / Nếu click nút này / Thì bỏ thao tác đang làm. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form |  |
  | `txtTimKiem, btnTimKiem` | Lặp / Nếu click nút tìm / Thì lọc KHUVUC theo mã, vị trí, loại / Cho đến khi đóng form | KHUVUC |
  | `btnDong` | Lặp / Nếu click nút này / Thì trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B04 | KHUVUC | Thêm (insert toàn bộ bản ghi) | Bản ghi mới: ma_khuvuc, vitri, loai_khuvuc |
  | B04 | KHUVUC | Sửa (update) | vitri, loai_khuvuc |
  | B04 | KHUVUC | Xóa (delete bản ghi) | Xóa bản ghi |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.2 Giao diện lập chứng từ (nghiệp vụ)

### 6.2.1 Lập Phiếu mượn sách (01.01.03)
- **Người sử dụng:** Nhân viên Bộ phận mượn trả sách (N1). Sau khi đăng nhập, N1 vào thẳng màn hình này
- **Nhiệm vụ:** Lập Phiếu mượn sách mới hoặc bổ sung bản sách vào Phiếu mượn sách Bạn đọc đã lập trong ngày; sửa Phiếu mượn sách đã lưu khi nhập sai; in Phiếu mượn sách (MB02). Tiến trình gắn với giao dịch: tra tình trạng, vị trí bản sách; chuyển Phiếu yêu cầu lấy sách trong kho, yêu cầu đổi bản sách cho Bộ phận kho sách (03.01.02); chuyển thông tin sách đã cho mượn cho Bộ phận kho sách để cập nhật tình trạng (03.01.04).
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N1 – Mượn trả sách; Sau khi đăng nhập, N1 vào thẳng màn hình này (hoặc chọn Menu Mượn trả sách → 01.01.03 Lập Phiếu mượn sách); Bạn đọc đã có trong BANDOC và đủ điều kiện mượn (kiểm tra ở giao diện Bạn đọc – 01.01.01); Bản sách có trong SACH; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi In phiếu (btnIn) với phiếu mới: hệ thống cấp so_pmuon, ngaylap_pmuon; dữ liệu phiếu được chèn vào PHIEUMUON, các dòng bản sách vào CT_PHIEUMUON; thông tin sách đã cho mượn được chuyển cho Bộ phận kho sách để cập nhật tình trạng (03.01.04); in Phiếu mượn sách (MB02); form xóa trắng và trở về trạng thái 1 để lập chứng từ tiếp theo.
  - Nếu Bạn đọc đã có Phiếu mượn sách lập trong ngày: hệ thống nạp phiếu đó, các bản sách mượn thêm được bổ sung vào CT_PHIEUMUON của phiếu cũ, không cấp số phiếu mới.
  - Sau khi sửa phiếu đã lưu (btnTimPhieu → sửa → btnIn): các dòng thay đổi được cập nhật trong CT_PHIEUMUON; bản sách thay đổi được chuyển lại cho Bộ phận kho sách.
  - Sau khi Hủy: xóa trắng form, không cấp số phiếu.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | PHIEUMUON | so_pmuon | R | – |
  | PHIEUMUON | ma_bandoc | C, R | Phải tồn tại trong BANDOC; Bạn đọc đủ điều kiện mượn |
  | PHIEUMUON | ngaylap_pmuon | R | – |
  | PHIEUMUON | hoten | C, R | Lấy từ BANDOC.hoten |
  | PHIEUMUON | ma_nd, ten_nd | C, R | Lấy từ tài khoản đăng nhập |
  | CT_PHIEUMUON | ma_bansach | C, E, R | Phải tồn tại trong SACH, tinhtrang = “Sẵn sàng”, không trùng trong phiếu |
  | CT_PHIEUMUON | so_pmuon | C, R | = so_pmuon của phiếu |
  | CT_PHIEUMUON | ngaymuon | C, R | = ngày hiện tại |
  | CT_PHIEUMUON | hantra | C, R | = ngaymuon + số ngày được mượn theo BANDOC.doituong |
  | CT_PHIEUMUON | tinhtrang_muon | C, E, R | Không rỗng |
  | CT_PHIEUMUON | ghichu_muon | C, E, R | Không bắt buộc |
  | CT_PHIEUMUON | ten_sach | C, R | Lấy từ SACH.ten_sach |
  | BANDOC | ma_bandoc, hoten, doituong, ma_svhvcb | R | – |
  | SACH | ma_bansach, ten_sach, tinhtrang | R | – |
  | KHUVUC | vitri | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmLapPhieuMuon (Load)` | Nếu mở form / Thì trạng thái 1: chỉ cho nhập ma_svhvcb và tìm Bạn đọc (hoặc nhập so_pmuon để mở phiếu đã lưu cần sửa); txtMaSV, btnTimBanDoc, btnTimPhieu → Enable = True; txtMaBanSach, btnThemSach, dgvChiTiet, btnIn, btnHuy → Enable = False |  |
  | `txtMaSV, btnTimBanDoc` | Lặp / Nếu nhập ma_svhvcb và click nút tìm / Thì tìm trong BANDOC, hiển thị hoten, doituong; kiểm tra điều kiện mượn; tìm Phiếu mượn sách lập trong ngày của Bạn đọc, nếu có thì nạp phiếu ở chế độ bổ sung. Nếu tìm thấy và hợp lệ thì chuyển trạng thái 2: txtMaBanSach, btnThemSach, dgvChiTiet, btnHuy → Enable = True; txtMaSV, btnTimBanDoc, btnTimPhieu → Enable = False / Cho đến khi đóng form | BANDOC, PHIEUMUON, CT_PHIEUMUON, BIENBAN |
  | `txtMaBanSach, btnThemSach` | Lặp / Nếu nhập ma_bansach và click nút thêm / Thì kiểm tra SACH.tinhtrang = “Sẵn sàng” (không thì thông báo không đáp ứng hoặc gửi yêu cầu lấy sách trong kho, đổi bản sách); thêm dòng vào dgvChiTiet; tính hantra; hiển thị vị trí lưu trữ. Khi lưới đã có ít nhất 1 dòng hợp lệ thì chuyển trạng thái 3: btnIn → Enable = True / Cho đến khi đóng form | SACH, KHUVUC, BANDOC |
  | `dgvChiTiet` | Lặp / Nếu double click vào dòng i / Thì cho sửa tinhtrang_muon, ghichu_muon hoặc bỏ dòng i khỏi lưới (dòng chưa lưu). Nếu lưới không còn dòng nào thì btnIn → Enable = False / Cho đến khi đóng form | CT_PHIEUMUON |
  | `btnIn` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập; nếu là Phiếu mượn sách mới thì tự động sinh so_pmuon, ngaylap_pmuon; lưu PHIEUMUON, CT_PHIEUMUON; chuyển thông tin sách đã cho mượn cho Bộ phận kho sách; hiển thị tiến trình lưu (%) và thông báo; in Phiếu mượn sách; xóa trắng form, trở về trạng thái 1 để lập chứng từ tiếp theo; txtMaSV, btnTimBanDoc, btnTimPhieu → Enable = True; txtMaBanSach, btnThemSach, dgvChiTiet, btnIn, btnHuy → Enable = False / Cho đến khi đóng form | PHIEUMUON, CT_PHIEUMUON |
  | `btnTimPhieu` | Lặp / Nếu nhập so_pmuon và click nút này / Thì nạp Phiếu mượn sách đã lưu lên form ở chế độ sửa. Chuyển trạng thái 3: txtMaBanSach, btnThemSach, dgvChiTiet, btnIn, btnHuy → Enable = True; txtMaSV, btnTimBanDoc, btnTimPhieu → Enable = False / Cho đến khi đóng form | PHIEUMUON, CT_PHIEUMUON |
  | `btnHuy` | Lặp / Nếu click nút này / Thì xóa trắng form, không cấp số phiếu; trở về trạng thái 1: txtMaSV, btnTimBanDoc, btnTimPhieu → Enable = True; txtMaBanSach, btnThemSach, dgvChiTiet, btnIn, btnHuy → Enable = False / Cho đến khi đóng form |  |
  | `btnDong` | Lặp / Nếu click nút này / Thì trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B05 | PHIEUMUON | Thêm (insert bản ghi phiếu mới) | so_pmuon, ma_bandoc, ngaylap_pmuon, hoten, ma_nd, ten_nd |
  | B05.1 | CT_PHIEUMUON | Thêm (insert) / Sửa (update) | ma_bansach, so_pmuon, ngaymuon, hantra, tinhtrang_muon, ghichu_muon, ten_sach |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.2.2 Lập Phiếu gia hạn sách (01.02.01)
- **Người sử dụng:** Nhân viên Bộ phận mượn trả sách (N1)
- **Nhiệm vụ:** Lập Phiếu gia hạn sách cho các bản sách trên một Phiếu mượn sách; kiểm tra điều kiện gia hạn, tính hạn trả mới; sửa phiếu đã lưu khi nhập sai; in Phiếu gia hạn sách (MB03).
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N1 – Mượn trả sách; Từ màn hình chính chọn Menu Mượn trả sách → 01.02.01 Lập Phiếu gia hạn sách; Phiếu mượn sách đã có trong PHIEUMUON, CT_PHIEUMUON và còn bản sách chưa trả; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi In phiếu (btnIn) với phiếu mới: hệ thống cấp so_pgiahan, ngaylap_pgiahan; dữ liệu được chèn vào PHIEUGIAHAN, CT_PHIEUGIAHAN với ketqua_giahan, hantra_moi (hoặc lydo_khonggh); in Phiếu gia hạn sách (MB03).
  - Bản sách không đủ điều kiện gia hạn: hiển thị lý do từ chối, dòng đó ghi ketqua_giahan = “Không đồng ý”.
  - Sau khi sửa phiếu đã lưu: các dòng thay đổi được cập nhật trong CT_PHIEUGIAHAN.
  - Sau khi Hủy: xóa trắng form, không cấp số phiếu.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | PHIEUGIAHAN | so_pgiahan | R | – |
  | PHIEUGIAHAN | so_pmuon | C, R | Phải tồn tại trong PHIEUMUON |
  | PHIEUGIAHAN | ma_bandoc | C, R | Lấy từ PHIEUMUON.ma_bandoc |
  | PHIEUGIAHAN | ngaylap_pgiahan | R | – |
  | PHIEUGIAHAN | ngay_denghi | C, E, R | dd/mm/yyyy, ≥ ngày hiện tại |
  | PHIEUGIAHAN | ma_nd, ten_nd | C, R | Lấy từ tài khoản đăng nhập |
  | CT_PHIEUGIAHAN | ma_bansach | C, R | Thuộc Phiếu mượn sách, chưa trả |
  | CT_PHIEUGIAHAN | so_pgiahan | C, R | = so_pgiahan của phiếu |
  | CT_PHIEUGIAHAN | hantra_cu | C, R | Lấy hạn trả hiện hành của bản sách |
  | CT_PHIEUGIAHAN | ketqua_giahan | C, E, R | Thuộc {Đồng ý, Không đồng ý} |
  | CT_PHIEUGIAHAN | hantra_moi | C, R | = hantra_cu + số ngày được gia hạn (khi Đồng ý) |
  | CT_PHIEUGIAHAN | lydo_khonggh | C, E, R | Bắt buộc khi Không đồng ý |
  | CT_PHIEUGIAHAN | ten_sach | C, R | Lấy từ SACH.ten_sach |
  | CT_PHIEUGIAHAN | so_lan_giahan | C, R | = COUNT(lần gia hạn được đồng ý của bản sách) |
  | PHIEUMUON | so_pmuon, ma_bandoc | R | – |
  | CT_PHIEUMUON | ma_bansach, hantra | R | – |
  | BANDOC | ma_bandoc, hoten | R | – |
  | SACH | ma_bansach, tinhtrang | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmLapPhieuGiaHan (Load)` | Nếu mở form / Thì trạng thái 1: chỉ cho nhập so_pmuon và tìm Phiếu mượn sách (hoặc nhập so_pgiahan để mở phiếu đã lưu cần sửa); txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = True; dgvChiTiet, btnIn, btnHuy → Enable = False |  |
  | `txtSoPhieuMuon, btnTimPhieuMuon` | Lặp / Nếu nhập so_pmuon và click nút tìm / Thì nạp Bạn đọc và các bản sách chưa trả của phiếu mượn lên dgvChiTiet. Nếu tìm thấy và hợp lệ thì chuyển trạng thái 2: dgvChiTiet, btnHuy → Enable = True; txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = False / Cho đến khi đóng form | PHIEUMUON, CT_PHIEUMUON, BANDOC |
  | `dgvChiTiet (chọn bản sách)` | Lặp / Nếu chọn bản sách và nhập ngay_denghi / Thì tính số lần đã gia hạn; kiểm tra điều kiện gia hạn; nếu đạt thì tính hantra_moi, ngược lại ghi lý do từ chối. Khi lưới đã có ít nhất 1 dòng hợp lệ thì chuyển trạng thái 3: btnIn → Enable = True / Cho đến khi đóng form | CT_PHIEUGIAHAN, SACH |
  | `btnIn` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập; nếu là Phiếu gia hạn sách mới thì tự động sinh so_pgiahan, ngaylap_pgiahan; lưu PHIEUGIAHAN, CT_PHIEUGIAHAN; ghi nhật ký; hiển thị tiến trình lưu (%) và thông báo; in Phiếu gia hạn sách; xóa trắng form, trở về trạng thái 1 để lập chứng từ tiếp theo; txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = True; dgvChiTiet, btnIn, btnHuy → Enable = False / Cho đến khi đóng form | PHIEUGIAHAN, CT_PHIEUGIAHAN |
  | `btnTimPhieu` | Lặp / Nếu nhập so_pgiahan và click nút này / Thì nạp Phiếu gia hạn sách đã lưu lên form ở chế độ sửa. Chuyển trạng thái 3: dgvChiTiet, btnIn, btnHuy → Enable = True; txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = False / Cho đến khi đóng form | PHIEUGIAHAN, CT_PHIEUGIAHAN |
  | `btnHuy` | Lặp / Nếu click nút này / Thì xóa trắng form, không cấp số phiếu; trở về trạng thái 1: txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = True; dgvChiTiet, btnIn, btnHuy → Enable = False / Cho đến khi đóng form |  |
  | `btnDong` | Lặp / Nếu click nút này / Thì trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B06 | PHIEUGIAHAN | Thêm (insert bản ghi phiếu mới) | so_pgiahan, so_pmuon, ma_bandoc, ngaylap_pgiahan, ngay_denghi, ma_nd, ten_nd |
  | B06.1 | CT_PHIEUGIAHAN | Thêm (insert) / Sửa (update) | ma_bansach, so_pgiahan, hantra_cu, ketqua_giahan, hantra_moi, lydo_khonggh, ten_sach, so_lan_giahan |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.2.3 Lập Phiếu trả sách (01.02.02)
- **Người sử dụng:** Nhân viên Bộ phận mượn trả sách (N1)
- **Nhiệm vụ:** Lập Phiếu trả sách cho một Phiếu mượn sách (mỗi Phiếu mượn có một Phiếu trả); Bạn đọc trả làm nhiều lần thì bổ sung bản sách vào Phiếu trả đã lập; tính số ngày quá hạn, xác định trường hợp vi phạm; sửa phiếu đã lưu; in Phiếu trả sách (MB04). Tiến trình gắn với giao dịch: chuyển sách trả cho Bộ phận kho sách để cập nhật tình trạng (03.02.03); chuyển trường hợp vi phạm cho Bộ phận xử lý vi phạm (02.03.01).
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N1 – Mượn trả sách; Từ màn hình chính chọn Menu Mượn trả sách → 01.02.02 Lập Phiếu trả sách; Phiếu mượn sách đã có trong PHIEUMUON, CT_PHIEUMUON; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi In phiếu (btnIn) với phiếu mới: hệ thống cấp so_ptra, ngaylap_ptra; chèn PHIEUTRA và các dòng CT_PHIEUTRA (tinhtrang_tra, so_ngay_quahan); sách trả được chuyển cho Bộ phận kho sách để cập nhật tình trạng (03.02.03); in Phiếu trả sách (MB04).
  - Nếu Phiếu mượn sách đã có Phiếu trả (trả lần trước): các bản sách trả lần này được bổ sung vào CT_PHIEUTRA của phiếu cũ, không cấp số mới.
  - Có bản sách quá hạn, mất hoặc hư hỏng: hệ thống chuyển trường hợp vi phạm sang Bộ phận xử lý vi phạm (giao diện Lập Biên bản vi phạm).
  - Sau khi sửa phiếu đã lưu: các dòng thay đổi được cập nhật trong CT_PHIEUTRA.
  - Sau khi Hủy: xóa trắng form, không cấp số phiếu.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | PHIEUTRA | so_ptra | R | – |
  | PHIEUTRA | so_pmuon | C, R | Phải tồn tại trong PHIEUMUON; mỗi so_pmuon chỉ có một Phiếu trả |
  | PHIEUTRA | ma_bandoc | C, R | Lấy từ PHIEUMUON.ma_bandoc |
  | PHIEUTRA | ngaylap_ptra | R | – |
  | PHIEUTRA | ngaytra | C, E, R | dd/mm/yyyy, ≥ ngaymuon |
  | PHIEUTRA | ma_nd, ten_nd | C, R | Lấy từ tài khoản đăng nhập |
  | CT_PHIEUTRA | ma_bansach | C, R | Thuộc Phiếu mượn sách, chưa trả |
  | CT_PHIEUTRA | so_ptra | C, R | = so_ptra của phiếu |
  | CT_PHIEUTRA | tinhtrang_tra | C, E, R | Thuộc {Bình thường, Hư hỏng, Mất} |
  | CT_PHIEUTRA | ghichu_tra | C, E, R | Không bắt buộc |
  | CT_PHIEUTRA | so_ngay_quahan | C, R | = ngaytra − hạn trả hiện hành (khi > 0) |
  | PHIEUMUON | so_pmuon, ma_bandoc | R | – |
  | CT_PHIEUMUON | ma_bansach, ngaymuon, hantra | R | – |
  | BANDOC | ma_bandoc, hoten | R | – |
  | SACH | ma_bansach, ten_sach, tinhtrang | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmLapPhieuTra (Load)` | Nếu mở form / Thì trạng thái 1: chỉ cho nhập so_pmuon và tìm Phiếu mượn sách (hoặc nhập so_ptra để mở phiếu đã lưu cần sửa); txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = True; dgvChiTiet, btnIn, btnHuy → Enable = False |  |
  | `txtSoPhieuMuon, btnTimPhieuMuon` | Lặp / Nếu nhập so_pmuon và click nút tìm / Thì nạp Bạn đọc và các bản sách chưa trả; tìm Phiếu trả đã lập của phiếu mượn, nếu có thì nạp ở chế độ bổ sung. Nếu tìm thấy và hợp lệ thì chuyển trạng thái 2: dgvChiTiet, btnHuy → Enable = True; txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = False / Cho đến khi đóng form | PHIEUMUON, CT_PHIEUMUON, PHIEUTRA, BANDOC |
  | `dgvChiTiet` | Lặp / Nếu chọn bản sách trả, nhập tinhtrang_tra, ghichu_tra / Thì tính so_ngay_quahan; đánh dấu trường hợp vi phạm (quá hạn, mất, hư hỏng); bản sách sai lệch thì thông báo. Khi lưới đã có ít nhất 1 dòng hợp lệ thì chuyển trạng thái 3: btnIn → Enable = True / Cho đến khi đóng form | CT_PHIEUTRA, SACH |
  | `btnIn` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập; nếu là Phiếu trả sách mới thì tự động sinh so_ptra, ngaylap_ptra; lưu PHIEUTRA, CT_PHIEUTRA; chuyển sách trả cho Bộ phận kho sách; chuyển trường hợp vi phạm cho Bộ phận xử lý vi phạm; hiển thị tiến trình lưu (%) và thông báo; in Phiếu trả sách; xóa trắng form, trở về trạng thái 1 để lập chứng từ tiếp theo; txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = True; dgvChiTiet, btnIn, btnHuy → Enable = False / Cho đến khi đóng form | PHIEUTRA, CT_PHIEUTRA |
  | `btnTimPhieu` | Lặp / Nếu nhập so_ptra và click nút này / Thì nạp Phiếu trả sách đã lưu ở chế độ sửa. Chuyển trạng thái 3: dgvChiTiet, btnIn, btnHuy → Enable = True; txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = False / Cho đến khi đóng form | PHIEUTRA, CT_PHIEUTRA |
  | `btnHuy` | Lặp / Nếu click nút này / Thì xóa trắng form, không cấp số phiếu; trở về trạng thái 1: txtSoPhieuMuon, btnTimPhieuMuon, btnTimPhieu → Enable = True; dgvChiTiet, btnIn, btnHuy → Enable = False / Cho đến khi đóng form |  |
  | `btnDong` | Lặp / Nếu click nút này / Thì trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B07 | PHIEUTRA | Thêm (insert bản ghi phiếu mới) | so_ptra, so_pmuon, ma_bandoc, ngaylap_ptra, ngaytra, ma_nd, ten_nd |
  | B07.1 | CT_PHIEUTRA | Thêm (insert) / Sửa (update) | ma_bansach, so_ptra, tinhtrang_tra, ghichu_tra, so_ngay_quahan |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.2.4 Lập Biên bản vi phạm mượn trả sách (02.03.01)
- **Người sử dụng:** Nhân viên Bộ phận xử lý vi phạm (N2). Sau khi đăng nhập, N2 vào thẳng màn hình này
- **Nhiệm vụ:** Lập Biên bản vi phạm cho trường hợp vi phạm do Bộ phận mượn trả chuyển sang; chọn khoản thu, tính thành tiền, tổng tiền; ghi nhận thanh toán; sửa biên bản đã lưu; in Biên bản vi phạm (MB05). Tiến trình gắn với giao dịch: chuyển kết quả xử lý vi phạm cho Bộ phận mượn trả sách; chuyển thông tin sách bị mất, hư hỏng cho Bộ phận kho sách để cập nhật tình trạng (03.03.02).
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N2 – Xử lý vi phạm; Sau khi đăng nhập, N2 vào thẳng màn hình này (hoặc chọn Menu Xử lý vi phạm → 02.03.01 Lập Biên bản vi phạm mượn trả sách); Có trường hợp vi phạm được chuyển từ Phiếu trả sách; danh mục KHOANTHU đã có dữ liệu; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi In biên bản (btnIn): hệ thống cấp so_bienban, thoigian_lap; chèn BIENBAN (ht_thanhtoan, tong_tien) và các dòng CT_BIENBAN (thanh_tien); thông tin sách bị mất, hư hỏng được chuyển cho Bộ phận kho sách (03.03.02); in Biên bản vi phạm (MB05).
  - Sau khi ghi nhận thanh toán: ht_thanhtoan trong BIENBAN được cập nhật; kết quả xử lý vi phạm được chuyển về Bộ phận mượn trả sách.
  - Sau khi sửa biên bản đã lưu: BIENBAN, CT_BIENBAN được cập nhật, tong_tien tính lại.
  - Sau khi Hủy: xóa trắng form, không cấp số biên bản.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | BIENBAN | so_bienban | R | – |
  | BIENBAN | so_pmuon | C, R | Phải tồn tại trong PHIEUMUON |
  | BIENBAN | ma_bandoc | C, R | Lấy từ PHIEUMUON.ma_bandoc |
  | BIENBAN | thoigian_lap | R | – |
  | BIENBAN | ht_thanhtoan | C, E, R | Thuộc {Tiền mặt, Chuyển khoản, Chưa thanh toán} |
  | BIENBAN | ma_nd, ten_nd | C, R | Lấy từ tài khoản đăng nhập |
  | BIENBAN | tong_tien | C, E, R | = SUM(thanh_tien) của biên bản |
  | CT_BIENBAN | ma_khoanthu | C, E, R | Phải tồn tại trong KHOANTHU |
  | CT_BIENBAN | so_bienban | C, R | = so_bienban của biên bản |
  | CT_BIENBAN | ma_bansach | C, R | Thuộc Phiếu mượn sách vi phạm |
  | CT_BIENBAN | soluong | C, E, R | N(5), > 0 |
  | CT_BIENBAN | thanh_tien | C, E, R | = soluong * KHOANTHU.dongia |
  | KHOANTHU | ma_khoanthu, noidung_tt, dongia | R | – |
  | PHIEUMUON | so_pmuon, ma_bandoc | R | – |
  | CT_PHIEUMUON | ma_bansach | R | – |
  | BANDOC | ma_bandoc, hoten, lop_donvi | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmLapBienBan (Load)` | Nếu mở form / Thì trạng thái 1: chỉ cho nhập so_pmuon của trường hợp vi phạm và tìm (hoặc nhập so_bienban để mở biên bản đã lưu cần sửa); txtSoPhieuMuon, btnTimPhieuMuon, btnTimBienBan → Enable = True; cboKhoanThu, btnThemCT, dgvChiTiet, cboHTThanhToan, btnIn, btnHuy → Enable = False |  |
  | `txtSoPhieuMuon, btnTimPhieuMuon` | Lặp / Nếu nhập so_pmuon của trường hợp vi phạm và click nút tìm / Thì nạp Bạn đọc và các bản sách vi phạm. Nếu tìm thấy và hợp lệ thì chuyển trạng thái 2: cboKhoanThu, btnThemCT, dgvChiTiet, cboHTThanhToan, btnHuy → Enable = True; txtSoPhieuMuon, btnTimPhieuMuon, btnTimBienBan → Enable = False / Cho đến khi đóng form | PHIEUMUON, CT_PHIEUMUON, BANDOC |
  | `cboKhoanThu` | Lặp / Nếu thay đổi khoản thu / Thì lấy noidung_tt, dongia / Cho đến khi đóng form | KHOANTHU |
  | `btnThemCT` | Lặp / Nếu click nút này / Thì kiểm tra ma_khoanthu, ma_bansach, soluong > 0; thêm dòng vào dgvChiTiet; tính thanh_tien = soluong * dongia; tính lại tong_tien. Khi lưới đã có ít nhất 1 dòng hợp lệ thì chuyển trạng thái 3: btnIn → Enable = True / Cho đến khi đóng form | CT_BIENBAN, KHOANTHU |
  | `dgvChiTiet` | Lặp / Nếu double click vào dòng i / Thì cho sửa soluong hoặc bỏ dòng i khỏi lưới (dòng chưa lưu); tính lại tong_tien. Nếu lưới không còn dòng nào thì btnIn → Enable = False / Cho đến khi đóng form | CT_BIENBAN |
  | `cboHTThanhToan` | Lặp / Nếu Bạn đọc nộp tiền / Thì chọn ht_thanhtoan (Tiền mặt, Chuyển khoản) / Cho đến khi đóng form | BIENBAN |
  | `btnIn` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập; nếu là Biên bản vi phạm mới thì tự động sinh so_bienban, thoigian_lap; lưu BIENBAN, CT_BIENBAN; chuyển kết quả xử lý về Bộ phận mượn trả sách; chuyển thông tin sách bị mất, hư hỏng cho Bộ phận kho sách; hiển thị tiến trình lưu (%) và thông báo; in Biên bản vi phạm; xóa trắng form, trở về trạng thái 1 để lập chứng từ tiếp theo; txtSoPhieuMuon, btnTimPhieuMuon, btnTimBienBan → Enable = True; cboKhoanThu, btnThemCT, dgvChiTiet, cboHTThanhToan, btnIn, btnHuy → Enable = False / Cho đến khi đóng form | BIENBAN, CT_BIENBAN |
  | `btnTimBienBan` | Lặp / Nếu nhập so_bienban và click nút này / Thì nạp Biên bản vi phạm đã lưu ở chế độ sửa. Chuyển trạng thái 3: cboKhoanThu, btnThemCT, dgvChiTiet, cboHTThanhToan, btnIn, btnHuy → Enable = True; txtSoPhieuMuon, btnTimPhieuMuon, btnTimBienBan → Enable = False / Cho đến khi đóng form | BIENBAN, CT_BIENBAN |
  | `btnHuy` | Lặp / Nếu click nút này / Thì xóa trắng form, không cấp số biên bản; trở về trạng thái 1: txtSoPhieuMuon, btnTimPhieuMuon, btnTimBienBan → Enable = True; cboKhoanThu, btnThemCT, dgvChiTiet, cboHTThanhToan, btnIn, btnHuy → Enable = False / Cho đến khi đóng form |  |
  | `btnDong` | Lặp / Nếu click nút này / Thì trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B08 | BIENBAN | Thêm (insert) / Sửa (update) | so_bienban, so_pmuon, ma_bandoc, thoigian_lap, ht_thanhtoan, ma_nd, ten_nd, tong_tien |
  | B08.1 | CT_BIENBAN | Thêm (insert) / Sửa (update) | ma_khoanthu, so_bienban, ma_bansach, soluong, thanh_tien |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.3 Giao diện quản trị hệ thống (N4) và Đăng nhập

### 6.3.1 Đăng nhập
- **Người sử dụng:** Mọi nhóm người dùng (N1 – Mượn trả sách, N2 – Xử lý vi phạm, N3 – Kho sách, N4 – Quản trị)
- **Nhiệm vụ:** Hộp thoại có nhập dữ liệu: xác thực tài khoản (tendangnhap, matkhau) trước khi sử dụng phần mềm; xác định nhóm người dùng để mở menu và màn hình tương ứng theo thiết kế kiểm soát (2.1, 2.2, 2.3).
- **Tiền điều kiện:** Phần mềm vừa khởi động hoặc người dùng vừa đăng xuất; Tài khoản đã được Quản trị tạo trong NGUOIDUNG với tinhtrang = “Hoạt động”; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi đăng nhập thành công: ghi lượt đăng nhập vào NHATKY_DANGNHAP; nạp quyền của nhóm theo PHANQUYEN; mở màn hình chính chỉ với menu của nhóm (N1 vào thẳng Lập Phiếu mượn sách, N2 vào thẳng Lập Biên bản vi phạm mượn trả sách, N3 vào Danh mục sách, N4 vào Quản lý tài khoản người dùng).
  - Sau khi đăng nhập sai: thông báo “Tên đăng nhập hoặc mật khẩu không đúng”; xóa trắng ô mật khẩu; sai 5 lần liên tiếp thì tài khoản bị khóa (tinhtrang = “Khóa”) và phải liên hệ Quản trị.
  - Tài khoản đang bị khóa: thông báo tài khoản bị khóa, không cho đăng nhập.
  - Sau khi Thoát: hỏi xác nhận rồi đóng phần mềm.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | NGUOIDUNG | tendangnhap | R | C(30), chữ cái + chữ số, không rỗng |
  | NGUOIDUNG | matkhau | R | C(64), không rỗng, hiển thị dạng ký tự ẩn, so sánh sau khi băm |
  | NGUOIDUNG | ma_nd, ten_nd, ma_nhom | R | – |
  | NGUOIDUNG | tinhtrang | E, R | Thuộc {Hoạt động, Khóa}; chuyển “Khóa” khi sai mật khẩu 5 lần liên tiếp |
  | PHANQUYEN | ma_nhom, ma_quyen | R | – |
  | NHATKY_DANGNHAP | ma_dangnhap | C, R | Tự sinh, không trùng |
  | NHATKY_DANGNHAP | ma_nd | C, R | = ma_nd của tài khoản vừa đăng nhập |
  | NHATKY_DANGNHAP | tg_dangnhap | C, R | = thời gian hiện tại, dd/mm/yyyy hh:mm |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmDangNhap (Load)` | Nếu mở form / Thì xóa trắng các ô, đặt con trỏ tại txtTenDangNhap; btnDangNhap → Enable = False; btnThoat → Enable = True |  |
  | `txtTenDangNhap, txtMatKhau` | Lặp / Nếu nhập đủ cả hai ô / Thì btnDangNhap → Enable = True (thiếu một ô thì Enable = False) / Cho đến khi đóng form |  |
  | `btnDangNhap` | Lặp / Nếu click nút này / Thì tìm tendangnhap trong NGUOIDUNG, so khớp matkhau (đã băm) và tinhtrang = “Hoạt động” / Nếu đúng / Thì sinh ma_dangnhap, ghi NHATKY_DANGNHAP (ma_nd, tg_dangnhap); nạp quyền của ma_nhom từ PHANQUYEN; đóng hộp thoại, mở màn hình chính theo nhóm / Nếu sai / Thì thông báo, xóa trắng txtMatKhau, tăng số lần sai; sai 5 lần liên tiếp thì cập nhật NGUOIDUNG.tinhtrang = “Khóa” / Cho đến khi đóng form | NGUOIDUNG, PHANQUYEN, NHATKY_DANGNHAP |
  | `btnThoat` | Nếu click nút này / Thì hỏi xác nhận rồi đóng phần mềm |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B13 | NHATKY_DANGNHAP | Thêm (insert toàn bộ bản ghi) | ma_dangnhap, ma_nd, tg_dangnhap (tg_dangxuat được cập nhật khi người dùng đăng xuất ở màn hình chính) |
  | B10 | NGUOIDUNG | Sửa (update) | tinhtrang = “Khóa” (chỉ khi sai mật khẩu 5 lần liên tiếp) |

### 6.3.2 Quản lý tài khoản người dùng (04.00.01)
- **Người sử dụng:** Quản trị (N4)
- **Nhiệm vụ:** Thực hiện tiến trình quản trị 04.00.01: xem danh sách tài khoản và các chức năng thêm, sửa, khóa hoặc mở khóa, đặt lại mật khẩu, tìm kiếm tài khoản; gán tài khoản vào nhóm người dùng (N1 – N4) để tài khoản nhận quyền của nhóm.
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N4 – Quản trị; Từ màn hình chính chọn Menu Quản trị → 04.00.01 Quản lý tài khoản người dùng (sau khi đăng nhập, N4 vào thẳng màn hình này); Danh mục nhóm người dùng (NHOMNGUOIDUNG) đã có dữ liệu N1 – N4; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi thêm tài khoản: hiển thị thông báo (thành công hoặc thất bại); tài khoản mới có tinhtrang mặc định “Hoạt động”, matkhau là mật khẩu mặc định đã băm; danh sách hiển thị tài khoản mới.
  - Sau khi sửa: hiển thị thông báo; danh sách cập nhật thông tin đã sửa (không sửa ma_nd, tendangnhap).
  - Sau khi khóa hoặc mở khóa: hỏi xác nhận; tinhtrang chuyển “Khóa” hoặc “Hoạt động”; tài khoản bị khóa không đăng nhập được.
  - Không có chức năng xóa: tài khoản đã lập chứng từ (ma_nd có trong PHIEUMUON, PHIEUGIAHAN, PHIEUTRA, BIENBAN) phải giữ lại để kiểm soát giao dịch, chỉ được khóa.
  - Sau khi đặt lại mật khẩu: hỏi xác nhận; matkhau được đặt về mật khẩu mặc định đã băm; hiển thị thông báo.
  - Sau khi tìm kiếm: danh sách chỉ hiển thị các tài khoản phù hợp từ khóa; không tìm thấy thì thông báo.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | NGUOIDUNG | ma_nd | C, R | C(10), chữ cái + chữ số, tự sinh, không trùng |
  | NGUOIDUNG | ten_nd | C, E, R | C(50), chữ cái, không rỗng |
  | NGUOIDUNG | tendangnhap | C, R | C(30), chữ cái + chữ số, không trùng, không rỗng |
  | NGUOIDUNG | matkhau | C, E | C(64), tối thiểu 8 ký tự, lưu dạng đã băm |
  | NGUOIDUNG | sdt | C, E, R | C(15), chữ số |
  | NGUOIDUNG | chucvu | C, E, R | C(50), chữ cái |
  | NGUOIDUNG | tinhtrang | C, E, R | Mặc định “Hoạt động”; thuộc {Hoạt động, Khóa} |
  | NGUOIDUNG | ma_nhom | C, E, R | Chọn từ NHOMNGUOIDUNG (phải tồn tại trong NHOMNGUOIDUNG) |
  | NHOMNGUOIDUNG | ma_nhom, ten_nhom | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmTaiKhoan (Load)` | Nếu mở form / Thì nạp NGUOIDUNG lên dgvNguoiDung, nạp NHOMNGUOIDUNG lên cboNhom; các ô nhập ReadOnly = True; btnThem, btnTimKiem → Enable = True; btnSua, btnKhoa, btnDatLaiMK, btnLuu, btnHuy → Enable = False | NGUOIDUNG, NHOMNGUOIDUNG |
  | `dgvNguoiDung` | Lặp / Nếu click vào dòng i của dgvNguoiDung / Thì đổ ma_nd, ten_nd, tendangnhap, sdt, chucvu, tinhtrang, nhóm của dòng i lên các ô (chỉ đọc); btnKhoa hiển thị “Khóa” hoặc “Mở khóa” theo tinhtrang. btnSua, btnKhoa, btnDatLaiMK → Enable = True / Cho đến khi đóng form | NGUOIDUNG, NHOMNGUOIDUNG |
  | `btnThem` | Lặp / Nếu click nút này / Thì xóa trắng các ô, tự sinh ma_nd, mở các ô nhập, tinhtrang mặc định “Hoạt động”. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnKhoa, btnDatLaiMK, btnTimKiem → Enable = False / Cho đến khi đóng form | NGUOIDUNG |
  | `btnSua` | Lặp / Nếu click nút này (đã chọn 1 dòng) / Thì mở các ô cho sửa, trừ ma_nd, tendangnhap. btnLuu, btnHuy → Enable = True; btnThem, btnSua, btnKhoa, btnDatLaiMK, btnTimKiem → Enable = False / Cho đến khi đóng form |  |
  | `btnKhoa` | Lặp / Nếu click nút này (đã chọn 1 dòng) / Thì hỏi xác nhận; đổi NGUOIDUNG.tinhtrang giữa “Hoạt động” và “Khóa”; ghi nhật ký; tải lại dgvNguoiDung. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form | NGUOIDUNG, NHATKY_HOATDONG |
  | `btnDatLaiMK` | Lặp / Nếu click nút này (đã chọn 1 dòng) / Thì hỏi xác nhận; cập nhật NGUOIDUNG.matkhau = mật khẩu mặc định (đã băm); ghi nhật ký; thông báo. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form | NGUOIDUNG, NHATKY_HOATDONG |
  | `btnLuu` | Lặp / Nếu click nút này / Thì kiểm soát dữ liệu nhập theo điều kiện toàn vẹn; thêm mới hoặc cập nhật NGUOIDUNG; ghi nhật ký; thông báo kết quả; tải lại dgvNguoiDung. Các ô nhập ReadOnly = True; các nút trở về trạng thái khi mở form / Cho đến khi đóng form | NGUOIDUNG, NHATKY_HOATDONG |
  | `btnHuy` | Lặp / Nếu click nút này / Thì bỏ thao tác đang làm, trả các ô về trạng thái chỉ đọc. Các nút trở về trạng thái khi mở form / Cho đến khi đóng form |  |
  | `txtTimKiem, btnTimKiem` | Lặp / Nếu nhập từ khóa và click nút tìm / Thì lọc NGUOIDUNG theo ma_nd, ten_nd, tendangnhap, nhóm, tinhtrang; hiển thị lên dgvNguoiDung / Cho đến khi đóng form | NGUOIDUNG, NHOMNGUOIDUNG |
  | `btnDong` | Lặp / Nếu click nút này / Thì đóng form, trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B10 | NGUOIDUNG | Thêm (insert toàn bộ bản ghi) | Bản ghi mới: ma_nd, ten_nd, tendangnhap, matkhau (mặc định, đã băm), sdt, chucvu, tinhtrang = “Hoạt động”, ma_nhom |
  | B10 | NGUOIDUNG | Sửa (update) | ten_nd, sdt, chucvu, ma_nhom; tinhtrang (khóa, mở khóa); matkhau (đặt lại mật khẩu) |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.3.3 Cấp quyền cho tài khoản (04.00.02)
- **Người sử dụng:** Quản trị (N4)
- **Nhiệm vụ:** Thực hiện tiến trình quản trị 04.00.02: cấp hoặc thu hồi quyền cho từng nhóm người dùng theo thiết kế kiểm soát (quyền C, E, R, D trên bảng dữ liệu ở 2.2 và quyền thực hiện tiến trình hệ thống ở 2.3). Tài khoản nhận quyền của nhóm mà tài khoản thuộc về (NGUOIDUNG.ma_nhom, gán ở 4.6.2).
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N4 – Quản trị; Từ màn hình chính chọn Menu Quản trị → 04.00.02 Cấp quyền cho tài khoản; NHOMNGUOIDUNG và danh mục quyền QUYEN đã có dữ liệu; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi chọn nhóm: danh sách quyền hiển thị, các quyền nhóm đang có được đánh dấu.
  - Sau khi lưu: hỏi xác nhận; quyền mới đánh dấu được thêm vào PHANQUYEN, quyền bỏ đánh dấu bị xóa khỏi PHANQUYEN; hiển thị thông báo; quyền mới có hiệu lực từ lần đăng nhập tiếp theo của tài khoản thuộc nhóm.
  - Không cho bỏ các quyền quản trị của nhóm N4, tránh hệ thống không còn tài khoản quản trị.
  - Sau khi hủy: nạp lại quyền hiện có của nhóm, không thay đổi PHANQUYEN.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | NHOMNGUOIDUNG | ma_nhom, ten_nhom, mota | R | – |
  | QUYEN | ma_quyen, ten_quyen, mota | R | – |
  | PHANQUYEN | ma_nhom | C, R, D | Chọn từ NHOMNGUOIDUNG |
  | PHANQUYEN | ma_quyen | C, R, D | Chọn từ QUYEN; cặp (ma_nhom, ma_quyen) không trùng |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmPhanQuyen (Load)` | Nếu mở form / Thì nạp NHOMNGUOIDUNG lên cboNhom, nạp QUYEN lên dgvQuyen (có cột chọn); btnLuu, btnHuy → Enable = False | NHOMNGUOIDUNG, QUYEN |
  | `cboNhom` | Lặp / Nếu thay đổi nhóm người dùng / Thì đánh dấu trên dgvQuyen các ma_quyen nhóm đang có trong PHANQUYEN; btnLuu, btnHuy → Enable = False / Cho đến khi đóng form | PHANQUYEN |
  | `dgvQuyen` | Lặp / Nếu đánh dấu hoặc bỏ đánh dấu quyền ở dòng i / Thì ghi nhận thay đổi tạm trên lưới (quyền quản trị của N4 không cho bỏ); btnLuu, btnHuy → Enable = True; cboNhom → Enable = False / Cho đến khi đóng form | QUYEN |
  | `btnLuu` | Lặp / Nếu click nút này / Thì hỏi xác nhận; thêm vào PHANQUYEN các cặp (ma_nhom, ma_quyen) mới đánh dấu, xóa khỏi PHANQUYEN các cặp bị bỏ đánh dấu; ghi nhật ký; thông báo. btnLuu, btnHuy → Enable = False; cboNhom → Enable = True / Cho đến khi đóng form | PHANQUYEN, NHATKY_HOATDONG |
  | `btnHuy` | Lặp / Nếu click nút này / Thì nạp lại quyền hiện có của nhóm. btnLuu, btnHuy → Enable = False; cboNhom → Enable = True / Cho đến khi đóng form | PHANQUYEN |
  | `btnDong` | Lặp / Nếu click nút này / Thì đóng form, trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B12 | PHANQUYEN | Thêm (insert toàn bộ bản ghi) | Cặp ma_nhom, ma_quyen mới được cấp |
  | B12 | PHANQUYEN | Xóa (delete bản ghi) | Cặp ma_nhom, ma_quyen bị thu hồi |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.3.4 Theo dõi nhật ký hệ thống (04.00.03, 04.00.08)
- **Người sử dụng:** Quản trị (N4)
- **Nhiệm vụ:** Thực hiện tiến trình quản trị 04.00.03 Theo dõi lịch sử đăng nhập (thẻ Lịch sử đăng nhập) và 04.00.08 Theo dõi nhật ký hoạt động (thẻ Nhật ký hoạt động): xem, lọc theo người dùng và khoảng thời gian để truy vết thao tác trên dữ liệu.
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N4 – Quản trị; Từ màn hình chính chọn Menu Quản trị → 04.00.03 Theo dõi lịch sử đăng nhập hoặc 04.00.08 Theo dõi nhật ký hoạt động (mở đúng thẻ tương ứng); Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi lọc: lưới chỉ hiển thị các dòng nhật ký của người dùng và khoảng thời gian đã chọn; hiển thị số lượt = COUNT(ma_dangnhap) hoặc COUNT(ma_hoatdong); không có dữ liệu thì thông báo.
  - Giao diện chỉ đọc: không có chức năng thêm, sửa, xóa nhật ký để bảo đảm truy vết.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | NHATKY_DANGNHAP | ma_dangnhap, ma_nd, tg_dangnhap, tg_dangxuat | R | – |
  | NHATKY_HOATDONG | ma_hoatdong, ma_nd, tg_thuchien, noidung | R | – |
  | NGUOIDUNG | ma_nd, ten_nd, tendangnhap | R | – |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmNhatKy (Load)` | Nếu mở form / Thì mở thẻ theo chức năng được chọn trên menu; dtpTuNgay = ngày đầu tháng, dtpDenNgay = ngày hiện tại (chọn ngày, không nhập tay); cboNguoiDung = “Tất cả”; nạp nhật ký trong khoảng ngày lên lưới của thẻ | NHATKY_DANGNHAP, NHATKY_HOATDONG, NGUOIDUNG |
  | `tabNhatKy` | Lặp / Nếu chuyển thẻ (Lịch sử đăng nhập, Nhật ký hoạt động) / Thì nạp lại lưới của thẻ theo điều kiện lọc hiện tại / Cho đến khi đóng form | NHATKY_DANGNHAP, NHATKY_HOATDONG |
  | `cboNguoiDung, dtpTuNgay, dtpDenNgay, btnLoc` | Lặp / Nếu chọn điều kiện và click nút lọc / Thì kiểm tra dtpTuNgay ≤ dtpDenNgay; lọc nhật ký theo ma_nd và khoảng thời gian; hiển thị lên lưới kèm ten_nd và số lượt / Cho đến khi đóng form | NHATKY_DANGNHAP, NHATKY_HOATDONG, NGUOIDUNG |
  | `btnDong` | Lặp / Nếu click nút này / Thì đóng form, trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | – | – | Không thay đổi | Giao diện chỉ đọc NHATKY_DANGNHAP, NHATKY_HOATDONG |

### 6.3.5 Sao lưu và phục hồi dữ liệu (04.00.09)
- **Người sử dụng:** Quản trị (N4)
- **Nhiệm vụ:** Thực hiện tiến trình quản trị 04.00.09 (hộp thoại có nhập dữ liệu): sao lưu toàn bộ CSDL ra tệp sao lưu theo định kỳ hoặc khi cần; phục hồi CSDL từ tệp sao lưu khi dữ liệu bị lỗi, mất.
- **Tiền điều kiện:** Đã đăng nhập với tài khoản thuộc nhóm N4 – Quản trị; Từ màn hình chính chọn Menu Quản trị → 04.00.09 Sao lưu và phục hồi dữ liệu; Khi phục hồi: các tài khoản khác đã đăng xuất; Phần mềm đã được kết nối với CSDL
- **Hậu điều kiện:**
  - Sau khi sao lưu: tạo tệp sao lưu toàn bộ CSDL tại thư mục đã chọn (tên tệp kèm ngày giờ sao lưu); ghi nhật ký; thông báo thành công hoặc thất bại.
  - Trước khi phục hồi: hiển thị thông báo xác nhận vì thao tác ghi đè toàn bộ dữ liệu; hệ thống tự sao lưu dữ liệu hiện tại để có thể quay lại nếu chọn nhầm tệp.
  - Sau khi phục hồi: toàn bộ các bảng dữ liệu trở về thời điểm của tệp sao lưu; ghi nhật ký; thông báo và yêu cầu đăng nhập lại.
  - Sau khi đóng: trở về menu chính.
- **Bảng dữ liệu sử dụng:**
  | Bảng | Thuộc tính | Mức độ | Điều kiện toàn vẹn |
  |---|---|---|---|
  | NHATKY_HOATDONG | ma_hoatdong | C, R | Tự sinh, không trùng |
  | NHATKY_HOATDONG | ma_nd | C, R | = ma_nd của tài khoản đang đăng nhập |
  | NHATKY_HOATDONG | tg_thuchien | C, R | = thời gian hiện tại, dd/mm/yyyy hh:mm |
  | NHATKY_HOATDONG | noidung | C, R | “Sao lưu …” hoặc “Phục hồi …” kèm tên tệp |
- **Control và xử lý (lập trình nút lệnh, kèm trạng thái nút):**
  | Control | Xử lý | Bảng |
  |---|---|---|
  | `frmSaoLuu (Load)` | Nếu mở form / Thì chọn mặc định rdoSaoLuu; txtDuongDan ReadOnly = True (chỉ chọn qua btnChon); btnThucHien → Enable = False |  |
  | `rdoSaoLuu, rdoPhucHoi` | Lặp / Nếu thay đổi lựa chọn / Thì xóa txtDuongDan; btnChon đổi thành chọn thư mục (sao lưu) hoặc chọn tệp sao lưu (phục hồi); btnThucHien → Enable = False / Cho đến khi đóng form |  |
  | `btnChon` | Lặp / Nếu click nút này / Thì mở hộp thoại chọn thư mục hoặc tệp; hiển thị đường dẫn lên txtDuongDan. btnThucHien → Enable = True / Cho đến khi đóng form |  |
  | `btnThucHien` | Lặp / Nếu click nút này / Thì nếu sao lưu: tạo tệp sao lưu toàn bộ CSDL; nếu phục hồi: hỏi xác nhận, tự sao lưu dữ liệu hiện tại, phục hồi CSDL từ tệp đã chọn; ghi nhật ký; hiển thị tiến trình (%) và thông báo. btnThucHien → Enable = False / Cho đến khi đóng form | Toàn bộ CSDL, NHATKY_HOATDONG |
  | `btnDong` | Lặp / Nếu click nút này / Thì đóng form, trở về menu chính / Cho đến khi đóng form |  |
- **Kết quả đầu ra (bảng thay đổi):**
  | Mã bảng | Bảng | Thao tác | Trường |
  |---|---|---|---|
  | B01 – B14 | Toàn bộ bảng dữ liệu | Thay thế (chỉ khi phục hồi) | Dữ liệu tất cả các bảng được thay bằng dữ liệu trong tệp sao lưu; sao lưu không thay đổi bảng nào (chỉ tạo tệp) |
  | B14 | NHATKY_HOATDONG | Thêm (insert toàn bộ bản ghi) | ma_hoatdong, ma_nd, tg_thuchien, noidung |

### 6.4 Mẫu in chứng từ và báo cáo
Mỗi mẫu in có tiêu đề “THƯ VIỆN TRƯỜNG ĐẠI HỌC …” (để trống tên trường), tên mẫu, số chứng từ, ngày lập, khối thông tin, bảng chi tiết, dòng tổng, chữ ký các bên.

#### 6.4.1 Phiếu mượn sách (MB02)
- Người lập: Nhân viên Bộ phận mượn trả sách (N1); Nơi nhận: Bạn đọc; Khổ giấy: Khổ giấy A5; Số lượng: 02 bản in (01 bản giao Bạn đọc, 01 bản lưu)
- Nhiệm vụ: Xác nhận các bản sách Bạn đọc đã mượn và hạn trả; làm căn cứ khi gia hạn, trả sách và xử lý vi phạm.
- Dữ liệu: PHIEUMUON(so_pmuon, ngaylap_pmuon, ma_bandoc, hoten, ten_nd); BANDOC(doituong, ma_svhvcb, lop_donvi, so_thebd); CT_PHIEUMUON(ma_bansach, ten_sach, ngaymuon, hantra, tinhtrang_muon, ghichu_muon)
- Công thức: hantra = ngaymuon + số ngày được mượn theo doituong; Tổng số bản sách mượn = COUNT(CT_PHIEUMUON.ma_bansach) của phiếu

#### 6.4.2 Phiếu gia hạn sách (MB03)
- Người lập: Nhân viên Bộ phận mượn trả sách (N1); Nơi nhận: Bạn đọc; Khổ giấy: Khổ giấy A5; Số lượng: 02 bản in
- Nhiệm vụ: Xác nhận kết quả gia hạn từng bản sách và hạn trả mới.
- Dữ liệu: PHIEUGIAHAN(so_pgiahan, ngaylap_pgiahan, so_pmuon, ma_bandoc, ngay_denghi, ten_nd); BANDOC(hoten, ma_svhvcb); CT_PHIEUGIAHAN(ma_bansach, ten_sach, hantra_cu, ketqua_giahan, hantra_moi, lydo_khonggh, so_lan_giahan)
- Công thức: hantra_moi = hantra_cu + số ngày được gia hạn; so_lan_giahan = COUNT(lần gia hạn được đồng ý của bản sách)

#### 6.4.3 Phiếu trả sách (MB04)
- Người lập: Nhân viên Bộ phận mượn trả sách (N1); Nơi nhận: Bạn đọc; Khổ giấy: Khổ giấy A5; Số lượng: 02 bản in
- Nhiệm vụ: Xác nhận các bản sách Bạn đọc đã trả, tình trạng khi trả và số ngày quá hạn.
- Dữ liệu: PHIEUTRA(so_ptra, ngaylap_ptra, so_pmuon, ma_bandoc, ngaytra, ten_nd); BANDOC(hoten, ma_svhvcb); CT_PHIEUTRA(ma_bansach, tinhtrang_tra, ghichu_tra, so_ngay_quahan); CT_PHIEUMUON(ten_sach, hantra)
- Công thức: so_ngay_quahan = ngaytra − hạn trả hiện hành (khi > 0)

#### 6.4.4 Biên bản vi phạm mượn trả sách (MB05)
- Người lập: Nhân viên Bộ phận xử lý vi phạm (N2); Nơi nhận: Bạn đọc; Khổ giấy: Khổ giấy A4; Số lượng: 02 bản in (01 bản giao Bạn đọc, 01 bản lưu)
- Nhiệm vụ: Ghi nhận vi phạm (quá hạn, mất, hư hỏng sách), các khoản thu và số tiền Bạn đọc phải nộp; làm căn cứ thu tiền và báo cáo vi phạm.
- Dữ liệu: BIENBAN(so_bienban, thoigian_lap, so_pmuon, ma_bandoc, ht_thanhtoan, tong_tien, ten_nd); BANDOC(hoten, lop_donvi); CT_BIENBAN(ma_bansach, ma_khoanthu, soluong, thanh_tien); KHOANTHU(noidung_tt, dongia)
- Công thức: thanh_tien = soluong * dongia; tong_tien = SUM(thanh_tien) của biên bản

#### 6.4.5 Báo cáo tình hình mượn trả sách (MB06)
- Người lập: Nhân viên Bộ phận mượn trả sách (N1); Nơi nhận: Ban giám đốc thư viện; Khổ giấy: Khổ giấy A4; Số lượng: 01 bản in
- Nhiệm vụ: - Tổng hợp số lượt mượn, gia hạn, trả sách trong kỳ báo cáo. / - Thống kê số sách đang mượn, số sách quá hạn. / - Liệt kê chi tiết các lượt mượn, gia hạn, trả để Ban giám đốc theo dõi.
- Dữ liệu: PHIEUMUON(so_pmuon, ma_bandoc, hoten); CT_PHIEUMUON(ma_bansach, ten_sach, ngaymuon, hantra); PHIEUGIAHAN(so_pgiahan, ngaylap_pgiahan); CT_PHIEUGIAHAN(ma_bansach, ten_sach, ketqua_giahan, hantra_moi); PHIEUTRA(so_ptra, ngaytra); CT_PHIEUTRA(ma_bansach, tinhtrang_tra, so_ngay_quahan)
- Công thức: Tổng số lượt mượn = COUNT(CT_PHIEUMUON.ma_bansach) có ngaymuon trong kỳ; Tổng số lượt gia hạn = COUNT(CT_PHIEUGIAHAN.ma_bansach) có ketqua_giahan = “Đồng ý” trong kỳ; Tổng số lượt trả = COUNT(CT_PHIEUTRA.ma_bansach) có ngaytra trong kỳ; Số sách quá hạn = COUNT(sách chưa trả có hantra < ngày cuối kỳ báo cáo)

#### 6.4.6 Báo cáo tình hình vi phạm mượn trả sách (MB07)
- Người lập: Nhân viên Bộ phận xử lý vi phạm (N2); Nơi nhận: Ban giám đốc thư viện; Khổ giấy: Khổ giấy A4; Số lượng: 01 bản in
- Nhiệm vụ: - Tổng hợp số trường hợp vi phạm theo từng nội dung (khoản thu) trong kỳ báo cáo. / - Tổng hợp số tiền thu được. / - Liệt kê chi tiết Bạn đọc vi phạm và tình trạng thanh toán.
- Dữ liệu: BIENBAN(so_bienban, thoigian_lap, ma_bandoc, ht_thanhtoan, tong_tien); CT_BIENBAN(ma_bansach, ma_khoanthu, soluong, thanh_tien); KHOANTHU(noidung_tt, dongia); BANDOC(hoten, lop_donvi)
- Công thức: Số trường hợp vi phạm = COUNT(CT_BIENBAN.ma_bansach) theo ma_khoanthu; Tổng tiền theo khoản thu = SUM(thanh_tien) theo ma_khoanthu; Tổng tiền thu trong kỳ = SUM(tong_tien) của các biên bản đã thanh toán

Báo cáo MB06/MB07: có bộ lọc kỳ báo cáo (chọn ngày, không gõ tay), nút Tổng hợp (01.04.02 / 02.05.01) rồi nút Lập báo cáo (01.04.03 / 02.05.02) mở trang in; có bảng tổng hợp và biểu đồ theo `09_BIEU_DO_THONG_KE.md`.

## 7. Dữ liệu mẫu cho môi trường thử nghiệm (chỉ nạp khi chạy môi trường Development)
- 4 nhóm N1–N4 và 4 tài khoản nhân viên: `muontra` (N1), `vipham` (N2), `khosach` (N3), `admin` (N4), mật khẩu `Thuvien@123` (chỉ Development).
- 4 khu vực, 40 bản sách (đủ các tình trạng), 15 Bạn đọc (đủ 3 đối tượng, có 2 thẻ hết hạn), 5 khoản thu (phạt quá hạn/ngày, bồi thường mất sách, hư hỏng nhẹ, hư hỏng nặng, làm lại thẻ).
- Lịch sử 2 tháng: ~25 phiếu mượn, ~8 phiếu gia hạn, ~18 phiếu trả, ~5 biên bản (có quá hạn, mất, hư hỏng) để báo cáo MB06/MB07 có số liệu.
- Tên người/sách là dữ liệu giả, ghi rõ trong seed là dữ liệu demo.

