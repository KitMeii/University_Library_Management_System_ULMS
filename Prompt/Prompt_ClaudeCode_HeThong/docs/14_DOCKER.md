# 14 – ĐÓNG GÓI VÀ CHẠY TOÀN HỆ THỐNG BẰNG DOCKER

Mục tiêu: **một lệnh** dựng toàn bộ hệ thống trên máy bất kỳ có Docker (máy dev, máy chủ thư viện): `docker compose up -d`. Dùng được cho phát triển, kiểm thử E2E và triển khai thật.

## 1. Nguyên tắc
- **Basecore đã có Docker** (Dockerfile, docker-compose): mở rộng theo đúng cấu trúc đó, đổi tên/thêm service của ta, xóa service mẫu đã duyệt xóa (file 12). **Chưa có**: tạo mới theo file này.
- Ghim phiên bản mọi image bằng tag cụ thể (không dùng `latest`), bản đã phát hành ≥ 14 ngày; ghi vào `README.md`.
- Không đưa bí mật vào image hay repo: dùng file `.env` (có `.env.example` mẫu, `.env` nằm trong `.gitignore`); môi trường thật dùng Docker secrets hoặc biến môi trường của máy chủ.
- Dữ liệu bền vững nằm trong **volume có tên**, xóa container không mất dữ liệu.

## 2. Dockerfile cho từng service .NET và ứng dụng web
- **Multi-stage**: stage `build` dùng `mcr.microsoft.com/dotnet/sdk:10.0` (hoặc đúng phiên bản basecore), `restore` tách riêng để tận dụng cache; stage `final` dùng `mcr.microsoft.com/dotnet/aspnet:10.0`.
- Chạy bằng **người dùng không phải root** (`USER app` có sẵn trong image .NET 8+), cổng nội bộ 8080.
- `HEALTHCHECK` gọi `/health` (mỗi service phải có endpoint health: sống + kết nối CSDL + kết nối message bus).
- `.dockerignore` loại `bin/`, `obj/`, `node_modules/`, `.git/`, `tests/`, `docs/`.
- Ứng dụng web: thư viện frontend trong `wwwroot/lib` được đóng gói sẵn vào image (không tải CDN lúc chạy); bật nén response và cache tĩnh.

## 3. `docker-compose.yml` (gốc dự án)
| Service | Image / build | Ghi chú |
|---|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-CUxx-ubuntu-22.04` (ghim CU cụ thể) | `ACCEPT_EULA=Y`, mật khẩu `SA` từ `.env`; volume `thuvien-sql-data:/var/opt/mssql`; volume `thuvien-backup:/var/opt/mssql/backup` cho sao lưu 04.00.09; healthcheck bằng `sqlcmd` |
| `rabbitmq` (hoặc bus basecore đang dùng) | ghim tag có giao diện quản lý | chỉ khi basecore dùng message bus |
| `redis` | chỉ khi basecore dùng cache phân tán | |
| `identity`, `kho-sach`, `muon-tra`, `vi-pham`, `thong-bao`, `nhat-ky` | build từ Dockerfile mỗi service | `depends_on` có `condition: service_healthy`; chuỗi kết nối từ biến môi trường |
| `gateway` | build | cổng duy nhất mở ra ngoài cho API |
| `web` | build | cổng bạn đọc + dashboard; volume `thuvien-uploads` cho ảnh bìa tải lên |
| `migrator` | build (một lần) | chạy migration + dữ liệu khởi tạo (nhóm, quyền, tài khoản quản trị) rồi thoát; các service chờ nó hoàn tất (`service_completed_successfully`) |

- Một mạng nội bộ `thuvien-net`; chỉ `web` (và `gateway` nếu cần) mở cổng ra máy chủ. SQL Server, bus **không** mở cổng ra ngoài ở cấu hình thật.
- Múi giờ: `TZ=Asia/Ho_Chi_Minh` cho mọi container (quan trọng cho đặt chỗ 24 giờ, báo cáo theo ngày); code vẫn lấy giờ qua `IClock`.
- Giới hạn tài nguyên hợp lý (`mem_limit`) và `restart: unless-stopped`.

## 4. Các file compose bổ sung
| File | Dùng khi | Nội dung |
|---|---|---|
| `docker-compose.override.yml` | phát triển (tự nạp) | mở cổng SQL Server 1433, giao diện bus; `ASPNETCORE_ENVIRONMENT=Development`; nạp **dữ liệu mẫu**; mount mã nguồn web để sửa CSS/JS thấy ngay |
| `docker-compose.test.yml` | kiểm thử | CSDL tạm (không volume), dữ liệu mẫu cố định, service `e2e` dùng image `mcr.microsoft.com/playwright/dotnet` (ghim tag) chạy toàn bộ test E2E + chụp ảnh giao diện, xuất kết quả ra `./artifacts` |
| `docker-compose.prod.yml` | máy chủ thư viện | không mở cổng nội bộ, HTTPS qua reverse proxy (ví dụ Nginx/Caddy container hoặc IIS phía trước), log ra volume, `ASPNETCORE_ENVIRONMENT=Production`, không nạp dữ liệu mẫu |

## 5. Lệnh chuẩn (ghi trong README, có script `scripts/` cho Windows PowerShell và bash)
| Việc | Lệnh |
|---|---|
| Chạy dev | `docker compose up -d --build` |
| Xem log | `docker compose logs -f web muon-tra` |
| Chạy kiểm thử E2E + chụp ảnh | `docker compose -f docker-compose.yml -f docker-compose.test.yml up --build --abort-on-container-exit e2e` |
| Triển khai | `docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d` |
| Xóa sạch môi trường dev (kể cả dữ liệu) | `docker compose down -v` (ghi cảnh báo rõ) |

## 6. Sao lưu – phục hồi trong Docker (SPEC 6.3.5, file 11)
- `BACKUP DATABASE` ghi vào `/var/opt/mssql/backup` (volume `thuvien-backup`); tác vụ sao lưu tự động chạy trong service identity (hoặc service sao lưu) gọi lệnh SQL tới container `sqlserver`.
- Hướng dẫn chép bản sao lưu ra ngoài máy chủ (`docker cp` hoặc mount thư mục máy chủ) để không mất khi hỏng ổ đĩa.
- Phục hồi: theo file 11; sau phục hồi khởi động lại các service (`docker compose restart`).

## 7. Lưu ý môi trường
- Windows: Docker Desktop với WSL 2. Máy Mac chip Apple: image SQL Server chỉ có bản x64 → bật “Use Rosetta for x86/amd64 emulation” trong Docker Desktop; ghi rõ trong README.
- CI (nếu basecore có): build image mọi service, chạy `docker-compose.test.yml`, lưu ảnh chụp và báo cáo test làm artifact.

## 8. Nghiệm thu
- [ ] Máy mới chỉ cần Docker + `.env` (từ `.env.example`) → `docker compose up -d --build` chạy được toàn hệ thống, đăng nhập được tài khoản quản trị khởi tạo.
- [ ] Mọi container `healthy`; dừng `sqlserver` thì health các service báo lỗi, bật lại tự phục hồi.
- [ ] `docker compose down` rồi `up` lại: dữ liệu, ảnh bìa, bản sao lưu còn nguyên.
- [ ] Kiểm thử E2E chạy hoàn toàn trong Docker và đạt.
- [ ] Image chạy bằng người dùng không phải root; không có bí mật trong image (kiểm tra bằng `docker history`/quét image).
