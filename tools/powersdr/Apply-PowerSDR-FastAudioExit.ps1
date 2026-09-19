[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-ASIO-EXIT] $s" }

$audioCs = Join-Path $SourceRoot 'Console\audio.cs'
$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $audioCs)) { throw "PowerSDR audio source missing: $audioCs" }
if(!(Test-Path -LiteralPath $consoleCs)) { throw "PowerSDR console source missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$audio = [IO.File]::ReadAllText($audioCs).Replace("`r`n", "`n")
$console = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P04 anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P04 anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

function Replace-CSharpMethod([string]$Text, [string]$Signature, [string]$Replacement, [string]$Label) {
    $Replacement = $Replacement.Replace("`r`n", "`n")
    $start = $Text.IndexOf($Signature, [StringComparison]::Ordinal)
    if($start -lt 0) { throw "P04 method signature missing: $Label" }
    if($Text.IndexOf($Signature, $start + $Signature.Length, [StringComparison]::Ordinal) -ge 0) {
        throw "P04 method signature not unique: $Label"
    }

    $brace = $Text.IndexOf('{', $start)
    if($brace -lt 0) { throw "P04 method opening brace missing: $Label" }
    $depth = 0
    $end = -1
    for($i=$brace; $i -lt $Text.Length; $i++) {
        if($Text[$i] -eq '{') { $depth++ }
        elseif($Text[$i] -eq '}') {
            $depth--
            if($depth -eq 0) { $end = $i + 1; break }
        }
    }
    if($end -lt 0) { throw "P04 method closing brace missing: $Label" }
    return $Text.Substring(0,$start) + $Replacement + $Text.Substring($end)
}

$audioAnchor = @'
        // FireWire Flex-3000, Flex-5000 Stop ASIO IQ stream 
        public unsafe static void StopAudio() 
'@

$audioAnchorNew = @'
        // SQ4KOU P04: only application exit uses the immediate PortAudio/ASIO
        // abort path. Normal Start/Stop operation keeps the native KE9NS
        // graceful PA_StopStream behaviour unchanged.
        private static bool fast_application_shutdown = false;
        public static bool FastApplicationShutdown
        {
            get { return fast_application_shutdown; }
            set { fast_application_shutdown = value; }
        }

        // FireWire Flex-3000, Flex-5000 Stop ASIO IQ stream 
        public unsafe static void StopAudio() 
'@

$audio = Replace-ExactOnce $audio $audioAnchor $audioAnchorNew 'FastApplicationShutdown flag insertion'

$newStopAudio = @'
        public unsafe static void StopAudio()
        {
            int error = 0;
            Debug.WriteLine("STOP ASIO");

            if (fast_application_shutdown)
            {
                Debug.WriteLine("SQ4KOU P04 FAST APPLICATION ASIO EXIT");
                if (stream1 != (void*)IntPtr.Zero)
                {
                    PA19.PA_AbortStream(stream1);
                    error = PA19.PA_CloseStream(stream1);
                    stream1 = (void*)IntPtr.Zero;
                    if (error != 0) PortAudioErrorMessageBox(error);
                }
                return;
            }

            int error1 = PA19.PA_StopStream(stream1); //.323a

            if (error1 != 0)
            {
                Debug.WriteLine("PA_StopStream1 error: " + PA19.PA_GetErrorText(error));
                PA19.PA_AbortStream(stream1);
            }

            Thread.Sleep(100);  // .323a
            int cnt = 0;
            while (PA19.PA_IsStreamActive(stream1) == 1 && cnt < 50)
            {
                Thread.Sleep(200);
                cnt++;
            }

            error = PA19.PA_CloseStream(stream1);
            stream1 = (void*)IntPtr.Zero;
            if (error != 0) PortAudioErrorMessageBox(error);
            Thread.Sleep(200);  // native normal Stop/Start settle delay
        }
'@

$newStopVac = @'
        public unsafe static void StopAudioVAC()
        {
            int error = 0;
            Debug.WriteLine("StopAudioVAC");

            if (fast_application_shutdown)
            {
                Debug.WriteLine("SQ4KOU P04 FAST APPLICATION VAC EXIT");
                if (stream2 != (void*)IntPtr.Zero)
                {
                    PA19.PA_AbortStream(stream2);
                    error = PA19.PA_CloseStream(stream2);
                    stream2 = (void*)IntPtr.Zero;
                    if (error != 0) PortAudioErrorMessageBox(error);
                }
                return;
            }

            int error1 = PA19.PA_StopStream(stream2); //.323a

            if (error1 != 0)
            {
                Debug.WriteLine("PA_StopStream2 error: " + PA19.PA_GetErrorText(error));
                PA19.PA_AbortStream(stream2);
            }

            Thread.Sleep(100);  // .323a
            error = PA19.PA_CloseStream(stream2);
            stream2 = (void*)IntPtr.Zero;
            if (error != 0) PortAudioErrorMessageBox(error);
        }
'@

$newStopVac2 = @'
        public unsafe static void StopAudioVAC2()
        {
            int error = 0;
            Debug.WriteLine("StopAudioVAC2");

            if (fast_application_shutdown)
            {
                Debug.WriteLine("SQ4KOU P04 FAST APPLICATION VAC2 EXIT");
                if (stream3 != (void*)IntPtr.Zero)
                {
                    PA19.PA_AbortStream(stream3);
                    error = PA19.PA_CloseStream(stream3);
                    stream3 = (void*)IntPtr.Zero;
                    if (error != 0) PortAudioErrorMessageBox(error);
                }
                return;
            }

            int error1 = PA19.PA_StopStream(stream3); //.323a

            if (error1 != 0)
            {
                Debug.WriteLine("PA_StopStream3 error: " + PA19.PA_GetErrorText(error));
                PA19.PA_AbortStream(stream3);
            }

            Thread.Sleep(100);  // .323a
            error = PA19.PA_CloseStream(stream3);
            stream3 = (void*)IntPtr.Zero;
            if (error != 0) PortAudioErrorMessageBox(error);
        }
'@

$audio = Replace-CSharpMethod $audio '        public unsafe static void StopAudio()' $newStopAudio 'StopAudio'
$audio = Replace-CSharpMethod $audio '        public unsafe static void StopAudioVAC()' $newStopVac 'StopAudioVAC'
$audio = Replace-CSharpMethod $audio '        public unsafe static void StopAudioVAC2()' $newStopVac2 'StopAudioVAC2'

$consoleAnchor = @'
        public void Console_Closing(object sender, FormClosingEventArgs e)
        {
            Stopwatch sq4kouShutdown1Timer = Stopwatch.StartNew();

            if (chkPower.Checked)
'@

$consoleNew = @'
        public void Console_Closing(object sender, FormClosingEventArgs e)
        {
            Stopwatch sq4kouShutdown1Timer = Stopwatch.StartNew();

            // Application exit is terminal: do not spend up to 10 seconds waiting
            // for the FireWire/ASIO stream to drain gracefully. Audio.StopAudio()
            // will use PA_AbortStream only for this close path.
            Audio.FastApplicationShutdown = true;

            if (chkPower.Checked)
'@

$console = Replace-ExactOnce $console $consoleAnchor $consoleNew 'enable fast audio path before Power OFF'

$checks = @(
    @{ Name='fast flag'; Ok=$audio.Contains('public static bool FastApplicationShutdown') },
    @{ Name='ASIO abort path'; Ok=$audio.Contains('SQ4KOU P04 FAST APPLICATION ASIO EXIT') -and $audio.Contains('PA19.PA_AbortStream(stream1)') },
    @{ Name='VAC abort path'; Ok=$audio.Contains('SQ4KOU P04 FAST APPLICATION VAC EXIT') -and $audio.Contains('PA19.PA_AbortStream(stream2)') },
    @{ Name='VAC2 abort path'; Ok=$audio.Contains('SQ4KOU P04 FAST APPLICATION VAC2 EXIT') -and $audio.Contains('PA19.PA_AbortStream(stream3)') },
    @{ Name='native normal timeout preserved'; Ok=$audio.Contains('cnt < 50') -and $audio.Contains('Thread.Sleep(200)') },
    @{ Name='close-path flag set before power off'; Ok=$console.Contains('Audio.FastApplicationShutdown = true;') }
)

$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P04 fast audio exit post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($audioCs, $audio.Replace("`n", "`r`n"), $utf8)
[IO.File]::WriteAllText($consoleCs, $console.Replace("`n", "`r`n"), $utf8)
Stage 'PASS: terminal app close uses immediate ASIO/VAC abort; normal Power Stop/Start remains native'
