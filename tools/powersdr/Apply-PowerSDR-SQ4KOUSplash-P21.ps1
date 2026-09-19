[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
function Stage([string]$s){ Write-Host "[SQ4KOU-P21-SPLASH] $s" }

$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$parts = 1..4 | ForEach-Object { Join-Path $PSScriptRoot ("resources\P21-SQ4KOU-splash-q60.part{0}.b64" -f $_) }

foreach($p in @($splashResx,$splashDesigner)+$parts){
    if(!(Test-Path -LiteralPath $p)){ throw "P21 input missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13)+([string][char]10)
$lf = [string][char]10

function Read-Normal([string]$p){ [IO.File]::ReadAllText($p).Replace($crlf,$lf) }
function Write-Normal([string]$p,[string]$s){ [IO.File]::WriteAllText($p,$s.Replace($lf,$crlf),$utf8) }
function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace($crlf,$lf); $New=$New.Replace($crlf,$lf)
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P21 anchor missing: $Label" }
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){ throw "P21 anchor not unique: $Label" }
    $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

$payload = (($parts | ForEach-Object { [IO.File]::ReadAllText($_) }) -join '') -replace '\s',''
if(!$payload.StartsWith('/9j/')){ throw 'P21 artwork is not JPEG base64' }
$bytes=[Convert]::FromBase64String($payload)
$sha=[BitConverter]::ToString(([Security.Cryptography.SHA256]::Create()).ComputeHash($bytes)).Replace('-','').ToLowerInvariant()
$expected='09ddb839cda31d421439f7872fdc70b0ab469297c5338aa1f43ff2153c9505e4'
if($sha -ne $expected){ throw "P21 artwork SHA256 mismatch: $sha" }

Add-Type -AssemblyName System.Drawing
$ms=New-Object IO.MemoryStream(,$bytes)
$img=$null
try{
    $img=[Drawing.Image]::FromStream($ms)
    if($img.Width -ne 600 -or $img.Height -ne 384){ throw "P21 artwork dimensions invalid: $($img.Width)x$($img.Height)" }
} finally {
    if($img){$img.Dispose()}
    $ms.Dispose()
}

$resx=[IO.File]::ReadAllText($splashResx)
$rx=[regex]::new('(?s)(<data name="\$this\.BackgroundImage"[^>]*>.*?<value>)(.*?)(</value>.*?</data>)')
$m=$rx.Matches($resx)
if($m.Count -ne 1){ throw "P21 BackgroundImage count=$($m.Count), expected 1 after P20" }
$resx=$rx.Replace($resx,{param($x) $x.Groups[1].Value+[Environment]::NewLine+'        '+$payload+[Environment]::NewLine+'    '+$x.Groups[3].Value},1)
[IO.File]::WriteAllText($splashResx,$resx,$utf8)

$designer=Read-Normal $splashDesigner
$designer=Replace-ExactOnce $designer @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
'@ @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCallsign.Visible = false; // P21: callsign is in approved artwork
'@ 'hide duplicate P20 callsign'
Write-Normal $splashDesigner $designer

Stage "PASS: approved SQ4KOU background installed (600x384, SHA256=$sha); native dynamic status/progress retained"
