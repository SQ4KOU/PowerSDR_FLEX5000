from pathlib import Path
import sys

root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
path = root / "Project Files/Source/Console/setup.cs"
text = path.read_text(encoding="utf-8-sig")

sig = "        private void InitWaterfallQualityControls()\n"
start = text.find(sig)
if start < 0:
    raise SystemExit("InitWaterfallQualityControls signature not found")
brace = text.find("{", start)
if brace < 0:
    raise SystemExit("InitWaterfallQualityControls opening brace not found")
level = 0
end = None
for i in range(brace, len(text)):
    if text[i] == "{":
        level += 1
    elif text[i] == "}":
        level -= 1
        if level == 0:
            end = i + 1
            break
if end is None:
    raise SystemExit("InitWaterfallQualityControls closing brace not found")

method = r'''        private void InitWaterfallQualityControls()
        {
            if (comboPaletteRes != null) return; // already built

            // EU2AV supplied the complete WaterfallEnhancer backend, but the original
            // programmatic controls were appended to the narrow DirectX group and could
            // be clipped below its visible area. Put the complete control set in the
            // unused Display/General area between Multimeter and Spectral Warning LEDs.
            Control parent = grpDisplayDriverEngine.Parent;
            if (parent == null) return;

            GroupBoxTS grp = new GroupBoxTS();
            grp.Name = "grpWaterfallQuality";
            grp.Text = "Waterfall Quality";
            grp.Location = new System.Drawing.Point(
                grpSpectralWarningLeds.Location.X,
                grpDisplayMultimeter.Location.Y);
            grp.Size = new System.Drawing.Size(
                grpSpectralWarningLeds.Width,
                grpDisplayMultimeter.Height);
            parent.Controls.Add(grp);
            grp.BringToFront();

            int y = 21;

            lblPaletteRes = new LabelTS();
            lblPaletteRes.Text = "Quality:";
            lblPaletteRes.Location = new System.Drawing.Point(8, y + 3);
            lblPaletteRes.Size = new System.Drawing.Size(52, 16);
            grp.Controls.Add(lblPaletteRes);

            comboPaletteRes = new ComboBoxTS();
            comboPaletteRes.Name = "comboPaletteRes";
            comboPaletteRes.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPaletteRes.Items.AddRange(new object[] { "Classic", "Vivid", "Sharp", "Ultra" });
            comboPaletteRes.Location = new System.Drawing.Point(62, y);
            comboPaletteRes.Size = new System.Drawing.Size(94, 21);
            comboPaletteRes.SelectedIndex = 0;
            comboPaletteRes.SelectedIndexChanged += new EventHandler(comboPaletteRes_SelectedIndexChanged);
            toolTip1.SetToolTip(comboPaletteRes,
                "Waterfall post-processing: Classic, Vivid, Sharp or Ultra.");
            grp.Controls.Add(comboPaletteRes);

            int y2 = y + 27;
            chkWFDither = new CheckBoxTS();
            chkWFDither.Name = "chkWFDither";
            chkWFDither.Text = "Dither";
            chkWFDither.AutoSize = true;
            chkWFDither.Location = new System.Drawing.Point(8, y2);
            chkWFDither.Checked = false;
            chkWFDither.CheckedChanged += new EventHandler(chkWFDither_CheckedChanged);
            toolTip1.SetToolTip(chkWFDither,
                "Adds dithering to reduce visible colour banding (Ultra enables it automatically).");
            grp.Controls.Add(chkWFDither);

            int y3 = y2 + 25;
            lblWFGamma = new LabelTS();
            lblWFGamma.Text = "Gamma:";
            lblWFGamma.Location = new System.Drawing.Point(8, y3 + 3);
            lblWFGamma.Size = new System.Drawing.Size(48, 16);
            grp.Controls.Add(lblWFGamma);

            tbWFGamma = new TrackBarTS();
            tbWFGamma.Name = "tbWFGamma";
            tbWFGamma.Minimum = 50;
            tbWFGamma.Maximum = 200;
            tbWFGamma.Value = 100;
            tbWFGamma.TickFrequency = 25;
            tbWFGamma.Location = new System.Drawing.Point(55, y3 - 2);
            tbWFGamma.Size = new System.Drawing.Size(73, 28);
            tbWFGamma.Scroll += new EventHandler(tbWFGamma_Scroll);
            toolTip1.SetToolTip(tbWFGamma,
                "Waterfall gamma curve, 0.50 to 2.00. 1.00 is neutral.");
            grp.Controls.Add(tbWFGamma);

            lblWFGammaVal = new LabelTS();
            lblWFGammaVal.Text = "1.00";
            lblWFGammaVal.Location = new System.Drawing.Point(130, y3 + 3);
            lblWFGammaVal.Size = new System.Drawing.Size(32, 16);
            grp.Controls.Add(lblWFGammaVal);

            int y4 = y3 + 32;
            lblColorDepth = new LabelTS();
            lblColorDepth.Text = "Depth:";
            lblColorDepth.Location = new System.Drawing.Point(8, y4 + 3);
            lblColorDepth.Size = new System.Drawing.Size(52, 16);
            grp.Controls.Add(lblColorDepth);

            comboColorDepth = new ComboBoxTS();
            comboColorDepth.Name = "comboColorDepth";
            comboColorDepth.DropDownStyle = ComboBoxStyle.DropDownList;
            comboColorDepth.Items.AddRange(new object[] { "8-bit", "16-bit Float" });
            comboColorDepth.Location = new System.Drawing.Point(62, y4);
            comboColorDepth.Size = new System.Drawing.Size(94, 21);
            comboColorDepth.SelectedIndex = 0;
            comboColorDepth.SelectedIndexChanged += new EventHandler(comboColorDepth_SelectedIndexChanged);
            toolTip1.SetToolTip(comboColorDepth,
                "Waterfall render surface: classic 8-bit or 16-bit floating point. Change is live.");
            grp.Controls.Add(comboColorDepth);

            // The RX1/RX2/TX palette combos are intentionally compact in the legacy
            // layout. Widen the drop-down itself so the new Console 256 / Thermal 256 /
            // DeepBlue 256 names are always readable without disturbing adjacent controls.
            if (comboColorPalette != null) comboColorPalette.DropDownWidth = 125;
            if (comboRX2ColorPalette != null) comboRX2ColorPalette.DropDownWidth = 125;
            if (comboColorPalette_tx != null) comboColorPalette_tx.DropDownWidth = 125;
        }'''

text = text[:start] + method + text[end:]

required = [
    'grp.Text = "Waterfall Quality";',
    'comboPaletteRes.Items.AddRange(new object[] { "Classic", "Vivid", "Sharp", "Ultra" });',
    'comboColorDepth.Items.AddRange(new object[] { "8-bit", "16-bit Float" });',
    'comboColorPalette.DropDownWidth = 125;',
    'comboRX2ColorPalette.DropDownWidth = 125;',
    'comboColorPalette_tx.DropDownWidth = 125;',
    'SyncWaterfallEnhancerFromControls();',
]
for marker in required:
    if marker not in text:
        raise SystemExit(f"postcheck failed: {marker}")

path.write_text(text, encoding="utf-8")
print("Waterfall UI completed in setup.cs")
