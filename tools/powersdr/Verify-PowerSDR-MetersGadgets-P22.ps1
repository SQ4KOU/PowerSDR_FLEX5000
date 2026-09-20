[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Require([bool]$ok,[string]$message) {
    if(!$ok) { throw "P22 VERIFY: $message" }
}

$consoleDir = Join-Path $SourceRoot 'Console'
$setupCs   = Join-Path $consoleDir 'setup.cs'
$projectCs = Join-Path $consoleDir 'PowerSDR.csproj'
$helperCs  = Join-Path $consoleDir 'SQ4KOUMetersGadgets.cs'

foreach($p in @($setupCs,$projectCs,$helperCs)) {
    Require (Test-Path -LiteralPath $p) "missing $p"
}

$setup   = [IO.File]::ReadAllText($setupCs)
$project = [IO.File]::ReadAllText($projectCs)
$helper  = [IO.File]::ReadAllText($helperCs)

Require (([regex]::Matches($setup,[regex]::Escape('SQ4KOUMetersGadgets.Install(console, this);'))).Count -eq 1) 'Setup hook must occur exactly once'
Require (([regex]::Matches($project,'Compile Include="SQ4KOUMetersGadgets\.cs"')).Count -eq 1) 'helper Compile item must occur exactly once'
Require ($helper -match 'class\s+MetersGadgetsForm') 'MetersGadgetsForm missing'
Require ($helper -match 'class\s+Flex5000MeterAdapter') 'FLEX5000 adapter missing'
Require ($helper -match 'CalculateRXMeter\(0,\s*0,\s*DttSP\.MeterType\.SIGNAL_STRENGTH\)') 'native RX signal read missing'
Require ($helper -match 'CalculateTXMeter') 'native TX meter reads missing'
Require ($helper -match 'VFOAFreq') 'VFO A mapping missing'
Require ($helper -match 'VFOBFreq') 'VFO B mapping missing'
Require ($helper -match 'FWCPAPower') 'native forward-power conversion mapping missing'
Require ($helper -match 'FWCSWR') 'native SWR mapping missing'
Require ($helper -match 'layout\.json') 'layout persistence missing'
Require ($helper -match 'File\.Replace') 'atomic layout save path missing'
Require ($helper -match 'oe3ide\.com') 'OE3IDE catalogue URL missing'
Require ($helper -match 'SafeExtractZip') 'safe meter skin extraction missing'
Require ($helper -match 'StartsWith\(root,\s*StringComparison\.OrdinalIgnoreCase\)') 'zip traversal guard missing'
Require ($project -match '<Reference Include="System\.IO\.Compression') 'compression reference missing'
Require ($project -match '<Reference Include="System\.IO\.Compression\.FileSystem') 'compression filesystem reference missing'

$forbidden = @(
    'NetworkIO',
    'ChannelMaster',
    'Protocol1',
    'Protocol2',
    'RedPitaya',
    'HPSDR',
    'PortAudio',
    'FWC\.Set',
    'PAL\.'
)
foreach($token in $forbidden) {
    Require ($helper -notmatch $token) "backend isolation token found: $token"
}

# P22 must not touch generated Setup designer/resources or native hardware/audio/DSP source.
$unexpected = @(
    'setup.Designer.cs',
    'setup.resx',
    'FWC\fwc.cs',
    'FWC\fwcatuform.cs',
    'audio.cs',
    'dttsp.cs'
)
foreach($rel in $unexpected) {
    Require (Test-Path (Join-Path $consoleDir $rel)) "baseline native file missing: $rel"
}

Write-Host 'P22_METERS_GADGETS_VERIFY=PASS'
Write-Host 'P22_DATA=POWERSDR_DTTSP_NATIVE'
Write-Host 'P22_OE3IDE=CATALOG_DOWNLOAD_SAFE_EXTRACT_IMAGE_ASSET'
Write-Host 'P22_BACKEND_REPLACEMENT=NONE'
