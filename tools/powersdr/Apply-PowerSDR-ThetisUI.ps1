[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SourceRoot
)

$ErrorActionPreference = "Stop"

function Write-Stage([string]$Message) {
    Write-Host "[THETIS-UI-STAGE2] $Message"
}

$consoleDir   = Join-Path $SourceRoot "Console"
$consoleCs    = Join-Path $consoleDir "console.cs"
$projectCs    = Join-Path $consoleDir "PowerSDR.csproj"
$assemblyInfo = Join-Path $consoleDir "AssemblyInfo.cs"
$helperSrc    = Join-Path $PSScriptRoot "ThetisStage1Ui.cs"
$helperDst    = Join-Path $consoleDir "ThetisStage1Ui.cs"

foreach ($required in @($consoleCs, $projectCs, $assemblyInfo, $helperSrc)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Required Stage 2 input missing: $required"
    }
}

Write-Stage "Applying UI-only transplant to $SourceRoot"
Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force
Write-Stage "Copied Stage 2 layout helper"

$utf8Bom = New-Object System.Text.UTF8Encoding($true)

# Give each CI UI package a strictly newer FILE version.  The assembly identity
# remains the native KE9NS 2.8.0.334 AssemblyVersion; only AssemblyFileVersion is
# advanced.  This is required because Windows Installer will otherwise treat two
# UI builds with the same 2.8.0 product/file version as the same installed build
# and can legitimately keep the older PowerSDR.exe on disk.
$uiBuild = 1
if ($env:GITHUB_RUN_NUMBER -match '^\d+$') {
    $uiBuild = [int]$env:GITHUB_RUN_NUMBER
}
if ($uiBuild -lt 1) { $uiBuild = 1 }
if ($uiBuild -gt 65535) { $uiBuild = 65535 }
$uiFileVersion = "2.8.$uiBuild.0"
$assemblyText = [IO.File]::ReadAllText($assemblyInfo)
$fileVersionRx = [regex]'\[assembly:\s*AssemblyFileVersion\("[^\"]+"\)\]'
$fileVersionMatches = $fileVersionRx.Matches($assemblyText)
if ($fileVersionMatches.Count -ne 1) {
    throw "AssemblyFileVersion anchor count=$($fileVersionMatches.Count)"
}
$assemblyText = $fileVersionRx.Replace($assemblyText, "[assembly: AssemblyFileVersion(`"$uiFileVersion`")]", 1)
[IO.File]::WriteAllText($assemblyInfo, $assemblyText, $utf8Bom)
Write-Stage "Set UI package file version to $uiFileVersion (AssemblyVersion unchanged)"

# Main-form hook.  Install is called after InitializeComponent(); the helper then
# waits for Form.Shown before it reparents only WinForms controls.  Native FLEX
# PAL/FWC/FireWire/ASIO/DttSP initialization remains untouched.
$consoleText = [IO.File]::ReadAllText($consoleCs)
if ($consoleText -notmatch 'ThetisStage1Ui\.Install\(this\);') {
    $annotated = [regex]'InitializeComponent\(\);\s*//\s*Windows Forms Generated Code'
    $annotatedMatches = $annotated.Matches($consoleText)

    if ($annotatedMatches.Count -eq 1) {
        $replacement = $annotatedMatches[0].Value + "`r`n            ThetisStage1Ui.Install(this); // SQ4KOU PowerSDR -> Thetis UI Stage 2"
        $consoleText = $annotated.Replace(
            $consoleText,
            [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $replacement },
            1)
    }
    else {
        $plain = [regex]'InitializeComponent\(\);'
        $plainMatches = $plain.Matches($consoleText)
        if ($plainMatches.Count -ne 1) {
            throw "Stage 2 UI hook anchor is ambiguous: InitializeComponent count=$($plainMatches.Count), annotated count=$($annotatedMatches.Count)"
        }
        $replacement = $plainMatches[0].Value + "`r`n            ThetisStage1Ui.Install(this); // SQ4KOU PowerSDR -> Thetis UI Stage 2"
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
    Write-Stage "Registered Stage 2 helper in PowerSDR.csproj"
}
else {
    Write-Stage "PowerSDR.csproj already contains UI helper"
}

# Hard boundary: this transplant is UI only.  No donor radio/DSP transport may
# enter the native PowerSDR FLEX-5000 build.
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
        throw "Stage 2 helper crossed backend boundary: token '$token' found"
    }
}

if ($helperText -notmatch 'namespace\s+PowerSDR') {
    throw "Stage 2 helper namespace validation failed"
}
if ($helperText -notmatch 'sq4kouThetisStage2Root') {
    throw "Stage 2 layout marker missing"
}
if ($consoleText -notmatch 'ThetisStage1Ui\.Install\(this\);') {
    throw "Stage 2 hook post-check failed"
}
if ($projectText -notmatch 'Compile Include="ThetisStage1Ui\.cs"') {
    throw "Stage 2 project post-check failed"
}
if ($assemblyText -notmatch [regex]::Escape("AssemblyFileVersion(`"$uiFileVersion`")")) {
    throw "Stage 2 file-version post-check failed"
}

Write-Stage "PASS: Stage 2 UI integrated; package is upgradeable; native FLEX-5000 backend untouched"
