@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title RuleTrace build

echo ============================================
echo  RuleTrace build  v21-no-vb-rewrite
echo  (title must NOT say v20e-phase2-sanitize)
echo ============================================

set "MSBUILD="

REM 1) vswhere (VS 2017+ / Build Tools)
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist "%VSWHERE%" (
  for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"
)

REM 2) well-known VS paths
if not defined MSBUILD for %%p in (
  "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
  "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) do if not defined MSBUILD if exist %%p set "MSBUILD=%%~p"

REM 3) dotnet msbuild (if .NET SDK is installed)
if not defined MSBUILD where dotnet >nul 2>nul && set "MSBUILD=dotnet msbuild"

if not defined MSBUILD (
  echo.
  echo ERROR: MSBuild not found. Install "Visual Studio Build Tools" ^(.NET desktop build tools^).
  echo        https://aka.ms/vs/17/release/vs_BuildTools.exe
  pause
  exit /b 1
)

echo MSBuild: %MSBUILD%
echo.

REM Quote path when it contains spaces (e.g. C:\Program Files\...)
if exist "%MSBUILD%" (
  "%MSBUILD%" RuleTrace.sln /nologo /v:m /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
) else (
  %MSBUILD% RuleTrace.sln /nologo /v:m /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
)
if errorlevel 1 (
  echo.
  echo BUILD FAILED
  pause
  exit /b 1
)

echo.
echo ============================================
echo  OK  ->  %~dp0bin\RuleTrace.exe
echo  Window title must be: RuleTrace v21-no-vb-rewrite
echo ============================================
echo Starting RuleTrace...
start "" "%~dp0bin\RuleTrace.exe"
