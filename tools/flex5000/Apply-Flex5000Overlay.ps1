$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$src  = Join-Path $repo 'Project Files\Source'
$consoleDir = Join-Path $src 'Console'

function Read-Utf8([string]$Path) {
    return [System.IO.File]::ReadAllText($Path)
}
function Write-Utf8([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}
function Replace-Exactly([string]$Text, [string]$Pattern, [string]$Replacement, [int]$Expected, [string]$Name) {
    $rx = [regex]::new($Pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $count = $rx.Matches($Text).Count
    if ($count -ne $Expected) { throw "$Name anchor count expected=$Expected actual=$count" }
    return $rx.Replace($Text, $Replacement)
}
function Inject-After-Once([string]$Text, [string]$Pattern, [string]$Marker, [string]$Insertion, [string]$Name) {
    if ($Text.Contains($Marker)) { return $Text }
    $rx = [regex]::new($Pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $m = $rx.Match($Text)
    if (!$m.Success) { throw "$Name anchor not found" }
    if ($rx.Matches($Text).Count -ne 1) { throw "$Name anchor is not unique" }
    return $Text.Substring(0, $m.Index + $m.Length) + $Insertion + $Text.Substring($m.Index + $m.Length)
}

# Managed project: dedicated x86 symbol + transport source.
$thetisProj = Join-Path $consoleDir 'Thetis.csproj'
[xml]$px = Get-Content -LiteralPath $thetisProj -Raw
$ns = New-Object System.Xml.XmlNamespaceManager($px.NameTable)
$ns.AddNamespace('m', 'http://schemas.microsoft.com/developer/msbuild/2003')
$pg = $px.SelectSingleNode("//m:PropertyGroup[contains(@Condition,'Release|x86')]", $ns)
if ($null -eq $pg) { throw 'Release|x86 PropertyGroup not found in Thetis.csproj' }
$dc = $pg.SelectSingleNode('m:DefineConstants', $ns)
if ($null -eq $dc) { throw 'DefineConstants missing in Release|x86' }
if (($dc.InnerText -split ';') -notcontains 'FLEX5000_NATIVE') {
    $dc.InnerText = ($dc.InnerText.TrimEnd(';') + ';FLEX5000_NATIVE;')
}
$existing = $px.SelectSingleNode("//m:Compile[@Include='Flex5000Transport.cs']", $ns)
if ($null -eq $existing) {
    $ig = $px.SelectSingleNode("//m:ItemGroup[m:Compile[@Include='cmaster.cs']]", $ns)
    if ($null -eq $ig) { throw 'Compile ItemGroup not found in Thetis.csproj' }
    $n = $px.CreateElement('Compile', $px.DocumentElement.NamespaceURI)
    $n.SetAttribute('Include', 'Flex5000Transport.cs')
    [void]$ig.AppendChild($n)
}
$px.Save($thetisProj)

# Native cmASIO project: compile the dedicated 8x8 FLEX host.
$asioProj = Join-Path $src 'cmASIO\cmASIO.vcxproj'
[xml]$ax = Get-Content -LiteralPath $asioProj -Raw
$ans = New-Object System.Xml.XmlNamespaceManager($ax.NameTable)
$ans.AddNamespace('m', 'http://schemas.microsoft.com/developer/msbuild/2003')
if ($null -eq $ax.SelectSingleNode("//m:ClInclude[@Include='flexasio.h']", $ans)) {
    $ig = $ax.SelectSingleNode("//m:ItemGroup[m:ClInclude[@Include='framework.h']]", $ans)
    if ($null -eq $ig) { throw 'cmASIO include ItemGroup not found' }
    $n = $ax.CreateElement('ClInclude', $ax.DocumentElement.NamespaceURI)
    $n.SetAttribute('Include', 'flexasio.h')
    [void]$ig.AppendChild($n)
}
if ($null -eq $ax.SelectSingleNode("//m:ClCompile[@Include='flexasio.cpp']", $ans)) {
    $ig = $ax.SelectSingleNode("//m:ItemGroup[m:ClCompile[@Include='hostsample.cpp']]", $ans)
    if ($null -eq $ig) { throw 'cmASIO compile ItemGroup not found' }
    $n = $ax.CreateElement('ClCompile', $ax.DocumentElement.NamespaceURI)
    $n.SetAttribute('Include', 'flexasio.cpp')
    [void]$ig.AppendChild($n)
}
$ax.Save($asioProj)

# FLEX owns radio discovery/tuning; HPSDR code remains compiled but unreachable.
$networkPath = Join-Path $consoleDir 'HPSDR\NetworkIO.cs'
$t = Read-Utf8 $networkPath
$t = Inject-After-Once $t 'public static int InitRadio\(\)\s*\{' 'FLEX5000_NATIVE_INITRADIO' @"

#if FLEX5000_NATIVE
            // FLEX5000_NATIVE_INITRADIO: bypass HPSDR discovery entirely.
            return Flex5000Transport.InitializeRadio();
#endif
"@ 'NetworkIO.InitRadio'
$t = Inject-After-Once $t 'unsafe public static void VFOfreq\(int id, double f, int tx\)\s*\{' 'FLEX5000_NATIVE_VFO' @"

#if FLEX5000_NATIVE
            // FLEX5000_NATIVE_VFO: PAL/FWC uses MHz and the proven 500 MHz DDS clock.
            _lastVFOfreq[tx][id] = f;
            Flex5000Transport.SetFrequency(id, f * _freq_correction_factor, tx != 0);
            return;
#endif
"@ 'NetworkIO.VFOfreq'
Write-Utf8 $networkPath $t

# Audio start/stop use FLEX ASIO. Thetis/WDSP/VAC/TCI remain unchanged.
$audioPath = Join-Path $consoleDir 'audio.cs'
$t = Read-Utf8 $audioPath
if (!$t.Contains('FLEX5000_NATIVE_AUDIO_START')) {
    $t = Replace-Exactly $t '^[ \t]*int result = NetworkIO\.StartAudioNative\(\);[ \t]*\r?$' @"
#if FLEX5000_NATIVE
            // FLEX5000_NATIVE_AUDIO_START
            int result = Flex5000Transport.Start();
#else
            int result = NetworkIO.StartAudioNative();
#endif
"@ 1 'Audio.StartAudioNative'
}
if (!$t.Contains('FLEX5000_NATIVE_AUDIO_STOP')) {
    $t = Replace-Exactly $t '^[ \t]*NetworkIO\.StopAudio\(\);[ \t]*\r?$' @"
#if FLEX5000_NATIVE
            // FLEX5000_NATIVE_AUDIO_STOP
            Flex5000Transport.Stop();
#else
            NetworkIO.StopAudio();
#endif
"@ 1 'Audio.StopAudio'
}
$t = Inject-After-Once $t '(?s)public static bool MOX\s*\{.*?set\s*\{\s*mox = value;' 'FLEX5000_NATIVE_MOX_STATE' @"

#if FLEX5000_NATIVE
                // FLEX5000_NATIVE_MOX_STATE: Thetis/WDSP state only; hardware edge is HdwMOXChanged.
#endif
"@ 'Audio.MOX'
if (!$t.Contains('FLEX5000_NATIVE_OUTPUT_POWER')) {
    $pattern = '^[ \t]*NetworkIO\.SetOutputPower\(\(float\)\(value \* 1\.02\)\);[ \t]*\r?$'
    $rx = [regex]::new($pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $count = $rx.Matches($t).Count
    if ($count -ne 1) { throw "Audio output-power anchor count expected=1 actual=$count" }
    $t = $rx.Replace($t, @"
#if !FLEX5000_NATIVE
                NetworkIO.SetOutputPower((float)(value * 1.02));
#else
                // FLEX5000_NATIVE_OUTPUT_POWER: P0 keeps fixed conservative TX IQ scale.
#endif
"@)
}
Write-Utf8 $audioPath $t

# Keep native Thetis MOX/UI/DSP, terminate hardware transition at PAL/FWC.
$consolePath = Join-Path $consoleDir 'console.cs'
$t = Read-Utf8 $consolePath
$t = Inject-After-Once $t 'private void HdwMOXChanged\(bool tx, double freq\)\s*\{' 'FLEX5000_NATIVE_HDW_MOX' @"

#if FLEX5000_NATIVE
            // FLEX5000_NATIVE_HDW_MOX: no HPSDR/Alex/TRX hardware commands in FLEX build.
            Flex5000Transport.SetFrequency(0, freq, true);
            Flex5000Transport.SetMox(tx);
            return;
#endif
"@ 'Console.HdwMOXChanged'
if (!$t.Contains('FLEX5000_NATIVE_PTT_POLL')) {
    $rx = [regex]::new('^[ \t]*int dotdashptt = NetworkIO\.nativeGetDotDashPTT\(\);[ \t]*\r?$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $count = $rx.Matches($t).Count
    if ($count -lt 1) { throw 'Console nativeGetDotDashPTT anchors not found' }
    $script:firstPtt = $true
    $t = $rx.Replace($t, {
        param($m)
        $marker = if ($script:firstPtt) { "            // FLEX5000_NATIVE_PTT_POLL`r`n" } else { '' }
        $script:firstPtt = $false
        return @"
#if FLEX5000_NATIVE
$marker            int dotdashptt = 0; // physical FLEX PTT deferred; manual MOX/CAT remains native Thetis
#else
$m
#endif
"@
    })
}
Write-Utf8 $consolePath $t

# ChannelMaster is the DSP engine; no Ethernet/USB RNet transport in FLEX build.
$cmPath = Join-Path $consoleDir 'cmaster.cs'
$t = Read-Utf8 $cmPath
if (!$t.Contains('FLEX5000_NATIVE_NO_RNET')) {
    $t = Replace-Exactly $t '^[ \t]*NetworkIO\.CreateRNet\(\);[ \t]*\r?$' @"
#if !FLEX5000_NATIVE
            NetworkIO.CreateRNet();
#else
            // FLEX5000_NATIVE_NO_RNET: samples arrive directly from ASIO FlexRadio.
#endif
"@ 1 'cmaster.CreateRNet'
}
Write-Utf8 $cmPath $t

$gates = @(
    @{ Path=$thetisProj; Text='FLEX5000_NATIVE' },
    @{ Path=$thetisProj; Text='Flex5000Transport.cs' },
    @{ Path=$asioProj; Text='flexasio.cpp' },
    @{ Path=$networkPath; Text='FLEX5000_NATIVE_INITRADIO' },
    @{ Path=$networkPath; Text='FLEX5000_NATIVE_VFO' },
    @{ Path=$audioPath; Text='FLEX5000_NATIVE_AUDIO_START' },
    @{ Path=$audioPath; Text='FLEX5000_NATIVE_AUDIO_STOP' },
    @{ Path=$consolePath; Text='FLEX5000_NATIVE_HDW_MOX' },
    @{ Path=$cmPath; Text='FLEX5000_NATIVE_NO_RNET' }
)
foreach ($g in $gates) {
    if (!(Read-Utf8 $g.Path).Contains($g.Text)) { throw "Overlay verification failed: $($g.Text)" }
}

Write-Host 'FLEX5000 overlay applied successfully.'
