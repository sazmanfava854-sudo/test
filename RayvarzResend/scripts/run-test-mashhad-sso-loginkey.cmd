@echo off
setlocal
cd /d "%~dp0"
echo Running SSO loginKey test (ExecutionPolicy Bypass for this process only)...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0test-mashhad-sso-loginkey.ps1" %*
exit /b %ERRORLEVEL%
