# HR Performance - Windows (فقط .NET - بدون نیاز به Node.js)
$ErrorActionPreference = "Continue"
Set-Location $PSScriptRoot

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  HR Performance System" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "dotnet یافت نشد!" -ForegroundColor Red
    Write-Host "ابتدا .NET 8 SDK را نصب کنید:" -ForegroundColor Yellow
    Write-Host "  https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "انتخاب پورت آزاد..." -ForegroundColor Yellow
$portFile = Join-Path $env:TEMP 'hr-performance-port.txt'
& "$PSScriptRoot/scripts/resolve-app-port.ps1" -OutFile $portFile | Out-Null
$appPort = if (Test-Path $portFile) { (Get-Content $portFile -Raw).Trim() } else { '5280' }
if ($appPort -notmatch '^\d+$') { $appPort = '5280' }

$env:ASPNETCORE_URLS = "http://localhost:$appPort"
$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host ""
Write-Host "در حال اجرا..." -ForegroundColor Green
Write-Host "  Application -> http://localhost:$appPort" -ForegroundColor White
Write-Host "  Swagger     -> http://localhost:$appPort/swagger" -ForegroundColor White
Write-Host ""
Write-Host "برای توقف: Ctrl+C" -ForegroundColor Gray
Write-Host ""

dotnet run --project src/HRPerformance.API/HRPerformance.API.csproj --no-launch-profile
