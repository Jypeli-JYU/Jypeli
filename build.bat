@echo off
setlocal
rem | Compiles the library

call build_platform Windows
if errorlevel 1 goto error

call build_platform WindowsGL
if errorlevel 1 goto error

call build_platform Linux
if errorlevel 1 goto error

call build_platform Windows8
if errorlevel 1 goto error

protobuild -generate WindowsPhone81
echo.
echo WP8.1 must be built in Visual Studio!
echo Build it, then press any key to continue...
pause > NUL:
call copy_compiled WindowsPhone81

goto end

:error
endlocal
exit /B 1

:end
endlocal
