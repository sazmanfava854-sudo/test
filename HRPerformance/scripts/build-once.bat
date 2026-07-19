@echo off
chcp 65001 >nul
cd /d "%~dp0\.."

echo ==========================================
echo   HR Performance - Build Once
echo ==========================================
echo.

if not exist "src\HRPerformance.API\obj\project.assets.json" (
    call scripts\restore-packages.bat
    if errorlevel 1 exit /b 1
)

echo Building solution...
dotnet build HRPerformance.sln -c Release -v minimal
if errorlevel 1 (
    echo Build failed.
    pause
    exit /b 1
)

echo.
echo [OK] Build complete. Run: start-local.bat
pause
