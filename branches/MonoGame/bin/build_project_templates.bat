@echo off
setlocal

set zip="Q:\Programs\Archive\7zip\7z.exe"
if not exist %zip% (
  echo ERROR: 7-Zip not found
  goto error
)

if "%1"=="" (
  echo Usage: %0 templates_dir
  goto error
)
set templates_dir=%1

pushd %templates_dir%

for /D %%d in (*) do (
  pushd %%d
  echo zipataan %%d
  %zip% a %%d.zip *
  move %%d.zip ..
  popd
)

popd
goto finish


:error
endlocal
exit /B 1

:finish
endlocal
