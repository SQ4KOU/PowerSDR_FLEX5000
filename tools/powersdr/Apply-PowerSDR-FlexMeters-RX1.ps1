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
$adapterSource=Join-Path $PSScriptRoot 'meters-native\PowerSDR\Console.FlexMetersAdapter.cs'
$adapterTarget=Join-Path $consoleDir 'Console.FlexMetersAdapter.cs'
$outDir=Join-Path $SourceRoot 'bin\Release'
$dllTarget=Join-Path $outDir 'FlexMeters.dll'

foreach($required in @($csproj,$adapterSource,$FlexMetersDll)){
    if(!(Test-Path $required)){throw "Required FlexMeters RX1 input missing: $required"}
}
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
Copy-Item $adapterSource $adapterTarget -Force
Copy-Item $FlexMetersDll $dllTarget -Force

$cs=[IO.File]::ReadAllText($csproj)
if($cs.Contains('<Reference Include="FlexMeters">') -or $cs.Contains('<Compile Include="Console.FlexMetersAdapter.cs"')){
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
  </ItemGroup>
"@

$cs=$cs.Replace($marker,$items+[Environment]::NewLine+$marker)
[IO.File]::WriteAllText($csproj,$cs,[Text.UTF8Encoding]::new($false))

Write-Host "FLEXMETERS_RX1_ADAPTER=$adapterTarget"
Write-Host "FLEXMETERS_DLL=$dllTarget"
