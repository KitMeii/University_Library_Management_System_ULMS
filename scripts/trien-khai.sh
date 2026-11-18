#!/usr/bin/env sh
# Lệnh chuẩn triển khai trên máy chủ Linux (file 14 mục 5) – bản PowerShell: scripts/trien-khai.ps1
# Dùng:  sh scripts/trien-khai.sh <lenh>
#   prod           kiểm tra mật khẩu SQL → docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
#   kiem-mat-khau  chỉ kiểm tra mật khẩu SQL trong .env theo chính sách máy chủ thật
#   trang-thai | log | sao-luu | dung
# Môi trường phát triển (docker compose up -d --build) KHÔNG đi qua kiểm tra này (GIA_DINH D85).
set -eu
cd "$(dirname "$0")/.."
[ -f .env ] || { echo "Chưa có .env – chép từ .env.example rồi điền giá trị (HUONG_DAN_TRIEN_KHAI.md mục 3)." >&2; exit 1; }
DC="docker compose -f docker-compose.yml -f docker-compose.prod.yml"

# Giá trị có hiệu lực: biến môi trường của phiên (Compose ưu tiên) rồi mới tới .env
gia_tri() {
  eval "v=\${$1:-}"
  [ -n "$v" ] || v=$(grep -E "^$1=" .env | tail -n 1 | cut -d= -f2- | tr -d '\r')
  printf '%s' "$v"
}

# Chính sách: ≥ 8 ký tự, chữ hoa, chữ thường, số, ký tự đặc biệt; không chứa ; " ' $ \ (GIA_DINH D85, HUONG_DAN_TRIEN_KHAI mục 3.1)
kiem_mat_khau() {
  loi=0
  for ten in MSSQL_SA_PASSWORD SVC_QUANTRI_PASSWORD SVC_KHOSACH_PASSWORD SVC_MUONTRA_PASSWORD SVC_VIPHAM_PASSWORD; do
    mk=$(gia_tri "$ten")
    thieu=""
    [ "${#mk}" -ge 8 ] || thieu="$thieu, ít nhất 8 ký tự"
    printf '%s' "$mk" | grep -q '[A-Z]' || thieu="$thieu, chữ hoa"
    printf '%s' "$mk" | grep -q '[a-z]' || thieu="$thieu, chữ thường"
    printf '%s' "$mk" | grep -q '[0-9]' || thieu="$thieu, chữ số"
    printf '%s' "$mk" | grep -q '[^A-Za-z0-9]' || thieu="$thieu, ký tự đặc biệt"
    printf '%s' "$mk" | grep -q "[;\"'\$\\\\]" && thieu="$thieu, không được chứa ; \" ' \$ \\"
    if [ -n "$thieu" ]; then
      [ "$loi" = 0 ] && echo "Mật khẩu SQL trong .env CHƯA đạt chính sách máy chủ thật:" >&2
      echo "  - $ten: cần ${thieu#, }" >&2
      loi=1
    fi
  done
  if [ "$loi" = 1 ]; then
    echo "Sửa .env (HUONG_DAN_TRIEN_KHAI.md mục 3.1) rồi chạy lại. Môi trường phát triển không bị kiểm tra." >&2
    return 1
  fi
  echo "Mật khẩu SQL đạt chính sách máy chủ thật."
}

case "${1:-}" in
  kiem-mat-khau) kiem_mat_khau ;;
  prod)       kiem_mat_khau && $DC up -d --build && $DC ps ;;
  trang-thai) $DC ps ;;
  log)        $DC logs -f --tail 200 web muontra ;;
  sao-luu)    d="sao-luu-$(date +%F)"; $DC cp sqlserver:/var/opt/mssql/backup "$d" && echo "Đã chép bản sao lưu vào $d – chép tiếp sang NAS / máy khác." ;;
  dung)       $DC down ;;
  *) echo "Dùng: sh scripts/trien-khai.sh {prod|kiem-mat-khau|trang-thai|log|sao-luu|dung}" >&2; exit 2 ;;
esac
