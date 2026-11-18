# ULMS – Hệ thống quản lý mượn trả sách thư viện đại học

Gồm hai mặt:
- **Cổng bạn đọc** (`/`): sinh viên, học viên, cán bộ xem kệ sách, đặt chỗ giữ sách 24 giờ, theo dõi tài khoản.
- **Hệ thống nghiệp vụ** (`/quan-tri`): nhân viên N1–N4 (mượn trả, xử lý vi phạm, kho sách, quản trị) theo đặc tả BT4.

Toàn bộ hệ thống chạy bằng **Docker Compose**: 4 service nghiệp vụ, API Gateway và ứng dụng web. CSDL là SQL Server 2022 – chọn **SQL Server cài sẵn trên máy** (mở bằng SSMS) hoặc **SQL Server trong Docker** (mục 2.1).

| Tài liệu | Dành cho |
|---|---|
| `README.md` (tệp này) | Máy phát triển: cài đặt, chạy, kiểm thử |
| [`HUONG_DAN_TRIEN_KHAI.md`](Prompt/Prompt_ClaudeCode_HeThong/docs/HUONG_DAN_TRIEN_KHAI.md) | Triển khai máy chủ thư viện: `docker-compose.prod.yml`, HTTPS, sao lưu, cập nhật, xử lý sự cố |
| [`HUONG_DAN_SU_DUNG.md`](Prompt/Prompt_ClaudeCode_HeThong/docs/HUONG_DAN_SU_DUNG.md) | Người dùng: bạn đọc và nhân viên N1–N4, có ảnh chụp |

## 1. Yêu cầu

| Thành phần | Ghi chú |
|---|---|
| Docker Desktop (Windows: bật WSL 2) | Chạy SQL Server trong Docker thì cấp ≥ 4 GB RAM. Mac chip Apple: bật “Use Rosetta for x86/amd64 emulation” (image SQL Server chỉ có bản x64) |
| SQL Server 2022 + SSMS (tùy chọn) | Khi dùng SQL Server cài sẵn trên máy (mục 2.1) |
| .NET SDK 10 (tùy chọn) | Chỉ cần khi build/gỡ lỗi ngoài container. Không có SDK vẫn build/test được bằng `scripts/build-docker.sh`, `scripts/test-docker.sh` |
| Git | |

## 2. Chạy hệ thống (máy phát triển)

```bash
cp .env.example .env          # rồi điền mật khẩu (xem chú thích trong file)
docker compose up -d --build  # lần đầu mất vài phút
```

Mở **http://localhost:8080** (cổng bạn đọc) và **http://localhost:8080/quan-tri/dang-nhap** (nhân viên).

`docker-compose.override.yml` được nạp tự động khi chạy dev: môi trường Development, nạp **DỮ LIỆU MẪU**, mở cổng SQL Server cho SSMS và gateway (5000).

### 2.1. Chọn SQL Server (GIA_DINH D44)

| | Cách 1 – SQL Server cài sẵn trên máy (SSMS) | Cách 2 – SQL Server trong Docker |
|---|---|---|
| `.env` | `COMPOSE_PROFILES=` (trống)<br>`DB_SERVER=host.docker.internal,1433`<br>`MSSQL_SA_PASSWORD=<mật khẩu sa của máy>`<br>`SAO_LUU_THU_MUC=` (trống) | `COMPOSE_PROFILES=sql-docker`<br>`DB_SERVER=sqlserver`<br>`MSSQL_SA_PASSWORD=<tự đặt>`<br>`SAO_LUU_THU_MUC=/var/opt/mssql/backup` |
| Yêu cầu | Bật TCP/IP cổng 1433 (SQL Server Configuration Manager) và SQL Server Authentication | Docker cấp ≥ 4 GB RAM |
| SSMS | Server name: tên máy (ví dụ `TunKittt`) hoặc `localhost` | `localhost,1434` (hoặc cổng `SQL_PORT`) |

Container `migrator` tự tạo CSDL `ThuVien`, login service và dữ liệu khởi tạo trên SQL Server đã chọn; không đụng các CSDL khác.
Muốn tạo bằng SSMS: chạy các tệp trong [`Database/`](Database/README.md) (sinh tự động từ migration bằng `sh scripts/xuat-sql.sh`).

### Tài khoản dữ liệu mẫu (chỉ Development)

| Tài khoản | Nhóm | Mật khẩu | Màn hình mở sau đăng nhập |
|---|---|---|---|
| `muontra` | N1 – Mượn trả sách | `Thuvien@123` | 01.01.03 Lập Phiếu mượn sách |
| `vipham` | N2 – Xử lý vi phạm | `Thuvien@123` | 02.03.01 Lập Biên bản vi phạm |
| `khosach` | N3 – Kho sách | `Thuvien@123` | Danh mục sách |
| `admin` | N4 – Quản trị | giá trị `ADMIN_PASSWORD` trong `.env` | 04.00.01 Quản lý tài khoản người dùng |
| Bạn đọc `SV22000001` … (10 tài khoản đã kích hoạt) | Cổng bạn đọc | `Docgia@123` | Trang chủ |

Ở Production, `admin` phải đổi mật khẩu ở lần đăng nhập đầu tiên.

### Kết nối SQL Server bằng SSMS

| Mục | Giá trị |
|---|---|
| Server name | Cách 1: tên máy / `localhost`. Cách 2: `localhost,1434` (hoặc cổng `SQL_PORT`) |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | giá trị `MSSQL_SA_PASSWORD` trong `.env` |
| Options | tích **Trust server certificate** |

CSDL `ThuVien` có 4 schema: `quantri`, `khosach`, `muontra`, `vipham` – mỗi service một login riêng (`svc_*`), chỉ ghi được schema của mình.

## 3. Lệnh thường dùng

| Việc | Lệnh |
|---|---|
| Chạy / cập nhật | `docker compose up -d --build` |
| Xem trạng thái | `docker compose ps` |
| Xem log | `docker compose logs -f web muontra` |
| Build toàn solution (không cần SDK) | `sh scripts/build-docker.sh` |
| Kiểm thử (đơn vị + ảnh + tích hợp + E2E) | `docker compose up -d --build` (build image) → `sh scripts/test-docker.sh` (hoặc `unit`, `integration`, `e2e`, `anh`) – Windows PowerShell: `.\scripts\test-docker.ps1` |
| Đo hiệu năng API (50.000 bản sách, 200.000 dòng chi tiết – P95 < 300 ms) | `sh scripts/test-docker.sh hieunang` → `artifacts/test-results/hieu_nang_api.json` |
| Kiểm tra nghiệm thu triển khai (prod + SQL Server trong Docker, bộ riêng) | `sh scripts/kiem-tra-trien-khai.sh` → `artifacts/test-results/kiem_tra_trien_khai.txt` |
| Chạy như máy chủ thật | `sh scripts/trien-khai.sh prod` (Windows: `.\scripts\trien-khai.ps1 prod`) – kiểm tra mật khẩu SQL theo chính sách máy chủ thật rồi `docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build` (HUONG_DAN_TRIEN_KHAI.md). Máy dev không bị kiểm tra |
| Chụp bộ ảnh Bài 4 (CSDL sạch, không nhãn DỮ LIỆU MẪU) | `sh scripts/test-docker.sh anh` |

**Kiểm thử không đụng CSDL `ThuVien`** (GIA_DINH D49): `scripts/test-docker.sh` dựng bộ hệ thống riêng (project `ulms-test`, web cổng 18080) trên CSDL **`ThuVien_Test`**, xóa – tạo lại CSDL này + dữ liệu mẫu mỗi lần chạy, chạy xong thì gỡ bộ kiểm thử (`GIU_LAI=1` để giữ lại xem). Dùng SQL Server cài sẵn thì đặt `SAO_LUU_THU_MUC_KIEM_THU` trong `.env` – thư mục riêng cho tệp sao lưu do kiểm thử tạo (vd thư mục tạm của dịch vụ `C:\Windows\ServiceProfiles\MSSQLSERVER\AppData\Local\Temp\ULMS_KiemThu`), **không phải thư mục Backup thật**; tệp `.bak` kiểm thử được dọn sau mỗi lần chạy.
| Xuất lại kịch bản SQL sau khi thêm migration | `sh scripts/xuat-sql.sh` |
| Dừng | `docker compose down` |
| **Xóa sạch môi trường dev, kể cả dữ liệu** | `docker compose down -v` ⚠️ mất ảnh tải lên, khóa cookie; Cách 2 mất cả CSDL và bản sao lưu |

Kết quả kiểm thử: `artifacts/test-results/*.trx`. Ảnh giao diện: `Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien/`.

### Sao lưu – phục hồi (04.00.09)
- Cách 1: tệp sao lưu nằm trong thư mục Backup mặc định của SQL Server (ví dụ `C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup`). Cách 2: volume `thuvien-backup` (`/var/opt/mssql/backup`), chép ra bằng `docker compose cp sqlserver:/var/opt/mssql/backup ./sao-luu`.
- Hệ thống tự sao lưu lúc 23:00 hằng ngày, giữ 14 bản `TuDong_*.bak`.
- Phục hồi ghi đè toàn bộ dữ liệu; hệ thống tự sao lưu bản hiện tại (`TruocPhucHoi_*.bak`) trước khi phục hồi. Sau phục hồi **mọi phiên đăng nhập khác (nhân viên, bạn đọc) bị hủy ngay**, phải đăng nhập lại (D36).

## 4. Kiến trúc

```
Trình duyệt ─► web (MVC, cookie NhanVien/BanDoc, BFF /api/** → gateway kèm JWT)
                 └─► gateway (Ocelot) ─► quantri | khosach | muontra | vipham ─► SQL Server (1 DB, 4 schema)
migrator: chạy một lần – migration, login/quyền SQL, dữ liệu khởi tạo (+ dữ liệu mẫu ở Development)
```

| Thư mục | Nội dung |
|---|---|
| `Backend/src/` | Gateway, 4 service, Migrator, thư viện chung (`BaseCore.Common`, `Entities`, `Repository`, `DTO`, `Libs`) |
| `Backend/tests/` | Kiểm thử đơn vị, tích hợp |
| `Frontend/ThuVien.Web/` | Ứng dụng web (cổng + dashboard) |
| `Frontend/tests/ThuVien.E2E/` | Playwright E2E + chụp ảnh |
| `Prompt/Prompt_ClaudeCode_HeThong/docs/` | Đặc tả, kế hoạch, tiến độ, giả định |
| `Database/` | Kịch bản `.sql` tạo CSDL bằng SSMS (sinh từ migrator) |
| `scripts/` | Build/test trong Docker (`.sh` và `.ps1`), kiểm tra triển khai, sinh dữ liệu quyền từ SPEC, sinh cấu hình gateway, xuất kịch bản SQL, tải thư viện frontend |
| `deploy/` | `Caddyfile` – reverse proxy HTTPS cho `docker-compose.prod.yml` |
| `Database/mot_lan/` | Kịch bản dữ liệu chạy một lần (vd D77 cập nhật từ khóa mẫu) |

## 5. Phiên bản

| Thành phần | Phiên bản |
|---|---|
| .NET / ASP.NET Core / EF Core | 10 (image `mcr.microsoft.com/dotnet/sdk:10.0`, `aspnet:10.0`), EF Core 10.0.14 |
| SQL Server | 2022 – cài sẵn trên máy, hoặc image `mcr.microsoft.com/mssql/server:2022-CU27-GDR2-ubuntu-22.04` |
| Caddy (reverse proxy HTTPS, chỉ prod) | image `caddy:2.10.2-alpine` |
| SkiaSharp | 3.119.2 (ảnh thẻ → WebP) |
| Ocelot | 25.0.2 |
| YARP | 2.3.0 |
| Serilog.AspNetCore | 10.0.0 |
| Playwright .NET | 1.64.1 (image `mcr.microsoft.com/playwright/dotnet:v1.64.1-noble`) |
| Bootstrap / Bootstrap Icons | 5.3.3 / 1.11.3 (tự host trong `wwwroot/lib`) |
| GSAP (core, ScrollTrigger, Observer, Draggable) | 3.13.0 (tự host) |
| Lenis / page-flip / ECharts | 1.3.27 / 2.0.7 / 5.6.0 (tự host); mã vạch Code128 tự vẽ SVG – xem THU_VIEN_BEN_THU_BA.md |
| Font Be Vietnam Pro | @fontsource 5.2.8 (tự host) |

Chi tiết giấy phép thư viện bên thứ ba: `Prompt/Prompt_ClaudeCode_HeThong/docs/THU_VIEN_BEN_THU_BA.md`.
