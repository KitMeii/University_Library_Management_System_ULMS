# syntax=docker/dockerfile:1
# BaseCore.ApiGateway – API Gateway Ocelot
# Build từ thư mục gốc dự án:  docker build -f Backend/src/BaseCore.ApiGateway/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY Backend/src/BaseCore.Common/BaseCore.Common.csproj Backend/src/BaseCore.Common/
COPY Backend/src/BaseCore.ApiGateway/BaseCore.ApiGateway.csproj Backend/src/BaseCore.ApiGateway/
RUN dotnet restore Backend/src/BaseCore.ApiGateway/BaseCore.ApiGateway.csproj
COPY Backend/src/ Backend/src/
RUN dotnet publish Backend/src/BaseCore.ApiGateway/BaseCore.ApiGateway.csproj -c Release -o /app --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080     TZ=Asia/Ho_Chi_Minh     Logging__ThuMuc=/app/logs
COPY --from=build /app .
RUN mkdir -p /app/logs && chown -R app:app /app/logs
USER app
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=40s --retries=5 CMD ["dotnet", "BaseCore.ApiGateway.dll", "--healthcheck"]
ENTRYPOINT ["dotnet", "BaseCore.ApiGateway.dll"]
