@echo off
chcp 65001 >nul
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "%~dp0iis-bind-site-5050.ps1" -PhysicalPath "%~dp0"
pause
