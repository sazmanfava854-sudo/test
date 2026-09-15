@echo off
setlocal EnableExtensions
title RuleTrace build

REM pushd handles paths with parentheses better than cd /d (GitHub ZIP: ...-88fc(44)\...)
pushd "%~dp0" 2>nul
if errorlevel 1 (
  echo.
  echo ERROR: cannot open project folder:
  echo   %~dp0
  echo.
  echo If the path contains (44) or similar, Windows often breaks .cmd files.
  echo Fix: double-click install.cmd  OR  copy the folder to C:\ruletrace
  echo.
  pause
  exit /b 1
)

echo ============================================
echo  RuleTrace build  v21c-cross-class
echo  Folder: %CD%
echo ============================================

if not exist "RuleTrace.sln" (
  echo.
  echo ERROR: RuleTrace.sln not found in:
  echo   %CD%
  echo.
  echo GitHub ZIP creates a nested folder. Open the INNER folder that contains
  echo RuleTrace.sln and build.cmd, or run install.cmd to copy to C:\ruletrace
  echo.
  pause
  popd
  exit /b 1
)

set "MSBUILD="

REM 1) vswhere (VS 2017+ / Build Tools)
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist "%VSWHERE%" (
  for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"
)

REM 2) well-known VS paths (quoted — Program Files has spaces)
if not defined MSBUILD for %%p in (
  "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) do if not defined MSBUILD if exist "%%~p" set "MSBUILD=%%~p"

REM 3) msbuild on PATH
if not defined MSBUILD for /f "delims=" %%i in ('where msbuild 2^>nul') do if not defined MSBUILD set "MSBUILD=%%i"

REM 4) dotnet msbuild
if not defined MSBUILD (
  where dotnet >nul 2>nul
  if not errorlevel 1 set "MSBUILD=dotnet msbuild"
)

if not defined MSBUILD (
  echo.
  echo ERROR: MSBuild not found.
  echo Install "Visual Studio Build Tools" with .NET desktop build tools:
  echo   https://aka.ms/vs/17/release/vs_BuildTools.exe
  pause
  popd
  exit /b 1
)

echo MSBuild: %MSBUILD%
echo.

if exist "%MSBUILD%" (
  "%MSBUILD%" "RuleTrace.sln" /nologo /v:m /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
) else (
  %MSBUILD% "RuleTrace.sln" /nologo /v:m /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
)
if errorlevel 1 (
  echo.
  echo BUILD FAILED
  pause
  popd
  exit /b 1
)

if not exist "bin\RuleTrace.exe" (
  echo.
  echo ERROR: build reported OK but bin\RuleTrace.exe is missing
  pause
  popd
  exit /b 1
)

echo.
echo ============================================
echo  OK  -^>  %CD%\bin\RuleTrace.exe
echo  Window title must be: RuleTrace v21c-cross-class
echo ============================================
echo Starting RuleTrace...
start "" "%CD%\bin\RuleTrace.exe"
popd
