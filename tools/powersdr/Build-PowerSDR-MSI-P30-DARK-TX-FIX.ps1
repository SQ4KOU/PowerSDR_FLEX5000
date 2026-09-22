param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$baseScript=Join-Path $PSScriptRoot 'Build-PowerSDR-MSI.ps1'
if(!(Test-Path $baseScript)){throw 'Base P21 build script missing'}
$c=[IO.File]::ReadAllText($baseScript)

$anchor='    $buildLog=Join-Path $LogRoot ''MSBUILD_POWERSDR.log'''
if(!$c.Contains($anchor)){throw 'P30 build insertion anchor missing'}

$insert=@'
    # P30: P29 accepted base + official dark skins + native FLEX-5000 TX telemetry.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P30.ps1') -SourceRoot $WorkRoot

    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE_P30.log')
    if($LASTEXITCODE -ne 0){throw "P30 NuGet restore failed rc=$LASTEXITCODE"}

'@
$c=$c.Replace($anchor,$insert+$anchor)

$c=$c.Replace(
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P21-SQ4KOU-SPLASH.x86.msi',
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P30-DARK-TX-FIX.x86.msi'
)

$old="'THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
$new="'METERS_GADGETS=THETIS_DIRECT_2023_02_26','THETIS_METER_SOURCE_SHA=8220ec089451380054e9c31313d4ac2d4bf11776','THETIS_METER_RENDERER=SHARPDX_NATIVE','THETIS_METER_PERSISTENCE=REPLACE_ALL','THETIS_METER_SKINS=OFFICIAL_DEFAULT_METERS_ALL_IMAGES','THETIS_METER_SKIN_SOURCE=ramdor/ThetisSkins@601018a9486359b3bdb661063f4687b4c75ad917','THETIS_METER_DARK_SKINS=ananMM-dark,cross-needle-dark','THETIS_METER_CONFIG=THETIS_MULTIMETERS2_2023_02_26_DIRECT_CONTROLS','THETIS_TX_TELEMETRY=POWERSDR_NATIVE_DTTSP_THREAD1_AND_FLEX5000_PA_FIELDS','THETIS_TX_SUPPORTED=MIC,MIC_PK,EQ,EQ_PK,LEVELER,LEVELER_PK,LVL_G,ALC,ALC_PK,ALC_G,COMP,COMP_PK,PWR,REVERSE_PWR,SWR,VOLTS','THETIS_TX_NATIVE_UNAVAILABLE=CFC,CFC_GAIN,ALC_GROUP,AMPS','THETIS_DARK_FALLBACK=NORMAL_IMAGE_ONLY_IF_OFFICIAL_DARK_ABSENT','THETIS_POWER_SCALE=FLEX5000_100W','RX1_METER=UNCHANGED_FROM_P29','RX2_METER=NOT_EXPOSED','FLEXMETERS_HELPER_DLL=ABSENT','THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
if(!$c.Contains($old)){throw 'P30 manifest anchor missing'}
$c=$c.Replace($old,$new)

$generated=Join-Path $PSScriptRoot '_Build-P30.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try
{
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0){throw "Generated P30 build failed rc=$LASTEXITCODE"}
}
finally
{
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
