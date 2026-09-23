[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$setupPath=Join-Path $consoleDir 'setup.cs'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$designerPath=Join-Path $consoleDir 'console.Designer.cs'
$src=Join-Path $PSScriptRoot 'P39LegacyItems.cs'
$dst=Join-Path $consoleDir 'P39LegacyItems.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

if(!(Test-Path $src)){throw 'P39 source missing'}
foreach($p in @($projPath,$setupPath,$bridgePath,$designerPath)){
    if(!(Test-Path $p)){throw "P39 required PowerSDR file missing: $p"}
}

Copy-Item $src $dst -Force

# Gate the exact native controls used by the first Legacy Items stage.
$designer=[IO.File]::ReadAllText($designerPath)
foreach($token in @(
    'grpMultimeter','grpRX2Meter','pwrMstWatts','pwrMstSWR',
    'panelBandHF','panelBandGN','panelBandVHF',
    'panelMode','panelFilter',
    'grpVFOA','grpVFOB','VFODialA','VFODialAA','VFODialB','VFODialBB',
    'grpVFOBetween','chkVFOSync'
)){
    if(!$designer.Contains($token)){throw "P39 native control gate missing: $token"}
}

# Initialise the Thetis-style Legacy Items page after Setup has its Console reference.
$setup=[IO.File]::ReadAllText($setupPath)
$setupAnchor='            console = c;   // ke9ns mod  to allow console to pass back values to setup screen'
if(!$setup.Contains($setupAnchor)){
    $setupAnchor='            console = c;'
}
if(!$setup.Contains($setupAnchor)){throw 'P39 Setup console assignment anchor missing'}
if(!$setup.Contains('P39InitLegacyItemsUI();')){
    $setup=$setup.Replace($setupAnchor,$setupAnchor+$nl+'            P39InitLegacyItemsUI();')
}
[IO.File]::WriteAllText($setupPath,$setup,$utf8)

# Apply persisted visibility once the main console is shown, after native layout init.
$bridge=[IO.File]::ReadAllText($bridgePath)
$shownAnchor='        private void P25ThetisMetersShown(object sender, EventArgs e)'+$nl+
             '        {'+$nl+
             '            try { P25InitThetisMeters(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P25 meter init: " + ex); }'
if(!$bridge.Contains($shownAnchor)){throw 'P39 P25 shown-handler anchor missing'}
$shownReplacement='        private void P25ThetisMetersShown(object sender, EventArgs e)'+$nl+
                  '        {'+$nl+
                  '            try { P39InitLegacyItemsController(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P39 legacy init: " + ex); }'+$nl+
                  '            try { P25InitThetisMeters(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P25 meter init: " + ex); }'
$bridge=$bridge.Replace($shownAnchor,$shownReplacement)
[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

# Compile the new controller into PowerSDR.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P32ThetisCompatibility.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P39 compile anchor missing'}
if(!$proj.Contains('<Compile Include="P39LegacyItems.cs" />')){
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P39LegacyItems.cs" />')
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

# Source gates: first stage only, no spectrum-area geometry changes yet.
$verify=[IO.File]::ReadAllText($dst)
foreach($token in @(
    'Hide legacy meters','Hide band button grid','Hide mode button grid','Hide filter button grid',
    'Hide VFO A','Hide VFO B','Hide VFOSync box',
    'SQ4KOU_LegacyItems','P39LegacyControlVisibleChanged','P39ApplyLegacyItems'
)){
    if(!$verify.Contains($token)){throw "P39 source gate missing: $token"}
}
if($verify.Contains('panelDisplay.Size') -or $verify.Contains('panelDisplay.Bounds') -or
   $verify.Contains('panelDisplay2.Size') -or $verify.Contains('panelDisplay2.Bounds')){
    throw 'P39 stage-1 must not modify spectrum display geometry'
}

Write-Host 'P39_LEGACY_ITEMS=STAGE1_VISIBILITY_ONLY'
Write-Host 'P39_CONTROLS=METERS_BAND_MODE_FILTER_VFOA_VFOB_VFOSYNC'
Write-Host 'P39_PERSISTENCE=POWERSDR_DB_SQ4KOU_LEGACYITEMS'
Write-Host 'P39_SPECTRUM_GEOMETRY=UNCHANGED'
