[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P20-ORIGINAL-FLEX-SPLASH-VERIFY] $s" }

$splashCs       = Join-Path $SourceRoot 'Console\splash.cs'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$consoleCs      = Join-Path $SourceRoot 'Console\console.cs'
$presentationCs = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($splashCs,$splashDesigner,$splashResx,$consoleCs,$presentationCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P20 verification input missing: $p" }
}

$splash=[IO.File]::ReadAllText($splashCs)
$designer=[IO.File]::ReadAllText($splashDesigner)
$resx=[IO.File]::ReadAllText($splashResx)
$console=[IO.File]::ReadAllText($consoleCs)
$presentation=[IO.File]::ReadAllText($presentationCs)

foreach($token in @(
    'SQ4KOU P20: restore the original FlexRadio PowerSDR splash artwork.',
    'pictureBox1.Visible = false;'
)){
    if(!$splash.Contains($token)){ throw "P20 splash marker missing: $token" }
}
foreach($forbidden in @('Properties.Resources.moonearth6','Properties.Resources.moonearth2','Random random = new Random()')){
    if($splash.Contains($forbidden)){ throw "P20 KE9NS splash code still present: $forbidden" }
}

foreach($token in @(
    'this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));',
    'this.ClientSize = new System.Drawing.Size(600, 384);',
    'this.textBox1.Visible = false;',
    'this.pictureBox1.Visible = false;',
    'this.lblCallsign.Text = "SQ4KOU";',
    'this.pnlStatus.Location = new System.Drawing.Point(50, 247);',
    'this.lblStatus.Location = new System.Drawing.Point(0, 287);'
)){
    if(!$designer.Contains($token)){ throw "P20 designer marker missing: $token" }
}

$bgRx=[regex]::new('(?s)<data name="\$this\.BackgroundImage"[^>]*>.*?<value>(.*?)</value>.*?</data>')
$m=$bgRx.Match($resx)
if(!$m.Success){ throw 'P20 splash BackgroundImage resource missing' }
$payload=($m.Groups[1].Value -replace '\s','')
if(!$payload.StartsWith('iVBORw0KGgo')){ throw 'P20 splash BackgroundImage is not PNG' }
if($payload.Length -lt 100000 -or $payload.Length -gt 250000){
    throw "P20 Flex splash PNG payload length unexpected: $($payload.Length)"
}

if($console.Contains('Splash.CloseForm();')){
    throw 'P20 constructor still closes splash before main UI reveal'
}
if(!$console.Contains('Splash.SetStatus("Finalizing Main Window")')){
    throw 'P20 finalizing splash status marker missing'
}
foreach($token in @('"FLEX_SPLASH_CLOSE_AT_REVEAL"','Splash.CloseForm();','form.Opacity = 1.0;')){
    if(!$presentation.Contains($token)){ throw "P20 presentation marker missing: $token" }
}

Stage "PASS: original FlexRadio 600x384 splash installed; SQ4KOU overlay present; PNG payload chars=$($payload.Length); P19 reveal/audio ordering preserved"
