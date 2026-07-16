@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance - DEMO MODE
echo   بدون SQL Server
echo ==========================================
echo.
echo   Application: http://localhost:5050
echo   Login:       admin / Admin@123
echo.
echo برای توقف Ctrl+C
echo.

dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --launch-profile demo
pause
