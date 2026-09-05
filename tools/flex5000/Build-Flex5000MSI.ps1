param(
    [string]$Configuration = 'Release',
    [string]$Platform = 'x86',
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$src = Join-Path $repo 'Project Files\Source'
$sln = Join-Path $src 'Thetis_VS2026.sln'
$wix = Join-Path $src 'Thetis-Installer\Thetis-Installer.wixproj'
$artifactDir = Join-Path $repo 'artifacts\flex5000'
$logDir = Join-Path $artifactDir 'logs'

New-Item -ItemType Directory -Force -Path $artifactDir,$logDir | Out-Null
Get-ChildItem -LiteralPath $artifactDir -File -ErrorAction SilentlyContinue | Remove-Item -Force

# Checkout-only source integration. Errors throw and stop the build.
& (Join-Path $PSScriptRoot 'Apply-Flex5000Overlay.ps1')
& (Join-Path $PSScriptRoot 'Apply-Flex5000Ptt.ps1')
& (Join-Path $PSScriptRoot 'Apply-Flex5000Cleanup.ps1')
& (Join-Path $PSScriptRoot 'Apply-Flex5000Audio192k.ps1')

# SQ4KOU x86 currently has no prebuilt NR_Algorithms_x86. Build the exact pinned
# RNNoise/SpecBleach sources as Win32 before WDSP, then patch only the disposable
# Release|Win32 WDSP link configuration.
& (Join-Path $PSScriptRoot 'Prepare-Flex5000NrX86.ps1') -LogDir $logDir

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (!(Test-Path -LiteralPath $vswhere)) { throw 'vswhere.exe not found' }
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (!$msbuild -or !(Test-Path -LiteralPath $msbuild)) { throw 'MSBuild not found' }
Write-Host "MSBUILD=$msbuild"

if (!$SkipRestore) {
    $nugetCmd = Get-Command nuget.exe -ErrorAction SilentlyContinue | Select-Object -First 1
    $nuget = if ($null -ne $nugetCmd) { $nugetCmd.Source } else { $null }
    if (!$nuget) {
        $nuget = Join-Path $env:TEMP 'nuget-flex5000.exe'
        Invoke-WebRequest -UseBasicParsing -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile $nuget
    }
    Write-Host "NUGET=$nuget"
    & $nuget restore $sln -NonInteractive
    if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed rc=$LASTEXITCODE" }
}

$buildLog = Join-Path $logDir 'MSBUILD_FLEX5000.log'
$binlog = Join-Path $logDir 'MSBUILD_FLEX5000.binlog'
$args = @(
    $sln,
    '/m',
    '/t:Build',
    "/p:Configuration=$Configuration",
    "/p:Platform=$Platform",
    '/p:BuildProjectReferences=true',
    '/v:minimal',
    "/flp:logfile=$buildLog;verbosity=normal",
    "/bl:$binlog"
)
& $msbuild @args
if ($LASTEXITCODE -ne 0) { throw "Thetis solution build failed rc=$LASTEXITCODE" }

$thetisExe = Join-Path $repo "Project Files\bin\$Platform\$Configuration\Thetis.exe"
if (!(Test-Path -LiteralPath $thetisExe)) { throw "Thetis.exe missing: $thetisExe" }
$cmAsio = Join-Path $repo "Project Files\bin\$Platform\$Configuration\cmASIO.dll"
if (!(Test-Path -LiteralPath $cmAsio)) { throw "cmASIO.dll missing: $cmAsio" }
$wdspDll = Join-Path $repo "Project Files\bin\$Platform\$Configuration\wdsp.dll"
if (!(Test-Path -LiteralPath $wdspDll)) { throw "wdsp.dll missing: $wdspDll" }

# Build installer explicitly as final gate.
$wixLog = Join-Path $logDir 'MSBUILD_FLEX5000_WIX.log'
& $msbuild $wix '/m' '/t:Build' "/p:Configuration=$Configuration" "/p:Platform=$Platform" '/v:minimal' "/flp:logfile=$wixLog;verbosity=normal"
if ($LASTEXITCODE -ne 0) { throw "WiX MSI build failed rc=$LASTEXITCODE" }

$installerDir = Join-Path $repo 'Project Files\bin\Installers'
$msi = Get-ChildItem -LiteralPath $installerDir -Filter '*.x86.msi' -File | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (!$msi) { throw "No x86 MSI found in $installerDir" }

$fv = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($thetisExe).FileVersion
if (!$fv) { $fv = 'unknown' }
$safeVersion = ($fv -replace '[^0-9A-Za-z._-]', '_')
$finalName = "Thetis-FLEX5000-P0-SQ4KOU-v$safeVersion.x86.msi"
$finalMsi = Join-Path $artifactDir $finalName
Copy-Item -LiteralPath $msi.FullName -Destination $finalMsi -Force

$sha = (Get-FileHash -Algorithm SHA256 -LiteralPath $finalMsi).Hash.ToLowerInvariant()
"$sha  $finalName" | Set-Content -LiteralPath (Join-Path $artifactDir ($finalName + '.sha256')) -Encoding ASCII

$branch = if (Test-Path Env:GITHUB_REF_NAME) { $env:GITHUB_REF_NAME } else { 'local' }
$sourceSha = if (Test-Path Env:GITHUB_SHA) { $env:GITHUB_SHA } else { 'local' }
$manifest = @(
    'BUILD=FLEX5000_P0_SQ4KOU',
    "SOURCE_BRANCH=$branch",
    "SOURCE_SHA=$sourceSha",
    "THETIS_EXE=$thetisExe",
    "THETIS_FILE_VERSION=$fv",
    "CMASIO=$cmAsio",
    "WDSP=$wdspDll",
    "MSI=$finalName",
    "MSI_SHA256=$sha",
    'ARCH=x86',
    'TRANSPORT=PAL_FWC_ASIO_FLEXRADIO_8X8_192K',
    'DSP=CHANNELMASTER_WDSP',
    'HPSDR_RNET=DISABLED',
    'PHYSICAL_PTT=PAL_FWC_READ_PTT_EDGE_LOGGED',
    'TX_INPUT=ASIO_CH6_CH7_NATIVE_192K',
    'RX_AUDIO=CHANNELMASTER_AAMIX_NATIVE_192K',
    'NR_X86=RNNOISE_GENERIC_PLUS_SPECBLEACH'
)
$manifest | Set-Content -LiteralPath (Join-Path $artifactDir 'FLEX5000_BUILD_MANIFEST.txt') -Encoding UTF8

Write-Host "MSI_READY=$finalMsi"
Write-Host "MSI_SHA256=$sha"