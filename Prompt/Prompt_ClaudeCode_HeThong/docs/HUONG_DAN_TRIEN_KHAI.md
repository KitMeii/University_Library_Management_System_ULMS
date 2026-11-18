# HƯỚNG DẪN TRIỂN KHAI ULMS TRÊN MÁY CHỦ THƯ VIỆN

Dành cho cán bộ CNTT triển khai và vận hành hệ thống. Toàn bộ hệ thống chạy bằng **Docker Compose** với tệp `docker-compose.prod.yml`
(file 14 mục 4). Máy phát triển xem `README.md`.

## 1. Thành phần khi chạy thật

```
Internet / mạng trường ──► caddy (HTTPS 443, 80 → chuyển sang 443)
                              └─► web:8080 (cổng bạn đọc + hệ thống nghiệp vụ)
                                    └─► gateway:8080 ─► quantri | khosach | muontra | vipham ─► SQL Server (CSDL ThuVien, 4 schema)
migrator: chạy một lần mỗi lần khởi động – áp dụng migration, tạo login SQL cho từng service, tạo nhóm/quyền/tài khoản admin
```

- Chỉ **caddy** mở cổng ra máy chủ (80, 443). Web, gateway, các service và SQL Server trong Docker chỉ nằm trong mạng nội bộ `thuvien-net`.
- Môi trường `Production`: không nạp dữ liệu mẫu, không hiện nhãn “DỮ LIỆU MẪU”, trang lỗi không lộ chi tiết, bật HSTS,
  cookie `Secure` + `HttpOnly` + `SameSite=Lax`, CSP chỉ cho nguồn nội bộ.
- Dữ liệu bền vững nằm trong volume có tên – xóa/cập nhật container không mất dữ liệu:

| Volume | Nội dung |
|---|---|
| `ulms_thuvien-sql-data` | Dữ liệu SQL Server (chỉ khi chạy SQL Server trong Docker) |
| `ulms_thuvien-backup` | Tệp sao lưu `.bak` (chỉ khi chạy SQL Server trong Docker) |
| `ulms_thuvien-uploads` | Ảnh bìa sách, ảnh thẻ bạn đọc tải lên |
| `ulms_thuvien-web-keys` | Khóa mã hóa cookie đăng nhập (mất thì mọi người phải đăng nhập lại) |
| `ulms_thuvien-logs` | Log ứng dụng (Serilog, theo ngày) |
| `ulms_thuvien-caddy-data`, `-config`, `-logs` | Chứng chỉ HTTPS, cấu hình, log truy cập của Caddy |

## 2. Yêu cầu máy chủ

| Mục | Tối thiểu |
|---|---|
| Hệ điều hành | Linux x64 (Ubuntu 22.04/24.04) hoặc Windows Server / Windows 10–11 có Docker Desktop (WSL 2) |
| Docker | Docker Engine 24+ với Compose v2.24+ (cần cú pháp `!reset`) |
| RAM | 4 GB khi dùng SQL Server cài sẵn; **8 GB** khi chạy SQL Server trong Docker (container SQL giới hạn 2 GB) |
| Ổ đĩa | 20 GB trống + dung lượng bản sao lưu (giữ 14 bản tự động) |
| Mạng | Mở cổng 80 và 443 vào máy chủ. Tên miền thật (vd `thuvien.truong.edu.vn`) trỏ về máy chủ nếu muốn chứng chỉ công khai |

## 3. Chuẩn bị lần đầu

1. Cài Docker, kiểm tra: `docker compose version`.
2. Lấy mã nguồn: `git clone https://github.com/KitMeii/University_Library_Management_System_ULMS.git ulms && cd ulms`.
3. Tạo `.env` từ mẫu: `cp .env.example .env` rồi sửa (**không commit `.env`**, đặt quyền chỉ quản trị đọc được: `chmod 600 .env`).

### 3.1. Các giá trị bắt buộc trong `.env`

| Biến | Giá trị |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `MSSQL_SA_PASSWORD` | Mật khẩu `sa` (SQL trong Docker: tự đặt; SQL cài sẵn: mật khẩu sẵn có) – theo **chính sách mật khẩu máy chủ thật** bên dưới |
| `SVC_QUANTRI_PASSWORD` … `SVC_VIPHAM_PASSWORD` | 4 mật khẩu khác nhau cho login SQL của từng service – cùng chính sách |
| `JWT_SECRET_KEY` | Chuỗi ngẫu nhiên ≥ 32 ký tự, ví dụ `openssl rand -base64 48` |
| `ADMIN_PASSWORD` | Mật khẩu ban đầu của tài khoản `admin` (N4). Lần đăng nhập đầu tiên hệ thống **bắt đổi mật khẩu** |
| `ULMS_DOMAIN` | Tên miền (vd `thuvien.truong.edu.vn`) hoặc IP nội bộ / `localhost` |
| `ULMS_ACME_EMAIL` | Email nhận thông báo chứng chỉ Let's Encrypt (chỉ khi dùng tên miền thật) |

**Chính sách mật khẩu SQL cho máy chủ thật (bắt buộc – GIA_DINH D85).** `MSSQL_SA_PASSWORD` và 4 mật khẩu `SVC_*` phải:
- dài **ít nhất 8 ký tự**;
- có **chữ hoa**, **chữ thường**, **chữ số** và **ký tự đặc biệt** (ví dụ `# @ ! % & * - _ + =`);
- **không** chứa `;` `"` `'` `$` `\` (các ký tự này phá chuỗi kết nối / tệp `.env`).

SQL Server trong Docker từ chối mật khẩu `sa` yếu (container không khởi động được). Lệnh triển khai `scripts/trien-khai.sh prod`
(Windows: `.\scripts\trien-khai.ps1 prod`) **kiểm tra trước** và dừng với thông báo rõ biến nào thiếu điều kiện gì; kiểm tra riêng:
`sh scripts/trien-khai.sh kiem-mat-khau`. Kịch bản nghiệm thu `scripts/kiem-tra-trien-khai.sh` cũng kiểm tra các mật khẩu `SVC_*`
(mật khẩu `sa` của nó là mật khẩu ngẫu nhiên đủ mạnh, tự sinh cho SQL Server tạm).
Máy **phát triển** (`docker compose up -d --build`, SQL Server cài sẵn trên máy lập trình viên) **không** bị kiểm tra – giữ mật khẩu `.env` hiện có.

### 3.2. Chọn SQL Server

| | SQL Server trong Docker (khuyến nghị cho máy chủ mới) | SQL Server cài sẵn |
|---|---|---|
| `.env` | `COMPOSE_PROFILES=sql-docker`<br>`DB_SERVER=sqlserver`<br>`SAO_LUU_THU_MUC=/var/opt/mssql/backup`<br>`MSSQL_PID=Express` (miễn phí) hoặc `Standard`/`Enterprise` theo giấy phép của trường | `COMPOSE_PROFILES=` (trống)<br>`DB_SERVER=<máy SQL>,1433`<br>`SAO_LUU_THU_MUC=` (trống = thư mục Backup mặc định) |
| Ghi chú | `Developer` **không được** dùng cho môi trường thật | Bật TCP/IP 1433 + SQL Server Authentication; tường lửa chỉ cho máy chủ Docker kết nối |

Quyền SQL Server cần có: tài khoản trong `MSSQL_SA_PASSWORD` phải là **sysadmin** (migrator tạo CSDL, login `svc_*`;
chức năng Sao lưu – phục hồi 04.00.09 chạy `BACKUP`/`RESTORE`, chuyển CSDL `SINGLE_USER`). Các service nghiệp vụ chỉ dùng login riêng
`svc_*`, mỗi login chỉ ghi được schema của mình.

### 3.3. Thông tin thư viện (GIA_DINH D75)

Các biến `THUVIEN_*` hiển thị ở header (giờ mở cửa hôm nay), cột **Liên hệ** ở footer và khu **Thông tin thư viện** trên trang chủ.
`.env.example` đang để **chỗ giữ** trong ngoặc vuông – thay bằng thông tin thật trước khi mở cho bạn đọc:

```
THUVIEN_TEN_TRUONG=Đại học ABC          # hiển thị “Thư viện Đại học ABC”, cũng in trên các mẫu phiếu MB02–MB07
THUVIEN_DIA_CHI=Số 1 Đường X, Quận Y, TP Z
THUVIEN_EMAIL=thuvien@abc.edu.vn
THUVIEN_DIEN_THOAI=028 1234 5678
THUVIEN_GIO_T2=7:00 – 20:00            # … T3–T6 tương tự
THUVIEN_GIO_T7=7:30 – 17:00
THUVIEN_GIO_CN=Nghỉ
THUVIEN_NOI_QUY=                        # ghi chú thêm (tùy chọn)
```

Để trống một biến thì dòng đó không hiển thị. Nội quy mượn trả / đặt chỗ trên trang chủ tự lấy từ quy định của hệ thống
(số ngày mượn, số bản tối đa theo đối tượng, gia hạn, giữ chỗ 24 giờ, tối đa 3 đặt chỗ). Giá trị mặc định theo SPEC mục 4 nằm ở
`BaseCore.Common/NghiepVu/QuyDinhOptions.cs` (mục cấu hình `ThuVien:QuyDinh`); muốn đổi theo quy định thật của trường thì thêm biến môi trường
`ThuVien__QuyDinh__…` cho service `muontra` (và `quantri` nếu đổi mật khẩu mặc định) trong một tệp compose bổ sung – web tự lấy theo, không sửa `THUVIEN_NOI_QUY`.

## 4. Khởi động

```bash
sh scripts/trien-khai.sh prod             # Windows: .\scripts\trien-khai.ps1 prod
# = kiểm tra mật khẩu SQL (mục 3.1) → docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build → ps
docker compose -f docker-compose.yml -f docker-compose.prod.yml ps        # chờ mọi container "healthy" (2–5 phút lần đầu)
```

> Luôn ghi **cả hai** `-f`. Chạy `docker compose up` không có `-f` sẽ nạp `docker-compose.override.yml` (Development, dữ liệu mẫu).
> Có thể đặt sẵn trong `.env`: `COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml` (Windows dùng dấu `;`) rồi chỉ cần `docker compose up -d`.

Sau khi healthy:
1. Mở `https://<ULMS_DOMAIN>/quan-tri/dang-nhap`, đăng nhập `admin` / `ADMIN_PASSWORD` → hệ thống bắt đổi mật khẩu.
2. Vào 04.00.01 tạo tài khoản nhân viên N1–N3 (mật khẩu mặc định `Thuvien@123`, nhân viên đổi sau khi đăng nhập).
3. Nhập danh mục (khu vực, khoản thu, sách, bạn đọc) ở các giao diện 6.1; bạn đọc kích hoạt tài khoản cổng tại `/kich-hoat`.
4. Kiểm tra 04.00.09: chọn thư mục sao lưu, sao lưu thử một bản.

### 4.1. HTTPS

- **Tên miền thật** + cổng 80/443 mở ra Internet: Caddy tự xin và gia hạn chứng chỉ Let's Encrypt, không cần làm gì thêm.
- **Mạng nội bộ / IP** (`ULMS_DOMAIN=192.168.1.10` hoặc `localhost`): Caddy dùng CA nội bộ. Cài chứng chỉ gốc lên máy trạm để trình duyệt tin cậy:
  ```bash
  docker compose -f docker-compose.yml -f docker-compose.prod.yml cp caddy:/data/caddy/pki/authorities/local/root.crt ./ulms-root.crt
  ```
  Windows: mở `ulms-root.crt` → Install Certificate → Local Machine → *Trusted Root Certification Authorities* (có thể phát hành qua Group Policy).
- **Đã có reverse proxy của trường** (IIS/Nginx/F5): bỏ service `caddy` (`docker compose ... up -d --scale caddy=0`), mở cổng web bằng một tệp
  compose riêng `ports: ["127.0.0.1:8080:8080"]`, cho proxy của trường chuyển tới `http://127.0.0.1:8080` kèm `X-Forwarded-Proto: https`
  và `X-Forwarded-For`. Web tin header chuyển tiếp từ mọi địa chỉ nên **không được** mở cổng 8080 ra ngoài.

## 5. Kiểm tra nghiệm thu triển khai

Có sẵn kịch bản kiểm tra tự động trên một bộ hệ thống **riêng** (project `ulms-trienkhai`, SQL Server trong Docker, cổng 18081/18443),
không đụng hệ thống đang chạy:

```bash
sh scripts/kiem-tra-trien-khai.sh       # kết quả: artifacts/test-results/kiem_tra_trien_khai.txt
```

Nội dung: dựng từ đầu chỉ với Docker + `.env`; mọi container healthy; chỉ Caddy mở cổng; HTTP → HTTPS; HSTS, CSP, nosniff, không lộ
header `Server`; cookie Secure/HttpOnly/SameSite; Production không có dữ liệu mẫu; dừng SQL Server → service báo `unhealthy`, bật lại → tự
`healthy`; `down` rồi `up`: CSDL, ảnh tải lên, bản sao lưu còn nguyên; mọi image chạy user `app` (không phải root); giá trị bí mật trong
`.env` không xuất hiện trong lịch sử/cấu hình image.

## 6. Sao lưu – phục hồi (SPEC 6.3.5, file 11 mục 2)

- **Tự động**: 23:00 hằng ngày (giờ Việt Nam), tệp `TuDong_ThuVien_yyyyMMdd_HHmmss.bak`, giữ 14 bản gần nhất.
- **Thủ công**: N4 → 04.00.09 → Sao lưu.
- **Chép ra ngoài máy chủ** (bắt buộc – hỏng ổ đĩa sẽ mất cả CSDL lẫn bản sao lưu):
  ```bash
  # SQL Server trong Docker
  docker compose -f docker-compose.yml -f docker-compose.prod.yml cp sqlserver:/var/opt/mssql/backup ./sao-luu-$(date +%F)
  # rồi chép thư mục đó sang NAS / máy khác (rsync, robocopy…). Lập lịch cron / Task Scheduler sau 23:30 mỗi ngày.
  ```
  SQL Server cài sẵn: chép thư mục Backup của SQL Server (vd `C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup`).
- Sao lưu thêm volume ảnh tải lên:
  ```bash
  docker run --rm -v ulms_thuvien-uploads:/du-lieu -v "$PWD":/ra alpine tar czf /ra/uploads-$(date +%F).tgz -C /du-lieu .
  ```
- **Phục hồi**: chỉ N4, 04.00.09 → chọn tệp → xác nhận 2 lần. Hệ thống tự sao lưu bản hiện tại (`TruocPhucHoi_*.bak`), chuyển CSDL sang
  `SINGLE_USER WITH ROLLBACK IMMEDIATE` → `RESTORE` → `MULTI_USER`, hủy mọi phiên đăng nhập khác. Sau đó khởi động lại các service:
  `docker compose -f docker-compose.yml -f docker-compose.prod.yml restart`.
- Phục hồi khi web không chạy được: chép `.bak` vào thư mục sao lưu rồi chạy `RESTORE DATABASE ThuVien FROM DISK = '…' WITH REPLACE`
  bằng SSMS hoặc `docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C`.

## 7. Cập nhật phiên bản

```bash
git pull                                                         # hoặc chép mã nguồn phiên bản mới
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

- `migrator` tự áp dụng migration mới khi khởi động; các service chờ migrator chạy xong.
- **Sao lưu thủ công trước khi cập nhật.** Quay lại bản cũ: `git checkout <phiên bản cũ>` → build lại → phục hồi bản sao lưu trước cập nhật
  (migration không tự lùi).
- Kịch bản dữ liệu chạy một lần nằm ở `Database/mot_lan/` (vd `D77_cap_nhat_tu_khoa_mau.sql` – chỉ áp dụng cho CSDL có dữ liệu mẫu);
  CSDL thật không cần chạy.

## 8. Theo dõi và xử lý sự cố

| Việc | Lệnh (`DC="docker compose -f docker-compose.yml -f docker-compose.prod.yml"`) |
|---|---|
| Trạng thái, health | `$DC ps` |
| Log trực tiếp | `$DC logs -f --tail 200 web muontra` |
| Log tệp theo ngày | `$DC exec web ls /app/logs` → `$DC cp web:/app/logs ./logs` |
| Log truy cập HTTPS | `$DC exec caddy tail -n 100 /var/log/caddy/truy-cap.log` |
| Khởi động lại một service | `$DC restart muontra` |
| Tài nguyên | `docker stats` |

| Hiện tượng | Nguyên nhân thường gặp | Cách xử lý |
|---|---|---|
| `migrator` thoát mã khác 0, các service không chạy | Sai `MSSQL_SA_PASSWORD`/`DB_SERVER`, SQL Server chưa bật TCP/IP | `$DC logs migrator`; sửa `.env` → `$DC up -d` |
| Service `unhealthy` | Mất kết nối SQL Server | Kiểm tra SQL Server; service tự healthy lại khi SQL chạy (đã kiểm chứng ở mục 5) |
| Trình duyệt báo chứng chỉ không tin cậy | Dùng IP/`localhost` (CA nội bộ) | Cài `ulms-root.crt` (mục 4.1) hoặc dùng tên miền thật |
| Caddy không xin được chứng chỉ | Cổng 80/443 chưa mở, tên miền chưa trỏ đúng | `$DC logs caddy`; kiểm tra DNS + tường lửa |
| Mọi người bị đăng xuất sau cập nhật | Volume `thuvien-web-keys` bị xóa | Không xóa volume này khi cập nhật |
| Đăng nhập bị chặn “thử lại sau” | Giới hạn tần suất đăng nhập (chống dò mật khẩu) | Chờ 1 phút; tài khoản nhân viên sai 5 lần bị khóa – N4 mở khóa ở 04.00.01 |
| Sao lưu báo lỗi quyền ghi | Thư mục sao lưu không thuộc user SQL Server | SQL trong Docker: service `sqlserver-quyen` tự cấp quyền khi khởi động; SQL cài sẵn: cấp quyền ghi cho tài khoản dịch vụ SQL |

⚠️ `docker compose down -v` **xóa toàn bộ volume** (CSDL trong Docker, bản sao lưu, ảnh, khóa cookie). Chỉ dùng khi muốn xóa sạch.

## 9. Bảo mật vận hành

- Chỉ mở 80/443; không mở 1433/1434/5000/8080 ra ngoài. Đặt `.env` quyền 600, không gửi qua email/chat.
- Đổi `ADMIN_PASSWORD` sau khi khởi tạo không có tác dụng (chỉ dùng lần đầu) – đổi mật khẩu admin trong hệ thống.
- Đổi `JWT_SECRET_KEY` → mọi phiên đăng nhập bị hủy (dùng khi nghi lộ khóa).
- Cập nhật image nền định kỳ: `docker compose ... build --pull` rồi `up -d` (giữ nguyên tag đã ghim trong Dockerfile / compose).
- Nhật ký đăng nhập (04.00.08) ghi mọi lần đăng nhập, đăng xuất, hết phiên của nhân viên; nhật ký hoạt động ghi mọi thao tác ghi dữ liệu.
