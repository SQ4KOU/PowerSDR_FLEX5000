from pathlib import Path

BOM = b"\xef\xbb\xbf"


def read_source(path: str):
    p = Path(path)
    raw = p.read_bytes()
    had_bom = raw.startswith(BOM)
    if had_bom:
        raw = raw[len(BOM):]
    s = raw.decode("utf-8")
    newline = "\r\n" if "\r\n" in s else "\n"
    return p, had_bom, newline, s.replace("\r\n", "\n")


def write_source(p: Path, had_bom: bool, newline: str, s: str):
    if newline == "\r\n":
        s = s.replace("\n", "\r\n")
    raw = s.encode("utf-8")
    if had_bom:
        raw = BOM + raw
    p.write_bytes(raw)


# 1. Selective Ramdor N1MM engine delta.
p, bom, nl, s = read_source("Project Files/Source/Console/N1MM.cs")
if "public static bool CWShiftEnable" not in s:
    anchor = "        public static void Resize(int rx)\n"
    if anchor not in s:
        raise RuntimeError("N1MM Resize(int rx) anchor not found")

    prop = """        private static bool _cw_shift_enable = false;
        public static bool CWShiftEnable
        {
            set
            {
                bool old = _cw_shift_enable;
                _cw_shift_enable = value;
                if (old != _cw_shift_enable)
                {
                    Resize(1);
                    Resize(2);
                }
            }
            get
            {
                return _cw_shift_enable;
            }
        }
"""
    s = s.replace(anchor, prop + anchor, 1)

    start_marker = "                // MW0LGE [2.9.0.7] fix issue where spectrum is offset by cwpitch\n"
    end_marker = "                dL += nPitch * 1e-6;\n"
    a = s.find(start_marker)
    b = s.find(end_marker, a)
    if a < 0 or b < 0:
        raise RuntimeError("N1MM CW pitch block anchors not found")

    replacement = """                // MW0LGE [2.9.0.7] fix issue where spectrum is offset by cwpitch
                // MW0LGE [2.10.3.15] option to disable CW shift; default OFF for N1MM
                int nPitch = 0;
                if (_cw_shift_enable)
                {
                    switch (rx)
                    {
                        case 1:
                            {
                                if (Display.RX1DSPMode == DSPMode.CWL)
                                {
                                    nPitch = -Display.CWPitch;
                                }
                                else if (Display.RX1DSPMode == DSPMode.CWU)
                                {
                                    nPitch = Display.CWPitch;
                                }
                            }
                            break;
                        case 2:
                            if (Display.RX2DSPMode == DSPMode.CWL)
                            {
                                nPitch = -Display.CWPitch;
                            }
                            else if (Display.RX2DSPMode == DSPMode.CWU)
                            {
                                nPitch = Display.CWPitch;
                            }
                            break;
                    }
                }

"""
    s = s[:a] + replacement + s[b:]
    write_source(p, bom, nl, s)


# 2. Integrate the N1MM checkbox programmatically into EU2AV Setup.
# EU2AV already uses this pattern for DetCal and Phase Rotator controls.
p, bom, nl, s = read_source("Project Files/Source/Console/setup.cs")
if "InitN1mmCWShiftOption();" not in s:
    lines = s.splitlines(True)
    out = []
    inserted = False
    for line in lines:
        out.append(line)
        if "InitPhaseRotatorControls();" in line and not inserted:
            indent = line[: len(line) - len(line.lstrip())]
            out.append(indent + "InitN1mmCWShiftOption(); // Ramdor N1MM CW spectrum shift option\n")
            inserted = True
    if not inserted:
        raise RuntimeError("Setup constructor anchor not found")
    s = "".join(out)

    handler_anchor = "        private void chkWaterfall_smear_CheckedChanged(object sender, EventArgs e)\n"
    if handler_anchor not in s:
        raise RuntimeError("Setup end-of-class anchor not found")

    ui_code = """        private CheckBoxTS chkN1mm_include_cw_shift;

        private void InitN1mmCWShiftOption()
        {
            if (chkN1mm_include_cw_shift != null) return;

            chkN1mm_include_cw_shift = new CheckBoxTS();
            chkN1mm_include_cw_shift.AutoSize = true;
            chkN1mm_include_cw_shift.Image = null;
            chkN1mm_include_cw_shift.Location = new System.Drawing.Point(22, 136);
            chkN1mm_include_cw_shift.Name = "chkN1mm_include_cw_shift";
            chkN1mm_include_cw_shift.Size = new System.Drawing.Size(104, 17);
            chkN1mm_include_cw_shift.TabIndex = 76;
            chkN1mm_include_cw_shift.Text = "Include CW shift";
            chkN1mm_include_cw_shift.UseVisualStyleBackColor = true;
            toolTip1.SetToolTip(chkN1mm_include_cw_shift,
                "Include the CW frequency shift. This is not normally required; enable only if the N1MM spectrum frequency is offset from the signal. (default off)");
            chkN1mm_include_cw_shift.CheckedChanged += chkN1mm_include_cw_shift_CheckedChanged;
            groupBoxTS16.Controls.Add(chkN1mm_include_cw_shift);

            // Preserve the EU2AV designer while matching the Ramdor N1MM layout.
            groupBoxTS16.Size = new System.Drawing.Size(323, 159);
            panelTS15.Location = new System.Drawing.Point(361, 310);
            panelTS15.Size = new System.Drawing.Size(26, 77);
            groupBoxTS69.Location = new System.Drawing.Point(386, 310);
            groupBoxTS69.Size = new System.Drawing.Size(330, 86);

            N1MM.CWShiftEnable = chkN1mm_include_cw_shift.Checked;
        }

        private void chkN1mm_include_cw_shift_CheckedChanged(object sender, EventArgs e)
        {
            N1MM.CWShiftEnable = chkN1mm_include_cw_shift.Checked;
        }

"""
    s = s.replace(handler_anchor, ui_code + handler_anchor, 1)
    write_source(p, bom, nl, s)


# 3. Keep the neutral build-name compatibility symbol idempotently.
# Other upstream files reference TitleBar.BUILD_NAME, so the symbol must remain.
p, bom, nl, s = read_source("Project Files/Source/Console/titlebar.cs")
s = s.replace('        public const string BUILD_NAME = "";\n', '        public static readonly string BUILD_NAME = "";\n')
if 'public static readonly string BUILD_NAME = "";' not in s:
    anchor = "    class TitleBar\n    {\n"
    if anchor not in s:
        raise RuntimeError("TitleBar class anchor not found")
    s = s.replace(anchor, anchor + '        public static readonly string BUILD_NAME = "";\n\n', 1)

conditional = '            if (BUILD_NAME != "") s += " " + BUILD_NAME;\n'
if conditional not in s:
    anchor = '            s += " (" + VersionInfo.BuildDate + ")<FW>";  //[2.10.2.2]MW0LGE use the auto generated class from pre build event for the BuildDate\n'
    if anchor not in s:
        raise RuntimeError("TitleBar BuildDate anchor not found")
    s = s.replace(anchor, anchor + "\n" + conditional, 1)
write_source(p, bom, nl, s)


# 4. Record provenance and exclusions.
prov = Path(".sq4kou/SYNTHESIS_BASE.md")
text = prov.read_text(encoding="utf-8")
marker = "8071b543e2565b959cd60512eacda154d0873ad2"
if marker not in text:
    text += """

Integrated Ramdor delta:
- `8071b543e2565b959cd60512eacda154d0873ad2` — N1MM CW spectrum shift option only.
- Functional logic from Ramdor `N1MM.cs` retained.
- Setup control integrated programmatically to preserve EU2AV Setup designer additions.
- Package upgrades, app.config changes, unrelated test cleanup, branding and release-note edits from the Ramdor commit were deliberately excluded.
"""
    prov.write_text(text, encoding="utf-8")

print("Selective N1MM integration prepared successfully.")
