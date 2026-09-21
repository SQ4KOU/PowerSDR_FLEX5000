param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$SourceRoot=(Resolve-Path $SourceRoot).Path
$adapter=Join-Path $SourceRoot 'Console\Console.FlexMetersAdapter.cs'
$csproj=Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$cat=Join-Path $SourceRoot 'Console\CAT\CATCommands.cs'
$consoleCs=Join-Path $SourceRoot 'Console\console.cs'
$setupCs=Join-Path $SourceRoot 'Console\setup.cs'
$setupDesigner=Join-Path $SourceRoot 'Console\setup.Designer.cs'
$setupEntry=Join-Path $SourceRoot 'Console\Setup.FlexMetersEntry.cs'
$dll=Join-Path $SourceRoot 'bin\Release\FlexMeters.dll'

foreach($required in @($adapter,$csproj,$cat,$consoleCs,$setupCs,$setupDesigner,$setupEntry,$dll)){
    if(!(Test-Path $required)){throw "FlexMeters RX1 verification input missing: $required"}
}

$a=[IO.File]::ReadAllText($adapter)
$p=[IO.File]::ReadAllText($csproj)
$c=[IO.File]::ReadAllText($cat)
$console=[IO.File]::ReadAllText($consoleCs)
$setup=[IO.File]::ReadAllText($setupCs)
$setupDesignerText=[IO.File]::ReadAllText($setupDesigner)
$entry=[IO.File]::ReadAllText($setupEntry)

if($a -match '\bdynamic\b'){throw 'dynamic is forbidden in Console.FlexMetersAdapter.cs'}
if(([regex]::Matches($p,'<Reference Include="FlexMeters">')).Count -ne 1){throw 'FlexMeters reference count is not exactly one'}
if(([regex]::Matches($p,'<Compile Include="Console\.FlexMetersAdapter\.cs"')).Count -ne 1){throw 'FlexMeters adapter compile item count is not exactly one'}
if(([regex]::Matches($p,'<Compile Include="Setup\.FlexMetersEntry\.cs"')).Count -ne 1){throw 'FlexMeters Setup entry compile item count is not exactly one'}

$nativeTokens=@(
    'DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.SIGNAL_STRENGTH)',
    'console.MultiMeterCalOffset',
    'Display.RX1PreampOffset',
    'console.RX1FilterSizeCalOffset',
    'console.RX1PathOffset',
    'console.RX1XVTRGainOffset',
    'console.RX1Loop',
    'console.LoopGain'
)
foreach($token in $nativeTokens){
    if(!$c.Contains($token)){throw "Pinned KE9NS native RX1 evidence missing: $token"}
}

$adapterTokens=@(
    'DttSP.CalculateRXMeter(',
    'DttSP.MeterType.SIGNAL_STRENGTH',
    '_console.MultiMeterCalOffset',
    'Display.RX1PreampOffset',
    '_console.RX1FilterSizeCalOffset',
    '_console.RX1PathOffset',
    '_console.RX1XVTRGainOffset',
    '_console.RX1Loop',
    '_console.LoopGain',
    'Flex5000Rx1SignalCalibration.Apply',
    'MeterReadingResult.Unsupported',
    'CreateFlexMetersLiveRuntime',
    'new FlexMeters.MeterLiveRuntime',
    'MeterWorkspaceRuntimeHost',
    'DataTableMeterStore',
    'DB.ds.Tables.Contains(FlexMetersDatabaseTableName)',
    'ReplaceFlexMetersWorkspace',
    'ReloadFlexMetersWorkspaceRuntime',
    'DB.Update()',
    'ShutdownFlexMetersWorkspaceRuntime',
    'MeterWorkspaceManager',
    'flexMetersContainerManager.AddContainer',
    'flexMetersContainerManager.RemoveContainer',
    'flexMetersContainerManager.ReplaceContainer',
    'delegate { DB.Update(); }',
    'WinFormsMeterWindowHost',
    'this.Shown += FlexMetersConsoleShown',
    'RestoreWindows(',
    'windowHost.Dispose()',
    'CreateFlexMetersRadioState()',
    'FlexMetersRadioStateAdapter',
    '_console.VFOAFreq * 1000000.0',
    '_console.VFOBFreq * 1000000.0',
    'CreateFlexMetersRadioState(),',
    'Mox = _console.MOX',
    'Tune = _console.TUN',
    'EnsureFlexMetersWorkspaceManager'
)
foreach($token in $adapterTokens){
    if(!$a.Contains($token)){throw "FlexMeters RX1 adapter gate missing: $token"}
}

$lifecycleTokens=@(
    'InitializeFlexMetersWorkspaceRuntime();',
    'ShutdownFlexMetersWorkspaceRuntime();'
)
foreach($token in $lifecycleTokens){
    if(!$console.Contains($token)){throw "FlexMeters lifecycle hook missing from console.cs: $token"}
}
if(([regex]::Matches($console,'InitializeFlexMetersWorkspaceRuntime\(\);')).Count -ne 1){
    throw 'FlexMeters startup hook count is not exactly one'
}
if(([regex]::Matches($console,'ShutdownFlexMetersWorkspaceRuntime\(\);')).Count -ne 2){
    throw 'FlexMeters shutdown hook count is not exactly two'
}
if($console.IndexOf('InitializeFlexMetersWorkspaceRuntime();',[StringComparison]::Ordinal) -lt
   $console.IndexOf('DB_Exists = DB.Init(',[StringComparison]::Ordinal)){
    throw 'FlexMeters workspace starts before DB.Init'
}

if(([regex]::Matches($setup,'InitializeFlexMetersAppearanceEntry\(\);')).Count -ne 1){
    throw 'FlexMeters Setup Appearance hook count is not exactly one'
}
if($setup.IndexOf('InitializeFlexMetersAppearanceEntry();',[StringComparison]::Ordinal) -lt
   $setup.IndexOf('console = c;',[StringComparison]::Ordinal)){
    throw 'FlexMeters Setup Appearance entry runs before console assignment'
}
if($setupDesignerText.Contains('FlexMeters')){
    throw 'FlexMeters must not modify legacy setup.Designer.cs'
}
if($entry -match '\bdynamic\b'){
    throw 'dynamic is forbidden in Setup.FlexMetersEntry.cs'
}
$entryTokens=@(
    'tcAppearance.TabPages.Add(flexMetersAppearancePage)',
    'flexMetersAppearancePage.Text = "Meters/Gadgets"',
    'new FlexMeters.FlexMetersEditorForm(manager)',
    'console.EnsureFlexMetersWorkspaceManager()',
    'flexMetersEditorForm.Show(console)'
)
foreach($token in $entryTokens){
    if(!$entry.Contains($token)){throw "FlexMeters Setup entry gate missing: $token"}
}

Write-Host 'FLEXMETERS_SETUP_ENTRY=PASS'
Write-Host 'FLEXMETERS_RX1_SOURCE_GATE=PASS'
