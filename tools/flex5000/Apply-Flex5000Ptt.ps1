$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$consoleDir = Join-Path $repo 'Project Files\Source\Console'
$transportPath = Join-Path $consoleDir 'Flex5000Transport.cs'
$consolePath = Join-Path $consoleDir 'console.cs'

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path)
}
function Write-Utf8([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}
function Inject-After-Once([string]$Text, [string]$Pattern, [string]$Marker, [string]$Insertion, [string]$Name) {
    if ($Text.Contains($Marker)) { return $Text }
    $rx = [regex]::new($Pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $matches = $rx.Matches($Text)
    if ($matches.Count -ne 1) { throw "$Name anchor count expected=1 actual=$($matches.Count)" }
    $m = $matches[0]
    return $Text.Substring(0, $m.Index + $m.Length) + $Insertion + $Text.Substring($m.Index + $m.Length)
}

# PAL/FWC native PTT: RDAL_OP_READ_PTT=1266.
# FWC raw bits: bit0 dot, bit1 dash, bit2 RCA PTT, bit3 MIC PTT.
# Thetis nativeGetDotDashPTT contract: bit0 PTT, bit1 dash, bit2 dot.
$t = Read-Utf8 $transportPath
$t = Inject-After-Once $t '^[ \t]*private const int OpSetPaFilter = 1260;[ \t]*\r?$' 'FLEX5000_NATIVE_PTT_OPCODE' @"

        // FLEX5000_NATIVE_PTT_OPCODE: PowerSDR FWC.Opcode.RDAL_OP_READ_PTT.
        private const int OpReadPtt = 1266;
"@ 'Flex5000 PTT opcode'

$t = Inject-After-Once $t '^[ \t]*internal static uint Serial \{ get \{ return _serial; \} \}[ \t]*\r?$' 'FLEX5000_NATIVE_PTT_BRIDGE' @"

        // FLEX5000_NATIVE_PTT_BRIDGE: translate PAL/FWC PTT/key bits to Thetis native bit layout.
        internal static int ReadDotDashPtt()
        {
            lock (Sync)
            {
                if (!_connected || _readOp == null) return 0;
                try
                {
                    uint raw;
                    int rc = _readOp(OpReadPtt, 0, 0, out raw);
                    if (rc == 0) return 0;

                    int bits = 0;
                    if ((raw & 0x0C) != 0) bits |= 0x01; // RCA PTT or MIC PTT -> Thetis PTT
                    if ((raw & 0x02) != 0) bits |= 0x02; // dash
                    if ((raw & 0x01) != 0) bits |= 0x04; // dot
                    return bits;
                }
                catch
                {
                    return 0;
                }
            }
        }
"@ 'Flex5000 PTT bridge method'
Write-Utf8 $transportPath $t

# The main FLEX overlay deliberately deferred physical PTT by returning zero.
# Replace only that explicit deferred path; leave normal non-FLEX ChannelMaster polling untouched.
$t = Read-Utf8 $consolePath
$pattern = 'int dotdashptt = 0; // physical FLEX PTT deferred; manual MOX/CAT remains native Thetis'
$rx = [regex]::new([regex]::Escape($pattern))
$count = $rx.Matches($t).Count
if ($count -lt 1) { throw 'Deferred FLEX PTT poll anchor not found in console.cs' }
$t = $rx.Replace($t, 'int dotdashptt = Flex5000Transport.ReadDotDashPtt(); // native FLEX-5000 PAL/FWC physical PTT')
Write-Utf8 $consolePath $t

# Final deterministic gates.
$transport = Read-Utf8 $transportPath
$console = Read-Utf8 $consolePath
if (!$transport.Contains('FLEX5000_NATIVE_PTT_OPCODE')) { throw 'PTT opcode gate failed' }
if (!$transport.Contains('FLEX5000_NATIVE_PTT_BRIDGE')) { throw 'PTT bridge gate failed' }
if (!$console.Contains('Flex5000Transport.ReadDotDashPtt()')) { throw 'Console PTT poll gate failed' }
if ($console.Contains('physical FLEX PTT deferred')) { throw 'Deferred physical PTT path still present' }

Write-Host "FLEX5000 physical PTT bridge applied successfully (poll sites=$count)."
