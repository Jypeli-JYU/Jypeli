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

%msbuild% /p:Configuration=Release MonoDevelop.Jypeli.Windows.sln
pushd bin\Windows\Release
%mdtool% setup pack MonoDevelop.Jypeli.Windows.dll
popd
for %%f in (bin\Windows\Release\*.mpack) do move %%f bin\mpack\%%~Nf_Windows.mpack

%msbuild% /p:Configuration=Release MonoDevelop.Jypeli.Linux.v4.sln
pushd bin\Linux\Release-v4
%mdtool% setup pack MonoDevelop.Jypeli.Linux.dll
popd
for %%f in (bin\Linux\Release-v4\*.mpack) do move %%f bin\mpack\%%~Nf_Linux.v4.mpack

%msbuild% /p:Configuration=Release MonoDevelop.Jypeli.Linux.sln
pushd bin\Linux\Release
%mdtool% setup pack MonoDevelop.Jypeli.Linux.dll
popd
for %%f in (bin\Linux\Release\*.mpack) do move %%f bin\mpack\%%~Nf_Linux.mpack

endlocal