@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "DllPath=%~1"
if "%DllPath%"=="" set "DllPath=C:\Users\sadathoseini-sh\Desktop\dll10"

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
  echo ERROR: vswhere not found. Install Visual Studio or Build Tools.
  exit /b 1
)

set "MSBUILD="
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"

if not defined MSBUILD (
  echo ERROR: MSBuild not found.
  exit /b 1
)

echo MSBuild : %MSBUILD%
echo DllPath : %DllPath%
echo.

"%MSBUILD%" RuleTrace.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DllPath="%DllPath%"
if errorlevel 1 exit /b 1

echo.
echo Output: %~dp0bin\RuleTrace.exe
echo Run:    cd bin
echo         RuleTrace.exe --help
