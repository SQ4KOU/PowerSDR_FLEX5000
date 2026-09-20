using System;
using System.Collections.Generic;

namespace PowerSDR
{
    // Exact enum values retained where MeterManager serialisation/switches depend on them.
    public enum HPSDRModel
    {
        FIRST = -1, HPSDR, HERMES, ANAN10, ANAN10E, ANAN100, ANAN100B, ANAN100D,
        ANAN200D, ORIONMKII, ANAN7000D, ANAN8000D, ANAN_G2, ANAN_G2_1K,
        ANVELINAPRO3, HERMESLITE, REDPITAYA, ANAN_G2E, LAST
    }

    public enum SquelchState { OFF = 0, SQL = 1, VSQL = 2, LAST }

    public enum OtherButtonId
    {
        POWER = 0, RX_2, MON, TUN, MOX, TWOTON, DUP, PS_A, XPA, REC, PLAY, NR, ANF, NB, SNB,
        MNF, MNF_PLUS, SPLT, A_TO_B, ZERO_BEAT, B_TO_A, IF_TO_V, SWAP_AB, AVG, PEAK_HOLD, CTUN,
        VAC1, VAC2, MUTE, BIN, SUBRX, PAN_SWAP, NR1, NR2, NR3, NR4, NB1, NB2, SPECTRUM,
        PANADAPTER, SCOPE, SCOPE2, PHASE, WATERFALL, HISTOGRAM, PANAFALL, PANASCOPE,
        SPECTRASCOPE, DISPLAY_OFF, PAUSE, PEAK_BLOBS, CURSOR_INFO, SPOTS, FILL_SPECTRUM,
        SQL, SQL_SQL, SQL_VSQL, RIT, RIT0, XIT, XIT0, MIC, COMP, VOX, DEXP, RX_EQ, TX_EQ,
        TX_FILTER, CFC, CFC_EQ, LEVELER, PHASE_ROT, AGC_FIXED, AGC_LONG, AGC_SLOW, AGC_MEDIUM,
        AGC_FAST, AGC_CUSTOM, AGC_AUTO, DITHER, RANDOM, SR_48000, SR_96000, SR_192000, SR_384000,
        SR_768000, SR_1536000, ATT_STEP, ATT_0, ATT_10, ATT_20, ATT_30, ATT_40, ATT_50, ATT_P1,
        ATT_M1, PAN_P5, PAN_M5, PAN_CENTRE, ZTB, ZOOM_0P5, ZOOM_1, ZOOM_2, ZOOM_4, MUTE_ALL,
        MAF_P5, MAF_M5, AF_P5, AF_M5, BAL_P5, BAL_M5, SAF_P5, SAF_M5, SBAL_P5, SBAL_M5,
        SQL_P5, SQL_M5, MIC_P1, MIC_M1, COMP_P1, COMP_M1, VOX_P1, VOX_M1, AGC_P5, AGC_M5,
        DRIVE_P5, DRIVE_M5, TUNE_P5, TUNE_M5, DRIVE_0, TUN_0, FORM_SETUP, FORM_DBMAN, FORM_MEMORY,
        FORM_AMPVIEW, FORM_EQ, FORM_XVTR, FORM_CWX, FORM_DIVERSITY, FORM_LINEARITY, FORM_WB,
        CWX_KEY, CWX_STOP, CWX_F1, CWX_F2, CWX_F3, CWX_F4, CWX_F5, CWX_F6, CWX_F7, CWX_F8,
        CWX_F9, VFO_SYNC, LOCK_A, LOCK_B, TUNE_STEP_U, TUNE_STEP_D, STACK_U, STACK_D, NF,
        WAVE_RECORD, ACTITVE_PEAK,
        _MACRO_0, _MACRO_1, _MACRO_2, _MACRO_3, _MACRO_4, _MACRO_5, _MACRO_6, _MACRO_7,
        _MACRO_8, _MACRO_9, _MACRO_10, _MACRO_11, _MACRO_12, _MACRO_13, _MACRO_14, _MACRO_15,
        _MACRO_16, _MACRO_17, _MACRO_18, _MACRO_19, _MACRO_20, _MACRO_21, _MACRO_22, _MACRO_23,
        _MACRO_24, _MACRO_25, _MACRO_26, _MACRO_27, _MACRO_28, _MACRO_29, _MACRO_30,
        INFO_TEXT = 998, SPLITTER = 999, INIT = 1000, UNKNOWN = 2000
    }

    [Serializable]
    public class OtherButtonMacroSettings
    {
        public enum OB_ButtonState { OFF = 0, ON, TOGGLE, LED, CONT_VIS, CAT }

        public int Number { get; set; }
        public string OnText { get; set; }
        public string OffText { get; set; }
        public string Notes { get; set; }
        public bool ClosesParent { get; set; }
        public bool[] ClosesContainer { get; private set; }
        public bool[] OpensContainer { get; private set; }
        public string[] CloseContainerID { get; private set; }
        public string[] OpenContainerID { get; private set; }
        public bool[] OpenUsesLocation { get; private set; }
        public bool[] SendsViaMMIO { get; private set; }
        public string[] MMICFourChar { get; private set; }
        public string[] MMIOMessageON { get; private set; }
        public string[] MMIOMessageOFF { get; private set; }
        public OB_ButtonState ButtonStateType { get; set; }
        public string LedIndiciatorFourChar { get; set; }
        public string ContainerVisibleID { get; set; }
        public string ButtonStateCatReply { get; set; }
        public bool[] CatMacroSend { get; private set; }
        public bool RunStateCommandOnVisible { get; set; }
        public bool OnState { get; set; }
        public string CatMacro { get; set; }

        public OtherButtonMacroSettings()
        {
            ClosesContainer = new bool[4];
            OpensContainer = new bool[4];
            CloseContainerID = new string[4];
            OpenContainerID = new string[4];
            OpenUsesLocation = new bool[4];
            SendsViaMMIO = new bool[4];
            MMICFourChar = new string[4];
            MMIOMessageON = new string[4];
            MMIOMessageOFF = new string[4];
            CatMacroSend = new bool[1];
            ButtonStateType = OB_ButtonState.OFF;
        }

        public OtherButtonMacroSettings(OtherButtonMacroSettings other) : this()
        {
            if (other == null) return;
            Number = other.Number; OnText = other.OnText; OffText = other.OffText; Notes = other.Notes;
            ClosesParent = other.ClosesParent; ButtonStateType = other.ButtonStateType;
            LedIndiciatorFourChar = other.LedIndiciatorFourChar; ContainerVisibleID = other.ContainerVisibleID;
            ButtonStateCatReply = other.ButtonStateCatReply; RunStateCommandOnVisible = other.RunStateCommandOnVisible;
            OnState = other.OnState; CatMacro = other.CatMacro;
            Array.Copy(other.ClosesContainer, ClosesContainer, 4);
            Array.Copy(other.OpensContainer, OpensContainer, 4);
            Array.Copy(other.CloseContainerID, CloseContainerID, 4);
            Array.Copy(other.OpenContainerID, OpenContainerID, 4);
            Array.Copy(other.OpenUsesLocation, OpenUsesLocation, 4);
            Array.Copy(other.SendsViaMMIO, SendsViaMMIO, 4);
            Array.Copy(other.MMICFourChar, MMICFourChar, 4);
            Array.Copy(other.MMIOMessageON, MMIOMessageON, 4);
            Array.Copy(other.MMIOMessageOFF, MMIOMessageOFF, 4);
            Array.Copy(other.CatMacroSend, CatMacroSend, 1);
        }
    }

    public sealed class P24DisplayAdaptorInfo
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public string DeviceName { get; set; }
        public long DedicatedVideoMemory { get; set; }
        public int VendorId { get; set; }
        public int DeviceId { get; set; }
        public override string ToString() { return String.IsNullOrEmpty(Description) ? (DeviceName ?? "") : Description; }
    }

    // HPSDR spectrum path is intentionally inert for FLEX-5000. It only exists so the original
    // meter item classes compile; FLEX readings are supplied through the FLEX adapter.
    public class SpecHPSDR
    {
        private int _lowFreq;
        private int _highFreq;

        public SpecHPSDR(int d)
        {
            FrameRate = 15;
            PixelOut = 1;
            Pixels = 2048;
            FFTSize = 4096;
            BlockSize = 2048;
            SampleRate = 48000;
            WindowType = 4;
            PanSlider = 0.5;
            Update = false;
        }

        public bool Update { get; set; }
        public int FrameRate { get; set; }
        public int PixelOut { get; set; }
        public bool IgnoreFrequencyOffset { get; set; }
        public int Pixels { get; set; }
        public bool AverageOn { get; set; }
        public int DetTypePan { get; set; }
        public double AvTau { get; set; }
        public int FFTSize { get; set; }
        public int BlockSize { get; set; }
        public int SampleRate { get; set; }
        public int WindowType { get; set; }
        public int AverageMode { get; set; }
        public bool NormOneHzPan { get; set; }
        public double PanSlider { get; set; }
        public int LowFreq { get { return _lowFreq; } }
        public int HighFreq { get { return _highFreq; } }

        public void initAnalyzer()
        {
            Tuple<int,int> ext = FrequencyExtents(0.0, PanSlider);
            _lowFreq = ext.Item1;
            _highFreq = ext.Item2;
        }

        public void resetPixelBuffers() { }

        public void ZoomToBandwidth(double targetBandwidthHz)
        {
            if (SampleRate <= 0) return;
            double width = Math.Max(1.0, Math.Min((double)SampleRate, targetBandwidthHz));
            double ratio = width / SampleRate;
            _lowFreq = -(int)Math.Round(width / 2.0);
            _highFreq = (int)Math.Round(width / 2.0);
        }

        public (int, int) GetFrequencyExtents(double zoomSlider, double panSlider)
        {
            Tuple<int,int> ext = FrequencyExtents(zoomSlider, panSlider);
            return (ext.Item1, ext.Item2);
        }

        private Tuple<int,int> FrequencyExtents(double zoomSlider, double panSlider)
        {
            int sr = SampleRate > 0 ? SampleRate : 48000;
            double zoom = Math.Max(0.0, Math.Min(1.0, zoomSlider));
            double width = sr * (1.0 - 0.99 * zoom);
            double centreShift = (Math.Max(0.0, Math.Min(1.0, panSlider)) - 0.5) * (sr - width);
            int lo = (int)Math.Round(-width / 2.0 + centreShift);
            int hi = (int)Math.Round(width / 2.0 + centreShift);
            return Tuple.Create(lo, hi);
        }
    }

    public class MNotch : IComparable
    {
        public double FCenter { get; set; }
        public double FWidth { get; set; }
        public bool Active { get; set; }

        public MNotch(double freq, double width, bool active)
        {
            FCenter = freq;
            FWidth = width;
            Active = active;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            MNotch other = obj as MNotch;
            return other == null ? 1 : FCenter.CompareTo(other.FCenter);
        }
    }

    public sealed class RecordingDetails
    {
        public DateTime UtcTime { get; set; }
        public string Frequency { get; set; }
        public string Mode { get; set; }
        public string Band { get; set; }
        public string WavFile { get; set; }
        public long? WavFileSizeBytes { get; set; }
        public DateTime? WavFileLastWriteUtc { get; set; }
        public double? PlayDurationSeconds { get; set; }
        public int SampleRate { get; set; }
        public short BitDepth { get; set; }
        public short Channels { get; set; }
        public short FormatTag { get; set; }
        public string Mp3File { get; set; }
        public long? Mp3FileSizeBytes { get; set; }
    }

    internal static class P24CMaster
    {
        private static int _next = 16;
        internal static int AllocAnalyzer(int type, int id, int size) { return System.Threading.Interlocked.Increment(ref _next); }
        internal static void RunAnalyzer(int display, int run) { }
        internal static void FreeAnalyzer(int display) { }
        internal static int inid(int rx, int subrx) { return rx * 2 + subrx; }
    }

    internal static unsafe class P24SpecHPSDRDLL
    {
        internal static void GetPixels(int disp, int pixout, float* pix, ref int flag)
        {
            flag = 0;
        }
    }

    // Native PowerSDR wave/recording integration will be adapted after the original renderer compiles.
    public sealed class clsAudioRecordPlayback
    {
        public sealed class RecordingJsonModel
        {
            public string utc_time { get; set; }
            public string frequency { get; set; }
            public string mode { get; set; }
            public string band { get; set; }
            public string ddcfrequency { get; set; }
            public string wav_file { get; set; }
            public long wav_file_size_bytes { get; set; }
            public string wav_file_last_write_utc { get; set; }
            public double play_duration_seconds { get; set; }
            public int sample_rate { get; set; }
            public short bit_depth { get; set; }
            public short channels { get; set; }
            public short format_tag { get; set; }
            public string tag_description { get; set; }
            public string mp3_file { get; set; }
            public long mp3_file_size_bytes { get; set; }
        }

        public sealed class RecordingDetails
        {
            public DateTime UtcTime { get; set; }
            public string Frequency { get; set; }
            public string Mode { get; set; }
            public string Band { get; set; }
            public string DDCFrequency { get; set; }
            public string WavFile { get; set; }
            public string Mp3File { get; set; }
            public double? PlayDurationSeconds { get; set; }
        }

        public bool IsPlaying { get { return false; } }
        public bool IsRecording { get { return false; } }
        public bool IsBusy { get { return false; } }
        public bool StopPlayback() { return true; }
        public bool StopRecord() { return true; }
    }
}
