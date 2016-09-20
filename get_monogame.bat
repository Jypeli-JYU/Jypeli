@echo off
setlocal

if exist "C:\Program Files\git\bin\sh.exe" goto checkmonogame
echo TortoiseGit not found
echo Please go to http://tortoisegit.tigris.org and install the latest version.
goto error

:checkmonogame
if exist MonoGame\.git goto end
pushd MonoGame
"C:\Program Files\git\bin\sh.exe" module_init.sh
popd

:error
endlocal
exit /B 1

:end
endlocal
