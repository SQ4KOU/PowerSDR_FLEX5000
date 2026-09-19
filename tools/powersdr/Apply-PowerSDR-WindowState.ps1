[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-WINDOW] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $consoleCs)) { throw "PowerSDR console source missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$text = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "Window-state anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "Window-state anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

$oldSave = @'
            a.Add("console_zaximize/" + this.WindowState.ToString()); // ke9ns add: for max detection

            a.Add("console_top/" + this.Top.ToString());                    // save form positions
            a.Add("console_left/" + this.Left.ToString());
            a.Add("console_width/" + this.Width.ToString());
            a.Add("console_height/" + this.Height.ToString());
'@

$newSave = @'
            // Preserve both the actual window state and the NORMAL restore rectangle.
            // Reading Top/Left/Width/Height while maximized stores the monitor work area,
            // not the user's restored window rectangle.
            Rectangle consoleBoundsToSave =
                (this.WindowState == FormWindowState.Normal) ? this.Bounds : this.RestoreBounds;

            a.Add("console_zaximize/" +
                (this.WindowState == FormWindowState.Maximized ? "Maximized" : "Normal"));

            a.Add("console_top/" + consoleBoundsToSave.Top.ToString());      // save normal restore position
            a.Add("console_left/" + consoleBoundsToSave.Left.ToString());
            a.Add("console_width/" + consoleBoundsToSave.Width.ToString());
            a.Add("console_height/" + consoleBoundsToSave.Height.ToString());
'@

$oldRestore = @'
            if (CONSOLEM == true)
            {
                //  Debug.WriteLine("MAXIMUM2" + CONSOLEL);
                this.Top = CONSOLET;
                this.Left = CONSOLEL;
                this.Width = CONSOLEW;
                this.Height = CONSOLEH;

                //  this.WindowState = FormWindowState.Normal;


                //   this.WindowState = FormWindowState.Maximized;


                //  Debug.WriteLine("MAXIMUM2");
            }
'@

$newRestore = @'
            if (CONSOLEM == true)
            {
                // Restore the last NORMAL rectangle first, then return to maximized state.
                // This also preserves a useful restore size when the user later clicks Restore.
                this.Top = CONSOLET;
                this.Left = CONSOLEL;
                this.Width = CONSOLEW;
                this.Height = CONSOLEH;
                this.WindowState = FormWindowState.Maximized;
            }
'@

$text = Replace-ExactOnce $text $oldSave $newSave 'SaveState maximized/restore bounds'
$text = Replace-ExactOnce $text $oldRestore $newRestore 'startup maximized restore'

$checks = @(
    @{ Name='RestoreBounds persisted'; Ok=$text.Contains('this.RestoreBounds') -and $text.Contains('consoleBoundsToSave') },
    @{ Name='Maximized explicitly restored'; Ok=$text.Contains('this.WindowState = FormWindowState.Maximized;') },
    @{ Name='legacy DB key preserved'; Ok=$text.Contains('console_zaximize/') }
)
$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('Window-state post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs, $text.Replace("`n", "`r`n"), $utf8)
Stage 'PASS: maximized state and normal RestoreBounds are persisted/restored'
