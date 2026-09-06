// SQ4KOU P03: Thetis-style RX1 PANADAPTER for KE9NS PowerSDR 2.8.0.334.
// Rendering only.  Acquisition remains native PowerSDR/DttSP and all FLEX-5000
// PAL/FWC/FireWire/ASIO paths remain untouched.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PowerSDR
{
    sealed partial class Display
    {
        unsafe private static bool SQ4KOU_DrawCleanPanadapter(Graphics g, int width, int fullHeight)
        {
            if (g == null || width < 64 || fullHeight < 82) return false;
            if (console == null || console.setupForm == null) return false;

            int panHeight = fullHeight - SQ4KOU_FREQ_SCALE_HEIGHT;
            if (panHeight < 64) return false;

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

            if (data_ready)
            {
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));
                data_ready = false;
            }

            if (average_on)
                console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);
            if (peak_on)
                UpdateDisplayPeak(rx1_peak_buffer, current_display_data);

            EnsureSQ4KOUArrays(width);
            if (!SQ4KOU_ResampleRX1(width, low, high, current_display_data, sq4kou_pan_values))
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

                float yf = (spectrum_grid_max - value) * panHeight / (float)yRange;
                if (yf < 0) yf = 0;
                if (yf > panHeight - 1) yf = panHeight - 1;
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

                // Use exactly the same visual primitives as the confirmed P02
                // PANAFALL renderer: background, grid, filter, fill, trace and ruler.
                using (SolidBrush bg = new SolidBrush(display_background_color))
                    g.FillRectangle(bg, 0, 0, width, fullHeight);

                SQ4KOU_DrawPanGrid(g, width, panHeight, low, high);
                SQ4KOU_DrawFilter(g, width, panHeight, low, high);

                if (pan_fill && width > 1)
                {
                    PointF[] polygon = new PointF[width + 2];
                    Array.Copy(sq4kou_pan_points, polygon, width);
                    polygon[width] = new PointF(width - 1, panHeight - 1);
                    polygon[width + 1] = new PointF(0, panHeight - 1);
                    int alpha = panfillalpha;
                    if (alpha < 0) alpha = 0;
                    if (alpha > 180) alpha = 180;
                    using (SolidBrush fill = new SolidBrush(Color.FromArgb(alpha, data_line_pen.Color)))
                        g.FillPolygon(fill, polygon);
                }

                if (width > 1)
                    g.DrawLines(data_line_pen, sq4kou_pan_points);

                SQ4KOU_DrawFrequencyScale(g, width, panHeight, low, high);
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
    }
}
