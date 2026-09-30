@echo off
setlocal
cd /d "%~dp0"
if "%~1"=="" (
  echo Usage:
  echo   run-test-mashhad-sso-loginkey.cmd -SecretKey YOUR_32_CHAR_SECRET
  echo   run-test-mashhad-sso-loginkey.cmd -SecretKey YOUR_SECRET -ApiName FinancialAssistant -ClientId 53db42619cf3C333b13a18D34fbd9111
  echo Optional: -PrintOnly
  exit /b 1
)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0test-mashhad-sso-loginkey.ps1" %*
exit /b %ERRORLEVEL%
