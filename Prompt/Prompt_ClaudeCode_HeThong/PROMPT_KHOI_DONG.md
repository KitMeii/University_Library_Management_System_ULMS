# Dán đoạn dưới đây vào Claude Code (chế độ Plan) sau khi đã đặt `basecore/` và `docs/` vào gốc dự án

Bạn là Senior Full-stack Engineer (.NET, microservice), UI/UX Engineer và Motion Designer. Nhiệm vụ: xây dựng **hệ thống thật** “Quản lý mượn trả sách thư viện đại học” **trực tiếp trên thư mục `basecore/`** (backend microservice có sẵn), gồm **Cổng bạn đọc** (trang chủ có kệ sách coverflow, mở sách, đặt chỗ giữ sách 24 giờ, khu tài khoản kính mờ) và **Hệ thống nghiệp vụ** cho nhân viên N1–N4 (dashboard trắng – đỏ, đúng toàn bộ chức năng đã thiết kế). Hệ thống phải dùng tốt trên cả máy tính và điện thoại, và **toàn bộ chạy bằng Docker Compose** (file `14_DOCKER.md`).

Toàn bộ yêu cầu nằm trong `docs/`:
1. Đọc `docs/00_KE_HOACH_TONG_THE.md` trước, rồi đọc **tất cả** file còn lại, `docs/SPEC_NGHIEP_VU_BT4.md` và ảnh `docs/tham_khao/hieu_ung_ke_sach_coverflow.png`.
2. `SPEC_NGHIEP_VU_BT4.md` là nguồn chuẩn nghiệp vụ: giữ nguyên tên bảng, tên trường, mã tiến trình, phân quyền, tên control, trạng thái nút. Phần mở rộng chỉ theo `02_CSDL_VA_MO_RONG.md`.
3. **Pha 0A (chỉ đọc, chưa sửa gì):** khảo sát kỹ toàn bộ `basecore/` theo `12_BASECORE_MICROSERVICE.md`, lập `docs/BASECORE_PHAN_TICH.md`, gắn nhãn GIỮ / TÁI DÙNG / XÓA / CHƯA RÕ cho từng thư mục, project, service.
4. **Pha 0:** trình kế hoạch trên nền basecore: ranh giới service và bảng sở hữu, kế hoạch Docker (Dockerfile từng service, các file compose, volume, healthcheck), CSDL chung hay riêng (nêu ưu nhược để tôi chọn), sự kiện giữa service, gateway, xác thực hai loại tài khoản, frontend, cách làm kệ sách như ảnh, đặt chỗ an toàn đồng thời, chiến lược kiểm thử (gồm mobile), rủi ro, và **danh sách câu hỏi cho tôi**. **Không xóa, không viết code cho đến khi tôi duyệt kế hoạch và danh sách XÓA.**
5. Sau khi duyệt: làm lần lượt Pha 1 → Pha 8. Hết mỗi pha: chạy toàn hệ thống bằng `docker compose up -d --build`, chạy kiểm thử, tự chụp ảnh kiểm tra bằng Playwright (desktop và điện thoại), sửa lỗi, báo cáo theo mẫu mục 5 file 00, cập nhật `docs/TIEN_DO.md`, rồi **dừng chờ tôi duyệt**.
6. Phiên làm việc mới: đọc `docs/TIEN_DO.md`, `docs/GIA_DINH.md`, `docs/BASECORE_PHAN_TICH.md` để làm tiếp đúng chỗ.

Bắt đầu Pha 0A.
