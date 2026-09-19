[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P07] $s" }

$databaseCs = Join-Path $SourceRoot 'Console\database.cs'
$consoleCs  = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $databaseCs)) { throw "PowerSDR database source missing: $databaseCs" }
if(!(Test-Path -LiteralPath $consoleCs))  { throw "PowerSDR console source missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P07 anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P07 anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

function Replace-CSharpMethod([string]$Text, [string]$Signature, [string]$Replacement, [string]$Label) {
    $Replacement = $Replacement.Replace("`r`n", "`n")
    $start = $Text.IndexOf($Signature, [StringComparison]::Ordinal)
    if($start -lt 0) { throw "P07 method signature missing: $Label" }
    if($Text.IndexOf($Signature, $start + $Signature.Length, [StringComparison]::Ordinal) -ge 0) {
        throw "P07 method signature not unique: $Label"
    }

    $brace = $Text.IndexOf('{', $start)
    if($brace -lt 0) { throw "P07 method opening brace missing: $Label" }

    $depth = 0
    $end = -1
    for($i=$brace; $i -lt $Text.Length; $i++) {
        if($Text[$i] -eq '{') { $depth++ }
        elseif($Text[$i] -eq '}') {
            $depth--
            if($depth -eq 0) { $end = $i + 1; break }
        }
    }
    if($end -lt 0) { throw "P07 method closing brace missing: $Label" }

    return $Text.Substring(0,$start) + $Replacement + $Text.Substring($end)
}

# ---------------------------------------------------------------------------
# 1) DB.SaveVars: replace repeated DataTable.Select scans with one in-memory
#    index. Preserve KE9NS semantics for duplicate keys and table case/locale.
# ---------------------------------------------------------------------------
$db = [IO.File]::ReadAllText($databaseCs).Replace("`r`n", "`n")

$newSaveVars = @'
        public static void SaveVars(string tableName, ref ArrayList list)
        {
            if (!ds.Tables.Contains(tableName)) AddFormTable(tableName);

            DataTable table = ds.Tables[tableName];

            // SQ4KOU P07: the original code called DataTable.Select once for
            // every setting. State/Options contain hundreds/thousands of keys,
            // so shutdown repeatedly parsed an expression and rescanned the
            // whole table. Build one key index instead: O(rows + settings).
            System.StringComparer keyComparer =
                System.StringComparer.Create(table.Locale, !table.CaseSensitive);

            System.Collections.Generic.Dictionary<string, DataRow> rowsByKey =
                new System.Collections.Generic.Dictionary<string, DataRow>(keyComparer);

            System.Collections.Generic.HashSet<string> duplicateKeys =
                new System.Collections.Generic.HashSet<string>(keyComparer);

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted ||
                    row.RowState == DataRowState.Detached ||
                    row.IsNull(0))
                    continue;

                string key = row[0].ToString();
                DataRow existing;

                if (rowsByKey.TryGetValue(key, out existing))
                {
                    // Native KE9NS SaveVars intentionally leaves a key
                    // untouched when Select() returns more than one row.
                    rowsByKey.Remove(key);
                    duplicateKeys.Add(key);
                }
                else if (!duplicateKeys.Contains(key))
                {
                    rowsByKey.Add(key, row);
                }
            }

            foreach (string s in list)
            {
                if (s == null) continue;

                int separator = s.IndexOf('/');
                if (separator < 0) continue; // native behaviour: no value supplied

                string key = s.Substring(0, separator);
                string value = s.Substring(separator + 1); // preserves embedded '/'

                if (duplicateKeys.Contains(key))
                    continue; // preserve native rows.Length > 1 behaviour

                DataRow row;
                if (rowsByKey.TryGetValue(key, out row))
                {
                    row[1] = value;
                }
                else
                {
                    DataRow newRow = table.NewRow();
                    newRow[0] = key;
                    newRow[1] = value;
                    table.Rows.Add(newRow);
                    rowsByKey.Add(key, newRow);
                }
            }
        }
'@

$db = Replace-CSharpMethod $db '        public static void SaveVars(string tableName, ref ArrayList list)' $newSaveVars 'DB.SaveVars'

$dbChecks = @(
    @{Name='indexed SaveVars'; Ok=$db.Contains('Dictionary<string, DataRow> rowsByKey')},
    @{Name='locale/case comparer'; Ok=$db.Contains('StringComparer.Create(table.Locale, !table.CaseSensitive)')},
    @{Name='duplicate semantics'; Ok=$db.Contains('HashSet<string> duplicateKeys') -and $db.Contains('duplicateKeys.Contains(key)')},
    @{Name='deleted rows skipped'; Ok=$db.Contains('row.RowState == DataRowState.Deleted')},
    @{Name='repeated Select removed from SaveVars'; Ok=(!$newSaveVars.Contains('table.Select(')) -and (!$newSaveVars.Contains('ds.Tables[tableName].Select('))}
)
$failed = @($dbChecks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P07 DB post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($databaseCs, $db.Replace("`n", "`r`n"), $utf8)

# ---------------------------------------------------------------------------
# 2) Main-window persistence: capture WindowState + restore rectangle at the
#    first line of Console_Closing, before Power OFF / Hide / Dispose touches
#    the form. SaveState then consumes the captured values.
# ---------------------------------------------------------------------------
$console = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")

$stateSignature = '        public void SaveState()'
$stateIndex = $console.IndexOf($stateSignature, [StringComparison]::Ordinal)
if($stateIndex -lt 0) { throw 'P07 SaveState insertion point missing' }

$windowFields = @'
        // SQ4KOU P07: immutable close-time main-window snapshot.
        private bool sq4kouClosingWindowCaptured = false;
        private FormWindowState sq4kouClosingWindowState = FormWindowState.Normal;
        private Rectangle sq4kouClosingRestoreBounds = Rectangle.Empty;

'@
$console = $console.Substring(0,$stateIndex) + $windowFields + $console.Substring($stateIndex)

$oldTimer = '            Stopwatch sq4kouShutdown1Timer = Stopwatch.StartNew();'
$newTimer = @'
            Stopwatch sq4kouShutdown1Timer = Stopwatch.StartNew();

            // Capture before chkPower changes, Hide() and any child-form teardown.
            sq4kouClosingWindowState = this.WindowState;
            sq4kouClosingRestoreBounds =
                (this.WindowState == FormWindowState.Normal) ? this.Bounds : this.RestoreBounds;
            sq4kouClosingWindowCaptured = true;
'@
$console = Replace-ExactOnce $console $oldTimer $newTimer 'pre-hide window snapshot'

$oldWindowSave = @'
            // Preserve both the actual window state and the NORMAL restore rectangle.
            // Reading Top/Left/Width/Height while maximized stores the monitor work area,
            // not the user's restored window rectangle.
            Rectangle consoleBoundsToSave =
                (this.WindowState == FormWindowState.Normal) ? this.Bounds : this.RestoreBounds;

            a.Add("console_zaximize/" +
                (this.WindowState == FormWindowState.Maximized ? "Maximized" : "Normal"));

            a.Add("console_top/" + consoleBoundsToSave.Top.ToString());      // save normal restore position
            a.Add("console_left/" + consoleBoundsToSave.Left.ToString());
            a.Add("console_width/" + consoleBoundsToSave.Width.ToString());
            a.Add("console_height/" + consoleBoundsToSave.Height.ToString());
'@

$newWindowSave = @'
            // Preserve both the actual window state and the NORMAL restore rectangle.
            // On application close prefer the snapshot captured before Hide()/teardown.
            Rectangle consoleBoundsToSave = sq4kouClosingWindowCaptured
                ? sq4kouClosingRestoreBounds
                : ((this.WindowState == FormWindowState.Normal) ? this.Bounds : this.RestoreBounds);

            FormWindowState consoleWindowStateToSave = sq4kouClosingWindowCaptured
                ? sq4kouClosingWindowState
                : this.WindowState;

            a.Add("console_zaximize/" +
                (consoleWindowStateToSave == FormWindowState.Maximized ? "Maximized" : "Normal"));

            a.Add("console_top/" + consoleBoundsToSave.Top.ToString());      // save normal restore position
            a.Add("console_left/" + consoleBoundsToSave.Left.ToString());
            a.Add("console_width/" + consoleBoundsToSave.Width.ToString());
            a.Add("console_height/" + consoleBoundsToSave.Height.ToString());
'@

$console = Replace-ExactOnce $console $oldWindowSave $newWindowSave 'SaveState close-time window values'

$windowChecks = @(
    @{Name='window capture fields'; Ok=$console.Contains('sq4kouClosingWindowCaptured') -and $console.Contains('sq4kouClosingRestoreBounds')},
    @{Name='capture before teardown'; Ok=$console.Contains('sq4kouClosingWindowState = this.WindowState;')},
    @{Name='captured state used'; Ok=$console.Contains('consoleWindowStateToSave = sq4kouClosingWindowCaptured')},
    @{Name='legacy key preserved'; Ok=$console.Contains('console_zaximize/')},
    @{Name='max restore still active'; Ok=$console.Contains('this.WindowState = FormWindowState.Maximized;')}
)
$failed = @($windowChecks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P07 window post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs, $console.Replace("`n", "`r`n"), $utf8)

Stage 'PASS: indexed DB.SaveVars + pre-hide maximized/RestoreBounds snapshot'
