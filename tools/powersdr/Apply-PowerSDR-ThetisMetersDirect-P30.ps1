[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P30: P29 remains the accepted RX/config/persistence/normal-skin base.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P29.ps1') -SourceRoot $SourceRoot

$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$meterManagerPath=Join-Path $consoleDir 'P25_MeterManager.cs'
$bridgePath=Join-Path $consoleDir 'P25ThetisMetersBridge.cs'
$srcTxBridge=Join-Path $PSScriptRoot 'P30ThetisMetersTxBridge.cs'
$dstTxBridge=Join-Path $consoleDir 'P30ThetisMetersTxBridge.cs'
$skinDir=Join-Path $consoleDir 'MeterSkins'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

if(!(Test-Path $meterManagerPath)){throw "P30 MeterManager missing: $meterManagerPath"}
if(!(Test-Path $bridgePath)){throw "P30 P25 bridge missing: $bridgePath"}
if(!(Test-Path $srcTxBridge)){throw "P30 TX bridge missing: $srcTxBridge"}
Copy-Item $srcTxBridge $dstTxBridge -Force

# Add the TX adapter to the PowerSDR build.
$proj=[IO.File]::ReadAllText($projPath)
$compileAnchor='<Compile Include="P25ThetisMetersBridge.cs" />'
if(!$proj.Contains($compileAnchor)){throw 'P30 csproj P25 bridge anchor missing'}
if(!$proj.Contains('<Compile Include="P30ThetisMetersTxBridge.cs" />'))
{
    $proj=$proj.Replace($compileAnchor,$compileAnchor+$nl+'    <Compile Include="P30ThetisMetersTxBridge.cs" />')
}

# Load every official DefaultMeters image, not a hand-picked subset. The Thetis
# renderer selects -dark / power-rating / size variants dynamically by basename.
$ThetisSkinsSha='601018a9486359b3bdb661063f4687b4c75ad917'
$zipUrl="https://raw.githubusercontent.com/ramdor/ThetisSkins/$ThetisSkinsSha/DefaultMeters.zip"
$zipPath=Join-Path $env:TEMP 'P30_DefaultMeters.zip'
$extractDir=Join-Path $env:TEMP 'P30_DefaultMeters'

if(Test-Path $extractDir){Remove-Item $extractDir -Recurse -Force}
New-Item -ItemType Directory -Force -Path $extractDir | Out-Null
New-Item -ItemType Directory -Force -Path $skinDir | Out-Null
Invoke-WebRequest -UseBasicParsing -Uri $zipUrl -OutFile $zipPath
Expand-Archive -Path $zipPath -DestinationPath $extractDir -Force

$images=@(
    Get-ChildItem $extractDir -Recurse -File | Where-Object {
        $_.Extension -match '^\.(png|jpg|jpeg|bmp)$'
    }
)
if($images.Count -lt 1){throw 'P30 official DefaultMeters.zip contains no image files'}

$seen=@{}
foreach($f in $images)
{
    $key=$f.Name.ToLowerInvariant()
    if($seen.ContainsKey($key)){continue}
    $seen[$key]=$true

    $dst=Join-Path $skinDir $f.Name
    Copy-Item $f.FullName $dst -Force

    $rel='MeterSkins\'+$f.Name
    if($proj -notmatch ('Content Include="'+[regex]::Escape($rel)+'"'))
    {
        $item='    <Content Include="'+$rel+'">'+$nl+
              '      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>'+$nl+
              '    </Content>'+$nl
        $igPos=$proj.IndexOf('</ItemGroup>')
        if($igPos -lt 0){throw 'P30 csproj ItemGroup close missing'}
        $proj=$proj.Insert($igPos,$item)
    }
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

$skinBaseNames=@(Get-ChildItem $skinDir -File | ForEach-Object { $_.BaseName })
Write-Host ('P30_SKIN_LIST='+($skinBaseNames -join ','))

# These are the exact basenames selected by MeterManager.renderImage when
# DarkMode is enabled on the primary ANANMM/CROSS image items.
foreach($required in @('ananMM-dark','cross-needle-dark'))
{
    if(-not ($skinBaseNames -contains $required))
    {
        throw "P30 required official dark-mode meter skin missing: $required"
    }
}

# Tell the unmodified Thetis scale builder that FLEX-5000 is a 100 W PA before
# restored/new PWR/CROSS/ANAN meter groups are constructed.
$mm=[IO.File]::ReadAllText($meterManagerPath)

# P30 compatibility adaptation: the official DefaultMeters pack has dark primary
# artwork for ANANMM and CROSS, but no ananMM-bg-tx-dark. The pinned 2023
# renderer blindly appends "-dark" to every Primary clsImage. Fall back to the
# corresponding normal image only when the requested dark basename is absent.
$renderOld='                string sImage = img.ImageName + (img.DarkMode ? "-dark" : "");'
$renderNew='                string sImage = img.ImageName + (img.DarkMode ? "-dark" : "");'+$nl+
           '                if (img.DarkMode && !MeterManager.ContainsBitmap(sImage)) sImage = img.ImageName;'
if(!$mm.Contains($renderOld)){throw 'P30 renderImage dark fallback anchor missing'}
$mm=$mm.Replace($renderOld,$renderNew)

$powerAnchor='        public static int GetMeterTXRXType(MeterType meter)'
if(!$mm.Contains($powerAnchor)){throw 'P30 MeterManager power-rating anchor missing'}
if(!$mm.Contains('internal static void P30ConfigureFlex5000()'))
{
$powerMethod=@'
        internal static void P30ConfigureFlex5000()
        {
            _paPresent = true;
            _alexPresent = false;
            _apolloPresent = false;
            _transverterIndex = -1;
        }

'@
    $mm=$mm.Replace($powerAnchor,$powerMethod+$powerAnchor)
}

# P25 deliberately returned during MOX because TX was not yet ported. Replace only
# that gate; the existing RX1 code below it stays byte-for-byte in place.
$oldGate='            if (!_power || mox) return;'
if(!$mm.Contains($oldGate)){throw 'P30 P25 RX/TX gate anchor missing'}

$txBlock=@'
            if (!_power) return;

            if (mox)
            {
                // Native PowerSDR DttSP TX telemetry (main TX DSP = thread 1).
                if (_readings[1].RequiresUpdate(Reading.MIC))
                    _readings[1].SetReading(Reading.MIC, _console.P30ReadTxDsp(DttSP.MeterType.MIC));
                if (_readings[1].RequiresUpdate(Reading.MIC_PK))
                    _readings[1].SetReading(Reading.MIC_PK, _console.P30ReadTxDsp(DttSP.MeterType.MIC_PK));

                if (_readings[1].RequiresUpdate(Reading.EQ))
                    _readings[1].SetReading(Reading.EQ, _console.P30ReadTxDsp(DttSP.MeterType.EQ));
                if (_readings[1].RequiresUpdate(Reading.EQ_PK))
                    _readings[1].SetReading(Reading.EQ_PK, _console.P30ReadTxDsp(DttSP.MeterType.EQ_PK));

                if (_readings[1].RequiresUpdate(Reading.LEVELER))
                    _readings[1].SetReading(Reading.LEVELER, _console.P30ReadTxDsp(DttSP.MeterType.LEVELER));
                if (_readings[1].RequiresUpdate(Reading.LEVELER_PK))
                    _readings[1].SetReading(Reading.LEVELER_PK, _console.P30ReadTxDsp(DttSP.MeterType.LEVELER_PK));
                if (_readings[1].RequiresUpdate(Reading.LVL_G))
                    _readings[1].SetReading(Reading.LVL_G, _console.P30ReadTxDsp(DttSP.MeterType.LVL_G));

                if (_readings[1].RequiresUpdate(Reading.ALC))
                    _readings[1].SetReading(Reading.ALC, _console.P30ReadTxDsp(DttSP.MeterType.ALC));
                if (_readings[1].RequiresUpdate(Reading.ALC_PK))
                    _readings[1].SetReading(Reading.ALC_PK, _console.P30ReadTxDsp(DttSP.MeterType.ALC_PK));
                if (_readings[1].RequiresUpdate(Reading.ALC_G))
                    _readings[1].SetReading(Reading.ALC_G, _console.P30ReadTxDsp(DttSP.MeterType.ALC_G));

                if (_readings[1].RequiresUpdate(Reading.COMP))
                    _readings[1].SetReading(Reading.COMP, _console.P30ReadTxDsp(DttSP.MeterType.COMP));
                if (_readings[1].RequiresUpdate(Reading.COMP_PK))
                    _readings[1].SetReading(Reading.COMP_PK, _console.P30ReadTxDsp(DttSP.MeterType.COMP_PK));

                // Native FLEX-5000 PA ADC/calibration path.
                if (_readings[1].RequiresUpdate(Reading.PWR))
                    _readings[1].SetReading(Reading.PWR, _console.P30ReadTxForwardWatts());
                if (_readings[1].RequiresUpdate(Reading.REVERSE_PWR))
                    _readings[1].SetReading(Reading.REVERSE_PWR, _console.P30ReadTxReverseWatts());
                if (_readings[1].RequiresUpdate(Reading.SWR))
                    _readings[1].SetReading(Reading.SWR, _console.P30ReadTxSWR());

                if (_readings[1].RequiresUpdate(Reading.PA_FWD_PWR))
                    _readings[1].SetReading(Reading.PA_FWD_PWR, _console.P30ReadTxForwardWatts());
                if (_readings[1].RequiresUpdate(Reading.PA_REV_PWR))
                    _readings[1].SetReading(Reading.PA_REV_PWR, _console.P30ReadTxReverseWatts());
                if (_readings[1].RequiresUpdate(Reading.CAL_FWD_PWR))
                    _readings[1].SetReading(Reading.CAL_FWD_PWR, _console.P30ReadTxForwardWatts());

                if (_readings[1].RequiresUpdate(Reading.VOLTS))
                    _readings[1].SetReading(Reading.VOLTS, _console.P30ReadPaVolts());

                // CFC/ALC_GROUP/AMPS are not synthesized: legacy PowerSDR/DttSP
                // has no native equivalent for those Thetis-specific sources.
                return;
            }
'@
$mm=$mm.Replace($oldGate,$txBlock)
[IO.File]::WriteAllText($meterManagerPath,$mm,$utf8)

# Configure 100 W FLEX-5000 scaling after MeterManager.Init but before restore.
$bridge=[IO.File]::ReadAllText($bridgePath)
$initAnchor='MeterManager.Init(this, Path.Combine(Application.StartupPath, "MeterSkins"));'
if(!$bridge.Contains($initAnchor)){throw 'P30 MeterManager.Init bridge anchor missing'}
if(!$bridge.Contains('MeterManager.P30ConfigureFlex5000();'))
{
    $bridge=$bridge.Replace($initAnchor,$initAnchor+$nl+'                MeterManager.P30ConfigureFlex5000();')
}
[IO.File]::WriteAllText($bridgePath,$bridge,$utf8)

# Hard regression gates.
$verifyMM=[IO.File]::ReadAllText($meterManagerPath)
$verifyBridge=[IO.File]::ReadAllText($bridgePath)
$verifyTx=[IO.File]::ReadAllText($dstTxBridge)

foreach($token in @(
    'P25ReadRx1SignalDbm',
    'P25ReadRx1AverageSignalDbm',
    'P25ReadRx1AgcGain'
)){
    if(!$verifyMM.Contains($token)){throw "P30 RX1 regression gate missing: $token"}
}
foreach($token in @(
    'P30ReadTxDsp(DttSP.MeterType.MIC)',
    'P30ReadTxDsp(DttSP.MeterType.ALC)',
    'P30ReadTxDsp(DttSP.MeterType.COMP)',
    'P30ReadTxForwardWatts',
    'P30ReadTxReverseWatts',
    'P30ReadTxSWR',
    'P30ReadPaVolts'
)){
    if(!$verifyMM.Contains($token)){throw "P30 TX MeterManager gate missing: $token"}
}
foreach($token in @(
    'DttSP.CalculateTXMeter(1, meter)',
    'FWC.ReadPAADC(5, out fwd)',
    'FWC.ReadPAADC(4, out rev)',
    'FWC.ReadPAADC(2, out volts)'
)){
    if(!$verifyTx.Contains($token)){throw "P30 TX adapter gate missing: $token"}
}
if($verifyMM.Contains('if (!_power || mox) return;'))
{
    throw 'P30 obsolete MOX-blocking gate still present'
}
if(!$verifyBridge.Contains('MeterManager.P30ConfigureFlex5000();'))
{
    throw 'P30 FLEX5000 100W scale configuration gate missing'
}

Write-Host 'P30_RX1=UNCHANGED_FROM_P29'
Write-Host 'P30_TX_DSP=POWERSDR_DTTSP_THREAD1'
Write-Host 'P30_TX_RF=FLEX5000_FWC_PA_ADC'
Write-Host 'P30_TX_SUPPORTED=MIC,MIC_PK,EQ,EQ_PK,LEVELER,LEVELER_PK,LVL_G,ALC,ALC_PK,ALC_G,COMP,COMP_PK,PWR,REVERSE_PWR,SWR,VOLTS'
Write-Host 'P30_TX_UNAVAILABLE_NATIVE=CFC,CFC_GAIN,ALC_GROUP,AMPS'
Write-Host 'P30_POWER_SCALE=FLEX5000_100W'
Write-Host 'P30_DARK_SKINS=OFFICIAL_DEFAULT_METERS_ALL_IMAGES'`nWrite-Host 'P30_DARK_MISSING_VARIANT_FALLBACK=NORMAL_IMAGE_ONLY_IF_OFFICIAL_DARK_ABSENT'
Write-Host "P30_THETIS_SKINS_SHA=$ThetisSkinsSha"
