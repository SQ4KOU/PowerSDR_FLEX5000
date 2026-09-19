[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P21-SPLASH] $s" }

$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'

foreach($p in @($splashResx,$splashDesigner)){
    if(!(Test-Path -LiteralPath $p)){ throw "P21 splash input missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13)+([string][char]10)
$lf = [string][char]10

function Read-Normal([string]$p){ return [IO.File]::ReadAllText($p).Replace($crlf,$lf) }
function Write-Normal([string]$p,[string]$s){ [IO.File]::WriteAllText($p,$s.Replace($lf,$crlf),$utf8) }

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace($crlf,$lf)
    $New=$New.Replace($crlf,$lf)
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P21 anchor missing: $Label" }
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){ throw "P21 anchor not unique: $Label" }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

Add-Type -AssemblyName System.Drawing

$resx=[IO.File]::ReadAllText($splashResx)
$bgRx=[regex]::new('(?s)(<data name="\$this\.BackgroundImage"[^>]*>.*?<value>)(.*?)(</value>.*?</data>)')
$m=$bgRx.Match($resx)
if(!$m.Success){ throw 'P21 BackgroundImage resource missing after P20' }

$payload=($m.Groups[2].Value -replace '\s','')
$bytes=[Convert]::FromBase64String($payload)

$input=[IO.MemoryStream]::new($bytes)
try{
    $src=[Drawing.Image]::FromStream($input)
    if($src.Width -ne 600 -or $src.Height -ne 384){
        throw "P21 source splash dimensions invalid: $($src.Width)x$($src.Height)"
    }

    $bmp=[Drawing.Bitmap]::new(600,384,[Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g=[Drawing.Graphics]::FromImage($bmp)
    try{
        $g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.TextRenderingHint=[Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $g.DrawImage($src,0,0,600,384)

        # Remove the baked KE9NS callsign and ke9ns.com/flexpage.html line from
        # the original P20 bitmap.  Rebuild the local blue background using
        # colours sampled immediately around the original inscription area.
        $rect=[Drawing.Rectangle]::new(14,136,252,67)
        $leftTop=$bmp.GetPixel(14,132)
        $rightTop=$bmp.GetPixel(266,132)
        $leftBottom=$bmp.GetPixel(14,207)
        $rightBottom=$bmp.GetPixel(266,207)

        function Mix-Color([Drawing.Color]$a,[Drawing.Color]$b,[double]$t){
            $r=[int][Math]::Round($a.R + ($b.R-$a.R)*$t)
            $gg=[int][Math]::Round($a.G + ($b.G-$a.G)*$t)
            $bb=[int][Math]::Round($a.B + ($b.B-$a.B)*$t)
            return [Drawing.Color]::FromArgb($r,$gg,$bb)
        }

        for($yy=0;$yy -lt $rect.Height;$yy++){
            $ty=[double]$yy/[Math]::Max(1,$rect.Height-1)
            $lc=Mix-Color $leftTop $leftBottom $ty
            $rc=Mix-Color $rightTop $rightBottom $ty
            $lineRect=[Drawing.Rectangle]::new($rect.X,($rect.Y+$yy),$rect.Width,1)
            $brush=[Drawing.Drawing2D.LinearGradientBrush]::new($lineRect,$lc,$rc,0.0)
            try{ $g.FillRectangle($brush,$lineRect) } finally { $brush.Dispose() }
        }

        # Put SQ4KOU in the former KE9NS position, using the same metallic/shadow
        # visual language as the PowerSDR title.  No KE9NS text remains.
        $font=[Drawing.Font]::new('Arial',30,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
        try{
            $shadow=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(100,15,18,22))
            try{ $g.DrawString('SQ4KOU',$font,$shadow,21,144) } finally { $shadow.Dispose() }

            $textRect=[Drawing.RectangleF]::new(18,140,190,42)
            $metal=[Drawing.Drawing2D.LinearGradientBrush]::new($textRect,[Drawing.Color]::White,[Drawing.Color]::FromArgb(145,145,145),90.0)
            try{ $g.DrawString('SQ4KOU',$font,$metal,18,140) } finally { $metal.Dispose() }
        }finally{
            $font.Dispose()
        }

        $output=[IO.MemoryStream]::new()
        try{
            $bmp.Save($output,[Drawing.Imaging.ImageFormat]::Png)
            $newPayload=[Convert]::ToBase64String($output.ToArray())
        }finally{
            $output.Dispose()
        }
    }finally{
        if($g){$g.Dispose()}
        if($bmp){$bmp.Dispose()}
    }
}finally{
    if($src){$src.Dispose()}
    $input.Dispose()
}

$resx=$bgRx.Replace(
    $resx,
    {param($x) $x.Groups[1].Value + [Environment]::NewLine + '        ' + $newPayload + [Environment]::NewLine + '    ' + $x.Groups[3].Value},
    1)
[IO.File]::WriteAllText($splashResx,$resx,$utf8)

# P20 added a second SQ4KOU label in the bottom-right corner. P21 disables it:
# the callsign now exists only in the former KE9NS position in the bitmap.
$designer=Read-Normal $splashDesigner
$designer=Replace-ExactOnce $designer @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
'@ @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCallsign.Visible = false; // P21: callsign is rendered in the former KE9NS position
'@ 'disable duplicate P20 callsign overlay'
Write-Normal $splashDesigner $designer

Stage 'PASS: KE9NS artwork removed; SQ4KOU rendered in its original position; dynamic progress/status controls preserved'
