param(
    [string]$LogDir
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$pf = Join-Path $repo 'Project Files'
$src = Join-Path $pf 'Source'
$lib = Join-Path $pf 'lib'
$nr64 = Join-Path $lib 'NR_Algorithms_x64'
$nr86 = Join-Path $lib 'NR_Algorithms_x86'
$rnSrc = Join-Path $nr64 'src\rnnoise'
$sbSrc = Join-Path $nr64 'src\libspecbleach'
$fftw = Join-Path $lib 'fftw_x86'
$wdsp = Join-Path $src 'wdsp'
$wdspProj = Join-Path $wdsp 'wdsp.vcxproj'

if (!$LogDir) { $LogDir = Join-Path $repo 'artifacts\flex5000\logs' }
New-Item -ItemType Directory -Force -Path $LogDir,$nr86 | Out-Null

function Need([string]$p) { if (!(Test-Path -LiteralPath $p)) { throw "Required path missing: $p" } }
function Run([string]$Name, [string]$Exe, [string[]]$Args, [string]$Log) {
    Write-Host "NR_X86|$Name"
    & $Exe @Args 2>&1 | Tee-Object -FilePath $Log
    if ($LASTEXITCODE -ne 0) { throw "$Name failed rc=$LASTEXITCODE; see $Log" }
}

foreach ($p in @($rnSrc,$sbSrc,(Join-Path $nr64 'rnnoise.h'),(Join-Path $nr64 'specbleach_adenoiser.h'),(Join-Path $fftw 'fftw3.h'),$wdspProj)) { Need $p }
$fftwLib = Get-ChildItem -LiteralPath $fftw -File -Filter '*fftw3f*.lib' | Select-Object -First 1
if (!$fftwLib) { throw "No single-precision FFTW x86 import library in $fftw" }
$cmake = (Get-Command cmake.exe -ErrorAction Stop).Source

# RNNoise: exact compatibility change already proven by the earlier FLEX5000 V1.12 build.
$vecPath = Join-Path $rnSrc 'src\vec.h'
Need $vecPath
$vec = [IO.File]::ReadAllText($vecPath)
$x86Inc = '(?m)^\s*#include\s+"x86/x86_arch_macros\.h"\s*\r?\n'
$simd = '(?m)^[ \t]*#if[ \t]+defined\(__AVX__\)[ \t]*\|\|[ \t]*defined\(__SSE2__\)[ \t]*$'
if ([regex]::Matches($vec,$x86Inc).Count -eq 1) { $vec = [regex]::Replace($vec,$x86Inc,'') }
elseif ($vec -match 'x86/x86_arch_macros\.h') { throw 'RNNoise x86 include layout changed' }
if ([regex]::Matches($vec,$simd).Count -eq 1) {
    $vec = [regex]::Replace($vec,$simd,'#if 0 /* SQ4KOU FLEX5000 Win32 generic C */')
}
elseif ($vec -notmatch 'SQ4KOU FLEX5000 Win32 generic C') { throw 'RNNoise SIMD selector layout changed' }
[IO.File]::WriteAllText($vecPath,$vec,(New-Object Text.UTF8Encoding($false)))

$rnBuild = Join-Path $env:RUNNER_TEMP 'sq4kou-rnnoise-x86'
if (Test-Path $rnBuild) { Remove-Item $rnBuild -Recurse -Force }
Run 'RNNoise configure Win32' $cmake @('-S',$rnSrc,'-B',$rnBuild,'-A','Win32') (Join-Path $LogDir 'RNNOISE_X86_CONFIG.log')
Run 'RNNoise build Win32' $cmake @('--build',$rnBuild,'--config','Release','--target','rnnoise','--parallel') (Join-Path $LogDir 'RNNOISE_X86_BUILD.log')
$rnLib = Get-ChildItem $rnBuild -Recurse -File -Filter '*rnnoise*.lib' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
$rnDll = Get-ChildItem $rnBuild -Recurse -File -Filter '*rnnoise*.dll' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (!$rnLib -or !$rnDll) { throw 'RNNoise Win32 build did not produce .lib and .dll' }

# SpecBleach: switch disposable CMake references to x86 FFTW and replace one C99 VLA for MSVC.
$sbCmake = Join-Path $sbSrc 'CMakeLists.txt'
Need $sbCmake
$sbCmakeText = [IO.File]::ReadAllText($sbCmake)
$sbCmakeText = $sbCmakeText -replace 'fftw_x64','fftw_x86'
[IO.File]::WriteAllText($sbCmake,$sbCmakeText,(New-Object Text.UTF8Encoding($false)))

$spectral = Join-Path $sbSrc 'src\shared\utils\spectral_utils.c'
Need $spectral
$st = [IO.File]::ReadAllText($spectral)
$vla = '(?m)^([ \t]*)([A-Za-z_][A-Za-z0-9_ \t\*]*?)\s+tmp_buffer\s*\[\s*([^\]\r\n]+?)\s*\]\s*;[ \t]*$'
$matches = [regex]::Matches($st,$vla)
if ($matches.Count -eq 1) {
    $m=$matches[0]; $indent=$m.Groups[1].Value; $ctype=$m.Groups[2].Value.Trim(); $count=$m.Groups[3].Value.Trim()
    $replacement=$indent+$ctype+' *tmp_buffer = ('+$ctype+' *)_alloca(sizeof('+$ctype+') * ('+$count+'));'
    $st=$st.Substring(0,$m.Index)+$replacement+$st.Substring($m.Index+$m.Length)
    if ($st -notmatch '(?m)^\s*#\s*include\s*<malloc\.h>\s*$') {
        $first=[regex]::Match($st,'(?m)^\s*#\s*include[^\r\n]*(?:\r?\n)')
        if (!$first.Success) { throw 'SpecBleach include anchor missing' }
        $eol=if($first.Value.EndsWith("`r`n")){"`r`n"}else{"`n"}
        $at=$first.Index+$first.Length
        $st=$st.Substring(0,$at)+'#include <malloc.h>'+$eol+$st.Substring($at)
    }
    [IO.File]::WriteAllText($spectral,$st,(New-Object Text.UTF8Encoding($false)))
}
elseif ($st -notmatch '_alloca\s*\(') { throw "SpecBleach tmp_buffer VLA layout changed (matches=$($matches.Count))" }

$sbBuild = Join-Path $env:RUNNER_TEMP 'sq4kou-specbleach-x86'
if (Test-Path $sbBuild) { Remove-Item $sbBuild -Recurse -Force }
$fftwHeader = Join-Path $fftw 'fftw3.h'
$sbArgs=@('-S',$sbSrc,'-B',$sbBuild,'-A','Win32',
    "-DFFTW3f_INCLUDE_DIR=$fftw", "-DFFTW3F_INCLUDE_DIR=$fftw",
    "-DFFTW3f_LIBRARY=$($fftwLib.FullName)", "-DFFTW3F_LIBRARY=$($fftwLib.FullName)",
    "-DFFTW3_INCLUDE_DIR=$fftw", "-DFFTW3_LIBRARY=$($fftwLib.FullName)")
Run 'SpecBleach configure Win32' $cmake $sbArgs (Join-Path $LogDir 'SPECBLEACH_X86_CONFIG.log')
Run 'SpecBleach build Win32' $cmake @('--build',$sbBuild,'--config','Release','--target','specbleach','--parallel') (Join-Path $LogDir 'SPECBLEACH_X86_BUILD.log')
$sbLib = Get-ChildItem $sbBuild -Recurse -File -Filter '*specbleach*.lib' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
$sbDll = Get-ChildItem $sbBuild -Recurse -File -Filter '*specbleach*.dll' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if (!$sbLib -or !$sbDll) { throw 'SpecBleach Win32 build did not produce .lib and .dll' }

# Platform-correct disposable library tree. Generic RNNoise is also used under the AVX2 filename;
# same ABI, only optimization differs, avoiding any x64 DLL in the x86 MSI.
Copy-Item $rnLib.FullName (Join-Path $nr86 'rnnoise.lib') -Force
Copy-Item $rnDll.FullName (Join-Path $nr86 'rnnoise.dll') -Force
Copy-Item $rnDll.FullName (Join-Path $nr86 'rnnoise_avx2.dll') -Force
Copy-Item $sbLib.FullName (Join-Path $nr86 'specbleach.lib') -Force
Copy-Item $sbDll.FullName (Join-Path $nr86 'specbleach.dll') -Force
Copy-Item (Join-Path $nr64 'rnnoise.h') (Join-Path $nr86 'rnnoise.h') -Force
Copy-Item (Join-Path $nr64 'specbleach_adenoiser.h') (Join-Path $nr86 'specbleach_adenoiser.h') -Force
Copy-Item (Join-Path $nr64 'rnnoise.h') (Join-Path $wdsp 'rnnoise.h') -Force
Copy-Item (Join-Path $nr64 'specbleach_adenoiser.h') (Join-Path $wdsp 'specbleach_adenoiser.h') -Force

# Link Release|Win32 WDSP to the x86 NR import libraries only.
[xml]$x = Get-Content -LiteralPath $wdspProj -Raw
$groups=@($x.SelectNodes("/*[local-name()='Project']/*[local-name()='ItemDefinitionGroup']") | Where-Object { $_.Condition -match 'Release\|Win32' })
if ($groups.Count -ne 1) { throw "WDSP Release|Win32 group count=$($groups.Count)" }
$link=$groups[0].SelectSingleNode("./*[local-name()='Link']")
if (!$link) { throw 'WDSP Release|Win32 Link node missing' }
$ns=$x.DocumentElement.NamespaceURI
$dirs=$link.SelectSingleNode("./*[local-name()='AdditionalLibraryDirectories']")
if (!$dirs) { $dirs=$x.CreateElement('AdditionalLibraryDirectories',$ns); [void]$link.AppendChild($dirs) }
$dirParts=@($dirs.InnerText.Split(';') | Where-Object { $_ })
$relNr='../../lib/NR_Algorithms_x86'
if ($dirParts -notcontains $relNr) { $dirs.InnerText=($relNr+';'+$dirs.InnerText).TrimEnd(';') }
$deps=$link.SelectSingleNode("./*[local-name()='AdditionalDependencies']")
if (!$deps) { $deps=$x.CreateElement('AdditionalDependencies',$ns); [void]$link.AppendChild($deps) }
$depParts=@($deps.InnerText.Split(';') | Where-Object { $_ })
$prefix=@()
if ($depParts -notcontains 'rnnoise.lib') { $prefix+='rnnoise.lib' }
if ($depParts -notcontains 'specbleach.lib') { $prefix+='specbleach.lib' }
if ($prefix.Count) { $deps.InnerText=(($prefix -join ';')+';'+$deps.InnerText).TrimEnd(';') }
$x.Save($wdspProj)

foreach($p in @((Join-Path $nr86 'rnnoise.lib'),(Join-Path $nr86 'rnnoise.dll'),(Join-Path $nr86 'rnnoise_avx2.dll'),(Join-Path $nr86 'specbleach.lib'),(Join-Path $nr86 'specbleach.dll'),(Join-Path $wdsp 'rnnoise.h'),(Join-Path $wdsp 'specbleach_adenoiser.h'))) { Need $p }
Write-Host "NR_X86_READY=$nr86"
