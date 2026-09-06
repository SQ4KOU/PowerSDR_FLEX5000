// SQ4KOU display subsystem for KE9NS PowerSDR 2.8.0.334
//
// Scope: RX1 PANAFALL rendering only.  The native PowerSDR DttSP acquisition,
// radio control, FLEX-5000 PAL/FWC/FireWire/ASIO paths, Console control tree,
// Skin and mouse/tuning handlers are intentionally not replaced.
//
// The mechanism follows the useful part of the modern Thetis display design:
// explicit pan/ruler/waterfall rectangles and a waterfall bitmap owned by the
// renderer.  It consumes the existing PowerSDR display buffers rather than any
// HPSDR/Thetis backend.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;

namespace PowerSDR
{
    sealed partial class Display
    {
        private const int SQ4KOU_FREQ_SCALE_HEIGHT = 18;

        private static Bitmap sq4kou_waterfall_bmp;
        private static int sq4kou_waterfall_w = -1;
        private static int sq4kou_waterfall_h = -1;
        private static int sq4kou_waterfall_low = Int32.MinValue;
        private static int sq4kou_waterfall_high = Int32.MinValue;
        private static long sq4kou_waterfall_vfo_anchor = Int64.MinValue;
        private static DateTime sq4kou_waterfall_last_update = DateTime.MinValue;

        private static PointF[] sq4kou_pan_points;
        private static float[] sq4kou_pan_values;
        private static float[] sq4kou_waterfall_values;

        // Keep every specialised KE9NS display feature on its original renderer
        // until it is explicitly ported.  There is never an overlay of old and
        // new renderers: a frame is produced by exactly one path.
        private static bool SQ4KOU_CanUseCleanPanafall()
        {
            if (console == null || console.setupForm == null) return false;
            if (mox) return false;                       // preserve native TX/CW visual path
            if (map != 0 || continuum != 0) return false;
            if (autobright != 0 || autobright2 != 0 || autobright3 != 0) return false;
            if (sub_rx1_enabled) return false;
            if (console.N1MM_ON || console.BeaconSigAvg) return false;
            if (console.setupForm.check3DPan.Checked) return false;
            if (average_on && console.setupForm.chkAvgMove.Checked) return false;

            if (console.ScanForm != null)
            {
                if (console.ScanForm.chkBoxIdent.Checked) return false;
            }

            return true;
        }

        // Called only for the normal, unsplit RX1 PANAFALL case.  If a geometry
        // or state precondition is invalid the caller falls through to the
        // untouched KE9NS renderer for that frame.
        unsafe private static bool SQ4KOU_DrawCleanPanafall(Graphics g, int width, int fullHeight)
        {
            if (g == null || width < 64 || fullHeight < 120) return false;
            if (console == null || console.setupForm == null) return false;

            int dividerY = (int)console.setupForm.udSS2H.Value + (fullHeight / 2);
            int minPan = 64;
            int minWater = 48;
            if (dividerY < minPan) dividerY = minPan;
            if (dividerY > fullHeight - SQ4KOU_FREQ_SCALE_HEIGHT - minWater)
                dividerY = fullHeight - SQ4KOU_FREQ_SCALE_HEIGHT - minWater;

            int waterfallY = dividerY + SQ4KOU_FREQ_SCALE_HEIGHT;
            int waterfallHeight = fullHeight - waterfallY;
            if (waterfallHeight < 1) return false;

            int low;
            int high;
            if (Console.UPDATEOFF > 0)
            {
                low = LowLast;
                high = HighLast;
            }
            else
            {
                low = rx_display_low;
                high = rx_display_high;
                LowLast = low;
                HighLast = high;
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                low += 12000;
                high += 12000;
            }

            if (high <= low || sample_rate <= 0) return false;

            // One acquisition/copy step feeds both the panadapter and waterfall.
            // This is the central architectural change: rendering no longer
            // consumes the DttSP buffer twice for a single PANAFALL frame.
            if (data_ready)
            {
                fixed (void* rptr = &new_display_data[0])
                {
                    fixed (void* wptr = &current_display_data[0])
                        Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));

                    fixed (void* wptr = &current_display_data1[0])
                        Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                }
                data_ready = false;
            }

            if (average_on)
                console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
            if (peak_on)
                UpdateDisplayPeak(rx1_peak_buffer, current_display_data);

            EnsureSQ4KOUArrays(width);
            if (!SQ4KOU_ResampleRX1(width, low, high, current_display_data, sq4kou_pan_values))
                return false;

            // The waterfall can deliberately bypass PAN averaging using the
            // existing KE9NS PW_AVG option.  The raw companion buffer is filled
            // in the same acquisition copy above.
            float[] wfSource = (((pw_avg & 1) == 0) ? current_display_data : current_display_data1);
            if (!SQ4KOU_ResampleRX1(width, low, high, wfSource, sq4kou_waterfall_values))
                return false;

            float localMax = Single.MinValue;
            int localMaxX = 0;
            int yRange = spectrum_grid_max - spectrum_grid_min;
            if (yRange <= 0) yRange = 1;

            for (int x = 0; x < width; x++)
            {
                float value = sq4kou_pan_values[x];
                if (value > localMax)
                {
                    localMax = value;
                    localMaxX = x;
                }

                float yf = (spectrum_grid_max - value) * dividerY / (float)yRange;
                if (yf < 0) yf = 0;
                if (yf > dividerY - 1) yf = dividerY - 1;
                sq4kou_pan_points[x] = new PointF(x, yf);
            }

            max_y = localMax;
            max_x = localMaxX;

            SmoothingMode oldSmoothing = g.SmoothingMode;
            PixelOffsetMode oldPixelOffset = g.PixelOffsetMode;
            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (SolidBrush bg = new SolidBrush(display_background_color))
                    g.FillRectangle(bg, 0, 0, width, fullHeight);

                SQ4KOU_DrawPanGrid(g, width, dividerY, low, high);
                SQ4KOU_DrawFilter(g, width, dividerY, low, high);

                if (pan_fill && width > 1)
                {
                    PointF[] polygon = new PointF[width + 2];
                    Array.Copy(sq4kou_pan_points, polygon, width);
                    polygon[width] = new PointF(width - 1, dividerY - 1);
                    polygon[width + 1] = new PointF(0, dividerY - 1);
                    int alpha = panfillalpha;
                    if (alpha < 0) alpha = 0;
                    if (alpha > 180) alpha = 180;
                    using (SolidBrush fill = new SolidBrush(Color.FromArgb(alpha, data_line_pen.Color)))
                        g.FillPolygon(fill, polygon);
                }

                if (width > 1)
                    g.DrawLines(data_line_pen, sq4kou_pan_points);

                SQ4KOU_DrawFrequencyScale(g, width, dividerY, low, high);
                SQ4KOU_UpdateAndDrawWaterfall(g, width, waterfallY, waterfallHeight, low, high);
                SQ4KOU_DrawCursor(g, width, fullHeight);

                if (high_swr)
                {
                    using (Font f = new Font("Arial", 14, FontStyle.Bold))
                    using (Brush b = new SolidBrush(Color.Red))
                        g.DrawString("High SWR", f, b, 245, 20);
                }
            }
            finally
            {
                g.SmoothingMode = oldSmoothing;
                g.PixelOffsetMode = oldPixelOffset;
            }

            return true;
        }

        private static void EnsureSQ4KOUArrays(int width)
        {
            if (sq4kou_pan_points == null || sq4kou_pan_points.Length != width)
                sq4kou_pan_points = new PointF[width];
            if (sq4kou_pan_values == null || sq4kou_pan_values.Length != width)
                sq4kou_pan_values = new float[width];
            if (sq4kou_waterfall_values == null || sq4kou_waterfall_values.Length != width)
                sq4kou_waterfall_values = new float[width];
        }

        private static int SQ4KOU_Mod(int value, int modulus)
        {
            int r = value % modulus;
            return r < 0 ? r + modulus : r;
        }

        // PowerSDR/DttSP owns the FFT buffer.  This method is only the adapter
        // from its 4096-point (or current BUFFER_SIZE) data to display pixels.
        private static bool SQ4KOU_ResampleRX1(int width, int low, int high, float[] source, float[] destination)
        {
            if (source == null || destination == null || width < 1) return false;
            if (destination.Length < width || source.Length < 2) return false;
            if (sample_rate <= 0 || high <= low) return false;

            int sourceLength = source.Length;
            int start = (BUFFER_SIZE >> 1) + (int)(((long)low * (long)BUFFER_SIZE) / (long)sample_rate);
            int count = (int)(((long)(high - low) * (long)BUFFER_SIZE) / (long)sample_rate);
            if (count < 2) count = 2;
            float slope = count / (float)width;

            for (int x = 0; x < width; x++)
            {
                float dval = start + (x * slope);
                int left = (int)Math.Floor(dval);
                int right = (int)Math.Floor(dval + slope);
                float value = Single.MinValue;

                if (slope <= 1.0f || left == right)
                {
                    int i0 = SQ4KOU_Mod(left, sourceLength);
                    int i1 = SQ4KOU_Mod(left + 1, sourceLength);
                    float frac = dval - (float)Math.Floor(dval);
                    value = source[i0] * (1.0f - frac) + source[i1] * frac;
                }
                else
                {
                    if (right <= left) right = left + 1;
                    for (int i = left; i < right; i++)
                    {
                        float v = source[SQ4KOU_Mod(i, sourceLength)];
                        if (v > value) value = v;
                    }
                }

                value += rx1_display_cal_offset;
                value += rx1_preamp_offset;
                destination[x] = value;
            }

            return true;
        }

        private static void SQ4KOU_DrawPanGrid(Graphics g, int width, int height, int low, int high)
        {
            int yRange = spectrum_grid_max - spectrum_grid_min;
            if (yRange <= 0) yRange = 1;
            int dbStep = spectrum_grid_step;
            if (dbStep <= 0) dbStep = 10;

            using (Pen gridPen = new Pen(Color.FromArgb(120, grid_color)))
            using (Brush textBrush = new SolidBrush(grid_text_color))
            using (Font font = new Font("Segoe UI", 8.0f, FontStyle.Regular))
            {
                for (int db = spectrum_grid_max - dbStep; db > spectrum_grid_min; db -= dbStep)
                {
                    int y = (int)((spectrum_grid_max - db) * height / (float)yRange);
                    if (y <= 0 || y >= height) continue;
                    g.DrawLine(gridPen, 0, y, width, y);
                    g.DrawString(db.ToString(CultureInfo.InvariantCulture), font, textBrush, 3, y - 12);
                }

                double span = (double)(high - low);
                double step = SQ4KOU_NiceFrequencyStep(span / 9.0);
                double first = Math.Ceiling((vfoa_hz + low) / step) * step;
                double right = vfoa_hz + high;
                for (double freq = first; freq <= right + 0.5 * step; freq += step)
                {
                    double offset = freq - vfoa_hz;
                    int x = (int)Math.Round((offset - low) * width / span);
                    if (x < 0 || x >= width) continue;
                    g.DrawLine(gridPen, x, 0, x, height);
                }
            }

            if (low <= 0 && high >= 0)
            {
                int x0 = (int)Math.Round((-low) * width / (double)(high - low));
                using (Pen zero = new Pen(grid_zero_color, 1.0f))
                    g.DrawLine(zero, x0, 0, x0, height);
            }
        }

        private static void SQ4KOU_DrawFilter(Graphics g, int width, int height, int low, int high)
        {
            if (high <= low) return;
            int left = (int)Math.Round((rx1_filter_low - low) * width / (double)(high - low));
            int right = (int)Math.Round((rx1_filter_high - low) * width / (double)(high - low));
            if (right < left)
            {
                int t = left;
                left = right;
                right = t;
            }
            if (left < 0) left = 0;
            if (right > width) right = width;
            if (right <= left) return;

            using (SolidBrush b = new SolidBrush(display_filter_color))
                g.FillRectangle(b, left, 0, right - left, height);
        }

        private static double SQ4KOU_NiceFrequencyStep(double target)
        {
            if (target <= 1.0) return 1.0;
            double power = Math.Pow(10.0, Math.Floor(Math.Log10(target)));
            double n = target / power;
            double nice;
            if (n <= 1.0) nice = 1.0;
            else if (n <= 2.0) nice = 2.0;
            else if (n <= 5.0) nice = 5.0;
            else nice = 10.0;
            return nice * power;
        }

        private static void SQ4KOU_DrawFrequencyScale(Graphics g, int width, int y, int low, int high)
        {
            if (high <= low) return;
            using (SolidBrush bg = new SolidBrush(display_background_color))
                g.FillRectangle(bg, 0, y, width, SQ4KOU_FREQ_SCALE_HEIGHT);

            using (Pen line = new Pen(grid_color))
                g.DrawLine(line, 0, y, width, y);

            double span = (double)(high - low);
            double step = SQ4KOU_NiceFrequencyStep(span / 9.0);
            double first = Math.Ceiling((vfoa_hz + low) / step) * step;
            double right = vfoa_hz + high;

            using (Pen tick = new Pen(grid_color))
            using (Brush text = new SolidBrush(grid_text_color))
            using (Font font = new Font("Segoe UI", 8.0f, FontStyle.Regular))
            {
                for (double freq = first; freq <= right + 0.5 * step; freq += step)
                {
                    double offset = freq - vfoa_hz;
                    int x = (int)Math.Round((offset - low) * width / span);
                    if (x < 0 || x >= width) continue;
                    g.DrawLine(tick, x, y, x, y + 4);

                    string label;
                    if (show_freq_offset)
                        label = ((int)Math.Round(offset)).ToString(CultureInfo.InvariantCulture);
                    else
                        label = (freq / 1000000.0).ToString("0.000", CultureInfo.InvariantCulture);

                    SizeF size = g.MeasureString(label, font);
                    float lx = x - size.Width * 0.5f;
                    if (lx < 0) lx = 0;
                    if (lx + size.Width > width) lx = width - size.Width;
                    g.DrawString(label, font, text, lx, y + 3);
                }
            }
        }

        private static void SQ4KOU_EnsureWaterfallBitmap(int width, int height, int low, int high)
        {
            bool recreate = sq4kou_waterfall_bmp == null ||
                            sq4kou_waterfall_w != width ||
                            sq4kou_waterfall_h != height ||
                            sq4kou_waterfall_low != low ||
                            sq4kou_waterfall_high != high;

            if (recreate)
            {
                if (sq4kou_waterfall_bmp != null)
                    sq4kou_waterfall_bmp.Dispose();

                sq4kou_waterfall_bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                using (Graphics gg = Graphics.FromImage(sq4kou_waterfall_bmp))
                    gg.Clear(Color.Black);

                sq4kou_waterfall_w = width;
                sq4kou_waterfall_h = height;
                sq4kou_waterfall_low = low;
                sq4kou_waterfall_high = high;
                sq4kou_waterfall_vfo_anchor = vfoa_hz;
                sq4kou_waterfall_last_update = DateTime.MinValue;
                return;
            }

            // A VFO move keeps history aligned by shifting the existing bitmap
            // in frequency.  There is no oversized 3x/5x WaterMove surface.
            long deltaHz = vfoa_hz - sq4kou_waterfall_vfo_anchor;
            if (deltaHz != 0 && high > low)
            {
                double pixelsExact = deltaHz * width / (double)(high - low);
                int pixels = (int)Math.Round(pixelsExact);
                if (pixels != 0)
                {
                    if (Math.Abs(pixels) >= width)
                    {
                        using (Graphics gg = Graphics.FromImage(sq4kou_waterfall_bmp))
                            gg.Clear(Color.Black);
                    }
                    else
                    {
                        Bitmap shifted = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                        using (Graphics gg = Graphics.FromImage(shifted))
                        {
                            gg.Clear(Color.Black);
                            gg.DrawImageUnscaled(sq4kou_waterfall_bmp, -pixels, 0);
                        }
                        sq4kou_waterfall_bmp.Dispose();
                        sq4kou_waterfall_bmp = shifted;
                    }

                    sq4kou_waterfall_vfo_anchor = vfoa_hz;
                }
            }
        }

        unsafe private static void SQ4KOU_UpdateAndDrawWaterfall(Graphics g, int width, int y, int height, int low, int high)
        {
            SQ4KOU_EnsureWaterfallBitmap(width, height, low, high);
            if (sq4kou_waterfall_bmp == null) return;

            int period = waterfall_update_period;
            if (period < 1) period = 1;
            DateTime now = DateTime.UtcNow;
            bool updateLine = sq4kou_waterfall_last_update == DateTime.MinValue ||
                              (now - sq4kou_waterfall_last_update).TotalMilliseconds >= period;

            if (updateLine)
            {
                sq4kou_waterfall_last_update = now;
                Rectangle rect = new Rectangle(0, 0, sq4kou_waterfall_bmp.Width, sq4kou_waterfall_bmp.Height);
                BitmapData bd = sq4kou_waterfall_bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                try
                {
                    byte* basePtr = (byte*)bd.Scan0.ToPointer();
                    int stride = bd.Stride;

                    // Bottom-up row copies make the overlap explicit and safe.
                    for (int row = height - 1; row > 0; row--)
                    {
                        void* dst = basePtr + row * stride;
                        void* src = basePtr + (row - 1) * stride;
                        Win32.memcpy(dst, src, stride);
                    }

                    float lowThreshold = waterfall_low_threshold;
                    float highThreshold = waterfall_high_threshold;
                    if (highThreshold <= lowThreshold) highThreshold = lowThreshold + 1.0f;
                    float range = highThreshold - lowThreshold;
                    byte* top = basePtr;

                    for (int x = 0; x < width; x++)
                    {
                        Color c = SQ4KOU_WaterfallColor(sq4kou_waterfall_values[x], lowThreshold, highThreshold, range);
                        int p = x * 3;
                        top[p + 0] = c.B;
                        top[p + 1] = c.G;
                        top[p + 2] = c.R;
                    }
                }
                finally
                {
                    sq4kou_waterfall_bmp.UnlockBits(bd);
                }
            }

            g.DrawImageUnscaled(sq4kou_waterfall_bmp, 0, y);
        }

        private static Color SQ4KOU_WaterfallColor(float value, float low, float high, float range)
        {
            if (Gray_Scale != 0)
            {
                float p = (value - low) / range;
                if (p < 0) p = 0;
                if (p > 1) p = 1;
                int gray = (int)Math.Round(p * 255.0f);
                return Color.FromArgb(gray, gray, gray);
            }

            if (value <= low) return waterfall_low_color;
            if (value >= high) return Color.FromArgb(192, 124, 255);

            float overall = (value - low) / range;
            int max = 255;
            int r;
            int g;
            int b;

            if (overall < 2.0f / 9.0f)
            {
                float p = overall / (2.0f / 9.0f);
                r = (int)((1.0f - p) * waterfall_low_color.R);
                g = (int)((1.0f - p) * waterfall_low_color.G);
                b = (int)(waterfall_low_color.B + p * (max - waterfall_low_color.B));
            }
            else if (overall < 3.0f / 9.0f)
            {
                float p = (overall - 2.0f / 9.0f) / (1.0f / 9.0f);
                r = 0; g = (int)(p * max); b = max;
            }
            else if (overall < 4.0f / 9.0f)
            {
                float p = (overall - 3.0f / 9.0f) / (1.0f / 9.0f);
                r = 0; g = max; b = (int)((1.0f - p) * max);
            }
            else if (overall < 5.0f / 9.0f)
            {
                float p = (overall - 4.0f / 9.0f) / (1.0f / 9.0f);
                r = (int)(p * max); g = max; b = 0;
            }
            else if (overall < 7.0f / 9.0f)
            {
                float p = (overall - 5.0f / 9.0f) / (2.0f / 9.0f);
                r = max; g = (int)((1.0f - p) * max); b = 0;
            }
            else if (overall < 8.0f / 9.0f)
            {
                float p = (overall - 7.0f / 9.0f) / (1.0f / 9.0f);
                r = max; g = 0; b = (int)(p * max);
            }
            else
            {
                float p = (overall - 8.0f / 9.0f) / (1.0f / 9.0f);
                r = (int)((0.75f + 0.25f * (1.0f - p)) * max);
                g = (int)(p * max * 0.5f);
                b = max;
            }

            if (r < 0) r = 0; if (r > 255) r = 255;
            if (g < 0) g = 0; if (g > 255) g = 255;
            if (b < 0) b = 0; if (b > 255) b = 255;
            return Color.FromArgb(r, g, b);
        }

        private static void SQ4KOU_DrawCursor(Graphics g, int width, int fullHeight)
        {
            if (current_click_tune_mode == ClickTuneMode.Off) return;
            if (console != null && console.mouseinS) return;

            Color color = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_color : Color.Red;
            using (Pen p = new Pen(color))
            {
                g.DrawLine(p, display_cursor_x, 0, display_cursor_x, fullHeight);
                if (display_cursor_y >= 0 && display_cursor_y < fullHeight)
                    g.DrawLine(p, 0, display_cursor_y, width, display_cursor_y);
            }
        }
    }
}
