[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Preserve the proven P49 build chain and append only the P50 console patch.
$base = Join-Path $PSScriptRoot 'Build-PowerSDR-MSI-P49-DATABASE-PERSISTENCE.ps1'
if(!(Test-Path $base)){throw 'P49 builder missing'}
$c = [IO.File]::ReadAllText($base)

$old = @'
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-DatabasePersistence-P49.ps1') -SourceRoot $WorkRoot

    nuget restore
'@
$new = @'
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-DatabasePersistence-P49.ps1') -SourceRoot $WorkRoot

    # P50: reposition native top-level console panels with Shift+LMB.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-WindowPositions-P50.ps1') -SourceRoot $WorkRoot

    nuget restore
'@
if(!$c.Contains($old)){throw 'P50 insertion anchor in P49 builder missing'}
$c = $c.Replace($old,$new)

$oldMsi = 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P49-DATABASE-PERSISTENCE.x86.msi'
$newMsi = 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P50-WINDOW-POSITIONS.x86.msi'
if(!$c.Contains($oldMsi)){throw 'P50 MSI name anchor missing'}
$c = $c.Replace($oldMsi,$newMsi)

$oldManifest = "'P49_METERS=P48_DURABLE_RETAINED'"
$newManifest = "'P49_METERS=P48_DURABLE_RETAINED','P50_WINDOW_POSITIONS=SHIFT_LMB','P50_WINDOW_PERSISTENCE=DB_REPLACEVARS_IMMEDIATE','P50_DISPLAY=UNCHANGED','P50_FLEX5000_BACKEND=UNCHANGED'"
if(!$c.Contains($oldManifest)){throw 'P50 manifest anchor missing'}
$c = $c.Replace($oldManifest,$newManifest)

$generated = Join-Path $PSScriptRoot '_Build-P50.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try
{
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0){throw "P50 builder exited with rc=$LASTEXITCODE"}
}
finally
{
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
