# Packs About credit bodies into Assets/notice.bin (deflate + XOR).
# Plaintext lives only here for regen — runtime ships the blob, not these strings.
param(
  [Parameter(Mandatory = $false)]
  [string]$Out = (Join-Path $PSScriptRoot "..\warriors-recomp\launcher\Assets\notice.bin")
)
$ErrorActionPreference = "Stop"

# lang|body  (UTF-8). Keep author credit intact when regenerating.
$lines = @(
  "en|EUR bring-up: Matteo Santoro (Matt-code-d). Runtime SDK: TGP482 / XboxXBERecomp. See CONTRIBUTING.md.",
  "it|EUR bring-up: Matteo Santoro (Matt-code-d). Runtime SDK: TGP482 / XboxXBERecomp. Vedi CONTRIBUTING.md.",
  "de|EUR bring-up: Matteo Santoro (Matt-code-d). Runtime SDK: TGP482 / XboxXBERecomp. Siehe CONTRIBUTING.md.",
  "fr|EUR bring-up: Matteo Santoro (Matt-code-d). Runtime SDK: TGP482 / XboxXBERecomp. Voir CONTRIBUTING.md.",
  "es|EUR bring-up: Matteo Santoro (Matt-code-d). Runtime SDK: TGP482 / XboxXBERecomp. Ver CONTRIBUTING.md."
)
$raw = [Text.Encoding]::UTF8.GetBytes(($lines -join "`n") + "`n")

$ms = New-Object IO.MemoryStream
$ds = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionLevel]::Optimal)
$ds.Write($raw, 0, $raw.Length)
$ds.Close()
$comp = $ms.ToArray()

# Different key than chrome.bin / title_bg.bin
$key = [byte[]](0x4E,0x4F,0x54,0x49,0x43,0x45,0x2D,0x54,0x57,0x52,0x2D,0x32,0x30,0x32,0x36,0x21)
$x = New-Object byte[] $comp.Length
for ($i = 0; $i -lt $comp.Length; $i++) { $x[$i] = $comp[$i] -bxor $key[$i % $key.Length] }

$dir = Split-Path $Out -Parent
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
[IO.File]::WriteAllBytes($Out, $x)
Write-Host "pack_credits: $($raw.Length) bytes plain -> $Out ($($x.Length) bytes)"
