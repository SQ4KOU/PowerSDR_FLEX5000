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
$panSrc     = Join-Path $PSScriptRoot 'SQ4KOUPanadapter.cs'
$panDst     = Join-Path $consoleDir 'SQ4KOUPanadapter.cs'

foreach($p in @($displayCs,$projectCs,$helperSrc,$panSrc)) {
    if(!(Test-Path -LiteralPath $p)) { throw "Required display input missing: $p" }
}

# Hard invariant: display transplant only. Native FLEX-5000 backend files must
# remain byte-identical to the pinned KE9NS 2.8.0.334 source.
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

foreach($src in @($helperSrc,$panSrc)) {
    $text = [IO.File]::ReadAllText($src)
    foreach($token in @('SpecHPSDRDLL','NetworkIO','ChannelMaster','WDSP','FWC\.','PAL\.','PortAudio')) {
        if($text -match $token) { throw "Display helper crossed backend boundary: $token in $src" }
    }
    if($text -notmatch 'sealed\s+partial\s+class\s+Display') {
        throw "Display helper is not a partial PowerSDR Display implementation: $src"
    }
}

$helperText = [IO.File]::ReadAllText($helperSrc)
if($helperText -notmatch 'SQ4KOU_DrawCleanPanafall') { throw 'Clean panafall renderer marker missing' }
$panText = [IO.File]::ReadAllText($panSrc)
if($panText -notmatch 'SQ4KOU_DrawCleanPanadapter') { throw 'Clean panadapter renderer marker missing' }

Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force
Copy-Item -LiteralPath $panSrc -Destination $panDst -Force
Stage 'Copied isolated P02 PANAFALL and P03 PANADAPTER renderer modules'

# Preserve the already-confirmed P02 visual language exactly. This patch is
# applied only to the disposable helper copy in the clean KE9NS worktree.
$utf8 = New-Object System.Text.UTF8Encoding($false)
$renderText = [IO.File]::ReadAllText($helperDst)

function Replace-Once([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P02 visual anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P02 visual anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

$renderText = Replace-Once $renderText '// Scope: RX1 PANAFALL rendering only.' "// SQ4KOU-DISPLAY-P03-THETIS-PAN`r`n// Scope: RX1 PANAFALL rendering only." 'P03 marker'
$renderText = Replace-Once $renderText 'private const int SQ4KOU_FREQ_SCALE_HEIGHT = 18;' 'private const int SQ4KOU_FREQ_SCALE_HEIGHT = 20;' 'Thetis 20px frequency ruler'

# These ordinary RX states are rendered through the new path, matching the
# confirmed P02 behaviour.
$renderText = Replace-Once $renderText '            if (autobright != 0 || autobright2 != 0 || autobright3 != 0) return false;' '' 'Auto Brightness fallback removal'
$renderText = Replace-Once $renderText '            if (average_on && console.setupForm.chkAvgMove.Checked) return false;' '' 'Moving average fallback removal'

# Thetis PanDisplay visual defaults: black field, subtle alpha-white grid,
# yellow scales, red zero line, blue RX passband, white trace and blue fill.
$renderText = $renderText.Replace('new SolidBrush(display_background_color)', 'new SolidBrush(Color.Black)')
$renderText = Replace-Once $renderText 'new Pen(Color.FromArgb(120, grid_color))' 'new Pen(Color.FromArgb(65, 255, 255, 255))' 'Thetis grid pen'
$renderText = $renderText.Replace('new SolidBrush(grid_text_color)', 'new SolidBrush(Color.Yellow)')
$renderText = Replace-Once $renderText 'new Pen(grid_zero_color, 1.0f)' 'new Pen(Color.Red, 1.0f)' 'Thetis zero line'
$renderText = Replace-Once $renderText 'new SolidBrush(display_filter_color)' 'new SolidBrush(Color.FromArgb(95, 30, 144, 255))' 'Thetis RX filter fill'

$panRx = [regex]::new('(?s)\s+if \(pan_fill && width > 1\)\s*\{.*?\}\s*\r?\n\s*if \(width > 1\)\s*\r?\n\s*g\.DrawLines\(data_line_pen, sq4kou_pan_points\);')
if($panRx.Matches($renderText).Count -ne 1) { throw 'P02 pan fill/trace anchor count mismatch' }
$panReplacement = @'

                if (width > 1)
                {
                    PointF[] polygon = new PointF[width + 2];
                    Array.Copy(sq4kou_pan_points, polygon, width);
                    polygon[width] = new PointF(width - 1, dividerY - 1);
                    polygon[width + 1] = new PointF(0, dividerY - 1);
                    using (SolidBrush fill = new SolidBrush(Color.FromArgb(100, 0, 0, 127)))
                        g.FillPolygon(fill, polygon);
                    using (Pen trace = new Pen(Color.White, 1.0f))
                        g.DrawLines(trace, sq4kou_pan_points);
                }
'@
$renderText = $panRx.Replace($renderText,$panReplacement,1)

$oldRuler = @'
            using (Pen line = new Pen(grid_color))
                g.DrawLine(line, 0, y, width, y);
'@
$newRuler = @'
            using (Pen border = new Pen(Color.AntiqueWhite, 2.0f))
                g.DrawRectangle(border, 0, y, width - 1, SQ4KOU_FREQ_SCALE_HEIGHT - 1);
'@
$renderText = Replace-Once $renderText $oldRuler $newRuler 'Thetis frequency ruler border'
$renderText = Replace-Once $renderText 'using (Pen tick = new Pen(grid_color))' 'using (Pen tick = new Pen(Color.FromArgb(150, 255, 255, 255)))' 'Thetis frequency ticks'

# Thetis Enhanced waterfall curve retained unchanged from confirmed P02.
$renderText = Replace-Once $renderText 'if (value <= low) return waterfall_low_color;' 'if (value <= low) return Color.Black;' 'Thetis waterfall black floor'
$oldLowRamp = @'
                r = (int)((1.0f - p) * waterfall_low_color.R);
                g = (int)((1.0f - p) * waterfall_low_color.G);
                b = (int)(waterfall_low_color.B + p * (max - waterfall_low_color.B));
'@
$newLowRamp = @'
                r = 0;
                g = 0;
                b = (int)(p * max);
'@
$renderText = Replace-Once $renderText $oldLowRamp $newLowRamp 'Thetis waterfall black-to-blue ramp'

[IO.File]::WriteAllText($helperDst,$renderText,$utf8)
Stage 'Preserved confirmed P02 Thetis PANAFALL visual language'

$displayText = [IO.File]::ReadAllText($displayCs)

# Extend, do not replace, the native Display type.
$classRx = [regex]'\bsealed\s+class\s+Display\b'
$classMatches = $classRx.Matches($displayText)
if($classMatches.Count -ne 1) { throw "Display class anchor count=$($classMatches.Count)" }
$displayText = $classRx.Replace($displayText,'sealed partial class Display',1)

# P02 PANAFALL dispatch remains unchanged. Its unique normal-RX anchor is also
# used below to select the matching PANADAPTER case from KE9NS's 12 switches.
$panafallRx = [regex]'(?m)^(\s*case\s+DisplayMode\.PANAFALL:\s*\r?\n\s*\r?\n)(\s*if\s*\(map\s*==\s*1\))'
if($panafallRx.Matches($displayText).Count -ne 1) { throw 'Unsplit PANAFALL dispatch anchor count mismatch' }
$panafallHook = @'
$1                        // SQ4KOU P02: isolated RX1 panafall renderer.
                        if (SQ4KOU_CanUseCleanPanafall() && SQ4KOU_DrawCleanPanafall(e.Graphics, W, H))
                        {
                            K9 = 3;
                            K11 = 0;
                            update = true;
                            break;
                        }

$2
'@
$displayText = $panafallRx.Replace($displayText,$panafallHook,1)

# P03: KE9NS display.cs contains 12 PANADAPTER case labels in independent
# switches. The old global-count anchor therefore failed before compilation.
# Select only the PANADAPTER case in the same normal RX display switch as the
# already-proven unique P02 PANAFALL dispatch: the nearest preceding case label.
$panafallMarker = '// SQ4KOU P02: isolated RX1 panafall renderer.'
$panafallMarkerIndex = $displayText.IndexOf($panafallMarker, [StringComparison]::Ordinal)
if($panafallMarkerIndex -lt 0) { throw 'P02 PANAFALL marker missing after dispatch patch' }

$panToken = 'case DisplayMode.PANADAPTER:'
$panCaseIndex = $displayText.LastIndexOf($panToken, $panafallMarkerIndex, [StringComparison]::Ordinal)
if($panCaseIndex -lt 0) { throw 'Normal RX PANADAPTER case not found before P02 PANAFALL dispatch' }

$panLineEnd = $displayText.IndexOf("`n", $panCaseIndex)
if($panLineEnd -lt 0 -or $panLineEnd -ge $panafallMarkerIndex) { throw 'Normal RX PANADAPTER case line boundary invalid' }

$panadapterHook = @'
                        // SQ4KOU P03: isolated RX1 panadapter renderer.
                        if (SQ4KOU_CanUseCleanPanafall() && SQ4KOU_DrawCleanPanadapter(e.Graphics, W, H))
                        {
                            update = true;
                            break;
                        }

'@
$displayText = $displayText.Insert($panLineEnd + 1, $panadapterHook)

if(([regex]::Matches($displayText, 'SQ4KOU_DrawCleanPanadapter\(')).Count -ne 1) {
    throw 'P03 PANADAPTER dispatch insertion count mismatch'
}

[IO.File]::WriteAllText($displayCs,$displayText,$utf8)
Stage 'Patched P02 PANAFALL and the matching normal-RX P03 PANADAPTER dispatch only'

$projectText = [IO.File]::ReadAllText($projectCs)
$compileRx = [regex]'(?s)(<Compile Include="display\.cs">\s*<SubType>Code</SubType>\s*</Compile>)'
if($projectText -notmatch 'Compile Include="SQ4KOUDisplay\.cs"') {
    if($compileRx.Matches($projectText).Count -ne 1) { throw 'display.cs project anchor count mismatch' }
    $insert = '$1' + "`r`n    <Compile Include=`"SQ4KOUDisplay.cs`">`r`n      <SubType>Code</SubType>`r`n    </Compile>"
    $projectText = $compileRx.Replace($projectText,$insert,1)
}
if($projectText -notmatch 'Compile Include="SQ4KOUPanadapter\.cs"') {
    $anchor = '    <Compile Include="SQ4KOUDisplay.cs">' + "`r`n" + '      <SubType>Code</SubType>' + "`r`n" + '    </Compile>'
    if($projectText.IndexOf($anchor,[StringComparison]::Ordinal) -lt 0) { throw 'SQ4KOUDisplay.cs project anchor missing' }
    $insert = $anchor + "`r`n    <Compile Include=`"SQ4KOUPanadapter.cs`">`r`n      <SubType>Code</SubType>`r`n    </Compile>"
    $projectText = $projectText.Replace($anchor,$insert)
}
[IO.File]::WriteAllText($projectCs,$projectText,$utf8)
Stage 'Registered P02 and P03 renderer modules in legacy csproj'

# Post-checks.
$verifyDisplay = [IO.File]::ReadAllText($displayCs)
$verifyProject = [IO.File]::ReadAllText($projectCs)
$verifyHelper = [IO.File]::ReadAllText($helperDst)
$verifyPan = [IO.File]::ReadAllText($panDst)
if($verifyDisplay -notmatch 'sealed\s+partial\s+class\s+Display') { throw 'Display partial post-check failed' }
if($verifyDisplay -notmatch 'SQ4KOU_DrawCleanPanafall') { throw 'Panafall dispatch post-check failed' }
if($verifyDisplay -notmatch 'SQ4KOU_DrawCleanPanadapter') { throw 'Panadapter dispatch post-check failed' }
if($verifyProject -notmatch 'Compile Include="SQ4KOUDisplay\.cs"') { throw 'P02 project post-check failed' }
if($verifyProject -notmatch 'Compile Include="SQ4KOUPanadapter\.cs"') { throw 'P03 project post-check failed' }
if($verifyHelper -notmatch 'SQ4KOU-DISPLAY-P03-THETIS-PAN') { throw 'P03 visual marker post-check failed' }
if($verifyHelper -notmatch 'Color\.AntiqueWhite') { throw 'Thetis ruler post-check failed' }
if($verifyHelper -notmatch 'Color\.FromArgb\(100, 0, 0, 127\)') { throw 'Thetis pan fill post-check failed' }
if($verifyHelper -notmatch 'new Pen\(Color\.White, 1\.0f\)') { throw 'Thetis trace post-check failed' }
if($verifyPan -notmatch 'SQ4KOU_DrawCleanPanadapter') { throw 'P03 renderer file post-check failed' }
if($verifyPan -notmatch 'Color\.FromArgb\(100, 0, 0, 127\)') { throw 'P03 fill post-check failed' }
if($verifyPan -notmatch 'new Pen\(Color\.White, 1\.0f\)') { throw 'P03 trace post-check failed' }

foreach($rel in $backendRel) {
    $p = Join-Path $SourceRoot $rel
    $now = (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash
    if($now -ne $backendHash[$rel]) { throw "Native backend changed unexpectedly: $rel" }
}

Stage 'PASS: P03 PANADAPTER + confirmed P02 PANAFALL; native FLEX-5000 backend byte-identical'
