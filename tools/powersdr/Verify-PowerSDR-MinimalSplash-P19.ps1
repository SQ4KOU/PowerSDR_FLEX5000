[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P19-MINIMAL-SPLASH-VERIFY] $s" }

$splashCs       = Join-Path $SourceRoot 'Console\splash.cs'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$consoleCs      = Join-Path $SourceRoot 'Console\console.cs'
$presentationCs = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($splashCs,$splashDesigner,$consoleCs,$presentationCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P19 minimal splash verification input missing: $p" }
}

$splash=[IO.File]::ReadAllText($splashCs)
$designer=[IO.File]::ReadAllText($splashDesigner)
$console=[IO.File]::ReadAllText($consoleCs)
$presentation=[IO.File]::ReadAllText($presentationCs)

foreach($token in @(
    'pictureBox1.Visible = false;',
    'lblTimeRemaining.Visible = false;'
)){
    if(!$splash.Contains($token)){ throw "P19 splash marker missing: $token" }
}

foreach($token in @(
    'this.ClientSize = new System.Drawing.Size(520, 210);',
    'this.lblProduct.Text = "PowerSDR  •  FLEX-5000";',
    'this.BackColor = System.Drawing.Color.FromArgb(24, 24, 27);',
    'this.lblStatus.Text = "Starting...";'
)){
    if(!$designer.Contains($token)){ throw "P19 designer marker missing: $token" }
}

if($console.Contains('Splash.CloseForm();')){
    throw 'P19 constructor still closes splash before main UI reveal'
}
if(!$console.Contains('Splash.SetStatus("Finalizing Main Window")')){
    throw 'P19 finalizing splash status marker missing'
}

foreach($token in @(
    '"SPLASH_CLOSE_AT_REVEAL"',
    'Splash.CloseForm();',
    'form.Opacity = 1.0;'
)){
    if(!$presentation.Contains($token)){ throw "P19 presentation marker missing: $token" }
}

Stage 'PASS: minimalist splash retained until stable main UI reveal; no blank transition interval'
