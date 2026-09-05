$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$importsPath = Join-Path $repo 'Project Files\Source\Console\HPSDR\NetworkIOImports.cs'

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path)
}
function Write-Utf8([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}

$t = Read-Utf8 $importsPath
$marker = 'FLEX5000_NATIVE_NETWORKIO_STUBS'

if (!$t.Contains($marker)) {
    $pattern = '(?ms)(?<attr>^[ \t]*\[DllImport\([^\r\n]*\)[^\r\n]*\r?\n)(?<gap>(?:^[ \t]*\r?\n)*)(?<indent>^[ \t]*)public static extern[ \t]+(?<ret>[A-Za-z_][A-Za-z0-9_\.\[\]\*<>]*)[ \t]+(?<name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*\((?<args>.*?)\)[ \t]*;'
    $rx = [regex]::new($pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $matches = $rx.Matches($t)
    if ($matches.Count -lt 40) {
        throw "NetworkIO DllImport isolation found too few methods: $($matches.Count)"
    }

    $sb = New-Object System.Text.StringBuilder
    $pos = 0
    foreach ($m in $matches) {
        [void]$sb.Append($t.Substring($pos, $m.Index - $pos))

        $attr = $m.Groups['attr'].Value
        $gap = $m.Groups['gap'].Value
        $indent = $m.Groups['indent'].Value
        $ret = $m.Groups['ret'].Value
        $name = $m.Groups['name'].Value
        $args = $m.Groups['args'].Value

        $body = New-Object System.Text.StringBuilder
        [void]$body.Append("$indent{")

        $outRx = [regex]::new('(?<![A-Za-z0-9_])out[ \t]+(?<type>[A-Za-z_][A-Za-z0-9_\.\[\]\*<>]*)[ \t]+(?<name>[A-Za-z_][A-Za-z0-9_]*)')
        foreach ($om in $outRx.Matches($args)) {
            $ot = $om.Groups['type'].Value
            $on = $om.Groups['name'].Value
            [void]$body.Append("`r`n$indent    $on = default($ot);")
        }
        if ($ret -ne 'void') {
            [void]$body.Append("`r`n$indent    return default($ret);")
        }
        [void]$body.Append("`r`n$indent}")

        $replacement = @"
#if FLEX5000_NATIVE
$indent// FLEX5000_NATIVE_NETWORKIO_STUBS: HPSDR ChannelMaster transport/hardware entrypoint suppressed.
${indent}public static $ret $name($args)
$($body.ToString())
#else
$attr$gap${indent}public static extern $ret $name($args);
#endif
"@
        [void]$sb.Append($replacement)
        $pos = $m.Index + $m.Length
    }
    [void]$sb.Append($t.Substring($pos))
    $t = $sb.ToString()
    Write-Utf8 $importsPath $t
}

$verify = Read-Utf8 $importsPath
$stubCount = ([regex]::Matches($verify, 'FLEX5000_NATIVE_NETWORKIO_STUBS')).Count
if ($stubCount -lt 40) {
    throw "FLEX5000 NetworkIO stub gate count too small: $stubCount"
}

foreach ($required in @('SetCWSidetoneVolume', 'SetRxADC', 'SetCWPTTDelay', 'SetCWHangTime', 'SetCWSidetoneFreq', 'SetCWKeyerSpeed', 'SetMicBoost', 'SetMicBias', 'SetAntBits', 'SetOCBits')) {
    $needle = "public static void $required("
    if (!$verify.Contains($needle)) {
        throw "FLEX5000 required HPSDR no-op stub missing: $required"
    }
}

Write-Host "FLEX5000 HPSDR NetworkIO P/Invoke isolation applied. Stubbed methods=$stubCount"
