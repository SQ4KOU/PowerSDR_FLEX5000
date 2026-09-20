[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$m){Write-Host "[P23-THETIS-METERS-1TO1] $m"}

$consoleDir=Join-Path $SourceRoot 'Console'
$setupCs=Join-Path $consoleDir 'setup.cs'
$projectCs=Join-Path $consoleDir 'PowerSDR.csproj'
$nl=[Environment]::NewLine
$utf8Bom=New-Object System.Text.UTF8Encoding($true)

$files=@(
 'SQ4KOUThetisMetersP23.Base.cs',
 'SQ4KOUThetisMetersP23.Core.cs',
 'SQ4KOUThetisMetersP23.Container.cs',
 'SQ4KOUThetisMetersP23.UI.cs'
)

foreach($name in $files){
    $src=Join-Path $PSScriptRoot $name
    if(!(Test-Path $src)){throw "P23 source missing: $src"}
    Copy-Item $src (Join-Path $consoleDir $name) -Force
}

$setup=[IO.File]::ReadAllText($setupCs)
$hook='SQ4KOUThetisMetersP23.Install(console, this);'
if(!$setup.Contains($hook)){
    $anchor=[regex]'(?m)^(\s*)console\s*=\s*c;\s*(?://[^\r\n]*)?$'
    $m=$anchor.Matches($setup)
    if($m.Count -ne 1){throw "P23 Setup hook anchor count=$($m.Count); expected 1"}
    $indent=$m[0].Groups[1].Value
    $replacement=$m[0].Value+$nl+$indent+$hook+' // SQ4KOU P23 Thetis Meters/Gadgets 1:1'
    $setup=$anchor.Replace($setup,[System.Text.RegularExpressions.MatchEvaluator]{param($x)$replacement},1)
    [IO.File]::WriteAllText($setupCs,$setup,$utf8Bom)
}
Stage 'Installed Setup -> Appearance P23 hook'

$project=[IO.File]::ReadAllText($projectCs)
$compileAnchor=[regex]'(<Compile Include="Skin\.cs"\s*/>)'
$m=$compileAnchor.Matches($project)
if($m.Count -ne 1){throw "P23 csproj Skin.cs anchor count=$($m.Count); expected 1"}
foreach($name in $files){
    if($project -notmatch ('Compile Include="'+[regex]::Escape($name)+'"')){
        $insert='$1'+$nl+'    <Compile Include="'+$name+'" />'
        $project=$compileAnchor.Replace($project,$insert,1)
    }
}

if($project -notmatch '<Reference Include="System\.IO\.Compression"'){
    $referenceAnchor=[regex]'(?s)(<Reference Include="System\.Drawing">.*?</Reference>)'
    $m=$referenceAnchor.Matches($project)
    if($m.Count -ne 1){throw "P23 System.Drawing reference anchor count=$($m.Count); expected 1"}
    $refs='$1'+$nl+'    <Reference Include="System.IO.Compression" />'+
          $nl+'    <Reference Include="System.IO.Compression.FileSystem" />'
    $project=$referenceAnchor.Replace($project,$refs,1)
}
elseif($project -notmatch '<Reference Include="System\.IO\.Compression\.FileSystem"'){
    $compression=[regex]'(<Reference Include="System\.IO\.Compression"[^>]*?/>)'
    $m=$compression.Matches($project)
    if($m.Count -ne 1){throw "P23 compression anchor count=$($m.Count); expected 1"}
    $project=$compression.Replace($project,'$1'+$nl+'    <Reference Include="System.IO.Compression.FileSystem" />',1)
}

[IO.File]::WriteAllText($projectCs,$project,$utf8Bom)

$joined=''
foreach($name in $files){$joined += [IO.File]::ReadAllText((Join-Path $consoleDir $name))+$nl}
foreach($token in @('NetworkIO','ChannelMaster','Protocol1','Protocol2','RedPitaya','PortAudio','FWC\.Set','PAL\.')){
    if($joined -match $token){throw "P23 backend boundary failed: '$token' found in compiled P23 layer"}
}
foreach($required in @(
 'class P23MeterManager',
 'class P23MeterContainer',
 'class P23MeterSurface',
 'class P23MetersSetupPanel',
 'class P23MultiMeterIO',
 'class P23RadioAdapter',
 'SQ4KOUThetisMetersP23.Install',
 'CalculateRXMeter',
 'CalculateTXMeter',
 'FWCPAPower',
 'FWCSWR',
 'Meters/Gadgets',
 'OE3IDE'
)){
    if($joined -notmatch [regex]::Escape($required)){throw "P23 required implementation marker missing: $required"}
}

Stage 'PASS: complete P23 compatibility layer copied; native backend untouched'
