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
$assetB64       = Join-Path $PSScriptRoot 'resources\PowerSDR-SQ4KOU-P21-600x384.jpg.b64'

foreach($p in @($splashResx,$splashDesigner,$assetB64)){
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

# Exact user-approved artwork, normalized to the native PowerSDR splash size.
$payload = ([IO.File]::ReadAllText($assetB64) -replace '\s','')
if(!$payload.StartsWith('/9j/')){ throw 'P21 approved artwork is not JPEG base64' }

$bytes=[Convert]::FromBase64String($payload)
$sha=[BitConverter]::ToString(([Security.Cryptography.SHA256]::Create()).ComputeHash($bytes)).Replace('-','').ToLowerInvariant()
$expectedSha='08fb326d6c2b5847fd4e9ca5484330e77cb8ccb997bc4d497291772596c32a54'
if($sha -ne $expectedSha){ throw "P21 artwork SHA256 mismatch: $sha" }

Add-Type -AssemblyName System.Drawing
$ms=New-Object IO.MemoryStream(,$bytes)
try{
    $img=[Drawing.Image]::FromStream($ms)
    if($img.Width -ne 600 -or $img.Height -ne 384){
        throw "P21 artwork dimensions invalid: $($img.Width)x$($img.Height)"
    }
}finally{
    if($img){$img.Dispose()}
    $ms.Dispose()
}

$resx=[IO.File]::ReadAllText($splashResx)
$bgRx=[regex]::new('(?s)(<data name="\$this\.BackgroundImage"[^>]*>.*?<value>)(.*?)(</value>.*?</data>)')
$matches=$bgRx.Matches($resx)
if($matches.Count -ne 1){ throw "P21 BackgroundImage count=$($matches.Count), expected 1 after P20" }
$resx=$bgRx.Replace($resx,{param($m) $m.Groups[1].Value + [Environment]::NewLine + '        ' + $payload + [Environment]::NewLine + '    ' + $m.Groups[3].Value},1)
[IO.File]::WriteAllText($splashResx,$resx,$utf8)

# SQ4KOU is now part of the approved artwork in the former KE9NS position.
# Disable P20's extra bottom-right label so there is exactly one callsign.
$designer=Read-Normal $splashDesigner
$designer=Replace-ExactOnce $designer @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
'@ @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCallsign.Visible = false; // P21: callsign is baked into approved artwork
'@ 'hide P20 duplicate callsign overlay'

Write-Normal $splashDesigner $designer

Stage "PASS: approved SQ4KOU 600x384 splash installed; dynamic progress/status controls preserved; SHA256=$sha"
