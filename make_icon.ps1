# 生成 256x256 的 icon.png：金色盾牌上一道蓝色灵能光刃

Add-Type -AssemblyName System.Drawing

$Size = 256
$Output = Join-Path $PSScriptRoot "icon.png"

$bmp = New-Object System.Drawing.Bitmap $Size, $Size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$shield = New-Object System.Drawing.Drawing2D.GraphicsPath
$shield.AddLines([System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 128, 12),
    (New-Object System.Drawing.PointF 230, 48),
    (New-Object System.Drawing.PointF 218, 150),
    (New-Object System.Drawing.PointF 128, 244),
    (New-Object System.Drawing.PointF 38, 150),
    (New-Object System.Drawing.PointF 26, 48)))
$shield.CloseFigure()
$gold = New-Object System.Drawing.Drawing2D.LinearGradientBrush (
    (New-Object System.Drawing.PointF 0, 12), (New-Object System.Drawing.PointF 0, 244),
    [System.Drawing.Color]::FromArgb(255, 255, 225, 120),
    [System.Drawing.Color]::FromArgb(255, 170, 100, 20))
$g.FillPath($gold, $shield)
$rim = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 60, 35, 5)), 8
$rim.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$g.DrawPath($rim, $shield)

$inner = New-Object System.Drawing.Drawing2D.GraphicsPath
$inner.AddLines([System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 128, 40),
    (New-Object System.Drawing.PointF 200, 66),
    (New-Object System.Drawing.PointF 192, 142),
    (New-Object System.Drawing.PointF 128, 210),
    (New-Object System.Drawing.PointF 64, 142),
    (New-Object System.Drawing.PointF 56, 66)))
$inner.CloseFigure()
$dark = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 14, 24, 60))
$g.FillPath($dark, $inner)

$blade = New-Object System.Drawing.Drawing2D.GraphicsPath
$blade.AddLines([System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF 128, 52),
    (New-Object System.Drawing.PointF 146, 120),
    (New-Object System.Drawing.PointF 128, 196),
    (New-Object System.Drawing.PointF 110, 120)))
$blade.CloseFigure()
$glow = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(110, 80, 200, 255)), 16
$glow.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$g.DrawPath($glow, $blade)
$psi = New-Object System.Drawing.Drawing2D.LinearGradientBrush (
    (New-Object System.Drawing.PointF 0, 52), (New-Object System.Drawing.PointF 0, 196),
    [System.Drawing.Color]::FromArgb(255, 220, 250, 255),
    [System.Drawing.Color]::FromArgb(255, 40, 150, 255))
$g.FillPath($psi, $blade)

$bmp.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)
$psi.Dispose(); $glow.Dispose(); $blade.Dispose(); $dark.Dispose(); $inner.Dispose()
$rim.Dispose(); $gold.Dispose(); $shield.Dispose(); $g.Dispose(); $bmp.Dispose()
Write-Host "Icon created: $Output" -ForegroundColor Green
