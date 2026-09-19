[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P21-FINAL-SPLASH-VERIFY] $s" }

$splashCs       = Join-Path $SourceRoot 'Console\splash.cs'
$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$consoleCs      = Join-Path $SourceRoot 'Console\console.cs'
$presentationCs = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($splashCs,$splashResx,$splashDesigner,$consoleCs,$presentationCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P21 final verify input missing: $p" }
}

$splash=[IO.File]::ReadAllText($splashCs)
$resx=[IO.File]::ReadAllText($splashResx)
$designer=[IO.File]::ReadAllText($splashDesigner)
$console=[IO.File]::ReadAllText($consoleCs)
$presentation=[IO.File]::ReadAllText($presentationCs)

$bgRx=[regex]::new('(?s)<data name="\$this\.BackgroundImage"[^>]*>.*?<value>(.*?)</value>.*?</data>')
$m=$bgRx.Match($resx)
if(!$m.Success){ throw 'P21 final BackgroundImage resource missing' }

$payload=($m.Groups[1].Value -replace '\s','')
if(!$payload.StartsWith('/9j/')){ throw 'P21 final BackgroundImage is not JPEG' }

$bytes=[Convert]::FromBase64String($payload)
$sha=[BitConverter]::ToString(([Security.Cryptography.SHA256]::Create()).ComputeHash($bytes)).Replace('-','').ToLowerInvariant()
$expectedSha='e825c4d36cc80bb20b64217d3a0a94a07980a2d41f24241fb129b0d74eb28326'
if($sha -ne $expectedSha){ throw "P21 final embedded artwork SHA256 mismatch: $sha" }

Add-Type -AssemblyName System.Drawing
$img=$null
$ms=New-Object IO.MemoryStream(,$bytes)
try{
    $img=[Drawing.Image]::FromStream($ms)
    if($img.Width -ne 600 -or $img.Height -ne 384){
        throw "P21 final embedded artwork dimensions invalid: $($img.Width)x$($img.Height)"
    }
}finally{
    if($img){$img.Dispose()}
    $ms.Dispose()
}

foreach($token in @(
    'this.ClientSize = new System.Drawing.Size(600, 384);',
    'this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));',
    'this.lblCallsign.Visible = false;',
    'this.pnlStatus.Location = new System.Drawing.Point(50, 247);',
    'this.pnlStatus.Size = new System.Drawing.Size(475, 24);',
    'this.lblStatus.Location = new System.Drawing.Point(0, 287);',
    'this.lblTimeRemaining.Location = new System.Drawing.Point(296, 289);'
)){
    if(!$designer.Contains($token)){ throw "P21 final designer marker missing: $token" }
}

foreach($forbidden in @(
    'Properties.Resources.moonearth6',
    'Properties.Resources.moonearth2',
    'Random random = new Random()'
)){
    if($splash.Contains($forbidden)){ throw "P21 final KE9NS random splash code returned: $forbidden" }
}

if($console.Contains('Splash.CloseForm();')){
    throw 'P21 final constructor closes splash before stable main UI reveal'
}
if(!$console.Contains('Splash.SetStatus("Finalizing Main Window")')){
    throw 'P21 final splash status marker missing'
}
foreach($token in @('"FLEX_SPLASH_CLOSE_AT_REVEAL"','Splash.CloseForm();','form.Opacity = 1.0;')){
    if(!$presentation.Contains($token)){ throw "P21 final startup ordering marker missing: $token" }
}

Stage "PASS: P21 FINAL exact SQ4KOU splash embedded 600x384; duplicate overlay hidden; stable reveal-before-audio preserved; SHA256=$sha"
