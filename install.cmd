@echo off
title RuleTrace install to C:\ruletrace
echo.
echo RuleTrace: copy to C:\ruletrace and build (fixes Downloads paths with parentheses)
echo.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1"
if errorlevel 1 pause
