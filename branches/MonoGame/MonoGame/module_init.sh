#!/bin/sh

# First run: clone the repository
git init && git remote add -t \* -f origin https://github.com/mono/MonoGame.git

# All runs: fetch changes, merge them, update modules, apply new patches
git fetch
git merge origin/master
git submodule update --init --recursive

for file in `find . -name '*.cs.diff'`
do
   patch -N -r - `echo $file | sed -e 's/\.diff//g'` $file
done
