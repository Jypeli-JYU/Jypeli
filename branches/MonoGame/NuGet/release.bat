@echo off
setlocal

call clean.bat
call build.bat

for /R . %%G IN (*.nupkg) do nuget push "%%G" -Source https://www.nuget.org/api/v2/package

endlocal
