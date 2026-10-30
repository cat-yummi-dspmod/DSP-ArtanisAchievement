# 生成成就图标到 icons\：白色线稿 + 外圈，透明底，与官方成就图标风格一致

Add-Type -AssemblyName System.Drawing

$Size = 256
$OutDir = Join-Path $PSScriptRoot "icons"
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

function P([float]$x, [float]$y) { New-Object System.Drawing.PointF $x, $y }

function New-Icon([string]$name, [scriptblock]$draw) {
    $bmp = New-Object System.Drawing.Bitmap $Size, $Size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), 10
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
    $g.DrawEllipse($pen, 14, 14, 228, 228)
    & $draw $g $pen $brush
    $out = Join-Path $OutDir "$name.png"
    $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
    $brush.Dispose(); $pen.Dispose(); $g.Dispose(); $bmp.Dispose()
    Write-Host "Icon: $out"
}

# 激活护盾：盾牌 + 加号 / Activating Shields: shield with a plus
New-Icon 'activating-shields' {
    param($g, $pen, $brush)
    $g.DrawPolygon($pen, [System.Drawing.PointF[]]@((P 128 58), (P 190 80), (P 182 150), (P 128 200), (P 74 150), (P 66 80)))
    $g.DrawLine($pen, 128, 100, 128, 160)
    $g.DrawLine($pen, 98, 130, 158, 130)
}

# 大家做得很好：三道军衔折线 / You Have Done Well: three rank chevrons
New-Icon 'well-done' {
    param($g, $pen, $brush)
    foreach ($y in 82, 120, 158) {
        $g.DrawLines($pen, [System.Drawing.PointF[]]@((P 80 ($y + 28)), (P 128 $y), (P 176 ($y + 28))))
    }
}

# 漂亮的一仗：交叉双刃 / Well Fought: crossed blades
New-Icon 'well-fought' {
    param($g, $pen, $brush)
    $g.DrawLine($pen, 76, 76, 172, 172)
    $g.DrawLine($pen, 180, 76, 84, 172)
    $g.DrawLine($pen, 140, 172, 172, 140)
    $g.DrawLine($pen, 84, 140, 116, 172)
    $g.FillEllipse($brush, 168, 168, 18, 18)
    $g.FillEllipse($brush, 70, 168, 18, 18)
}

# 精准无人机：菱形机身 + 准星 / Precision Drone: diamond body with a crosshair
New-Icon 'battle-turned' {
    param($g, $pen, $brush)
    $g.DrawPolygon($pen, [System.Drawing.PointF[]]@((P 128 70), (P 178 128), (P 128 186), (P 78 128)))
    $g.DrawEllipse($pen, 108, 108, 40, 40)
    $g.DrawLine($pen, 128, 92, 128, 104)
    $g.DrawLine($pen, 128, 152, 128, 164)
}

# 攻击无人机：三角机身 + 两根前叉 / Attack Drone: delta body with two prongs
New-Icon 'could-not-hold' {
    param($g, $pen, $brush)
    $g.DrawPolygon($pen, [System.Drawing.PointF[]]@((P 128 110), (P 186 186), (P 70 186)))
    $g.DrawLine($pen, 104, 140, 96, 70)
    $g.DrawLine($pen, 152, 140, 160, 70)
}

# 护卫舰：小型箭形舰 / Corvette: small arrowhead ship
New-Icon 'great-losses' {
    param($g, $pen, $brush)
    $g.DrawPolygon($pen, [System.Drawing.PointF[]]@((P 128 66), (P 166 176), (P 128 156), (P 90 176)))
    $g.DrawLine($pen, 128, 110, 128, 150)
}

# 拥抱战斗的荣耀：护盾圈向外爆散的光芒 / We embrace the glory of battle: rays bursting out of a shield ring
New-Icon 'embrace-glory' {
    param($g, $pen, $brush)
    $g.DrawEllipse($pen, 98, 98, 60, 60)
    for ($i = 0; $i -lt 8; $i++) {
        $a = $i * [Math]::PI / 4
        $c = [Math]::Cos($a); $s = [Math]::Sin($a)
        $g.DrawLine($pen, [float](128 + 46 * $c), [float](128 + 46 * $s), [float](128 + 84 * $c), [float](128 + 84 * $s))
    }
}

# 驱逐舰：长舰体 + 两翼 / Destroyer: long hull with wings
New-Icon 'must-regroup' {
    param($g, $pen, $brush)
    $g.DrawPolygon($pen, [System.Drawing.PointF[]]@((P 128 56), (P 146 96), (P 146 196), (P 110 196), (P 110 96)))
    $g.DrawPolygon($pen, [System.Drawing.PointF[]]@((P 110 126), (P 66 176), (P 110 176)))
    $g.DrawPolygon($pen, [System.Drawing.PointF[]]@((P 146 126), (P 190 176), (P 146 176)))
}
