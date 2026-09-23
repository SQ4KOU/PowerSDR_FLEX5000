param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$src=Join-Path $PSScriptRoot 'P46TCIStreaming.cs'
$dst=Join-Path $SourceRoot 'Console\P46TCIStreaming.cs'
$projPath=Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$audioPath=Join-Path $SourceRoot 'Console\audio.cs'

foreach($p in @($src,$projPath,$audioPath)){
    if(!(Test-Path $p)){throw "P46 required file missing: $p"}
}

$utf8NoBom=New-Object Text.UTF8Encoding($false)
Copy-Item $src $dst -Force

$proj=[IO.File]::ReadAllText($projPath)
$nl=if($proj.Contains("`r`n")){"`r`n"}else{"`n"}
$compileAnchor='    <Compile Include="P44TCI.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P46 csproj P44 compile anchor missing'}
if(!$proj.Contains('<Compile Include="P46TCIStreaming.cs" />')){
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P46TCIStreaming.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

$audio=[IO.File]::ReadAllText($audioPath).Replace("`r`n","`n")

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace("`r`n","`n")
    $New=$New.Replace("`r`n","`n")
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){throw "P46 audio anchor missing: $Label"}
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){throw "P46 audio anchor not unique: $Label"}
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

$iqAnchor=@'
            //---------------------------------------------------------------------------------
            // handle Direct IQ for VAC1
            #region vac1IQ
'@
$iqNew=@'
            // SQ4KOU P46: TCI RX1 IQ capture. Copying/packet I/O is deferred
            // to the P46 worker; the PortAudio callback never performs socket writes.
            P46TCIStreaming.CaptureIQ(rx1_in_l, rx1_in_r, frameCount, sample_rate1);

            //---------------------------------------------------------------------------------
            // handle Direct IQ for VAC1
            #region vac1IQ
'@
$audio=Replace-ExactOnce $audio $iqAnchor $iqNew 'RX1 IQ capture'

$dspAnchor=@'
            // ke9ns DttSP 

            if (localmox && (tx_dsp_mode == DSPMode.CWL || tx_dsp_mode == DSPMode.CWU))
'@
$dspNew=@'
            // ke9ns DttSP 

            // SQ4KOU P46: when trx:0,true,tci owns TX, replace the physical/VAC
            // microphone block with TCI TX_AUDIO_STREAM samples immediately before DSP.
            P46TCIStreaming.FillTxAudio(tx_in_l, tx_in_r, frameCount, sample_rate1);

            if (localmox && (tx_dsp_mode == DSPMode.CWL || tx_dsp_mode == DSPMode.CWU))
'@
$audio=Replace-ExactOnce $audio $dspAnchor $dspNew 'TCI TX audio injection'

$postDspAnchor=@'
                DttSP.ExchangeSamples2(ex_input, ex_output, frameCount);            // ke9ns for standard audio do this routine found in  winmain.c as Audio_Callback2

            }


#if (MINMAX)
'@
$postDspNew=@'
                DttSP.ExchangeSamples2(ex_input, ex_output, frameCount);            // ke9ns for standard audio do this routine found in  winmain.c as Audio_Callback2

            }

            // SQ4KOU P46: post-DSP RX audio follows Thetis TCI semantics.
            // During TX publish the TX monitor path into RX1's TCI audio stream.
            if (localmox)
                P46TCIStreaming.CaptureAudio(tx_out_l, tx_out_r, frameCount, sample_rate1);
            else
                P46TCIStreaming.CaptureAudio(rx1_out_l, rx1_out_r, frameCount, sample_rate1);


#if (MINMAX)
'@
$audio=Replace-ExactOnce $audio $postDspAnchor $postDspNew 'RX audio capture'

[IO.File]::WriteAllText($audioPath,$audio.Replace("`n","`r`n"),$utf8NoBom)

$verify=[IO.File]::ReadAllText($audioPath)
foreach($token in @(
    'P46TCIStreaming.CaptureIQ(rx1_in_l, rx1_in_r, frameCount, sample_rate1);',
    'P46TCIStreaming.FillTxAudio(tx_in_l, tx_in_r, frameCount, sample_rate1);',
    'P46TCIStreaming.CaptureAudio(tx_out_l, tx_out_r, frameCount, sample_rate1);',
    'P46TCIStreaming.CaptureAudio(rx1_out_l, rx1_out_r, frameCount, sample_rate1);'
)){
    if(!$verify.Contains($token)){throw "P46 audio gate missing: $token"}
}

$stream=[IO.File]::ReadAllText($dst)
foreach($token in @(
    'IQ_STREAM = 0',
    'RX_AUDIO_STREAM = 1',
    'TX_AUDIO_STREAM = 2',
    'TX_CHRONO = 3',
    'audio_stream_sample_type:',
    'audio_stream_channels:',
    'audio_stream_samples:',
    'tx_stream_audio_buffering:',
    'HandleBinary',
    'FillTxAudio',
    'CaptureIQ',
    'CaptureAudio'
)){
    if(!$stream.Contains($token)){throw "P46 stream gate missing: $token"}
}

Write-Host 'P46_TCI_STREAMING=RX_IQ_RX_AUDIO_TX_AUDIO'
Write-Host 'P46_TCI_IQ=RX1_PRE_DSP_FLOAT32'
Write-Host 'P46_TCI_RX_AUDIO=RX1_POST_DSP'
Write-Host 'P46_TCI_TX_AUDIO=TRX_TCI_OVERRIDE'
Write-Host 'P46_TCI_BINARY_HEADER=THETIS_64_BYTE'
Write-Host 'P46_TCI_NETWORK_IO=WORKER_THREAD_NOT_AUDIO_CALLBACK'
Write-Host 'P46_TCI_RX2=NOT_EXPOSED'
