# 09 – BIỂU ĐỒ THỐNG KÊ

## 1. Công nghệ chọn: Apache ECharts 5.6.0
Lý do chọn thay vì Chart.js hay vẽ tay:
- Hiệu ứng vào/cập nhật dữ liệu mượt sẵn có (`animationDuration`, `animationEasing`, `animationDelay` theo từng điểm, chuyển tiếp khi đổi dữ liệu), hợp yêu cầu giao diện chuyển động.
- Đủ loại cần dùng: line/area, bar (chồng, ngang), donut, gauge, heatmap lịch, funnel; `dataZoom` để kéo khoảng thời gian; tooltip giàu thông tin.
- Hai bộ dựng hình: **canvas** cho màn hình, **SVG** cho trang in báo cáo (in nét, không vỡ).
- Có theme JSON → làm theme trắng – đỏ (dashboard) và emerald (cổng) dùng chung.
- Có hỗ trợ accessibility (`aria`, decal/hoa văn phân biệt khi in đen trắng), xuất ảnh PNG/SVG.
- Chạy với JavaScript thuần, không cần framework; tải từ `wwwroot/lib/echarts/5.6.0/echarts.min.js`.

## 2. Quy ước
- Module `wwwroot/js/charts/`: `chart-theme.js` (đăng ký theme `thuvien-do` và `thuvien-emerald`), `chart-factory.js` (tạo biểu đồ, resize bằng `ResizeObserver`, hủy khi rời trang), mỗi biểu đồ một hàm `veXxx(el, duLieu)`.
- Theme `thuvien-do`: bảng màu `['#DC2626','#F87171','#7F1D1D','#FCA5A5','#525252','#A3A3A3','#B45309']`, nền trong suốt, lưới `#ECECEC`, chữ `#5C5C5C`, font Be Vietnam Pro, tooltip nền trắng viền `#ECECEC` bóng nhẹ.
- Định dạng số và ngày tiếng Việt: tự viết formatter (`1.234`, `1,2 triệu đ`, `dd/MM`).
- Hiệu ứng: lần đầu `animationDuration: 900`, `animationEasing: 'cubicOut'`, cột/điểm trễ dần `idx × 30ms`; cập nhật dữ liệu `animationDurationUpdate: 600`; tắt hết khi `prefers-reduced-motion`.
- Dữ liệu lấy từ API tổng hợp riêng (`/api/quan-tri/thong-ke/...`), tính bằng SQL ở server, có bộ lọc kỳ (chọn ngày). **Mỗi API có test so sánh với truy vấn kiểm chứng.**
- Mỗi biểu đồ có: tiêu đề, chú thích kỳ, trạng thái đang tải (skeleton), trạng thái rỗng (“Chưa có dữ liệu trong kỳ”), nút “Xem bảng số liệu” (bảng thay thế cho trình đọc màn hình) và “Tải ảnh”.

## 3. Danh sách biểu đồ

### Tổng quan N1 – Mượn trả sách
| Thành phần | Loại | Dữ liệu |
|---|---|---|
| KPI: Lượt mượn hôm nay · Đang mượn · Quá hạn · Đặt chỗ đang giữ (số sắp hết hạn) | thẻ số đếm lên | CT_PHIEUMUON, CT_PHIEUTRA, PHIEUDATCHO |
| Lượt mượn – trả 30 ngày | area 2 đường + dataZoom | ngaymuon, ngaytra |
| Top 10 đầu sách mượn nhiều | bar ngang | CT_PHIEUMUON ⨝ DAUSACH_BANSACH |
| Đặt chỗ theo kết quả trong tháng (Đã nhận / Hết hạn / Đã hủy) | donut | PHIEUDATCHO |
| Lượt mượn theo ngày trong năm | heatmap lịch | ngaymuon |

### Tổng quan N2 – Xử lý vi phạm
| Thành phần | Loại | Dữ liệu |
|---|---|---|
| KPI: Biên bản tháng này · Tổng tiền thu · Chưa thanh toán | thẻ số | BIENBAN |
| Vi phạm theo loại (quá hạn / mất / hư hỏng) theo tháng | bar chồng | CT_BIENBAN ⨝ KHOANTHU |
| Tiền thu theo tháng | line + area | BIENBAN.tong_tien |
| Hình thức thanh toán | donut | BIENBAN.ht_thanhtoan |

### Tổng quan N3 – Kho sách
| Thành phần | Loại | Dữ liệu |
|---|---|---|
| KPI: Tổng bản sách · Sẵn sàng · Đang được mượn · Hư hỏng/Mất | thẻ số | SACH |
| Bản sách theo tình trạng | donut | SACH.tinhtrang |
| Bản sách theo khu vực lưu trữ | bar | SACH ⨝ KHUVUC |
| Sách hư hỏng, mất theo tháng | bar | CT_PHIEUTRA.tinhtrang_tra |

### Tổng quan N4 – Quản trị
| Thành phần | Loại | Dữ liệu |
|---|---|---|
| KPI: Tài khoản hoạt động · Lượt đăng nhập hôm nay · Thao tác hôm nay · Lần sao lưu gần nhất | thẻ số | NGUOIDUNG, NHATKY_* |
| Lượt đăng nhập 30 ngày | line | NHATKY_DANGNHAP |
| Thao tác theo nhóm người dùng | bar chồng | NHATKY_HOATDONG ⨝ NGUOIDUNG |

### Báo cáo MB06, MB07 (SPEC 6.4.5–6.4.6)
- Trang báo cáo: bộ lọc kỳ → nút Tổng hợp (01.04.02 / 02.05.01) → hiển thị bảng tổng hợp + biểu đồ (MB06: cột lượt mượn/gia hạn/trả theo ngày trong kỳ; MB07: bar chồng vi phạm theo loại + donut tỷ lệ) → nút Lập báo cáo (01.04.03 / 02.05.02) mở trang in, biểu đồ dựng bằng **SVG renderer**, tắt animation khi in.
- Các con số trong báo cáo đúng công thức SPEC 6.4 (có test).

### Cổng bạn đọc (theme emerald, tiết chế)
- Khu “Thư viện bằng số”: chỉ số đếm lên, không cần biểu đồ.
- Trang cá nhân: một biểu đồ nhỏ “Số sách đã mượn theo tháng (12 tháng)” dạng cột bo góc.
