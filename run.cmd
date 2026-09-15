@echo off
pushd "%~dp0" 2>nul
if errorlevel 1 (
  echo ERROR: cannot open "%~dp0"
  echo Try install.cmd to copy to C:\ruletrace
  pause
  exit /b 1
)
if not exist "bin\RuleTrace.exe" (
  echo bin\RuleTrace.exe not found - building first...
  call "%~dp0build.cmd"
  popd
  exit /b
)
start "" "%CD%\bin\RuleTrace.exe"
popd
