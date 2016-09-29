all:	jypeli physics2d simplephysics

physics2d: physics2d-windows physics2d-linux

simplephysics: simplephysics-windows simplephysics-linux

physics2d-windows:	jypeli-windowsgl
	cp Physics2d/bin/WindowsGL/AnyCPU/Release/* Compiled/WindowsGL-AnyCPU/

simplephysics-windows:	jypeli-windowsgl
	cp SimplePhysics/bin/WindowsGL/AnyCPU/Release/* Compiled/WindowsGL-AnyCPU/

physics2d-linux:	jypeli-linux
	cp Physics2d/bin/Linux/AnyCPU/Release/* Compiled/Linux-AnyCPU/

simplephysics-linux:	jypeli-linux
	cp SimplePhysics/bin/Linux/AnyCPU/Release/* Compiled/Linux-AnyCPU/

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

getmonogame:
	bash -c "cd MonoGame; if ! [ -a .git ]; then sh module_init.sh; fi"

