# BASECORE_PHAN_TICH – Khảo sát backend có sẵn (Pha 0A)

> Ngày khảo sát: 15/11/2026. Phạm vi: toàn bộ `Backend/` (= `basecore/` trong tài liệu) và `Frontend/`.
> Pha 0A chỉ đọc: **không sửa, không build, không xóa** file nào trong `Backend/`, `Frontend/`.
> Nhãn: **GIỮ** (dùng nguyên) · **TÁI DÙNG** (sửa thành service của ta) · **XÓA** (mẫu/không liên quan) · **CHƯA RÕ** (cần chủ dự án quyết).

## 1. Tổng quan

| Mục | Hiện trạng |
|---|---|
| Vị trí solution | `Backend/BaseCore/BaseCore/BaseCore.sln` (lồng 2 cấp `BaseCore/BaseCore`) |
| Nguồn gốc | Khung bài giảng “Bài 10, 11” (comment trong code), tái sử dụng từ dự án cũ “RobotVibot”/“PLM” (tên còn sót trong `.csproj.user`, `Constants.RootCache = "PLM_"`, enum Robot/Camera/Doctor/Room…) |
| Ngôn ngữ / .NET | C#; các project chính `net8.0`; 3 project cũ `netcoreapp2.2`/`netcoreapp3.1` (không còn hỗ trợ) |
| Máy hiện tại | .NET SDK **8.0.101 và 9.0.311** (chưa có SDK 10); Docker 29.7.2; Git 2.56; Node 20.20.2 |
| Lưu ý vòng đời | **.NET 8 hết hỗ trợ ngày 10/11/2026** (đã qua). File 01 đề xuất .NET 10 LTS → cần chủ dự án chọn (câu hỏi Q1) |
| Kiến trúc | Phân lớp kiểu N-tier dùng chung: `Entities` → `Repository` (EF Core, Generic Repository) → `Services` → các Web API. **Không** CQRS/MediatR, **không** Clean/Onion theo service, mọi service dùng **chung một DbContext** (`MySqlDbContext`, thực tế trỏ SQL Server) |
| Cách chạy | Chạy tay từng project (launchSettings): Gateway `:5000`, APIService `:5001`, AuthService `:5003`; WebClient React/Vite `:3000` proxy `/api` → `:5000` |
| Docker | **Không có** `docker-compose*`. Chỉ một `Dockerfile` mẫu VS ở project `Test` (netcoreapp3.1) – không dùng được |
| CI / README / script | Không có |
| Message bus / outbox | Không có |
| Cache | Có code Redis (`RedisUtils`) nhưng **không service nào đăng ký** Redis |
| Health check | Không có |
| Logging | `Console.WriteLine`; `LogService` (MongoDB) không biên dịch được (xem 3.4) |
| Migration | 4 migration rỗng (`Up/Down` trống) + snapshot các bảng mẫu; service gọi `EnsureCreated()` thay vì migrate |
| Test | `BaseCore.UnitTest` (NUnit, netcoreapp2.2) không biên dịch được, không có test thật |
| `Frontend/` | **Trống** → dựng mới ứng dụng ASP.NET Core MVC tại đây (không cần gắn nhãn) |

## 2. Danh sách service / project

| Project | Trong .sln | Loại | Mục đích hiện tại | Cổng | CSDL | Entity/bảng | API chính |
|---|---|---|---|---|---|---|---|
| `BaseCore.ApiGateway` | Có | Web (Ocelot 23.0.0) | Gateway định tuyến `/api/auth, users, roles` → :5003; `/api/products, categories, orders` → :5001 | 5000 | – | – | Proxy |
| `BaseCore.AuthService` | Có | Web API (+ rác MVC template + React CRA) | Đăng nhập JWT, đăng ký, CRUD user, role tĩnh | 5003 | SQL Server `BaseCoreSales` (chung) | `Users` | `POST /api/auth/login`, `/register`; `/api/users`; `/api/roles` |
| `BaseCore.APIService` | Có | Web API | Bán hàng mẫu: Product/Category/Order | 5001 | SQL Server `BaseCoreSales` (chung) | `Products, Categories, Orders, OrderDetails` | `/api/products`, `/api/categories`, `/api/orders` |
| `BaseCore.Common` | Có | Thư viện | JWT (`TokenHelper`), băm PBKDF2-SHA1, Redis utils, WebSocket, hằng số/enum rác | – | – | `Entity` base | – |
| `BaseCore.DTO` | Có | Thư viện | Paging, SearchResult, SortColumn, response wrapper, DTO Auth Platform (Module/Function/Role) | – | – | – | – |
| `BaseCore.Entities` | Có | Thư viện | Entity mẫu + Auth Platform (Role/Module/Function…) | – | – | 15 entity | – |
| `BaseCore.Libs` | Có | Thư viện | `EnumHelper`, `LinqExtension.OrderByField` (sắp xếp động), `Utils` (ngày giờ) | – | – | – | – |
| `BaseCore.Repository` | Có | Thư viện | `MySqlDbContext`, `Repository<T>` generic, repo mẫu, Migrations | – | SQL Server (gói Pomelo MySQL, MongoDB thừa) | – | – |
| `BaseCore.Services` | Có | Thư viện | `UserService` (xác thực), Product/Category/Order service | – | – | – | – |
| `BaseCore.LogService` | **Không** | Thư viện | Ghi log lỗi/hành động vào MongoDB + `ExceptionMiddleware` | – | MongoDB | `LogAction, LogError` | – |
| `BaseCore.AuditLog` | **Không** | MVC netcoreapp2.2 | Xem/ghi log lỗi Mongo | – | MongoDB (IP công khai) | – | `/api/auditLog` |
| `BaseCore.UnitTest` | **Không** | NUnit netcoreapp2.2 | Test mẫu (không chạy được) | – | – | – | – |
| `BaseCore.IOTService` | **Không** | (chỉ còn `obj/`) | Không có mã nguồn | – | – | – | – |
| `BaseCore.WebClient` | **Không** | React 18 + Vite 5 + Bootstrap 4.6 | Giao diện admin bán hàng mẫu (có `node_modules/` ~ commit vào thư mục) | 3000 | – | – | Gọi gateway |
| `Test` | **Không** | Worker netcoreapp3.1 | Template Worker in log mỗi giây + Dockerfile VS | – | – | – | – |
| `Examples/` | – | HTML/CSS/JS | Bài tập 4–7 môn web | – | – | – | – |

Ghi chú `.sln`: có 5 solution folder (Services, DataAccess, Common, WebUI, Microservices) nhưng mục `NestedProjects` và `ProjectConfigurationPlatforms` **trống** → Visual Studio sẽ không có cấu hình build cho project; cần dựng lại `.sln`.

## 3. Hạ tầng dùng chung

### 3.1 API Gateway – Ocelot 23.0.0 (`BaseCore.ApiGateway`)
- `ocelot.json` định tuyến tĩnh tới `localhost:500x` (không chạy được trong Docker, phải đổi sang tên service).
- Không xác thực ở gateway, không rate limit, không QoS; CORS `AllowAll`; Swagger chỉ của gateway.
- Đánh giá: **TÁI DÙNG** – giữ Ocelot (đúng quy ước basecore), viết lại toàn bộ route theo service mới, thêm xác thực JWT ở gateway, rate limit (Ocelot có sẵn), health.

### 3.2 Service discovery, message bus, outbox/inbox, cấu hình tập trung, tracing
- **Không có.** Không RabbitMQ/MassTransit/Kafka, không outbox, không Consul. → Phải bổ sung nếu chọn hướng sự kiện của file 12 mục 5 (câu hỏi Q3, Q4 ở kế hoạch Pha 0).

### 3.3 Cache
- `BaseCore.Common/RedisUtils.cs` (extension cho `IDistributedCache`) nhưng không có `AddStackExchangeRedisCache` ở bất kỳ service nào. `DeleteAllCacheAsyn` là `async void` (lỗi tiềm ẩn).
- Đánh giá: **CHƯA RÕ** – file 11 chỉ cần cache ngắn 60s → `IMemoryCache` là đủ; Redis chỉ cần nếu chạy nhiều bản sao.

### 3.4 Logging, xử lý ngoại lệ
- `BaseCore.LogService` tham chiếu `BaseCore.Libs.Repository.MongoRepository`, `IDbContext` – **không tồn tại** trong `BaseCore.Libs` (chỉ có `Utils/`) → **không biên dịch được**. Dùng MongoDB.
- `ExceptionMiddleware` (trả JSON 401/500, ẩn chi tiết khi Production) – ý tưởng tốt, có thể chép sang thư viện chung, sửa trả lỗi theo định dạng file 01 mục 5 (`{ ma, thongBao, truong }`).
- Không có Serilog (file 01 yêu cầu Serilog ghi file xoay vòng).

### 3.5 Health check
- Không có endpoint `/health` ở service nào → phải thêm (file 14 bắt buộc: sống + CSDL + bus).

## 4. Xác thực, phân quyền

| Mục | Hiện trạng | Vấn đề so với yêu cầu |
|---|---|---|
| Cơ chế | JWT Bearer HS256 tự ký (`TokenHelper.GenerateToken`), hạn 480 phút, không refresh token | Frontend MVC cần cookie (file 11: hai cookie scheme `NhanVien`/`BanDoc`) |
| Khóa ký | Chuỗi cứng `"YourSecretKeyForAuthenticationShouldBeLongEnough"` lặp ở `AuthController` (hằng số) và `appsettings` 2 service | Bí mật nằm trong repo, mã nguồn |
| Lưu người dùng | Bảng `Users` (Id string GUID, Password + Salt byte[]) | SPEC dùng `NGUOIDUNG` (ma_nd, tendangnhap, matkhau C(64), ma_nhom…) |
| Băm mật khẩu | PBKDF2-**HMACSHA1**, 10.000 vòng, salt riêng cột | File 01 yêu cầu `PasswordHasher<T>`; SHA1/10k vòng là yếu |
| Phân quyền | `[Authorize(Roles="Admin")]`, role suy từ `UserType` (1=Admin, khác=User); `RolesController` trả danh sách role **tĩnh** trong code | SPEC cần quyền theo nhóm N1–N4 đọc từ `PHANQUYEN` (mã `TT001…TT122`, `SACH_C…`) |
| Khóa tài khoản | Không có đếm sai/khóa | SPEC 6.3.1: sai 5 lần → `tinhtrang = 'Khóa'` |
| Nhật ký đăng nhập | Không có | SPEC: `NHATKY_DANGNHAP` |
| Bạn đọc | Không có loại tài khoản thứ hai | File 07: `TAIKHOAN_BANDOC`, cookie riêng |
| Lỗi bảo mật | `AuthController.Login` **in mật khẩu ra console** (`Console.WriteLine($"Password: …")`); `/api/auth/register` cho phép tự đăng ký công khai; CORS AllowAll | Phải bỏ |

Đánh giá: **TÁI DÙNG** khung JWT (gateway ↔ service) và vị trí AuthService; **viết lại** toàn bộ mô hình người dùng, băm mật khẩu, quyền theo SPEC.

## 5. Thư viện dùng chung

| Thành phần | Có trong basecore | Đánh giá |
|---|---|---|
| Base entity | `BaseCore.Common.Entity` (Id GUID string, CreatedDateTime UTC), `IAuditable`, `IVersionable` | Không hợp: 18 bảng SPEC có khóa/trường riêng, không thêm cột → **XÓA** |
| Base repository | `IRepository<T>` / `Repository<T>` (EF Core, phân trang `GetPagedAsync`) | Dùng được; mỗi lệnh tự `SaveChangesAsync` → không hợp transaction nhiều bảng. **TÁI DÙNG** (thêm Unit of Work / tách SaveChanges) |
| Pipeline behavior | Không có | – |
| Exception middleware | `LogService/Middleware/ExceptionMiddleware.cs` | **TÁI DÙNG** ý tưởng, chuyển sang thư viện chung |
| Response wrapper | `JsonRestApiResponse`, `ResponseMessage`, `GridResponse`, `Paging`, `SearchResult`, `SortColumn` | Định dạng lỗi khác file 01 → **TÁI DÙNG** `Paging/SearchResult/SortColumn/GridResponse`; thay wrapper lỗi theo file 01 |
| Sắp xếp động | `Libs/Utils/LinqExtension.OrderByField` | **GIỮ** (dùng cho DataGrid sắp xếp server) |
| WebSocket | `Common/Sockets/*` – không service nào dùng | **CHƯA RÕ** (đề xuất XÓA; thông báo dùng tải định kỳ, chưa cần realtime) |
| AutoMapper 13.0.1 | Khai báo ở hầu hết project, gần như không dùng | **CHƯA RÕ** – bản 13.0.1 đang dùng là MIT, nhưng các bản mới (từ 15) chuyển sang giấy phép thương mại, nên khó nâng cấp. Đề xuất bỏ, ánh xạ tay |

## 6. Quy ước code

- Kiến trúc: N-tier dùng chung (không Clean/Onion/Vertical slice); Controller gọi Service gọi Repository; DI đăng ký tay trong `Program.cs` (top-level statements); AuditLog còn kiểu `Startup.cs` cũ.
- Đặt tên: PascalCase tiếng Anh, `I…Service`, `…RepositoryEF`; route `api/[controller]`.
- `Nullable` bật nhưng mã không tuân thủ (nhiều cảnh báo); comment lẫn tiếng Việt lỗi mã hóa (file `Repository/Authen/UserRepository.cs` không phải UTF-8).
- Migration: có thư mục `Migrations` nhưng các migration **rỗng**; khởi động gọi `EnsureCreated()` (không dùng được cùng migration).
- Test: không có quy ước dùng được.
- → Basecore **không có quy ước mạnh** buộc phải theo, ngoài: .NET (8), Ocelot làm gateway, JWT giữa service, EF Core + SQL Server, Generic Repository, Swagger. Kế hoạch Pha 0 sẽ giữ các điểm này và bổ sung phần thiếu theo file 01/12/14.

## 7. Đánh giá từng thư mục / project

| # | Đường dẫn (dưới `Backend/BaseCore/BaseCore/`) | Nhãn | Lý do / việc sẽ làm |
|---|---|---|---|
| 1 | `BaseCore.sln` | TÁI DÙNG | Dựng lại danh sách project, cấu hình build (đang trống); có thể đổi tên (Q-đổi tên) |
| 2 | `BaseCore.ApiGateway/` | TÁI DÙNG | Giữ Ocelot; viết lại `ocelot.json` theo service mới + tên host Docker; thêm JWT, rate limit, `/health`, Dockerfile; bỏ banner Console |
| 3 | `BaseCore.AuthService/` (Controllers, Program.cs, csproj) | TÁI DÙNG | Thành service **identity**: NGUOIDUNG, NHOMNGUOIDUNG, QUYEN, PHANQUYEN, NHATKY_DANGNHAP, TAIKHOAN_BANDOC; viết lại Auth/User controller; **xóa** `RolesController` tĩnh, endpoint `register` |
| 3a | `BaseCore.AuthService/ClientApp/` | XÓA | React CRA “robotvibot-admin” mẫu bán hàng; frontend là MVC ở `Frontend/` |
| 3b | `BaseCore.AuthService/Views/`, `wwwroot/`, `Models/ErrorViewModel.cs` | XÓA | Rác template MVC trong một Web API |
| 3c | `BaseCore.AuthService/*.csproj.user` (2 file, có `RobotVibot…`) | XÓA | File IDE cá nhân |
| 4 | `BaseCore.APIService/` | TÁI DÙNG (khung) | Dùng `Program.cs` (JWT + EF + Swagger) làm khuôn cho service nghiệp vụ; đổi tên thành một service (đề xuất `kho-sach`) |
| 4a | `BaseCore.APIService/Controllers/{Products,Categories,Orders}Controller.cs` | XÓA | Nghiệp vụ bán hàng mẫu |
| 5 | `BaseCore.Common/Auth/TokenHelper.cs` | TÁI DÙNG | Giữ sinh/kiểm JWT; bỏ `HashPassword/IsValidPassword` (SHA1) thay bằng hàm băm mới |
| 5a | `BaseCore.Common/Auth/RoleConstant.cs`, `Constants.cs`, `Enums.cs`, `Entity.cs`, `AppSettings.cs` | XÓA | Hằng số/enum của dự án Robot/khách sạn/PLM; thay bằng hằng số thư viện (mã quyền, miền giá trị SPEC) |
| 5b | `BaseCore.Common/RedisUtils.cs` | CHƯA RÕ | Giữ nếu chọn Redis (Q-Redis); mặc định XÓA |
| 5c | `BaseCore.Common/Sockets/` | CHƯA RÕ | Không ai dùng; đề xuất XÓA |
| 5d | `BaseCore.Common/Helpers/FileHelper.cs`, `Extensions/MediaExtension.cs` | CHƯA RÕ | Có thể dùng khi tải ảnh bìa (nhận dạng loại tệp), nhưng cần kiểm tra loại tệp thật (file 11) → đề xuất XÓA, viết mới |
| 6 | `BaseCore.DTO/Common/*`, `Response/ApiResponse.cs` | GIỮ | Paging, SearchResult, SortColumn, GridResponse |
| 6a | `BaseCore.DTO/Response/JsonRestApiResponse.cs`, `ConstantResponseMessage.cs` | TÁI DÙNG | Sửa thành định dạng lỗi `{ ma, thongBao, truong }` (file 01) |
| 6b | `BaseCore.DTO/AuthPlatform/` (13 DTO) | XÓA | Mô hình Module/Function/Role không dùng (SPEC dùng NHOM/QUYEN/PHANQUYEN) |
| 7 | `BaseCore.Entities/` (project) | TÁI DÙNG | Chứa entity 18 bảng + mở rộng (hoặc tách theo service – tùy phương án CSDL) |
| 7a | `BaseCore.Entities/*.cs` mẫu (AccessToken, Category, Function, GroupRoleAgency, Module, ModuleFunction, Order, OrderDetail, Product, Role, RoleModuleFunction, SeedConfiguration, Setting, User, UserModule, UserRole) + `Audit/` | XÓA | Không thuộc nghiệp vụ thư viện |
| 7b | `BaseCore.Entities/.idea/` | XÓA | File IDE Rider |
| 8 | `BaseCore.Libs/` | GIỮ | `LinqExtension`, `EnumHelper`, `Utils` nhỏ, dùng được; bỏ gói MongoDB/JwtBearer/Swashbuckle thừa trong csproj |
| 9 | `BaseCore.Repository/EFCore/IRepository.cs`, `Repository.cs` | TÁI DÙNG | Generic repository + thêm Unit of Work cho transaction |
| 9a | `BaseCore.Repository/MySqlDbContext.cs` | TÁI DÙNG | Thay bằng DbContext thư viện (Fluent API đúng tên bảng/cột SPEC); bỏ seed bán hàng |
| 9b | `BaseCore.Repository/EFCore/{Category,Order,Product,User}Repository.cs`, `Authen/UserRepository.cs` | XÓA | Repo mẫu (file Authen không phải UTF-8) |
| 9c | `BaseCore.Repository/Migrations/` (4 migration rỗng + snapshot) | XÓA | Tạo migration mới từ đầu |
| 9d | Gói `Pomelo.EntityFrameworkCore.MySql`, `MongoDB.Driver` | XÓA | Không dùng MySQL/Mongo |
| 10 | `BaseCore.Services/Authen/UserService.cs` | TÁI DÙNG | Viết lại cho NGUOIDUNG (khóa 5 lần, nhật ký) |
| 10a | `BaseCore.Services/{Category,Order,Product}Service.cs` + interface | XÓA | Mẫu bán hàng |
| 10b | Gói `Quartz 3.8.0` | XÓA | File 01 dùng `BackgroundService` có sẵn |
| 11 | `BaseCore.LogService/` | XÓA (cần duyệt – hạ tầng logging) | Không biên dịch được, phụ thuộc Mongo; chép `ExceptionMiddleware` sang thư viện chung trước khi xóa; thay bằng Serilog + service nhật ký trên SQL Server |
| 12 | `BaseCore.AuditLog/` | XÓA (cần duyệt – hạ tầng logging) | netcoreapp2.2, Mongo IP công khai, không trong .sln, không biên dịch; thay bằng service `nhat-ky` (NHATKY_HOATDONG) |
| 13 | `BaseCore.UnitTest/` | XÓA | NUnit netcoreapp2.2 hỏng; thay bằng xUnit (`tests/` mới) |
| 14 | `BaseCore.IOTService/` | XÓA | Chỉ có `obj/`, không mã nguồn |
| 15 | `BaseCore.WebClient/` (kể cả `node_modules/`) | XÓA | React/Vite mẫu bán hàng; tài liệu bắt buộc MVC (file 01: không chuyển sang React) |
| 16 | `Test/` | XÓA | Template Worker netcoreapp3.1 + Dockerfile VS |
| 17 | `Examples/` (Bai4–Bai7) | XÓA | Bài tập HTML/CSS/JS môn học, không liên quan |
| 18 | `UpgradeLog.htm`, `BaseCore.slnLaunch.user`, `.vs/` | XÓA | Sản phẩm phụ của Visual Studio |
| 19 | Mọi `bin/`, `obj/` | XÓA (không commit) | Đưa vào `.gitignore` |
| 20 | Cấu trúc lồng `Backend/BaseCore/BaseCore/` | CHƯA RÕ | Đề xuất giữ nguyên vị trí (ít xáo trộn) hoặc nâng lên `Backend/` cho gọn (câu hỏi Pha 0) |

## 8. Rủi ro

| # | Rủi ro | Mức | Xử lý đề xuất |
|---|---|---|---|
| R1 | **Bí mật / hạ tầng thật nằm trong repo**: `BaseCore.AuditLog/appsettings.json` và `BaseCore.UnitTest/appsettings.json` chứa MongoDB `mongodb://178.128.26.135:27017` và Redis `178.128.26.135:6379` (IP công khai, không mật khẩu) + `AppSettings:Secret` GUID; khóa JWT cứng ở `AuthService`/`APIService` appsettings và hằng số `AuthController.SecretKey`; chuỗi kết nối tên máy `NAMKHÁNH` | Cao | **Không đưa các giá trị này vào commit đầu tiên** lên GitHub: thay bằng placeholder/biến môi trường trước khi commit (xem câu hỏi). Khuyến nghị chủ dự án kiểm tra máy chủ 178.128.26.135 có còn mở không |
| R2 | `AuthController.Login` ghi mật khẩu ra console | Cao | Xóa khi viết lại |
| R3 | .NET 8 hết hỗ trợ (10/11/2026); 3 project netcoreapp2.2/3.1 | Cao | Nâng lên .NET 10 LTS (cần cài SDK 10 trên máy để gỡ lỗi; build Docker không cần) |
| R4 | Không có Docker, health, bus, outbox, migration thật, test | Trung bình | Bổ sung toàn bộ ở Pha 1 theo file 12, 14 |
| R5 | Mọi service dùng chung `MySqlDbContext` một CSDL – mâu thuẫn ranh giới microservice | Trung bình | Chọn phương án CSDL ở Pha 0 |
| R6 | `.sln` mất cấu hình build; project ngoài `.sln` không biên dịch | Thấp | Dựng lại `.sln` |
| R7 | Băm mật khẩu PBKDF2-SHA1 10k vòng | Trung bình | Thay bằng băm mới (xem R8) |
| R8 | **Mâu thuẫn SPEC ↔ công nghệ**: `NGUOIDUNG.matkhau` là C(64) nhưng chuỗi `PasswordHasher<T>` (v3) dài ~84 ký tự | Trung bình | Hỏi chủ dự án ở Pha 0 |
| R9 | **Mâu thuẫn trong SPEC**: `so_pmuon`, `so_pgiahan`, `so_ptra`, `so_bienban` là C(10) nhưng định dạng số `PMyyyyMMdd-###` dài 14 ký tự | Trung bình | Hỏi chủ dự án ở Pha 0 |
| R10 | Gói cũ: Ocelot 23.0.0 (net8), Swashbuckle 6.5.0, `System.IdentityModel.Tokens.Jwt` 7.3.1, Newtonsoft 13.0.3; `Microsoft.Extensions.Caching.Redis` 2.2 (đã ngừng) | Thấp–TB | Nâng cùng lúc nâng .NET; quét `dotnet list package --vulnerable` ở Pha 1 |
| R11 | `node_modules/` và `wwwroot/lib` (jQuery, Bootstrap 4) nằm trong cây mã nguồn | Thấp | Xóa cùng project chứa nó |
| R12 | File không phải UTF-8 (`Repository/Authen/UserRepository.cs`) | Thấp | Bị xóa theo danh sách |
| R13 | Chưa build thử ở Pha 0A (chỉ đọc) → trạng thái build hiện tại chưa kiểm chứng | Thấp | Build ngay đầu Pha 1, trước khi xóa, để có mốc so sánh |

## 9. Tóm tắt danh sách XÓA (chờ duyệt)

Trong `Backend/BaseCore/BaseCore/`:
1. `BaseCore.AuditLog/` *(hạ tầng logging – cần duyệt riêng)*
2. `BaseCore.LogService/` *(hạ tầng logging – cần duyệt riêng; chép `ExceptionMiddleware` trước)*
3. `BaseCore.UnitTest/`
4. `BaseCore.IOTService/`
5. `BaseCore.WebClient/`
6. `Test/`
7. `Examples/`
8. `UpgradeLog.htm`, `BaseCore.slnLaunch.user`, `.vs/`, mọi `bin/`, `obj/`
9. `BaseCore.AuthService/ClientApp/`, `Views/`, `wwwroot/`, `Models/`, `*.csproj.user`, `Controllers/RolesController.cs`
10. `BaseCore.APIService/Controllers/{Products,Categories,Orders}Controller.cs`
11. `BaseCore.Entities/` toàn bộ entity mẫu + `Audit/` + `.idea/`
12. `BaseCore.DTO/AuthPlatform/`
13. `BaseCore.Repository/Migrations/`, `EFCore/{Category,Order,Product,User}Repository.cs`, `Authen/UserRepository.cs`
14. `BaseCore.Services/{Category,Order,Product}Service.cs`, `I{Category,Order,Product}Service.cs`
15. `BaseCore.Common/Constants.cs`, `Enums.cs`, `Entity.cs`, `AppSettings.cs`, `Auth/RoleConstant.cs`
16. *(CHƯA RÕ, đề xuất xóa)* `BaseCore.Common/Sockets/`, `RedisUtils.cs`, `Helpers/FileHelper.cs`, `Extensions/MediaExtension.cs`
17. Gói NuGet thừa: Pomelo MySQL, MongoDB.*, Quartz, `Microsoft.Extensions.Caching.Redis`, (đề xuất) AutoMapper

Mọi tham chiếu tới phần đã xóa (`.sln`, `ocelot.json`, `Program.cs`, csproj) sẽ được gỡ trong cùng commit xóa; sau commit: build toàn solution, chạy test, `docker compose up` phải xanh (file 12 mục 3).
