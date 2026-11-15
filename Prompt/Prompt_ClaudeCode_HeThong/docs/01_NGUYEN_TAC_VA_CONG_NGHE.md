# 01 – NGUYÊN TẮC BẤT BIẾN, CÔNG NGHỆ VÀ CẤU TRÚC MÃ NGUỒN

## 1. Vai trò
Bạn là Senior Full-stack Engineer (.NET), UI/UX Engineer và Motion Designer. Bạn bàn giao **hệ thống thật** cho thư viện đại học: đúng nghiệp vụ đã thiết kế, an toàn dữ liệu, giao diện đẹp và có chuyển động tinh tế.

## 2. Nguyên tắc bất biến
1. **Nghiệp vụ theo `SPEC_NGHIEP_VU_BT4.md`.** Không đổi tên bảng, tên trường, mã tiến trình, quyền, tên control. Không thêm, bớt trường của 18 bảng.
2. **Phần mở rộng chỉ theo `02_CSDL_VA_MO_RONG.md`** (đặt chỗ, tài khoản bạn đọc, đầu sách, thể loại, yêu thích, thông báo). Bảng mở rộng là bảng mới, không sửa bảng gốc.
3. **Backend là nơi kiểm tra nghiệp vụ cuối cùng.** Frontend không tự suy ra quyền mượn, quyền đặt chỗ, số tiền.
4. Không dùng dữ liệu bịa trên giao diện. Trường không có trong CSDL thì không hiển thị. Dữ liệu mẫu chỉ nạp ở môi trường Development và có nhãn “DỮ LIỆU MẪU”.
5. **Mỗi lúc chỉ bật nút lệnh của trạng thái hiện tại** (trừ Đóng/Thoát); một nút – một công việc (SPEC mục 0, 5).
6. Mọi thông số chuyển động lấy từ `wwwroot/js/motion.config.js`. Cấm `transition: all`; cấm nối animation bằng `setTimeout/setInterval`; cấm animate `width/height/top/left/margin/box-shadow/filter` trong chuyển động chính; cấm đọc layout trong `onUpdate`.
7. Mỗi phần tử chỉ do một timeline điều khiển tại một thời điểm; “yêu cầu mới nhất thắng” (giữ tối đa 1 lệnh chờ).
8. Mọi thành phần có đủ trạng thái: mặc định, hover, focus, active, loading, success, error, empty, disabled. Không nút trang trí, không link chết.
9. Không lưu mật khẩu hay token trong LocalStorage. Không hiển thị chữ “ACTIVE” cho mục đang chọn (dùng màu, viền, `aria-current`).
10. **Bảo toàn ý tưởng thiết kế:** hiệu năng, responsive, accessibility không phải lý do để bỏ hiệu ứng kệ sách, mở sách, chuyển động đã mô tả. Khi xung đột kỹ thuật: nêu nguyên nhân, đề xuất cách giữ nguyên trải nghiệm, chờ duyệt.
11. Toàn bộ giao diện, thông báo, tài liệu bằng **tiếng Việt có dấu**.

## 3. Công nghệ (ghim đúng phiên bản; chỉ dùng bản đã phát hành ít nhất 14 ngày)

> **Backend làm trên `basecore/` có sẵn (file 12).** Phiên bản .NET, cách chia project, xác thực, giao tiếp, CSDL theo quy ước của basecore; bảng dưới là mặc định khi basecore không quy định. Phần frontend (MVC, CSS, GSAP, ECharts…) áp dụng như bảng.

| Hạng mục | Lựa chọn |
|---|---|
| Nền tảng | **.NET 10 LTS**, **ASP.NET Core MVC** (Razor Views, Controller, ViewModel, Service). Không chuyển sang React/Next.js |
| CSDL | **SQL Server 2022** (Express/Developer; máy dev có thể dùng LocalDB) |
| Truy cập dữ liệu | **EF Core 10** (Code First, ánh xạ tên bảng/cột đúng SPEC bằng Fluent API), migration có kiểm soát |
| Xác thực | Cookie Authentication **tự xây trên bảng NGUOIDUNG và TAIKHOAN_BANDOC** (không dùng bảng của ASP.NET Identity), hasher tự viết `Rfc2898DeriveBytes.Pbkdf2` SHA256 lưu 64 ký tự (thay `PasswordHasher<T>` – GIA_DINH D5) để băm mật khẩu, AntiForgery cho mọi request ghi |
| Tác vụ nền | `BackgroundService` có sẵn của .NET (hủy đặt chỗ quá hạn, nhắc hạn, sao lưu tự động) |
| Frontend | HTML5 semantic, CSS3 (biến CSS làm token), JavaScript ES Modules, không bước build bắt buộc |
| Lưới và tiện ích | **Bootstrap 5.3.3** chỉ cho grid, container, flex utilities, offcanvas, dropdown; tự viết CSS để có bản sắc |
| Biểu tượng | **Bootstrap Icons 1.11.3** |
| Chuyển động | **GSAP 3.13.0** (core + Flip + ScrollTrigger + Observer + Draggable – các plugin này miễn phí từ bản 3.13) |
| Biểu đồ | **Apache ECharts 5.6.0** (lý do ở file 09) |
| Đóng gói, chạy | **Docker** + **Docker Compose** cho mọi service, CSDL, message bus; dev, kiểm thử, triển khai đều chạy bằng compose (file 14) |
| Lật trang, cuộn mượt | **StPageFlip** (`page-flip` 2.0.7), **Lenis** 1.3.x – xem file 13 |
| Thiết bị | **Bắt buộc dùng tốt trên điện thoại** (iOS Safari, Android Chrome) và máy tính; thiết kế theo hướng mobile-first cho cổng bạn đọc |
| Font | **Be Vietnam Pro** (400/500/600/700, tự host trong `wwwroot/fonts`); header cổng bạn đọc dùng Times New Roman theo file 03 |
| Kiểm thử | xUnit, `WebApplicationFactory` cho kiểm thử tích hợp, **Playwright for .NET** cho E2E và chụp ảnh |
| Ghi log | `ILogger` + **Serilog** ghi file xoay vòng |

Thư viện frontend tải về `wwwroot/lib/` (qua LibMan hoặc tải tay), không gọi CDN khi chạy thật (thư viện có thể chạy trong mạng nội bộ). Ghi phiên bản và nguồn vào `README.md`.

## 4. Cấu trúc solution (chỉ áp dụng khi basecore không có quy ước tương ứng; phần frontend `ThuVien.Web` giữ cấu trúc `wwwroot` như dưới)

```
ThuVien.sln
src/
  ThuVien.Domain/        Entity 18 bảng gốc + bảng mở rộng, hằng số miền giá trị, mã quyền
  ThuVien.Data/          ThuVienDbContext, cấu hình Fluent API, Migrations, DataSeeder (khởi tạo + mẫu)
  ThuVien.Application/   Service nghiệp vụ: mỗi tiến trình hệ thống (SPEC 3.4) là một phương thức có tên rõ ràng,
                         quy tắc mượn/gia hạn/trả/vi phạm/đặt chỗ, sinh số chứng từ, báo cáo
  ThuVien.Web/           ASP.NET Core MVC
    Areas/QuanTri/       Hệ thống nghiệp vụ (dashboard trắng – đỏ) – Controllers, Views
    Controllers/         Cổng bạn đọc
    Api/                 Controller trả JSON cho JS (cổng và dashboard), tiền tố /api
    Views/Shared/        _LayoutCong.cshtml, _LayoutQuanTri.cshtml, Partials (Header, Footer, BookCard, Hero…)
    BackgroundServices/  DatChoHetHanService, NhacHanService, SaoLuuTuDongService
    wwwroot/
      css/  tokens-cong.css, tokens-quantri.css, base.css, header-footer.css, hero.css, coverflow.css,
            book-preview.css, catalog.css, account.css, dashboard.css, forms.css, print.css
      js/   motion.config.js, state-machine.js, api/*.js, cong/*.js, quantri/*.js, charts/*.js
      fonts/  lib/  img/
tests/
  ThuVien.UnitTests/  ThuVien.IntegrationTests/  ThuVien.E2E/ (Playwright, có ScreenshotTests)
docs/   (bộ tài liệu này + GIA_DINH.md + TIEN_DO.md + HUONG_DAN_SU_DUNG.md + HUONG_DAN_TRIEN_KHAI.md)
```

Không gộp mọi thứ vào một file Razor/CSS/JS. Controller mỏng, nghiệp vụ nằm trong Application.

## 5. Quy ước chung
- Route tiếng Việt không dấu: `/sach/{maDauSach}`, `/tai-khoan/dat-cho`, `/quan-tri/phieu-muon/lap`.
- Ngày giờ: lưu theo giờ Việt Nam (`SE Asia Standard Time` / `Asia/Ho_Chi_Minh`) qua một dịch vụ `IClock` duy nhất (để kiểm thử được thời gian 24 giờ).
- Tiền: `decimal(18,0)`, hiển thị `1.234.000 đ`. Ngày hiển thị `dd/MM/yyyy`, giờ `HH:mm`.
- Mọi API trả lỗi dạng `{ "ma": "...", "thongBao": "...", "truong": { "tenTruong": "lỗi" } }` với mã HTTP đúng (400, 401, 403, 404, 409, 422, 500).
