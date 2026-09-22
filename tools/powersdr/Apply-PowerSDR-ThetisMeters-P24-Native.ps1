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

    # Keep original implementation but move it into the PowerSDR namespace.
    $text=$text.Replace('namespace Thetis','namespace PowerSDR')

    # Preserve original Thetis bitmap resources byte-for-byte, resolved from an
    # output folder instead of importing the entire Thetis Resources.resx.
    $resourceRx=[regex]'(?:(?:global::)?Thetis\.)?Properties\.Resources\.([A-Za-z0-9_]+)'
    foreach($m in $resourceRx.Matches($text)){[void]$resourceNames.Add($m.Groups[1].Value)}
    $text=$resourceRx.Replace($text,[System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        return 'global::PowerSDR.P24MeterResources.Get("' + $m.Groups[1].Value + '")'
    })

    # Type-only incompatibility must be handled before the generic Display facade.
    $text=$text.Replace('Display.AdaptorInfo','P24DisplayAdaptorInfo')

    # Route all Thetis.Console access through one facade. Do not mutate the real
    # PowerSDR Console class with hundreds of Thetis-only members.
    if($name -in @('MeterManager.cs','clsLegacyItemController.cs','frmMeterDisplay.cs','ucMeter.cs')){
        $text=[regex]::Replace($text,'\bConsole\s+(?=(?:_console|c|console)\b)','dynamic ')
        $text=$text.Replace('public Console Console','public dynamic Console')
        $text=$text.Replace('_console = c;','_console = P24ConsoleDynamic.WrapObject(c);')
        $text=$text.Replace('_console = console;','_console = P24ConsoleDynamic.WrapObject(console);')
        $text=$text.Replace('_console = value;','_console = P24ConsoleDynamic.WrapObject(value);')
    }

    # Static Thetis service classes are isolated behind reflective facades. Existing
    # native PowerSDR members are forwarded; Thetis-only members are contained here.
    foreach($staticName in @(
        'Display','Common','BandStackManager','ThetisBotDiscord',
        'HardwareSpecific','MNotchDB','SpecHPSDRDLL','Alex','ColorInterpolator'
    )){
        $pattern='(?<![A-Za-z0-9_\.])'+[regex]::Escape($staticName)+'\.'
        $text=[regex]::Replace($text,$pattern,'P24Statics.'+$staticName+'.')
    }

    # Exact helper semantics that are simple and UI-local.
    $text=$text.Replace('P24Statics.Common.DoubleBufferAll','P24ThetisMeterCompat.DoubleBufferAll')
    $text=$text.Replace('P24Statics.Common.CtrlKeyDown','P24ThetisMeterCompat.CtrlKeyDown')
    $text=$text.Replace('P24Statics.Common.ShiftKeyDown','P24ThetisMeterCompat.ShiftKeyDown')
    $text=$text.Replace('P24Statics.Common.ColourToString','P24ThetisMeterCompat.ColourToString')
    $text=$text.Replace('P24Statics.Common.ColourFromString','P24ThetisMeterCompat.ColourFromString')
    $text=$text.Replace('P24Statics.Common.FiveDigitHash','P24ThetisMeterCompat.FiveDigitHash')
    $text=$text.Replace('P24Statics.Common.ForceFormOnScreen','P24ThetisMeterCompat.ForceFormOnScreen')
    $text=$text.Replace('_console.GetXPAStatus()','P24ThetisMeterCompat.GetXPAStatus((object)_console)')
    $text=$text.Replace('_console.specRX.GetSpecRX(_id)','P24ThetisMeterCompat.GetSpectrumSpec((object)_console, _id)')
    $text=$text.Replace('P24Statics.SpecHPSDRDLL.','P24SpecHPSDRDLL.')
    $text=$text.Replace('cmaster.','P24CMaster.')
    $text=$text.Replace('.Selectable = true;','.TabStop = true;')
    $text=$text.Replace('.Selectable = false;','.TabStop = false;')
    $text=$text.Replace('case DisplayMode.SCOPE2:', 'case (DisplayMode)(-1002):')
    $text=$text.Replace('case DisplayMode.SPECTRASCOPE:', 'case (DisplayMode)(-1003):')
    $text=$text.Replace('HiPerfTimer','P24HiPerfTimer')

    # PowerSDR has no Thetis touch switch. Mouse behaviour remains original.
    $text=$text.Replace('_console.TouchSupport','P24ThetisMeterCompat.TouchSupport(_console)')
    $text=$text.Replace('Display.AdaptorInfo','P24AdaptorInfo')

    # Event buses are Thetis.Console implementation details. P24 mirrors native
    # PowerSDR state through typed adapters/polling; do not bind dynamic events.
    $text=[regex]::Replace($text,'(?m)^\s*_console\.[A-Za-z0-9_]*(?:Handlers|Handers)\s*[+-]=.*?;\s*$','')
    $text=[regex]::Replace($text,'(?m)^\s*_console\.ARP\.[A-Za-z0-9_]+\s*[+-]=.*?;\s*$','')
    $text=[regex]::Replace($text,'(?m)^\s*P24Statics\.ThetisBotDiscord\.[A-Za-z0-9_]*Handlers\s*[+-]=.*?;\s*$','')
    $text=$text.Replace('_console.ARP.StopPlayback(out _);','P24ThetisMeterCompat.StopPlayback((object)_console);')
    $text=$text.Replace('_console.ARP.StopRecord(out _);','P24ThetisMeterCompat.StopRecord((object)_console);')

    # Thetis main-window presentation deltas do not exist in KE9NS.
    $text=$text.Replace('_console.HDelta','0')
    $text=$text.Replace('_console.VDelta','0')

    [IO.File]::WriteAllText($dst,$text,$utf8Bom)
}

foreach($helper in @(
    'P24MeterResources.cs',
    'P24ThetisMeterCompat.cs',
    'P24ThetisTypeCompat.cs',
    'P24DynamicBridge.cs',
    'P24OtherButtonHelpers.cs',
    'P24SetupNativeMeters.cs',
 'P24SetupCompat.cs',
    'P24ThetisMetersRuntime.cs',
    'P24_ucSignalSelect.cs',
    'P24_ucSignalSelect.Designer.cs',
    'P24_ucOtherButtonsOptionsGrid.cs',
    'P24_ucOtherButtonsOptionsGrid.Designer.cs',
    'P24_ucTunestepOptionsGrid.cs',
    'P24_ucTunestepOptionsGrid.Designer.cs'
)){
    $src=Join-Path $PSScriptRoot $helper
    if(!(Test-Path $src)){throw "P24 helper missing: $src"}
    Copy-Item $src (Join-Path $consoleDir $helper) -Force
}

# Preserve the original WinForms .resx payloads for every imported Thetis
# control/form. ComponentResourceManager(typeof(T)) resolves the neutral resource
# by the runtime type name (PowerSDR.T), so the manifest names must match exactly.
$thetisConsoleDir=Join-Path $ThetisRoot 'Project Files\Source\Console'
$p24Resx=@(
    @('ucSignalSelect.resx','PowerSDR.ucSignalSelect.resources'),
    @('ucOtherButtonsOptionsGrid.resx','PowerSDR.ucOtherButtonsOptionsGrid.resources'),
    @('ucTunestepOptionsGrid.resx','PowerSDR.ucTunestepOptionsGrid.resources'),
    @('ucMeter.resx','PowerSDR.ucMeter.resources'),
    @('frmMeterDisplay.resx','PowerSDR.frmMeterDisplay.resources'),
    @('frmVariablePicker.resx','PowerSDR.frmVariablePicker.resources')
)
foreach($res in $p24Resx){
    $name=$res[0]
    $source=Join-Path $thetisConsoleDir $name
    $dest=Join-Path $consoleDir $name
    if(!(Test-Path $source)){throw "P24 Thetis WinForms resource missing: $source"}
    Copy-Item $source $dest -Force
}

# Copy every exact Thetis PNG used by the compiled subsystem.
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

# Container chrome is mandatory and must be the exact Thetis artwork.
foreach($candidate in @(
 'gear.png','pin_not_on_top.png','pin_on_top.png','dot.png',
 'dockIcon_dock.png','dockIcon_float.png','resizegrab.png',
 'arrow_left.png','arrow_topleft.png','arrow_up.png','arrow_topright.png',
 'arrow_right.png','arrow_bottomright.png','down.png','arrow_bottomleft.png',
 'arrow_left_black.png','arrow_right_black.png','arrow_up_black.png','down_black.png',
 'brush32border.png','pipette32border.png','cont_copy.png','cont_load.png','cont_save.png',
 'copy.png','grid.png'
)){
    if(!$icons.Contains($candidate)){
        $p=Join-Path $resSrc $candidate
        if(!(Test-Path $p)){throw "P24 exact Thetis resource missing: $candidate"}
        $icons.Add($candidate)
        Copy-Item $p (Join-Path $resDst $candidate) -Force
    }
}

# Dependencies used by the original Thetis Direct2D renderer, script engine and web-image item.
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
 @('System.Collections.Immutable','9.0.0'),
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

# Embed the copied WinForms resources under the exact runtime names expected by
# ComponentResourceManager after the namespace move Thetis -> PowerSDR.
$resourceAnchor=[regex]'(<EmbeddedResource Include="helpbox1\.resx">)'
if($resourceAnchor.Matches($project).Count -ne 1){throw 'P24 EmbeddedResource anchor invalid'}
foreach($res in $p24Resx){
    $name=$res[0]
    $logical=$res[1]
    if($project -notmatch ('<EmbeddedResource Include="'+[regex]::Escape($name)+'"')){
        $entry='    <EmbeddedResource Include="'+$name+'">'+$nl+
               '      <LogicalName>'+$logical+'</LogicalName>'+$nl+
               '      <SubType>Designer</SubType>'+$nl+
               '    </EmbeddedResource>'
        $project=$resourceAnchor.Replace($project,$entry+$nl+'$1',1)
    }
}

# Compile original transformed subsystem plus compatibility layer.
$compileAnchor=[regex]'(<Compile Include="Skin\.cs"\s*/>)'
if($compileAnchor.Matches($project).Count -ne 1){throw 'P24 csproj Skin.cs anchor invalid'}
$compileFiles=@(
 'P24MeterResources.cs',
 'P24ThetisMeterCompat.cs',
 'P24ThetisTypeCompat.cs',
 'P24DynamicBridge.cs',
 'P24OtherButtonHelpers.cs',
 'P24SetupNativeMeters.cs',
 'P24ThetisMetersRuntime.cs',
 'P24_ucSignalSelect.cs',
 'P24_ucSignalSelect.Designer.cs',
 'P24_ucOtherButtonsOptionsGrid.cs',
 'P24_ucOtherButtonsOptionsGrid.Designer.cs',
 'P24_ucTunestepOptionsGrid.cs',
 'P24_ucTunestepOptionsGrid.Designer.cs'
)
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

# Add only the required assembly references. Do not replace the native PowerSDR DSP/backend.
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
 @('System.Collections.Immutable','System.Collections.Immutable.9.0.0\lib\net462\System.Collections.Immutable.dll'),
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
if($project -notmatch '<Reference Include="Microsoft\.CSharp"'){
    $project=$refAnchor.Replace($project,'    <Reference Include="Microsoft.CSharp" />'+$nl+'$1',1)
}

[IO.File]::WriteAllText($projectCs,$project,$utf8Bom)

# Bootstrap the original Thetis meter runtime and exact Meters/Gadgets setup page.
$setupCs=Join-Path $consoleDir 'setup.cs'
$setupText=[IO.File]::ReadAllText($setupCs)
$runtimeHook='P24ThetisMetersRuntime.Init(c);'
$restoreHook='P24ThetisMetersRuntime.RestoreFromPowerSdrOptions(a);'
$storeHook='P24ThetisMetersRuntime.StoreIntoPowerSdrOptions(a);'
$finishHook='P24ThetisMetersRuntime.FinishSetup();'
$uiHook='P24InitNativeMetersGadgets();'

# Runtime adapter: insert immediately after the native Setup.console assignment.
if(!$setupText.Contains($runtimeHook)){
    $runtimeAnchor='            console = c;'
    $runtimeIndex=$setupText.IndexOf($runtimeAnchor,[StringComparison]::Ordinal)
    if($runtimeIndex -lt 0){throw 'P24 runtime hook anchor missing'}
    $runtimeReplacement=$runtimeAnchor+$nl+'            '+$runtimeHook
    $setupText=$setupText.Replace($runtimeAnchor,$runtimeReplacement)
}

# Restore native Thetis MultiMeter state from the normal PowerSDR Options table.
# This runs inside the existing GetOptions() path, before PowerSDR processes the
# remaining UI controls.
if(!$setupText.Contains($restoreHook)){
    $restoreAnchor='            ArrayList a = DB.GetVars("Options");'
    $restoreIndex=$setupText.IndexOf($restoreAnchor,[StringComparison]::Ordinal)
    if($restoreIndex -lt 0){throw 'P24 Options restore anchor missing'}
    $restoreReplacement=$restoreAnchor+$nl+'            '+$restoreHook
    $setupText=$setupText.Replace($restoreAnchor,$restoreReplacement)
}

# Store native Thetis MultiMeter state in the same Options table as the rest of
# PowerSDR. MeterManager emits meterContData_*, meterData_*, meterIGData_* and
# meterIGSettings_2_* records; the runtime converts them to PowerSDR key/value rows.
if(!$setupText.Contains($storeHook)){
    $storeAnchor='            DB.SaveVars("Options", ref a);'
    $storeIndex=$setupText.IndexOf($storeAnchor,[StringComparison]::Ordinal)
    if($storeIndex -lt 0){throw 'P24 Options store anchor missing'}
    $storeReplacement='            '+$storeHook+$nl+$storeAnchor
    $setupText=$setupText.Replace($storeAnchor,$storeReplacement)
}

# UI integration: finalise the restored model and initialize the page once at
# the end of the native Setup constructor. This is deterministic and does not
# depend on Form.Shown.
if(!$setupText.Contains($uiHook)){
    $uiAnchor='        } // setup'
    $uiIndex=$setupText.IndexOf($uiAnchor,[StringComparison]::Ordinal)
    if($uiIndex -lt 0){throw 'P24 UI hook anchor missing'}
    $uiReplacement='            '+$finishHook+$nl+'            '+$uiHook+$nl+$uiAnchor
    $setupText=$setupText.Replace($uiAnchor,$uiReplacement)
}

[IO.File]::WriteAllText($setupCs,$setupText,$utf8Bom)

if(([regex]::Matches($setupText,[regex]::Escape($runtimeHook))).Count -ne 1){throw 'P24 runtime hook count invalid'}
if(([regex]::Matches($setupText,[regex]::Escape($restoreHook))).Count -ne 1){throw 'P24 Options restore hook count invalid'}
if(([regex]::Matches($setupText,[regex]::Escape($storeHook))).Count -ne 1){throw 'P24 Options store hook count invalid'}
if(([regex]::Matches($setupText,[regex]::Escape($finishHook))).Count -ne 1){throw 'P24 finish hook count invalid'}
if(([regex]::Matches($setupText,[regex]::Escape($uiHook))).Count -ne 1){throw 'P24 setup UI hook count invalid'}
if($setupText.Contains('this.Shown += delegate')){throw 'P24 obsolete Shown hook still present'}

Stage "Original Thetis MeterManager + Setup Meters/Gadgets staged; PowerSDR Options DB restore/store hooks active; resources=$($icons.Count)"
