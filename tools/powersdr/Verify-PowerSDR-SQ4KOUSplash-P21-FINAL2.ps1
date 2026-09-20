[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
function Stage([string]$s){ Write-Host "[SQ4KOU-P21-FINAL2-SPLASH-VERIFY] $s" }

$splashCs       = Join-Path $SourceRoot 'Console\splash.cs'
$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$consoleCs      = Join-Path $SourceRoot 'Console\console.cs'
$presentationCs = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'
foreach($p in @($splashCs,$splashResx,$splashDesigner,$consoleCs,$presentationCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P21 FINAL2 verify input missing: $p" }
}

$splash=[IO.File]::ReadAllText($splashCs)
$resx=[IO.File]::ReadAllText($splashResx)
$designer=[IO.File]::ReadAllText($splashDesigner)
$console=[IO.File]::ReadAllText($consoleCs)
$presentation=[IO.File]::ReadAllText($presentationCs)

$bgRx=[regex]::new('(?s)<data name="\$this\.BackgroundImage"[^>]*>.*?<value>(.*?)</value>.*?</data>')
$m=$bgRx.Match($resx)
if(!$m.Success){ throw 'P21 FINAL2 BackgroundImage resource missing' }
$payload=($m.Groups[1].Value -replace '\s','')
if($payload.Length -ne 17620){ throw "P21 FINAL2 embedded base64 length mismatch: $($payload.Length)" }
$bytes=[Convert]::FromBase64String($payload)
$sha=[BitConverter]::ToString(([Security.Cryptography.SHA256]::Create()).ComputeHash($bytes)).Replace('-','').ToLowerInvariant()
$expectedSha='15cf71be0bf24e2b533ea0ca73807490296c156f3ee3e571ca18bf458b2a2a77'
if($sha -ne $expectedSha){ throw "P21 FINAL2 embedded artwork SHA256 mismatch: $sha" }

Add-Type -AssemblyName System.Drawing
$img=$null
$ms=New-Object IO.MemoryStream(,$bytes)
try{
    $img=[Drawing.Image]::FromStream($ms)
    if($img.Width -ne 600 -or $img.Height -ne 384){ throw "P21 FINAL2 embedded dimensions invalid: $($img.Width)x$($img.Height)" }
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
)){ if(!$designer.Contains($token)){ throw "P21 FINAL2 designer marker missing: $token" } }

foreach($forbidden in @('Properties.Resources.moonearth6','Properties.Resources.moonearth2','Random random = new Random()')){
    if($splash.Contains($forbidden)){ throw "P21 FINAL2 KE9NS random splash code returned: $forbidden" }
}

if($console.Contains('Splash.CloseForm();')){ throw 'P21 FINAL2 constructor closes splash before stable UI reveal' }
if(!$console.Contains('Splash.SetStatus("Finalizing Main Window")')){ throw 'P21 FINAL2 finalizing marker missing' }
foreach($token in @('"FLEX_SPLASH_CLOSE_AT_REVEAL"','Splash.CloseForm();','form.Opacity = 1.0;')){
    if(!$presentation.Contains($token)){ throw "P21 FINAL2 startup ordering marker missing: $token" }
}

Stage "PASS: exact SQ4KOU splash embedded and verified; stable reveal-before-audio preserved; SHA256=$sha"
