[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
function Stage([string]$s){ Write-Host "[SQ4KOU-P21-FINAL2-SPLASH] $s" }

$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$assetParts = 1..5 | ForEach-Object { Join-Path $PSScriptRoot ("resources\P21-FINAL2-splash.part{0}.b64" -f $_) }

foreach($p in @($splashResx,$splashDesigner)+$assetParts){
    if(!(Test-Path -LiteralPath $p)){ throw "P21 FINAL2 splash input missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13)+([string][char]10)
$lf = [string][char]10
function Read-Normal([string]$p){ return [IO.File]::ReadAllText($p).Replace($crlf,$lf) }
function Write-Normal([string]$p,[string]$s){ [IO.File]::WriteAllText($p,$s.Replace($lf,$crlf),$utf8) }
function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace($crlf,$lf); $New=$New.Replace($crlf,$lf)
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P21 FINAL2 anchor missing: $Label" }
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){ throw "P21 FINAL2 anchor not unique: $Label" }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

$payload=''
foreach($p in $assetParts){ $payload += ([IO.File]::ReadAllText($p) -replace '\s','') }
if($payload.Length -ne 17620){ throw "P21 FINAL2 base64 length mismatch: $($payload.Length)" }
if(!$payload.StartsWith('/9j/')){ throw 'P21 FINAL2 artwork is not JPEG base64' }

$bytes=[Convert]::FromBase64String($payload)
$sha=[BitConverter]::ToString(([Security.Cryptography.SHA256]::Create()).ComputeHash($bytes)).Replace('-','').ToLowerInvariant()
$expectedSha='15cf71be0bf24e2b533ea0ca73807490296c156f3ee3e571ca18bf458b2a2a77'
if($sha -ne $expectedSha){ throw "P21 FINAL2 artwork SHA256 mismatch: $sha" }

Add-Type -AssemblyName System.Drawing
$img=$null
$ms=New-Object IO.MemoryStream(,$bytes)
try{
    $img=[Drawing.Image]::FromStream($ms)
    if($img.Width -ne 600 -or $img.Height -ne 384){ throw "P21 FINAL2 artwork dimensions invalid: $($img.Width)x$($img.Height)" }
}finally{
    if($img){$img.Dispose()}
    $ms.Dispose()
}

$resx=[IO.File]::ReadAllText($splashResx)
$bgRx=[regex]::new('(?s)(<data name="\$this\.BackgroundImage"[^>]*>.*?<value>)(.*?)(</value>.*?</data>)')
$matches=$bgRx.Matches($resx)
if($matches.Count -ne 1){ throw "P21 FINAL2 BackgroundImage count=$($matches.Count), expected 1 after P20" }
$resx=$bgRx.Replace($resx,{param($m) $m.Groups[1].Value+[Environment]::NewLine+'        '+$payload+[Environment]::NewLine+'    '+$m.Groups[3].Value},1)
[IO.File]::WriteAllText($splashResx,$resx,$utf8)

$designer=Read-Normal $splashDesigner
$designer=Replace-ExactOnce $designer @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
'@ @'
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCallsign.Visible = false; // P21 FINAL2: SQ4KOU is baked into approved artwork
'@ 'hide duplicate P20 callsign overlay'
Write-Normal $splashDesigner $designer

Stage "PASS: approved SQ4KOU splash embedded 600x384; dynamic P20 status/progress preserved; SHA256=$sha"
