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
# Exact Thetis configuration-button graphics.
$resourcePropertyAnchor='        public static Image resizegrab { get { return Load("resizegrab"); } }'
if(!$bridge.Contains($resourcePropertyAnchor)){throw 'P27 P25MeterResources anchor missing'}
$resourceProperties=@'
        public static Image arrow_left_black { get { return Load("arrow_left_black"); } }
        public static Image arrow_right_black { get { return Load("arrow_right_black"); } }
        public static Image arrow_up_black { get { return Load("arrow_up_black"); } }
        public static Image down_black { get { return Load("down_black"); } }
        public static Image pipette32border { get { return Load("pipette32border"); } }
        public static Image brush32border { get { return Load("brush32border"); } }
'@
$bridge=$bridge.Replace($resourcePropertyAnchor,$resourcePropertyAnchor+$nl+$resourceProperties)

$resDir=Join-Path $consoleDir 'Resources'
New-Item -ItemType Directory -Force -Path $resDir | Out-Null
$resourceUrls=@{
    'arrow_left_black'='https://raw.githubusercontent.com/ramdor/Thetis/a53b19274e715182d8f386bfabbb2c4164287a0e/Project%20Files/Source/Console/Resources/arrow_left_black.png'
    'arrow_right_black'='https://raw.githubusercontent.com/ramdor/Thetis/a53b19274e715182d8f386bfabbb2c4164287a0e/Project%20Files/Source/Console/Resources/arrow_right_black.png'
    'arrow_up_black'='https://raw.githubusercontent.com/ramdor/Thetis/a53b19274e715182d8f386bfabbb2c4164287a0e/Project%20Files/Source/Console/Resources/arrow_up_black.png'
    'down_black'='https://raw.githubusercontent.com/ramdor/Thetis/a53b19274e715182d8f386bfabbb2c4164287a0e/Project%20Files/Source/Console/Resources/down_black.png'
    'pipette32border'='https://raw.githubusercontent.com/ramdor/Thetis/a53b19274e715182d8f386bfabbb2c4164287a0e/Project%20Files/Source/Console/Resources/pipette32border.png'
    'brush32border'='https://raw.githubusercontent.com/ramdor/Thetis/a53b19274e715182d8f386bfabbb2c4164287a0e/Project%20Files/Source/Console/Resources/brush32border.png'
}
foreach($r in $resourceUrls.Keys)
{
    Invoke-WebRequest -UseBasicParsing -Uri $resourceUrls[$r] -OutFile (Join-Path $resDir ($r+'.png'))
}


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

$resourceNames=@('arrow_left_black','arrow_right_black','arrow_up_black','down_black','pipette32border','brush32border')
foreach($r in $resourceNames)
{
    $rel='Resources\'+$r+'.png'
    if($proj -notmatch ('Content Include="'+[regex]::Escape($rel)+'"'))
    {
        $item='    <Content Include="'+$rel+'">'+$nl+
              '      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>'+$nl+
              '    </Content>'+$nl
        $igPos=$proj.IndexOf('</ItemGroup>')
        if($igPos -lt 0){throw 'P27 csproj ItemGroup close missing'}
        $proj=$proj.Insert($igPos,$item)
    }
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
    'grpMultiMeterHolder',
    'chkLockContainer',
    'chkContainerShowTX',
    'chkContainerShowRX',
    'chkContainerNoTitle',
    'chkMultiMeter_auto_container_height',
    'chkContainerMinimises',
    'txtContainerNotes',
    'grpMeterItemSettings',
    'grpMeterItemClockSettings',
    'grpMeterItemVfoDisplaySettings',
    'grpMeterItemSpacerSettings',
    'grpTextOverlay',
    'grpMeterItemDataOutNode',
    'grpMeterItemRotator',
    'grpLedIndicator',
    'grpWebImage',
    'grpBandButtons',
    'grpHistoryItem',
    'GetSettingsForMeterGroup',
    'ApplySettingsForMeterGroup',
    'SetOrderForMeterType',
    'RemoveMeterType',
    'setupMMSettingsGroupBoxes',
    'ShowMultiMeterSetupTab'
)){
    if(!$verifyConfig.Contains($token)){throw "P27 exact-Thetis UI gate missing: $token"}
}

foreach($forbidden in @('PropertyGrid','MeterSettingsProxy','P23MeterManager','FlexMeters.dll'))
{
    if($verifyConfig.Contains($forbidden)){throw "P27 forbidden replacement UI/model found: $forbidden"}
}

Write-Host 'P27_THETIS_CONFIG=THETIS_MULTIMETERS2_A53B192_EXACT_SURFACE'
Write-Host 'P27_PROPERTYGRID=ABSENT'
Write-Host 'P27_P25_CORE=UNCHANGED'
Write-Host 'P27_RX2=NOT_EXPOSED'
