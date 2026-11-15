# 10 – CHUYỂN ĐỘNG, CHUYỂN TRANG VÀ MICRO-INTERACTION

## 1. `wwwroot/js/motion.config.js` – nguồn duy nhất của mọi thông số
```js
export const motion = {
  dur: { xs: 0.15, sm: 0.25, md: 0.45, lg: 0.7, xl: 1.1 },
  ease: { out: 'power2.out', outStrong: 'power3.out', inOut: 'power2.inOut', expo: 'expo.out', back: 'back.out(1.6)' },
  stagger: { tight: 0.03, normal: 0.06, loose: 0.1 },
  hero: { in: 1.1, out: 1.1, interval: 4.5, resumeAfterDot: 6, xPercent: 7, scaleFrom: 1.05 },
  coverflow: { step: 0.7, perDistance: 0.08, maxJump: 1.1, ease: 'power3.out', wheelLock: 0.25, hoverLift: 10, shelfSwitch: 0.6,
               arcRadius: 1.6, arcStepDeg: 9, cursorSize: 58, cursorLag: 0.18, tiltMax: 6, swipeMaxJump: 5 },
  glass: { hoverLift: 5, cardStagger: 0.06 },
  scroll: { lenisLerp: 0.1, lenisOnTouch: false },
  preview: { flip: 0.55, open: 1.6, pageTurn: 0.22, close: 0.9 },
  dashboard: { pageIn: 0.45, cardStagger: 0.05, countUp: 1.2, rowStagger: 0.02, toast: 0.35, modal: 0.3 },
  reduced: { fade: 0.2 }
};
```
Mọi file JS import từ đây; không viết số thời lượng/easing ở chỗ khác. `prefers-reduced-motion: reduce` → dùng `motion.reduced`.

## 2. State machine (`state-machine.js`)
Một state machine cho mỗi khu vực tương tác lớn (kệ coverflow, Book Preview, catalog, form chứng từ dashboard). Mọi chuyển trạng thái đi qua **một hàm `chuyen(suKien)`** có guard; animation chỉ khởi động từ hàm này; “yêu cầu mới nhất thắng” (giữ tối đa 1 lệnh chờ); khi bị ngắt: `timeline.kill()`, khôi phục transform/opacity về giá trị đích, không kẹt trạng thái. Hủy request API (AbortController) xử lý riêng, không đồng nghĩa hủy timeline.

## 3. Cổng bạn đọc
| Thành phần | Chuyển động |
|---|---|
| Cuộn mượt | Lenis trên desktop (file 13), đồng bộ `ScrollTrigger.update`, `gsap.ticker.lagSmoothing(0)`; điện thoại giữ cuộn gốc |
| Chuyển trang (MPA) | View Transitions API giữa các trang cùng nguồn: `@view-transition { navigation: auto; }`, nền mờ chéo `dur.sm`; header giữ nguyên (`view-transition-name: header`). Trình duyệt không hỗ trợ: tải bình thường |
| Xuất hiện khi cuộn | ScrollTrigger: tiêu đề khu vực trượt lên 24px + mờ → rõ; thẻ sách stagger `normal`; chạy một lần |
| Thanh tiến độ cuộn | scaleX theo tỷ lệ cuộn, cập nhật bằng `quickSetter` |
| Hero, coverflow, Book Preview | Theo file 04, 05, 06 |
| Danh mục → lọc lưới | GSAP Flip: thẻ cũ rời đi mờ + thu nhỏ 0.96, thẻ còn lại dịch chỗ, thẻ mới vào stagger; thanh công cụ đổi nhãn bằng Flip |
| Số liệu “Thư viện bằng số” | đếm lên `dashboard.countUp`, định dạng nghìn bằng dấu chấm |
| Nút Đặt chỗ | nhấn scale .97; đang xử lý: vòng tròn vẽ dần; thành công: tem “ĐÃ GIỮ CHỖ” rơi xuống `back` + rung nhẹ 1 lần |
| Đếm ngược giữ chỗ | đổi số kiểu lật (flip clock) mỗi phút, chuyển sang màu cảnh báo khi < 2 giờ |
| Yêu thích | tim phóng `back` + vài hạt nhỏ tỏa ra (SVG), một lần |
| Thông báo mới | chuông lắc 2 lần, số đỏ nảy |

## 4. Dashboard trắng – đỏ
| Thành phần | Chuyển động |
|---|---|
| Vào trang | nội dung trượt lên 12px + mờ → rõ `dashboard.pageIn`; thẻ KPI stagger `cardStagger` |
| KPI | số đếm lên; mũi tên xu hướng trượt vào; thanh tiến độ mảnh dưới số chạy từ 0 |
| Biểu đồ | hiệu ứng ECharts theo file 09 |
| Sidebar | thu/mở bằng `transform` (lớp nền) + mờ nhãn; thanh chỉ báo đỏ trượt tới mục được chọn (Flip) |
| Lưới dữ liệu | lần tải đầu: hàng stagger `rowStagger`; đổi trang: mờ chéo nội dung, giữ khung (không nhảy chiều cao); hàng mới thêm: nền `--red-50` nhạt dần 1.2s |
| Form chứng từ 3 trạng thái | chuyển trạng thái: thanh bước trượt chỉ báo đỏ; control vừa được bật có vòng sáng đỏ nhạt 1 lần 600ms (hướng mắt người dùng tới việc tiếp theo); control bị tắt mờ dần |
| Lỗi nhập liệu | ô rung ngang 6px 0.3s (chỉ trục x), viền đỏ, dòng lỗi trượt xuống |
| Lưu thành công | dấu tích vẽ nét SVG + toast trượt từ góc phải trên |
| Modal xác nhận | nền mờ `dur.sm`, hộp scale 0.96 → 1 `dashboard.modal` |
| Skeleton | ánh sáng quét ngang (dùng `transform` trên lớp giả) |
| Đặt chỗ sắp hết hạn | chấm đỏ nhịp thở (opacity) bên cạnh dòng |

## 5. Quy tắc hiệu năng chuyển động
- Chỉ animate `transform` và `opacity` trong chuyển động chính; `will-change` chỉ bật trong lúc chạy.
- Không đọc layout trong `onUpdate`; đo trước khi chạy.
- Đo fps bằng Performance API trong kiểm thử Playwright; báo cáo số đo ở mỗi pha có chuyển động.
- Tôn trọng `prefers-reduced-motion` ở mọi chỗ.
