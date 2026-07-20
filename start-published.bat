@echo off
setlocal EnableExtensions
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance - اجرای Publish
echo ==========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo dotnet یافت نشد! .NET 8 Runtime/SDK را نصب کنید.
    pause
    exit /b 1
)

if not exist "app\HRPerformance.API.dll" (
    echo پوشه app\ یافت نشد — از ZIP کامل v2.9.5-dev-local استفاده کنید.
    pause
    exit /b 1
)

if not exist "app\wwwroot\index.html" (
    echo app\wwwroot\index.html یافت نشد — ZIP ناقص است.
    pause
    exit /b 1
)

set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set ASPNETCORE_ENVIRONMENT=Development
set APP_PORT=
set PORT_FILE=%TEMP%\hr-performance-port.txt

echo انتخاب پورت آزاد...
if exist "%PORT_FILE%" del /f /q "%PORT_FILE%" >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\resolve-app-port.ps1" -OutFile "%PORT_FILE%" >nul 2>&1
if exist "%PORT_FILE%" set /p APP_PORT=<"%PORT_FILE%"
if "%APP_PORT%"=="" set APP_PORT=5280

set ASPNETCORE_URLS=http://localhost:%APP_PORT%

echo   Application: http://localhost:%APP_PORT%
echo   wwwroot:     app\wwwroot\
echo   تنظیمات:    app\appsettings.Development.json
echo.
echo برای توقف: Ctrl+C
echo.

cd app
dotnet HRPerformance.API.dll

pause
