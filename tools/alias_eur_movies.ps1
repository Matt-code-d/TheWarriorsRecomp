# EUR disc stores Bink as *_pal.bik under bik_pal/. Host may report NTSC flags and the title then
# opens D:\bik\logo.bik — hardlink NTSC names beside the PAL files (and keep bik -> bik_pal junction).
param(
    [string]$Assets = "D:\Games\TheWarriorsRecomp\warriors-recomp\assets"
)

$ErrorActionPreference = "Stop"
$pal = Join-Path $Assets "bik_pal"
if (-not (Test-Path $pal)) { throw "missing $pal" }

$bik = Join-Path $Assets "bik"
if (-not (Test-Path $bik)) {
    cmd /c "mklink /J `"$bik`" `"$pal`"" | Out-Host
}

Get-ChildItem $pal -Filter "*_pal.bik" | ForEach-Object {
    $ntsc = Join-Path $pal ($_.Name -replace '_pal\.bik$', '.bik')
    if (-not (Test-Path $ntsc)) {
        New-Item -ItemType HardLink -Path $ntsc -Target $_.FullName | Out-Null
        Write-Host "linked $(Split-Path $ntsc -Leaf)"
    }
}
Write-Host "EUR movie aliases ready in $pal"
