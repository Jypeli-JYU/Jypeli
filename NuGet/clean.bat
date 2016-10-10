@echo off
setlocal

rmdir /S /Q Jypeli.WindowsGL\lib
rmdir /S /Q Jypeli.Linux\lib
rmdir /S /Q Jypeli.Physics2d
rmdir /S /Q Jypeli.SimplePhysics
rmdir /S /Q Jypeli.Physics2d.WindowsGL\lib
rmdir /S /Q Jypeli.SimplePhysics.WindowsGL\lib
rmdir /S /Q Jypeli.Physics2d.Linux\lib
rmdir /S /Q Jypeli.SimplePhysics.Linux\lib

for /R . %%G IN (*.nupkg) do del "%%G"

endlocal
