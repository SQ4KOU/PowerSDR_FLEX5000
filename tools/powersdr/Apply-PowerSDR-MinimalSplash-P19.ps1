[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$SourceRoot
)

$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest

function Stage([string]$s){ Write-Host "[SQ4KOU-P19-MINIMAL-SPLASH] $s" }

$splashCs       = Join-Path $SourceRoot 'Console\splash.cs'
$splashDesigner = Join-Path $SourceRoot 'Console\splash.Designer.cs'
$consoleCs      = Join-Path $SourceRoot 'Console\console.cs'
$presentationCs = Join-Path $SourceRoot 'Console\SQ4KOUStartupPresentation.cs'

foreach($p in @($splashCs,$splashDesigner,$consoleCs,$presentationCs)){
    if(!(Test-Path -LiteralPath $p)){ throw "P19 minimal splash input missing: $p" }
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
    if($first -lt 0){ throw "P19 minimal splash anchor missing: $Label" }
    $second=$Text.IndexOf($Old,$first+$Old.Length,[StringComparison]::Ordinal)
    if($second -ge 0){ throw "P19 minimal splash anchor not unique: $Label" }
    return $Text.Substring(0,$first)+$New+$Text.Substring($first+$Old.Length)
}

$splash = Read-Normal $splashCs
$designer = Read-Normal $splashDesigner
$console = Read-Normal $consoleCs
$presentation = Read-Normal $presentationCs

# 1) Remove the old random Earth/Moon artwork and legacy countdown setup.
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
            // SQ4KOU P19: minimalist splash. No random artwork, countdown
            // or image-parented controls. Keep the startup status and progress only.
            pictureBox1.Visible = false;
            lblTimeRemaining.Visible = false;
            lblStatus.BackColor = Color.Transparent;
'@ 'remove legacy splash artwork'

# 2) Replace legacy designer with a compact, deterministic presentation.
$designer = Replace-ExactOnce $designer @'
            this.pnlStatus.BackColor = System.Drawing.Color.Black;
            this.pnlStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.pnlStatus.Location = new System.Drawing.Point(64, 315);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(475, 2);
'@ @'
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(48, 48, 52);
            this.pnlStatus.ForeColor = System.Drawing.Color.White;
            this.pnlStatus.Location = new System.Drawing.Point(30, 178);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(460, 2);
'@ 'progress geometry'

$designer = Replace-ExactOnce $designer @'
            this.textBox1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.textBox1.Location = new System.Drawing.Point(218, 155);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(133, 24);
            this.textBox1.TabIndex = 3;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "v2.8.0.0";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
'@ @'
            this.textBox1.BackColor = System.Drawing.Color.FromArgb(24, 24, 27);
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.FromArgb(165, 165, 170);
            this.textBox1.Location = new System.Drawing.Point(30, 91);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(460, 22);
            this.textBox1.TabIndex = 3;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "v2.8.0.0";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
'@ 'version geometry'

$designer = Replace-ExactOnce $designer @'
            this.pictureBox1.Location = new System.Drawing.Point(-2, -1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(604, 384);
'@ @'
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1, 1);
            this.pictureBox1.Visible = false;
'@ 'picture hidden'

$designer = Replace-ExactOnce $designer @'
            this.lblTimeRemaining.BackColor = System.Drawing.Color.Transparent;
            this.lblTimeRemaining.ForeColor = System.Drawing.Color.White;
            this.lblTimeRemaining.Image = null;
            this.lblTimeRemaining.Location = new System.Drawing.Point(457, 298);
            this.lblTimeRemaining.Name = "lblTimeRemaining";
            this.lblTimeRemaining.Size = new System.Drawing.Size(100, 14);
            this.lblTimeRemaining.TabIndex = 1;
            this.lblTimeRemaining.Text = "Time";
'@ @'
            this.lblTimeRemaining.BackColor = System.Drawing.Color.Transparent;
            this.lblTimeRemaining.ForeColor = System.Drawing.Color.Transparent;
            this.lblTimeRemaining.Image = null;
            this.lblTimeRemaining.Location = new System.Drawing.Point(0, 0);
            this.lblTimeRemaining.Name = "lblTimeRemaining";
            this.lblTimeRemaining.Size = new System.Drawing.Size(1, 1);
            this.lblTimeRemaining.TabIndex = 1;
            this.lblTimeRemaining.Text = "";
            this.lblTimeRemaining.Visible = false;
'@ 'countdown hidden'

$designer = Replace-ExactOnce $designer @'
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Image = null;
            this.lblStatus.Location = new System.Drawing.Point(12, 296);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(400, 16);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Status";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
'@ @'
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(185, 185, 190);
            this.lblStatus.Image = null;
            this.lblStatus.Location = new System.Drawing.Point(30, 148);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(460, 20);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Starting...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
'@ 'status geometry'

# Add one product label without adding any new asset.
$designer = Replace-ExactOnce $designer @'
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblTimeRemaining = new System.Windows.Forms.LabelTS();
            this.lblStatus = new System.Windows.Forms.LabelTS();
'@ @'
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblTimeRemaining = new System.Windows.Forms.LabelTS();
            this.lblStatus = new System.Windows.Forms.LabelTS();
            this.lblProduct = new System.Windows.Forms.Label();
'@ 'product label construct'

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
            // lblProduct
            // 
            this.lblProduct.AutoSize = false;
            this.lblProduct.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProduct.ForeColor = System.Drawing.Color.White;
            this.lblProduct.Location = new System.Drawing.Point(28, 31);
            this.lblProduct.Name = "lblProduct";
            this.lblProduct.Size = new System.Drawing.Size(462, 50);
            this.lblProduct.TabIndex = 5;
            this.lblProduct.Text = "PowerSDR  •  FLEX-5000";
            this.lblProduct.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Splash
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.FromArgb(24, 24, 27);
            this.BackgroundImage = null;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(520, 210);
            this.Controls.Add(this.lblProduct);
            this.Controls.Add(this.textBox1);
'@ 'form minimalist layout'

$designer = Replace-ExactOnce $designer @'
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pictureBox1;
'@ @'
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblProduct;
'@ 'product label field'

# 3) Do not close splash in the constructor. That created the blank gap while
# P18 was intentionally keeping the main form transparent.
$closeNeedle = 'Splash.CloseForm();'
$closeCount = ([regex]::Matches($console,[regex]::Escape($closeNeedle))).Count
if($closeCount -ne 1){ throw "P19 expected exactly one constructor Splash.CloseForm(), found $closeCount" }
$console = $console.Replace(
    $closeNeedle,
    'Splash.SetStatus("Finalizing Main Window"); // P19: keep splash visible until stable UI reveal')

# 4) At the exact transition to the stable main UI, reveal it and then fade
# the splash away. The main form is already painted underneath, so no blank gap.
$presentation = Replace-ExactOnce $presentation @'
                    form.Opacity = 1.0;
                    form.Activate();
                    form.Update();
'@ @'
                    form.Opacity = 1.0;
                    form.Activate();
                    form.Update();

                    SQ4KOUUIDiagnostics.Mark("STARTUP", "SPLASH_CLOSE_AT_REVEAL", null);
                    Splash.CloseForm();
'@ 'close splash at reveal'

Write-Normal $splashCs $splash
Write-Normal $splashDesigner $designer
Write-Normal $consoleCs $console
Write-Normal $presentationCs $presentation

Stage 'PASS: minimalist splash installed and lifetime tied to stable main-window reveal'
