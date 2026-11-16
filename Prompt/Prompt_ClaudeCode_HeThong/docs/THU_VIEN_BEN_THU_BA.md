# THƯ VIỆN BÊN THỨ BA (file 13)

Chỉ dùng thư viện đã duyệt (file 01, 13, GIA_DINH D17). Phiên bản đều phát hành ≥ 14 ngày tính đến 15/11/2026.

## Backend / kiểm thử (NuGet – ghim trong `Directory.Packages.props`)

| Gói | Phiên bản | Giấy phép | Dùng cho |
|---|---|---|---|
| Microsoft.EntityFrameworkCore(.SqlServer/.Design) | 10.0.14 | MIT | Truy cập dữ liệu, migration |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.14 | MIT | Xác thực JWT giữa web ↔ gateway ↔ service |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 10.0.14 | MIT | `/health` kiểm tra CSDL |
| System.IdentityModel.Tokens.Jwt | 8.23.0 | MIT | Sinh/đọc JWT |
| System.Security.Cryptography.Pkcs | 10.0.14 | MIT | Ghim phụ thuộc gián tiếp – bản 9.0.11 có lỗ hổng GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7 |
| Ocelot | 25.0.2 | MIT | API Gateway (giữ từ basecore) |
| Yarp.ReverseProxy | 2.3.0 | MIT | BFF trong web (D17) |
| SkiaSharp / SkiaSharp.NativeAssets.Linux.NoDependencies | 3.119.2 | MIT | Ảnh thẻ bạn đọc → WebP, thu nhỏ, xoay EXIF (D35). Bản NoDependencies không cần cài fontconfig/libfreetype trong image aspnet |
| Serilog.AspNetCore / Serilog.Sinks.File | 10.0.0 / 7.0.0 | Apache-2.0 | Log kỹ thuật ghi file xoay vòng |
| Swashbuckle.AspNetCore | 10.3.1 | MIT | Swagger (chỉ Development) |
| Microsoft.Extensions.Hosting | 10.0.0 | MIT | Migrator |
| xunit / xunit.runner.visualstudio / Microsoft.NET.Test.Sdk | 2.9.3 / 3.1.5 / 18.11.1 | Apache-2.0 / MIT | Kiểm thử |
| Microsoft.Data.SqlClient | 7.1.1 | MIT | Đọc mốc thu hồi phiên (D36), sao lưu/phục hồi, kiểm thử tích hợp |
| Microsoft.Playwright | 1.64.1 | Apache-2.0 | E2E, chụp ảnh |
| Deque.AxeCore.Playwright | 4.14.0 | MPL-2.0 | Kiểm tra accessibility (D17) |

## Frontend (tự host trong `Frontend/ThuVien.Web/wwwroot`, tải bằng `scripts/tai-thu-vien-frontend.sh`)

| Thư viện | Phiên bản | Giấy phép | Dùng cho |
|---|---|---|---|
| Bootstrap | 5.3.3 | MIT | Lưới, offcanvas, dropdown, modal |
| Bootstrap Icons | 1.11.3 | MIT | Biểu tượng |
| GSAP (core, Flip, ScrollTrigger, Observer, Draggable) | 3.13.0 | GSAP Standard License (miễn phí từ 3.13) | Chuyển động |
| Be Vietnam Pro (@fontsource/be-vietnam-pro) | 5.2.8 | SIL OFL 1.1 | Font chữ |

Sẽ thêm ở các pha sau (đã duyệt trong file 01/13): Apache ECharts 5.6.0 (Pha 4), page-flip 2.0.7, Lenis 1.3.x (Pha 5–6), JsBarcode (Pha 6).

## Image Docker

| Image | Tag | Ghi chú |
|---|---|---|
| mcr.microsoft.com/mssql/server | 2022-CU27-GDR2-ubuntu-22.04 | Build 22/10/2026 (CU28 build 05/11/2026 chưa đủ 14 ngày) |
| mcr.microsoft.com/dotnet/sdk, aspnet | 10.0 | Theo GIA_DINH D6 |
| mcr.microsoft.com/playwright/dotnet | v1.64.1-noble | Kèm .NET SDK 10 và trình duyệt cho E2E |
