$ErrorActionPreference = "Stop"

Write-Host "============================================"
Write-Host " HR Performance - NuGet Package Restore"
Write-Host "============================================"
Write-Host ""

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] dotnet SDK not found. Install .NET 8 SDK:" -ForegroundColor Red
    Write-Host "        https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

Write-Host "Using: $(dotnet --version)"
Write-Host ""

$maxAttempts = 3
for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
    Write-Host "[Attempt $attempt/$maxAttempts] Restoring packages..."
    dotnet restore HRPerformance.sln --disable-parallel --verbosity minimal
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "[OK] Packages restored successfully." -ForegroundColor Green
        Write-Host "     Reopen the solution in Cursor/VS Code for faster IntelliSense."
        exit 0
    }

    Write-Host ""
    Write-Host "[WARN] Restore failed. Common fixes:" -ForegroundColor Yellow
    Write-Host "  1. Check internet / VPN / corporate proxy"
    Write-Host "  2. Run: dotnet nuget locals all --clear"
    Write-Host "  3. Temporarily disable antivirus SSL inspection"
    Write-Host "  4. Set proxy: `$env:HTTPS_PROXY='http://proxy:port'"
    Write-Host ""

    if ($attempt -lt $maxAttempts) {
        Start-Sleep -Seconds 5
    }
}

Write-Host "[ERROR] Restore failed after $maxAttempts attempts." -ForegroundColor Red
exit 1
