// Exact compatibility types copied from Thetis commit 852bf0ef0b4f3886a13fc2846489aee16f361872
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PowerSDR
{
public enum HPSDRModel
    {
        //IMPORTANT: Please keep the int value order on these enums, ie add new items before LAST,
        //otherwise 'bad' things might happen
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
        ANAN_G2,        //G8NJJ
        ANAN_G2_1K,     //G8NJJ
        ANVELINAPRO3,
        HERMESLITE,     //MI0BOT
        REDPITAYA,      //DH1KLM
        ANAN_G2E,       //N1GP G2E added
        LAST
    }

public enum SquelchState
    {
        OFF = 0,
        SQL = 1,
        VSQL = 2,
        LAST
    }

public enum OtherButtonId
    {
        POWER = 0,
        RX_2,
        MON,
        TUN,
        MOX,
        TWOTON,
        DUP,
        PS_A,
        XPA,
        REC,
        PLAY,
        NR,
        ANF,
        NB,
        SNB,
        MNF,
        MNF_PLUS,
        SPLT,
        A_TO_B,
        ZERO_BEAT,
        B_TO_A,
        IF_TO_V,
        SWAP_AB,
        AVG,
        PEAK_HOLD,
        CTUN,
        VAC1,
        VAC2,
        MUTE,
        BIN,
        SUBRX,
        PAN_SWAP,
        NR1,
        NR2,
        NR3,
        NR4,
        NB1,
        NB2,
        SPECTRUM,
        PANADAPTER,
        SCOPE,
        SCOPE2,
        PHASE,
        WATERFALL,
        HISTOGRAM,
        PANAFALL,
        PANASCOPE,
        SPECTRASCOPE,
        DISPLAY_OFF,
        PAUSE,
        PEAK_BLOBS,
        CURSOR_INFO,
        SPOTS,
        FILL_SPECTRUM,
        SQL,
        SQL_SQL,
        SQL_VSQL,
        RIT,
        RIT0,
        XIT,
        XIT0,
        MIC,
        COMP,
        VOX,
        DEXP,
        RX_EQ,
        TX_EQ,
        TX_FILTER,
        CFC,
        CFC_EQ,
        LEVELER,
        PHASE_ROT,
        AGC_FIXED,
        AGC_LONG,
        AGC_SLOW,
        AGC_MEDIUM,
        AGC_FAST,
        AGC_CUSTOM,
        AGC_AUTO,
        DITHER,
        RANDOM,
        SR_48000,
        SR_96000,
        SR_192000,
        SR_384000,
        SR_768000,
        SR_1536000,
        ATT_STEP,
        ATT_0,
        ATT_10,
        ATT_20,
        ATT_30,
        ATT_40,
        ATT_50,
        ATT_P1,
        ATT_M1,
        PAN_P5,
        PAN_M5,
        PAN_CENTRE,
        ZTB,
        ZOOM_0P5,
        ZOOM_1,
        ZOOM_2,
        ZOOM_4,
        MUTE_ALL,
        MAF_P5,
        MAF_M5,
        AF_P5,
        AF_M5,
        BAL_P5,
        BAL_M5,
        SAF_P5,
        SAF_M5,
        SBAL_P5,
        SBAL_M5,
        SQL_P5,
        SQL_M5,
        MIC_P1,
        MIC_M1,
        COMP_P1,
        COMP_M1,
        VOX_P1,
        VOX_M1,
        AGC_P5,
        AGC_M5,
        DRIVE_P5,
        DRIVE_M5,
        TUNE_P5,
        TUNE_M5,
        DRIVE_0,
        TUN_0,
        FORM_SETUP,
        FORM_DBMAN,
        FORM_MEMORY,
        FORM_AMPVIEW,
        FORM_EQ,
        FORM_XVTR,
        FORM_CWX,
        FORM_DIVERSITY,
        FORM_LINEARITY,
        FORM_WB,
        CWX_KEY,
        CWX_STOP,
        CWX_F1,
        CWX_F2,
        CWX_F3,
        CWX_F4,
        CWX_F5,
        CWX_F6,
        CWX_F7,
        CWX_F8,
        CWX_F9,
        VFO_SYNC,
        LOCK_A,
        LOCK_B,
        TUNE_STEP_U,
        TUNE_STEP_D,
        STACK_U,
        STACK_D,
        NF,
        WAVE_RECORD,
        ACTITVE_PEAK,

        _MACRO_0,
        _MACRO_1,
        _MACRO_2,
        _MACRO_3,
        _MACRO_4,
        _MACRO_5,
        _MACRO_6,
        _MACRO_7,
        _MACRO_8,
        _MACRO_9,
        _MACRO_10,
        _MACRO_11,
        _MACRO_12,
        _MACRO_13,
        _MACRO_14,
        _MACRO_15,
        _MACRO_16,
        _MACRO_17,
        _MACRO_18,
        _MACRO_19,
        _MACRO_20,
        _MACRO_21,
        _MACRO_22,
        _MACRO_23,
        _MACRO_24,
        _MACRO_25,
        _MACRO_26,
        _MACRO_27,
        _MACRO_28,
        _MACRO_29,
        _MACRO_30,

        INFO_TEXT = 998,
        SPLITTER = 999,

        INIT = 1000,

        UNKNOWN = 2000
    }

public class OtherButtonMacroSettings
    {
        public enum OB_ButtonState
        {
            OFF = 0,
            ON,
            TOGGLE,
            LED,
            CONT_VIS,
            CAT
        }

        private int _number;
        private string _on_text;
        private string _off_text;
        private string _notes;
        private bool _closes_parent;
        private bool[] _closes_container = new bool[4];
        private bool[] _opens_container = new bool[4];
        private string[] _close_container_id = new string[4];
        private string[] _open_container_id = new string[4];
        private bool[] _open_uses_location = new bool[4];
        private bool[] _send_via_mmio = new bool[4];
        private string[] _mmio_4char = new string[4];
        private string[] _mmio_message_on = new string[4];
        private string[] _mmio_message_off = new string[4];
        private OB_ButtonState _buttonstate_type;
        private string _led_indicator_four_char;
        private string _buttonstate_container_visible_id;
        private string _buttonstate_cat_on_reply;
        private bool _run_state_command_on_visible;
        private bool[] _cat_macro_send = new bool[1]; //0=to thetis, future proofing using array
        private bool _on_state;
        private string _cat_macro;

        //private static OtherButtonMacroSettings deep_clone(OtherButtonMacroSettings source)
        //{
        //    if (source == null) return null;
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    formatter.Serialize(ms, source);
        //    ms.Position = 0;
        //    object deserialized = formatter.Deserialize(ms);
        //    return (OtherButtonMacroSettings)deserialized;
        //}

        private static T deep_clone<T>(T obj)
        {
            if (!typeof(T).IsSerializable) throw new InvalidOperationException("Type must be serializable");
            using (MemoryStream memory_stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memory_stream, obj);
                memory_stream.Position = 0;
                object deserialized = formatter.Deserialize(memory_stream);
                return (T)deserialized;
            }
        }

        public OtherButtonMacroSettings(OtherButtonMacroSettings settings)
        {
            if (settings != null)
            {
                OtherButtonMacroSettings deep = deep_clone(settings);
                MemberInfo[] members = FormatterServices.GetSerializableMembers(typeof(OtherButtonMacroSettings));
                object[] data = FormatterServices.GetObjectData(deep, members);
                FormatterServices.PopulateObjectMembers(this, members, data);

                //OtherButtonMacroSettings clone = deep_clone(settings);
                //_number = clone._number;
                //_on_text = clone._on_text;
                //_off_text = clone._off_text;
                //_notes = clone._notes;
                //_closes_parent = clone._closes_parent;
                //_closes_container = clone._closes_container;
                //_opens_container = clone._opens_container;
                //_close_container_id = clone._close_container_id;
                //_open_container_id = clone._open_container_id;
                //_open_uses_location = clone._open_uses_location;
                //_send_via_mmio = clone._send_via_mmio;
                //_mmio_4char = clone._mmio_4char;
                //_mmio_message_on = clone._mmio_message_on;
                //_mmio_message_off = clone._mmio_message_off;
                //_buttonstate_type = clone._buttonstate_type;
                //_led_indicator_four_char = clone._led_indicator_four_char;
                //_buttonstate_container_visible_id = clone._buttonstate_container_visible_id;
                //_buttonstate_cat_on_reply = clone._buttonstate_cat_on_reply;
                //_cat_macro_send = clone._cat_macro_send;
                //_on_state = clone._on_state;
                //_cat_macro = clone._cat_macro;
                //_run_state_command_on_visible = clone._run_state_command_on_visible;
            }
        }

        public OtherButtonMacroSettings()
        {
            _buttonstate_type = OB_ButtonState.OFF;
        }

        public int Number
        {
            get { return _number; }
            set { _number = value; }
        }
        public string OnText
        {
            get { return _on_text; }
            set { _on_text = value; }
        }
        public string OffText
        {
            get { return _off_text; }
            set { _off_text = value; }
        }
        public string Notes
        {
            get { return _notes; }
            set { _notes = value; }
        }
        public bool ClosesParent
        {
            get { return _closes_parent; }
            set { _closes_parent = value; }
        }
        public bool[] ClosesContainer
        {
            get { return _closes_container; }
        }
        public bool[] OpensContainer
        {
            get { return _opens_container; }
        }
        public string[] CloseContainerID
        {
            get
            {
                return _close_container_id;
            }
        }
        public string[] OpenContainerID
        {
            get
            {
                return _open_container_id;
            }
        }
        public bool[] OpenUsesLocation
        {
            get
            {
                return _open_uses_location;
            }
        }
        public bool[] SendsViaMMIO
        {
            get
            {
                return _send_via_mmio;
            }
        }
        public string[] MMICFourChar
        {
            get
            {
                return _mmio_4char;
            }
        }
        public string[] MMIOMessageON
        {
            get
            {
                return _mmio_message_on;
            }
        }
        public string[] MMIOMessageOFF
        {
            get
            {
                return _mmio_message_off;
            }
        }
        public OB_ButtonState ButtonStateType
        {
            get
            {
                return _buttonstate_type;
            }
            set
            {
                _buttonstate_type = value;
            }
        }
        public string LedIndiciatorFourChar
        {
            get
            {
                return _led_indicator_four_char;
            }
            set
            {
                _led_indicator_four_char = value;
            }
        }
        public string ContainerVisibleID
        {
            get
            {
                return _buttonstate_container_visible_id;
            }
            set
            {
                _buttonstate_container_visible_id = value;
            }
        }
        public string ButtonStateCatReply
        {
            get
            {
                return _buttonstate_cat_on_reply;
            }
            set
            {
                _buttonstate_cat_on_reply = value;
            }
        }
        public bool[] CatMacroSend
        {
            get
            {
                return _cat_macro_send;
            }
        }
        public bool RunStateCommandOnVisible
        {
            get
            {
                return _run_state_command_on_visible;
            }
            set
            {
                _run_state_command_on_visible = value;
            }
        }
        public bool OnState
        {
            get
            {
                return _on_state;
            }
            set
            {
                _on_state = value;
            }
        }
        public string CatMacro
        {
            get { return _cat_macro; }
            set { _cat_macro = value; }
        }
    }
}