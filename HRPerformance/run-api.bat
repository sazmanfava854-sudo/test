@echo off
chcp 65001 >nul
cd /d "%~dp0src\HRPerformance.API"

echo ==========================================
echo   HR Performance API
echo ==========================================
echo.
echo آدرس: http://localhost:5050
echo تست:  http://localhost:5050/api/health
echo.
echo این پنجره را باز نگه دارید. توقف: Ctrl+C
echo.

dotnet run --launch-profile http
echo.
echo برنامه بسته شد. کد خروج: %ERRORLEVEL%
pause
