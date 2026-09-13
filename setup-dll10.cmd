@echo off
set "SRC=%~1"
if "%SRC%"=="" set "SRC=C:\Users\sadathoseini-sh\Desktop\dll10"

if not exist "%SRC%" (
  echo ERROR: source folder not found: %SRC%
  exit /b 1
)

if not exist "c:\dll10" mkdir "c:\dll10"
xcopy /Y /E "%SRC%\*" "c:\dll10\"
echo.
echo Done. BIZ.SC.DLL should be at c:\dll10\BIZ.SC.DLL
dir "c:\dll10\BIZ.SC.*" 2>nul
