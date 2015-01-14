@echo off
setlocal

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

set mdtool="C:\Program Files (x86)\Xamarin Studio\bin\mdtool.exe"
set msbuild=%FrameworkDir%\v4.0.30319\MSBuild.exe

call copy_assemblies.bat
mkdir bin\mpack
mkdir bin\mpack\MonoDevelop-4.0
mkdir bin\mpack\MonoDevelop-4.0\Linux
mkdir bin\mpack\MonoDevelop-5.0\Linux
mkdir bin\mpack\MonoDevelop-5.0\Windows

del /s /f /q obj
%msbuild% /p:Configuration=Release MonoDevelop.Jypeli.Windows.sln
pushd bin\Windows\Release
%mdtool% setup pack MonoDevelop.Jypeli.Windows.dll
popd
move bin\Windows\Release\*.mpack bin\mpack\MonoDevelop-5.0\Windows\

del /s /f /q obj
%msbuild% /p:Configuration=Release MonoDevelop.Jypeli.Linux.v4.sln
pushd bin\Linux\Release-v4
%mdtool% setup pack MonoDevelop.Jypeli.Linux.dll
popd
move bin\Linux\Release-v4\*.mpack bin\mpack\MonoDevelop-4.0\Linux\

del /s /f /q obj
%msbuild% /p:Configuration=Release MonoDevelop.Jypeli.Linux.sln
pushd bin\Linux\Release
%mdtool% setup pack MonoDevelop.Jypeli.Linux.dll
popd
move bin\Linux\Release\*.mpack bin\mpack\MonoDevelop-5.0\Linux\
del /s /f /q obj

%mdtool% setup rep-build bin\mpack\MonoDevelop-4.0\Linux
%mdtool% setup rep-build bin\mpack\MonoDevelop-5.0\Linux
%mdtool% setup rep-build bin\mpack\MonoDevelop-5.0\Windows

del bin\mpack\MonoDevelop-4.0\Linux\index.html
del bin\mpack\MonoDevelop-5.0\Linux\index.html
del bin\mpack\MonoDevelop-5.0\Windows\index.html
del bin\mpack\MonoDevelop-5.0\Mac\index.html

endlocal