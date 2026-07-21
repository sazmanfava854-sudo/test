# یک‌بار publish — بعداً بدون build اجرا می‌شود
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\RayvarzResend.Web

Write-Host "Publishing..." -ForegroundColor Cyan
dotnet publish -c Release -o ..\publish --nologo

Write-Host "Starting (no build)..." -ForegroundColor Green
Set-Location ..\publish
.\RayvarzResend.Web.exe
