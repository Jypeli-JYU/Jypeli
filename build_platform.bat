@echo off
setlocal
rem | Compiles the library for a specific platform

rem Globals
set baseDir=Compiled
set platform=%1
set arch=AnyCPU
set arch2="Any CPU"
set configuration=Release

rem Argument check
set argC=0
for %%x in (%*) do Set /A argC+=1

if %argC% NEQ 1 (
  if %argC% NEQ 2 goto argfail
  set arch=%2%
  set arch2=%2%
)

rem MSBuild
call find_msbuild

if exist "%msbuild%" goto msbuildok
if exist 
ECHO.
ECHO.
echo MSBuild 15.0 (Visual Studio 2017) required.
ECHO.
ECHO.
goto error

:msbuildok

rem Protobuild
rem HUOM! Protobuild täytyy ajaa kerran aina kun lähdekoodi on haettu SVN:stä!
rem Jostain syystä protobuildin generoimat Android-projektit kuitenkin käyttävät API leveliä 17.
rem Tämä on liian vanha, joten protobuild pitää kommentoida pois ensimmäisen käännösyrityksen
rem jälkeen.
rem protobuild -generate %platform%

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


"%msbuild%" Jypeli.%platform%.sln /t:Rebuild /p:Platform=%arch2% /p:Configuration=%configuration%
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
