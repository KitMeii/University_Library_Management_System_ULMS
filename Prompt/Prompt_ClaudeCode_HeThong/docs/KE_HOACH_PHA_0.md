# KẾ HOẠCH PHA 0 – ULMS trên nền basecore

> Trạng thái: **chờ chủ dự án duyệt** (15/11/2026). Đã tính các quyết định D1–D10 trong `GIA_DINH.md`.
> Chỗ ghi **[Qn]** là điểm cần chủ dự án trả lời (mục 15). Phương án ghi kèm là đề xuất mặc định.

## 1. Kiến trúc tổng thể

```
 Trình duyệt (máy tính / điện thoại)
        │  HTTPS, cookie NhanVien | BanDoc + AntiForgery   (không token trong JS/LocalStorage)
        ▼
 web  (Frontend/ThuVien.Web – ASP.NET Core MVC .NET 10)
   ├─ Cổng bạn đọc  /            Razor + JS module
   ├─ Dashboard     /quan-tri    Area QuanTri
   └─ /api/**  → chuyển tiếp (BFF) tới gateway, gắn "Authorization: Bearer <JWT>" lấy từ cookie   [Q7]
        ▼
 gateway (BaseCore.ApiGateway – Ocelot) : kiểm JWT, rate limit, định tuyến
        ▼
 ┌───────────────┬───────────────┬────────────────┬───────────────┐
 quantri          khosach          muontra           vipham          (Web API .NET 10)
 └───────────────┴───────────────┴────────────────┴───────────────┘
        ▼ mỗi service một SQL login, chỉ ghi schema của mình
 sqlserver (SQL Server 2022, DB ThuVien, schema: quantri | khosach | muontra | vipham)
 migrator (chạy một lần: migration + quyền SQL + dữ liệu khởi tạo/mẫu)
```

## 2. Ranh giới service và bảng sở hữu  [Q1][Q2]

Đề xuất **4 service nghiệp vụ** (gộp nhat-ky + sao-luu vào quantri như file 12 cho phép; gộp thong-bao vào muontra vì gần như mọi thông báo phát sinh từ đặt chỗ/mượn trả của bạn đọc).

| Service (schema) | Bảng sở hữu (ghi) | Nghiệp vụ / giao diện SPEC |
|---|---|---|
| **quantri** (từ `AuthService`) | NHOMNGUOIDUNG, NGUOIDUNG, QUYEN, PHANQUYEN, NHATKY_DANGNHAP, NHATKY_HOATDONG *(đọc/quản lý)* | 6.3.1 Đăng nhập NV, 6.3.2 Tài khoản, 6.3.3 Cấp quyền, 6.3.4 Nhật ký, 6.3.5 Sao lưu – phục hồi (+ `SaoLuuTuDongService`), tổng quan N4 |
| **khosach** (từ `APIService`) | SACH, KHUVUC, DAUSACH, DAUSACH_BANSACH, THELOAI | 6.1.1 Danh mục sách (03.01.02, 03.01.04, 03.02.03, 03.03.02, 03.04.01), 6.1.4 Khu vực; API kệ sách/tìm sách của cổng; tổng quan N3 |
| **muontra** (mới) | BANDOC, PHIEUMUON, CT_PHIEUMUON, PHIEUGIAHAN, CT_PHIEUGIAHAN, PHIEUTRA, CT_PHIEUTRA, PHIEUDATCHO, SOCHUNGTU, YEUTHICH, THONGBAO, **TAIKHOAN_BANDOC** [Q2] | 6.1.3 Bạn đọc (01.01.01), 6.2.1–6.2.3, MB02–MB04, MB06; đặt chỗ, tài khoản/đăng nhập bạn đọc, thông báo; `DatChoHetHanService`, `NhacHanService`; tổng quan N1 |
| **vipham** (mới) | KHOANTHU, BIENBAN, CT_BIENBAN | 6.1.2 Khoản thu, 6.2.4 (02.03.01), MB05, MB07; tổng quan N2 |

- `TAIKHOAN_BANDOC` đặt ở muontra (file 12 gợi ý identity) vì file 07 yêu cầu tạo tài khoản **trong cùng transaction** khi lưu BANDOC.
- `NHATKY_HOATDONG`, `THONGBAO`: mọi service chỉ có quyền **INSERT** (bảng nối thêm), để ghi cùng transaction với nghiệp vụ như file 02 mục 5. Quyền đọc/sửa thuộc chủ sở hữu.
- `SOCHUNGTU` thuộc muontra; vipham cấp số `BB` qua thủ tục `muontra.usp_CapSoChungTu` (chỉ quyền EXECUTE).

## 3. CSDL (đã chọn: 1 database, mỗi service 1 schema – D10)

- **Khóa ngoại vật lý giữ đủ như SPEC**, kể cả giữa hai schema (một DB nên làm được) → toàn vẹn đúng thiết kế, `SchemaMatchesSpecTests` so 18 bảng + 8 bảng mở rộng trực tiếp. [Q4]
- **Quyền SQL thực thi ranh giới:** migrator tạo 4 login `svc_quantri`, `svc_khosach`, `svc_muontra`, `svc_vipham` (mật khẩu trong `.env`, SID cố định để phục hồi không bị “orphan user”). Mỗi login: toàn quyền DML trên schema mình; `SELECT` trên đúng các bảng cần đọc của schema khác; `INSERT` trên NHATKY_HOATDONG, THONGBAO.
- **Đọc chéo** (thay cho bản sao đọc + sự kiện): ví dụ muontra đọc `khosach.SACH`, `khosach.KHUVUC`; vipham đọc `muontra.PHIEUMUON/CT_PHIEUMUON/CT_PHIEUTRA/BANDOC`; khosach đọc `muontra.CT_PHIEUMUON/CT_PHIEUTRA/PHIEUDATCHO`, `vipham.CT_BIENBAN`. Service ánh xạ bảng của schema khác dạng chỉ đọc (`ExcludeFromMigrations`, không `SaveChanges`).
- **Migration tập trung:** project `BaseCore.Migrator` có `ThuVienSchemaDbContext` mô tả toàn bộ 26 bảng (Fluent API đúng tên bảng/cột SPEC, kiểu theo file 02, `CHECK` miền giá trị, chỉ mục, chỉ mục lọc của PHIEUDATCHO), SEQUENCE, thủ tục cấp số, login/quyền, dữ liệu khởi tạo (N1–N4, QUYEN/PHANQUYEN từ SPEC 3.2 + 3.4 + quyền mở rộng, admin) và dữ liệu mẫu (chỉ Development). DbContext của từng service **không** chạy migration.
- Mã tự sinh đề xuất [Q9]: `ma_nd` = `ND` + 4 số; `ma_dangnhap` = `DN` + yyMMdd + 7 số (15 ký tự); `ma_hoatdong` = `HD` + yyMMdd + 7 số (15) – dùng SEQUENCE. Số chứng từ theo D4.
- Thời gian: `datetime2(0)` giờ Việt Nam qua `IClock`; container `TZ=Asia/Ho_Chi_Minh`.

## 4. Giao tiếp giữa service – “chuyển thông tin giữa bộ phận”  [Q3][Q5]

Đề xuất **không dùng message bus** ở giai đoạn này: nhờ một DB + FK + quyền đọc chéo, các bước “chuyển cho bộ phận …” trong SPEC được hiện thực bằng **danh sách suy ra từ dữ liệu**, không cần thêm bảng ngoài file 02:

| Bước SPEC | Hiện thực |
|---|---|
| 01.01.03 → 03.01.04 “sách đã cho mượn” (dgvChuyenDen) | Dòng CT_PHIEUMUON chưa trả mà `SACH.tinhtrang` ≠ “Đang được mượn” |
| 01.02.02 → 03.02.03 “sách trả” | Dòng CT_PHIEUTRA `tinhtrang_tra = 'Bình thường'` mà `SACH.tinhtrang` = “Đang được mượn” |
| 02.03.01 → 03.03.02 “sách mất/hư hỏng” | Dòng CT_PHIEUTRA `Mất/Hư hỏng` (đã có CT_BIENBAN) mà `SACH.tinhtrang` chưa tương ứng |
| 01.02.02 → 02.03.01 “trường hợp vi phạm” | CT_PHIEUTRA có `so_ngay_quahan > 0` hoặc `Mất/Hư hỏng` chưa có trong CT_BIENBAN |
| 02.03.01 → 01.xx “kết quả xử lý” | BIENBAN.`ht_thanhtoan` đọc trực tiếp khi kiểm tra điều kiện mượn |
| “Đánh dấu đã xử lý” | Tự hết khỏi danh sách khi `SACH.tinhtrang` đã khớp |
| Nhật ký, thông báo | INSERT trực tiếp trong transaction nghiệp vụ |

Ưu điểm: nguyên tử, ít thành phần (không RabbitMQ, outbox, inbox), dễ kiểm thử, ra ảnh Bài 4 sớm. Nhược: các service phụ thuộc lược đồ của nhau ở mức đọc. Phương án thay thế (nếu chủ dự án muốn thể hiện sự kiện): RabbitMQ + MassTransit 8.x (Apache-2.0; bản 9 là thương mại) + outbox EF → thêm bảng kỹ thuật ngoài file 02.

## 5. Gateway (Ocelot – giữ của basecore)

- Route theo tiền tố, ví dụ: `/api/quan-tri/{tai-khoan,phan-quyen,nhat-ky,sao-luu,xac-thuc}` → quantri; `/api/quan-tri/{sach,khu-vuc,dau-sach,the-loai}`, `/api/cong/{sach,ke-sach,the-loai}` → khosach; `/api/quan-tri/{ban-doc,phieu-muon,phieu-gia-han,phieu-tra,dat-cho,bao-cao/muon-tra}`, `/api/cong/{dat-cho,tai-khoan,thong-bao,yeu-thich,dang-nhap,kich-hoat}` → muontra; `/api/quan-tri/{khoan-thu,bien-ban,bao-cao/vi-pham}` → vipham; `/api/quan-tri/thong-ke/{n1..n4}` → service tương ứng.
- Kiểm JWT ở gateway (route công khai: kệ sách, tìm sách, đăng nhập, kích hoạt); rate limit đăng nhập/kích hoạt/đặt chỗ; host theo tên service Docker; `/health`.
- Service vẫn tự kiểm JWT + quyền (không tin gateway tuyệt đối).

## 6. Xác thực hai loại tài khoản

| | Nhân viên N1–N4 | Bạn đọc |
|---|---|---|
| Bảng | `quantri.NGUOIDUNG` | `muontra.TAIKHOAN_BANDOC` (đăng nhập bằng `BANDOC.ma_svhvcb`) |
| Cấp JWT | quantri | muontra |
| Băm | Hasher chung D5 (`BaseCore.Common`) | Như bên trái |
| Khóa | Sai 5 lần liên tiếp → `tinhtrang = 'Khóa'` (SPEC 6.3.1) | Sai 5 lần → khóa 15 phút (`tg_khoa_den`) |
| Cookie ở web | `NhanVien`, trượt 30 phút | `BanDoc`, “Ghi nhớ” tối đa 14 ngày |
| Claim | `loai=NV`, `ma_nd`, `ten_nd`, `ma_nhom`, **danh sách mã quyền** nạp lúc đăng nhập (đúng SPEC 6.3.3: quyền mới hiệu lực từ lần đăng nhập sau) | `loai=BD`, `ma_bandoc` |

- JWT HS256 ký bằng khóa trong `.env` (dùng chung quantri/muontra/gateway); hạn JWT dài bằng hạn tối đa của cookie, cookie hết hạn thì phải đăng nhập lại. JWT nằm **trong cookie xác thực đã mã hóa** (HttpOnly, không lộ ra JS).
- `[YeuCauQuyen("TT025")]` / `[YeuCauQuyen("SACH_E")]` ở service + web (ẩn menu, ẩn nút theo SPEC “Visible = False”). Token bạn đọc gọi `/api/quan-tri/**` → 403 và ngược lại.
- Đăng xuất ghi `tg_dangxuat`; hết phiên cũng ghi (khi phát hiện ở lần truy cập sau/tác vụ nền).
- `admin` khởi tạo: mật khẩu từ biến môi trường, bắt đổi lần đầu ở Production; Development dùng `Thuvien@123` theo SPEC 7. [Q12]

## 7. Cấu trúc mã nguồn  [Q8]

```
Backend/                          (làm phẳng từ Backend/BaseCore/BaseCore bằng git mv – giữ lịch sử)
  src/
    BaseCore.ApiGateway/
    BaseCore.QuanTriService/      (đổi tên từ AuthService)
    BaseCore.KhoSachService/      (đổi tên từ APIService)
    BaseCore.MuonTraService/      (mới, theo khuôn Program.cs của basecore)
    BaseCore.ViPhamService/       (mới)
    BaseCore.Migrator/            (mới)
    BaseCore.Common/              JWT, hasher, IClock, YeuCauQuyen, lỗi {ma,thongBao,truong}, ExceptionMiddleware, health, Serilog
    BaseCore.Entities/            entity 26 bảng + hằng số miền giá trị, mã quyền
    BaseCore.Repository/          Repository<T> + Unit of Work
    BaseCore.DTO/  BaseCore.Libs/
  tests/  BaseCore.UnitTests/  BaseCore.IntegrationTests/
Frontend/
  ThuVien.Web/                    cấu trúc Areas/QuanTri, Controllers, Views, wwwroot như file 01 mục 4
  tests/ThuVien.E2E/              Playwright .NET (E2E + ScreenshotTests)
ULMS.slnx                         một solution ở gốc cho cả Backend + Frontend
docker-compose.yml, docker-compose.override.yml, docker-compose.test.yml, docker-compose.prod.yml, .env.example, README.md, scripts/
```
Mỗi service: `Controllers/` mỏng → `NghiepVu/` (mỗi tiến trình SPEC 3.4 một phương thức tên rõ ràng) → `DuLieu/` (DbContext riêng). `BaseCore.Services` (dùng chung) bỏ, vì nghiệp vụ thuộc từng service.

## 8. Docker (file 14)

| Service compose | Image / build | Cổng dev | Healthcheck | Volume |
|---|---|---|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-CUxx-ubuntu-22.04` (ghim CU mới nhất phát hành ≥ 14 ngày, chốt ở Pha 1) | **1434:1433** (override) | `sqlcmd -C -Q "SELECT 1"` | `thuvien-sql-data`, `thuvien-backup` |
| `migrator` | build | – | chạy xong thoát 0 | – |
| `quantri`, `khosach`, `muontra`, `vipham` | build (sdk:10.0 → aspnet:10.0, `USER app`, cổng 8080) | không mở | `/health` (sống + DB) qua cờ `--healthcheck` của chính ứng dụng (không cài curl) | quantri gắn `thuvien-backup` (liệt kê tệp) |
| `gateway` | build | 5000 (chỉ dev) | `/health` | – |
| `web` | build | **8080** | `/health` | `thuvien-uploads` |
| `e2e` (chỉ test) | `mcr.microsoft.com/playwright/dotnet:v1.xx-noble` (ghim ở Pha 1) | – | – | `./artifacts`, `docs/anh_giao_dien` |

- Thứ tự: sqlserver healthy → migrator `service_completed_successfully` → 4 service healthy → gateway → web.
- Mạng `thuvien-net`; `TZ=Asia/Ho_Chi_Minh`; `restart: unless-stopped`; `mem_limit` (SQL Server ≥ 2 GB).
- Dockerfile multi-stage, `restore` tách lớp cache; `.dockerignore`.
- `override`: Development, dữ liệu mẫu, mở 1434/5000, mount `wwwroot` để sửa CSS/JS thấy ngay. `test`: DB tạm không volume + `e2e`. `prod`: không mở cổng nội bộ, reverse proxy HTTPS (Pha 8).
- README: cách chạy, cách nối SSMS (`localhost,1434`, SQL Server Authentication, `sa`, mật khẩu `.env`, Trust server certificate).

## 9. Frontend (Frontend/ThuVien.Web)

- ASP.NET Core MVC .NET 10, Razor; JS ES Modules không bước build; thư viện tải bằng LibMan vào `wwwroot/lib` (Bootstrap 5.3.3, Bootstrap Icons 1.11.3, GSAP 3.13.0, ECharts 5.6.0, page-flip 2.0.7, Lenis 1.3.x, JsBarcode); font Be Vietnam Pro tự host.
- Hai layout `_LayoutCong` (emerald) / `_LayoutQuanTri` (trắng – đỏ), token CSS theo file 03, `motion.config.js`, `state-machine.js`, `form-state.js` (bảng trạng thái nút khai báo dạng dữ liệu, lấy đúng dòng Enable/Visible của SPEC).
- Control giữ đúng tên SPEC làm `id`; `dgv…` = DataGrid phân trang server; DatePicker không gõ tay; modal tự làm (không `alert()`).
- Trang in `/quan-tri/in/...` khổ A5/A4 `@media print`; MB06/MB07 dùng ECharts SVG renderer.
- Mobile: dashboard sidebar → offcanvas, lưới cuộn ngang; cổng mobile-first.

## 10. Kệ sách coverflow (file 05, ảnh tham khảo)

- Khối sách CSS 3D (bìa, gáy, mặt giấy trên) trong `perspective: 1800px`; **một biến `pos` liên tục**, GSAP tween `pos`; `layout(pos)` tính `x, z, rotationY, scale` cho cửa sổ ±20 cuốn (ảo hóa), ghi bằng `gsap.quickSetter`; đo W/H/T một lần qua `ResizeObserver`.
- Công thức theo file 05: góc 0°→58°→82°, `scale = max(0.42, 1 − 0.075·a^0.85)`, cung lùi `z = −R(1 − cos(aθ))`, lớp `.book__shade` tối dần; mỗi khối có gáy ở cả hai cạnh để phía nào cũng lộ gáy về người xem như ảnh.
- Chữ trang trí chấm bán sắc “TRI/THỨC”, “THƯ/VIỆN” (SVG pattern), bóng elip tĩnh, tên sách tách ký tự, thanh thời gian theo năm, nút đổi kệ; Observer cho bánh xe, Pointer Events + quán tính cho kéo/vuốt (`touch-action: pan-y`).
- Nghiệm thu: ảnh chụp đặt cạnh ảnh tham khảo; đo fps bằng rAF trong Playwright (CPU throttling 4×).

## 11. Đặt chỗ an toàn đồng thời và tính “có thể mượn”  [Q6]

- **Bản sách có thể mượn/đặt** = `SACH.tinhtrang = 'Sẵn sàng'` **và** không nằm trong CT_PHIEUMUON chưa trả **và** không có PHIEUDATCHO “Đang giữ” còn hạn của người khác. (Lý do: theo SPEC, `SACH.tinhtrang` chỉ đổi khi N3 xử lý 03.01.04, nên ngay sau khi N1 cho mượn, SACH vẫn “Sẵn sàng” – nếu chỉ xét tình trạng sẽ cho mượn/đặt trùng.)
- Đặt chỗ: một transaction trong muontra; chọn bản ứng viên `WITH (UPDLOCK, HOLDLOCK)` (ưu tiên Kệ mở, mã nhỏ nhất) → INSERT PHIEUDATCHO; chỉ mục lọc `UX_DATCHO_BANSACH_DANGGIU`, `UX_DATCHO_BANDOC_DAUSACH_DANGGIU` chặn trùng; lỗi 2601/2627 → thử bản khác; hết → 409 “Sách vừa được người khác đặt”.
- Lập Phiếu mượn khóa cùng thứ tự (bản sách) để không đụng độ với đặt chỗ; `btnThemSach` từ chối bản đang giữ cho người khác.
- Hết hạn: truy vấn luôn coi `tg_hethan < bây giờ` là hết; `DatChoHetHanService` (mỗi 5 phút) chuyển trạng thái + THONGBAO. Mọi thời điểm qua `IClock`.
- Test: 20 request song song cùng bản cuối → đúng 1 thành công.

## 12. Kiểm thử

| Lớp | Công cụ | Nội dung chính |
|---|---|---|
| Unit | xUnit | Hasher (đúng 64 ký tự, FixedTimeEquals, đổi số vòng), số chứng từ D4 (sang tháng đánh lại), hạn trả/gia hạn/quá hạn/thành tiền, điều kiện mượn/đặt chỗ từng điều kiện, vòng đời đặt chỗ với `IClock` giả |
| Tích hợp | xUnit + `WebApplicationFactory` từng service + SQL Server container (compose test) | `SchemaMatchesSpecTests` (26 bảng, mọi schema); **ma trận quyền** sinh từ SPEC 3.2/3.4 → mọi API ngoài quyền 403; transaction lập phiếu; danh sách “chuyển đến”; đặt chỗ đồng thời; MB06/MB07 so truy vấn kiểm chứng; login SQL không ghi được schema khác |
| E2E | Playwright .NET trong container `e2e` | Luồng khép kín file 11; trạng thái nút từng form (đọc bảng trạng thái); coverflow; Book Preview; reduced motion |
| Ảnh chụp | `ScreenshotTests` | 1366×768 + 390×844, tên theo số mục báo cáo (`4.2.1_Danh_muc_sach.png`, `4.3.1_Lap_phieu_muon_TT1.png`…) vào `docs/anh_giao_dien/` – **chụp ngay từ Pha 1–4** |
| Mobile | Playwright thiết bị iPhone 13, Pixel 7 | Cổng (vuốt kệ, preview 1 trang, tab đáy) + dashboard (offcanvas, lưới cuộn, form) |
| Accessibility | axe-core trong Playwright [Q7] | Không lỗi nghiêm trọng |

## 13. Lộ trình pha (sắp lại theo D9)

| Pha | Nội dung | Ảnh Bài 4 xuất ra |
|---|---|---|
| **1** Nền tảng | `git init` + `.gitignore` + **xóa bí mật** → commit gốc → push; build mốc basecore cũ; **commit xóa riêng** (D8); nâng .NET 10, làm phẳng/đổi tên project; Common (JWT, hasher, IClock, quyền, lỗi, health, Serilog); migrator 26 bảng + quyền SQL + khởi tạo + mẫu; 4 service khung + gateway; web 2 layout + token + motion; đăng nhập nhân viên đầy đủ (6.3.1) + khung đăng nhập bạn đọc; toàn bộ Docker compose | 4.6.1 Đăng nhập |
| **2** Danh mục + quản trị | 6.1.1–6.1.4, 6.3.2–6.3.5, menu theo nhóm, màn hình mặc định theo nhóm, nhật ký đủ | 4.2.1–4.2.4, 4.6.2–4.6.5 |
| **3** Chứng từ + in | 6.2.1–6.2.4 (3 trạng thái), MB02–MB05, dgvChuyenDen/03.xx trong Danh mục sách | 4.3.1–4.3.4 (mỗi form TT1/TT2/TT3), 4.4.1–4.4.4 |
| **4** Báo cáo + tổng quan | MB06, MB07 (Tổng hợp → Lập báo cáo), tổng quan N1–N4 với ECharts | 4.5.1–4.5.2 → **đủ bộ ảnh Bài 4** |
| **5** Cổng: khung + kệ sách | Header, footer, hero, coverflow; **quản lý đầu sách, thể loại (mở rộng) + ô chọn đầu sách ở Danh mục sách** (chuyển từ Pha 2 xuống để ảnh Bài 4 sạch phần mở rộng) | – |
| **6** Book Preview + đặt chỗ | Preview, đặt chỗ 24 giờ, tài khoản kính mờ, thông báo, tác vụ nền; **Quầy đặt chỗ + `dgvDatCho`/`btnNhanDatCho` trong Lập Phiếu mượn** (chuyển từ Pha 3 xuống) | – |
| **7** Trang chủ hoàn chỉnh | Danh mục, khám phá, hướng dẫn, số liệu, thông tin | – |
| **8** Hoàn thiện | Chuyển động, responsive, a11y, bảo mật, hiệu năng, E2E trong Docker, `prod.yml`, tài liệu | Chụp lại toàn bộ |

Khối mở rộng ở Lập Phiếu mượn chỉ hiện khi bạn đọc có đặt chỗ “Đang giữ”, nên ảnh 4.3.1 vẫn đúng hồ sơ.

## 14. Rủi ro

| # | Rủi ro | Xử lý |
|---|---|---|
| K1 | Ocelot chưa có bản build riêng cho .NET 10 | Gói target net8/net9 chạy được trên runtime 10; kiểm ngay Pha 1; dự phòng YARP làm gateway (cần duyệt) |
| K2 | SQL Server container cần ≥ 2 GB RAM trong WSL 2 | Ghi README; kiểm `.wslconfig` |
| K3 | Phục hồi CSDL: SINGLE_USER, login các service, SID | Login tạo với SID cố định; sau phục hồi `docker compose restart`; quyền RESTORE chỉ cấp cho kết nối quản trị của quantri |
| K4 | PBKDF2 600.000 vòng ~ 0,3–0,6 s CPU/lần đăng nhập | Chấp nhận (có rate limit); test dùng số vòng nhỏ qua cấu hình |
| K5 | Cookie lớn do danh sách quyền trong JWT (N4 ~ 70 mã) | ASP.NET tự chia cookie; nếu vượt thì lưu phiên phía server |
| K6 | Hiệu năng 3D/backdrop-filter trên iPhone | Theo quy tắc file 03/05; cần ≥ 1 máy thật [Q14] |
| K7 | Đọc chéo schema làm service phụ thuộc lược đồ nhau | Chỉ cấp SELECT đúng bảng cần; test hợp đồng; thay bằng bus sau nếu cần |
| K8 | Mâu thuẫn còn sót trong SPEC phát hiện khi code | Ghi vào câu hỏi pha tương ứng, không tự quyết |

## 15. Câu hỏi cho chủ dự án

- **Q1** Gộp còn **4 service nghiệp vụ** (quantri gồm identity + nhật ký + sao lưu; khosach; muontra gồm đặt chỗ + thông báo + tài khoản bạn đọc; vipham)?
- **Q2** Đặt `TAIKHOAN_BANDOC` ở **muontra** (cùng transaction với BANDOC) thay vì identity như file 12 gợi ý?
- **Q3** **Không dùng message bus**: liên kết bộ phận bằng đọc chéo schema + INSERT trực tiếp NHATKY_HOATDONG/THONGBAO? Hay muốn RabbitMQ + MassTransit 8 + outbox (thêm bảng kỹ thuật ngoài file 02)?
- **Q4** Giữ **khóa ngoại vật lý giữa các schema** đúng như SPEC?
- **Q5** Đồng ý định nghĩa các danh sách “chuyển đến”/“trường hợp vi phạm” ở mục 4 (suy ra từ dữ liệu, không thêm bảng)?
- **Q6** Đồng ý quy tắc “bản sách có thể mượn” ở mục 11 (thêm điều kiện không thuộc phiếu mượn chưa trả)?
- **Q7** Duyệt thêm 2 gói: **YARP** (`Yarp.ReverseProxy`, MIT, Microsoft) trong web làm BFF chuyển `/api/**` tới gateway; **Deque.AxeCore.Playwright** (MPL-2.0) chạy axe-core?
- **Q8** Cấu trúc thư mục mục 7: làm phẳng `Backend/BaseCore/BaseCore` → `Backend/`, đổi tên AuthService → QuanTriService, APIService → KhoSachService, giữ tiền tố `BaseCore.*`, một solution `ULMS.slnx` ở gốc?
- **Q9** Định dạng mã tự sinh: `ma_nd` = `ND####`; `ma_dangnhap` = `DNyyMMdd#######`; `ma_hoatdong` = `HDyyMMdd#######` (đều ≤ C(15))?
- **Q10** Sao lưu – phục hồi trên web: `btnChon` chọn thư mục con/tệp **trong thư mục sao lưu của máy chủ** (volume `thuvien-backup`), không duyệt ổ đĩa tùy ý. Có cần thêm nút tải tệp sao lưu về máy / tải tệp lên để phục hồi không?
- **Q11** Thông tin thư viện (tên trường, địa chỉ, giờ mở cửa, email, điện thoại, nội quy) cho `appsettings ThuVien:ThongTin`: anh/chị cung cấp, hay để chỗ trống “THƯ VIỆN TRƯỜNG ĐẠI HỌC …” như SPEC 6.4?
- **Q12** Môi trường Development: `admin/Thuvien@123` không bắt đổi mật khẩu lần đầu (Production vẫn bắt đổi)?
- **Q13** Ảnh chụp cho báo cáo Bài 4: có hiển thị nhãn “DỮ LIỆU MẪU” (nguyên tắc 4 file 01) trên ảnh không, hay có chế độ chụp ẩn nhãn?
- **Q14** Có iPhone/Android thật để kiểm thử cổng bạn đọc trước nghiệm thu (file 05 mục 5b yêu cầu ≥ 1 máy thật)?

### Giả định sẽ áp dụng nếu không có ý kiến khác
- `btnThoat` (đăng nhập) và `btnDong` trên web: hỏi xác nhận → về trang chủ cổng / trang tổng quan của nhóm.
- Cổng web dev: `http://localhost:8080`; gateway dev `http://localhost:5000`.
- Mã đặt chỗ giữ `DCyyyyMMdd-####` (nvarchar(20), file 02), đếm theo ngày.
