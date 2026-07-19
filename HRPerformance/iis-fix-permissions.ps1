# Fix IIS permissions — "Cannot read configuration file due to insufficient permissions"
# Run PowerShell AS ADMINISTRATOR

param(
    [string]$SitePath = $PSScriptRoot,
    [string]$PoolName = "HRPerformance"
)

$ErrorActionPreference = "Stop"

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[ERROR] Run PowerShell as Administrator" -ForegroundColor Red
    exit 1
}

# auto-detect site folder
if (-not (Test-Path (Join-Path $SitePath "web.config"))) {
    if (Test-Path "C:\inetpub\HRPerformance\web.config") { $SitePath = "C:\inetpub\HRPerformance" }
    elseif (Test-Path (Join-Path $SitePath "HRPerformance.API.dll")) { }
    else {
        Write-Host "[ERROR] web.config not found in: $SitePath" -ForegroundColor Red
        Write-Host "Usage: .\iis-fix-permissions.ps1 -SitePath C:\inetpub\HRPerformance" -ForegroundColor Yellow
        exit 1
    }
}

$SitePath = (Resolve-Path $SitePath).Path
$identity = "IIS AppPool\$PoolName"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IIS Permission Fix" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Site path : $SitePath" -ForegroundColor White
Write-Host "App Pool  : $PoolName" -ForegroundColor White
Write-Host ""

# create runtime folders
foreach ($dir in @("logs", "uploads")) {
    New-Item -ItemType Directory -Force -Path (Join-Path $SitePath $dir) | Out-Null
}

# IMPORTANT: site must NOT be only in Downloads with restricted ACL
$identities = @(
    $identity,
    "IIS_IUSRS",
    "IUSR"
)

foreach ($id in $identities) {
    Write-Host "Granting $id ..." -ForegroundColor Yellow
    # Read + Execute on entire site (includes web.config)
    icacls $SitePath /grant "${id}:(OI)(CI)RX" /T | Out-Null
}

# Write on logs and uploads only
foreach ($id in @($identity, "IIS_IUSRS")) {
    icacls (Join-Path $SitePath "logs") /grant "${id}:(OI)(CI)M" /T | Out-Null
    icacls (Join-Path $SitePath "uploads") /grant "${id}:(OI)(CI)M" /T | Out-Null
}

# web.config explicit read
icacls (Join-Path $SitePath "web.config") /grant "IIS_IUSRS:R" | Out-Null
icacls (Join-Path $SitePath "web.config") /grant "${identity}:R" | Out-Null

Write-Host ""
Write-Host "[OK] Permissions applied." -ForegroundColor Green
Write-Host ""
Write-Host "Next:" -ForegroundColor Cyan
Write-Host "  1. iisreset" -ForegroundColor White
Write-Host "  2. Open http://localhost/api/health" -ForegroundColor White
Write-Host ""
Write-Host "If site is in Downloads/Documents, move to C:\inetpub\HRPerformance" -ForegroundColor Yellow
Write-Host ""
