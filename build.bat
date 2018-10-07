@echo off
setlocal
rem | Compiles the library

call get_monogame.bat
if errorlevel 1 goto error

call build_platform Windows
if errorlevel 1 goto error

rem call build_platform WindowsGL
if errorlevel 1 goto error

rem call build_platform Linux
if errorlevel 1 goto error

call build_platform Android
if errorlevel 1 goto error

rem WindowsUniversalissa on jotain ongelmaa, pitää katsoa myöhemmin
rem call build_platform WindowsUniversal
rem if errorlevel 1 goto error

rem Windows8 and WP81 ovat kuolleita alustoja
rem call build_platform Windows8
rem if errorlevel 1 goto error

rem call build_platform WindowsPhone81
rem if errorlevel 1 goto error

call build_content_extension TextFileContentExtension
if errorlevel 1 goto error

goto end

:error
endlocal
exit /B 1

:end
endlocal
