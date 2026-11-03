# Pack launcher HOME title clip into Assets/title_bg.bin (TWB1 frames → deflate + XOR).
# Optimized: every other frame, max width 720, JPEG quality ~55.
param(
    [Parameter(Mandatory = $true)][string]$SrcDir,
    [Parameter(Mandatory = $true)][string]$Out
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $SrcDir)) { throw "missing source: $SrcDir" }

$metaPath = Join-Path $SrcDir "meta.txt"
$fpsIn = 10
if (Test-Path $metaPath) {
    foreach ($line in Get-Content $metaPath) {
        if ($line -match '^fps=(\d+)') { $fpsIn = [int]$Matches[1] }
    }
}

$frames = Get-ChildItem $SrcDir -Filter "frame_*.jpg" | Sort-Object Name
if ($frames.Count -lt 2) { throw "need at least 2 frames in $SrcDir" }

# Keep motion: every other frame; slightly faster than half-rate (5→6.5≈7).
$picked = @()
for ($i = 0; $i -lt $frames.Count; $i += 2) { $picked += $frames[$i] }
$fpsOut = 6

$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter $ms
$bw.Write([Text.Encoding]::ASCII.GetBytes("TWB1"))
$bw.Write([uint32]$fpsOut)
$bw.Write([uint32]$picked.Count)
$bw.Write([uint32]0) # width filled after first encode
$bw.Write([uint32]0) # height

$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq "image/jpeg" }
$ep = New-Object System.Drawing.Imaging.EncoderParameters 1
$ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality, 55L)

$maxW = 720
$outW = 0
$outH = 0
$jpegBlobs = New-Object System.Collections.Generic.List[byte[]]

foreach ($f in $picked) {
    $img = [System.Drawing.Image]::FromFile($f.FullName)
    try {
        $w = $img.Width
        $h = $img.Height
        if ($w -gt $maxW) {
            $nw = $maxW
            $nh = [int][Math]::Round($h * $maxW / [double]$w)
            $bmp = New-Object System.Drawing.Bitmap $nw, $nh
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.DrawImage($img, 0, 0, $nw, $nh)
            $g.Dispose()
            $img.Dispose()
            $img = $bmp
            $w = $nw; $h = $nh
        }
        if ($outW -eq 0) { $outW = $w; $outH = $h }
        $jpg = New-Object System.IO.MemoryStream
        $img.Save($jpg, $codec, $ep)
        $jpegBlobs.Add($jpg.ToArray())
        $jpg.Dispose()
    }
    finally { $img.Dispose() }
}

# rewrite header with size
$bw.Seek(12, "Begin") | Out-Null
$bw.Write([uint32]$outW)
$bw.Write([uint32]$outH)
$bw.Seek(0, "End") | Out-Null
foreach ($blob in $jpegBlobs) {
    $bw.Write([uint32]$blob.Length)
    $bw.Write($blob)
}
$bw.Flush()
$raw = $ms.ToArray()
$bw.Dispose(); $ms.Dispose()

# deflate
$compMs = New-Object System.IO.MemoryStream
$ds = New-Object System.IO.Compression.DeflateStream($compMs, [System.IO.Compression.CompressionLevel]::Optimal, $true)
$ds.Write($raw, 0, $raw.Length)
$ds.Dispose()
$comp = $compMs.ToArray()
$compMs.Dispose()

$key = [Text.Encoding]::ASCII.GetBytes("WARRIORSRECOMP26")
$enc = New-Object byte[] $comp.Length
for ($i = 0; $i -lt $comp.Length; $i++) {
    $enc[$i] = $comp[$i] -bxor $key[$i % $key.Length]
}

$dir = Split-Path -Parent $Out
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
[IO.File]::WriteAllBytes($Out, $enc)
Write-Host ("pack_title_bg: {0} frames @ {1}fps {2}x{3} raw={4}KB -> {5} ({6} bytes)" -f `
    $picked.Count, $fpsOut, $outW, $outH, [math]::Round($raw.Length/1KB), $Out, $enc.Length)
