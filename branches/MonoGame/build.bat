@echo off
setlocal
rem | Compiles the library

call get_monogame.bat
if errorlevel 1 goto error

call build_content_extension TextFileContentExtension
if errorlevel 1 goto error

call build_platform Windows
if errorlevel 1 goto error

call build_platform WindowsGL
if errorlevel 1 goto error

call build_platform Linux
if errorlevel 1 goto error

call build_platform Windows8
if errorlevel 1 goto error

call build_platform WindowsPhone81
if errorlevel 1 goto error


goto end

:error
endlocal
exit /B 1

:end
endlocal
