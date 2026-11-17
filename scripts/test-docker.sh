#!/bin/sh
# Chạy kiểm thử trong container (không cần SDK 10 / trình duyệt trên máy) trên một BỘ HỆ THỐNG KIỂM THỬ RIÊNG
# (GIA_DINH D49): project Compose "ulms-test", mạng thuvien-test-net, CSDL ThuVien_Test.
#   - Không đụng CSDL ThuVien và bộ hệ thống đang chạy (project ulms).
#   - Mỗi lần chạy: xóa + tạo lại ThuVien_Test (dữ liệu mẫu sạch) → chạy kiểm thử → dọn .bak kiểm thử → gỡ bộ kiểm thử.
#   - Tệp sao lưu kiểm thử ghi vào SAO_LUU_THU_MUC_KIEM_THU (.env), không ghi vào thư mục Backup thật.
# Dùng image đã build bởi:  docker compose up -d --build
# Dùng:  sh scripts/test-docker.sh [unit|integration|e2e|anh|all]   (mặc định all)
#   all = đơn vị → chụp bộ ảnh (CSDL sạch) → tích hợp → E2E
#   anh = chỉ chụp bộ ảnh giao diện; e2e = E2E (không gồm chụp ảnh)
#   GIU_LAI=1 sh scripts/test-docker.sh …  giữ bộ kiểm thử sau khi chạy (web: http://localhost:18080)
# Kết quả .trx ở artifacts/test-results, ảnh giao diện ở Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien
set -e
cd "$(dirname "$0")/.."
ROOT=$(pwd -W 2>/dev/null || pwd)
LOAI=${1:-all}
mkdir -p artifacts/test-results Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien
export MSYS_NO_PATHCONV=1 MSYS2_ENV_CONV_EXCL='*'

doc_env() { grep -E "^$1=" .env 2>/dev/null | tail -n 1 | cut -d= -f2- | tr -d '\r'; }

# ---- Bộ hệ thống kiểm thử ----
export DB_NAME=ThuVien_Test ULMS_NET=thuvien-test-net WEB_PORT=18080 GATEWAY_PORT=15000 SQL_PORT=14330
export HIEN_NHAN_DU_LIEU_MAU=false TAO_LAI_CSDL=true
# Tác vụ nền đặt chỗ quét mỗi 3 giây trong bộ kiểm thử (kiểm thử “hết hạn giữ” không phải chờ 5 phút)
export DATCHO_CHU_KY_PHUT=0.05
TM_KT=$(doc_env SAO_LUU_THU_MUC_KIEM_THU)
if [ -z "$TM_KT" ]; then
  case "$(doc_env DB_SERVER)" in
    ""|sqlserver*) TM_KT=/var/opt/mssql/backup ;; # SQL Server trong Docker: volume riêng của project ulms-test
    *) echo "Thiếu SAO_LUU_THU_MUC_KIEM_THU trong .env – thư mục riêng cho tệp sao lưu kiểm thử trên máy SQL Server"
       echo "(vd C:\\Windows\\ServiceProfiles\\MSSQLSERVER\\AppData\\Local\\Temp\\ULMS_KiemThu), không dùng thư mục Backup thật."
       exit 1 ;;
  esac
fi
export SAO_LUU_THU_MUC="$TM_KT"
NET=$ULMS_NET
KT="docker compose -p ulms-test"

CHUNG="-v $ROOT:/src -v ulms-nuget:/root/.nuget/packages -e ULMS_BUILD_OUT=/tmp/ulms -e ULMS_ROOT=/src -w /src"
TEST="test --nologo --logger trx --results-directory /src/artifacts/test-results"
LOI=0

don_dep() {
  echo "== Dọn tệp sao lưu kiểm thử, gỡ bộ kiểm thử =="
  $KT run --rm --no-deps migrator --don-sao-luu-kiem-thu || true
  [ "${GIU_LAI:-0}" = 1 ] || $KT down -v --remove-orphans
}

chay_unit() {
  echo "== Kiểm thử đơn vị =="
  docker run --rm $CHUNG -v ulms-build:/tmp/ulms mcr.microsoft.com/dotnet/sdk:10.0 \
    sh -c "dotnet $TEST --logger 'trx;LogFileName=unit.trx' Backend/tests/BaseCore.UnitTests" || LOI=1
}

khoi_dong() {
  echo "== Khởi động bộ kiểm thử (ulms-test, CSDL $DB_NAME, sao lưu: $SAO_LUU_THU_MUC) =="
  trap don_dep EXIT
  $KT up -d --remove-orphans
  i=0
  until [ "$(docker inspect -f '{{.State.Health.Status}}' ulms-test-web-1 2>/dev/null)" = healthy ]; do
    i=$((i + 1))
    [ $i -gt 90 ] && { echo "Bộ kiểm thử không sẵn sàng sau 180 giây"; $KT logs --tail 50 migrator; exit 1; }
    sleep 2
  done
}

chay_tich_hop() {
  echo "== Kiểm thử tích hợp (gateway, service, SQL Server – $DB_NAME) =="
  docker run --rm --network $NET --env-file .env $CHUNG -v ulms-build:/tmp/ulms \
    -e ULMS_GATEWAY=http://gateway:8080 -e DB_NAME=$DB_NAME -e "SAO_LUU_THU_MUC=$SAO_LUU_THU_MUC" mcr.microsoft.com/dotnet/sdk:10.0 \
    sh -c "dotnet $TEST --logger 'trx;LogFileName=integration.trx' Backend/tests/BaseCore.IntegrationTests" || LOI=1
}

# $1: bộ lọc; $2: tên tệp kết quả
chay_e2e() {
  docker run --rm --ipc=host --network $NET $CHUNG -v ulms-build-e2e:/tmp/ulms \
    -e ULMS_WEB=http://web:8080 -e ULMS_ANH=/src/Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien \
    mcr.microsoft.com/playwright/dotnet:v1.64.1-noble \
    sh -c "dotnet $TEST --logger 'trx;LogFileName=$2' --filter '$1' Frontend/tests/ThuVien.E2E" || LOI=1
}

case "$LOAI" in unit|all) chay_unit ;; esac
case "$LOAI" in integration|e2e|anh|all) khoi_dong ;; esac
case "$LOAI" in anh|all) echo "== Chụp bộ ảnh giao diện (CSDL sạch) =="; chay_e2e "FullyQualifiedName~ChupAnh" anh.trx ;; esac
case "$LOAI" in integration|all) chay_tich_hop ;; esac
case "$LOAI" in e2e|all) echo "== E2E (Playwright) =="; chay_e2e "${E2E_LOC:-FullyQualifiedName!~ChupAnh}" e2e.trx ;; esac

[ $LOI = 0 ] && echo "== Tất cả kiểm thử đạt ==" || echo "== CÓ KIỂM THỬ KHÔNG ĐẠT – xem artifacts/test-results =="
exit $LOI
