#!/bin/sh
# Build/test toàn solution trong container .NET 10 (không cần cài SDK 10 trên máy).
# Dùng: sh scripts/build-docker.sh [lệnh dotnet...]   (mặc định: build ULMS.slnx)
set -e
cd "$(dirname "$0")/.."
ROOT=$(pwd -W 2>/dev/null || pwd)
CMD=${*:-"build ULMS.slnx -nologo -v q"}
MSYS_NO_PATHCONV=1 docker run --rm -v "$ROOT:/src" -v ulms-nuget:/root/.nuget/packages -v ulms-build:/tmp/ulms \
  -e ULMS_BUILD_OUT=/tmp/ulms -w /src mcr.microsoft.com/dotnet/sdk:10.0 sh -c "dotnet $CMD"
