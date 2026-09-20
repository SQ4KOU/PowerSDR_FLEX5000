using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static class P24ThetisMeterCompat
    {
        internal static bool CtrlKeyDown
        {
            get { return (Control.ModifierKeys & Keys.Control) == Keys.Control; }
        }

        internal static bool ShiftKeyDown
        {
            get { return (Control.ModifierKeys & Keys.Shift) == Keys.Shift; }
        }

        internal static void DoubleBufferAll(Control root, bool enabled)
        {
            if (root == null) return;
            try
            {
                PropertyInfo p = typeof(Control).GetProperty(
                    "DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                if (p != null) p.SetValue(root, enabled, null);
            }
            catch { }

            foreach (Control child in root.Controls)
                DoubleBufferAll(child, enabled);
        }

        internal static string ColourToString(Color c)
        {
            return c.ToArgb().ToString(CultureInfo.InvariantCulture);
        }

        internal static Color ColourFromString(string s)
        {
            if (String.IsNullOrWhiteSpace(s)) return Color.Black;
            int argb;
            if (Int32.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out argb))
                return Color.FromArgb(argb);
            try { return ColorTranslator.FromHtml(s); }
            catch { return Color.Black; }
        }

        internal static int FiveDigitHash(string text)
        {
            unchecked
            {
                uint hash = 2166136261;
                string s = text ?? "";
                for (int i = 0; i < s.Length; i++)
                {
                    hash ^= s[i];
                    hash *= 16777619;
                }
                return (int)(hash % 100000);
            }
        }

        internal static bool TouchSupport(PowerSDR.Console console)
        {
            // FLEX-5000 PowerSDR has no native Thetis touch API.
            // Keep the exact mouse container behaviour; touch is simply unavailable.
            return false;
        }

        internal static (bool resized, bool relocated) ForceFormOnScreen(Form form, bool shrinkToFit)
        {
            if (form == null) return (false, false);
            Rectangle before = form.Bounds;
            bool resized = false;

            if (shrinkToFit)
            {
                Screen s = Screen.FromControl(form);
                Rectangle wa = s == null ? Screen.PrimaryScreen.WorkingArea : s.WorkingArea;
                int w = Math.Min(form.Width, wa.Width);
                int h = Math.Min(form.Height, wa.Height);
                if (w != form.Width || h != form.Height)
                {
                    form.Size = new Size(w, h);
                    resized = true;
                }
            }

            try { Common.ForceFormOnScreen(form); } catch { }
            bool relocated = before.Location != form.Location;
            return (resized, relocated);
        }

        internal static (bool in_use, bool enabled) GetXPAStatus(object console)
        {
            // FLEX-5000 has no Thetis XPA state. Keep the UI item valid but explicitly inactive.
            return (false, false);
        }

        internal static SpecHPSDR GetSpectrumSpec(object console, int id)
        {
            SpecHPSDR spec = new SpecHPSDR(id);
            spec.Update = false;
            spec.initAnalyzer();
            return spec;
        }

        internal static void StopPlayback(object console) { }
        internal static void StopRecord(object console) { }
    }
}
