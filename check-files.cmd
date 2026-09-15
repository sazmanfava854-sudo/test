@echo off
setlocal
pushd "%~dp0"
echo RuleTrace folder check
echo Folder: %CD%
echo.
echo --- build scripts ---
dir /b build.cmd build.bat build.ps1 build.vbs install.cmd 2>nul
echo.
if exist "build.cmd" (echo [OK] build.cmd exists) else (echo [MISSING] build.cmd)
if exist "RuleTrace.sln" (echo [OK] RuleTrace.sln exists) else (echo [MISSING] RuleTrace.sln)
echo.
echo If build.cmd exists but double-click fails, use build.vbs or install.cmd
pause
popd
