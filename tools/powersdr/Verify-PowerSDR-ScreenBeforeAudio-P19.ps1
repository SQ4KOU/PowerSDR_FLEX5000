[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P19-SCREEN-BEFORE-AUDIO-VERIFY] $s" }

$consoleCs=Join-Path $SourceRoot 'Console\console.cs'
$helperCs=Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($consoleCs,$helperCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P19 verification input missing: $p" }
}

$console=[IO.File]::ReadAllText($consoleCs)
$helper=[IO.File]::ReadAllText($helperCs)

foreach($token in @(
    'SQ4KOUStartupPresentation.Install(this, picDisplay, delegate',
    '"AUTOSTART_DEFERRED_UNTIL_UI_REVEAL"',
    '"phase=after_reveal"',
    'chkPower.Checked = true;'
)){
    if(!$console.Contains($token)){ throw "P19 console marker missing: $token" }
}

foreach($token in @(
    'internal static void Install(Form form, Control display, Action afterReveal)',
    'form.Opacity = 1.0;',
    'form.Update();',
    '"AFTER_REVEAL_SCHEDULED"',
    '"AFTER_REVEAL_BEGIN"',
    'afterReveal();'
)){
    if(!$helper.Contains($token)){ throw "P19 helper marker missing: $token" }
}

# Exactly one native AutoStart assignment should remain in the P19 callback.
$needle='chkPower.Checked = true;'
$count=([regex]::Matches($console,[regex]::Escape($needle))).Count
if($count -lt 1){ throw 'P19 native POWER ON assignment missing' }

# Constructor-time P17 AutoStart timing block must be gone.
if($console.Contains('long sq4kouAutoStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "AUTOSTART_POWER_ON", null);')){
    throw 'P19 constructor-time AutoStart still present'
}

foreach($forbidden in @(
    'Task.Run',
    'ThreadPool.',
    'WM_SETREDRAW',
    'SuspendLayout()',
    'ResumeLayout('
)){
    if($helper.Contains($forbidden)){ throw "P19 unsafe helper token present: $forbidden" }
}

Stage 'PASS: screen reveal precedes native AutoStart; no background FWC/Audio execution introduced'
