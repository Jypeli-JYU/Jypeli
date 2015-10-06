#!/bin/sh
git init && git remote add -t \* -f origin https://github.com/mono/MonoGame.git && git pull origin master && git submodule update --init --recursive ThirdParty
for file in `find . -name '*.cs.diff'`
do
   patch `echo $file | sed -e 's/\.diff//g'` $file
done
