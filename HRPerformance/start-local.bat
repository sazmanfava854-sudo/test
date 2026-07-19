@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance - اجرا (بدون Build)
echo ==========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo dotnet یافت نشد! .NET 8 Runtime/SDK را نصب کنید:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

if not exist "app\HRPerformance.API.dll" (
    echo [خطا] پوشه app یافت نشد!
    echo.
    echo این نسخه باید از قبل Build شده باشد.
    echo نسخه run-only را دانلود کنید — نیازی به Build نیست.
    echo.
    pause
    exit /b 1
)

set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:5050

echo اجرای مستقیم — بدون Build...
echo   Application: http://localhost:5050
echo   Health:      http://localhost:5050/api/health
echo.
echo تنظیمات: app\appsettings.Development.json
echo   (راهنما: app-CONNECTION_SETUP.txt)
echo.
echo برای توقف: Ctrl+C — این پنجره را باز نگه دارید.
echo.

pushd app
dotnet HRPerformance.API.dll
popd

pause
