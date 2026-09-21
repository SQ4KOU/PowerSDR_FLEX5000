using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PowerSDR
{
    internal sealed class P26ThetisMetersConfigForm : Form
    {
        private readonly Console _console;
        private readonly ComboBox _containers;
        private readonly Button _addContainer;
        private readonly Button _removeContainer;
        private readonly CheckBox _highlight;
        private readonly CheckBox _border;
        private readonly Button _background;
        private readonly ListBox _available;
        private readonly ListBox _inUse;
        private readonly Button _addMeter;
        private readonly Button _removeMeter;
        private readonly Button _up;
        private readonly Button _down;
        private readonly PropertyGrid _settings;
        private readonly Button _save;
        private bool _loading;
        private bool _shutdown;

        private sealed class ContainerItem
        {
            internal string Id;
            internal string Text;
            public override string ToString() { return Text ?? ""; }
        }

        private sealed class MeterTypeItem
        {
            internal MeterType MeterType;
            internal int Order;

            internal MeterTypeItem(MeterType meterType, int order)
            {
                MeterType = meterType;
                Order = order;
            }

            public override string ToString()
            {
                return MeterManager.MeterName(MeterType);
            }
        }

        internal P26ThetisMetersConfigForm(Console console)
        {
            if (console == null) throw new ArgumentNullException("console");
            _console = console;

            Text = "PowerSDR - Meters/Gadgets";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(930, 560);
            ClientSize = new Size(1050, 650);
            ShowIcon = false;
            ShowInTaskbar = true;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(8);
            root.RowCount = 3;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(root);

            GroupBox containersBox = new GroupBox();
            containersBox.Text = "Meter containers";
            containersBox.Dock = DockStyle.Fill;
            root.Controls.Add(containersBox, 0, 0);

            _containers = new ComboBox();
            _containers.Name = "comboContainerSelect";
            _containers.DropDownStyle = ComboBoxStyle.DropDownList;
            _containers.Location = new Point(12, 26);
            _containers.Size = new Size(285, 24);
            _containers.SelectedIndexChanged += delegate { if (!_loading) LoadSelectedContainer(); };
            containersBox.Controls.Add(_containers);

            _addContainer = MakeButton("Add RX1 Container", 309, 23, 118, 30);
            _addContainer.Click += delegate { AddContainer(); };
            containersBox.Controls.Add(_addContainer);

            _removeContainer = MakeButton("Remove", 435, 23, 82, 30);
            _removeContainer.Click += delegate { RemoveContainer(); };
            containersBox.Controls.Add(_removeContainer);

            _highlight = new CheckBox();
            _highlight.Name = "chkContainerHighlight";
            _highlight.Text = "Highlight";
            _highlight.AutoSize = true;
            _highlight.Location = new Point(532, 29);
            _highlight.CheckedChanged += delegate
            {
                if (_loading) return;
                MeterManager.HighlightContainer(_highlight.Checked ? SelectedContainerId() : "");
            };
            containersBox.Controls.Add(_highlight);

            _border = new CheckBox();
            _border.Name = "chkContainerBorder";
            _border.Text = "Border";
            _border.AutoSize = true;
            _border.Location = new Point(620, 29);
            _border.CheckedChanged += delegate
            {
                if (_loading) return;
                string id = SelectedContainerId();
                if (id != "") MeterManager.ContainerBorder(id, _border.Checked);
            };
            containersBox.Controls.Add(_border);

            Label bgLabel = new Label();
            bgLabel.Text = "Background:";
            bgLabel.AutoSize = true;
            bgLabel.Location = new Point(704, 31);
            containersBox.Controls.Add(bgLabel);

            _background = MakeButton("", 782, 23, 48, 30);
            _background.Name = "clrbtnContainerBackground";
            _background.BackColor = Color.Black;
            _background.Click += delegate { ChooseContainerBackground(); };
            containersBox.Controls.Add(_background);

            Label scope = new Label();
            scope.AutoSize = true;
            scope.Location = new Point(12, 60);
            scope.Text = "RX1/FLEX-5000 scope. Meter model/rendering is the native Thetis core used by P25.";
            containersBox.Controls.Add(scope);

            TableLayoutPanel body = new TableLayoutPanel();
            body.Dock = DockStyle.Fill;
            body.Padding = new Padding(0, 8, 0, 4);
            body.ColumnCount = 3;
            body.RowCount = 1;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            root.Controls.Add(body, 0, 1);

            GroupBox availableBox = new GroupBox();
            availableBox.Text = "Available meters";
            availableBox.Dock = DockStyle.Fill;
            body.Controls.Add(availableBox, 0, 0);

            _available = MakeList();
            _available.Dock = DockStyle.Fill;
            _available.DoubleClick += delegate { AddMeter(); };
            _available.SelectedIndexChanged += delegate { UpdateButtons(); };
            availableBox.Controls.Add(_available);

            Panel availableButtons = new Panel();
            availableButtons.Dock = DockStyle.Bottom;
            availableButtons.Height = 42;
            availableBox.Controls.Add(availableButtons);
            _addMeter = MakeButton("Add  >", 8, 7, 92, 28);
            _addMeter.Click += delegate { AddMeter(); };
            availableButtons.Controls.Add(_addMeter);

            GroupBox inUseBox = new GroupBox();
            inUseBox.Text = "Meters in use";
            inUseBox.Dock = DockStyle.Fill;
            body.Controls.Add(inUseBox, 1, 0);

            _inUse = MakeList();
            _inUse.Dock = DockStyle.Fill;
            _inUse.DoubleClick += delegate { RemoveMeter(); };
            _inUse.SelectedIndexChanged += delegate
            {
                UpdateButtons();
                LoadMeterSettings();
            };
            inUseBox.Controls.Add(_inUse);

            Panel inUseButtons = new Panel();
            inUseButtons.Dock = DockStyle.Bottom;
            inUseButtons.Height = 42;
            inUseBox.Controls.Add(inUseButtons);

            _removeMeter = MakeButton("< Remove", 8, 7, 92, 28);
            _removeMeter.Click += delegate { RemoveMeter(); };
            inUseButtons.Controls.Add(_removeMeter);

            _up = MakeButton("Up", 108, 7, 62, 28);
            _up.Click += delegate { MoveMeter(-1); };
            inUseButtons.Controls.Add(_up);

            _down = MakeButton("Down", 176, 7, 62, 28);
            _down.Click += delegate { MoveMeter(1); };
            inUseButtons.Controls.Add(_down);

            GroupBox settingsBox = new GroupBox();
            settingsBox.Text = "Selected meter settings";
            settingsBox.Dock = DockStyle.Fill;
            body.Controls.Add(settingsBox, 2, 0);

            _settings = new PropertyGrid();
            _settings.Dock = DockStyle.Fill;
            _settings.HelpVisible = true;
            _settings.ToolbarVisible = true;
            settingsBox.Controls.Add(_settings);

            Panel footer = new Panel();
            footer.Dock = DockStyle.Fill;
            root.Controls.Add(footer, 0, 2);

            _save = MakeButton("Save meter configuration", 8, 7, 164, 28);
            _save.Click += delegate { _console.P26SaveMetersConfiguration(); };
            footer.Controls.Add(_save);

            Button refresh = MakeButton("Refresh", 180, 7, 80, 28);
            refresh.Click += delegate { RefreshAll(SelectedContainerId(), SelectedMeterType()); };
            footer.Controls.Add(refresh);

            Button close = MakeButton("Close", 268, 7, 80, 28);
            close.Click += delegate { Hide(); };
            footer.Controls.Add(close);

            FormClosing += OnFormClosing;
            VisibleChanged += OnVisibleChanged;
            Shown += delegate { RefreshAll("", MeterType.NONE); };
        }

        private static Button MakeButton(string text, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            return b;
        }

        private static ListBox MakeList()
        {
            ListBox list = new ListBox();
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.IntegralHeight = false;
            list.DrawItem += DrawMeterItem;
            return list;
        }

        private static void DrawMeterItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            ListBox list = sender as ListBox;
            if (list == null || e.Index < 0 || e.Index >= list.Items.Count) return;

            MeterTypeItem item = list.Items[e.Index] as MeterTypeItem;
            if (item != null)
            {
                Color strip = Color.PaleTurquoise;
                int type = MeterManager.GetMeterTXRXType(item.MeterType);
                if (type == 0) strip = Color.PaleGreen;
                else if (type == 1) strip = Color.PaleVioletRed;

                using (SolidBrush sb = new SolidBrush(strip))
                    e.Graphics.FillRectangle(sb, new Rectangle(e.Bounds.X, e.Bounds.Y, 5, e.Bounds.Height));
            }

            Color textColor = (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : SystemColors.ControlText;
            using (SolidBrush tb = new SolidBrush(textColor))
                e.Graphics.DrawString("  " + list.Items[e.Index].ToString(), e.Font, tb, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private string SelectedContainerId()
        {
            ContainerItem ci = _containers.SelectedItem as ContainerItem;
            return ci == null ? "" : ci.Id;
        }

        private MeterType SelectedMeterType()
        {
            MeterTypeItem mi = _inUse.SelectedItem as MeterTypeItem;
            return mi == null ? MeterType.NONE : mi.MeterType;
        }

        private MeterManager.clsMeter SelectedMeter()
        {
            string id = SelectedContainerId();
            return id == "" ? null : MeterManager.MeterFromId(id);
        }

        private void RefreshAll(string selectContainerId, MeterType selectMeter)
        {
            _loading = true;
            try
            {
                _containers.Items.Clear();
                int selectIndex = -1;
                int index = 0;

                foreach (var pair in MeterManager.MeterContainers)
                {
                    MeterManager.clsMeter meter = MeterManager.MeterFromId(pair.Key);
                    string meterName = meter == null || String.IsNullOrEmpty(meter.Name) ? "Meter" : meter.Name;
                    int rx = meter == null ? pair.Value.RX : meter.RX;
                    string shortId = pair.Key.Length > 5 ? pair.Key.Substring(0, 5).ToUpperInvariant() : pair.Key.ToUpperInvariant();

                    ContainerItem ci = new ContainerItem();
                    ci.Id = pair.Key;
                    ci.Text = meterName + " - RX" + rx.ToString() + " [" + shortId + "]";
                    _containers.Items.Add(ci);

                    if (pair.Key == selectContainerId) selectIndex = index;
                    index++;
                }

                if (_containers.Items.Count > 0)
                {
                    if (selectIndex < 0) selectIndex = 0;
                    _containers.SelectedIndex = selectIndex;
                }

                _addContainer.Enabled = MeterManager.TotalMeterContainers < 10;
            }
            finally
            {
                _loading = false;
            }

            LoadSelectedContainer();
            RefreshMeterLists(selectMeter);
        }

        private void LoadSelectedContainer()
        {
            string id = SelectedContainerId();
            bool enabled = id != "";

            _removeContainer.Enabled = enabled;
            _highlight.Enabled = enabled;
            _border.Enabled = enabled;
            _background.Enabled = enabled;

            _loading = true;
            try
            {
                if (enabled)
                {
                    _border.Checked = MeterManager.ContainerHasBorder(id);
                    _background.BackColor = MeterManager.GetContainerBackgroundColour(id);
                    if (_highlight.Checked) MeterManager.HighlightContainer(id);
                }
                else
                {
                    _highlight.Checked = false;
                    _border.Checked = false;
                    _background.BackColor = Color.Black;
                    MeterManager.HighlightContainer("");
                }
            }
            finally
            {
                _loading = false;
            }

            RefreshMeterLists(MeterType.NONE);
        }

        private void RefreshMeterLists(MeterType selectMeter)
        {
            MeterManager.clsMeter meter = SelectedMeter();

            _available.BeginUpdate();
            _inUse.BeginUpdate();
            try
            {
                _available.Items.Clear();
                _inUse.Items.Clear();

                if (meter != null)
                {
                    MeterTypeItem[] all = Enumerable.Range(1, (int)MeterType.LAST - 1)
                        .Select(n => (MeterType)n)
                        .Select(mt => new MeterTypeItem(mt, meter.HasMeterType(mt) ? meter.GetOrderForMeterType(mt) : -1))
                        .ToArray();

                    foreach (MeterTypeItem item in all.Where(x => x.Order < 0))
                        _available.Items.Add(item);

                    foreach (MeterTypeItem item in all.Where(x => x.Order >= 0).OrderBy(x => x.Order))
                    {
                        int idx = _inUse.Items.Add(item);
                        if (item.MeterType == selectMeter) _inUse.SelectedIndex = idx;
                    }
                }
            }
            finally
            {
                _available.EndUpdate();
                _inUse.EndUpdate();
            }

            _settings.SelectedObject = null;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            _addMeter.Enabled = _available.SelectedIndex >= 0 && SelectedMeter() != null;
            bool inUse = _inUse.SelectedIndex >= 0 && SelectedMeter() != null;
            _removeMeter.Enabled = inUse;
            _up.Enabled = inUse && _inUse.SelectedIndex > 0;
            _down.Enabled = inUse && _inUse.SelectedIndex < _inUse.Items.Count - 1;
        }

        private void AddContainer()
        {
            if (MeterManager.TotalMeterContainers >= 10) return;
            string id = MeterManager.AddMeterContainer(1, false, _console.MOX);
            RefreshAll(id, MeterType.NONE);
        }

        private void RemoveContainer()
        {
            string id = SelectedContainerId();
            if (id == "") return;
            MeterManager.RemoveMeterContainer(id);
            MeterManager.HighlightContainer("");
            RefreshAll("", MeterType.NONE);
        }

        private void ChooseContainerBackground()
        {
            string id = SelectedContainerId();
            if (id == "") return;

            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.FullOpen = true;
                dlg.Color = MeterManager.GetContainerBackgroundColour(id);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                MeterManager.ContainerBackgroundColour(id, dlg.Color);
                _background.BackColor = dlg.Color;
            }
        }

        private void AddMeter()
        {
            MeterTypeItem item = _available.SelectedItem as MeterTypeItem;
            MeterManager.clsMeter meter = SelectedMeter();
            if (item == null || meter == null) return;

            meter.AddMeter(item.MeterType);
            meter.Rebuild();
            RefreshMeterLists(item.MeterType);
        }

        private void RemoveMeter()
        {
            MeterTypeItem item = _inUse.SelectedItem as MeterTypeItem;
            MeterManager.clsMeter meter = SelectedMeter();
            if (item == null || meter == null) return;

            meter.RemoveMeterType(item.MeterType, true);
            RefreshMeterLists(MeterType.NONE);
        }

        private void MoveMeter(int delta)
        {
            MeterTypeItem item = _inUse.SelectedItem as MeterTypeItem;
            MeterManager.clsMeter meter = SelectedMeter();
            if (item == null || meter == null) return;

            int target = _inUse.SelectedIndex + delta;
            if (target < 0 || target >= _inUse.Items.Count) return;

            meter.SetOrderForMeterType(item.MeterType, target, true, delta < 0);
            RefreshMeterLists(item.MeterType);
        }

        private void LoadMeterSettings()
        {
            MeterTypeItem item = _inUse.SelectedItem as MeterTypeItem;
            MeterManager.clsMeter meter = SelectedMeter();
            if (item == null || meter == null)
            {
                _settings.SelectedObject = null;
                return;
            }

            MeterManager.clsIGSettings settings = meter.GetSettingsForMeterGroup(item.MeterType);
            _settings.SelectedObject = settings == null ? null : new MeterSettingsProxy(meter, item.MeterType, settings);
        }

        private void OnVisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                _highlight.Checked = false;
                MeterManager.HighlightContainer("");
                return;
            }
            if (!_shutdown)
                RefreshAll(SelectedContainerId(), SelectedMeterType());
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_shutdown && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            MeterManager.HighlightContainer("");
        }

        internal void CloseForShutdown()
        {
            _shutdown = true;
            try { Close(); } catch { }
            try { Dispose(); } catch { }
        }

        private sealed class MeterSettingsProxy
        {
            private readonly MeterManager.clsMeter _meter;
            private readonly MeterType _type;
            private readonly MeterManager.clsIGSettings _s;

            internal MeterSettingsProxy(MeterManager.clsMeter meter, MeterType type, MeterManager.clsIGSettings settings)
            {
                _meter = meter;
                _type = type;
                _s = settings;
            }

            private void Apply()
            {
                _meter.ApplySettingsForMeterGroup(_type, _s);
            }

            [Category("Identity"), DisplayName("Meter type"), ReadOnly(true)]
            public string MeterTypeName { get { return MeterManager.MeterName(_type); } }

            [Category("Timing"), DisplayName("Update interval (ms)")]
            public int UpdateInterval { get { return _s.UpdateInterval; } set { _s.UpdateInterval = Math.Max(1, value); Apply(); } }

            [Category("Timing"), DisplayName("Attack ratio")]
            public float AttackRatio { get { return _s.AttackRatio; } set { _s.AttackRatio = Clamp01(value); Apply(); } }

            [Category("Timing"), DisplayName("Decay ratio")]
            public float DecayRatio { get { return _s.DecayRatio; } set { _s.DecayRatio = Clamp01(value); Apply(); } }

            [Category("Colours"), DisplayName("Low colour")]
            public Color LowColor { get { return _s.LowColor; } set { _s.LowColor = Color.FromArgb(255, value); Apply(); } }

            [Category("Colours"), DisplayName("High colour")]
            public Color HighColor { get { return _s.HighColor; } set { _s.HighColor = Color.FromArgb(255, value); Apply(); } }

            [Category("Colours"), DisplayName("Indicator colour")]
            public Color IndicatorColor { get { return _s.MarkerColour; } set { _s.MarkerColour = Color.FromArgb(255, value); Apply(); } }

            [Category("Colours"), DisplayName("Bar/background colour")]
            public Color MainColor { get { return _s.Colour; } set { _s.Colour = Color.FromArgb(255, value); Apply(); } }

            [Category("History"), DisplayName("Show history")]
            public bool ShowHistory { get { return _s.ShowHistory; } set { _s.ShowHistory = value; Apply(); } }

            [Category("History"), DisplayName("History colour")]
            public Color HistoryColor
            {
                get { return Color.FromArgb(255, _s.HistoryColor); }
                set { _s.HistoryColor = Color.FromArgb(_s.HistoryColor.A, value); Apply(); }
            }

            [Category("History"), DisplayName("History alpha")]
            public int HistoryAlpha
            {
                get { return _s.HistoryColor.A; }
                set { _s.HistoryColor = Color.FromArgb(Math.Max(0, Math.Min(255, value)), _s.HistoryColor); Apply(); }
            }

            [Category("History"), DisplayName("History duration (ms)")]
            public int HistoryDuration { get { return _s.HistoryDuration; } set { _s.HistoryDuration = Math.Max(0, value); Apply(); } }

            [Category("Peak"), DisplayName("Peak hold")]
            public bool PeakHold { get { return _s.PeakHold; } set { _s.PeakHold = value; Apply(); } }

            [Category("Peak"), DisplayName("Peak hold colour")]
            public Color PeakHoldColor { get { return _s.PeakHoldMarkerColor; } set { _s.PeakHoldMarkerColor = Color.FromArgb(255, value); Apply(); } }

            [Category("Peak"), DisplayName("Show peak value")]
            public bool PeakValue { get { return _s.PeakValue; } set { _s.PeakValue = value; Apply(); } }

            [Category("Peak"), DisplayName("Peak value colour")]
            public Color PeakValueColor { get { return _s.PeakValueColour; } set { _s.PeakValueColour = Color.FromArgb(255, value); Apply(); } }

            [Category("Display"), DisplayName("Segmented")]
            public bool Segmented
            {
                get { return _s.BarStyle == MeterManager.clsBarItem.BarStyle.Segments; }
                set { _s.BarStyle = value ? MeterManager.clsBarItem.BarStyle.Segments : MeterManager.clsBarItem.BarStyle.Line; Apply(); }
            }

            [Category("Display"), DisplayName("Segment colour")]
            public Color SegmentedColor { get { return _s.SegmentedColour; } set { _s.SegmentedColour = value; Apply(); } }

            [Category("Display"), DisplayName("Shadow")]
            public bool Shadow { get { return _s.Shadow; } set { _s.Shadow = value; Apply(); } }

            [Category("Display"), DisplayName("Fade on RX")]
            public bool FadeOnRx { get { return _s.FadeOnRx; } set { _s.FadeOnRx = value; Apply(); } }

            [Category("Display"), DisplayName("Fade on TX")]
            public bool FadeOnTx { get { return _s.FadeOnTx; } set { _s.FadeOnTx = value; Apply(); } }

            [Category("Title"), DisplayName("Show meter title")]
            public bool ShowTitle { get { return _s.ShowType; } set { _s.ShowType = value; Apply(); } }

            [Category("Title"), DisplayName("Title colour")]
            public Color TitleColor { get { return _s.TitleColor; } set { _s.TitleColor = value; Apply(); } }

            [Category("Special"), DisplayName("Magic Eye scale")]
            public float EyeScale { get { return _s.EyeScale; } set { _s.EyeScale = Math.Max(0.01f, Math.Min(1.0f, value)); Apply(); } }

            [Category("Special"), DisplayName("Average signal")]
            public bool Average { get { return _s.Average; } set { _s.Average = value; Apply(); } }

            [Category("Special"), DisplayName("Dark mode")]
            public bool DarkMode { get { return _s.DarkMode; } set { _s.DarkMode = value; Apply(); } }

            private static float Clamp01(float value)
            {
                if (value < 0f) return 0f;
                if (value > 1f) return 1f;
                return value;
            }
        }
    }

    sealed unsafe public partial class Console
    {
        private P26ThetisMetersConfigForm p26MetersConfigForm;

        internal void P26ShowMetersConfig()
        {
            if (p26MetersConfigForm == null || p26MetersConfigForm.IsDisposed)
                p26MetersConfigForm = new P26ThetisMetersConfigForm(this);

            if (!p26MetersConfigForm.Visible)
                p26MetersConfigForm.Show(this);
            else
            {
                p26MetersConfigForm.WindowState = FormWindowState.Normal;
                p26MetersConfigForm.BringToFront();
                p26MetersConfigForm.Activate();
            }
        }

        internal void P26SaveMetersConfiguration()
        {
            P25SaveThetisMeters();
        }

        internal void P26CloseMetersConfig()
        {
            P26ThetisMetersConfigForm form = p26MetersConfigForm;
            p26MetersConfigForm = null;
            if (form != null && !form.IsDisposed)
                form.CloseForShutdown();
        }
    }
}
