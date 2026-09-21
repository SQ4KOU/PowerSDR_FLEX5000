// P22 - minimal native Thetis meter container port for PowerSDR/FLEX-5000.
// Thetis reference: ramdor/Thetis @ 852bf0ef0b4f3886a13fc2846489aee16f361872
//   frmMeterDisplay.cs + frmMeterDisplay.Designer.cs (container mechanics)
// PowerSDR telemetry reference: ke9ns/PowerSDR-KE9NS-v2.8.0 @
//   d558979570c4c2e4572b63ac218d3d4477926cb8, Console/console.cs.
//
// Deliberate Stage-1 boundary:
// - RX1 only. RX2 is not instantiated or exposed.
// - The borderless floating container and PowerSDR DB form persistence are the
//   ported core under physical test.
// - The meter face is intentionally a minimal diagnostic surface. It is NOT the
//   final visual Meters/Gadgets port and must not be used as evidence of visual parity.
// - One telemetry adapter reads the native FLEX-5000 RX1 signal path with the same
//   calibration terms used by PowerSDR's own SIGNAL_STRENGTH case.
// - Physical log is authoritative for Stage-1 PASS.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PowerSDR
{
    internal struct Flex5000Rx1TelemetrySample
    {
        public DateTime TimeUtc;
        public bool PowerOn;
        public bool Mox;
        public Model Model;
        public MeterRXMode NativeMeterMode;
        public float SignalDbm;
        public float NativeMeterData;
    }

    internal sealed class Flex5000Rx1TelemetryAdapter
    {
        private readonly Console _console;

        public Flex5000Rx1TelemetryAdapter(Console console)
        {
            if (console == null) throw new ArgumentNullException("console");
            _console = console;
        }

        public Flex5000Rx1TelemetrySample Read()
        {
            Flex5000Rx1TelemetrySample s = new Flex5000Rx1TelemetrySample();
            s.TimeUtc = DateTime.UtcNow;
            s.PowerOn = _console.PowerOn;
            s.Mox = _console.MOX;
            s.Model = _console.CurrentModel;
            s.NativeMeterMode = _console.CurrentMeterRXMode;
            s.NativeMeterData = _console.NewMeterData;

            if (s.PowerOn && !s.Mox && s.Model == Model.FLEX5000)
                s.SignalDbm = _console.P22ReadFlex5000Rx1SignalDbm();
            else
                s.SignalDbm = float.NaN;

            return s;
        }
    }

    internal static class P22MeterPhysicalLog
    {
        private static readonly object Sync = new object();
        private static string _path;

        public static string LogPath
        {
            get
            {
                if (_path == null)
                {
                    string dir = Application.UserAppDataPath;
                    _path = Path.Combine(dir, "P22_ThetisMeter_RX1_PHYSICAL.log");
                }
                return _path;
            }
        }

        public static void Write(string text)
        {
            try
            {
                lock (Sync)
                {
                    string path = LogPath;
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.AppendAllText(
                        path,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                        " | " + text + Environment.NewLine);
                }
            }
            catch
            {
                // Diagnostics must never affect radio operation.
            }
        }
    }

    // Minimal Stage-1 adapter lives in the Console partial so it can consume the
    // exact private calibration state already used by the native PowerSDR meter.
    sealed unsafe public partial class Console
    {
        private ThetisStage1MeterDisplay _p22MeterDisplay;
        private ToolStripMenuItem _p22MeterMenu;
        private bool _p22MenuSync;

        internal float P22ReadFlex5000Rx1SignalDbm()
        {
            // Copied from the native MeterRXMode.SIGNAL_STRENGTH FLEX5000 branch
            // in pinned KE9NS console.cs. No new calibration constants.
            float num = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.SIGNAL_STRENGTH);

            if (fwc_init || hid_init)
            {
                switch (current_model)
                {
                    case Model.SDRX:
                        num = num +
                            rx1_meter_cal_offset +
                            meter_offset +
                            rx1_filter_size_cal_offset +
                            rx1_xvtr_gain_offset;
                        break;

                    case Model.FLEX5000:
                    case Model.FLEX3000:
                        num = num +
                            rx1_meter_cal_offset +
                            rx1_preamp_offset[(int)rx1_preamp_mode] +
                            rx1_filter_size_cal_offset +
                            rx1_path_offset +
                            rx1_xvtr_gain_offset +
                            rx1_loop_offset;
                        break;

                    case Model.FLEX1500:
                        num = num +
                            rx1_meter_cal_offset +
                            rx1_preamp_offset[(int)rx1_preamp_mode] +
                            rx1_filter_size_cal_offset +
                            rx1_xvtr_gain_offset;
                        break;
                }
            }
            else
            {
                num = num +
                    rx1_meter_cal_offset +
                    rx1_preamp_offset[(int)rx1_preamp_mode] +
                    rx1_filter_size_cal_offset +
                    rx1_xvtr_gain_offset;
            }

            return num;
        }

        private void P22ThetisMeterShown(object sender, EventArgs e)
        {
            if (_p22MeterDisplay != null) return;

            try
            {
                Flex5000Rx1TelemetryAdapter adapter = new Flex5000Rx1TelemetryAdapter(this);
                _p22MeterDisplay = new ThetisStage1MeterDisplay(this, adapter);
                _p22MeterDisplay.RequestedVisibilityChanged += P22MeterRequestedVisibilityChanged;

                _p22MeterMenu = new ToolStripMenuItem();
                _p22MeterMenu.Name = "p22ThetisRx1MeterToolStripMenuItem";
                _p22MeterMenu.Text = "RX1 Meter [P22]";
                _p22MeterMenu.CheckOnClick = true;
                _p22MeterMenu.CheckedChanged += P22MeterMenuCheckedChanged;
                menuStrip1.Items.Add(_p22MeterMenu);

                _p22MenuSync = true;
                _p22MeterMenu.Checked = _p22MeterDisplay.RequestedVisible;
                _p22MenuSync = false;

                if (_p22MeterDisplay.RequestedVisible)
                    _p22MeterDisplay.Show(this);

                P22MeterPhysicalLog.Write(
                    "INIT source=FLEX5000_NATIVE_SIGNAL_PATH container=THETIS_FRMMETERDISPLAY " +
                    "requested_visible=" + (_p22MeterDisplay.RequestedVisible ? "1" : "0") +
                    " log=" + P22MeterPhysicalLog.LogPath);
            }
            catch (Exception ex)
            {
                P22MeterPhysicalLog.Write("INIT_FAIL " + ex.GetType().FullName + " " + ex.Message);
            }
        }

        private void P22MeterMenuCheckedChanged(object sender, EventArgs e)
        {
            if (_p22MenuSync || _p22MeterDisplay == null || _p22MeterMenu == null) return;

            if (_p22MeterMenu.Checked)
                _p22MeterDisplay.ShowRequested(this);
            else
                _p22MeterDisplay.HideRequested();
        }

        private void P22MeterRequestedVisibilityChanged(object sender, EventArgs e)
        {
            if (_p22MeterMenu == null || _p22MeterDisplay == null) return;

            _p22MenuSync = true;
            _p22MeterMenu.Checked = _p22MeterDisplay.RequestedVisible;
            _p22MenuSync = false;
        }

        private void P22ThetisMeterConsoleClosing(object sender, FormClosingEventArgs e)
        {
            if (_p22MeterDisplay != null)
            {
                _p22MeterDisplay.SaveForApplicationExit();
                P22MeterPhysicalLog.Write("CONSOLE_CLOSING reason=" + e.CloseReason.ToString());
            }
        }
    }

    internal sealed class ThetisStage1MeterDisplay : Form
    {
        private const string PersistenceTable = "MeterDisplay_SQ4KOU_P22_RX1";
        private readonly Console _console;
        private readonly Flex5000Rx1TelemetryAdapter _adapter;
        private readonly System.Windows.Forms.CheckBoxTS _persistVisible;
        private readonly ThetisStage1Rx1Surface _surface;
        private bool _requestedVisible;

        public event EventHandler RequestedVisibilityChanged;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;

        public ThetisStage1MeterDisplay(Console console, Flex5000Rx1TelemetryAdapter adapter)
        {
            _console = console;
            _adapter = adapter;

            // Thetis frmMeterDisplay.Designer.cs core.
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Black;
            ClientSize = new Size(400, 200);
            FormBorderStyle = FormBorderStyle.None; // confirmed Thetis behavior: no X
            MinimizeBox = false;
            MinimumSize = new Size(100, 32);
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "RX1 Meter [P22]";
            StartPosition = FormStartPosition.Manual;

            // Hidden native PowerSDR control lets Common.SaveForm/RestoreForm persist
            // requested visibility in the same DB table as the form geometry.
            _persistVisible = new System.Windows.Forms.CheckBoxTS();
            _persistVisible.Name = "chkP22RequestedVisible";
            _persistVisible.Checked = true; // first run default: visible for physical test
            _persistVisible.Visible = false;
            Controls.Add(_persistVisible);

            _surface = new ThetisStage1Rx1Surface(_console, _adapter);
            _surface.Dock = DockStyle.Fill;
            Controls.Add(_surface);
            _surface.BringToFront();

            Common.RestoreForm(this, PersistenceTable, true);
            Common.ForceFormOnScreen(this);
            _requestedVisible = _persistVisible.Checked;

            FormClosing += ThetisStage1MeterDisplay_FormClosing;

            P22MeterPhysicalLog.Write(
                "RESTORE visible=" + (_requestedVisible ? "1" : "0") +
                " bounds=" + BoundsToText());
        }

        public bool RequestedVisible
        {
            get { return _requestedVisible; }
        }

        public void ShowRequested(IWin32Window owner)
        {
            _requestedVisible = true;
            _persistVisible.Checked = true;
            if (!Visible) Show(owner);
            else BringToFront();
            _surface.StartSampling();
            SaveState("SHOW");
            OnRequestedVisibilityChanged();
        }

        public void HideRequested()
        {
            _requestedVisible = false;
            _persistVisible.Checked = false;
            _surface.StopSampling();
            Hide();
            SaveState("HIDE");
            OnRequestedVisibilityChanged();
        }

        public void SaveForApplicationExit()
        {
            if (Visible) _requestedVisible = true;
            _persistVisible.Checked = _requestedVisible;
            SaveState("APP_EXIT");
            _surface.StopSampling();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _surface.StartSampling();
            P22MeterPhysicalLog.Write("SHOWN bounds=" + BoundsToText());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Exact Thetis frmMeterDisplay DPI workaround: move one pixel and back.
            IntPtr handle = Handle;
            Rectangle bounds = Bounds;
            int originalX = bounds.X;
            int originalY = bounds.Y;
            SetWindowPos(handle, IntPtr.Zero, originalX + 1, originalY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
            SetWindowPos(handle, IntPtr.Zero, originalX, originalY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        private void ThetisStage1MeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                HideRequested();
                e.Cancel = true;
                return;
            }

            _persistVisible.Checked = _requestedVisible;
            SaveState("FORM_CLOSING_" + e.CloseReason.ToString());
        }

        private void SaveState(string reason)
        {
            try
            {
                Common.SaveForm(this, PersistenceTable);
                P22MeterPhysicalLog.Write(
                    "SAVE reason=" + reason +
                    " visible=" + (_requestedVisible ? "1" : "0") +
                    " bounds=" + BoundsToText());
            }
            catch (Exception ex)
            {
                P22MeterPhysicalLog.Write("SAVE_FAIL reason=" + reason + " " + ex.Message);
            }
        }

        private string BoundsToText()
        {
            return Left.ToString(CultureInfo.InvariantCulture) + "," +
                   Top.ToString(CultureInfo.InvariantCulture) + "," +
                   Width.ToString(CultureInfo.InvariantCulture) + "," +
                   Height.ToString(CultureInfo.InvariantCulture);
        }

        private void OnRequestedVisibilityChanged()
        {
            EventHandler h = RequestedVisibilityChanged;
            if (h != null) h(this, EventArgs.Empty);
        }
    }

    internal sealed class ThetisStage1Rx1Surface : UserControl
    {
        private readonly Console _console;
        private readonly Flex5000Rx1TelemetryAdapter _adapter;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Panel _dragBar;
        private readonly Label _rxLabel;
        private readonly Panel _resizeGrip;

        private Flex5000Rx1TelemetrySample _sample;
        private bool _hasSample;
        private bool _dragging;
        private Point _dragCursorStart;
        private Point _dragFormStart;
        private bool _resizing;
        private Point _resizeCursorStart;
        private Size _resizeFormStart;
        private DateTime _lastLogUtc = DateTime.MinValue;

        public ThetisStage1Rx1Surface(Console console, Flex5000Rx1TelemetryAdapter adapter)
        {
            _console = console;
            _adapter = adapter;

            DoubleBuffered = true;
            BackColor = Color.Black;
            MinimumSize = new Size(100, 32);

            _dragBar = new Panel();
            _dragBar.Height = 18;
            _dragBar.Dock = DockStyle.Top;
            _dragBar.BackColor = Color.DimGray;
            _dragBar.Visible = false;
            _dragBar.Cursor = Cursors.SizeAll;
            _dragBar.MouseDown += DragBarMouseDown;
            _dragBar.MouseMove += DragBarMouseMove;
            _dragBar.MouseUp += DragBarMouseUp;
            _dragBar.MouseLeave += DragBarMouseLeave;

            _rxLabel = new Label();
            _rxLabel.Text = "RX1";
            _rxLabel.ForeColor = Color.White;
            _rxLabel.BackColor = Color.DimGray;
            _rxLabel.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
            _rxLabel.AutoSize = true;
            _rxLabel.Location = new Point(6, 2);
            _rxLabel.MouseDown += DragBarMouseDown;
            _rxLabel.MouseMove += DragBarMouseMove;
            _rxLabel.MouseUp += DragBarMouseUp;
            _dragBar.Controls.Add(_rxLabel);

            _resizeGrip = new Panel();
            _resizeGrip.Size = new Size(16, 16);
            _resizeGrip.BackColor = Color.DimGray;
            _resizeGrip.Cursor = Cursors.SizeNWSE;
            _resizeGrip.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            _resizeGrip.Visible = false;
            _resizeGrip.MouseDown += ResizeMouseDown;
            _resizeGrip.MouseMove += ResizeMouseMove;
            _resizeGrip.MouseUp += ResizeMouseUp;
            _resizeGrip.MouseLeave += ResizeMouseLeave;

            Controls.Add(_resizeGrip);
            Controls.Add(_dragBar);

            MouseMove += SurfaceMouseMove;
            MouseLeave += SurfaceMouseLeave;
            Resize += SurfaceResize;

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 50;
            _timer.Tick += TimerTick;

            SurfaceResize(this, EventArgs.Empty);
        }

        public void StartSampling()
        {
            if (!_timer.Enabled) _timer.Start();
        }

        public void StopSampling()
        {
            if (_timer.Enabled) _timer.Stop();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
                _rxLabel.Dispose();
                _dragBar.Dispose();
                _resizeGrip.Dispose();
            }
            base.Dispose(disposing);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                _sample = _adapter.Read();
                _hasSample = true;

                if ((DateTime.UtcNow - _lastLogUtc).TotalMilliseconds >= 1000.0)
                {
                    _lastLogUtc = DateTime.UtcNow;

                    string delta = "NA";
                    if (_sample.PowerOn &&
                        !_sample.Mox &&
                        _sample.Model == Model.FLEX5000 &&
                        _sample.NativeMeterMode == MeterRXMode.SIGNAL_STRENGTH &&
                        !float.IsNaN(_sample.SignalDbm))
                    {
                        delta = (_sample.SignalDbm - _sample.NativeMeterData)
                            .ToString("F2", CultureInfo.InvariantCulture);
                    }

                    P22MeterPhysicalLog.Write(
                        "SAMPLE model=" + _sample.Model.ToString() +
                        " power=" + (_sample.PowerOn ? "1" : "0") +
                        " mox=" + (_sample.Mox ? "1" : "0") +
                        " native_mode=" + _sample.NativeMeterMode.ToString() +
                        " signal_dbm=" + FloatText(_sample.SignalDbm) +
                        " native_meter=" + FloatText(_sample.NativeMeterData) +
                        " delta_when_comparable=" + delta);
                }

                Invalidate();
            }
            catch (Exception ex)
            {
                P22MeterPhysicalLog.Write("SAMPLE_FAIL " + ex.GetType().FullName + " " + ex.Message);
            }
        }

        private static string FloatText(float value)
        {
            if (float.IsNaN(value)) return "NA";
            return value.ToString("F2", CultureInfo.InvariantCulture);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle r = ClientRectangle;
            g.Clear(Color.Black);

            string stateText;
            string valueText;

            if (!_hasSample)
            {
                stateText = "RX1 / waiting";
                valueText = "---";
            }
            else if (!_sample.PowerOn)
            {
                stateText = "RX1 / POWER OFF";
                valueText = "---";
            }
            else if (_sample.Mox)
            {
                stateText = "RX1 / TX";
                valueText = "---";
            }
            else if (_sample.Model != Model.FLEX5000)
            {
                stateText = "RX1 / model " + _sample.Model.ToString();
                valueText = "---";
            }
            else
            {
                stateText = "RX1 SIGNAL";
                valueText = _sample.SignalDbm.ToString("F1", CultureInfo.InvariantCulture) + " dBm";
            }

            using (Font titleFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 9.0f, FontStyle.Bold))
            using (Font valueFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 20.0f, FontStyle.Bold))
            using (Font smallFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 8.0f, FontStyle.Regular))
            using (Brush white = new SolidBrush(Color.White))
            using (Brush gray = new SolidBrush(Color.Silver))
            using (Pen scalePen = new Pen(Color.Gray))
            using (Pen signalPen = new Pen(Color.White, 4.0f))
            {
                g.DrawString(stateText, titleFont, white, 8, 8);
                SizeF vs = g.MeasureString(valueText, valueFont);
                g.DrawString(valueText, valueFont, white, Math.Max(8, (r.Width - vs.Width) / 2), 38);

                int left = 16;
                int right = Math.Max(left + 10, r.Width - 16);
                int y = Math.Max(92, r.Height - 48);

                g.DrawLine(scalePen, left, y, right, y);

                int[] marks = new int[] { -140, -120, -100, -80, -60, -40, -20 };
                for (int i = 0; i < marks.Length; i++)
                {
                    float p = (marks[i] + 140.0f) / 120.0f;
                    int x = left + (int)((right - left) * p);
                    g.DrawLine(scalePen, x, y - 4, x, y + 4);
                    string t = marks[i].ToString(CultureInfo.InvariantCulture);
                    SizeF ts = g.MeasureString(t, smallFont);
                    g.DrawString(t, smallFont, gray, x - ts.Width / 2, y + 7);
                }

                if (_hasSample && !float.IsNaN(_sample.SignalDbm) && _sample.PowerOn && !_sample.Mox)
                {
                    float clamped = Math.Max(-140.0f, Math.Min(-20.0f, _sample.SignalDbm));
                    float p = (clamped + 140.0f) / 120.0f;
                    int x = left + (int)((right - left) * p);
                    g.DrawLine(signalPen, left, y - 10, x, y - 10);
                }

                string footer = "P22 physical-test meter | log: P22_ThetisMeter_RX1_PHYSICAL.log";
                g.DrawString(footer, smallFont, gray, 8, Math.Max(8, r.Height - 18));
            }
        }

        private void SurfaceMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging && e.Y <= 18 && !_dragBar.Visible)
            {
                _dragBar.Visible = true;
                _dragBar.BringToFront();
            }

            if (!_resizing &&
                e.X >= ClientSize.Width - 20 &&
                e.Y >= ClientSize.Height - 20 &&
                !_resizeGrip.Visible)
            {
                _resizeGrip.Visible = true;
                _resizeGrip.BringToFront();
            }
        }

        private void SurfaceMouseLeave(object sender, EventArgs e)
        {
            if (!_dragging) _dragBar.Visible = false;
            if (!_resizing) _resizeGrip.Visible = false;
        }

        private Form ParentFormSafe()
        {
            return FindForm();
        }

        private void DragBarMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Form f = ParentFormSafe();
            if (f == null) return;

            _dragging = true;
            _dragCursorStart = Cursor.Position;
            _dragFormStart = f.Location;
        }

        private void DragBarMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            Form f = ParentFormSafe();
            if (f == null) return;

            Point p = Cursor.Position;
            f.Location = new Point(
                _dragFormStart.X + (p.X - _dragCursorStart.X),
                _dragFormStart.Y + (p.Y - _dragCursorStart.Y));
        }

        private void DragBarMouseUp(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            _dragging = false;
            P22MeterPhysicalLog.Write("MOVE bounds=" + FormBoundsText());
        }

        private void DragBarMouseLeave(object sender, EventArgs e)
        {
            if (!_dragging) _dragBar.Visible = false;
        }

        private void ResizeMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Form f = ParentFormSafe();
            if (f == null) return;

            _resizing = true;
            _resizeCursorStart = Cursor.Position;
            _resizeFormStart = f.Size;
        }

        private void ResizeMouseMove(object sender, MouseEventArgs e)
        {
            if (!_resizing) return;
            Form f = ParentFormSafe();
            if (f == null) return;

            Point p = Cursor.Position;
            int w = Math.Max(f.MinimumSize.Width, _resizeFormStart.Width + (p.X - _resizeCursorStart.X));
            int h = Math.Max(f.MinimumSize.Height, _resizeFormStart.Height + (p.Y - _resizeCursorStart.Y));
            f.Size = new Size(w, h);
        }

        private void ResizeMouseUp(object sender, MouseEventArgs e)
        {
            if (!_resizing) return;
            _resizing = false;
            P22MeterPhysicalLog.Write("RESIZE bounds=" + FormBoundsText());
        }

        private void ResizeMouseLeave(object sender, EventArgs e)
        {
            if (!_resizing) _resizeGrip.Visible = false;
        }

        private void SurfaceResize(object sender, EventArgs e)
        {
            _resizeGrip.Location = new Point(
                Math.Max(0, ClientSize.Width - _resizeGrip.Width),
                Math.Max(0, ClientSize.Height - _resizeGrip.Height));
        }

        private string FormBoundsText()
        {
            Form f = ParentFormSafe();
            if (f == null) return "NA";
            return f.Left.ToString(CultureInfo.InvariantCulture) + "," +
                   f.Top.ToString(CultureInfo.InvariantCulture) + "," +
                   f.Width.ToString(CultureInfo.InvariantCulture) + "," +
                   f.Height.ToString(CultureInfo.InvariantCulture);
        }
    }
}
