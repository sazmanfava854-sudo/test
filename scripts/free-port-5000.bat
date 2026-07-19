@echo off
setlocal
cd /d "%~dp0"

set PORT=5000
if not "%~1"=="" set PORT=%~1

where powershell >nul 2>&1
if %errorlevel% equ 0 (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0free-port.ps1" -Port %PORT%
    exit /b %errorlevel%
)

echo PowerShell یافت نشد.
exit /b 1
