# 00 – KẾ HOẠCH TỔNG THỂ (ĐỌC FILE NÀY ĐẦU TIÊN)

Hệ thống: **Quản lý mượn trả sách thư viện đại học**. Đây là hệ thống dùng thật, không phải bản demo.

## 1. Hệ thống gồm hai mặt

| Mặt | Người dùng | Mục đích | Phong cách |
|---|---|---|---|
| **Cổng bạn đọc** (`/`) | Sinh viên, học viên, cán bộ (bảng BANDOC) | Xem kệ sách, tìm sách, mở xem sách, **đặt chỗ** (giữ sách 24 giờ), theo dõi sách đang mượn, đặt chỗ, thông báo | Academic Emerald, nhiều chuyển động (file 03, 04, 05, 06) |
| **Hệ thống nghiệp vụ** (`/quan-tri`) | Nhân viên N1–N4 (bảng NGUOIDUNG) | Toàn bộ chức năng đã thiết kế trong BT1–BT4: danh mục, lập chứng từ, in, báo cáo, quản trị | Dashboard **trắng – đỏ** (file 03, 08) |

## 2. Bộ tài liệu và vai trò

| File | Nội dung | Ghi chú |
|---|---|---|
| `SPEC_NGHIEP_VU_BT4.md` | **Nguồn chuẩn** về CSDL 18 bảng, phân quyền 2.1–2.3, 13 giao diện nghiệp vụ, 6 mẫu in | Không được làm trái |
| `01_NGUYEN_TAC_VA_CONG_NGHE.md` | Nguyên tắc bất biến, công nghệ, phiên bản, cấu trúc solution | Đọc kèm mọi pha |
| `02_CSDL_VA_MO_RONG.md` | Ánh xạ 18 bảng sang SQL Server + **bảng mở rộng** (đặt chỗ, tài khoản bạn đọc, đầu sách, thể loại, yêu thích, thông báo) | Mở rộng chỉ có trong hệ thống, không đưa vào hồ sơ Word |
| `03_DESIGN_SYSTEM.md` | Token màu, chữ, thành phần cho cả hai mặt | |
| `04_TRANG_CHU_CONG_BAN_DOC.md` | Bố cục trang chủ và các trang cổng bạn đọc | |
| `05_KE_SACH_COVERFLOW.md` | Hiệu ứng kệ sách xoay theo ảnh `tham_khao/hieu_ung_ke_sach_coverflow.png` | Phần quan trọng nhất về thị giác |
| `06_BOOK_PREVIEW_VA_DAT_CHO.md` | Mở sách xem thông tin, nghiệp vụ **đặt chỗ 24 giờ** | |
| `07_TAI_KHOAN_BAN_DOC.md` | Kích hoạt tài khoản, đăng nhập, trang cá nhân của bạn đọc | |
| `08_DASHBOARD_NGHIEP_VU.md` | Hệ thống nghiệp vụ N1–N4 theo SPEC + quầy đặt chỗ | |
| `09_BIEU_DO_THONG_KE.md` | Công nghệ biểu đồ (Apache ECharts) và danh sách biểu đồ | |
| `10_CHUYEN_DONG_VA_HIEU_UNG.md` | `motion.config.js`, chuyển trang, micro-interaction | |
| `11_BAO_MAT_HIEU_NANG_KIEM_THU.md` | Bảo mật, hiệu năng, kiểm thử, nghiệm thu, tài liệu | |
| `12_BASECORE_MICROSERVICE.md` | **Làm trực tiếp trên thư mục `basecore/` (microservice có sẵn)**: khảo sát, xóa phần thừa, chia service, sự kiện | Quy ước basecore thay cho phần backend của file 01 |
| `14_DOCKER.md` | Đóng gói mọi service bằng Docker, `docker compose` cho dev / kiểm thử / triển khai, sao lưu trong container | Chạy toàn hệ thống bằng một lệnh |
| `13_MA_NGUON_MO_THAM_KHAO.md` | Thư viện mã nguồn mở được dùng, nguồn tham khảo hiệu ứng, cách áp dụng 2 nguồn tham khảo của chủ dự án | |

Khi các file mâu thuẫn: **SPEC_NGHIEP_VU_BT4.md > 02 > quy ước của basecore (file 12) > 01 > các file còn lại**. Nếu vẫn chưa rõ: hỏi chủ dự án, không tự quyết.

## 3. Cách làm việc bắt buộc với Claude Code

1. **Pha 0A rồi Pha 0 – Khảo sát basecore và lập kế hoạch (chế độ Plan):** đọc toàn bộ thư mục `docs/` (kể cả ảnh tham khảo). Trình kế hoạch gồm: cấu trúc solution và project; danh sách entity, migration; danh sách Controller/Action/route và mã quyền kiểm tra; danh sách View; danh sách module JS/CSS; cách triển khai từng phần ở mục 4; rủi ro; **danh sách câu hỏi cần chủ dự án trả lời**. Chưa viết code khi chưa được duyệt.
2. Làm **từng pha** ở mục 4, theo đúng thứ tự. Mỗi pha: đọc lại các file được ghi ở cột “Đọc”, làm, build, chạy toàn hệ thống bằng `docker compose up -d --build` (migration chạy qua service `migrator`), chạy kiểm thử của pha, tự kiểm tra trực quan bằng Playwright (chụp ảnh), sửa lỗi, rồi báo cáo theo mẫu ở mục 5 và **dừng chờ duyệt**.
3. Không sang pha sau khi pha trước còn lỗi nghiêm trọng. Không đánh dấu hoàn thành chức năng còn chạy bằng dữ liệu giả.
4. Ghi mọi quyết định đã duyệt vào `docs/GIA_DINH.md`; ghi tiến độ vào `docs/TIEN_DO.md` (pha, việc đã xong, việc còn lại) để phiên làm việc sau đọc tiếp được.

## 4. Các pha thực hiện

| Pha | Nội dung | Đọc | Kết quả nghiệm thu chính |
|---|---|---|---|
| 0A | **Khảo sát basecore** (chỉ đọc): lập `docs/BASECORE_PHAN_TICH.md`, gắn nhãn GIỮ/TÁI DÙNG/XÓA/CHƯA RÕ | 12 | Bản phân tích + danh sách XÓA được duyệt |
| 0 | Lập kế hoạch trên nền basecore (ranh giới service, CSDL chung hay riêng, sự kiện, frontend) | Tất cả | Kế hoạch được duyệt |
| 1 | Nền tảng: xóa phần thừa của basecore (commit riêng), dựng/đổi tên các service theo file 12, **Dockerfile + docker-compose (file 14) chạy được ngay từ pha này**, EF Core + SQL Server, 18 bảng + bảng mở rộng, migration, dữ liệu khởi tạo (nhóm, quyền, tài khoản quản trị), dữ liệu mẫu Development, xác thực 2 cơ chế (nhân viên / bạn đọc), phân quyền theo PHANQUYEN, layout hai mặt, design tokens, `motion.config.js` | 01, 02, 03, 10, 11, SPEC mục 2–3 | Đăng nhập được hai loại tài khoản; menu nhân viên đúng nhóm; test schema khớp SPEC mục 2 |
| 2 | Dashboard: 4 giao diện danh mục (SPEC 6.1) + 5 giao diện quản trị và đăng nhập (SPEC 6.3) + quản lý đầu sách, thể loại (mở rộng) | 08, SPEC 6.1, 6.3 | CRUD đúng quyền 2.2; trạng thái nút đúng đặc tả; nhật ký ghi đủ |
| 3 | Dashboard: 4 giao diện lập chứng từ (SPEC 6.2), in MB02–MB05, tích hợp **nhận sách đặt chỗ** vào Lập Phiếu mượn, trang Quầy đặt chỗ | 06 mục 4–5, 08, SPEC 6.2, 6.4 | Luồng mượn → gia hạn → trả → biên bản chạy đúng, 3 trạng thái form đúng |
| 4 | Báo cáo MB06, MB07 + trang tổng quan dashboard từng nhóm với biểu đồ | 08, 09, SPEC 6.4 | Số liệu biểu đồ khớp truy vấn kiểm chứng |
| 5 | Cổng bạn đọc: header, footer, hero, **kệ sách coverflow** | 03, 04, 05, 10 | Hiệu ứng giống ảnh tham khảo, 60 fps trên máy tầm trung |
| 6 | Book Preview + **đặt chỗ 24 giờ** + tài khoản bạn đọc + thông báo + tác vụ nền hủy đặt chỗ quá hạn | 06, 07 | Đặt – nhận – hủy – hết hạn đúng quy tắc, không giữ trùng bản sách |
| 7 | Trang chủ hoàn chỉnh: danh mục, khám phá/tìm kiếm sách, hướng dẫn, số liệu, thông tin thư viện | 04, 10 | Mọi khu vực dùng dữ liệu thật |
| 8 | Hoàn thiện: chuyển động, responsive (đặc biệt **mobile**), accessibility, bảo mật, hiệu năng, kiểm thử E2E **chạy trong Docker**, `docker-compose.prod.yml`, chụp ảnh giao diện, tài liệu | 10, 11, 12, 13, 14 | Đạt toàn bộ mục nghiệm thu ở file 11 |

## 5. Mẫu báo cáo sau mỗi pha

```
PHA n – <tên pha>
- Đã làm:
- Chưa làm / còn dữ liệu giả:
- Kiểm thử đã chạy và kết quả:
- Ảnh chụp kiểm tra (đường dẫn):
- Số đo chuyển động (nếu có): fps trung bình, % khung hình > 33 ms
- Lỗi còn tồn tại:
- Thông số motion.config.js đã chỉnh và lý do:
- Câu hỏi / đề xuất cần chủ dự án duyệt:
```

## 6. Định nghĩa “hoàn thành” của toàn hệ thống

- Toàn bộ chức năng trong SPEC chạy đúng trên dữ liệu thật, đúng phân quyền, đúng trạng thái nút.
- Bạn đọc xem sách, đặt chỗ, nhận sách tại quầy trong 24 giờ; quá 24 giờ hệ thống tự hủy và trả bản sách về trạng thái có thể mượn.
- Cổng bạn đọc có hiệu ứng kệ sách như ảnh tham khảo và chuyển động mượt; dashboard trắng – đỏ có biểu đồ thống kê động.
- Kiểm thử tự động đạt; có README, hướng dẫn sử dụng, hướng dẫn triển khai; ảnh chụp mọi giao diện theo số mục báo cáo.
