@echo off
setlocal

if exist MonoGame\.git goto end

if exist "%ProgramFiles%\Git\bin\sh.exe" goto getmonogame32
if exist "%ProgramFiles(x86)%\Git\bin\sh.exe" goto getmonogame6432
echo Git not found
echo Please go to https://git-scm.com and install the latest version.
goto error

:getmonogame32
pushd MonoGame
"%ProgramFiles%\git\bin\sh.exe" module_init.sh
popd

:getmonogame6432
pushd MonoGame
"%ProgramFiles(x86)%\git\bin\sh.exe" module_init.sh
popd

:error
endlocal
exit /B 1

:end
endlocal
