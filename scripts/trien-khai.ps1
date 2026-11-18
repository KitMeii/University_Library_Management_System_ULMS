# Windows PowerShell: các lệnh chuẩn của file 14 mục 5 cho máy chủ / máy dev chạy Windows.
# Dùng:  .\scripts\trien-khai.ps1 <lenh>
#   dev          docker compose up -d --build                 (Development, dữ liệu mẫu – KHÔNG kiểm tra độ mạnh mật khẩu)
#   prod         kiểm tra mật khẩu SQL → docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
#   kiem-mat-khau  chỉ kiểm tra mật khẩu SQL trong .env theo chính sách máy chủ thật (không chạy gì)
#   trang-thai   docker compose ps (prod)
#   log          docker compose logs -f web muontra (prod)
#   sao-luu      chép thư mục sao lưu của SQL Server trong Docker ra .\sao-luu-yyyy-MM-dd
#   dung         docker compose down (prod) – KHÔNG xóa dữ liệu
#   kiem-tra     kiểm tra nghiệm thu triển khai trên bộ riêng (scripts/kiem-tra-trien-khai.sh – cần Git for Windows)
param([Parameter(Mandatory)][ValidateSet('dev', 'prod', 'kiem-mat-khau', 'trang-thai', 'log', 'sao-luu', 'dung', 'kiem-tra')][string]$Lenh)

$ErrorActionPreference = 'Stop'

# Giá trị có hiệu lực: biến môi trường của phiên (Compose ưu tiên) rồi mới tới .env
function Get-GiaTri([string]$ten) {
    $v = [Environment]::GetEnvironmentVariable($ten)
    if ($v) { return $v }
    $dong = Get-Content .env -Encoding UTF8 | Where-Object { $_ -match "^$ten=" } | Select-Object -Last 1
    if ($dong) { return ($dong -replace "^$ten=", '').TrimEnd("`r") }
    return ''
}

# Chính sách mật khẩu SQL Server cho máy chủ thật / SQL Server trong Docker (GIA_DINH D85, HUONG_DAN_TRIEN_KHAI mục 3.1):
# ≥ 8 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt; không chứa ; " ' $ \ (dùng trong chuỗi kết nối)
function Test-MatKhauSql {
    $loi = @()
    foreach ($ten in 'MSSQL_SA_PASSWORD', 'SVC_QUANTRI_PASSWORD', 'SVC_KHOSACH_PASSWORD', 'SVC_MUONTRA_PASSWORD', 'SVC_VIPHAM_PASSWORD') {
        $mk = Get-GiaTri $ten
        $thieu = @()
        if ($mk.Length -lt 8) { $thieu += 'ít nhất 8 ký tự' }
        if ($mk -cnotmatch '[A-Z]') { $thieu += 'chữ hoa' }
        if ($mk -cnotmatch '[a-z]') { $thieu += 'chữ thường' }
        if ($mk -notmatch '[0-9]') { $thieu += 'chữ số' }
        if ($mk -notmatch '[^A-Za-z0-9]') { $thieu += 'ký tự đặc biệt' }
        if ($mk -match "[;`"'`$\\]") { $thieu += 'không được chứa ; " '' $ \' }
        if ($thieu.Count) { $loi += "  - ${ten}: cần $($thieu -join ', ')" }
    }
    if ($loi.Count) {
        Write-Host 'Mật khẩu SQL trong .env CHƯA đạt chính sách máy chủ thật:' -ForegroundColor Red
        $loi | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        Write-Host 'Sửa .env (HUONG_DAN_TRIEN_KHAI.md mục 3.1) rồi chạy lại. Môi trường phát triển (lệnh dev) không bị kiểm tra.' -ForegroundColor Yellow
        return $false
    }
    Write-Host 'Mật khẩu SQL đạt chính sách máy chủ thật.' -ForegroundColor Green
    return $true
}

Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    if (-not (Test-Path .env)) { throw 'Chưa có .env – chép từ .env.example rồi điền giá trị (HUONG_DAN_TRIEN_KHAI.md mục 3).' }
    $prod = @('-f', 'docker-compose.yml', '-f', 'docker-compose.prod.yml')
    switch ($Lenh) {
        'dev'           { docker compose up -d --build }
        'kiem-mat-khau' { if (-not (Test-MatKhauSql)) { exit 1 }; exit 0 }
        'prod'          {
            if (-not (Test-MatKhauSql)) { exit 1 }
            docker compose @prod up -d --build; docker compose @prod ps
        }
        'trang-thai'    { docker compose @prod ps }
        'log'           { docker compose @prod logs -f --tail 200 web muontra }
        'sao-luu'       {
            $dich = "sao-luu-$(Get-Date -Format 'yyyy-MM-dd')"
            docker compose @prod cp sqlserver:/var/opt/mssql/backup $dich
            Write-Host "Đã chép bản sao lưu vào $dich – chép tiếp sang NAS / máy khác."
        }
        'dung'          { docker compose @prod down }
        'kiem-tra'      {
            $sh = @("$env:ProgramFiles\Git\bin\sh.exe", "${env:ProgramFiles(x86)}\Git\bin\sh.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
            if (-not $sh) { throw 'Cần Git for Windows (sh.exe).' }
            & $sh 'scripts/kiem-tra-trien-khai.sh'
        }
    }
    exit $LASTEXITCODE
}
finally { Pop-Location }
