[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P18-CLEAN-STARTUP] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
$projectCs = Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$helperSrc = Join-Path $PSScriptRoot 'SQ4KOUStartupPresentation.cs'
$helperDst = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($consoleCs,$projectCs,$helperSrc)){
    if(!(Test-Path -LiteralPath $p)){ throw "P18 input missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13)+([string][char]10)
$lf = [string][char]10

Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force

$console=[IO.File]::ReadAllText($consoleCs).Replace($crlf,$lf)
$project=[IO.File]::ReadAllText($projectCs).Replace($crlf,$lf)

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace($crlf,$lf)
    $New=$New.Replace($crlf,$lf)
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P18 anchor missing: $Label" }
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){ throw "P18 anchor not unique: $Label" }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

if($project -notmatch 'Compile Include="SQ4KOUStartupPresentation\.cs"'){
    $project = Replace-ExactOnce $project @'
    <Compile Include="SQ4KOUUIDiagnostics.cs" />
'@ @'
    <Compile Include="SQ4KOUUIDiagnostics.cs" />
    <Compile Include="SQ4KOUStartupPresentation.cs" />
'@ 'register startup presentation helper'
}

$console = Replace-ExactOnce $console @'
            SQ4KOUUIDiagnostics.AttachUI(this);
'@ @'
            SQ4KOUUIDiagnostics.AttachUI(this);
            SQ4KOUStartupPresentation.Install(this, picDisplay);
'@ 'install clean startup presentation gate'

[IO.File]::WriteAllText($consoleCs,$console.Replace($lf,$crlf),$utf8)
[IO.File]::WriteAllText($projectCs,$project.Replace($lf,$crlf),$utf8)

Stage 'PASS: form visibility is gated until first display Paint completes and WinForms becomes idle'
