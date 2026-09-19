[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P19-SCREEN-BEFORE-AUDIO] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $consoleCs)){ throw "P19 input missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13)+([string][char]10)
$lf = [string][char]10
$console=[IO.File]::ReadAllText($consoleCs).Replace($crlf,$lf)

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace($crlf,$lf)
    $New=$New.Replace($crlf,$lf)
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P19 anchor missing: $Label" }
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){ throw "P19 anchor not unique: $Label" }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

# P18 already installs the presentation gate immediately after InitializeComponent.
# P19 adds one post-reveal callback which performs native AutoStart only after
# the stable main form has actually been exposed to the operator.
$console = Replace-ExactOnce $console @'
            SQ4KOUStartupPresentation.Install(this, picDisplay);
'@ @'
            SQ4KOUStartupPresentation.Install(this, picDisplay, delegate
            {
                if (setupForm != null &&
                    setupForm.chkBoxAutoStart.Checked == true &&
                    chkPower.Checked == false)
                {
                    long sq4kouAutoStartAfterReveal = SQ4KOUUIDiagnostics.OperationBegin(
                        "STARTUP",
                        "AUTOSTART_POWER_ON",
                        "phase=after_reveal");

                    chkPower.Checked = true;

                    SQ4KOUUIDiagnostics.OperationEnd(
                        sq4kouAutoStartAfterReveal,
                        "STARTUP",
                        "AUTOSTART_POWER_ON",
                        "phase=after_reveal;power=" + chkPower.Checked);
                }
            });
'@ 'P18 presentation callback'

# Remove only the constructor-time automatic POWER ON. Keep the user's
# AutoStart preference intact and leave manual POWER semantics untouched.
$console = Replace-ExactOnce $console @'
                if (setupForm.chkBoxAutoStart.Checked == true)
                {
                    long sq4kouAutoStart = SQ4KOUUIDiagnostics.OperationBegin("STARTUP", "AUTOSTART_POWER_ON", null);
                    chkPower.Checked = true;
                    SQ4KOUUIDiagnostics.OperationEnd(sq4kouAutoStart, "STARTUP", "AUTOSTART_POWER_ON", "power=" + chkPower.Checked);
                }
'@ @'
                if (setupForm.chkBoxAutoStart.Checked == true)
                {
                    SQ4KOUUIDiagnostics.Mark(
                        "STARTUP",
                        "AUTOSTART_DEFERRED_UNTIL_UI_REVEAL",
                        "power=" + chkPower.Checked);
                }
'@ 'constructor AutoStart deferral'

[IO.File]::WriteAllText($consoleCs,$console.Replace($lf,$crlf),$utf8)

Stage 'PASS: native AutoStart is deferred until the P18 stable UI reveal; manual POWER path unchanged'
