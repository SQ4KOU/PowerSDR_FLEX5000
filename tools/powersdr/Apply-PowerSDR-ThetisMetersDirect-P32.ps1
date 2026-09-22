[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P32 rule: Thetis is the source of truth. P30 remains the verified PowerSDR/FLEX-5000
# hardware/telemetry base. All P32 VFO/BAND/MODE/TUNESTEP code below is extracted
# mechanically from one pinned Thetis source revision; adapters only bridge APIs
# that do not exist under the same name in PowerSDR.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P30.ps1') -SourceRoot $SourceRoot

$consoleDir = Join-Path $SourceRoot 'Console'
$mmPath = Join-Path $consoleDir 'P25_MeterManager.cs'
$ucPath = Join-Path $consoleDir 'P25_ucMeter.cs'
$bridgePath = Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$projPath = Join-Path $consoleDir 'PowerSDR.csproj'
$outPath = Join-Path $consoleDir 'P32ThetisExactGadgets.cs'
$adapterPath = Join-Path $consoleDir 'P32PowerSDRThetisAdapter.cs'
$utf8 = New-Object Text.UTF8Encoding($true)
$utf8NoBom = New-Object Text.UTF8Encoding($false)
$nl = [Environment]::NewLine

foreach($p in @($mmPath,$ucPath,$bridgePath,$projPath)) {
    if(!(Test-Path $p)){ throw "P32 required P30 file missing: $p" }
}

$ThetisSha='3dbd787eaef30eca66d089ccfb4c4ccb8c0cfb7e'
$ThetisMeterUrl="https://raw.githubusercontent.com/ramdor/Thetis/$ThetisSha/Project%20Files/Source/Console/MeterManager.cs"
$ThetisBandStackUrl="https://raw.githubusercontent.com/ramdor/Thetis/$ThetisSha/Project%20Files/Source/Console/clsBandStackManager.cs"
$ThetisCommonUrl="https://raw.githubusercontent.com/ramdor/Thetis/$ThetisSha/Project%20Files/Source/Console/common.cs"

$thetisMeter=(Invoke-WebRequest -UseBasicParsing -Uri $ThetisMeterUrl).Content
$thetisBand=(Invoke-WebRequest -UseBasicParsing -Uri $ThetisBandStackUrl).Content
$thetisCommon=(Invoke-WebRequest -UseBasicParsing -Uri $ThetisCommonUrl).Content

function Slice-Between([string]$text,[string]$start,[string]$end) {
    $a=$text.IndexOf($start,[StringComparison]::Ordinal)
    if($a -lt 0){throw "P32 Thetis source anchor missing: $start"}
    $b=$text.IndexOf($end,$a+[Math]::Max(1,$start.Length),[StringComparison]::Ordinal)
    if($b -lt 0){throw "P32 Thetis source end anchor missing: $end"}
    return $text.Substring($a,$b-$a).TrimEnd()
}

# Exact Thetis classes, byte-for-byte inside each extracted block.
$fadeClass = Slice-Between $thetisMeter '        internal class clsFadeCover : clsMeterItem' '        internal class clsFilterButtonBox : clsButtonBox'
$tuneClass = Slice-Between $thetisMeter '        internal class clsTunestepButtons : clsButtonBox' '        internal class clsModeButtonBox : clsButtonBox'
$modeClass = Slice-Between $thetisMeter '        internal class clsModeButtonBox : clsButtonBox' '        internal class clsBandButtonBox : clsButtonBox'
$bandClass = Slice-Between $thetisMeter '        internal class clsBandButtonBox : clsButtonBox' '        internal class clsButtonBox : clsMeterItem'
$buttonClass = Slice-Between $thetisMeter '        internal class clsButtonBox : clsMeterItem' '        internal class clsVfoDisplay : clsMeterItem'
$vfoClass = Slice-Between $thetisMeter '        internal class clsVfoDisplay : clsMeterItem' '        internal class clsClock : clsMeterItem'

# Exact Thetis constructors/layout methods.
$addBand = Slice-Between $thetisMeter '            public string AddBandButtons(' '            public string AddModeButtons('
$addMode = Slice-Between $thetisMeter '            public string AddModeButtons(' '            public string AddFilterButtons('
$addTune = Slice-Between $thetisMeter '            public string AddTunestepButtons(' '            public string AddVFODisplay('
$addVfo = Slice-Between $thetisMeter '            public string AddVFODisplay(' '            public string AddClock('
$getFadeCover = Slice-Between $thetisMeter '            private clsFadeCover getFadeCover(' '            private string addSMeterBar('
$getBounds = Slice-Between $thetisMeter '            internal System.Drawing.RectangleF getBounds(' '            internal Dictionary<string, clsMeterItem> itemsFromID('

# Exact Thetis renderer and interaction path.
$renderBlock = Slice-Between $thetisMeter '            private void renderFadeCover(' '            private void renderClock('
$mouseBlock = Slice-Between $thetisMeter '            private void OnMouseEnter(' '            private int drawMeters('
$fadeMethod = Slice-Between $thetisMeter '            private int fade(clsMeterItem mi, clsMeter m)' '            private void renderNeedleScale('
$measureMethod = Slice-Between $thetisMeter '            private SizeF measureString(string sText, string sFontFamily, FontStyle style, float emSize, bool ignore_caching = false)' '            private void renderScale('

# Exact BandToColour / BandToString and luminance helpers, moved only to adapter class names.
$bandColour = Slice-Between $thetisBand '        public static Color BandToColour(Band b)' '        public static string BandToString(Band b)'
$bandString = Slice-Between $thetisBand '        public static string BandToString(Band b)' '        public static Band StringToBand(string s)'
$luminance = Slice-Between $thetisCommon '        public static int GetLuminance(Color c)' '        public static void DoubleBufferAll('

# Compatibility-only textual substitutions. They do not change Thetis layout, colours,
# dimensions, state machine or click semantics.
foreach($name in @('fadeClass','tuneClass','modeClass','bandClass','buttonClass','vfoClass','addBand','addMode','addTune','addVfo','getFadeCover','getBounds','renderBlock','mouseBlock','fadeMethod','measureMethod')) {
    $v=Get-Variable $name -ValueOnly
    $v=$v.Replace('BandStackManager.','P32ThetisBandStackManager.')
    $v=$v.Replace('lock (_meterItemsLock)','lock (P32MeterItemsLock)')
    $v=$v.Replace('lock (m._meterItemsLock)','lock (m.P32MeterItemsLock)')
    Set-Variable -Name $name -Value $v
}
# Avoid a collision with the legacy 2023 renderer event method while replacing its subscription.
$mouseBlock=$mouseBlock.Replace('private void OnMouseEnter(','private void P32OnMouseEnter(')
$mouseBlock=$mouseBlock.Replace('private void OnMouseLeave(','private void P32OnMouseLeave(')
$mouseBlock=$mouseBlock.Replace('private void OnMouseMove(','private void P32OnMouseMove(')
$mouseBlock=$mouseBlock.Replace('private void OnMouseWheel(','private void P32OnMouseWheel(')
$mouseBlock=$mouseBlock.Replace('private void OnMouseClick(','private void P32OnMouseClick(')
$mouseBlock=$mouseBlock.Replace('private void OnMouseDown(','private void P32OnMouseDown(')
$mouseBlock=$mouseBlock.Replace('private void OnMouseUp(','private void P32OnMouseUp(')

# Thetis owns these types. The RX2 execution path is deliberately not exposed in this
# FLEX-5000 project, but the original VFO-B code remains present.
$generated=@"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace PowerSDR
{
    public enum BandGroups
    {
        GEN = 0,
        HF,
        VHF,
        LAST = 99
    }

    internal static partial class MeterManager
    {
$fadeClass

$tuneClass

$modeClass

$bandClass

$buttonClass

$vfoClass

        public partial class clsMeterItem
        {
            private PointF _p32MouseDownPoint = new PointF(0,0);
            private PointF _p32MouseUpPoint = new PointF(0,0);
            private PointF _p32MouseMovePoint = new PointF(0,0);
            private bool _p32MouseEntered = false;
            private bool _p32MouseButtonDown = false;
            private MouseButtons _p32MouseButton = MouseButtons.None;
            private int _p32FadeValue = 255;
            private bool _p32Disabled = false;
            private bool _p32Mox = false;
            private bool _p32Visible = true;

            public virtual bool Visible { get { return _p32Visible; } set { _p32Visible = value; } }
            public virtual bool MOX { get { return _p32Mox; } set { _p32Mox = value; } }
            public virtual int FadeValue { get { return _p32FadeValue; } set { _p32FadeValue = value; } }
            public virtual bool Disabled { get { return _p32Disabled; } set { _p32Disabled = value; } }
            public virtual void MouseClick(MouseEventArgs e) { }
            public virtual void MouseDown(MouseEventArgs e) { }
            public virtual void MouseUp(MouseEventArgs e) { }
            public virtual MouseButtons MouseButton { get { return _p32MouseButton; } set { _p32MouseButton = value; } }
            public virtual bool MouseButtonDown { get { return _p32MouseButtonDown; } set { _p32MouseButtonDown = value; } }
            public virtual PointF MouseDownPoint { get { return _p32MouseDownPoint; } set { _p32MouseDownPoint = value; } }
            public virtual PointF MouseUpPoint { get { return _p32MouseUpPoint; } set { _p32MouseUpPoint = value; } }
            public virtual PointF MouseMovePoint { get { return _p32MouseMovePoint; } set { _p32MouseMovePoint = value; } }
            public virtual bool MouseEntered { get { return _p32MouseEntered; } set { _p32MouseEntered = value; } }
            public virtual void KeyDown(Keys keycode) { }
            public virtual void KeyUp(Keys keycode) { }
            public virtual void MouseWheel(int number_of_moves) { }
            public virtual void Removing() { }
            public virtual void BandPanelsChanged(bool gen, bool hf, bool vhf) { }
            public virtual void BandChanged(Band oldBand, Band newBand) { }
            public virtual void ModeChanged(DSPMode oldMode, DSPMode newMode) { }
            public virtual void TuneStepIndexChanged(int old_index, int new_index) { }
        }

        public partial class clsMeter
        {
            private double _p32VfoA;
            private double _p32VfoB;
            private double _p32VfoSub;
            private DSPMode _p32ModeVfoA;
            private DSPMode _p32ModeVfoB;
            private Band _p32BandVfoA;
            private Band _p32BandVfoB;
            private Band _p32BandVfoASub;
            private Filter _p32FilterVfoA;
            private Filter _p32FilterVfoB;
            private string _p32FilterVfoAName="";
            private string _p32FilterVfoBName="";
            private string _p32VfoABandText="";
            private string _p32VfoBBandText="";
            private bool _p32VfoALock;
            private bool _p32VfoBLock;
            private bool _p32VfoSync;
            private bool _p32Split;
            private bool _p32TxVfoB;
            private bool _p32Rx2Enabled;
            private bool _p32MultiRxEnabled;
            private bool _p32QuickSplitEnabled;
            private int _p32TuneStepIndex;

            internal object P32MeterItemsLock { get { return _objMeterItemLock; } }
            public bool VFOSync { get { return _p32VfoSync; } set { _p32VfoSync=value; } }
            public bool VFOALock { get { return _p32VfoALock; } set { _p32VfoALock=value; } }
            public bool VFOBLock { get { return _p32VfoBLock; } set { _p32VfoBLock=value; } }
            public bool Split { get { return _p32Split; } set { _p32Split=value; } }
            public bool TXVFOb { get { return _p32TxVfoB; } set { _p32TxVfoB=value; } }
            public int TuneStepIndex { get { return _p32TuneStepIndex; } set { _p32TuneStepIndex=value; } }
            public double VfoA { get { return _p32VfoA; } set { _p32VfoA=value; } }
            public double VfoB { get { return _p32VfoB; } set { _p32VfoB=value; } }
            public double VfoSub { get { return _p32VfoSub; } set { _p32VfoSub=value; } }
            public DSPMode ModeVfoA { get { return _p32ModeVfoA; } set { _p32ModeVfoA=value; } }
            public DSPMode ModeVfoB { get { return _p32ModeVfoB; } set { _p32ModeVfoB=value; } }
            public Band BandVfoA { get { return _p32BandVfoA; } set { _p32BandVfoA=value; P32UpdateBandText(true); } }
            public Band BandVfoB { get { return _p32BandVfoB; } set { _p32BandVfoB=value; P32UpdateBandText(false); } }
            public Band BandVfoASub { get { return _p32BandVfoASub; } set { _p32BandVfoASub=value; } }
            public Filter FilterVfoA { get { return _p32FilterVfoA; } set { _p32FilterVfoA=value; } }
            public Filter FilterVfoB { get { return _p32FilterVfoB; } set { _p32FilterVfoB=value; } }
            public string FilterVfoAName { get { return _p32FilterVfoAName; } set { _p32FilterVfoAName=value ?? ""; } }
            public string FilterVfoBName { get { return _p32FilterVfoBName; } set { _p32FilterVfoBName=value ?? ""; } }
            public string VFOABandText { get { return _p32VfoABandText; } }
            public string VFOBBandText { get { return _p32VfoBBandText; } }
            public bool RX2Enabled { get { return _p32Rx2Enabled; } set { _p32Rx2Enabled=value; } }
            public bool MultiRxEnabled { get { return _p32MultiRxEnabled; } set { _p32MultiRxEnabled=value; } }
            public bool QuickSplitEnabled { get { return _p32QuickSplitEnabled; } set { _p32QuickSplitEnabled=value; } }

            private void P32UpdateBandText(bool vfoA)
            {
                string s=P32ThetisBandStackManager.BandToString(vfoA ? _p32BandVfoA : _p32BandVfoB);
                if(vfoA) _p32VfoABandText=s; else _p32VfoBBandText=s;
            }

            public BandGroups GetBandGroupFromBand(Band b)
            {
                switch (b)
                {
                    case Band.B160M: case Band.B80M: case Band.B60M: case Band.B40M:
                    case Band.B30M: case Band.B20M: case Band.B17M: case Band.B15M:
                    case Band.B12M: case Band.B10M: case Band.B6M: case Band.B2M:
                    case Band.WWV: return BandGroups.HF;
                    case Band.BLMF: case Band.B120M: case Band.B90M: case Band.B61M:
                    case Band.B49M: case Band.B41M: case Band.B31M: case Band.B25M:
                    case Band.B22M: case Band.B19M: case Band.B16M: case Band.B14M:
                    case Band.B13M: case Band.B11M: return BandGroups.GEN;
                    case Band.VHF0: case Band.VHF1: case Band.VHF2: case Band.VHF3:
                    case Band.VHF4: case Band.VHF5: case Band.VHF6: case Band.VHF7:
                    case Band.VHF8: case Band.VHF9: case Band.VHF10: case Band.VHF11:
                    case Band.VHF12: case Band.VHF13: return BandGroups.VHF;
                    default: return BandGroups.GEN;
                }
            }
            public void SetBandPanel(Console c, int rx, bool gen, bool hf, bool vhf)
            {
                if (c == null || rx > 1) return;
                if (gen) { hf=false; vhf=false; }
                else if (hf) { gen=false; vhf=false; }
                else if (vhf) { gen=false; hf=false; }
                _console.BeginInvoke(new MethodInvoker(delegate {
                    if (gen && !c.BandGENSelected) c.BandGENSelected=true;
                    else if (hf && !c.BandHFSelected) c.BandHFSelected=true;
                    else if (vhf && !c.BandVHFSelected) c.BandVHFSelected=true;
                }));
            }

$getBounds

$getFadeCover

$addBand

$addMode

$addTune

$addVfo

            public void P32KeyDown(Keys keycode)
            {
                lock (P32MeterItemsLock)
                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems) kvp.Value.KeyDown(keycode);
            }

            public void P32BandChanged(Band oldBand, Band newBand)
            {
                lock (P32MeterItemsLock)
                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems) kvp.Value.BandChanged(oldBand,newBand);
            }
            public void P32ModeChanged(DSPMode oldMode, DSPMode newMode)
            {
                lock (P32MeterItemsLock)
                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems) kvp.Value.ModeChanged(oldMode,newMode);
            }
            public void P32TuneStepChanged(int oldIndex, int newIndex)
            {
                lock (P32MeterItemsLock)
                    foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems) kvp.Value.TuneStepIndexChanged(oldIndex,newIndex);
            }
        }

        public static bool P32IsOnTop(string id)
        {
            if(_lstUCMeters==null || !_lstUCMeters.ContainsKey(id)) return false;
            return _lstUCMeters[id].PinOnTop;
        }

        public static void P32RefreshNativeState()
        {
            if(_console==null) return;
            lock(_metersLock)
            {
                foreach(KeyValuePair<string,clsMeter> kvp in _meters)
                {
                    clsMeter m=kvp.Value;
                    DSPMode oldMode=m.ModeVfoA;
                    Band oldBand=m.BandVfoA;
                    int oldStep=m.TuneStepIndex;

                    m.MOX=_console.MOX;
                    m.Split=_console.VFOSplit;
                    m.TXVFOb=_console.VFOBTX;
                    m.RX2Enabled=false;
                    m.MultiRxEnabled=_console.P32MultiRxEnabled;
                    m.VFOSync=_console.VFOSync;
                    m.VFOALock=_console.P32VFOALock;
                    m.VFOBLock=_console.P32VFOBLock;
                    m.QuickSplitEnabled=false;

                    m.VfoA=_console.VFOAFreq;
                    m.VfoB=_console.VFOBFreq;
                    m.VfoSub=_console.VFOASubFreq;
                    m.ModeVfoA=_console.RX1DSPMode;
                    m.ModeVfoB=_console.RX1DSPMode;
                    m.BandVfoA=_console.RX1Band;
                    m.BandVfoB=_console.P32BandForVFOB;
                    m.BandVfoASub=_console.RX1Band;
                    m.FilterVfoA=_console.RX1Filter;
                    m.FilterVfoB=_console.RX1Filter;
                    m.FilterVfoAName=_console.P32RX1FilterName;
                    m.FilterVfoBName=m.FilterVfoAName;
                    m.TuneStepIndex=_console.TuneStepIndex;

                    if(oldBand!=m.BandVfoA) m.P32BandChanged(oldBand,m.BandVfoA);
                    if(oldMode!=m.ModeVfoA) m.P32ModeChanged(oldMode,m.ModeVfoA);
                    if(oldStep!=m.TuneStepIndex) m.P32TuneStepChanged(oldStep,m.TuneStepIndex);
                }
            }
        }

        public static void P32GlobalKeyDown(Keys keycode)
        {
            lock(_metersLock)
                foreach(KeyValuePair<string,clsMeter> kvp in _meters) kvp.Value.P32KeyDown(keycode);
        }

        private partial class DXRenderer
        {
            private Queue<string> _stringMeasureKeys = new Queue<string>();

$measureMethod

$fadeMethod

$renderBlock

$mouseBlock
        }
    }

    internal static class P32ThetisBandStackManager
    {
$bandColour

$bandString
    }

    public static partial class Common
    {
$luminance
    }
}
"@

# One mechanical replacement is required because current Thetis calls the current
# MeterManager IsOnTop API, while the pinned 2023 core does not expose it.
$generated=$generated.Replace('MeterManager.IsOnTop(','MeterManager.P32IsOnTop(')
[IO.File]::WriteAllText($outPath,$generated,$utf8)

# PowerSDR API adapter. This file contains no meter rendering or visual policy.
$adapter=@"
using System;
using System.Windows.Forms;

namespace PowerSDR
{
    sealed unsafe public partial class Console
    {
        public bool BandHFSelected
        {
            get { return panelBandHF.Visible; }
            set { if(value) btnBandHF_Click(this, EventArgs.Empty); }
        }
        public bool BandVHFSelected
        {
            get { return panelBandVHF.Visible; }
            set { if(value) btnBandVHF_Click(this, EventArgs.Empty); }
        }
        public bool BandGENSelected
        {
            get { return panelBandGN.Visible; }
            set { if(value) btnBandGEN_Click(this, EventArgs.Empty); }
        }

        public bool P32VFOALock { get { return VFOLock; } set { VFOLock=value; } }
        public bool P32VFOBLock { get { return VFOLockB; } set { VFOLockB=value; } }
        public bool P32MultiRxEnabled { get { return chkEnableMultiRX.Checked; } }
        public Band P32BandForVFOB { get { return BandByFreq(VFOBFreq, -1, false, current_region); } }
        public string P32RX1FilterName
        {
            get
            {
                try { return rx1_filters[(int)RX1DSPMode].GetName(RX1Filter); }
                catch { return RX1Filter.ToString(); }
            }
        }

        public bool GetVHFEnabled(int index)
        {
            switch(index)
            {
                case 0:return radBandVHF0.Enabled; case 1:return radBandVHF1.Enabled;
                case 2:return radBandVHF2.Enabled; case 3:return radBandVHF3.Enabled;
                case 4:return radBandVHF4.Enabled; case 5:return radBandVHF5.Enabled;
                case 6:return radBandVHF6.Enabled; case 7:return radBandVHF7.Enabled;
                case 8:return radBandVHF8.Enabled; case 9:return radBandVHF9.Enabled;
                case 10:return radBandVHF10.Enabled; case 11:return radBandVHF11.Enabled;
                case 12:return radBandVHF12.Enabled; case 13:return radBandVHF13.Enabled;
                default:return false;
            }
        }
        public string GetVHFText(int index)
        {
            switch(index)
            {
                case 0:return radBandVHF0.Text; case 1:return radBandVHF1.Text;
                case 2:return radBandVHF2.Text; case 3:return radBandVHF3.Text;
                case 4:return radBandVHF4.Text; case 5:return radBandVHF5.Text;
                case 6:return radBandVHF6.Text; case 7:return radBandVHF7.Text;
                case 8:return radBandVHF8.Text; case 9:return radBandVHF9.Text;
                case 10:return radBandVHF10.Text; case 11:return radBandVHF11.Text;
                case 12:return radBandVHF12.Text; case 13:return radBandVHF13.Text;
                default:return "";
            }
        }

        public void SetupRX2Band(Band band, bool vfoBOnly)
        {
            // RX2 is intentionally not exposed in this target. In the exact Thetis
            // RX2-disabled path this operation changes the VFO-B frequency only.
            VFOBFreq=BandToFreq(band);
        }

        public void PopupBandstack(int rx, Band band, bool topMost)
        {
            // PowerSDR KE9NS has no Thetis bandstack-popup subsystem. This adapter is
            // intentionally empty rather than inventing a different UI/behaviour.
        }
        public void PopupFilterContextMenu(int rx, Control owner)
        {
            // The pinned P32 scope uses the 3dbd VFO implementation; no PowerSDR-native
            // equivalent of Thetis' filter popup exists. Do not substitute another UI.
        }
    }
}
"@
[IO.File]::WriteAllText($adapterPath,$adapter,$utf8)

# Core class surfaces.
$mm=[IO.File]::ReadAllText($mmPath)
$mm=$mm.Replace('internal static class MeterManager','internal static partial class MeterManager')
$mm=$mm.Replace('public class clsMeterItem','public partial class clsMeterItem')
$mm=$mm.Replace('public class clsMeter','public partial class clsMeter')
$mm=$mm.Replace('private class DXRenderer','private partial class DXRenderer')

if(!$mm.Contains('VFO_DISPLAY,')) {
    $mm=$mm.Replace('        CROSS,','        CROSS,'+$nl+'        VFO_DISPLAY,'+$nl+'        BAND_BUTTONS,'+$nl+'        MODE_BUTTONS,'+$nl+'        TUNESTEP_BUTTONS,')
}
if(!$mm.Contains('                VFO_DISPLAY,')) {
    $mm=$mm.Replace('                HISTORY,'+$nl+'                ITEM_GROUP','                HISTORY,'+$nl+'                VFO_DISPLAY,'+$nl+'                FADE_COVER,'+$nl+'                BAND_BUTTONS,'+$nl+'                MODE_BUTTONS,'+$nl+'                TUNESTEP_BUTTONS,'+$nl+'                ITEM_GROUP')
}

$crossAdd='                    case MeterType.CROSS: AddCrossNeedle(nDelay, 0, out bBottom, restoreIg); break;'
if(!$mm.Contains($crossAdd)){throw 'P32 AddMeter CROSS anchor missing'}
if(!$mm.Contains('case MeterType.VFO_DISPLAY: AddVFODisplay'))
{
    $mm=$mm.Replace($crossAdd,$crossAdd+$nl+
      '                    case MeterType.VFO_DISPLAY: AddVFODisplay(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
      '                    case MeterType.BAND_BUTTONS: AddBandButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
      '                    case MeterType.MODE_BUTTONS: AddModeButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
      '                    case MeterType.TUNESTEP_BUTTONS: AddTunestepButtons(nDelay, 0, out bBottom, restoreIg); break;')
}

$nameAnchor='                case MeterType.CROSS: return "Cross Meter";'
if(!$mm.Contains($nameAnchor)){throw 'P32 MeterName CROSS anchor missing'}
if(!$mm.Contains('case MeterType.VFO_DISPLAY: return "Vfo Display";'))
{
    $mm=$mm.Replace($nameAnchor,$nameAnchor+$nl+
      '                case MeterType.VFO_DISPLAY: return "Vfo Display";'+$nl+
      '                case MeterType.BAND_BUTTONS: return "Band Buttons";'+$nl+
      '                case MeterType.MODE_BUTTONS: return "Mode Buttons";'+$nl+
      '                case MeterType.TUNESTEP_BUTTONS: return "Tunestep Buttons";')
}

$typeAnchor='                case MeterType.CROSS: return 2;'
if(!$mm.Contains($typeAnchor)){throw 'P32 GetMeterTXRXType CROSS anchor missing'}
if(!$mm.Contains('case MeterType.VFO_DISPLAY: return 2;'))
{
    $mm=$mm.Replace($typeAnchor,$typeAnchor+$nl+
      '                case MeterType.VFO_DISPLAY: return 2;'+$nl+
      '                case MeterType.BAND_BUTTONS: return 2;'+$nl+
      '                case MeterType.MODE_BUTTONS: return 2;'+$nl+
      '                case MeterType.TUNESTEP_BUTTONS: return 2;')
}

# Replace the legacy text-measure implementation with the exact pinned Thetis implementation.
$oldMeasureStart=$mm.IndexOf('            private SizeF measureString(string sText, string sFontFamily, FontStyle style, float emSize)')
$oldMeasureEnd=$mm.IndexOf('            private void renderScale(',$oldMeasureStart)
if($oldMeasureStart -lt 0 -or $oldMeasureEnd -lt 0){throw 'P32 legacy measureString anchors missing'}
$mm=$mm.Remove($oldMeasureStart,$oldMeasureEnd-$oldMeasureStart)

# Exact Thetis renderer switch entries.
$renderAnchor='                                case clsMeterItem.MeterItemType.MAGIC_EYE:'+$nl+
              '                                    renderEye(rect, mi, m);'+$nl+
              '                                    break;'
if(!$mm.Contains($renderAnchor)){throw 'P32 renderer MAGIC_EYE anchor missing'}
$mm=$mm.Replace($renderAnchor,
              '                                case clsMeterItem.MeterItemType.VFO_DISPLAY:'+$nl+
              '                                    renderVfoDisplay(rect, mi, m);'+$nl+
              '                                    break;'+$nl+
              '                                case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:'+$nl+
              '                                case clsMeterItem.MeterItemType.MODE_BUTTONS:'+$nl+
              '                                case clsMeterItem.MeterItemType.BAND_BUTTONS:'+$nl+
              '                                    renderButtonBox(rect, mi, m);'+$nl+
              '                                    break;'+$nl+
              '                                case clsMeterItem.MeterItemType.FADE_COVER:'+$nl+
              '                                    renderFadeCover(rect, mi, m);'+$nl+
              '                                    break;'+$nl+
              $renderAnchor)

# The exact 3dbd mouse dispatcher supersedes the 2023 MouseUp-only dispatcher.
$oldEvent='                _displayTarget.MouseUp += OnMouseUp;'
if(!$mm.Contains($oldEvent)){throw 'P32 legacy MouseUp subscription missing'}
$newEvents=
'                _displayTarget.MouseUp += P32OnMouseUp;'+$nl+
'                _displayTarget.MouseDown += P32OnMouseDown;'+$nl+
'                _displayTarget.MouseWheel += P32OnMouseWheel;'+$nl+
'                _displayTarget.MouseMove += P32OnMouseMove;'+$nl+
'                _displayTarget.MouseLeave += P32OnMouseLeave;'+$nl+
'                _displayTarget.MouseEnter += P32OnMouseEnter;'+$nl+
'                _displayTarget.MouseClick += P32OnMouseClick;'
$mm=$mm.Replace($oldEvent,$newEvents)

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# Keep VFO/BAND/MODE/STEP state current using the existing P25 PowerSDR polling bridge.
$bridge=[IO.File]::ReadAllText($bridgePath)
$refreshAnchor='        internal void P25RefreshPowerSDR()'+$nl+'        {'
if(!$bridge.Contains($refreshAnchor)){throw 'P32 P25RefreshPowerSDR anchor missing'}
if(!$bridge.Contains('MeterManager.P32RefreshNativeState();'))
{
    $bridge=$bridge.Replace($refreshAnchor,$refreshAnchor+$nl+'            MeterManager.P32RefreshNativeState();')
}
[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

# Add generated exact source and API adapter.
$proj=[IO.File]::ReadAllText($projPath)
$anchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($anchor)){throw 'P32 csproj P30 anchor missing'}
if(!$proj.Contains('<Compile Include="P32ThetisExactGadgets.cs" />'))
{
    $proj=$proj.Replace($anchor,$anchor+$nl+
      '    <Compile Include="P32ThetisExactGadgets.cs" />'+$nl+
      '    <Compile Include="P32PowerSDRThetisAdapter.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Hard evidence gates: custom P31 implementation must not be present; exact pinned source tokens must.
$verify=[IO.File]::ReadAllText($outPath)
foreach($token in @(
    'public enum renderState','TUNE_STEP','VFODisplayMode','MouseDownLong',
    'class clsBandButtonBox','class clsModeButtonBox','class clsTunestepButtons',
    'private void renderVfoDisplay','private void renderButtonBox',
    'private clsVfoDisplay.buttonState drawBand','private clsVfoDisplay.buttonState drawMode',
    'private clsVfoDisplay.buttonState drawFilter','private clsVfoDisplay.buttonState drawTuneStep'
)){
    if(!$verify.Contains($token)){throw "P32 exact-source gate missing: $token"}
}
if(Test-Path (Join-Path $consoleDir 'P31ModernThetisGadgets.cs')){throw 'P32 rejected: P31 custom gadget source present'}

Write-Host "P32_THETIS_SOURCE_SHA=$ThetisSha"
Write-Host 'P32_IMPLEMENTATION=MECHANICALLY_EXTRACTED_THETIS_SOURCE'
Write-Host 'P32_GADGETS=VFO_DISPLAY,BAND_BUTTONS,MODE_BUTTONS,TUNESTEP_BUTTONS'
Write-Host 'P32_RENDERER=THETIS_EXACT_SOURCE_BLOCKS'
Write-Host 'P32_MOUSE=THETIS_EXACT_SOURCE_DISPATCH'
Write-Host 'P32_RX2=NOT_EXPOSED_BY_PROJECT_REQUIREMENT'
Write-Host 'P32_UNAVAILABLE_NATIVE_UI=THETIS_BANDSTACK_POPUP,THETIS_FILTER_CONTEXT_POPUP'
