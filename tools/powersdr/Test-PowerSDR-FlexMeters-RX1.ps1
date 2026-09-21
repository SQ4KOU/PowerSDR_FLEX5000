param()

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$HarnessRoot=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$WorkRoot=Join-Path $HarnessRoot '.work\flexmeters-rx1-ci'
$SourceSha='d558979570c4c2e4572b63ac218d3d4477926cb8'

if(Test-Path $WorkRoot){Remove-Item $WorkRoot -Recurse -Force}
New-Item -ItemType Directory -Force -Path $WorkRoot | Out-Null

if (!(Get-Command innoextract.exe -ErrorAction SilentlyContinue) -and !(Get-Command innoextract -ErrorAction SilentlyContinue)) {
    throw 'innoextract is required'
}
if (!(Get-Command nuget.exe -ErrorAction SilentlyContinue) -and !(Get-Command nuget -ErrorAction SilentlyContinue)) {
    throw 'nuget is required'
}

$SourceRoot=Join-Path $WorkRoot 'ke9ns'
& git clone --no-tags https://github.com/ke9ns/PowerSDR-KE9NS-v2.8.0.git $SourceRoot
if($LASTEXITCODE -ne 0){throw "KE9NS clone failed rc=$LASTEXITCODE"}

Push-Location $SourceRoot
try{
    & git checkout --detach $SourceSha
    if($LASTEXITCODE -ne 0){throw "KE9NS checkout failed rc=$LASTEXITCODE"}
    if((git rev-parse HEAD).Trim() -ne $SourceSha){throw 'Pinned KE9NS source SHA mismatch'}
}finally{
    Pop-Location
}

$outDir=Join-Path $SourceRoot 'bin\Release'
$logs=Join-Path $WorkRoot 'logs'
New-Item -ItemType Directory -Force -Path $outDir,$logs | Out-Null
& (Join-Path $PSScriptRoot 'Prepare-PowerSDR-Runtime.ps1') -WorkRoot $SourceRoot -OutDir $outDir -LogRoot $logs

& nuget restore (Join-Path $SourceRoot 'PowerSDR.sln') -NonInteractive
if($LASTEXITCODE -ne 0){throw "NuGet restore failed rc=$LASTEXITCODE"}

$pf86=[Environment]::GetFolderPath('ProgramFilesX86')
$vswhere=Join-Path $pf86 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild=& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if(!$msbuild){throw 'MSBuild not found'}

$testProject=Join-Path $PSScriptRoot 'meters-native\FlexMeters.Tests\FlexMeters.Tests.csproj'
& $msbuild $testProject '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=x86' '/v:minimal'
if($LASTEXITCODE -ne 0){throw "FlexMeters build failed rc=$LASTEXITCODE"}

$testExe=Join-Path $PSScriptRoot 'meters-native\FlexMeters.Tests\bin\Release\FlexMeters.Tests.exe'
& $testExe
if($LASTEXITCODE -ne 0){throw "FlexMeters tests failed rc=$LASTEXITCODE"}

$flexDll=Join-Path $PSScriptRoot 'meters-native\FlexMeters\bin\Release\FlexMeters.dll'
if(!(Test-Path $flexDll)){throw "FlexMeters.dll missing: $flexDll"}

$powerMateDll=Join-Path $outDir 'PowerMate.dll'
if(!(Test-Path $powerMateDll)){
    & $msbuild (Join-Path $SourceRoot 'PowerMate\PowerMate.vcxproj') '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=Win32' '/v:minimal'
    if($LASTEXITCODE -ne 0){throw "PowerMate build failed rc=$LASTEXITCODE"}
    Copy-Item (Join-Path $SourceRoot 'PowerMate\bin\Release\PowerMate.dll') $powerMateDll -Force
}

$csproj=Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$cs=[IO.File]::ReadAllText($csproj)
$rx=[regex]::new('(?ms)\s*<ProjectReference Include="\.\.\\PowerMate\\PowerMate\.vcxproj">.*?</ProjectReference>')
if($rx.Matches($cs).Count -ne 1){throw 'Unexpected PowerMate ProjectReference layout'}
$pmRef=@"
    <Reference Include="PowerMate">
      <HintPath>..\bin\Release\PowerMate.dll</HintPath>
      <Private>True</Private>
    </Reference>
"@
$cs=$rx.Replace($cs,[Environment]::NewLine+$pmRef,1)
[IO.File]::WriteAllText($csproj,$cs,[Text.UTF8Encoding]::new($false))

& (Join-Path $PSScriptRoot 'Apply-PowerSDR-DatabaseReliability.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-WindowState.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-FastShutdown.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-FastAudioExit.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-AudioBeforeStandby.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ShutdownEvidence.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-FastStateSave-P07.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-DisplayPerformance-P08.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-StableDisplayPacing-P13.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Verify-PowerSDR-StabilityBaseline-P14.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-UIDiagnostics-P17.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Verify-PowerSDR-UIDiagnostics-P17.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-CleanStartupPresentation-P18.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Verify-PowerSDR-CleanStartupPresentation-P18.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ScreenBeforeAudio-P19.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Verify-PowerSDR-ScreenBeforeAudio-P19.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-OriginalFlexSplash-P20.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Verify-PowerSDR-OriginalFlexSplash-P20.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-SQ4KOUSplash-P21-FINAL2.ps1') -SourceRoot $SourceRoot
& (Join-Path $PSScriptRoot 'Verify-PowerSDR-SQ4KOUSplash-P21-FINAL2.ps1') -SourceRoot $SourceRoot

& (Join-Path $PSScriptRoot 'Apply-PowerSDR-FlexMeters-RX1.ps1') -SourceRoot $SourceRoot -FlexMetersDll $flexDll
& (Join-Path $PSScriptRoot 'Verify-PowerSDR-FlexMeters-RX1.ps1') -SourceRoot $SourceRoot

$buildLog=Join-Path $logs 'MSBUILD_POWERSDR_FLEXMETERS_RX1.log'
& $msbuild $csproj '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=x86' '/p:BuildProjectReferences=false' '/v:minimal' "/flp:logfile=$buildLog;verbosity=normal"
if($LASTEXITCODE -ne 0){throw "Integrated PowerSDR x86 build failed rc=$LASTEXITCODE"}

$exe=Join-Path $outDir 'PowerSDR.exe'
if(!(Test-Path $exe)){throw 'PowerSDR.exe missing after integrated build'}
$fv=[Diagnostics.FileVersionInfo]::GetVersionInfo($exe).FileVersion
if($fv -ne '2.8.0.336'){throw "Unexpected PowerSDR file version: $fv"}
if(!(Test-Path (Join-Path $outDir 'FlexMeters.dll'))){throw 'FlexMeters.dll missing from final PowerSDR output'}

Write-Host 'FLEXMETERS_RX1_NATIVE_ADAPTER=PASS'
Write-Host 'FLEXMETERS_RX1_SOURCE_PARITY=PASS'
Write-Host 'POWERSDR_P21_INTEGRATED_BUILD=PASS'
