@echo off
chcp 65001 >nul
cd /d "%~dp0"

if not exist "app\HRPerformance.API.dll" (
    echo پوشه app یافت نشد. start-local.bat را از ریشه پروژه اجرا کنید.
    pause
    exit /b 1
)

set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:5050

pushd app
dotnet HRPerformance.API.dll
popd
pause
