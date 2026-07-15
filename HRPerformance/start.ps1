# HR Performance - Windows (فقط .NET - بدون نیاز به Node.js)
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  HR Performance System" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "dotnet یافت نشد!" -ForegroundColor Red
    Write-Host "ابتدا .NET 9 SDK را نصب کنید:" -ForegroundColor Yellow
    Write-Host "  https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "در حال اجرا..." -ForegroundColor Green
Write-Host "  Application -> http://localhost:5000" -ForegroundColor White
Write-Host "  Swagger     -> http://localhost:5000/swagger" -ForegroundColor White
Write-Host ""
Write-Host "برای توقف: Ctrl+C" -ForegroundColor Gray
Write-Host ""

dotnet run --project src/HRPerformance.API/HRPerformance.API.csproj --launch-profile http
