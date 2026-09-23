[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# Build on top of the P47 audited persistence path.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-MetersPersistence-P47.ps1') -SourceRoot $SourceRoot

$consoleDir=Join-Path $SourceRoot 'Console'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$formPath=Join-Path $consoleDir 'P25_frmMeterDisplay.cs'
$utf8=New-Object Text.UTF8Encoding($true)

foreach($p in @($bridgePath,$formPath)){
    if(!(Test-Path $p)){throw "P48 required file missing: $p"}
}

$bridge=[IO.File]::ReadAllText($bridgePath).Replace("`r`n","`n")

# P47's audit was intentionally strict, but a transient form/container count
# mismatch can happen while a user closes/hides a floating form. That must not
# suppress the whole persistence write. P48 treats audit as diagnostic only.
$oldAudit=@'
            string audit;
            if (!MeterManager.P47AuditPersistenceSnapshot(settings, out audit))
                throw new InvalidOperationException("Meters/Gadgets persistence snapshot incomplete: " + audit);
'@.Replace("`r`n","`n")
$newAudit=@'
            string audit;
            bool auditOk = MeterManager.P47AuditPersistenceSnapshot(settings, out audit);
            if (!auditOk)
                System.Diagnostics.Debug.WriteLine("P48 Meters/Gadgets persistence audit warning: " + audit);
'@.Replace("`r`n","`n")
if(!$bridge.Contains($oldAudit)){throw 'P48 audit anchor missing'}
$bridge=$bridge.Replace($oldAudit,$newAudit)

# ReplaceVars and Common.SaveForm modify the in-memory DataSet. Make the meter
# transaction durable immediately instead of waiting for the much later DB.Exit().
$oldTail=@'
            MeterManager.P47SaveLegacyWindowTables();

            System.Diagnostics.Debug.WriteLine("P47 Meters/Gadgets persistence saved: " + audit);
'@.Replace("`r`n","`n")
$newTail=@'
            MeterManager.P47SaveLegacyWindowTables();

            // P48: persist the meter transaction to the actual XML database NOW.
            // This removes shutdown-order dependency and makes closing the
            // Meters/Gadgets configuration window an actual save point.
            DB.Update();

            System.Diagnostics.Debug.WriteLine("P48 Meters/Gadgets persistence saved and flushed: " + audit);
'@.Replace("`r`n","`n")
if(!$bridge.Contains($oldTail)){throw 'P48 durable flush anchor missing'}
$bridge=$bridge.Replace($oldTail,$newTail)

[IO.File]::WriteAllText($bridgePath,$bridge.Replace("`n","`r`n"),$utf8)

# Native Thetis frmMeterDisplay hides/cancels UserClosing and only calls Common.SaveForm.
# In PowerSDR that keeps the geometry table updated in memory, but it does not save
# the complete MeterManager model at that exact user action. Promote UserClosing to
# a complete meter transaction by calling the existing console save path.
$form=[IO.File]::ReadAllText($formPath).Replace("`r`n","`n")
$oldFormClose=@'
        private void frmMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
            Common.SaveForm(this, "MeterDisplay_" + _id);
        }
'@.Replace("`r`n","`n")
$newFormClose=@'
        private void frmMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool userClosing = e.CloseReason == CloseReason.UserClosing;
            if (userClosing)
            {
                this.Hide();
                e.Cancel = true;
            }

            Common.SaveForm(this, "MeterDisplay_" + _id);

            // P48: closing a floating meter window is a user-visible persistence
            // boundary. Save the complete container/item model and flush DB now.
            if (userClosing && _console != null)
            {
                try { _console.P27SaveMetersConfiguration(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("P48 floating meter save failed: " + ex);
                }
            }
        }
'@.Replace("`r`n","`n")
if(!$form.Contains($oldFormClose)){throw 'P48 frmMeterDisplay close anchor missing'}
$form=$form.Replace($oldFormClose,$newFormClose)
[IO.File]::WriteAllText($formPath,$form.Replace("`n","`r`n"),$utf8)

$verifyBridge=[IO.File]::ReadAllText($bridgePath)
$verifyForm=[IO.File]::ReadAllText($formPath)

foreach($token in @(
    'bool auditOk = MeterManager.P47AuditPersistenceSnapshot',
    'P48 Meters/Gadgets persistence audit warning',
    'DB.ReplaceVars("SQ4KOU_ThetisMeters", ref a);',
    'MeterManager.P47SaveLegacyWindowTables();',
    'DB.Update();',
    'P48 Meters/Gadgets persistence saved and flushed'
)){
    if(!$verifyBridge.Contains($token)){throw "P48 bridge gate missing: $token"}
}
foreach($token in @(
    'bool userClosing = e.CloseReason == CloseReason.UserClosing;',
    '_console.P27SaveMetersConfiguration();',
    'P48 floating meter save failed'
)){
    if(!$verifyForm.Contains($token)){throw "P48 form gate missing: $token"}
}

Write-Host 'P48_METER_PERSISTENCE=DURABLE_IMMEDIATE_FLUSH'
Write-Host 'P48_METER_USER_CLOSE=FULL_MODEL_SAVE_PLUS_DB_UPDATE'
Write-Host 'P48_METER_CONFIG_HIDE=FULL_MODEL_SAVE_PLUS_DB_UPDATE'
Write-Host 'P48_METER_APP_CLOSE=FULL_MODEL_SAVE_PLUS_DB_UPDATE_BEFORE_SHUTDOWN'
Write-Host 'P48_METER_AUDIT=DIAGNOSTIC_NOT_SAVE_BLOCKER'
Write-Host 'P48_METER_GEOMETRY=P47_AUTHORITATIVE_PLUS_NATIVE_FALLBACK'
Write-Host 'P48_RX2=NOT_EXPOSED'
