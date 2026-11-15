# 06 – BOOK PREVIEW (MỞ SÁCH) VÀ NGHIỆP VỤ ĐẶT CHỖ 24 GIỜ

## 1. Book Preview – nơi duy nhất xem thông tin sách
- Mọi điểm vào (cuốn giữa kệ coverflow, Book Card, kết quả tìm kiếm, yêu thích, thông báo, liên kết `/sach/{maDauSach}`) đều mở **cùng một Book Preview**. Không làm trang chi tiết riêng, không dùng modal thông tin thay cho mở sách.
- Hiệu ứng: bìa bay từ vị trí nguồn (GSAP Flip) ra giữa màn hình, lớp nền tối dần (rgba(11,42,37,.55)), bìa mở như cuốn sách thật (bìa trước quay quanh gáy `rotateY 0 → −180°`), lật nhanh 2–3 tờ giấy, dừng ở **hai trang giấy** nền `--paper`. Tổng thời gian theo `motion.preview.open` (mặc định 1.6s; giảm chuyển động: mờ dần 0.25s).
- Lật trang dùng **StPageFlip** (file 13): desktop hiển thị 2 trang; **điện thoại hiển thị 1 trang, vuốt để lật** (trang 1: bìa + thông tin; trang 2: giới thiệu + khối hành động – nút Đặt chỗ luôn ghim ở đáy màn hình trên điện thoại).
- **Trang trái:** ảnh bìa lớn, tên sách, tác giả, nhà xuất bản, năm xb, ISBN, thể loại, từ khóa (nút nhỏ → tìm theo từ khóa).
- **Trang phải:** giới thiệu (cuộn trong trang nếu dài); tình trạng (“Còn N bản có thể mượn”, khu vực: Kệ mở/Kho theo KHUVUC của các bản); **khối hành động**: nút **Đặt chỗ** (chính), nút Yêu thích, nút Chia sẻ liên kết; dòng quy định “Giữ sách 24 giờ – nhận tại quầy mượn trả”.
- Đóng: nút ×, phím Esc, bấm nền, nút Back của trình duyệt (dùng `history.pushState` với `/sach/{maDauSach}`); khi đóng, sách gập lại và bay về đúng vị trí nguồn (đo lại vị trí nguồn lúc đóng). Focus bị giữ trong preview khi mở, trả về phần tử nguồn khi đóng.

## 2. Quy tắc đặt chỗ (Backend quyết định, tham số trong `appsettings` `ThuVien:DatCho`)
| Tham số | Mặc định |
|---|---|
| `SoGioGiu` | 24 |
| `ToiDaDangGiu` | 3 đặt chỗ “Đang giữ” cùng lúc |
| `NhacTruocGio` | 2 (gửi thông báo khi còn 2 giờ) |
| `ChuKyQuetPhut` | 5 |

Điều kiện được đặt chỗ (mỗi điều kiện sai trả mã lỗi riêng và câu thông báo tiếng Việt cụ thể):
1. Đã đăng nhập bằng tài khoản bạn đọc trạng thái “Hoạt động”.
2. Thẻ còn hạn: `hsd_tungay ≤ hôm nay ≤ hsd_denngay`.
3. Không có sách quá hạn chưa trả; không có biên bản `ht_thanhtoan = 'Chưa thanh toán'`.
4. `số bản đang mượn + số đặt chỗ đang giữ < số bản tối đa theo doituong` (SPEC mục 4).
5. Số đặt chỗ đang giữ < `ToiDaDangGiu`; chưa giữ một bản khác của cùng đầu sách.
6. Đầu sách còn ít nhất 1 bản `SACH.tinhtrang = 'Sẵn sàng'` và **không** đang bị giữ chỗ.

Xử lý khi đặt (một transaction, `SERIALIZABLE` hoặc `UPDLOCK, HOLDLOCK`): chọn bản sách phù hợp (ưu tiên khu vực Kệ mở, rồi mã nhỏ nhất) → tạo `PHIEUDATCHO` trạng thái “Đang giữ”, `tg_hethan = tg_dat + 24h` → tạo THONGBAO “Đặt chỗ thành công” → trả về mã đặt chỗ, hạn giữ, vị trí quầy. Nếu chỉ mục duy nhất báo trùng (người khác vừa giữ) → thử bản khác; hết bản → HTTP 409 “Sách vừa được người khác đặt”.
**Không đổi `SACH.tinhtrang`** khi đặt chỗ (giữ nguyên 18 bảng gốc); số bản có thể mượn = bản “Sẵn sàng” trừ bản đang giữ.

## 3. Vòng đời đặt chỗ
```
            đặt chỗ                      nhân viên giao sách (Lập Phiếu mượn)
 (không) ───────────► ĐANG GIỮ ──────────────────────────────────────► ĐÃ NHẬN
                         │  bạn đọc hủy / nhân viên hủy (ghi lý do)
                         ├──────────────────────────────────────────► ĐÃ HỦY
                         │  quá tg_hethan (tác vụ nền)
                         └──────────────────────────────────────────► HẾT HẠN
```
- **Tác vụ nền `DatChoHetHanService`** chạy mỗi `ChuKyQuetPhut`: chuyển “Đang giữ” có `tg_hethan < bây giờ` sang “Hết hạn” (`tg_xuly`), tạo THONGBAO “Hết hạn giữ”. Đồng thời mọi truy vấn số bản có thể mượn đều coi đặt chỗ quá hạn là đã hết (không phụ thuộc tác vụ nền chạy đúng giờ).
- `NhacHanService`: khi còn ≤ `NhacTruocGio` giờ, gửi THONGBAO “Sắp hết hạn giữ” (một lần).
- Mọi thời điểm qua `IClock` để kiểm thử tua thời gian.

## 4. Tích hợp vào giao diện nghiệp vụ 01.01.03 Lập Phiếu mượn sách (đánh dấu “mở rộng” trong mã)
- Sau `btnTimBanDoc` (trạng thái 2), nếu bạn đọc có đặt chỗ “Đang giữ”: hiện khối **“Sách đặt chỗ đang giữ”** (`dgvDatCho`: mã đặt chỗ, tên sách, mã bản sách, vị trí, hạn giữ còn lại) và nút **`btnNhanDatCho`** “Đưa sách đặt chỗ vào phiếu”.
- `btnNhanDatCho`: thêm các bản sách đang giữ vào `dgvChiTiet` như khi `btnThemSach` (vẫn qua toàn bộ kiểm tra của 01.01.03); khi lưới có dòng → trạng thái 3.
- `btnThemSach` với một bản sách đang được giữ cho **người khác** → từ chối: “Bản sách đang được giữ chỗ cho bạn đọc khác đến HH:mm dd/MM”.
- `btnIn` (lưu phiếu): trong cùng transaction, các đặt chỗ có bản sách nằm trong phiếu chuyển “Đã nhận”, ghi `so_pmuon`, `ma_nd_xuly`, `tg_xuly`; THONGBAO “Đã nhận sách”.

## 5. Trang “Quầy đặt chỗ” cho N1 (`/quan-tri/dat-cho`, mở rộng)
- Danh sách “Đang giữ” sắp theo hạn gần nhất: mã đặt chỗ, bạn đọc (`hoten`, `ma_svhvcb`), tên sách, mã bản sách, khu vực/vị trí, đếm ngược (đỏ khi < 2 giờ).
- Bộ lọc trạng thái, ngày; tìm theo `ma_svhvcb`.
- Nút **In phiếu soạn sách** (danh sách bản sách cần lấy từ kệ/kho, A4) để chuẩn bị trước.
- Nút **Hủy đặt chỗ** (bắt buộc nhập lý do, xác nhận). Không có nút “Nhận” ở đây: nhận sách chỉ qua Lập Phiếu mượn (đúng nghiệp vụ đã thiết kế).
- Chuông topbar dashboard hiển thị số đặt chỗ sắp hết hạn.

## 6. Giao diện đặt chỗ phía bạn đọc
- Bấm **Đặt chỗ** trong Book Preview: nút chuyển trạng thái đang xử lý (vòng tròn nhỏ) → thành công: trang phải hiện **vé giữ chỗ** dạng cuống vé (mã đặt chỗ, mã vạch Code128 của mã đặt chỗ vẽ bằng SVG, hạn giữ, đồng hồ đếm ngược, quầy nhận), hiệu ứng tem đóng dấu “ĐÃ GIỮ CHỖ” + tia nhỏ; nút “Hủy đặt chỗ”.
- Chưa đăng nhập: bấm Đặt chỗ → hộp đăng nhập ngay trong trang sách (giữ ngữ cảnh), đăng nhập xong tự tiếp tục đặt chỗ.
- Không đủ điều kiện: hiển thị lý do cụ thể ngay trên trang sách và hướng xử lý (ví dụ “Bạn đang có 1 sách quá hạn – vui lòng trả sách trước”).
- Trang `/tai-khoan/dat-cho`: thẻ từng đặt chỗ với đếm ngược, trạng thái, lịch sử.

## 7. API
| Phương thức | Đường dẫn | Mô tả |
|---|---|---|
| GET | `/api/cong/sach/{maDauSach}` | Dữ liệu Book Preview + số bản có thể mượn + đặt chỗ của tôi |
| POST | `/api/cong/dat-cho` `{maDauSach}` | Đặt chỗ (AntiForgery) |
| DELETE | `/api/cong/dat-cho/{maDatCho}` | Bạn đọc hủy |
| GET | `/api/cong/dat-cho/cua-toi` | Danh sách của tôi |
| GET | `/api/quan-tri/dat-cho?trangThai=&tuNgay=&denNgay=` | Quầy đặt chỗ (quyền `DATCHO_R`) |
| POST | `/api/quan-tri/dat-cho/{ma}/huy` `{lyDo}` | Nhân viên hủy (`DATCHO_GIAO`) |

## 8. Nghiệm thu
- [ ] Hai bạn đọc cùng đặt bản cuối cùng của một đầu sách cùng lúc (test song song 20 request): đúng 1 thành công, các request khác nhận 409.
- [ ] Tua `IClock` qua 24 giờ: đặt chỗ thành “Hết hạn”, bản sách trở lại có thể mượn, có thông báo.
- [ ] Nhận tại quầy qua Lập Phiếu mượn: đặt chỗ “Đã nhận”, có `so_pmuon`; nhân viên không thể cho người khác mượn bản đang giữ.
- [ ] Mọi điều kiện ở mục 2 có test riêng và thông báo đúng.
- [ ] Book Preview mở/đóng từ mọi điểm vào, Back trình duyệt đóng sách, liên kết sâu hoạt động.
