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
# 1. MiniSpec analyzers must not be allocated before ChannelMaster CreateRadio().
#    In stock startup InitConsole() runs before radio = new Radio(AppDataPath),
#    while alloc_analyzer() requires create_analyzer_alloc() created by cmaster.
# -----------------------------------------------------------------------------
$t = Read-Utf8 $consolePath
$miniMarker = 'FLEX5000_NATIVE_MINISPEC_AFTER_CM'

if (!$t.Contains($miniMarker)) {
    $oldBlockRx = [regex]::new('(?ms)^[ \t]*// setup additional spectrum analysers, used by meter system\r?\n[ \t]*if \(_use_additional_sas\)\r?\n[ \t]*\{\r?\n[ \t]*MiniSpec\.Init\(this\);\r?\n[ \t]*MiniSpec\.Add\(1, 0, false\);[^\r\n]*\r?\n[ \t]*MiniSpec\.Add\(2, 1, false\);[^\r\n]*\r?\n[ \t]*//MiniSpec\.Add\(1, 0, true\);[^\r\n]*\r?\n[ \t]*\}\r?\n[ \t]*//\r?\n')
    $oldMatches = $oldBlockRx.Matches($t)
    if ($oldMatches.Count -ne 1) {
        throw "MiniSpec early-init block anchor expected=1 actual=$($oldMatches.Count)"
    }
    $old = $oldMatches[0]
    $replacement = @"
#if !FLEX5000_NATIVE
$($old.Value.TrimEnd())
#else
            // FLEX5000: delayed until ChannelMaster CreateRadio() has created analyzer allocator.
#endif
"@ + "`r`n"
    $t = $t.Substring(0, $old.Index) + $replacement + $t.Substring($old.Index + $old.Length)

    $radioRx = [regex]::new('(?m)^(?<indent>[ \t]*)radio = new Radio\(AppDataPath\);[^\r\n]*\r?$')
    $radioMatches = $radioRx.Matches($t)
    if ($radioMatches.Count -ne 1) {
        throw "radio = new Radio(AppDataPath) anchor expected=1 actual=$($radioMatches.Count)"
    }
    $m = $radioMatches[0]
    $indent = $m.Groups['indent'].Value
    $insert = @"

#if FLEX5000_NATIVE
${indent}// FLEX5000_NATIVE_MINISPEC_AFTER_CM: ChannelMaster CreateRadio() and
${indent}// create_analyzer_alloc() have completed inside RadioDSP.CreateDSP().
${indent}if (_use_additional_sas)
${indent}{
${indent}    MiniSpec.Init(this);
${indent}    MiniSpec.Add(1, 0, false);
${indent}    MiniSpec.Add(2, 1, false);
${indent}}
#endif
"@
    $t = $t.Substring(0, $m.Index + $m.Length) + $insert + $t.Substring($m.Index + $m.Length)
    Write-Utf8 $consolePath $t
}

$verifyConsole = Read-Utf8 $consolePath
if (!$verifyConsole.Contains($miniMarker)) {
    throw 'FLEX5000 delayed MiniSpec marker missing'
}
$radioPos = $verifyConsole.IndexOf('radio = new Radio(AppDataPath);', [System.StringComparison]::Ordinal)
$miniPos = $verifyConsole.IndexOf($miniMarker, [System.StringComparison]::Ordinal)
if ($radioPos -lt 0 -or $miniPos -lt 0 -or $miniPos -lt $radioPos) {
    throw 'FLEX5000 MiniSpec is not physically after Radio/ChannelMaster creation'
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

Write-Host 'FLEX5000 startup fixes applied: MiniSpec delayed until ChannelMaster ready; DirectX hardware failure falls back to WARP.'
