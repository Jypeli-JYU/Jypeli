@echo off
setlocal
rem | Copies the output files for a specific platform to Compiled directory

rem Globals
set baseDir=..\Compiled
set platform=%1
set arch=AnyCPU

rem Argument check
set argC=0
for %%x in (%*) do Set /A argC+=1

if %argC% NEQ 1 (
  if %argC% NEQ 2 goto argfail
  set arch=%2%
)

rem Directories
set monosrc=MonoGame\MonoGame.Framework\bin\%platform%\%arch%\Release
set outputDir=%baseDir%\%platform%-%arch%

if not exist %baseDir% mkdir %baseDir%
if not exist %outputDir% mkdir %outputDir%

rem Platform specific files

copy %monosrc%\SharpDX.* %outputDir%\
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
