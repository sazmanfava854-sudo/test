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

echo تنظیمات: src\HRPerformance.API\appsettings.Development.json
echo   ConnectionStrings:DefaultConnection
echo   HrIntegration:Password
echo.
echo   Application: http://localhost:5050
echo   Swagger:     http://localhost:5050/swagger
echo   Health:      http://localhost:5050/api/health
echo.
echo برای توقف: Ctrl+C
echo.

dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --launch-profile http
pause
