# 11 – BẢO MẬT, HIỆU NĂNG, KIỂM THỬ, TÀI LIỆU VÀ NGHIỆM THU

## 1. Bảo mật
- Hai cookie scheme tách biệt: `NhanVien` (Area QuanTri, hết hạn trượt 30 phút) và `BanDoc` (cổng). Cookie `HttpOnly`, `Secure`, `SameSite=Lax`.
- Mật khẩu băm PBKDF2-SHA256 tự viết, lưu Base64(salt 16 + hash 32) = 64 ký tự (GIA_DINH D5); chính sách ≥ 8 ký tự có chữ và số; tài khoản quản trị bắt đổi mật khẩu lần đầu.
- Khóa tài khoản nhân viên sau 5 lần sai liên tiếp (SPEC 6.3.1: chuyển `tinhtrang = 'Khóa'`); bạn đọc khóa tạm 15 phút.
- `[AutoValidateAntiforgeryToken]` toàn cục; JS gửi token qua header.
- Phân quyền: `IAuthorizationHandler` đọc PHANQUYEN theo nhóm của người dùng (bộ nhớ đệm, làm mới khi 04.00.02 lưu); attribute `[YeuCauQuyen("TT025")]`, `[YeuCauQuyen("SACH_E")]` trên Action.
- Rate limiting (có sẵn trong .NET) cho đăng nhập, kích hoạt, đặt chỗ.
- Header bảo mật: CSP (chỉ nguồn nội bộ), X-Content-Type-Options, Referrer-Policy, frame-ancestors none.
- Tải ảnh bìa: kiểm tra loại tệp thật, giới hạn 2 MB, chuyển sang WebP, tên tệp ngẫu nhiên.
- Mọi truy vấn qua EF Core / tham số hóa; không ghép chuỗi SQL.
- Ghi NHATKY_DANGNHAP (đăng nhập, đăng xuất, hết phiên) và NHATKY_HOATDONG (bảng – khóa – thao tác – giá trị chính) cho nhân viên.

## 2. Sao lưu – phục hồi (SPEC 6.3.5)
- Trong Docker: thư mục sao lưu là volume `thuvien-backup` gắn vào container SQL Server (file 14 mục 6).
- Sao lưu: `BACKUP DATABASE … TO DISK` vào thư mục cấu hình, tên kèm thời gian; tự động hằng ngày lúc 23:00 (`SaoLuuTuDongService`), giữ 14 bản gần nhất.
- Phục hồi: chỉ N4, xác nhận 2 lần, tự sao lưu bản hiện tại trước; chuyển CSDL sang `SINGLE_USER WITH ROLLBACK IMMEDIATE` → `RESTORE` → `MULTI_USER`; buộc mọi người đăng nhập lại. Ghi rõ trong hướng dẫn triển khai quyền SQL Server cần có.

## 3. Hiệu năng
- Phân trang, lọc, sắp xếp ở server cho mọi lưới; chỉ mục theo file 02.
- Mục tiêu: phản hồi API < 300 ms (P95) với 50.000 bản sách, 200.000 dòng chi tiết; trang chủ LCP < 2.5s, CLS < 0.05.
- Ảnh: WebP, `srcset`, `loading=lazy` trừ ảnh hero đầu và bìa cuốn giữa kệ; cache tĩnh dài hạn có version.
- Bộ nhớ đệm ngắn (60s) cho dữ liệu kệ sách và thống kê tổng quan.

## 4. Kiểm thử
| Lớp | Nội dung tối thiểu |
|---|---|
| Unit (xUnit) | Quy tắc mượn, hạn trả, gia hạn, quá hạn, thành tiền, tổng tiền, điều kiện đặt chỗ (từng điều kiện), sinh số chứng từ, vòng đời đặt chỗ với `IClock` giả |
| Tích hợp (`WebApplicationFactory` + SQL Server LocalDB/container) | `SchemaMatchesSpecTests`; mọi API ngoài quyền → 403; transaction lập phiếu; đặt chỗ đồng thời 20 request; tác vụ hết hạn; báo cáo MB06/MB07 so với truy vấn kiểm chứng |
| E2E (Playwright .NET) | Luồng khép kín: kích hoạt tài khoản bạn đọc → đặt chỗ trên cổng → N1 nhận đặt chỗ trong Lập Phiếu mượn → gia hạn → trả 1 bản đúng hạn, 1 bản quá hạn → N2 lập biên bản → N3 cập nhật tình trạng → MB06, MB07 đúng; trạng thái nút từng form; coverflow (bàn phím, kéo, đổi kệ); Book Preview (mở/đóng/Back/liên kết sâu); reduced motion |
| Chụp ảnh | `ScreenshotTests` chụp mọi giao diện ở các trạng thái chính, 1366×768 và 390×844, đặt tên theo số mục báo cáo (`4.2.1_Danh_muc_sach.png`, `4.3.1_Lap_phieu_muon_TT1.png`…, `Cong_Trang_chu.png`, `Cong_Ke_sach.png`, `Cong_Book_preview.png`, `Cong_Ve_dat_cho.png`) vào `docs/anh_giao_dien/` |
| Accessibility | axe-core chạy trong Playwright cho mọi trang: không lỗi mức nghiêm trọng |
| Mobile | Playwright mô phỏng iPhone 13 và Pixel 7: cổng bạn đọc (kệ vuốt, Book Preview 1 trang, đặt chỗ, tài khoản tab đáy) và dashboard (sidebar thành offcanvas, lưới cuộn ngang, form dùng được) |
| Microservice | Test hợp đồng web ↔ gateway ↔ service; test tích hợp luồng sự kiện file 12 mục 5 (kể cả nhận trùng); chạy toàn hệ thống bằng docker compose trong CI |

## 5. Tài liệu bàn giao
- `README.md`: yêu cầu hệ thống, cài đặt, chuỗi kết nối, chạy migration, khởi tạo tài khoản quản trị, chạy kiểm thử, phiên bản thư viện frontend.
- `docs/HUONG_DAN_SU_DUNG.md`: từng nhóm N1–N4 và bạn đọc, có ảnh chụp.
- `docs/HUONG_DAN_TRIEN_KHAI.md`: triển khai bằng **Docker Compose** trên máy chủ (`docker-compose.prod.yml`), reverse proxy HTTPS, sao lưu, phục hồi, cập nhật phiên bản (build image mới → `docker compose up -d`), xử lý sự cố container.
- `docs/GIA_DINH.md`, `docs/TIEN_DO.md`.

## 6. Nghiệm thu cuối
- [ ] Toàn bộ kiểm thử đạt; build không cảnh báo nghiêm trọng.
- [ ] Đủ chức năng SPEC 6.1–6.4 đúng phân quyền và trạng thái nút.
- [ ] Đặt chỗ 24 giờ đúng mọi quy tắc file 06; không thể giữ trùng bản sách.
- [ ] Trang chủ đủ 9 khu vực; kệ coverflow giống ảnh tham khảo, ≥ 55 fps.
- [ ] Dashboard trắng – đỏ, biểu đồ ECharts có hiệu ứng, số liệu khớp truy vấn kiểm chứng.
- [ ] Ảnh chụp mọi giao diện có trong `docs/anh_giao_dien/` (cả bản desktop và điện thoại).
- [ ] Basecore đã dọn sạch phần không liên quan, đạt nghiệm thu file 12 mục 6.
- [ ] Đạt nghiệm thu Docker ở file 14 mục 8.
