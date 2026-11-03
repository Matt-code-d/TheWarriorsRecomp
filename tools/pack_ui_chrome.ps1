param(
  [Parameter(Mandatory = $true)][string]$Zip,
  [Parameter(Mandatory = $true)][string]$Out
)
$ErrorActionPreference = "Stop"
$tmp = Join-Path $env:TEMP ("twr-nba-" + [guid]::NewGuid().ToString("n"))
New-Item -ItemType Directory -Path $tmp | Out-Null
try {
  Expand-Archive -Path $Zip -DestinationPath $tmp -Force
  $ttf = Get-ChildItem $tmp -Recurse -Filter *.ttf | Select-Object -First 1
  if (-not $ttf) { throw "No TTF inside $Zip" }
  $raw = [IO.File]::ReadAllBytes($ttf.FullName)
  $ms = New-Object IO.MemoryStream
  $ds = New-Object IO.Compression.DeflateStream($ms, [IO.Compression.CompressionLevel]::Optimal)
  $ds.Write($raw, 0, $raw.Length)
  $ds.Close()
  $comp = $ms.ToArray()
  $key = [byte[]](0x57,0x41,0x52,0x52,0x49,0x4F,0x52,0x53,0x52,0x45,0x43,0x4F,0x4D,0x50,0x32,0x36)
  $x = New-Object byte[] $comp.Length
  for ($i = 0; $i -lt $comp.Length; $i++) { $x[$i] = $comp[$i] -bxor $key[$i % $key.Length] }
  $dir = Split-Path $Out -Parent
  if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
  [IO.File]::WriteAllBytes($Out, $x)
  Write-Host "pack_ui_chrome: $($ttf.Name) $($raw.Length) -> $Out ($($x.Length) bytes)"
}
finally {
  Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue
}
