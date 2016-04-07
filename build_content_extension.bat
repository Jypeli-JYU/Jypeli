@echo off
setlocal
rem | Compiles the content extensions

rem Globals
set baseDir=Compiled
set extName=%1

rem MSBuild
set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

if exist %msbuild% goto msbuildok
ECHO.
ECHO.
echo MSBuild 14.0 (Visual Studio 2015) required.
ECHO.
ECHO.
goto error

:msbuildok

rem Directories
if not exist %baseDir% mkdir %baseDir%
set outputDir=%baseDir%\ContentExtensions
if not exist %outputDir% mkdir %outputDir%

rem Build

pushd ContentExtensions\%extName%
%msbuild% %extName%.sln /t:Rebuild /p:Configuration=Release
if errorlevel 1 goto error
popd

copy ContentExtensions\%extName%\bin\Release\* %outputDir%
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
