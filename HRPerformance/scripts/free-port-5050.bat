@echo off
setlocal
cd /d "%~dp0"

where powershell >nul 2>&1
if %errorlevel% equ 0 (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0free-port-5050.ps1"
    exit /b %errorlevel%
)

setlocal enabledelayedexpansion
set PORT=5050
set FOUND=0

for /f "tokens=5" %%P in ('netstat -ano ^| findstr ":%PORT%" ^| findstr "LISTENING"') do (
    set FOUND=1
    echo [پورت %PORT%] در حال استفاده توسط PID %%P - در حال آزادسازی...
    taskkill /PID %%P /F >nul 2>&1
    if errorlevel 1 (
        echo خطا: امکان توقف PID %%P نیست.
        exit /b 1
    )
    echo پروسه %%P متوقف شد.
)

if !FOUND! equ 1 (
    timeout /t 1 /nobreak >nul
)

exit /b 0
