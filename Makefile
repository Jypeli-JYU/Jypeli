common:	jypeli physics2d simplephysics

all:	jypeli-all physics2d-all simplephysics-all

physics2d: physics2d-windows physics2d-linux

physics2d-all:	physics2d-windows physics2d-linux physics2d-android physics2d-mac

simplephysics:	simplephysics-windows simplephysics-linux

simplephysics-all:	simplephysics-windows simplephysics-linux simplephysics-android simplephysics-mac

windowsgl:	jypeli-windowsgl physics2d-windowsgl simplephysics-windowsgl

linux:	jypeli-linux physics2d-linux simplephysics-linux

macos:	jypeli-macos physics2d-macos simplephysics-macos

android: jypeli-android physics2d-android simplephysics-android

rebuild-macos:
	rm -f Compiled/MacOS-AnyCPU/* && make macos

physics2d-windows:	jypeli-windowsgl
	cp Physics2d/bin/WindowsGL/AnyCPU/Debug/* Compiled/WindowsGL-AnyCPU/

simplephysics-windows:	jypeli-windowsgl
	cp SimplePhysics/bin/WindowsGL/AnyCPU/Debug/* Compiled/WindowsGL-AnyCPU/

physics2d-linux:	jypeli-linux
	cp Physics2d/bin/Linux/AnyCPU/Debug/* Compiled/Linux-AnyCPU/

simplephysics-linux:	jypeli-linux
	cp SimplePhysics/bin/Linux/AnyCPU/Debug/* Compiled/Linux-AnyCPU/

physics2d-macos:	jypeli-macos
	cp Physics2d/bin/MacOS/AnyCPU/Debug/* Compiled/MacOS-AnyCPU/

simplephysics-macos:	jypeli-macos
	cp SimplePhysics/bin/MacOS/AnyCPU/Debug/* Compiled/MacOS-AnyCPU/

physics2d-android:	jypeli-android
	cp Physics2d/bin/Android/AnyCPU/Debug/* Compiled/Android-AnyCPU/

simplephysics-android:	jypeli-android
	cp SimplePhysics/bin/Android/AnyCPU/Debug/* Compiled/Android-AnyCPU/

jypeli: jypeli-windowsgl jypeli-linux

jypeli-all:	jypeli-windowsgl jypeli-linux jypeli-android jypeli-macos

jypeli-windowsgl:	getmonogame
	mono Protobuild.exe -generate WindowsGL && \
	xbuild Jypeli.WindowsGL.sln && \
	mkdir -p Compiled/WindowsGL-AnyCPU && \
	cp Jypeli/bin/WindowsGL/AnyCPU/Debug/* Compiled/WindowsGL-AnyCPU/

jypeli-linux:	getmonogame
	mono Protobuild.exe -generate Linux && \
	xbuild Jypeli.Linux.sln && \
	mkdir -p Compiled/Linux-AnyCPU && \
	cp Jypeli/bin/Linux/AnyCPU/Debug/* Compiled/Linux-AnyCPU/

jypeli-macos:	getmonogame
	mono Protobuild.exe -generate MacOS && \
	xbuild /t:Rebuild Jypeli.MacOS.sln && \
	mkdir -p Compiled/MacOS-AnyCPU && \
	cp Jypeli/bin/MacOS/AnyCPU/Debug/* Compiled/MacOS-AnyCPU/

jypeli-android:	getmonogame
	mono Protobuild.exe -generate Android && \
	xbuild Jypeli.Android.sln && \
	mkdir -p Compiled/Android-AnyCPU && \
	cp Jypeli/bin/Android/AnyCPU/Debug/* Compiled/Android-AnyCPU/

getmonogame:
	bash -c "cd MonoGame; if ! [ -a .git ]; then sh module_init.sh; fi"

