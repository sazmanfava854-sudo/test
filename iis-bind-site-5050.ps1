# IIS — سایت HR Performance روی پورت ثابت 5050
# PowerShell as Administrator
param(
    [string]$SiteName = "HRPerformance",
    [string]$AppPoolName = "HRPerformance",
    [string]$PhysicalPath = $PSScriptRoot,
    [int]$Port = 5050
)

$ErrorActionPreference = "Stop"

Import-Module WebAdministration -ErrorAction Stop

if (-not (Test-Path (Join-Path $PhysicalPath "HRPerformance.API.dll"))) {
    Write-Error "HRPerformance.API.dll در $PhysicalPath یافت نشد — Physical Path را درست بدهید."
}

if (-not (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue)) {
    Write-Host "ایجاد App Pool: $AppPoolName"
    New-WebAppPool -Name $AppPoolName | Out-Null
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
}

$existing = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "به‌روزرسانی سایت $SiteName → پورت $Port"
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $PhysicalPath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
    # حذف bindingهای قبلی http و افزودن 5050
    Get-WebBinding -Name $SiteName -Protocol "http" -ErrorAction SilentlyContinue | ForEach-Object { $_.Remove() }
    New-WebBinding -Name $SiteName -Protocol http -Port $Port -IPAddress "*"
} else {
    Write-Host "ایجاد سایت $SiteName روی http://*:${Port}"
    New-Website -Name $SiteName -Port $Port -PhysicalPath $PhysicalPath -ApplicationPool $AppPoolName | Out-Null
}

Write-Host ""
Write-Host "[OK] IIS Site: http://localhost:$Port" -ForegroundColor Green
Write-Host "     Physical Path: $PhysicalPath"
Write-Host "     App Pool: $AppPoolName"
Write-Host ""
Write-Host "تست: http://localhost:$Port/api/health"
