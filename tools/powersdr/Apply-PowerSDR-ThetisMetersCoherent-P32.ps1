[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# Coherent P32 rebuild: one Thetis source point for the complete meter core/container.
# P30 is retained only for FLEX-5000 telemetry helpers, DB replace-all semantics and
# the official DefaultMeters package. Its old P25 meter sources are replaced below.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P30.ps1') -SourceRoot $SourceRoot

$ThetisSha='a53b19274e715182d8f386bfabbb2c4164287a0e'
$RawBase="https://raw.githubusercontent.com/ramdor/Thetis/$ThetisSha/Project%20Files/Source/Console"
$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$pkgPath=Join-Path $consoleDir 'packages.config'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$mmPath=Join-Path $consoleDir 'P25_MeterManager.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

$sourceMap=[ordered]@{
 'MeterManager.cs'='P25_MeterManager.cs'
 'ucMeter.cs'='P25_ucMeter.cs'
 'ucMeter.Designer.cs'='P25_ucMeter.Designer.cs'
 'frmMeterDisplay.cs'='P25_frmMeterDisplay.cs'
 'frmMeterDisplay.Designer.cs'='P25_frmMeterDisplay.Designer.cs'
 'clsImgeFetcher.cs'='P32_clsImageFetcher.cs'
}
foreach($kv in $sourceMap.GetEnumerator())
{
    $tmp=Join-Path $env:TEMP ('P32_'+$kv.Key)
    Invoke-WebRequest -UseBasicParsing -Uri ($RawBase+'/'+$kv.Key) -OutFile $tmp
    $c=[IO.File]::ReadAllText($tmp)
    $c=$c.Replace('namespace Thetis','namespace PowerSDR')
    $c=$c.Replace('global::Thetis.Properties.Resources.','global::PowerSDR.P25MeterResources.')
    $c=$c.Replace('Properties.Resources.','P25MeterResources.')
    if($kv.Key -eq 'MeterManager.cs')
    {
        $c=$c.Replace('using RawInput_dll;','')
        $c=$c.Replace(
            'private HiPerfTimer _objFrameStartTimer = new HiPerfTimer();',
            'private System.Diagnostics.Stopwatch _objFrameStartTimer = System.Diagnostics.Stopwatch.StartNew();'
        )
        $c=$c.Replace('_objFrameStartTimer.ElapsedMsec','_objFrameStartTimer.Elapsed.TotalMilliseconds')
        # PowerSDR has no Thetis S9Frequency setting; this is the same fixed split
        # already used by the physically verified P25 bridge.
        $c=$c.Replace('_console.S9Frequency','30.0')
    }
    if($kv.Key -eq 'ucMeter.Designer.cs')
    {
        $c=[regex]::Replace($c,'(?m)^\s*this\.[A-Za-z0-9_]+\.Selectable\s*=\s*false;\s*$','')
    }
    [IO.File]::WriteAllText((Join-Path $consoleDir $kv.Value),$c,$utf8)
}

# Newer coherent ucMeter/renderer resources.
$resDir=Join-Path $consoleDir 'Resources'
New-Item -ItemType Directory -Force -Path $resDir | Out-Null
$res=[ordered]@{
 'gear'='gear.png'
 'Lock_64'='Lock-64.png'
 'Link_64'='Link-64.png'
}
foreach($name in $res.Keys)
{
    $file=$res[$name]
    $url="$RawBase/Resources/$file"
    Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile (Join-Path $resDir $file)
}

# Resource adapter and minimum PowerSDR hardware identity expected by the coherent core.
$bridge=[IO.File]::ReadAllText($bridgePath)
$resourceAnchor='        public static Image resizegrab { get { return Load("resizegrab"); } }'
if(!$bridge.Contains($resourceAnchor)){throw 'P32 coherent resource anchor missing'}
foreach($line in @(
 '        public static Image gear { get { return Load("gear"); } }',
 '        public static Image Lock_64 { get { return Load("Lock-64"); } }',
 '        public static Image Link_64 { get { return Load("Link-64"); } }'
))
{
    if(!$bridge.Contains($line)){$bridge=$bridge.Replace($resourceAnchor,$resourceAnchor+$nl+$line)}
}
$identityAnchor='        internal bool ApolloPresent { get { return false; } }'
if(!$bridge.Contains($identityAnchor)){throw 'P32 coherent console identity anchor missing'}
if(!$bridge.Contains('internal bool PAPresent'))
{
    $bridge=$bridge.Replace($identityAnchor,$identityAnchor+$nl+
      '        internal bool PAPresent { get { return current_model == Model.FLEX5000 || current_model == Model.FLEX3000; } }'+$nl+
      '        internal int TXXVTRIndex { get { return -1; } }')
}

# Newer MeterManager Init owns image loading and no longer takes a skin path.
$bridge=$bridge.Replace(
 'MeterManager.Init(this, Path.Combine(Application.StartupPath, "MeterSkins"));',
 'MeterManager.Init(this);'
)
# P30 inserts its scale call after the old Init signature. Preserve it if present.
$bridge=$bridge.Replace(
 'MeterManager.Init(this);'+$nl+'                MeterManager.P30ConfigureFlex5000();',
 'MeterManager.Init(this);'+$nl+'                MeterManager.P30ConfigureFlex5000();'
)
[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

$mm=[IO.File]::ReadAllText($mmPath)

# Thetis event bus is not copied into PowerSDR. State is polled through the single
# adapter below, while native Thetis meter/item state machines remain intact.
$eventRx='(?s)        private static void addDelegates\(\).*?        private static void OnTransverterIndexChanged'
$eventReplacement=@'
        private static void addDelegates()
        {
            _delegatesAdded = true;
        }
        private static void removeDelegates()
        {
            if (_lstUCMeters != null)
            {
                foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
                    kvp.Value.RemoveDelegates();
            }
            _delegatesAdded = false;
        }
        private static void OnTransverterIndexChanged
'@
$mm2=[regex]::Replace($mm,$eventRx,$eventReplacement,1)
if($mm2 -eq $mm){throw 'P32 coherent failed to replace Thetis event bus'}
$mm=$mm2

# Installed meter skin directory instead of OpenHPSDR AppData skin tree.
$mm=$mm.Replace(
 '_openHPSDR_appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\OpenHPSDR";',
 '_openHPSDR_appdatapath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "MeterSkins");'
)
$mm=$mm.Replace(
 'string sDefaultFileName = sDefaultPath + "\\Meters\\" + imageFileNames[n];',
 'string sDefaultFileName = sDefaultPath + "\\" + imageFileNames[n];'
)
# No external Thetis skin selector exists in PowerSDR.
$mm=$mm.Replace(
 'string sSkinFileName = sSkinPath + "\\Meters\\" + imageFileNames[n];',
 'string sSkinFileName = System.IO.Path.Combine(sDefaultPath, "__no_external_skin__" + imageFileNames[n]);'
)

# FLEX-5000 meter telemetry/state adapter. Unsupported native readings are not
# synthesized; they remain unavailable rather than receiving invented values.
$updateAnchor='        private static void UpdateMeters()'
if(!$mm.Contains($updateAnchor)){throw 'P32 coherent UpdateMeters anchor missing'}
$adapter=@'
        internal static void P30ConfigureFlex5000()
        {
            _paPresent = true;
            _alexPresent = false;
            _apolloPresent = false;
            _transverterIndex = -1;
        }

        private static Band P32BandFromFrequency(double mhz)
        {
            if (mhz >= 1.8 && mhz < 2.0) return Band.B160M;
            if (mhz >= 3.5 && mhz < 4.0) return Band.B80M;
            if (mhz >= 5.0 && mhz < 5.6) return Band.B60M;
            if (mhz >= 7.0 && mhz < 7.4) return Band.B40M;
            if (mhz >= 10.0 && mhz < 10.3) return Band.B30M;
            if (mhz >= 14.0 && mhz < 14.5) return Band.B20M;
            if (mhz >= 18.0 && mhz < 18.3) return Band.B17M;
            if (mhz >= 21.0 && mhz < 21.6) return Band.B15M;
            if (mhz >= 24.8 && mhz < 25.1) return Band.B12M;
            if (mhz >= 28.0 && mhz < 30.0) return Band.B10M;
            if (mhz >= 50.0 && mhz < 54.0) return Band.B6M;
            return Band.GEN;
        }

        private static void P32RefreshPowerSDR()
        {
            if (_console == null) return;

            bool mox = _console.MOX;

            if (_readings[1].RequiresUpdate(Reading.SIGNAL_STRENGTH))
                _readings[1].SetReading(Reading.SIGNAL_STRENGTH, _console.P25ReadRx1SignalDbm());
            if (_readings[1].RequiresUpdate(Reading.AVG_SIGNAL_STRENGTH))
                _readings[1].SetReading(Reading.AVG_SIGNAL_STRENGTH, _console.P25ReadRx1AverageSignalDbm());
            if (_readings[1].RequiresUpdate(Reading.AGC_GAIN))
                _readings[1].SetReading(Reading.AGC_GAIN, _console.P25ReadRx1AgcGain());

            if (mox)
            {
                if (_readings[1].RequiresUpdate(Reading.MIC))
                    _readings[1].SetReading(Reading.MIC, _console.P30ReadTxAverage(DttSP.MeterType.MIC));
                if (_readings[1].RequiresUpdate(Reading.MIC_PK))
                    _readings[1].SetReading(Reading.MIC_PK, _console.P30ReadTxPeak(DttSP.MeterType.MIC_PK));
                if (_readings[1].RequiresUpdate(Reading.EQ))
                    _readings[1].SetReading(Reading.EQ, _console.P30ReadTxAverage(DttSP.MeterType.EQ));
                if (_readings[1].RequiresUpdate(Reading.EQ_PK))
                    _readings[1].SetReading(Reading.EQ_PK, _console.P30ReadTxPeak(DttSP.MeterType.EQ_PK));
                if (_readings[1].RequiresUpdate(Reading.LEVELER))
                    _readings[1].SetReading(Reading.LEVELER, _console.P30ReadTxAverage(DttSP.MeterType.LEVELER));
                if (_readings[1].RequiresUpdate(Reading.LEVELER_PK))
                    _readings[1].SetReading(Reading.LEVELER_PK, _console.P30ReadTxPeak(DttSP.MeterType.LEVELER_PK));
                if (_readings[1].RequiresUpdate(Reading.LVL_G))
                    _readings[1].SetReading(Reading.LVL_G, _console.P30ReadLevelerGain());
                if (_readings[1].RequiresUpdate(Reading.ALC))
                    _readings[1].SetReading(Reading.ALC, _console.P30ReadTxAverage(DttSP.MeterType.ALC));
                if (_readings[1].RequiresUpdate(Reading.ALC_PK))
                    _readings[1].SetReading(Reading.ALC_PK, _console.P30ReadTxPeak(DttSP.MeterType.ALC_PK));
                if (_readings[1].RequiresUpdate(Reading.ALC_G))
                    _readings[1].SetReading(Reading.ALC_G, _console.P30ReadAlcGain());
                if (_readings[1].RequiresUpdate(Reading.COMP))
                    _readings[1].SetReading(Reading.COMP, _console.P30ReadTxAverage(DttSP.MeterType.COMP));
                if (_readings[1].RequiresUpdate(Reading.COMP_PK))
                    _readings[1].SetReading(Reading.COMP_PK, _console.P30ReadTxPeak(DttSP.MeterType.COMP_PK));
                if (_readings[1].RequiresUpdate(Reading.PWR))
                    _readings[1].SetReading(Reading.PWR, _console.P30ReadTxForwardWatts());
                if (_readings[1].RequiresUpdate(Reading.REVERSE_PWR))
                    _readings[1].SetReading(Reading.REVERSE_PWR, _console.P30ReadTxReverseWatts());
                if (_readings[1].RequiresUpdate(Reading.SWR))
                    _readings[1].SetReading(Reading.SWR, _console.P30ReadTxSWR());
                if (_readings[1].RequiresUpdate(Reading.PA_FWD_PWR))
                    _readings[1].SetReading(Reading.PA_FWD_PWR, _console.P30ReadTxForwardWatts());
                if (_readings[1].RequiresUpdate(Reading.PA_REV_PWR))
                    _readings[1].SetReading(Reading.PA_REV_PWR, _console.P30ReadTxReverseWatts());
                if (_readings[1].RequiresUpdate(Reading.CAL_FWD_PWR))
                    _readings[1].SetReading(Reading.CAL_FWD_PWR, _console.P30ReadTxForwardWatts());
                if (_readings[1].RequiresUpdate(Reading.VOLTS))
                    _readings[1].SetReading(Reading.VOLTS, _console.P30ReadPaVolts());
            }

            lock (_metersLock)
            {
                foreach (KeyValuePair<string, clsMeter> kvp in _meters)
                {
                    clsMeter meter = kvp.Value;
                    if (meter.RX != 1) continue;

                    Band oldBand = meter.BandVfoA;
                    DSPMode oldMode = meter.ModeVfoA;
                    int oldStep = meter.TuneStepIndex;

                    meter.MOX = mox;
                    meter.Split = _console.VFOSplit;
                    meter.TXVFOb = _console.VFOBTX;
                    meter.RX2Enabled = false;
                    meter.MultiRxEnabled = false;
                    meter.VfoA = _console.VFOAFreq;
                    meter.VfoB = _console.VFOBFreq;
                    meter.VfoSub = _console.VFOASubFreq;
                    meter.ModeVfoA = _console.RX1DSPMode;
                    meter.ModeVfoB = _console.RX1DSPMode;
                    meter.BandVfoA = _console.RX1Band;
                    meter.BandVfoB = P32BandFromFrequency(_console.VFOBFreq);
                    meter.BandVfoASub = _console.RX1Band;
                    meter.FilterVfoA = _console.RX1Filter;
                    meter.FilterVfoB = _console.RX1Filter;
                    meter.FilterVfoAName = _console.RX1Filter.ToString();
                    meter.FilterVfoBName = _console.RX1Filter.ToString();
                    meter.VFOALock = _console.P32VFOALock;
                    meter.VFOBLock = _console.P32VFOBLock;
                    meter.VFOSync = _console.VFOSync;
                    meter.QuickSplitEnabled = false;
                    meter.TuneStepIndex = _console.TuneStepIndex;

                    if (meter.SortedMeterItemsForZOrder != null)
                    {
                        foreach (clsMeterItem item in meter.SortedMeterItemsForZOrder)
                        {
                            if (oldBand != meter.BandVfoA) item.BandChanged(oldBand, meter.BandVfoA);
                            if (oldMode != meter.ModeVfoA) item.ModeChanged(oldMode, meter.ModeVfoA);
                            if (oldStep != meter.TuneStepIndex) item.TuneStepIndexChanged(oldStep, meter.TuneStepIndex);
                        }
                    }
                }
            }
        }

'@
$mm=$mm.Replace($updateAnchor,$adapter+$updateAnchor)

$loopMarker='while (_meterThreadRunning)'
$loopPos=$mm.IndexOf($loopMarker,[StringComparison]::Ordinal)
if($loopPos -lt 0){throw 'P32 coherent meter loop anchor missing'}
$openBracePos=$mm.IndexOf('{',$loopPos+$loopMarker.Length)
if($openBracePos -lt 0){throw 'P32 coherent meter loop opening brace missing'}
$mm=$mm.Insert($openBracePos+1,$nl+'                P32RefreshPowerSDR();')

# Culture adapter: exact Thetis VFO parsing assumes ".".
$dxMethod='            private void dxRender()'
$pos=$mm.IndexOf($dxMethod,[StringComparison]::Ordinal)
if($pos -lt 0){throw 'P32 coherent dxRender marker missing'}
$guard='                if (!_bDXSetup) return;'
$g=$mm.IndexOf($guard,$pos,[StringComparison]::Ordinal)
if($g -lt 0){throw 'P32 coherent dxRender guard missing'}
$culture='                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;'
if(!$mm.Contains($culture)){$mm=$mm.Insert($g+$guard.Length,$nl+$nl+$culture)}

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# Console-only API boundary needed by the coherent Thetis core.
$adapterPath=Join-Path $consoleDir 'P32ThetisPowerSDRAdapter.cs'
$adapterSource=@'
using System;
using System.Windows.Forms;

namespace PowerSDR
{
    sealed unsafe public partial class Console
    {
        internal bool P32VFOALock
        {
            get { return (CATVFOLockAB & 1) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 1) : (CATVFOLockAB & ~1); }
        }
        internal bool P32VFOBLock
        {
            get { return (CATVFOLockAB & 2) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 2) : (CATVFOLockAB & ~2); }
        }

        internal void P32SetRX1Band(Band band) { RX1Band = band; }
        internal void P32SetRX2Band(Band band) { RX2Band = band; }

        internal void P32PopupFilterMenu(int rx)
        {
            // PowerSDR has no Thetis filter-popup API. Filter buttons themselves
            // map directly to RX1Filter; only the Thetis context-popup has no native peer.
        }
    }
}
'@
[IO.File]::WriteAllText($adapterPath,$adapterSource,$utf8)

# Mechanical substitutions for APIs whose PowerSDR equivalents differ in name.
$mm=[IO.File]::ReadAllText($mmPath)
$mm=$mm.Replace('_console.BandPreChangeHandlers?.Invoke(1, b);','_console.P32SetRX1Band(b);')
$mm=$mm.Replace('_console.SetupRX2Band(b, false);','_console.P32SetRX2Band(b);')
$mm=$mm.Replace('_console.SetupRX2Band(b);','_console.P32SetRX2Band(b);')
$mm=$mm.Replace('_console.PopupFilterContextMenu(_owningmeter.RX, e);','_console.P32PopupFilterMenu(_owningmeter.RX);')
$mm=$mm.Replace('_console.PopupFilterContextMenu(_owningmeter.RX, null);','_console.P32PopupFilterMenu(_owningmeter.RX);')
[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# NuGet packages used by the coherent Thetis MeterManager/ImageFetcher.
$pkg=[IO.File]::ReadAllText($pkgPath)
$packages=@(
 @('HtmlAgilityPack','1.11.62','net48'),
 @('Microsoft.CodeAnalysis.Common','4.10.0','net48'),
 @('Microsoft.CodeAnalysis.CSharp','4.10.0','net48'),
 @('Microsoft.CodeAnalysis.CSharp.Scripting','4.10.0','net48'),
 @('Microsoft.CodeAnalysis.Scripting.Common','4.10.0','net48'),
 @('SkiaSharp','2.88.8','net48'),
 @('SkiaSharp.NativeAssets.Win32','2.88.8','net48'),
 @('Svg','3.4.7','net48')
)
foreach($p in $packages)
{
    if($pkg -notmatch ('id="'+[regex]::Escape($p[0])+'"'))
    {
        $line='  <package id="'+$p[0]+'" version="'+$p[1]+'" targetFramework="'+$p[2]+'" />'
        $pkg=$pkg.Replace('</packages>',$line+$nl+'</packages>')
    }
}
[IO.File]::WriteAllText($pkgPath,$pkg,$utf8NoBom)

$proj=[IO.File]::ReadAllText($projPath)
$refAnchor='<Reference Include="System.Drawing">'
if(!$proj.Contains($refAnchor)){throw 'P32 coherent reference anchor missing'}
$refs=@'
    <Reference Include="HtmlAgilityPack, Version=1.11.62.0, Culture=neutral, PublicKeyToken=bd319b19eaf3b43a, processorArchitecture=MSIL">
      <HintPath>..\packages\HtmlAgilityPack.1.11.62\lib\Net45\HtmlAgilityPack.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.CodeAnalysis, Version=4.10.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.CodeAnalysis.Common.4.10.0\lib\netstandard2.0\Microsoft.CodeAnalysis.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.CodeAnalysis.CSharp, Version=4.10.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.CodeAnalysis.CSharp.4.10.0\lib\netstandard2.0\Microsoft.CodeAnalysis.CSharp.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.CodeAnalysis.CSharp.Scripting, Version=4.10.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.CodeAnalysis.CSharp.Scripting.4.10.0\lib\netstandard2.0\Microsoft.CodeAnalysis.CSharp.Scripting.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.CodeAnalysis.Scripting, Version=4.10.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.CodeAnalysis.Scripting.Common.4.10.0\lib\netstandard2.0\Microsoft.CodeAnalysis.Scripting.dll</HintPath>
    </Reference>
    <Reference Include="SkiaSharp, Version=2.88.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756, processorArchitecture=MSIL">
      <HintPath>..\packages\SkiaSharp.2.88.8\lib\net462\SkiaSharp.dll</HintPath>
    </Reference>
    <Reference Include="Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL">
      <HintPath>..\packages\Svg.3.4.7\lib\net472\Svg.dll</HintPath>
    </Reference>
'@
if(!$proj.Contains('Reference Include="HtmlAgilityPack'))
{
    $proj=$proj.Replace($refAnchor,$refs+$nl+'    '+$refAnchor)
}

$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P32 coherent compile anchor missing'}
foreach($name in @('P32_clsImageFetcher.cs','P32ThetisPowerSDRAdapter.cs'))
{
    if(!$proj.Contains('<Compile Include="'+$name+'" />'))
    {
        $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="'+$name+'" />')
    }
}
foreach($file in @('gear.png','Lock-64.png','Link-64.png'))
{
    $rel='Resources\'+$file
    if($proj -notmatch ('Content Include="'+[regex]::Escape($rel)+'"'))
    {
        $item='    <Content Include="'+$rel+'">'+$nl+
              '      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>'+$nl+
              '    </Content>'+$nl
        $igPos=$proj.IndexOf('</ItemGroup>')
        if($igPos -lt 0){throw 'P32 coherent content ItemGroup anchor missing'}
        $proj=$proj.Insert($igPos,$item)
    }
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Hard source-coherence gates. These are source gates only; CI compile is the next gate.
$verify=[IO.File]::ReadAllText($mmPath)
foreach($token in @(
 'SIGNAL_TEXT','VFO_DISPLAY','CLOCK','SPACER','TEXT_OVERLAY','DATA_OUT','ROTATOR','LED','WEB_IMAGE',
 'BAND_BUTTONS','MODE_BUTTONS','FILTER_BUTTONS','ANTENNA_BUTTONS','HISTORY','TUNESTEP_BUTTONS',
 '_displayTarget.MouseDown += OnMouseDown;','_displayTarget.MouseWheel += OnMouseWheel;',
 'public static void LockContainer','public static bool ContainerLocked',
 'SetSetting<','GetSetting<'
))
{
    if(!$verify.Contains($token)){throw "P32 coherent source gate missing: $token"}
}
$ucVerify=[IO.File]::ReadAllText((Join-Path $consoleDir 'P25_ucMeter.cs'))
foreach($token in @('NoControls','Locked','ShowOnRX','ShowOnTX','AutoHeight'))
{
    if(!$ucVerify.Contains($token)){throw "P32 coherent ucMeter gate missing: $token"}
}

Write-Host "P32_THETIS_SOURCE_SHA=$ThetisSha"
Write-Host 'P32_SOURCE_COHERENCE=METER_MANAGER_UCMETER_DISPLAY_SAME_COMMIT'
Write-Host 'P32_CONTAINER=THETIS_NATIVE_21_FIELD_MODEL'
Write-Host 'P32_INPUT=THETIS_NATIVE_MOUSE_PIPELINE'
Write-Host 'P32_METER_TYPES=FULL_A53B192_GENERATION'
Write-Host 'P32_RX2=NOT_EXPOSED'
