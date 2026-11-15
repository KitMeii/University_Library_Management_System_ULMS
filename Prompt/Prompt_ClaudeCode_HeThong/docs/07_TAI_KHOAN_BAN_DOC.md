# 07 – TÀI KHOẢN BẠN ĐỌC VÀ TRANG CÁ NHÂN

## 1. Tạo và kích hoạt tài khoản
- Khi nhân viên N1 lưu thẻ bạn đọc mới (01.01.01, SPEC 6.1.3), hệ thống tạo `TAIKHOAN_BANDOC` trạng thái “Chưa kích hoạt” trong cùng transaction (mở rộng – không đổi giao diện 01.01.01, chỉ thêm dòng thông báo “Đã tạo tài khoản cổng bạn đọc, chờ kích hoạt”).
- `/kich-hoat`: bạn đọc nhập `ma_svhvcb` + `so_thebd` + `ngaysinh` (chọn ngày) → nếu khớp BANDOC và thẻ còn hạn → đặt mật khẩu (≥ 8 ký tự, có chữ và số, nhập lại) → “Hoạt động”. Sai 5 lần trong 15 phút → tạm khóa kích hoạt 15 phút.
- Quên mật khẩu: lặp lại bước kích hoạt để đặt lại (không có email trong CSDL gốc; nếu `dienthoai_email` là email hợp lệ thì có thể gửi mã – chỉ làm khi chủ dự án duyệt).
- N4 có thể khóa/mở khóa tài khoản bạn đọc ở Danh mục Bạn đọc (thêm cột tình trạng tài khoản, mở rộng).

## 2. Đăng nhập
- `/dang-nhap`: `ma_svhvcb` + mật khẩu; cookie scheme **`BanDoc`** tách biệt cookie nhân viên; “Ghi nhớ” tối đa 14 ngày; sai 5 lần → khóa 15 phút (`tg_khoa_den`).
- Hộp đăng nhập nhúng trong Book Preview dùng cùng API, không rời trang.
- Đăng nhập thành công: cập nhật `tg_dangnhap_cuoi`; quay lại đúng ngữ cảnh trước đó.

## 3. Trang cá nhân `/tai-khoan` (phong cách kính mờ trên nền pastel – file 03 mục C)
Bố cục: thanh viên thuốc dọc bên trái (mobile: thanh tab dưới đáy) + lưới thẻ kính. Thẻ vào trang lần lượt (stagger), hover nâng 5px. Mỗi thẻ có nhãn loại màu pastel và thanh tiến độ mảnh khi có thời hạn.
| Trang | Nội dung (chỉ đọc dữ liệu gốc) |
|---|---|
| Tổng quan | Lời chào, thẻ bạn đọc (họ tên, mã, đối tượng, lớp/đơn vị, hạn thẻ), 4 số: đang mượn, sắp đến hạn (≤ 3 ngày), quá hạn, đặt chỗ đang giữ; nhắc việc cần làm |
| Sách đang mượn | Từ CT_PHIEUMUON chưa có trong CT_PHIEUTRA: tên sách, ngày mượn, hạn trả (đã tính gia hạn từ CT_PHIEUGIAHAN), số ngày còn lại / quá hạn (màu) |
| Đặt chỗ | Theo file 06 mục 6 |
| Lịch sử | Phiếu mượn, gia hạn, trả của tôi (lọc theo năm) |
| Vi phạm và khoản phải nộp | BIENBAN + CT_BIENBAN của tôi: nội dung, số tiền, tình trạng thanh toán |
| Yêu thích | Lưới sách yêu thích (YEUTHICH) |
| Thông báo | Danh sách THONGBAO, đánh dấu đã đọc |
| Đổi mật khẩu | |

Bạn đọc **không** tự gia hạn, tự trả hay tự thanh toán trên cổng (các nghiệp vụ này do nhân viên thực hiện theo thiết kế BT4).

## 4. Thông báo
- Chuông trên header cổng: số chưa đọc, danh sách thả xuống 10 thông báo mới nhất, liên kết tới đúng chỗ.
- `NhacHanService` mỗi ngày 7:00 tạo THONGBAO “Sắp đến hạn trả” (còn 2 ngày) và “Quá hạn trả”.

## 5. Nghiệm thu
- [ ] Không kích hoạt được với thông tin sai; khóa tạm đúng quy định.
- [ ] Cookie bạn đọc không truy cập được bất kỳ route `/quan-tri` hay `/api/quan-tri` nào (403) và ngược lại.
- [ ] Số liệu trang cá nhân khớp truy vấn kiểm chứng trên dữ liệu mẫu.
