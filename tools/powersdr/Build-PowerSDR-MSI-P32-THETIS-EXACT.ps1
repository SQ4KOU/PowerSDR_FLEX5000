param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$baseScript=Join-Path $PSScriptRoot 'Build-PowerSDR-MSI.ps1'
if(!(Test-Path $baseScript)){throw 'Base PowerSDR build script missing'}
$c=[IO.File]::ReadAllText($baseScript)

$anchor='    $buildLog=Join-Path $LogRoot ''MSBUILD_POWERSDR.log'''
if(!$c.Contains($anchor)){throw 'P32 build insertion anchor missing'}

$insert=@'
    # P32: coherent Thetis meter/container core from one source commit plus FLEX-5000 adapter.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersCoherent-P32.ps1') -SourceRoot $WorkRoot

    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE_P32.log')
    if($LASTEXITCODE -ne 0){throw "P32 NuGet restore failed rc=$LASTEXITCODE"}

'@
$c=$c.Replace($anchor,$insert+$anchor)

$c=$c.Replace(
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P21-SQ4KOU-SPLASH.x86.msi',
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P32-THETIS-EXACT-METERS.x86.msi'
)

$old="'THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
$new="'METERS_GADGETS=THETIS_DIRECT_EXACT_ADAPTER','P32_BASE=P30_SAFE','P32_THETIS_SOURCE_SHA=a53b19274e715182d8f386bfabbb2c4164287a0e','P32_SOURCE_OF_TRUTH=THETIS_ONLY','P32_SOURCE_COHERENCE=METER_MANAGER_UCMETER_DISPLAY_SAME_COMMIT','P32_CONTAINER=THETIS_NATIVE_21_FIELD_MODEL','P32_INPUT=THETIS_NATIVE_MOUSE_PIPELINE','P32_METER_TYPES=FULL_A53B192_GENERATION','P32_RENDERER=THETIS_EXACT','P32_ADAPTER=POWERSDR_NATIVE_API_ONLY','P32_RX2=NOT_EXPOSED','THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
if(!$c.Contains($old)){throw 'P32 manifest anchor missing'}
$c=$c.Replace($old,$new)

$generated=Join-Path $PSScriptRoot '_Build-P32.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try
{
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0){throw "Generated P32 build failed rc=$LASTEXITCODE"}
}
finally
{
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
