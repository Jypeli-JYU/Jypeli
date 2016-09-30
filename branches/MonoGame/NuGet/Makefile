all:	windowsgl linux physics2d simplephysics

rebuild:
	make clean && make

clean:
	rm -f $$(find . -wholename '*/lib/Jypeli.*' -or -name '*.nupkg') && \
	rm -fd $$(find . -name 'lib')

release: rebuild
	find . -name '*.nupkg' -exec nuget push {} \;

windowsgl: copywindowsgl
	sh -c "cd Jypeli.WindowsGL && nuget pack"

linux:	copylinux
	sh -c "cd Jypeli.Linux && nuget pack"

physics2d:	copyphys2d
	sh -c "cd Jypeli.Physics2d && nuget pack"

simplephysics:	copysimplephys
	sh -c "cd Jypeli.SimplePhysics && nuget pack"

copywindowsgl:
	mkdir -p Jypeli.WindowsGL/lib ; \
	cp ../Compiled/WindowsGL-AnyCPU/Jypeli.dll Jypeli.WindowsGL/lib

copylinux:
	mkdir -p Jypeli.Linux/lib ; \
	cp ../Compiled/Linux-AnyCPU/Jypeli.dll Jypeli.Linux/lib && \
	cp ../Compiled/Linux-AnyCPU/MonoGame.Framework.dll Jypeli.Linux/lib && \
	cp ../Compiled/Linux-AnyCPU/NVorbis.dll Jypeli.Linux/lib && \
	cp ../Compiled/Linux-AnyCPU/OpenTK.* Jypeli.Linux/lib && \
	cp ../Compiled/Linux-AnyCPU/Tao.Sdl.* Jypeli.Linux/lib  


copyphys2d:
	mkdir -p Jypeli.Physics2d/lib ; \
	cp ../Compiled/WindowsGL-AnyCPU/Jypeli.Physics2d.dll Jypeli.Physics2d/lib/Jypeli.Physics2d.WindowsGL.dll && \
	cp ../Compiled/Linux-AnyCPU/Jypeli.Physics2d.dll Jypeli.Physics2d/lib/Jypeli.Physics2d.Linux.dll

copysimplephys:
	mkdir -p Jypeli.SimplePhysics/lib ; \
	cp ../Compiled/WindowsGL-AnyCPU/Jypeli.SimplePhysics.dll Jypeli.SimplePhysics/lib/Jypeli.SimplePhysics.WindowsGL.dll && \
	cp ../Compiled/Linux-AnyCPU/Jypeli.SimplePhysics.dll Jypeli.SimplePhysics/lib/Jypeli.SimplePhysics.Linux.dll

