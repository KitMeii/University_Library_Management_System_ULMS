# GIẢ ĐỊNH VÀ QUYẾT ĐỊNH ĐÃ DUYỆT

Chỉ ghi các quyết định đã được chủ dự án duyệt. Mỗi mục: ngày, nội dung, lý do.

| # | Ngày | Quyết định | Nguồn |
|---|---|---|---|
| D1 | 15/11/2026 | Ánh xạ đường dẫn: `basecore/` = `Backend/`; `docs/` = `Prompt/Prompt_ClaudeCode_HeThong/docs/`; ứng dụng web MVC đặt ở `Frontend/`; file compose, `.env.example`, `README.md`, `.gitignore` ở gốc | Yêu cầu khởi động của chủ dự án |
| D3 | 15/11/2026 | **Bí mật (R1):** trước commit đầu tiên thay toàn bộ MongoDB/Redis URI, Secret GUID, khóa JWT, chuỗi kết nối trong `Backend/**/appsettings*.json` và hằng số trong code bằng chỗ trống/biến môi trường. Không cần kiểm tra máy chủ 178.128.26.135 (không thuộc chủ dự án) | Duyệt Pha 0A |
| D4 | 15/11/2026 | **Số chứng từ giữ C(10):** `PMyyMM####`, `GHyyMM####`, `PTyyMM####`, `BByyMM####`, đánh lại từ 0001 mỗi tháng (ví dụ `PM26110001`). Đã cập nhật SPEC mục 4, file 02 (mục 1, X08: `SOCHUNGTU.ngay` = ngày đầu tháng) | Duyệt Pha 0A (giải R9) |
| D5 | 15/11/2026 | **Mật khẩu giữ C(64):** không dùng `PasswordHasher<T>`; hasher tự viết `Rfc2898DeriveBytes.Pbkdf2` – HMAC-SHA256, salt 16 byte ngẫu nhiên, khóa 32 byte, số vòng lặp trong cấu hình (mặc định 600.000) → lưu `Base64(salt‖hash)` = 48 byte = đúng 64 ký tự; so sánh bằng `CryptographicOperations.FixedTimeEquals`. Dùng chung cho NGUOIDUNG và TAIKHOAN_BANDOC. Đã cập nhật file 01, 02, 11 | Duyệt Pha 0A (giải R8) |
| D6 | 15/11/2026 | **.NET 10 LTS** cho mọi project; Docker dùng `mcr.microsoft.com/dotnet/sdk:10.0` / `aspnet:10.0`; chủ dự án tự cài SDK 10 trên máy | Duyệt Pha 0A (giải R3) |
| D7 | 15/11/2026 | Sửa `AuthController.Login` không in mật khẩu ra console (Pha 1) | Duyệt Pha 0A (R2) |
| D8 | 15/11/2026 | **Duyệt XÓA toàn bộ mục 9** của `BASECORE_PHAN_TICH.md` (AuditLog, LogService, UnitTest, IOTService, WebClient, Test, Examples/ và các file/gói liệt kê). Xóa trong **một commit riêng** ở Pha 1. Nhật ký nghiệp vụ: bảng NHATKY_DANGNHAP / NHATKY_HOATDONG theo SPEC; log kỹ thuật: Serilog; test viết mới | Duyệt Pha 0A |
| D9 | 15/11/2026 | **Ưu tiên giao diện Bài 4:** đăng nhập + danh mục + quản trị (Pha 2), 4 form chứng từ + mẫu in MB02–MB05 (Pha 3), báo cáo MB06–MB07 (Pha 4) xong và có ảnh chụp **trước** phần cổng bạn đọc | Yêu cầu chủ dự án |
| D10 | 15/11/2026 | **CSDL:** SQL Server 2022 trong Docker, **một database, mỗi service một schema riêng, chỉ ghi vào bảng của mình**. Container map cổng **1434** ra máy (`localhost,1434`) để mở bằng SSMS, không đụng 1433 của bản cài sẵn. README ghi cách kết nối SSMS: Server `localhost,1434`, SQL Server Authentication, `sa`, mật khẩu trong `.env`, tích Trust server certificate | Yêu cầu chủ dự án |
| D11 | 15/11/2026 | **4 service nghiệp vụ** (schema cùng tên): `quantri` (NHOMNGUOIDUNG, NGUOIDUNG, QUYEN, PHANQUYEN, NHATKY_DANGNHAP, NHATKY_HOATDONG; sao lưu), `khosach` (SACH, KHUVUC, DAUSACH, DAUSACH_BANSACH, THELOAI), `muontra` (BANDOC, PHIEUMUON, CT_PHIEUMUON, PHIEUGIAHAN, CT_PHIEUGIAHAN, PHIEUTRA, CT_PHIEUTRA, PHIEUDATCHO, SOCHUNGTU, YEUTHICH, THONGBAO, TAIKHOAN_BANDOC), `vipham` (KHOANTHU, BIENBAN, CT_BIENBAN) | Q1 |
| D12 | 15/11/2026 | **TAIKHOAN_BANDOC thuộc muontra** (khác gợi ý file 12). Lý do: file 07 yêu cầu tạo tài khoản “Chưa kích hoạt” **trong cùng transaction** khi N1 lưu thẻ BANDOC (01.01.01); BANDOC thuộc muontra, mà mỗi service chỉ ghi bảng của mình → đặt cùng service mới bảo đảm nguyên tử, không cần giao dịch phân tán. muontra cấp JWT cho bạn đọc | Q2 |
| D13 | 15/11/2026 | **Không dùng message bus.** Các bước “chuyển cho bộ phận …” hiện thực bằng đọc chéo schema (quyền SELECT) + INSERT trực tiếp NHATKY_HOATDONG, THONGBAO trong transaction nghiệp vụ (mọi service chỉ có quyền INSERT trên 2 bảng này) | Q3 |
| D14 | 15/11/2026 | **Giữ khóa ngoại vật lý giữa các schema** đúng SPEC. Ranh giới ghi được ép bằng login SQL riêng từng service | Q4 |
| D15 | 15/11/2026 | Danh sách “chuyển đến” (dgvChuyenDen) và “trường hợp vi phạm chờ lập biên bản” **suy ra từ dữ liệu** theo `KE_HOACH_PHA_0.md` mục 4, không thêm bảng | Q5 |
| D16 | 15/11/2026 | **Bản sách có thể mượn/đặt chỗ** = `SACH.tinhtrang = 'Sẵn sàng'` và **không thuộc phiếu mượn chưa trả** và không đang được giữ chỗ (còn hạn) cho người khác. Chỉ là quy tắc trong code, không đổi bảng; áp dụng cho cả lập phiếu mượn và đặt chỗ | Q6 |
| D17 | 15/11/2026 | Duyệt thêm gói **Yarp.ReverseProxy** (web làm BFF chuyển `/api/**` → gateway, gắn JWT từ cookie) và **Deque.AxeCore.Playwright** (kiểm accessibility) | Q7 |
| D18 | 15/11/2026 | Làm phẳng `Backend/BaseCore/BaseCore` → `Backend/`; project `BaseCore.QuanTriService` (từ AuthService), `BaseCore.KhoSachService` (từ APIService), `BaseCore.MuonTraService`, `BaseCore.ViPhamService`; một solution `ULMS.slnx` ở gốc | Q8 |
| D19 | 15/11/2026 | Mã tự sinh: `ma_nd` = `ND####` (6 ký tự ≤ C(10)); `ma_dangnhap` = `DN` + yyMMdd + 7 số = 15 ký tự = C(15); `ma_hoatdong` = `HD` + yyMMdd + 7 số = 15 = C(15). Đã kiểm độ dài khớp SPEC | Q9 |
| D20 | 15/11/2026 | 04.00.09 giữ đúng control SPEC; **không** thêm nút tải tệp sao lưu về / tải lên. `btnChon` chọn thư mục/tệp trong thư mục sao lưu của máy chủ | Q10 |
| D21 | 15/11/2026 | `appsettings` mục `ThuVien:ThongTin`: `TenTruong` = “[TÊN TRƯỜNG CỦA BẠN]”; địa chỉ, giờ mở cửa, email, điện thoại, nội quy để trống (chủ dự án bổ sung sau). Khu vực không có dữ liệu thì không hiển thị | Q11 |
| D22 | 15/11/2026 | Development: `admin/Thuvien@123` không bắt đổi mật khẩu lần đầu; Production bắt đổi | Q12 |
| D23 | 15/11/2026 | **Chế độ chụp ảnh** ẩn nhãn “DỮ LIỆU MẪU”: cấu hình `ThuVien:HienNhanDuLieuMau` (mặc định true ở Development), đặt false khi chụp bộ ảnh Bài 4 | Q13 |
| D24 | 15/11/2026 | Chủ dự án có điện thoại thật để kiểm thử cổng bạn đọc (loại máy: chưa ghi rõ – “[iPhone / Android]”) | Q14 |
| D25 | 16/11/2026 | `PHIEUDATCHO.so_pmuon` dùng **nvarchar(10)** thay vì nvarchar(20) của file 02: là khóa ngoại tới `PHIEUMUON.so_pmuon` C(10) nên phải cùng kiểu (SQL Server yêu cầu); D4 cũng đã cố định số phiếu 10 ký tự | Phát sinh Pha 1 – duyệt 16/11/2026 |
| D26 | 16/11/2026 | Mã quyền vượt C(10) được rút gọn: bảng B05 `PMUON_C/E/R/D`, B06 `PGHAN_*`, B07 `PTRA_*` (thống nhất cách đặt), quyền mở rộng `DATCHO_GIAO` (11 ký tự) → **`DATCHO_GH`**. Các mã khác giữ đúng SPEC (`SACH_C`, `TT001`…) | Phát sinh Pha 1 (Q9) – duyệt 16/11/2026 |
| D27 | 16/11/2026 | Đếm số lần sai mật khẩu liên tiếp của nhân viên lưu **trong bộ nhớ service quantri** (NGUOIDUNG không có cột đếm, không được thêm cột); trạng thái “Khóa” lưu bền vào `NGUOIDUNG.tinhtrang` và ghi 1 dòng NHATKY_HOATDONG. Khởi động lại service thì bộ đếm về 0 | Phát sinh Pha 1 – duyệt 16/11/2026 |
| D28 | 16/11/2026 | Thêm 9 mã quyền chức năng quản trị `QT040001`…`QT040009` (04.00.01–04.00.09) cấp cho N4 – SPEC 3.4 không có mã tiến trình cho nhóm 04.00.xx nhưng 6.3.3 cần “quyền quản trị của N4” để không cho bỏ | Phát sinh Pha 1 – duyệt 16/11/2026 |
| D29 | 16/11/2026 | Quyền đọc chéo cấp ở mức **schema** (SELECT toàn schema khác) thay vì từng bảng, kèm **DENY SELECT cột `matkhau`** của NGUOIDUNG và TAIKHOAN_BANDOC với service không sở hữu. Ghi vẫn chỉ trong schema của mình | Phát sinh Pha 1 – duyệt 16/11/2026 |
| D30 | 16/11/2026 | Cổng SSMS cấu hình được bằng biến `SQL_PORT` (mặc định **1434** theo D10). Trên máy hiện tại 1434 đang bị container `da_nnpl-mssql-1` của dự án khác chiếm nên `.env` tạm đặt **1435** | Phát sinh Pha 1 – duyệt 16/11/2026 |
| D31 | 16/11/2026 | Giới hạn đăng nhập theo IP người dùng cấu hình bằng `GioiHanTanSuat:DangNhapMoiPhut` (mặc định 10/phút, dev 300/phút để kiểm thử tự động chạy được) | Phát sinh Pha 1 – duyệt 16/11/2026 |
| D32 | 16/11/2026 | Menu dashboard chỉ hiện mục người dùng có quyền; chức năng chưa đến pha làm hiển thị trang “Giao diện này được xây dựng ở Pha n” (không link chết, không dữ liệu giả). “Danh mục Bạn đọc” ẩn với N1 vì N1 dùng “01.01.01 Lưu thông tin Thẻ Bạn đọc” (cùng giao diện 6.1.3, SPEC 3.3) | Phát sinh Pha 1 – duyệt 16/11/2026 |
| D33 | 16/11/2026 | `btnThoat` (6.3.1) trên web: hỏi xác nhận (modal) rồi về trang chủ cổng | Giả định Pha 0 – duyệt 16/11/2026 |
| D34 | 16/11/2026 | 04.00.01: ngoài SPEC, **không cho tự khóa / tự bỏ nhóm N4 của chính tài khoản đang đăng nhập** và **không khóa / chuyển nhóm tài khoản N4 hoạt động cuối cùng** (cùng tinh thần 6.3.3 “tránh hệ thống không còn tài khoản quản trị”) | Phát sinh Pha 2 – duyệt 16/11/2026 |
| D35 | 16/11/2026 | `BANDOC.anh`: tải ảnh JPG/PNG/WebP ≤ 2 MB (kiểm tra chữ ký tệp, tên ngẫu nhiên). **SkiaSharp 3.119.2 (MIT)** giải mã, xoay theo EXIF, thu nhỏ vừa khung 600×800 (không phóng to), lưu **WebP** chất lượng 82 (bỏ siêu dữ liệu EXIF/GPS), chặn ảnh > 40 triệu điểm ảnh; lưu volume `thuvien-uploads`, đường dẫn `/uploads/ban-doc/{guid}.webp` (50 ký tự ≤ C(100)). Không dùng ImageSharp | Phát sinh Pha 2 – duyệt 16/11/2026 (chủ dự án chọn SkiaSharp) |
| D36 | 16/11/2026 | 04.00.09 chạy bằng kết nối quản trị CSDL riêng (`ConnectionStrings:QuanTriCsdl`, quyền sa) – chỉ service quantri, chỉ N4. Tiến trình % thật (STATS của SQL Server). Phục hồi: xác nhận 2 lần, tự sao lưu `TruocPhucHoi_…`; **sau phục hồi hủy ngay mọi phiên đăng nhập khác (nhân viên và bạn đọc)**: đặt mốc thu hồi (extended property `ULMS_MocThuHoiPhien` của CSDL – không thêm bảng ngoài SPEC), mọi service từ chối JWT cấp trước mốc (401 `PHIEN_DA_BI_HUY`), web kiểm tra cookie mỗi 15 giây và xóa cookie, trang đăng nhập báo lý do; đóng các lượt `NHATKY_DANGNHAP` đang mở; ghi NHATKY_HOATDONG “Hủy mọi phiên đăng nhập khác…”. Phiên của chính người phục hồi được giữ | Phát sinh Pha 2 – duyệt 16/11/2026 (chủ dự án yêu cầu hủy ngay) |
| D37 | 16/11/2026 | Sao lưu tự động hằng ngày 23:00, tệp `TuDong_ThuVien_yyyyMMdd_HHmmss.bak`, giữ 14 bản gần nhất (file 11); nhật ký ghi tên tài khoản N4 hoạt động đầu tiên kèm “(hệ thống)” vì NHATKY_HOATDONG.ma_nd bắt buộc FK NGUOIDUNG | Phát sinh Pha 2 – duyệt 16/11/2026 |
| D38 | 16/11/2026 | Mã tự sinh: `ma_nd` = ND#### do hệ thống cấp (SPEC “tự sinh”); `ma_bandoc` do N1 nhập (SPEC C, R, D) nhưng hệ thống **gợi ý** BD#### kế tiếp khi bấm Thêm | Phát sinh Pha 2 – duyệt 16/11/2026 |
| D39 | 16/11/2026 | 6.1.1: chỉ **btnTimKiem có từ khóa** (03.01.02) mới loại bản sách thuộc phiếu mượn chưa trả; mở form và `cboLocTinhTrang` hiển thị đủ bản sách | Phát sinh Pha 2 – duyệt 16/11/2026 |
| D40 | 16/11/2026 | `btnKiemTraDK` kiểm thêm **số bản tối đa theo đối tượng** (điều kiện mượn SPEC mục 4) ngoài hạn thẻ, sách quá hạn, vi phạm chưa thanh toán ghi ở 6.1.3 | Phát sinh Pha 2 – duyệt 16/11/2026 |
| D41 | 16/11/2026 | Xóa Bạn đọc: chỉ khi **chưa có phiếu mượn, phiếu gia hạn, phiếu trả, biên bản vi phạm** (và đặt chỗ); nếu đã có → 409 `DA_CO_CHUNG_TU` nêu rõ loại chứng từ. Được xóa thì xóa kèm TAIKHOAN_BANDOC, YEUTHICH, THONGBAO (bảng mở rộng cùng service) trong cùng transaction. Xóa bản sách: xóa kèm DAUSACH_BANSACH; chặn nếu có CT_PHIEUMUON, CT_BIENBAN, PHIEUDATCHO | Phát sinh Pha 2 – duyệt 16/11/2026 (chủ dự án bổ sung gia hạn/trả/biên bản) |
| D42 | 16/11/2026 | DatePicker tự làm: hiển thị `dd/MM/yyyy`, chỉ chọn trên lịch (không gõ tay), giữ đúng id control SPEC (`dtpTuNgay`, `dtpNgaySinh`…) – ô ngày của trình duyệt hiển thị theo ngôn ngữ hệ điều hành nên không bảo đảm định dạng | Phát sinh Pha 2 – duyệt 16/11/2026 |
| D43 | 16/11/2026 | Tham số nghiệp vụ SPEC mục 4 đặt ở `ThuVien:QuyDinh` (số ngày/số bản theo đối tượng, gia hạn 7 ngày × 1 lần, mật khẩu mặc định) – dùng chung các service | Phát sinh Pha 2 – duyệt 16/11/2026 |
| D2 | 15/11/2026 | Git: remote `https://github.com/KitMeii/University_Library_Management_System_ULMS.git`, nhánh `main`; kiểm tra bí mật trước commit đầu; xóa phần thừa trong commit riêng; commit + push sau mỗi pha được duyệt | Yêu cầu khởi động của chủ dự án |
| D44 | 16/11/2026 | **CSDL dùng SQL Server cài sẵn trên máy** (SSMS, `TunKittt`, SQL Server Authentication, `sa`) theo yêu cầu chủ dự án: `.env` đặt `DB_SERVER=host.docker.internal,1433`, `COMPOSE_PROFILES=` (không chạy container SQL Server); vẫn giữ cách chạy SQL Server trong Docker (`COMPOSE_PROFILES=sql-docker`, `DB_SERVER=sqlserver`). Mật khẩu sa chỉ nằm trong `.env` (không commit). Thư mục sao lưu: `SaoLuu:ThuMuc` trống → `InstanceDefaultBackupPath` của SQL Server (Windows: `C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup`), ghép đường dẫn đúng dấu phân cách. Thư mục `Database/` chứa kịch bản `.sql` (lược đồ idempotent, dữ liệu khởi tạo, login service) **sinh từ migrator** (`scripts/xuat-sql.sh`) để tạo CSDL bằng SSMS | Yêu cầu chủ dự án 16/11/2026 |
| D45 | 16/11/2026 | **Điều kiện gia hạn** (SPEC 6.2.2 chỉ ghi “kiểm tra điều kiện gia hạn”): (1) số lần gia hạn được đồng ý của bản sách < `SoLanGiaHanToiDa` (1); (2) ngày đề nghị ≤ hạn trả hiện hành (đã quá hạn thì không gia hạn); (3) thẻ bạn đọc còn hạn đến hạn trả mới; (4) bạn đọc không còn biên bản chưa thanh toán. Không đạt → “Không đồng ý” kèm lý do hệ thống ghi; nhân viên được chủ động ghi “Không đồng ý” kèm lý do, không được ép “Đồng ý” khi không đạt. Hạn trả hiện hành = hạn mới nhất của các lần gia hạn “Đồng ý” | Phát sinh Pha 3 – duyệt 16/11/2026 |
| D46 | 16/11/2026 | Trả sách nhiều lần (một Phiếu trả/Phiếu mượn): `PHIEUTRA.ngaytra` = ngày trả gần nhất; `so_ngay_quahan` của từng dòng tính theo ngày trả của lần đó. Ngày trả ≥ ngày mượn và ≤ hôm nay. Dòng đã lập biên bản vi phạm không sửa được | Phát sinh Pha 3 – duyệt 16/11/2026 |
| D47 | 16/11/2026 | **Bản sách chỉ bị chặn cho mượn khi**: (a) đang thuộc phiếu mượn chưa trả, **hoặc** (b) lần trả gần nhất là Mất/Hư hỏng mà **Bộ phận kho sách chưa xử lý** (03.03.02). “Đã xử lý” được ghi ở bảng mở rộng **X09 `khosach.XULY_CHUYENDEN`** (*bảng mở rộng thứ 9 – chủ dự án duyệt 16/11/2026, đã ghi vào file 02; chỉ có trong hệ thống, không đưa vào hồ sơ Word*; ma_bansach, so_chungtu = so_ptra, tientrinh = 03.03.02, tinhtrang_moi, ma_nd, thoigian_xuly) khi N3 bấm `btnCapNhatTT` trên dòng 03.03.02 **hoặc** đổi tình trạng bản sách bằng `btnSua`. Sau đó N3 sửa sách xong đặt lại “Sẵn sàng” thì **mượn lại được bình thường**, dòng 03.03.02 không xuất hiện lại (không còn suy “đã xử lý” từ SACH.tinhtrang). Dòng 03.03.02 chỉ hiện ở dgvChuyenDen sau khi N2 lập biên bản (02.03.01 → 03.03.02); từ lúc trả đến lúc N3 xử lý, bản sách vẫn bị chặn. Migration ghi nhận lùi các lần trả Mất/Hư hỏng đã được cập nhật SACH tương ứng. Khóa ứng dụng theo bản sách (`sp_getapplock`) trong transaction lưu phiếu để hai người không cùng cho mượn một bản. Không đáp ứng → gợi ý bản sách cùng tên, cùng tác giả còn sẵn (đổi bản sách – TT017) | Phát sinh Pha 3 – chủ dự án sửa 16/11/2026 (bỏ hạn chế sách hư hỏng đã sửa không mượn lại được) |
| D48 | 16/11/2026 | Form chứng từ trên web: thêm ô nhập số chứng từ cạnh `btnTimPhieu`/`btnTimBienBan` (`txtSoPhieu`, `txtSoBienBan` – SPEC ghi “nhập so_… để mở phiếu đã lưu” nhưng không đặt tên control); 6.2.4 thêm `cboBanSach`, `txtSoLuong` (chọn ma_bansach, soluong của dòng CT_BIENBAN) và lưới `dgvTruongHop` (TT075 – trường hợp vi phạm chờ lập biên bản, nhấp đúp để mở). Dòng chi tiết đã lưu chỉ sửa được, không bỏ khỏi chứng từ (SPEC: “bỏ dòng chưa lưu”); bổ sung bản sách chỉ vào Phiếu mượn lập trong ngày. btnIn: lưu (một API/transaction) → hộp tiến trình % → in trong khung ẩn trang `/quan-tri/in/…` (không mở tab, không bị chặn popup) + liên kết “Mở bản in” để in lại; mỗi mẫu in đủ **02 liên** trên 2 tờ (A5 cho MB02–MB04, A4 cho MB05), tiêu đề “THƯ VIỆN TRƯỜNG ĐẠI HỌC …” lấy `ThuVien:ThongTin:TenTruong` (để trống/[…] thì in dấu chấm). **Danh sách đầy đủ control thêm ngoài SPEC theo từng giao diện: mục “Phụ lục A” cuối tệp** | Phát sinh Pha 3 – duyệt 16/11/2026 |
| D49 | 16/11/2026 | **Kiểm thử chạy trên CSDL riêng `ThuVien_Test`**, không đụng CSDL ThuVien: `scripts/test-docker.sh` dựng bộ hệ thống kiểm thử riêng (project Compose `ulms-test`, mạng `thuvien-test-net`, web cổng 18080, `DB_NAME=ThuVien_Test`), migrator xóa – tạo lại `ThuVien_Test` + dữ liệu mẫu ở mỗi lần chạy (`ThuVien__TaoLaiCsdl`, chỉ chấp nhận tên CSDL kết thúc `_Test`), chạy kiểm thử, rồi gỡ bộ kiểm thử (`down -v`). Tệp sao lưu do kiểm thử tạo ghi vào **`SAO_LUU_THU_MUC_KIEM_THU`** (máy này: thư mục tạm riêng của dịch vụ SQL Server `C:\Windows\ServiceProfiles\MSSQLSERVER\AppData\Local\Temp\ULMS_KiemThu`), **không ghi vào thư mục Backup thật**; kiểm thử sao lưu tự xóa tệp của nó, cuối mỗi lần chạy migrator dọn mọi tệp `*ThuVien_Test_*.bak`, `KiemTra_*.bak` còn lại (`--don-sao-luu-kiem-thu`). Bộ ảnh giao diện chụp trước, trên CSDL vừa tạo lại | Yêu cầu chủ dự án 16/11/2026 |
| D50 | 16/11/2026 | **Pha 4 – Tổng quan N1**: “Top 10 sách được mượn nhiều” **tạm tính theo `SACH.ten_sach`** (gộp các bản sách cùng tên) cho tới khi có đầu sách ở cổng bạn đọc; **biểu đồ và KPI đặt chỗ ẩn** đến Pha 6 (Quầy đặt chỗ) thì bật lại | Quyết định chủ dự án 16/11/2026 |
| D51 | 16/11/2026 | **Cách tính MB06/MB07 theo kỳ.** Kỳ: bắt buộc từ ngày, đến ngày; từ ≤ đến; đến ngày ≤ hôm nay; tối đa 366 ngày (TT058, 422 theo trường). MB06: lượt mượn theo `ngaymuon`; lượt gia hạn theo `ngaylap_pgiahan`, chỉ đếm “Đồng ý” (danh sách chi tiết vẫn liệt kê cả “Không đồng ý”); lượt trả theo `PHIEUTRA.ngaytra` – trả nhiều lần trên một phiếu thì cả phiếu tính theo ngày trả gần nhất (CT_PHIEUTRA không có ngày riêng – D46); **đang mượn / quá hạn tính đến ngày cuối kỳ** (kể cả kỳ quá khứ), hạn trả dùng hạn trả hiện hành gồm các lần gia hạn “Đồng ý” lập đến ngày cuối kỳ. MB07: kỳ theo ngày của `thoigian_lap`; “tổng tiền thu trong kỳ” = SUM(tong_tien) biên bản lập trong kỳ đã thanh toán (Tiền mặt/Chuyển khoản) – CSDL không có ngày thanh toán riêng; liệt kê đủ mọi khoản thu (cả khoản 0 trường hợp) | Phát sinh Pha 4 – duyệt 16/11/2026 |
| D52 | 16/11/2026 | **Trang tổng quan theo nhóm** (file 09): mỗi tài khoản chỉ thấy tổng quan nhóm của mình (theo `ma_nhom` + quyền: N1 TT057, N2 TT090, N3 TT117, N4 QT040003/QT040008; API kiểm lại). Khoảng thời gian: N1 lượt mượn – trả 30 ngày, Top 10 và heatmap 12 tháng/365 ngày; N2 biểu đồ 12 tháng theo tháng lập biên bản, KPI “Tiền đã thu tháng này” = biên bản lập trong tháng đã thanh toán, “Chưa thanh toán” = mọi thời điểm; N3 sách hư hỏng/mất theo tháng của ngày trả, 12 tháng; N4 đăng nhập 30 ngày, thao tác 14 ngày chồng theo nhóm, “Lần sao lưu gần nhất” lấy từ nhật ký `04.00.09 Sao lưu…` (thủ công hoặc tự động – login svc_quantri không đọc msdb). KPI bấm được: N1 đang mượn/quá hạn và N2 chưa thanh toán mở danh sách trong hộp thoại; các KPI khác mở trang tương ứng (N3 mở Danh mục sách lọc sẵn tình trạng `?tinhTrang=`) | Phát sinh Pha 4 – duyệt 16/11/2026 |
| D53 | 16/11/2026 | **Giao diện báo cáo** (SPEC 6.4 chỉ mô tả “bộ lọc kỳ → nút Tổng hợp → nút Lập báo cáo mở trang in”): 01.04.02/02.05.01 là một trang (kỳ + chọn nhanh Hôm nay/7 ngày/Tháng này/Tháng trước, `btnTongHop`, thẻ số liệu, biểu đồ, bảng chi tiết theo thẻ); `btnLapBaoCao` mở 01.04.03/02.05.02 với cùng kỳ – trang riêng xem trước MB06/MB07 trong khung, ngày lập tự sinh (TT072), `btnIn` ghi nhật ký “01.04.03/02.05.02 Lập Báo cáo … kỳ …” rồi in. Chỉ ghi nhật ký khi lập/in báo cáo, không ghi khi xem tổng hợp. Trang in A4, 01 bản, nơi nhận Ban giám đốc thư viện, ký: Người lập báo cáo / Xác nhận của Ban giám đốc; biểu đồ in dựng SVG, tắt animation. Control đặt tên ngoài SPEC: Phụ lục A mục 6.4.5–6.4.6 | Phát sinh Pha 4 – duyệt 16/11/2026 |
| D54 | 16/11/2026 | **Ngoại lệ chống nhúng khung cho trang in.** Mọi trang giữ `X-Frame-Options: DENY`, CSP `frame-ancestors 'none'`; riêng `/quan-tri/in/…` (MB02–MB07) đổi thành `SAMEORIGIN` / `frame-ancestors 'self'` – chỉ chính trang web nhúng được. Lý do: btnIn của 6.2.1–6.2.4 in trong khung ẩn và 01.04.03/02.05.02 xem trước báo cáo trong khung; với DENY trình duyệt hiện khung trống (lỗi có từ Pha 3, kiểm thử trước đây chạy với BypassCSP nên không phát hiện). Thêm kiểm thử E2E chạy **CSP thật** (không bỏ qua): kiểm header hai loại trang, xem trước MB06 hiển thị, tổng quan + tooltip + trang in không vi phạm CSP. Tooltip biểu đồ dùng chấm màu theo lớp CSS thay `marker` có style nội tuyến của ECharts (CSP `style-src 'self'` chặn) | Phát sinh Pha 4 – duyệt 16/11/2026 |

## Phụ lục A – Control thêm ngoài SPEC (theo từng giao diện, để bổ sung hồ sơ Bài 4)

Đối chiếu tự động: id HTML của mỗi giao diện web ↔ tên control trong bảng “Control và xử lý” của SPEC mục 6 (cập nhật 16/11/2026, đến Pha 4).
Mọi control SPEC đã nêu tên đều có trên web với đúng id. Cột **Nhóm**:
- **Đặt tên** – SPEC có mô tả (“các ô nhập”, “hiển thị hoten, doituong”, trường ở “Bảng dữ liệu sử dụng”) nhưng không ghi tên control; web đặt tên theo quy ước tiền tố của SPEC.
- **Thêm mới** – chức năng/phần tử không có trong SPEC, thêm để dùng được trên web (D48 và các quyết định ghi kèm).

Loại: `TextBox` = ô nhập văn bản, `ComboBox` = hộp chọn, `DatePicker` = chọn ngày (D42), `DataGridView` = lưới, `Label` = nhãn hiển thị số liệu, `Form` = khung nhập, `Button`, `Tab`, `Image`.

### 6.1.1 Danh mục sách (`frmDanhMucSach`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `frmChiTiet` | Form | Khung chứa các ô nhập chi tiết bản sách (chỉ đọc khi xem, mở khi Thêm/Sửa) | Đặt tên |
| `txtMaBanSach` | TextBox | ma_bansach – C(10); khóa khi Sửa | Đặt tên |
| `txtTenSach` | TextBox | ten_sach – C(200) | Đặt tên |
| `txtTacGia` | TextBox | tacgia – C(100) | Đặt tên |
| `cboKhuVuc` | ComboBox | ma_khuvuc – chọn từ KHUVUC (hiện mã – vị trí) | Đặt tên |
| `cboTinhTrang` | ComboBox | tinhtrang – {Sẵn sàng, Đang được mượn, Mất, Hư hỏng}, mặc định “Sẵn sàng” | Đặt tên |

### 6.1.2 Danh mục khoản thu (`frmDanhMucKhoanThu`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `frmChiTiet` | Form | Khung chứa các ô nhập chi tiết khoản thu | Đặt tên |
| `txtMaKhoanThu` | TextBox | ma_khoanthu – C(10); khóa khi Sửa | Đặt tên |
| `txtNoiDungTT` | TextBox | noidung_tt – C(100) | Đặt tên |
| `txtDonGia` | TextBox | dongia – N(10) > 0, hiển thị phân nhóm hàng nghìn | Đặt tên |

### 6.1.3 Danh mục Bạn đọc (`frmDanhMucBanDoc`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `frmChiTiet` | Form | Khung chứa các ô nhập thông tin Thẻ Bạn đọc | Đặt tên |
| `txtMaBanDoc` | TextBox | ma_bandoc – C(10); gợi ý BD#### khi Thêm (D38); khóa khi Sửa | Đặt tên |
| `txtHoTen` | TextBox | hoten – C(50) | Đặt tên |
| `cboDoiTuong` | ComboBox | doituong – {Sinh viên, Học viên, Cán bộ} | Đặt tên |
| `txtMaSvHvCb` | TextBox | ma_svhvcb – C(15) | Đặt tên |
| `txtLopDonVi` | TextBox | lop_donvi – C(50) | Đặt tên |
| `dtpNgaySinh` | DatePicker | ngaysinh – dd/MM/yyyy, nhỏ hơn hôm nay | Đặt tên |
| `txtSoTheBD` | TextBox | so_thebd – C(15) | Đặt tên |
| `txtDienThoaiEmail` | TextBox | dienthoai_email – C(50), không bắt buộc | Đặt tên |
| `dtpHsdTuNgay` | DatePicker | hsd_tungay | Đặt tên |
| `dtpHsdDenNgay` | DatePicker | hsd_denngay > hsd_tungay | Đặt tên |
| `imgAnh` | Image | Xem ảnh thẻ (WebP đã thu nhỏ – D35) | Đặt tên |
| `txtAnh` | TextBox (ẩn) | anh – đường dẫn ảnh sau khi tải lên | Đặt tên |
| `dgvSachDangMuon` | DataGridView | Danh sách sách Bạn đọc đang mượn sau khi tìm theo ma_svhvcb (SPEC: “hiển thị thông tin Bạn đọc và danh sách sách đang mượn”) | Đặt tên |

### 6.1.4 Danh mục khu vực lưu trữ (`frmDanhMucKhuVuc`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `frmChiTiet` | Form | Khung chứa các ô nhập chi tiết khu vực | Đặt tên |
| `txtMaKhuVuc` | TextBox | ma_khuvuc – C(10); khóa khi Sửa | Đặt tên |
| `txtViTri` | TextBox | vitri – C(50) | Đặt tên |
| `cboLoaiKhuVuc` | ComboBox | loai_khuvuc – {Kệ mở, Kho} | Đặt tên |

### 6.2.1 Lập Phiếu mượn sách (`frmLapPhieuMuon`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `txtSoPhieu` | TextBox | Nhập so_pmuon để mở phiếu đã lưu bằng `btnTimPhieu` (SPEC: “nhập so_pmuon để mở phiếu đã lưu” – không ghi tên ô) | Thêm mới (D48) |
| `txtSoPMuon` | TextBox (chỉ đọc) | so_pmuon của phiếu đang lập/sửa (“Tự sinh khi lưu” với phiếu mới) | Đặt tên |
| `txtNgayLap` | TextBox (chỉ đọc) | ngaylap_pmuon | Đặt tên |
| `txtNguoiLap` | TextBox (chỉ đọc) | ma_nd, ten_nd lấy từ tài khoản đăng nhập | Đặt tên |
| `txtMaBanDoc` | TextBox (chỉ đọc) | ma_bandoc của Bạn đọc tìm được | Đặt tên |
| `txtHoTen` | TextBox (chỉ đọc) | hoten (SPEC: “hiển thị hoten, doituong”) | Đặt tên |
| `txtDoiTuong` | TextBox (chỉ đọc) | doituong | Đặt tên |
| `txtLopDonVi` | TextBox (chỉ đọc) | lop_donvi (in trên MB02) | Đặt tên |
| `lblTongSoBan` | Label | Tổng số bản sách mượn = số dòng dgvChiTiet (MB02) | Thêm mới |
| `lblHanTra` | Label | Hạn trả của các bản mượn hôm nay (= ngaymuon + số ngày theo đối tượng) | Thêm mới |

### 6.2.2 Lập Phiếu gia hạn sách (`frmLapPhieuGiaHan`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `txtSoPhieu` | TextBox | Nhập so_pgiahan để mở phiếu đã lưu bằng `btnTimPhieu` | Thêm mới (D48) |
| `txtSoPGiaHan` | TextBox (chỉ đọc) | so_pgiahan (tự sinh khi lưu) | Đặt tên |
| `txtNgayLap` | TextBox (chỉ đọc) | ngaylap_pgiahan | Đặt tên |
| `txtNguoiLap` | TextBox (chỉ đọc) | ma_nd, ten_nd | Đặt tên |
| `dtpNgayDeNghi` | DatePicker | ngay_denghi – ngày Bạn đọc đề nghị gia hạn | Đặt tên |
| `txtMaBanDoc`, `txtHoTen`, `txtMaSvHvCb` | TextBox (chỉ đọc) | Bạn đọc của Phiếu mượn sách | Đặt tên |
| `lblDongY` | Label | Số bản sách “Đồng ý” gia hạn | Thêm mới |
| `lblKhongDongY` | Label | Số bản sách “Không đồng ý” | Thêm mới |
| `lblSoNgayGiaHan` | Label | Số ngày gia hạn thêm theo quy định (7) | Thêm mới |

### 6.2.3 Lập Phiếu trả sách (`frmLapPhieuTra`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `txtSoPhieu` | TextBox | Nhập so_ptra để mở phiếu đã lưu bằng `btnTimPhieu` | Thêm mới (D48) |
| `txtSoPTra` | TextBox (chỉ đọc) | so_ptra (tự sinh khi lưu) | Đặt tên |
| `txtNgayLap` | TextBox (chỉ đọc) | ngaylap_ptra | Đặt tên |
| `txtNguoiLap` | TextBox (chỉ đọc) | ma_nd, ten_nd | Đặt tên |
| `dtpNgayTra` | DatePicker | ngaytra – ≥ ngày mượn, ≤ hôm nay (D46) | Đặt tên |
| `txtMaBanDoc`, `txtHoTen`, `txtMaSvHvCb` | TextBox (chỉ đọc) | Bạn đọc của Phiếu mượn sách | Đặt tên |
| `lblSoTra` | Label | Số bản sách trả lần này | Thêm mới |
| `lblViPham` | Label | Số trường hợp vi phạm (quá hạn, mất, hư hỏng) sẽ chuyển Bộ phận xử lý vi phạm | Thêm mới |

### 6.2.4 Lập Biên bản vi phạm (`frmLapBienBan`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `dgvTruongHop` | DataGridView | Trường hợp vi phạm chờ lập biên bản (TT075 – thông tin Bộ phận mượn trả chuyển sang); nhấp đúp để nạp so_pmuon | Thêm mới (D48) |
| `txtSoBienBan` | TextBox | Nhập so_bienban để mở biên bản đã lưu bằng `btnTimBienBan` (sửa, ghi nhận thanh toán) | Thêm mới (D48) |
| `dgvSachViPham` | DataGridView | Các bản sách vi phạm của phiếu mượn (SPEC: “nạp Bạn đọc và các bản sách vi phạm”) – quá hạn, mất, hư hỏng | Đặt tên |
| `cboBanSach` | ComboBox | ma_bansach của dòng CT_BIENBAN (chọn trong các bản sách vi phạm) | Thêm mới (D48) |
| `txtSoLuong` | TextBox (số) | soluong > 0 của dòng CT_BIENBAN (quá hạn: gợi ý = số ngày quá hạn) | Thêm mới (D48) |
| `lblDonGia` | Label | dongia của khoản thu đang chọn (SPEC cboKhoanThu “lấy noidung_tt, dongia”) | Đặt tên |
| `lblTongTien` | Label | tong_tien = SUM(thanh_tien) | Đặt tên |
| `txtThoiGianLap` | TextBox (chỉ đọc) | thoigian_lap (tự sinh khi lưu) | Đặt tên |
| `txtNguoiLap` | TextBox (chỉ đọc) | ma_nd, ten_nd | Đặt tên |
| `txtMaBanDoc`, `txtHoTen`, `txtLopDonVi` | TextBox (chỉ đọc) | Bạn đọc vi phạm | Đặt tên |

### 6.3.1 Đăng nhập (`frmDangNhap`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `btnHienMatKhau` | Button | Hiện/ẩn mật khẩu đang gõ (trợ năng, điện thoại) | Thêm mới |
| `btnXacNhanThoat` | Button | Nút “Thoát” trong hộp xác nhận của `btnThoat` (web không đóng được cửa sổ – về trang chủ) | Thêm mới |

### 6.3.2 Quản lý tài khoản người dùng (`frmTaiKhoan`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `frmChiTiet` | Form | Khung chứa các ô nhập tài khoản | Đặt tên |
| `txtMaND` | TextBox (chỉ đọc) | ma_nd – hệ thống tự sinh ND#### (D38) | Đặt tên |
| `txtMaNDGoiY` | Label | Dòng gợi ý “Hệ thống tự sinh.” dưới `txtMaND` | Thêm mới |
| `txtTenND` | TextBox | ten_nd | Đặt tên |
| `txtTenDangNhap` | TextBox | ten_dangnhap | Đặt tên |
| `txtChucVu` | TextBox | chucvu | Đặt tên |
| `txtSDT` | TextBox | sdt | Đặt tên |
| `cboTinhTrang` | ComboBox | tinhtrang tài khoản (Hoạt động/Khóa) | Đặt tên |

### 6.3.3 Cấp quyền cho tài khoản (`frmPhanQuyen`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `txtLocQuyen` | TextBox (tìm) | Lọc danh sách quyền theo mã hoặc tên (danh sách quyền dài, ~150 dòng) | Thêm mới |
| `cboLoaiQuyen` | ComboBox | Lọc danh sách quyền theo loại quyền (giá trị lấy từ dữ liệu QUYEN) | Thêm mới |

### 6.3.4 Theo dõi nhật ký hệ thống (`frmNhatKy`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `tabDangNhap` | Tab | Chuyển sang 04.00.03 Lịch sử đăng nhập | Thêm mới |
| `tabHoatDong` | Tab | Chuyển sang 04.00.08 Nhật ký hoạt động | Thêm mới |

### 6.3.5 Sao lưu và phục hồi dữ liệu (`frmSaoLuu`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `btnChonXong` | Button | Nút “Chọn” trong hộp duyệt thư mục/tệp .bak của `btnChon` (web không có hộp chọn thư mục của máy chủ SQL – D20) | Thêm mới |
### 6.4.5 Báo cáo tình hình mượn trả sách – 01.04.02 (`frmTongHopMuonTra`), 01.04.03 (`frmBaoCaoMuonTra`)
SPEC chỉ nêu “bộ lọc kỳ báo cáo (chọn ngày), nút Tổng hợp, nút Lập báo cáo mở trang in, bảng tổng hợp và biểu đồ” – mọi control dưới đây do web đặt tên (D53).

| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `dtpTuNgay`, `dtpDenNgay` | DatePicker | Kỳ báo cáo (TT058) – có trên cả 01.04.02 và 01.04.03 | Đặt tên |
| nút chọn nhanh `data-ky` (Hôm nay, 7 ngày, Tháng này, Tháng trước) | Button | Điền nhanh kỳ báo cáo | Thêm mới |
| `btnTongHop` | Button | “Nút Tổng hợp” – 01.04.02 (TT059–TT068) | Đặt tên |
| `lblTongLuotMuon`, `lblTongLuotGiaHan`, `lblTongLuotTra`, `lblSoSachDangMuon`, `lblSoSachQuaHan` | Label | Số liệu tổng hợp theo công thức SPEC 6.4.5 | Đặt tên |
| `bdTheoNgay` | Biểu đồ | Lượt mượn, gia hạn, trả theo ngày trong kỳ (file 09) | Đặt tên |
| `tabLuotMuon`, `tabLuotGiaHan`, `tabLuotTra`, `tabSachQuaHan` | Tab | Chuyển giữa các bảng chi tiết | Thêm mới |
| `dgvLuotMuon`, `dgvLuotGiaHan`, `dgvLuotTra`, `dgvSachQuaHan` | DataGridView | Liệt kê chi tiết lượt mượn, gia hạn, trả, sách quá hạn | Đặt tên |
| `btnLapBaoCao` | Button | “Nút Lập báo cáo” – mở 01.04.03 với cùng kỳ (bật sau khi tổng hợp) | Đặt tên |
| `btnXemBaoCao` | Button | 01.04.03: nhận số liệu tổng hợp của kỳ (TT070) và hiển thị chi tiết báo cáo (TT071) | Thêm mới |
| `txtNgayLap`, `txtNguoiLap` | TextBox (chỉ đọc) | Ngày lập báo cáo tự sinh (TT072), người lập | Đặt tên |
| `khungXemTruoc` | Khung xem trước | Trang in MB06 hiển thị trong form | Thêm mới |
| `btnIn` | Button | In Báo cáo MB06 (TT073) – ghi nhật ký rồi in | Đặt tên |
| `lnkMoBanIn` | Liên kết | Mở bản in ở thẻ mới để in lại | Thêm mới |
| `btnDong` | Button | Trở về menu chính | Đặt tên |

### 6.4.6 Báo cáo tình hình vi phạm mượn trả sách – 02.05.01 (`frmTongHopViPham`), 02.05.02 (`frmBaoCaoViPham`)
| Control | Loại | Mục đích | Nhóm |
|---|---|---|---|
| `dtpTuNgay`, `dtpDenNgay`, nút chọn nhanh `data-ky`, `btnTongHop`, `btnLapBaoCao`, `btnXemBaoCao`, `txtNgayLap`, `txtNguoiLap`, `khungXemTruoc`, `btnIn`, `lnkMoBanIn`, `btnDong` | – | Như 6.4.5 (02.05.01 TT090–TT094; 02.05.02 TT095–TT098) | Như 6.4.5 |
| `lblSoBienBan`, `lblSoTruongHop`, `lblTongTienViPham`, `lblTongTienDaThu`, `lblTongTienChuaThu` | Label | Số liệu tổng hợp theo công thức SPEC 6.4.6 | Đặt tên |
| `bdTheoKhoanThu` | Biểu đồ | Số trường hợp vi phạm theo khoản thu, chồng theo tình trạng thanh toán | Đặt tên |
| `bdTyLeKhoanThu` | Biểu đồ | Tỷ lệ trường hợp vi phạm theo khoản thu (donut) | Đặt tên |
| `tabTheoKhoanThu`, `tabBienBan` | Tab | Chuyển giữa hai bảng chi tiết | Thêm mới |
| `dgvTheoKhoanThu` | DataGridView | Tổng hợp theo nội dung vi phạm (khoản thu) | Đặt tên |
| `dgvBienBan` | DataGridView | Bạn đọc vi phạm và tình trạng thanh toán | Đặt tên |

### Trang tổng quan (không thuộc SPEC mục 6 – file 08 mục 4, file 09)
Thẻ KPI `kpi…` (N1 `kpiLuotMuonHomNay`, `kpiDangMuon`, `kpiQuaHan`; N2 `kpiBienBanThang`, `kpiTienThu`, `kpiChuaThanhToan`; N3 `kpiTongBanSach`, `kpiSanSang`, `kpiDangDuocMuon`, `kpiHuHongMat`; N4 `kpiTaiKhoan`, `kpiDangNhapHomNay`, `kpiThaoTacHomNay`, `kpiSaoLuu`), biểu đồ `bd…`, lưới chi tiết KPI `dgvChiTietKpi`, nút `btnLamMoi` – thêm mới theo file 08/09.