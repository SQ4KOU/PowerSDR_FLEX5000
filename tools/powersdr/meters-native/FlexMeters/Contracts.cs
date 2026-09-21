using System;

namespace FlexMeters
{
    public enum MeterReceiver
    {
        Rx1 = 1,
        Rx2 = 2
    }

    public enum MeterReading
    {
        SignalStrength,
        AverageSignalStrength,
        AdcPeak,
        AdcAverage,
        AgcGain,
        Mic,
        MicPeak,
        Eq,
        EqPeak,
        Leveler,
        LevelerPeak,
        LevelerGain,
        Compressor,
        CompressorPeak,
        Alc,
        AlcPeak,
        AlcGain,
        AlcGroup,
        ForwardPower,
        ReversePower,
        Swr,
        Volts,
        Amps
    }

    public struct MeterReadingResult
    {
        private MeterReadingResult(bool isSupported, double? value, string reason)
        {
            IsSupported = isSupported;
            Value = value;
            Reason = reason;
        }

        public bool IsSupported { get; private set; }
        public double? Value { get; private set; }
        public string Reason { get; private set; }

        public static MeterReadingResult Supported(double value)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
                throw new ArgumentOutOfRangeException("value", "Meter values must be finite.");

            return new MeterReadingResult(true, value, null);
        }

        public static MeterReadingResult Unsupported(string reason)
        {
            if (String.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Unsupported readings require a capability reason.", "reason");

            return new MeterReadingResult(false, null, reason);
        }
    }

    public interface IMeterTelemetrySource
    {
        MeterReadingResult Read(MeterReading reading, MeterReceiver receiver);
    }

    public sealed class MeterRadioStateSnapshot
    {
        public long VfoAHertz { get; set; }
        public long VfoBHertz { get; set; }
        public string Mode { get; set; }
        public string Band { get; set; }
        public int FilterLowHz { get; set; }
        public int FilterHighHz { get; set; }
        public int DrivePower { get; set; }
        public bool Mox { get; set; }
        public bool Tune { get; set; }
        public bool Split { get; set; }
        public bool Rx2Enabled { get; set; }
    }

    public interface IMeterRadioState
    {
        MeterRadioStateSnapshot CaptureState();
    }

    public interface IMeterCommandSink
    {
        bool SupportsMox { get; }
        void SetMox(bool enabled);

        bool SupportsTune { get; }
        void SetTune(bool enabled);

        bool SupportsSplit { get; }
        void SetSplit(bool enabled);
    }

    public interface IMeterStore
    {
        MeterWorkspaceSnapshot Load();
        void ReplaceAll(MeterWorkspaceSnapshot snapshot);
    }

    public interface IMeterWindowHost
    {
        void RestoreWindows(MeterWorkspaceSnapshot snapshot);
        bool TryCaptureGeometry(Guid containerId, out MeterWindowGeometry geometry);
        void CloseAll();
    }
}
