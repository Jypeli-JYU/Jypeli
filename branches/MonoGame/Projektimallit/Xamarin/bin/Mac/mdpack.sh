#!/bin/sh
MDTOOL="/Applications/Xamarin Studio.app/Contents/MacOS/mdtool"
mkdir mpack
rm mpack/*
"${MDTOOL}" setup pack Release/MonoDevelop.Jypeli.Mac.dll && mv *.mpack mpack && "${MDTOOL}" setup rep-build mpack
rm mpack/index.html
