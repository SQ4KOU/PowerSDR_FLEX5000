param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$baseScript=Join-Path $PSScriptRoot 'Build-PowerSDR-MSI.ps1'
if(!(Test-Path $baseScript)){throw 'Base P21 build script missing'}
$c=[IO.File]::ReadAllText($baseScript)

$anchor='    $buildLog=Join-Path $LogRoot ''MSBUILD_POWERSDR.log'''
if(!$c.Contains($anchor)){throw 'P27 build insertion anchor missing'}

$insert=@'
    # P27: P25 direct Thetis core unchanged + direct Thetis MultiMeters2 controls.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P27.ps1') -SourceRoot $WorkRoot

    # Direct Thetis renderer references pinned SharpDX packages added after base restore.
    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE_P27.log')
    if($LASTEXITCODE -ne 0){throw "P27 NuGet restore failed rc=$LASTEXITCODE"}

'@
$c=$c.Replace($anchor,$insert+$anchor)

$c=$c.Replace(
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P21-SQ4KOU-SPLASH.x86.msi',
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P27-DIRECT-THETIS-METERS-CONFIG-1TO1.x86.msi'
)

$old="'THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
$new="'METERS_GADGETS=THETIS_DIRECT_2023_02_26','THETIS_METER_SOURCE_SHA=8220ec089451380054e9c31313d4ac2d4bf11776','THETIS_METER_RENDERER=SHARPDX_NATIVE','THETIS_METER_CONTAINER=NATIVE_UCMETER_FRMMETERDISPLAY','THETIS_METER_PERSISTENCE=POWERSDR_DB_NATIVE','THETIS_METER_CONFIG=THETIS_MULTIMETERS2_2023_02_26_DIRECT_CONTROLS','THETIS_METER_CONFIG_PROPERTYGRID=ABSENT','P25_METER_CORE=UNCHANGED','RX2_METER=NOT_EXPOSED','FLEXMETERS_HELPER_DLL=ABSENT','THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
if(!$c.Contains($old)){throw 'P27 manifest anchor missing'}
$c=$c.Replace($old,$new)

$generated=Join-Path $PSScriptRoot '_Build-P27.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try
{
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0){throw "Generated P27 build failed rc=$LASTEXITCODE"}
}
finally
{
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
