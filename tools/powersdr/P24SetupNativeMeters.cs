// P24 native Thetis Meters/Gadgets setup port
// Generated from ramdor/Thetis commit 852bf0ef0b4f3886a13fc2846489aee16f361872.
// Layout/event behaviour is source-derived; radio backend remains PowerSDR/FLEX-5000.
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PowerSDR
{
    public partial class Setup
    {
        private readonly System.Windows.Forms.Timer tmrLedValid = new System.Windows.Forms.Timer();
        // P24 compatibility members copied verbatim from audited Thetis setup.cs.
        private Font _textOverlayFont1 = null;

        private Font _textOverlayFont2 = null;

        private bool _reset_button_map_layout = false;

        private bool _reset_waverecord_order_map = false;

        private readonly System.Windows.Forms.Timer _recording_keybind_timer = new System.Windows.Forms.Timer();

        private bool _listening_for_recording_keycodes = false;

        private bool _setting_globalkeybind = false;

        private Keys _globalPlayRecordInterrupKeybind = Keys.None;

        private bool _alt_pressed = P24ThetisMeterCompat.AltlKeyDown;

        private bool _shift_pressed = P24ThetisMeterCompat.ShiftKeyDown;

        private bool _ctrl_pressed = P24ThetisMeterCompat.CtrlKeyDown;

        private bool _suppressEvents = false;

        private int _selected_voice_slot = 0;

        private bool _ignore_slot_count = false; // prevent updateItemSettingsControlsForSelected from updating slot count

        KeyValuePair<string, string>[] _hamqsl_urls =
        {
        new KeyValuePair<string, string>("select one", ""),
        new KeyValuePair<string, string>("Layout 1 - sun", "https://www.hamqsl.com/solarn0nbh.php"),
        new KeyValuePair<string, string>("Layout 2 - sun", "https://www.hamqsl.com/solarpic.php"),
        new KeyValuePair<string, string>("Layout 3", "https://www.hamqsl.com/solarvhf.php"),
        new KeyValuePair<string, string>("Layout 4", "https://www.hamqsl.com/solar.php"),
        new KeyValuePair<string, string>("Layout 5", "https://www.hamqsl.com/solarsmall.php"),
        new KeyValuePair<string, string>("Layout 6", "https://www.hamqsl.com/solarbrief.php"),
        new KeyValuePair<string, string>("Layout 7", "https://www.hamqsl.com/solarbc.php"),
        new KeyValuePair<string, string>("Layout 8", "https://www.hamqsl.com/solar100sc.php"),
        new KeyValuePair<string, string>("Layout 9", "https://www.hamqsl.com/solar2.php"),
        new KeyValuePair<string, string>("Layout 10 - sun", "https://www.hamqsl.com/solarpich.php"),
        new KeyValuePair<string, string>("Layout 11 - sun", "https://www.hamqsl.com/solar101pic.php"),
        new KeyValuePair<string, string>("Layout 12", "https://www.hamqsl.com/solar101vhf.php"),
        new KeyValuePair<string, string>("Layout 13", "https://www.hamqsl.com/solar101vhfper.php"),
        new KeyValuePair<string, string>("Layout 14 - sun", "https://www.hamqsl.com/solar101vhfpic.php"),
        new KeyValuePair<string, string>("Layout 15", "https://www.hamqsl.com/solar101sc.php"),
        new KeyValuePair<string, string>("Layout 16 - sun", "https://www.hamqsl.com/solarsun.php"),
        new KeyValuePair<string, string>("Layout 17 - graphs", "https://www.hamqsl.com/solargraph.php"),
        new KeyValuePair<string, string>("Layout 18 - graphs", "https://www.hamqsl.com/marston.php"),
        new KeyValuePair<string, string>("Greyline 1", "https://www.hamqsl.com/solarmuf.php"),
        new KeyValuePair<string, string>("Greyline 2", "https://www.hamqsl.com/solarmap.php"),
        new KeyValuePair<string, string>("Earth 1", "https://www.hamqsl.com/solarglobe.php"),
        new KeyValuePair<string, string>("Earth 2", "https://www.hamqsl.com/moonglobe.php"),
        new KeyValuePair<string, string>("Planets", "https://www.hamqsl.com/solarsystem.php"),
        };

        private KeyValuePair<string, string>[] _bsdworld_urls =
        {
        new KeyValuePair<string, string>("select one", ""),
        new KeyValuePair<string, string>("NA Propagation All", "https://bsdworld.org/DXCC/continent/NA/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("NA Propagation Zone 3", "https://bsdworld.org/DXCC/cqzone/3/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("NA Propagation Zone 4", "https://bsdworld.org/DXCC/cqzone/4/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("NA Propagation Zone 5", "https://bsdworld.org/DXCC/cqzone/5/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("EU Propagation All", "https://bsdworld.org/DXCC/continent/EU/tn_latest<light_mode>.webp"),
        new KeyValuePair<string, string>("EU Propagation Zone 14", "https://bsdworld.org/DXCC/cqzone/14/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("EU Propagation Zone 15", "https://bsdworld.org/DXCC/cqzone/15/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("EU Propagation Zone 16", "https://bsdworld.org/DXCC/cqzone/16/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("EU Propagation Zone 20", "https://bsdworld.org/DXCC/cqzone/20/latest<light_mode>.webp"),
        new KeyValuePair<string, string>("OC Propagation All", "https://bsdworld.org/DXCC/continent/OC/tn_latest<light_mode>.webp"),
        new KeyValuePair<string, string>("AS Propagation All", "https://bsdworld.org/DXCC/continent/AS/tn_latest<light_mode>.webp"),
        new KeyValuePair<string, string>("SA Propagation All", "https://bsdworld.org/DXCC/continent/SA/tn_latest<light_mode>.webp"),
        new KeyValuePair<string, string>("AF Propagation All", "https://bsdworld.org/DXCC/continent/AF/tn_latest<light_mode>.webp"),
        new KeyValuePair<string, string>("A-Index", "https://bsdworld.org/aindex<light_mode>.svgz"),
        new KeyValuePair<string, string>("PK Index", "https://bsdworld.org/pkindex<light_mode>.svgz"),
        new KeyValuePair<string, string>("PK Predictions", "https://bsdworld.org/pki-forecast<light_mode>.svgz"),
        new KeyValuePair<string, string>("Flux", "https://bsdworld.org/flux<light_mode>.svgz"),
        new KeyValuePair<string, string>("Outlook", "https://bsdworld.org/outlook<light_mode>.svgz"),
        new KeyValuePair<string, string>("Solar Wind", "https://bsdworld.org/solarwind<light_mode>.svgz"),
        new KeyValuePair<string, string>("SSN", "https://bsdworld.org/ssn<light_mode>.svgz"),
        new KeyValuePair<string, string>("SSN History", "https://bsdworld.org/ssnhist<light_mode>.svgz"),
        new KeyValuePair<string, string>("EISN", "https://bsdworld.org/eisn<light_mode>.svgz"),
        new KeyValuePair<string, string>("Proton Flux", "https://bsdworld.org/proton_flux<light_mode>.svgz"),
        new KeyValuePair<string, string>("X-Ray Flux", "https://bsdworld.org/xray_flux<light_mode>.svgz"),
        new KeyValuePair<string, string>("D-Layer", "https://bsdworld.org/d-rap/latest<light_mode>.svgz"),
        };

        private KeyValuePair<string, string>[] _nasa_urls =
        {
        new KeyValuePair<string, string>("select one", ""),
        new KeyValuePair<string, string>("SOHO EIT 171", "https://soho.nascom.nasa.gov/data/realtime/eit_171/512/latest.jpg"),
        new KeyValuePair<string, string>("SOHO EIT 195", "https://soho.nascom.nasa.gov/data/realtime/eit_195/512/latest.jpg"),
        new KeyValuePair<string, string>("SOHO EIT 284", "https://soho.nascom.nasa.gov/data/realtime/eit_284/512/latest.jpg"),
        new KeyValuePair<string, string>("SOHO EIT 304", "https://soho.nascom.nasa.gov/data/realtime/eit_304/512/latest.jpg"),
        new KeyValuePair<string, string>("SOHO SDO/HMI Continuum", "https://soho.nascom.nasa.gov/data/realtime/hmi_igr/512/latest.jpg"),
        new KeyValuePair<string, string>("SOHO SDO/HMI Magnetogram", "https://soho.nascom.nasa.gov/data/realtime/hmi_mag/512/latest.jpg"),
        new KeyValuePair<string, string>("SOHO LASCO C2", "https://soho.nascom.nasa.gov/data/realtime/c2/512/latest.jpg"),
        new KeyValuePair<string, string>("SOHO LASCO C3", "https://soho.nascom.nasa.gov/data/realtime/c3/512/latest.jpg")
        };

        private KeyValuePair<string, string>[] _noaa_urls =
        {
        new KeyValuePair<string, string>("select one", ""),
        new KeyValuePair<string, string>("Northern Aurora Latest", "https://services.swpc.noaa.gov/images/animations/ovation/north/latest.jpg"),
        new KeyValuePair<string, string>("Southern Aurora Latest", "https://services.swpc.noaa.gov/images/animations/ovation/south/latest.jpg"),
        new KeyValuePair<string, string>("Northern Aurora Forecast", "https://services.swpc.noaa.gov/images/aurora-forecast-northern-hemisphere.jpg"),
        new KeyValuePair<string, string>("Southern Aurora Forecast", "https://services.swpc.noaa.gov/images/aurora-forecast-southern-hemisphere.jpg"),
        new KeyValuePair<string, string>("SWX Solar Overiew", "https://services.swpc.noaa.gov/images/swx-overview-large.gif"),
        new KeyValuePair<string, string>("K Indicies", "https://services.swpc.noaa.gov/images/station-k-index.png"),
        new KeyValuePair<string, string>("D Region Absorption Map", "https://services.swpc.noaa.gov/images/animations/d-rap/global/d-rap/latest.png")
        };

        private class clsComboHistoryItem
        {
        private string _reading_name;
        private Reading _reading;
        public clsComboHistoryItem(Reading r)
        {
        _reading = r;
        _reading_name = MeterManager.ReadingName(r);
        }
        public Reading Reading
        {
        get { return _reading; }
        }
        public string ReadingName
        {
        get { return _reading_name; }
        }
        public override string ToString()
        {
        return _reading_name;
        }
        }

        private System.Windows.Forms.ButtonTS p24_bntMultiMeterItemRotator_default_pstRotator;
        private System.Windows.Forms.ButtonTS p24_btnAddMeterItem;
        private System.Windows.Forms.ButtonTS p24_btnAddRX1Container;
        private System.Windows.Forms.ButtonTS p24_btnBandButtons_font;
        private System.Windows.Forms.ButtonTS p24_btnContainerDelete;
        private System.Windows.Forms.ButtonTS p24_btnContainer_dupe;
        private System.Windows.Forms.ButtonTS p24_btnContainer_load;
        private System.Windows.Forms.ButtonTS p24_btnContainer_save;
        private System.Windows.Forms.ButtonTS p24_btnFilter_4char_copy;
        private System.Windows.Forms.ButtonTS p24_btnHistory_copy_minmax_from_0;
        private System.Windows.Forms.ButtonTS p24_btnLedIndicatorVarPicker;
        private System.Windows.Forms.ButtonTS p24_btnLedIndicator_4char_copy;
        private System.Windows.Forms.ButtonTS p24_btnLedIndicator_copy_sizex_to_y;
        private System.Windows.Forms.ButtonTS p24_btnLedIndicator_copy_truefalse_colours;
        private System.Windows.Forms.ButtonTS p24_btnMMIO_variable;
        private System.Windows.Forms.ButtonTS p24_btnMMIO_variable_2;
        private System.Windows.Forms.ButtonTS p24_btnMMIO_variable_2_history;
        private System.Windows.Forms.ButtonTS p24_btnMMIO_variable_2_rotator;
        private System.Windows.Forms.ButtonTS p24_btnMMIO_variable_history;
        private System.Windows.Forms.ButtonTS p24_btnMMIO_variable_rotator;
        private System.Windows.Forms.ButtonTS p24_btnMeterCopySettings;
        private System.Windows.Forms.ButtonTS p24_btnMeterDown;
        private System.Windows.Forms.ButtonTS p24_btnMeterPasteSettings;
        private System.Windows.Forms.ButtonTS p24_btnMeterUp;
        private System.Windows.Forms.ButtonTS p24_btnOtherButtons_reset_layout;
        private System.Windows.Forms.ButtonTS p24_btnRecording_4char_copy;
        private System.Windows.Forms.ButtonTS p24_btnRecording_assingnkeybind;
        private System.Windows.Forms.ButtonTS p24_btnRecording_export_wav_from_slot;
        private System.Windows.Forms.ButtonTS p24_btnRecording_globalkeybind_assign;
        private System.Windows.Forms.ButtonTS p24_btnRecording_load_wav_to_slot;
        private System.Windows.Forms.ButtonTS p24_btnRecording_openStorageFolder;
        private System.Windows.Forms.ButtonTS p24_btnRecoverContainer;
        private System.Windows.Forms.ButtonTS p24_btnRemoveMeterItem;
        private System.Windows.Forms.ButtonTS p24_btnTextOverlayVarPicker;
        private System.Windows.Forms.ButtonTS p24_btnTextOverlay_Font1;
        private System.Windows.Forms.ButtonTS p24_btnTextOverlay_Font2;
        private System.Windows.Forms.ButtonTS p24_btnTextOverlay_copyfonts;
        private System.Windows.Forms.ButtonTS p24_btnTextOverlay_copyoffsets;
        private System.Windows.Forms.ButtonTS p24_btnVFOCopyColourFromMainNumbers;
        private System.Windows.Forms.ButtonTS p24_btnWaveRecord_reset_layout;
        private System.Windows.Forms.ButtonTS p24_btnWebImage_bsdworld_visit;
        private System.Windows.Forms.ButtonTS p24_btnWebImage_goto_next;
        private System.Windows.Forms.ButtonTS p24_btnWebImage_hamqsl_donate;
        private System.Windows.Forms.ButtonTS p24_buttonTS1;
        private System.Windows.Forms.ButtonTS p24_buttonTS2;
        private System.Windows.Forms.CheckBoxTS p24_chkBSDWorldDarkMode;
        private System.Windows.Forms.CheckBoxTS p24_chkBandButtons_band_inactive_use;
        private System.Windows.Forms.CheckBoxTS p24_chkBandButtons_fade_rx;
        private System.Windows.Forms.CheckBoxTS p24_chkBandButtons_fade_tx;
        private System.Windows.Forms.CheckBoxTS p24_chkBandButtons_use_indicator;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_byp;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_ext1;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_rx1;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_rx2;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_rx3;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_rxtxant;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_tx1;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_tx2;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_tx3;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_antenna_xvtr;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_fix_text_size;
        private System.Windows.Forms.CheckBoxTS p24_chkButtonBox_use_icons;
        private System.Windows.Forms.CheckBoxTS p24_chkContainerBorder;
        private System.Windows.Forms.CheckBoxTS p24_chkContainerHighlight;
        private System.Windows.Forms.CheckBoxTS p24_chkContainerMinimises;
        private System.Windows.Forms.CheckBoxTS p24_chkContainerNoTitle;
        private System.Windows.Forms.CheckBoxTS p24_chkContainerShowRX;
        private System.Windows.Forms.CheckBoxTS p24_chkContainerShowTX;
        private System.Windows.Forms.CheckBoxTS p24_chkContainer_hidewhennotused;
        private System.Windows.Forms.CheckBoxTS p24_chkDialDisplay_alwaysshow_vfos;
        private System.Windows.Forms.CheckBoxTS p24_chkDialDisplay_fade_rx;
        private System.Windows.Forms.CheckBoxTS p24_chkDialDisplay_fade_tx;
        private System.Windows.Forms.CheckBoxTS p24_chkDial_align;
        private System.Windows.Forms.CheckBoxTS p24_chkFilterDisplay_fadeonrx;
        private System.Windows.Forms.CheckBoxTS p24_chkFilterDisplay_fadeontx;
        private System.Windows.Forms.CheckBoxTS p24_chkFilterDisplay_fixed_tx_zoom;
        private System.Windows.Forms.CheckBoxTS p24_chkFilterDisplay_fixed_zoom;
        private System.Windows.Forms.CheckBoxTS p24_chkFilterDisplay_show_limits;
        private System.Windows.Forms.CheckBoxTS p24_chkFilter_characteristic;
        private System.Windows.Forms.CheckBoxTS p24_chkFilter_fill_spec;
        private System.Windows.Forms.CheckBoxTS p24_chkFilter_grey_outsidepb;
        private System.Windows.Forms.CheckBoxTS p24_chkFilter_sideband_mode;
        private System.Windows.Forms.CheckBoxTS p24_chkHistory_1_show_axis;
        private System.Windows.Forms.CheckBoxTS p24_chkHistory_auto_0_scale;
        private System.Windows.Forms.CheckBoxTS p24_chkHistory_auto_1_scale;
        private System.Windows.Forms.CheckBoxTS p24_chkHistory_fade_rx;
        private System.Windows.Forms.CheckBoxTS p24_chkHistory_fade_tx;
        private System.Windows.Forms.CheckBoxTS p24_chkLedIndicator_FadeOnRX;
        private System.Windows.Forms.CheckBoxTS p24_chkLedIndicator_FadeOnTX;
        private System.Windows.Forms.CheckBoxTS p24_chkLedIndicator_ShowPanel;
        private System.Windows.Forms.CheckBoxTS p24_chkLed_notx_false;
        private System.Windows.Forms.CheckBoxTS p24_chkLed_notx_true;
        private System.Windows.Forms.CheckBoxTS p24_chkLed_process_when_hidden;
        private System.Windows.Forms.CheckBoxTS p24_chkLed_show_false;
        private System.Windows.Forms.CheckBoxTS p24_chkLed_show_true;
        private System.Windows.Forms.CheckBoxTS p24_chkLockContainer;
        private System.Windows.Forms.CheckBoxTS p24_chkMMClockTitle;
        private System.Windows.Forms.CheckBoxTS p24_chkMaintainNFAdjustDeltaRX1;
        private System.Windows.Forms.CheckBoxTS p24_chkMaintainNFAdjustDeltaRX2;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemDarkMode;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemDarkModeRotator;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemFadeOnRx;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemFadeOnRxRotator;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemFadeOnRxSpacer;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemFadeOnTx;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemFadeOnTxRotator;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemFadeOnTxSpacer;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemHistory;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemPeakHold;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemPeakValue;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemRotatorAllowControl;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemRotatorCardinals;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemRotatorShowBeamWidth;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemSegmented;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemShadow;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemShowIndicator;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemShowSubIndicator;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemSolid;
        private System.Windows.Forms.CheckBoxTS p24_chkMeterItemTitle;
        private System.Windows.Forms.CheckBoxTS p24_chkMultiMeter_auto_container_height;
        private System.Windows.Forms.CheckBoxTS p24_chkMultiMeter_vfo_show_bandtext;
        private System.Windows.Forms.CheckBoxTS p24_chkRecording_canRepeat;
        private System.Windows.Forms.CheckBoxTS p24_chkRecording_globalkeybind;
        private System.Windows.Forms.CheckBoxTS p24_chkRecording_ignore_play_tempchanges;
        private System.Windows.Forms.CheckBoxTS p24_chkRecording_ignore_record_tempchanges;
        private System.Windows.Forms.CheckBoxTS p24_chkRecording_playkeybind;
        private System.Windows.Forms.CheckBoxTS p24_chkRecording_slot_locked;
        private System.Windows.Forms.CheckBoxTS p24_chkTextOverlay_FadeOnRX;
        private System.Windows.Forms.CheckBoxTS p24_chkTextOverlay_FadeOnTX;
        private System.Windows.Forms.CheckBoxTS p24_chkTextOverlay_ShowPanel;
        private System.Windows.Forms.CheckBoxTS p24_chkTextOverlay_rx_on_led;
        private System.Windows.Forms.CheckBoxTS p24_chkTextOverlay_textback1;
        private System.Windows.Forms.CheckBoxTS p24_chkTextOverlay_textback2;
        private System.Windows.Forms.CheckBoxTS p24_chkTextOverlay_tx_on_led;
        private System.Windows.Forms.CheckBoxTS p24_chkWaveRecord_fade_rx;
        private System.Windows.Forms.CheckBoxTS p24_chkWaveRecord_fade_tx;
        private System.Windows.Forms.CheckBoxTS p24_chkWebImage_background;
        private System.Windows.Forms.CheckBoxTS p24_chkWebImage_bypass_cache;
        private System.Windows.Forms.CheckBoxTS p24_chkWebImage_fade_rx;
        private System.Windows.Forms.CheckBoxTS p24_chkWebImage_fade_tx;
        private PowerSDR.ColorButton p24_clrbtnBandButtons_border;
        private PowerSDR.ColorButton p24_clrbtnBandButtons_fill;
        private PowerSDR.ColorButton p24_clrbtnBandButtons_hover;
        private PowerSDR.ColorButton p24_clrbtnBandButtons_indicator_off;
        private PowerSDR.ColorButton p24_clrbtnBandButtons_indicator_on;
        private PowerSDR.ColorButton p24_clrbtnButonBox_click;
        private PowerSDR.ColorButton p24_clrbtnButonBox_fontcolour;
        private PowerSDR.ColorButton p24_clrbtnContainerBackground;
        private PowerSDR.ColorButton p24_clrbtnDial_button_highlight;
        private PowerSDR.ColorButton p24_clrbtnDial_button_off;
        private PowerSDR.ColorButton p24_clrbtnDial_button_on;
        private PowerSDR.ColorButton p24_clrbtnDial_circle;
        private PowerSDR.ColorButton p24_clrbtnDial_fast;
        private PowerSDR.ColorButton p24_clrbtnDial_hold;
        private PowerSDR.ColorButton p24_clrbtnDial_pad;
        private PowerSDR.ColorButton p24_clrbtnDial_pad_pressed;
        private PowerSDR.ColorButton p24_clrbtnDial_ring;
        private PowerSDR.ColorButton p24_clrbtnDial_slow;
        private PowerSDR.ColorButton p24_clrbtnDial_text;
        private PowerSDR.ColorButton p24_clrbtnFilterDisplay_backcolour;
        private PowerSDR.ColorButton p24_clrbtnFilter_button_highlight;
        private PowerSDR.ColorButton p24_clrbtnFilter_data_fill;
        private PowerSDR.ColorButton p24_clrbtnFilter_data_line;
        private PowerSDR.ColorButton p24_clrbtnFilter_edge_highlight;
        private PowerSDR.ColorButton p24_clrbtnFilter_edges;
        private PowerSDR.ColorButton p24_clrbtnFilter_edges_tx;
        private PowerSDR.ColorButton p24_clrbtnFilter_extents;
        private PowerSDR.ColorButton p24_clrbtnFilter_meter_back;
        private PowerSDR.ColorButton p24_clrbtnFilter_notch;
        private PowerSDR.ColorButton p24_clrbtnFilter_notch_highlight;
        private PowerSDR.ColorButton p24_clrbtnFilter_number_highlight;
        private PowerSDR.ColorButton p24_clrbtnFilter_setting_on;
        private PowerSDR.ColorButton p24_clrbtnFilter_snap_line;
        private PowerSDR.ColorButton p24_clrbtnFilter_text;
        private PowerSDR.ColorButton p24_clrbtnFilter_wf_low;
        private PowerSDR.ColorButton p24_clrbtnHistory_background;
        private PowerSDR.ColorButton p24_clrbtnHistory_colour_0;
        private PowerSDR.ColorButton p24_clrbtnHistory_colour_1;
        private PowerSDR.ColorButton p24_clrbtnHistory_lines;
        private PowerSDR.ColorButton p24_clrbtnHistory_time;
        private PowerSDR.ColorButton p24_clrbtnLedIndicator_PanelBackground;
        private PowerSDR.ColorButton p24_clrbtnLedIndicator_PanelBackgroundTX;
        private PowerSDR.ColorButton p24_clrbtnLedIndicator_false;
        private PowerSDR.ColorButton p24_clrbtnLedIndicator_true;
        private PowerSDR.ColorButton p24_clrbtnMMClockBackground;
        private PowerSDR.ColorButton p24_clrbtnMMClockTitle;
        private PowerSDR.ColorButton p24_clrbtnMMDate;
        private PowerSDR.ColorButton p24_clrbtnMMTime;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDigitHighlight;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayBackground;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayBand;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayFilter;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayFrequency;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayFrequency_small;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayMode;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayRx;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplaySplit;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplaySplitBack;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayTitle;
        private PowerSDR.ColorButton p24_clrbtnMMVfoDisplayTx;
        private PowerSDR.ColorButton p24_clrbtnMeterItemHBackground;
        private PowerSDR.ColorButton p24_clrbtnMeterItemHBackgroundRotator;
        private PowerSDR.ColorButton p24_clrbtnMeterItemHBackgroundSpacerRX;
        private PowerSDR.ColorButton p24_clrbtnMeterItemHBackgroundSpacerTX;
        private PowerSDR.ColorButton p24_clrbtnMeterItemHigh;
        private PowerSDR.ColorButton p24_clrbtnMeterItemHistory;
        private PowerSDR.ColorButton p24_clrbtnMeterItemIndicator;
        private PowerSDR.ColorButton p24_clrbtnMeterItemLow;
        private PowerSDR.ColorButton p24_clrbtnMeterItemMeterTitle;
        private PowerSDR.ColorButton p24_clrbtnMeterItemPeakHold;
        private PowerSDR.ColorButton p24_clrbtnMeterItemPeakValueColour;
        private PowerSDR.ColorButton p24_clrbtnMeterItemPowerScale;
        private PowerSDR.ColorButton p24_clrbtnMeterItemRotatorArrow;
        private PowerSDR.ColorButton p24_clrbtnMeterItemRotatorBeamWidth;
        private PowerSDR.ColorButton p24_clrbtnMeterItemRotatorControlColour;
        private PowerSDR.ColorButton p24_clrbtnMeterItemRotatorLargeDot;
        private PowerSDR.ColorButton p24_clrbtnMeterItemRotatorSmallDot;
        private PowerSDR.ColorButton p24_clrbtnMeterItemRotatorText;
        private PowerSDR.ColorButton p24_clrbtnMeterItemSegmentedSolidColourHigh;
        private PowerSDR.ColorButton p24_clrbtnMeterItemSegmentedSolidColourLow;
        private PowerSDR.ColorButton p24_clrbtnMeterItemSubIndicator;
        private PowerSDR.ColorButton p24_clrbtnMultiMeter_vfo_lock;
        private PowerSDR.ColorButton p24_clrbtnMultiMeter_vfo_show_bandtext;
        private PowerSDR.ColorButton p24_clrbtnMultiMeter_vfo_sync;
        private PowerSDR.ColorButton p24_clrbtnTextOverlay_PanelBackground;
        private PowerSDR.ColorButton p24_clrbtnTextOverlay_PanelBackgroundTX;
        private PowerSDR.ColorButton p24_clrbtnTextOverlay_TextBackColour1;
        private PowerSDR.ColorButton p24_clrbtnTextOverlay_TextBackColour2;
        private PowerSDR.ColorButton p24_clrbtnTextOverlay_TextColour1;
        private PowerSDR.ColorButton p24_clrbtnTextOverlay_TextColour2;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_back;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_border;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_button_border;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_button_fill;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_button_hover;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_delete;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_play;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_row;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_scroll_hover;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_scroll_thumb;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_scroll_track;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_stop;
        private PowerSDR.ColorButton p24_clrbtnWaveRecord_text;
        private System.Windows.Forms.ComboBoxTS p24_comboContainerSelect;
        private System.Windows.Forms.ComboBoxTS p24_comboFilter_wf_palette;
        private System.Windows.Forms.ComboBoxTS p24_comboHistory_reading_0;
        private System.Windows.Forms.ComboBoxTS p24_comboHistory_reading_1;
        private System.Windows.Forms.ComboBoxTS p24_comboWebImage_BsdWorld;
        private System.Windows.Forms.ComboBoxTS p24_comboWebImage_HamQsl;
        private System.Windows.Forms.ComboBoxTS p24_comboWebImage_nasa;
        private System.Windows.Forms.ComboBoxTS p24_comboWebImage_noaa;
        private System.Windows.Forms.GroupBoxTS p24_groupBoxTS40;
        private System.Windows.Forms.GroupBoxTS p24_groupBoxTS41;
        private System.Windows.Forms.GroupBoxTS p24_groupBoxTS42;
        private System.Windows.Forms.GroupBoxTS p24_groupBoxTS43;
        private System.Windows.Forms.GroupBoxTS p24_groupBoxTS45;
        private System.Windows.Forms.GroupBoxTS p24_groupBoxTS46;
        private System.Windows.Forms.GroupBoxTS p24_grpButtonBox;
        private System.Windows.Forms.GroupBoxTS p24_grpDialDisplay;
        private System.Windows.Forms.GroupBoxTS p24_grpGlobalStopPlayRecord;
        private System.Windows.Forms.GroupBoxTS p24_grpHistoryItem;
        private System.Windows.Forms.GroupBoxTS p24_grpLedIndicator;
        private System.Windows.Forms.GroupBoxTS p24_grpMeterItemClockSettings;
        private System.Windows.Forms.GroupBoxTS p24_grpMeterItemDataOutNode;
        private System.Windows.Forms.GroupBoxTS p24_grpMeterItemFilterDisplay;
        private System.Windows.Forms.GroupBoxTS p24_grpMeterItemRotator;
        private System.Windows.Forms.GroupBoxTS p24_grpMeterItemSettings;
        private System.Windows.Forms.GroupBoxTS p24_grpMeterItemSpacerSettings;
        private System.Windows.Forms.GroupBoxTS p24_grpMeterItemVfoDisplaySettings;
        private System.Windows.Forms.GroupBoxTS p24_grpMultiMeterHolder;
        private System.Windows.Forms.GroupBoxTS p24_grpTextOverlay;
        private System.Windows.Forms.GroupBoxTS p24_grpWaveRecordItem;
        private System.Windows.Forms.GroupBoxTS p24_grpWebImage;
        private System.Windows.Forms.Label p24_label22;
        private System.Windows.Forms.LabelTS p24_labelTS162;
        private System.Windows.Forms.LabelTS p24_labelTS163;
        private System.Windows.Forms.LabelTS p24_labelTS164;
        private System.Windows.Forms.LabelTS p24_labelTS166;
        private System.Windows.Forms.LabelTS p24_labelTS167;
        private System.Windows.Forms.LabelTS p24_labelTS168;
        private System.Windows.Forms.LabelTS p24_labelTS169;
        private System.Windows.Forms.LabelTS p24_labelTS170;
        private System.Windows.Forms.LabelTS p24_labelTS171;
        private System.Windows.Forms.LabelTS p24_labelTS172;
        private System.Windows.Forms.LabelTS p24_labelTS173;
        private System.Windows.Forms.LabelTS p24_labelTS174;
        private System.Windows.Forms.LabelTS p24_labelTS175;
        private System.Windows.Forms.LabelTS p24_labelTS176;
        private System.Windows.Forms.LabelTS p24_labelTS177;
        private System.Windows.Forms.LabelTS p24_labelTS196;
        private System.Windows.Forms.LabelTS p24_labelTS197;
        private System.Windows.Forms.LabelTS p24_labelTS199;
        private System.Windows.Forms.LabelTS p24_labelTS200;
        private System.Windows.Forms.LabelTS p24_labelTS201;
        private System.Windows.Forms.LabelTS p24_labelTS202;
        private System.Windows.Forms.LabelTS p24_labelTS203;
        private System.Windows.Forms.LabelTS p24_labelTS204;
        private System.Windows.Forms.LabelTS p24_labelTS205;
        private System.Windows.Forms.LabelTS p24_labelTS206;
        private System.Windows.Forms.LabelTS p24_labelTS207;
        private System.Windows.Forms.LabelTS p24_labelTS208;
        private System.Windows.Forms.LabelTS p24_labelTS209;
        private System.Windows.Forms.LabelTS p24_labelTS210;
        private System.Windows.Forms.LabelTS p24_labelTS212;
        private System.Windows.Forms.LabelTS p24_labelTS215;
        private System.Windows.Forms.LabelTS p24_labelTS216;
        private System.Windows.Forms.LabelTS p24_labelTS217;
        private System.Windows.Forms.LabelTS p24_labelTS218;
        private System.Windows.Forms.LabelTS p24_labelTS219;
        private System.Windows.Forms.LabelTS p24_labelTS220;
        private System.Windows.Forms.LabelTS p24_labelTS221;
        private System.Windows.Forms.LabelTS p24_labelTS222;
        private System.Windows.Forms.LabelTS p24_labelTS223;
        private System.Windows.Forms.LabelTS p24_labelTS224;
        private System.Windows.Forms.LabelTS p24_labelTS225;
        private System.Windows.Forms.LabelTS p24_labelTS226;
        private System.Windows.Forms.LabelTS p24_labelTS227;
        private System.Windows.Forms.LabelTS p24_labelTS228;
        private System.Windows.Forms.LabelTS p24_labelTS229;
        private System.Windows.Forms.LabelTS p24_labelTS230;
        private System.Windows.Forms.LabelTS p24_labelTS231;
        private System.Windows.Forms.LabelTS p24_labelTS232;
        private System.Windows.Forms.LabelTS p24_labelTS233;
        private System.Windows.Forms.LabelTS p24_labelTS234;
        private System.Windows.Forms.LabelTS p24_labelTS235;
        private System.Windows.Forms.LabelTS p24_labelTS236;
        private System.Windows.Forms.LabelTS p24_labelTS237;
        private System.Windows.Forms.LabelTS p24_labelTS238;
        private System.Windows.Forms.LabelTS p24_labelTS239;
        private System.Windows.Forms.LabelTS p24_labelTS240;
        private System.Windows.Forms.LabelTS p24_labelTS241;
        private System.Windows.Forms.LabelTS p24_labelTS242;
        private System.Windows.Forms.LabelTS p24_labelTS243;
        private System.Windows.Forms.LabelTS p24_labelTS244;
        private System.Windows.Forms.LabelTS p24_labelTS245;
        private System.Windows.Forms.LabelTS p24_labelTS246;
        private System.Windows.Forms.LabelTS p24_labelTS247;
        private System.Windows.Forms.LabelTS p24_labelTS248;
        private System.Windows.Forms.LabelTS p24_labelTS249;
        private System.Windows.Forms.LabelTS p24_labelTS250;
        private System.Windows.Forms.LabelTS p24_labelTS251;
        private System.Windows.Forms.LabelTS p24_labelTS252;
        private System.Windows.Forms.LabelTS p24_labelTS253;
        private System.Windows.Forms.LabelTS p24_labelTS258;
        private System.Windows.Forms.LabelTS p24_labelTS259;
        private System.Windows.Forms.LabelTS p24_labelTS260;
        private System.Windows.Forms.LabelTS p24_labelTS261;
        private System.Windows.Forms.LabelTS p24_labelTS262;
        private System.Windows.Forms.LabelTS p24_labelTS263;
        private System.Windows.Forms.LabelTS p24_labelTS264;
        private System.Windows.Forms.LabelTS p24_labelTS265;
        private System.Windows.Forms.LabelTS p24_labelTS278;
        private System.Windows.Forms.LabelTS p24_labelTS279;
        private System.Windows.Forms.LabelTS p24_labelTS280;
        private System.Windows.Forms.LabelTS p24_labelTS281;
        private System.Windows.Forms.LabelTS p24_labelTS287;
        private System.Windows.Forms.LabelTS p24_labelTS292;
        private System.Windows.Forms.LabelTS p24_labelTS293;
        private System.Windows.Forms.LabelTS p24_labelTS294;
        private System.Windows.Forms.LabelTS p24_labelTS296;
        private System.Windows.Forms.LabelTS p24_labelTS297;
        private System.Windows.Forms.LabelTS p24_labelTS298;
        private System.Windows.Forms.LabelTS p24_labelTS299;
        private System.Windows.Forms.LabelTS p24_labelTS300;
        private System.Windows.Forms.LabelTS p24_labelTS301;
        private System.Windows.Forms.LabelTS p24_labelTS330;
        private System.Windows.Forms.LabelTS p24_labelTS331;
        private System.Windows.Forms.LabelTS p24_labelTS332;
        private System.Windows.Forms.LabelTS p24_labelTS333;
        private System.Windows.Forms.LabelTS p24_labelTS334;
        private System.Windows.Forms.LabelTS p24_labelTS335;
        private System.Windows.Forms.LabelTS p24_labelTS336;
        private System.Windows.Forms.LabelTS p24_labelTS337;
        private System.Windows.Forms.LabelTS p24_labelTS338;
        private System.Windows.Forms.LabelTS p24_labelTS339;
        private System.Windows.Forms.LabelTS p24_labelTS341;
        private System.Windows.Forms.LabelTS p24_labelTS344;
        private System.Windows.Forms.LabelTS p24_labelTS345;
        private System.Windows.Forms.LabelTS p24_labelTS346;
        private System.Windows.Forms.LabelTS p24_labelTS347;
        private System.Windows.Forms.LabelTS p24_labelTS348;
        private System.Windows.Forms.LabelTS p24_labelTS349;
        private System.Windows.Forms.LabelTS p24_labelTS350;
        private System.Windows.Forms.LabelTS p24_labelTS351;
        private System.Windows.Forms.LabelTS p24_labelTS352;
        private System.Windows.Forms.LabelTS p24_labelTS353;
        private System.Windows.Forms.LabelTS p24_labelTS354;
        private System.Windows.Forms.LabelTS p24_labelTS407;
        private System.Windows.Forms.LabelTS p24_labelTS408;
        private System.Windows.Forms.LabelTS p24_labelTS409;
        private System.Windows.Forms.LabelTS p24_labelTS410;
        private System.Windows.Forms.LabelTS p24_labelTS411;
        private System.Windows.Forms.LabelTS p24_labelTS412;
        private System.Windows.Forms.LabelTS p24_labelTS413;
        private System.Windows.Forms.LabelTS p24_labelTS414;
        private System.Windows.Forms.LabelTS p24_labelTS415;
        private System.Windows.Forms.LabelTS p24_labelTS416;
        private System.Windows.Forms.LabelTS p24_labelTS417;
        private System.Windows.Forms.LabelTS p24_labelTS418;
        private System.Windows.Forms.LabelTS p24_labelTS419;
        private System.Windows.Forms.LabelTS p24_labelTS420;
        private System.Windows.Forms.LabelTS p24_labelTS421;
        private System.Windows.Forms.LabelTS p24_labelTS422;
        private System.Windows.Forms.LabelTS p24_labelTS424;
        private System.Windows.Forms.LabelTS p24_labelTS428;
        private System.Windows.Forms.LabelTS p24_labelTS429;
        private System.Windows.Forms.LabelTS p24_labelTS431;
        private System.Windows.Forms.LabelTS p24_labelTS432;
        private System.Windows.Forms.LabelTS p24_labelTS433;
        private System.Windows.Forms.LabelTS p24_labelTS482;
        private System.Windows.Forms.LabelTS p24_labelTS650;
        private System.Windows.Forms.LabelTS p24_labelTS651;
        private System.Windows.Forms.LabelTS p24_labelTS652;
        private System.Windows.Forms.LabelTS p24_labelTS653;
        private System.Windows.Forms.LabelTS p24_labelTS654;
        private System.Windows.Forms.LabelTS p24_labelTS655;
        private System.Windows.Forms.LabelTS p24_labelTS657;
        private System.Windows.Forms.LabelTS p24_labelTS658;
        private System.Windows.Forms.LabelTS p24_lblBandButtons_indicator_border;
        private System.Windows.Forms.LabelTS p24_lblBandButtons_indicator_style;
        private System.Windows.Forms.LabelTS p24_lblLedIndicator_panelbackground;
        private System.Windows.Forms.LabelTS p24_lblLedIndicator_panelbackgroundTX;
        private System.Windows.Forms.LabelTS p24_lblLed_Valid;
        private System.Windows.Forms.LabelTS p24_lblMMBackground;
        private System.Windows.Forms.LabelTS p24_lblMMClockBackground;
        private System.Windows.Forms.LabelTS p24_lblMMContainerBackground;
        private System.Windows.Forms.LabelTS p24_lblMMContainerNotes;
        private System.Windows.Forms.LabelTS p24_lblMMEyeBezelSize;
        private System.Windows.Forms.LabelTS p24_lblMMEyeSize;
        private System.Windows.Forms.LabelTS p24_lblMMHigh;
        private System.Windows.Forms.LabelTS p24_lblMMHistory;
        private System.Windows.Forms.LabelTS p24_lblMMHistoryIgnore;
        private System.Windows.Forms.LabelTS p24_lblMMIndicator;
        private System.Windows.Forms.LabelTS p24_lblMMIndicatorSub;
        private System.Windows.Forms.LabelTS p24_lblMMLow;
        private System.Windows.Forms.LabelTS p24_lblMMPowerLimit;
        private System.Windows.Forms.LabelTS p24_lblMMsegSolHigh;
        private System.Windows.Forms.LabelTS p24_lblMMsegSolLow;
        private System.Windows.Forms.LabelTS p24_lblMeterItemRotatorAZcommand;
        private System.Windows.Forms.LabelTS p24_lblMeterItemRotatorBeamWidth_alpha;
        private System.Windows.Forms.LabelTS p24_lblMeterItemRotatorBeamWidth_degrees;
        private System.Windows.Forms.LabelTS p24_lblMeterItemRotatorELEcommand;
        private System.Windows.Forms.LabelTS p24_lblRotator_4charID;
        private System.Windows.Forms.LabelTS p24_lblTextOverlay_panelbackground;
        private System.Windows.Forms.LabelTS p24_lblTextOverlay_panelbackgroundTX;
        private System.Windows.Forms.LabelTS p24_lblTextOverlay_panelpadding;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordBack;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordBorder;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordButtonBorder;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordButtonFill;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordButtonHover;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordDelete;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordHeightRatio;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordPlay;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordRadius;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordRow;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordScrollHover;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordScrollThumb;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordScrollTrack;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordStop;
        private System.Windows.Forms.LabelTS p24_lblWaveRecordText;
        private System.Windows.Forms.LabelTS p24_lblWebImage_after;
        private System.Windows.Forms.LabelTS p24_lblWebImage_secs;
        private System.Windows.Forms.LabelTS p24_lblWebImage_state;
        private System.Windows.Forms.ListBox p24_lstMetersAvailable;
        private System.Windows.Forms.ListBox p24_lstMetersInUse;
        private System.Windows.Forms.NumericUpDownTS p24_nudBandButtons_border;
        private System.Windows.Forms.NumericUpDownTS p24_nudBandButtons_columns;
        private System.Windows.Forms.NumericUpDownTS p24_nudBandButtons_height_ratio;
        private System.Windows.Forms.NumericUpDownTS p24_nudBandButtons_indicator_border;
        private System.Windows.Forms.NumericUpDownTS p24_nudBandButtons_indicator_style;
        private System.Windows.Forms.NumericUpDownTS p24_nudBandButtons_margin;
        private System.Windows.Forms.NumericUpDownTS p24_nudBandButtons_radius;
        private System.Windows.Forms.NumericUpDownTS p24_nudButtonBox_font_scale;
        private System.Windows.Forms.NumericUpDownTS p24_nudButtonBox_font_x_shift;
        private System.Windows.Forms.NumericUpDownTS p24_nudButtonBox_font_y_shift;
        private System.Windows.Forms.NumericUpDownTS p24_nudDataOutNode_sendinterval;
        private System.Windows.Forms.NumericUpDownTS p24_nudDialDisplay_font_scale;
        private System.Windows.Forms.NumericUpDownTS p24_nudDialDisplay_vertical_ratio;
        private System.Windows.Forms.NumericUpDownTS p24_nudDial_decrement;
        private System.Windows.Forms.NumericUpDownTS p24_nudDial_degrees_for_change;
        private System.Windows.Forms.NumericUpDownTS p24_nudDial_increment;
        private System.Windows.Forms.NumericUpDownTS p24_nudDial_interval;
        private System.Windows.Forms.NumericUpDownTS p24_nudDial_max_increments;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilterDisplay_fixed_tx_zoom_level;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilterDisplay_fixed_zoom_level;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilterDisplay_vertical_ratio;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilterItem_cw_scale;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilterItem_font_scale;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilterItem_others_scale;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilterItem_sidebands_scale;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilter_lower_characteristic;
        private System.Windows.Forms.NumericUpDownTS p24_nudFilter_waterfall_frame_update;
        private System.Windows.Forms.NumericUpDownTS p24_nudHistory_axis0_max;
        private System.Windows.Forms.NumericUpDownTS p24_nudHistory_axis0_min;
        private System.Windows.Forms.NumericUpDownTS p24_nudHistory_axis1_max;
        private System.Windows.Forms.NumericUpDownTS p24_nudHistory_axis1_min;
        private System.Windows.Forms.NumericUpDownTS p24_nudHistory_keep_for;
        private System.Windows.Forms.NumericUpDownTS p24_nudHistory_update;
        private System.Windows.Forms.NumericUpDownTS p24_nudHistory_vertical_ratio;
        private System.Windows.Forms.NumericUpDownTS p24_nudLedIndicator_PanelPadding;
        private System.Windows.Forms.NumericUpDownTS p24_nudLedIndicator_UpdateInterval;
        private System.Windows.Forms.NumericUpDownTS p24_nudLedIndicator_xOffset;
        private System.Windows.Forms.NumericUpDownTS p24_nudLedIndicator_xSize;
        private System.Windows.Forms.NumericUpDownTS p24_nudLedIndicator_yOffset;
        private System.Windows.Forms.NumericUpDownTS p24_nudLedIndicator_ySize;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemAttackRate;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemDecayRate;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemEyeBezelScale;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemEyeScale;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemHistoryDuration;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemIgnoreHistoryDuration;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemRotatorBeamWidth;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemRotatorBeamWidth_alpha;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemRotator_padding;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemSpacerPadding;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemUpdateRate;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemUpdateRateRotator;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItem_custom_high;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItem_custom_max;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItem_custom_min;
        private System.Windows.Forms.NumericUpDownTS p24_nudMeterItemsPowerLimit;
        private System.Windows.Forms.NumericUpDownTS p24_nudRecording_repeatDelay;
        private System.Windows.Forms.NumericUpDownTS p24_nudRecording_slot_settings;
        private System.Windows.Forms.NumericUpDownTS p24_nudRecording_tx_gain_adjust;
        private System.Windows.Forms.NumericUpDownTS p24_nudTextOverlay_PanelPadding;
        private System.Windows.Forms.NumericUpDownTS p24_nudTextOverlay_RXxOffset;
        private System.Windows.Forms.NumericUpDownTS p24_nudTextOverlay_RXyOffset;
        private System.Windows.Forms.NumericUpDownTS p24_nudTextOverlay_TXxOffset;
        private System.Windows.Forms.NumericUpDownTS p24_nudTextOverlay_TXyOffset;
        private System.Windows.Forms.NumericUpDownTS p24_nudVoiceRecordingPlayback_slots;
        private System.Windows.Forms.NumericUpDownTS p24_nudWaveRecord_radius;
        private System.Windows.Forms.NumericUpDownTS p24_nudWaveRecord_vertical_ratio;
        private System.Windows.Forms.NumericUpDownTS p24_nudWebImage_background_time;
        private System.Windows.Forms.NumericUpDownTS p24_nudWebImage_update_interval;
        private System.Windows.Forms.NumericUpDownTS p24_nudWebImage_width_scale;
        private System.Windows.Forms.PictureBox p24_picButtonBoxInfo;
        private System.Windows.Forms.PictureBox p24_picMultiMeterRotatorControlInfo;
        private System.Windows.Forms.PanelTS p24_pnlButtonBox_antenna_toggles;
        private System.Windows.Forms.PanelTS p24_pnlFilterModeModifiers;
        private System.Windows.Forms.PanelTS p24_pnlMeterItemSettings;
        private System.Windows.Forms.PanelTS p24_pnlMeterItemSettings_custom;
        private System.Windows.Forms.PanelTS p24_pnlVariableInUse_1;
        private System.Windows.Forms.PanelTS p24_pnlVariableInUse_1_history;
        private System.Windows.Forms.PanelTS p24_pnlVariableInUse_1_rotator;
        private System.Windows.Forms.PanelTS p24_pnlVariableInUse_2;
        private System.Windows.Forms.PanelTS p24_pnlVariableInUse_2_history;
        private System.Windows.Forms.PanelTS p24_pnlVariableInUse_2_rotator;
        private System.Windows.Forms.PanelTS p24_pnlVoiceRecordPlayback;
        private System.Windows.Forms.RadioButtonTS p24_radContainer_rx1_data;
        private System.Windows.Forms.RadioButtonTS p24_radContainer_rx2_data;
        private System.Windows.Forms.RadioButtonTS p24_radFilterItem_none;
        private System.Windows.Forms.RadioButtonTS p24_radFilterItem_panadaptor;
        private System.Windows.Forms.RadioButtonTS p24_radFilterItem_panafall;
        private System.Windows.Forms.RadioButtonTS p24_radFilterItem_waterfall;
        private System.Windows.Forms.RadioButtonTS p24_radLed_light_blink;
        private System.Windows.Forms.RadioButtonTS p24_radLed_light_on_off;
        private System.Windows.Forms.RadioButtonTS p24_radLed_light_pulsate;
        private System.Windows.Forms.RadioButtonTS p24_radMM12Clock;
        private System.Windows.Forms.RadioButtonTS p24_radMM24Clock;
        private System.Windows.Forms.RadioButtonTS p24_radMeterItemRotator_show_az;
        private System.Windows.Forms.RadioButtonTS p24_radMeterItemRotator_show_both;
        private System.Windows.Forms.RadioButtonTS p24_radMeterItemRotator_show_ele;
        private System.Windows.Forms.RadioButtonTS p24_radMeterItemSettings;
        private System.Windows.Forms.RadioButtonTS p24_radMeterItemSettings_custom;
        private System.Windows.Forms.RadioButtonTS p24_radMultiMeter_vfo_display_both;
        private System.Windows.Forms.RadioButtonTS p24_radMultiMeter_vfo_display_vfoa;
        private System.Windows.Forms.RadioButtonTS p24_radMultiMeter_vfo_display_vfob;
        private System.Windows.Forms.ScrollableControl p24_scrlFilter;
        private System.Windows.Forms.ScrollableControl p24_scrollableControl1;
        private System.Windows.Forms.ScrollableControl p24_scrollableControl2;
        private System.Windows.Forms.TrackBarTS p24_tbMeterItemHistoryAlpha;
        private System.Windows.Forms.TabPage p24_tpAppearanceMeter2;
        private System.Windows.Forms.TextBoxTS p24_txtContainerNotes;
        private System.Windows.Forms.TextBoxTS p24_txtDataOutNode_4charID;
        private System.Windows.Forms.TextBoxTS p24_txtLedIndicator_4char;
        private System.Windows.Forms.TextBoxTS p24_txtLedIndicator_condition;
        private System.Windows.Forms.TextBoxTS p24_txtMeterItemRotatorAZcommand;
        private System.Windows.Forms.TextBoxTS p24_txtMeterItemRotatorELEcommand;
        private System.Windows.Forms.TextBoxTS p24_txtMeterItemRotatorSTOPcommand;
        private System.Windows.Forms.TextBoxTS p24_txtMeterItem_custom_title;
        private System.Windows.Forms.TextBoxTS p24_txtMeterItem_custom_units;
        private System.Windows.Forms.TextBoxTS p24_txtRecording_4char;
        private System.Windows.Forms.TextBoxTS p24_txtRecording_globalkeybind;
        private System.Windows.Forms.TextBoxTS p24_txtRecording_labelText;
        private System.Windows.Forms.TextBoxTS p24_txtRecording_playkeybind;
        private System.Windows.Forms.TextBoxTS p24_txtRotator_4charID;
        private System.Windows.Forms.TextBoxTS p24_txtTextOverlay_RXText;
        private System.Windows.Forms.TextBoxTS p24_txtTextOverlay_TXText;
        private System.Windows.Forms.TextBoxTS p24_txtTextOverlay_rx_on_led_4char;
        private System.Windows.Forms.TextBoxTS p24_txtTextOverlay_tx_on_led_4char;
        private System.Windows.Forms.TextBoxTS p24_txtWebImage_4char;
        private System.Windows.Forms.TextBoxTS p24_txtWebImage_background_4char;
        private System.Windows.Forms.TextBoxTS p24_txtWebImage_url;
        private PowerSDR.ucSignalSelect p24_ucMeterItemSignalType;
        private PowerSDR.ucOtherButtonsOptionsGrid p24_ucOtherButtonsOptionsGrid_buttons;
        private PowerSDR.ucTunestepOptionsGrid p24_ucTunestepOptionsGrid_buttons;

        private bool p24_native_meters_ui_ready = false;

        internal void P24InitNativeMetersGadgets()
        {
            if (p24_native_meters_ui_ready) return;
            p24_native_meters_ui_ready = true;
            tmrLedValid.Interval = 250;
            tmrLedValid.Tick += tmrLedValid_Tick;

            p24_bntMultiMeterItemRotator_default_pstRotator = new System.Windows.Forms.ButtonTS();
            p24_btnAddMeterItem = new System.Windows.Forms.ButtonTS();
            p24_btnAddRX1Container = new System.Windows.Forms.ButtonTS();
            p24_btnBandButtons_font = new System.Windows.Forms.ButtonTS();
            p24_btnContainerDelete = new System.Windows.Forms.ButtonTS();
            p24_btnContainer_dupe = new System.Windows.Forms.ButtonTS();
            p24_btnContainer_load = new System.Windows.Forms.ButtonTS();
            p24_btnContainer_save = new System.Windows.Forms.ButtonTS();
            p24_btnFilter_4char_copy = new System.Windows.Forms.ButtonTS();
            p24_btnHistory_copy_minmax_from_0 = new System.Windows.Forms.ButtonTS();
            p24_btnLedIndicatorVarPicker = new System.Windows.Forms.ButtonTS();
            p24_btnLedIndicator_4char_copy = new System.Windows.Forms.ButtonTS();
            p24_btnLedIndicator_copy_sizex_to_y = new System.Windows.Forms.ButtonTS();
            p24_btnLedIndicator_copy_truefalse_colours = new System.Windows.Forms.ButtonTS();
            p24_btnMMIO_variable = new System.Windows.Forms.ButtonTS();
            p24_btnMMIO_variable_2 = new System.Windows.Forms.ButtonTS();
            p24_btnMMIO_variable_2_history = new System.Windows.Forms.ButtonTS();
            p24_btnMMIO_variable_2_rotator = new System.Windows.Forms.ButtonTS();
            p24_btnMMIO_variable_history = new System.Windows.Forms.ButtonTS();
            p24_btnMMIO_variable_rotator = new System.Windows.Forms.ButtonTS();
            p24_btnMeterCopySettings = new System.Windows.Forms.ButtonTS();
            p24_btnMeterDown = new System.Windows.Forms.ButtonTS();
            p24_btnMeterPasteSettings = new System.Windows.Forms.ButtonTS();
            p24_btnMeterUp = new System.Windows.Forms.ButtonTS();
            p24_btnOtherButtons_reset_layout = new System.Windows.Forms.ButtonTS();
            p24_btnRecording_4char_copy = new System.Windows.Forms.ButtonTS();
            p24_btnRecording_assingnkeybind = new System.Windows.Forms.ButtonTS();
            p24_btnRecording_export_wav_from_slot = new System.Windows.Forms.ButtonTS();
            p24_btnRecording_globalkeybind_assign = new System.Windows.Forms.ButtonTS();
            p24_btnRecording_load_wav_to_slot = new System.Windows.Forms.ButtonTS();
            p24_btnRecording_openStorageFolder = new System.Windows.Forms.ButtonTS();
            p24_btnRecoverContainer = new System.Windows.Forms.ButtonTS();
            p24_btnRemoveMeterItem = new System.Windows.Forms.ButtonTS();
            p24_btnTextOverlayVarPicker = new System.Windows.Forms.ButtonTS();
            p24_btnTextOverlay_Font1 = new System.Windows.Forms.ButtonTS();
            p24_btnTextOverlay_Font2 = new System.Windows.Forms.ButtonTS();
            p24_btnTextOverlay_copyfonts = new System.Windows.Forms.ButtonTS();
            p24_btnTextOverlay_copyoffsets = new System.Windows.Forms.ButtonTS();
            p24_btnVFOCopyColourFromMainNumbers = new System.Windows.Forms.ButtonTS();
            p24_btnWaveRecord_reset_layout = new System.Windows.Forms.ButtonTS();
            p24_btnWebImage_bsdworld_visit = new System.Windows.Forms.ButtonTS();
            p24_btnWebImage_goto_next = new System.Windows.Forms.ButtonTS();
            p24_btnWebImage_hamqsl_donate = new System.Windows.Forms.ButtonTS();
            p24_buttonTS1 = new System.Windows.Forms.ButtonTS();
            p24_buttonTS2 = new System.Windows.Forms.ButtonTS();
            p24_chkBSDWorldDarkMode = new System.Windows.Forms.CheckBoxTS();
            p24_chkBandButtons_band_inactive_use = new System.Windows.Forms.CheckBoxTS();
            p24_chkBandButtons_fade_rx = new System.Windows.Forms.CheckBoxTS();
            p24_chkBandButtons_fade_tx = new System.Windows.Forms.CheckBoxTS();
            p24_chkBandButtons_use_indicator = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_byp = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_ext1 = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_rx1 = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_rx2 = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_rx3 = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_rxtxant = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_tx1 = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_tx2 = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_tx3 = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_antenna_xvtr = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_fix_text_size = new System.Windows.Forms.CheckBoxTS();
            p24_chkButtonBox_use_icons = new System.Windows.Forms.CheckBoxTS();
            p24_chkContainerBorder = new System.Windows.Forms.CheckBoxTS();
            p24_chkContainerHighlight = new System.Windows.Forms.CheckBoxTS();
            p24_chkContainerMinimises = new System.Windows.Forms.CheckBoxTS();
            p24_chkContainerNoTitle = new System.Windows.Forms.CheckBoxTS();
            p24_chkContainerShowRX = new System.Windows.Forms.CheckBoxTS();
            p24_chkContainerShowTX = new System.Windows.Forms.CheckBoxTS();
            p24_chkContainer_hidewhennotused = new System.Windows.Forms.CheckBoxTS();
            p24_chkDialDisplay_alwaysshow_vfos = new System.Windows.Forms.CheckBoxTS();
            p24_chkDialDisplay_fade_rx = new System.Windows.Forms.CheckBoxTS();
            p24_chkDialDisplay_fade_tx = new System.Windows.Forms.CheckBoxTS();
            p24_chkDial_align = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilterDisplay_fadeonrx = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilterDisplay_fadeontx = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilterDisplay_fixed_tx_zoom = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilterDisplay_fixed_zoom = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilterDisplay_show_limits = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilter_characteristic = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilter_fill_spec = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilter_grey_outsidepb = new System.Windows.Forms.CheckBoxTS();
            p24_chkFilter_sideband_mode = new System.Windows.Forms.CheckBoxTS();
            p24_chkHistory_1_show_axis = new System.Windows.Forms.CheckBoxTS();
            p24_chkHistory_auto_0_scale = new System.Windows.Forms.CheckBoxTS();
            p24_chkHistory_auto_1_scale = new System.Windows.Forms.CheckBoxTS();
            p24_chkHistory_fade_rx = new System.Windows.Forms.CheckBoxTS();
            p24_chkHistory_fade_tx = new System.Windows.Forms.CheckBoxTS();
            p24_chkLedIndicator_FadeOnRX = new System.Windows.Forms.CheckBoxTS();
            p24_chkLedIndicator_FadeOnTX = new System.Windows.Forms.CheckBoxTS();
            p24_chkLedIndicator_ShowPanel = new System.Windows.Forms.CheckBoxTS();
            p24_chkLed_notx_false = new System.Windows.Forms.CheckBoxTS();
            p24_chkLed_notx_true = new System.Windows.Forms.CheckBoxTS();
            p24_chkLed_process_when_hidden = new System.Windows.Forms.CheckBoxTS();
            p24_chkLed_show_false = new System.Windows.Forms.CheckBoxTS();
            p24_chkLed_show_true = new System.Windows.Forms.CheckBoxTS();
            p24_chkLockContainer = new System.Windows.Forms.CheckBoxTS();
            p24_chkMMClockTitle = new System.Windows.Forms.CheckBoxTS();
            p24_chkMaintainNFAdjustDeltaRX1 = new System.Windows.Forms.CheckBoxTS();
            p24_chkMaintainNFAdjustDeltaRX2 = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemDarkMode = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemDarkModeRotator = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemFadeOnRx = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemFadeOnRxRotator = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemFadeOnRxSpacer = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemFadeOnTx = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemFadeOnTxRotator = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemFadeOnTxSpacer = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemHistory = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemPeakHold = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemPeakValue = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemRotatorAllowControl = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemRotatorCardinals = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemRotatorShowBeamWidth = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemSegmented = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemShadow = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemShowIndicator = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemShowSubIndicator = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemSolid = new System.Windows.Forms.CheckBoxTS();
            p24_chkMeterItemTitle = new System.Windows.Forms.CheckBoxTS();
            p24_chkMultiMeter_auto_container_height = new System.Windows.Forms.CheckBoxTS();
            p24_chkMultiMeter_vfo_show_bandtext = new System.Windows.Forms.CheckBoxTS();
            p24_chkRecording_canRepeat = new System.Windows.Forms.CheckBoxTS();
            p24_chkRecording_globalkeybind = new System.Windows.Forms.CheckBoxTS();
            p24_chkRecording_ignore_play_tempchanges = new System.Windows.Forms.CheckBoxTS();
            p24_chkRecording_ignore_record_tempchanges = new System.Windows.Forms.CheckBoxTS();
            p24_chkRecording_playkeybind = new System.Windows.Forms.CheckBoxTS();
            p24_chkRecording_slot_locked = new System.Windows.Forms.CheckBoxTS();
            p24_chkTextOverlay_FadeOnRX = new System.Windows.Forms.CheckBoxTS();
            p24_chkTextOverlay_FadeOnTX = new System.Windows.Forms.CheckBoxTS();
            p24_chkTextOverlay_ShowPanel = new System.Windows.Forms.CheckBoxTS();
            p24_chkTextOverlay_rx_on_led = new System.Windows.Forms.CheckBoxTS();
            p24_chkTextOverlay_textback1 = new System.Windows.Forms.CheckBoxTS();
            p24_chkTextOverlay_textback2 = new System.Windows.Forms.CheckBoxTS();
            p24_chkTextOverlay_tx_on_led = new System.Windows.Forms.CheckBoxTS();
            p24_chkWaveRecord_fade_rx = new System.Windows.Forms.CheckBoxTS();
            p24_chkWaveRecord_fade_tx = new System.Windows.Forms.CheckBoxTS();
            p24_chkWebImage_background = new System.Windows.Forms.CheckBoxTS();
            p24_chkWebImage_bypass_cache = new System.Windows.Forms.CheckBoxTS();
            p24_chkWebImage_fade_rx = new System.Windows.Forms.CheckBoxTS();
            p24_chkWebImage_fade_tx = new System.Windows.Forms.CheckBoxTS();
            p24_clrbtnBandButtons_border = new PowerSDR.ColorButton();
            p24_clrbtnBandButtons_fill = new PowerSDR.ColorButton();
            p24_clrbtnBandButtons_hover = new PowerSDR.ColorButton();
            p24_clrbtnBandButtons_indicator_off = new PowerSDR.ColorButton();
            p24_clrbtnBandButtons_indicator_on = new PowerSDR.ColorButton();
            p24_clrbtnButonBox_click = new PowerSDR.ColorButton();
            p24_clrbtnButonBox_fontcolour = new PowerSDR.ColorButton();
            p24_clrbtnContainerBackground = new PowerSDR.ColorButton();
            p24_clrbtnDial_button_highlight = new PowerSDR.ColorButton();
            p24_clrbtnDial_button_off = new PowerSDR.ColorButton();
            p24_clrbtnDial_button_on = new PowerSDR.ColorButton();
            p24_clrbtnDial_circle = new PowerSDR.ColorButton();
            p24_clrbtnDial_fast = new PowerSDR.ColorButton();
            p24_clrbtnDial_hold = new PowerSDR.ColorButton();
            p24_clrbtnDial_pad = new PowerSDR.ColorButton();
            p24_clrbtnDial_pad_pressed = new PowerSDR.ColorButton();
            p24_clrbtnDial_ring = new PowerSDR.ColorButton();
            p24_clrbtnDial_slow = new PowerSDR.ColorButton();
            p24_clrbtnDial_text = new PowerSDR.ColorButton();
            p24_clrbtnFilterDisplay_backcolour = new PowerSDR.ColorButton();
            p24_clrbtnFilter_button_highlight = new PowerSDR.ColorButton();
            p24_clrbtnFilter_data_fill = new PowerSDR.ColorButton();
            p24_clrbtnFilter_data_line = new PowerSDR.ColorButton();
            p24_clrbtnFilter_edge_highlight = new PowerSDR.ColorButton();
            p24_clrbtnFilter_edges = new PowerSDR.ColorButton();
            p24_clrbtnFilter_edges_tx = new PowerSDR.ColorButton();
            p24_clrbtnFilter_extents = new PowerSDR.ColorButton();
            p24_clrbtnFilter_meter_back = new PowerSDR.ColorButton();
            p24_clrbtnFilter_notch = new PowerSDR.ColorButton();
            p24_clrbtnFilter_notch_highlight = new PowerSDR.ColorButton();
            p24_clrbtnFilter_number_highlight = new PowerSDR.ColorButton();
            p24_clrbtnFilter_setting_on = new PowerSDR.ColorButton();
            p24_clrbtnFilter_snap_line = new PowerSDR.ColorButton();
            p24_clrbtnFilter_text = new PowerSDR.ColorButton();
            p24_clrbtnFilter_wf_low = new PowerSDR.ColorButton();
            p24_clrbtnHistory_background = new PowerSDR.ColorButton();
            p24_clrbtnHistory_colour_0 = new PowerSDR.ColorButton();
            p24_clrbtnHistory_colour_1 = new PowerSDR.ColorButton();
            p24_clrbtnHistory_lines = new PowerSDR.ColorButton();
            p24_clrbtnHistory_time = new PowerSDR.ColorButton();
            p24_clrbtnLedIndicator_PanelBackground = new PowerSDR.ColorButton();
            p24_clrbtnLedIndicator_PanelBackgroundTX = new PowerSDR.ColorButton();
            p24_clrbtnLedIndicator_false = new PowerSDR.ColorButton();
            p24_clrbtnLedIndicator_true = new PowerSDR.ColorButton();
            p24_clrbtnMMClockBackground = new PowerSDR.ColorButton();
            p24_clrbtnMMClockTitle = new PowerSDR.ColorButton();
            p24_clrbtnMMDate = new PowerSDR.ColorButton();
            p24_clrbtnMMTime = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDigitHighlight = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayBackground = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayBand = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayFilter = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayFrequency = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayFrequency_small = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayMode = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayRx = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplaySplit = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplaySplitBack = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayTitle = new PowerSDR.ColorButton();
            p24_clrbtnMMVfoDisplayTx = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemHBackground = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemHBackgroundRotator = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemHBackgroundSpacerRX = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemHBackgroundSpacerTX = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemHigh = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemHistory = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemIndicator = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemLow = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemMeterTitle = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemPeakHold = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemPeakValueColour = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemPowerScale = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemRotatorArrow = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemRotatorBeamWidth = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemRotatorControlColour = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemRotatorLargeDot = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemRotatorSmallDot = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemRotatorText = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemSegmentedSolidColourHigh = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemSegmentedSolidColourLow = new PowerSDR.ColorButton();
            p24_clrbtnMeterItemSubIndicator = new PowerSDR.ColorButton();
            p24_clrbtnMultiMeter_vfo_lock = new PowerSDR.ColorButton();
            p24_clrbtnMultiMeter_vfo_show_bandtext = new PowerSDR.ColorButton();
            p24_clrbtnMultiMeter_vfo_sync = new PowerSDR.ColorButton();
            p24_clrbtnTextOverlay_PanelBackground = new PowerSDR.ColorButton();
            p24_clrbtnTextOverlay_PanelBackgroundTX = new PowerSDR.ColorButton();
            p24_clrbtnTextOverlay_TextBackColour1 = new PowerSDR.ColorButton();
            p24_clrbtnTextOverlay_TextBackColour2 = new PowerSDR.ColorButton();
            p24_clrbtnTextOverlay_TextColour1 = new PowerSDR.ColorButton();
            p24_clrbtnTextOverlay_TextColour2 = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_back = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_border = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_button_border = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_button_fill = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_button_hover = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_delete = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_play = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_row = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_scroll_hover = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_scroll_thumb = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_scroll_track = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_stop = new PowerSDR.ColorButton();
            p24_clrbtnWaveRecord_text = new PowerSDR.ColorButton();
            p24_comboContainerSelect = new System.Windows.Forms.ComboBoxTS();
            p24_comboFilter_wf_palette = new System.Windows.Forms.ComboBoxTS();
            p24_comboHistory_reading_0 = new System.Windows.Forms.ComboBoxTS();
            p24_comboHistory_reading_1 = new System.Windows.Forms.ComboBoxTS();
            p24_comboWebImage_BsdWorld = new System.Windows.Forms.ComboBoxTS();
            p24_comboWebImage_HamQsl = new System.Windows.Forms.ComboBoxTS();
            p24_comboWebImage_nasa = new System.Windows.Forms.ComboBoxTS();
            p24_comboWebImage_noaa = new System.Windows.Forms.ComboBoxTS();
            p24_groupBoxTS40 = new System.Windows.Forms.GroupBoxTS();
            p24_groupBoxTS41 = new System.Windows.Forms.GroupBoxTS();
            p24_groupBoxTS42 = new System.Windows.Forms.GroupBoxTS();
            p24_groupBoxTS43 = new System.Windows.Forms.GroupBoxTS();
            p24_groupBoxTS45 = new System.Windows.Forms.GroupBoxTS();
            p24_groupBoxTS46 = new System.Windows.Forms.GroupBoxTS();
            p24_grpButtonBox = new System.Windows.Forms.GroupBoxTS();
            p24_grpDialDisplay = new System.Windows.Forms.GroupBoxTS();
            p24_grpGlobalStopPlayRecord = new System.Windows.Forms.GroupBoxTS();
            p24_grpHistoryItem = new System.Windows.Forms.GroupBoxTS();
            p24_grpLedIndicator = new System.Windows.Forms.GroupBoxTS();
            p24_grpMeterItemClockSettings = new System.Windows.Forms.GroupBoxTS();
            p24_grpMeterItemDataOutNode = new System.Windows.Forms.GroupBoxTS();
            p24_grpMeterItemFilterDisplay = new System.Windows.Forms.GroupBoxTS();
            p24_grpMeterItemRotator = new System.Windows.Forms.GroupBoxTS();
            p24_grpMeterItemSettings = new System.Windows.Forms.GroupBoxTS();
            p24_grpMeterItemSpacerSettings = new System.Windows.Forms.GroupBoxTS();
            p24_grpMeterItemVfoDisplaySettings = new System.Windows.Forms.GroupBoxTS();
            p24_grpMultiMeterHolder = new System.Windows.Forms.GroupBoxTS();
            p24_grpTextOverlay = new System.Windows.Forms.GroupBoxTS();
            p24_grpWaveRecordItem = new System.Windows.Forms.GroupBoxTS();
            p24_grpWebImage = new System.Windows.Forms.GroupBoxTS();
            p24_label22 = new System.Windows.Forms.Label();
            p24_labelTS162 = new System.Windows.Forms.LabelTS();
            p24_labelTS163 = new System.Windows.Forms.LabelTS();
            p24_labelTS164 = new System.Windows.Forms.LabelTS();
            p24_labelTS166 = new System.Windows.Forms.LabelTS();
            p24_labelTS167 = new System.Windows.Forms.LabelTS();
            p24_labelTS168 = new System.Windows.Forms.LabelTS();
            p24_labelTS169 = new System.Windows.Forms.LabelTS();
            p24_labelTS170 = new System.Windows.Forms.LabelTS();
            p24_labelTS171 = new System.Windows.Forms.LabelTS();
            p24_labelTS172 = new System.Windows.Forms.LabelTS();
            p24_labelTS173 = new System.Windows.Forms.LabelTS();
            p24_labelTS174 = new System.Windows.Forms.LabelTS();
            p24_labelTS175 = new System.Windows.Forms.LabelTS();
            p24_labelTS176 = new System.Windows.Forms.LabelTS();
            p24_labelTS177 = new System.Windows.Forms.LabelTS();
            p24_labelTS196 = new System.Windows.Forms.LabelTS();
            p24_labelTS197 = new System.Windows.Forms.LabelTS();
            p24_labelTS199 = new System.Windows.Forms.LabelTS();
            p24_labelTS200 = new System.Windows.Forms.LabelTS();
            p24_labelTS201 = new System.Windows.Forms.LabelTS();
            p24_labelTS202 = new System.Windows.Forms.LabelTS();
            p24_labelTS203 = new System.Windows.Forms.LabelTS();
            p24_labelTS204 = new System.Windows.Forms.LabelTS();
            p24_labelTS205 = new System.Windows.Forms.LabelTS();
            p24_labelTS206 = new System.Windows.Forms.LabelTS();
            p24_labelTS207 = new System.Windows.Forms.LabelTS();
            p24_labelTS208 = new System.Windows.Forms.LabelTS();
            p24_labelTS209 = new System.Windows.Forms.LabelTS();
            p24_labelTS210 = new System.Windows.Forms.LabelTS();
            p24_labelTS212 = new System.Windows.Forms.LabelTS();
            p24_labelTS215 = new System.Windows.Forms.LabelTS();
            p24_labelTS216 = new System.Windows.Forms.LabelTS();
            p24_labelTS217 = new System.Windows.Forms.LabelTS();
            p24_labelTS218 = new System.Windows.Forms.LabelTS();
            p24_labelTS219 = new System.Windows.Forms.LabelTS();
            p24_labelTS220 = new System.Windows.Forms.LabelTS();
            p24_labelTS221 = new System.Windows.Forms.LabelTS();
            p24_labelTS222 = new System.Windows.Forms.LabelTS();
            p24_labelTS223 = new System.Windows.Forms.LabelTS();
            p24_labelTS224 = new System.Windows.Forms.LabelTS();
            p24_labelTS225 = new System.Windows.Forms.LabelTS();
            p24_labelTS226 = new System.Windows.Forms.LabelTS();
            p24_labelTS227 = new System.Windows.Forms.LabelTS();
            p24_labelTS228 = new System.Windows.Forms.LabelTS();
            p24_labelTS229 = new System.Windows.Forms.LabelTS();
            p24_labelTS230 = new System.Windows.Forms.LabelTS();
            p24_labelTS231 = new System.Windows.Forms.LabelTS();
            p24_labelTS232 = new System.Windows.Forms.LabelTS();
            p24_labelTS233 = new System.Windows.Forms.LabelTS();
            p24_labelTS234 = new System.Windows.Forms.LabelTS();
            p24_labelTS235 = new System.Windows.Forms.LabelTS();
            p24_labelTS236 = new System.Windows.Forms.LabelTS();
            p24_labelTS237 = new System.Windows.Forms.LabelTS();
            p24_labelTS238 = new System.Windows.Forms.LabelTS();
            p24_labelTS239 = new System.Windows.Forms.LabelTS();
            p24_labelTS240 = new System.Windows.Forms.LabelTS();
            p24_labelTS241 = new System.Windows.Forms.LabelTS();
            p24_labelTS242 = new System.Windows.Forms.LabelTS();
            p24_labelTS243 = new System.Windows.Forms.LabelTS();
            p24_labelTS244 = new System.Windows.Forms.LabelTS();
            p24_labelTS245 = new System.Windows.Forms.LabelTS();
            p24_labelTS246 = new System.Windows.Forms.LabelTS();
            p24_labelTS247 = new System.Windows.Forms.LabelTS();
            p24_labelTS248 = new System.Windows.Forms.LabelTS();
            p24_labelTS249 = new System.Windows.Forms.LabelTS();
            p24_labelTS250 = new System.Windows.Forms.LabelTS();
            p24_labelTS251 = new System.Windows.Forms.LabelTS();
            p24_labelTS252 = new System.Windows.Forms.LabelTS();
            p24_labelTS253 = new System.Windows.Forms.LabelTS();
            p24_labelTS258 = new System.Windows.Forms.LabelTS();
            p24_labelTS259 = new System.Windows.Forms.LabelTS();
            p24_labelTS260 = new System.Windows.Forms.LabelTS();
            p24_labelTS261 = new System.Windows.Forms.LabelTS();
            p24_labelTS262 = new System.Windows.Forms.LabelTS();
            p24_labelTS263 = new System.Windows.Forms.LabelTS();
            p24_labelTS264 = new System.Windows.Forms.LabelTS();
            p24_labelTS265 = new System.Windows.Forms.LabelTS();
            p24_labelTS278 = new System.Windows.Forms.LabelTS();
            p24_labelTS279 = new System.Windows.Forms.LabelTS();
            p24_labelTS280 = new System.Windows.Forms.LabelTS();
            p24_labelTS281 = new System.Windows.Forms.LabelTS();
            p24_labelTS287 = new System.Windows.Forms.LabelTS();
            p24_labelTS292 = new System.Windows.Forms.LabelTS();
            p24_labelTS293 = new System.Windows.Forms.LabelTS();
            p24_labelTS294 = new System.Windows.Forms.LabelTS();
            p24_labelTS296 = new System.Windows.Forms.LabelTS();
            p24_labelTS297 = new System.Windows.Forms.LabelTS();
            p24_labelTS298 = new System.Windows.Forms.LabelTS();
            p24_labelTS299 = new System.Windows.Forms.LabelTS();
            p24_labelTS300 = new System.Windows.Forms.LabelTS();
            p24_labelTS301 = new System.Windows.Forms.LabelTS();
            p24_labelTS330 = new System.Windows.Forms.LabelTS();
            p24_labelTS331 = new System.Windows.Forms.LabelTS();
            p24_labelTS332 = new System.Windows.Forms.LabelTS();
            p24_labelTS333 = new System.Windows.Forms.LabelTS();
            p24_labelTS334 = new System.Windows.Forms.LabelTS();
            p24_labelTS335 = new System.Windows.Forms.LabelTS();
            p24_labelTS336 = new System.Windows.Forms.LabelTS();
            p24_labelTS337 = new System.Windows.Forms.LabelTS();
            p24_labelTS338 = new System.Windows.Forms.LabelTS();
            p24_labelTS339 = new System.Windows.Forms.LabelTS();
            p24_labelTS341 = new System.Windows.Forms.LabelTS();
            p24_labelTS344 = new System.Windows.Forms.LabelTS();
            p24_labelTS345 = new System.Windows.Forms.LabelTS();
            p24_labelTS346 = new System.Windows.Forms.LabelTS();
            p24_labelTS347 = new System.Windows.Forms.LabelTS();
            p24_labelTS348 = new System.Windows.Forms.LabelTS();
            p24_labelTS349 = new System.Windows.Forms.LabelTS();
            p24_labelTS350 = new System.Windows.Forms.LabelTS();
            p24_labelTS351 = new System.Windows.Forms.LabelTS();
            p24_labelTS352 = new System.Windows.Forms.LabelTS();
            p24_labelTS353 = new System.Windows.Forms.LabelTS();
            p24_labelTS354 = new System.Windows.Forms.LabelTS();
            p24_labelTS407 = new System.Windows.Forms.LabelTS();
            p24_labelTS408 = new System.Windows.Forms.LabelTS();
            p24_labelTS409 = new System.Windows.Forms.LabelTS();
            p24_labelTS410 = new System.Windows.Forms.LabelTS();
            p24_labelTS411 = new System.Windows.Forms.LabelTS();
            p24_labelTS412 = new System.Windows.Forms.LabelTS();
            p24_labelTS413 = new System.Windows.Forms.LabelTS();
            p24_labelTS414 = new System.Windows.Forms.LabelTS();
            p24_labelTS415 = new System.Windows.Forms.LabelTS();
            p24_labelTS416 = new System.Windows.Forms.LabelTS();
            p24_labelTS417 = new System.Windows.Forms.LabelTS();
            p24_labelTS418 = new System.Windows.Forms.LabelTS();
            p24_labelTS419 = new System.Windows.Forms.LabelTS();
            p24_labelTS420 = new System.Windows.Forms.LabelTS();
            p24_labelTS421 = new System.Windows.Forms.LabelTS();
            p24_labelTS422 = new System.Windows.Forms.LabelTS();
            p24_labelTS424 = new System.Windows.Forms.LabelTS();
            p24_labelTS428 = new System.Windows.Forms.LabelTS();
            p24_labelTS429 = new System.Windows.Forms.LabelTS();
            p24_labelTS431 = new System.Windows.Forms.LabelTS();
            p24_labelTS432 = new System.Windows.Forms.LabelTS();
            p24_labelTS433 = new System.Windows.Forms.LabelTS();
            p24_labelTS482 = new System.Windows.Forms.LabelTS();
            p24_labelTS650 = new System.Windows.Forms.LabelTS();
            p24_labelTS651 = new System.Windows.Forms.LabelTS();
            p24_labelTS652 = new System.Windows.Forms.LabelTS();
            p24_labelTS653 = new System.Windows.Forms.LabelTS();
            p24_labelTS654 = new System.Windows.Forms.LabelTS();
            p24_labelTS655 = new System.Windows.Forms.LabelTS();
            p24_labelTS657 = new System.Windows.Forms.LabelTS();
            p24_labelTS658 = new System.Windows.Forms.LabelTS();
            p24_lblBandButtons_indicator_border = new System.Windows.Forms.LabelTS();
            p24_lblBandButtons_indicator_style = new System.Windows.Forms.LabelTS();
            p24_lblLedIndicator_panelbackground = new System.Windows.Forms.LabelTS();
            p24_lblLedIndicator_panelbackgroundTX = new System.Windows.Forms.LabelTS();
            p24_lblLed_Valid = new System.Windows.Forms.LabelTS();
            p24_lblMMBackground = new System.Windows.Forms.LabelTS();
            p24_lblMMClockBackground = new System.Windows.Forms.LabelTS();
            p24_lblMMContainerBackground = new System.Windows.Forms.LabelTS();
            p24_lblMMContainerNotes = new System.Windows.Forms.LabelTS();
            p24_lblMMEyeBezelSize = new System.Windows.Forms.LabelTS();
            p24_lblMMEyeSize = new System.Windows.Forms.LabelTS();
            p24_lblMMHigh = new System.Windows.Forms.LabelTS();
            p24_lblMMHistory = new System.Windows.Forms.LabelTS();
            p24_lblMMHistoryIgnore = new System.Windows.Forms.LabelTS();
            p24_lblMMIndicator = new System.Windows.Forms.LabelTS();
            p24_lblMMIndicatorSub = new System.Windows.Forms.LabelTS();
            p24_lblMMLow = new System.Windows.Forms.LabelTS();
            p24_lblMMPowerLimit = new System.Windows.Forms.LabelTS();
            p24_lblMMsegSolHigh = new System.Windows.Forms.LabelTS();
            p24_lblMMsegSolLow = new System.Windows.Forms.LabelTS();
            p24_lblMeterItemRotatorAZcommand = new System.Windows.Forms.LabelTS();
            p24_lblMeterItemRotatorBeamWidth_alpha = new System.Windows.Forms.LabelTS();
            p24_lblMeterItemRotatorBeamWidth_degrees = new System.Windows.Forms.LabelTS();
            p24_lblMeterItemRotatorELEcommand = new System.Windows.Forms.LabelTS();
            p24_lblRotator_4charID = new System.Windows.Forms.LabelTS();
            p24_lblTextOverlay_panelbackground = new System.Windows.Forms.LabelTS();
            p24_lblTextOverlay_panelbackgroundTX = new System.Windows.Forms.LabelTS();
            p24_lblTextOverlay_panelpadding = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordBack = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordBorder = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordButtonBorder = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordButtonFill = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordButtonHover = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordDelete = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordHeightRatio = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordPlay = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordRadius = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordRow = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordScrollHover = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordScrollThumb = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordScrollTrack = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordStop = new System.Windows.Forms.LabelTS();
            p24_lblWaveRecordText = new System.Windows.Forms.LabelTS();
            p24_lblWebImage_after = new System.Windows.Forms.LabelTS();
            p24_lblWebImage_secs = new System.Windows.Forms.LabelTS();
            p24_lblWebImage_state = new System.Windows.Forms.LabelTS();
            p24_lstMetersAvailable = new System.Windows.Forms.ListBox();
            p24_lstMetersInUse = new System.Windows.Forms.ListBox();
            p24_nudBandButtons_border = new System.Windows.Forms.NumericUpDownTS();
            p24_nudBandButtons_columns = new System.Windows.Forms.NumericUpDownTS();
            p24_nudBandButtons_height_ratio = new System.Windows.Forms.NumericUpDownTS();
            p24_nudBandButtons_indicator_border = new System.Windows.Forms.NumericUpDownTS();
            p24_nudBandButtons_indicator_style = new System.Windows.Forms.NumericUpDownTS();
            p24_nudBandButtons_margin = new System.Windows.Forms.NumericUpDownTS();
            p24_nudBandButtons_radius = new System.Windows.Forms.NumericUpDownTS();
            p24_nudButtonBox_font_scale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudButtonBox_font_x_shift = new System.Windows.Forms.NumericUpDownTS();
            p24_nudButtonBox_font_y_shift = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDataOutNode_sendinterval = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDialDisplay_font_scale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDialDisplay_vertical_ratio = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDial_decrement = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDial_degrees_for_change = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDial_increment = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDial_interval = new System.Windows.Forms.NumericUpDownTS();
            p24_nudDial_max_increments = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilterDisplay_fixed_tx_zoom_level = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilterDisplay_fixed_zoom_level = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilterDisplay_vertical_ratio = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilterItem_cw_scale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilterItem_font_scale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilterItem_others_scale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilterItem_sidebands_scale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilter_lower_characteristic = new System.Windows.Forms.NumericUpDownTS();
            p24_nudFilter_waterfall_frame_update = new System.Windows.Forms.NumericUpDownTS();
            p24_nudHistory_axis0_max = new System.Windows.Forms.NumericUpDownTS();
            p24_nudHistory_axis0_min = new System.Windows.Forms.NumericUpDownTS();
            p24_nudHistory_axis1_max = new System.Windows.Forms.NumericUpDownTS();
            p24_nudHistory_axis1_min = new System.Windows.Forms.NumericUpDownTS();
            p24_nudHistory_keep_for = new System.Windows.Forms.NumericUpDownTS();
            p24_nudHistory_update = new System.Windows.Forms.NumericUpDownTS();
            p24_nudHistory_vertical_ratio = new System.Windows.Forms.NumericUpDownTS();
            p24_nudLedIndicator_PanelPadding = new System.Windows.Forms.NumericUpDownTS();
            p24_nudLedIndicator_UpdateInterval = new System.Windows.Forms.NumericUpDownTS();
            p24_nudLedIndicator_xOffset = new System.Windows.Forms.NumericUpDownTS();
            p24_nudLedIndicator_xSize = new System.Windows.Forms.NumericUpDownTS();
            p24_nudLedIndicator_yOffset = new System.Windows.Forms.NumericUpDownTS();
            p24_nudLedIndicator_ySize = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemAttackRate = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemDecayRate = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemEyeBezelScale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemEyeScale = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemHistoryDuration = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemIgnoreHistoryDuration = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemRotatorBeamWidth = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemRotatorBeamWidth_alpha = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemRotator_padding = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemSpacerPadding = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemUpdateRate = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemUpdateRateRotator = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItem_custom_high = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItem_custom_max = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItem_custom_min = new System.Windows.Forms.NumericUpDownTS();
            p24_nudMeterItemsPowerLimit = new System.Windows.Forms.NumericUpDownTS();
            p24_nudRecording_repeatDelay = new System.Windows.Forms.NumericUpDownTS();
            p24_nudRecording_slot_settings = new System.Windows.Forms.NumericUpDownTS();
            p24_nudRecording_tx_gain_adjust = new System.Windows.Forms.NumericUpDownTS();
            p24_nudTextOverlay_PanelPadding = new System.Windows.Forms.NumericUpDownTS();
            p24_nudTextOverlay_RXxOffset = new System.Windows.Forms.NumericUpDownTS();
            p24_nudTextOverlay_RXyOffset = new System.Windows.Forms.NumericUpDownTS();
            p24_nudTextOverlay_TXxOffset = new System.Windows.Forms.NumericUpDownTS();
            p24_nudTextOverlay_TXyOffset = new System.Windows.Forms.NumericUpDownTS();
            p24_nudVoiceRecordingPlayback_slots = new System.Windows.Forms.NumericUpDownTS();
            p24_nudWaveRecord_radius = new System.Windows.Forms.NumericUpDownTS();
            p24_nudWaveRecord_vertical_ratio = new System.Windows.Forms.NumericUpDownTS();
            p24_nudWebImage_background_time = new System.Windows.Forms.NumericUpDownTS();
            p24_nudWebImage_update_interval = new System.Windows.Forms.NumericUpDownTS();
            p24_nudWebImage_width_scale = new System.Windows.Forms.NumericUpDownTS();
            p24_picButtonBoxInfo = new System.Windows.Forms.PictureBox();
            p24_picMultiMeterRotatorControlInfo = new System.Windows.Forms.PictureBox();
            p24_pnlButtonBox_antenna_toggles = new System.Windows.Forms.PanelTS();
            p24_pnlFilterModeModifiers = new System.Windows.Forms.PanelTS();
            p24_pnlMeterItemSettings = new System.Windows.Forms.PanelTS();
            p24_pnlMeterItemSettings_custom = new System.Windows.Forms.PanelTS();
            p24_pnlVariableInUse_1 = new System.Windows.Forms.PanelTS();
            p24_pnlVariableInUse_1_history = new System.Windows.Forms.PanelTS();
            p24_pnlVariableInUse_1_rotator = new System.Windows.Forms.PanelTS();
            p24_pnlVariableInUse_2 = new System.Windows.Forms.PanelTS();
            p24_pnlVariableInUse_2_history = new System.Windows.Forms.PanelTS();
            p24_pnlVariableInUse_2_rotator = new System.Windows.Forms.PanelTS();
            p24_pnlVoiceRecordPlayback = new System.Windows.Forms.PanelTS();
            p24_radContainer_rx1_data = new System.Windows.Forms.RadioButtonTS();
            p24_radContainer_rx2_data = new System.Windows.Forms.RadioButtonTS();
            p24_radFilterItem_none = new System.Windows.Forms.RadioButtonTS();
            p24_radFilterItem_panadaptor = new System.Windows.Forms.RadioButtonTS();
            p24_radFilterItem_panafall = new System.Windows.Forms.RadioButtonTS();
            p24_radFilterItem_waterfall = new System.Windows.Forms.RadioButtonTS();
            p24_radLed_light_blink = new System.Windows.Forms.RadioButtonTS();
            p24_radLed_light_on_off = new System.Windows.Forms.RadioButtonTS();
            p24_radLed_light_pulsate = new System.Windows.Forms.RadioButtonTS();
            p24_radMM12Clock = new System.Windows.Forms.RadioButtonTS();
            p24_radMM24Clock = new System.Windows.Forms.RadioButtonTS();
            p24_radMeterItemRotator_show_az = new System.Windows.Forms.RadioButtonTS();
            p24_radMeterItemRotator_show_both = new System.Windows.Forms.RadioButtonTS();
            p24_radMeterItemRotator_show_ele = new System.Windows.Forms.RadioButtonTS();
            p24_radMeterItemSettings = new System.Windows.Forms.RadioButtonTS();
            p24_radMeterItemSettings_custom = new System.Windows.Forms.RadioButtonTS();
            p24_radMultiMeter_vfo_display_both = new System.Windows.Forms.RadioButtonTS();
            p24_radMultiMeter_vfo_display_vfoa = new System.Windows.Forms.RadioButtonTS();
            p24_radMultiMeter_vfo_display_vfob = new System.Windows.Forms.RadioButtonTS();
            p24_scrlFilter = new System.Windows.Forms.ScrollableControl();
            p24_scrollableControl1 = new System.Windows.Forms.ScrollableControl();
            p24_scrollableControl2 = new System.Windows.Forms.ScrollableControl();
            p24_tbMeterItemHistoryAlpha = new System.Windows.Forms.TrackBarTS();
            p24_tpAppearanceMeter2 = new System.Windows.Forms.TabPage();
            p24_txtContainerNotes = new System.Windows.Forms.TextBoxTS();
            p24_txtDataOutNode_4charID = new System.Windows.Forms.TextBoxTS();
            p24_txtLedIndicator_4char = new System.Windows.Forms.TextBoxTS();
            p24_txtLedIndicator_condition = new System.Windows.Forms.TextBoxTS();
            p24_txtMeterItemRotatorAZcommand = new System.Windows.Forms.TextBoxTS();
            p24_txtMeterItemRotatorELEcommand = new System.Windows.Forms.TextBoxTS();
            p24_txtMeterItemRotatorSTOPcommand = new System.Windows.Forms.TextBoxTS();
            p24_txtMeterItem_custom_title = new System.Windows.Forms.TextBoxTS();
            p24_txtMeterItem_custom_units = new System.Windows.Forms.TextBoxTS();
            p24_txtRecording_4char = new System.Windows.Forms.TextBoxTS();
            p24_txtRecording_globalkeybind = new System.Windows.Forms.TextBoxTS();
            p24_txtRecording_labelText = new System.Windows.Forms.TextBoxTS();
            p24_txtRecording_playkeybind = new System.Windows.Forms.TextBoxTS();
            p24_txtRotator_4charID = new System.Windows.Forms.TextBoxTS();
            p24_txtTextOverlay_RXText = new System.Windows.Forms.TextBoxTS();
            p24_txtTextOverlay_TXText = new System.Windows.Forms.TextBoxTS();
            p24_txtTextOverlay_rx_on_led_4char = new System.Windows.Forms.TextBoxTS();
            p24_txtTextOverlay_tx_on_led_4char = new System.Windows.Forms.TextBoxTS();
            p24_txtWebImage_4char = new System.Windows.Forms.TextBoxTS();
            p24_txtWebImage_background_4char = new System.Windows.Forms.TextBoxTS();
            p24_txtWebImage_url = new System.Windows.Forms.TextBoxTS();
            p24_ucMeterItemSignalType = new PowerSDR.ucSignalSelect();
            p24_ucOtherButtonsOptionsGrid_buttons = new PowerSDR.ucOtherButtonsOptionsGrid();
            p24_ucTunestepOptionsGrid_buttons = new PowerSDR.ucTunestepOptionsGrid();

            // 
            // p24_bntMultiMeterItemRotator_default_pstRotator
            // 
            this.p24_bntMultiMeterItemRotator_default_pstRotator.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_bntMultiMeterItemRotator_default_pstRotator.Image = null;
            this.p24_bntMultiMeterItemRotator_default_pstRotator.Location = new System.Drawing.Point(222, 245);
            this.p24_bntMultiMeterItemRotator_default_pstRotator.Name = "p24_bntMultiMeterItemRotator_default_pstRotator";
            this.p24_bntMultiMeterItemRotator_default_pstRotator.TabStop = true;
            this.p24_bntMultiMeterItemRotator_default_pstRotator.Size = new System.Drawing.Size(63, 23);
            this.p24_bntMultiMeterItemRotator_default_pstRotator.TabIndex = 166;
            this.p24_bntMultiMeterItemRotator_default_pstRotator.Text = "pstRotator";
            this.toolTip1.SetToolTip(this.p24_bntMultiMeterItemRotator_default_pstRotator, "Reset for PST Rotator");
            this.p24_bntMultiMeterItemRotator_default_pstRotator.UseVisualStyleBackColor = true;
            this.p24_bntMultiMeterItemRotator_default_pstRotator.Click += new System.EventHandler(this.bntMultiMeterItemRotator_default_pstRotator_Click);

            // 
            // p24_btnAddMeterItem
            // 
            this.p24_btnAddMeterItem.Image = global::PowerSDR.P24MeterResources.arrow_right_black;
            this.p24_btnAddMeterItem.Location = new System.Drawing.Point(153, 174);
            this.p24_btnAddMeterItem.Name = "p24_btnAddMeterItem";
            this.p24_btnAddMeterItem.TabStop = true;
            this.p24_btnAddMeterItem.Size = new System.Drawing.Size(32, 32);
            this.p24_btnAddMeterItem.TabIndex = 92;
            this.toolTip1.SetToolTip(this.p24_btnAddMeterItem, "Include the item");
            this.p24_btnAddMeterItem.UseVisualStyleBackColor = true;
            this.p24_btnAddMeterItem.Click += new System.EventHandler(this.btnAddMeterItem_Click);

            // 
            // p24_btnAddRX1Container
            // 
            this.p24_btnAddRX1Container.Image = null;
            this.p24_btnAddRX1Container.Location = new System.Drawing.Point(209, 13);
            this.p24_btnAddRX1Container.Name = "p24_btnAddRX1Container";
            this.p24_btnAddRX1Container.TabStop = true;
            this.p24_btnAddRX1Container.Size = new System.Drawing.Size(71, 44);
            this.p24_btnAddRX1Container.TabIndex = 0;
            this.p24_btnAddRX1Container.Text = "Add\r\nContainer";
            this.toolTip1.SetToolTip(this.p24_btnAddRX1Container, "Add a meter item container");
            this.p24_btnAddRX1Container.UseVisualStyleBackColor = true;
            this.p24_btnAddRX1Container.Click += new System.EventHandler(this.btnAddRX1Container_Click);

            // 
            // p24_btnBandButtons_font
            // 
            this.p24_btnBandButtons_font.Image = null;
            this.p24_btnBandButtons_font.Location = new System.Drawing.Point(241, 61);
            this.p24_btnBandButtons_font.Name = "p24_btnBandButtons_font";
            this.p24_btnBandButtons_font.TabStop = true;
            this.p24_btnBandButtons_font.Size = new System.Drawing.Size(56, 23);
            this.p24_btnBandButtons_font.TabIndex = 137;
            this.p24_btnBandButtons_font.Text = "Font";
            this.p24_btnBandButtons_font.UseVisualStyleBackColor = true;
            this.p24_btnBandButtons_font.Click += new System.EventHandler(this.btnBandButtons_font_Click);

            // 
            // p24_btnContainerDelete
            // 
            this.p24_btnContainerDelete.Image = null;
            this.p24_btnContainerDelete.Location = new System.Drawing.Point(209, 119);
            this.p24_btnContainerDelete.Name = "p24_btnContainerDelete";
            this.p24_btnContainerDelete.TabStop = true;
            this.p24_btnContainerDelete.Size = new System.Drawing.Size(71, 44);
            this.p24_btnContainerDelete.TabIndex = 88;
            this.p24_btnContainerDelete.Text = "Remove Container";
            this.toolTip1.SetToolTip(this.p24_btnContainerDelete, "Removes the selected container and all meter items contained within");
            this.p24_btnContainerDelete.UseVisualStyleBackColor = true;
            this.p24_btnContainerDelete.Click += new System.EventHandler(this.btnContainerDelete_Click);

            // 
            // p24_btnContainer_dupe
            // 
            this.p24_btnContainer_dupe.Image = global::PowerSDR.P24MeterResources.cont_copy;
            this.p24_btnContainer_dupe.Location = new System.Drawing.Point(153, 278);
            this.p24_btnContainer_dupe.Name = "p24_btnContainer_dupe";
            this.p24_btnContainer_dupe.TabStop = true;
            this.p24_btnContainer_dupe.Size = new System.Drawing.Size(32, 32);
            this.p24_btnContainer_dupe.TabIndex = 117;
            this.toolTip1.SetToolTip(this.p24_btnContainer_dupe, "Duplicate the current container");
            this.p24_btnContainer_dupe.UseVisualStyleBackColor = true;
            this.p24_btnContainer_dupe.Click += new System.EventHandler(this.btnContainer_dupe_Click);

            // 
            // p24_btnContainer_load
            // 
            this.p24_btnContainer_load.Image = global::PowerSDR.P24MeterResources.cont_load;
            this.p24_btnContainer_load.Location = new System.Drawing.Point(153, 316);
            this.p24_btnContainer_load.Name = "p24_btnContainer_load";
            this.p24_btnContainer_load.TabStop = true;
            this.p24_btnContainer_load.Size = new System.Drawing.Size(32, 32);
            this.p24_btnContainer_load.TabIndex = 116;
            this.toolTip1.SetToolTip(this.p24_btnContainer_load, "Load a container file");
            this.p24_btnContainer_load.UseVisualStyleBackColor = true;
            this.p24_btnContainer_load.Click += new System.EventHandler(this.btnContainer_load_Click);

            // 
            // p24_btnContainer_save
            // 
            this.p24_btnContainer_save.Image = global::PowerSDR.P24MeterResources.cont_save;
            this.p24_btnContainer_save.Location = new System.Drawing.Point(153, 354);
            this.p24_btnContainer_save.Name = "p24_btnContainer_save";
            this.p24_btnContainer_save.TabStop = true;
            this.p24_btnContainer_save.Size = new System.Drawing.Size(32, 32);
            this.p24_btnContainer_save.TabIndex = 115;
            this.p24_btnContainer_save.Text = "S";
            this.toolTip1.SetToolTip(this.p24_btnContainer_save, "Save a container file");
            this.p24_btnContainer_save.UseVisualStyleBackColor = true;
            this.p24_btnContainer_save.Click += new System.EventHandler(this.btnContainer_save_Click);

            // 
            // p24_btnFilter_4char_copy
            // 
            this.p24_btnFilter_4char_copy.Image = global::PowerSDR.P24MeterResources.copy;
            this.p24_btnFilter_4char_copy.Location = new System.Drawing.Point(284, 84);
            this.p24_btnFilter_4char_copy.Name = "p24_btnFilter_4char_copy";
            this.p24_btnFilter_4char_copy.TabStop = true;
            this.p24_btnFilter_4char_copy.Size = new System.Drawing.Size(27, 27);
            this.p24_btnFilter_4char_copy.TabIndex = 118;
            this.toolTip1.SetToolTip(this.p24_btnFilter_4char_copy, "Copy to clipboard");
            this.p24_btnFilter_4char_copy.UseVisualStyleBackColor = true;
            this.p24_btnFilter_4char_copy.Click += new System.EventHandler(this.btnFilter_4char_copy_Click);

            // 
            // p24_btnHistory_copy_minmax_from_0
            // 
            this.p24_btnHistory_copy_minmax_from_0.Image = null;
            this.p24_btnHistory_copy_minmax_from_0.Location = new System.Drawing.Point(249, 72);
            this.p24_btnHistory_copy_minmax_from_0.Name = "p24_btnHistory_copy_minmax_from_0";
            this.p24_btnHistory_copy_minmax_from_0.TabStop = true;
            this.p24_btnHistory_copy_minmax_from_0.Size = new System.Drawing.Size(32, 24);
            this.p24_btnHistory_copy_minmax_from_0.TabIndex = 146;
            this.p24_btnHistory_copy_minmax_from_0.Text = "=";
            this.p24_btnHistory_copy_minmax_from_0.UseVisualStyleBackColor = true;
            this.p24_btnHistory_copy_minmax_from_0.Click += new System.EventHandler(this.btnHistory_copy_minmax_from_0_Click);

            // 
            // p24_btnLedIndicatorVarPicker
            // 
            this.p24_btnLedIndicatorVarPicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnLedIndicatorVarPicker.Image = null;
            this.p24_btnLedIndicatorVarPicker.Location = new System.Drawing.Point(289, 98);
            this.p24_btnLedIndicatorVarPicker.Name = "p24_btnLedIndicatorVarPicker";
            this.p24_btnLedIndicatorVarPicker.TabStop = true;
            this.p24_btnLedIndicatorVarPicker.Size = new System.Drawing.Size(28, 28);
            this.p24_btnLedIndicatorVarPicker.TabIndex = 181;
            this.p24_btnLedIndicatorVarPicker.Text = "%";
            this.p24_btnLedIndicatorVarPicker.UseVisualStyleBackColor = true;
            this.p24_btnLedIndicatorVarPicker.Click += new System.EventHandler(this.btnLedIndicatorVarPicker_Click);

            // 
            // p24_btnLedIndicator_4char_copy
            // 
            this.p24_btnLedIndicator_4char_copy.Image = global::PowerSDR.P24MeterResources.copy;
            this.p24_btnLedIndicator_4char_copy.Location = new System.Drawing.Point(290, 210);
            this.p24_btnLedIndicator_4char_copy.Name = "p24_btnLedIndicator_4char_copy";
            this.p24_btnLedIndicator_4char_copy.TabStop = true;
            this.p24_btnLedIndicator_4char_copy.Size = new System.Drawing.Size(27, 27);
            this.p24_btnLedIndicator_4char_copy.TabIndex = 179;
            this.toolTip1.SetToolTip(this.p24_btnLedIndicator_4char_copy, "Copy to clipboard");
            this.p24_btnLedIndicator_4char_copy.UseVisualStyleBackColor = true;
            this.p24_btnLedIndicator_4char_copy.Click += new System.EventHandler(this.btnLedIndicator_4char_copy_Click);

            // 
            // p24_btnLedIndicator_copy_sizex_to_y
            // 
            this.p24_btnLedIndicator_copy_sizex_to_y.Image = null;
            this.p24_btnLedIndicator_copy_sizex_to_y.Location = new System.Drawing.Point(180, 313);
            this.p24_btnLedIndicator_copy_sizex_to_y.Name = "p24_btnLedIndicator_copy_sizex_to_y";
            this.p24_btnLedIndicator_copy_sizex_to_y.TabStop = true;
            this.p24_btnLedIndicator_copy_sizex_to_y.Size = new System.Drawing.Size(33, 23);
            this.p24_btnLedIndicator_copy_sizex_to_y.TabIndex = 155;
            this.p24_btnLedIndicator_copy_sizex_to_y.Text = "=";
            this.toolTip1.SetToolTip(this.p24_btnLedIndicator_copy_sizex_to_y, "Copy the size values from X to Y");
            this.p24_btnLedIndicator_copy_sizex_to_y.UseVisualStyleBackColor = true;
            this.p24_btnLedIndicator_copy_sizex_to_y.Click += new System.EventHandler(this.btnLedIndicator_copy_sizex_to_y_Click);

            // 
            // p24_btnLedIndicator_copy_truefalse_colours
            // 
            this.p24_btnLedIndicator_copy_truefalse_colours.Image = null;
            this.p24_btnLedIndicator_copy_truefalse_colours.Location = new System.Drawing.Point(115, 171);
            this.p24_btnLedIndicator_copy_truefalse_colours.Name = "p24_btnLedIndicator_copy_truefalse_colours";
            this.p24_btnLedIndicator_copy_truefalse_colours.TabStop = true;
            this.p24_btnLedIndicator_copy_truefalse_colours.Size = new System.Drawing.Size(33, 23);
            this.p24_btnLedIndicator_copy_truefalse_colours.TabIndex = 165;
            this.p24_btnLedIndicator_copy_truefalse_colours.Text = "=";
            this.toolTip1.SetToolTip(this.p24_btnLedIndicator_copy_truefalse_colours, "Copy the colours from TRUE to FALSE");
            this.p24_btnLedIndicator_copy_truefalse_colours.UseVisualStyleBackColor = true;
            this.p24_btnLedIndicator_copy_truefalse_colours.Click += new System.EventHandler(this.btnLedIndicator_copy_truefalse_colours_Click);

            // 
            // p24_btnMMIO_variable
            // 
            this.p24_btnMMIO_variable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnMMIO_variable.Image = null;
            this.p24_btnMMIO_variable.Location = new System.Drawing.Point(243, 57);
            this.p24_btnMMIO_variable.Name = "p24_btnMMIO_variable";
            this.p24_btnMMIO_variable.TabStop = true;
            this.p24_btnMMIO_variable.Size = new System.Drawing.Size(28, 28);
            this.p24_btnMMIO_variable.TabIndex = 128;
            this.p24_btnMMIO_variable.Text = "%";
            this.p24_btnMMIO_variable.UseVisualStyleBackColor = true;
            this.p24_btnMMIO_variable.Click += new System.EventHandler(this.btnMMIO_variable_Click);

            // 
            // p24_btnMMIO_variable_2
            // 
            this.p24_btnMMIO_variable_2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnMMIO_variable_2.Image = null;
            this.p24_btnMMIO_variable_2.Location = new System.Drawing.Point(275, 57);
            this.p24_btnMMIO_variable_2.Name = "p24_btnMMIO_variable_2";
            this.p24_btnMMIO_variable_2.TabStop = true;
            this.p24_btnMMIO_variable_2.Size = new System.Drawing.Size(28, 28);
            this.p24_btnMMIO_variable_2.TabIndex = 129;
            this.p24_btnMMIO_variable_2.Text = "%";
            this.p24_btnMMIO_variable_2.UseVisualStyleBackColor = true;
            this.p24_btnMMIO_variable_2.Click += new System.EventHandler(this.btnMMIO_variable_2_Click);

            // 
            // p24_btnMMIO_variable_2_history
            // 
            this.p24_btnMMIO_variable_2_history.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnMMIO_variable_2_history.Image = null;
            this.p24_btnMMIO_variable_2_history.Location = new System.Drawing.Point(284, 52);
            this.p24_btnMMIO_variable_2_history.Name = "p24_btnMMIO_variable_2_history";
            this.p24_btnMMIO_variable_2_history.TabStop = true;
            this.p24_btnMMIO_variable_2_history.Size = new System.Drawing.Size(28, 28);
            this.p24_btnMMIO_variable_2_history.TabIndex = 140;
            this.p24_btnMMIO_variable_2_history.Text = "%";
            this.p24_btnMMIO_variable_2_history.UseVisualStyleBackColor = true;
            this.p24_btnMMIO_variable_2_history.Click += new System.EventHandler(this.btnMMIO_variable_2_history_Click);

            // 
            // p24_btnMMIO_variable_2_rotator
            // 
            this.p24_btnMMIO_variable_2_rotator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnMMIO_variable_2_rotator.Image = null;
            this.p24_btnMMIO_variable_2_rotator.Location = new System.Drawing.Point(275, 46);
            this.p24_btnMMIO_variable_2_rotator.Name = "p24_btnMMIO_variable_2_rotator";
            this.p24_btnMMIO_variable_2_rotator.TabStop = true;
            this.p24_btnMMIO_variable_2_rotator.Size = new System.Drawing.Size(28, 28);
            this.p24_btnMMIO_variable_2_rotator.TabIndex = 129;
            this.p24_btnMMIO_variable_2_rotator.Text = "%";
            this.p24_btnMMIO_variable_2_rotator.UseVisualStyleBackColor = true;
            this.p24_btnMMIO_variable_2_rotator.Click += new System.EventHandler(this.btnMMIO_variable_2_rotator_Click);

            // 
            // p24_btnMMIO_variable_history
            // 
            this.p24_btnMMIO_variable_history.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnMMIO_variable_history.Image = null;
            this.p24_btnMMIO_variable_history.Location = new System.Drawing.Point(252, 52);
            this.p24_btnMMIO_variable_history.Name = "p24_btnMMIO_variable_history";
            this.p24_btnMMIO_variable_history.TabStop = true;
            this.p24_btnMMIO_variable_history.Size = new System.Drawing.Size(28, 28);
            this.p24_btnMMIO_variable_history.TabIndex = 139;
            this.p24_btnMMIO_variable_history.Text = "%";
            this.p24_btnMMIO_variable_history.UseVisualStyleBackColor = true;
            this.p24_btnMMIO_variable_history.Click += new System.EventHandler(this.btnMMIO_variable_history_Click);

            // 
            // p24_btnMMIO_variable_rotator
            // 
            this.p24_btnMMIO_variable_rotator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnMMIO_variable_rotator.Image = null;
            this.p24_btnMMIO_variable_rotator.Location = new System.Drawing.Point(243, 46);
            this.p24_btnMMIO_variable_rotator.Name = "p24_btnMMIO_variable_rotator";
            this.p24_btnMMIO_variable_rotator.TabStop = true;
            this.p24_btnMMIO_variable_rotator.Size = new System.Drawing.Size(28, 28);
            this.p24_btnMMIO_variable_rotator.TabIndex = 128;
            this.p24_btnMMIO_variable_rotator.Text = "%";
            this.p24_btnMMIO_variable_rotator.UseVisualStyleBackColor = true;
            this.p24_btnMMIO_variable_rotator.Click += new System.EventHandler(this.btnMMIO_variable_rotator_Click);

            // 
            // p24_btnMeterCopySettings
            // 
            this.p24_btnMeterCopySettings.Image = global::PowerSDR.P24MeterResources.pipette32border;
            this.p24_btnMeterCopySettings.Location = new System.Drawing.Point(337, 316);
            this.p24_btnMeterCopySettings.Name = "p24_btnMeterCopySettings";
            this.p24_btnMeterCopySettings.TabStop = true;
            this.p24_btnMeterCopySettings.Size = new System.Drawing.Size(32, 32);
            this.p24_btnMeterCopySettings.TabIndex = 103;
            this.toolTip1.SetToolTip(this.p24_btnMeterCopySettings, "Copy settings and colours");
            this.p24_btnMeterCopySettings.UseVisualStyleBackColor = true;
            this.p24_btnMeterCopySettings.Click += new System.EventHandler(this.btnMeterCopySettings_Click);

            // 
            // p24_btnMeterDown
            // 
            this.p24_btnMeterDown.Image = global::PowerSDR.P24MeterResources.down_black;
            this.p24_btnMeterDown.Location = new System.Drawing.Point(336, 212);
            this.p24_btnMeterDown.Name = "p24_btnMeterDown";
            this.p24_btnMeterDown.TabStop = true;
            this.p24_btnMeterDown.Size = new System.Drawing.Size(32, 32);
            this.p24_btnMeterDown.TabIndex = 94;
            this.toolTip1.SetToolTip(this.p24_btnMeterDown, "Move item down");
            this.p24_btnMeterDown.UseVisualStyleBackColor = true;
            this.p24_btnMeterDown.Click += new System.EventHandler(this.btnMeterDown_Click);

            // 
            // p24_btnMeterPasteSettings
            // 
            this.p24_btnMeterPasteSettings.Image = global::PowerSDR.P24MeterResources.brush32border;
            this.p24_btnMeterPasteSettings.Location = new System.Drawing.Point(336, 353);
            this.p24_btnMeterPasteSettings.Name = "p24_btnMeterPasteSettings";
            this.p24_btnMeterPasteSettings.TabStop = true;
            this.p24_btnMeterPasteSettings.Size = new System.Drawing.Size(32, 32);
            this.p24_btnMeterPasteSettings.TabIndex = 102;
            this.toolTip1.SetToolTip(this.p24_btnMeterPasteSettings, "Paste settings and colours into suitable meter item");
            this.p24_btnMeterPasteSettings.UseVisualStyleBackColor = true;
            this.p24_btnMeterPasteSettings.Click += new System.EventHandler(this.btnMeterPasteSettings_Click);

            // 
            // p24_btnMeterUp
            // 
            this.p24_btnMeterUp.Image = global::PowerSDR.P24MeterResources.arrow_up_black;
            this.p24_btnMeterUp.Location = new System.Drawing.Point(337, 174);
            this.p24_btnMeterUp.Name = "p24_btnMeterUp";
            this.p24_btnMeterUp.TabStop = true;
            this.p24_btnMeterUp.Size = new System.Drawing.Size(32, 32);
            this.p24_btnMeterUp.TabIndex = 95;
            this.toolTip1.SetToolTip(this.p24_btnMeterUp, "Move item up");
            this.p24_btnMeterUp.UseVisualStyleBackColor = true;
            this.p24_btnMeterUp.Click += new System.EventHandler(this.btnMeterUp_Click);

            // 
            // p24_btnOtherButtons_reset_layout
            // 
            this.p24_btnOtherButtons_reset_layout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.p24_btnOtherButtons_reset_layout.Image = global::PowerSDR.P24MeterResources.grid;
            this.p24_btnOtherButtons_reset_layout.Location = new System.Drawing.Point(106, 338);
            this.p24_btnOtherButtons_reset_layout.Name = "p24_btnOtherButtons_reset_layout";
            this.p24_btnOtherButtons_reset_layout.TabStop = true;
            this.p24_btnOtherButtons_reset_layout.Size = new System.Drawing.Size(32, 32);
            this.p24_btnOtherButtons_reset_layout.TabIndex = 113;
            this.toolTip1.SetToolTip(this.p24_btnOtherButtons_reset_layout, "Reset layout");
            this.p24_btnOtherButtons_reset_layout.UseVisualStyleBackColor = true;
            this.p24_btnOtherButtons_reset_layout.Click += new System.EventHandler(this.btnOtherButtons_reset_layout_Click);

            // 
            // p24_btnRecording_4char_copy
            // 
            this.p24_btnRecording_4char_copy.Image = global::PowerSDR.P24MeterResources.copy;
            this.p24_btnRecording_4char_copy.Location = new System.Drawing.Point(107, 24);
            this.p24_btnRecording_4char_copy.Name = "p24_btnRecording_4char_copy";
            this.p24_btnRecording_4char_copy.TabStop = true;
            this.p24_btnRecording_4char_copy.Size = new System.Drawing.Size(27, 27);
            this.p24_btnRecording_4char_copy.TabIndex = 185;
            this.toolTip1.SetToolTip(this.p24_btnRecording_4char_copy, "Copy to clipboard");
            this.p24_btnRecording_4char_copy.UseVisualStyleBackColor = true;
            this.p24_btnRecording_4char_copy.Click += new System.EventHandler(this.btnRecording_4char_copy_Click);

            // 
            // p24_btnRecording_assingnkeybind
            // 
            this.p24_btnRecording_assingnkeybind.Enabled = false;
            this.p24_btnRecording_assingnkeybind.Image = null;
            this.p24_btnRecording_assingnkeybind.Location = new System.Drawing.Point(93, 67);
            this.p24_btnRecording_assingnkeybind.Name = "p24_btnRecording_assingnkeybind";
            this.p24_btnRecording_assingnkeybind.TabStop = true;
            this.p24_btnRecording_assingnkeybind.Size = new System.Drawing.Size(53, 23);
            this.p24_btnRecording_assingnkeybind.TabIndex = 179;
            this.p24_btnRecording_assingnkeybind.Text = "assign";
            this.toolTip1.SetToolTip(this.p24_btnRecording_assingnkeybind, "Listen for the next 10 seconds for a keybind");
            this.p24_btnRecording_assingnkeybind.UseVisualStyleBackColor = true;
            this.p24_btnRecording_assingnkeybind.Click += new System.EventHandler(this.btnRecording_assingnkeybind_Click);

            // 
            // p24_btnRecording_export_wav_from_slot
            // 
            this.p24_btnRecording_export_wav_from_slot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.p24_btnRecording_export_wav_from_slot.Image = global::PowerSDR.P24MeterResources.cont_save;
            this.p24_btnRecording_export_wav_from_slot.Location = new System.Drawing.Point(140, 36);
            this.p24_btnRecording_export_wav_from_slot.Name = "p24_btnRecording_export_wav_from_slot";
            this.p24_btnRecording_export_wav_from_slot.TabStop = true;
            this.p24_btnRecording_export_wav_from_slot.Size = new System.Drawing.Size(27, 27);
            this.p24_btnRecording_export_wav_from_slot.TabIndex = 186;
            this.toolTip1.SetToolTip(this.p24_btnRecording_export_wav_from_slot, "Export wav from slot");
            this.p24_btnRecording_export_wav_from_slot.UseVisualStyleBackColor = true;
            this.p24_btnRecording_export_wav_from_slot.Click += new System.EventHandler(this.btnRecording_export_wav_from_slot_Click);

            // 
            // p24_btnRecording_globalkeybind_assign
            // 
            this.p24_btnRecording_globalkeybind_assign.Enabled = false;
            this.p24_btnRecording_globalkeybind_assign.Image = null;
            this.p24_btnRecording_globalkeybind_assign.Location = new System.Drawing.Point(103, 19);
            this.p24_btnRecording_globalkeybind_assign.Name = "p24_btnRecording_globalkeybind_assign";
            this.p24_btnRecording_globalkeybind_assign.TabStop = true;
            this.p24_btnRecording_globalkeybind_assign.Size = new System.Drawing.Size(53, 23);
            this.p24_btnRecording_globalkeybind_assign.TabIndex = 182;
            this.p24_btnRecording_globalkeybind_assign.Text = "assign";
            this.toolTip1.SetToolTip(this.p24_btnRecording_globalkeybind_assign, "Listen for the next 10 seconds for a keybind");
            this.p24_btnRecording_globalkeybind_assign.UseVisualStyleBackColor = true;
            this.p24_btnRecording_globalkeybind_assign.Click += new System.EventHandler(this.btnRecording_globalkeybind_assign_Click);

            // 
            // p24_btnRecording_load_wav_to_slot
            // 
            this.p24_btnRecording_load_wav_to_slot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.p24_btnRecording_load_wav_to_slot.Image = global::PowerSDR.P24MeterResources.cont_load;
            this.p24_btnRecording_load_wav_to_slot.Location = new System.Drawing.Point(140, 3);
            this.p24_btnRecording_load_wav_to_slot.Name = "p24_btnRecording_load_wav_to_slot";
            this.p24_btnRecording_load_wav_to_slot.TabStop = true;
            this.p24_btnRecording_load_wav_to_slot.Size = new System.Drawing.Size(27, 27);
            this.p24_btnRecording_load_wav_to_slot.TabIndex = 114;
            this.toolTip1.SetToolTip(this.p24_btnRecording_load_wav_to_slot, "Load wav to slot");
            this.p24_btnRecording_load_wav_to_slot.UseVisualStyleBackColor = true;
            this.p24_btnRecording_load_wav_to_slot.Click += new System.EventHandler(this.btnRecording_load_wav_to_slot_Click);

            // 
            // p24_btnRecording_openStorageFolder
            // 
            this.p24_btnRecording_openStorageFolder.Image = null;
            this.p24_btnRecording_openStorageFolder.Location = new System.Drawing.Point(100, 3);
            this.p24_btnRecording_openStorageFolder.Name = "p24_btnRecording_openStorageFolder";
            this.p24_btnRecording_openStorageFolder.TabStop = true;
            this.p24_btnRecording_openStorageFolder.Size = new System.Drawing.Size(34, 20);
            this.p24_btnRecording_openStorageFolder.TabIndex = 172;
            this.p24_btnRecording_openStorageFolder.Text = "...";
            this.toolTip1.SetToolTip(this.p24_btnRecording_openStorageFolder, "Open storage folder");
            this.p24_btnRecording_openStorageFolder.UseVisualStyleBackColor = true;
            this.p24_btnRecording_openStorageFolder.Click += new System.EventHandler(this.btnRecording_openStorageFolder_Click);

            // 
            // p24_btnRecoverContainer
            // 
            this.p24_btnRecoverContainer.Image = null;
            this.p24_btnRecoverContainer.Location = new System.Drawing.Point(209, 63);
            this.p24_btnRecoverContainer.Name = "p24_btnRecoverContainer";
            this.p24_btnRecoverContainer.TabStop = true;
            this.p24_btnRecoverContainer.Size = new System.Drawing.Size(71, 44);
            this.p24_btnRecoverContainer.TabIndex = 118;
            this.p24_btnRecoverContainer.Text = "Recover Container";
            this.toolTip1.SetToolTip(this.p24_btnRecoverContainer, "Recover this container to the console window");
            this.p24_btnRecoverContainer.UseVisualStyleBackColor = true;
            this.p24_btnRecoverContainer.Click += new System.EventHandler(this.btnRecoverContainer_Click);

            // 
            // p24_btnRemoveMeterItem
            // 
            this.p24_btnRemoveMeterItem.Image = global::PowerSDR.P24MeterResources.arrow_left_black;
            this.p24_btnRemoveMeterItem.Location = new System.Drawing.Point(153, 212);
            this.p24_btnRemoveMeterItem.Name = "p24_btnRemoveMeterItem";
            this.p24_btnRemoveMeterItem.TabStop = true;
            this.p24_btnRemoveMeterItem.Size = new System.Drawing.Size(32, 32);
            this.p24_btnRemoveMeterItem.TabIndex = 93;
            this.toolTip1.SetToolTip(this.p24_btnRemoveMeterItem, "Remove the item");
            this.p24_btnRemoveMeterItem.UseVisualStyleBackColor = true;
            this.p24_btnRemoveMeterItem.Click += new System.EventHandler(this.btnRemoveMeterItem_Click);

            // 
            // p24_btnTextOverlayVarPicker
            // 
            this.p24_btnTextOverlayVarPicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_btnTextOverlayVarPicker.Image = null;
            this.p24_btnTextOverlayVarPicker.Location = new System.Drawing.Point(289, 98);
            this.p24_btnTextOverlayVarPicker.Name = "p24_btnTextOverlayVarPicker";
            this.p24_btnTextOverlayVarPicker.TabStop = true;
            this.p24_btnTextOverlayVarPicker.Size = new System.Drawing.Size(28, 28);
            this.p24_btnTextOverlayVarPicker.TabIndex = 170;
            this.p24_btnTextOverlayVarPicker.Text = "%";
            this.p24_btnTextOverlayVarPicker.UseVisualStyleBackColor = true;
            this.p24_btnTextOverlayVarPicker.Click += new System.EventHandler(this.btnTextOverlayVarPicker_Click);

            // 
            // p24_btnTextOverlay_Font1
            // 
            this.p24_btnTextOverlay_Font1.Image = null;
            this.p24_btnTextOverlay_Font1.Location = new System.Drawing.Point(36, 183);
            this.p24_btnTextOverlay_Font1.Name = "p24_btnTextOverlay_Font1";
            this.p24_btnTextOverlay_Font1.TabStop = true;
            this.p24_btnTextOverlay_Font1.Size = new System.Drawing.Size(49, 23);
            this.p24_btnTextOverlay_Font1.TabIndex = 137;
            this.p24_btnTextOverlay_Font1.Text = "Font";
            this.p24_btnTextOverlay_Font1.UseVisualStyleBackColor = true;
            this.p24_btnTextOverlay_Font1.Click += new System.EventHandler(this.btnTextOverlay_Font1_Click);

            // 
            // p24_btnTextOverlay_Font2
            // 
            this.p24_btnTextOverlay_Font2.Image = null;
            this.p24_btnTextOverlay_Font2.Location = new System.Drawing.Point(36, 209);
            this.p24_btnTextOverlay_Font2.Name = "p24_btnTextOverlay_Font2";
            this.p24_btnTextOverlay_Font2.TabStop = true;
            this.p24_btnTextOverlay_Font2.Size = new System.Drawing.Size(49, 23);
            this.p24_btnTextOverlay_Font2.TabIndex = 140;
            this.p24_btnTextOverlay_Font2.Text = "Font";
            this.p24_btnTextOverlay_Font2.UseVisualStyleBackColor = true;
            this.p24_btnTextOverlay_Font2.Click += new System.EventHandler(this.btnTextOverlay_Font2_Click);

            // 
            // p24_btnTextOverlay_copyfonts
            // 
            this.p24_btnTextOverlay_copyfonts.Image = null;
            this.p24_btnTextOverlay_copyfonts.Location = new System.Drawing.Point(236, 195);
            this.p24_btnTextOverlay_copyfonts.Name = "p24_btnTextOverlay_copyfonts";
            this.p24_btnTextOverlay_copyfonts.TabStop = true;
            this.p24_btnTextOverlay_copyfonts.Size = new System.Drawing.Size(28, 23);
            this.p24_btnTextOverlay_copyfonts.TabIndex = 165;
            this.p24_btnTextOverlay_copyfonts.Text = "=";
            this.toolTip1.SetToolTip(this.p24_btnTextOverlay_copyfonts, "Copy the font and colours from RX to TX");
            this.p24_btnTextOverlay_copyfonts.UseVisualStyleBackColor = true;
            this.p24_btnTextOverlay_copyfonts.Click += new System.EventHandler(this.btnTextOverlay_copyfonts_Click);

            // 
            // p24_btnTextOverlay_copyoffsets
            // 
            this.p24_btnTextOverlay_copyoffsets.Image = null;
            this.p24_btnTextOverlay_copyoffsets.Location = new System.Drawing.Point(178, 333);
            this.p24_btnTextOverlay_copyoffsets.Name = "p24_btnTextOverlay_copyoffsets";
            this.p24_btnTextOverlay_copyoffsets.TabStop = true;
            this.p24_btnTextOverlay_copyoffsets.Size = new System.Drawing.Size(33, 23);
            this.p24_btnTextOverlay_copyoffsets.TabIndex = 155;
            this.p24_btnTextOverlay_copyoffsets.Text = "=";
            this.toolTip1.SetToolTip(this.p24_btnTextOverlay_copyoffsets, "Copy the offset values from RX to TX");
            this.p24_btnTextOverlay_copyoffsets.UseVisualStyleBackColor = true;
            this.p24_btnTextOverlay_copyoffsets.Click += new System.EventHandler(this.btnTextOverlay_copyoffsets_Click);

            // 
            // p24_btnVFOCopyColourFromMainNumbers
            // 
            this.p24_btnVFOCopyColourFromMainNumbers.Image = null;
            this.p24_btnVFOCopyColourFromMainNumbers.Location = new System.Drawing.Point(274, 88);
            this.p24_btnVFOCopyColourFromMainNumbers.Name = "p24_btnVFOCopyColourFromMainNumbers";
            this.p24_btnVFOCopyColourFromMainNumbers.TabStop = true;
            this.p24_btnVFOCopyColourFromMainNumbers.Size = new System.Drawing.Size(25, 23);
            this.p24_btnVFOCopyColourFromMainNumbers.TabIndex = 139;
            this.p24_btnVFOCopyColourFromMainNumbers.Text = "=";
            this.toolTip1.SetToolTip(this.p24_btnVFOCopyColourFromMainNumbers, "Copy the colour from the main #\'s");
            this.p24_btnVFOCopyColourFromMainNumbers.UseVisualStyleBackColor = true;
            this.p24_btnVFOCopyColourFromMainNumbers.Click += new System.EventHandler(this.btnVFOCopyColourFromMainNumbers_Click);

            // 
            // p24_btnWaveRecord_reset_layout
            // 
            this.p24_btnWaveRecord_reset_layout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.p24_btnWaveRecord_reset_layout.Image = global::PowerSDR.P24MeterResources.grid;
            this.p24_btnWaveRecord_reset_layout.Location = new System.Drawing.Point(280, 229);
            this.p24_btnWaveRecord_reset_layout.Name = "p24_btnWaveRecord_reset_layout";
            this.p24_btnWaveRecord_reset_layout.TabStop = true;
            this.p24_btnWaveRecord_reset_layout.Size = new System.Drawing.Size(32, 32);
            this.p24_btnWaveRecord_reset_layout.TabIndex = 32;
            this.toolTip1.SetToolTip(this.p24_btnWaveRecord_reset_layout, "Reset order");
            this.p24_btnWaveRecord_reset_layout.UseVisualStyleBackColor = true;
            this.p24_btnWaveRecord_reset_layout.Click += new System.EventHandler(this.btnWaveRecord_reset_layout_Click);

            // 
            // p24_btnWebImage_bsdworld_visit
            // 
            this.p24_btnWebImage_bsdworld_visit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.p24_btnWebImage_bsdworld_visit.Image = null;
            this.p24_btnWebImage_bsdworld_visit.Location = new System.Drawing.Point(208, 12);
            this.p24_btnWebImage_bsdworld_visit.Name = "p24_btnWebImage_bsdworld_visit";
            this.p24_btnWebImage_bsdworld_visit.TabStop = true;
            this.p24_btnWebImage_bsdworld_visit.Size = new System.Drawing.Size(63, 24);
            this.p24_btnWebImage_bsdworld_visit.TabIndex = 1;
            this.p24_btnWebImage_bsdworld_visit.Text = "Visit";
            this.p24_btnWebImage_bsdworld_visit.UseVisualStyleBackColor = false;
            this.p24_btnWebImage_bsdworld_visit.Click += new System.EventHandler(this.btnWebImage_bsdworld_visit_Click);

            // 
            // p24_btnWebImage_goto_next
            // 
            this.p24_btnWebImage_goto_next.Image = null;
            this.p24_btnWebImage_goto_next.Location = new System.Drawing.Point(257, 162);
            this.p24_btnWebImage_goto_next.Name = "p24_btnWebImage_goto_next";
            this.p24_btnWebImage_goto_next.TabStop = true;
            this.p24_btnWebImage_goto_next.Size = new System.Drawing.Size(26, 26);
            this.p24_btnWebImage_goto_next.TabIndex = 151;
            this.p24_btnWebImage_goto_next.Text = ">";
            this.toolTip1.SetToolTip(this.p24_btnWebImage_goto_next, "Go to the webimage item");
            this.p24_btnWebImage_goto_next.UseVisualStyleBackColor = true;
            this.p24_btnWebImage_goto_next.Click += new System.EventHandler(this.btnWebImage_goto_next_Click);

            // 
            // p24_btnWebImage_hamqsl_donate
            // 
            this.p24_btnWebImage_hamqsl_donate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.p24_btnWebImage_hamqsl_donate.Image = null;
            this.p24_btnWebImage_hamqsl_donate.Location = new System.Drawing.Point(208, 12);
            this.p24_btnWebImage_hamqsl_donate.Name = "p24_btnWebImage_hamqsl_donate";
            this.p24_btnWebImage_hamqsl_donate.TabStop = true;
            this.p24_btnWebImage_hamqsl_donate.Size = new System.Drawing.Size(63, 24);
            this.p24_btnWebImage_hamqsl_donate.TabIndex = 1;
            this.p24_btnWebImage_hamqsl_donate.Text = "Donate";
            this.p24_btnWebImage_hamqsl_donate.UseVisualStyleBackColor = false;
            this.p24_btnWebImage_hamqsl_donate.Click += new System.EventHandler(this.btnWebImage_hamqsl_donate_Click);

            // 
            // p24_buttonTS1
            // 
            this.p24_buttonTS1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.p24_buttonTS1.Image = null;
            this.p24_buttonTS1.Location = new System.Drawing.Point(208, 12);
            this.p24_buttonTS1.Name = "p24_buttonTS1";
            this.p24_buttonTS1.TabStop = true;
            this.p24_buttonTS1.Size = new System.Drawing.Size(63, 24);
            this.p24_buttonTS1.TabIndex = 1;
            this.p24_buttonTS1.Text = "Visit";
            this.p24_buttonTS1.UseVisualStyleBackColor = false;
            this.p24_buttonTS1.Visible = false;

            // 
            // p24_buttonTS2
            // 
            this.p24_buttonTS2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.p24_buttonTS2.Image = null;
            this.p24_buttonTS2.Location = new System.Drawing.Point(208, 12);
            this.p24_buttonTS2.Name = "p24_buttonTS2";
            this.p24_buttonTS2.TabStop = true;
            this.p24_buttonTS2.Size = new System.Drawing.Size(63, 24);
            this.p24_buttonTS2.TabIndex = 1;
            this.p24_buttonTS2.Text = "Visit";
            this.p24_buttonTS2.UseVisualStyleBackColor = false;
            this.p24_buttonTS2.Visible = false;

            // 
            // p24_chkBSDWorldDarkMode
            // 
            this.p24_chkBSDWorldDarkMode.AutoSize = true;
            this.p24_chkBSDWorldDarkMode.Image = null;
            this.p24_chkBSDWorldDarkMode.Location = new System.Drawing.Point(192, 40);
            this.p24_chkBSDWorldDarkMode.Name = "p24_chkBSDWorldDarkMode";
            this.p24_chkBSDWorldDarkMode.Size = new System.Drawing.Size(79, 17);
            this.p24_chkBSDWorldDarkMode.TabIndex = 2;
            this.p24_chkBSDWorldDarkMode.Text = "Dark Mode";
            this.p24_chkBSDWorldDarkMode.UseVisualStyleBackColor = true;

            // 
            // p24_chkBandButtons_band_inactive_use
            // 
            this.p24_chkBandButtons_band_inactive_use.AutoSize = true;
            this.p24_chkBandButtons_band_inactive_use.Image = null;
            this.p24_chkBandButtons_band_inactive_use.Location = new System.Drawing.Point(102, 235);
            this.p24_chkBandButtons_band_inactive_use.Name = "p24_chkBandButtons_band_inactive_use";
            this.p24_chkBandButtons_band_inactive_use.Size = new System.Drawing.Size(45, 17);
            this.p24_chkBandButtons_band_inactive_use.TabIndex = 157;
            this.p24_chkBandButtons_band_inactive_use.Text = "Use";
            this.toolTip1.SetToolTip(this.p24_chkBandButtons_band_inactive_use, "The indicator will use this colour when inactive");
            this.p24_chkBandButtons_band_inactive_use.UseVisualStyleBackColor = true;
            this.p24_chkBandButtons_band_inactive_use.CheckedChanged += new System.EventHandler(this.chkBandButtons_band_inactive_use_CheckedChanged);

            // 
            // p24_chkBandButtons_fade_rx
            // 
            this.p24_chkBandButtons_fade_rx.AutoSize = true;
            this.p24_chkBandButtons_fade_rx.Image = null;
            this.p24_chkBandButtons_fade_rx.Location = new System.Drawing.Point(215, 17);
            this.p24_chkBandButtons_fade_rx.Name = "p24_chkBandButtons_fade_rx";
            this.p24_chkBandButtons_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.p24_chkBandButtons_fade_rx.TabIndex = 2;
            this.p24_chkBandButtons_fade_rx.Text = "Fade on RX";
            this.p24_chkBandButtons_fade_rx.UseVisualStyleBackColor = true;
            this.p24_chkBandButtons_fade_rx.CheckedChanged += new System.EventHandler(this.chkBandButtons_fade_rx_CheckedChanged);

            // 
            // p24_chkBandButtons_fade_tx
            // 
            this.p24_chkBandButtons_fade_tx.AutoSize = true;
            this.p24_chkBandButtons_fade_tx.Image = null;
            this.p24_chkBandButtons_fade_tx.Location = new System.Drawing.Point(215, 40);
            this.p24_chkBandButtons_fade_tx.Name = "p24_chkBandButtons_fade_tx";
            this.p24_chkBandButtons_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.p24_chkBandButtons_fade_tx.TabIndex = 3;
            this.p24_chkBandButtons_fade_tx.Text = "Fade on TX";
            this.p24_chkBandButtons_fade_tx.UseVisualStyleBackColor = true;
            this.p24_chkBandButtons_fade_tx.CheckedChanged += new System.EventHandler(this.chkBandButtons_fade_tx_CheckedChanged);

            // 
            // p24_chkBandButtons_use_indicator
            // 
            this.p24_chkBandButtons_use_indicator.AutoSize = true;
            this.p24_chkBandButtons_use_indicator.Image = null;
            this.p24_chkBandButtons_use_indicator.Location = new System.Drawing.Point(25, 152);
            this.p24_chkBandButtons_use_indicator.Name = "p24_chkBandButtons_use_indicator";
            this.p24_chkBandButtons_use_indicator.Size = new System.Drawing.Size(89, 17);
            this.p24_chkBandButtons_use_indicator.TabIndex = 135;
            this.p24_chkBandButtons_use_indicator.Text = "Use Indicator";
            this.toolTip1.SetToolTip(this.p24_chkBandButtons_use_indicator, "Use a ring indicator to show active");
            this.p24_chkBandButtons_use_indicator.UseVisualStyleBackColor = true;
            this.p24_chkBandButtons_use_indicator.CheckedChanged += new System.EventHandler(this.chkBandButtons_use_indicator_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_byp
            // 
            this.p24_chkButtonBox_antenna_byp.AutoSize = true;
            this.p24_chkButtonBox_antenna_byp.Image = null;
            this.p24_chkButtonBox_antenna_byp.Location = new System.Drawing.Point(74, 11);
            this.p24_chkButtonBox_antenna_byp.Name = "p24_chkButtonBox_antenna_byp";
            this.p24_chkButtonBox_antenna_byp.Size = new System.Drawing.Size(44, 17);
            this.p24_chkButtonBox_antenna_byp.TabIndex = 6;
            this.p24_chkButtonBox_antenna_byp.Text = "Byp";
            this.p24_chkButtonBox_antenna_byp.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_byp.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_byp_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_ext1
            // 
            this.p24_chkButtonBox_antenna_ext1.AutoSize = true;
            this.p24_chkButtonBox_antenna_ext1.Image = null;
            this.p24_chkButtonBox_antenna_ext1.Location = new System.Drawing.Point(74, 32);
            this.p24_chkButtonBox_antenna_ext1.Name = "p24_chkButtonBox_antenna_ext1";
            this.p24_chkButtonBox_antenna_ext1.Size = new System.Drawing.Size(50, 17);
            this.p24_chkButtonBox_antenna_ext1.TabIndex = 7;
            this.p24_chkButtonBox_antenna_ext1.Text = "Ext 1";
            this.p24_chkButtonBox_antenna_ext1.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_ext1.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_ext1_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_rx1
            // 
            this.p24_chkButtonBox_antenna_rx1.AutoSize = true;
            this.p24_chkButtonBox_antenna_rx1.Image = null;
            this.p24_chkButtonBox_antenna_rx1.Location = new System.Drawing.Point(14, 11);
            this.p24_chkButtonBox_antenna_rx1.Name = "p24_chkButtonBox_antenna_rx1";
            this.p24_chkButtonBox_antenna_rx1.Size = new System.Drawing.Size(48, 17);
            this.p24_chkButtonBox_antenna_rx1.TabIndex = 0;
            this.p24_chkButtonBox_antenna_rx1.Text = "Rx 1";
            this.p24_chkButtonBox_antenna_rx1.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_rx1.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rx1_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_rx2
            // 
            this.p24_chkButtonBox_antenna_rx2.AutoSize = true;
            this.p24_chkButtonBox_antenna_rx2.Image = null;
            this.p24_chkButtonBox_antenna_rx2.Location = new System.Drawing.Point(14, 32);
            this.p24_chkButtonBox_antenna_rx2.Name = "p24_chkButtonBox_antenna_rx2";
            this.p24_chkButtonBox_antenna_rx2.Size = new System.Drawing.Size(48, 17);
            this.p24_chkButtonBox_antenna_rx2.TabIndex = 1;
            this.p24_chkButtonBox_antenna_rx2.Text = "Rx 2";
            this.p24_chkButtonBox_antenna_rx2.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_rx2.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rx2_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_rx3
            // 
            this.p24_chkButtonBox_antenna_rx3.AutoSize = true;
            this.p24_chkButtonBox_antenna_rx3.Image = null;
            this.p24_chkButtonBox_antenna_rx3.Location = new System.Drawing.Point(14, 53);
            this.p24_chkButtonBox_antenna_rx3.Name = "p24_chkButtonBox_antenna_rx3";
            this.p24_chkButtonBox_antenna_rx3.Size = new System.Drawing.Size(48, 17);
            this.p24_chkButtonBox_antenna_rx3.TabIndex = 2;
            this.p24_chkButtonBox_antenna_rx3.Text = "Rx 3";
            this.p24_chkButtonBox_antenna_rx3.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_rx3.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rx3_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_rxtxant
            // 
            this.p24_chkButtonBox_antenna_rxtxant.AutoSize = true;
            this.p24_chkButtonBox_antenna_rxtxant.Image = null;
            this.p24_chkButtonBox_antenna_rxtxant.Location = new System.Drawing.Point(72, 95);
            this.p24_chkButtonBox_antenna_rxtxant.Name = "p24_chkButtonBox_antenna_rxtxant";
            this.p24_chkButtonBox_antenna_rxtxant.Size = new System.Drawing.Size(75, 17);
            this.p24_chkButtonBox_antenna_rxtxant.TabIndex = 9;
            this.p24_chkButtonBox_antenna_rxtxant.Text = "Rx/Tx Ant";
            this.p24_chkButtonBox_antenna_rxtxant.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_rxtxant.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rxtxant_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_tx1
            // 
            this.p24_chkButtonBox_antenna_tx1.AutoSize = true;
            this.p24_chkButtonBox_antenna_tx1.Image = null;
            this.p24_chkButtonBox_antenna_tx1.Location = new System.Drawing.Point(14, 74);
            this.p24_chkButtonBox_antenna_tx1.Name = "p24_chkButtonBox_antenna_tx1";
            this.p24_chkButtonBox_antenna_tx1.Size = new System.Drawing.Size(47, 17);
            this.p24_chkButtonBox_antenna_tx1.TabIndex = 3;
            this.p24_chkButtonBox_antenna_tx1.Text = "Tx 1";
            this.p24_chkButtonBox_antenna_tx1.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_tx1.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_tx1_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_tx2
            // 
            this.p24_chkButtonBox_antenna_tx2.AutoSize = true;
            this.p24_chkButtonBox_antenna_tx2.Image = null;
            this.p24_chkButtonBox_antenna_tx2.Location = new System.Drawing.Point(14, 95);
            this.p24_chkButtonBox_antenna_tx2.Name = "p24_chkButtonBox_antenna_tx2";
            this.p24_chkButtonBox_antenna_tx2.Size = new System.Drawing.Size(47, 17);
            this.p24_chkButtonBox_antenna_tx2.TabIndex = 4;
            this.p24_chkButtonBox_antenna_tx2.Text = "Tx 2";
            this.p24_chkButtonBox_antenna_tx2.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_tx2.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_tx2_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_tx3
            // 
            this.p24_chkButtonBox_antenna_tx3.AutoSize = true;
            this.p24_chkButtonBox_antenna_tx3.Image = null;
            this.p24_chkButtonBox_antenna_tx3.Location = new System.Drawing.Point(14, 116);
            this.p24_chkButtonBox_antenna_tx3.Name = "p24_chkButtonBox_antenna_tx3";
            this.p24_chkButtonBox_antenna_tx3.Size = new System.Drawing.Size(47, 17);
            this.p24_chkButtonBox_antenna_tx3.TabIndex = 5;
            this.p24_chkButtonBox_antenna_tx3.Text = "Tx 3";
            this.p24_chkButtonBox_antenna_tx3.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_tx3.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_tx3_CheckedChanged);

            // 
            // p24_chkButtonBox_antenna_xvtr
            // 
            this.p24_chkButtonBox_antenna_xvtr.AutoSize = true;
            this.p24_chkButtonBox_antenna_xvtr.Image = null;
            this.p24_chkButtonBox_antenna_xvtr.Location = new System.Drawing.Point(74, 53);
            this.p24_chkButtonBox_antenna_xvtr.Name = "p24_chkButtonBox_antenna_xvtr";
            this.p24_chkButtonBox_antenna_xvtr.Size = new System.Drawing.Size(45, 17);
            this.p24_chkButtonBox_antenna_xvtr.TabIndex = 8;
            this.p24_chkButtonBox_antenna_xvtr.Text = "Xvtr";
            this.p24_chkButtonBox_antenna_xvtr.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_antenna_xvtr.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_xvtr_CheckedChanged);

            // 
            // p24_chkButtonBox_fix_text_size
            // 
            this.p24_chkButtonBox_fix_text_size.AutoSize = true;
            this.p24_chkButtonBox_fix_text_size.Image = null;
            this.p24_chkButtonBox_fix_text_size.Location = new System.Drawing.Point(154, 91);
            this.p24_chkButtonBox_fix_text_size.Name = "p24_chkButtonBox_fix_text_size";
            this.p24_chkButtonBox_fix_text_size.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_chkButtonBox_fix_text_size.Size = new System.Drawing.Size(39, 17);
            this.p24_chkButtonBox_fix_text_size.TabIndex = 169;
            this.p24_chkButtonBox_fix_text_size.Text = "Fix";
            this.toolTip1.SetToolTip(this.p24_chkButtonBox_fix_text_size, "Do not fit text to button. With this enabled use scale to manually fit the text.");
            this.p24_chkButtonBox_fix_text_size.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_fix_text_size.CheckedChanged += new System.EventHandler(this.chkButtonBox_fix_text_size_CheckedChanged);

            // 
            // p24_chkButtonBox_use_icons
            // 
            this.p24_chkButtonBox_use_icons.AutoSize = true;
            this.p24_chkButtonBox_use_icons.Image = null;
            this.p24_chkButtonBox_use_icons.Location = new System.Drawing.Point(265, 172);
            this.p24_chkButtonBox_use_icons.Name = "p24_chkButtonBox_use_icons";
            this.p24_chkButtonBox_use_icons.Size = new System.Drawing.Size(52, 17);
            this.p24_chkButtonBox_use_icons.TabIndex = 170;
            this.p24_chkButtonBox_use_icons.Text = "Icons";
            this.toolTip1.SetToolTip(this.p24_chkButtonBox_use_icons, "Use icons if a buton has one associated with it.");
            this.p24_chkButtonBox_use_icons.UseVisualStyleBackColor = true;
            this.p24_chkButtonBox_use_icons.CheckedChanged += new System.EventHandler(this.chkButtonBox_use_icons_CheckedChanged);

            // 
            // p24_chkContainerBorder
            // 
            this.p24_chkContainerBorder.AutoSize = true;
            this.p24_chkContainerBorder.Image = null;
            this.p24_chkContainerBorder.Location = new System.Drawing.Point(8, 61);
            this.p24_chkContainerBorder.Name = "p24_chkContainerBorder";
            this.p24_chkContainerBorder.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.p24_chkContainerBorder.Size = new System.Drawing.Size(57, 17);
            this.p24_chkContainerBorder.TabIndex = 97;
            this.p24_chkContainerBorder.Text = "Border";
            this.toolTip1.SetToolTip(this.p24_chkContainerBorder, "Container has a border");
            this.p24_chkContainerBorder.UseVisualStyleBackColor = true;
            this.p24_chkContainerBorder.CheckedChanged += new System.EventHandler(this.chkContainerBorder_CheckedChanged);

            // 
            // p24_chkContainerHighlight
            // 
            this.p24_chkContainerHighlight.AutoSize = true;
            this.p24_chkContainerHighlight.Image = null;
            this.p24_chkContainerHighlight.Location = new System.Drawing.Point(8, 40);
            this.p24_chkContainerHighlight.Name = "p24_chkContainerHighlight";
            this.p24_chkContainerHighlight.Size = new System.Drawing.Size(67, 17);
            this.p24_chkContainerHighlight.TabIndex = 89;
            this.p24_chkContainerHighlight.Text = "Highlight";
            this.toolTip1.SetToolTip(this.p24_chkContainerHighlight, "Highlight selected container");
            this.p24_chkContainerHighlight.UseVisualStyleBackColor = true;
            this.p24_chkContainerHighlight.CheckedChanged += new System.EventHandler(this.chkContainerHighlight_CheckedChanged);

            // 
            // p24_chkContainerMinimises
            // 
            this.p24_chkContainerMinimises.AutoSize = true;
            this.p24_chkContainerMinimises.Image = null;
            this.p24_chkContainerMinimises.Location = new System.Drawing.Point(294, 121);
            this.p24_chkContainerMinimises.Name = "p24_chkContainerMinimises";
            this.p24_chkContainerMinimises.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_chkContainerMinimises.Size = new System.Drawing.Size(66, 17);
            this.p24_chkContainerMinimises.TabIndex = 108;
            this.p24_chkContainerMinimises.Text = "Minimise";
            this.toolTip1.SetToolTip(this.p24_chkContainerMinimises, "Container will minimise if main window is minimised");
            this.p24_chkContainerMinimises.UseVisualStyleBackColor = true;
            this.p24_chkContainerMinimises.CheckedChanged += new System.EventHandler(this.chkContainerMinimises_CheckedChanged);

            // 
            // p24_chkContainerNoTitle
            // 
            this.p24_chkContainerNoTitle.AutoSize = true;
            this.p24_chkContainerNoTitle.Image = null;
            this.p24_chkContainerNoTitle.Location = new System.Drawing.Point(8, 82);
            this.p24_chkContainerNoTitle.Name = "p24_chkContainerNoTitle";
            this.p24_chkContainerNoTitle.Size = new System.Drawing.Size(77, 17);
            this.p24_chkContainerNoTitle.TabIndex = 104;
            this.p24_chkContainerNoTitle.Text = "No title bar";
            this.toolTip1.SetToolTip(this.p24_chkContainerNoTitle, "Prevents the display of the mouse over title bar and the resize grabber in the co" +
        "rner. Hold shift to bypass this.");
            this.p24_chkContainerNoTitle.UseVisualStyleBackColor = true;
            this.p24_chkContainerNoTitle.CheckedChanged += new System.EventHandler(this.chkContainerNoTitle_CheckedChanged);

            // 
            // p24_chkContainerShowRX
            // 
            this.p24_chkContainerShowRX.AutoSize = true;
            this.p24_chkContainerShowRX.Image = null;
            this.p24_chkContainerShowRX.Location = new System.Drawing.Point(128, 40);
            this.p24_chkContainerShowRX.Name = "p24_chkContainerShowRX";
            this.p24_chkContainerShowRX.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_chkContainerShowRX.Size = new System.Drawing.Size(71, 17);
            this.p24_chkContainerShowRX.TabIndex = 105;
            this.p24_chkContainerShowRX.Text = "Show RX";
            this.toolTip1.SetToolTip(this.p24_chkContainerShowRX, "Show the selected container on RX");
            this.p24_chkContainerShowRX.UseVisualStyleBackColor = true;
            this.p24_chkContainerShowRX.CheckedChanged += new System.EventHandler(this.chkContainerShowRX_CheckedChanged);

            // 
            // p24_chkContainerShowTX
            // 
            this.p24_chkContainerShowTX.AutoSize = true;
            this.p24_chkContainerShowTX.Image = null;
            this.p24_chkContainerShowTX.Location = new System.Drawing.Point(129, 60);
            this.p24_chkContainerShowTX.Name = "p24_chkContainerShowTX";
            this.p24_chkContainerShowTX.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_chkContainerShowTX.Size = new System.Drawing.Size(70, 17);
            this.p24_chkContainerShowTX.TabIndex = 110;
            this.p24_chkContainerShowTX.Text = "Show TX";
            this.toolTip1.SetToolTip(this.p24_chkContainerShowTX, "Show the selected container on TX");
            this.p24_chkContainerShowTX.UseVisualStyleBackColor = true;
            this.p24_chkContainerShowTX.CheckedChanged += new System.EventHandler(this.chkContainerShowTX_CheckedChanged);

            // 
            // p24_chkContainer_hidewhennotused
            // 
            this.p24_chkContainer_hidewhennotused.AutoSize = true;
            this.p24_chkContainer_hidewhennotused.Image = null;
            this.p24_chkContainer_hidewhennotused.Location = new System.Drawing.Point(286, 85);
            this.p24_chkContainer_hidewhennotused.Name = "p24_chkContainer_hidewhennotused";
            this.p24_chkContainer_hidewhennotused.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_chkContainer_hidewhennotused.Size = new System.Drawing.Size(74, 30);
            this.p24_chkContainer_hidewhennotused.TabIndex = 114;
            this.p24_chkContainer_hidewhennotused.Text = "Hide if RX\r\nnot in use";
            this.toolTip1.SetToolTip(this.p24_chkContainer_hidewhennotused, "Hide this container if RX is not in use");
            this.p24_chkContainer_hidewhennotused.UseVisualStyleBackColor = true;
            this.p24_chkContainer_hidewhennotused.CheckedChanged += new System.EventHandler(this.chkContainer_hidewhennotused_CheckedChanged);

            // 
            // p24_chkDialDisplay_alwaysshow_vfos
            // 
            this.p24_chkDialDisplay_alwaysshow_vfos.AutoSize = true;
            this.p24_chkDialDisplay_alwaysshow_vfos.Image = null;
            this.p24_chkDialDisplay_alwaysshow_vfos.Location = new System.Drawing.Point(42, 65);
            this.p24_chkDialDisplay_alwaysshow_vfos.Name = "p24_chkDialDisplay_alwaysshow_vfos";
            this.p24_chkDialDisplay_alwaysshow_vfos.Size = new System.Drawing.Size(116, 17);
            this.p24_chkDialDisplay_alwaysshow_vfos.TabIndex = 164;
            this.p24_chkDialDisplay_alwaysshow_vfos.Text = "Always show VFOs";
            this.toolTip1.SetToolTip(this.p24_chkDialDisplay_alwaysshow_vfos, "Will always show VFOa when on RX2 and VFOb on RX1 when RX2 enabled");
            this.p24_chkDialDisplay_alwaysshow_vfos.UseVisualStyleBackColor = true;
            this.p24_chkDialDisplay_alwaysshow_vfos.CheckedChanged += new System.EventHandler(this.chkDialDisplay_alwaysshow_vfos_CheckedChanged);

            // 
            // p24_chkDialDisplay_fade_rx
            // 
            this.p24_chkDialDisplay_fade_rx.AutoSize = true;
            this.p24_chkDialDisplay_fade_rx.Image = null;
            this.p24_chkDialDisplay_fade_rx.Location = new System.Drawing.Point(228, 19);
            this.p24_chkDialDisplay_fade_rx.Name = "p24_chkDialDisplay_fade_rx";
            this.p24_chkDialDisplay_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.p24_chkDialDisplay_fade_rx.TabIndex = 2;
            this.p24_chkDialDisplay_fade_rx.Text = "Fade on RX";
            this.p24_chkDialDisplay_fade_rx.UseVisualStyleBackColor = true;
            this.p24_chkDialDisplay_fade_rx.CheckedChanged += new System.EventHandler(this.chkDialDisplay_fade_rx_CheckedChanged);

            // 
            // p24_chkDialDisplay_fade_tx
            // 
            this.p24_chkDialDisplay_fade_tx.AutoSize = true;
            this.p24_chkDialDisplay_fade_tx.Image = null;
            this.p24_chkDialDisplay_fade_tx.Location = new System.Drawing.Point(228, 40);
            this.p24_chkDialDisplay_fade_tx.Name = "p24_chkDialDisplay_fade_tx";
            this.p24_chkDialDisplay_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.p24_chkDialDisplay_fade_tx.TabIndex = 3;
            this.p24_chkDialDisplay_fade_tx.Text = "Fade on TX";
            this.p24_chkDialDisplay_fade_tx.UseVisualStyleBackColor = true;
            this.p24_chkDialDisplay_fade_tx.CheckedChanged += new System.EventHandler(this.chkDialDisplay_fade_tx_CheckedChanged);

            // 
            // p24_chkDial_align
            // 
            this.p24_chkDial_align.AutoSize = true;
            this.p24_chkDial_align.Image = null;
            this.p24_chkDial_align.Location = new System.Drawing.Point(42, 86);
            this.p24_chkDial_align.Name = "p24_chkDial_align";
            this.p24_chkDial_align.Size = new System.Drawing.Size(165, 17);
            this.p24_chkDial_align.TabIndex = 176;
            this.p24_chkDial_align.Text = "Align frequency with tunestep";
            this.toolTip1.SetToolTip(this.p24_chkDial_align, "Align the frequency with tunstep");
            this.p24_chkDial_align.UseVisualStyleBackColor = true;
            this.p24_chkDial_align.CheckedChanged += new System.EventHandler(this.chkDial_align_CheckedChanged);

            // 
            // p24_chkFilterDisplay_fadeonrx
            // 
            this.p24_chkFilterDisplay_fadeonrx.AutoSize = true;
            this.p24_chkFilterDisplay_fadeonrx.Image = null;
            this.p24_chkFilterDisplay_fadeonrx.Location = new System.Drawing.Point(229, 39);
            this.p24_chkFilterDisplay_fadeonrx.Name = "p24_chkFilterDisplay_fadeonrx";
            this.p24_chkFilterDisplay_fadeonrx.Size = new System.Drawing.Size(83, 17);
            this.p24_chkFilterDisplay_fadeonrx.TabIndex = 2;
            this.p24_chkFilterDisplay_fadeonrx.Text = "Fade on RX";
            this.p24_chkFilterDisplay_fadeonrx.UseVisualStyleBackColor = true;
            this.p24_chkFilterDisplay_fadeonrx.CheckedChanged += new System.EventHandler(this.chkFilterDisplay_fadeonrx_CheckedChanged);

            // 
            // p24_chkFilterDisplay_fadeontx
            // 
            this.p24_chkFilterDisplay_fadeontx.AutoSize = true;
            this.p24_chkFilterDisplay_fadeontx.Image = null;
            this.p24_chkFilterDisplay_fadeontx.Location = new System.Drawing.Point(229, 60);
            this.p24_chkFilterDisplay_fadeontx.Name = "p24_chkFilterDisplay_fadeontx";
            this.p24_chkFilterDisplay_fadeontx.Size = new System.Drawing.Size(82, 17);
            this.p24_chkFilterDisplay_fadeontx.TabIndex = 3;
            this.p24_chkFilterDisplay_fadeontx.Text = "Fade on TX";
            this.p24_chkFilterDisplay_fadeontx.UseVisualStyleBackColor = true;
            this.p24_chkFilterDisplay_fadeontx.CheckedChanged += new System.EventHandler(this.chkFilterDisplay_fadeontx_CheckedChanged);

            // 
            // p24_chkFilterDisplay_fixed_tx_zoom
            // 
            this.p24_chkFilterDisplay_fixed_tx_zoom.AutoSize = true;
            this.p24_chkFilterDisplay_fixed_tx_zoom.Image = null;
            this.p24_chkFilterDisplay_fixed_tx_zoom.Location = new System.Drawing.Point(23, 111);
            this.p24_chkFilterDisplay_fixed_tx_zoom.Name = "p24_chkFilterDisplay_fixed_tx_zoom";
            this.p24_chkFilterDisplay_fixed_tx_zoom.Size = new System.Drawing.Size(96, 17);
            this.p24_chkFilterDisplay_fixed_tx_zoom.TabIndex = 141;
            this.p24_chkFilterDisplay_fixed_tx_zoom.Text = "Fixed TX zoom";
            this.p24_chkFilterDisplay_fixed_tx_zoom.UseVisualStyleBackColor = true;
            this.p24_chkFilterDisplay_fixed_tx_zoom.CheckedChanged += new System.EventHandler(this.chkFilterDisplay_fixed_tx_zoom_CheckedChanged);

            // 
            // p24_chkFilterDisplay_fixed_zoom
            // 
            this.p24_chkFilterDisplay_fixed_zoom.AutoSize = true;
            this.p24_chkFilterDisplay_fixed_zoom.Image = null;
            this.p24_chkFilterDisplay_fixed_zoom.Location = new System.Drawing.Point(23, 85);
            this.p24_chkFilterDisplay_fixed_zoom.Name = "p24_chkFilterDisplay_fixed_zoom";
            this.p24_chkFilterDisplay_fixed_zoom.Size = new System.Drawing.Size(97, 17);
            this.p24_chkFilterDisplay_fixed_zoom.TabIndex = 139;
            this.p24_chkFilterDisplay_fixed_zoom.Text = "Fixed RX zoom";
            this.p24_chkFilterDisplay_fixed_zoom.UseVisualStyleBackColor = true;
            this.p24_chkFilterDisplay_fixed_zoom.CheckedChanged += new System.EventHandler(this.chkFilterDisplay_fixed_zoom_CheckedChanged);

            // 
            // p24_chkFilterDisplay_show_limits
            // 
            this.p24_chkFilterDisplay_show_limits.AutoSize = true;
            this.p24_chkFilterDisplay_show_limits.Image = null;
            this.p24_chkFilterDisplay_show_limits.Location = new System.Drawing.Point(23, 62);
            this.p24_chkFilterDisplay_show_limits.Name = "p24_chkFilterDisplay_show_limits";
            this.p24_chkFilterDisplay_show_limits.Size = new System.Drawing.Size(100, 17);
            this.p24_chkFilterDisplay_show_limits.TabIndex = 138;
            this.p24_chkFilterDisplay_show_limits.Text = "Show filter limits";
            this.p24_chkFilterDisplay_show_limits.UseVisualStyleBackColor = true;
            this.p24_chkFilterDisplay_show_limits.CheckedChanged += new System.EventHandler(this.chkFilterDisplay_show_limits_CheckedChanged);

            // 
            // p24_chkFilter_characteristic
            // 
            this.p24_chkFilter_characteristic.AutoSize = true;
            this.p24_chkFilter_characteristic.Image = null;
            this.p24_chkFilter_characteristic.Location = new System.Drawing.Point(113, 187);
            this.p24_chkFilter_characteristic.Name = "p24_chkFilter_characteristic";
            this.p24_chkFilter_characteristic.Size = new System.Drawing.Size(125, 17);
            this.p24_chkFilter_characteristic.TabIndex = 201;
            this.p24_chkFilter_characteristic.Text = "Show Characteristics";
            this.toolTip1.SetToolTip(this.p24_chkFilter_characteristic, "Show the filter characteristic");
            this.p24_chkFilter_characteristic.UseVisualStyleBackColor = true;
            this.p24_chkFilter_characteristic.CheckedChanged += new System.EventHandler(this.chkFilter_characteristic_CheckedChanged);

            // 
            // p24_chkFilter_fill_spec
            // 
            this.p24_chkFilter_fill_spec.AutoSize = true;
            this.p24_chkFilter_fill_spec.Checked = true;
            this.p24_chkFilter_fill_spec.CheckState = System.Windows.Forms.CheckState.Checked;
            this.p24_chkFilter_fill_spec.Image = null;
            this.p24_chkFilter_fill_spec.Location = new System.Drawing.Point(13, 52);
            this.p24_chkFilter_fill_spec.Name = "p24_chkFilter_fill_spec";
            this.p24_chkFilter_fill_spec.Size = new System.Drawing.Size(86, 17);
            this.p24_chkFilter_fill_spec.TabIndex = 164;
            this.p24_chkFilter_fill_spec.Text = "Fill Spectrum";
            this.p24_chkFilter_fill_spec.UseVisualStyleBackColor = true;
            this.p24_chkFilter_fill_spec.CheckedChanged += new System.EventHandler(this.chkFilter_fill_spec_CheckedChanged);

            // 
            // p24_chkFilter_grey_outsidepb
            // 
            this.p24_chkFilter_grey_outsidepb.AutoSize = true;
            this.p24_chkFilter_grey_outsidepb.Checked = true;
            this.p24_chkFilter_grey_outsidepb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.p24_chkFilter_grey_outsidepb.Image = null;
            this.p24_chkFilter_grey_outsidepb.Location = new System.Drawing.Point(13, 29);
            this.p24_chkFilter_grey_outsidepb.Name = "p24_chkFilter_grey_outsidepb";
            this.p24_chkFilter_grey_outsidepb.Size = new System.Drawing.Size(100, 17);
            this.p24_chkFilter_grey_outsidepb.TabIndex = 194;
            this.p24_chkFilter_grey_outsidepb.Text = "Grey outside pb";
            this.toolTip1.SetToolTip(this.p24_chkFilter_grey_outsidepb, "Show a grey scale outside the passband");
            this.p24_chkFilter_grey_outsidepb.UseVisualStyleBackColor = true;
            this.p24_chkFilter_grey_outsidepb.CheckedChanged += new System.EventHandler(this.chkFilter_grey_outsidepb_CheckedChanged);

            // 
            // p24_chkFilter_sideband_mode
            // 
            this.p24_chkFilter_sideband_mode.AutoSize = true;
            this.p24_chkFilter_sideband_mode.Image = null;
            this.p24_chkFilter_sideband_mode.Location = new System.Drawing.Point(13, 5);
            this.p24_chkFilter_sideband_mode.Name = "p24_chkFilter_sideband_mode";
            this.p24_chkFilter_sideband_mode.Size = new System.Drawing.Size(100, 17);
            this.p24_chkFilter_sideband_mode.TabIndex = 189;
            this.p24_chkFilter_sideband_mode.Text = "Sideband mode";
            this.toolTip1.SetToolTip(this.p24_chkFilter_sideband_mode, "Align the filter display to the sideband");
            this.p24_chkFilter_sideband_mode.UseVisualStyleBackColor = true;
            this.p24_chkFilter_sideband_mode.CheckedChanged += new System.EventHandler(this.chkFilter_sideband_mode_CheckedChanged);

            // 
            // p24_chkHistory_1_show_axis
            // 
            this.p24_chkHistory_1_show_axis.AutoSize = true;
            this.p24_chkHistory_1_show_axis.Image = null;
            this.p24_chkHistory_1_show_axis.Location = new System.Drawing.Point(62, 0);
            this.p24_chkHistory_1_show_axis.Name = "p24_chkHistory_1_show_axis";
            this.p24_chkHistory_1_show_axis.Size = new System.Drawing.Size(15, 14);
            this.p24_chkHistory_1_show_axis.TabIndex = 145;
            this.p24_chkHistory_1_show_axis.UseVisualStyleBackColor = true;
            this.p24_chkHistory_1_show_axis.CheckedChanged += new System.EventHandler(this.chkHistory_1_show_axis_CheckedChanged);

            // 
            // p24_chkHistory_auto_0_scale
            // 
            this.p24_chkHistory_auto_0_scale.AutoSize = true;
            this.p24_chkHistory_auto_0_scale.Image = null;
            this.p24_chkHistory_auto_0_scale.Location = new System.Drawing.Point(73, 52);
            this.p24_chkHistory_auto_0_scale.Name = "p24_chkHistory_auto_0_scale";
            this.p24_chkHistory_auto_0_scale.Size = new System.Drawing.Size(78, 17);
            this.p24_chkHistory_auto_0_scale.TabIndex = 139;
            this.p24_chkHistory_auto_0_scale.Text = "Auto Scale";
            this.p24_chkHistory_auto_0_scale.UseVisualStyleBackColor = true;
            this.p24_chkHistory_auto_0_scale.CheckedChanged += new System.EventHandler(this.chkHistory_auto_0_scale_CheckedChanged);

            // 
            // p24_chkHistory_auto_1_scale
            // 
            this.p24_chkHistory_auto_1_scale.AutoSize = true;
            this.p24_chkHistory_auto_1_scale.Image = null;
            this.p24_chkHistory_auto_1_scale.Location = new System.Drawing.Point(73, 52);
            this.p24_chkHistory_auto_1_scale.Name = "p24_chkHistory_auto_1_scale";
            this.p24_chkHistory_auto_1_scale.Size = new System.Drawing.Size(78, 17);
            this.p24_chkHistory_auto_1_scale.TabIndex = 139;
            this.p24_chkHistory_auto_1_scale.Text = "Auto Scale";
            this.p24_chkHistory_auto_1_scale.UseVisualStyleBackColor = true;
            this.p24_chkHistory_auto_1_scale.CheckedChanged += new System.EventHandler(this.chkHistory_auto_1_scale_CheckedChanged);

            // 
            // p24_chkHistory_fade_rx
            // 
            this.p24_chkHistory_fade_rx.AutoSize = true;
            this.p24_chkHistory_fade_rx.Image = null;
            this.p24_chkHistory_fade_rx.Location = new System.Drawing.Point(229, 93);
            this.p24_chkHistory_fade_rx.Name = "p24_chkHistory_fade_rx";
            this.p24_chkHistory_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.p24_chkHistory_fade_rx.TabIndex = 2;
            this.p24_chkHistory_fade_rx.Text = "Fade on RX";
            this.p24_chkHistory_fade_rx.UseVisualStyleBackColor = true;
            this.p24_chkHistory_fade_rx.CheckedChanged += new System.EventHandler(this.chkHistory_fade_rx_CheckedChanged);

            // 
            // p24_chkHistory_fade_tx
            // 
            this.p24_chkHistory_fade_tx.AutoSize = true;
            this.p24_chkHistory_fade_tx.Image = null;
            this.p24_chkHistory_fade_tx.Location = new System.Drawing.Point(229, 116);
            this.p24_chkHistory_fade_tx.Name = "p24_chkHistory_fade_tx";
            this.p24_chkHistory_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.p24_chkHistory_fade_tx.TabIndex = 3;
            this.p24_chkHistory_fade_tx.Text = "Fade on TX";
            this.p24_chkHistory_fade_tx.UseVisualStyleBackColor = true;
            this.p24_chkHistory_fade_tx.CheckedChanged += new System.EventHandler(this.chkHistory_fade_tx_CheckedChanged);

            // 
            // p24_chkLedIndicator_FadeOnRX
            // 
            this.p24_chkLedIndicator_FadeOnRX.AutoSize = true;
            this.p24_chkLedIndicator_FadeOnRX.Image = null;
            this.p24_chkLedIndicator_FadeOnRX.Location = new System.Drawing.Point(219, 30);
            this.p24_chkLedIndicator_FadeOnRX.Name = "p24_chkLedIndicator_FadeOnRX";
            this.p24_chkLedIndicator_FadeOnRX.Size = new System.Drawing.Size(83, 17);
            this.p24_chkLedIndicator_FadeOnRX.TabIndex = 2;
            this.p24_chkLedIndicator_FadeOnRX.Text = "Fade on RX";
            this.p24_chkLedIndicator_FadeOnRX.UseVisualStyleBackColor = true;
            this.p24_chkLedIndicator_FadeOnRX.CheckedChanged += new System.EventHandler(this.chkLedIndicator_FadeOnRX_CheckedChanged);

            // 
            // p24_chkLedIndicator_FadeOnTX
            // 
            this.p24_chkLedIndicator_FadeOnTX.AutoSize = true;
            this.p24_chkLedIndicator_FadeOnTX.Image = null;
            this.p24_chkLedIndicator_FadeOnTX.Location = new System.Drawing.Point(219, 53);
            this.p24_chkLedIndicator_FadeOnTX.Name = "p24_chkLedIndicator_FadeOnTX";
            this.p24_chkLedIndicator_FadeOnTX.Size = new System.Drawing.Size(82, 17);
            this.p24_chkLedIndicator_FadeOnTX.TabIndex = 3;
            this.p24_chkLedIndicator_FadeOnTX.Text = "Fade on TX";
            this.p24_chkLedIndicator_FadeOnTX.UseVisualStyleBackColor = true;
            this.p24_chkLedIndicator_FadeOnTX.CheckedChanged += new System.EventHandler(this.chkLedIndicator_FadeOnTX_CheckedChanged);

            // 
            // p24_chkLedIndicator_ShowPanel
            // 
            this.p24_chkLedIndicator_ShowPanel.AutoSize = true;
            this.p24_chkLedIndicator_ShowPanel.Image = null;
            this.p24_chkLedIndicator_ShowPanel.Location = new System.Drawing.Point(18, 30);
            this.p24_chkLedIndicator_ShowPanel.Name = "p24_chkLedIndicator_ShowPanel";
            this.p24_chkLedIndicator_ShowPanel.Size = new System.Drawing.Size(83, 17);
            this.p24_chkLedIndicator_ShowPanel.TabIndex = 135;
            this.p24_chkLedIndicator_ShowPanel.Text = "Show Panel";
            this.p24_chkLedIndicator_ShowPanel.UseVisualStyleBackColor = true;
            this.p24_chkLedIndicator_ShowPanel.CheckedChanged += new System.EventHandler(this.chkLedIndicator_ShowPanel_CheckedChanged);

            // 
            // p24_chkLed_notx_false
            // 
            this.p24_chkLed_notx_false.AutoSize = true;
            this.p24_chkLed_notx_false.Image = null;
            this.p24_chkLed_notx_false.Location = new System.Drawing.Point(213, 189);
            this.p24_chkLed_notx_false.Name = "p24_chkLed_notx_false";
            this.p24_chkLed_notx_false.Size = new System.Drawing.Size(57, 17);
            this.p24_chkLed_notx_false.TabIndex = 174;
            this.p24_chkLed_notx_false.Text = "No TX";
            this.toolTip1.SetToolTip(this.p24_chkLed_notx_false, "Stop and/or prevent Tx/Mox");
            this.p24_chkLed_notx_false.UseVisualStyleBackColor = true;
            this.p24_chkLed_notx_false.CheckedChanged += new System.EventHandler(this.chkLed_notx_false_CheckedChanged);

            // 
            // p24_chkLed_notx_true
            // 
            this.p24_chkLed_notx_true.AutoSize = true;
            this.p24_chkLed_notx_true.Image = null;
            this.p24_chkLed_notx_true.Location = new System.Drawing.Point(213, 165);
            this.p24_chkLed_notx_true.Name = "p24_chkLed_notx_true";
            this.p24_chkLed_notx_true.Size = new System.Drawing.Size(57, 17);
            this.p24_chkLed_notx_true.TabIndex = 173;
            this.p24_chkLed_notx_true.Text = "No TX";
            this.toolTip1.SetToolTip(this.p24_chkLed_notx_true, "Stop and/or prevent Tx/Mox");
            this.p24_chkLed_notx_true.UseVisualStyleBackColor = true;
            this.p24_chkLed_notx_true.CheckedChanged += new System.EventHandler(this.chkLed_notx_true_CheckedChanged);

            // 
            // p24_chkLed_process_when_hidden
            // 
            this.p24_chkLed_process_when_hidden.AutoSize = true;
            this.p24_chkLed_process_when_hidden.Image = null;
            this.p24_chkLed_process_when_hidden.Location = new System.Drawing.Point(189, 353);
            this.p24_chkLed_process_when_hidden.Name = "p24_chkLed_process_when_hidden";
            this.p24_chkLed_process_when_hidden.Size = new System.Drawing.Size(128, 17);
            this.p24_chkLed_process_when_hidden.TabIndex = 180;
            this.p24_chkLed_process_when_hidden.Text = "Process when hidden";
            this.toolTip1.SetToolTip(this.p24_chkLed_process_when_hidden, "Will cause this led to continue to process the condition even when the parent con" +
        "tainer is hidden");
            this.p24_chkLed_process_when_hidden.UseVisualStyleBackColor = true;
            this.p24_chkLed_process_when_hidden.CheckedChanged += new System.EventHandler(this.chkLed_process_when_hidden_CheckedChanged);

            // 
            // p24_chkLed_show_false
            // 
            this.p24_chkLed_show_false.AutoSize = true;
            this.p24_chkLed_show_false.Image = null;
            this.p24_chkLed_show_false.Location = new System.Drawing.Point(154, 189);
            this.p24_chkLed_show_false.Name = "p24_chkLed_show_false";
            this.p24_chkLed_show_false.Size = new System.Drawing.Size(53, 17);
            this.p24_chkLed_show_false.TabIndex = 169;
            this.p24_chkLed_show_false.Text = "Show";
            this.p24_chkLed_show_false.UseVisualStyleBackColor = true;
            this.p24_chkLed_show_false.CheckedChanged += new System.EventHandler(this.chkLed_show_false_CheckedChanged);

            // 
            // p24_chkLed_show_true
            // 
            this.p24_chkLed_show_true.AutoSize = true;
            this.p24_chkLed_show_true.Image = null;
            this.p24_chkLed_show_true.Location = new System.Drawing.Point(154, 166);
            this.p24_chkLed_show_true.Name = "p24_chkLed_show_true";
            this.p24_chkLed_show_true.Size = new System.Drawing.Size(53, 17);
            this.p24_chkLed_show_true.TabIndex = 168;
            this.p24_chkLed_show_true.Text = "Show";
            this.p24_chkLed_show_true.UseVisualStyleBackColor = true;
            this.p24_chkLed_show_true.CheckedChanged += new System.EventHandler(this.chkLed_show_true_CheckedChanged);

            // 
            // p24_chkLockContainer
            // 
            this.p24_chkLockContainer.AutoSize = true;
            this.p24_chkLockContainer.Image = null;
            this.p24_chkLockContainer.Location = new System.Drawing.Point(310, 62);
            this.p24_chkLockContainer.Name = "p24_chkLockContainer";
            this.p24_chkLockContainer.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_chkLockContainer.Size = new System.Drawing.Size(50, 17);
            this.p24_chkLockContainer.TabIndex = 111;
            this.p24_chkLockContainer.Text = "Lock";
            this.toolTip1.SetToolTip(this.p24_chkLockContainer, "Lock the container to prevent removal and to prevent add/remove of items. You can" +
        " still make adjustments to items");
            this.p24_chkLockContainer.UseVisualStyleBackColor = true;
            this.p24_chkLockContainer.CheckedChanged += new System.EventHandler(this.chkLockContainer_CheckedChanged);

            // 
            // p24_chkMMClockTitle
            // 
            this.p24_chkMMClockTitle.AutoSize = true;
            this.p24_chkMMClockTitle.Image = null;
            this.p24_chkMMClockTitle.Location = new System.Drawing.Point(24, 89);
            this.p24_chkMMClockTitle.Name = "p24_chkMMClockTitle";
            this.p24_chkMMClockTitle.Size = new System.Drawing.Size(76, 17);
            this.p24_chkMMClockTitle.TabIndex = 109;
            this.p24_chkMMClockTitle.Text = "Meter Title";
            this.toolTip1.SetToolTip(this.p24_chkMMClockTitle, "Show meter title");
            this.p24_chkMMClockTitle.UseVisualStyleBackColor = true;
            this.p24_chkMMClockTitle.CheckedChanged += new System.EventHandler(this.chkMMClockTitle_CheckedChanged);

            // 
            // p24_chkMaintainNFAdjustDeltaRX1
            // 
            this.p24_chkMaintainNFAdjustDeltaRX1.AutoSize = true;
            this.p24_chkMaintainNFAdjustDeltaRX1.Image = null;
            this.p24_chkMaintainNFAdjustDeltaRX1.Location = new System.Drawing.Point(13, 140);
            this.p24_chkMaintainNFAdjustDeltaRX1.Name = "p24_chkMaintainNFAdjustDeltaRX1";
            this.p24_chkMaintainNFAdjustDeltaRX1.Size = new System.Drawing.Size(92, 17);
            this.p24_chkMaintainNFAdjustDeltaRX1.TabIndex = 85;
            this.p24_chkMaintainNFAdjustDeltaRX1.Text = "Maintain delta";
            this.toolTip1.SetToolTip(this.p24_chkMaintainNFAdjustDeltaRX1, "If min is adjusted, max will be changed by same adjustment");
            this.p24_chkMaintainNFAdjustDeltaRX1.UseVisualStyleBackColor = true;
            this.p24_chkMaintainNFAdjustDeltaRX1.CheckedChanged += new System.EventHandler(this.chkMaintainNFAdjustDeltaRX1_CheckedChanged);

            // 
            // p24_chkMaintainNFAdjustDeltaRX2
            // 
            this.p24_chkMaintainNFAdjustDeltaRX2.AutoSize = true;
            this.p24_chkMaintainNFAdjustDeltaRX2.Image = null;
            this.p24_chkMaintainNFAdjustDeltaRX2.Location = new System.Drawing.Point(13, 140);
            this.p24_chkMaintainNFAdjustDeltaRX2.Name = "p24_chkMaintainNFAdjustDeltaRX2";
            this.p24_chkMaintainNFAdjustDeltaRX2.Size = new System.Drawing.Size(92, 17);
            this.p24_chkMaintainNFAdjustDeltaRX2.TabIndex = 87;
            this.p24_chkMaintainNFAdjustDeltaRX2.Text = "Maintain delta";
            this.toolTip1.SetToolTip(this.p24_chkMaintainNFAdjustDeltaRX2, "If min is adjusted, max will be changed by same adjustment");
            this.p24_chkMaintainNFAdjustDeltaRX2.UseVisualStyleBackColor = true;
            this.p24_chkMaintainNFAdjustDeltaRX2.CheckedChanged += new System.EventHandler(this.chkMaintainNFAdjustDeltaRX2_CheckedChanged);

            // 
            // p24_chkMeterItemDarkMode
            // 
            this.p24_chkMeterItemDarkMode.AutoSize = true;
            this.p24_chkMeterItemDarkMode.Image = null;
            this.p24_chkMeterItemDarkMode.Location = new System.Drawing.Point(183, 220);
            this.p24_chkMeterItemDarkMode.Name = "p24_chkMeterItemDarkMode";
            this.p24_chkMeterItemDarkMode.Size = new System.Drawing.Size(79, 17);
            this.p24_chkMeterItemDarkMode.TabIndex = 112;
            this.p24_chkMeterItemDarkMode.Text = "Dark Mode";
            this.p24_chkMeterItemDarkMode.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemDarkMode.CheckedChanged += new System.EventHandler(this.chkMeterItemDarkMode_CheckedChanged);

            // 
            // p24_chkMeterItemDarkModeRotator
            // 
            this.p24_chkMeterItemDarkModeRotator.AutoSize = true;
            this.p24_chkMeterItemDarkModeRotator.Image = null;
            this.p24_chkMeterItemDarkModeRotator.Location = new System.Drawing.Point(220, 137);
            this.p24_chkMeterItemDarkModeRotator.Name = "p24_chkMeterItemDarkModeRotator";
            this.p24_chkMeterItemDarkModeRotator.Size = new System.Drawing.Size(79, 17);
            this.p24_chkMeterItemDarkModeRotator.TabIndex = 112;
            this.p24_chkMeterItemDarkModeRotator.Text = "Dark Mode";
            this.p24_chkMeterItemDarkModeRotator.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemDarkModeRotator.CheckedChanged += new System.EventHandler(this.chkMeterItemDarkModeRotator_CheckedChanged);

            // 
            // p24_chkMeterItemFadeOnRx
            // 
            this.p24_chkMeterItemFadeOnRx.AutoSize = true;
            this.p24_chkMeterItemFadeOnRx.Image = null;
            this.p24_chkMeterItemFadeOnRx.Location = new System.Drawing.Point(4, 57);
            this.p24_chkMeterItemFadeOnRx.Name = "p24_chkMeterItemFadeOnRx";
            this.p24_chkMeterItemFadeOnRx.Size = new System.Drawing.Size(83, 17);
            this.p24_chkMeterItemFadeOnRx.TabIndex = 0;
            this.p24_chkMeterItemFadeOnRx.Text = "Fade on RX";
            this.p24_chkMeterItemFadeOnRx.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemFadeOnRx.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnRx_CheckedChanged);

            // 
            // p24_chkMeterItemFadeOnRxRotator
            // 
            this.p24_chkMeterItemFadeOnRxRotator.AutoSize = true;
            this.p24_chkMeterItemFadeOnRxRotator.Image = null;
            this.p24_chkMeterItemFadeOnRxRotator.Location = new System.Drawing.Point(220, 87);
            this.p24_chkMeterItemFadeOnRxRotator.Name = "p24_chkMeterItemFadeOnRxRotator";
            this.p24_chkMeterItemFadeOnRxRotator.Size = new System.Drawing.Size(83, 17);
            this.p24_chkMeterItemFadeOnRxRotator.TabIndex = 0;
            this.p24_chkMeterItemFadeOnRxRotator.Text = "Fade on RX";
            this.p24_chkMeterItemFadeOnRxRotator.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemFadeOnRxRotator.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnRxRotator_CheckedChanged);

            // 
            // p24_chkMeterItemFadeOnRxSpacer
            // 
            this.p24_chkMeterItemFadeOnRxSpacer.AutoSize = true;
            this.p24_chkMeterItemFadeOnRxSpacer.Image = null;
            this.p24_chkMeterItemFadeOnRxSpacer.Location = new System.Drawing.Point(59, 89);
            this.p24_chkMeterItemFadeOnRxSpacer.Name = "p24_chkMeterItemFadeOnRxSpacer";
            this.p24_chkMeterItemFadeOnRxSpacer.Size = new System.Drawing.Size(83, 17);
            this.p24_chkMeterItemFadeOnRxSpacer.TabIndex = 2;
            this.p24_chkMeterItemFadeOnRxSpacer.Text = "Fade on RX";
            this.p24_chkMeterItemFadeOnRxSpacer.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemFadeOnRxSpacer.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnRxSpacer_CheckedChanged);

            // 
            // p24_chkMeterItemFadeOnTx
            // 
            this.p24_chkMeterItemFadeOnTx.AutoSize = true;
            this.p24_chkMeterItemFadeOnTx.Image = null;
            this.p24_chkMeterItemFadeOnTx.Location = new System.Drawing.Point(4, 80);
            this.p24_chkMeterItemFadeOnTx.Name = "p24_chkMeterItemFadeOnTx";
            this.p24_chkMeterItemFadeOnTx.Size = new System.Drawing.Size(82, 17);
            this.p24_chkMeterItemFadeOnTx.TabIndex = 1;
            this.p24_chkMeterItemFadeOnTx.Text = "Fade on TX";
            this.p24_chkMeterItemFadeOnTx.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemFadeOnTx.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnTx_CheckedChanged);

            // 
            // p24_chkMeterItemFadeOnTxRotator
            // 
            this.p24_chkMeterItemFadeOnTxRotator.AutoSize = true;
            this.p24_chkMeterItemFadeOnTxRotator.Image = null;
            this.p24_chkMeterItemFadeOnTxRotator.Location = new System.Drawing.Point(220, 110);
            this.p24_chkMeterItemFadeOnTxRotator.Name = "p24_chkMeterItemFadeOnTxRotator";
            this.p24_chkMeterItemFadeOnTxRotator.Size = new System.Drawing.Size(82, 17);
            this.p24_chkMeterItemFadeOnTxRotator.TabIndex = 1;
            this.p24_chkMeterItemFadeOnTxRotator.Text = "Fade on TX";
            this.p24_chkMeterItemFadeOnTxRotator.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemFadeOnTxRotator.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnTxRotator_CheckedChanged);

            // 
            // p24_chkMeterItemFadeOnTxSpacer
            // 
            this.p24_chkMeterItemFadeOnTxSpacer.AutoSize = true;
            this.p24_chkMeterItemFadeOnTxSpacer.Image = null;
            this.p24_chkMeterItemFadeOnTxSpacer.Location = new System.Drawing.Point(59, 112);
            this.p24_chkMeterItemFadeOnTxSpacer.Name = "p24_chkMeterItemFadeOnTxSpacer";
            this.p24_chkMeterItemFadeOnTxSpacer.Size = new System.Drawing.Size(82, 17);
            this.p24_chkMeterItemFadeOnTxSpacer.TabIndex = 3;
            this.p24_chkMeterItemFadeOnTxSpacer.Text = "Fade on TX";
            this.p24_chkMeterItemFadeOnTxSpacer.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemFadeOnTxSpacer.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnTxSpacer_CheckedChanged);

            // 
            // p24_chkMeterItemHistory
            // 
            this.p24_chkMeterItemHistory.AutoSize = true;
            this.p24_chkMeterItemHistory.Image = null;
            this.p24_chkMeterItemHistory.Location = new System.Drawing.Point(183, 103);
            this.p24_chkMeterItemHistory.Name = "p24_chkMeterItemHistory";
            this.p24_chkMeterItemHistory.Size = new System.Drawing.Size(88, 17);
            this.p24_chkMeterItemHistory.TabIndex = 2;
            this.p24_chkMeterItemHistory.Text = "Show History";
            this.p24_chkMeterItemHistory.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemHistory.CheckedChanged += new System.EventHandler(this.chkMeterItemHistory_CheckedChanged);

            // 
            // p24_chkMeterItemPeakHold
            // 
            this.p24_chkMeterItemPeakHold.AutoSize = true;
            this.p24_chkMeterItemPeakHold.Image = null;
            this.p24_chkMeterItemPeakHold.Location = new System.Drawing.Point(183, 149);
            this.p24_chkMeterItemPeakHold.Name = "p24_chkMeterItemPeakHold";
            this.p24_chkMeterItemPeakHold.Size = new System.Drawing.Size(106, 17);
            this.p24_chkMeterItemPeakHold.TabIndex = 3;
            this.p24_chkMeterItemPeakHold.Text = "Show Peak Hold";
            this.p24_chkMeterItemPeakHold.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemPeakHold.CheckedChanged += new System.EventHandler(this.chkMeterItemPeakHold_CheckedChanged);

            // 
            // p24_chkMeterItemPeakValue
            // 
            this.p24_chkMeterItemPeakValue.AutoSize = true;
            this.p24_chkMeterItemPeakValue.Image = null;
            this.p24_chkMeterItemPeakValue.Location = new System.Drawing.Point(4, 195);
            this.p24_chkMeterItemPeakValue.Name = "p24_chkMeterItemPeakValue";
            this.p24_chkMeterItemPeakValue.Size = new System.Drawing.Size(81, 17);
            this.p24_chkMeterItemPeakValue.TabIndex = 96;
            this.p24_chkMeterItemPeakValue.Text = "Peak Value";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemPeakValue, "Show peak value");
            this.p24_chkMeterItemPeakValue.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemPeakValue.CheckedChanged += new System.EventHandler(this.chkMeterItemPeakValue_CheckedChanged);

            // 
            // p24_chkMeterItemRotatorAllowControl
            // 
            this.p24_chkMeterItemRotatorAllowControl.AutoSize = true;
            this.p24_chkMeterItemRotatorAllowControl.Image = null;
            this.p24_chkMeterItemRotatorAllowControl.Location = new System.Drawing.Point(23, 272);
            this.p24_chkMeterItemRotatorAllowControl.Name = "p24_chkMeterItemRotatorAllowControl";
            this.p24_chkMeterItemRotatorAllowControl.Size = new System.Drawing.Size(86, 17);
            this.p24_chkMeterItemRotatorAllowControl.TabIndex = 136;
            this.p24_chkMeterItemRotatorAllowControl.Text = "Allow control";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemRotatorAllowControl, "Allow rotator control. Click/hold/drag on the rotator");
            this.p24_chkMeterItemRotatorAllowControl.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemRotatorAllowControl.CheckedChanged += new System.EventHandler(this.chkMeterItemRotatorAllowControl_CheckedChanged);

            // 
            // p24_chkMeterItemRotatorCardinals
            // 
            this.p24_chkMeterItemRotatorCardinals.AutoSize = true;
            this.p24_chkMeterItemRotatorCardinals.Image = null;
            this.p24_chkMeterItemRotatorCardinals.Location = new System.Drawing.Point(23, 249);
            this.p24_chkMeterItemRotatorCardinals.Name = "p24_chkMeterItemRotatorCardinals";
            this.p24_chkMeterItemRotatorCardinals.Size = new System.Drawing.Size(69, 17);
            this.p24_chkMeterItemRotatorCardinals.TabIndex = 134;
            this.p24_chkMeterItemRotatorCardinals.Text = "Cardinals";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemRotatorCardinals, "Show cardinals instead of degrees");
            this.p24_chkMeterItemRotatorCardinals.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemRotatorCardinals.CheckedChanged += new System.EventHandler(this.chkMeterItemRotatorCardinals_CheckedChanged);

            // 
            // p24_chkMeterItemRotatorShowBeamWidth
            // 
            this.p24_chkMeterItemRotatorShowBeamWidth.AutoSize = true;
            this.p24_chkMeterItemRotatorShowBeamWidth.Image = null;
            this.p24_chkMeterItemRotatorShowBeamWidth.Location = new System.Drawing.Point(129, 144);
            this.p24_chkMeterItemRotatorShowBeamWidth.Name = "p24_chkMeterItemRotatorShowBeamWidth";
            this.p24_chkMeterItemRotatorShowBeamWidth.Size = new System.Drawing.Size(53, 17);
            this.p24_chkMeterItemRotatorShowBeamWidth.TabIndex = 122;
            this.p24_chkMeterItemRotatorShowBeamWidth.Text = "Show";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemRotatorShowBeamWidth, "Show the 3dB beam width");
            this.p24_chkMeterItemRotatorShowBeamWidth.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemRotatorShowBeamWidth.CheckedChanged += new System.EventHandler(this.chkMeterItemRotatorShowBeamWidth_CheckedChanged);

            // 
            // p24_chkMeterItemSegmented
            // 
            this.p24_chkMeterItemSegmented.AutoSize = true;
            this.p24_chkMeterItemSegmented.Image = null;
            this.p24_chkMeterItemSegmented.Location = new System.Drawing.Point(4, 126);
            this.p24_chkMeterItemSegmented.Name = "p24_chkMeterItemSegmented";
            this.p24_chkMeterItemSegmented.Size = new System.Drawing.Size(80, 17);
            this.p24_chkMeterItemSegmented.TabIndex = 94;
            this.p24_chkMeterItemSegmented.Text = "Segmented";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemSegmented, "Segmented bar");
            this.p24_chkMeterItemSegmented.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemSegmented.CheckedChanged += new System.EventHandler(this.chkMeterItemSegmented_CheckedChanged);

            // 
            // p24_chkMeterItemShadow
            // 
            this.p24_chkMeterItemShadow.AutoSize = true;
            this.p24_chkMeterItemShadow.Image = null;
            this.p24_chkMeterItemShadow.Location = new System.Drawing.Point(4, 103);
            this.p24_chkMeterItemShadow.Name = "p24_chkMeterItemShadow";
            this.p24_chkMeterItemShadow.Size = new System.Drawing.Size(65, 17);
            this.p24_chkMeterItemShadow.TabIndex = 4;
            this.p24_chkMeterItemShadow.Text = "Shadow";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemShadow, "Show shadow on needles");
            this.p24_chkMeterItemShadow.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemShadow.CheckedChanged += new System.EventHandler(this.chkMeterItemShadow_CheckedChanged);

            // 
            // p24_chkMeterItemShowIndicator
            // 
            this.p24_chkMeterItemShowIndicator.AutoSize = true;
            this.p24_chkMeterItemShowIndicator.Image = null;
            this.p24_chkMeterItemShowIndicator.Location = new System.Drawing.Point(100, 33);
            this.p24_chkMeterItemShowIndicator.Name = "p24_chkMeterItemShowIndicator";
            this.p24_chkMeterItemShowIndicator.Size = new System.Drawing.Size(53, 17);
            this.p24_chkMeterItemShowIndicator.TabIndex = 119;
            this.p24_chkMeterItemShowIndicator.Text = "Show";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemShowIndicator, "Show the indicator line on horizontal bar meters");
            this.p24_chkMeterItemShowIndicator.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemShowIndicator.CheckedChanged += new System.EventHandler(this.chkMeterItemShowIndicator_CheckedChanged);

            // 
            // p24_chkMeterItemShowSubIndicator
            // 
            this.p24_chkMeterItemShowSubIndicator.AutoSize = true;
            this.p24_chkMeterItemShowSubIndicator.Image = null;
            this.p24_chkMeterItemShowSubIndicator.Location = new System.Drawing.Point(246, 33);
            this.p24_chkMeterItemShowSubIndicator.Name = "p24_chkMeterItemShowSubIndicator";
            this.p24_chkMeterItemShowSubIndicator.Size = new System.Drawing.Size(53, 17);
            this.p24_chkMeterItemShowSubIndicator.TabIndex = 122;
            this.p24_chkMeterItemShowSubIndicator.Text = "Show";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemShowSubIndicator, "Show sub indicators");
            this.p24_chkMeterItemShowSubIndicator.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemShowSubIndicator.CheckedChanged += new System.EventHandler(this.chkMeterItemShowSubIndicator_CheckedChanged);

            // 
            // p24_chkMeterItemSolid
            // 
            this.p24_chkMeterItemSolid.AutoSize = true;
            this.p24_chkMeterItemSolid.Image = null;
            this.p24_chkMeterItemSolid.Location = new System.Drawing.Point(4, 149);
            this.p24_chkMeterItemSolid.Name = "p24_chkMeterItemSolid";
            this.p24_chkMeterItemSolid.Size = new System.Drawing.Size(49, 17);
            this.p24_chkMeterItemSolid.TabIndex = 115;
            this.p24_chkMeterItemSolid.Text = "Solid";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemSolid, "Solid bar");
            this.p24_chkMeterItemSolid.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemSolid.CheckedChanged += new System.EventHandler(this.chkMeterItemSolid_CheckedChanged);

            // 
            // p24_chkMeterItemTitle
            // 
            this.p24_chkMeterItemTitle.AutoSize = true;
            this.p24_chkMeterItemTitle.Image = null;
            this.p24_chkMeterItemTitle.Location = new System.Drawing.Point(4, 172);
            this.p24_chkMeterItemTitle.Name = "p24_chkMeterItemTitle";
            this.p24_chkMeterItemTitle.Size = new System.Drawing.Size(76, 17);
            this.p24_chkMeterItemTitle.TabIndex = 95;
            this.p24_chkMeterItemTitle.Text = "Meter Title";
            this.toolTip1.SetToolTip(this.p24_chkMeterItemTitle, "Show meter title");
            this.p24_chkMeterItemTitle.UseVisualStyleBackColor = true;
            this.p24_chkMeterItemTitle.CheckedChanged += new System.EventHandler(this.chkMeterItemTitle_CheckedChanged);

            // 
            // p24_chkMultiMeter_auto_container_height
            // 
            this.p24_chkMultiMeter_auto_container_height.AutoSize = true;
            this.p24_chkMultiMeter_auto_container_height.Image = null;
            this.p24_chkMultiMeter_auto_container_height.Location = new System.Drawing.Point(119, 81);
            this.p24_chkMultiMeter_auto_container_height.Name = "p24_chkMultiMeter_auto_container_height";
            this.p24_chkMultiMeter_auto_container_height.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_chkMultiMeter_auto_container_height.Size = new System.Drawing.Size(80, 17);
            this.p24_chkMultiMeter_auto_container_height.TabIndex = 109;
            this.p24_chkMultiMeter_auto_container_height.Text = "Auto height";
            this.toolTip1.SetToolTip(this.p24_chkMultiMeter_auto_container_height, "Automatically adjust height of container to fit");
            this.p24_chkMultiMeter_auto_container_height.UseVisualStyleBackColor = true;
            this.p24_chkMultiMeter_auto_container_height.CheckedChanged += new System.EventHandler(this.chkMultiMeter_auto_container_height_CheckedChanged);

            // 
            // p24_chkMultiMeter_vfo_show_bandtext
            // 
            this.p24_chkMultiMeter_vfo_show_bandtext.AutoSize = true;
            this.p24_chkMultiMeter_vfo_show_bandtext.Image = null;
            this.p24_chkMultiMeter_vfo_show_bandtext.Location = new System.Drawing.Point(184, 183);
            this.p24_chkMultiMeter_vfo_show_bandtext.Name = "p24_chkMultiMeter_vfo_show_bandtext";
            this.p24_chkMultiMeter_vfo_show_bandtext.Size = new System.Drawing.Size(105, 17);
            this.p24_chkMultiMeter_vfo_show_bandtext.TabIndex = 135;
            this.p24_chkMultiMeter_vfo_show_bandtext.Text = "Show Band Text";
            this.p24_chkMultiMeter_vfo_show_bandtext.UseVisualStyleBackColor = true;
            this.p24_chkMultiMeter_vfo_show_bandtext.CheckedChanged += new System.EventHandler(this.chkMultiMeter_vfo_show_bandtext_CheckedChanged);

            // 
            // p24_chkRecording_canRepeat
            // 
            this.p24_chkRecording_canRepeat.AutoSize = true;
            this.p24_chkRecording_canRepeat.Image = null;
            this.p24_chkRecording_canRepeat.Location = new System.Drawing.Point(3, 27);
            this.p24_chkRecording_canRepeat.Name = "p24_chkRecording_canRepeat";
            this.p24_chkRecording_canRepeat.Size = new System.Drawing.Size(78, 17);
            this.p24_chkRecording_canRepeat.TabIndex = 2;
            this.p24_chkRecording_canRepeat.Text = "Can repeat";
            this.toolTip1.SetToolTip(this.p24_chkRecording_canRepeat, "If checked, and the slot is in repeat mode (long hold click) then \r\nit will auto " +
        "play again after an inital manual play");
            this.p24_chkRecording_canRepeat.UseVisualStyleBackColor = true;
            this.p24_chkRecording_canRepeat.CheckedChanged += new System.EventHandler(this.chkRecording_canRepeat_CheckedChanged);

            // 
            // p24_chkRecording_globalkeybind
            // 
            this.p24_chkRecording_globalkeybind.AutoSize = true;
            this.p24_chkRecording_globalkeybind.Image = null;
            this.p24_chkRecording_globalkeybind.Location = new System.Drawing.Point(6, 25);
            this.p24_chkRecording_globalkeybind.Name = "p24_chkRecording_globalkeybind";
            this.p24_chkRecording_globalkeybind.Size = new System.Drawing.Size(64, 17);
            this.p24_chkRecording_globalkeybind.TabIndex = 181;
            this.p24_chkRecording_globalkeybind.Text = "Keybind";
            this.toolTip1.SetToolTip(this.p24_chkRecording_globalkeybind, "Assign a key that will stop any recording / playback");
            this.p24_chkRecording_globalkeybind.UseVisualStyleBackColor = true;
            this.p24_chkRecording_globalkeybind.CheckedChanged += new System.EventHandler(this.chkRecording_globalkeybind_CheckedChanged);

            // 
            // p24_chkRecording_ignore_play_tempchanges
            // 
            this.p24_chkRecording_ignore_play_tempchanges.AutoSize = true;
            this.p24_chkRecording_ignore_play_tempchanges.Checked = true;
            this.p24_chkRecording_ignore_play_tempchanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.p24_chkRecording_ignore_play_tempchanges.Image = null;
            this.p24_chkRecording_ignore_play_tempchanges.Location = new System.Drawing.Point(3, 142);
            this.p24_chkRecording_ignore_play_tempchanges.Name = "p24_chkRecording_ignore_play_tempchanges";
            this.p24_chkRecording_ignore_play_tempchanges.Size = new System.Drawing.Size(122, 17);
            this.p24_chkRecording_ignore_play_tempchanges.TabIndex = 184;
            this.p24_chkRecording_ignore_play_tempchanges.Text = "Ignore play changes";
            this.toolTip1.SetToolTip(this.p24_chkRecording_ignore_play_tempchanges, "Ignore the temporary changes when playing.\r\nSettings for these changes are in Aud" +
        "io->Recording.");
            this.p24_chkRecording_ignore_play_tempchanges.UseVisualStyleBackColor = true;
            this.p24_chkRecording_ignore_play_tempchanges.CheckedChanged += new System.EventHandler(this.chkRecording_ignore_play_tempchanges_CheckedChanged);

            // 
            // p24_chkRecording_ignore_record_tempchanges
            // 
            this.p24_chkRecording_ignore_record_tempchanges.AutoSize = true;
            this.p24_chkRecording_ignore_record_tempchanges.Checked = true;
            this.p24_chkRecording_ignore_record_tempchanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.p24_chkRecording_ignore_record_tempchanges.Image = null;
            this.p24_chkRecording_ignore_record_tempchanges.Location = new System.Drawing.Point(3, 165);
            this.p24_chkRecording_ignore_record_tempchanges.Name = "p24_chkRecording_ignore_record_tempchanges";
            this.p24_chkRecording_ignore_record_tempchanges.Size = new System.Drawing.Size(118, 17);
            this.p24_chkRecording_ignore_record_tempchanges.TabIndex = 185;
            this.p24_chkRecording_ignore_record_tempchanges.Text = "Ignore rec changes";
            this.toolTip1.SetToolTip(this.p24_chkRecording_ignore_record_tempchanges, "Ignore the temporary changes when recording.\r\nSettings for these changes are in A" +
        "udio->Recording.");
            this.p24_chkRecording_ignore_record_tempchanges.UseVisualStyleBackColor = true;
            this.p24_chkRecording_ignore_record_tempchanges.CheckedChanged += new System.EventHandler(this.chkRecording_ignore_record_tempchanges_CheckedChanged);

            // 
            // p24_chkRecording_playkeybind
            // 
            this.p24_chkRecording_playkeybind.AutoSize = true;
            this.p24_chkRecording_playkeybind.Image = null;
            this.p24_chkRecording_playkeybind.Location = new System.Drawing.Point(3, 73);
            this.p24_chkRecording_playkeybind.Name = "p24_chkRecording_playkeybind";
            this.p24_chkRecording_playkeybind.Size = new System.Drawing.Size(86, 17);
            this.p24_chkRecording_playkeybind.TabIndex = 178;
            this.p24_chkRecording_playkeybind.Text = "Play keybind";
            this.toolTip1.SetToolTip(this.p24_chkRecording_playkeybind, "Assign a keypress combo to play/stop this");
            this.p24_chkRecording_playkeybind.UseVisualStyleBackColor = true;
            this.p24_chkRecording_playkeybind.CheckedChanged += new System.EventHandler(this.chkRecording_playkeybind_CheckedChanged);

            // 
            // p24_chkRecording_slot_locked
            // 
            this.p24_chkRecording_slot_locked.AutoSize = true;
            this.p24_chkRecording_slot_locked.Image = null;
            this.p24_chkRecording_slot_locked.Location = new System.Drawing.Point(3, 50);
            this.p24_chkRecording_slot_locked.Name = "p24_chkRecording_slot_locked";
            this.p24_chkRecording_slot_locked.Size = new System.Drawing.Size(62, 17);
            this.p24_chkRecording_slot_locked.TabIndex = 176;
            this.p24_chkRecording_slot_locked.Text = "Locked";
            this.toolTip1.SetToolTip(this.p24_chkRecording_slot_locked, "Prevent recording into this slot");
            this.p24_chkRecording_slot_locked.UseVisualStyleBackColor = true;
            this.p24_chkRecording_slot_locked.CheckedChanged += new System.EventHandler(this.chkRecording_slot_locked_CheckedChanged);

            // 
            // p24_chkTextOverlay_FadeOnRX
            // 
            this.p24_chkTextOverlay_FadeOnRX.AutoSize = true;
            this.p24_chkTextOverlay_FadeOnRX.Image = null;
            this.p24_chkTextOverlay_FadeOnRX.Location = new System.Drawing.Point(219, 30);
            this.p24_chkTextOverlay_FadeOnRX.Name = "p24_chkTextOverlay_FadeOnRX";
            this.p24_chkTextOverlay_FadeOnRX.Size = new System.Drawing.Size(83, 17);
            this.p24_chkTextOverlay_FadeOnRX.TabIndex = 2;
            this.p24_chkTextOverlay_FadeOnRX.Text = "Fade on RX";
            this.p24_chkTextOverlay_FadeOnRX.UseVisualStyleBackColor = true;
            this.p24_chkTextOverlay_FadeOnRX.CheckedChanged += new System.EventHandler(this.chkTextOverlay_FadeOnRX_CheckedChanged);

            // 
            // p24_chkTextOverlay_FadeOnTX
            // 
            this.p24_chkTextOverlay_FadeOnTX.AutoSize = true;
            this.p24_chkTextOverlay_FadeOnTX.Image = null;
            this.p24_chkTextOverlay_FadeOnTX.Location = new System.Drawing.Point(219, 53);
            this.p24_chkTextOverlay_FadeOnTX.Name = "p24_chkTextOverlay_FadeOnTX";
            this.p24_chkTextOverlay_FadeOnTX.Size = new System.Drawing.Size(82, 17);
            this.p24_chkTextOverlay_FadeOnTX.TabIndex = 3;
            this.p24_chkTextOverlay_FadeOnTX.Text = "Fade on TX";
            this.p24_chkTextOverlay_FadeOnTX.UseVisualStyleBackColor = true;
            this.p24_chkTextOverlay_FadeOnTX.CheckedChanged += new System.EventHandler(this.chkTextOverlay_FadeOnTX_CheckedChanged);

            // 
            // p24_chkTextOverlay_ShowPanel
            // 
            this.p24_chkTextOverlay_ShowPanel.AutoSize = true;
            this.p24_chkTextOverlay_ShowPanel.Image = null;
            this.p24_chkTextOverlay_ShowPanel.Location = new System.Drawing.Point(18, 30);
            this.p24_chkTextOverlay_ShowPanel.Name = "p24_chkTextOverlay_ShowPanel";
            this.p24_chkTextOverlay_ShowPanel.Size = new System.Drawing.Size(83, 17);
            this.p24_chkTextOverlay_ShowPanel.TabIndex = 135;
            this.p24_chkTextOverlay_ShowPanel.Text = "Show Panel";
            this.p24_chkTextOverlay_ShowPanel.UseVisualStyleBackColor = true;
            this.p24_chkTextOverlay_ShowPanel.CheckedChanged += new System.EventHandler(this.chkTextOverlay_ShowPanel_CheckedChanged);

            // 
            // p24_chkTextOverlay_rx_on_led
            // 
            this.p24_chkTextOverlay_rx_on_led.AutoSize = true;
            this.p24_chkTextOverlay_rx_on_led.Image = null;
            this.p24_chkTextOverlay_rx_on_led.Location = new System.Drawing.Point(8, 242);
            this.p24_chkTextOverlay_rx_on_led.Name = "p24_chkTextOverlay_rx_on_led";
            this.p24_chkTextOverlay_rx_on_led.Size = new System.Drawing.Size(77, 17);
            this.p24_chkTextOverlay_rx_on_led.TabIndex = 166;
            this.p24_chkTextOverlay_rx_on_led.Text = "RX on Led";
            this.toolTip1.SetToolTip(this.p24_chkTextOverlay_rx_on_led, "Show RX text only when this Led Indicator is true");
            this.p24_chkTextOverlay_rx_on_led.UseVisualStyleBackColor = true;
            this.p24_chkTextOverlay_rx_on_led.CheckedChanged += new System.EventHandler(this.chkTextOverlay_rx_on_led_CheckedChanged);

            // 
            // p24_chkTextOverlay_textback1
            // 
            this.p24_chkTextOverlay_textback1.AutoSize = true;
            this.p24_chkTextOverlay_textback1.Image = null;
            this.p24_chkTextOverlay_textback1.Location = new System.Drawing.Point(179, 187);
            this.p24_chkTextOverlay_textback1.Name = "p24_chkTextOverlay_textback1";
            this.p24_chkTextOverlay_textback1.Size = new System.Drawing.Size(51, 17);
            this.p24_chkTextOverlay_textback1.TabIndex = 158;
            this.p24_chkTextOverlay_textback1.Text = "Back";
            this.p24_chkTextOverlay_textback1.UseVisualStyleBackColor = true;
            this.p24_chkTextOverlay_textback1.CheckedChanged += new System.EventHandler(this.chkTextOverlay_textback1_CheckedChanged);

            // 
            // p24_chkTextOverlay_textback2
            // 
            this.p24_chkTextOverlay_textback2.AutoSize = true;
            this.p24_chkTextOverlay_textback2.Image = null;
            this.p24_chkTextOverlay_textback2.Location = new System.Drawing.Point(179, 213);
            this.p24_chkTextOverlay_textback2.Name = "p24_chkTextOverlay_textback2";
            this.p24_chkTextOverlay_textback2.Size = new System.Drawing.Size(51, 17);
            this.p24_chkTextOverlay_textback2.TabIndex = 159;
            this.p24_chkTextOverlay_textback2.Text = "Back";
            this.p24_chkTextOverlay_textback2.UseVisualStyleBackColor = true;
            this.p24_chkTextOverlay_textback2.CheckedChanged += new System.EventHandler(this.chkTextOverlay_textback2_CheckedChanged);

            // 
            // p24_chkTextOverlay_tx_on_led
            // 
            this.p24_chkTextOverlay_tx_on_led.AutoSize = true;
            this.p24_chkTextOverlay_tx_on_led.Image = null;
            this.p24_chkTextOverlay_tx_on_led.Location = new System.Drawing.Point(140, 242);
            this.p24_chkTextOverlay_tx_on_led.Name = "p24_chkTextOverlay_tx_on_led";
            this.p24_chkTextOverlay_tx_on_led.Size = new System.Drawing.Size(76, 17);
            this.p24_chkTextOverlay_tx_on_led.TabIndex = 168;
            this.p24_chkTextOverlay_tx_on_led.Text = "TX on Led";
            this.toolTip1.SetToolTip(this.p24_chkTextOverlay_tx_on_led, "Show TX text only when this Led Indicator is true");
            this.p24_chkTextOverlay_tx_on_led.UseVisualStyleBackColor = true;
            this.p24_chkTextOverlay_tx_on_led.CheckedChanged += new System.EventHandler(this.chkTextOverlay_tx_on_led_CheckedChanged);

            // 
            // p24_chkWaveRecord_fade_rx
            // 
            this.p24_chkWaveRecord_fade_rx.AutoSize = true;
            this.p24_chkWaveRecord_fade_rx.Image = null;
            this.p24_chkWaveRecord_fade_rx.Location = new System.Drawing.Point(205, 18);
            this.p24_chkWaveRecord_fade_rx.Name = "p24_chkWaveRecord_fade_rx";
            this.p24_chkWaveRecord_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.p24_chkWaveRecord_fade_rx.TabIndex = 1;
            this.p24_chkWaveRecord_fade_rx.Text = "Fade on RX";
            this.p24_chkWaveRecord_fade_rx.UseVisualStyleBackColor = true;
            this.p24_chkWaveRecord_fade_rx.CheckedChanged += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_chkWaveRecord_fade_tx
            // 
            this.p24_chkWaveRecord_fade_tx.AutoSize = true;
            this.p24_chkWaveRecord_fade_tx.Image = null;
            this.p24_chkWaveRecord_fade_tx.Location = new System.Drawing.Point(205, 39);
            this.p24_chkWaveRecord_fade_tx.Name = "p24_chkWaveRecord_fade_tx";
            this.p24_chkWaveRecord_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.p24_chkWaveRecord_fade_tx.TabIndex = 2;
            this.p24_chkWaveRecord_fade_tx.Text = "Fade on TX";
            this.p24_chkWaveRecord_fade_tx.UseVisualStyleBackColor = true;
            this.p24_chkWaveRecord_fade_tx.CheckedChanged += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_chkWebImage_background
            // 
            this.p24_chkWebImage_background.AutoSize = true;
            this.p24_chkWebImage_background.Image = null;
            this.p24_chkWebImage_background.Location = new System.Drawing.Point(14, 147);
            this.p24_chkWebImage_background.Name = "p24_chkWebImage_background";
            this.p24_chkWebImage_background.Size = new System.Drawing.Size(183, 17);
            this.p24_chkWebImage_background.TabIndex = 147;
            this.p24_chkWebImage_background.Text = "Use as spectral area background";
            this.p24_chkWebImage_background.UseVisualStyleBackColor = true;
            this.p24_chkWebImage_background.CheckedChanged += new System.EventHandler(this.chkWebImage_background_CheckedChanged);

            // 
            // p24_chkWebImage_bypass_cache
            // 
            this.p24_chkWebImage_bypass_cache.AutoSize = true;
            this.p24_chkWebImage_bypass_cache.Image = null;
            this.p24_chkWebImage_bypass_cache.Location = new System.Drawing.Point(221, 63);
            this.p24_chkWebImage_bypass_cache.Name = "p24_chkWebImage_bypass_cache";
            this.p24_chkWebImage_bypass_cache.Size = new System.Drawing.Size(94, 17);
            this.p24_chkWebImage_bypass_cache.TabIndex = 146;
            this.p24_chkWebImage_bypass_cache.Text = "Bypass Cache";
            this.toolTip1.SetToolTip(this.p24_chkWebImage_bypass_cache, "Append a unique id to the url each request, bypassing most servers caching polici" +
        "es");
            this.p24_chkWebImage_bypass_cache.UseVisualStyleBackColor = true;
            this.p24_chkWebImage_bypass_cache.CheckedChanged += new System.EventHandler(this.chkWebImage_bypass_cache_CheckedChanged);

            // 
            // p24_chkWebImage_fade_rx
            // 
            this.p24_chkWebImage_fade_rx.AutoSize = true;
            this.p24_chkWebImage_fade_rx.Image = null;
            this.p24_chkWebImage_fade_rx.Location = new System.Drawing.Point(221, 17);
            this.p24_chkWebImage_fade_rx.Name = "p24_chkWebImage_fade_rx";
            this.p24_chkWebImage_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.p24_chkWebImage_fade_rx.TabIndex = 2;
            this.p24_chkWebImage_fade_rx.Text = "Fade on RX";
            this.p24_chkWebImage_fade_rx.UseVisualStyleBackColor = true;
            this.p24_chkWebImage_fade_rx.CheckedChanged += new System.EventHandler(this.chkWebImage_fade_rx_CheckedChanged);

            // 
            // p24_chkWebImage_fade_tx
            // 
            this.p24_chkWebImage_fade_tx.AutoSize = true;
            this.p24_chkWebImage_fade_tx.Image = null;
            this.p24_chkWebImage_fade_tx.Location = new System.Drawing.Point(221, 40);
            this.p24_chkWebImage_fade_tx.Name = "p24_chkWebImage_fade_tx";
            this.p24_chkWebImage_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.p24_chkWebImage_fade_tx.TabIndex = 3;
            this.p24_chkWebImage_fade_tx.Text = "Fade on TX";
            this.p24_chkWebImage_fade_tx.UseVisualStyleBackColor = true;
            this.p24_chkWebImage_fade_tx.CheckedChanged += new System.EventHandler(this.chkWebImage_fade_tx_CheckedChanged);

            // 
            // p24_clrbtnBandButtons_border
            // 
            this.p24_clrbtnBandButtons_border.Automatic = "Automatic";
            this.p24_clrbtnBandButtons_border.Color = System.Drawing.Color.White;
            this.p24_clrbtnBandButtons_border.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnBandButtons_border.Image = null;
            this.p24_clrbtnBandButtons_border.Location = new System.Drawing.Point(60, 260);
            this.p24_clrbtnBandButtons_border.MoreColors = "More Colors...";
            this.p24_clrbtnBandButtons_border.Name = "p24_clrbtnBandButtons_border";
            this.p24_clrbtnBandButtons_border.TabStop = true;
            this.p24_clrbtnBandButtons_border.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnBandButtons_border.TabIndex = 151;
            this.toolTip1.SetToolTip(this.p24_clrbtnBandButtons_border, "Border colour");
            this.p24_clrbtnBandButtons_border.Changed += new System.EventHandler(this.clrbtnBandButtons_border_Changed);

            // 
            // p24_clrbtnBandButtons_fill
            // 
            this.p24_clrbtnBandButtons_fill.Automatic = "Automatic";
            this.p24_clrbtnBandButtons_fill.Color = System.Drawing.Color.Black;
            this.p24_clrbtnBandButtons_fill.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnBandButtons_fill.Image = null;
            this.p24_clrbtnBandButtons_fill.Location = new System.Drawing.Point(60, 289);
            this.p24_clrbtnBandButtons_fill.MoreColors = "More Colors...";
            this.p24_clrbtnBandButtons_fill.Name = "p24_clrbtnBandButtons_fill";
            this.p24_clrbtnBandButtons_fill.TabStop = true;
            this.p24_clrbtnBandButtons_fill.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnBandButtons_fill.TabIndex = 153;
            this.toolTip1.SetToolTip(this.p24_clrbtnBandButtons_fill, "Fill colour");
            this.p24_clrbtnBandButtons_fill.Changed += new System.EventHandler(this.clrbtnBandButtons_fill_Changed);

            // 
            // p24_clrbtnBandButtons_hover
            // 
            this.p24_clrbtnBandButtons_hover.Automatic = "Automatic";
            this.p24_clrbtnBandButtons_hover.Color = System.Drawing.Color.LightGray;
            this.p24_clrbtnBandButtons_hover.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnBandButtons_hover.Image = null;
            this.p24_clrbtnBandButtons_hover.Location = new System.Drawing.Point(60, 318);
            this.p24_clrbtnBandButtons_hover.MoreColors = "More Colors...";
            this.p24_clrbtnBandButtons_hover.Name = "p24_clrbtnBandButtons_hover";
            this.p24_clrbtnBandButtons_hover.TabStop = true;
            this.p24_clrbtnBandButtons_hover.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnBandButtons_hover.TabIndex = 155;
            this.toolTip1.SetToolTip(this.p24_clrbtnBandButtons_hover, "Hover colour");
            this.p24_clrbtnBandButtons_hover.Changed += new System.EventHandler(this.clrbtnBandButtons_hover_Changed);

            // 
            // p24_clrbtnBandButtons_indicator_off
            // 
            this.p24_clrbtnBandButtons_indicator_off.Automatic = "Automatic";
            this.p24_clrbtnBandButtons_indicator_off.Color = System.Drawing.Color.LightGray;
            this.p24_clrbtnBandButtons_indicator_off.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnBandButtons_indicator_off.Image = null;
            this.p24_clrbtnBandButtons_indicator_off.Location = new System.Drawing.Point(60, 231);
            this.p24_clrbtnBandButtons_indicator_off.MoreColors = "More Colors...";
            this.p24_clrbtnBandButtons_indicator_off.Name = "p24_clrbtnBandButtons_indicator_off";
            this.p24_clrbtnBandButtons_indicator_off.TabStop = true;
            this.p24_clrbtnBandButtons_indicator_off.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnBandButtons_indicator_off.TabIndex = 149;
            this.toolTip1.SetToolTip(this.p24_clrbtnBandButtons_indicator_off, "Inactive colour");
            this.p24_clrbtnBandButtons_indicator_off.Changed += new System.EventHandler(this.clrbtnBandButtons_indicator_off_Changed);

            // 
            // p24_clrbtnBandButtons_indicator_on
            // 
            this.p24_clrbtnBandButtons_indicator_on.Automatic = "Automatic";
            this.p24_clrbtnBandButtons_indicator_on.Color = System.Drawing.Color.CornflowerBlue;
            this.p24_clrbtnBandButtons_indicator_on.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnBandButtons_indicator_on.Image = null;
            this.p24_clrbtnBandButtons_indicator_on.Location = new System.Drawing.Point(60, 202);
            this.p24_clrbtnBandButtons_indicator_on.MoreColors = "More Colors...";
            this.p24_clrbtnBandButtons_indicator_on.Name = "p24_clrbtnBandButtons_indicator_on";
            this.p24_clrbtnBandButtons_indicator_on.TabStop = true;
            this.p24_clrbtnBandButtons_indicator_on.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnBandButtons_indicator_on.TabIndex = 129;
            this.toolTip1.SetToolTip(this.p24_clrbtnBandButtons_indicator_on, "Active colour");
            this.p24_clrbtnBandButtons_indicator_on.Changed += new System.EventHandler(this.clrbtnBandButtons_indicator_on_Changed);

            // 
            // p24_clrbtnButonBox_click
            // 
            this.p24_clrbtnButonBox_click.Automatic = "Automatic";
            this.p24_clrbtnButonBox_click.Color = System.Drawing.Color.White;
            this.p24_clrbtnButonBox_click.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnButonBox_click.Image = null;
            this.p24_clrbtnButonBox_click.Location = new System.Drawing.Point(60, 347);
            this.p24_clrbtnButonBox_click.MoreColors = "More Colors...";
            this.p24_clrbtnButonBox_click.Name = "p24_clrbtnButonBox_click";
            this.p24_clrbtnButonBox_click.TabStop = true;
            this.p24_clrbtnButonBox_click.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnButonBox_click.TabIndex = 166;
            this.toolTip1.SetToolTip(this.p24_clrbtnButonBox_click, "Click colour");
            this.p24_clrbtnButonBox_click.Changed += new System.EventHandler(this.clrbtnButonBox_click_Changed);

            // 
            // p24_clrbtnButonBox_fontcolour
            // 
            this.p24_clrbtnButonBox_fontcolour.Automatic = "Automatic";
            this.p24_clrbtnButonBox_fontcolour.Color = System.Drawing.Color.White;
            this.p24_clrbtnButonBox_fontcolour.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnButonBox_fontcolour.Image = null;
            this.p24_clrbtnButonBox_fontcolour.Location = new System.Drawing.Point(195, 60);
            this.p24_clrbtnButonBox_fontcolour.MoreColors = "More Colors...";
            this.p24_clrbtnButonBox_fontcolour.Name = "p24_clrbtnButonBox_fontcolour";
            this.p24_clrbtnButonBox_fontcolour.TabStop = true;
            this.p24_clrbtnButonBox_fontcolour.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnButonBox_fontcolour.TabIndex = 168;
            this.toolTip1.SetToolTip(this.p24_clrbtnButonBox_fontcolour, "Font colour");
            this.p24_clrbtnButonBox_fontcolour.Changed += new System.EventHandler(this.clrbtnButonBox_fontcolour_Changed);

            // 
            // p24_clrbtnContainerBackground
            // 
            this.p24_clrbtnContainerBackground.Automatic = "Automatic";
            this.p24_clrbtnContainerBackground.Color = System.Drawing.Color.Black;
            this.p24_clrbtnContainerBackground.Image = null;
            this.p24_clrbtnContainerBackground.Location = new System.Drawing.Point(159, 102);
            this.p24_clrbtnContainerBackground.MoreColors = "More Colors...";
            this.p24_clrbtnContainerBackground.Name = "p24_clrbtnContainerBackground";
            this.p24_clrbtnContainerBackground.TabStop = true;
            this.p24_clrbtnContainerBackground.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnContainerBackground.TabIndex = 98;
            this.toolTip1.SetToolTip(this.p24_clrbtnContainerBackground, "Container Background Colour");
            this.p24_clrbtnContainerBackground.Changed += new System.EventHandler(this.clrbtnContainerBackground_Changed);

            // 
            // p24_clrbtnDial_button_highlight
            // 
            this.p24_clrbtnDial_button_highlight.Automatic = "Automatic";
            this.p24_clrbtnDial_button_highlight.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_button_highlight.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_button_highlight.Image = null;
            this.p24_clrbtnDial_button_highlight.Location = new System.Drawing.Point(271, 314);
            this.p24_clrbtnDial_button_highlight.MoreColors = "More Colors...";
            this.p24_clrbtnDial_button_highlight.Name = "p24_clrbtnDial_button_highlight";
            this.p24_clrbtnDial_button_highlight.TabStop = true;
            this.p24_clrbtnDial_button_highlight.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_button_highlight.TabIndex = 195;
            this.p24_clrbtnDial_button_highlight.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_button_off
            // 
            this.p24_clrbtnDial_button_off.Automatic = "Automatic";
            this.p24_clrbtnDial_button_off.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_button_off.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_button_off.Image = null;
            this.p24_clrbtnDial_button_off.Location = new System.Drawing.Point(271, 339);
            this.p24_clrbtnDial_button_off.MoreColors = "More Colors...";
            this.p24_clrbtnDial_button_off.Name = "p24_clrbtnDial_button_off";
            this.p24_clrbtnDial_button_off.TabStop = true;
            this.p24_clrbtnDial_button_off.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_button_off.TabIndex = 185;
            this.p24_clrbtnDial_button_off.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_button_on
            // 
            this.p24_clrbtnDial_button_on.Automatic = "Automatic";
            this.p24_clrbtnDial_button_on.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_button_on.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_button_on.Image = null;
            this.p24_clrbtnDial_button_on.Location = new System.Drawing.Point(99, 339);
            this.p24_clrbtnDial_button_on.MoreColors = "More Colors...";
            this.p24_clrbtnDial_button_on.Name = "p24_clrbtnDial_button_on";
            this.p24_clrbtnDial_button_on.TabStop = true;
            this.p24_clrbtnDial_button_on.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_button_on.TabIndex = 183;
            this.p24_clrbtnDial_button_on.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_circle
            // 
            this.p24_clrbtnDial_circle.Automatic = "Automatic";
            this.p24_clrbtnDial_circle.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_circle.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_circle.Image = null;
            this.p24_clrbtnDial_circle.Location = new System.Drawing.Point(99, 264);
            this.p24_clrbtnDial_circle.MoreColors = "More Colors...";
            this.p24_clrbtnDial_circle.Name = "p24_clrbtnDial_circle";
            this.p24_clrbtnDial_circle.TabStop = true;
            this.p24_clrbtnDial_circle.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_circle.TabIndex = 177;
            this.p24_clrbtnDial_circle.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_fast
            // 
            this.p24_clrbtnDial_fast.Automatic = "Automatic";
            this.p24_clrbtnDial_fast.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_fast.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_fast.Image = null;
            this.p24_clrbtnDial_fast.Location = new System.Drawing.Point(271, 289);
            this.p24_clrbtnDial_fast.MoreColors = "More Colors...";
            this.p24_clrbtnDial_fast.Name = "p24_clrbtnDial_fast";
            this.p24_clrbtnDial_fast.TabStop = true;
            this.p24_clrbtnDial_fast.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_fast.TabIndex = 193;
            this.p24_clrbtnDial_fast.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_hold
            // 
            this.p24_clrbtnDial_hold.Automatic = "Automatic";
            this.p24_clrbtnDial_hold.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_hold.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_hold.Image = null;
            this.p24_clrbtnDial_hold.Location = new System.Drawing.Point(271, 264);
            this.p24_clrbtnDial_hold.MoreColors = "More Colors...";
            this.p24_clrbtnDial_hold.Name = "p24_clrbtnDial_hold";
            this.p24_clrbtnDial_hold.TabStop = true;
            this.p24_clrbtnDial_hold.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_hold.TabIndex = 191;
            this.p24_clrbtnDial_hold.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_pad
            // 
            this.p24_clrbtnDial_pad.Automatic = "Automatic";
            this.p24_clrbtnDial_pad.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_pad.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_pad.Image = null;
            this.p24_clrbtnDial_pad.Location = new System.Drawing.Point(99, 289);
            this.p24_clrbtnDial_pad.MoreColors = "More Colors...";
            this.p24_clrbtnDial_pad.Name = "p24_clrbtnDial_pad";
            this.p24_clrbtnDial_pad.TabStop = true;
            this.p24_clrbtnDial_pad.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_pad.TabIndex = 179;
            this.p24_clrbtnDial_pad.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_pad_pressed
            // 
            this.p24_clrbtnDial_pad_pressed.Automatic = "Automatic";
            this.p24_clrbtnDial_pad_pressed.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_pad_pressed.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_pad_pressed.Image = null;
            this.p24_clrbtnDial_pad_pressed.Location = new System.Drawing.Point(99, 314);
            this.p24_clrbtnDial_pad_pressed.MoreColors = "More Colors...";
            this.p24_clrbtnDial_pad_pressed.Name = "p24_clrbtnDial_pad_pressed";
            this.p24_clrbtnDial_pad_pressed.TabStop = true;
            this.p24_clrbtnDial_pad_pressed.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_pad_pressed.TabIndex = 181;
            this.p24_clrbtnDial_pad_pressed.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_ring
            // 
            this.p24_clrbtnDial_ring.Automatic = "Automatic";
            this.p24_clrbtnDial_ring.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_ring.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_ring.Image = null;
            this.p24_clrbtnDial_ring.Location = new System.Drawing.Point(271, 214);
            this.p24_clrbtnDial_ring.MoreColors = "More Colors...";
            this.p24_clrbtnDial_ring.Name = "p24_clrbtnDial_ring";
            this.p24_clrbtnDial_ring.TabStop = true;
            this.p24_clrbtnDial_ring.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_ring.TabIndex = 187;
            this.p24_clrbtnDial_ring.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_slow
            // 
            this.p24_clrbtnDial_slow.Automatic = "Automatic";
            this.p24_clrbtnDial_slow.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_slow.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_slow.Image = null;
            this.p24_clrbtnDial_slow.Location = new System.Drawing.Point(271, 239);
            this.p24_clrbtnDial_slow.MoreColors = "More Colors...";
            this.p24_clrbtnDial_slow.Name = "p24_clrbtnDial_slow";
            this.p24_clrbtnDial_slow.TabStop = true;
            this.p24_clrbtnDial_slow.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_slow.TabIndex = 189;
            this.p24_clrbtnDial_slow.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnDial_text
            // 
            this.p24_clrbtnDial_text.Automatic = "Automatic";
            this.p24_clrbtnDial_text.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnDial_text.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnDial_text.Image = null;
            this.p24_clrbtnDial_text.Location = new System.Drawing.Point(99, 239);
            this.p24_clrbtnDial_text.MoreColors = "More Colors...";
            this.p24_clrbtnDial_text.Name = "p24_clrbtnDial_text";
            this.p24_clrbtnDial_text.TabStop = true;
            this.p24_clrbtnDial_text.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnDial_text.TabIndex = 165;
            this.p24_clrbtnDial_text.Changed += new System.EventHandler(this.clrbtnDial_colours_changed);

            // 
            // p24_clrbtnFilterDisplay_backcolour
            // 
            this.p24_clrbtnFilterDisplay_backcolour.Automatic = "Automatic";
            this.p24_clrbtnFilterDisplay_backcolour.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnFilterDisplay_backcolour.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnFilterDisplay_backcolour.Image = null;
            this.p24_clrbtnFilterDisplay_backcolour.Location = new System.Drawing.Point(272, 13);
            this.p24_clrbtnFilterDisplay_backcolour.MoreColors = "More Colors...";
            this.p24_clrbtnFilterDisplay_backcolour.Name = "p24_clrbtnFilterDisplay_backcolour";
            this.p24_clrbtnFilterDisplay_backcolour.TabStop = true;
            this.p24_clrbtnFilterDisplay_backcolour.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilterDisplay_backcolour.TabIndex = 129;
            this.toolTip1.SetToolTip(this.p24_clrbtnFilterDisplay_backcolour, "Background colour");
            this.p24_clrbtnFilterDisplay_backcolour.Changed += new System.EventHandler(this.clrbtnFilterDisplay_backcolour_Changed);

            // 
            // p24_clrbtnFilter_button_highlight
            // 
            this.p24_clrbtnFilter_button_highlight.Automatic = "Automatic";
            this.p24_clrbtnFilter_button_highlight.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_button_highlight.Image = null;
            this.p24_clrbtnFilter_button_highlight.Location = new System.Drawing.Point(95, 183);
            this.p24_clrbtnFilter_button_highlight.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_button_highlight.Name = "p24_clrbtnFilter_button_highlight";
            this.p24_clrbtnFilter_button_highlight.TabStop = true;
            this.p24_clrbtnFilter_button_highlight.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_button_highlight.TabIndex = 200;
            this.p24_clrbtnFilter_button_highlight.Changed += new System.EventHandler(this.clrbtnFilter_button_highlight_Changed);

            // 
            // p24_clrbtnFilter_data_fill
            // 
            this.p24_clrbtnFilter_data_fill.Automatic = "Automatic";
            this.p24_clrbtnFilter_data_fill.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_data_fill.Image = null;
            this.p24_clrbtnFilter_data_fill.Location = new System.Drawing.Point(95, 94);
            this.p24_clrbtnFilter_data_fill.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_data_fill.Name = "p24_clrbtnFilter_data_fill";
            this.p24_clrbtnFilter_data_fill.TabStop = true;
            this.p24_clrbtnFilter_data_fill.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_data_fill.TabIndex = 172;
            this.p24_clrbtnFilter_data_fill.Changed += new System.EventHandler(this.clrbtnFilter_data_fill_Changed);

            // 
            // p24_clrbtnFilter_data_line
            // 
            this.p24_clrbtnFilter_data_line.Automatic = "Automatic";
            this.p24_clrbtnFilter_data_line.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_data_line.Image = null;
            this.p24_clrbtnFilter_data_line.Location = new System.Drawing.Point(95, 72);
            this.p24_clrbtnFilter_data_line.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_data_line.Name = "p24_clrbtnFilter_data_line";
            this.p24_clrbtnFilter_data_line.TabStop = true;
            this.p24_clrbtnFilter_data_line.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_data_line.TabIndex = 170;
            this.p24_clrbtnFilter_data_line.Changed += new System.EventHandler(this.clrbtnFilter_data_line_Changed);

            // 
            // p24_clrbtnFilter_edge_highlight
            // 
            this.p24_clrbtnFilter_edge_highlight.Automatic = "Automatic";
            this.p24_clrbtnFilter_edge_highlight.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_edge_highlight.Image = null;
            this.p24_clrbtnFilter_edge_highlight.Location = new System.Drawing.Point(249, 143);
            this.p24_clrbtnFilter_edge_highlight.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_edge_highlight.Name = "p24_clrbtnFilter_edge_highlight";
            this.p24_clrbtnFilter_edge_highlight.TabStop = true;
            this.p24_clrbtnFilter_edge_highlight.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_edge_highlight.TabIndex = 180;
            this.p24_clrbtnFilter_edge_highlight.Changed += new System.EventHandler(this.clrbtnFilter_edge_highlight_Changed);

            // 
            // p24_clrbtnFilter_edges
            // 
            this.p24_clrbtnFilter_edges.Automatic = "Automatic";
            this.p24_clrbtnFilter_edges.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_edges.Image = null;
            this.p24_clrbtnFilter_edges.Location = new System.Drawing.Point(249, 95);
            this.p24_clrbtnFilter_edges.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_edges.Name = "p24_clrbtnFilter_edges";
            this.p24_clrbtnFilter_edges.TabStop = true;
            this.p24_clrbtnFilter_edges.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_edges.TabIndex = 178;
            this.p24_clrbtnFilter_edges.Changed += new System.EventHandler(this.clrbtnFilter_edges_Changed);

            // 
            // p24_clrbtnFilter_edges_tx
            // 
            this.p24_clrbtnFilter_edges_tx.Automatic = "Automatic";
            this.p24_clrbtnFilter_edges_tx.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_edges_tx.Image = null;
            this.p24_clrbtnFilter_edges_tx.Location = new System.Drawing.Point(249, 119);
            this.p24_clrbtnFilter_edges_tx.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_edges_tx.Name = "p24_clrbtnFilter_edges_tx";
            this.p24_clrbtnFilter_edges_tx.TabStop = true;
            this.p24_clrbtnFilter_edges_tx.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_edges_tx.TabIndex = 202;
            this.p24_clrbtnFilter_edges_tx.Changed += new System.EventHandler(this.clrbtnFilter_edges_tx_Changed);

            // 
            // p24_clrbtnFilter_extents
            // 
            this.p24_clrbtnFilter_extents.Automatic = "Automatic";
            this.p24_clrbtnFilter_extents.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_extents.Image = null;
            this.p24_clrbtnFilter_extents.Location = new System.Drawing.Point(95, 138);
            this.p24_clrbtnFilter_extents.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_extents.Name = "p24_clrbtnFilter_extents";
            this.p24_clrbtnFilter_extents.TabStop = true;
            this.p24_clrbtnFilter_extents.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_extents.TabIndex = 188;
            this.p24_clrbtnFilter_extents.Changed += new System.EventHandler(this.clrbtnFilter_extents_Changed);

            // 
            // p24_clrbtnFilter_meter_back
            // 
            this.p24_clrbtnFilter_meter_back.Automatic = "Automatic";
            this.p24_clrbtnFilter_meter_back.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_meter_back.Image = null;
            this.p24_clrbtnFilter_meter_back.Location = new System.Drawing.Point(95, 116);
            this.p24_clrbtnFilter_meter_back.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_meter_back.Name = "p24_clrbtnFilter_meter_back";
            this.p24_clrbtnFilter_meter_back.TabStop = true;
            this.p24_clrbtnFilter_meter_back.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_meter_back.TabIndex = 182;
            this.p24_clrbtnFilter_meter_back.Changed += new System.EventHandler(this.clrbtnFilter_meter_back_Changed);

            // 
            // p24_clrbtnFilter_notch
            // 
            this.p24_clrbtnFilter_notch.Automatic = "Automatic";
            this.p24_clrbtnFilter_notch.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_notch.Image = null;
            this.p24_clrbtnFilter_notch.Location = new System.Drawing.Point(249, 165);
            this.p24_clrbtnFilter_notch.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_notch.Name = "p24_clrbtnFilter_notch";
            this.p24_clrbtnFilter_notch.TabStop = true;
            this.p24_clrbtnFilter_notch.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_notch.TabIndex = 184;
            this.p24_clrbtnFilter_notch.Changed += new System.EventHandler(this.clrbtnFilter_notch_Changed);

            // 
            // p24_clrbtnFilter_notch_highlight
            // 
            this.p24_clrbtnFilter_notch_highlight.Automatic = "Automatic";
            this.p24_clrbtnFilter_notch_highlight.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_notch_highlight.Image = null;
            this.p24_clrbtnFilter_notch_highlight.Location = new System.Drawing.Point(249, 187);
            this.p24_clrbtnFilter_notch_highlight.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_notch_highlight.Name = "p24_clrbtnFilter_notch_highlight";
            this.p24_clrbtnFilter_notch_highlight.TabStop = true;
            this.p24_clrbtnFilter_notch_highlight.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_notch_highlight.TabIndex = 186;
            this.p24_clrbtnFilter_notch_highlight.Changed += new System.EventHandler(this.clrbtnFilter_notch_highlight_Changed);

            // 
            // p24_clrbtnFilter_number_highlight
            // 
            this.p24_clrbtnFilter_number_highlight.Automatic = "Automatic";
            this.p24_clrbtnFilter_number_highlight.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_number_highlight.Image = null;
            this.p24_clrbtnFilter_number_highlight.Location = new System.Drawing.Point(249, 72);
            this.p24_clrbtnFilter_number_highlight.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_number_highlight.Name = "p24_clrbtnFilter_number_highlight";
            this.p24_clrbtnFilter_number_highlight.TabStop = true;
            this.p24_clrbtnFilter_number_highlight.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_number_highlight.TabIndex = 176;
            this.p24_clrbtnFilter_number_highlight.Changed += new System.EventHandler(this.clrbtnFilter_number_highlight_Changed);

            // 
            // p24_clrbtnFilter_setting_on
            // 
            this.p24_clrbtnFilter_setting_on.Automatic = "Automatic";
            this.p24_clrbtnFilter_setting_on.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_setting_on.Image = null;
            this.p24_clrbtnFilter_setting_on.Location = new System.Drawing.Point(95, 160);
            this.p24_clrbtnFilter_setting_on.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_setting_on.Name = "p24_clrbtnFilter_setting_on";
            this.p24_clrbtnFilter_setting_on.TabStop = true;
            this.p24_clrbtnFilter_setting_on.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_setting_on.TabIndex = 198;
            this.p24_clrbtnFilter_setting_on.Changed += new System.EventHandler(this.clrbtnFilter_setting_on_Changed);

            // 
            // p24_clrbtnFilter_snap_line
            // 
            this.p24_clrbtnFilter_snap_line.Automatic = "Automatic";
            this.p24_clrbtnFilter_snap_line.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_snap_line.Image = null;
            this.p24_clrbtnFilter_snap_line.Location = new System.Drawing.Point(249, 209);
            this.p24_clrbtnFilter_snap_line.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_snap_line.Name = "p24_clrbtnFilter_snap_line";
            this.p24_clrbtnFilter_snap_line.TabStop = true;
            this.p24_clrbtnFilter_snap_line.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_snap_line.TabIndex = 196;
            this.p24_clrbtnFilter_snap_line.Changed += new System.EventHandler(this.clrbtnFilter_snap_line_Changed);

            // 
            // p24_clrbtnFilter_text
            // 
            this.p24_clrbtnFilter_text.Automatic = "Automatic";
            this.p24_clrbtnFilter_text.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_text.Image = null;
            this.p24_clrbtnFilter_text.Location = new System.Drawing.Point(249, 50);
            this.p24_clrbtnFilter_text.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_text.Name = "p24_clrbtnFilter_text";
            this.p24_clrbtnFilter_text.TabStop = true;
            this.p24_clrbtnFilter_text.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_text.TabIndex = 174;
            this.p24_clrbtnFilter_text.Changed += new System.EventHandler(this.clrbtnFilter_text_Changed);

            // 
            // p24_clrbtnFilter_wf_low
            // 
            this.p24_clrbtnFilter_wf_low.Automatic = "Automatic";
            this.p24_clrbtnFilter_wf_low.Color = System.Drawing.Color.Transparent;
            this.p24_clrbtnFilter_wf_low.Image = null;
            this.p24_clrbtnFilter_wf_low.Location = new System.Drawing.Point(249, 27);
            this.p24_clrbtnFilter_wf_low.MoreColors = "More Colors...";
            this.p24_clrbtnFilter_wf_low.Name = "p24_clrbtnFilter_wf_low";
            this.p24_clrbtnFilter_wf_low.TabStop = true;
            this.p24_clrbtnFilter_wf_low.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnFilter_wf_low.TabIndex = 168;
            this.p24_clrbtnFilter_wf_low.Changed += new System.EventHandler(this.clrbtnFilter_wf_low_Changed);

            // 
            // p24_clrbtnHistory_background
            // 
            this.p24_clrbtnHistory_background.Automatic = "Automatic";
            this.p24_clrbtnHistory_background.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnHistory_background.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnHistory_background.Image = null;
            this.p24_clrbtnHistory_background.Location = new System.Drawing.Point(272, 23);
            this.p24_clrbtnHistory_background.MoreColors = "More Colors...";
            this.p24_clrbtnHistory_background.Name = "p24_clrbtnHistory_background";
            this.p24_clrbtnHistory_background.TabStop = true;
            this.p24_clrbtnHistory_background.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnHistory_background.TabIndex = 129;
            this.toolTip1.SetToolTip(this.p24_clrbtnHistory_background, "Background colour");
            this.p24_clrbtnHistory_background.Changed += new System.EventHandler(this.clrbtnHistory_background_Changed);

            // 
            // p24_clrbtnHistory_colour_0
            // 
            this.p24_clrbtnHistory_colour_0.Automatic = "Automatic";
            this.p24_clrbtnHistory_colour_0.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnHistory_colour_0.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnHistory_colour_0.Image = null;
            this.p24_clrbtnHistory_colour_0.Location = new System.Drawing.Point(215, 21);
            this.p24_clrbtnHistory_colour_0.MoreColors = "More Colors...";
            this.p24_clrbtnHistory_colour_0.Name = "p24_clrbtnHistory_colour_0";
            this.p24_clrbtnHistory_colour_0.TabStop = true;
            this.p24_clrbtnHistory_colour_0.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnHistory_colour_0.TabIndex = 130;
            this.toolTip1.SetToolTip(this.p24_clrbtnHistory_colour_0, "Reading colour");
            this.p24_clrbtnHistory_colour_0.Changed += new System.EventHandler(this.clrbtnHistory_colour_0_Changed);

            // 
            // p24_clrbtnHistory_colour_1
            // 
            this.p24_clrbtnHistory_colour_1.Automatic = "Automatic";
            this.p24_clrbtnHistory_colour_1.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnHistory_colour_1.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnHistory_colour_1.Image = null;
            this.p24_clrbtnHistory_colour_1.Location = new System.Drawing.Point(215, 21);
            this.p24_clrbtnHistory_colour_1.MoreColors = "More Colors...";
            this.p24_clrbtnHistory_colour_1.Name = "p24_clrbtnHistory_colour_1";
            this.p24_clrbtnHistory_colour_1.TabStop = true;
            this.p24_clrbtnHistory_colour_1.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnHistory_colour_1.TabIndex = 147;
            this.toolTip1.SetToolTip(this.p24_clrbtnHistory_colour_1, "Reading colour");
            this.p24_clrbtnHistory_colour_1.Changed += new System.EventHandler(this.clrbtnHistory_colour_1_Changed);

            // 
            // p24_clrbtnHistory_lines
            // 
            this.p24_clrbtnHistory_lines.Automatic = "Automatic";
            this.p24_clrbtnHistory_lines.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnHistory_lines.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnHistory_lines.Image = null;
            this.p24_clrbtnHistory_lines.Location = new System.Drawing.Point(72, 110);
            this.p24_clrbtnHistory_lines.MoreColors = "More Colors...";
            this.p24_clrbtnHistory_lines.Name = "p24_clrbtnHistory_lines";
            this.p24_clrbtnHistory_lines.TabStop = true;
            this.p24_clrbtnHistory_lines.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnHistory_lines.TabIndex = 145;
            this.toolTip1.SetToolTip(this.p24_clrbtnHistory_lines, "Lines colour");
            this.p24_clrbtnHistory_lines.Changed += new System.EventHandler(this.clrbtnHistory_lines_Changed);

            // 
            // p24_clrbtnHistory_time
            // 
            this.p24_clrbtnHistory_time.Automatic = "Automatic";
            this.p24_clrbtnHistory_time.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnHistory_time.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnHistory_time.Image = null;
            this.p24_clrbtnHistory_time.Location = new System.Drawing.Point(170, 110);
            this.p24_clrbtnHistory_time.MoreColors = "More Colors...";
            this.p24_clrbtnHistory_time.Name = "p24_clrbtnHistory_time";
            this.p24_clrbtnHistory_time.TabStop = true;
            this.p24_clrbtnHistory_time.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnHistory_time.TabIndex = 146;
            this.toolTip1.SetToolTip(this.p24_clrbtnHistory_time, "Time colour");
            this.p24_clrbtnHistory_time.Changed += new System.EventHandler(this.clrbtnHistory_time_Changed);

            // 
            // p24_clrbtnLedIndicator_PanelBackground
            // 
            this.p24_clrbtnLedIndicator_PanelBackground.Automatic = "Automatic";
            this.p24_clrbtnLedIndicator_PanelBackground.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnLedIndicator_PanelBackground.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnLedIndicator_PanelBackground.Image = null;
            this.p24_clrbtnLedIndicator_PanelBackground.Location = new System.Drawing.Point(125, 47);
            this.p24_clrbtnLedIndicator_PanelBackground.MoreColors = "More Colors...";
            this.p24_clrbtnLedIndicator_PanelBackground.Name = "p24_clrbtnLedIndicator_PanelBackground";
            this.p24_clrbtnLedIndicator_PanelBackground.TabStop = true;
            this.p24_clrbtnLedIndicator_PanelBackground.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnLedIndicator_PanelBackground.TabIndex = 129;
            this.toolTip1.SetToolTip(this.p24_clrbtnLedIndicator_PanelBackground, "Background colour");
            this.p24_clrbtnLedIndicator_PanelBackground.Changed += new System.EventHandler(this.clrbtnLedIndicator_PanelBackground_Changed);

            // 
            // p24_clrbtnLedIndicator_PanelBackgroundTX
            // 
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.Automatic = "Automatic";
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.Image = null;
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.Location = new System.Drawing.Point(125, 72);
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.MoreColors = "More Colors...";
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.Name = "p24_clrbtnLedIndicator_PanelBackgroundTX";
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.TabStop = true;
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.TabIndex = 162;
            this.toolTip1.SetToolTip(this.p24_clrbtnLedIndicator_PanelBackgroundTX, "Background colour");
            this.p24_clrbtnLedIndicator_PanelBackgroundTX.Changed += new System.EventHandler(this.clrbtnLedIndicator_PanelBackgroundTX_Changed);

            // 
            // p24_clrbtnLedIndicator_false
            // 
            this.p24_clrbtnLedIndicator_false.Automatic = "Automatic";
            this.p24_clrbtnLedIndicator_false.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnLedIndicator_false.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnLedIndicator_false.Image = null;
            this.p24_clrbtnLedIndicator_false.Location = new System.Drawing.Point(65, 188);
            this.p24_clrbtnLedIndicator_false.MoreColors = "More Colors...";
            this.p24_clrbtnLedIndicator_false.Name = "p24_clrbtnLedIndicator_false";
            this.p24_clrbtnLedIndicator_false.TabStop = true;
            this.p24_clrbtnLedIndicator_false.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnLedIndicator_false.TabIndex = 157;
            this.toolTip1.SetToolTip(this.p24_clrbtnLedIndicator_false, "False colour");
            this.p24_clrbtnLedIndicator_false.Changed += new System.EventHandler(this.clrbtnLedIndicator_false_Changed);

            // 
            // p24_clrbtnLedIndicator_true
            // 
            this.p24_clrbtnLedIndicator_true.Automatic = "Automatic";
            this.p24_clrbtnLedIndicator_true.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnLedIndicator_true.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnLedIndicator_true.Image = null;
            this.p24_clrbtnLedIndicator_true.Location = new System.Drawing.Point(65, 162);
            this.p24_clrbtnLedIndicator_true.MoreColors = "More Colors...";
            this.p24_clrbtnLedIndicator_true.Name = "p24_clrbtnLedIndicator_true";
            this.p24_clrbtnLedIndicator_true.TabStop = true;
            this.p24_clrbtnLedIndicator_true.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnLedIndicator_true.TabIndex = 156;
            this.toolTip1.SetToolTip(this.p24_clrbtnLedIndicator_true, "True colour");
            this.p24_clrbtnLedIndicator_true.Changed += new System.EventHandler(this.clrbtnLedIndicator_true_Changed);

            // 
            // p24_clrbtnMMClockBackground
            // 
            this.p24_clrbtnMMClockBackground.Automatic = "Automatic";
            this.p24_clrbtnMMClockBackground.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnMMClockBackground.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMClockBackground.Image = null;
            this.p24_clrbtnMMClockBackground.Location = new System.Drawing.Point(106, 53);
            this.p24_clrbtnMMClockBackground.MoreColors = "More Colors...";
            this.p24_clrbtnMMClockBackground.Name = "p24_clrbtnMMClockBackground";
            this.p24_clrbtnMMClockBackground.TabStop = true;
            this.p24_clrbtnMMClockBackground.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMClockBackground.TabIndex = 115;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMClockBackground, "Background colour");
            this.p24_clrbtnMMClockBackground.Changed += new System.EventHandler(this.clrbtnMMClockBackground_Changed);

            // 
            // p24_clrbtnMMClockTitle
            // 
            this.p24_clrbtnMMClockTitle.Automatic = "Automatic";
            this.p24_clrbtnMMClockTitle.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMClockTitle.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMClockTitle.Image = null;
            this.p24_clrbtnMMClockTitle.Location = new System.Drawing.Point(105, 86);
            this.p24_clrbtnMMClockTitle.MoreColors = "More Colors...";
            this.p24_clrbtnMMClockTitle.Name = "p24_clrbtnMMClockTitle";
            this.p24_clrbtnMMClockTitle.TabStop = true;
            this.p24_clrbtnMMClockTitle.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMClockTitle.TabIndex = 110;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMClockTitle, "Meter title colour");
            this.p24_clrbtnMMClockTitle.Changed += new System.EventHandler(this.clrbtnMMClockTitle_Changed);

            // 
            // p24_clrbtnMMDate
            // 
            this.p24_clrbtnMMDate.Automatic = "Automatic";
            this.p24_clrbtnMMDate.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMDate.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMDate.Image = null;
            this.p24_clrbtnMMDate.Location = new System.Drawing.Point(105, 142);
            this.p24_clrbtnMMDate.MoreColors = "More Colors...";
            this.p24_clrbtnMMDate.Name = "p24_clrbtnMMDate";
            this.p24_clrbtnMMDate.TabStop = true;
            this.p24_clrbtnMMDate.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMDate.TabIndex = 113;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMDate, "Date colour");
            this.p24_clrbtnMMDate.Changed += new System.EventHandler(this.clrbtnMMDate_Changed);

            // 
            // p24_clrbtnMMTime
            // 
            this.p24_clrbtnMMTime.Automatic = "Automatic";
            this.p24_clrbtnMMTime.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMTime.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMTime.Image = null;
            this.p24_clrbtnMMTime.Location = new System.Drawing.Point(105, 118);
            this.p24_clrbtnMMTime.MoreColors = "More Colors...";
            this.p24_clrbtnMMTime.Name = "p24_clrbtnMMTime";
            this.p24_clrbtnMMTime.TabStop = true;
            this.p24_clrbtnMMTime.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMTime.TabIndex = 111;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMTime, "Time");
            this.p24_clrbtnMMTime.Changed += new System.EventHandler(this.clrbtnMMTime_Changed);

            // 
            // p24_clrbtnMMVfoDigitHighlight
            // 
            this.p24_clrbtnMMVfoDigitHighlight.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDigitHighlight.Color = System.Drawing.SystemColors.ControlLight;
            this.p24_clrbtnMMVfoDigitHighlight.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDigitHighlight.Image = null;
            this.p24_clrbtnMMVfoDigitHighlight.Location = new System.Drawing.Point(105, 288);
            this.p24_clrbtnMMVfoDigitHighlight.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDigitHighlight.Name = "p24_clrbtnMMVfoDigitHighlight";
            this.p24_clrbtnMMVfoDigitHighlight.TabStop = true;
            this.p24_clrbtnMMVfoDigitHighlight.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDigitHighlight.TabIndex = 130;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDigitHighlight, "Any highlights from the mouse will be in this colour");
            this.p24_clrbtnMMVfoDigitHighlight.Changed += new System.EventHandler(this.clrbtnMMVfoDigitHighlight_Changed);

            // 
            // p24_clrbtnMMVfoDisplayBackground
            // 
            this.p24_clrbtnMMVfoDisplayBackground.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayBackground.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnMMVfoDisplayBackground.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayBackground.Image = null;
            this.p24_clrbtnMMVfoDisplayBackground.Location = new System.Drawing.Point(105, 30);
            this.p24_clrbtnMMVfoDisplayBackground.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayBackground.Name = "p24_clrbtnMMVfoDisplayBackground";
            this.p24_clrbtnMMVfoDisplayBackground.TabStop = true;
            this.p24_clrbtnMMVfoDisplayBackground.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayBackground.TabIndex = 127;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayBackground, "Background colour");
            this.p24_clrbtnMMVfoDisplayBackground.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayBackground_Changed);

            // 
            // p24_clrbtnMMVfoDisplayBand
            // 
            this.p24_clrbtnMMVfoDisplayBand.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayBand.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayBand.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayBand.Image = null;
            this.p24_clrbtnMMVfoDisplayBand.Location = new System.Drawing.Point(105, 259);
            this.p24_clrbtnMMVfoDisplayBand.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayBand.Name = "p24_clrbtnMMVfoDisplayBand";
            this.p24_clrbtnMMVfoDisplayBand.TabStop = true;
            this.p24_clrbtnMMVfoDisplayBand.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayBand.TabIndex = 123;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayBand, "Band colour");
            this.p24_clrbtnMMVfoDisplayBand.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayBand_Changed);

            // 
            // p24_clrbtnMMVfoDisplayFilter
            // 
            this.p24_clrbtnMMVfoDisplayFilter.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayFilter.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayFilter.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayFilter.Image = null;
            this.p24_clrbtnMMVfoDisplayFilter.Location = new System.Drawing.Point(105, 230);
            this.p24_clrbtnMMVfoDisplayFilter.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayFilter.Name = "p24_clrbtnMMVfoDisplayFilter";
            this.p24_clrbtnMMVfoDisplayFilter.TabStop = true;
            this.p24_clrbtnMMVfoDisplayFilter.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayFilter.TabIndex = 121;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayFilter, "Filter colour");
            this.p24_clrbtnMMVfoDisplayFilter.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayFilter_Changed);

            // 
            // p24_clrbtnMMVfoDisplayFrequency
            // 
            this.p24_clrbtnMMVfoDisplayFrequency.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayFrequency.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayFrequency.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayFrequency.Image = null;
            this.p24_clrbtnMMVfoDisplayFrequency.Location = new System.Drawing.Point(105, 88);
            this.p24_clrbtnMMVfoDisplayFrequency.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayFrequency.Name = "p24_clrbtnMMVfoDisplayFrequency";
            this.p24_clrbtnMMVfoDisplayFrequency.TabStop = true;
            this.p24_clrbtnMMVfoDisplayFrequency.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayFrequency.TabIndex = 125;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayFrequency, "Frequency Colour");
            this.p24_clrbtnMMVfoDisplayFrequency.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayFrequency_Changed);

            // 
            // p24_clrbtnMMVfoDisplayFrequency_small
            // 
            this.p24_clrbtnMMVfoDisplayFrequency_small.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayFrequency_small.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayFrequency_small.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayFrequency_small.Image = null;
            this.p24_clrbtnMMVfoDisplayFrequency_small.Location = new System.Drawing.Point(228, 88);
            this.p24_clrbtnMMVfoDisplayFrequency_small.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayFrequency_small.Name = "p24_clrbtnMMVfoDisplayFrequency_small";
            this.p24_clrbtnMMVfoDisplayFrequency_small.TabStop = true;
            this.p24_clrbtnMMVfoDisplayFrequency_small.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayFrequency_small.TabIndex = 137;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayFrequency_small, "Frequency Colour");
            this.p24_clrbtnMMVfoDisplayFrequency_small.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayFrequency_small_Changed);

            // 
            // p24_clrbtnMMVfoDisplayMode
            // 
            this.p24_clrbtnMMVfoDisplayMode.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayMode.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayMode.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayMode.Image = null;
            this.p24_clrbtnMMVfoDisplayMode.Location = new System.Drawing.Point(105, 118);
            this.p24_clrbtnMMVfoDisplayMode.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayMode.Name = "p24_clrbtnMMVfoDisplayMode";
            this.p24_clrbtnMMVfoDisplayMode.TabStop = true;
            this.p24_clrbtnMMVfoDisplayMode.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayMode.TabIndex = 111;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayMode, "Mode colour");
            this.p24_clrbtnMMVfoDisplayMode.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayMode_Changed);

            // 
            // p24_clrbtnMMVfoDisplayRx
            // 
            this.p24_clrbtnMMVfoDisplayRx.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayRx.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayRx.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayRx.Image = null;
            this.p24_clrbtnMMVfoDisplayRx.Location = new System.Drawing.Point(105, 171);
            this.p24_clrbtnMMVfoDisplayRx.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayRx.Name = "p24_clrbtnMMVfoDisplayRx";
            this.p24_clrbtnMMVfoDisplayRx.TabStop = true;
            this.p24_clrbtnMMVfoDisplayRx.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayRx.TabIndex = 117;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayRx, "RX box colour");
            this.p24_clrbtnMMVfoDisplayRx.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayRx_Changed);

            // 
            // p24_clrbtnMMVfoDisplaySplit
            // 
            this.p24_clrbtnMMVfoDisplaySplit.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplaySplit.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplaySplit.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplaySplit.Image = null;
            this.p24_clrbtnMMVfoDisplaySplit.Location = new System.Drawing.Point(205, 142);
            this.p24_clrbtnMMVfoDisplaySplit.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplaySplit.Name = "p24_clrbtnMMVfoDisplaySplit";
            this.p24_clrbtnMMVfoDisplaySplit.TabStop = true;
            this.p24_clrbtnMMVfoDisplaySplit.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplaySplit.TabIndex = 115;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplaySplit, "Split colour");
            this.p24_clrbtnMMVfoDisplaySplit.Changed += new System.EventHandler(this.clrbtnMMVfoDisplaySplit_Changed);

            // 
            // p24_clrbtnMMVfoDisplaySplitBack
            // 
            this.p24_clrbtnMMVfoDisplaySplitBack.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplaySplitBack.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplaySplitBack.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplaySplitBack.Image = null;
            this.p24_clrbtnMMVfoDisplaySplitBack.Location = new System.Drawing.Point(105, 142);
            this.p24_clrbtnMMVfoDisplaySplitBack.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplaySplitBack.Name = "p24_clrbtnMMVfoDisplaySplitBack";
            this.p24_clrbtnMMVfoDisplaySplitBack.TabStop = true;
            this.p24_clrbtnMMVfoDisplaySplitBack.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplaySplitBack.TabIndex = 113;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplaySplitBack, "Background of the split");
            this.p24_clrbtnMMVfoDisplaySplitBack.Changed += new System.EventHandler(this.clrbtnMMVfoDisplaySplitBack_Changed);

            // 
            // p24_clrbtnMMVfoDisplayTitle
            // 
            this.p24_clrbtnMMVfoDisplayTitle.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayTitle.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayTitle.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayTitle.Image = null;
            this.p24_clrbtnMMVfoDisplayTitle.Location = new System.Drawing.Point(105, 59);
            this.p24_clrbtnMMVfoDisplayTitle.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayTitle.Name = "p24_clrbtnMMVfoDisplayTitle";
            this.p24_clrbtnMMVfoDisplayTitle.TabStop = true;
            this.p24_clrbtnMMVfoDisplayTitle.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayTitle.TabIndex = 110;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayTitle, "Titles colour");
            this.p24_clrbtnMMVfoDisplayTitle.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayTitle_Changed);

            // 
            // p24_clrbtnMMVfoDisplayTx
            // 
            this.p24_clrbtnMMVfoDisplayTx.Automatic = "Automatic";
            this.p24_clrbtnMMVfoDisplayTx.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMMVfoDisplayTx.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMMVfoDisplayTx.Image = null;
            this.p24_clrbtnMMVfoDisplayTx.Location = new System.Drawing.Point(105, 200);
            this.p24_clrbtnMMVfoDisplayTx.MoreColors = "More Colors...";
            this.p24_clrbtnMMVfoDisplayTx.Name = "p24_clrbtnMMVfoDisplayTx";
            this.p24_clrbtnMMVfoDisplayTx.TabStop = true;
            this.p24_clrbtnMMVfoDisplayTx.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMMVfoDisplayTx.TabIndex = 119;
            this.toolTip1.SetToolTip(this.p24_clrbtnMMVfoDisplayTx, "TX box colour");
            this.p24_clrbtnMMVfoDisplayTx.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayTx_Changed);

            // 
            // p24_clrbtnMeterItemHBackground
            // 
            this.p24_clrbtnMeterItemHBackground.Automatic = "Automatic";
            this.p24_clrbtnMeterItemHBackground.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnMeterItemHBackground.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemHBackground.Image = null;
            this.p24_clrbtnMeterItemHBackground.Location = new System.Drawing.Point(263, 17);
            this.p24_clrbtnMeterItemHBackground.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemHBackground.Name = "p24_clrbtnMeterItemHBackground";
            this.p24_clrbtnMeterItemHBackground.TabStop = true;
            this.p24_clrbtnMeterItemHBackground.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemHBackground.TabIndex = 92;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemHBackground, "Background colour");
            this.p24_clrbtnMeterItemHBackground.Changed += new System.EventHandler(this.clrbtnMeterItemHBackground_Changed);

            // 
            // p24_clrbtnMeterItemHBackgroundRotator
            // 
            this.p24_clrbtnMeterItemHBackgroundRotator.Automatic = "Automatic";
            this.p24_clrbtnMeterItemHBackgroundRotator.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnMeterItemHBackgroundRotator.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemHBackgroundRotator.Image = null;
            this.p24_clrbtnMeterItemHBackgroundRotator.Location = new System.Drawing.Point(263, 17);
            this.p24_clrbtnMeterItemHBackgroundRotator.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemHBackgroundRotator.Name = "p24_clrbtnMeterItemHBackgroundRotator";
            this.p24_clrbtnMeterItemHBackgroundRotator.TabStop = true;
            this.p24_clrbtnMeterItemHBackgroundRotator.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemHBackgroundRotator.TabIndex = 92;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemHBackgroundRotator, "Background colour");
            this.p24_clrbtnMeterItemHBackgroundRotator.Changed += new System.EventHandler(this.clrbtnMeterItemHBackgroundRotator_Changed);

            // 
            // p24_clrbtnMeterItemHBackgroundSpacerRX
            // 
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.Automatic = "Automatic";
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.Image = null;
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.Location = new System.Drawing.Point(103, 29);
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.Name = "p24_clrbtnMeterItemHBackgroundSpacerRX";
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.TabStop = true;
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.TabIndex = 129;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemHBackgroundSpacerRX, "Background colour");
            this.p24_clrbtnMeterItemHBackgroundSpacerRX.Changed += new System.EventHandler(this.clrbtnMeterItemHBackgroundSpacerRX_Changed);

            // 
            // p24_clrbtnMeterItemHBackgroundSpacerTX
            // 
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.Automatic = "Automatic";
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.Image = null;
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.Location = new System.Drawing.Point(103, 58);
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.Name = "p24_clrbtnMeterItemHBackgroundSpacerTX";
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.TabStop = true;
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.TabIndex = 133;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemHBackgroundSpacerTX, "Background colour");
            this.p24_clrbtnMeterItemHBackgroundSpacerTX.Changed += new System.EventHandler(this.clrbtnMeterItemHBackgroundSpacerTX_Changed);

            // 
            // p24_clrbtnMeterItemHigh
            // 
            this.p24_clrbtnMeterItemHigh.Automatic = "Automatic";
            this.p24_clrbtnMeterItemHigh.Color = System.Drawing.Color.Red;
            this.p24_clrbtnMeterItemHigh.Image = null;
            this.p24_clrbtnMeterItemHigh.Location = new System.Drawing.Point(148, 4);
            this.p24_clrbtnMeterItemHigh.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemHigh.Name = "p24_clrbtnMeterItemHigh";
            this.p24_clrbtnMeterItemHigh.TabStop = true;
            this.p24_clrbtnMeterItemHigh.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemHigh.TabIndex = 77;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemHigh, "High scale colour");
            this.p24_clrbtnMeterItemHigh.Changed += new System.EventHandler(this.clrbtnMeterItemHigh_Changed);

            // 
            // p24_clrbtnMeterItemHistory
            // 
            this.p24_clrbtnMeterItemHistory.Automatic = "Automatic";
            this.p24_clrbtnMeterItemHistory.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemHistory.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemHistory.Image = null;
            this.p24_clrbtnMeterItemHistory.Location = new System.Drawing.Point(191, 122);
            this.p24_clrbtnMeterItemHistory.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemHistory.Name = "p24_clrbtnMeterItemHistory";
            this.p24_clrbtnMeterItemHistory.TabStop = true;
            this.p24_clrbtnMeterItemHistory.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemHistory.TabIndex = 83;
            this.p24_clrbtnMeterItemHistory.Changed += new System.EventHandler(this.clrbtnMeterItemHistory_Changed);

            // 
            // p24_clrbtnMeterItemIndicator
            // 
            this.p24_clrbtnMeterItemIndicator.Automatic = "Automatic";
            this.p24_clrbtnMeterItemIndicator.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemIndicator.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemIndicator.Image = null;
            this.p24_clrbtnMeterItemIndicator.Location = new System.Drawing.Point(55, 29);
            this.p24_clrbtnMeterItemIndicator.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemIndicator.Name = "p24_clrbtnMeterItemIndicator";
            this.p24_clrbtnMeterItemIndicator.TabStop = true;
            this.p24_clrbtnMeterItemIndicator.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemIndicator.TabIndex = 80;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemIndicator, "Indicator colour");
            this.p24_clrbtnMeterItemIndicator.Changed += new System.EventHandler(this.clrbtnMeterItemIndicator_Changed);

            // 
            // p24_clrbtnMeterItemLow
            // 
            this.p24_clrbtnMeterItemLow.Automatic = "Automatic";
            this.p24_clrbtnMeterItemLow.Color = System.Drawing.Color.White;
            this.p24_clrbtnMeterItemLow.Image = null;
            this.p24_clrbtnMeterItemLow.Location = new System.Drawing.Point(55, 4);
            this.p24_clrbtnMeterItemLow.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemLow.Name = "p24_clrbtnMeterItemLow";
            this.p24_clrbtnMeterItemLow.TabStop = true;
            this.p24_clrbtnMeterItemLow.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemLow.TabIndex = 76;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemLow, "Low scale colour and value");
            this.p24_clrbtnMeterItemLow.Changed += new System.EventHandler(this.clrbtnMeterItemLow_Changed);

            // 
            // p24_clrbtnMeterItemMeterTitle
            // 
            this.p24_clrbtnMeterItemMeterTitle.Automatic = "Automatic";
            this.p24_clrbtnMeterItemMeterTitle.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemMeterTitle.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemMeterTitle.Image = null;
            this.p24_clrbtnMeterItemMeterTitle.Location = new System.Drawing.Point(85, 168);
            this.p24_clrbtnMeterItemMeterTitle.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemMeterTitle.Name = "p24_clrbtnMeterItemMeterTitle";
            this.p24_clrbtnMeterItemMeterTitle.TabStop = true;
            this.p24_clrbtnMeterItemMeterTitle.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemMeterTitle.TabIndex = 108;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemMeterTitle, "Meter title colour");
            this.p24_clrbtnMeterItemMeterTitle.Changed += new System.EventHandler(this.clrbtnMeterItemMeterTitle_Changed);

            // 
            // p24_clrbtnMeterItemPeakHold
            // 
            this.p24_clrbtnMeterItemPeakHold.Automatic = "Automatic";
            this.p24_clrbtnMeterItemPeakHold.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemPeakHold.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemPeakHold.Image = null;
            this.p24_clrbtnMeterItemPeakHold.Location = new System.Drawing.Point(191, 168);
            this.p24_clrbtnMeterItemPeakHold.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemPeakHold.Name = "p24_clrbtnMeterItemPeakHold";
            this.p24_clrbtnMeterItemPeakHold.TabStop = true;
            this.p24_clrbtnMeterItemPeakHold.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemPeakHold.TabIndex = 82;
            this.p24_clrbtnMeterItemPeakHold.Changed += new System.EventHandler(this.clrbtnMeterItemPeakHold_Changed);

            // 
            // p24_clrbtnMeterItemPeakValueColour
            // 
            this.p24_clrbtnMeterItemPeakValueColour.Automatic = "Automatic";
            this.p24_clrbtnMeterItemPeakValueColour.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemPeakValueColour.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemPeakValueColour.Image = null;
            this.p24_clrbtnMeterItemPeakValueColour.Location = new System.Drawing.Point(85, 191);
            this.p24_clrbtnMeterItemPeakValueColour.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemPeakValueColour.Name = "p24_clrbtnMeterItemPeakValueColour";
            this.p24_clrbtnMeterItemPeakValueColour.TabStop = true;
            this.p24_clrbtnMeterItemPeakValueColour.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemPeakValueColour.TabIndex = 107;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemPeakValueColour, "Peak value colour");
            this.p24_clrbtnMeterItemPeakValueColour.Changed += new System.EventHandler(this.clrbtnMeterItemPeakValueColour_Changed);

            // 
            // p24_clrbtnMeterItemPowerScale
            // 
            this.p24_clrbtnMeterItemPowerScale.Automatic = "Automatic";
            this.p24_clrbtnMeterItemPowerScale.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemPowerScale.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemPowerScale.Image = null;
            this.p24_clrbtnMeterItemPowerScale.Location = new System.Drawing.Point(249, 240);
            this.p24_clrbtnMeterItemPowerScale.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemPowerScale.Name = "p24_clrbtnMeterItemPowerScale";
            this.p24_clrbtnMeterItemPowerScale.TabStop = true;
            this.p24_clrbtnMeterItemPowerScale.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemPowerScale.TabIndex = 125;
            this.p24_clrbtnMeterItemPowerScale.Changed += new System.EventHandler(this.clrbtnMeterItemPowerScale_Changed);

            // 
            // p24_clrbtnMeterItemRotatorArrow
            // 
            this.p24_clrbtnMeterItemRotatorArrow.Automatic = "Automatic";
            this.p24_clrbtnMeterItemRotatorArrow.Color = System.Drawing.Color.White;
            this.p24_clrbtnMeterItemRotatorArrow.Image = null;
            this.p24_clrbtnMeterItemRotatorArrow.Location = new System.Drawing.Point(83, 52);
            this.p24_clrbtnMeterItemRotatorArrow.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemRotatorArrow.Name = "p24_clrbtnMeterItemRotatorArrow";
            this.p24_clrbtnMeterItemRotatorArrow.TabStop = true;
            this.p24_clrbtnMeterItemRotatorArrow.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemRotatorArrow.TabIndex = 76;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemRotatorArrow, "The arrow/pointer colour");
            this.p24_clrbtnMeterItemRotatorArrow.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorArrow_Changed);

            // 
            // p24_clrbtnMeterItemRotatorBeamWidth
            // 
            this.p24_clrbtnMeterItemRotatorBeamWidth.Automatic = "Automatic";
            this.p24_clrbtnMeterItemRotatorBeamWidth.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemRotatorBeamWidth.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemRotatorBeamWidth.Image = null;
            this.p24_clrbtnMeterItemRotatorBeamWidth.Location = new System.Drawing.Point(83, 139);
            this.p24_clrbtnMeterItemRotatorBeamWidth.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemRotatorBeamWidth.Name = "p24_clrbtnMeterItemRotatorBeamWidth";
            this.p24_clrbtnMeterItemRotatorBeamWidth.TabStop = true;
            this.p24_clrbtnMeterItemRotatorBeamWidth.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemRotatorBeamWidth.TabIndex = 120;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemRotatorBeamWidth, "Beam width colour");
            this.p24_clrbtnMeterItemRotatorBeamWidth.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorBeamWidth_Changed);

            // 
            // p24_clrbtnMeterItemRotatorControlColour
            // 
            this.p24_clrbtnMeterItemRotatorControlColour.Automatic = "Automatic";
            this.p24_clrbtnMeterItemRotatorControlColour.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemRotatorControlColour.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemRotatorControlColour.Image = null;
            this.p24_clrbtnMeterItemRotatorControlColour.Location = new System.Drawing.Point(110, 268);
            this.p24_clrbtnMeterItemRotatorControlColour.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemRotatorControlColour.Name = "p24_clrbtnMeterItemRotatorControlColour";
            this.p24_clrbtnMeterItemRotatorControlColour.TabStop = true;
            this.p24_clrbtnMeterItemRotatorControlColour.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemRotatorControlColour.TabIndex = 137;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemRotatorControlColour, "Control arrow colour");
            this.p24_clrbtnMeterItemRotatorControlColour.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorControlColour_Changed);

            // 
            // p24_clrbtnMeterItemRotatorLargeDot
            // 
            this.p24_clrbtnMeterItemRotatorLargeDot.Automatic = "Automatic";
            this.p24_clrbtnMeterItemRotatorLargeDot.Color = System.Drawing.Color.Red;
            this.p24_clrbtnMeterItemRotatorLargeDot.Image = null;
            this.p24_clrbtnMeterItemRotatorLargeDot.Location = new System.Drawing.Point(83, 81);
            this.p24_clrbtnMeterItemRotatorLargeDot.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemRotatorLargeDot.Name = "p24_clrbtnMeterItemRotatorLargeDot";
            this.p24_clrbtnMeterItemRotatorLargeDot.TabStop = true;
            this.p24_clrbtnMeterItemRotatorLargeDot.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemRotatorLargeDot.TabIndex = 77;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemRotatorLargeDot, "Large dot colour");
            this.p24_clrbtnMeterItemRotatorLargeDot.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorLargeDot_Changed);

            // 
            // p24_clrbtnMeterItemRotatorSmallDot
            // 
            this.p24_clrbtnMeterItemRotatorSmallDot.Automatic = "Automatic";
            this.p24_clrbtnMeterItemRotatorSmallDot.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemRotatorSmallDot.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemRotatorSmallDot.Image = null;
            this.p24_clrbtnMeterItemRotatorSmallDot.Location = new System.Drawing.Point(83, 110);
            this.p24_clrbtnMeterItemRotatorSmallDot.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemRotatorSmallDot.Name = "p24_clrbtnMeterItemRotatorSmallDot";
            this.p24_clrbtnMeterItemRotatorSmallDot.TabStop = true;
            this.p24_clrbtnMeterItemRotatorSmallDot.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemRotatorSmallDot.TabIndex = 80;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemRotatorSmallDot, "Small dot colour");
            this.p24_clrbtnMeterItemRotatorSmallDot.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorSmallDot_Changed);

            // 
            // p24_clrbtnMeterItemRotatorText
            // 
            this.p24_clrbtnMeterItemRotatorText.Automatic = "Automatic";
            this.p24_clrbtnMeterItemRotatorText.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemRotatorText.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemRotatorText.Image = null;
            this.p24_clrbtnMeterItemRotatorText.Location = new System.Drawing.Point(83, 194);
            this.p24_clrbtnMeterItemRotatorText.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemRotatorText.Name = "p24_clrbtnMeterItemRotatorText";
            this.p24_clrbtnMeterItemRotatorText.TabStop = true;
            this.p24_clrbtnMeterItemRotatorText.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemRotatorText.TabIndex = 132;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemRotatorText, "Text colour");
            this.p24_clrbtnMeterItemRotatorText.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorText_Changed);

            // 
            // p24_clrbtnMeterItemSegmentedSolidColourHigh
            // 
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.Automatic = "Automatic";
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.Image = null;
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.Location = new System.Drawing.Point(131, 122);
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.Name = "p24_clrbtnMeterItemSegmentedSolidColourHigh";
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.TabStop = true;
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.TabIndex = 116;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemSegmentedSolidColourHigh, "High section colour");
            this.p24_clrbtnMeterItemSegmentedSolidColourHigh.Changed += new System.EventHandler(this.clrbtnMeterItemSegmentedSolidColourHigh_Changed);

            // 
            // p24_clrbtnMeterItemSegmentedSolidColourLow
            // 
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.Automatic = "Automatic";
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.Image = null;
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.Location = new System.Drawing.Point(85, 122);
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.Name = "p24_clrbtnMeterItemSegmentedSolidColourLow";
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.TabStop = true;
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.TabIndex = 106;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemSegmentedSolidColourLow, "Low section colour");
            this.p24_clrbtnMeterItemSegmentedSolidColourLow.Changed += new System.EventHandler(this.clrbtnMeterItemSegmentedSolidColourLow_Changed);

            // 
            // p24_clrbtnMeterItemSubIndicator
            // 
            this.p24_clrbtnMeterItemSubIndicator.Automatic = "Automatic";
            this.p24_clrbtnMeterItemSubIndicator.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMeterItemSubIndicator.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMeterItemSubIndicator.Image = null;
            this.p24_clrbtnMeterItemSubIndicator.Location = new System.Drawing.Point(200, 29);
            this.p24_clrbtnMeterItemSubIndicator.MoreColors = "More Colors...";
            this.p24_clrbtnMeterItemSubIndicator.Name = "p24_clrbtnMeterItemSubIndicator";
            this.p24_clrbtnMeterItemSubIndicator.TabStop = true;
            this.p24_clrbtnMeterItemSubIndicator.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMeterItemSubIndicator.TabIndex = 120;
            this.toolTip1.SetToolTip(this.p24_clrbtnMeterItemSubIndicator, "Sub Indicator colour for sub needles and avg markers on some horizontal meters");
            this.p24_clrbtnMeterItemSubIndicator.Changed += new System.EventHandler(this.clrbtnMeterItemSubIndicator_Changed);

            // 
            // p24_clrbtnMultiMeter_vfo_lock
            // 
            this.p24_clrbtnMultiMeter_vfo_lock.Automatic = "Automatic";
            this.p24_clrbtnMultiMeter_vfo_lock.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMultiMeter_vfo_lock.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMultiMeter_vfo_lock.Image = null;
            this.p24_clrbtnMultiMeter_vfo_lock.Location = new System.Drawing.Point(203, 253);
            this.p24_clrbtnMultiMeter_vfo_lock.MoreColors = "More Colors...";
            this.p24_clrbtnMultiMeter_vfo_lock.Name = "p24_clrbtnMultiMeter_vfo_lock";
            this.p24_clrbtnMultiMeter_vfo_lock.TabStop = true;
            this.p24_clrbtnMultiMeter_vfo_lock.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMultiMeter_vfo_lock.TabIndex = 140;
            this.toolTip1.SetToolTip(this.p24_clrbtnMultiMeter_vfo_lock, "Lock colour");
            this.p24_clrbtnMultiMeter_vfo_lock.Changed += new System.EventHandler(this.clrbtnMultiMeter_vfo_lock_Changed);

            // 
            // p24_clrbtnMultiMeter_vfo_show_bandtext
            // 
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.Automatic = "Automatic";
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.Image = null;
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.Location = new System.Drawing.Point(203, 203);
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.MoreColors = "More Colors...";
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.Name = "p24_clrbtnMultiMeter_vfo_show_bandtext";
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.TabStop = true;
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.TabIndex = 136;
            this.toolTip1.SetToolTip(this.p24_clrbtnMultiMeter_vfo_show_bandtext, "Band text colour");
            this.p24_clrbtnMultiMeter_vfo_show_bandtext.Changed += new System.EventHandler(this.clrbtnMultiMeter_vfo_show_bandtext_Changed);

            // 
            // p24_clrbtnMultiMeter_vfo_sync
            // 
            this.p24_clrbtnMultiMeter_vfo_sync.Automatic = "Automatic";
            this.p24_clrbtnMultiMeter_vfo_sync.Color = System.Drawing.Color.Yellow;
            this.p24_clrbtnMultiMeter_vfo_sync.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnMultiMeter_vfo_sync.Image = null;
            this.p24_clrbtnMultiMeter_vfo_sync.Location = new System.Drawing.Point(203, 282);
            this.p24_clrbtnMultiMeter_vfo_sync.MoreColors = "More Colors...";
            this.p24_clrbtnMultiMeter_vfo_sync.Name = "p24_clrbtnMultiMeter_vfo_sync";
            this.p24_clrbtnMultiMeter_vfo_sync.TabStop = true;
            this.p24_clrbtnMultiMeter_vfo_sync.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnMultiMeter_vfo_sync.TabIndex = 142;
            this.toolTip1.SetToolTip(this.p24_clrbtnMultiMeter_vfo_sync, "Sync colour");
            this.p24_clrbtnMultiMeter_vfo_sync.Changed += new System.EventHandler(this.clrbtnMultiMeter_vfo_sync_Changed);

            // 
            // p24_clrbtnTextOverlay_PanelBackground
            // 
            this.p24_clrbtnTextOverlay_PanelBackground.Automatic = "Automatic";
            this.p24_clrbtnTextOverlay_PanelBackground.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnTextOverlay_PanelBackground.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnTextOverlay_PanelBackground.Image = null;
            this.p24_clrbtnTextOverlay_PanelBackground.Location = new System.Drawing.Point(141, 45);
            this.p24_clrbtnTextOverlay_PanelBackground.MoreColors = "More Colors...";
            this.p24_clrbtnTextOverlay_PanelBackground.Name = "p24_clrbtnTextOverlay_PanelBackground";
            this.p24_clrbtnTextOverlay_PanelBackground.TabStop = true;
            this.p24_clrbtnTextOverlay_PanelBackground.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnTextOverlay_PanelBackground.TabIndex = 129;
            this.toolTip1.SetToolTip(this.p24_clrbtnTextOverlay_PanelBackground, "Background colour");
            this.p24_clrbtnTextOverlay_PanelBackground.Changed += new System.EventHandler(this.clrbtnTextOverlay_PanelBackground_Changed);

            // 
            // p24_clrbtnTextOverlay_PanelBackgroundTX
            // 
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.Automatic = "Automatic";
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.Image = null;
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.Location = new System.Drawing.Point(141, 70);
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.MoreColors = "More Colors...";
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.Name = "p24_clrbtnTextOverlay_PanelBackgroundTX";
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.TabStop = true;
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.TabIndex = 162;
            this.toolTip1.SetToolTip(this.p24_clrbtnTextOverlay_PanelBackgroundTX, "Background colour");
            this.p24_clrbtnTextOverlay_PanelBackgroundTX.Changed += new System.EventHandler(this.clrbtnTextOverlay_PanelBackgroundTX_Changed);

            // 
            // p24_clrbtnTextOverlay_TextBackColour1
            // 
            this.p24_clrbtnTextOverlay_TextBackColour1.Automatic = "Automatic";
            this.p24_clrbtnTextOverlay_TextBackColour1.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnTextOverlay_TextBackColour1.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnTextOverlay_TextBackColour1.Image = null;
            this.p24_clrbtnTextOverlay_TextBackColour1.Location = new System.Drawing.Point(133, 183);
            this.p24_clrbtnTextOverlay_TextBackColour1.MoreColors = "More Colors...";
            this.p24_clrbtnTextOverlay_TextBackColour1.Name = "p24_clrbtnTextOverlay_TextBackColour1";
            this.p24_clrbtnTextOverlay_TextBackColour1.TabStop = true;
            this.p24_clrbtnTextOverlay_TextBackColour1.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnTextOverlay_TextBackColour1.TabIndex = 156;
            this.toolTip1.SetToolTip(this.p24_clrbtnTextOverlay_TextBackColour1, "Background colour");
            this.p24_clrbtnTextOverlay_TextBackColour1.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextBackColour1_Changed);

            // 
            // p24_clrbtnTextOverlay_TextBackColour2
            // 
            this.p24_clrbtnTextOverlay_TextBackColour2.Automatic = "Automatic";
            this.p24_clrbtnTextOverlay_TextBackColour2.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnTextOverlay_TextBackColour2.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnTextOverlay_TextBackColour2.Image = null;
            this.p24_clrbtnTextOverlay_TextBackColour2.Location = new System.Drawing.Point(133, 209);
            this.p24_clrbtnTextOverlay_TextBackColour2.MoreColors = "More Colors...";
            this.p24_clrbtnTextOverlay_TextBackColour2.Name = "p24_clrbtnTextOverlay_TextBackColour2";
            this.p24_clrbtnTextOverlay_TextBackColour2.TabStop = true;
            this.p24_clrbtnTextOverlay_TextBackColour2.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnTextOverlay_TextBackColour2.TabIndex = 157;
            this.toolTip1.SetToolTip(this.p24_clrbtnTextOverlay_TextBackColour2, "Background colour");
            this.p24_clrbtnTextOverlay_TextBackColour2.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextBackColour2_Changed);

            // 
            // p24_clrbtnTextOverlay_TextColour1
            // 
            this.p24_clrbtnTextOverlay_TextColour1.Automatic = "Automatic";
            this.p24_clrbtnTextOverlay_TextColour1.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnTextOverlay_TextColour1.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnTextOverlay_TextColour1.Image = null;
            this.p24_clrbtnTextOverlay_TextColour1.Location = new System.Drawing.Point(87, 183);
            this.p24_clrbtnTextOverlay_TextColour1.MoreColors = "More Colors...";
            this.p24_clrbtnTextOverlay_TextColour1.Name = "p24_clrbtnTextOverlay_TextColour1";
            this.p24_clrbtnTextOverlay_TextColour1.TabStop = true;
            this.p24_clrbtnTextOverlay_TextColour1.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnTextOverlay_TextColour1.TabIndex = 133;
            this.toolTip1.SetToolTip(this.p24_clrbtnTextOverlay_TextColour1, "Font colour");
            this.p24_clrbtnTextOverlay_TextColour1.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextColour1_Changed);

            // 
            // p24_clrbtnTextOverlay_TextColour2
            // 
            this.p24_clrbtnTextOverlay_TextColour2.Automatic = "Automatic";
            this.p24_clrbtnTextOverlay_TextColour2.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnTextOverlay_TextColour2.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnTextOverlay_TextColour2.Image = null;
            this.p24_clrbtnTextOverlay_TextColour2.Location = new System.Drawing.Point(87, 209);
            this.p24_clrbtnTextOverlay_TextColour2.MoreColors = "More Colors...";
            this.p24_clrbtnTextOverlay_TextColour2.Name = "p24_clrbtnTextOverlay_TextColour2";
            this.p24_clrbtnTextOverlay_TextColour2.TabStop = true;
            this.p24_clrbtnTextOverlay_TextColour2.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnTextOverlay_TextColour2.TabIndex = 142;
            this.toolTip1.SetToolTip(this.p24_clrbtnTextOverlay_TextColour2, "Font colour");
            this.p24_clrbtnTextOverlay_TextColour2.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextColour2_Changed);

            // 
            // p24_clrbtnWaveRecord_back
            // 
            this.p24_clrbtnWaveRecord_back.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_back.Color = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(16)))));
            this.p24_clrbtnWaveRecord_back.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_back.Image = null;
            this.p24_clrbtnWaveRecord_back.Location = new System.Drawing.Point(118, 72);
            this.p24_clrbtnWaveRecord_back.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_back.Name = "p24_clrbtnWaveRecord_back";
            this.p24_clrbtnWaveRecord_back.TabStop = true;
            this.p24_clrbtnWaveRecord_back.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_back.TabIndex = 3;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_back, "Panel background colour");
            this.p24_clrbtnWaveRecord_back.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_border
            // 
            this.p24_clrbtnWaveRecord_border.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_border.Color = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.p24_clrbtnWaveRecord_border.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_border.Image = null;
            this.p24_clrbtnWaveRecord_border.Location = new System.Drawing.Point(272, 72);
            this.p24_clrbtnWaveRecord_border.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_border.Name = "p24_clrbtnWaveRecord_border";
            this.p24_clrbtnWaveRecord_border.TabStop = true;
            this.p24_clrbtnWaveRecord_border.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_border.TabIndex = 4;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_border, "Row border colour");
            this.p24_clrbtnWaveRecord_border.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_button_border
            // 
            this.p24_clrbtnWaveRecord_button_border.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_button_border.Color = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(86)))), ((int)(((byte)(86)))));
            this.p24_clrbtnWaveRecord_button_border.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_button_border.Image = null;
            this.p24_clrbtnWaveRecord_button_border.Location = new System.Drawing.Point(272, 120);
            this.p24_clrbtnWaveRecord_button_border.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_button_border.Name = "p24_clrbtnWaveRecord_button_border";
            this.p24_clrbtnWaveRecord_button_border.TabStop = true;
            this.p24_clrbtnWaveRecord_button_border.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_button_border.TabIndex = 8;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_button_border, "Button border colour");
            this.p24_clrbtnWaveRecord_button_border.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_button_fill
            // 
            this.p24_clrbtnWaveRecord_button_fill.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_button_fill.Color = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.p24_clrbtnWaveRecord_button_fill.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_button_fill.Image = null;
            this.p24_clrbtnWaveRecord_button_fill.Location = new System.Drawing.Point(118, 120);
            this.p24_clrbtnWaveRecord_button_fill.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_button_fill.Name = "p24_clrbtnWaveRecord_button_fill";
            this.p24_clrbtnWaveRecord_button_fill.TabStop = true;
            this.p24_clrbtnWaveRecord_button_fill.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_button_fill.TabIndex = 7;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_button_fill, "Button background colour");
            this.p24_clrbtnWaveRecord_button_fill.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_button_hover
            // 
            this.p24_clrbtnWaveRecord_button_hover.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_button_hover.Color = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.p24_clrbtnWaveRecord_button_hover.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_button_hover.Image = null;
            this.p24_clrbtnWaveRecord_button_hover.Location = new System.Drawing.Point(118, 144);
            this.p24_clrbtnWaveRecord_button_hover.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_button_hover.Name = "p24_clrbtnWaveRecord_button_hover";
            this.p24_clrbtnWaveRecord_button_hover.TabStop = true;
            this.p24_clrbtnWaveRecord_button_hover.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_button_hover.TabIndex = 9;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_button_hover, "Button hover fill colour");
            this.p24_clrbtnWaveRecord_button_hover.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_delete
            // 
            this.p24_clrbtnWaveRecord_delete.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_delete.Color = System.Drawing.Color.Orange;
            this.p24_clrbtnWaveRecord_delete.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_delete.Image = null;
            this.p24_clrbtnWaveRecord_delete.Location = new System.Drawing.Point(272, 168);
            this.p24_clrbtnWaveRecord_delete.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_delete.Name = "p24_clrbtnWaveRecord_delete";
            this.p24_clrbtnWaveRecord_delete.TabStop = true;
            this.p24_clrbtnWaveRecord_delete.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_delete.TabIndex = 12;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_delete, "Trash icon colour");
            this.p24_clrbtnWaveRecord_delete.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_play
            // 
            this.p24_clrbtnWaveRecord_play.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_play.Color = System.Drawing.Color.LimeGreen;
            this.p24_clrbtnWaveRecord_play.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_play.Image = null;
            this.p24_clrbtnWaveRecord_play.Location = new System.Drawing.Point(272, 144);
            this.p24_clrbtnWaveRecord_play.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_play.Name = "p24_clrbtnWaveRecord_play";
            this.p24_clrbtnWaveRecord_play.TabStop = true;
            this.p24_clrbtnWaveRecord_play.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_play.TabIndex = 10;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_play, "Play icon colour");
            this.p24_clrbtnWaveRecord_play.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_row
            // 
            this.p24_clrbtnWaveRecord_row.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_row.Color = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.p24_clrbtnWaveRecord_row.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_row.Image = null;
            this.p24_clrbtnWaveRecord_row.Location = new System.Drawing.Point(118, 96);
            this.p24_clrbtnWaveRecord_row.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_row.Name = "p24_clrbtnWaveRecord_row";
            this.p24_clrbtnWaveRecord_row.TabStop = true;
            this.p24_clrbtnWaveRecord_row.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_row.TabIndex = 5;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_row, "Row background colour");
            this.p24_clrbtnWaveRecord_row.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_scroll_hover
            // 
            this.p24_clrbtnWaveRecord_scroll_hover.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_scroll_hover.Color = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.p24_clrbtnWaveRecord_scroll_hover.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_scroll_hover.Image = null;
            this.p24_clrbtnWaveRecord_scroll_hover.Location = new System.Drawing.Point(118, 216);
            this.p24_clrbtnWaveRecord_scroll_hover.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_scroll_hover.Name = "p24_clrbtnWaveRecord_scroll_hover";
            this.p24_clrbtnWaveRecord_scroll_hover.TabStop = true;
            this.p24_clrbtnWaveRecord_scroll_hover.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_scroll_hover.TabIndex = 15;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_scroll_hover, "Scrollbar thumb hover colour");
            this.p24_clrbtnWaveRecord_scroll_hover.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_scroll_thumb
            // 
            this.p24_clrbtnWaveRecord_scroll_thumb.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_scroll_thumb.Color = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(118)))), ((int)(((byte)(118)))));
            this.p24_clrbtnWaveRecord_scroll_thumb.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_scroll_thumb.Image = null;
            this.p24_clrbtnWaveRecord_scroll_thumb.Location = new System.Drawing.Point(272, 192);
            this.p24_clrbtnWaveRecord_scroll_thumb.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_scroll_thumb.Name = "p24_clrbtnWaveRecord_scroll_thumb";
            this.p24_clrbtnWaveRecord_scroll_thumb.TabStop = true;
            this.p24_clrbtnWaveRecord_scroll_thumb.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_scroll_thumb.TabIndex = 14;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_scroll_thumb, "Scrollbar thumb colour");
            this.p24_clrbtnWaveRecord_scroll_thumb.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_scroll_track
            // 
            this.p24_clrbtnWaveRecord_scroll_track.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_scroll_track.Color = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(36)))));
            this.p24_clrbtnWaveRecord_scroll_track.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_scroll_track.Image = null;
            this.p24_clrbtnWaveRecord_scroll_track.Location = new System.Drawing.Point(118, 192);
            this.p24_clrbtnWaveRecord_scroll_track.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_scroll_track.Name = "p24_clrbtnWaveRecord_scroll_track";
            this.p24_clrbtnWaveRecord_scroll_track.TabStop = true;
            this.p24_clrbtnWaveRecord_scroll_track.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_scroll_track.TabIndex = 13;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_scroll_track, "Scrollbar track colour");
            this.p24_clrbtnWaveRecord_scroll_track.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_stop
            // 
            this.p24_clrbtnWaveRecord_stop.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_stop.Color = System.Drawing.Color.Olive;
            this.p24_clrbtnWaveRecord_stop.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_stop.Image = null;
            this.p24_clrbtnWaveRecord_stop.Location = new System.Drawing.Point(118, 168);
            this.p24_clrbtnWaveRecord_stop.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_stop.Name = "p24_clrbtnWaveRecord_stop";
            this.p24_clrbtnWaveRecord_stop.TabStop = true;
            this.p24_clrbtnWaveRecord_stop.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_stop.TabIndex = 11;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_stop, "Stop icon colour");
            this.p24_clrbtnWaveRecord_stop.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_clrbtnWaveRecord_text
            // 
            this.p24_clrbtnWaveRecord_text.Automatic = "Automatic";
            this.p24_clrbtnWaveRecord_text.Color = System.Drawing.Color.White;
            this.p24_clrbtnWaveRecord_text.ForeColor = System.Drawing.Color.Black;
            this.p24_clrbtnWaveRecord_text.Image = null;
            this.p24_clrbtnWaveRecord_text.Location = new System.Drawing.Point(272, 96);
            this.p24_clrbtnWaveRecord_text.MoreColors = "More Colors...";
            this.p24_clrbtnWaveRecord_text.Name = "p24_clrbtnWaveRecord_text";
            this.p24_clrbtnWaveRecord_text.TabStop = true;
            this.p24_clrbtnWaveRecord_text.Size = new System.Drawing.Size(40, 23);
            this.p24_clrbtnWaveRecord_text.TabIndex = 6;
            this.toolTip1.SetToolTip(this.p24_clrbtnWaveRecord_text, "Recording name colour");
            this.p24_clrbtnWaveRecord_text.Changed += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_comboContainerSelect
            // 
            this.p24_comboContainerSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboContainerSelect.FormattingEnabled = true;
            this.p24_comboContainerSelect.Location = new System.Drawing.Point(6, 13);
            this.p24_comboContainerSelect.Name = "p24_comboContainerSelect";
            this.p24_comboContainerSelect.Size = new System.Drawing.Size(193, 21);
            this.p24_comboContainerSelect.TabIndex = 87;
            this.toolTip1.SetToolTip(this.p24_comboContainerSelect, "Selected container. Note: each will get own id in the title text when undocked");
            this.p24_comboContainerSelect.SelectedIndexChanged += new System.EventHandler(this.comboContainerSelect_SelectedIndexChanged);

            // 
            // p24_comboFilter_wf_palette
            // 
            this.p24_comboFilter_wf_palette.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboFilter_wf_palette.DropDownWidth = 48;
            this.p24_comboFilter_wf_palette.Items.AddRange(new object[] {
            "Enhanced",
            "Spectran",
            "BlackWhite",
            "LinLog",
            "LinRad",
            "LinAuto",
            "Custom"});
            this.p24_comboFilter_wf_palette.Location = new System.Drawing.Point(217, 5);
            this.p24_comboFilter_wf_palette.Name = "p24_comboFilter_wf_palette";
            this.p24_comboFilter_wf_palette.Size = new System.Drawing.Size(72, 21);
            this.p24_comboFilter_wf_palette.TabIndex = 166;
            this.toolTip1.SetToolTip(this.p24_comboFilter_wf_palette, "Sets the color scheme");
            this.p24_comboFilter_wf_palette.SelectedIndexChanged += new System.EventHandler(this.comboFilter_wf_palette_SelectedIndexChanged);

            // 
            // p24_comboHistory_reading_0
            // 
            this.p24_comboHistory_reading_0.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboHistory_reading_0.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_comboHistory_reading_0.FormattingEnabled = true;
            this.p24_comboHistory_reading_0.Location = new System.Drawing.Point(73, 21);
            this.p24_comboHistory_reading_0.Name = "p24_comboHistory_reading_0";
            this.p24_comboHistory_reading_0.Size = new System.Drawing.Size(136, 23);
            this.p24_comboHistory_reading_0.TabIndex = 137;
            this.p24_comboHistory_reading_0.SelectedIndexChanged += new System.EventHandler(this.comboHistory_reading_0_SelectedIndexChanged);

            // 
            // p24_comboHistory_reading_1
            // 
            this.p24_comboHistory_reading_1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboHistory_reading_1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_comboHistory_reading_1.FormattingEnabled = true;
            this.p24_comboHistory_reading_1.Location = new System.Drawing.Point(73, 21);
            this.p24_comboHistory_reading_1.Name = "p24_comboHistory_reading_1";
            this.p24_comboHistory_reading_1.Size = new System.Drawing.Size(136, 23);
            this.p24_comboHistory_reading_1.TabIndex = 137;
            this.p24_comboHistory_reading_1.SelectedIndexChanged += new System.EventHandler(this.comboHistory_reading_1_SelectedIndexChanged);

            // 
            // p24_comboWebImage_BsdWorld
            // 
            this.p24_comboWebImage_BsdWorld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboWebImage_BsdWorld.FormattingEnabled = true;
            this.p24_comboWebImage_BsdWorld.Location = new System.Drawing.Point(12, 15);
            this.p24_comboWebImage_BsdWorld.Name = "p24_comboWebImage_BsdWorld";
            this.p24_comboWebImage_BsdWorld.Size = new System.Drawing.Size(190, 21);
            this.p24_comboWebImage_BsdWorld.TabIndex = 0;
            this.p24_comboWebImage_BsdWorld.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_BsdWorld_SelectedIndexChanged);

            // 
            // p24_comboWebImage_HamQsl
            // 
            this.p24_comboWebImage_HamQsl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboWebImage_HamQsl.FormattingEnabled = true;
            this.p24_comboWebImage_HamQsl.Location = new System.Drawing.Point(12, 15);
            this.p24_comboWebImage_HamQsl.Name = "p24_comboWebImage_HamQsl";
            this.p24_comboWebImage_HamQsl.Size = new System.Drawing.Size(190, 21);
            this.p24_comboWebImage_HamQsl.TabIndex = 0;
            this.p24_comboWebImage_HamQsl.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_HamQsl_SelectedIndexChanged);

            // 
            // p24_comboWebImage_nasa
            // 
            this.p24_comboWebImage_nasa.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboWebImage_nasa.FormattingEnabled = true;
            this.p24_comboWebImage_nasa.Location = new System.Drawing.Point(12, 15);
            this.p24_comboWebImage_nasa.Name = "p24_comboWebImage_nasa";
            this.p24_comboWebImage_nasa.Size = new System.Drawing.Size(190, 21);
            this.p24_comboWebImage_nasa.TabIndex = 0;
            this.p24_comboWebImage_nasa.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_nasa_SelectedIndexChanged);

            // 
            // p24_comboWebImage_noaa
            // 
            this.p24_comboWebImage_noaa.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.p24_comboWebImage_noaa.FormattingEnabled = true;
            this.p24_comboWebImage_noaa.Location = new System.Drawing.Point(12, 15);
            this.p24_comboWebImage_noaa.Name = "p24_comboWebImage_noaa";
            this.p24_comboWebImage_noaa.Size = new System.Drawing.Size(190, 21);
            this.p24_comboWebImage_noaa.TabIndex = 0;
            this.p24_comboWebImage_noaa.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_noaa_SelectedIndexChanged);

            // 
            // p24_groupBoxTS40
            // 
            this.p24_groupBoxTS40.Controls.Add(this.p24_btnWebImage_hamqsl_donate);
            this.p24_groupBoxTS40.Controls.Add(this.p24_comboWebImage_HamQsl);
            this.p24_groupBoxTS40.Location = new System.Drawing.Point(3, 3);
            this.p24_groupBoxTS40.Name = "p24_groupBoxTS40";
            this.p24_groupBoxTS40.Size = new System.Drawing.Size(278, 44);
            this.p24_groupBoxTS40.TabIndex = 141;
            this.p24_groupBoxTS40.TabStop = false;
            this.p24_groupBoxTS40.Text = "hamqsl.com";

            // 
            // p24_groupBoxTS41
            // 
            this.p24_groupBoxTS41.Controls.Add(this.p24_chkBSDWorldDarkMode);
            this.p24_groupBoxTS41.Controls.Add(this.p24_btnWebImage_bsdworld_visit);
            this.p24_groupBoxTS41.Controls.Add(this.p24_comboWebImage_BsdWorld);
            this.p24_groupBoxTS41.Location = new System.Drawing.Point(3, 47);
            this.p24_groupBoxTS41.Name = "p24_groupBoxTS41";
            this.p24_groupBoxTS41.Size = new System.Drawing.Size(278, 63);
            this.p24_groupBoxTS41.TabIndex = 143;
            this.p24_groupBoxTS41.TabStop = false;
            this.p24_groupBoxTS41.Text = "bsdworld.org";

            // 
            // p24_groupBoxTS42
            // 
            this.p24_groupBoxTS42.Controls.Add(this.p24_buttonTS1);
            this.p24_groupBoxTS42.Controls.Add(this.p24_comboWebImage_noaa);
            this.p24_groupBoxTS42.Location = new System.Drawing.Point(3, 155);
            this.p24_groupBoxTS42.Name = "p24_groupBoxTS42";
            this.p24_groupBoxTS42.Size = new System.Drawing.Size(278, 44);
            this.p24_groupBoxTS42.TabIndex = 145;
            this.p24_groupBoxTS42.TabStop = false;
            this.p24_groupBoxTS42.Text = "noaa";

            // 
            // p24_groupBoxTS43
            // 
            this.p24_groupBoxTS43.Controls.Add(this.p24_buttonTS2);
            this.p24_groupBoxTS43.Controls.Add(this.p24_comboWebImage_nasa);
            this.p24_groupBoxTS43.Location = new System.Drawing.Point(3, 111);
            this.p24_groupBoxTS43.Name = "p24_groupBoxTS43";
            this.p24_groupBoxTS43.Size = new System.Drawing.Size(278, 44);
            this.p24_groupBoxTS43.TabIndex = 144;
            this.p24_groupBoxTS43.TabStop = false;
            this.p24_groupBoxTS43.Text = "nasa";

            // 
            // p24_groupBoxTS45
            // 
            this.p24_groupBoxTS45.Controls.Add(this.p24_clrbtnHistory_colour_0);
            this.p24_groupBoxTS45.Controls.Add(this.p24_nudHistory_axis0_max);
            this.p24_groupBoxTS45.Controls.Add(this.p24_labelTS262);
            this.p24_groupBoxTS45.Controls.Add(this.p24_nudHistory_axis0_min);
            this.p24_groupBoxTS45.Controls.Add(this.p24_labelTS261);
            this.p24_groupBoxTS45.Controls.Add(this.p24_chkHistory_auto_0_scale);
            this.p24_groupBoxTS45.Controls.Add(this.p24_comboHistory_reading_0);
            this.p24_groupBoxTS45.Controls.Add(this.p24_labelTS260);
            this.p24_groupBoxTS45.Location = new System.Drawing.Point(14, 143);
            this.p24_groupBoxTS45.Name = "p24_groupBoxTS45";
            this.p24_groupBoxTS45.Size = new System.Drawing.Size(297, 102);
            this.p24_groupBoxTS45.TabIndex = 143;
            this.p24_groupBoxTS45.TabStop = false;
            this.p24_groupBoxTS45.Text = "Left Axis";

            // 
            // p24_groupBoxTS46
            // 
            this.p24_groupBoxTS46.Controls.Add(this.p24_clrbtnHistory_colour_1);
            this.p24_groupBoxTS46.Controls.Add(this.p24_btnHistory_copy_minmax_from_0);
            this.p24_groupBoxTS46.Controls.Add(this.p24_chkHistory_1_show_axis);
            this.p24_groupBoxTS46.Controls.Add(this.p24_nudHistory_axis1_max);
            this.p24_groupBoxTS46.Controls.Add(this.p24_labelTS263);
            this.p24_groupBoxTS46.Controls.Add(this.p24_nudHistory_axis1_min);
            this.p24_groupBoxTS46.Controls.Add(this.p24_labelTS264);
            this.p24_groupBoxTS46.Controls.Add(this.p24_chkHistory_auto_1_scale);
            this.p24_groupBoxTS46.Controls.Add(this.p24_comboHistory_reading_1);
            this.p24_groupBoxTS46.Controls.Add(this.p24_labelTS265);
            this.p24_groupBoxTS46.Location = new System.Drawing.Point(14, 252);
            this.p24_groupBoxTS46.Name = "p24_groupBoxTS46";
            this.p24_groupBoxTS46.Size = new System.Drawing.Size(297, 102);
            this.p24_groupBoxTS46.TabIndex = 144;
            this.p24_groupBoxTS46.TabStop = false;
            this.p24_groupBoxTS46.Text = "Right Axis";

            // 
            // p24_grpButtonBox
            // 
            this.p24_grpButtonBox.Controls.Add(this.p24_picButtonBoxInfo);
            this.p24_grpButtonBox.Controls.Add(this.p24_btnOtherButtons_reset_layout);
            this.p24_grpButtonBox.Controls.Add(this.p24_chkButtonBox_use_icons);
            this.p24_grpButtonBox.Controls.Add(this.p24_chkButtonBox_fix_text_size);
            this.p24_grpButtonBox.Controls.Add(this.p24_clrbtnButonBox_fontcolour);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS292);
            this.p24_grpButtonBox.Controls.Add(this.p24_clrbtnButonBox_click);
            this.p24_grpButtonBox.Controls.Add(this.p24_pnlButtonBox_antenna_toggles);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudButtonBox_font_y_shift);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS250);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudButtonBox_font_x_shift);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS247);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudButtonBox_font_scale);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS248);
            this.p24_grpButtonBox.Controls.Add(this.p24_lblBandButtons_indicator_style);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudBandButtons_indicator_style);
            this.p24_grpButtonBox.Controls.Add(this.p24_chkBandButtons_band_inactive_use);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS246);
            this.p24_grpButtonBox.Controls.Add(this.p24_clrbtnBandButtons_hover);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS245);
            this.p24_grpButtonBox.Controls.Add(this.p24_clrbtnBandButtons_fill);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS244);
            this.p24_grpButtonBox.Controls.Add(this.p24_clrbtnBandButtons_border);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS243);
            this.p24_grpButtonBox.Controls.Add(this.p24_clrbtnBandButtons_indicator_off);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS242);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudBandButtons_indicator_border);
            this.p24_grpButtonBox.Controls.Add(this.p24_lblBandButtons_indicator_border);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudBandButtons_height_ratio);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS241);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudBandButtons_radius);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS240);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudBandButtons_margin);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS239);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudBandButtons_border);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS238);
            this.p24_grpButtonBox.Controls.Add(this.p24_btnBandButtons_font);
            this.p24_grpButtonBox.Controls.Add(this.p24_chkBandButtons_use_indicator);
            this.p24_grpButtonBox.Controls.Add(this.p24_nudBandButtons_columns);
            this.p24_grpButtonBox.Controls.Add(this.p24_labelTS249);
            this.p24_grpButtonBox.Controls.Add(this.p24_clrbtnBandButtons_indicator_on);
            this.p24_grpButtonBox.Controls.Add(this.p24_chkBandButtons_fade_tx);
            this.p24_grpButtonBox.Controls.Add(this.p24_chkBandButtons_fade_rx);
            this.p24_grpButtonBox.Location = new System.Drawing.Point(12, 23);
            this.p24_grpButtonBox.Name = "p24_grpButtonBox";
            this.p24_grpButtonBox.Size = new System.Drawing.Size(323, 376);
            this.p24_grpButtonBox.TabIndex = 109;
            this.p24_grpButtonBox.TabStop = false;
            this.p24_grpButtonBox.Text = "Button Box";
            this.p24_grpButtonBox.Visible = false;

            // 
            // p24_grpDialDisplay
            // 
            this.p24_grpDialDisplay.Controls.Add(this.p24_nudDial_degrees_for_change);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS421);
            this.p24_grpDialDisplay.Controls.Add(this.p24_nudDial_max_increments);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS419);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS418);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_button_highlight);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS414);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_fast);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS415);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_hold);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS416);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_slow);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS417);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_ring);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS412);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_button_off);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS413);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_button_on);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS410);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_pad_pressed);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS411);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_pad);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS409);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_circle);
            this.p24_grpDialDisplay.Controls.Add(this.p24_chkDial_align);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS407);
            this.p24_grpDialDisplay.Controls.Add(this.p24_nudDial_interval);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS408);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS353);
            this.p24_grpDialDisplay.Controls.Add(this.p24_nudDial_decrement);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS354);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS352);
            this.p24_grpDialDisplay.Controls.Add(this.p24_nudDial_increment);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS351);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS350);
            this.p24_grpDialDisplay.Controls.Add(this.p24_clrbtnDial_text);
            this.p24_grpDialDisplay.Controls.Add(this.p24_chkDialDisplay_alwaysshow_vfos);
            this.p24_grpDialDisplay.Controls.Add(this.p24_nudDialDisplay_font_scale);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS420);
            this.p24_grpDialDisplay.Controls.Add(this.p24_nudDialDisplay_vertical_ratio);
            this.p24_grpDialDisplay.Controls.Add(this.p24_labelTS424);
            this.p24_grpDialDisplay.Controls.Add(this.p24_chkDialDisplay_fade_tx);
            this.p24_grpDialDisplay.Controls.Add(this.p24_chkDialDisplay_fade_rx);
            this.p24_grpDialDisplay.Location = new System.Drawing.Point(202, 43);
            this.p24_grpDialDisplay.Name = "p24_grpDialDisplay";
            this.p24_grpDialDisplay.Size = new System.Drawing.Size(323, 376);
            this.p24_grpDialDisplay.TabIndex = 112;
            this.p24_grpDialDisplay.TabStop = false;
            this.p24_grpDialDisplay.Text = "Dial Display";
            this.p24_grpDialDisplay.Visible = false;

            // 
            // p24_grpGlobalStopPlayRecord
            // 
            this.p24_grpGlobalStopPlayRecord.Controls.Add(this.p24_btnRecording_globalkeybind_assign);
            this.p24_grpGlobalStopPlayRecord.Controls.Add(this.p24_chkRecording_globalkeybind);
            this.p24_grpGlobalStopPlayRecord.Controls.Add(this.p24_txtRecording_globalkeybind);
            this.p24_grpGlobalStopPlayRecord.Location = new System.Drawing.Point(416, 90);
            this.p24_grpGlobalStopPlayRecord.Name = "p24_grpGlobalStopPlayRecord";
            this.p24_grpGlobalStopPlayRecord.Size = new System.Drawing.Size(162, 68);
            this.p24_grpGlobalStopPlayRecord.TabIndex = 96;
            this.p24_grpGlobalStopPlayRecord.TabStop = false;
            this.p24_grpGlobalStopPlayRecord.Text = "Global Stop Play/Record";

            // 
            // p24_grpHistoryItem
            // 
            this.p24_grpHistoryItem.Controls.Add(this.p24_labelTS281);
            this.p24_grpHistoryItem.Controls.Add(this.p24_labelTS280);
            this.p24_grpHistoryItem.Controls.Add(this.p24_clrbtnHistory_time);
            this.p24_grpHistoryItem.Controls.Add(this.p24_clrbtnHistory_lines);
            this.p24_grpHistoryItem.Controls.Add(this.p24_groupBoxTS46);
            this.p24_grpHistoryItem.Controls.Add(this.p24_groupBoxTS45);
            this.p24_grpHistoryItem.Controls.Add(this.p24_pnlVariableInUse_2_history);
            this.p24_grpHistoryItem.Controls.Add(this.p24_btnMMIO_variable_2_history);
            this.p24_grpHistoryItem.Controls.Add(this.p24_btnMMIO_variable_history);
            this.p24_grpHistoryItem.Controls.Add(this.p24_pnlVariableInUse_1_history);
            this.p24_grpHistoryItem.Controls.Add(this.p24_nudHistory_keep_for);
            this.p24_grpHistoryItem.Controls.Add(this.p24_labelTS259);
            this.p24_grpHistoryItem.Controls.Add(this.p24_nudHistory_update);
            this.p24_grpHistoryItem.Controls.Add(this.p24_labelTS252);
            this.p24_grpHistoryItem.Controls.Add(this.p24_nudHistory_vertical_ratio);
            this.p24_grpHistoryItem.Controls.Add(this.p24_labelTS253);
            this.p24_grpHistoryItem.Controls.Add(this.p24_labelTS258);
            this.p24_grpHistoryItem.Controls.Add(this.p24_clrbtnHistory_background);
            this.p24_grpHistoryItem.Controls.Add(this.p24_chkHistory_fade_tx);
            this.p24_grpHistoryItem.Controls.Add(this.p24_chkHistory_fade_rx);
            this.p24_grpHistoryItem.Location = new System.Drawing.Point(12, 14);
            this.p24_grpHistoryItem.Name = "p24_grpHistoryItem";
            this.p24_grpHistoryItem.Size = new System.Drawing.Size(323, 376);
            this.p24_grpHistoryItem.TabIndex = 110;
            this.p24_grpHistoryItem.TabStop = false;
            this.p24_grpHistoryItem.Text = "History Graph";
            this.p24_grpHistoryItem.Visible = false;

            // 
            // p24_grpLedIndicator
            // 
            this.p24_grpLedIndicator.Controls.Add(this.p24_btnLedIndicatorVarPicker);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLed_process_when_hidden);
            this.p24_grpLedIndicator.Controls.Add(this.p24_btnLedIndicator_4char_copy);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS482);
            this.p24_grpLedIndicator.Controls.Add(this.p24_txtLedIndicator_4char);
            this.p24_grpLedIndicator.Controls.Add(this.p24_nudLedIndicator_UpdateInterval);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS287);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLed_notx_false);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLed_notx_true);
            this.p24_grpLedIndicator.Controls.Add(this.p24_radLed_light_pulsate);
            this.p24_grpLedIndicator.Controls.Add(this.p24_radLed_light_blink);
            this.p24_grpLedIndicator.Controls.Add(this.p24_radLed_light_on_off);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLed_show_false);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLed_show_true);
            this.p24_grpLedIndicator.Controls.Add(this.p24_lblLed_Valid);
            this.p24_grpLedIndicator.Controls.Add(this.p24_btnLedIndicator_copy_truefalse_colours);
            this.p24_grpLedIndicator.Controls.Add(this.p24_lblLedIndicator_panelbackgroundTX);
            this.p24_grpLedIndicator.Controls.Add(this.p24_clrbtnLedIndicator_PanelBackgroundTX);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS219);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS220);
            this.p24_grpLedIndicator.Controls.Add(this.p24_clrbtnLedIndicator_false);
            this.p24_grpLedIndicator.Controls.Add(this.p24_clrbtnLedIndicator_true);
            this.p24_grpLedIndicator.Controls.Add(this.p24_btnLedIndicator_copy_sizex_to_y);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS221);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS222);
            this.p24_grpLedIndicator.Controls.Add(this.p24_nudLedIndicator_ySize);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS223);
            this.p24_grpLedIndicator.Controls.Add(this.p24_nudLedIndicator_xSize);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS224);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS226);
            this.p24_grpLedIndicator.Controls.Add(this.p24_nudLedIndicator_yOffset);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS231);
            this.p24_grpLedIndicator.Controls.Add(this.p24_nudLedIndicator_xOffset);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS233);
            this.p24_grpLedIndicator.Controls.Add(this.p24_txtLedIndicator_condition);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLedIndicator_ShowPanel);
            this.p24_grpLedIndicator.Controls.Add(this.p24_nudLedIndicator_PanelPadding);
            this.p24_grpLedIndicator.Controls.Add(this.p24_labelTS234);
            this.p24_grpLedIndicator.Controls.Add(this.p24_lblLedIndicator_panelbackground);
            this.p24_grpLedIndicator.Controls.Add(this.p24_clrbtnLedIndicator_PanelBackground);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLedIndicator_FadeOnTX);
            this.p24_grpLedIndicator.Controls.Add(this.p24_chkLedIndicator_FadeOnRX);
            this.p24_grpLedIndicator.Location = new System.Drawing.Point(12, 23);
            this.p24_grpLedIndicator.Name = "p24_grpLedIndicator";
            this.p24_grpLedIndicator.Size = new System.Drawing.Size(323, 376);
            this.p24_grpLedIndicator.TabIndex = 107;
            this.p24_grpLedIndicator.TabStop = false;
            this.p24_grpLedIndicator.Text = "Led Indicator";
            this.p24_grpLedIndicator.Visible = false;

            // 
            // p24_grpMeterItemClockSettings
            // 
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_lblMMClockBackground);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_clrbtnMMClockBackground);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_labelTS164);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_clrbtnMMDate);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_labelTS162);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_clrbtnMMTime);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_clrbtnMMClockTitle);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_chkMMClockTitle);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_radMM24Clock);
            this.p24_grpMeterItemClockSettings.Controls.Add(this.p24_radMM12Clock);
            this.p24_grpMeterItemClockSettings.Location = new System.Drawing.Point(12, 14);
            this.p24_grpMeterItemClockSettings.Name = "p24_grpMeterItemClockSettings";
            this.p24_grpMeterItemClockSettings.Size = new System.Drawing.Size(323, 376);
            this.p24_grpMeterItemClockSettings.TabIndex = 100;
            this.p24_grpMeterItemClockSettings.TabStop = false;
            this.p24_grpMeterItemClockSettings.Text = "Clock Settings";
            this.p24_grpMeterItemClockSettings.Visible = false;

            // 
            // p24_grpMeterItemDataOutNode
            // 
            this.p24_grpMeterItemDataOutNode.Controls.Add(this.p24_labelTS210);
            this.p24_grpMeterItemDataOutNode.Controls.Add(this.p24_txtDataOutNode_4charID);
            this.p24_grpMeterItemDataOutNode.Controls.Add(this.p24_labelTS217);
            this.p24_grpMeterItemDataOutNode.Controls.Add(this.p24_nudDataOutNode_sendinterval);
            this.p24_grpMeterItemDataOutNode.Controls.Add(this.p24_labelTS215);
            this.p24_grpMeterItemDataOutNode.Location = new System.Drawing.Point(12, 15);
            this.p24_grpMeterItemDataOutNode.Name = "p24_grpMeterItemDataOutNode";
            this.p24_grpMeterItemDataOutNode.Size = new System.Drawing.Size(323, 376);
            this.p24_grpMeterItemDataOutNode.TabIndex = 105;
            this.p24_grpMeterItemDataOutNode.TabStop = false;
            this.p24_grpMeterItemDataOutNode.Text = "Data Out Node";
            this.p24_grpMeterItemDataOutNode.Visible = false;

            // 
            // p24_grpMeterItemFilterDisplay
            // 
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_labelTS349);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_nudFilter_lower_characteristic);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_scrlFilter);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_labelTS348);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_labelTS339);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_chkFilter_characteristic);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_nudFilter_waterfall_frame_update);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_labelTS341);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_nudFilterItem_font_scale);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_labelTS297);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_radFilterItem_none);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_radFilterItem_panadaptor);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_radFilterItem_waterfall);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_radFilterItem_panafall);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_pnlFilterModeModifiers);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_nudFilterDisplay_fixed_tx_zoom_level);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_chkFilterDisplay_fixed_tx_zoom);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_nudFilterDisplay_fixed_zoom_level);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_chkFilterDisplay_fixed_zoom);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_chkFilterDisplay_show_limits);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_nudFilterDisplay_vertical_ratio);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_labelTS332);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_labelTS333);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_clrbtnFilterDisplay_backcolour);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_chkFilterDisplay_fadeontx);
            this.p24_grpMeterItemFilterDisplay.Controls.Add(this.p24_chkFilterDisplay_fadeonrx);
            this.p24_grpMeterItemFilterDisplay.Location = new System.Drawing.Point(26, 24);
            this.p24_grpMeterItemFilterDisplay.Name = "p24_grpMeterItemFilterDisplay";
            this.p24_grpMeterItemFilterDisplay.Size = new System.Drawing.Size(323, 376);
            this.p24_grpMeterItemFilterDisplay.TabIndex = 111;
            this.p24_grpMeterItemFilterDisplay.TabStop = false;
            this.p24_grpMeterItemFilterDisplay.Text = "Filter Display";
            this.p24_grpMeterItemFilterDisplay.Visible = false;

            // 
            // p24_grpMeterItemRotator
            // 
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_lblMeterItemRotatorBeamWidth_alpha);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_lblMeterItemRotatorBeamWidth_degrees);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_nudMeterItemRotatorBeamWidth_alpha);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS237);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_txtMeterItemRotatorSTOPcommand);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_nudMeterItemRotator_padding);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_radMeterItemRotator_show_both);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_radMeterItemRotator_show_ele);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_radMeterItemRotator_show_az);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_lblRotator_4charID);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_txtRotator_4charID);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_bntMultiMeterItemRotator_default_pstRotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_picMultiMeterRotatorControlInfo);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_lblMeterItemRotatorELEcommand);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_lblMeterItemRotatorAZcommand);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_txtMeterItemRotatorELEcommand);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_txtMeterItemRotatorAZcommand);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_clrbtnMeterItemRotatorControlColour);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_chkMeterItemRotatorAllowControl);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_pnlVariableInUse_2_rotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_chkMeterItemRotatorCardinals);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS212);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_clrbtnMeterItemRotatorText);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_nudMeterItemRotatorBeamWidth);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_btnMMIO_variable_2_rotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_btnMMIO_variable_rotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_chkMeterItemRotatorShowBeamWidth);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS218);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_clrbtnMeterItemRotatorBeamWidth);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_chkMeterItemDarkModeRotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_nudMeterItemUpdateRateRotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS225);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS227);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_clrbtnMeterItemHBackgroundRotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS228);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_clrbtnMeterItemRotatorSmallDot);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS229);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_labelTS230);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_clrbtnMeterItemRotatorLargeDot);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_clrbtnMeterItemRotatorArrow);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_chkMeterItemFadeOnTxRotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_chkMeterItemFadeOnRxRotator);
            this.p24_grpMeterItemRotator.Controls.Add(this.p24_pnlVariableInUse_1_rotator);
            this.p24_grpMeterItemRotator.Location = new System.Drawing.Point(17, 24);
            this.p24_grpMeterItemRotator.Name = "p24_grpMeterItemRotator";
            this.p24_grpMeterItemRotator.Size = new System.Drawing.Size(323, 376);
            this.p24_grpMeterItemRotator.TabIndex = 106;
            this.p24_grpMeterItemRotator.TabStop = false;
            this.p24_grpMeterItemRotator.Text = "Rotator";
            this.p24_grpMeterItemRotator.Visible = false;

            // 
            // p24_grpMeterItemSettings
            // 
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_radMeterItemSettings_custom);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_radMeterItemSettings);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_pnlMeterItemSettings);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_pnlVariableInUse_2);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_btnMMIO_variable_2);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_btnMMIO_variable);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_nudMeterItemDecayRate);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_labelTS169);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_nudMeterItemAttackRate);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_labelTS168);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_nudMeterItemUpdateRate);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_labelTS167);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_lblMMBackground);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemHBackground);
            this.p24_grpMeterItemSettings.Controls.Add(this.p24_pnlVariableInUse_1);
            this.p24_grpMeterItemSettings.Location = new System.Drawing.Point(374, 15);
            this.p24_grpMeterItemSettings.Name = "p24_grpMeterItemSettings";
            this.p24_grpMeterItemSettings.Size = new System.Drawing.Size(323, 376);
            this.p24_grpMeterItemSettings.TabIndex = 96;
            this.p24_grpMeterItemSettings.TabStop = false;
            this.p24_grpMeterItemSettings.Text = "Settings";
            this.p24_grpMeterItemSettings.Visible = false;

            // 
            // p24_grpMeterItemSpacerSettings
            // 
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_labelTS199);
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_clrbtnMeterItemHBackgroundSpacerTX);
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_nudMeterItemSpacerPadding);
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_labelTS197);
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_labelTS196);
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_clrbtnMeterItemHBackgroundSpacerRX);
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_chkMeterItemFadeOnTxSpacer);
            this.p24_grpMeterItemSpacerSettings.Controls.Add(this.p24_chkMeterItemFadeOnRxSpacer);
            this.p24_grpMeterItemSpacerSettings.Location = new System.Drawing.Point(12, 14);
            this.p24_grpMeterItemSpacerSettings.Name = "p24_grpMeterItemSpacerSettings";
            this.p24_grpMeterItemSpacerSettings.Size = new System.Drawing.Size(323, 376);
            this.p24_grpMeterItemSpacerSettings.TabIndex = 102;
            this.p24_grpMeterItemSpacerSettings.TabStop = false;
            this.p24_grpMeterItemSpacerSettings.Text = "Spacer";
            this.p24_grpMeterItemSpacerSettings.Visible = false;

            // 
            // p24_grpMeterItemVfoDisplaySettings
            // 
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS278);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMultiMeter_vfo_sync);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS279);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMultiMeter_vfo_lock);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_btnVFOCopyColourFromMainNumbers);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS251);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayFrequency_small);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMultiMeter_vfo_show_bandtext);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_chkMultiMeter_vfo_show_bandtext);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_radMultiMeter_vfo_display_vfob);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_radMultiMeter_vfo_display_vfoa);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_radMultiMeter_vfo_display_both);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS216);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDigitHighlight);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS177);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS176);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayBackground);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS175);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayFrequency);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS174);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayBand);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS173);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayFilter);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS172);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayTx);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS171);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayRx);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS170);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplaySplit);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS163);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplaySplitBack);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_labelTS166);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayMode);
            this.p24_grpMeterItemVfoDisplaySettings.Controls.Add(this.p24_clrbtnMMVfoDisplayTitle);
            this.p24_grpMeterItemVfoDisplaySettings.Location = new System.Drawing.Point(15, 24);
            this.p24_grpMeterItemVfoDisplaySettings.Name = "p24_grpMeterItemVfoDisplaySettings";
            this.p24_grpMeterItemVfoDisplaySettings.Size = new System.Drawing.Size(323, 376);
            this.p24_grpMeterItemVfoDisplaySettings.TabIndex = 101;
            this.p24_grpMeterItemVfoDisplaySettings.TabStop = false;
            this.p24_grpMeterItemVfoDisplaySettings.Text = "VFO Display Settings";
            this.p24_grpMeterItemVfoDisplaySettings.Visible = false;

            // 
            // p24_grpMultiMeterHolder
            // 
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnRecoverContainer);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnContainer_dupe);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnContainer_load);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkContainer_hidewhennotused);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkContainerMinimises);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnContainer_save);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_lblMMContainerNotes);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_radContainer_rx2_data);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_clrbtnContainerBackground);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnContainerDelete);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_lblMMContainerBackground);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_radContainer_rx1_data);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkContainerShowTX);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkMultiMeter_auto_container_height);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_txtContainerNotes);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnMeterCopySettings);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkContainerNoTitle);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnMeterPasteSettings);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_grpMeterItemSettings);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkContainerBorder);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkContainerShowRX);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnMeterUp);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkLockContainer);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnMeterDown);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnRemoveMeterItem);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnAddMeterItem);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_lstMetersInUse);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_lstMetersAvailable);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_comboContainerSelect);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_btnAddRX1Container);
            this.p24_grpMultiMeterHolder.Controls.Add(this.p24_chkContainerHighlight);
            this.p24_grpMultiMeterHolder.Location = new System.Drawing.Point(8, 8);
            this.p24_grpMultiMeterHolder.Name = "p24_grpMultiMeterHolder";
            this.p24_grpMultiMeterHolder.Size = new System.Drawing.Size(710, 395);
            this.p24_grpMultiMeterHolder.TabIndex = 86;
            this.p24_grpMultiMeterHolder.TabStop = false;

            // 
            // p24_grpTextOverlay
            // 
            this.p24_grpTextOverlay.Controls.Add(this.p24_btnTextOverlayVarPicker);
            this.p24_grpTextOverlay.Controls.Add(this.p24_txtTextOverlay_tx_on_led_4char);
            this.p24_grpTextOverlay.Controls.Add(this.p24_chkTextOverlay_tx_on_led);
            this.p24_grpTextOverlay.Controls.Add(this.p24_txtTextOverlay_rx_on_led_4char);
            this.p24_grpTextOverlay.Controls.Add(this.p24_chkTextOverlay_rx_on_led);
            this.p24_grpTextOverlay.Controls.Add(this.p24_btnTextOverlay_copyfonts);
            this.p24_grpTextOverlay.Controls.Add(this.p24_lblTextOverlay_panelbackgroundTX);
            this.p24_grpTextOverlay.Controls.Add(this.p24_clrbtnTextOverlay_PanelBackgroundTX);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS202);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS201);
            this.p24_grpTextOverlay.Controls.Add(this.p24_chkTextOverlay_textback2);
            this.p24_grpTextOverlay.Controls.Add(this.p24_chkTextOverlay_textback1);
            this.p24_grpTextOverlay.Controls.Add(this.p24_clrbtnTextOverlay_TextBackColour2);
            this.p24_grpTextOverlay.Controls.Add(this.p24_clrbtnTextOverlay_TextBackColour1);
            this.p24_grpTextOverlay.Controls.Add(this.p24_btnTextOverlay_copyoffsets);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS207);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS208);
            this.p24_grpTextOverlay.Controls.Add(this.p24_nudTextOverlay_TXyOffset);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS209);
            this.p24_grpTextOverlay.Controls.Add(this.p24_nudTextOverlay_TXxOffset);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS206);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS200);
            this.p24_grpTextOverlay.Controls.Add(this.p24_nudTextOverlay_RXyOffset);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS205);
            this.p24_grpTextOverlay.Controls.Add(this.p24_nudTextOverlay_RXxOffset);
            this.p24_grpTextOverlay.Controls.Add(this.p24_clrbtnTextOverlay_TextColour2);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS204);
            this.p24_grpTextOverlay.Controls.Add(this.p24_btnTextOverlay_Font2);
            this.p24_grpTextOverlay.Controls.Add(this.p24_txtTextOverlay_TXText);
            this.p24_grpTextOverlay.Controls.Add(this.p24_labelTS203);
            this.p24_grpTextOverlay.Controls.Add(this.p24_btnTextOverlay_Font1);
            this.p24_grpTextOverlay.Controls.Add(this.p24_txtTextOverlay_RXText);
            this.p24_grpTextOverlay.Controls.Add(this.p24_chkTextOverlay_ShowPanel);
            this.p24_grpTextOverlay.Controls.Add(this.p24_clrbtnTextOverlay_TextColour1);
            this.p24_grpTextOverlay.Controls.Add(this.p24_nudTextOverlay_PanelPadding);
            this.p24_grpTextOverlay.Controls.Add(this.p24_lblTextOverlay_panelpadding);
            this.p24_grpTextOverlay.Controls.Add(this.p24_lblTextOverlay_panelbackground);
            this.p24_grpTextOverlay.Controls.Add(this.p24_clrbtnTextOverlay_PanelBackground);
            this.p24_grpTextOverlay.Controls.Add(this.p24_chkTextOverlay_FadeOnTX);
            this.p24_grpTextOverlay.Controls.Add(this.p24_chkTextOverlay_FadeOnRX);
            this.p24_grpTextOverlay.Location = new System.Drawing.Point(12, 16);
            this.p24_grpTextOverlay.Name = "p24_grpTextOverlay";
            this.p24_grpTextOverlay.Size = new System.Drawing.Size(323, 376);
            this.p24_grpTextOverlay.TabIndex = 104;
            this.p24_grpTextOverlay.TabStop = false;
            this.p24_grpTextOverlay.Text = "Text Overlay";
            this.p24_grpTextOverlay.Visible = false;

            // 
            // p24_grpWaveRecordItem
            // 
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordScrollHover);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordScrollThumb);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordScrollTrack);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordDelete);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordStop);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordPlay);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordButtonHover);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordButtonBorder);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordButtonFill);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordText);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordRow);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordBorder);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordBack);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordRadius);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_lblWaveRecordHeightRatio);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_btnWaveRecord_reset_layout);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_scroll_hover);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_scroll_thumb);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_scroll_track);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_delete);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_stop);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_play);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_button_hover);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_button_border);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_button_fill);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_text);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_row);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_border);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_clrbtnWaveRecord_back);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_chkWaveRecord_fade_tx);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_chkWaveRecord_fade_rx);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_nudWaveRecord_radius);
            this.p24_grpWaveRecordItem.Controls.Add(this.p24_nudWaveRecord_vertical_ratio);
            this.p24_grpWaveRecordItem.Location = new System.Drawing.Point(26, 24);
            this.p24_grpWaveRecordItem.Name = "p24_grpWaveRecordItem";
            this.p24_grpWaveRecordItem.Size = new System.Drawing.Size(323, 344);
            this.p24_grpWaveRecordItem.TabIndex = 111;
            this.p24_grpWaveRecordItem.TabStop = false;
            this.p24_grpWaveRecordItem.Text = "WaveList Player";
            this.p24_grpWaveRecordItem.Visible = false;

            // 
            // p24_grpWebImage
            // 
            this.p24_grpWebImage.Controls.Add(this.p24_lblWebImage_after);
            this.p24_grpWebImage.Controls.Add(this.p24_scrollableControl1);
            this.p24_grpWebImage.Controls.Add(this.p24_btnWebImage_goto_next);
            this.p24_grpWebImage.Controls.Add(this.p24_txtWebImage_background_4char);
            this.p24_grpWebImage.Controls.Add(this.p24_lblWebImage_secs);
            this.p24_grpWebImage.Controls.Add(this.p24_nudWebImage_background_time);
            this.p24_grpWebImage.Controls.Add(this.p24_chkWebImage_background);
            this.p24_grpWebImage.Controls.Add(this.p24_btnFilter_4char_copy);
            this.p24_grpWebImage.Controls.Add(this.p24_chkWebImage_bypass_cache);
            this.p24_grpWebImage.Controls.Add(this.p24_labelTS347);
            this.p24_grpWebImage.Controls.Add(this.p24_txtWebImage_4char);
            this.p24_grpWebImage.Controls.Add(this.p24_lblWebImage_state);
            this.p24_grpWebImage.Controls.Add(this.p24_labelTS236);
            this.p24_grpWebImage.Controls.Add(this.p24_txtWebImage_url);
            this.p24_grpWebImage.Controls.Add(this.p24_nudWebImage_update_interval);
            this.p24_grpWebImage.Controls.Add(this.p24_labelTS232);
            this.p24_grpWebImage.Controls.Add(this.p24_nudWebImage_width_scale);
            this.p24_grpWebImage.Controls.Add(this.p24_labelTS235);
            this.p24_grpWebImage.Controls.Add(this.p24_chkWebImage_fade_tx);
            this.p24_grpWebImage.Controls.Add(this.p24_chkWebImage_fade_rx);
            this.p24_grpWebImage.Location = new System.Drawing.Point(12, 16);
            this.p24_grpWebImage.Name = "p24_grpWebImage";
            this.p24_grpWebImage.Size = new System.Drawing.Size(323, 376);
            this.p24_grpWebImage.TabIndex = 108;
            this.p24_grpWebImage.TabStop = false;
            this.p24_grpWebImage.Text = "Web Image";
            this.p24_grpWebImage.Visible = false;

            // 
            // p24_label22
            // 
            this.p24_label22.AutoSize = true;
            this.p24_label22.Location = new System.Drawing.Point(125, 8);
            this.p24_label22.Name = "p24_label22";
            this.p24_label22.Size = new System.Drawing.Size(88, 13);
            this.p24_label22.TabIndex = 165;
            this.p24_label22.Text = "Waterfall Palette:";
            this.p24_label22.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.p24_label22, "Color scheme");

            // 
            // p24_labelTS162
            // 
            this.p24_labelTS162.AutoSize = true;
            this.p24_labelTS162.Image = null;
            this.p24_labelTS162.Location = new System.Drawing.Point(66, 123);
            this.p24_labelTS162.Name = "p24_labelTS162";
            this.p24_labelTS162.Size = new System.Drawing.Size(33, 13);
            this.p24_labelTS162.TabIndex = 112;
            this.p24_labelTS162.Text = "Time:";

            // 
            // p24_labelTS163
            // 
            this.p24_labelTS163.AutoSize = true;
            this.p24_labelTS163.Image = null;
            this.p24_labelTS163.Location = new System.Drawing.Point(44, 147);
            this.p24_labelTS163.Name = "p24_labelTS163";
            this.p24_labelTS163.Size = new System.Drawing.Size(58, 13);
            this.p24_labelTS163.TabIndex = 114;
            this.p24_labelTS163.Text = "Split Back:";

            // 
            // p24_labelTS164
            // 
            this.p24_labelTS164.AutoSize = true;
            this.p24_labelTS164.Image = null;
            this.p24_labelTS164.Location = new System.Drawing.Point(66, 147);
            this.p24_labelTS164.Name = "p24_labelTS164";
            this.p24_labelTS164.Size = new System.Drawing.Size(33, 13);
            this.p24_labelTS164.TabIndex = 114;
            this.p24_labelTS164.Text = "Date:";

            // 
            // p24_labelTS166
            // 
            this.p24_labelTS166.AutoSize = true;
            this.p24_labelTS166.Image = null;
            this.p24_labelTS166.Location = new System.Drawing.Point(66, 123);
            this.p24_labelTS166.Name = "p24_labelTS166";
            this.p24_labelTS166.Size = new System.Drawing.Size(37, 13);
            this.p24_labelTS166.TabIndex = 112;
            this.p24_labelTS166.Text = "Mode:";

            // 
            // p24_labelTS167
            // 
            this.p24_labelTS167.Image = null;
            this.p24_labelTS167.Location = new System.Drawing.Point(6, 20);
            this.p24_labelTS167.Name = "p24_labelTS167";
            this.p24_labelTS167.Size = new System.Drawing.Size(71, 16);
            this.p24_labelTS167.TabIndex = 100;
            this.p24_labelTS167.Text = "Update (ms):";
            this.p24_labelTS167.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS168
            // 
            this.p24_labelTS168.Image = null;
            this.p24_labelTS168.Location = new System.Drawing.Point(34, 47);
            this.p24_labelTS168.Name = "p24_labelTS168";
            this.p24_labelTS168.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS168.TabIndex = 102;
            this.p24_labelTS168.Text = "Attack:";
            this.p24_labelTS168.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS169
            // 
            this.p24_labelTS169.Image = null;
            this.p24_labelTS169.Location = new System.Drawing.Point(27, 71);
            this.p24_labelTS169.Name = "p24_labelTS169";
            this.p24_labelTS169.Size = new System.Drawing.Size(50, 16);
            this.p24_labelTS169.TabIndex = 104;
            this.p24_labelTS169.Text = "Decay:";
            this.p24_labelTS169.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS170
            // 
            this.p24_labelTS170.AutoSize = true;
            this.p24_labelTS170.Image = null;
            this.p24_labelTS170.Location = new System.Drawing.Point(169, 147);
            this.p24_labelTS170.Name = "p24_labelTS170";
            this.p24_labelTS170.Size = new System.Drawing.Size(30, 13);
            this.p24_labelTS170.TabIndex = 116;
            this.p24_labelTS170.Text = "Split:";

            // 
            // p24_labelTS171
            // 
            this.p24_labelTS171.AutoSize = true;
            this.p24_labelTS171.Image = null;
            this.p24_labelTS171.Location = new System.Drawing.Point(73, 176);
            this.p24_labelTS171.Name = "p24_labelTS171";
            this.p24_labelTS171.Size = new System.Drawing.Size(25, 13);
            this.p24_labelTS171.TabIndex = 118;
            this.p24_labelTS171.Text = "RX:";

            // 
            // p24_labelTS172
            // 
            this.p24_labelTS172.AutoSize = true;
            this.p24_labelTS172.Image = null;
            this.p24_labelTS172.Location = new System.Drawing.Point(73, 205);
            this.p24_labelTS172.Name = "p24_labelTS172";
            this.p24_labelTS172.Size = new System.Drawing.Size(24, 13);
            this.p24_labelTS172.TabIndex = 120;
            this.p24_labelTS172.Text = "TX:";

            // 
            // p24_labelTS173
            // 
            this.p24_labelTS173.AutoSize = true;
            this.p24_labelTS173.Image = null;
            this.p24_labelTS173.Location = new System.Drawing.Point(69, 235);
            this.p24_labelTS173.Name = "p24_labelTS173";
            this.p24_labelTS173.Size = new System.Drawing.Size(32, 13);
            this.p24_labelTS173.TabIndex = 122;
            this.p24_labelTS173.Text = "Filter:";

            // 
            // p24_labelTS174
            // 
            this.p24_labelTS174.AutoSize = true;
            this.p24_labelTS174.Image = null;
            this.p24_labelTS174.Location = new System.Drawing.Point(69, 264);
            this.p24_labelTS174.Name = "p24_labelTS174";
            this.p24_labelTS174.Size = new System.Drawing.Size(35, 13);
            this.p24_labelTS174.TabIndex = 124;
            this.p24_labelTS174.Text = "Band:";

            // 
            // p24_labelTS175
            // 
            this.p24_labelTS175.AutoSize = true;
            this.p24_labelTS175.Image = null;
            this.p24_labelTS175.Location = new System.Drawing.Point(40, 93);
            this.p24_labelTS175.Name = "p24_labelTS175";
            this.p24_labelTS175.Size = new System.Drawing.Size(60, 13);
            this.p24_labelTS175.TabIndex = 126;
            this.p24_labelTS175.Text = "Frequency:";

            // 
            // p24_labelTS176
            // 
            this.p24_labelTS176.AutoSize = true;
            this.p24_labelTS176.Image = null;
            this.p24_labelTS176.Location = new System.Drawing.Point(31, 35);
            this.p24_labelTS176.Name = "p24_labelTS176";
            this.p24_labelTS176.Size = new System.Drawing.Size(68, 13);
            this.p24_labelTS176.TabIndex = 128;
            this.p24_labelTS176.Text = "Background:";

            // 
            // p24_labelTS177
            // 
            this.p24_labelTS177.AutoSize = true;
            this.p24_labelTS177.Image = null;
            this.p24_labelTS177.Location = new System.Drawing.Point(65, 64);
            this.p24_labelTS177.Name = "p24_labelTS177";
            this.p24_labelTS177.Size = new System.Drawing.Size(32, 13);
            this.p24_labelTS177.TabIndex = 129;
            this.p24_labelTS177.Text = "Titles";

            // 
            // p24_labelTS196
            // 
            this.p24_labelTS196.AutoSize = true;
            this.p24_labelTS196.Image = null;
            this.p24_labelTS196.Location = new System.Drawing.Point(14, 34);
            this.p24_labelTS196.Name = "p24_labelTS196";
            this.p24_labelTS196.Size = new System.Drawing.Size(86, 13);
            this.p24_labelTS196.TabIndex = 130;
            this.p24_labelTS196.Text = "Background RX:";

            // 
            // p24_labelTS197
            // 
            this.p24_labelTS197.AutoSize = true;
            this.p24_labelTS197.Image = null;
            this.p24_labelTS197.Location = new System.Drawing.Point(10, 140);
            this.p24_labelTS197.Name = "p24_labelTS197";
            this.p24_labelTS197.Size = new System.Drawing.Size(87, 13);
            this.p24_labelTS197.TabIndex = 131;
            this.p24_labelTS197.Text = "Vertical Padding:";

            // 
            // p24_labelTS199
            // 
            this.p24_labelTS199.AutoSize = true;
            this.p24_labelTS199.Image = null;
            this.p24_labelTS199.Location = new System.Drawing.Point(14, 63);
            this.p24_labelTS199.Name = "p24_labelTS199";
            this.p24_labelTS199.Size = new System.Drawing.Size(85, 13);
            this.p24_labelTS199.TabIndex = 134;
            this.p24_labelTS199.Text = "Background TX:";

            // 
            // p24_labelTS200
            // 
            this.p24_labelTS200.AutoSize = true;
            this.p24_labelTS200.Image = null;
            this.p24_labelTS200.Location = new System.Drawing.Point(159, 272);
            this.p24_labelTS200.Name = "p24_labelTS200";
            this.p24_labelTS200.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS200.TabIndex = 148;
            this.p24_labelTS200.Text = "x";

            // 
            // p24_labelTS201
            // 
            this.p24_labelTS201.AutoSize = true;
            this.p24_labelTS201.Image = null;
            this.p24_labelTS201.Location = new System.Drawing.Point(5, 188);
            this.p24_labelTS201.Name = "p24_labelTS201";
            this.p24_labelTS201.Size = new System.Drawing.Size(25, 13);
            this.p24_labelTS201.TabIndex = 160;
            this.p24_labelTS201.Text = "RX:";

            // 
            // p24_labelTS202
            // 
            this.p24_labelTS202.AutoSize = true;
            this.p24_labelTS202.Image = null;
            this.p24_labelTS202.Location = new System.Drawing.Point(5, 214);
            this.p24_labelTS202.Name = "p24_labelTS202";
            this.p24_labelTS202.Size = new System.Drawing.Size(24, 13);
            this.p24_labelTS202.TabIndex = 161;
            this.p24_labelTS202.Text = "TX:";

            // 
            // p24_labelTS203
            // 
            this.p24_labelTS203.AutoSize = true;
            this.p24_labelTS203.Image = null;
            this.p24_labelTS203.Location = new System.Drawing.Point(5, 133);
            this.p24_labelTS203.Name = "p24_labelTS203";
            this.p24_labelTS203.Size = new System.Drawing.Size(49, 13);
            this.p24_labelTS203.TabIndex = 138;
            this.p24_labelTS203.Text = "RX Text:";

            // 
            // p24_labelTS204
            // 
            this.p24_labelTS204.AutoSize = true;
            this.p24_labelTS204.Image = null;
            this.p24_labelTS204.Location = new System.Drawing.Point(5, 159);
            this.p24_labelTS204.Name = "p24_labelTS204";
            this.p24_labelTS204.Size = new System.Drawing.Size(48, 13);
            this.p24_labelTS204.TabIndex = 141;
            this.p24_labelTS204.Text = "TX Text:";

            // 
            // p24_labelTS205
            // 
            this.p24_labelTS205.AutoSize = true;
            this.p24_labelTS205.Image = null;
            this.p24_labelTS205.Location = new System.Drawing.Point(6, 272);
            this.p24_labelTS205.Name = "p24_labelTS205";
            this.p24_labelTS205.Size = new System.Drawing.Size(85, 13);
            this.p24_labelTS205.TabIndex = 145;
            this.p24_labelTS205.Text = "RX Text Offsets:";

            // 
            // p24_labelTS206
            // 
            this.p24_labelTS206.AutoSize = true;
            this.p24_labelTS206.Image = null;
            this.p24_labelTS206.Location = new System.Drawing.Point(159, 298);
            this.p24_labelTS206.Name = "p24_labelTS206";
            this.p24_labelTS206.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS206.TabIndex = 149;
            this.p24_labelTS206.Text = "y";

            // 
            // p24_labelTS207
            // 
            this.p24_labelTS207.AutoSize = true;
            this.p24_labelTS207.Image = null;
            this.p24_labelTS207.Location = new System.Drawing.Point(159, 350);
            this.p24_labelTS207.Name = "p24_labelTS207";
            this.p24_labelTS207.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS207.TabIndex = 154;
            this.p24_labelTS207.Text = "y";

            // 
            // p24_labelTS208
            // 
            this.p24_labelTS208.AutoSize = true;
            this.p24_labelTS208.Image = null;
            this.p24_labelTS208.Location = new System.Drawing.Point(159, 324);
            this.p24_labelTS208.Name = "p24_labelTS208";
            this.p24_labelTS208.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS208.TabIndex = 153;
            this.p24_labelTS208.Text = "x";

            // 
            // p24_labelTS209
            // 
            this.p24_labelTS209.AutoSize = true;
            this.p24_labelTS209.Image = null;
            this.p24_labelTS209.Location = new System.Drawing.Point(5, 324);
            this.p24_labelTS209.Name = "p24_labelTS209";
            this.p24_labelTS209.Size = new System.Drawing.Size(84, 13);
            this.p24_labelTS209.TabIndex = 151;
            this.p24_labelTS209.Text = "TX Text Offsets:";

            // 
            // p24_labelTS210
            // 
            this.p24_labelTS210.AutoSize = true;
            this.p24_labelTS210.Image = null;
            this.p24_labelTS210.Location = new System.Drawing.Point(35, 51);
            this.p24_labelTS210.Name = "p24_labelTS210";
            this.p24_labelTS210.Size = new System.Drawing.Size(52, 13);
            this.p24_labelTS210.TabIndex = 137;
            this.p24_labelTS210.Text = "4Char ID:";

            // 
            // p24_labelTS212
            // 
            this.p24_labelTS212.AutoSize = true;
            this.p24_labelTS212.Image = null;
            this.p24_labelTS212.Location = new System.Drawing.Point(46, 199);
            this.p24_labelTS212.Name = "p24_labelTS212";
            this.p24_labelTS212.Size = new System.Drawing.Size(31, 13);
            this.p24_labelTS212.TabIndex = 133;
            this.p24_labelTS212.Text = "Text:";

            // 
            // p24_labelTS215
            // 
            this.p24_labelTS215.AutoSize = true;
            this.p24_labelTS215.Image = null;
            this.p24_labelTS215.Location = new System.Drawing.Point(14, 25);
            this.p24_labelTS215.Name = "p24_labelTS215";
            this.p24_labelTS215.Size = new System.Drawing.Size(73, 13);
            this.p24_labelTS215.TabIndex = 131;
            this.p24_labelTS215.Text = "Send Interval:";

            // 
            // p24_labelTS216
            // 
            this.p24_labelTS216.AutoSize = true;
            this.p24_labelTS216.Image = null;
            this.p24_labelTS216.Location = new System.Drawing.Point(9, 293);
            this.p24_labelTS216.Name = "p24_labelTS216";
            this.p24_labelTS216.Size = new System.Drawing.Size(67, 13);
            this.p24_labelTS216.TabIndex = 131;
            this.p24_labelTS216.Text = "VfoHighlight:";

            // 
            // p24_labelTS217
            // 
            this.p24_labelTS217.AutoSize = true;
            this.p24_labelTS217.Image = null;
            this.p24_labelTS217.Location = new System.Drawing.Point(155, 24);
            this.p24_labelTS217.Name = "p24_labelTS217";
            this.p24_labelTS217.Size = new System.Drawing.Size(20, 13);
            this.p24_labelTS217.TabIndex = 135;
            this.p24_labelTS217.Text = "ms";

            // 
            // p24_labelTS218
            // 
            this.p24_labelTS218.AutoSize = true;
            this.p24_labelTS218.Image = null;
            this.p24_labelTS218.Location = new System.Drawing.Point(9, 144);
            this.p24_labelTS218.Name = "p24_labelTS218";
            this.p24_labelTS218.Size = new System.Drawing.Size(68, 13);
            this.p24_labelTS218.TabIndex = 121;
            this.p24_labelTS218.Text = "Beam Width:";

            // 
            // p24_labelTS219
            // 
            this.p24_labelTS219.AutoSize = true;
            this.p24_labelTS219.Image = null;
            this.p24_labelTS219.Location = new System.Drawing.Point(24, 193);
            this.p24_labelTS219.Name = "p24_labelTS219";
            this.p24_labelTS219.Size = new System.Drawing.Size(35, 13);
            this.p24_labelTS219.TabIndex = 161;
            this.p24_labelTS219.Text = "False:";

            // 
            // p24_labelTS220
            // 
            this.p24_labelTS220.AutoSize = true;
            this.p24_labelTS220.Image = null;
            this.p24_labelTS220.Location = new System.Drawing.Point(27, 166);
            this.p24_labelTS220.Name = "p24_labelTS220";
            this.p24_labelTS220.Size = new System.Drawing.Size(32, 13);
            this.p24_labelTS220.TabIndex = 160;
            this.p24_labelTS220.Text = "True:";

            // 
            // p24_labelTS221
            // 
            this.p24_labelTS221.AutoSize = true;
            this.p24_labelTS221.Image = null;
            this.p24_labelTS221.Location = new System.Drawing.Point(161, 330);
            this.p24_labelTS221.Name = "p24_labelTS221";
            this.p24_labelTS221.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS221.TabIndex = 154;
            this.p24_labelTS221.Text = "y";

            // 
            // p24_labelTS222
            // 
            this.p24_labelTS222.AutoSize = true;
            this.p24_labelTS222.Image = null;
            this.p24_labelTS222.Location = new System.Drawing.Point(161, 304);
            this.p24_labelTS222.Name = "p24_labelTS222";
            this.p24_labelTS222.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS222.TabIndex = 153;
            this.p24_labelTS222.Text = "x";

            // 
            // p24_labelTS223
            // 
            this.p24_labelTS223.AutoSize = true;
            this.p24_labelTS223.Image = null;
            this.p24_labelTS223.Location = new System.Drawing.Point(62, 304);
            this.p24_labelTS223.Name = "p24_labelTS223";
            this.p24_labelTS223.Size = new System.Drawing.Size(30, 13);
            this.p24_labelTS223.TabIndex = 151;
            this.p24_labelTS223.Text = "Size:";

            // 
            // p24_labelTS224
            // 
            this.p24_labelTS224.AutoSize = true;
            this.p24_labelTS224.Image = null;
            this.p24_labelTS224.Location = new System.Drawing.Point(161, 278);
            this.p24_labelTS224.Name = "p24_labelTS224";
            this.p24_labelTS224.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS224.TabIndex = 149;
            this.p24_labelTS224.Text = "y";

            // 
            // p24_labelTS225
            // 
            this.p24_labelTS225.Image = null;
            this.p24_labelTS225.Location = new System.Drawing.Point(6, 20);
            this.p24_labelTS225.Name = "p24_labelTS225";
            this.p24_labelTS225.Size = new System.Drawing.Size(71, 16);
            this.p24_labelTS225.TabIndex = 100;
            this.p24_labelTS225.Text = "Update (ms):";
            this.p24_labelTS225.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS226
            // 
            this.p24_labelTS226.AutoSize = true;
            this.p24_labelTS226.Image = null;
            this.p24_labelTS226.Location = new System.Drawing.Point(161, 252);
            this.p24_labelTS226.Name = "p24_labelTS226";
            this.p24_labelTS226.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS226.TabIndex = 148;
            this.p24_labelTS226.Text = "x";

            // 
            // p24_labelTS227
            // 
            this.p24_labelTS227.AutoSize = true;
            this.p24_labelTS227.Image = null;
            this.p24_labelTS227.Location = new System.Drawing.Point(184, 22);
            this.p24_labelTS227.Name = "p24_labelTS227";
            this.p24_labelTS227.Size = new System.Drawing.Size(68, 13);
            this.p24_labelTS227.TabIndex = 93;
            this.p24_labelTS227.Text = "Background:";

            // 
            // p24_labelTS228
            // 
            this.p24_labelTS228.AutoSize = true;
            this.p24_labelTS228.Image = null;
            this.p24_labelTS228.Location = new System.Drawing.Point(22, 115);
            this.p24_labelTS228.Name = "p24_labelTS228";
            this.p24_labelTS228.Size = new System.Drawing.Size(55, 13);
            this.p24_labelTS228.TabIndex = 81;
            this.p24_labelTS228.Text = "Small Dot:";

            // 
            // p24_labelTS229
            // 
            this.p24_labelTS229.AutoSize = true;
            this.p24_labelTS229.Image = null;
            this.p24_labelTS229.Location = new System.Drawing.Point(20, 86);
            this.p24_labelTS229.Name = "p24_labelTS229";
            this.p24_labelTS229.Size = new System.Drawing.Size(57, 13);
            this.p24_labelTS229.TabIndex = 79;
            this.p24_labelTS229.Text = "Large Dot:";

            // 
            // p24_labelTS230
            // 
            this.p24_labelTS230.AutoSize = true;
            this.p24_labelTS230.Image = null;
            this.p24_labelTS230.Location = new System.Drawing.Point(40, 57);
            this.p24_labelTS230.Name = "p24_labelTS230";
            this.p24_labelTS230.Size = new System.Drawing.Size(37, 13);
            this.p24_labelTS230.TabIndex = 78;
            this.p24_labelTS230.Text = "Arrow:";
            this.p24_labelTS230.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS231
            // 
            this.p24_labelTS231.AutoSize = true;
            this.p24_labelTS231.Image = null;
            this.p24_labelTS231.Location = new System.Drawing.Point(10, 252);
            this.p24_labelTS231.Name = "p24_labelTS231";
            this.p24_labelTS231.Size = new System.Drawing.Size(83, 13);
            this.p24_labelTS231.TabIndex = 145;
            this.p24_labelTS231.Text = "Position Offsets:";

            // 
            // p24_labelTS232
            // 
            this.p24_labelTS232.Image = null;
            this.p24_labelTS232.Location = new System.Drawing.Point(18, 54);
            this.p24_labelTS232.Name = "p24_labelTS232";
            this.p24_labelTS232.Size = new System.Drawing.Size(71, 16);
            this.p24_labelTS232.TabIndex = 133;
            this.p24_labelTS232.Text = "Update (s):";
            this.p24_labelTS232.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS233
            // 
            this.p24_labelTS233.AutoSize = true;
            this.p24_labelTS233.Image = null;
            this.p24_labelTS233.Location = new System.Drawing.Point(5, 133);
            this.p24_labelTS233.Name = "p24_labelTS233";
            this.p24_labelTS233.Size = new System.Drawing.Size(54, 13);
            this.p24_labelTS233.TabIndex = 138;
            this.p24_labelTS233.Text = "Condition:";

            // 
            // p24_labelTS234
            // 
            this.p24_labelTS234.AutoSize = true;
            this.p24_labelTS234.Image = null;
            this.p24_labelTS234.Location = new System.Drawing.Point(39, 102);
            this.p24_labelTS234.Name = "p24_labelTS234";
            this.p24_labelTS234.Size = new System.Drawing.Size(79, 13);
            this.p24_labelTS234.TabIndex = 131;
            this.p24_labelTS234.Text = "Panel Padding:";

            // 
            // p24_labelTS235
            // 
            this.p24_labelTS235.AutoSize = true;
            this.p24_labelTS235.Image = null;
            this.p24_labelTS235.Location = new System.Drawing.Point(21, 30);
            this.p24_labelTS235.Name = "p24_labelTS235";
            this.p24_labelTS235.Size = new System.Drawing.Size(68, 13);
            this.p24_labelTS235.TabIndex = 131;
            this.p24_labelTS235.Text = "Width Scale:";

            // 
            // p24_labelTS236
            // 
            this.p24_labelTS236.AutoSize = true;
            this.p24_labelTS236.Image = null;
            this.p24_labelTS236.Location = new System.Drawing.Point(16, 120);
            this.p24_labelTS236.Name = "p24_labelTS236";
            this.p24_labelTS236.Size = new System.Drawing.Size(23, 13);
            this.p24_labelTS236.TabIndex = 140;
            this.p24_labelTS236.Text = "Url:";

            // 
            // p24_labelTS237
            // 
            this.p24_labelTS237.AutoSize = true;
            this.p24_labelTS237.Image = null;
            this.p24_labelTS237.Location = new System.Drawing.Point(5, 352);
            this.p24_labelTS237.Name = "p24_labelTS237";
            this.p24_labelTS237.Size = new System.Drawing.Size(39, 13);
            this.p24_labelTS237.TabIndex = 174;
            this.p24_labelTS237.Text = "STOP:";

            // 
            // p24_labelTS238
            // 
            this.p24_labelTS238.AutoSize = true;
            this.p24_labelTS238.Image = null;
            this.p24_labelTS238.Location = new System.Drawing.Point(31, 47);
            this.p24_labelTS238.Name = "p24_labelTS238";
            this.p24_labelTS238.Size = new System.Drawing.Size(41, 13);
            this.p24_labelTS238.TabIndex = 138;
            this.p24_labelTS238.Text = "Border:";
            this.p24_labelTS238.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS239
            // 
            this.p24_labelTS239.AutoSize = true;
            this.p24_labelTS239.Image = null;
            this.p24_labelTS239.Location = new System.Drawing.Point(31, 73);
            this.p24_labelTS239.Name = "p24_labelTS239";
            this.p24_labelTS239.Size = new System.Drawing.Size(42, 13);
            this.p24_labelTS239.TabIndex = 140;
            this.p24_labelTS239.Text = "Margin:";
            this.p24_labelTS239.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS240
            // 
            this.p24_labelTS240.AutoSize = true;
            this.p24_labelTS240.Image = null;
            this.p24_labelTS240.Location = new System.Drawing.Point(31, 99);
            this.p24_labelTS240.Name = "p24_labelTS240";
            this.p24_labelTS240.Size = new System.Drawing.Size(43, 13);
            this.p24_labelTS240.TabIndex = 142;
            this.p24_labelTS240.Text = "Radius:";
            this.p24_labelTS240.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS241
            // 
            this.p24_labelTS241.AutoSize = true;
            this.p24_labelTS241.Image = null;
            this.p24_labelTS241.Location = new System.Drawing.Point(3, 125);
            this.p24_labelTS241.Name = "p24_labelTS241";
            this.p24_labelTS241.Size = new System.Drawing.Size(69, 13);
            this.p24_labelTS241.TabIndex = 144;
            this.p24_labelTS241.Text = "Height Ratio:";
            this.p24_labelTS241.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS242
            // 
            this.p24_labelTS242.AutoSize = true;
            this.p24_labelTS242.Image = null;
            this.p24_labelTS242.Location = new System.Drawing.Point(14, 207);
            this.p24_labelTS242.Name = "p24_labelTS242";
            this.p24_labelTS242.Size = new System.Drawing.Size(40, 13);
            this.p24_labelTS242.TabIndex = 148;
            this.p24_labelTS242.Text = "Active:";
            this.p24_labelTS242.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS243
            // 
            this.p24_labelTS243.AutoSize = true;
            this.p24_labelTS243.Image = null;
            this.p24_labelTS243.Location = new System.Drawing.Point(6, 236);
            this.p24_labelTS243.Name = "p24_labelTS243";
            this.p24_labelTS243.Size = new System.Drawing.Size(48, 13);
            this.p24_labelTS243.TabIndex = 150;
            this.p24_labelTS243.Text = "Inactive:";
            this.p24_labelTS243.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS244
            // 
            this.p24_labelTS244.AutoSize = true;
            this.p24_labelTS244.Image = null;
            this.p24_labelTS244.Location = new System.Drawing.Point(13, 265);
            this.p24_labelTS244.Name = "p24_labelTS244";
            this.p24_labelTS244.Size = new System.Drawing.Size(41, 13);
            this.p24_labelTS244.TabIndex = 152;
            this.p24_labelTS244.Text = "Border:";
            this.p24_labelTS244.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS245
            // 
            this.p24_labelTS245.AutoSize = true;
            this.p24_labelTS245.Image = null;
            this.p24_labelTS245.Location = new System.Drawing.Point(32, 294);
            this.p24_labelTS245.Name = "p24_labelTS245";
            this.p24_labelTS245.Size = new System.Drawing.Size(22, 13);
            this.p24_labelTS245.TabIndex = 154;
            this.p24_labelTS245.Text = "Fill:";
            this.p24_labelTS245.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS246
            // 
            this.p24_labelTS246.AutoSize = true;
            this.p24_labelTS246.Image = null;
            this.p24_labelTS246.Location = new System.Drawing.Point(15, 323);
            this.p24_labelTS246.Name = "p24_labelTS246";
            this.p24_labelTS246.Size = new System.Drawing.Size(39, 13);
            this.p24_labelTS246.TabIndex = 156;
            this.p24_labelTS246.Text = "Hover:";
            this.p24_labelTS246.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS247
            // 
            this.p24_labelTS247.AutoSize = true;
            this.p24_labelTS247.Image = null;
            this.p24_labelTS247.Location = new System.Drawing.Point(194, 147);
            this.p24_labelTS247.Name = "p24_labelTS247";
            this.p24_labelTS247.Size = new System.Drawing.Size(41, 13);
            this.p24_labelTS247.TabIndex = 162;
            this.p24_labelTS247.Text = "Shift Y:";
            this.p24_labelTS247.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS248
            // 
            this.p24_labelTS248.AutoSize = true;
            this.p24_labelTS248.Image = null;
            this.p24_labelTS248.Location = new System.Drawing.Point(194, 92);
            this.p24_labelTS248.Name = "p24_labelTS248";
            this.p24_labelTS248.Size = new System.Drawing.Size(37, 13);
            this.p24_labelTS248.TabIndex = 160;
            this.p24_labelTS248.Text = "Scale:";
            this.p24_labelTS248.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS249
            // 
            this.p24_labelTS249.AutoSize = true;
            this.p24_labelTS249.Image = null;
            this.p24_labelTS249.Location = new System.Drawing.Point(22, 21);
            this.p24_labelTS249.Name = "p24_labelTS249";
            this.p24_labelTS249.Size = new System.Drawing.Size(50, 13);
            this.p24_labelTS249.TabIndex = 131;
            this.p24_labelTS249.Text = "Columns:";
            this.p24_labelTS249.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS250
            // 
            this.p24_labelTS250.AutoSize = true;
            this.p24_labelTS250.Image = null;
            this.p24_labelTS250.Location = new System.Drawing.Point(194, 118);
            this.p24_labelTS250.Name = "p24_labelTS250";
            this.p24_labelTS250.Size = new System.Drawing.Size(41, 13);
            this.p24_labelTS250.TabIndex = 164;
            this.p24_labelTS250.Text = "Shift X:";
            this.p24_labelTS250.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS251
            // 
            this.p24_labelTS251.AutoSize = true;
            this.p24_labelTS251.Image = null;
            this.p24_labelTS251.Location = new System.Drawing.Point(170, 93);
            this.p24_labelTS251.Name = "p24_labelTS251";
            this.p24_labelTS251.Size = new System.Drawing.Size(52, 13);
            this.p24_labelTS251.TabIndex = 138;
            this.p24_labelTS251.Text = "Small #\'s:";

            // 
            // p24_labelTS252
            // 
            this.p24_labelTS252.Image = null;
            this.p24_labelTS252.Location = new System.Drawing.Point(41, 52);
            this.p24_labelTS252.Name = "p24_labelTS252";
            this.p24_labelTS252.Size = new System.Drawing.Size(71, 16);
            this.p24_labelTS252.TabIndex = 133;
            this.p24_labelTS252.Text = "Update (ms):";
            this.p24_labelTS252.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS253
            // 
            this.p24_labelTS253.AutoSize = true;
            this.p24_labelTS253.Image = null;
            this.p24_labelTS253.Location = new System.Drawing.Point(45, 28);
            this.p24_labelTS253.Name = "p24_labelTS253";
            this.p24_labelTS253.Size = new System.Drawing.Size(73, 13);
            this.p24_labelTS253.TabIndex = 131;
            this.p24_labelTS253.Text = "Vertical Ratio:";

            // 
            // p24_labelTS258
            // 
            this.p24_labelTS258.AutoSize = true;
            this.p24_labelTS258.Image = null;
            this.p24_labelTS258.Location = new System.Drawing.Point(198, 28);
            this.p24_labelTS258.Name = "p24_labelTS258";
            this.p24_labelTS258.Size = new System.Drawing.Size(68, 13);
            this.p24_labelTS258.TabIndex = 130;
            this.p24_labelTS258.Text = "Background:";

            // 
            // p24_labelTS259
            // 
            this.p24_labelTS259.Image = null;
            this.p24_labelTS259.Location = new System.Drawing.Point(18, 78);
            this.p24_labelTS259.Name = "p24_labelTS259";
            this.p24_labelTS259.Size = new System.Drawing.Size(94, 16);
            this.p24_labelTS259.TabIndex = 135;
            this.p24_labelTS259.Text = "Keep For (secs) :";
            this.p24_labelTS259.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS260
            // 
            this.p24_labelTS260.Image = null;
            this.p24_labelTS260.Location = new System.Drawing.Point(9, 23);
            this.p24_labelTS260.Name = "p24_labelTS260";
            this.p24_labelTS260.Size = new System.Drawing.Size(58, 16);
            this.p24_labelTS260.TabIndex = 138;
            this.p24_labelTS260.Text = "Reading:";
            this.p24_labelTS260.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS261
            // 
            this.p24_labelTS261.Image = null;
            this.p24_labelTS261.Location = new System.Drawing.Point(24, 73);
            this.p24_labelTS261.Name = "p24_labelTS261";
            this.p24_labelTS261.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS261.TabIndex = 140;
            this.p24_labelTS261.Text = "Min:";
            this.p24_labelTS261.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS262
            // 
            this.p24_labelTS262.Image = null;
            this.p24_labelTS262.Location = new System.Drawing.Point(138, 73);
            this.p24_labelTS262.Name = "p24_labelTS262";
            this.p24_labelTS262.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS262.TabIndex = 142;
            this.p24_labelTS262.Text = "Max:";
            this.p24_labelTS262.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS263
            // 
            this.p24_labelTS263.Image = null;
            this.p24_labelTS263.Location = new System.Drawing.Point(138, 73);
            this.p24_labelTS263.Name = "p24_labelTS263";
            this.p24_labelTS263.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS263.TabIndex = 142;
            this.p24_labelTS263.Text = "Max:";
            this.p24_labelTS263.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS264
            // 
            this.p24_labelTS264.Image = null;
            this.p24_labelTS264.Location = new System.Drawing.Point(24, 73);
            this.p24_labelTS264.Name = "p24_labelTS264";
            this.p24_labelTS264.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS264.TabIndex = 140;
            this.p24_labelTS264.Text = "Min:";
            this.p24_labelTS264.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS265
            // 
            this.p24_labelTS265.Image = null;
            this.p24_labelTS265.Location = new System.Drawing.Point(9, 23);
            this.p24_labelTS265.Name = "p24_labelTS265";
            this.p24_labelTS265.Size = new System.Drawing.Size(58, 16);
            this.p24_labelTS265.TabIndex = 138;
            this.p24_labelTS265.Text = "Reading:";
            this.p24_labelTS265.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS278
            // 
            this.p24_labelTS278.AutoSize = true;
            this.p24_labelTS278.Image = null;
            this.p24_labelTS278.Location = new System.Drawing.Point(163, 287);
            this.p24_labelTS278.Name = "p24_labelTS278";
            this.p24_labelTS278.Size = new System.Drawing.Size(34, 13);
            this.p24_labelTS278.TabIndex = 143;
            this.p24_labelTS278.Text = "Sync:";

            // 
            // p24_labelTS279
            // 
            this.p24_labelTS279.AutoSize = true;
            this.p24_labelTS279.Image = null;
            this.p24_labelTS279.Location = new System.Drawing.Point(163, 258);
            this.p24_labelTS279.Name = "p24_labelTS279";
            this.p24_labelTS279.Size = new System.Drawing.Size(34, 13);
            this.p24_labelTS279.TabIndex = 141;
            this.p24_labelTS279.Text = "Lock:";

            // 
            // p24_labelTS280
            // 
            this.p24_labelTS280.Image = null;
            this.p24_labelTS280.Location = new System.Drawing.Point(8, 113);
            this.p24_labelTS280.Name = "p24_labelTS280";
            this.p24_labelTS280.Size = new System.Drawing.Size(58, 16);
            this.p24_labelTS280.TabIndex = 147;
            this.p24_labelTS280.Text = "Lines:";
            this.p24_labelTS280.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS281
            // 
            this.p24_labelTS281.Image = null;
            this.p24_labelTS281.Location = new System.Drawing.Point(118, 113);
            this.p24_labelTS281.Name = "p24_labelTS281";
            this.p24_labelTS281.Size = new System.Drawing.Size(46, 16);
            this.p24_labelTS281.TabIndex = 148;
            this.p24_labelTS281.Text = "Time:";
            this.p24_labelTS281.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS287
            // 
            this.p24_labelTS287.Image = null;
            this.p24_labelTS287.Location = new System.Drawing.Point(177, 73);
            this.p24_labelTS287.Name = "p24_labelTS287";
            this.p24_labelTS287.Size = new System.Drawing.Size(71, 16);
            this.p24_labelTS287.TabIndex = 175;
            this.p24_labelTS287.Text = "Update (ms):";
            this.p24_labelTS287.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS292
            // 
            this.p24_labelTS292.AutoSize = true;
            this.p24_labelTS292.Image = null;
            this.p24_labelTS292.Location = new System.Drawing.Point(21, 352);
            this.p24_labelTS292.Name = "p24_labelTS292";
            this.p24_labelTS292.Size = new System.Drawing.Size(33, 13);
            this.p24_labelTS292.TabIndex = 167;
            this.p24_labelTS292.Text = "Click:";
            this.p24_labelTS292.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS293
            // 
            this.p24_labelTS293.AutoSize = true;
            this.p24_labelTS293.Image = null;
            this.p24_labelTS293.Location = new System.Drawing.Point(3, 3);
            this.p24_labelTS293.Name = "p24_labelTS293";
            this.p24_labelTS293.Size = new System.Drawing.Size(49, 26);
            this.p24_labelTS293.TabIndex = 0;
            this.p24_labelTS293.Text = "lsb, usb\r\ndigl, digu";
            this.p24_labelTS293.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS294
            // 
            this.p24_labelTS294.AutoSize = true;
            this.p24_labelTS294.Image = null;
            this.p24_labelTS294.Location = new System.Drawing.Point(3, 33);
            this.p24_labelTS294.Name = "p24_labelTS294";
            this.p24_labelTS294.Size = new System.Drawing.Size(49, 13);
            this.p24_labelTS294.TabIndex = 1;
            this.p24_labelTS294.Text = "cwl, cwu";
            this.p24_labelTS294.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS296
            // 
            this.p24_labelTS296.AutoSize = true;
            this.p24_labelTS296.Image = null;
            this.p24_labelTS296.Location = new System.Drawing.Point(16, 55);
            this.p24_labelTS296.Name = "p24_labelTS296";
            this.p24_labelTS296.Size = new System.Drawing.Size(36, 13);
            this.p24_labelTS296.TabIndex = 2;
            this.p24_labelTS296.Text = "others";
            this.p24_labelTS296.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS297
            // 
            this.p24_labelTS297.AutoSize = true;
            this.p24_labelTS297.Image = null;
            this.p24_labelTS297.Location = new System.Drawing.Point(51, 43);
            this.p24_labelTS297.Name = "p24_labelTS297";
            this.p24_labelTS297.Size = new System.Drawing.Size(61, 13);
            this.p24_labelTS297.TabIndex = 162;
            this.p24_labelTS297.Text = "Font Scale:";
            this.p24_labelTS297.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS298
            // 
            this.p24_labelTS298.AutoSize = true;
            this.p24_labelTS298.Image = null;
            this.p24_labelTS298.Location = new System.Drawing.Point(170, 31);
            this.p24_labelTS298.Name = "p24_labelTS298";
            this.p24_labelTS298.Size = new System.Drawing.Size(75, 13);
            this.p24_labelTS298.TabIndex = 167;
            this.p24_labelTS298.Text = "Waterfall Low:";
            this.p24_labelTS298.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS299
            // 
            this.p24_labelTS299.AutoSize = true;
            this.p24_labelTS299.Image = null;
            this.p24_labelTS299.Location = new System.Drawing.Point(33, 77);
            this.p24_labelTS299.Name = "p24_labelTS299";
            this.p24_labelTS299.Size = new System.Drawing.Size(56, 13);
            this.p24_labelTS299.TabIndex = 169;
            this.p24_labelTS299.Text = "Data Line:";
            this.p24_labelTS299.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS300
            // 
            this.p24_labelTS300.AutoSize = true;
            this.p24_labelTS300.Image = null;
            this.p24_labelTS300.Location = new System.Drawing.Point(41, 99);
            this.p24_labelTS300.Name = "p24_labelTS300";
            this.p24_labelTS300.Size = new System.Drawing.Size(48, 13);
            this.p24_labelTS300.TabIndex = 171;
            this.p24_labelTS300.Text = "Data Fill:";
            this.p24_labelTS300.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS301
            // 
            this.p24_labelTS301.AutoSize = true;
            this.p24_labelTS301.Image = null;
            this.p24_labelTS301.Location = new System.Drawing.Point(212, 55);
            this.p24_labelTS301.Name = "p24_labelTS301";
            this.p24_labelTS301.Size = new System.Drawing.Size(31, 13);
            this.p24_labelTS301.TabIndex = 173;
            this.p24_labelTS301.Text = "Text:";
            this.p24_labelTS301.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS330
            // 
            this.p24_labelTS330.AutoSize = true;
            this.p24_labelTS330.Image = null;
            this.p24_labelTS330.Location = new System.Drawing.Point(152, 77);
            this.p24_labelTS330.Name = "p24_labelTS330";
            this.p24_labelTS330.Size = new System.Drawing.Size(91, 13);
            this.p24_labelTS330.TabIndex = 175;
            this.p24_labelTS330.Text = "Number Highlight:";
            this.p24_labelTS330.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS331
            // 
            this.p24_labelTS331.AutoSize = true;
            this.p24_labelTS331.Image = null;
            this.p24_labelTS331.Location = new System.Drawing.Point(160, 100);
            this.p24_labelTS331.Name = "p24_labelTS331";
            this.p24_labelTS331.Size = new System.Drawing.Size(83, 13);
            this.p24_labelTS331.TabIndex = 177;
            this.p24_labelTS331.Text = "RX Filter Edges:";
            this.p24_labelTS331.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS332
            // 
            this.p24_labelTS332.AutoSize = true;
            this.p24_labelTS332.Image = null;
            this.p24_labelTS332.Location = new System.Drawing.Point(39, 18);
            this.p24_labelTS332.Name = "p24_labelTS332";
            this.p24_labelTS332.Size = new System.Drawing.Size(73, 13);
            this.p24_labelTS332.TabIndex = 131;
            this.p24_labelTS332.Text = "Vertical Ratio:";

            // 
            // p24_labelTS333
            // 
            this.p24_labelTS333.AutoSize = true;
            this.p24_labelTS333.Image = null;
            this.p24_labelTS333.Location = new System.Drawing.Point(198, 18);
            this.p24_labelTS333.Name = "p24_labelTS333";
            this.p24_labelTS333.Size = new System.Drawing.Size(68, 13);
            this.p24_labelTS333.TabIndex = 130;
            this.p24_labelTS333.Text = "Background:";

            // 
            // p24_labelTS334
            // 
            this.p24_labelTS334.AutoSize = true;
            this.p24_labelTS334.Image = null;
            this.p24_labelTS334.Location = new System.Drawing.Point(139, 148);
            this.p24_labelTS334.Name = "p24_labelTS334";
            this.p24_labelTS334.Size = new System.Drawing.Size(104, 13);
            this.p24_labelTS334.TabIndex = 179;
            this.p24_labelTS334.Text = "Filter Edge Highlight:";
            this.p24_labelTS334.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS335
            // 
            this.p24_labelTS335.AutoSize = true;
            this.p24_labelTS335.Image = null;
            this.p24_labelTS335.Location = new System.Drawing.Point(24, 121);
            this.p24_labelTS335.Name = "p24_labelTS335";
            this.p24_labelTS335.Size = new System.Drawing.Size(65, 13);
            this.p24_labelTS335.TabIndex = 181;
            this.p24_labelTS335.Text = "Meter Back:";
            this.p24_labelTS335.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS336
            // 
            this.p24_labelTS336.AutoSize = true;
            this.p24_labelTS336.Image = null;
            this.p24_labelTS336.Location = new System.Drawing.Point(160, 192);
            this.p24_labelTS336.Name = "p24_labelTS336";
            this.p24_labelTS336.Size = new System.Drawing.Size(83, 13);
            this.p24_labelTS336.TabIndex = 185;
            this.p24_labelTS336.Text = "Notch Highlight:";
            this.p24_labelTS336.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS337
            // 
            this.p24_labelTS337.AutoSize = true;
            this.p24_labelTS337.Image = null;
            this.p24_labelTS337.Location = new System.Drawing.Point(204, 170);
            this.p24_labelTS337.Name = "p24_labelTS337";
            this.p24_labelTS337.Size = new System.Drawing.Size(39, 13);
            this.p24_labelTS337.TabIndex = 183;
            this.p24_labelTS337.Text = "Notch:";
            this.p24_labelTS337.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS338
            // 
            this.p24_labelTS338.AutoSize = true;
            this.p24_labelTS338.Image = null;
            this.p24_labelTS338.Location = new System.Drawing.Point(9, 143);
            this.p24_labelTS338.Name = "p24_labelTS338";
            this.p24_labelTS338.Size = new System.Drawing.Size(80, 13);
            this.p24_labelTS338.TabIndex = 187;
            this.p24_labelTS338.Text = "Edges/Extents:";
            this.p24_labelTS338.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS339
            // 
            this.p24_labelTS339.AutoSize = true;
            this.p24_labelTS339.Image = null;
            this.p24_labelTS339.Location = new System.Drawing.Point(287, 166);
            this.p24_labelTS339.Margin = new System.Windows.Forms.Padding(0);
            this.p24_labelTS339.Name = "p24_labelTS339";
            this.p24_labelTS339.Size = new System.Drawing.Size(33, 13);
            this.p24_labelTS339.TabIndex = 193;
            this.p24_labelTS339.Text = "frame";

            // 
            // p24_labelTS341
            // 
            this.p24_labelTS341.AutoSize = true;
            this.p24_labelTS341.Image = null;
            this.p24_labelTS341.Location = new System.Drawing.Point(117, 166);
            this.p24_labelTS341.Name = "p24_labelTS341";
            this.p24_labelTS341.Size = new System.Drawing.Size(116, 13);
            this.p24_labelTS341.TabIndex = 191;
            this.p24_labelTS341.Text = "Update waterfall every:";
            this.p24_labelTS341.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.p24_labelTS341, "How often to update (scroll another pixel line) on the waterfall display.  Note t" +
        "hat this is tamed by the FPS setting.");

            // 
            // p24_labelTS344
            // 
            this.p24_labelTS344.AutoSize = true;
            this.p24_labelTS344.Image = null;
            this.p24_labelTS344.Location = new System.Drawing.Point(185, 214);
            this.p24_labelTS344.Name = "p24_labelTS344";
            this.p24_labelTS344.Size = new System.Drawing.Size(58, 13);
            this.p24_labelTS344.TabIndex = 195;
            this.p24_labelTS344.Text = "Snap Line:";
            this.p24_labelTS344.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS345
            // 
            this.p24_labelTS345.AutoSize = true;
            this.p24_labelTS345.Image = null;
            this.p24_labelTS345.Location = new System.Drawing.Point(4, 188);
            this.p24_labelTS345.Name = "p24_labelTS345";
            this.p24_labelTS345.Size = new System.Drawing.Size(85, 13);
            this.p24_labelTS345.TabIndex = 199;
            this.p24_labelTS345.Text = "Button Highlight:";
            this.p24_labelTS345.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS346
            // 
            this.p24_labelTS346.AutoSize = true;
            this.p24_labelTS346.Image = null;
            this.p24_labelTS346.Location = new System.Drawing.Point(29, 165);
            this.p24_labelTS346.Name = "p24_labelTS346";
            this.p24_labelTS346.Size = new System.Drawing.Size(60, 13);
            this.p24_labelTS346.TabIndex = 197;
            this.p24_labelTS346.Text = "Setting On:";
            this.p24_labelTS346.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS347
            // 
            this.p24_labelTS347.AutoSize = true;
            this.p24_labelTS347.Image = null;
            this.p24_labelTS347.Location = new System.Drawing.Point(180, 89);
            this.p24_labelTS347.Name = "p24_labelTS347";
            this.p24_labelTS347.Size = new System.Drawing.Size(52, 13);
            this.p24_labelTS347.TabIndex = 117;
            this.p24_labelTS347.Text = "4Char ID:";

            // 
            // p24_labelTS348
            // 
            this.p24_labelTS348.AutoSize = true;
            this.p24_labelTS348.Image = null;
            this.p24_labelTS348.Location = new System.Drawing.Point(163, 207);
            this.p24_labelTS348.Name = "p24_labelTS348";
            this.p24_labelTS348.Size = new System.Drawing.Size(64, 13);
            this.p24_labelTS348.TabIndex = 203;
            this.p24_labelTS348.Text = "Lower level:";

            // 
            // p24_labelTS349
            // 
            this.p24_labelTS349.AutoSize = true;
            this.p24_labelTS349.Image = null;
            this.p24_labelTS349.Location = new System.Drawing.Point(286, 207);
            this.p24_labelTS349.Margin = new System.Windows.Forms.Padding(0);
            this.p24_labelTS349.Name = "p24_labelTS349";
            this.p24_labelTS349.Size = new System.Drawing.Size(20, 13);
            this.p24_labelTS349.TabIndex = 204;
            this.p24_labelTS349.Text = "dB";

            // 
            // p24_labelTS350
            // 
            this.p24_labelTS350.AutoSize = true;
            this.p24_labelTS350.Image = null;
            this.p24_labelTS350.Location = new System.Drawing.Point(62, 244);
            this.p24_labelTS350.Name = "p24_labelTS350";
            this.p24_labelTS350.Size = new System.Drawing.Size(31, 13);
            this.p24_labelTS350.TabIndex = 166;
            this.p24_labelTS350.Text = "Text:";

            // 
            // p24_labelTS351
            // 
            this.p24_labelTS351.AutoSize = true;
            this.p24_labelTS351.Image = null;
            this.p24_labelTS351.Location = new System.Drawing.Point(20, 112);
            this.p24_labelTS351.Name = "p24_labelTS351";
            this.p24_labelTS351.Size = new System.Drawing.Size(119, 13);
            this.p24_labelTS351.TabIndex = 167;
            this.p24_labelTS351.Text = "Increment Tunestep @:";

            // 
            // p24_labelTS352
            // 
            this.p24_labelTS352.AutoSize = true;
            this.p24_labelTS352.Image = null;
            this.p24_labelTS352.Location = new System.Drawing.Point(195, 112);
            this.p24_labelTS352.Name = "p24_labelTS352";
            this.p24_labelTS352.Size = new System.Drawing.Size(35, 13);
            this.p24_labelTS352.TabIndex = 169;
            this.p24_labelTS352.Text = "deg/s";

            // 
            // p24_labelTS353
            // 
            this.p24_labelTS353.AutoSize = true;
            this.p24_labelTS353.Image = null;
            this.p24_labelTS353.Location = new System.Drawing.Point(195, 136);
            this.p24_labelTS353.Name = "p24_labelTS353";
            this.p24_labelTS353.Size = new System.Drawing.Size(35, 13);
            this.p24_labelTS353.TabIndex = 172;
            this.p24_labelTS353.Text = "deg/s";

            // 
            // p24_labelTS354
            // 
            this.p24_labelTS354.AutoSize = true;
            this.p24_labelTS354.Image = null;
            this.p24_labelTS354.Location = new System.Drawing.Point(15, 136);
            this.p24_labelTS354.Name = "p24_labelTS354";
            this.p24_labelTS354.Size = new System.Drawing.Size(124, 13);
            this.p24_labelTS354.TabIndex = 170;
            this.p24_labelTS354.Text = "Decrement Tunestep @:";

            // 
            // p24_labelTS407
            // 
            this.p24_labelTS407.AutoSize = true;
            this.p24_labelTS407.Image = null;
            this.p24_labelTS407.Location = new System.Drawing.Point(195, 160);
            this.p24_labelTS407.Name = "p24_labelTS407";
            this.p24_labelTS407.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS407.TabIndex = 175;
            this.p24_labelTS407.Text = "s";

            // 
            // p24_labelTS408
            // 
            this.p24_labelTS408.AutoSize = true;
            this.p24_labelTS408.Image = null;
            this.p24_labelTS408.Location = new System.Drawing.Point(6, 160);
            this.p24_labelTS408.Name = "p24_labelTS408";
            this.p24_labelTS408.Size = new System.Drawing.Size(133, 13);
            this.p24_labelTS408.TabIndex = 173;
            this.p24_labelTS408.Text = "Tunestep Change Interval:";

            // 
            // p24_labelTS409
            // 
            this.p24_labelTS409.AutoSize = true;
            this.p24_labelTS409.Image = null;
            this.p24_labelTS409.Location = new System.Drawing.Point(32, 269);
            this.p24_labelTS409.Name = "p24_labelTS409";
            this.p24_labelTS409.Size = new System.Drawing.Size(61, 13);
            this.p24_labelTS409.TabIndex = 178;
            this.p24_labelTS409.Text = "Main circle:";

            // 
            // p24_labelTS410
            // 
            this.p24_labelTS410.AutoSize = true;
            this.p24_labelTS410.Image = null;
            this.p24_labelTS410.Location = new System.Drawing.Point(24, 319);
            this.p24_labelTS410.Name = "p24_labelTS410";
            this.p24_labelTS410.Size = new System.Drawing.Size(69, 13);
            this.p24_labelTS410.TabIndex = 182;
            this.p24_labelTS410.Text = "Pad pressed:";

            // 
            // p24_labelTS411
            // 
            this.p24_labelTS411.AutoSize = true;
            this.p24_labelTS411.Image = null;
            this.p24_labelTS411.Location = new System.Drawing.Point(64, 294);
            this.p24_labelTS411.Name = "p24_labelTS411";
            this.p24_labelTS411.Size = new System.Drawing.Size(29, 13);
            this.p24_labelTS411.TabIndex = 180;
            this.p24_labelTS411.Text = "Pad:";

            // 
            // p24_labelTS412
            // 
            this.p24_labelTS412.AutoSize = true;
            this.p24_labelTS412.Image = null;
            this.p24_labelTS412.Location = new System.Drawing.Point(207, 344);
            this.p24_labelTS412.Name = "p24_labelTS412";
            this.p24_labelTS412.Size = new System.Drawing.Size(58, 13);
            this.p24_labelTS412.TabIndex = 186;
            this.p24_labelTS412.Text = "Button Off:";

            // 
            // p24_labelTS413
            // 
            this.p24_labelTS413.AutoSize = true;
            this.p24_labelTS413.Image = null;
            this.p24_labelTS413.Location = new System.Drawing.Point(35, 344);
            this.p24_labelTS413.Name = "p24_labelTS413";
            this.p24_labelTS413.Size = new System.Drawing.Size(58, 13);
            this.p24_labelTS413.TabIndex = 184;
            this.p24_labelTS413.Text = "Button On:";

            // 
            // p24_labelTS414
            // 
            this.p24_labelTS414.AutoSize = true;
            this.p24_labelTS414.Image = null;
            this.p24_labelTS414.Location = new System.Drawing.Point(184, 294);
            this.p24_labelTS414.Name = "p24_labelTS414";
            this.p24_labelTS414.Size = new System.Drawing.Size(81, 13);
            this.p24_labelTS414.TabIndex = 194;
            this.p24_labelTS414.Text = "Auto accel fast:";

            // 
            // p24_labelTS415
            // 
            this.p24_labelTS415.AutoSize = true;
            this.p24_labelTS415.Image = null;
            this.p24_labelTS415.Location = new System.Drawing.Point(181, 269);
            this.p24_labelTS415.Name = "p24_labelTS415";
            this.p24_labelTS415.Size = new System.Drawing.Size(84, 13);
            this.p24_labelTS415.TabIndex = 192;
            this.p24_labelTS415.Text = "Auto accel hold:";

            // 
            // p24_labelTS416
            // 
            this.p24_labelTS416.AutoSize = true;
            this.p24_labelTS416.Image = null;
            this.p24_labelTS416.Location = new System.Drawing.Point(180, 244);
            this.p24_labelTS416.Name = "p24_labelTS416";
            this.p24_labelTS416.Size = new System.Drawing.Size(85, 13);
            this.p24_labelTS416.TabIndex = 190;
            this.p24_labelTS416.Text = "Auto accel slow:";

            // 
            // p24_labelTS417
            // 
            this.p24_labelTS417.AutoSize = true;
            this.p24_labelTS417.Image = null;
            this.p24_labelTS417.Location = new System.Drawing.Point(203, 219);
            this.p24_labelTS417.Name = "p24_labelTS417";
            this.p24_labelTS417.Size = new System.Drawing.Size(62, 13);
            this.p24_labelTS417.TabIndex = 188;
            this.p24_labelTS417.Text = "Dotted ring:";

            // 
            // p24_labelTS418
            // 
            this.p24_labelTS418.AutoSize = true;
            this.p24_labelTS418.Image = null;
            this.p24_labelTS418.Location = new System.Drawing.Point(180, 319);
            this.p24_labelTS418.Name = "p24_labelTS418";
            this.p24_labelTS418.Size = new System.Drawing.Size(85, 13);
            this.p24_labelTS418.TabIndex = 196;
            this.p24_labelTS418.Text = "Button Highlight:";

            // 
            // p24_labelTS419
            // 
            this.p24_labelTS419.AutoSize = true;
            this.p24_labelTS419.Image = null;
            this.p24_labelTS419.Location = new System.Drawing.Point(45, 184);
            this.p24_labelTS419.Name = "p24_labelTS419";
            this.p24_labelTS419.Size = new System.Drawing.Size(94, 13);
            this.p24_labelTS419.TabIndex = 197;
            this.p24_labelTS419.Text = "Max # increments:";

            // 
            // p24_labelTS420
            // 
            this.p24_labelTS420.AutoSize = true;
            this.p24_labelTS420.Image = null;
            this.p24_labelTS420.Location = new System.Drawing.Point(51, 43);
            this.p24_labelTS420.Name = "p24_labelTS420";
            this.p24_labelTS420.Size = new System.Drawing.Size(61, 13);
            this.p24_labelTS420.TabIndex = 162;
            this.p24_labelTS420.Text = "Font Scale:";
            this.p24_labelTS420.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS421
            // 
            this.p24_labelTS421.AutoSize = true;
            this.p24_labelTS421.Image = null;
            this.p24_labelTS421.Location = new System.Drawing.Point(35, 208);
            this.p24_labelTS421.Name = "p24_labelTS421";
            this.p24_labelTS421.Size = new System.Drawing.Size(104, 13);
            this.p24_labelTS421.TabIndex = 199;
            this.p24_labelTS421.Text = "Degrees for change:";

            // 
            // p24_labelTS422
            // 
            this.p24_labelTS422.AutoSize = true;
            this.p24_labelTS422.Image = null;
            this.p24_labelTS422.Location = new System.Drawing.Point(161, 124);
            this.p24_labelTS422.Name = "p24_labelTS422";
            this.p24_labelTS422.Size = new System.Drawing.Size(82, 13);
            this.p24_labelTS422.TabIndex = 201;
            this.p24_labelTS422.Text = "TX Filter Edges:";
            this.p24_labelTS422.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_labelTS424
            // 
            this.p24_labelTS424.AutoSize = true;
            this.p24_labelTS424.Image = null;
            this.p24_labelTS424.Location = new System.Drawing.Point(39, 18);
            this.p24_labelTS424.Name = "p24_labelTS424";
            this.p24_labelTS424.Size = new System.Drawing.Size(73, 13);
            this.p24_labelTS424.TabIndex = 131;
            this.p24_labelTS424.Text = "Vertical Ratio:";

            // 
            // p24_labelTS428
            // 
            this.p24_labelTS428.Image = null;
            this.p24_labelTS428.Location = new System.Drawing.Point(5, 5);
            this.p24_labelTS428.Name = "p24_labelTS428";
            this.p24_labelTS428.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS428.TabIndex = 104;
            this.p24_labelTS428.Text = "Min:";
            this.p24_labelTS428.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS429
            // 
            this.p24_labelTS429.Image = null;
            this.p24_labelTS429.Location = new System.Drawing.Point(5, 31);
            this.p24_labelTS429.Name = "p24_labelTS429";
            this.p24_labelTS429.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS429.TabIndex = 106;
            this.p24_labelTS429.Text = "Max:";
            this.p24_labelTS429.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS431
            // 
            this.p24_labelTS431.Image = null;
            this.p24_labelTS431.Location = new System.Drawing.Point(5, 57);
            this.p24_labelTS431.Name = "p24_labelTS431";
            this.p24_labelTS431.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS431.TabIndex = 110;
            this.p24_labelTS431.Text = "High:";
            this.p24_labelTS431.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS432
            // 
            this.p24_labelTS432.Image = null;
            this.p24_labelTS432.Location = new System.Drawing.Point(5, 85);
            this.p24_labelTS432.Name = "p24_labelTS432";
            this.p24_labelTS432.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS432.TabIndex = 113;
            this.p24_labelTS432.Text = "Units:";
            this.p24_labelTS432.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS433
            // 
            this.p24_labelTS433.Image = null;
            this.p24_labelTS433.Location = new System.Drawing.Point(5, 111);
            this.p24_labelTS433.Name = "p24_labelTS433";
            this.p24_labelTS433.Size = new System.Drawing.Size(43, 16);
            this.p24_labelTS433.TabIndex = 115;
            this.p24_labelTS433.Text = "Title:";
            this.p24_labelTS433.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_labelTS482
            // 
            this.p24_labelTS482.AutoSize = true;
            this.p24_labelTS482.Image = null;
            this.p24_labelTS482.Location = new System.Drawing.Point(186, 215);
            this.p24_labelTS482.Name = "p24_labelTS482";
            this.p24_labelTS482.Size = new System.Drawing.Size(52, 13);
            this.p24_labelTS482.TabIndex = 178;
            this.p24_labelTS482.Text = "4Char ID:";

            // 
            // p24_labelTS650
            // 
            this.p24_labelTS650.AutoSize = true;
            this.p24_labelTS650.Image = null;
            this.p24_labelTS650.Location = new System.Drawing.Point(3, 3);
            this.p24_labelTS650.Name = "p24_labelTS650";
            this.p24_labelTS650.Size = new System.Drawing.Size(40, 13);
            this.p24_labelTS650.TabIndex = 0;
            this.p24_labelTS650.Text = "# Slots";

            // 
            // p24_labelTS651
            // 
            this.p24_labelTS651.AutoSize = true;
            this.p24_labelTS651.Image = null;
            this.p24_labelTS651.Location = new System.Drawing.Point(3, 4);
            this.p24_labelTS651.Name = "p24_labelTS651";
            this.p24_labelTS651.Size = new System.Drawing.Size(36, 13);
            this.p24_labelTS651.TabIndex = 0;
            this.p24_labelTS651.Text = "Label:";

            // 
            // p24_labelTS652
            // 
            this.p24_labelTS652.AutoSize = true;
            this.p24_labelTS652.Image = null;
            this.p24_labelTS652.Location = new System.Drawing.Point(128, 28);
            this.p24_labelTS652.Name = "p24_labelTS652";
            this.p24_labelTS652.Size = new System.Drawing.Size(12, 13);
            this.p24_labelTS652.TabIndex = 175;
            this.p24_labelTS652.Text = "s";

            // 
            // p24_labelTS653
            // 
            this.p24_labelTS653.AutoSize = true;
            this.p24_labelTS653.Image = null;
            this.p24_labelTS653.Location = new System.Drawing.Point(16, 54);
            this.p24_labelTS653.Name = "p24_labelTS653";
            this.p24_labelTS653.Size = new System.Drawing.Size(25, 13);
            this.p24_labelTS653.TabIndex = 180;
            this.p24_labelTS653.Text = "Slot";

            // 
            // p24_labelTS654
            // 
            this.p24_labelTS654.AutoSize = true;
            this.p24_labelTS654.Image = null;
            this.p24_labelTS654.Location = new System.Drawing.Point(2, 118);
            this.p24_labelTS654.Name = "p24_labelTS654";
            this.p24_labelTS654.Size = new System.Drawing.Size(75, 13);
            this.p24_labelTS654.TabIndex = 182;
            this.p24_labelTS654.Text = "TX gain adjust";

            // 
            // p24_labelTS655
            // 
            this.p24_labelTS655.AutoSize = true;
            this.p24_labelTS655.Image = null;
            this.p24_labelTS655.Location = new System.Drawing.Point(126, 118);
            this.p24_labelTS655.Name = "p24_labelTS655";
            this.p24_labelTS655.Size = new System.Drawing.Size(20, 13);
            this.p24_labelTS655.TabIndex = 183;
            this.p24_labelTS655.Text = "dB";

            // 
            // p24_labelTS657
            // 
            this.p24_labelTS657.AutoSize = true;
            this.p24_labelTS657.Image = null;
            this.p24_labelTS657.Location = new System.Drawing.Point(91, 54);
            this.p24_labelTS657.Name = "p24_labelTS657";
            this.p24_labelTS657.Size = new System.Drawing.Size(43, 13);
            this.p24_labelTS657.TabIndex = 182;
            this.p24_labelTS657.Text = "settings";

            // 
            // p24_labelTS658
            // 
            this.p24_labelTS658.AutoSize = true;
            this.p24_labelTS658.Image = null;
            this.p24_labelTS658.Location = new System.Drawing.Point(3, 29);
            this.p24_labelTS658.Name = "p24_labelTS658";
            this.p24_labelTS658.Size = new System.Drawing.Size(52, 13);
            this.p24_labelTS658.TabIndex = 184;
            this.p24_labelTS658.Text = "4Char ID:";

            // 
            // p24_lblBandButtons_indicator_border
            // 
            this.p24_lblBandButtons_indicator_border.AutoSize = true;
            this.p24_lblBandButtons_indicator_border.Image = null;
            this.p24_lblBandButtons_indicator_border.Location = new System.Drawing.Point(44, 174);
            this.p24_lblBandButtons_indicator_border.Name = "p24_lblBandButtons_indicator_border";
            this.p24_lblBandButtons_indicator_border.Size = new System.Drawing.Size(41, 13);
            this.p24_lblBandButtons_indicator_border.TabIndex = 146;
            this.p24_lblBandButtons_indicator_border.Text = "Border:";
            this.p24_lblBandButtons_indicator_border.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_lblBandButtons_indicator_style
            // 
            this.p24_lblBandButtons_indicator_style.AutoSize = true;
            this.p24_lblBandButtons_indicator_style.Image = null;
            this.p24_lblBandButtons_indicator_style.Location = new System.Drawing.Point(159, 174);
            this.p24_lblBandButtons_indicator_style.Name = "p24_lblBandButtons_indicator_style";
            this.p24_lblBandButtons_indicator_style.Size = new System.Drawing.Size(33, 13);
            this.p24_lblBandButtons_indicator_style.TabIndex = 159;
            this.p24_lblBandButtons_indicator_style.Text = "Style:";
            this.p24_lblBandButtons_indicator_style.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_lblLedIndicator_panelbackground
            // 
            this.p24_lblLedIndicator_panelbackground.AutoSize = true;
            this.p24_lblLedIndicator_panelbackground.Image = null;
            this.p24_lblLedIndicator_panelbackground.Location = new System.Drawing.Point(33, 52);
            this.p24_lblLedIndicator_panelbackground.Name = "p24_lblLedIndicator_panelbackground";
            this.p24_lblLedIndicator_panelbackground.Size = new System.Drawing.Size(86, 13);
            this.p24_lblLedIndicator_panelbackground.TabIndex = 130;
            this.p24_lblLedIndicator_panelbackground.Text = "RX Background:";

            // 
            // p24_lblLedIndicator_panelbackgroundTX
            // 
            this.p24_lblLedIndicator_panelbackgroundTX.AutoSize = true;
            this.p24_lblLedIndicator_panelbackgroundTX.Image = null;
            this.p24_lblLedIndicator_panelbackgroundTX.Location = new System.Drawing.Point(34, 77);
            this.p24_lblLedIndicator_panelbackgroundTX.Name = "p24_lblLedIndicator_panelbackgroundTX";
            this.p24_lblLedIndicator_panelbackgroundTX.Size = new System.Drawing.Size(85, 13);
            this.p24_lblLedIndicator_panelbackgroundTX.TabIndex = 163;
            this.p24_lblLedIndicator_panelbackgroundTX.Text = "TX Background:";

            // 
            // p24_lblLed_Valid
            // 
            this.p24_lblLed_Valid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_lblLed_Valid.Image = null;
            this.p24_lblLed_Valid.Location = new System.Drawing.Point(215, 151);
            this.p24_lblLed_Valid.Name = "p24_lblLed_Valid";
            this.p24_lblLed_Valid.Size = new System.Drawing.Size(104, 15);
            this.p24_lblLed_Valid.TabIndex = 166;
            this.p24_lblLed_Valid.Text = "Syntax Invalid";
            this.p24_lblLed_Valid.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_lblMMBackground
            // 
            this.p24_lblMMBackground.AutoSize = true;
            this.p24_lblMMBackground.Image = null;
            this.p24_lblMMBackground.Location = new System.Drawing.Point(184, 22);
            this.p24_lblMMBackground.Name = "p24_lblMMBackground";
            this.p24_lblMMBackground.Size = new System.Drawing.Size(68, 13);
            this.p24_lblMMBackground.TabIndex = 93;
            this.p24_lblMMBackground.Text = "Background:";

            // 
            // p24_lblMMClockBackground
            // 
            this.p24_lblMMClockBackground.AutoSize = true;
            this.p24_lblMMClockBackground.Image = null;
            this.p24_lblMMClockBackground.Location = new System.Drawing.Point(32, 58);
            this.p24_lblMMClockBackground.Name = "p24_lblMMClockBackground";
            this.p24_lblMMClockBackground.Size = new System.Drawing.Size(68, 13);
            this.p24_lblMMClockBackground.TabIndex = 116;
            this.p24_lblMMClockBackground.Text = "Background:";

            // 
            // p24_lblMMContainerBackground
            // 
            this.p24_lblMMContainerBackground.AutoSize = true;
            this.p24_lblMMContainerBackground.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p24_lblMMContainerBackground.Image = null;
            this.p24_lblMMContainerBackground.Location = new System.Drawing.Point(89, 107);
            this.p24_lblMMContainerBackground.Name = "p24_lblMMContainerBackground";
            this.p24_lblMMContainerBackground.Size = new System.Drawing.Size(68, 13);
            this.p24_lblMMContainerBackground.TabIndex = 99;
            this.p24_lblMMContainerBackground.Text = "Background:";
            this.p24_lblMMContainerBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblMMContainerNotes
            // 
            this.p24_lblMMContainerNotes.AutoSize = true;
            this.p24_lblMMContainerNotes.Image = null;
            this.p24_lblMMContainerNotes.Location = new System.Drawing.Point(7, 113);
            this.p24_lblMMContainerNotes.Name = "p24_lblMMContainerNotes";
            this.p24_lblMMContainerNotes.Size = new System.Drawing.Size(38, 13);
            this.p24_lblMMContainerNotes.TabIndex = 107;
            this.p24_lblMMContainerNotes.Text = "Notes:";
            this.p24_lblMMContainerNotes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblMMEyeBezelSize
            // 
            this.p24_lblMMEyeBezelSize.AutoSize = true;
            this.p24_lblMMEyeBezelSize.Image = null;
            this.p24_lblMMEyeBezelSize.Location = new System.Drawing.Point(1, 242);
            this.p24_lblMMEyeBezelSize.Name = "p24_lblMMEyeBezelSize";
            this.p24_lblMMEyeBezelSize.Size = new System.Drawing.Size(59, 13);
            this.p24_lblMMEyeBezelSize.TabIndex = 123;
            this.p24_lblMMEyeBezelSize.Text = "Bezel Size:";

            // 
            // p24_lblMMEyeSize
            // 
            this.p24_lblMMEyeSize.AutoSize = true;
            this.p24_lblMMEyeSize.Image = null;
            this.p24_lblMMEyeSize.Location = new System.Drawing.Point(1, 221);
            this.p24_lblMMEyeSize.Name = "p24_lblMMEyeSize";
            this.p24_lblMMEyeSize.Size = new System.Drawing.Size(51, 13);
            this.p24_lblMMEyeSize.TabIndex = 109;
            this.p24_lblMMEyeSize.Text = "Eye Size:";

            // 
            // p24_lblMMHigh
            // 
            this.p24_lblMMHigh.AutoSize = true;
            this.p24_lblMMHigh.Image = null;
            this.p24_lblMMHigh.Location = new System.Drawing.Point(114, 9);
            this.p24_lblMMHigh.Name = "p24_lblMMHigh";
            this.p24_lblMMHigh.Size = new System.Drawing.Size(32, 13);
            this.p24_lblMMHigh.TabIndex = 79;
            this.p24_lblMMHigh.Text = "High:";

            // 
            // p24_lblMMHistory
            // 
            this.p24_lblMMHistory.AutoSize = true;
            this.p24_lblMMHistory.Image = null;
            this.p24_lblMMHistory.Location = new System.Drawing.Point(163, 58);
            this.p24_lblMMHistory.Name = "p24_lblMMHistory";
            this.p24_lblMMHistory.Size = new System.Drawing.Size(64, 13);
            this.p24_lblMMHistory.TabIndex = 97;
            this.p24_lblMMHistory.Text = "History (ms):";

            // 
            // p24_lblMMHistoryIgnore
            // 
            this.p24_lblMMHistoryIgnore.AutoSize = true;
            this.p24_lblMMHistoryIgnore.Image = null;
            this.p24_lblMMHistoryIgnore.Location = new System.Drawing.Point(100, 81);
            this.p24_lblMMHistoryIgnore.Name = "p24_lblMMHistoryIgnore";
            this.p24_lblMMHistoryIgnore.Size = new System.Drawing.Size(127, 13);
            this.p24_lblMMHistoryIgnore.TabIndex = 126;
            this.p24_lblMMHistoryIgnore.Text = "Ignore History/Peak (ms):";
            this.p24_lblMMHistoryIgnore.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_lblMMIndicator
            // 
            this.p24_lblMMIndicator.AutoSize = true;
            this.p24_lblMMIndicator.Image = null;
            this.p24_lblMMIndicator.Location = new System.Drawing.Point(5, 34);
            this.p24_lblMMIndicator.Name = "p24_lblMMIndicator";
            this.p24_lblMMIndicator.Size = new System.Drawing.Size(51, 13);
            this.p24_lblMMIndicator.TabIndex = 81;
            this.p24_lblMMIndicator.Text = "Indicator:";

            // 
            // p24_lblMMIndicatorSub
            // 
            this.p24_lblMMIndicatorSub.AutoSize = true;
            this.p24_lblMMIndicatorSub.Image = null;
            this.p24_lblMMIndicatorSub.Location = new System.Drawing.Point(154, 34);
            this.p24_lblMMIndicatorSub.Name = "p24_lblMMIndicatorSub";
            this.p24_lblMMIndicatorSub.Size = new System.Drawing.Size(40, 13);
            this.p24_lblMMIndicatorSub.TabIndex = 121;
            this.p24_lblMMIndicatorSub.Text = "Sub(s):";
            this.p24_lblMMIndicatorSub.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_lblMMLow
            // 
            this.p24_lblMMLow.AutoSize = true;
            this.p24_lblMMLow.Image = null;
            this.p24_lblMMLow.Location = new System.Drawing.Point(19, 9);
            this.p24_lblMMLow.Name = "p24_lblMMLow";
            this.p24_lblMMLow.Size = new System.Drawing.Size(30, 13);
            this.p24_lblMMLow.TabIndex = 78;
            this.p24_lblMMLow.Text = "Low:";
            this.p24_lblMMLow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblMMPowerLimit
            // 
            this.p24_lblMMPowerLimit.Image = null;
            this.p24_lblMMPowerLimit.Location = new System.Drawing.Point(143, 243);
            this.p24_lblMMPowerLimit.Name = "p24_lblMMPowerLimit";
            this.p24_lblMMPowerLimit.Size = new System.Drawing.Size(44, 16);
            this.p24_lblMMPowerLimit.TabIndex = 113;
            this.p24_lblMMPowerLimit.Text = "Power:";
            this.p24_lblMMPowerLimit.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // p24_lblMMsegSolHigh
            // 
            this.p24_lblMMsegSolHigh.AutoSize = true;
            this.p24_lblMMsegSolHigh.Image = null;
            this.p24_lblMMsegSolHigh.Location = new System.Drawing.Point(139, 106);
            this.p24_lblMMsegSolHigh.Name = "p24_lblMMsegSolHigh";
            this.p24_lblMMsegSolHigh.Size = new System.Drawing.Size(27, 13);
            this.p24_lblMMsegSolHigh.TabIndex = 118;
            this.p24_lblMMsegSolHigh.Text = "high";

            // 
            // p24_lblMMsegSolLow
            // 
            this.p24_lblMMsegSolLow.AutoSize = true;
            this.p24_lblMMsegSolLow.Image = null;
            this.p24_lblMMsegSolLow.Location = new System.Drawing.Point(94, 106);
            this.p24_lblMMsegSolLow.Name = "p24_lblMMsegSolLow";
            this.p24_lblMMsegSolLow.Size = new System.Drawing.Size(23, 13);
            this.p24_lblMMsegSolLow.TabIndex = 117;
            this.p24_lblMMsegSolLow.Text = "low";

            // 
            // p24_lblMeterItemRotatorAZcommand
            // 
            this.p24_lblMeterItemRotatorAZcommand.AutoSize = true;
            this.p24_lblMeterItemRotatorAZcommand.Image = null;
            this.p24_lblMeterItemRotatorAZcommand.Location = new System.Drawing.Point(11, 300);
            this.p24_lblMeterItemRotatorAZcommand.Name = "p24_lblMeterItemRotatorAZcommand";
            this.p24_lblMeterItemRotatorAZcommand.Size = new System.Drawing.Size(24, 13);
            this.p24_lblMeterItemRotatorAZcommand.TabIndex = 140;
            this.p24_lblMeterItemRotatorAZcommand.Text = "AZ:";

            // 
            // p24_lblMeterItemRotatorBeamWidth_alpha
            // 
            this.p24_lblMeterItemRotatorBeamWidth_alpha.AutoSize = true;
            this.p24_lblMeterItemRotatorBeamWidth_alpha.Image = null;
            this.p24_lblMeterItemRotatorBeamWidth_alpha.Location = new System.Drawing.Point(153, 169);
            this.p24_lblMeterItemRotatorBeamWidth_alpha.Name = "p24_lblMeterItemRotatorBeamWidth_alpha";
            this.p24_lblMeterItemRotatorBeamWidth_alpha.Size = new System.Drawing.Size(37, 13);
            this.p24_lblMeterItemRotatorBeamWidth_alpha.TabIndex = 177;
            this.p24_lblMeterItemRotatorBeamWidth_alpha.Text = "Alpha:";

            // 
            // p24_lblMeterItemRotatorBeamWidth_degrees
            // 
            this.p24_lblMeterItemRotatorBeamWidth_degrees.AutoSize = true;
            this.p24_lblMeterItemRotatorBeamWidth_degrees.Image = null;
            this.p24_lblMeterItemRotatorBeamWidth_degrees.Location = new System.Drawing.Point(27, 169);
            this.p24_lblMeterItemRotatorBeamWidth_degrees.Name = "p24_lblMeterItemRotatorBeamWidth_degrees";
            this.p24_lblMeterItemRotatorBeamWidth_degrees.Size = new System.Drawing.Size(50, 13);
            this.p24_lblMeterItemRotatorBeamWidth_degrees.TabIndex = 176;
            this.p24_lblMeterItemRotatorBeamWidth_degrees.Text = "Degrees:";

            // 
            // p24_lblMeterItemRotatorELEcommand
            // 
            this.p24_lblMeterItemRotatorELEcommand.AutoSize = true;
            this.p24_lblMeterItemRotatorELEcommand.Image = null;
            this.p24_lblMeterItemRotatorELEcommand.Location = new System.Drawing.Point(5, 326);
            this.p24_lblMeterItemRotatorELEcommand.Name = "p24_lblMeterItemRotatorELEcommand";
            this.p24_lblMeterItemRotatorELEcommand.Size = new System.Drawing.Size(30, 13);
            this.p24_lblMeterItemRotatorELEcommand.TabIndex = 141;
            this.p24_lblMeterItemRotatorELEcommand.Text = "ELE:";

            // 
            // p24_lblRotator_4charID
            // 
            this.p24_lblRotator_4charID.AutoSize = true;
            this.p24_lblRotator_4charID.Image = null;
            this.p24_lblRotator_4charID.Location = new System.Drawing.Point(210, 275);
            this.p24_lblRotator_4charID.Name = "p24_lblRotator_4charID";
            this.p24_lblRotator_4charID.Size = new System.Drawing.Size(52, 13);
            this.p24_lblRotator_4charID.TabIndex = 168;
            this.p24_lblRotator_4charID.Text = "4Char ID:";

            // 
            // p24_lblTextOverlay_panelbackground
            // 
            this.p24_lblTextOverlay_panelbackground.AutoSize = true;
            this.p24_lblTextOverlay_panelbackground.Image = null;
            this.p24_lblTextOverlay_panelbackground.Location = new System.Drawing.Point(49, 50);
            this.p24_lblTextOverlay_panelbackground.Name = "p24_lblTextOverlay_panelbackground";
            this.p24_lblTextOverlay_panelbackground.Size = new System.Drawing.Size(86, 13);
            this.p24_lblTextOverlay_panelbackground.TabIndex = 130;
            this.p24_lblTextOverlay_panelbackground.Text = "RX Background:";

            // 
            // p24_lblTextOverlay_panelbackgroundTX
            // 
            this.p24_lblTextOverlay_panelbackgroundTX.AutoSize = true;
            this.p24_lblTextOverlay_panelbackgroundTX.Image = null;
            this.p24_lblTextOverlay_panelbackgroundTX.Location = new System.Drawing.Point(50, 75);
            this.p24_lblTextOverlay_panelbackgroundTX.Name = "p24_lblTextOverlay_panelbackgroundTX";
            this.p24_lblTextOverlay_panelbackgroundTX.Size = new System.Drawing.Size(85, 13);
            this.p24_lblTextOverlay_panelbackgroundTX.TabIndex = 163;
            this.p24_lblTextOverlay_panelbackgroundTX.Text = "TX Background:";

            // 
            // p24_lblTextOverlay_panelpadding
            // 
            this.p24_lblTextOverlay_panelpadding.AutoSize = true;
            this.p24_lblTextOverlay_panelpadding.Image = null;
            this.p24_lblTextOverlay_panelpadding.Location = new System.Drawing.Point(39, 102);
            this.p24_lblTextOverlay_panelpadding.Name = "p24_lblTextOverlay_panelpadding";
            this.p24_lblTextOverlay_panelpadding.Size = new System.Drawing.Size(79, 13);
            this.p24_lblTextOverlay_panelpadding.TabIndex = 131;
            this.p24_lblTextOverlay_panelpadding.Text = "Panel Padding:";

            // 
            // p24_lblWaveRecordBack
            // 
            this.p24_lblWaveRecordBack.Image = null;
            this.p24_lblWaveRecordBack.Location = new System.Drawing.Point(10, 76);
            this.p24_lblWaveRecordBack.Name = "p24_lblWaveRecordBack";
            this.p24_lblWaveRecordBack.Size = new System.Drawing.Size(104, 16);
            this.p24_lblWaveRecordBack.TabIndex = 17;
            this.p24_lblWaveRecordBack.Text = "Background:";
            this.p24_lblWaveRecordBack.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordBorder
            // 
            this.p24_lblWaveRecordBorder.Image = null;
            this.p24_lblWaveRecordBorder.Location = new System.Drawing.Point(164, 76);
            this.p24_lblWaveRecordBorder.Name = "p24_lblWaveRecordBorder";
            this.p24_lblWaveRecordBorder.Size = new System.Drawing.Size(102, 16);
            this.p24_lblWaveRecordBorder.TabIndex = 18;
            this.p24_lblWaveRecordBorder.Text = "Row Border:";
            this.p24_lblWaveRecordBorder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordButtonBorder
            // 
            this.p24_lblWaveRecordButtonBorder.Image = null;
            this.p24_lblWaveRecordButtonBorder.Location = new System.Drawing.Point(164, 124);
            this.p24_lblWaveRecordButtonBorder.Name = "p24_lblWaveRecordButtonBorder";
            this.p24_lblWaveRecordButtonBorder.Size = new System.Drawing.Size(102, 16);
            this.p24_lblWaveRecordButtonBorder.TabIndex = 22;
            this.p24_lblWaveRecordButtonBorder.Text = "Button Border:";
            this.p24_lblWaveRecordButtonBorder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordButtonFill
            // 
            this.p24_lblWaveRecordButtonFill.Image = null;
            this.p24_lblWaveRecordButtonFill.Location = new System.Drawing.Point(10, 124);
            this.p24_lblWaveRecordButtonFill.Name = "p24_lblWaveRecordButtonFill";
            this.p24_lblWaveRecordButtonFill.Size = new System.Drawing.Size(104, 16);
            this.p24_lblWaveRecordButtonFill.TabIndex = 21;
            this.p24_lblWaveRecordButtonFill.Text = "Button Fill:";
            this.p24_lblWaveRecordButtonFill.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordButtonHover
            // 
            this.p24_lblWaveRecordButtonHover.Image = null;
            this.p24_lblWaveRecordButtonHover.Location = new System.Drawing.Point(10, 148);
            this.p24_lblWaveRecordButtonHover.Name = "p24_lblWaveRecordButtonHover";
            this.p24_lblWaveRecordButtonHover.Size = new System.Drawing.Size(104, 16);
            this.p24_lblWaveRecordButtonHover.TabIndex = 23;
            this.p24_lblWaveRecordButtonHover.Text = "Button Hover:";
            this.p24_lblWaveRecordButtonHover.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordDelete
            // 
            this.p24_lblWaveRecordDelete.Image = null;
            this.p24_lblWaveRecordDelete.Location = new System.Drawing.Point(164, 172);
            this.p24_lblWaveRecordDelete.Name = "p24_lblWaveRecordDelete";
            this.p24_lblWaveRecordDelete.Size = new System.Drawing.Size(102, 16);
            this.p24_lblWaveRecordDelete.TabIndex = 26;
            this.p24_lblWaveRecordDelete.Text = "Trash Icon:";
            this.p24_lblWaveRecordDelete.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordHeightRatio
            // 
            this.p24_lblWaveRecordHeightRatio.Image = null;
            this.p24_lblWaveRecordHeightRatio.Location = new System.Drawing.Point(33, 18);
            this.p24_lblWaveRecordHeightRatio.Name = "p24_lblWaveRecordHeightRatio";
            this.p24_lblWaveRecordHeightRatio.Size = new System.Drawing.Size(80, 16);
            this.p24_lblWaveRecordHeightRatio.TabIndex = 16;
            this.p24_lblWaveRecordHeightRatio.Text = "Height Ratio:";
            this.p24_lblWaveRecordHeightRatio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordPlay
            // 
            this.p24_lblWaveRecordPlay.Image = null;
            this.p24_lblWaveRecordPlay.Location = new System.Drawing.Point(164, 148);
            this.p24_lblWaveRecordPlay.Name = "p24_lblWaveRecordPlay";
            this.p24_lblWaveRecordPlay.Size = new System.Drawing.Size(102, 16);
            this.p24_lblWaveRecordPlay.TabIndex = 24;
            this.p24_lblWaveRecordPlay.Text = "Play Icon:";
            this.p24_lblWaveRecordPlay.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordRadius
            // 
            this.p24_lblWaveRecordRadius.Image = null;
            this.p24_lblWaveRecordRadius.Location = new System.Drawing.Point(57, 40);
            this.p24_lblWaveRecordRadius.Name = "p24_lblWaveRecordRadius";
            this.p24_lblWaveRecordRadius.Size = new System.Drawing.Size(56, 16);
            this.p24_lblWaveRecordRadius.TabIndex = 30;
            this.p24_lblWaveRecordRadius.Text = "Radius:";
            this.p24_lblWaveRecordRadius.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordRow
            // 
            this.p24_lblWaveRecordRow.Image = null;
            this.p24_lblWaveRecordRow.Location = new System.Drawing.Point(10, 100);
            this.p24_lblWaveRecordRow.Name = "p24_lblWaveRecordRow";
            this.p24_lblWaveRecordRow.Size = new System.Drawing.Size(104, 16);
            this.p24_lblWaveRecordRow.TabIndex = 19;
            this.p24_lblWaveRecordRow.Text = "Row:";
            this.p24_lblWaveRecordRow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordScrollHover
            // 
            this.p24_lblWaveRecordScrollHover.Image = null;
            this.p24_lblWaveRecordScrollHover.Location = new System.Drawing.Point(10, 220);
            this.p24_lblWaveRecordScrollHover.Name = "p24_lblWaveRecordScrollHover";
            this.p24_lblWaveRecordScrollHover.Size = new System.Drawing.Size(104, 16);
            this.p24_lblWaveRecordScrollHover.TabIndex = 29;
            this.p24_lblWaveRecordScrollHover.Text = "Scroll Hover:";
            this.p24_lblWaveRecordScrollHover.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordScrollThumb
            // 
            this.p24_lblWaveRecordScrollThumb.Image = null;
            this.p24_lblWaveRecordScrollThumb.Location = new System.Drawing.Point(164, 196);
            this.p24_lblWaveRecordScrollThumb.Name = "p24_lblWaveRecordScrollThumb";
            this.p24_lblWaveRecordScrollThumb.Size = new System.Drawing.Size(102, 16);
            this.p24_lblWaveRecordScrollThumb.TabIndex = 28;
            this.p24_lblWaveRecordScrollThumb.Text = "Scroll Thumb:";
            this.p24_lblWaveRecordScrollThumb.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordScrollTrack
            // 
            this.p24_lblWaveRecordScrollTrack.Image = null;
            this.p24_lblWaveRecordScrollTrack.Location = new System.Drawing.Point(10, 196);
            this.p24_lblWaveRecordScrollTrack.Name = "p24_lblWaveRecordScrollTrack";
            this.p24_lblWaveRecordScrollTrack.Size = new System.Drawing.Size(104, 16);
            this.p24_lblWaveRecordScrollTrack.TabIndex = 27;
            this.p24_lblWaveRecordScrollTrack.Text = "Scroll Track:";
            this.p24_lblWaveRecordScrollTrack.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordStop
            // 
            this.p24_lblWaveRecordStop.Image = null;
            this.p24_lblWaveRecordStop.Location = new System.Drawing.Point(10, 172);
            this.p24_lblWaveRecordStop.Name = "p24_lblWaveRecordStop";
            this.p24_lblWaveRecordStop.Size = new System.Drawing.Size(104, 16);
            this.p24_lblWaveRecordStop.TabIndex = 25;
            this.p24_lblWaveRecordStop.Text = "Stop Icon:";
            this.p24_lblWaveRecordStop.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWaveRecordText
            // 
            this.p24_lblWaveRecordText.Image = null;
            this.p24_lblWaveRecordText.Location = new System.Drawing.Point(164, 100);
            this.p24_lblWaveRecordText.Name = "p24_lblWaveRecordText";
            this.p24_lblWaveRecordText.Size = new System.Drawing.Size(102, 16);
            this.p24_lblWaveRecordText.TabIndex = 20;
            this.p24_lblWaveRecordText.Text = "Text:";
            this.p24_lblWaveRecordText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // p24_lblWebImage_after
            // 
            this.p24_lblWebImage_after.AutoSize = true;
            this.p24_lblWebImage_after.Image = null;
            this.p24_lblWebImage_after.Location = new System.Drawing.Point(33, 169);
            this.p24_lblWebImage_after.Name = "p24_lblWebImage_after";
            this.p24_lblWebImage_after.Size = new System.Drawing.Size(29, 13);
            this.p24_lblWebImage_after.TabIndex = 152;
            this.p24_lblWebImage_after.Text = "After";

            // 
            // p24_lblWebImage_secs
            // 
            this.p24_lblWebImage_secs.AutoSize = true;
            this.p24_lblWebImage_secs.Image = null;
            this.p24_lblWebImage_secs.Location = new System.Drawing.Point(118, 169);
            this.p24_lblWebImage_secs.Name = "p24_lblWebImage_secs";
            this.p24_lblWebImage_secs.Size = new System.Drawing.Size(84, 13);
            this.p24_lblWebImage_secs.TabIndex = 149;
            this.p24_lblWebImage_secs.Text = "s goto 4Char ID:";

            // 
            // p24_lblWebImage_state
            // 
            this.p24_lblWebImage_state.Image = null;
            this.p24_lblWebImage_state.Location = new System.Drawing.Point(42, 101);
            this.p24_lblWebImage_state.Name = "p24_lblWebImage_state";
            this.p24_lblWebImage_state.Size = new System.Drawing.Size(93, 13);
            this.p24_lblWebImage_state.TabIndex = 142;
            this.p24_lblWebImage_state.Text = "state";

            // 
            // p24_lstMetersAvailable
            // 
            this.p24_lstMetersAvailable.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.p24_lstMetersAvailable.FormattingEnabled = true;
            this.p24_lstMetersAvailable.Location = new System.Drawing.Point(7, 174);
            this.p24_lstMetersAvailable.Name = "p24_lstMetersAvailable";
            this.p24_lstMetersAvailable.Size = new System.Drawing.Size(140, 212);
            this.p24_lstMetersAvailable.TabIndex = 90;
            this.p24_lstMetersAvailable.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstMetersAvailable_DrawItem);
            this.p24_lstMetersAvailable.SelectedIndexChanged += new System.EventHandler(this.lstMetersAvailable_SelectedIndexChanged);
            this.p24_lstMetersAvailable.DoubleClick += new System.EventHandler(this.lstMetersAvailable_DoubleClick);

            // 
            // p24_lstMetersInUse
            // 
            this.p24_lstMetersInUse.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.p24_lstMetersInUse.FormattingEnabled = true;
            this.p24_lstMetersInUse.Location = new System.Drawing.Point(191, 174);
            this.p24_lstMetersInUse.Name = "p24_lstMetersInUse";
            this.p24_lstMetersInUse.Size = new System.Drawing.Size(140, 212);
            this.p24_lstMetersInUse.TabIndex = 91;
            this.p24_lstMetersInUse.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstMetersInUse_DrawItem);
            this.p24_lstMetersInUse.SelectedIndexChanged += new System.EventHandler(this.lstMetersInUse_SelectedIndexChanged);
            this.p24_lstMetersInUse.DoubleClick += new System.EventHandler(this.lstMetersInUse_DoubleClick);

            // 
            // p24_nudBandButtons_border
            // 
            this.p24_nudBandButtons_border.DecimalPlaces = 2;
            this.p24_nudBandButtons_border.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudBandButtons_border.Location = new System.Drawing.Point(78, 45);
            this.p24_nudBandButtons_border.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_border.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudBandButtons_border.Name = "p24_nudBandButtons_border";
            this.p24_nudBandButtons_border.Size = new System.Drawing.Size(56, 20);
            this.p24_nudBandButtons_border.TabIndex = 139;            this.toolTip1.SetToolTip(this.p24_nudBandButtons_border, "Border size");
            this.p24_nudBandButtons_border.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_border.ValueChanged += new System.EventHandler(this.nudBandButtons_border_ValueChanged);

            // 
            // p24_nudBandButtons_columns
            // 
            this.p24_nudBandButtons_columns.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_columns.Location = new System.Drawing.Point(78, 19);
            this.p24_nudBandButtons_columns.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.p24_nudBandButtons_columns.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_columns.Name = "p24_nudBandButtons_columns";
            this.p24_nudBandButtons_columns.Size = new System.Drawing.Size(56, 20);
            this.p24_nudBandButtons_columns.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudBandButtons_columns, "Number of button columns");
            this.p24_nudBandButtons_columns.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_columns.ValueChanged += new System.EventHandler(this.nudBandButtons_columns_ValueChanged);

            // 
            // p24_nudBandButtons_height_ratio
            // 
            this.p24_nudBandButtons_height_ratio.DecimalPlaces = 2;
            this.p24_nudBandButtons_height_ratio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudBandButtons_height_ratio.Location = new System.Drawing.Point(78, 123);
            this.p24_nudBandButtons_height_ratio.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.p24_nudBandButtons_height_ratio.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudBandButtons_height_ratio.Name = "p24_nudBandButtons_height_ratio";
            this.p24_nudBandButtons_height_ratio.Size = new System.Drawing.Size(56, 20);
            this.p24_nudBandButtons_height_ratio.TabIndex = 145;            this.toolTip1.SetToolTip(this.p24_nudBandButtons_height_ratio, "Ratio of height to width");
            this.p24_nudBandButtons_height_ratio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_height_ratio.ValueChanged += new System.EventHandler(this.nudBandButtons_height_ratio_ValueChanged);

            // 
            // p24_nudBandButtons_indicator_border
            // 
            this.p24_nudBandButtons_indicator_border.DecimalPlaces = 2;
            this.p24_nudBandButtons_indicator_border.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudBandButtons_indicator_border.Location = new System.Drawing.Point(91, 172);
            this.p24_nudBandButtons_indicator_border.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_indicator_border.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudBandButtons_indicator_border.Name = "p24_nudBandButtons_indicator_border";
            this.p24_nudBandButtons_indicator_border.Size = new System.Drawing.Size(56, 20);
            this.p24_nudBandButtons_indicator_border.TabIndex = 147;            this.toolTip1.SetToolTip(this.p24_nudBandButtons_indicator_border, "Border size of indicator ring");
            this.p24_nudBandButtons_indicator_border.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_indicator_border.ValueChanged += new System.EventHandler(this.nudBandButtons_indicator_border_ValueChanged);

            // 
            // p24_nudBandButtons_indicator_style
            // 
            this.p24_nudBandButtons_indicator_style.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_indicator_style.Location = new System.Drawing.Point(198, 172);
            this.p24_nudBandButtons_indicator_style.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.p24_nudBandButtons_indicator_style.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudBandButtons_indicator_style.Name = "p24_nudBandButtons_indicator_style";
            this.p24_nudBandButtons_indicator_style.Size = new System.Drawing.Size(37, 20);
            this.p24_nudBandButtons_indicator_style.TabIndex = 158;            this.toolTip1.SetToolTip(this.p24_nudBandButtons_indicator_style, "Indicator style");
            this.p24_nudBandButtons_indicator_style.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_indicator_style.ValueChanged += new System.EventHandler(this.nudBandButtons_indicator_style_ValueChanged);

            // 
            // p24_nudBandButtons_margin
            // 
            this.p24_nudBandButtons_margin.DecimalPlaces = 2;
            this.p24_nudBandButtons_margin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudBandButtons_margin.Location = new System.Drawing.Point(78, 71);
            this.p24_nudBandButtons_margin.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_margin.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudBandButtons_margin.Name = "p24_nudBandButtons_margin";
            this.p24_nudBandButtons_margin.Size = new System.Drawing.Size(56, 20);
            this.p24_nudBandButtons_margin.TabIndex = 141;            this.toolTip1.SetToolTip(this.p24_nudBandButtons_margin, "Margin size");
            this.p24_nudBandButtons_margin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_margin.ValueChanged += new System.EventHandler(this.nudBandButtons_margin_ValueChanged);

            // 
            // p24_nudBandButtons_radius
            // 
            this.p24_nudBandButtons_radius.DecimalPlaces = 2;
            this.p24_nudBandButtons_radius.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudBandButtons_radius.Location = new System.Drawing.Point(78, 97);
            this.p24_nudBandButtons_radius.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.p24_nudBandButtons_radius.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudBandButtons_radius.Name = "p24_nudBandButtons_radius";
            this.p24_nudBandButtons_radius.Size = new System.Drawing.Size(56, 20);
            this.p24_nudBandButtons_radius.TabIndex = 143;            this.toolTip1.SetToolTip(this.p24_nudBandButtons_radius, "Radius corner size");
            this.p24_nudBandButtons_radius.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudBandButtons_radius.ValueChanged += new System.EventHandler(this.nudBandButtons_radius_ValueChanged);

            // 
            // p24_nudButtonBox_font_scale
            // 
            this.p24_nudButtonBox_font_scale.DecimalPlaces = 2;
            this.p24_nudButtonBox_font_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudButtonBox_font_scale.Location = new System.Drawing.Point(241, 90);
            this.p24_nudButtonBox_font_scale.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.p24_nudButtonBox_font_scale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudButtonBox_font_scale.Name = "p24_nudButtonBox_font_scale";
            this.p24_nudButtonBox_font_scale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudButtonBox_font_scale.TabIndex = 161;            this.toolTip1.SetToolTip(this.p24_nudButtonBox_font_scale, "Font scale adjustment");
            this.p24_nudButtonBox_font_scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudButtonBox_font_scale.ValueChanged += new System.EventHandler(this.nudButtonBox_font_scale_ValueChanged);

            // 
            // p24_nudButtonBox_font_x_shift
            // 
            this.p24_nudButtonBox_font_x_shift.DecimalPlaces = 2;
            this.p24_nudButtonBox_font_x_shift.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudButtonBox_font_x_shift.Location = new System.Drawing.Point(241, 116);
            this.p24_nudButtonBox_font_x_shift.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.p24_nudButtonBox_font_x_shift.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            -2147352576});
            this.p24_nudButtonBox_font_x_shift.Name = "p24_nudButtonBox_font_x_shift";
            this.p24_nudButtonBox_font_x_shift.Size = new System.Drawing.Size(56, 20);
            this.p24_nudButtonBox_font_x_shift.TabIndex = 163;            this.toolTip1.SetToolTip(this.p24_nudButtonBox_font_x_shift, "Font x shift");
            this.p24_nudButtonBox_font_x_shift.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudButtonBox_font_x_shift.ValueChanged += new System.EventHandler(this.nudButtonBox_font_x_shift_ValueChanged);

            // 
            // p24_nudButtonBox_font_y_shift
            // 
            this.p24_nudButtonBox_font_y_shift.DecimalPlaces = 2;
            this.p24_nudButtonBox_font_y_shift.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudButtonBox_font_y_shift.Location = new System.Drawing.Point(241, 142);
            this.p24_nudButtonBox_font_y_shift.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.p24_nudButtonBox_font_y_shift.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            -2147352576});
            this.p24_nudButtonBox_font_y_shift.Name = "p24_nudButtonBox_font_y_shift";
            this.p24_nudButtonBox_font_y_shift.Size = new System.Drawing.Size(56, 20);
            this.p24_nudButtonBox_font_y_shift.TabIndex = 165;            this.toolTip1.SetToolTip(this.p24_nudButtonBox_font_y_shift, "Font y shift");
            this.p24_nudButtonBox_font_y_shift.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudButtonBox_font_y_shift.ValueChanged += new System.EventHandler(this.nudButtonBox_font_y_shift_ValueChanged);

            // 
            // p24_nudDataOutNode_sendinterval
            // 
            this.p24_nudDataOutNode_sendinterval.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudDataOutNode_sendinterval.Location = new System.Drawing.Point(93, 22);
            this.p24_nudDataOutNode_sendinterval.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.p24_nudDataOutNode_sendinterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.p24_nudDataOutNode_sendinterval.Name = "p24_nudDataOutNode_sendinterval";
            this.p24_nudDataOutNode_sendinterval.Size = new System.Drawing.Size(56, 20);
            this.p24_nudDataOutNode_sendinterval.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudDataOutNode_sendinterval, "The interval that the data is sent.");
            this.p24_nudDataOutNode_sendinterval.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.p24_nudDataOutNode_sendinterval.ValueChanged += new System.EventHandler(this.nudDataOutNode_sendinterval_ValueChanged);

            // 
            // p24_nudDialDisplay_font_scale
            // 
            this.p24_nudDialDisplay_font_scale.DecimalPlaces = 2;
            this.p24_nudDialDisplay_font_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudDialDisplay_font_scale.Location = new System.Drawing.Point(118, 39);
            this.p24_nudDialDisplay_font_scale.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            65536});
            this.p24_nudDialDisplay_font_scale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudDialDisplay_font_scale.Name = "p24_nudDialDisplay_font_scale";
            this.p24_nudDialDisplay_font_scale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudDialDisplay_font_scale.TabIndex = 163;            this.toolTip1.SetToolTip(this.p24_nudDialDisplay_font_scale, "Font scale adjustment");
            this.p24_nudDialDisplay_font_scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDialDisplay_font_scale.ValueChanged += new System.EventHandler(this.nudDialDisplay_font_scale_ValueChanged);

            // 
            // p24_nudDialDisplay_vertical_ratio
            // 
            this.p24_nudDialDisplay_vertical_ratio.DecimalPlaces = 3;
            this.p24_nudDialDisplay_vertical_ratio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudDialDisplay_vertical_ratio.Location = new System.Drawing.Point(118, 16);
            this.p24_nudDialDisplay_vertical_ratio.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDialDisplay_vertical_ratio.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            131072});
            this.p24_nudDialDisplay_vertical_ratio.Name = "p24_nudDialDisplay_vertical_ratio";
            this.p24_nudDialDisplay_vertical_ratio.Size = new System.Drawing.Size(56, 20);
            this.p24_nudDialDisplay_vertical_ratio.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudDialDisplay_vertical_ratio, "Vertical size, compared to width");
            this.p24_nudDialDisplay_vertical_ratio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDialDisplay_vertical_ratio.ValueChanged += new System.EventHandler(this.nudDialDisplay_vertical_ratio_ValueChanged);

            // 
            // p24_nudDial_decrement
            // 
            this.p24_nudDial_decrement.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_decrement.Location = new System.Drawing.Point(145, 134);
            this.p24_nudDial_decrement.Maximum = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.p24_nudDial_decrement.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.p24_nudDial_decrement.Name = "p24_nudDial_decrement";
            this.p24_nudDial_decrement.Size = new System.Drawing.Size(44, 20);
            this.p24_nudDial_decrement.TabIndex = 171;            this.toolTip1.SetToolTip(this.p24_nudDial_decrement, "At this degree per second tunstep will be decremented");
            this.p24_nudDial_decrement.Value = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.p24_nudDial_decrement.ValueChanged += new System.EventHandler(this.nudDial_decrement_ValueChanged);

            // 
            // p24_nudDial_degrees_for_change
            // 
            this.p24_nudDial_degrees_for_change.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_degrees_for_change.Location = new System.Drawing.Point(145, 206);
            this.p24_nudDial_degrees_for_change.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.p24_nudDial_degrees_for_change.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_degrees_for_change.Name = "p24_nudDial_degrees_for_change";
            this.p24_nudDial_degrees_for_change.Size = new System.Drawing.Size(44, 20);
            this.p24_nudDial_degrees_for_change.TabIndex = 200;            this.toolTip1.SetToolTip(this.p24_nudDial_degrees_for_change, "The number of degrees required for a VFO change");
            this.p24_nudDial_degrees_for_change.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.p24_nudDial_degrees_for_change.ValueChanged += new System.EventHandler(this.nudDial_degrees_for_change_ValueChanged);

            // 
            // p24_nudDial_increment
            // 
            this.p24_nudDial_increment.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_increment.Location = new System.Drawing.Point(145, 110);
            this.p24_nudDial_increment.Maximum = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.p24_nudDial_increment.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.p24_nudDial_increment.Name = "p24_nudDial_increment";
            this.p24_nudDial_increment.Size = new System.Drawing.Size(44, 20);
            this.p24_nudDial_increment.TabIndex = 168;            this.toolTip1.SetToolTip(this.p24_nudDial_increment, "At this degree per second tunstep will be incremented");
            this.p24_nudDial_increment.Value = new decimal(new int[] {
            540,
            0,
            0,
            0});
            this.p24_nudDial_increment.ValueChanged += new System.EventHandler(this.nudDial_increment_ValueChanged);

            // 
            // p24_nudDial_interval
            // 
            this.p24_nudDial_interval.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_interval.Location = new System.Drawing.Point(145, 158);
            this.p24_nudDial_interval.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudDial_interval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_interval.Name = "p24_nudDial_interval";
            this.p24_nudDial_interval.Size = new System.Drawing.Size(44, 20);
            this.p24_nudDial_interval.TabIndex = 174;            this.toolTip1.SetToolTip(this.p24_nudDial_interval, "Apply a tunestep change at this interval");
            this.p24_nudDial_interval.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.p24_nudDial_interval.ValueChanged += new System.EventHandler(this.nudDial_interval_ValueChanged);

            // 
            // p24_nudDial_max_increments
            // 
            this.p24_nudDial_max_increments.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_max_increments.Location = new System.Drawing.Point(145, 182);
            this.p24_nudDial_max_increments.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.p24_nudDial_max_increments.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudDial_max_increments.Name = "p24_nudDial_max_increments";
            this.p24_nudDial_max_increments.Size = new System.Drawing.Size(44, 20);
            this.p24_nudDial_max_increments.TabIndex = 198;            this.toolTip1.SetToolTip(this.p24_nudDial_max_increments, "The maximum number of tunestep increments that can happen");
            this.p24_nudDial_max_increments.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.p24_nudDial_max_increments.ValueChanged += new System.EventHandler(this.nudDial_max_increments_ValueChanged);

            // 
            // p24_nudFilterDisplay_fixed_tx_zoom_level
            // 
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.DecimalPlaces = 2;
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.Location = new System.Drawing.Point(126, 110);
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.Name = "p24_nudFilterDisplay_fixed_tx_zoom_level";
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.Size = new System.Drawing.Size(56, 20);
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.TabIndex = 142;            this.toolTip1.SetToolTip(this.p24_nudFilterDisplay_fixed_tx_zoom_level, "Scale");
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_fixed_tx_zoom_level.ValueChanged += new System.EventHandler(this.nudFilterDisplay_fixed_tx_zoom_level_ValueChanged);

            // 
            // p24_nudFilterDisplay_fixed_zoom_level
            // 
            this.p24_nudFilterDisplay_fixed_zoom_level.DecimalPlaces = 2;
            this.p24_nudFilterDisplay_fixed_zoom_level.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudFilterDisplay_fixed_zoom_level.Location = new System.Drawing.Point(126, 84);
            this.p24_nudFilterDisplay_fixed_zoom_level.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_fixed_zoom_level.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_fixed_zoom_level.Name = "p24_nudFilterDisplay_fixed_zoom_level";
            this.p24_nudFilterDisplay_fixed_zoom_level.Size = new System.Drawing.Size(56, 20);
            this.p24_nudFilterDisplay_fixed_zoom_level.TabIndex = 140;            this.toolTip1.SetToolTip(this.p24_nudFilterDisplay_fixed_zoom_level, "Scale");
            this.p24_nudFilterDisplay_fixed_zoom_level.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_fixed_zoom_level.ValueChanged += new System.EventHandler(this.nudFilterDisplay_fixed_zoom_level_ValueChanged);

            // 
            // p24_nudFilterDisplay_vertical_ratio
            // 
            this.p24_nudFilterDisplay_vertical_ratio.DecimalPlaces = 3;
            this.p24_nudFilterDisplay_vertical_ratio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudFilterDisplay_vertical_ratio.Location = new System.Drawing.Point(118, 16);
            this.p24_nudFilterDisplay_vertical_ratio.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_vertical_ratio.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            131072});
            this.p24_nudFilterDisplay_vertical_ratio.Name = "p24_nudFilterDisplay_vertical_ratio";
            this.p24_nudFilterDisplay_vertical_ratio.Size = new System.Drawing.Size(56, 20);
            this.p24_nudFilterDisplay_vertical_ratio.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudFilterDisplay_vertical_ratio, "Vertical size, compared to width");
            this.p24_nudFilterDisplay_vertical_ratio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilterDisplay_vertical_ratio.ValueChanged += new System.EventHandler(this.nudFilterDisplay_vertical_ratio_ValueChanged);

            // 
            // p24_nudFilterItem_cw_scale
            // 
            this.p24_nudFilterItem_cw_scale.DecimalPlaces = 2;
            this.p24_nudFilterItem_cw_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudFilterItem_cw_scale.Location = new System.Drawing.Point(58, 31);
            this.p24_nudFilterItem_cw_scale.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudFilterItem_cw_scale.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudFilterItem_cw_scale.Name = "p24_nudFilterItem_cw_scale";
            this.p24_nudFilterItem_cw_scale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudFilterItem_cw_scale.TabIndex = 142;            this.toolTip1.SetToolTip(this.p24_nudFilterItem_cw_scale, "Mode scale");
            this.p24_nudFilterItem_cw_scale.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudFilterItem_cw_scale.ValueChanged += new System.EventHandler(this.nudFilterItem_cw_scale_ValueChanged);

            // 
            // p24_nudFilterItem_font_scale
            // 
            this.p24_nudFilterItem_font_scale.DecimalPlaces = 2;
            this.p24_nudFilterItem_font_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudFilterItem_font_scale.Location = new System.Drawing.Point(118, 39);
            this.p24_nudFilterItem_font_scale.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.p24_nudFilterItem_font_scale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudFilterItem_font_scale.Name = "p24_nudFilterItem_font_scale";
            this.p24_nudFilterItem_font_scale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudFilterItem_font_scale.TabIndex = 163;            this.toolTip1.SetToolTip(this.p24_nudFilterItem_font_scale, "Font scale adjustment");
            this.p24_nudFilterItem_font_scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilterItem_font_scale.ValueChanged += new System.EventHandler(this.nudFilterItem_font_scale_ValueChanged);

            // 
            // p24_nudFilterItem_others_scale
            // 
            this.p24_nudFilterItem_others_scale.DecimalPlaces = 2;
            this.p24_nudFilterItem_others_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudFilterItem_others_scale.Location = new System.Drawing.Point(58, 53);
            this.p24_nudFilterItem_others_scale.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudFilterItem_others_scale.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudFilterItem_others_scale.Name = "p24_nudFilterItem_others_scale";
            this.p24_nudFilterItem_others_scale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudFilterItem_others_scale.TabIndex = 143;            this.toolTip1.SetToolTip(this.p24_nudFilterItem_others_scale, "Mode scale");
            this.p24_nudFilterItem_others_scale.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudFilterItem_others_scale.ValueChanged += new System.EventHandler(this.nudFilterItem_others_scale_ValueChanged);

            // 
            // p24_nudFilterItem_sidebands_scale
            // 
            this.p24_nudFilterItem_sidebands_scale.DecimalPlaces = 2;
            this.p24_nudFilterItem_sidebands_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudFilterItem_sidebands_scale.Location = new System.Drawing.Point(58, 9);
            this.p24_nudFilterItem_sidebands_scale.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudFilterItem_sidebands_scale.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudFilterItem_sidebands_scale.Name = "p24_nudFilterItem_sidebands_scale";
            this.p24_nudFilterItem_sidebands_scale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudFilterItem_sidebands_scale.TabIndex = 141;            this.toolTip1.SetToolTip(this.p24_nudFilterItem_sidebands_scale, "Mode scale");
            this.p24_nudFilterItem_sidebands_scale.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudFilterItem_sidebands_scale.ValueChanged += new System.EventHandler(this.nudFilterItem_sidebands_scale_ValueChanged);

            // 
            // p24_nudFilter_lower_characteristic
            // 
            this.p24_nudFilter_lower_characteristic.DecimalPlaces = 1;
            this.p24_nudFilter_lower_characteristic.Enabled = false;
            this.p24_nudFilter_lower_characteristic.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilter_lower_characteristic.Location = new System.Drawing.Point(233, 205);
            this.p24_nudFilter_lower_characteristic.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
            this.p24_nudFilter_lower_characteristic.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            -2147483648});
            this.p24_nudFilter_lower_characteristic.Name = "p24_nudFilter_lower_characteristic";
            this.p24_nudFilter_lower_characteristic.Size = new System.Drawing.Size(50, 20);
            this.p24_nudFilter_lower_characteristic.TabIndex = 202;            this.toolTip1.SetToolTip(this.p24_nudFilter_lower_characteristic, "The lower level of the characteristic plot");
            this.p24_nudFilter_lower_characteristic.Value = new decimal(new int[] {
            250,
            0,
            0,
            -2147483648});
            this.p24_nudFilter_lower_characteristic.ValueChanged += new System.EventHandler(this.nudFilter_lower_characteristic_ValueChanged);

            // 
            // p24_nudFilter_waterfall_frame_update
            // 
            this.p24_nudFilter_waterfall_frame_update.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilter_waterfall_frame_update.Location = new System.Drawing.Point(237, 164);
            this.p24_nudFilter_waterfall_frame_update.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.p24_nudFilter_waterfall_frame_update.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudFilter_waterfall_frame_update.Name = "p24_nudFilter_waterfall_frame_update";
            this.p24_nudFilter_waterfall_frame_update.Size = new System.Drawing.Size(47, 20);
            this.p24_nudFilter_waterfall_frame_update.TabIndex = 190;            this.toolTip1.SetToolTip(this.p24_nudFilter_waterfall_frame_update, "How often to update (scroll another pixel line) on the waterfall display.\r\nFilter" +
        "s have a fixed fps of 30, so a setting of 1 here will be adding a row\r\nevery fra" +
        "me.");
            this.p24_nudFilter_waterfall_frame_update.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.p24_nudFilter_waterfall_frame_update.ValueChanged += new System.EventHandler(this.nudFilter_waterfall_frame_update_ValueChanged);

            // 
            // p24_nudHistory_axis0_max
            // 
            this.p24_nudHistory_axis0_max.DecimalPlaces = 1;
            this.p24_nudHistory_axis0_max.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudHistory_axis0_max.Location = new System.Drawing.Point(187, 73);
            this.p24_nudHistory_axis0_max.Maximum = new decimal(new int[] {
            40000,
            0,
            0,
            0});
            this.p24_nudHistory_axis0_max.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            -2147483648});
            this.p24_nudHistory_axis0_max.Name = "p24_nudHistory_axis0_max";
            this.p24_nudHistory_axis0_max.Size = new System.Drawing.Size(56, 20);
            this.p24_nudHistory_axis0_max.TabIndex = 143;            this.toolTip1.SetToolTip(this.p24_nudHistory_axis0_max, "Max scale");
            this.p24_nudHistory_axis0_max.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudHistory_axis0_max.ValueChanged += new System.EventHandler(this.nudHistory_axis0_max_ValueChanged);

            // 
            // p24_nudHistory_axis0_min
            // 
            this.p24_nudHistory_axis0_min.DecimalPlaces = 1;
            this.p24_nudHistory_axis0_min.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudHistory_axis0_min.Location = new System.Drawing.Point(73, 73);
            this.p24_nudHistory_axis0_min.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.p24_nudHistory_axis0_min.Minimum = new decimal(new int[] {
            40000,
            0,
            0,
            -2147483648});
            this.p24_nudHistory_axis0_min.Name = "p24_nudHistory_axis0_min";
            this.p24_nudHistory_axis0_min.Size = new System.Drawing.Size(56, 20);
            this.p24_nudHistory_axis0_min.TabIndex = 141;            this.toolTip1.SetToolTip(this.p24_nudHistory_axis0_min, "Min scale");
            this.p24_nudHistory_axis0_min.Value = new decimal(new int[] {
            150,
            0,
            0,
            -2147483648});
            this.p24_nudHistory_axis0_min.ValueChanged += new System.EventHandler(this.nudHistory_axis0_min_ValueChanged);

            // 
            // p24_nudHistory_axis1_max
            // 
            this.p24_nudHistory_axis1_max.DecimalPlaces = 1;
            this.p24_nudHistory_axis1_max.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudHistory_axis1_max.Location = new System.Drawing.Point(187, 73);
            this.p24_nudHistory_axis1_max.Maximum = new decimal(new int[] {
            40000,
            0,
            0,
            0});
            this.p24_nudHistory_axis1_max.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            -2147483648});
            this.p24_nudHistory_axis1_max.Name = "p24_nudHistory_axis1_max";
            this.p24_nudHistory_axis1_max.Size = new System.Drawing.Size(56, 20);
            this.p24_nudHistory_axis1_max.TabIndex = 143;            this.toolTip1.SetToolTip(this.p24_nudHistory_axis1_max, "Max scale");
            this.p24_nudHistory_axis1_max.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudHistory_axis1_max.ValueChanged += new System.EventHandler(this.nudHistory_axis1_max_ValueChanged);

            // 
            // p24_nudHistory_axis1_min
            // 
            this.p24_nudHistory_axis1_min.DecimalPlaces = 1;
            this.p24_nudHistory_axis1_min.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudHistory_axis1_min.Location = new System.Drawing.Point(73, 73);
            this.p24_nudHistory_axis1_min.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.p24_nudHistory_axis1_min.Minimum = new decimal(new int[] {
            40000,
            0,
            0,
            -2147483648});
            this.p24_nudHistory_axis1_min.Name = "p24_nudHistory_axis1_min";
            this.p24_nudHistory_axis1_min.Size = new System.Drawing.Size(56, 20);
            this.p24_nudHistory_axis1_min.TabIndex = 141;            this.toolTip1.SetToolTip(this.p24_nudHistory_axis1_min, "Min scale");
            this.p24_nudHistory_axis1_min.Value = new decimal(new int[] {
            150,
            0,
            0,
            -2147483648});
            this.p24_nudHistory_axis1_min.ValueChanged += new System.EventHandler(this.nudHistory_axis1_min_ValueChanged);

            // 
            // p24_nudHistory_keep_for
            // 
            this.p24_nudHistory_keep_for.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudHistory_keep_for.Location = new System.Drawing.Point(118, 78);
            this.p24_nudHistory_keep_for.Maximum = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            this.p24_nudHistory_keep_for.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudHistory_keep_for.Name = "p24_nudHistory_keep_for";
            this.p24_nudHistory_keep_for.Size = new System.Drawing.Size(56, 20);
            this.p24_nudHistory_keep_for.TabIndex = 136;            this.toolTip1.SetToolTip(this.p24_nudHistory_keep_for, "Reading update and is related to screen update");
            this.p24_nudHistory_keep_for.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.p24_nudHistory_keep_for.ValueChanged += new System.EventHandler(this.nudHistory_keep_for_ValueChanged);

            // 
            // p24_nudHistory_update
            // 
            this.p24_nudHistory_update.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudHistory_update.Location = new System.Drawing.Point(118, 52);
            this.p24_nudHistory_update.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.p24_nudHistory_update.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.p24_nudHistory_update.Name = "p24_nudHistory_update";
            this.p24_nudHistory_update.Size = new System.Drawing.Size(56, 20);
            this.p24_nudHistory_update.TabIndex = 134;            this.toolTip1.SetToolTip(this.p24_nudHistory_update, "Reading update and is related to screen update");
            this.p24_nudHistory_update.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.p24_nudHistory_update.ValueChanged += new System.EventHandler(this.nudHistory_update_ValueChanged);

            // 
            // p24_nudHistory_vertical_ratio
            // 
            this.p24_nudHistory_vertical_ratio.DecimalPlaces = 3;
            this.p24_nudHistory_vertical_ratio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudHistory_vertical_ratio.Location = new System.Drawing.Point(118, 26);
            this.p24_nudHistory_vertical_ratio.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudHistory_vertical_ratio.Minimum = new decimal(new int[] {
            130,
            0,
            0,
            196608});
            this.p24_nudHistory_vertical_ratio.Name = "p24_nudHistory_vertical_ratio";
            this.p24_nudHistory_vertical_ratio.Size = new System.Drawing.Size(56, 20);
            this.p24_nudHistory_vertical_ratio.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudHistory_vertical_ratio, "Vertical size, compared to width");
            this.p24_nudHistory_vertical_ratio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudHistory_vertical_ratio.ValueChanged += new System.EventHandler(this.nudHistory_vertical_ratio_ValueChanged);

            // 
            // p24_nudLedIndicator_PanelPadding
            // 
            this.p24_nudLedIndicator_PanelPadding.DecimalPlaces = 3;
            this.p24_nudLedIndicator_PanelPadding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudLedIndicator_PanelPadding.Location = new System.Drawing.Point(125, 100);
            this.p24_nudLedIndicator_PanelPadding.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudLedIndicator_PanelPadding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudLedIndicator_PanelPadding.Name = "p24_nudLedIndicator_PanelPadding";
            this.p24_nudLedIndicator_PanelPadding.Size = new System.Drawing.Size(56, 20);
            this.p24_nudLedIndicator_PanelPadding.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudLedIndicator_PanelPadding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudLedIndicator_PanelPadding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudLedIndicator_PanelPadding.ValueChanged += new System.EventHandler(this.nudLedIndicator_PanelPadding_ValueChanged);

            // 
            // p24_nudLedIndicator_UpdateInterval
            // 
            this.p24_nudLedIndicator_UpdateInterval.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudLedIndicator_UpdateInterval.Location = new System.Drawing.Point(254, 73);
            this.p24_nudLedIndicator_UpdateInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.p24_nudLedIndicator_UpdateInterval.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.p24_nudLedIndicator_UpdateInterval.Name = "p24_nudLedIndicator_UpdateInterval";
            this.p24_nudLedIndicator_UpdateInterval.Size = new System.Drawing.Size(56, 20);
            this.p24_nudLedIndicator_UpdateInterval.TabIndex = 176;            this.toolTip1.SetToolTip(this.p24_nudLedIndicator_UpdateInterval, "Reading update and is related to screen update");
            this.p24_nudLedIndicator_UpdateInterval.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.p24_nudLedIndicator_UpdateInterval.ValueChanged += new System.EventHandler(this.nudLedIndicator_UpdateInterval_ValueChanged);

            // 
            // p24_nudLedIndicator_xOffset
            // 
            this.p24_nudLedIndicator_xOffset.DecimalPlaces = 3;
            this.p24_nudLedIndicator_xOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudLedIndicator_xOffset.Location = new System.Drawing.Point(99, 250);
            this.p24_nudLedIndicator_xOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudLedIndicator_xOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudLedIndicator_xOffset.Name = "p24_nudLedIndicator_xOffset";
            this.p24_nudLedIndicator_xOffset.Size = new System.Drawing.Size(56, 20);
            this.p24_nudLedIndicator_xOffset.TabIndex = 143;            this.toolTip1.SetToolTip(this.p24_nudLedIndicator_xOffset, "X offset");
            this.p24_nudLedIndicator_xOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudLedIndicator_xOffset.ValueChanged += new System.EventHandler(this.nudLedIndicator_xOffset_ValueChanged);

            // 
            // p24_nudLedIndicator_xSize
            // 
            this.p24_nudLedIndicator_xSize.DecimalPlaces = 3;
            this.p24_nudLedIndicator_xSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudLedIndicator_xSize.Location = new System.Drawing.Point(99, 302);
            this.p24_nudLedIndicator_xSize.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudLedIndicator_xSize.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudLedIndicator_xSize.Name = "p24_nudLedIndicator_xSize";
            this.p24_nudLedIndicator_xSize.Size = new System.Drawing.Size(56, 20);
            this.p24_nudLedIndicator_xSize.TabIndex = 150;            this.toolTip1.SetToolTip(this.p24_nudLedIndicator_xSize, "Led size X");
            this.p24_nudLedIndicator_xSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudLedIndicator_xSize.ValueChanged += new System.EventHandler(this.nudLedIndicator_xSize_ValueChanged);

            // 
            // p24_nudLedIndicator_yOffset
            // 
            this.p24_nudLedIndicator_yOffset.DecimalPlaces = 3;
            this.p24_nudLedIndicator_yOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudLedIndicator_yOffset.Location = new System.Drawing.Point(99, 276);
            this.p24_nudLedIndicator_yOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudLedIndicator_yOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudLedIndicator_yOffset.Name = "p24_nudLedIndicator_yOffset";
            this.p24_nudLedIndicator_yOffset.Size = new System.Drawing.Size(56, 20);
            this.p24_nudLedIndicator_yOffset.TabIndex = 147;            this.toolTip1.SetToolTip(this.p24_nudLedIndicator_yOffset, "Y offset");
            this.p24_nudLedIndicator_yOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudLedIndicator_yOffset.ValueChanged += new System.EventHandler(this.nudLedIndicator_yOffset_ValueChanged);

            // 
            // p24_nudLedIndicator_ySize
            // 
            this.p24_nudLedIndicator_ySize.DecimalPlaces = 3;
            this.p24_nudLedIndicator_ySize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudLedIndicator_ySize.Location = new System.Drawing.Point(99, 328);
            this.p24_nudLedIndicator_ySize.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudLedIndicator_ySize.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudLedIndicator_ySize.Name = "p24_nudLedIndicator_ySize";
            this.p24_nudLedIndicator_ySize.Size = new System.Drawing.Size(56, 20);
            this.p24_nudLedIndicator_ySize.TabIndex = 152;            this.toolTip1.SetToolTip(this.p24_nudLedIndicator_ySize, "Led size Y");
            this.p24_nudLedIndicator_ySize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudLedIndicator_ySize.ValueChanged += new System.EventHandler(this.nudLedIndicator_ySize_ValueChanged);

            // 
            // p24_nudMeterItemAttackRate
            // 
            this.p24_nudMeterItemAttackRate.DecimalPlaces = 3;
            this.p24_nudMeterItemAttackRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.p24_nudMeterItemAttackRate.Location = new System.Drawing.Point(83, 45);
            this.p24_nudMeterItemAttackRate.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemAttackRate.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudMeterItemAttackRate.Name = "p24_nudMeterItemAttackRate";
            this.p24_nudMeterItemAttackRate.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemAttackRate.TabIndex = 103;            this.toolTip1.SetToolTip(this.p24_nudMeterItemAttackRate, "The \'speed of rise\' to the new value if above current");
            this.p24_nudMeterItemAttackRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemAttackRate.ValueChanged += new System.EventHandler(this.nudMeterItemAttackRate_ValueChanged);

            // 
            // p24_nudMeterItemDecayRate
            // 
            this.p24_nudMeterItemDecayRate.DecimalPlaces = 3;
            this.p24_nudMeterItemDecayRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.p24_nudMeterItemDecayRate.Location = new System.Drawing.Point(83, 70);
            this.p24_nudMeterItemDecayRate.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemDecayRate.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudMeterItemDecayRate.Name = "p24_nudMeterItemDecayRate";
            this.p24_nudMeterItemDecayRate.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemDecayRate.TabIndex = 105;            this.toolTip1.SetToolTip(this.p24_nudMeterItemDecayRate, "The \'speed of fall\' to the new value if below current");
            this.p24_nudMeterItemDecayRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemDecayRate.ValueChanged += new System.EventHandler(this.nudMeterItemDecayRate_ValueChanged);

            // 
            // p24_nudMeterItemEyeBezelScale
            // 
            this.p24_nudMeterItemEyeBezelScale.DecimalPlaces = 2;
            this.p24_nudMeterItemEyeBezelScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudMeterItemEyeBezelScale.Location = new System.Drawing.Point(69, 241);
            this.p24_nudMeterItemEyeBezelScale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemEyeBezelScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudMeterItemEyeBezelScale.Name = "p24_nudMeterItemEyeBezelScale";
            this.p24_nudMeterItemEyeBezelScale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemEyeBezelScale.TabIndex = 124;            this.toolTip1.SetToolTip(this.p24_nudMeterItemEyeBezelScale, "Size of the eye bezel, 1.0 is full width of container");
            this.p24_nudMeterItemEyeBezelScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemEyeBezelScale.ValueChanged += new System.EventHandler(this.nudMeterItemEyeBezelScale_ValueChanged);

            // 
            // p24_nudMeterItemEyeScale
            // 
            this.p24_nudMeterItemEyeScale.DecimalPlaces = 2;
            this.p24_nudMeterItemEyeScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudMeterItemEyeScale.Location = new System.Drawing.Point(69, 219);
            this.p24_nudMeterItemEyeScale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemEyeScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudMeterItemEyeScale.Name = "p24_nudMeterItemEyeScale";
            this.p24_nudMeterItemEyeScale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemEyeScale.TabIndex = 110;            this.toolTip1.SetToolTip(this.p24_nudMeterItemEyeScale, "Size of the eye, 1.0 is full width of container");
            this.p24_nudMeterItemEyeScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemEyeScale.ValueChanged += new System.EventHandler(this.nudMeterItemEyeScale_ValueChanged);

            // 
            // p24_nudMeterItemHistoryDuration
            // 
            this.p24_nudMeterItemHistoryDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemHistoryDuration.Location = new System.Drawing.Point(233, 56);
            this.p24_nudMeterItemHistoryDuration.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.p24_nudMeterItemHistoryDuration.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.p24_nudMeterItemHistoryDuration.Name = "p24_nudMeterItemHistoryDuration";
            this.p24_nudMeterItemHistoryDuration.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemHistoryDuration.TabIndex = 98;            this.toolTip1.SetToolTip(this.p24_nudMeterItemHistoryDuration, "History duration for history display and peak hold");
            this.p24_nudMeterItemHistoryDuration.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.p24_nudMeterItemHistoryDuration.ValueChanged += new System.EventHandler(this.nudMeterItemHistoryDuration_ValueChanged);

            // 
            // p24_nudMeterItemIgnoreHistoryDuration
            // 
            this.p24_nudMeterItemIgnoreHistoryDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemIgnoreHistoryDuration.Location = new System.Drawing.Point(233, 79);
            this.p24_nudMeterItemIgnoreHistoryDuration.Maximum = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.p24_nudMeterItemIgnoreHistoryDuration.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudMeterItemIgnoreHistoryDuration.Name = "p24_nudMeterItemIgnoreHistoryDuration";
            this.p24_nudMeterItemIgnoreHistoryDuration.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemIgnoreHistoryDuration.TabIndex = 127;            this.toolTip1.SetToolTip(this.p24_nudMeterItemIgnoreHistoryDuration, "When rx/tx transition or band change let meters settle before gathering history/p" +
        "eak values");
            this.p24_nudMeterItemIgnoreHistoryDuration.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.p24_nudMeterItemIgnoreHistoryDuration.ValueChanged += new System.EventHandler(this.nudMeterItemIgnoreHistoryDuration_ValueChanged);

            // 
            // p24_nudMeterItemRotatorBeamWidth
            // 
            this.p24_nudMeterItemRotatorBeamWidth.DecimalPlaces = 1;
            this.p24_nudMeterItemRotatorBeamWidth.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.p24_nudMeterItemRotatorBeamWidth.Location = new System.Drawing.Point(83, 167);
            this.p24_nudMeterItemRotatorBeamWidth.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.p24_nudMeterItemRotatorBeamWidth.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudMeterItemRotatorBeamWidth.Name = "p24_nudMeterItemRotatorBeamWidth";
            this.p24_nudMeterItemRotatorBeamWidth.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemRotatorBeamWidth.TabIndex = 131;            this.toolTip1.SetToolTip(this.p24_nudMeterItemRotatorBeamWidth, "3dB beam width");
            this.p24_nudMeterItemRotatorBeamWidth.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudMeterItemRotatorBeamWidth.ValueChanged += new System.EventHandler(this.nudMeterItemRotatorBeamWidth_ValueChanged);

            // 
            // p24_nudMeterItemRotatorBeamWidth_alpha
            // 
            this.p24_nudMeterItemRotatorBeamWidth_alpha.DecimalPlaces = 2;
            this.p24_nudMeterItemRotatorBeamWidth_alpha.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudMeterItemRotatorBeamWidth_alpha.Location = new System.Drawing.Point(196, 167);
            this.p24_nudMeterItemRotatorBeamWidth_alpha.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemRotatorBeamWidth_alpha.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudMeterItemRotatorBeamWidth_alpha.Name = "p24_nudMeterItemRotatorBeamWidth_alpha";
            this.p24_nudMeterItemRotatorBeamWidth_alpha.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemRotatorBeamWidth_alpha.TabIndex = 175;            this.toolTip1.SetToolTip(this.p24_nudMeterItemRotatorBeamWidth_alpha, "3dB beam width");
            this.p24_nudMeterItemRotatorBeamWidth_alpha.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudMeterItemRotatorBeamWidth_alpha.ValueChanged += new System.EventHandler(this.nudMeterItemRotatorBeamWidth_alpha_ValueChanged);

            // 
            // p24_nudMeterItemRotator_padding
            // 
            this.p24_nudMeterItemRotator_padding.DecimalPlaces = 3;
            this.p24_nudMeterItemRotator_padding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudMeterItemRotator_padding.Location = new System.Drawing.Point(222, 223);
            this.p24_nudMeterItemRotator_padding.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemRotator_padding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudMeterItemRotator_padding.Name = "p24_nudMeterItemRotator_padding";
            this.p24_nudMeterItemRotator_padding.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemRotator_padding.TabIndex = 172;            this.toolTip1.SetToolTip(this.p24_nudMeterItemRotator_padding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudMeterItemRotator_padding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemRotator_padding.ValueChanged += new System.EventHandler(this.nudMeterItemRotator_padding_ValueChanged);

            // 
            // p24_nudMeterItemSpacerPadding
            // 
            this.p24_nudMeterItemSpacerPadding.DecimalPlaces = 3;
            this.p24_nudMeterItemSpacerPadding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudMeterItemSpacerPadding.Location = new System.Drawing.Point(102, 138);
            this.p24_nudMeterItemSpacerPadding.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudMeterItemSpacerPadding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudMeterItemSpacerPadding.Name = "p24_nudMeterItemSpacerPadding";
            this.p24_nudMeterItemSpacerPadding.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemSpacerPadding.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudMeterItemSpacerPadding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudMeterItemSpacerPadding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemSpacerPadding.ValueChanged += new System.EventHandler(this.nudMeterItemSpacerPadding_ValueChanged);

            // 
            // p24_nudMeterItemUpdateRate
            // 
            this.p24_nudMeterItemUpdateRate.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRate.Location = new System.Drawing.Point(83, 20);
            this.p24_nudMeterItemUpdateRate.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRate.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRate.Name = "p24_nudMeterItemUpdateRate";
            this.p24_nudMeterItemUpdateRate.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemUpdateRate.TabIndex = 101;            this.toolTip1.SetToolTip(this.p24_nudMeterItemUpdateRate, "Reading update and is related to screen update");
            this.p24_nudMeterItemUpdateRate.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRate.ValueChanged += new System.EventHandler(this.nudMeterItemUpdateRate_ValueChanged);

            // 
            // p24_nudMeterItemUpdateRateRotator
            // 
            this.p24_nudMeterItemUpdateRateRotator.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRateRotator.Location = new System.Drawing.Point(83, 20);
            this.p24_nudMeterItemUpdateRateRotator.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRateRotator.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRateRotator.Name = "p24_nudMeterItemUpdateRateRotator";
            this.p24_nudMeterItemUpdateRateRotator.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemUpdateRateRotator.TabIndex = 101;            this.toolTip1.SetToolTip(this.p24_nudMeterItemUpdateRateRotator, "Reading update and is related to screen update");
            this.p24_nudMeterItemUpdateRateRotator.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.p24_nudMeterItemUpdateRateRotator.ValueChanged += new System.EventHandler(this.nudMeterItemUpdateRateRotator_ValueChanged);

            // 
            // p24_nudMeterItem_custom_high
            // 
            this.p24_nudMeterItem_custom_high.DecimalPlaces = 1;
            this.p24_nudMeterItem_custom_high.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudMeterItem_custom_high.Location = new System.Drawing.Point(54, 55);
            this.p24_nudMeterItem_custom_high.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.p24_nudMeterItem_custom_high.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.p24_nudMeterItem_custom_high.Name = "p24_nudMeterItem_custom_high";
            this.p24_nudMeterItem_custom_high.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItem_custom_high.TabIndex = 111;            this.p24_nudMeterItem_custom_high.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItem_custom_high.ValueChanged += new System.EventHandler(this.nudMeterItem_custom_high_ValueChanged);

            // 
            // p24_nudMeterItem_custom_max
            // 
            this.p24_nudMeterItem_custom_max.DecimalPlaces = 1;
            this.p24_nudMeterItem_custom_max.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudMeterItem_custom_max.Location = new System.Drawing.Point(54, 29);
            this.p24_nudMeterItem_custom_max.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.p24_nudMeterItem_custom_max.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.p24_nudMeterItem_custom_max.Name = "p24_nudMeterItem_custom_max";
            this.p24_nudMeterItem_custom_max.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItem_custom_max.TabIndex = 107;            this.p24_nudMeterItem_custom_max.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItem_custom_max.ValueChanged += new System.EventHandler(this.nudMeterItem_custom_max_ValueChanged);

            // 
            // p24_nudMeterItem_custom_min
            // 
            this.p24_nudMeterItem_custom_min.DecimalPlaces = 1;
            this.p24_nudMeterItem_custom_min.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudMeterItem_custom_min.Location = new System.Drawing.Point(54, 3);
            this.p24_nudMeterItem_custom_min.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.p24_nudMeterItem_custom_min.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            -2147483648});
            this.p24_nudMeterItem_custom_min.Name = "p24_nudMeterItem_custom_min";
            this.p24_nudMeterItem_custom_min.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItem_custom_min.TabIndex = 105;            this.p24_nudMeterItem_custom_min.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudMeterItem_custom_min.ValueChanged += new System.EventHandler(this.nudMeterItem_custom_min_ValueChanged);

            // 
            // p24_nudMeterItemsPowerLimit
            // 
            this.p24_nudMeterItemsPowerLimit.DecimalPlaces = 1;
            this.p24_nudMeterItemsPowerLimit.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudMeterItemsPowerLimit.Location = new System.Drawing.Point(193, 241);
            this.p24_nudMeterItemsPowerLimit.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.p24_nudMeterItemsPowerLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudMeterItemsPowerLimit.Name = "p24_nudMeterItemsPowerLimit";
            this.p24_nudMeterItemsPowerLimit.Size = new System.Drawing.Size(56, 20);
            this.p24_nudMeterItemsPowerLimit.TabIndex = 114;            this.toolTip1.SetToolTip(this.p24_nudMeterItemsPowerLimit, "Power limit of scale");
            this.p24_nudMeterItemsPowerLimit.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.p24_nudMeterItemsPowerLimit.ValueChanged += new System.EventHandler(this.nudMeterItemsPowerLimit_ValueChanged);

            // 
            // p24_nudRecording_repeatDelay
            // 
            this.p24_nudRecording_repeatDelay.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudRecording_repeatDelay.Location = new System.Drawing.Point(87, 26);
            this.p24_nudRecording_repeatDelay.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.p24_nudRecording_repeatDelay.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.p24_nudRecording_repeatDelay.Name = "p24_nudRecording_repeatDelay";
            this.p24_nudRecording_repeatDelay.Size = new System.Drawing.Size(38, 20);
            this.p24_nudRecording_repeatDelay.TabIndex = 174;            this.toolTip1.SetToolTip(this.p24_nudRecording_repeatDelay, "Auto repeat duration");
            this.p24_nudRecording_repeatDelay.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudRecording_repeatDelay.ValueChanged += new System.EventHandler(this.nudRecording_repeatDelay_ValueChanged);

            // 
            // p24_nudRecording_slot_settings
            // 
            this.p24_nudRecording_slot_settings.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudRecording_slot_settings.Location = new System.Drawing.Point(47, 52);
            this.p24_nudRecording_slot_settings.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.p24_nudRecording_slot_settings.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudRecording_slot_settings.Name = "p24_nudRecording_slot_settings";
            this.p24_nudRecording_slot_settings.ReadOnly = true;
            this.p24_nudRecording_slot_settings.Size = new System.Drawing.Size(42, 20);
            this.p24_nudRecording_slot_settings.TabIndex = 181;            this.toolTip1.SetToolTip(this.p24_nudRecording_slot_settings, "The settings below are for this slot");
            this.p24_nudRecording_slot_settings.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudRecording_slot_settings.ValueChanged += new System.EventHandler(this.nudRecording_slot_settings_ValueChanged);

            // 
            // p24_nudRecording_tx_gain_adjust
            // 
            this.p24_nudRecording_tx_gain_adjust.DecimalPlaces = 1;
            this.p24_nudRecording_tx_gain_adjust.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.p24_nudRecording_tx_gain_adjust.Location = new System.Drawing.Point(78, 116);
            this.p24_nudRecording_tx_gain_adjust.Maximum = new decimal(new int[] {
            70,
            0,
            0,
            0});
            this.p24_nudRecording_tx_gain_adjust.Minimum = new decimal(new int[] {
            70,
            0,
            0,
            -2147483648});
            this.p24_nudRecording_tx_gain_adjust.Name = "p24_nudRecording_tx_gain_adjust";
            this.p24_nudRecording_tx_gain_adjust.Size = new System.Drawing.Size(48, 20);
            this.p24_nudRecording_tx_gain_adjust.TabIndex = 181;            this.toolTip1.SetToolTip(this.p24_nudRecording_tx_gain_adjust, "Adjust the TX gain for this slot");
            this.p24_nudRecording_tx_gain_adjust.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudRecording_tx_gain_adjust.ValueChanged += new System.EventHandler(this.nudRecording_tx_gain_adjust_ValueChanged);

            // 
            // p24_nudTextOverlay_PanelPadding
            // 
            this.p24_nudTextOverlay_PanelPadding.DecimalPlaces = 3;
            this.p24_nudTextOverlay_PanelPadding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudTextOverlay_PanelPadding.Location = new System.Drawing.Point(125, 100);
            this.p24_nudTextOverlay_PanelPadding.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudTextOverlay_PanelPadding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudTextOverlay_PanelPadding.Name = "p24_nudTextOverlay_PanelPadding";
            this.p24_nudTextOverlay_PanelPadding.Size = new System.Drawing.Size(56, 20);
            this.p24_nudTextOverlay_PanelPadding.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudTextOverlay_PanelPadding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudTextOverlay_PanelPadding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudTextOverlay_PanelPadding.ValueChanged += new System.EventHandler(this.nudTextOverlay_PanelPadding_ValueChanged);

            // 
            // p24_nudTextOverlay_RXxOffset
            // 
            this.p24_nudTextOverlay_RXxOffset.DecimalPlaces = 3;
            this.p24_nudTextOverlay_RXxOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudTextOverlay_RXxOffset.Location = new System.Drawing.Point(97, 270);
            this.p24_nudTextOverlay_RXxOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudTextOverlay_RXxOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudTextOverlay_RXxOffset.Name = "p24_nudTextOverlay_RXxOffset";
            this.p24_nudTextOverlay_RXxOffset.Size = new System.Drawing.Size(56, 20);
            this.p24_nudTextOverlay_RXxOffset.TabIndex = 143;            this.toolTip1.SetToolTip(this.p24_nudTextOverlay_RXxOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudTextOverlay_RXxOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudTextOverlay_RXxOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_RXxOffset_ValueChanged);

            // 
            // p24_nudTextOverlay_RXyOffset
            // 
            this.p24_nudTextOverlay_RXyOffset.DecimalPlaces = 3;
            this.p24_nudTextOverlay_RXyOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudTextOverlay_RXyOffset.Location = new System.Drawing.Point(97, 296);
            this.p24_nudTextOverlay_RXyOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudTextOverlay_RXyOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudTextOverlay_RXyOffset.Name = "p24_nudTextOverlay_RXyOffset";
            this.p24_nudTextOverlay_RXyOffset.Size = new System.Drawing.Size(56, 20);
            this.p24_nudTextOverlay_RXyOffset.TabIndex = 147;            this.toolTip1.SetToolTip(this.p24_nudTextOverlay_RXyOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudTextOverlay_RXyOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudTextOverlay_RXyOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_RXyOffset_ValueChanged);

            // 
            // p24_nudTextOverlay_TXxOffset
            // 
            this.p24_nudTextOverlay_TXxOffset.DecimalPlaces = 3;
            this.p24_nudTextOverlay_TXxOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudTextOverlay_TXxOffset.Location = new System.Drawing.Point(97, 322);
            this.p24_nudTextOverlay_TXxOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudTextOverlay_TXxOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudTextOverlay_TXxOffset.Name = "p24_nudTextOverlay_TXxOffset";
            this.p24_nudTextOverlay_TXxOffset.Size = new System.Drawing.Size(56, 20);
            this.p24_nudTextOverlay_TXxOffset.TabIndex = 150;            this.toolTip1.SetToolTip(this.p24_nudTextOverlay_TXxOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudTextOverlay_TXxOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudTextOverlay_TXxOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_TXxOffset_ValueChanged);

            // 
            // p24_nudTextOverlay_TXyOffset
            // 
            this.p24_nudTextOverlay_TXyOffset.DecimalPlaces = 3;
            this.p24_nudTextOverlay_TXyOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudTextOverlay_TXyOffset.Location = new System.Drawing.Point(97, 348);
            this.p24_nudTextOverlay_TXyOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.p24_nudTextOverlay_TXyOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.p24_nudTextOverlay_TXyOffset.Name = "p24_nudTextOverlay_TXyOffset";
            this.p24_nudTextOverlay_TXyOffset.Size = new System.Drawing.Size(56, 20);
            this.p24_nudTextOverlay_TXyOffset.TabIndex = 152;            this.toolTip1.SetToolTip(this.p24_nudTextOverlay_TXyOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.p24_nudTextOverlay_TXyOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudTextOverlay_TXyOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_TXyOffset_ValueChanged);

            // 
            // p24_nudVoiceRecordingPlayback_slots
            // 
            this.p24_nudVoiceRecordingPlayback_slots.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudVoiceRecordingPlayback_slots.Location = new System.Drawing.Point(49, 1);
            this.p24_nudVoiceRecordingPlayback_slots.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.p24_nudVoiceRecordingPlayback_slots.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudVoiceRecordingPlayback_slots.Name = "p24_nudVoiceRecordingPlayback_slots";
            this.p24_nudVoiceRecordingPlayback_slots.Size = new System.Drawing.Size(42, 20);
            this.p24_nudVoiceRecordingPlayback_slots.TabIndex = 133;            this.toolTip1.SetToolTip(this.p24_nudVoiceRecordingPlayback_slots, "Number of recording/playback slots");
            this.p24_nudVoiceRecordingPlayback_slots.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudVoiceRecordingPlayback_slots.ValueChanged += new System.EventHandler(this.nudVoiceRecordingPlayback_slots_ValueChanged);

            // 
            // p24_nudWaveRecord_radius
            // 
            this.p24_nudWaveRecord_radius.DecimalPlaces = 2;
            this.p24_nudWaveRecord_radius.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.p24_nudWaveRecord_radius.Location = new System.Drawing.Point(118, 38);
            this.p24_nudWaveRecord_radius.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.p24_nudWaveRecord_radius.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.p24_nudWaveRecord_radius.Name = "p24_nudWaveRecord_radius";
            this.p24_nudWaveRecord_radius.Size = new System.Drawing.Size(56, 20);
            this.p24_nudWaveRecord_radius.TabIndex = 31;            this.toolTip1.SetToolTip(this.p24_nudWaveRecord_radius, "Corner radius for row panels and buttons");
            this.p24_nudWaveRecord_radius.Value = new decimal(new int[] {
            20,
            0,
            0,
            131072});
            this.p24_nudWaveRecord_radius.ValueChanged += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_nudWaveRecord_vertical_ratio
            // 
            this.p24_nudWaveRecord_vertical_ratio.DecimalPlaces = 3;
            this.p24_nudWaveRecord_vertical_ratio.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.p24_nudWaveRecord_vertical_ratio.Location = new System.Drawing.Point(118, 16);
            this.p24_nudWaveRecord_vertical_ratio.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.p24_nudWaveRecord_vertical_ratio.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            this.p24_nudWaveRecord_vertical_ratio.Name = "p24_nudWaveRecord_vertical_ratio";
            this.p24_nudWaveRecord_vertical_ratio.Size = new System.Drawing.Size(56, 20);
            this.p24_nudWaveRecord_vertical_ratio.TabIndex = 0;            this.toolTip1.SetToolTip(this.p24_nudWaveRecord_vertical_ratio, "Height compared to width");
            this.p24_nudWaveRecord_vertical_ratio.Value = new decimal(new int[] {
            60,
            0,
            0,
            131072});
            this.p24_nudWaveRecord_vertical_ratio.ValueChanged += new System.EventHandler(this.waveRecordSettingControlChanged);

            // 
            // p24_nudWebImage_background_time
            // 
            this.p24_nudWebImage_background_time.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudWebImage_background_time.Location = new System.Drawing.Point(65, 167);
            this.p24_nudWebImage_background_time.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.p24_nudWebImage_background_time.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.p24_nudWebImage_background_time.Name = "p24_nudWebImage_background_time";
            this.p24_nudWebImage_background_time.Size = new System.Drawing.Size(47, 20);
            this.p24_nudWebImage_background_time.TabIndex = 148;            this.p24_nudWebImage_background_time.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.p24_nudWebImage_background_time.ValueChanged += new System.EventHandler(this.nudWebImage_background_time_ValueChanged);

            // 
            // p24_nudWebImage_update_interval
            // 
            this.p24_nudWebImage_update_interval.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudWebImage_update_interval.Location = new System.Drawing.Point(95, 54);
            this.p24_nudWebImage_update_interval.Maximum = new decimal(new int[] {
            7200,
            0,
            0,
            0});
            this.p24_nudWebImage_update_interval.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.p24_nudWebImage_update_interval.Name = "p24_nudWebImage_update_interval";
            this.p24_nudWebImage_update_interval.Size = new System.Drawing.Size(56, 20);
            this.p24_nudWebImage_update_interval.TabIndex = 134;            this.toolTip1.SetToolTip(this.p24_nudWebImage_update_interval, "Frequency to grab the web image");
            this.p24_nudWebImage_update_interval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.p24_nudWebImage_update_interval.ValueChanged += new System.EventHandler(this.nudWebImage_update_interval_ValueChanged);

            // 
            // p24_nudWebImage_width_scale
            // 
            this.p24_nudWebImage_width_scale.DecimalPlaces = 3;
            this.p24_nudWebImage_width_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudWebImage_width_scale.Location = new System.Drawing.Point(95, 28);
            this.p24_nudWebImage_width_scale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudWebImage_width_scale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.p24_nudWebImage_width_scale.Name = "p24_nudWebImage_width_scale";
            this.p24_nudWebImage_width_scale.Size = new System.Drawing.Size(56, 20);
            this.p24_nudWebImage_width_scale.TabIndex = 132;            this.toolTip1.SetToolTip(this.p24_nudWebImage_width_scale, "Width scale. 1.0 will fill the container width");
            this.p24_nudWebImage_width_scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.p24_nudWebImage_width_scale.ValueChanged += new System.EventHandler(this.nudWebImage_width_scale_ValueChanged);

            // 
            // p24_picButtonBoxInfo
            // 
            this.p24_picButtonBoxInfo.Image = null;
            this.p24_picButtonBoxInfo.Location = new System.Drawing.Point(114, 307);
            this.p24_picButtonBoxInfo.Name = "p24_picButtonBoxInfo";
            this.p24_picButtonBoxInfo.Size = new System.Drawing.Size(20, 20);
            this.p24_picButtonBoxInfo.TabIndex = 171;
            this.p24_picButtonBoxInfo.TabStop = false;

            // 
            // p24_picMultiMeterRotatorControlInfo
            // 
            this.p24_picMultiMeterRotatorControlInfo.Image = null;
            this.p24_picMultiMeterRotatorControlInfo.Location = new System.Drawing.Point(291, 247);
            this.p24_picMultiMeterRotatorControlInfo.Name = "p24_picMultiMeterRotatorControlInfo";
            this.p24_picMultiMeterRotatorControlInfo.Size = new System.Drawing.Size(20, 20);
            this.p24_picMultiMeterRotatorControlInfo.TabIndex = 165;
            this.p24_picMultiMeterRotatorControlInfo.TabStop = false;
            this.toolTip1.SetToolTip(this.p24_picMultiMeterRotatorControlInfo, "%AZ%\r\n%ELE%");

            // 
            // p24_pnlButtonBox_antenna_toggles
            // 
            this.p24_pnlButtonBox_antenna_toggles.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlButtonBox_antenna_toggles.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_rxtxant);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_xvtr);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_ext1);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_byp);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_tx3);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_tx2);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_tx1);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_rx3);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_rx2);
            this.p24_pnlButtonBox_antenna_toggles.Controls.Add(this.p24_chkButtonBox_antenna_rx1);
            this.p24_pnlButtonBox_antenna_toggles.Location = new System.Drawing.Point(150, 194);
            this.p24_pnlButtonBox_antenna_toggles.Name = "p24_pnlButtonBox_antenna_toggles";
            this.p24_pnlButtonBox_antenna_toggles.Size = new System.Drawing.Size(170, 178);
            this.p24_pnlButtonBox_antenna_toggles.TabIndex = 110;

            // 
            // p24_pnlFilterModeModifiers
            // 
            this.p24_pnlFilterModeModifiers.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlFilterModeModifiers.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlFilterModeModifiers.Controls.Add(this.p24_nudFilterItem_others_scale);
            this.p24_pnlFilterModeModifiers.Controls.Add(this.p24_nudFilterItem_cw_scale);
            this.p24_pnlFilterModeModifiers.Controls.Add(this.p24_nudFilterItem_sidebands_scale);
            this.p24_pnlFilterModeModifiers.Controls.Add(this.p24_labelTS296);
            this.p24_pnlFilterModeModifiers.Controls.Add(this.p24_labelTS294);
            this.p24_pnlFilterModeModifiers.Controls.Add(this.p24_labelTS293);
            this.p24_pnlFilterModeModifiers.Enabled = false;
            this.p24_pnlFilterModeModifiers.Location = new System.Drawing.Point(190, 81);
            this.p24_pnlFilterModeModifiers.Name = "p24_pnlFilterModeModifiers";
            this.p24_pnlFilterModeModifiers.Size = new System.Drawing.Size(127, 79);
            this.p24_pnlFilterModeModifiers.TabIndex = 143;

            // 
            // p24_pnlMeterItemSettings
            // 
            this.p24_pnlMeterItemSettings.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlMeterItemSettings.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_ucMeterItemSignalType);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemLow);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMIndicator);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMsegSolHigh);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_nudMeterItemHistoryDuration);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemFadeOnRx);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMHistory);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMsegSolLow);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemFadeOnTx);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemShowIndicator);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_tbMeterItemHistoryAlpha);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemHistory);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemPeakValue);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemSegmentedSolidColourHigh);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_nudMeterItemIgnoreHistoryDuration);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemSubIndicator);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemSegmentedSolidColourLow);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemPeakHold);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemTitle);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemSolid);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMHistoryIgnore);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMIndicatorSub);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemPeakValueColour);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemShadow);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemSegmented);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_nudMeterItemsPowerLimit);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemPowerScale);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemShowSubIndicator);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemMeterTitle);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemIndicator);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemHistory);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMPowerLimit);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMHigh);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMEyeBezelSize);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMEyeSize);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_nudMeterItemEyeBezelScale);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_chkMeterItemDarkMode);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_lblMMLow);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemHigh);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_nudMeterItemEyeScale);
            this.p24_pnlMeterItemSettings.Controls.Add(this.p24_clrbtnMeterItemPeakHold);
            this.p24_pnlMeterItemSettings.Location = new System.Drawing.Point(9, 98);
            this.p24_pnlMeterItemSettings.Name = "p24_pnlMeterItemSettings";
            this.p24_pnlMeterItemSettings.Size = new System.Drawing.Size(308, 273);
            this.p24_pnlMeterItemSettings.TabIndex = 112;

            // 
            // p24_pnlMeterItemSettings_custom
            // 
            this.p24_pnlMeterItemSettings_custom.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlMeterItemSettings_custom.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_labelTS433);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_txtMeterItem_custom_title);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_labelTS432);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_txtMeterItem_custom_units);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_nudMeterItem_custom_high);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_labelTS431);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_nudMeterItem_custom_max);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_labelTS429);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_nudMeterItem_custom_min);
            this.p24_pnlMeterItemSettings_custom.Controls.Add(this.p24_labelTS428);
            this.p24_pnlMeterItemSettings_custom.Location = new System.Drawing.Point(209, 95);
            this.p24_pnlMeterItemSettings_custom.Name = "p24_pnlMeterItemSettings_custom";
            this.p24_pnlMeterItemSettings_custom.Size = new System.Drawing.Size(308, 273);
            this.p24_pnlMeterItemSettings_custom.TabIndex = 113;

            // 
            // p24_pnlVariableInUse_1
            // 
            this.p24_pnlVariableInUse_1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_1.BackColor = System.Drawing.Color.Lime;
            this.p24_pnlVariableInUse_1.Location = new System.Drawing.Point(243, 84);
            this.p24_pnlVariableInUse_1.Name = "p24_pnlVariableInUse_1";
            this.p24_pnlVariableInUse_1.Size = new System.Drawing.Size(28, 6);
            this.p24_pnlVariableInUse_1.TabIndex = 130;

            // 
            // p24_pnlVariableInUse_1_history
            // 
            this.p24_pnlVariableInUse_1_history.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_1_history.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_1_history.BackColor = System.Drawing.Color.Lime;
            this.p24_pnlVariableInUse_1_history.Location = new System.Drawing.Point(252, 79);
            this.p24_pnlVariableInUse_1_history.Name = "p24_pnlVariableInUse_1_history";
            this.p24_pnlVariableInUse_1_history.Size = new System.Drawing.Size(28, 6);
            this.p24_pnlVariableInUse_1_history.TabIndex = 141;

            // 
            // p24_pnlVariableInUse_1_rotator
            // 
            this.p24_pnlVariableInUse_1_rotator.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_1_rotator.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_1_rotator.BackColor = System.Drawing.Color.Lime;
            this.p24_pnlVariableInUse_1_rotator.Location = new System.Drawing.Point(243, 73);
            this.p24_pnlVariableInUse_1_rotator.Name = "p24_pnlVariableInUse_1_rotator";
            this.p24_pnlVariableInUse_1_rotator.Size = new System.Drawing.Size(28, 6);
            this.p24_pnlVariableInUse_1_rotator.TabIndex = 130;

            // 
            // p24_pnlVariableInUse_2
            // 
            this.p24_pnlVariableInUse_2.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_2.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_2.BackColor = System.Drawing.Color.Lime;
            this.p24_pnlVariableInUse_2.Location = new System.Drawing.Point(275, 84);
            this.p24_pnlVariableInUse_2.Name = "p24_pnlVariableInUse_2";
            this.p24_pnlVariableInUse_2.Size = new System.Drawing.Size(28, 6);
            this.p24_pnlVariableInUse_2.TabIndex = 131;

            // 
            // p24_pnlVariableInUse_2_history
            // 
            this.p24_pnlVariableInUse_2_history.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_2_history.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_2_history.BackColor = System.Drawing.Color.Lime;
            this.p24_pnlVariableInUse_2_history.Location = new System.Drawing.Point(284, 79);
            this.p24_pnlVariableInUse_2_history.Name = "p24_pnlVariableInUse_2_history";
            this.p24_pnlVariableInUse_2_history.Size = new System.Drawing.Size(28, 6);
            this.p24_pnlVariableInUse_2_history.TabIndex = 142;

            // 
            // p24_pnlVariableInUse_2_rotator
            // 
            this.p24_pnlVariableInUse_2_rotator.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_2_rotator.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlVariableInUse_2_rotator.BackColor = System.Drawing.Color.Lime;
            this.p24_pnlVariableInUse_2_rotator.Location = new System.Drawing.Point(275, 73);
            this.p24_pnlVariableInUse_2_rotator.Name = "p24_pnlVariableInUse_2_rotator";
            this.p24_pnlVariableInUse_2_rotator.Size = new System.Drawing.Size(28, 6);
            this.p24_pnlVariableInUse_2_rotator.TabIndex = 135;

            // 
            // p24_pnlVoiceRecordPlayback
            // 
            this.p24_pnlVoiceRecordPlayback.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.p24_pnlVoiceRecordPlayback.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_btnRecording_export_wav_from_slot);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_btnRecording_load_wav_to_slot);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_btnRecording_4char_copy);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_labelTS657);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_scrollableControl2);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_btnRecording_openStorageFolder);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_nudRecording_slot_settings);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_labelTS658);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_labelTS650);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_txtRecording_4char);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_nudVoiceRecordingPlayback_slots);
            this.p24_pnlVoiceRecordPlayback.Controls.Add(this.p24_labelTS653);
            this.p24_pnlVoiceRecordPlayback.Location = new System.Drawing.Point(356, 20);
            this.p24_pnlVoiceRecordPlayback.Name = "p24_pnlVoiceRecordPlayback";
            this.p24_pnlVoiceRecordPlayback.Size = new System.Drawing.Size(170, 178);
            this.p24_pnlVoiceRecordPlayback.TabIndex = 113;

            // 
            // p24_radContainer_rx1_data
            // 
            this.p24_radContainer_rx1_data.AutoSize = true;
            this.p24_radContainer_rx1_data.Image = null;
            this.p24_radContainer_rx1_data.Location = new System.Drawing.Point(290, 14);
            this.p24_radContainer_rx1_data.Name = "p24_radContainer_rx1_data";
            this.p24_radContainer_rx1_data.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_radContainer_rx1_data.Size = new System.Drawing.Size(70, 17);
            this.p24_radContainer_rx1_data.TabIndex = 112;
            this.p24_radContainer_rx1_data.TabStop = true;
            this.p24_radContainer_rx1_data.Text = "RX1 data";
            this.toolTip1.SetToolTip(this.p24_radContainer_rx1_data, "This container will use RX1 data");
            this.p24_radContainer_rx1_data.UseVisualStyleBackColor = true;
            this.p24_radContainer_rx1_data.CheckedChanged += new System.EventHandler(this.radContainer_rx1_data_CheckedChanged);

            // 
            // p24_radContainer_rx2_data
            // 
            this.p24_radContainer_rx2_data.AutoSize = true;
            this.p24_radContainer_rx2_data.Image = null;
            this.p24_radContainer_rx2_data.Location = new System.Drawing.Point(290, 33);
            this.p24_radContainer_rx2_data.Name = "p24_radContainer_rx2_data";
            this.p24_radContainer_rx2_data.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.p24_radContainer_rx2_data.Size = new System.Drawing.Size(70, 17);
            this.p24_radContainer_rx2_data.TabIndex = 113;
            this.p24_radContainer_rx2_data.TabStop = true;
            this.p24_radContainer_rx2_data.Text = "RX2 data";
            this.toolTip1.SetToolTip(this.p24_radContainer_rx2_data, "This container will use RX2 data");
            this.p24_radContainer_rx2_data.UseVisualStyleBackColor = true;
            this.p24_radContainer_rx2_data.CheckedChanged += new System.EventHandler(this.radContainer_rx2_data_CheckedChanged);

            // 
            // p24_radFilterItem_none
            // 
            this.p24_radFilterItem_none.AutoSize = true;
            this.p24_radFilterItem_none.Image = null;
            this.p24_radFilterItem_none.Location = new System.Drawing.Point(23, 203);
            this.p24_radFilterItem_none.Name = "p24_radFilterItem_none";
            this.p24_radFilterItem_none.Size = new System.Drawing.Size(51, 17);
            this.p24_radFilterItem_none.TabIndex = 147;
            this.p24_radFilterItem_none.Text = "None";
            this.p24_radFilterItem_none.UseVisualStyleBackColor = true;
            this.p24_radFilterItem_none.CheckedChanged += new System.EventHandler(this.radFilterItem_none_CheckedChanged);

            // 
            // p24_radFilterItem_panadaptor
            // 
            this.p24_radFilterItem_panadaptor.AutoSize = true;
            this.p24_radFilterItem_panadaptor.Image = null;
            this.p24_radFilterItem_panadaptor.Location = new System.Drawing.Point(23, 134);
            this.p24_radFilterItem_panadaptor.Name = "p24_radFilterItem_panadaptor";
            this.p24_radFilterItem_panadaptor.Size = new System.Drawing.Size(80, 17);
            this.p24_radFilterItem_panadaptor.TabIndex = 146;
            this.p24_radFilterItem_panadaptor.Text = "Panadaptor";
            this.p24_radFilterItem_panadaptor.UseVisualStyleBackColor = true;
            this.p24_radFilterItem_panadaptor.CheckedChanged += new System.EventHandler(this.radFilterItem_panadaptor_CheckedChanged);

            // 
            // p24_radFilterItem_panafall
            // 
            this.p24_radFilterItem_panafall.AutoSize = true;
            this.p24_radFilterItem_panafall.Checked = true;
            this.p24_radFilterItem_panafall.Image = null;
            this.p24_radFilterItem_panafall.Location = new System.Drawing.Point(23, 180);
            this.p24_radFilterItem_panafall.Name = "p24_radFilterItem_panafall";
            this.p24_radFilterItem_panafall.Size = new System.Drawing.Size(63, 17);
            this.p24_radFilterItem_panafall.TabIndex = 144;
            this.p24_radFilterItem_panafall.TabStop = true;
            this.p24_radFilterItem_panafall.Text = "Panafall";
            this.p24_radFilterItem_panafall.UseVisualStyleBackColor = true;
            this.p24_radFilterItem_panafall.CheckedChanged += new System.EventHandler(this.radFilterItem_panafall_CheckedChanged);

            // 
            // p24_radFilterItem_waterfall
            // 
            this.p24_radFilterItem_waterfall.AutoSize = true;
            this.p24_radFilterItem_waterfall.Image = null;
            this.p24_radFilterItem_waterfall.Location = new System.Drawing.Point(23, 157);
            this.p24_radFilterItem_waterfall.Name = "p24_radFilterItem_waterfall";
            this.p24_radFilterItem_waterfall.Size = new System.Drawing.Size(67, 17);
            this.p24_radFilterItem_waterfall.TabIndex = 145;
            this.p24_radFilterItem_waterfall.Text = "Waterfall";
            this.p24_radFilterItem_waterfall.UseVisualStyleBackColor = true;
            this.p24_radFilterItem_waterfall.CheckedChanged += new System.EventHandler(this.radFilterItem_waterfall_CheckedChanged);

            // 
            // p24_radLed_light_blink
            // 
            this.p24_radLed_light_blink.AutoSize = true;
            this.p24_radLed_light_blink.Image = null;
            this.p24_radLed_light_blink.Location = new System.Drawing.Point(182, 263);
            this.p24_radLed_light_blink.Name = "p24_radLed_light_blink";
            this.p24_radLed_light_blink.Size = new System.Drawing.Size(137, 17);
            this.p24_radLed_light_blink.TabIndex = 171;
            this.p24_radLed_light_blink.TabStop = true;
            this.p24_radLed_light_blink.Text = "Blink (when initially true)";
            this.p24_radLed_light_blink.UseVisualStyleBackColor = true;
            this.p24_radLed_light_blink.CheckedChanged += new System.EventHandler(this.radLed_light_blink_CheckedChanged);

            // 
            // p24_radLed_light_on_off
            // 
            this.p24_radLed_light_on_off.AutoSize = true;
            this.p24_radLed_light_on_off.Image = null;
            this.p24_radLed_light_on_off.Location = new System.Drawing.Point(182, 240);
            this.p24_radLed_light_on_off.Name = "p24_radLed_light_on_off";
            this.p24_radLed_light_on_off.Size = new System.Drawing.Size(58, 17);
            this.p24_radLed_light_on_off.TabIndex = 170;
            this.p24_radLed_light_on_off.TabStop = true;
            this.p24_radLed_light_on_off.Text = "On/Off";
            this.p24_radLed_light_on_off.UseVisualStyleBackColor = true;
            this.p24_radLed_light_on_off.CheckedChanged += new System.EventHandler(this.radLed_light_on_off_CheckedChanged);

            // 
            // p24_radLed_light_pulsate
            // 
            this.p24_radLed_light_pulsate.AutoSize = true;
            this.p24_radLed_light_pulsate.Image = null;
            this.p24_radLed_light_pulsate.Location = new System.Drawing.Point(182, 286);
            this.p24_radLed_light_pulsate.Name = "p24_radLed_light_pulsate";
            this.p24_radLed_light_pulsate.Size = new System.Drawing.Size(116, 17);
            this.p24_radLed_light_pulsate.TabIndex = 172;
            this.p24_radLed_light_pulsate.TabStop = true;
            this.p24_radLed_light_pulsate.Text = "Pulsate (when true)";
            this.p24_radLed_light_pulsate.UseVisualStyleBackColor = true;
            this.p24_radLed_light_pulsate.CheckedChanged += new System.EventHandler(this.radLed_light_pulsate_CheckedChanged);

            // 
            // p24_radMM12Clock
            // 
            this.p24_radMM12Clock.AutoSize = true;
            this.p24_radMM12Clock.Image = null;
            this.p24_radMM12Clock.Location = new System.Drawing.Point(192, 48);
            this.p24_radMM12Clock.Name = "p24_radMM12Clock";
            this.p24_radMM12Clock.Size = new System.Drawing.Size(49, 17);
            this.p24_radMM12Clock.TabIndex = 0;
            this.p24_radMM12Clock.TabStop = true;
            this.p24_radMM12Clock.Text = "12 hr";
            this.toolTip1.SetToolTip(this.p24_radMM12Clock, "12 hr clock");
            this.p24_radMM12Clock.UseVisualStyleBackColor = true;
            this.p24_radMM12Clock.CheckedChanged += new System.EventHandler(this.radMM12Clock_CheckedChanged);

            // 
            // p24_radMM24Clock
            // 
            this.p24_radMM24Clock.AutoSize = true;
            this.p24_radMM24Clock.Image = null;
            this.p24_radMM24Clock.Location = new System.Drawing.Point(192, 67);
            this.p24_radMM24Clock.Name = "p24_radMM24Clock";
            this.p24_radMM24Clock.Size = new System.Drawing.Size(49, 17);
            this.p24_radMM24Clock.TabIndex = 1;
            this.p24_radMM24Clock.TabStop = true;
            this.p24_radMM24Clock.Text = "24 hr";
            this.toolTip1.SetToolTip(this.p24_radMM24Clock, "24 hr clock");
            this.p24_radMM24Clock.UseVisualStyleBackColor = true;
            this.p24_radMM24Clock.CheckedChanged += new System.EventHandler(this.radMM24Clock_CheckedChanged);

            // 
            // p24_radMeterItemRotator_show_az
            // 
            this.p24_radMeterItemRotator_show_az.AutoSize = true;
            this.p24_radMeterItemRotator_show_az.Image = null;
            this.p24_radMeterItemRotator_show_az.Location = new System.Drawing.Point(25, 223);
            this.p24_radMeterItemRotator_show_az.Name = "p24_radMeterItemRotator_show_az";
            this.p24_radMeterItemRotator_show_az.Size = new System.Drawing.Size(62, 17);
            this.p24_radMeterItemRotator_show_az.TabIndex = 169;
            this.p24_radMeterItemRotator_show_az.TabStop = true;
            this.p24_radMeterItemRotator_show_az.Text = "Azimuth";
            this.toolTip1.SetToolTip(this.p24_radMeterItemRotator_show_az, "Show azimuth only");
            this.p24_radMeterItemRotator_show_az.UseVisualStyleBackColor = true;
            this.p24_radMeterItemRotator_show_az.CheckedChanged += new System.EventHandler(this.radMeterItemRotator_show_az_CheckedChanged);

            // 
            // p24_radMeterItemRotator_show_both
            // 
            this.p24_radMeterItemRotator_show_both.AutoSize = true;
            this.p24_radMeterItemRotator_show_both.Image = null;
            this.p24_radMeterItemRotator_show_both.Location = new System.Drawing.Point(168, 223);
            this.p24_radMeterItemRotator_show_both.Name = "p24_radMeterItemRotator_show_both";
            this.p24_radMeterItemRotator_show_both.Size = new System.Drawing.Size(47, 17);
            this.p24_radMeterItemRotator_show_both.TabIndex = 171;
            this.p24_radMeterItemRotator_show_both.TabStop = true;
            this.p24_radMeterItemRotator_show_both.Text = "Both";
            this.toolTip1.SetToolTip(this.p24_radMeterItemRotator_show_both, "Show both azimuth and elevation");
            this.p24_radMeterItemRotator_show_both.UseVisualStyleBackColor = true;
            this.p24_radMeterItemRotator_show_both.CheckedChanged += new System.EventHandler(this.radMeterItemRotator_show_both_CheckedChanged);

            // 
            // p24_radMeterItemRotator_show_ele
            // 
            this.p24_radMeterItemRotator_show_ele.AutoSize = true;
            this.p24_radMeterItemRotator_show_ele.Image = null;
            this.p24_radMeterItemRotator_show_ele.Location = new System.Drawing.Point(93, 223);
            this.p24_radMeterItemRotator_show_ele.Name = "p24_radMeterItemRotator_show_ele";
            this.p24_radMeterItemRotator_show_ele.Size = new System.Drawing.Size(69, 17);
            this.p24_radMeterItemRotator_show_ele.TabIndex = 170;
            this.p24_radMeterItemRotator_show_ele.TabStop = true;
            this.p24_radMeterItemRotator_show_ele.Text = "Elevation";
            this.toolTip1.SetToolTip(this.p24_radMeterItemRotator_show_ele, "Show Elevation only");
            this.p24_radMeterItemRotator_show_ele.UseVisualStyleBackColor = true;
            this.p24_radMeterItemRotator_show_ele.CheckedChanged += new System.EventHandler(this.radMeterItemRotator_show_ele_CheckedChanged);

            // 
            // p24_radMeterItemSettings
            // 
            this.p24_radMeterItemSettings.AutoSize = true;
            this.p24_radMeterItemSettings.Checked = true;
            this.p24_radMeterItemSettings.Image = null;
            this.p24_radMeterItemSettings.Location = new System.Drawing.Point(156, 53);
            this.p24_radMeterItemSettings.Name = "p24_radMeterItemSettings";
            this.p24_radMeterItemSettings.Size = new System.Drawing.Size(63, 17);
            this.p24_radMeterItemSettings.TabIndex = 132;
            this.p24_radMeterItemSettings.TabStop = true;
            this.p24_radMeterItemSettings.Text = "Settings";
            this.p24_radMeterItemSettings.UseVisualStyleBackColor = true;
            this.p24_radMeterItemSettings.Visible = false;
            this.p24_radMeterItemSettings.CheckedChanged += new System.EventHandler(this.radMeterItemSettings_CheckedChanged);

            // 
            // p24_radMeterItemSettings_custom
            // 
            this.p24_radMeterItemSettings_custom.AutoSize = true;
            this.p24_radMeterItemSettings_custom.Image = null;
            this.p24_radMeterItemSettings_custom.Location = new System.Drawing.Point(156, 73);
            this.p24_radMeterItemSettings_custom.Name = "p24_radMeterItemSettings_custom";
            this.p24_radMeterItemSettings_custom.Size = new System.Drawing.Size(60, 17);
            this.p24_radMeterItemSettings_custom.TabIndex = 133;
            this.p24_radMeterItemSettings_custom.Text = "Custom";
            this.p24_radMeterItemSettings_custom.UseVisualStyleBackColor = true;
            this.p24_radMeterItemSettings_custom.Visible = false;
            this.p24_radMeterItemSettings_custom.CheckedChanged += new System.EventHandler(this.radMeterItemSettings_custom_CheckedChanged);

            // 
            // p24_radMultiMeter_vfo_display_both
            // 
            this.p24_radMultiMeter_vfo_display_both.AutoSize = true;
            this.p24_radMultiMeter_vfo_display_both.Image = null;
            this.p24_radMultiMeter_vfo_display_both.Location = new System.Drawing.Point(105, 323);
            this.p24_radMultiMeter_vfo_display_both.Name = "p24_radMultiMeter_vfo_display_both";
            this.p24_radMultiMeter_vfo_display_both.Size = new System.Drawing.Size(47, 17);
            this.p24_radMultiMeter_vfo_display_both.TabIndex = 132;
            this.p24_radMultiMeter_vfo_display_both.TabStop = true;
            this.p24_radMultiMeter_vfo_display_both.Text = "Both";
            this.p24_radMultiMeter_vfo_display_both.UseVisualStyleBackColor = true;
            this.p24_radMultiMeter_vfo_display_both.CheckedChanged += new System.EventHandler(this.radMultiMeter_vfo_display_both_CheckedChanged);

            // 
            // p24_radMultiMeter_vfo_display_vfoa
            // 
            this.p24_radMultiMeter_vfo_display_vfoa.AutoSize = true;
            this.p24_radMultiMeter_vfo_display_vfoa.Image = null;
            this.p24_radMultiMeter_vfo_display_vfoa.Location = new System.Drawing.Point(158, 323);
            this.p24_radMultiMeter_vfo_display_vfoa.Name = "p24_radMultiMeter_vfo_display_vfoa";
            this.p24_radMultiMeter_vfo_display_vfoa.Size = new System.Drawing.Size(56, 17);
            this.p24_radMultiMeter_vfo_display_vfoa.TabIndex = 133;
            this.p24_radMultiMeter_vfo_display_vfoa.TabStop = true;
            this.p24_radMultiMeter_vfo_display_vfoa.Text = "VFO A";
            this.p24_radMultiMeter_vfo_display_vfoa.UseVisualStyleBackColor = true;
            this.p24_radMultiMeter_vfo_display_vfoa.CheckedChanged += new System.EventHandler(this.radMultiMeter_vfo_display_vfoa_CheckedChanged);

            // 
            // p24_radMultiMeter_vfo_display_vfob
            // 
            this.p24_radMultiMeter_vfo_display_vfob.AutoSize = true;
            this.p24_radMultiMeter_vfo_display_vfob.Image = null;
            this.p24_radMultiMeter_vfo_display_vfob.Location = new System.Drawing.Point(220, 323);
            this.p24_radMultiMeter_vfo_display_vfob.Name = "p24_radMultiMeter_vfo_display_vfob";
            this.p24_radMultiMeter_vfo_display_vfob.Size = new System.Drawing.Size(56, 17);
            this.p24_radMultiMeter_vfo_display_vfob.TabIndex = 134;
            this.p24_radMultiMeter_vfo_display_vfob.TabStop = true;
            this.p24_radMultiMeter_vfo_display_vfob.Text = "VFO B";
            this.p24_radMultiMeter_vfo_display_vfob.UseVisualStyleBackColor = true;
            this.p24_radMultiMeter_vfo_display_vfob.CheckedChanged += new System.EventHandler(this.radMultiMeter_vfo_display_vfob_CheckedChanged);

            // 
            // p24_scrlFilter
            // 
            this.p24_scrlFilter.AutoScroll = true;
            this.p24_scrlFilter.BackColor = System.Drawing.SystemColors.ControlLight;
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS422);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_edges_tx);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_button_highlight);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS345);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_setting_on);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_wf_low);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS346);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS298);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_snap_line);
            this.p24_scrlFilter.Controls.Add(this.p24_comboFilter_wf_palette);
            this.p24_scrlFilter.Controls.Add(this.p24_label22);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS344);
            this.p24_scrlFilter.Controls.Add(this.p24_chkFilter_grey_outsidepb);
            this.p24_scrlFilter.Controls.Add(this.p24_chkFilter_fill_spec);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS299);
            this.p24_scrlFilter.Controls.Add(this.p24_chkFilter_sideband_mode);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_data_line);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS300);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_extents);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_data_fill);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS338);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS301);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_notch_highlight);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_text);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS336);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS330);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_notch);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_number_highlight);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS337);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS331);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_meter_back);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_edges);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS335);
            this.p24_scrlFilter.Controls.Add(this.p24_labelTS334);
            this.p24_scrlFilter.Controls.Add(this.p24_clrbtnFilter_edge_highlight);
            this.p24_scrlFilter.Location = new System.Drawing.Point(6, 231);
            this.p24_scrlFilter.Name = "p24_scrlFilter";
            this.p24_scrlFilter.Size = new System.Drawing.Size(311, 139);
            this.p24_scrlFilter.TabIndex = 112;

            // 
            // p24_scrollableControl1
            // 
            this.p24_scrollableControl1.AutoScroll = true;
            this.p24_scrollableControl1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.p24_scrollableControl1.Controls.Add(this.p24_groupBoxTS40);
            this.p24_scrollableControl1.Controls.Add(this.p24_groupBoxTS42);
            this.p24_scrollableControl1.Controls.Add(this.p24_groupBoxTS43);
            this.p24_scrollableControl1.Controls.Add(this.p24_groupBoxTS41);
            this.p24_scrollableControl1.Location = new System.Drawing.Point(6, 194);
            this.p24_scrollableControl1.Name = "p24_scrollableControl1";
            this.p24_scrollableControl1.Size = new System.Drawing.Size(311, 176);
            this.p24_scrollableControl1.TabIndex = 112;
            this.p24_scrollableControl1.Text = "p24_scrollableControl1";

            // 
            // p24_scrollableControl2
            // 
            this.p24_scrollableControl2.AutoScroll = true;
            this.p24_scrollableControl2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.p24_scrollableControl2.Controls.Add(this.p24_nudRecording_tx_gain_adjust);
            this.p24_scrollableControl2.Controls.Add(this.p24_chkRecording_ignore_record_tempchanges);
            this.p24_scrollableControl2.Controls.Add(this.p24_chkRecording_ignore_play_tempchanges);
            this.p24_scrollableControl2.Controls.Add(this.p24_labelTS655);
            this.p24_scrollableControl2.Controls.Add(this.p24_labelTS654);
            this.p24_scrollableControl2.Controls.Add(this.p24_btnRecording_assingnkeybind);
            this.p24_scrollableControl2.Controls.Add(this.p24_labelTS651);
            this.p24_scrollableControl2.Controls.Add(this.p24_chkRecording_playkeybind);
            this.p24_scrollableControl2.Controls.Add(this.p24_txtRecording_labelText);
            this.p24_scrollableControl2.Controls.Add(this.p24_txtRecording_playkeybind);
            this.p24_scrollableControl2.Controls.Add(this.p24_chkRecording_slot_locked);
            this.p24_scrollableControl2.Controls.Add(this.p24_chkRecording_canRepeat);
            this.p24_scrollableControl2.Controls.Add(this.p24_labelTS652);
            this.p24_scrollableControl2.Controls.Add(this.p24_nudRecording_repeatDelay);
            this.p24_scrollableControl2.Location = new System.Drawing.Point(0, 76);
            this.p24_scrollableControl2.Name = "p24_scrollableControl2";
            this.p24_scrollableControl2.Size = new System.Drawing.Size(170, 103);
            this.p24_scrollableControl2.TabIndex = 113;

            // 
            // p24_tbMeterItemHistoryAlpha
            // 
            this.p24_tbMeterItemHistoryAlpha.AutoSize = false;
            this.p24_tbMeterItemHistoryAlpha.Location = new System.Drawing.Point(230, 122);
            this.p24_tbMeterItemHistoryAlpha.Maximum = 255;
            this.p24_tbMeterItemHistoryAlpha.Name = "p24_tbMeterItemHistoryAlpha";
            this.p24_tbMeterItemHistoryAlpha.Size = new System.Drawing.Size(66, 18);
            this.p24_tbMeterItemHistoryAlpha.TabIndex = 99;
            this.p24_tbMeterItemHistoryAlpha.TickFrequency = 64;
            this.p24_tbMeterItemHistoryAlpha.Value = 255;
            this.p24_tbMeterItemHistoryAlpha.Scroll += new System.EventHandler(this.tbMeterItemHistoryAlpha_Scroll);

            // 
            // p24_tpAppearanceMeter2
            // 
            this.p24_tpAppearanceMeter2.BackColor = System.Drawing.SystemColors.Control;
            this.p24_tpAppearanceMeter2.Controls.Add(this.p24_grpMultiMeterHolder);
            this.p24_tpAppearanceMeter2.Location = new System.Drawing.Point(4, 22);
            this.p24_tpAppearanceMeter2.Name = "p24_tpAppearanceMeter2";
            this.p24_tpAppearanceMeter2.Size = new System.Drawing.Size(724, 410);
            this.p24_tpAppearanceMeter2.TabIndex = 5;
            this.p24_tpAppearanceMeter2.Text = "Meters/Gadgets";

            // 
            // p24_txtContainerNotes
            // 
            this.p24_txtContainerNotes.Location = new System.Drawing.Point(7, 130);
            this.p24_txtContainerNotes.MaxLength = 2048;
            this.p24_txtContainerNotes.Multiline = true;
            this.p24_txtContainerNotes.Name = "p24_txtContainerNotes";
            this.p24_txtContainerNotes.Size = new System.Drawing.Size(196, 33);
            this.p24_txtContainerNotes.TabIndex = 106;
            this.toolTip1.SetToolTip(this.p24_txtContainerNotes, "Somewhere to store notes about this container");
            this.p24_txtContainerNotes.TextChanged += new System.EventHandler(this.txtContainerNotes_TextChanged);

            // 
            // p24_txtDataOutNode_4charID
            // 
            this.p24_txtDataOutNode_4charID.Location = new System.Drawing.Point(93, 48);
            this.p24_txtDataOutNode_4charID.MaxLength = 4;
            this.p24_txtDataOutNode_4charID.Name = "p24_txtDataOutNode_4charID";
            this.p24_txtDataOutNode_4charID.Size = new System.Drawing.Size(43, 20);
            this.p24_txtDataOutNode_4charID.TabIndex = 136;
            this.p24_txtDataOutNode_4charID.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtDataOutNode_4charID, "The four character code used to reference which MultiMeterIO is to be used for th" +
        "e output data.");
            this.p24_txtDataOutNode_4charID.TextChanged += new System.EventHandler(this.txtDataOutNode_4charID_TextChanged);

            // 
            // p24_txtLedIndicator_4char
            // 
            this.p24_txtLedIndicator_4char.Location = new System.Drawing.Point(244, 212);
            this.p24_txtLedIndicator_4char.MaxLength = 4;
            this.p24_txtLedIndicator_4char.Name = "p24_txtLedIndicator_4char";
            this.p24_txtLedIndicator_4char.Size = new System.Drawing.Size(43, 20);
            this.p24_txtLedIndicator_4char.TabIndex = 177;
            this.p24_txtLedIndicator_4char.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtLedIndicator_4char, "The four character code for this web image");
            this.p24_txtLedIndicator_4char.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLedIndicator_4char_KeyPress);

            // 
            // p24_txtLedIndicator_condition
            // 
            this.p24_txtLedIndicator_condition.Location = new System.Drawing.Point(65, 130);
            this.p24_txtLedIndicator_condition.Name = "p24_txtLedIndicator_condition";
            this.p24_txtLedIndicator_condition.Size = new System.Drawing.Size(252, 20);
            this.p24_txtLedIndicator_condition.TabIndex = 136;
            this.p24_txtLedIndicator_condition.TextChanged += new System.EventHandler(this.txtLedIndicator_condition_TextChanged);

            // 
            // p24_txtMeterItemRotatorAZcommand
            // 
            this.p24_txtMeterItemRotatorAZcommand.Location = new System.Drawing.Point(39, 297);
            this.p24_txtMeterItemRotatorAZcommand.Name = "p24_txtMeterItemRotatorAZcommand";
            this.p24_txtMeterItemRotatorAZcommand.Size = new System.Drawing.Size(272, 20);
            this.p24_txtMeterItemRotatorAZcommand.TabIndex = 138;
            this.toolTip1.SetToolTip(this.p24_txtMeterItemRotatorAZcommand, "The AZ rotator string");
            this.p24_txtMeterItemRotatorAZcommand.TextChanged += new System.EventHandler(this.txtMeterItemRotatorAZcommand_TextChanged);

            // 
            // p24_txtMeterItemRotatorELEcommand
            // 
            this.p24_txtMeterItemRotatorELEcommand.Location = new System.Drawing.Point(39, 323);
            this.p24_txtMeterItemRotatorELEcommand.Name = "p24_txtMeterItemRotatorELEcommand";
            this.p24_txtMeterItemRotatorELEcommand.Size = new System.Drawing.Size(272, 20);
            this.p24_txtMeterItemRotatorELEcommand.TabIndex = 139;
            this.toolTip1.SetToolTip(this.p24_txtMeterItemRotatorELEcommand, "The ELE rotator string");
            this.p24_txtMeterItemRotatorELEcommand.TextChanged += new System.EventHandler(this.txtMeterItemRotatorELEcommand_TextChanged);

            // 
            // p24_txtMeterItemRotatorSTOPcommand
            // 
            this.p24_txtMeterItemRotatorSTOPcommand.Location = new System.Drawing.Point(50, 349);
            this.p24_txtMeterItemRotatorSTOPcommand.Name = "p24_txtMeterItemRotatorSTOPcommand";
            this.p24_txtMeterItemRotatorSTOPcommand.Size = new System.Drawing.Size(261, 20);
            this.p24_txtMeterItemRotatorSTOPcommand.TabIndex = 173;
            this.toolTip1.SetToolTip(this.p24_txtMeterItemRotatorSTOPcommand, "The ELE rotator string");
            this.p24_txtMeterItemRotatorSTOPcommand.TextChanged += new System.EventHandler(this.txtMeterItemRotatorSTOPcommand_TextChanged);

            // 
            // p24_txtMeterItem_custom_title
            // 
            this.p24_txtMeterItem_custom_title.Location = new System.Drawing.Point(54, 107);
            this.p24_txtMeterItem_custom_title.MaxLength = 20;
            this.p24_txtMeterItem_custom_title.Name = "p24_txtMeterItem_custom_title";
            this.p24_txtMeterItem_custom_title.Size = new System.Drawing.Size(150, 20);
            this.p24_txtMeterItem_custom_title.TabIndex = 114;
            this.p24_txtMeterItem_custom_title.TextChanged += new System.EventHandler(this.txtMeterItem_custom_title_TextChanged);

            // 
            // p24_txtMeterItem_custom_units
            // 
            this.p24_txtMeterItem_custom_units.Location = new System.Drawing.Point(54, 81);
            this.p24_txtMeterItem_custom_units.MaxLength = 4;
            this.p24_txtMeterItem_custom_units.Name = "p24_txtMeterItem_custom_units";
            this.p24_txtMeterItem_custom_units.Size = new System.Drawing.Size(56, 20);
            this.p24_txtMeterItem_custom_units.TabIndex = 112;
            this.p24_txtMeterItem_custom_units.TextChanged += new System.EventHandler(this.txtMeterItem_custom_units_TextChanged);

            // 
            // p24_txtRecording_4char
            // 
            this.p24_txtRecording_4char.Location = new System.Drawing.Point(61, 26);
            this.p24_txtRecording_4char.MaxLength = 4;
            this.p24_txtRecording_4char.Name = "p24_txtRecording_4char";
            this.p24_txtRecording_4char.Size = new System.Drawing.Size(43, 20);
            this.p24_txtRecording_4char.TabIndex = 183;
            this.p24_txtRecording_4char.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtRecording_4char, "The four character code used by CAT to access this Voice Record/Play item");
            this.p24_txtRecording_4char.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtRecording_4char_KeyPress);

            // 
            // p24_txtRecording_globalkeybind
            // 
            this.p24_txtRecording_globalkeybind.Enabled = false;
            this.p24_txtRecording_globalkeybind.Location = new System.Drawing.Point(6, 42);
            this.p24_txtRecording_globalkeybind.Name = "p24_txtRecording_globalkeybind";
            this.p24_txtRecording_globalkeybind.ReadOnly = true;
            this.p24_txtRecording_globalkeybind.Size = new System.Drawing.Size(150, 20);
            this.p24_txtRecording_globalkeybind.TabIndex = 180;
            this.p24_txtRecording_globalkeybind.Text = "unset";

            // 
            // p24_txtRecording_labelText
            // 
            this.p24_txtRecording_labelText.Location = new System.Drawing.Point(45, 1);
            this.p24_txtRecording_labelText.MaxLength = 32;
            this.p24_txtRecording_labelText.Name = "p24_txtRecording_labelText";
            this.p24_txtRecording_labelText.Size = new System.Drawing.Size(101, 20);
            this.p24_txtRecording_labelText.TabIndex = 1;
            this.p24_txtRecording_labelText.TextChanged += new System.EventHandler(this.txtRecording_labelText_TextChanged);

            // 
            // p24_txtRecording_playkeybind
            // 
            this.p24_txtRecording_playkeybind.Enabled = false;
            this.p24_txtRecording_playkeybind.Location = new System.Drawing.Point(3, 90);
            this.p24_txtRecording_playkeybind.Name = "p24_txtRecording_playkeybind";
            this.p24_txtRecording_playkeybind.ReadOnly = true;
            this.p24_txtRecording_playkeybind.Size = new System.Drawing.Size(143, 20);
            this.p24_txtRecording_playkeybind.TabIndex = 177;
            this.p24_txtRecording_playkeybind.Text = "unset";

            // 
            // p24_txtRotator_4charID
            // 
            this.p24_txtRotator_4charID.Location = new System.Drawing.Point(268, 272);
            this.p24_txtRotator_4charID.MaxLength = 4;
            this.p24_txtRotator_4charID.Name = "p24_txtRotator_4charID";
            this.p24_txtRotator_4charID.Size = new System.Drawing.Size(43, 20);
            this.p24_txtRotator_4charID.TabIndex = 167;
            this.p24_txtRotator_4charID.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtRotator_4charID, "The four character code used to reference which MultiMeterIO is to be used for th" +
        "e output data.");
            this.p24_txtRotator_4charID.TextChanged += new System.EventHandler(this.txtRotator_4charID_TextChanged);

            // 
            // p24_txtTextOverlay_RXText
            // 
            this.p24_txtTextOverlay_RXText.Location = new System.Drawing.Point(53, 130);
            this.p24_txtTextOverlay_RXText.Name = "p24_txtTextOverlay_RXText";
            this.p24_txtTextOverlay_RXText.Size = new System.Drawing.Size(264, 20);
            this.p24_txtTextOverlay_RXText.TabIndex = 136;
            this.p24_txtTextOverlay_RXText.TextChanged += new System.EventHandler(this.txtTextOverlay_RXText_TextChanged);

            // 
            // p24_txtTextOverlay_TXText
            // 
            this.p24_txtTextOverlay_TXText.Location = new System.Drawing.Point(53, 156);
            this.p24_txtTextOverlay_TXText.Name = "p24_txtTextOverlay_TXText";
            this.p24_txtTextOverlay_TXText.Size = new System.Drawing.Size(264, 20);
            this.p24_txtTextOverlay_TXText.TabIndex = 139;
            this.p24_txtTextOverlay_TXText.TextChanged += new System.EventHandler(this.txtTextOverlay_TXText_TextChanged);

            // 
            // p24_txtTextOverlay_rx_on_led_4char
            // 
            this.p24_txtTextOverlay_rx_on_led_4char.Location = new System.Drawing.Point(87, 238);
            this.p24_txtTextOverlay_rx_on_led_4char.MaxLength = 4;
            this.p24_txtTextOverlay_rx_on_led_4char.Name = "p24_txtTextOverlay_rx_on_led_4char";
            this.p24_txtTextOverlay_rx_on_led_4char.Size = new System.Drawing.Size(43, 20);
            this.p24_txtTextOverlay_rx_on_led_4char.TabIndex = 167;
            this.p24_txtTextOverlay_rx_on_led_4char.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtTextOverlay_rx_on_led_4char, "The four character id of the Led Indicator to query");
            this.p24_txtTextOverlay_rx_on_led_4char.TextChanged += new System.EventHandler(this.txtTextOverlay_rx_on_led_4char_TextChanged);

            // 
            // p24_txtTextOverlay_tx_on_led_4char
            // 
            this.p24_txtTextOverlay_tx_on_led_4char.Location = new System.Drawing.Point(219, 238);
            this.p24_txtTextOverlay_tx_on_led_4char.MaxLength = 4;
            this.p24_txtTextOverlay_tx_on_led_4char.Name = "p24_txtTextOverlay_tx_on_led_4char";
            this.p24_txtTextOverlay_tx_on_led_4char.Size = new System.Drawing.Size(43, 20);
            this.p24_txtTextOverlay_tx_on_led_4char.TabIndex = 169;
            this.p24_txtTextOverlay_tx_on_led_4char.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtTextOverlay_tx_on_led_4char, "The four character id of the Led Indicator to query");
            this.p24_txtTextOverlay_tx_on_led_4char.TextChanged += new System.EventHandler(this.txtTextOverlay_tx_on_led_4char_TextChanged);

            // 
            // p24_txtWebImage_4char
            // 
            this.p24_txtWebImage_4char.Location = new System.Drawing.Point(238, 86);
            this.p24_txtWebImage_4char.MaxLength = 4;
            this.p24_txtWebImage_4char.Name = "p24_txtWebImage_4char";
            this.p24_txtWebImage_4char.Size = new System.Drawing.Size(43, 20);
            this.p24_txtWebImage_4char.TabIndex = 116;
            this.p24_txtWebImage_4char.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtWebImage_4char, "The four character code for this web image");
            this.p24_txtWebImage_4char.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWebImage_4char_KeyPress);

            // 
            // p24_txtWebImage_background_4char
            // 
            this.p24_txtWebImage_background_4char.Location = new System.Drawing.Point(208, 166);
            this.p24_txtWebImage_background_4char.MaxLength = 4;
            this.p24_txtWebImage_background_4char.Name = "p24_txtWebImage_background_4char";
            this.p24_txtWebImage_background_4char.Size = new System.Drawing.Size(43, 20);
            this.p24_txtWebImage_background_4char.TabIndex = 150;
            this.p24_txtWebImage_background_4char.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.p24_txtWebImage_background_4char, "The four character code of the next webimage to show");
            this.p24_txtWebImage_background_4char.TextChanged += new System.EventHandler(this.txtWebImage_background_4char_TextChanged);

            // 
            // p24_txtWebImage_url
            // 
            this.p24_txtWebImage_url.Location = new System.Drawing.Point(45, 117);
            this.p24_txtWebImage_url.Name = "p24_txtWebImage_url";
            this.p24_txtWebImage_url.Size = new System.Drawing.Size(266, 20);
            this.p24_txtWebImage_url.TabIndex = 139;
            this.p24_txtWebImage_url.TextChanged += new System.EventHandler(this.txtWebImage_url_TextChanged);

            // 
            // p24_ucMeterItemSignalType
            // 
            this.p24_ucMeterItemSignalType.Location = new System.Drawing.Point(137, 195);
            this.p24_ucMeterItemSignalType.Name = "p24_ucMeterItemSignalType";
            this.p24_ucMeterItemSignalType.SignalType = PowerSDR.Reading.SIGNAL_STRENGTH;
            this.p24_ucMeterItemSignalType.Size = new System.Drawing.Size(162, 24);
            this.p24_ucMeterItemSignalType.TabIndex = 87;
            this.p24_ucMeterItemSignalType.SignalTypeChanged += new System.EventHandler<PowerSDR.ucSignalSelect.SignalTypeChangedEventArgs>(this.ucMeterItemSignalType_SignalTypeChanged);

            // 
            // p24_ucOtherButtonsOptionsGrid_buttons
            // 
            this.p24_ucOtherButtonsOptionsGrid_buttons.AutoScroll = true;
            this.p24_ucOtherButtonsOptionsGrid_buttons.BackColor = System.Drawing.SystemColors.ControlLight;
            this.p24_ucOtherButtonsOptionsGrid_buttons.Location = new System.Drawing.Point(532, 20);
            this.p24_ucOtherButtonsOptionsGrid_buttons.Name = "p24_ucOtherButtonsOptionsGrid_buttons";
            this.p24_ucOtherButtonsOptionsGrid_buttons.Size = new System.Drawing.Size(170, 178);
            this.p24_ucOtherButtonsOptionsGrid_buttons.TabIndex = 112;
            this.p24_ucOtherButtonsOptionsGrid_buttons.CheckboxChanged += new System.EventHandler(this.ucOtherButtonsOptionsGrid_buttons_CheckboxChanged);
            this.p24_ucOtherButtonsOptionsGrid_buttons.MacroSetupClicked += new System.EventHandler<PowerSDR.ucOtherButtonsOptionsGrid.MacroButtonEventArgs>(this.ucOtherButtonsOptionsGrid_buttons_MacroSetupClicked);

            // 
            // p24_ucTunestepOptionsGrid_buttons
            // 
            this.p24_ucTunestepOptionsGrid_buttons.Bitfield = 0;
            this.p24_ucTunestepOptionsGrid_buttons.Location = new System.Drawing.Point(532, 210);
            this.p24_ucTunestepOptionsGrid_buttons.Name = "p24_ucTunestepOptionsGrid_buttons";
            this.p24_ucTunestepOptionsGrid_buttons.Size = new System.Drawing.Size(170, 178);
            this.p24_ucTunestepOptionsGrid_buttons.TabIndex = 111;
            this.p24_ucTunestepOptionsGrid_buttons.CheckboxChanged += new System.EventHandler(this.ucTunestepOptionsGrid_buttons_checkbox_changed);



            TabControl appearanceTabs = P24FindAppearanceInnerTabs();
            if (appearanceTabs != null && !appearanceTabs.TabPages.Contains(p24_tpAppearanceMeter2))
            {
                p24_tpAppearanceMeter2.Text = "Meters/Gadgets";
                p24_tpAppearanceMeter2.BackColor = SystemColors.Control;
                p24_tpAppearanceMeter2.UseVisualStyleBackColor = false;
                appearanceTabs.TabPages.Add(p24_tpAppearanceMeter2);
            }

            P24ConfigureSetupGeometry();

            try
            {
                updateMeter2Controls();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("P24 Meters/Gadgets setup init: " + ex.Message);
            }
        }

        private TabControl P24FindAppearanceInnerTabs()
        {
            TabPage appearance = P24FindTabPage(this, "Appearance");
            if (appearance == null) return null;
            return P24FindBestTabControl(appearance);
        }

        private static TabPage P24FindTabPage(Control root, string text)
        {
            foreach (Control child in root.Controls)
            {
                TabPage tp = child as TabPage;
                if (tp != null && String.Equals((tp.Text ?? "").Trim(), text, StringComparison.OrdinalIgnoreCase))
                    return tp;
                TabPage nested = P24FindTabPage(child, text);
                if (nested != null) return nested;
            }
            return null;
        }

        private static TabControl P24FindBestTabControl(Control root)
        {
            TabControl best = null;
            foreach (Control child in root.Controls)
            {
                TabControl tc = child as TabControl;
                if (tc != null && (best == null || tc.TabPages.Count > best.TabPages.Count)) best = tc;
                TabControl nested = P24FindBestTabControl(child);
                if (nested != null && (best == null || nested.TabPages.Count > best.TabPages.Count)) best = nested;
            }
            return best;
        }


        private void P24SelectNativeMetersTab()
        {
            try
            {
                TabPage appearance = P24FindTabPage(this, "Appearance");
                if (appearance != null)
                {
                    TabControl top = appearance.Parent as TabControl;
                    if (top != null) top.SelectedTab = appearance;
                }
                TabControl inner = P24FindAppearanceInnerTabs();
                if (inner != null && p24_tpAppearanceMeter2 != null)
                    inner.SelectedTab = p24_tpAppearanceMeter2;
            }
            catch { }
        }

        private void P24ConfigureSetupGeometry()
        {
            try
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                MaximizeBox = true;
                MaximumSize = Size.Empty;
                AutoScroll = false;

                Rectangle wa = Screen.FromControl(this).WorkingArea;
                int minW = Math.Min(780, Math.Max(680, wa.Width - 20));
                int minH = Math.Min(560, Math.Max(500, wa.Height - 20));
                MinimumSize = new Size(minW, minH);

                int wantedW = Math.Min(900, wa.Width);
                int wantedH = Math.Min(650, wa.Height);
                if (Width < wantedW || Height < wantedH)
                {
                    int w = Math.Max(Width, wantedW);
                    int h = Math.Max(Height, wantedH);
                    Bounds = new Rectangle(
                        Math.Max(wa.Left, Math.Min(Left, wa.Right - w)),
                        Math.Max(wa.Top, Math.Min(Top, wa.Bottom - h)),
                        Math.Min(w, wa.Width),
                        Math.Min(h, wa.Height));
                }

                TabControl rootTabs = P24FindBestTabControl(this);
                if (rootTabs != null)
                {
                    rootTabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    int right = Math.Max(8, rootTabs.Left);
                    rootTabs.Size = new Size(
                        Math.Max(600, ClientSize.Width - rootTabs.Left - right),
                        Math.Max(390, ClientSize.Height - rootTabs.Top - 78));
                }

                TabControl appearanceTabs = P24FindAppearanceInnerTabs();
                if (appearanceTabs != null)
                {
                    appearanceTabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    appearanceTabs.Size = new Size(
                        Math.Max(730, appearanceTabs.Parent.ClientSize.Width - appearanceTabs.Left - 4),
                        Math.Max(420, appearanceTabs.Parent.ClientSize.Height - appearanceTabs.Top - 4));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("P24 setup geometry: " + ex.Message);
            }
        }

#region MultiMeter2
        // multimeter 2
        private const int MAX_CONTAINERS = 50;

        private class clsContainerComboboxItem
        {
            public string Text { get; set; }
            public string ID { get; set; }
            public int ListIndex { get; set; }

            public override string ToString()
            {
                return ListIndex.ToString() + " - " + Text;
            }
        }
        private class clsMeterTypeComboboxItem
        {
            private MeterType _meterType;
            private int _order;
            public clsMeterTypeComboboxItem(MeterType mt, int nOrder)
            {
                _meterType = mt;
                _order = nOrder;
            }
            public MeterType MeterType
            {
                get { return _meterType; }
                set { _meterType = value; }
            }
            public int Order
            {
                get { return _order; }
                set { _order = value; }
            }
            public override string ToString()
            {
                return MeterManager.MeterName(_meterType);
            }
        }
        private MeterManager.clsMeter meterFromSelectedContainer()
        {
            clsContainerComboboxItem cci = p24_comboContainerSelect.SelectedItem as clsContainerComboboxItem;
            if (cci == null) return null;

            return MeterManager.MeterFromId(cci.ID);
        }
        private void btnAddRX1Container_Click(object sender, EventArgs e)
        {
            if (MeterManager.TotalMeterContainers < MAX_CONTAINERS)
            {
                string sId = MeterManager.AddMeterContainer(1, false);
                updateMeter2Controls(sId);
            }
        }

        private void btnAddRX2Container_Click(object sender, EventArgs e)
        {
            if (MeterManager.TotalMeterContainers < MAX_CONTAINERS)
            {
                string sId = MeterManager.AddMeterContainer(2, false);
                updateMeter2Controls(sId);
            }
        }
        private string containerNameFromId(string id)
        {
            string notes = MeterManager.GetContainerNotes(id).Trim();
            int newlineIndex = notes.IndexOf('\n');
            if (newlineIndex >= 0)
            {
                notes = notes.Substring(0, newlineIndex);
            }
            if (notes.Length > 40)
            {
                notes = notes.Substring(0, 40);
            }
            if (string.IsNullOrEmpty(notes))
            {
                return "Container";
            }
            else
            {
                return notes;
            }
        }
        private void updateMeter2Controls(string sId = "")
        {
            bool bEnableAdd = MeterManager.TotalMeterContainers < MAX_CONTAINERS;

            p24_btnAddRX1Container.Enabled = bEnableAdd;

            p24_comboContainerSelect.Items.Clear();
            int i = 0;
            int nSelect = 0;

            // add the containers to the list
            foreach (KeyValuePair<string, ucMeter> kvp in MeterManager.MeterContainers.OrderBy((KeyValuePair<string, ucMeter> kvp2) => kvp2.Value.Sequence))
            {
                clsContainerComboboxItem cci = new clsContainerComboboxItem();

                cci.ID = kvp.Value.ID;
                cci.ListIndex = i + 1;

                cci.Text = containerNameFromId(cci.ID);

                p24_comboContainerSelect.Items.Add(cci);

                if (cci.ID == sId && nSelect == 0) nSelect = i;

                i++;
            }

            bool bEnableControls;
            if (p24_comboContainerSelect.Items.Count > 0)
            {
                p24_comboContainerSelect.SelectedIndex = nSelect;
                bEnableControls = true;
            }
            else
            {
                p24_comboContainerSelect.SelectedIndex = -1;
                p24_comboContainerSelect.Refresh(); // neeed as disabling the controls causes the update/invalidate to fail sometimes
                bEnableControls = false;
            }

            bool locked = p24_chkLockContainer.Checked;

            p24_btnContainer_save.Enabled = bEnableControls;// MeterManager.TotalMeterContainers > 0 && p24_comboContainerSelect.SelectedIndex > -1;
            p24_btnContainer_load.Enabled = bEnableAdd;// MeterManager.TotalMeterContainers < MAX_CONTAINERS;
            p24_btnContainer_dupe.Enabled = bEnableControls;// && p24_comboContainerSelect.SelectedIndex > -1;
            p24_btnRecoverContainer.Enabled = bEnableControls && !locked;
            p24_btnContainerDelete.Enabled = bEnableControls && !locked;
            p24_chkContainerHighlight.Enabled = bEnableControls;
            p24_comboContainerSelect.Enabled = bEnableControls;
            p24_clrbtnContainerBackground.Enabled = bEnableControls;
            p24_chkContainerBorder.Enabled = bEnableControls;
            p24_chkContainerNoTitle.Enabled = bEnableControls;
            p24_chkMultiMeter_auto_container_height.Enabled = bEnableControls;
            p24_chkLockContainer.Enabled = bEnableControls;
            p24_chkContainerShowRX.Enabled = bEnableControls;
            p24_chkContainerShowTX.Enabled = bEnableControls;
            p24_chkContainerMinimises.Enabled = bEnableControls;
            p24_txtContainerNotes.Enabled = bEnableControls;
            p24_lblMMContainerBackground.Enabled = bEnableControls;
            p24_lblMMContainerNotes.Enabled = bEnableControls;
            p24_lstMetersAvailable.Enabled = bEnableControls;
            p24_lstMetersInUse.Enabled = bEnableControls;
            p24_btnAddMeterItem.Enabled = bEnableControls && !locked;
            p24_btnRemoveMeterItem.Enabled = bEnableControls && !locked;
            p24_btnMeterUp.Enabled = bEnableControls && !locked && p24_lstMetersInUse.Items.Count > 0;
            p24_btnMeterDown.Enabled = bEnableControls && !locked && p24_lstMetersInUse.Items.Count > 0;

            p24_btnMeterCopySettings.Enabled = bEnableControls && p24_lstMetersInUse.Items.Count > 0;
            p24_btnMeterPasteSettings.Enabled = bEnableControls && p24_lstMetersInUse.Items.Count > 0;

            p24_radContainer_rx1_data.Enabled = bEnableControls;
            p24_radContainer_rx2_data.Enabled = bEnableControls;

            p24_chkContainer_hidewhennotused.Enabled = bEnableControls;

            if (!bEnableControls) p24_txtContainerNotes.Text = "";
            if (!bEnableControls) p24_comboContainerSelect.Text = "";

            updateMeterLists();
        }
        private void updateMeterLists()
        {
            p24_lstMetersInUse.Items.Clear();

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            List<clsMeterTypeComboboxItem> inuse = new List<clsMeterTypeComboboxItem>();
            List<clsMeterTypeComboboxItem> notinuse = new List<clsMeterTypeComboboxItem>();

            for (int n = 1; n < (int)MeterType.LAST; n++)
            {
                MeterType mt = (MeterType)n;

                if (m.HasMeterType(mt))
                {
                    List<int> orders = m.GetOrderForMeterType(mt); // can be muliple orders
                    foreach (int order in orders)
                    {
                        clsMeterTypeComboboxItem mtci = new clsMeterTypeComboboxItem(mt, order);
                        inuse.Add(mtci);
                    }
                }

                clsMeterTypeComboboxItem mtci2 = new clsMeterTypeComboboxItem(mt, -1);
                notinuse.Add(mtci2);
            }

            if (p24_lstMetersAvailable.Items.Count == 0)
            {
                foreach (clsMeterTypeComboboxItem mtci in notinuse)
                {
                    // work out where to add it alphabetically, per block, rx, tx, special
                    int insert_pos = findIndexForInsertOfSpecialItem(mtci, p24_lstMetersAvailable);
                    p24_lstMetersAvailable.Items.Insert(insert_pos, mtci);
                }
            }

            foreach (clsMeterTypeComboboxItem mtci in inuse.OrderBy(o => o.Order))
            {
                p24_lstMetersInUse.Items.Add(mtci);
            }

            lstMetersAvailable_SelectedIndexChanged(this, EventArgs.Empty);
            lstMetersInUse_SelectedIndexChanged(this, EventArgs.Empty);
        }
        private int findIndexForInsertOfSpecialItem(clsMeterTypeComboboxItem mtci, ListBox lb)
        {
            // block 0=rx, 1=tx, 2=special
            if (lb == null || mtci == null) return 0;
            if (lb.Items.Count == 0) return 0;

            MeterType t = mtci.MeterType;
            int block = -1;
            if ( ((int)t > (int)MeterType.NONE) && ((int)t <= (int)MeterType.ESTIMATED_PBSNR) || (int)t == (int)MeterType.ACG_MAX_MAG)
            {
                block = 0;
            }
            else if ( ((int)t >= (int)MeterType.MIC) && ((int)t <= (int)MeterType.SWR) )
            {
                block = 1;
            }
            else if ( ((int)t >= (int)MeterType.MAGIC_EYE) && ((int)t < (int)MeterType.LAST) )
            {
                block = 2;
            }
            else
                return 0;

            for (int n = 0; n < lb.Items.Count; n++)
            {
                clsMeterTypeComboboxItem mi = (clsMeterTypeComboboxItem)lb.Items[n];
                bool good_block = false;
                switch (block)
                {
                    case 0:
                        good_block = ((int)mi.MeterType > (int)MeterType.NONE) && ((int)mi.MeterType <= (int)MeterType.ESTIMATED_PBSNR);
                        break;
                    case 1:
                        good_block = ((int)mi.MeterType >= (int)MeterType.MIC) && ((int)mi.MeterType <= (int)MeterType.SWR);
                        break;
                    case 2:
                        good_block = ((int)mi.MeterType >= (int)MeterType.MAGIC_EYE) && ((int)mi.MeterType < (int)MeterType.LAST);
                        break;
                }
                if (!good_block) continue;

                if (string.Compare(mtci.ToString(), MeterManager.MeterName(mi.MeterType), true) < 0) return n;
            }
            return lb.Items.Count;
        }
        private void btnContainerDelete_Click(object sender, EventArgs e)
        {
            if (p24_chkLockContainer.Checked) return;
            if (preventIfContainerContainsLockedRecordings()) return;

            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;

            if (cci != null)
            {
                MeterManager.RemoveMeterContainer(cci.ID);
                int selected = p24_comboContainerSelect.SelectedIndex;
                p24_comboContainerSelect.Items.Remove(cci);

                updateMeter2Controls();

                setupMMSettingsGroupBoxes(MeterType.NONE);

                if (selected > p24_comboContainerSelect.Items.Count - 1) selected = p24_comboContainerSelect.Items.Count - 1;
                if (selected > -1) p24_comboContainerSelect.SelectedIndex = selected;
            }
        }

        private void comboContainerSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;

            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci == null) return;

            if (p24_chkContainerHighlight.Checked)
            {
                MeterManager.HighlightContainer(cci.ID);
            }

            p24_chkContainerBorder.Checked = MeterManager.ContainerHasBorder(cci.ID);
            p24_clrbtnContainerBackground.Color = MeterManager.GetContainerBackgroundColour(cci.ID);
            p24_chkContainerNoTitle.Checked = MeterManager.ContainerNoTitleBar(cci.ID);

            p24_chkLockContainer.Checked = MeterManager.ContainerLocked(cci.ID);
            chkLockContainer_CheckedChanged(this, EventArgs.Empty); // force it

            p24_chkContainer_hidewhennotused.Checked = MeterManager.ContainerHidesWhenRXNotUsed(cci.ID); //needs to be before the rx2/rx1 data radios below

            int rx = MeterManager.GetContainerRX(cci.ID);
            switch (rx)
            {
                case 2:
                    p24_radContainer_rx2_data.Checked = true;
                    radContainer_rx2_data_CheckedChanged(this, EventArgs.Empty); // force it
                    break;
                default:
                    p24_radContainer_rx1_data.Checked = true;
                    radContainer_rx1_data_CheckedChanged(this, EventArgs.Empty); // force it
                    break;
            }

            p24_chkContainerShowRX.Checked = MeterManager.ContainerShowOnRX(cci.ID);
            p24_chkContainerShowTX.Checked = MeterManager.ContainerShowOnTX(cci.ID);
            p24_chkContainerMinimises.Checked = MeterManager.ContainerMinimises(cci.ID);
            p24_txtContainerNotes.Text = MeterManager.GetContainerNotes(cci.ID);
            p24_chkMultiMeter_auto_container_height.Checked = MeterManager.ContainerAutoHeight(cci.ID);

            updateMeterLists();
        }

        private void chkContainerHighlight_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (p24_chkContainerHighlight.Checked)
            {
                clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
                if (cci != null)
                {
                    MeterManager.HighlightContainer(cci.ID);
                }
            }
            else
            {
                MeterManager.HighlightContainer("");
            }
        }
        private void chkContainerShowRX_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ShowContainerOnRX(cci.ID, p24_chkContainerShowRX.Checked);
            }
        }
        private void chkContainerShowTX_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ShowContainerOnTX(cci.ID, p24_chkContainerShowTX.Checked);
            }
        }
        private void txtContainerNotes_TextChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                string sTmp = MeterManager.GetContainerNotes(cci.ID);
                if (p24_txtContainerNotes.Text != sTmp)
                {
                    MeterManager.ContainerNotes(cci.ID, p24_txtContainerNotes.Text);

                    //update the cci if required, similar code as in updateMeter2Controls()

                    string notes = MeterManager.GetContainerNotes(cci.ID).Trim();
                    int newlineIndex = notes.IndexOf('\n');
                    if (newlineIndex >= 0)
                    {
                        notes = notes.Substring(0, newlineIndex);
                    }
                    if (notes.Length > 40)
                    {
                        notes = notes.Substring(0, 40);
                    }
                    if (string.IsNullOrEmpty(notes))
                    {
                        cci.Text = "Container";
                    }
                    else
                    {
                        cci.Text = notes;
                    }

                    // update it
                    int index = p24_comboContainerSelect.SelectedIndex;
                    if (index >= 0)
                    {
                        p24_comboContainerSelect.Items[p24_comboContainerSelect.SelectedIndex] = cci;
                    }
                }
            }
        }
        private void btnAddMeterItem_Click(object sender, EventArgs e)
        {
            if (p24_chkLockContainer.Checked) return;
            clsMeterTypeComboboxItem mti = p24_lstMetersAvailable.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            m.AddMeter(mti.MeterType);
            m.ZeroOut(true, true);
            m.Rebuild();
            updateMeterLists();

            lstMetersAvailable_SelectedIndexChanged(sender, e);
        }

        private void lstMetersAvailable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            p24_btnAddMeterItem.Enabled = !p24_chkLockContainer.Checked && p24_lstMetersAvailable.SelectedIndex >= 0;
        }

        private void lstMetersInUse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;

            bool bEnabled = p24_lstMetersInUse.SelectedIndex >= 0;

            if (bEnabled)
                updateItemSettingsControlsForSelected();
            else
                setupMMSettingsGroupBoxes(MeterType.NONE);

            p24_btnRemoveMeterItem.Enabled = !p24_chkLockContainer.Checked && bEnabled;
            p24_btnMeterUp.Enabled = !p24_chkLockContainer.Checked && bEnabled;
            p24_btnMeterDown.Enabled = !p24_chkLockContainer.Checked && bEnabled;

            p24_btnMeterCopySettings.Enabled = bEnabled;
            p24_btnMeterPasteSettings.Enabled = bEnabled && canPasteSettings();
        }

        private void btnRemoveMeterItem_Click(object sender, EventArgs e)
        {
            if (p24_chkLockContainer.Checked) return;
            if (preventIfItemContainsLockedRecordings()) return;

            clsMeterTypeComboboxItem mti = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            m.RemoveMeterType(mti.MeterType, mti.Order, true);

            updateMeterLists();

            lstMetersInUse_SelectedIndexChanged(sender, e);
        }

        private void btnMeterUp_Click(object sender, EventArgs e)
        {
            if (p24_chkLockContainer.Checked) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            int n = p24_lstMetersInUse.SelectedIndex - 1;
            if (n < 0) return;

            m.SetOrderForMeterType(mtci.MeterType, n, true, true, mtci.Order);

            updateMeterLists();

            p24_lstMetersInUse.SelectedIndex = n;
        }

        private void btnMeterDown_Click(object sender, EventArgs e)
        {
            if (p24_chkLockContainer.Checked) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            int n = p24_lstMetersInUse.SelectedIndex + 1;
            if (n > p24_lstMetersInUse.Items.Count - 1) return;

            m.SetOrderForMeterType(mtci.MeterType, n, true, false, mtci.Order);

            updateMeterLists();

            p24_lstMetersInUse.SelectedIndex = n;
        }

        private void lstMetersAvailable_DoubleClick(object sender, EventArgs e)
        {
            btnAddMeterItem_Click(sender, e);
        }

        private void lstMetersInUse_DoubleClick(object sender, EventArgs e)
        {
            btnRemoveMeterItem_Click(sender, e);
        }

        private void lstMetersAvailable_DrawItem(object sender, DrawItemEventArgs e)
        {
            lstMetersInUse_DrawItem(sender, e);
        }

        private void lstMetersInUse_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            Graphics g = e.Graphics;

            if (e.Index >= 0)
            {
                clsMeterTypeComboboxItem mtci = (clsMeterTypeComboboxItem)((ListBox)sender).Items[e.Index];
                if (mtci != null)
                {
                    SolidBrush sb;

                    int n = MeterManager.GetMeterTXRXType(mtci.MeterType);
                    switch (n)
                    {
                        case 0: //rx
                            sb = new SolidBrush(Color.PaleGreen);
                            break;
                        case 1: //tx
                            sb = new SolidBrush(Color.PaleVioletRed);
                            break;
                        default: //other
                            sb = new SolidBrush(Color.CornflowerBlue);
                            break;
                    }

                    Rectangle r = new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height);

                    g.FillRectangle(sb, r);
                    sb.Dispose();
                }
                SolidBrush sbt;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    sbt = new SolidBrush(Color.White);
                else
                    sbt = new SolidBrush(Color.Black);
                g.DrawString(" " + ((ListBox)sender).Items[e.Index].ToString(), e.Font, sbt, e.Bounds, StringFormat.GenericDefault);
                sbt.Dispose();

                e.DrawFocusRectangle();
            }
        }
        private string meterItemGroupIDfromSelected()
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return "";

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return "";
            if (!m.HasMeterType(mtci.MeterType)) return "";

            return m.MeterGroupID(mtci.MeterType, mtci.Order);
        }
        private MeterType meterItemGroupTypefromSelected()
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return MeterType.NONE;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return MeterType.NONE;
            if (!m.HasMeterType(mtci.MeterType)) return MeterType.NONE;

            return mtci.MeterType;
        }
        private void chkMeterItemHistory_CheckedChanged(object sender, EventArgs e)
        {
            bool bEnabled = p24_chkMeterItemHistory.Checked;

            updateHistoryControls(bEnabled, Color.Red, false);

            updateMeterType();
        }
        private MeterManager.clsIGSettings updateMeterType()
        {
            if (initializing || _ignoreMeterItemChangeEvents) return null;

            string mgID = meterItemGroupIDfromSelected();
            if (mgID == "") return null;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return null;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return null;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return null;

            MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt, mtci.Order);
            if (igs == null) return null;

            if (mt == MeterType.HISTORY)
            {
                igs.SetSetting<float>("history_vertical_ratio", (float)p24_nudHistory_vertical_ratio.Value);
                igs.SetSetting<System.Drawing.Color>("history_background_colour", p24_clrbtnHistory_background.Color);
                igs.SetSetting<float>("history_update", (float)p24_nudHistory_update.Value);
                igs.SetSetting<float>("history_keep_for", (float)p24_nudHistory_keep_for.Value);

                clsComboHistoryItem chi = p24_comboHistory_reading_0.SelectedItem as clsComboHistoryItem;
                if (chi != null)
                    igs.SetSetting<Reading>("history_reading_0", chi.Reading);
                else
                    igs.SetSetting<Reading>("history_reading_0", Reading.SIGNAL_STRENGTH);

                chi = p24_comboHistory_reading_1.SelectedItem as clsComboHistoryItem;
                if (chi != null)
                    igs.SetSetting<Reading>("history_reading_1", chi.Reading);
                else
                    igs.SetSetting<Reading>("history_reading_1", Reading.SIGNAL_STRENGTH);

                igs.SetSetting<bool>("history_auto_scale_0", p24_chkHistory_auto_0_scale.Checked);
                igs.SetSetting<float>("history_min_0", (float)p24_nudHistory_axis0_min.Value);
                igs.SetSetting<float>("history_max_0", (float)p24_nudHistory_axis0_max.Value);

                igs.SetSetting<bool>("history_show_scale_1", p24_chkHistory_1_show_axis.Checked);
                igs.SetSetting<bool>("history_auto_scale_1", p24_chkHistory_auto_1_scale.Checked);
                igs.SetSetting<float>("history_min_1", (float)p24_nudHistory_axis1_min.Value);
                igs.SetSetting<float>("history_max_1", (float)p24_nudHistory_axis1_max.Value);

                igs.SetSetting<System.Drawing.Color>("history_colour_0", p24_clrbtnHistory_colour_0.Color);
                igs.SetSetting<System.Drawing.Color>("history_colour_1", p24_clrbtnHistory_colour_1.Color);

                igs.SetSetting<System.Drawing.Color>("history_colour_lines", p24_clrbtnHistory_lines.Color);
                igs.SetSetting<System.Drawing.Color>("history_colour_time", p24_clrbtnHistory_time.Color);

                igs.FadeOnRx = p24_chkHistory_fade_rx.Checked;
                igs.FadeOnTx = p24_chkHistory_fade_tx.Checked;
            }
            else if (mt == MeterType.WAVE_RECORD)
            {
                if (_reset_waverecord_order_map) igs.SetSetting<short[]>("waverecord_order_map", null);

                igs.SetSetting<float>("waverecord_vertical_ratio", (float)p24_nudWaveRecord_vertical_ratio.Value);
                igs.SetSetting<float>("waverecord_radius", (float)p24_nudWaveRecord_radius.Value);
                igs.SetSetting<System.Drawing.Color>("waverecord_back_colour", p24_clrbtnWaveRecord_back.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_row_colour", p24_clrbtnWaveRecord_row.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_row_border_colour", p24_clrbtnWaveRecord_border.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_text_colour", p24_clrbtnWaveRecord_text.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_button_fill_colour", p24_clrbtnWaveRecord_button_fill.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_button_border_colour", p24_clrbtnWaveRecord_button_border.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_button_hover_colour", p24_clrbtnWaveRecord_button_hover.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_play_colour", p24_clrbtnWaveRecord_play.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_stop_colour", p24_clrbtnWaveRecord_stop.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_delete_colour", p24_clrbtnWaveRecord_delete.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_scrolltrack_colour", p24_clrbtnWaveRecord_scroll_track.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_scrollthumb_colour", p24_clrbtnWaveRecord_scroll_thumb.Color);
                igs.SetSetting<System.Drawing.Color>("waverecord_scrollthumb_hover_colour", p24_clrbtnWaveRecord_scroll_hover.Color);
                igs.FadeOnRx = p24_chkWaveRecord_fade_rx.Checked;
                igs.FadeOnTx = p24_chkWaveRecord_fade_tx.Checked;
            }
            else if (mt == MeterType.BAND_BUTTONS || mt == MeterType.MODE_BUTTONS || mt == MeterType.FILTER_BUTTONS ||
                     mt == MeterType.ANTENNA_BUTTONS || mt == MeterType.TUNESTEP_BUTTONS || mt == MeterType.DISCORD_BUTTONS ||
                     mt == MeterType.OTHER_BUTTONS || mt == MeterType.VOICE_RECORD_PLAY_BUTTONS)
            {
                if (mt == MeterType.VOICE_RECORD_PLAY_BUTTONS)
                {
                    if (_reset_button_map_layout) igs.SetSetting<short[]>("buttonbox_button_map", null);

                    igs.SetSetting<int>("buttonbox_recordplayback_slots", (int)p24_nudVoiceRecordingPlayback_slots.Value);
                    if (_selected_voice_slot > -1)
                    {
                        igs.SetSetting<string>("buttonbox_recordplayback_label_" + _selected_voice_slot.ToString(), p24_txtRecording_labelText.Text);
                        igs.SetSetting<bool>("buttonbox_recordplayback_locked_" + _selected_voice_slot.ToString(), p24_chkRecording_slot_locked.Checked);
                        igs.SetSetting<bool>("buttonbox_recordplayback_useskeybind_" + _selected_voice_slot.ToString(), p24_chkRecording_playkeybind.Checked);
                        igs.SetSetting<Keys>("buttonbox_recordplayback_keybind_" + _selected_voice_slot.ToString(), (Keys)p24_txtRecording_playkeybind.Tag);
                        igs.SetSetting<bool>("buttonbox_recordplayback_canrepeat_" + _selected_voice_slot.ToString(), p24_chkRecording_canRepeat.Checked);
                        igs.SetSetting<int>("buttonbox_recordplayback_repeatdelay_" + _selected_voice_slot.ToString(), (int)p24_nudRecording_repeatDelay.Value);
                        igs.SetSetting<double>("buttonbox_recordplayback_txgainadjust_" + _selected_voice_slot.ToString(), (double)p24_nudRecording_tx_gain_adjust.Value);
                        igs.SetSetting<bool>("buttonbox_recordplayback_ignoreplaytempchanges_" + _selected_voice_slot.ToString(), p24_chkRecording_ignore_play_tempchanges.Checked);
                        igs.SetSetting<bool>("buttonbox_recordplayback_ignorerecordtempchanges_" + _selected_voice_slot.ToString(), p24_chkRecording_ignore_record_tempchanges.Checked);
                    }

                    int max_buttons = (int)p24_nudVoiceRecordingPlayback_slots.Value;
                    if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                    if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                }
                else if (mt == MeterType.OTHER_BUTTONS)
                {
                    if (_reset_button_map_layout) igs.SetSetting<short[]>("buttonbox_button_map", null);

                    int max_buttons = 0;
                    for (int n = 0; n < OtherButtonIdHelpers.MAX_BITFIELD_GROUP; n++)
                    {
                        igs.SetSetting<int>("buttonbox_other_buttons_bitfield_" + n.ToString(), p24_ucOtherButtonsOptionsGrid_buttons.GetBitfield(n));
                        max_buttons += p24_ucOtherButtonsOptionsGrid_buttons.GetCheckedCount(n);
                    }
                    for (int n = 0; n < OtherButtonIdHelpers.MACRO_BUTTONS_PERGROUP; n++)
                    {
                        igs.SetSetting<OtherButtonMacroSettings>("buttonbox_other_buttons_macro_settings_" + n.ToString(), p24_ucOtherButtonsOptionsGrid_buttons.GetMacroSettings(n));
                    }

                    max_buttons = Math.Max(1, max_buttons);
                    if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                    if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                }
                else if (mt == MeterType.TUNESTEP_BUTTONS)
                {
                    igs.SetSetting<int>("buttonbox_tunestep_bitfield", p24_ucTunestepOptionsGrid_buttons.Bitfield);

                    int max_buttons = p24_ucTunestepOptionsGrid_buttons.GetCheckedCount();
                    max_buttons = Math.Max(1, max_buttons);
                    if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                    if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                }
                else if (mt == MeterType.ANTENNA_BUTTONS)
                {
                    igs.SetSetting<bool>("buttonbox_rx1", p24_chkButtonBox_antenna_rx1.Checked);
                    igs.SetSetting<bool>("buttonbox_rx2", p24_chkButtonBox_antenna_rx2.Checked);
                    igs.SetSetting<bool>("buttonbox_rx3", p24_chkButtonBox_antenna_rx3.Checked);
                    igs.SetSetting<bool>("buttonbox_tx1", p24_chkButtonBox_antenna_tx1.Checked);
                    igs.SetSetting<bool>("buttonbox_tx2", p24_chkButtonBox_antenna_tx2.Checked);
                    igs.SetSetting<bool>("buttonbox_tx3", p24_chkButtonBox_antenna_tx3.Checked);
                    igs.SetSetting<bool>("buttonbox_byp", p24_chkButtonBox_antenna_byp.Checked);
                    igs.SetSetting<bool>("buttonbox_ext1", p24_chkButtonBox_antenna_ext1.Checked);
                    igs.SetSetting<bool>("buttonbox_xvtr", p24_chkButtonBox_antenna_xvtr.Checked);
                    igs.SetSetting<bool>("buttonbox_rxtxant", p24_chkButtonBox_antenna_rxtxant.Checked);

                    int max_buttons = getTotalColumnsNeededForAntennaButtons();
                    max_buttons = Math.Max(1, max_buttons);
                    if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                    if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                }

                igs.SetSetting<int>("buttonbox_columns", (int)p24_nudBandButtons_columns.Value);
                igs.SetSetting<float>("buttonbox_border", (float)p24_nudBandButtons_border.Value);
                igs.SetSetting<float>("buttonbox_margin", (float)p24_nudBandButtons_margin.Value);
                igs.SetSetting<float>("buttonbox_radius", (float)p24_nudBandButtons_radius.Value);
                igs.SetSetting<float>("buttonbox_height_ratio", (float)p24_nudBandButtons_height_ratio.Value);

                igs.SetSetting<bool>("buttonbox_use_indicator", p24_chkBandButtons_use_indicator.Checked);
                igs.SetSetting<float>("buttonbox_indicator_border", (float)p24_nudBandButtons_indicator_border.Value);
                igs.SetSetting<System.Drawing.Color>("buttonbox_on_colour", p24_clrbtnBandButtons_indicator_on.Color);
                igs.SetSetting<System.Drawing.Color>("buttonbox_off_colour", p24_clrbtnBandButtons_indicator_off.Color);

                igs.SetSetting<System.Drawing.Color>("buttonbox_fill_colour", p24_clrbtnBandButtons_fill.Color);
                igs.SetSetting<System.Drawing.Color>("buttonbox_hover_colour", p24_clrbtnBandButtons_hover.Color);
                igs.SetSetting<System.Drawing.Color>("buttonbox_border_colour", p24_clrbtnBandButtons_border.Color);

                igs.SetSetting<System.Drawing.Color>("buttonbox_click_colour", p24_clrbtnButonBox_click.Color);
                igs.SetSetting<System.Drawing.Color>("buttonbox_font_colour", p24_clrbtnButonBox_fontcolour.Color);

                igs.SetSetting<bool>("buttonbox_use_off_colour", p24_chkBandButtons_band_inactive_use.Checked);

                igs.SetSetting<MeterManager.clsButtonBox.IndicatorType>("buttonbox_indicator_type", (MeterManager.clsButtonBox.IndicatorType)((int)p24_nudBandButtons_indicator_style.Value));

                igs.SetSetting<float>("buttonbox_font_scale", (float)p24_nudButtonBox_font_scale.Value);
                igs.SetSetting<float>("buttonbox_font_shift_x", (float)p24_nudButtonBox_font_x_shift.Value);
                igs.SetSetting<float>("buttonbox_font_shift_y", (float)p24_nudButtonBox_font_y_shift.Value);

                igs.SetSetting<bool>("buttonbox_fix_text_size", p24_chkButtonBox_fix_text_size.Checked);
                igs.SetSetting<bool>("buttonbox_use_icons", p24_chkButtonBox_use_icons.Checked);

                if (_bandButtons_font != null)
                {
                    igs.FontFamily1 = _bandButtons_font.FontFamily.Name;
                    igs.FontStyle1 = _bandButtons_font.Style;
                }

                igs.FadeOnRx = p24_chkBandButtons_fade_rx.Checked;
                igs.FadeOnTx = p24_chkBandButtons_fade_tx.Checked;
            }
            else if (mt == MeterType.DIAL_DISPLAY)
            {
                igs.FadeOnRx = p24_chkDialDisplay_fade_rx.Checked;
                igs.FadeOnTx = p24_chkDialDisplay_fade_tx.Checked;

                igs.SetSetting<float>("dialdisplay_vertical_ratio", (float)p24_nudDialDisplay_vertical_ratio.Value);
                igs.SetSetting<float>("dialdisplay_font_scale", (float)p24_nudDialDisplay_font_scale.Value);
                igs.SetSetting<bool>("dialdisplay_alwaysshow_vfos", p24_chkDialDisplay_alwaysshow_vfos.Checked);
                igs.SetSetting<bool>("dialdisplay_align_with_tunestep", p24_chkDial_align.Checked);

                igs.SetSetting<int>("dialdisplay_increment", (int)p24_nudDial_increment.Value);
                igs.SetSetting<int>("dialdisplay_decrement", (int)p24_nudDial_decrement.Value);
                igs.SetSetting<int>("dialdisplay_interval", (int)p24_nudDial_interval.Value);
                igs.SetSetting<int>("dialdisplay_max_increments", (int)p24_nudDial_max_increments.Value);
                igs.SetSetting<int>("dialdisplay_degrees_for_change", (int)p24_nudDial_degrees_for_change.Value);

                igs.SetSetting<System.Drawing.Color>("dialdisplay_text", p24_clrbtnDial_text.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_cirlce", p24_clrbtnDial_circle.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_pad", p24_clrbtnDial_pad.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_pad_pressed", p24_clrbtnDial_pad_pressed.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_button_on", p24_clrbtnDial_button_on.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_button_off", p24_clrbtnDial_button_off.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_button_highlight", p24_clrbtnDial_button_highlight.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_ring", p24_clrbtnDial_ring.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_slow", p24_clrbtnDial_slow.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_hold", p24_clrbtnDial_hold.Color);
                igs.SetSetting<System.Drawing.Color>("dialdisplay_fast", p24_clrbtnDial_fast.Color);
            }
            else if (mt == MeterType.WEB_IMAGE)
            {
                igs.UpdateInterval = (int)p24_nudWebImage_update_interval.Value;
                igs.EyeScale = (float)p24_nudWebImage_width_scale.Value;
                igs.FadeOnRx = p24_chkWebImage_fade_rx.Checked;
                igs.FadeOnTx = p24_chkWebImage_fade_tx.Checked;
                igs.Text1 = p24_txtWebImage_url.Text;
                igs.DarkMode = p24_chkWebImage_bypass_cache.Checked;

                igs.SetSetting<bool>("webimage_background", p24_chkWebImage_background.Checked);
                igs.SetSetting<int>("webimage_background_interval", (int)p24_nudWebImage_background_time.Value);
                igs.SetSetting<string>("webimage_background_4char", p24_txtWebImage_background_4char.Text);
            }
            else if (mt == MeterType.ROTATOR)
            {
                igs.UpdateInterval = (int)p24_nudMeterItemUpdateRateRotator.Value;
                igs.Colour = Color.FromArgb(255, p24_clrbtnMeterItemHBackgroundRotator.Color);
                igs.TitleColor = p24_clrbtnMeterItemRotatorArrow.Color;
                igs.MarkerColour = p24_clrbtnMeterItemRotatorLargeDot.Color;
                igs.SubMarkerColour = p24_clrbtnMeterItemRotatorSmallDot.Color;
                igs.ShowMarker = p24_chkMeterItemRotatorShowBeamWidth.Checked;
                igs.LowColor = p24_clrbtnMeterItemRotatorBeamWidth.Color;
                igs.HighColor = p24_clrbtnMeterItemRotatorText.Color;
                igs.ShowHistory = p24_chkMeterItemRotatorCardinals.Checked;
                igs.FadeOnRx = p24_chkMeterItemFadeOnRxRotator.Checked;
                igs.FadeOnTx = p24_chkMeterItemFadeOnTxRotator.Checked;
                igs.DarkMode = p24_chkMeterItemDarkModeRotator.Checked;
                igs.AttackRatio = (float)p24_nudMeterItemRotatorBeamWidth.Value;
                igs.EyeScale = (float)p24_nudMeterItemRotator_padding.Value;

                //
                if (p24_radMeterItemRotator_show_az.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsRotatorItem.RotatorMode.AZ;
                else if (p24_radMeterItemRotator_show_ele.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsRotatorItem.RotatorMode.ELE;
                else if (p24_radMeterItemRotator_show_both.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsRotatorItem.RotatorMode.BOTH;
                //

                igs.ShowType = p24_chkMeterItemRotatorAllowControl.Checked;
                igs.HistoryColor = p24_clrbtnMeterItemRotatorControlColour.Color;
                igs.Text1 = p24_txtMeterItemRotatorAZcommand.Text;
                igs.Text2 = p24_txtMeterItemRotatorELEcommand.Text;
                igs.FontFamily1 = p24_txtMeterItemRotatorSTOPcommand.Text;

                Guid guid = MultiMeterIO.GuidfromFourChar(p24_txtRotator_4charID.Text);
                if (guid != Guid.Empty)
                {
                    igs.SetMMIOGuid(2, guid);
                }
                else
                {
                    igs.SetMMIOGuid(2, Guid.Empty);
                }

                igs.SetSetting<float>("rotator_beamwidth_alpha", (float)p24_nudMeterItemRotatorBeamWidth_alpha.Value);
            }
            else if (mt == MeterType.DATA_OUT)
            {
                Guid guid = MultiMeterIO.GuidfromFourChar(p24_txtDataOutNode_4charID.Text);
                if (guid != Guid.Empty)
                {
                    igs.SetMMIOGuid(0, guid);
                    igs.UpdateInterval = (int)p24_nudDataOutNode_sendinterval.Value;
                }
                else
                {
                    igs.SetMMIOGuid(0, Guid.Empty);
                    igs.UpdateInterval = 500;
                }
            }
            else if (mt == MeterType.SIGNAL_TEXT)
            {
                igs.UpdateInterval = (int)p24_nudMeterItemUpdateRate.Value;
                igs.AttackRatio = (float)Math.Round(p24_nudMeterItemAttackRate.Value, 3);
                igs.DecayRatio = (float)p24_nudMeterItemDecayRate.Value;
                igs.FadeOnRx = p24_chkMeterItemFadeOnRx.Checked;
                igs.FadeOnTx = p24_chkMeterItemFadeOnTx.Checked;
                igs.Colour = p24_clrbtnMeterItemHBackground.Color;
                igs.MarkerColour = p24_clrbtnMeterItemIndicator.Color;
                igs.SubMarkerColour = p24_clrbtnMeterItemSubIndicator.Color;
                igs.ShowSubMarker = p24_chkMeterItemShowSubIndicator.Checked;
                igs.PeakValueColour = p24_clrbtnMeterItemPeakValueColour.Color;
                igs.PeakValue = p24_chkMeterItemPeakValue.Checked;
                igs.ShowType = p24_chkMeterItemTitle.Checked;
                igs.TitleColor = p24_clrbtnMeterItemMeterTitle.Color;
                switch (p24_ucMeterItemSignalType.SignalType)
                {
                    case Reading.AVG_SIGNAL_STRENGTH:
                        igs.Average = true;
                        igs.MaxBin = false;
                        break;
                    case Reading.SIGNAL_MAX_BIN:
                        igs.Average = false;
                        igs.MaxBin = true;
                        break;
                    default:
                        igs.Average = false;
                        igs.MaxBin = false;
                        break;
                }
                igs.HistoryDuration = (int)p24_nudMeterItemHistoryDuration.Value;
                igs.IgnoreHistoryDuration = (int)p24_nudMeterItemIgnoreHistoryDuration.Value;
            }
            else if (mt == MeterType.VFO_DISPLAY)
            {
                igs.Colour = p24_clrbtnMMVfoDisplayBackground.Color;
                igs.TitleColor = p24_clrbtnMMVfoDisplayTitle.Color;

                //using exisinng igs settings
                igs.MarkerColour = p24_clrbtnMMVfoDisplayFrequency.Color;
                igs.SubMarkerColour = p24_clrbtnMMVfoDisplayMode.Color;
                igs.LowColor = p24_clrbtnMMVfoDisplaySplitBack.Color;
                igs.HighColor = p24_clrbtnMMVfoDisplaySplit.Color;
                igs.PeakValueColour = p24_clrbtnMMVfoDisplayRx.Color;
                igs.PeakHoldMarkerColor = p24_clrbtnMMVfoDisplayTx.Color;
                igs.HistoryColor = p24_clrbtnMMVfoDisplayFilter.Color;
                igs.SegmentedSolidLowColour = p24_clrbtnMMVfoDisplayBand.Color;
                igs.PowerScaleColour = p24_clrbtnMMVfoDigitHighlight.Color;

                igs.SetSetting<bool>("vfo_showbandtext", p24_chkMultiMeter_vfo_show_bandtext.Checked);
                igs.SetSetting<System.Drawing.Color>("vfo_showbandtext_colour", p24_clrbtnMultiMeter_vfo_show_bandtext.Color);
                igs.SetSetting<System.Drawing.Color>("vfo_frequency_small_numbers_colour", p24_clrbtnMMVfoDisplayFrequency_small.Color);

                igs.SetSetting<System.Drawing.Color>("vfo_lock_colour", p24_clrbtnMultiMeter_vfo_lock.Color);
                igs.SetSetting<System.Drawing.Color>("vfo_sync_colour", p24_clrbtnMultiMeter_vfo_sync.Color);

                if (p24_radMultiMeter_vfo_display_both.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsVfoDisplay.VFODisplayMode.VFO_BOTH;
                else if (p24_radMultiMeter_vfo_display_vfoa.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsVfoDisplay.VFODisplayMode.VFO_A;
                else if (p24_radMultiMeter_vfo_display_vfob.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsVfoDisplay.VFODisplayMode.VFO_B;
            }
            else if (mt == MeterType.CLOCK)
            {
                igs.Colour = p24_clrbtnMMClockBackground.Color;
                igs.ShowType = p24_chkMMClockTitle.Checked;
                igs.TitleColor = p24_clrbtnMMClockTitle.Color;
                igs.MarkerColour = p24_clrbtnMMTime.Color;
                igs.SubMarkerColour = p24_clrbtnMMDate.Color;
                igs.ShowMarker = p24_radMM24Clock.Checked; // use the show marker bool for this                
            }
            else if (mt == MeterType.LED)
            {
                igs.FadeOnRx = p24_chkLedIndicator_FadeOnRX.Checked;
                igs.FadeOnTx = p24_chkLedIndicator_FadeOnTX.Checked;
                igs.Colour = p24_clrbtnLedIndicator_true.Color;
                igs.MarkerColour = p24_clrbtnLedIndicator_false.Color;

                igs.TitleColor = p24_clrbtnLedIndicator_PanelBackground.Color;
                igs.HistoryColor = p24_clrbtnLedIndicator_PanelBackgroundTX.Color;
                igs.ShowSubMarker = p24_chkLedIndicator_ShowPanel.Checked;

                igs.EyeScale = (float)p24_nudLedIndicator_xOffset.Value;
                igs.EyeBezelScale = (float)p24_nudLedIndicator_yOffset.Value;
                igs.AttackRatio = (float)p24_nudLedIndicator_xSize.Value;
                igs.DecayRatio = (float)p24_nudLedIndicator_ySize.Value;
                igs.UpdateInterval = (int)p24_nudLedIndicator_UpdateInterval.Value;

                igs.Text1 = p24_txtLedIndicator_condition.Text;

                igs.SpacerPadding = (float)p24_nudLedIndicator_PanelPadding.Value;

                igs.PeakHold = p24_chkLed_show_true.Checked;
                igs.ShowMarker = p24_chkLed_show_false.Checked;
                if (p24_radLed_light_on_off.Checked)
                    igs.IgnoreHistoryDuration = 0;
                else if (p24_radLed_light_blink.Checked)
                    igs.IgnoreHistoryDuration = 1;
                else if (p24_radLed_light_pulsate.Checked)
                    igs.IgnoreHistoryDuration = 2;
                // also showhistory + showtype are return states for valid/error

                igs.SetSetting<bool>("led_notx_true", p24_chkLed_notx_true.Checked);
                igs.SetSetting<bool>("led_notx_false", p24_chkLed_notx_false.Checked);
                igs.SetSetting<bool>("led_process_when_hidden", p24_chkLed_process_when_hidden.Checked);
            }
            else if (mt == MeterType.TEXT_OVERLAY)
            {
                igs.FadeOnRx = p24_chkTextOverlay_FadeOnRX.Checked;
                igs.FadeOnTx = p24_chkTextOverlay_FadeOnTX.Checked;
                igs.Colour = p24_clrbtnTextOverlay_TextColour1.Color;
                igs.MarkerColour = p24_clrbtnTextOverlay_TextColour2.Color;
                igs.SubMarkerColour = p24_clrbtnTextOverlay_TextBackColour1.Color;
                igs.ShowMarker = p24_chkTextOverlay_textback1.Checked;
                igs.PeakValueColour = p24_clrbtnTextOverlay_TextBackColour2.Color;
                igs.ShowType = p24_chkTextOverlay_textback2.Checked;

                igs.TitleColor = p24_clrbtnTextOverlay_PanelBackground.Color;
                igs.HistoryColor = p24_clrbtnTextOverlay_PanelBackgroundTX.Color;
                igs.ShowSubMarker = p24_chkTextOverlay_ShowPanel.Checked;

                igs.EyeScale = (float)p24_nudTextOverlay_RXxOffset.Value;
                igs.EyeBezelScale = (float)p24_nudTextOverlay_RXyOffset.Value;
                igs.AttackRatio = (float)p24_nudTextOverlay_TXxOffset.Value;
                igs.DecayRatio = (float)p24_nudTextOverlay_TXyOffset.Value;

                igs.Text1 = p24_txtTextOverlay_RXText.Text;
                igs.Text2 = p24_txtTextOverlay_TXText.Text;

                if (_textOverlayFont1 != null)
                {
                    igs.FontFamily1 = _textOverlayFont1.FontFamily.Name;
                    igs.FontStyle1 = _textOverlayFont1.Style;
                    igs.FontSize1 = _textOverlayFont1.Size;
                }
                if (_textOverlayFont2 != null)
                {
                    igs.FontFamily2 = _textOverlayFont2.FontFamily.Name;
                    igs.FontStyle2 = _textOverlayFont2.Style;
                    igs.FontSize2 = _textOverlayFont2.Size;
                }

                igs.SpacerPadding = (float)p24_nudTextOverlay_PanelPadding.Value;

                igs.SetSetting<bool>("textoverlay_rx_ledlogic", p24_chkTextOverlay_rx_on_led.Checked);
                igs.SetSetting<bool>("textoverlay_tx_ledlogic", p24_chkTextOverlay_tx_on_led.Checked);

                igs.SetSetting<string>("textoverlay_rx_4char", p24_txtTextOverlay_rx_on_led_4char.Text);
                igs.SetSetting<string>("textoverlay_tx_4char", p24_txtTextOverlay_tx_on_led_4char.Text);
            }
            else if (mt == MeterType.SPACER)
            {
                igs.Colour = p24_clrbtnMeterItemHBackgroundSpacerRX.Color;
                igs.MarkerColour = p24_clrbtnMeterItemHBackgroundSpacerTX.Color;
                igs.FadeOnRx = p24_chkMeterItemFadeOnRxSpacer.Checked;
                igs.FadeOnTx = p24_chkMeterItemFadeOnTxSpacer.Checked;
                igs.SpacerPadding = (float)p24_nudMeterItemSpacerPadding.Value;
            }
            else if (mt == MeterType.FILTER_DISPLAY)
            {
                igs.Colour = p24_clrbtnFilterDisplay_backcolour.Color;
                igs.FadeOnRx = p24_chkFilterDisplay_fadeonrx.Checked;
                igs.FadeOnTx = p24_chkFilterDisplay_fadeontx.Checked;

                igs.SetSetting<float>("filterdisplay_vertical_ratio", (float)p24_nudFilterDisplay_vertical_ratio.Value);

                igs.SetSetting<bool>("filterdisplay_show_filter_limits", p24_chkFilterDisplay_show_limits.Checked);
                igs.SetSetting<bool>("filterdisplay_show_fixed_rx_zoom", p24_chkFilterDisplay_fixed_zoom.Checked);
                igs.SetSetting<bool>("filterdisplay_show_fixed_tx_zoom", p24_chkFilterDisplay_fixed_tx_zoom.Checked);
                igs.SetSetting<float>("filterdisplay_rx_zoom", (float)p24_nudFilterDisplay_fixed_zoom_level.Value);
                igs.SetSetting<float>("filterdisplay_tx_zoom", (float)p24_nudFilterDisplay_fixed_tx_zoom_level.Value);

                igs.SetSetting<float>("filterdisplay_sidebands_scale", (float)p24_nudFilterItem_sidebands_scale.Value);
                igs.SetSetting<float>("filterdisplay_cw_scale", (float)p24_nudFilterItem_cw_scale.Value);
                igs.SetSetting<float>("filterdisplay_others_scale", (float)p24_nudFilterItem_others_scale.Value);

                if (p24_radFilterItem_panadaptor.Checked)
                {
                    igs.SetSetting<MeterManager.clsFilterItem.FIDisplayMode>("filterdisplay_others_displaymode", MeterManager.clsFilterItem.FIDisplayMode.PANADAPTOR);
                }
                else if (p24_radFilterItem_waterfall.Checked)
                {
                    igs.SetSetting<MeterManager.clsFilterItem.FIDisplayMode>("filterdisplay_others_displaymode", MeterManager.clsFilterItem.FIDisplayMode.WATERFALL);
                }
                else if (p24_radFilterItem_panafall.Checked)
                {
                    igs.SetSetting<MeterManager.clsFilterItem.FIDisplayMode>("filterdisplay_others_displaymode", MeterManager.clsFilterItem.FIDisplayMode.PANAFALL);
                }
                else if (p24_radFilterItem_none.Checked)
                {
                    igs.SetSetting<MeterManager.clsFilterItem.FIDisplayMode>("filterdisplay_others_displaymode", MeterManager.clsFilterItem.FIDisplayMode.NONE);
                }

                igs.SetSetting<float>("filterdisplay_font_scale", (float)p24_nudFilterItem_font_scale.Value);

                igs.SetSetting<bool>("filterdisplay_fill_spec", p24_chkFilter_fill_spec.Checked);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_dataline_colour", p24_clrbtnFilter_data_line.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_datafill_colour", p24_clrbtnFilter_data_fill.Color);
                igs.SetSetting<MeterManager.clsFilterItem.FIWaterfallPalette>("filterdisplay_wf_palette", (MeterManager.clsFilterItem.FIWaterfallPalette)p24_comboFilter_wf_palette.SelectedIndex);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_wflow_colour", p24_clrbtnFilter_wf_low.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_text_colour", p24_clrbtnFilter_text.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_numberhighlight_colour", p24_clrbtnFilter_number_highlight.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_edges_colour", p24_clrbtnFilter_edges.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_edges_colour_tx", p24_clrbtnFilter_edges_tx.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_edgehighlight_colour", p24_clrbtnFilter_edge_highlight.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_meterback_colour", p24_clrbtnFilter_meter_back.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_notch_colour", p24_clrbtnFilter_notch.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_notchhighlight_colour", p24_clrbtnFilter_notch_highlight.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_extents_colour", p24_clrbtnFilter_extents.Color);
                igs.SetSetting<bool>("filterdisplay_sideband_mode", p24_chkFilter_sideband_mode.Checked);
                igs.SetSetting<int>("filterdisplay_waterfall_frameupdate", (int)p24_nudFilter_waterfall_frame_update.Value);
                igs.SetSetting<bool>("filterdisplay_use_grey", p24_chkFilter_grey_outsidepb.Checked);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_snap_line_colour", p24_clrbtnFilter_snap_line.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_settingon_colour", p24_clrbtnFilter_setting_on.Color);
                igs.SetSetting<System.Drawing.Color>("filterdisplay_button_highlight_colour", p24_clrbtnFilter_button_highlight.Color);
                igs.SetSetting<bool>("filterdisplay_characteristic", p24_chkFilter_characteristic.Checked);
                igs.SetSetting<float>("filterdisplay_characteristic_low", (float)p24_nudFilter_lower_characteristic.Value);
            }
            else
            {
                //custom meter bar
                if (mt == MeterType.CUSTOM_METER_BAR)
                {
                    igs.SetSetting<float>("meter_custom_min", (float)p24_nudMeterItem_custom_min.Value);
                    igs.SetSetting<float>("meter_custom_max", (float)p24_nudMeterItem_custom_max.Value);
                    igs.SetSetting<float>("meter_custom_high", (float)p24_nudMeterItem_custom_high.Value);
                    igs.SetSetting<string>("meter_custom_units", p24_txtMeterItem_custom_units.Text);
                    igs.SetSetting<string>("meter_custom_title", p24_txtMeterItem_custom_title.Text);
                }
                //

                igs.LowColor = Color.FromArgb(255, p24_clrbtnMeterItemLow.Color);
                igs.HighColor = Color.FromArgb(255, p24_clrbtnMeterItemHigh.Color);
                igs.MarkerColour = Color.FromArgb(255, p24_clrbtnMeterItemIndicator.Color);
                igs.SubMarkerColour = Color.FromArgb(255, p24_clrbtnMeterItemSubIndicator.Color);
                igs.ShowMarker = p24_chkMeterItemShowIndicator.Checked;
                igs.ShowSubMarker = p24_chkMeterItemShowSubIndicator.Checked;
                igs.Colour = Color.FromArgb(255, p24_clrbtnMeterItemHBackground.Color);
                igs.UpdateInterval = (int)p24_nudMeterItemUpdateRate.Value;
                igs.AttackRatio = (float)Math.Round(p24_nudMeterItemAttackRate.Value, 3);
                igs.DecayRatio = (float)p24_nudMeterItemDecayRate.Value;
                igs.ShowHistory = p24_chkMeterItemHistory.Checked;
                igs.HistoryColor = Color.FromArgb(p24_tbMeterItemHistoryAlpha.Value, p24_clrbtnMeterItemHistory.Color);
                igs.Shadow = p24_chkMeterItemShadow.Checked;
                igs.HistoryDuration = (int)p24_nudMeterItemHistoryDuration.Value;
                igs.IgnoreHistoryDuration = (int)p24_nudMeterItemIgnoreHistoryDuration.Value;

                if (p24_chkMeterItemSegmented.Checked)
                    igs.BarStyle = MeterManager.clsBarItem.BarStyle.Segments;
                else if (p24_chkMeterItemSolid.Checked)
                    igs.BarStyle = MeterManager.clsBarItem.BarStyle.SolidFilled;
                else
                    igs.BarStyle = MeterManager.clsBarItem.BarStyle.Line;

                igs.SegmentedSolidLowColour = p24_clrbtnMeterItemSegmentedSolidColourLow.Color;
                igs.SegmentedSolidHighColour = p24_clrbtnMeterItemSegmentedSolidColourHigh.Color;

                igs.PeakHold = p24_chkMeterItemPeakHold.Checked;
                igs.PeakHoldMarkerColor = Color.FromArgb(255, p24_clrbtnMeterItemPeakHold.Color);
                igs.HistoryDuration = (int)p24_nudMeterItemHistoryDuration.Value;
                igs.FadeOnRx = p24_chkMeterItemFadeOnRx.Checked;
                igs.FadeOnTx = p24_chkMeterItemFadeOnTx.Checked;
                igs.ShowType = p24_chkMeterItemTitle.Checked;
                igs.TitleColor = p24_clrbtnMeterItemMeterTitle.Color;
                igs.PeakValue = p24_chkMeterItemPeakValue.Checked;
                igs.PeakValueColour = p24_clrbtnMeterItemPeakValueColour.Color;
                igs.EyeScale = (float)p24_nudMeterItemEyeScale.Value;
                igs.EyeBezelScale = (float)p24_nudMeterItemEyeBezelScale.Value;
                igs.MaxPower = (float)p24_nudMeterItemsPowerLimit.Value;
                igs.PowerScaleColour = p24_clrbtnMeterItemPowerScale.Color;

                if (mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE)
                {
                    switch (p24_ucMeterItemSignalType.SignalType)
                    {
                        case Reading.AVG_SIGNAL_STRENGTH:
                            igs.Average = true;
                            igs.MaxBin = false;
                            break;
                        case Reading.SIGNAL_MAX_BIN:
                            igs.Average = false;
                            igs.MaxBin = true;
                            break;
                        default:
                            igs.Average = false;
                            igs.MaxBin = false;
                            break;
                    }
                }
                if (mt == MeterType.ANANMM || mt == MeterType.CROSS) igs.DarkMode = p24_chkMeterItemDarkMode.Checked;
            }

            m.ApplySettingsForMeterGroup(mt, igs, null, mtci.Order, true);

            updateLedValidControls();
            return igs;
        }
        private bool _ignoreMeterItemChangeEvents = false;
        private void updateItemSettingsControlsForSelected()
        {
            if (initializing) return;

            string mgID = meterItemGroupIDfromSelected();
            if (mgID == "") return;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return;

            MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt, mtci.Order);
            if (igs == null) return;

            _ignoreMeterItemChangeEvents = true;

            // setup any meter that has variable % buttons, ignore those that do not
            if (mt != MeterType.ROTATOR && mt != MeterType.SIGNAL_TEXT && mt != MeterType.VFO_DISPLAY && mt != MeterType.CLOCK &&
                mt != MeterType.TEXT_OVERLAY && mt != MeterType.SPACER && mt != MeterType.LED &&
                mt != MeterType.BAND_BUTTONS && mt != MeterType.MODE_BUTTONS && mt != MeterType.FILTER_BUTTONS && mt != MeterType.ANTENNA_BUTTONS &&
                mt != MeterType.HISTORY && mt != MeterType.TUNESTEP_BUTTONS && mt != MeterType.DISCORD_BUTTONS && mt != MeterType.FILTER_DISPLAY &&
                mt != MeterType.DIAL_DISPLAY && mt != MeterType.OTHER_BUTTONS && mt != MeterType.WAVE_RECORD && mt != MeterType.VOICE_RECORD_PLAY_BUTTONS
                )
            {
                switch (m.MeterVariables(mt))
                {
                    case 1:
                        p24_btnMMIO_variable.Enabled = true;
                        p24_btnMMIO_variable_2.Enabled = false;
                        toolTip1.SetToolTip(p24_btnMMIO_variable, m.MeterVariablesReadingString(mt, 0));
                        p24_pnlVariableInUse_1.Visible = variableInUse(0);
                        p24_pnlVariableInUse_2.Visible = false;
                        break;
                    case 2:
                        p24_btnMMIO_variable.Enabled = true;
                        p24_btnMMIO_variable_2.Enabled = true;
                        toolTip1.SetToolTip(p24_btnMMIO_variable, m.MeterVariablesReadingString(mt, 0));
                        toolTip1.SetToolTip(p24_btnMMIO_variable_2, m.MeterVariablesReadingString(mt, 1));
                        p24_pnlVariableInUse_1.Visible = variableInUse(0);
                        p24_pnlVariableInUse_2.Visible = variableInUse(1);
                        break;
                    case 7:
                        //todo? anan mm
                        p24_btnMMIO_variable.Enabled = false;
                        p24_btnMMIO_variable_2.Enabled = false;
                        p24_pnlVariableInUse_1.Visible = false;
                        p24_pnlVariableInUse_2.Visible = false;
                        break;
                    default:
                        p24_btnMMIO_variable.Enabled = false;
                        p24_btnMMIO_variable_2.Enabled = false;
                        p24_pnlVariableInUse_1.Visible = false;
                        p24_pnlVariableInUse_2.Visible = false;
                        break;
                }
            }
            else if (mt == MeterType.ROTATOR)
            {
                // unique controls for rotator as own setting grp
                switch (m.MeterVariables(mt))
                {
                    case 2:
                        p24_btnMMIO_variable_rotator.Enabled = true;
                        p24_btnMMIO_variable_2_rotator.Enabled = true;
                        toolTip1.SetToolTip(p24_btnMMIO_variable_rotator, m.MeterVariablesReadingString(mt, 0));
                        toolTip1.SetToolTip(p24_btnMMIO_variable_2_rotator, m.MeterVariablesReadingString(mt, 1));
                        p24_pnlVariableInUse_1_rotator.Visible = variableInUse(0);
                        p24_pnlVariableInUse_2_rotator.Visible = variableInUse(1);
                        break;
                }
            }
            else if (mt == MeterType.HISTORY)
            {
                // unique controls for history as own setting grp
                switch (m.MeterVariables(mt))
                {
                    case 2:
                        p24_btnMMIO_variable_history.Enabled = true;
                        p24_btnMMIO_variable_2_history.Enabled = true;
                        toolTip1.SetToolTip(p24_btnMMIO_variable_history, m.MeterVariablesReadingString(mt, 0));
                        toolTip1.SetToolTip(p24_btnMMIO_variable_2_history, m.MeterVariablesReadingString(mt, 1));
                        p24_pnlVariableInUse_1_history.Visible = variableInUse(0);
                        p24_pnlVariableInUse_2_history.Visible = variableInUse(1);
                        break;
                }
            }

            if (mt == MeterType.HISTORY)
            {
                p24_nudHistory_vertical_ratio.Value = (decimal)igs.GetSetting<float>("history_vertical_ratio", true, 0.130f, 1f, 0.5f);
                p24_clrbtnHistory_background.Color = igs.GetSetting<System.Drawing.Color>("history_background_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Black);
                p24_nudHistory_update.Value = (decimal)igs.GetSetting<float>("history_update", true, 50f, 10000f, 0.5f);
                p24_nudHistory_keep_for.Value = (decimal)igs.GetSetting<float>("history_keep_for", true, 1f, 86400f, 20f);
                Reading r = igs.GetSetting<Reading>("history_reading_0", false, Reading.NONE, Reading.NONE, Reading.SIGNAL_STRENGTH);
                foreach (clsComboHistoryItem chi in p24_comboHistory_reading_0.Items)
                {
                    if (chi.Reading == r)
                    {
                        p24_comboHistory_reading_0.SelectedItem = chi;
                        break;
                    }
                }
                r = igs.GetSetting<Reading>("history_reading_1", false, Reading.NONE, Reading.NONE, Reading.SIGNAL_STRENGTH);
                foreach (clsComboHistoryItem chi in p24_comboHistory_reading_1.Items)
                {
                    if (chi.Reading == r)
                    {
                        p24_comboHistory_reading_1.SelectedItem = chi;
                        break;
                    }
                }

                p24_chkHistory_auto_0_scale.Checked = igs.GetSetting<bool>("history_auto_scale_0", false, false, false, true);
                p24_nudHistory_axis0_min.Value = (decimal)igs.GetSetting<float>("history_min_0", true, -40000f, 40000f, -150f);
                p24_nudHistory_axis0_max.Value = (decimal)igs.GetSetting<float>("history_max_0", true, -40000f, 40000f, 0f);

                p24_chkHistory_1_show_axis.Checked = igs.GetSetting<bool>("history_show_scale_1", false, false, false, true);
                p24_chkHistory_auto_1_scale.Checked = igs.GetSetting<bool>("history_auto_scale_1", false, false, false, true);
                p24_nudHistory_axis1_min.Value = (decimal)igs.GetSetting<float>("history_min_1", true, -40000f, 40000f, -150f);
                p24_nudHistory_axis1_max.Value = (decimal)igs.GetSetting<float>("history_max_1", true, -40000f, 40000f, 0f);

                p24_clrbtnHistory_colour_0.Color = igs.GetSetting<System.Drawing.Color>("history_colour_0", false, Color.Empty, Color.Empty, System.Drawing.Color.Red);
                p24_clrbtnHistory_colour_1.Color = igs.GetSetting<System.Drawing.Color>("history_colour_1", false, Color.Empty, Color.Empty, System.Drawing.Color.Yellow);

                p24_clrbtnHistory_lines.Color = igs.GetSetting<System.Drawing.Color>("history_colour_lines", false, Color.Empty, Color.Empty, System.Drawing.Color.White);
                p24_clrbtnHistory_time.Color = igs.GetSetting<System.Drawing.Color>("history_colour_time", false, Color.Empty, Color.Empty, System.Drawing.Color.Gray);

                p24_chkHistory_fade_rx.Checked = igs.FadeOnRx;
                p24_chkHistory_fade_tx.Checked = igs.FadeOnTx;
            }
            else if (mt == MeterType.WAVE_RECORD)
            {
                p24_nudWaveRecord_vertical_ratio.Value = (decimal)igs.GetSetting<float>("waverecord_vertical_ratio", true, 0.10f, 2f, 0.60f);
                p24_nudWaveRecord_radius.Value = (decimal)igs.GetSetting<float>("waverecord_radius", true, 0f, 2f, 0.20f);
                p24_clrbtnWaveRecord_back.Color = igs.GetSetting<System.Drawing.Color>("waverecord_back_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(16, 16, 16));
                p24_clrbtnWaveRecord_border.Color = igs.GetSetting<System.Drawing.Color>("waverecord_row_border_colour", false, Color.Empty, Color.Empty, igs.GetSetting<System.Drawing.Color>("waverecord_border_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(70, 70, 70)));
                p24_clrbtnWaveRecord_row.Color = igs.GetSetting<System.Drawing.Color>("waverecord_row_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(28, 28, 28));
                p24_clrbtnWaveRecord_text.Color = igs.GetSetting<System.Drawing.Color>("waverecord_text_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.White);
                p24_clrbtnWaveRecord_button_fill.Color = igs.GetSetting<System.Drawing.Color>("waverecord_button_fill_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(40, 40, 40));
                p24_clrbtnWaveRecord_button_border.Color = igs.GetSetting<System.Drawing.Color>("waverecord_button_border_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(86, 86, 86));
                p24_clrbtnWaveRecord_button_hover.Color = igs.GetSetting<System.Drawing.Color>("waverecord_button_hover_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(90, 90, 90));
                p24_clrbtnWaveRecord_play.Color = igs.GetSetting<System.Drawing.Color>("waverecord_play_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                p24_clrbtnWaveRecord_stop.Color = igs.GetSetting<System.Drawing.Color>("waverecord_stop_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Olive);
                p24_clrbtnWaveRecord_delete.Color = igs.GetSetting<System.Drawing.Color>("waverecord_delete_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Orange);
                p24_clrbtnWaveRecord_scroll_track.Color = igs.GetSetting<System.Drawing.Color>("waverecord_scrolltrack_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(36, 36, 36));
                p24_clrbtnWaveRecord_scroll_thumb.Color = igs.GetSetting<System.Drawing.Color>("waverecord_scrollthumb_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(118, 118, 118));
                p24_clrbtnWaveRecord_scroll_hover.Color = igs.GetSetting<System.Drawing.Color>("waverecord_scrollthumb_hover_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.FromArgb(160, 160, 160));
                p24_chkWaveRecord_fade_rx.Checked = igs.FadeOnRx;
                p24_chkWaveRecord_fade_tx.Checked = igs.FadeOnTx;
            }
            else if (mt == MeterType.BAND_BUTTONS || mt == MeterType.MODE_BUTTONS || mt == MeterType.FILTER_BUTTONS ||
                     mt == MeterType.ANTENNA_BUTTONS || mt == MeterType.TUNESTEP_BUTTONS || mt == MeterType.DISCORD_BUTTONS ||
                     mt == MeterType.OTHER_BUTTONS || mt == MeterType.VOICE_RECORD_PLAY_BUTTONS)
            {
                int columns = 1;
                int max_buttons = 1;

                if (mt == MeterType.VOICE_RECORD_PLAY_BUTTONS)
                {
                    if(!_ignore_slot_count) p24_nudVoiceRecordingPlayback_slots.Value = igs.GetSetting<int>("buttonbox_recordplayback_slots", true, 0, int.MaxValue, 8);
                    if (_selected_voice_slot > -1)
                    {
                        p24_txtRecording_labelText.Text = igs.GetSetting<string>("buttonbox_recordplayback_label_" + _selected_voice_slot.ToString(), false, null, null, "Slot " + (_selected_voice_slot + 1).ToString());
                        p24_chkRecording_slot_locked.Checked = igs.GetSetting<bool>("buttonbox_recordplayback_locked_" + _selected_voice_slot.ToString(), false, false, false, false);
                        p24_btnRecording_load_wav_to_slot.Enabled = !p24_chkRecording_slot_locked.Checked;
                        p24_chkRecording_playkeybind.Checked = igs.GetSetting<bool>("buttonbox_recordplayback_useskeybind_" + _selected_voice_slot.ToString(), false, false, false, false);
                        p24_txtRecording_playkeybind.Tag = igs.GetSetting<Keys>("buttonbox_recordplayback_keybind_" + _selected_voice_slot.ToString(), false, Keys.None, Keys.None, Keys.None);
                        Keys data = (Keys)p24_txtRecording_playkeybind.Tag;
                        Keys keycode = data & Keys.KeyCode;
                        bool alt = (data & Keys.Alt) != 0;
                        bool ctrl = (data & Keys.Control) != 0;
                        bool shift = (data & Keys.Shift) != 0;
                        if(keycode == Keys.None)
                        {
                            p24_txtRecording_playkeybind.Text = "unset";
                        }
                        else
                        {
                            string prefix = "";
                            if (alt) prefix += "ALT+";
                            if (ctrl) prefix += "CTRL+";
                            if (shift) prefix += "SHIFT+";
                            p24_txtRecording_playkeybind.Text = prefix + keycode.ToString();
                        }

                        p24_chkRecording_canRepeat.Checked = igs.GetSetting<bool>("buttonbox_recordplayback_canrepeat_" + _selected_voice_slot.ToString(), false, false, false, false);
                        int delay = igs.GetSetting<int>("buttonbox_recordplayback_repeatdelay_" + _selected_voice_slot.ToString(), false, (int)p24_nudRecording_repeatDelay.Minimum, (int)p24_nudRecording_repeatDelay.Maximum, 10);
                        p24_nudRecording_repeatDelay.Value = (decimal)delay;

                        p24_nudRecording_tx_gain_adjust.Value = (decimal)igs.GetSetting<double>("buttonbox_recordplayback_txgainadjust_" + _selected_voice_slot.ToString(), false, -70, 70, 0);
                        p24_chkRecording_ignore_play_tempchanges.Checked = igs.GetSetting<bool>("buttonbox_recordplayback_ignoreplaytempchanges_" + _selected_voice_slot.ToString(), false, false, false, true);
                        p24_chkRecording_ignore_record_tempchanges.Checked = igs.GetSetting<bool>("buttonbox_recordplayback_ignorerecordtempchanges_" + _selected_voice_slot.ToString(), false, false, false, true);
                    }
                }
                else if (mt == MeterType.OTHER_BUTTONS)
                {
                    for (int n = 0; n < OtherButtonIdHelpers.MAX_BITFIELD_GROUP; n++)
                    {
                        p24_ucOtherButtonsOptionsGrid_buttons.SetBitfield(n, igs.GetSetting<int>("buttonbox_other_buttons_bitfield_" + n.ToString(), true, 0, int.MaxValue, 0));
                    }
                    for (int n = 0; n < OtherButtonIdHelpers.MACRO_BUTTONS_PERGROUP; n++)
                    {
                        p24_ucOtherButtonsOptionsGrid_buttons.SetMacroSettings(n, igs.GetSetting<OtherButtonMacroSettings>("buttonbox_other_buttons_macro_settings_" + n.ToString(), false, null, null, null));
                    }
                }
                else if (mt == MeterType.TUNESTEP_BUTTONS)
                {
                    p24_ucTunestepOptionsGrid_buttons.Bitfield = igs.GetSetting<int>("buttonbox_tunestep_bitfield", true, 0, int.MaxValue, 0);
                }
                else if (mt == MeterType.ANTENNA_BUTTONS)
                {
                    p24_chkButtonBox_antenna_rx1.Checked = igs.GetSetting<bool>("buttonbox_rx1", false, false, false, true);
                    p24_chkButtonBox_antenna_rx2.Checked = igs.GetSetting<bool>("buttonbox_rx2", false, false, false, true);
                    p24_chkButtonBox_antenna_rx3.Checked = igs.GetSetting<bool>("buttonbox_rx3", false, false, false, true);
                    p24_chkButtonBox_antenna_tx1.Checked = igs.GetSetting<bool>("buttonbox_tx1", false, false, false, true);
                    p24_chkButtonBox_antenna_tx2.Checked = igs.GetSetting<bool>("buttonbox_tx2", false, false, false, true);
                    p24_chkButtonBox_antenna_tx3.Checked = igs.GetSetting<bool>("buttonbox_tx3", false, false, false, true);
                    p24_chkButtonBox_antenna_byp.Checked = igs.GetSetting<bool>("buttonbox_byp", false, false, false, true);
                    p24_chkButtonBox_antenna_ext1.Checked = igs.GetSetting<bool>("buttonbox_ext1", false, false, false, true);
                    p24_chkButtonBox_antenna_xvtr.Checked = igs.GetSetting<bool>("buttonbox_xvtr", false, false, false, true);
                    p24_chkButtonBox_antenna_rxtxant.Checked = igs.GetSetting<bool>("buttonbox_rxtxant", false, false, false, true);
                }

                switch (mt)
                {
                    case MeterType.BAND_BUTTONS:
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, 15, 15);
                        if (p24_nudBandButtons_columns.Value > 15) p24_nudBandButtons_columns.Value = 15;
                        if (p24_nudBandButtons_columns.Maximum != 15) p24_nudBandButtons_columns.Maximum = 15;
                        break;
                    case MeterType.MODE_BUTTONS:
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, 12, 12);
                        if (p24_nudBandButtons_columns.Value > 12) p24_nudBandButtons_columns.Value = 12;
                        if (p24_nudBandButtons_columns.Maximum != 12) p24_nudBandButtons_columns.Maximum = 12;
                        break;
                    case MeterType.FILTER_BUTTONS:
                        max_buttons = m.RX == 1 ? 12 : 9; // rx2 only has 9 filter buttons
                        max_buttons = Math.Max(1, max_buttons);
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                        if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                        break;
                    case MeterType.ANTENNA_BUTTONS:
                        max_buttons = getTotalColumnsNeededForAntennaButtons();
                        max_buttons = Math.Max(1, max_buttons);
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                        if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                        break;
                    case MeterType.TUNESTEP_BUTTONS:
                        max_buttons = p24_ucTunestepOptionsGrid_buttons.GetCheckedCount();
                        max_buttons = Math.Max(1, max_buttons);
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                        if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                        break;
                    case MeterType.DISCORD_BUTTONS:
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, 12, 12);
                        if (p24_nudBandButtons_columns.Value > 12) p24_nudBandButtons_columns.Value = 12;
                        if (p24_nudBandButtons_columns.Maximum != 12) p24_nudBandButtons_columns.Maximum = 12;
                        break;
                    case MeterType.OTHER_BUTTONS:
                        max_buttons = 0;
                        for (int n = 0; n < OtherButtonIdHelpers.MAX_BITFIELD_GROUP; n++)
                        {
                            max_buttons += p24_ucOtherButtonsOptionsGrid_buttons.GetCheckedCount(n);
                        }
                        max_buttons = Math.Max(1, max_buttons);
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                        if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                        break;
                    case MeterType.VOICE_RECORD_PLAY_BUTTONS:
                        max_buttons = (int)p24_nudVoiceRecordingPlayback_slots.Value;
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (p24_nudBandButtons_columns.Value > max_buttons) p24_nudBandButtons_columns.Value = max_buttons;
                        if (p24_nudBandButtons_columns.Maximum != max_buttons) p24_nudBandButtons_columns.Maximum = max_buttons;
                        p24_txtRecording_4char.Text = igs.GetSetting<string>("buttonbox_recordplayback_4char", false, "", "", "");
                        break;
                }
                p24_nudBandButtons_columns.Value = columns;
                p24_nudBandButtons_border.Value = (decimal)igs.GetSetting<float>("buttonbox_border", true, 0f, 1f, 0.05f);
                p24_nudBandButtons_margin.Value = (decimal)igs.GetSetting<float>("buttonbox_margin", true, 0f, 1f, 0f);
                p24_nudBandButtons_radius.Value = (decimal)igs.GetSetting<float>("buttonbox_radius", true, 0f, 2f, 0f);
                p24_nudBandButtons_height_ratio.Value = (decimal)igs.GetSetting<float>("buttonbox_height_ratio", true, 0.01f, 2f, 0.5f);

                p24_chkBandButtons_use_indicator.Checked = igs.GetSetting<bool>("buttonbox_use_indicator", false, false, false, false);
                p24_nudBandButtons_indicator_border.Value = (decimal)igs.GetSetting<float>("buttonbox_indicator_border", true, 0f, 1f, 0.05f);
                p24_clrbtnBandButtons_indicator_on.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_on_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.CornflowerBlue);
                p24_clrbtnBandButtons_indicator_off.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_off_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LightGray);

                p24_clrbtnBandButtons_fill.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_fill_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Black);
                p24_clrbtnBandButtons_hover.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_hover_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LightGray);
                p24_clrbtnBandButtons_border.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_border_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.White);

                p24_clrbtnButonBox_click.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_click_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Orange);
                p24_clrbtnButonBox_fontcolour.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_font_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.White);

                p24_chkBandButtons_band_inactive_use.Checked = igs.GetSetting<bool>("buttonbox_use_off_colour", false, false, false, false);

                p24_nudBandButtons_indicator_style.Value = (decimal)((int)igs.GetSetting<MeterManager.clsButtonBox.IndicatorType>("buttonbox_indicator_type", true, MeterManager.clsButtonBox.IndicatorType.RING, MeterManager.clsButtonBox.IndicatorType.LAST, MeterManager.clsButtonBox.IndicatorType.RING));

                p24_nudButtonBox_font_scale.Value = (decimal)igs.GetSetting<float>("buttonbox_font_scale", true, 0.01f, 2f, 1f);
                p24_nudButtonBox_font_x_shift.Value = (decimal)igs.GetSetting<float>("buttonbox_font_shift_x", true, -0.25f, 0.25f, 0f);
                p24_nudButtonBox_font_y_shift.Value = (decimal)igs.GetSetting<float>("buttonbox_font_shift_y", true, -0.25f, 0.25f, 0f);

                p24_chkButtonBox_fix_text_size.Checked = igs.GetSetting<bool>("buttonbox_fix_text_size", false, false, false, false);
                p24_chkButtonBox_use_icons.Checked = igs.GetSetting<bool>("buttonbox_use_icons", false, false, false, true);

                _bandButtons_font = new Font(igs.FontFamily1, igs.FontSize1, igs.FontStyle1);
                p24_chkBandButtons_fade_rx.Checked = igs.FadeOnRx;
                p24_chkBandButtons_fade_tx.Checked = igs.FadeOnTx;

                updateButtonIndicatorControls();
            }
            else if (mt == MeterType.DIAL_DISPLAY)
            {
                p24_chkDialDisplay_fade_rx.Checked = igs.FadeOnRx;
                p24_chkDialDisplay_fade_tx.Checked = igs.FadeOnTx;

                p24_nudDialDisplay_vertical_ratio.Value = (decimal)igs.GetSetting<float>("dialdisplay_vertical_ratio", true, 0.01f, 1f, 1f);
                p24_nudDialDisplay_font_scale.Value = (decimal)igs.GetSetting<float>("dialdisplay_font_scale", true, 0.01f, 1.1f, 1f);
                p24_chkDialDisplay_alwaysshow_vfos.Checked = igs.GetSetting<bool>("dialdisplay_alwaysshow_vfos", false, false, false, false);
                p24_chkDial_align.Checked = igs.GetSetting<bool>("dialdisplay_align_with_tunestep", false, false, false, true);

                p24_nudDial_increment.Value = (decimal)igs.GetSetting<int>("dialdisplay_increment", true, 90, 720, 540);
                p24_nudDial_decrement.Value = (decimal)igs.GetSetting<int>("dialdisplay_decrement", true, 90, 720, 360);
                p24_nudDial_interval.Value = (decimal)igs.GetSetting<int>("dialdisplay_interval", true, 1, 10, 2);
                p24_nudDial_max_increments.Value = (decimal)igs.GetSetting<int>("dialdisplay_max_increments", true, 1, 30, 4);
                p24_nudDial_degrees_for_change.Value = (decimal)igs.GetSetting<int>("dialdisplay_degrees_for_change", true, 1, 90, 5);

                p24_clrbtnDial_text.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_text", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.White);
                p24_clrbtnDial_circle.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_cirlce", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Black);
                p24_clrbtnDial_pad.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_pad", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Blue);
                p24_clrbtnDial_pad_pressed.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_pad_pressed", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Orange);
                p24_clrbtnDial_button_on.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_button_on", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.CornflowerBlue);
                p24_clrbtnDial_button_off.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_button_off", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Black);
                p24_clrbtnDial_button_highlight.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_button_highlight", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Gray);
                p24_clrbtnDial_ring.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_ring", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Gray);
                p24_clrbtnDial_slow.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_slow", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Blue);
                p24_clrbtnDial_hold.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_hold", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Green);
                p24_clrbtnDial_fast.Color = igs.GetSetting<System.Drawing.Color>("dialdisplay_fast", false, System.Drawing.Color.Empty, System.Drawing.Color.Empty, System.Drawing.Color.Red);
            }
            else if (mt == MeterType.WEB_IMAGE)
            {
                p24_nudWebImage_update_interval.Value = igs.UpdateInterval;
                p24_nudWebImage_width_scale.Value = (decimal)igs.EyeScale;
                p24_chkWebImage_fade_rx.Checked = igs.FadeOnRx;
                p24_chkWebImage_fade_tx.Checked = igs.FadeOnTx;
                p24_txtWebImage_url.Text = igs.Text1;
                p24_chkWebImage_bypass_cache.Checked = igs.DarkMode;
                updateWebImageState((ImageFetcher.State)igs.HistoryDuration);

                p24_txtWebImage_4char.Text = igs.GetSetting<string>("webimage_4char", false, "", "", "");
                p24_chkWebImage_background.Checked = igs.GetSetting<bool>("webimage_background", false, false, false, false);
                p24_nudWebImage_background_time.Value = (decimal)igs.GetSetting<int>("webimage_background_interval", true, 5, 3600, 5);
                p24_txtWebImage_background_4char.Text = igs.GetSetting<string>("webimage_background_4char", false, "", "", "");
                updateWebImageBackground();
            }
            else if (mt == MeterType.ROTATOR)
            {
                p24_nudMeterItemUpdateRateRotator.Value = igs.UpdateInterval;
                p24_clrbtnMeterItemHBackgroundRotator.Color = igs.Colour;
                p24_clrbtnMeterItemRotatorArrow.Color = igs.TitleColor;
                p24_clrbtnMeterItemRotatorLargeDot.Color = igs.MarkerColour;
                p24_clrbtnMeterItemRotatorSmallDot.Color = igs.SubMarkerColour;
                p24_chkMeterItemRotatorShowBeamWidth.Checked = igs.ShowMarker;
                p24_clrbtnMeterItemRotatorBeamWidth.Color = igs.LowColor;
                p24_clrbtnMeterItemRotatorText.Color = igs.HighColor;
                p24_chkMeterItemRotatorCardinals.Checked = igs.ShowHistory;
                p24_chkMeterItemFadeOnRxRotator.Checked = igs.FadeOnRx;
                p24_chkMeterItemFadeOnTxRotator.Checked = igs.FadeOnTx;
                p24_chkMeterItemDarkModeRotator.Checked = igs.DarkMode;
                p24_nudMeterItemRotatorBeamWidth.Value = (decimal)igs.AttackRatio;
                p24_nudMeterItemRotator_padding.Value = (decimal)igs.EyeScale;
                updateShowBeamWidthControls();

                p24_chkMeterItemRotatorAllowControl.Checked = igs.ShowType;
                p24_clrbtnMeterItemRotatorControlColour.Color = igs.HistoryColor;
                p24_txtMeterItemRotatorAZcommand.Text = igs.Text1;
                p24_txtMeterItemRotatorELEcommand.Text = igs.Text2;
                p24_txtMeterItemRotatorSTOPcommand.Text = igs.FontFamily1;
                updateRotatorControlControls();

                //
                switch ((MeterManager.clsRotatorItem.RotatorMode)igs.HistoryDuration)
                {
                    case MeterManager.clsRotatorItem.RotatorMode.AZ:
                        p24_radMeterItemRotator_show_az.Checked = true;
                        break;
                    case MeterManager.clsRotatorItem.RotatorMode.ELE:
                        p24_radMeterItemRotator_show_ele.Checked = true;
                        break;
                    case MeterManager.clsRotatorItem.RotatorMode.BOTH:
                        p24_radMeterItemRotator_show_both.Checked = true;
                        break;
                    default:
                        break;
                }
                //

                Guid guid = igs.GetMMIOGuid(2);
                if (MultiMeterIO.Data.ContainsKey(guid))
                {
                    MultiMeterIO.clsMMIO mmio = MultiMeterIO.Data[guid];
                    p24_txtRotator_4charID.Text = mmio.FourChar;
                }
                else
                {
                    p24_txtRotator_4charID.Text = "";
                }

                p24_nudMeterItemRotatorBeamWidth_alpha.Value = (decimal)igs.GetSetting<float>("rotator_beamwidth_alpha", true, 0, 1f, 0.6f);
            }
            else if (mt == MeterType.DATA_OUT)
            {
                Guid guid = igs.GetMMIOGuid(0);
                if (MultiMeterIO.Data.ContainsKey(guid))
                {
                    MultiMeterIO.clsMMIO mmio = MultiMeterIO.Data[guid];
                    p24_txtDataOutNode_4charID.Text = mmio.FourChar;
                    p24_nudDataOutNode_sendinterval.Value = (decimal)igs.UpdateInterval;
                }
                else
                {
                    p24_txtDataOutNode_4charID.Text = "";
                    p24_nudDataOutNode_sendinterval.Value = 500;
                }
            }
            else if (mt == MeterType.SIGNAL_TEXT)
            {
                p24_nudMeterItemUpdateRate.Value = igs.UpdateInterval < p24_nudMeterItemUpdateRate.Minimum ? p24_nudMeterItemUpdateRate.Minimum : igs.UpdateInterval;
                p24_nudMeterItemAttackRate.Value = (decimal)igs.AttackRatio;
                p24_nudMeterItemDecayRate.Value = (decimal)igs.DecayRatio;

                p24_chkMeterItemFadeOnRx.Checked = igs.FadeOnRx;
                p24_chkMeterItemFadeOnTx.Checked = igs.FadeOnTx;
                p24_clrbtnMeterItemHBackground.Color = igs.Colour;
                p24_clrbtnMeterItemIndicator.Color = igs.MarkerColour;
                p24_clrbtnMeterItemSubIndicator.Color = igs.SubMarkerColour;
                p24_clrbtnMeterItemPeakValueColour.Color = igs.PeakValueColour;
                p24_chkMeterItemPeakValue.Checked = igs.PeakValue;
                p24_chkMeterItemTitle.Checked = igs.ShowType;
                p24_clrbtnMeterItemMeterTitle.Color = igs.TitleColor;
                if (igs.MaxBin)
                {
                    p24_ucMeterItemSignalType.SignalType = Reading.SIGNAL_MAX_BIN;
                }
                else
                {
                    if (igs.Average)
                    {
                        p24_ucMeterItemSignalType.SignalType = Reading.AVG_SIGNAL_STRENGTH;
                    }
                    else
                    {
                        p24_ucMeterItemSignalType.SignalType = Reading.SIGNAL_STRENGTH;
                    }
                }
                p24_chkMeterItemShowSubIndicator.Checked = igs.ShowSubMarker;

                p24_nudMeterItemHistoryDuration.Value = igs.HistoryDuration < p24_nudMeterItemHistoryDuration.Minimum ? p24_nudMeterItemHistoryDuration.Minimum : igs.HistoryDuration;
                p24_nudMeterItemIgnoreHistoryDuration.Value = igs.IgnoreHistoryDuration;

                p24_lblMMLow.Enabled = false;
                p24_lblMMHigh.Enabled = false;
                p24_clrbtnMeterItemLow.Enabled = false;
                p24_clrbtnMeterItemHigh.Enabled = false;

                p24_lblMMIndicator.Enabled = true;
                p24_clrbtnMeterItemIndicator.Enabled = true;
                p24_chkMeterItemShowIndicator.Enabled = false;

                p24_lblMMIndicatorSub.Enabled = true;
                p24_clrbtnMeterItemSubIndicator.Enabled = true;
                p24_chkMeterItemShowSubIndicator.Enabled = true;
                p24_chkMeterItemTitle.Enabled = true;
                p24_clrbtnMeterItemMeterTitle.Enabled = true;

                p24_lblMMBackground.Enabled = true;
                p24_clrbtnMeterItemHBackground.Enabled = true;
                p24_chkMeterItemFadeOnRx.Enabled = true;
                p24_chkMeterItemFadeOnTx.Enabled = true;
                p24_chkMeterItemSegmented.Enabled = false;
                p24_chkMeterItemSolid.Enabled = false;
                p24_lblMMsegSolLow.Enabled = false;
                p24_lblMMsegSolHigh.Enabled = false;
                p24_clrbtnMeterItemSegmentedSolidColourLow.Enabled = false;
                p24_clrbtnMeterItemSegmentedSolidColourHigh.Enabled = false;
                p24_chkMeterItemPeakValue.Enabled = true;
                updatePeakValueControls();
                p24_lblMMEyeSize.Enabled = false;
                p24_lblMMEyeBezelSize.Enabled = false;
                p24_nudMeterItemEyeScale.Enabled = false;
                p24_nudMeterItemEyeBezelScale.Enabled = false;
                p24_chkMeterItemShadow.Enabled = false;
                p24_lblMMHistory.Enabled = true;
                p24_lblMMHistoryIgnore.Enabled = true;
                p24_nudMeterItemHistoryDuration.Enabled = true;
                p24_nudMeterItemIgnoreHistoryDuration.Enabled = true;
                p24_chkMeterItemHistory.Enabled = false;
                p24_clrbtnMeterItemHistory.Enabled = false;
                p24_chkMeterItemPeakHold.Enabled = false;
                p24_clrbtnMeterItemPeakHold.Enabled = false;
                p24_ucMeterItemSignalType.Enabled = true;
                p24_chkMeterItemDarkMode.Enabled = false;
                p24_lblMMPowerLimit.Enabled = false;
                p24_nudMeterItemsPowerLimit.Enabled = false;
                p24_clrbtnMeterItemPowerScale.Enabled = false;
            }
            else if (mt == MeterType.VFO_DISPLAY)
            {
                p24_clrbtnMMVfoDisplayBackground.Color = igs.Colour;
                p24_clrbtnMMVfoDisplayTitle.Color = igs.TitleColor;

                //using exisinng igs settings
                p24_clrbtnMMVfoDisplayFrequency.Color = igs.MarkerColour;
                p24_clrbtnMMVfoDisplayMode.Color = igs.SubMarkerColour;
                p24_clrbtnMMVfoDisplaySplitBack.Color = igs.LowColor;
                p24_clrbtnMMVfoDisplaySplit.Color = igs.HighColor;
                p24_clrbtnMMVfoDisplayRx.Color = igs.PeakValueColour;
                p24_clrbtnMMVfoDisplayTx.Color = igs.PeakHoldMarkerColor;
                p24_clrbtnMMVfoDisplayFilter.Color = igs.HistoryColor;
                p24_clrbtnMMVfoDisplayBand.Color = igs.SegmentedSolidLowColour;
                p24_clrbtnMMVfoDigitHighlight.Color = igs.PowerScaleColour;

                p24_chkMultiMeter_vfo_show_bandtext.Checked = igs.GetSetting<bool>("vfo_showbandtext", false, false, false, false);
                p24_clrbtnMultiMeter_vfo_show_bandtext.Color = igs.GetSetting<System.Drawing.Color>("vfo_showbandtext_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                p24_clrbtnMMVfoDisplayFrequency_small.Color = igs.GetSetting<System.Drawing.Color>("vfo_frequency_small_numbers_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Orange);

                p24_clrbtnMultiMeter_vfo_lock.Color = igs.GetSetting<System.Drawing.Color>("vfo_lock_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                p24_clrbtnMultiMeter_vfo_sync.Color = igs.GetSetting<System.Drawing.Color>("vfo_sync_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);


                switch ((MeterManager.clsVfoDisplay.VFODisplayMode)igs.HistoryDuration)
                {
                    case MeterManager.clsVfoDisplay.VFODisplayMode.VFO_BOTH:
                        p24_radMultiMeter_vfo_display_both.Checked = true;
                        break;
                    case MeterManager.clsVfoDisplay.VFODisplayMode.VFO_A:
                        p24_radMultiMeter_vfo_display_vfoa.Checked = true;
                        break;
                    case MeterManager.clsVfoDisplay.VFODisplayMode.VFO_B:
                        p24_radMultiMeter_vfo_display_vfob.Checked = true;
                        break;
                }

                updateVfoShowBandtextColour();
            }
            else if (mt == MeterType.CLOCK)
            {
                p24_clrbtnMMClockBackground.Color = igs.Colour;
                p24_chkMMClockTitle.Checked = igs.ShowType;
                p24_clrbtnMMClockTitle.Color = igs.TitleColor;
                p24_clrbtnMMTime.Color = igs.MarkerColour;
                p24_clrbtnMMDate.Color = igs.SubMarkerColour;
                p24_radMM24Clock.Checked = igs.ShowMarker; // use the show marker bool for this
                if (!p24_radMM24Clock.Checked && !p24_radMM12Clock.Checked) p24_radMM12Clock.Checked = true;
                updateTitleControlsClock();
            }
            else if (mt == MeterType.LED)
            {
                p24_chkLedIndicator_FadeOnRX.Checked = igs.FadeOnRx;
                p24_chkLedIndicator_FadeOnTX.Checked = igs.FadeOnTx;
                p24_clrbtnLedIndicator_true.Color = igs.Colour;
                p24_clrbtnLedIndicator_false.Color = igs.MarkerColour;

                p24_clrbtnLedIndicator_PanelBackground.Color = igs.TitleColor;
                p24_clrbtnLedIndicator_PanelBackgroundTX.Color = igs.HistoryColor;
                p24_chkLedIndicator_ShowPanel.Checked = igs.ShowSubMarker;

                p24_nudLedIndicator_xOffset.Value = (decimal)igs.EyeScale;
                p24_nudLedIndicator_yOffset.Value = (decimal)igs.EyeBezelScale;
                p24_nudLedIndicator_xSize.Value = (decimal)igs.AttackRatio;
                p24_nudLedIndicator_ySize.Value = (decimal)igs.DecayRatio;
                p24_nudLedIndicator_UpdateInterval.Value = (decimal)igs.UpdateInterval;

                p24_txtLedIndicator_condition.Text = igs.Text1;

                p24_nudLedIndicator_PanelPadding.Value = (decimal)igs.SpacerPadding;

                p24_chkLed_show_true.Checked = igs.PeakHold;
                p24_chkLed_show_false.Checked = igs.ShowMarker;
                switch (igs.IgnoreHistoryDuration)
                {
                    case 0:
                        p24_radLed_light_on_off.Checked = true;
                        break;
                    case 1:
                        p24_radLed_light_blink.Checked = true;
                        break;
                    case 2:
                        p24_radLed_light_pulsate.Checked = true;
                        break;
                }

                p24_chkLed_notx_true.Checked = igs.GetSetting<bool>("led_notx_true", false, false, false, false);
                p24_chkLed_notx_false.Checked = igs.GetSetting<bool>("led_notx_false", false, false, false, false);
                p24_txtLedIndicator_4char.Text = igs.GetSetting<string>("led_4char", false, "", "", "");
                p24_chkLed_process_when_hidden.Checked = igs.GetSetting<bool>("led_process_when_hidden", false, false, false, false);

                updateLedIndicatorPanelControls();
                updateLedValidControls();
            }
            else if (mt == MeterType.TEXT_OVERLAY)
            {
                p24_chkTextOverlay_FadeOnRX.Checked = igs.FadeOnRx;
                p24_chkTextOverlay_FadeOnTX.Checked = igs.FadeOnTx;
                p24_clrbtnTextOverlay_TextColour1.Color = igs.Colour;
                p24_clrbtnTextOverlay_TextColour2.Color = igs.MarkerColour;
                p24_clrbtnTextOverlay_TextBackColour1.Color = igs.SubMarkerColour;
                p24_chkTextOverlay_textback1.Checked = igs.ShowMarker;
                p24_clrbtnTextOverlay_TextBackColour2.Color = igs.PeakValueColour;
                p24_chkTextOverlay_textback2.Checked = igs.ShowType;

                p24_clrbtnTextOverlay_PanelBackground.Color = igs.TitleColor;
                p24_clrbtnTextOverlay_PanelBackgroundTX.Color = igs.HistoryColor;
                p24_chkTextOverlay_ShowPanel.Checked = igs.ShowSubMarker;

                p24_nudTextOverlay_RXxOffset.Value = (decimal)igs.EyeScale;
                p24_nudTextOverlay_RXyOffset.Value = (decimal)igs.EyeBezelScale;
                p24_nudTextOverlay_TXxOffset.Value = (decimal)igs.AttackRatio;
                p24_nudTextOverlay_TXyOffset.Value = (decimal)igs.DecayRatio;

                p24_txtTextOverlay_RXText.Text = igs.Text1;
                p24_txtTextOverlay_TXText.Text = igs.Text2;

                _textOverlayFont1 = new Font(igs.FontFamily1, igs.FontSize1, igs.FontStyle1);
                _textOverlayFont2 = new Font(igs.FontFamily2, igs.FontSize2, igs.FontStyle2);

                p24_nudTextOverlay_PanelPadding.Value = (decimal)igs.SpacerPadding;

                p24_chkTextOverlay_rx_on_led.Checked = igs.GetSetting<bool>("textoverlay_rx_ledlogic", false, false, false, false);
                p24_chkTextOverlay_tx_on_led.Checked = igs.GetSetting<bool>("textoverlay_tx_ledlogic", false, false, false, false);

                p24_txtTextOverlay_rx_on_led_4char.Text = igs.GetSetting<string>("textoverlay_rx_4char", false, "", "", "");
                p24_txtTextOverlay_tx_on_led_4char.Text = igs.GetSetting<string>("textoverlay_tx_4char", false, "", "", "");

                updateTextOverlayPanelControls();
                updateTextOverlayBackTextControls();
                updateTextOverlayLedIndicator();
            }
            else if (mt == MeterType.SPACER)
            {
                p24_clrbtnMeterItemHBackgroundSpacerRX.Color = igs.Colour;
                p24_clrbtnMeterItemHBackgroundSpacerTX.Color = igs.MarkerColour;
                p24_chkMeterItemFadeOnRxSpacer.Checked = igs.FadeOnRx;
                p24_chkMeterItemFadeOnTxSpacer.Checked = igs.FadeOnTx;
                p24_nudMeterItemSpacerPadding.Value = (decimal)igs.SpacerPadding;
            }
            else if (mt == MeterType.FILTER_DISPLAY)
            {
                p24_clrbtnFilterDisplay_backcolour.Color = igs.Colour;
                p24_chkFilterDisplay_fadeonrx.Checked = igs.FadeOnRx;
                p24_chkFilterDisplay_fadeontx.Checked = igs.FadeOnTx;

                p24_nudFilterDisplay_vertical_ratio.Value = (decimal)igs.GetSetting<float>("filterdisplay_vertical_ratio", true, 0.15f, 1f, 0.2f);

                p24_chkFilterDisplay_show_limits.Checked = igs.GetSetting<bool>("filterdisplay_show_filter_limits", false, false, false, true);
                p24_chkFilterDisplay_fixed_zoom.Checked = igs.GetSetting<bool>("filterdisplay_show_fixed_rx_zoom", false, false, false, false);
                p24_chkFilterDisplay_fixed_tx_zoom.Checked = igs.GetSetting<bool>("filterdisplay_show_fixed_tx_zoom", false, false, false, false);
                p24_nudFilterDisplay_fixed_zoom_level.Value = (decimal)igs.GetSetting<float>("filterdisplay_rx_zoom", true, 1f, 10f, 1f);
                p24_nudFilterDisplay_fixed_tx_zoom_level.Value = (decimal)igs.GetSetting<float>("filterdisplay_tx_zoom", true, 1f, 20f, 1f);

                p24_nudFilterItem_sidebands_scale.Value = (decimal)igs.GetSetting<float>("filterdisplay_sidebands_scale", true, 0f, 10f, 0f);
                p24_nudFilterItem_cw_scale.Value = (decimal)igs.GetSetting<float>("filterdisplay_cw_scale", true, 0f, 10f, 0f);
                p24_nudFilterItem_others_scale.Value = (decimal)igs.GetSetting<float>("filterdisplay_others_scale", true, 0f, 10f, 0f);

                switch (igs.GetSetting<MeterManager.clsFilterItem.FIDisplayMode>("filterdisplay_others_displaymode", false, MeterManager.clsFilterItem.FIDisplayMode.PANADAPTOR, MeterManager.clsFilterItem.FIDisplayMode.NONE, MeterManager.clsFilterItem.FIDisplayMode.PANAFALL))
                {
                    case MeterManager.clsFilterItem.FIDisplayMode.PANADAPTOR:
                        p24_radFilterItem_panadaptor.Checked = true;
                        break;
                    case MeterManager.clsFilterItem.FIDisplayMode.WATERFALL:
                        p24_radFilterItem_waterfall.Checked = true;
                        break;
                    case MeterManager.clsFilterItem.FIDisplayMode.PANAFALL:
                        p24_radFilterItem_panafall.Checked = true;
                        break;
                    case MeterManager.clsFilterItem.FIDisplayMode.NONE:
                        p24_radFilterItem_none.Checked = true;
                        break;
                }

                p24_nudFilterItem_font_scale.Value = (decimal)igs.GetSetting<float>("filterdisplay_font_scale", true, 0.01f, 4f, 1f);

                p24_chkFilter_fill_spec.Checked = igs.GetSetting<bool>("filterdisplay_fill_spec", false, false, false, true);
                p24_clrbtnFilter_data_line.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_dataline_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                p24_clrbtnFilter_data_fill.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_datafill_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                p24_comboFilter_wf_palette.SelectedIndex = (int)igs.GetSetting<MeterManager.clsFilterItem.FIWaterfallPalette>("filterdisplay_wf_palette", false, MeterManager.clsFilterItem.FIWaterfallPalette.NONE, MeterManager.clsFilterItem.FIWaterfallPalette.NONE, MeterManager.clsFilterItem.FIWaterfallPalette.ENHANCED);
                p24_clrbtnFilter_wf_low.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_wflow_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Black);
                p24_clrbtnFilter_text.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_text_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.White);
                p24_clrbtnFilter_number_highlight.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_numberhighlight_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.DarkRed);
                p24_clrbtnFilter_edges.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_edges_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Yellow);
                p24_clrbtnFilter_edges_tx.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_edges_colour_tx", false, Color.Empty, Color.Empty, System.Drawing.Color.Red);
                p24_clrbtnFilter_edge_highlight.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_edgehighlight_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.White);
                p24_clrbtnFilter_meter_back.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_meterback_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Black);
                p24_clrbtnFilter_notch.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_notch_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.OrangeRed);
                p24_clrbtnFilter_notch_highlight.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_notchhighlight_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                p24_clrbtnFilter_extents.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_extents_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Gray);
                p24_chkFilter_sideband_mode.Checked = igs.GetSetting<bool>("filterdisplay_sideband_mode", false, false, false, false);
                p24_nudFilter_waterfall_frame_update.Value = igs.GetSetting<int>("filterdisplay_waterfall_frameupdate", true, 1, 1000, 4);
                p24_chkFilter_grey_outsidepb.Checked = igs.GetSetting<bool>("filterdisplay_use_grey", false, false, false, true);
                p24_clrbtnFilter_snap_line.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_snap_line_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Gray);
                p24_clrbtnFilter_setting_on.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_settingon_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.CornflowerBlue);
                p24_clrbtnFilter_button_highlight.Color = igs.GetSetting<System.Drawing.Color>("filterdisplay_button_highlight_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Gray);
                p24_chkFilter_characteristic.Checked = igs.GetSetting<bool>("filterdisplay_characteristic", false, false, false, false);
                p24_nudFilter_lower_characteristic.Value = (decimal)igs.GetSetting<float>("filterdisplay_characteristic_low", true, -300f, -50f, -250f);
            }
            else
            {
                //custom meter bar
                p24_radMeterItemSettings.Visible = mt == MeterType.CUSTOM_METER_BAR;
                p24_radMeterItemSettings_custom.Visible = mt == MeterType.CUSTOM_METER_BAR;
                p24_radMeterItemSettings.Checked = true;
                radMeterItemSettings_CheckedChanged(this, EventArgs.Empty);
                if (mt == MeterType.CUSTOM_METER_BAR)
                {
                    p24_nudMeterItem_custom_min.Value = (decimal)igs.GetSetting<float>("meter_custom_min", true, -5000f, 5000f, 0);
                    p24_nudMeterItem_custom_max.Value = (decimal)igs.GetSetting<float>("meter_custom_max", true, -5000f, 5000f, 10);
                    p24_nudMeterItem_custom_high.Value = (decimal)igs.GetSetting<float>("meter_custom_high", true, -5000f, 5000f, 7.5f);
                    p24_txtMeterItem_custom_units.Text = igs.GetSetting<string>("meter_custom_units", false, "", "", "?");
                    p24_txtMeterItem_custom_title.Text = igs.GetSetting<string>("meter_custom_title", false, "", "", "Title");
                }
                //

                p24_clrbtnMeterItemLow.Color = igs.LowColor;
                p24_clrbtnMeterItemHigh.Color = igs.HighColor;
                p24_clrbtnMeterItemIndicator.Color = igs.MarkerColour;
                p24_clrbtnMeterItemSubIndicator.Color = igs.SubMarkerColour;
                p24_chkMeterItemShowIndicator.Checked = igs.ShowMarker;
                p24_chkMeterItemShowSubIndicator.Checked = igs.ShowSubMarker;
                p24_clrbtnMeterItemHBackground.Color = igs.Colour;
                p24_nudMeterItemUpdateRate.Value = igs.UpdateInterval < p24_nudMeterItemUpdateRate.Minimum ? p24_nudMeterItemUpdateRate.Minimum : igs.UpdateInterval;
                p24_nudMeterItemAttackRate.Value = (decimal)igs.AttackRatio;
                p24_nudMeterItemDecayRate.Value = (decimal)igs.DecayRatio;

                updateHistoryControls(igs.ShowHistory, igs.HistoryColor, igs.ShowHistory);
                p24_nudMeterItemHistoryDuration.Value = igs.HistoryDuration < p24_nudMeterItemHistoryDuration.Minimum ? p24_nudMeterItemHistoryDuration.Minimum : igs.HistoryDuration;
                p24_nudMeterItemIgnoreHistoryDuration.Value = igs.IgnoreHistoryDuration;

                if (igs.BarStyle == MeterManager.clsBarItem.BarStyle.Segments)
                    p24_chkMeterItemSegmented.Checked = true; // will cause solid to turn off
                else if (igs.BarStyle == MeterManager.clsBarItem.BarStyle.SolidFilled)
                    p24_chkMeterItemSolid.Checked = true; // will cause segment to turn off
                else
                {
                    p24_chkMeterItemSegmented.Checked = false;
                    p24_chkMeterItemSolid.Checked = false;
                }

                p24_clrbtnMeterItemSegmentedSolidColourLow.Color = igs.SegmentedSolidLowColour;
                p24_clrbtnMeterItemSegmentedSolidColourHigh.Color = igs.SegmentedSolidHighColour;
                updateSegmentedSolidControls();

                updatePeakHoldControls(igs.PeakHold, igs.PeakHoldMarkerColor, igs.PeakHold);

                p24_chkMeterItemShadow.Checked = igs.Shadow;
                p24_chkMeterItemFadeOnRx.Checked = igs.FadeOnRx;
                p24_chkMeterItemFadeOnTx.Checked = igs.FadeOnTx;
                p24_chkMeterItemTitle.Checked = igs.ShowType;

                p24_clrbtnMeterItemMeterTitle.Color = igs.TitleColor;
                updateTitleControls();

                p24_chkMeterItemPeakValue.Checked = igs.PeakValue;
                p24_clrbtnMeterItemPeakValueColour.Color = igs.PeakValueColour;
                updatePeakValueControls();

                if (mt == MeterType.CROSS || mt == MeterType.ANANMM || mt == MeterType.PWR || mt == MeterType.REVERSE_PWR) p24_nudMeterItemsPowerLimit.Value = (decimal)igs.MaxPower;
                if (mt == MeterType.CROSS || mt == MeterType.ANANMM) p24_clrbtnMeterItemPowerScale.Color = igs.PowerScaleColour;

                // specific to mt
                bool bMagicEye = mt == MeterType.MAGIC_EYE;
                if (bMagicEye)
                {
                    p24_nudMeterItemEyeScale.Value = (decimal)igs.EyeScale; // prevents setting it to 0 as other items will have 0 //FIX THIS
                    p24_nudMeterItemEyeBezelScale.Value = (decimal)igs.EyeBezelScale;
                }
                p24_nudMeterItemEyeScale.Enabled = bMagicEye;
                p24_nudMeterItemEyeBezelScale.Enabled = bMagicEye;
                p24_lblMMEyeSize.Enabled = bMagicEye;
                p24_lblMMEyeBezelSize.Enabled = bMagicEye;
                p24_lblMMHistory.Enabled = !bMagicEye;
                p24_nudMeterItemHistoryDuration.Enabled = !bMagicEye;
                p24_lblMMHistoryIgnore.Enabled = !bMagicEye;
                p24_nudMeterItemIgnoreHistoryDuration.Enabled = !bMagicEye;
                p24_chkMeterItemHistory.Enabled = !bMagicEye;
                p24_clrbtnMeterItemHistory.Enabled = !bMagicEye && p24_chkMeterItemHistory.Checked;
                p24_chkMeterItemPeakHold.Enabled = !(bMagicEye || mt == MeterType.CROSS);
                p24_clrbtnMeterItemPeakHold.Enabled = !(bMagicEye || mt == MeterType.CROSS) && p24_chkMeterItemPeakHold.Checked;

                p24_chkMeterItemShadow.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;
                p24_chkMeterItemDarkMode.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;

                p24_lblMMPowerLimit.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;
                p24_clrbtnMeterItemPowerScale.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;

                p24_nudMeterItemsPowerLimit.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS || mt == MeterType.PWR || mt == MeterType.REVERSE_PWR;

                bool bEnable = mt == MeterType.ANANMM || mt == MeterType.CROSS || mt == MeterType.MAGIC_EYE;
                p24_chkMeterItemSegmented.Enabled = !bEnable;
                p24_chkMeterItemSolid.Enabled = !bEnable;

                p24_clrbtnMeterItemSegmentedSolidColourLow.Enabled = !bEnable && (p24_chkMeterItemSegmented.Checked || p24_chkMeterItemSolid.Checked);
                p24_clrbtnMeterItemSegmentedSolidColourHigh.Enabled = !bEnable && (p24_chkMeterItemSegmented.Checked || p24_chkMeterItemSolid.Checked);
                p24_lblMMsegSolLow.Enabled = !bEnable && (p24_chkMeterItemSegmented.Checked || p24_chkMeterItemSolid.Checked);
                p24_lblMMsegSolHigh.Enabled = !bEnable && (p24_chkMeterItemSegmented.Checked || p24_chkMeterItemSolid.Checked);

                p24_chkMeterItemTitle.Enabled = !bEnable;
                p24_clrbtnMeterItemMeterTitle.Enabled = !bEnable;
                p24_chkMeterItemPeakValue.Enabled = !bEnable;
                p24_clrbtnMeterItemPeakValueColour.Enabled = !bEnable;
                p24_lblMMLow.Enabled = !bEnable;
                p24_lblMMHigh.Enabled = !bEnable;
                p24_lblMMBackground.Enabled = !bEnable;
                p24_clrbtnMeterItemLow.Enabled = !bEnable;
                p24_clrbtnMeterItemHigh.Enabled = !bEnable;
                p24_clrbtnMeterItemHBackground.Enabled = !bEnable;
                p24_chkMeterItemShowIndicator.Enabled = !bEnable;
                //
                p24_lblMMIndicatorSub.Enabled = igs.SubIndicators;
                p24_clrbtnMeterItemSubIndicator.Enabled = igs.SubIndicators;
                p24_chkMeterItemShowSubIndicator.Enabled = !bEnable && igs.SubIndicators;
                //

                p24_ucMeterItemSignalType.Enabled = mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE;
                if (mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE)
                {
                    if (igs.MaxBin)
                    {
                        p24_ucMeterItemSignalType.SignalType = Reading.SIGNAL_MAX_BIN;
                    }
                    else
                    {
                        if (igs.Average)
                        {
                            p24_ucMeterItemSignalType.SignalType = Reading.AVG_SIGNAL_STRENGTH;
                        }
                        else
                        {
                            p24_ucMeterItemSignalType.SignalType = Reading.SIGNAL_STRENGTH;
                        }
                    }
                }
                if (mt == MeterType.ANANMM || mt == MeterType.CROSS) p24_chkMeterItemDarkMode.Checked = igs.DarkMode;
                //
            }

            setupMMSettingsGroupBoxes(mt);

            _ignoreMeterItemChangeEvents = false;
        }
        private void updateHistoryControls(bool showHistory, Color c, bool updateColor = false)
        {
            p24_chkMeterItemHistory.Checked = showHistory;
            p24_clrbtnMeterItemHistory.Enabled = showHistory;
            p24_tbMeterItemHistoryAlpha.Enabled = showHistory;

            if (updateColor)
            {
                p24_tbMeterItemHistoryAlpha.Value = c.A;
                p24_clrbtnMeterItemHistory.Color = Color.FromArgb(255, c);
            }
        }
        private void updateSegmentedSolidControls()
        {
            bool bEnabled = p24_chkMeterItemSegmented.Checked || p24_chkMeterItemSolid.Checked;
            p24_clrbtnMeterItemSegmentedSolidColourLow.Enabled = bEnabled;
            p24_clrbtnMeterItemSegmentedSolidColourHigh.Enabled = bEnabled;
            p24_lblMMsegSolLow.Enabled = bEnabled;
            p24_lblMMsegSolHigh.Enabled = bEnabled;
        }
        private void updateTitleControls()
        {
            p24_clrbtnMeterItemMeterTitle.Enabled = p24_chkMeterItemTitle.Checked;
        }
        private void updateTitleControlsClock()
        {
            p24_clrbtnMMClockTitle.Enabled = p24_chkMMClockTitle.Checked;
        }
        private void updatePeakValueControls()
        {
            p24_clrbtnMeterItemPeakValueColour.Enabled = p24_chkMeterItemPeakValue.Checked;
        }
        private void updatePeakHoldControls(bool showPeakHold, Color c, bool updateColor = false)
        {
            p24_chkMeterItemPeakHold.Checked = showPeakHold;
            p24_clrbtnMeterItemPeakHold.Enabled = showPeakHold;

            if (updateColor)
            {
                p24_clrbtnMeterItemPeakHold.Color = Color.FromArgb(255, c);
            }
        }

        private void clrbtnMeterItemHistory_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void tbMeterItemHistoryAlpha_Scroll(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemFadeOnRx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemFadeOnTx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemSegmented_CheckedChanged(object sender, EventArgs e)
        {
            if (p24_chkMeterItemSegmented.Checked) p24_chkMeterItemSolid.Checked = false; // can only be one

            updateSegmentedSolidControls();
            updateMeterType();
        }

        private void chkMeterItemTitle_CheckedChanged(object sender, EventArgs e)
        {
            updateTitleControls();
            updateMeterType();
        }

        private void chkMeterItemPeakValue_CheckedChanged(object sender, EventArgs e)
        {
            updatePeakValueControls();
            updateMeterType();
        }

        private void chkMeterItemPeakHold_CheckedChanged(object sender, EventArgs e)
        {
            bool bEnabled = p24_chkMeterItemPeakHold.Checked;

            updatePeakHoldControls(bEnabled, Color.Red, false);

            updateMeterType();
        }

        private void clrbtnMeterItemPeakHold_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemHistoryDuration_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemUpdateRate_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemAttackRate_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemDecayRate_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemShadow_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemLow_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemHigh_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }
        private void clrbtnMeterItemHBackground_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }
        private void clrbtnMeterItemMeterTitle_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemPeakValueColour_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemEyeScale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemDarkMode_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMaintainNFAdjustDeltaRX2_CheckedChanged(object sender, EventArgs e)
        {
            P24ConsoleDynamic.WrapObject(console).MaintainNFAdjustDeltaRX2 = p24_chkMaintainNFAdjustDeltaRX2.Checked;
        }

        private void chkMaintainNFAdjustDeltaRX1_CheckedChanged(object sender, EventArgs e)
        {
            P24ConsoleDynamic.WrapObject(console).MaintainNFAdjustDeltaRX1 = p24_chkMaintainNFAdjustDeltaRX1.Checked;
        }

        private void chkContainerBorder_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ContainerBorder(cci.ID, p24_chkContainerBorder.Checked);
            }
        }

        private void clrbtnContainerBackground_Changed(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ContainerBackgroundColour(cci.ID, p24_clrbtnContainerBackground.Color);
            }
        }

        private void nudMeterItemsPowerLimit_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemSolid_CheckedChanged(object sender, EventArgs e)
        {
            if (p24_chkMeterItemSolid.Checked) p24_chkMeterItemSegmented.Checked = false; // can only be one

            updateSegmentedSolidControls();
            updateMeterType();
        }

        private void clrbtnMeterItemSegmentedSolidColourHigh_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemSegmentedSolidColourLow_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemShowIndicator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemShowSubIndicator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }
        private void waveRecordSettingControlChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }
        private void moveButtonBoxSettings()
        {
            //move the buttonbox settings
            //used just after init to move things where the should be
            //so that finder can at least show items/gadget tab
            setupMMSettingsGroupBoxes(MeterType.VFO_DISPLAY, false);
            setupMMSettingsGroupBoxes(MeterType.CLOCK, false);
            setupMMSettingsGroupBoxes(MeterType.SPACER, false);
            setupMMSettingsGroupBoxes(MeterType.FILTER_DISPLAY, false);
            setupMMSettingsGroupBoxes(MeterType.TEXT_OVERLAY, false);
            setupMMSettingsGroupBoxes(MeterType.DATA_OUT, false);
            setupMMSettingsGroupBoxes(MeterType.ROTATOR, false);
            setupMMSettingsGroupBoxes(MeterType.LED, false);
            setupMMSettingsGroupBoxes(MeterType.DIAL_DISPLAY, false);
            setupMMSettingsGroupBoxes(MeterType.WEB_IMAGE, false);
            setupMMSettingsGroupBoxes(MeterType.WAVE_RECORD, false);
            setupMMSettingsGroupBoxes(MeterType.OTHER_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.VOICE_RECORD_PLAY_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.TUNESTEP_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.ANTENNA_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.FILTER_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.MODE_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.BAND_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.DISCORD_BUTTONS, false);
            setupMMSettingsGroupBoxes(MeterType.HISTORY, false);
        }
        private void setupMMSettingsGroupBoxes(MeterType mt, bool all = true)
        {
            // p24_grpMeterItemSettings defines the x,y used by all
            Point loc = p24_grpMeterItemSettings.Location;

            if (all)
            {
                p24_grpMeterItemSettings.Visible = false;
                p24_grpMeterItemClockSettings.Visible = false;
                p24_grpMeterItemVfoDisplaySettings.Visible = false;
                p24_grpMeterItemSpacerSettings.Visible = false;
                p24_grpTextOverlay.Visible = false;
                p24_grpMeterItemDataOutNode.Visible = false;
                p24_grpMeterItemRotator.Visible = false;
                p24_grpLedIndicator.Visible = false;
                p24_grpWebImage.Visible = false;
                p24_grpButtonBox.Visible = false;
                p24_pnlButtonBox_antenna_toggles.Visible = false;
                p24_grpHistoryItem.Visible = false;
                p24_grpMeterItemFilterDisplay.Visible = false;
                p24_grpDialDisplay.Visible = false;
                p24_grpWaveRecordItem.Visible = false;
            }

            switch (mt)
            {
                case MeterType.NONE:
                    break;
                case MeterType.VFO_DISPLAY:
                    p24_grpMeterItemVfoDisplaySettings.Parent = p24_grpMultiMeterHolder;
                    p24_grpMeterItemVfoDisplaySettings.Location = loc;
                    p24_grpMeterItemVfoDisplaySettings.Visible = true;
                    break;
                case MeterType.CLOCK:
                    p24_grpMeterItemClockSettings.Parent = p24_grpMultiMeterHolder;
                    p24_grpMeterItemClockSettings.Location = loc;
                    p24_grpMeterItemClockSettings.Visible = true;
                    break;
                case MeterType.SPACER:
                    p24_grpMeterItemSpacerSettings.Parent = p24_grpMultiMeterHolder;
                    p24_grpMeterItemSpacerSettings.Location = loc;
                    p24_grpMeterItemSpacerSettings.Visible = true;
                    break;
                case MeterType.FILTER_DISPLAY:
                    p24_grpMeterItemFilterDisplay.Parent = p24_grpMultiMeterHolder;
                    p24_grpMeterItemFilterDisplay.Location = loc;
                    p24_grpMeterItemFilterDisplay.Visible = true;
                    break;
                case MeterType.TEXT_OVERLAY:
                    p24_grpTextOverlay.Parent = p24_grpMultiMeterHolder;
                    p24_grpTextOverlay.Location = loc;
                    p24_grpTextOverlay.Visible = true;
                    break;
                case MeterType.DATA_OUT:
                    p24_grpMeterItemDataOutNode.Parent = p24_grpMultiMeterHolder;
                    p24_grpMeterItemDataOutNode.Location = loc;
                    p24_grpMeterItemDataOutNode.Visible = true;
                    break;
                case MeterType.ROTATOR:
                    p24_grpMeterItemRotator.Parent = p24_grpMultiMeterHolder;
                    p24_grpMeterItemRotator.Location = loc;
                    p24_grpMeterItemRotator.Visible = true;
                    break;
                case MeterType.LED:
                    p24_grpLedIndicator.Parent = p24_grpMultiMeterHolder;
                    p24_grpLedIndicator.Location = loc;
                    p24_grpLedIndicator.Visible = true;
                    break;
                case MeterType.DIAL_DISPLAY:
                    p24_grpDialDisplay.Parent = p24_grpMultiMeterHolder;
                    p24_grpDialDisplay.Location = loc;
                    p24_grpDialDisplay.Visible = true;
                    break;
                case MeterType.WEB_IMAGE:
                    p24_grpWebImage.Parent = p24_grpMultiMeterHolder;
                    p24_grpWebImage.Location = loc;
                    p24_grpWebImage.Visible = true;
                    p24_comboWebImage_HamQsl.SelectedIndex = 0;
                    p24_comboWebImage_BsdWorld.SelectedIndex = 0;
                    p24_comboWebImage_nasa.SelectedIndex = 0;
                    p24_comboWebImage_noaa.SelectedIndex = 0;
                    break;
                case MeterType.WAVE_RECORD:
                    p24_grpWaveRecordItem.Parent = p24_grpMultiMeterHolder;
                    p24_grpWaveRecordItem.Location = loc;
                    p24_grpWaveRecordItem.Visible = true;
                    p24_grpWaveRecordItem.BringToFront();
                    break;
                case MeterType.VOICE_RECORD_PLAY_BUTTONS:
                case MeterType.OTHER_BUTTONS:
                case MeterType.TUNESTEP_BUTTONS:
                case MeterType.ANTENNA_BUTTONS:
                case MeterType.FILTER_BUTTONS:
                case MeterType.MODE_BUTTONS:
                case MeterType.BAND_BUTTONS:
                case MeterType.DISCORD_BUTTONS:
                    {
                        p24_clrbtnButonBox_fontcolour.Visible = mt != MeterType.ANTENNA_BUTTONS;
                        p24_chkButtonBox_use_icons.Visible = mt == MeterType.OTHER_BUTTONS;
                        p24_btnOtherButtons_reset_layout.Visible = mt == MeterType.OTHER_BUTTONS || mt == MeterType.VOICE_RECORD_PLAY_BUTTONS;

                        p24_grpButtonBox.Parent = p24_grpMultiMeterHolder;
                        p24_grpButtonBox.Location = loc;
                        p24_grpButtonBox.Visible = true;

                        p24_picButtonBoxInfo.Visible = false;

                        Point pos = new Point(150, 194);

                        switch (mt)
                        {
                            case MeterType.ANTENNA_BUTTONS:
                                p24_pnlButtonBox_antenna_toggles.Parent = p24_grpButtonBox;
                                p24_pnlButtonBox_antenna_toggles.Location = pos;
                                p24_pnlButtonBox_antenna_toggles.Visible = true;
                                p24_ucTunestepOptionsGrid_buttons.Visible = false;
                                p24_ucOtherButtonsOptionsGrid_buttons.Visible = false;
                                p24_pnlVoiceRecordPlayback.Visible = false;
                                break;
                            case MeterType.TUNESTEP_BUTTONS:
                                p24_ucTunestepOptionsGrid_buttons.Parent = p24_grpButtonBox;
                                p24_ucTunestepOptionsGrid_buttons.Location = pos;
                                p24_ucTunestepOptionsGrid_buttons.Visible = true;
                                p24_pnlButtonBox_antenna_toggles.Visible = false;
                                p24_ucOtherButtonsOptionsGrid_buttons.Visible = false;
                                p24_pnlVoiceRecordPlayback.Visible = false;
                                if (console != null)
                                {
                                    p24_ucTunestepOptionsGrid_buttons.Init(console.TuneStepList);
                                }
                                break;
                            case MeterType.OTHER_BUTTONS:
                                p24_ucOtherButtonsOptionsGrid_buttons.Parent = p24_grpButtonBox;
                                p24_ucOtherButtonsOptionsGrid_buttons.Location = pos;
                                p24_ucOtherButtonsOptionsGrid_buttons.Visible = true;
                                p24_ucTunestepOptionsGrid_buttons.Visible = false;
                                p24_pnlButtonBox_antenna_toggles.Visible = false;
                                p24_pnlVoiceRecordPlayback.Visible = false;
                                toolTip1.SetToolTip(p24_picButtonBoxInfo, "- alt drag slots to move them around");
                                p24_picButtonBoxInfo.Visible = true;
                                break;
                            case MeterType.VOICE_RECORD_PLAY_BUTTONS:
                                p24_pnlVoiceRecordPlayback.Parent = p24_grpButtonBox;
                                p24_pnlVoiceRecordPlayback.Location = pos;
                                p24_pnlVoiceRecordPlayback.Visible = true;
                                p24_pnlButtonBox_antenna_toggles.Visible = false;
                                p24_ucTunestepOptionsGrid_buttons.Visible = false;
                                p24_ucOtherButtonsOptionsGrid_buttons.Visible = false;
                                toolTip1.SetToolTip(p24_picButtonBoxInfo, "- right click slot in record mode to delete recording\n"+
                                                                      "- shift click slot in playback to quick record to that slot\n" +
                                                                      "- alt drag slots to move them around");
                                p24_picButtonBoxInfo.Visible = true;
                                break;
                            default:
                                p24_pnlButtonBox_antenna_toggles.Visible = false;
                                p24_ucTunestepOptionsGrid_buttons.Visible = false;
                                p24_ucOtherButtonsOptionsGrid_buttons.Visible = false;
                                p24_pnlVoiceRecordPlayback.Visible = false;
                                break;
                        }
                    }
                    break;
                case MeterType.HISTORY:
                    p24_grpHistoryItem.Parent = p24_grpMultiMeterHolder;
                    p24_grpHistoryItem.Location = loc;
                    p24_grpHistoryItem.Visible = true;
                    break;
                default:
                    p24_grpMeterItemSettings.Parent = p24_grpMultiMeterHolder;
                    p24_grpMeterItemSettings.Visible = true;
                    break;
            }
        }

        private void radMM12Clock_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void radMM24Clock_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMMClockTitle_CheckedChanged(object sender, EventArgs e)
        {
            updateTitleControlsClock();
            updateMeterType();
        }

        private void clrbtnMMClockTitle_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMTime_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMDate_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMClockBackground_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayBackground_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayTitle_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayFrequency_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayMode_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplaySplitBack_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplaySplit_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayRx_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayTx_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayFilter_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMMVfoDisplayBand_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemEyeBezelScale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemPowerScale_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemIgnoreHistoryDuration_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private MeterManager.clsIGSettings _itemGroupSettings = null;
        private MeterType _itemGroupSettingsMeterType = MeterType.NONE;

        private void btnMeterCopySettings_Click(object sender, EventArgs e)
        {
            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return;

            // copy settings into igs
            _itemGroupSettings = m.GetSettingsForMeterGroup(mt, mtci.Order);
            if (_itemGroupSettings != null)
                _itemGroupSettingsMeterType = meterItemGroupTypefromSelected();
            else
                _itemGroupSettingsMeterType = MeterType.NONE;
        }

        private void btnMeterPasteSettings_Click(object sender, EventArgs e)
        {
            if (_itemGroupSettings == null || _itemGroupSettingsMeterType == MeterType.NONE) return;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return;

            if (canPasteSettings())
            {
                // ignore some things [2.10.1.0] MW0LGE - fixes issue where bar with change units is paste into new bar, and source bars have no sub indicator
                MeterManager.clsIGSettings currentSettings = m.GetSettingsForMeterGroup(mt, mtci.Order);

                // prevent overwrite of the following
                _itemGroupSettings.Unit = currentSettings.Unit;

                if (!_itemGroupSettings.SubIndicators)
                {
                    // no sub indicators on the source, replace with current so we end up with no change on the paste
                    _itemGroupSettings.ShowSubMarker = currentSettings.ShowMarker;
                    _itemGroupSettings.ShowSubMarker = currentSettings.ShowSubMarker;
                    _itemGroupSettings.SubMarkerColour = currentSettings.SubMarkerColour;
                }

                if (mt == MeterType.VOICE_RECORD_PLAY_BUTTONS)
                {
                    // prevent overwrite of the following, copy from current settings
                    _itemGroupSettings.SetSetting<string>("buttonbox_recordplayback_uid", currentSettings.GetSetting<string>("buttonbox_recordplayback_uid", false, null, null, null));
                    _itemGroupSettings.SetSetting<int>("buttonbox_recordplayback_slots", currentSettings.GetSetting<int>("buttonbox_recordplayback_slots", false, 1, MeterManager.clsVoiceRecordPlay.MAX_SLOTS, 8));
                    _itemGroupSettings.SetSetting<short[]>("buttonbox_button_map", currentSettings.GetSetting<short[]>("buttonbox_button_map", false, null, null, null));
                    _itemGroupSettings.SetSetting<string[]>("buttonbox_recordplayback_filepaths", currentSettings.GetSetting<string[]>("buttonbox_recordplayback_filepaths", false, null, null, null)); ;
                    _itemGroupSettings.SetSetting<bool>("buttonbox_recordplayback_wdsp", currentSettings.GetSetting<bool>("buttonbox_recordplayback_wdsp", false, false, false, true));

                    int slots = currentSettings.GetSetting<int>("buttonbox_recordplayback_slots", false, 1, 64, 8);

                    for (int n = 0; n < slots; n++)
                    {
                        _itemGroupSettings.SetSetting<string>("buttonbox_recordplayback_label_" + n.ToString(), currentSettings.GetSetting<string>("buttonbox_recordplayback_label_" + n.ToString(), false, null, null, "Slot " + (n + 1).ToString()));
                        _itemGroupSettings.SetSetting<bool>("buttonbox_recordplayback_locked_" + n.ToString(), currentSettings.GetSetting<bool>("buttonbox_recordplayback_locked_" + n.ToString(), false, false, false, false));
                        _itemGroupSettings.SetSetting<bool>("buttonbox_recordplayback_useskeybind_" + n.ToString(), currentSettings.GetSetting<bool>("buttonbox_recordplayback_useskeybind_" + n.ToString(), false, false, false, false));
                        _itemGroupSettings.SetSetting<Keys>("buttonbox_recordplayback_keybind_" + n.ToString(), currentSettings.GetSetting<Keys>("buttonbox_recordplayback_keybind_" + n.ToString(), false, Keys.None, Keys.None, Keys.None));
                        _itemGroupSettings.SetSetting<bool>("buttonbox_recordplayback_canrepeat_" + n.ToString(), currentSettings.GetSetting<bool>("buttonbox_recordplayback_canrepeat_" + n.ToString(), false, false, false, false));
                        _itemGroupSettings.SetSetting<int>("buttonbox_recordplayback_repeatdelay_" + n.ToString(), currentSettings.GetSetting<int>("buttonbox_recordplayback_repeatdelay_" + n.ToString(), false, 2, 60, 10));
                        _itemGroupSettings.SetSetting<bool>("buttonbox_recordplayback_repeatenabled_" + n.ToString(), currentSettings.GetSetting<bool>("buttonbox_recordplayback_repeatenabled_" + n.ToString(), false, false, false, false));
                        _itemGroupSettings.SetSetting<double>("buttonbox_recordplayback_txgainadjust_" + n.ToString(), currentSettings.GetSetting<double>("buttonbox_recordplayback_txgainadjust_" + n.ToString(), false, -70, 70, 0));
                        _itemGroupSettings.SetSetting<bool>("buttonbox_recordplayback_ignoreplaytempchanges_" + n.ToString(), currentSettings.GetSetting<bool>("buttonbox_recordplayback_ignoreplaytempchanges_" + n.ToString(), false, false, false, true));
                        _itemGroupSettings.SetSetting<bool>("buttonbox_recordplayback_ignorerecordtempchanges_" + n.ToString(), currentSettings.GetSetting<bool>("buttonbox_recordplayback_ignorerecordtempchanges_" + n.ToString(), false, false, false, true));
                    }
                }
                else if (mt == MeterType.WAVE_RECORD)
                {
                    _itemGroupSettings.SetSetting<string[]>("waverecord_filepaths", currentSettings.GetSetting<string[]>("waverecord_filepaths", false, null, null, null));
                    _itemGroupSettings.SetSetting<short[]>("waverecord_order_map", currentSettings.GetSetting<short[]>("waverecord_order_map", false, null, null, null));
                }
                else if (mt == MeterType.OTHER_BUTTONS)
                {
                    // prevent overwrite of the following, copy from current settings
                    _itemGroupSettings.SetSetting<short[]>("buttonbox_button_map", currentSettings.GetSetting<short[]>("buttonbox_button_map", false, null, null, null));
                    for (int n = 0; n < OtherButtonIdHelpers.MAX_BITFIELD_GROUP; n++)
                    {
                        _itemGroupSettings.SetSetting<int>("buttonbox_other_buttons_bitfield_" + n.ToString(), currentSettings.GetSetting<int>("buttonbox_other_buttons_bitfield_" + n.ToString(), true, 0, int.MaxValue, 0));
                    }
                    for (int n = 0; n < OtherButtonIdHelpers.MACRO_BUTTONS_PERGROUP; n++)
                    {
                        _itemGroupSettings.SetSetting<OtherButtonMacroSettings>("buttonbox_other_buttons_macro_settings_" + n.ToString(), currentSettings.GetSetting<OtherButtonMacroSettings>("buttonbox_other_buttons_macro_settings_" + n.ToString(), false, null, null, null));
                    }
                }
                else if (mt == MeterType.TUNESTEP_BUTTONS)
                {
                    // prevent overwrite of the following, copy from current settings
                    _itemGroupSettings.SetSetting<int>("buttonbox_tunestep_bitfield", currentSettings.GetSetting<int>("buttonbox_tunestep_bitfield", true, 0, int.MaxValue, 0));
                }
                else if (mt == MeterType.ANTENNA_BUTTONS)
                {
                    // prevent overwrite of the following, copy from current settings
                    _itemGroupSettings.SetSetting<bool>("buttonbox_rx1", currentSettings.GetSetting<bool>("buttonbox_rx1", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_rx2", currentSettings.GetSetting<bool>("buttonbox_rx2", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_rx3", currentSettings.GetSetting<bool>("buttonbox_rx3", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_tx1", currentSettings.GetSetting<bool>("buttonbox_tx1", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_tx2", currentSettings.GetSetting<bool>("buttonbox_tx2", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_tx3", currentSettings.GetSetting<bool>("buttonbox_tx3", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_byp", currentSettings.GetSetting<bool>("buttonbox_byp", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_ext1", currentSettings.GetSetting<bool>("buttonbox_ext1", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_xvtr", currentSettings.GetSetting<bool>("buttonbox_xvtr", false, false, false, true));
                    _itemGroupSettings.SetSetting<bool>("buttonbox_rxtxant", currentSettings.GetSetting<bool>("buttonbox_rxtxant", false, false, false, true));
                }
                //

                m.ApplySettingsForMeterGroup(mt, _itemGroupSettings, null, mtci.Order);
                updateItemSettingsControlsForSelected();
            }
        }
        private bool canPasteSettings()
        {
            if (_itemGroupSettings == null || _itemGroupSettingsMeterType == MeterType.NONE) return false;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return false;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return false;

            bool bPaste;

            // only allow paste into matching items. Some items allow pasting into non matching types as they share settings
            if (mt == MeterType.MAGIC_EYE || mt == MeterType.CROSS ||
                mt == MeterType.ANANMM || mt == MeterType.SIGNAL_TEXT ||
                mt == MeterType.SPACER || mt == MeterType.TEXT_OVERLAY ||
                mt == MeterType.LED || mt == MeterType.ROTATOR || mt == MeterType.HISTORY ||
                mt == MeterType.VFO_DISPLAY || mt == MeterType.CLOCK ||
                mt == MeterType.FILTER_DISPLAY || mt == MeterType.CUSTOM_METER_BAR ||
                mt == MeterType.WAVE_RECORD ||
                mt == MeterType.VOICE_RECORD_PLAY_BUTTONS
                )
            {
                bPaste = _itemGroupSettingsMeterType == mt;
            }
            else if (_itemGroupSettingsMeterType == MeterType.MAGIC_EYE || _itemGroupSettingsMeterType == MeterType.CROSS ||
                _itemGroupSettingsMeterType == MeterType.ANANMM || _itemGroupSettingsMeterType == MeterType.SIGNAL_TEXT ||
                _itemGroupSettingsMeterType == MeterType.SPACER || _itemGroupSettingsMeterType == MeterType.TEXT_OVERLAY ||
                _itemGroupSettingsMeterType == MeterType.LED || mt == MeterType.ROTATOR || mt == MeterType.HISTORY ||
                _itemGroupSettingsMeterType == MeterType.VFO_DISPLAY || _itemGroupSettingsMeterType == MeterType.CLOCK ||
                _itemGroupSettingsMeterType == MeterType.FILTER_DISPLAY || _itemGroupSettingsMeterType == MeterType.CUSTOM_METER_BAR ||
                _itemGroupSettingsMeterType == MeterType.WAVE_RECORD ||
                _itemGroupSettingsMeterType == MeterType.VOICE_RECORD_PLAY_BUTTONS
                )
            {
                bPaste = mt == _itemGroupSettingsMeterType;
            }
            else
            {
                bPaste = true;
            }

            return bPaste;
        }
        public void ShowMultiMeterSetupTab(string sID = "")
        {
            // show multimeter tab, with meter container already selected if sID is provided
            if (sID != "" && p24_comboContainerSelect.Items.Count > 0)
            {
                for (int n = 0; n < p24_comboContainerSelect.Items.Count; n++)
                {
                    clsContainerComboboxItem cci = p24_comboContainerSelect.Items[n] as clsContainerComboboxItem;
                    if (cci != null && cci.ID == sID)
                    {
                        p24_comboContainerSelect.SelectedIndex = n;
                        break;
                    }
                }
            }

            Show();
            Focus();
            WindowState = FormWindowState.Normal;
            P24SelectMetersGadgetsTab();
        }
        #endregion

private void bntMultiMeterItemRotator_default_pstRotator_Click(object sender, EventArgs e)
        {
            p24_txtMeterItemRotatorAZcommand.Text = "<PST><AZIMUTH>%AZ%</AZIMUTH></PST>";
            p24_txtMeterItemRotatorELEcommand.Text = "<PST><ELEVATION>%ELE%</ELEVATION></PST>";
            p24_txtMeterItemRotatorSTOPcommand.Text = "<PST><STOP>1</STOP></PST>";
        }

private void btnBandButtons_font_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = _bandButtons_font;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!isFontTrueType(fontDialog.Font)) return;
                    _bandButtons_font = fontDialog.Font;
                    updateMeterType();
                }
            }
        }

private void btnContainer_dupe_Click(object sender, EventArgs e)
        {
            if (MeterManager.TotalMeterContainers >= MAX_CONTAINERS) return;

            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci == null) return;

            DialogResult dr = MessageBox.Show("Are you sure you want to duplicate the current container?",
                "Container Duplicate",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, P24ThetisMeterCompat.MB_TOPMOST);

            if (dr != DialogResult.Yes) return;

            if (MeterManager.TotalMeterContainers < MAX_CONTAINERS)
            {
                string data64 = MeterManager.ContainerToString(cci.ID);

                MeterScriptEngine.BeginBatch();
                ucMeter ucm = MeterManager.ContainerFromString(data64);
                MeterManager.RunRendererDisplay(ucm.ID);
                MeterManager.FinishSetupAndDisplay(ucm.ID);

                updateMeter2Controls(ucm.ID);

                MeterScriptEngine.EndBatch();

                p24_btnContainer_save.Enabled = MeterManager.TotalMeterContainers < MAX_CONTAINERS;
            }
        }

private void btnContainer_load_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.Filter = "Container Files|*.dat";
                ofd.Title = "Load Container";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;
                DialogResult dr = ofd.ShowDialog();

                if (ofd.FileName != "" && dr == DialogResult.OK)
                {
                    if (File.Exists(ofd.FileName))
                    {
                        string txt = txt = File.ReadAllText(ofd.FileName, Encoding.UTF8);

                        List<string> webimages = new List<string>();

                        MeterScriptEngine.BeginBatch();

                        ucMeter ucm = MeterManager.ContainerFromString(txt, webimages);

                        if (ucm == null)
                        {
                            MessageBox.Show("This doesnt seem to be a valid container file.",
                                "Container file not recognised",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);

                            MeterScriptEngine.EndBatch();

                            return;
                        }
                        if (webimages.Count > 0)
                        {
                            string msg = "This container file includes web image items(s).\nDo you want to load it?\n\nThe url(s) are as follows.\n\n\n";
                            foreach (string s in webimages)
                            {
                                msg += s + "\n\n";
                            }
                            dr = MessageBox.Show(msg,
                                "Container file contents warning",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, P24ThetisMeterCompat.MB_TOPMOST);

                            if (dr != DialogResult.Yes)
                            {
                                MeterManager.RemoveMeterContainer(ucm.ID);

                                MeterScriptEngine.EndBatch();

                                return;
                            }
                        }

                        dr = MessageBox.Show("Do you want DBManager to take a backup of the database before loading this container?",
                            "Database backup",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);

                        if (dr == DialogResult.Yes)
                        {
                            P24ThetisMeterCompat.TryDatabaseBackup();
                        }

                        MeterManager.RunRendererDisplay(ucm.ID);
                        MeterManager.FinishSetupAndDisplay(ucm.ID);

                        updateMeter2Controls(ucm.ID);

                        MeterScriptEngine.EndBatch();

                        p24_btnContainer_save.Enabled = MeterManager.TotalMeterContainers < MAX_CONTAINERS;
                    }
                }
            }
        }

private void btnContainer_save_Click(object sender, EventArgs e)
        {
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;

            if (cci != null)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    sfd.Filter = "Container Files|*.dat";
                    sfd.Title = "Save Container";
                    sfd.FilterIndex = 1;
                    sfd.RestoreDirectory = true;
                    DialogResult dr = sfd.ShowDialog();

                    if (sfd.FileName != "" && dr == DialogResult.OK)
                    {
                        string data64 = MeterManager.ContainerToString(cci.ID);

                        File.WriteAllText(sfd.FileName, data64, Encoding.UTF8);
                    }
                }
            }
        }

private void btnFilter_4char_copy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(p24_txtWebImage_4char.Text);
        }

private void btnHistory_copy_minmax_from_0_Click(object sender, EventArgs e)
        {
            p24_nudHistory_axis1_min.Value = p24_nudHistory_axis0_min.Value;
            p24_nudHistory_axis1_max.Value = p24_nudHistory_axis0_max.Value;
        }

private void btnLedIndicatorVarPicker_Click(object sender, EventArgs e)
        {
            string var = showVarPickerForClipboard();
        }

private void btnLedIndicator_4char_copy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(p24_txtLedIndicator_4char.Text);
        }

private void btnLedIndicator_copy_sizex_to_y_Click(object sender, EventArgs e)
        {
            p24_nudLedIndicator_ySize.Value = p24_nudLedIndicator_xSize.Value;
        }

private void btnLedIndicator_copy_truefalse_colours_Click(object sender, EventArgs e)
        {
            p24_clrbtnLedIndicator_false.Color = p24_clrbtnLedIndicator_true.Color;
        }

private void btnMMIO_variable_2_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(1);
        }

private void btnMMIO_variable_2_history_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(1);
        }

private void btnMMIO_variable_2_rotator_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(1);
        }

private void btnMMIO_variable_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(0);
        }

private void btnMMIO_variable_history_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(0);
        }

private void btnMMIO_variable_rotator_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(0);
        }

private void btnOtherButtons_reset_layout_Click(object sender, EventArgs e)
        {
            _reset_button_map_layout = true;
            updateMeterType();
            _reset_button_map_layout = false;
        }

private void btnRecording_4char_copy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(p24_txtRecording_4char.Text);
        }

private void btnRecording_assingnkeybind_Click(object sender, EventArgs e)
        {
            p24_pnlVoiceRecordPlayback.Focus();//move focus off the button, so that space/enter can be used
            _setting_globalkeybind = false;
            handleAssignKeybind();
        }

private void btnRecording_export_wav_from_slot_Click(object sender, EventArgs e)
        {
            clsMeterTypeComboboxItem mti = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;
            MeterManager.clsMeterItem mi = m.GetMeterItem(mti.MeterType, mti.Order, MeterManager.clsMeterItem.MeterItemType.VOICE_RECORD_PLAY_BUTTONS);
            if (m == null) return;
            MeterManager.clsVoiceRecordPlay vrp = mi as MeterManager.clsVoiceRecordPlay;
            if (vrp == null) return;

            string file = "Slot_" + (_selected_voice_slot + 1).ToString() + ".wav";
            string fullPath = System.IO.Path.Combine(P24ConsoleDynamic.WrapObject(console).ARP.AudioFolder, vrp.UniqueID, file);

            if (!File.Exists(fullPath))
            {
                return;
            }
            
            bool jsonok = P24ConsoleDynamic.WrapObject(console).ARP.GetJSONDetailsFromFile(fullPath, out clsAudioRecordPlayback.RecordingJsonModel json_data);
            if (jsonok)
            {
                bool fulldata = !string.IsNullOrEmpty(json_data.mode) &&
                    !string.IsNullOrEmpty(json_data.frequency) &&
                    //!string.IsNullOrEmpty(json_data.ddcfrequency) &&
                    json_data.bit_depth > 0 &&
                    json_data.sample_rate > 0 &&
                    !string.IsNullOrEmpty(json_data.utc_time);
                if (fulldata)
                {
                    file = $"{json_data.mode}_{json_data.frequency}MHz_[{json_data.bit_depth}bits_{json_data.sample_rate}Hz]_{json_data.utc_time}.wav";

                    string invalid = new string(Path.GetInvalidFileNameChars());
                    foreach (char c in invalid)
                    {
                        file = file.Replace(c, '_');
                    }
                }
                
            }

            string save_filename = null;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Save WAV file";
                dlg.Filter = "WAV files (*.wav)|*.wav";
                dlg.FilterIndex = 1;
                dlg.DefaultExt = "wav";
                dlg.AddExtension = true;
                dlg.OverwritePrompt = true;
                dlg.CheckPathExists = true;
                dlg.DereferenceLinks = true;

                dlg.InitialDirectory = P24ConsoleDynamic.WrapObject(console).ARP.AudioFolder;

                dlg.FileName = file;

                DialogResult result = dlg.ShowDialog(this);
                if (result != DialogResult.OK) return;

                save_filename = dlg.FileName;
            }

            if (string.IsNullOrEmpty(save_filename)) return;

            if(File.Exists(save_filename))
            {
                try
                {
                    File.Delete(save_filename);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to overwrite existing file.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                    return;
                }
            }

            try
            {
                File.Copy(fullPath, save_filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export WAV file from slot.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                return;
            }
        }

private void btnRecording_globalkeybind_assign_Click(object sender, EventArgs e)
        {
            p24_grpGlobalStopPlayRecord.Focus();//move focus off the button, so that space/enter can be used
            _setting_globalkeybind = true;
            handleAssignKeybind();
        }

private void btnRecording_load_wav_to_slot_Click(object sender, EventArgs e)
        {
            clsMeterTypeComboboxItem mti = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;
            MeterManager.clsMeterItem mi = m.GetMeterItem(mti.MeterType, mti.Order, MeterManager.clsMeterItem.MeterItemType.VOICE_RECORD_PLAY_BUTTONS);
            if (m == null) return;
            MeterManager.clsVoiceRecordPlay vrp = mi as MeterManager.clsVoiceRecordPlay;
            if (vrp == null) return;

            if (vrp.GetSlotLocked(_selected_voice_slot))
            {
                MessageBox.Show("This slot is locked. Unlock it if you want to load a recording into it.", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                return;
            }

            string load_filename = null;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select WAV file";
                dlg.Filter = "WAV files (*.wav)|*.wav";
                dlg.FilterIndex = 1;
                dlg.DefaultExt = "wav";
                dlg.AddExtension = true;
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Multiselect = false;
                dlg.DereferenceLinks = true;

                dlg.InitialDirectory = P24ConsoleDynamic.WrapObject(console).ARP.AudioFolder;

                DialogResult result = dlg.ShowDialog(this);
                if (result != DialogResult.OK) return;

                load_filename = dlg.FileName;
            }
            if (string.IsNullOrEmpty(load_filename)) return;
            if (!File.Exists(load_filename)) return;

            if(!P24ConsoleDynamic.WrapObject(console).ARP.CanBePlayed(load_filename))
            {
                MessageBox.Show("The selected file can not be used. It may be an unsupported format, or it may be corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                return;
            }

            string fullPath = System.IO.Path.Combine(P24ConsoleDynamic.WrapObject(console).ARP.AudioFolder, vrp.UniqueID, "Slot_" + (_selected_voice_slot + 1).ToString() + ".wav");

            P24ConsoleDynamic.WrapObject(console).ARP.DeleteRecording(fullPath, out _);

            if (File.Exists(fullPath))
            {
                MessageBox.Show("A recording already exists that could not be removed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                return; // unable to delete
            }

            string folder = System.IO.Path.Combine(P24ConsoleDynamic.WrapObject(console).ARP.AudioFolder, vrp.UniqueID);
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load WAV file to slot.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                return;
            }

            try
            {
                File.Copy(load_filename, fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load WAV file to slot.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                return;
            }
        }

private void btnRecording_openStorageFolder_Click(object sender, EventArgs e)
        {
            clsMeterTypeComboboxItem mti = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;
            MeterManager.clsMeterItem mi = m.GetMeterItem(mti.MeterType, mti.Order, MeterManager.clsMeterItem.MeterItemType.VOICE_RECORD_PLAY_BUTTONS);
            if (m == null) return;
            MeterManager.clsVoiceRecordPlay vrp = mi as MeterManager.clsVoiceRecordPlay;
            if (vrp == null) return;

            string fullPath = System.IO.Path.Combine(P24ConsoleDynamic.WrapObject(console).ARP.AudioFolder, vrp.UniqueID);

            try
            {
                // recorder class only makes the folder if a recording is made, just add it here if we go to view it
                // before recordings made
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }
            catch { }
            try
            {
                if (Directory.Exists(fullPath))
                {
                    Process.Start("explorer.exe", fullPath);
                }
            }
            catch { }
        }

private void btnRecoverContainer_Click(object sender, EventArgs e)
        {
            if (p24_chkLockContainer.Checked) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;

            if (cci != null)
            {
                MeterManager.RecoverContainer(cci.ID);
                p24_chkContainerShowRX.Checked = true;
                p24_chkContainerShowTX.Checked = true;
            }
        }

private void btnTextOverlayVarPicker_Click(object sender, EventArgs e)
        {
            string var = showVarPickerForClipboard();
        }

private void btnTextOverlay_Font1_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = _textOverlayFont1;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!isFontTrueType(fontDialog.Font)) return;
                    _textOverlayFont1 = fontDialog.Font;
                    updateMeterType();
                }
            }
        }

private void btnTextOverlay_Font2_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = _textOverlayFont2;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!isFontTrueType(fontDialog.Font)) return;
                    _textOverlayFont2 = fontDialog.Font;
                    updateMeterType();
                }
            }
        }

private void btnTextOverlay_copyfonts_Click(object sender, EventArgs e)
        {
            _textOverlayFont2 = new Font(_textOverlayFont1.FontFamily, _textOverlayFont1.Size, _textOverlayFont1.Style);
            p24_clrbtnTextOverlay_TextColour2.Color = p24_clrbtnTextOverlay_TextColour1.Color;
            p24_clrbtnTextOverlay_TextBackColour2.Color = p24_clrbtnTextOverlay_TextBackColour1.Color;
            p24_chkTextOverlay_textback2.Checked = p24_chkTextOverlay_textback1.Checked;

            updateMeterType();
        }

private void btnTextOverlay_copyoffsets_Click(object sender, EventArgs e)
        {
            p24_nudTextOverlay_TXxOffset.Value = p24_nudTextOverlay_RXxOffset.Value;
            p24_nudTextOverlay_TXyOffset.Value = p24_nudTextOverlay_RXyOffset.Value;
        }

private void btnVFOCopyColourFromMainNumbers_Click(object sender, EventArgs e)
        {
            p24_clrbtnMMVfoDisplayFrequency_small.Color = p24_clrbtnMMVfoDisplayFrequency.Color;
        }

private void btnWaveRecord_reset_layout_Click(object sender, EventArgs e)
        {
            _reset_waverecord_order_map = true;
            updateMeterType();
            _reset_waverecord_order_map = false;
        }

private void btnWebImage_bsdworld_visit_Click(object sender, EventArgs e)
        {
            P24ThetisMeterCompat.OpenUri("https://bsdworld.org/help.html");
        }

private void btnWebImage_goto_next_Click(object sender, EventArgs e)
        {
            string four_char = p24_txtWebImage_background_4char.Text;
            if (string.IsNullOrEmpty(four_char)) return;

            (string mid, string igid) = MeterManager.GetWebImageIDsFrom4Char(four_char);
            if (mid == null && igid == null) return;

            ShowMultiMeterSetupTab(mid);

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            foreach (clsMeterTypeComboboxItem mtci in p24_lstMetersInUse.Items)
            {
                string id = m.MeterGroupID(mtci.MeterType, mtci.Order);
                if (id == igid)
                {
                    p24_lstMetersInUse.SelectedItem = mtci;
                    break;
                }
            }
        }

private void btnWebImage_hamqsl_donate_Click(object sender, EventArgs e)
        {
            P24ThetisMeterCompat.OpenUri("https://www.hamqsl.com/donate.html");
        }

private void chkBandButtons_band_inactive_use_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkBandButtons_fade_rx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkBandButtons_fade_tx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkBandButtons_use_indicator_CheckedChanged(object sender, EventArgs e)
        {
            updateButtonIndicatorControls();
            updateMeterType();
        }

private void chkButtonBox_antenna_byp_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_ext1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_rx1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_rx2_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_rx3_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_rxtxant_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_tx1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_tx2_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_tx3_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_antenna_xvtr_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_fix_text_size_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkButtonBox_use_icons_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkContainerMinimises_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ContainerMinimises(cci.ID, p24_chkContainerMinimises.Checked);
            }
        }

private void chkContainerNoTitle_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.NoTitle(cci.ID, p24_chkContainerNoTitle.Checked);
            }
        }

private void chkContainer_hidewhennotused_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ContainerHidesWhenRXNotUsed(cci.ID, p24_chkContainer_hidewhennotused.Checked);
            }
        }

private void chkDialDisplay_alwaysshow_vfos_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkDialDisplay_fade_rx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkDialDisplay_fade_tx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkDial_align_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkFilterDisplay_fadeonrx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkFilterDisplay_fadeontx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkFilterDisplay_fixed_tx_zoom_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            p24_pnlFilterModeModifiers.Enabled = p24_chkFilterDisplay_fixed_zoom.Checked || p24_chkFilterDisplay_fixed_tx_zoom.Checked;
        }

private void chkFilterDisplay_fixed_zoom_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            p24_pnlFilterModeModifiers.Enabled = p24_chkFilterDisplay_fixed_zoom.Checked || p24_chkFilterDisplay_fixed_tx_zoom.Checked;
        }

private void chkFilterDisplay_show_limits_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkFilter_characteristic_CheckedChanged(object sender, EventArgs e)
        {
            p24_nudFilter_lower_characteristic.Enabled = p24_chkFilter_characteristic.Checked;
            updateMeterType();
        }

private void chkFilter_fill_spec_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkFilter_grey_outsidepb_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkFilter_sideband_mode_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkHistory_1_show_axis_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkHistory_auto_0_scale_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkHistory_auto_1_scale_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkHistory_fade_rx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkHistory_fade_tx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLedIndicator_FadeOnRX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLedIndicator_FadeOnTX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLedIndicator_ShowPanel_CheckedChanged(object sender, EventArgs e)
        {
            updateLedIndicatorPanelControls();
            updateMeterType();
        }

private void chkLed_notx_false_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLed_notx_true_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLed_process_when_hidden_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLed_show_false_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLed_show_true_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkLockContainer_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.LockContainer(cci.ID, p24_chkLockContainer.Checked);
                p24_btnRecoverContainer.Enabled = !p24_chkLockContainer.Checked;
                p24_btnContainerDelete.Enabled = !p24_chkLockContainer.Checked;
                p24_btnAddMeterItem.Enabled = !p24_chkLockContainer.Checked;
                p24_btnRemoveMeterItem.Enabled = !p24_chkLockContainer.Checked;
                p24_btnMeterUp.Enabled = !p24_chkLockContainer.Checked;
                p24_btnMeterDown.Enabled = !p24_chkLockContainer.Checked;
            }
        }

private void chkMeterItemDarkModeRotator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkMeterItemFadeOnRxRotator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkMeterItemFadeOnRxSpacer_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkMeterItemFadeOnTxRotator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkMeterItemFadeOnTxSpacer_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkMeterItemRotatorAllowControl_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateRotatorControlControls();
        }

private void chkMeterItemRotatorCardinals_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkMeterItemRotatorShowBeamWidth_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateShowBeamWidthControls();
        }

private void chkMultiMeter_auto_container_height_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.AutoContainerHeight(cci.ID, p24_chkMultiMeter_auto_container_height.Checked);
            }
        }

private void chkMultiMeter_vfo_show_bandtext_CheckedChanged(object sender, EventArgs e)
        {
            updateVfoShowBandtextColour();
            updateMeterType();
        }

private void chkRecording_canRepeat_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            updateMeterType();
        }

private void chkRecording_globalkeybind_CheckedChanged(object sender, EventArgs e)
        {
            p24_txtRecording_globalkeybind.Enabled = p24_chkRecording_globalkeybind.Checked;
            p24_btnRecording_globalkeybind_assign.Enabled = p24_chkRecording_globalkeybind.Checked;
            if(!initializing) _recording_keybind_timer.Stop();
        }

private void chkRecording_ignore_play_tempchanges_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            updateMeterType();
        }

private void chkRecording_ignore_record_tempchanges_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            updateMeterType();
        }

private void chkRecording_playkeybind_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            p24_txtRecording_playkeybind.Enabled = p24_chkRecording_playkeybind.Checked;
            p24_btnRecording_assingnkeybind.Enabled = p24_chkRecording_playkeybind.Checked;
            _recording_keybind_timer.Stop();
            updateMeterType();
        }

private void chkRecording_slot_locked_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            p24_btnRecording_load_wav_to_slot.Enabled = !p24_chkRecording_slot_locked.Checked;
            updateMeterType();
        }

private void chkTextOverlay_FadeOnRX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkTextOverlay_FadeOnTX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkTextOverlay_ShowPanel_CheckedChanged(object sender, EventArgs e)
        {
            updateTextOverlayPanelControls();
            updateMeterType();
        }

private void chkTextOverlay_rx_on_led_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateTextOverlayLedIndicator();
        }

private void chkTextOverlay_textback1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateTextOverlayBackTextControls();
        }

private void chkTextOverlay_textback2_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateTextOverlayBackTextControls();
        }

private void chkTextOverlay_tx_on_led_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateTextOverlayLedIndicator();
        }

private void chkWebImage_background_CheckedChanged(object sender, EventArgs e)
        {
            updateWebImageBackground();
            updateMeterType();
        }

private void chkWebImage_bypass_cache_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkWebImage_fade_rx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void chkWebImage_fade_tx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnBandButtons_border_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnBandButtons_fill_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnBandButtons_hover_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnBandButtons_indicator_off_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnBandButtons_indicator_on_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnButonBox_click_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnButonBox_fontcolour_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnDial_colours_changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilterDisplay_backcolour_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_button_highlight_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_data_fill_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_data_line_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_edge_highlight_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_edges_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_edges_tx_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_extents_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_meter_back_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_notch_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_notch_highlight_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_number_highlight_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_setting_on_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_snap_line_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_text_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnFilter_wf_low_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnHistory_background_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnHistory_colour_0_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnHistory_colour_1_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnHistory_lines_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnHistory_time_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnLedIndicator_PanelBackgroundTX_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnLedIndicator_PanelBackground_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnLedIndicator_false_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnLedIndicator_true_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMMVfoDigitHighlight_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMMVfoDisplayFrequency_small_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemHBackgroundRotator_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemHBackgroundSpacerRX_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemHBackgroundSpacerTX_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemIndicator_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemRotatorArrow_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemRotatorBeamWidth_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemRotatorControlColour_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemRotatorLargeDot_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemRotatorSmallDot_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemRotatorText_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMeterItemSubIndicator_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMultiMeter_vfo_lock_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMultiMeter_vfo_show_bandtext_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnMultiMeter_vfo_sync_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnTextOverlay_PanelBackgroundTX_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnTextOverlay_PanelBackground_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnTextOverlay_TextBackColour1_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnTextOverlay_TextBackColour2_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnTextOverlay_TextColour1_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void clrbtnTextOverlay_TextColour2_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void comboFilter_wf_palette_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void comboHistory_reading_0_SelectedIndexChanged(object sender, EventArgs e)
        {
            clsComboHistoryItem chi = p24_comboHistory_reading_0.SelectedItem as clsComboHistoryItem;
            if (chi == null) return;

            updateMeterType();
        }

private void comboHistory_reading_1_SelectedIndexChanged(object sender, EventArgs e)
        {
            clsComboHistoryItem chi = p24_comboHistory_reading_1.SelectedItem as clsComboHistoryItem;
            if (chi == null) return;

            updateMeterType();
        }

private void comboWebImage_BsdWorld_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (p24_comboWebImage_BsdWorld.SelectedIndex == -1) return;
            if (p24_comboWebImage_BsdWorld.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _bsdworld_urls[p24_comboWebImage_BsdWorld.SelectedIndex];
            string url = kvp.Value.Replace("<light_mode>", p24_chkBSDWorldDarkMode.Checked ? "-dark" : "-light");
            p24_txtWebImage_url.Text = url;

            p24_comboWebImage_BsdWorld.SelectedIndex = 0;
        }

private void comboWebImage_HamQsl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (p24_comboWebImage_HamQsl.SelectedIndex == -1) return;
            if (p24_comboWebImage_HamQsl.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _hamqsl_urls[p24_comboWebImage_HamQsl.SelectedIndex];
            p24_txtWebImage_url.Text = kvp.Value;

            p24_comboWebImage_HamQsl.SelectedIndex = 0;
        }

private void comboWebImage_nasa_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (p24_comboWebImage_nasa.SelectedIndex == -1) return;
            if (p24_comboWebImage_nasa.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _nasa_urls[p24_comboWebImage_nasa.SelectedIndex];
            p24_txtWebImage_url.Text = kvp.Value;

            p24_comboWebImage_nasa.SelectedIndex = 0;
        }

private void comboWebImage_noaa_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (p24_comboWebImage_noaa.SelectedIndex == -1) return;
            if (p24_comboWebImage_noaa.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _noaa_urls[p24_comboWebImage_noaa.SelectedIndex];
            p24_txtWebImage_url.Text = kvp.Value;

            p24_comboWebImage_noaa.SelectedIndex = 0;
        }

private string fixBSDWorldUrls(string url)
        {
            if (!url.Contains("bsdworld.org", StringComparison.OrdinalIgnoreCase)) return "";
            if (url.Contains("-light", StringComparison.OrdinalIgnoreCase) || url.Contains("-dark", StringComparison.OrdinalIgnoreCase)) return "";
            if (!_bsdworld_urls.Any(kvp => (url.Replace("<light_mode>", "")).Contains((kvp.Value).Replace("<light_mode>", "")))) return ""; // the url passed is not in _bsdworld_urls

            // insert before .webp or .svgz
            string mode = p24_chkBSDWorldDarkMode.Checked ? "-dark" : "-light";
            int pos = url.IndexOf(".webp");
            if (pos != -1)
            {
                return url.Replace(".webp", $"{mode}.webp");
            }
            pos = url.IndexOf(".svgz");
            if (pos != -1)
            {
                return url.Replace(".svgz", $"{mode}.svgz");
            }
            return "";
        }

private void handleAssignKeybind()
        {
            _recording_keybind_timer.Stop();

            //start timer, listen for keycode, stop listening after timer end, or this button pressed again
            _listening_for_recording_keycodes = !_listening_for_recording_keycodes;

            // any state will clear it. Only completing via a keypress will store it
            if (_setting_globalkeybind)
            {
                _globalPlayRecordInterrupKeybind = Keys.None;
            }
            else
            {
                p24_txtRecording_playkeybind.Tag = Keys.None;
                updateMeterType();
            }

            if (_listening_for_recording_keycodes)
            {
                _alt_pressed = P24ThetisMeterCompat.AltlKeyDown;
                _shift_pressed = P24ThetisMeterCompat.ShiftKeyDown;
                _ctrl_pressed = P24ThetisMeterCompat.CtrlKeyDown;

                _recording_keybind_timer.Start();

                if (_setting_globalkeybind)
                {
                    p24_txtRecording_globalkeybind.Text = "unset";
                    p24_btnRecording_globalkeybind_assign.Text = "stop";
                }
                else
                {
                    p24_txtRecording_playkeybind.Text = "unset";
                    p24_btnRecording_assingnkeybind.Text = "stop";
                }
            }
            else
            {
                if (_setting_globalkeybind)
                {
                    p24_btnRecording_globalkeybind_assign.Text = "assign";
                }
                else
                {
                    p24_btnRecording_assingnkeybind.Text = "assign";
                }
            }
        }

public bool isFontTrueType(Font f)
        {
            try
            {
                FontFamily ff = new FontFamily(f.Name);
            }
            catch (ArgumentException e)
            {
                if (e.Message.Contains("TrueType", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("This font is not a TrueType font and can not be used.",
                    "Font issue",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, P24ThetisMeterCompat.MB_TOPMOST);
                    return false;
                }

                return false;// also if not found
            }
            return true;
        }

private void mmioSetupVariable(int variable)
        {
            string mgID = meterItemGroupIDfromSelected();
            if (mgID == "") return;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return;

            MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt, mtci.Order);
            if (igs == null) return;

            frmVariablePicker f = new frmVariablePicker();
            f.Init(variable, igs.GetMMIOGuid(variable), igs.GetMMIOVariable(variable));
            DialogResult dr = f.ShowDialog(this);
            if (dr == DialogResult.OK || dr == DialogResult.Ignore)
            {
                igs.SetMMIOGuid(variable, f.Guid);
                igs.SetMMIOVariable(variable, f.Variable);

                m.ApplySettingsForMeterGroup(mt, igs, null, mtci.Order);

                switch (mt)
                {
                    case MeterType.HISTORY:
                        {
                            switch (variable)
                            {
                                case 0:
                                    p24_pnlVariableInUse_1_history.Visible = variableInUse(0);
                                    break;
                                case 1:
                                    p24_pnlVariableInUse_2_history.Visible = variableInUse(1);
                                    break;
                            }
                        }
                        break;
                    case MeterType.ROTATOR:
                        {
                            switch (variable)
                            {
                                case 0:
                                    p24_pnlVariableInUse_1_rotator.Visible = variableInUse(0);
                                    break;
                                case 1:
                                    p24_pnlVariableInUse_2_rotator.Visible = variableInUse(1);
                                    break;
                            }
                        }
                        break;
                    default:
                        {
                            switch (variable)
                            {
                                case 0:
                                    p24_pnlVariableInUse_1.Visible = variableInUse(0);
                                    break;
                                case 1:
                                    p24_pnlVariableInUse_2.Visible = variableInUse(1);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

private void nudBandButtons_border_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudBandButtons_columns_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudBandButtons_height_ratio_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudBandButtons_indicator_border_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudBandButtons_indicator_style_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudBandButtons_margin_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudBandButtons_radius_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudButtonBox_font_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudButtonBox_font_x_shift_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudButtonBox_font_y_shift_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudDataOutNode_sendinterval_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudDialDisplay_font_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudDialDisplay_vertical_ratio_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudDial_decrement_ValueChanged(object sender, EventArgs e)
        {
            if (p24_nudDial_decrement.Value > p24_nudDial_increment.Value - 90)
            {
                p24_nudDial_decrement.Value = p24_nudDial_increment.Value - 90;
                return;
            }
            updateMeterType();
        }

private void nudDial_degrees_for_change_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudDial_increment_ValueChanged(object sender, EventArgs e)
        {
            if (p24_nudDial_increment.Value < p24_nudDial_decrement.Value + 90)
            {
                p24_nudDial_increment.Value = p24_nudDial_decrement.Value + 90;
                return;
            }
            updateMeterType();
        }

private void nudDial_interval_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudDial_max_increments_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilterDisplay_fixed_tx_zoom_level_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilterDisplay_fixed_zoom_level_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilterDisplay_vertical_ratio_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilterItem_cw_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilterItem_font_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilterItem_others_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilterItem_sidebands_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilter_lower_characteristic_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudFilter_waterfall_frame_update_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudHistory_axis0_max_ValueChanged(object sender, EventArgs e)
        {
            if (p24_nudHistory_axis0_max.Value < p24_nudHistory_axis0_min.Value) p24_nudHistory_axis0_min.Value = p24_nudHistory_axis0_max.Value;
            updateMeterType();
        }

private void nudHistory_axis0_min_ValueChanged(object sender, EventArgs e)
        {
            if (p24_nudHistory_axis0_min.Value > p24_nudHistory_axis0_max.Value) p24_nudHistory_axis0_max.Value = p24_nudHistory_axis0_min.Value;
            updateMeterType();
        }

private void nudHistory_axis1_max_ValueChanged(object sender, EventArgs e)
        {
            if (p24_nudHistory_axis1_max.Value < p24_nudHistory_axis1_min.Value) p24_nudHistory_axis1_min.Value = p24_nudHistory_axis1_max.Value;
            updateMeterType();
        }

private void nudHistory_axis1_min_ValueChanged(object sender, EventArgs e)
        {
            if (p24_nudHistory_axis1_min.Value > p24_nudHistory_axis1_max.Value) p24_nudHistory_axis1_max.Value = p24_nudHistory_axis1_min.Value;
            updateMeterType();
        }

private void nudHistory_keep_for_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudHistory_update_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudHistory_vertical_ratio_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudLedIndicator_PanelPadding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudLedIndicator_UpdateInterval_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudLedIndicator_xOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudLedIndicator_xSize_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudLedIndicator_yOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudLedIndicator_ySize_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudMeterItemRotatorBeamWidth_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudMeterItemRotatorBeamWidth_alpha_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudMeterItemRotator_padding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudMeterItemSpacerPadding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudMeterItemUpdateRateRotator_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudMeterItem_custom_high_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || initializing) return;

            _suppressEvents = true;

            if (p24_nudMeterItem_custom_high.Value > p24_nudMeterItem_custom_max.Value)
            {
                p24_nudMeterItem_custom_high.Value = p24_nudMeterItem_custom_max.Value;
            }

            if (p24_nudMeterItem_custom_high.Value < p24_nudMeterItem_custom_min.Value)
            {
                p24_nudMeterItem_custom_high.Value = p24_nudMeterItem_custom_min.Value;
            }

            _suppressEvents = false;

            updateMeterType();
        }

private void nudMeterItem_custom_max_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || initializing) return;

            _suppressEvents = true;

            if (p24_nudMeterItem_custom_max.Value < p24_nudMeterItem_custom_min.Value + (decimal)0.1f)
            {
                p24_nudMeterItem_custom_max.Value = p24_nudMeterItem_custom_min.Value + (decimal)0.1f;
            }

            if (p24_nudMeterItem_custom_high.Value > p24_nudMeterItem_custom_max.Value)
            {
                p24_nudMeterItem_custom_high.Value = p24_nudMeterItem_custom_max.Value;
            }

            _suppressEvents = false;

            updateMeterType();
        }

private void nudMeterItem_custom_min_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressEvents || initializing) return;

            _suppressEvents = true;

            if (p24_nudMeterItem_custom_min.Value > p24_nudMeterItem_custom_max.Value - (decimal)0.1f)
            {
                p24_nudMeterItem_custom_min.Value = p24_nudMeterItem_custom_max.Value - (decimal)0.1f;
            }

            if (p24_nudMeterItem_custom_high.Value < p24_nudMeterItem_custom_min.Value)
            {
                p24_nudMeterItem_custom_high.Value = p24_nudMeterItem_custom_min.Value;
            }

            _suppressEvents = false;

            updateMeterType();
        }

private void nudRecording_repeatDelay_ValueChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            updateMeterType();
        }

private void nudRecording_slot_settings_ValueChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            _selected_voice_slot = ((int)p24_nudRecording_slot_settings.Value) - 1;
            updateItemSettingsControlsForSelected();
        }

private void nudRecording_tx_gain_adjust_ValueChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            updateMeterType();
        }

private void nudTextOverlay_PanelPadding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudTextOverlay_RXxOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudTextOverlay_RXyOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudTextOverlay_TXxOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudTextOverlay_TXyOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudVoiceRecordingPlayback_slots_ValueChanged(object sender, EventArgs e)
        {
            if (initializing) return;

            int slots = (int)p24_nudVoiceRecordingPlayback_slots.Value;

            //check if any slots are locked
            clsMeterTypeComboboxItem mti = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;
            MeterManager.clsMeterItem mi = m.GetMeterItem(mti.MeterType, mti.Order, MeterManager.clsMeterItem.MeterItemType.VOICE_RECORD_PLAY_BUTTONS);
            if (m == null) return;
            MeterManager.clsVoiceRecordPlay vrp = mi as MeterManager.clsVoiceRecordPlay;
            if (vrp == null) return;

            if (vrp.Slots == slots)
            {
                updateSlotSettings(slots);
                updateItemSettingsControlsForSelected();
                return; // slots the same, pointless
            }

            if (vrp.HasLockedSlots)
            {
                // can not be changed whilst recording slots locked, otherwise may delete recordings                
                DialogResult dr = MessageBox.Show("You can not change the number of slots whilst some of the recordings are locked.\n\n" +
                    "Some recordings may be lost if you do this. Do you want to change the number of slots anyway?",
                    "Locked recording slots",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, P24ThetisMeterCompat.MB_TOPMOST);

                if (dr == DialogResult.No)
                {
                    p24_nudVoiceRecordingPlayback_slots.ValueChanged -= nudVoiceRecordingPlayback_slots_ValueChanged;
                    p24_nudVoiceRecordingPlayback_slots.Value = vrp.Slots;
                    p24_nudVoiceRecordingPlayback_slots.ValueChanged += nudVoiceRecordingPlayback_slots_ValueChanged;
                    return;
                }
            }

            updateSlotSettings(slots);

            updateMeterType();
            updateItemSettingsControlsForSelected();
        }

private void nudWebImage_background_time_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudWebImage_update_interval_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void nudWebImage_width_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void radContainer_rx1_data_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (!p24_radContainer_rx1_data.Checked) return;

            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.SetContainerRX(cci.ID, 1);
            }

            chkContainer_hidewhennotused_CheckedChanged(this, EventArgs.Empty);
        }

private void radContainer_rx2_data_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (!p24_radContainer_rx2_data.Checked) return;

            clsContainerComboboxItem cci = (clsContainerComboboxItem)p24_comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.SetContainerRX(cci.ID, 2);
            }

            chkContainer_hidewhennotused_CheckedChanged(this, EventArgs.Empty);
        }

private void radFilterItem_none_CheckedChanged(object sender, EventArgs e)
        {
            if (!p24_radFilterItem_none.Checked) return;
            updateMeterType();
        }

private void radFilterItem_panadaptor_CheckedChanged(object sender, EventArgs e)
        {
            if (!p24_radFilterItem_panadaptor.Checked) return;
            updateMeterType();
        }

private void radFilterItem_panafall_CheckedChanged(object sender, EventArgs e)
        {
            if (!p24_radFilterItem_panafall.Checked) return;
            updateMeterType();
        }

private void radFilterItem_waterfall_CheckedChanged(object sender, EventArgs e)
        {
            if (!p24_radFilterItem_waterfall.Checked) return;
            updateMeterType();
        }

private void radLed_light_blink_CheckedChanged(object sender, EventArgs e)
        {
            if (p24_radLed_light_blink.Checked)
                updateMeterType();
        }

private void radLed_light_on_off_CheckedChanged(object sender, EventArgs e)
        {
            if (p24_radLed_light_on_off.Checked)
                updateMeterType();
        }

private void radLed_light_pulsate_CheckedChanged(object sender, EventArgs e)
        {
            if (p24_radLed_light_pulsate.Checked)
                updateMeterType();
        }

private void radMeterItemRotator_show_az_CheckedChanged(object sender, EventArgs e)
        {
            // only do the checked state for rad controls, as all the others in the group will fire as well
            if (p24_radMeterItemRotator_show_az.Checked)
            {
                updateMeterType();
                p24_nudMeterItemRotator_padding.Enabled = true;
            }
        }

private void radMeterItemRotator_show_both_CheckedChanged(object sender, EventArgs e)
        {
            if (p24_radMeterItemRotator_show_both.Checked)
            {
                updateMeterType();
                p24_nudMeterItemRotator_padding.Enabled = false;
            }
        }

private void radMeterItemRotator_show_ele_CheckedChanged(object sender, EventArgs e)
        {
            if (p24_radMeterItemRotator_show_ele.Checked)
            {
                updateMeterType();
                p24_nudMeterItemRotator_padding.Enabled = true;
            }
        }

private void radMeterItemSettings_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (!p24_radMeterItemSettings.Checked) return;
            setupCustomItemSettings(true);
        }

private void radMeterItemSettings_custom_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (!p24_radMeterItemSettings_custom.Checked) return;
            setupCustomItemSettings(false);
        }

private void radMultiMeter_vfo_display_both_CheckedChanged(object sender, EventArgs e)
        {
            if (!p24_radMultiMeter_vfo_display_both.Checked) return;
            updateMeterType();
        }

private void radMultiMeter_vfo_display_vfoa_CheckedChanged(object sender, EventArgs e)
        {
            if (!p24_radMultiMeter_vfo_display_vfoa.Checked) return;
            updateMeterType();
        }

private void radMultiMeter_vfo_display_vfob_CheckedChanged(object sender, EventArgs e)
        {
            if (!p24_radMultiMeter_vfo_display_vfob.Checked) return;
            updateMeterType();
        }

private void setupCustomItemSettings(bool show_settings)
        {
            if (show_settings)
            {
                p24_pnlMeterItemSettings.Visible = true;
                p24_pnlMeterItemSettings_custom.Visible = false;
            }
            else
            {
                p24_pnlMeterItemSettings_custom.Parent = p24_pnlMeterItemSettings.Parent;
                p24_pnlMeterItemSettings_custom.Location = p24_pnlMeterItemSettings.Location;
                p24_pnlMeterItemSettings_custom.Visible = true;
                p24_pnlMeterItemSettings.Visible = false;
            }
        }

private string showVarPickerForClipboard()
        {
            frmVariablePicker f = new frmVariablePicker();
            f.Init(0, Guid.Empty, "", true);
            DialogResult dr = f.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                string tmp = "%" + f.Variable.Trim('%') + "%";
                Clipboard.SetText(tmp);
                return tmp;
            }
            return "";
        }

private void txtDataOutNode_4charID_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtLedIndicator_4char_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

private void txtLedIndicator_condition_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtMeterItemRotatorAZcommand_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtMeterItemRotatorELEcommand_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtMeterItemRotatorSTOPcommand_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtMeterItem_custom_title_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtMeterItem_custom_units_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtRecording_4char_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

private void txtRecording_labelText_TextChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            updateMeterType();
        }

private void txtRotator_4charID_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtTextOverlay_RXText_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtTextOverlay_TXText_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtTextOverlay_rx_on_led_4char_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtTextOverlay_tx_on_led_4char_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtWebImage_4char_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

private void txtWebImage_background_4char_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void txtWebImage_url_TextChanged(object sender, EventArgs e)
        {
            if (p24_txtWebImage_url.Text.Contains("hamqsl.com", StringComparison.InvariantCultureIgnoreCase) ||
                p24_txtWebImage_url.Text.Contains("bsdworld.org", StringComparison.InvariantCultureIgnoreCase) ||
                //p24_txtWebImage_url.Text.Contains("nascom.nasa.gov", StringComparison.InvariantCultureIgnoreCase) ||
                //p24_txtWebImage_url.Text.Contains("swpc.noaa.gov", StringComparison.InvariantCultureIgnoreCase) ||
                p24_txtWebImage_url.Text.Contains("kc2g.com", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                // for old bsdworld urls, insert -light or -dark
                string bsd = fixBSDWorldUrls(p24_txtWebImage_url.Text);
                if (!string.IsNullOrEmpty(bsd))
                {
                    p24_txtWebImage_url.Text = bsd;
                    return;
                }

                bool old_ignore = _ignoreMeterItemChangeEvents;

                // lock and set the update interval
                p24_nudWebImage_update_interval.Enabled = false;
                _ignoreMeterItemChangeEvents = true;
                p24_nudWebImage_update_interval.Value = (decimal)600;
                _ignoreMeterItemChangeEvents = old_ignore;

                // lock and set the bypass cache
                p24_chkWebImage_bypass_cache.Enabled = false;
                _ignoreMeterItemChangeEvents = true;
                p24_chkWebImage_bypass_cache.Checked = false;
                _ignoreMeterItemChangeEvents = old_ignore;
            }
            else
            {
                if (!p24_nudWebImage_update_interval.Enabled)
                    p24_nudWebImage_update_interval.Enabled = true;
                if (!p24_chkWebImage_bypass_cache.Enabled)
                    p24_chkWebImage_bypass_cache.Enabled = true;
            }

            updateMeterType();
        }

private void ucMeterItemSignalType_SignalTypeChanged(object sender, ucSignalSelect.SignalTypeChangedEventArgs e)
        {
            updateMeterType();
        }

private void ucOtherButtonsOptionsGrid_buttons_CheckboxChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void ucOtherButtonsOptionsGrid_buttons_MacroSetupClicked(object sender, ucOtherButtonsOptionsGrid.MacroButtonEventArgs e)
        {
            MessageBox.Show(this,
                "Macro editor is disabled in this P24 test build. The native Thetis meter/container renderer remains active.",
                "P24 Meters/Gadgets",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

private void ucTunestepOptionsGrid_buttons_checkbox_changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

private void updateButtonIndicatorControls()
        {
            if (initializing) return;
            bool enable = p24_chkBandButtons_use_indicator.Checked;
            p24_lblBandButtons_indicator_border.Enabled = enable;
            p24_nudBandButtons_indicator_border.Enabled = enable;
            p24_lblBandButtons_indicator_style.Enabled = enable;
            p24_nudBandButtons_indicator_style.Enabled = enable;
        }

private void updateLedIndicatorPanelControls()
        {
            bool enabled = p24_chkLedIndicator_ShowPanel.Checked;
            p24_clrbtnLedIndicator_PanelBackground.Enabled = enabled;
            p24_clrbtnLedIndicator_PanelBackgroundTX.Enabled = enabled;
            p24_nudLedIndicator_PanelPadding.Enabled = enabled;
            p24_chkLedIndicator_FadeOnRX.Enabled = enabled;
            p24_chkLedIndicator_FadeOnTX.Enabled = enabled;
            p24_lblLedIndicator_panelbackground.Enabled = enabled;
            p24_lblLedIndicator_panelbackgroundTX.Enabled = enabled;
            p24_nudLedIndicator_PanelPadding.Enabled = enabled;
        }

private void updateRotatorControlControls()
        {
            bool en = p24_chkMeterItemRotatorAllowControl.Checked;
            p24_clrbtnMeterItemRotatorControlColour.Enabled = en;
            p24_txtMeterItemRotatorAZcommand.Enabled = en;
            p24_txtMeterItemRotatorELEcommand.Enabled = en;
            p24_txtMeterItemRotatorSTOPcommand.Enabled = en;
            p24_picMultiMeterRotatorControlInfo.Enabled = en;
            p24_lblMeterItemRotatorAZcommand.Enabled = en;
            p24_lblMeterItemRotatorELEcommand.Enabled = en;
            p24_picMultiMeterRotatorControlInfo.Enabled = en;
            p24_bntMultiMeterItemRotator_default_pstRotator.Enabled = en;
            p24_txtRotator_4charID.Enabled = en;
            p24_lblRotator_4charID.Enabled = en;
        }

private void updateShowBeamWidthControls()
        {
            bool en = p24_chkMeterItemRotatorShowBeamWidth.Checked;
            p24_clrbtnMeterItemRotatorBeamWidth.Enabled = en;
            p24_lblMeterItemRotatorBeamWidth_degrees.Enabled = en;
            p24_nudMeterItemRotatorBeamWidth.Enabled = en;
            p24_lblMeterItemRotatorBeamWidth_alpha.Enabled = en;
            p24_nudMeterItemRotatorBeamWidth_alpha.Enabled = en;
        }

private void updateSlotSettings(int slots)
        {
            p24_nudRecording_slot_settings.ValueChanged -= nudRecording_slot_settings_ValueChanged;
            p24_nudRecording_slot_settings.Maximum = slots;
            p24_nudRecording_slot_settings.ValueChanged += nudRecording_slot_settings_ValueChanged;

            if (_selected_voice_slot + 1 > slots)
            {
                _selected_voice_slot = slots - 1;
                _ignore_slot_count = true;
                updateItemSettingsControlsForSelected();
                _ignore_slot_count = false;
            }
        }

private void updateTextOverlayBackTextControls()
        {
            p24_clrbtnTextOverlay_TextBackColour1.Enabled = p24_chkTextOverlay_textback1.Checked;
            p24_clrbtnTextOverlay_TextBackColour2.Enabled = p24_chkTextOverlay_textback2.Checked;
        }

private void updateTextOverlayLedIndicator()
        {
            p24_txtTextOverlay_rx_on_led_4char.Enabled = p24_chkTextOverlay_rx_on_led.Checked;
            p24_txtTextOverlay_tx_on_led_4char.Enabled = p24_chkTextOverlay_tx_on_led.Checked;
        }

private void updateTextOverlayPanelControls()
        {
            bool enabled = p24_chkTextOverlay_ShowPanel.Checked;
            p24_clrbtnTextOverlay_PanelBackground.Enabled = enabled;
            p24_clrbtnTextOverlay_PanelBackgroundTX.Enabled = enabled;
            p24_nudTextOverlay_PanelPadding.Enabled = enabled;
            p24_chkTextOverlay_FadeOnRX.Enabled = enabled;
            p24_chkTextOverlay_FadeOnTX.Enabled = enabled;
            p24_lblTextOverlay_panelbackground.Enabled = enabled;
            p24_lblTextOverlay_panelbackgroundTX.Enabled = enabled;
            p24_lblTextOverlay_panelpadding.Enabled = enabled;
        }

private void updateVfoShowBandtextColour()
        {
            p24_clrbtnMultiMeter_vfo_show_bandtext.Enabled = p24_chkMultiMeter_vfo_show_bandtext.Checked;
        }

private void updateWebImageBackground()
        {
            bool enabled = p24_chkWebImage_background.Checked;
            p24_lblWebImage_after.Enabled = enabled;
            p24_nudWebImage_background_time.Enabled = enabled;
            p24_lblWebImage_secs.Enabled = enabled;
            p24_txtWebImage_background_4char.Enabled = enabled;
            p24_btnWebImage_goto_next.Enabled = enabled;
        }

private bool variableInUse(int variable)
        {
            string mgID = meterItemGroupIDfromSelected();
            if (mgID == "") return false;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return false;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return false;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return false;

            MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt, mtci.Order);
            if (igs == null) return false;

            return igs.GetMMIOVariable(variable) == "--DEFAULT--" ? false : true;
        }

        private bool preventIfContainerContainsLockedRecordings()
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return false;

            bool prevent = m.MeterHasLockedVoiceRecords();
            if (prevent)
            {
                DialogResult dr = MessageBox.Show("This container contains Voice Record/Playback item(s) that\n" +
                    "have locked recordings. These and all other recordings made with these will be deleted.\n\n" +
                    "Do you want to do this and remove this container?",
                    "Locked recording slots",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, P24ThetisMeterCompat.MB_TOPMOST);

                if (dr == DialogResult.Yes)
                {
                    prevent = false;
                }
            }

            return prevent;
        }

        private bool preventIfItemContainsLockedRecordings()
        {
            clsMeterTypeComboboxItem mti = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return false;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return false;
            MeterManager.clsMeterItem mi = m.GetMeterItem(mti.MeterType, mti.Order, MeterManager.clsMeterItem.MeterItemType.VOICE_RECORD_PLAY_BUTTONS);
            if (m == null) return false;
            MeterManager.clsVoiceRecordPlay vrp = mi as MeterManager.clsVoiceRecordPlay;
            if (vrp == null) return false;

            bool prevent = vrp.HasLockedSlots;
            if (prevent)
            {
                DialogResult dr = MessageBox.Show("This Voice Record/Playback item has locked recordings.\n" +
                    "If you remove this item those recordings will be lost and deleted.\n\n" +
                    "Do you want to do this and remove this item?",
                    "Locked recording slots",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, P24ThetisMeterCompat.MB_TOPMOST);

                if (dr == DialogResult.Yes)
                {
                    prevent = false;
                }
            }

            return prevent;
        }

        private int getTotalColumnsNeededForAntennaButtons()
        {
            int enable_count = 0;
            if (p24_chkButtonBox_antenna_rx1.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_rx2.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_rx3.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_tx1.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_tx2.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_tx3.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_byp.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_ext1.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_xvtr.Checked) enable_count++;
            if (p24_chkButtonBox_antenna_rxtxant.Checked) enable_count++;

            return enable_count;
        }

        private void updateLedValidControls()
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            clsMeterTypeComboboxItem mtci = p24_lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return;

            if (mt == MeterType.LED)
            {
                MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt, mtci.Order);
                if (igs == null) return;

                p24_lblLed_Valid.Text = "Syntax " + (igs.ShowType ? "Valid" : "Invalid");
                p24_lblLed_Valid.ForeColor = igs.ShowType ? Color.LimeGreen : Color.Red;

                tmrLedValid.Enabled = true;
            }
        }

        private void updateWebImageState(ImageFetcher.State state, bool checkSelected = false, string id = "")
        {
            if (checkSelected)
            {
                string mgID = meterItemGroupIDfromSelected();
                if (mgID == "") return;
                if (mgID != id) return;

                MeterType mt = meterItemGroupTypefromSelected();
                if (mt == MeterType.NONE) return;
                if (mt != MeterType.WEB_IMAGE) return;
            }

            string txt;

            switch (state)
            {
                case ImageFetcher.State.IDLE:
                    txt = "idle";
                    break;
                case ImageFetcher.State.OK:
                    txt = "ok";
                    break;
                case ImageFetcher.State.ERROR_URL_ISSUE:
                    txt = "url issue";
                    break;
                case ImageFetcher.State.ERROR_IMAGE_CONVERSION_PROBLEM:
                    txt = "bad image";
                    break;
                case ImageFetcher.State.ERROR_NO_SUITABLE_IMAGE:
                    txt = "no image";
                    break;
                case ImageFetcher.State.WAITING:
                    txt = "waiting";
                    break;
                case ImageFetcher.State.GATHERING_IMAGES:
                    txt = "gathering";
                    break;
                default:
                    txt = "";
                    break;
            }
            p24_lblWebImage_state.Text = txt;
        }

private Font _bandButtons_font = null;

        private void P24SelectMetersGadgetsTab()
        {
            try
            {
                if (p24_tpAppearanceMeter2 == null) return;
                TabControl inner = p24_tpAppearanceMeter2.Parent as TabControl;
                if (inner != null) inner.SelectedTab = p24_tpAppearanceMeter2;

                Control node = inner;
                while (node != null)
                {
                    TabPage page = node.Parent as TabPage;
                    if (page != null && page.Text.IndexOf("Appearance", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        TabControl outer = page.Parent as TabControl;
                        if (outer != null) outer.SelectedTab = page;
                        break;
                    }
                    node = node.Parent;
                }
            }
            catch { }
        }

        private void tmrLedValid_Tick(object sender, EventArgs e)
        {
            if (p24_txtLedIndicator_condition != null && !p24_txtLedIndicator_condition.Visible)
                tmrLedValid.Enabled = false;
            updateLedValidControls();
        }

}
}
