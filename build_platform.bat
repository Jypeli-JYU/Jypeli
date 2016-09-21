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

rem Protobuild
protobuild -generate %platform%

rem Directories
if not exist %baseDir% mkdir %baseDir%
set outputDir=%baseDir%\%platform%-%arch%
if not exist %outputDir% mkdir %outputDir%

rem Build

if "%platform%"=="WindowsGL" goto copysdl
goto nocopysdl

:copysdl
copy MonoGame\ThirdParty\GamepadConfig\SDL_mixer.dll %outputdir%
copy MonoGame\ThirdParty\GamepadConfig\sdl.dll %outputdir%
copy Jypeli\smpeg.dll %outputdir%
:nocopysdl

if "%platform%"=="Linux" (
  copy MonoGame\ThirdParty\Libs\OpenTK.dll.config %outputDir%
)


%msbuild% Jypeli.%platform%.sln /t:Rebuild /p:Configuration=Release;Platform=%arch2%
if errorlevel 1 goto error

call copy_compiled.bat %platform% %arch%
goto end

:argfail
echo Syntax: %0% (platform) [build architecture]
goto error

:error
endlocal
exit /B 1


:end
endlocal
