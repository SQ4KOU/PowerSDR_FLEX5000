[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P14-STABILITY-BASELINE] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
$spotCs    = Join-Path $SourceRoot 'Console\spot.cs'
foreach($p in @($consoleCs,$spotCs)) {
    if(!(Test-Path -LiteralPath $p)) { throw "P14 source missing: $p" }
}

$console = [IO.File]::ReadAllText($consoleCs)
$spot    = [IO.File]::ReadAllText($spotCs)

$forbidden = @(
    'sq4kouDeferredAutoStart',
    'sq4kouShownOnce',
    'protected override void OnShown(EventArgs e)',
    'SQ4KOU P09: enable the existing WinForms double-buffer policy',
    'sq4kouSpotControlLoadDone',
    'IntPtr sq4kouSpotFormHandle',
    'SpotForm.SpotControl_Load(SpotForm, EventArgs.Empty);'
)

$found = @($forbidden | Where-Object { $console.Contains($_) -or $spot.Contains($_) })
if($found.Count -gt 0) {
    throw ('P14 forbidden P09/P10 markers still present: ' + ($found -join ', '))
}

if(!$console.Contains('SQ4KOU P13: stable pacing')) {
    throw 'P14 expected P13 stable display pacing marker missing'
}
if(!$console.Contains('draw_display_thread.Priority = ThreadPriority.BelowNormal;')) {
    throw 'P14 expected BelowNormal display priority missing'
}
if(!$console.Contains('Thread.Sleep(display_delay);')) {
    throw 'P14 expected stable display sleep missing'
}

Stage 'PASS: P09/P10 fully absent; P03-P08 retained; P13 stable renderer retained'
