@echo off
REM Launcher when Windows refuses to run build.cmd from Downloads paths with (44) etc.
call "%~dp0build.cmd" %*
