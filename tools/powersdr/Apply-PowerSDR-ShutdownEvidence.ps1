[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P06] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $consoleCs)) { throw "Missing $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$consoleRaw = [IO.File]::ReadAllText($consoleCs)
$beforeLen = (New-Object IO.FileInfo($consoleCs)).Length
$beforeCrCrLf = ([regex]::Matches($consoleRaw, "`r`r`n")).Count
$beforeCrLf = ([regex]::Matches($consoleRaw, "(?<!`r)`r`n")).Count
$beforeLfOnly = ([regex]::Matches($consoleRaw, "(?<!`r)`n")).Count
Stage ("INPUT bytes={0} CRCRLF={1} CRLF={2} LF={3}" -f $beforeLen,$beforeCrCrLf,$beforeCrLf,$beforeLfOnly)
$console = $consoleRaw.Replace("`r`n", "`n")

function Replace-InMethod([string]$Text, [string]$Signature, [scriptblock]$Transform) {
    $start = $Text.IndexOf($Signature, [StringComparison]::Ordinal)
    if($start -lt 0) { throw "P06 method signature missing: $Signature" }
    if($Text.IndexOf($Signature, $start + $Signature.Length, [StringComparison]::Ordinal) -ge 0) {
        throw "P06 method signature not unique: $Signature"
    }
    $brace = $Text.IndexOf('{', $start)
    if($brace -lt 0) { throw "P06 method opening brace missing: $Signature" }
    $depth = 0
    $end = -1
    for($i=$brace; $i -lt $Text.Length; $i++) {
        if($Text[$i] -eq '{') { $depth++ }
        elseif($Text[$i] -eq '}') {
            $depth--
            if($depth -eq 0) { $end = $i + 1; break }
        }
    }
    if($end -lt 0) { throw "P06 method closing brace missing: $Signature" }
    $method = $Text.Substring($start, $end-$start)
    $changed = & $Transform $method
    if($changed -eq $method) { throw "P06 transform made no change: $Signature" }
    return $Text.Substring(0,$start) + $changed + $Text.Substring($end)
}

# SaveState detailed timings.
$stateSig='        public void SaveState()'
$stateIdx=$console.IndexOf($stateSig,[StringComparison]::Ordinal)
if($stateIdx -lt 0){ throw 'P06 SaveState insertion point missing' }
$stateFields=@'
        // SQ4KOU P06 shutdown evidence; diagnostics only, no behavior change.
        private long sq4kouSaveStateTotalMs = -1;
        private long sq4kouSaveStateSWRMs = -1;
        private long sq4kouSaveStateKe9ns8Ms = -1;
        private long sq4kouSaveStateBuildMs = -1;
        private long sq4kouSaveStatePurgeMs = -1;
        private long sq4kouSaveStateDbVarsMs = -1;

'@
$console=$console.Substring(0,$stateIdx)+$stateFields+$console.Substring($stateIdx)

$console=Replace-InMethod $console $stateSig {
    param($m)
    $sig="public void SaveState()`n        {"
    if(!$m.Contains($sig)){ throw 'P06 SaveState body anchor missing' }
    $m=$m.Replace($sig,$sig + @'

            Stopwatch sq4kouSaveStateTimer = Stopwatch.StartNew();
            Stopwatch sq4kouSaveStatePartTimer = Stopwatch.StartNew();
'@)

    $old='            SWR_Logger_Write(); // save SWR data'
    $new=@'
            sq4kouSaveStatePartTimer.Restart();
            SWR_Logger_Write(); // save SWR data
            sq4kouSaveStateSWRMs = sq4kouSaveStatePartTimer.ElapsedMilliseconds;
'@
    if(!$m.Contains($old)){ throw 'P06 SWR anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            string file_name2 = AppDataPath + "ke9ns8.dat"; // save data for my mods'
    $new=@'
            sq4kouSaveStatePartTimer.Restart();
            string file_name2 = AppDataPath + "ke9ns8.dat"; // save data for my mods
'@
    if(!$m.Contains($old)){ throw 'P06 ke9ns8 start anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            stream2.Close();   // close stream'
    $new=@'
            stream2.Close();   // close stream
            sq4kouSaveStateKe9ns8Ms = sq4kouSaveStatePartTimer.ElapsedMilliseconds;
'@
    if(!$m.Contains($old)){ throw 'P06 ke9ns8 end anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            ArrayList a = new ArrayList();     // storage for saving everything'
    $new=@'
            sq4kouSaveStatePartTimer.Restart();
            ArrayList a = new ArrayList();     // storage for saving everything
'@
    if(!$m.Contains($old)){ throw 'P06 state-build start anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            DB.PurgeNotches();                      // remove old notches from DB'
    $new=@'
            sq4kouSaveStateBuildMs = sq4kouSaveStatePartTimer.ElapsedMilliseconds;
            sq4kouSaveStatePartTimer.Restart();
            DB.PurgeNotches();                      // remove old notches from DB
            sq4kouSaveStatePurgeMs = sq4kouSaveStatePartTimer.ElapsedMilliseconds;
'@
    if(!$m.Contains($old)){ throw 'P06 PurgeNotches anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            DB.SaveVars("State", ref a);            // save the "State" values to the DB'
    $new=@'
            sq4kouSaveStatePartTimer.Restart();
            DB.SaveVars("State", ref a);            // save the "State" values to the DB
            sq4kouSaveStateDbVarsMs = sq4kouSaveStatePartTimer.ElapsedMilliseconds;
            sq4kouSaveStateTotalMs = sq4kouSaveStateTimer.ElapsedMilliseconds;
'@
    if(!$m.Contains($old)){ throw 'P06 State SaveVars anchor missing' }
    $m=$m.Replace($old,$new)
    return $m
}

# Console_Closing phase-by-phase timing.
$console=Replace-InMethod $console '        public void Console_Closing(object sender, FormClosingEventArgs e)' {
    param($m)

    $old='            writer.WriteLine("This is a PowerSDR powering downlog: 1-8");'
    $new=@'
            writer.WriteLine("This is a PowerSDR powering downlog: 1-8");
            Stopwatch sq4kouStageTimer = new Stopwatch();
            Stopwatch sq4kouSubTimer = new Stopwatch();
'@
    if(!$m.Contains($old)){ throw 'P06 shutdown writer anchor missing' }
    $m=$m.Replace($old,$new)

    $stageReplacements=@(
      @{ Old='            writer.WriteLine("1) Disable Audio, CAT, CXAuto, Rotor, VFODIAL, N1MM, QuicRec, Powermate, CWX Polling, timers, VOARUN, MUF");'; New='            writer.WriteLine("1) Disable Audio, CAT, CXAuto, Rotor, VFODIAL, N1MM, QuicRec, Powermate, CWX Polling, timers, VOARUN, MUF");' + "`n" + '            sq4kouStageTimer.Restart();' },
      @{ Old='            writer.WriteLine("1) Done");'; New='            writer.WriteLine("1) Done");' + "`n" + '            writer.WriteLine("SQ4KOU_STEP1_DISABLE_SERVICES_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());' },
      @{ Old='            writer.WriteLine("2) Hide all forms ");'; New='            writer.WriteLine("2) Hide all forms ");' + "`n" + '            sq4kouStageTimer.Restart();' },
      @{ Old='            writer.WriteLine("2) Done");'; New='            writer.WriteLine("2) Done");' + "`n" + '            writer.WriteLine("SQ4KOU_STEP2_HIDE_FORMS_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());' },
      @{ Old='            writer.WriteLine("3) Save MemoryList and DXMemList");'; New='            writer.WriteLine("3) Save MemoryList and DXMemList");' + "`n" + '            sq4kouStageTimer.Restart();' },
      @{ Old='            writer.WriteLine("4) Save SWL_logger, ke9ns8.dat, and Database STATE variables, and Power.csv file");'; New='            writer.WriteLine("4) Save SWL_logger, ke9ns8.dat, and Database STATE variables, and Power.csv file");' + "`n" + '            sq4kouStageTimer.Restart();' },
      @{ Old='            writer.WriteLine("5) turn off PABias and MIDI");'; New='            writer.WriteLine("5) turn off PABias and MIDI");' + "`n" + '            sq4kouStageTimer.Restart();' },
      @{ Old='            writer.WriteLine("5) DONE");'; New='            writer.WriteLine("5) DONE");' + "`n" + '            writer.WriteLine("SQ4KOU_STEP5_PABIAS_MIDI_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());' },
      @{ Old='            writer.WriteLine("6) Save SetupForm OPTIONS variables for Database");'; New='            writer.WriteLine("6) Save SetupForm OPTIONS variables for Database");' + "`n" + '            sq4kouStageTimer.Restart();' },
      @{ Old='            writer.WriteLine("7) CLOSE all forms");'; New='            writer.WriteLine("7) CLOSE all forms");' + "`n" + '            sq4kouStageTimer.Restart();' },
      @{ Old='            writer.WriteLine("7) DONE");'; New='            writer.WriteLine("7) DONE");' + "`n" + '            writer.WriteLine("SQ4KOU_STEP7_CLOSE_FORMS_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());' }
    )
    foreach($r in $stageReplacements){
      if(!$m.Contains([string]$r.Old)){ throw ('P06 stage anchor missing: '+[string]$r.Old) }
      $m=$m.Replace([string]$r.Old,[string]$r.New)
    }

    $old='            MemoryList.Save();'
    $new=@'
            sq4kouSubTimer.Restart();
            MemoryList.Save();
            writer.WriteLine("SQ4KOU_MEMORYLIST_SAVE_MS=" + sq4kouSubTimer.ElapsedMilliseconds.ToString());
'@
    if(!$m.Contains($old)){ throw 'P06 MemoryList anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            DXMemList.Save1(); // ke9ns add'
    $new=@'
            sq4kouSubTimer.Restart();
            DXMemList.Save1(); // ke9ns add
            writer.WriteLine("SQ4KOU_DXMEMLIST_SAVE_MS=" + sq4kouSubTimer.ElapsedMilliseconds.ToString());
'@
    if(!$m.Contains($old)){ throw 'P06 DXMemList anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            writer.WriteLine("3) Done");'
    $new=$old + "`n" + '            writer.WriteLine("SQ4KOU_STEP3_MEMORY_TOTAL_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());'
    if(!$m.Contains($old)){ throw 'P06 step3 end anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            SaveState();'
    $new=$old + "`n" +
        '            writer.WriteLine("SQ4KOU_STEP4_SAVESTATE_TOTAL_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_SWR_LOGGER_MS=" + sq4kouSaveStateSWRMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_KE9NS8_MS=" + sq4kouSaveStateKe9ns8Ms.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_BUILD_STATE_MS=" + sq4kouSaveStateBuildMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_PURGE_NOTCHES_MS=" + sq4kouSaveStatePurgeMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_DB_SAVEVARS_MS=" + sq4kouSaveStateDbVarsMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_INTERNAL_TOTAL_MS=" + sq4kouSaveStateTotalMs.ToString());'
    if(!$m.Contains($old)){ throw 'P06 SaveState call anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            if (setupForm != null) setupForm.SaveOptions();'
    $new=$old + "`n" +
        '            writer.WriteLine("SQ4KOU_STEP6_SAVEOPTIONS_TOTAL_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());'
    if(!$m.Contains($old)){ throw 'P06 SaveOptions call anchor missing' }
    $m=$m.Replace($old,$new)

    return $m
}

# Exact physical DB write timing inside ExitConsole.
$console=Replace-InMethod $console '        public void ExitConsole()' {
    param($m)
    $old='            DB.Exit();                  // close and save database'
    $new=@'
            Stopwatch sq4kouDbExitTimer = Stopwatch.StartNew();
            DB.Exit();                  // close and save database
            writer.WriteLine("SQ4KOU_DB_EXIT_PHYSICAL_WRITE_MS=" + sq4kouDbExitTimer.ElapsedMilliseconds.ToString());
'@
    if(!$m.Contains($old)){ throw 'P06 DB.Exit timing anchor missing' }
    $m=$m.Replace($old,$new)
    return $m
}

$checks=@(
 @{Name='step4 timing';Ok=$console.Contains('SQ4KOU_STEP4_SAVESTATE_TOTAL_MS=')},
 @{Name='step7 timing';Ok=$console.Contains('SQ4KOU_STEP7_CLOSE_FORMS_MS=')},
 @{Name='SWR timing';Ok=$console.Contains('SQ4KOU_SAVESTATE_SWR_LOGGER_MS=')},
 @{Name='physical DB timing';Ok=$console.Contains('SQ4KOU_DB_EXIT_PHYSICAL_WRITE_MS=')}
)
$failed=@($checks|Where-Object{-not $_.Ok})
if($failed.Count -gt 0){throw ('P06 post-check failed: '+(($failed|ForEach-Object{$_.Name})-join ', '))}

$console = $console.Replace("`r`r`n", "`n").Replace("`r`n", "`n")
[IO.File]::WriteAllText($consoleCs, $console.Replace("`n", "`r`n"), $utf8)
$afterRaw = [IO.File]::ReadAllText($consoleCs)
$afterLen = (New-Object IO.FileInfo($consoleCs)).Length
$afterCrCrLf = ([regex]::Matches($afterRaw, "`r`r`n")).Count
$afterCrLf = ([regex]::Matches($afterRaw, "(?<!`r)`r`n")).Count
$afterLfOnly = ([regex]::Matches($afterRaw, "(?<!`r)`n")).Count
Stage ("OUTPUT bytes={0} CRCRLF={1} CRLF={2} LF={3}" -f $afterLen,$afterCrCrLf,$afterCrLf,$afterLfOnly)
Stage 'PASS: evidence-only timing for shutdown phases, SaveState/SWR, State SaveVars, SaveOptions total and physical DB write'
