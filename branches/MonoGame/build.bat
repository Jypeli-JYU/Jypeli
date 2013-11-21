@echo off
setlocal
rem | Compiles the library

call build_platform WindowsGL x86
if errorlevel 1 goto error

call build_platform Linux x86
if errorlevel 1 goto error

call build_platform WP8 x86
if errorlevel 1 goto error

call build_platform WP8 ARM
if errorlevel 1 goto error

call build_platform Win8 x86
if errorlevel 1 goto error

call build_platform Win8 ARM
if errorlevel 1 goto error

goto end

:error
endlocal
exit /B 1

:end
endlocal
