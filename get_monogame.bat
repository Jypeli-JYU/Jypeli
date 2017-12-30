@echo off
setlocal

if exist "C:\Program Files (x86)\Git\bin\sh.exe" goto checkmonogame
echo Git not found
echo Please go to https://git-scm.com and install the latest version.
goto error

:checkmonogame
if exist MonoGame\.git goto end
pushd MonoGame
"C:\Program Files (x86)\git\bin\sh.exe" module_init.sh
popd

:error
endlocal
exit /B 1

:end
endlocal
