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
function Replace-Exactly([string]$Text, [string]$Pattern, [string]$Replacement, [int]$Expected, [string]$Name) {
    $rx = [regex]::new($Pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $count = $rx.Matches($Text).Count
    if ($count -ne $Expected) { throw "$Name anchor count expected=$Expected actual=$count" }
    return $rx.Replace($Text, $Replacement)
}

$t = Read-Utf8 $transportPath
if (!$t.Contains('FLEX5000_NATIVE_TX_LEGACY_CLEANUP')) {
    $t = Replace-Exactly $t `
        '^[ \t]*private const int SampleRate = 192000;[ \t]*\r?\n[ \t]*private const int TxInputRate = 48000;[ \t]*\r?\n[ \t]*private const int TxDecimate = 4;[ \t]*\r?$' `
        @"
        private const int SampleRate = 192000;
        // FLEX5000_NATIVE_TX_LEGACY_CLEANUP: ChannelMaster now accepts native 192 kHz TX input.
"@ `
        1 `
        'FLEX5000 obsolete TX constants'

    $t = Replace-Exactly $t `
        '^[ \t]*private static int _txDecimatePhase;[ \t]*\r?$' `
        '' `
        1 `
        'FLEX5000 obsolete TX decimation phase field'

    $t = Replace-Exactly $t `
        '^[ \t]*_rxFill = _txFill = _txDecimatePhase = 0;[ \t]*\r?$' `
        '                    _rxFill = _txFill = 0;' `
        1 `
        'FLEX5000 obsolete TX decimation phase reset'
}

Write-Utf8 $transportPath $t

$verify = Read-Utf8 $transportPath
if (!$verify.Contains('FLEX5000_NATIVE_TX_LEGACY_CLEANUP')) { throw 'TX legacy cleanup marker missing' }
if ($verify.Contains('TxInputRate')) { throw 'Obsolete TxInputRate still present' }
if ($verify.Contains('TxDecimate')) { throw 'Obsolete TxDecimate still present' }
if ($verify.Contains('_txDecimatePhase')) { throw 'Obsolete _txDecimatePhase still present' }

Write-Host 'FLEX5000 obsolete TX decimation state removed successfully.'
