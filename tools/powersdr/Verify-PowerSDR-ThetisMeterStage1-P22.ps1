param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

$consoleDir=Join-Path $SourceRoot 'Console'
$consoleCs=Join-Path $consoleDir 'console.cs'
$csproj=Join-Path $consoleDir 'PowerSDR.csproj'
$stage=Join-Path $consoleDir 'ThetisMeterStage1.cs'

foreach($p in @($consoleCs,$csproj,$stage)){
    if(!(Test-Path -LiteralPath $p)){throw "P22 verify missing: $p"}
}

$c=[IO.File]::ReadAllText($consoleCs)
$p=[IO.File]::ReadAllText($csproj)
$s=[IO.File]::ReadAllText($stage)

$requiredConsole=@(
 'this.Shown += new EventHandler(P22ThetisMeterShown);',
 'this.FormClosing += new FormClosingEventHandler(P22ThetisMeterConsoleClosing);'
)
foreach($token in $requiredConsole){
    if(!$c.Contains($token)){throw "P22 console hook missing: $token"}
}

if(([regex]::Matches($p,'Compile Include="ThetisMeterStage1\.cs"')).Count -ne 1){
    throw 'P22 compile item must occur exactly once'
}

$requiredStage=@(
 'FormBorderStyle = FormBorderStyle.None',
 'Common.RestoreForm(this, PersistenceTable, true)',
 'Common.SaveForm(this, PersistenceTable)',
 'SetWindowPos(handle, IntPtr.Zero, originalX + 1',
 'DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.SIGNAL_STRENGTH)',
 'rx1_meter_cal_offset',
 'rx1_preamp_offset[(int)rx1_preamp_mode]',
 'rx1_filter_size_cal_offset',
 'rx1_path_offset',
 'rx1_xvtr_gain_offset',
 'rx1_loop_offset',
 'P22_ThetisMeter_RX1_PHYSICAL.log',
 'RX1 Meter [P22]'
)
foreach($token in $requiredStage){
    if(!$s.Contains($token)){throw "P22 source gate missing: $token"}
}

# Architecture gate: rejected helper-runtime branch must not leak into clean port.
foreach($token in @('FlexMeters.dll','MeterLiveRuntime','MeterWorkspaceManager','DataTableMeterStore')){
    if($s.Contains($token) -or $p.Contains($token)){throw "P22 rejected architecture leaked: $token"}
}

Write-Host 'P22_VERIFY=PASS'
Write-Host 'P22_PHYSICAL_VALIDATION=PENDING'
# P22 workflow trigger: physical validation remains pending.
