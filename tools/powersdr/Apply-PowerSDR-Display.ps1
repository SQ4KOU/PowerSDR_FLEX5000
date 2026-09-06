[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-DISPLAY] $s" }

$consoleDir = Join-Path $SourceRoot 'Console'
$displayCs  = Join-Path $consoleDir 'display.cs'
$projectCs  = Join-Path $consoleDir 'PowerSDR.csproj'
$helperSrc  = Join-Path $PSScriptRoot 'SQ4KOUDisplay.cs'
$helperDst  = Join-Path $consoleDir 'SQ4KOUDisplay.cs'

foreach($p in @($displayCs,$projectCs,$helperSrc)) {
    if(!(Test-Path -LiteralPath $p)) { throw "Required display input missing: $p" }
}

# Hard invariant: this patch is a renderer transplant only.  These are the
# native FLEX-5000 backend files that must remain byte-identical.
$backendRel = @(
    'Console\audio.cs',
    'Console\FWC\fwc.cs',
    'Console\FWC\pal.cs',
    'Console\portaudio.cs'
)
$backendHash = @{}
foreach($rel in $backendRel) {
    $p = Join-Path $SourceRoot $rel
    if(!(Test-Path -LiteralPath $p)) { throw "Native backend source missing: $rel" }
    $backendHash[$rel] = (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash
}

$helperText = [IO.File]::ReadAllText($helperSrc)
foreach($token in @('SpecHPSDRDLL','NetworkIO','ChannelMaster','WDSP','FWC\.','PAL\.','PortAudio')) {
    if($helperText -match $token) { throw "Display helper crossed backend boundary: $token" }
}
if($helperText -notmatch 'sealed\s+partial\s+class\s+Display') {
    throw 'Display helper is not a partial PowerSDR Display implementation'
}
if($helperText -notmatch 'SQ4KOU_DrawCleanPanafall') {
    throw 'Clean panafall renderer marker missing'
}

Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force
Stage 'Copied isolated renderer module'

$utf8 = New-Object System.Text.UTF8Encoding($false)
$displayText = [IO.File]::ReadAllText($displayCs)

# Extend, do not replace, the native Display type.
$classRx = [regex]'\bsealed\s+class\s+Display\b'
$classMatches = $classRx.Matches($displayText)
if($classMatches.Count -ne 1) {
    throw "Display class anchor count=$($classMatches.Count)"
}
$displayText = $classRx.Replace($displayText,'sealed partial class Display',1)

# Insert one dispatch gate only in the normal unsplit PANAFALL case.  Every
# specialised mode remains on the original KE9NS implementation.  If the new
# renderer declines a frame, execution simply continues into the untouched
# legacy PANAFALL code below it.
$hookRx = [regex]'(?m)^(\s*case\s+DisplayMode\.PANAFALL:\s*\r?\n\s*\r?\n)(\s*if\s*\(map\s*==\s*1\))'
$hookMatches = $hookRx.Matches($displayText)
if($hookMatches.Count -ne 1) {
    throw "Unsplit PANAFALL dispatch anchor count=$($hookMatches.Count)"
}
$hook = @'
$1                        // SQ4KOU: isolated RX1 panafall renderer.
                        // One frame is rendered by one path only; there is no overlay/reparenting.
                        if (SQ4KOU_CanUseCleanPanafall() && SQ4KOU_DrawCleanPanafall(e.Graphics, W, H))
                        {
                            K9 = 3;
                            K11 = 0;
                            update = true;
                            break;
                        }

$2
'@
$displayText = $hookRx.Replace($displayText,$hook,1)
[IO.File]::WriteAllText($displayCs,$displayText,$utf8)
Stage 'Patched only Display class declaration and unsplit PANAFALL dispatch'

$projectText = [IO.File]::ReadAllText($projectCs)
if($projectText -notmatch 'Compile Include="SQ4KOUDisplay\.cs"') {
    $compileRx = [regex]'(?s)(<Compile Include="display\.cs">\s*<SubType>Code</SubType>\s*</Compile>)'
    $compileMatches = $compileRx.Matches($projectText)
    if($compileMatches.Count -ne 1) {
        throw "display.cs project anchor count=$($compileMatches.Count)"
    }
    $insert = '$1' + "`r`n    <Compile Include=`"SQ4KOUDisplay.cs`">`r`n      <SubType>Code</SubType>`r`n    </Compile>"
    $projectText = $compileRx.Replace($projectText,$insert,1)
    [IO.File]::WriteAllText($projectCs,$projectText,$utf8)
}
Stage 'Registered renderer module in legacy csproj'

# Post-checks.
$verifyDisplay = [IO.File]::ReadAllText($displayCs)
$verifyProject = [IO.File]::ReadAllText($projectCs)
if($verifyDisplay -notmatch 'sealed\s+partial\s+class\s+Display') { throw 'Display partial post-check failed' }
if($verifyDisplay -notmatch 'SQ4KOU_CanUseCleanPanafall\(\)\s*&&\s*SQ4KOU_DrawCleanPanafall') { throw 'Panafall dispatch post-check failed' }
if($verifyProject -notmatch 'Compile Include="SQ4KOUDisplay\.cs"') { throw 'Renderer project post-check failed' }

foreach($rel in $backendRel) {
    $p = Join-Path $SourceRoot $rel
    $now = (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash
    if($now -ne $backendHash[$rel]) { throw "Native backend changed unexpectedly: $rel" }
}

Stage 'PASS: display-only patch; native FLEX-5000 backend byte-identical'
