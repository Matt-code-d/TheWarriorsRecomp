# Keep compiler/work/Warriors/built.txt matching the current Warriors.exe + assets/default.xbe
# so PLAY does not re-run xbe_recomp / clang after a host rebuild.
$ErrorActionPreference = "Stop"
$root = Join-Path $PSScriptRoot "..\warriors-recomp\out\build\release" | Resolve-Path
$exe = Get-Item (Join-Path $root "Warriors.exe")
$dllPath = Join-Path $root "Warriors.dll"
if (-not (Test-Path $dllPath)) { throw "Warriors.dll missing in $root" }
$dll = Get-Item $dllPath
$dll.LastWriteTime = $exe.LastWriteTime.AddSeconds(2)

$xbe = Join-Path $root "assets\default.xbe"
if (-not (Test-Path $xbe)) {
    $alt = Join-Path $PSScriptRoot "..\warriors-recomp\assets\default.xbe"
    if (Test-Path $alt) { $xbe = (Resolve-Path $alt).Path }
}
if (-not (Test-Path $xbe)) {
    Write-Host "no default.xbe — first PLAY will build the DLL once"
    exit 0
}

$sha = (Get-FileHash -Path $xbe -Algorithm SHA1).Hash.ToLowerInvariant()
$ft = [DateTimeOffset]$exe.LastWriteTimeUtc
$stamp = "{0} {1} {2}" -f $exe.Length, $ft.ToFileTime(), $sha
$work = Join-Path $root "compiler\work\Warriors"
New-Item -ItemType Directory -Force -Path $work | Out-Null
Set-Content -Path (Join-Path $work "built.txt") -Value $stamp -Encoding Ascii
Write-Host "stamp -> $stamp"
Write-Host "dll mtime -> $($dll.LastWriteTime)"
