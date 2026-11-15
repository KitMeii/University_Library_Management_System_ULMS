# TIẾN ĐỘ DỰ ÁN ULMS

| Pha | Trạng thái | Ghi chú |
|---|---|---|
| 0A – Khảo sát basecore | **Đã duyệt** (15/11/2026) | `BASECORE_PHAN_TICH.md` |
| 0 – Kế hoạch | **Đã duyệt** (15/11/2026) | `KE_HOACH_PHA_0.md`; quyết định D1–D24 |
| 1 – Nền tảng | **Đã duyệt** (16/11/2026) | D25–D33 đã duyệt |
| 2 – Danh mục + quản trị | **Đang làm** | Đăng nhập (4.6.1) đã xong ở Pha 1 |
| 3 – Chứng từ + in MB02–MB05 | Chưa làm | Kèm nạp lịch sử phiếu mẫu |
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

## Việc còn lại / chuyển pha sau
- Lịch sử phiếu mượn/gia hạn/trả/biên bản mẫu → Pha 3; đặt chỗ mẫu → Pha 6.
- `docker-compose.test.yml` (CSDL tạm), `docker-compose.prod.yml` → Pha 8 (hiện test chạy trên stack dev bằng `scripts/test-docker.sh`).
- Kích hoạt tài khoản bạn đọc, trang cá nhân → Pha 6.
