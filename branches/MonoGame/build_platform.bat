@echo off
setlocal
rem | Compiles the library for a specific platform

rem Globals
set baseDir=Compiled
set platform=%1
set arch=AnyCPU
set arch2="Any CPU"

rem Argument check
set argC=0
for %%x in (%*) do Set /A argC+=1

if %argC% NEQ 1 (
  if %argC% NEQ 2 goto argfail
  set arch=%2%
  set arch2=%2%
)

rem Framework dir
set DefaultFrameworkDir=%WINDIR%\Microsoft.NET\Framework

if not defined FrameworkDir (
   set FrameworkDir=%DefaultFrameworkDir%
)

echo %FrameworkDir%
if not exist %FrameworkDir%\v4.0.30319 (
  ECHO.
  ECHO.
  echo .NET Framework 4.0 required.
  ECHO.
  ECHO.
  goto error
)

rem Protobuild
if not exist "Jypeli.%platform%.sln" (
  protobuild /generate %platform%
)

rem Directories
if not exist %baseDir% mkdir %baseDir%
set outputDir=%baseDir%\%platform%-%arch%
if not exist %outputDir% mkdir %outputDir%

rem Build
set msbuild=%FrameworkDir%\v4.0.30319\MSBuild.exe

if "%platform%"=="Windows" goto copywin
if "%platform%"=="WindowsGL" goto copywin
goto nocopywin

:copywin
copy MonoGame\ThirdParty\GamepadConfig\SDL_mixer.dll %outputdir%
:nocopywin

if "%platform%"=="Linux" (
  copy MonoGame\ThirdParty\Libs\OpenTK.dll.config %outputDir%
)


%msbuild% Jypeli.%platform%.sln /t:Rebuild /p:Configuration=Release;Platform=%arch2%
if errorlevel 1 goto error

copy Jypeli\bin\%platform%\%arch%\Release\*.dll %outputDir%\
copy Jypeli\bin\%platform%\%arch%\Release\*.xml %outputDir%\
copy Jypeli\bin\%platform%\%arch%\Release\*.config %outputDir%\
copy SimplePhysics\bin\%platform%\%arch%\Release\* %outputDir%\
copy Physics2d\bin\%platform%\%arch%\Release\* %outputDir%\

goto end

:argfail
echo Syntax: %0% (platform) [build architecture]
goto error

:error
endlocal
exit /B 1


:end
endlocal
