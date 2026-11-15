# 12 – LÀM VIỆC TRÊN BASECORE (KIẾN TRÚC MICROSERVICE CÓ SẴN)

Chủ dự án sẽ đặt thư mục **`basecore/`** (mã nguồn backend dạng microservice có sẵn) vào gốc dự án. Hệ thống được xây **trực tiếp trên basecore**: đọc kỹ, xóa phần không liên quan, chỉnh sửa và bổ sung ngay trong basecore. **Không** dựng một backend mới song song.

## 1. Thứ tự ưu tiên khi có basecore
1. `SPEC_NGHIEP_VU_BT4.md` (nghiệp vụ, CSDL, quyền, giao diện) – vẫn cao nhất.
2. **Quy ước của basecore** (ngôn ngữ, phiên bản .NET, cách chia service, CQRS/MediatR, repository, cách giao tiếp, xác thực, logging, cấu trúc thư mục, cách viết test) – **thay thế** mục 3 (phần backend) và mục 4 của file 01 khi khác nhau.
3. Các file 02–11 về chức năng, giao diện, chuyển động – giữ nguyên ý, điều chỉnh vị trí code cho hợp basecore.

Nếu basecore dùng frontend khác (Angular, React…) hoặc không có frontend: **dừng và hỏi** chủ dự án trước khi chọn (mặc định đề xuất: thêm một ứng dụng web ASP.NET Core MVC làm frontend, gọi backend qua API Gateway).

## 2. Pha 0A – Khảo sát basecore (bắt buộc, chỉ đọc, không sửa file nào)
Đọc toàn bộ `basecore/` (solution, mọi project, `docker-compose*`, `appsettings*`, Dockerfile, script, CI, README, migration, test). Lập `docs/BASECORE_PHAN_TICH.md` gồm:

| Mục | Nội dung cần ghi |
|---|---|
| Tổng quan | Phiên bản .NET/ngôn ngữ, cách build/chạy, cách chạy local (docker compose?), yêu cầu hạ tầng |
| Danh sách service | Tên, mục đích hiện tại, project, cổng, CSDL riêng hay chung, bảng/entity hiện có, API chính |
| Hạ tầng dùng chung | API Gateway (Ocelot/YARP/…), service discovery, message bus (RabbitMQ/MassTransit/Kafka/…), outbox/inbox, cache (Redis?), cấu hình tập trung, health check, logging/tracing |
| Xác thực, phân quyền | Cơ chế (JWT/cookie/IdentityServer/Keycloak/OpenIddict…), nơi lưu người dùng, cách kiểm tra quyền |
| Thư viện dùng chung | BuildingBlocks/Shared kernel: base entity, base repository, pipeline behavior, exception middleware, response wrapper… |
| Quy ước code | Kiến trúc từng service (Clean/Onion/Vertical slice), đặt tên, cách viết migration, cách viết test |
| Đánh giá theo nhu cầu dự án | Mỗi thư mục/project gắn nhãn: **GIỮ** (dùng nguyên), **TÁI DÙNG** (sửa thành service của ta), **XÓA** (mẫu/không liên quan, ví dụ service Product/Order/Basket mẫu), **CHƯA RÕ** – kèm lý do |
| Rủi ro | Phụ thuộc phiên bản cũ, gói có lỗ hổng, cấu hình bí mật nằm trong repo, thiếu test… |

Trình file này cùng kế hoạch Pha 0 và **chờ duyệt danh sách XÓA** trước khi xóa bất cứ thứ gì.

## 3. Quy tắc xóa và sửa
- Làm trên git: tạo nhánh `feature/thu-vien`; commit trạng thái gốc trước; **xóa trong một commit riêng** có danh sách đã duyệt; sau khi xóa: build toàn solution, chạy test còn lại, chạy `docker compose up` (nếu có) phải xanh.
- Xóa sạch tham chiếu: project khỏi `.sln`, route khỏi gateway, service khỏi docker-compose, cấu hình, migration, test, tài liệu.
- Không xóa hạ tầng dùng chung (gateway, bus, BuildingBlocks, health check, logging) dù chưa dùng hết, trừ khi được duyệt.
- Sửa theo đúng phong cách sẵn có của basecore; không trộn hai kiểu kiến trúc trong cùng một service.
- Bí mật (chuỗi kết nối, khóa) chuyển sang biến môi trường/user-secrets; không commit bí mật.

## 4. Đề xuất ranh giới service (Claude Code điều chỉnh theo basecore và trình duyệt)

| Service | Bảng sở hữu (18 bảng SPEC + mở rộng file 02) | Nghiệp vụ chính |
|---|---|---|
| **identity** (quản trị) | NHOMNGUOIDUNG, NGUOIDUNG, QUYEN, PHANQUYEN, NHATKY_DANGNHAP, TAIKHOAN_BANDOC | Đăng nhập nhân viên và bạn đọc, cấp quyền 04.00.01–04.00.02, nhật ký đăng nhập |
| **kho-sach** (catalog) | SACH, KHUVUC, DAUSACH, DAUSACH_BANSACH, THELOAI | Danh mục sách, khu vực, các tiến trình 03.xx, dữ liệu kệ sách/tìm kiếm cho cổng |
| **muon-tra** (circulation) | BANDOC, PHIEUMUON, CT_PHIEUMUON, PHIEUGIAHAN, CT_PHIEUGIAHAN, PHIEUTRA, CT_PHIEUTRA, PHIEUDATCHO, SOCHUNGTU, YEUTHICH | 01.01.01, 01.01.03, 01.02.01, 01.02.02, đặt chỗ, báo cáo MB06 |
| **vi-pham** | KHOANTHU, BIENBAN, CT_BIENBAN | 02.03.01, báo cáo MB07 |
| **thong-bao** | THONGBAO | Thông báo cho bạn đọc, nhận sự kiện từ các service |
| **nhat-ky** (có thể gộp vào identity) | NHATKY_HOATDONG | Nhận sự kiện “đã thao tác” từ mọi service; 04.00.08 |
| **sao-luu** (có thể là job trong identity) | – | 04.00.09 |
| **gateway** | – | Định tuyến `/api/...`, xác thực token/cookie, rate limit |
| **web** (frontend) | – | Cổng bạn đọc + dashboard nghiệp vụ (MVC), gọi gateway |

Nguyên tắc dữ liệu:
- Mỗi bảng có **đúng một service sở hữu**, chỉ service đó được ghi. Tên bảng/cột vẫn đúng SPEC.
- Nếu basecore dùng **CSDL riêng cho từng service**: khóa ngoại giữa hai service trở thành tham chiếu logic (ghi rõ trong `docs/GIA_DINH.md`), kiểm tra tồn tại bằng gọi đồng bộ hoặc bản sao đọc (read model) cập nhật qua sự kiện. Test `SchemaMatchesSpecTests` kiểm tra **hợp của mọi CSDL** khớp SPEC mục 2.
- Nếu basecore dùng **một CSDL chung**: giữ đủ khóa ngoại như SPEC, mỗi service chỉ ánh xạ các bảng mình sở hữu (bảng của service khác chỉ đọc qua API hoặc view).
- **Chọn phương án nào phải hỏi chủ dự án ở Pha 0**, trình bày ưu nhược điểm.

## 5. Giao dịch xuyên service (theo đúng thiết kế BT4)
Thiết kế BT4 vốn tách việc theo bộ phận nên hợp với sự kiện bất đồng bộ:

| Sự kiện (phát bởi) | Bên nhận | Tác dụng |
|---|---|---|
| `PhieuMuonDaLap` (muon-tra) | kho-sach | Đưa bản sách vào danh sách “sách chuyển đến” để N3 thực hiện 03.01.04 (cập nhật “Đang được mượn”) đúng SPEC 6.1.1 |
| `PhieuTraDaLap` (muon-tra) | kho-sach | 03.02.03 cập nhật sách sau trả; nếu mất/hư hỏng → chuyển sang vi-pham |
| `TruongHopViPham` (muon-tra) | vi-pham | Danh sách trường hợp chờ lập biên bản |
| `BienBanDaLap` (vi-pham) | muon-tra, kho-sach, thong-bao | Kết quả xử lý; 03.03.02 cập nhật sách mất/hư hỏng |
| `DatChoDaTao/DaNhan/DaHuy/HetHan` (muon-tra) | thong-bao | Thông báo bạn đọc |
| `DaThaoTac` (mọi service) | nhat-ky | Ghi NHATKY_HOATDONG |
| `TinhTrangSachDaDoi` (kho-sach) | muon-tra | Cập nhật bản sao đọc tình trạng sách dùng cho kiểm tra mượn và đặt chỗ |

- Ghi dữ liệu + phát sự kiện trong **cùng transaction nội bộ service** bằng outbox (dùng cơ chế outbox của basecore nếu có).
- Bên nhận xử lý **idempotent** (lưu mã sự kiện đã xử lý).
- Kiểm tra điều kiện mượn/đặt chỗ cần dữ liệu nhiều service (thẻ, quá hạn, biên bản chưa thanh toán): muon-tra giữ bản sao đọc tối thiểu (ví dụ “bạn đọc có biên bản chưa thanh toán”) cập nhật qua sự kiện; không gọi chuỗi đồng bộ dài trong lúc lập phiếu.
- Tính duy nhất của đặt chỗ (file 02 X05) đảm bảo bằng chỉ mục duy nhất trong CSDL của muon-tra.

## 6. Nghiệm thu riêng cho basecore
- [ ] `docs/BASECORE_PHAN_TICH.md` đầy đủ, danh sách XÓA được duyệt trước khi xóa.
- [ ] Sau khi xóa: build, test, docker compose chạy được; không còn tham chiếu tới phần đã xóa (tìm kiếm toàn repo).
- [ ] Mỗi service chạy độc lập, có health check; gateway định tuyến đủ API của file 06–09.
- [ ] Test hợp đồng (contract) giữa web ↔ gateway ↔ service; test tích hợp cho từng luồng sự kiện ở mục 5 (kể cả nhận trùng sự kiện).
- [ ] README mô tả cách chạy toàn bộ hệ thống local bằng `docker compose up -d --build` (file 14) và cách chạy riêng từng service khi gỡ lỗi.
