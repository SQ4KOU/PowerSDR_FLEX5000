param(
    [Parameter(Mandatory=$true)]
    [string]$PackRoot
)

$ErrorActionPreference = 'Stop'
$path = Join-Path $PackRoot 'PATCHES/13_DB_RELIABILITY_P0_P2.ps1'
if (-not (Test-Path -LiteralPath $path)) { throw "DB patch not found: $path" }

$text = [IO.File]::ReadAllText($path).Replace("`r`n", "`n")

# The clean EU2AV+Ramdor synthesis contains EU2AV's VerifyTXProfileColumns()
# between AddTXProfileTable(TXProfileDef) and WriteDB(). Preserve that migration
# helper while allowing SQ4KOU P0-P2 to remove only the WriteDB() call.
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

if (($text.Split('VerifyTXProfileColumns();').Count - 1) -lt 2) {
    throw 'DB P0-P2 compatibility postcheck failed'
}

[IO.File]::WriteAllText($path, $text.Replace("`n", "`r`n"), [Text.UTF8Encoding]::new($true))
Write-Host 'DB_P0_P2_EU2AV_COMPAT=PASS'
