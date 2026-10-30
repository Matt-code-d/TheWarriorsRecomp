@echo off
setlocal
set CI=1
cd /d "D:\Games\TheWarriorsRecomp\warriors-recomp"

set "VS=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"
call "%VS%\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64 >nul
set "VSCMAKE=%VS%\Common7\IDE\CommonExtensions\Microsoft\CMake"
set "PATH=%VSCMAKE%\CMake\bin;%VSCMAKE%\Ninja;%VS%\VC\Tools\Llvm\x64\bin;%ProgramFiles%\CMake\bin;%PATH%"

echo === resume Warriors build ===
cmake --build out\build\release
if errorlevel 1 (
  echo GAME BUILD FAILED
  exit /b 1
)
echo === DONE ===
dir out\build\release\Warriors.exe
exit /b 0
