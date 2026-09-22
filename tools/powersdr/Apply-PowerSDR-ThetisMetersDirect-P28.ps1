[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P28: keep P27 renderer/telemetry/UI unchanged; fix only persistence semantics.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P27.ps1') -SourceRoot $SourceRoot

$consoleDir=Join-Path $SourceRoot 'Console'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$dbPath=Join-Path $consoleDir 'database.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$nl=[Environment]::NewLine

if(!(Test-Path $bridgePath)){throw "P28 bridge missing: $bridgePath"}
if(!(Test-Path $dbPath)){throw "P28 database missing: $dbPath"}

# PowerSDR DB.SaveVars is an upsert-only API. For meter configuration this is wrong:
# deleted meterContData_*, meterIGData_* and meterIGSettings_* rows survive and are
# restored on the next launch. Add a table-local replace-all API without changing
# SaveVars semantics for the rest of PowerSDR.
$db=[IO.File]::ReadAllText($dbPath)
if($db -notmatch 'public static void ReplaceVars\(string tableName, ref ArrayList list\)')
{
    $anchor='        public static ArrayList GetVars(string tableName)'
    if(!$db.Contains($anchor)){throw 'P28 database GetVars anchor missing'}

    $method=@'
        // P28: replace-all persistence for data sets where deletion must be authoritative.
        // SaveVars remains unchanged for legacy callers.
        public static void ReplaceVars(string tableName, ref ArrayList list)
        {
            if (!ds.Tables.Contains(tableName)) AddFormTable(tableName);

            ds.Tables[tableName].Rows.Clear();
            SaveVars(tableName, ref list);
        }

'@
    $db=$db.Replace($anchor,$method+$anchor)
}
[IO.File]::WriteAllText($dbPath,$db,$utf8)

$bridge=[IO.File]::ReadAllText($bridgePath)

# Preserve an intentionally empty meter configuration. P25 previously created a
# default RX1 Signal container whenever no containers existed after restore.
$autoDefault='            if (MeterManager.TotalMeterContainers == 0)'+$nl+
             '                P25AddRx1SignalMeter();'+$nl+$nl
if($bridge.Contains($autoDefault))
{
    $bridge=$bridge.Replace($autoDefault,'')
}

$saveOld='            DB.SaveVars("SQ4KOU_ThetisMeters", ref a);'
$saveNew='            DB.ReplaceVars("SQ4KOU_ThetisMeters", ref a);'
if(!$bridge.Contains($saveOld) -and !$bridge.Contains($saveNew))
{
    throw 'P28 meter persistence anchor missing'
}
$bridge=$bridge.Replace($saveOld,$saveNew)

[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

# Hard gates: this build is invalid if either stale-key source remains.
$verifyDb=[IO.File]::ReadAllText($dbPath)
$verifyBridge=[IO.File]::ReadAllText($bridgePath)

if(!$verifyDb.Contains('public static void ReplaceVars(string tableName, ref ArrayList list)'))
{
    throw 'P28 ReplaceVars gate missing'
}
if(!$verifyDb.Contains('ds.Tables[tableName].Rows.Clear();'))
{
    throw 'P28 ReplaceVars clear gate missing'
}
if(!$verifyBridge.Contains('DB.ReplaceVars("SQ4KOU_ThetisMeters", ref a);'))
{
    throw 'P28 meter replace-all save gate missing'
}
if($verifyBridge.Contains('if (MeterManager.TotalMeterContainers == 0)'))
{
    throw 'P28 forced-default-container gate failed'
}

Write-Host 'P28_METER_PERSISTENCE=REPLACE_ALL'
Write-Host 'P28_STALE_METER_KEYS=PURGED_ON_SAVE'
Write-Host 'P28_ZERO_CONTAINERS=PRESERVED'
Write-Host 'P28_RENDERER=UNCHANGED_FROM_P27'
Write-Host 'P28_TELEMETRY=UNCHANGED_FROM_P27'
Write-Host 'P28_UI=UNCHANGED_FROM_P27'
