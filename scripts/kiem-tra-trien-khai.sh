#!/usr/bin/env sh
# Kiểm tra nghiệm thu triển khai (file 14 mục 8, file 11 mục 1) trên một bộ hệ thống RIÊNG mô phỏng máy chủ mới:
#   docker-compose.yml + docker-compose.prod.yml, SQL Server TRONG Docker (profile sql-docker), project "ulms-trienkhai",
#   mạng riêng, cổng 18081/18443 – không đụng CSDL ThuVien hay bộ dev/kiểm thử. Xong thì gỡ sạch (down -v).
# Kiểm tra: mọi container healthy; chỉ Caddy mở cổng; HTTP → HTTPS; HSTS + CSP; cookie Secure; Production không có dữ liệu mẫu;
#   dừng sqlserver → các service báo unhealthy, bật lại → tự healthy; down rồi up: CSDL, ảnh tải lên, bản sao lưu còn nguyên;
#   image chạy user không phải root; không lộ bí mật trong image (docker history / inspect).
# Dùng: sh scripts/kiem-tra-trien-khai.sh      (cần .env có MSSQL_SA_PASSWORD, SVC_*_PASSWORD, JWT_SECRET_KEY, ADMIN_PASSWORD)
set -u
cd "$(dirname "$0")/.."
# Git Bash (Windows): không đổi đường dẫn /opt/..., /app/... trong tham số docker exec
export MSYS_NO_PATHCONV=1 MSYS2_ENV_CONV_EXCL='*'

P=ulms-trienkhai
KQ=artifacts/test-results/kiem_tra_trien_khai.txt
mkdir -p artifacts/test-results
: > "$KQ"
export COMPOSE_PROFILES=sql-docker DB_SERVER=sqlserver DB_NAME=ThuVien ULMS_NET=thuvien-trienkhai-net \
       HTTP_PORT=18081 HTTPS_PORT=18443 ULMS_DOMAIN=localhost SAO_LUU_THU_MUC=/var/opt/mssql/backup
DC="docker compose -p $P -f docker-compose.yml -f docker-compose.prod.yml"
DAT=0; LOI=0
ghi() { echo "$*" | tee -a "$KQ"; }
dat() { DAT=$((DAT + 1)); ghi "  [ĐẠT] $*"; }
loi() { LOI=$((LOI + 1)); ghi "  [LỖI] $*"; }
kiem() { if eval "$2" >/dev/null 2>&1; then dat "$1"; else loi "$1"; fi; }
doc_env() { grep -E "^$1=" .env | head -1 | cut -d= -f2-; }
# SQL Server MỚI trong bộ kiểm tra: mật khẩu sa riêng, ngẫu nhiên, đủ chính sách (không dùng sa của SQL Server cài sẵn trong .env)
SA="Kt#$(od -An -N12 -tx1 /dev/urandom | tr -d ' \n')Aa9"
export MSSQL_SA_PASSWORD="$SA"
# Login service tạo trên SQL Server trong Docker phải đạt chính sách mật khẩu mạnh (GIA_DINH D85) – báo lỗi rõ trước khi dựng
sh scripts/trien-khai.sh kiem-mat-khau || exit 1
sql() { $DC exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -d ThuVien -U sa -P "$SA" -C -b -h -1 -W -Q "SET NOCOUNT ON; $1" | tr -d '[:space:]'; }

cho_healthy() { # $1 = số giây tối đa
  i=0
  while [ $i -lt "$1" ]; do
    chua=$($DC ps --format '{{.Service}} {{.Health}}' | awk '$2 != "healthy" && $2 != "" {print $1}')
    [ -z "$chua" ] && return 0
    sleep 5; i=$((i + 5))
  done
  echo "Chưa healthy: $chua" | tee -a "$KQ"; return 1
}
health_dv() { docker inspect -f '{{.State.Health.Status}}' "$($DC ps -q "$1")"; }

don() { $DC down -v --remove-orphans >/dev/null 2>&1; }
trap don EXIT

ghi "== Kiểm tra triển khai $(date '+%d/%m/%Y %H:%M') – project $P =="
ghi "-- 1. Máy mới: chỉ Docker + .env → up -d --build"
don
if $DC up -d --build >>"$KQ.log" 2>&1 && cho_healthy 420; then dat "docker compose (prod + SQL Server trong Docker) dựng xong, mọi container healthy"
else loi "dựng hệ thống / healthy (xem $KQ.log)"; $DC ps | tee -a "$KQ"; exit 1; fi

ghi "-- 2. Cổng và HTTPS"
mo=$($DC ps --format '{{.Service}}|{{.Ports}}' | awk -F'|' '$2 ~ /->/ {print $1}' | sort -u | tr '\n' ' ')
kiem "chỉ caddy mở cổng ra máy chủ (thấy: $mo)" '[ "$mo" = "caddy " ]'
kiem "HTTP 18081 chuyển hướng sang HTTPS" 'curl -s -o /dev/null -w "%{http_code} %{redirect_url}" http://localhost:18081/ | grep -Eq "^30[178] https://"'
H=$(curl -sk -D - -o /dev/null https://localhost:18443/)
kiem "HTTPS trang chủ 200" 'echo "$H" | grep -q "^HTTP/[0-9.]* 200"'
kiem "Strict-Transport-Security" 'echo "$H" | grep -qi "^strict-transport-security: max-age="'
kiem "CSP script-src self, không unsafe-inline/unsafe-hashes" 'echo "$H" | grep -i "^content-security-policy:" | grep -q "script-src '"'"'self'"'"'" && ! echo "$H" | grep -i "^content-security-policy:" | grep -qi "unsafe-"'
kiem "X-Content-Type-Options nosniff" 'echo "$H" | grep -qi "^x-content-type-options: nosniff"'
kiem "Không lộ header Server" '! echo "$H" | grep -qi "^server:"'
HD=$(curl -sk -D - -o /dev/null https://localhost:18443/dang-nhap)
kiem "Cookie (antiforgery) có Secure + HttpOnly + SameSite" 'echo "$HD" | grep -i "^set-cookie:" | grep -i "secure" | grep -i "httponly" | grep -qi "samesite"'
kiem "Production không nạp dữ liệu mẫu (0 đầu sách)" 'curl -sk https://localhost:18443/api/cong/sach/so-lieu | grep -q "\"soDauSach\":0"'
kiem "Production không có nhãn DỮ LIỆU MẪU" '! curl -sk https://localhost:18443/ | grep -q "DỮ LIỆU MẪU"'
T404=$(curl -sk -w '|%{http_code}' https://localhost:18443/khong-co-trang-nay)
kiem "Trang lỗi 404 tiếng Việt, không lộ chi tiết lỗi" 'echo "$T404" | grep -q "|404$" && echo "$T404" | grep -q "<h1>Không tìm thấy trang</h1>" && ! echo "$T404" | grep -qi "exception\|stack trace"'
kiem "Tài khoản admin khởi tạo tồn tại (Production bắt đổi mật khẩu ở lần đăng nhập đầu – XacThucNhanVienService)" '[ "$(sql "SELECT COUNT(*) FROM quantri.NGUOIDUNG WHERE tendangnhap = '"'"'admin'"'"'")" = "1" ]'

ghi "-- 3. Dừng SQL Server → service báo lỗi; bật lại → tự phục hồi"
$DC stop sqlserver >/dev/null 2>&1
i=0; while [ $i -lt 180 ] && [ "$(health_dv khosach)" = "healthy" ]; do sleep 5; i=$((i + 5)); done
kiem "khosach unhealthy khi SQL Server dừng (sau ${i}s)" '[ "$(health_dv khosach)" = "unhealthy" ]'
$DC start sqlserver >/dev/null 2>&1
if cho_healthy 300; then dat "bật lại SQL Server → mọi container healthy trở lại"; else loi "không tự phục hồi"; fi
kiem "trang chủ chạy lại sau phục hồi" 'curl -sk -o /dev/null -w "%{http_code}" https://localhost:18443/ | grep -q 200'

ghi "-- 4. down rồi up: dữ liệu, ảnh tải lên, bản sao lưu còn nguyên"
sql "INSERT INTO khosach.THELOAI (ma_theloai, ten_theloai, mota, bieutuong, thutu_hienthi) VALUES ('TLKTTK', N'Kiểm tra triển khai', N'bền vững', 'bi-bookmark', 99);" >/dev/null
sql "BACKUP DATABASE ThuVien TO DISK = '/var/opt/mssql/backup/kiem_tra_trien_khai.bak' WITH INIT;" >/dev/null
$DC exec -T web sh -c 'echo ulms > /app/wwwroot/uploads/kiem_tra_trien_khai.txt'
$DC down >/dev/null 2>&1
$DC up -d >>"$KQ.log" 2>&1 && cho_healthy 300 >/dev/null
kiem "CSDL còn dòng đã ghi" '[ "$(sql "SELECT COUNT(*) FROM khosach.THELOAI WHERE ma_theloai = '"'"'TLKTTK'"'"'")" = "1" ]'
kiem "volume ảnh tải lên còn tệp" '$DC exec -T web test -f /app/wwwroot/uploads/kiem_tra_trien_khai.txt'
kiem "volume sao lưu còn tệp .bak" '$DC exec -T sqlserver test -f /var/opt/mssql/backup/kiem_tra_trien_khai.bak'

ghi "-- 5. Image: user không phải root, không có bí mật"
for im in web gateway quantri khosach muontra vipham migrator; do
  u=$(docker image inspect -f '{{.Config.User}}' "ulms/$im:local")
  kiem "ulms/$im chạy user '$u'" '[ -n "$u" ] && [ "$u" != "root" ] && [ "$u" != "0" ]'
done
lo=""
for k in SVC_QUANTRI_PASSWORD SVC_KHOSACH_PASSWORD SVC_MUONTRA_PASSWORD SVC_VIPHAM_PASSWORD JWT_SECRET_KEY ADMIN_PASSWORD; do
  v=$(doc_env $k)
  [ -z "$v" ] && continue
  for im in web gateway quantri khosach muontra vipham migrator; do
    if docker history --no-trunc "ulms/$im:local" | grep -qF "$v" || docker image inspect "ulms/$im:local" | grep -qF "$v"; then lo="$lo $k@$im"; fi
  done
done
v="$SA"; for im in web gateway quantri khosach muontra vipham migrator; do docker history --no-trunc "ulms/$im:local" | grep -qF "$v" && lo="$lo MSSQL_SA_PASSWORD@$im"; done
kiem "không thấy giá trị bí mật .env trong lịch sử / cấu hình image${lo:+ (lộ:$lo)}" '[ -z "$lo" ]'
kiem ".env không nằm trong image web" '! docker run --rm --entrypoint sh ulms/web:local -c "test -f /app/.env"'

ghi "== Kết quả: $DAT đạt, $LOI lỗi =="
[ "$LOI" -eq 0 ]
