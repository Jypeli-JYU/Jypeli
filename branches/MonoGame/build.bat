@echo off
setlocal
rem | Compiles the library

call build_platform WindowsGL
if errorlevel 1 goto error

call build_platform Linux
if errorlevel 1 goto error

call build_platform Windows8
if errorlevel 1 goto error

rem call build_platform WindowsPhone81
rem if errorlevel 1 goto error

call build_platform WindowsPhone x86
if errorlevel 1 goto error

call build_platform WindowsPhone ARM
if errorlevel 1 goto error

goto end

:error
endlocal
exit /B 1

:end
endlocal
