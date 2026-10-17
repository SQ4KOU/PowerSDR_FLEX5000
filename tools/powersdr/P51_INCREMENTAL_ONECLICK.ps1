[CmdletBinding()]
param([string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if([string]::IsNullOrWhiteSpace($SourceRoot)) {
    $SourceRoot = Join-Path $projectRoot '.work\powersdr-ke9ns'
}
if(!(Test-Path $SourceRoot)) {
    throw "P51 source folder not found: $SourceRoot. Build or retain P50 workspace first."
}
$SourceRoot=(Resolve-Path $SourceRoot).Path

$logDir=Join-Path $projectRoot 'artifacts\powersdr\logs'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$log=Join-Path $logDir 'P51_INCREMENTAL_MSBUILD.log'
$csproj=Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$p50=Join-Path $SourceRoot 'Console\P50WindowPositions.cs'
$p51=Join-Path $SourceRoot 'Console\P51BandscopeResize.cs'
if(!(Test-Path $csproj) -or !(Test-Path $p50)) {
    throw 'P51 incremental requires the existing, already built P50 source tree.'
}

Write-Host '[1/4] Verify P50 base and apply only P51 patch'
if(!(Test-Path $p51)) {
    & (Join-Path $PSScriptRoot 'Apply-PowerSDR-WindowResize-P51.ps1') -SourceRoot $SourceRoot
}
else {
    $body=[IO.File]::ReadAllText($p50)
    $project=[IO.File]::ReadAllText($csproj)
    if(!$body.Contains('P51FilterBandscopeResize(message)') -or
       !$project.Contains('<Compile Include="P51BandscopeResize.cs" />')) {
        throw 'P51 is partially applied; resolve inconsistent source before build.'
    }
    Write-Host 'P51 already applied: no source modifications.'
}

Write-Host '[2/4] Resolve existing MSBuild (no configure or full rebuild)'
$vswhere=Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Microsoft Visual Studio\Installer\vswhere.exe'
if(!(Test-Path $vswhere)){throw "vswhere not found: $vswhere"}
$msbuild=& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if(!$msbuild){throw 'MSBuild not found'}

Write-Host '[3/4] Incremental x86 compile – live diagnostics'
& $msbuild $csproj '/m' '/t:Build' '/p:Configuration=Release' '/p:Platform=x86' '/p:BuildProjectReferences=false' '/v:normal' "/flp:logfile=$log;verbosity=normal" 2>&1 |
    Tee-Object -FilePath (Join-Path $logDir 'P51_INCREMENTAL_CONSOLE.log')
if($LASTEXITCODE -ne 0){throw "P51 compile FAILED (exit=$LASTEXITCODE), log=$log"}

Write-Host '[4/4] Verify actual executable and SHA-256'
$exe=Join-Path $SourceRoot 'bin\Release\PowerSDR.exe'
if(!(Test-Path $exe)){throw "Incremental compiler returned no PowerSDR.exe: $exe"}
$version=[Diagnostics.FileVersionInfo]::GetVersionInfo($exe).FileVersion
if($version -ne '2.8.0.336') {
    throw "Unexpected executable version: $version"
}
$hash=(Get-FileHash -Algorithm SHA256 -Path $exe).Hash.ToLowerInvariant()
Write-Host "P51_INCREMENTAL_BUILD=PASS"
Write-Host "P51_EXE=$exe"
Write-Host "P51_SHA256=$hash"
Write-Host "P51_LOG=$log"
Write-Host 'P51_DEPLOY=NOT_PERFORMED'
