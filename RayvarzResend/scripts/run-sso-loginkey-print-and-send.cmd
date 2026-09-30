@echo off
setlocal
cd /d "%~dp0"
echo SSO print and send (ExecutionPolicy Bypass for this process only)...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0sso-loginkey-print-and-send.ps1" %*
exit /b %ERRORLEVEL%
