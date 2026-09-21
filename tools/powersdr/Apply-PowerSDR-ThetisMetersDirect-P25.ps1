[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$ThetisSha='8220ec089451380054e9c31313d4ac2d4bf11776'
$RawBase="https://raw.githubusercontent.com/ramdor/Thetis/$ThetisSha/Project%20Files/Source/Console"
$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$pkgPath=Join-Path $consoleDir 'packages.config'
$commonPath=Join-Path $consoleDir 'common.cs'
$consolePath=Join-Path $consoleDir 'console.cs'
$utf8=New-Object Text.UTF8Encoding($true)
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

$files=@(
  'MeterManager.cs',
  'ucMeter.cs',
  'ucMeter.Designer.cs',
  'frmMeterDisplay.cs',
  'frmMeterDisplay.Designer.cs'
)

foreach($name in $files)
{
    $url="$RawBase/$name"
    $tmp=Join-Path $env:TEMP ("P25_"+$name)
    Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $tmp
    $c=[IO.File]::ReadAllText($tmp)
    $c=$c.Replace('namespace Thetis','namespace PowerSDR')
    $c=$c.Replace('global::Thetis.Properties.Resources.','global::PowerSDR.P25MeterResources.')
    $c=$c.Replace('Properties.Resources.','P25MeterResources.')
    [IO.File]::WriteAllText((Join-Path $consoleDir ("P25_"+$name)),$c,$utf8)
}

# Keep the native Thetis meter model/container/SharpDX renderer.
# Replace only the later Thetis event bus with a small PowerSDR telemetry boundary.
$mmPath=Join-Path $consoleDir 'P25_MeterManager.cs'
$mm=[IO.File]::ReadAllText($mmPath)

# The 2023 Thetis renderer expects ElapsedMsec. PowerSDR's older HiPerfTimer
# does not expose that member, so use Stopwatch for the same elapsed-time role.
$mm=$mm.Replace(
    'private HiPerfTimer _objFrameStartTimer = new HiPerfTimer();',
    'private System.Diagnostics.Stopwatch _objFrameStartTimer = System.Diagnostics.Stopwatch.StartNew();'
)
$mm=$mm.Replace('_objFrameStartTimer.ElapsedMsec','_objFrameStartTimer.Elapsed.TotalMilliseconds')

$eventRx='(?s)        private static void addDelegates\(\).*?        private static void OnTransverterIndexChanged'
$eventReplacement=@'
        private static void addDelegates()
        {
            // P25: native PowerSDR state is polled by the existing Thetis meter thread.
            _delegatesAdded = true;
        }
        private static void removeDelegates()
        {
            if (_lstUCMeters != null)
            {
                foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
                    kvp.Value.RemoveDelegates();
            }
            _delegatesAdded = false;
        }
        private static void OnTransverterIndexChanged
'@
$mm2=[regex]::Replace($mm,$eventRx,$eventReplacement,1)
if($mm2 -eq $mm){throw 'P25 failed to replace Thetis MeterManager event-bus methods'}
$mm=$mm2

$addImageAnchor='        public static void AddImage(string sKey, System.Drawing.Bitmap image)'
if(!$mm.Contains($addImageAnchor)){throw 'P25 MeterManager AddImage anchor missing'}
$telemetryBridge=@'
        private static bool _p25LastMox;
        private static bool _p25MoxValid;

        private static void P25RefreshPowerSDR()
        {
            if (_console == null) return;

            _power = _console.PowerOn;
            _rx1VHForAbove = _console.VFOAFreq >= 30.0;
            _rx2VHForAbove = false;

            bool mox = _console.MOX;
            if (!_p25MoxValid || mox != _p25LastMox)
            {
                bool oldMox = _p25MoxValid ? _p25LastMox : mox;
                _p25LastMox = mox;
                _p25MoxValid = true;
                OnMox(1, oldMox, mox);
                _console.P25RaiseMeterMox(1, oldMox, mox);
            }

            if (!_power || mox) return;

            if (_readings[1].RequiresUpdate(Reading.SIGNAL_STRENGTH))
                _readings[1].SetReading(Reading.SIGNAL_STRENGTH, _console.P25ReadRx1SignalDbm());

            if (_readings[1].RequiresUpdate(Reading.AVG_SIGNAL_STRENGTH))
                _readings[1].SetReading(Reading.AVG_SIGNAL_STRENGTH, _console.P25ReadRx1AverageSignalDbm());

            if (_readings[1].RequiresUpdate(Reading.AGC_GAIN))
                _readings[1].SetReading(Reading.AGC_GAIN, _console.P25ReadRx1AgcGain());
        }

'@
$mm=$mm.Replace($addImageAnchor,$telemetryBridge+$addImageAnchor)

$loopRx='(?m)^(\s*)while \(_meterThreadRunning\)\s*\{'
$loopMatch=[regex]::Match($mm,$loopRx)
if(!$loopMatch.Success){throw 'P25 MeterManager loop anchor missing'}
$loopInsert=$nl+$loopMatch.Groups[1].Value+'    P25RefreshPowerSDR();'
$mm=$mm.Insert($loopMatch.Index+$loopMatch.Length,$loopInsert)
[IO.File]::WriteAllText($mmPath,$mm,$utf8)

# PowerSDR ButtonTS predates Thetis' Selectable property.
$ucDesignerPath=Join-Path $consoleDir 'P25_ucMeter.Designer.cs'
$ucDesigner=[IO.File]::ReadAllText($ucDesignerPath)
$ucDesigner=[regex]::Replace(
    $ucDesigner,
    '(?m)^\s*this\.[A-Za-z0-9_]+\.Selectable\s*=\s*false;\s*$',
    ''
)
[IO.File]::WriteAllText($ucDesignerPath,$ucDesigner,$utf8)

# Exact small toolbar graphics used by this pinned Thetis ucMeter.
$resourceNames=@(
 'dockIcon_dock','dockIcon_float','dot','arrow_left','arrow_topleft','arrow_up',
 'arrow_topright','arrow_right','arrow_bottomright','down','arrow_bottomleft',
 'pin_on_top','pin_not_on_top','resizegrab'
)
$resDir=Join-Path $consoleDir 'Resources'
New-Item -ItemType Directory -Force -Path $resDir | Out-Null
foreach($r in $resourceNames)
{
    $url="$RawBase/Resources/$r.png"
    Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile (Join-Path $resDir ($r+'.png'))
}

Copy-Item (Join-Path $PSScriptRoot 'P25ThetisMetersBridge.cs') (Join-Path $consoleDir 'P25ThetisMetersBridge.cs') -Force

# Existing PowerSDR form persistence is reused by frmMeterDisplay.
$common=[IO.File]::ReadAllText($commonPath)
if($common -notmatch 'public partial class Common')
{
    $common=$common.Replace('public class Common','public partial class Common')
    [IO.File]::WriteAllText($commonPath,$common,$utf8)
}

# Start meters only after the main form exists. Save/stop them before native PowerSDR close.
$con=[IO.File]::ReadAllText($consolePath)
if($con -notmatch 'this\.Shown \+= new EventHandler\(P25ThetisMetersShown\)')
{
    $init='            InitializeComponent();                              // Windows Forms Generated Code'
    if(!$con.Contains($init)){throw 'P25 console InitializeComponent anchor missing'}
    $con=$con.Replace($init,$init+$nl+'            this.Shown += new EventHandler(P25ThetisMetersShown);')
}
if($con -notmatch 'P25ShutdownThetisMeters\(\);\s*// P25')
{
    $close='        public void Console_Closing(object sender, FormClosingEventArgs e)'+$nl+'        {'
    if(!$con.Contains($close)){throw 'P25 Console_Closing anchor missing'}
    $con=$con.Replace(
        $close,
        $close+$nl+'            P25ShutdownThetisMeters(); // P25: save/stop native Thetis meters before PowerSDR shutdown'
    )
}
[IO.File]::WriteAllText($consolePath,$con,$utf8)

# Pinned SharpDX version matching the direct renderer.
$pkg=[IO.File]::ReadAllText($pkgPath)
$packages=@(
 @('SharpDX','4.2.0'),
 @('SharpDX.Desktop','4.2.0'),
 @('SharpDX.Direct2D1','4.2.0'),
 @('SharpDX.Direct3D11','4.2.0'),
 @('SharpDX.DXGI','4.2.0'),
 @('SharpDX.Mathematics','4.2.0')
)
foreach($x in $packages)
{
    if($pkg -notmatch ('id="'+[regex]::Escape($x[0])+'"'))
    {
        $line='  <package id="'+$x[0]+'" version="'+$x[1]+'" targetFramework="net48" />'
        $pkg=$pkg.Replace('</packages>',$line+$nl+'</packages>')
    }
}
[IO.File]::WriteAllText($pkgPath,$pkg,$utf8NoBom)

$proj=[IO.File]::ReadAllText($projPath)
$refs=@'
    <Reference Include="SharpDX, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.4.2.0\lib\net45\SharpDX.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Desktop, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Desktop.4.2.0\lib\net45\SharpDX.Desktop.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Direct2D1, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Direct2D1.4.2.0\lib\net45\SharpDX.Direct2D1.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Direct3D11, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Direct3D11.4.2.0\lib\net45\SharpDX.Direct3D11.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.DXGI, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.DXGI.4.2.0\lib\net45\SharpDX.DXGI.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Mathematics, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Mathematics.4.2.0\lib\net45\SharpDX.Mathematics.dll</HintPath>
    </Reference>
'@
if($proj -notmatch '<Reference Include="SharpDX,')
{
    $anchorRef='<Reference Include="System.Drawing">'
    $idx=$proj.IndexOf($anchorRef)
    if($idx -lt 0){throw 'P25 csproj reference anchor missing'}
    $proj=$proj.Insert($idx,$refs+$nl+'    ')
}

$compile=@(
 'P25_MeterManager.cs',
 'P25_ucMeter.cs',
 'P25_ucMeter.Designer.cs',
 'P25_frmMeterDisplay.cs',
 'P25_frmMeterDisplay.Designer.cs',
 'P25ThetisMetersBridge.cs'
)
$anchorCompile='<Compile Include="Skin.cs" />'
if(!$proj.Contains($anchorCompile)){throw 'P25 csproj compile anchor missing'}
foreach($name in $compile)
{
    if($proj -notmatch ('Compile Include="'+[regex]::Escape($name)+'"'))
    {
        $proj=$proj.Replace($anchorCompile,$anchorCompile+$nl+'    <Compile Include="'+$name+'" />')
    }
}

# Put toolbar PNG files next to the executable under Resources\.
foreach($r in $resourceNames)
{
    $rel='Resources\'+$r+'.png'
    if($proj -notmatch ('Content Include="'+[regex]::Escape($rel)+'"'))
    {
        $item='    <Content Include="'+$rel+'">'+$nl+
              '      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>'+$nl+
              '    </Content>'+$nl
        $igPos=$proj.IndexOf('</ItemGroup>')
        if($igPos -lt 0){throw 'P25 csproj ItemGroup close missing'}
        $proj=$proj.Insert($igPos,$item)
    }
}

[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

Write-Host "P25_THETIS_SOURCE_SHA=$ThetisSha"
Write-Host 'P25_THETIS_CORE=DIRECT_2023_02_26'
Write-Host 'P25_RX2=NOT_EXPOSED'
Write-Host 'P25_RENDERER=THETIS_SHARPDX'
Write-Host 'P25_TELEMETRY=POWERSDR_FLEX5000_RX1_NATIVE_CALIBRATION'
