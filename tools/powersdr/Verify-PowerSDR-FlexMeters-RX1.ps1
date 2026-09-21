param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$SourceRoot=(Resolve-Path $SourceRoot).Path
$adapter=Join-Path $SourceRoot 'Console\Console.FlexMetersAdapter.cs'
$csproj=Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$cat=Join-Path $SourceRoot 'Console\CAT\CATCommands.cs'
$consoleCs=Join-Path $SourceRoot 'Console\console.cs'
$dll=Join-Path $SourceRoot 'bin\Release\FlexMeters.dll'

foreach($required in @($adapter,$csproj,$cat,$consoleCs,$dll)){
    if(!(Test-Path $required)){throw "FlexMeters RX1 verification input missing: $required"}
}

$a=[IO.File]::ReadAllText($adapter)
$p=[IO.File]::ReadAllText($csproj)
$c=[IO.File]::ReadAllText($cat)
$console=[IO.File]::ReadAllText($consoleCs)

if($a -match '\bdynamic\b'){throw 'dynamic is forbidden in Console.FlexMetersAdapter.cs'}
if(([regex]::Matches($p,'<Reference Include="FlexMeters">')).Count -ne 1){throw 'FlexMeters reference count is not exactly one'}
if(([regex]::Matches($p,'<Compile Include="Console\.FlexMetersAdapter\.cs"')).Count -ne 1){throw 'FlexMeters adapter compile item count is not exactly one'}

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
    'flexMetersWindowHost.Dispose()'
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

foreach($setupName in @('setup.cs','setup.Designer.cs')){
    $setup=Join-Path $SourceRoot ('Console\'+$setupName)
    if(Test-Path $setup){
        $s=[IO.File]::ReadAllText($setup)
        if($s.Contains('FlexMeters')){throw "FlexMeters RX1 stage leaked into $setupName"}
    }
}

Write-Host 'FLEXMETERS_RX1_SOURCE_GATE=PASS'
