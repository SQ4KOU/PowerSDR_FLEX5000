param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$baseScript=Join-Path $PSScriptRoot 'Build-PowerSDR-MSI.ps1'
if(!(Test-Path $baseScript)){throw 'Base P21 build script missing'}
$c=[IO.File]::ReadAllText($baseScript)

$anchor='    $buildLog=Join-Path $LogRoot ''MSBUILD_POWERSDR.log'''
if(!$c.Contains($anchor)){throw 'P26 build insertion anchor missing'}

$insert=@'
    # P26: verified P25 direct Thetis meter core + standalone native configuration UI.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P26.ps1') -SourceRoot $WorkRoot

    # Direct Thetis meter core uses pinned SharpDX packages; restore after applying P26.
    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE_P26.log')
    if($LASTEXITCODE -ne 0){throw "P26 NuGet restore failed rc=$LASTEXITCODE"}

'@
$c=$c.Replace($anchor,$insert+$anchor)

$c=$c.Replace(
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P21-SQ4KOU-SPLASH.x86.msi',
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P26-DIRECT-THETIS-METERS-CONFIG.x86.msi'
)

$old="'THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
$new="'METERS_GADGETS=THETIS_DIRECT_2023_02_26','THETIS_METER_SOURCE_SHA=8220ec089451380054e9c31313d4ac2d4bf11776','THETIS_METER_RENDERER=SHARPDX_NATIVE','THETIS_METER_CONTAINER=NATIVE_UCMETER_FRMMETERDISPLAY','THETIS_METER_PERSISTENCE=POWERSDR_DB_NATIVE','THETIS_METER_CONFIG=STANDALONE_NATIVE_API_2023_02_26_1TO1','P25_METER_CORE=UNCHANGED','RX2_METER=NOT_EXPOSED','FLEXMETERS_HELPER_DLL=ABSENT','THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
if(!$c.Contains($old)){throw 'P26 manifest anchor missing'}
$c=$c.Replace($old,$new)

$generated=Join-Path $PSScriptRoot '_Build-P26.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try
{
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0){throw "Generated P26 build failed rc=$LASTEXITCODE"}
}
finally
{
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
