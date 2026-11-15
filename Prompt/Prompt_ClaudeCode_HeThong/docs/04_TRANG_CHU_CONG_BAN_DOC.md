# 04 – TRANG CHỦ VÀ CÁC TRANG CỔNG BẠN ĐỌC

Mục tiêu trải nghiệm: sinh viên vào trang chủ → **lướt kệ sách** → mở xem một cuốn → **đặt chỗ** → trong 24 giờ đến quầy nhận sách. Bố cục rõ ràng, mỗi khu vực một nhiệm vụ, đọc từ trên xuống là hiểu thư viện có gì và mượn thế nào.

## 1. Bố cục trang chủ `/` (từ trên xuống)

| # | Khu vực | Nội dung | Nguồn dữ liệu | Nền |
|---|---|---|---|---|
| 1 | Header (sticky) | Vùng tiện ích: logo + tên thư viện, giờ mở cửa hôm nay, trạng thái tài khoản. Vùng điều hướng: Trang chủ, Khám phá sách, Danh mục, Kệ sách nổi bật, Hướng dẫn mượn sách, Thông tin thư viện; bên phải: Tìm kiếm, Yêu thích (số lượng), Thông báo (số chưa đọc), Ảnh đại diện hoặc Đăng nhập | `appsettings` mục `ThuVien:ThongTin`, tài khoản | #287A5A |
| 2 | Hero | Khung ảnh chạy lần lượt với câu «Tri thức mở lối – Tương lai rộng mở.» (thẻ h1), dải mô tả + thanh tìm kiếm ngay dưới khung | ảnh trong `wwwroot/img/hero` | #FFFFFF |
| 3 | **Kệ sách nổi bật (coverflow)** | Hiệu ứng như ảnh tham khảo – chi tiết ở file 05. Có các “kệ”: Sách mới về, Mượn nhiều nhất, và mỗi thể loại | DAUSACH, đếm lượt mượn thật từ CT_PHIEUMUON | #EFEBE3 |
| 4 | Danh mục thể loại | Lưới ô thể loại (biểu tượng, tên, số đầu sách). Bấm một ô: cuộn tới khu 5 và lọc theo thể loại, **không đổi route, không tải lại trang** | THELOAI, đếm DAUSACH | #E5EEE8 |
| 5 | Khám phá sách | Thanh công cụ (ô tìm, thể loại, năm xuất bản, chỉ sách còn có thể mượn, sắp xếp: mới nhất/tên/mượn nhiều) + lưới thẻ sách ảnh bìa tỷ lệ 2:3, 4/3/2 cột theo màn hình; phân trang “Xem thêm”; đổi bộ lọc dùng GSAP Flip để các thẻ dịch chuyển mượt | API `/api/cong/sach` | #F7F8F5 |
| 6 | Hướng dẫn mượn và đặt chỗ | 4 bước có số và biểu tượng: (1) Tìm và mở sách → (2) Bấm **Đặt chỗ** (hệ thống giữ 1 bản cho bạn) → (3) Đến quầy mượn trả **trong 24 giờ** mang thẻ bạn đọc → (4) Nhận sách và Phiếu mượn. Ghi rõ: quá 24 giờ hệ thống tự hủy, tối đa 3 đặt chỗ cùng lúc | `appsettings` `ThuVien:DatCho` | #FFFFFF |
| 7 | Thư viện bằng số | 4 số: đầu sách, bản sách, bạn đọc, lượt mượn tháng này – đếm từ 0 lên khi cuộn tới | truy vấn thật | #F3EBDD |
| 8 | Thông tin thư viện | Giờ mở cửa theo ngày, địa chỉ, email, điện thoại, nội quy ngắn | `appsettings` | #FFFFFF |
| 9 | Footer | Tên thư viện, liên hệ, liên kết hướng dẫn, bản quyền | `appsettings` | #EDF3EE / #E4ECE6 |

Không hiển thị khu vực nào khi không có dữ liệu thật (ví dụ thể loại rỗng).

## 2. Header, Hero (giữ yêu cầu từ bộ prompt 02, 03 của chủ dự án)
- Header một dải màu #287A5A đồng nhất, sticky, khi cuộn vẫn xanh, có bóng nhẹ khi `scrollY > 8`, thanh tiến độ cuộn 3px màu vàng ngay dưới header. Mục đang chọn chữ #F0D69A + gạch chân vàng đồng. Mobile: nút menu + Offcanvas.
- Hero: khung bo 18px, cao `clamp(300px, 54vh, 500px)`, lớp phủ rgba(7,63,50,.58), viền vàng 2px cách mép 14px; ≥ 4 ảnh chuyển lần lượt (vào: opacity 0→1, xPercent 7→0, scale 1.05→1, 1.1s power2.inOut; ra: xPercent →-7), tự chuyển mỗi 4.5s bằng `gsap.delayedCall`, **có nút Tạm dừng/Phát**, dừng khi hover/focus, `prefers-reduced-motion` thì chỉ crossfade 0.3s, ảnh đầu không lazy-load, ảnh sau `decode()` xong mới chuyển. Dấu chấm điều hướng có `aria-label` và `aria-current`.
- Thanh tìm kiếm Hero: ô nhập (tên sách, tác giả, ISBN, từ khóa) + nút Tìm + vài gợi ý từ khóa (lấy từ `DAUSACH.tukhoa` phổ biến). Enter → cuộn tới khu 5 với kết quả.

## 3. Các trang khác của cổng
| Route | Trang | Ghi chú |
|---|---|---|
| `/kham-pha` | Khám phá sách toàn trang | Cùng thành phần với khu 5, có bộ lọc nâng cao |
| `/sach/{maDauSach}` | Liên kết sâu tới Book Preview | Mở trang chủ rồi tự mở Book Preview của sách đó (không làm trang chi tiết riêng – file 06) |
| `/huong-dan` | Hướng dẫn mượn, gia hạn, trả, đặt chỗ, xử lý vi phạm | Nội dung tĩnh lấy từ quy định ở `appsettings` |
| `/dang-nhap`, `/kich-hoat` | Đăng nhập, kích hoạt tài khoản bạn đọc | File 07 |
| `/tai-khoan/*` | Trang cá nhân | File 07 |

## 4. Book Card (thẻ sách trong lưới)
Ảnh bìa 2:3 (lazy-load, có `width/height`), tên sách (tối đa 2 dòng), tác giả, năm xb, nhãn tình trạng: “Còn N bản có thể mượn” (xanh) / “Đang được mượn hết” (xám) – tính ở server: số bản `SACH.tinhtrang = 'Sẵn sàng'` trừ số bản đang được giữ chỗ. Nút tim (yêu thích). Bấm thẻ → mở Book Preview. Hover: nâng thẻ 4px, bìa nghiêng nhẹ 3D (rotateY 6°), bóng đậm hơn qua lớp giả `::after` (không animate box-shadow trực tiếp).

## 5. Nghiệm thu
- [ ] Đọc từ trên xuống thấy đủ 9 khu vực theo thứ tự; không khu vực nào dùng số liệu bịa.
- [ ] Chọn thể loại, tìm kiếm, lọc: không đổi route, không tải lại trang, danh sách cũ giữ nguyên trong lúc tải, response cũ bị bỏ (AbortController + requestId).
- [ ] Hero đạt các yêu cầu ở mục 2, có nút Tạm dừng/Phát.
- [ ] Không có CLS khi tải ảnh; LCP < 2.5s trên mạng 4G mô phỏng.
