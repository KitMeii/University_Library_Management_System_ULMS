# THAM KHẢO https://mta.edu.vn – HIỆU ỨNG VÀ BỐ CỤC (bổ sung Pha 7)

> Chỉ học **kiểu bố cục và hiệu ứng**. Không chép logo, ảnh, chữ, màu nhận diện hay mã nguồn của mta.edu.vn. Ảnh chụp đối chiếu ở
> `tham_khao_mta/` chỉ giữ trên máy (`.gitignore`, quyết định chủ dự án 17/11/2026) – tài liệu này chỉ mô tả bằng chữ.
> Giữ nguyên: design token file 03, bố cục khu file 04, toàn bộ chuyển động sách (coverflow file 05, Book Preview file 06,
> vé giữ chỗ) và các giá trị `motion.coverflow`, `motion.preview` trong `motion.config.js`.

## 1. Cách nghiên cứu
- Playwright (Chromium) mở trang ở **1440×900** và **390×844**. Máy tính: lăn chuột từng nấc 700px, mỗi nấc chụp 3 khung (120ms, 400ms, 1500ms). Điện thoại: cuộn từng đoạn 80% khung nhìn, 3 khung mỗi đoạn. Có thêm rê chuột lên menu (100/250/700ms), và đọc computed style, quy tắc CSS có `transition`/`animation`, thư viện trên `window`.
- Chỉ đọc. Mã nghiên cứu là lớp kiểm thử tạm, không commit. Số liệu thô ở `artifacts/mta/*.json`.

## 2. Công nghệ của trang tham khảo
| Hạng mục | Quan sát |
|---|---|
| Khung | Next.js (Turbopack, `_next/static/chunks`), Tailwind CSS (lớp `duration-300`, `group-hover:scale-105`…), Mantine UI (Menu, Carousel = Embla) |
| Cuộn | Máy tính: cả trang nằm trong một **div `fixed inset-0 overflow-y-auto`** (không cuộn `window`); điện thoại cuộn `window` bình thường |
| Hiệu ứng | Chủ yếu **CSS transition** của Tailwind/Mantine (0.1–0.5s, `cubic-bezier(0.4,0,0.2,1)`). Hiệu ứng vào khu dùng opacity + dịch lên (thư viện React kiểu framer-motion, không lộ trên `window`). Không có AOS/WOW/GSAP/Swiper/jQuery |
| Font | Inter, Montserrat (tiêu đề), Playfair/Noto Serif tải kèm |

## 3. Từng hiệu ứng → đề xuất cho trang chủ ULMS

Thông số đề xuất đều lấy từ `motion.config.js` hiện có (`dur.xs 0.15 · sm 0.25 · md 0.45 · lg 0.7`, `ease.out power2.out`, `stagger.normal 0.06`) để cùng nhịp với chuyển động sách. Chỉ animate `transform`/`opacity`, không thuộc tính `style` nội tuyến (CSP chặt), có bản giảm chuyển động.

### 3.1 Header dính, đổi trạng thái khi cuộn

- **Quan sát:** đầu trang, header gồm dải tiện ích 44px (lời chào, mạng xã hội, Tìm kiếm, ngôn ngữ) và dải menu, nằm **trong suốt đè lên Hero ảnh tràn**. Cuộn xuống: dải tiện ích ẩn, còn một thanh màu đặc cao ~110px. Đổi ngay, không có chuyển tiếp dài.
- **Đề xuất ULMS – Header:** giữ một màu #287A5A (file 04 mục 2). Cuộn xuống quá 120px thì **thu gọn dải tiện ích**: cả header dịch lên bằng `translateY(-chiều cao dải tiện ích)`, logo mờ dần còn dải menu, `dur.sm` + `ease.out`. Cuộn lên hoặc về đầu trang thì hiện lại. Dùng `transform` (không đổi `height`, không reflow); bóng nhẹ khi `scrollY > 8` giữ như cũ.
- **KHÔNG áp dụng:** header trong suốt đè lên Hero. File 04 quy định header một màu, Hero dạng khung bo góc có viền vàng.

### 3.2 Menu thả xuống

- **Quan sát:** rê chuột lên mục thì mục hiện **nền trắng dạng tab nối liền với bảng con** trắng bên dưới (rộng 350px, cao 210px). Bảng hiện bằng `opacity` + `transform`, **0.2s**.
- **Đề xuất ULMS – Header:**
  - Mục “Danh mục” có bảng thả xuống liệt kê thể loại (lấy từ `/api/cong/the-loai`, kèm số đầu sách). Bấm một thể loại sẽ lọc khu Khám phá sách nếu đang ở trang chủ, hoặc mở `/kham-pha?theLoai=`.
  - Mục “Hướng dẫn mượn sách” có bảng gồm các mục: Mượn sách, Đặt chỗ, Gia hạn, Trả sách, Vi phạm.
  - Hiệu ứng: tab nền trắng + bảng `opacity 0→1`, `translateY(-6px→0)`, `dur.sm`, `ease.out`.
  - Mở bằng rê chuột, focus hoặc phím Enter/↓; Esc đóng. Dùng Bootstrap dropdown sẵn có (Popper ghi style bằng CSSOM, hợp CSP).
  - Điện thoại: thành nhóm xổ trong Offcanvas.

### 3.3 Hero
- **Quan sát:** ảnh tràn màn hình, chữ lớn ở giữa, nút ← → hai bên, biểu tượng chuột **“CUỘN XUỐNG” nhấp nhô** ở đáy.
- **Đề xuất ULMS – Hero:**
  - Giữ nguyên Hero file 04 (khung bo 18px, lớp phủ, viền vàng, thông số chuyển ảnh).
  - Thêm **gợi ý “Cuộn xuống”**: biểu tượng chuột nhấp nhô bằng CSS `@keyframes`, `translateY 0→6px`, chu kỳ 1.6s, `ease-in-out`. Biểu tượng tự ẩn khi đã cuộn và không nhấp nhô khi giảm chuyển động. Bấm vào thì cuộn tới kệ sách.
  - Thêm **nút ← →** cạnh chấm điều hướng, dùng chính hàm chuyển ảnh hiện có của hero.js (không thêm chuyển động mới).
- **KHÔNG áp dụng:** ảnh tràn màn hình, chữ đè cực lớn. Trái file 04; trang ULMS còn kệ sách ngay bên dưới.

### 3.4 Hiệu ứng xuất hiện khi cuộn

- **Quan sát:** khi khu vào khung nhìn, tiêu đề và nội dung **mờ → rõ và trượt lên** (~100px), xong trong khoảng 0.8–1s. Tiêu đề, nội dung trái, nội dung phải xuất hiện gần cùng lúc.
- **Đề xuất ULMS – Danh mục, Khám phá, Hướng dẫn, Thư viện bằng số, Thông tin thư viện:**
  - Giữ biên độ nhỏ 24px như hiện có (êm hơn, hợp nhịp sách) và thêm **so le trong khu**: tiêu đề → hàng ô/thẻ, `stagger.normal` (0.06s), `dur.lg`, `ease.out`, chạy một lần.
  - Ô thể loại và 4 bước hướng dẫn vào lần lượt từ trái sang.
  - Giảm chuyển động: hiện ngay.
- **KHÔNG áp dụng cho khu Kệ sách** (`#ke-sach`): kệ có hiệu ứng vào riêng (file 05).

### 3.5 Kiểu tiêu đề khu

- **Quan sát:**
  - Tiêu đề in hoa đậm, **vạch nhấn dọc** bên trái, liên kết “Xem thêm ▸” canh phải.
  - Khu số liệu: tiêu đề canh giữa, **gạch ngắn** dưới tiêu đề.
- **Đề xuất ULMS – mọi khu dưới kệ sách:**
  - Thêm vạch nhấn dọc màu `--gold` trước tiêu đề, giữ font tiêu đề file 03.
  - Liên kết bên phải:
    - “Xem tất cả sách ▸” → `/kham-pha`
    - “Xem hướng dẫn đầy đủ ▸” → `/huong-dan`
    - Danh mục: “Khám phá theo thể loại ▸”
  - Khu Thư viện bằng số: tiêu đề canh giữa + gạch ngắn `--gold`. Gạch dài ra từ 0 khi xuất hiện (`scaleX`, `dur.md`).

### 3.6 Ô lối tắt có biểu tượng (điện thoại)

- **Quan sát:** ô bo góc, biểu tượng trong vòng tròn màu nhạt bên trái, tên đậm, **biểu tượng mờ cỡ lớn làm hình nền** bên phải.
- **Đề xuất ULMS – Danh mục thể loại:** giữ lưới hiện có, thêm biểu tượng Bootstrap Icons cỡ lớn mờ (opacity .08) ở góc phải mỗi ô. Rê chuột thì ô nâng 4px như cũ, vòng biểu tượng phóng 1.06 và biểu tượng mờ trượt 6px, `dur.sm`.

### 3.7 Rê chuột lên thẻ
- **Quan sát (CSS đọc được):** ảnh thẻ tin **phóng 1.05** trong khung `overflow:hidden`, `transition: transform 0.3s/0.5s cubic-bezier(0.4,0,0.2,1)`. Nút: nền đổi màu 0.3s. Nút Tìm kiếm: `scale(1.05)` 0.1s.
- **Đề xuất ULMS:**
  - Nút “Tìm” (Hero, Khám phá) và liên kết “Xem thêm ▸”: mũi tên trượt 4px khi rê chuột, `dur.xs`.
  - Ô Hướng dẫn: nâng 4px + viền `--jade` như ô thể loại.
- **KHÔNG áp dụng cho thẻ sách** (Book Card – file 04 mục 4 đã có nâng 4px + nghiêng 3D 6° + bóng `::after`). Thêm phóng ảnh sẽ chồng lên hiệu ứng sách.

### 3.8 Bộ đếm số

- **Quan sát:** 4 cột, mỗi cột có biểu tượng nét mảnh ở trên, **số lớn đếm từ 0** khi vào khung nhìn, nhãn bên dưới. Nền ảnh tràn có lớp phủ tối.
- **Đề xuất ULMS – Thư viện bằng số:**
  - Đã có đếm lên (`dashboard.countUp`). Thêm: biểu tượng trong vòng tròn viền `--bronze`, số và nhãn canh giữa, tiêu đề canh giữa có gạch ngắn (3.5).
  - Các cột vào so le `stagger.normal`.
  - Giữ nền `--paper` (file 04), không dùng ảnh nền.
  - Không thêm “+” giả vào số (số liệu thật).

### 3.9 Footer và nút lên đầu trang

- **Quan sát:**
  - Footer nền màu đậm, **hoa văn lớn rất mờ** phía sau.
  - Hàng trên: logo + các cột liên hệ có biểu tượng. Giữa: “Cơ sở” (dòng có biểu tượng định vị) và “Liên kết” (dòng có ▸). Dưới: biểu tượng mạng xã hội tròn, dải bản quyền.
  - **Nút lên đầu trang** hình vuông cố định ở góc phải dưới.
- **Đề xuất ULMS – Footer:**
  - Giữ màu file 04 (#EDF3EE / #E4ECE6). Bố cục lại 3 cột: Thư viện (tên, mô tả, liên hệ có biểu tượng), Liên kết (dòng ▸ có mũi tên trượt khi rê chuột), Giờ mở cửa.
  - Hoa văn nền tự vẽ SVG (đường gáy sách/kệ sách nét mảnh, opacity .06). Không dùng hoa văn của MTA.
  - Thêm **nút lên đầu trang** tròn ở góc phải dưới:
    - Hiện khi cuộn quá 1 khung nhìn (mờ + trượt lên, `dur.sm`). Bấm thì cuộn mượt bằng Lenis.
    - Ẩn khi Book Preview mở. Trên điện thoại nằm trên vùng an toàn; trang sách đã có nút Đặt chỗ ghim đáy nên không chồng lên.
- **Thông tin thư viện (khu 8):** dùng kiểu dòng có biểu tượng định vị/điện thoại/email như footer tham khảo. Vẫn chỉ hiện khi đã có dữ liệu thật (D75).

### 3.10 Mục KHÔNG áp dụng
| Hiệu ứng ở trang tham khảo | Lý do |
|---|---|
| Logo xoay 3D liên tục trên header (`rotateY`) | Chuyển động liên tục kéo mắt khỏi kệ sách; tốn khung hình |
| Toàn trang cuộn trong một `div` cố định | Phá Lenis, ScrollTrigger, kéo kệ coverflow, phím cuộn và trình đọc màn hình |
| Băng chữ chạy (`.animate-marquee`, 30s lặp vô hạn) | Chuyển động liên tục chạy cùng lúc với kệ sách, giảm fps |
| Carousel “Gương mặt tiêu biểu” (Embla) | Kệ coverflow đã là băng chuyền chính; thêm băng chuyền thứ hai làm rối và cần thư viện mới |
| Header trong suốt khi cuộn trên điện thoại | Chữ trắng đè lên nội dung, khó đọc |
| Ảnh nền tràn có lớp phủ tối cho khu số liệu | File 04 quy định nền `--paper`; ảnh nền làm trang nặng |
| Phóng ảnh khi rê chuột lên thẻ | Trùng với hiệu ứng nghiêng 3D của thẻ sách |

## 4. Điều kiện kỹ thuật khi làm
1. **Không chạy cùng lúc với kệ sách:** hiệu ứng xuất hiện chỉ gắn với các khu dưới kệ, chạy một lần. Header thu gọn chỉ theo cuộn trang; kéo kệ không cuộn trang nên không kích hoạt. Khi Book Preview mở thì dừng Hero và ẩn nút lên đầu trang.
2. **fps kệ sách:** giữ `content-visibility: auto` cho các khu dưới kệ, mọi hiệu ứng mới chỉ dùng `transform`/`opacity`. Đo lại theo file 05 mục 7, không thấp hơn mức hiện tại (CPU chậm 4× ~25 fps trong container không GPU).
3. **CSP chặt** (không `'unsafe-inline'`, không `'unsafe-hashes'`):
   - Mọi hiệu ứng mới dùng lớp CSS + GSAP (CSSOM).
   - **Phải đổi D78:** bỏ plugin GSAP Flip (thứ duy nhất cần `'unsafe-hashes'`) và tự viết hiệu ứng FLIP cho lưới Khám phá sách bằng GSAP core (đo vị trí trước/sau, `gsap.fromTo` x/y). Cảm giác giữ như hiện tại.
   - Băm thẻ `<style>` của page-flip (D69) không cần `'unsafe-hashes'`, giữ nguyên.
4. **Thư viện:** chỉ GSAP core + ScrollTrigger, Lenis, Bootstrap 5.3.3, Bootstrap Icons (đã duyệt ở file 13). **Không thêm thư viện mới.**
5. **Giảm chuyển động:** mọi hiệu ứng mới có nhánh `prefers-reduced-motion` (hiện ngay, không nhấp nhô, không trượt).
6. **Điện thoại:**
   - Menu thả xuống thành nhóm xổ trong Offcanvas.
   - Header thu gọn cũng chạy trên điện thoại (dải tiện ích vốn thấp).
   - Ô thể loại 1–2 cột, nút lên đầu trang không che nội dung.

## 5. Kế hoạch sau khi được duyệt
1. Chụp ảnh **trước** (desktop 1366×768 + mobile 390×844) các khu bị chỉnh.
2. Sửa theo mục 3: header thu gọn + menu thả xuống; Hero (gợi ý cuộn, nút ← →); tiêu đề khu; so le xuất hiện; ô thể loại; khu số liệu; footer + nút lên đầu trang; bỏ GSAP Flip (CSP).
3. Chụp ảnh **sau** cùng vị trí, ghép ảnh so sánh trước/sau. Đo lại fps kệ sách. Chạy lại toàn bộ kiểm thử Pha 7 (và cả bộ kiểm thử). Cập nhật GIA_DINH, TIEN_DO, DANH_SACH_ANH.

## 6. Câu hỏi cần chủ dự án quyết
1. Duyệt danh sách áp dụng (mục 3) và không áp dụng (mục 3.10)?
2. Menu thả xuống cho “Danh mục” (danh sách thể loại) và “Hướng dẫn mượn sách” (các mục) – đồng ý hai mục này?
3. Nút ← → ở Hero (bổ sung cho chấm điều hướng) – đồng ý?
4. 12 ảnh chụp trang tham khảo trong `docs/tham_khao_mta/` (~12MB) có hình ảnh, logo của mta.edu.vn: có commit lên GitHub không, hay chỉ giữ ở máy (thêm vào `.gitignore`)?
