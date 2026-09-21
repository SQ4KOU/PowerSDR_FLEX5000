[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P27: keep the physically verified P25 direct Thetis meter core unchanged.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P25.ps1') -SourceRoot $SourceRoot

$consoleDir=Join-Path $SourceRoot 'Console'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$srcConfig=Join-Path $PSScriptRoot 'P27ThetisMetersConfigForm.cs'
$dstConfig=Join-Path $consoleDir 'P27ThetisMetersConfigForm.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

if(!(Test-Path $srcConfig)){throw "P27 config source missing: $srcConfig"}
Copy-Item $srcConfig $dstConfig -Force

$bridge=[IO.File]::ReadAllText($bridgePath)

$field='        private ToolStripMenuItem p25AddRx1Menu;'
if(!$bridge.Contains($field)){throw 'P27 bridge menu field anchor missing'}
$bridge=$bridge.Replace($field,$field+$nl+'        private ToolStripMenuItem p27ConfigureMetersMenu;')

$menu='            p25MetersMenu = new ToolStripMenuItem("Meters/Gadgets");'+$nl+
      '            p25AddRx1Menu = new ToolStripMenuItem("Add RX1 Signal Meter");'
if(!$bridge.Contains($menu)){throw 'P27 menu creation anchor missing'}
$bridge=$bridge.Replace(
    $menu,
    '            p25MetersMenu = new ToolStripMenuItem("Meters/Gadgets");'+$nl+
    '            p27ConfigureMetersMenu = new ToolStripMenuItem("Configure Meters/Gadgets...");'+$nl+
    '            p27ConfigureMetersMenu.Click += delegate { P27ShowMetersConfig(); };'+$nl+
    '            p25AddRx1Menu = new ToolStripMenuItem("Add RX1 Signal Meter");'
)

$add='            p25MetersMenu.DropDownItems.Add(p25AddRx1Menu);'
if(!$bridge.Contains($add)){throw 'P27 menu add anchor missing'}
$bridge=$bridge.Replace(
    $add,
    '            p25MetersMenu.DropDownItems.Add(p27ConfigureMetersMenu);'+$nl+
    '            p25MetersMenu.DropDownItems.Add(new ToolStripSeparator());'+$nl+
    $add
)

$shutdown='            p25MetersClosing = true;'+$nl+
          '            try { P25SaveThetisMeters(); } catch { }'
if(!$bridge.Contains($shutdown)){throw 'P27 shutdown anchor missing'}
$bridge=$bridge.Replace(
    $shutdown,
    '            p25MetersClosing = true;'+$nl+
    '            try { P27CloseMetersConfig(); } catch { }'+$nl+
    '            try { P25SaveThetisMeters(); } catch { }'
)

[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

$proj=[IO.File]::ReadAllText($projPath)
$anchor='<Compile Include="P25ThetisMetersBridge.cs" />'
if(!$proj.Contains($anchor)){throw 'P27 csproj P25 bridge anchor missing'}
if(!$proj.Contains('<Compile Include="P27ThetisMetersConfigForm.cs" />'))
{
    $proj=$proj.Replace($anchor,$anchor+$nl+'    <Compile Include="P27ThetisMetersConfigForm.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

$verifyBridge=[IO.File]::ReadAllText($bridgePath)
$verifyConfig=[IO.File]::ReadAllText($dstConfig)

foreach($token in @(
    'Configure Meters/Gadgets...',
    'P27ShowMetersConfig',
    'P27CloseMetersConfig'
)){
    if(!$verifyBridge.Contains($token)){throw "P27 bridge gate missing: $token"}
}

foreach($token in @(
    'GroupBoxTS groupBoxTS28',
    'ComboBoxTS comboContainerSelect',
    'ColorButton clrbtnContainerBackground',
    'ListBox lstMetersAvailable',
    'ListBox lstMetersInUse',
    'GroupBoxTS grpMeterItemSettings',
    'NumericUpDownTS nudMeterItemAttackRate',
    'NumericUpDownTS nudMeterItemDecayRate',
    'CheckBoxTS chkMeterItemHistory',
    'CheckBoxTS chkMeterItemPeakHold',
    'CheckBoxTS chkMeterItemSignalAverage',
    'CheckBoxTS chkMeterItemDarkMode',
    'btnMeterCopySettings',
    'btnMeterPasteSettings',
    'GetSettingsForMeterGroup',
    'ApplySettingsForMeterGroup',
    'SetOrderForMeterType',
    'RemoveMeterType'
)){
    if(!$verifyConfig.Contains($token)){throw "P27 direct-port UI gate missing: $token"}
}

foreach($forbidden in @('PropertyGrid','MeterSettingsProxy','P23MeterManager','FlexMeters.dll'))
{
    if($verifyConfig.Contains($forbidden)){throw "P27 forbidden replacement UI/model found: $forbidden"}
}

Write-Host 'P27_THETIS_CONFIG=THETIS_MULTIMETERS2_2023_02_26_DIRECT_CONTROLS'
Write-Host 'P27_PROPERTYGRID=ABSENT'
Write-Host 'P27_P25_CORE=UNCHANGED'
Write-Host 'P27_RX2=NOT_EXPOSED'
