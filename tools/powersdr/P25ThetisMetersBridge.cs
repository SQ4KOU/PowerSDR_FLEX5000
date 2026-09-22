using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PowerSDR
{
    // Minimal Thetis hardware-model compatibility required by the 2023 meter core.
    public enum HPSDRModel
    {
        HERMES = 0,
        ORIONMKII,
        ANAN8000D,
        ANAN10,
        ANAN10E,
        ANAN_G2
    }

    internal static class P25MeterResources
    {
        private static Image Load(string name)
        {
            try
            {
                string p = Path.Combine(Application.StartupPath, "Resources", name + ".png");
                if (File.Exists(p))
                {
                    using (Image src = Image.FromFile(p))
                        return new Bitmap(src);
                }
            }
            catch { }
            return null;
        }

        public static Image dockIcon_dock { get { return Load("dockIcon_dock"); } }
        public static Image dockIcon_float { get { return Load("dockIcon_float"); } }
        public static Image dot { get { return Load("dot"); } }
        public static Image arrow_left { get { return Load("arrow_left"); } }
        public static Image arrow_topleft { get { return Load("arrow_topleft"); } }
        public static Image arrow_up { get { return Load("arrow_up"); } }
        public static Image arrow_topright { get { return Load("arrow_topright"); } }
        public static Image arrow_right { get { return Load("arrow_right"); } }
        public static Image arrow_bottomright { get { return Load("arrow_bottomright"); } }
        public static Image down { get { return Load("down"); } }
        public static Image arrow_bottomleft { get { return Load("arrow_bottomleft"); } }
        public static Image pin_on_top { get { return Load("pin_on_top"); } }
        public static Image pin_not_on_top { get { return Load("pin_not_on_top"); } }
        public static Image resizegrab { get { return Load("resizegrab"); } }
    }

    public partial class Common
    {
        public static int FiveDigitHash(string str)
        {
            if (String.IsNullOrEmpty(str)) return 0;
            uint hash = 0;
            foreach (byte b in System.Text.Encoding.Unicode.GetBytes(str))
            {
                hash += b; hash += (hash << 10); hash ^= (hash >> 6);
            }
            hash += (hash << 3); hash ^= (hash >> 11); hash += (hash << 15);
            return (int)(hash % 99999);
        }

        public static string ColourToString(Color c)
        {
            return c.A + "." + c.R + "." + c.G + "." + c.B;
        }

        public static Color ColourFromString(string s)
        {
            if (String.IsNullOrEmpty(s)) return Color.Transparent;
            string[] p = s.Split('.');
            if (p.Length != 4) return Color.Transparent;
            int a, r, g, b;
            if (!Int32.TryParse(p[0], out a) || !Int32.TryParse(p[1], out r) ||
                !Int32.TryParse(p[2], out g) || !Int32.TryParse(p[3], out b))
                return Color.Transparent;
            try { return Color.FromArgb(a, r, g, b); }
            catch { return Color.Transparent; }
        }
    }

    sealed unsafe public partial class Console
    {
        private bool p25MetersInitialised;
        private bool p25MetersClosing;
        private ToolStripMenuItem p25MetersMenu;
        private ToolStripMenuItem p25AddRx1Menu;

        internal event Action<int, bool, bool> MoxChangeHandlers;

        internal HPSDRModel CurrentHPSDRModel { get { return HPSDRModel.HERMES; } }
        internal bool AlexPresent { get { return current_model == Model.FLEX5000 || current_model == Model.FLEX3000; } }
        internal bool ApolloPresent { get { return false; } }

        internal void P25RaiseMeterMox(int rx, bool oldMox, bool newMox)
        {
            Action<int, bool, bool> h = MoxChangeHandlers;
            if (h != null) h(rx, oldMox, newMox);
        }

        internal float P25ReadRx1SignalDbm()
        {
            if (!PowerOn) return -200.0f;

            float num = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.SIGNAL_STRENGTH);

            if (fwc_init || hid_init)
            {
                switch (current_model)
                {
                    case Model.SDRX:
                        num += rx1_meter_cal_offset + meter_offset + rx1_filter_size_cal_offset + rx1_xvtr_gain_offset;
                        break;
                    case Model.FLEX5000:
                    case Model.FLEX3000:
                        num += rx1_meter_cal_offset +
                               rx1_preamp_offset[(int)rx1_preamp_mode] +
                               rx1_filter_size_cal_offset +
                               rx1_path_offset +
                               rx1_xvtr_gain_offset +
                               rx1_loop_offset;
                        break;
                    case Model.FLEX1500:
                        num += rx1_meter_cal_offset +
                               rx1_preamp_offset[(int)rx1_preamp_mode] +
                               rx1_filter_size_cal_offset +
                               rx1_xvtr_gain_offset;
                        break;
                }
            }
            else
            {
                num += rx1_meter_cal_offset +
                       rx1_preamp_offset[(int)rx1_preamp_mode] +
                       rx1_filter_size_cal_offset +
                       rx1_xvtr_gain_offset;
            }

            return num;
        }

        internal float P25ReadRx1AverageSignalDbm()
        {
            if (!PowerOn) return -200.0f;

            float num = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.AVG_SIGNAL_STRENGTH);

            if (fwc_init || hid_init)
            {
                switch (current_model)
                {
                    case Model.SDRX:
                        num += rx1_meter_cal_offset + meter_offset + rx1_filter_size_cal_offset + rx1_xvtr_gain_offset;
                        break;
                    case Model.FLEX5000:
                    case Model.FLEX3000:
                        num += rx1_meter_cal_offset +
                               rx1_preamp_offset[(int)rx1_preamp_mode] +
                               rx1_filter_size_cal_offset +
                               rx1_path_offset +
                               rx1_xvtr_gain_offset +
                               rx1_loop_offset;
                        break;
                    case Model.FLEX1500:
                        num += rx1_meter_cal_offset +
                               rx1_preamp_offset[(int)rx1_preamp_mode] +
                               rx1_filter_size_cal_offset +
                               rx1_xvtr_gain_offset;
                        break;
                }
            }
            else
            {
                num += rx1_meter_cal_offset +
                       rx1_preamp_offset[(int)rx1_preamp_mode] +
                       rx1_filter_size_cal_offset +
                       rx1_xvtr_gain_offset;
            }

            return num;
        }

        internal float P25ReadRx1AgcGain()
        {
            if (!PowerOn) return -200.0f;
            return DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.AGC_GAIN);
        }

        private void P25InitThetisMeters()
        {
            if (p25MetersInitialised) return;
            p25MetersInitialised = true;

            MeterManager.Init(this, Path.Combine(Application.StartupPath, "MeterSkins"));
            P32EnsureWindowStateEvents();

            ArrayList stored = DB.GetVars("SQ4KOU_ThetisMeters");
            if (stored != null && stored.Count > 0)
            {
                var settings = new System.Collections.Generic.Dictionary<string, string>();
                foreach (object o in stored)
                {
                    string s = o as string;
                    if (String.IsNullOrEmpty(s)) continue;
                    int slash = s.IndexOf('/');
                    if (slash <= 0 || slash >= s.Length - 1) continue;
                    settings[s.Substring(0, slash)] = s.Substring(slash + 1);
                }
                if (settings.Count > 0) MeterManager.RestoreSettings(ref settings);
            }

            if (MeterManager.TotalMeterContainers == 0)
                P25AddRx1SignalMeter();

            p25MetersMenu = new ToolStripMenuItem("Meters/Gadgets");
            p25AddRx1Menu = new ToolStripMenuItem("Add RX1 Signal Meter");
            p25AddRx1Menu.Click += delegate { P25AddRx1SignalMeter(); };
            p25MetersMenu.DropDownItems.Add(p25AddRx1Menu);
            menuStrip1.Items.Add(p25MetersMenu);
        }

        private string P25AddRx1SignalMeter()
        {
            string id = MeterManager.AddMeterContainer(1, true);
            MeterManager.clsMeter m = MeterManager.MeterFromId(id);
            if (m != null)
            {
                m.Name = "RX1 Signal";
                m.AddMeter(MeterType.SIGNAL_STRENGTH);
                m.Rebuild();
            }
            return id;
        }

        private void P25SaveThetisMeters()
        {
            if (!p25MetersInitialised) return;
            var settings = new System.Collections.Generic.Dictionary<string, string>();
            MeterManager.StoreSettings2(ref settings);
            ArrayList a = new ArrayList();
            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in settings)
                a.Add(kvp.Key + "/" + kvp.Value);
            DB.SaveVars("SQ4KOU_ThetisMeters", ref a);
        }

        private void P25ShutdownThetisMeters()
        {
            if (!p25MetersInitialised || p25MetersClosing) return;
            p25MetersClosing = true;
            try { P25SaveThetisMeters(); } catch { }
            try { MeterManager.Shutdown(); } catch { }
        }

        private void P25ThetisMetersShown(object sender, EventArgs e)
        {
            try { P25InitThetisMeters(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine("P25 meter init: " + ex); }
        }

        private void P25ThetisMetersClosing(object sender, FormClosingEventArgs e)
        {
            P25ShutdownThetisMeters();
        }
    }
}
