all:	windowsgl linux physics2d simplephysics

clean:
	rm -f $$(find . -wholename '*/lib/Jypeli.*' -or -name '*.nupkg') && \
	rm -fd $$(find . -name 'lib')

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
	cp ../Compiled/WindowsGL-AnyCPU/Jypeli.* Jypeli.WindowsGL/lib

copylinux:
	mkdir -p Jypeli.Linux/lib ; \
	cp ../Compiled/Linux-AnyCPU/Jypeli.* Jypeli.Linux/lib

copyphys2d:
	mkdir -p Jypeli.Physics2d/lib ; \
	cp ../Compiled/WindowsGL-AnyCPU/Jypeli.Physics2d.dll Jypeli.Physics2d/lib

copysimplephys:
	mkdir -p Jypeli.SimplePhysics/lib ; \
	cp ../Compiled/WindowsGL-AnyCPU/Jypeli.SimplePhysics.dll Jypeli.Physics2d/lib

