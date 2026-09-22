// P24 compatibility types extracted from Thetis 852bf0ef0b4f3886a13fc2846489aee16f361872
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

public static class OtherButtonIdHelpers
    {
        public const int MAX_BITFIELD_GROUP = 12; // the last bitfield group + 1 in CheckBoxData
        public const int MACRO_BUTTONS_PERGROUP = 31;

        private static Dictionary<OtherButtonId, int> _id_to_index_map = new Dictionary<OtherButtonId, int>();
        private static Dictionary<(int, int), int> _bit_group_bit_to_index_map = new Dictionary<(int, int), int>();

        public static readonly (OtherButtonId id, int bit_group, int bit_number, string caption, string icon_on, string icon_off, string tool_tip)[] CheckBoxData =
        new (OtherButtonId, int, int, string, string, string, string)[]
        {
            (OtherButtonId.INFO_TEXT,     -1, -1, "General", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.POWER,          0,  0, "Power", "power", "", "Power on/off"),
            (OtherButtonId.RX_2,           0,  1, "RX 2", "", "", "Enable RX2"),
            (OtherButtonId.MON,            0,  2, "MON", "", "", "Monitor your tx audio"),
            (OtherButtonId.TUN,            0,  3, "TUN", "", "", "Put radio into tune"),
            (OtherButtonId.MOX,            0,  4, "MOX", "", "", "Put radio into mox"),
            (OtherButtonId.TWOTON,         0,  5, "2TON", "", "", "Put out a two tone signal"),
            (OtherButtonId.DUP,            0,  6, "DUP", "", "", "Duplex mode, view the tx rx"),
            (OtherButtonId.PS_A,           0,  7, "PS-A", "", "", "Puresignal auto"),
            (OtherButtonId.XPA,            0,  8, "xPA", "", "", "Override OC pins with this"),
            (OtherButtonId.REC,            0,  9, "Rec", "stop", "record", "Wave quick record"),
            (OtherButtonId.PLAY,           0, 10, "Play", "stop", "play", "Wave quick playback"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "Noise / ATT", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.NR,             1, 0, "NR", "", "", "Cycle through NR's"),
            (OtherButtonId.NR1,            1, 1, "NR1", "", "", "NR1 on/off"),
            (OtherButtonId.NR2,            1, 2, "NR2", "", "", "NR2 on/off"),
            (OtherButtonId.NR3,            1, 3, "NR3", "", "", "NR3 on/off"),
            (OtherButtonId.NR4,            1, 4, "NR4", "", "", "NR4 on/off"),
            (OtherButtonId.ANF,            1, 5, "ANF", "", "", "Auto notch filter"),
            (OtherButtonId.NB,             1, 6, "NB", "", "", "Cycle through noise blankers"),
            (OtherButtonId.NB1,            1, 7, "NB1", "", "", "NB1 on/off"),
            (OtherButtonId.NB2,            1, 8, "NB2", "", "", "NB2 on/off"),
            (OtherButtonId.SNB,            1, 9, "SNB", "", "", "Enabled / disable spectral noise blanker"),
            (OtherButtonId.MNF,            1, 10, "MNF", "", "", "Manual not filters on/off"),
            (OtherButtonId.MNF_PLUS,       1, 11, "MNF+", "", "", "Add a manual notch"),
            (OtherButtonId.ATT_STEP,       1, 12, "S-ATT", "", "", "Step attenuator on/off"),
            (OtherButtonId.ATT_0,          1, 13, "ATT-0", "", "", "0dB attenuation"),
            (OtherButtonId.ATT_10,         1, 14, "ATT-10", "", "", "10dB attenuation"),
            (OtherButtonId.ATT_20,         1, 15, "ATT-20", "", "", "20db attenuation"),
            (OtherButtonId.ATT_30,         1, 16, "ATT-30", "", "", "30db attenuation"),
            (OtherButtonId.ATT_40,         1, 17, "ATT-40", "", "", "40db attenuation"),
            (OtherButtonId.ATT_50,         1, 18, "ATT-50", "", "", "50db attenuation"),
            (OtherButtonId.ATT_P1,         1, 19, "ATT+", "", "", "Increaase attenuation"),
            (OtherButtonId.ATT_M1,         1, 20, "ATT-", "", "", "Decrease attenuation"),
            (OtherButtonId.NF,             1, 21, "NF", "", "", "Noise floor display on/off"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "VFOs", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.SPLT,           2, 0, "Split", "", "", "Split on/off"),
            (OtherButtonId.A_TO_B,         2, 1, "A > B", "", "", "Copy VFOa frequency to VFOb"),
            (OtherButtonId.ZERO_BEAT,      2, 2, "0 Beat", "", "", "0 beat to strongest signal"),
            (OtherButtonId.B_TO_A,         2, 3, "A < B", "", "", "Copy VFOa frequency to VFOb"),
            (OtherButtonId.IF_TO_V,        2, 4, "IF > V", "", "", "IF to the VFO"),
            (OtherButtonId.SWAP_AB,        2, 5, "A <> B", "", "", "Swap VFOa and VFOb frequencies"),
            (OtherButtonId.RIT,            2, 6, "RIT", "", "", "RIT on/off"),
            (OtherButtonId.RIT0,           2, 7, "RIT0", "", "", "0 rit"),
            (OtherButtonId.XIT,            2, 8, "XIT", "", "", "XIT on/off"),
            (OtherButtonId.XIT0,           2, 9, "XIT0", "", "", "0 rit"),
            (OtherButtonId.VFO_SYNC,       2, 10, "SYNC", "", "", "Sync the vfos"),
            (OtherButtonId.LOCK_A,         2, 11, "LockA", "", "", "Lock VFOa"),
            (OtherButtonId.LOCK_B,         2, 12, "LockB", "", "", "Lock VFOb"),
            (OtherButtonId.TUNE_STEP_U,    2, 13, "TS+", "", "", "Increase tunestep"),
            (OtherButtonId.TUNE_STEP_D,    2, 14, "TS-", "", "", "Decrease tunestep"),
            (OtherButtonId.STACK_U,        2, 15, "STACK+", "", "", "Move up one bandstack entry"),
            (OtherButtonId.STACK_D,        2, 16, "STACK-", "", "", "Move down on bandstack entry"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "Display", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.AVG,            3, 0, "Avg", "", "", "Display averaging on/off"),
            (OtherButtonId.PEAK_HOLD,           3, 1, "Peak", "", "", "Peak hold on/off"),
            (OtherButtonId.CTUN,           3, 2, "CTUN", "", "", "ClickTune on/off"),
            (OtherButtonId.SPECTRUM,       3, 3, "Spectrum", "spectrum", "", "Spectrum"),
            (OtherButtonId.PANADAPTER,     3, 4, "Panadapter", "panadapter", "", "Panadaptor"),
            (OtherButtonId.SCOPE,          3, 5, "Scope", "scope", "", "Scope"),
            (OtherButtonId.SCOPE2,         3, 6, "Scope2", "scope2", "", "Scope2"),
            (OtherButtonId.PHASE,          3, 7, "Phase", "phase", "", "Phase"),
            (OtherButtonId.WATERFALL,      3, 8, "Waterfall", "waterfall", "", "Waterfall"),
            (OtherButtonId.HISTOGRAM,      3, 9, "Histogram", "histogram", "", "Histogram"),
            (OtherButtonId.PANAFALL,       3, 10, "Panafall", "panafall", "", "Panafall"),
            (OtherButtonId.PANASCOPE,      3, 11, "Panascope", "panascope", "", "Panascope"),
            (OtherButtonId.SPECTRASCOPE,   3, 12, "Spectrascope", "spectrascope", "", "Spectrascope"),
            (OtherButtonId.DISPLAY_OFF,    3, 13, "Off", "display_off", "", "Display off"),
            (OtherButtonId.PAUSE,          3, 14, "Pause", "disp_pause", "disp_play", "Pause the display"),
            (OtherButtonId.PEAK_BLOBS,     3, 15, "Peak Blobs", "", "", "Peak blobs on/off"),
            (OtherButtonId.CURSOR_INFO,    3, 16, "Cur Info", "", "", "Show info on the cursor"),
            (OtherButtonId.SPOTS,          3, 17, "Spots", "", "", "Show TCI spots"),
            (OtherButtonId.FILL_SPECTRUM,  3, 18, "Fill", "", "", "Fill the panadaptor area"),
            (OtherButtonId.PAN_P5,         3, 19, "PAN+", "", "", "Pan the display to the right"),
            (OtherButtonId.PAN_M5,         3, 20, "PAN-", "", "", "Pan the display to the left"),
            (OtherButtonId.PAN_CENTRE,     3, 21, "Centre", "", "", "Centre the display"),
            (OtherButtonId.ZTB,            3, 22, "ZTB", "", "", "Zoom to band"),
            (OtherButtonId.ZOOM_0P5,       3, 23, "0.5x", "", "", "0.5x zoom"),
            (OtherButtonId.ZOOM_1,         3, 24, "1x", "", "", "1x zoom"),
            (OtherButtonId.ZOOM_2,         3, 25, "2x", "", "", "2x zoom"),
            (OtherButtonId.ZOOM_4,         3, 26, "4x", "", "", "4x zoom"),
            (OtherButtonId.ACTITVE_PEAK,   3, 27, "A-Peak", "", "", "Active Peak"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "Audio / DSP", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.VAC1,           4, 0, "Vac1", "", "", "Vac1 on/off"),
            (OtherButtonId.VAC2,           4, 1, "Vac2", "", "", "Vac2 on/off"),
            (OtherButtonId.MUTE,           4, 2, "Mute", "mute_on", "mute_off", "Mute on/off"),
            (OtherButtonId.MUTE_ALL,       4, 3, "Mute All", "mute_all_on", "mute_all_off", "Mute all receivers on/off"),
            (OtherButtonId.BIN,            4, 4, "Bin", "", "", "Binaurial mode"),
            (OtherButtonId.SUBRX,          4, 5, "SubRX", "", "", "Enable the sub receiver for rx1"),
            (OtherButtonId.PAN_SWAP,       4, 6, "SwapLR", "", "", "Swap left and right speakers"),
            (OtherButtonId.SQL,            4, 7, "SQL", "", "", "Enable squeltch"),
            (OtherButtonId.SQL_SQL,        4, 8, "Reg SQL", "", "", "Normal squeltch"),
            (OtherButtonId.SQL_VSQL,       4, 9, "Voice SQL", "", "", "Voice base squeltch"),
            (OtherButtonId.SQL_P5,         4, 10, "SQL+", "", "", "Increase squelch sensitivity"),
            (OtherButtonId.SQL_M5,         4, 11, "SQL-", "", "", "Decrease squelch sensitivity"),
            (OtherButtonId.MIC,            4, 12, "MIC", "mic_on", "mic_off", "Mic on/off"),
            (OtherButtonId.COMP,           4, 13, "COMP", "", "", "Compressor on/off"),
            (OtherButtonId.VOX,            4, 14, "VOX", "", "", "Vox on/off"),
            (OtherButtonId.DEXP,           4, 15, "DEXP", "", "", "Downward expander on/off"),
            (OtherButtonId.RX_EQ,          4, 16, "RX EQ", "", "", "Receiver EQ on/off"),
            (OtherButtonId.TX_EQ,          4, 17, "TX EQ", "", "", "Transmit EQ on/off"),
            (OtherButtonId.TX_FILTER,      4, 18, "TX Filter", "", "", "Show transmit filter"),
            (OtherButtonId.CFC,            4, 19, "CFC", "", "", "Coninuous frequency compression on/off"),
            (OtherButtonId.CFC_EQ,         4, 20, "CFC EQ", "", "", "CFC eq on/off"),
            (OtherButtonId.MAF_P5,         4, 21, "MAF+", "", "", "Increase Master AF"),
            (OtherButtonId.MAF_M5,         4, 22, "MAF-", "", "", "Decrease Master AF"),
            (OtherButtonId.AF_P5,          4, 23, "AF+", "", "", "Increase rx AF"),
            (OtherButtonId.AF_M5,          4, 24, "AF-", "", "", "Decrease rx AF"),
            (OtherButtonId.BAL_P5,         4, 25, "BAL+", "", "", "Pan audio right"),
            (OtherButtonId.BAL_M5,         4, 26, "BAL-", "", "", "Pan audio left"),
            (OtherButtonId.SAF_P5,         4, 27, "SAF+", "", "", "Increase subrx AF"),
            (OtherButtonId.SAF_M5,         4, 28, "SAF-", "", "", "Decrease subrx AF"),
            (OtherButtonId.SBAL_P5,        4, 29, "SBAL+", "", "", "Pan subrx audio right"),
            (OtherButtonId.SBAL_M5,        4, 30, "SBAL-", "", "", "Pan subrx audio left"),
            (OtherButtonId.MIC_P1,         5, 0, "MIC+", "", "", "Increase mic gain"),
            (OtherButtonId.MIC_M1,         5, 1, "MIC-", "", "", "Decrease mic gain"),
            (OtherButtonId.COMP_P1,        5, 3, "COMP+", "", "", "Increase compression"),
            (OtherButtonId.COMP_M1,        5, 4, "COMP-", "", "", "Decrease compression"),
            (OtherButtonId.VOX_P1,         5, 5, "VOX+", "", "", "Increase vox level"),
            (OtherButtonId.VOX_M1,         5, 6, "VOX-", "", "", "Decrease vox level"),
            (OtherButtonId.LEVELER,        5, 7, "Leveler", "", "", "Leveler on/off"),
            (OtherButtonId.PHASE_ROT,      5, 8, "PhaRot", "", "", "Phase rotator on/off"),
            (OtherButtonId.WAVE_RECORD,    5, 9, "Wav Record", "stop", "record", "Wave record"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "AGC", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.AGC_FIXED,      6, 0, "FIX", "", "", "Fixed agc"),
            (OtherButtonId.AGC_LONG,       6, 1, "LONG", "", "", "Long agc"),
            (OtherButtonId.AGC_SLOW,       6, 2, "SLOW", "", "", "Slow agc"),
            (OtherButtonId.AGC_MEDIUM,     6, 3, "MED", "", "", "Medium agc"),
            (OtherButtonId.AGC_FAST,       6, 4, "FAST", "", "", "Fast agc"),
            (OtherButtonId.AGC_CUSTOM,     6, 5, "CUST", "", "", "Custom agc"),
            (OtherButtonId.AGC_AUTO,       6, 6, "AUTO", "", "", "Auto adjust agc based on noise floor"),
            (OtherButtonId.AGC_P5,         6, 7, "AGC+", "", "", "Increase gain"),
            (OtherButtonId.AGC_M5,         6, 8, "AGC-", "", "", "Decrease gain"),
            (OtherButtonId.INFO_TEXT,     -1, -1, "Hardware", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.DITHER,         7, 0, "Dither", "", "", "Enable fpga dither on/off"),
            (OtherButtonId.RANDOM,         7, 1, "Random", "", "", "Enable fpga random on/off"),
            (OtherButtonId.SR_48000,       7, 2, "48k", "", "", "48k hardware sample rate"),
            (OtherButtonId.SR_96000,       7, 3, "96k", "", "", "96k hardware sample rate"),
            (OtherButtonId.SR_192000,      7, 4, "192k", "", "", "192k hardware sample rate"),
            (OtherButtonId.SR_384000,      7, 5, "384k", "", "", "384k hardware sample rate"),
            (OtherButtonId.SR_768000,      7, 6, "768k", "", "", "768k hardware sample rate"),
            (OtherButtonId.SR_1536000,     7, 7, "1536k", "", "", "1536k hardware sample rate"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "RF Power", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.DRIVE_P5,       8, 0, "DRI+", "", "", "Increase drive"),
            (OtherButtonId.DRIVE_M5,       8, 1, "DRI-", "", "", "Decrease drive"),
            (OtherButtonId.TUNE_P5,        8, 2, "TUN+", "", "", "Increase tune power"),
            (OtherButtonId.TUNE_M5,        8, 3, "TUN-", "", "", "Decrease tune power"),
            (OtherButtonId.DRIVE_0,        8, 4, "DRI-0", "", "", "Set drive power to 0"),
            (OtherButtonId.TUN_0,          8, 5, "TUN-0", "", "", "Set tune power to 0"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "Forms", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.FORM_SETUP,     9, 0, "Setup", "", "", "Show setup"),
            (OtherButtonId.FORM_DBMAN,     9, 1, "DB Man", "", "", "Show database manager"),
            (OtherButtonId.FORM_MEMORY,    9, 2, "Memory", "", "", "Show memory form"),
            (OtherButtonId.FORM_AMPVIEW,   9, 3, "AmpView", "", "", "Show ampview form"),
            (OtherButtonId.FORM_EQ,        9, 4, "Equaliser", "", "", "Show equaliser form"),
            (OtherButtonId.FORM_XVTR,      9, 5, "XVTR", "", "", "Show transverter form"),
            (OtherButtonId.FORM_CWX,       9, 6, "CWX", "", "", "Show CWX form"),
            (OtherButtonId.FORM_DIVERSITY, 9, 8, "Diversity", "", "", "Show diversity form"),
            (OtherButtonId.FORM_LINEARITY, 9, 9, "Linearity", "", "", "Show linearity form"),
            (OtherButtonId.FORM_WB,        9, 10, "Wideband", "", "", "Show wideband"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "CWX", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId.CWX_KEY,        10, 0, "KEY", "", "", "Key cwx"),
            (OtherButtonId.CWX_STOP,       10, 1, "STOP", "", "", "Stop cwx"),
            (OtherButtonId.CWX_F1,         10, 2, "F1", "", "", "Function key 1"),
            (OtherButtonId.CWX_F2,         10, 3, "F2", "", "", "Function key 2"),
            (OtherButtonId.CWX_F3,         10, 4, "F3", "", "", "Function key 3"),
            (OtherButtonId.CWX_F4,         10, 5, "F4", "", "", "Function key 4"),
            (OtherButtonId.CWX_F5,         10, 6, "F5", "", "", "Function key 5"),
            (OtherButtonId.CWX_F6,         10, 7, "F6", "", "", "Function key 6"),
            (OtherButtonId.CWX_F7,         10, 8, "F7", "", "", "Function key 7"),
            (OtherButtonId.CWX_F8,         10, 9, "F8", "", "", "Function key 8"),
            (OtherButtonId.CWX_F9,         10, 10, "F9", "", "", "Function key 9"),

            (OtherButtonId.INFO_TEXT,     -1, -1, "Macros", "", "", ""),
            (OtherButtonId.SPLITTER,      -1, -1, "", "", "", ""),
            (OtherButtonId._MACRO_0,       11, 0, "M1", "", "", "Macro 1"),
            (OtherButtonId._MACRO_1,       11, 1, "M2", "", "", "Macro 2"),
            (OtherButtonId._MACRO_2,       11, 2, "M3", "", "", "Macro 3"),
            (OtherButtonId._MACRO_3,       11, 3, "M4", "", "", "Macro 4"),
            (OtherButtonId._MACRO_4,       11, 4, "M5", "", "", "Macro 5"),
            (OtherButtonId._MACRO_5,       11, 5, "M6", "", "", "Macro 6"),
            (OtherButtonId._MACRO_6,       11, 6, "M7", "", "", "Macro 7"),
            (OtherButtonId._MACRO_7,       11, 7, "M8", "", "", "Macro 8"),
            (OtherButtonId._MACRO_8,       11, 8, "M9", "", "", "Macro 9"),
            (OtherButtonId._MACRO_9,       11, 9, "M10", "", "", "Macro 10"),
            (OtherButtonId._MACRO_10,      11, 10, "M11", "", "", "Macro 11"),
            (OtherButtonId._MACRO_11,      11, 11, "M12", "", "", "Macro 12"),
            (OtherButtonId._MACRO_12,      11, 12, "M13", "", "", "Macro 13"),
            (OtherButtonId._MACRO_13,      11, 13, "M14", "", "", "Macro 14"),
            (OtherButtonId._MACRO_14,      11, 14, "M15", "", "", "Macro 15"),
            (OtherButtonId._MACRO_15,      11, 15, "M16", "", "", "Macro 16"),
            (OtherButtonId._MACRO_16,      11, 16, "M17", "", "", "Macro 17"),
            (OtherButtonId._MACRO_17,      11, 17, "M18", "", "", "Macro 18"),
            (OtherButtonId._MACRO_18,      11, 18, "M19", "", "", "Macro 19"),
            (OtherButtonId._MACRO_19,      11, 19, "M20", "", "", "Macro 20"),
            (OtherButtonId._MACRO_20,      11, 20, "M21", "", "", "Macro 21"),
            (OtherButtonId._MACRO_21,      11, 21, "M22", "", "", "Macro 22"),
            (OtherButtonId._MACRO_22,      11, 22, "M23", "", "", "Macro 23"),
            (OtherButtonId._MACRO_23,      11, 23, "M24", "", "", "Macro 24"),
            (OtherButtonId._MACRO_24,      11, 24, "M25", "", "", "Macro 25"),
            (OtherButtonId._MACRO_25,      11, 25, "M26", "", "", "Macro 26"),
            (OtherButtonId._MACRO_26,      11, 26, "M27", "", "", "Macro 27"),
            (OtherButtonId._MACRO_27,      11, 27, "M28", "", "", "Macro 28"),
            (OtherButtonId._MACRO_28,      11, 28, "M29", "", "", "Macro 29"),
            (OtherButtonId._MACRO_29,      11, 29, "M30", "", "", "Macro 30"),
            (OtherButtonId._MACRO_30,      11, 30, "M31", "", "", "Macro 31")
        };

        static OtherButtonIdHelpers()
        {
            _id_to_index_map = new Dictionary<OtherButtonId, int>(CheckBoxData.Length);
            _bit_group_bit_to_index_map = new Dictionary<(int, int), int>(CheckBoxData.Length);

            for (int i = 0; i < CheckBoxData.Length; i++)
            {
                (OtherButtonId id, int bit_group, int bit_number, string caption, string icon_on, string icon_off, string tooltip) = CheckBoxData[i];

                if (!_id_to_index_map.ContainsKey(id)) _id_to_index_map[id] = i;

                (int, int) key = (bit_group, bit_number);
                if (!_bit_group_bit_to_index_map.ContainsKey(key)) _bit_group_bit_to_index_map[key] = i;
            }

            //checkImplemented();
        }

        public static string OtherButtonIDToText(OtherButtonId id)
        {
            if (_id_to_index_map.ContainsKey(id)) return CheckBoxData[_id_to_index_map[id]].caption;
            return id.ToString();
        }
        public static string OtherButtonIDToIconOn(OtherButtonId id)
        {
            if (_id_to_index_map.ContainsKey(id)) return CheckBoxData[_id_to_index_map[id]].icon_on;
            return string.Empty;
        }
        public static string OtherButtonIDToIconOff(OtherButtonId id)
        {
            if (_id_to_index_map.ContainsKey(id)) return CheckBoxData[_id_to_index_map[id]].icon_off;
            return string.Empty;
        }


        public static OtherButtonId BitToID(int bit_group, int bit_number)
        {
            if (_bit_group_bit_to_index_map.TryGetValue((bit_group, bit_number), out int idx)) return CheckBoxData[idx].id;
            return OtherButtonId.UNKNOWN;
        }

        public static (int bit_group, int bit) BitFromID(OtherButtonId id)
        {
            if (_id_to_index_map.ContainsKey(id)) return (CheckBoxData[_id_to_index_map[id]].bit_group, CheckBoxData[_id_to_index_map[id]].bit_number);
            return (-1, -1);
        }

        public static string BitToText(int bit_group, int bit_number)
        {
            OtherButtonId id = BitToID(bit_group, bit_number);
            return OtherButtonIDToText(id);
        }
        public static (string, string) BitToIcon(int bit_group, int bit_number)
        {
            OtherButtonId id = BitToID(bit_group, bit_number);
            return (OtherButtonIDToIconOn(id), OtherButtonIDToIconOff(id));
        }
        public static string OtherButtonIDToTooltip(OtherButtonId id)
        {
            if (_id_to_index_map.TryGetValue(id, out int idx)) return CheckBoxData[idx].tool_tip;
            return string.Empty;
        }
        private static void checkImplemented()
        {
            List<OtherButtonId> all = new List<OtherButtonId>();

            for(OtherButtonId n = OtherButtonId.POWER; n <= OtherButtonId.UNKNOWN; n++)
            {
                all.Add(n);
            }

            for (int n = 0; n < CheckBoxData.Length; n++)
            {
                (OtherButtonId id, int bit_group, int bit_number, string caption, string icon_on, string icon_off, string tool_tip) d = CheckBoxData[n];
                if (all.Contains(d.id)) all.Remove(d.id);
            }
            
            foreach(OtherButtonId id in all)
            {
                Debug.Print($"MISSED FROM OTHERBUTTONS = {id.ToString()}");
            }
        }
    }

    internal sealed class P24AdaptorInfo
    {
        public int DeviceId { get; set; }
        public int VendorId { get; set; }
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
            public int sample_rate { get; set; }
            public int channels { get; set; }
            public string utc_time { get; set; }
        }
    }
}
