[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P08-DISPLAY] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
$displayCs = Join-Path $SourceRoot 'Console\display.cs'
$win32Cs = Join-Path $SourceRoot 'Console\win32.cs'
$setupDesignerCs = Join-Path $SourceRoot 'Console\setup.Designer.cs'

foreach($p in @($consoleCs,$displayCs,$win32Cs,$setupDesignerCs)) {
    if(!(Test-Path -LiteralPath $p)) { throw "P08 source missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P08 anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P08 anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

function Get-MethodSpan([string]$Text, [string]$Signature) {
    $start = $Text.IndexOf($Signature, [StringComparison]::Ordinal)
    if($start -lt 0) { throw "P08 method signature missing: $Signature" }
    if($Text.IndexOf($Signature, $start + $Signature.Length, [StringComparison]::Ordinal) -ge 0) {
        throw "P08 method signature not unique: $Signature"
    }
    $brace = $Text.IndexOf('{', $start)
    if($brace -lt 0) { throw "P08 method opening brace missing: $Signature" }
    $depth = 0
    $end = -1
    for($i=$brace; $i -lt $Text.Length; $i++) {
        if($Text[$i] -eq '{') { $depth++ }
        elseif($Text[$i] -eq '}') {
            $depth--
            if($depth -eq 0) { $end = $i + 1; break }
        }
    }
    if($end -lt 0) { throw "P08 method closing brace missing: $Signature" }
    return @($start,$end)
}

function Replace-InMethod([string]$Text, [string]$Signature, [scriptblock]$Transform) {
    $span = Get-MethodSpan $Text $Signature
    $method = $Text.Substring($span[0], $span[1]-$span[0])
    $newMethod = & $Transform $method
    if($newMethod -eq $method) { throw "P08 method transform made no changes: $Signature" }
    return $Text.Substring(0,$span[0]) + $newMethod + $Text.Substring($span[1])
}

# Console scheduler
$console = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")

$priorityPattern = 'draw_display_thread\.Priority\s*=\s*ThreadPriority\.BelowNormal;'
$priorityCount = ([regex]::Matches($console, $priorityPattern)).Count
if($priorityCount -lt 1) { throw "P08 display-thread priority anchor missing" }
$console = [regex]::Replace($console, $priorityPattern, 'draw_display_thread.Priority = ThreadPriority.Normal;')

$console = Replace-ExactOnce $console '        private int display_fps = 15;' '        private int display_fps = 30;' 'display fps default'
$console = Replace-ExactOnce $console '        private int display_delay = 1000 / 15;' '        private int display_delay = 1000 / 30;' 'display delay default'

$console = Replace-InMethod $console '        private void RunDisplay()' {
    param($m)

    $sig = "private void RunDisplay()`n        {"
    if(!$m.Contains($sig)) { throw 'P08 RunDisplay signature anchor missing' }

    $clockBlock = @'

            // SQ4KOU P08: deadline-based scheduler. The native loop slept a full
            // display_delay after doing its work, so work time was added to every
            // frame and the real FPS sagged below the configured value.
            Stopwatch sq4kouDisplayClock = Stopwatch.StartNew();
            long sq4kouNextFrameMs = 0;
'@
    $m = $m.Replace($sig, $sig + $clockBlock)

    $sleepPattern = 'if\s*\(chkPower\.Checked\)\s*\{\s*Thread\.Sleep\(display_delay\);\s*\}'
    $sleepMatches = [regex]::Matches($m, $sleepPattern)
    if($sleepMatches.Count -ne 1) { throw ("P08 RunDisplay sleep anchor count: " + $sleepMatches.Count) }

    $newSleep = @'
if (chkPower.Checked)
                {
                    int sq4kouFramePeriodMs = Math.Max(1, display_delay);
                    sq4kouNextFrameMs += sq4kouFramePeriodMs;

                    long sq4kouSleepMs = sq4kouNextFrameMs - sq4kouDisplayClock.ElapsedMilliseconds;
                    if (sq4kouSleepMs > 1)
                    {
                        Thread.Sleep((int)sq4kouSleepMs);
                    }
                    else if (sq4kouSleepMs < -sq4kouFramePeriodMs)
                    {
                        // If UI/GDI work missed a whole frame, rebase instead of
                        // trying to catch up with a burst of invalidations.
                        sq4kouNextFrameMs = sq4kouDisplayClock.ElapsedMilliseconds;
                        Thread.Sleep(0);
                    }
                    else
                    {
                        Thread.Sleep(0);
                    }
                }
'@
    $m = [regex]::Replace($m, $sleepPattern, $newSleep, 1)

    return $m
}

# Renderer
$display = [IO.File]::ReadAllText($displayCs).Replace("`r`n", "`n")

$oldPointReset = @'
            if (console.setupForm != null && console.setupForm.check3DPan.Checked == false)
            {
                points = null; // ke9ns: reset the points array
            }
'@
$newPointReset = @'
            if (console.setupForm != null && console.setupForm.check3DPan.Checked == false)
            {
                // SQ4KOU P08: keep the Point[] buffer for the next frame.
                // DrawPanadapter already resizes it when W grows. Nulling it here
                // forced a new W-sized managed allocation on every rendered frame.
            }
'@
$display = Replace-ExactOnce $display $oldPointReset $newPointReset 'panadapter Point buffer reuse'

$display = Replace-ExactOnce $display '            if ((duration > waterfall_update_period) && console.chkPower.Checked)' '            if ((duration >= waterfall_update_period) && console.chkPower.Checked)' 'waterfall update boundary'

$oldScroll = @'
                Win32.memcpy(
                    new IntPtr((int)bitmapData.Scan0 + (bitmapData.Stride)).ToPointer(),  // + stride is 1 row down
                  bitmapData.Scan0.ToPointer(),
                    total_size - bitmapData.Stride
                    );  // copy (dest, source, count)
'@
$newScroll = @'
                Win32.memmove(
                    new IntPtr((int)bitmapData.Scan0 + (bitmapData.Stride)).ToPointer(),  // + stride is 1 row down
                  bitmapData.Scan0.ToPointer(),
                    total_size - bitmapData.Stride
                    );  // overlapping scroll copy: dest starts one row below source
'@
$display = Replace-ExactOnce $display $oldScroll $newScroll 'overlapping waterfall bitmap scroll'

# memmove
$win32 = [IO.File]::ReadAllText($win32Cs).Replace("`r`n", "`n")
$oldMemcpy = @'
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
        public static extern void memcpy(void* destptr, void* srcptr, int n);
'@
$newMemcpy = @'
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
        public static extern void memcpy(void* destptr, void* srcptr, int n);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memmove")]
        public static extern void memmove(void* destptr, void* srcptr, int n);
'@
$win32 = Replace-ExactOnce $win32 $oldMemcpy $newMemcpy 'memmove declaration'

# Fresh install FPS default only
$designer = [IO.File]::ReadAllText($setupDesignerCs).Replace("`r`n", "`n")
$oldDefault = @'
            this.udDisplayFPS.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
'@
$newDefault = @'
            this.udDisplayFPS.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
'@
$designer = Replace-ExactOnce $designer $oldDefault $newDefault 'Setup FPS fresh default'

$checks = @(
    @{Name='normal display thread'; Ok=($console.Contains('draw_display_thread.Priority = ThreadPriority.Normal;') -and !$console.Contains('draw_display_thread.Priority = ThreadPriority.BelowNormal;'))},
    @{Name='deadline scheduler'; Ok=$console.Contains('sq4kouNextFrameMs') -and $console.Contains('sq4kouDisplayClock.ElapsedMilliseconds')},
    @{Name='30 fps default'; Ok=$console.Contains('private int display_fps = 30;')},
    @{Name='pan point reuse'; Ok=$display.Contains('SQ4KOU P08: keep the Point[] buffer for the next frame.') -and !$display.Contains('points = null; // ke9ns: reset the points array')},
    @{Name='waterfall >= period'; Ok=$display.Contains('if ((duration >= waterfall_update_period) && console.chkPower.Checked)')},
    @{Name='waterfall memmove'; Ok=$display.Contains('Win32.memmove(') -and $win32.Contains('EntryPoint = "memmove"')}
)
$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P08 post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs, $console.Replace("`n", "`r`n"), $utf8)
[IO.File]::WriteAllText($displayCs, $display.Replace("`n", "`r`n"), $utf8)
[IO.File]::WriteAllText($win32Cs, $win32.Replace("`n", "`r`n"), $utf8)
[IO.File]::WriteAllText($setupDesignerCs, $designer.Replace("`n", "`r`n"), $utf8)

Stage 'PASS: pan buffer reuse, normal-priority display feeder, stable frame pacing, waterfall timing + overlap-safe scroll'
