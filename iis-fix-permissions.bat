@echo off
chcp 65001 >nul
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "%~dp0iis-fix-permissions.ps1" -SitePath "%~dp0"
pause
