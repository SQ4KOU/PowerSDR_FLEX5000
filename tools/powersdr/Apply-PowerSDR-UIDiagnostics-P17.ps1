[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P17-UI-DIAG] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
$projectCs = Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$helperSrc = Join-Path $PSScriptRoot 'SQ4KOUUIDiagnostics.cs'
$helperDst = Join-Path $SourceRoot 'Console\SQ4KOUUIDiagnostics.cs'

foreach($p in @($consoleCs,$projectCs,$helperSrc)) {
    if(!(Test-Path -LiteralPath $p)) { throw "P17 input missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13) + ([string][char]10)
$lf = [string][char]10

Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force

$console = [IO.File]::ReadAllText($consoleCs).Replace($crlf,$lf)
$project = [IO.File]::ReadAllText($projectCs).Replace($crlf,$lf)

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label) {
    $Old = $Old.Replace($crlf,$lf)
    $New = $New.Replace($crlf,$lf)
    $first = $Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0) { throw "P17 anchor missing: $Label" }
    $second = $Text.IndexOf($Old,$first + $Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0) { throw "P17 anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

# Register diagnostics helper in the legacy non-SDK project.
if($project -notmatch 'Compile Include="SQ4KOUUIDiagnostics\.cs"') {
    $rx = [regex]'(?ms)(<Compile Include="console\.cs">\s*<SubType>Form</SubType>\s*</Compile>)'
    $matches = $rx.Matches($project)
    if($matches.Count -ne 1) { throw "P17 csproj console.cs anchor count=$($matches.Count)" }
    $project = $rx.Replace($project, '$1' + $lf + '    <Compile Include="SQ4KOUUIDiagnostics.cs" />', 1)
}

# Start the logger before the Console constructor so constructor time is visible.
$console = Replace-ExactOnce $console @'
            Debug.WriteLine("===START=== Main here");

            string app_data_path = "";
'@ @'
            Debug.WriteLine("===START=== Main here");
            SQ4KOUUIDiagnostics.StartEarly();
            SQ4KOUUIDiagnostics.Mark("STARTUP", "MAIN_BEGIN", null);

            string app_data_path = "";
'@ 'Main diagnostics start'

$console = Replace-ExactOnce $console @'
                Debug.WriteLine("===Start=== create Console() here "); // ke9ns: 

                theConsole = new Console(args);  // ke9ns: run Console here

                Debug.WriteLine("console1"); // ke9ns: 

                Application.Run(theConsole);
'@ @'
                Debug.WriteLine("===Start=== create Console() here "); // ke9ns: 
                long sq4kouConsoleCtorStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "CONSOLE_CTOR", null);

                theConsole = new Console(args);  // ke9ns: run Console here
                SQ4KOUUIDiagnostics.OperationEnd(sq4kouConsoleCtorStart, "STARTUP", "CONSOLE_CTOR", null);

                Debug.WriteLine("console1"); // ke9ns: 
                SQ4KOUUIDiagnostics.Mark("STARTUP", "APPLICATION_RUN_ENTER", null);

                Application.Run(theConsole);
'@ 'Main Console/Application.Run timing'

# Constructor begin.
$console = Replace-ExactOnce $console @'
        public Console(string[] args)                  // ke9ns: Main() called first
        {

            Debug.WriteLine("===START=== Console here");
'@ @'
        public Console(string[] args)                  // ke9ns: Main() called first
        {

            SQ4KOUUIDiagnostics.Mark("STARTUP", "CONSOLE_CTOR_BODY_BEGIN", null);
            Debug.WriteLine("===START=== Console here");
'@ 'Console constructor begin'

# InitializeComponent is a major synchronous WinForms construction phase.
$console = Replace-ExactOnce $console @'
            InitializeComponent();                              // Windows Forms Generated Code

            //  SetProcessShutdownParameters
'@ @'
            long sq4kouInitComponentsStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "INITIALIZE_COMPONENTS", null);
            InitializeComponent();                              // Windows Forms Generated Code
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouInitComponentsStart, "STARTUP", "INITIALIZE_COMPONENTS", null);
            SQ4KOUUIDiagnostics.AttachUI(this);

            //  SetProcessShutdownParameters
'@ 'InitializeComponent timing and UI attach'

# Database initialization.
$console = Replace-ExactOnce $console @'
            DB_Exists = DB.Init(radio_to_use.Model);		    // Initialize the database and pass the current radio model

            InitCTCSS();
'@ @'
            long sq4kouDbInitStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "DB_INIT", null);
            DB_Exists = DB.Init(radio_to_use.Model);		    // Initialize the database and pass the current radio model
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouDbInitStart, "STARTUP", "DB_INIT", "exists=" + DB_Exists);

            InitCTCSS();
'@ 'DB.Init timing'

# PortAudio initialization.
$console = Replace-ExactOnce $console @'
            PA19.PA_Initialize();								// Initialize the audio interface
            if (fwc_init) Thread.Sleep(600);
'@ @'
            long sq4kouPaInitStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "PORTAUDIO_INIT", null);
            PA19.PA_Initialize();								// Initialize the audio interface
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouPaInitStart, "STARTUP", "PORTAUDIO_INIT", null);
            if (fwc_init)
            {
                SQ4KOUUIDiagnostics.Mark("STARTUP", "FWC_SETTLE_SLEEP_BEGIN", "ms=600");
                Thread.Sleep(600);
                SQ4KOUUIDiagnostics.Mark("STARTUP", "FWC_SETTLE_SLEEP_END", "ms=600");
            }
'@ 'PortAudio and FWC settle timing'

# InitConsole contains display init, workers and construction of many secondary forms.
$console = Replace-ExactOnce $console @'
            InitConsole();											// Initialize all forms and main variables           

            Splash.SetStatus("Finished");
'@ @'
            long sq4kouInitConsoleStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "INIT_CONSOLE", null);
            InitConsole();											// Initialize all forms and main variables
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouInitConsoleStart, "STARTUP", "INIT_CONSOLE", null);

            Splash.SetStatus("Finished");
'@ 'InitConsole timing'

# SyncDSP is late in constructor and precedes AutoStart.
$console = Replace-ExactOnce $console @'
            SyncDSP();

            //  initializing = false;
'@ @'
            long sq4kouSyncDspStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "SYNC_DSP", null);
            SyncDSP();
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouSyncDspStart, "STARTUP", "SYNC_DSP", null);

            //  initializing = false;
'@ 'SyncDSP timing'

# Keep native AutoStart semantics unchanged, only bracket it with timestamps.
$console = Replace-ExactOnce $console @'
                if (setupForm.chkBoxAutoStart.Checked == true)
                {
                    chkPower.Checked = true;
                }
'@ @'
                if (setupForm.chkBoxAutoStart.Checked == true)
                {
                    long sq4kouAutoStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "AUTOSTART_POWER_ON", null);
                    chkPower.Checked = true;
                    SQ4KOUUIDiagnostics.OperationEnd(sq4kouAutoStart, "STARTUP", "AUTOSTART_POWER_ON", "power=" + chkPower.Checked);
                }
'@ 'native AutoStart timing'

# Constructor completion. Do not move initializing=false.
$console = Replace-ExactOnce $console @'
            initializing = false; //.242 moved down here
'@ @'
            initializing = false; //.242 moved down here
            SQ4KOUUIDiagnostics.Mark("STARTUP", "CONSOLE_CTOR_BODY_END", "initializing=false");
'@ 'constructor end timing'

# Count display requests globally, without changing the native scheduling mechanism.
$console = Replace-ExactOnce $console @'
                case DisplayEngine.GDI_PLUS:

                    picDisplay.Invalidate();


                    break;
'@ @'
                case DisplayEngine.GDI_PLUS:

                    SQ4KOUUIDiagnostics.InvalidateRequested();
                    picDisplay.Invalidate();


                    break;
'@ 'UpdateDisplay invalidate counter'

# Count producer cycles at the native RunDisplay handoff.
$console = Replace-ExactOnce $console @'
                UpdateDisplay(); // ke9ns 

                if (chkPower.Checked)
'@ @'
                SQ4KOUUIDiagnostics.ProducerTick();
                UpdateDisplay(); // ke9ns 

                if (chkPower.Checked)
'@ 'RunDisplay producer counter'

# Paint and renderer timings. No rendering behavior is changed.
$console = Replace-ExactOnce $console @'
        private void picDisplay_Paint(object sender, PaintEventArgs e) //System.Windows.Forms.PaintEventArgs
        {
            PD = e;
'@ @'
        private void picDisplay_Paint(object sender, PaintEventArgs e) //System.Windows.Forms.PaintEventArgs
        {
            long sq4kouPaintStart = SQ4KOUUIDiagnostics.PaintBegin();
            PD = e;
'@ 'picDisplay paint begin'

$console = Replace-ExactOnce $console @'
                case DisplayEngine.GDI_PLUS:
                    Display.RenderGDIPlus(ref PD);  // System.Windows.Forms.PaintEventArgs
                    break;
'@ @'
                case DisplayEngine.GDI_PLUS:
                    long sq4kouRenderStart = SQ4KOUUIDiagnostics.RenderBegin();
                    try
                    {
                        Display.RenderGDIPlus(ref PD);  // System.Windows.Forms.PaintEventArgs
                    }
                    finally
                    {
                        SQ4KOUUIDiagnostics.RenderEnd(sq4kouRenderStart);
                    }
                    break;
'@ 'RenderGDIPlus timing'

$console = Replace-ExactOnce $console @'
        } //picDisplay_paint
'@ @'
            SQ4KOUUIDiagnostics.PaintEnd(sq4kouPaintStart);
        } //picDisplay_paint
'@ 'picDisplay paint end'

# Band path timings: save old stack, apply mode/filter/frequency, then RX1 band hardware/state cascade.
$console = Replace-ExactOnce $console @'
        public void SaveBandA()
        {
            checkBoxIICPTT.Checked = false; // ke9ns add
'@ @'
        public void SaveBandA()
        {
            long sq4kouSaveBandStart = SQ4KOUUIDiagnostics.OperationBegin("BAND", "SAVE_BAND_A", "band=" + rx1_band + ";vfo=" + VFOAFreq.ToString("F6"));
            checkBoxIICPTT.Checked = false; // ke9ns add
'@ 'SaveBandA begin'

$console = Replace-ExactOnce $console @'
        } // saveband


        //======================================================================================
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouSaveBandStart, "BAND", "SAVE_BAND_A", "band=" + rx1_band);
        } // saveband


        //======================================================================================
'@ 'SaveBandA end'

$console = Replace-ExactOnce $console @'
        public void SetBand(string mode, string filter, double freq)
        {
            if (filter.Contains("@"))
'@ @'
        public void SetBand(string mode, string filter, double freq)
        {
            long sq4kouSetBandStart = SQ4KOUUIDiagnostics.OperationBegin("BAND", "SET_BAND", "mode=" + mode + ";filter=" + filter + ";freq=" + freq.ToString("F6"));
            if (filter.Contains("@"))
'@ 'SetBand begin'

$console = Replace-ExactOnce $console @'
        } // setband

        private void radGenBandVHF_Click
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouSetBandStart, "BAND", "SET_BAND", "rx1_band=" + rx1_band + ";vfo=" + VFOAFreq.ToString("F6"));
        } // setband

        private void radGenBandVHF_Click
'@ 'SetBand end'

$console = Replace-ExactOnce $console @'
        private void SetRX1Band(Band b)
        {

            //   panelBandHF.Invalidate();
'@ @'
        private void SetRX1Band(Band b)
        {
            long sq4kouSetRx1BandStart = SQ4KOUUIDiagnostics.OperationBegin("BAND", "SET_RX1_BAND", "old=" + rx1_band + ";new=" + b);

            //   panelBandHF.Invalidate();
'@ 'SetRX1Band begin'

$console = Replace-ExactOnce $console @'
        } // setrx1band

        private void SetRX2Band
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouSetRx1BandStart, "BAND", "SET_RX1_BAND", "now=" + rx1_band);
        } // setrx1band

        private void SetRX2Band
'@ 'SetRX1Band end'

# Layout/resize timing. This is intentionally passive and leaves all geometry writes native.
$console = Replace-ExactOnce $console @'
        public void Console_Resize(object sender, System.EventArgs e)
        {

            //  if (FirstDown == true) return;
'@ @'
        public void Console_Resize(object sender, System.EventArgs e)
        {
            long sq4kouResizeStart = SQ4KOUUIDiagnostics.OperationBegin("LAYOUT", "CONSOLE_RESIZE", "state=" + WindowState + ";size=" + Width + "x" + Height);

            //  if (FirstDown == true) return;
'@ 'Console_Resize begin'

$console = Replace-ExactOnce $console @'
        } // console resize

        private void Console_SizeChanged
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouResizeStart, "LAYOUT", "CONSOLE_RESIZE", "state=" + WindowState + ";size=" + Width + "x" + Height);
        } // console resize

        private void Console_SizeChanged
'@ 'Console_Resize end'

[IO.File]::WriteAllText($consoleCs,$console.Replace($lf,$crlf),$utf8)
[IO.File]::WriteAllText($projectCs,$project.Replace($lf,$crlf),$utf8)

Stage 'PASS: passive P17 instrumentation installed; native PAL/FWC/Audio/display ordering unchanged'
