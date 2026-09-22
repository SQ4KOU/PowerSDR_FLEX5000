param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$baseScript=Join-Path $PSScriptRoot 'Build-PowerSDR-MSI.ps1'
if(!(Test-Path $baseScript)){throw 'Base P21 build script missing'}
$c=[IO.File]::ReadAllText($baseScript)

$anchor='    $buildLog=Join-Path $LogRoot ''MSBUILD_POWERSDR.log'''
if(!$c.Contains($anchor)){throw 'P32 build insertion anchor missing'}

$insert=@'
    # P32: P30 SAFE plus mechanically extracted, pinned Thetis gadget implementation.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P32.ps1') -SourceRoot $WorkRoot

    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE_P32.log')
    if($LASTEXITCODE -ne 0){throw "P32 NuGet restore failed rc=$LASTEXITCODE"}

'@
$c=$c.Replace($anchor,$insert+$anchor)

$c=$c.Replace(
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P21-SQ4KOU-SPLASH.x86.msi',
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P32-THETIS-1TO1.x86.msi'
)

$old="'THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
$new="'METERS_GADGETS=THETIS_DIRECT_EXACT_PINNED_SOURCE','P32_BASE=P30_SAFE_5fb179f357771f2c6aebcfdd7f13136d8c5f1e72','P32_THETIS_SOURCE_SHA=3dbd787eaef30eca66d089ccfb4c4ccb8c0cfb7e','P32_GADGETS=VFO_DISPLAY,BAND_BUTTONS,MODE_BUTTONS,TUNESTEP_BUTTONS','P32_RENDERER=THETIS_EXACT_SOURCE_BLOCKS','P32_MOUSE=THETIS_EXACT_SOURCE_DISPATCH','P32_RX2=NOT_EXPOSED_BY_PROJECT_REQUIREMENT','P32_UNAVAILABLE_NATIVE_UI=THETIS_BANDSTACK_POPUP,THETIS_FILTER_CONTEXT_POPUP','THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
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
