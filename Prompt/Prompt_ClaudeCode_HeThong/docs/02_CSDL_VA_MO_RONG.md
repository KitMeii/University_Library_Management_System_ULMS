# 02 – CƠ SỞ DỮ LIỆU: 18 BẢNG GỐC VÀ PHẦN MỞ RỘNG

## 1. 18 bảng gốc
Tạo đúng 18 bảng ở `SPEC_NGHIEP_VU_BT4.md` mục 2: **tên bảng, tên cột viết đúng như SPEC** (chữ hoa tên bảng, chữ thường có gạch dưới tên cột), đúng khóa chính, khóa ngoại.

- Ánh xạ kiểu: `C(n)` → `nvarchar(n)`; `N` → `int`; tiền (`dongia`, `thanh_tien`, `tong_tien`) → `decimal(18,0)`; `D` → `date`; thời điểm (`tg_*`, `thoigian_lap`) → `datetime2(0)`.
- Ràng buộc miền giá trị (SPEC mục 2, đoạn “Miền giá trị”) tạo thành `CHECK` constraint.
- Khóa chính dạng chuỗi do hệ thống sinh: dùng bảng phụ trợ sinh số an toàn đồng thời (xem mục 4) hoặc `SEQUENCE` của SQL Server, định dạng `PMyyMM####`, `GHyyMM####`, `PTyyMM####`, `BByyMM####` (10 ký tự, đánh lại mỗi tháng – SPEC mục 4, GIA_DINH D4).
- Tạo chỉ mục cho mọi khóa ngoại và các cột tìm kiếm (`ten_sach`, `tacgia`, `ma_svhvcb`, `hoten`, `ngaymuon`, `hantra`, `ngaytra`, `thoigian_lap`).
  - *(Pha 8 – hiệu năng, GIA_DINH D82)* Chỉ mục duy nhất `IX_PHIEUTRA_so_pmuon` kèm cột `INCLUDE (ngaytra)` (migration `ChiMucPhieuTraNgayTra`) – chỉ đổi chỉ mục vật lý, không đổi cột/bảng: truy vấn “bản sách chưa trả tính đến ngày” (MB06, tổng quan N1) không phải tra khóa từng dòng.
- Kiểm thử tự động `SchemaMatchesSpecTests`: đọc metadata SQL Server, so với danh sách bảng/cột trích từ SPEC mục 2 → phải khớp 100%.

## 2. 9 bảng mở rộng X01–X09 (chỉ có trong hệ thống, KHÔNG đưa vào hồ sơ Word BT1–BT4)
Đặt tiền tố mã bảng `X` trong tài liệu nội bộ. Không sửa bất kỳ cột nào của 18 bảng gốc.

### X01. THELOAI – thể loại sách (dùng cho danh mục ở cổng bạn đọc)
| Cột | Kiểu | Ràng buộc |
|---|---|---|
| **ma_theloai** | nvarchar(10) | PK |
| ten_theloai | nvarchar(100) | không rỗng, duy nhất |
| mota | nvarchar(500) | |
| bieutuong | nvarchar(50) | tên Bootstrap Icon |
| thutu_hienthi | int | mặc định 0 |

### X02. DAUSACH – đầu sách (nhóm các bản sách cùng tên + tác giả để hiển thị ở cổng)
| Cột | Kiểu | Ràng buộc |
|---|---|---|
| **ma_dausach** | nvarchar(10) | PK |
| ten_sach | nvarchar(200) | không rỗng |
| tacgia | nvarchar(100) | |
| nhaxuatban | nvarchar(150) | |
| namxb | int | 1900 ≤ namxb ≤ năm hiện tại |
| isbn | nvarchar(20) | duy nhất nếu có |
| ma_theloai | nvarchar(10) | FK → THELOAI |
| gioithieu | nvarchar(2000) | |
| tukhoa | nvarchar(300) | các từ khóa cách nhau bởi dấu phẩy |
| anhbia | nvarchar(260) | đường dẫn ảnh bìa (WebP 600×900 + bản nhỏ 300×450) |
| mau_gay | char(7) | màu gáy sách `#RRGGBB` (dùng cho kệ coverflow; nếu trống thì lấy màu chủ đạo ảnh bìa) |
| ngaytao | datetime2(0) | |

### X03. DAUSACH_BANSACH – gắn bản sách (SACH) vào đầu sách
| Cột | Kiểu | Ràng buộc |
|---|---|---|
| **ma_bansach** | nvarchar(10) | PK, FK → SACH |
| ma_dausach | nvarchar(10) | FK → DAUSACH |

Khởi tạo: migration dữ liệu nhóm các bản `SACH` theo (`ten_sach`, `tacgia`) để tạo `DAUSACH` và `DAUSACH_BANSACH`. Khi N3/N4 thêm bản sách mới ở Danh mục sách, form có thêm ô chọn đầu sách (đánh dấu “mở rộng”).

### X04. TAIKHOAN_BANDOC – tài khoản đăng nhập cổng của bạn đọc
| Cột | Kiểu | Ràng buộc |
|---|---|---|
| **ma_bandoc** | nvarchar(10) | PK, FK → BANDOC |
| matkhau | nvarchar(200) | băm bằng hasher PBKDF2-SHA256 chung của hệ thống (GIA_DINH D5; chuỗi 64 ký tự) |
| tinhtrang | nvarchar(20) | ∈ {Chưa kích hoạt, Hoạt động, Khóa} |
| so_lan_sai | int | mặc định 0 |
| tg_khoa_den | datetime2(0) | null |
| tg_dangnhap_cuoi | datetime2(0) | null |
| tg_tao | datetime2(0) | |

Bạn đọc đăng nhập bằng `BANDOC.ma_svhvcb`. Tài khoản tự tạo ở trạng thái “Chưa kích hoạt” khi nhân viên lưu thẻ bạn đọc (01.01.01). Kích hoạt xem file 07.

### X05. PHIEUDATCHO – đặt chỗ (giữ sách)
| Cột | Kiểu | Ràng buộc |
|---|---|---|
| **ma_datcho** | nvarchar(20) | PK, dạng `DCyyyyMMdd-####` |
| ma_bandoc | nvarchar(10) | FK → BANDOC |
| ma_dausach | nvarchar(10) | FK → DAUSACH |
| ma_bansach | nvarchar(10) | FK → SACH – bản sách cụ thể được giữ |
| tg_dat | datetime2(0) | thời điểm đặt |
| tg_hethan | datetime2(0) | = tg_dat + 24 giờ |
| trangthai | nvarchar(20) | ∈ {Đang giữ, Đã nhận, Đã hủy, Hết hạn} |
| tg_xuly | datetime2(0) | null – thời điểm nhận/hủy/hết hạn |
| ma_nd_xuly | nvarchar(10) | FK → NGUOIDUNG, null (nhân viên giao sách) |
| so_pmuon | nvarchar(20) | FK → PHIEUMUON, null (phiếu mượn khi nhận) |
| lydo_huy | nvarchar(200) | null |

Chỉ mục duy nhất có điều kiện:
- `UX_DATCHO_BANSACH_DANGGIU` trên (`ma_bansach`) WHERE `trangthai = N'Đang giữ'` → một bản sách chỉ được giữ cho một người.
- `UX_DATCHO_BANDOC_DAUSACH_DANGGIU` trên (`ma_bandoc`, `ma_dausach`) WHERE `trangthai = N'Đang giữ'` → mỗi bạn đọc giữ tối đa 1 bản của một đầu sách.

### X06. YEUTHICH – sách yêu thích của bạn đọc
(`ma_bandoc` FK, `ma_dausach` FK, `tg_them`) – PK kép (`ma_bandoc`, `ma_dausach`).

### X07. THONGBAO – thông báo cho bạn đọc
| Cột | Kiểu | Ràng buộc |
|---|---|---|
| **ma_thongbao** | bigint identity | PK |
| ma_bandoc | nvarchar(10) | FK → BANDOC |
| loai | nvarchar(30) | ∈ {Đặt chỗ thành công, Sắp hết hạn giữ, Hết hạn giữ, Đã nhận sách, Sắp đến hạn trả, Quá hạn trả, Biên bản vi phạm, Đặt chỗ bị hủy} – “Đặt chỗ bị hủy” bổ sung 17/11/2026 (GIA_DINH D63): khi nhân viên hủy đặt chỗ tại quầy |
| noidung | nvarchar(500) | |
| lienket | nvarchar(260) | null |
| tg_tao | datetime2(0) | |
| da_doc | bit | mặc định 0 |

### X08. SOCHUNGTU – bộ đếm số chứng từ theo tháng
(`loai` nvarchar(4), `ngay` date, `so_cuoi` int) – PK kép (`loai`, `ngay`). **`ngay` luôn là ngày đầu tháng** (đánh số lại mỗi tháng – GIA_DINH D4). Cấp số trong cùng transaction bằng `UPDATE … WITH (UPDLOCK, HOLDLOCK)`. Loại `DC` (đặt chỗ) vẫn đếm theo ngày, `ngay` = ngày đặt.

### X09. XULY_CHUYENDEN – Bộ phận kho sách đã xử lý thông tin chuyển đến (bổ sung 16/11/2026 – GIA_DINH D47)
Chỉ có trong hệ thống, **không đưa vào hồ sơ Word**. Ghi lại việc N3 đã xử lý một dòng “chuyển đến” của `dgvChuyenDen` (SPEC 6.1.1 “thông tin chuyển đến được đánh dấu đã xử lý”). Với 03.03.02: bản sách trả Mất/Hư hỏng chỉ bị chặn cho mượn tới khi có dòng này; sau đó N3 sửa sách xong đặt “Sẵn sàng” thì mượn lại bình thường.

| Cột | Kiểu | Ràng buộc |
|---|---|---|
| **ma_bansach** | nvarchar(10) | PK, FK → SACH |
| **so_chungtu** | nvarchar(10) | PK – với 03.03.02 là `so_ptra` của dòng CT_PHIEUTRA trả Mất/Hư hỏng |
| **tientrinh** | nvarchar(10) | PK – mã tiến trình (`03.03.02`) |
| tinhtrang_moi | nvarchar(30) | ∈ {Sẵn sàng, Đang được mượn, Mất, Hư hỏng} |
| ma_nd | nvarchar(10) | FK → NGUOIDUNG (nhân viên N3 xử lý) |
| thoigian_xuly | datetime2(0) | |

Ghi khi N3 bấm `btnCapNhatTT` trên dòng 03.03.02 hoặc đổi tình trạng bản sách bằng `btnSua`. Tổng cộng **9 bảng mở rộng** (X01–X09).

## 3. Dữ liệu khởi tạo (mọi môi trường)
- `NHOMNGUOIDUNG`: N1–N4 đúng tên SPEC 3.1.
- `QUYEN` + `PHANQUYEN`: nạp từ SPEC 3.2 (mã `SACH_C`, `SACH_E`, `SACH_R`, `SACH_D`…) và SPEC 3.4 (mã `TT001`…`TT122` theo STT). Thêm quyền mở rộng: `DATCHO_R` (xem đặt chỗ), `DATCHO_GIAO` (giao sách đặt chỗ) cho N1; `DAUSACH_C/E/R/D`, `THELOAI_C/E/R/D` cho N3 (C, E, R) và N4 (C, E, R, D).
- Một tài khoản quản trị N4 `admin`, mật khẩu đọc từ biến môi trường lúc khởi tạo, bắt đổi ở lần đăng nhập đầu.

## 4. Dữ liệu mẫu (chỉ Development)
Theo SPEC mục 7, cộng thêm: 8 thể loại; 60 đầu sách (đủ năm xuất bản 1990–2026 để thanh thời gian ở kệ coverflow có điểm rải đều), mỗi đầu sách 1–4 bản; ảnh bìa tạm tự sinh dạng SVG → WebP (nền màu `mau_gay`, tên sách, tác giả; góc ghi “ẢNH TẠM”); 10 tài khoản bạn đọc đã kích hoạt (mật khẩu `Docgia@123`); đặt chỗ đủ 4 trạng thái, trong đó 2 cái còn dưới 2 giờ.

## 5. Giao dịch và đồng thời
- Trên basecore microservice: “một transaction” dưới đây hiểu là **trong service sở hữu dữ liệu**; phần thuộc service khác đi qua sự kiện + outbox theo file 12 mục 5.
- Mỗi nghiệp vụ ghi (lập phiếu, đặt chỗ, nhận đặt chỗ, hủy, lập biên bản) chạy trong **một transaction**: chứng từ + chi tiết + cập nhật `SACH.tinhtrang` + `PHIEUDATCHO` + `NHATKY_HOATDONG` + `THONGBAO`.
- Không thêm cột `rowversion` vào bảng gốc (không được sửa 18 bảng) → kiểm soát xung đột bằng khóa `UPDLOCK` khi đọc bản ghi sẽ sửa và bằng chỉ mục duy nhất có điều kiện ở bảng mở rộng.
