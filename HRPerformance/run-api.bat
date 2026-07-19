@echo off
chcp 65001 >nul
cd /d "%~dp0\src\HRPerformance.API"

set ASPNETCORE_ENVIRONMENT=Development
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

if not exist "bin\Debug\net8.0\HRPerformance.API.dll" (
    echo Build لازم است — از ریشه پروژه start-local.bat را اجرا کنید.
    pause
    exit /b 1
)

dotnet run --launch-profile http --no-build
pause
