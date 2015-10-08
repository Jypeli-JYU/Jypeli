#!/bin/sh

# First run: clone the repository
git init && git remote add -t \* -f origin https://github.com/mono/MonoGame.git

# Subsequent runs: stash changes, update everything, pop stash, apply new patches
git stash && git pull origin master && git submodule update --init --recursive . && git stash pop
for file in `find . -name '*.cs.diff'`
do
   patch -N -r - `echo $file | sed -e 's/\.diff//g'` $file
done
