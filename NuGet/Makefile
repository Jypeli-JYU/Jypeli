all:	windowsgl linux

rebuild:
	make clean && make

clean:
	rm -f $$(find . -wholename '*/lib/Jypeli.*') && \
	rm -f $$(find . -wholename '*/lib/4.5/Jypeli.*' -or -name '*.nupkg') && \
	rm -f Jypeli.Linux/lib/4.5/MonoGame.Framework.dll Jypeli.Linux/lib/4.5/NVorbis.dll Jypeli.Linux/lib/4.5/OpenTK.* Jypeli.Linux/lib/4.5/Tao.Sdl.* && \
	rm -fd $$(find . -wholename '*/lib/4.5') && \
	rm -fd $$(find . -name 'lib')

release: rebuild
	find . -name '*.nupkg' -exec nuget push {} \;

windowsgl: copywindowsgl
	sh -c "cd Jypeli.WindowsGL && nuget pack"

linux:	copylinux
	sh -c "cd Jypeli.Linux && nuget pack" ; \
	sh -c "cd Jypeli.Physics2d.Linux && nuget pack" ; \
	sh -c "cd Jypeli.SimplePhysics.Linux && nuget pack"

copywindowsgl:
	mkdir -p Jypeli.WindowsGL/lib/4.5 ; \
	mkdir -p Jypeli.WindowsGL/content/4.5 ; \
	cp ../Compiled/WindowsGL-AnyCPU/Jypeli.dll Jypeli.WindowsGL/lib/4.5 && \
	cp ../Compiled/WindowsGL-AnyCPU/MonoGame.Framework.dll Jypeli.WindowsGL/lib/4.5 && \
	cp ../Compiled/WindowsGL-AnyCPU/NVorbis.dll Jypeli.WindowsGL/lib/4.5 && \
	cp ../Compiled/WindowsGL-AnyCPU/OpenTK.dll Jypeli.WindowsGL/lib/4.5 && \
	cp ../Compiled/WindowsGL-AnyCPU/Tao.Sdl.dll Jypeli.WindowsGL/lib/4.5 && \
	cp ../Compiled/WindowsGL-AnyCPU/SDL.dll Jypeli.WindowsGL/content/4.5 && \
	cp ../Compiled/WindowsGL-AnyCPU/SDL_mixer.dll Jypeli.WindowsGL/content/4.5 && \
	cp ../Compiled/WindowsGL-AnyCPU/smpeg.dll Jypeli.WindowsGL/content/4.5
	
copylinux: copylinuxjypeli copylinuxphys2d copylinuxsimplephys

copylinuxjypeli:
	mkdir -p Jypeli.Linux/lib/4.5 ; \
	cp ../Compiled/Linux-AnyCPU/Jypeli.dll Jypeli.Linux/lib/4.5 && \
	cp ../Compiled/Linux-AnyCPU/MonoGame.Framework.dll Jypeli.Linux/lib/4.5 && \
	cp ../Compiled/Linux-AnyCPU/NVorbis.dll Jypeli.Linux/lib/4.5 && \
	cp ../Compiled/Linux-AnyCPU/OpenTK.* Jypeli.Linux/lib/4.5 && \
	cp ../Compiled/Linux-AnyCPU/Tao.Sdl.* Jypeli.Linux/lib/4.5

copylinuxphys2d:
	mkdir -p Jypeli.Physics2d.Linux/lib/4.5 ; \
	cp ../Compiled/Linux-AnyCPU/Jypeli.Physics2d.dll Jypeli.Physics2d.Linux/lib/4.5

copylinuxsimplephys:
	mkdir -p Jypeli.SimplePhysics.Linux/lib/4.5 ; \
	cp ../Compiled/Linux-AnyCPU/Jypeli.SimplePhysics.dll Jypeli.SimplePhysics.Linux/lib/4.5

