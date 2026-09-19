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

# Register helper.
if($project -notmatch 'Compile Include="SQ4KOUUIDiagnostics\.cs"') {
    $oldProject = @'
    <Compile Include="console.cs">
      <SubType>Form</SubType>
    </Compile>
'@
    $newProject = @'
    <Compile Include="console.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="SQ4KOUUIDiagnostics.cs" />
'@
    $project = Replace-ExactOnce $project $oldProject $newProject 'PowerSDR.csproj helper registration'
}

# Main / constructor lifecycle.
$console = Replace-ExactOnce $console @'
            Debug.WriteLine("===START=== Main here");
'@ @'
            Debug.WriteLine("===START=== Main here");
            SQ4KOUUIDiagnostics.StartEarly();
            SQ4KOUUIDiagnostics.Mark("STARTUP", "MAIN_BEGIN", null);
'@ 'Main begin'

$console = Replace-ExactOnce $console @'
                theConsole = new Console(args);  // ke9ns: run Console here
'@ @'
                long sq4kouConsoleCtorStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "CONSOLE_CTOR", null);
                theConsole = new Console(args);  // ke9ns: run Console here
                SQ4KOUUIDiagnostics.OperationEnd(sq4kouConsoleCtorStart, "STARTUP", "CONSOLE_CTOR", null);
'@ 'Console constructor timing'

$console = Replace-ExactOnce $console @'
                Application.Run(theConsole);
'@ @'
                SQ4KOUUIDiagnostics.Mark("STARTUP", "APPLICATION_RUN_ENTER", null);
                Application.Run(theConsole);
'@ 'Application.Run timing'

$console = Replace-ExactOnce $console @'
            Debug.WriteLine("===START=== Console here");
'@ @'
            SQ4KOUUIDiagnostics.Mark("STARTUP", "CONSOLE_CTOR_BODY_BEGIN", null);
            Debug.WriteLine("===START=== Console here");
'@ 'Console constructor body begin'

$console = Replace-ExactOnce $console @'
            InitializeComponent();                              // Windows Forms Generated Code
'@ @'
            long sq4kouInitComponentsStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "INITIALIZE_COMPONENTS", null);
            InitializeComponent();                              // Windows Forms Generated Code
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouInitComponentsStart, "STARTUP", "INITIALIZE_COMPONENTS", null);
            SQ4KOUUIDiagnostics.AttachUI(this);
'@ 'InitializeComponent timing'

$console = Replace-ExactOnce $console @'
            DB_Exists = DB.Init(radio_to_use.Model);		    // Initialize the database and pass the current radio model
'@ @'
            long sq4kouDbInitStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "DB_INIT", null);
            DB_Exists = DB.Init(radio_to_use.Model);		    // Initialize the database and pass the current radio model
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouDbInitStart, "STARTUP", "DB_INIT", "exists=" + DB_Exists);
'@ 'DB.Init timing'

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
'@ 'PortAudio/FWC settle timing'

$console = Replace-ExactOnce $console @'
            InitConsole();											// Initialize all forms and main variables           
'@ @'
            long sq4kouInitConsoleStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "INIT_CONSOLE", null);
            InitConsole();											// Initialize all forms and main variables           
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouInitConsoleStart, "STARTUP", "INIT_CONSOLE", null);
'@ 'InitConsole timing'

$console = Replace-ExactOnce $console @'
            SyncDSP();
'@ @'
            long sq4kouSyncDspStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "SYNC_DSP", null);
            SyncDSP();
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouSyncDspStart, "STARTUP", "SYNC_DSP", null);
'@ 'SyncDSP timing'

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
'@ 'AutoStart timing'

$console = Replace-ExactOnce $console @'
            initializing = false; //.242 moved down here
'@ @'
            initializing = false; //.242 moved down here
            SQ4KOUUIDiagnostics.Mark("STARTUP", "CONSOLE_CTOR_BODY_END", "initializing=false");
'@ 'Constructor end'

# Display producer -> invalidate -> Paint -> RenderGDIPlus.
$console = Replace-ExactOnce $console @'
                    picDisplay.Invalidate();
'@ @'
                    SQ4KOUUIDiagnostics.InvalidateRequested();
                    picDisplay.Invalidate();
'@ 'Main display Invalidate counter'

$console = Replace-ExactOnce $console @'
                UpdateDisplay(); // ke9ns 
'@ @'
                SQ4KOUUIDiagnostics.ProducerTick();
                UpdateDisplay(); // ke9ns 
'@ 'RunDisplay producer counter'

$console = Replace-ExactOnce $console @'
        private void picDisplay_Paint(object sender, PaintEventArgs e) //System.Windows.Forms.PaintEventArgs
        {
            PD = e;
'@ @'
        private void picDisplay_Paint(object sender, PaintEventArgs e) //System.Windows.Forms.PaintEventArgs
        {
            long sq4kouPaintStart = SQ4KOUUIDiagnostics.PaintBegin();
            PD = e;
'@ 'Paint begin'

$console = Replace-ExactOnce $console @'
                case DisplayEngine.GDI_PLUS:
                    Display.RenderGDIPlus(ref PD);  // System.Windows.Forms.PaintEventArgs
                    break;
'@ @'
                case DisplayEngine.GDI_PLUS:
                {
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
                }
'@ 'RenderGDIPlus timing'

$console = Replace-ExactOnce $console @'
        } //picDisplay_paint
'@ @'
            SQ4KOUUIDiagnostics.PaintEnd(sq4kouPaintStart);
        } //picDisplay_paint
'@ 'Paint end'

# BAND path.
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

        // ke9ns add .206 band stack to vfoB
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouSetBandStart, "BAND", "SET_BAND", "rx1_band=" + rx1_band + ";vfo=" + VFOAFreq.ToString("F6"));
        } // setband

        // ke9ns add .206 band stack to vfoB
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
        } //SETRX1BAND

        private void SetRX2Band(Band b)
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouSetRx1BandStart, "BAND", "SET_RX1_BAND", "now=" + rx1_band);
        } //SETRX1BAND

        private void SetRX2Band(Band b)
'@ 'SetRX1Band end'

# Main form resize/layout.
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
        } // control resize



        private int rx2_fixed_gain = 20;
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouResizeStart, "LAYOUT", "CONSOLE_RESIZE", "state=" + WindowState + ";size=" + Width + "x" + Height);
        } // control resize



        private int rx2_fixed_gain = 20;
'@ 'Console_Resize end'

[IO.File]::WriteAllText($consoleCs,$console.Replace($lf,$crlf),$utf8)
[IO.File]::WriteAllText($projectCs,$project.Replace($lf,$crlf),$utf8)

Stage 'PASS: passive P17 instrumentation installed; native PAL/FWC/Audio/display ordering unchanged'
