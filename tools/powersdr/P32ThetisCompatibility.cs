using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PowerSDR
{
    public partial class Common
    {
        public static readonly MessageBoxOptions MB_TOPMOST = (MessageBoxOptions)0x00040000L;

        public static bool CtrlKeyDown { get { return (Control.ModifierKeys & Keys.Control) != 0; } }
        public static bool ShiftKeyDown { get { return (Control.ModifierKeys & Keys.Shift) != 0; } }

        public static void DoubleBuffered(Control control, bool enabled)
        {
            if (control == null) return;
            PropertyInfo pi = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (pi != null) pi.SetValue(control, enabled, null);
        }

        public static void DoubleBufferAll(Control control, bool enabled)
        {
            if (control == null) return;
            DoubleBuffered(control, enabled);
            foreach (Control child in control.Controls) DoubleBufferAll(child, enabled);
        }

        public static (bool resized, bool relocated) ForceFormOnScreen(Form f, bool shrink_to_fit, bool keep_on_screen = false)
        {
            bool resized = false, relocated = false;
            Screen[] screens = Screen.AllScreens;
            if (screens.Length == 0)
            {
                f.Location = new Point(0, 0);
                return (false, false);
            }

            if (keep_on_screen)
            {
                Rectangle bounds = Screen.FromPoint(Cursor.Position).WorkingArea;
                if (f.Left < bounds.Left) { f.Left = bounds.Left; relocated = true; }
                if (f.Top < bounds.Top) { f.Top = bounds.Top; relocated = true; }
                if (f.Right > bounds.Right) { f.Left = bounds.Right - f.Width; relocated = true; }
                if (f.Bottom > bounds.Bottom) { f.Top = bounds.Bottom - f.Height; relocated = true; }
                if (shrink_to_fit)
                {
                    int w = f.Width, h = f.Height;
                    if (w > bounds.Width) { w = bounds.Width; resized = true; }
                    if (h > bounds.Height) { h = bounds.Height; resized = true; }
                    f.Size = new Size(w, h);
                }
            }
            else
            {
                int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
                foreach (Screen s in screens)
                {
                    if (s.Bounds.Left < left) left = s.Bounds.Left;
                    if (s.Bounds.Top < top) top = s.Bounds.Top;
                    if (s.Bounds.Right > right) right = s.Bounds.Right;
                    if (s.Bounds.Bottom > bottom) bottom = s.Bounds.Bottom;
                }
                if (f.Left < left) { f.Left = left; relocated = true; }
                if (f.Top < top) { f.Top = top; relocated = true; }
                if (f.Right > right) { f.Left = right - f.Width; relocated = true; }
                if (f.Bottom > bottom) { f.Top = bottom - f.Height; relocated = true; }
                if (shrink_to_fit)
                {
                    int w = f.Width, h = f.Height;
                    if (w > right - left) { w = right - left; resized = true; }
                    if (h > bottom - top) { h = bottom - top; resized = true; }
                    f.Size = new Size(w, h);
                }
            }
            return (resized, relocated);
        }

        public static bool IsIpv4Valid(string ip, int port)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address)) return false;
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
            if (port < 1 || port > 65535) return false;
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(ip, pattern);
        }

        public static string SerializeToBase64<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(gz, obj);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static T DeserializeFromBase64<T>(string value)
        {
            byte[] data = Convert.FromBase64String(value);
            using (MemoryStream ms = new MemoryStream(data))
            using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
            {
                IFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(gz);
            }
        }

        public static bool IsValidUri(string uri)
        {
            if (String.IsNullOrEmpty(uri)) return false;
            Uri value;
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute) &&
                   Uri.TryCreate(uri, UriKind.Absolute, out value) &&
                   (value.Scheme == Uri.UriSchemeHttp || value.Scheme == Uri.UriSchemeHttps);
        }

        private static int rGBtoLin(int col)
        {
            float c = col / 255f;
            if (c <= 0.04045f) return (int)((c / 12.92f) * 255f);
            return (int)(Math.Pow((c + 0.055f) / 1.055f, 2.4) * 255f);
        }

        public static int GetLuminance(Color c)
        {
            int r = rGBtoLin(c.R), g = rGBtoLin(c.G), b = rGBtoLin(c.B);
            return (r + r + b + g + g + g) / 6;
        }

        public static double UVfromDBM(double dbm)
        {
            return Math.Sqrt(Math.Pow(10, dbm / 10) * 50 * 1e-3) * 1e6;
        }

        public static string SMeterFromDBM(double dbm, bool above)
        {
            int s, over;
            SMeterFromDBM2(dbm, above, out s, out over);
            return "    S " + s.ToString() + (over > 0 ? " + " + over.ToString() : "");
        }

        public static void SMeterFromDBM2(double dbm, bool above, out int S, out int over9dBm)
        {
            double base9 = above ? -90.0 : -70.0;
            double base0 = above ? -144.0 : -124.0;
            if (dbm <= base0) { S = 0; over9dBm = 0; return; }
            if (dbm <= base9)
            {
                S = (int)Math.Ceiling((dbm - base0) / 6.0);
                if (S < 0) S = 0;
                if (S > 9) S = 9;
                over9dBm = 0;
                return;
            }
            S = 9;
            double d = dbm - base9;
            if (d <= 4) over9dBm = 5;
            else if (d <= 10) over9dBm = 10;
            else if (d <= 14) over9dBm = 15;
            else if (d <= 24) over9dBm = 20;
            else if (d <= 34) over9dBm = 30;
            else if (d <= 44) over9dBm = 40;
            else if (d <= 54) over9dBm = 50;
            else over9dBm = 60;
        }
    }

    internal static class P32StringExtensions
    {
        internal static string Left(this string value, int length)
        {
            if (String.IsNullOrEmpty(value) || length <= 0) return "";
            return value.Substring(0, Math.Min(length, value.Length));
        }
    }

    internal static class BandStackManager
    {
        internal static string BandToString(Band b)
        {
            switch (b)
            {
                case Band.GEN: return "GEN";
                case Band.B160M: return "160M"; case Band.B80M: return "80M"; case Band.B60M: return "60M";
                case Band.B40M: return "40M"; case Band.B30M: return "30M"; case Band.B20M: return "20M";
                case Band.B17M: return "17M"; case Band.B15M: return "15M"; case Band.B12M: return "12M";
                case Band.B10M: return "10M"; case Band.B6M: return "6M"; case Band.B2M: return "2M";
                case Band.WWV: return "WWV"; case Band.BLMF: return "LMF";
                case Band.B120M: return "120M"; case Band.B90M: return "90M"; case Band.B61M: return "61M";
                case Band.B49M: return "49M"; case Band.B41M: return "41M"; case Band.B31M: return "31M";
                case Band.B25M: return "25M"; case Band.B22M: return "22M"; case Band.B19M: return "19M";
                case Band.B16M: return "16M"; case Band.B14M: return "14M"; case Band.B13M: return "13M";
                case Band.B11M: return "11M";
                case Band.VHF0: return "VHF0"; case Band.VHF1: return "VHF1"; case Band.VHF2: return "VHF2";
                case Band.VHF3: return "VHF3"; case Band.VHF4: return "VHF4"; case Band.VHF5: return "VHF5";
                case Band.VHF6: return "VHF6"; case Band.VHF7: return "VHF7"; case Band.VHF8: return "VHF8";
                case Band.VHF9: return "VHF9"; case Band.VHF10: return "VHF10"; case Band.VHF11: return "VHF11";
                case Band.VHF12: return "VHF12"; case Band.VHF13: return "VHF13";
                default: return "GEN";
            }
        }

        internal static Color BandToColour(Band b)
        {
            int n = (int)b;
            if (b == Band.WWV) return Color.Green;
            if (n >= (int)Band.BLMF && n <= (int)Band.B11M) return Color.Coral;
            if (n >= (int)Band.VHF0 && n <= (int)Band.VHF13) return Color.Gold;
            return Color.White;
        }
    }

    internal static class Alex
    {
        internal static Band AntBandFromFreq(double freq)
        {
            if (freq >= 12.075)
            {
                if (freq >= 23.17) return freq >= 26.465 ? (freq >= 39.85 ? Band.B6M : Band.B10M) : Band.B12M;
                return freq >= 16.209 ? (freq >= 19.584 ? Band.B15M : Band.B17M) : Band.B20M;
            }
            if (freq >= 6.20175) return freq >= 8.7 ? Band.B30M : Band.B40M;
            if (freq >= 4.66525) return Band.B60M;
            return freq >= 2.75 ? Band.B80M : Band.B160M;
        }
    }

    sealed unsafe public partial class Console
    {
        internal event Action<FormWindowState> WindowStateChangedHandlers;
        internal event Action<int, Band> BandPreChangeHandlers;
        private bool p32WindowStateHooked;
        private FormWindowState p32LastWindowState;

        internal void P32EnsureWindowStateEvents()
        {
            if (p32WindowStateHooked) return;
            p32WindowStateHooked = true;
            p32LastWindowState = WindowState;
            Resize += delegate
            {
                if (WindowState == p32LastWindowState) return;
                p32LastWindowState = WindowState;
                Action<FormWindowState> handler = WindowStateChangedHandlers;
                if (handler != null) handler(WindowState);
            };
        }

        internal bool VFOASubInUse { get { return VFOASubFreq > -900.0; } }
        internal Band RX1BandForVFOB() { return BandByFreq(VFOBFreq, rx2_xvtr_index, false, current_region); }
        internal bool VFOALock { get { return P32VFOALock; } set { P32VFOALock = value; } }
        internal bool VFOBLock { get { return P32VFOBLock; } set { P32VFOBLock = value; } }
        internal bool IsSetupFormNull { get { return setupForm == null; } }

        internal bool P32LevelerEnabled
        {
            get
            {
                try { return dsp != null && dsp.GetDSPTX(1) != null && dsp.GetDSPTX(1).TXLevelerOn; }
                catch { return false; }
            }
        }

        internal bool CFCEnabled { get { return false; } }
        internal bool GetQuickSplitEnabled { get { return false; } }
        internal bool BandGENSelected
        {
            get { return panelBandGN != null && panelBandGN.Visible; }
            set { if (value) btnBandGEN_Click(radBandGEN, EventArgs.Empty); }
        }
        internal bool BandHFSelected
        {
            get { return panelBandHF != null && panelBandHF.Visible; }
            set { if (value) btnBandHF_Click(btnBandHF, EventArgs.Empty); }
        }
        internal bool BandVHFSelected
        {
            get { return panelBandVHF != null && panelBandVHF.Visible; }
            set { if (value) btnBandVHF_Click(btnBandVHF, EventArgs.Empty); }
        }
        internal bool QSOTimerEnabled { get { return false; } }
        internal int QSOTimerSeconds { get { return 0; } }
        internal string LastNFRX1 { get { return ""; } }
        internal string LastNFRX2 { get { return ""; } }
        internal string PAProfile { get { return TXProfile; } }

        internal Band GetTransverterTranslatedRXBand(double freq)
        {
            return BandByFreq(freq, rx1_xvtr_index, false, current_region);
        }

        internal bool GetVHFEnabled(int index)
        {
            return xvtrForm != null && index >= 0 && index < 14 && xvtrForm.GetEnabled(index);
        }

        internal string GetVHFText(int index)
        {
            if (vhf_text == null || index < 0 || index >= vhf_text.Length || vhf_text[index] == null) return "";
            return vhf_text[index].Text;
        }

        internal void SetupRX2Band(Band b) { SetRX2Band(b); }
        internal void SetupRX2Band(Band b, bool unused) { SetRX2Band(b); }
        internal void PopupFilterContextMenu(int rx, MouseEventArgs e) { P32PopupFilterMenu(rx); }
        internal void PopupBandstack(int rx, Band band, bool onTop) { }

        internal void P32ShowMeterSetup(string id)
        {
            P27ShowMetersConfig();
            if (!String.IsNullOrEmpty(id)) MeterManager.HighlightContainer(id);
        }

        internal void P32SetRXAntenna(int antenna, Band band)
        {
            if (antenna >= 1 && antenna <= 3) SetRX1Ant(band, (FWCAnt)antenna);
        }

        internal void P32SetTXAntenna(int antenna, Band band)
        {
            if (antenna >= 1 && antenna <= 3) SetTXAnt(band, (FWCAnt)antenna);
        }

        internal void P32SetAuxAntenna(int antenna, Band band, bool byp, bool ext1) { }
        internal void ToggleRxTxAnt() { }
        internal void P32ToggleQuickSplit() { }
        internal void P32SetWebImageState(string parentId, ImageFetcher.State state) { }
    }
}
