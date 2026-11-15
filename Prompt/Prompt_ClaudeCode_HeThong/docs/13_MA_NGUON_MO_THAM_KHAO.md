# 13 – MÃ NGUỒN MỞ VÀ THAM KHẢO HIỆU ỨNG

Được phép dùng thư viện hoặc học cách làm từ dự án mã nguồn mở **phổ biến, còn bảo trì, giấy phép cho phép dùng thương mại (MIT/Apache-2.0/BSD hoặc giấy phép miễn phí của GSAP)**. Trước khi dùng: kiểm tra giấy phép, số sao/lượt tải, ngày phát hành bản định dùng (≥ 14 ngày), lỗ hổng đã biết; ghi vào `docs/THU_VIEN_BEN_THU_BA.md` (tên, phiên bản, giấy phép, dùng cho việc gì). Chép mã từ dự án mở phải giữ thông báo bản quyền.

## 1. Thư viện được duyệt dùng
| Thư viện | Dùng cho | Ghi chú |
|---|---|---|
| **GSAP 3.13** (core, Flip, ScrollTrigger, Observer, Draggable, InertiaPlugin) | Toàn bộ chuyển động, kệ coverflow, mở sách, morph lưới | Từ 3.13 toàn bộ plugin miễn phí |
| **StPageFlip** – gói npm `page-flip` 2.0.7 (MIT, không phụ thuộc, hỗ trợ mobile, chế độ ngang/dọc) | Lật trang trong Book Preview: desktop 2 trang, **mobile 1 trang vuốt lật** | Chỉ dùng phần lật trang; bìa bay ra và mở bìa vẫn làm bằng GSAP |
| **Lenis** (MIT, darkroom.engineering) – bản 1.3.x ổn định | Cuộn mượt cổng bạn đọc, đồng bộ ScrollTrigger | Tắt trên thiết bị cảm ứng (giữ cuộn gốc), tắt khi reduced motion; thêm `data-lenis-prevent` cho Book Preview, dropdown |
| **Apache ECharts 5.6** | Biểu đồ | File 09 |
| **Bootstrap 5.3.3**, **Bootstrap Icons 1.11.3** | Lưới, offcanvas, icon | File 01 |
| **JsBarcode** (MIT) | Mã vạch Code128 trên vé giữ chỗ | Hoặc tự vẽ SVG |

Không thêm thư viện khác nếu chưa được duyệt. Không dùng Three.js/WebGL cho kệ sách (CSS 3D đủ).

## 2. Tham khảo cách làm (đọc mã để học, không nhất thiết cài)
| Nguồn | Học gì |
|---|---|
| **Swiper** (nolimits4web/swiper, MIT) – module `effect-coverflow` | Công thức coverflow: `rotate`, `depth`, `stretch`, `modifier` theo khoảng cách tới tâm; cách xử lý cảm ứng, quán tính, “resistance” ở hai đầu trên mobile. Áp dụng ý tưởng vào hàm `layout(pos)` của file 05 |
| Bộ demo **Codrops** (tympanus/codrops trên GitHub, các bài “3D book”, “book showcase”, “page flip”) | Dựng khối sách CSS 3D (mặt trước, gáy, mặt trên giấy), ánh sáng/bóng giả, mở bìa |
| Trang demo GSAP chính thức (gsap.com/demos, CodePen của GreenSock) | Mẫu `quickSetter`, Observer cho cuộn/vuốt, Draggable + Inertia snap, Flip cho morph lưới |
| **StPageFlip demos** | Cấu hình lật trang mềm/cứng, chế độ dọc trên điện thoại |

## 3. Áp dụng hai nguồn tham khảo của chủ dự án

### 3.1 Bản dựng “kệ băng VHS” chạy trên Claude (tham khảo 1)
Đưa các điểm sau vào kệ coverflow (file 05 đã cập nhật):
- Sách nằm trên **một cung cong lùi dần vào chiều sâu**; cuốn chọn đứng thẳng ở giữa, lộ trọn bìa; gáy **nhỏ dần và tối dần** về hai mép.
- Chữ trang trí chấm lớn mờ hai bên; tên sách đậm cỡ lớn, dòng phụ, năm và thể loại bên dưới, mũi tên hai bên; thanh thời gian có chấm trượt tới năm của sách, bấm vào thanh để nhảy tới sách gần năm đó nhất.
- Kéo, cuộn chuột, phím mũi tên; gáy nâng lên khi hover; **con trỏ tròn 58px chữ “Mở”** khi rê trên cuốn giữa (chỉ máy có chuột); **bìa nghiêng nhẹ theo chuột** khi hover cuốn giữa (tối đa ±6°, trở về thẳng khi rời chuột – trạng thái nghỉ đứng thẳng như ảnh mẫu); bấm mở **Book Preview**.

### 3.2 Glassmorphism trên nền pastel (tham khảo 2)
- Áp dụng cho **khu tài khoản bạn đọc** (`/tai-khoan/*`) và hộp đăng nhập/kích hoạt; **không** dùng cho lưới sách và dashboard nhân viên (dashboard giữ trắng – đỏ phẳng, sạch).
- Chi tiết token và quy tắc hiệu năng ở file 03 mục C, bố cục ở file 07.
