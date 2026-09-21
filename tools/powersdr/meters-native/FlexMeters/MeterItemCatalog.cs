using System;
using System.Collections.Generic;

namespace FlexMeters
{
    public enum MeterItemRendererKind
    {
        SignalBar,
        SignalText,
        Linear
    }

    public sealed class MeterItemDescriptor
    {
        internal MeterItemDescriptor(
            string type,
            string displayName,
            MeterReading reading,
            MeterItemRendererKind rendererKind,
            bool txOnly,
            string units,
            double minimum,
            double maximum)
        {
            Type = type;
            DisplayName = displayName;
            Reading = reading;
            RendererKind = rendererKind;
            TxOnly = txOnly;
            Units = units;
            Minimum = minimum;
            Maximum = maximum;
        }

        public string Type { get; private set; }
        public string DisplayName { get; private set; }
        public MeterReading Reading { get; private set; }
        public MeterItemRendererKind RendererKind { get; private set; }
        public bool TxOnly { get; private set; }
        public string Units { get; private set; }
        public double Minimum { get; private set; }
        public double Maximum { get; private set; }
    }

    public static class MeterItemCatalog
    {
        private static readonly MeterItemDescriptor[] Items =
        {
            new MeterItemDescriptor(
                "SIGNAL_STRENGTH", "Signal Peak", MeterReading.SignalStrength,
                MeterItemRendererKind.SignalBar, false, "dBm", -133.0, -13.0),
            new MeterItemDescriptor(
                "SIGNAL_TEXT", "Signal Text", MeterReading.SignalStrength,
                MeterItemRendererKind.SignalText, false, "dBm", -133.0, -13.0),

            new MeterItemDescriptor(
                "MIC", "Mic", MeterReading.Mic,
                MeterItemRendererKind.Linear, true, "dB", -60.0, 0.0),
            new MeterItemDescriptor(
                "MIC_PK", "Mic Peak", MeterReading.MicPeak,
                MeterItemRendererKind.Linear, true, "dB", -60.0, 0.0),
            new MeterItemDescriptor(
                "EQ", "EQ", MeterReading.Eq,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "EQ_PK", "EQ Peak", MeterReading.EqPeak,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "LEVELER", "Leveler", MeterReading.Leveler,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "LEVELER_PK", "Leveler Peak", MeterReading.LevelerPeak,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "LVL_G", "Leveler Gain", MeterReading.LevelerGain,
                MeterItemRendererKind.Linear, true, "dB", 0.0, 30.0),
            new MeterItemDescriptor(
                "COMP", "Compressor", MeterReading.Compressor,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "COMP_PK", "Compressor Peak", MeterReading.CompressorPeak,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "ALC", "ALC", MeterReading.Alc,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "ALC_PK", "ALC Peak", MeterReading.AlcPeak,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 0.0),
            new MeterItemDescriptor(
                "ALC_G", "ALC Gain", MeterReading.AlcGain,
                MeterItemRendererKind.Linear, true, "dB", 0.0, 30.0),
            new MeterItemDescriptor(
                "ALC_GROUP", "ALC Group", MeterReading.AlcGroup,
                MeterItemRendererKind.Linear, true, "dB", -30.0, 30.0),
            new MeterItemDescriptor(
                "FWD_PWR", "Forward Power", MeterReading.ForwardPower,
                MeterItemRendererKind.Linear, true, "W", 0.0, 100.0),
            new MeterItemDescriptor(
                "REV_PWR", "Reverse Power", MeterReading.ReversePower,
                MeterItemRendererKind.Linear, true, "W", 0.0, 25.0),
            new MeterItemDescriptor(
                "SWR", "SWR", MeterReading.Swr,
                MeterItemRendererKind.Linear, true, "", 1.0, 5.0)
        };

        public static MeterItemDescriptor[] All
        {
            get
            {
                var copy = new MeterItemDescriptor[Items.Length];
                Array.Copy(Items, copy, Items.Length);
                return copy;
            }
        }

        public static bool TryGet(string type, out MeterItemDescriptor descriptor)
        {
            if (type != null)
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    if (String.Equals(Items[i].Type, type, StringComparison.Ordinal))
                    {
                        descriptor = Items[i];
                        return true;
                    }
                }
            }

            descriptor = null;
            return false;
        }

        public static bool IsSupported(string type)
        {
            MeterItemDescriptor ignored;
            return TryGet(type, out ignored);
        }
    }
}
