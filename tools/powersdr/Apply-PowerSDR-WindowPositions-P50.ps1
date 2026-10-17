[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$consoleDir = Join-Path $SourceRoot 'Console'
$projPath = Join-Path $consoleDir 'PowerSDR.csproj'
$designerPath = Join-Path $consoleDir 'console.Designer.cs'
$bridgePath = Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$srcPath = Join-Path $PSScriptRoot 'P50WindowPositions.cs'
$dstPath = Join-Path $consoleDir 'P50WindowPositions.cs'
$utf8 = New-Object Text.UTF8Encoding($true)
$utf8NoBom = New-Object Text.UTF8Encoding($false)
$newline = [Environment]::NewLine

foreach($path in @($projPath,$designerPath,$bridgePath,$srcPath)){
    if(!(Test-Path $path)){throw "P50 required file missing: $path"}
}

# Source gates: these are original KE9NS console controls, not Thetis meters.
$designer = [IO.File]::ReadAllText($designerPath)
foreach($control in @(
    'grpMultimeter','pwrMstWatts','pwrMstSWR','grpRX2Meter',
    'panelBandHF','panelBandGN','panelBandVHF',
    'panelBandHFRX2','panelBandGNRX2','panelBandVHFRX2',
    'panelMode','panelRX2Mode','panelFilter','panelRX2Filter',
    'grpVFOA','grpVFOB','grpVFOBetween',
    'VFODialA','VFODialAA','VFODialB','VFODialBB',
    'panelDSP','panelRX2DSP','panelOptions','panelAntenna',
    'panelSoundControls','panelModeSpecificPhone',
    'panelModeSpecificCW','panelModeSpecificFM',
    'panelModeSpecificDigital','panelDateTime'
)){
    if(!$designer.Contains('this.'+$control+'.Name = "'+$control+'"')){
        throw "P50 native control gate missing: $control"
    }
}

Copy-Item $srcPath $dstPath -Force
$proj = [IO.File]::ReadAllText($projPath)
$anchor = '<Compile Include="P39LegacyItems.cs" />'
$addition = '<Compile Include="P50WindowPositions.cs" />'
if(!$proj.Contains($anchor)){throw 'P50 project compile anchor missing'}
if($proj.Contains($addition)){throw 'P50 file already compiled; unexpected source state'}
$proj = $proj.Replace($anchor,$anchor+$newline+'    '+$addition)
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# P39 already hooks the main form's Shown event. Attach P50 after P39,
# without modifying or replacing the original event handler.
$bridge = [IO.File]::ReadAllText($bridgePath)
$shown = '            try { P39InitLegacyItemsController(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P39 legacy init: " + ex); }'
if(!$bridge.Contains($shown)){throw 'P50 P39 shown-handler anchor missing'}
$init = '            try { P50InitWindowPositions(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P50 move init: " + ex); }'
if($bridge.Contains($init)){throw 'P50 already hooked; unexpected source state'}
$bridge = $bridge.Replace($shown,$shown+$newline+$init)
[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

$verify = [IO.File]::ReadAllText($dstPath)
foreach($token in @(
    'Shift+left-drag','Application.AddMessageFilter(p50Filter)',
    'DB.ReplaceVars(P50DatabaseTable, ref rows)','DB.Update();',
    'P50FinishDrag()','p50ActiveGroup','panelModeSpecificDigital'
)){
    if(!$verify.Contains($token)){throw "P50 C# source gate missing: $token"}
}
$afterBridge = [IO.File]::ReadAllText($bridgePath)
if(!$afterBridge.Contains($init)){throw 'P50 startup verification failed'}

Write-Host 'P50_WINDOW_POSITIONS=SHIFT_LMB'
Write-Host 'P50_WINDOW_PERSISTENCE=DB_REPLACEVARS_IMMEDIATE'
Write-Host 'P50_DISPLAY=UNCHANGED'
Write-Host 'P50_FLEX5000_BACKEND=UNCHANGED'
