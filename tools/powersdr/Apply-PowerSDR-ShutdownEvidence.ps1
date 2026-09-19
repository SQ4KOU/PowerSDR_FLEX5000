[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P06] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
$setupCs = Join-Path $SourceRoot 'Console\setup.cs'
if(!(Test-Path -LiteralPath $consoleCs)) { throw "Missing $consoleCs" }
if(!(Test-Path -LiteralPath $setupCs)) { throw "Missing $setupCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$console = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")
$setup = [IO.File]::ReadAllText($setupCs).Replace("`r`n", "`n")

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

function Replace-Once([string]$Text,[string]$Old,[string]$New,[string]$Label) {
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P06 anchor missing: $Label" }
    if($Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal) -ge 0){ throw "P06 anchor not unique: $Label" }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

# SaveState detailed evidence fields.
$stateSig = '        public void SaveState()'
$fieldBlock = @'
        // SQ4KOU P06 shutdown evidence; diagnostics only, no behavior change.
        private long sq4kouSaveStateTotalMs = -1;
        private long sq4kouSaveStateSWRMs = -1;
        private long sq4kouSaveStateKe9ns8Ms = -1;
        private long sq4kouSaveStateBuildMs = -1;
        private long sq4kouSaveStatePurgeMs = -1;
        private long sq4kouSaveStateDbVarsMs = -1;

'@
$idx=$console.IndexOf($stateSig,[StringComparison]::Ordinal)
if($idx -lt 0){ throw 'P06 SaveState insertion point missing' }
$console=$console.Substring(0,$idx)+$fieldBlock+$console.Substring($idx)

$console = Replace-InMethod $console $stateSig {
    param($m)
    $sig = "public void SaveState()`n        {"
    if(!$m.Contains($sig)){ throw 'P06 SaveState signature body anchor missing' }
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

# Setup SaveOptions detailed evidence.
$setupFieldOld='        private static bool saving = false;'
$setupFieldNew=@'
        private static bool saving = false;
        public long Sq4kouLastSaveOptionsTotalMs = -1;
        public long Sq4kouLastSaveOptionsBuildMs = -1;
        public long Sq4kouLastSaveOptionsDbVarsMs = -1;
'@
$setup=Replace-Once $setup $setupFieldOld $setupFieldNew 'SaveOptions evidence fields'

$setup = Replace-InMethod $setup '        public void SaveOptions()' {
    param($m)
    $sig="public void SaveOptions()`n        {"
    if(!$m.Contains($sig)){ throw 'P06 SaveOptions body anchor missing' }
    $m=$m.Replace($sig,$sig + @'

            Stopwatch sq4kouSaveOptionsTimer = Stopwatch.StartNew();
            Stopwatch sq4kouSaveOptionsPartTimer = new Stopwatch();
'@)

    $old='            ArrayList a = new ArrayList();'
    $new=@'
            sq4kouSaveOptionsPartTimer.Restart();
            ArrayList a = new ArrayList();
'@
    if(!$m.Contains($old)){ throw 'P06 SaveOptions build anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            DB.SaveVars("Options", ref a);      // save the values to the DB'
    $new=@'
            Sq4kouLastSaveOptionsBuildMs = sq4kouSaveOptionsPartTimer.ElapsedMilliseconds;
            sq4kouSaveOptionsPartTimer.Restart();
            DB.SaveVars("Options", ref a);      // save the values to the DB
            Sq4kouLastSaveOptionsDbVarsMs = sq4kouSaveOptionsPartTimer.ElapsedMilliseconds;
'@
    if(!$m.Contains($old)){ throw 'P06 Options SaveVars anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            saving = false;'
    $new=@'
            saving = false;
            Sq4kouLastSaveOptionsTotalMs = sq4kouSaveOptionsTimer.ElapsedMilliseconds;
'@
    if(!$m.Contains($old)){ throw 'P06 SaveOptions total anchor missing' }
    $m=$m.Replace($old,$new)
    return $m
}

# Console_Closing phase-by-phase timing.
$console = Replace-InMethod $console '        public void Console_Closing(object sender, FormClosingEventArgs e)' {
    param($m)

    $old='            writer.WriteLine("This is a PowerSDR powering downlog: 1-8");'
    $new=@'
            writer.WriteLine("This is a PowerSDR powering downlog: 1-8");
            Stopwatch sq4kouStageTimer = new Stopwatch();
            Stopwatch sq4kouSubTimer = new Stopwatch();
'@
    if(!$m.Contains($old)){ throw 'P06 shutdown writer anchor missing' }
    $m=$m.Replace($old,$new)

    $steps=@(
      @('            writer.WriteLine("1) Disable Audio, CAT, CXAuto, Rotor, VFODIAL, N1MM, QuicRec, Powermate, CWX Polling, timers, VOARUN, MUF");',
        '            writer.WriteLine("1) Disable Audio, CAT, CXAuto, Rotor, VFODIAL, N1MM, QuicRec, Powermate, CWX Polling, timers, VOARUN, MUF");'+"`n"+'            sq4kouStageTimer.Restart();'),
      @('            writer.WriteLine("1) Done");',
        '            writer.WriteLine("1) Done");'+"`n"+'            writer.WriteLine("SQ4KOU_STEP1_DISABLE_SERVICES_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());'),
      @('            writer.WriteLine("2) Hide all forms ");',
        '            writer.WriteLine("2) Hide all forms ");'+"`n"+'            sq4kouStageTimer.Restart();'),
      @('            writer.WriteLine("2) Done");',
        '            writer.WriteLine("2) Done");'+"`n"+'            writer.WriteLine("SQ4KOU_STEP2_HIDE_FORMS_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());'),
      @('            writer.WriteLine("3) Save MemoryList and DXMemList");',
        '            writer.WriteLine("3) Save MemoryList and DXMemList");'+"`n"+'            sq4kouStageTimer.Restart();'),
      @('            writer.WriteLine("4) Save SWL_logger, ke9ns8.dat, and Database STATE variables, and Power.csv file");',
        '            writer.WriteLine("4) Save SWL_logger, ke9ns8.dat, and Database STATE variables, and Power.csv file");'+"`n"+'            sq4kouStageTimer.Restart();'),
      @('            writer.WriteLine("5) turn off PABias and MIDI");',
        '            writer.WriteLine("5) turn off PABias and MIDI");'+"`n"+'            sq4kouStageTimer.Restart();'),
      @('            writer.WriteLine("6) Save SetupForm OPTIONS variables for Database");',
        '            writer.WriteLine("6) Save SetupForm OPTIONS variables for Database");'+"`n"+'            sq4kouStageTimer.Restart();'),
      @('            writer.WriteLine("7) CLOSE all forms");',
        '            writer.WriteLine("7) CLOSE all forms");'+"`n"+'            sq4kouStageTimer.Restart();')
    )
    foreach($pair in $steps){
      if(!$m.Contains($pair[0])){ throw ('P06 step start anchor missing: '+$pair[0]) }
      $m=$m.Replace($pair[0],$pair[1])
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

    $old='            SaveState();                // put current settings back into database     DB.SaveVars("State", ref a);`t`t    // save the values to the DB'
    if(!$m.Contains($old))
    {
        $old='            SaveState();'
    }
    $new=$old + "`n" + '            writer.WriteLine("SQ4KOU_STEP4_SAVESTATE_TOTAL_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_SWR_LOGGER_MS=" + sq4kouSaveStateSWRMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_KE9NS8_MS=" + sq4kouSaveStateKe9ns8Ms.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_BUILD_STATE_MS=" + sq4kouSaveStateBuildMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_PURGE_NOTCHES_MS=" + sq4kouSaveStatePurgeMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_DB_SAVEVARS_MS=" + sq4kouSaveStateDbVarsMs.ToString());' + "`n" +
        '            writer.WriteLine("SQ4KOU_SAVESTATE_INTERNAL_TOTAL_MS=" + sq4kouSaveStateTotalMs.ToString());'
    if(!$m.Contains($old)){ throw 'P06 SaveState call anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            writer.WriteLine("4) Done");'
    if(!$m.Contains($old)){ throw 'P06 step4 end anchor missing' }

    $old='            writer.WriteLine("5) DONE");'
    $new='            writer.WriteLine("5) DONE");'+"`n"+'            writer.WriteLine("SQ4KOU_STEP5_PABIAS_MIDI_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());'
    if(!$m.Contains($old)){ throw 'P06 step5 end anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            if (setupForm != null) setupForm.SaveOptions();'
    $new=$old + "`n" + '            writer.WriteLine("SQ4KOU_STEP6_SAVEOPTIONS_TOTAL_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());' + "`n" +
        '            if (setupForm != null)' + "`n" +
        '            {' + "`n" +
        '                writer.WriteLine("SQ4KOU_SAVEOPTIONS_INTERNAL_TOTAL_MS=" + setupForm.Sq4kouLastSaveOptionsTotalMs.ToString());' + "`n" +
        '                writer.WriteLine("SQ4KOU_SAVEOPTIONS_BUILD_CONTROLS_MS=" + setupForm.Sq4kouLastSaveOptionsBuildMs.ToString());' + "`n" +
        '                writer.WriteLine("SQ4KOU_SAVEOPTIONS_DB_SAVEVARS_MS=" + setupForm.Sq4kouLastSaveOptionsDbVarsMs.ToString());' + "`n" +
        '            }'
    if(!$m.Contains($old)){ throw 'P06 SaveOptions call anchor missing' }
    $m=$m.Replace($old,$new)

    $old='            writer.WriteLine("6) DONE");'
    if(!$m.Contains($old)){ throw 'P06 step6 end anchor missing' }

    $old='            writer.WriteLine("7) DONE");'
    $new='            writer.WriteLine("7) DONE");'+"`n"+'            writer.WriteLine("SQ4KOU_STEP7_CLOSE_FORMS_MS=" + sq4kouStageTimer.ElapsedMilliseconds.ToString());'
    if(!$m.Contains($old)){ throw 'P06 step7 end anchor missing' }
    $m=$m.Replace($old,$new)

    return $m
}

# Exact physical database write time inside ExitConsole (DB.Exit -> patched atomic Update).
$console = Replace-InMethod $console '        public void ExitConsole()' {
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
 @{Name='step phase timings';Ok=$console.Contains('SQ4KOU_STEP4_SAVESTATE_TOTAL_MS=') -and $console.Contains('SQ4KOU_STEP7_CLOSE_FORMS_MS=')},
 @{Name='SWR timing';Ok=$console.Contains('SQ4KOU_SAVESTATE_SWR_LOGGER_MS=')},
 @{Name='state DB timing';Ok=$console.Contains('SQ4KOU_SAVESTATE_DB_SAVEVARS_MS=')},
 @{Name='options DB timing';Ok=$console.Contains('SQ4KOU_SAVEOPTIONS_DB_SAVEVARS_MS=')},
 @{Name='physical DB timing';Ok=$console.Contains('SQ4KOU_DB_EXIT_PHYSICAL_WRITE_MS=')},
 @{Name='setup evidence fields';Ok=$setup.Contains('Sq4kouLastSaveOptionsDbVarsMs')}
)
$failed=@($checks|Where-Object{-not $_.Ok})
if($failed.Count -gt 0){throw ('P06 post-check failed: '+(($failed|ForEach-Object{$_.Name})-join ', '))}

[IO.File]::WriteAllText($consoleCs,$console.Replace("`n","`r`n"),$utf8)
[IO.File]::WriteAllText($setupCs,$setup.Replace("`n","`r`n"),$utf8)
Stage 'PASS: evidence-only timing for shutdown phases, SaveState/SWR, SaveOptions and physical DB write'
