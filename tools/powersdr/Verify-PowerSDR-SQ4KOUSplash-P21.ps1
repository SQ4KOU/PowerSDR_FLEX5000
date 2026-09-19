[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P21-SPLASH-VERIFY] $s" }

$splashCs       = Join-Path $SourceRoot 'Console\splash.cs'
$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$consoleCs      = Join-Path $SourceRoot 'Console\console.cs'
$presentationCs = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($splashCs,$splashResx,$splashDesigner,$consoleCs,$presentationCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P21 verify input missing: $p" }
}

$splash=[IO.File]::ReadAllText($splashCs)
$resx=[IO.File]::ReadAllText($splashResx)
$designer=[IO.File]::ReadAllText($splashDesigner)
$console=[IO.File]::ReadAllText($consoleCs)
$presentation=[IO.File]::ReadAllText($presentationCs)

$bgRx=[regex]::new('(?s)<data name="\$this\.BackgroundImage"[^>]*>.*?<value>(.*?)</value>.*?</data>')
$m=$bgRx.Match($resx)
if(!$m.Success){ throw 'P21 BackgroundImage resource missing' }
$payload=($m.Groups[1].Value -replace '\s','')
if(!$payload.StartsWith('iVBORw0KGgo')){ throw 'P21 final BackgroundImage is not PNG' }

Add-Type -AssemblyName System.Drawing
$bytes=[Convert]::FromBase64String($payload)
$ms=New-Object IO.MemoryStream(,$bytes)
try{
    $img=[Drawing.Image]::FromStream($ms)
    if($img.Width -ne 600 -or $img.Height -ne 384){
        throw "P21 final splash dimensions invalid: $($img.Width)x$($img.Height)"
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
    if(!$designer.Contains($token)){ throw "P21 designer marker missing: $token" }
}

foreach($forbidden in @('Properties.Resources.moonearth6','Properties.Resources.moonearth2','Random random = new Random()')){
    if($splash.Contains($forbidden)){ throw "P21 KE9NS random splash code returned: $forbidden" }
}

# P21 is presentation-only. Preserve the tested P19/P20 startup sequence.
if($console.Contains('Splash.CloseForm();')){
    throw 'P21 constructor closes splash before stable UI reveal'
}
if(!$console.Contains('Splash.SetStatus("Finalizing Main Window")')){
    throw 'P21 finalizing splash status marker missing'
}
foreach($token in @('"FLEX_SPLASH_CLOSE_AT_REVEAL"','Splash.CloseForm();','form.Opacity = 1.0;')){
    if(!$presentation.Contains($token)){ throw "P21 presentation marker missing: $token" }
}

Stage 'PASS: 600x384 P21 splash present; duplicate overlay disabled; dynamic status/progress and P19 reveal-before-audio preserved'
