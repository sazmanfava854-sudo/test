@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ==========================================
echo   HR Performance - Build Once (Dev)
echo ==========================================
echo.

if not exist "src\HRPerformance.API\obj\project.assets.json" (
    call scripts\restore-packages.bat
    if errorlevel 1 exit /b 1
)

echo Building API project only...
dotnet build src\HRPerformance.API\HRPerformance.API.csproj -v minimal
if errorlevel 1 (
    echo Build failed.
    pause
    exit /b 1
)

echo.
echo [OK] آماده اجرا: start-local.bat
pause
