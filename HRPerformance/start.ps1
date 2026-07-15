# HR Performance - Windows startup script
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  HR Performance System" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "dotnet not found. Install .NET 9 SDK first." -ForegroundColor Red
    exit 1
}

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Host "npm not found. Install Node.js first." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "node_modules") -or -not (Test-Path "frontend/hr-performance-web/node_modules")) {
    Write-Host "Installing dependencies (first run)..." -ForegroundColor Yellow
    npm run setup
}

Write-Host ""
Write-Host "Starting services..." -ForegroundColor Green
Write-Host "  Backend  -> http://localhost:5000"
Write-Host "  Swagger  -> http://localhost:5000/swagger"
Write-Host "  Frontend -> http://localhost:3000"
Write-Host ""
Write-Host "Press Ctrl+C to stop"
Write-Host ""

npm run dev
