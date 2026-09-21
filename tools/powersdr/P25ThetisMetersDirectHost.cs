// Direct host boundary: native Thetis 2023/2024 meter subsystem on PowerSDR FLEX-5000.
// No replacement renderer/store/runtime. Only PowerSDR telemetry + native DB lifecycle live here.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PowerSDR
{
    public enum HPSDRModel
    {
        UNKNOWN = 0,
        HERMES,
        ORIONMKII,
        ANAN8000D,
        ANAN_G2,
        ANAN10,
        ANAN10E
    }

    public partial class Common
    {
        public static bool ShiftKeyDown { get { return (Control.ModifierKeys & Keys.Shift) == Keys.Shift; } }
        public static bool CtrlKeyDown { get { return (Control.ModifierKeys & Keys.Control) == Keys.Control; } }

        public static void DoubleBuffered(Control control, bool enabled)
        {
            if (control == null) return;
            var p = control.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (p != null) p.SetValue(control, enabled, null);
        }

        public static void DoubleBufferAll(Control control, bool enabled)
        {
            if (control == null) return;
            DoubleBuffered(control, enabled);
            foreach (Control child in control.Controls) DoubleBufferAll(child, enabled);
        }

        public static string ColourToString(Color c)
        {
            return c.A.ToString() + "." + c.R.ToString() + "." + c.G.ToString() + "." + c.B.ToString();
        }

        public static Color ColourFromString(string s)
        {
            if (String.IsNullOrEmpty(s)) return Color.Empty;
            string[] p = s.Split('.');
            if (p.Length != 4) return Color.Empty;
            int a, r, g, b;
            if (!Int32.TryParse(p[0], out a)) return Color.Empty;
            if (!Int32.TryParse(p[1], out r)) return Color.Empty;
            if (!Int32.TryParse(p[2], out g)) return Color.Empty;
            if (!Int32.TryParse(p[3], out b)) return Color.Empty;
            return Color.FromArgb(a, r, g, b);
        }

        public static MessageBoxOptions MB_TOPMOST = (MessageBoxOptions)0x00040000L;

        public static int FiveDigitHash(string str)
        {
            if (String.IsNullOrEmpty(str)) return 0;
            uint hash = 0;
            foreach (byte b in System.Text.Encoding.Unicode.GetBytes(str))
            {
                hash += b;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return (int)(hash % 99999);
        }

        public static double UVfromDBM(double dbm)
        {
            return Math.Sqrt(Math.Pow(10, dbm / 10.0) * 50.0 * 1e-3) * 1e6;
        }

        public static void SMeterFromDBM2(double dbm, bool above30, out int s, out int over9)
        {
            double floor = above30 ? -144.0 : -124.0;
            double s9 = above30 ? -90.0 : -70.0;
            if (dbm <= floor) { s = 0; over9 = 0; return; }
            if (dbm <= s9)
            {
                s = (int)Math.Ceiling((dbm - floor) / 6.0);
                if (s < 1) s = 1;
                if (s > 9) s = 9;
                over9 = 0;
                return;
            }
            s = 9;
            double d = dbm - s9;
            if (d <= 4) over9 = 5;
            else if (d <= 10) over9 = 10;
            else if (d <= 14) over9 = 15;
            else if (d <= 24) over9 = 20;
            else if (d <= 34) over9 = 30;
            else if (d <= 44) over9 = 40;
            else if (d <= 54) over9 = 50;
            else over9 = 60;
        }

        public static string SMeterFromDBM(double dbm, bool above30)
        {
            int s, over;
            SMeterFromDBM2(dbm, above30, out s, out over);
            string v = over > 0 ? "S 9 + " + over.ToString() : "S " + s.ToString();
            return "    " + v;
        }

        public static void FadeIn(Form form, int msTimeToFade)
        {
            if (form != null) form.Opacity = 1.0;
        }
    }

    public static class BandStackManager
    {
        public static string BandToString(Band b)
        {
            string s = b.ToString();
            if (s.StartsWith("B", StringComparison.Ordinal) && s.Length > 1)
                return s.Substring(1);
            return s;
        }
    }

    sealed unsafe public partial class Console
    {
        private bool p25_meters_started = false;

        public bool VFOASubInUse { get { return false; } }
        public bool IsSetupFormNull { get { return setupForm == null; } }
        public Setup SetupForm { get { return setupForm; } }

        internal Dictionary<Reading, float> P25GetMeterReadings()
        {
            Dictionary<Reading, float> r = new Dictionary<Reading, float>();
            for (Reading x = Reading.SIGNAL_STRENGTH; x < Reading.LAST; x++)
                r[x] = 0.0f;

            if (!PowerOn) return r;

            if (!MOX)
            {
                float sig = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.SIGNAL_STRENGTH);

                // Exact native KE9NS FLEX-5000 RX1 calibration path.
                if (current_model == Model.FLEX5000)
                {
                    sig = sig +
                        rx1_meter_cal_offset +
                        rx1_preamp_offset[(int)rx1_preamp_mode] +
                        rx1_filter_size_cal_offset +
                        rx1_path_offset +
                        rx1_xvtr_gain_offset +
                        rx1_loop_offset;
                }

                r[Reading.SIGNAL_STRENGTH] = sig;
                r[Reading.AVG_SIGNAL_STRENGTH] = sig;
                r[Reading.ADC_PK] = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.ADC_REAL);
                r[Reading.ADC_AV] = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.ADC_IMAG);
                r[Reading.AGC_GAIN] = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.AGC_GAIN);
            }
            else
            {
                r[Reading.MIC] = DttSP.CalculateTXMeter(0, DttSP.MeterType.MIC);
                r[Reading.MIC_PK] = DttSP.CalculateTXMeter(0, DttSP.MeterType.MIC_PK);
                r[Reading.ALC] = DttSP.CalculateTXMeter(0, DttSP.MeterType.ALC);
                r[Reading.ALC_PK] = DttSP.CalculateTXMeter(0, DttSP.MeterType.ALC_PK);
                r[Reading.ALC_G] = DttSP.CalculateTXMeter(0, DttSP.MeterType.ALC_G);
                r[Reading.EQ] = DttSP.CalculateTXMeter(0, DttSP.MeterType.EQ);
                r[Reading.EQ_PK] = DttSP.CalculateTXMeter(0, DttSP.MeterType.EQ_PK);
                r[Reading.LEVELER] = DttSP.CalculateTXMeter(0, DttSP.MeterType.LEVELER);
                r[Reading.LEVELER_PK] = DttSP.CalculateTXMeter(0, DttSP.MeterType.LEVELER_PK);
                r[Reading.LVL_G] = DttSP.CalculateTXMeter(0, DttSP.MeterType.LVL_G);
                r[Reading.COMP] = DttSP.CalculateTXMeter(0, DttSP.MeterType.COMP);
                r[Reading.COMP_PK] = DttSP.CalculateTXMeter(0, DttSP.MeterType.COMP_PK);

                // Power/SWR values come from the native FLEX-5000/FWC meter path already maintained by PowerSDR.
                r[Reading.PWR] = (float)lastfwdpower;
                r[Reading.REVERSE_PWR] = (float)lastrevpower;
                r[Reading.SWR] = (float)lastswr;
            }

            return r;
        }

        private Dictionary<string, string> P25LoadMeterState()
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            ArrayList a = DB.GetVars("ThetisMeters_P25");
            foreach (object o in a)
            {
                string s = o as string;
                if (String.IsNullOrEmpty(s)) continue;
                int slash = s.IndexOf('/');
                if (slash <= 0) continue;
                string key = s.Substring(0, slash);
                string val = s.Substring(slash + 1);
                d[key] = val;
            }
            return d;
        }

        private void P25SaveMeterState()
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            MeterManager.StoreSettings2(ref d);
            ArrayList a = new ArrayList();
            foreach (KeyValuePair<string, string> kv in d)
                a.Add(kv.Key + "/" + kv.Value);
            DB.SaveVars("ThetisMeters_P25", ref a);
        }

        private void P25ThetisMetersShown(object sender, EventArgs e)
        {
            if (p25_meters_started) return;
            p25_meters_started = true;

            MeterManager.Init(this);

            Dictionary<string, string> d = P25LoadMeterState();
            bool restored = d.Count > 0 && MeterManager.RestoreSettings(ref d);

            if (!restored || MeterManager.TotalMeterContainers == 0)
            {
                string id = MeterManager.AddMeterContainer(1, true);
                MeterManager.clsMeter m = MeterManager.MeterFromId(id);
                if (m != null)
                {
                    m.AddMeter(MeterType.SIGNAL_STRENGTH);
                    m.ZeroOut(true, true);
                    m.Rebuild();
                }
                MeterManager.RunRendererDisplay(id);
            }
            else
            {
                MeterManager.RunAllRendererDisplays();
            }

            MeterManager.FinishSetupAndDisplay();
        }

        private void P25ThetisMetersConsoleClosing(object sender, FormClosingEventArgs e)
        {
            if (!p25_meters_started) return;
            try { P25SaveMeterState(); } catch { }
            try { MeterManager.Shutdown(); } catch { }
            p25_meters_started = false;
        }
    }
}
