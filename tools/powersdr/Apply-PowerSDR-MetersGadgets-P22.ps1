[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$m) { Write-Host "[P22-METERS-GADGETS] $m" }

$consoleDir = Join-Path $SourceRoot 'Console'
$setupCs    = Join-Path $consoleDir 'setup.cs'
$projectCs  = Join-Path $consoleDir 'PowerSDR.csproj'
$helperSrc  = Join-Path $PSScriptRoot 'SQ4KOUMetersGadgets.cs'
$helperDst  = Join-Path $consoleDir 'SQ4KOUMetersGadgets.cs'
$nl = [Environment]::NewLine

foreach($p in @($setupCs,$projectCs,$helperSrc)) {
    if(!(Test-Path -LiteralPath $p)) { throw "Required P22 input missing: $p" }
}

Stage "Copying isolated Meters/Gadgets module"
Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force

$utf8Bom = New-Object System.Text.UTF8Encoding($true)

# Hook only the Setup constructor. Runtime code discovers Appearance recursively,
# so generated designer files remain untouched.
$setup = [IO.File]::ReadAllText($setupCs)
$hook = 'SQ4KOUMetersGadgets.Install(console, this);'
if(!$setup.Contains($hook)) {
    $anchor = [regex]'(?m)^(\s*)console\s*=\s*c;\s*(?://[^\r\n]*)?$'
    $matches = $anchor.Matches($setup)
    if($matches.Count -ne 1) {
        throw "P22 Setup hook anchor count=$($matches.Count); expected 1"
    }

    $m = $matches[0]
    $indent = $m.Groups[1].Value
    $replacement = $m.Value + $nl + $indent + $hook + ' // SQ4KOU P22'
    $setup = $anchor.Replace(
        $setup,
        [System.Text.RegularExpressions.MatchEvaluator]{ param($x) $replacement },
        1)
    [IO.File]::WriteAllText($setupCs,$setup,$utf8Bom)
    Stage "Installed Setup -> Appearance runtime hook"
}
else {
    Stage "Setup hook already installed"
}

$project = [IO.File]::ReadAllText($projectCs)
if($project -notmatch 'Compile Include="SQ4KOUMetersGadgets\.cs"') {
    $anchor = [regex]'(<Compile Include="Skin\.cs"\s*/>)'
    $matches = $anchor.Matches($project)
    if($matches.Count -ne 1) {
        throw "P22 csproj Skin.cs anchor count=$($matches.Count); expected 1"
    }
    $insert = '$1' + $nl + '    <Compile Include="SQ4KOUMetersGadgets.cs" />'
    $project = $anchor.Replace($project,$insert,1)
}

# Zip extraction uses only framework assemblies. Do not introduce a new NuGet
# dependency or modify the native radio stack.
if($project -notmatch '<Reference Include="System\.IO\.Compression"') {
    $referenceAnchor = [regex]'(<Reference Include="System\.Drawing"\s*/>)'
    $matches = $referenceAnchor.Matches($project)
    if($matches.Count -ne 1) {
        throw "P22 System.Drawing reference anchor count=$($matches.Count); expected 1"
    }
    $refs = '$1' + $nl + '    <Reference Include="System.IO.Compression" />' +
                  $nl + '    <Reference Include="System.IO.Compression.FileSystem" />'
    $project = $referenceAnchor.Replace($project,$refs,1)
}
elseif($project -notmatch '<Reference Include="System\.IO\.Compression\.FileSystem"') {
    $compression = [regex]'(<Reference Include="System\.IO\.Compression"[^>]*?/>)'
    $matches = $compression.Matches($project)
    if($matches.Count -ne 1) {
        throw "P22 compression reference anchor count=$($matches.Count); expected 1"
    }
    $project = $compression.Replace(
        $project,
        '$1' + $nl + '    <Reference Include="System.IO.Compression.FileSystem" />',
        1)
}

[IO.File]::WriteAllText($projectCs,$project,$utf8Bom)
Stage "Registered P22 module and framework compression references"

# Hard implementation boundary: UI/read-only adapter only. DttSP meter reads are
# intentionally allowed because DttSP is the native PowerSDR DSP path.
$helper = [IO.File]::ReadAllText($helperDst)
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
    if($helper -match $token) {
        throw "P22 backend boundary failed: '$token' found in helper"
    }
}

if($helper -notmatch 'class\s+Flex5000MeterAdapter') { throw 'P22 adapter class missing' }
if($helper -notmatch 'CalculateRXMeter') { throw 'P22 RX meter source missing' }
if($helper -notmatch 'CalculateTXMeter') { throw 'P22 TX meter source missing' }
if($helper -notmatch 'oe3ide\.com') { throw 'P22 OE3IDE catalogue endpoint missing' }
if($setup -notmatch [regex]::Escape($hook)) { throw 'P22 Setup hook post-check failed' }
if($project -notmatch 'Compile Include="SQ4KOUMetersGadgets\.cs"') { throw 'P22 csproj post-check failed' }

Stage "PASS: functional Meters/Gadgets module applied"
