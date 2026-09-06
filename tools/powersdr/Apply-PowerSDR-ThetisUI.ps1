[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SourceRoot
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Write-Stage([string]$Message) {
    Write-Host "[THETIS-UI-DIRECT] $Message"
}

$consoleDir   = Join-Path $SourceRoot "Console"
$consoleCs    = Join-Path $consoleDir "console.cs"
$targetResx   = Join-Path $consoleDir "console.resx"
$projectCs    = Join-Path $consoleDir "PowerSDR.csproj"
$assemblyInfo = Join-Path $consoleDir "AssemblyInfo.cs"
$helperSrc    = Join-Path $PSScriptRoot "ThetisStage1Ui.cs"
$helperDst    = Join-Path $consoleDir "ThetisStage1Ui.cs"

foreach ($required in @($consoleCs, $targetResx, $projectCs, $assemblyInfo, $helperSrc)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Required direct-UI input missing: $required"
    }
}

# Pin both sides.  The PowerSDR SHA is pinned by Build-PowerSDR-MSI.ps1;
# this SHA pins the UI donor so a future upstream change cannot silently alter
# the generated form.
$ThetisSha = '852bf0ef0b4f3886a13fc2846489aee16f361872'
$donorResx = Join-Path $SourceRoot '_sq4kou_thetis_console.resx'
$donorUrl = "https://raw.githubusercontent.com/ramdor/Thetis/$ThetisSha/Project%20Files/Source/Console/console.resx"

Write-Stage "Fetching pinned Thetis console geometry $ThetisSha"
Invoke-WebRequest -UseBasicParsing -Uri $donorUrl -OutFile $donorResx
if (-not (Test-Path -LiteralPath $donorResx)) { throw 'Pinned Thetis console.resx download failed' }

[xml]$targetXml = [IO.File]::ReadAllText($targetResx)
[xml]$donorXml  = [IO.File]::ReadAllText($donorResx)

function New-DataIndex([xml]$Doc) {
    $map = @{}
    foreach ($node in $Doc.root.data) {
        $name = $node.GetAttribute('name')
        if (-not [String]::IsNullOrEmpty($name)) { $map[$name] = $node }
    }
    return $map
}

function Convert-DonorName([string]$Name) {
    if ($Name -eq 'panelBandGEN') { return 'panelBandGN' }
    if ($Name -eq 'panelBandGENRX2') { return 'panelBandGNRX2' }
    if ($Name -match '^radBandGEN(\d+)(RX2)?$') {
        $tail = if ($Matches[2]) { 'RX2' } else { '' }
        return "radBandGN$($Matches[1])$tail"
    }
    return $Name
}

function Get-Parent($Index, [string]$ControlName) {
    $key = ">>$ControlName.Parent"
    if (-not $Index.ContainsKey($key)) { return $null }
    return $Index[$key].InnerText.Trim()
}

$targetIndex = New-DataIndex $targetXml
$donorIndex  = New-DataIndex $donorXml
$copied = 0
$skippedParent = 0

# Thetis base client size is part of the layout contract.
if (-not $targetIndex.ContainsKey('$this.ClientSize') -or -not $donorIndex.ContainsKey('$this.ClientSize')) {
    throw 'ClientSize resource missing in donor or target'
}
$targetIndex['$this.ClientSize'].InnerText = $donorIndex['$this.ClientSize'].InnerText
$copied++

# Copy geometry only.  Parent/type/text/events/visibility/styles are never copied.
# A parent equality gate prevents a child that moved to a new Thetis-only
# container from being assigned container-relative coordinates in PowerSDR.
foreach ($donorNode in $donorXml.root.data) {
    $donorKey = $donorNode.GetAttribute('name')
    if ($donorKey -notmatch '^(.*)\.(Location|Size)$') { continue }

    $donorControl = $Matches[1]
    if ($donorControl -eq '$this') { continue }
    $property = $Matches[2]
    $targetControl = Convert-DonorName $donorControl
    $targetKey = "$targetControl.$property"
    if (-not $targetIndex.ContainsKey($targetKey)) { continue }

    $donorParent = Get-Parent $donorIndex $donorControl
    $targetParent = Get-Parent $targetIndex $targetControl
    if ($null -eq $donorParent -or $null -eq $targetParent) { continue }
    $donorParent = Convert-DonorName $donorParent

    if ($donorParent -ne $targetParent) {
        $skippedParent++
        continue
    }

    $targetIndex[$targetKey].InnerText = $donorNode.InnerText
    $copied++
}

if ($copied -lt 80) {
    throw "Thetis geometry transplant copied only $copied entries; refusing a partial layout"
}

$xmlSettings = New-Object System.Xml.XmlWriterSettings
$xmlSettings.Encoding = New-Object System.Text.UTF8Encoding($true)
$xmlSettings.Indent = $true
$xmlSettings.NewLineChars = "`r`n"
$xmlSettings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
$writer = [System.Xml.XmlWriter]::Create($targetResx, $xmlSettings)
try { $targetXml.Save($writer) } finally { $writer.Close() }

# Hard geometry gates.  These values come directly from the pinned Thetis form.
[xml]$verifyXml = [IO.File]::ReadAllText($targetResx)
$verifyIndex = New-DataIndex $verifyXml
$expected = @{
    '$this.ClientSize'     = '1018, 721'
    'grpVFOA.Location'    = '125, 24'
    'grpVFOA.Size'        = '232, 88'
    'grpVFOBetween.Location' = '360, 24'
    'grpVFOB.Location'    = '603, 24'
    'grpMultimeter.Location' = '841, 24'
    'panelDisplay.Location' = '124, 118'
    'panelDisplay.Size'   = '710, 300'
    'panelBandHF.Location'= '840, 150'
    'panelBandGN.Location'= '840, 150'
    'panelMode.Location'  = '840, 284'
    'panelFilter.Location'= '840, 392'
}
foreach ($key in $expected.Keys) {
    if (-not $verifyIndex.ContainsKey($key)) { throw "Geometry gate missing target resource: $key" }
    $actual = $verifyIndex[$key].InnerText.Trim()
    if ($actual -ne $expected[$key]) {
        throw "Geometry gate failed for $key : '$actual' != '$($expected[$key])'"
    }
}
Write-Stage "Geometry transplant PASS: copied=$copied parent-gated-skips=$skippedParent"

Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force
Write-Stage "Copied direct partial Console layout code"

$utf8Bom = New-Object System.Text.UTF8Encoding($true)

# Give every CI package a strictly newer file/product version so Windows
# Installer always replaces the user's preceding UI build.
$uiBuild = 1
if ($env:GITHUB_RUN_NUMBER -match '^\d+$') { $uiBuild = [int]$env:GITHUB_RUN_NUMBER }
if ($uiBuild -lt 1) { $uiBuild = 1 }
if ($uiBuild -gt 65535) { $uiBuild = 65535 }
$uiFileVersion = "2.8.$uiBuild.0"
$assemblyText = [IO.File]::ReadAllText($assemblyInfo)
$fileVersionRx = [regex]'\[assembly:\s*AssemblyFileVersion\("[^\"]+"\)\]'
if ($fileVersionRx.Matches($assemblyText).Count -ne 1) {
    throw 'AssemblyFileVersion anchor is not unique'
}
$assemblyText = $fileVersionRx.Replace($assemblyText, "[assembly: AssemblyFileVersion(`"$uiFileVersion`")]", 1)
[IO.File]::WriteAllText($assemblyInfo, $assemblyText, $utf8Bom)
Write-Stage "Set package file version $uiFileVersion"

# Install the direct layout before PowerSDR captures its resize basis.
$consoleText = [IO.File]::ReadAllText($consoleCs)
$initRx = [regex]'InitializeComponent\(\);\s*//\s*Windows Forms Generated Code'
$initMatches = $initRx.Matches($consoleText)
if ($initMatches.Count -ne 1) {
    throw "InitializeComponent annotated anchor count=$($initMatches.Count)"
}
$initReplacement = $initMatches[0].Value + "`r`n            SQ4KOU_ApplyThetisBaseLayout(); // direct Thetis geometry before GrabConsoleSizeBasis"
$consoleText = $initRx.Replace($consoleText,
    [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $initReplacement }, 1)

# Bypass only the KE9NS hard-coded movement routine. Console_Resize itself is
# retained, including native minimum-size/DPI/state handling, and all calls to
# ResizeConsole converge here.
$resizeRx = [regex]'(?ms)(public\s+void\s+ResizeConsole\s*\(\s*int\s+h_delta1\s*,\s*int\s+v_delta\s*\)\s*[^\{]*\{)'
$resizeMatches = $resizeRx.Matches($consoleText)
if ($resizeMatches.Count -ne 1) {
    throw "ResizeConsole anchor count=$($resizeMatches.Count)"
}
$resizeGuard = @"
$($resizeMatches[0].Value)
            if (SQ4KOU_ThetisUiEnabled)
            {
                SQ4KOU_ResizeThetis(h_delta1, v_delta);
                previous_delta = h_delta1 + v_delta;
                return;
            }
"@
$consoleText = $resizeRx.Replace($consoleText,
    [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $resizeGuard }, 1)
[IO.File]::WriteAllText($consoleCs, $consoleText, $utf8Bom)

# Register the direct partial class in the legacy project.
$projectText = [IO.File]::ReadAllText($projectCs)
if ($projectText -notmatch 'Compile Include="ThetisStage1Ui\.cs"') {
    $compileAnchor = [regex]'(?s)(<Compile Include="console\.cs">\s*<SubType>Form</SubType>\s*</Compile>)'
    if ($compileAnchor.Matches($projectText).Count -ne 1) {
        throw 'PowerSDR.csproj console.cs Compile anchor is not unique'
    }
    $insert = '$1' + "`r`n    <Compile Include=`"ThetisStage1Ui.cs`">`r`n      <SubType>Code</SubType>`r`n    </Compile>"
    $projectText = $compileAnchor.Replace($projectText, $insert, 1)
    [IO.File]::WriteAllText($projectCs, $projectText, $utf8Bom)
}

# UI-only boundary.  The helper may move controls but may not introduce a donor
# transport or DSP implementation.
$forbiddenTokens = @('NetworkIO','ChannelMaster','WDSP','FWC\.Set','PAL\.','PortAudio')
$helperText = [IO.File]::ReadAllText($helperDst)
foreach ($token in $forbiddenTokens) {
    if ($helperText -match $token) { throw "Direct UI helper crossed backend boundary: '$token'" }
}

if ($helperText -notmatch 'partial\s+class\s+Console') { throw 'Direct partial Console marker missing' }
if ($helperText -notmatch 'SQ4KOU_ResizeThetis') { throw 'Direct resize implementation missing' }
if ($consoleText -notmatch 'SQ4KOU_ApplyThetisBaseLayout\(\)') { throw 'Direct layout constructor hook missing' }
if ($consoleText -notmatch 'SQ4KOU_ResizeThetis\(h_delta1, v_delta\)') { throw 'Direct resize routing missing' }
if ($consoleText -match 'ThetisStage1Ui\.Install') { throw 'Obsolete runtime overlay hook survived' }
if ($projectText -notmatch 'Compile Include="ThetisStage1Ui\.cs"') { throw 'Direct UI project include missing' }
if ($assemblyText -notmatch [regex]::Escape("AssemblyFileVersion(`"$uiFileVersion`")")) { throw 'UI file-version gate failed' }

Remove-Item -LiteralPath $donorResx -Force -ErrorAction SilentlyContinue
Write-Stage "PASS: pinned Thetis geometry integrated directly; native PowerSDR backend untouched"
