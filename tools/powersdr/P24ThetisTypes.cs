using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace PowerSDR
{
    public enum HPSDRModel
    {
        FIRST=-1,HPSDR,HERMES,ANAN10,ANAN10E,ANAN100,ANAN100B,ANAN100D,ANAN200D,
        ORIONMKII,ANAN7000D,ANAN8000D,ANAN_G2,ANAN_G2_1K,ANVELINAPRO3,HERMESLITE,
        REDPITAYA,ANAN_G2E,LAST
    }

    public enum SquelchState { OFF=0, SQL=1, VSQL=2, LAST }

    public sealed class P24AdaptorInfo
    {
        public int DeviceId { get; set; }
        public int VendorId { get; set; }
    }

    public enum OtherButtonId
    {
        POWER=0,RX_2,MON,TUN,MOX,TWOTON,DUP,PS_A,XPA,REC,PLAY,NR,ANF,NB,SNB,MNF,MNF_PLUS,SPLT,
        A_TO_B,ZERO_BEAT,B_TO_A,IF_TO_V,SWAP_AB,AVG,PEAK_HOLD,CTUN,VAC1,VAC2,MUTE,BIN,SUBRX,PAN_SWAP,
        NR1,NR2,NR3,NR4,NB1,NB2,SPECTRUM,PANADAPTER,SCOPE,SCOPE2,PHASE,WATERFALL,HISTOGRAM,PANAFALL,
        PANASCOPE,SPECTRASCOPE,DISPLAY_OFF,PAUSE,PEAK_BLOBS,CURSOR_INFO,SPOTS,FILL_SPECTRUM,SQL,SQL_SQL,
        SQL_VSQL,RIT,RIT0,XIT,XIT0,MIC,COMP,VOX,DEXP,RX_EQ,TX_EQ,TX_FILTER,CFC,CFC_EQ,LEVELER,PHASE_ROT,
        AGC_FIXED,AGC_LONG,AGC_SLOW,AGC_MEDIUM,AGC_FAST,AGC_CUSTOM,AGC_AUTO,DITHER,RANDOM,SR_48000,
        SR_96000,SR_192000,SR_384000,SR_768000,SR_1536000,ATT_STEP,ATT_0,ATT_10,ATT_20,ATT_30,ATT_40,
        ATT_50,ATT_P1,ATT_M1,PAN_P5,PAN_M5,PAN_CENTRE,ZTB,ZOOM_0P5,ZOOM_1,ZOOM_2,ZOOM_4,MUTE_ALL,
        MAF_P5,MAF_M5,AF_P5,AF_M5,BAL_P5,BAL_M5,SAF_P5,SAF_M5,SBAL_P5,SBAL_M5,SQL_P5,SQL_M5,MIC_P1,
        MIC_M1,COMP_P1,COMP_M1,VOX_P1,VOX_M1,AGC_P5,AGC_M5,DRIVE_P5,DRIVE_M5,TUNE_P5,TUNE_M5,DRIVE_0,
        TUN_0,FORM_SETUP,FORM_DBMAN,FORM_MEMORY,FORM_AMPVIEW,FORM_EQ,FORM_XVTR,FORM_CWX,FORM_DIVERSITY,
        FORM_LINEARITY,FORM_WB,CWX_KEY,CWX_STOP,CWX_F1,CWX_F2,CWX_F3,CWX_F4,CWX_F5,CWX_F6,CWX_F7,CWX_F8,
        CWX_F9,VFO_SYNC,LOCK_A,LOCK_B,TUNE_STEP_U,TUNE_STEP_D,STACK_U,STACK_D,NF,WAVE_RECORD,ACTITVE_PEAK,
        _MACRO_0,_MACRO_1,_MACRO_2,_MACRO_3,_MACRO_4,_MACRO_5,_MACRO_6,_MACRO_7,_MACRO_8,_MACRO_9,
        _MACRO_10,_MACRO_11,_MACRO_12,_MACRO_13,_MACRO_14,_MACRO_15,_MACRO_16,_MACRO_17,_MACRO_18,
        _MACRO_19,_MACRO_20,_MACRO_21,_MACRO_22,_MACRO_23,_MACRO_24,_MACRO_25,_MACRO_26,_MACRO_27,
        _MACRO_28,_MACRO_29,_MACRO_30,INFO_TEXT=998,SPLITTER=999,INIT=1000,UNKNOWN=2000
    }

    [Serializable]
    public class OtherButtonMacroSettings
    {
        public enum OB_ButtonState { OFF=0, ON, TOGGLE, LED, CONT_VIS, CAT }
        public int Number { get; set; }
        public string OnText { get; set; }
        public string OffText { get; set; }
        public string Notes { get; set; }
        public bool ClosesParent { get; set; }
        public bool[] ClosesContainer { get; private set; } = new bool[4];
        public bool[] OpensContainer { get; private set; } = new bool[4];
        public string[] CloseContainerID { get; private set; } = new string[4];
        public string[] OpenContainerID { get; private set; } = new string[4];
        public bool[] OpenUsesLocation { get; private set; } = new bool[4];
        public bool[] SendsViaMMIO { get; private set; } = new bool[4];
        public string[] MMICFourChar { get; private set; } = new string[4];
        public string[] MMIOMessageON { get; private set; } = new string[4];
        public string[] MMIOMessageOFF { get; private set; } = new string[4];
        public OB_ButtonState ButtonStateType { get; set; }
        public string LedIndiciatorFourChar { get; set; }
        public string ContainerVisibleID { get; set; }
        public string ButtonStateCatReply { get; set; }
        public bool RunStateCommandOnVisible { get; set; }
        public bool[] CatMacroSend { get; private set; } = new bool[1];
        public bool OnState { get; set; }
        public string CatMacro { get; set; }

        public OtherButtonMacroSettings() { ButtonStateType = OB_ButtonState.OFF; }
        public OtherButtonMacroSettings(OtherButtonMacroSettings other)
        {
            if(other==null) return;
            Number=other.Number; OnText=other.OnText; OffText=other.OffText; Notes=other.Notes;
            ClosesParent=other.ClosesParent; ButtonStateType=other.ButtonStateType;
            LedIndiciatorFourChar=other.LedIndiciatorFourChar; ContainerVisibleID=other.ContainerVisibleID;
            ButtonStateCatReply=other.ButtonStateCatReply; RunStateCommandOnVisible=other.RunStateCommandOnVisible;
            OnState=other.OnState; CatMacro=other.CatMacro;
            Array.Copy(other.ClosesContainer,ClosesContainer,Math.Min(4,other.ClosesContainer.Length));
            Array.Copy(other.OpensContainer,OpensContainer,Math.Min(4,other.OpensContainer.Length));
            Array.Copy(other.CloseContainerID,CloseContainerID,Math.Min(4,other.CloseContainerID.Length));
            Array.Copy(other.OpenContainerID,OpenContainerID,Math.Min(4,other.OpenContainerID.Length));
            Array.Copy(other.OpenUsesLocation,OpenUsesLocation,Math.Min(4,other.OpenUsesLocation.Length));
            Array.Copy(other.SendsViaMMIO,SendsViaMMIO,Math.Min(4,other.SendsViaMMIO.Length));
            Array.Copy(other.MMICFourChar,MMICFourChar,Math.Min(4,other.MMICFourChar.Length));
            Array.Copy(other.MMIOMessageON,MMIOMessageON,Math.Min(4,other.MMIOMessageON.Length));
            Array.Copy(other.MMIOMessageOFF,MMIOMessageOFF,Math.Min(4,other.MMIOMessageOFF.Length));
            Array.Copy(other.CatMacroSend,CatMacroSend,Math.Min(1,other.CatMacroSend.Length));
        }
    }

    public static class OtherButtonIdHelpers
    {
        public const int MAX_BITFIELD_GROUP = 12;
        public const int MACRO_BUTTONS_PERGROUP = 31;

        public static (int bit_group, int bit) BitFromID(OtherButtonId id)
        {
            int n=(int)id;
            if(n<0 || n>=MAX_BITFIELD_GROUP*32) return (-1,-1);
            return (n/32,n%32);
        }
        public static OtherButtonId BitToID(int group,int bit)
        {
            int n=group*32+bit;
            if(Enum.IsDefined(typeof(OtherButtonId),n)) return (OtherButtonId)n;
            return OtherButtonId.UNKNOWN;
        }
        public static string BitToText(int group,int bit)
        {
            OtherButtonId id=BitToID(group,bit);
            return id==OtherButtonId.UNKNOWN ? "" : id.ToString().Replace('_',' ');
        }
        public static (string,string) BitToIcon(int group,int bit) { return ("",""); }
    }

    public sealed class RecordingDetails
    {
        public string Band { get; set; }
        public string Frequency { get; set; }
        public string Mode { get; set; }
        public DateTime UtcTime { get; set; }
    }

    public static class BandStackManager
    {
        public static string BandToString(Band b) { return b.ToString(); }
    }

    public sealed class clsAudioRecordPlayback
    {
        public sealed class RecordingJsonModel
        {
            public string wav_file;
            public double play_duration_seconds;
            public string frequency;
            public string mode;
            public string band;
            public int bit_depth;
            public int sample_rate;
            public int channels;
        }
    }

    public sealed class P24AudioRecordAdapter
    {
        public static readonly P24AudioRecordAdapter Instance = new P24AudioRecordAdapter();
        public event Action<bool,string,string> RecordingChanged;
        public event Action<bool,string,string,bool> PlayingChanged;
        public event Action<string,clsAudioRecordPlayback.RecordingJsonModel> RecordingJsonWritten;
        public bool IsRecording { get; private set; }
        public bool IsPlaying { get; private set; }
        public int InputPCDeviceID { get { return -1; } }
        public int OutputPCDeviceID { get { return -1; } }
        public string AudioFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"PowerSDR","Audio"); } }
        public bool CanBePlayed(string path) { return !String.IsNullOrWhiteSpace(path) && File.Exists(path); }
        public bool StopPlayback(out string error) { error=null; IsPlaying=false; var h=PlayingChanged; if(h!=null) h(false,"","",false); return true; }
        public bool StopRecord(out string error) { error=null; IsRecording=false; var h=RecordingChanged; if(h!=null) h(false,"",""); return true; }
        public bool DeleteRecording(string path,out string error,bool wholeFolder=false)
        {
            error=null; try { if(wholeFolder && Directory.Exists(path)) Directory.Delete(path,true); else if(File.Exists(path)) File.Delete(path); return true; } catch(Exception ex){error=ex.Message;return false;}
        }
        public bool GetJSONDetailsFromFile(string path,out clsAudioRecordPlayback.RecordingJsonModel json) { json=null; return File.Exists(path); }
        public string RecordToFileFromWDSP(string id,string file,int rx,out string error,bool writeJson,RecordingDetails details,bool ignoreChanges=false) { error="FLEX-5000 P24: Thetis WDSP recorder is unavailable"; return null; }
        public string RecordToFileFromPCAudio(string id,string file,int device,out string error,bool writeJson,RecordingDetails details) { error="FLEX-5000 P24: Thetis PC recorder is unavailable"; return null; }
        public bool PlayFileViaWDSP(string id,string file,int rx,out string error,float gain=1f,bool ignoreChanges=false) { error="FLEX-5000 P24: Thetis WDSP playback is unavailable"; return false; }
        public bool PlayFileViaPCAudio(string id,string file,int device,out string error) { error="FLEX-5000 P24: Thetis PC playback is unavailable"; return false; }
    }

    public sealed class SpecHPSDR
    {
        public SpecHPSDR(int d) { }
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
        public int LowFreq { get; private set; }
        public int HighFreq { get; private set; }
        public void ZoomToBandwidth(double hz) { }
        public (int,int) GetFrequencyExtents(double z,double p) { return (LowFreq,HighFreq); }
        public void initAnalyzer() { }
        public void resetPixelBuffers() { }
    }

    public static unsafe class SpecHPSDRDLL
    {
        public static void GetPixels(int disp,int pixout,float* pix,ref int flag) { flag=0; }
    }
}
