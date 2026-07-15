@echo off
setlocal enabledelayedexpansion

echo ============================================
echo  HR Performance - NuGet Package Restore
echo ============================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet SDK not found. Install .NET 8 SDK:
    echo         https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

echo Using: 
dotnet --version
echo.

set RETRIES=3
set ATTEMPT=1

:restore_loop
echo [Attempt %ATTEMPT%/%RETRIES%] Restoring packages...
dotnet restore HRPerformance.sln --disable-parallel --verbosity minimal
if %errorlevel% equ 0 goto restore_ok

echo.
echo [WARN] Restore failed. Common fixes:
echo   1. Check internet / VPN / corporate proxy
echo   2. Run: dotnet nuget locals all --clear
echo   3. Temporarily disable antivirus SSL inspection
echo   4. Set proxy: set HTTPS_PROXY=http://proxy:port
echo.

set /a ATTEMPT+=1
if %ATTEMPT% leq %RETRIES% (
    timeout /t 5 /nobreak >nul
    goto restore_loop
)

echo [ERROR] Restore failed after %RETRIES% attempts.
exit /b 1

:restore_ok
echo.
echo [OK] Packages restored successfully.
echo      Reopen the solution in Cursor/VS Code for faster IntelliSense.
exit /b 0
