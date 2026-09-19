[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P09-WINFORMS] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
$spotCs = Join-Path $SourceRoot 'Console\spot.cs'
foreach($p in @($consoleCs,$spotCs)) {
    if(!(Test-Path -LiteralPath $p)) { throw "P09 source missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P09 anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P09 anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

$console = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")
$spot = [IO.File]::ReadAllText($spotCs).Replace("`r`n", "`n")

# ---------------------------------------------------------------------------
# Startup timing: objective evidence for the remaining cold-start cost.
# ---------------------------------------------------------------------------
$console = Replace-ExactOnce $console @'
        public Console(string[] args)                  // ke9ns: Main() called first
        {

            Debug.WriteLine("===START=== Console here");
'@ @'
        public Console(string[] args)                  // ke9ns: Main() called first
        {
            Stopwatch sq4kouStartupTotal = Stopwatch.StartNew();
            long sq4kouInitComponentsMs = 0;
            long sq4kouInitConsoleMs = 0;

            Debug.WriteLine("===START=== Console here");
'@ 'startup stopwatch'

# Enable optimized buffering immediately after the generated UI exists, rather
# than only after all initialization has already completed.
$console = Replace-ExactOnce $console @'
            InitializeComponent();                              // Windows Forms Generated Code

            //  SetProcessShutdownParameters
'@ @'
            Stopwatch sq4kouInitComponentsTimer = Stopwatch.StartNew();
            InitializeComponent();                              // Windows Forms Generated Code
            sq4kouInitComponentsTimer.Stop();
            sq4kouInitComponentsMs = sq4kouInitComponentsTimer.ElapsedMilliseconds;

            // SQ4KOU P09: enable the existing WinForms double-buffer policy before
            // the large startup mutation phase, not after it.
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            //  SetProcessShutdownParameters
'@ 'early optimized double buffer'

# The late duplicate buffering block is no longer needed.
$console = Replace-ExactOnce $console @'
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            Splash.CloseForm();
'@ @'
            // SQ4KOU P09: buffering was enabled immediately after InitializeComponent.

            Splash.CloseForm();
'@ 'remove late buffer activation'

# Time InitConsole and preserve all radio/DSP initialization.
$console = Replace-ExactOnce $console @'
            InitConsole();										// Initialize all forms and main variables            

            Splash.SetStatus("Finished");
'@ @'
            Stopwatch sq4kouInitConsoleTimer = Stopwatch.StartNew();
            InitConsole();										// Initialize all forms and main variables
            sq4kouInitConsoleTimer.Stop();
            sq4kouInitConsoleMs = sq4kouInitConsoleTimer.ElapsedMilliseconds;

            Splash.SetStatus("Finished");
'@ 'InitConsole timing'

# Replace visible warm-up of small auxiliary forms by handle creation only.
$console = Replace-ExactOnce $console @'
            IDBOXForm.Show();
            IDBOXForm.Close();
'@ @'
            // SQ4KOU P09: prime WinForms handle without displaying/painting the form.
            IntPtr sq4kouIDBoxHandle = IDBOXForm.Handle;
'@ 'IDBOX hidden prime'

$console = Replace-ExactOnce $console @'
            TOTBOXForm.Show();
            TOTBOXForm.Close();
'@ @'
            // SQ4KOU P09: prime WinForms handle without displaying/painting the form.
            IntPtr sq4kouTOTBoxHandle = TOTBOXForm.Handle;
'@ 'TOTBOX hidden prime'

$console = Replace-ExactOnce $console @'
            SpotWatchBoxForm.Show();
            SpotWatchBoxForm.Close();
'@ @'
            // SQ4KOU P09: prime WinForms handle without displaying/painting the form.
            IntPtr sq4kouSpotWatchBoxHandle = SpotWatchBoxForm.Handle;
'@ 'SpotWatchBox hidden prime'

# SpotForm.Show() existed mostly to fire Load before immediately hiding the form.
# Run the same initialization directly and create the handle without showing it.
$console = Replace-ExactOnce $console @'
            SpotForm.Show();
            SpotForm.Hide();
'@ @'
            // SQ4KOU P09: preserve SpotControl Load semantics without creating a
            // visible top-level window during application startup.
            IntPtr sq4kouSpotFormHandle = SpotForm.Handle;
            SpotForm.SpotControl_Load(SpotForm, EventArgs.Empty);
'@ 'SpotForm hidden prime'

# Write a tiny startup evidence file after initialization is complete.
$console = Replace-ExactOnce $console @'
            Splash.SetStatus("Finished");                       // Set progress point
                                                                // Activates double buffering
'@ @'
            Splash.SetStatus("Finished");                       // Set progress point

            sq4kouStartupTotal.Stop();
            try
            {
                File.WriteAllText(
                    app_data_path + "startup_p09.log",
                    "SQ4KOU_STARTUP_INITCOMPONENTS_MS=" + sq4kouInitComponentsMs + Environment.NewLine +
                    "SQ4KOU_STARTUP_INITCONSOLE_MS=" + sq4kouInitConsoleMs + Environment.NewLine +
                    "SQ4KOU_STARTUP_TO_FINISHED_MS=" + sq4kouStartupTotal.ElapsedMilliseconds + Environment.NewLine);
            }
            catch { }
                                                                // Activates double buffering
'@ 'startup evidence log'

# ---------------------------------------------------------------------------
# SpotControl internally showed/hid SpotAge and SpotWatch only to pre-create
# their native windows. Create the handles directly instead.
# ---------------------------------------------------------------------------
$spot = Replace-ExactOnce $spot @'
            SpotAge.Show();
            SpotAge.Hide();
'@ @'
            // SQ4KOU P09: create handle without top-level Show/Hide repaint.
            IntPtr sq4kouSpotAgeHandle = SpotAge.Handle;
'@ 'SpotAge hidden prime'

$spot = Replace-ExactOnce $spot @'
            SpotWatch.Show();
            SpotWatch.Hide();
'@ @'
            // SQ4KOU P09: create handle without top-level Show/Hide repaint.
            IntPtr sq4kouSpotWatchHandle = SpotWatch.Handle;
'@ 'SpotWatch hidden prime'

# SpotControl_Load is now invoked once explicitly during startup. Guard it so
# the first later user-visible Show does not repeat the heavy initialization.
$spot = Replace-ExactOnce $spot @'
        public void SpotControl_Load(object sender, EventArgs e)
        {
            Debug.WriteLine("SpotControl_Load here");
'@ @'
        private bool sq4kouSpotControlLoadDone = false;

        public void SpotControl_Load(object sender, EventArgs e)
        {
            if (sq4kouSpotControlLoadDone) return;
            sq4kouSpotControlLoadDone = true;

            Debug.WriteLine("SpotControl_Load here");
'@ 'SpotControl load-once guard'

# Post-checks.
$checks = @(
    @{Name='early buffering'; Ok=$console.Contains('SQ4KOU P09: enable the existing WinForms double-buffer policy')},
    @{Name='no SpotForm show-hide'; Ok=(!$console.Contains('SpotForm.Show();') -and !$console.Contains('SpotForm.Hide();'))},
    @{Name='no ID/TOT warm show'; Ok=(!$console.Contains('IDBOXForm.Show();') -and !$console.Contains('TOTBOXForm.Show();'))},
    @{Name='spot child no show-hide'; Ok=(!$spot.Contains('SpotAge.Show();') -and !$spot.Contains('SpotWatch.Show();'))},
    @{Name='spot load guard'; Ok=$spot.Contains('sq4kouSpotControlLoadDone')},
    @{Name='startup timing'; Ok=$console.Contains('SQ4KOU_STARTUP_TO_FINISHED_MS=')}
)
$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P09 post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs, $console.Replace("`n", "`r`n"), $utf8)
[IO.File]::WriteAllText($spotCs, $spot.Replace("`n", "`r`n"), $utf8)

Stage 'PASS: hidden form priming, early double buffering, Spot load-once, startup timing'
