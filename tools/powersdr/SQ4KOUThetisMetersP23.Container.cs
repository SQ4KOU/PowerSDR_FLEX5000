// SQ4KOU P23 - Thetis-like meter container + renderer
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace PowerSDR
{
    internal sealed class P23MeterFloatForm : Form
    {
        private readonly P23MeterContainer _container;

        internal P23MeterFloatForm(P23MeterContainer container)
        {
            _container = container;
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Black;
            ClientSize = new Size(400, 200);
            FormBorderStyle = FormBorderStyle.None;
            MinimizeBox = false;
            MinimumSize = new Size(100, 32);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "PowerSDR Meter [" + ShortId(container.Config.Id) + "]";
            FormClosing += OnClosing;
            Resize += delegate
            {
                if (_container != null && !_container.IsDisposed && WindowState == FormWindowState.Normal)
                    _container.Size = ClientSize;
            };
        }

        internal bool ContainerMinimises { get; set; }

        protected override bool ShowWithoutActivation
        {
            get { return false; }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && _container != null && !_container.IsDisposed)
            {
                e.Cancel = true;
                Hide();
                _container.Config.Enabled = false;
                _container.NotifyChanged();
            }
        }

        private static string ShortId(string id)
        {
            if (String.IsNullOrEmpty(id)) return "00000";
            return id.Substring(0, Math.Min(5, id.Length)).ToUpperInvariant();
        }
    }

    internal sealed class P23MeterContainer : UserControl
    {
        private const int BarHeight = 24;
        private readonly P23RadioAdapter _adapter;
        private readonly Panel _bar;
        private readonly Label _title;
        private readonly Button _btnFloat;
        private readonly Button _btnPin;
        private readonly Button _btnSettings;
        private readonly Label _grip;
        private readonly P23MeterSurface _surface;
        private P23MeterFloatForm _floatForm;
        private bool _dragging;
        private bool _resizing;
        private Point _dragStart;
        private Point _sizeStart;
        private Point _screenStart;
        private bool _highlight;
        private bool _runtimeVisible = true;

        internal event EventHandler SettingsClicked;
        internal event EventHandler ConfigurationChanged;

        internal P23ContainerConfig Config { get; private set; }

        internal P23MeterContainer(P23ContainerConfig config, P23RadioAdapter adapter)
        {
            Config = config;
            _adapter = adapter;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(Config.BackColorArgb);
            MinimumSize = new Size(100, 32);
            Size = new Size(Math.Max(100, Config.Width), Math.Max(32, Config.Height));

            _surface = new P23MeterSurface(this, adapter);
            _surface.Dock = DockStyle.Fill;
            _surface.BackColor = BackColor;
            _surface.MouseMove += ChildMouseMove;
            _surface.MouseLeave += ChildMouseLeave;

            _bar = new Panel();
            _bar.Height = BarHeight;
            _bar.Dock = DockStyle.Top;
            _bar.BackColor = Color.DimGray;
            _bar.Visible = false;
            _bar.MouseDown += BarMouseDown;
            _bar.MouseMove += BarMouseMove;
            _bar.MouseUp += BarMouseUp;
            _bar.MouseLeave += BarMouseLeave;

            _title = new Label();
            _title.AutoSize = false;
            _title.TextAlign = ContentAlignment.MiddleLeft;
            _title.ForeColor = Color.White;
            _title.BackColor = Color.Transparent;
            _title.Dock = DockStyle.Fill;
            _title.Padding = new Padding(5, 0, 0, 0);
            _title.MouseDown += BarMouseDown;
            _title.MouseMove += BarMouseMove;
            _title.MouseUp += BarMouseUp;

            _btnSettings = MakeBarButton("S", "Container settings");
            _btnSettings.Dock = DockStyle.Right;
            _btnSettings.Click += delegate
            {
                EventHandler h = SettingsClicked;
                if (h != null) h(this, EventArgs.Empty);
            };

            _btnPin = MakeBarButton("P", "Always on top");
            _btnPin.Dock = DockStyle.Right;
            _btnPin.Click += delegate
            {
                Config.PinOnTop = !Config.PinOnTop;
                ApplyTopMost();
                SetBarState();
                NotifyChanged();
            };

            _btnFloat = MakeBarButton("D", "Float / dock");
            _btnFloat.Dock = DockStyle.Right;
            _btnFloat.Click += delegate { ToggleFloatDock(); };

            _bar.Controls.Add(_title);
            _bar.Controls.Add(_btnSettings);
            _bar.Controls.Add(_btnPin);
            _bar.Controls.Add(_btnFloat);

            _grip = new Label();
            _grip.AutoSize = false;
            _grip.Size = new Size(18, 18);
            _grip.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _grip.Text = "///";
            _grip.TextAlign = ContentAlignment.BottomRight;
            _grip.ForeColor = Color.Silver;
            _grip.BackColor = Color.Transparent;
            _grip.Cursor = Cursors.SizeNWSE;
            _grip.Visible = false;
            _grip.MouseDown += GripMouseDown;
            _grip.MouseMove += GripMouseMove;
            _grip.MouseUp += GripMouseUp;

            Controls.Add(_surface);
            Controls.Add(_bar);
            Controls.Add(_grip);
            _bar.BringToFront();
            _grip.BringToFront();

            MouseMove += ChildMouseMove;
            MouseLeave += ChildMouseLeave;
            Resize += delegate
            {
                _grip.Location = new Point(Math.Max(0, ClientSize.Width - _grip.Width),
                                           Math.Max(0, ClientSize.Height - _grip.Height));
                _surface.Invalidate();
            };

            SetBarState();
            UpdateTitle();
            ApplyStyle();
        }

        private static Button MakeBarButton(string text, string tip)
        {
            Button b = new Button();
            b.Width = 26;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Text = text;
            b.ForeColor = Color.White;
            b.BackColor = Color.Transparent;
            b.TabStop = false;
            ToolTip tt = new ToolTip();
            tt.SetToolTip(b, tip);
            return b;
        }

        internal void AttachAccordingToConfig()
        {
            if (Config.Floating) FloatContainer();
            else DockContainer();
            RuntimeUpdate();
        }

        internal void CaptureGeometry()
        {
            try
            {
                Config.Width = Width;
                Config.Height = Height;
                if (Config.Floating && _floatForm != null && !_floatForm.IsDisposed)
                {
                    Config.FloatX = _floatForm.Left;
                    Config.FloatY = _floatForm.Top;
                    Config.Width = _floatForm.ClientSize.Width;
                    Config.Height = _floatForm.ClientSize.Height;
                }
                else
                {
                    Config.DockedX = Left;
                    Config.DockedY = Top;
                }
            }
            catch { }
        }

        internal void RuntimeUpdate()
        {
            bool show = Config.Enabled;
            if (show)
            {
                if (_adapter.IsTx) show = Config.ShowOnTX;
                else show = Config.ShowOnRX;
            }
            if (show && Config.HideWhenRxNotUsed && Config.RX == 2)
            {
                object rx2 = _adapter.ReadMember("RX2Enabled");
                if (rx2 != null)
                {
                    try { show = Convert.ToBoolean(rx2, CultureInfo.InvariantCulture); }
                    catch { }
                }
            }

            if (show != _runtimeVisible)
            {
                _runtimeVisible = show;
                ApplyVisibility();
            }
            else ApplyVisibility();

            UpdateTitle();
            if (Config.AutoHeight) ApplyAutoHeight();
            _surface.RuntimeUpdate();

            if (_floatForm != null && !_floatForm.IsDisposed && Config.ContainerMinimises)
            {
                Form owner = P23MeterManager.Console;
                if (owner != null && owner.WindowState == FormWindowState.Minimized)
                {
                    if (_floatForm.WindowState != FormWindowState.Minimized)
                        _floatForm.WindowState = FormWindowState.Minimized;
                }
                else if (_floatForm.WindowState == FormWindowState.Minimized)
                    _floatForm.WindowState = FormWindowState.Normal;
            }
        }

        internal void SetHighlight(bool value)
        {
            _highlight = value;
            Invalidate();
            _surface.Invalidate();
        }

        internal void NotifyChanged()
        {
            ApplyStyle();
            EventHandler h = ConfigurationChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        internal void DestroyContainer()
        {
            try
            {
                if (_floatForm != null)
                {
                    P23MeterFloatForm f = _floatForm;
                    _floatForm = null;
                    if (!f.IsDisposed) f.Dispose();
                }
                if (Parent != null) Parent.Controls.Remove(this);
                Dispose();
            }
            catch { }
        }

        internal void RecoverToConsole()
        {
            Config.Floating = false;
            Config.Enabled = true;
            DockContainer();
            NotifyChanged();
        }

        internal void ToggleFloatDock()
        {
            if (Config.Locked) return;
            CaptureGeometry();
            if (Config.Floating) DockContainer();
            else FloatContainer();
            NotifyChanged();
        }

        private void DockContainer()
        {
            Config.Floating = false;
            Form c = P23MeterManager.Console;
            if (c == null || c.IsDisposed) return;

            if (_floatForm != null && !_floatForm.IsDisposed)
            {
                try { _floatForm.Controls.Remove(this); } catch { }
                _floatForm.Hide();
                _floatForm.Dispose();
                _floatForm = null;
            }

            if (Parent != null) Parent.Controls.Remove(this);
            c.Controls.Add(this);
            Location = new Point(Math.Max(0, Config.DockedX), Math.Max(0, Config.DockedY));
            Size = new Size(Math.Max(MinimumSize.Width, Config.Width),
                            Math.Max(MinimumSize.Height, Config.Height));
            BringToFront();
            SetBarState();
            ApplyVisibility();
        }

        private void FloatContainer()
        {
            Config.Floating = true;
            if (Parent != null) Parent.Controls.Remove(this);

            if (_floatForm == null || _floatForm.IsDisposed)
                _floatForm = new P23MeterFloatForm(this);

            _floatForm.ContainerMinimises = Config.ContainerMinimises;
            _floatForm.Location = ForceOnScreen(new Point(Config.FloatX, Config.FloatY),
                                                new Size(Math.Max(100, Config.Width), Math.Max(32, Config.Height)));
            _floatForm.ClientSize = new Size(Math.Max(100, Config.Width), Math.Max(32, Config.Height));
            _floatForm.TopMost = Config.PinOnTop;
            _floatForm.Controls.Add(this);
            Dock = DockStyle.Fill;
            _floatForm.Show(P23MeterManager.Console);
            SetBarState();
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            bool visible = _runtimeVisible && Config.Enabled;
            if (Config.Floating)
            {
                if (_floatForm != null && !_floatForm.IsDisposed)
                {
                    if (visible && !_floatForm.Visible) _floatForm.Show(P23MeterManager.Console);
                    else if (!visible && _floatForm.Visible) _floatForm.Hide();
                }
            }
            else Visible = visible;
        }

        private static Point ForceOnScreen(Point p, Size s)
        {
            Rectangle union = Rectangle.Empty;
            Screen[] screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
                union = union.IsEmpty ? screens[i].WorkingArea : Rectangle.Union(union, screens[i].WorkingArea);
            if (union.IsEmpty) return p;
            int x = p.X;
            int y = p.Y;
            if (x < union.Left) x = union.Left;
            if (y < union.Top) y = union.Top;
            if (x + s.Width > union.Right) x = Math.Max(union.Left, union.Right - s.Width);
            if (y + s.Height > union.Bottom) y = Math.Max(union.Top, union.Bottom - s.Height);
            return new Point(x, y);
        }

        private void SetBarState()
        {
            _btnFloat.Text = Config.Floating ? "D" : "F";
            _btnPin.Visible = Config.Floating;
            _btnPin.BackColor = Config.PinOnTop ? Color.DarkOrange : Color.Transparent;
        }

        private void ApplyTopMost()
        {
            if (_floatForm != null && !_floatForm.IsDisposed)
                _floatForm.TopMost = Config.PinOnTop;
        }

        private void ApplyStyle()
        {
            BackColor = Color.FromArgb(Config.BackColorArgb);
            _surface.BackColor = BackColor;
            BorderStyle = Config.Border ? BorderStyle.FixedSingle : BorderStyle.None;
            if (_floatForm != null && !_floatForm.IsDisposed)
            {
                _floatForm.BackColor = BackColor;
                _floatForm.TopMost = Config.PinOnTop;
                _floatForm.ContainerMinimises = Config.ContainerMinimises;
            }
            SetBarState();
            UpdateTitle();
            _surface.Invalidate();
        }

        private void UpdateTitle()
        {
            string prefix = _adapter.IsTx ? "TX" : "RX";
            string note = FirstLine(Config.Notes);
            _title.Text = prefix + Config.RX.ToString(CultureInfo.InvariantCulture) +
                (String.IsNullOrEmpty(note) ? "" : " " + note);
        }

        private static string FirstLine(string s)
        {
            if (String.IsNullOrEmpty(s)) return "";
            string[] p = s.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return p.Length == 0 ? "" : p[0];
        }

        private void ApplyAutoHeight()
        {
            int h = _surface.RequiredHeight + (Config.NoControls ? 0 : 0);
            h = Math.Max(32, h);
            if (Height != h)
            {
                Height = h;
                if (_floatForm != null && !_floatForm.IsDisposed)
                    _floatForm.ClientSize = new Size(_floatForm.ClientSize.Width, h);
                Config.Height = h;
            }
        }

        private void ChildMouseMove(object sender, MouseEventArgs e)
        {
            Point pt = PointToClient(Control.MousePosition);
            bool allow = !Config.NoControls || (Control.ModifierKeys & Keys.Shift) != 0;
            bool overBar = pt.Y <= BarHeight + 2;
            if (allow && overBar && !_bar.Visible)
            {
                _bar.Visible = true;
                _bar.BringToFront();
            }
            else if ((!allow || !overBar) && _bar.Visible && !_dragging)
                _bar.Visible = false;

            bool overGrip = pt.X >= ClientSize.Width - 24 && pt.Y >= ClientSize.Height - 24;
            _grip.Visible = allow && overGrip;
            if (_grip.Visible) _grip.BringToFront();
        }

        private void ChildMouseLeave(object sender, EventArgs e)
        {
            if (_dragging || _resizing) return;
            Point p = PointToClient(Control.MousePosition);
            if (!ClientRectangle.Contains(p))
            {
                _bar.Visible = false;
                _grip.Visible = false;
            }
        }

        private void BarMouseLeave(object sender, EventArgs e)
        {
            ChildMouseLeave(sender, e);
        }

        private void BarMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || Config.Locked) return;
            _dragging = true;
            _screenStart = Control.MousePosition;
            if (Config.Floating && _floatForm != null)
                _dragStart = _floatForm.Location;
            else _dragStart = Location;
            Capture = true;
        }

        private void BarMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging || Config.Locked) return;
            Point now = Control.MousePosition;
            Point p = new Point(_dragStart.X + now.X - _screenStart.X,
                                _dragStart.Y + now.Y - _screenStart.Y);
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                p.X = Round10(p.X);
                p.Y = Round10(p.Y);
            }

            if (Config.Floating && _floatForm != null)
                _floatForm.Location = p;
            else
            {
                Control parent = Parent;
                if (parent != null)
                {
                    p.X = Math.Max(0, Math.Min(p.X, Math.Max(0, parent.ClientSize.Width - Width)));
                    p.Y = Math.Max(0, Math.Min(p.Y, Math.Max(0, parent.ClientSize.Height - Height)));
                    Location = p;
                    BringToFront();
                }
            }
        }

        private void BarMouseUp(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            _dragging = false;
            Capture = false;
            CaptureGeometry();
            NotifyChanged();
        }

        private void GripMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || Config.Locked) return;
            _resizing = true;
            _screenStart = Control.MousePosition;
            _sizeStart = new Point(Width, Height);
            Capture = true;
        }

        private void GripMouseMove(object sender, MouseEventArgs e)
        {
            if (!_resizing || Config.Locked) return;
            Point now = Control.MousePosition;
            int w = Math.Max(MinimumSize.Width, _sizeStart.X + now.X - _screenStart.X);
            int h = Math.Max(MinimumSize.Height, _sizeStart.Y + now.Y - _screenStart.Y);
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                w = Math.Max(MinimumSize.Width, Round10(w));
                h = Math.Max(MinimumSize.Height, Round10(h));
            }
            Size = new Size(w, h);
            if (_floatForm != null && Config.Floating) _floatForm.ClientSize = Size;
        }

        private void GripMouseUp(object sender, MouseEventArgs e)
        {
            if (!_resizing) return;
            _resizing = false;
            Capture = false;
            CaptureGeometry();
            NotifyChanged();
        }

        private static int Round10(int v)
        {
            return (int)Math.Round(v / 10.0, MidpointRounding.AwayFromZero) * 10;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_highlight)
            {
                using (Pen p = new Pen(Color.Yellow, 3f))
                    e.Graphics.DrawRectangle(p, 1, 1, Math.Max(1, Width - 3), Math.Max(1, Height - 3));
            }
        }
    }

    internal sealed class P23MeterSurface : Control
    {
        private readonly P23MeterContainer _owner;
        private readonly P23RadioAdapter _adapter;
        private readonly Dictionary<string, Queue<double>> _history =
            new Dictionary<string, Queue<double>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Image> _webImages =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _webPending =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, object> _snapshot =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private int _requiredHeight;
        private Image _backgroundImage;
        private string _backgroundPath = "";

        internal int RequiredHeight { get { return Math.Max(32, _requiredHeight); } }

        internal P23MeterSurface(P23MeterContainer owner, P23RadioAdapter adapter)
        {
            _owner = owner;
            _adapter = adapter;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            MouseWheel += OnWheel;
            MouseDown += OnMouseDownItem;
        }

        internal void RuntimeUpdate()
        {
            try
            {
                _snapshot = _adapter.Snapshot();
                Dictionary<string, object> io = P23MultiMeterIO.Snapshot();
                foreach (KeyValuePair<string, object> kv in io) _snapshot[kv.Key] = kv.Value;

                List<P23MeterItemConfig> items = _owner.Config.Items;
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] == null || !items[i].Enabled) continue;
                        if (items[i].Type == P23MeterItemType.HISTORY)
                        {
                            double v = ReadNumeric(items[i].Source, _adapter.Signal);
                            Queue<double> q;
                            if (!_history.TryGetValue(items[i].Id, out q))
                            {
                                q = new Queue<double>();
                                _history[items[i].Id] = q;
                            }
                            q.Enqueue(v);
                            while (q.Count > 240) q.Dequeue();
                        }
                        else if (items[i].Type == P23MeterItemType.WEB_IMAGE)
                            EnsureWebImage(items[i]);
                    }
                }
            }
            catch { }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(_owner.Config.BackColorArgb));
            EnsureBackgroundImage();
            if (_backgroundImage != null)
                g.DrawImage(_backgroundImage, ClientRectangle);

            int y = 0;
            List<P23MeterItemConfig> items = _owner.Config.Items;
            if (items == null) items = new List<P23MeterItemConfig>();
            for (int i = 0; i < items.Count; i++)
            {
                P23MeterItemConfig item = items[i];
                if (item == null || !item.Enabled) continue;
                int h = Math.Max(12, item.Height);
                Rectangle r = new Rectangle(0, y, Math.Max(1, ClientSize.Width), h);
                DrawItem(g, item, r);
                y += h;
            }
            _requiredHeight = y;
        }

        private void EnsureBackgroundImage()
        {
            string path = _owner.Config.BackgroundImagePath ?? "";
            if (String.Equals(path, _backgroundPath, StringComparison.OrdinalIgnoreCase)) return;
            _backgroundPath = path;
            if (_backgroundImage != null)
            {
                try { _backgroundImage.Dispose(); } catch { }
                _backgroundImage = null;
            }
            if (String.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image src = Image.FromStream(fs))
                    _backgroundImage = new Bitmap(src);
            }
            catch { _backgroundImage = null; }
        }

        private void DrawItem(Graphics g, P23MeterItemConfig item, Rectangle r)
        {
            Color fore = Color.FromArgb(item.ForeColorArgb);
            Color back = Color.FromArgb(item.BackColorArgb);
            using (SolidBrush b = new SolidBrush(back)) g.FillRectangle(b, r);

            switch (item.Type)
            {
                case P23MeterItemType.SIGNAL_STRENGTH:
                    DrawMeterBar(g, r, "SIGNAL", _adapter.Signal, item.Minimum, item.Maximum, " dBm", fore, item);
                    break;
                case P23MeterItemType.AVG_SIGNAL_STRENGTH:
                    DrawMeterBar(g, r, "SIG AVG", _adapter.AverageSignal, item.Minimum, item.Maximum, " dBm", fore, item);
                    break;
                case P23MeterItemType.SIGNAL_TEXT:
                    DrawCentered(g, r, _adapter.Signal.ToString("0.0", CultureInfo.InvariantCulture) +
                        " dBm   " + _adapter.SUnit(_adapter.Signal), fore, 16f, FontStyle.Bold);
                    break;
                case P23MeterItemType.ADC:
                    DrawMeterBar(g, r, "ADC", Math.Max(Math.Abs(_adapter.AdcReal), Math.Abs(_adapter.AdcImag)),
                        item.Minimum == -140 ? 0 : item.Minimum, item.Maximum == -20 ? 1 : item.Maximum, "", fore, item);
                    break;
                case P23MeterItemType.AGC:
                case P23MeterItemType.AGC_GAIN:
                    DrawMeterBar(g, r, "AGC", _adapter.AgcGain,
                        item.Minimum == -140 ? -20 : item.Minimum, item.Maximum == -20 ? 120 : item.Maximum, " dB", fore, item);
                    break;
                case P23MeterItemType.MIC:
                case P23MeterItemType.EQ:
                case P23MeterItemType.LEVELER:
                case P23MeterItemType.LEVELER_GAIN:
                case P23MeterItemType.ALC:
                case P23MeterItemType.ALC_GAIN:
                case P23MeterItemType.COMP:
                    DrawMeterBar(g, r, P23MeterItemNames.Display(item.Type).ToUpperInvariant(),
                        _adapter.TxMeter(item.Type), item.Minimum == -140 ? -60 : item.Minimum,
                        item.Maximum == -20 ? 20 : item.Maximum, " dB", fore, item);
                    break;
                case P23MeterItemType.PWR:
                    DrawMeterBar(g, r, "PWR", _adapter.ForwardPower, 0,
                        item.Maximum <= 0 ? 100 : item.Maximum, " W", fore, item);
                    break;
                case P23MeterItemType.REVERSE_PWR:
                    DrawMeterBar(g, r, "REV", _adapter.ReversePower, 0,
                        item.Maximum <= 0 ? 25 : item.Maximum, " W", fore, item);
                    break;
                case P23MeterItemType.SWR:
                    DrawMeterBar(g, r, "SWR", _adapter.SWR, 1,
                        item.Maximum <= 1 ? 5 : item.Maximum, "", fore, item);
                    break;
                case P23MeterItemType.MAGIC_EYE:
                    DrawMagicEye(g, r, _adapter.Signal, fore);
                    break;
                case P23MeterItemType.ANANMM:
                    DrawAnanMeter(g, r, fore);
                    break;
                case P23MeterItemType.CROSS:
                    DrawCross(g, r, fore);
                    break;
                case P23MeterItemType.VFO_DISPLAY:
                case P23MeterItemType.DIAL_DISPLAY:
                    DrawVfo(g, r, fore);
                    break;
                case P23MeterItemType.CLOCK:
                    DrawClock(g, r, fore);
                    break;
                case P23MeterItemType.SPACER:
                    break;
                case P23MeterItemType.TEXT_OVERLAY:
                    DrawTextOverlay(g, r, item, fore);
                    break;
                case P23MeterItemType.DATA_OUT:
                    DrawDataOut(g, r, item, fore);
                    break;
                case P23MeterItemType.ROTATOR:
                    DrawRotator(g, r, fore);
                    break;
                case P23MeterItemType.LED:
                    DrawLed(g, r, item, fore);
                    break;
                case P23MeterItemType.WEB_IMAGE:
                    DrawWebImage(g, r, item);
                    break;
                case P23MeterItemType.BAND_BUTTONS:
                    DrawButtonBox(g, r, "BAND", _adapter.Band, fore);
                    break;
                case P23MeterItemType.MODE_BUTTONS:
                    DrawButtonBox(g, r, "MODE", _adapter.Mode, fore);
                    break;
                case P23MeterItemType.FILTER_BUTTONS:
                    DrawButtonBox(g, r, "FILTER", _adapter.Filter, fore);
                    break;
                case P23MeterItemType.ANTENNA_BUTTONS:
                    DrawButtonBox(g, r, "ANT", _adapter.ReadText("RX1Ant", "RXAnt", "Antenna"), fore);
                    break;
                case P23MeterItemType.HISTORY:
                    DrawHistory(g, r, item, fore);
                    break;
                case P23MeterItemType.TUNESTEP_BUTTONS:
                    DrawButtonBox(g, r, "STEP", _adapter.ReadText("TuneStep", "CurrentTuneStep", "TuneStepIndex"), fore);
                    break;
                case P23MeterItemType.FILTER_DISPLAY:
                    DrawFilterDisplay(g, r, fore);
                    break;
                case P23MeterItemType.CUSTOM_METER_BAR:
                    DrawMeterBar(g, r, String.IsNullOrEmpty(item.Name) ? "CUSTOM" : item.Name,
                        ReadNumeric(item.Source, 0.0), item.Minimum, item.Maximum, "", fore, item);
                    break;
                case P23MeterItemType.OTHER_BUTTONS:
                    DrawOtherButtons(g, r, fore);
                    break;
                case P23MeterItemType.WAVE_RECORD:
                    DrawButtonBox(g, r, "WAVE", "PowerSDR Wave", fore);
                    break;
                case P23MeterItemType.VOICE_RECORD_PLAY_BUTTONS:
                    DrawButtonBox(g, r, "VOICE", "REC / PLAY", fore);
                    break;
                default:
                    DrawCentered(g, r, P23MeterItemNames.Display(item.Type), fore, 10f, FontStyle.Regular);
                    break;
            }
        }

        private void DrawMeterBar(Graphics g, Rectangle r, string label, double value,
            double min, double max, string suffix, Color fore, P23MeterItemConfig cfg)
        {
            if (max <= min) max = min + 1.0;
            double p = (value - min) / (max - min);
            p = Math.Max(0.0, Math.Min(1.0, p));
            Rectangle bar = new Rectangle(r.Left + 8, r.Top + 24, Math.Max(10, r.Width - 16), Math.Max(8, r.Height - 32));
            using (Pen border = new Pen(Color.DimGray)) g.DrawRectangle(border, bar);
            Rectangle fill = new Rectangle(bar.Left + 1, bar.Top + 1,
                Math.Max(0, (int)((bar.Width - 2) * p)), Math.Max(0, bar.Height - 2));
            using (LinearGradientBrush br = new LinearGradientBrush(fill.Width > 0 ? fill : new Rectangle(0,0,1,1),
                Color.FromArgb(60, fore), fore, LinearGradientMode.Horizontal))
                if (fill.Width > 0 && fill.Height > 0) g.FillRectangle(br, fill);

            using (Font f = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (Brush b = new SolidBrush(fore))
            {
                g.DrawString(label, f, b, r.Left + 7, r.Top + 4);
                if (cfg.ShowValue)
                {
                    string s = value.ToString(Math.Abs(value) < 10 ? "0.00" : "0.0", CultureInfo.InvariantCulture) + suffix;
                    SizeF sz = g.MeasureString(s, f);
                    g.DrawString(s, f, b, r.Right - sz.Width - 7, r.Top + 4);
                }
            }
            if (cfg.ShowScale)
            {
                using (Font f = new Font("Segoe UI", 7f))
                using (Brush b = new SolidBrush(Color.Gray))
                {
                    g.DrawString(min.ToString("0", CultureInfo.InvariantCulture), f, b, bar.Left, bar.Bottom - 13);
                    string mx = max.ToString("0", CultureInfo.InvariantCulture);
                    SizeF z = g.MeasureString(mx, f);
                    g.DrawString(mx, f, b, bar.Right - z.Width, bar.Bottom - 13);
                }
            }
        }

        private void DrawMagicEye(Graphics g, Rectangle r, double signal, Color fore)
        {
            double p = Math.Max(0.0, Math.Min(1.0, (signal + 140.0) / 120.0));
            Rectangle eye = new Rectangle(r.Left + r.Width / 4, r.Top + 6, r.Width / 2, Math.Max(10, r.Height - 12));
            using (Pen pen = new Pen(fore, 2f)) g.DrawEllipse(pen, eye);
            int w = (int)(eye.Width * p);
            Rectangle pupil = new Rectangle(eye.Left + (eye.Width - w) / 2, eye.Top + 4, Math.Max(2, w), Math.Max(2, eye.Height - 8));
            using (SolidBrush b = new SolidBrush(Color.FromArgb(170, fore))) g.FillEllipse(b, pupil);
        }

        private void DrawAnanMeter(Graphics g, Rectangle r, Color fore)
        {
            double signal = _adapter.Signal;
            string s = _adapter.SUnit(signal);
            DrawCentered(g, new Rectangle(r.X, r.Y, r.Width, r.Height / 2), s, fore, 17f, FontStyle.Bold);
            DrawMeterBar(g, new Rectangle(r.X, r.Y + r.Height / 2, r.Width, Math.Max(28, r.Height / 2)),
                "SIG", signal, -140, -20, " dBm", fore, new P23MeterItemConfig());
        }

        private void DrawCross(Graphics g, Rectangle r, Color fore)
        {
            int cx = r.Left + r.Width / 2;
            int cy = r.Top + r.Height / 2;
            using (Pen p = new Pen(Color.DimGray, 1f))
            {
                g.DrawLine(p, cx, r.Top + 5, cx, r.Bottom - 5);
                g.DrawLine(p, r.Left + 5, cy, r.Right - 5, cy);
            }
            double a = Math.Max(0, Math.Min(1, (_adapter.ForwardPower) / 100.0));
            double b = Math.Max(0, Math.Min(1, (_adapter.ReversePower) / 25.0));
            using (Pen p = new Pen(fore, 2f))
            {
                g.DrawLine(p, cx, cy, r.Left + (int)(r.Width * a), r.Top + 5);
                g.DrawLine(p, cx, cy, r.Right - (int)(r.Width * b), r.Top + 5);
            }
        }

        private void DrawVfo(Graphics g, Rectangle r, Color fore)
        {
            string a = FormatMHz(_adapter.VfoA);
            string b = FormatMHz(_adapter.VfoB);
            using (Font fa = new Font("Consolas", Math.Max(10f, Math.Min(26f, r.Height * 0.30f)), FontStyle.Bold))
            using (Font fb = new Font("Consolas", Math.Max(8f, Math.Min(16f, r.Height * 0.20f)), FontStyle.Regular))
            using (Brush br = new SolidBrush(fore))
            {
                g.DrawString(a, fa, br, r.Left + 8, r.Top + 4);
                g.DrawString("VFO B  " + b + "    " + _adapter.Band + "  " + _adapter.Mode, fb, br, r.Left + 9, r.Top + r.Height / 2);
            }
        }

        private void DrawClock(Graphics g, Rectangle r, Color fore)
        {
            string s = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            string d = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            DrawCentered(g, new Rectangle(r.X, r.Y, r.Width, r.Height * 2 / 3), s, fore, 20f, FontStyle.Bold);
            DrawCentered(g, new Rectangle(r.X, r.Y + r.Height * 2 / 3 - 4, r.Width, r.Height / 3), d, Color.Gray, 9f, FontStyle.Regular);
        }

        private void DrawTextOverlay(Graphics g, Rectangle r, P23MeterItemConfig item, Color fore)
        {
            string text = String.IsNullOrEmpty(item.Text) ? "%VFOA%  %MODE%  %SIGNAL% dBm" : item.Text;
            text = ExpandVariables(text);
            using (Font f = new Font("Segoe UI", Math.Max(8f, Math.Min(16f, r.Height * 0.30f)), FontStyle.Regular))
            using (Brush b = new SolidBrush(fore))
                g.DrawString(text, f, b, new RectangleF(r.Left + 5, r.Top + 4, Math.Max(1, r.Width - 10), Math.Max(1, r.Height - 8)));
        }

        private void DrawDataOut(Graphics g, Rectangle r, P23MeterItemConfig item, Color fore)
        {
            object v = Lookup(String.IsNullOrEmpty(item.Source) ? "SIGNAL" : item.Source);
            DrawCentered(g, r, (String.IsNullOrEmpty(item.Source) ? "SIGNAL" : item.Source) +
                " = " + (v == null ? "--" : Convert.ToString(v, CultureInfo.InvariantCulture)), fore, 11f, FontStyle.Regular);
        }

        private void DrawRotator(Graphics g, Rectangle r, Color fore)
        {
            double az = ReadNumeric("AZ", 0.0);
            double el = ReadNumeric("ELE", 0.0);
            int size = Math.Max(20, Math.Min(r.Width, r.Height) - 10);
            Rectangle c = new Rectangle(r.Left + (r.Width - size) / 2, r.Top + (r.Height - size) / 2, size, size);
            using (Pen p = new Pen(Color.DimGray, 1f)) g.DrawEllipse(p, c);
            double rad = (az - 90.0) * Math.PI / 180.0;
            Point center = new Point(c.Left + c.Width / 2, c.Top + c.Height / 2);
            Point end = new Point(center.X + (int)(Math.Cos(rad) * c.Width * 0.45),
                                  center.Y + (int)(Math.Sin(rad) * c.Height * 0.45));
            using (Pen p = new Pen(fore, 3f)) g.DrawLine(p, center, end);
            using (Font f = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (Brush b = new SolidBrush(fore))
                g.DrawString("AZ " + az.ToString("0", CultureInfo.InvariantCulture) +
                    "°  EL " + el.ToString("0", CultureInfo.InvariantCulture) + "°", f, b, r.Left + 5, r.Top + 4);
        }

        private void DrawLed(Graphics g, Rectangle r, P23MeterItemConfig item, Color fore)
        {
            bool on = EvaluateCondition(item.Condition);
            int d = Math.Max(10, Math.Min(24, r.Height - 8));
            Rectangle led = new Rectangle(r.Left + 8, r.Top + (r.Height - d) / 2, d, d);
            using (Brush b = new SolidBrush(on ? fore : Color.FromArgb(45, 45, 45))) g.FillEllipse(b, led);
            using (Pen p = new Pen(Color.Gray)) g.DrawEllipse(p, led);
            using (Font f = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (Brush b = new SolidBrush(fore))
                g.DrawString(String.IsNullOrEmpty(item.Text) ? item.Name : item.Text, f, b, led.Right + 8, r.Top + (r.Height - f.Height) / 2);
        }

        private void DrawWebImage(Graphics g, Rectangle r, P23MeterItemConfig item)
        {
            Image img;
            if (_webImages.TryGetValue(item.Id, out img) && img != null)
            {
                g.DrawImage(img, r);
                return;
            }
            DrawCentered(g, r, String.IsNullOrEmpty(item.Url) ? "Web Image - no URL" : "Loading web image...", Color.Gray, 9f, FontStyle.Regular);
        }

        private void DrawButtonBox(Graphics g, Rectangle r, string label, string value, Color fore)
        {
            Rectangle b = new Rectangle(r.Left + 5, r.Top + 5, Math.Max(20, r.Width - 10), Math.Max(18, r.Height - 10));
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(35, 35, 35))) g.FillRectangle(bg, b);
            using (Pen p = new Pen(Color.DimGray)) g.DrawRectangle(p, b);
            using (Font f = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (Brush br = new SolidBrush(fore))
                g.DrawString(label + "   " + (String.IsNullOrEmpty(value) ? "--" : value), f, br, b.Left + 8, b.Top + (b.Height - f.Height) / 2);
        }

        private void DrawHistory(Graphics g, Rectangle r, P23MeterItemConfig item, Color fore)
        {
            Queue<double> q;
            if (!_history.TryGetValue(item.Id, out q) || q.Count < 2)
            {
                DrawCentered(g, r, "History", Color.Gray, 9f, FontStyle.Regular);
                return;
            }
            double min = item.Minimum;
            double max = item.Maximum;
            if (max <= min) { min = -140; max = -20; }
            double[] a = q.ToArray();
            using (Pen p = new Pen(fore, 1.4f))
            {
                PointF last = PointF.Empty;
                for (int i = 0; i < a.Length; i++)
                {
                    float x = r.Left + (float)i * (Math.Max(1, r.Width - 1)) / Math.Max(1, a.Length - 1);
                    double norm = (a[i] - min) / (max - min);
                    norm = Math.Max(0, Math.Min(1, norm));
                    float y = r.Bottom - 2 - (float)(norm * Math.Max(1, r.Height - 4));
                    PointF cur = new PointF(x, y);
                    if (!last.IsEmpty) g.DrawLine(p, last, cur);
                    last = cur;
                }
            }
        }

        private void DrawFilterDisplay(Graphics g, Rectangle r, Color fore)
        {
            string s = _adapter.Filter + "  " + _adapter.Mode;
            DrawCentered(g, r, s, fore, 12f, FontStyle.Bold);
        }

        private void DrawOtherButtons(Graphics g, Rectangle r, Color fore)
        {
            string s = (_adapter.IsTx ? "MOX ON" : "MOX OFF") + "    POWER " + (_adapter.PowerOn ? "ON" : "OFF");
            DrawButtonBox(g, r, "RADIO", s, fore);
        }

        private void DrawCentered(Graphics g, Rectangle r, string text, Color color, float size, FontStyle style)
        {
            using (Font f = new Font("Segoe UI", size, style))
            using (Brush b = new SolidBrush(color))
            {
                SizeF z = g.MeasureString(text ?? "", f);
                g.DrawString(text ?? "", f, b,
                    r.Left + Math.Max(0, (r.Width - z.Width) / 2f),
                    r.Top + Math.Max(0, (r.Height - z.Height) / 2f));
            }
        }

        private static string FormatMHz(double mhz)
        {
            if (mhz <= 0 || Double.IsNaN(mhz) || Double.IsInfinity(mhz)) return "--";
            long hz = (long)Math.Round(mhz * 1000000.0);
            return (hz / 1000000L).ToString(CultureInfo.InvariantCulture) + "." +
                ((hz / 1000L) % 1000L).ToString("000", CultureInfo.InvariantCulture) + "." +
                (hz % 1000L).ToString("000", CultureInfo.InvariantCulture);
        }

        private string ExpandVariables(string text)
        {
            if (String.IsNullOrEmpty(text)) return "";
            int precision = 2;
            int pidx = text.IndexOf("%precis=", StringComparison.OrdinalIgnoreCase);
            if (pidx >= 0)
            {
                int end = text.IndexOf('%', pidx + 8);
                if (end > pidx)
                {
                    int n;
                    if (Int32.TryParse(text.Substring(pidx + 8, end - (pidx + 8)), out n))
                        precision = Math.Max(0, Math.Min(8, n));
                    text = text.Remove(pidx, end - pidx + 1);
                }
            }

            int guard = 0;
            while (guard++ < 100)
            {
                int a = text.IndexOf('%');
                if (a < 0) break;
                int b = text.IndexOf('%', a + 1);
                if (b <= a) break;
                string key = text.Substring(a + 1, b - a - 1);
                object v = Lookup(key);
                string replacement = "";
                if (v != null)
                {
                    if (v is float || v is double || v is decimal)
                        replacement = Convert.ToDouble(v, CultureInfo.InvariantCulture).ToString("F" + precision.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                    else replacement = Convert.ToString(v, CultureInfo.InvariantCulture);
                }
                text = text.Substring(0, a) + replacement + text.Substring(b + 1);
            }
            return text;
        }

        private object Lookup(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return null;
            string k = key.Trim().Trim('%');
            object v;
            if (_snapshot.TryGetValue(k, out v)) return v;
            if (_snapshot.TryGetValue(k.ToUpperInvariant(), out v)) return v;
            return P23MultiMeterIO.Get(k);
        }

        private double ReadNumeric(string key, double fallback)
        {
            object v = Lookup(key);
            if (v == null) return fallback;
            try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private bool EvaluateCondition(string condition)
        {
            if (String.IsNullOrWhiteSpace(condition)) return _adapter.PowerOn;
            string c = condition.Trim();
            string[] ops = new string[] { ">=", "<=", "==", "!=", ">", "<" };
            for (int i = 0; i < ops.Length; i++)
            {
                int pos = c.IndexOf(ops[i], StringComparison.Ordinal);
                if (pos <= 0) continue;
                object left = Lookup(c.Substring(0, pos).Trim());
                string rs = c.Substring(pos + ops[i].Length).Trim().Trim('"');
                if (left == null) return false;
                double ld, rd;
                if (Double.TryParse(Convert.ToString(left, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out ld) &&
                    Double.TryParse(rs, NumberStyles.Float, CultureInfo.InvariantCulture, out rd))
                {
                    if (ops[i] == ">=") return ld >= rd;
                    if (ops[i] == "<=") return ld <= rd;
                    if (ops[i] == ">") return ld > rd;
                    if (ops[i] == "<") return ld < rd;
                    if (ops[i] == "==") return Math.Abs(ld - rd) < 0.0000001;
                    if (ops[i] == "!=") return Math.Abs(ld - rd) >= 0.0000001;
                }
                string ls = Convert.ToString(left, CultureInfo.InvariantCulture);
                if (ops[i] == "==") return String.Equals(ls, rs, StringComparison.OrdinalIgnoreCase);
                if (ops[i] == "!=") return !String.Equals(ls, rs, StringComparison.OrdinalIgnoreCase);
                return false;
            }

            object direct = Lookup(c);
            if (direct == null) return false;
            try { return Convert.ToBoolean(direct, CultureInfo.InvariantCulture); }
            catch
            {
                double d;
                return Double.TryParse(Convert.ToString(direct, CultureInfo.InvariantCulture), out d) && d != 0;
            }
        }

        private void EnsureWebImage(P23MeterItemConfig item)
        {
            if (String.IsNullOrWhiteSpace(item.Url)) return;
            if (_webImages.ContainsKey(item.Id) && !item.CacheBypass) return;
            if (_webPending.Contains(item.Id)) return;
            _webPending.Add(item.Id);

            string url = item.Url;
            if (item.CacheBypass)
                url += (url.Contains("?") ? "&" : "?") + "t=" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);

            WebClient wc = new WebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "PowerSDR-SQ4KOU-P23-Meters";
            wc.DownloadDataCompleted += delegate(object sender, DownloadDataCompletedEventArgs e)
            {
                try
                {
                    if (!e.Cancelled && e.Error == null && e.Result != null)
                    {
                        using (MemoryStream ms = new MemoryStream(e.Result))
                        using (Image source = Image.FromStream(ms))
                        {
                            Image old;
                            if (_webImages.TryGetValue(item.Id, out old) && old != null) old.Dispose();
                            _webImages[item.Id] = new Bitmap(source);
                        }
                    }
                }
                catch { }
                finally
                {
                    _webPending.Remove(item.Id);
                    try { wc.Dispose(); } catch { }
                    try { BeginInvoke((MethodInvoker)delegate { Invalidate(); }); } catch { }
                }
            };
            try { wc.DownloadDataAsync(new Uri(url)); }
            catch { _webPending.Remove(item.Id); wc.Dispose(); }
        }

        private void OnWheel(object sender, MouseEventArgs e)
        {
            P23MeterItemConfig item = ItemAt(e.Y);
            if (item == null) return;
            int dir = e.Delta >= 0 ? 1 : -1;
            if (item.Type == P23MeterItemType.VFO_DISPLAY || item.Type == P23MeterItemType.DIAL_DISPLAY)
            {
                double step = (Control.ModifierKeys & Keys.Control) != 0 ? 0.001 :
                              (Control.ModifierKeys & Keys.Shift) != 0 ? 0.00001 : 0.0001;
                _adapter.SetVfoA(_adapter.VfoA + dir * step);
            }
            else if (item.Type == P23MeterItemType.BAND_BUTTONS)
                _adapter.CycleMember("RX1Band", dir);
            else if (item.Type == P23MeterItemType.MODE_BUTTONS)
                _adapter.CycleMember("RX1DSPMode", dir);
            else if (item.Type == P23MeterItemType.FILTER_BUTTONS)
                _adapter.CycleMember("RX1Filter", dir);
            else if (item.Type == P23MeterItemType.TUNESTEP_BUTTONS)
            {
                object o = _adapter.ReadMember("TuneStepIndex");
                if (o != null)
                {
                    try { _adapter.WriteNamed("TuneStepIndex", Math.Max(0, Convert.ToInt32(o) + dir)); }
                    catch { }
                }
            }
        }

        private void OnMouseDownItem(object sender, MouseEventArgs e)
        {
            P23MeterItemConfig item = ItemAt(e.Y);
            if (item == null) return;
            if (item.Type == P23MeterItemType.OTHER_BUTTONS && e.Button == MouseButtons.Left)
                _adapter.WriteNamed("MOX", !_adapter.IsTx);
            else if (item.Type == P23MeterItemType.WAVE_RECORD && e.Button == MouseButtons.Left)
                TryShowConsoleMemberForm("waveform", "waveForm", "WaveForm");
        }

        private void TryShowConsoleMemberForm(params string[] names)
        {
            try
            {
                Type t = _adapter.Console.GetType();
                BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                for (int i = 0; i < names.Length; i++)
                {
                    FieldInfo fi = t.GetField(names[i], f);
                    object o = fi == null ? null : fi.GetValue(_adapter.Console);
                    Form form = o as Form;
                    if (form != null)
                    {
                        if (!form.Visible) form.Show(_adapter.Console);
                        form.BringToFront();
                        return;
                    }
                }
            }
            catch { }
        }

        private P23MeterItemConfig ItemAt(int y)
        {
            int top = 0;
            List<P23MeterItemConfig> items = _owner.Config.Items;
            if (items == null) return null;
            for (int i = 0; i < items.Count; i++)
            {
                P23MeterItemConfig item = items[i];
                if (item == null || !item.Enabled) continue;
                int h = Math.Max(12, item.Height);
                if (y >= top && y < top + h) return item;
                top += h;
            }
            return null;
        }
    }
}
