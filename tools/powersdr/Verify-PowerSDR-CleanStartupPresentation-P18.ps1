[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P18-CLEAN-STARTUP-VERIFY] $s" }

$consoleCs=Join-Path $SourceRoot 'Console\console.cs'
$projectCs=Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$helperCs=Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($consoleCs,$projectCs,$helperCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P18 verification input missing: $p" }
}

$console=[IO.File]::ReadAllText($consoleCs)
$project=[IO.File]::ReadAllText($projectCs)
$helper=[IO.File]::ReadAllText($helperCs)

foreach($token in @(
    'SQ4KOUStartupPresentation.Install(this, picDisplay);'
)){
    if(!$console.Contains($token)){ throw "P18 console marker missing: $token" }
}

if(!$project.Contains('Compile Include="SQ4KOUStartupPresentation.cs"')){
    throw 'P18 helper registration missing'
}

foreach($token in @(
    'form.Opacity = 0.0;',
    'display.Paint += delegate',
    'Application.Idle += idleHandler;',
    'form.Opacity = 1.0;',
    '"PRESENTATION_GATE_ARMED"',
    '"PRESENTATION_REVEAL"',
    'first_paint_plus_idle'
)){
    if(!$helper.Contains($token)){ throw "P18 helper marker missing: $token" }
}

foreach($forbidden in @(
    'WM_SETREDRAW',
    'SuspendLayout()',
    'ResumeLayout(',
    'PerformLayout()',
    'FWC.',
    'PAL.',
    'Audio.Start',
    'DB.SaveVars'
)){
    if($helper.Contains($forbidden)){ throw "P18 forbidden token present: $forbidden" }
}

Stage 'PASS: presentation-only gate present; no backend/database/layout suppression code introduced'
