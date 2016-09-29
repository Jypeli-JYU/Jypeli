@echo off
setlocal

mkdir Jypeli.WindowsGL\lib
mkdir Jypeli.Linux\lib
mkdir Jypeli.Physics2d\lib
mkdir Jypeli.SimplePhysics\lib

copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.dll Jypeli.WindowsGL\lib
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.xml Jypeli.WindowsGL\content
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.dll Jypeli.Physics2d\lib
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.xml Jypeli.Physics2d\content
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.dll Jypeli.SimplePhysics\lib
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.xml Jypeli.SimplePhysics\content

copy ..\Compiled\Linux-AnyCPU\Jypeli.dll Jypeli.Linux\lib
copy ..\Compiled\Linux-AnyCPU\Jypeli.xml Jypeli.Linux\content
copy ..\Compiled\Linux-AnyCPU\Jypeli.Physics2d.dll Jypeli.Physics2d\lib
copy ..\Compiled\Linux-AnyCPU\Jypeli.Physics2d.xml Jypeli.Physics2d\content
copy ..\Compiled\Linux-AnyCPU\Jypeli.SimplePhysics.dll Jypeli.SimplePhysics\lib
copy ..\Compiled\Linux-AnyCPU\Jypeli.SimplePhysics.xml Jypeli.SimplePhysics\content

pushd Jypeli.WindowsGL
..\nuget pack
popd

pushd Jypeli.Linux
..\nuget pack
popd

pushd Jypeli.Physics2d
..\nuget pack
popd

pushd Jypeli.SimplePhysics
..\nuget pack
popd

endlocal
