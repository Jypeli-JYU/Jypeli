@echo off

call build
cd bin
call build_all_templates
call create_installer.bat ..\installer\jypeli.nsi
cd ..

