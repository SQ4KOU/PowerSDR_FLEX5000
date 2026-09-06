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

# Hard invariant: this patch is a renderer transplant only. These are the
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

# P02: port the visible PanDisplay language from pinned Thetis into the isolated
# PowerSDR renderer.  This is not a skin or overlay: only the renderer copy in
# the disposable KE9NS worktree is changed.  DttSP remains the data source.
$utf8 = New-Object System.Text.UTF8Encoding($false)
$renderText = [IO.File]::ReadAllText($helperDst)

function Replace-Once([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P02 visual anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P02 visual anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

$renderText = Replace-Once $renderText '// Scope: RX1 PANAFALL rendering only.' "// SQ4KOU-DISPLAY-P02-THETIS-VISUAL`r`n// Scope: RX1 PANAFALL rendering only." 'P02 marker'
$renderText = Replace-Once $renderText 'private const int SQ4KOU_FREQ_SCALE_HEIGHT = 18;' 'private const int SQ4KOU_FREQ_SCALE_HEIGHT = 20;' 'Thetis 20px frequency ruler'

# P01 could silently fall back to the KE9NS renderer when Auto Brightness or
# moving average was enabled.  Both are ordinary RX display states, not special
# display modes, so P02 renders them through the same new PANAFALL path.
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

# Thetis enhanced waterfall: black low end, then blue/cyan/green/yellow/red/
# magenta-purple.  P01 already used the same 2/9..8/9 transfer curve; P02 makes
# its low-end independent of the KE9NS WaterfallLowColor skin setting.
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
Stage 'Applied P02 Thetis PanDisplay visual language to isolated renderer'

$displayText = [IO.File]::ReadAllText($displayCs)

# Extend, do not replace, the native Display type.
$classRx = [regex]'\bsealed\s+class\s+Display\b'
$classMatches = $classRx.Matches($displayText)
if($classMatches.Count -ne 1) {
    throw "Display class anchor count=$($classMatches.Count)"
}
$displayText = $classRx.Replace($displayText,'sealed partial class Display',1)

# Insert one dispatch gate only in the normal unsplit PANAFALL case. Every
# specialised mode remains on the original KE9NS implementation. If the new
# renderer declines a frame, execution continues into the untouched legacy path.
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
$verifyHelper = [IO.File]::ReadAllText($helperDst)
if($verifyDisplay -notmatch 'sealed\s+partial\s+class\s+Display') { throw 'Display partial post-check failed' }
if($verifyDisplay -notmatch 'SQ4KOU_CanUseCleanPanafall\(\)\s*&&\s*SQ4KOU_DrawCleanPanafall') { throw 'Panafall dispatch post-check failed' }
if($verifyProject -notmatch 'Compile Include="SQ4KOUDisplay\.cs"') { throw 'Renderer project post-check failed' }
if($verifyHelper -notmatch 'SQ4KOU-DISPLAY-P02-THETIS-VISUAL') { throw 'P02 visual marker post-check failed' }
if($verifyHelper -notmatch 'Color\.AntiqueWhite') { throw 'P02 Thetis ruler post-check failed' }
if($verifyHelper -notmatch 'Color\.FromArgb\(100, 0, 0, 127\)') { throw 'P02 Thetis pan fill post-check failed' }
if($verifyHelper -notmatch 'new Pen\(Color\.White, 1\.0f\)') { throw 'P02 Thetis trace post-check failed' }

foreach($rel in $backendRel) {
    $p = Join-Path $SourceRoot $rel
    $now = (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash
    if($now -ne $backendHash[$rel]) { throw "Native backend changed unexpectedly: $rel" }
}

Stage 'PASS: P02 Thetis visual display-only patch; native FLEX-5000 backend byte-identical'
