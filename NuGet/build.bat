@echo off
setlocal

mkdir Jypeli\lib
mkdir Jypeli\content
mkdir Jypeli.Physics2d\lib
mkdir Jypeli.Physics2d\content
mkdir Jypeli.SimplePhysics\lib
mkdir Jypeli.SimplePhysics\content

copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.dll Jypeli\lib
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.xml Jypeli\content
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.dll Jypeli.Physics2d\lib
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.xml Jypeli.Physics2d\content
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.dll Jypeli.SimplePhysics\lib
copy ..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.xml Jypeli.SimplePhysics\content

pushd Jypeli
..\nuget pack
popd

pushd Jypeli.Physics2d
..\nuget pack
popd

pushd Jypeli.SimplePhysics
..\nuget pack
popd

endlocal
