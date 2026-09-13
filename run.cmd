@echo off
cd /d "%~dp0"
if not exist "bin\RuleTrace.exe" (
  echo bin\RuleTrace.exe not found - building first...
  call build.cmd
  exit /b
)
start "" "bin\RuleTrace.exe"
