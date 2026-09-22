[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

# P29: keep P28 code unchanged; add the official Thetis Default Meters image pack.
& (Join-Path $PSScriptRoot 'Apply-PowerSDR-ThetisMetersDirect-P28.ps1') -SourceRoot $SourceRoot

$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$skinDir=Join-Path $consoleDir 'MeterSkins'
$utf8NoBom=New-Object Text.UTF8Encoding($false)
$nl=[Environment]::NewLine

$ThetisSkinsSha='601018a9486359b3bdb661063f4687b4c75ad917'
$zipUrl="https://raw.githubusercontent.com/ramdor/ThetisSkins/$ThetisSkinsSha/DefaultMeters.zip"
$zipPath=Join-Path $env:TEMP 'P29_DefaultMeters.zip'
$extractDir=Join-Path $env:TEMP 'P29_DefaultMeters'

if(Test-Path $extractDir){Remove-Item $extractDir -Recurse -Force}
New-Item -ItemType Directory -Force -Path $extractDir | Out-Null
New-Item -ItemType Directory -Force -Path $skinDir | Out-Null

Invoke-WebRequest -UseBasicParsing -Uri $zipUrl -OutFile $zipPath
Expand-Archive -Path $zipPath -DestinationPath $extractDir -Force

$required=@(
    'ananMM',
    'ananMM-bg',
    'ananMM-bg-tx',
    'cross-needle',
    'cross-needle-bg'
)

$copied=@()
foreach($name in $required)
{
    $matches=@(
        Get-ChildItem $extractDir -Recurse -File | Where-Object {
            $_.BaseName -ieq $name -and $_.Extension -match '^\.(png|jpg|jpeg|bmp)$'
        }
    )

    if($matches.Count -lt 1)
    {
        throw "P29 required meter skin asset not found in DefaultMeters.zip: $name"
    }

    $src=$matches | Select-Object -First 1
    $dst=Join-Path $skinDir $src.Name
    Copy-Item $src.FullName $dst -Force
    $copied += Get-Item $dst
}

$proj=[IO.File]::ReadAllText($projPath)
foreach($f in $copied)
{
    $rel='MeterSkins\'+$f.Name
    if($proj -notmatch ('Content Include="'+[regex]::Escape($rel)+'"'))
    {
        $item='    <Content Include="'+$rel+'">'+$nl+
              '      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>'+$nl+
              '    </Content>'+$nl
        $igPos=$proj.IndexOf('</ItemGroup>')
        if($igPos -lt 0){throw 'P29 csproj ItemGroup close missing'}
        $proj=$proj.Insert($igPos,$item)
    }
}
[IO.File]::WriteAllText($projPath,$proj,$utf8NoBom)

foreach($name in $required)
{
    $found=@(Get-ChildItem $skinDir -File | Where-Object { $_.BaseName -ieq $name })
    if($found.Count -lt 1){throw "P29 MeterSkins packaging gate missing: $name"}
}

Write-Host "P29_THETIS_SKINS_SHA=$ThetisSkinsSha"
Write-Host 'P29_METER_SKINS=OFFICIAL_DEFAULT_METERS'
Write-Host 'P29_ANANMM_SKINS=ananMM,ananMM-bg,ananMM-bg-tx'
Write-Host 'P29_CROSS_SKINS=cross-needle,cross-needle-bg'
Write-Host 'P29_P28_CODE=UNCHANGED'
