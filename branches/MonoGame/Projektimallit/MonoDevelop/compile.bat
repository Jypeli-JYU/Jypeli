@echo off

if not exist "C:\Program Files (x86)\Xamarin Studio\bin\mdtool.exe" goto error

mkdir assemblies
mkdir assemblies\WindowsGL
copy ..\..\Compiled\WindowsGL-x86\*.* assemblies\WindowsGL

echo Running MDTool
"C:\Program Files (x86)\Xamarin Studio\bin\mdtool.exe" setup pack Jypeli.Windows.addin.xml
goto end

:error
echo MDTool not found! Do you have Xamarin Studio installed at C:\Program Files (x86)?

:end
