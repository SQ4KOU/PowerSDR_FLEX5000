[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P26 is deliberately a thin UI layer over the verified P25 direct Thetis meter core.
# First apply P25 unchanged, then add only the standalone configuration surface.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P25.ps1') -SourceRoot $SourceRoot

$consoleDir=Join-Path $SourceRoot 'Console'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$srcConfig=Join-Path $PSScriptRoot 'P26ThetisMetersConfigForm.cs'
$dstConfig=Join-Path $consoleDir 'P26ThetisMetersConfigForm.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

if(!(Test-Path $srcConfig)){throw "P26 config source missing: $srcConfig"}
Copy-Item $srcConfig $dstConfig -Force

$bridge=[IO.File]::ReadAllText($bridgePath)

$field='        private ToolStripMenuItem p25AddRx1Menu;'
if(!$bridge.Contains($field)){throw 'P26 bridge menu field anchor missing'}
$bridge=$bridge.Replace(
    $field,
    $field+$nl+'        private ToolStripMenuItem p26ConfigureMetersMenu;'
)

$menu='            p25MetersMenu = new ToolStripMenuItem("Meters/Gadgets");'+$nl+
      '            p25AddRx1Menu = new ToolStripMenuItem("Add RX1 Signal Meter");'
if(!$bridge.Contains($menu)){throw 'P26 menu creation anchor missing'}
$menuReplacement='            p25MetersMenu = new ToolStripMenuItem("Meters/Gadgets");'+$nl+
                 '            p26ConfigureMetersMenu = new ToolStripMenuItem("Configure Meters/Gadgets...");'+$nl+
                 '            p26ConfigureMetersMenu.Click += delegate { P26ShowMetersConfig(); };'+$nl+
                 '            p25AddRx1Menu = new ToolStripMenuItem("Add RX1 Signal Meter");'
$bridge=$bridge.Replace($menu,$menuReplacement)

$add='            p25MetersMenu.DropDownItems.Add(p25AddRx1Menu);'
if(!$bridge.Contains($add)){throw 'P26 menu add anchor missing'}
$bridge=$bridge.Replace(
    $add,
    '            p25MetersMenu.DropDownItems.Add(p26ConfigureMetersMenu);'+$nl+
    '            p25MetersMenu.DropDownItems.Add(new ToolStripSeparator());'+$nl+
    $add
)

$shutdown='            p25MetersClosing = true;'+$nl+
          '            try { P25SaveThetisMeters(); } catch { }'
if(!$bridge.Contains($shutdown)){throw 'P26 shutdown anchor missing'}
$bridge=$bridge.Replace(
    $shutdown,
    '            p25MetersClosing = true;'+$nl+
    '            try { P26CloseMetersConfig(); } catch { }'+$nl+
    '            try { P25SaveThetisMeters(); } catch { }'
)

[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

$proj=[IO.File]::ReadAllText($projPath)
$anchor='<Compile Include="P25ThetisMetersBridge.cs" />'
if(!$proj.Contains($anchor)){throw 'P26 csproj P25 bridge anchor missing'}
if(!$proj.Contains('<Compile Include="P26ThetisMetersConfigForm.cs" />'))
{
    $proj=$proj.Replace($anchor,$anchor+$nl+'    <Compile Include="P26ThetisMetersConfigForm.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

$verifyBridge=[IO.File]::ReadAllText($bridgePath)
$verifyConfig=[IO.File]::ReadAllText($dstConfig)
foreach($token in @(
    'Configure Meters/Gadgets...',
    'P26ShowMetersConfig',
    'P26CloseMetersConfig'
)){
    if(!$verifyBridge.Contains($token)){throw "P26 bridge gate missing: $token"}
}
foreach($token in @(
    'MeterManager.MeterContainers',
    'MeterManager.ContainerBorder',
    'MeterManager.ContainerBackgroundColour',
    'AddMeter(',
    'RemoveMeterType',
    'SetOrderForMeterType',
    'GetSettingsForMeterGroup',
    'ApplySettingsForMeterGroup',
    'class P26ThetisMetersConfigForm'
)){
    if(!$verifyConfig.Contains($token)){throw "P26 config gate missing: $token"}
}

Write-Host 'P26_THETIS_CONFIG=STANDALONE_NATIVE_API_2023_02_26_1TO1'
Write-Host 'P26_P25_CORE=UNCHANGED'
Write-Host 'P26_RX2=NOT_EXPOSED'
