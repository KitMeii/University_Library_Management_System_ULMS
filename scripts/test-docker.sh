#!/bin/sh
# Chạy kiểm thử trong container (không cần SDK 10 / trình duyệt trên máy). Hệ thống phải đang chạy:
#   docker compose up -d --build
# Dùng:  sh scripts/test-docker.sh [unit|integration|e2e|anh|all]   (mặc định all)
#   anh: chỉ chụp bộ ảnh giao diện (chạy trên CSDL sạch: docker compose down -v && HIEN_NHAN_DU_LIEU_MAU=false docker compose up -d --build)
# Kết quả .trx ở artifacts/test-results, ảnh giao diện ở Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien
set -e
cd "$(dirname "$0")/.."
ROOT=$(pwd -W 2>/dev/null || pwd)
LOAI=${1:-all}
NET=thuvien-net
mkdir -p artifacts/test-results Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien
CHUNG="-v $ROOT:/src -v ulms-nuget:/root/.nuget/packages -e ULMS_BUILD_OUT=/tmp/ulms -e ULMS_ROOT=/src -w /src"
TEST="test --nologo --logger trx --results-directory /src/artifacts/test-results"

if [ "$LOAI" = unit ] || [ "$LOAI" = all ]; then
  echo "== Kiểm thử đơn vị =="
  MSYS_NO_PATHCONV=1 docker run --rm $CHUNG -v ulms-build:/tmp/ulms mcr.microsoft.com/dotnet/sdk:10.0 \
    sh -c "dotnet $TEST --logger 'trx;LogFileName=unit.trx' Backend/tests/BaseCore.UnitTests"
fi
if [ "$LOAI" = integration ] || [ "$LOAI" = all ]; then
  echo "== Kiểm thử tích hợp (gateway, service, SQL Server) =="
  MSYS_NO_PATHCONV=1 docker run --rm --network $NET --env-file .env $CHUNG -v ulms-build:/tmp/ulms \
    -e ULMS_GATEWAY=http://gateway:8080 mcr.microsoft.com/dotnet/sdk:10.0 \
    sh -c "dotnet $TEST --logger 'trx;LogFileName=integration.trx' Backend/tests/BaseCore.IntegrationTests"
fi
LOC=""
[ "$LOAI" = anh ] && LOC="--filter FullyQualifiedName~ChupAnh"
if [ "$LOAI" = e2e ] || [ "$LOAI" = all ] || [ "$LOAI" = anh ]; then
  echo "== E2E + chụp ảnh (Playwright) =="
  MSYS_NO_PATHCONV=1 docker run --rm --ipc=host --network $NET $CHUNG -v ulms-build-e2e:/tmp/ulms \
    -e ULMS_WEB=http://web:8080 -e ULMS_ANH=/src/Prompt/Prompt_ClaudeCode_HeThong/docs/anh_giao_dien \
    mcr.microsoft.com/playwright/dotnet:v1.64.1-noble \
    sh -c "dotnet $TEST --logger 'trx;LogFileName=e2e.trx' $LOC Frontend/tests/ThuVien.E2E"
fi
