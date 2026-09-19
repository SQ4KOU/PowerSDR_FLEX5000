[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-SHUTDOWN] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $consoleCs)) { throw "PowerSDR console source missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$text = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "Fast-shutdown anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "Fast-shutdown anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

function Get-MethodSpan([string]$Text, [string]$Signature) {
    $start = $Text.IndexOf($Signature, [StringComparison]::Ordinal)
    if($start -lt 0) { throw "Method signature missing: $Signature" }
    $brace = $Text.IndexOf('{', $start)
    if($brace -lt 0) { throw "Method opening brace missing: $Signature" }
    $depth = 0
    $end = -1
    for($i=$brace; $i -lt $Text.Length; $i++) {
        if($Text[$i] -eq '{') { $depth++ }
        elseif($Text[$i] -eq '}') {
            $depth--
            if($depth -eq 0) { $end = $i + 1; break }
        }
    }
    if($end -lt 0) { throw "Method closing brace missing: $Signature" }
    return @($start,$end)
}

function Replace-InMethod([string]$Text, [string]$Signature, [scriptblock]$Transform) {
    $span = Get-MethodSpan $Text $Signature
    $method = $Text.Substring($span[0], $span[1]-$span[0])
    $newMethod = & $Transform $method
    if($newMethod -eq $method) { throw "Method transform made no changes: $Signature" }
    return $Text.Substring(0,$span[0]) + $newMethod + $Text.Substring($span[1])
}

$oldJoin = @'
                if (draw_display_thread != null)
                {
                    if (!draw_display_thread.Join(500))
                        draw_display_thread.Abort();
                }
                if (multimeter_thread != null)
                {
                    if (!multimeter_thread.Join(500))
                        multimeter_thread.Abort();
                }
                if (sql_update_thread != null)
                {
                    if (!sql_update_thread.Join(500))
                        sql_update_thread.Abort();
                }
                if (noise_gate_update_thread != null)
                {
                    if (!noise_gate_update_thread.Join(500))
                        noise_gate_update_thread.Abort();
                }
                if (vox_update_thread != null)
                {
                    if (!vox_update_thread.Join(500))
                        vox_update_thread.Abort();
                }

                if ((poll_ptt_thread != null)) // ke9ns mod
                {
                    if (!poll_ptt_thread.Join(500))
                        poll_ptt_thread.Abort();
                }
'@

$newJoin = @'
                // SQ4KOU P03: all worker loops see chkPower=false at the same time.
                // Give the whole group one 500 ms shutdown budget instead of up to
                // 500 ms per thread (the old code could accumulate to ~3 seconds).
                Thread[] shutdownThreads = new Thread[]
                {
                    draw_display_thread,
                    multimeter_thread,
                    sql_update_thread,
                    noise_gate_update_thread,
                    vox_update_thread,
                    poll_ptt_thread
                };

                Stopwatch shutdownJoinWatch = Stopwatch.StartNew();
                foreach (Thread shutdownThread in shutdownThreads)
                {
                    if (shutdownThread == null || !shutdownThread.IsAlive) continue;

                    int remaining = Math.Max(0, 500 - (int)shutdownJoinWatch.ElapsedMilliseconds);
                    if (remaining > 0) shutdownThread.Join(remaining);
                }

                foreach (Thread shutdownThread in shutdownThreads)
                {
                    if (shutdownThread == null || !shutdownThread.IsAlive) continue;
                    try { shutdownThread.Abort(); }
                    catch (ThreadStateException) { }
                }
'@

$text = Replace-ExactOnce $text $oldJoin $newJoin 'group worker-thread shutdown budget'

$text = Replace-InMethod $text '        public void Console_Closing(object sender, FormClosingEventArgs e)' {
    param($m)

    $activeSleeps = [regex]::Matches($m, '(?m)^\s*Thread\.Sleep\((800|300|100)\);[^\r\n]*$').Count
    if($activeSleeps -ne 7) { throw "Unexpected active shutdown1 sleep count: $activeSleeps" }

    $m = [regex]::Replace($m, '(?m)^\s*Thread\.Sleep\((800|300|100)\);[^\r\n]*\n?', '')

    $sig = "public void Console_Closing(object sender, FormClosingEventArgs e)`n        {"
    if(!$m.Contains($sig)) { throw 'Console_Closing timing anchor missing' }
    $m = $m.Replace($sig, $sig + "`n            Stopwatch sq4kouShutdown1Timer = Stopwatch.StartNew();")

    $close = '            writer.Close();'
    if(!$m.Contains($close)) { throw 'Console_Closing writer close anchor missing' }
    $m = $m.Replace($close, '            writer.WriteLine("SQ4KOU_SHUTDOWN1_TOTAL_MS=" + sq4kouShutdown1Timer.ElapsedMilliseconds.ToString());' + "`n" + $close)
    return $m
}

$text = Replace-InMethod $text '        protected override void Dispose(bool disposing)' {
    param($m)

    $old = @'
            USB.Exit(); // for 1500
            Thread.Sleep(300);
'@
    $new = @'
            USB.Exit(); // for 1500
            // FLEX-5000 does not use the SDR-1000/FLEX-1500 USB shutdown path.
            // Preserve the legacy delay for other models only.
            if (current_model != Model.FLEX5000) Thread.Sleep(300);
'@
    if(($m.Split($old).Count-1) -ne 1) { throw 'Dispose USB delay anchor mismatch' }
    $m = $m.Replace($old,$new)

    $sig = "protected override void Dispose(bool disposing)`n        {"
    if(!$m.Contains($sig)) { throw 'Dispose timing anchor missing' }
    $m = $m.Replace($sig, $sig + "`n            Stopwatch sq4kouShutdown2Timer = Stopwatch.StartNew();")

    $close = '            writer.Close(); // 2'
    if(!$m.Contains($close)) { throw 'Dispose writer close anchor missing' }
    $m = $m.Replace($close, '            writer.WriteLine("SQ4KOU_SHUTDOWN2_TOTAL_MS=" + sq4kouShutdown2Timer.ElapsedMilliseconds.ToString());' + "`n" + $close)
    return $m
}

$text = Replace-InMethod $text '        public void ExitConsole()' {
    param($m)

    # Keep only the two short PAL/FWCMidi settling delays inside the initial hardware-close block.
    $lines = $m -split "`n"
    $activeSleepLines = @()
    for($i=0; $i -lt $lines.Count; $i++) {
        if($lines[$i] -match '^\s*Thread\.Sleep\(100\);') { $activeSleepLines += $i }
    }
    if($activeSleepLines.Count -ne 7) { throw "Unexpected shutdown3 sleep count: $($activeSleepLines.Count)" }

    $keep = @($activeSleepLines[0], $activeSleepLines[1])
    $newLines = New-Object System.Collections.Generic.List[string]
    for($i=0; $i -lt $lines.Count; $i++) {
        if(($activeSleepLines -contains $i) -and -not ($keep -contains $i)) { continue }
        $newLines.Add($lines[$i])
    }
    $m = [string]::Join("`n",$newLines)

    $sig = "public void ExitConsole()`n        {"
    if(!$m.Contains($sig)) { throw 'ExitConsole timing anchor missing' }
    $m = $m.Replace($sig, $sig + "`n            Stopwatch sq4kouShutdown3Timer = Stopwatch.StartNew();")

    $close = '            writer.Close();'
    $last = $m.LastIndexOf($close, [StringComparison]::Ordinal)
    if($last -lt 0) { throw 'ExitConsole writer close anchor missing' }
    $insert = '            writer.WriteLine("SQ4KOU_SHUTDOWN3_TOTAL_MS=" + sq4kouShutdown3Timer.ElapsedMilliseconds.ToString());' + "`n"
    $m = $m.Substring(0,$last) + $insert + $m.Substring($last)
    return $m
}

$checks = @(
    @{ Name='single worker shutdown budget'; Ok=$text.Contains('500 - (int)shutdownJoinWatch.ElapsedMilliseconds') },
    @{ Name='six sequential Join500 removed'; Ok=(!$text.Contains('draw_display_thread.Join(500)')) -and (!$text.Contains('poll_ptt_thread.Join(500)')) },
    @{ Name='shutdown1 timing'; Ok=$text.Contains('SQ4KOU_SHUTDOWN1_TOTAL_MS=') },
    @{ Name='shutdown2 timing'; Ok=$text.Contains('SQ4KOU_SHUTDOWN2_TOTAL_MS=') },
    @{ Name='shutdown3 timing'; Ok=$text.Contains('SQ4KOU_SHUTDOWN3_TOTAL_MS=') },
    @{ Name='FLEX5000 USB sleep bypass'; Ok=$text.Contains('if (current_model != Model.FLEX5000) Thread.Sleep(300);') }
)

$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('Fast-shutdown post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs, $text.Replace("`n", "`r`n"), $utf8)
Stage 'PASS: cumulative close sleeps removed, worker joins capped globally, shutdown timing logs enabled'
