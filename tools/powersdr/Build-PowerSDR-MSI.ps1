param(
    [Parameter(Mandatory=$true)][string]$WixBin
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$HarnessRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$WorkRoot = Join-Path $HarnessRoot '.work\powersdr-ke9ns'
$ArtifactRoot = Join-Path $HarnessRoot 'artifacts\powersdr'
$LogRoot = Join-Path $ArtifactRoot 'logs'
$SourceRepo = 'https://github.com/ke9ns/PowerSDR-KE9NS-v2.8.0.git'
$SourceSha = 'fb05ec170fd09f32039afc4cdee7c119e08a2c29'

# KE9NS documents that a local v2.8.0 installation is required to compile the
# GitHub source because several runtime/build DLLs are intentionally not stored
# in the repository.  Use the official KE9NS release MSI as a reproducible source
# for those DLLs, but do not replace any source or backend code with binaries.
$DepsMsiUrl = 'https://github.com/ke9ns/PowerSDR-KE9NS-v2.8.0/releases/download/v2.8.0.329/PowerSDR_KE9NS_V2.8.0.329_Incremental_Installer.msi'
$DepsMsiSha256 = '6cb0f4aa820e4d7366e962e4c6f06eaf50326d886e87038d465e8a1f86e4e41c'

if (Test-Path $WorkRoot) { Remove-Item -LiteralPath $WorkRoot -Recurse -Force }
if (Test-Path $ArtifactRoot) { Remove-Item -LiteralPath $ArtifactRoot -Recurse -Force }
New-Item -ItemType Directory -Force -Path $WorkRoot,$ArtifactRoot,$LogRoot | Out-Null

function Invoke-Checked([string]$Exe, [string[]]$Arguments, [string]$What) {
    & $Exe @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$What failed rc=$LASTEXITCODE" }
}

Write-Host "Cloning native PowerSDR KE9NS source at pinned SHA $SourceSha"
Invoke-Checked 'git' @('clone','--no-tags',$SourceRepo,$WorkRoot) 'PowerSDR clone'

Push-Location $WorkRoot
try {
    Invoke-Checked 'git' @('checkout','--detach',$SourceSha) 'PowerSDR checkout'
    $actual = (git rev-parse HEAD).Trim()
    if ($actual -ne $SourceSha) { throw "Pinned source mismatch: $actual" }

    $outDir = Join-Path $WorkRoot 'bin\Release'
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null

    # ----------------------------------------------------------------------
    # Reproduce the upstream prerequisite step: populate bin\Release with the
    # DLLs shipped by KE9NS.  This is exactly the dependency model documented
    # by the upstream README and avoids stubbing/removing PowerSDR features.
    # ----------------------------------------------------------------------
    $depsMsi = Join-Path $WorkRoot '_ke9ns_deps.msi'
    $depsRoot = Join-Path $WorkRoot '_ke9ns_deps'
    $depsLog = Join-Path $LogRoot 'KE9NS_DEPENDENCY_EXTRACT.log'
    Write-Host 'Downloading official KE9NS PowerSDR dependency MSI...'
    Invoke-WebRequest -UseBasicParsing -Uri $DepsMsiUrl -OutFile $depsMsi
    $depsHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $depsMsi).Hash.ToLowerInvariant()
    if ($depsHash -ne $DepsMsiSha256) { throw "KE9NS dependency MSI hash mismatch: $depsHash" }
    New-Item -ItemType Directory -Force -Path $depsRoot | Out-Null

    $msiArgs = @('/a',"`"$depsMsi`"",'/qn',"TARGETDIR=`"$depsRoot`"",'/L*v',"`"$depsLog`"")
    $p = Start-Process -FilePath 'msiexec.exe' -ArgumentList $msiArgs -Wait -PassThru
    if ($p.ExitCode -ne 0) { throw "Administrative extraction of KE9NS MSI failed rc=$($p.ExitCode)" }

    $installedExe = Get-ChildItem -LiteralPath $depsRoot -Recurse -File -Filter 'PowerSDR.exe' -ErrorAction SilentlyContinue |
        Sort-Object FullName | Select-Object -First 1
    if (!$installedExe) { throw 'PowerSDR.exe not found in extracted official KE9NS MSI' }
    $installedDir = $installedExe.Directory.FullName
    $upstreamDlls = Get-ChildItem -LiteralPath $installedDir -File -Filter '*.dll'
    if ($upstreamDlls.Count -lt 10) { throw "Too few KE9NS DLLs extracted from $installedDir : $($upstreamDlls.Count)" }
    Copy-Item -LiteralPath $upstreamDlls.FullName -Destination $outDir -Force
    Write-Host "Copied $($upstreamDlls.Count) official KE9NS DLLs into bin\Release"

    # Upstream README builds PowerMate as a project reference.  The checked-in
    # solution does not mark PowerMate Build.0 for Release|Win32, so CI must
    # build the referenced C++/CLI assembly explicitly before compiling C#.
    Write-Host 'Restoring PowerSDR packages...'
    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive | Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE.log')
    if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed rc=$LASTEXITCODE" }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (!(Test-Path $vswhere)) { throw 'vswhere.exe not found' }
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (!$msbuild -or !(Test-Path $msbuild)) { throw 'MSBuild not found' }
    Write-Host "MSBUILD=$msbuild"

    $nativeLog = Join-Path $LogRoot 'MSBUILD_NATIVE.log'
    & $msbuild (Join-Path $WorkRoot 'PowerMate\PowerMate.vcxproj') '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=Win32' '/v:minimal' "/flp:logfile=$nativeLog;verbosity=normal"
    if ($LASTEXITCODE -ne 0) { throw "PowerMate build failed rc=$LASTEXITCODE" }
    $powerMateDll = Join-Path $outDir 'PowerMate.dll'
    if (!(Test-Path -LiteralPath $powerMateDll)) { throw "PowerMate.dll missing after native build: $powerMateDll" }

    & $msbuild (Join-Path $WorkRoot 'DttSP\DttSP.vcxproj') '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=Win32' '/v:minimal' "/flp:logfile=$nativeLog;verbosity=normal;append"
    if ($LASTEXITCODE -ne 0) { throw "DttSP build failed rc=$LASTEXITCODE" }

    # Resolve PowerMate as the already-built x86 assembly.  This changes only
    # CI project wiring; no PowerSDR source, PAL/FWC, ATU, Mixer or DSP logic.
    $csproj = Join-Path $WorkRoot 'Console\PowerSDR.csproj'
    $cs = [IO.File]::ReadAllText($csproj)
    $rx = [regex]::new('(?ms)\s*<ProjectReference Include="\.\.\\PowerMate\\PowerMate\.vcxproj">.*?</ProjectReference>')
    $pm = $rx.Matches($cs)
    if ($pm.Count -ne 1) { throw "PowerMate ProjectReference expected=1 actual=$($pm.Count)" }
    $replacement = @"
    <Reference Include="PowerMate">
      <HintPath>..\bin\Release\PowerMate.dll</HintPath>
      <Private>True</Private>
    </Reference>
"@
    $cs = $rx.Replace($cs, "`r`n$replacement", 1)
    [IO.File]::WriteAllText($csproj, $cs, (New-Object Text.UTF8Encoding($false)))

    # Apply only frontend/UI changes after the pristine backend/dependency gates.
    # This script is forbidden from modifying PAL/FWC, FireWire/ASIO, DSP, ATU,
    # Mixer, hardware I/O or their project files.
    $uiPatch = Join-Path $HarnessRoot 'tools\powersdr\Apply-PowerSDR-ThetisUI.ps1'
    if (Test-Path $uiPatch) { & $uiPatch -SourceRoot $WorkRoot }

    $buildLog = Join-Path $LogRoot 'MSBUILD_POWERSDR.log'
    $binlog = Join-Path $LogRoot 'MSBUILD_POWERSDR.binlog'
    & $msbuild $csproj '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=x86' '/p:BuildProjectReferences=false' '/v:minimal' "/flp:logfile=$buildLog;verbosity=normal" "/bl:$binlog"
    if ($LASTEXITCODE -ne 0) { throw "PowerSDR managed build failed rc=$LASTEXITCODE" }

    $exe = Join-Path $outDir 'PowerSDR.exe'
    if (!(Test-Path -LiteralPath $exe)) { throw "PowerSDR.exe missing: $exe" }
    if (Test-Path -LiteralPath (Join-Path $outDir 'Thetis.exe')) { throw 'Thetis.exe present in PowerSDR build output' }

    # Hard source gates for native FLEX-5000 functionality.  Do not depend on
    # guessed class/file names for Mixer: verify the actual PowerSDR menu wiring.
    foreach ($rel in @('Console\FWC\fwc.cs','Console\FWC\fwcatuform.cs','Console\console.Designer.cs')) {
        if (!(Test-Path -LiteralPath (Join-Path $WorkRoot $rel))) { throw "Required native PowerSDR FLEX source missing: $rel" }
    }
    $designerText = [IO.File]::ReadAllText((Join-Path $WorkRoot 'Console\console.Designer.cs'))
    foreach ($token in @('mixerToolStripMenuItem','aTUToolStripMenuItem','antennaToolStripMenuItem','chkFWCATU')) {
        if (!$designerText.Contains($token)) { throw "Required PowerSDR function missing from native UI: $token" }
    }

    $fvi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exe)
    $fileVersion = $fvi.FileVersion
    if (!$fileVersion) { $fileVersion = '2.8.0.0' }
    $m = [regex]::Match($fileVersion, '(\d+)\.(\d+)\.(\d+)')
    if (!$m.Success) { throw "Cannot derive MSI version from PowerSDR.exe: $fileVersion" }
    $msiVersion = "$($m.Groups[1].Value).$($m.Groups[2].Value).$($m.Groups[3].Value)"

    $wixWork = Join-Path $WorkRoot '_sq4kou_msi'
    New-Item -ItemType Directory -Force -Path $wixWork | Out-Null
    $harvest = Join-Path $wixWork 'Harvest.wxs'
    $product = Join-Path $wixWork 'Product.wxs'

    $heat = Join-Path $WixBin 'heat.exe'
    $candle = Join-Path $WixBin 'candle.exe'
    $light = Join-Path $WixBin 'light.exe'
    foreach ($tool in @($heat,$candle,$light)) { if (!(Test-Path $tool)) { throw "WiX tool missing: $tool" } }

    & $heat dir $outDir '-cg' 'AppFiles' '-dr' 'INSTALLFOLDER' '-gg' '-scom' '-sreg' '-sfrag' '-srd' '-var' 'var.SourceDir' '-out' $harvest
    if ($LASTEXITCODE -ne 0) { throw "WiX heat failed rc=$LASTEXITCODE" }

    $upgradeCode = 'A7432079-7327-4DAB-B044-8749A16C53A1'
    $productXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="PowerSDR" Language="1033" Version="$msiVersion" Manufacturer="FlexRadio Systems / KE9NS / SQ4KOU" UpgradeCode="$upgradeCode">
    <Package InstallerVersion="500" Compressed="yes" InstallScope="perMachine" Platform="x86" />
    <MajorUpgrade DowngradeErrorMessage="A newer version of PowerSDR is already installed." />
    <MediaTemplate EmbedCab="yes" CompressionLevel="high" />
    <Property Id="ARPPRODUCTICON" Value="PowerSDRIcon" />
    <Icon Id="PowerSDRIcon" SourceFile="$outDir\PowerSDR.exe" />
    <Property Id="WIXUI_INSTALLDIR" Value="INSTALLFOLDER" />
    <UIRef Id="WixUI_InstallDir" />
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="FlexRadioDir" Name="FlexRadio Systems">
          <Directory Id="INSTALLFOLDER" Name="PowerSDR" />
        </Directory>
      </Directory>
      <Directory Id="ProgramMenuFolder">
        <Directory Id="ProgramMenuPowerSDR" Name="PowerSDR">
          <Component Id="ProgramMenuShortcut" Guid="*">
            <Shortcut Id="PowerSDRStartMenuShortcut" Name="PowerSDR" Description="PowerSDR" Target="[INSTALLFOLDER]PowerSDR.exe" WorkingDirectory="INSTALLFOLDER" />
            <RemoveFolder Id="RemovePowerSDRProgramMenu" On="uninstall" />
            <RegistryValue Root="HKCU" Key="Software\PowerSDR\SQ4KOU" Name="StartMenuShortcut" Type="integer" Value="1" KeyPath="yes" />
          </Component>
        </Directory>
      </Directory>
      <Directory Id="DesktopFolder">
        <Component Id="DesktopShortcutComponent" Guid="*">
          <Shortcut Id="PowerSDRDesktopShortcut" Name="PowerSDR" Description="PowerSDR" Target="[INSTALLFOLDER]PowerSDR.exe" WorkingDirectory="INSTALLFOLDER" />
          <RegistryValue Root="HKCU" Key="Software\PowerSDR\SQ4KOU" Name="DesktopShortcut" Type="integer" Value="1" KeyPath="yes" />
        </Component>
      </Directory>
    </Directory>
    <Feature Id="MainFeature" Title="PowerSDR" Level="1">
      <ComponentGroupRef Id="AppFiles" />
      <ComponentRef Id="ProgramMenuShortcut" />
      <ComponentRef Id="DesktopShortcutComponent" />
    </Feature>
  </Product>
</Wix>
"@
    [IO.File]::WriteAllText($product, $productXml, (New-Object Text.UTF8Encoding($false)))

    Push-Location $wixWork
    try {
        & $candle '-arch' 'x86' "-dSourceDir=$outDir" '-ext' 'WixUIExtension' 'Product.wxs' 'Harvest.wxs'
        if ($LASTEXITCODE -ne 0) { throw "WiX candle failed rc=$LASTEXITCODE" }
        $safeFileVersion = $fileVersion -replace '[^0-9A-Za-z._-]', '_'
        $finalName = "PowerSDR-SQ4KOU-FLEX5000-v$safeFileVersion.x86.msi"
        $finalMsi = Join-Path $ArtifactRoot $finalName
        & $light '-ext' 'WixUIExtension' '-sice:ICE61' '-out' $finalMsi 'Product.wixobj' 'Harvest.wixobj'
        if ($LASTEXITCODE -ne 0) { throw "WiX light failed rc=$LASTEXITCODE" }
    }
    finally { Pop-Location }

    if (!(Test-Path -LiteralPath $finalMsi)) { throw 'MSI was not created' }
    $sha = (Get-FileHash -Algorithm SHA256 -LiteralPath $finalMsi).Hash.ToLowerInvariant()
    "$sha  $finalName" | Set-Content -LiteralPath (Join-Path $ArtifactRoot ($finalName + '.sha256')) -Encoding ASCII

    $manifest = @(
        'PRODUCT=PowerSDR',
        'EXE=PowerSDR.exe',
        "POWERSDR_FILE_VERSION=$fileVersion",
        "POWERSDR_SOURCE_REPO=$SourceRepo",
        "POWERSDR_SOURCE_SHA=$SourceSha",
        "KE9NS_RUNTIME_SOURCE_SHA256=$DepsMsiSha256",
        'ARCH=x86',
        'BASE=KE9NS_POWERSDR',
        'FLEX5000_BACKEND=POWERSDR_NATIVE_PAL_FWC_FIREWIRE_ASIO',
        'ATU=POWERSDR_NATIVE',
        'MIXER=POWERSDR_NATIVE',
        'DSP=POWERSDR_NATIVE_DTTSP',
        'THETIS_BACKEND=ABSENT',
        'THETIS_NETWORKIO=ABSENT',
        'THETIS_CHANNELMASTER=ABSENT',
        'THETIS_WDSP=ABSENT',
        'UI_DIRECTION=THETIS_LAYOUT_ON_POWERSDR',
        "MSI=$finalName",
        "MSI_SHA256=$sha"
    )
    $manifest | Set-Content -LiteralPath (Join-Path $ArtifactRoot 'POWERSDR_BUILD_MANIFEST.txt') -Encoding UTF8

    Write-Host "POWERSDR_MSI_READY=$finalMsi"
    Write-Host "POWERSDR_MSI_SHA256=$sha"
}
finally {
    Pop-Location
}
