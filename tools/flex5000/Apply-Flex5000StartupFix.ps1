$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$consolePath = Join-Path $repo 'Project Files\Source\Console\console.cs'
$displayPath = Join-Path $repo 'Project Files\Source\Console\display.cs'

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path)
}
function Write-Utf8([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}

# -----------------------------------------------------------------------------
# 1. FLEX5000 P0 startup: do NOT create MiniSpec additional analyzers here.
#
# Stock Thetis creates these optional spectrum analyzers during Console startup.
# Moving them merely behind Radio/ChannelMaster creation is still too early for
# FLEX5000: clsMiniSpec.setupSpecDetails() reads Console/display state that is not
# fully initialised yet and throws NullReferenceException.
#
# These are ADDITIONAL analyzers used by the meter system; they are not the main
# RX/TX DSP path.  For the dedicated FLEX5000 build the safe P0 behaviour is to
# suppress this optional startup block entirely.  We can re-enable it later at a
# proven post-initialisation point, but it must not gate application startup.
# -----------------------------------------------------------------------------
$t = Read-Utf8 $consolePath
$miniMarker = 'FLEX5000_NATIVE_MINISPEC_DISABLED_P0'

if (!$t.Contains($miniMarker)) {
    $oldBlockRx = [regex]::new('(?ms)^[ \t]*// setup additional spectrum analysers, used by meter system\r?\n[ \t]*if \(_use_additional_sas\)\r?\n[ \t]*\{\r?\n[ \t]*MiniSpec\.Init\(this\);\r?\n[ \t]*MiniSpec\.Add\(1, 0, false\);[^\r\n]*\r?\n[ \t]*MiniSpec\.Add\(2, 1, false\);[^\r\n]*\r?\n[ \t]*//MiniSpec\.Add\(1, 0, true\);[^\r\n]*\r?\n[ \t]*\}\r?\n[ \t]*//\r?\n')
    $matches = $oldBlockRx.Matches($t)
    if ($matches.Count -ne 1) {
        throw "MiniSpec startup block anchor expected=1 actual=$($matches.Count)"
    }

    $old = $matches[0]
    $replacement = @"
#if !FLEX5000_NATIVE
$($old.Value.TrimEnd())
#else
            // FLEX5000_NATIVE_MINISPEC_DISABLED_P0
            // Optional meter spectrum analyzers are intentionally suppressed during startup.
            // Main RX/TX DSP and display analyzers remain untouched.
#endif
"@ + "`r`n"

    $t = $t.Substring(0, $old.Index) + $replacement + $t.Substring($old.Index + $old.Length)
    Write-Utf8 $consolePath $t
}

$verifyConsole = Read-Utf8 $consolePath
if (!$verifyConsole.Contains($miniMarker)) {
    throw 'FLEX5000 MiniSpec-disable marker missing'
}

# Hard gate: the FLEX5000 side of the conditional must contain no MiniSpec.Add.
$markerPos = $verifyConsole.IndexOf($miniMarker, [System.StringComparison]::Ordinal)
if ($markerPos -lt 0) { throw 'FLEX5000 MiniSpec marker position missing' }
$afterMarker = $verifyConsole.Substring($markerPos, [Math]::Min(500, $verifyConsole.Length - $markerPos))
if ($afterMarker.Contains('MiniSpec.Add(')) {
    throw 'FLEX5000 MiniSpec.Add still present in enabled FLEX5000 startup path'
}

# -----------------------------------------------------------------------------
# 2. DirectX: if hardware D3D11 creation returns DXGI_ERROR_UNSUPPORTED, retry
#    using the Windows WARP software rasterizer instead of terminating display init.
# -----------------------------------------------------------------------------
$t = Read-Utf8 $displayPath
$dxMarker = 'FLEX5000_NATIVE_DX_WARP_FALLBACK'

if (!$t.Contains($dxMarker)) {
    $catchRx = [regex]::new('(?m)^(?<indent>[ \t]*)// issue setting up dx[ \t]*\r?\n\k<indent>ShutdownDX2D\(\);[ \t]*\r?$')
    $matches = $catchRx.Matches($t)
    if ($matches.Count -ne 1) {
        throw "DirectX catch anchor expected=1 actual=$($matches.Count)"
    }
    $m = $matches[0]
    $indent = $m.Groups['indent'].Value
    $insert = @"

#if FLEX5000_NATIVE
${indent}// FLEX5000_NATIVE_DX_WARP_FALLBACK: unsupported hardware/adapter -> WARP.
${indent}if (driverType == DriverType.Hardware)
${indent}{
${indent}    LogTool.AddLogEntry("DirectX hardware device unsupported; retrying with WARP.", "DX2D");
${indent}    initDX2D(DriverType.Warp, null);
${indent}    return;
${indent}}
#endif
"@
    $t = $t.Substring(0, $m.Index + $m.Length) + $insert + $t.Substring($m.Index + $m.Length)
    Write-Utf8 $displayPath $t
}

$verifyDisplay = Read-Utf8 $displayPath
if (!$verifyDisplay.Contains($dxMarker)) {
    throw 'FLEX5000 DirectX WARP fallback marker missing'
}
if (!$verifyDisplay.Contains('initDX2D(DriverType.Warp, null);')) {
    throw 'FLEX5000 DirectX WARP retry call missing'
}

Write-Host 'FLEX5000 startup fixes applied: optional MiniSpec startup analyzers disabled; DirectX hardware failure falls back to WARP.'
