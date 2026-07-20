@echo off
chcp 65001 >nul
echo ==========================================
echo   توقف IIS — برای کپی فایل در inetpub
echo ==========================================
echo.
echo PowerShell Admin لازم است.
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0iis-stop-for-update.ps1"
echo.
pause
