param(
    [Parameter(Mandatory=$true)][string]$SourceRoot,
    [Parameter(Mandatory=$true)][string]$FlexMetersDll
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$SourceRoot=(Resolve-Path $SourceRoot).Path
$FlexMetersDll=(Resolve-Path $FlexMetersDll).Path
$consoleDir=Join-Path $SourceRoot 'Console'
$csproj=Join-Path $consoleDir 'PowerSDR.csproj'
$consoleCs=Join-Path $consoleDir 'console.cs'
$setupCs=Join-Path $consoleDir 'setup.cs'
$adapterSource=Join-Path $PSScriptRoot 'meters-native\PowerSDR\Console.FlexMetersAdapter.cs'
$adapterTarget=Join-Path $consoleDir 'Console.FlexMetersAdapter.cs'
$setupEntrySource=Join-Path $PSScriptRoot 'meters-native\PowerSDR\Setup.FlexMetersEntry.cs'
$setupEntryTarget=Join-Path $consoleDir 'Setup.FlexMetersEntry.cs'
$outDir=Join-Path $SourceRoot 'bin\Release'
$dllTarget=Join-Path $outDir 'FlexMeters.dll'

foreach($required in @($csproj,$consoleCs,$setupCs,$adapterSource,$setupEntrySource,$FlexMetersDll)){
    if(!(Test-Path $required)){throw "Required FlexMeters RX1 input missing: $required"}
}
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
Copy-Item $adapterSource $adapterTarget -Force
Copy-Item $setupEntrySource $setupEntryTarget -Force
Copy-Item $FlexMetersDll $dllTarget -Force

$cs=[IO.File]::ReadAllText($csproj)
$crlf=([string][char]13)+([string][char]10)
$lf=[string][char]10
$console=[IO.File]::ReadAllText($consoleCs).Replace($crlf,$lf)
$setup=[IO.File]::ReadAllText($setupCs).Replace($crlf,$lf)

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace($crlf,$lf)
    $New=$New.Replace($crlf,$lf)
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){throw "FlexMeters lifecycle anchor missing: $Label"}
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){throw "FlexMeters lifecycle anchor not unique: $Label"}
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

if($cs.Contains('<Reference Include="FlexMeters">') -or
   $cs.Contains('<Compile Include="Console.FlexMetersAdapter.cs"') -or
   $cs.Contains('<Compile Include="Setup.FlexMetersEntry.cs"')){
    throw 'FlexMeters RX1 project wiring already exists; refusing duplicate injection.'
}

$marker='  <Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" />'
if(([regex]::Matches($cs,[regex]::Escape($marker))).Count -ne 1){
    throw 'Unexpected PowerSDR C# targets import layout.'
}

$items=@"
  <ItemGroup>
    <Reference Include="FlexMeters">
      <HintPath>..\bin\Release\FlexMeters.dll</HintPath>
      <Private>True</Private>
    </Reference>
    <Compile Include="Console.FlexMetersAdapter.cs" />
    <Compile Include="Setup.FlexMetersEntry.cs" />
  </ItemGroup>
"@

$cs=$cs.Replace($marker,$items+[Environment]::NewLine+$marker)

$setup=Replace-ExactOnce $setup @'
            console = c;
'@ @'
            console = c;
            InitializeFlexMetersAppearanceEntry();
'@ 'Setup Appearance Meters/Gadgets entry'

$console=Replace-ExactOnce $console @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouDbInitStart, "STARTUP", "DB_INIT", "exists=" + DB_Exists);
'@ @'
            SQ4KOUUIDiagnostics.OperationEnd(sq4kouDbInitStart, "STARTUP", "DB_INIT", "exists=" + DB_Exists);
            InitializeFlexMetersWorkspaceRuntime();
'@ 'start after DB.Init'

$console=Replace-ExactOnce $console @'
            Stopwatch sq4kouShutdown1Timer = Stopwatch.StartNew();
'@ @'
            Stopwatch sq4kouShutdown1Timer = Stopwatch.StartNew();
            ShutdownFlexMetersWorkspaceRuntime();
'@ 'stop on Console_Closing'

$console=Replace-ExactOnce $console @'
            Stopwatch sq4kouShutdown2Timer = Stopwatch.StartNew();
'@ @'
            Stopwatch sq4kouShutdown2Timer = Stopwatch.StartNew();
            ShutdownFlexMetersWorkspaceRuntime();
'@ 'stop on Dispose'

[IO.File]::WriteAllText($csproj,$cs,[Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllText($consoleCs,$console.Replace($lf,$crlf),[Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllText($setupCs,$setup.Replace($lf,$crlf),[Text.UTF8Encoding]::new($false))

Write-Host "FLEXMETERS_RX1_ADAPTER=$adapterTarget"
Write-Host "FLEXMETERS_SETUP_ENTRY=$setupEntryTarget"
Write-Host "FLEXMETERS_DLL=$dllTarget"
