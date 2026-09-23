[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$managerPath=Join-Path $consoleDir 'P25_MeterManager.cs'
$src=Join-Path $PSScriptRoot 'P47MeterPersistence.cs'
$dst=Join-Path $consoleDir 'P47MeterPersistence.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

foreach($p in @($projPath,$bridgePath,$managerPath,$src)){
    if(!(Test-Path $p)){throw "P47 required file missing: $p"}
}

Copy-Item $src $dst -Force

# P47 is an adapter around the coherent Thetis MeterManager. The source-of-truth
# logic remains in P25_MeterManager.cs; partial only exposes the same private state
# to the persistence adapter in another file.
$mm=[IO.File]::ReadAllText($managerPath)
$classOld='    internal static class MeterManager'
$classNew='    internal static partial class MeterManager'
if(!$mm.Contains($classOld) -and !$mm.Contains($classNew)){
    throw 'P47 MeterManager class declaration anchor missing'
}
$mm=$mm.Replace($classOld,$classNew)
[IO.File]::WriteAllText($managerPath,$mm,$utf8)

$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P32ThetisCompatibility.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P47 csproj P32 compatibility anchor missing'}
if(!$proj.Contains('<Compile Include="P47MeterPersistence.cs" />')){
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P47MeterPersistence.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

$bridge=[IO.File]::ReadAllText($bridgePath).Replace("`r`n","`n")

# Restore: native Thetis RestoreSettings reconstructs container/meter/item state.
# P47 then applies authoritative floating-window geometry before renderers start.
$restoreOld='                if (settings.Count > 0) MeterManager.RestoreSettings(ref settings);'
$restoreNew=@'
                if (settings.Count > 0)
                {
                    bool restored = MeterManager.RestoreSettings(ref settings);
                    if (!restored)
                        System.Diagnostics.Debug.WriteLine("P47 Meters/Gadgets RestoreSettings reported failure.");

                    MeterManager.P47RestoreWindowPersistence(settings);
                }
'@
$restoreNew=$restoreNew.Replace("`r`n","`n")
if(!$bridge.Contains($restoreOld)){throw 'P47 restore anchor missing'}
$bridge=$bridge.Replace($restoreOld,$restoreNew)

# Save: normalize docked geometry first, then store the exact upstream 21-field
# container model + all meter/item/group settings, then append floating bounds.
$saveOld=@'
        private void P25SaveThetisMeters()
        {
            if (!p25MetersInitialised) return;
            var settings = new System.Collections.Generic.Dictionary<string, string>();
            MeterManager.StoreSettings2(ref settings);
            ArrayList a = new ArrayList();
            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in settings)
                a.Add(kvp.Key + "/" + kvp.Value);
            DB.ReplaceVars("SQ4KOU_ThetisMeters", ref a);
        }
'@
$saveOld=$saveOld.Replace("`r`n","`n")
$saveNew=@'
        private void P25SaveThetisMeters()
        {
            if (!p25MetersInitialised) return;

            MeterManager.P47PreparePersistenceSnapshot();

            var settings = new System.Collections.Generic.Dictionary<string, string>();
            bool stored = MeterManager.StoreSettings2(ref settings);
            if (!stored)
                throw new InvalidOperationException("Meters/Gadgets StoreSettings2 failed.");

            MeterManager.P47AppendWindowPersistence(ref settings);

            string audit;
            if (!MeterManager.P47AuditPersistenceSnapshot(settings, out audit))
                throw new InvalidOperationException("Meters/Gadgets persistence snapshot incomplete: " + audit);

            ArrayList a = new ArrayList();
            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in settings)
                a.Add(kvp.Key + "/" + kvp.Value);

            // P28 replace-all semantics remain authoritative: removed containers,
            // meter items and settings cannot resurrect from stale database rows.
            DB.ReplaceVars("SQ4KOU_ThetisMeters", ref a);

            // Keep native Thetis per-window form tables current too, so older
            // databases/builds remain a valid migration/fallback path.
            MeterManager.P47SaveLegacyWindowTables();

            System.Diagnostics.Debug.WriteLine("P47 Meters/Gadgets persistence saved: " + audit);
        }
'@
$saveNew=$saveNew.Replace("`r`n","`n")
if(!$bridge.Contains($saveOld)){
    # Defensive fallback if an earlier source still says SaveVars.
    $saveOld2=$saveOld.Replace('DB.ReplaceVars("SQ4KOU_ThetisMeters", ref a);','DB.SaveVars("SQ4KOU_ThetisMeters", ref a);')
    if(!$bridge.Contains($saveOld2)){throw 'P47 P25SaveThetisMeters anchor missing'}
    $bridge=$bridge.Replace($saveOld2,$saveNew)
}else{
    $bridge=$bridge.Replace($saveOld,$saveNew)
}

# Do not silently swallow persistence failures during shutdown. Keep shutdown
# progressing, but emit the exact exception to Debug output.
$shutdownOld=@'
            try { P25SaveThetisMeters(); } catch { }
            try { MeterManager.Shutdown(); } catch { }
'@
$shutdownNew=@'
            try { P25SaveThetisMeters(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P47 meter save failed: " + ex); }
            try { MeterManager.Shutdown(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P47 meter shutdown failed: " + ex); }
'@
$shutdownOld=$shutdownOld.Replace("`r`n","`n")
$shutdownNew=$shutdownNew.Replace("`r`n","`n")
if(!$bridge.Contains($shutdownOld)){throw 'P47 shutdown diagnostics anchor missing'}
$bridge=$bridge.Replace($shutdownOld,$shutdownNew)

[IO.File]::WriteAllText($bridgePath,$bridge.Replace("`n","`r`n"),$utf8)

# Hard audit gates: all persistence layers have to coexist in the generated source.
$verifyBridge=[IO.File]::ReadAllText($bridgePath)
$verifyManager=[IO.File]::ReadAllText($managerPath)
$verifyAdapter=[IO.File]::ReadAllText($dst)
$verifyProj=[IO.File]::ReadAllText($projPath)

foreach($token in @(
    'MeterManager.P47PreparePersistenceSnapshot();',
    'bool stored = MeterManager.StoreSettings2(ref settings);',
    'MeterManager.P47AppendWindowPersistence(ref settings);',
    'MeterManager.P47RestoreWindowPersistence(settings);',
    'DB.ReplaceVars("SQ4KOU_ThetisMeters", ref a);',
    'MeterManager.P47SaveLegacyWindowTables();',
    'P47AuditPersistenceSnapshot'
)){
    if(!$verifyBridge.Contains($token)){throw "P47 bridge gate missing: $token"}
}
foreach($token in @(
    'internal static partial class MeterManager',
    'public static bool StoreSettings2',
    'public static bool RestoreSettings',
    'meterContData_',
    'meterData_',
    'meterIGData_',
    'meterIGSettings_2_'
)){
    if(!$verifyManager.Contains($token)){throw "P47 MeterManager gate missing: $token"}
}
foreach($token in @(
    'meterWindowData_',
    'P47PreparePersistenceSnapshot',
    'P47AppendWindowPersistence',
    'P47RestoreWindowPersistence',
    'P47SaveLegacyWindowTables',
    'uc.DockedLocation = uc.Location;',
    'uc.DockedSize = uc.Size;',
    'Common.SaveForm(form, "MeterDisplay_" + kvp.Key);',
    'Common.ForceFormOnScreen(form);'
)){
    if(!$verifyAdapter.Contains($token)){throw "P47 adapter gate missing: $token"}
}
if(!$verifyProj.Contains('<Compile Include="P47MeterPersistence.cs" />')){
    throw 'P47 csproj compile gate missing'
}

Write-Host 'P47_METER_PERSISTENCE=AUDITED_AND_HARDENED'
Write-Host 'P47_METER_MAIN_STORE=THETIS_STORESETTINGS2_REPLACE_ALL'
Write-Host 'P47_METER_DOCKED_GEOMETRY=ACTUAL_LOCATION_SIZE_NORMALIZED_BEFORE_STORE'
Write-Host 'P47_METER_FLOATING_GEOMETRY=AUTHORITATIVE_MAIN_TABLE_PLUS_LEGACY_FORM_TABLE'
Write-Host 'P47_METER_RESTORE=MAIN_TABLE_GEOMETRY_BEFORE_RENDERER_START'
Write-Host 'P47_METER_ITEMS=THETIS_METERDATA_METERIGDATA_METERIGSETTINGS2'
Write-Host 'P47_METER_STALE_KEYS=PURGED_BY_REPLACEVARS'
Write-Host 'P47_METER_SAVE_AUDIT=CONTAINER_FORM_GEOMETRY_COUNTS_REQUIRED'
Write-Host 'P47_RX2=NOT_EXPOSED'
