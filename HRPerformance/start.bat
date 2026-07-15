@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance System
echo ==========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo dotnet یافت نشد! ابتدا .NET 9 SDK را نصب کنید.
    echo https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)

echo در حال اجرا...
echo   Application: http://localhost:5000
echo   Swagger:     http://localhost:5000/swagger
echo.
echo برای توقف Ctrl+C
echo.

dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --launch-profile http
pause
