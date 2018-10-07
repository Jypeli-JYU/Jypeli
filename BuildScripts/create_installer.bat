@echo off

setlocal

if %1.==. goto paramfail

set nsis="%ProgramFiles%\NSIS\makensisw.exe"
set nsis6432="%ProgramFiles(x86)%\NSIS\makensisw.exe"

if exist %nsis% goto makensis
if exist %nsis6432% goto makensis6432
echo(
echo Nullsoft Scriptable Install System not found.
echo Please install it and retry.
echo(
goto end

:makensis
%nsis% %1
goto end

:makensis6432
%nsis6432% %1
goto end

:paramfail
echo Syntax: %0% (path to install script)

:end
endlocal
