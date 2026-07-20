@echo off
chcp 65001 >nul
echo ==========================================
echo   تست اتصال SQL (Windows Auth)
echo ==========================================
echo.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0test-sql-connection.ps1"
echo.
pause
