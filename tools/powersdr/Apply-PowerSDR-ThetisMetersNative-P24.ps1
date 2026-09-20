[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$nl=[Environment]::NewLine
$consoleDir=Join-Path $SourceRoot 'Console'
$projPath=Join-Path $consoleDir 'PowerSDR.csproj'
$commonPath=Join-Path $consoleDir 'common.cs'
$utf8=New-Object Text.UTF8Encoding($true)

$vendor=Join-Path $PSScriptRoot 'vendor\thetis-meters'
$copy=@(
 'MeterManager.cs',
 'ucMeter.cs',
 'ucMeter.Designer.cs',
 'frmMeterDisplay.cs',
 'frmMeterDisplay.Designer.cs',
 'frmVariablePicker.cs',
 'frmVariablePicker.Designer.cs',
 'clsCatAtonic.cs',
 'clsCATMessageQueue.cs',
 'clsLegacyItemController.cs',
 'clsTouchHandler.cs',
 'ucSignalSelect.cs',
 'ucSignalSelect.Designer.cs'
)

foreach($name in $copy){
  $src=Join-Path $vendor $name
  if(!(Test-Path $src)){throw "P24 vendored source missing: $name"}
  $dst=Join-Path $consoleDir ('P24_'+$name)
  $c=[IO.File]::ReadAllText($src)
  # Namespace only; preserve literal serialized Thetis type names and provenance text.
  $c=$c.Replace('namespace Thetis','namespace PowerSDR')
  $c=$c.Replace('global::Thetis.','global::PowerSDR.')
  $c=$c.Replace('Thetis.ColorButton','PowerSDR.ColorButton')
  $c=$c.Replace('Thetis.ucSignalSelect','PowerSDR.ucSignalSelect')
  $c=$c.Replace('Display.AdaptorInfo','P24AdaptorInfo')

  # Resource artwork is added in a later gate. For the first native compile,
  # remove only generated resource assignments, never renderer logic.
  $c=[regex]::Replace($c,'global::PowerSDR\.Properties\.Resources\.[A-Za-z0-9_]+','null')
  [IO.File]::WriteAllText($dst,$c,$utf8)
}

Copy-Item (Join-Path $PSScriptRoot 'P24ThetisMetersCompat.cs') (Join-Path $consoleDir 'P24ThetisMetersCompat.cs') -Force
Copy-Item (Join-Path $PSScriptRoot 'P24ThetisMetersCommonCompat.cs') (Join-Path $consoleDir 'P24ThetisMetersCommonCompat.cs') -Force

# Make Common extensible only in the isolated worktree.
$common=[IO.File]::ReadAllText($commonPath)
if($common -notmatch 'public partial class Common'){
  $common=$common.Replace('public class Common','public partial class Common')
  [IO.File]::WriteAllText($commonPath,$common,$utf8)
}

$proj=[IO.File]::ReadAllText($projPath)
$compileFiles=@()
foreach($name in $copy){$compileFiles += ('P24_'+$name)}
$compileFiles += 'P24ThetisMetersCompat.cs'
$compileFiles += 'P24ThetisMetersCommonCompat.cs'

$anchor='<Compile Include="Skin.cs" />'
if(!$proj.Contains($anchor)){throw 'P24 csproj compile anchor missing'}
foreach($name in $compileFiles){
  if($proj -notmatch ('Compile Include="'+[regex]::Escape($name)+'"')){
    $proj=$proj.Replace($anchor,$anchor+$nl+'    <Compile Include="'+$name+'" />')
  }
}
[IO.File]::WriteAllText($projPath,$proj,$utf8)

Write-Host 'P24_NATIVE_THETIS_SOURCES=COPIED'
Write-Host ('P24_NATIVE_THETIS_SOURCE_COUNT='+$compileFiles.Count)
Write-Host 'P24_RENDERER=THETIS_SHARPDX_DIRECT2D'
