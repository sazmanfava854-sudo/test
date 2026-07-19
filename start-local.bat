@echo off
setlocal EnableExtensions
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
set APP_PORT=
set PORT_FILE=%TEMP%\hr-performance-port.txt

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
    echo Build پروژه API ^(برای به‌روز UI^)...
    dotnet build src\HRPerformance.API\HRPerformance.API.csproj -v minimal
    if errorlevel 1 goto :done
)

echo.
echo انتخاب پورت آزاد...
if exist "%PORT_FILE%" del /f /q "%PORT_FILE%" >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\resolve-app-port.ps1" -OutFile "%PORT_FILE%" >nul 2>&1
if exist "%PORT_FILE%" (
    set /p APP_PORT=<"%PORT_FILE%"
)

if "%APP_PORT%"=="" (
    set APP_PORT=5280
    echo پورت پیش‌فرض: %APP_PORT%
)

set ASPNETCORE_URLS=http://localhost:%APP_PORT%

echo   نسخه: 2.8.7-dev  ^(فیلتر ShamsiDate شمسی^)
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
