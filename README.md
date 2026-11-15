# ULMS – Hệ thống quản lý mượn trả sách thư viện đại học

Gồm hai mặt:
- **Cổng bạn đọc** (`/`): sinh viên, học viên, cán bộ xem kệ sách, đặt chỗ giữ sách 24 giờ, theo dõi tài khoản.
- **Hệ thống nghiệp vụ** (`/quan-tri`): nhân viên N1–N4 (mượn trả, xử lý vi phạm, kho sách, quản trị) theo đặc tả BT4.

Toàn bộ hệ thống chạy bằng **Docker Compose**: SQL Server 2022, 4 service nghiệp vụ, API Gateway và ứng dụng web.

## 1. Yêu cầu

| Thành phần | Ghi chú |
|---|---|
| Docker Desktop (Windows: bật WSL 2) | Cấp cho Docker ≥ 4 GB RAM (SQL Server cần ≥ 2 GB). Mac chip Apple: bật “Use Rosetta for x86/amd64 emulation” (image SQL Server chỉ có bản x64) |
| .NET SDK 10 (tùy chọn) | Chỉ cần khi build/gỡ lỗi ngoài container. Không có SDK vẫn build/test được bằng `scripts/build-docker.sh`, `scripts/test-docker.sh` |
| Git | |

## 2. Chạy hệ thống (máy phát triển)

```bash
cp .env.example .env          # rồi điền mật khẩu (xem chú thích trong file)
docker compose up -d --build  # lần đầu mất vài phút
```

Mở **http://localhost:8080** (cổng bạn đọc) và **http://localhost:8080/quan-tri/dang-nhap** (nhân viên).

`docker-compose.override.yml` được nạp tự động khi chạy dev: môi trường Development, nạp **DỮ LIỆU MẪU**, mở cổng SQL Server cho SSMS và gateway (5000).

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
| Server name | `localhost,1434` (hoặc cổng `SQL_PORT` trong `.env` nếu 1434 đã bị dùng) |
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
| Kiểm thử (đơn vị + tích hợp + E2E/ảnh) | `sh scripts/test-docker.sh` (hoặc `unit`, `integration`, `e2e`) |
| Chụp bộ ảnh Bài 4 không có nhãn DỮ LIỆU MẪU | `HIEN_NHAN_DU_LIEU_MAU=false docker compose up -d web` rồi `sh scripts/test-docker.sh e2e` |
| Dừng | `docker compose down` |
| **Xóa sạch môi trường dev, kể cả dữ liệu** | `docker compose down -v` ⚠️ mất toàn bộ CSDL, ảnh bìa, bản sao lưu |

Kết quả kiểm thử: `artifacts/test-results/*.trx`. Ảnh giao diện: `Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien/`.

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
| `scripts/` | Build/test trong Docker, sinh dữ liệu quyền từ SPEC, sinh cấu hình gateway, tải thư viện frontend |

## 5. Phiên bản

| Thành phần | Phiên bản |
|---|---|
| .NET / ASP.NET Core / EF Core | 10 (image `mcr.microsoft.com/dotnet/sdk:10.0`, `aspnet:10.0`), EF Core 10.0.14 |
| SQL Server | `mcr.microsoft.com/mssql/server:2022-CU27-GDR2-ubuntu-22.04` |
| Ocelot | 25.0.2 |
| YARP | 2.3.0 |
| Serilog.AspNetCore | 10.0.0 |
| Playwright .NET | 1.64.1 (image `mcr.microsoft.com/playwright/dotnet:v1.64.1-noble`) |
| Bootstrap / Bootstrap Icons | 5.3.3 / 1.11.3 (tự host trong `wwwroot/lib`) |
| GSAP | 3.13.0 (tự host) |
| Font Be Vietnam Pro | @fontsource 5.2.8 (tự host) |

Chi tiết giấy phép thư viện bên thứ ba: `Prompt/Prompt_ClaudeCode_HeThong/docs/THU_VIEN_BEN_THU_BA.md`.
