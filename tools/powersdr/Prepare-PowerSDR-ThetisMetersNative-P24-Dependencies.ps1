[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$SourceRoot)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$nl=[Environment]::NewLine
$pkg=Join-Path $SourceRoot 'Console\packages.config'
$proj=Join-Path $SourceRoot 'Console\PowerSDR.csproj'
$utf8=New-Object Text.UTF8Encoding($true)
$p=[IO.File]::ReadAllText($pkg)
$packages=@(
 @('SharpDX','4.2.0'),
 @('SharpDX.Desktop','4.2.0'),
 @('SharpDX.Direct2D1','4.2.0'),
 @('SharpDX.Direct3D11','4.2.0'),
 @('SharpDX.DXGI','4.2.0'),
 @('SharpDX.Mathematics','4.2.0')
)
foreach($x in $packages){
 if($p -notmatch ('id="'+[regex]::Escape($x[0])+'"')){
   $line='  <package id="'+$x[0]+'" version="'+$x[1]+'" targetFramework="net48" />'
   $p=$p.Replace('</packages>',$line+$nl+'</packages>')
 }
}
[IO.File]::WriteAllText($pkg,$p,$utf8)
$c=[IO.File]::ReadAllText($proj)
$refs=@'
    <Reference Include="SharpDX, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.4.2.0\lib\net45\SharpDX.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Desktop, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Desktop.4.2.0\lib\net45\SharpDX.Desktop.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Direct2D1, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Direct2D1.4.2.0\lib\net45\SharpDX.Direct2D1.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Direct3D11, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Direct3D11.4.2.0\lib\net45\SharpDX.Direct3D11.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.DXGI, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.DXGI.4.2.0\lib\net45\SharpDX.DXGI.dll</HintPath>
    </Reference>
    <Reference Include="SharpDX.Mathematics, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1, processorArchitecture=MSIL">
      <HintPath>..\packages\SharpDX.Mathematics.4.2.0\lib\net45\SharpDX.Mathematics.dll</HintPath>
    </Reference>
'@
if($c -notmatch '<Reference Include="SharpDX,'){
  $anchor='<Reference Include="System.Drawing">'
  $i=$c.IndexOf($anchor)
  if($i -lt 0){throw 'P24 csproj reference anchor missing'}
  $c=$c.Insert($i,$refs+$nl+'    ')
}
$c=$c.Replace('<DefineConstants>NO_WIDETX;NO_KE9NS;NO_DJ;</DefineConstants>',
              '<DefineConstants>NO_WIDETX;NO_KE9NS;NO_DJ;</DefineConstants>'+$nl+'    <LangVersion>latest</LangVersion>')
[IO.File]::WriteAllText($proj,$c,$utf8)
Write-Host 'P24_NATIVE_DEPS=SHARPDX_4.2.0'
