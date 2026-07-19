@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance - توسعه لوکال
echo ==========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo dotnet یافت نشد! .NET 8 SDK را نصب کنید.
    pause
    exit /b 1
)

set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set ASPNETCORE_ENVIRONMENT=Development

if not exist "src\HRPerformance.API\obj\project.assets.json" (
    echo [اولین بار] Restore پکیج‌های NuGet...
    call scripts\restore-packages.bat
    if errorlevel 1 goto :done
)

set DLL=src\HRPerformance.API\bin\Debug\net8.0\HRPerformance.API.dll
if not exist "%DLL%" (
    echo [اولین بار] Build پروژه API...
    dotnet build src\HRPerformance.API\HRPerformance.API.csproj -v minimal
    if errorlevel 1 goto :done
) else (
    echo اجرا بدون Build مجدد ^(--no-build^)...
)

echo.
echo انتخاب پورت آزاد...
for /f "usebackq delims=" %%P in (`powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\resolve-app-port.ps1"`) do set APP_PORT=%%P
if "%APP_PORT%"=="" (
    echo.
    echo هیچ پورت آزادی یافت نشد.
    goto :done
)

set ASPNETCORE_URLS=http://localhost:%APP_PORT%

echo.
echo   Application: http://localhost:%APP_PORT%
echo   Health:      http://localhost:%APP_PORT%/api/health
echo   Swagger:     http://localhost:%APP_PORT%/swagger
echo.
echo تنظیمات: src\HRPerformance.API\appsettings.Development.json
echo.
echo برای توقف: Ctrl+C
echo.

dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --no-launch-profile --no-build

:done
pause
