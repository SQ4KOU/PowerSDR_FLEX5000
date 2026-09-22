// SQ4KOU P23 - Thetis Meters/Gadgets compatibility core for PowerSDR FLEX-5000
// The UI/container semantics are intentionally modelled on Thetis Meters/Gadgets.
// FLEX-5000 remains native PowerSDR PAL/FWC/FireWire/ASIO/DttSP.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PowerSDR
{
    internal enum P23MeterItemType
    {
        SIGNAL_STRENGTH,
        AVG_SIGNAL_STRENGTH,
        SIGNAL_TEXT,
        ADC,
        AGC,
        AGC_GAIN,
        MIC,
        EQ,
        LEVELER,
        LEVELER_GAIN,
        ALC,
        ALC_GAIN,
        COMP,
        PWR,
        REVERSE_PWR,
        SWR,
        MAGIC_EYE,
        ANANMM,
        CROSS,
        VFO_DISPLAY,
        CLOCK,
        SPACER,
        TEXT_OVERLAY,
        DATA_OUT,
        ROTATOR,
        LED,
        WEB_IMAGE,
        BAND_BUTTONS,
        MODE_BUTTONS,
        FILTER_BUTTONS,
        ANTENNA_BUTTONS,
        HISTORY,
        TUNESTEP_BUTTONS,
        FILTER_DISPLAY,
        DIAL_DISPLAY,
        CUSTOM_METER_BAR,
        OTHER_BUTTONS,
        WAVE_RECORD,
        VOICE_RECORD_PLAY_BUTTONS
    }

    internal static class P23MeterItemNames
    {
        internal static readonly P23MeterItemType[] Available = new P23MeterItemType[]
        {
            P23MeterItemType.SIGNAL_STRENGTH,
            P23MeterItemType.AVG_SIGNAL_STRENGTH,
            P23MeterItemType.SIGNAL_TEXT,
            P23MeterItemType.ADC,
            P23MeterItemType.AGC,
            P23MeterItemType.AGC_GAIN,
            P23MeterItemType.MIC,
            P23MeterItemType.EQ,
            P23MeterItemType.LEVELER,
            P23MeterItemType.LEVELER_GAIN,
            P23MeterItemType.ALC,
            P23MeterItemType.ALC_GAIN,
            P23MeterItemType.COMP,
            P23MeterItemType.PWR,
            P23MeterItemType.REVERSE_PWR,
            P23MeterItemType.SWR,
            P23MeterItemType.MAGIC_EYE,
            P23MeterItemType.ANANMM,
            P23MeterItemType.CROSS,
            P23MeterItemType.VFO_DISPLAY,
            P23MeterItemType.CLOCK,
            P23MeterItemType.SPACER,
            P23MeterItemType.TEXT_OVERLAY,
            P23MeterItemType.DATA_OUT,
            P23MeterItemType.ROTATOR,
            P23MeterItemType.LED,
            P23MeterItemType.WEB_IMAGE,
            P23MeterItemType.BAND_BUTTONS,
            P23MeterItemType.MODE_BUTTONS,
            P23MeterItemType.FILTER_BUTTONS,
            P23MeterItemType.ANTENNA_BUTTONS,
            P23MeterItemType.HISTORY,
            P23MeterItemType.TUNESTEP_BUTTONS,
            P23MeterItemType.FILTER_DISPLAY,
            P23MeterItemType.DIAL_DISPLAY,
            P23MeterItemType.CUSTOM_METER_BAR,
            P23MeterItemType.OTHER_BUTTONS,
            P23MeterItemType.WAVE_RECORD,
            P23MeterItemType.VOICE_RECORD_PLAY_BUTTONS
        };

        internal static string Display(P23MeterItemType t)
        {
            switch (t)
            {
                case P23MeterItemType.SIGNAL_STRENGTH: return "Signal Strength";
                case P23MeterItemType.AVG_SIGNAL_STRENGTH: return "Signal Average";
                case P23MeterItemType.SIGNAL_TEXT: return "Signal Text";
                case P23MeterItemType.ADC: return "ADC";
                case P23MeterItemType.AGC: return "AGC";
                case P23MeterItemType.AGC_GAIN: return "AGC Gain";
                case P23MeterItemType.MIC: return "Mic";
                case P23MeterItemType.EQ: return "EQ";
                case P23MeterItemType.LEVELER: return "Leveler";
                case P23MeterItemType.LEVELER_GAIN: return "Leveler Gain";
                case P23MeterItemType.ALC: return "ALC";
                case P23MeterItemType.ALC_GAIN: return "ALC Gain";
                case P23MeterItemType.COMP: return "Compression";
                case P23MeterItemType.PWR: return "Forward Power";
                case P23MeterItemType.REVERSE_PWR: return "Reverse Power";
                case P23MeterItemType.SWR: return "SWR";
                case P23MeterItemType.MAGIC_EYE: return "Magic Eye";
                case P23MeterItemType.ANANMM: return "ANAN Multi Meter";
                case P23MeterItemType.CROSS: return "Cross Meter";
                case P23MeterItemType.VFO_DISPLAY: return "Vfo Display";
                case P23MeterItemType.CLOCK: return "Clock";
                case P23MeterItemType.SPACER: return "Spacer";
                case P23MeterItemType.TEXT_OVERLAY: return "Text Overlay";
                case P23MeterItemType.DATA_OUT: return "Data Out Node";
                case P23MeterItemType.ROTATOR: return "Rotator";
                case P23MeterItemType.LED: return "LED Indicator";
                case P23MeterItemType.WEB_IMAGE: return "Web Image";
                case P23MeterItemType.BAND_BUTTONS: return "Band Buttons";
                case P23MeterItemType.MODE_BUTTONS: return "Mode Buttons";
                case P23MeterItemType.FILTER_BUTTONS: return "Filter Buttons";
                case P23MeterItemType.ANTENNA_BUTTONS: return "Antenna Buttons";
                case P23MeterItemType.HISTORY: return "History Graph";
                case P23MeterItemType.TUNESTEP_BUTTONS: return "Tunestep Buttons";
                case P23MeterItemType.FILTER_DISPLAY: return "Filter Display";
                case P23MeterItemType.DIAL_DISPLAY: return "Dial Display";
                case P23MeterItemType.CUSTOM_METER_BAR: return "Custom Meter Bar";
                case P23MeterItemType.OTHER_BUTTONS: return "Other Buttons";
                case P23MeterItemType.WAVE_RECORD: return "WaveList Player";
                case P23MeterItemType.VOICE_RECORD_PLAY_BUTTONS: return "Voice Record/Play";
                default: return t.ToString();
            }
        }
    }

    internal sealed class P23MeterItemConfig
    {
        public string Id { get; set; }
        public P23MeterItemType Type { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public int Height { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
        public string Condition { get; set; }
        public string Url { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public int ForeColorArgb { get; set; }
        public int BackColorArgb { get; set; }
        public bool ShowScale { get; set; }
        public bool ShowValue { get; set; }
        public bool CacheBypass { get; set; }
        public string Notes { get; set; }

        public P23MeterItemConfig()
        {
            Id = Guid.NewGuid().ToString("N");
            Type = P23MeterItemType.SIGNAL_STRENGTH;
            Name = "Signal Strength";
            Enabled = true;
            Height = 58;
            Source = "";
            Text = "";
            Condition = "";
            Url = "";
            Minimum = -140.0;
            Maximum = -20.0;
            ForeColorArgb = Color.White.ToArgb();
            BackColorArgb = Color.Black.ToArgb();
            ShowScale = true;
            ShowValue = true;
            CacheBypass = false;
            Notes = "";
        }

        public P23MeterItemConfig Clone()
        {
            return JsonConvert.DeserializeObject<P23MeterItemConfig>(
                JsonConvert.SerializeObject(this)) ?? new P23MeterItemConfig();
        }

        public override string ToString()
        {
            return String.IsNullOrEmpty(Name) ? P23MeterItemNames.Display(Type) : Name;
        }
    }

    internal sealed class P23ContainerConfig
    {
        public string Id { get; set; }
        public int Sequence { get; set; }
        public int RX { get; set; }
        public int DockedX { get; set; }
        public int DockedY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FloatX { get; set; }
        public int FloatY { get; set; }
        public bool Floating { get; set; }
        public bool PinOnTop { get; set; }
        public bool Border { get; set; }
        public bool NoControls { get; set; }
        public bool Enabled { get; set; }
        public bool ShowOnRX { get; set; }
        public bool ShowOnTX { get; set; }
        public bool Locked { get; set; }
        public bool ContainerMinimises { get; set; }
        public bool HideWhenRxNotUsed { get; set; }
        public bool AutoHeight { get; set; }
        public int BackColorArgb { get; set; }
        public string Notes { get; set; }
        public string BackgroundImagePath { get; set; }
        public List<P23MeterItemConfig> Items { get; set; }

        public P23ContainerConfig()
        {
            Id = Guid.NewGuid().ToString("N");
            Sequence = 0;
            RX = 1;
            DockedX = 10;
            DockedY = 10;
            Width = 400;
            Height = 200;
            FloatX = 100;
            FloatY = 100;
            Floating = true;
            PinOnTop = false;
            Border = true;
            NoControls = false;
            Enabled = true;
            ShowOnRX = true;
            ShowOnTX = true;
            Locked = false;
            ContainerMinimises = true;
            HideWhenRxNotUsed = false;
            AutoHeight = false;
            BackColorArgb = Color.Black.ToArgb();
            Notes = "";
            BackgroundImagePath = "";
            Items = new List<P23MeterItemConfig>();
        }

        public P23ContainerConfig CloneNewIdentity()
        {
            P23ContainerConfig c = JsonConvert.DeserializeObject<P23ContainerConfig>(
                JsonConvert.SerializeObject(this)) ?? new P23ContainerConfig();
            c.Id = Guid.NewGuid().ToString("N");
            c.Sequence = 0;
            c.DockedX += 20;
            c.DockedY += 20;
            c.FloatX += 20;
            c.FloatY += 20;
            if (c.Items == null) c.Items = new List<P23MeterItemConfig>();
            for (int i = 0; i < c.Items.Count; i++)
                c.Items[i].Id = Guid.NewGuid().ToString("N");
            return c;
        }

        public override string ToString()
        {
            string n = String.IsNullOrWhiteSpace(Notes) ? "" : " - " + FirstLine(Notes);
            return "RX" + RX.ToString(CultureInfo.InvariantCulture) + " [" +
                Id.Substring(0, Math.Min(5, Id.Length)).ToUpperInvariant() + "]" + n;
        }

        private static string FirstLine(string s)
        {
            if (String.IsNullOrEmpty(s)) return "";
            string[] p = s.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return p.Length == 0 ? s : p[0];
        }
    }

    internal sealed class P23RadioAdapter
    {
        private readonly PowerSDR.Console _console;
        private readonly Flex5000MeterAdapter _base;
        private readonly Type _type;
        private readonly BindingFlags _flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal P23RadioAdapter(PowerSDR.Console console)
        {
            _console = console;
            _base = new Flex5000MeterAdapter(console);
            _type = console.GetType();
        }

        internal PowerSDR.Console Console { get { return _console; } }
        internal bool PowerOn { get { return _base.PowerOn; } }
        internal bool IsTx { get { return _base.IsTx; } }
        internal double VfoA { get { return _base.VfoA; } }
        internal double VfoB { get { return _base.VfoB; } }
        internal string Band { get { return _base.Band; } }
        internal string Mode { get { return _base.Mode; } }
        internal string Filter { get { return _base.Filter; } }
        internal double Signal { get { return _base.SignalDbm; } }
        internal string SUnit(double dbm) { return _base.SUnit(dbm); }
        internal double ForwardPower { get { return _base.ForwardPower; } }
        internal double SWR { get { return _base.Swr; } }
        internal double Mic { get { return _base.MicDb; } }
        internal double Alc { get { return _base.AlcDb; } }

        internal double AverageSignal
        {
            get
            {
                try { return DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.AVG_SIGNAL_STRENGTH); }
                catch { return Signal; }
            }
        }

        internal double AgcGain
        {
            get
            {
                try { return DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.AGC_GAIN); }
                catch { return 0.0; }
            }
        }

        internal double AdcReal
        {
            get
            {
                try { return DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.ADC_REAL); }
                catch { return 0.0; }
            }
        }

        internal double AdcImag
        {
            get
            {
                try { return DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.ADC_IMAG); }
                catch { return 0.0; }
            }
        }

        internal double ReversePower
        {
            get
            {
                try
                {
                    object rev = ReadMember("pa_rev_power");
                    if (rev == null) return 0.0;
                    MethodInfo m = FindMethod("FWCPAPower", 1);
                    if (m == null) return 0.0;
                    ParameterInfo p = m.GetParameters()[0];
                    object a = ConvertForType(rev, p.ParameterType);
                    double watts = Convert.ToDouble(m.Invoke(_console, new object[] { a }), CultureInfo.InvariantCulture);
                    object table = ReadMember("atu_swr_table");
                    if (table is Array)
                    {
                        object txBand = ReadMember("TXBand");
                        int idx = txBand == null ? -1 : Convert.ToInt32(txBand, CultureInfo.InvariantCulture);
                        Array ar = (Array)table;
                        if (idx >= 0 && idx < ar.Length)
                            watts *= Convert.ToDouble(ar.GetValue(idx), CultureInfo.InvariantCulture);
                    }
                    if (Double.IsNaN(watts) || Double.IsInfinity(watts) || watts < 0.0) return 0.0;
                    return watts;
                }
                catch { return 0.0; }
            }
        }

        internal double TxMeter(P23MeterItemType type)
        {
            try
            {
                DttSP.MeterType mt = DttSP.MeterType.MIC;
                switch (type)
                {
                    case P23MeterItemType.MIC: mt = DttSP.MeterType.MIC; break;
                    case P23MeterItemType.EQ: mt = DttSP.MeterType.EQ; break;
                    case P23MeterItemType.LEVELER: mt = DttSP.MeterType.LEVELER; break;
                    case P23MeterItemType.LEVELER_GAIN: mt = DttSP.MeterType.LVL_G; break;
                    case P23MeterItemType.ALC: mt = DttSP.MeterType.ALC; break;
                    case P23MeterItemType.ALC_GAIN: mt = DttSP.MeterType.ALC_G; break;
                    case P23MeterItemType.COMP: mt = DttSP.MeterType.COMP; break;
                    case P23MeterItemType.PWR: mt = DttSP.MeterType.PWR; break;
                    default: mt = DttSP.MeterType.MIC; break;
                }
                return DttSP.CalculateTXMeter(0, mt);
            }
            catch { return -200.0; }
        }

        internal Dictionary<string, object> Snapshot()
        {
            Dictionary<string, object> d = _base.Snapshot();
            d["AVG_SIGNAL_STRENGTH"] = AverageSignal;
            d["AGC_GAIN"] = AgcGain;
            d["ADC_REAL"] = AdcReal;
            d["ADC_IMAG"] = AdcImag;
            d["REVERSE_PWR"] = ReversePower;
            d["EQ"] = TxMeter(P23MeterItemType.EQ);
            d["LEVELER"] = TxMeter(P23MeterItemType.LEVELER);
            d["LEVELER_GAIN"] = TxMeter(P23MeterItemType.LEVELER_GAIN);
            d["COMP"] = TxMeter(P23MeterItemType.COMP);
            d["ALC_GAIN"] = TxMeter(P23MeterItemType.ALC_GAIN);
            d["TIME_UTC"] = DateTime.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            d["DATE_UTC"] = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            d["TIME_LOC"] = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            d["DATE_LOC"] = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            d["TUNESTEP"] = ReadText("TuneStep", "CurrentTuneStep", "TuneStepIndex");
            d["ANTENNA"] = ReadText("RX1Ant", "RXAnt", "Antenna");
            d["RX2_ENABLED"] = ReadBool(false, "RX2Enabled");
            return d;
        }

        internal bool SetVfoA(double mhz) { return _base.SetVfoA(mhz); }
        internal bool SetVfoB(double mhz) { return _base.SetVfoB(mhz); }

        internal bool WriteNamed(string member, object value)
        {
            try
            {
                PropertyInfo p = _type.GetProperty(member, _flags);
                if (p != null && p.CanWrite)
                {
                    p.SetValue(_console, ConvertForType(value, p.PropertyType), null);
                    return true;
                }
                FieldInfo f = _type.GetField(member, _flags);
                if (f != null && !f.IsInitOnly)
                {
                    f.SetValue(_console, ConvertForType(value, f.FieldType));
                    return true;
                }
            }
            catch { }
            return false;
        }

        internal bool CycleMember(string member, int delta)
        {
            try
            {
                PropertyInfo p = _type.GetProperty(member, _flags);
                if (p == null || !p.CanRead || !p.CanWrite || !p.PropertyType.IsEnum) return false;
                Array vals = Enum.GetValues(p.PropertyType);
                object cur = p.GetValue(_console, null);
                int idx = Array.IndexOf(vals, cur);
                if (idx < 0) idx = 0;
                idx = (idx + (delta >= 0 ? 1 : -1) + vals.Length) % vals.Length;
                p.SetValue(_console, vals.GetValue(idx), null);
                return true;
            }
            catch { return false; }
        }

        internal string ReadText(params string[] names)
        {
            object o = ReadMember(names);
            return o == null ? "--" : Convert.ToString(o, CultureInfo.InvariantCulture);
        }

        internal object ReadMember(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                try
                {
                    PropertyInfo p = _type.GetProperty(names[i], _flags);
                    if (p != null && p.CanRead) return p.GetValue(_console, null);
                    FieldInfo f = _type.GetField(names[i], _flags);
                    if (f != null) return f.GetValue(_console);
                }
                catch { }
            }
            return null;
        }

        private bool ReadBool(bool fallback, params string[] names)
        {
            object o = ReadMember(names);
            if (o == null) return fallback;
            try { return Convert.ToBoolean(o, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private MethodInfo FindMethod(string name, int parameterCount)
        {
            MethodInfo[] methods = _type.GetMethods(_flags);
            for (int i = 0; i < methods.Length; i++)
                if (String.Equals(methods[i].Name, name, StringComparison.Ordinal) &&
                    methods[i].GetParameters().Length == parameterCount)
                    return methods[i];
            return null;
        }

        private static object ConvertForType(object value, Type t)
        {
            if (value == null) return null;
            if (t.IsAssignableFrom(value.GetType())) return value;
            if (t.IsEnum)
            {
                if (value is string) return Enum.Parse(t, (string)value, true);
                return Enum.ToObject(t, Convert.ToInt32(value, CultureInfo.InvariantCulture));
            }
            return Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
        }
    }

    internal static class P23MultiMeterIO
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, object> Vars =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private static UdpClient _udp;
        private static Thread _udpThread;
        private static SerialPort _serial;

        internal static event EventHandler VariablesChanged;

        internal static Dictionary<string, object> Snapshot()
        {
            lock (Sync) return new Dictionary<string, object>(Vars, StringComparer.OrdinalIgnoreCase);
        }

        internal static object Get(string key)
        {
            lock (Sync)
            {
                object v;
                return Vars.TryGetValue(key ?? "", out v) ? v : null;
            }
        }

        internal static void Set(string key, object value)
        {
            if (String.IsNullOrWhiteSpace(key)) return;
            lock (Sync) Vars[key.Trim()] = value;
            EventHandler h = VariablesChanged;
            if (h != null) h(null, EventArgs.Empty);
        }

        internal static void StartUdp(int port)
        {
            StopUdp();
            _udp = new UdpClient(port);
            _udpThread = new Thread(delegate()
            {
                while (_udp != null)
                {
                    try
                    {
                        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                        byte[] b = _udp.Receive(ref ep);
                        ParsePayload(Encoding.UTF8.GetString(b));
                    }
                    catch { if (_udp == null) break; }
                }
            });
            _udpThread.IsBackground = true;
            _udpThread.Name = "P23 MultiMeter UDP";
            _udpThread.Start();
        }

        internal static void StopUdp()
        {
            UdpClient u = _udp;
            _udp = null;
            if (u != null) try { u.Close(); } catch { }
            _udpThread = null;
        }

        internal static void StartSerial(string portName, int baud)
        {
            StopSerial();
            SerialPort sp = new SerialPort(portName, baud);
            sp.NewLine = "\n";
            sp.DataReceived += delegate
            {
                try { ParsePayload(sp.ReadExisting()); } catch { }
            };
            sp.Open();
            _serial = sp;
        }

        internal static void StopSerial()
        {
            SerialPort s = _serial;
            _serial = null;
            if (s != null)
            {
                try { s.Close(); } catch { }
                try { s.Dispose(); } catch { }
            }
        }

        internal static void StopAll()
        {
            StopUdp();
            StopSerial();
        }

        internal static void ParsePayload(string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return;
            string trimmed = text.Trim();
            try
            {
                if (trimmed.StartsWith("{", StringComparison.Ordinal))
                {
                    JObject o = JObject.Parse(trimmed);
                    foreach (JProperty p in o.Properties())
                    {
                        JValue jv = p.Value as JValue;
                        Set(p.Name, jv == null ? p.Value.ToString(Formatting.None) : jv.Value);
                    }
                    return;
                }
            }
            catch { }

            string[] lines = trimmed.Replace("\r", "").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int eq = lines[i].IndexOf('=');
                if (eq <= 0) eq = lines[i].IndexOf(':');
                if (eq <= 0) continue;
                string k = lines[i].Substring(0, eq).Trim();
                string v = lines[i].Substring(eq + 1).Trim();
                double d;
                bool b;
                if (Double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) Set(k, d);
                else if (Boolean.TryParse(v, out b)) Set(k, b);
                else Set(k, v);
            }
        }
    }

    internal static class P23MeterManager
    {
        private static readonly object Sync = new object();
        private static readonly List<P23MeterContainer> Containers = new List<P23MeterContainer>();
        private static PowerSDR.Console _console;
        private static Setup _setup;
        private static P23RadioAdapter _adapter;
        private static System.Windows.Forms.Timer _timer;
        private static string _root;
        private static string _statePath;
        private static int _sequence;
        private static bool _restoring;

        internal static event EventHandler ContainersChanged;
        internal static event Action<string> SelectContainerRequested;

        internal static PowerSDR.Console Console { get { return _console; } }
        internal static Setup SetupForm { get { return _setup; } }
        internal static P23RadioAdapter Adapter { get { return _adapter; } }
        internal static string Root { get { return _root; } }
        internal static int TotalMeterContainers { get { lock (Sync) return Containers.Count; } }

        internal static void Initialize(PowerSDR.Console console, Setup setup)
        {
            if (_console != null) return;
            _console = console;
            _setup = setup;
            _adapter = new P23RadioAdapter(console);
            _root = Path.Combine(MetersGadgetsForm.GetConfigRoot(console), "P23-Thetis-1to1");
            Directory.CreateDirectory(_root);
            _statePath = Path.Combine(_root, "containers.json");
            _sequence = 0;

            RestoreAll();

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 75;
            _timer.Tick += delegate { Tick(); };
            _timer.Start();

            console.FormClosing += delegate { Shutdown(); };
        }

        internal static P23MeterContainer[] GetContainers()
        {
            lock (Sync) return Containers.ToArray();
        }

        internal static P23MeterContainer Find(string id)
        {
            if (String.IsNullOrEmpty(id)) return null;
            lock (Sync)
            {
                for (int i = 0; i < Containers.Count; i++)
                    if (String.Equals(Containers[i].Config.Id, id, StringComparison.OrdinalIgnoreCase))
                        return Containers[i];
            }
            return null;
        }

        internal static P23MeterContainer AddContainer()
        {
            P23ContainerConfig cfg = NewDefaultConfig();
            return AddContainer(cfg, true);
        }

        internal static P23MeterContainer AddContainer(P23ContainerConfig cfg, bool save)
        {
            if (_console == null || cfg == null) return null;
            cfg.Sequence = ++_sequence;
            P23MeterContainer c = new P23MeterContainer(cfg, _adapter);
            c.SettingsClicked += delegate { RequestSelect(c.Config.Id); };
            c.ConfigurationChanged += delegate { if (!_restoring) SaveAll(); };

            lock (Sync) Containers.Add(c);
            c.AttachAccordingToConfig();

            RaiseContainersChanged();
            if (save && !_restoring) SaveAll();
            return c;
        }

        internal static P23MeterContainer Duplicate(string id)
        {
            P23MeterContainer c = Find(id);
            if (c == null) return null;
            return AddContainer(c.Config.CloneNewIdentity(), true);
        }

        internal static void Remove(string id)
        {
            P23MeterContainer c = Find(id);
            if (c == null || c.Config.Locked) return;
            lock (Sync) Containers.Remove(c);
            c.DestroyContainer();
            RaiseContainersChanged();
            SaveAll();
        }

        internal static void Recover(string id)
        {
            P23MeterContainer c = Find(id);
            if (c == null || c.Config.Locked) return;
            c.RecoverToConsole();
            SaveAll();
        }

        internal static void RemoveAll()
        {
            P23MeterContainer[] a = GetContainers();
            for (int i = 0; i < a.Length; i++) a[i].DestroyContainer();
            lock (Sync) Containers.Clear();
            RaiseContainersChanged();
            SaveAll();
        }

        internal static void SaveContainer(string id, string path)
        {
            P23MeterContainer c = Find(id);
            if (c == null) return;
            P23ContainerFile file = new P23ContainerFile();
            file.Signature = "SQ4KOU-P23-THETIS-METER-CONTAINER";
            file.Version = 1;
            file.Container = c.Config;
            string json = JsonConvert.SerializeObject(file, Formatting.Indented);
            WriteAtomic(path, json);
        }

        internal static P23MeterContainer LoadContainer(string path)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            P23ContainerConfig cfg = null;
            try
            {
                P23ContainerFile f = JsonConvert.DeserializeObject<P23ContainerFile>(text);
                if (f != null && f.Container != null) cfg = f.Container;
            }
            catch { }

            if (cfg == null)
            {
                try { cfg = JsonConvert.DeserializeObject<P23ContainerConfig>(text); }
                catch { }
            }
            if (cfg == null) return null;
            cfg = cfg.CloneNewIdentity();
            return AddContainer(cfg, true);
        }

        internal static void SaveAll()
        {
            if (_statePath == null || _restoring) return;
            try
            {
                List<P23ContainerConfig> list = new List<P23ContainerConfig>();
                P23MeterContainer[] a = GetContainers();
                for (int i = 0; i < a.Length; i++)
                {
                    a[i].CaptureGeometry();
                    list.Add(a[i].Config);
                }
                WriteAtomic(_statePath, JsonConvert.SerializeObject(list, Formatting.Indented));
            }
            catch (Exception ex) { Debug.WriteLine("P23 SaveAll: " + ex.Message); }
        }

        internal static void RestoreAll()
        {
            _restoring = true;
            try
            {
                if (File.Exists(_statePath))
                {
                    List<P23ContainerConfig> list =
                        JsonConvert.DeserializeObject<List<P23ContainerConfig>>(File.ReadAllText(_statePath, Encoding.UTF8));
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (list[i] != null) AddContainer(list[i], false);
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("P23 RestoreAll: " + ex.Message); }
            finally { _restoring = false; }

            if (TotalMeterContainers == 0)
                AddContainer(NewDefaultConfig(), false);
            SaveAll();
        }

        internal static void RequestSelect(string id)
        {
            Action<string> h = SelectContainerRequested;
            if (h != null) h(id);
        }

        private static P23ContainerConfig NewDefaultConfig()
        {
            P23ContainerConfig cfg = new P23ContainerConfig();
            cfg.FloatX = 120 + TotalMeterContainers * 24;
            cfg.FloatY = 120 + TotalMeterContainers * 24;

            P23MeterItemConfig s = new P23MeterItemConfig();
            s.Type = P23MeterItemType.SIGNAL_STRENGTH;
            s.Name = "Signal Strength";
            s.Height = 88;
            cfg.Items.Add(s);

            P23MeterItemConfig txt = new P23MeterItemConfig();
            txt.Type = P23MeterItemType.SIGNAL_TEXT;
            txt.Name = "Signal Text";
            txt.Height = 42;
            cfg.Items.Add(txt);

            return cfg;
        }

        private static void Tick()
        {
            if (_console == null || _console.IsDisposed) return;
            P23MeterContainer[] a = GetContainers();
            for (int i = 0; i < a.Length; i++)
            {
                try { a[i].RuntimeUpdate(); } catch { }
            }
        }

        private static void Shutdown()
        {
            if (_timer != null) { _timer.Stop(); _timer.Dispose(); _timer = null; }
            SaveAll();
            P23MultiMeterIO.StopAll();
        }

        private static void RaiseContainersChanged()
        {
            EventHandler h = ContainersChanged;
            if (h != null) h(null, EventArgs.Empty);
        }

        private static void WriteAtomic(string path, string text)
        {
            string dir = Path.GetDirectoryName(path);
            if (!String.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, text, new UTF8Encoding(false));
            if (File.Exists(path))
            {
                string bak = path + ".bak";
                try { File.Replace(tmp, path, bak, true); }
                catch { File.Copy(tmp, path, true); File.Delete(tmp); }
            }
            else File.Move(tmp, path);
        }
    }

    internal sealed class P23ContainerFile
    {
        public string Signature { get; set; }
        public int Version { get; set; }
        public P23ContainerConfig Container { get; set; }
    }
}
