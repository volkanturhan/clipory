# Generates clipory's application icon: an indigo rounded square with a small
# stack of "cards", evoking a stack of saved clips.
#
# Frames are written as uncompressed 32-bit BMP (DIB) entries rather than PNG,
# because System.Drawing.Icon / the WinForms NotifyIcon load BMP frames
# reliably, whereas PNG-compressed frames can fail to decode.
#
# Run from anywhere; it writes ../clipory/Assets/clipory.ico.
Add-Type -AssemblyName System.Drawing

function New-RoundedRect([single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-IconBitmap([int]$S) {
    $bmp = New-Object System.Drawing.Bitmap($S, $S, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    $indigo = [System.Drawing.Color]::FromArgb(255, 79, 70, 229)   # #4F46E5

    # Background rounded square.
    $m = [single]($S * 0.06)
    $side = [single]($S - 2 * $m)
    $bg = New-RoundedRect $m $m $side $side ([single]($S * 0.22))
    $g.FillPath((New-Object System.Drawing.SolidBrush($indigo)), $bg)

    # Two stacked cards.
    $cardW = [single]($S * 0.42)
    $cardH = [single]($S * 0.50)
    $cx = [single](($S - $cardW) / 2)
    $cy = [single](($S - $cardH) / 2)
    $cr = [single][Math]::Max(2, $S * 0.06)
    $off = [single][Math]::Max(1, $S * 0.07)

    $back = New-RoundedRect ($cx - $off) ($cy - $off) $cardW $cardH $cr
    $g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(120, 255, 255, 255))), $back)

    $front = New-RoundedRect $cx $cy $cardW $cardH $cr
    $g.FillPath((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), $front)

    # Text lines on the front card (only when large enough to read).
    if ($S -ge 32) {
        $lineBrush = New-Object System.Drawing.SolidBrush($indigo)
        $lh = [single][Math]::Max(1, $S * 0.045)
        $lx = [single]($cx + $cardW * 0.16)
        $lw = [single]($cardW * 0.68)
        for ($i = 0; $i -lt 3; $i++) {
            $ly = [single]($cy + $cardH * 0.24 + $i * $cardH * 0.22)
            $line = New-RoundedRect $lx $ly $lw $lh ([single]($lh / 2))
            $g.FillPath($lineBrush, $line)
        }
    }

    $g.Dispose()
    return $bmp
}

# Returns a complete single-frame .ico (as bytes) for one size, produced by
# GDI+ itself via GetHicon -> Icon.Save. Letting GDI+ write the frame means the
# pixel data and its directory entry are guaranteed mutually consistent; we only
# repackage them below.
function Get-SingleFrameIco([System.Drawing.Bitmap]$bmp) {
    $hicon = $bmp.GetHicon()
    $icon = [System.Drawing.Icon]::FromHandle($hicon)
    $ms = New-Object System.IO.MemoryStream
    $icon.Save($ms)
    $icon.Dispose()
    $bytes = $ms.ToArray()
    $ms.Dispose()
    return , $bytes
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)

# A typed list, not @() with +=, so the byte[] frames are not flattened.
$singles = New-Object 'System.Collections.Generic.List[byte[]]'
foreach ($s in $sizes) {
    $bmp = New-IconBitmap $s
    $singles.Add((Get-SingleFrameIco $bmp))
    $bmp.Dispose()
}

$out = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter($out)

# ICONDIR header.
$w.Write([uint16]0)
$w.Write([uint16]1)
$w.Write([uint16]$sizes.Count)

# ICONDIRENTRY per frame. Each single-frame .ico already holds a valid 16-byte
# entry at offset 6 describing its frame; we copy it verbatim (preserving the
# width/height/planes/bit-count GDI+ chose) and only patch the byte count and
# the offset to where the frame sits in the combined file.
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $single = $singles[$i]
    $blobLength = $single.Length - 22

    $entry = New-Object byte[] 16
    [System.Array]::Copy($single, 6, $entry, 0, 16)
    [System.BitConverter]::GetBytes([uint32]$blobLength).CopyTo($entry, 8)   # dwBytesInRes
    [System.BitConverter]::GetBytes([uint32]$offset).CopyTo($entry, 12)      # dwImageOffset

    $w.Write($entry, 0, 16)
    $offset += $blobLength
}

# Frame data, in the same order as the entries above.
foreach ($single in $singles) {
    $w.Write($single, 22, $single.Length - 22)
}
$w.Flush()

$target = Join-Path $PSScriptRoot '..\clipory\Assets\clipory.ico'
[System.IO.File]::WriteAllBytes($target, $out.ToArray())
$w.Dispose()
Write-Output "Wrote $((Resolve-Path $target).Path) ($((Get-Item $target).Length) bytes)"
