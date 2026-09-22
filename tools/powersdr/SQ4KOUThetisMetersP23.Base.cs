// P23 support layer: retained FLEX-5000 adapter + skin downloader from P22; legacy launcher is NOT hooked.
// Functional WinForms meter/gadget layer for native PowerSDR state.
// GPLv2-or-later, consistent with upstream PowerSDR/Thetis licensing.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace PowerSDR
{
    internal static class SQ4KOUMetersGadgetsP22Legacy
    {
        private const string SetupMarker = "sq4kouMetersGadgetsPage";
        private static PowerSDR.Console _console;
        private static Setup _setup;
        private static MetersGadgetsForm _form;

        public static void Install(PowerSDR.Console console, Setup setup)
        {
            if (console == null || setup == null) return;
            _console = console;
            _setup = setup;

            setup.Shown += delegate
            {
                try
                {
                    setup.BeginInvoke((MethodInvoker)delegate { InstallIntoAppearance(); });
                }
                catch { }
            };
        }

        private static void InstallIntoAppearance()
        {
            if (_setup == null || _setup.IsDisposed) return;
            if (_setup.Controls.Find(SetupMarker, true).Length != 0) return;

            TabPage appearance = FindAppearanceTab(_setup);
            if (appearance == null)
            {
                AddFallbackLauncher(_setup);
                return;
            }

            TabControl nested = FindFirstTabControl(appearance);
            if (nested != null)
            {
                foreach (TabPage existing in nested.TabPages)
                {
                    if (String.Equals(existing.Name, SetupMarker, StringComparison.OrdinalIgnoreCase) ||
                        Normalize(existing.Text).Contains("METERSGADGETS"))
                        return;
                }

                TabPage page = new TabPage();
                page.Name = SetupMarker;
                page.Text = "Meters / Gadgets";
                page.UseVisualStyleBackColor = true;
                BuildAppearancePage(page);
                nested.TabPages.Add(page);
            }
            else
            {
                GroupBox box = new GroupBox();
                box.Name = SetupMarker;
                box.Text = "Meters / Gadgets";
                box.Dock = DockStyle.Bottom;
                box.Height = 92;

                Button open = MakeOpenButton();
                open.Location = new Point(12, 25);
                box.Controls.Add(open);

                Button skins = MakeSkinsButton();
                skins.Location = new Point(176, 25);
                box.Controls.Add(skins);

                Label l = new Label();
                l.AutoSize = true;
                l.Location = new Point(12, 59);
                l.Text = "Native FLEX-5000 readings; independent meter windows and OE3IDE skin catalogue.";
                box.Controls.Add(l);

                appearance.Controls.Add(box);
                box.BringToFront();
            }
        }

        private static void BuildAppearancePage(Control page)
        {
            Panel panel = new Panel();
            panel.Name = SetupMarker + "Panel";
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(16);

            Label title = new Label();
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            title.Location = new Point(16, 16);
            title.Text = "FLEX-5000 Meters / Gadgets";

            Label detail = new Label();
            detail.AutoSize = false;
            detail.Location = new Point(16, 47);
            detail.Size = new Size(610, 44);
            detail.Text = "Live data comes from the existing PowerSDR / DttSP signal path. " +
                          "Meter windows do not replace or reconfigure the radio backend.";

            Button open = MakeOpenButton();
            open.Location = new Point(16, 101);

            Button skins = MakeSkinsButton();
            skins.Location = new Point(180, 101);

            Label compat = new Label();
            compat.AutoSize = false;
            compat.Location = new Point(16, 146);
            compat.Size = new Size(660, 72);
            compat.Text =
                "OE3IDE: catalogue, download and safe extraction are supported. " +
                "Image assets from a downloaded meter package can be used as a gadget background. " +
                "Items that require a Thetis-specific radio data source are not fabricated.";

            panel.Controls.Add(title);
            panel.Controls.Add(detail);
            panel.Controls.Add(open);
            panel.Controls.Add(skins);
            panel.Controls.Add(compat);
            page.Controls.Add(panel);
        }

        private static Button MakeOpenButton()
        {
            Button b = new Button();
            b.Name = "btnSQ4KOUMetersGadgets";
            b.Size = new Size(150, 30);
            b.Text = "Open Meters / Gadgets";
            b.Click += delegate { ShowMeters(); };
            return b;
        }

        private static Button MakeSkinsButton()
        {
            Button b = new Button();
            b.Name = "btnSQ4KOUOE3IDESkins";
            b.Size = new Size(145, 30);
            b.Text = "OE3IDE meter skins...";
            b.Click += delegate { ShowSkins(); };
            return b;
        }

        private static void AddFallbackLauncher(Form setup)
        {
            Button b = MakeOpenButton();
            b.Name = SetupMarker;
            b.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            b.Location = new Point(Math.Max(4, setup.ClientSize.Width - b.Width - 12),
                                   Math.Max(4, setup.ClientSize.Height - b.Height - 12));
            setup.Controls.Add(b);
            b.BringToFront();
        }

        private static void ShowMeters()
        {
            if (_console == null || _console.IsDisposed) return;
            if (_form == null || _form.IsDisposed)
            {
                _form = new MetersGadgetsForm(_console);
                _form.FormClosed += delegate { _form = null; };
            }

            if (!_form.Visible)
                _form.Show(_console);

            if (_form.WindowState == FormWindowState.Minimized)
                _form.WindowState = FormWindowState.Normal;

            _form.BringToFront();
            _form.Activate();
        }

        private static void ShowSkins()
        {
            if (_console == null || _console.IsDisposed) return;
            using (OE3IDESkinBrowser f = new OE3IDESkinBrowser(_console))
            {
                if (f.ShowDialog(_setup) == DialogResult.OK && !String.IsNullOrEmpty(f.SelectedBackground))
                {
                    ShowMeters();
                    if (_form != null) _form.ApplyBackground(f.SelectedBackground);
                }
            }
        }

        private static TabPage FindAppearanceTab(Control root)
        {
            foreach (Control c in root.Controls)
            {
                TabPage p = c as TabPage;
                if (p != null)
                {
                    string s = Normalize((p.Text ?? "") + " " + (p.Name ?? ""));
                    if (s.Contains("APPEARANCE")) return p;
                }

                TabPage nested = FindAppearanceTab(c);
                if (nested != null) return nested;
            }
            return null;
        }

        private static TabControl FindFirstTabControl(Control root)
        {
            foreach (Control c in root.Controls)
            {
                TabControl tc = c as TabControl;
                if (tc != null) return tc;
            }
            foreach (Control c in root.Controls)
            {
                TabControl tc = FindFirstTabControl(c);
                if (tc != null) return tc;
            }
            return null;
        }

        private static string Normalize(string s)
        {
            return (s ?? "").Replace(" ", "").Replace("/", "").Replace("-", "").ToUpperInvariant();
        }
    }

    internal sealed class Flex5000MeterAdapter
    {
        private readonly PowerSDR.Console _console;
        private readonly Type _type;
        private readonly BindingFlags _flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public Flex5000MeterAdapter(PowerSDR.Console console)
        {
            _console = console;
            _type = console.GetType();
        }

        public bool PowerOn { get { return ReadBool("PowerOn", "Power"); } }
        public bool IsTx { get { return ReadBool("MOX", "Mox", "TX", "Transmit"); } }

        public double VfoA { get { return ReadDouble(0.0, "VFOAFreq"); } }
        public double VfoB { get { return ReadDouble(0.0, "VFOBFreq"); } }
        public string Band { get { return ReadText("RX1Band", "TXBand", "Band"); } }
        public string Mode { get { return ReadText("RX1DSPMode", "CurrentDSPMode"); } }
        public string Filter { get { return ReadText("RX1Filter", "CurrentFilter"); } }

        public double SignalDbm
        {
            get
            {
                if (!PowerOn) return -200.0;
                try
                {
                    double raw = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.SIGNAL_STRENGTH);
                    double cal = ReadDouble(0.0, "MultiMeterCalOffset");
                    double pre = ReadDouble(0.0, "PreampOffset");
                    return raw + cal + pre;
                }
                catch { return -200.0; }
            }
        }

        public string SUnit(double dbm)
        {
            if (dbm <= -200.0) return "--";
            if (dbm >= -73.0)
            {
                int plus = (int)Math.Round(dbm + 73.0);
                return plus <= 0 ? "S9" : "S9+" + plus.ToString(CultureInfo.InvariantCulture);
            }
            int s = (int)Math.Floor((dbm + 127.0) / 6.0) + 1;
            if (s < 1) s = 1;
            if (s > 9) s = 9;
            return "S" + s.ToString(CultureInfo.InvariantCulture);
        }

        public double MicDb
        {
            get
            {
                try { return DttSP.CalculateTXMeter(0, DttSP.MeterType.MIC); }
                catch { return -200.0; }
            }
        }

        public double AlcDb
        {
            get
            {
                try { return DttSP.CalculateTXMeter(0, DttSP.MeterType.ALC); }
                catch { return -200.0; }
            }
        }

        public double ForwardPower
        {
            get
            {
                object raw = ReadMember("pa_fwd_power");
                if (raw == null) return 0.0;
                try
                {
                    MethodInfo m = FindMethod("FWCPAPower", 1);
                    if (m == null) return 0.0;
                    object arg = Convert.ChangeType(raw, m.GetParameters()[0].ParameterType, CultureInfo.InvariantCulture);
                    object value = m.Invoke(_console, new object[] { arg });
                    return Convert.ToDouble(value, CultureInfo.InvariantCulture);
                }
                catch { return 0.0; }
            }
        }

        public double Swr
        {
            get
            {
                object fwd = ReadMember("pa_fwd_power");
                object rev = ReadMember("pa_rev_power");
                if (fwd == null || rev == null) return 1.0;
                try
                {
                    MethodInfo m = FindMethod("FWCSWR", 2);
                    if (m == null) return 1.0;
                    ParameterInfo[] p = m.GetParameters();
                    object a0 = Convert.ChangeType(fwd, p[0].ParameterType, CultureInfo.InvariantCulture);
                    object a1 = Convert.ChangeType(rev, p[1].ParameterType, CultureInfo.InvariantCulture);
                    double value = Convert.ToDouble(m.Invoke(_console, new object[] { a0, a1 }), CultureInfo.InvariantCulture);
                    if (Double.IsNaN(value) || Double.IsInfinity(value) || value < 1.0) return 1.0;
                    return Math.Min(value, 99.9);
                }
                catch { return 1.0; }
            }
        }

        public bool SetVfoA(double mhz) { return WriteMember("VFOAFreq", mhz); }
        public bool SetVfoB(double mhz) { return WriteMember("VFOBFreq", mhz); }

        public Dictionary<string, object> Snapshot()
        {
            double dbm = SignalDbm;
            Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            d["RX1_SIGNAL_DBM"] = dbm;
            d["RX1_SUNIT"] = SUnit(dbm);
            d["VFOA"] = VfoA;
            d["VFOB"] = VfoB;
            d["BAND"] = Band;
            d["MODE"] = Mode;
            d["FILTER"] = Filter;
            d["TX"] = IsTx;
            d["POWER_ON"] = PowerOn;
            d["TX_PWR"] = ForwardPower;
            d["SWR"] = Swr;
            d["MIC"] = MicDb;
            d["ALC"] = AlcDb;

            // Common aliases used by meter skin authors.
            d["SIGNAL"] = dbm;
            d["PWR"] = d["TX_PWR"];
            d["FWD"] = d["TX_PWR"];
            d["MOX"] = d["TX"];
            d["RX1_MODE"] = d["MODE"];
            d["RX1_BAND"] = d["BAND"];
            return d;
        }

        private MethodInfo FindMethod(string name, int parameterCount)
        {
            MethodInfo[] methods = _type.GetMethods(_flags);
            foreach (MethodInfo m in methods)
                if (String.Equals(m.Name, name, StringComparison.Ordinal) &&
                    m.GetParameters().Length == parameterCount)
                    return m;
            return null;
        }

        private object ReadMember(params string[] names)
        {
            foreach (string name in names)
            {
                try
                {
                    PropertyInfo p = _type.GetProperty(name, _flags);
                    if (p != null && p.CanRead) return p.GetValue(_console, null);
                    FieldInfo f = _type.GetField(name, _flags);
                    if (f != null) return f.GetValue(_console);
                }
                catch { }
            }
            return null;
        }

        private bool WriteMember(string name, object value)
        {
            try
            {
                PropertyInfo p = _type.GetProperty(name, _flags);
                if (p != null && p.CanWrite)
                {
                    p.SetValue(_console, Convert.ChangeType(value, p.PropertyType, CultureInfo.InvariantCulture), null);
                    return true;
                }
                FieldInfo f = _type.GetField(name, _flags);
                if (f != null && !f.IsInitOnly)
                {
                    f.SetValue(_console, Convert.ChangeType(value, f.FieldType, CultureInfo.InvariantCulture));
                    return true;
                }
            }
            catch { }
            return false;
        }

        private bool ReadBool(params string[] names)
        {
            object v = ReadMember(names);
            if (v == null) return false;
            try { return Convert.ToBoolean(v, CultureInfo.InvariantCulture); }
            catch { return false; }
        }

        private double ReadDouble(double fallback, params string[] names)
        {
            object v = ReadMember(names);
            if (v == null) return fallback;
            try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private string ReadText(params string[] names)
        {
            object v = ReadMember(names);
            return v == null ? "--" : v.ToString();
        }
    }

    internal sealed class MeterLayout
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool TopMost { get; set; }
        public string BackgroundImagePath { get; set; }
        public List<string> VisibleGadgets { get; set; }

        public MeterLayout()
        {
            X = -1;
            Y = -1;
            Width = 840;
            Height = 410;
            TopMost = false;
            VisibleGadgets = new List<string>();
        }
    }

    internal sealed class GadgetCard : Panel
    {
        public string GadgetId { get; private set; }
        public Label Caption { get; private set; }
        public Label Value { get; private set; }

        public GadgetCard(string id, string caption)
        {
            GadgetId = id;
            Width = 184;
            Height = 88;
            Margin = new Padding(5);
            Padding = new Padding(8);
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.FromArgb(28, 31, 35);

            Caption = new Label();
            Caption.Dock = DockStyle.Top;
            Caption.Height = 22;
            Caption.Text = caption;
            Caption.ForeColor = Color.Silver;
            Caption.BackColor = Color.Transparent;
            Caption.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            Value = new Label();
            Value.Dock = DockStyle.Fill;
            Value.TextAlign = ContentAlignment.MiddleCenter;
            Value.ForeColor = Color.WhiteSmoke;
            Value.BackColor = Color.Transparent;
            Value.Font = new Font("Segoe UI", 15.5F, FontStyle.Bold);
            Value.Text = "--";

            Controls.Add(Value);
            Controls.Add(Caption);
        }
    }

    internal sealed class MetersGadgetsForm : Form
    {
        private readonly PowerSDR.Console _console;
        private readonly Flex5000MeterAdapter _adapter;
        private readonly FlowLayoutPanel _flow;
        private readonly Dictionary<string, GadgetCard> _cards =
            new Dictionary<string, GadgetCard>(StringComparer.OrdinalIgnoreCase);
        private readonly Timer _timer;
        private readonly string _configRoot;
        private readonly string _configPath;
        private MeterLayout _layout;
        private Image _backgroundImage;

        public MetersGadgetsForm(PowerSDR.Console console)
        {
            _console = console;
            _adapter = new Flex5000MeterAdapter(console);
            Text = "PowerSDR FLEX-5000 - Meters / Gadgets";
            StartPosition = FormStartPosition.Manual;
            MinimumSize = new Size(420, 230);
            BackColor = Color.FromArgb(19, 21, 24);
            ForeColor = Color.WhiteSmoke;
            Font = new Font("Segoe UI", 9F);

            _configRoot = GetConfigRoot(console);
            _configPath = Path.Combine(_configRoot, "layout.json");
            Directory.CreateDirectory(_configRoot);
            _layout = LoadLayout();

            ToolStrip strip = new ToolStrip();
            strip.GripStyle = ToolStripGripStyle.Hidden;
            strip.Dock = DockStyle.Top;

            ToolStripButton top = new ToolStripButton("Top most");
            top.CheckOnClick = true;
            top.Checked = _layout.TopMost;
            top.CheckedChanged += delegate
            {
                TopMost = top.Checked;
                _layout.TopMost = top.Checked;
            };

            ToolStripDropDownButton items = new ToolStripDropDownButton("Gadgets");
            ToolStripButton skins = new ToolStripButton("OE3IDE skins");
            skins.Click += delegate { BrowseSkins(); };
            ToolStripButton reset = new ToolStripButton("Reset layout");
            reset.Click += delegate { ResetLayout(); };

            strip.Items.Add(top);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(items);
            strip.Items.Add(skins);
            strip.Items.Add(reset);

            _flow = new FlowLayoutPanel();
            _flow.Dock = DockStyle.Fill;
            _flow.AutoScroll = true;
            _flow.WrapContents = true;
            _flow.Padding = new Padding(8);
            _flow.BackColor = Color.Transparent;

            Controls.Add(_flow);
            Controls.Add(strip);

            AddCard("signal", "RX1 SIGNAL", items);
            AddCard("sunit", "S-METER", items);
            AddCard("vfoa", "VFO A", items);
            AddCard("vfob", "VFO B", items);
            AddCard("band", "BAND", items);
            AddCard("mode", "MODE / FILTER", items);
            AddCard("state", "RADIO", items);
            AddCard("power", "TX POWER", items);
            AddCard("swr", "SWR", items);
            AddCard("mic", "MIC", items);
            AddCard("alc", "ALC", items);

            GadgetCard vfoA = _cards["vfoa"];
            GadgetCard vfoB = _cards["vfob"];
            vfoA.Cursor = Cursors.Hand;
            vfoB.Cursor = Cursors.Hand;
            vfoA.MouseWheel += delegate(object s, MouseEventArgs e) { StepVfo(true, e.Delta); };
            vfoB.MouseWheel += delegate(object s, MouseEventArgs e) { StepVfo(false, e.Delta); };
            vfoA.Value.MouseWheel += delegate(object s, MouseEventArgs e) { StepVfo(true, e.Delta); };
            vfoB.Value.MouseWheel += delegate(object s, MouseEventArgs e) { StepVfo(false, e.Delta); };

            ApplyLayout();

            _timer = new Timer();
            _timer.Interval = 100;
            _timer.Tick += delegate { RefreshValues(); };
            _timer.Start();

            FormClosing += delegate { SaveLayout(); };
            FormClosed += delegate
            {
                _timer.Stop();
                _timer.Dispose();
                if (_backgroundImage != null) _backgroundImage.Dispose();
            };

            RefreshValues();
        }

        private void AddCard(string id, string caption, ToolStripDropDownButton menu)
        {
            GadgetCard card = new GadgetCard(id, caption);
            _cards[id] = card;
            _flow.Controls.Add(card);

            ToolStripMenuItem item = new ToolStripMenuItem(caption);
            item.CheckOnClick = true;
            item.Checked = IsVisibleByLayout(id);
            item.CheckedChanged += delegate
            {
                card.Visible = item.Checked;
                SyncVisibleList();
            };
            menu.DropDownItems.Add(item);
            card.Visible = item.Checked;
        }

        private bool IsVisibleByLayout(string id)
        {
            if (_layout.VisibleGadgets == null || _layout.VisibleGadgets.Count == 0) return true;
            return _layout.VisibleGadgets.Exists(delegate(string x)
            {
                return String.Equals(x, id, StringComparison.OrdinalIgnoreCase);
            });
        }

        private void SyncVisibleList()
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, GadgetCard> kv in _cards)
                if (kv.Value.Visible) list.Add(kv.Key);
            _layout.VisibleGadgets = list;
        }

        private void ApplyLayout()
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            int w = Math.Max(MinimumSize.Width, _layout.Width);
            int h = Math.Max(MinimumSize.Height, _layout.Height);
            w = Math.Min(w, wa.Width);
            h = Math.Min(h, wa.Height);

            if (_layout.X >= wa.Left && _layout.X < wa.Right &&
                _layout.Y >= wa.Top && _layout.Y < wa.Bottom)
                Bounds = new Rectangle(_layout.X, _layout.Y, w, h);
            else
            {
                Size = new Size(w, h);
                Location = new Point(wa.Left + Math.Max(0, (wa.Width - w) / 2),
                                     wa.Top + Math.Max(0, (wa.Height - h) / 2));
            }

            TopMost = _layout.TopMost;
            if (!String.IsNullOrEmpty(_layout.BackgroundImagePath) &&
                File.Exists(_layout.BackgroundImagePath))
                ApplyBackground(_layout.BackgroundImagePath);
        }

        public void ApplyBackground(string path)
        {
            try
            {
                if (String.IsNullOrEmpty(path) || !File.Exists(path)) return;
                Image next;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image src = Image.FromStream(fs))
                    next = new Bitmap(src);

                Image old = _backgroundImage;
                _backgroundImage = next;
                BackgroundImage = _backgroundImage;
                BackgroundImageLayout = ImageLayout.Stretch;
                _flow.BackgroundImage = _backgroundImage;
                _flow.BackgroundImageLayout = ImageLayout.Stretch;
                _layout.BackgroundImagePath = path;
                if (old != null) old.Dispose();
            }
            catch { }
        }

        private void BrowseSkins()
        {
            using (OE3IDESkinBrowser f = new OE3IDESkinBrowser(_console))
            {
                if (f.ShowDialog(this) == DialogResult.OK &&
                    !String.IsNullOrEmpty(f.SelectedBackground))
                    ApplyBackground(f.SelectedBackground);
            }
        }

        private void ResetLayout()
        {
            _layout = new MeterLayout();
            _layout.VisibleGadgets = new List<string>();
            foreach (GadgetCard c in _cards.Values) c.Visible = true;
            TopMost = false;
            BackgroundImage = null;
            _flow.BackgroundImage = null;
            if (_backgroundImage != null)
            {
                _backgroundImage.Dispose();
                _backgroundImage = null;
            }
            _layout.BackgroundImagePath = null;
            Size = new Size(840, 410);
        }

        private void StepVfo(bool a, int delta)
        {
            double current = a ? _adapter.VfoA : _adapter.VfoB;
            if (current <= 0.0) return;
            double step = (Control.ModifierKeys & Keys.Control) != 0 ? 0.001 :
                          (Control.ModifierKeys & Keys.Shift) != 0 ? 0.00001 : 0.0001;
            double next = current + (delta >= 0 ? step : -step);
            if (a) _adapter.SetVfoA(next); else _adapter.SetVfoB(next);
        }

        private void RefreshValues()
        {
            try
            {
                Dictionary<string, object> s = _adapter.Snapshot();
                double dbm = Convert.ToDouble(s["RX1_SIGNAL_DBM"], CultureInfo.InvariantCulture);
                Set("signal", dbm <= -199.0 ? "--" : dbm.ToString("0.0", CultureInfo.InvariantCulture) + " dBm");
                Set("sunit", Convert.ToString(s["RX1_SUNIT"], CultureInfo.InvariantCulture));
                Set("vfoa", FormatMHz(Convert.ToDouble(s["VFOA"], CultureInfo.InvariantCulture)));
                Set("vfob", FormatMHz(Convert.ToDouble(s["VFOB"], CultureInfo.InvariantCulture)));
                Set("band", Convert.ToString(s["BAND"], CultureInfo.InvariantCulture));
                Set("mode", Convert.ToString(s["MODE"], CultureInfo.InvariantCulture) + " / " +
                            Convert.ToString(s["FILTER"], CultureInfo.InvariantCulture));

                bool tx = Convert.ToBoolean(s["TX"], CultureInfo.InvariantCulture);
                bool on = Convert.ToBoolean(s["POWER_ON"], CultureInfo.InvariantCulture);
                Set("state", tx ? "TX" : (on ? "RX" : "OFF"));
                _cards["state"].Value.ForeColor = tx ? Color.OrangeRed : (on ? Color.LightGreen : Color.Gray);

                double pwr = Convert.ToDouble(s["TX_PWR"], CultureInfo.InvariantCulture);
                Set("power", tx ? pwr.ToString("0.0", CultureInfo.InvariantCulture) + " W" : "--");
                double swr = Convert.ToDouble(s["SWR"], CultureInfo.InvariantCulture);
                Set("swr", tx ? swr.ToString("0.00", CultureInfo.InvariantCulture) : "--");
                double mic = Convert.ToDouble(s["MIC"], CultureInfo.InvariantCulture);
                double alc = Convert.ToDouble(s["ALC"], CultureInfo.InvariantCulture);
                Set("mic", tx ? mic.ToString("0.0", CultureInfo.InvariantCulture) + " dB" : "--");
                Set("alc", tx ? alc.ToString("0.0", CultureInfo.InvariantCulture) + " dB" : "--");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Meters/Gadgets refresh: " + ex.Message);
            }
        }

        private void Set(string id, string text)
        {
            GadgetCard c;
            if (_cards.TryGetValue(id, out c)) c.Value.Text = String.IsNullOrEmpty(text) ? "--" : text;
        }

        private static string FormatMHz(double mhz)
        {
            if (mhz <= 0.0 || Double.IsNaN(mhz) || Double.IsInfinity(mhz)) return "--";
            long hz = (long)Math.Round(mhz * 1000000.0);
            return (hz / 1000000L).ToString(CultureInfo.InvariantCulture) + "." +
                   ((hz / 1000L) % 1000L).ToString("000", CultureInfo.InvariantCulture) + "." +
                   (hz % 1000L).ToString("000", CultureInfo.InvariantCulture);
        }

        private MeterLayout LoadLayout()
        {
            try
            {
                if (!File.Exists(_configPath)) return new MeterLayout();
                MeterLayout x = JsonConvert.DeserializeObject<MeterLayout>(File.ReadAllText(_configPath));
                return x ?? new MeterLayout();
            }
            catch { return new MeterLayout(); }
        }

        private void SaveLayout()
        {
            try
            {
                if (WindowState == FormWindowState.Normal)
                {
                    _layout.X = Left;
                    _layout.Y = Top;
                    _layout.Width = Width;
                    _layout.Height = Height;
                }
                _layout.TopMost = TopMost;
                SyncVisibleList();
                Directory.CreateDirectory(_configRoot);
                string temp = _configPath + ".tmp";
                File.WriteAllText(temp, JsonConvert.SerializeObject(_layout, Formatting.Indented));
                if (File.Exists(_configPath))
                {
                    string bak = _configPath + ".bak";
                    try { File.Replace(temp, _configPath, bak, true); }
                    catch
                    {
                        File.Copy(temp, _configPath, true);
                        File.Delete(temp);
                    }
                }
                else File.Move(temp, _configPath);
            }
            catch { }
        }

        internal static string GetConfigRoot(PowerSDR.Console console)
        {
            try
            {
                PropertyInfo p = console.GetType().GetProperty(
                    "AppDataPath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null)
                {
                    string s = p.GetValue(console, null) as string;
                    if (!String.IsNullOrEmpty(s))
                        return Path.Combine(s, "MetersGadgets");
                }
            }
            catch { }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "PowerSDR", "MetersGadgets");
        }
    }

    internal sealed class ThetisSkin
    {
        public string SkinName { get; set; }
        public string SkinUrl { get; set; }
        public string SkinVersion { get; set; }
        public string FromThetisVersion { get; set; }
        public string ThumbnailUrl { get; set; }
        public string SkinHomepageUrl { get; set; }
        public string DateReleased { get; set; }
        public string Overview { get; set; }
        public bool IsMeterSkin { get; set; }

        public override string ToString()
        {
            string v = String.IsNullOrEmpty(SkinVersion) ? "" : "  v" + SkinVersion;
            return (SkinName ?? "(unnamed)") + v;
        }
    }

    internal sealed class SkinsData
    {
        public string AuthorName { get; set; }
        public string AuthorCallsign { get; set; }
        public string AuthorNickname { get; set; }
        public string SkinsHomepageUrl { get; set; }
        public string DonateUrl { get; set; }
        public List<ThetisSkin> ThetisSkins { get; set; }
    }

    internal sealed class OE3IDESkinBrowser : Form
    {
        private const string CatalogueUrl =
            "https://www.oe3ide.com/wordp/wp-content/uploads/thetisskins/thetis_skins.json";

        private readonly PowerSDR.Console _console;
        private readonly ListBox _list;
        private readonly Label _status;
        private readonly TextBox _details;
        private readonly Button _download;
        private readonly Button _use;
        private string _lastExtractedFolder;

        public string SelectedBackground { get; private set; }

        public OE3IDESkinBrowser(PowerSDR.Console console)
        {
            _console = console;
            Text = "OE3IDE - Thetis meter skins";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 500);
            MinimumSize = new Size(620, 380);
            Font = new Font("Segoe UI", 9F);

            _status = new Label();
            _status.Dock = DockStyle.Top;
            _status.Height = 28;
            _status.Padding = new Padding(8, 7, 8, 0);
            _status.Text = "Loading OE3IDE catalogue...";

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 310;

            _list = new ListBox();
            _list.Dock = DockStyle.Fill;
            _list.SelectedIndexChanged += delegate { ShowDetails(); };
            split.Panel1.Controls.Add(_list);

            _details = new TextBox();
            _details.Dock = DockStyle.Fill;
            _details.Multiline = true;
            _details.ReadOnly = true;
            _details.ScrollBars = ScrollBars.Vertical;
            split.Panel2.Controls.Add(_details);

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 46;
            buttons.Padding = new Padding(7);
            buttons.FlowDirection = FlowDirection.RightToLeft;

            Button close = new Button();
            close.Text = "Close";
            close.AutoSize = true;
            close.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };

            _use = new Button();
            _use.Text = "Use extracted image";
            _use.AutoSize = true;
            _use.Enabled = false;
            _use.Click += delegate { SelectExtractedBackground(); };

            _download = new Button();
            _download.Text = "Download / extract";
            _download.AutoSize = true;
            _download.Enabled = false;
            _download.Click += async delegate { await DownloadSelectedAsync(); };

            buttons.Controls.Add(close);
            buttons.Controls.Add(_use);
            buttons.Controls.Add(_download);

            Controls.Add(split);
            Controls.Add(buttons);
            Controls.Add(_status);

            Shown += async delegate { await LoadCatalogueAsync(); };
        }

        private async Task LoadCatalogueAsync()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                using (WebClient wc = NewWebClient())
                {
                    string json = await wc.DownloadStringTaskAsync(
                        new Uri(CatalogueUrl + "?sq4kou=" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)));
                    SkinsData data = JsonConvert.DeserializeObject<SkinsData>(json);
                    _list.Items.Clear();

                    if (data != null && data.ThetisSkins != null)
                    {
                        foreach (ThetisSkin skin in data.ThetisSkins)
                            if (skin != null && skin.IsMeterSkin) _list.Items.Add(skin);
                    }

                    string author = data == null ? "OE3IDE" :
                        (!String.IsNullOrEmpty(data.AuthorCallsign) ? data.AuthorCallsign :
                         (!String.IsNullOrEmpty(data.AuthorName) ? data.AuthorName : "OE3IDE"));
                    _status.Text = author + " - " + _list.Items.Count.ToString(CultureInfo.InvariantCulture) +
                                   " meter skin package(s)";
                    _download.Enabled = _list.Items.Count > 0;
                    if (_list.Items.Count > 0) _list.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _status.Text = "Catalogue unavailable: " + ex.Message;
                _download.Enabled = false;
            }
        }

        private void ShowDetails()
        {
            ThetisSkin s = _list.SelectedItem as ThetisSkin;
            if (s == null)
            {
                _details.Text = "";
                _download.Enabled = false;
                return;
            }
            _download.Enabled = !String.IsNullOrEmpty(s.SkinUrl);
            _details.Text =
                "Name: " + (s.SkinName ?? "") + Environment.NewLine +
                "Version: " + (s.SkinVersion ?? "") + Environment.NewLine +
                "For Thetis: " + (s.FromThetisVersion ?? "") + Environment.NewLine +
                "Released: " + (s.DateReleased ?? "") + Environment.NewLine + Environment.NewLine +
                (s.Overview ?? "") + Environment.NewLine + Environment.NewLine +
                "Source: " + (s.SkinUrl ?? "");
        }

        private async Task DownloadSelectedAsync()
        {
            ThetisSkin s = _list.SelectedItem as ThetisSkin;
            if (s == null || String.IsNullOrEmpty(s.SkinUrl)) return;

            _download.Enabled = false;
            _use.Enabled = false;
            _status.Text = "Downloading " + (s.SkinName ?? "meter skin") + "...";

            try
            {
                string root = Path.Combine(MetersGadgetsForm.GetConfigRoot(_console), "OE3IDE");
                Directory.CreateDirectory(root);
                string safeName = MakeSafeFileName(String.IsNullOrEmpty(s.SkinName) ? "meter-skin" : s.SkinName);
                string zip = Path.Combine(root, safeName + ".zip");
                string dst = Path.Combine(root, safeName);

                using (WebClient wc = NewWebClient())
                    await wc.DownloadFileTaskAsync(new Uri(s.SkinUrl), zip);

                if (Directory.Exists(dst)) Directory.Delete(dst, true);
                Directory.CreateDirectory(dst);
                SafeExtractZip(zip, dst);
                _lastExtractedFolder = dst;

                string image = FindBestImage(dst);
                if (!String.IsNullOrEmpty(image))
                {
                    _status.Text = "Downloaded and extracted. Compatible image asset found.";
                    SelectedBackground = image;
                    _use.Enabled = true;
                }
                else
                {
                    _status.Text = "Downloaded and extracted. No supported PNG/JPG/BMP background found.";
                }
            }
            catch (Exception ex)
            {
                _status.Text = "Download failed: " + ex.Message;
            }
            finally
            {
                _download.Enabled = _list.SelectedItem is ThetisSkin;
            }
        }

        private void SelectExtractedBackground()
        {
            string image = FindBestImage(_lastExtractedFolder);
            if (String.IsNullOrEmpty(image))
            {
                MessageBox.Show(this, "No supported image asset was found in this package.",
                    "Meters / Gadgets", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedBackground = image;
            DialogResult = DialogResult.OK;
            Close();
        }

        private static WebClient NewWebClient()
        {
            WebClient wc = new WebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "PowerSDR-SQ4KOU-FLEX5000-MetersGadgets";
            wc.Headers[HttpRequestHeader.CacheControl] = "no-cache";
            return wc;
        }

        private static string MakeSafeFileName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value.Trim();
        }

        private static void SafeExtractZip(string zipPath, string destination)
        {
            string root = Path.GetFullPath(destination);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                root += Path.DirectorySeparatorChar;

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (String.IsNullOrEmpty(entry.FullName)) continue;
                    string target = Path.GetFullPath(Path.Combine(destination, entry.FullName));
                    if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Blocked unsafe path in meter skin package.");

                    if (entry.FullName.EndsWith("/", StringComparison.Ordinal) ||
                        entry.FullName.EndsWith("\\", StringComparison.Ordinal))
                    {
                        Directory.CreateDirectory(target);
                        continue;
                    }

                    string dir = Path.GetDirectoryName(target);
                    if (!String.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                    entry.ExtractToFile(target, true);
                }
            }
        }

        private static string FindBestImage(string folder)
        {
            if (String.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return null;
            string[] patterns = new string[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };
            List<string> files = new List<string>();
            foreach (string pattern in patterns)
            {
                try { files.AddRange(Directory.GetFiles(folder, pattern, SearchOption.AllDirectories)); }
                catch { }
            }
            if (files.Count == 0) return null;

            string[] preferred = new string[] { "background", "meter", "face", "main" };
            foreach (string key in preferred)
                foreach (string f in files)
                    if (Path.GetFileNameWithoutExtension(f).IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                        return f;

            return files[0];
        }
    }
}
