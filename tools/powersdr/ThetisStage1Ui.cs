// PowerSDR-THETIS UI Stage 1 - SQ4KOU
// Target: KE9NS PowerSDR v2.8.0, x86, FLEX-5000 native backend unchanged.
// Thetis is used only as the UI/UX reference. No donor radio-backend code is imported.
// GPLv2-or-later, consistent with the upstream PowerSDR/Thetis source trees.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static class ThetisStage1Ui
    {
        private const string RootName = "sq4kouThetisStage1Root";
        private static readonly Color Bg = Color.FromArgb(22, 24, 28);
        private static readonly Color PanelBg = Color.FromArgb(31, 34, 39);
        private static readonly Color PanelBg2 = Color.FromArgb(39, 43, 49);
        private static readonly Color Fg = Color.FromArgb(224, 228, 233);
        private static readonly Color Muted = Color.FromArgb(145, 154, 165);
        private static readonly Color Accent = Color.FromArgb(96, 176, 255);
        private static readonly Color Tx = Color.FromArgb(235, 95, 95);

        private sealed class UiState
        {
            public Form Form;
            public Label VfoA;
            public Label VfoB;
            public Label InfoA;
            public Label InfoB;
            public Label Meter;
            public Timer Timer;
        }

        private static readonly Dictionary<Form, UiState> States = new Dictionary<Form, UiState>();

        public static void Install(Form form)
        {
            if (form == null) return;
            form.Shown += delegate
            {
                try { form.BeginInvoke((MethodInvoker)delegate { Apply(form); }); }
                catch { }
            };
        }

        private static void Apply(Form form)
        {
            if (form == null || form.IsDisposed) return;
            if (form.Controls.Find(RootName, true).Length != 0) return;

            form.SuspendLayout();
            try
            {
                form.BackColor = Bg;
                form.ForeColor = Fg;
                if (form.MinimumSize.Width < 1050 || form.MinimumSize.Height < 650)
                    form.MinimumSize = new Size(Math.Max(1050, form.MinimumSize.Width), Math.Max(650, form.MinimumSize.Height));

                List<Control> original = SnapshotControls(form);
                StyleTree(form);

                Panel root = new Panel();
                root.Name = RootName;
                root.Dock = DockStyle.Top;
                root.Height = 214;
                root.Padding = new Padding(6, 5, 6, 5);
                root.BackColor = Bg;

                TableLayoutPanel vfoGrid = new TableLayoutPanel();
                vfoGrid.Dock = DockStyle.Top;
                vfoGrid.Height = 91;
                vfoGrid.ColumnCount = 3;
                vfoGrid.RowCount = 1;
                vfoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
                vfoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
                vfoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
                vfoGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                Label freqA, infoA, freqB, infoB;
                Panel a = MakeVfoPanel("VFO A", true, out freqA, out infoA);
                Panel b = MakeVfoPanel("VFO B", false, out freqB, out infoB);
                Label meterText;
                Panel meter = MakeMeterPanel(out meterText);
                vfoGrid.Controls.Add(a, 0, 0);
                vfoGrid.Controls.Add(b, 1, 0);
                vfoGrid.Controls.Add(meter, 2, 0);

                TableLayoutPanel commandGrid = new TableLayoutPanel();
                commandGrid.Dock = DockStyle.Fill;
                commandGrid.ColumnCount = 1;
                commandGrid.RowCount = 3;
                commandGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
                commandGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
                commandGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));

                FlowLayoutPanel rowBand = MakeCommandRow("BAND");
                FlowLayoutPanel rowMode = MakeCommandRow("MODE");
                FlowLayoutPanel rowFilter = MakeCommandRow("FILTER / DSP / TX");
                commandGrid.Controls.Add(rowBand, 0, 0);
                commandGrid.Controls.Add(rowMode, 0, 1);
                commandGrid.Controls.Add(rowFilter, 0, 2);

                root.Controls.Add(commandGrid);
                root.Controls.Add(vfoGrid);
                form.Controls.Add(root);
                root.BringToFront();

                MoveButtons(original, rowBand, ButtonCategory.Band);
                MoveButtons(original, rowMode, ButtonCategory.Mode);
                MoveButtons(original, rowFilter, ButtonCategory.Filter);
                MoveButtons(original, rowFilter, ButtonCategory.Dsp);
                MoveButtons(original, rowFilter, ButtonCategory.TxRx);

                UiState state = new UiState();
                state.Form = form;
                state.VfoA = freqA;
                state.VfoB = freqB;
                state.InfoA = infoA;
                state.InfoB = infoB;
                state.Meter = meterText;
                state.Timer = new Timer();
                state.Timer.Interval = 100;
                state.Timer.Tick += delegate { RefreshState(state); };
                state.Timer.Start();
                States[form] = state;

                WireVfoMouse(form, freqA, "VFOAFreq");
                WireVfoMouse(form, freqB, "VFOBFreq");

                form.FormClosed += delegate
                {
                    UiState s;
                    if (States.TryGetValue(form, out s))
                    {
                        try { s.Timer.Stop(); s.Timer.Dispose(); } catch { }
                        States.Remove(form);
                    }
                };
                RefreshState(state);
            }
            finally { form.ResumeLayout(true); }
        }

        private enum ButtonCategory { Band, Mode, Filter, Dsp, TxRx }

        private static List<Control> SnapshotControls(Control root)
        {
            List<Control> list = new List<Control>();
            AddChildren(root, list);
            return list;
        }

        private static void AddChildren(Control root, List<Control> list)
        {
            foreach (Control c in root.Controls)
            {
                list.Add(c);
                if (c.HasChildren) AddChildren(c, list);
            }
        }

        private static void StyleTree(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is MenuStrip || c is StatusStrip || c is ToolStrip) { }
                else if (c is TextBoxBase)
                {
                    c.BackColor = Color.FromArgb(18, 20, 23);
                    c.ForeColor = Fg;
                }
                else if (c is GroupBox || c is Panel || c is TabPage)
                {
                    c.BackColor = PanelBg;
                    c.ForeColor = Fg;
                }
                else if (c is Label)
                {
                    c.ForeColor = Fg;
                    if (c.BackColor != Color.Transparent) c.BackColor = Color.Transparent;
                }
                else if (c is ButtonBase)
                {
                    c.ForeColor = Fg;
                    c.BackColor = PanelBg2;
                }
                if (c.HasChildren) StyleTree(c);
            }
        }

        private static Panel MakeVfoPanel(string title, bool primary, out Label frequency, out Label info)
        {
            Panel p = new Panel();
            p.Dock = DockStyle.Fill;
            p.Margin = new Padding(2);
            p.Padding = new Padding(8, 4, 8, 4);
            p.BackColor = primary ? Color.FromArgb(28, 33, 40) : PanelBg;

            Label cap = new Label();
            cap.Dock = DockStyle.Top;
            cap.Height = 17;
            cap.Text = title;
            cap.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            cap.ForeColor = primary ? Accent : Muted;
            cap.BackColor = Color.Transparent;

            frequency = new Label();
            frequency.Dock = DockStyle.Fill;
            frequency.TextAlign = ContentAlignment.MiddleLeft;
            frequency.Text = "0.000.000";
            frequency.Font = new Font("Segoe UI", 25F, FontStyle.Bold, GraphicsUnit.Point);
            frequency.ForeColor = Fg;
            frequency.BackColor = Color.Transparent;
            frequency.Cursor = Cursors.Hand;

            info = new Label();
            info.Dock = DockStyle.Bottom;
            info.Height = 18;
            info.Text = "---";
            info.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            info.ForeColor = Muted;
            info.BackColor = Color.Transparent;

            p.Controls.Add(frequency);
            p.Controls.Add(info);
            p.Controls.Add(cap);
            return p;
        }

        private static Panel MakeMeterPanel(out Label meter)
        {
            Panel p = new Panel();
            p.Dock = DockStyle.Fill;
            p.Margin = new Padding(2);
            p.Padding = new Padding(6);
            p.BackColor = PanelBg;

            Label cap = new Label();
            cap.Dock = DockStyle.Top;
            cap.Height = 18;
            cap.Text = "RX / TX";
            cap.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            cap.ForeColor = Muted;

            meter = new Label();
            meter.Dock = DockStyle.Fill;
            meter.TextAlign = ContentAlignment.MiddleCenter;
            meter.Text = "RX";
            meter.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            meter.ForeColor = Accent;

            p.Controls.Add(meter);
            p.Controls.Add(cap);
            return p;
        }

        private static FlowLayoutPanel MakeCommandRow(string label)
        {
            FlowLayoutPanel row = new FlowLayoutPanel();
            row.Dock = DockStyle.Fill;
            row.WrapContents = false;
            row.AutoScroll = true;
            row.FlowDirection = FlowDirection.LeftToRight;
            row.Padding = new Padding(3, 2, 3, 1);
            row.Margin = new Padding(0);
            row.BackColor = Bg;

            Label l = new Label();
            l.Text = label;
            l.AutoSize = false;
            l.Width = 105;
            l.Height = 25;
            l.TextAlign = ContentAlignment.MiddleLeft;
            l.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            l.ForeColor = Muted;
            l.Margin = new Padding(0, 1, 5, 0);
            row.Controls.Add(l);
            return row;
        }

        private static void MoveButtons(List<Control> original, FlowLayoutPanel target, ButtonCategory category)
        {
            List<Control> matches = new List<Control>();
            foreach (Control c in original)
            {
                ButtonBase b = c as ButtonBase;
                if (b == null || b.IsDisposed || !b.Visible) continue;
                string name = Normalize(b.Name);
                if (name.Contains("RX2")) continue;
                if (!MatchesCategory(b, category)) continue;
                if (!matches.Contains(b)) matches.Add(b);
            }
            matches.Sort(delegate(Control x, Control y) { return CategoryOrder(x, category).CompareTo(CategoryOrder(y, category)); });
            foreach (Control c in matches)
            {
                try
                {
                    ButtonBase b = (ButtonBase)c;
                    b.AutoSize = false;
                    b.Width = Math.Max(47, Math.Min(72, TextRenderer.MeasureText(b.Text ?? "", b.Font).Width + 18));
                    b.Height = 24;
                    b.Margin = new Padding(1);
                    b.ForeColor = Fg;
                    b.BackColor = PanelBg2;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.BorderColor = Color.FromArgb(67, 72, 80);
                    target.Controls.Add(b);
                }
                catch { }
            }
        }

        private static bool MatchesCategory(ButtonBase b, ButtonCategory category)
        {
            string t = Normalize(b.Text);
            string n = Normalize(b.Name);
            switch (category)
            {
                case ButtonCategory.Band:
                    return IsOneOf(t, "160", "160M", "80", "80M", "60", "60M", "40", "40M", "30", "30M", "20", "20M", "17", "17M", "15", "15M", "12", "12M", "10", "10M", "6", "6M", "GEN", "WWV") ||
                           (n.Contains("BAND") && IsOneOf(t, "160", "160M", "80", "80M", "60", "60M", "40", "40M", "30", "30M", "20", "20M", "17", "17M", "15", "15M", "12", "12M", "10", "10M", "6", "6M"));
                case ButtonCategory.Mode:
                    return IsOneOf(t, "LSB", "USB", "DSB", "CWL", "CWU", "AM", "SAM", "FM", "DIGL", "DIGU", "DRM", "SPEC");
                case ButtonCategory.Filter:
                    return IsOneOf(t, "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "VAR1", "VAR2") || n.Contains("FILTER") && (t.StartsWith("F") || t.StartsWith("VAR"));
                case ButtonCategory.Dsp:
                    return IsOneOf(t, "NR", "NR2", "NB", "NB2", "ANF", "SNB", "BIN", "SQL", "SQL1", "APF");
                case ButtonCategory.TxRx:
                    return IsOneOf(t, "MOX", "TUN", "TUNE", "SPLIT", "RIT", "XIT", "RX2", "MUT", "MUTE", "VOX");
            }
            return false;
        }

        private static int CategoryOrder(Control c, ButtonCategory category)
        {
            string t = Normalize(c.Text);
            string sequence = category == ButtonCategory.Band ? "160M|160|80M|80|60M|60|40M|40|30M|30|20M|20|17M|17|15M|15|12M|12|10M|10|6M|6|GEN|WWV" :
                              category == ButtonCategory.Mode ? "LSB|USB|DSB|CWL|CWU|AM|SAM|FM|DIGL|DIGU|DRM|SPEC" :
                              category == ButtonCategory.Filter ? "F1|F2|F3|F4|F5|F6|F7|F8|F9|F10|VAR1|VAR2" :
                              category == ButtonCategory.Dsp ? "NR|NR2|NB|NB2|ANF|SNB|APF|BIN|SQL|SQL1" :
                              "MOX|TUN|TUNE|SPLIT|RIT|XIT|RX2|VOX|MUT|MUTE";
            string[] a = sequence.Split('|');
            for (int i = 0; i < a.Length; i++) if (t == a[i]) return i;
            return 999;
        }

        private static bool IsOneOf(string value, params string[] choices)
        {
            foreach (string s in choices) if (value == s) return true;
            return false;
        }

        private static string Normalize(string s)
        {
            if (String.IsNullOrEmpty(s)) return "";
            return s.Trim().Replace(" ", "").Replace("-", "").ToUpperInvariant();
        }

        private static void RefreshState(UiState state)
        {
            if (state == null || state.Form == null || state.Form.IsDisposed) return;
            double a, b;
            bool haveA = TryReadDouble(state.Form, "VFOAFreq", out a);
            bool haveB = TryReadDouble(state.Form, "VFOBFreq", out b);
            if (haveA) state.VfoA.Text = FormatMHz(a);
            if (haveB) state.VfoB.Text = FormatMHz(b);

            object modeA = ReadProperty(state.Form, "RX1DSPMode");
            object modeB = ReadProperty(state.Form, "RX2DSPMode");
            object bandA = ReadProperty(state.Form, "RX1Band");
            object bandB = ReadProperty(state.Form, "RX2Band");
            state.InfoA.Text = JoinInfo(bandA, modeA);
            state.InfoB.Text = JoinInfo(bandB, modeB);

            bool tx = ReadBoolAny(state.Form, "MOX", "Mox", "TX", "Transmit");
            bool power = ReadBoolAny(state.Form, "PowerOn", "Power");
            state.Meter.Text = tx ? "TX" : (power ? "RX" : "OFF");
            state.Meter.ForeColor = tx ? Tx : (power ? Accent : Muted);
        }

        private static string JoinInfo(object a, object b)
        {
            string sa = a == null ? "" : a.ToString();
            string sb = b == null ? "" : b.ToString();
            if (sa.Length == 0) return sb.Length == 0 ? "---" : sb;
            if (sb.Length == 0) return sa;
            return sa + "   " + sb;
        }

        private static string FormatMHz(double mhz)
        {
            if (Double.IsNaN(mhz) || Double.IsInfinity(mhz) || mhz < 0) return "---";
            long hz = (long)Math.Round(mhz * 1000000.0);
            long mhzWhole = hz / 1000000L;
            long khz = (hz / 1000L) % 1000L;
            long rest = hz % 1000L;
            return mhzWhole.ToString(CultureInfo.InvariantCulture) + "." + khz.ToString("000", CultureInfo.InvariantCulture) + "." + rest.ToString("000", CultureInfo.InvariantCulture);
        }

        private static void WireVfoMouse(Form form, Label label, string propertyName)
        {
            label.MouseWheel += delegate(object sender, MouseEventArgs e)
            {
                double current;
                if (!TryReadDouble(form, propertyName, out current)) return;
                double stepHz = (Control.ModifierKeys & Keys.Control) != 0 ? 1000.0 : ((Control.ModifierKeys & Keys.Shift) != 0 ? 10.0 : 100.0);
                double next = current + (e.Delta > 0 ? stepHz : -stepHz) / 1000000.0;
                WriteProperty(form, propertyName, next);
            };
            label.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
                double current;
                if (!TryReadDouble(form, propertyName, out current)) return;
                double stepHz = (Control.ModifierKeys & Keys.Control) != 0 ? 1000.0 : ((Control.ModifierKeys & Keys.Shift) != 0 ? 10.0 : 100.0);
                double next = current + (e.Button == MouseButtons.Left ? stepHz : -stepHz) / 1000000.0;
                WriteProperty(form, propertyName, next);
            };
        }

        private static PropertyInfo FindProperty(object obj, string name)
        {
            if (obj == null) return null;
            return obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static object ReadProperty(object obj, string name)
        {
            try
            {
                PropertyInfo p = FindProperty(obj, name);
                return p == null || !p.CanRead ? null : p.GetValue(obj, null);
            }
            catch { return null; }
        }

        private static bool TryReadDouble(object obj, string name, out double value)
        {
            value = 0.0;
            object v = ReadProperty(obj, name);
            if (v == null) return false;
            try { value = Convert.ToDouble(v, CultureInfo.InvariantCulture); return true; }
            catch { return false; }
        }

        private static bool WriteProperty(object obj, string name, object value)
        {
            try
            {
                PropertyInfo p = FindProperty(obj, name);
                if (p == null || !p.CanWrite) return false;
                object converted = Convert.ChangeType(value, p.PropertyType, CultureInfo.InvariantCulture);
                p.SetValue(obj, converted, null);
                return true;
            }
            catch { return false; }
        }

        private static bool ReadBoolAny(object obj, params string[] names)
        {
            foreach (string name in names)
            {
                object v = ReadProperty(obj, name);
                if (v == null) continue;
                try { return Convert.ToBoolean(v, CultureInfo.InvariantCulture); }
                catch { }
            }
            return false;
        }
    }
}
