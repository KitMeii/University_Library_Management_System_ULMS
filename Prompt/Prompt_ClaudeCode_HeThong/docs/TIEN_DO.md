# TIẾN ĐỘ DỰ ÁN ULMS

| Pha | Trạng thái | Ghi chú |
|---|---|---|
| 0A – Khảo sát basecore | **Đã duyệt** (15/11/2026) | `BASECORE_PHAN_TICH.md` |
| 0 – Kế hoạch | **Đã duyệt** (15/11/2026) | `KE_HOACH_PHA_0.md`; quyết định D1–D24 |
| 1 – Nền tảng | **Đã duyệt** (16/11/2026) | D25–D33 đã duyệt |
| 2 – Danh mục + quản trị | **Đã duyệt** (16/11/2026) | D34–D43 đã duyệt (D35, D36, D41 theo quyết định chủ dự án); D44 SQL Server cài sẵn; ảnh 4.2.1–4.2.4, 4.6.1–4.6.5 |
| 3 – Chứng từ + in MB02–MB05 | **Đang làm** | Kèm nạp lịch sử phiếu mẫu |
| 4 – Báo cáo MB06–MB07 + tổng quan | Chưa làm | Xong pha này = đủ bộ ảnh Bài 4 |
| 5 – Cổng: header, hero, coverflow + đầu sách/thể loại | Chưa làm | |
| 6 – Book Preview + đặt chỗ + tài khoản + Quầy đặt chỗ | Chưa làm | |
| 7 – Trang chủ hoàn chỉnh | Chưa làm | |
| 8 – Hoàn thiện, prod.yml | Chưa làm | |

## Pha 1 – đã làm
- Git: init, xóa bí mật, commit gốc `a3c44a2` + push; commit xóa riêng `3c0956d` (chưa push – push khi Pha 1 được duyệt).
- Làm phẳng `Backend/` (git mv giữ lịch sử), đổi tên QuanTriService/KhoSachService, thêm MuonTraService, ViPhamService, Migrator; .NET 10; `ULMS.slnx`; quản lý phiên bản gói tập trung.
- `BaseCore.Common`: JWT, hasher D5, IClock, `[YeuCauQuyen]`, lỗi `{ma,thongBao,truong}`, health, Serilog, rate limit.
- CSDL: 26 bảng (18 SPEC + 8 mở rộng) đúng tên/kiểu, CHECK, FK giữa schema, chỉ mục lọc đặt chỗ, SEQUENCE nhật ký, thủ tục cấp số chứng từ; login SQL từng service.
- Dữ liệu khởi tạo (N1–N4, 173 quyền sinh từ SPEC, admin) + dữ liệu mẫu Development (4 NV, 4 khu vực, 8 thể loại, 60 đầu sách, 112 bản sách, 15 bạn đọc + tài khoản cổng, 5 khoản thu).
- Đăng nhập nhân viên SPEC 6.3.1 đầy đủ; đăng xuất; đổi mật khẩu; đăng nhập bạn đọc; gateway Ocelot; web BFF YARP; 2 layout; token CSS; motion.config.js.
- Docker: 7 Dockerfile, compose + override; kiểm thử: unit, tích hợp, E2E/ảnh.

## Pha 2 – đã làm
- 6.1.1 Danh mục sách (+ 03.01.02 tìm loại sách đang mượn, 03.04.01 thống kê tình trạng, dgvChuyenDen + btnCapNhatTT cho 03.01.04/03.02.03/03.03.02).
- 6.1.2 Khoản thu, 6.1.3 Bạn đọc (01.01.01: txtMaSV tìm/mở thêm, btnKiemTraDK, tạo TAIKHOAN_BANDOC, ảnh thẻ), 6.1.4 Khu vực.
- 6.3.2 Tài khoản (khóa/mở khóa, đặt lại MK), 6.3.3 Cấp quyền, 6.3.4 Nhật ký 2 thẻ, 6.3.5 Sao lưu – phục hồi (tiến trình % thật, sao lưu tự động 23:00).
- Khung frontend dùng chung: api.js, form-state.js, luoi.js (DataGrid), danh-muc.js, hop-thoai.js (modal/toast), chon-ngay.js (DatePicker dd/MM/yyyy).
- Sửa lỗi phát hiện khi kiểm thử: AntiForgery theo scheme NhanVien; DTO record không dịch được ORDER BY; BACKUP thất bại bị báo thành công (cờ FireInfoMessageEventOnUserErrors) + quyền ghi volume sao lưu; tràn ngang trên điện thoại.

## Pha 2 – sửa theo quyết định duyệt (16/11/2026)
- D36: sau phục hồi hủy ngay mọi phiên khác (mốc thu hồi = extended property `ULMS_MocThuHoiPhien`; service từ chối JWT cũ – 401 `PHIEN_DA_BI_HUY`; web kiểm tra cookie 15 giây/lần, BFF xóa cookie khi backend trả 401, trang đăng nhập báo lý do; đóng NHATKY_DANGNHAP đang mở; ghi nhật ký). Giữ phiên người phục hồi. Phục hồi chỉ nhận .bak của CSDL ThuVien (RESTORE HEADERONLY).
- D35: ảnh thẻ → WebP 600×800 bằng SkiaSharp 3.119.2 (xoay EXIF, bỏ siêu dữ liệu, chặn ảnh > 40 MP).
- D41: chặn xóa bạn đọc đã có phiếu mượn/gia hạn/trả/biên bản (409 `DA_CO_CHUNG_TU` nêu rõ loại).
- D44: chạy trên SQL Server cài sẵn (`TunKittt`, `DB_SERVER=host.docker.internal,1433`, profile `sql-docker` tắt); thư mục sao lưu theo `InstanceDefaultBackupPath` (đường dẫn Windows); `Database/01–03*.sql` sinh từ migrator (`scripts/xuat-sql.sh`), đã chạy thử 2 lần trên SQL Server trống.
- Kiểm thử mới: `MocThuHoiPhienTests` (unit), `ThuHoiPhienTests`, `BanDoc_DaCoChungTu_KhongXoaDuoc`, `PhucHoi_TuChoiTepCuaCsdlKhac` (tích hợp), `BanDoc_AnhThe_ChuyenWebP_ThuNho` (E2E). Đã chạy thử chu trình sao lưu → phục hồi thật trên SQL Server của máy.

## Việc còn lại / chuyển pha sau
- Lịch sử phiếu mượn/gia hạn/trả/biên bản mẫu → Pha 3; đặt chỗ mẫu → Pha 6.
- `docker-compose.test.yml` (CSDL tạm), `docker-compose.prod.yml` → Pha 8 (hiện test chạy trên stack dev bằng `scripts/test-docker.sh`).
- Kích hoạt tài khoản bạn đọc, trang cá nhân → Pha 6.
