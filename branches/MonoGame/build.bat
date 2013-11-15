@echo off
setlocal
rem | Compiles the library

set outputDir=Compiled
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


set msbuild=%FrameworkDir%\v4.0.30319\MSBuild.exe

%msbuild% Jypeli.proj
if errorlevel 1 goto error

if not exist %outputDir% mkdir %outputDir%

call copy_dlls Jypeli\bin\WindowsGL\Release %outputDir%\WindowsGL
call copy_dlls Jypeli\bin\Linux\Release %outputDir%\Linux
call copy_dlls Jypeli\bin\WindowsPhone\x86\Release %outputDir%\WP8-x86
call copy_dlls Jypeli\bin\WindowsPhone\ARM\Release %outputDir%\WP8-ARM
call copy_dlls Jypeli\bin\Windows8\x86\Release %outputDir%\Win8-x86
call copy_dlls Jypeli\bin\Windows8\ARM\Release %outputDir%\Win8-ARM

goto end


:error
endlocal
exit /B 1


:end
endlocal
