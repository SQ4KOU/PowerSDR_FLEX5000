[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P05] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $consoleCs)) { throw "PowerSDR console source missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$text = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P05 anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P05 anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

# 0) Persist direct FWC standby timing across chkPower -> Console_Closing.
$oldField = @'
        private bool one_time = true;
        private void chkPower_CheckedChanged(object sender, System.EventArgs e)
'@

$newField = @'
        private bool one_time = true;
        private long sq4kouLastFwcStandbyMs = -1;
        private void chkPower_CheckedChanged(object sender, System.EventArgs e)
'@

$text = Replace-ExactOnce $text $oldField $newField 'P05 standby timing field'

# 1) Start terminal audio teardown before FWC standby only on application close.
$oldStandby = @'
                if (!(fwc_init && (current_model == Model.FLEX5000 || current_model == Model.FLEX3000)))
                {
                    //  if(current_model == Model.SDR1000)     Hdw.StandBy();
                }
                else
                {
                    FWC.SetStandby(true);
                }
'@

$newStandby = @'
                bool sq4kouTerminalAudioStopped = false;

                // SQ4KOU P05: on application close only, stop FireWire/ASIO first.
                // The native KE9NS power-off order sends FWC standby before closing
                // the audio stream. On some FLEX-5000 driver stacks that synchronous
                // PAL call can wait for the still-active FireWire stream.
                if (Audio.FastApplicationShutdown &&
                    (current_model == Model.FLEX5000 || current_model == Model.FLEX3000))
                {
                    Audio.callback_return = 2;
                    Audio.StopAudio();

                    if (vac_enabled) Audio.StopAudioVAC();
                    if (vac2_enabled) Audio.StopAudioVAC2();

                    sq4kouTerminalAudioStopped = true;
                }

                Stopwatch sq4kouFwcStandbyTimer = Stopwatch.StartNew();

                if (!(fwc_init && (current_model == Model.FLEX5000 || current_model == Model.FLEX3000)))
                {
                    //  if(current_model == Model.SDR1000)     Hdw.StandBy();
                }
                else
                {
                    FWC.SetStandby(true);
                }

                sq4kouLastFwcStandbyMs = sq4kouFwcStandbyTimer.ElapsedMilliseconds;
'@

$text = Replace-ExactOnce $text $oldStandby $newStandby 'audio before FWC standby'

# 2) Do not stop the same streams twice in the later native section.
$oldStop = @'
                    case Model.FLEX5000:
                    case Model.FLEX3000:
                    //  case Model.SOFTROCK40:
                    case Model.DEMO:
                        Audio.callback_return = 2;
                        Audio.StopAudio();
                        break;
'@

$newStop = @'
                    case Model.FLEX5000:
                    case Model.FLEX3000:
                    //  case Model.SOFTROCK40:
                    case Model.DEMO:
                        if (!sq4kouTerminalAudioStopped)
                        {
                            Audio.callback_return = 2;
                            Audio.StopAudio();
                        }
                        break;
'@

$text = Replace-ExactOnce $text $oldStop $newStop 'skip duplicate primary audio stop'

$oldVac1 = @'
                if (vac_enabled)
                {
                    Debug.WriteLine("test8===============");

                    Audio.StopAudioVAC();
                }
'@
$newVac1 = @'
                if (vac_enabled && !sq4kouTerminalAudioStopped)
                {
                    Debug.WriteLine("test8===============");

                    Audio.StopAudioVAC();
                }
'@
$text = Replace-ExactOnce $text $oldVac1 $newVac1 'skip duplicate VAC stop'

$oldVac2 = @'
                if (vac2_enabled)
                {
                    Audio.StopAudioVAC2();
                }
'@
$newVac2 = @'
                if (vac2_enabled && !sq4kouTerminalAudioStopped)
                {
                    Audio.StopAudioVAC2();
                }
'@
$text = Replace-ExactOnce $text $oldVac2 $newVac2 'skip duplicate VAC2 stop'

# 3) Persist timing from the initial application Power OFF phase into shutdown1.log.
$oldTitle = '            writer.WriteLine("This is a PowerSDR powering downlog: 1-8");'
$newTitle = $oldTitle + "`n" + '            writer.WriteLine("SQ4KOU_POWER_OFF_PHASE_MS=" + sq4kouShutdown1Timer.ElapsedMilliseconds.ToString());'
$text = Replace-ExactOnce $text $oldTitle $newTitle 'initial power off timing'

# 4) Persist direct FWC standby timing once the writer exists.
$oldStep1Done = @'
            writer.WriteLine("1) Done");
            writer.WriteLine("2) Hide all forms ");
'@
$newStep1Done = @'
            writer.WriteLine("1) Done");
            writer.WriteLine("SQ4KOU_FWC_STANDBY_MS=" + sq4kouLastFwcStandbyMs.ToString());
            writer.WriteLine("2) Hide all forms ");
'@
$text = Replace-ExactOnce $text $oldStep1Done $newStep1Done 'FWC standby timing log'

$checks = @(
    @{ Name='terminal audio before standby'; Ok=$text.Contains('SQ4KOU P05: on application close only, stop FireWire/ASIO first.') },
    @{ Name='standby timing field'; Ok=$text.Contains('private long sq4kouLastFwcStandbyMs = -1;') },
    @{ Name='direct standby timer'; Ok=$text.Contains('Stopwatch sq4kouFwcStandbyTimer = Stopwatch.StartNew();') },
    @{ Name='power off phase timing'; Ok=$text.Contains('SQ4KOU_POWER_OFF_PHASE_MS=') },
    @{ Name='standby timing'; Ok=$text.Contains('SQ4KOU_FWC_STANDBY_MS=') },
    @{ Name='normal Stop/Start preserved'; Ok=$text.Contains('if (Audio.FastApplicationShutdown &&') }
)
$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P05 post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs, $text.Replace("`n", "`r`n"), $utf8)
Stage 'PASS: terminal close stops audio before synchronous FWC standby and logs both timings'
