[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Stage([string]$s) { Write-Host "[SQ4KOU-P10-UI-FIRST] $s" }

$consoleCs = Join-Path $SourceRoot 'Console\console.cs'
if(!(Test-Path -LiteralPath $consoleCs)) { throw "P10 source missing: $consoleCs" }

$utf8 = New-Object System.Text.UTF8Encoding($false)
$console = [IO.File]::ReadAllText($consoleCs).Replace("`r`n", "`n")

function Replace-ExactOnce([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $Old = $Old.Replace("`r`n", "`n")
    $New = $New.Replace("`r`n", "`n")
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if($first -lt 0) { throw "P10 anchor missing: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if($second -ge 0) { throw "P10 anchor not unique: $Label" }
    return $Text.Substring(0,$first) + $New + $Text.Substring($first + $Old.Length)
}

$console = Replace-ExactOnce $console @'
        #region Constructor and Destructor
        // ======================================================
        // Constructor and Destructor
        // ======================================================
        public Console(string[] args)
'@ @'
        #region Constructor and Destructor
        // ======================================================
        // Constructor and Destructor
        // ======================================================

        // SQ4KOU P10:
        // AutoStart must not power the radio from inside the Form constructor.
        // Until Application.Run() starts, WinForms has no normal message pump,
        // so Audio/DSP can already be live while the main window still cannot paint.
        private bool sq4kouDeferredAutoStart = false;
        private bool sq4kouShownOnce = false;

        public Console(string[] args)
'@ 'deferred autostart fields'

$console = Replace-ExactOnce $console @'
                if (s == "-autostart")
                    chkPower.Checked = true;
'@ @'
                if (s == "-autostart")
                    sq4kouDeferredAutoStart = true;
'@ 'command line autostart defer'

$console = Replace-ExactOnce $console @'
                if (setupForm.chkBoxAutoStart.Checked == true)
                {
                    chkPower.Checked = true;
                }
'@ @'
                if (setupForm.chkBoxAutoStart.Checked == true)
                {
                    sq4kouDeferredAutoStart = true;
                }
'@ 'setup autostart defer'

$console = Replace-ExactOnce $console @'
            initializing = false; //.242 moved down here

        } // public console


        //    protected override void OnClosed(EventArgs e)
'@ @'
            initializing = false; //.242 moved down here

        } // public console


        // SQ4KOU P10: let the main window enter the WinForms message loop first.
        // BeginInvoke posts AutoStart behind the Shown cycle so the constructor
        // is completely finished and the form can paint before Audio.Start().
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (sq4kouShownOnce) return;
            sq4kouShownOnce = true;

            try
            {
                File.AppendAllText(
                    app_data_path + "startup_p10.log",
                    "SQ4KOU_MAINFORM_SHOWN_UTC=" + DateTime.UtcNow.ToString("O") + Environment.NewLine +
                    "SQ4KOU_AUTOSTART_PENDING=" + sq4kouDeferredAutoStart + Environment.NewLine);
            }
            catch { }

            if (sq4kouDeferredAutoStart)
            {
                BeginInvoke(new MethodInvoker(delegate
                {
                    if (IsDisposed || Disposing) return;
                    if (chkPower.Checked) return;

                    try
                    {
                        File.AppendAllText(
                            app_data_path + "startup_p10.log",
                            "SQ4KOU_AUTOSTART_BEGIN_UTC=" + DateTime.UtcNow.ToString("O") + Environment.NewLine);
                    }
                    catch { }

                    chkPower.Checked = true;

                    try
                    {
                        File.AppendAllText(
                            app_data_path + "startup_p10.log",
                            "SQ4KOU_AUTOSTART_END_UTC=" + DateTime.UtcNow.ToString("O") + Environment.NewLine);
                    }
                    catch { }
                }));
            }
        }


        //    protected override void OnClosed(EventArgs e)
'@ 'OnShown deferred autostart'

$checks = @(
    @{Name='deferred flag'; Ok=$console.Contains('private bool sq4kouDeferredAutoStart = false;')},
    @{Name='shown override'; Ok=$console.Contains('protected override void OnShown(EventArgs e)')},
    @{Name='begininvoke'; Ok=$console.Contains('BeginInvoke(new MethodInvoker(delegate')},
    @{Name='startup log'; Ok=$console.Contains('startup_p10.log')}
)

$failed = @($checks | Where-Object { -not $_.Ok })
if($failed.Count -gt 0) {
    throw ('P10 post-check failed: ' + (($failed | ForEach-Object { $_.Name }) -join ', '))
}

[IO.File]::WriteAllText($consoleCs, $console.Replace("`n", "`r`n"), $utf8)

Stage 'PASS: main WinForms window is shown before deferred AutoStart powers the radio'
