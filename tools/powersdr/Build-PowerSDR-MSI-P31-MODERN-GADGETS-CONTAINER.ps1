param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$baseScript=Join-Path $PSScriptRoot 'Build-PowerSDR-MSI.ps1'
if(!(Test-Path $baseScript)){throw 'Base P21 build script missing'}
$c=[IO.File]::ReadAllText($baseScript)

$anchor='    $buildLog=Join-Path $LogRoot ''MSBUILD_POWERSDR.log'''
if(!$c.Contains($anchor)){throw 'P31 build insertion anchor missing'}

$insert=@'
    # P31: P30 verified base + modern Thetis VFO/BAND/MODE/STEP + Lock/NoTitle.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P31.ps1') -SourceRoot $WorkRoot

    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE_P31.log')
    if($LASTEXITCODE -ne 0){throw "P31 NuGet restore failed rc=$LASTEXITCODE"}

'@
$c=$c.Replace($anchor,$insert+$anchor)

$c=$c.Replace(
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P21-SQ4KOU-SPLASH.x86.msi',
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P31-MODERN-GADGETS-CONTAINER.x86.msi'
)

$old="'THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
$new="'METERS_GADGETS=THETIS_DIRECT_2023_02_26_PLUS_MODERN_BACKPORT','THETIS_METER_SOURCE_SHA=8220ec089451380054e9c31313d4ac2d4bf11776','THETIS_GADGET_SOURCE_SHA=3dbd787eaef30eca66d089ccfb4c4ccb8c0cfb7e','THETIS_CONTAINER_LOCK_SOURCE_SHA=a53b19274e715182d8f386bfabbb2c4164287a0e','THETIS_GADGETS=VFO_DISPLAY,BAND_BUTTONS,MODE_BUTTONS,TUNESTEP_BUTTONS','THETIS_CONTAINER_CONTROLS=LOCK,NO_TITLE_BAR_SHIFT_ACCESS','THETIS_METER_RENDERER=SHARPDX_NATIVE_P30_PLUS_P31_GADGETS','THETIS_METER_PERSISTENCE=REPLACE_ALL','THETIS_METER_SKINS=OFFICIAL_DEFAULT_METERS_ALL_IMAGES','THETIS_TX_TELEMETRY=POWERSDR_NATIVE_DTTSP_THREAD1_AND_FLEX5000_PA_FIELDS','P31_TARGET_ADAPTER=POWERSDR_RX1_VFOA_BAND_MODE_TUNESTEP','P30_BASE=UNCHANGED','RX2_METER=NOT_EXPOSED','FLEXMETERS_HELPER_DLL=ABSENT','THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
if(!$c.Contains($old)){throw 'P31 manifest anchor missing'}
$c=$c.Replace($old,$new)

$generated=Join-Path $PSScriptRoot '_Build-P31.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try
{
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0){throw "Generated P31 build failed rc=$LASTEXITCODE"}
}
finally
{
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
