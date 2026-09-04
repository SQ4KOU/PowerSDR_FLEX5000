param(
    [Parameter(Mandatory=$true)]
    [string]$PackRoot
)

$ErrorActionPreference = 'Stop'
$path = Join-Path $PackRoot 'PATCHES/13_DB_RELIABILITY_P0_P2.ps1'
if (-not (Test-Path -LiteralPath $path)) { throw "DB patch not found: $path" }

$text = [IO.File]::ReadAllText($path).Replace("`r`n", "`n")

function Replace-ScriptExactOnce([string]$source, [string]$old, [string]$new, [string]$name)
{
    $count = ([regex]::Matches($source, [regex]::Escape($old))).Count
    if ($count -ne 1) { throw "$name count=$count, expected 1" }
    return $source.Replace($old, $new)
}

# EU2AV adds VerifyTXProfileColumns() before WriteDB(). Preserve it while SQ4KOU
# P0-P2 removes only the unsafe write from VerifyTables().
$oldPattern = '(?s)(\$oldVerifyTail\s*=\s*@''\n.*?AddTXProfileTable\("TXProfileDef", true\);)(\n\n\s*WriteDB\(\);\n\s*}\n''@)'
$oldMatches = [regex]::Matches($text, $oldPattern)
if ($oldMatches.Count -ne 1) {
    throw "DB P0-P2 old VerifyTables compatibility anchor count=$($oldMatches.Count), expected 1"
}
$text = [regex]::Replace(
    $text,
    $oldPattern,
    '$1' + "`n`n            VerifyTXProfileColumns();" + '$2',
    1
)

$newPattern = '(?s)(\$newVerifyTail\s*=\s*@''\n.*?AddTXProfileTable\("TXProfileDef", true\);)(\n\s*}\n''@)'
$newMatches = [regex]::Matches($text, $newPattern)
if ($newMatches.Count -ne 1) {
    throw "DB P0-P2 new VerifyTables compatibility anchor count=$($newMatches.Count), expected 1"
}
$text = [regex]::Replace(
    $text,
    $newPattern,
    '$1' + "`n`n            VerifyTXProfileColumns();" + '$2',
    1
)

# EU2AV already propagates the bool result from checkVersion and passes an
# explicit schema/data compatibility result. Keep that call instead of forcing
# the older 3-argument Ramdor call expected by the original P0-P2 script.
$oldPropagation = '    $dbm = Replace-ExactOnce $dbm "                        if(ok) checkVersion(made_new, ctrl_key_force_update, updateFile);" "                        if(ok) ok = checkVersion(made_new, ctrl_key_force_update, updateFile);" "checkVersion return propagation"'
$newPropagation = @'
    if ($dbm -notmatch 'ok\s*=\s*checkVersion\(made_new,\s*ctrl_key_force_update,\s*updateFile,\s*schema_mismatch,\s*mismatch_reason\);') {
        throw "Brak kotwicy: EU2AV schema-aware checkVersion propagation"
    }
    Write-Log "clsDBMan.cs: EU2AV schema-aware checkVersion bool propagation zachowane"
'@.TrimEnd()
$text = Replace-ScriptExactOnce $text $oldPropagation $newPropagation 'DB P0-P2 checkVersion propagation adapter'

$oldNewMethodSig = '        private static bool checkVersion(bool made_new, bool force_upgrade = false, bool force_upgrade_via_file = false)'
$newNewMethodSig = '        private static bool checkVersion(bool made_new, bool force_upgrade = false, bool force_upgrade_via_file = false, bool schema_mismatch = false, string schema_mismatch_reason = "")'
$text = Replace-ScriptExactOnce $text $oldNewMethodSig $newNewMethodSig 'DB P0-P2 generated checkVersion signature'

$oldEarlyReturn = '            if (!force_upgrade && !schemaUpgradeRequired && Common.GetVerNum() == version) return true;'
$newEarlyReturn = '            if (!force_upgrade && !force_upgrade_via_file && !schemaUpgradeRequired && !schema_mismatch && Common.GetVerNum() == version) return true;'
$text = Replace-ScriptExactOnce $text $oldEarlyReturn $newEarlyReturn 'DB P0-P2 checkVersion trigger condition'

$oldForceInfo = '            string force_info = force_upgrade ? "Force database update requested.\n\n" : "";'
$newForceInfo = '            string force_info = force_upgrade ? "Force database update requested.\n\n" : (force_upgrade_via_file ? "Database update requested by updatedb.txt.\n\n" : (schema_mismatch ? "Database schema/data compatibility check requires an update.\n" + schema_mismatch_reason + "\n\n" : ""));'
$text = Replace-ScriptExactOnce $text $oldForceInfo $newForceInfo 'DB P0-P2 schema mismatch reason propagation'

$oldReplaceMethod = '    $dbm = Replace-Method $dbm "        private static void checkVersion(bool made_new, bool force_upgrade = false, bool force_upgrade_via_file = false)" $newCheckVersion'
$newReplaceMethod = '    $dbm = Replace-Method $dbm "        private static bool checkVersion(bool made_new, bool force_upgrade = false, bool force_upgrade_via_file = false, bool schema_mismatch = false, string schema_mismatch_reason = \"\")" $newCheckVersion'
$text = Replace-ScriptExactOnce $text $oldReplaceMethod $newReplaceMethod 'DB P0-P2 EU2AV checkVersion method anchor'

$oldPostcheck = '        @{ Name="checkVersion propagated"; Ok=$dbm.Contains("if(ok) ok = checkVersion") },'
$newPostcheck = '        @{ Name="checkVersion propagated"; Ok=$dbm.Contains("ok = checkVersion(made_new, ctrl_key_force_update, updateFile, schema_mismatch, mismatch_reason);") },'
$text = Replace-ScriptExactOnce $text $oldPostcheck $newPostcheck 'DB P0-P2 checkVersion propagation postcheck'

if (($text.Split('VerifyTXProfileColumns();').Count - 1) -lt 2) {
    throw 'DB P0-P2 VerifyTXProfileColumns compatibility postcheck failed'
}
if (-not $text.Contains('schema_mismatch_reason')) {
    throw 'DB P0-P2 schema-aware compatibility postcheck failed'
}

[IO.File]::WriteAllText($path, $text.Replace("`n", "`r`n"), [Text.UTF8Encoding]::new($true))
Write-Host 'DB_P0_P2_EU2AV_COMPAT=PASS'
