@echo off
setlocal EnableExtensions
cd /d "%~dp0"
if errorlevel 1 (
  echo ERROR: cannot cd to "%~dp0"
  pause
  exit /b 1
)
if not exist "%CD%\bin\RuleTrace.exe" (
  echo bin\RuleTrace.exe not found - building first...
  call "%CD%\build.cmd"
  exit /b
)
start "" "%CD%\bin\RuleTrace.exe"
