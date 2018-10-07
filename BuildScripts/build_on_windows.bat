@echo off
rem Compiles Jypeli for Windows
rem Author: Rami Pasanen https://github.com/Rampastring

call find_msbuild

if exist "%msbuild%" goto msbuildok
ECHO.
ECHO.
echo Visual Studio 2017 required.
ECHO.
ECHO.
goto error

:msbuildok

ECHO.
echo Compiling %configuration% %platform%
ECHO.

"%msbuild%" ..\Jypeli.Windows.sln /t:Rebuild /p:Platform="Any CPU" /p:Configuration=Release
if errorlevel 1 goto error

"%msbuild%" ..\Jypeli.Android.sln /t:Rebuild /p:Platform="Any CPU" /p:Configuration=Release
if errorlevel 1 goto error

"%msbuild%" ..\MSBuildExtension\MGCBTask\MGCBTask.sln /t:Rebuild /p:Platform="Any CPU" /p:Configuration=Debug
if errorlevel 1 goto error

ECHO.
echo Compiled succesfully.
ECHO.

call copy_compiled Windows
call copy_compiled Android

call build_content_extension TextFileContentExtension

goto end

:error
endlocal
exit /B 1

:end
endlocal