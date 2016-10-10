@echo off
REM Usage: call nuget_subdir <subdir name> <nuget command>

setlocal
pushd %1
..\nuget.exe %2
popd
endlocal
