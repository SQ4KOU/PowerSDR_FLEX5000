// P24 compatibility types required by the original Thetis MeterManager.
// These definitions isolate Thetis UI/renderer code from HPSDR-only backend objects.
using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace PowerSDR
{
    public enum HPSDRModel
    {
        FIRST = -1,
        HPSDR,
        HERMES,
        ANAN10,
        ANAN10E,
        ANAN100,
        ANAN100B,
        ANAN100D,
        ANAN200D,
        ORIONMKII,
        ANAN7000D,
        ANAN8000D,
        ANAN_G2,
        ANAN_G2_1K,
        ANVELINAPRO3,
        HERMESLITE,
        REDPITAYA,
        ANAN_G2E,
        LAST
    }

    public enum SquelchState
    {
        OFF = 0,
        SQL = 1,
        VSQL = 2,
        LAST
    }

    internal static class HardwareSpecific
    {
        internal static HPSDRModel Model { get { return HPSDRModel.HPSDR; } }
    }

    internal sealed class P24DisplayAdaptorInfo
    {
        public string Description { get; set; }
        public bool IsHardware { get; set; }
        public bool IsDefaultHardware { get; set; }
        public bool IsDisplayAttached { get; set; }
        public int VendorId { get; set; }
        public int DeviceId { get; set; }
        public long DedicatedVideoMemory { get; set; }
        public long DedicatedSystemMemory { get; set; }
        public long SharedSystemMemory { get; set; }
        public long AdapterLuid { get; set; }
    }

    public sealed class RecordingDetails
    {
        public DateTime UtcTime { get; set; }
        public string Frequency { get; set; }
        public string Mode { get; set; }
        public string Band { get; set; }

        public RecordingDetails()
        {
            Frequency = "";
            Mode = "";
            Band = "";
        }
    }

    public sealed class clsAudioRecordPlayback
    {
        public sealed class RecordingJsonModel
        {
            public string wav_file { get; set; }
            public double play_duration_seconds { get; set; }
            public string frequency { get; set; }
            public string mode { get; set; }
            public string band { get; set; }
            public int bit_depth { get; set; }
            public int channels { get; set; }
            public int sample_rate { get; set; }
            public DateTime utc_time { get; set; }
        }
    }

    internal static class BandStackManager
    {
        internal static string BandToString(Band band)
        {
            return band.ToString();
        }

        internal static Color BandToColour(Band band)
        {
            // PowerSDR has no Thetis BandStack colour service.
            // The native meter renderer still receives a stable band colour.
            int v = Math.Abs((int)band);
            int r = 64 + ((v * 53) % 128);
            int g = 64 + ((v * 79) % 128);
            int b = 64 + ((v * 101) % 128);
            return Color.FromArgb(r, g, b);
        }
    }

    internal sealed class P24AudioRecordPlaybackAdapter
    {
        private readonly PowerSDR.Console _console;
        internal P24AudioRecordPlaybackAdapter(PowerSDR.Console console) { _console = console; }

        internal event Action<bool, string, string, bool> PlayingChanged;
        internal event Action<bool, string, string> RecordingChanged;
        internal event Action<string, clsAudioRecordPlayback.RecordingJsonModel> RecordingJsonWritten;

        internal bool IsPlaying { get; private set; }
        internal bool IsRecording { get; private set; }
        internal int InputPCDeviceID { get { return 0; } }
        internal int OutputPCDeviceID { get { return 0; } }
        internal string AudioFolder
        {
            get
            {
                string root = P23MeterManager.Root;
                if (String.IsNullOrEmpty(root))
                    root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FlexRadio Systems", "PowerSDR");
                string p = Path.Combine(root, "P24MeterAudio");
                Directory.CreateDirectory(p);
                return p;
            }
        }

        internal bool CanBePlayed(string path)
        {
            return !String.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        internal bool StopPlayback(out string error)
        {
            error = null;
            if (!IsPlaying) return true;
            IsPlaying = false;
            Action<bool, string, string, bool> h = PlayingChanged;
            if (h != null) h(false, "", "", true);
            return true;
        }

        internal bool StopRecord(out string error)
        {
            error = null;
            if (!IsRecording) return true;
            IsRecording = false;
            Action<bool, string, string> h = RecordingChanged;
            if (h != null) h(false, "", "");
            return true;
        }

        internal bool DeleteRecording(string path, out string error)
        {
            return DeleteRecording(path, out error, false);
        }

        internal bool DeleteRecording(string path, out string error, bool folder)
        {
            error = null;
            try
            {
                if (String.IsNullOrWhiteSpace(path)) return true;
                string p = path;
                if (!Path.IsPathRooted(p)) p = Path.Combine(AudioFolder, p);
                if (folder)
                {
                    if (Directory.Exists(p)) Directory.Delete(p, true);
                }
                else
                {
                    if (File.Exists(p)) File.Delete(p);
                    string json = Path.ChangeExtension(p, ".json");
                    if (File.Exists(json)) File.Delete(json);
                }
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        internal bool GetJSONDetailsFromFile(string wavPath, out clsAudioRecordPlayback.RecordingJsonModel model)
        {
            model = null;
            return !String.IsNullOrWhiteSpace(wavPath) && File.Exists(wavPath);
        }

        internal bool PlayFileViaWDSP(string id, string path, int rx, out string error)
        {
            return PlayFileViaWDSP(id, path, rx, out error, 0f, false);
        }

        internal bool PlayFileViaWDSP(string id, string path, int rx, out string error, float gain, bool ignoreTemp)
        {
            error = null;
            if (!CanBePlayed(path)) { error = "File not found"; return false; }
            IsPlaying = true;
            Action<bool, string, string, bool> h = PlayingChanged;
            if (h != null) h(true, id ?? "", path, true);
            return true;
        }

        internal bool PlayFileViaPCAudio(string id, string path, int device, out string error)
        {
            error = null;
            if (!CanBePlayed(path)) { error = "File not found"; return false; }
            IsPlaying = true;
            Action<bool, string, string, bool> h = PlayingChanged;
            if (h != null) h(true, id ?? "", path, false);
            return true;
        }

        internal string RecordToFileFromWDSP(string id, string file, int rx, out string error, bool writeJson, RecordingDetails details, bool ignoreTemp)
        {
            return StartRecord(id, file, out error, details);
        }

        internal string RecordToFileFromPCAudio(string id, string file, int device, out string error, bool writeJson, RecordingDetails details)
        {
            return StartRecord(id, file, out error, details);
        }

        private string StartRecord(string id, string file, out string error, RecordingDetails details)
        {
            error = "Native Thetis ARP recording is not mapped to PowerSDR yet";
            return null;
        }
    }

    internal class SpecHPSDR
    {
        internal SpecHPSDR(int display) { }
        internal bool Update { get; set; }
        internal int FrameRate { get; set; }
        internal int PixelOut { get; set; }
        internal bool IgnoreFrequencyOffset { get; set; }
        internal int Pixels { get; set; }
        internal bool AverageOn { get; set; }
        internal dynamic DetTypePan { get; set; }
        internal double AvTau { get; set; }
        internal int FFTSize { get; set; }
        internal int BlockSize { get; set; }
        internal int SampleRate { get; set; }
        internal dynamic WindowType { get; set; }
        internal dynamic AverageMode { get; set; }
        internal bool NormOneHzPan { get; set; }
        internal void initAnalyzer() { }
        internal void resetPixelBuffers() { }
    }

    internal static unsafe class SpecHPSDRDLL
    {
        internal static void GetPixels(int display, int channel, float* data, ref bool flag)
        {
            flag = false;
        }
    }

    internal static class cmaster
    {
        private static int _next = 1;
        internal static int AllocAnalyzer(int stream, int id, int size) { return _next++; }
        internal static void RunAnalyzer(int display, int run) { }
        internal static void FreeAnalyzer(int display) { }
        internal static int inid(int rx, int sub) { return rx * 2 + sub; }
    }
}
