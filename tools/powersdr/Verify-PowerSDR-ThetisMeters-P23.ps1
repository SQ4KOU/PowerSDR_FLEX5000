[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$consoleDir=Join-Path $SourceRoot 'Console'
$setup=[IO.File]::ReadAllText((Join-Path $consoleDir 'setup.cs'))
$proj=[IO.File]::ReadAllText((Join-Path $consoleDir 'PowerSDR.csproj'))

$hook='SQ4KOUThetisMetersP23.Install(console, this);'
if(([regex]::Matches($setup,[regex]::Escape($hook))).Count -ne 1){throw 'P23 Setup hook count is not exactly one'}

$files=@(
 'SQ4KOUThetisMetersP23.Base.cs',
 'SQ4KOUThetisMetersP23.Core.cs',
 'SQ4KOUThetisMetersP23.Container.cs',
 'SQ4KOUThetisMetersP23.UI.cs'
)
$all=''
foreach($name in $files){
    $p=Join-Path $consoleDir $name
    if(!(Test-Path $p)){throw "P23 compiled source missing: $name"}
    if(([regex]::Matches($proj,'Compile Include="'+[regex]::Escape($name)+'"')).Count -ne 1){throw "P23 csproj compile count invalid: $name"}
    $all += [IO.File]::ReadAllText($p)
}
foreach($required in @(
 'P23MeterManager','P23MeterContainer','P23MeterSurface','P23MetersSetupPanel',
 'P23MultiMeterIO','VFO_DISPLAY','TEXT_OVERLAY','ROTATOR','WEB_IMAGE','BAND_BUTTONS',
 'MODE_BUTTONS','FILTER_BUTTONS','HISTORY','CUSTOM_METER_BAR','VOICE_RECORD_PLAY_BUTTONS',
 'HideWhenRxNotUsed','ContainerMinimises','AutoHeight','PinOnTop','NoControls',
 'SaveContainer','LoadContainer','StartUdp','StartSerial'
)){
    if(!$all.Contains($required)){throw "P23 feature gate missing: $required"}
}
foreach($token in @('NetworkIO','ChannelMaster','Protocol1','Protocol2','RedPitaya','PortAudio','FWC.Set','PAL.')){
    if($all.Contains($token)){throw "P23 forbidden backend marker: $token"}
}
Write-Host 'P23_THETIS_METERS_VERIFY=PASS'
Write-Host 'P23_UI=THETIS_METERS_GADGETS_CONTAINER_MODEL'
Write-Host 'P23_DATA=POWERSDR_DTTSP_FWC_NATIVE'
Write-Host 'P23_MMIO=UDP_SERIAL_VARIABLES'
Write-Host 'P23_BACKEND_REPLACEMENT=NONE'
