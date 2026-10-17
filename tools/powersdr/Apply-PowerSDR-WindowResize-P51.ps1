[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$consoleDir = Join-Path $SourceRoot 'Console'
$projPath = Join-Path $consoleDir 'PowerSDR.csproj'
$positionPath = Join-Path $consoleDir 'P50WindowPositions.cs'
$designerPath = Join-Path $consoleDir 'console.Designer.cs'
$resizeSrc = Join-Path $PSScriptRoot 'P51BandscopeResize.cs'
$resizeDst = Join-Path $consoleDir 'P51BandscopeResize.cs'
$consolePath = Join-Path $consoleDir 'console.cs'
$utf8 = New-Object System.Text.UTF8Encoding($true)
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$lf=[string][char]10
$crlf=([string][char]13)+[char]10
foreach($path in @($projPath,$positionPath,$designerPath,$resizeSrc,$consolePath)) {
    if(!(Test-Path $path)) { throw "P51 prerequisite missing: $path" }
}

function Replace-Once([string]$text,[string]$old,[string]$replacement,[string]$name) {
    $start = $text.IndexOf($old,[System.StringComparison]::Ordinal)
    if($start -lt 0) { throw "P51 missing source anchor: $name" }
    if($text.IndexOf($old,$start+$old.Length,[System.StringComparison]::Ordinal) -ge 0) {
        throw "P51 ambiguous source anchor: $name"
    }
    return $text.Substring(0,$start) + $replacement + $text.Substring($start+$old.Length)
}

$designer = [IO.File]::ReadAllText($designerPath)
foreach($name in @(
    'panelDisplay','picDisplay','panelDisplay2','panelVFO','panelTS1',
    'panelTSRadar','panelRX2Display','panelRX2Mixer','panelMultiRX',
    'grpDisplaySplit','picSquelch','ptbSquelch','picRX2Squelch',
    'ptbRX2Squelch','ptbRX2RF','lblRX2RF','lblRX2Band',
    'comboRX2Band','chkRX2Preamp','chkRX2Squelch'
)) {
    if(!$designer.Contains('this.'+$name+'.Name = "'+$name+'"')) {
        throw "P51 native control not found: $name"
    }
}

$code = [IO.File]::ReadAllText($positionPath).Replace($crlf,$lf)
$oldGroups = @'
            P50AddGroup("Date_Time", saved, panelDateTime);
'@.Replace($crlf,$lf)
$newGroups = @'
            P50AddGroup("Date_Time", saved, panelDateTime);

            // P51 – remaining movable native control groups and the spectrum.
            // Distinct groups retain independent positions in the P50 table.
            // These lower RX1/RX2 selector panels belong to the display.
            // They must follow the waterfall when moving it.
            P50AddGroup("BandscopeWaterfall", saved,
                panelDisplay, panelDisplay2, panelRX2Display);
            P50AddGroup("VFO_Controls", saved, panelVFO);
            P50AddGroup("TS_Controls", saved, panelTS1);
            P50AddGroup("Radar", saved, panelTSRadar);
            P50AddGroup("DisplaySplit", saved, grpDisplaySplit);
            P50AddGroup("RX2_Mixer", saved, panelRX2Mixer);
            P50AddGroup("MultiRX", saved, panelMultiRX);
            P50AddGroup("Squelch", saved, picSquelch, ptbSquelch);
            P50AddGroup("RX2_Squelch", saved, picRX2Squelch, ptbRX2Squelch);
            P50AddGroup("RX2_RF", saved, ptbRX2RF, lblRX2RF);
            P50AddGroup("RX2_BandOptions", saved,
                lblRX2Band, comboRX2Band, chkRX2Preamp, chkRX2Squelch);
'@.Replace($crlf,$lf)
$code = Replace-Once $code $oldGroups $newGroups 'P50 native group registration'

$oldReady = @'
            p50Filter = new P50MouseFilter(this);
'@.Replace($crlf,$lf)
$newReady = @'
            // Add P51 size restoration after P50 has restored panel positions.
            P51InitBandscopeResize();
            p50Filter = new P50MouseFilter(this);
'@.Replace($crlf,$lf)
$code = Replace-Once $code $oldReady $newReady 'P51 display resize startup'

$oldFilter = @'
        private bool P50FilterMouse(Message message)
        {
            if (p50ActiveGroup != null)
'@.Replace($crlf,$lf)
$newFilter = @'
        private bool P50FilterMouse(Message message)
        {
            if (P51FilterBandscopeResize(message)) return true;
            if (p50ActiveGroup != null)
'@.Replace($crlf,$lf)
$code = Replace-Once $code $oldFilter $newFilter 'P51 shared mouse filter'
$code = $code.Replace(
    '// Do not register panelDisplay, panelDisplay2 or Meters/Gadgets.',
    '// Meters/Gadgets remains owned by its native P48 persistence.')
[IO.File]::WriteAllText($positionPath,$code.Replace($lf,$crlf),$utf8)

Copy-Item $resizeSrc $resizeDst -Force

# KE9NS ResizeConsole recalculates the toolbar positions independently of
# panelDisplay/picDisplay sizing. Re-align only AFTER the original native
# WinForms layout has completed (no DSP/renderer modification).
$console = [IO.File]::ReadAllText($consolePath).Replace($crlf,$lf)
$resizeStart = @'
        public void ResizeConsole(int h_delta1, int v_delta)        //k6jca 1/15/08
        {
'@.Replace($crlf,$lf)
$resizeStartNew = @'
        public void ResizeConsole(int h_delta1, int v_delta)        //k6jca 1/15/08
        {
            bool p51NativeLayoutWasApplied =
                !(h_delta1 == 0 && v_delta == 0 && previous_delta == 0);
'@.Replace($crlf,$lf)
$console = Replace-Once $console $resizeStart $resizeStartNew 'KE9NS ResizeConsole entry'

$resizeEnd = @'
            panelTSRadar.Invalidate(); //.246


        } // resizeconsole
'@.Replace($crlf,$lf)
$resizeEndNew = @'
            panelTSRadar.Invalidate(); //.246

            if (p51NativeLayoutWasApplied)
                P51AfterNativeConsoleResize();

        } // resizeconsole
'@.Replace($crlf,$lf)
$console = Replace-Once $console $resizeEnd $resizeEndNew 'KE9NS ResizeConsole completion'
[IO.File]::WriteAllText($consolePath,$console.Replace($lf,$crlf),$utf8)

$proj = [IO.File]::ReadAllText($projPath)
$oldProj = '<Compile Include="P50WindowPositions.cs" />'
if(!$proj.Contains($oldProj)){throw 'P51 P50 csproj anchor missing'}
if($proj.Contains('<Compile Include="P51BandscopeResize.cs" />')){
    throw 'P51 already compiled unexpectedly'
}
$proj = Replace-Once $proj $oldProj ($oldProj+[Environment]::NewLine+'    '+'<Compile Include="P51BandscopeResize.cs" />') 'P51 csproj'
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

$verify = [IO.File]::ReadAllText($positionPath)
foreach($required in @(
    'P50AddGroup("BandscopeWaterfall", saved,',
    'panelDisplay, panelDisplay2, panelRX2Display);',
    'P51InitBandscopeResize();','P51FilterBandscopeResize(message)'
)){
    if(!$verify.Contains($required)){throw "P51 source gate failed: $required"}
}
$resize = [IO.File]::ReadAllText($resizeDst)
foreach($required in @(
    'picDisplay.Size = new Size(', 'P51SaveSizeOffset();',
    'P50StoreOffsets();', 'P51NativeDisplaySizeChanged',
    'P51DragResize()', 'P51FinishResize()',
    'P51AfterNativeConsoleResize()', 'p51Rx1StripStart',
    'p51Rx2StripStart'
)){
    if(!$resize.Contains($required)){throw "P51 native resize gate failed: $required"}
}

$verifyNative = [IO.File]::ReadAllText($consolePath)
foreach($required in @('bool p51NativeLayoutWasApplied', 'P51AfterNativeConsoleResize();')){
    if(!$verifyNative.Contains($required)){ throw "P51 native-layout gate missing: $required" }
}

Write-Host 'P51_MAIN_UI=EXTENDED_P50_SHIFT_LMB'
Write-Host 'P51_BANDSCOPE_MOVE=P50_SHIFT_LMB'
Write-Host 'P51_BANDSCOPE_RESIZE=SHIFT_LMB_ALL_EDGES'
Write-Host 'P51_RENDERER=NATIVE_PICDISPLAY_RESIZE'
Write-Host 'P51_GEOMETRY=P50_POSITION_PLUS_P51_SIZE_DURABLE'
Write-Host 'P51_FLEX5000_DSP=UNCHANGED'
