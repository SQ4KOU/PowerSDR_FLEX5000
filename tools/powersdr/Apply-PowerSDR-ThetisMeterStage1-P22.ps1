param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$consoleDir=Join-Path $SourceRoot 'Console'
$consoleCs=Join-Path $consoleDir 'console.cs'
$csproj=Join-Path $consoleDir 'PowerSDR.csproj'
$template=Join-Path $PSScriptRoot 'native-meter-stage1\ThetisMeterStage1.cs'
$target=Join-Path $consoleDir 'ThetisMeterStage1.cs'

foreach($p in @($consoleCs,$csproj,$template)){
    if(!(Test-Path -LiteralPath $p)){throw "P22 required file missing: $p"}
}

Copy-Item -LiteralPath $template -Destination $target -Force

# One explicit lifecycle hook only. It is registered after native WinForms
# InitializeComponent but executes on Shown, after PowerSDR DB/hardware startup.
$c=[IO.File]::ReadAllText($consoleCs)
$needle='InitializeComponent();                              // Windows Forms Generated Code'
if(([regex]::Matches($c,[regex]::Escape($needle))).Count -ne 1){
    throw 'P22 console InitializeComponent anchor mismatch'
}
$replacement=@"
$needle
            this.Shown += new EventHandler(P22ThetisMeterShown);
            this.FormClosing += new FormClosingEventHandler(P22ThetisMeterConsoleClosing); // P22 physical-test persistence/log flush
"@
$c=$c.Replace($needle,$replacement)
[IO.File]::WriteAllText($consoleCs,$c,(New-Object Text.UTF8Encoding($false)))

# Compile the single Stage-1 source directly into PowerSDR.exe. No helper DLL.
$p=[IO.File]::ReadAllText($csproj)
$anchor=@"
    <Compile Include="console.cs">
      <SubType>Form</SubType>
    </Compile>
"@
if(([regex]::Matches($p,[regex]::Escape($anchor))).Count -ne 1){
    throw 'P22 PowerSDR.csproj console.cs anchor mismatch'
}
$insert=$anchor + "    <Compile Include=`"ThetisMeterStage1.cs`" />`r`n"
$p=$p.Replace($anchor,$insert)
[IO.File]::WriteAllText($csproj,$p,(New-Object Text.UTF8Encoding($false)))

Write-Host 'P22_APPLY=PASS'
Write-Host 'P22_CORE=THETIS_FRMMETERDISPLAY_BORDERLESS_PERSISTENCE_DPI'
Write-Host 'P22_TELEMETRY=FLEX5000_NATIVE_RX1_SIGNAL_CAL_PATH'
Write-Host 'P22_RX2=ABSENT'
Write-Host 'P22_HELPER_DLL=ABSENT'
