@echo off
mkdir assemblies\Windows
copy ..\..\Compiled\Windows-AnyCPU\*.* assemblies\Windows

mkdir assemblies\WindowsGL
copy ..\..\Compiled\WindowsGL-AnyCPU\*.* assemblies\WindowsGL

mkdir assemblies\Linux
copy ..\..\Compiled\Linux-AnyCPU\*.* assemblies\Linux
