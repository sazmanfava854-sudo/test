@echo off
chcp 65001 >nul
title HR Performance - Demo
cd /d "%~dp0"

echo.
echo  ============================================
echo    HR Performance - DEMO (Publish)
echo    بدون SQL Server
echo  ============================================
echo.
echo    1. Extract شده باشد
echo    2. .NET 8 SDK نصب باشد
echo.
echo    Application : http://localhost:5050
echo    Login       : admin
echo    Password    : Admin@123
echo.
echo  ============================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] dotnet یافت نشد.
    echo نصب: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --launch-profile demo
pause
