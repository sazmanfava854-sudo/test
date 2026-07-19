@echo off
setlocal
cd /d "%~dp0"

where powershell >nul 2>&1
if %errorlevel% neq 0 (
    echo PowerShell یافت نشد.
    exit /b 1
)

for /f "usebackq delims=" %%P in (`powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0resolve-app-port.ps1"`) do set APP_PORT=%%P
if "%APP_PORT%"=="" exit /b 1
echo %APP_PORT%
exit /b 0
