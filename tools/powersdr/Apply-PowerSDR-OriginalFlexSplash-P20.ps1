[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P20-ORIGINAL-FLEX-SPLASH] $s" }

$splashCs       = Join-Path $SourceRoot 'Console\splash.cs'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$splashResx     = Join-Path $SourceRoot 'Console\splash.resx'
$consoleCs      = Join-Path $SourceRoot 'Console\console.cs'
$presentationCs = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'
$referenceResx  = Join-Path $PSScriptRoot 'resources\FlexRadio-PowerSDR-2.7.2-original-splash.resx'

foreach($p in @($splashCs,$splashDesigner,$splashResx,$consoleCs,$presentationCs,$referenceResx)){
    if(!(Test-Path -LiteralPath $p)){ throw "P20 original Flex splash input missing: $p" }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$crlf = ([string][char]13)+([string][char]10)
$lf = [string][char]10

function Read-Normal([string]$p){ return [IO.File]::ReadAllText($p).Replace($crlf,$lf) }
function Write-Normal([string]$p,[string]$s){ [IO.File]::WriteAllText($p,$s.Replace($lf,$crlf),$utf8) }

function Replace-ExactOnce([string]$Text,[string]$Old,[string]$New,[string]$Label){
    $Old=$Old.Replace($crlf,$lf)
    $New=$New.Replace($crlf,$lf)
    $first=$Text.IndexOf($Old,[StringComparison]::Ordinal)
    if($first -lt 0){ throw "P20 original Flex splash anchor missing: $Label" }
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){ throw "P20 original Flex splash anchor not unique: $Label" }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

$splash = Read-Normal $splashCs
$designer = Read-Normal $splashDesigner
$console = Read-Normal $consoleCs
$presentation = Read-Normal $presentationCs
$sourceResx = [IO.File]::ReadAllText($splashResx)
$refResx = [IO.File]::ReadAllText($referenceResx)

# 1) Replace only the form BackgroundImage payload with the preserved
# FlexRadio PowerSDR 2.7.2-era artwork. Keep the current icon and all unrelated
# resources from the pinned KE9NS source.
$bgRx = [regex]::new('(?s)(<data name="\$this\.BackgroundImage"[^>]*>.*?<value>)(.*?)(</value>.*?</data>)')
$refMatches = $bgRx.Matches($refResx)
$srcMatches = $bgRx.Matches($sourceResx)
if($refMatches.Count -ne 1){ throw "P20 reference BackgroundImage count=$($refMatches.Count), expected 1" }
if($srcMatches.Count -ne 1){ throw "P20 source BackgroundImage count=$($srcMatches.Count), expected 1" }

$refPayload = $refMatches[0].Groups[2].Value
$refPayloadCompact = ($refPayload -replace '\s','')
if(!$refPayloadCompact.StartsWith('iVBORw0KGgo')){ throw 'P20 reference background is not a PNG payload' }
if($refPayloadCompact.Length -lt 100000){ throw "P20 reference background unexpectedly small: $($refPayloadCompact.Length)" }

$sourceResx = $bgRx.Replace(
    $sourceResx,
    { param($m) $m.Groups[1].Value + $refPayload + $m.Groups[3].Value },
    1)

[IO.File]::WriteAllText($splashResx,$sourceResx,$utf8)

# 2) Remove the later KE9NS random Earth/Moon artwork. The form-level
# BackgroundImage now is the original FlexRadio artwork.
$splash = Replace-ExactOnce $splash @'
            Random random = new Random();  
            bool randomBool = random.Next(2) == 0;  // Returns true or false randomly

            if (randomBool) pictureBox1.Image = Properties.Resources.moonearth6;
            else pictureBox1.Image = Properties.Resources.moonearth2;


            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
           
            var pos = this.PointToScreen(lblStatus.Location);
            pos = pictureBox1.PointToClient(pos);

            lblStatus.Parent = pictureBox1;
            lblStatus.BackColor = Color.Transparent;
            lblStatus.Location = pos;

            lblTimeRemaining.Parent = pictureBox1;
            lblTimeRemaining.BackColor = Color.Transparent;

            pos = this.PointToScreen(lblTimeRemaining.Location);
            pos = pictureBox1.PointToClient(pos);

            lblTimeRemaining.Location = pos;
'@ @'
            // SQ4KOU P20: restore the original FlexRadio PowerSDR splash artwork.
            // Do not use the later KE9NS random Earth/Moon images.
            pictureBox1.Visible = false;
            lblStatus.BackColor = Color.Transparent;
            lblTimeRemaining.BackColor = Color.Transparent;
'@ 'remove KE9NS random splash artwork'

# 3) Restore the 600x384 form-level background and legacy status/progress
# geometry. Hide the KE9NS version textbox so the Flex artwork is not covered.
$designer = Replace-ExactOnce $designer @'
            this.pnlStatus.BackColor = System.Drawing.Color.Black;
            this.pnlStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.pnlStatus.Location = new System.Drawing.Point(64, 315);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(475, 2);
'@ @'
            this.pnlStatus.BackColor = System.Drawing.Color.Transparent;
            this.pnlStatus.ForeColor = System.Drawing.Color.White;
            this.pnlStatus.Location = new System.Drawing.Point(50, 247);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(475, 24);
'@ 'legacy progress geometry'

$designer = Replace-ExactOnce $designer @'
            this.textBox1.Text = "v2.8.0.0";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
'@ @'
            this.textBox1.Text = "v2.8.0.0";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.Visible = false;
'@ 'hide KE9NS version overlay'

$designer = Replace-ExactOnce $designer @'
            this.pictureBox1.Location = new System.Drawing.Point(-2, -1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(604, 384);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
'@ @'
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1, 1);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
'@ 'disable KE9NS artwork host'

$designer = Replace-ExactOnce $designer @'
            this.lblTimeRemaining.Location = new System.Drawing.Point(457, 298);
            this.lblTimeRemaining.Name = "lblTimeRemaining";
            this.lblTimeRemaining.Size = new System.Drawing.Size(100, 14);
'@ @'
            this.lblTimeRemaining.Location = new System.Drawing.Point(296, 289);
            this.lblTimeRemaining.Name = "lblTimeRemaining";
            this.lblTimeRemaining.Size = new System.Drawing.Size(100, 16);
'@ 'legacy time geometry'

$designer = Replace-ExactOnce $designer @'
            this.lblStatus.Location = new System.Drawing.Point(12, 296);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(400, 16);
'@ @'
            this.lblStatus.Location = new System.Drawing.Point(0, 287);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(400, 16);
'@ 'legacy status geometry'

$designer = Replace-ExactOnce $designer @'
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblTimeRemaining = new System.Windows.Forms.LabelTS();
            this.lblStatus = new System.Windows.Forms.LabelTS();
'@ @'
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblTimeRemaining = new System.Windows.Forms.LabelTS();
            this.lblStatus = new System.Windows.Forms.LabelTS();
            this.lblCallsign = new System.Windows.Forms.Label();
'@ 'callsign construct'

$designer = Replace-ExactOnce $designer @'
            // 
            // Splash
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(600, 384);
            this.Controls.Add(this.textBox1);
'@ @'
            // 
            // lblCallsign
            // 
            this.lblCallsign.BackColor = System.Drawing.Color.Transparent;
            this.lblCallsign.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCallsign.ForeColor = System.Drawing.Color.White;
            this.lblCallsign.Location = new System.Drawing.Point(468, 350);
            this.lblCallsign.Name = "lblCallsign";
            this.lblCallsign.Size = new System.Drawing.Size(112, 22);
            this.lblCallsign.TabIndex = 5;
            this.lblCallsign.Text = "SQ4KOU";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Splash
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(600, 384);
            this.Controls.Add(this.lblCallsign);
            this.Controls.Add(this.textBox1);
'@ 'form background and callsign'

$designer = Replace-ExactOnce $designer @'
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pictureBox1;
'@ @'
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblCallsign;
'@ 'callsign field'

# 4) Preserve the P19 clean transition: keep the splash visible while the
# transparent main form performs first layout/paint, then close the splash at
# the exact stable reveal. Native AutoStart/audio remains deferred by P19.
$closeNeedle = 'Splash.CloseForm();'
$closeCount = ([regex]::Matches($console,[regex]::Escape($closeNeedle))).Count
if($closeCount -ne 1){ throw "P20 expected exactly one constructor Splash.CloseForm(), found $closeCount" }
$console = $console.Replace(
    $closeNeedle,
    'Splash.SetStatus("Finalizing Main Window"); // P20: retain original Flex splash until stable UI reveal')

$presentation = Replace-ExactOnce $presentation @'
                    form.Opacity = 1.0;
                    form.Activate();
                    form.Update();
'@ @'
                    form.Opacity = 1.0;
                    form.Activate();
                    form.Update();

                    SQ4KOUUIDiagnostics.Mark("STARTUP", "FLEX_SPLASH_CLOSE_AT_REVEAL", null);
                    Splash.CloseForm();
'@ 'close original Flex splash at reveal'

Write-Normal $splashCs $splash
Write-Normal $splashDesigner $designer
Write-Normal $consoleCs $console
Write-Normal $presentationCs $presentation

Stage 'PASS: original FlexRadio PowerSDR splash artwork + SQ4KOU, retained until stable main-window reveal'
