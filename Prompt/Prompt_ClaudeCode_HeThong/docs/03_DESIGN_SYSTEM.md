# 03 – DESIGN SYSTEM CHO HAI MẶT

Tất cả màu, cỡ chữ, khoảng cách, bo góc, bóng là **biến CSS** trong `tokens-cong.css` và `tokens-quantri.css`. Không viết mã màu trực tiếp trong component.

## A. CỔNG BẠN ĐỌC – “Light Academic Emerald”

### Màu
| Nhóm | Token | Giá trị |
|---|---|---|
| Header | `--hdr-bg` / `--hdr-text` / `--hdr-hover` / `--hdr-hover-bg` / `--hdr-accent` | #287A5A / #FFFFFF / #F0D69A / rgba(255,255,255,.12) / #D2AD68 |
| Nền | `--bg` / `--surface` / `--sage-1` / `--sage-2` / `--paper` | #F7F8F5 / #FFFFFF / #EDF3EE / #E5EEE8 / #F3EBDD |
| Thương hiệu | `--forest` / `--emerald` / `--midnight` / `--jade` | #123F35 / #176B54 / #0B2A25 / #4A9B84 |
| Điểm nhấn | `--gold` / `--champagne` / `--bronze` / `--warm-bronze` | #C6A15B / #E3C98B / #806126 / #A9793E |
| Chữ | `--text` / `--text-2` / `--border` | #24322D / #6A7770 / #DFE7E1 |
| Trạng thái | `--error` / `--success` / `--warning` | #B94040 / #267A55 / #A9793E |
| Kệ coverflow | `--shelf-bg` / `--shelf-deco` / `--shelf-title` | #EFEBE3 / rgba(23,107,84,.16) / #123F35 |

Nền theo khu vực: Header #287A5A · Hero #FFFFFF · Kệ coverflow #EFEBE3 (theo ảnh tham khảo) · Danh mục #E5EEE8 · Khám phá sách #F7F8F5 · Book Card #FFFFFF · Book Preview #F3EBDD · Footer #EDF3EE (phần cuối #E4ECE6).

### Chữ
Be Vietnam Pro: Hero 48–68px · tiêu đề khu vực 28–36px · tên sách lớn ở kệ 44–56px (700) · tiêu đề thẻ 17–20px · nội dung 15–16px · chú thích 12–13px (chữ hoa, giãn chữ .12em cho nhãn nhỏ như ảnh tham khảo).
Header: Times New Roman; tên thư viện 32px Bold, menu 19px Bold, tiện ích 15px Bold.

### Bố cục
Container 1280px, lề ngang 24px, khoảng cách khu vực 72–96px, thẻ bo 12px, nút bo 8px.

## B. HỆ THỐNG NGHIỆP VỤ – DASHBOARD TRẮNG – ĐỎ

Nguyên tắc: **nền trắng chiếm ưu thế, đỏ là màu thương hiệu dùng có chủ đích** (nút chính, mục menu đang chọn, số liệu nổi bật, biểu đồ). Không phủ đỏ diện rộng; không dùng đỏ cho trạng thái “thành công”.

### Màu
| Token | Giá trị | Dùng cho |
|---|---|---|
| `--red-900` | #7F1D1D | chữ trên nền đỏ nhạt, số liệu nhấn mạnh |
| `--red-700` | #B91C1C | thương hiệu, logo, tiêu đề nhấn |
| `--red-600` | #DC2626 | nút chính, thanh chỉ báo mục đang chọn |
| `--red-500` | #EF4444 | hover nút chính, biểu đồ |
| `--red-300` | #FCA5A5 | biểu đồ, viền focus |
| `--red-100` | #FEE2E2 | nền mục menu đang chọn, nhãn |
| `--red-50` | #FEF2F2 | nền hàng được chọn trong lưới |
| `--white` / `--bg` / `--surface-2` | #FFFFFF / #FAFAFA / #F5F5F5 | nền |
| `--line` | #ECECEC | viền, kẻ bảng |
| `--ink` / `--ink-2` / `--ink-3` | #1F1F1F / #5C5C5C / #8A8A8A | chữ |
| `--ok` / `--warn` / `--info` | #15803D / #B45309 / #1D4ED8 | thành công / cảnh báo / thông tin |
| `--danger` | #B91C1C | lỗi: **luôn kèm biểu tượng và chữ** vì trùng họ màu thương hiệu |

Tương phản: chữ trắng trên #DC2626 đạt AA cho chữ ≥ 14px đậm; chữ thường trên nền đỏ dùng #B91C1C trở lên.

### Bố cục dashboard
- Sidebar trắng rộng 264px (thu gọn 76px), viền phải `--line`; mục đang chọn: nền `--red-100`, chữ `--red-700`, thanh dọc 3px `--red-600` bên trái, `aria-current="page"`.
- Topbar trắng cao 64px: logo chữ đỏ, ô tìm nhanh, chuông thông báo (đặt chỗ sắp hết hạn), tên + nhóm người dùng, menu tài khoản.
- Vùng nội dung nền `--bg`, thẻ trắng bo 14px, bóng rất nhẹ `0 1px 2px rgba(0,0,0,.04), 0 8px 24px rgba(0,0,0,.04)`.
- Form nghiệp vụ: khu nhập liệu trên, lưới dưới, hàng nút cố định đáy thẻ. Ô chỉ đọc nền `--surface-2`; ô bắt buộc dấu * đỏ; nút đang bật nổi bật, nút đang tắt mờ 40% và có `aria-disabled`.
- Chỉ báo trạng thái form chứng từ (Trạng thái 1/2/3) dạng thanh bước ở đầu form, bước hiện tại màu đỏ.
- Font Be Vietnam Pro; số liệu dùng `font-variant-numeric: tabular-nums`.

## C. KHU TÀI KHOẢN BẠN ĐỌC – GLASSMORPHISM TRÊN NỀN PASTEL EMERALD
Chỉ dùng cho `/tai-khoan/*`, hộp đăng nhập/kích hoạt. Không dùng cho lưới sách, Book Preview, dashboard nhân viên.
- Nền: dải màu loang (mesh) nhẹ bằng 3–4 `radial-gradient` cố định: `#E5EEE8`, `#EDF3EE`, `#F3EBDD`, `#E3EFE9` và một vệt `rgba(198,161,91,.18)`; không animate nền.
- Thẻ kính `.glass-card`: `background: rgba(255,255,255,.58)`; `backdrop-filter: blur(14px) saturate(1.2)`; viền `1px solid rgba(255,255,255,.72)`; bo 16–20px; bóng `0 8px 24px rgba(12,91,69,.08)`.
- Hover thẻ: nâng `translateY(-5px)` bằng GSAP/transition `transform` (không animate `box-shadow`: dùng lớp `::after` chứa bóng đậm hơn và đổi `opacity`).
- Thanh điều hướng tài khoản: **dạng viên thuốc dọc màu Deep Forest #123F35** bo tròn, nổi giữa nền sáng; icon đang chọn có vòng tròn sáng `--champagne`, tooltip khi trỏ/focus; trên mobile chuyển thành **thanh tab dưới đáy** (cùng phong cách viên thuốc, cao 64px + safe-area).
- Màu theo loại nội dung (nhãn nhỏ góc thẻ + ảnh nhỏ): Đang mượn `#DDEFE6`, Đặt chỗ `#F6EBD3`, Quá hạn/vi phạm `#F6DEDE`, Yêu thích `#E7E3F3`, Thông báo `#E1ECF4`.
- Thanh tiến độ mảnh 4px (ví dụ thời gian còn lại của hạn trả, của đặt chỗ).
- **Hiệu năng:** tối đa 6 phần tử có `backdrop-filter` trên một màn hình; trên màn hình < 768px hoặc khi `@supports not (backdrop-filter: blur(1px))` → thay bằng nền `rgba(255,255,255,.86)` không blur; tôn trọng `prefers-reduced-transparency` nếu trình duyệt hỗ trợ.

## D. Thành phần dùng chung
Button (chính, phụ, viền, nguy hiểm, biểu tượng), Input, Select, DatePicker (chọn ngày, không bắt gõ tay ngày), DataGrid (phân trang server, sắp xếp, chọn dòng), Modal xác nhận, Toast, Skeleton, Empty state có minh họa nhỏ, Badge trạng thái, Pill “DỮ LIỆU MẪU”, Countdown (đếm ngược giờ giữ chỗ), Stepper trạng thái form.

Chế độ tối: không bắt buộc. Nếu làm, chỉ cho cổng bạn đọc, trang sách vẫn giữ nền giấy.
