[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-DB] $s" }

$databaseCs = Join-Path $SourceRoot 'Console\database.cs'
if(!(Test-Path -LiteralPath $databaseCs)) { throw "PowerSDR database source missing: $databaseCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$text = [IO.File]::ReadAllText($databaseCs).Replace("`r`n", "`n")

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "DB anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "DB anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

function Replace-CSharpMethod([string]$Text, [string]$Signature, [string]$Replacement, [string]$Label) {
    $Replacement = $Replacement.Replace("`r`n", "`n")
    $start = $Text.IndexOf($Signature, [StringComparison]::Ordinal)
    if($start -lt 0) { throw "DB method signature missing: $Label" }
    if($Text.IndexOf($Signature, $start + $Signature.Length, [StringComparison]::Ordinal) -ge 0) {
        throw "DB method signature not unique: $Label"
    }

    $brace = $Text.IndexOf('{', $start)
    if($brace -lt 0) { throw "DB method opening brace missing: $Label" }
    $depth = 0
    $end = -1
    for($i=$brace; $i -lt $Text.Length; $i++) {
        $c = $Text[$i]
        if($c -eq '{') { $depth++ }
        elseif($c -eq '}') {
            $depth--
            if($depth -eq 0) {
                $end = $i + 1
                break
            }
        }
    }
    if($end -lt 0) { throw "DB method closing brace missing: $Label" }
    return $Text.Substring(0,$start) + $Replacement + $Text.Substring($end)
}

$oldAnchor = @'
        public static string FileName1
        {
            set { file_name1 = value; }
        }
'@

$helperBlock = @'
        public static string FileName1
        {
            set { file_name1 = value; }
        }

        // SQ4KOU DB reliability layer.
        // Keeps the native KE9NS DataSet schema/semantics, but makes disk I/O
        // transactional and recovery non-destructive.
        private static readonly object db_io_lock = new object();

        private static string DatabaseSiblingPath(string suffix)
        {
            string directory = Path.GetDirectoryName(file_name);
            string stem = Path.GetFileNameWithoutExtension(file_name);
            string extension = Path.GetExtension(file_name);
            if (String.IsNullOrEmpty(extension)) extension = ".xml";
            string name = stem + suffix + extension;
            return String.IsNullOrEmpty(directory) ? name : Path.Combine(directory, name);
        }

        private static bool TryLoadDatabaseFile(string path, out DataSet loaded, out string error)
        {
            loaded = null;
            error = "";

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

                if (candidate.Tables.Count == 0)
                    throw new InvalidDataException("Database contains no tables.");

                loaded = candidate;
                return true;
            }
            catch (Exception ex)
            {
                error = path + ": " + ex.Message;
                return false;
            }
        }

        private static bool TryLoadFirstValidBackup(out DataSet loaded, out string source, out string errors)
        {
            loaded = null;
            source = "";
            errors = "";

            string[] candidates = new string[]
            {
                DatabaseSiblingPath("_sbu"),
                DatabaseSiblingPath("_bak1"),
                DatabaseSiblingPath("_bak2"),
                DatabaseSiblingPath("_bak3")
            };

            foreach (string candidate in candidates)
            {
                if (!File.Exists(candidate)) continue;

                DataSet recovered;
                string error;
                if (TryLoadDatabaseFile(candidate, out recovered, out error))
                {
                    loaded = recovered;
                    source = candidate;
                    return true;
                }

                if (errors.Length > 0) errors += Environment.NewLine;
                errors += error;
            }

            return false;
        }

        private static void PreserveCorruptDatabase(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path)) return;

            try
            {
                string directory = Path.GetDirectoryName(path);
                string stem = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);
                string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                string name = stem + "_corrupt_" + stamp + extension;
                string target = String.IsNullOrEmpty(directory) ? name : Path.Combine(directory, name);

                int copy = 1;
                while (File.Exists(target))
                {
                    name = stem + "_corrupt_" + stamp + "_" + copy.ToString() + extension;
                    target = String.IsNullOrEmpty(directory) ? name : Path.Combine(directory, name);
                    copy++;
                }

                File.Copy(path, target, false);
                Debug.WriteLine("Preserved corrupt PowerSDR database: " + target);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not preserve corrupt PowerSDR database: " + ex.Message);
            }
        }

        private static void AtomicWriteDataSet(DataSet source, string destination)
        {
            if (source == null) throw new InvalidOperationException("Database DataSet is null.");
            if (String.IsNullOrEmpty(destination)) throw new InvalidOperationException("Database filename is empty.");

            string directory = Path.GetDirectoryName(destination);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string temp = destination + ".tmp." + Process.GetCurrentProcess().Id.ToString();
            try
            {
                if (File.Exists(temp)) File.Delete(temp);

                using (FileStream stream = new FileStream(
                    temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, FileOptions.WriteThrough))
                {
                    source.WriteXml(stream, XmlWriteMode.WriteSchema);
                    stream.Flush(true);
                }

                DataSet verified;
                string verifyError;
                if (!TryLoadDatabaseFile(temp, out verified, out verifyError))
                    throw new InvalidDataException("Database write verification failed. " + verifyError);

                if (File.Exists(destination))
                    File.Replace(temp, destination, null);
                else
                    File.Move(temp, destination);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    try { File.Delete(temp); }
                    catch { }
                }
            }
        }

        private static void RotateValidBackup(string source, string destination)
        {
            if (!File.Exists(source)) return;

            DataSet candidate;
            string error;
            if (!TryLoadDatabaseFile(source, out candidate, out error))
            {
                Debug.WriteLine("Skipping invalid database backup during rotation: " + error);
                return;
            }

            AtomicWriteDataSet(candidate, destination);
        }

        private static void CreateValidatedSessionBackup()
        {
            string sbu = DatabaseSiblingPath("_sbu");
            string bak1 = DatabaseSiblingPath("_bak1");
            string bak2 = DatabaseSiblingPath("_bak2");
            string bak3 = DatabaseSiblingPath("_bak3");

            RotateValidBackup(bak2, bak3);
            RotateValidBackup(bak1, bak2);
            RotateValidBackup(sbu, bak1);
            AtomicWriteDataSet(ds, sbu);
        }
'@

$text = Replace-ExactOnce $text $oldAnchor $helperBlock 'DB helper insertion'

$newInit = @'
        public static bool Init(Model model) // ke9ns first sets up a default FRSRegion.US, then in console.cs changes it
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
                    ds = loaded;
                    database_exists = true;
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
                            "The active database was unreadable and has been preserved for diagnosis.\n\n" +
                            "PowerSDR recovered the most recent VALID backup:\n" + recoverySource,
                            "Database Recovered",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            "The active database is unreadable and no valid backup could be loaded.\n\n" +
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
                        "The legacy PowerSDR database could not be imported.\n\n" + legacyError +
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

$newUpdate = @'
        public static void Update()  // ke9ns write database file
        {
            if (ds == null || String.IsNullOrEmpty(file_name)) return;

            try
            {
                lock (db_io_lock)
                {
                    AtomicWriteDataSet(ds, file_name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "A database write operation failed. The previous database file was not intentionally deleted.\n\n" +
                    "The exception error was:\n\n" + ex.Message,
                    "ERROR: Database Write Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
'@

$text = Replace-CSharpMethod $text '        public static bool Init(Model model)' $newInit 'Init'
$text = Replace-CSharpMethod $text '        public static void Update()' $newUpdate 'Update'

$checks = @(
    @{ Name='atomic writer'; Ok=$text.Contains('private static void AtomicWriteDataSet') },
    @{ Name='write-through'; Ok=$text.Contains('FileOptions.WriteThrough') },
    @{ Name='round-trip verification'; Ok=$text.Contains('Database write verification failed') },
    @{ Name='atomic replace'; Ok=$text.Contains('File.Replace(temp, destination, null)') },
    @{ Name='backup fallback chain'; Ok=$text.Contains('TryLoadFirstValidBackup') -and $text.Contains('DatabaseSiblingPath("_bak3")') },
    @{ Name='corrupt preservation'; Ok=$text.Contains('PreserveCorruptDatabase(file_name)') },
    @{ Name='legacy source preserved'; Ok=$text.Contains('The original file was left unchanged') },
    @{ Name='direct primary WriteXml removed'; Ok=(!$text.Contains('ds.WriteXml(file_name, XmlWriteMode.WriteSchema)')) },
    @{ Name='single-backup recovery removed'; Ok=(!$text.Contains('ds.ReadXml(recovery_db)')) }
)

$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('DB reliability post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($databaseCs, $text.Replace("`n", "`r`n"), $utf8)
Stage 'PASS: atomic writes, verified backup chain, non-destructive corruption recovery'
