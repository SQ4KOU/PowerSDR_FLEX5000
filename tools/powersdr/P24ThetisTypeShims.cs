using System;
using System.Collections.Generic;
using System.Drawing;

namespace PowerSDR
{
    internal sealed class P24AdaptorInfo
    {
        public int AdaptorIndex { get; set; }
        public string Description { get; set; }
    }

    public enum HPSDRModel
    {
        FIRST = -1,
        HPSDR = 0,
        HERMES = 1,
        ANAN10,
        ANAN10E,
        ANAN100,
        ANAN100B,
        ANAN100D,
        ANAN200D,
        ORIONMKII,
        ANAN8000D,
        ANAN_G2,
        REDPITAYA,
        LAST
    }

    internal static class HardwareSpecific
    {
        internal static HPSDRModel Model { get { return HPSDRModel.HERMES; } }
    }

    public enum SquelchState
    {
        OFF = 0,
        SQL = 1,
        VSQL = 2,
        LAST
    }

    public sealed class clsAudioRecordPlayback
    {
        public sealed class RecordingJsonModel
        {
            public string wav_file;
            public string frequency;
            public string mode;
            public string band;
            public double play_duration_seconds;
            public int bit_depth;
            public int sample_rate;
            public int channels;
            public string utc_time;
        }
    }

    // Thetis Filter Display uses MiniSpec/WDSP. FLEX-5000 PowerSDR has a different
    // DSP/display pipeline, so P24 keeps the item API available without importing HPSDR.
    public static class MiniSpec
    {
        public const int TX_BANDWIDTH = 20000;
        public const int PIXELS = 1024;
        public const int FRAME_RATE = 30;

        public sealed class FilterCharacteristics
        {
            public double[] segments = new double[0];
            public int index_low;
            public int index_upper;
            public double corner_freq;
            public int hz_span;
            public int middle_index;
            public int six_db_shift;
            public double min;
            public double max;
        }

        public sealed class Notch
        {
            public int index;
            public double frequency_hz;
            public double width_hz;
            public bool active;
        }

        public sealed class clsMiniSpec
        {
            private readonly float[] _data = new float[PIXELS];
            private readonly float[] _raw = new float[PIXELS];
            public double CentreFreq { get; set; }
            public double RXFrequency { get; set; }
            public double TXFrequency { get; set; }
            public int DataIndex { get; private set; }
            public float[] Data { get { return _data; } }
            public float[] DataRaw { get { return _raw; } }
        }

        private static readonly object _notchLocker = new object();
        private static readonly FilterCharacteristics _rx = new FilterCharacteristics();
        private static readonly FilterCharacteristics _tx = new FilterCharacteristics();

        public static object NotchLocker { get { return _notchLocker; } }
        public static object FilterCharacteristicsLocker { get { return _notchLocker; } }

        public static clsMiniSpec GetMiniRX(int id, bool sub_receiver = false) { return null; }
        public static void UsingFilter(int id, bool sub_receiver = false) { }
        public static void StopUsingFilter(int id, bool sub_receiver = false) { }
        public static void ShutdownAllRX() { }
        public static Notch GetNotch(int index) { return null; }
        public static List<Notch> GetNotches() { return new List<Notch>(); }
        public static FilterCharacteristics GetRXCharacteristic(int rx) { return _rx; }
        public static FilterCharacteristics GetTXCharacteristic() { return _tx; }
    }
}
