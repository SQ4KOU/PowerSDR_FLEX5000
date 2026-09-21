using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace FlexMeters
{
    public sealed class FlexTxMeterControl : Control
    {
        private readonly MeterItemDescriptor _descriptor;
        private bool _supported;
        private string _reason;
        private double? _value;

        public FlexTxMeterControl(MeterItemDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException("descriptor");
            if (descriptor.RendererKind != MeterItemRendererKind.Linear)
                throw new ArgumentException("Linear renderer descriptor required.", "descriptor");

            _descriptor = descriptor;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.Yellow;
            MinimumSize = new Size(180, 58);
            Height = 72;
            Dock = DockStyle.Top;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        public string RendererKind
        {
            get { return "FLEX5000_LINEAR_" + _descriptor.Type; }
        }

        public string DiagnosticText
        {
            get
            {
                if (!_supported)
                {
                    if (String.IsNullOrEmpty(_reason))
                        return _descriptor.Type + ": pending";
                    return _descriptor.Type + ": UNSUPPORTED - " + _reason;
                }

                if (!_value.HasValue)
                    return _descriptor.Type + ": INVALID";

                string units = String.IsNullOrEmpty(_descriptor.Units)
                    ? ""
                    : " " + _descriptor.Units;

                return _descriptor.Type + ": " +
                    _value.Value.ToString("0.0", CultureInfo.InvariantCulture) +
                    units;
            }
        }

        public double NormalizedPosition
        {
            get
            {
                if (!_supported || !_value.HasValue)
                    return Double.NaN;

                double span = _descriptor.Maximum - _descriptor.Minimum;
                if (span <= 0.0)
                    return Double.NaN;

                double position =
                    (_value.Value - _descriptor.Minimum) / span;
                if (position < 0.0) return 0.0;
                if (position > 1.0) return 1.0;
                return position;
            }
        }

        public void UpdateReading(MeterReadingResult result)
        {
            if (!result.IsSupported || !result.Value.HasValue)
            {
                _supported = false;
                _reason = result.Reason;
                _value = null;
                Invalidate();
                return;
            }

            _supported = true;
            _reason = null;
            _value = result.Value.Value;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (!_supported || !_value.HasValue)
            {
                DrawUnavailable(g);
                return;
            }

            int width = Math.Max(1, ClientSize.Width);
            int height = Math.Max(1, ClientSize.Height);
            float left = 8.0f;
            float right = 8.0f;
            float meterWidth = Math.Max(10.0f, width - left - right);
            float barTop = Math.Max(32.0f, height * 0.56f);
            float barHeight = Math.Max(10.0f, height - barTop - 8.0f);

            using (var titleFont = new Font("Trebuchet MS", 9.0f, FontStyle.Regular))
            using (var valueFont = new Font("Trebuchet MS", 10.0f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.DarkGray))
            using (var valueBrush = new SolidBrush(Color.Yellow))
            using (var trackBrush = new SolidBrush(Color.FromArgb(55, 55, 55)))
            using (var fillBrush = new SolidBrush(Color.Yellow))
            using (var borderPen = new Pen(Color.DimGray))
            {
                g.DrawString(_descriptor.DisplayName, titleFont, titleBrush, left, 4.0f);

                string units = String.IsNullOrEmpty(_descriptor.Units)
                    ? ""
                    : " " + _descriptor.Units;
                string text =
                    _value.Value.ToString("0.0", CultureInfo.InvariantCulture) +
                    units;
                SizeF valueSize = g.MeasureString(text, valueFont);
                g.DrawString(
                    text,
                    valueFont,
                    valueBrush,
                    Math.Max(left, width - right - valueSize.Width),
                    3.0f);

                RectangleF track =
                    new RectangleF(left, barTop, meterWidth, barHeight);
                g.FillRectangle(trackBrush, track);
                g.DrawRectangle(
                    borderPen,
                    track.X,
                    track.Y,
                    track.Width,
                    track.Height);

                double normalized = NormalizedPosition;
                if (!Double.IsNaN(normalized) && normalized > 0.0)
                {
                    RectangleF fill = new RectangleF(
                        track.X + 1.0f,
                        track.Y + 1.0f,
                        (float)Math.Max(
                            0.0,
                            (track.Width - 2.0f) * normalized),
                        Math.Max(0.0f, track.Height - 2.0f));
                    g.FillRectangle(fillBrush, fill);
                }
            }
        }

        private void DrawUnavailable(Graphics g)
        {
            string text = String.IsNullOrEmpty(_reason)
                ? _descriptor.DisplayName + ": pending"
                : _descriptor.DisplayName + ": UNSUPPORTED - " + _reason;

            using (var font = new Font("Trebuchet MS", 9.0f, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.Gray))
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(text, font, brush, ClientRectangle, format);
            }
        }
    }
}
