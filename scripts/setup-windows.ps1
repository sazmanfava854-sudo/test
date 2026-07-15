# HR Performance - First-time Windows setup
$ErrorActionPreference = "Stop"
Set-Location (Split-Path -Parent $PSScriptRoot)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  HR Performance - Windows Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] .NET 8 SDK not found." -ForegroundColor Red
    Write-Host "Install from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

Write-Host "dotnet: $(dotnet --version)"
Write-Host ""

& "$PSScriptRoot\restore-packages.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "[OK] Setup complete." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Run database scripts 01-10 in SQL Server (HRPerformanceDB)"
Write-Host "  2. Configure appsettings.json (connection string, HrIntegration password)"
Write-Host "  3. Run: .\start.ps1"
Write-Host "  4. Open: http://localhost:5000"
Write-Host ""
Write-Host "For Cursor IDE: open only HRPerformance.sln after setup." -ForegroundColor Gray
