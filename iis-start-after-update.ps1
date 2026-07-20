# راه‌اندازی مجدد بعد از کپی فایل
param(
    [string]$SiteName = "HRPerformance",
    [string]$AppPoolName = "HRPerformance"
)

$ErrorActionPreference = "Stop"
Import-Module WebAdministration

if (-not (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue)) {
    Write-Host "App Pool '$AppPoolName' یافت نشد — iis-bind-site-5050.bat را اجرا کنید." -ForegroundColor Yellow
    exit 1
}

Start-WebAppPool -Name $AppPoolName
Start-Sleep -Seconds 1
Start-Website -Name $SiteName

Write-Host "[OK] Site راه‌اندازی شد: http://localhost:5050" -ForegroundColor Green
