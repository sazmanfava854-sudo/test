@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance - LOCAL (SQL Server)
echo ==========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo dotnet یافت نشد! .NET 8 SDK را نصب کنید.
    pause
    exit /b 1
)

set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:5050

REM نسخه از پیش Build شده — بدون انتظار (توصیه‌شده)
if exist "app\HRPerformance.API.dll" (
    echo اجرای نسخه آماده — بدون Build...
    echo   Application: http://localhost:5050
    echo   Health:      http://localhost:5050/api/health
    echo.
    echo تنظیمات: app\appsettings.Development.json
    echo   (راهنما: app-CONNECTION_SETUP.txt)
    echo.
    pushd app
    dotnet HRPerformance.API.dll
    popd
    goto :done
)

echo نسخه آماده یافت نشد — یک بار Build می‌شود...
echo تنظیمات: src\HRPerformance.API\appsettings.Development.json
echo.

if not exist "src\HRPerformance.API\obj\project.assets.json" (
    call scripts\restore-packages.bat
    if errorlevel 1 goto :done
)

echo Building (فقط یک بار)...
dotnet build HRPerformance.sln -c Release -v minimal
if errorlevel 1 (
    echo Build ناموفق بود.
    goto :done
)

echo.
echo   Application: http://localhost:5050
echo   Health:      http://localhost:5050/api/health
echo.
echo برای توقف: Ctrl+C
echo.

dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --launch-profile http --no-build

:done
pause
