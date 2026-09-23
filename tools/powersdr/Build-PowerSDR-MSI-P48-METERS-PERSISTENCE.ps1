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

    # P48: comprehensive Meters/Gadgets persistence + durable save points.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-MetersPersistence-P48.ps1') -SourceRoot $WorkRoot

    # P39: Thetis-style Legacy Items, stage 1 = visibility only.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-LegacyItems-P39.ps1') -SourceRoot $WorkRoot

    # P44/P45: native PowerSDR TCI WebSocket control/status server + extended setup.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-TCI-P44.ps1') -SourceRoot $WorkRoot

    # P46: Thetis-compatible binary TCI RX IQ / RX audio / TX audio streaming.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-TCI-Streaming-P46.ps1') -SourceRoot $WorkRoot

    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE_P32.log')
    if($LASTEXITCODE -ne 0){throw "P32 NuGet restore failed rc=$LASTEXITCODE"}

'@
$c=$c.Replace($anchor,$insert+$anchor)

$c=$c.Replace(
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P21-SQ4KOU-SPLASH.x86.msi',
 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P48-METERS-PERSISTENCE.x86.msi'
)

$old="'THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
$new="'METERS_GADGETS=THETIS_DIRECT_EXACT_ADAPTER','P32_BASE=P30_SAFE','P32_THETIS_SOURCE_SHA=a53b19274e715182d8f386bfabbb2c4164287a0e','P32_SOURCE_OF_TRUTH=THETIS_ONLY','P32_SOURCE_COHERENCE=METER_MANAGER_UCMETER_DISPLAY_SAME_COMMIT','P32_CONTAINER=THETIS_NATIVE_21_FIELD_MODEL','P32_INPUT=THETIS_NATIVE_MOUSE_PIPELINE','P32_METER_TYPES=FULL_A53B192_GENERATION','P32_RENDERER=THETIS_EXACT','P32_ADAPTER=POWERSDR_NATIVE_API_ONLY','P32_RX2=NOT_EXPOSED','P47_METER_PERSISTENCE=AUDITED_AND_HARDENED','P47_METER_MAIN_STORE=THETIS_STORESETTINGS2_REPLACE_ALL','P47_METER_DOCKED_GEOMETRY=ACTUAL_LOCATION_SIZE_NORMALIZED_BEFORE_STORE','P47_METER_FLOATING_GEOMETRY=AUTHORITATIVE_MAIN_TABLE_PLUS_LEGACY_FORM_TABLE','P47_METER_RESTORE=MAIN_TABLE_GEOMETRY_BEFORE_RENDERER_START','P47_METER_ITEMS=THETIS_METERDATA_METERIGDATA_METERIGSETTINGS2','P47_METER_STALE_KEYS=PURGED_BY_REPLACEVARS','P47_METER_SAVE_AUDIT=CONTAINER_FORM_GEOMETRY_COUNTS_REQUIRED','P48_METER_PERSISTENCE=DURABLE_IMMEDIATE_FLUSH','P48_METER_USER_CLOSE=FULL_MODEL_SAVE_PLUS_DB_UPDATE','P48_METER_CONFIG_HIDE=FULL_MODEL_SAVE_PLUS_DB_UPDATE','P48_METER_APP_CLOSE=FULL_MODEL_SAVE_PLUS_DB_UPDATE_BEFORE_SHUTDOWN','P48_METER_AUDIT=DIAGNOSTIC_NOT_SAVE_BLOCKER','P48_METER_GEOMETRY=P47_AUTHORITATIVE_PLUS_NATIVE_FALLBACK','P39_LEGACY_ITEMS=STAGE1_VISIBILITY_ONLY','P39_CONTROLS=METERS_BAND_MODE_FILTER_VFOA_VFOB_VFOSYNC','P39_PERSISTENCE=POWERSDR_DB_SQ4KOU_LEGACYITEMS','P39_SPECTRUM_GEOMETRY=UNCHANGED','P44_TCI=CORE_CONTROL_WEBSOCKET','P44_TCI_PROTOCOL=EXPERTSDR3_2_0_COMPAT','P44_TCI_PORT_DEFAULT=50001','P44_TCI_BIND_DEFAULT=0.0.0.0','P44_TCI_RX=RX1_VFOA_VFOB','P44_TCI_RX2=NOT_EXPOSED','P44_TCI_AUDIO_IQ=SUPERSEDED_BY_P46','P46_TCI_STREAMING=RX_IQ_RX_AUDIO_TX_AUDIO','P46_TCI_IQ=RX1_PRE_DSP_FLOAT32','P46_TCI_RX_AUDIO=RX1_POST_DSP','P46_TCI_TX_AUDIO=TRX_TCI_OVERRIDE','P46_TCI_BINARY_HEADER=THETIS_64_BYTE','P46_TCI_NETWORK_IO=WORKER_THREAD_NOT_AUDIO_CALLBACK','P46_TCI_RX2=NOT_EXPOSED','THETIS_BACKEND=ABSENT','THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',"
if(!$c.Contains($old)){throw 'P32 manifest anchor missing'}
$c=$c.Replace($old,$new)

$generated=Join-Path $PSScriptRoot '_Build-P48.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try
{
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0){throw "Generated P48 build failed rc=$LASTEXITCODE"}
}
finally
{
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
