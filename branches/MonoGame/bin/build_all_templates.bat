@echo off
setlocal
call build_project_templates ..\Projektimallit\Windows
call build_project_templates ..\Projektimallit\WindowsGL
call build_project_templates ..\Projektimallit\WindowsUniversal
endlocal
