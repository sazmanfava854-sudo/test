@echo off
chcp 65001 >nul
cd /d "%~dp0..\frontend\hr-performance-web"

echo [UI] npm install...
call npm install
if errorlevel 1 exit /b 1

echo [UI] npm run build...
call npm run build
if errorlevel 1 exit /b 1

echo [UI] کپی به wwwroot...
xcopy /E /Y /I dist\* "..\..\src\HRPerformance.API\wwwroot\" >nul

echo [API] dotnet build...
dotnet build "..\..\src\HRPerformance.API\HRPerformance.API.csproj" -v minimal
echo UI build OK.
exit /b 0
