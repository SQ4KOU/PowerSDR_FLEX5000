using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SharpDX.Mathematics.Interop;

namespace PowerSDR
{
    // P31: backport of the later Thetis meter gadgets into the physically verified P30 core.
    // Source model: Thetis 3dbd787 (VFO/BAND/MODE/TUNESTEP button-box generation).
    // Target adaptation is deliberately RX1-only because this FLEX-5000 project has RX2 disabled.
    internal static partial class MeterManager
    {
        public partial class clsMeterItem
        {
            private PointF p31MouseMovePoint;
            private bool p31MouseEntered;
            private bool p31MouseButtonDown;
            private MouseButtons p31MouseButton;

            public virtual PointF P31MouseMovePoint
            {
                get { return p31MouseMovePoint; }
                set { p31MouseMovePoint = value; }
            }
            public virtual bool P31MouseEntered
            {
                get { return p31MouseEntered; }
                set { p31MouseEntered = value; }
            }
            public virtual bool P31MouseButtonDown
            {
                get { return p31MouseButtonDown; }
                set { p31MouseButtonDown = value; }
            }
            public virtual MouseButtons P31MouseButton
            {
                get { return p31MouseButton; }
                set { p31MouseButton = value; }
            }

            public virtual void P31MouseDown(MouseEventArgs e) { }
            public virtual void P31MouseUp(MouseEventArgs e) { }
            public virtual void P31MouseWheel(int moves) { }
            public virtual void P31Refresh() { }
        }

        internal class clsButtonBox : clsMeterItem
        {
            public enum IndicatorType
            {
                RING = 0,
                BAR_LEFT,
                BAR_RIGHT,
                BAR_BOTTOM,
                BAR_TOP,
                DOT_LEFT,
                DOT_RIGHT,
                DOT_BOTTOM,
                DOT_TOP
            }

            private int buttons;
            private int columns;
            private float margin;
            private float border;
            private float radius;
            private float heightRatio;
            private int buttonIndex;

            private string[] text;
            private bool[] on;
            private bool[] enabled;
            private bool[] visible;
            private Color[] fill;
            private Color[] hover;
            private Color[] borderColour;
            private Color[] fontColour;
            private Color[] onColour;
            private Color[] offColour;
            private string[] fontFamily;
            private FontStyle[] fontStyle;
            private float[] fontSize;
            private bool[] useIndicator;
            private bool[] useOffColour;
            private float[] indicatorWidth;
            private IndicatorType[] indicatorType;

            public clsButtonBox()
            {
                columns = 1;
                margin = 0.0f;
                border = 0.005f;
                radius = 0.01f;
                heightRatio = 1.0f;
                buttonIndex = -1;
                UpdateInterval = 50;
                Buttons = 1;
            }

            public int Buttons
            {
                get { return buttons; }
                set
                {
                    buttons = Math.Max(1, value);
                    text = new string[buttons];
                    on = new bool[buttons];
                    enabled = new bool[buttons];
                    visible = new bool[buttons];
                    fill = new Color[buttons];
                    hover = new Color[buttons];
                    borderColour = new Color[buttons];
                    fontColour = new Color[buttons];
                    onColour = new Color[buttons];
                    offColour = new Color[buttons];
                    fontFamily = new string[buttons];
                    fontStyle = new FontStyle[buttons];
                    fontSize = new float[buttons];
                    useIndicator = new bool[buttons];
                    useOffColour = new bool[buttons];
                    indicatorWidth = new float[buttons];
                    indicatorType = new IndicatorType[buttons];

                    for (int i = 0; i < buttons; i++)
                    {
                        text[i] = "";
                        on[i] = false;
                        enabled[i] = true;
                        visible[i] = true;
                        fill[i] = Color.Black;
                        hover[i] = Color.LightGray;
                        borderColour[i] = Color.White;
                        fontColour[i] = Color.White;
                        onColour[i] = Color.CornflowerBlue;
                        offColour[i] = Color.Black;
                        fontFamily[i] = "Trebuchet MS";
                        fontStyle[i] = FontStyle.Regular;
                        fontSize[i] = 18.0f;
                        useIndicator[i] = true;
                        useOffColour[i] = false;
                        indicatorWidth[i] = 0.005f;
                        indicatorType[i] = IndicatorType.RING;
                    }
                }
            }

            public int Columns { get { return columns; } set { columns = Math.Max(1, value); } }
            public float Margin { get { return margin; } set { margin = Math.Max(0, value); } }
            public float Border { get { return border; } set { border = Math.Max(0, value); } }
            public float Radius { get { return radius; } set { radius = Math.Max(0, value); } }
            public float HeightRatio { get { return heightRatio; } set { heightRatio = Math.Max(0.1f, value); } }
            public int ButtonIndex { get { return buttonIndex; } set { buttonIndex = value; } }

            public void SetText(int i, string v) { if (Valid(i)) text[i] = v ?? ""; }
            public string GetText(int i) { return Valid(i) ? text[i] : ""; }
            public void SetOn(int i, bool v) { if (Valid(i)) on[i] = v; }
            public bool GetOn(int i) { return Valid(i) && on[i]; }
            public void SetEnabled(int i, bool v) { if (Valid(i)) enabled[i] = v; }
            public bool GetEnabled(int i) { return Valid(i) && enabled[i]; }
            public void SetVisible(int i, bool v) { if (Valid(i)) visible[i] = v; }
            public bool GetVisible(int i) { return Valid(i) && visible[i]; }
            public void SetFillColour(int i, Color v) { if (Valid(i)) fill[i] = v; }
            public Color GetFillColour(int i) { return Valid(i) ? fill[i] : Color.Black; }
            public void SetHoverColour(int i, Color v) { if (Valid(i)) hover[i] = v; }
            public Color GetHoverColour(int i) { return Valid(i) ? hover[i] : Color.LightGray; }
            public void SetBorderColour(int i, Color v) { if (Valid(i)) borderColour[i] = v; }
            public Color GetBorderColour(int i) { return Valid(i) ? borderColour[i] : Color.White; }
            public void SetFontColour(int i, Color v) { if (Valid(i)) fontColour[i] = v; }
            public Color GetFontColour(int i) { return Valid(i) ? fontColour[i] : Color.White; }
            public void SetOnColour(int i, Color v) { if (Valid(i)) onColour[i] = v; }
            public Color GetOnColour(int i) { return Valid(i) ? onColour[i] : Color.CornflowerBlue; }
            public void SetOffColour(int i, Color v) { if (Valid(i)) offColour[i] = v; }
            public Color GetOffColour(int i) { return Valid(i) ? offColour[i] : Color.Black; }
            public void SetFontFamily(int i, string v) { if (Valid(i)) fontFamily[i] = v ?? "Trebuchet MS"; }
            public string GetFontFamily(int i) { return Valid(i) ? fontFamily[i] : "Trebuchet MS"; }
            public void SetFontStyle(int i, FontStyle v) { if (Valid(i)) fontStyle[i] = v; }
            public FontStyle GetFontStyle(int i) { return Valid(i) ? fontStyle[i] : FontStyle.Regular; }
            public void SetFontSize(int i, float v) { if (Valid(i)) fontSize[i] = v; }
            public float GetFontSize(int i) { return Valid(i) ? fontSize[i] : 18.0f; }
            public void SetUseIndicator(int i, bool v) { if (Valid(i)) useIndicator[i] = v; }
            public bool GetUseIndicator(int i) { return Valid(i) && useIndicator[i]; }
            public void SetUseOffColour(int i, bool v) { if (Valid(i)) useOffColour[i] = v; }
            public bool GetUseOffColour(int i) { return Valid(i) && useOffColour[i]; }
            public void SetIndicatorWidth(int i, float v) { if (Valid(i)) indicatorWidth[i] = v; }
            public float GetIndicatorWidth(int i) { return Valid(i) ? indicatorWidth[i] : 0.005f; }
            public void SetIndicatorType(int i, IndicatorType v) { if (Valid(i)) indicatorType[i] = v; }
            public IndicatorType GetIndicatorType(int i) { return Valid(i) ? indicatorType[i] : IndicatorType.RING; }

            private bool Valid(int i) { return i >= 0 && i < buttons; }

            public float P31LayoutHeight()
            {
                int visibleCount = 0;
                for (int i = 0; i < Buttons; i++) if (GetVisible(i)) visibleCount++;
                if (visibleCount < 1) return 0.08f;
                int rows = (visibleCount + Columns - 1) / Columns;
                return (((1.0f - 0.04f) / Columns) * HeightRatio) * rows;
            }
        }

        internal sealed class clsBandButtonBox : clsButtonBox
        {
            private static readonly Band[] P31Bands = new Band[]
            {
                Band.B160M, Band.B80M, Band.B60M, Band.B40M, Band.B30M, Band.B20M,
                Band.B17M, Band.B15M, Band.B12M, Band.B10M, Band.B6M
            };
            private static readonly string[] P31Names = new string[]
            {
                "160", "80", "60", "40", "30", "20", "17", "15", "12", "10", "6"
            };
            private static readonly double[] P31CentersMHz = new double[]
            {
                1.900, 3.750, 5.350, 7.100, 10.125, 14.200, 18.100, 21.200, 24.950, 28.400, 50.100
            };

            public clsBandButtonBox()
            {
                ItemType = MeterItemType.BAND_BUTTONS;
                Buttons = P31Bands.Length;
                for (int i = 0; i < Buttons; i++)
                {
                    SetText(i, P31Names[i]);
                    SetFontColour(i, Color.White);
                    SetUseIndicator(i, true);
                }
                P31Refresh();
            }

            public override void P31Refresh()
            {
                if (_console == null) return;
                Band current = _console.RX1Band;
                for (int i = 0; i < Buttons; i++) SetOn(i, P31Bands[i] == current);
            }

            public override void P31MouseUp(MouseEventArgs e)
            {
                if (_console == null || ButtonIndex < 0 || ButtonIndex >= P31Bands.Length) return;
                int i = ButtonIndex;
                _console.BeginInvoke(new MethodInvoker(delegate
                {
                    // PowerSDR uses VFO frequency as the authoritative band-selection path.
                    // This is the target adapter replacing Thetis BandPreChangeHandlers.
                    _console.VFOAFreq = P31CentersMHz[i];
                }));
            }
        }

        internal sealed class clsModeButtonBox : clsButtonBox
        {
            private static readonly DSPMode[] P31Modes = new DSPMode[]
            {
                DSPMode.LSB, DSPMode.USB, DSPMode.DSB, DSPMode.CWL, DSPMode.CWU, DSPMode.FM,
                DSPMode.AM, DSPMode.SAM, DSPMode.DIGL, DSPMode.DIGU, DSPMode.SPEC, DSPMode.DRM
            };

            public clsModeButtonBox()
            {
                ItemType = MeterItemType.MODE_BUTTONS;
                Buttons = P31Modes.Length;
                for (int i = 0; i < Buttons; i++)
                {
                    SetText(i, P31Modes[i].ToString());
                    SetFontColour(i, Color.White);
                    SetUseIndicator(i, true);
                }
                P31Refresh();
            }

            public override void P31Refresh()
            {
                if (_console == null) return;
                DSPMode current = _console.RX1DSPMode;
                for (int i = 0; i < Buttons; i++) SetOn(i, P31Modes[i] == current);
            }

            public override void P31MouseUp(MouseEventArgs e)
            {
                if (_console == null || ButtonIndex < 0 || ButtonIndex >= P31Modes.Length) return;
                DSPMode mode = P31Modes[ButtonIndex];
                _console.BeginInvoke(new MethodInvoker(delegate { _console.RX1DSPMode = mode; }));
            }
        }

        internal sealed class clsTunestepButtons : clsButtonBox
        {
            public clsTunestepButtons()
            {
                ItemType = MeterItemType.TUNESTEP_BUTTONS;
                int n = (_console != null && _console.TuneStepList != null) ? _console.TuneStepList.Count : 1;
                Buttons = Math.Max(1, n);
                for (int i = 0; i < Buttons; i++)
                {
                    string s = i.ToString();
                    if (_console != null && _console.TuneStepList != null && i < _console.TuneStepList.Count)
                        s = _console.TuneStepList[i].Name.Replace("Hz", "");
                    SetText(i, s);
                    SetFontColour(i, Color.White);
                    SetUseIndicator(i, true);
                    // Thetis 3dbd default shows the first ten tune-step buttons.
                    SetVisible(i, i < 10);
                }
                P31Refresh();
            }

            public override void P31Refresh()
            {
                if (_console == null) return;
                int idx = _console.TuneStepIndex;
                for (int i = 0; i < Buttons; i++) SetOn(i, i == idx);
            }

            public override void P31MouseUp(MouseEventArgs e)
            {
                if (_console == null || ButtonIndex < 0 || ButtonIndex >= Buttons) return;
                int idx = ButtonIndex;
                _console.BeginInvoke(new MethodInvoker(delegate { _console.TuneStepIndex = idx; }));
            }
        }

        internal sealed class clsVfoDisplay : clsMeterItem
        {
            public Color FrequencyColour = Color.Orange;
            public Color ModeColour = Color.Gray;
            public Color BandColour = Color.White;
            public Color StepColour = Color.CornflowerBlue;
            public string FontFamily = "Trebuchet MS";
            public FontStyle Style = FontStyle.Regular;
            public float FontSize = 18.0f;

            public clsVfoDisplay()
            {
                ItemType = MeterItemType.VFO_DISPLAY;
                StoreSettings = false;
                UpdateInterval = 50;
            }

            public override void P31MouseWheel(int moves)
            {
                if (_console == null || moves == 0) return;
                double hz = _console.CurrentTuneStepHz;
                _console.BeginInvoke(new MethodInvoker(delegate
                {
                    _console.VFOAFreq += (Math.Sign(moves) * hz) / 1000000.0;
                }));
            }

            public override void P31MouseUp(MouseEventArgs e)
            {
                if (_console == null) return;
                double hz = _console.CurrentTuneStepHz;
                int sign = e.Button == MouseButtons.Right ? -1 : 1;
                _console.BeginInvoke(new MethodInvoker(delegate
                {
                    _console.VFOAFreq += (sign * hz) / 1000000.0;
                }));
            }
        }

        public partial class clsMeter
        {
            public string P31AddVFODisplay(int nMSupdate, float fTop, out float fBottom, clsItemGroup restoreIg)
            {
                clsItemGroup ig = new clsItemGroup();
                if (restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                clsSolidColour sc = new clsSolidColour();
                sc.ParentID = ig.ID;
                sc.TopLeft = new PointF(_fPadX, fTop + _fPadY - _fHeight * 0.75f);
                sc.Size = new SizeF(1.0f - _fPadX * 2.0f, (_fHeight + _fHeight * 0.75f) * 1.55f);
                sc.Colour = Color.FromArgb(32, 32, 32);
                sc.ZOrder = 1;
                addMeterItem(sc);

                clsVfoDisplay vfo = new clsVfoDisplay();
                vfo.ParentID = ig.ID;
                vfo.Primary = true;
                vfo.TopLeft = sc.TopLeft;
                vfo.Size = sc.Size;
                vfo.ZOrder = 2;
                addMeterItem(vfo);

                fBottom = sc.TopLeft.Y + sc.Size.Height;
                ig.TopLeft = sc.TopLeft;
                ig.Size = new SizeF(sc.Size.Width, fBottom);
                ig.MeterType = MeterType.VFO_DISPLAY;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;
                addMeterItem(ig);
                return vfo.ID;
            }

            public string P31AddBandButtons(int nMSupdate, float fTop, out float fBottom, clsItemGroup restoreIg)
            {
                return P31AddButtonBox(new clsBandButtonBox(), MeterType.BAND_BUTTONS, fTop, out fBottom, restoreIg);
            }

            public string P31AddModeButtons(int nMSupdate, float fTop, out float fBottom, clsItemGroup restoreIg)
            {
                return P31AddButtonBox(new clsModeButtonBox(), MeterType.MODE_BUTTONS, fTop, out fBottom, restoreIg);
            }

            public string P31AddTunestepButtons(int nMSupdate, float fTop, out float fBottom, clsItemGroup restoreIg)
            {
                return P31AddButtonBox(new clsTunestepButtons(), MeterType.TUNESTEP_BUTTONS, fTop, out fBottom, restoreIg);
            }

            private string P31AddButtonBox(clsButtonBox bb, MeterType mt, float fTop, out float fBottom, clsItemGroup restoreIg)
            {
                clsItemGroup ig = new clsItemGroup();
                if (restoreIg != null) ig.ID = restoreIg.ID;
                ig.ParentID = ID;

                bb.ParentID = ig.ID;
                bb.Primary = true;
                bb.TopLeft = new PointF(_fPadX, fTop + _fPadY - (_fHeight * 0.75f));
                bb.Size = new SizeF(1.0f - _fPadX * 2.0f, 1.0f);
                bb.ZOrder = 1;
                bb.Columns = 3;
                bb.Margin = 0.005f;
                bb.Radius = 0.01f;
                bb.HeightRatio = 0.5f;
                bb.Border = 0.005f;
                bb.Size = new SizeF(bb.Size.Width, bb.P31LayoutHeight());
                addMeterItem(bb);

                fBottom = bb.TopLeft.Y + bb.Size.Height;
                ig.TopLeft = bb.TopLeft;
                ig.Size = new SizeF(bb.Size.Width, fBottom);
                ig.MeterType = mt;
                ig.Order = restoreIg == null ? numberOfMeterGroups() : restoreIg.Order;
                addMeterItem(ig);
                return bb.ID;
            }
        }

        public static bool ContainerLocked(string id)
        {
            return _lstUCMeters.ContainsKey(id) && _lstUCMeters[id].Locked;
        }

        public static void LockContainer(string id, bool locked)
        {
            if (_lstUCMeters.ContainsKey(id)) _lstUCMeters[id].Locked = locked;
        }

        public static bool ContainerNoTitleBar(string id)
        {
            return _lstUCMeters.ContainsKey(id) && _lstUCMeters[id].NoControls;
        }

        public static void ContainerNoTitleBar(string id, bool noControls)
        {
            if (_lstUCMeters.ContainsKey(id)) _lstUCMeters[id].NoControls = noControls;
        }

        private partial class DXRenderer
        {
            private void P31RenderVfoDisplay(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsVfoDisplay vfo = mi as clsVfoDisplay;
                if (vfo == null || _console == null) return;

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                string freq = P31FormatFrequency(_console.VFOAFreq);
                float big = Math.Max(11.0f, (vfo.FontSize / 16.0f) * (rect.Width / 28.0f));
                float small = Math.Max(7.0f, big * 0.42f);

                SharpDX.RectangleF rf = new SharpDX.RectangleF(x + w * 0.02f, y + h * 0.04f, w * 0.96f, h * 0.60f);
                P31DrawCentered(freq, vfo.FontFamily, big, FontStyle.Regular, rf, vfo.FrequencyColour);

                string band = P31BandText(_console.RX1Band);
                string mode = _console.RX1DSPMode.ToString();
                string step = "";
                try { step = _console.TuneStepList[_console.TuneStepIndex].Name; } catch { step = _console.CurrentTuneStepHz.ToString() + " Hz"; }

                float third = w / 3.0f;
                P31DrawCentered(band, vfo.FontFamily, small, FontStyle.Regular,
                    new SharpDX.RectangleF(x, y + h * 0.62f, third, h * 0.32f), vfo.BandColour);
                P31DrawCentered(mode, vfo.FontFamily, small, FontStyle.Regular,
                    new SharpDX.RectangleF(x + third, y + h * 0.62f, third, h * 0.32f), vfo.ModeColour);
                P31DrawCentered(step, vfo.FontFamily, small, FontStyle.Regular,
                    new SharpDX.RectangleF(x + third * 2.0f, y + h * 0.62f, third, h * 0.32f), vfo.StepColour);
            }

            private static string P31FormatFrequency(double mhz)
            {
                long hz = (long)Math.Round(mhz * 1000000.0);
                long m = hz / 1000000;
                long k = (hz / 1000) % 1000;
                long h = hz % 1000;
                return m.ToString() + "." + k.ToString("000") + "." + h.ToString("000");
            }

            private static string P31BandText(Band b)
            {
                string s = b.ToString();
                if (s.StartsWith("B")) s = s.Substring(1);
                return s;
            }

            private void P31RenderButtonBox(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsButtonBox bb = mi as clsButtonBox;
                if (bb == null || bb.Columns <= 0) return;
                bb.P31Refresh();

                float x = (mi.DisplayTopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.DisplayTopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                int visibleCount = 0;
                for (int i = 0; i < bb.Buttons; i++) if (bb.GetVisible(i)) visibleCount++;
                if (visibleCount < 1) return;

                int rows = (visibleCount + bb.Columns - 1) / bb.Columns;
                float largest = Math.Max(w, h);
                float margin = bb.Margin * largest;
                float border = Math.Max(1.0f, bb.Border * largest);
                float radius = bb.Radius * largest;
                float cellW = w / bb.Columns;
                float cellH = h / rows;

                int visibleIndex = 0;
                int hit = -1;
                for (int i = 0; i < bb.Buttons; i++)
                {
                    if (!bb.GetVisible(i)) continue;
                    int row = visibleIndex / bb.Columns;
                    int col = visibleIndex % bb.Columns;
                    visibleIndex++;

                    SharpDX.RectangleF r = new SharpDX.RectangleF(
                        x + col * cellW + margin,
                        y + row * cellH + margin,
                        Math.Max(1.0f, cellW - margin * 2.0f),
                        Math.Max(1.0f, cellH - margin * 2.0f));

                    SharpDX.Direct2D1.RoundedRectangle rr = new SharpDX.Direct2D1.RoundedRectangle();
                    rr.Rect = r;
                    rr.RadiusX = radius;
                    rr.RadiusY = radius;

                    Color bg = bb.GetOn(i) ? bb.GetOnColour(i) :
                               (bb.GetUseOffColour(i) ? bb.GetOffColour(i) : bb.GetFillColour(i));

                    if (!bb.GetEnabled(i)) bg = Color.FromArgb(255, bg.R / 3, bg.G / 3, bg.B / 3);

                    bool over = bb.P31MouseEntered && r.Contains(new SharpDX.Point((int)bb.P31MouseMovePoint.X, (int)bb.P31MouseMovePoint.Y));
                    if (over)
                    {
                        bg = bb.GetHoverColour(i);
                        hit = i;
                    }

                    _renderTarget.FillRoundedRectangle(rr, getDXBrushForColour(bg));
                    _renderTarget.DrawRoundedRectangle(rr, getDXBrushForColour(bb.GetBorderColour(i)), border);

                    if (bb.GetUseIndicator(i) && bb.GetOn(i))
                    {
                        float iw = Math.Max(2.0f, bb.GetIndicatorWidth(i) * largest);
                        SharpDX.RectangleF ind = new SharpDX.RectangleF(r.Left + iw, r.Bottom - iw * 2.0f, Math.Max(1.0f, r.Width - iw * 2.0f), iw);
                        _renderTarget.FillRectangle(ind, getDXBrushForColour(bb.GetOnColour(i)));
                    }

                    float fs = Math.Max(7.0f, (bb.GetFontSize(i) / 16.0f) * (rect.Width / 52.0f));
                    P31DrawCentered(bb.GetText(i), bb.GetFontFamily(i), fs, bb.GetFontStyle(i), r, bb.GetFontColour(i));
                }
                bb.ButtonIndex = hit;
            }

            private void P31DrawCentered(string text, string family, float em, FontStyle style, SharpDX.RectangleF rect, Color colour)
            {
                SharpDX.DirectWrite.TextFormat tf = getDXTextFormatForFont(family, em, style);
                if (tf == null) return;
                SharpDX.DirectWrite.TextAlignment oldA = tf.TextAlignment;
                SharpDX.DirectWrite.ParagraphAlignment oldP = tf.ParagraphAlignment;
                tf.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                tf.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                _renderTarget.DrawText(text ?? "", tf, rect, getDXBrushForColour(colour));
                tf.TextAlignment = oldA;
                tf.ParagraphAlignment = oldP;
            }

            private void P31MouseDown(object sender, MouseEventArgs e)
            {
                P31DispatchMouse(sender, e, 0);
            }

            private void P31MouseMove(object sender, MouseEventArgs e)
            {
                P31DispatchMouse(sender, e, 1);
            }

            private void P31MouseWheel(object sender, MouseEventArgs e)
            {
                P31DispatchMouse(sender, e, 2);
            }

            private void P31MouseEnter(object sender, EventArgs e)
            {
                PictureBox pb = sender as PictureBox;
                if (pb == null || pb.Tag == null) return;
                string id = pb.Tag.ToString();
                if (!_meters.ContainsKey(id)) return;
                clsMeter m = _meters[id];
                foreach (KeyValuePair<string, clsMeterItem> kv in m.SortedMeterItemsForZOrder)
                    kv.Value.P31MouseEntered = true;
            }

            private void P31MouseLeave(object sender, EventArgs e)
            {
                PictureBox pb = sender as PictureBox;
                if (pb == null || pb.Tag == null) return;
                string id = pb.Tag.ToString();
                if (!_meters.ContainsKey(id)) return;
                clsMeter m = _meters[id];
                foreach (KeyValuePair<string, clsMeterItem> kv in m.SortedMeterItemsForZOrder)
                    kv.Value.P31MouseEntered = false;
            }

            internal void P31DispatchMouseUp(object sender, MouseEventArgs e)
            {
                P31DispatchMouse(sender, e, 3);
            }

            private void P31DispatchMouse(object sender, MouseEventArgs e, int kind)
            {
                PictureBox pb = sender as PictureBox;
                if (pb == null || pb.Tag == null) return;
                string id = pb.Tag.ToString();
                if (!_meters.ContainsKey(id)) return;
                clsMeter m = _meters[id];
                if (m.SortedMeterItemsForZOrder == null) return;

                float tw = targetWidth - 1.0f;
                SharpDX.RectangleF root = new SharpDX.RectangleF(0, 0, tw * m.XRatio, tw * m.YRatio);

                foreach (KeyValuePair<string, clsMeterItem> kv in m.SortedMeterItemsForZOrder)
                {
                    clsMeterItem mi = kv.Value;
                    if (mi.ItemType != clsMeterItem.MeterItemType.VFO_DISPLAY &&
                        mi.ItemType != clsMeterItem.MeterItemType.BAND_BUTTONS &&
                        mi.ItemType != clsMeterItem.MeterItemType.MODE_BUTTONS &&
                        mi.ItemType != clsMeterItem.MeterItemType.TUNESTEP_BUTTONS) continue;

                    float x = (mi.DisplayTopLeft.X / m.XRatio) * root.Width;
                    float y = (mi.DisplayTopLeft.Y / m.YRatio) * root.Height;
                    float w = root.Width * (mi.Size.Width / m.XRatio);
                    float h = root.Height * (mi.Size.Height / m.YRatio);
                    SharpDX.RectangleF hit = new SharpDX.RectangleF(x, y, w, h);
                    bool inside = hit.Contains(new SharpDX.Point(e.X, e.Y));

                    mi.P31MouseEntered = inside;
                    if (inside) mi.P31MouseMovePoint = new PointF(e.X, e.Y);

                    clsButtonBox bb = mi as clsButtonBox;
                    if (inside && bb != null) P31UpdateButtonIndex(bb, hit, e.X, e.Y);

                    if (!inside) continue;
                    if (kind == 0)
                    {
                        mi.P31MouseButton = e.Button;
                        mi.P31MouseButtonDown = true;
                        mi.P31MouseDown(e);
                    }
                    else if (kind == 2)
                    {
                        int moves = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
                        mi.P31MouseWheel(moves);
                    }
                    else if (kind == 3)
                    {
                        mi.P31MouseButton = e.Button;
                        mi.P31MouseButtonDown = false;
                        mi.P31MouseUp(e);
                    }
                }
            }

            private void P31UpdateButtonIndex(clsButtonBox bb, SharpDX.RectangleF r, int mx, int my)
            {
                int visibleCount = 0;
                for (int i = 0; i < bb.Buttons; i++) if (bb.GetVisible(i)) visibleCount++;
                int rows = Math.Max(1, (visibleCount + bb.Columns - 1) / bb.Columns);
                float cellW = r.Width / bb.Columns;
                float cellH = r.Height / rows;
                int col = Math.Max(0, Math.Min(bb.Columns - 1, (int)((mx - r.Left) / cellW)));
                int row = Math.Max(0, Math.Min(rows - 1, (int)((my - r.Top) / cellH)));
                int visibleTarget = row * bb.Columns + col;
                int n = -1;
                int v = 0;
                for (int i = 0; i < bb.Buttons; i++)
                {
                    if (!bb.GetVisible(i)) continue;
                    if (v == visibleTarget) { n = i; break; }
                    v++;
                }
                bb.ButtonIndex = n;
            }
        }
    }
}
