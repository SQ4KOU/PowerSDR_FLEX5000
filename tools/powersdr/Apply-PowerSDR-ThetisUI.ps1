[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SourceRoot
)

$ErrorActionPreference = "Stop"

function Write-Stage([string]$Message) {
    Write-Host "[THETIS-UI-STAGE1] $Message"
}

$consoleDir = Join-Path $SourceRoot "Console"
$consoleCs  = Join-Path $consoleDir "console.cs"
$projectCs = Join-Path $consoleDir "PowerSDR.csproj"
$helperSrc = Join-Path $PSScriptRoot "ThetisStage1Ui.cs"
$helperDst = Join-Path $consoleDir "ThetisStage1Ui.cs"

foreach ($required in @($consoleCs, $projectCs, $helperSrc)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Required Stage 1 input missing: $required"
    }
}

Write-Stage "Applying UI-only transplant to $SourceRoot"
Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force
Write-Stage "Copied ThetisStage1Ui.cs"

$utf8Bom = New-Object System.Text.UTF8Encoding($true)

# Main-form hook. It is deliberately installed after InitializeComponent(), and
# ThetisStage1Ui itself waits for Form.Shown before changing the visual tree.
# No FLEX-5000 PAL/FWC/audio initialization path is replaced or bypassed.
$consoleText = [IO.File]::ReadAllText($consoleCs)
if ($consoleText -notmatch 'ThetisStage1Ui\.Install\(this\);') {
    $annotated = [regex]'InitializeComponent\(\);\s*//\s*Windows Forms Generated Code'
    $annotatedMatches = $annotated.Matches($consoleText)

    if ($annotatedMatches.Count -eq 1) {
        $replacement = $annotatedMatches[0].Value + "`r`n            ThetisStage1Ui.Install(this); // SQ4KOU PowerSDR -> Thetis UI Stage 1"
        $consoleText = $annotated.Replace(
            $consoleText,
            [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $replacement },
            1)
    }
    else {
        $plain = [regex]'InitializeComponent\(\);'
        $plainMatches = $plain.Matches($consoleText)
        if ($plainMatches.Count -ne 1) {
            throw "Stage 1 UI hook anchor is ambiguous: InitializeComponent count=$($plainMatches.Count), annotated count=$($annotatedMatches.Count)"
        }
        $replacement = $plainMatches[0].Value + "`r`n            ThetisStage1Ui.Install(this); // SQ4KOU PowerSDR -> Thetis UI Stage 1"
        $consoleText = $plain.Replace(
            $consoleText,
            [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $replacement },
            1)
    }

    [IO.File]::WriteAllText($consoleCs, $consoleText, $utf8Bom)
    Write-Stage "Inserted deferred main-form hook"
}
else {
    Write-Stage "Main-form hook already present"
}

# Add helper to the legacy non-SDK csproj next to console.cs.
$projectText = [IO.File]::ReadAllText($projectCs)
if ($projectText -notmatch 'Compile Include="ThetisStage1Ui\.cs"') {
    $compileAnchor = [regex]'(?s)(<Compile Include="console\.cs">\s*<SubType>Form</SubType>\s*</Compile>)'
    $anchorMatches = $compileAnchor.Matches($projectText)
    if ($anchorMatches.Count -ne 1) {
        throw "PowerSDR.csproj console.cs Compile anchor count=$($anchorMatches.Count)"
    }

    $insert = '$1' + "`r`n    <Compile Include=`"ThetisStage1Ui.cs`">`r`n      <SubType>Code</SubType>`r`n    </Compile>"
    $projectText = $compileAnchor.Replace($projectText, $insert, 1)
    [IO.File]::WriteAllText($projectCs, $projectText, $utf8Bom)
    Write-Stage "Registered ThetisStage1Ui.cs in PowerSDR.csproj"
}
else {
    Write-Stage "PowerSDR.csproj already contains Stage 1 helper"
}

# Hard boundary: Stage 1 is UI only. The MSI build script independently hashes
# the native FLEX-5000 backend before and after this script and fails on change.
$forbiddenTokens = @(
    'NetworkIO',
    'ChannelMaster',
    'WDSP',
    'FWC\.Set',
    'PAL\.',
    'PortAudio'
)
$helperText = [IO.File]::ReadAllText($helperDst)
foreach ($token in $forbiddenTokens) {
    if ($helperText -match $token) {
        throw "Stage 1 helper crossed backend boundary: token '$token' found"
    }
}

if ($helperText -notmatch 'namespace\s+PowerSDR') {
    throw "Stage 1 helper namespace validation failed"
}
if ($consoleText -notmatch 'ThetisStage1Ui\.Install\(this\);') {
    throw "Stage 1 hook post-check failed"
}
if ($projectText -notmatch 'Compile Include="ThetisStage1Ui\.cs"') {
    throw "Stage 1 project post-check failed"
}

Write-Stage "PASS: Stage 1 UI integrated; native FLEX-5000 backend untouched"
