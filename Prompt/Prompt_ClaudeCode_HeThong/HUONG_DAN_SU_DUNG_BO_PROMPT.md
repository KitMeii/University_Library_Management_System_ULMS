# Cách dùng bộ prompt

1. Cài trên máy: **Docker Desktop** (Windows bật WSL 2; Mac chip Apple bật Rosetta cho image x86/amd64 – cần cho SQL Server), .NET SDK đúng phiên bản basecore (để Claude Code build và gỡ lỗi ngoài container), Node.js (để Playwright tải trình duyệt), Git, VS Code + Claude Code.
2. Tạo thư mục dự án, đặt vào gốc: thư mục **`basecore/`** của bạn và **nguyên thư mục `docs/`** của gói này. Chạy `git init` và commit một lần trạng thái ban đầu (để mọi thay đổi, kể cả xóa, đều quay lại được).
3. Mở thư mục dự án bằng VS Code, mở Claude Code, chuyển sang chế độ **Plan**, dán nội dung `PROMPT_KHOI_DONG.md`.
4. Claude Code sẽ khảo sát basecore trước (không sửa gì) và trình `docs/BASECORE_PHAN_TICH.md` + kế hoạch + câu hỏi. Đọc kỹ danh sách XÓA rồi mới duyệt.
5. Từ Pha 1 trở đi, bạn tự chạy thử được bằng: chép `.env.example` thành `.env`, điền mật khẩu, rồi `docker compose up -d --build`; mở địa chỉ ghi trong README.
6. Mỗi pha xong: xem báo cáo, ảnh chụp desktop và điện thoại trước khi cho làm tiếp.
7. Phiên bị ngắt: mở phiên mới, nói “Đọc docs/TIEN_DO.md và làm tiếp pha đang dở”.
