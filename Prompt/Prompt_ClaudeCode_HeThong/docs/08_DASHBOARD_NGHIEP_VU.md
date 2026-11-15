# 08 – HỆ THỐNG NGHIỆP VỤ (DASHBOARD TRẮNG – ĐỎ) CHO N1–N4

## 1. Phạm vi
Hiện thực **toàn bộ** giao diện ở `SPEC_NGHIEP_VU_BT4.md` mục 6 trong Area `QuanTri`, đúng: người sử dụng, tiền/hậu điều kiện, bảng dữ liệu, điều kiện toàn vẹn, **từng control và xử lý (giữ nguyên tên control làm `id`)**, trạng thái nút, kết quả đầu ra. Cộng phần mở rộng: Quầy đặt chỗ (file 06 mục 5), Quản lý đầu sách và thể loại (file 02 X01–X03).

| Nhóm menu (sidebar) | Mục | SPEC | Quyền |
|---|---|---|---|
| Tổng quan | Trang tổng quan theo nhóm | file 09 | mọi nhóm |
| Mượn trả sách (N1) | 01.01.01 Lưu thông tin Thẻ Bạn đọc · 01.01.03 Lập Phiếu mượn sách · 01.02.01 Lập Phiếu gia hạn sách · 01.02.02 Lập Phiếu trả sách · Quầy đặt chỗ | 6.1.3, 6.2.1–6.2.3 | N1 |
| Xử lý vi phạm (N2) | 02.03.01 Lập Biên bản vi phạm mượn trả sách | 6.2.4 | N2 |
| Kho sách (N3) | Danh mục sách (thực hiện 03.01.02, 03.01.04, 03.02.03, 03.03.02, 03.04.01) · Danh mục khu vực lưu trữ · Đầu sách và thể loại | 6.1.1, 6.1.4 | N3 |
| Thống kê | 01.04.02/01.04.03 Báo cáo tình hình mượn trả sách (N1) · 02.05.01/02.05.02 Báo cáo tình hình vi phạm (N2) | 6.4.5, 6.4.6 | N1, N2 |
| Danh mục | Danh mục sách · khoản thu · Bạn đọc · khu vực lưu trữ (chỉ xem hoặc đầy đủ tùy quyền 2.2) | 6.1 | theo 2.2 |
| Quản trị (N4) | 04.00.01 Tài khoản · 04.00.02 Cấp quyền · 04.00.03/04.00.08 Nhật ký · 04.00.09 Sao lưu – phục hồi · 04.00.04–07 (dùng các trang danh mục) | 6.3 | N4 |

Menu chỉ hiện mục người dùng có quyền (SPEC 3.3); route và API kiểm tra lại quyền (SPEC 3.2, 3.4). Sau đăng nhập mở đúng màn hình mặc định của nhóm (SPEC 3.3).

## 2. Khung trang dashboard
- `_LayoutQuanTri.cshtml`: sidebar trắng có nhóm menu thu gọn được, topbar trắng (logo đỏ “THƯ VIỆN”, tìm nhanh, chuông, người dùng), breadcrumb, vùng nội dung.
- Mỗi trang nghiệp vụ: tiêu đề = “Tên giao diện” trong SPEC; thanh bước trạng thái (form chứng từ); thẻ nhập liệu; lưới; hàng nút.
- Hộp thoại đăng nhập SPEC 6.3.1 là trang riêng `/quan-tri/dang-nhap` (nền trắng, khối đỏ bên trái có tên thư viện và hình minh họa sách đường nét).

## 3. Thực hiện form nghiệp vụ trên web (quy ước chuyển từ đặc tả kiểu WinForms)
- `txt…` → `<input>`, `cbo…` → `<select>` có tìm kiếm, `dtp…` → bộ chọn ngày, `dgv…` → DataGrid (phân trang server, chọn dòng, double-click = sự kiện “double click vào dòng” trong SPEC), `btn…` → `<button>`, `rdo…` → radio, `tab…` → tab, `frm… (Load)` → khởi tạo trang.
- **Trạng thái nút đúng từng dòng “Enable = True/False” trong SPEC**: viết một module `form-state.js` khai báo trạng thái dưới dạng dữ liệu (ví dụ `TT1: { bat: ['txtMaSV','btnTimBanDoc','btnTimPhieu'], tat: [...] }`) và một hàm chuyển trạng thái duy nhất; kiểm thử E2E xác nhận từng trạng thái.
- Mỗi nút gọi **một** API tương ứng một tiến trình hệ thống (đặt tên Action theo tiến trình, ví dụ `LapPhieuMuon_LuuVaIn`). Server kiểm tra lại toàn bộ điều kiện toàn vẹn của SPEC.
- “In” (btnIn): lưu trong transaction rồi mở trang in (`/quan-tri/in/phieu-muon/{so}`) theo mẫu SPEC 6.4 (A5/A4, `@media print`), sau đó form về trạng thái 1.
- Danh mục sách (6.1.1) có danh sách “sách chuyển đến” (`dgvChuyenDen`) để N3 thực hiện các tiến trình 03.xx đúng như SPEC.

## 4. Trang tổng quan theo nhóm
Thẻ KPI + biểu đồ theo file 09. Thẻ KPI bấm được → mở danh sách chi tiết tương ứng (ví dụ “Sách quá hạn” → danh sách bạn đọc quá hạn).

## 5. Nghiệm thu
- [ ] Mỗi giao diện SPEC 6.1–6.3 có đủ control đúng `id`, đúng trạng thái nút từng bước (test E2E đọc bảng trạng thái từ SPEC).
- [ ] Mỗi thao tác ghi đúng bảng ở “Kết quả đầu ra” và một dòng NHATKY_HOATDONG.
- [ ] Đăng nhập lần lượt N1–N4: chỉ thấy menu của mình; gọi API ngoài quyền → 403.
- [ ] Mẫu in MB02–MB07 đúng khổ giấy, đủ trường, đúng công thức SPEC 6.4.
