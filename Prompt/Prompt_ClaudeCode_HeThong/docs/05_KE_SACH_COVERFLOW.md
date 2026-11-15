# 05 – KỆ SÁCH NỔI BẬT: HIỆU ỨNG COVERFLOW GÁY SÁCH (THEO ẢNH THAM KHẢO)

Ảnh tham khảo: `docs/tham_khao/hieu_ung_ke_sach_coverflow.png`. **Phải mở ảnh này và làm giống bố cục, cảm giác chuyển động.** Chỉ thay nội dung (sách thư viện) và màu theo design system.

## 1. Mô tả hình ảnh cần đạt
- Một hàng sách nằm ngang giữa khu vực, **cuốn ở giữa quay mặt bìa thẳng về người xem** và lớn nhất.
- Các cuốn hai bên **xoay gần vuông góc, lộ gáy sách** (gáy có tên sách viết dọc, màu gáy riêng). Hai cuốn sát cuốn giữa xoay ít hơn nên còn thấy một phần bìa.
- Càng ra xa tâm sách **càng nhỏ dần đều cả chiều cao** → tổng thể tạo hình “con mắt”/thấu kính nằm ngang, đối xứng hai bên (như ảnh).
- Dưới hàng sách có **bóng đổ elip mờ** trên mặt sàn.
- Hai bên khu vực có **chữ trang trí cỡ rất lớn** dạng chấm bán sắc mờ (ảnh dùng chữ Nhật; ta dùng “TRI / THỨC” bên trái và “THƯ / VIỆN” bên phải, viết dọc theo khối), màu `--shelf-deco`, `aria-hidden="true"`.
- Bên dưới hàng sách: **tên cuốn đang ở giữa** cỡ lớn (44–56px, 700, màu `--shelf-title`), hai mũi tên ← → hai bên tên; dòng phụ chữ hoa giãn chữ (tác giả); một gạch ngắn; dòng meta chữ hoa nhỏ “NĂM XB — THỂ LOẠI”.
- Cuối khu vực: **thanh thời gian** ngang có vạch chia theo năm xuất bản (nhãn mỗi 5 năm), một chấm cho biết vị trí cuốn đang ở giữa.
- Bên phải: **hai nút tròn lên/xuống** (52px, nền xám bán trong suốt) để chuyển giữa các kệ (Sách mới về → Mượn nhiều nhất → từng thể loại), có chỉ báo tên kệ hiện tại.
- Nền khu vực `--shelf-bg` (#EFEBE3), cao `clamp(560px, 82vh, 780px)`.

## 2. Dữ liệu
`GET /api/cong/ke-sach?ke={moi|muon-nhieu|the-loai:{ma}}&gioiHan=41` trả danh sách đầu sách: `maDauSach, tenSach, tacGia, namXb, tenTheLoai, anhBia (nhỏ, lớn), mauGay, soBanCoTheMuon, daDatCho (của bạn đọc hiện tại)`. Kệ “Mượn nhiều” đếm thật từ CT_PHIEUMUON 90 ngày gần nhất. Thanh thời gian sắp theo `namXb` của danh sách hiện tại (kệ “Mượn nhiều” thì thanh thời gian hiển thị thứ hạng 1…n thay cho năm).

## 3. Cấu trúc DOM và 3D
```
section.coverflow [role=region][aria-roledescription=carousel][aria-label="Kệ sách nổi bật"]
  .coverflow__deco--left / --right (aria-hidden)
  .coverflow__stage (perspective: 1800px)
    ul.coverflow__track (transform-style: preserve-3d)
      li.book [data-index] > button.book__body (aria-label="Tên sách – tác giả")
        .book__front (ảnh bìa)   .book__spine (màu gáy + tên dọc)   .book__top (mặt trên, màu giấy)
  .coverflow__floor (bóng elip – radial-gradient, tĩnh)
  .coverflow__caption: button.prev  h3.title  button.next  p.author  hr  p.meta
  .coverflow__timeline (ticks + chấm)    .coverflow__shelfnav (nút lên/xuống + tên kệ)
  div[aria-live=polite].sr-only (đọc “Đang chọn: tên sách, tác giả”)
```
Mỗi cuốn là khối hộp CSS 3D: mặt trước rộng W, cao H (tỷ lệ 2:3), gáy dày T (lấy theo số trang nếu có, mặc định 0.16W) đặt ở `rotateY(-90deg)` cạnh trái. Không dùng WebGL/Three.js.

## 4. Thuật toán bố trí (một biến vị trí liên tục)
- Trạng thái chỉ có **một số thực `pos`** (chỉ số cuốn ở giữa, có thể lẻ trong lúc chuyển động). Hàm `layout(pos)` tính transform cho mọi cuốn trong cửa sổ hiển thị; GSAP tween `pos` (không tween từng cuốn) → chuyển nhiều bước vẫn liền mạch như ảnh.
- Với mỗi cuốn i, `d = i − pos`, `a = |d|`:
  - Độ lớn góc xoay: `|rotY| = lerp(0°, 58°, clamp(a,0,1))` khi a ≤ 1, rồi `lerp(58°, 82°, clamp(a−1,0,1))`. Chiều xoay mỗi phía chọn sao cho **mặt gáy hướng về người xem ở cả hai phía** và hai cuốn ±1 còn thấy một phần bìa như ảnh (nếu một phía cần, đặt thêm mặt gáy phụ ở cạnh còn lại của khối sách). Kiểm tra bằng ảnh chụp so với ảnh tham khảo trước khi tinh chỉnh.
  - `scale = max(S_MIN, 1 − K × a^0.85)` với `S_MIN = 0.42`, `K = 0.075` (sách nhỏ dần đều tạo hình thấu kính).
  - `x`: cộng dồn bề rộng hiển thị của các cuốn từ tâm ra: cuốn giữa chiếm W, cuốn kế cận chiếm `W×cos(rotY)×scale + T×scale`, khoảng cách 6px × scale. Tính bằng tích lũy theo phần nguyên và nội suy phần lẻ của `pos` để không giật.
  - **Cung cong lùi vào chiều sâu** (theo tham khảo 1, file 13): `z = −R × (1 − cos(min(a, aMax) × θ))` với `R ≈ 1.6W`, `θ ≈ 9°` mỗi bước (tham số trong `motion.config.js`), cộng `−a × 12px`; `zIndex = 1000 − round(a×10)`.
  - **Gáy tối dần về hai mép**: `filter` không được animate → lớp phủ `.book__shade` màu `#0B2A25` với `opacity = clamp(a×0.06, 0, 0.45)`.
- Chỉ dựng DOM cho cửa sổ ±20 cuốn quanh `pos` (ảo hóa); ẩn cuốn ngoài khung bằng `visibility`.
- Mọi transform ghi bằng `gsap.quickSetter` (x, rotationY, scale, z); không đọc layout trong vòng lặp; kích thước W, H, T đo **một lần** khi resize (debounce bằng `ResizeObserver`).

Số cuốn hiển thị mỗi bên: ≥1440px: 18 · 1280px: 15 · 1024px: 11 · 768px: 7 · <768px: 3 (mobile: cuốn giữa chiếm 46vw, mũi tên to 48px).

## 5. Tương tác
| Hành động | Kết quả |
|---|---|
| Bấm ← / → hoặc phím ←/→ khi khu vực có focus | `pos` ±1, `duration: motion.coverflow.step` |
| Cuộn chuột / vuốt touchpad ngang (GSAP Observer, chỉ khi con trỏ trong khu vực) | ±1 mỗi “nấc”, khóa 250ms giữa hai nấc; không chặn cuộn dọc trang |
| Kéo / vuốt (Pointer Events hoặc Draggable) | `pos` theo tay (1 cuốn = 0.6W px kéo); thả ra có quán tính rồi bám về số nguyên gần nhất |
| Bấm vào gáy một cuốn | Xoay cuốn đó về giữa; thời lượng `min(0.35 + 0.08×khoảng cách, 1.1)s` |
| Bấm cuốn đang ở giữa / Enter | Mở **Book Preview** (file 06): bìa cuốn giữa là điểm xuất phát của hiệu ứng mở sách |
| Hover gáy | Nâng `y −10px`, lớp sáng mép gáy; con trỏ “pointer” |
| Rê chuột trên cuốn giữa (chỉ thiết bị có chuột: `@media (hover:hover) and (pointer:fine)`) | **Con trỏ tròn 58px** nền `--forest` chữ trắng “Mở” đi theo chuột (GSAP `quickTo`, trễ mềm), ẩn con trỏ hệ thống trên vùng này; **bìa nghiêng theo chuột** tối đa ±6° (rotateX/rotateY), rời chuột trở về đứng thẳng |
| Bấm vạch năm trên thanh thời gian / kéo chấm | Nhảy tới cuốn đầu tiên của năm đó (tween `pos` một lần) |
| Nút lên / xuống | Đổi kệ: hàng sách cũ trượt `yPercent −12` + mờ dần, hàng mới vào từ dưới; tên kệ đổi bằng hiệu ứng lật chữ; giữ “yêu cầu mới nhất thắng” khi bấm liên tục |
| Đổi cuốn ở giữa | Tên sách: chữ cũ trượt lên + mờ, chữ mới trượt vào (chia ký tự, stagger 0.012s); tác giả, meta mờ chéo; chấm thanh thời gian trượt tới vị trí mới |
| Tự chạy | **Không** tự chạy (tránh gây khó chịu khi đọc tên sách) |

Trạng thái kệ dùng chung state machine (file 10): `KE_SAN_SANG → KE_DANG_XOAY → KE_SAN_SANG`, `KE_DOI_KE`, `MO_SACH`. Lệnh đến trong lúc đang xoay: cập nhật đích của tween hiện tại (không xếp hàng nhiều lệnh).

## 5b. Trên điện thoại (bắt buộc dùng tốt)
- Khu vực cao `min(100svh − header, 640px)`; cuốn giữa rộng 46vw (tối đa 220px); mỗi bên hiển thị 3–4 gáy; chữ trang trí hai bên thu nhỏ, mờ hơn hoặc ẩn khi < 400px.
- Vuốt ngang bằng Pointer Events (`touch-action: pan-y` để vẫn cuộn dọc trang được); quán tính + bám về cuốn gần nhất; vuốt mạnh nhảy tối đa 5 cuốn; chạm gáy → về giữa; chạm cuốn giữa → mở Book Preview.
- Không có con trỏ tròn, không nghiêng theo chuột; mũi tên 48px dễ chạm; thanh thời gian chuyển thành thanh trượt chạm được (vùng chạm cao 44px).
- Nút đổi kệ lên/xuống chuyển thành hàng “chip” tên kệ cuộn ngang phía trên khu vực.
- Kiểm thử trên mô phỏng iPhone 13/Pixel 7 trong Playwright và ít nhất một máy thật trước khi nghiệm thu.

## 6. Accessibility và giảm chuyển động
- Mỗi cuốn là `button` có `aria-label`; cuốn giữa có `aria-current="true"`; nút mũi tên có nhãn “Sách trước”, “Sách sau”; vùng `aria-live` đọc tên cuốn mới.
- Tab chỉ đi vào: mũi tên trái, cuốn giữa, mũi tên phải, thanh thời gian, nút đổi kệ (các cuốn khác `tabindex=-1`).
- `prefers-reduced-motion`: không xoay 3D khi chuyển; đổi cuốn bằng crossfade 0.2s; vẫn giữ bố cục gáy sách tĩnh.
- Không có JS: hiển thị lưới ảnh bìa đơn giản (progressive enhancement).

## 7. Hiệu năng và nghiệm thu
- [ ] Bố cục, tỷ lệ, cảm giác chuyển động khớp ảnh tham khảo (đặt ảnh chụp màn hình cạnh ảnh tham khảo trong báo cáo pha 5).
- [ ] Chuyển nhiều bước (bấm cuốn cách 8 vị trí) liền mạch, không giật, không nhấp nháy.
- [ ] ≥ 55 fps trung bình, < 5% khung hình > 33ms khi kéo liên tục (Chrome, CPU throttling 4×).
- [ ] Ảnh bìa dùng bản nhỏ cho các cuốn ngoài tâm, bản lớn chỉ cho cuốn giữa; tải trước 3 cuốn kế bên.
- [ ] Bấm liên tục mũi tên 10 lần/giây không kẹt trạng thái; đổi kệ trong lúc đang xoay không lỗi.
- [ ] Bàn phím, trình đọc màn hình, reduced motion đạt mục 6.
