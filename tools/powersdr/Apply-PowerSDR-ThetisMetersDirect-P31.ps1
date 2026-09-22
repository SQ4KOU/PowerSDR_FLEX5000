[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P31: start strictly from P30 SAFE functionality.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P30.ps1') -SourceRoot $SourceRoot

$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$mmPath=Join-Path $consoleDir 'P25_MeterManager.cs'
$ucPath=Join-Path $consoleDir 'P25_ucMeter.cs'
$configPath=Join-Path $consoleDir 'P27ThetisMetersConfigForm.cs'
$srcModern=Join-Path $PSScriptRoot 'P31ModernThetisGadgets.cs'
$dstModern=Join-Path $consoleDir 'P31ModernThetisGadgets.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

foreach($p in @($mmPath,$ucPath,$configPath,$projPath,$srcModern))
{
    if(!(Test-Path $p)){throw "P31 required file missing: $p"}
}
Copy-Item $srcModern $dstModern -Force

# ---------- MeterManager: only the four later Thetis gadget types ----------
$mm=[IO.File]::ReadAllText($mmPath)

foreach($pair in @(
    @('internal static class MeterManager','internal static partial class MeterManager'),
    @('public class clsMeterItem','public partial class clsMeterItem'),
    @('public class clsMeter','public partial class clsMeter'),
    @('private class DXRenderer','private partial class DXRenderer')
))
{
    if(!$mm.Contains($pair[0])){throw "P31 partial-class anchor missing: $($pair[0])"}
    $mm=$mm.Replace($pair[0],$pair[1])
}

$meterEnumMarker='        CROSS,'
if(!$mm.Contains($meterEnumMarker)){throw 'P31 MeterType enum anchor missing'}
if(!$mm.Contains('        VFO_DISPLAY,'))
{
    $mm=$mm.Replace($meterEnumMarker,$meterEnumMarker+$nl+
        '        // later Thetis gadgets backported in P31'+$nl+
        '        VFO_DISPLAY,'+$nl+
        '        BAND_BUTTONS,'+$nl+
        '        MODE_BUTTONS,'+$nl+
        '        TUNESTEP_BUTTONS,')
}

$itemEnumMarker='                ITEM_GROUP'
if(!$mm.Contains($itemEnumMarker)){throw 'P31 MeterItemType enum anchor missing'}
if(!$mm.Contains('                VFO_DISPLAY,'))
{
    $mm=$mm.Replace($itemEnumMarker,
        '                VFO_DISPLAY,'+$nl+
        '                BAND_BUTTONS,'+$nl+
        '                MODE_BUTTONS,'+$nl+
        '                TUNESTEP_BUTTONS,'+$nl+
        $itemEnumMarker)
}


$typeMarker='                case MeterType.CROSS: return 2;'
if(!$mm.Contains($typeMarker)){throw 'P31 GetMeterTXRXType anchor missing'}
if(!$mm.Contains('                case MeterType.VFO_DISPLAY: return 2;'))
{
    $mm=$mm.Replace($typeMarker,$typeMarker+$nl+
        '                case MeterType.VFO_DISPLAY: return 2;'+$nl+
        '                case MeterType.BAND_BUTTONS: return 2;'+$nl+
        '                case MeterType.MODE_BUTTONS: return 2;'+$nl+
        '                case MeterType.TUNESTEP_BUTTONS: return 2;')
}

$nameMarker='                case MeterType.CROSS: return "Cross Meter";'
if(!$mm.Contains($nameMarker)){throw 'P31 MeterName anchor missing'}
if(!$mm.Contains('                case MeterType.VFO_DISPLAY: return "VFO Display";'))
{
    $mm=$mm.Replace($nameMarker,$nameMarker+$nl+
        '                case MeterType.VFO_DISPLAY: return "VFO Display";'+$nl+
        '                case MeterType.BAND_BUTTONS: return "Band Buttons";'+$nl+
        '                case MeterType.MODE_BUTTONS: return "Mode Buttons";'+$nl+
        '                case MeterType.TUNESTEP_BUTTONS: return "Tune Step Buttons";')
}

$addMarker='                    case MeterType.CROSS: AddCrossNeedle(nDelay, 0, out bBottom, restoreIg); break;'
if(!$mm.Contains($addMarker)){throw 'P31 AddMeter switch anchor missing'}
if(!$mm.Contains('                    case MeterType.VFO_DISPLAY: P31AddVFODisplay'))
{
    $mm=$mm.Replace($addMarker,$addMarker+$nl+
        '                    case MeterType.VFO_DISPLAY: P31AddVFODisplay(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
        '                    case MeterType.BAND_BUTTONS: P31AddBandButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
        '                    case MeterType.MODE_BUTTONS: P31AddModeButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
        '                    case MeterType.TUNESTEP_BUTTONS: P31AddTunestepButtons(nDelay, 0, out bBottom, restoreIg); break;')
}

$renderPattern='(?ms)(\s*case clsMeterItem\.MeterItemType\.MAGIC_EYE:\s*\r?\n\s*renderEye\(rect, mi, m\);\s*\r?\n\s*break;)'
if(-not [regex]::IsMatch($mm,$renderPattern)){throw 'P31 drawMeters switch anchor missing'}
if(!$mm.Contains('case clsMeterItem.MeterItemType.VFO_DISPLAY:'))
{
    $renderAdd='$1'+$nl+
        '                                case clsMeterItem.MeterItemType.VFO_DISPLAY:'+$nl+
        '                                    P31RenderVfoDisplay(rect, mi, m);'+$nl+
        '                                    break;'+$nl+
        '                                case clsMeterItem.MeterItemType.BAND_BUTTONS:'+$nl+
        '                                case clsMeterItem.MeterItemType.MODE_BUTTONS:'+$nl+
        '                                case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:'+$nl+
        '                                    P31RenderButtonBox(rect, mi, m);'+$nl+
        '                                    break;'
    $mm=[regex]::Replace($mm,$renderPattern,$renderAdd,1)
}

$eventMarker='                _displayTarget.MouseUp += OnMouseUp;'
if(!$mm.Contains($eventMarker)){throw 'P31 DXRenderer event anchor missing'}
if(!$mm.Contains('_displayTarget.MouseDown += P31MouseDown;'))
{
    $mm=$mm.Replace($eventMarker,$eventMarker+$nl+
        '                _displayTarget.MouseDown += P31MouseDown;'+$nl+
        '                _displayTarget.MouseUp += P31MouseUp;'+$nl+
        '                _displayTarget.MouseMove += P31MouseMove;'+$nl+
        '                _displayTarget.MouseWheel += P31MouseWheel;'+$nl+
        '                _displayTarget.MouseEnter += P31MouseEnter;'+$nl+
        '                _displayTarget.MouseLeave += P31MouseLeave;')
}

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# ---------- ucMeter: lock + no-title-bar, persisted backward-compatibly ----------
$uc=[IO.File]::ReadAllText($ucPath)

$ctorAnchor='            _border = true;'
if(!$uc.Contains($ctorAnchor)){throw 'P31 ucMeter constructor anchor missing'}
$uc=$uc.Replace($ctorAnchor,$ctorAnchor+$nl+'            _locked = false;'+$nl+'            _noControls = false;')

$fieldAnchor='        private bool _border;'
if(!$uc.Contains($fieldAnchor)){throw 'P31 ucMeter field anchor missing'}
$uc=$uc.Replace($fieldAnchor,$fieldAnchor+$nl+'        private bool _locked;'+$nl+'        private bool _noControls;')

foreach($handler in @(
    'pnlBar_MouseDown',
    'pnlBar_MouseMove',
    'pbGrab_MouseDown',
    'pbGrab_MouseMove',
    'lblRX_MouseDown',
    'lblRX_MouseMove'
))
{
    $pattern='(?m)(\s*private void '+[regex]::Escape($handler)+'\(object sender, MouseEventArgs e\)\s*\r?\n\s*\{)'
    if(-not [regex]::IsMatch($uc,$pattern)){throw "P31 ucMeter lock handler anchor missing: $handler"}
    $uc=[regex]::Replace($uc,$pattern,'$1'+$nl+'            if (_locked) return;',1)
}

$movePattern='(?m)(\s*private void picContainer_MouseMove\(object sender, MouseEventArgs e\)\s*\r?\n\s*\{)\s*\r?\n(\s*)bool bContains;'
if(-not [regex]::IsMatch($uc,$movePattern)){throw 'P31 NoControls hover anchor missing'}
$moveReplace='$1'+$nl+
             '            if (_noControls && (ModifierKeys & Keys.Shift) != Keys.Shift)'+$nl+
             '            {'+$nl+
             '                pnlBar.Hide();'+$nl+
             '                pbGrab.Hide();'+$nl+
             '                return;'+$nl+
             '            }'+$nl+$nl+
             '            bool bContains;'
$uc=[regex]::Replace($uc,$movePattern,$moveReplace,1)

$propertyAnchor='        private void btnAxis_Click(object sender, EventArgs e)'
$properties=@'
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;
                if (_locked)
                {
                    _dragging = false;
                    _resizing = false;
                    pbGrab.Hide();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool NoControls
        {
            get { return _noControls; }
            set
            {
                _noControls = value;
                if (_noControls)
                {
                    pnlBar.Hide();
                    pbGrab.Hide();
                }
            }
        }

'@
if(!$uc.Contains($propertyAnchor)){throw 'P31 ucMeter property insertion anchor missing'}
$uc=$uc.Replace($propertyAnchor,$properties+$propertyAnchor)

$oldTail='                Common.ColourToString(this.BackColor);'
$newTail='                Common.ColourToString(this.BackColor) + "|" +'+$nl+
         '                NoControls.ToString() + "|" +'+$nl+
         '                Locked.ToString();'
if(!$uc.Contains($oldTail)){throw 'P31 ucMeter ToString anchor missing'}
$uc=$uc.Replace($oldTail,$newTail)

$oldLen='                if(tmp.Length == 13)'
if(!$uc.Contains($oldLen)){throw 'P31 ucMeter TryParse length anchor missing'}
$uc=$uc.Replace($oldLen,'                if(tmp.Length == 13 || tmp.Length == 15)')

$parseAnchor=@'
                    Color c = Common.ColourFromString(tmp[12]);
                    bOk = c != System.Drawing.Color.Transparent;
                    if(bOk) this.BackColor = c;
'@
$parseNew=@'
                    Color c = Common.ColourFromString(tmp[12]);
                    bOk = c != System.Drawing.Color.Transparent;
                    if(bOk) this.BackColor = c;

                    // P31 fields are appended, so all P30/P28 13-field records remain valid.
                    if (bOk && tmp.Length >= 15)
                    {
                        bool noControls = false;
                        bool locked = false;
                        bOk = bool.TryParse(tmp[13], out noControls);
                        if (bOk) NoControls = noControls;
                        if (bOk) bOk = bool.TryParse(tmp[14], out locked);
                        if (bOk) Locked = locked;
                    }
'@
if(!$uc.Contains($parseAnchor)){throw 'P31 ucMeter TryParse body anchor missing'}
$uc=$uc.Replace($parseAnchor,$parseNew)

[IO.File]::WriteAllText($ucPath,$uc,$utf8)

# ---------- P27 configuration surface: add the later container controls ----------
$cfg=[IO.File]::ReadAllText($configPath)

$fieldCfg='        private readonly CheckBoxTS chkContainerBorder;'
if(!$cfg.Contains($fieldCfg)){throw 'P31 config field anchor missing'}
$cfg=$cfg.Replace($fieldCfg,$fieldCfg+$nl+
'        private readonly CheckBoxTS chkContainerLocked;'+$nl+
'        private readonly CheckBoxTS chkContainerNoTitleBar;')

$uiAnchor='            groupBoxTS28.Controls.Add(chkContainerBorder);'
$uiNew=$uiAnchor+$nl+$nl+
'            chkContainerLocked = NewCheck("chkContainerLocked", "Lock", 237, 77);'+$nl+
'            toolTip1.SetToolTip(chkContainerLocked, "Lock meter window position and size");'+$nl+
'            chkContainerLocked.CheckedChanged += chkContainerLocked_CheckedChanged;'+$nl+
'            groupBoxTS28.Controls.Add(chkContainerLocked);'+$nl+$nl+
'            chkContainerNoTitleBar = NewCheck("chkContainerNoTitleBar", "No Title Bar", 237, 100);'+$nl+
'            toolTip1.SetToolTip(chkContainerNoTitleBar, "Hide meter title/control bar; hold Shift over the meter for temporary access");'+$nl+
'            chkContainerNoTitleBar.CheckedChanged += chkContainerNoTitleBar_CheckedChanged;'+$nl+
'            groupBoxTS28.Controls.Add(chkContainerNoTitleBar);'
if(!$cfg.Contains($uiAnchor)){throw 'P31 config UI anchor missing'}
$cfg=$cfg.Replace($uiAnchor,$uiNew)

$enableAnchor='                chkContainerBorder.Enabled = enabled;'
if(!$cfg.Contains($enableAnchor)){throw 'P31 config enable anchor missing'}
$cfg=$cfg.Replace($enableAnchor,$enableAnchor+$nl+
'                chkContainerLocked.Enabled = enabled;'+$nl+
'                chkContainerNoTitleBar.Enabled = enabled;')

$loadAnchor='                chkContainerBorder.Checked = MeterManager.ContainerHasBorder(cci.ID);'
if(!$cfg.Contains($loadAnchor)){throw 'P31 config load anchor missing'}
$cfg=$cfg.Replace($loadAnchor,$loadAnchor+$nl+
'                chkContainerLocked.Checked = MeterManager.ContainerLocked(cci.ID);'+$nl+
'                chkContainerNoTitleBar.Checked = MeterManager.ContainerNoTitleBar(cci.ID);')

$handlerAnchor='        private void clrbtnContainerBackground_Changed(object sender, EventArgs e)'
$handlers=@'
        private void chkContainerLocked_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null)
            {
                MeterManager.LockContainer(cci.ID, chkContainerLocked.Checked);
                console.P27SaveMetersConfiguration();
            }
        }

        private void chkContainerNoTitleBar_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null)
            {
                MeterManager.ContainerNoTitleBar(cci.ID, chkContainerNoTitleBar.Checked);
                console.P27SaveMetersConfiguration();
            }
        }

'@
if(!$cfg.Contains($handlerAnchor)){throw 'P31 config handler insertion anchor missing'}
$cfg=$cfg.Replace($handlerAnchor,$handlers+$handlerAnchor)

$selAnchor='        private void lstMetersInUse_SelectedIndexChanged(object sender, EventArgs e)'+$nl+'        {'+$nl+
'            bool enabled = lstMetersInUse.SelectedIndex >= 0;'+$nl+
'            if (enabled) updateItemSettingsControlsForSelected();'
$selNew='        private void lstMetersInUse_SelectedIndexChanged(object sender, EventArgs e)'+$nl+'        {'+$nl+
'            bool enabled = lstMetersInUse.SelectedIndex >= 0;'+$nl+
'            clsMeterTypeComboboxItem selectedItem = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;'+$nl+
'            bool modern = selectedItem != null && P31IsModernGadget(selectedItem.MeterType);'+$nl+
'            if (enabled && !modern) updateItemSettingsControlsForSelected();'
if(!$cfg.Contains($selAnchor)){throw 'P31 config selected-item anchor missing'}
$cfg=$cfg.Replace($selAnchor,$selNew)

$grpAnchor='            grpMeterItemSettings.Enabled = enabled;'
if(!$cfg.Contains($grpAnchor)){throw 'P31 config settings group anchor missing'}
$cfg=$cfg.Replace($grpAnchor,'            grpMeterItemSettings.Enabled = enabled && !modern;')

$helperAnchor='        private void lstMetersAvailable_DoubleClick(object sender, EventArgs e)'
$helper=@'
        private static bool P31IsModernGadget(MeterType mt)
        {
            return mt == MeterType.VFO_DISPLAY ||
                   mt == MeterType.BAND_BUTTONS ||
                   mt == MeterType.MODE_BUTTONS ||
                   mt == MeterType.TUNESTEP_BUTTONS;
        }

'@
if(!$cfg.Contains($helperAnchor)){throw 'P31 config helper anchor missing'}
$cfg=$cfg.Replace($helperAnchor,$helper+$helperAnchor)

[IO.File]::WriteAllText($configPath,$cfg,$utf8)

# ---------- Project compile ----------
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P31 csproj P30 bridge anchor missing'}
if(!$proj.Contains('<Compile Include="P31ModernThetisGadgets.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P31ModernThetisGadgets.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# ---------- Hard scope/regression gates ----------
$verifyMM=[IO.File]::ReadAllText($mmPath)
$verifyUC=[IO.File]::ReadAllText($ucPath)
$verifyCfg=[IO.File]::ReadAllText($configPath)
$verifyModern=[IO.File]::ReadAllText($dstModern)

foreach($token in @(
    'P30ReadTxForwardWatts',
    'P30ReadTxSWR',
    'P25ReadRx1SignalDbm',
    'ananMM-dark'
)){
    if(!$verifyMM.Contains($token)){throw "P31 P30 regression gate missing: $token"}
}
foreach($token in @(
    'VFO_DISPLAY',
    'BAND_BUTTONS',
    'MODE_BUTTONS',
    'TUNESTEP_BUTTONS',
    'P31RenderVfoDisplay',
    'P31RenderButtonBox'
)){
    if(!$verifyMM.Contains($token) -and !$verifyModern.Contains($token)){throw "P31 gadget gate missing: $token"}
}
foreach($token in @(
    'public bool Locked',
    'public bool NoControls',
    'tmp.Length == 13 || tmp.Length == 15',
    'ModifierKeys & Keys.Shift'
)){
    if(!$verifyUC.Contains($token)){throw "P31 container gate missing: $token"}
}
foreach($token in @(
    'chkContainerLocked',
    'chkContainerNoTitleBar',
    'ContainerLocked',
    'ContainerNoTitleBar'
)){
    if(!$verifyCfg.Contains($token)){throw "P31 config gate missing: $token"}
}

Write-Host 'P31_BASE=P30_SAFE_5fb179f357771f2c6aebcfdd7f13136d8c5f1e72'
Write-Host 'P31_GADGETS=VFO_DISPLAY,BAND_BUTTONS,MODE_BUTTONS,TUNESTEP_BUTTONS'
Write-Host 'P31_CONTAINER_CONTROLS=LOCK,NO_TITLE_BAR'
Write-Host 'P31_NO_TITLE_BAR_SHIFT_ACCESS=ENABLED'
Write-Host 'P31_RX2=NOT_EXPOSED'
Write-Host 'P31_RX_TX_DARK=P30_UNCHANGED'
