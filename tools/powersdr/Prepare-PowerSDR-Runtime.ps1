param(
    [Parameter(Mandatory=$true)][string]$WorkRoot,
    [Parameter(Mandatory=$true)][string]$OutDir,
    [Parameter(Mandatory=$true)][string]$LogRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$FullUrl = 'https://github.com/ke9ns/PowerSDR-KE9NS-v2.8.0/releases/download/v2.8.0.329/PowerSDR_v2.8.0_Installer.exe'
$FullSha = 'ee31af4f244b4a0939bf6bed9987d0afc23d09cc64632c4772d6bb283ea767cd'
$IncUrl = 'https://github.com/ke9ns/PowerSDR-KE9NS-v2.8.0/releases/download/v2.8.0.329/PowerSDR_KE9NS_V2.8.0.329_Incremental_Installer.msi'
$IncSha = '6cb0f4aa820e4d7366e962e4c6f06eaf50326d886e87038d465e8a1f86e4e41c'

function Assert-Hash([string]$Path,[string]$Expected,[string]$Label) {
    $actual=(Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
    if($actual -ne $Expected){throw "$Label SHA256 mismatch: $actual"}
}

function Copy-AppTree([string]$Root,[string]$Label) {
    $candidates = Get-ChildItem -LiteralPath $Root -Recurse -File -Filter PowerSDR.exe -ErrorAction SilentlyContinue
    if(!$candidates){throw "$Label: no PowerSDR.exe found"}
    $ranked = foreach($exe in $candidates){
        $dir=$exe.Directory.FullName
        $dllCount=(Get-ChildItem -LiteralPath $dir -File -Filter '*.dll' -ErrorAction SilentlyContinue).Count
        [pscustomobject]@{Exe=$exe;Dir=$dir;Dlls=$dllCount}
    }
    $best=$ranked | Sort-Object Dlls -Descending | Select-Object -First 1
    Write-Host "$Label APPDIR=$($best.Dir) DLLS=$($best.Dlls)"
    Copy-Item -LiteralPath (Join-Path $best.Dir '*') -Destination $OutDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $OutDir,$LogRoot | Out-Null

# Full official installer supplies the complete legacy PowerSDR runtime that the
# KE9NS source tree intentionally does not carry. Extract only; do not execute it.
$fullExe=Join-Path $WorkRoot '_ke9ns_full.exe'
$fullRoot=Join-Path $WorkRoot '_ke9ns_full'
Invoke-WebRequest -UseBasicParsing -Uri $FullUrl -OutFile $fullExe
Assert-Hash $fullExe $FullSha 'KE9NS full installer'
New-Item -ItemType Directory -Force -Path $fullRoot | Out-Null
$seven='C:\Program Files\7-Zip\7z.exe'
if(!(Test-Path -LiteralPath $seven)){$seven=(Get-Command 7z.exe -ErrorAction SilentlyContinue).Source}
if(!$seven -or !(Test-Path -LiteralPath $seven)){throw '7-Zip not found on runner'}
& $seven x '-y' "-o$fullRoot" $fullExe | Tee-Object -FilePath (Join-Path $LogRoot 'KE9NS_FULL_7ZIP.log')
if($LASTEXITCODE -ne 0){throw "7-Zip full installer extraction failed rc=$LASTEXITCODE"}
Copy-AppTree $fullRoot 'KE9NS_FULL'

# Incremental official MSI then overlays the current KE9NS v2.8.0.329 files.
$incMsi=Join-Path $WorkRoot '_ke9ns_incremental.msi'
$incRoot=Join-Path $WorkRoot '_ke9ns_incremental'
Invoke-WebRequest -UseBasicParsing -Uri $IncUrl -OutFile $incMsi
Assert-Hash $incMsi $IncSha 'KE9NS incremental installer'
New-Item -ItemType Directory -Force -Path $incRoot | Out-Null
$msiLog=Join-Path $LogRoot 'KE9NS_INCREMENTAL_EXTRACT.log'
$msiArgs=@('/a',"`"$incMsi`"",'/qn',"TARGETDIR=`"$incRoot`"",'/L*v',"`"$msiLog`"")
$p=Start-Process msiexec.exe -ArgumentList $msiArgs -Wait -PassThru
if($p.ExitCode -ne 0){throw "KE9NS incremental extraction failed rc=$($p.ExitCode)"}

# The administrative image can be flattened by MSI authoring. Overlay every
# file found beside the best PowerSDR.exe candidate, and then resolve any
# required DLL still absent by exact-name recursive search in the image.
Copy-AppTree $incRoot 'KE9NS_INCREMENTAL'

$required=@(
 'DttSP.dll','Interop.TDxInput.dll','Sanford.Collections.dll',
 'Sanford.Multimedia.dll','Sanford.Multimedia.Midi.dll',
 'Sanford.Multimedia.Timers.dll','Sanford.Threading.dll','TNF.dll'
)
foreach($name in $required){
    $dest=Join-Path $OutDir $name
    if(!(Test-Path -LiteralPath $dest)){
        $hit=Get-ChildItem -LiteralPath $fullRoot,$incRoot -Recurse -File -Filter $name -ErrorAction SilentlyContinue | Select-Object -First 1
        if($hit){Copy-Item -LiteralPath $hit.FullName -Destination $dest -Force}
    }
    if(!(Test-Path -LiteralPath $dest)){throw "Required official PowerSDR runtime missing: $name"}
}

$runtimeFiles=(Get-ChildItem -LiteralPath $OutDir -File).Count
$runtimeDlls=(Get-ChildItem -LiteralPath $OutDir -File -Filter '*.dll').Count
if($runtimeDlls -lt 20){throw "Official runtime incomplete: DLL count=$runtimeDlls"}
Write-Host "POWERSDR_RUNTIME_READY files=$runtimeFiles dlls=$runtimeDlls"
Write-Output "FULL_SHA256=$FullSha"
Write-Output "INCREMENTAL_SHA256=$IncSha"
