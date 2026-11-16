#!/bin/sh
# Xuất bộ tệp .sql tạo CSDL ThuVien (Database/01..03) từ migration + dữ liệu khởi tạo + quyền SQL – GIA_DINH D44.
# Chạy lại sau mỗi lần thêm migration. Không cần SDK trên máy:  sh scripts/xuat-sql.sh
set -e
cd "$(dirname "$0")/.."
ROOT=$(pwd -W 2>/dev/null || pwd)
MSYS_NO_PATHCONV=1 docker run --rm -v "$ROOT:/src" -v ulms-nuget:/root/.nuget/packages -v ulms-build:/tmp/ulms \
  -e ULMS_BUILD_OUT=/tmp/ulms -e ULMS_ROOT=/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 \
  sh -c "dotnet run --project Backend/src/BaseCore.Migrator -c Release -- --xuat-sql /src/Database"
