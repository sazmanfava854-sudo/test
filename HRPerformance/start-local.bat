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

REM اولین بار فقط Restore (ممکن است چند دقیقه طول بکشد)
if not exist "src\HRPerformance.API\obj\project.assets.json" (
    echo [اولین بار] Restore پکیج‌های NuGet...
    call scripts\restore-packages.bat
    if errorlevel 1 goto :done
)

REM Build فقط اگر DLL وجود ندارد یا سورس جدیدتر است
set DLL=src\HRPerformance.API\bin\Debug\net8.0\HRPerformance.API.dll
if not exist "%DLL%" (
    echo [اولین بار] Build پروژه API...
    dotnet build src\HRPerformance.API\HRPerformance.API.csproj -v minimal
    if errorlevel 1 goto :done
) else (
    echo اجرا بدون Build مجدد ^(--no-build^)...
)

echo.
echo بررسی پورت 5050...
call scripts\free-port-5050.bat
if errorlevel 1 (
    echo.
    echo پورت 5050 آزاد نشد. احتمالاً یک نمونه قبلی HR Performance هنوز در حال اجراست.
    echo   netstat -ano ^| findstr :5050
    echo   taskkill /PID ^<pid^> /F
    goto :done
)

echo.
echo   Application: http://localhost:5050
echo   Health:      http://localhost:5050/api/health
echo   Swagger:     http://localhost:5050/swagger
echo.
echo تنظیمات: src\HRPerformance.API\appsettings.Development.json
echo.
echo برای توقف: Ctrl+C
echo.

dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --launch-profile http --no-build

:done
pause
