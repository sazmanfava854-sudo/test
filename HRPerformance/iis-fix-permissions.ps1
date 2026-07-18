# Fix IIS folder permissions for HR Performance
# Run as Administrator in PowerShell

param(
    [string]$SitePath = (Split-Path -Parent $MyInvocation.MyCommand.Path)
)

if ($SitePath -like "*publish-iis*") { $SitePath = Join-Path (Split-Path $SitePath) "publish\iis" }
if (Test-Path (Join-Path $SitePath "HRPerformance.API.dll")) { } 
elseif (Test-Path "C:\inetpub\HRPerformance\HRPerformance.API.dll") { $SitePath = "C:\inetpub\HRPerformance" }

$ErrorActionPreference = "Stop"
Write-Host "Fixing permissions for: $SitePath" -ForegroundColor Cyan

$poolName = Read-Host "Application Pool name (default: HRPerformance)"
if ([string]::IsNullOrWhiteSpace($poolName)) { $poolName = "HRPerformance" }

$identity = "IIS AppPool\$poolName"
$folders = @("logs", "uploads")

foreach ($f in $folders) {
    $full = Join-Path $SitePath $f
    New-Item -ItemType Directory -Force -Path $full | Out-Null
    icacls $full /grant "${identity}:(OI)(CI)M" /T | Out-Null
    Write-Host "  OK: $full" -ForegroundColor Green
}

icacls $SitePath /grant "${identity}:(OI)(CI)RX" /T | Out-Null
Write-Host ""
Write-Host "Done. Restart IIS site and check logs\stdout_*.log if error continues." -ForegroundColor Yellow
