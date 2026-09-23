[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$setupPath=Join-Path $consoleDir 'setup.cs'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$src=Join-Path $PSScriptRoot 'P44TCI.cs'
$dst=Join-Path $consoleDir 'P44TCI.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

if(!(Test-Path $src)){throw 'P44 TCI source missing'}
foreach($p in @($projPath,$setupPath,$bridgePath)){
    if(!(Test-Path $p)){throw "P44 required PowerSDR file missing: $p"}
}

Copy-Item $src $dst -Force

# Add a dedicated TCI page to Setup after the native Console reference is assigned.
$setup=[IO.File]::ReadAllText($setupPath)
$setupAnchor='            console = c;   // ke9ns mod  to allow console to pass back values to setup screen'
if(!$setup.Contains($setupAnchor)){
    $setupAnchor='            console = c;'
}
if(!$setup.Contains($setupAnchor)){throw 'P44 Setup console assignment anchor missing'}
if(!$setup.Contains('P44InitTCIUI();')){
    $setup=$setup.Replace($setupAnchor,$setupAnchor+$nl+'            P44InitTCIUI();')
}
[IO.File]::WriteAllText($setupPath,$setup,$utf8)

# Start the TCI server only after the main PowerSDR window has completed native startup.
$bridge=[IO.File]::ReadAllText($bridgePath)
$meterInit='            try { P25InitThetisMeters(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P25 meter init: " + ex); }'
if(!$bridge.Contains($meterInit)){throw 'P44 shown-handler meter anchor missing'}
if(!$bridge.Contains('P44StartTCI();')){
    $bridge=$bridge.Replace(
        $meterInit,
        '            try { P44StartTCI(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P44 TCI init: " + ex); }'+$nl+$meterInit
    )
}
[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

# Compile the TCI server/adapter into the existing PowerSDR project.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P39LegacyItems.cs" />'
if(!$proj.Contains($compileAnchor)){
    $compileAnchor='<Compile Include="P32ThetisCompatibility.cs" />'
}
if(!$proj.Contains($compileAnchor)){throw 'P44 compile anchor missing'}
if(!$proj.Contains('<Compile Include="P44TCI.cs" />')){
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P44TCI.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Source gates: control/status TCI only. No audio/IQ callbacks are introduced in P44.
$verify=[IO.File]::ReadAllText($dst)
foreach($token in @(
    'TcpListener',
    'Sec-WebSocket-Accept',
    'protocol:ExpertSDR3,2.0;',
    'device:FLEX-5000;',
    'trx_count:1;',
    'channels_count:2;',
    'vfo:0,0,',
    'vfo:0,1,',
    'tx_frequency:',
    'rx_filter_band:0,',
    'agc_mode:0,',
    'agc_gain:0,',
    'split_enable:0,',
    'rx_nr_enable:0,',
    'rx_nb_enable:0,',
    'rx_anf_enable:0,',
    'sql_enable:0,',
    'rx_step_att_ex:0,',
    'rx_preamp_att_ex:0,',
    'drive:0,',
    'tune_drive:0,',
    'trx:0,',
    'tune:0,',
    'modulation:0,',
    'volume:',
    'iq_samplerate:',
    'audio_samplerate:',
    'SQ4KOU_TCI',
    '50001'
)){
    if(!$verify.Contains($token)){throw "P44 TCI source gate missing: $token"}
}
foreach($forbidden in @('OutboundTCIRxIQ','InboundTCITxAudio','PublishIQSamples','PublishRxAudioSamples')){
    if($verify.Contains($forbidden)){throw "P44 control-only build contains forbidden streaming token: $forbidden"}
}

Write-Host 'P44_TCI=CORE_CONTROL_WEBSOCKET'
Write-Host 'P44_TCI_PROTOCOL=EXPERTSDR3_2_0_COMPAT'
Write-Host 'P44_TCI_PORT_DEFAULT=50001'
Write-Host 'P44_TCI_BIND_DEFAULT=0.0.0.0'
Write-Host 'P44_TCI_RX=RX1_VFOA_VFOB'
Write-Host 'P44_TCI_RX2=NOT_EXPOSED'
Write-Host 'P44_TCI_AUDIO_IQ=DEFERRED'
