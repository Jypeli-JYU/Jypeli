@echo off
setlocal
set argC=0
for %%x in (%*) do Set /A argC+=1

if %argC% NEQ 2 goto argfail

set copy_if_newer=xcopy /D /S /Q /I /Y

if not exist %2 mkdir %2
if not exist %2\Jypeli mkdir %2\Jypeli

%copy_if_newer% %1\Jypeli.dll %2\Jypeli
%copy_if_newer% %1\Jypeli.xml %2\Jypeli
%copy_if_newer% %1\Jypeli.SimplePhysics.dll %2\Jypeli
%copy_if_newer% %1\Jypeli.SimplePhysics.xml %2\Jypeli
%copy_if_newer% %1\Jypeli.Physics2d.dll %2\Jypeli
%copy_if_newer% %1\Jypeli.Physics2d.xml %2\Jypeli
%copy_if_newer% %1\Jypeli.MonoGame.Framework.dll %2\Jypeli
%copy_if_newer% %1\Lidgren.Network.dll %2\Jypeli
%copy_if_newer% %1\Tao.Sdl.dll %2\Jypeli

goto end


:argfail
echo Syntax: %0% (srcdir) (destdir)
goto end

:end

endlocal