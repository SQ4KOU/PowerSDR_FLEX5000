[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P31: P30 remains the verified base. Add only modern Thetis gadgets and
# modern container Lock/No Title Bar behavior.
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
$nl="`n"  # pinned Thetis/P27 sources are LF; do not use Windows CRLF for source anchors

foreach($p in @($projPath,$mmPath,$ucPath,$configPath,$srcModern))
{
    if(!(Test-Path $p)){throw "P31 required file missing: $p"}
}
Copy-Item $srcModern $dstModern -Force

# ---------------------------------------------------------------------------
# MeterManager: make only the existing classes partial and add modern enum
# values/dispatch hooks. The existing P30 implementation remains in place.
# ---------------------------------------------------------------------------
$mm=[IO.File]::ReadAllText($mmPath)
$mm=$mm.Replace("`r`n","`n")

foreach($pair in @(
    @('internal static class MeterManager','internal static partial class MeterManager'),
    @('        public class clsMeterItem','        public partial class clsMeterItem'),
    @('        public class clsMeter','        public partial class clsMeter'),
    @('        private class DXRenderer','        private partial class DXRenderer')
))
{
    if(!$mm.Contains($pair[0])){throw "P31 partial-class anchor missing: $($pair[0])"}
    $mm=$mm.Replace($pair[0],$pair[1])
}

$meterEnumPattern='(?m)^(\s*)//HISTORY,\r?\n(\s*)LAST\s*$'
if(-not [regex]::IsMatch($mm,$meterEnumPattern)){throw 'P31 MeterType enum anchor missing'}
$mm=[regex]::Replace($mm,$meterEnumPattern,
    '$1// modern Thetis gadgets backported in P31'+$nl+
    '$1VFO_DISPLAY,'+$nl+
    '$1BAND_BUTTONS,'+$nl+
    '$1MODE_BUTTONS,'+$nl+
    '$1TUNESTEP_BUTTONS,'+$nl+
    '$1//HISTORY,'+$nl+
    '$2LAST',1)

$itemEnumPattern='(?m)^(\s*)HISTORY,\r?\n(\s*)ITEM_GROUP\s*$'
if(-not [regex]::IsMatch($mm,$itemEnumPattern)){throw 'P31 MeterItemType enum anchor missing'}
$mm=[regex]::Replace($mm,$itemEnumPattern,
    '$1HISTORY,'+$nl+
    '$1VFO_DISPLAY,'+$nl+
    '$1BAND_BUTTONS,'+$nl+
    '$1MODE_BUTTONS,'+$nl+
    '$1TUNESTEP_BUTTONS,'+$nl+
    '$2ITEM_GROUP',1)

$typeAnchor='                case MeterType.MAGIC_EYE: return 2;'
if(!$mm.Contains($typeAnchor)){throw 'P31 GetMeterTXRXType anchor missing'}
$mm=$mm.Replace($typeAnchor,
    '                case MeterType.VFO_DISPLAY: return 2;'+$nl+
    '                case MeterType.BAND_BUTTONS: return 2;'+$nl+
    '                case MeterType.MODE_BUTTONS: return 2;'+$nl+
    '                case MeterType.TUNESTEP_BUTTONS: return 2;'+$nl+
    $typeAnchor)

$nameAnchor='                case MeterType.MAGIC_EYE: return "Magic Eye";'
if(!$mm.Contains($nameAnchor)){throw 'P31 MeterName anchor missing'}
$mm=$mm.Replace($nameAnchor,
    '                case MeterType.VFO_DISPLAY: return "VFO Display";'+$nl+
    '                case MeterType.BAND_BUTTONS: return "Band Buttons";'+$nl+
    '                case MeterType.MODE_BUTTONS: return "Mode Buttons";'+$nl+
    '                case MeterType.TUNESTEP_BUTTONS: return "Tunestep Buttons";'+$nl+
    $nameAnchor)

$addAnchor='                    case MeterType.SWR: AddSWRBar(nDelay, 0, out bBottom, restoreIg); break;'
if(!$mm.Contains($addAnchor)){throw 'P31 AddMeter switch anchor missing'}
$mm=$mm.Replace($addAnchor,
    $addAnchor+$nl+
    '                    case MeterType.VFO_DISPLAY: P31AddVFODisplay(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
    '                    case MeterType.BAND_BUTTONS: P31AddBandButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
    '                    case MeterType.MODE_BUTTONS: P31AddModeButtons(nDelay, 0, out bBottom, restoreIg); break;'+$nl+
    '                    case MeterType.TUNESTEP_BUTTONS: P31AddTunestepButtons(nDelay, 0, out bBottom, restoreIg); break;')

$drawAnchor='                                case clsMeterItem.MeterItemType.MAGIC_EYE:'
if(!$mm.Contains($drawAnchor)){throw 'P31 drawMeters anchor missing'}
$drawNew=
'                                case clsMeterItem.MeterItemType.VFO_DISPLAY:'+$nl+
'                                    P31RenderVfoDisplay(rect, mi, m);'+$nl+
'                                    break;'+$nl+
'                                case clsMeterItem.MeterItemType.BAND_BUTTONS:'+$nl+
'                                case clsMeterItem.MeterItemType.MODE_BUTTONS:'+$nl+
'                                case clsMeterItem.MeterItemType.TUNESTEP_BUTTONS:'+$nl+
'                                    P31RenderButtonBox(rect, mi, m);'+$nl+
'                                    break;'+$nl+
$drawAnchor
$mm=$mm.Replace($drawAnchor,$drawNew)

$subAnchor='                _displayTarget.MouseUp += OnMouseUp;'
if(!$mm.Contains($subAnchor)){throw 'P31 DX mouse subscription anchor missing'}
$mm=$mm.Replace($subAnchor,
    $subAnchor+$nl+
    '                _displayTarget.MouseDown += P31MouseDown;'+$nl+
    '                _displayTarget.MouseMove += P31MouseMove;'+$nl+
    '                _displayTarget.MouseWheel += P31MouseWheel;'+$nl+
    '                _displayTarget.MouseEnter += P31MouseEnter;'+$nl+
    '                _displayTarget.MouseLeave += P31MouseLeave;')

$unsubAnchor='                    _displayTarget.MouseUp -= OnMouseUp;'
if(!$mm.Contains($unsubAnchor)){throw 'P31 DX mouse unsubscribe anchor missing'}
$mm=$mm.Replace($unsubAnchor,
    $unsubAnchor+$nl+
    '                    _displayTarget.MouseDown -= P31MouseDown;'+$nl+
    '                    _displayTarget.MouseMove -= P31MouseMove;'+$nl+
    '                    _displayTarget.MouseWheel -= P31MouseWheel;'+$nl+
    '                    _displayTarget.MouseEnter -= P31MouseEnter;'+$nl+
    '                    _displayTarget.MouseLeave -= P31MouseLeave;')

$mouseUpAnchor='            private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)'+$nl+'            {'
if(!$mm.Contains($mouseUpAnchor)){throw 'P31 OnMouseUp anchor missing'}
$mm=$mm.Replace($mouseUpAnchor,$mouseUpAnchor+$nl+'                P31DispatchMouseUp(sender, e);')

[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# ---------------------------------------------------------------------------
# ucMeter: backport NoControls (No Title Bar) + Shift quick access and Locked
# persistence while keeping all original 13 fields compatible.
# ---------------------------------------------------------------------------
$uc=[IO.File]::ReadAllText($ucPath)
$uc=$uc.Replace("`r`n","`n")

$fieldAnchor='        private bool _border;'
if(!$uc.Contains($fieldAnchor)){throw 'P31 ucMeter field anchor missing'}
$uc=$uc.Replace($fieldAnchor,$fieldAnchor+$nl+'        private bool _no_controls;'+$nl+'        private bool _locked;')

$initAnchor='            _border = true;'
if(!$uc.Contains($initAnchor)){throw 'P31 ucMeter init anchor missing'}
$uc=$uc.Replace($initAnchor,$initAnchor+$nl+'            _no_controls = false;'+$nl+'            _locked = false;')

$propertyAnchor='        private void btnAxis_Click(object sender, EventArgs e)'
if(!$uc.Contains($propertyAnchor)){throw 'P31 ucMeter property anchor missing'}
$properties=@'
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; }
        }

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

'@
$uc=$uc.Replace($propertyAnchor,$properties+$propertyAnchor)

$moveDecl='            bool bContains;'
if(!$uc.Contains($moveDecl)){throw 'P31 ucMeter mouse declaration anchor missing'}
$uc=$uc.Replace($moveDecl,
    $moveDecl+$nl+'            bool noControls = _no_controls && (Control.ModifierKeys & Keys.Shift) != Keys.Shift;')

$barHit='                bContains = pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition));'
if(!$uc.Contains($barHit)){throw 'P31 ucMeter top-bar hit-test anchor missing'}
$uc=$uc.Replace($barHit,
    '                bContains = !noControls && pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition));')

$grabHit='                bContains = pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));'
if(!$uc.Contains($grabHit)){throw 'P31 ucMeter resize-grabber hit-test anchor missing'}
$uc=$uc.Replace($grabHit,
    '                bContains = !noControls && pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));')

$toOld='                Common.ColourToString(this.BackColor);'
$toNew='                Common.ColourToString(this.BackColor) + "|" +'+$nl+
       '                NoControls.ToString().ToLower() + "|" +'+$nl+
       '                Locked.ToString().ToLower();'
if(!$uc.Contains($toOld)){throw 'P31 ucMeter ToString anchor missing'}
$uc=$uc.Replace($toOld,$toNew)

$parseLen='                if(tmp.Length == 13)'
if(!$uc.Contains($parseLen)){throw 'P31 ucMeter TryParse length anchor missing'}
$uc=$uc.Replace($parseLen,'                if(tmp.Length >= 13 && tmp.Length <= 15)')

$parseAnchor='                    if(bOk) this.BackColor = c;'
if(!$uc.Contains($parseAnchor)){throw 'P31 ucMeter TryParse tail anchor missing'}
$parseTail=@'
                    if(bOk) this.BackColor = c;
                    if (bOk && tmp.Length > 13)
                    {
                        bool noControls = false;
                        bOk = bool.TryParse(tmp[13], out noControls);
                        if (bOk) NoControls = noControls;
                    }
                    if (bOk && tmp.Length > 14)
                    {
                        bool locked = false;
                        bOk = bool.TryParse(tmp[14], out locked);
                        if (bOk) Locked = locked;
                    }
'@
$parseTail=$parseTail.Replace("`r`n","`n")
$uc=$uc.Replace($parseAnchor,$parseTail)

[IO.File]::WriteAllText($ucPath,$uc,$utf8)

# ---------------------------------------------------------------------------
# P27 config window: add the exact newer-Thetis Lock semantics and No Title Bar.
# ---------------------------------------------------------------------------
$cfg=[IO.File]::ReadAllText($configPath)
$cfg=$cfg.Replace("`r`n","`n")

$fieldCfg='        private readonly CheckBoxTS chkContainerBorder;'
if(!$cfg.Contains($fieldCfg)){throw 'P31 config field anchor missing'}
$cfg=$cfg.Replace($fieldCfg,$fieldCfg+$nl+
    '        private readonly CheckBoxTS chkLockContainer;'+$nl+
    '        private readonly CheckBoxTS chkContainerNoTitle;')

$controlAnchor='            groupBoxTS28.Controls.Add(chkContainerBorder);'
if(!$cfg.Contains($controlAnchor)){throw 'P31 config control anchor missing'}
$controlBlock=@'

            chkLockContainer = NewCheck("chkLockContainer", "Lock", 313, 20);
            toolTip1.SetToolTip(chkLockContainer, "Lock the container to prevent removal and to prevent add/remove of items. You can still make adjustments to items");
            chkLockContainer.CheckedChanged += chkLockContainer_CheckedChanged;
            groupBoxTS28.Controls.Add(chkLockContainer);

            chkContainerNoTitle = NewCheck("chkContainerNoTitle", "No Title Bar", 313, 44);
            toolTip1.SetToolTip(chkContainerNoTitle, "Hide the meter top control bar and resize grabber. Hold Shift over the meter for temporary access.");
            chkContainerNoTitle.CheckedChanged += chkContainerNoTitle_CheckedChanged;
            groupBoxTS28.Controls.Add(chkContainerNoTitle);
'@
$cfg=$cfg.Replace($controlAnchor,$controlAnchor+$controlBlock)

$enableAnchor='                chkContainerBorder.Enabled = enabled;'+$nl+'                clrbtnContainerBackground.Enabled = enabled;'
if(!$cfg.Contains($enableAnchor)){throw 'P31 config enable anchor missing'}
$cfg=$cfg.Replace($enableAnchor,
    '                chkContainerBorder.Enabled = enabled;'+$nl+
    '                chkLockContainer.Enabled = enabled;'+$nl+
    '                chkContainerNoTitle.Enabled = enabled;'+$nl+
    '                clrbtnContainerBackground.Enabled = enabled;')

$loadAnchor='                chkContainerBorder.Checked = MeterManager.ContainerHasBorder(cci.ID);'+$nl+
            '                clrbtnContainerBackground.Color = MeterManager.GetContainerBackgroundColour(cci.ID);'
if(!$cfg.Contains($loadAnchor)){throw 'P31 config container-load anchor missing'}
$cfg=$cfg.Replace($loadAnchor,
    '                chkContainerBorder.Checked = MeterManager.ContainerHasBorder(cci.ID);'+$nl+
    '                chkLockContainer.Checked = MeterManager.ContainerLocked(cci.ID);'+$nl+
    '                chkContainerNoTitle.Checked = MeterManager.ContainerNoTitleBar(cci.ID);'+$nl+
    '                clrbtnContainerBackground.Color = MeterManager.GetContainerBackgroundColour(cci.ID);')

# Guard mutations exactly as newer Thetis Lock does.
foreach($methodName in @(
    'btnContainerDelete_Click',
    'btnAddMeterItem_Click',
    'btnRemoveMeterItem_Click',
    'btnMeterUp_Click',
    'btnMeterDown_Click'
))
{
    $sig='        private void '+$methodName+'(object sender, EventArgs e)'+$nl+'        {'
    if(!$cfg.Contains($sig)){throw "P31 config mutation anchor missing: $methodName"}
    $cfg=$cfg.Replace($sig,$sig+$nl+'            if (chkLockContainer.Checked) return;')
}

$availOld='            btnAddMeterItem.Enabled = lstMetersAvailable.SelectedIndex >= 0;'
if(!$cfg.Contains($availOld)){throw 'P31 available-list lock anchor missing'}
$cfg=$cfg.Replace($availOld,'            btnAddMeterItem.Enabled = !chkLockContainer.Checked && lstMetersAvailable.SelectedIndex >= 0;')

$stateOld='            btnRemoveMeterItem.Enabled = enabled;'
if(!$cfg.Contains($stateOld)){throw 'P31 remove state anchor missing'}
$cfg=$cfg.Replace($stateOld,'            btnRemoveMeterItem.Enabled = !chkLockContainer.Checked && enabled;')
$stateOld='            btnMeterUp.Enabled = enabled;'
if(!$cfg.Contains($stateOld)){throw 'P31 up state anchor missing'}
$cfg=$cfg.Replace($stateOld,'            btnMeterUp.Enabled = !chkLockContainer.Checked && enabled;')
$stateOld='            btnMeterDown.Enabled = enabled;'
if(!$cfg.Contains($stateOld)){throw 'P31 down state anchor missing'}
$cfg=$cfg.Replace($stateOld,'            btnMeterDown.Enabled = !chkLockContainer.Checked && enabled;')

$handlerAnchor='        private void clrbtnContainerBackground_Changed(object sender, EventArgs e)'
if(!$cfg.Contains($handlerAnchor)){throw 'P31 config handler anchor missing'}
$handlers=@'
        private void chkLockContainer_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null) MeterManager.LockContainer(cci.ID, chkLockContainer.Checked);
            ApplyContainerLockState();
        }

        private void chkContainerNoTitle_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci != null) MeterManager.ContainerNoTitleBar(cci.ID, chkContainerNoTitle.Checked);
        }

        private void ApplyContainerLockState()
        {
            bool unlocked = !chkLockContainer.Checked;
            bool hasContainer = comboContainerSelect.SelectedItem != null;
            btnContainerDelete.Enabled = hasContainer && unlocked;
            btnAddMeterItem.Enabled = hasContainer && unlocked && lstMetersAvailable.SelectedIndex >= 0;
            btnRemoveMeterItem.Enabled = hasContainer && unlocked && lstMetersInUse.SelectedIndex >= 0;
            btnMeterUp.Enabled = hasContainer && unlocked && lstMetersInUse.SelectedIndex >= 0;
            btnMeterDown.Enabled = hasContainer && unlocked && lstMetersInUse.SelectedIndex >= 0;
        }

'@
$cfg=$cfg.Replace($handlerAnchor,$handlers+$handlerAnchor)

# Apply lock state after a container selection was fully loaded.
$selectionTail='            updateMeterLists();'+$nl+'        }'+$nl+$nl+'        private void chkContainerHighlight_CheckedChanged'
if(!$cfg.Contains($selectionTail)){throw 'P31 config selection-tail anchor missing'}
$cfg=$cfg.Replace($selectionTail,
    '            updateMeterLists();'+$nl+
    '            ApplyContainerLockState();'+$nl+
    '        }'+$nl+$nl+
    '        private void chkContainerHighlight_CheckedChanged')

[IO.File]::WriteAllText($configPath,$cfg,$utf8)

# Add the modern-gadget source file to the project.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P30ThetisMetersTxBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P31 csproj P30 bridge anchor missing'}
if(!$proj.Contains('<Compile Include="P31ModernThetisGadgets.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P31ModernThetisGadgets.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Hard source gates.
$verifyMM=[IO.File]::ReadAllText($mmPath)
$verifyUC=[IO.File]::ReadAllText($ucPath)
$verifyCFG=[IO.File]::ReadAllText($configPath)
$verifyModern=[IO.File]::ReadAllText($dstModern)

foreach($token in @(
    'VFO_DISPLAY','BAND_BUTTONS','MODE_BUTTONS','TUNESTEP_BUTTONS',
    'P31AddVFODisplay','P31AddBandButtons','P31AddModeButtons','P31AddTunestepButtons',
    'P31RenderVfoDisplay','P31RenderButtonBox','P31DispatchMouseUp'
)){
    if(!$verifyMM.Contains($token) -and !$verifyModern.Contains($token)){throw "P31 modern gadget gate missing: $token"}
}
foreach($token in @('public bool Locked','public bool NoControls','Keys.Shift','tmp.Length >= 13 && tmp.Length <= 15'))
{
    if(!$verifyUC.Contains($token)){throw "P31 container gate missing: $token"}
}
foreach($token in @('chkLockContainer','No Title Bar','ApplyContainerLockState','MeterManager.ContainerNoTitleBar'))
{
    if(!$verifyCFG.Contains($token)){throw "P31 config gate missing: $token"}
}

Write-Host 'P31_BASE=P30_UNCHANGED_RX_TX_SKINS_PERSISTENCE'
Write-Host 'P31_GADGET_SOURCE_SHA=3dbd787eaef30eca66d089ccfb4c4ccb8c0cfb7e'
Write-Host 'P31_CONTAINER_LOCK_SOURCE_SHA=a53b19274e715182d8f386bfabbb2c4164287a0e'
Write-Host 'P31_GADGETS=VFO_DISPLAY,BAND_BUTTONS,MODE_BUTTONS,TUNESTEP_BUTTONS'
Write-Host 'P31_CONTAINER_CONTROLS=LOCK,NO_TITLE_BAR_SHIFT_ACCESS'
Write-Host 'P31_TARGET_ADAPTER=POWERSDR_RX1_VFOA_BAND_MODE_TUNESTEP'
Write-Host 'P31_RX2=NOT_EXPOSED'
