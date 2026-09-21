[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$utf8 = New-Object Text.UTF8Encoding($true)
$nl = [Environment]::NewLine
$consoleDir = Join-Path $SourceRoot 'Console'
$projPath = Join-Path $consoleDir 'PowerSDR.csproj'
$commonPath = Join-Path $consoleDir 'common.cs'

$thetisCommit = '0df9e2364e043d001b9fe2e9cff4cb9f63fe405e'
$files = @(
    @{ Name='MeterManager.cs'; Blob='35157e27b290cdd5a7530aca51864fb7d2c506c7' },
    @{ Name='ucMeter.cs'; Blob='5ba79fef38dac49d30989bc967bf5483abba83f9' },
    @{ Name='ucMeter.Designer.cs'; Blob='2a9f4d1c25584ee9612c2198549d16a14c5906ad' },
    @{ Name='frmMeterDisplay.cs'; Blob='8722c486c7691403d9b1b2328c4c2009a6291a32' },
    @{ Name='frmMeterDisplay.Designer.cs'; Blob='83ef54f61a5c33c4d217a35d4d058fe15ea2734d' }
)

function Replace-Between([string]$Text,[string]$Start,[string]$End,[string]$Replacement) {
    $i = $Text.IndexOf($Start)
    if($i -lt 0){ throw "P25 start marker missing: $Start" }
    $j = $Text.IndexOf($End,$i+$Start.Length)
    if($j -lt 0){ throw "P25 end marker missing: $End" }
    return $Text.Substring(0,$i) + $Replacement + $Text.Substring($j)
}

foreach($f in $files){
    $name=$f.Name
    $raw='https://raw.githubusercontent.com/ramdor/Thetis/'+$thetisCommit+'/Project%20Files/Source/Console/'+$name
    $tmp=Join-Path $env:TEMP ('p25_'+$name)
    Invoke-WebRequest -Uri $raw -OutFile $tmp -UseBasicParsing

    $blob=(& git hash-object $tmp).Trim()
    if($blob -ne $f.Blob){ throw "P25 Thetis source hash mismatch for $name : $blob != $($f.Blob)" }

    $c=[IO.File]::ReadAllText($tmp)
    $c=$c.Replace('namespace Thetis','namespace PowerSDR')
    $c=$c.Replace('global::Thetis.','global::PowerSDR.')
    $c=$c.Replace('Thetis.ColorButton','PowerSDR.ColorButton')

    $c=[regex]::Replace($c,'global::PowerSDR\.Properties\.Resources\.[A-Za-z0-9_]+','null')
    $c=[regex]::Replace($c,'Properties\.Resources\.[A-Za-z0-9_]+','null')

    if($name -eq 'MeterManager.cs'){
        $c=$c.Replace('_currentHPSDRmodel = _console.CurrentHPSDRModel;','_currentHPSDRmodel = HPSDRModel.UNKNOWN;')
        $c=$c.Replace('_apolloPresent = _console.ApolloPresent;','_apolloPresent = false;')
        $c=$c.Replace('_alexPresent = _console.AlexPresent;','_alexPresent = false;')

        $add = @'
        private static void addDelegates()
        {
            // PowerSDR host synchronization is performed at the single P25 data boundary.
            _delegatesAdded = true;
        }
'@
        $c=Replace-Between $c '        private static void addDelegates()' '        private static void removeDelegates()' $add

        $rem = @'
        private static void removeDelegates()
        {
            foreach (KeyValuePair<string, ucMeter> kvp in _lstUCMeters)
                kvp.Value.RemoveDelegates();
            _delegatesAdded = false;
        }
'@
        $c=Replace-Between $c '        private static void removeDelegates()' '        private static void OnSplitChanged' $rem

        $init = @'
        private static void initConsoleData(int rx)
        {
            if (_console == null || rx != 1) return;

            lock (_metersLock)
            {
                foreach (KeyValuePair<string, clsMeter> mkvp in _meters.Where(o => o.Value.RX == 1))
                {
                    clsMeter m = mkvp.Value;

                    m.MOX = _console.MOX;
                    m.Split = _console.VFOSplit;
                    m.TXVFOb = _console.VFOBTX;
                    m.RX2Enabled = false;
                    m.MultiRxEnabled = false;

                    m.VfoA = _console.VFOAFreq;
                    m.ModeVfoA = _console.RX1DSPMode;
                    m.BandVfoA = _console.RX1Band;
                    m.FilterVfoA = _console.RX1Filter;
                    m.FilterVfoAName = getFilterName(1);

                    m.VfoB = _console.VFOBFreq;
                    m.VfoSub = _console.VFOASubFreq;
                    m.ModeVfoB = _console.RX1DSPMode;
                    m.BandVfoB = _console.RX1Band;
                    m.FilterVfoB = _console.RX1Filter;
                    m.FilterVfoBName = getFilterName(1);

                    m.TXEQEnabled = false;
                    m.LevelerEnabled = false;
                    m.CFCEnabled = false;
                    m.CompandEnabled = false;
                    m.QuickSplitEnabled = false;
                }
            }
        }
'@
        $c=Replace-Between $c '        private static void initConsoleData(int rx)' '        private static string getFilterName' $init

        $filter = @'
        private static string getFilterName(int rx)
        {
            try
            {
                if (rx != 1) return "";
                if (_console.RX1DSPMode == DSPMode.FIRST || _console.RX1DSPMode == DSPMode.LAST ||
                    _console.RX1Filter == Filter.FIRST || _console.RX1Filter == Filter.LAST) return "";
                return _console.rx1_filters[(int)_console.RX1DSPMode].GetName(_console.RX1Filter);
            }
            catch { return ""; }
        }
'@
        $c=Replace-Between $c '        private static string getFilterName(int rx)' '        private static void OnPower' $filter

        $refresh = @'
        private static void P25RefreshPowerSDRHost()
        {
            if (_console == null) return;

            initConsoleData(1);

            Dictionary<Reading, float> readings = _console.P25GetMeterReadings();
            OnMeterReadings(1, _console.MOX, ref readings);
        }

'@
        $marker='        private static void UpdateMeters()'
        $idx=$c.IndexOf($marker)
        if($idx -lt 0){throw 'P25 UpdateMeters marker missing'}
        $c=$c.Insert($idx,$refresh)

        $old='            while (_meterThreadRunning)' + $nl + '            {'
        if(!$c.Contains($old)){throw 'P25 meter worker loop marker missing'}
        $new=$old+$nl+'                P25RefreshPowerSDRHost();'
        $c=$c.Replace($old,$new)
    }

    if($name -eq 'ucMeter.cs'){
        $addUc = @'
        private void addDelegates()
        {
            // MOX/title state is synchronized by MeterManager from PowerSDR.
        }
        public void RemoveDelegates()
        {
        }
'@
        $c=Replace-Between $c '        private void addDelegates()' '        private void OnMoxChangeHandler' $addUc
    }

    $dst=Join-Path $consoleDir ('P25_'+$name)
    [IO.File]::WriteAllText($dst,$c,$utf8)
}

Copy-Item (Join-Path $PSScriptRoot 'P25ThetisMetersDirectHost.cs') (Join-Path $consoleDir 'P25ThetisMetersDirectHost.cs') -Force

$common=[IO.File]::ReadAllText($commonPath)
if($common -notmatch 'public partial class Common'){
    $common=$common.Replace('public class Common','public partial class Common')
    [IO.File]::WriteAllText($commonPath,$common,$utf8)
}

$consolePath=Join-Path $consoleDir 'console.cs'
$cc=[IO.File]::ReadAllText($consolePath)
$hook='            InitializeComponent();'
if(!$cc.Contains($hook)){throw 'P25 console InitializeComponent marker missing'}
if(!$cc.Contains('P25ThetisMetersShown')){
    $cc=$cc.Replace($hook,$hook+$nl+'            this.Shown += new EventHandler(P25ThetisMetersShown);'+$nl+'            this.FormClosing += new FormClosingEventHandler(P25ThetisMetersConsoleClosing);')
}
[IO.File]::WriteAllText($consolePath,$cc,$utf8)

$proj=[IO.File]::ReadAllText($projPath)
$compile=@(
 'P25_MeterManager.cs',
 'P25_ucMeter.cs',
 'P25_ucMeter.Designer.cs',
 'P25_frmMeterDisplay.cs',
 'P25_frmMeterDisplay.Designer.cs',
 'P25ThetisMetersDirectHost.cs'
)
$anchor='<Compile Include="Skin.cs" />'
if(!$proj.Contains($anchor)){throw 'P25 csproj compile anchor missing'}
foreach($name in $compile){
    if($proj -notmatch ('Compile Include="'+[regex]::Escape($name)+'"')){
        $proj=$proj.Replace($anchor,$anchor+$nl+'    <Compile Include="'+$name+'" />')
    }
}
[IO.File]::WriteAllText($projPath,$proj,$utf8)

Write-Host 'P25_DIRECT_THETIS_PORT=APPLIED'
Write-Host ('P25_THETIS_COMMIT='+$thetisCommit)
Write-Host 'P25_RX2=DISABLED'
Write-Host 'P25_STORE=POWERSDR_DB'
Write-Host 'P25_RENDERER=THETIS_SHARPDX_DIRECT2D'
