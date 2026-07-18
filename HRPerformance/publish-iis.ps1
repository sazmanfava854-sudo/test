# Publish HR Performance for IIS
# Run in PowerShell as Administrator (recommended)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Project = Join-Path $Root "src\HRPerformance.API\HRPerformance.API.csproj"
$OutDir = Join-Path $Root "publish\iis"

Write-Host "Publishing to: $OutDir" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "dotnet SDK not found. Install .NET 8 SDK first." -ForegroundColor Red
    exit 1
}

dotnet publish $Project -c Release -o $OutDir --self-contained false

# folders needed at runtime
New-Item -ItemType Directory -Force -Path (Join-Path $OutDir "logs") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $OutDir "uploads") | Out-Null

Write-Host ""
Write-Host "Publish completed." -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Edit appsettings.Production.json in publish folder (SQL + JWT + MIS password)"
Write-Host "  2. IIS: Application Pool -> No Managed Code, Integrated pipeline"
Write-Host "  3. IIS: Site physical path -> $OutDir"
Write-Host "  4. App Pool identity needs read/write on logs and uploads"
Write-Host "  5. Install ASP.NET Core 8 Hosting Bundle if not installed"
Write-Host ""
Write-Host "Demo on IIS: set environment variable ASPNETCORE_ENVIRONMENT=Demo on the site"
