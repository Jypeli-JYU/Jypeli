call find_msbuild

if exist "%msbuild%" goto msbuildok
ECHO.
ECHO.
echo Visual Studio 2019 required.
ECHO.
ECHO.
goto error

:msbuildok

"%msbuild%" ..\Jypeli.Core.sln /t:Rebuild /p:Platform="Any CPU" /p:Configuration=Release -t:pack