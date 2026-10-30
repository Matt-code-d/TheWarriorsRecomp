@echo off
setlocal
set CI=1
cd /d "D:\Games\TheWarriorsRecomp\warriors-recomp"

set "VS=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools"
call "%VS%\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64
set "VSCMAKE=%VS%\Common7\IDE\CommonExtensions\Microsoft\CMake"
rem Prefer VS Clang + VS/CMake/Ninja ahead of MinGW on PATH
set "PATH=%VSCMAKE%\CMake\bin;%VSCMAKE%\Ninja;%VS%\VC\Tools\Llvm\x64\bin;%ProgramFiles%\CMake\bin;%PATH%"

echo === toolchain ===
where clang++
where cmake
where ninja
clang++ --version
cmake --version

echo === presets ===
cmake --list-presets

echo === configure ===
cmake --preset release
if errorlevel 1 (
  echo CONFIGURE FAILED
  exit /b 1
)

echo === build xbe_symbols ===
cmake --build out\build\release --target xbe_symbols
if errorlevel 1 (
  echo XBE_SYMBOLS TARGET FAILED
  exit /b 1
)

if not exist out\build\release\sdk_dir.txt (
  echo sdk_dir.txt missing
  exit /b 1
)
set /p SDK=<out\build\release\sdk_dir.txt
echo SDK=%SDK%

if not exist config mkdir config
findstr /b /l /c:"[symbols]" config\game.toml >nul 2>nul
if errorlevel 1 (
  echo === run xbe_symbols ===
  "out\build\release\xbe_symbols.exe" assets\default.xbe "%SDK%\xdk" --library Warriors.dll --out config\game.toml
  if errorlevel 1 (
    echo XBE_SYMBOLS RUN FAILED
    exit /b 1
  )
) else (
  echo config\game.toml already has [symbols]
)

echo === build Warriors ===
cmake --build out\build\release
if errorlevel 1 (
  echo GAME BUILD FAILED
  exit /b 1
)

echo === DONE ===
dir out\build\release\Warriors.exe
exit /b 0
