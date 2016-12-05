all:	jypeli physics2d simplephysics

physics2d: physics2d-windows physics2d-linux physics2d-android

simplephysics: simplephysics-windows simplephysics-linux simplephysics-android

macos:	jypeli-macos physics2d-macos simplephysics-macos

android: jypeli-android physics2d-android simplephysics-android

rebuild-macos:
	rm -f Compiled/MacOS-AnyCPU/* && make macos

physics2d-windows:	jypeli-windowsgl
	cp Physics2d/bin/WindowsGL/AnyCPU/Release/* Compiled/WindowsGL-AnyCPU/

simplephysics-windows:	jypeli-windowsgl
	cp SimplePhysics/bin/WindowsGL/AnyCPU/Release/* Compiled/WindowsGL-AnyCPU/

physics2d-linux:	jypeli-linux
	cp Physics2d/bin/Linux/AnyCPU/Release/* Compiled/Linux-AnyCPU/

simplephysics-linux:	jypeli-linux
	cp SimplePhysics/bin/Linux/AnyCPU/Release/* Compiled/Linux-AnyCPU/

physics2d-macos:	jypeli-macos
	cp Physics2d/bin/MacOS/AnyCPU/Debug/* Compiled/MacOS-AnyCPU/

simplephysics-macos:	jypeli-macos
	cp SimplePhysics/bin/MacOS/AnyCPU/Debug/* Compiled/MacOS-AnyCPU/

physics2d-android:	jypeli-android
	cp Physics2d/bin/Android/AnyCPU/Debug/* Compiled/Android-AnyCPU/

simplephysics-android:	jypeli-android
	cp SimplePhysics/bin/Android/AnyCPU/Debug/* Compiled/Android-AnyCPU/

jypeli: jypeli-windowsgl jypeli-linux

jypeli-windowsgl:	getmonogame
	mono Protobuild.exe -generate WindowsGL && \
	xbuild /p:Configuration=Release Jypeli.WindowsGL.sln && \
	mkdir -p Compiled/WindowsGL-AnyCPU && \
	cp Jypeli/bin/WindowsGL/AnyCPU/Release/* Compiled/WindowsGL-AnyCPU/

jypeli-linux:	getmonogame
	mono Protobuild.exe -generate Linux && \
	xbuild /p:Configuration=Release Jypeli.Linux.sln && \
	mkdir -p Compiled/Linux-AnyCPU && \
	cp Jypeli/bin/Linux/AnyCPU/Release/* Compiled/Linux-AnyCPU/

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

