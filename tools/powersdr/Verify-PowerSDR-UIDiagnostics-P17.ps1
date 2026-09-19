[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P17-UI-DIAG-VERIFY] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
$projectCs = Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$helperCs  = Join-Path $SourceRoot 'Console\SQ4KOUUIDiagnostics.cs'

foreach($p in @($consoleCs,$projectCs,$helperCs)) {
    if(!(Test-Path -LiteralPath $p)) { throw "P17 verification input missing: $p" }
}

$console = [IO.File]::ReadAllText($consoleCs)
$project = [IO.File]::ReadAllText($projectCs)
$helper  = [IO.File]::ReadAllText($helperCs)

$requiredConsole = @(
    'SQ4KOUUIDiagnostics.StartEarly();',
    'SQ4KOUUIDiagnostics.AttachUI(this);',
    'SQ4KOUUIDiagnostics.InvalidateRequested();',
    'SQ4KOUUIDiagnostics.ProducerTick();',
    'SQ4KOUUIDiagnostics.PaintBegin();',
    'SQ4KOUUIDiagnostics.RenderBegin();',
    '"AUTOSTART_POWER_ON"',
    '"SET_BAND"',
    '"SET_RX1_BAND"',
    '"CONSOLE_RESIZE"'
)

$missing = @($requiredConsole | Where-Object { !$console.Contains($_) })
if($missing.Count -gt 0) {
    throw ('P17 console markers missing: ' + ($missing -join ', '))
}

if(!$project.Contains('Compile Include="SQ4KOUUIDiagnostics.cs"')) {
    throw 'P17 helper is not registered in PowerSDR.csproj'
}

$requiredHelper = @(
    'ConcurrentQueue<string>',
    'Application.AddMessageFilter',
    'BeginInvoke(new MethodInvoker(UiPingAck))',
    '"STALL"',
    '"PERF"',
    'GetGuiResources',
    'SLOW_PAINT',
    'SLOW_RENDER'
)
$missingHelper = @($requiredHelper | Where-Object { !$helper.Contains($_) })
if($missingHelper.Count -gt 0) {
    throw ('P17 helper markers missing: ' + ($missingHelper -join ', '))
}

# Diagnostic build must not reintroduce the rejected startup masking experiments.
$forbidden = @(
    'SQ4KOU_WM_SETREDRAW',
    'sq4kouDeferredStartupContinuation',
    'sq4kouDeferredAutoStart',
    'sq4kouSpotFormHandle'
)
$found = @($forbidden | Where-Object { $console.Contains($_) -or $helper.Contains($_) })
if($found.Count -gt 0) {
    throw ('P17 forbidden startup experiment marker present: ' + ($found -join ', '))
}

# The helper is observation-only and must stay out of the FLEX backend.
$backendTokens = @('FWC.', 'Pal.', 'PAL.', 'NetworkIO', 'ChannelMaster', 'PortAudio.Start', 'Audio.Start')
$crossed = @($backendTokens | Where-Object { $helper.Contains($_) })
if($crossed.Count -gt 0) {
    throw ('P17 diagnostics crossed backend boundary: ' + ($crossed -join ', '))
}

Stage 'PASS: P17 instrumentation present, nonblocking writer/watchdog enabled, native backend boundary retained'
