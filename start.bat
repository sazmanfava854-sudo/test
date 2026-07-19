@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance System
echo ==========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo dotnet یافت نشد! ابتدا .NET 8 SDK را نصب کنید.
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

call start-local.bat
