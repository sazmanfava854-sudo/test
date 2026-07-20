# Publish HR Performance for IIS
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Project = Join-Path $Root "src\HRPerformance.API\HRPerformance.API.csproj"
$OutDir = Join-Path $Root "publish\iis"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  HR Performance - IIS Publish" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output folder:" -ForegroundColor Yellow
Write-Host "  $OutDir" -ForegroundColor White
Write-Host ""

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] dotnet SDK not found." -ForegroundColor Red
    Write-Host "Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

dotnet publish $Project -c Release -r win-x64 --self-contained false -o $OutDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

New-Item -ItemType Directory -Force -Path (Join-Path $OutDir "logs") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $OutDir "uploads") | Out-Null

$ProdExample = Join-Path $Root "src\HRPerformance.API\appsettings.Production.example.json"
$ProdTarget = Join-Path $OutDir "appsettings.Production.json"
if (Test-Path $ProdExample) {
    Copy-Item -Force $ProdExample $ProdTarget
}

Write-Host ""
Write-Host "[OK] Publish completed!" -ForegroundColor Green
Write-Host ""
Write-Host "IIS Physical Path = this folder:" -ForegroundColor Yellow
Write-Host "  $OutDir" -ForegroundColor White
Write-Host ""
Write-Host "Settings for IIS (Production):" -ForegroundColor Yellow
Write-Host "  $OutDir\appsettings.Production.json  (ConnectionStrings + HrIntegration)" -ForegroundColor White
Write-Host "  NOT appsettings.Development.json (local dev only)" -ForegroundColor Gray
Write-Host ""
Write-Host "Login: admin (from 08_SeedData.sql)" -ForegroundColor Cyan
Write-Host ""
