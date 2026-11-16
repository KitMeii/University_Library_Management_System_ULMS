# Database – kịch bản SQL của CSDL `ThuVien`

Các tệp `.sql` ở đây được **sinh tự động** từ migration, dữ liệu khởi tạo và quyền SQL mà `BaseCore.Migrator` dùng
(`sh scripts/xuat-sql.sh`), nên luôn khớp mã nguồn. Không sửa tay; thêm migration xong thì chạy lại lệnh sinh.

| Tệp | Nội dung | Chạy lại nhiều lần |
|---|---|---|
| `01_CSDL_LuocDo.sql` | Tạo CSDL `ThuVien`, 4 schema `quantri`, `khosach`, `muontra`, `vipham`, 27 bảng, ràng buộc, sequence, thủ tục `muontra.usp_CapSoChungTu` | Được (chỉ áp dụng migration còn thiếu) |
| `02_DuLieuKhoiTao.sql` | 4 nhóm người dùng N1–N4, 173 quyền (SPEC 3.2, 3.4), phân quyền mặc định | Được (không ghi đè phân quyền N4 đã chỉnh) |
| `03_LoginDichVu.sql` | Login `svc_quantri`, `svc_khosach`, `svc_muontra`, `svc_vipham` + quyền theo schema; tùy chọn tạo `admin` | Được (đổi lại mật khẩu login) |

## Cách 1 (khuyên dùng) – để hệ thống tự tạo

Không cần chạy tệp nào. Khi `docker compose up -d --build`, container `migrator` tự áp dụng đúng các bước trên
(và nạp **dữ liệu mẫu** ở môi trường Development) vào SQL Server khai báo bởi `DB_SERVER` trong `.env`.

## Cách 2 – tạo bằng SSMS

1. Kết nối SSMS tới SQL Server (ví dụ `TunKittt` hoặc `localhost`), **SQL Server Authentication**, login `sa`.
2. Mở và chạy (F5) lần lượt `01_CSDL_LuocDo.sql`, `02_DuLieuKhoiTao.sql`.
3. Mở `03_LoginDichVu.sql`, bật **Query → SQLCMD Mode**, điền 4 dòng `:setvar SVC_…_PASSWORD` **giống hệt** các biến
   cùng tên trong `.env` (đó là mật khẩu các service dùng để kết nối), rồi chạy. Không lưu/commit tệp đã điền mật khẩu.
   Muốn tạo luôn `admin`: lấy chuỗi băm bằng `docker compose run --rm migrator --bam-mat-khau "<mật khẩu>"`
   rồi điền vào `:setvar MATKHAU_ADMIN_BAM`.
4. `docker compose up -d --build` – migrator thấy CSDL đã đủ, chỉ bổ sung phần còn thiếu.

## Kết nối hệ thống tới SQL Server cài sẵn trên máy (GIA_DINH D44)

Trong `.env`:

```env
COMPOSE_PROFILES=                      # trống: không chạy SQL Server trong Docker
DB_SERVER=host.docker.internal,1433    # container gọi về SQL Server của máy
MSSQL_SA_PASSWORD=<mật khẩu sa của máy>
SAO_LUU_THU_MUC=                       # trống: dùng thư mục Backup mặc định của SQL Server
```

SQL Server cần: bật **TCP/IP** cổng 1433 (SQL Server Configuration Manager → Protocols), bật **SQL Server and Windows
Authentication mode** (Server Properties → Security). Muốn quay lại SQL Server trong Docker: `COMPOSE_PROFILES=sql-docker`,
`DB_SERVER=sqlserver`, `SAO_LUU_THU_MUC=/var/opt/mssql/backup`.

## Xóa sạch để làm lại (chỉ CSDL của dự án)

```sql
ALTER DATABASE [ThuVien] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [ThuVien];
-- Login dùng chung cấp máy chủ (giữ lại cũng được – migrator sẽ đặt lại mật khẩu):
-- DROP LOGIN [svc_quantri]; DROP LOGIN [svc_khosach]; DROP LOGIN [svc_muontra]; DROP LOGIN [svc_vipham];
```
