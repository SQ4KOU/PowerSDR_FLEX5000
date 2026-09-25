using System;

namespace PowerSDR
{
    internal sealed class P24DisplayAdaptorInfo
    {
        public int VendorId;
        public int DeviceId;
    }

    internal enum P24HPSDRModel
    {
        FIRST=-1,HPSDR,HERMES,ANAN10,ANAN10E,ANAN100,ANAN100B,ANAN100D,ANAN200D,
        ORIONMKII,ANAN7000D,ANAN8000D,ANAN_G2,ANAN_G2_1K,ANVELINAPRO3,HERMESLITE,
        REDPITAYA,ANAN_G2E,LAST
    }

    internal enum P24SquelchState
    {
        OFF=0, SQL=1, VSQL=2, LAST
    }

    internal enum P24OtherButtonId
    {
        POWER=0,RX_2,MON,TUN,MOX,TWOTON,DUP,PS_A,XPA,REC,PLAY,NR,ANF,NB,SNB,MNF,MNF_PLUS,
        SPLT,A_TO_B,ZERO_BEAT,B_TO_A,IF_TO_V,SWAP_AB,AVG,PEAK_HOLD,CTUN,VAC1,VAC2,MUTE,BIN,
        SUBRX,PAN_SWAP,NR1,NR2,NR3,NR4,NB1,NB2,SPECTRUM,PANADAPTER,SCOPE,SCOPE2,PHASE,
        WATERFALL,HISTOGRAM,PANAFALL,PANASCOPE,SPECTRASCOPE,DISPLAY_OFF,PAUSE,PEAK_BLOBS,
        CURSOR_INFO,SPOTS,FILL_SPECTRUM,SQL,SQL_SQL,SQL_VSQL,RIT,RIT0,XIT,XIT0,MIC,COMP,VOX,
        DEXP,RX_EQ,TX_EQ,TX_FILTER,CFC,CFC_EQ,LEVELER,PHASE_ROT,AGC_FIXED,AGC_LONG,AGC_SLOW,
        AGC_MEDIUM,AGC_FAST,AGC_CUSTOM,AGC_AUTO,DITHER,RANDOM,SR_48000,SR_96000,SR_192000,
        SR_384000,SR_768000,SR_1536000,ATT_STEP,ATT_0,ATT_10,ATT_20,ATT_30,ATT_40,ATT_50,
        ATT_P1,ATT_M1,PAN_P5,PAN_M5,PAN_CENTRE,ZTB,ZOOM_0P5,ZOOM_1,ZOOM_2,ZOOM_4,MUTE_ALL,
        MAF_P5,MAF_M5,AF_P5,AF_M5,BAL_P5,BAL_M5,SAF_P5,SAF_M5,SBAL_P5,SBAL_M5,SQL_P5,
        SQL_M5,MIC_P1,MIC_M1,COMP_P1,COMP_M1,VOX_P1,VOX_M1,AGC_P5,AGC_M5,DRIVE_P5,DRIVE_M5,
        TUNE_P5,TUNE_M5,DRIVE_0,TUN_0,FORM_SETUP,FORM_DBMAN,FORM_MEMORY,FORM_AMPVIEW,FORM_EQ,
        FORM_XVTR,FORM_CWX,FORM_DIVERSITY,FORM_LINEARITY,FORM_WB,CWX_KEY,CWX_STOP,CWX_F1,CWX_F2,
        CWX_F3,CWX_F4,CWX_F5,CWX_F6,CWX_F7,CWX_F8,CWX_F9,VFO_SYNC,LOCK_A,LOCK_B,TUNE_STEP_U,
        TUNE_STEP_D,STACK_U,STACK_D,NF,WAVE_RECORD,ACTITVE_PEAK,
        _MACRO_0,_MACRO_1,_MACRO_2,_MACRO_3,_MACRO_4,_MACRO_5,_MACRO_6,_MACRO_7,_MACRO_8,
        _MACRO_9,_MACRO_10,_MACRO_11,_MACRO_12,_MACRO_13,_MACRO_14,_MACRO_15,_MACRO_16,
        _MACRO_17,_MACRO_18,_MACRO_19,_MACRO_20,_MACRO_21,_MACRO_22,_MACRO_23,_MACRO_24,
        _MACRO_25,_MACRO_26,_MACRO_27,_MACRO_28,_MACRO_29,_MACRO_30,
        INFO_TEXT=998,SPLITTER=999,INIT=1000,UNKNOWN=2000
    }

    [Serializable]
    internal sealed class P24OtherButtonMacroSettings
    {
        internal enum OB_ButtonState { OFF=0, ON, TOGGLE, LED, CONT_VIS, CAT }
        public int Number { get; set; }
        public string OnText { get; set; }
        public string OffText { get; set; }
        public string Notes { get; set; }
        public bool ClosesParent { get; set; }
        public OB_ButtonState ButtonStateType { get; set; }
        public string LedIndicatorFourChar { get; set; }
        public string ContainerVisibleID { get; set; }
        public string ButtonStateCatOnReply { get; set; }
        public bool RunStateCommandOnVisible { get; set; }
        public bool OnState { get; set; }
        public string CatMacro { get; set; }
        public bool[] ClosesContainer { get; set; }
        public bool[] OpensContainer { get; set; }
        public string[] CloseContainerID { get; set; }
        public string[] OpenContainerID { get; set; }
        public bool[] OpenUsesLocation { get; set; }
        public bool[] SendViaMMIO { get; set; }
        public string[] MMIO4Char { get; set; }
        public string[] MMIOMessageOn { get; set; }
        public string[] MMIOMessageOff { get; set; }
        public bool[] CatMacroSend { get; set; }

        public P24OtherButtonMacroSettings()
        {
            OnText="";OffText="";Notes="";LedIndicatorFourChar="";ContainerVisibleID="";
            ButtonStateCatOnReply="";CatMacro="";
            ClosesContainer=new bool[4];OpensContainer=new bool[4];
            CloseContainerID=new string[4];OpenContainerID=new string[4];
            OpenUsesLocation=new bool[4];SendViaMMIO=new bool[4];
            MMIO4Char=new string[4];MMIOMessageOn=new string[4];MMIOMessageOff=new string[4];
            CatMacroSend=new bool[1];
        }
        public P24OtherButtonMacroSettings(P24OtherButtonMacroSettings other) : this()
        {
            if(other==null)return;
            Number=other.Number;OnText=other.OnText;OffText=other.OffText;Notes=other.Notes;
            ClosesParent=other.ClosesParent;ButtonStateType=other.ButtonStateType;
            LedIndicatorFourChar=other.LedIndicatorFourChar;ContainerVisibleID=other.ContainerVisibleID;
            ButtonStateCatOnReply=other.ButtonStateCatOnReply;RunStateCommandOnVisible=other.RunStateCommandOnVisible;
            OnState=other.OnState;CatMacro=other.CatMacro;
        }
    }

    internal sealed class P24RecordingJsonModel
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

    internal sealed class P24SpecHPSDR
    {
        public P24SpecHPSDR(int display) { LowFreq=-24000; HighFreq=24000; SampleRate=48000; }
        public bool Update { get; set; }
        public int FrameRate { get; set; }
        public int PixelOut { get; set; }
        public bool IgnoreFrequencyOffset { get; set; }
        public int Pixels { get; set; }
        public int DetTypePan { get; set; }
        public double AvTau { get; set; }
        public int FFTSize { get; set; }
        public int BlockSize { get; set; }
        public int SampleRate { get; set; }
        public int WindowType { get; set; }
        public int AverageMode { get; set; }
        public bool AverageOn { get; set; }
        public bool NormOneHzPan { get; set; }
        public double PanSlider { get; set; }
        public int LowFreq { get; set; }
        public int HighFreq { get; set; }
        public void initAnalyzer() {}
        public void resetPixelBuffers() {}
        public void ZoomToBandwidth(double bandwidth)
        {
            int half=(int)Math.Max(1, bandwidth/2.0);
            LowFreq=-half; HighFreq=half;
        }
        public (int,int) GetFrequencyExtents(double zoom, double pan) { return (LowFreq,HighFreq); }
    }
}
