[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P31 is a narrow backport on top of frozen P30 SAFE.
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

foreach($p in @($projPath,$mmPath,$ucPath,$configPath,$srcModern)){
    if(!(Test-Path $p)){throw "P31 required file missing: $p"}
}
Copy-Item $srcModern $dstModern -Force

# ---------- MeterManager: enable partial backport surface ----------
$mm=[IO.File]::ReadAllText($mmPath)
$mm=$mm.Replace("`r`n","`n").Replace("`n",$nl)

foreach($pair in @(
    @('internal static class MeterManager','internal static partial class MeterManager'),
    @('public class clsMeterItem','public partial class clsMeterItem'),
    @('public class clsMeter','public partial class clsMeter'),
    @('private class DXRenderer','private partial class DXRenderer')
)){
    if(!$mm.Contains($pair[0])){throw "P31 partial-class anchor missing: $($pair[0])"}
    $mm=$mm.Replace($pair[0],$pair[1])
}

$meterEnumAnchor='        CROSS,'+$nl+'        //HISTORY,'
if(!$mm.Contains($meterEnumAnchor)){throw 'P31 MeterType enum anchor missing'}
$mm=$mm.Replace($meterEnumAnchor,
    '        CROSS,'+$nl+
    '        VFO_DISPLAY,'+$nl+
    '        BAND_BUTTONS,'+$nl+
    '        MODE_BUTTONS,'+$nl+
    '        TUNESTEP_BUTTONS,'+$nl+
    '        //HISTORY,'
)

$itemEnumAnchor='                HISTORY,'+$nl+'                ITEM_GROUP'
if(!$mm.Contains($itemEnumAnchor)){throw 'P31 MeterItemType enum anchor missing'}
$mm=$mm.Replace($itemEnumAnchor,
    '                HISTORY,'+$nl+
    '                VFO_DISPLAY,'+$nl+
    '                BAND_BUTTONS,'+$nl+
    '                MODE_BUTTONS,'+$nl+
    '                TUNESTEP_BUTTONS,'+$nl+
    '                ITEM_GROUP'
)

$addAnchor='                    case MeterType.CROSS: AddCrossNeedle(nDelay, 0, out bBottom, restoreIg); break;'
if(!$mm.Contains($addAnchor)){throw 'P31 AddMeter CROSS anchor missing'}
$mm=$mm.Replace($addAnchor,
    $addAnchor+$nl+
    '                    case MeterType.VFO_DISPLAY: P31AddVFODisplay(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
    '                    case MeterType.BAND_BUTTONS: P31AddBandButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
    '                    case MeterType.MODE_BUTTONS: P31AddModeButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
    '                    case MeterType.TUNESTEP_BUTTONS: P31AddTunestepButtons(nDelay, 0, out bBottom, restoreIg); break;'
)

$nameAnchor='                case MeterType.CROSS: return "Cross Meter";'
if(!$mm.Contains($nameAnchor)){throw 'P31 MeterName anchor missing'}
$mm=$mm.Replace($nameAnchor,
    $nameAnchor+$nl+
    '                case MeterType.VFO_DISPLAY: return "VFO";'+$nl+
    '                case MeterType.BAND_BUTTONS: return "Band";'+$nl+
    '                case MeterType.MODE_BUTTONS: return "Mode";'+$nl+
    '                case MeterType.TUNESTEP_BUTTONS: return "Step";'
)

$typeAnchor='                case MeterType.CROSS: return 2;'
if(!$mm.Contains($typeAnchor)){throw 'P31 GetMeterTXRXType anchor missing'}
$mm=$mm.Replace($typeAnchor,
    $typeAnchor+$nl+
    '                case MeterType.VFO_DISPLAY: return 2;'+$nl+
    '                case MeterType.BAND_BUTTONS: return 2;'+$nl+
    '                case MeterType.MODE_BUTTONS: return 2;'+$nl+
    '                case MeterType.TUNESTEP_BUTTONS: return 2;'
)

$renderAnchor='                                case clsMeterItem.MeterItemType.MAGIC_EYE:'+$nl+
              '                                    renderEye(rect, mi, m);'+$nl+
              '                                    break;'
if(!$mm.Contains($renderAnchor)){throw 'P31 renderer switch anchor missing'}
$mm=$mm.Replace($renderAnchor,
    '                                case clsMeterItem.MeterItemType.VFO_DISPLAY:'+$nl+
    '                                    P31RenderVfoDisplay(rect, mi, m);'+$nl+
    '                                    break;'+$nl+
    '                                case clsMeterItem.MeterItemType.BAND_BUTTONS:'+$nl+
    '                                case clsMeterItem.MeterItemType.MODE_BUTTONS:'+$nl+
    '                                case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:'+$nl+
    '                                    P31RenderButtonBox(rect, mi, m);'+$nl+
    '                                    break;'+$nl+
    $renderAnchor
)

$eventAnchor='                _displayTarget.MouseUp += OnMouseUp;'
if(!$mm.Contains($eventAnchor)){throw 'P31 renderer mouse event anchor missing'}
$mm=$mm.Replace($eventAnchor,
    $eventAnchor+$nl+
    '                _displayTarget.MouseUp += P31DispatchMouseUp;'+$nl+
    '                _displayTarget.MouseDown += P31MouseDown;'+$nl+
    '                _displayTarget.MouseMove += P31MouseMove;'+$nl+
    '                _displayTarget.MouseWheel += P31MouseWheel;'+$nl+
    '                _displayTarget.MouseEnter += P31MouseEnter;'+$nl+
    '                _displayTarget.MouseLeave += P31MouseLeave;'
)
[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# ---------- ucMeter: later Thetis Lock + NoControls behaviour ----------
$uc=[IO.File]::ReadAllText($ucPath)
$uc=$uc.Replace("`r`n","`n").Replace("`n",$nl)

$fieldAnchor='        private bool _border;'
if(!$uc.Contains($fieldAnchor)){throw 'P31 ucMeter field anchor missing'}
$uc=$uc.Replace($fieldAnchor,
    $fieldAnchor+$nl+
    '        private bool _no_controls = false;'+$nl+
    '        private bool _locked = false;'
)

$lockHandler1='        private void pnlBar_MouseDown(object sender, MouseEventArgs e)'+$nl+'        {'
$lockHandler2='        private void pbGrab_MouseDown(object sender, MouseEventArgs e)'+$nl+'        {'
$lockHandler3='        private void lblRX_MouseDown(object sender, MouseEventArgs e)'+$nl+'        {'
foreach($handler in @($lockHandler1,$lockHandler2,$lockHandler3)){
    if(!$uc.Contains($handler)){throw "P31 ucMeter lock handler anchor missing: $handler"}
    $uc=$uc.Replace($handler,$handler+$nl+'            if (_locked) return;')
}

$moveAnchor='        private void picContainer_MouseMove(object sender, MouseEventArgs e)'+$nl+'        {'+$nl+'            bool bContains;'
if(!$uc.Contains($moveAnchor)){throw 'P31 ucMeter NoControls mouse anchor missing'}
$uc=$uc.Replace($moveAnchor,
    '        private void picContainer_MouseMove(object sender, MouseEventArgs e)'+$nl+
    '        {'+$nl+
    '            bool bContains;'+$nl+$nl+
    '            // Later Thetis behaviour: NoControls hides bar/grab; Shift temporarily exposes them.'+$nl+
    '            if (_no_controls && (Control.ModifierKeys & Keys.Shift) == 0)'+$nl+
    '            {'+$nl+
    '                if (pnlBar.Visible) pnlBar.Hide();'+$nl+
    '                if (pbGrab.Visible) pbGrab.Hide();'+$nl+
    '                return;'+$nl+
    '            }'
)

$resizeAnchor='            if (!_resizing)'+$nl+'            {'+$nl+'                bContains = pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));'
if(!$uc.Contains($resizeAnchor)){throw 'P31 ucMeter resize visibility anchor missing'}
$uc=$uc.Replace($resizeAnchor,
    '            if (!_resizing && !_locked)'+$nl+
    '            {'+$nl+
    '                bContains = pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));'
)

$propAnchor='        private void btnAxis_Click(object sender, EventArgs e)'
if(!$uc.Contains($propAnchor)){throw 'P31 ucMeter property insertion anchor missing'}
$props=@'
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool NoControls
        {
            get { return _no_controls; }
            set
            {
                _no_controls = value;
                if (_no_controls)
                {
                    pnlBar.Hide();
                    pbGrab.Hide();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;
                if (_locked) pbGrab.Hide();
            }
        }

'@
$uc=$uc.Replace($propAnchor,$props+$propAnchor)

$toStringAnchor='                Common.ColourToString(this.BackColor);'
if(!$uc.Contains($toStringAnchor)){throw 'P31 ucMeter serialization anchor missing'}
$uc=$uc.Replace($toStringAnchor,
    '                Common.ColourToString(this.BackColor) + "|" +'+$nl+
    '                NoControls.ToString() + "|" +'+$nl+
    '                Locked.ToString();'
)

$parseLen='                if(tmp.Length == 13)'
if(!$uc.Contains($parseLen)){throw 'P31 ucMeter parse length anchor missing'}
$uc=$uc.Replace($parseLen,'                if(tmp.Length == 13 || tmp.Length == 15)')

$backAnchor='                    if(bOk) this.BackColor = c;'
if(!$uc.Contains($backAnchor)){throw 'P31 ucMeter parse tail anchor missing'}
$uc=$uc.Replace($backAnchor,
    $backAnchor+$nl+
    '                    if (bOk && tmp.Length == 15)'+$nl+
    '                    {'+$nl+
    '                        if (bOk) bOk = bool.TryParse(tmp[13], out tmpBool);'+$nl+
    '                        if (bOk) NoControls = tmpBool;'+$nl+
    '                        if (bOk) bOk = bool.TryParse(tmp[14], out tmpBool);'+$nl+
    '                        if (bOk) Locked = tmpBool;'+$nl+
    '                    }'
)
[IO.File]::WriteAllText($ucPath,$uc,$utf8)

# ---------- P27 configuration window: expose Lock and No Title Bar ----------
$cfg=[IO.File]::ReadAllText($configPath)
$cfg=$cfg.Replace("`r`n","`n").Replace("`n",$nl)

$cfgField='        private readonly CheckBoxTS chkContainerBorder;'
if(!$cfg.Contains($cfgField)){throw 'P31 config field anchor missing'}
$cfg=$cfg.Replace($cfgField,
    $cfgField+$nl+
    '        private readonly CheckBoxTS chkContainerLocked;'+$nl+
    '        private readonly CheckBoxTS chkContainerNoTitleBar;'
)

$cfgCtor='            groupBoxTS28.Controls.Add(chkContainerBorder);'
if(!$cfg.Contains($cfgCtor)){throw 'P31 config constructor anchor missing'}
$cfg=$cfg.Replace($cfgCtor,
    $cfgCtor+$nl+$nl+
    '            chkContainerLocked = NewCheck("chkContainerLocked", "Lock", 18, 92);'+$nl+
    '            chkContainerLocked.CheckedChanged += chkContainerLocked_CheckedChanged;'+$nl+
    '            groupBoxTS28.Controls.Add(chkContainerLocked);'+$nl+$nl+
    '            chkContainerNoTitleBar = NewCheck("chkContainerNoTitleBar", "No Title Bar", 102, 92);'+$nl+
    '            chkContainerNoTitleBar.CheckedChanged += chkContainerNoTitleBar_CheckedChanged;'+$nl+
    '            groupBoxTS28.Controls.Add(chkContainerNoTitleBar);'
)

$enableAnchor='                chkContainerBorder.Enabled = enabled;'
if(!$cfg.Contains($enableAnchor)){throw 'P31 config enable anchor missing'}
$cfg=$cfg.Replace($enableAnchor,
    $enableAnchor+$nl+
    '                chkContainerLocked.Enabled = enabled;'+$nl+
    '                chkContainerNoTitleBar.Enabled = enabled;'
)

$selectAnchor='                chkContainerBorder.Checked = MeterManager.ContainerHasBorder(cci.ID);'
if(!$cfg.Contains($selectAnchor)){throw 'P31 config selection anchor missing'}
$cfg=$cfg.Replace($selectAnchor,
    $selectAnchor+$nl+
    '                chkContainerLocked.Checked = MeterManager.ContainerLocked(cci.ID);'+$nl+
    '                chkContainerNoTitleBar.Checked = MeterManager.ContainerNoTitleBar(cci.ID);'
)

$handlerAnchor='        private void clrbtnContainerBackground_Changed(object sender, EventArgs e)'
if(!$cfg.Contains($handlerAnchor)){throw 'P31 config handler insertion anchor missing'}
$handlers=@'
        private void chkContainerLocked_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null) MeterManager.LockContainer(cci.ID, chkContainerLocked.Checked);
        }

        private void chkContainerNoTitleBar_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null) MeterManager.ContainerNoTitleBar(cci.ID, chkContainerNoTitleBar.Checked);
        }

'@
$cfg=$cfg.Replace($handlerAnchor,$handlers+$handlerAnchor)
[IO.File]::WriteAllText($configPath,$cfg,$utf8)

# ---------- Project include ----------
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P31 csproj P30 anchor missing'}
if(!$proj.Contains('<Compile Include="P31ModernThetisGadgets.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P31ModernThetisGadgets.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# ---------- Hard gates ----------
$verifyMM=[IO.File]::ReadAllText($mmPath)
$verifyUC=[IO.File]::ReadAllText($ucPath)
$verifyCfg=[IO.File]::ReadAllText($configPath)
$verifyModern=[IO.File]::ReadAllText($dstModern)

foreach($token in @(
 'VFO_DISPLAY','BAND_BUTTONS','MODE_BUTTONS','TUNESTEP_BUTTONS',
 'P31RenderVfoDisplay','P31RenderButtonBox','P31DispatchMouseUp'
)){
 if(!$verifyMM.Contains($token)){throw "P31 MeterManager gate missing: $token"}
}
foreach($token in @(
 'clsVfoDisplay','clsBandButtonBox','clsModeButtonBox','clsTunestepButtons',
 'VFOAFreq','RX1Band','RX1DSPMode','TuneStepIndex'
)){
 if(!$verifyModern.Contains($token)){throw "P31 modern gadget gate missing: $token"}
}
foreach($token in @('public bool NoControls','public bool Locked','tmp.Length == 13 || tmp.Length == 15')){
 if(!$verifyUC.Contains($token)){throw "P31 ucMeter gate missing: $token"}
}
foreach($token in @('chkContainerLocked','chkContainerNoTitleBar','ContainerLocked','ContainerNoTitleBar')){
 if(!$verifyCfg.Contains($token)){throw "P31 config gate missing: $token"}
}

Write-Host 'P31_BASE=P30_SAFE_5fb179f357771f2c6aebcfdd7f13136d8c5f1e72'
Write-Host 'P31_GADGETS=VFO_DISPLAY,BAND_BUTTONS,MODE_BUTTONS,TUNESTEP_BUTTONS'
Write-Host 'P31_CONTAINER=LOCKED,NO_TITLE_BAR'
Write-Host 'P31_RX2=NOT_EXPOSED'
Write-Host 'P31_P30_RX_TX_DARK_SKINS=UNCHANGED'
