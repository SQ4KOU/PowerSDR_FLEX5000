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
$resourceNames=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)

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

    # Original Thetis image resources are kept as exact PNG payloads, but resolved
    # through a lightweight runtime loader instead of importing Thetis Resources.resx.
    $resourceRx=[regex]'(?:(?:global::)?Thetis\.)?Properties\.Resources\.([A-Za-z0-9_]+)'
    foreach($m in $resourceRx.Matches($text)){[void]$resourceNames.Add($m.Groups[1].Value)}
    $text=$resourceRx.Replace($text,[System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        'global::PowerSDR.P24MeterResources.Get("'+$m.Groups[1].Value+'")'
    })

    # Route Thetis.Console accesses through one dynamic compatibility facade.
    # This preserves the original MeterManager/UI code instead of cloning hundreds
    # of Thetis-only members into the native PowerSDR Console class.
    if($name -in @('MeterManager.cs','clsLegacyItemController.cs','frmMeterDisplay.cs','ucMeter.cs')){
        $text=[regex]::Replace($text,'\bConsole\s+(?=(?:_console|c|console)\b)','dynamic ')
        $text=$text.Replace('public Console Console','public dynamic Console')
        $text=$text.Replace('_console = c;','_console = P24ConsoleDynamic.WrapObject(c);')
        $text=$text.Replace('_console = console;','_console = P24ConsoleDynamic.WrapObject(console);')
    }

    # Static Thetis service classes are also isolated behind reflective facades.
    foreach($staticName in @('Display','Common','BandStackManager','ThetisBotDiscord','OtherButtonIdHelpers','HardwareSpecific','MNotchDB','SpecHPSDRDLL','Alex','ColorInterpolator')){
        $text=[regex]::Replace($text,'(?<![A-Za-z0-9_\.])'+[regex]::Escape($staticName)+'\.','P24Statics.'+$staticName+'.')
    }
    $text=$text.Replace('HiPerfTimer','P24HiPerfTimer')

    # Only compatibility substitutions for helpers PowerSDR KE9NS does not have.
    $text=$text.Replace('P24Statics.Common.DoubleBufferAll','P24ThetisMeterCompat.DoubleBufferAll')
    $text=$text.Replace('P24Statics.Common.CtrlKeyDown','P24ThetisMeterCompat.CtrlKeyDown')
    $text=$text.Replace('P24Statics.Common.ShiftKeyDown','P24ThetisMeterCompat.ShiftKeyDown')
    $text=$text.Replace('P24Statics.Common.ColourToString','P24ThetisMeterCompat.ColourToString')
    $text=$text.Replace('P24Statics.Common.ColourFromString','P24ThetisMeterCompat.ColourFromString')
    $text=$text.Replace('P24Statics.Common.FiveDigitHash','P24ThetisMeterCompat.FiveDigitHash')

    # PowerSDR does not expose the Thetis touch switch. Mouse behaviour remains native.
    $text=$text.Replace('_console.TouchSupport','P24ThetisMeterCompat.TouchSupport(_console)')

    # Thetis event bus members do not exist in KE9NS. Runtime state will be bridged separately.
    $text=[regex]::Replace($text,'(?m)^\s*_console\.MoxChangeHandlers\s*[+-]=.*?;\s*$','')
    $text=[regex]::Replace($text,'(?m)^\s*_console\.WindowStateChangedHandlers\s*[+-]=.*?;\s*$','')
    $text=[regex]::Replace($text,'(?m)^\s*_console\.RX2EnabledChangedHandlers\s*[+-]=.*?;\s*

    # Main-window delta members are Thetis-only presentation offsets.
    $text=$text.Replace('_console.HDelta','0')
    $text=$text.Replace('_console.VDelta','0')
    $text=$text.Replace('Display.AdaptorInfo','P24DisplayAdaptorInfo')

    [IO.File]::WriteAllText($dst,$text,$utf8Bom)
}

Copy-Item (Join-Path $PSScriptRoot 'P24MeterResources.cs') (Join-Path $consoleDir 'P24MeterResources.cs') -Force
Copy-Item (Join-Path $PSScriptRoot 'P24ThetisMeterCompat.cs') (Join-Path $consoleDir 'P24ThetisMeterCompat.cs') -Force
Copy-Item (Join-Path $PSScriptRoot 'P24ThetisTypeCompat.cs') (Join-Path $consoleDir 'P24ThetisTypeCompat.cs') -Force
Copy-Item (Join-Path $PSScriptRoot 'P24DynamicBridge.cs') (Join-Path $consoleDir 'P24DynamicBridge.cs') -Force

# Exact Thetis container chrome icons from the audited commit.
$resSrc=Join-Path $ThetisRoot 'Project Files\Source\Console\Resources'
$resDst=Join-Path $consoleDir 'P24ThetisMeterResources'
New-Item -ItemType Directory -Force -Path $resDst | Out-Null
$icons=New-Object 'System.Collections.Generic.List[string]'
foreach($resourceName in $resourceNames){
    $candidate=$resourceName+'.png'
    $p=Join-Path $resSrc $candidate
    if(Test-Path $p){
        $icons.Add($candidate)
        Copy-Item $p (Join-Path $resDst $candidate) -Force
    }
}
# Container chrome must always be exact, even if the compiler optimises a resource path away.
foreach($candidate in @('gear.png','pin_not_on_top.png','pin_on_top.png','dot.png','dockIcon_dock.png','dockIcon_float.png','resizegrab.png','arrow_left.png','arrow_topleft.png','arrow_up.png','arrow_topright.png','arrow_right.png','arrow_bottomright.png','down.png','arrow_bottomleft.png')){
    if(!$icons.Contains($candidate)){
        $p=Join-Path $resSrc $candidate
        if(!(Test-Path $p)){throw "P24 exact Thetis resource missing: $candidate"}
        $icons.Add($candidate)
        Copy-Item $p (Join-Path $resDst $candidate) -Force
    }
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
 @('Svg','3.4.7'),
 @('System.Collections.Immutable','9.0.0')
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
$compileFiles=@('P24MeterResources.cs','P24ThetisMeterCompat.cs','P24ThetisTypeCompat.cs','P24DynamicBridge.cs')
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
 @('Svg','Svg.3.4.7\lib\net472\Svg.dll'),
 @('System.Collections.Immutable','System.Collections.Immutable.9.0.0\lib\net462\System.Collections.Immutable.dll')
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

if($project -notmatch '<Reference Include="Microsoft.CSharp"'){
    $project=$refAnchor.Replace($project,'    <Reference Include="Microsoft.CSharp" />'+$nl+'$1',1)
}
[IO.File]::WriteAllText($projectCs,$project,$utf8Bom)

Stage 'Original Thetis meter sources + Direct2D/Roslyn dependencies staged'
,'')
    $text=[regex]::Replace($text,'(?m)^\s*_console\.[A-Za-z0-9_]*Handlers\s*[+-]=.*?;\s*

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
,'')
    $text=[regex]::Replace($text,'(?m)^\s*P24Statics\.ThetisBotDiscord\.[A-Za-z0-9_]*Handlers\s*[+-]=.*?;\s*

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
,'')

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
