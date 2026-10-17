[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Append P51 to the existing P50 chain; no changes to the P49 / P50 patches.
$base = Join-Path $PSScriptRoot 'Build-PowerSDR-MSI-P50-WINDOW-POSITIONS.ps1'
if(!(Test-Path $base)) {throw 'P50 build script missing'}
$c = [IO.File]::ReadAllText($base)

$old = @'
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-WindowPositions-P50.ps1') -SourceRoot $WorkRoot

    nuget restore
'@
$new = @'
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-WindowPositions-P50.ps1') -SourceRoot $WorkRoot

    # P51: extend P50 native panels, enable native picture resize.
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-WindowResize-P51.ps1') -SourceRoot $WorkRoot

    nuget restore
'@
if(!$c.Contains($old)){throw 'P51 insertion anchor missing'}
$c = $c.Replace($old,$new)

$oldName = 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P50-WINDOW-POSITIONS.x86.msi'
$newName = 'PowerSDR-SQ4KOU-FLEX5000-KE9NS-v2.8.0.336-P51-WINDOW-RESIZE.x86.msi'
if(!$c.Contains($oldName)){throw 'P51 MSI name anchor missing'}
$c = $c.Replace($oldName,$newName)

$oldManifest = "'P50_FLEX5000_BACKEND=UNCHANGED'"
$newManifest = "'P50_FLEX5000_BACKEND=UNCHANGED','P51_MAIN_UI=EXTENDED_P50_SHIFT_LMB','P51_BANDSCOPE_MOVE=P50_SHIFT_LMB','P51_BANDSCOPE_RESIZE=SHIFT_LMB_ALL_EDGES','P51_RENDERER=NATIVE_PICDISPLAY_RESIZE','P51_GEOMETRY=P50_POSITION_PLUS_P51_SIZE_DURABLE','P51_FLEX5000_DSP=UNCHANGED'"
if(!$c.Contains($oldManifest)){throw 'P51 manifest anchor missing'}
$c = $c.Replace($oldManifest,$newManifest)

$generated = Join-Path $PSScriptRoot '_Build-P51.generated.ps1'
[IO.File]::WriteAllText($generated,$c,(New-Object Text.UTF8Encoding($false)))
try {
    & $generated -WixBin $WixBin
    if($LASTEXITCODE -ne 0) {throw "P51 builder exited with rc=$LASTEXITCODE"}
}
finally {
    Remove-Item $generated -Force -ErrorAction SilentlyContinue
}
