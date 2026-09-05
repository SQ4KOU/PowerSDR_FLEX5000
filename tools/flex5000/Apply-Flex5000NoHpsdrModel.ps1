$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$hwPath = Join-Path $repo 'Project Files\Source\Console\clsHardwareSpecific.cs'

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path)
}
function Write-Utf8([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}

$t = Read-Utf8 $hwPath
$marker = 'FLEX5000_NATIVE_NO_HPSDR_MODEL_INIT'

if (!$t.Contains($marker)) {
    $pattern = '(?m)^(?<indent>[ \t]*)_old_model = _model;[ \t]*\r?\n\k<indent>_model = value;[ \t]*\r?\n'
    $rx = [regex]::new($pattern)
    $matches = $rx.Matches($t)
    if ($matches.Count -ne 1) {
        throw "HardwareSpecific.Model setter anchor count expected=1 actual=$($matches.Count)"
    }

    $m = $matches[0]
    $indent = $m.Groups['indent'].Value
    $replacement = $m.Value + @"

#if FLEX5000_NATIVE
${indent}// FLEX5000_NATIVE_NO_HPSDR_MODEL_INIT: FLEX-5000 hardware is owned by PAL/FWC,
${indent}// not by HPSDR NetworkIO model initialisation (ADC/BPF/LR-audio/protocol state).
${indent}return;
#endif
"@
    $t = $t.Substring(0, $m.Index) + $replacement + $t.Substring($m.Index + $m.Length)
    Write-Utf8 $hwPath $t
}

$verify = Read-Utf8 $hwPath
if (!$verify.Contains($marker)) {
    throw 'FLEX5000 HardwareSpecific.Model HPSDR bypass marker missing'
}

# Verify that the bypass is physically before the first HPSDR model-side-effect call.
$markerPos = $verify.IndexOf($marker, [System.StringComparison]::Ordinal)
$fwPos = $verify.IndexOf('NetworkIO.FWVersionsChecked = false;', [System.StringComparison]::Ordinal)
if ($markerPos -lt 0 -or $fwPos -lt 0 -or $markerPos -gt $fwPos) {
    throw 'FLEX5000 HardwareSpecific.Model bypass is not before NetworkIO model initialisation'
}

Write-Host 'FLEX5000 HardwareSpecific.Model HPSDR NetworkIO initialisation bypassed successfully.'
