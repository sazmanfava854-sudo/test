@echo off
cd /d "%~dp0"
if "%~1"=="" (
  echo Usage: run-sso-loginkey-curl-only.cmd -SecretKey YOUR_SECRET
  exit /b 1
)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0sso-loginkey-curl-only.ps1" %*
exit /b %ERRORLEVEL%
