# syntax=docker/dockerfile:1
# BaseCore.Migrator – chạy một lần: migration + quyền SQL + dữ liệu khởi tạo/mẫu
# Build từ thư mục gốc dự án:  docker build -f Backend/src/BaseCore.Migrator/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY Backend/src/BaseCore.Common/BaseCore.Common.csproj Backend/src/BaseCore.Common/
COPY Backend/src/BaseCore.DTO/BaseCore.DTO.csproj Backend/src/BaseCore.DTO/
COPY Backend/src/BaseCore.Entities/BaseCore.Entities.csproj Backend/src/BaseCore.Entities/
COPY Backend/src/BaseCore.Libs/BaseCore.Libs.csproj Backend/src/BaseCore.Libs/
COPY Backend/src/BaseCore.Repository/BaseCore.Repository.csproj Backend/src/BaseCore.Repository/
COPY Backend/src/BaseCore.Migrator/BaseCore.Migrator.csproj Backend/src/BaseCore.Migrator/
RUN dotnet restore Backend/src/BaseCore.Migrator/BaseCore.Migrator.csproj
COPY Backend/src/ Backend/src/
RUN dotnet publish Backend/src/BaseCore.Migrator/BaseCore.Migrator.csproj -c Release -o /app --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV TZ=Asia/Ho_Chi_Minh Logging__ThuMuc=/app/logs
COPY --from=build /app .
RUN mkdir -p /app/logs && chown -R app:app /app/logs
USER app
ENTRYPOINT ["dotnet", "BaseCore.Migrator.dll"]
