[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P13-STABLE-DISPLAY] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $consoleCs)) { throw "P13 source missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13) + ([string][char]10)
$lf = [string][char]10
$console = [IO.File]::ReadAllText($consoleCs).Replace($crlf,$lf)

# 1) Restore display feeder priority below the main WinForms UI.
$normalToken = 'draw_display_thread.Priority = ThreadPriority.Normal;'
$normalCount = ([regex]::Matches($console,[regex]::Escape($normalToken))).Count
if($normalCount -lt 1) { throw 'P13 display priority anchor missing' }
$console = $console.Replace($normalToken,'draw_display_thread.Priority = ThreadPriority.BelowNormal;')

# 2) Remove the P08 deadline scheduler clock state from RunDisplay.
$clockBlock = @'
            // SQ4KOU P08: deadline-based scheduler. The native loop slept a full
            // display_delay after doing its work, so work time was added to every
            // frame and the real FPS sagged below the configured value.
            Stopwatch sq4kouDisplayClock = Stopwatch.StartNew();
            long sq4kouNextFrameMs = 0;
'@
$clockCount = ([regex]::Matches($console,[regex]::Escape($clockBlock))).Count
if($clockCount -ne 1) { throw "P13 scheduler clock anchor count=$clockCount" }
$console = $console.Replace($clockBlock,'')

# 3) Replace the aggressive Sleep(0)/catch-up block by the original safe pacing.
$schedulerBlock = @'
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

$safeBlock = @'
if (chkPower.Checked)
                {
                    // SQ4KOU P13: stable pacing. Keep P08 memory/rendering fixes,
                    // but never enter a Sleep(0) catch-up loop after a heavy UI
                    // event such as BAND change.
                    Thread.Sleep(display_delay);
                }
'@

$schedulerCount = ([regex]::Matches($console,[regex]::Escape($schedulerBlock))).Count
if($schedulerCount -ne 1) { throw "P13 scheduler block anchor count=$schedulerCount" }
$console = $console.Replace($schedulerBlock,$safeBlock)

$checks = @(
    @{Name='below-normal display thread'; Ok=$console.Contains('draw_display_thread.Priority = ThreadPriority.BelowNormal;')},
    @{Name='no display Sleep(0) scheduler'; Ok=(!$console.Contains('sq4kouNextFrameMs') -and !$console.Contains('sq4kouDisplayClock'))},
    @{Name='safe display sleep'; Ok=$console.Contains('SQ4KOU P13: stable pacing') -and $console.Contains('Thread.Sleep(display_delay);')}
)
$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P13 post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs,$console.Replace($lf,$crlf),$utf8)
Stage 'PASS: P08 memory optimisations retained; deadline catch-up removed; stable BelowNormal display pacing restored'
