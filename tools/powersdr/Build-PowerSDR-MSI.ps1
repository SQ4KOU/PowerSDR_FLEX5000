param([Parameter(Mandatory=$true)][string]$WixBin)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$HarnessRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$WorkRoot = Join-Path $HarnessRoot '.work\powersdr-ke9ns'
$ArtifactRoot = Join-Path $HarnessRoot 'artifacts\powersdr'
$LogRoot = Join-Path $ArtifactRoot 'logs'
$SourceRepo = 'https://github.com/ke9ns/PowerSDR-KE9NS-v2.8.0.git'
$SourceSha = 'fb05ec170fd09f32039afc4cdee7c119e08a2c29'
$DepsMsiUrl = 'https://github.com/ke9ns/PowerSDR-KE9NS-v2.8.0/releases/download/v2.8.0.329/PowerSDR_KE9NS_V2.8.0.329_Incremental_Installer.msi'
$DepsMsiSha256 = '6cb0f4aa820e4d7366e962e4c6f06eaf50326d886e87038d465e8a1f86e4e41c'

if (Test-Path $WorkRoot) { Remove-Item $WorkRoot -Recurse -Force }
if (Test-Path $ArtifactRoot) { Remove-Item $ArtifactRoot -Recurse -Force }
New-Item -ItemType Directory -Force -Path $WorkRoot,$ArtifactRoot,$LogRoot | Out-Null

Write-Host "POWERSDR_SOURCE=$SourceRepo@$SourceSha"
& git clone --no-tags $SourceRepo $WorkRoot
if ($LASTEXITCODE -ne 0) { throw "PowerSDR clone failed rc=$LASTEXITCODE" }
Push-Location $WorkRoot
try {
    & git checkout --detach $SourceSha
    if ($LASTEXITCODE -ne 0) { throw "PowerSDR checkout failed rc=$LASTEXITCODE" }
    if ((git rev-parse HEAD).Trim() -ne $SourceSha) { throw 'Pinned source SHA mismatch' }

    $outDir = Join-Path $WorkRoot 'bin\Release'
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null

    # KE9NS explicitly documents that his installed PowerSDR DLL set is required
    # to compile the GitHub source.  Extract the official release MSI and seed
    # bin\Release with that complete runtime.  This keeps native FLEX-5000,
    # PAL/FWC, FireWire/ASIO and DttSP exactly on the PowerSDR side.
    $depsMsi = Join-Path $WorkRoot '_ke9ns_runtime.msi'
    $depsRoot = Join-Path $WorkRoot '_ke9ns_runtime'
    Invoke-WebRequest -UseBasicParsing -Uri $DepsMsiUrl -OutFile $depsMsi
    $h = (Get-FileHash -Algorithm SHA256 $depsMsi).Hash.ToLowerInvariant()
    if ($h -ne $DepsMsiSha256) { throw "KE9NS MSI SHA256 mismatch: $h" }
    New-Item -ItemType Directory -Force -Path $depsRoot | Out-Null
    $msiArgs = @('/a',"`"$depsMsi`"",'/qn',"TARGETDIR=`"$depsRoot`"",'/L*v',"`"$(Join-Path $LogRoot 'KE9NS_RUNTIME_EXTRACT.log')`"")
    $p = Start-Process msiexec.exe -ArgumentList $msiArgs -Wait -PassThru
    if ($p.ExitCode -ne 0) { throw "KE9NS MSI administrative extraction failed rc=$($p.ExitCode)" }

    $installedExe = Get-ChildItem $depsRoot -Recurse -File -Filter PowerSDR.exe | Select-Object -First 1
    if (!$installedExe) { throw 'PowerSDR.exe not found in official KE9NS MSI' }
    $installedDir = $installedExe.Directory.FullName
    Copy-Item (Join-Path $installedDir '*') $outDir -Recurse -Force
    Write-Host "KE9NS_RUNTIME=$installedDir"

    $mustRuntime = @(
        'DttSP.dll','Interop.TDxInput.dll','Sanford.Collections.dll',
        'Sanford.Multimedia.dll','Sanford.Multimedia.Midi.dll',
        'Sanford.Multimedia.Timers.dll','Sanford.Threading.dll','TNF.dll'
    )
    foreach ($f in $mustRuntime) {
        if (!(Test-Path (Join-Path $outDir $f))) { throw "Required upstream runtime missing: $f" }
    }

    nuget restore (Join-Path $WorkRoot 'PowerSDR.sln') -NonInteractive |
        Tee-Object -FilePath (Join-Path $LogRoot 'NUGET_RESTORE.log')
    if ($LASTEXITCODE -ne 0) { throw "NuGet restore failed rc=$LASTEXITCODE" }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (!$msbuild) { throw 'MSBuild not found' }

    # PowerMate is a compile-time C++/CLI reference. Prefer the official copy;
    # otherwise build the unchanged upstream project. DttSP is deliberately not
    # rebuilt: the official PowerSDR DttSP.dll remains the DSP engine.
    $powerMateDll = Join-Path $outDir 'PowerMate.dll'
    if (!(Test-Path $powerMateDll)) {
        $pmLog = Join-Path $LogRoot 'MSBUILD_POWERMATE.log'
        & $msbuild (Join-Path $WorkRoot 'PowerMate\PowerMate.vcxproj') '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=Win32' '/v:minimal' "/flp:logfile=$pmLog;verbosity=normal"
        if ($LASTEXITCODE -ne 0) { throw "PowerMate build failed rc=$LASTEXITCODE" }
        $builtPM = Join-Path $WorkRoot 'PowerMate\bin\Release\PowerMate.dll'
        if (!(Test-Path $builtPM)) { throw "PowerMate output missing: $builtPM" }
        Copy-Item $builtPM $powerMateDll -Force
    }

    # Fix only clean-CI project wiring: point the C# project at the existing x86
    # PowerMate assembly. No radio/backend source is modified here.
    $csproj = Join-Path $WorkRoot 'Console\PowerSDR.csproj'
    $cs = [IO.File]::ReadAllText($csproj)
    $prx = [regex]::new('(?ms)\s*<ProjectReference Include="\.\.\\PowerMate\\PowerMate\.vcxproj">.*?</ProjectReference>')
    if ($prx.Matches($cs).Count -ne 1) { throw 'Unexpected PowerMate ProjectReference layout' }
    $pmRef = "`r`n    <Reference Include=`"PowerMate`">`r`n      <HintPath>..\bin\Release\PowerMate.dll</HintPath>`r`n      <Private>True</Private>`r`n    </Reference>"
    $cs = $prx.Replace($cs,$pmRef,1)
    [IO.File]::WriteAllText($csproj,$cs,(New-Object Text.UTF8Encoding($false)))

    # UI-only port. This script may not modify FWC, audio, DttSP, ATU, Mixer,
    # PAL, FireWire/ASIO, hardware transport or their source files.
    $ui = Join-Path $HarnessRoot 'tools\powersdr\Apply-PowerSDR-ThetisUI.ps1'
    if (Test-Path $ui) { & $ui -SourceRoot $WorkRoot }

    $buildLog = Join-Path $LogRoot 'MSBUILD_POWERSDR.log'
    $binlog = Join-Path $LogRoot 'MSBUILD_POWERSDR.binlog'
    & $msbuild $csproj '/m' '/t:Rebuild' '/p:Configuration=Release' '/p:Platform=x86' '/p:BuildProjectReferences=false' '/v:minimal' "/flp:logfile=$buildLog;verbosity=normal" "/bl:$binlog"
    if ($LASTEXITCODE -ne 0) { throw "PowerSDR x86 build failed rc=$LASTEXITCODE" }

    $exe = Join-Path $outDir 'PowerSDR.exe'
    if (!(Test-Path $exe)) { throw 'PowerSDR.exe missing after build' }
    if (Test-Path (Join-Path $outDir 'Thetis.exe')) { throw 'Thetis.exe leaked into PowerSDR output' }

    foreach ($rel in @('Console\FWC\fwc.cs','Console\FWC\fwcatuform.cs','Console\console.Designer.cs')) {
        if (!(Test-Path (Join-Path $WorkRoot $rel))) { throw "PowerSDR native source missing: $rel" }
    }
    $designer = [IO.File]::ReadAllText((Join-Path $WorkRoot 'Console\console.Designer.cs'))
    foreach ($token in @('mixerToolStripMenuItem','aTUToolStripMenuItem','antennaToolStripMenuItem','chkFWCATU')) {
        if (!$designer.Contains($token)) { throw "PowerSDR native function gate failed: $token" }
    }

    $fv = [Diagnostics.FileVersionInfo]::GetVersionInfo($exe).FileVersion
    if (!$fv) { $fv='2.8.0.0' }
    $vm = [regex]::Match($fv,'(\d+)\.(\d+)\.(\d+)')
    if (!$vm.Success) { throw "Bad PowerSDR file version: $fv" }
    $msiVersion = "$($vm.Groups[1].Value).$($vm.Groups[2].Value).$($vm.Groups[3].Value)"

    $wixWork = Join-Path $WorkRoot '_sq4kou_msi'
    New-Item -ItemType Directory -Force $wixWork | Out-Null
    $harvest = Join-Path $wixWork 'Harvest.wxs'
    $product = Join-Path $wixWork 'Product.wxs'
    $heat=Join-Path $WixBin 'heat.exe'; $candle=Join-Path $WixBin 'candle.exe'; $light=Join-Path $WixBin 'light.exe'
    foreach($x in @($heat,$candle,$light)){if(!(Test-Path $x)){throw "WiX tool missing: $x"}}
    & $heat dir $outDir '-cg' 'AppFiles' '-dr' 'INSTALLFOLDER' '-gg' '-scom' '-sreg' '-sfrag' '-srd' '-var' 'var.SourceDir' '-out' $harvest
    if($LASTEXITCODE -ne 0){throw "WiX heat failed rc=$LASTEXITCODE"}

    $xml=@"
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
 <Product Id="*" Name="PowerSDR" Language="1033" Version="$msiVersion" Manufacturer="FlexRadio Systems / KE9NS / SQ4KOU" UpgradeCode="A7432079-7327-4DAB-B044-8749A16C53A1">
  <Package InstallerVersion="500" Compressed="yes" InstallScope="perMachine" Platform="x86" />
  <MajorUpgrade DowngradeErrorMessage="A newer version of PowerSDR is already installed." />
  <MediaTemplate EmbedCab="yes" CompressionLevel="high" />
  <Property Id="WIXUI_INSTALLDIR" Value="INSTALLFOLDER"/><UIRef Id="WixUI_InstallDir"/>
  <Directory Id="TARGETDIR" Name="SourceDir">
   <Directory Id="ProgramFilesFolder"><Directory Id="FlexRadioDir" Name="FlexRadio Systems"><Directory Id="INSTALLFOLDER" Name="PowerSDR"/></Directory></Directory>
   <Directory Id="ProgramMenuFolder"><Directory Id="ProgramMenuPowerSDR" Name="PowerSDR"><Component Id="MenuShortcut" Guid="*"><Shortcut Id="PowerSDRMenu" Name="PowerSDR" Target="[INSTALLFOLDER]PowerSDR.exe" WorkingDirectory="INSTALLFOLDER"/><RemoveFolder Id="RemoveMenu" On="uninstall"/><RegistryValue Root="HKCU" Key="Software\PowerSDR\SQ4KOU" Name="Menu" Type="integer" Value="1" KeyPath="yes"/></Component></Directory></Directory>
   <Directory Id="DesktopFolder"><Component Id="DesktopShortcut" Guid="*"><Shortcut Id="PowerSDRDesktop" Name="PowerSDR" Target="[INSTALLFOLDER]PowerSDR.exe" WorkingDirectory="INSTALLFOLDER"/><RegistryValue Root="HKCU" Key="Software\PowerSDR\SQ4KOU" Name="Desktop" Type="integer" Value="1" KeyPath="yes"/></Component></Directory>
  </Directory>
  <Feature Id="MainFeature" Title="PowerSDR" Level="1"><ComponentGroupRef Id="AppFiles"/><ComponentRef Id="MenuShortcut"/><ComponentRef Id="DesktopShortcut"/></Feature>
 </Product>
</Wix>
"@
    [IO.File]::WriteAllText($product,$xml,(New-Object Text.UTF8Encoding($false)))

    Push-Location $wixWork
    try {
        & $candle '-arch' 'x86' "-dSourceDir=$outDir" '-ext' 'WixUIExtension' 'Product.wxs' 'Harvest.wxs'
        if($LASTEXITCODE -ne 0){throw "WiX candle failed rc=$LASTEXITCODE"}
        $safe=$fv -replace '[^0-9A-Za-z._-]','_'
        $name="PowerSDR-SQ4KOU-FLEX5000-v$safe.x86.msi"
        $final=Join-Path $ArtifactRoot $name
        & $light '-ext' 'WixUIExtension' '-sice:ICE61' '-out' $final 'Product.wixobj' 'Harvest.wixobj'
        if($LASTEXITCODE -ne 0){throw "WiX light failed rc=$LASTEXITCODE"}
    } finally { Pop-Location }

    $sha=(Get-FileHash -Algorithm SHA256 $final).Hash.ToLowerInvariant()
    "$sha  $name" | Set-Content (Join-Path $ArtifactRoot ($name+'.sha256')) -Encoding ASCII
    @(
      'PRODUCT=PowerSDR','EXE=PowerSDR.exe',"POWERSDR_FILE_VERSION=$fv",
      "POWERSDR_SOURCE_REPO=$SourceRepo","POWERSDR_SOURCE_SHA=$SourceSha",
      "KE9NS_RUNTIME_SOURCE_SHA256=$DepsMsiSha256",'ARCH=x86','BASE=KE9NS_POWERSDR',
      'FLEX5000_BACKEND=POWERSDR_NATIVE_PAL_FWC_FIREWIRE_ASIO','ATU=POWERSDR_NATIVE',
      'MIXER=POWERSDR_NATIVE','DSP=POWERSDR_NATIVE_DTTSP','THETIS_BACKEND=ABSENT',
      'THETIS_NETWORKIO=ABSENT','THETIS_CHANNELMASTER=ABSENT','THETIS_WDSP=ABSENT',
      'UI_DIRECTION=THETIS_LAYOUT_ON_POWERSDR',"MSI=$name","MSI_SHA256=$sha"
    ) | Set-Content (Join-Path $ArtifactRoot 'POWERSDR_BUILD_MANIFEST.txt') -Encoding UTF8
    Write-Host "POWERSDR_MSI_READY=$final"
    Write-Host "POWERSDR_MSI_SHA256=$sha"
} finally { Pop-Location }
