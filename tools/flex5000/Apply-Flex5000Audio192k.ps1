$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$transportPath = Join-Path $repo 'Project Files\Source\Console\Flex5000Transport.cs'

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path)
}
function Write-Utf8([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}
function Replace-Exactly([string]$Text, [string]$Pattern, [string]$Replacement, [int]$Expected, [string]$Name, [System.Text.RegularExpressions.RegexOptions]$Options = [System.Text.RegularExpressions.RegexOptions]::Multiline) {
    $rx = [regex]::new($Pattern, $Options)
    $count = $rx.Matches($Text).Count
    if ($count -ne $Expected) { throw "$Name anchor count expected=$Expected actual=$count" }
    return $rx.Replace($Text, $Replacement)
}
function Inject-After-Once([string]$Text, [string]$Pattern, [string]$Marker, [string]$Insertion, [string]$Name) {
    if ($Text.Contains($Marker)) { return $Text }
    $rx = [regex]::new($Pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $matches = $rx.Matches($Text)
    if ($matches.Count -ne 1) { throw "$Name anchor count expected=1 actual=$($matches.Count)" }
    $m = $matches[0]
    return $Text.Substring(0, $m.Index + $m.Length) + $Insertion + $Text.Substring($m.Index + $m.Length)
}

$t = Read-Utf8 $transportPath

# ChannelMaster already owns a proper resampler in AAMix.  Exported SetCMAudioOutrate()
# changes the mixer output rate and sizes while preserving each input stream's own rate.
$t = Inject-After-Once $t `
    '^[ \t]*private static extern void SendpOutboundTx\(CmOutboundDelegate callback\);[ \t]*\r?$' `
    'FLEX5000_NATIVE_RX_AUDIO_192K_API' `
    @"

        // FLEX5000_NATIVE_RX_AUDIO_192K_API: let ChannelMaster/AAMix resample RX audio to the device rate.
        [DllImport("ChannelMaster.dll", EntryPoint = "SetCMAudioOutrate", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetCMAudioOutrate(int inId, int rate);
"@ `
    'ChannelMaster RX audio rate API'

$t = Inject-After-Once $t `
    '^[ \t]*cmaster\.SetXcmInrate\(_txStream, SampleRate\);[ \t]*\r?$' `
    'FLEX5000_NATIVE_RX_AUDIO_192K_START' `
    @"

                    // FLEX5000_NATIVE_RX_AUDIO_192K_START: AAMix performs proper 48/other-rate -> 192 kHz resampling.
                    SetCMAudioOutrate(_rxStream, SampleRate);
"@ `
    'FLEX5000 RX audio rate setup'

if (!$t.Contains('FLEX5000_NATIVE_RX_AUDIO_DIRECT_RENDER')) {
    $t = Replace-Exactly $t `
        '^[ \t]*private static int _audioRepeat;[ \t]*\r?$' `
        '' `
        1 `
        'obsolete audio repeat state'
    $t = Replace-Exactly $t `
        '^[ \t]*private static float _heldL;[ \t]*\r?$' `
        '' `
        1 `
        'obsolete held left audio sample'
    $t = Replace-Exactly $t `
        '^[ \t]*private static float _heldR;[ \t]*\r?$' `
        '' `
        1 `
        'obsolete held right audio sample'

    $renderPattern = '(?ms)^[ \t]*private static void RenderAudio\(float\* hpR, float\* hpL, float\* extR, float\* extL, float\* line, float\* speaker, int frames\)\s*\{.*?^[ \t]*\}\s*\r?\n\s*private static void RenderTx'
    $renderReplacement = @"
        // FLEX5000_NATIVE_RX_AUDIO_DIRECT_RENDER: ChannelMaster callback is already 192 kHz.
        private static void RenderAudio(float* hpR, float* hpL, float* extR, float* extL, float* line, float* speaker, int frames)
        {
            for (int n = 0; n < frames; n++)
            {
                float l = 0.0f;
                float r = 0.0f;
                int rd = Volatile.Read(ref _audioRead);
                if (rd != Volatile.Read(ref _audioWrite))
                {
                    l = AudioL[rd];
                    r = AudioR[rd];
                    Volatile.Write(ref _audioRead, (rd + 1) & AudioRingMask);
                }

                if (hpR != null) hpR[n] = r;
                if (hpL != null) hpL[n] = l;
                if (extR != null) extR[n] = r;
                if (extL != null) extL[n] = l;
                if (line != null) line[n] = l;
                if (speaker != null) speaker[n] = l;
            }
        }

        private static void RenderTx
"@
    $t = Replace-Exactly $t $renderPattern $renderReplacement 1 'RX audio render path' ([System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline)

    $t = Replace-Exactly $t `
        '^[ \t]*_audioRepeat = 0;[ \t]*\r?\n[ \t]*_heldL = _heldR = 0\.0f;[ \t]*\r?$' `
        '' `
        1 `
        'obsolete RX audio hold reset'
}

Write-Utf8 $transportPath $t

$verify = Read-Utf8 $transportPath
if (!$verify.Contains('FLEX5000_NATIVE_RX_AUDIO_192K_API')) { throw 'RX audio 192k API gate failed' }
if (!$verify.Contains('FLEX5000_NATIVE_RX_AUDIO_192K_START')) { throw 'RX audio 192k start gate failed' }
if (!$verify.Contains('FLEX5000_NATIVE_RX_AUDIO_DIRECT_RENDER')) { throw 'RX audio direct render gate failed' }
if ($verify.Contains('_audioRepeat')) { throw 'Obsolete _audioRepeat remains' }
if ($verify.Contains('_heldL') -or $verify.Contains('_heldR')) { throw 'Obsolete held audio sample state remains' }

Write-Host 'FLEX5000 RX audio now uses ChannelMaster native resampling to 192 kHz.'
