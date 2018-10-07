@echo off
setlocal
rem | Compiles the content extensions

rem Globals
set baseDir=..\Compiled
set extName=%1

rem MSBuild
call find_msbuild

if exist "%msbuild%" goto msbuildok
ECHO.
ECHO.
echo MSBuild 15.0 (Visual Studio 2017) required.
ECHO.
ECHO.
goto error

:msbuildok

rem Directories
if not exist %baseDir% mkdir %baseDir%
set outputDir=%baseDir%\ContentExtensions
if not exist %outputDir% mkdir %outputDir%

rem Build

pushd ..\ContentExtensions\%extName%

if errorlevel 1 goto error
popd

"%msbuild%" ..\ContentExtensions\%extName%\%extName%.sln /t:Rebuild /p:Platform="Any CPU" /p:Configuration=Release

copy ..\ContentExtensions\%extName%\bin\Release\* %outputDir%
goto end

:argfail
echo Syntax: %0% (extension name)
goto error

:error
popd
endlocal
exit /B 1


:end
endlocal
