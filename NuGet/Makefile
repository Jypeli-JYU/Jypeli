all:	windowsgl linux

rebuild:
	make clean && make

rebuild-macos:
	make clean && make macos

rebuild-android:
	make clean && make android

clean:
	rm -f Jypeli.*/lib/4.5/* Jypeli.*/content/4.5/* && \
	rm -fd Jypeli.*/lib/4.5 Jypeli.*/content/4.5 && \
	rm -fd Jypeli.*/lib Jypeli.*/content && \
	rm -f $$(find . -wholename '*/lib/4.5/Jypeli.*' -or -name '*.nupkg')

release: rebuild
	find . -name '*.nupkg' -exec nuget push {} \;

release-macos: rebuild-macos
	find . -wholename '*MacOS/*.nupkg' -exec nuget push {} \;

release-android: rebuild-android
	find . -wholename '*Android/*.nupkg' -exec nuget push {} \;

windowsgl: copywindowsgl
	sh -c "cd Jypeli.WindowsGL && nuget pack"

linux:	copylinux
	sh -c "cd Jypeli.Linux && nuget pack" ; \
	sh -c "cd Jypeli.Physics2d.Linux && nuget pack" ; \
	sh -c "cd Jypeli.SimplePhysics.Linux && nuget pack"

macos:	copymacos
	sh -c "cd Jypeli.MacOS && nuget pack" ; \
	sh -c "cd Jypeli.Physics2d.MacOS && nuget pack" ; \
	sh -c "cd Jypeli.SimplePhysics.MacOS && nuget pack"

android:	copyandroid
	sh -c "cd Jypeli.Android && nuget pack" ; \
	sh -c "cd Jypeli.Physics2d.Android && nuget pack" ; \
	sh -c "cd Jypeli.SimplePhysics.Android && nuget pack"

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

copymacos: copymacosjypeli copymacosphys2d copymacossimplephys

copymacosjypeli:
	mkdir -p Jypeli.MacOS/lib/4.5 Jypeli.MacOS/content/4.5 ; \
	cp ../Compiled/MacOS-AnyCPU/Jypeli.dll Jypeli.MacOS/lib/4.5 && \
	cp ../Compiled/MacOS-AnyCPU/Jypeli.xml Jypeli.MacOS/lib/4.5 && \
	cp ../Compiled/MacOS-AnyCPU/Jypeli.dll.mdb Jypeli.MacOS/lib/4.5 && \
	cp ../Compiled/MacOS-AnyCPU/MonoGame.Framework.dll Jypeli.MacOS/lib/4.5 && \
	cp ../Precompiled/DesktopGL/MonoGame.Framework.dll.config Jypeli.MacOS/lib/4.5 && \
	cp ../Compiled/MacOS-AnyCPU/*.dylib Jypeli.MacOS/content/4.5

copymacosphys2d:
	mkdir -p Jypeli.Physics2d.MacOS/lib/4.5 ; \
	cp ../Compiled/MacOS-AnyCPU/Jypeli.Physics2d.dll Jypeli.Physics2d.MacOS/lib/4.5

copymacossimplephys:
	mkdir -p Jypeli.SimplePhysics.MacOS/lib/4.5 ; \
	cp ../Compiled/MacOS-AnyCPU/Jypeli.SimplePhysics.dll Jypeli.SimplePhysics.MacOS/lib/4.5

copyandroid: copyandroidjypeli copyandroidphys2d copyandroidsimplephys

copyandroidjypeli:
	mkdir -p Jypeli.Android/lib Jypeli.Android/content ; \
	cp ../Compiled/Android-AnyCPU/Jypeli.dll Jypeli.Android/lib && \
	cp ../Compiled/Android-AnyCPU/Jypeli.xml Jypeli.Android/lib && \
	cp ../Compiled/Android-AnyCPU/Jypeli.dll.mdb Jypeli.Android/lib && \
	cp ../Compiled/Android-AnyCPU/MonoGame.Framework.dll Jypeli.Android/lib && \
	cp ../Precompiled/DesktopGL/MonoGame.Framework.dll.config Jypeli.Android/lib

copyandroidphys2d:
	mkdir -p Jypeli.Physics2d.Android/lib/4.5 ; \
	cp ../Compiled/Android-AnyCPU/Jypeli.Physics2d.dll Jypeli.Physics2d.Android/lib/4.5

copyandroidsimplephys:
	mkdir -p Jypeli.SimplePhysics.Android/lib/4.5 ; \
	cp ../Compiled/Android-AnyCPU/Jypeli.SimplePhysics.dll Jypeli.SimplePhysics.Android/lib/4.5


