using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace FlexMeters
{
    public sealed class ThetisMeterItemSettings
    {
        public const string UpdateIntervalKey = "_updateInterval";
        public const string AttackKey = "_attack";
        public const string DecayKey = "_decay";
        public const string ShadowKey = "_shadow";
        public const string ShowHistoryKey = "_showHistory";
        public const string PeakHoldKey = "_peakHold";
        public const string FadeOnRxKey = "_fadeOnRx";
        public const string FadeOnTxKey = "_fadeOnTx";
        public const string ShowTypeKey = "_showType";
        public const string PeakValueKey = "_peakValue";
        public const string DarkModeKey = "_darkMode";
        public const string LowColorKey = "_lowColor";
        public const string HighColorKey = "_highColor";
        public const string BackgroundColorKey = "_backgroundColor";
        public const string TitleColorKey = "_titleColor";
        public const string HistoryColorKey = "_historyColor";
        public const string PeakHoldColorKey = "_peakHoldMarkerColor";
        public const string PeakValueColorKey = "_peakValueColour";
        public const string SegmentedKey = "_segmented";
        public const string SolidKey = "_solid";

        public int UpdateIntervalMs { get; set; }
        public double Attack { get; set; }
        public double Decay { get; set; }
        public bool Shadow { get; set; }
        public bool ShowHistory { get; set; }
        public bool PeakHold { get; set; }
        public bool FadeOnRx { get; set; }
        public bool FadeOnTx { get; set; }
        public bool ShowType { get; set; }
        public bool PeakValue { get; set; }
        public bool DarkMode { get; set; }
        public bool Segmented { get; set; }
        public bool Solid { get; set; }

        public Color LowColor { get; set; }
        public Color HighColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color TitleColor { get; set; }
        public Color HistoryColor { get; set; }
        public Color PeakHoldColor { get; set; }
        public Color PeakValueColor { get; set; }

        public ThetisMeterItemSettings()
        {
            UpdateIntervalMs = 100;
            Attack = 0.8;
            Decay = 0.2;
            Shadow = false;
            ShowHistory = true;
            PeakHold = true;
            FadeOnRx = false;
            FadeOnTx = false;
            ShowType = true;
            PeakValue = true;
            DarkMode = true;
            Segmented = false;
            Solid = true;

            LowColor = Color.Yellow;
            HighColor = Color.Red;
            BackgroundColor = Color.FromArgb(32, 32, 32);
            TitleColor = Color.DarkGray;
            HistoryColor = Color.FromArgb(128, Color.Red);
            PeakHoldColor = Color.Red;
            PeakValueColor = Color.Red;
        }

        public static ThetisMeterItemSettings FromItem(MeterItemSnapshot item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            var s = new ThetisMeterItemSettings();
            s.UpdateIntervalMs = GetInt(item.Settings, UpdateIntervalKey, s.UpdateIntervalMs, 10, 5000);
            s.Attack = GetDouble(item.Settings, AttackKey, s.Attack, 0.0, 1.0);
            s.Decay = GetDouble(item.Settings, DecayKey, s.Decay, 0.0, 1.0);
            s.Shadow = GetBool(item.Settings, ShadowKey, s.Shadow);
            s.ShowHistory = GetBool(item.Settings, ShowHistoryKey, s.ShowHistory);
            s.PeakHold = GetBool(item.Settings, PeakHoldKey, s.PeakHold);
            s.FadeOnRx = GetBool(item.Settings, FadeOnRxKey, s.FadeOnRx);
            s.FadeOnTx = GetBool(item.Settings, FadeOnTxKey, s.FadeOnTx);
            s.ShowType = GetBool(item.Settings, ShowTypeKey, s.ShowType);
            s.PeakValue = GetBool(item.Settings, PeakValueKey, s.PeakValue);
            s.DarkMode = GetBool(item.Settings, DarkModeKey, s.DarkMode);
            s.Segmented = GetBool(item.Settings, SegmentedKey, s.Segmented);
            s.Solid = GetBool(item.Settings, SolidKey, s.Solid);

            s.LowColor = GetColor(item.Settings, LowColorKey, s.LowColor);
            s.HighColor = GetColor(item.Settings, HighColorKey, s.HighColor);
            s.BackgroundColor = GetColor(item.Settings, BackgroundColorKey, s.BackgroundColor);
            s.TitleColor = GetColor(item.Settings, TitleColorKey, s.TitleColor);
            s.HistoryColor = GetColor(item.Settings, HistoryColorKey, s.HistoryColor);
            s.PeakHoldColor = GetColor(item.Settings, PeakHoldColorKey, s.PeakHoldColor);
            s.PeakValueColor = GetColor(item.Settings, PeakValueColorKey, s.PeakValueColor);
            return s;
        }

        public void ApplyTo(MeterItemSnapshot item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            Put(item.Settings, UpdateIntervalKey, UpdateIntervalMs.ToString(CultureInfo.InvariantCulture));
            Put(item.Settings, AttackKey, Attack.ToString("0.###", CultureInfo.InvariantCulture));
            Put(item.Settings, DecayKey, Decay.ToString("0.###", CultureInfo.InvariantCulture));
            Put(item.Settings, ShadowKey, B(Shadow));
            Put(item.Settings, ShowHistoryKey, B(ShowHistory));
            Put(item.Settings, PeakHoldKey, B(PeakHold));
            Put(item.Settings, FadeOnRxKey, B(FadeOnRx));
            Put(item.Settings, FadeOnTxKey, B(FadeOnTx));
            Put(item.Settings, ShowTypeKey, B(ShowType));
            Put(item.Settings, PeakValueKey, B(PeakValue));
            Put(item.Settings, DarkModeKey, B(DarkMode));
            Put(item.Settings, SegmentedKey, B(Segmented));
            Put(item.Settings, SolidKey, B(Solid));

            Put(item.Settings, LowColorKey, LowColor.ToArgb().ToString(CultureInfo.InvariantCulture));
            Put(item.Settings, HighColorKey, HighColor.ToArgb().ToString(CultureInfo.InvariantCulture));
            Put(item.Settings, BackgroundColorKey, BackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
            Put(item.Settings, TitleColorKey, TitleColor.ToArgb().ToString(CultureInfo.InvariantCulture));
            Put(item.Settings, HistoryColorKey, HistoryColor.ToArgb().ToString(CultureInfo.InvariantCulture));
            Put(item.Settings, PeakHoldColorKey, PeakHoldColor.ToArgb().ToString(CultureInfo.InvariantCulture));
            Put(item.Settings, PeakValueColorKey, PeakValueColor.ToArgb().ToString(CultureInfo.InvariantCulture));
        }

        public static void ApplyDefaults(MeterItemSnapshot item)
        {
            new ThetisMeterItemSettings().ApplyTo(item);
        }

        private static string B(bool value)
        {
            return value ? "1" : "0";
        }

        private static void Put(IDictionary<string, string> settings, string key, string value)
        {
            if (settings.ContainsKey(key))
                settings[key] = value;
            else
                settings.Add(key, value);
        }

        private static bool GetBool(
            IDictionary<string, string> settings,
            string key,
            bool fallback)
        {
            string value;
            if (!settings.TryGetValue(key, out value))
                return fallback;
            if (value == "1") return true;
            if (value == "0") return false;
            bool parsed;
            return Boolean.TryParse(value, out parsed) ? parsed : fallback;
        }

        private static int GetInt(
            IDictionary<string, string> settings,
            string key,
            int fallback,
            int min,
            int max)
        {
            string value;
            int parsed;
            if (!settings.TryGetValue(key, out value) ||
                !Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                return fallback;
            if (parsed < min) return min;
            if (parsed > max) return max;
            return parsed;
        }

        private static double GetDouble(
            IDictionary<string, string> settings,
            string key,
            double fallback,
            double min,
            double max)
        {
            string value;
            double parsed;
            if (!settings.TryGetValue(key, out value) ||
                !Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return fallback;
            if (parsed < min) return min;
            if (parsed > max) return max;
            return parsed;
        }

        private static Color GetColor(
            IDictionary<string, string> settings,
            string key,
            Color fallback)
        {
            string value;
            int argb;
            if (!settings.TryGetValue(key, out value) ||
                !Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out argb))
                return fallback;
            return Color.FromArgb(argb);
        }
    }
}
