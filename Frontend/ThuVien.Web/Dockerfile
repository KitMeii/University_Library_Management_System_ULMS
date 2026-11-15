# syntax=docker/dockerfile:1
# ThuVien.Web – Cổng bạn đọc + Hệ thống nghiệp vụ (MVC). Thư viện frontend nằm sẵn trong wwwroot/lib (không gọi CDN).
# Build từ thư mục gốc dự án:  docker build -f Frontend/ThuVien.Web/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY Backend/src/BaseCore.Common/BaseCore.Common.csproj Backend/src/BaseCore.Common/
COPY Frontend/ThuVien.Web/ThuVien.Web.csproj Frontend/ThuVien.Web/
RUN dotnet restore Frontend/ThuVien.Web/ThuVien.Web.csproj
COPY Backend/src/BaseCore.Common/ Backend/src/BaseCore.Common/
COPY Frontend/ThuVien.Web/ Frontend/ThuVien.Web/
RUN dotnet publish Frontend/ThuVien.Web/ThuVien.Web.csproj -c Release -o /app --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    TZ=Asia/Ho_Chi_Minh \
    Logging__ThuMuc=/app/logs \
    DataProtection__ThuMuc=/app/keys
COPY --from=build /app .
RUN mkdir -p /app/logs /app/keys /app/wwwroot/uploads && chown -R app:app /app/logs /app/keys /app/wwwroot/uploads
USER app
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 CMD ["dotnet", "ThuVien.Web.dll", "--healthcheck"]
ENTRYPOINT ["dotnet", "ThuVien.Web.dll"]
