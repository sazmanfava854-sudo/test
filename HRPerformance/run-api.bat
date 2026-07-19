@echo off
chcp 65001 >nul
cd /d "%~dp0"

set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:5050

if exist "app\HRPerformance.API.dll" (
    echo API: http://localhost:5050
    pushd app
    dotnet HRPerformance.API.dll
    popd
) else (
  cd src\HRPerformance.API
  dotnet run --launch-profile http --no-build 2>nul
  if errorlevel 1 dotnet run --launch-profile http
)

pause
