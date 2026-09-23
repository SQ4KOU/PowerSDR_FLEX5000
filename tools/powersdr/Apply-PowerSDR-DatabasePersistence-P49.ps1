[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$consoleDir = Join-Path $SourceRoot 'Console'
$dbPath = Join-Path $consoleDir 'database.cs'
$setupPath = Join-Path $consoleDir 'setup.cs'
$commonPath = Join-Path $consoleDir 'common.cs'
$spotWatchPath = Join-Path $consoleDir 'SpotWatchBox.cs'
$wavePath = Join-Path $consoleDir 'wave.cs'
$spotDecoderPath = Join-Path $consoleDir 'spot_decoder.cs'
$preSelPath = Join-Path $consoleDir 'FWC\PreSelForm.cs'
$filterPath = Join-Path $consoleDir 'FilterForm.cs'
$legacyPath = Join-Path $consoleDir 'P39LegacyItems.cs'
$tciPath = Join-Path $consoleDir 'P44TCI.cs'

$required = @($dbPath,$setupPath,$commonPath,$spotWatchPath,$wavePath,$spotDecoderPath,$preSelPath,$filterPath,$legacyPath,$tciPath)
foreach($p in $required){ if(!(Test-Path $p)){ throw "P49 required file missing: $p" } }

$utf8 = New-Object Text.UTF8Encoding($true)
$utf8NoBom = New-Object Text.UTF8Encoding($false)
$lf = [string][char]10
$crlf = [string][char]13 + [char]10

function Normalize([string]$s){ return $s.Replace($crlf,$lf) }

function Replace-CSharpMethod([string]$Text,[string]$Signature,[string]$Replacement,[string]$Label)
{
    $Text = Normalize $Text
    $Replacement = Normalize $Replacement
    $start = $Text.IndexOf($Signature,[StringComparison]::Ordinal)
    if($start -lt 0){ throw "P49 method signature missing: $Label" }
    if($Text.IndexOf($Signature,$start+$Signature.Length,[StringComparison]::Ordinal) -ge 0){
        throw "P49 method signature not unique: $Label"
    }
    $brace = $Text.IndexOf('{',$start)
    if($brace -lt 0){ throw "P49 method opening brace missing: $Label" }
    $depth=0
    $end=-1
    for($i=$brace;$i -lt $Text.Length;$i++){
        if($Text[$i] -eq '{'){ $depth++ }
        elseif($Text[$i] -eq '}'){
            $depth--
            if($depth -eq 0){ $end=$i+1; break }
        }
    }
    if($end -lt 0){ throw "P49 method closing brace missing: $Label" }
    return $Text.Substring(0,$start)+$Replacement+$Text.Substring($end)
}

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label)
{
    $Text=Normalize $Text
    $Old=Normalize $Old
    $New=Normalize $New
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P49 anchor missing: $Label" }
    if($Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal) -ge 0){
        throw "P49 anchor not unique: $Label"
    }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}


$newTryLoadDatabaseFile=@'
        private static bool TryLoadDatabaseFile(string path, out DataSet loaded, out string error)
        {
            loaded = null;
            error = "";
            last_logical_repair_summary = "";

            if (String.IsNullOrEmpty(path) || !File.Exists(path))
            {
                error = "File does not exist: " + path;
                return false;
            }

            try
            {
                DataSet candidate = new DataSet("Data");
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    candidate.ReadXml(stream, XmlReadMode.ReadSchema);
                }

                string repairs;
                string fatal;
                if (!P49ValidateLogicalDatabase(candidate, out repairs, out fatal))
                    throw new InvalidDataException("Logical database validation failed: " + fatal);

                last_logical_repair_summary = repairs;
                loaded = candidate;
                return true;
            }
            catch (Exception ex)
            {
                error = path + ": " + ex.Message;
                return false;
            }
        }
'@
$db = Replace-CSharpMethod $db '        private static bool TryLoadDatabaseFile(string path, out DataSet loaded, out string error)' $newTryLoadDatabaseFile 'P49 logical TryLoadDatabaseFile'

$newDbInit=@'
        public static bool Init(Model model)
        {
            if (file_name.Contains("database_F") || file_name.Contains("database_D"))
            {
                file_name1 = file_name;
                file_name = file_name.Replace("database_", "database-RevQ_");
            }

            bool database_exists = false;
            bool loaded_from_legacy = false;
            ds = new DataSet("Data");

            if (File.Exists(file_name))
            {
                DataSet loaded;
                string loadError;
                if (TryLoadDatabaseFile(file_name, out loaded, out loadError))
                {
                    string logicalRepairs = last_logical_repair_summary;
                    ds = loaded;
                    database_exists = true;

                    if (!String.IsNullOrEmpty(logicalRepairs))
                    {
                        PreserveCorruptDatabase(file_name);
                        MessageBox.Show(
                            "Logical database damage was detected even though the XML file was readable.\n\n" +
                            "PowerSDR repaired the database in memory and preserved the pre-repair file for diagnosis.\n\n" +
                            logicalRepairs,
                            "Database Logical Repair",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    PreserveCorruptDatabase(file_name);

                    DataSet recovered;
                    string recoverySource;
                    string recoveryErrors;
                    if (TryLoadFirstValidBackup(out recovered, out recoverySource, out recoveryErrors))
                    {
                        ds = recovered;
                        database_exists = true;
                        MessageBox.Show(
                            "The active database failed structural/logical validation and has been preserved for diagnosis.\n\n" +
                            "PowerSDR recovered the most recent valid backup:\n" + recoverySource +
                            (String.IsNullOrEmpty(last_logical_repair_summary) ? "" :
                                "\n\nThe backup also required safe key/value repair:\n" + last_logical_repair_summary),
                            "Database Recovered",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            "The active database is unusable and no structurally/logically valid backup could be loaded.\n\n" +
                            "Primary error:\n" + loadError +
                            (recoveryErrors.Length > 0 ? "\n\nBackup errors:\n" + recoveryErrors : "") +
                            "\n\nThe unreadable database was preserved. A new default database will be created.",
                            "ERROR: Database Recovery Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ds = new DataSet("Data");
                    }
                }
            }
            else if (!String.IsNullOrEmpty(file_name1) && File.Exists(file_name1))
            {
                DataSet legacy;
                string legacyError;
                if (TryLoadDatabaseFile(file_name1, out legacy, out legacyError))
                {
                    ds = legacy;
                    database_exists = true;
                    loaded_from_legacy = true;
                }
                else
                {
                    PreserveCorruptDatabase(file_name1);
                    MessageBox.Show(
                        "The legacy PowerSDR database failed structural/logical validation.\n\n" + legacyError +
                        "\n\nThe original file was left unchanged and a new default RevQ database will be created.",
                        "ERROR: Legacy Database Import Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (loaded_from_legacy)
                AddBandStackSWL();

            VerifyTables(model);
            CheckBandTextValid();

            if (database_exists)
            {
                try
                {
                    lock (db_io_lock)
                    {
                        CreateValidatedSessionBackup();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "A database backup operation failed.\n\n" + ex.Message +
                        "\n\nThe active database remains in use and existing valid backups are left intact.",
                        "ERROR: Database Backup Creation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return database_exists;
        }
'@
$db = Replace-CSharpMethod $db '        public static bool Init(Model model)' $newDbInit 'P49 logical Init'

$newDbUpdate=@'
        public static void Update()
        {
            if (ds == null || String.IsNullOrEmpty(file_name)) return;

            try
            {
                lock (db_io_lock)
                {
                    string repairs;
                    string fatal;
                    if (!P49ValidateLogicalDatabase(ds, out repairs, out fatal))
                        throw new InvalidDataException("Logical database validation failed before write: " + fatal);

                    if (!String.IsNullOrEmpty(repairs))
                        Debug.WriteLine("P49 repaired logical database before write: " + repairs);

                    AtomicWriteDataSet(ds, file_name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "A database write operation was blocked because the database could not be safely validated/written.\n\n" +
                    "The previous database file was not intentionally deleted.\n\n" +
                    ex.Message,
                    "ERROR: Database Write Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
'@
$db = Replace-CSharpMethod $db '        public static void Update()' $newDbUpdate 'P49 logical Update'

# ---------------------------------------------------------------------------
# 1. Database API: one lock for DataSet mutation, reads, disk snapshots and exit.
#    P03 already provides atomic disk writes; P49 removes races before serialization.
# ---------------------------------------------------------------------------
$db = Normalize ([IO.File]::ReadAllText($dbPath))

# P49 logical-integrity layer. The P03 reliability patch validated only that
# XML could be parsed and contained tables. That misses syntactically-valid
# logical damage such as duplicate Key rows, broken key/value table schemas,
# and malformed core tables.
$dbLogicalAnchor='        private static bool TryLoadDatabaseFile(string path, out DataSet loaded, out string error)'
if(!$db.Contains($dbLogicalAnchor)){ throw 'P49 logical DB loader anchor missing' }

$dbLogicalHelpers=@'
        private static string last_logical_repair_summary = "";

        private static bool P49HasColumns(DataTable table, params string[] names)
        {
            if (table == null) return false;
            foreach (string name in names)
                if (!table.Columns.Contains(name)) return false;
            return true;
        }

        private static bool P49IsKnownKeyValueTableName(string name)
        {
            if (String.IsNullOrEmpty(name)) return false;
            if (name == "State" ||
                name == "Options" ||
                name == "SQ4KOU_TCI" ||
                name == "SQ4KOU_LegacyItems" ||
                name == "SQ4KOU_ThetisMeters")
                return true;

            if (name.StartsWith("MeterDisplay_", StringComparison.Ordinal))
                return true;

            return false;
        }

        private static bool P49ValidateLogicalDatabase(
            DataSet candidate,
            out string repairSummary,
            out string fatalError)
        {
            repairSummary = "";
            fatalError = "";

            if (candidate == null)
            {
                fatalError = "Database DataSet is null.";
                return false;
            }

            if (candidate.Tables.Count == 0)
            {
                fatalError = "Database contains no tables.";
                return false;
            }

            // Core tables may be absent in an older database and VerifyTables()
            // will create them. If present, however, their structural schema must
            // be usable or the database is logically corrupt.
            DataTable t;
            if (candidate.Tables.Contains("BandText"))
            {
                t = candidate.Tables["BandText"];
                if (!P49HasColumns(t, "Low", "High", "Name", "TX"))
                {
                    fatalError = "BandText table schema is incomplete.";
                    return false;
                }
            }

            if (candidate.Tables.Contains("BandStack"))
            {
                t = candidate.Tables["BandStack"];
                if (!P49HasColumns(t, "BandName", "Mode", "Filter", "Freq"))
                {
                    fatalError = "BandStack table schema is incomplete.";
                    return false;
                }
            }

            if (candidate.Tables.Contains("TXProfile"))
            {
                t = candidate.Tables["TXProfile"];
                if (!t.Columns.Contains("Name"))
                {
                    fatalError = "TXProfile table has no Name column.";
                    return false;
                }
            }

            if (candidate.Tables.Contains("TXProfileDef"))
            {
                t = candidate.Tables["TXProfileDef"];
                if (!t.Columns.Contains("Name"))
                {
                    fatalError = "TXProfileDef table has no Name column.";
                    return false;
                }
            }

            int duplicateRowsRemoved = 0;
            int emptyKeysRemoved = 0;
            int keyValueTablesChecked = 0;

            foreach (DataTable table in candidate.Tables)
            {
                bool hasKey = table.Columns.Contains("Key");
                bool hasValue = table.Columns.Contains("Value");
                bool knownKeyValue = P49IsKnownKeyValueTableName(table.TableName);

                if (knownKeyValue && (!hasKey || !hasValue))
                {
                    fatalError = "Key/value table '" + table.TableName +
                        "' has a damaged schema (Key/Value column missing).";
                    return false;
                }

                if (!hasKey || !hasValue)
                    continue;

                keyValueTablesChecked++;

                System.Collections.Generic.Dictionary<string, DataRow> lastByKey =
                    new System.Collections.Generic.Dictionary<string, DataRow>(
                        System.StringComparer.Create(table.Locale, !table.CaseSensitive));

                System.Collections.Generic.List<DataRow> remove =
                    new System.Collections.Generic.List<DataRow>();

                foreach (DataRow row in table.Rows)
                {
                    if (row.RowState == DataRowState.Deleted ||
                        row.RowState == DataRowState.Detached)
                        continue;

                    if (row.IsNull("Key") || String.IsNullOrEmpty(row["Key"].ToString()))
                    {
                        remove.Add(row);
                        emptyKeysRemoved++;
                        continue;
                    }

                    string key = row["Key"].ToString();
                    DataRow previous;
                    if (lastByKey.TryGetValue(key, out previous))
                    {
                        // Keep the most recently appended row. This matches the
                        // only deterministic ordering available in legacy XML.
                        remove.Add(previous);
                        duplicateRowsRemoved++;
                    }
                    lastByKey[key] = row;
                }

                foreach (DataRow row in remove)
                {
                    if (row.RowState != DataRowState.Detached &&
                        row.RowState != DataRowState.Deleted)
                        table.Rows.Remove(row);
                }
            }

            if (duplicateRowsRemoved > 0 || emptyKeysRemoved > 0)
            {
                repairSummary =
                    "key/value tables checked=" + keyValueTablesChecked.ToString() +
                    ", duplicate rows removed=" + duplicateRowsRemoved.ToString() +
                    ", empty-key rows removed=" + emptyKeysRemoved.ToString();
            }

            return true;
        }

'@
$db=$db.Replace($dbLogicalAnchor,$dbLogicalHelpers+$dbLogicalAnchor)

$newSaveVars = @'
        public static void SaveVars(string tableName, ref ArrayList list)
        {
            lock (db_io_lock)
            {
                if (ds == null) return;
                if (!ds.Tables.Contains(tableName)) AddFormTable(tableName);

                DataTable table = ds.Tables[tableName];

                System.StringComparer keyComparer =
                    System.StringComparer.Create(table.Locale, !table.CaseSensitive);

                System.Collections.Generic.Dictionary<string, DataRow> rowsByKey =
                    new System.Collections.Generic.Dictionary<string, DataRow>(keyComparer);

                System.Collections.Generic.List<DataRow> duplicateRows =
                    new System.Collections.Generic.List<DataRow>();

                foreach (DataRow row in table.Rows)
                {
                    if (row.RowState == DataRowState.Deleted ||
                        row.RowState == DataRowState.Detached ||
                        row.IsNull(0) ||
                        String.IsNullOrEmpty(row[0].ToString()))
                        continue;

                    string key = row[0].ToString();
                    DataRow existing;
                    if (rowsByKey.TryGetValue(key, out existing))
                        duplicateRows.Add(existing); // keep the newest row
                    rowsByKey[key] = row;
                }

                foreach (DataRow duplicate in duplicateRows)
                {
                    if (duplicate.RowState != DataRowState.Deleted &&
                        duplicate.RowState != DataRowState.Detached)
                        table.Rows.Remove(duplicate);
                }

                foreach (string s in list)
                {
                    if (s == null) continue;
                    int separator = s.IndexOf('/');
                    if (separator < 0) continue;

                    string key = s.Substring(0, separator);
                    string value = s.Substring(separator + 1);

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
        }
'@
$db = Replace-CSharpMethod $db '        public static void SaveVars(string tableName, ref ArrayList list)' $newSaveVars 'DB.SaveVars synchronized'

$newGetVars = @'
        public static ArrayList GetVars(string tableName)
        {
            lock (db_io_lock)
            {
                ArrayList list = new ArrayList();
                if (ds == null || !ds.Tables.Contains(tableName))
                    return list;

                DataTable t = ds.Tables[tableName];
                for (int i = 0; i < t.Rows.Count; i++)
                {
                    if (t.Rows[i].RowState == DataRowState.Deleted ||
                        t.Rows[i].RowState == DataRowState.Detached)
                        continue;
                    list.Add(t.Rows[i][0].ToString() + "/" + t.Rows[i][1].ToString());
                }
                return list;
            }
        }
'@
$db = Replace-CSharpMethod $db '        public static ArrayList GetVars(string tableName)' $newGetVars 'DB.GetVars synchronized'

$newReplaceVars = @'
        public static void ReplaceVars(string tableName, ref ArrayList list)
        {
            lock (db_io_lock)
            {
                if (ds == null) return;
                if (!ds.Tables.Contains(tableName)) AddFormTable(tableName);
                ds.Tables[tableName].Rows.Clear();
                SaveVars(tableName, ref list);
            }
        }
'@
$db = Replace-CSharpMethod $db '        public static void ReplaceVars(string tableName, ref ArrayList list)' $newReplaceVars 'DB.ReplaceVars synchronized'

$newImport = @'
        public static bool ImportDatabase(string filename)
        {
            DataSet file = new DataSet();
            try
            {
                file.ReadXml(filename);
            }
            catch (Exception)
            {
                return false;
            }

            if (file.Tables.Contains("BandStack"))
            {
                DataRow[] rows = file.Tables["BandStack"].Select("Mode = 'FMN'");
                foreach (DataRow dr in rows)
                    dr["Mode"] = "FM";
            }

            lock (db_io_lock)
            {
                ds = file;
            }
            return true;
        }
'@
$db = Replace-CSharpMethod $db '        public static bool ImportDatabase(string filename)' $newImport 'DB.ImportDatabase synchronized'

$newExit = @'
        public static void Exit()
        {
            lock (db_io_lock)
            {
                Update();
                ds = null;
            }
        }
'@
$db = Replace-CSharpMethod $db '        public static void Exit()' $newExit 'DB.Exit synchronized'

[IO.File]::WriteAllText($dbPath,$db.Replace($lf,$crlf),$utf8NoBom)

# ---------------------------------------------------------------------------
# 2. Setup options: serialize ALL supported WinForms/TS controls, not only TS.
#    Eliminate background threads that race DB.Exit and touch WinForms off UI thread.
# ---------------------------------------------------------------------------
$setup = Normalize ([IO.File]::ReadAllText($setupPath))

$newSetupControlList = @'
        private void ControlList(Control c, ref ArrayList a)
        {
            if (c.Controls.Count > 0)
            {
                foreach (Control c2 in c.Controls)
                    ControlList(c2, ref a);
            }

            if (c is CheckBox ||
                c is ComboBox ||
                c is NumericUpDown ||
                c is RadioButton ||
                c is TextBox ||
                c is TrackBar ||
                c is ColorButton)
                a.Add(c);
        }
'@
$setup = Replace-CSharpMethod $setup '        private void ControlList(Control c, ref ArrayList a)' $newSetupControlList 'Setup.ControlList'

$helperAnchor='        private static bool saving = false;'
if(!$setup.Contains($helperAnchor)){ throw 'P49 setup saving anchor missing' }
$setupHelpers = @'
        private bool P49IsPersistentOptionControl(Control c)
        {
            if (c == null || String.IsNullOrEmpty(c.Name)) return false;

            // P39/P44 dynamically-created controls own dedicated DB tables.
            if (c.Name.StartsWith("p39", StringComparison.OrdinalIgnoreCase) ||
                c.Name.StartsWith("p44", StringComparison.OrdinalIgnoreCase))
                return false;

            return c is CheckBox ||
                   c is ComboBox ||
                   c is NumericUpDown ||
                   c is RadioButton ||
                   c is TextBox ||
                   c is TrackBar ||
                   c is ColorButton;
        }

        private bool P49SerializeOptionControl(Control c, ArrayList a)
        {
            if (!P49IsPersistentOptionControl(c)) return false;

            ColorButton color = c as ColorButton;
            if (color != null)
            {
                Color clr = color.Color;
                a.Add(c.Name + "/" + clr.R + "." + clr.G + "." + clr.B + "." + clr.A);
                return true;
            }

            CheckBox check = c as CheckBox;
            if (check != null) { a.Add(c.Name + "/" + check.Checked.ToString()); return true; }

            ComboBox combo = c as ComboBox;
            if (combo != null) { a.Add(c.Name + "/" + combo.Text); return true; }

            NumericUpDown numeric = c as NumericUpDown;
            if (numeric != null)
            {
                a.Add(c.Name + "/" + numeric.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return true;
            }

            RadioButton radio = c as RadioButton;
            if (radio != null) { a.Add(c.Name + "/" + radio.Checked.ToString()); return true; }

            TextBox text = c as TextBox;
            if (text != null) { a.Add(c.Name + "/" + text.Text); return true; }

            TrackBar track = c as TrackBar;
            if (track != null)
            {
                a.Add(c.Name + "/" + track.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return true;
            }

            return false;
        }

        private bool P49RestoreOptionControl(Control c, string val)
        {
            if (!P49IsPersistentOptionControl(c)) return false;

            ColorButton color = c as ColorButton;
            if (color != null)
            {
                string[] colors = val.Split('.');
                int r,g,b,a;
                if (colors.Length == 4 &&
                    Int32.TryParse(colors[0], out r) &&
                    Int32.TryParse(colors[1], out g) &&
                    Int32.TryParse(colors[2], out b) &&
                    Int32.TryParse(colors[3], out a))
                {
                    color.Color = Color.FromArgb(a,r,g,b);
                    return true;
                }
                return false;
            }

            CheckBox check = c as CheckBox;
            if (check != null)
            {
                bool value;
                if (Boolean.TryParse(val,out value)) { check.Checked=value; return true; }
                return false;
            }

            ComboBox combo = c as ComboBox;
            if (combo != null)
            {
                if (combo.Items.Count == 0 || combo.Items[0] is string)
                {
                    combo.Text=val;
                    return true;
                }
                foreach(object item in combo.Items)
                {
                    if (item != null && item.ToString() == val)
                    {
                        combo.Text=val;
                        return true;
                    }
                }
                return false;
            }

            NumericUpDown numeric = c as NumericUpDown;
            if (numeric != null)
            {
                decimal value;
                if (!Decimal.TryParse(val,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out value) &&
                    !Decimal.TryParse(val,out value))
                    return false;
                if (value > numeric.Maximum) value=numeric.Maximum;
                if (value < numeric.Minimum) value=numeric.Minimum;
                numeric.Value=value;
                return true;
            }

            RadioButton radio = c as RadioButton;
            if (radio != null)
            {
                bool value;
                if (Boolean.TryParse(val,out value)) { radio.Checked=value; return true; }
                return false;
            }

            TextBox text = c as TextBox;
            if (text != null) { text.Text=val; return true; }

            TrackBar track = c as TrackBar;
            if (track != null)
            {
                int value;
                if (!Int32.TryParse(val,System.Globalization.NumberStyles.Integer,System.Globalization.CultureInfo.InvariantCulture,out value) &&
                    !Int32.TryParse(val,out value))
                    return false;
                if (value > track.Maximum) value=track.Maximum;
                if (value < track.Minimum) value=track.Minimum;
                track.Value=value;
                return true;
            }

            return false;
        }

'@
$setup=$setup.Replace($helperAnchor,$setupHelpers+$helperAnchor)

$newSaveOptions = @'
        public void SaveOptions()
        {
            if (saving) return;
            saving = true;

            try
            {
                if (CrashProtection && console.chkPower.Checked)
                {
                    textBoxSAVE.Text = "Radio Paused";
                    PON = true;
                    console.chkPower.Checked = false;
                    Thread.Sleep((int)udPFNDelay.Value);
                }

                ArrayList a = new ArrayList();
                ArrayList temp = new ArrayList();
                ControlList(this, ref temp);

                foreach (Control c in temp)
                    P49SerializeOptionControl(c, a);

                // Options is a complete snapshot of named Setup controls.
                // Replace-all removes obsolete/renamed controls instead of letting
                // stale rows influence later restore/default decisions.
                DB.ReplaceVars("Options", ref a);
            }
            finally
            {
                saving = false;
                if (PON)
                {
                    PON = false;
                    console.chkPower.Checked = true;
                    Thread.Sleep(800);
                    textBoxSAVE.Text = "Radio Started";
                }
            }
        }
'@
$setup = Replace-CSharpMethod $setup '        public void SaveOptions()' $newSaveOptions 'Setup.SaveOptions'

$newGetOptions = @'
        public void GetOptions()
        {
            ArrayList temp = new ArrayList();
            ControlList(this, ref temp);

            System.Collections.Generic.Dictionary<string,Control> controls =
                new System.Collections.Generic.Dictionary<string,Control>(StringComparer.Ordinal);

            foreach(Control c in temp)
            {
                if (P49IsPersistentOptionControl(c))
                    controls[c.Name] = c;
            }

            ArrayList a = DB.GetVars("Options");
            System.Collections.Generic.HashSet<string> savedNames =
                new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

            foreach(string s in a)
            {
                if (String.IsNullOrEmpty(s)) continue;
                int slash=s.IndexOf('/');
                if (slash <= 0) continue;
                savedNames.Add(s.Substring(0,slash));
            }

            bool missingCurrentControl = false;
            foreach(string name in controls.Keys)
            {
                if (!savedNames.Contains(name))
                {
                    missingCurrentControl = true;
                    break;
                }
            }

            if (missingCurrentControl)
            {
                InitGeneralTab();
                InitAudioTab();
                InitDSPTab();
                InitDisplayTab();
                InitKeyboardTab();
                InitAppearanceTab();
            }

            foreach(string s in a)
            {
                if (String.IsNullOrEmpty(s)) continue;
                int slash=s.IndexOf('/');
                if (slash <= 0) continue;

                string name=s.Substring(0,slash);
                string val=s.Substring(slash+1);

                Control control;
                if (!controls.TryGetValue(name,out control))
                    continue; // stale/foreign legacy row: ignore without modal warnings

                if (!P49RestoreOptionControl(control,val))
                    Debug.WriteLine("P49 option restore rejected value: " + name + "=" + val);
            }

            foreach(Control c in temp)
            {
                ColorButton color = c as ColorButton;
                if (color != null) color.Automatic = "";
            }
        }
'@
$setup = Replace-CSharpMethod $setup '        public void GetOptions()' $newGetOptions 'Setup.GetOptions'

$newOk = @'
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            SaveOptions();
            DB.Update();
            this.Hide();
        }
'@
$setup = Replace-CSharpMethod $setup '        private void btnOK_Click(object sender, System.EventArgs e)' $newOk 'Setup OK synchronous save'

$newCancel = @'
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            GetOptions();
            this.Hide();
        }
'@
$setup = Replace-CSharpMethod $setup '        private void btnCancel_Click(object sender, System.EventArgs e)' $newCancel 'Setup Cancel synchronous restore'

$newApplyClick = @'
        public void btnApply_Click(object sender, System.EventArgs e)
        {
            textBoxSAVE.Text = " ";
            ApplyOptions();
        }
'@
$setup = Replace-CSharpMethod $setup '        public void btnApply_Click(object sender, System.EventArgs e)' $newApplyClick 'Setup Apply synchronous save'

$newSetupClosing = @'
        private void Setup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveOptions();
            DB.Update();
            this.Hide();
            e.Cancel = true;
        }
'@
$setup = Replace-CSharpMethod $setup '        private void Setup_Closing(object sender, System.ComponentModel.CancelEventArgs e)' $newSetupClosing 'Setup X durable save'

[IO.File]::WriteAllText($setupPath,$setup.Replace($lf,$crlf),$utf8)

# ---------------------------------------------------------------------------
# 3. Common form persistence: same blind spot existed globally. Serialize both
#    TS and standard controls, use replace-all per form, preserve RestoreBounds.
# ---------------------------------------------------------------------------
$common = Normalize ([IO.File]::ReadAllText($commonPath))

$newCommonControlList = @'
        public static void ControlList(Control c, ref ArrayList a)
        {
            if (c.Controls.Count > 0)
            {
                foreach (Control c2 in c.Controls)
                    ControlList(c2, ref a);
            }

            if (c is CheckBox ||
                c is ComboBox ||
                c is NumericUpDown ||
                c is RadioButton ||
                c is TextBox ||
                c is TrackBar ||
                c is ColorButton)
                a.Add(c);
        }
'@
$common = Replace-CSharpMethod $common '        public static void ControlList(Control c, ref ArrayList a)' $newCommonControlList 'Common.ControlList'

$commonAnchor='        public static void SaveForm(Form form, string tablename)'
if(!$common.Contains($commonAnchor)){ throw 'P49 Common.SaveForm anchor missing' }
$commonHelpers = @'
        private static bool P49SerializeFormControl(Control c, ArrayList a)
        {
            if (c == null || String.IsNullOrEmpty(c.Name)) return false;

            ColorButton color = c as ColorButton;
            if (color != null)
            {
                Color clr=color.Color;
                a.Add(c.Name + "/" + clr.R + "." + clr.G + "." + clr.B + "." + clr.A);
                return true;
            }

            CheckBox check=c as CheckBox;
            if(check != null){ a.Add(c.Name + "/" + check.Checked.ToString()); return true; }

            ComboBox combo=c as ComboBox;
            if(combo != null){ a.Add(c.Name + "/" + combo.Text); return true; }

            NumericUpDown numeric=c as NumericUpDown;
            if(numeric != null){
                a.Add(c.Name + "/" + numeric.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return true;
            }

            RadioButton radio=c as RadioButton;
            if(radio != null){ a.Add(c.Name + "/" + radio.Checked.ToString()); return true; }

            TextBox text=c as TextBox;
            if(text != null){ a.Add(c.Name + "/" + text.Text); return true; }

            TrackBar track=c as TrackBar;
            if(track != null){
                a.Add(c.Name + "/" + track.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return true;
            }

            return false;
        }

        private static bool P49RestoreFormControl(Control c, string val)
        {
            if(c == null) return false;

            ColorButton color=c as ColorButton;
            if(color != null)
            {
                string[] parts=val.Split('.');
                int r,g,b,a;
                if(parts.Length==4 &&
                   Int32.TryParse(parts[0],out r) &&
                   Int32.TryParse(parts[1],out g) &&
                   Int32.TryParse(parts[2],out b) &&
                   Int32.TryParse(parts[3],out a))
                {
                    color.Color=Color.FromArgb(a,r,g,b);
                    return true;
                }
                return false;
            }

            CheckBox check=c as CheckBox;
            if(check != null){ bool v; if(Boolean.TryParse(val,out v)){ check.Checked=v; return true; } return false; }

            ComboBox combo=c as ComboBox;
            if(combo != null){ combo.Text=val; return true; }

            NumericUpDown numeric=c as NumericUpDown;
            if(numeric != null)
            {
                decimal v;
                if(!Decimal.TryParse(val,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out v) &&
                   !Decimal.TryParse(val,out v)) return false;
                if(v>numeric.Maximum)v=numeric.Maximum;
                if(v<numeric.Minimum)v=numeric.Minimum;
                numeric.Value=v;
                return true;
            }

            RadioButton radio=c as RadioButton;
            if(radio != null){ bool v; if(Boolean.TryParse(val,out v)){ radio.Checked=v; return true; } return false; }

            TextBox text=c as TextBox;
            if(text != null){ text.Text=val; return true; }

            TrackBar track=c as TrackBar;
            if(track != null)
            {
                int v;
                if(!Int32.TryParse(val,System.Globalization.NumberStyles.Integer,System.Globalization.CultureInfo.InvariantCulture,out v) &&
                   !Int32.TryParse(val,out v)) return false;
                if(v>track.Maximum)v=track.Maximum;
                if(v<track.Minimum)v=track.Minimum;
                track.Value=v;
                return true;
            }

            return false;
        }

'@
$common=$common.Replace($commonAnchor,$commonHelpers+$commonAnchor)

$newSaveForm = @'
        public static void SaveForm(Form form, string tablename)
        {
            ArrayList a = new ArrayList();
            ArrayList temp = new ArrayList();
            ControlList(form, ref temp);

            foreach(Control c in temp)
                P49SerializeFormControl(c,a);

            Rectangle bounds = form.WindowState == FormWindowState.Normal
                ? form.Bounds
                : form.RestoreBounds;

            a.Add("Top/" + bounds.Top.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("Left/" + bounds.Left.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("Width/" + bounds.Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("Height/" + bounds.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
            a.Add("WindowState/" + (form.WindowState == FormWindowState.Maximized ? "Maximized" : "Normal"));

            DB.ReplaceVars(tablename, ref a);
        }
'@
$common = Replace-CSharpMethod $common '        public static void SaveForm(Form form, string tablename)' $newSaveForm 'Common.SaveForm complete snapshot'

$newRestoreForm = @'
        public static void RestoreForm(Form form, string tablename, bool restore_size)
        {
            ArrayList temp = new ArrayList();
            ControlList(form, ref temp);

            System.Collections.Generic.Dictionary<string,Control> controls =
                new System.Collections.Generic.Dictionary<string,Control>(StringComparer.Ordinal);

            foreach(Control c in temp)
            {
                if(c != null && !String.IsNullOrEmpty(c.Name))
                    controls[c.Name]=c;
            }

            ArrayList a=DB.GetVars(tablename);
            string windowState="Normal";

            foreach(string s in a)
            {
                if(String.IsNullOrEmpty(s)) continue;
                int slash=s.IndexOf('/');
                if(slash <= 0) continue;

                string name=s.Substring(0,slash);
                string val=s.Substring(slash+1);
                int n;

                if(name=="Top")
                {
                    if(Int32.TryParse(val,out n)){ form.StartPosition=FormStartPosition.Manual; form.Top=n; }
                    continue;
                }
                if(name=="Left")
                {
                    if(Int32.TryParse(val,out n)){ form.StartPosition=FormStartPosition.Manual; form.Left=n; }
                    continue;
                }
                if(name=="Width")
                {
                    if(restore_size && Int32.TryParse(val,out n) && n>0) form.Width=n;
                    continue;
                }
                if(name=="Height")
                {
                    if(restore_size && Int32.TryParse(val,out n) && n>0) form.Height=n;
                    continue;
                }
                if(name=="WindowState")
                {
                    windowState=val;
                    continue;
                }

                Control control;
                if(controls.TryGetValue(name,out control))
                {
                    if(!P49RestoreFormControl(control,val))
                        Debug.WriteLine("P49 form restore rejected value: " + tablename + "." + name + "=" + val);
                }
            }

            ForceFormOnScreen(form);
            if(restore_size && String.Equals(windowState,"Maximized",StringComparison.OrdinalIgnoreCase))
                form.WindowState=FormWindowState.Maximized;
        }
'@
$common = Replace-CSharpMethod $common '        public static void RestoreForm(Form form, string tablename, bool restore_size)' $newRestoreForm 'Common.RestoreForm generalized'

[IO.File]::WriteAllText($commonPath,$common.Replace($lf,$crlf),$utf8)

# ---------------------------------------------------------------------------
# 4. Correct known form-table defects discovered by the full persistence audit.
# ---------------------------------------------------------------------------
$spotWatch=Normalize ([IO.File]::ReadAllText($spotWatchPath))
if(!$spotWatch.Contains('Common.SaveForm(this, " SpotWatchBox");')){ throw 'P49 SpotWatchBox typo anchor missing' }
$spotWatch=$spotWatch.Replace('Common.SaveForm(this, " SpotWatchBox");','Common.SaveForm(this, "SpotWatchBox");')
[IO.File]::WriteAllText($spotWatchPath,$spotWatch.Replace($lf,$crlf),$utf8)

$wave=Normalize ([IO.File]::ReadAllText($wavePath))
$waveRestore='            Common.RestoreForm(this, "WaveOptions", false);'
$waveRestoreNew=@'
            // P49: Wave and WaveOptions were incorrectly sharing one DB table.
            if (DB.GetVars("WaveForm").Count > 0)
                Common.RestoreForm(this, "WaveForm", false);
            else
                Common.RestoreForm(this, "WaveOptions", false); // one-time migration fallback
'@
if(!$wave.Contains($waveRestore)){ throw 'P49 wave restore collision anchor missing' }
$wave=$wave.Replace($waveRestore,(Normalize $waveRestoreNew))
if(!$wave.Contains('Common.SaveForm(this, "WaveOptions");')){ throw 'P49 wave save collision anchor missing' }
$wave=$wave.Replace('Common.SaveForm(this, "WaveOptions");','Common.SaveForm(this, "WaveForm");')
[IO.File]::WriteAllText($wavePath,$wave.Replace($lf,$crlf),$utf8)

$spotDecoder=Normalize ([IO.File]::ReadAllText($spotDecoderPath))
$spotRestore='            Common.RestoreForm(this, "SpotOptions", false);'
$spotRestoreNew=@'
            // P49: SpotDecoder and SpotOptions were incorrectly sharing one DB table.
            if (DB.GetVars("SpotDecoder").Count > 0)
                Common.RestoreForm(this, "SpotDecoder", false);
            else
                Common.RestoreForm(this, "SpotOptions", false); // one-time migration fallback
'@
if(!$spotDecoder.Contains($spotRestore)){ throw 'P49 spot decoder restore collision anchor missing' }
$spotDecoder=$spotDecoder.Replace($spotRestore,(Normalize $spotRestoreNew))
if(!$spotDecoder.Contains('Common.SaveForm(this, "SpotOptions");')){ throw 'P49 spot decoder save collision anchor missing' }
$spotDecoder=$spotDecoder.Replace('Common.SaveForm(this, "SpotOptions");','Common.SaveForm(this, "SpotDecoder");')
[IO.File]::WriteAllText($spotDecoderPath,$spotDecoder.Replace($lf,$crlf),$utf8)

# Preselector had SaveForm but no RestoreForm.
$preSel=Normalize ([IO.File]::ReadAllText($preSelPath))
$preAnchor=@'
            InitializeComponent();
            console = c;
            mox = c.MOX;
            UpdatePreSel();
'@
$preNew=@'
            InitializeComponent();
            console = c;
            mox = c.MOX;
            Common.RestoreForm(this, "PreSelForm", false);
            UpdatePreSel();
'@
$preSel=Replace-ExactOnce $preSel $preAnchor $preNew 'PreSelForm restore counterpart'
[IO.File]::WriteAllText($preSelPath,$preSel.Replace($lf,$crlf),$utf8)

# FilterForm had RestoreForm but no SaveForm.
$filter=Normalize ([IO.File]::ReadAllText($filterPath))
$filterAnchor='            Common.RestoreForm(this, "FilterForm", false);'
$filterNew=@'
            Common.RestoreForm(this, "FilterForm", false);
            this.FormClosing += new FormClosingEventHandler(FilterForm_FormClosing);
'@
if(!$filter.Contains($filterAnchor)){ throw 'P49 FilterForm restore anchor missing' }
$filter=$filter.Replace($filterAnchor,(Normalize $filterNew))
$classEnd=$filter.LastIndexOf($lf+'    }'+$lf+'}',[StringComparison]::Ordinal)
if($classEnd -lt 0){ throw 'P49 FilterForm class end anchor missing' }
$filterMethod=@'

        private void FilterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Common.SaveForm(this, "FilterForm");
        }
'@
$filter=$filter.Substring(0,$classEnd)+(Normalize $filterMethod)+$filter.Substring($classEnd)
[IO.File]::WriteAllText($filterPath,$filter.Replace($lf,$crlf),$utf8)

# ---------------------------------------------------------------------------
# 5. Dedicated extension tables: replace-all + immediate durable commit.
# ---------------------------------------------------------------------------
$legacy=Normalize ([IO.File]::ReadAllText($legacyPath))
$legacyOld='            DB.SaveVars("SQ4KOU_LegacyItems", ref a);'
$legacyNew=@'
            DB.ReplaceVars("SQ4KOU_LegacyItems", ref a);
            DB.Update();
'@
if(!$legacy.Contains($legacyOld)){ throw 'P49 LegacyItems save anchor missing' }
$legacy=$legacy.Replace($legacyOld,(Normalize $legacyNew))
[IO.File]::WriteAllText($legacyPath,$legacy.Replace($lf,$crlf),$utf8)

$tci=Normalize ([IO.File]::ReadAllText($tciPath))
$tciOld='            DB.SaveVars("SQ4KOU_TCI", ref a);'
$tciNew=@'
            DB.ReplaceVars("SQ4KOU_TCI", ref a);
            DB.Update();
'@
if(!$tci.Contains($tciOld)){ throw 'P49 TCI save anchor missing' }
$tci=$tci.Replace($tciOld,(Normalize $tciNew))
[IO.File]::WriteAllText($tciPath,$tci.Replace($lf,$crlf),$utf8)

# ---------------------------------------------------------------------------
# 6. Hard source gates: the build is rejected if any audited persistence defect
#    remains in generated source.
# ---------------------------------------------------------------------------
$verifyDb=[IO.File]::ReadAllText($dbPath)
$verifySetup=[IO.File]::ReadAllText($setupPath)
$verifyCommon=[IO.File]::ReadAllText($commonPath)
$verifyWave=[IO.File]::ReadAllText($wavePath)
$verifySpot=[IO.File]::ReadAllText($spotDecoderPath)
$verifyWatch=[IO.File]::ReadAllText($spotWatchPath)
$verifyPre=[IO.File]::ReadAllText($preSelPath)
$verifyFilter=[IO.File]::ReadAllText($filterPath)
$verifyLegacy=[IO.File]::ReadAllText($legacyPath)
$verifyTCI=[IO.File]::ReadAllText($tciPath)

foreach($token in @(
    'lock (db_io_lock)',
    'public static void SaveVars(string tableName, ref ArrayList list)',
    'public static ArrayList GetVars(string tableName)',
    'public static void ReplaceVars(string tableName, ref ArrayList list)',
    'public static bool ImportDatabase(string filename)',
    'public static void Exit()',
    'P49ValidateLogicalDatabase',
    'last_logical_repair_summary',
    'Logical database validation failed before write'
)){
    if(!$verifyDb.Contains($token)){ throw "P49 DB gate missing: $token" }
}

foreach($token in @(
    'P49SerializeOptionControl',
    'P49RestoreOptionControl',
    'DB.ReplaceVars("Options", ref a);',
    'SaveOptions();',
    'DB.Update();'
)){
    if(!$verifySetup.Contains($token)){ throw "P49 Setup gate missing: $token" }
}
if($verifySetup.Contains('new Thread(new ThreadStart(SaveOptions))') -or
   $verifySetup.Contains('new Thread(new ThreadStart(GetOptions))') -or
   $verifySetup.Contains('new Thread(new ThreadStart(ApplyOptions))')){
    throw 'P49 async Setup persistence thread gate failed'
}

foreach($token in @(
    'c is CheckBox',
    'c is ComboBox',
    'c is NumericUpDown',
    'c is RadioButton',
    'c is TextBox',
    'c is TrackBar',
    'DB.ReplaceVars(tablename, ref a);',
    'form.RestoreBounds'
)){
    if(!$verifyCommon.Contains($token)){ throw "P49 Common gate missing: $token" }
}

if($verifyWatch.Contains('" SpotWatchBox"')){ throw 'P49 leading-space SpotWatchBox table still present' }
if(!$verifyWave.Contains('Common.SaveForm(this, "WaveForm");')){ throw 'P49 WaveForm separation gate missing' }
if(!$verifySpot.Contains('Common.SaveForm(this, "SpotDecoder");')){ throw 'P49 SpotDecoder separation gate missing' }
if(!$verifyPre.Contains('Common.RestoreForm(this, "PreSelForm", false);')){ throw 'P49 PreSel restore gate missing' }
if(!$verifyFilter.Contains('Common.SaveForm(this, "FilterForm");')){ throw 'P49 Filter save gate missing' }
if(!$verifyLegacy.Contains('DB.ReplaceVars("SQ4KOU_LegacyItems", ref a);') -or !$verifyLegacy.Contains('DB.Update();')){
    throw 'P49 LegacyItems durability gate missing'
}
if(!$verifyTCI.Contains('DB.ReplaceVars("SQ4KOU_TCI", ref a);') -or !$verifyTCI.Contains('DB.Update();')){
    throw 'P49 TCI durability gate missing'
}

Write-Host 'P49_DATABASE_PERSISTENCE=FULL_AUDIT_HARDENED'
Write-Host 'P49_DB_DATASET_ACCESS=SYNCHRONIZED_SAVE_GET_REPLACE_IMPORT_EXIT'
Write-Host 'P49_DB_DISK_WRITE=P03_ATOMIC_WRITE_THROUGH_RETAINED'
Write-Host 'P49_DB_LOGICAL_VALIDATION=CORE_SCHEMA_PLUS_KEYVALUE_INTEGRITY'
Write-Host 'P49_DB_DUPLICATE_KEYS=AUTO_DEDUPE_KEEP_LAST'
Write-Host 'P49_DB_INVALID_XML_OR_SCHEMA=BACKUP_RECOVERY'
Write-Host 'P49_DB_READABLE_BUT_LOGICALLY_DAMAGED=DETECT_REPAIR_PRESERVE_ORIGINAL'
Write-Host 'P49_SETUP_SAVE=UI_THREAD_SYNCHRONOUS'
Write-Host 'P49_SETUP_OPTIONS=TS_PLUS_STANDARD_WINFORMS_REPLACE_ALL'
Write-Host 'P49_SETUP_OK_APPLY_X=DURABLE_DB_UPDATE'
Write-Host 'P49_COMMON_FORMS=TS_PLUS_STANDARD_WINFORMS_REPLACE_ALL'
Write-Host 'P49_COMMON_GEOMETRY=RESTOREBOUNDS_SAFE'
Write-Host 'P49_TABLE_COLLISIONS=WAVE_SPOTDECODER_SEPARATED'
Write-Host 'P49_TABLE_TYPO=SPOTWATCHBOX_FIXED'
Write-Host 'P49_FORM_PAIRS=PRESEL_RESTORE_FILTER_SAVE_ADDED'
Write-Host 'P49_EXTENSION_TABLES=TCI_LEGACYITEMS_DURABLE'
Write-Host 'P49_METERS=P48_DURABLE_RETAINED'
Write-Host 'P49_RX2=NOT_EXPOSED'
