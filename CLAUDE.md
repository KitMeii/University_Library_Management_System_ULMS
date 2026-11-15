# CLAUDE.md – ULMS (Quản lý mượn trả sách thư viện đại học)

## Đọc trước khi làm (mỗi phiên mới)
1. File này.
2. `Prompt/Prompt_ClaudeCode_HeThong/docs/TIEN_DO.md` – đang ở pha nào, việc còn lại.
3. `Prompt/Prompt_ClaudeCode_HeThong/docs/GIA_DINH.md` – các quyết định đã được chủ dự án duyệt.
4. `Prompt/Prompt_ClaudeCode_HeThong/docs/BASECORE_PHAN_TICH.md` – phân tích backend có sẵn, nhãn GIỮ/TÁI DÙNG/XÓA/CHƯA RÕ.
5. `Prompt/Prompt_ClaudeCode_HeThong/docs/KE_HOACH_PHA_0.md` – kiến trúc, ranh giới service, Docker, lộ trình pha đã sắp lại.
6. Khi làm một pha: đọc lại các file ở cột “Đọc” của pha đó trong `docs/00_KE_HOACH_TONG_THE.md`.

## Quy ước đường dẫn (bộ tài liệu viết theo tên chung, dự án này ánh xạ như sau)
| Trong tài liệu | Thực tế trong dự án |
|---|---|
| Thư mục gốc dự án | Thư mục chứa file này. `docker-compose*.yml`, `.env.example`, `README.md`, `.gitignore` đặt ở đây |
| `basecore/` | `Backend/` (solution hiện ở `Backend/BaseCore/BaseCore/BaseCore.sln`) – sửa trực tiếp, không dựng backend song song |
| `docs/` | `Prompt/Prompt_ClaudeCode_HeThong/docs/` – mọi file tài liệu tạo thêm (BASECORE_PHAN_TICH.md, TIEN_DO.md, GIA_DINH.md, HUONG_DAN_*.md, THU_VIEN_BEN_THU_BA.md, anh_giao_dien/…) ghi vào đây |
| Ứng dụng web (`ThuVien.Web`) | `Frontend/` – ASP.NET Core MVC: Cổng bạn đọc + Dashboard nghiệp vụ (Views, wwwroot) |

## Nguồn chuẩn và thứ tự ưu tiên
`SPEC_NGHIEP_VU_BT4.md` > `02_CSDL_VA_MO_RONG.md` > quy ước basecore (file 12) > `01` > các file còn lại. Chưa rõ thì hỏi chủ dự án, không tự quyết.
Giữ nguyên tên bảng, tên trường, mã tiến trình, phân quyền, tên control (làm `id` HTML), trạng thái nút theo SPEC.

## Cách làm việc
- Làm lần lượt Pha 0A → 0 → 1 … 8. Hết mỗi pha: `docker compose up -d --build`, chạy kiểm thử, chụp ảnh Playwright (desktop 1366×768 và điện thoại 390×844), sửa lỗi, báo cáo theo mẫu mục 5 file 00, cập nhật `TIEN_DO.md`, rồi **DỪNG chờ duyệt**.
- Không xóa, không viết code khi kế hoạch và danh sách XÓA chưa được duyệt.
- Giao diện, thông báo, tài liệu: tiếng Việt có dấu.

## Git
- Remote: `https://github.com/KitMeii/University_Library_Management_System_ULMS.git`, nhánh chính `main`.
- Nếu gốc chưa có git: trước khi xóa/sửa bất kỳ file nào ở Pha 1 thì
  a. `git init`, tạo `.gitignore` (bin/, obj/, node_modules/, .vs/, .env, artifacts/, *.user);
  b. kiểm tra bí mật trong `Backend/**/appsettings*.json` (mật khẩu, chuỗi kết nối thật, khóa API) → báo chủ dự án, chuyển sang `.env`/biến môi trường **trước khi commit**;
  c. commit trạng thái ban đầu, `git branch -M main`, `git remote add origin <remote>`;
  d. nếu repo GitHub đã có file: `git pull origin main --allow-unrelated-histories` rồi `git push -u origin main`.
- Việc xóa phần thừa của basecore nằm trong **một commit riêng**.
- Hết mỗi pha được duyệt: commit và push lên origin.
