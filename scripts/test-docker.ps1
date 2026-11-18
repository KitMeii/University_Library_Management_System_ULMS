# Windows PowerShell: chạy kiểm thử trong Docker (cùng logic với scripts/test-docker.sh – GIA_DINH D49).
# Dùng:  .\scripts\test-docker.ps1 [unit|integration|e2e|anh|all|hieunang]
# Cần Git for Windows (có sẵn sh.exe) – README mục 1.
param([ValidateSet('unit', 'integration', 'e2e', 'anh', 'all', 'hieunang')][string]$Loai = 'all')

$ErrorActionPreference = 'Stop'
$goc = Split-Path -Parent $PSScriptRoot
$sh = @(
    (Get-Command sh.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -First 1),
    "$env:ProgramFiles\Git\bin\sh.exe",
    "${env:ProgramFiles(x86)}\Git\bin\sh.exe"
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $sh) { throw 'Không tìm thấy sh.exe – cài Git for Windows (https://git-scm.com) rồi chạy lại.' }

Push-Location $goc
try {
    & $sh 'scripts/test-docker.sh' $Loai
    exit $LASTEXITCODE
}
finally { Pop-Location }
