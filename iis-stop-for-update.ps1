# توقف سایت/App Pool قبل از کپی فایل جدید در inetpub
# PowerShell as Administrator
param(
    [string]$SiteName = "HRPerformance",
    [string]$AppPoolName = "HRPerformance"
)

$ErrorActionPreference = "SilentlyContinue"
Import-Module WebAdministration -ErrorAction SilentlyContinue

Write-Host "توقف IIS برای به‌روزرسانی فایل‌ها..." -ForegroundColor Yellow

if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Stop-Website -Name $SiteName
    Write-Host "  Site '$SiteName' متوقف شد"
}

if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
    Stop-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 2
    Write-Host "  App Pool '$AppPoolName' متوقف شد"
}

# آزاد کردن dotnet/out-of-process
Get-Process -Name "dotnet","w3wp" -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -like "*HRPerformance*" -or $_.MainWindowTitle -like "*HRPerformance*" } |
    ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }

Start-Sleep -Seconds 1
Write-Host ""
Write-Host "[OK] حالا فایل‌ها را در inetpub کپی کنید." -ForegroundColor Green
Write-Host "     بعد از کپی: iis-start-after-update.bat" -ForegroundColor Cyan
