[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P32: Thetis is the sole behavioural/rendering source for the modern Meters/Gadgets.
# This layer does not redraw or reinterpret VFO/BAND/MODE/TUNESTEP. It mechanically
# imports the exact Thetis implementation pinned below and supplies only PowerSDR API adapters.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P30.ps1') -SourceRoot $SourceRoot

$ThetisSha='6388c61d3c484eb7f72b0f03d0ec4cab14058c6e'
$RawMeterManager="https://raw.githubusercontent.com/ramdor/Thetis/$ThetisSha/Project%20Files/Source/Console/MeterManager.cs"
$consoleDir=Join-Path $SourceRoot 'Console'
$mmPath=Join-Path $consoleDir 'P25_MeterManager.cs'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$dstExact=Join-Path $consoleDir 'P32ThetisExactGadgets.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

if(!(Test-Path $mmPath)){throw "P32 MeterManager missing: $mmPath"}

$tmp=Join-Path $env:TEMP 'P32_Thetis_MeterManager.cs'
Invoke-WebRequest -UseBasicParsing -Uri $RawMeterManager -OutFile $tmp
$thetis=[IO.File]::ReadAllText($tmp)
if(!$thetis.Contains('internal class clsVfoDisplay : clsMeterItem')){throw 'P32 pinned Thetis source lacks clsVfoDisplay'}
if(!$thetis.Contains('internal class clsBandButtonBox : clsButtonBox')){throw 'P32 pinned Thetis source lacks clsBandButtonBox'}
if(!$thetis.Contains('internal class clsModeButtonBox : clsButtonBox')){throw 'P32 pinned Thetis source lacks clsModeButtonBox'}
if(!$thetis.Contains('internal class clsTunestepButtons : clsButtonBox')){throw 'P32 pinned Thetis source lacks clsTunestepButtons'}

function Get-BracedBlock([string]$text,[string]$marker)
{
    $start=$text.IndexOf($marker,[StringComparison]::Ordinal)
    if($start -lt 0){throw "P32 source marker missing: $marker"}
    $brace=$text.IndexOf('{',$start)
    if($brace -lt 0){throw "P32 opening brace missing: $marker"}
    $depth=0
    for($i=$brace;$i -lt $text.Length;$i++)
    {
        $ch=$text[$i]
        if($ch -eq '{'){$depth++}
        elseif($ch -eq '}')
        {
            $depth--
            if($depth -eq 0){return $text.Substring($start,$i-$start+1)}
        }
    }
    throw "P32 closing brace missing: $marker"
}

# Exact Thetis 2024-09-07 implementation blocks. No hand-authored substitutes.
$clsButton=Get-BracedBlock $thetis 'internal class clsButtonBox : clsMeterItem'
$clsBand=Get-BracedBlock $thetis 'internal class clsBandButtonBox : clsButtonBox'
$clsMode=Get-BracedBlock $thetis 'internal class clsModeButtonBox : clsButtonBox'
$clsStep=Get-BracedBlock $thetis 'internal class clsTunestepButtons : clsButtonBox'
$clsVfo=Get-BracedBlock $thetis 'internal class clsVfoDisplay : clsMeterItem'

$addBand=Get-BracedBlock $thetis 'public string AddBandButtons('
$addMode=Get-BracedBlock $thetis 'public string AddModeButtons('
$addStep=Get-BracedBlock $thetis 'public string AddTunestepButtons('
$addVfo=Get-BracedBlock $thetis 'public string AddVFODisplay('

$renderVfo=Get-BracedBlock $thetis 'private void renderVfoDisplay('
$renderButton=Get-BracedBlock $thetis 'private void renderButtonBox('
$drawBand=Get-BracedBlock $thetis 'private clsVfoDisplay.buttonState drawBand('
$drawMode=Get-BracedBlock $thetis 'private clsVfoDisplay.buttonState drawMode('
$drawFilter=Get-BracedBlock $thetis 'private clsVfoDisplay.buttonState drawFilter('
$drawStep=Get-BracedBlock $thetis 'private clsVfoDisplay.buttonState drawTuneStep('
$getParts=Get-BracedBlock $thetis 'private void getParts('
$plotText=Get-BracedBlock $thetis 'private (float, float) plotText('
$shrinkRect=Get-BracedBlock $thetis 'private SharpDX.RectangleF shrinkRectangle('
$drawRounded=Get-BracedBlock $thetis 'private void drawRoundedRectangle('
$fillRounded=Get-BracedBlock $thetis 'private void fillRoundedRectangle('
$drawSafe=Get-BracedBlock $thetis 'private void drawSafeLine('
$adjustContrast=Get-BracedBlock $thetis 'private System.Drawing.Color adjustTextColourForContrast('
$contrast=Get-BracedBlock $thetis 'private double calculateContrastRatio('

$getBandGroup=Get-BracedBlock $thetis 'public BandGroups GetBandGroupFromBand('
$setBandPanel=Get-BracedBlock $thetis 'public void SetBandPanel('

# Mechanical namespace/API-name adaptations only. Observable Thetis gadget logic is retained.
foreach($v in @('clsBand','clsVfo'))
{
    $value=Get-Variable $v -ValueOnly
    $value=$value.Replace('BandStackManager.','P32ThetisBandStack.')
    $value=$value.Replace('_console.BandPreChangeHandlers?.Invoke(1, band);','_console.P32SetRX1Band(band);')
    $value=$value.Replace('_console.SetupRX2Band(band);','_console.P32SetVFOBBand(band);')
    $value=$value.Replace('_console.PopupBandstack(_owningmeter.RX, b, MeterManager.IsOnTop(_owningmeter.ID));','_console.P32PopupBandstack(_owningmeter.RX, b);')
    Set-Variable $v $value
}
$clsVfo=$clsVfo.Replace('_console.PopupBandstack(_owningmeter.RX, _owningmeter.BandVfoA, MeterManager.IsOnTop(_owningmeter.ID));','_console.P32PopupBandstack(_owningmeter.RX, _owningmeter.BandVfoA);')
$clsVfo=$clsVfo.Replace('_console.PopupFilterContextMenu(_owningmeter.RX, null);','_console.P32PopupFilterMenu(_owningmeter.RX);')

$setBandPanel=$setBandPanel.Replace('c.BandGENSelected = true;','c.P32SelectBandPanel(BandGroups.GEN);')
$setBandPanel=$setBandPanel.Replace('c.BandHFSelected = true;','c.P32SelectBandPanel(BandGroups.HF);')
$setBandPanel=$setBandPanel.Replace('c.BandVHFSelected = true;','c.P32SelectBandPanel(BandGroups.VHF);')

# Convert the old direct-core classes to partial so exact Thetis blocks can be attached cleanly.
$mm=[IO.File]::ReadAllText($mmPath)
$mm=$mm.Replace('internal static class MeterManager','internal static partial class MeterManager')
$mm=$mm.Replace('public class clsMeterItem','public partial class clsMeterItem')
$mm=$mm.Replace('public class clsMeter','public partial class clsMeter')
$mm=$mm.Replace('private class DXRenderer','private partial class DXRenderer')

# Robust enum insertion. This deliberately does not depend on CRLF shape.
if($mm -notmatch '(?m)^\s*VFO_DISPLAY,\s*$')
{
    $rx=[regex]'(?m)^(?<indent>\s*)CROSS,\s*
}
if($mm -notmatch '(?m)^\s*VFO_DISPLAY,\s*$' -or $mm -notmatch '(?m)^\s*TUNESTEP_BUTTONS,\s*$')
{
    throw 'P32 MeterType insertion failed'
}

# MeterItemType: insert the exact modern item kinds after ITEM_GROUP.
$miStart=$mm.IndexOf('public enum MeterItemType')
if($miStart -lt 0){throw 'P32 MeterItemType enum missing'}
$miEnd=$mm.IndexOf('}',$miStart)
if($miEnd -lt 0){throw 'P32 MeterItemType enum close missing'}
$miBlock=$mm.Substring($miStart,$miEnd-$miStart)
if($miBlock -notmatch 'VFO_DISPLAY')
{
    $rxItem=[regex]'(?m)^(?<indent>\s*)ITEM_GROUP\s*,?\s*
}

# Exact AddMeter routes. P30 core has CROSS as last special case.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY:')
{
    $anchor='                    case MeterType.CROSS: AddCross(nMSupdate, fTop, out fBottom, restoreIg); break;'
    if(!$mm.Contains($anchor)){throw 'P32 AddMeter CROSS anchor missing'}
    $insert=@'
                    case MeterType.VFO_DISPLAY: AddVFODisplay(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.BAND_BUTTONS: AddBandButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.MODE_BUTTONS: AddModeButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.TUNESTEP_BUTTONS: AddTunestepButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
'@
    $mm=$mm.Replace($anchor,$anchor+$nl+$insert.TrimEnd())
}

# Names and RX/TX classification are Thetis names.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return "Vfo Display";')
{
    $nameAnchor='                case MeterType.CROSS: return "Cross Meter";'
    if(!$mm.Contains($nameAnchor)){throw 'P32 MeterName CROSS anchor missing'}
    $nameInsert=@'
                case MeterType.VFO_DISPLAY: return "Vfo Display";
                case MeterType.BAND_BUTTONS: return "Band Buttons";
                case MeterType.MODE_BUTTONS: return "Mode Buttons";
                case MeterType.TUNESTEP_BUTTONS: return "Tunestep Buttons";
'@
    $mm=$mm.Replace($nameAnchor,$nameAnchor+$nl+$nameInsert.TrimEnd())
}
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return 2;')
{
    $typeAnchor='                case MeterType.CROSS: return 2;'
    if(!$mm.Contains($typeAnchor)){throw 'P32 TXRX CROSS anchor missing'}
    $typeInsert=@'
                case MeterType.VFO_DISPLAY: return 2;
                case MeterType.BAND_BUTTONS: return 2;
                case MeterType.MODE_BUTTONS: return 2;
                case MeterType.TUNESTEP_BUTTONS: return 2;
'@
    $mm=$mm.Replace($typeAnchor,$typeAnchor+$nl+$typeInsert.TrimEnd())
}

# Exact renderer routing.
if($mm -notmatch 'case clsMeterItem\.MeterItemType\.VFO_DISPLAY:')
{
    $renderAnchor='                            case clsMeterItem.MeterItemType.MAGIC_EYE:'
    if(!$mm.Contains($renderAnchor)){throw 'P32 renderer MAGIC_EYE anchor missing'}
    $renderCases=@'
                            case clsMeterItem.MeterItemType.VFO_DISPLAY:
                                renderVfoDisplay(rect, mi, m);
                                break;
                            case clsMeterItem.MeterItemType.BAND_BUTTONS:
                            case clsMeterItem.MeterItemType.MODE_BUTTONS:
                            case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:
                                renderButtonBox(rect, mi, m);
                                break;
'@
    $mm=$mm.Replace($renderAnchor,$renderCases.TrimEnd()+$nl+$renderAnchor)
}

# Feed the exact Thetis state snapshot immediately after the existing PowerSDR telemetry refresh.
if(!$mm.Contains('P32RefreshExactGadgetState();'))
{
    $loopAnchor='    P25RefreshPowerSDR();'
    if(!$mm.Contains($loopAnchor)){throw 'P32 P25 refresh anchor missing'}
    $mm=$mm.Replace($loopAnchor,$loopAnchor+$nl+'    P32RefreshExactGadgetState();')
}

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# The exact 2024 classes expect the modern mouse/key callbacks. Add them to the old 2023 base
# with the same neutral virtual behaviour, then route WinForms events to them.
$source=@"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace PowerSDR
{
    internal static partial class MeterManager
    {
        public enum BandGroups { GEN = 0, HF, VHF, LAST = 99 }

        public partial class clsMeterItem
        {
            private MouseButtons _p32MouseButton;
            private bool _p32MouseButtonDown;
            private PointF _p32MouseDownPoint;
            private PointF _p32MouseUpPoint;
            private PointF _p32MouseMovePoint;
            private bool _p32MouseEntered;

            public virtual void MouseDown(MouseEventArgs e) { }
            public virtual void MouseUp(MouseEventArgs e) { }
            public virtual void MouseWheel(int number_of_moves) { }
            public virtual void KeyDown(Keys keycode) { }
            public virtual void KeyUp(Keys keycode) { }
            public virtual MouseButtons MouseButton { get { return _p32MouseButton; } set { _p32MouseButton = value; } }
            public virtual bool MouseButtonDown { get { return _p32MouseButtonDown; } set { _p32MouseButtonDown = value; } }
            public virtual PointF MouseDownPoint { get { return _p32MouseDownPoint; } set { _p32MouseDownPoint = value; } }
            public virtual PointF MouseUpPoint { get { return _p32MouseUpPoint; } set { _p32MouseUpPoint = value; } }
            public virtual PointF MouseMovePoint { get { return _p32MouseMovePoint; } set { _p32MouseMovePoint = value; } }
            public virtual bool MouseEntered { get { return _p32MouseEntered; } set { _p32MouseEntered = value; } }
            public virtual bool ClickHighlight { get { return false; } set { } }
            public virtual void BandPanelsChanged(bool gen, bool hf, bool vhf) { }
            public virtual void BandChanged(Band oldBand, Band newBand) { }
            public virtual void ModeChanged(DSPMode oldMode, DSPMode newMode) { }
            public virtual void TuneStepIndexChanged(int old_index, int new_index) { }
        }

        $clsButton

        $clsBand

        $clsMode

        $clsStep

        $clsVfo

        public partial class clsMeter
        {
            private double _p32VfoA, _p32VfoB, _p32VfoSub;
            private DSPMode _p32ModeA, _p32ModeB;
            private Band _p32BandA, _p32BandB, _p32BandSub;
            private Filter _p32FilterA, _p32FilterB;
            private string _p32FilterAName="", _p32FilterBName="";
            private string _p32BandAText="", _p32BandBText="";
            private bool _p32RX2, _p32Multi, _p32Split, _p32TXVfoB, _p32LockA, _p32LockB, _p32Sync, _p32QuickSplit;
            private int _p32TuneStep;

            public double VfoA { get { return _p32VfoA; } set { _p32VfoA=value; } }
            public double VfoB { get { return _p32VfoB; } set { _p32VfoB=value; } }
            public double VfoSub { get { return _p32VfoSub; } set { _p32VfoSub=value; } }
            public DSPMode ModeVfoA { get { return _p32ModeA; } set { _p32ModeA=value; } }
            public DSPMode ModeVfoB { get { return _p32ModeB; } set { _p32ModeB=value; } }
            public Band BandVfoA { get { return _p32BandA; } set { _p32BandA=value; } }
            public Band BandVfoB { get { return _p32BandB; } set { _p32BandB=value; } }
            public Band BandVfoASub { get { return _p32BandSub; } set { _p32BandSub=value; } }
            public Filter FilterVfoA { get { return _p32FilterA; } set { _p32FilterA=value; } }
            public Filter FilterVfoB { get { return _p32FilterB; } set { _p32FilterB=value; } }
            public string FilterVfoAName { get { return _p32FilterAName; } set { _p32FilterAName=value ?? ""; } }
            public string FilterVfoBName { get { return _p32FilterBName; } set { _p32FilterBName=value ?? ""; } }
            public string VFOABandText { get { return _p32BandAText; } set { _p32BandAText=value ?? ""; } }
            public string VFOBBandText { get { return _p32BandBText; } set { _p32BandBText=value ?? ""; } }
            public bool RX2Enabled { get { return _p32RX2; } set { _p32RX2=value; } }
            public bool MultiRxEnabled { get { return _p32Multi; } set { _p32Multi=value; } }
            public bool Split { get { return _p32Split; } set { _p32Split=value; } }
            public bool TXVFOb { get { return _p32TXVfoB; } set { _p32TXVfoB=value; } }
            public bool VFOALock { get { return _p32LockA; } set { _p32LockA=value; } }
            public bool VFOBLock { get { return _p32LockB; } set { _p32LockB=value; } }
            public bool VFOSync { get { return _p32Sync; } set { _p32Sync=value; } }
            public bool QuickSplitEnabled { get { return _p32QuickSplit; } set { _p32QuickSplit=value; } }
            public int TuneStepIndex { get { return _p32TuneStep; } set { _p32TuneStep=value; } }

            $getBandGroup

            $setBandPanel

            $addBand

            $addMode

            $addStep

            $addVfo
        }

        private static string P32FilterName(Filter f)
        {
            return f.ToString();
        }

        private static void P32RefreshExactGadgetState()
        {
            if (_console == null) return;
            lock (_metersLock)
            {
                foreach (KeyValuePair<string, clsMeter> kvp in _meters)
                {
                    clsMeter m=kvp.Value;
                    if (m.RX != 1) continue; // RX2 intentionally not exposed in this FLEX-5000 target.
                    Band oldBand=m.BandVfoA;
                    DSPMode oldMode=m.ModeVfoA;
                    int oldStep=m.TuneStepIndex;

                    m.MOX=_console.MOX;
                    m.Split=_console.VFOSplit;
                    m.TXVFOb=_console.VFOBTX;
                    m.RX2Enabled=false;
                    m.MultiRxEnabled=false;
                    m.VfoA=_console.VFOAFreq;
                    m.VfoB=_console.VFOBFreq;
                    m.VfoSub=_console.VFOASubFreq;
                    m.ModeVfoA=_console.RX1DSPMode;
                    m.ModeVfoB=_console.RX1DSPMode;
                    m.BandVfoA=_console.RX1Band;
                    m.BandVfoB=_console.P32BandFromFrequency(_console.VFOBFreq);
                    m.BandVfoASub=_console.RX1Band;
                    m.FilterVfoA=_console.RX1Filter;
                    m.FilterVfoB=_console.RX1Filter;
                    m.FilterVfoAName=P32FilterName(_console.RX1Filter);
                    m.FilterVfoBName=P32FilterName(_console.RX1Filter);
                    m.VFOALock=_console.VFOALock;
                    m.VFOBLock=_console.VFOBLock;
                    m.VFOSync=_console.VFOSync;
                    m.QuickSplitEnabled=false;
                    m.TuneStepIndex=_console.TuneStepIndex;
                    m.VFOABandText=P32ThetisBandStack.BandToString(m.BandVfoA);
                    m.VFOBBandText=P32ThetisBandStack.BandToString(m.BandVfoB);

                    if(oldBand != m.BandVfoA)
                        foreach(var item in m.MeterItems.Values) item.BandChanged(oldBand,m.BandVfoA);
                    if(oldMode != m.ModeVfoA)
                        foreach(var item in m.MeterItems.Values) item.ModeChanged(oldMode,m.ModeVfoA);
                    if(oldStep != m.TuneStepIndex)
                        foreach(var item in m.MeterItems.Values) item.TuneStepIndexChanged(oldStep,m.TuneStepIndex);
                }
            }
        }

        private partial class DXRenderer
        {
            private Dictionary<string, BitmapBrush> _bitmap_brushes = new Dictionary<string, BitmapBrush>();

            $getParts

            $plotText

            $shrinkRect

            $drawRounded

            $fillRounded

            $drawSafe

            $contrast

            $adjustContrast

            $drawBand

            $drawMode

            $drawFilter

            $drawStep

            $renderVfo

            $renderButton
        }
    }

    internal static class P32ThetisBandStack
    {
        internal static string BandToString(Band b)
        {
            switch(b)
            {
                case Band.GEN: return "GEN";
                case Band.B160M: return "160M"; case Band.B80M: return "80M"; case Band.B60M: return "60M";
                case Band.B40M: return "40M"; case Band.B30M: return "30M"; case Band.B20M: return "20M";
                case Band.B17M: return "17M"; case Band.B15M: return "15M"; case Band.B12M: return "12M";
                case Band.B10M: return "10M"; case Band.B6M: return "6M"; case Band.B2M: return "2M";
                case Band.WWV: return "WWV"; case Band.BLMF: return "LMF";
                case Band.B120M: return "120M"; case Band.B90M: return "90M"; case Band.B61M: return "61M";
                case Band.B49M: return "49M"; case Band.B41M: return "41M"; case Band.B31M: return "31M";
                case Band.B25M: return "25M"; case Band.B22M: return "22M"; case Band.B19M: return "19M";
                case Band.B16M: return "16M"; case Band.B14M: return "14M"; case Band.B13M: return "13M";
                case Band.B11M: return "11M";
                case Band.VHF0: return "VHF0"; case Band.VHF1: return "VHF1"; case Band.VHF2: return "VHF2";
                case Band.VHF3: return "VHF3"; case Band.VHF4: return "VHF4"; case Band.VHF5: return "VHF5";
                case Band.VHF6: return "VHF6"; case Band.VHF7: return "VHF7"; case Band.VHF8: return "VHF8";
                case Band.VHF9: return "VHF9"; case Band.VHF10: return "VHF10"; case Band.VHF11: return "VHF11";
                case Band.VHF12: return "VHF12"; case Band.VHF13: return "VHF13";
                default: return "GEN";
            }
        }

        internal static Color BandToColour(Band b)
        {
            if(b==Band.WWV) return Color.Green;
            if(b>=Band.B120M && b<=Band.B11M) return Color.Coral;
            if(b>=Band.VHF0 && b<=Band.VHF13) return Color.Gold;
            return Color.White;
        }
    }

    sealed unsafe public partial class Console
    {
        private MeterManager.BandGroups _p32BandPanel = MeterManager.BandGroups.LAST;

        public bool BandGENSelected { get { return P32BandPanel == MeterManager.BandGroups.GEN; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.GEN); } }
        public bool BandHFSelected { get { return P32BandPanel == MeterManager.BandGroups.HF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.HF); } }
        public bool BandVHFSelected { get { return P32BandPanel == MeterManager.BandGroups.VHF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.VHF); } }

        private MeterManager.BandGroups P32BandPanel
        {
            get
            {
                if(_p32BandPanel==MeterManager.BandGroups.LAST)
                {
                    Band b=RX1Band;
                    if(b>=Band.B160M && b<=Band.B2M || b==Band.WWV) _p32BandPanel=MeterManager.BandGroups.HF;
                    else if(b>=Band.VHF0 && b<=Band.VHF13) _p32BandPanel=MeterManager.BandGroups.VHF;
                    else _p32BandPanel=MeterManager.BandGroups.GEN;
                }
                return _p32BandPanel;
            }
        }

        public void P32SelectBandPanel(MeterManager.BandGroups group) { _p32BandPanel=group; }

        public bool VFOALock
        {
            get { return (CATVFOLockAB & 1) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 1) : (CATVFOLockAB & ~1); }
        }
        public bool VFOBLock
        {
            get { return (CATVFOLockAB & 2) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 2) : (CATVFOLockAB & ~2); }
        }

        public void P32SetRX1Band(Band band) { RX1Band=band; }

        public void P32SetVFOBBand(Band band)
        {
            // PowerSDR exposes RX2Band as its native VFO-B band path. This is the direct API mapping.
            RX2Band=band;
        }

        public Band P32BandFromFrequency(double mhz)
        {
            if(mhz>=1.8 && mhz<2.0) return Band.B160M;
            if(mhz>=3.5 && mhz<4.0) return Band.B80M;
            if(mhz>=5.0 && mhz<5.6) return Band.B60M;
            if(mhz>=7.0 && mhz<7.4) return Band.B40M;
            if(mhz>=10.0 && mhz<10.3) return Band.B30M;
            if(mhz>=14.0 && mhz<14.5) return Band.B20M;
            if(mhz>=18.0 && mhz<18.3) return Band.B17M;
            if(mhz>=21.0 && mhz<21.6) return Band.B15M;
            if(mhz>=24.8 && mhz<25.1) return Band.B12M;
            if(mhz>=28.0 && mhz<30.0) return Band.B10M;
            if(mhz>=50.0 && mhz<54.0) return Band.B6M;
            return RX1Band;
        }

        public bool GetVHFEnabled(int index) { return false; }
        public string GetVHFText(int index) { return "VHF"+index.ToString(); }

        public void P32PopupBandstack(int rx, Band band)
        {
            if(StackForm != null)
            {
                StackForm.Show();
                StackForm.BringToFront();
            }
        }

        public void P32PopupFilterMenu(int rx)
        {
            // PowerSDR has no Thetis filter popup API; the exact VFO renderer remains unchanged.
            // This adapter intentionally performs no substitute UI action.
        }
    }
}
"@

# Mechanical source adaptation for the old SharpDX renderer API.
$source=$source.Replace('private (float, float) plotText(','private Tuple<float,float> plotText(')
$source=$source.Replace('return (0, 0);','return Tuple.Create(0f,0f);')
$source=[regex]::Replace($source,'return \(szTextSize\.Width, szTextSize\.Height\);','return Tuple.Create(szTextSize.Width,szTextSize.Height);')

[IO.File]::WriteAllText($dstExact,$source,$utf8)

# Wire source into the project.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P32 csproj P30 anchor missing'}
if(!$proj.Contains('<Compile Include="P32ThetisExactGadgets.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P32ThetisExactGadgets.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Hard source gates: if any gadget is still the P31 substitute, this build is invalid.
$verify=[IO.File]::ReadAllText($dstExact)
foreach($token in @(
 'internal class clsVfoDisplay : clsMeterItem',
 'internal class clsButtonBox : clsMeterItem',
 'internal class clsBandButtonBox : clsButtonBox',
 'internal class clsModeButtonBox : clsButtonBox',
 'internal class clsTunestepButtons : clsButtonBox',
 'private void renderVfoDisplay(',
 'private void renderButtonBox(',
 'public string AddVFODisplay(',
 'public string AddBandButtons(',
 'public string AddModeButtons(',
 'public string AddTunestepButtons('
)){
    if(!$verify.Contains($token)){throw "P32 exact-source gate missing: $token"}
}
if($proj.Contains('P31ModernThetisGadgets.cs')){throw 'P32 invalid: P31 custom gadget implementation still compiled'}

Write-Host "P32_THETIS_SOURCE_SHA=$ThetisSha"
Write-Host 'P32_SOURCE_OF_TRUTH=THETIS_ONLY'
Write-Host 'P32_GADGET_CODE=EXACT_EXTRACTED_THETIS_VFO_BAND_MODE_TUNESTEP'
Write-Host 'P32_RENDERER=EXACT_EXTRACTED_THETIS_RENDER_VFO_AND_BUTTONBOX'
Write-Host 'P32_ADAPTER_POLICY=POWERSDR_API_MAPPING_ONLY'
Write-Host 'P32_RX2=NOT_EXPOSED'

    $match=$rx.Match($mm)
    if(!$match.Success){throw 'P32 MeterType CROSS anchor missing'}
    $indent=$match.Groups['indent'].Value
    $replacement=$indent+'CROSS,'+$nl+
                 $indent+'VFO_DISPLAY,'+$nl+
                 $indent+'BAND_BUTTONS,'+$nl+
                 $indent+'MODE_BUTTONS,'+$nl+
                 $indent+'TUNESTEP_BUTTONS,'
    $mm=$mm.Substring(0,$match.Index)+$replacement+$mm.Substring($match.Index+$match.Length)
}
if($mm -notmatch '(?m)^\s*VFO_DISPLAY,\s*$' -or $mm -notmatch '(?m)^\s*TUNESTEP_BUTTONS,\s*$')
{
    throw 'P32 MeterType insertion failed'
}

# MeterItemType: insert the exact modern item kinds after ITEM_GROUP.
$miStart=$mm.IndexOf('public enum MeterItemType')
if($miStart -lt 0){throw 'P32 MeterItemType enum missing'}
$miEnd=$mm.IndexOf('}',$miStart)
if($miEnd -lt 0){throw 'P32 MeterItemType enum close missing'}
$miBlock=$mm.Substring($miStart,$miEnd-$miStart)
if($miBlock -notmatch 'VFO_DISPLAY')
{
    $mm=[regex]::Replace($mm,'(?m)^(\s*)ITEM_GROUP\s*(,?)\s*(\r?\n)', '1ITEM_GROUP,31VFO_DISPLAY,31BAND_BUTTONS,31MODE_BUTTONS,31TUNESTEP_BUTTONS3',1)
}

# Exact AddMeter routes. P30 core has CROSS as last special case.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY:')
{
    $anchor='                    case MeterType.CROSS: AddCross(nMSupdate, fTop, out fBottom, restoreIg); break;'
    if(!$mm.Contains($anchor)){throw 'P32 AddMeter CROSS anchor missing'}
    $insert=@'
                    case MeterType.VFO_DISPLAY: AddVFODisplay(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.BAND_BUTTONS: AddBandButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.MODE_BUTTONS: AddModeButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.TUNESTEP_BUTTONS: AddTunestepButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
'@
    $mm=$mm.Replace($anchor,$anchor+$nl+$insert.TrimEnd())
}

# Names and RX/TX classification are Thetis names.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return "Vfo Display";')
{
    $nameAnchor='                case MeterType.CROSS: return "Cross Meter";'
    if(!$mm.Contains($nameAnchor)){throw 'P32 MeterName CROSS anchor missing'}
    $nameInsert=@'
                case MeterType.VFO_DISPLAY: return "Vfo Display";
                case MeterType.BAND_BUTTONS: return "Band Buttons";
                case MeterType.MODE_BUTTONS: return "Mode Buttons";
                case MeterType.TUNESTEP_BUTTONS: return "Tunestep Buttons";
'@
    $mm=$mm.Replace($nameAnchor,$nameAnchor+$nl+$nameInsert.TrimEnd())
}
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return 2;')
{
    $typeAnchor='                case MeterType.CROSS: return 2;'
    if(!$mm.Contains($typeAnchor)){throw 'P32 TXRX CROSS anchor missing'}
    $typeInsert=@'
                case MeterType.VFO_DISPLAY: return 2;
                case MeterType.BAND_BUTTONS: return 2;
                case MeterType.MODE_BUTTONS: return 2;
                case MeterType.TUNESTEP_BUTTONS: return 2;
'@
    $mm=$mm.Replace($typeAnchor,$typeAnchor+$nl+$typeInsert.TrimEnd())
}

# Exact renderer routing.
if($mm -notmatch 'case clsMeterItem\.MeterItemType\.VFO_DISPLAY:')
{
    $renderAnchor='                            case clsMeterItem.MeterItemType.MAGIC_EYE:'
    if(!$mm.Contains($renderAnchor)){throw 'P32 renderer MAGIC_EYE anchor missing'}
    $renderCases=@'
                            case clsMeterItem.MeterItemType.VFO_DISPLAY:
                                renderVfoDisplay(rect, mi, m);
                                break;
                            case clsMeterItem.MeterItemType.BAND_BUTTONS:
                            case clsMeterItem.MeterItemType.MODE_BUTTONS:
                            case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:
                                renderButtonBox(rect, mi, m);
                                break;
'@
    $mm=$mm.Replace($renderAnchor,$renderCases.TrimEnd()+$nl+$renderAnchor)
}

# Feed the exact Thetis state snapshot immediately after the existing PowerSDR telemetry refresh.
if(!$mm.Contains('P32RefreshExactGadgetState();'))
{
    $loopAnchor='    P25RefreshPowerSDR();'
    if(!$mm.Contains($loopAnchor)){throw 'P32 P25 refresh anchor missing'}
    $mm=$mm.Replace($loopAnchor,$loopAnchor+$nl+'    P32RefreshExactGadgetState();')
}

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# The exact 2024 classes expect the modern mouse/key callbacks. Add them to the old 2023 base
# with the same neutral virtual behaviour, then route WinForms events to them.
$source=@"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace PowerSDR
{
    internal static partial class MeterManager
    {
        public enum BandGroups { GEN = 0, HF, VHF, LAST = 99 }

        public partial class clsMeterItem
        {
            private MouseButtons _p32MouseButton;
            private bool _p32MouseButtonDown;
            private PointF _p32MouseDownPoint;
            private PointF _p32MouseUpPoint;
            private PointF _p32MouseMovePoint;
            private bool _p32MouseEntered;

            public virtual void MouseDown(MouseEventArgs e) { }
            public virtual void MouseUp(MouseEventArgs e) { }
            public virtual void MouseWheel(int number_of_moves) { }
            public virtual void KeyDown(Keys keycode) { }
            public virtual void KeyUp(Keys keycode) { }
            public virtual MouseButtons MouseButton { get { return _p32MouseButton; } set { _p32MouseButton = value; } }
            public virtual bool MouseButtonDown { get { return _p32MouseButtonDown; } set { _p32MouseButtonDown = value; } }
            public virtual PointF MouseDownPoint { get { return _p32MouseDownPoint; } set { _p32MouseDownPoint = value; } }
            public virtual PointF MouseUpPoint { get { return _p32MouseUpPoint; } set { _p32MouseUpPoint = value; } }
            public virtual PointF MouseMovePoint { get { return _p32MouseMovePoint; } set { _p32MouseMovePoint = value; } }
            public virtual bool MouseEntered { get { return _p32MouseEntered; } set { _p32MouseEntered = value; } }
            public virtual bool ClickHighlight { get { return false; } set { } }
            public virtual void BandPanelsChanged(bool gen, bool hf, bool vhf) { }
            public virtual void BandChanged(Band oldBand, Band newBand) { }
            public virtual void ModeChanged(DSPMode oldMode, DSPMode newMode) { }
            public virtual void TuneStepIndexChanged(int old_index, int new_index) { }
        }

        $clsButton

        $clsBand

        $clsMode

        $clsStep

        $clsVfo

        public partial class clsMeter
        {
            private double _p32VfoA, _p32VfoB, _p32VfoSub;
            private DSPMode _p32ModeA, _p32ModeB;
            private Band _p32BandA, _p32BandB, _p32BandSub;
            private Filter _p32FilterA, _p32FilterB;
            private string _p32FilterAName="", _p32FilterBName="";
            private string _p32BandAText="", _p32BandBText="";
            private bool _p32RX2, _p32Multi, _p32Split, _p32TXVfoB, _p32LockA, _p32LockB, _p32Sync, _p32QuickSplit;
            private int _p32TuneStep;

            public double VfoA { get { return _p32VfoA; } set { _p32VfoA=value; } }
            public double VfoB { get { return _p32VfoB; } set { _p32VfoB=value; } }
            public double VfoSub { get { return _p32VfoSub; } set { _p32VfoSub=value; } }
            public DSPMode ModeVfoA { get { return _p32ModeA; } set { _p32ModeA=value; } }
            public DSPMode ModeVfoB { get { return _p32ModeB; } set { _p32ModeB=value; } }
            public Band BandVfoA { get { return _p32BandA; } set { _p32BandA=value; } }
            public Band BandVfoB { get { return _p32BandB; } set { _p32BandB=value; } }
            public Band BandVfoASub { get { return _p32BandSub; } set { _p32BandSub=value; } }
            public Filter FilterVfoA { get { return _p32FilterA; } set { _p32FilterA=value; } }
            public Filter FilterVfoB { get { return _p32FilterB; } set { _p32FilterB=value; } }
            public string FilterVfoAName { get { return _p32FilterAName; } set { _p32FilterAName=value ?? ""; } }
            public string FilterVfoBName { get { return _p32FilterBName; } set { _p32FilterBName=value ?? ""; } }
            public string VFOABandText { get { return _p32BandAText; } set { _p32BandAText=value ?? ""; } }
            public string VFOBBandText { get { return _p32BandBText; } set { _p32BandBText=value ?? ""; } }
            public bool RX2Enabled { get { return _p32RX2; } set { _p32RX2=value; } }
            public bool MultiRxEnabled { get { return _p32Multi; } set { _p32Multi=value; } }
            public bool Split { get { return _p32Split; } set { _p32Split=value; } }
            public bool TXVFOb { get { return _p32TXVfoB; } set { _p32TXVfoB=value; } }
            public bool VFOALock { get { return _p32LockA; } set { _p32LockA=value; } }
            public bool VFOBLock { get { return _p32LockB; } set { _p32LockB=value; } }
            public bool VFOSync { get { return _p32Sync; } set { _p32Sync=value; } }
            public bool QuickSplitEnabled { get { return _p32QuickSplit; } set { _p32QuickSplit=value; } }
            public int TuneStepIndex { get { return _p32TuneStep; } set { _p32TuneStep=value; } }

            $getBandGroup

            $setBandPanel

            $addBand

            $addMode

            $addStep

            $addVfo
        }

        private static string P32FilterName(Filter f)
        {
            return f.ToString();
        }

        private static void P32RefreshExactGadgetState()
        {
            if (_console == null) return;
            lock (_metersLock)
            {
                foreach (KeyValuePair<string, clsMeter> kvp in _meters)
                {
                    clsMeter m=kvp.Value;
                    if (m.RX != 1) continue; // RX2 intentionally not exposed in this FLEX-5000 target.
                    Band oldBand=m.BandVfoA;
                    DSPMode oldMode=m.ModeVfoA;
                    int oldStep=m.TuneStepIndex;

                    m.MOX=_console.MOX;
                    m.Split=_console.VFOSplit;
                    m.TXVFOb=_console.VFOBTX;
                    m.RX2Enabled=false;
                    m.MultiRxEnabled=false;
                    m.VfoA=_console.VFOAFreq;
                    m.VfoB=_console.VFOBFreq;
                    m.VfoSub=_console.VFOASubFreq;
                    m.ModeVfoA=_console.RX1DSPMode;
                    m.ModeVfoB=_console.RX1DSPMode;
                    m.BandVfoA=_console.RX1Band;
                    m.BandVfoB=_console.P32BandFromFrequency(_console.VFOBFreq);
                    m.BandVfoASub=_console.RX1Band;
                    m.FilterVfoA=_console.RX1Filter;
                    m.FilterVfoB=_console.RX1Filter;
                    m.FilterVfoAName=P32FilterName(_console.RX1Filter);
                    m.FilterVfoBName=P32FilterName(_console.RX1Filter);
                    m.VFOALock=_console.VFOALock;
                    m.VFOBLock=_console.VFOBLock;
                    m.VFOSync=_console.VFOSync;
                    m.QuickSplitEnabled=false;
                    m.TuneStepIndex=_console.TuneStepIndex;
                    m.VFOABandText=P32ThetisBandStack.BandToString(m.BandVfoA);
                    m.VFOBBandText=P32ThetisBandStack.BandToString(m.BandVfoB);

                    if(oldBand != m.BandVfoA)
                        foreach(var item in m.MeterItems.Values) item.BandChanged(oldBand,m.BandVfoA);
                    if(oldMode != m.ModeVfoA)
                        foreach(var item in m.MeterItems.Values) item.ModeChanged(oldMode,m.ModeVfoA);
                    if(oldStep != m.TuneStepIndex)
                        foreach(var item in m.MeterItems.Values) item.TuneStepIndexChanged(oldStep,m.TuneStepIndex);
                }
            }
        }

        private partial class DXRenderer
        {
            private Dictionary<string, BitmapBrush> _bitmap_brushes = new Dictionary<string, BitmapBrush>();

            $getParts

            $plotText

            $shrinkRect

            $drawRounded

            $fillRounded

            $drawSafe

            $contrast

            $adjustContrast

            $drawBand

            $drawMode

            $drawFilter

            $drawStep

            $renderVfo

            $renderButton
        }
    }

    internal static class P32ThetisBandStack
    {
        internal static string BandToString(Band b)
        {
            switch(b)
            {
                case Band.GEN: return "GEN";
                case Band.B160M: return "160M"; case Band.B80M: return "80M"; case Band.B60M: return "60M";
                case Band.B40M: return "40M"; case Band.B30M: return "30M"; case Band.B20M: return "20M";
                case Band.B17M: return "17M"; case Band.B15M: return "15M"; case Band.B12M: return "12M";
                case Band.B10M: return "10M"; case Band.B6M: return "6M"; case Band.B2M: return "2M";
                case Band.WWV: return "WWV"; case Band.BLMF: return "LMF";
                case Band.B120M: return "120M"; case Band.B90M: return "90M"; case Band.B61M: return "61M";
                case Band.B49M: return "49M"; case Band.B41M: return "41M"; case Band.B31M: return "31M";
                case Band.B25M: return "25M"; case Band.B22M: return "22M"; case Band.B19M: return "19M";
                case Band.B16M: return "16M"; case Band.B14M: return "14M"; case Band.B13M: return "13M";
                case Band.B11M: return "11M";
                case Band.VHF0: return "VHF0"; case Band.VHF1: return "VHF1"; case Band.VHF2: return "VHF2";
                case Band.VHF3: return "VHF3"; case Band.VHF4: return "VHF4"; case Band.VHF5: return "VHF5";
                case Band.VHF6: return "VHF6"; case Band.VHF7: return "VHF7"; case Band.VHF8: return "VHF8";
                case Band.VHF9: return "VHF9"; case Band.VHF10: return "VHF10"; case Band.VHF11: return "VHF11";
                case Band.VHF12: return "VHF12"; case Band.VHF13: return "VHF13";
                default: return "GEN";
            }
        }

        internal static Color BandToColour(Band b)
        {
            if(b==Band.WWV) return Color.Green;
            if(b>=Band.B120M && b<=Band.B11M) return Color.Coral;
            if(b>=Band.VHF0 && b<=Band.VHF13) return Color.Gold;
            return Color.White;
        }
    }

    sealed unsafe public partial class Console
    {
        private MeterManager.BandGroups _p32BandPanel = MeterManager.BandGroups.LAST;

        public bool BandGENSelected { get { return P32BandPanel == MeterManager.BandGroups.GEN; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.GEN); } }
        public bool BandHFSelected { get { return P32BandPanel == MeterManager.BandGroups.HF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.HF); } }
        public bool BandVHFSelected { get { return P32BandPanel == MeterManager.BandGroups.VHF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.VHF); } }

        private MeterManager.BandGroups P32BandPanel
        {
            get
            {
                if(_p32BandPanel==MeterManager.BandGroups.LAST)
                {
                    Band b=RX1Band;
                    if(b>=Band.B160M && b<=Band.B2M || b==Band.WWV) _p32BandPanel=MeterManager.BandGroups.HF;
                    else if(b>=Band.VHF0 && b<=Band.VHF13) _p32BandPanel=MeterManager.BandGroups.VHF;
                    else _p32BandPanel=MeterManager.BandGroups.GEN;
                }
                return _p32BandPanel;
            }
        }

        public void P32SelectBandPanel(MeterManager.BandGroups group) { _p32BandPanel=group; }

        public bool VFOALock
        {
            get { return (CATVFOLockAB & 1) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 1) : (CATVFOLockAB & ~1); }
        }
        public bool VFOBLock
        {
            get { return (CATVFOLockAB & 2) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 2) : (CATVFOLockAB & ~2); }
        }

        public void P32SetRX1Band(Band band) { RX1Band=band; }

        public void P32SetVFOBBand(Band band)
        {
            // PowerSDR exposes RX2Band as its native VFO-B band path. This is the direct API mapping.
            RX2Band=band;
        }

        public Band P32BandFromFrequency(double mhz)
        {
            if(mhz>=1.8 && mhz<2.0) return Band.B160M;
            if(mhz>=3.5 && mhz<4.0) return Band.B80M;
            if(mhz>=5.0 && mhz<5.6) return Band.B60M;
            if(mhz>=7.0 && mhz<7.4) return Band.B40M;
            if(mhz>=10.0 && mhz<10.3) return Band.B30M;
            if(mhz>=14.0 && mhz<14.5) return Band.B20M;
            if(mhz>=18.0 && mhz<18.3) return Band.B17M;
            if(mhz>=21.0 && mhz<21.6) return Band.B15M;
            if(mhz>=24.8 && mhz<25.1) return Band.B12M;
            if(mhz>=28.0 && mhz<30.0) return Band.B10M;
            if(mhz>=50.0 && mhz<54.0) return Band.B6M;
            return RX1Band;
        }

        public bool GetVHFEnabled(int index) { return false; }
        public string GetVHFText(int index) { return "VHF"+index.ToString(); }

        public void P32PopupBandstack(int rx, Band band)
        {
            if(StackForm != null)
            {
                StackForm.Show();
                StackForm.BringToFront();
            }
        }

        public void P32PopupFilterMenu(int rx)
        {
            // PowerSDR has no Thetis filter popup API; the exact VFO renderer remains unchanged.
            // This adapter intentionally performs no substitute UI action.
        }
    }
}
"@

# Mechanical source adaptation for the old SharpDX renderer API.
$source=$source.Replace('private (float, float) plotText(','private Tuple<float,float> plotText(')
$source=$source.Replace('return (0, 0);','return Tuple.Create(0f,0f);')
$source=[regex]::Replace($source,'return \(szTextSize\.Width, szTextSize\.Height\);','return Tuple.Create(szTextSize.Width,szTextSize.Height);')

[IO.File]::WriteAllText($dstExact,$source,$utf8)

# Wire source into the project.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P32 csproj P30 anchor missing'}
if(!$proj.Contains('<Compile Include="P32ThetisExactGadgets.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P32ThetisExactGadgets.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Hard source gates: if any gadget is still the P31 substitute, this build is invalid.
$verify=[IO.File]::ReadAllText($dstExact)
foreach($token in @(
 'internal class clsVfoDisplay : clsMeterItem',
 'internal class clsButtonBox : clsMeterItem',
 'internal class clsBandButtonBox : clsButtonBox',
 'internal class clsModeButtonBox : clsButtonBox',
 'internal class clsTunestepButtons : clsButtonBox',
 'private void renderVfoDisplay(',
 'private void renderButtonBox(',
 'public string AddVFODisplay(',
 'public string AddBandButtons(',
 'public string AddModeButtons(',
 'public string AddTunestepButtons('
)){
    if(!$verify.Contains($token)){throw "P32 exact-source gate missing: $token"}
}
if($proj.Contains('P31ModernThetisGadgets.cs')){throw 'P32 invalid: P31 custom gadget implementation still compiled'}

Write-Host "P32_THETIS_SOURCE_SHA=$ThetisSha"
Write-Host 'P32_SOURCE_OF_TRUTH=THETIS_ONLY'
Write-Host 'P32_GADGET_CODE=EXACT_EXTRACTED_THETIS_VFO_BAND_MODE_TUNESTEP'
Write-Host 'P32_RENDERER=EXACT_EXTRACTED_THETIS_RENDER_VFO_AND_BUTTONBOX'
Write-Host 'P32_ADAPTER_POLICY=POWERSDR_API_MAPPING_ONLY'
Write-Host 'P32_RX2=NOT_EXPOSED'

    $matchItem=$rxItem.Match($mm)
    if(!$matchItem.Success){throw 'P32 MeterItemType ITEM_GROUP anchor missing'}
    $indentItem=$matchItem.Groups['indent'].Value
    $replacementItem=$indentItem+'ITEM_GROUP,'+$nl+
                     $indentItem+'VFO_DISPLAY,'+$nl+
                     $indentItem+'BAND_BUTTONS,'+$nl+
                     $indentItem+'MODE_BUTTONS,'+$nl+
                     $indentItem+'TUNESTEP_BUTTONS'
    $mm=$mm.Substring(0,$matchItem.Index)+$replacementItem+$mm.Substring($matchItem.Index+$matchItem.Length)
}

# Exact AddMeter routes. P30 core has CROSS as last special case.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY:')
{
    $anchor='                    case MeterType.CROSS: AddCross(nMSupdate, fTop, out fBottom, restoreIg); break;'
    if(!$mm.Contains($anchor)){throw 'P32 AddMeter CROSS anchor missing'}
    $insert=@'
                    case MeterType.VFO_DISPLAY: AddVFODisplay(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.BAND_BUTTONS: AddBandButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.MODE_BUTTONS: AddModeButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.TUNESTEP_BUTTONS: AddTunestepButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
'@
    $mm=$mm.Replace($anchor,$anchor+$nl+$insert.TrimEnd())
}

# Names and RX/TX classification are Thetis names.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return "Vfo Display";')
{
    $nameAnchor='                case MeterType.CROSS: return "Cross Meter";'
    if(!$mm.Contains($nameAnchor)){throw 'P32 MeterName CROSS anchor missing'}
    $nameInsert=@'
                case MeterType.VFO_DISPLAY: return "Vfo Display";
                case MeterType.BAND_BUTTONS: return "Band Buttons";
                case MeterType.MODE_BUTTONS: return "Mode Buttons";
                case MeterType.TUNESTEP_BUTTONS: return "Tunestep Buttons";
'@
    $mm=$mm.Replace($nameAnchor,$nameAnchor+$nl+$nameInsert.TrimEnd())
}
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return 2;')
{
    $typeAnchor='                case MeterType.CROSS: return 2;'
    if(!$mm.Contains($typeAnchor)){throw 'P32 TXRX CROSS anchor missing'}
    $typeInsert=@'
                case MeterType.VFO_DISPLAY: return 2;
                case MeterType.BAND_BUTTONS: return 2;
                case MeterType.MODE_BUTTONS: return 2;
                case MeterType.TUNESTEP_BUTTONS: return 2;
'@
    $mm=$mm.Replace($typeAnchor,$typeAnchor+$nl+$typeInsert.TrimEnd())
}

# Exact renderer routing.
if($mm -notmatch 'case clsMeterItem\.MeterItemType\.VFO_DISPLAY:')
{
    $renderAnchor='                            case clsMeterItem.MeterItemType.MAGIC_EYE:'
    if(!$mm.Contains($renderAnchor)){throw 'P32 renderer MAGIC_EYE anchor missing'}
    $renderCases=@'
                            case clsMeterItem.MeterItemType.VFO_DISPLAY:
                                renderVfoDisplay(rect, mi, m);
                                break;
                            case clsMeterItem.MeterItemType.BAND_BUTTONS:
                            case clsMeterItem.MeterItemType.MODE_BUTTONS:
                            case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:
                                renderButtonBox(rect, mi, m);
                                break;
'@
    $mm=$mm.Replace($renderAnchor,$renderCases.TrimEnd()+$nl+$renderAnchor)
}

# Feed the exact Thetis state snapshot immediately after the existing PowerSDR telemetry refresh.
if(!$mm.Contains('P32RefreshExactGadgetState();'))
{
    $loopAnchor='    P25RefreshPowerSDR();'
    if(!$mm.Contains($loopAnchor)){throw 'P32 P25 refresh anchor missing'}
    $mm=$mm.Replace($loopAnchor,$loopAnchor+$nl+'    P32RefreshExactGadgetState();')
}

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# The exact 2024 classes expect the modern mouse/key callbacks. Add them to the old 2023 base
# with the same neutral virtual behaviour, then route WinForms events to them.
$source=@"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace PowerSDR
{
    internal static partial class MeterManager
    {
        public enum BandGroups { GEN = 0, HF, VHF, LAST = 99 }

        public partial class clsMeterItem
        {
            private MouseButtons _p32MouseButton;
            private bool _p32MouseButtonDown;
            private PointF _p32MouseDownPoint;
            private PointF _p32MouseUpPoint;
            private PointF _p32MouseMovePoint;
            private bool _p32MouseEntered;

            public virtual void MouseDown(MouseEventArgs e) { }
            public virtual void MouseUp(MouseEventArgs e) { }
            public virtual void MouseWheel(int number_of_moves) { }
            public virtual void KeyDown(Keys keycode) { }
            public virtual void KeyUp(Keys keycode) { }
            public virtual MouseButtons MouseButton { get { return _p32MouseButton; } set { _p32MouseButton = value; } }
            public virtual bool MouseButtonDown { get { return _p32MouseButtonDown; } set { _p32MouseButtonDown = value; } }
            public virtual PointF MouseDownPoint { get { return _p32MouseDownPoint; } set { _p32MouseDownPoint = value; } }
            public virtual PointF MouseUpPoint { get { return _p32MouseUpPoint; } set { _p32MouseUpPoint = value; } }
            public virtual PointF MouseMovePoint { get { return _p32MouseMovePoint; } set { _p32MouseMovePoint = value; } }
            public virtual bool MouseEntered { get { return _p32MouseEntered; } set { _p32MouseEntered = value; } }
            public virtual bool ClickHighlight { get { return false; } set { } }
            public virtual void BandPanelsChanged(bool gen, bool hf, bool vhf) { }
            public virtual void BandChanged(Band oldBand, Band newBand) { }
            public virtual void ModeChanged(DSPMode oldMode, DSPMode newMode) { }
            public virtual void TuneStepIndexChanged(int old_index, int new_index) { }
        }

        $clsButton

        $clsBand

        $clsMode

        $clsStep

        $clsVfo

        public partial class clsMeter
        {
            private double _p32VfoA, _p32VfoB, _p32VfoSub;
            private DSPMode _p32ModeA, _p32ModeB;
            private Band _p32BandA, _p32BandB, _p32BandSub;
            private Filter _p32FilterA, _p32FilterB;
            private string _p32FilterAName="", _p32FilterBName="";
            private string _p32BandAText="", _p32BandBText="";
            private bool _p32RX2, _p32Multi, _p32Split, _p32TXVfoB, _p32LockA, _p32LockB, _p32Sync, _p32QuickSplit;
            private int _p32TuneStep;

            public double VfoA { get { return _p32VfoA; } set { _p32VfoA=value; } }
            public double VfoB { get { return _p32VfoB; } set { _p32VfoB=value; } }
            public double VfoSub { get { return _p32VfoSub; } set { _p32VfoSub=value; } }
            public DSPMode ModeVfoA { get { return _p32ModeA; } set { _p32ModeA=value; } }
            public DSPMode ModeVfoB { get { return _p32ModeB; } set { _p32ModeB=value; } }
            public Band BandVfoA { get { return _p32BandA; } set { _p32BandA=value; } }
            public Band BandVfoB { get { return _p32BandB; } set { _p32BandB=value; } }
            public Band BandVfoASub { get { return _p32BandSub; } set { _p32BandSub=value; } }
            public Filter FilterVfoA { get { return _p32FilterA; } set { _p32FilterA=value; } }
            public Filter FilterVfoB { get { return _p32FilterB; } set { _p32FilterB=value; } }
            public string FilterVfoAName { get { return _p32FilterAName; } set { _p32FilterAName=value ?? ""; } }
            public string FilterVfoBName { get { return _p32FilterBName; } set { _p32FilterBName=value ?? ""; } }
            public string VFOABandText { get { return _p32BandAText; } set { _p32BandAText=value ?? ""; } }
            public string VFOBBandText { get { return _p32BandBText; } set { _p32BandBText=value ?? ""; } }
            public bool RX2Enabled { get { return _p32RX2; } set { _p32RX2=value; } }
            public bool MultiRxEnabled { get { return _p32Multi; } set { _p32Multi=value; } }
            public bool Split { get { return _p32Split; } set { _p32Split=value; } }
            public bool TXVFOb { get { return _p32TXVfoB; } set { _p32TXVfoB=value; } }
            public bool VFOALock { get { return _p32LockA; } set { _p32LockA=value; } }
            public bool VFOBLock { get { return _p32LockB; } set { _p32LockB=value; } }
            public bool VFOSync { get { return _p32Sync; } set { _p32Sync=value; } }
            public bool QuickSplitEnabled { get { return _p32QuickSplit; } set { _p32QuickSplit=value; } }
            public int TuneStepIndex { get { return _p32TuneStep; } set { _p32TuneStep=value; } }

            $getBandGroup

            $setBandPanel

            $addBand

            $addMode

            $addStep

            $addVfo
        }

        private static string P32FilterName(Filter f)
        {
            return f.ToString();
        }

        private static void P32RefreshExactGadgetState()
        {
            if (_console == null) return;
            lock (_metersLock)
            {
                foreach (KeyValuePair<string, clsMeter> kvp in _meters)
                {
                    clsMeter m=kvp.Value;
                    if (m.RX != 1) continue; // RX2 intentionally not exposed in this FLEX-5000 target.
                    Band oldBand=m.BandVfoA;
                    DSPMode oldMode=m.ModeVfoA;
                    int oldStep=m.TuneStepIndex;

                    m.MOX=_console.MOX;
                    m.Split=_console.VFOSplit;
                    m.TXVFOb=_console.VFOBTX;
                    m.RX2Enabled=false;
                    m.MultiRxEnabled=false;
                    m.VfoA=_console.VFOAFreq;
                    m.VfoB=_console.VFOBFreq;
                    m.VfoSub=_console.VFOASubFreq;
                    m.ModeVfoA=_console.RX1DSPMode;
                    m.ModeVfoB=_console.RX1DSPMode;
                    m.BandVfoA=_console.RX1Band;
                    m.BandVfoB=_console.P32BandFromFrequency(_console.VFOBFreq);
                    m.BandVfoASub=_console.RX1Band;
                    m.FilterVfoA=_console.RX1Filter;
                    m.FilterVfoB=_console.RX1Filter;
                    m.FilterVfoAName=P32FilterName(_console.RX1Filter);
                    m.FilterVfoBName=P32FilterName(_console.RX1Filter);
                    m.VFOALock=_console.VFOALock;
                    m.VFOBLock=_console.VFOBLock;
                    m.VFOSync=_console.VFOSync;
                    m.QuickSplitEnabled=false;
                    m.TuneStepIndex=_console.TuneStepIndex;
                    m.VFOABandText=P32ThetisBandStack.BandToString(m.BandVfoA);
                    m.VFOBBandText=P32ThetisBandStack.BandToString(m.BandVfoB);

                    if(oldBand != m.BandVfoA)
                        foreach(var item in m.MeterItems.Values) item.BandChanged(oldBand,m.BandVfoA);
                    if(oldMode != m.ModeVfoA)
                        foreach(var item in m.MeterItems.Values) item.ModeChanged(oldMode,m.ModeVfoA);
                    if(oldStep != m.TuneStepIndex)
                        foreach(var item in m.MeterItems.Values) item.TuneStepIndexChanged(oldStep,m.TuneStepIndex);
                }
            }
        }

        private partial class DXRenderer
        {
            private Dictionary<string, BitmapBrush> _bitmap_brushes = new Dictionary<string, BitmapBrush>();

            $getParts

            $plotText

            $shrinkRect

            $drawRounded

            $fillRounded

            $drawSafe

            $contrast

            $adjustContrast

            $drawBand

            $drawMode

            $drawFilter

            $drawStep

            $renderVfo

            $renderButton
        }
    }

    internal static class P32ThetisBandStack
    {
        internal static string BandToString(Band b)
        {
            switch(b)
            {
                case Band.GEN: return "GEN";
                case Band.B160M: return "160M"; case Band.B80M: return "80M"; case Band.B60M: return "60M";
                case Band.B40M: return "40M"; case Band.B30M: return "30M"; case Band.B20M: return "20M";
                case Band.B17M: return "17M"; case Band.B15M: return "15M"; case Band.B12M: return "12M";
                case Band.B10M: return "10M"; case Band.B6M: return "6M"; case Band.B2M: return "2M";
                case Band.WWV: return "WWV"; case Band.BLMF: return "LMF";
                case Band.B120M: return "120M"; case Band.B90M: return "90M"; case Band.B61M: return "61M";
                case Band.B49M: return "49M"; case Band.B41M: return "41M"; case Band.B31M: return "31M";
                case Band.B25M: return "25M"; case Band.B22M: return "22M"; case Band.B19M: return "19M";
                case Band.B16M: return "16M"; case Band.B14M: return "14M"; case Band.B13M: return "13M";
                case Band.B11M: return "11M";
                case Band.VHF0: return "VHF0"; case Band.VHF1: return "VHF1"; case Band.VHF2: return "VHF2";
                case Band.VHF3: return "VHF3"; case Band.VHF4: return "VHF4"; case Band.VHF5: return "VHF5";
                case Band.VHF6: return "VHF6"; case Band.VHF7: return "VHF7"; case Band.VHF8: return "VHF8";
                case Band.VHF9: return "VHF9"; case Band.VHF10: return "VHF10"; case Band.VHF11: return "VHF11";
                case Band.VHF12: return "VHF12"; case Band.VHF13: return "VHF13";
                default: return "GEN";
            }
        }

        internal static Color BandToColour(Band b)
        {
            if(b==Band.WWV) return Color.Green;
            if(b>=Band.B120M && b<=Band.B11M) return Color.Coral;
            if(b>=Band.VHF0 && b<=Band.VHF13) return Color.Gold;
            return Color.White;
        }
    }

    sealed unsafe public partial class Console
    {
        private MeterManager.BandGroups _p32BandPanel = MeterManager.BandGroups.LAST;

        public bool BandGENSelected { get { return P32BandPanel == MeterManager.BandGroups.GEN; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.GEN); } }
        public bool BandHFSelected { get { return P32BandPanel == MeterManager.BandGroups.HF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.HF); } }
        public bool BandVHFSelected { get { return P32BandPanel == MeterManager.BandGroups.VHF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.VHF); } }

        private MeterManager.BandGroups P32BandPanel
        {
            get
            {
                if(_p32BandPanel==MeterManager.BandGroups.LAST)
                {
                    Band b=RX1Band;
                    if(b>=Band.B160M && b<=Band.B2M || b==Band.WWV) _p32BandPanel=MeterManager.BandGroups.HF;
                    else if(b>=Band.VHF0 && b<=Band.VHF13) _p32BandPanel=MeterManager.BandGroups.VHF;
                    else _p32BandPanel=MeterManager.BandGroups.GEN;
                }
                return _p32BandPanel;
            }
        }

        public void P32SelectBandPanel(MeterManager.BandGroups group) { _p32BandPanel=group; }

        public bool VFOALock
        {
            get { return (CATVFOLockAB & 1) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 1) : (CATVFOLockAB & ~1); }
        }
        public bool VFOBLock
        {
            get { return (CATVFOLockAB & 2) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 2) : (CATVFOLockAB & ~2); }
        }

        public void P32SetRX1Band(Band band) { RX1Band=band; }

        public void P32SetVFOBBand(Band band)
        {
            // PowerSDR exposes RX2Band as its native VFO-B band path. This is the direct API mapping.
            RX2Band=band;
        }

        public Band P32BandFromFrequency(double mhz)
        {
            if(mhz>=1.8 && mhz<2.0) return Band.B160M;
            if(mhz>=3.5 && mhz<4.0) return Band.B80M;
            if(mhz>=5.0 && mhz<5.6) return Band.B60M;
            if(mhz>=7.0 && mhz<7.4) return Band.B40M;
            if(mhz>=10.0 && mhz<10.3) return Band.B30M;
            if(mhz>=14.0 && mhz<14.5) return Band.B20M;
            if(mhz>=18.0 && mhz<18.3) return Band.B17M;
            if(mhz>=21.0 && mhz<21.6) return Band.B15M;
            if(mhz>=24.8 && mhz<25.1) return Band.B12M;
            if(mhz>=28.0 && mhz<30.0) return Band.B10M;
            if(mhz>=50.0 && mhz<54.0) return Band.B6M;
            return RX1Band;
        }

        public bool GetVHFEnabled(int index) { return false; }
        public string GetVHFText(int index) { return "VHF"+index.ToString(); }

        public void P32PopupBandstack(int rx, Band band)
        {
            if(StackForm != null)
            {
                StackForm.Show();
                StackForm.BringToFront();
            }
        }

        public void P32PopupFilterMenu(int rx)
        {
            // PowerSDR has no Thetis filter popup API; the exact VFO renderer remains unchanged.
            // This adapter intentionally performs no substitute UI action.
        }
    }
}
"@

# Mechanical source adaptation for the old SharpDX renderer API.
$source=$source.Replace('private (float, float) plotText(','private Tuple<float,float> plotText(')
$source=$source.Replace('return (0, 0);','return Tuple.Create(0f,0f);')
$source=[regex]::Replace($source,'return \(szTextSize\.Width, szTextSize\.Height\);','return Tuple.Create(szTextSize.Width,szTextSize.Height);')

[IO.File]::WriteAllText($dstExact,$source,$utf8)

# Wire source into the project.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P32 csproj P30 anchor missing'}
if(!$proj.Contains('<Compile Include="P32ThetisExactGadgets.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P32ThetisExactGadgets.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Hard source gates: if any gadget is still the P31 substitute, this build is invalid.
$verify=[IO.File]::ReadAllText($dstExact)
foreach($token in @(
 'internal class clsVfoDisplay : clsMeterItem',
 'internal class clsButtonBox : clsMeterItem',
 'internal class clsBandButtonBox : clsButtonBox',
 'internal class clsModeButtonBox : clsButtonBox',
 'internal class clsTunestepButtons : clsButtonBox',
 'private void renderVfoDisplay(',
 'private void renderButtonBox(',
 'public string AddVFODisplay(',
 'public string AddBandButtons(',
 'public string AddModeButtons(',
 'public string AddTunestepButtons('
)){
    if(!$verify.Contains($token)){throw "P32 exact-source gate missing: $token"}
}
if($proj.Contains('P31ModernThetisGadgets.cs')){throw 'P32 invalid: P31 custom gadget implementation still compiled'}

Write-Host "P32_THETIS_SOURCE_SHA=$ThetisSha"
Write-Host 'P32_SOURCE_OF_TRUTH=THETIS_ONLY'
Write-Host 'P32_GADGET_CODE=EXACT_EXTRACTED_THETIS_VFO_BAND_MODE_TUNESTEP'
Write-Host 'P32_RENDERER=EXACT_EXTRACTED_THETIS_RENDER_VFO_AND_BUTTONBOX'
Write-Host 'P32_ADAPTER_POLICY=POWERSDR_API_MAPPING_ONLY'
Write-Host 'P32_RX2=NOT_EXPOSED'

    $match=$rx.Match($mm)
    if(!$match.Success){throw 'P32 MeterType CROSS anchor missing'}
    $indent=$match.Groups['indent'].Value
    $replacement=$indent+'CROSS,'+$nl+
                 $indent+'VFO_DISPLAY,'+$nl+
                 $indent+'BAND_BUTTONS,'+$nl+
                 $indent+'MODE_BUTTONS,'+$nl+
                 $indent+'TUNESTEP_BUTTONS,'
    $mm=$mm.Substring(0,$match.Index)+$replacement+$mm.Substring($match.Index+$match.Length)
}
if($mm -notmatch '(?m)^\s*VFO_DISPLAY,\s*$' -or $mm -notmatch '(?m)^\s*TUNESTEP_BUTTONS,\s*$')
{
    throw 'P32 MeterType insertion failed'
}

# MeterItemType: insert the exact modern item kinds after ITEM_GROUP.
$miStart=$mm.IndexOf('public enum MeterItemType')
if($miStart -lt 0){throw 'P32 MeterItemType enum missing'}
$miEnd=$mm.IndexOf('}',$miStart)
if($miEnd -lt 0){throw 'P32 MeterItemType enum close missing'}
$miBlock=$mm.Substring($miStart,$miEnd-$miStart)
if($miBlock -notmatch 'VFO_DISPLAY')
{
    $mm=[regex]::Replace($mm,'(?m)^(\s*)ITEM_GROUP\s*(,?)\s*(\r?\n)', '1ITEM_GROUP,31VFO_DISPLAY,31BAND_BUTTONS,31MODE_BUTTONS,31TUNESTEP_BUTTONS3',1)
}

# Exact AddMeter routes. P30 core has CROSS as last special case.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY:')
{
    $anchor='                    case MeterType.CROSS: AddCross(nMSupdate, fTop, out fBottom, restoreIg); break;'
    if(!$mm.Contains($anchor)){throw 'P32 AddMeter CROSS anchor missing'}
    $insert=@'
                    case MeterType.VFO_DISPLAY: AddVFODisplay(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.BAND_BUTTONS: AddBandButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.MODE_BUTTONS: AddModeButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
                    case MeterType.TUNESTEP_BUTTONS: AddTunestepButtons(nMSupdate, fTop, out fBottom, restoreIg); break;
'@
    $mm=$mm.Replace($anchor,$anchor+$nl+$insert.TrimEnd())
}

# Names and RX/TX classification are Thetis names.
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return "Vfo Display";')
{
    $nameAnchor='                case MeterType.CROSS: return "Cross Meter";'
    if(!$mm.Contains($nameAnchor)){throw 'P32 MeterName CROSS anchor missing'}
    $nameInsert=@'
                case MeterType.VFO_DISPLAY: return "Vfo Display";
                case MeterType.BAND_BUTTONS: return "Band Buttons";
                case MeterType.MODE_BUTTONS: return "Mode Buttons";
                case MeterType.TUNESTEP_BUTTONS: return "Tunestep Buttons";
'@
    $mm=$mm.Replace($nameAnchor,$nameAnchor+$nl+$nameInsert.TrimEnd())
}
if($mm -notmatch 'case MeterType\.VFO_DISPLAY: return 2;')
{
    $typeAnchor='                case MeterType.CROSS: return 2;'
    if(!$mm.Contains($typeAnchor)){throw 'P32 TXRX CROSS anchor missing'}
    $typeInsert=@'
                case MeterType.VFO_DISPLAY: return 2;
                case MeterType.BAND_BUTTONS: return 2;
                case MeterType.MODE_BUTTONS: return 2;
                case MeterType.TUNESTEP_BUTTONS: return 2;
'@
    $mm=$mm.Replace($typeAnchor,$typeAnchor+$nl+$typeInsert.TrimEnd())
}

# Exact renderer routing.
if($mm -notmatch 'case clsMeterItem\.MeterItemType\.VFO_DISPLAY:')
{
    $renderAnchor='                            case clsMeterItem.MeterItemType.MAGIC_EYE:'
    if(!$mm.Contains($renderAnchor)){throw 'P32 renderer MAGIC_EYE anchor missing'}
    $renderCases=@'
                            case clsMeterItem.MeterItemType.VFO_DISPLAY:
                                renderVfoDisplay(rect, mi, m);
                                break;
                            case clsMeterItem.MeterItemType.BAND_BUTTONS:
                            case clsMeterItem.MeterItemType.MODE_BUTTONS:
                            case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:
                                renderButtonBox(rect, mi, m);
                                break;
'@
    $mm=$mm.Replace($renderAnchor,$renderCases.TrimEnd()+$nl+$renderAnchor)
}

# Feed the exact Thetis state snapshot immediately after the existing PowerSDR telemetry refresh.
if(!$mm.Contains('P32RefreshExactGadgetState();'))
{
    $loopAnchor='    P25RefreshPowerSDR();'
    if(!$mm.Contains($loopAnchor)){throw 'P32 P25 refresh anchor missing'}
    $mm=$mm.Replace($loopAnchor,$loopAnchor+$nl+'    P32RefreshExactGadgetState();')
}

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# The exact 2024 classes expect the modern mouse/key callbacks. Add them to the old 2023 base
# with the same neutral virtual behaviour, then route WinForms events to them.
$source=@"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace PowerSDR
{
    internal static partial class MeterManager
    {
        public enum BandGroups { GEN = 0, HF, VHF, LAST = 99 }

        public partial class clsMeterItem
        {
            private MouseButtons _p32MouseButton;
            private bool _p32MouseButtonDown;
            private PointF _p32MouseDownPoint;
            private PointF _p32MouseUpPoint;
            private PointF _p32MouseMovePoint;
            private bool _p32MouseEntered;

            public virtual void MouseDown(MouseEventArgs e) { }
            public virtual void MouseUp(MouseEventArgs e) { }
            public virtual void MouseWheel(int number_of_moves) { }
            public virtual void KeyDown(Keys keycode) { }
            public virtual void KeyUp(Keys keycode) { }
            public virtual MouseButtons MouseButton { get { return _p32MouseButton; } set { _p32MouseButton = value; } }
            public virtual bool MouseButtonDown { get { return _p32MouseButtonDown; } set { _p32MouseButtonDown = value; } }
            public virtual PointF MouseDownPoint { get { return _p32MouseDownPoint; } set { _p32MouseDownPoint = value; } }
            public virtual PointF MouseUpPoint { get { return _p32MouseUpPoint; } set { _p32MouseUpPoint = value; } }
            public virtual PointF MouseMovePoint { get { return _p32MouseMovePoint; } set { _p32MouseMovePoint = value; } }
            public virtual bool MouseEntered { get { return _p32MouseEntered; } set { _p32MouseEntered = value; } }
            public virtual bool ClickHighlight { get { return false; } set { } }
            public virtual void BandPanelsChanged(bool gen, bool hf, bool vhf) { }
            public virtual void BandChanged(Band oldBand, Band newBand) { }
            public virtual void ModeChanged(DSPMode oldMode, DSPMode newMode) { }
            public virtual void TuneStepIndexChanged(int old_index, int new_index) { }
        }

        $clsButton

        $clsBand

        $clsMode

        $clsStep

        $clsVfo

        public partial class clsMeter
        {
            private double _p32VfoA, _p32VfoB, _p32VfoSub;
            private DSPMode _p32ModeA, _p32ModeB;
            private Band _p32BandA, _p32BandB, _p32BandSub;
            private Filter _p32FilterA, _p32FilterB;
            private string _p32FilterAName="", _p32FilterBName="";
            private string _p32BandAText="", _p32BandBText="";
            private bool _p32RX2, _p32Multi, _p32Split, _p32TXVfoB, _p32LockA, _p32LockB, _p32Sync, _p32QuickSplit;
            private int _p32TuneStep;

            public double VfoA { get { return _p32VfoA; } set { _p32VfoA=value; } }
            public double VfoB { get { return _p32VfoB; } set { _p32VfoB=value; } }
            public double VfoSub { get { return _p32VfoSub; } set { _p32VfoSub=value; } }
            public DSPMode ModeVfoA { get { return _p32ModeA; } set { _p32ModeA=value; } }
            public DSPMode ModeVfoB { get { return _p32ModeB; } set { _p32ModeB=value; } }
            public Band BandVfoA { get { return _p32BandA; } set { _p32BandA=value; } }
            public Band BandVfoB { get { return _p32BandB; } set { _p32BandB=value; } }
            public Band BandVfoASub { get { return _p32BandSub; } set { _p32BandSub=value; } }
            public Filter FilterVfoA { get { return _p32FilterA; } set { _p32FilterA=value; } }
            public Filter FilterVfoB { get { return _p32FilterB; } set { _p32FilterB=value; } }
            public string FilterVfoAName { get { return _p32FilterAName; } set { _p32FilterAName=value ?? ""; } }
            public string FilterVfoBName { get { return _p32FilterBName; } set { _p32FilterBName=value ?? ""; } }
            public string VFOABandText { get { return _p32BandAText; } set { _p32BandAText=value ?? ""; } }
            public string VFOBBandText { get { return _p32BandBText; } set { _p32BandBText=value ?? ""; } }
            public bool RX2Enabled { get { return _p32RX2; } set { _p32RX2=value; } }
            public bool MultiRxEnabled { get { return _p32Multi; } set { _p32Multi=value; } }
            public bool Split { get { return _p32Split; } set { _p32Split=value; } }
            public bool TXVFOb { get { return _p32TXVfoB; } set { _p32TXVfoB=value; } }
            public bool VFOALock { get { return _p32LockA; } set { _p32LockA=value; } }
            public bool VFOBLock { get { return _p32LockB; } set { _p32LockB=value; } }
            public bool VFOSync { get { return _p32Sync; } set { _p32Sync=value; } }
            public bool QuickSplitEnabled { get { return _p32QuickSplit; } set { _p32QuickSplit=value; } }
            public int TuneStepIndex { get { return _p32TuneStep; } set { _p32TuneStep=value; } }

            $getBandGroup

            $setBandPanel

            $addBand

            $addMode

            $addStep

            $addVfo
        }

        private static string P32FilterName(Filter f)
        {
            return f.ToString();
        }

        private static void P32RefreshExactGadgetState()
        {
            if (_console == null) return;
            lock (_metersLock)
            {
                foreach (KeyValuePair<string, clsMeter> kvp in _meters)
                {
                    clsMeter m=kvp.Value;
                    if (m.RX != 1) continue; // RX2 intentionally not exposed in this FLEX-5000 target.
                    Band oldBand=m.BandVfoA;
                    DSPMode oldMode=m.ModeVfoA;
                    int oldStep=m.TuneStepIndex;

                    m.MOX=_console.MOX;
                    m.Split=_console.VFOSplit;
                    m.TXVFOb=_console.VFOBTX;
                    m.RX2Enabled=false;
                    m.MultiRxEnabled=false;
                    m.VfoA=_console.VFOAFreq;
                    m.VfoB=_console.VFOBFreq;
                    m.VfoSub=_console.VFOASubFreq;
                    m.ModeVfoA=_console.RX1DSPMode;
                    m.ModeVfoB=_console.RX1DSPMode;
                    m.BandVfoA=_console.RX1Band;
                    m.BandVfoB=_console.P32BandFromFrequency(_console.VFOBFreq);
                    m.BandVfoASub=_console.RX1Band;
                    m.FilterVfoA=_console.RX1Filter;
                    m.FilterVfoB=_console.RX1Filter;
                    m.FilterVfoAName=P32FilterName(_console.RX1Filter);
                    m.FilterVfoBName=P32FilterName(_console.RX1Filter);
                    m.VFOALock=_console.VFOALock;
                    m.VFOBLock=_console.VFOBLock;
                    m.VFOSync=_console.VFOSync;
                    m.QuickSplitEnabled=false;
                    m.TuneStepIndex=_console.TuneStepIndex;
                    m.VFOABandText=P32ThetisBandStack.BandToString(m.BandVfoA);
                    m.VFOBBandText=P32ThetisBandStack.BandToString(m.BandVfoB);

                    if(oldBand != m.BandVfoA)
                        foreach(var item in m.MeterItems.Values) item.BandChanged(oldBand,m.BandVfoA);
                    if(oldMode != m.ModeVfoA)
                        foreach(var item in m.MeterItems.Values) item.ModeChanged(oldMode,m.ModeVfoA);
                    if(oldStep != m.TuneStepIndex)
                        foreach(var item in m.MeterItems.Values) item.TuneStepIndexChanged(oldStep,m.TuneStepIndex);
                }
            }
        }

        private partial class DXRenderer
        {
            private Dictionary<string, BitmapBrush> _bitmap_brushes = new Dictionary<string, BitmapBrush>();

            $getParts

            $plotText

            $shrinkRect

            $drawRounded

            $fillRounded

            $drawSafe

            $contrast

            $adjustContrast

            $drawBand

            $drawMode

            $drawFilter

            $drawStep

            $renderVfo

            $renderButton
        }
    }

    internal static class P32ThetisBandStack
    {
        internal static string BandToString(Band b)
        {
            switch(b)
            {
                case Band.GEN: return "GEN";
                case Band.B160M: return "160M"; case Band.B80M: return "80M"; case Band.B60M: return "60M";
                case Band.B40M: return "40M"; case Band.B30M: return "30M"; case Band.B20M: return "20M";
                case Band.B17M: return "17M"; case Band.B15M: return "15M"; case Band.B12M: return "12M";
                case Band.B10M: return "10M"; case Band.B6M: return "6M"; case Band.B2M: return "2M";
                case Band.WWV: return "WWV"; case Band.BLMF: return "LMF";
                case Band.B120M: return "120M"; case Band.B90M: return "90M"; case Band.B61M: return "61M";
                case Band.B49M: return "49M"; case Band.B41M: return "41M"; case Band.B31M: return "31M";
                case Band.B25M: return "25M"; case Band.B22M: return "22M"; case Band.B19M: return "19M";
                case Band.B16M: return "16M"; case Band.B14M: return "14M"; case Band.B13M: return "13M";
                case Band.B11M: return "11M";
                case Band.VHF0: return "VHF0"; case Band.VHF1: return "VHF1"; case Band.VHF2: return "VHF2";
                case Band.VHF3: return "VHF3"; case Band.VHF4: return "VHF4"; case Band.VHF5: return "VHF5";
                case Band.VHF6: return "VHF6"; case Band.VHF7: return "VHF7"; case Band.VHF8: return "VHF8";
                case Band.VHF9: return "VHF9"; case Band.VHF10: return "VHF10"; case Band.VHF11: return "VHF11";
                case Band.VHF12: return "VHF12"; case Band.VHF13: return "VHF13";
                default: return "GEN";
            }
        }

        internal static Color BandToColour(Band b)
        {
            if(b==Band.WWV) return Color.Green;
            if(b>=Band.B120M && b<=Band.B11M) return Color.Coral;
            if(b>=Band.VHF0 && b<=Band.VHF13) return Color.Gold;
            return Color.White;
        }
    }

    sealed unsafe public partial class Console
    {
        private MeterManager.BandGroups _p32BandPanel = MeterManager.BandGroups.LAST;

        public bool BandGENSelected { get { return P32BandPanel == MeterManager.BandGroups.GEN; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.GEN); } }
        public bool BandHFSelected { get { return P32BandPanel == MeterManager.BandGroups.HF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.HF); } }
        public bool BandVHFSelected { get { return P32BandPanel == MeterManager.BandGroups.VHF; } set { if(value) P32SelectBandPanel(MeterManager.BandGroups.VHF); } }

        private MeterManager.BandGroups P32BandPanel
        {
            get
            {
                if(_p32BandPanel==MeterManager.BandGroups.LAST)
                {
                    Band b=RX1Band;
                    if(b>=Band.B160M && b<=Band.B2M || b==Band.WWV) _p32BandPanel=MeterManager.BandGroups.HF;
                    else if(b>=Band.VHF0 && b<=Band.VHF13) _p32BandPanel=MeterManager.BandGroups.VHF;
                    else _p32BandPanel=MeterManager.BandGroups.GEN;
                }
                return _p32BandPanel;
            }
        }

        public void P32SelectBandPanel(MeterManager.BandGroups group) { _p32BandPanel=group; }

        public bool VFOALock
        {
            get { return (CATVFOLockAB & 1) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 1) : (CATVFOLockAB & ~1); }
        }
        public bool VFOBLock
        {
            get { return (CATVFOLockAB & 2) != 0; }
            set { CATVFOLockAB = value ? (CATVFOLockAB | 2) : (CATVFOLockAB & ~2); }
        }

        public void P32SetRX1Band(Band band) { RX1Band=band; }

        public void P32SetVFOBBand(Band band)
        {
            // PowerSDR exposes RX2Band as its native VFO-B band path. This is the direct API mapping.
            RX2Band=band;
        }

        public Band P32BandFromFrequency(double mhz)
        {
            if(mhz>=1.8 && mhz<2.0) return Band.B160M;
            if(mhz>=3.5 && mhz<4.0) return Band.B80M;
            if(mhz>=5.0 && mhz<5.6) return Band.B60M;
            if(mhz>=7.0 && mhz<7.4) return Band.B40M;
            if(mhz>=10.0 && mhz<10.3) return Band.B30M;
            if(mhz>=14.0 && mhz<14.5) return Band.B20M;
            if(mhz>=18.0 && mhz<18.3) return Band.B17M;
            if(mhz>=21.0 && mhz<21.6) return Band.B15M;
            if(mhz>=24.8 && mhz<25.1) return Band.B12M;
            if(mhz>=28.0 && mhz<30.0) return Band.B10M;
            if(mhz>=50.0 && mhz<54.0) return Band.B6M;
            return RX1Band;
        }

        public bool GetVHFEnabled(int index) { return false; }
        public string GetVHFText(int index) { return "VHF"+index.ToString(); }

        public void P32PopupBandstack(int rx, Band band)
        {
            if(StackForm != null)
            {
                StackForm.Show();
                StackForm.BringToFront();
            }
        }

        public void P32PopupFilterMenu(int rx)
        {
            // PowerSDR has no Thetis filter popup API; the exact VFO renderer remains unchanged.
            // This adapter intentionally performs no substitute UI action.
        }
    }
}
"@

# Mechanical source adaptation for the old SharpDX renderer API.
$source=$source.Replace('private (float, float) plotText(','private Tuple<float,float> plotText(')
$source=$source.Replace('return (0, 0);','return Tuple.Create(0f,0f);')
$source=[regex]::Replace($source,'return \(szTextSize\.Width, szTextSize\.Height\);','return Tuple.Create(szTextSize.Width,szTextSize.Height);')

[IO.File]::WriteAllText($dstExact,$source,$utf8)

# Wire source into the project.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P32 csproj P30 anchor missing'}
if(!$proj.Contains('<Compile Include="P32ThetisExactGadgets.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P32ThetisExactGadgets.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Hard source gates: if any gadget is still the P31 substitute, this build is invalid.
$verify=[IO.File]::ReadAllText($dstExact)
foreach($token in @(
 'internal class clsVfoDisplay : clsMeterItem',
 'internal class clsButtonBox : clsMeterItem',
 'internal class clsBandButtonBox : clsButtonBox',
 'internal class clsModeButtonBox : clsButtonBox',
 'internal class clsTunestepButtons : clsButtonBox',
 'private void renderVfoDisplay(',
 'private void renderButtonBox(',
 'public string AddVFODisplay(',
 'public string AddBandButtons(',
 'public string AddModeButtons(',
 'public string AddTunestepButtons('
)){
    if(!$verify.Contains($token)){throw "P32 exact-source gate missing: $token"}
}
if($proj.Contains('P31ModernThetisGadgets.cs')){throw 'P32 invalid: P31 custom gadget implementation still compiled'}

Write-Host "P32_THETIS_SOURCE_SHA=$ThetisSha"
Write-Host 'P32_SOURCE_OF_TRUTH=THETIS_ONLY'
Write-Host 'P32_GADGET_CODE=EXACT_EXTRACTED_THETIS_VFO_BAND_MODE_TUNESTEP'
Write-Host 'P32_RENDERER=EXACT_EXTRACTED_THETIS_RENDER_VFO_AND_BUTTONBOX'
Write-Host 'P32_ADAPTER_POLICY=POWERSDR_API_MAPPING_ONLY'
Write-Host 'P32_RX2=NOT_EXPOSED'
