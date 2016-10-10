@echo off
setlocal

mkdir Jypeli.WindowsGL\lib\4.5
mkdir Jypeli.Linux\lib\4.5
mkdir Jypeli.Physics2d.WindowsGL\lib\4.5
mkdir Jypeli.SimplePhysics.WindowsGL\lib\4.5
mkdir Jypeli.Physics2d.Linux\lib\4.5
mkdir Jypeli.SimplePhysics.Linux\lib\4.5

copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.dll Jypeli.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\MonoGame.Framework.dll Jypeli.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\OpenTK.dll Jypeli.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\Tao.Sdl.dll Jypeli.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\NVorbis.dll Jypeli.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\SDL.dll Jypeli.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\SDL_mixer.dll Jypeli.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\smpeg.dll Jypeli.WindowsGL\lib\4.5

copy ..\Compiled\Linux-AnyCPU\Jypeli.dll Jypeli.Linux\lib\4.5
copy ..\Compiled\Linux-AnyCPU\MonoGame.Framework.dll Jypeli.Linux\lib\4.5
copy ..\Compiled\Linux-AnyCPU\OpenTK.dll Jypeli.Linux\lib\4.5
copy ..\Compiled\Linux-AnyCPU\OpenTK.dll.config Jypeli.Linux\lib\4.5
copy ..\Compiled\Linux-AnyCPU\Tao.Sdl.dll Jypeli.Linux\lib\4.5
copy ..\Compiled\Linux-AnyCPU\Tao.Sdl.dll.config Jypeli.Linux\lib\4.5
copy ..\Compiled\Linux-AnyCPU\NVorbis.dll Jypeli.Linux\lib\4.5

copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.dll Jypeli.Physics2d.WindowsGL\lib\4.5
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.dll Jypeli.SimplePhysics.WindowsGL\lib\4.5

copy ..\Compiled\Linux-AnyCPU\Jypeli.Physics2d.dll Jypeli.Physics2d.Linux\lib\4.5
copy ..\Compiled\Linux-AnyCPU\Jypeli.SimplePhysics.dll Jypeli.SimplePhysics.Linux\lib\4.5

call nuget_subdir Jypeli.WindowsGL pack
call nuget_subdir Jypeli.Linux pack
call nuget_subdir Jypeli.Physics2d.WindowsGL pack
call nuget_subdir Jypeli.Physics2d.Linux pack
call nuget_subdir Jypeli.SimplePhysics.WindowsGL pack
call nuget_subdir Jypeli.SimplePhysics.Linux pack

endlocal
