[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$SourceRoot,
    [Parameter(Mandatory=$true)][string]$ThetisRoot
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$m){Write-Host "[P24-NATIVE-THETIS-METERS] $m"}

$consoleDir=Join-Path $SourceRoot 'Console'
$vendor=Join-Path $PSScriptRoot 'vendor\thetis-meters'
$projectCs=Join-Path $consoleDir 'PowerSDR.csproj'
$nl=[Environment]::NewLine
$utf8Bom=New-Object System.Text.UTF8Encoding($true)

$srcFiles=@(
 'MeterManager.cs',
 'ucMeter.cs',
 'ucMeter.Designer.cs',
 'frmMeterDisplay.cs',
 'frmMeterDisplay.Designer.cs',
 'frmVariablePicker.cs',
 'frmVariablePicker.Designer.cs',
 'clsMeterScriptEngine.cs',
 'clsLegacyItemController.cs',
 'clsCatAtonic.cs',
 'clsCATMessageQueue.cs',
 'clsImgeFetcher.cs',
 'clsTouchHandler.cs'
)

foreach($name in $srcFiles){
    $src=Join-Path $vendor $name
    if(!(Test-Path $src)){throw "P24 vendor source missing: $src"}
    $dst=Join-Path $consoleDir ("P24_" + $name)
    $text=[IO.File]::ReadAllText($src)

    # Keep original implementation but place it in the PowerSDR namespace.
    $text=$text.Replace('namespace Thetis','namespace PowerSDR')
    $text=$text.Replace('global::Thetis.Properties.Resources.','global::PowerSDR.P24MeterResources.')
    $text=$text.Replace('Thetis.Properties.Resources.','PowerSDR.P24MeterResources.')

    # Only compatibility substitutions for helpers PowerSDR KE9NS does not have.
    $text=$text.Replace('Common.DoubleBufferAll','P24ThetisMeterCompat.DoubleBufferAll')
    $text=$text.Replace('Common.CtrlKeyDown','P24ThetisMeterCompat.CtrlKeyDown')
    $text=$text.Replace('Common.ShiftKeyDown','P24ThetisMeterCompat.ShiftKeyDown')
    $text=$text.Replace('Common.ColourToString','P24ThetisMeterCompat.ColourToString')
    $text=$text.Replace('Common.ColourFromString','P24ThetisMeterCompat.ColourFromString')
    $text=$text.Replace('Common.FiveDigitHash','P24ThetisMeterCompat.FiveDigitHash')

    # PowerSDR does not expose the Thetis touch switch. Mouse behaviour remains native.
    $text=$text.Replace('_console.TouchSupport','P24ThetisMeterCompat.TouchSupport(_console)')

    # Thetis event bus members do not exist in KE9NS. Runtime state will be bridged separately.
    $text=[regex]::Replace($text,'(?m)^\s*_console\.MoxChangeHandlers\s*[+-]=.*?;\s*$','')
    $text=[regex]::Replace($text,'(?m)^\s*_console\.WindowStateChangedHandlers\s*[+-]=.*?;\s*$','')
    $text=[regex]::Replace($text,'(?m)^\s*_console\.RX2EnabledChangedHandlers\s*[+-]=.*?;\s*$','')

    # Main-window delta members are Thetis-only presentation offsets.
    $text=$text.Replace('_console.HDelta','0')
    $text=$text.Replace('_console.VDelta','0')
    $text=$text.Replace('Display.AdaptorInfo','P24DisplayAdaptorInfo')

    [IO.File]::WriteAllText($dst,$text,$utf8Bom)
}

Copy-Item (Join-Path $PSScriptRoot 'P24MeterResources.cs') (Join-Path $consoleDir 'P24MeterResources.cs') -Force
Copy-Item (Join-Path $PSScriptRoot 'P24ThetisMeterCompat.cs') (Join-Path $consoleDir 'P24ThetisMeterCompat.cs') -Force
Copy-Item (Join-Path $PSScriptRoot 'P24ThetisTypeCompat.cs') (Join-Path $consoleDir 'P24ThetisTypeCompat.cs') -Force

# Exact Thetis container chrome icons from the audited commit.
$resSrc=Join-Path $ThetisRoot 'Project Files\Source\Console\Resources'
$resDst=Join-Path $consoleDir 'P24ThetisMeterResources'
New-Item -ItemType Directory -Force -Path $resDst | Out-Null
$icons=@(
 'gear.png','pin_not_on_top.png','pin_on_top.png','dot.png',
 'dockIcon_dock.png','dockIcon_float.png','resizegrab.png',
 'arrow_left.png','arrow_topleft.png','arrow_up.png','arrow_topright.png',
 'arrow_right.png','arrow_bottomright.png','down.png','arrow_bottomleft.png'
)
foreach($icon in $icons){
    $p=Join-Path $resSrc $icon
    if(!(Test-Path $p)){throw "P24 exact Thetis resource missing: $icon"}
    Copy-Item $p (Join-Path $resDst $icon) -Force
}

# Install only the dependencies used by the original Thetis meter renderer/script engine.
$packagesDir=Join-Path $SourceRoot 'packages'
$packages=@(
 @('SharpDX','4.2.0'),
 @('SharpDX.Desktop','4.2.0'),
 @('SharpDX.Direct2D1','4.2.0'),
 @('SharpDX.Direct3D11','4.2.0'),
 @('SharpDX.DXGI','4.2.0'),
 @('SharpDX.Mathematics','4.2.0'),
 @('Microsoft.CodeAnalysis.Common','5.3.0'),
 @('Microsoft.CodeAnalysis.CSharp','5.3.0'),
 @('Microsoft.CodeAnalysis.Scripting.Common','5.3.0'),
 @('Microsoft.CodeAnalysis.CSharp.Scripting','5.3.0'),
 @('HtmlAgilityPack','1.12.4'),
 @('SkiaSharp','3.119.2'),
 @('Svg','3.4.7')
)
foreach($pkg in $packages){
    $id=$pkg[0];$ver=$pkg[1]
    $folder=Join-Path $packagesDir ($id+'.'+$ver)
    if(!(Test-Path $folder)){
        & nuget install $id -Version $ver -OutputDirectory $packagesDir -NonInteractive -DependencyVersion Highest
        if($LASTEXITCODE -ne 0){throw "nuget install failed: $id $ver"}
    }
}

$project=[IO.File]::ReadAllText($projectCs)

# Compile the original subsystem files.
$compileAnchor=[regex]'(<Compile Include="Skin\.cs"\s*/>)'
if($compileAnchor.Matches($project).Count -ne 1){throw 'P24 csproj Skin.cs anchor invalid'}
$compileFiles=@('P24MeterResources.cs','P24ThetisMeterCompat.cs','P24ThetisTypeCompat.cs')
foreach($name in $srcFiles){$compileFiles += ('P24_'+$name)}
foreach($name in $compileFiles){
    if($project -notmatch ('Compile Include="'+[regex]::Escape($name)+'"')){
        $project=$compileAnchor.Replace($project,'$1'+$nl+'    <Compile Include="'+$name+'" />',1)
    }
}

# Exact icon payloads are copied next to PowerSDR.exe.
$contentAnchor=[regex]'(<None Include="packages\.config"\s*/>)'
if($contentAnchor.Matches($project).Count -ne 1){throw 'P24 packages.config anchor invalid'}
foreach($icon in $icons){
    $inc='P24ThetisMeterResources\'+$icon
    if($project -notmatch [regex]::Escape($inc)){
        $entry='    <Content Include="'+$inc+'">'+$nl+
               '      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>'+$nl+
               '    </Content>'
        $project=$contentAnchor.Replace($project,$entry+$nl+'$1',1)
    }
}

# Add references without changing existing PowerSDR package versions.
$refAnchor=[regex]'(<Reference Include="System">)'
if($refAnchor.Matches($project).Count -ne 1){throw 'P24 System reference anchor invalid'}
$refs=@(
 @('SharpDX','SharpDX.4.2.0\lib\net45\SharpDX.dll'),
 @('SharpDX.Desktop','SharpDX.Desktop.4.2.0\lib\net45\SharpDX.Desktop.dll'),
 @('SharpDX.Direct2D1','SharpDX.Direct2D1.4.2.0\lib\net45\SharpDX.Direct2D1.dll'),
 @('SharpDX.Direct3D11','SharpDX.Direct3D11.4.2.0\lib\net45\SharpDX.Direct3D11.dll'),
 @('SharpDX.DXGI','SharpDX.DXGI.4.2.0\lib\net45\SharpDX.DXGI.dll'),
 @('SharpDX.Mathematics','SharpDX.Mathematics.4.2.0\lib\net45\SharpDX.Mathematics.dll'),
 @('Microsoft.CodeAnalysis','Microsoft.CodeAnalysis.Common.5.3.0\lib\netstandard2.0\Microsoft.CodeAnalysis.dll'),
 @('Microsoft.CodeAnalysis.CSharp','Microsoft.CodeAnalysis.CSharp.5.3.0\lib\netstandard2.0\Microsoft.CodeAnalysis.CSharp.dll'),
 @('Microsoft.CodeAnalysis.Scripting','Microsoft.CodeAnalysis.Scripting.Common.5.3.0\lib\netstandard2.0\Microsoft.CodeAnalysis.Scripting.dll'),
 @('Microsoft.CodeAnalysis.CSharp.Scripting','Microsoft.CodeAnalysis.CSharp.Scripting.5.3.0\lib\netstandard2.0\Microsoft.CodeAnalysis.CSharp.Scripting.dll'),
 @('HtmlAgilityPack','HtmlAgilityPack.1.12.4\lib\Net45\HtmlAgilityPack.dll'),
 @('SkiaSharp','SkiaSharp.3.119.2\lib\net462\SkiaSharp.dll'),
 @('Svg','Svg.3.4.7\lib\net472\Svg.dll')
)
foreach($ref in $refs){
    $name=$ref[0];$hint='..\packages\'+$ref[1]
    if($project -notmatch ('<Reference Include="'+[regex]::Escape($name)+'(?:,|")')){
        $entry='    <Reference Include="'+$name+'">'+$nl+
               '      <HintPath>'+$hint+'</HintPath>'+$nl+
               '      <Private>True</Private>'+$nl+
               '    </Reference>'
        $project=$refAnchor.Replace($project,$entry+$nl+'$1',1)
    }
}

[IO.File]::WriteAllText($projectCs,$project,$utf8Bom)

Stage 'Original Thetis meter sources + Direct2D/Roslyn dependencies staged'
