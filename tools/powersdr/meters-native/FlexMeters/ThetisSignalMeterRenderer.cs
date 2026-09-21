using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace FlexMeters
{
    // Renderer behavior and scale geometry are ported from pinned Thetis
    // MeterManager.cs/common.cs at 852bf0ef0b4f3886a13fc2846489aee16f361872.
    // GDI+ is used deliberately because pinned KE9NS has no SharpDX dependency.
    public static class ThetisSignalMeterMath
    {
        public const long S9FrequencyThresholdHertz = 30000000L;

        public static double MapSignalDbmToPosition(double dbm, bool aboveS9Frequency)
        {
            double value = dbm + (aboveS9Frequency ? 20.0 : 0.0);

            if (value <= -133.0)
                return 0.0;
            if (value >= -13.0)
                return 0.99;

            if (value <= -73.0)
                return ((value + 133.0) / 60.0) * 0.5;

            return 0.5 + (((value + 73.0) / 60.0) * 0.49);
        }

        public static double UvFromDbm(double dbm)
        {
            return Math.Sqrt(Math.Pow(10.0, dbm / 10.0) * 50.0 * 1e-3) * 1e6;
        }

        public static void SmeterFromDbm(
            double dbm,
            bool aboveS9Frequency,
            out int s,
            out int over9Db)
        {
            if (aboveS9Frequency)
            {
                if (dbm <= -144.0) { s = 0; over9Db = 0; }
                else if (dbm <= -138.0) { s = 1; over9Db = 0; }
                else if (dbm <= -132.0) { s = 2; over9Db = 0; }
                else if (dbm <= -126.0) { s = 3; over9Db = 0; }
                else if (dbm <= -120.0) { s = 4; over9Db = 0; }
                else if (dbm <= -114.0) { s = 5; over9Db = 0; }
                else if (dbm <= -108.0) { s = 6; over9Db = 0; }
                else if (dbm <= -102.0) { s = 7; over9Db = 0; }
                else if (dbm <= -96.0) { s = 8; over9Db = 0; }
                else if (dbm <= -90.0) { s = 9; over9Db = 0; }
                else if (dbm <= -86.0) { s = 9; over9Db = 5; }
                else if (dbm <= -80.0) { s = 9; over9Db = 10; }
                else if (dbm <= -76.0) { s = 9; over9Db = 15; }
                else if (dbm <= -66.0) { s = 9; over9Db = 20; }
                else if (dbm <= -56.0) { s = 9; over9Db = 30; }
                else if (dbm <= -46.0) { s = 9; over9Db = 40; }
                else if (dbm <= -36.0) { s = 9; over9Db = 50; }
                else { s = 9; over9Db = 60; }
                return;
            }

            if (dbm <= -124.0) { s = 0; over9Db = 0; }
            else if (dbm <= -118.0) { s = 1; over9Db = 0; }
            else if (dbm <= -112.0) { s = 2; over9Db = 0; }
            else if (dbm <= -106.0) { s = 3; over9Db = 0; }
            else if (dbm <= -100.0) { s = 4; over9Db = 0; }
            else if (dbm <= -94.0) { s = 5; over9Db = 0; }
            else if (dbm <= -88.0) { s = 6; over9Db = 0; }
            else if (dbm <= -82.0) { s = 7; over9Db = 0; }
            else if (dbm <= -76.0) { s = 8; over9Db = 0; }
            else if (dbm <= -70.0) { s = 9; over9Db = 0; }
            else if (dbm <= -66.0) { s = 9; over9Db = 5; }
            else if (dbm <= -60.0) { s = 9; over9Db = 10; }
            else if (dbm <= -56.0) { s = 9; over9Db = 15; }
            else if (dbm <= -46.0) { s = 9; over9Db = 20; }
            else if (dbm <= -36.0) { s = 9; over9Db = 30; }
            else if (dbm <= -26.0) { s = 9; over9Db = 40; }
            else if (dbm <= -16.0) { s = 9; over9Db = 50; }
            else { s = 9; over9Db = 60; }
        }

        public static string SmeterText(double dbm, bool aboveS9Frequency)
        {
            int s;
            int over;
            SmeterFromDbm(dbm, aboveS9Frequency, out s, out over);
            return over > 0
                ? "S " + s.ToString(CultureInfo.InvariantCulture) + " +" + over.ToString(CultureInfo.InvariantCulture)
                : "S " + s.ToString(CultureInfo.InvariantCulture);
        }
    }

    public sealed class ThetisSignalMeterControl : Control
    {
        private sealed class HistoryPoint
        {
            public DateTime TimeUtc;
            public double Value;
        }

        private const double AttackRatio = 0.8;
        private const double DecayRatio = 0.2;
        private static readonly TimeSpan HistoryDuration = TimeSpan.FromSeconds(4);

        private readonly string _itemType;
        private readonly List<HistoryPoint> _history = new List<HistoryPoint>();

        private bool _supported;
        private string _reason;
        private double? _rawValue;
        private double _value;
        private bool _hasValue;
        private bool _aboveS9Frequency;

        public ThetisSignalMeterControl(string itemType)
        {
            if (String.IsNullOrWhiteSpace(itemType))
                throw new ArgumentException("Meter item type is required.", "itemType");

            _itemType = itemType;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(32, 32, 32);
            ForeColor = Color.Yellow;
            MinimumSize = new Size(180, 72);
            Height = IsSignalText ? 112 : 92;
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
            get { return IsSignalText ? "THETIS_SIGNAL_TEXT" : "THETIS_SIGNAL_BAR"; }
        }

        public string DiagnosticText
        {
            get
            {
                if (!_supported)
                {
                    if (String.IsNullOrEmpty(_reason))
                        return _itemType + ": pending";
                    return _itemType + ": UNSUPPORTED - " + _reason;
                }

                if (!_rawValue.HasValue)
                    return _itemType + ": INVALID";

                return _itemType + ": " +
                    _rawValue.Value.ToString("0.0", CultureInfo.InvariantCulture) +
                    " dBm";
            }
        }

        public double NormalizedPosition
        {
            get
            {
                if (!_supported || !_hasValue)
                    return Double.NaN;
                return ThetisSignalMeterMath.MapSignalDbmToPosition(
                    _value,
                    _aboveS9Frequency);
            }
        }

        public double SmoothedValue
        {
            get { return _hasValue ? _value : Double.NaN; }
        }

        public bool AboveS9Frequency
        {
            get { return _aboveS9Frequency; }
        }

        public void UpdateReading(
            MeterReadingResult result,
            bool aboveS9Frequency)
        {
            if (!result.IsSupported || !result.Value.HasValue)
            {
                _supported = false;
                _reason = result.Reason;
                _rawValue = null;
                Invalidate();
                return;
            }

            if (!_hasValue || _aboveS9Frequency != aboveS9Frequency)
            {
                _aboveS9Frequency = aboveS9Frequency;
                _value = aboveS9Frequency ? -153.0 : -133.0;
                _history.Clear();
                _hasValue = true;
            }

            _supported = true;
            _reason = null;
            _rawValue = result.Value.Value;

            double reading = result.Value.Value;
            if (reading > _value)
                _value = (reading * AttackRatio) + (_value * (1.0 - AttackRatio));
            else
                _value = (reading * DecayRatio) + (_value * (1.0 - DecayRatio));

            DateTime now = DateTime.UtcNow;
            _history.Add(new HistoryPoint { TimeUtc = now, Value = _value });
            DateTime cutoff = now - HistoryDuration;
            while (_history.Count > 0 && _history[0].TimeUtc < cutoff)
                _history.RemoveAt(0);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (!_supported || !_hasValue)
            {
                DrawUnavailable(g);
                return;
            }

            if (IsSignalText)
                DrawSignalText(g);
            else
                DrawSignalBar(g);
        }

        private bool IsSignalText
        {
            get { return String.Equals(_itemType, "SIGNAL_TEXT", StringComparison.Ordinal); }
        }

        private void DrawUnavailable(Graphics g)
        {
            string text = String.IsNullOrEmpty(_reason)
                ? "pending"
                : "UNSUPPORTED" + Environment.NewLine + _reason;

            using (var font = new Font("Trebuchet MS", 9.0f, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.Gray))
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(text, font, brush, ClientRectangle, format);
            }
        }

        private void DrawSignalBar(Graphics g)
        {
            int width = Math.Max(1, ClientSize.Width);
            int height = Math.Max(1, ClientSize.Height);

            float x = Math.Max(4.0f, width * 0.004f);
            float w = Math.Max(20.0f, width - (2.0f * x));
            float top = Math.Max(28.0f, height * 0.30f);
            float h = Math.Max(32.0f, height - top - 4.0f);
            float baseY = top + (h * 0.85f);

            using (var titleFont = new Font("Trebuchet MS", Math.Max(7.0f, width / 52.0f), FontStyle.Regular))
            using (var titleBrush = new SolidBrush(Color.DarkGray))
            using (var yellow = new SolidBrush(Color.Yellow))
            using (var red = new SolidBrush(Color.Red))
            using (var valueFont = new Font("Trebuchet MS", Math.Max(7.0f, width / 58.0f), FontStyle.Regular))
            {
                DrawCentered(g, "Signal Peak", titleFont, titleBrush, new RectangleF(x, 1.0f, w, top * 0.45f));

                string currentText = _value.ToString("0.0", CultureInfo.InvariantCulture) + "dBm";
                g.DrawString(currentText, valueFont, yellow, x, Math.Max(0.0f, top - valueFont.Height - (h * 0.1f)));

                double peak = HistoryMax();
                string peakText = peak.ToString("0.0", CultureInfo.InvariantCulture) + "dBm";
                SizeF peakSize = g.MeasureString(peakText, valueFont);
                g.DrawString(
                    peakText,
                    valueFont,
                    red,
                    Math.Max(x, x + w - peakSize.Width),
                    Math.Max(0.0f, top - valueFont.Height - (h * 0.1f)));
            }

            float minX = x + (float)(ThetisSignalMeterMath.MapSignalDbmToPosition(
                HistoryMin(),
                _aboveS9Frequency) * w);
            float maxX = x + (float)(ThetisSignalMeterMath.MapSignalDbmToPosition(
                HistoryMax(),
                _aboveS9Frequency) * w);

            if (maxX < minX)
            {
                float tmp = minX;
                minX = maxX;
                maxX = tmp;
            }

            using (var historyBrush = new SolidBrush(Color.FromArgb(128, Color.Red)))
            {
                g.FillRectangle(
                    historyBrush,
                    minX,
                    top,
                    Math.Max(0.0f, maxX - minX),
                    h * 0.85f);
            }

            DrawSignalScale(g, x, top, w, h, baseY);

            float markerX = x + (float)(NormalizedPosition * w);
            using (var markerPen = new Pen(Color.Yellow, 3.0f))
                g.DrawLine(markerPen, markerX, top, markerX, top + h);
        }

        private static void DrawSignalScale(
            Graphics g,
            float x,
            float y,
            float w,
            float h,
            float baseY)
        {
            const int lowLongTicks = 6;
            const int highLongTicks = 3;
            const float centre = 0.5f;

            float lowSpacing = (w * centre) / (lowLongTicks - 1);
            float highSpacing =
                ((w - (lowSpacing * (lowLongTicks - 1))) - (w * 0.01f)) /
                highLongTicks;

            using (var lowPen = new Pen(Color.White, 2.0f))
            using (var highPen = new Pen(Color.Red, 2.0f))
            using (var lowBrush = new SolidBrush(Color.White))
            using (var highBrush = new SolidBrush(Color.Red))
            using (var font = new Font("Trebuchet MS", Math.Max(7.0f, w / 52.0f), FontStyle.Regular))
            {
                float splitX = x + (lowSpacing * (lowLongTicks - 1));
                g.DrawLine(lowPen, x, baseY, splitX, baseY);
                g.DrawLine(highPen, splitX, baseY, x + (w * 0.99f), baseY);

                for (int i = 1; i < lowLongTicks; i++)
                {
                    float shortX = x + (i * lowSpacing) - (lowSpacing * 0.5f);
                    g.DrawLine(lowPen, shortX, baseY, shortX, baseY - (h * 0.15f));

                    float longX = x + (i * lowSpacing);
                    float longY = baseY - (h * 0.3f);
                    g.DrawLine(lowPen, longX, baseY, longX, longY);
                    DrawScaleText(
                        g,
                        (1 + ((i - 1) * 2)).ToString(CultureInfo.InvariantCulture),
                        font,
                        lowBrush,
                        longX,
                        longY,
                        false,
                        x,
                        w);
                }

                for (int i = 1; i <= highLongTicks; i++)
                {
                    float shortX =
                        splitX + (i * highSpacing) - (highSpacing * 0.5f);
                    g.DrawLine(highPen, shortX, baseY, shortX, baseY - (h * 0.15f));

                    float longX = splitX + (i * highSpacing);
                    float longY = baseY - (h * 0.3f);
                    g.DrawLine(highPen, longX, baseY, longX, longY);
                    DrawScaleText(
                        g,
                        "+" + (i * 20).ToString(CultureInfo.InvariantCulture),
                        font,
                        highBrush,
                        longX,
                        longY,
                        i == highLongTicks,
                        x,
                        w);
                }
            }
        }

        private static void DrawScaleText(
            Graphics g,
            string text,
            Font font,
            Brush brush,
            float centreX,
            float topY,
            bool rightAlign,
            float x,
            float w)
        {
            SizeF size = g.MeasureString(text, font);
            float drawX = rightAlign
                ? x + w - size.Width
                : centreX - (size.Width / 2.0f);
            g.DrawString(text, font, brush, drawX, topY - size.Height);
        }

        private void DrawSignalText(Graphics g)
        {
            float w = Math.Max(1.0f, ClientSize.Width);
            float h = Math.Max(1.0f, ClientSize.Height);

            int s;
            int over;
            ThetisSignalMeterMath.SmeterFromDbm(
                _value,
                _aboveS9Frequency,
                out s,
                out over);

            double peak = HistoryMax();
            int peakS;
            int peakOver;
            ThetisSignalMeterMath.SmeterFromDbm(
                peak,
                _aboveS9Frequency,
                out peakS,
                out peakOver);

            using (var mainFont = new Font("Trebuchet MS", Math.Max(11.0f, w / 15.0f), FontStyle.Regular))
            using (var smallFont = new Font("Trebuchet MS", Math.Max(7.0f, w / 34.0f), FontStyle.Regular))
            using (var peakFont = new Font("Trebuchet MS", Math.Max(8.0f, w / 26.0f), FontStyle.Regular))
            using (var yellow = new SolidBrush(Color.Yellow))
            using (var gray = new SolidBrush(Color.Gray))
            using (var red = new SolidBrush(Color.Red))
            {
                string main = "S " + s.ToString(CultureInfo.InvariantCulture);
                DrawCentered(g, main, mainFont, yellow, new RectangleF(0, 0, w, h * 0.46f));

                if (over > 0)
                    g.DrawString(
                        "+" + over.ToString(CultureInfo.InvariantCulture),
                        smallFont,
                        yellow,
                        (w * 0.64f),
                        h * 0.18f);

                g.DrawString(
                    _value.ToString("0.0", CultureInfo.InvariantCulture) + "dBm",
                    smallFont,
                    gray,
                    w * 0.02f,
                    h * 0.27f);

                string uv = ThetisSignalMeterMath.UvFromDbm(_value)
                    .ToString("0.00", CultureInfo.InvariantCulture) + "uV";
                SizeF uvSize = g.MeasureString(uv, smallFont);
                g.DrawString(uv, smallFont, gray, Math.Max(0.0f, (w * 0.98f) - uvSize.Width), h * 0.27f);

                DrawPeakTypeMarker(g, gray, smallFont, w, h);

                string peakMain = "S " + peakS.ToString(CultureInfo.InvariantCulture);
                DrawCentered(g, peakMain, peakFont, red, new RectangleF(0, h * 0.58f, w, h * 0.25f));

                if (peakOver > 0)
                    g.DrawString(
                        "+" + peakOver.ToString(CultureInfo.InvariantCulture),
                        smallFont,
                        red,
                        w * 0.61f,
                        h * 0.67f);

                g.DrawString(
                    peak.ToString("0.0", CultureInfo.InvariantCulture) + "dBm",
                    smallFont,
                    red,
                    w * 0.02f,
                    h * 0.82f);

                string peakUv = ThetisSignalMeterMath.UvFromDbm(peak)
                    .ToString("0.00", CultureInfo.InvariantCulture) + "uV";
                SizeF peakUvSize = g.MeasureString(peakUv, smallFont);
                g.DrawString(peakUv, smallFont, red, Math.Max(0.0f, (w * 0.98f) - peakUvSize.Width), h * 0.82f);
            }
        }

        private static void DrawPeakTypeMarker(
            Graphics g,
            Brush brush,
            Font font,
            float w,
            float h)
        {
            using (var pen = new Pen(((SolidBrush)brush).Color, 1.5f))
            {
                PointF tip = new PointF(w * 0.965f, h * 0.10f);
                g.DrawLine(pen, w * 0.95f, h * 0.20f, tip.X, tip.Y);
                g.DrawLine(pen, w * 0.98f, h * 0.20f, tip.X, tip.Y);
                g.DrawLine(pen, w * 0.95f, h * 0.09f, w * 0.98f, h * 0.09f);
            }

            g.DrawString("PK", font, brush, w * 0.88f, h * 0.01f);
        }

        private double HistoryMin()
        {
            if (_history.Count == 0)
                return _value;

            double value = _history[0].Value;
            for (int i = 1; i < _history.Count; i++)
                value = Math.Min(value, _history[i].Value);
            return value;
        }

        private double HistoryMax()
        {
            if (_history.Count == 0)
                return _value;

            double value = _history[0].Value;
            for (int i = 1; i < _history.Count; i++)
                value = Math.Max(value, _history[i].Value);
            return value;
        }

        private static void DrawCentered(
            Graphics g,
            string text,
            Font font,
            Brush brush,
            RectangleF bounds)
        {
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(text, font, brush, bounds, format);
            }
        }
    }
}
