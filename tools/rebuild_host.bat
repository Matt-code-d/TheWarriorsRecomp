@echo off
setlocal
set "PF86=%ProgramFiles(x86)%"
set "VSWHERE=%PF86%\Microsoft Visual Studio\Installer\vswhere.exe"
for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do set "VSDIR=%%i"
call "%VSDIR%\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64
set "VSCMAKE=%VSDIR%\Common7\IDE\CommonExtensions\Microsoft\CMake"
set "PATH=C:\Program Files\CMake\bin;%VSCMAKE%\CMake\bin;%VSCMAKE%\Ninja;%VSDIR%\VC\Tools\Llvm\x64\bin;%PATH%"
if exist "%ProgramFiles%\LLVM\bin\clang++.exe" set "PATH=%ProgramFiles%\LLVM\bin;%PATH%"
cd /d D:\Games\TheWarriorsRecomp\warriors-recomp
echo === reconfigure ===
cmake -S . -B out\build\release -G Ninja -DCMAKE_BUILD_TYPE=Release -DCMAKE_C_COMPILER=clang -DCMAKE_CXX_COMPILER=clang++ -DCMAKE_C_FLAGS=-march=x86-64-v2 -DCMAKE_CXX_FLAGS=-march=x86-64-v2
if errorlevel 1 exit /b 1
echo === build Warriors + Warriors_code ===
cmake --build out\build\release --target Warriors Warriors_code -j 32
if errorlevel 1 exit /b 1
rem Keep built.txt stamp in sync (EXE size+mtime+XBE sha1) so PLAY does not recompile the game DLL.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0sync_game_stamp.ps1"
echo === publish launcher beside EXE + into port\ ===
dotnet build launcher\WarriorsLauncher.csproj -c Release -o out\build\release
if errorlevel 1 exit /b 1
if exist ..\port (
  copy /Y out\build\release\WarriorsLauncher.exe ..\port\ >nul
  copy /Y out\build\release\WarriorsLauncher.dll ..\port\ >nul
  copy /Y out\build\release\WarriorsLauncher.deps.json ..\port\ >nul
  copy /Y out\build\release\WarriorsLauncher.runtimeconfig.json ..\port\ >nul
)
echo === done ===
dir out\build\release\Warriors.exe out\build\release\Warriors.dll out\build\release\WarriorsLauncher.exe
exit /b 0
