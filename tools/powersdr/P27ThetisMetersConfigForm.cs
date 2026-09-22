using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PowerSDR
{
    internal sealed class P27ThetisMetersConfigForm : Form
    {
        private readonly Console console;
        private readonly ToolTip toolTip1;
        private bool initializing;
        private System.Windows.Forms.GroupBoxTS grpMultiMeterHolder;
        private System.Windows.Forms.CheckBoxTS chkLockContainer;
        private System.Windows.Forms.CheckBoxTS chkContainerShowTX;
        private System.Windows.Forms.CheckBoxTS chkMultiMeter_auto_container_height;
        private System.Windows.Forms.CheckBoxTS chkContainerMinimises;
        private System.Windows.Forms.LabelTS lblMMContainerNotes;
        private System.Windows.Forms.TextBoxTS txtContainerNotes;
        private System.Windows.Forms.CheckBoxTS chkContainerShowRX;
        private System.Windows.Forms.CheckBoxTS chkContainerNoTitle;
        private System.Windows.Forms.ButtonTS btnMeterCopySettings;
        private System.Windows.Forms.ButtonTS btnMeterPasteSettings;
        private System.Windows.Forms.LabelTS lblMMContainerBackground;
        private PowerSDR.ColorButton clrbtnContainerBackground;
        private System.Windows.Forms.CheckBoxTS chkContainerBorder;
        private System.Windows.Forms.GroupBoxTS grpMeterItemSettings;
        private System.Windows.Forms.PanelTS pnlVariableInUse_2;
        private System.Windows.Forms.ButtonTS btnMMIO_variable_2;
        private System.Windows.Forms.ButtonTS btnMMIO_variable;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemIgnoreHistoryDuration;
        private System.Windows.Forms.LabelTS lblMMHistoryIgnore;
        private PowerSDR.ColorButton clrbtnMeterItemPowerScale;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemEyeBezelScale;
        private System.Windows.Forms.LabelTS lblMMEyeBezelSize;
        private System.Windows.Forms.CheckBoxTS chkMeterItemShowSubIndicator;
        private System.Windows.Forms.LabelTS lblMMIndicatorSub;
        private PowerSDR.ColorButton clrbtnMeterItemSubIndicator;
        private System.Windows.Forms.CheckBoxTS chkMeterItemShowIndicator;
        private System.Windows.Forms.LabelTS lblMMsegSolHigh;
        private System.Windows.Forms.LabelTS lblMMsegSolLow;
        private PowerSDR.ColorButton clrbtnMeterItemSegmentedSolidColourHigh;
        private System.Windows.Forms.CheckBoxTS chkMeterItemSolid;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemsPowerLimit;
        private System.Windows.Forms.LabelTS lblMMPowerLimit;
        private System.Windows.Forms.CheckBoxTS chkMeterItemDarkMode;
        private System.Windows.Forms.CheckBoxTS chkMeterItemSignalAverage;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemEyeScale;
        private System.Windows.Forms.LabelTS lblMMEyeSize;
        private PowerSDR.ColorButton clrbtnMeterItemMeterTitle;
        private PowerSDR.ColorButton clrbtnMeterItemPeakValueColour;
        private PowerSDR.ColorButton clrbtnMeterItemSegmentedSolidColourLow;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemDecayRate;
        private System.Windows.Forms.LabelTS labelTS169;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemAttackRate;
        private System.Windows.Forms.LabelTS labelTS168;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemUpdateRate;
        private System.Windows.Forms.LabelTS labelTS167;
        private System.Windows.Forms.TrackBarTS tbMeterItemHistoryAlpha;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemHistoryDuration;
        private System.Windows.Forms.LabelTS lblMMHistory;
        private System.Windows.Forms.CheckBoxTS chkMeterItemPeakValue;
        private System.Windows.Forms.CheckBoxTS chkMeterItemTitle;
        private System.Windows.Forms.CheckBoxTS chkMeterItemSegmented;
        private System.Windows.Forms.LabelTS lblMMBackground;
        private PowerSDR.ColorButton clrbtnMeterItemHBackground;
        private PowerSDR.ColorButton clrbtnMeterItemHistory;
        private PowerSDR.ColorButton clrbtnMeterItemPeakHold;
        private System.Windows.Forms.LabelTS lblMMIndicator;
        private PowerSDR.ColorButton clrbtnMeterItemIndicator;
        private System.Windows.Forms.LabelTS lblMMHigh;
        private System.Windows.Forms.LabelTS lblMMLow;
        private PowerSDR.ColorButton clrbtnMeterItemHigh;
        private PowerSDR.ColorButton clrbtnMeterItemLow;
        private System.Windows.Forms.CheckBoxTS chkMeterItemShadow;
        private System.Windows.Forms.CheckBoxTS chkMeterItemPeakHold;
        private System.Windows.Forms.CheckBoxTS chkMeterItemHistory;
        private System.Windows.Forms.CheckBoxTS chkMeterItemFadeOnTx;
        private System.Windows.Forms.CheckBoxTS chkMeterItemFadeOnRx;
        private System.Windows.Forms.PanelTS pnlVariableInUse_1;
        private System.Windows.Forms.ButtonTS btnMeterUp;
        private System.Windows.Forms.ButtonTS btnMeterDown;
        private System.Windows.Forms.ButtonTS btnRemoveMeterItem;
        private System.Windows.Forms.ButtonTS btnAddMeterItem;
        private System.Windows.Forms.ListBox lstMetersInUse;
        private System.Windows.Forms.ListBox lstMetersAvailable;
        private System.Windows.Forms.CheckBoxTS chkContainerHighlight;
        private System.Windows.Forms.ButtonTS btnContainerDelete;
        private System.Windows.Forms.ComboBoxTS comboContainerSelect;
        private System.Windows.Forms.ButtonTS btnAddRX2Container;
        private System.Windows.Forms.ButtonTS btnAddRX1Container;
        private System.Windows.Forms.GroupBoxTS grpMeterItemClockSettings;
        private System.Windows.Forms.LabelTS lblMMClockBackground;
        private PowerSDR.ColorButton clrbtnMMClockBackground;
        private System.Windows.Forms.LabelTS labelTS164;
        private PowerSDR.ColorButton clrbtnMMDate;
        private System.Windows.Forms.LabelTS labelTS162;
        private PowerSDR.ColorButton clrbtnMMTime;
        private PowerSDR.ColorButton clrbtnMMClockTitle;
        private System.Windows.Forms.CheckBoxTS chkMMClockTitle;
        private System.Windows.Forms.RadioButtonTS radMM24Clock;
        private System.Windows.Forms.RadioButtonTS radMM12Clock;
        private System.Windows.Forms.GroupBoxTS grpMeterItemVfoDisplaySettings;
        private System.Windows.Forms.LabelTS labelTS278;
        private PowerSDR.ColorButton clrbtnMultiMeter_vfo_sync;
        private System.Windows.Forms.LabelTS labelTS279;
        private PowerSDR.ColorButton clrbtnMultiMeter_vfo_lock;
        private System.Windows.Forms.ButtonTS btnVFOCopyColourFromMainNumbers;
        private System.Windows.Forms.LabelTS labelTS251;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayFrequency_small;
        private PowerSDR.ColorButton clrbtnMultiMeter_vfo_show_bandtext;
        private System.Windows.Forms.CheckBoxTS chkMultiMeter_vfo_show_bandtext;
        private System.Windows.Forms.RadioButtonTS radMultiMeter_vfo_display_vfob;
        private System.Windows.Forms.RadioButtonTS radMultiMeter_vfo_display_vfoa;
        private System.Windows.Forms.RadioButtonTS radMultiMeter_vfo_display_both;
        private System.Windows.Forms.LabelTS labelTS216;
        private PowerSDR.ColorButton clrbtnMMVfoDigitHighlight;
        private System.Windows.Forms.LabelTS labelTS177;
        private System.Windows.Forms.LabelTS labelTS176;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayBackground;
        private System.Windows.Forms.LabelTS labelTS175;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayFrequency;
        private System.Windows.Forms.LabelTS labelTS174;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayBand;
        private System.Windows.Forms.LabelTS labelTS173;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayFilter;
        private System.Windows.Forms.LabelTS labelTS172;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayTx;
        private System.Windows.Forms.LabelTS labelTS171;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayRx;
        private System.Windows.Forms.LabelTS labelTS170;
        private PowerSDR.ColorButton clrbtnMMVfoDisplaySplit;
        private System.Windows.Forms.LabelTS labelTS163;
        private PowerSDR.ColorButton clrbtnMMVfoDisplaySplitBack;
        private System.Windows.Forms.LabelTS labelTS166;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayMode;
        private PowerSDR.ColorButton clrbtnMMVfoDisplayTitle;
        private System.Windows.Forms.GroupBoxTS grpMeterItemSpacerSettings;
        private System.Windows.Forms.LabelTS labelTS199;
        private PowerSDR.ColorButton clrbtnMeterItemHBackgroundSpacerTX;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemSpacerPadding;
        private System.Windows.Forms.LabelTS labelTS197;
        private System.Windows.Forms.LabelTS labelTS196;
        private PowerSDR.ColorButton clrbtnMeterItemHBackgroundSpacerRX;
        private System.Windows.Forms.CheckBoxTS chkMeterItemFadeOnTxSpacer;
        private System.Windows.Forms.CheckBoxTS chkMeterItemFadeOnRxSpacer;
        private System.Windows.Forms.GroupBoxTS grpTextOverlay;
        private System.Windows.Forms.ButtonTS btnTextOverlay_copyfonts;
        private System.Windows.Forms.PictureBox pbTextOverlay_variables;
        private System.Windows.Forms.LabelTS lblTextOverlay_panelbackgroundTX;
        private PowerSDR.ColorButton clrbtnTextOverlay_PanelBackgroundTX;
        private System.Windows.Forms.LabelTS labelTS202;
        private System.Windows.Forms.LabelTS labelTS201;
        private System.Windows.Forms.CheckBoxTS chkTextOverlay_textback2;
        private System.Windows.Forms.CheckBoxTS chkTextOverlay_textback1;
        private PowerSDR.ColorButton clrbtnTextOverlay_TextBackColour2;
        private PowerSDR.ColorButton clrbtnTextOverlay_TextBackColour1;
        private System.Windows.Forms.ButtonTS btnTextOverlay_copyoffsets;
        private System.Windows.Forms.LabelTS labelTS207;
        private System.Windows.Forms.LabelTS labelTS208;
        private System.Windows.Forms.NumericUpDownTS nudTextOverlay_TXyOffset;
        private System.Windows.Forms.LabelTS labelTS209;
        private System.Windows.Forms.NumericUpDownTS nudTextOverlay_TXxOffset;
        private System.Windows.Forms.LabelTS labelTS206;
        private System.Windows.Forms.LabelTS labelTS200;
        private System.Windows.Forms.NumericUpDownTS nudTextOverlay_RXyOffset;
        private System.Windows.Forms.LabelTS labelTS205;
        private System.Windows.Forms.NumericUpDownTS nudTextOverlay_RXxOffset;
        private PowerSDR.ColorButton clrbtnTextOverlay_TextColour2;
        private System.Windows.Forms.LabelTS labelTS204;
        private System.Windows.Forms.ButtonTS btnTextOverlay_Font2;
        private System.Windows.Forms.TextBoxTS txtTextOverlay_TXText;
        private System.Windows.Forms.LabelTS labelTS203;
        private System.Windows.Forms.ButtonTS btnTextOverlay_Font1;
        private System.Windows.Forms.TextBoxTS txtTextOverlay_RXText;
        private System.Windows.Forms.CheckBoxTS chkTextOverlay_ShowPanel;
        private PowerSDR.ColorButton clrbtnTextOverlay_TextColour1;
        private System.Windows.Forms.NumericUpDownTS nudTextOverlay_PanelPadding;
        private System.Windows.Forms.LabelTS lblTextOverlay_panelpadding;
        private System.Windows.Forms.LabelTS lblTextOverlay_panelbackground;
        private PowerSDR.ColorButton clrbtnTextOverlay_PanelBackground;
        private System.Windows.Forms.CheckBoxTS chkTextOverlay_FadeOnTX;
        private System.Windows.Forms.CheckBoxTS chkTextOverlay_FadeOnRX;
        private System.Windows.Forms.GroupBoxTS grpMeterItemDataOutNode;
        private System.Windows.Forms.LabelTS labelTS210;
        private System.Windows.Forms.TextBoxTS txtDataOutNode_4charID;
        private System.Windows.Forms.LabelTS labelTS217;
        private System.Windows.Forms.NumericUpDownTS nudDataOutNode_sendinterval;
        private System.Windows.Forms.LabelTS labelTS215;
        private System.Windows.Forms.GroupBoxTS grpMeterItemRotator;
        private System.Windows.Forms.LabelTS lblMeterItemRotatorBeamWidth_alpha;
        private System.Windows.Forms.LabelTS lblMeterItemRotatorBeamWidth_degrees;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemRotatorBeamWidth_alpha;
        private System.Windows.Forms.LabelTS labelTS237;
        private System.Windows.Forms.TextBoxTS txtMeterItemRotatorSTOPcommand;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemRotator_padding;
        private System.Windows.Forms.RadioButtonTS radMeterItemRotator_show_both;
        private System.Windows.Forms.RadioButtonTS radMeterItemRotator_show_ele;
        private System.Windows.Forms.RadioButtonTS radMeterItemRotator_show_az;
        private System.Windows.Forms.LabelTS lblRotator_4charID;
        private System.Windows.Forms.TextBoxTS txtRotator_4charID;
        private System.Windows.Forms.ButtonTS bntMultiMeterItemRotator_default_pstRotator;
        private System.Windows.Forms.PictureBox picMultiMeterRotatorControlInfo;
        private System.Windows.Forms.LabelTS lblMeterItemRotatorELEcommand;
        private System.Windows.Forms.LabelTS lblMeterItemRotatorAZcommand;
        private System.Windows.Forms.TextBoxTS txtMeterItemRotatorELEcommand;
        private System.Windows.Forms.TextBoxTS txtMeterItemRotatorAZcommand;
        private PowerSDR.ColorButton clrbtnMeterItemRotatorControlColour;
        private System.Windows.Forms.CheckBoxTS chkMeterItemRotatorAllowControl;
        private System.Windows.Forms.PanelTS pnlVariableInUse_2_rotator;
        private System.Windows.Forms.CheckBoxTS chkMeterItemRotatorCardinals;
        private System.Windows.Forms.LabelTS labelTS212;
        private PowerSDR.ColorButton clrbtnMeterItemRotatorText;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemRotatorBeamWidth;
        private System.Windows.Forms.ButtonTS btnMMIO_variable_2_rotator;
        private System.Windows.Forms.ButtonTS btnMMIO_variable_rotator;
        private System.Windows.Forms.CheckBoxTS chkMeterItemRotatorShowBeamWidth;
        private System.Windows.Forms.LabelTS labelTS218;
        private PowerSDR.ColorButton clrbtnMeterItemRotatorBeamWidth;
        private System.Windows.Forms.CheckBoxTS chkMeterItemDarkModeRotator;
        private System.Windows.Forms.NumericUpDownTS nudMeterItemUpdateRateRotator;
        private System.Windows.Forms.LabelTS labelTS225;
        private System.Windows.Forms.LabelTS labelTS227;
        private PowerSDR.ColorButton clrbtnMeterItemHBackgroundRotator;
        private System.Windows.Forms.LabelTS labelTS228;
        private PowerSDR.ColorButton clrbtnMeterItemRotatorSmallDot;
        private System.Windows.Forms.LabelTS labelTS229;
        private System.Windows.Forms.LabelTS labelTS230;
        private PowerSDR.ColorButton clrbtnMeterItemRotatorLargeDot;
        private PowerSDR.ColorButton clrbtnMeterItemRotatorArrow;
        private System.Windows.Forms.CheckBoxTS chkMeterItemFadeOnTxRotator;
        private System.Windows.Forms.CheckBoxTS chkMeterItemFadeOnRxRotator;
        private System.Windows.Forms.PanelTS pnlVariableInUse_1_rotator;
        private System.Windows.Forms.GroupBoxTS grpLedIndicator;
        private System.Windows.Forms.RadioButtonTS radLed_light_pulsate;
        private System.Windows.Forms.RadioButtonTS radLed_light_blink;
        private System.Windows.Forms.RadioButtonTS radLed_light_on_off;
        private System.Windows.Forms.CheckBoxTS chkLed_show_false;
        private System.Windows.Forms.CheckBoxTS chkLed_show_true;
        private System.Windows.Forms.LabelTS lblLed_Error;
        private System.Windows.Forms.LabelTS lblLed_Valid;
        private System.Windows.Forms.ButtonTS btnLedIndicator_copy_truefalse_colours;
        private System.Windows.Forms.PictureBox pbLedIndicator_condition_tips;
        private System.Windows.Forms.LabelTS lblLedIndicator_panelbackgroundTX;
        private PowerSDR.ColorButton clrbtnLedIndicator_PanelBackgroundTX;
        private System.Windows.Forms.LabelTS labelTS219;
        private System.Windows.Forms.LabelTS labelTS220;
        private PowerSDR.ColorButton clrbtnLedIndicator_false;
        private PowerSDR.ColorButton clrbtnLedIndicator_true;
        private System.Windows.Forms.ButtonTS btnLedIndicator_copy_sizex_to_y;
        private System.Windows.Forms.LabelTS labelTS221;
        private System.Windows.Forms.LabelTS labelTS222;
        private System.Windows.Forms.NumericUpDownTS nudLedIndicator_ySize;
        private System.Windows.Forms.LabelTS labelTS223;
        private System.Windows.Forms.NumericUpDownTS nudLedIndicator_xSize;
        private System.Windows.Forms.LabelTS labelTS224;
        private System.Windows.Forms.LabelTS labelTS226;
        private System.Windows.Forms.NumericUpDownTS nudLedIndicator_yOffset;
        private System.Windows.Forms.LabelTS labelTS231;
        private System.Windows.Forms.NumericUpDownTS nudLedIndicator_xOffset;
        private System.Windows.Forms.LabelTS labelTS233;
        private System.Windows.Forms.TextBoxTS txtLedIndicator_condition;
        private System.Windows.Forms.CheckBoxTS chkLedIndicator_ShowPanel;
        private System.Windows.Forms.NumericUpDownTS nudLedIndicator_PanelPadding;
        private System.Windows.Forms.LabelTS labelTS234;
        private System.Windows.Forms.LabelTS lblLedIndicator_panelbackground;
        private PowerSDR.ColorButton clrbtnLedIndicator_PanelBackground;
        private System.Windows.Forms.CheckBoxTS chkLedIndicator_FadeOnTX;
        private System.Windows.Forms.CheckBoxTS chkLedIndicator_FadeOnRX;
        private System.Windows.Forms.GroupBoxTS grpWebImage;
        private System.Windows.Forms.CheckBoxTS chkWebImage_bypass_cache;
        private System.Windows.Forms.GroupBoxTS groupBoxTS42;
        private System.Windows.Forms.ButtonTS buttonTS1;
        private System.Windows.Forms.ComboBoxTS comboWebImage_noaa;
        private System.Windows.Forms.GroupBoxTS groupBoxTS41;
        private System.Windows.Forms.ButtonTS btnWebImage_bsdworld_visit;
        private System.Windows.Forms.ComboBoxTS comboWebImage_BsdWorld;
        private System.Windows.Forms.GroupBoxTS groupBoxTS43;
        private System.Windows.Forms.ButtonTS buttonTS2;
        private System.Windows.Forms.ComboBoxTS comboWebImage_nasa;
        private System.Windows.Forms.LabelTS lblWebImage_state;
        private System.Windows.Forms.GroupBoxTS groupBoxTS40;
        private System.Windows.Forms.ButtonTS btnWebImage_hamqsl_donate;
        private System.Windows.Forms.ComboBoxTS comboWebImage_HamQsl;
        private System.Windows.Forms.LabelTS labelTS236;
        private System.Windows.Forms.TextBoxTS txtWebImage_url;
        private System.Windows.Forms.NumericUpDownTS nudWebImage_update_interval;
        private System.Windows.Forms.LabelTS labelTS232;
        private System.Windows.Forms.NumericUpDownTS nudWebImage_width_scale;
        private System.Windows.Forms.LabelTS labelTS235;
        private System.Windows.Forms.CheckBoxTS chkWebImage_fade_tx;
        private System.Windows.Forms.CheckBoxTS chkWebImage_fade_rx;
        private System.Windows.Forms.GroupBoxTS grpBandButtons;
        private System.Windows.Forms.PanelTS pnlButtonBox_antenna_toggles;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_rxtxant;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_xvtr;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_ext1;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_byp;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_tx3;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_tx2;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_tx1;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_rx3;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_rx2;
        private System.Windows.Forms.CheckBoxTS chkButtonBox_antenna_rx1;
        private System.Windows.Forms.NumericUpDownTS nudButtonBox_font_y_shift;
        private System.Windows.Forms.LabelTS labelTS250;
        private System.Windows.Forms.NumericUpDownTS nudButtonBox_font_x_shift;
        private System.Windows.Forms.LabelTS labelTS247;
        private System.Windows.Forms.NumericUpDownTS nudButtonBox_font_scale;
        private System.Windows.Forms.LabelTS labelTS248;
        private System.Windows.Forms.LabelTS lblBandButtons_indicator_style;
        private System.Windows.Forms.NumericUpDownTS nudBandButtons_indicator_style;
        private System.Windows.Forms.CheckBoxTS chkBandButtons_band_inactive_use;
        private System.Windows.Forms.LabelTS labelTS246;
        private PowerSDR.ColorButton clrbtnBandButtons_hover;
        private System.Windows.Forms.LabelTS labelTS245;
        private PowerSDR.ColorButton clrbtnBandButtons_fill;
        private System.Windows.Forms.LabelTS labelTS244;
        private PowerSDR.ColorButton clrbtnBandButtons_border;
        private System.Windows.Forms.LabelTS labelTS243;
        private PowerSDR.ColorButton clrbtnBandButtons_indicator_off;
        private System.Windows.Forms.LabelTS labelTS242;
        private System.Windows.Forms.NumericUpDownTS nudBandButtons_indicator_border;
        private System.Windows.Forms.LabelTS lblBandButtons_indicator_border;
        private System.Windows.Forms.NumericUpDownTS nudBandButtons_height_ratio;
        private System.Windows.Forms.LabelTS labelTS241;
        private System.Windows.Forms.NumericUpDownTS nudBandButtons_radius;
        private System.Windows.Forms.LabelTS labelTS240;
        private System.Windows.Forms.NumericUpDownTS nudBandButtons_margin;
        private System.Windows.Forms.LabelTS labelTS239;
        private System.Windows.Forms.NumericUpDownTS nudBandButtons_border;
        private System.Windows.Forms.LabelTS labelTS238;
        private System.Windows.Forms.ButtonTS btnBandButtons_font;
        private System.Windows.Forms.CheckBoxTS chkBandButtons_use_indicator;
        private System.Windows.Forms.NumericUpDownTS nudBandButtons_columns;
        private System.Windows.Forms.LabelTS labelTS249;
        private PowerSDR.ColorButton clrbtnBandButtons_indicator_on;
        private System.Windows.Forms.CheckBoxTS chkBandButtons_fade_tx;
        private System.Windows.Forms.CheckBoxTS chkBandButtons_fade_rx;
        private System.Windows.Forms.GroupBoxTS grpHistoryItem;
        private System.Windows.Forms.LabelTS labelTS281;
        private System.Windows.Forms.LabelTS labelTS280;
        private PowerSDR.ColorButton clrbtnHistory_time;
        private PowerSDR.ColorButton clrbtnHistory_lines;
        private System.Windows.Forms.GroupBoxTS groupBoxTS46;
        private PowerSDR.ColorButton clrbtnHistory_colour_1;
        private System.Windows.Forms.ButtonTS btnHistory_copy_minmax_from_0;
        private System.Windows.Forms.CheckBoxTS chkHistory_1_show_axis;
        private System.Windows.Forms.NumericUpDownTS nudHistory_axis1_max;
        private System.Windows.Forms.LabelTS labelTS263;
        private System.Windows.Forms.NumericUpDownTS nudHistory_axis1_min;
        private System.Windows.Forms.LabelTS labelTS264;
        private System.Windows.Forms.CheckBoxTS chkHistory_auto_1_scale;
        private System.Windows.Forms.ComboBoxTS comboHistory_reading_1;
        private System.Windows.Forms.LabelTS labelTS265;
        private System.Windows.Forms.GroupBoxTS groupBoxTS45;
        private PowerSDR.ColorButton clrbtnHistory_colour_0;
        private System.Windows.Forms.NumericUpDownTS nudHistory_axis0_max;
        private System.Windows.Forms.LabelTS labelTS262;
        private System.Windows.Forms.NumericUpDownTS nudHistory_axis0_min;
        private System.Windows.Forms.LabelTS labelTS261;
        private System.Windows.Forms.CheckBoxTS chkHistory_auto_0_scale;
        private System.Windows.Forms.ComboBoxTS comboHistory_reading_0;
        private System.Windows.Forms.LabelTS labelTS260;
        private System.Windows.Forms.PanelTS pnlVariableInUse_2_history;
        private System.Windows.Forms.ButtonTS btnMMIO_variable_2_history;
        private System.Windows.Forms.ButtonTS btnMMIO_variable_history;
        private System.Windows.Forms.PanelTS pnlVariableInUse_1_history;
        private System.Windows.Forms.NumericUpDownTS nudHistory_keep_for;
        private System.Windows.Forms.LabelTS labelTS259;
        private System.Windows.Forms.NumericUpDownTS nudHistory_update;
        private System.Windows.Forms.LabelTS labelTS252;
        private System.Windows.Forms.NumericUpDownTS nudHistory_vertical_ratio;
        private System.Windows.Forms.LabelTS labelTS253;
        private System.Windows.Forms.LabelTS labelTS258;
        private PowerSDR.ColorButton clrbtnHistory_background;
        private System.Windows.Forms.CheckBoxTS chkHistory_fade_tx;
        private System.Windows.Forms.CheckBoxTS chkHistory_fade_rx;


        internal P27ThetisMetersConfigForm(Console c)
        {
            if (c == null) throw new ArgumentNullException("c");
            console = c;
            toolTip1 = new ToolTip();
            initializing = true;
            InitializeExactComponent();
            initializing = false;
            btnAddRX2Container.Visible = false;
            updateMeter2Controls("");
            FormClosing += P27ThetisMetersConfigForm_FormClosing;
            VisibleChanged += P27ThetisMetersConfigForm_VisibleChanged;
        }

        private void InitializeExactComponent()
        {
            this.grpMultiMeterHolder = new System.Windows.Forms.GroupBoxTS();
            this.chkLockContainer = new System.Windows.Forms.CheckBoxTS();
            this.chkContainerShowTX = new System.Windows.Forms.CheckBoxTS();
            this.chkMultiMeter_auto_container_height = new System.Windows.Forms.CheckBoxTS();
            this.chkContainerMinimises = new System.Windows.Forms.CheckBoxTS();
            this.lblMMContainerNotes = new System.Windows.Forms.LabelTS();
            this.txtContainerNotes = new System.Windows.Forms.TextBoxTS();
            this.chkContainerShowRX = new System.Windows.Forms.CheckBoxTS();
            this.chkContainerNoTitle = new System.Windows.Forms.CheckBoxTS();
            this.btnMeterCopySettings = new System.Windows.Forms.ButtonTS();
            this.btnMeterPasteSettings = new System.Windows.Forms.ButtonTS();
            this.lblMMContainerBackground = new System.Windows.Forms.LabelTS();
            this.clrbtnContainerBackground = new PowerSDR.ColorButton();
            this.chkContainerBorder = new System.Windows.Forms.CheckBoxTS();
            this.grpMeterItemSettings = new System.Windows.Forms.GroupBoxTS();
            this.pnlVariableInUse_2 = new System.Windows.Forms.PanelTS();
            this.btnMMIO_variable_2 = new System.Windows.Forms.ButtonTS();
            this.btnMMIO_variable = new System.Windows.Forms.ButtonTS();
            this.nudMeterItemIgnoreHistoryDuration = new System.Windows.Forms.NumericUpDownTS();
            this.lblMMHistoryIgnore = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemPowerScale = new PowerSDR.ColorButton();
            this.nudMeterItemEyeBezelScale = new System.Windows.Forms.NumericUpDownTS();
            this.lblMMEyeBezelSize = new System.Windows.Forms.LabelTS();
            this.chkMeterItemShowSubIndicator = new System.Windows.Forms.CheckBoxTS();
            this.lblMMIndicatorSub = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemSubIndicator = new PowerSDR.ColorButton();
            this.chkMeterItemShowIndicator = new System.Windows.Forms.CheckBoxTS();
            this.lblMMsegSolHigh = new System.Windows.Forms.LabelTS();
            this.lblMMsegSolLow = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemSegmentedSolidColourHigh = new PowerSDR.ColorButton();
            this.chkMeterItemSolid = new System.Windows.Forms.CheckBoxTS();
            this.nudMeterItemsPowerLimit = new System.Windows.Forms.NumericUpDownTS();
            this.lblMMPowerLimit = new System.Windows.Forms.LabelTS();
            this.chkMeterItemDarkMode = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemSignalAverage = new System.Windows.Forms.CheckBoxTS();
            this.nudMeterItemEyeScale = new System.Windows.Forms.NumericUpDownTS();
            this.lblMMEyeSize = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemMeterTitle = new PowerSDR.ColorButton();
            this.clrbtnMeterItemPeakValueColour = new PowerSDR.ColorButton();
            this.clrbtnMeterItemSegmentedSolidColourLow = new PowerSDR.ColorButton();
            this.nudMeterItemDecayRate = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS169 = new System.Windows.Forms.LabelTS();
            this.nudMeterItemAttackRate = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS168 = new System.Windows.Forms.LabelTS();
            this.nudMeterItemUpdateRate = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS167 = new System.Windows.Forms.LabelTS();
            this.tbMeterItemHistoryAlpha = new System.Windows.Forms.TrackBarTS();
            this.nudMeterItemHistoryDuration = new System.Windows.Forms.NumericUpDownTS();
            this.lblMMHistory = new System.Windows.Forms.LabelTS();
            this.chkMeterItemPeakValue = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemTitle = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemSegmented = new System.Windows.Forms.CheckBoxTS();
            this.lblMMBackground = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemHBackground = new PowerSDR.ColorButton();
            this.clrbtnMeterItemHistory = new PowerSDR.ColorButton();
            this.clrbtnMeterItemPeakHold = new PowerSDR.ColorButton();
            this.lblMMIndicator = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemIndicator = new PowerSDR.ColorButton();
            this.lblMMHigh = new System.Windows.Forms.LabelTS();
            this.lblMMLow = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemHigh = new PowerSDR.ColorButton();
            this.clrbtnMeterItemLow = new PowerSDR.ColorButton();
            this.chkMeterItemShadow = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemPeakHold = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemHistory = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemFadeOnTx = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemFadeOnRx = new System.Windows.Forms.CheckBoxTS();
            this.pnlVariableInUse_1 = new System.Windows.Forms.PanelTS();
            this.btnMeterUp = new System.Windows.Forms.ButtonTS();
            this.btnMeterDown = new System.Windows.Forms.ButtonTS();
            this.btnRemoveMeterItem = new System.Windows.Forms.ButtonTS();
            this.btnAddMeterItem = new System.Windows.Forms.ButtonTS();
            this.lstMetersInUse = new System.Windows.Forms.ListBox();
            this.lstMetersAvailable = new System.Windows.Forms.ListBox();
            this.chkContainerHighlight = new System.Windows.Forms.CheckBoxTS();
            this.btnContainerDelete = new System.Windows.Forms.ButtonTS();
            this.comboContainerSelect = new System.Windows.Forms.ComboBoxTS();
            this.btnAddRX2Container = new System.Windows.Forms.ButtonTS();
            this.btnAddRX1Container = new System.Windows.Forms.ButtonTS();
            this.grpMeterItemClockSettings = new System.Windows.Forms.GroupBoxTS();
            this.lblMMClockBackground = new System.Windows.Forms.LabelTS();
            this.clrbtnMMClockBackground = new PowerSDR.ColorButton();
            this.labelTS164 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMDate = new PowerSDR.ColorButton();
            this.labelTS162 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMTime = new PowerSDR.ColorButton();
            this.clrbtnMMClockTitle = new PowerSDR.ColorButton();
            this.chkMMClockTitle = new System.Windows.Forms.CheckBoxTS();
            this.radMM24Clock = new System.Windows.Forms.RadioButtonTS();
            this.radMM12Clock = new System.Windows.Forms.RadioButtonTS();
            this.grpMeterItemVfoDisplaySettings = new System.Windows.Forms.GroupBoxTS();
            this.labelTS278 = new System.Windows.Forms.LabelTS();
            this.clrbtnMultiMeter_vfo_sync = new PowerSDR.ColorButton();
            this.labelTS279 = new System.Windows.Forms.LabelTS();
            this.clrbtnMultiMeter_vfo_lock = new PowerSDR.ColorButton();
            this.btnVFOCopyColourFromMainNumbers = new System.Windows.Forms.ButtonTS();
            this.labelTS251 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayFrequency_small = new PowerSDR.ColorButton();
            this.clrbtnMultiMeter_vfo_show_bandtext = new PowerSDR.ColorButton();
            this.chkMultiMeter_vfo_show_bandtext = new System.Windows.Forms.CheckBoxTS();
            this.radMultiMeter_vfo_display_vfob = new System.Windows.Forms.RadioButtonTS();
            this.radMultiMeter_vfo_display_vfoa = new System.Windows.Forms.RadioButtonTS();
            this.radMultiMeter_vfo_display_both = new System.Windows.Forms.RadioButtonTS();
            this.labelTS216 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDigitHighlight = new PowerSDR.ColorButton();
            this.labelTS177 = new System.Windows.Forms.LabelTS();
            this.labelTS176 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayBackground = new PowerSDR.ColorButton();
            this.labelTS175 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayFrequency = new PowerSDR.ColorButton();
            this.labelTS174 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayBand = new PowerSDR.ColorButton();
            this.labelTS173 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayFilter = new PowerSDR.ColorButton();
            this.labelTS172 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayTx = new PowerSDR.ColorButton();
            this.labelTS171 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayRx = new PowerSDR.ColorButton();
            this.labelTS170 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplaySplit = new PowerSDR.ColorButton();
            this.labelTS163 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplaySplitBack = new PowerSDR.ColorButton();
            this.labelTS166 = new System.Windows.Forms.LabelTS();
            this.clrbtnMMVfoDisplayMode = new PowerSDR.ColorButton();
            this.clrbtnMMVfoDisplayTitle = new PowerSDR.ColorButton();
            this.grpMeterItemSpacerSettings = new System.Windows.Forms.GroupBoxTS();
            this.labelTS199 = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemHBackgroundSpacerTX = new PowerSDR.ColorButton();
            this.nudMeterItemSpacerPadding = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS197 = new System.Windows.Forms.LabelTS();
            this.labelTS196 = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemHBackgroundSpacerRX = new PowerSDR.ColorButton();
            this.chkMeterItemFadeOnTxSpacer = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemFadeOnRxSpacer = new System.Windows.Forms.CheckBoxTS();
            this.grpTextOverlay = new System.Windows.Forms.GroupBoxTS();
            this.btnTextOverlay_copyfonts = new System.Windows.Forms.ButtonTS();
            this.pbTextOverlay_variables = new System.Windows.Forms.PictureBox();
            this.lblTextOverlay_panelbackgroundTX = new System.Windows.Forms.LabelTS();
            this.clrbtnTextOverlay_PanelBackgroundTX = new PowerSDR.ColorButton();
            this.labelTS202 = new System.Windows.Forms.LabelTS();
            this.labelTS201 = new System.Windows.Forms.LabelTS();
            this.chkTextOverlay_textback2 = new System.Windows.Forms.CheckBoxTS();
            this.chkTextOverlay_textback1 = new System.Windows.Forms.CheckBoxTS();
            this.clrbtnTextOverlay_TextBackColour2 = new PowerSDR.ColorButton();
            this.clrbtnTextOverlay_TextBackColour1 = new PowerSDR.ColorButton();
            this.btnTextOverlay_copyoffsets = new System.Windows.Forms.ButtonTS();
            this.labelTS207 = new System.Windows.Forms.LabelTS();
            this.labelTS208 = new System.Windows.Forms.LabelTS();
            this.nudTextOverlay_TXyOffset = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS209 = new System.Windows.Forms.LabelTS();
            this.nudTextOverlay_TXxOffset = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS206 = new System.Windows.Forms.LabelTS();
            this.labelTS200 = new System.Windows.Forms.LabelTS();
            this.nudTextOverlay_RXyOffset = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS205 = new System.Windows.Forms.LabelTS();
            this.nudTextOverlay_RXxOffset = new System.Windows.Forms.NumericUpDownTS();
            this.clrbtnTextOverlay_TextColour2 = new PowerSDR.ColorButton();
            this.labelTS204 = new System.Windows.Forms.LabelTS();
            this.btnTextOverlay_Font2 = new System.Windows.Forms.ButtonTS();
            this.txtTextOverlay_TXText = new System.Windows.Forms.TextBoxTS();
            this.labelTS203 = new System.Windows.Forms.LabelTS();
            this.btnTextOverlay_Font1 = new System.Windows.Forms.ButtonTS();
            this.txtTextOverlay_RXText = new System.Windows.Forms.TextBoxTS();
            this.chkTextOverlay_ShowPanel = new System.Windows.Forms.CheckBoxTS();
            this.clrbtnTextOverlay_TextColour1 = new PowerSDR.ColorButton();
            this.nudTextOverlay_PanelPadding = new System.Windows.Forms.NumericUpDownTS();
            this.lblTextOverlay_panelpadding = new System.Windows.Forms.LabelTS();
            this.lblTextOverlay_panelbackground = new System.Windows.Forms.LabelTS();
            this.clrbtnTextOverlay_PanelBackground = new PowerSDR.ColorButton();
            this.chkTextOverlay_FadeOnTX = new System.Windows.Forms.CheckBoxTS();
            this.chkTextOverlay_FadeOnRX = new System.Windows.Forms.CheckBoxTS();
            this.grpMeterItemDataOutNode = new System.Windows.Forms.GroupBoxTS();
            this.labelTS210 = new System.Windows.Forms.LabelTS();
            this.txtDataOutNode_4charID = new System.Windows.Forms.TextBoxTS();
            this.labelTS217 = new System.Windows.Forms.LabelTS();
            this.nudDataOutNode_sendinterval = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS215 = new System.Windows.Forms.LabelTS();
            this.grpMeterItemRotator = new System.Windows.Forms.GroupBoxTS();
            this.lblMeterItemRotatorBeamWidth_alpha = new System.Windows.Forms.LabelTS();
            this.lblMeterItemRotatorBeamWidth_degrees = new System.Windows.Forms.LabelTS();
            this.nudMeterItemRotatorBeamWidth_alpha = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS237 = new System.Windows.Forms.LabelTS();
            this.txtMeterItemRotatorSTOPcommand = new System.Windows.Forms.TextBoxTS();
            this.nudMeterItemRotator_padding = new System.Windows.Forms.NumericUpDownTS();
            this.radMeterItemRotator_show_both = new System.Windows.Forms.RadioButtonTS();
            this.radMeterItemRotator_show_ele = new System.Windows.Forms.RadioButtonTS();
            this.radMeterItemRotator_show_az = new System.Windows.Forms.RadioButtonTS();
            this.lblRotator_4charID = new System.Windows.Forms.LabelTS();
            this.txtRotator_4charID = new System.Windows.Forms.TextBoxTS();
            this.bntMultiMeterItemRotator_default_pstRotator = new System.Windows.Forms.ButtonTS();
            this.picMultiMeterRotatorControlInfo = new System.Windows.Forms.PictureBox();
            this.lblMeterItemRotatorELEcommand = new System.Windows.Forms.LabelTS();
            this.lblMeterItemRotatorAZcommand = new System.Windows.Forms.LabelTS();
            this.txtMeterItemRotatorELEcommand = new System.Windows.Forms.TextBoxTS();
            this.txtMeterItemRotatorAZcommand = new System.Windows.Forms.TextBoxTS();
            this.clrbtnMeterItemRotatorControlColour = new PowerSDR.ColorButton();
            this.chkMeterItemRotatorAllowControl = new System.Windows.Forms.CheckBoxTS();
            this.pnlVariableInUse_2_rotator = new System.Windows.Forms.PanelTS();
            this.chkMeterItemRotatorCardinals = new System.Windows.Forms.CheckBoxTS();
            this.labelTS212 = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemRotatorText = new PowerSDR.ColorButton();
            this.nudMeterItemRotatorBeamWidth = new System.Windows.Forms.NumericUpDownTS();
            this.btnMMIO_variable_2_rotator = new System.Windows.Forms.ButtonTS();
            this.btnMMIO_variable_rotator = new System.Windows.Forms.ButtonTS();
            this.chkMeterItemRotatorShowBeamWidth = new System.Windows.Forms.CheckBoxTS();
            this.labelTS218 = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemRotatorBeamWidth = new PowerSDR.ColorButton();
            this.chkMeterItemDarkModeRotator = new System.Windows.Forms.CheckBoxTS();
            this.nudMeterItemUpdateRateRotator = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS225 = new System.Windows.Forms.LabelTS();
            this.labelTS227 = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemHBackgroundRotator = new PowerSDR.ColorButton();
            this.labelTS228 = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemRotatorSmallDot = new PowerSDR.ColorButton();
            this.labelTS229 = new System.Windows.Forms.LabelTS();
            this.labelTS230 = new System.Windows.Forms.LabelTS();
            this.clrbtnMeterItemRotatorLargeDot = new PowerSDR.ColorButton();
            this.clrbtnMeterItemRotatorArrow = new PowerSDR.ColorButton();
            this.chkMeterItemFadeOnTxRotator = new System.Windows.Forms.CheckBoxTS();
            this.chkMeterItemFadeOnRxRotator = new System.Windows.Forms.CheckBoxTS();
            this.pnlVariableInUse_1_rotator = new System.Windows.Forms.PanelTS();
            this.grpLedIndicator = new System.Windows.Forms.GroupBoxTS();
            this.radLed_light_pulsate = new System.Windows.Forms.RadioButtonTS();
            this.radLed_light_blink = new System.Windows.Forms.RadioButtonTS();
            this.radLed_light_on_off = new System.Windows.Forms.RadioButtonTS();
            this.chkLed_show_false = new System.Windows.Forms.CheckBoxTS();
            this.chkLed_show_true = new System.Windows.Forms.CheckBoxTS();
            this.lblLed_Error = new System.Windows.Forms.LabelTS();
            this.lblLed_Valid = new System.Windows.Forms.LabelTS();
            this.btnLedIndicator_copy_truefalse_colours = new System.Windows.Forms.ButtonTS();
            this.pbLedIndicator_condition_tips = new System.Windows.Forms.PictureBox();
            this.lblLedIndicator_panelbackgroundTX = new System.Windows.Forms.LabelTS();
            this.clrbtnLedIndicator_PanelBackgroundTX = new PowerSDR.ColorButton();
            this.labelTS219 = new System.Windows.Forms.LabelTS();
            this.labelTS220 = new System.Windows.Forms.LabelTS();
            this.clrbtnLedIndicator_false = new PowerSDR.ColorButton();
            this.clrbtnLedIndicator_true = new PowerSDR.ColorButton();
            this.btnLedIndicator_copy_sizex_to_y = new System.Windows.Forms.ButtonTS();
            this.labelTS221 = new System.Windows.Forms.LabelTS();
            this.labelTS222 = new System.Windows.Forms.LabelTS();
            this.nudLedIndicator_ySize = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS223 = new System.Windows.Forms.LabelTS();
            this.nudLedIndicator_xSize = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS224 = new System.Windows.Forms.LabelTS();
            this.labelTS226 = new System.Windows.Forms.LabelTS();
            this.nudLedIndicator_yOffset = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS231 = new System.Windows.Forms.LabelTS();
            this.nudLedIndicator_xOffset = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS233 = new System.Windows.Forms.LabelTS();
            this.txtLedIndicator_condition = new System.Windows.Forms.TextBoxTS();
            this.chkLedIndicator_ShowPanel = new System.Windows.Forms.CheckBoxTS();
            this.nudLedIndicator_PanelPadding = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS234 = new System.Windows.Forms.LabelTS();
            this.lblLedIndicator_panelbackground = new System.Windows.Forms.LabelTS();
            this.clrbtnLedIndicator_PanelBackground = new PowerSDR.ColorButton();
            this.chkLedIndicator_FadeOnTX = new System.Windows.Forms.CheckBoxTS();
            this.chkLedIndicator_FadeOnRX = new System.Windows.Forms.CheckBoxTS();
            this.grpWebImage = new System.Windows.Forms.GroupBoxTS();
            this.chkWebImage_bypass_cache = new System.Windows.Forms.CheckBoxTS();
            this.groupBoxTS42 = new System.Windows.Forms.GroupBoxTS();
            this.buttonTS1 = new System.Windows.Forms.ButtonTS();
            this.comboWebImage_noaa = new System.Windows.Forms.ComboBoxTS();
            this.groupBoxTS41 = new System.Windows.Forms.GroupBoxTS();
            this.btnWebImage_bsdworld_visit = new System.Windows.Forms.ButtonTS();
            this.comboWebImage_BsdWorld = new System.Windows.Forms.ComboBoxTS();
            this.groupBoxTS43 = new System.Windows.Forms.GroupBoxTS();
            this.buttonTS2 = new System.Windows.Forms.ButtonTS();
            this.comboWebImage_nasa = new System.Windows.Forms.ComboBoxTS();
            this.lblWebImage_state = new System.Windows.Forms.LabelTS();
            this.groupBoxTS40 = new System.Windows.Forms.GroupBoxTS();
            this.btnWebImage_hamqsl_donate = new System.Windows.Forms.ButtonTS();
            this.comboWebImage_HamQsl = new System.Windows.Forms.ComboBoxTS();
            this.labelTS236 = new System.Windows.Forms.LabelTS();
            this.txtWebImage_url = new System.Windows.Forms.TextBoxTS();
            this.nudWebImage_update_interval = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS232 = new System.Windows.Forms.LabelTS();
            this.nudWebImage_width_scale = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS235 = new System.Windows.Forms.LabelTS();
            this.chkWebImage_fade_tx = new System.Windows.Forms.CheckBoxTS();
            this.chkWebImage_fade_rx = new System.Windows.Forms.CheckBoxTS();
            this.grpBandButtons = new System.Windows.Forms.GroupBoxTS();
            this.pnlButtonBox_antenna_toggles = new System.Windows.Forms.PanelTS();
            this.chkButtonBox_antenna_rxtxant = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_xvtr = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_ext1 = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_byp = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_tx3 = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_tx2 = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_tx1 = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_rx3 = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_rx2 = new System.Windows.Forms.CheckBoxTS();
            this.chkButtonBox_antenna_rx1 = new System.Windows.Forms.CheckBoxTS();
            this.nudButtonBox_font_y_shift = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS250 = new System.Windows.Forms.LabelTS();
            this.nudButtonBox_font_x_shift = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS247 = new System.Windows.Forms.LabelTS();
            this.nudButtonBox_font_scale = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS248 = new System.Windows.Forms.LabelTS();
            this.lblBandButtons_indicator_style = new System.Windows.Forms.LabelTS();
            this.nudBandButtons_indicator_style = new System.Windows.Forms.NumericUpDownTS();
            this.chkBandButtons_band_inactive_use = new System.Windows.Forms.CheckBoxTS();
            this.labelTS246 = new System.Windows.Forms.LabelTS();
            this.clrbtnBandButtons_hover = new PowerSDR.ColorButton();
            this.labelTS245 = new System.Windows.Forms.LabelTS();
            this.clrbtnBandButtons_fill = new PowerSDR.ColorButton();
            this.labelTS244 = new System.Windows.Forms.LabelTS();
            this.clrbtnBandButtons_border = new PowerSDR.ColorButton();
            this.labelTS243 = new System.Windows.Forms.LabelTS();
            this.clrbtnBandButtons_indicator_off = new PowerSDR.ColorButton();
            this.labelTS242 = new System.Windows.Forms.LabelTS();
            this.nudBandButtons_indicator_border = new System.Windows.Forms.NumericUpDownTS();
            this.lblBandButtons_indicator_border = new System.Windows.Forms.LabelTS();
            this.nudBandButtons_height_ratio = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS241 = new System.Windows.Forms.LabelTS();
            this.nudBandButtons_radius = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS240 = new System.Windows.Forms.LabelTS();
            this.nudBandButtons_margin = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS239 = new System.Windows.Forms.LabelTS();
            this.nudBandButtons_border = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS238 = new System.Windows.Forms.LabelTS();
            this.btnBandButtons_font = new System.Windows.Forms.ButtonTS();
            this.chkBandButtons_use_indicator = new System.Windows.Forms.CheckBoxTS();
            this.nudBandButtons_columns = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS249 = new System.Windows.Forms.LabelTS();
            this.clrbtnBandButtons_indicator_on = new PowerSDR.ColorButton();
            this.chkBandButtons_fade_tx = new System.Windows.Forms.CheckBoxTS();
            this.chkBandButtons_fade_rx = new System.Windows.Forms.CheckBoxTS();
            this.grpHistoryItem = new System.Windows.Forms.GroupBoxTS();
            this.labelTS281 = new System.Windows.Forms.LabelTS();
            this.labelTS280 = new System.Windows.Forms.LabelTS();
            this.clrbtnHistory_time = new PowerSDR.ColorButton();
            this.clrbtnHistory_lines = new PowerSDR.ColorButton();
            this.groupBoxTS46 = new System.Windows.Forms.GroupBoxTS();
            this.clrbtnHistory_colour_1 = new PowerSDR.ColorButton();
            this.btnHistory_copy_minmax_from_0 = new System.Windows.Forms.ButtonTS();
            this.chkHistory_1_show_axis = new System.Windows.Forms.CheckBoxTS();
            this.nudHistory_axis1_max = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS263 = new System.Windows.Forms.LabelTS();
            this.nudHistory_axis1_min = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS264 = new System.Windows.Forms.LabelTS();
            this.chkHistory_auto_1_scale = new System.Windows.Forms.CheckBoxTS();
            this.comboHistory_reading_1 = new System.Windows.Forms.ComboBoxTS();
            this.labelTS265 = new System.Windows.Forms.LabelTS();
            this.groupBoxTS45 = new System.Windows.Forms.GroupBoxTS();
            this.clrbtnHistory_colour_0 = new PowerSDR.ColorButton();
            this.nudHistory_axis0_max = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS262 = new System.Windows.Forms.LabelTS();
            this.nudHistory_axis0_min = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS261 = new System.Windows.Forms.LabelTS();
            this.chkHistory_auto_0_scale = new System.Windows.Forms.CheckBoxTS();
            this.comboHistory_reading_0 = new System.Windows.Forms.ComboBoxTS();
            this.labelTS260 = new System.Windows.Forms.LabelTS();
            this.pnlVariableInUse_2_history = new System.Windows.Forms.PanelTS();
            this.btnMMIO_variable_2_history = new System.Windows.Forms.ButtonTS();
            this.btnMMIO_variable_history = new System.Windows.Forms.ButtonTS();
            this.pnlVariableInUse_1_history = new System.Windows.Forms.PanelTS();
            this.nudHistory_keep_for = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS259 = new System.Windows.Forms.LabelTS();
            this.nudHistory_update = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS252 = new System.Windows.Forms.LabelTS();
            this.nudHistory_vertical_ratio = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS253 = new System.Windows.Forms.LabelTS();
            this.labelTS258 = new System.Windows.Forms.LabelTS();
            this.clrbtnHistory_background = new PowerSDR.ColorButton();
            this.chkHistory_fade_tx = new System.Windows.Forms.CheckBoxTS();
            this.chkHistory_fade_rx = new System.Windows.Forms.CheckBoxTS();

            // 
            this.grpMultiMeterHolder.Controls.Add(this.chkLockContainer);
            this.grpMultiMeterHolder.Controls.Add(this.chkContainerShowTX);
            this.grpMultiMeterHolder.Controls.Add(this.chkMultiMeter_auto_container_height);
            this.grpMultiMeterHolder.Controls.Add(this.chkContainerMinimises);
            this.grpMultiMeterHolder.Controls.Add(this.lblMMContainerNotes);
            this.grpMultiMeterHolder.Controls.Add(this.txtContainerNotes);
            this.grpMultiMeterHolder.Controls.Add(this.chkContainerShowRX);
            this.grpMultiMeterHolder.Controls.Add(this.chkContainerNoTitle);
            this.grpMultiMeterHolder.Controls.Add(this.btnMeterCopySettings);
            this.grpMultiMeterHolder.Controls.Add(this.btnMeterPasteSettings);
            this.grpMultiMeterHolder.Controls.Add(this.lblMMContainerBackground);
            this.grpMultiMeterHolder.Controls.Add(this.clrbtnContainerBackground);
            this.grpMultiMeterHolder.Controls.Add(this.chkContainerBorder);
            this.grpMultiMeterHolder.Controls.Add(this.grpMeterItemSettings);
            this.grpMultiMeterHolder.Controls.Add(this.btnMeterUp);
            this.grpMultiMeterHolder.Controls.Add(this.btnMeterDown);
            this.grpMultiMeterHolder.Controls.Add(this.btnRemoveMeterItem);
            this.grpMultiMeterHolder.Controls.Add(this.btnAddMeterItem);
            this.grpMultiMeterHolder.Controls.Add(this.lstMetersInUse);
            this.grpMultiMeterHolder.Controls.Add(this.lstMetersAvailable);
            this.grpMultiMeterHolder.Controls.Add(this.chkContainerHighlight);
            this.grpMultiMeterHolder.Controls.Add(this.btnContainerDelete);
            this.grpMultiMeterHolder.Controls.Add(this.comboContainerSelect);
            this.grpMultiMeterHolder.Controls.Add(this.btnAddRX2Container);
            this.grpMultiMeterHolder.Controls.Add(this.btnAddRX1Container);
            this.grpMultiMeterHolder.Location = new System.Drawing.Point(8, 8);
            this.grpMultiMeterHolder.Name = "grpMultiMeterHolder";
            this.grpMultiMeterHolder.Size = new System.Drawing.Size(703, 395);
            this.grpMultiMeterHolder.TabIndex = 86;
            this.grpMultiMeterHolder.TabStop = false;
            // 
            this.chkLockContainer.AutoSize = true;
            this.chkLockContainer.Image = null;
            this.chkLockContainer.Location = new System.Drawing.Point(153, 40);
            this.chkLockContainer.Name = "chkLockContainer";
            this.chkLockContainer.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkLockContainer.Size = new System.Drawing.Size(50, 17);
            this.chkLockContainer.TabIndex = 111;
            this.chkLockContainer.Text = "Lock";
            this.toolTip1.SetToolTip(this.chkLockContainer, "Lock the container to prevent removal and to prevent add/remove of items. You can" +
        " still make adjustments to items");
            this.chkLockContainer.UseVisualStyleBackColor = true;
            this.chkLockContainer.CheckedChanged += new System.EventHandler(this.chkLockContainer_CheckedChanged);
            // 
            this.chkContainerShowTX.AutoSize = true;
            this.chkContainerShowTX.Image = null;
            this.chkContainerShowTX.Location = new System.Drawing.Point(89, 128);
            this.chkContainerShowTX.Name = "chkContainerShowTX";
            this.chkContainerShowTX.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkContainerShowTX.Size = new System.Drawing.Size(70, 17);
            this.chkContainerShowTX.TabIndex = 110;
            this.chkContainerShowTX.Text = "Show TX";
            this.toolTip1.SetToolTip(this.chkContainerShowTX, "Show the selected container on TX");
            this.chkContainerShowTX.UseVisualStyleBackColor = true;
            this.chkContainerShowTX.CheckedChanged += new System.EventHandler(this.chkContainerShowTX_CheckedChanged);
            // 
            this.chkMultiMeter_auto_container_height.AutoSize = true;
            this.chkMultiMeter_auto_container_height.Image = null;
            this.chkMultiMeter_auto_container_height.Location = new System.Drawing.Point(3, 106);
            this.chkMultiMeter_auto_container_height.Name = "chkMultiMeter_auto_container_height";
            this.chkMultiMeter_auto_container_height.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkMultiMeter_auto_container_height.Size = new System.Drawing.Size(80, 17);
            this.chkMultiMeter_auto_container_height.TabIndex = 109;
            this.chkMultiMeter_auto_container_height.Text = "Auto height";
            this.toolTip1.SetToolTip(this.chkMultiMeter_auto_container_height, "Automatically adjust height of container to fit");
            this.chkMultiMeter_auto_container_height.UseVisualStyleBackColor = true;
            this.chkMultiMeter_auto_container_height.CheckedChanged += new System.EventHandler(this.chkMultiMeter_auto_container_height_CheckedChanged);
            // 
            this.chkContainerMinimises.AutoSize = true;
            this.chkContainerMinimises.Image = null;
            this.chkContainerMinimises.Location = new System.Drawing.Point(302, 122);
            this.chkContainerMinimises.Name = "chkContainerMinimises";
            this.chkContainerMinimises.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkContainerMinimises.Size = new System.Drawing.Size(66, 17);
            this.chkContainerMinimises.TabIndex = 108;
            this.chkContainerMinimises.Text = "Minimise";
            this.toolTip1.SetToolTip(this.chkContainerMinimises, "Container will minimise if main window is minimised");
            this.chkContainerMinimises.UseVisualStyleBackColor = true;
            this.chkContainerMinimises.CheckedChanged += new System.EventHandler(this.chkContainerMinimises_CheckedChanged);
            // 
            this.lblMMContainerNotes.AutoSize = true;
            this.lblMMContainerNotes.Image = null;
            this.lblMMContainerNotes.Location = new System.Drawing.Point(163, 67);
            this.lblMMContainerNotes.Name = "lblMMContainerNotes";
            this.lblMMContainerNotes.Size = new System.Drawing.Size(38, 13);
            this.lblMMContainerNotes.TabIndex = 107;
            this.lblMMContainerNotes.Text = "Notes:";
            this.lblMMContainerNotes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.txtContainerNotes.Location = new System.Drawing.Point(166, 83);
            this.txtContainerNotes.MaxLength = 2048;
            this.txtContainerNotes.Multiline = true;
            this.txtContainerNotes.Name = "txtContainerNotes";
            this.txtContainerNotes.Size = new System.Drawing.Size(202, 33);
            this.txtContainerNotes.TabIndex = 106;
            this.toolTip1.SetToolTip(this.txtContainerNotes, "Somewhere to store notes about this container");
            this.txtContainerNotes.TextChanged += new System.EventHandler(this.txtContainerNotes_TextChanged);
            // 
            this.chkContainerShowRX.AutoSize = true;
            this.chkContainerShowRX.Image = null;
            this.chkContainerShowRX.Location = new System.Drawing.Point(89, 106);
            this.chkContainerShowRX.Name = "chkContainerShowRX";
            this.chkContainerShowRX.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkContainerShowRX.Size = new System.Drawing.Size(71, 17);
            this.chkContainerShowRX.TabIndex = 105;
            this.chkContainerShowRX.Text = "Show RX";
            this.toolTip1.SetToolTip(this.chkContainerShowRX, "Show the selected container on RX");
            this.chkContainerShowRX.UseVisualStyleBackColor = true;
            this.chkContainerShowRX.CheckedChanged += new System.EventHandler(this.chkContainerShowRX_CheckedChanged);
            // 
            this.chkContainerNoTitle.Image = null;
            this.chkContainerNoTitle.Location = new System.Drawing.Point(17, 85);
            this.chkContainerNoTitle.Name = "chkContainerNoTitle";
            this.chkContainerNoTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkContainerNoTitle.Size = new System.Drawing.Size(129, 17);
            this.chkContainerNoTitle.TabIndex = 104;
            this.chkContainerNoTitle.Text = "No container controls";
            this.toolTip1.SetToolTip(this.chkContainerNoTitle, "Prevents the display of the mouse over title bar and the resize grabber in the co" +
        "rner. Hold shift to bypass this.");
            this.chkContainerNoTitle.UseVisualStyleBackColor = true;
            this.chkContainerNoTitle.CheckedChanged += new System.EventHandler(this.chkContainerNoTitle_CheckedChanged);
            // 
            this.btnMeterCopySettings.Image = global::PowerSDR.P25MeterResources.pipette32border;
            this.btnMeterCopySettings.Location = new System.Drawing.Point(336, 293);
            this.btnMeterCopySettings.Name = "btnMeterCopySettings";

            this.btnMeterCopySettings.Size = new System.Drawing.Size(32, 32);
            this.btnMeterCopySettings.TabIndex = 103;
            this.toolTip1.SetToolTip(this.btnMeterCopySettings, "Copy settings and colours");
            this.btnMeterCopySettings.UseVisualStyleBackColor = true;
            this.btnMeterCopySettings.Click += new System.EventHandler(this.btnMeterCopySettings_Click);
            // 
            this.btnMeterPasteSettings.Image = global::PowerSDR.P25MeterResources.brush32border;
            this.btnMeterPasteSettings.Location = new System.Drawing.Point(336, 340);
            this.btnMeterPasteSettings.Name = "btnMeterPasteSettings";

            this.btnMeterPasteSettings.Size = new System.Drawing.Size(32, 32);
            this.btnMeterPasteSettings.TabIndex = 102;
            this.toolTip1.SetToolTip(this.btnMeterPasteSettings, "Paste settings and colours into suitable meter item");
            this.btnMeterPasteSettings.UseVisualStyleBackColor = true;
            this.btnMeterPasteSettings.Click += new System.EventHandler(this.btnMeterPasteSettings_Click);
            // 
            this.lblMMContainerBackground.AutoSize = true;
            this.lblMMContainerBackground.Image = null;
            this.lblMMContainerBackground.Location = new System.Drawing.Point(184, 123);
            this.lblMMContainerBackground.Name = "lblMMContainerBackground";
            this.lblMMContainerBackground.Size = new System.Drawing.Size(68, 13);
            this.lblMMContainerBackground.TabIndex = 99;
            this.lblMMContainerBackground.Text = "Background:";
            this.lblMMContainerBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.clrbtnContainerBackground.Automatic = "Automatic";
            this.clrbtnContainerBackground.Color = System.Drawing.Color.Black;
            this.clrbtnContainerBackground.Image = null;
            this.clrbtnContainerBackground.Location = new System.Drawing.Point(258, 118);
            this.clrbtnContainerBackground.MoreColors = "More Colors...";
            this.clrbtnContainerBackground.Name = "clrbtnContainerBackground";

            this.clrbtnContainerBackground.Size = new System.Drawing.Size(40, 23);
            this.clrbtnContainerBackground.TabIndex = 98;
            this.toolTip1.SetToolTip(this.clrbtnContainerBackground, "Container Background Colour");
            this.clrbtnContainerBackground.Changed += new System.EventHandler(this.clrbtnContainerBackground_Changed);
            // 
            this.chkContainerBorder.AutoSize = true;
            this.chkContainerBorder.Image = null;
            this.chkContainerBorder.Location = new System.Drawing.Point(89, 63);
            this.chkContainerBorder.Name = "chkContainerBorder";
            this.chkContainerBorder.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkContainerBorder.Size = new System.Drawing.Size(57, 17);
            this.chkContainerBorder.TabIndex = 97;
            this.chkContainerBorder.Text = "Border";
            this.toolTip1.SetToolTip(this.chkContainerBorder, "Container has a border");
            this.chkContainerBorder.UseVisualStyleBackColor = true;
            this.chkContainerBorder.CheckedChanged += new System.EventHandler(this.chkContainerBorder_CheckedChanged);
            // 
            this.grpMeterItemSettings.Controls.Add(this.pnlVariableInUse_2);
            this.grpMeterItemSettings.Controls.Add(this.btnMMIO_variable_2);
            this.grpMeterItemSettings.Controls.Add(this.btnMMIO_variable);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemIgnoreHistoryDuration);
            this.grpMeterItemSettings.Controls.Add(this.lblMMHistoryIgnore);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemPowerScale);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemEyeBezelScale);
            this.grpMeterItemSettings.Controls.Add(this.lblMMEyeBezelSize);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemShowSubIndicator);
            this.grpMeterItemSettings.Controls.Add(this.lblMMIndicatorSub);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemSubIndicator);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemShowIndicator);
            this.grpMeterItemSettings.Controls.Add(this.lblMMsegSolHigh);
            this.grpMeterItemSettings.Controls.Add(this.lblMMsegSolLow);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemSegmentedSolidColourHigh);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemSolid);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemsPowerLimit);
            this.grpMeterItemSettings.Controls.Add(this.lblMMPowerLimit);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemDarkMode);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemSignalAverage);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemEyeScale);
            this.grpMeterItemSettings.Controls.Add(this.lblMMEyeSize);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemMeterTitle);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemPeakValueColour);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemSegmentedSolidColourLow);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemDecayRate);
            this.grpMeterItemSettings.Controls.Add(this.labelTS169);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemAttackRate);
            this.grpMeterItemSettings.Controls.Add(this.labelTS168);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemUpdateRate);
            this.grpMeterItemSettings.Controls.Add(this.labelTS167);
            this.grpMeterItemSettings.Controls.Add(this.tbMeterItemHistoryAlpha);
            this.grpMeterItemSettings.Controls.Add(this.nudMeterItemHistoryDuration);
            this.grpMeterItemSettings.Controls.Add(this.lblMMHistory);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemPeakValue);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemTitle);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemSegmented);
            this.grpMeterItemSettings.Controls.Add(this.lblMMBackground);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemHBackground);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemHistory);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemPeakHold);
            this.grpMeterItemSettings.Controls.Add(this.lblMMIndicator);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemIndicator);
            this.grpMeterItemSettings.Controls.Add(this.lblMMHigh);
            this.grpMeterItemSettings.Controls.Add(this.lblMMLow);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemHigh);
            this.grpMeterItemSettings.Controls.Add(this.clrbtnMeterItemLow);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemShadow);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemPeakHold);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemHistory);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemFadeOnTx);
            this.grpMeterItemSettings.Controls.Add(this.chkMeterItemFadeOnRx);
            this.grpMeterItemSettings.Controls.Add(this.pnlVariableInUse_1);
            this.grpMeterItemSettings.Location = new System.Drawing.Point(374, 15);
            this.grpMeterItemSettings.Name = "grpMeterItemSettings";
            this.grpMeterItemSettings.Size = new System.Drawing.Size(323, 376);
            this.grpMeterItemSettings.TabIndex = 96;
            this.grpMeterItemSettings.TabStop = false;
            this.grpMeterItemSettings.Text = "Settings";
            this.grpMeterItemSettings.Visible = false;
            // 
            this.pnlVariableInUse_2.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_2.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_2.BackColor = System.Drawing.Color.Lime;
            this.pnlVariableInUse_2.Location = new System.Drawing.Point(277, 97);
            this.pnlVariableInUse_2.Name = "pnlVariableInUse_2";
            this.pnlVariableInUse_2.Size = new System.Drawing.Size(28, 6);
            this.pnlVariableInUse_2.TabIndex = 131;
            // 
            this.btnMMIO_variable_2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMMIO_variable_2.Image = null;
            this.btnMMIO_variable_2.Location = new System.Drawing.Point(277, 70);
            this.btnMMIO_variable_2.Name = "btnMMIO_variable_2";

            this.btnMMIO_variable_2.Size = new System.Drawing.Size(28, 28);
            this.btnMMIO_variable_2.TabIndex = 129;
            this.btnMMIO_variable_2.Text = "%";
            this.btnMMIO_variable_2.UseVisualStyleBackColor = true;
            this.btnMMIO_variable_2.Click += new System.EventHandler(this.btnMMIO_variable_2_Click);
            // 
            this.btnMMIO_variable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMMIO_variable.Image = null;
            this.btnMMIO_variable.Location = new System.Drawing.Point(245, 70);
            this.btnMMIO_variable.Name = "btnMMIO_variable";

            this.btnMMIO_variable.Size = new System.Drawing.Size(28, 28);
            this.btnMMIO_variable.TabIndex = 128;
            this.btnMMIO_variable.Text = "%";
            this.btnMMIO_variable.UseVisualStyleBackColor = true;
            this.btnMMIO_variable.Click += new System.EventHandler(this.btnMMIO_variable_Click);
            // 
            this.nudMeterItemIgnoreHistoryDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemIgnoreHistoryDuration.Location = new System.Drawing.Point(247, 170);
            this.nudMeterItemIgnoreHistoryDuration.Maximum = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.nudMeterItemIgnoreHistoryDuration.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMeterItemIgnoreHistoryDuration.Name = "nudMeterItemIgnoreHistoryDuration";
            this.nudMeterItemIgnoreHistoryDuration.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemIgnoreHistoryDuration.TabIndex = 127;
            this.nudMeterItemIgnoreHistoryDuration.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemIgnoreHistoryDuration, "When rx/tx transition or band change let meters settle before gathering history/p" +
        "eak values");
            this.nudMeterItemIgnoreHistoryDuration.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudMeterItemIgnoreHistoryDuration.ValueChanged += new System.EventHandler(this.nudMeterItemIgnoreHistoryDuration_ValueChanged);
            // 
            this.lblMMHistoryIgnore.AutoSize = true;
            this.lblMMHistoryIgnore.Image = null;
            this.lblMMHistoryIgnore.Location = new System.Drawing.Point(114, 172);
            this.lblMMHistoryIgnore.Name = "lblMMHistoryIgnore";
            this.lblMMHistoryIgnore.Size = new System.Drawing.Size(127, 13);
            this.lblMMHistoryIgnore.TabIndex = 126;
            this.lblMMHistoryIgnore.Text = "Ignore History/Peak (ms):";
            this.lblMMHistoryIgnore.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.clrbtnMeterItemPowerScale.Automatic = "Automatic";
            this.clrbtnMeterItemPowerScale.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemPowerScale.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemPowerScale.Image = null;
            this.clrbtnMeterItemPowerScale.Location = new System.Drawing.Point(263, 333);
            this.clrbtnMeterItemPowerScale.MoreColors = "More Colors...";
            this.clrbtnMeterItemPowerScale.Name = "clrbtnMeterItemPowerScale";

            this.clrbtnMeterItemPowerScale.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemPowerScale.TabIndex = 125;
            this.clrbtnMeterItemPowerScale.Changed += new System.EventHandler(this.clrbtnMeterItemPowerScale_Changed);
            // 
            this.nudMeterItemEyeBezelScale.DecimalPlaces = 2;
            this.nudMeterItemEyeBezelScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMeterItemEyeBezelScale.Location = new System.Drawing.Point(83, 334);
            this.nudMeterItemEyeBezelScale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemEyeBezelScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMeterItemEyeBezelScale.Name = "nudMeterItemEyeBezelScale";
            this.nudMeterItemEyeBezelScale.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemEyeBezelScale.TabIndex = 124;
            this.nudMeterItemEyeBezelScale.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemEyeBezelScale, "Size of the eye bezel, 1.0 is full width of container");
            this.nudMeterItemEyeBezelScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemEyeBezelScale.ValueChanged += new System.EventHandler(this.nudMeterItemEyeBezelScale_ValueChanged);
            // 
            this.lblMMEyeBezelSize.AutoSize = true;
            this.lblMMEyeBezelSize.Image = null;
            this.lblMMEyeBezelSize.Location = new System.Drawing.Point(15, 335);
            this.lblMMEyeBezelSize.Name = "lblMMEyeBezelSize";
            this.lblMMEyeBezelSize.Size = new System.Drawing.Size(59, 13);
            this.lblMMEyeBezelSize.TabIndex = 123;
            this.lblMMEyeBezelSize.Text = "Bezel Size:";
            // 
            this.chkMeterItemShowSubIndicator.AutoSize = true;
            this.chkMeterItemShowSubIndicator.Image = null;
            this.chkMeterItemShowSubIndicator.Location = new System.Drawing.Point(270, 108);
            this.chkMeterItemShowSubIndicator.Name = "chkMeterItemShowSubIndicator";
            this.chkMeterItemShowSubIndicator.Size = new System.Drawing.Size(53, 17);
            this.chkMeterItemShowSubIndicator.TabIndex = 122;
            this.chkMeterItemShowSubIndicator.Text = "Show";
            this.toolTip1.SetToolTip(this.chkMeterItemShowSubIndicator, "Show sub indicators");
            this.chkMeterItemShowSubIndicator.UseVisualStyleBackColor = true;
            this.chkMeterItemShowSubIndicator.CheckedChanged += new System.EventHandler(this.chkMeterItemShowSubIndicator_CheckedChanged);
            // 
            this.lblMMIndicatorSub.AutoSize = true;
            this.lblMMIndicatorSub.Image = null;
            this.lblMMIndicatorSub.Location = new System.Drawing.Point(176, 109);
            this.lblMMIndicatorSub.Name = "lblMMIndicatorSub";
            this.lblMMIndicatorSub.Size = new System.Drawing.Size(40, 13);
            this.lblMMIndicatorSub.TabIndex = 121;
            this.lblMMIndicatorSub.Text = "Sub(s):";
            // 
            this.clrbtnMeterItemSubIndicator.Automatic = "Automatic";
            this.clrbtnMeterItemSubIndicator.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemSubIndicator.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemSubIndicator.Image = null;
            this.clrbtnMeterItemSubIndicator.Location = new System.Drawing.Point(215, 104);
            this.clrbtnMeterItemSubIndicator.MoreColors = "More Colors...";
            this.clrbtnMeterItemSubIndicator.Name = "clrbtnMeterItemSubIndicator";

            this.clrbtnMeterItemSubIndicator.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemSubIndicator.TabIndex = 120;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemSubIndicator, "Sub Indicator colour for sub needles and avg markers on some horizontal meters");
            this.clrbtnMeterItemSubIndicator.Changed += new System.EventHandler(this.clrbtnMeterItemSubIndicator_Changed);
            // 
            this.chkMeterItemShowIndicator.AutoSize = true;
            this.chkMeterItemShowIndicator.Image = null;
            this.chkMeterItemShowIndicator.Location = new System.Drawing.Point(128, 108);
            this.chkMeterItemShowIndicator.Name = "chkMeterItemShowIndicator";
            this.chkMeterItemShowIndicator.Size = new System.Drawing.Size(53, 17);
            this.chkMeterItemShowIndicator.TabIndex = 119;
            this.chkMeterItemShowIndicator.Text = "Show";
            this.toolTip1.SetToolTip(this.chkMeterItemShowIndicator, "Show the indicator line on horizontal bar meters");
            this.chkMeterItemShowIndicator.UseVisualStyleBackColor = true;
            this.chkMeterItemShowIndicator.CheckedChanged += new System.EventHandler(this.chkMeterItemShowIndicator_CheckedChanged);
            // 
            this.lblMMsegSolHigh.AutoSize = true;
            this.lblMMsegSolHigh.Image = null;
            this.lblMMsegSolHigh.Location = new System.Drawing.Point(153, 197);
            this.lblMMsegSolHigh.Name = "lblMMsegSolHigh";
            this.lblMMsegSolHigh.Size = new System.Drawing.Size(27, 13);
            this.lblMMsegSolHigh.TabIndex = 118;
            this.lblMMsegSolHigh.Text = "high";
            // 
            this.lblMMsegSolLow.AutoSize = true;
            this.lblMMsegSolLow.Image = null;
            this.lblMMsegSolLow.Location = new System.Drawing.Point(108, 197);
            this.lblMMsegSolLow.Name = "lblMMsegSolLow";
            this.lblMMsegSolLow.Size = new System.Drawing.Size(23, 13);
            this.lblMMsegSolLow.TabIndex = 117;
            this.lblMMsegSolLow.Text = "low";
            // 
            this.clrbtnMeterItemSegmentedSolidColourHigh.Automatic = "Automatic";
            this.clrbtnMeterItemSegmentedSolidColourHigh.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemSegmentedSolidColourHigh.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemSegmentedSolidColourHigh.Image = null;
            this.clrbtnMeterItemSegmentedSolidColourHigh.Location = new System.Drawing.Point(145, 213);
            this.clrbtnMeterItemSegmentedSolidColourHigh.MoreColors = "More Colors...";
            this.clrbtnMeterItemSegmentedSolidColourHigh.Name = "clrbtnMeterItemSegmentedSolidColourHigh";

            this.clrbtnMeterItemSegmentedSolidColourHigh.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemSegmentedSolidColourHigh.TabIndex = 116;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemSegmentedSolidColourHigh, "High section colour");
            this.clrbtnMeterItemSegmentedSolidColourHigh.Changed += new System.EventHandler(this.clrbtnMeterItemSegmentedSolidColourHigh_Changed);
            // 
            this.chkMeterItemSolid.AutoSize = true;
            this.chkMeterItemSolid.Image = null;
            this.chkMeterItemSolid.Location = new System.Drawing.Point(18, 240);
            this.chkMeterItemSolid.Name = "chkMeterItemSolid";
            this.chkMeterItemSolid.Size = new System.Drawing.Size(49, 17);
            this.chkMeterItemSolid.TabIndex = 115;
            this.chkMeterItemSolid.Text = "Solid";
            this.toolTip1.SetToolTip(this.chkMeterItemSolid, "Solid bar");
            this.chkMeterItemSolid.UseVisualStyleBackColor = true;
            this.chkMeterItemSolid.CheckedChanged += new System.EventHandler(this.chkMeterItemSolid_CheckedChanged);
            // 
            this.nudMeterItemsPowerLimit.DecimalPlaces = 1;
            this.nudMeterItemsPowerLimit.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudMeterItemsPowerLimit.Location = new System.Drawing.Point(207, 334);
            this.nudMeterItemsPowerLimit.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudMeterItemsPowerLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudMeterItemsPowerLimit.Name = "nudMeterItemsPowerLimit";
            this.nudMeterItemsPowerLimit.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemsPowerLimit.TabIndex = 114;
            this.nudMeterItemsPowerLimit.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemsPowerLimit, "Power limit of scale");
            this.nudMeterItemsPowerLimit.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMeterItemsPowerLimit.ValueChanged += new System.EventHandler(this.nudMeterItemsPowerLimit_ValueChanged);
            // 
            this.lblMMPowerLimit.Image = null;
            this.lblMMPowerLimit.Location = new System.Drawing.Point(157, 336);
            this.lblMMPowerLimit.Name = "lblMMPowerLimit";
            this.lblMMPowerLimit.Size = new System.Drawing.Size(44, 16);
            this.lblMMPowerLimit.TabIndex = 113;
            this.lblMMPowerLimit.Text = "Power:";
            this.lblMMPowerLimit.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.chkMeterItemDarkMode.AutoSize = true;
            this.chkMeterItemDarkMode.Image = null;
            this.chkMeterItemDarkMode.Location = new System.Drawing.Point(197, 313);
            this.chkMeterItemDarkMode.Name = "chkMeterItemDarkMode";
            this.chkMeterItemDarkMode.Size = new System.Drawing.Size(79, 17);
            this.chkMeterItemDarkMode.TabIndex = 112;
            this.chkMeterItemDarkMode.Text = "Dark Mode";
            this.chkMeterItemDarkMode.UseVisualStyleBackColor = true;
            this.chkMeterItemDarkMode.CheckedChanged += new System.EventHandler(this.chkMeterItemDarkMode_CheckedChanged);
            // 
            this.chkMeterItemSignalAverage.AutoSize = true;
            this.chkMeterItemSignalAverage.Image = null;
            this.chkMeterItemSignalAverage.Location = new System.Drawing.Point(197, 286);
            this.chkMeterItemSignalAverage.Name = "chkMeterItemSignalAverage";
            this.chkMeterItemSignalAverage.Size = new System.Drawing.Size(98, 17);
            this.chkMeterItemSignalAverage.TabIndex = 111;
            this.chkMeterItemSignalAverage.Text = "Signal Average";
            this.toolTip1.SetToolTip(this.chkMeterItemSignalAverage, "Use sig average instead of sig");
            this.chkMeterItemSignalAverage.UseVisualStyleBackColor = true;
            this.chkMeterItemSignalAverage.CheckedChanged += new System.EventHandler(this.chkMeterItemSignalAverage_CheckedChanged);
            // 
            this.nudMeterItemEyeScale.DecimalPlaces = 2;
            this.nudMeterItemEyeScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMeterItemEyeScale.Location = new System.Drawing.Point(83, 312);
            this.nudMeterItemEyeScale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemEyeScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMeterItemEyeScale.Name = "nudMeterItemEyeScale";
            this.nudMeterItemEyeScale.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemEyeScale.TabIndex = 110;
            this.nudMeterItemEyeScale.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemEyeScale, "Size of the eye, 1.0 is full width of container");
            this.nudMeterItemEyeScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemEyeScale.ValueChanged += new System.EventHandler(this.nudMeterItemEyeScale_ValueChanged);
            // 
            this.lblMMEyeSize.AutoSize = true;
            this.lblMMEyeSize.Image = null;
            this.lblMMEyeSize.Location = new System.Drawing.Point(15, 314);
            this.lblMMEyeSize.Name = "lblMMEyeSize";
            this.lblMMEyeSize.Size = new System.Drawing.Size(51, 13);
            this.lblMMEyeSize.TabIndex = 109;
            this.lblMMEyeSize.Text = "Eye Size:";
            // 
            this.clrbtnMeterItemMeterTitle.Automatic = "Automatic";
            this.clrbtnMeterItemMeterTitle.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemMeterTitle.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemMeterTitle.Image = null;
            this.clrbtnMeterItemMeterTitle.Location = new System.Drawing.Point(99, 259);
            this.clrbtnMeterItemMeterTitle.MoreColors = "More Colors...";
            this.clrbtnMeterItemMeterTitle.Name = "clrbtnMeterItemMeterTitle";

            this.clrbtnMeterItemMeterTitle.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemMeterTitle.TabIndex = 108;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemMeterTitle, "Meter title colour");
            this.clrbtnMeterItemMeterTitle.Changed += new System.EventHandler(this.clrbtnMeterItemMeterTitle_Changed);
            // 
            this.clrbtnMeterItemPeakValueColour.Automatic = "Automatic";
            this.clrbtnMeterItemPeakValueColour.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemPeakValueColour.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemPeakValueColour.Image = null;
            this.clrbtnMeterItemPeakValueColour.Location = new System.Drawing.Point(99, 282);
            this.clrbtnMeterItemPeakValueColour.MoreColors = "More Colors...";
            this.clrbtnMeterItemPeakValueColour.Name = "clrbtnMeterItemPeakValueColour";

            this.clrbtnMeterItemPeakValueColour.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemPeakValueColour.TabIndex = 107;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemPeakValueColour, "Peak value colour");
            this.clrbtnMeterItemPeakValueColour.Changed += new System.EventHandler(this.clrbtnMeterItemPeakValueColour_Changed);
            // 
            this.clrbtnMeterItemSegmentedSolidColourLow.Automatic = "Automatic";
            this.clrbtnMeterItemSegmentedSolidColourLow.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemSegmentedSolidColourLow.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemSegmentedSolidColourLow.Image = null;
            this.clrbtnMeterItemSegmentedSolidColourLow.Location = new System.Drawing.Point(99, 213);
            this.clrbtnMeterItemSegmentedSolidColourLow.MoreColors = "More Colors...";
            this.clrbtnMeterItemSegmentedSolidColourLow.Name = "clrbtnMeterItemSegmentedSolidColourLow";

            this.clrbtnMeterItemSegmentedSolidColourLow.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemSegmentedSolidColourLow.TabIndex = 106;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemSegmentedSolidColourLow, "Low section colour");
            this.clrbtnMeterItemSegmentedSolidColourLow.Changed += new System.EventHandler(this.clrbtnMeterItemSegmentedSolidColourLow_Changed);
            // 
            this.nudMeterItemDecayRate.DecimalPlaces = 2;
            this.nudMeterItemDecayRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.nudMeterItemDecayRate.Location = new System.Drawing.Point(247, 45);
            this.nudMeterItemDecayRate.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemDecayRate.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMeterItemDecayRate.Name = "nudMeterItemDecayRate";
            this.nudMeterItemDecayRate.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemDecayRate.TabIndex = 105;
            this.nudMeterItemDecayRate.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemDecayRate, "The \'speed of fall\' to the new value if below current");
            this.nudMeterItemDecayRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemDecayRate.ValueChanged += new System.EventHandler(this.nudMeterItemDecayRate_ValueChanged);
            // 
            this.labelTS169.Image = null;
            this.labelTS169.Location = new System.Drawing.Point(184, 47);
            this.labelTS169.Name = "labelTS169";
            this.labelTS169.Size = new System.Drawing.Size(50, 16);
            this.labelTS169.TabIndex = 104;
            this.labelTS169.Text = "Decay:";
            // 
            this.nudMeterItemAttackRate.DecimalPlaces = 2;
            this.nudMeterItemAttackRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.nudMeterItemAttackRate.Location = new System.Drawing.Point(83, 45);
            this.nudMeterItemAttackRate.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemAttackRate.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMeterItemAttackRate.Name = "nudMeterItemAttackRate";
            this.nudMeterItemAttackRate.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemAttackRate.TabIndex = 103;
            this.nudMeterItemAttackRate.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemAttackRate, "The \'speed of rise\' to the new value if above current");
            this.nudMeterItemAttackRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemAttackRate.ValueChanged += new System.EventHandler(this.nudMeterItemAttackRate_ValueChanged);
            // 
            this.labelTS168.Image = null;
            this.labelTS168.Location = new System.Drawing.Point(34, 47);
            this.labelTS168.Name = "labelTS168";
            this.labelTS168.Size = new System.Drawing.Size(43, 16);
            this.labelTS168.TabIndex = 102;
            this.labelTS168.Text = "Attack:";
            this.labelTS168.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.nudMeterItemUpdateRate.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemUpdateRate.Location = new System.Drawing.Point(83, 20);
            this.nudMeterItemUpdateRate.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudMeterItemUpdateRate.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudMeterItemUpdateRate.Name = "nudMeterItemUpdateRate";
            this.nudMeterItemUpdateRate.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemUpdateRate.TabIndex = 101;
            this.nudMeterItemUpdateRate.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemUpdateRate, "Reading update and is related to screen update");
            this.nudMeterItemUpdateRate.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMeterItemUpdateRate.ValueChanged += new System.EventHandler(this.nudMeterItemUpdateRate_ValueChanged);
            // 
            this.labelTS167.Image = null;
            this.labelTS167.Location = new System.Drawing.Point(6, 20);
            this.labelTS167.Name = "labelTS167";
            this.labelTS167.Size = new System.Drawing.Size(71, 16);
            this.labelTS167.TabIndex = 100;
            this.labelTS167.Text = "Update (ms):";
            this.labelTS167.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.tbMeterItemHistoryAlpha.AutoSize = false;
            this.tbMeterItemHistoryAlpha.Location = new System.Drawing.Point(254, 213);
            this.tbMeterItemHistoryAlpha.Maximum = 255;
            this.tbMeterItemHistoryAlpha.Name = "tbMeterItemHistoryAlpha";
            this.tbMeterItemHistoryAlpha.Size = new System.Drawing.Size(66, 18);
            this.tbMeterItemHistoryAlpha.TabIndex = 99;
            this.tbMeterItemHistoryAlpha.TickFrequency = 64;
            this.tbMeterItemHistoryAlpha.Value = 255;
            this.tbMeterItemHistoryAlpha.Scroll += new System.EventHandler(this.tbMeterItemHistoryAlpha_Scroll);
            // 
            this.nudMeterItemHistoryDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemHistoryDuration.Location = new System.Drawing.Point(247, 147);
            this.nudMeterItemHistoryDuration.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudMeterItemHistoryDuration.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudMeterItemHistoryDuration.Name = "nudMeterItemHistoryDuration";
            this.nudMeterItemHistoryDuration.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemHistoryDuration.TabIndex = 98;
            this.nudMeterItemHistoryDuration.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemHistoryDuration, "History duration for history display and peak hold");
            this.nudMeterItemHistoryDuration.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudMeterItemHistoryDuration.ValueChanged += new System.EventHandler(this.nudMeterItemHistoryDuration_ValueChanged);
            // 
            this.lblMMHistory.AutoSize = true;
            this.lblMMHistory.Image = null;
            this.lblMMHistory.Location = new System.Drawing.Point(177, 149);
            this.lblMMHistory.Name = "lblMMHistory";
            this.lblMMHistory.Size = new System.Drawing.Size(64, 13);
            this.lblMMHistory.TabIndex = 97;
            this.lblMMHistory.Text = "History (ms):";
            // 
            this.chkMeterItemPeakValue.AutoSize = true;
            this.chkMeterItemPeakValue.Image = null;
            this.chkMeterItemPeakValue.Location = new System.Drawing.Point(18, 286);
            this.chkMeterItemPeakValue.Name = "chkMeterItemPeakValue";
            this.chkMeterItemPeakValue.Size = new System.Drawing.Size(81, 17);
            this.chkMeterItemPeakValue.TabIndex = 96;
            this.chkMeterItemPeakValue.Text = "Peak Value";
            this.toolTip1.SetToolTip(this.chkMeterItemPeakValue, "Show peak value");
            this.chkMeterItemPeakValue.UseVisualStyleBackColor = true;
            this.chkMeterItemPeakValue.CheckedChanged += new System.EventHandler(this.chkMeterItemPeakValue_CheckedChanged);
            // 
            this.chkMeterItemTitle.AutoSize = true;
            this.chkMeterItemTitle.Image = null;
            this.chkMeterItemTitle.Location = new System.Drawing.Point(18, 263);
            this.chkMeterItemTitle.Name = "chkMeterItemTitle";
            this.chkMeterItemTitle.Size = new System.Drawing.Size(76, 17);
            this.chkMeterItemTitle.TabIndex = 95;
            this.chkMeterItemTitle.Text = "Meter Title";
            this.toolTip1.SetToolTip(this.chkMeterItemTitle, "Show meter title");
            this.chkMeterItemTitle.UseVisualStyleBackColor = true;
            this.chkMeterItemTitle.CheckedChanged += new System.EventHandler(this.chkMeterItemTitle_CheckedChanged);
            // 
            this.chkMeterItemSegmented.AutoSize = true;
            this.chkMeterItemSegmented.Image = null;
            this.chkMeterItemSegmented.Location = new System.Drawing.Point(18, 217);
            this.chkMeterItemSegmented.Name = "chkMeterItemSegmented";
            this.chkMeterItemSegmented.Size = new System.Drawing.Size(80, 17);
            this.chkMeterItemSegmented.TabIndex = 94;
            this.chkMeterItemSegmented.Text = "Segmented";
            this.toolTip1.SetToolTip(this.chkMeterItemSegmented, "Segmented bar");
            this.chkMeterItemSegmented.UseVisualStyleBackColor = true;
            this.chkMeterItemSegmented.CheckedChanged += new System.EventHandler(this.chkMeterItemSegmented_CheckedChanged);
            // 
            this.lblMMBackground.AutoSize = true;
            this.lblMMBackground.Image = null;
            this.lblMMBackground.Location = new System.Drawing.Point(184, 22);
            this.lblMMBackground.Name = "lblMMBackground";
            this.lblMMBackground.Size = new System.Drawing.Size(68, 13);
            this.lblMMBackground.TabIndex = 93;
            this.lblMMBackground.Text = "Background:";
            // 
            this.clrbtnMeterItemHBackground.Automatic = "Automatic";
            this.clrbtnMeterItemHBackground.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnMeterItemHBackground.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemHBackground.Image = null;
            this.clrbtnMeterItemHBackground.Location = new System.Drawing.Point(263, 17);
            this.clrbtnMeterItemHBackground.MoreColors = "More Colors...";
            this.clrbtnMeterItemHBackground.Name = "clrbtnMeterItemHBackground";

            this.clrbtnMeterItemHBackground.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemHBackground.TabIndex = 92;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemHBackground, "Background colour");
            this.clrbtnMeterItemHBackground.Changed += new System.EventHandler(this.clrbtnMeterItemHBackground_Changed);
            // 
            this.clrbtnMeterItemHistory.Automatic = "Automatic";
            this.clrbtnMeterItemHistory.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemHistory.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemHistory.Image = null;
            this.clrbtnMeterItemHistory.Location = new System.Drawing.Point(215, 213);
            this.clrbtnMeterItemHistory.MoreColors = "More Colors...";
            this.clrbtnMeterItemHistory.Name = "clrbtnMeterItemHistory";

            this.clrbtnMeterItemHistory.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemHistory.TabIndex = 83;
            this.clrbtnMeterItemHistory.Changed += new System.EventHandler(this.clrbtnMeterItemHistory_Changed);
            // 
            this.clrbtnMeterItemPeakHold.Automatic = "Automatic";
            this.clrbtnMeterItemPeakHold.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemPeakHold.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemPeakHold.Image = null;
            this.clrbtnMeterItemPeakHold.Location = new System.Drawing.Point(215, 259);
            this.clrbtnMeterItemPeakHold.MoreColors = "More Colors...";
            this.clrbtnMeterItemPeakHold.Name = "clrbtnMeterItemPeakHold";

            this.clrbtnMeterItemPeakHold.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemPeakHold.TabIndex = 82;
            this.clrbtnMeterItemPeakHold.Changed += new System.EventHandler(this.clrbtnMeterItemPeakHold_Changed);
            // 
            this.lblMMIndicator.AutoSize = true;
            this.lblMMIndicator.Image = null;
            this.lblMMIndicator.Location = new System.Drawing.Point(15, 109);
            this.lblMMIndicator.Name = "lblMMIndicator";
            this.lblMMIndicator.Size = new System.Drawing.Size(51, 13);
            this.lblMMIndicator.TabIndex = 81;
            this.lblMMIndicator.Text = "Indicator:";
            // 
            this.clrbtnMeterItemIndicator.Automatic = "Automatic";
            this.clrbtnMeterItemIndicator.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemIndicator.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemIndicator.Image = null;
            this.clrbtnMeterItemIndicator.Location = new System.Drawing.Point(83, 104);
            this.clrbtnMeterItemIndicator.MoreColors = "More Colors...";
            this.clrbtnMeterItemIndicator.Name = "clrbtnMeterItemIndicator";

            this.clrbtnMeterItemIndicator.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemIndicator.TabIndex = 80;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemIndicator, "Indicator colour");
            this.clrbtnMeterItemIndicator.Changed += new System.EventHandler(this.clrbtnMeterItemIndicator_Changed);
            // 
            this.lblMMHigh.AutoSize = true;
            this.lblMMHigh.Image = null;
            this.lblMMHigh.Location = new System.Drawing.Point(142, 75);
            this.lblMMHigh.Name = "lblMMHigh";
            this.lblMMHigh.Size = new System.Drawing.Size(32, 13);
            this.lblMMHigh.TabIndex = 79;
            this.lblMMHigh.Text = "High:";
            // 
            this.lblMMLow.AutoSize = true;
            this.lblMMLow.Image = null;
            this.lblMMLow.Location = new System.Drawing.Point(47, 75);
            this.lblMMLow.Name = "lblMMLow";
            this.lblMMLow.Size = new System.Drawing.Size(30, 13);
            this.lblMMLow.TabIndex = 78;
            this.lblMMLow.Text = "Low:";
            this.lblMMLow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.clrbtnMeterItemHigh.Automatic = "Automatic";
            this.clrbtnMeterItemHigh.Color = System.Drawing.Color.Red;
            this.clrbtnMeterItemHigh.Image = null;
            this.clrbtnMeterItemHigh.Location = new System.Drawing.Point(176, 70);
            this.clrbtnMeterItemHigh.MoreColors = "More Colors...";
            this.clrbtnMeterItemHigh.Name = "clrbtnMeterItemHigh";

            this.clrbtnMeterItemHigh.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemHigh.TabIndex = 77;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemHigh, "High scale colour");
            this.clrbtnMeterItemHigh.Changed += new System.EventHandler(this.clrbtnMeterItemHigh_Changed);
            // 
            this.clrbtnMeterItemLow.Automatic = "Automatic";
            this.clrbtnMeterItemLow.Color = System.Drawing.Color.White;
            this.clrbtnMeterItemLow.Image = null;
            this.clrbtnMeterItemLow.Location = new System.Drawing.Point(83, 70);
            this.clrbtnMeterItemLow.MoreColors = "More Colors...";
            this.clrbtnMeterItemLow.Name = "clrbtnMeterItemLow";

            this.clrbtnMeterItemLow.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemLow.TabIndex = 76;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemLow, "Low scale colour and value");
            this.clrbtnMeterItemLow.Changed += new System.EventHandler(this.clrbtnMeterItemLow_Changed);
            // 
            this.chkMeterItemShadow.AutoSize = true;
            this.chkMeterItemShadow.Image = null;
            this.chkMeterItemShadow.Location = new System.Drawing.Point(18, 194);
            this.chkMeterItemShadow.Name = "chkMeterItemShadow";
            this.chkMeterItemShadow.Size = new System.Drawing.Size(65, 17);
            this.chkMeterItemShadow.TabIndex = 4;
            this.chkMeterItemShadow.Text = "Shadow";
            this.toolTip1.SetToolTip(this.chkMeterItemShadow, "Show shadow on needles");
            this.chkMeterItemShadow.UseVisualStyleBackColor = true;
            this.chkMeterItemShadow.CheckedChanged += new System.EventHandler(this.chkMeterItemShadow_CheckedChanged);
            // 
            this.chkMeterItemPeakHold.AutoSize = true;
            this.chkMeterItemPeakHold.Image = null;
            this.chkMeterItemPeakHold.Location = new System.Drawing.Point(197, 240);
            this.chkMeterItemPeakHold.Name = "chkMeterItemPeakHold";
            this.chkMeterItemPeakHold.Size = new System.Drawing.Size(106, 17);
            this.chkMeterItemPeakHold.TabIndex = 3;
            this.chkMeterItemPeakHold.Text = "Show Peak Hold";
            this.chkMeterItemPeakHold.UseVisualStyleBackColor = true;
            this.chkMeterItemPeakHold.CheckedChanged += new System.EventHandler(this.chkMeterItemPeakHold_CheckedChanged);
            // 
            this.chkMeterItemHistory.AutoSize = true;
            this.chkMeterItemHistory.Image = null;
            this.chkMeterItemHistory.Location = new System.Drawing.Point(197, 194);
            this.chkMeterItemHistory.Name = "chkMeterItemHistory";
            this.chkMeterItemHistory.Size = new System.Drawing.Size(88, 17);
            this.chkMeterItemHistory.TabIndex = 2;
            this.chkMeterItemHistory.Text = "Show History";
            this.chkMeterItemHistory.UseVisualStyleBackColor = true;
            this.chkMeterItemHistory.CheckedChanged += new System.EventHandler(this.chkMeterItemHistory_CheckedChanged);
            // 
            this.chkMeterItemFadeOnTx.AutoSize = true;
            this.chkMeterItemFadeOnTx.Image = null;
            this.chkMeterItemFadeOnTx.Location = new System.Drawing.Point(18, 171);
            this.chkMeterItemFadeOnTx.Name = "chkMeterItemFadeOnTx";
            this.chkMeterItemFadeOnTx.Size = new System.Drawing.Size(82, 17);
            this.chkMeterItemFadeOnTx.TabIndex = 1;
            this.chkMeterItemFadeOnTx.Text = "Fade on TX";
            this.chkMeterItemFadeOnTx.UseVisualStyleBackColor = true;
            this.chkMeterItemFadeOnTx.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnTx_CheckedChanged);
            // 
            this.chkMeterItemFadeOnRx.AutoSize = true;
            this.chkMeterItemFadeOnRx.Image = null;
            this.chkMeterItemFadeOnRx.Location = new System.Drawing.Point(18, 148);
            this.chkMeterItemFadeOnRx.Name = "chkMeterItemFadeOnRx";
            this.chkMeterItemFadeOnRx.Size = new System.Drawing.Size(83, 17);
            this.chkMeterItemFadeOnRx.TabIndex = 0;
            this.chkMeterItemFadeOnRx.Text = "Fade on RX";
            this.chkMeterItemFadeOnRx.UseVisualStyleBackColor = true;
            this.chkMeterItemFadeOnRx.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnRx_CheckedChanged);
            // 
            this.pnlVariableInUse_1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_1.BackColor = System.Drawing.Color.Lime;
            this.pnlVariableInUse_1.Location = new System.Drawing.Point(245, 97);
            this.pnlVariableInUse_1.Name = "pnlVariableInUse_1";
            this.pnlVariableInUse_1.Size = new System.Drawing.Size(28, 6);
            this.pnlVariableInUse_1.TabIndex = 130;
            // 
            this.btnMeterUp.Image = global::PowerSDR.P25MeterResources.arrow_up_black;
            this.btnMeterUp.Location = new System.Drawing.Point(337, 147);
            this.btnMeterUp.Name = "btnMeterUp";

            this.btnMeterUp.Size = new System.Drawing.Size(32, 32);
            this.btnMeterUp.TabIndex = 95;
            this.toolTip1.SetToolTip(this.btnMeterUp, "Move item up");
            this.btnMeterUp.UseVisualStyleBackColor = true;
            this.btnMeterUp.Click += new System.EventHandler(this.btnMeterUp_Click);
            // 
            this.btnMeterDown.Image = global::PowerSDR.P25MeterResources.down_black;
            this.btnMeterDown.Location = new System.Drawing.Point(337, 194);
            this.btnMeterDown.Name = "btnMeterDown";

            this.btnMeterDown.Size = new System.Drawing.Size(32, 32);
            this.btnMeterDown.TabIndex = 94;
            this.toolTip1.SetToolTip(this.btnMeterDown, "Move item down");
            this.btnMeterDown.UseVisualStyleBackColor = true;
            this.btnMeterDown.Click += new System.EventHandler(this.btnMeterDown_Click);
            // 
            this.btnRemoveMeterItem.Image = global::PowerSDR.P25MeterResources.arrow_left_black;
            this.btnRemoveMeterItem.Location = new System.Drawing.Point(153, 194);
            this.btnRemoveMeterItem.Name = "btnRemoveMeterItem";

            this.btnRemoveMeterItem.Size = new System.Drawing.Size(32, 32);
            this.btnRemoveMeterItem.TabIndex = 93;
            this.toolTip1.SetToolTip(this.btnRemoveMeterItem, "Remove the item");
            this.btnRemoveMeterItem.UseVisualStyleBackColor = true;
            this.btnRemoveMeterItem.Click += new System.EventHandler(this.btnRemoveMeterItem_Click);
            // 
            this.btnAddMeterItem.Image = global::PowerSDR.P25MeterResources.arrow_right_black;
            this.btnAddMeterItem.Location = new System.Drawing.Point(153, 147);
            this.btnAddMeterItem.Name = "btnAddMeterItem";

            this.btnAddMeterItem.Size = new System.Drawing.Size(32, 32);
            this.btnAddMeterItem.TabIndex = 92;
            this.toolTip1.SetToolTip(this.btnAddMeterItem, "Include the item");
            this.btnAddMeterItem.UseVisualStyleBackColor = true;
            this.btnAddMeterItem.Click += new System.EventHandler(this.btnAddMeterItem_Click);
            // 
            this.lstMetersInUse.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstMetersInUse.FormattingEnabled = true;
            this.lstMetersInUse.Location = new System.Drawing.Point(191, 148);
            this.lstMetersInUse.Name = "lstMetersInUse";
            this.lstMetersInUse.Size = new System.Drawing.Size(140, 238);
            this.lstMetersInUse.TabIndex = 91;
            this.lstMetersInUse.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstMetersInUse_DrawItem);
            this.lstMetersInUse.SelectedIndexChanged += new System.EventHandler(this.lstMetersInUse_SelectedIndexChanged);
            this.lstMetersInUse.DoubleClick += new System.EventHandler(this.lstMetersInUse_DoubleClick);
            // 
            this.lstMetersAvailable.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstMetersAvailable.FormattingEnabled = true;
            this.lstMetersAvailable.Location = new System.Drawing.Point(7, 148);
            this.lstMetersAvailable.Name = "lstMetersAvailable";
            this.lstMetersAvailable.Size = new System.Drawing.Size(140, 238);
            this.lstMetersAvailable.TabIndex = 90;
            this.lstMetersAvailable.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstMetersAvailable_DrawItem);
            this.lstMetersAvailable.SelectedIndexChanged += new System.EventHandler(this.lstMetersAvailable_SelectedIndexChanged);
            this.lstMetersAvailable.DoubleClick += new System.EventHandler(this.lstMetersAvailable_DoubleClick);
            // 
            this.chkContainerHighlight.AutoSize = true;
            this.chkContainerHighlight.Image = null;
            this.chkContainerHighlight.Location = new System.Drawing.Point(79, 40);
            this.chkContainerHighlight.Name = "chkContainerHighlight";
            this.chkContainerHighlight.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkContainerHighlight.Size = new System.Drawing.Size(67, 17);
            this.chkContainerHighlight.TabIndex = 89;
            this.chkContainerHighlight.Text = "Highlight";
            this.toolTip1.SetToolTip(this.chkContainerHighlight, "Highlight selected container");
            this.chkContainerHighlight.UseVisualStyleBackColor = true;
            this.chkContainerHighlight.CheckedChanged += new System.EventHandler(this.chkContainerHighlight_CheckedChanged);
            // 
            this.btnContainerDelete.Image = null;
            this.btnContainerDelete.Location = new System.Drawing.Point(6, 40);
            this.btnContainerDelete.Name = "btnContainerDelete";

            this.btnContainerDelete.Size = new System.Drawing.Size(59, 32);
            this.btnContainerDelete.TabIndex = 88;
            this.btnContainerDelete.Text = "Remove";
            this.toolTip1.SetToolTip(this.btnContainerDelete, "Removes the selected container and all meter items contained within");
            this.btnContainerDelete.UseVisualStyleBackColor = true;
            this.btnContainerDelete.Click += new System.EventHandler(this.btnContainerDelete_Click);
            // 
            this.comboContainerSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboContainerSelect.FormattingEnabled = true;
            this.comboContainerSelect.Location = new System.Drawing.Point(6, 13);
            this.comboContainerSelect.Name = "comboContainerSelect";
            this.comboContainerSelect.Size = new System.Drawing.Size(193, 21);
            this.comboContainerSelect.TabIndex = 87;
            this.toolTip1.SetToolTip(this.comboContainerSelect, "Selected container. Note: each will get own id in the title text when undocked");
            this.comboContainerSelect.SelectedIndexChanged += new System.EventHandler(this.comboContainerSelect_SelectedIndexChanged);
            // 
            this.btnAddRX2Container.Image = null;
            this.btnAddRX2Container.Location = new System.Drawing.Point(298, 21);
            this.btnAddRX2Container.Name = "btnAddRX2Container";

            this.btnAddRX2Container.Size = new System.Drawing.Size(70, 56);
            this.btnAddRX2Container.TabIndex = 1;
            this.btnAddRX2Container.Text = "Add TRX2 Container";
            this.toolTip1.SetToolTip(this.btnAddRX2Container, "Add a meter item container that uses RX2 readings");
            this.btnAddRX2Container.UseVisualStyleBackColor = true;
            this.btnAddRX2Container.Click += new System.EventHandler(this.btnAddRX2Container_Click);
            // 
            this.btnAddRX1Container.Image = null;
            this.btnAddRX1Container.Location = new System.Drawing.Point(225, 21);
            this.btnAddRX1Container.Name = "btnAddRX1Container";

            this.btnAddRX1Container.Size = new System.Drawing.Size(70, 56);
            this.btnAddRX1Container.TabIndex = 0;
            this.btnAddRX1Container.Text = "Add TRX1 Container";
            this.toolTip1.SetToolTip(this.btnAddRX1Container, "Add a meter item container that uses RX1 readings");
            this.btnAddRX1Container.UseVisualStyleBackColor = true;
            this.btnAddRX1Container.Click += new System.EventHandler(this.btnAddRX1Container_Click);
            // 
            this.grpMeterItemClockSettings.Controls.Add(this.lblMMClockBackground);
            this.grpMeterItemClockSettings.Controls.Add(this.clrbtnMMClockBackground);
            this.grpMeterItemClockSettings.Controls.Add(this.labelTS164);
            this.grpMeterItemClockSettings.Controls.Add(this.clrbtnMMDate);
            this.grpMeterItemClockSettings.Controls.Add(this.labelTS162);
            this.grpMeterItemClockSettings.Controls.Add(this.clrbtnMMTime);
            this.grpMeterItemClockSettings.Controls.Add(this.clrbtnMMClockTitle);
            this.grpMeterItemClockSettings.Controls.Add(this.chkMMClockTitle);
            this.grpMeterItemClockSettings.Controls.Add(this.radMM24Clock);
            this.grpMeterItemClockSettings.Controls.Add(this.radMM12Clock);
            this.grpMeterItemClockSettings.Location = new System.Drawing.Point(12, 14);
            this.grpMeterItemClockSettings.Name = "grpMeterItemClockSettings";
            this.grpMeterItemClockSettings.Size = new System.Drawing.Size(323, 376);
            this.grpMeterItemClockSettings.TabIndex = 100;
            this.grpMeterItemClockSettings.TabStop = false;
            this.grpMeterItemClockSettings.Text = "Clock Settings";
            this.grpMeterItemClockSettings.Visible = false;
            // 
            this.lblMMClockBackground.AutoSize = true;
            this.lblMMClockBackground.Image = null;
            this.lblMMClockBackground.Location = new System.Drawing.Point(32, 58);
            this.lblMMClockBackground.Name = "lblMMClockBackground";
            this.lblMMClockBackground.Size = new System.Drawing.Size(68, 13);
            this.lblMMClockBackground.TabIndex = 116;
            this.lblMMClockBackground.Text = "Background:";
            // 
            this.clrbtnMMClockBackground.Automatic = "Automatic";
            this.clrbtnMMClockBackground.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnMMClockBackground.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMClockBackground.Image = null;
            this.clrbtnMMClockBackground.Location = new System.Drawing.Point(106, 53);
            this.clrbtnMMClockBackground.MoreColors = "More Colors...";
            this.clrbtnMMClockBackground.Name = "clrbtnMMClockBackground";

            this.clrbtnMMClockBackground.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMClockBackground.TabIndex = 115;
            this.toolTip1.SetToolTip(this.clrbtnMMClockBackground, "Background colour");
            this.clrbtnMMClockBackground.Changed += new System.EventHandler(this.clrbtnMMClockBackground_Changed);
            // 
            this.labelTS164.AutoSize = true;
            this.labelTS164.Image = null;
            this.labelTS164.Location = new System.Drawing.Point(66, 147);
            this.labelTS164.Name = "labelTS164";
            this.labelTS164.Size = new System.Drawing.Size(33, 13);
            this.labelTS164.TabIndex = 114;
            this.labelTS164.Text = "Date:";
            // 
            this.clrbtnMMDate.Automatic = "Automatic";
            this.clrbtnMMDate.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMDate.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMDate.Image = null;
            this.clrbtnMMDate.Location = new System.Drawing.Point(105, 142);
            this.clrbtnMMDate.MoreColors = "More Colors...";
            this.clrbtnMMDate.Name = "clrbtnMMDate";

            this.clrbtnMMDate.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMDate.TabIndex = 113;
            this.toolTip1.SetToolTip(this.clrbtnMMDate, "Date colour");
            this.clrbtnMMDate.Changed += new System.EventHandler(this.clrbtnMMDate_Changed);
            // 
            this.labelTS162.AutoSize = true;
            this.labelTS162.Image = null;
            this.labelTS162.Location = new System.Drawing.Point(66, 123);
            this.labelTS162.Name = "labelTS162";
            this.labelTS162.Size = new System.Drawing.Size(33, 13);
            this.labelTS162.TabIndex = 112;
            this.labelTS162.Text = "Time:";
            // 
            this.clrbtnMMTime.Automatic = "Automatic";
            this.clrbtnMMTime.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMTime.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMTime.Image = null;
            this.clrbtnMMTime.Location = new System.Drawing.Point(105, 118);
            this.clrbtnMMTime.MoreColors = "More Colors...";
            this.clrbtnMMTime.Name = "clrbtnMMTime";

            this.clrbtnMMTime.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMTime.TabIndex = 111;
            this.toolTip1.SetToolTip(this.clrbtnMMTime, "Time");
            this.clrbtnMMTime.Changed += new System.EventHandler(this.clrbtnMMTime_Changed);
            // 
            this.clrbtnMMClockTitle.Automatic = "Automatic";
            this.clrbtnMMClockTitle.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMClockTitle.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMClockTitle.Image = null;
            this.clrbtnMMClockTitle.Location = new System.Drawing.Point(105, 86);
            this.clrbtnMMClockTitle.MoreColors = "More Colors...";
            this.clrbtnMMClockTitle.Name = "clrbtnMMClockTitle";

            this.clrbtnMMClockTitle.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMClockTitle.TabIndex = 110;
            this.toolTip1.SetToolTip(this.clrbtnMMClockTitle, "Meter title colour");
            this.clrbtnMMClockTitle.Changed += new System.EventHandler(this.clrbtnMMClockTitle_Changed);
            // 
            this.chkMMClockTitle.AutoSize = true;
            this.chkMMClockTitle.Image = null;
            this.chkMMClockTitle.Location = new System.Drawing.Point(24, 89);
            this.chkMMClockTitle.Name = "chkMMClockTitle";
            this.chkMMClockTitle.Size = new System.Drawing.Size(76, 17);
            this.chkMMClockTitle.TabIndex = 109;
            this.chkMMClockTitle.Text = "Meter Title";
            this.toolTip1.SetToolTip(this.chkMMClockTitle, "Show meter title");
            this.chkMMClockTitle.UseVisualStyleBackColor = true;
            this.chkMMClockTitle.CheckedChanged += new System.EventHandler(this.chkMMClockTitle_CheckedChanged);
            // 
            this.radMM24Clock.AutoSize = true;
            this.radMM24Clock.Image = null;
            this.radMM24Clock.Location = new System.Drawing.Point(192, 67);
            this.radMM24Clock.Name = "radMM24Clock";
            this.radMM24Clock.Size = new System.Drawing.Size(49, 17);
            this.radMM24Clock.TabIndex = 1;
            this.radMM24Clock.TabStop = true;
            this.radMM24Clock.Text = "24 hr";
            this.toolTip1.SetToolTip(this.radMM24Clock, "24 hr clock");
            this.radMM24Clock.UseVisualStyleBackColor = true;
            this.radMM24Clock.CheckedChanged += new System.EventHandler(this.radMM24Clock_CheckedChanged);
            // 
            this.radMM12Clock.AutoSize = true;
            this.radMM12Clock.Image = null;
            this.radMM12Clock.Location = new System.Drawing.Point(192, 48);
            this.radMM12Clock.Name = "radMM12Clock";
            this.radMM12Clock.Size = new System.Drawing.Size(49, 17);
            this.radMM12Clock.TabIndex = 0;
            this.radMM12Clock.TabStop = true;
            this.radMM12Clock.Text = "12 hr";
            this.toolTip1.SetToolTip(this.radMM12Clock, "12 hr clock");
            this.radMM12Clock.UseVisualStyleBackColor = true;
            this.radMM12Clock.CheckedChanged += new System.EventHandler(this.radMM12Clock_CheckedChanged);
            // 
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS278);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMultiMeter_vfo_sync);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS279);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMultiMeter_vfo_lock);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.btnVFOCopyColourFromMainNumbers);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS251);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayFrequency_small);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMultiMeter_vfo_show_bandtext);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.chkMultiMeter_vfo_show_bandtext);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.radMultiMeter_vfo_display_vfob);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.radMultiMeter_vfo_display_vfoa);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.radMultiMeter_vfo_display_both);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS216);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDigitHighlight);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS177);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS176);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayBackground);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS175);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayFrequency);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS174);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayBand);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS173);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayFilter);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS172);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayTx);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS171);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayRx);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS170);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplaySplit);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS163);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplaySplitBack);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.labelTS166);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayMode);
            this.grpMeterItemVfoDisplaySettings.Controls.Add(this.clrbtnMMVfoDisplayTitle);
            this.grpMeterItemVfoDisplaySettings.Location = new System.Drawing.Point(15, 24);
            this.grpMeterItemVfoDisplaySettings.Name = "grpMeterItemVfoDisplaySettings";
            this.grpMeterItemVfoDisplaySettings.Size = new System.Drawing.Size(323, 376);
            this.grpMeterItemVfoDisplaySettings.TabIndex = 101;
            this.grpMeterItemVfoDisplaySettings.TabStop = false;
            this.grpMeterItemVfoDisplaySettings.Text = "VFO Display Settings";
            this.grpMeterItemVfoDisplaySettings.Visible = false;
            // 
            this.labelTS278.AutoSize = true;
            this.labelTS278.Image = null;
            this.labelTS278.Location = new System.Drawing.Point(163, 287);
            this.labelTS278.Name = "labelTS278";
            this.labelTS278.Size = new System.Drawing.Size(34, 13);
            this.labelTS278.TabIndex = 143;
            this.labelTS278.Text = "Sync:";
            // 
            this.clrbtnMultiMeter_vfo_sync.Automatic = "Automatic";
            this.clrbtnMultiMeter_vfo_sync.Color = System.Drawing.Color.Yellow;
            this.clrbtnMultiMeter_vfo_sync.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMultiMeter_vfo_sync.Image = null;
            this.clrbtnMultiMeter_vfo_sync.Location = new System.Drawing.Point(203, 282);
            this.clrbtnMultiMeter_vfo_sync.MoreColors = "More Colors...";
            this.clrbtnMultiMeter_vfo_sync.Name = "clrbtnMultiMeter_vfo_sync";

            this.clrbtnMultiMeter_vfo_sync.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMultiMeter_vfo_sync.TabIndex = 142;
            this.toolTip1.SetToolTip(this.clrbtnMultiMeter_vfo_sync, "Filter colour");
            this.clrbtnMultiMeter_vfo_sync.Changed += new System.EventHandler(this.clrbtnMultiMeter_vfo_sync_Changed);
            // 
            this.labelTS279.AutoSize = true;
            this.labelTS279.Image = null;
            this.labelTS279.Location = new System.Drawing.Point(163, 258);
            this.labelTS279.Name = "labelTS279";
            this.labelTS279.Size = new System.Drawing.Size(34, 13);
            this.labelTS279.TabIndex = 141;
            this.labelTS279.Text = "Lock:";
            // 
            this.clrbtnMultiMeter_vfo_lock.Automatic = "Automatic";
            this.clrbtnMultiMeter_vfo_lock.Color = System.Drawing.Color.Yellow;
            this.clrbtnMultiMeter_vfo_lock.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMultiMeter_vfo_lock.Image = null;
            this.clrbtnMultiMeter_vfo_lock.Location = new System.Drawing.Point(203, 253);
            this.clrbtnMultiMeter_vfo_lock.MoreColors = "More Colors...";
            this.clrbtnMultiMeter_vfo_lock.Name = "clrbtnMultiMeter_vfo_lock";

            this.clrbtnMultiMeter_vfo_lock.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMultiMeter_vfo_lock.TabIndex = 140;
            this.toolTip1.SetToolTip(this.clrbtnMultiMeter_vfo_lock, "TX box colour");
            this.clrbtnMultiMeter_vfo_lock.Changed += new System.EventHandler(this.clrbtnMultiMeter_vfo_lock_Changed);
            // 
            this.btnVFOCopyColourFromMainNumbers.Image = null;
            this.btnVFOCopyColourFromMainNumbers.Location = new System.Drawing.Point(274, 88);
            this.btnVFOCopyColourFromMainNumbers.Name = "btnVFOCopyColourFromMainNumbers";

            this.btnVFOCopyColourFromMainNumbers.Size = new System.Drawing.Size(25, 23);
            this.btnVFOCopyColourFromMainNumbers.TabIndex = 139;
            this.btnVFOCopyColourFromMainNumbers.Text = "=";
            this.toolTip1.SetToolTip(this.btnVFOCopyColourFromMainNumbers, "Copy the colour from the main #\'s");
            this.btnVFOCopyColourFromMainNumbers.UseVisualStyleBackColor = true;
            this.btnVFOCopyColourFromMainNumbers.Click += new System.EventHandler(this.btnVFOCopyColourFromMainNumbers_Click);
            // 
            this.labelTS251.AutoSize = true;
            this.labelTS251.Image = null;
            this.labelTS251.Location = new System.Drawing.Point(170, 93);
            this.labelTS251.Name = "labelTS251";
            this.labelTS251.Size = new System.Drawing.Size(52, 13);
            this.labelTS251.TabIndex = 138;
            this.labelTS251.Text = "Small #\'s:";
            // 
            this.clrbtnMMVfoDisplayFrequency_small.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayFrequency_small.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayFrequency_small.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayFrequency_small.Image = null;
            this.clrbtnMMVfoDisplayFrequency_small.Location = new System.Drawing.Point(228, 88);
            this.clrbtnMMVfoDisplayFrequency_small.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayFrequency_small.Name = "clrbtnMMVfoDisplayFrequency_small";

            this.clrbtnMMVfoDisplayFrequency_small.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayFrequency_small.TabIndex = 137;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayFrequency_small, "Frequency Colour");
            this.clrbtnMMVfoDisplayFrequency_small.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayFrequency_small_Changed);
            // 
            this.clrbtnMultiMeter_vfo_show_bandtext.Automatic = "Automatic";
            this.clrbtnMultiMeter_vfo_show_bandtext.Color = System.Drawing.Color.Yellow;
            this.clrbtnMultiMeter_vfo_show_bandtext.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMultiMeter_vfo_show_bandtext.Image = null;
            this.clrbtnMultiMeter_vfo_show_bandtext.Location = new System.Drawing.Point(203, 203);
            this.clrbtnMultiMeter_vfo_show_bandtext.MoreColors = "More Colors...";
            this.clrbtnMultiMeter_vfo_show_bandtext.Name = "clrbtnMultiMeter_vfo_show_bandtext";

            this.clrbtnMultiMeter_vfo_show_bandtext.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMultiMeter_vfo_show_bandtext.TabIndex = 136;
            this.toolTip1.SetToolTip(this.clrbtnMultiMeter_vfo_show_bandtext, "Split colour");
            this.clrbtnMultiMeter_vfo_show_bandtext.Changed += new System.EventHandler(this.clrbtnMultiMeter_vfo_show_bandtext_Changed);
            // 
            this.chkMultiMeter_vfo_show_bandtext.AutoSize = true;
            this.chkMultiMeter_vfo_show_bandtext.Image = null;
            this.chkMultiMeter_vfo_show_bandtext.Location = new System.Drawing.Point(184, 183);
            this.chkMultiMeter_vfo_show_bandtext.Name = "chkMultiMeter_vfo_show_bandtext";
            this.chkMultiMeter_vfo_show_bandtext.Size = new System.Drawing.Size(105, 17);
            this.chkMultiMeter_vfo_show_bandtext.TabIndex = 135;
            this.chkMultiMeter_vfo_show_bandtext.Text = "Show Band Text";
            this.chkMultiMeter_vfo_show_bandtext.UseVisualStyleBackColor = true;
            this.chkMultiMeter_vfo_show_bandtext.CheckedChanged += new System.EventHandler(this.chkMultiMeter_vfo_show_bandtext_CheckedChanged);
            // 
            this.radMultiMeter_vfo_display_vfob.AutoSize = true;
            this.radMultiMeter_vfo_display_vfob.Image = null;
            this.radMultiMeter_vfo_display_vfob.Location = new System.Drawing.Point(220, 323);
            this.radMultiMeter_vfo_display_vfob.Name = "radMultiMeter_vfo_display_vfob";
            this.radMultiMeter_vfo_display_vfob.Size = new System.Drawing.Size(56, 17);
            this.radMultiMeter_vfo_display_vfob.TabIndex = 134;
            this.radMultiMeter_vfo_display_vfob.TabStop = true;
            this.radMultiMeter_vfo_display_vfob.Text = "VFO B";
            this.radMultiMeter_vfo_display_vfob.UseVisualStyleBackColor = true;
            this.radMultiMeter_vfo_display_vfob.CheckedChanged += new System.EventHandler(this.radMultiMeter_vfo_display_vfob_CheckedChanged);
            // 
            this.radMultiMeter_vfo_display_vfoa.AutoSize = true;
            this.radMultiMeter_vfo_display_vfoa.Image = null;
            this.radMultiMeter_vfo_display_vfoa.Location = new System.Drawing.Point(158, 323);
            this.radMultiMeter_vfo_display_vfoa.Name = "radMultiMeter_vfo_display_vfoa";
            this.radMultiMeter_vfo_display_vfoa.Size = new System.Drawing.Size(56, 17);
            this.radMultiMeter_vfo_display_vfoa.TabIndex = 133;
            this.radMultiMeter_vfo_display_vfoa.TabStop = true;
            this.radMultiMeter_vfo_display_vfoa.Text = "VFO A";
            this.radMultiMeter_vfo_display_vfoa.UseVisualStyleBackColor = true;
            this.radMultiMeter_vfo_display_vfoa.CheckedChanged += new System.EventHandler(this.radMultiMeter_vfo_display_vfoa_CheckedChanged);
            // 
            this.radMultiMeter_vfo_display_both.AutoSize = true;
            this.radMultiMeter_vfo_display_both.Image = null;
            this.radMultiMeter_vfo_display_both.Location = new System.Drawing.Point(105, 323);
            this.radMultiMeter_vfo_display_both.Name = "radMultiMeter_vfo_display_both";
            this.radMultiMeter_vfo_display_both.Size = new System.Drawing.Size(47, 17);
            this.radMultiMeter_vfo_display_both.TabIndex = 132;
            this.radMultiMeter_vfo_display_both.TabStop = true;
            this.radMultiMeter_vfo_display_both.Text = "Both";
            this.radMultiMeter_vfo_display_both.UseVisualStyleBackColor = true;
            this.radMultiMeter_vfo_display_both.CheckedChanged += new System.EventHandler(this.radMultiMeter_vfo_display_both_CheckedChanged);
            // 
            this.labelTS216.AutoSize = true;
            this.labelTS216.Image = null;
            this.labelTS216.Location = new System.Drawing.Point(9, 293);
            this.labelTS216.Name = "labelTS216";
            this.labelTS216.Size = new System.Drawing.Size(67, 13);
            this.labelTS216.TabIndex = 131;
            this.labelTS216.Text = "VfoHighlight:";
            // 
            this.clrbtnMMVfoDigitHighlight.Automatic = "Automatic";
            this.clrbtnMMVfoDigitHighlight.Color = System.Drawing.SystemColors.ControlLight;
            this.clrbtnMMVfoDigitHighlight.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDigitHighlight.Image = null;
            this.clrbtnMMVfoDigitHighlight.Location = new System.Drawing.Point(105, 288);
            this.clrbtnMMVfoDigitHighlight.MoreColors = "More Colors...";
            this.clrbtnMMVfoDigitHighlight.Name = "clrbtnMMVfoDigitHighlight";

            this.clrbtnMMVfoDigitHighlight.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDigitHighlight.TabIndex = 130;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDigitHighlight, "Any highlights from the mouse will be in this colour");
            this.clrbtnMMVfoDigitHighlight.Changed += new System.EventHandler(this.clrbtnMMVfoDigitHighlight_Changed);
            // 
            this.labelTS177.AutoSize = true;
            this.labelTS177.Image = null;
            this.labelTS177.Location = new System.Drawing.Point(65, 64);
            this.labelTS177.Name = "labelTS177";
            this.labelTS177.Size = new System.Drawing.Size(32, 13);
            this.labelTS177.TabIndex = 129;
            this.labelTS177.Text = "Titles";
            // 
            this.labelTS176.AutoSize = true;
            this.labelTS176.Image = null;
            this.labelTS176.Location = new System.Drawing.Point(31, 35);
            this.labelTS176.Name = "labelTS176";
            this.labelTS176.Size = new System.Drawing.Size(68, 13);
            this.labelTS176.TabIndex = 128;
            this.labelTS176.Text = "Background:";
            // 
            this.clrbtnMMVfoDisplayBackground.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayBackground.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnMMVfoDisplayBackground.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayBackground.Image = null;
            this.clrbtnMMVfoDisplayBackground.Location = new System.Drawing.Point(105, 30);
            this.clrbtnMMVfoDisplayBackground.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayBackground.Name = "clrbtnMMVfoDisplayBackground";

            this.clrbtnMMVfoDisplayBackground.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayBackground.TabIndex = 127;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayBackground, "Background colour");
            this.clrbtnMMVfoDisplayBackground.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayBackground_Changed);
            // 
            this.labelTS175.AutoSize = true;
            this.labelTS175.Image = null;
            this.labelTS175.Location = new System.Drawing.Point(40, 93);
            this.labelTS175.Name = "labelTS175";
            this.labelTS175.Size = new System.Drawing.Size(60, 13);
            this.labelTS175.TabIndex = 126;
            this.labelTS175.Text = "Frequency:";
            // 
            this.clrbtnMMVfoDisplayFrequency.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayFrequency.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayFrequency.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayFrequency.Image = null;
            this.clrbtnMMVfoDisplayFrequency.Location = new System.Drawing.Point(105, 88);
            this.clrbtnMMVfoDisplayFrequency.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayFrequency.Name = "clrbtnMMVfoDisplayFrequency";

            this.clrbtnMMVfoDisplayFrequency.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayFrequency.TabIndex = 125;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayFrequency, "Frequency Colour");
            this.clrbtnMMVfoDisplayFrequency.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayFrequency_Changed);
            // 
            this.labelTS174.AutoSize = true;
            this.labelTS174.Image = null;
            this.labelTS174.Location = new System.Drawing.Point(69, 264);
            this.labelTS174.Name = "labelTS174";
            this.labelTS174.Size = new System.Drawing.Size(35, 13);
            this.labelTS174.TabIndex = 124;
            this.labelTS174.Text = "Band:";
            // 
            this.clrbtnMMVfoDisplayBand.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayBand.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayBand.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayBand.Image = null;
            this.clrbtnMMVfoDisplayBand.Location = new System.Drawing.Point(105, 259);
            this.clrbtnMMVfoDisplayBand.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayBand.Name = "clrbtnMMVfoDisplayBand";

            this.clrbtnMMVfoDisplayBand.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayBand.TabIndex = 123;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayBand, "Band colour");
            this.clrbtnMMVfoDisplayBand.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayBand_Changed);
            // 
            this.labelTS173.AutoSize = true;
            this.labelTS173.Image = null;
            this.labelTS173.Location = new System.Drawing.Point(69, 235);
            this.labelTS173.Name = "labelTS173";
            this.labelTS173.Size = new System.Drawing.Size(32, 13);
            this.labelTS173.TabIndex = 122;
            this.labelTS173.Text = "Filter:";
            // 
            this.clrbtnMMVfoDisplayFilter.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayFilter.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayFilter.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayFilter.Image = null;
            this.clrbtnMMVfoDisplayFilter.Location = new System.Drawing.Point(105, 230);
            this.clrbtnMMVfoDisplayFilter.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayFilter.Name = "clrbtnMMVfoDisplayFilter";

            this.clrbtnMMVfoDisplayFilter.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayFilter.TabIndex = 121;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayFilter, "Filter colour");
            this.clrbtnMMVfoDisplayFilter.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayFilter_Changed);
            // 
            this.labelTS172.AutoSize = true;
            this.labelTS172.Image = null;
            this.labelTS172.Location = new System.Drawing.Point(73, 205);
            this.labelTS172.Name = "labelTS172";
            this.labelTS172.Size = new System.Drawing.Size(24, 13);
            this.labelTS172.TabIndex = 120;
            this.labelTS172.Text = "TX:";
            // 
            this.clrbtnMMVfoDisplayTx.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayTx.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayTx.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayTx.Image = null;
            this.clrbtnMMVfoDisplayTx.Location = new System.Drawing.Point(105, 200);
            this.clrbtnMMVfoDisplayTx.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayTx.Name = "clrbtnMMVfoDisplayTx";

            this.clrbtnMMVfoDisplayTx.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayTx.TabIndex = 119;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayTx, "TX box colour");
            this.clrbtnMMVfoDisplayTx.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayTx_Changed);
            // 
            this.labelTS171.AutoSize = true;
            this.labelTS171.Image = null;
            this.labelTS171.Location = new System.Drawing.Point(73, 176);
            this.labelTS171.Name = "labelTS171";
            this.labelTS171.Size = new System.Drawing.Size(25, 13);
            this.labelTS171.TabIndex = 118;
            this.labelTS171.Text = "RX:";
            // 
            this.clrbtnMMVfoDisplayRx.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayRx.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayRx.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayRx.Image = null;
            this.clrbtnMMVfoDisplayRx.Location = new System.Drawing.Point(105, 171);
            this.clrbtnMMVfoDisplayRx.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayRx.Name = "clrbtnMMVfoDisplayRx";

            this.clrbtnMMVfoDisplayRx.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayRx.TabIndex = 117;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayRx, "RX box colour");
            this.clrbtnMMVfoDisplayRx.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayRx_Changed);
            // 
            this.labelTS170.AutoSize = true;
            this.labelTS170.Image = null;
            this.labelTS170.Location = new System.Drawing.Point(169, 147);
            this.labelTS170.Name = "labelTS170";
            this.labelTS170.Size = new System.Drawing.Size(30, 13);
            this.labelTS170.TabIndex = 116;
            this.labelTS170.Text = "Split:";
            // 
            this.clrbtnMMVfoDisplaySplit.Automatic = "Automatic";
            this.clrbtnMMVfoDisplaySplit.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplaySplit.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplaySplit.Image = null;
            this.clrbtnMMVfoDisplaySplit.Location = new System.Drawing.Point(205, 142);
            this.clrbtnMMVfoDisplaySplit.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplaySplit.Name = "clrbtnMMVfoDisplaySplit";

            this.clrbtnMMVfoDisplaySplit.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplaySplit.TabIndex = 115;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplaySplit, "Split colour");
            this.clrbtnMMVfoDisplaySplit.Changed += new System.EventHandler(this.clrbtnMMVfoDisplaySplit_Changed);
            // 
            this.labelTS163.AutoSize = true;
            this.labelTS163.Image = null;
            this.labelTS163.Location = new System.Drawing.Point(44, 147);
            this.labelTS163.Name = "labelTS163";
            this.labelTS163.Size = new System.Drawing.Size(58, 13);
            this.labelTS163.TabIndex = 114;
            this.labelTS163.Text = "Split Back:";
            // 
            this.clrbtnMMVfoDisplaySplitBack.Automatic = "Automatic";
            this.clrbtnMMVfoDisplaySplitBack.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplaySplitBack.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplaySplitBack.Image = null;
            this.clrbtnMMVfoDisplaySplitBack.Location = new System.Drawing.Point(105, 142);
            this.clrbtnMMVfoDisplaySplitBack.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplaySplitBack.Name = "clrbtnMMVfoDisplaySplitBack";

            this.clrbtnMMVfoDisplaySplitBack.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplaySplitBack.TabIndex = 113;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplaySplitBack, "Background of the split");
            this.clrbtnMMVfoDisplaySplitBack.Changed += new System.EventHandler(this.clrbtnMMVfoDisplaySplitBack_Changed);
            // 
            this.labelTS166.AutoSize = true;
            this.labelTS166.Image = null;
            this.labelTS166.Location = new System.Drawing.Point(66, 123);
            this.labelTS166.Name = "labelTS166";
            this.labelTS166.Size = new System.Drawing.Size(37, 13);
            this.labelTS166.TabIndex = 112;
            this.labelTS166.Text = "Mode:";
            // 
            this.clrbtnMMVfoDisplayMode.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayMode.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayMode.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayMode.Image = null;
            this.clrbtnMMVfoDisplayMode.Location = new System.Drawing.Point(105, 118);
            this.clrbtnMMVfoDisplayMode.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayMode.Name = "clrbtnMMVfoDisplayMode";

            this.clrbtnMMVfoDisplayMode.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayMode.TabIndex = 111;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayMode, "Mode colour");
            this.clrbtnMMVfoDisplayMode.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayMode_Changed);
            // 
            this.clrbtnMMVfoDisplayTitle.Automatic = "Automatic";
            this.clrbtnMMVfoDisplayTitle.Color = System.Drawing.Color.Yellow;
            this.clrbtnMMVfoDisplayTitle.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMMVfoDisplayTitle.Image = null;
            this.clrbtnMMVfoDisplayTitle.Location = new System.Drawing.Point(105, 59);
            this.clrbtnMMVfoDisplayTitle.MoreColors = "More Colors...";
            this.clrbtnMMVfoDisplayTitle.Name = "clrbtnMMVfoDisplayTitle";

            this.clrbtnMMVfoDisplayTitle.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMMVfoDisplayTitle.TabIndex = 110;
            this.toolTip1.SetToolTip(this.clrbtnMMVfoDisplayTitle, "Titles colour");
            this.clrbtnMMVfoDisplayTitle.Changed += new System.EventHandler(this.clrbtnMMVfoDisplayTitle_Changed);
            // 
            this.grpMeterItemSpacerSettings.Controls.Add(this.labelTS199);
            this.grpMeterItemSpacerSettings.Controls.Add(this.clrbtnMeterItemHBackgroundSpacerTX);
            this.grpMeterItemSpacerSettings.Controls.Add(this.nudMeterItemSpacerPadding);
            this.grpMeterItemSpacerSettings.Controls.Add(this.labelTS197);
            this.grpMeterItemSpacerSettings.Controls.Add(this.labelTS196);
            this.grpMeterItemSpacerSettings.Controls.Add(this.clrbtnMeterItemHBackgroundSpacerRX);
            this.grpMeterItemSpacerSettings.Controls.Add(this.chkMeterItemFadeOnTxSpacer);
            this.grpMeterItemSpacerSettings.Controls.Add(this.chkMeterItemFadeOnRxSpacer);
            this.grpMeterItemSpacerSettings.Location = new System.Drawing.Point(12, 14);
            this.grpMeterItemSpacerSettings.Name = "grpMeterItemSpacerSettings";
            this.grpMeterItemSpacerSettings.Size = new System.Drawing.Size(323, 376);
            this.grpMeterItemSpacerSettings.TabIndex = 102;
            this.grpMeterItemSpacerSettings.TabStop = false;
            this.grpMeterItemSpacerSettings.Text = "Spacer";
            this.grpMeterItemSpacerSettings.Visible = false;
            // 
            this.labelTS199.AutoSize = true;
            this.labelTS199.Image = null;
            this.labelTS199.Location = new System.Drawing.Point(14, 63);
            this.labelTS199.Name = "labelTS199";
            this.labelTS199.Size = new System.Drawing.Size(85, 13);
            this.labelTS199.TabIndex = 134;
            this.labelTS199.Text = "Background TX:";
            // 
            this.clrbtnMeterItemHBackgroundSpacerTX.Automatic = "Automatic";
            this.clrbtnMeterItemHBackgroundSpacerTX.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnMeterItemHBackgroundSpacerTX.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemHBackgroundSpacerTX.Image = null;
            this.clrbtnMeterItemHBackgroundSpacerTX.Location = new System.Drawing.Point(103, 58);
            this.clrbtnMeterItemHBackgroundSpacerTX.MoreColors = "More Colors...";
            this.clrbtnMeterItemHBackgroundSpacerTX.Name = "clrbtnMeterItemHBackgroundSpacerTX";

            this.clrbtnMeterItemHBackgroundSpacerTX.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemHBackgroundSpacerTX.TabIndex = 133;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemHBackgroundSpacerTX, "Background colour");
            this.clrbtnMeterItemHBackgroundSpacerTX.Changed += new System.EventHandler(this.clrbtnMeterItemHBackgroundSpacerTX_Changed);
            // 
            this.nudMeterItemSpacerPadding.DecimalPlaces = 3;
            this.nudMeterItemSpacerPadding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudMeterItemSpacerPadding.Location = new System.Drawing.Point(102, 138);
            this.nudMeterItemSpacerPadding.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMeterItemSpacerPadding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudMeterItemSpacerPadding.Name = "nudMeterItemSpacerPadding";
            this.nudMeterItemSpacerPadding.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemSpacerPadding.TabIndex = 132;
            this.nudMeterItemSpacerPadding.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemSpacerPadding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudMeterItemSpacerPadding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemSpacerPadding.ValueChanged += new System.EventHandler(this.nudMeterItemSpacerPadding_ValueChanged);
            // 
            this.labelTS197.AutoSize = true;
            this.labelTS197.Image = null;
            this.labelTS197.Location = new System.Drawing.Point(10, 140);
            this.labelTS197.Name = "labelTS197";
            this.labelTS197.Size = new System.Drawing.Size(87, 13);
            this.labelTS197.TabIndex = 131;
            this.labelTS197.Text = "Vertical Padding:";
            // 
            this.labelTS196.AutoSize = true;
            this.labelTS196.Image = null;
            this.labelTS196.Location = new System.Drawing.Point(14, 34);
            this.labelTS196.Name = "labelTS196";
            this.labelTS196.Size = new System.Drawing.Size(86, 13);
            this.labelTS196.TabIndex = 130;
            this.labelTS196.Text = "Background RX:";
            // 
            this.clrbtnMeterItemHBackgroundSpacerRX.Automatic = "Automatic";
            this.clrbtnMeterItemHBackgroundSpacerRX.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnMeterItemHBackgroundSpacerRX.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemHBackgroundSpacerRX.Image = null;
            this.clrbtnMeterItemHBackgroundSpacerRX.Location = new System.Drawing.Point(103, 29);
            this.clrbtnMeterItemHBackgroundSpacerRX.MoreColors = "More Colors...";
            this.clrbtnMeterItemHBackgroundSpacerRX.Name = "clrbtnMeterItemHBackgroundSpacerRX";

            this.clrbtnMeterItemHBackgroundSpacerRX.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemHBackgroundSpacerRX.TabIndex = 129;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemHBackgroundSpacerRX, "Background colour");
            this.clrbtnMeterItemHBackgroundSpacerRX.Changed += new System.EventHandler(this.clrbtnMeterItemHBackgroundSpacerRX_Changed);
            // 
            this.chkMeterItemFadeOnTxSpacer.AutoSize = true;
            this.chkMeterItemFadeOnTxSpacer.Image = null;
            this.chkMeterItemFadeOnTxSpacer.Location = new System.Drawing.Point(59, 112);
            this.chkMeterItemFadeOnTxSpacer.Name = "chkMeterItemFadeOnTxSpacer";
            this.chkMeterItemFadeOnTxSpacer.Size = new System.Drawing.Size(82, 17);
            this.chkMeterItemFadeOnTxSpacer.TabIndex = 3;
            this.chkMeterItemFadeOnTxSpacer.Text = "Fade on TX";
            this.chkMeterItemFadeOnTxSpacer.UseVisualStyleBackColor = true;
            this.chkMeterItemFadeOnTxSpacer.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnTxSpacer_CheckedChanged);
            // 
            this.chkMeterItemFadeOnRxSpacer.AutoSize = true;
            this.chkMeterItemFadeOnRxSpacer.Image = null;
            this.chkMeterItemFadeOnRxSpacer.Location = new System.Drawing.Point(59, 89);
            this.chkMeterItemFadeOnRxSpacer.Name = "chkMeterItemFadeOnRxSpacer";
            this.chkMeterItemFadeOnRxSpacer.Size = new System.Drawing.Size(83, 17);
            this.chkMeterItemFadeOnRxSpacer.TabIndex = 2;
            this.chkMeterItemFadeOnRxSpacer.Text = "Fade on RX";
            this.chkMeterItemFadeOnRxSpacer.UseVisualStyleBackColor = true;
            this.chkMeterItemFadeOnRxSpacer.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnRxSpacer_CheckedChanged);
            // 
            this.grpTextOverlay.Controls.Add(this.btnTextOverlay_copyfonts);
            this.grpTextOverlay.Controls.Add(this.pbTextOverlay_variables);
            this.grpTextOverlay.Controls.Add(this.lblTextOverlay_panelbackgroundTX);
            this.grpTextOverlay.Controls.Add(this.clrbtnTextOverlay_PanelBackgroundTX);
            this.grpTextOverlay.Controls.Add(this.labelTS202);
            this.grpTextOverlay.Controls.Add(this.labelTS201);
            this.grpTextOverlay.Controls.Add(this.chkTextOverlay_textback2);
            this.grpTextOverlay.Controls.Add(this.chkTextOverlay_textback1);
            this.grpTextOverlay.Controls.Add(this.clrbtnTextOverlay_TextBackColour2);
            this.grpTextOverlay.Controls.Add(this.clrbtnTextOverlay_TextBackColour1);
            this.grpTextOverlay.Controls.Add(this.btnTextOverlay_copyoffsets);
            this.grpTextOverlay.Controls.Add(this.labelTS207);
            this.grpTextOverlay.Controls.Add(this.labelTS208);
            this.grpTextOverlay.Controls.Add(this.nudTextOverlay_TXyOffset);
            this.grpTextOverlay.Controls.Add(this.labelTS209);
            this.grpTextOverlay.Controls.Add(this.nudTextOverlay_TXxOffset);
            this.grpTextOverlay.Controls.Add(this.labelTS206);
            this.grpTextOverlay.Controls.Add(this.labelTS200);
            this.grpTextOverlay.Controls.Add(this.nudTextOverlay_RXyOffset);
            this.grpTextOverlay.Controls.Add(this.labelTS205);
            this.grpTextOverlay.Controls.Add(this.nudTextOverlay_RXxOffset);
            this.grpTextOverlay.Controls.Add(this.clrbtnTextOverlay_TextColour2);
            this.grpTextOverlay.Controls.Add(this.labelTS204);
            this.grpTextOverlay.Controls.Add(this.btnTextOverlay_Font2);
            this.grpTextOverlay.Controls.Add(this.txtTextOverlay_TXText);
            this.grpTextOverlay.Controls.Add(this.labelTS203);
            this.grpTextOverlay.Controls.Add(this.btnTextOverlay_Font1);
            this.grpTextOverlay.Controls.Add(this.txtTextOverlay_RXText);
            this.grpTextOverlay.Controls.Add(this.chkTextOverlay_ShowPanel);
            this.grpTextOverlay.Controls.Add(this.clrbtnTextOverlay_TextColour1);
            this.grpTextOverlay.Controls.Add(this.nudTextOverlay_PanelPadding);
            this.grpTextOverlay.Controls.Add(this.lblTextOverlay_panelpadding);
            this.grpTextOverlay.Controls.Add(this.lblTextOverlay_panelbackground);
            this.grpTextOverlay.Controls.Add(this.clrbtnTextOverlay_PanelBackground);
            this.grpTextOverlay.Controls.Add(this.chkTextOverlay_FadeOnTX);
            this.grpTextOverlay.Controls.Add(this.chkTextOverlay_FadeOnRX);
            this.grpTextOverlay.Location = new System.Drawing.Point(12, 16);
            this.grpTextOverlay.Name = "grpTextOverlay";
            this.grpTextOverlay.Size = new System.Drawing.Size(323, 376);
            this.grpTextOverlay.TabIndex = 104;
            this.grpTextOverlay.TabStop = false;
            this.grpTextOverlay.Text = "Text Overlay";
            this.grpTextOverlay.Visible = false;
            // 
            this.btnTextOverlay_copyfonts.Image = null;
            this.btnTextOverlay_copyfonts.Location = new System.Drawing.Point(284, 196);
            this.btnTextOverlay_copyfonts.Name = "btnTextOverlay_copyfonts";

            this.btnTextOverlay_copyfonts.Size = new System.Drawing.Size(33, 23);
            this.btnTextOverlay_copyfonts.TabIndex = 165;
            this.btnTextOverlay_copyfonts.Text = "=";
            this.toolTip1.SetToolTip(this.btnTextOverlay_copyfonts, "Copy the font and colours from RX to TX");
            this.btnTextOverlay_copyfonts.UseVisualStyleBackColor = true;
            this.btnTextOverlay_copyfonts.Click += new System.EventHandler(this.btnTextOverlay_copyfonts_Click);
            // 

            this.pbTextOverlay_variables.Location = new System.Drawing.Point(297, 100);
            this.pbTextOverlay_variables.Name = "pbTextOverlay_variables";
            this.pbTextOverlay_variables.Size = new System.Drawing.Size(20, 20);
            this.pbTextOverlay_variables.TabIndex = 164;
            this.pbTextOverlay_variables.TabStop = false;
            this.toolTip1.SetToolTip(this.pbTextOverlay_variables, "Click for info");
            this.pbTextOverlay_variables.Click += new System.EventHandler(this.pbTextOverlay_variables_Click);
            // 
            this.lblTextOverlay_panelbackgroundTX.AutoSize = true;
            this.lblTextOverlay_panelbackgroundTX.Image = null;
            this.lblTextOverlay_panelbackgroundTX.Location = new System.Drawing.Point(50, 75);
            this.lblTextOverlay_panelbackgroundTX.Name = "lblTextOverlay_panelbackgroundTX";
            this.lblTextOverlay_panelbackgroundTX.Size = new System.Drawing.Size(85, 13);
            this.lblTextOverlay_panelbackgroundTX.TabIndex = 163;
            this.lblTextOverlay_panelbackgroundTX.Text = "TX Background:";
            // 
            this.clrbtnTextOverlay_PanelBackgroundTX.Automatic = "Automatic";
            this.clrbtnTextOverlay_PanelBackgroundTX.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnTextOverlay_PanelBackgroundTX.ForeColor = System.Drawing.Color.Black;
            this.clrbtnTextOverlay_PanelBackgroundTX.Image = null;
            this.clrbtnTextOverlay_PanelBackgroundTX.Location = new System.Drawing.Point(141, 70);
            this.clrbtnTextOverlay_PanelBackgroundTX.MoreColors = "More Colors...";
            this.clrbtnTextOverlay_PanelBackgroundTX.Name = "clrbtnTextOverlay_PanelBackgroundTX";

            this.clrbtnTextOverlay_PanelBackgroundTX.Size = new System.Drawing.Size(40, 23);
            this.clrbtnTextOverlay_PanelBackgroundTX.TabIndex = 162;
            this.toolTip1.SetToolTip(this.clrbtnTextOverlay_PanelBackgroundTX, "Background colour");
            this.clrbtnTextOverlay_PanelBackgroundTX.Changed += new System.EventHandler(this.clrbtnTextOverlay_PanelBackgroundTX_Changed);
            // 
            this.labelTS202.AutoSize = true;
            this.labelTS202.Image = null;
            this.labelTS202.Location = new System.Drawing.Point(22, 214);
            this.labelTS202.Name = "labelTS202";
            this.labelTS202.Size = new System.Drawing.Size(24, 13);
            this.labelTS202.TabIndex = 161;
            this.labelTS202.Text = "TX:";
            // 
            this.labelTS201.AutoSize = true;
            this.labelTS201.Image = null;
            this.labelTS201.Location = new System.Drawing.Point(22, 188);
            this.labelTS201.Name = "labelTS201";
            this.labelTS201.Size = new System.Drawing.Size(25, 13);
            this.labelTS201.TabIndex = 160;
            this.labelTS201.Text = "RX:";
            // 
            this.chkTextOverlay_textback2.AutoSize = true;
            this.chkTextOverlay_textback2.Image = null;
            this.chkTextOverlay_textback2.Location = new System.Drawing.Point(205, 213);
            this.chkTextOverlay_textback2.Name = "chkTextOverlay_textback2";
            this.chkTextOverlay_textback2.Size = new System.Drawing.Size(75, 17);
            this.chkTextOverlay_textback2.TabIndex = 159;
            this.chkTextOverlay_textback2.Text = "Text Back";
            this.chkTextOverlay_textback2.UseVisualStyleBackColor = true;
            this.chkTextOverlay_textback2.CheckedChanged += new System.EventHandler(this.chkTextOverlay_textback2_CheckedChanged);
            // 
            this.chkTextOverlay_textback1.AutoSize = true;
            this.chkTextOverlay_textback1.Image = null;
            this.chkTextOverlay_textback1.Location = new System.Drawing.Point(205, 187);
            this.chkTextOverlay_textback1.Name = "chkTextOverlay_textback1";
            this.chkTextOverlay_textback1.Size = new System.Drawing.Size(75, 17);
            this.chkTextOverlay_textback1.TabIndex = 158;
            this.chkTextOverlay_textback1.Text = "Text Back";
            this.chkTextOverlay_textback1.UseVisualStyleBackColor = true;
            this.chkTextOverlay_textback1.CheckedChanged += new System.EventHandler(this.chkTextOverlay_textback1_CheckedChanged);
            // 
            this.clrbtnTextOverlay_TextBackColour2.Automatic = "Automatic";
            this.clrbtnTextOverlay_TextBackColour2.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnTextOverlay_TextBackColour2.ForeColor = System.Drawing.Color.Black;
            this.clrbtnTextOverlay_TextBackColour2.Image = null;
            this.clrbtnTextOverlay_TextBackColour2.Location = new System.Drawing.Point(159, 209);
            this.clrbtnTextOverlay_TextBackColour2.MoreColors = "More Colors...";
            this.clrbtnTextOverlay_TextBackColour2.Name = "clrbtnTextOverlay_TextBackColour2";

            this.clrbtnTextOverlay_TextBackColour2.Size = new System.Drawing.Size(40, 23);
            this.clrbtnTextOverlay_TextBackColour2.TabIndex = 157;
            this.toolTip1.SetToolTip(this.clrbtnTextOverlay_TextBackColour2, "Background colour");
            this.clrbtnTextOverlay_TextBackColour2.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextBackColour2_Changed);
            // 
            this.clrbtnTextOverlay_TextBackColour1.Automatic = "Automatic";
            this.clrbtnTextOverlay_TextBackColour1.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnTextOverlay_TextBackColour1.ForeColor = System.Drawing.Color.Black;
            this.clrbtnTextOverlay_TextBackColour1.Image = null;
            this.clrbtnTextOverlay_TextBackColour1.Location = new System.Drawing.Point(159, 183);
            this.clrbtnTextOverlay_TextBackColour1.MoreColors = "More Colors...";
            this.clrbtnTextOverlay_TextBackColour1.Name = "clrbtnTextOverlay_TextBackColour1";

            this.clrbtnTextOverlay_TextBackColour1.Size = new System.Drawing.Size(40, 23);
            this.clrbtnTextOverlay_TextBackColour1.TabIndex = 156;
            this.toolTip1.SetToolTip(this.clrbtnTextOverlay_TextBackColour1, "Background colour");
            this.clrbtnTextOverlay_TextBackColour1.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextBackColour1_Changed);
            // 
            this.btnTextOverlay_copyoffsets.Image = null;
            this.btnTextOverlay_copyoffsets.Location = new System.Drawing.Point(182, 313);
            this.btnTextOverlay_copyoffsets.Name = "btnTextOverlay_copyoffsets";

            this.btnTextOverlay_copyoffsets.Size = new System.Drawing.Size(33, 23);
            this.btnTextOverlay_copyoffsets.TabIndex = 155;
            this.btnTextOverlay_copyoffsets.Text = "=";
            this.toolTip1.SetToolTip(this.btnTextOverlay_copyoffsets, "Copy the offset values from RX to TX");
            this.btnTextOverlay_copyoffsets.UseVisualStyleBackColor = true;
            this.btnTextOverlay_copyoffsets.Click += new System.EventHandler(this.btnTextOverlay_copyoffsets_Click);
            // 
            this.labelTS207.AutoSize = true;
            this.labelTS207.Image = null;
            this.labelTS207.Location = new System.Drawing.Point(163, 330);
            this.labelTS207.Name = "labelTS207";
            this.labelTS207.Size = new System.Drawing.Size(12, 13);
            this.labelTS207.TabIndex = 154;
            this.labelTS207.Text = "y";
            // 
            this.labelTS208.AutoSize = true;
            this.labelTS208.Image = null;
            this.labelTS208.Location = new System.Drawing.Point(163, 304);
            this.labelTS208.Name = "labelTS208";
            this.labelTS208.Size = new System.Drawing.Size(12, 13);
            this.labelTS208.TabIndex = 153;
            this.labelTS208.Text = "x";
            // 
            this.nudTextOverlay_TXyOffset.DecimalPlaces = 3;
            this.nudTextOverlay_TXyOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudTextOverlay_TXyOffset.Location = new System.Drawing.Point(101, 328);
            this.nudTextOverlay_TXyOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudTextOverlay_TXyOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudTextOverlay_TXyOffset.Name = "nudTextOverlay_TXyOffset";
            this.nudTextOverlay_TXyOffset.Size = new System.Drawing.Size(56, 20);
            this.nudTextOverlay_TXyOffset.TabIndex = 152;
            this.nudTextOverlay_TXyOffset.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudTextOverlay_TXyOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudTextOverlay_TXyOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTextOverlay_TXyOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_TXyOffset_ValueChanged);
            // 
            this.labelTS209.AutoSize = true;
            this.labelTS209.Image = null;
            this.labelTS209.Location = new System.Drawing.Point(9, 304);
            this.labelTS209.Name = "labelTS209";
            this.labelTS209.Size = new System.Drawing.Size(84, 13);
            this.labelTS209.TabIndex = 151;
            this.labelTS209.Text = "TX Text Offsets:";
            // 
            this.nudTextOverlay_TXxOffset.DecimalPlaces = 3;
            this.nudTextOverlay_TXxOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudTextOverlay_TXxOffset.Location = new System.Drawing.Point(101, 302);
            this.nudTextOverlay_TXxOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudTextOverlay_TXxOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudTextOverlay_TXxOffset.Name = "nudTextOverlay_TXxOffset";
            this.nudTextOverlay_TXxOffset.Size = new System.Drawing.Size(56, 20);
            this.nudTextOverlay_TXxOffset.TabIndex = 150;
            this.nudTextOverlay_TXxOffset.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudTextOverlay_TXxOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudTextOverlay_TXxOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTextOverlay_TXxOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_TXxOffset_ValueChanged);
            // 
            this.labelTS206.AutoSize = true;
            this.labelTS206.Image = null;
            this.labelTS206.Location = new System.Drawing.Point(163, 278);
            this.labelTS206.Name = "labelTS206";
            this.labelTS206.Size = new System.Drawing.Size(12, 13);
            this.labelTS206.TabIndex = 149;
            this.labelTS206.Text = "y";
            // 
            this.labelTS200.AutoSize = true;
            this.labelTS200.Image = null;
            this.labelTS200.Location = new System.Drawing.Point(163, 252);
            this.labelTS200.Name = "labelTS200";
            this.labelTS200.Size = new System.Drawing.Size(12, 13);
            this.labelTS200.TabIndex = 148;
            this.labelTS200.Text = "x";
            // 
            this.nudTextOverlay_RXyOffset.DecimalPlaces = 3;
            this.nudTextOverlay_RXyOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudTextOverlay_RXyOffset.Location = new System.Drawing.Point(101, 276);
            this.nudTextOverlay_RXyOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudTextOverlay_RXyOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudTextOverlay_RXyOffset.Name = "nudTextOverlay_RXyOffset";
            this.nudTextOverlay_RXyOffset.Size = new System.Drawing.Size(56, 20);
            this.nudTextOverlay_RXyOffset.TabIndex = 147;
            this.nudTextOverlay_RXyOffset.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudTextOverlay_RXyOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudTextOverlay_RXyOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTextOverlay_RXyOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_RXyOffset_ValueChanged);
            // 
            this.labelTS205.AutoSize = true;
            this.labelTS205.Image = null;
            this.labelTS205.Location = new System.Drawing.Point(10, 252);
            this.labelTS205.Name = "labelTS205";
            this.labelTS205.Size = new System.Drawing.Size(85, 13);
            this.labelTS205.TabIndex = 145;
            this.labelTS205.Text = "RX Text Offsets:";
            // 
            this.nudTextOverlay_RXxOffset.DecimalPlaces = 3;
            this.nudTextOverlay_RXxOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudTextOverlay_RXxOffset.Location = new System.Drawing.Point(101, 250);
            this.nudTextOverlay_RXxOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudTextOverlay_RXxOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudTextOverlay_RXxOffset.Name = "nudTextOverlay_RXxOffset";
            this.nudTextOverlay_RXxOffset.Size = new System.Drawing.Size(56, 20);
            this.nudTextOverlay_RXxOffset.TabIndex = 143;
            this.nudTextOverlay_RXxOffset.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudTextOverlay_RXxOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudTextOverlay_RXxOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTextOverlay_RXxOffset.ValueChanged += new System.EventHandler(this.nudTextOverlay_RXxOffset_ValueChanged);
            // 
            this.clrbtnTextOverlay_TextColour2.Automatic = "Automatic";
            this.clrbtnTextOverlay_TextColour2.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnTextOverlay_TextColour2.ForeColor = System.Drawing.Color.Black;
            this.clrbtnTextOverlay_TextColour2.Image = null;
            this.clrbtnTextOverlay_TextColour2.Location = new System.Drawing.Point(104, 209);
            this.clrbtnTextOverlay_TextColour2.MoreColors = "More Colors...";
            this.clrbtnTextOverlay_TextColour2.Name = "clrbtnTextOverlay_TextColour2";

            this.clrbtnTextOverlay_TextColour2.Size = new System.Drawing.Size(40, 23);
            this.clrbtnTextOverlay_TextColour2.TabIndex = 142;
            this.toolTip1.SetToolTip(this.clrbtnTextOverlay_TextColour2, "Background colour");
            this.clrbtnTextOverlay_TextColour2.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextColour2_Changed);
            // 
            this.labelTS204.AutoSize = true;
            this.labelTS204.Image = null;
            this.labelTS204.Location = new System.Drawing.Point(5, 159);
            this.labelTS204.Name = "labelTS204";
            this.labelTS204.Size = new System.Drawing.Size(48, 13);
            this.labelTS204.TabIndex = 141;
            this.labelTS204.Text = "TX Text:";
            // 
            this.btnTextOverlay_Font2.Image = null;
            this.btnTextOverlay_Font2.Location = new System.Drawing.Point(53, 209);
            this.btnTextOverlay_Font2.Name = "btnTextOverlay_Font2";

            this.btnTextOverlay_Font2.Size = new System.Drawing.Size(49, 23);
            this.btnTextOverlay_Font2.TabIndex = 140;
            this.btnTextOverlay_Font2.Text = "Font";
            this.btnTextOverlay_Font2.UseVisualStyleBackColor = true;
            this.btnTextOverlay_Font2.Click += new System.EventHandler(this.btnTextOverlay_Font2_Click);
            // 
            this.txtTextOverlay_TXText.Location = new System.Drawing.Point(53, 156);
            this.txtTextOverlay_TXText.Name = "txtTextOverlay_TXText";
            this.txtTextOverlay_TXText.Size = new System.Drawing.Size(264, 20);
            this.txtTextOverlay_TXText.TabIndex = 139;
            this.txtTextOverlay_TXText.TextChanged += new System.EventHandler(this.txtTextOverlay_TXText_TextChanged);
            // 
            this.labelTS203.AutoSize = true;
            this.labelTS203.Image = null;
            this.labelTS203.Location = new System.Drawing.Point(5, 133);
            this.labelTS203.Name = "labelTS203";
            this.labelTS203.Size = new System.Drawing.Size(49, 13);
            this.labelTS203.TabIndex = 138;
            this.labelTS203.Text = "RX Text:";
            // 
            this.btnTextOverlay_Font1.Image = null;
            this.btnTextOverlay_Font1.Location = new System.Drawing.Point(53, 183);
            this.btnTextOverlay_Font1.Name = "btnTextOverlay_Font1";

            this.btnTextOverlay_Font1.Size = new System.Drawing.Size(49, 23);
            this.btnTextOverlay_Font1.TabIndex = 137;
            this.btnTextOverlay_Font1.Text = "Font";
            this.btnTextOverlay_Font1.UseVisualStyleBackColor = true;
            this.btnTextOverlay_Font1.Click += new System.EventHandler(this.btnTextOverlay_Font1_Click);
            // 
            this.txtTextOverlay_RXText.Location = new System.Drawing.Point(53, 130);
            this.txtTextOverlay_RXText.Name = "txtTextOverlay_RXText";
            this.txtTextOverlay_RXText.Size = new System.Drawing.Size(264, 20);
            this.txtTextOverlay_RXText.TabIndex = 136;
            this.txtTextOverlay_RXText.TextChanged += new System.EventHandler(this.txtTextOverlay_RXText_TextChanged);
            // 
            this.chkTextOverlay_ShowPanel.AutoSize = true;
            this.chkTextOverlay_ShowPanel.Image = null;
            this.chkTextOverlay_ShowPanel.Location = new System.Drawing.Point(18, 30);
            this.chkTextOverlay_ShowPanel.Name = "chkTextOverlay_ShowPanel";
            this.chkTextOverlay_ShowPanel.Size = new System.Drawing.Size(83, 17);
            this.chkTextOverlay_ShowPanel.TabIndex = 135;
            this.chkTextOverlay_ShowPanel.Text = "Show Panel";
            this.chkTextOverlay_ShowPanel.UseVisualStyleBackColor = true;
            this.chkTextOverlay_ShowPanel.CheckedChanged += new System.EventHandler(this.chkTextOverlay_ShowPanel_CheckedChanged);
            // 
            this.clrbtnTextOverlay_TextColour1.Automatic = "Automatic";
            this.clrbtnTextOverlay_TextColour1.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnTextOverlay_TextColour1.ForeColor = System.Drawing.Color.Black;
            this.clrbtnTextOverlay_TextColour1.Image = null;
            this.clrbtnTextOverlay_TextColour1.Location = new System.Drawing.Point(104, 183);
            this.clrbtnTextOverlay_TextColour1.MoreColors = "More Colors...";
            this.clrbtnTextOverlay_TextColour1.Name = "clrbtnTextOverlay_TextColour1";

            this.clrbtnTextOverlay_TextColour1.Size = new System.Drawing.Size(40, 23);
            this.clrbtnTextOverlay_TextColour1.TabIndex = 133;
            this.toolTip1.SetToolTip(this.clrbtnTextOverlay_TextColour1, "Background colour");
            this.clrbtnTextOverlay_TextColour1.Changed += new System.EventHandler(this.clrbtnTextOverlay_TextColour1_Changed);
            // 
            this.nudTextOverlay_PanelPadding.DecimalPlaces = 3;
            this.nudTextOverlay_PanelPadding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudTextOverlay_PanelPadding.Location = new System.Drawing.Point(125, 100);
            this.nudTextOverlay_PanelPadding.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudTextOverlay_PanelPadding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudTextOverlay_PanelPadding.Name = "nudTextOverlay_PanelPadding";
            this.nudTextOverlay_PanelPadding.Size = new System.Drawing.Size(56, 20);
            this.nudTextOverlay_PanelPadding.TabIndex = 132;
            this.nudTextOverlay_PanelPadding.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudTextOverlay_PanelPadding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudTextOverlay_PanelPadding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTextOverlay_PanelPadding.ValueChanged += new System.EventHandler(this.nudTextOverlay_PanelPadding_ValueChanged);
            // 
            this.lblTextOverlay_panelpadding.AutoSize = true;
            this.lblTextOverlay_panelpadding.Image = null;
            this.lblTextOverlay_panelpadding.Location = new System.Drawing.Point(39, 102);
            this.lblTextOverlay_panelpadding.Name = "lblTextOverlay_panelpadding";
            this.lblTextOverlay_panelpadding.Size = new System.Drawing.Size(79, 13);
            this.lblTextOverlay_panelpadding.TabIndex = 131;
            this.lblTextOverlay_panelpadding.Text = "Panel Padding:";
            // 
            this.lblTextOverlay_panelbackground.AutoSize = true;
            this.lblTextOverlay_panelbackground.Image = null;
            this.lblTextOverlay_panelbackground.Location = new System.Drawing.Point(49, 50);
            this.lblTextOverlay_panelbackground.Name = "lblTextOverlay_panelbackground";
            this.lblTextOverlay_panelbackground.Size = new System.Drawing.Size(86, 13);
            this.lblTextOverlay_panelbackground.TabIndex = 130;
            this.lblTextOverlay_panelbackground.Text = "RX Background:";
            // 
            this.clrbtnTextOverlay_PanelBackground.Automatic = "Automatic";
            this.clrbtnTextOverlay_PanelBackground.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnTextOverlay_PanelBackground.ForeColor = System.Drawing.Color.Black;
            this.clrbtnTextOverlay_PanelBackground.Image = null;
            this.clrbtnTextOverlay_PanelBackground.Location = new System.Drawing.Point(141, 45);
            this.clrbtnTextOverlay_PanelBackground.MoreColors = "More Colors...";
            this.clrbtnTextOverlay_PanelBackground.Name = "clrbtnTextOverlay_PanelBackground";

            this.clrbtnTextOverlay_PanelBackground.Size = new System.Drawing.Size(40, 23);
            this.clrbtnTextOverlay_PanelBackground.TabIndex = 129;
            this.toolTip1.SetToolTip(this.clrbtnTextOverlay_PanelBackground, "Background colour");
            this.clrbtnTextOverlay_PanelBackground.Changed += new System.EventHandler(this.clrbtnTextOverlay_PanelBackground_Changed);
            // 
            this.chkTextOverlay_FadeOnTX.AutoSize = true;
            this.chkTextOverlay_FadeOnTX.Image = null;
            this.chkTextOverlay_FadeOnTX.Location = new System.Drawing.Point(219, 53);
            this.chkTextOverlay_FadeOnTX.Name = "chkTextOverlay_FadeOnTX";
            this.chkTextOverlay_FadeOnTX.Size = new System.Drawing.Size(82, 17);
            this.chkTextOverlay_FadeOnTX.TabIndex = 3;
            this.chkTextOverlay_FadeOnTX.Text = "Fade on TX";
            this.chkTextOverlay_FadeOnTX.UseVisualStyleBackColor = true;
            this.chkTextOverlay_FadeOnTX.CheckedChanged += new System.EventHandler(this.chkTextOverlay_FadeOnTX_CheckedChanged);
            // 
            this.chkTextOverlay_FadeOnRX.AutoSize = true;
            this.chkTextOverlay_FadeOnRX.Image = null;
            this.chkTextOverlay_FadeOnRX.Location = new System.Drawing.Point(219, 30);
            this.chkTextOverlay_FadeOnRX.Name = "chkTextOverlay_FadeOnRX";
            this.chkTextOverlay_FadeOnRX.Size = new System.Drawing.Size(83, 17);
            this.chkTextOverlay_FadeOnRX.TabIndex = 2;
            this.chkTextOverlay_FadeOnRX.Text = "Fade on RX";
            this.chkTextOverlay_FadeOnRX.UseVisualStyleBackColor = true;
            this.chkTextOverlay_FadeOnRX.CheckedChanged += new System.EventHandler(this.chkTextOverlay_FadeOnRX_CheckedChanged);
            // 
            this.grpMeterItemDataOutNode.Controls.Add(this.labelTS210);
            this.grpMeterItemDataOutNode.Controls.Add(this.txtDataOutNode_4charID);
            this.grpMeterItemDataOutNode.Controls.Add(this.labelTS217);
            this.grpMeterItemDataOutNode.Controls.Add(this.nudDataOutNode_sendinterval);
            this.grpMeterItemDataOutNode.Controls.Add(this.labelTS215);
            this.grpMeterItemDataOutNode.Location = new System.Drawing.Point(12, 15);
            this.grpMeterItemDataOutNode.Name = "grpMeterItemDataOutNode";
            this.grpMeterItemDataOutNode.Size = new System.Drawing.Size(323, 376);
            this.grpMeterItemDataOutNode.TabIndex = 105;
            this.grpMeterItemDataOutNode.TabStop = false;
            this.grpMeterItemDataOutNode.Text = "Data Out Node";
            this.grpMeterItemDataOutNode.Visible = false;
            // 
            this.labelTS210.AutoSize = true;
            this.labelTS210.Image = null;
            this.labelTS210.Location = new System.Drawing.Point(35, 51);
            this.labelTS210.Name = "labelTS210";
            this.labelTS210.Size = new System.Drawing.Size(52, 13);
            this.labelTS210.TabIndex = 137;
            this.labelTS210.Text = "4Char ID:";
            // 
            this.txtDataOutNode_4charID.Location = new System.Drawing.Point(93, 48);
            this.txtDataOutNode_4charID.MaxLength = 4;
            this.txtDataOutNode_4charID.Name = "txtDataOutNode_4charID";
            this.txtDataOutNode_4charID.Size = new System.Drawing.Size(43, 20);
            this.txtDataOutNode_4charID.TabIndex = 136;
            this.txtDataOutNode_4charID.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.txtDataOutNode_4charID, "The four character code used to reference which MultiMeterIO is to be used for th" +
        "e output data.");
            this.txtDataOutNode_4charID.TextChanged += new System.EventHandler(this.txtDataOutNode_4charID_TextChanged);
            // 
            this.labelTS217.AutoSize = true;
            this.labelTS217.Image = null;
            this.labelTS217.Location = new System.Drawing.Point(155, 24);
            this.labelTS217.Name = "labelTS217";
            this.labelTS217.Size = new System.Drawing.Size(20, 13);
            this.labelTS217.TabIndex = 135;
            this.labelTS217.Text = "ms";
            // 
            this.nudDataOutNode_sendinterval.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudDataOutNode_sendinterval.Location = new System.Drawing.Point(93, 22);
            this.nudDataOutNode_sendinterval.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudDataOutNode_sendinterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudDataOutNode_sendinterval.Name = "nudDataOutNode_sendinterval";
            this.nudDataOutNode_sendinterval.Size = new System.Drawing.Size(56, 20);
            this.nudDataOutNode_sendinterval.TabIndex = 132;
            this.nudDataOutNode_sendinterval.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudDataOutNode_sendinterval, "The interval that the data is sent.");
            this.nudDataOutNode_sendinterval.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudDataOutNode_sendinterval.ValueChanged += new System.EventHandler(this.nudDataOutNode_sendinterval_ValueChanged);
            // 
            this.labelTS215.AutoSize = true;
            this.labelTS215.Image = null;
            this.labelTS215.Location = new System.Drawing.Point(14, 25);
            this.labelTS215.Name = "labelTS215";
            this.labelTS215.Size = new System.Drawing.Size(73, 13);
            this.labelTS215.TabIndex = 131;
            this.labelTS215.Text = "Send Interval:";
            // 
            this.grpMeterItemRotator.Controls.Add(this.lblMeterItemRotatorBeamWidth_alpha);
            this.grpMeterItemRotator.Controls.Add(this.lblMeterItemRotatorBeamWidth_degrees);
            this.grpMeterItemRotator.Controls.Add(this.nudMeterItemRotatorBeamWidth_alpha);
            this.grpMeterItemRotator.Controls.Add(this.labelTS237);
            this.grpMeterItemRotator.Controls.Add(this.txtMeterItemRotatorSTOPcommand);
            this.grpMeterItemRotator.Controls.Add(this.nudMeterItemRotator_padding);
            this.grpMeterItemRotator.Controls.Add(this.radMeterItemRotator_show_both);
            this.grpMeterItemRotator.Controls.Add(this.radMeterItemRotator_show_ele);
            this.grpMeterItemRotator.Controls.Add(this.radMeterItemRotator_show_az);
            this.grpMeterItemRotator.Controls.Add(this.lblRotator_4charID);
            this.grpMeterItemRotator.Controls.Add(this.txtRotator_4charID);
            this.grpMeterItemRotator.Controls.Add(this.bntMultiMeterItemRotator_default_pstRotator);
            this.grpMeterItemRotator.Controls.Add(this.picMultiMeterRotatorControlInfo);
            this.grpMeterItemRotator.Controls.Add(this.lblMeterItemRotatorELEcommand);
            this.grpMeterItemRotator.Controls.Add(this.lblMeterItemRotatorAZcommand);
            this.grpMeterItemRotator.Controls.Add(this.txtMeterItemRotatorELEcommand);
            this.grpMeterItemRotator.Controls.Add(this.txtMeterItemRotatorAZcommand);
            this.grpMeterItemRotator.Controls.Add(this.clrbtnMeterItemRotatorControlColour);
            this.grpMeterItemRotator.Controls.Add(this.chkMeterItemRotatorAllowControl);
            this.grpMeterItemRotator.Controls.Add(this.pnlVariableInUse_2_rotator);
            this.grpMeterItemRotator.Controls.Add(this.chkMeterItemRotatorCardinals);
            this.grpMeterItemRotator.Controls.Add(this.labelTS212);
            this.grpMeterItemRotator.Controls.Add(this.clrbtnMeterItemRotatorText);
            this.grpMeterItemRotator.Controls.Add(this.nudMeterItemRotatorBeamWidth);
            this.grpMeterItemRotator.Controls.Add(this.btnMMIO_variable_2_rotator);
            this.grpMeterItemRotator.Controls.Add(this.btnMMIO_variable_rotator);
            this.grpMeterItemRotator.Controls.Add(this.chkMeterItemRotatorShowBeamWidth);
            this.grpMeterItemRotator.Controls.Add(this.labelTS218);
            this.grpMeterItemRotator.Controls.Add(this.clrbtnMeterItemRotatorBeamWidth);
            this.grpMeterItemRotator.Controls.Add(this.chkMeterItemDarkModeRotator);
            this.grpMeterItemRotator.Controls.Add(this.nudMeterItemUpdateRateRotator);
            this.grpMeterItemRotator.Controls.Add(this.labelTS225);
            this.grpMeterItemRotator.Controls.Add(this.labelTS227);
            this.grpMeterItemRotator.Controls.Add(this.clrbtnMeterItemHBackgroundRotator);
            this.grpMeterItemRotator.Controls.Add(this.labelTS228);
            this.grpMeterItemRotator.Controls.Add(this.clrbtnMeterItemRotatorSmallDot);
            this.grpMeterItemRotator.Controls.Add(this.labelTS229);
            this.grpMeterItemRotator.Controls.Add(this.labelTS230);
            this.grpMeterItemRotator.Controls.Add(this.clrbtnMeterItemRotatorLargeDot);
            this.grpMeterItemRotator.Controls.Add(this.clrbtnMeterItemRotatorArrow);
            this.grpMeterItemRotator.Controls.Add(this.chkMeterItemFadeOnTxRotator);
            this.grpMeterItemRotator.Controls.Add(this.chkMeterItemFadeOnRxRotator);
            this.grpMeterItemRotator.Controls.Add(this.pnlVariableInUse_1_rotator);
            this.grpMeterItemRotator.Location = new System.Drawing.Point(17, 24);
            this.grpMeterItemRotator.Name = "grpMeterItemRotator";
            this.grpMeterItemRotator.Size = new System.Drawing.Size(323, 376);
            this.grpMeterItemRotator.TabIndex = 106;
            this.grpMeterItemRotator.TabStop = false;
            this.grpMeterItemRotator.Text = "Rotator";
            this.grpMeterItemRotator.Visible = false;
            // 
            this.lblMeterItemRotatorBeamWidth_alpha.AutoSize = true;
            this.lblMeterItemRotatorBeamWidth_alpha.Image = null;
            this.lblMeterItemRotatorBeamWidth_alpha.Location = new System.Drawing.Point(153, 169);
            this.lblMeterItemRotatorBeamWidth_alpha.Name = "lblMeterItemRotatorBeamWidth_alpha";
            this.lblMeterItemRotatorBeamWidth_alpha.Size = new System.Drawing.Size(37, 13);
            this.lblMeterItemRotatorBeamWidth_alpha.TabIndex = 177;
            this.lblMeterItemRotatorBeamWidth_alpha.Text = "Alpha:";
            // 
            this.lblMeterItemRotatorBeamWidth_degrees.AutoSize = true;
            this.lblMeterItemRotatorBeamWidth_degrees.Image = null;
            this.lblMeterItemRotatorBeamWidth_degrees.Location = new System.Drawing.Point(27, 169);
            this.lblMeterItemRotatorBeamWidth_degrees.Name = "lblMeterItemRotatorBeamWidth_degrees";
            this.lblMeterItemRotatorBeamWidth_degrees.Size = new System.Drawing.Size(50, 13);
            this.lblMeterItemRotatorBeamWidth_degrees.TabIndex = 176;
            this.lblMeterItemRotatorBeamWidth_degrees.Text = "Degrees:";
            // 
            this.nudMeterItemRotatorBeamWidth_alpha.DecimalPlaces = 2;
            this.nudMeterItemRotatorBeamWidth_alpha.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudMeterItemRotatorBeamWidth_alpha.Location = new System.Drawing.Point(196, 167);
            this.nudMeterItemRotatorBeamWidth_alpha.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemRotatorBeamWidth_alpha.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMeterItemRotatorBeamWidth_alpha.Name = "nudMeterItemRotatorBeamWidth_alpha";
            this.nudMeterItemRotatorBeamWidth_alpha.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemRotatorBeamWidth_alpha.TabIndex = 175;
            this.nudMeterItemRotatorBeamWidth_alpha.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemRotatorBeamWidth_alpha, "3dB beam width");
            this.nudMeterItemRotatorBeamWidth_alpha.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMeterItemRotatorBeamWidth_alpha.ValueChanged += new System.EventHandler(this.nudMeterItemRotatorBeamWidth_alpha_ValueChanged);
            // 
            this.labelTS237.AutoSize = true;
            this.labelTS237.Image = null;
            this.labelTS237.Location = new System.Drawing.Point(5, 352);
            this.labelTS237.Name = "labelTS237";
            this.labelTS237.Size = new System.Drawing.Size(39, 13);
            this.labelTS237.TabIndex = 174;
            this.labelTS237.Text = "STOP:";
            // 
            this.txtMeterItemRotatorSTOPcommand.Location = new System.Drawing.Point(50, 349);
            this.txtMeterItemRotatorSTOPcommand.Name = "txtMeterItemRotatorSTOPcommand";
            this.txtMeterItemRotatorSTOPcommand.Size = new System.Drawing.Size(261, 20);
            this.txtMeterItemRotatorSTOPcommand.TabIndex = 173;
            this.toolTip1.SetToolTip(this.txtMeterItemRotatorSTOPcommand, "The ELE rotator string");
            this.txtMeterItemRotatorSTOPcommand.TextChanged += new System.EventHandler(this.txtMeterItemRotatorSTOPcommand_TextChanged);
            // 
            this.nudMeterItemRotator_padding.DecimalPlaces = 3;
            this.nudMeterItemRotator_padding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudMeterItemRotator_padding.Location = new System.Drawing.Point(222, 223);
            this.nudMeterItemRotator_padding.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemRotator_padding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudMeterItemRotator_padding.Name = "nudMeterItemRotator_padding";
            this.nudMeterItemRotator_padding.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemRotator_padding.TabIndex = 172;
            this.nudMeterItemRotator_padding.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemRotator_padding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudMeterItemRotator_padding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemRotator_padding.ValueChanged += new System.EventHandler(this.nudMeterItemRotator_padding_ValueChanged);
            // 
            this.radMeterItemRotator_show_both.AutoSize = true;
            this.radMeterItemRotator_show_both.Image = null;
            this.radMeterItemRotator_show_both.Location = new System.Drawing.Point(168, 223);
            this.radMeterItemRotator_show_both.Name = "radMeterItemRotator_show_both";
            this.radMeterItemRotator_show_both.Size = new System.Drawing.Size(47, 17);
            this.radMeterItemRotator_show_both.TabIndex = 171;
            this.radMeterItemRotator_show_both.TabStop = true;
            this.radMeterItemRotator_show_both.Text = "Both";
            this.toolTip1.SetToolTip(this.radMeterItemRotator_show_both, "Show both azimuth and elevation");
            this.radMeterItemRotator_show_both.UseVisualStyleBackColor = true;
            this.radMeterItemRotator_show_both.CheckedChanged += new System.EventHandler(this.radMeterItemRotator_show_both_CheckedChanged);
            // 
            this.radMeterItemRotator_show_ele.AutoSize = true;
            this.radMeterItemRotator_show_ele.Image = null;
            this.radMeterItemRotator_show_ele.Location = new System.Drawing.Point(93, 223);
            this.radMeterItemRotator_show_ele.Name = "radMeterItemRotator_show_ele";
            this.radMeterItemRotator_show_ele.Size = new System.Drawing.Size(69, 17);
            this.radMeterItemRotator_show_ele.TabIndex = 170;
            this.radMeterItemRotator_show_ele.TabStop = true;
            this.radMeterItemRotator_show_ele.Text = "Elevation";
            this.toolTip1.SetToolTip(this.radMeterItemRotator_show_ele, "Show Elevation only");
            this.radMeterItemRotator_show_ele.UseVisualStyleBackColor = true;
            this.radMeterItemRotator_show_ele.CheckedChanged += new System.EventHandler(this.radMeterItemRotator_show_ele_CheckedChanged);
            // 
            this.radMeterItemRotator_show_az.AutoSize = true;
            this.radMeterItemRotator_show_az.Image = null;
            this.radMeterItemRotator_show_az.Location = new System.Drawing.Point(25, 223);
            this.radMeterItemRotator_show_az.Name = "radMeterItemRotator_show_az";
            this.radMeterItemRotator_show_az.Size = new System.Drawing.Size(62, 17);
            this.radMeterItemRotator_show_az.TabIndex = 169;
            this.radMeterItemRotator_show_az.TabStop = true;
            this.radMeterItemRotator_show_az.Text = "Azimuth";
            this.toolTip1.SetToolTip(this.radMeterItemRotator_show_az, "Show azimuth only");
            this.radMeterItemRotator_show_az.UseVisualStyleBackColor = true;
            this.radMeterItemRotator_show_az.CheckedChanged += new System.EventHandler(this.radMeterItemRotator_show_az_CheckedChanged);
            // 
            this.lblRotator_4charID.AutoSize = true;
            this.lblRotator_4charID.Image = null;
            this.lblRotator_4charID.Location = new System.Drawing.Point(210, 275);
            this.lblRotator_4charID.Name = "lblRotator_4charID";
            this.lblRotator_4charID.Size = new System.Drawing.Size(52, 13);
            this.lblRotator_4charID.TabIndex = 168;
            this.lblRotator_4charID.Text = "4Char ID:";
            // 
            this.txtRotator_4charID.Location = new System.Drawing.Point(268, 272);
            this.txtRotator_4charID.MaxLength = 4;
            this.txtRotator_4charID.Name = "txtRotator_4charID";
            this.txtRotator_4charID.Size = new System.Drawing.Size(43, 20);
            this.txtRotator_4charID.TabIndex = 167;
            this.txtRotator_4charID.Text = "AGHJ";
            this.toolTip1.SetToolTip(this.txtRotator_4charID, "The four character code used to reference which MultiMeterIO is to be used for th" +
        "e output data.");
            this.txtRotator_4charID.TextChanged += new System.EventHandler(this.txtRotator_4charID_TextChanged);
            // 
            this.bntMultiMeterItemRotator_default_pstRotator.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bntMultiMeterItemRotator_default_pstRotator.Image = null;
            this.bntMultiMeterItemRotator_default_pstRotator.Location = new System.Drawing.Point(222, 245);
            this.bntMultiMeterItemRotator_default_pstRotator.Name = "bntMultiMeterItemRotator_default_pstRotator";

            this.bntMultiMeterItemRotator_default_pstRotator.Size = new System.Drawing.Size(63, 23);
            this.bntMultiMeterItemRotator_default_pstRotator.TabIndex = 166;
            this.bntMultiMeterItemRotator_default_pstRotator.Text = "pstRotator";
            this.toolTip1.SetToolTip(this.bntMultiMeterItemRotator_default_pstRotator, "Reset for PST Rotator");
            this.bntMultiMeterItemRotator_default_pstRotator.UseVisualStyleBackColor = true;
            this.bntMultiMeterItemRotator_default_pstRotator.Click += new System.EventHandler(this.bntMultiMeterItemRotator_default_pstRotator_Click);
            // 

            this.picMultiMeterRotatorControlInfo.Location = new System.Drawing.Point(291, 247);
            this.picMultiMeterRotatorControlInfo.Name = "picMultiMeterRotatorControlInfo";
            this.picMultiMeterRotatorControlInfo.Size = new System.Drawing.Size(20, 20);
            this.picMultiMeterRotatorControlInfo.TabIndex = 165;
            this.picMultiMeterRotatorControlInfo.TabStop = false;
            this.toolTip1.SetToolTip(this.picMultiMeterRotatorControlInfo, "%AZ%\r\n%ELE%");
            // 
            this.lblMeterItemRotatorELEcommand.AutoSize = true;
            this.lblMeterItemRotatorELEcommand.Image = null;
            this.lblMeterItemRotatorELEcommand.Location = new System.Drawing.Point(5, 326);
            this.lblMeterItemRotatorELEcommand.Name = "lblMeterItemRotatorELEcommand";
            this.lblMeterItemRotatorELEcommand.Size = new System.Drawing.Size(30, 13);
            this.lblMeterItemRotatorELEcommand.TabIndex = 141;
            this.lblMeterItemRotatorELEcommand.Text = "ELE:";
            // 
            this.lblMeterItemRotatorAZcommand.AutoSize = true;
            this.lblMeterItemRotatorAZcommand.Image = null;
            this.lblMeterItemRotatorAZcommand.Location = new System.Drawing.Point(11, 300);
            this.lblMeterItemRotatorAZcommand.Name = "lblMeterItemRotatorAZcommand";
            this.lblMeterItemRotatorAZcommand.Size = new System.Drawing.Size(24, 13);
            this.lblMeterItemRotatorAZcommand.TabIndex = 140;
            this.lblMeterItemRotatorAZcommand.Text = "AZ:";
            // 
            this.txtMeterItemRotatorELEcommand.Location = new System.Drawing.Point(39, 323);
            this.txtMeterItemRotatorELEcommand.Name = "txtMeterItemRotatorELEcommand";
            this.txtMeterItemRotatorELEcommand.Size = new System.Drawing.Size(272, 20);
            this.txtMeterItemRotatorELEcommand.TabIndex = 139;
            this.toolTip1.SetToolTip(this.txtMeterItemRotatorELEcommand, "The ELE rotator string");
            this.txtMeterItemRotatorELEcommand.TextChanged += new System.EventHandler(this.txtMeterItemRotatorELEcommand_TextChanged);
            // 
            this.txtMeterItemRotatorAZcommand.Location = new System.Drawing.Point(39, 297);
            this.txtMeterItemRotatorAZcommand.Name = "txtMeterItemRotatorAZcommand";
            this.txtMeterItemRotatorAZcommand.Size = new System.Drawing.Size(272, 20);
            this.txtMeterItemRotatorAZcommand.TabIndex = 138;
            this.toolTip1.SetToolTip(this.txtMeterItemRotatorAZcommand, "The AZ rotator string");
            this.txtMeterItemRotatorAZcommand.TextChanged += new System.EventHandler(this.txtMeterItemRotatorAZcommand_TextChanged);
            // 
            this.clrbtnMeterItemRotatorControlColour.Automatic = "Automatic";
            this.clrbtnMeterItemRotatorControlColour.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemRotatorControlColour.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemRotatorControlColour.Image = null;
            this.clrbtnMeterItemRotatorControlColour.Location = new System.Drawing.Point(110, 268);
            this.clrbtnMeterItemRotatorControlColour.MoreColors = "More Colors...";
            this.clrbtnMeterItemRotatorControlColour.Name = "clrbtnMeterItemRotatorControlColour";

            this.clrbtnMeterItemRotatorControlColour.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemRotatorControlColour.TabIndex = 137;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemRotatorControlColour, "Control arrow colour");
            this.clrbtnMeterItemRotatorControlColour.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorControlColour_Changed);
            // 
            this.chkMeterItemRotatorAllowControl.AutoSize = true;
            this.chkMeterItemRotatorAllowControl.Image = null;
            this.chkMeterItemRotatorAllowControl.Location = new System.Drawing.Point(23, 272);
            this.chkMeterItemRotatorAllowControl.Name = "chkMeterItemRotatorAllowControl";
            this.chkMeterItemRotatorAllowControl.Size = new System.Drawing.Size(86, 17);
            this.chkMeterItemRotatorAllowControl.TabIndex = 136;
            this.chkMeterItemRotatorAllowControl.Text = "Allow control";
            this.toolTip1.SetToolTip(this.chkMeterItemRotatorAllowControl, "Allow rotator control. Click/hold/drag on the rotator");
            this.chkMeterItemRotatorAllowControl.UseVisualStyleBackColor = true;
            this.chkMeterItemRotatorAllowControl.CheckedChanged += new System.EventHandler(this.chkMeterItemRotatorAllowControl_CheckedChanged);
            // 
            this.pnlVariableInUse_2_rotator.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_2_rotator.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_2_rotator.BackColor = System.Drawing.Color.Lime;
            this.pnlVariableInUse_2_rotator.Location = new System.Drawing.Point(275, 73);
            this.pnlVariableInUse_2_rotator.Name = "pnlVariableInUse_2_rotator";
            this.pnlVariableInUse_2_rotator.Size = new System.Drawing.Size(28, 6);
            this.pnlVariableInUse_2_rotator.TabIndex = 135;
            // 
            this.chkMeterItemRotatorCardinals.AutoSize = true;
            this.chkMeterItemRotatorCardinals.Image = null;
            this.chkMeterItemRotatorCardinals.Location = new System.Drawing.Point(23, 249);
            this.chkMeterItemRotatorCardinals.Name = "chkMeterItemRotatorCardinals";
            this.chkMeterItemRotatorCardinals.Size = new System.Drawing.Size(69, 17);
            this.chkMeterItemRotatorCardinals.TabIndex = 134;
            this.chkMeterItemRotatorCardinals.Text = "Cardinals";
            this.toolTip1.SetToolTip(this.chkMeterItemRotatorCardinals, "Show cardinals instead of degrees");
            this.chkMeterItemRotatorCardinals.UseVisualStyleBackColor = true;
            this.chkMeterItemRotatorCardinals.CheckedChanged += new System.EventHandler(this.chkMeterItemRotatorCardinals_CheckedChanged);
            // 
            this.labelTS212.AutoSize = true;
            this.labelTS212.Image = null;
            this.labelTS212.Location = new System.Drawing.Point(46, 199);
            this.labelTS212.Name = "labelTS212";
            this.labelTS212.Size = new System.Drawing.Size(31, 13);
            this.labelTS212.TabIndex = 133;
            this.labelTS212.Text = "Text:";
            // 
            this.clrbtnMeterItemRotatorText.Automatic = "Automatic";
            this.clrbtnMeterItemRotatorText.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemRotatorText.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemRotatorText.Image = null;
            this.clrbtnMeterItemRotatorText.Location = new System.Drawing.Point(83, 194);
            this.clrbtnMeterItemRotatorText.MoreColors = "More Colors...";
            this.clrbtnMeterItemRotatorText.Name = "clrbtnMeterItemRotatorText";

            this.clrbtnMeterItemRotatorText.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemRotatorText.TabIndex = 132;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemRotatorText, "Text colour");
            this.clrbtnMeterItemRotatorText.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorText_Changed);
            // 
            this.nudMeterItemRotatorBeamWidth.DecimalPlaces = 1;
            this.nudMeterItemRotatorBeamWidth.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.nudMeterItemRotatorBeamWidth.Location = new System.Drawing.Point(83, 167);
            this.nudMeterItemRotatorBeamWidth.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.nudMeterItemRotatorBeamWidth.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMeterItemRotatorBeamWidth.Name = "nudMeterItemRotatorBeamWidth";
            this.nudMeterItemRotatorBeamWidth.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemRotatorBeamWidth.TabIndex = 131;
            this.nudMeterItemRotatorBeamWidth.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemRotatorBeamWidth, "3dB beam width");
            this.nudMeterItemRotatorBeamWidth.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudMeterItemRotatorBeamWidth.ValueChanged += new System.EventHandler(this.nudMeterItemRotatorBeamWidth_ValueChanged);
            // 
            this.btnMMIO_variable_2_rotator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMMIO_variable_2_rotator.Image = null;
            this.btnMMIO_variable_2_rotator.Location = new System.Drawing.Point(275, 46);
            this.btnMMIO_variable_2_rotator.Name = "btnMMIO_variable_2_rotator";

            this.btnMMIO_variable_2_rotator.Size = new System.Drawing.Size(28, 28);
            this.btnMMIO_variable_2_rotator.TabIndex = 129;
            this.btnMMIO_variable_2_rotator.Text = "%";
            this.btnMMIO_variable_2_rotator.UseVisualStyleBackColor = true;
            this.btnMMIO_variable_2_rotator.Click += new System.EventHandler(this.btnMMIO_variable_2_rotator_Click);
            // 
            this.btnMMIO_variable_rotator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMMIO_variable_rotator.Image = null;
            this.btnMMIO_variable_rotator.Location = new System.Drawing.Point(243, 46);
            this.btnMMIO_variable_rotator.Name = "btnMMIO_variable_rotator";

            this.btnMMIO_variable_rotator.Size = new System.Drawing.Size(28, 28);
            this.btnMMIO_variable_rotator.TabIndex = 128;
            this.btnMMIO_variable_rotator.Text = "%";
            this.btnMMIO_variable_rotator.UseVisualStyleBackColor = true;
            this.btnMMIO_variable_rotator.Click += new System.EventHandler(this.btnMMIO_variable_rotator_Click);
            // 
            this.chkMeterItemRotatorShowBeamWidth.AutoSize = true;
            this.chkMeterItemRotatorShowBeamWidth.Image = null;
            this.chkMeterItemRotatorShowBeamWidth.Location = new System.Drawing.Point(129, 144);
            this.chkMeterItemRotatorShowBeamWidth.Name = "chkMeterItemRotatorShowBeamWidth";
            this.chkMeterItemRotatorShowBeamWidth.Size = new System.Drawing.Size(53, 17);
            this.chkMeterItemRotatorShowBeamWidth.TabIndex = 122;
            this.chkMeterItemRotatorShowBeamWidth.Text = "Show";
            this.toolTip1.SetToolTip(this.chkMeterItemRotatorShowBeamWidth, "Show the 3dB beam width");
            this.chkMeterItemRotatorShowBeamWidth.UseVisualStyleBackColor = true;
            this.chkMeterItemRotatorShowBeamWidth.CheckedChanged += new System.EventHandler(this.chkMeterItemRotatorShowBeamWidth_CheckedChanged);
            // 
            this.labelTS218.AutoSize = true;
            this.labelTS218.Image = null;
            this.labelTS218.Location = new System.Drawing.Point(9, 144);
            this.labelTS218.Name = "labelTS218";
            this.labelTS218.Size = new System.Drawing.Size(68, 13);
            this.labelTS218.TabIndex = 121;
            this.labelTS218.Text = "Beam Width:";
            // 
            this.clrbtnMeterItemRotatorBeamWidth.Automatic = "Automatic";
            this.clrbtnMeterItemRotatorBeamWidth.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemRotatorBeamWidth.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemRotatorBeamWidth.Image = null;
            this.clrbtnMeterItemRotatorBeamWidth.Location = new System.Drawing.Point(83, 139);
            this.clrbtnMeterItemRotatorBeamWidth.MoreColors = "More Colors...";
            this.clrbtnMeterItemRotatorBeamWidth.Name = "clrbtnMeterItemRotatorBeamWidth";

            this.clrbtnMeterItemRotatorBeamWidth.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemRotatorBeamWidth.TabIndex = 120;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemRotatorBeamWidth, "Beam width colour");
            this.clrbtnMeterItemRotatorBeamWidth.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorBeamWidth_Changed);
            // 
            this.chkMeterItemDarkModeRotator.AutoSize = true;
            this.chkMeterItemDarkModeRotator.Image = null;
            this.chkMeterItemDarkModeRotator.Location = new System.Drawing.Point(220, 137);
            this.chkMeterItemDarkModeRotator.Name = "chkMeterItemDarkModeRotator";
            this.chkMeterItemDarkModeRotator.Size = new System.Drawing.Size(79, 17);
            this.chkMeterItemDarkModeRotator.TabIndex = 112;
            this.chkMeterItemDarkModeRotator.Text = "Dark Mode";
            this.chkMeterItemDarkModeRotator.UseVisualStyleBackColor = true;
            this.chkMeterItemDarkModeRotator.CheckedChanged += new System.EventHandler(this.chkMeterItemDarkModeRotator_CheckedChanged);
            // 
            this.nudMeterItemUpdateRateRotator.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMeterItemUpdateRateRotator.Location = new System.Drawing.Point(83, 20);
            this.nudMeterItemUpdateRateRotator.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudMeterItemUpdateRateRotator.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudMeterItemUpdateRateRotator.Name = "nudMeterItemUpdateRateRotator";
            this.nudMeterItemUpdateRateRotator.Size = new System.Drawing.Size(56, 20);
            this.nudMeterItemUpdateRateRotator.TabIndex = 101;
            this.nudMeterItemUpdateRateRotator.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudMeterItemUpdateRateRotator, "Reading update and is related to screen update");
            this.nudMeterItemUpdateRateRotator.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMeterItemUpdateRateRotator.ValueChanged += new System.EventHandler(this.nudMeterItemUpdateRateRotator_ValueChanged);
            // 
            this.labelTS225.Image = null;
            this.labelTS225.Location = new System.Drawing.Point(6, 20);
            this.labelTS225.Name = "labelTS225";
            this.labelTS225.Size = new System.Drawing.Size(71, 16);
            this.labelTS225.TabIndex = 100;
            this.labelTS225.Text = "Update (ms):";
            this.labelTS225.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.labelTS227.AutoSize = true;
            this.labelTS227.Image = null;
            this.labelTS227.Location = new System.Drawing.Point(184, 22);
            this.labelTS227.Name = "labelTS227";
            this.labelTS227.Size = new System.Drawing.Size(68, 13);
            this.labelTS227.TabIndex = 93;
            this.labelTS227.Text = "Background:";
            // 
            this.clrbtnMeterItemHBackgroundRotator.Automatic = "Automatic";
            this.clrbtnMeterItemHBackgroundRotator.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnMeterItemHBackgroundRotator.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemHBackgroundRotator.Image = null;
            this.clrbtnMeterItemHBackgroundRotator.Location = new System.Drawing.Point(263, 17);
            this.clrbtnMeterItemHBackgroundRotator.MoreColors = "More Colors...";
            this.clrbtnMeterItemHBackgroundRotator.Name = "clrbtnMeterItemHBackgroundRotator";

            this.clrbtnMeterItemHBackgroundRotator.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemHBackgroundRotator.TabIndex = 92;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemHBackgroundRotator, "Background colour");
            this.clrbtnMeterItemHBackgroundRotator.Changed += new System.EventHandler(this.clrbtnMeterItemHBackgroundRotator_Changed);
            // 
            this.labelTS228.AutoSize = true;
            this.labelTS228.Image = null;
            this.labelTS228.Location = new System.Drawing.Point(22, 115);
            this.labelTS228.Name = "labelTS228";
            this.labelTS228.Size = new System.Drawing.Size(55, 13);
            this.labelTS228.TabIndex = 81;
            this.labelTS228.Text = "Small Dot:";
            // 
            this.clrbtnMeterItemRotatorSmallDot.Automatic = "Automatic";
            this.clrbtnMeterItemRotatorSmallDot.Color = System.Drawing.Color.Yellow;
            this.clrbtnMeterItemRotatorSmallDot.ForeColor = System.Drawing.Color.Black;
            this.clrbtnMeterItemRotatorSmallDot.Image = null;
            this.clrbtnMeterItemRotatorSmallDot.Location = new System.Drawing.Point(83, 110);
            this.clrbtnMeterItemRotatorSmallDot.MoreColors = "More Colors...";
            this.clrbtnMeterItemRotatorSmallDot.Name = "clrbtnMeterItemRotatorSmallDot";

            this.clrbtnMeterItemRotatorSmallDot.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemRotatorSmallDot.TabIndex = 80;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemRotatorSmallDot, "Small dot colour");
            this.clrbtnMeterItemRotatorSmallDot.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorSmallDot_Changed);
            // 
            this.labelTS229.AutoSize = true;
            this.labelTS229.Image = null;
            this.labelTS229.Location = new System.Drawing.Point(20, 86);
            this.labelTS229.Name = "labelTS229";
            this.labelTS229.Size = new System.Drawing.Size(57, 13);
            this.labelTS229.TabIndex = 79;
            this.labelTS229.Text = "Large Dot:";
            // 
            this.labelTS230.AutoSize = true;
            this.labelTS230.Image = null;
            this.labelTS230.Location = new System.Drawing.Point(40, 57);
            this.labelTS230.Name = "labelTS230";
            this.labelTS230.Size = new System.Drawing.Size(37, 13);
            this.labelTS230.TabIndex = 78;
            this.labelTS230.Text = "Arrow:";
            this.labelTS230.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.clrbtnMeterItemRotatorLargeDot.Automatic = "Automatic";
            this.clrbtnMeterItemRotatorLargeDot.Color = System.Drawing.Color.Red;
            this.clrbtnMeterItemRotatorLargeDot.Image = null;
            this.clrbtnMeterItemRotatorLargeDot.Location = new System.Drawing.Point(83, 81);
            this.clrbtnMeterItemRotatorLargeDot.MoreColors = "More Colors...";
            this.clrbtnMeterItemRotatorLargeDot.Name = "clrbtnMeterItemRotatorLargeDot";

            this.clrbtnMeterItemRotatorLargeDot.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemRotatorLargeDot.TabIndex = 77;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemRotatorLargeDot, "Large dot colour");
            this.clrbtnMeterItemRotatorLargeDot.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorLargeDot_Changed);
            // 
            this.clrbtnMeterItemRotatorArrow.Automatic = "Automatic";
            this.clrbtnMeterItemRotatorArrow.Color = System.Drawing.Color.White;
            this.clrbtnMeterItemRotatorArrow.Image = null;
            this.clrbtnMeterItemRotatorArrow.Location = new System.Drawing.Point(83, 52);
            this.clrbtnMeterItemRotatorArrow.MoreColors = "More Colors...";
            this.clrbtnMeterItemRotatorArrow.Name = "clrbtnMeterItemRotatorArrow";

            this.clrbtnMeterItemRotatorArrow.Size = new System.Drawing.Size(40, 23);
            this.clrbtnMeterItemRotatorArrow.TabIndex = 76;
            this.toolTip1.SetToolTip(this.clrbtnMeterItemRotatorArrow, "The arrow/pointer colour");
            this.clrbtnMeterItemRotatorArrow.Changed += new System.EventHandler(this.clrbtnMeterItemRotatorArrow_Changed);
            // 
            this.chkMeterItemFadeOnTxRotator.AutoSize = true;
            this.chkMeterItemFadeOnTxRotator.Image = null;
            this.chkMeterItemFadeOnTxRotator.Location = new System.Drawing.Point(220, 110);
            this.chkMeterItemFadeOnTxRotator.Name = "chkMeterItemFadeOnTxRotator";
            this.chkMeterItemFadeOnTxRotator.Size = new System.Drawing.Size(82, 17);
            this.chkMeterItemFadeOnTxRotator.TabIndex = 1;
            this.chkMeterItemFadeOnTxRotator.Text = "Fade on TX";
            this.chkMeterItemFadeOnTxRotator.UseVisualStyleBackColor = true;
            this.chkMeterItemFadeOnTxRotator.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnTxRotator_CheckedChanged);
            // 
            this.chkMeterItemFadeOnRxRotator.AutoSize = true;
            this.chkMeterItemFadeOnRxRotator.Image = null;
            this.chkMeterItemFadeOnRxRotator.Location = new System.Drawing.Point(220, 87);
            this.chkMeterItemFadeOnRxRotator.Name = "chkMeterItemFadeOnRxRotator";
            this.chkMeterItemFadeOnRxRotator.Size = new System.Drawing.Size(83, 17);
            this.chkMeterItemFadeOnRxRotator.TabIndex = 0;
            this.chkMeterItemFadeOnRxRotator.Text = "Fade on RX";
            this.chkMeterItemFadeOnRxRotator.UseVisualStyleBackColor = true;
            this.chkMeterItemFadeOnRxRotator.CheckedChanged += new System.EventHandler(this.chkMeterItemFadeOnRxRotator_CheckedChanged);
            // 
            this.pnlVariableInUse_1_rotator.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_1_rotator.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_1_rotator.BackColor = System.Drawing.Color.Lime;
            this.pnlVariableInUse_1_rotator.Location = new System.Drawing.Point(243, 73);
            this.pnlVariableInUse_1_rotator.Name = "pnlVariableInUse_1_rotator";
            this.pnlVariableInUse_1_rotator.Size = new System.Drawing.Size(28, 6);
            this.pnlVariableInUse_1_rotator.TabIndex = 130;
            // 
            this.grpLedIndicator.Controls.Add(this.radLed_light_pulsate);
            this.grpLedIndicator.Controls.Add(this.radLed_light_blink);
            this.grpLedIndicator.Controls.Add(this.radLed_light_on_off);
            this.grpLedIndicator.Controls.Add(this.chkLed_show_false);
            this.grpLedIndicator.Controls.Add(this.chkLed_show_true);
            this.grpLedIndicator.Controls.Add(this.lblLed_Error);
            this.grpLedIndicator.Controls.Add(this.lblLed_Valid);
            this.grpLedIndicator.Controls.Add(this.btnLedIndicator_copy_truefalse_colours);
            this.grpLedIndicator.Controls.Add(this.pbLedIndicator_condition_tips);
            this.grpLedIndicator.Controls.Add(this.lblLedIndicator_panelbackgroundTX);
            this.grpLedIndicator.Controls.Add(this.clrbtnLedIndicator_PanelBackgroundTX);
            this.grpLedIndicator.Controls.Add(this.labelTS219);
            this.grpLedIndicator.Controls.Add(this.labelTS220);
            this.grpLedIndicator.Controls.Add(this.clrbtnLedIndicator_false);
            this.grpLedIndicator.Controls.Add(this.clrbtnLedIndicator_true);
            this.grpLedIndicator.Controls.Add(this.btnLedIndicator_copy_sizex_to_y);
            this.grpLedIndicator.Controls.Add(this.labelTS221);
            this.grpLedIndicator.Controls.Add(this.labelTS222);
            this.grpLedIndicator.Controls.Add(this.nudLedIndicator_ySize);
            this.grpLedIndicator.Controls.Add(this.labelTS223);
            this.grpLedIndicator.Controls.Add(this.nudLedIndicator_xSize);
            this.grpLedIndicator.Controls.Add(this.labelTS224);
            this.grpLedIndicator.Controls.Add(this.labelTS226);
            this.grpLedIndicator.Controls.Add(this.nudLedIndicator_yOffset);
            this.grpLedIndicator.Controls.Add(this.labelTS231);
            this.grpLedIndicator.Controls.Add(this.nudLedIndicator_xOffset);
            this.grpLedIndicator.Controls.Add(this.labelTS233);
            this.grpLedIndicator.Controls.Add(this.txtLedIndicator_condition);
            this.grpLedIndicator.Controls.Add(this.chkLedIndicator_ShowPanel);
            this.grpLedIndicator.Controls.Add(this.nudLedIndicator_PanelPadding);
            this.grpLedIndicator.Controls.Add(this.labelTS234);
            this.grpLedIndicator.Controls.Add(this.lblLedIndicator_panelbackground);
            this.grpLedIndicator.Controls.Add(this.clrbtnLedIndicator_PanelBackground);
            this.grpLedIndicator.Controls.Add(this.chkLedIndicator_FadeOnTX);
            this.grpLedIndicator.Controls.Add(this.chkLedIndicator_FadeOnRX);
            this.grpLedIndicator.Location = new System.Drawing.Point(12, 23);
            this.grpLedIndicator.Name = "grpLedIndicator";
            this.grpLedIndicator.Size = new System.Drawing.Size(323, 376);
            this.grpLedIndicator.TabIndex = 107;
            this.grpLedIndicator.TabStop = false;
            this.grpLedIndicator.Text = "Led Indicator";
            this.grpLedIndicator.Visible = false;
            // 
            this.radLed_light_pulsate.AutoSize = true;
            this.radLed_light_pulsate.Image = null;
            this.radLed_light_pulsate.Location = new System.Drawing.Point(219, 243);
            this.radLed_light_pulsate.Name = "radLed_light_pulsate";
            this.radLed_light_pulsate.Size = new System.Drawing.Size(60, 17);
            this.radLed_light_pulsate.TabIndex = 172;
            this.radLed_light_pulsate.TabStop = true;
            this.radLed_light_pulsate.Text = "Pulsate";
            this.radLed_light_pulsate.UseVisualStyleBackColor = true;
            this.radLed_light_pulsate.CheckedChanged += new System.EventHandler(this.radLed_light_pulsate_CheckedChanged);
            // 
            this.radLed_light_blink.AutoSize = true;
            this.radLed_light_blink.Image = null;
            this.radLed_light_blink.Location = new System.Drawing.Point(219, 220);
            this.radLed_light_blink.Name = "radLed_light_blink";
            this.radLed_light_blink.Size = new System.Drawing.Size(48, 17);
            this.radLed_light_blink.TabIndex = 171;
            this.radLed_light_blink.TabStop = true;
            this.radLed_light_blink.Text = "Blink";
            this.radLed_light_blink.UseVisualStyleBackColor = true;
            this.radLed_light_blink.CheckedChanged += new System.EventHandler(this.radLed_light_blink_CheckedChanged);
            // 
            this.radLed_light_on_off.AutoSize = true;
            this.radLed_light_on_off.Image = null;
            this.radLed_light_on_off.Location = new System.Drawing.Point(219, 197);
            this.radLed_light_on_off.Name = "radLed_light_on_off";
            this.radLed_light_on_off.Size = new System.Drawing.Size(58, 17);
            this.radLed_light_on_off.TabIndex = 170;
            this.radLed_light_on_off.TabStop = true;
            this.radLed_light_on_off.Text = "On/Off";
            this.radLed_light_on_off.UseVisualStyleBackColor = true;
            this.radLed_light_on_off.CheckedChanged += new System.EventHandler(this.radLed_light_on_off_CheckedChanged);
            // 
            this.chkLed_show_false.AutoSize = true;
            this.chkLed_show_false.Image = null;
            this.chkLed_show_false.Location = new System.Drawing.Point(154, 189);
            this.chkLed_show_false.Name = "chkLed_show_false";
            this.chkLed_show_false.Size = new System.Drawing.Size(53, 17);
            this.chkLed_show_false.TabIndex = 169;
            this.chkLed_show_false.Text = "Show";
            this.chkLed_show_false.UseVisualStyleBackColor = true;
            this.chkLed_show_false.CheckedChanged += new System.EventHandler(this.chkLed_show_false_CheckedChanged);
            // 
            this.chkLed_show_true.AutoSize = true;
            this.chkLed_show_true.Image = null;
            this.chkLed_show_true.Location = new System.Drawing.Point(154, 166);
            this.chkLed_show_true.Name = "chkLed_show_true";
            this.chkLed_show_true.Size = new System.Drawing.Size(53, 17);
            this.chkLed_show_true.TabIndex = 168;
            this.chkLed_show_true.Text = "Show";
            this.chkLed_show_true.UseVisualStyleBackColor = true;
            this.chkLed_show_true.CheckedChanged += new System.EventHandler(this.chkLed_show_true_CheckedChanged);
            // 
            this.lblLed_Error.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLed_Error.Image = null;
            this.lblLed_Error.Location = new System.Drawing.Point(271, 174);
            this.lblLed_Error.Name = "lblLed_Error";
            this.lblLed_Error.Size = new System.Drawing.Size(46, 13);
            this.lblLed_Error.TabIndex = 167;
            this.lblLed_Error.Text = "error";
            this.lblLed_Error.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.lblLed_Valid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLed_Valid.Image = null;
            this.lblLed_Valid.Location = new System.Drawing.Point(271, 159);
            this.lblLed_Valid.Name = "lblLed_Valid";
            this.lblLed_Valid.Size = new System.Drawing.Size(46, 15);
            this.lblLed_Valid.TabIndex = 166;
            this.lblLed_Valid.Text = "valid";
            this.lblLed_Valid.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.btnLedIndicator_copy_truefalse_colours.Image = null;
            this.btnLedIndicator_copy_truefalse_colours.Location = new System.Drawing.Point(115, 171);
            this.btnLedIndicator_copy_truefalse_colours.Name = "btnLedIndicator_copy_truefalse_colours";

            this.btnLedIndicator_copy_truefalse_colours.Size = new System.Drawing.Size(33, 23);
            this.btnLedIndicator_copy_truefalse_colours.TabIndex = 165;
            this.btnLedIndicator_copy_truefalse_colours.Text = "=";
            this.toolTip1.SetToolTip(this.btnLedIndicator_copy_truefalse_colours, "Copy the font and colours from RX to TX");
            this.btnLedIndicator_copy_truefalse_colours.UseVisualStyleBackColor = true;
            this.btnLedIndicator_copy_truefalse_colours.Click += new System.EventHandler(this.btnLedIndicator_copy_truefalse_colours_Click);
            // 

            this.pbLedIndicator_condition_tips.Location = new System.Drawing.Point(297, 100);
            this.pbLedIndicator_condition_tips.Name = "pbLedIndicator_condition_tips";
            this.pbLedIndicator_condition_tips.Size = new System.Drawing.Size(20, 20);
            this.pbLedIndicator_condition_tips.TabIndex = 164;
            this.pbLedIndicator_condition_tips.TabStop = false;
            this.toolTip1.SetToolTip(this.pbLedIndicator_condition_tips, "You can create code based conditions.\r\neg.\r\n%swr% > 1.5\r\n%variable% == 2\r\n%variab" +
        "le% == \"Hello\"\r\n%pwr% > 15\r\n%variable1% == %variable2%\r\n%pwr% <= 10");
            // 
            this.lblLedIndicator_panelbackgroundTX.AutoSize = true;
            this.lblLedIndicator_panelbackgroundTX.Image = null;
            this.lblLedIndicator_panelbackgroundTX.Location = new System.Drawing.Point(50, 75);
            this.lblLedIndicator_panelbackgroundTX.Name = "lblLedIndicator_panelbackgroundTX";
            this.lblLedIndicator_panelbackgroundTX.Size = new System.Drawing.Size(85, 13);
            this.lblLedIndicator_panelbackgroundTX.TabIndex = 163;
            this.lblLedIndicator_panelbackgroundTX.Text = "TX Background:";
            // 
            this.clrbtnLedIndicator_PanelBackgroundTX.Automatic = "Automatic";
            this.clrbtnLedIndicator_PanelBackgroundTX.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnLedIndicator_PanelBackgroundTX.ForeColor = System.Drawing.Color.Black;
            this.clrbtnLedIndicator_PanelBackgroundTX.Image = null;
            this.clrbtnLedIndicator_PanelBackgroundTX.Location = new System.Drawing.Point(141, 70);
            this.clrbtnLedIndicator_PanelBackgroundTX.MoreColors = "More Colors...";
            this.clrbtnLedIndicator_PanelBackgroundTX.Name = "clrbtnLedIndicator_PanelBackgroundTX";

            this.clrbtnLedIndicator_PanelBackgroundTX.Size = new System.Drawing.Size(40, 23);
            this.clrbtnLedIndicator_PanelBackgroundTX.TabIndex = 162;
            this.toolTip1.SetToolTip(this.clrbtnLedIndicator_PanelBackgroundTX, "Background colour");
            this.clrbtnLedIndicator_PanelBackgroundTX.Changed += new System.EventHandler(this.clrbtnLedIndicator_PanelBackgroundTX_Changed);
            // 
            this.labelTS219.AutoSize = true;
            this.labelTS219.Image = null;
            this.labelTS219.Location = new System.Drawing.Point(24, 193);
            this.labelTS219.Name = "labelTS219";
            this.labelTS219.Size = new System.Drawing.Size(35, 13);
            this.labelTS219.TabIndex = 161;
            this.labelTS219.Text = "False:";
            // 
            this.labelTS220.AutoSize = true;
            this.labelTS220.Image = null;
            this.labelTS220.Location = new System.Drawing.Point(27, 166);
            this.labelTS220.Name = "labelTS220";
            this.labelTS220.Size = new System.Drawing.Size(32, 13);
            this.labelTS220.TabIndex = 160;
            this.labelTS220.Text = "True:";
            // 
            this.clrbtnLedIndicator_false.Automatic = "Automatic";
            this.clrbtnLedIndicator_false.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnLedIndicator_false.ForeColor = System.Drawing.Color.Black;
            this.clrbtnLedIndicator_false.Image = null;
            this.clrbtnLedIndicator_false.Location = new System.Drawing.Point(65, 188);
            this.clrbtnLedIndicator_false.MoreColors = "More Colors...";
            this.clrbtnLedIndicator_false.Name = "clrbtnLedIndicator_false";

            this.clrbtnLedIndicator_false.Size = new System.Drawing.Size(40, 23);
            this.clrbtnLedIndicator_false.TabIndex = 157;
            this.toolTip1.SetToolTip(this.clrbtnLedIndicator_false, "Background colour");
            this.clrbtnLedIndicator_false.Changed += new System.EventHandler(this.clrbtnLedIndicator_false_Changed);
            // 
            this.clrbtnLedIndicator_true.Automatic = "Automatic";
            this.clrbtnLedIndicator_true.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnLedIndicator_true.ForeColor = System.Drawing.Color.Black;
            this.clrbtnLedIndicator_true.Image = null;
            this.clrbtnLedIndicator_true.Location = new System.Drawing.Point(65, 162);
            this.clrbtnLedIndicator_true.MoreColors = "More Colors...";
            this.clrbtnLedIndicator_true.Name = "clrbtnLedIndicator_true";

            this.clrbtnLedIndicator_true.Size = new System.Drawing.Size(40, 23);
            this.clrbtnLedIndicator_true.TabIndex = 156;
            this.toolTip1.SetToolTip(this.clrbtnLedIndicator_true, "Background colour");
            this.clrbtnLedIndicator_true.Changed += new System.EventHandler(this.clrbtnLedIndicator_true_Changed);
            // 
            this.btnLedIndicator_copy_sizex_to_y.Image = null;
            this.btnLedIndicator_copy_sizex_to_y.Location = new System.Drawing.Point(182, 313);
            this.btnLedIndicator_copy_sizex_to_y.Name = "btnLedIndicator_copy_sizex_to_y";

            this.btnLedIndicator_copy_sizex_to_y.Size = new System.Drawing.Size(33, 23);
            this.btnLedIndicator_copy_sizex_to_y.TabIndex = 155;
            this.btnLedIndicator_copy_sizex_to_y.Text = "=";
            this.toolTip1.SetToolTip(this.btnLedIndicator_copy_sizex_to_y, "Copy the offset values from RX to TX");
            this.btnLedIndicator_copy_sizex_to_y.UseVisualStyleBackColor = true;
            this.btnLedIndicator_copy_sizex_to_y.Click += new System.EventHandler(this.btnLedIndicator_copy_sizex_to_y_Click);
            // 
            this.labelTS221.AutoSize = true;
            this.labelTS221.Image = null;
            this.labelTS221.Location = new System.Drawing.Point(163, 330);
            this.labelTS221.Name = "labelTS221";
            this.labelTS221.Size = new System.Drawing.Size(12, 13);
            this.labelTS221.TabIndex = 154;
            this.labelTS221.Text = "y";
            // 
            this.labelTS222.AutoSize = true;
            this.labelTS222.Image = null;
            this.labelTS222.Location = new System.Drawing.Point(163, 304);
            this.labelTS222.Name = "labelTS222";
            this.labelTS222.Size = new System.Drawing.Size(12, 13);
            this.labelTS222.TabIndex = 153;
            this.labelTS222.Text = "x";
            // 
            this.nudLedIndicator_ySize.DecimalPlaces = 3;
            this.nudLedIndicator_ySize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudLedIndicator_ySize.Location = new System.Drawing.Point(101, 328);
            this.nudLedIndicator_ySize.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudLedIndicator_ySize.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudLedIndicator_ySize.Name = "nudLedIndicator_ySize";
            this.nudLedIndicator_ySize.Size = new System.Drawing.Size(56, 20);
            this.nudLedIndicator_ySize.TabIndex = 152;
            this.nudLedIndicator_ySize.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudLedIndicator_ySize, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudLedIndicator_ySize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLedIndicator_ySize.ValueChanged += new System.EventHandler(this.nudLedIndicator_ySize_ValueChanged);
            // 
            this.labelTS223.AutoSize = true;
            this.labelTS223.Image = null;
            this.labelTS223.Location = new System.Drawing.Point(62, 304);
            this.labelTS223.Name = "labelTS223";
            this.labelTS223.Size = new System.Drawing.Size(30, 13);
            this.labelTS223.TabIndex = 151;
            this.labelTS223.Text = "Size:";
            // 
            this.nudLedIndicator_xSize.DecimalPlaces = 3;
            this.nudLedIndicator_xSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudLedIndicator_xSize.Location = new System.Drawing.Point(101, 302);
            this.nudLedIndicator_xSize.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudLedIndicator_xSize.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudLedIndicator_xSize.Name = "nudLedIndicator_xSize";
            this.nudLedIndicator_xSize.Size = new System.Drawing.Size(56, 20);
            this.nudLedIndicator_xSize.TabIndex = 150;
            this.nudLedIndicator_xSize.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudLedIndicator_xSize, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudLedIndicator_xSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLedIndicator_xSize.ValueChanged += new System.EventHandler(this.nudLedIndicator_xSize_ValueChanged);
            // 
            this.labelTS224.AutoSize = true;
            this.labelTS224.Image = null;
            this.labelTS224.Location = new System.Drawing.Point(163, 278);
            this.labelTS224.Name = "labelTS224";
            this.labelTS224.Size = new System.Drawing.Size(12, 13);
            this.labelTS224.TabIndex = 149;
            this.labelTS224.Text = "y";
            // 
            this.labelTS226.AutoSize = true;
            this.labelTS226.Image = null;
            this.labelTS226.Location = new System.Drawing.Point(163, 252);
            this.labelTS226.Name = "labelTS226";
            this.labelTS226.Size = new System.Drawing.Size(12, 13);
            this.labelTS226.TabIndex = 148;
            this.labelTS226.Text = "x";
            // 
            this.nudLedIndicator_yOffset.DecimalPlaces = 3;
            this.nudLedIndicator_yOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudLedIndicator_yOffset.Location = new System.Drawing.Point(101, 276);
            this.nudLedIndicator_yOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudLedIndicator_yOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudLedIndicator_yOffset.Name = "nudLedIndicator_yOffset";
            this.nudLedIndicator_yOffset.Size = new System.Drawing.Size(56, 20);
            this.nudLedIndicator_yOffset.TabIndex = 147;
            this.nudLedIndicator_yOffset.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudLedIndicator_yOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudLedIndicator_yOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLedIndicator_yOffset.ValueChanged += new System.EventHandler(this.nudLedIndicator_yOffset_ValueChanged);
            // 
            this.labelTS231.AutoSize = true;
            this.labelTS231.Image = null;
            this.labelTS231.Location = new System.Drawing.Point(10, 252);
            this.labelTS231.Name = "labelTS231";
            this.labelTS231.Size = new System.Drawing.Size(83, 13);
            this.labelTS231.TabIndex = 145;
            this.labelTS231.Text = "Position Offsets:";
            // 
            this.nudLedIndicator_xOffset.DecimalPlaces = 3;
            this.nudLedIndicator_xOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudLedIndicator_xOffset.Location = new System.Drawing.Point(101, 250);
            this.nudLedIndicator_xOffset.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudLedIndicator_xOffset.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.nudLedIndicator_xOffset.Name = "nudLedIndicator_xOffset";
            this.nudLedIndicator_xOffset.Size = new System.Drawing.Size(56, 20);
            this.nudLedIndicator_xOffset.TabIndex = 143;
            this.nudLedIndicator_xOffset.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudLedIndicator_xOffset, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudLedIndicator_xOffset.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLedIndicator_xOffset.ValueChanged += new System.EventHandler(this.nudLedIndicator_xOffset_ValueChanged);
            // 
            this.labelTS233.AutoSize = true;
            this.labelTS233.Image = null;
            this.labelTS233.Location = new System.Drawing.Point(5, 133);
            this.labelTS233.Name = "labelTS233";
            this.labelTS233.Size = new System.Drawing.Size(54, 13);
            this.labelTS233.TabIndex = 138;
            this.labelTS233.Text = "Condition:";
            // 
            this.txtLedIndicator_condition.Location = new System.Drawing.Point(65, 130);
            this.txtLedIndicator_condition.Name = "txtLedIndicator_condition";
            this.txtLedIndicator_condition.Size = new System.Drawing.Size(252, 20);
            this.txtLedIndicator_condition.TabIndex = 136;
            this.txtLedIndicator_condition.TextChanged += new System.EventHandler(this.txtLedIndicator_condition_TextChanged);
            // 
            this.chkLedIndicator_ShowPanel.AutoSize = true;
            this.chkLedIndicator_ShowPanel.Image = null;
            this.chkLedIndicator_ShowPanel.Location = new System.Drawing.Point(18, 30);
            this.chkLedIndicator_ShowPanel.Name = "chkLedIndicator_ShowPanel";
            this.chkLedIndicator_ShowPanel.Size = new System.Drawing.Size(83, 17);
            this.chkLedIndicator_ShowPanel.TabIndex = 135;
            this.chkLedIndicator_ShowPanel.Text = "Show Panel";
            this.chkLedIndicator_ShowPanel.UseVisualStyleBackColor = true;
            this.chkLedIndicator_ShowPanel.CheckedChanged += new System.EventHandler(this.chkLedIndicator_ShowPanel_CheckedChanged);
            // 
            this.nudLedIndicator_PanelPadding.DecimalPlaces = 3;
            this.nudLedIndicator_PanelPadding.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudLedIndicator_PanelPadding.Location = new System.Drawing.Point(125, 100);
            this.nudLedIndicator_PanelPadding.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudLedIndicator_PanelPadding.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudLedIndicator_PanelPadding.Name = "nudLedIndicator_PanelPadding";
            this.nudLedIndicator_PanelPadding.Size = new System.Drawing.Size(56, 20);
            this.nudLedIndicator_PanelPadding.TabIndex = 132;
            this.nudLedIndicator_PanelPadding.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudLedIndicator_PanelPadding, "Size of the spacer. The number is a ratio with reference to the width.");
            this.nudLedIndicator_PanelPadding.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudLedIndicator_PanelPadding.ValueChanged += new System.EventHandler(this.nudLedIndicator_PanelPadding_ValueChanged);
            // 
            this.labelTS234.AutoSize = true;
            this.labelTS234.Image = null;
            this.labelTS234.Location = new System.Drawing.Point(39, 102);
            this.labelTS234.Name = "labelTS234";
            this.labelTS234.Size = new System.Drawing.Size(79, 13);
            this.labelTS234.TabIndex = 131;
            this.labelTS234.Text = "Panel Padding:";
            // 
            this.lblLedIndicator_panelbackground.AutoSize = true;
            this.lblLedIndicator_panelbackground.Image = null;
            this.lblLedIndicator_panelbackground.Location = new System.Drawing.Point(49, 50);
            this.lblLedIndicator_panelbackground.Name = "lblLedIndicator_panelbackground";
            this.lblLedIndicator_panelbackground.Size = new System.Drawing.Size(86, 13);
            this.lblLedIndicator_panelbackground.TabIndex = 130;
            this.lblLedIndicator_panelbackground.Text = "RX Background:";
            // 
            this.clrbtnLedIndicator_PanelBackground.Automatic = "Automatic";
            this.clrbtnLedIndicator_PanelBackground.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnLedIndicator_PanelBackground.ForeColor = System.Drawing.Color.Black;
            this.clrbtnLedIndicator_PanelBackground.Image = null;
            this.clrbtnLedIndicator_PanelBackground.Location = new System.Drawing.Point(141, 45);
            this.clrbtnLedIndicator_PanelBackground.MoreColors = "More Colors...";
            this.clrbtnLedIndicator_PanelBackground.Name = "clrbtnLedIndicator_PanelBackground";

            this.clrbtnLedIndicator_PanelBackground.Size = new System.Drawing.Size(40, 23);
            this.clrbtnLedIndicator_PanelBackground.TabIndex = 129;
            this.toolTip1.SetToolTip(this.clrbtnLedIndicator_PanelBackground, "Background colour");
            this.clrbtnLedIndicator_PanelBackground.Changed += new System.EventHandler(this.clrbtnLedIndicator_PanelBackground_Changed);
            // 
            this.chkLedIndicator_FadeOnTX.AutoSize = true;
            this.chkLedIndicator_FadeOnTX.Image = null;
            this.chkLedIndicator_FadeOnTX.Location = new System.Drawing.Point(219, 53);
            this.chkLedIndicator_FadeOnTX.Name = "chkLedIndicator_FadeOnTX";
            this.chkLedIndicator_FadeOnTX.Size = new System.Drawing.Size(82, 17);
            this.chkLedIndicator_FadeOnTX.TabIndex = 3;
            this.chkLedIndicator_FadeOnTX.Text = "Fade on TX";
            this.chkLedIndicator_FadeOnTX.UseVisualStyleBackColor = true;
            this.chkLedIndicator_FadeOnTX.CheckedChanged += new System.EventHandler(this.chkLedIndicator_FadeOnTX_CheckedChanged);
            // 
            this.chkLedIndicator_FadeOnRX.AutoSize = true;
            this.chkLedIndicator_FadeOnRX.Image = null;
            this.chkLedIndicator_FadeOnRX.Location = new System.Drawing.Point(219, 30);
            this.chkLedIndicator_FadeOnRX.Name = "chkLedIndicator_FadeOnRX";
            this.chkLedIndicator_FadeOnRX.Size = new System.Drawing.Size(83, 17);
            this.chkLedIndicator_FadeOnRX.TabIndex = 2;
            this.chkLedIndicator_FadeOnRX.Text = "Fade on RX";
            this.chkLedIndicator_FadeOnRX.UseVisualStyleBackColor = true;
            this.chkLedIndicator_FadeOnRX.CheckedChanged += new System.EventHandler(this.chkLedIndicator_FadeOnRX_CheckedChanged);
            // 
            this.grpWebImage.Controls.Add(this.chkWebImage_bypass_cache);
            this.grpWebImage.Controls.Add(this.groupBoxTS42);
            this.grpWebImage.Controls.Add(this.groupBoxTS41);
            this.grpWebImage.Controls.Add(this.groupBoxTS43);
            this.grpWebImage.Controls.Add(this.lblWebImage_state);
            this.grpWebImage.Controls.Add(this.groupBoxTS40);
            this.grpWebImage.Controls.Add(this.labelTS236);
            this.grpWebImage.Controls.Add(this.txtWebImage_url);
            this.grpWebImage.Controls.Add(this.nudWebImage_update_interval);
            this.grpWebImage.Controls.Add(this.labelTS232);
            this.grpWebImage.Controls.Add(this.nudWebImage_width_scale);
            this.grpWebImage.Controls.Add(this.labelTS235);
            this.grpWebImage.Controls.Add(this.chkWebImage_fade_tx);
            this.grpWebImage.Controls.Add(this.chkWebImage_fade_rx);
            this.grpWebImage.Location = new System.Drawing.Point(12, 16);
            this.grpWebImage.Name = "grpWebImage";
            this.grpWebImage.Size = new System.Drawing.Size(323, 376);
            this.grpWebImage.TabIndex = 108;
            this.grpWebImage.TabStop = false;
            this.grpWebImage.Text = "Web Image";
            this.grpWebImage.Visible = false;
            // 
            this.chkWebImage_bypass_cache.AutoSize = true;
            this.chkWebImage_bypass_cache.Image = null;
            this.chkWebImage_bypass_cache.Location = new System.Drawing.Point(221, 63);
            this.chkWebImage_bypass_cache.Name = "chkWebImage_bypass_cache";
            this.chkWebImage_bypass_cache.Size = new System.Drawing.Size(94, 17);
            this.chkWebImage_bypass_cache.TabIndex = 146;
            this.chkWebImage_bypass_cache.Text = "Bypass Cache";
            this.toolTip1.SetToolTip(this.chkWebImage_bypass_cache, "Append a unique id to the url each request, bypassing most servers caching polici" +
        "es");
            this.chkWebImage_bypass_cache.UseVisualStyleBackColor = true;
            this.chkWebImage_bypass_cache.CheckedChanged += new System.EventHandler(this.chkWebImage_bypass_cache_CheckedChanged);
            // 
            this.groupBoxTS42.Controls.Add(this.buttonTS1);
            this.groupBoxTS42.Controls.Add(this.comboWebImage_noaa);
            this.groupBoxTS42.Location = new System.Drawing.Point(16, 253);
            this.groupBoxTS42.Name = "groupBoxTS42";
            this.groupBoxTS42.Size = new System.Drawing.Size(293, 44);
            this.groupBoxTS42.TabIndex = 145;
            this.groupBoxTS42.TabStop = false;
            this.groupBoxTS42.Text = "noaa";
            // 
            this.buttonTS1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.buttonTS1.Image = null;
            this.buttonTS1.Location = new System.Drawing.Point(208, 12);
            this.buttonTS1.Name = "buttonTS1";

            this.buttonTS1.Size = new System.Drawing.Size(75, 24);
            this.buttonTS1.TabIndex = 1;
            this.buttonTS1.Text = "Visit";
            this.buttonTS1.UseVisualStyleBackColor = false;
            this.buttonTS1.Visible = false;
            // 
            this.comboWebImage_noaa.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWebImage_noaa.FormattingEnabled = true;
            this.comboWebImage_noaa.Location = new System.Drawing.Point(12, 15);
            this.comboWebImage_noaa.Name = "comboWebImage_noaa";
            this.comboWebImage_noaa.Size = new System.Drawing.Size(190, 21);
            this.comboWebImage_noaa.TabIndex = 0;
            this.comboWebImage_noaa.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_noaa_SelectedIndexChanged);
            // 
            this.groupBoxTS41.Controls.Add(this.btnWebImage_bsdworld_visit);
            this.groupBoxTS41.Controls.Add(this.comboWebImage_BsdWorld);
            this.groupBoxTS41.Location = new System.Drawing.Point(16, 165);
            this.groupBoxTS41.Name = "groupBoxTS41";
            this.groupBoxTS41.Size = new System.Drawing.Size(293, 44);
            this.groupBoxTS41.TabIndex = 143;
            this.groupBoxTS41.TabStop = false;
            this.groupBoxTS41.Text = "bsdworld.org";
            // 
            this.btnWebImage_bsdworld_visit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnWebImage_bsdworld_visit.Image = null;
            this.btnWebImage_bsdworld_visit.Location = new System.Drawing.Point(208, 12);
            this.btnWebImage_bsdworld_visit.Name = "btnWebImage_bsdworld_visit";

            this.btnWebImage_bsdworld_visit.Size = new System.Drawing.Size(75, 24);
            this.btnWebImage_bsdworld_visit.TabIndex = 1;
            this.btnWebImage_bsdworld_visit.Text = "Visit";
            this.btnWebImage_bsdworld_visit.UseVisualStyleBackColor = false;
            this.btnWebImage_bsdworld_visit.Click += new System.EventHandler(this.btnWebImage_bsdworld_visit_Click);
            // 
            this.comboWebImage_BsdWorld.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWebImage_BsdWorld.FormattingEnabled = true;
            this.comboWebImage_BsdWorld.Location = new System.Drawing.Point(12, 15);
            this.comboWebImage_BsdWorld.Name = "comboWebImage_BsdWorld";
            this.comboWebImage_BsdWorld.Size = new System.Drawing.Size(190, 21);
            this.comboWebImage_BsdWorld.TabIndex = 0;
            this.comboWebImage_BsdWorld.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_BsdWorld_SelectedIndexChanged);
            // 
            this.groupBoxTS43.Controls.Add(this.buttonTS2);
            this.groupBoxTS43.Controls.Add(this.comboWebImage_nasa);
            this.groupBoxTS43.Location = new System.Drawing.Point(16, 209);
            this.groupBoxTS43.Name = "groupBoxTS43";
            this.groupBoxTS43.Size = new System.Drawing.Size(293, 44);
            this.groupBoxTS43.TabIndex = 144;
            this.groupBoxTS43.TabStop = false;
            this.groupBoxTS43.Text = "nasa";
            // 
            this.buttonTS2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.buttonTS2.Image = null;
            this.buttonTS2.Location = new System.Drawing.Point(208, 12);
            this.buttonTS2.Name = "buttonTS2";

            this.buttonTS2.Size = new System.Drawing.Size(75, 24);
            this.buttonTS2.TabIndex = 1;
            this.buttonTS2.Text = "Visit";
            this.buttonTS2.UseVisualStyleBackColor = false;
            this.buttonTS2.Visible = false;
            // 
            this.comboWebImage_nasa.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWebImage_nasa.FormattingEnabled = true;
            this.comboWebImage_nasa.Location = new System.Drawing.Point(12, 15);
            this.comboWebImage_nasa.Name = "comboWebImage_nasa";
            this.comboWebImage_nasa.Size = new System.Drawing.Size(190, 21);
            this.comboWebImage_nasa.TabIndex = 0;
            this.comboWebImage_nasa.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_nasa_SelectedIndexChanged);
            // 
            this.lblWebImage_state.Image = null;
            this.lblWebImage_state.Location = new System.Drawing.Point(220, 108);
            this.lblWebImage_state.Name = "lblWebImage_state";
            this.lblWebImage_state.Size = new System.Drawing.Size(93, 13);
            this.lblWebImage_state.TabIndex = 142;
            this.lblWebImage_state.Text = "state";
            this.lblWebImage_state.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.groupBoxTS40.Controls.Add(this.btnWebImage_hamqsl_donate);
            this.groupBoxTS40.Controls.Add(this.comboWebImage_HamQsl);
            this.groupBoxTS40.Location = new System.Drawing.Point(16, 121);
            this.groupBoxTS40.Name = "groupBoxTS40";
            this.groupBoxTS40.Size = new System.Drawing.Size(293, 44);
            this.groupBoxTS40.TabIndex = 141;
            this.groupBoxTS40.TabStop = false;
            this.groupBoxTS40.Text = "hamqsl.com";
            // 
            this.btnWebImage_hamqsl_donate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnWebImage_hamqsl_donate.Image = null;
            this.btnWebImage_hamqsl_donate.Location = new System.Drawing.Point(208, 12);
            this.btnWebImage_hamqsl_donate.Name = "btnWebImage_hamqsl_donate";

            this.btnWebImage_hamqsl_donate.Size = new System.Drawing.Size(75, 24);
            this.btnWebImage_hamqsl_donate.TabIndex = 1;
            this.btnWebImage_hamqsl_donate.Text = "Donate";
            this.btnWebImage_hamqsl_donate.UseVisualStyleBackColor = false;
            this.btnWebImage_hamqsl_donate.Click += new System.EventHandler(this.btnWebImage_hamqsl_donate_Click);
            // 
            this.comboWebImage_HamQsl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWebImage_HamQsl.FormattingEnabled = true;
            this.comboWebImage_HamQsl.Location = new System.Drawing.Point(12, 15);
            this.comboWebImage_HamQsl.Name = "comboWebImage_HamQsl";
            this.comboWebImage_HamQsl.Size = new System.Drawing.Size(190, 21);
            this.comboWebImage_HamQsl.TabIndex = 0;
            this.comboWebImage_HamQsl.SelectedIndexChanged += new System.EventHandler(this.comboWebImage_HamQsl_SelectedIndexChanged);
            // 
            this.labelTS236.AutoSize = true;
            this.labelTS236.Image = null;
            this.labelTS236.Location = new System.Drawing.Point(18, 87);
            this.labelTS236.Name = "labelTS236";
            this.labelTS236.Size = new System.Drawing.Size(23, 13);
            this.labelTS236.TabIndex = 140;
            this.labelTS236.Text = "Url:";
            // 
            this.txtWebImage_url.Location = new System.Drawing.Point(47, 84);
            this.txtWebImage_url.Name = "txtWebImage_url";
            this.txtWebImage_url.Size = new System.Drawing.Size(267, 20);
            this.txtWebImage_url.TabIndex = 139;
            this.txtWebImage_url.TextChanged += new System.EventHandler(this.txtWebImage_url_TextChanged);
            // 
            this.nudWebImage_update_interval.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudWebImage_update_interval.Location = new System.Drawing.Point(95, 54);
            this.nudWebImage_update_interval.Maximum = new decimal(new int[] {
            7200,
            0,
            0,
            0});
            this.nudWebImage_update_interval.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudWebImage_update_interval.Name = "nudWebImage_update_interval";
            this.nudWebImage_update_interval.Size = new System.Drawing.Size(56, 20);
            this.nudWebImage_update_interval.TabIndex = 134;
            this.nudWebImage_update_interval.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudWebImage_update_interval, "Frequency to grab the web image");
            this.nudWebImage_update_interval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudWebImage_update_interval.ValueChanged += new System.EventHandler(this.nudWebImage_update_interval_ValueChanged);
            // 
            this.labelTS232.Image = null;
            this.labelTS232.Location = new System.Drawing.Point(18, 54);
            this.labelTS232.Name = "labelTS232";
            this.labelTS232.Size = new System.Drawing.Size(71, 16);
            this.labelTS232.TabIndex = 133;
            this.labelTS232.Text = "Update (s):";
            this.labelTS232.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.nudWebImage_width_scale.DecimalPlaces = 3;
            this.nudWebImage_width_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudWebImage_width_scale.Location = new System.Drawing.Point(95, 28);
            this.nudWebImage_width_scale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudWebImage_width_scale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudWebImage_width_scale.Name = "nudWebImage_width_scale";
            this.nudWebImage_width_scale.Size = new System.Drawing.Size(56, 20);
            this.nudWebImage_width_scale.TabIndex = 132;
            this.nudWebImage_width_scale.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudWebImage_width_scale, "Width scale. 1.0 will fill the container width");
            this.nudWebImage_width_scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudWebImage_width_scale.ValueChanged += new System.EventHandler(this.nudWebImage_width_scale_ValueChanged);
            // 
            this.labelTS235.AutoSize = true;
            this.labelTS235.Image = null;
            this.labelTS235.Location = new System.Drawing.Point(21, 30);
            this.labelTS235.Name = "labelTS235";
            this.labelTS235.Size = new System.Drawing.Size(68, 13);
            this.labelTS235.TabIndex = 131;
            this.labelTS235.Text = "Width Scale:";
            // 
            this.chkWebImage_fade_tx.AutoSize = true;
            this.chkWebImage_fade_tx.Image = null;
            this.chkWebImage_fade_tx.Location = new System.Drawing.Point(221, 40);
            this.chkWebImage_fade_tx.Name = "chkWebImage_fade_tx";
            this.chkWebImage_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.chkWebImage_fade_tx.TabIndex = 3;
            this.chkWebImage_fade_tx.Text = "Fade on TX";
            this.chkWebImage_fade_tx.UseVisualStyleBackColor = true;
            this.chkWebImage_fade_tx.CheckedChanged += new System.EventHandler(this.chkWebImage_fade_tx_CheckedChanged);
            // 
            this.chkWebImage_fade_rx.AutoSize = true;
            this.chkWebImage_fade_rx.Image = null;
            this.chkWebImage_fade_rx.Location = new System.Drawing.Point(221, 17);
            this.chkWebImage_fade_rx.Name = "chkWebImage_fade_rx";
            this.chkWebImage_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.chkWebImage_fade_rx.TabIndex = 2;
            this.chkWebImage_fade_rx.Text = "Fade on RX";
            this.chkWebImage_fade_rx.UseVisualStyleBackColor = true;
            this.chkWebImage_fade_rx.CheckedChanged += new System.EventHandler(this.chkWebImage_fade_rx_CheckedChanged);
            // 
            this.grpBandButtons.Controls.Add(this.pnlButtonBox_antenna_toggles);
            this.grpBandButtons.Controls.Add(this.nudButtonBox_font_y_shift);
            this.grpBandButtons.Controls.Add(this.labelTS250);
            this.grpBandButtons.Controls.Add(this.nudButtonBox_font_x_shift);
            this.grpBandButtons.Controls.Add(this.labelTS247);
            this.grpBandButtons.Controls.Add(this.nudButtonBox_font_scale);
            this.grpBandButtons.Controls.Add(this.labelTS248);
            this.grpBandButtons.Controls.Add(this.lblBandButtons_indicator_style);
            this.grpBandButtons.Controls.Add(this.nudBandButtons_indicator_style);
            this.grpBandButtons.Controls.Add(this.chkBandButtons_band_inactive_use);
            this.grpBandButtons.Controls.Add(this.labelTS246);
            this.grpBandButtons.Controls.Add(this.clrbtnBandButtons_hover);
            this.grpBandButtons.Controls.Add(this.labelTS245);
            this.grpBandButtons.Controls.Add(this.clrbtnBandButtons_fill);
            this.grpBandButtons.Controls.Add(this.labelTS244);
            this.grpBandButtons.Controls.Add(this.clrbtnBandButtons_border);
            this.grpBandButtons.Controls.Add(this.labelTS243);
            this.grpBandButtons.Controls.Add(this.clrbtnBandButtons_indicator_off);
            this.grpBandButtons.Controls.Add(this.labelTS242);
            this.grpBandButtons.Controls.Add(this.nudBandButtons_indicator_border);
            this.grpBandButtons.Controls.Add(this.lblBandButtons_indicator_border);
            this.grpBandButtons.Controls.Add(this.nudBandButtons_height_ratio);
            this.grpBandButtons.Controls.Add(this.labelTS241);
            this.grpBandButtons.Controls.Add(this.nudBandButtons_radius);
            this.grpBandButtons.Controls.Add(this.labelTS240);
            this.grpBandButtons.Controls.Add(this.nudBandButtons_margin);
            this.grpBandButtons.Controls.Add(this.labelTS239);
            this.grpBandButtons.Controls.Add(this.nudBandButtons_border);
            this.grpBandButtons.Controls.Add(this.labelTS238);
            this.grpBandButtons.Controls.Add(this.btnBandButtons_font);
            this.grpBandButtons.Controls.Add(this.chkBandButtons_use_indicator);
            this.grpBandButtons.Controls.Add(this.nudBandButtons_columns);
            this.grpBandButtons.Controls.Add(this.labelTS249);
            this.grpBandButtons.Controls.Add(this.clrbtnBandButtons_indicator_on);
            this.grpBandButtons.Controls.Add(this.chkBandButtons_fade_tx);
            this.grpBandButtons.Controls.Add(this.chkBandButtons_fade_rx);
            this.grpBandButtons.Location = new System.Drawing.Point(12, 23);
            this.grpBandButtons.Name = "grpBandButtons";
            this.grpBandButtons.Size = new System.Drawing.Size(323, 376);
            this.grpBandButtons.TabIndex = 109;
            this.grpBandButtons.TabStop = false;
            this.grpBandButtons.Text = "Button Box";
            this.grpBandButtons.Visible = false;
            // 
            this.pnlButtonBox_antenna_toggles.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlButtonBox_antenna_toggles.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_rxtxant);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_xvtr);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_ext1);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_byp);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_tx3);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_tx2);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_tx1);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_rx3);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_rx2);
            this.pnlButtonBox_antenna_toggles.Controls.Add(this.chkButtonBox_antenna_rx1);
            this.pnlButtonBox_antenna_toggles.Location = new System.Drawing.Point(166, 194);
            this.pnlButtonBox_antenna_toggles.Name = "pnlButtonBox_antenna_toggles";
            this.pnlButtonBox_antenna_toggles.Size = new System.Drawing.Size(157, 182);
            this.pnlButtonBox_antenna_toggles.TabIndex = 110;
            // 
            this.chkButtonBox_antenna_rxtxant.AutoSize = true;
            this.chkButtonBox_antenna_rxtxant.Image = null;
            this.chkButtonBox_antenna_rxtxant.Location = new System.Drawing.Point(66, 92);
            this.chkButtonBox_antenna_rxtxant.Name = "chkButtonBox_antenna_rxtxant";
            this.chkButtonBox_antenna_rxtxant.Size = new System.Drawing.Size(75, 17);
            this.chkButtonBox_antenna_rxtxant.TabIndex = 9;
            this.chkButtonBox_antenna_rxtxant.Text = "Rx/Tx Ant";
            this.chkButtonBox_antenna_rxtxant.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_rxtxant.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rxtxant_CheckedChanged);
            // 
            this.chkButtonBox_antenna_xvtr.AutoSize = true;
            this.chkButtonBox_antenna_xvtr.Image = null;
            this.chkButtonBox_antenna_xvtr.Location = new System.Drawing.Point(68, 53);
            this.chkButtonBox_antenna_xvtr.Name = "chkButtonBox_antenna_xvtr";
            this.chkButtonBox_antenna_xvtr.Size = new System.Drawing.Size(45, 17);
            this.chkButtonBox_antenna_xvtr.TabIndex = 8;
            this.chkButtonBox_antenna_xvtr.Text = "Xvtr";
            this.chkButtonBox_antenna_xvtr.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_xvtr.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_xvtr_CheckedChanged);
            // 
            this.chkButtonBox_antenna_ext1.AutoSize = true;
            this.chkButtonBox_antenna_ext1.Image = null;
            this.chkButtonBox_antenna_ext1.Location = new System.Drawing.Point(68, 32);
            this.chkButtonBox_antenna_ext1.Name = "chkButtonBox_antenna_ext1";
            this.chkButtonBox_antenna_ext1.Size = new System.Drawing.Size(50, 17);
            this.chkButtonBox_antenna_ext1.TabIndex = 7;
            this.chkButtonBox_antenna_ext1.Text = "Ext 1";
            this.chkButtonBox_antenna_ext1.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_ext1.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_ext1_CheckedChanged);
            // 
            this.chkButtonBox_antenna_byp.AutoSize = true;
            this.chkButtonBox_antenna_byp.Image = null;
            this.chkButtonBox_antenna_byp.Location = new System.Drawing.Point(68, 11);
            this.chkButtonBox_antenna_byp.Name = "chkButtonBox_antenna_byp";
            this.chkButtonBox_antenna_byp.Size = new System.Drawing.Size(44, 17);
            this.chkButtonBox_antenna_byp.TabIndex = 6;
            this.chkButtonBox_antenna_byp.Text = "Byp";
            this.chkButtonBox_antenna_byp.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_byp.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_byp_CheckedChanged);
            // 
            this.chkButtonBox_antenna_tx3.AutoSize = true;
            this.chkButtonBox_antenna_tx3.Image = null;
            this.chkButtonBox_antenna_tx3.Location = new System.Drawing.Point(14, 116);
            this.chkButtonBox_antenna_tx3.Name = "chkButtonBox_antenna_tx3";
            this.chkButtonBox_antenna_tx3.Size = new System.Drawing.Size(47, 17);
            this.chkButtonBox_antenna_tx3.TabIndex = 5;
            this.chkButtonBox_antenna_tx3.Text = "Tx 3";
            this.chkButtonBox_antenna_tx3.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_tx3.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_tx3_CheckedChanged);
            // 
            this.chkButtonBox_antenna_tx2.AutoSize = true;
            this.chkButtonBox_antenna_tx2.Image = null;
            this.chkButtonBox_antenna_tx2.Location = new System.Drawing.Point(14, 95);
            this.chkButtonBox_antenna_tx2.Name = "chkButtonBox_antenna_tx2";
            this.chkButtonBox_antenna_tx2.Size = new System.Drawing.Size(47, 17);
            this.chkButtonBox_antenna_tx2.TabIndex = 4;
            this.chkButtonBox_antenna_tx2.Text = "Tx 2";
            this.chkButtonBox_antenna_tx2.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_tx2.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_tx2_CheckedChanged);
            // 
            this.chkButtonBox_antenna_tx1.AutoSize = true;
            this.chkButtonBox_antenna_tx1.Image = null;
            this.chkButtonBox_antenna_tx1.Location = new System.Drawing.Point(14, 74);
            this.chkButtonBox_antenna_tx1.Name = "chkButtonBox_antenna_tx1";
            this.chkButtonBox_antenna_tx1.Size = new System.Drawing.Size(47, 17);
            this.chkButtonBox_antenna_tx1.TabIndex = 3;
            this.chkButtonBox_antenna_tx1.Text = "Tx 1";
            this.chkButtonBox_antenna_tx1.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_tx1.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_tx1_CheckedChanged);
            // 
            this.chkButtonBox_antenna_rx3.AutoSize = true;
            this.chkButtonBox_antenna_rx3.Image = null;
            this.chkButtonBox_antenna_rx3.Location = new System.Drawing.Point(14, 53);
            this.chkButtonBox_antenna_rx3.Name = "chkButtonBox_antenna_rx3";
            this.chkButtonBox_antenna_rx3.Size = new System.Drawing.Size(48, 17);
            this.chkButtonBox_antenna_rx3.TabIndex = 2;
            this.chkButtonBox_antenna_rx3.Text = "Rx 3";
            this.chkButtonBox_antenna_rx3.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_rx3.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rx3_CheckedChanged);
            // 
            this.chkButtonBox_antenna_rx2.AutoSize = true;
            this.chkButtonBox_antenna_rx2.Image = null;
            this.chkButtonBox_antenna_rx2.Location = new System.Drawing.Point(14, 32);
            this.chkButtonBox_antenna_rx2.Name = "chkButtonBox_antenna_rx2";
            this.chkButtonBox_antenna_rx2.Size = new System.Drawing.Size(48, 17);
            this.chkButtonBox_antenna_rx2.TabIndex = 1;
            this.chkButtonBox_antenna_rx2.Text = "Rx 2";
            this.chkButtonBox_antenna_rx2.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_rx2.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rx2_CheckedChanged);
            // 
            this.chkButtonBox_antenna_rx1.AutoSize = true;
            this.chkButtonBox_antenna_rx1.Image = null;
            this.chkButtonBox_antenna_rx1.Location = new System.Drawing.Point(14, 11);
            this.chkButtonBox_antenna_rx1.Name = "chkButtonBox_antenna_rx1";
            this.chkButtonBox_antenna_rx1.Size = new System.Drawing.Size(48, 17);
            this.chkButtonBox_antenna_rx1.TabIndex = 0;
            this.chkButtonBox_antenna_rx1.Text = "Rx 1";
            this.chkButtonBox_antenna_rx1.UseVisualStyleBackColor = true;
            this.chkButtonBox_antenna_rx1.CheckedChanged += new System.EventHandler(this.chkButtonBox_antenna_rx1_CheckedChanged);
            // 
            this.nudButtonBox_font_y_shift.DecimalPlaces = 2;
            this.nudButtonBox_font_y_shift.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudButtonBox_font_y_shift.Location = new System.Drawing.Point(241, 142);
            this.nudButtonBox_font_y_shift.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.nudButtonBox_font_y_shift.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            -2147352576});
            this.nudButtonBox_font_y_shift.Name = "nudButtonBox_font_y_shift";
            this.nudButtonBox_font_y_shift.Size = new System.Drawing.Size(56, 20);
            this.nudButtonBox_font_y_shift.TabIndex = 165;
            this.nudButtonBox_font_y_shift.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudButtonBox_font_y_shift, "Ratio of height to width");
            this.nudButtonBox_font_y_shift.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudButtonBox_font_y_shift.ValueChanged += new System.EventHandler(this.nudButtonBox_font_y_shift_ValueChanged);
            // 
            this.labelTS250.AutoSize = true;
            this.labelTS250.Image = null;
            this.labelTS250.Location = new System.Drawing.Point(194, 118);
            this.labelTS250.Name = "labelTS250";
            this.labelTS250.Size = new System.Drawing.Size(41, 13);
            this.labelTS250.TabIndex = 164;
            this.labelTS250.Text = "Shift X:";
            this.labelTS250.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudButtonBox_font_x_shift.DecimalPlaces = 2;
            this.nudButtonBox_font_x_shift.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudButtonBox_font_x_shift.Location = new System.Drawing.Point(241, 116);
            this.nudButtonBox_font_x_shift.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.nudButtonBox_font_x_shift.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            -2147352576});
            this.nudButtonBox_font_x_shift.Name = "nudButtonBox_font_x_shift";
            this.nudButtonBox_font_x_shift.Size = new System.Drawing.Size(56, 20);
            this.nudButtonBox_font_x_shift.TabIndex = 163;
            this.nudButtonBox_font_x_shift.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudButtonBox_font_x_shift, "Font x shift");
            this.nudButtonBox_font_x_shift.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudButtonBox_font_x_shift.ValueChanged += new System.EventHandler(this.nudButtonBox_font_x_shift_ValueChanged);
            // 
            this.labelTS247.AutoSize = true;
            this.labelTS247.Image = null;
            this.labelTS247.Location = new System.Drawing.Point(194, 147);
            this.labelTS247.Name = "labelTS247";
            this.labelTS247.Size = new System.Drawing.Size(41, 13);
            this.labelTS247.TabIndex = 162;
            this.labelTS247.Text = "Shift Y:";
            this.labelTS247.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudButtonBox_font_scale.DecimalPlaces = 2;
            this.nudButtonBox_font_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudButtonBox_font_scale.Location = new System.Drawing.Point(241, 90);
            this.nudButtonBox_font_scale.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudButtonBox_font_scale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudButtonBox_font_scale.Name = "nudButtonBox_font_scale";
            this.nudButtonBox_font_scale.Size = new System.Drawing.Size(56, 20);
            this.nudButtonBox_font_scale.TabIndex = 161;
            this.nudButtonBox_font_scale.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudButtonBox_font_scale, "Font scale adjustment");
            this.nudButtonBox_font_scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudButtonBox_font_scale.ValueChanged += new System.EventHandler(this.nudButtonBox_font_scale_ValueChanged);
            // 
            this.labelTS248.AutoSize = true;
            this.labelTS248.Image = null;
            this.labelTS248.Location = new System.Drawing.Point(194, 92);
            this.labelTS248.Name = "labelTS248";
            this.labelTS248.Size = new System.Drawing.Size(37, 13);
            this.labelTS248.TabIndex = 160;
            this.labelTS248.Text = "Scale:";
            this.labelTS248.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.lblBandButtons_indicator_style.AutoSize = true;
            this.lblBandButtons_indicator_style.Image = null;
            this.lblBandButtons_indicator_style.Location = new System.Drawing.Point(163, 174);
            this.lblBandButtons_indicator_style.Name = "lblBandButtons_indicator_style";
            this.lblBandButtons_indicator_style.Size = new System.Drawing.Size(33, 13);
            this.lblBandButtons_indicator_style.TabIndex = 159;
            this.lblBandButtons_indicator_style.Text = "Style:";
            this.lblBandButtons_indicator_style.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudBandButtons_indicator_style.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_indicator_style.Location = new System.Drawing.Point(202, 172);
            this.nudBandButtons_indicator_style.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudBandButtons_indicator_style.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudBandButtons_indicator_style.Name = "nudBandButtons_indicator_style";
            this.nudBandButtons_indicator_style.Size = new System.Drawing.Size(56, 20);
            this.nudBandButtons_indicator_style.TabIndex = 158;
            this.nudBandButtons_indicator_style.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudBandButtons_indicator_style, "Border size of indicator ring");
            this.nudBandButtons_indicator_style.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_indicator_style.ValueChanged += new System.EventHandler(this.nudBandButtons_indicator_style_ValueChanged);
            // 
            this.chkBandButtons_band_inactive_use.AutoSize = true;
            this.chkBandButtons_band_inactive_use.Image = null;
            this.chkBandButtons_band_inactive_use.Location = new System.Drawing.Point(120, 253);
            this.chkBandButtons_band_inactive_use.Name = "chkBandButtons_band_inactive_use";
            this.chkBandButtons_band_inactive_use.Size = new System.Drawing.Size(45, 17);
            this.chkBandButtons_band_inactive_use.TabIndex = 157;
            this.chkBandButtons_band_inactive_use.Text = "Use";
            this.chkBandButtons_band_inactive_use.UseVisualStyleBackColor = true;
            this.chkBandButtons_band_inactive_use.CheckedChanged += new System.EventHandler(this.chkBandButtons_band_inactive_use_CheckedChanged);
            // 
            this.labelTS246.AutoSize = true;
            this.labelTS246.Image = null;
            this.labelTS246.Location = new System.Drawing.Point(29, 341);
            this.labelTS246.Name = "labelTS246";
            this.labelTS246.Size = new System.Drawing.Size(39, 13);
            this.labelTS246.TabIndex = 156;
            this.labelTS246.Text = "Hover:";
            this.labelTS246.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.clrbtnBandButtons_hover.Automatic = "Automatic";
            this.clrbtnBandButtons_hover.Color = System.Drawing.Color.LightGray;
            this.clrbtnBandButtons_hover.ForeColor = System.Drawing.Color.Black;
            this.clrbtnBandButtons_hover.Image = null;
            this.clrbtnBandButtons_hover.Location = new System.Drawing.Point(74, 336);
            this.clrbtnBandButtons_hover.MoreColors = "More Colors...";
            this.clrbtnBandButtons_hover.Name = "clrbtnBandButtons_hover";

            this.clrbtnBandButtons_hover.Size = new System.Drawing.Size(40, 23);
            this.clrbtnBandButtons_hover.TabIndex = 155;
            this.toolTip1.SetToolTip(this.clrbtnBandButtons_hover, "Active colour");
            this.clrbtnBandButtons_hover.Changed += new System.EventHandler(this.clrbtnBandButtons_hover_Changed);
            // 
            this.labelTS245.AutoSize = true;
            this.labelTS245.Image = null;
            this.labelTS245.Location = new System.Drawing.Point(46, 312);
            this.labelTS245.Name = "labelTS245";
            this.labelTS245.Size = new System.Drawing.Size(22, 13);
            this.labelTS245.TabIndex = 154;
            this.labelTS245.Text = "Fill:";
            this.labelTS245.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.clrbtnBandButtons_fill.Automatic = "Automatic";
            this.clrbtnBandButtons_fill.Color = System.Drawing.Color.Black;
            this.clrbtnBandButtons_fill.ForeColor = System.Drawing.Color.Black;
            this.clrbtnBandButtons_fill.Image = null;
            this.clrbtnBandButtons_fill.Location = new System.Drawing.Point(74, 307);
            this.clrbtnBandButtons_fill.MoreColors = "More Colors...";
            this.clrbtnBandButtons_fill.Name = "clrbtnBandButtons_fill";

            this.clrbtnBandButtons_fill.Size = new System.Drawing.Size(40, 23);
            this.clrbtnBandButtons_fill.TabIndex = 153;
            this.toolTip1.SetToolTip(this.clrbtnBandButtons_fill, "Active colour");
            this.clrbtnBandButtons_fill.Changed += new System.EventHandler(this.clrbtnBandButtons_fill_Changed);
            // 
            this.labelTS244.AutoSize = true;
            this.labelTS244.Image = null;
            this.labelTS244.Location = new System.Drawing.Point(27, 283);
            this.labelTS244.Name = "labelTS244";
            this.labelTS244.Size = new System.Drawing.Size(41, 13);
            this.labelTS244.TabIndex = 152;
            this.labelTS244.Text = "Border:";
            this.labelTS244.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.clrbtnBandButtons_border.Automatic = "Automatic";
            this.clrbtnBandButtons_border.Color = System.Drawing.Color.White;
            this.clrbtnBandButtons_border.ForeColor = System.Drawing.Color.Black;
            this.clrbtnBandButtons_border.Image = null;
            this.clrbtnBandButtons_border.Location = new System.Drawing.Point(74, 278);
            this.clrbtnBandButtons_border.MoreColors = "More Colors...";
            this.clrbtnBandButtons_border.Name = "clrbtnBandButtons_border";

            this.clrbtnBandButtons_border.Size = new System.Drawing.Size(40, 23);
            this.clrbtnBandButtons_border.TabIndex = 151;
            this.toolTip1.SetToolTip(this.clrbtnBandButtons_border, "Active colour");
            this.clrbtnBandButtons_border.Changed += new System.EventHandler(this.clrbtnBandButtons_border_Changed);
            // 
            this.labelTS243.AutoSize = true;
            this.labelTS243.Image = null;
            this.labelTS243.Location = new System.Drawing.Point(20, 254);
            this.labelTS243.Name = "labelTS243";
            this.labelTS243.Size = new System.Drawing.Size(48, 13);
            this.labelTS243.TabIndex = 150;
            this.labelTS243.Text = "Inactive:";
            this.labelTS243.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.clrbtnBandButtons_indicator_off.Automatic = "Automatic";
            this.clrbtnBandButtons_indicator_off.Color = System.Drawing.Color.LightGray;
            this.clrbtnBandButtons_indicator_off.ForeColor = System.Drawing.Color.Black;
            this.clrbtnBandButtons_indicator_off.Image = null;
            this.clrbtnBandButtons_indicator_off.Location = new System.Drawing.Point(74, 249);
            this.clrbtnBandButtons_indicator_off.MoreColors = "More Colors...";
            this.clrbtnBandButtons_indicator_off.Name = "clrbtnBandButtons_indicator_off";

            this.clrbtnBandButtons_indicator_off.Size = new System.Drawing.Size(40, 23);
            this.clrbtnBandButtons_indicator_off.TabIndex = 149;
            this.toolTip1.SetToolTip(this.clrbtnBandButtons_indicator_off, "Inactive colour");
            this.clrbtnBandButtons_indicator_off.Changed += new System.EventHandler(this.clrbtnBandButtons_indicator_off_Changed);
            // 
            this.labelTS242.AutoSize = true;
            this.labelTS242.Image = null;
            this.labelTS242.Location = new System.Drawing.Point(28, 225);
            this.labelTS242.Name = "labelTS242";
            this.labelTS242.Size = new System.Drawing.Size(40, 13);
            this.labelTS242.TabIndex = 148;
            this.labelTS242.Text = "Active:";
            this.labelTS242.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudBandButtons_indicator_border.DecimalPlaces = 2;
            this.nudBandButtons_indicator_border.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudBandButtons_indicator_border.Location = new System.Drawing.Point(91, 172);
            this.nudBandButtons_indicator_border.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_indicator_border.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudBandButtons_indicator_border.Name = "nudBandButtons_indicator_border";
            this.nudBandButtons_indicator_border.Size = new System.Drawing.Size(56, 20);
            this.nudBandButtons_indicator_border.TabIndex = 147;
            this.nudBandButtons_indicator_border.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudBandButtons_indicator_border, "Border size of indicator ring");
            this.nudBandButtons_indicator_border.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_indicator_border.ValueChanged += new System.EventHandler(this.nudBandButtons_indicator_border_ValueChanged);
            // 
            this.lblBandButtons_indicator_border.AutoSize = true;
            this.lblBandButtons_indicator_border.Image = null;
            this.lblBandButtons_indicator_border.Location = new System.Drawing.Point(44, 174);
            this.lblBandButtons_indicator_border.Name = "lblBandButtons_indicator_border";
            this.lblBandButtons_indicator_border.Size = new System.Drawing.Size(41, 13);
            this.lblBandButtons_indicator_border.TabIndex = 146;
            this.lblBandButtons_indicator_border.Text = "Border:";
            this.lblBandButtons_indicator_border.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudBandButtons_height_ratio.DecimalPlaces = 2;
            this.nudBandButtons_height_ratio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudBandButtons_height_ratio.Location = new System.Drawing.Point(78, 123);
            this.nudBandButtons_height_ratio.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudBandButtons_height_ratio.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudBandButtons_height_ratio.Name = "nudBandButtons_height_ratio";
            this.nudBandButtons_height_ratio.Size = new System.Drawing.Size(56, 20);
            this.nudBandButtons_height_ratio.TabIndex = 145;
            this.nudBandButtons_height_ratio.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudBandButtons_height_ratio, "Ratio of height to width");
            this.nudBandButtons_height_ratio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_height_ratio.ValueChanged += new System.EventHandler(this.nudBandButtons_height_ratio_ValueChanged);
            // 
            this.labelTS241.AutoSize = true;
            this.labelTS241.Image = null;
            this.labelTS241.Location = new System.Drawing.Point(3, 125);
            this.labelTS241.Name = "labelTS241";
            this.labelTS241.Size = new System.Drawing.Size(69, 13);
            this.labelTS241.TabIndex = 144;
            this.labelTS241.Text = "Height Ratio:";
            this.labelTS241.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudBandButtons_radius.DecimalPlaces = 2;
            this.nudBandButtons_radius.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudBandButtons_radius.Location = new System.Drawing.Point(78, 97);
            this.nudBandButtons_radius.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudBandButtons_radius.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudBandButtons_radius.Name = "nudBandButtons_radius";
            this.nudBandButtons_radius.Size = new System.Drawing.Size(56, 20);
            this.nudBandButtons_radius.TabIndex = 143;
            this.nudBandButtons_radius.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudBandButtons_radius, "Radius corner size");
            this.nudBandButtons_radius.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_radius.ValueChanged += new System.EventHandler(this.nudBandButtons_radius_ValueChanged);
            // 
            this.labelTS240.AutoSize = true;
            this.labelTS240.Image = null;
            this.labelTS240.Location = new System.Drawing.Point(31, 99);
            this.labelTS240.Name = "labelTS240";
            this.labelTS240.Size = new System.Drawing.Size(43, 13);
            this.labelTS240.TabIndex = 142;
            this.labelTS240.Text = "Radius:";
            this.labelTS240.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudBandButtons_margin.DecimalPlaces = 2;
            this.nudBandButtons_margin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudBandButtons_margin.Location = new System.Drawing.Point(78, 71);
            this.nudBandButtons_margin.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_margin.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudBandButtons_margin.Name = "nudBandButtons_margin";
            this.nudBandButtons_margin.Size = new System.Drawing.Size(56, 20);
            this.nudBandButtons_margin.TabIndex = 141;
            this.nudBandButtons_margin.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudBandButtons_margin, "Margin size");
            this.nudBandButtons_margin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_margin.ValueChanged += new System.EventHandler(this.nudBandButtons_margin_ValueChanged);
            // 
            this.labelTS239.AutoSize = true;
            this.labelTS239.Image = null;
            this.labelTS239.Location = new System.Drawing.Point(31, 73);
            this.labelTS239.Name = "labelTS239";
            this.labelTS239.Size = new System.Drawing.Size(42, 13);
            this.labelTS239.TabIndex = 140;
            this.labelTS239.Text = "Margin:";
            this.labelTS239.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.nudBandButtons_border.DecimalPlaces = 2;
            this.nudBandButtons_border.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudBandButtons_border.Location = new System.Drawing.Point(78, 45);
            this.nudBandButtons_border.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_border.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudBandButtons_border.Name = "nudBandButtons_border";
            this.nudBandButtons_border.Size = new System.Drawing.Size(56, 20);
            this.nudBandButtons_border.TabIndex = 139;
            this.nudBandButtons_border.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudBandButtons_border, "Border size");
            this.nudBandButtons_border.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_border.ValueChanged += new System.EventHandler(this.nudBandButtons_border_ValueChanged);
            // 
            this.labelTS238.AutoSize = true;
            this.labelTS238.Image = null;
            this.labelTS238.Location = new System.Drawing.Point(31, 47);
            this.labelTS238.Name = "labelTS238";
            this.labelTS238.Size = new System.Drawing.Size(41, 13);
            this.labelTS238.TabIndex = 138;
            this.labelTS238.Text = "Border:";
            this.labelTS238.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.btnBandButtons_font.Image = null;
            this.btnBandButtons_font.Location = new System.Drawing.Point(241, 61);
            this.btnBandButtons_font.Name = "btnBandButtons_font";

            this.btnBandButtons_font.Size = new System.Drawing.Size(56, 23);
            this.btnBandButtons_font.TabIndex = 137;
            this.btnBandButtons_font.Text = "Font";
            this.btnBandButtons_font.UseVisualStyleBackColor = true;
            this.btnBandButtons_font.Click += new System.EventHandler(this.btnBandButtons_font_Click);
            // 
            this.chkBandButtons_use_indicator.AutoSize = true;
            this.chkBandButtons_use_indicator.Image = null;
            this.chkBandButtons_use_indicator.Location = new System.Drawing.Point(25, 152);
            this.chkBandButtons_use_indicator.Name = "chkBandButtons_use_indicator";
            this.chkBandButtons_use_indicator.Size = new System.Drawing.Size(89, 17);
            this.chkBandButtons_use_indicator.TabIndex = 135;
            this.chkBandButtons_use_indicator.Text = "Use Indicator";
            this.toolTip1.SetToolTip(this.chkBandButtons_use_indicator, "Use a ring indicator to show active");
            this.chkBandButtons_use_indicator.UseVisualStyleBackColor = true;
            this.chkBandButtons_use_indicator.CheckedChanged += new System.EventHandler(this.chkBandButtons_use_indicator_CheckedChanged);
            // 
            this.nudBandButtons_columns.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_columns.Location = new System.Drawing.Point(78, 19);
            this.nudBandButtons_columns.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudBandButtons_columns.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_columns.Name = "nudBandButtons_columns";
            this.nudBandButtons_columns.Size = new System.Drawing.Size(56, 20);
            this.nudBandButtons_columns.TabIndex = 132;
            this.nudBandButtons_columns.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudBandButtons_columns, "Number of button columns");
            this.nudBandButtons_columns.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBandButtons_columns.ValueChanged += new System.EventHandler(this.nudBandButtons_columns_ValueChanged);
            // 
            this.labelTS249.AutoSize = true;
            this.labelTS249.Image = null;
            this.labelTS249.Location = new System.Drawing.Point(22, 21);
            this.labelTS249.Name = "labelTS249";
            this.labelTS249.Size = new System.Drawing.Size(50, 13);
            this.labelTS249.TabIndex = 131;
            this.labelTS249.Text = "Columns:";
            this.labelTS249.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            this.clrbtnBandButtons_indicator_on.Automatic = "Automatic";
            this.clrbtnBandButtons_indicator_on.Color = System.Drawing.Color.CornflowerBlue;
            this.clrbtnBandButtons_indicator_on.ForeColor = System.Drawing.Color.Black;
            this.clrbtnBandButtons_indicator_on.Image = null;
            this.clrbtnBandButtons_indicator_on.Location = new System.Drawing.Point(74, 220);
            this.clrbtnBandButtons_indicator_on.MoreColors = "More Colors...";
            this.clrbtnBandButtons_indicator_on.Name = "clrbtnBandButtons_indicator_on";

            this.clrbtnBandButtons_indicator_on.Size = new System.Drawing.Size(40, 23);
            this.clrbtnBandButtons_indicator_on.TabIndex = 129;
            this.toolTip1.SetToolTip(this.clrbtnBandButtons_indicator_on, "Active colour");
            this.clrbtnBandButtons_indicator_on.Changed += new System.EventHandler(this.clrbtnBandButtons_indicator_on_Changed);
            // 
            this.chkBandButtons_fade_tx.AutoSize = true;
            this.chkBandButtons_fade_tx.Image = null;
            this.chkBandButtons_fade_tx.Location = new System.Drawing.Point(216, 40);
            this.chkBandButtons_fade_tx.Name = "chkBandButtons_fade_tx";
            this.chkBandButtons_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.chkBandButtons_fade_tx.TabIndex = 3;
            this.chkBandButtons_fade_tx.Text = "Fade on TX";
            this.chkBandButtons_fade_tx.UseVisualStyleBackColor = true;
            this.chkBandButtons_fade_tx.CheckedChanged += new System.EventHandler(this.chkBandButtons_fade_tx_CheckedChanged);
            // 
            this.chkBandButtons_fade_rx.AutoSize = true;
            this.chkBandButtons_fade_rx.Image = null;
            this.chkBandButtons_fade_rx.Location = new System.Drawing.Point(215, 17);
            this.chkBandButtons_fade_rx.Name = "chkBandButtons_fade_rx";
            this.chkBandButtons_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.chkBandButtons_fade_rx.TabIndex = 2;
            this.chkBandButtons_fade_rx.Text = "Fade on RX";
            this.chkBandButtons_fade_rx.UseVisualStyleBackColor = true;
            this.chkBandButtons_fade_rx.CheckedChanged += new System.EventHandler(this.chkBandButtons_fade_rx_CheckedChanged);
            // 
            this.grpHistoryItem.Controls.Add(this.labelTS281);
            this.grpHistoryItem.Controls.Add(this.labelTS280);
            this.grpHistoryItem.Controls.Add(this.clrbtnHistory_time);
            this.grpHistoryItem.Controls.Add(this.clrbtnHistory_lines);
            this.grpHistoryItem.Controls.Add(this.groupBoxTS46);
            this.grpHistoryItem.Controls.Add(this.groupBoxTS45);
            this.grpHistoryItem.Controls.Add(this.pnlVariableInUse_2_history);
            this.grpHistoryItem.Controls.Add(this.btnMMIO_variable_2_history);
            this.grpHistoryItem.Controls.Add(this.btnMMIO_variable_history);
            this.grpHistoryItem.Controls.Add(this.pnlVariableInUse_1_history);
            this.grpHistoryItem.Controls.Add(this.nudHistory_keep_for);
            this.grpHistoryItem.Controls.Add(this.labelTS259);
            this.grpHistoryItem.Controls.Add(this.nudHistory_update);
            this.grpHistoryItem.Controls.Add(this.labelTS252);
            this.grpHistoryItem.Controls.Add(this.nudHistory_vertical_ratio);
            this.grpHistoryItem.Controls.Add(this.labelTS253);
            this.grpHistoryItem.Controls.Add(this.labelTS258);
            this.grpHistoryItem.Controls.Add(this.clrbtnHistory_background);
            this.grpHistoryItem.Controls.Add(this.chkHistory_fade_tx);
            this.grpHistoryItem.Controls.Add(this.chkHistory_fade_rx);
            this.grpHistoryItem.Location = new System.Drawing.Point(12, 14);
            this.grpHistoryItem.Name = "grpHistoryItem";
            this.grpHistoryItem.Size = new System.Drawing.Size(323, 376);
            this.grpHistoryItem.TabIndex = 110;
            this.grpHistoryItem.TabStop = false;
            this.grpHistoryItem.Text = "History Graph";
            this.grpHistoryItem.Visible = false;
            // 
            this.labelTS281.Image = null;
            this.labelTS281.Location = new System.Drawing.Point(118, 113);
            this.labelTS281.Name = "labelTS281";
            this.labelTS281.Size = new System.Drawing.Size(46, 16);
            this.labelTS281.TabIndex = 148;
            this.labelTS281.Text = "Time:";
            this.labelTS281.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.labelTS280.Image = null;
            this.labelTS280.Location = new System.Drawing.Point(8, 113);
            this.labelTS280.Name = "labelTS280";
            this.labelTS280.Size = new System.Drawing.Size(58, 16);
            this.labelTS280.TabIndex = 147;
            this.labelTS280.Text = "Lines:";
            this.labelTS280.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.clrbtnHistory_time.Automatic = "Automatic";
            this.clrbtnHistory_time.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnHistory_time.ForeColor = System.Drawing.Color.Black;
            this.clrbtnHistory_time.Image = null;
            this.clrbtnHistory_time.Location = new System.Drawing.Point(170, 110);
            this.clrbtnHistory_time.MoreColors = "More Colors...";
            this.clrbtnHistory_time.Name = "clrbtnHistory_time";

            this.clrbtnHistory_time.Size = new System.Drawing.Size(40, 23);
            this.clrbtnHistory_time.TabIndex = 146;
            this.toolTip1.SetToolTip(this.clrbtnHistory_time, "Background colour");
            this.clrbtnHistory_time.Changed += new System.EventHandler(this.clrbtnHistory_time_Changed);
            // 
            this.clrbtnHistory_lines.Automatic = "Automatic";
            this.clrbtnHistory_lines.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnHistory_lines.ForeColor = System.Drawing.Color.Black;
            this.clrbtnHistory_lines.Image = null;
            this.clrbtnHistory_lines.Location = new System.Drawing.Point(72, 110);
            this.clrbtnHistory_lines.MoreColors = "More Colors...";
            this.clrbtnHistory_lines.Name = "clrbtnHistory_lines";

            this.clrbtnHistory_lines.Size = new System.Drawing.Size(40, 23);
            this.clrbtnHistory_lines.TabIndex = 145;
            this.toolTip1.SetToolTip(this.clrbtnHistory_lines, "Background colour");
            this.clrbtnHistory_lines.Changed += new System.EventHandler(this.clrbtnHistory_lines_Changed);
            // 
            this.groupBoxTS46.Controls.Add(this.clrbtnHistory_colour_1);
            this.groupBoxTS46.Controls.Add(this.btnHistory_copy_minmax_from_0);
            this.groupBoxTS46.Controls.Add(this.chkHistory_1_show_axis);
            this.groupBoxTS46.Controls.Add(this.nudHistory_axis1_max);
            this.groupBoxTS46.Controls.Add(this.labelTS263);
            this.groupBoxTS46.Controls.Add(this.nudHistory_axis1_min);
            this.groupBoxTS46.Controls.Add(this.labelTS264);
            this.groupBoxTS46.Controls.Add(this.chkHistory_auto_1_scale);
            this.groupBoxTS46.Controls.Add(this.comboHistory_reading_1);
            this.groupBoxTS46.Controls.Add(this.labelTS265);
            this.groupBoxTS46.Location = new System.Drawing.Point(14, 252);
            this.groupBoxTS46.Name = "groupBoxTS46";
            this.groupBoxTS46.Size = new System.Drawing.Size(297, 102);
            this.groupBoxTS46.TabIndex = 144;
            this.groupBoxTS46.TabStop = false;
            this.groupBoxTS46.Text = "Right Axis";
            // 
            this.clrbtnHistory_colour_1.Automatic = "Automatic";
            this.clrbtnHistory_colour_1.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnHistory_colour_1.ForeColor = System.Drawing.Color.Black;
            this.clrbtnHistory_colour_1.Image = null;
            this.clrbtnHistory_colour_1.Location = new System.Drawing.Point(215, 21);
            this.clrbtnHistory_colour_1.MoreColors = "More Colors...";
            this.clrbtnHistory_colour_1.Name = "clrbtnHistory_colour_1";

            this.clrbtnHistory_colour_1.Size = new System.Drawing.Size(40, 23);
            this.clrbtnHistory_colour_1.TabIndex = 147;
            this.toolTip1.SetToolTip(this.clrbtnHistory_colour_1, "Background colour");
            this.clrbtnHistory_colour_1.Changed += new System.EventHandler(this.clrbtnHistory_colour_1_Changed);
            // 
            this.btnHistory_copy_minmax_from_0.Image = null;
            this.btnHistory_copy_minmax_from_0.Location = new System.Drawing.Point(249, 72);
            this.btnHistory_copy_minmax_from_0.Name = "btnHistory_copy_minmax_from_0";

            this.btnHistory_copy_minmax_from_0.Size = new System.Drawing.Size(32, 24);
            this.btnHistory_copy_minmax_from_0.TabIndex = 146;
            this.btnHistory_copy_minmax_from_0.Text = "=";
            this.btnHistory_copy_minmax_from_0.UseVisualStyleBackColor = true;
            this.btnHistory_copy_minmax_from_0.Click += new System.EventHandler(this.btnHistory_copy_minmax_from_0_Click);
            // 
            this.chkHistory_1_show_axis.AutoSize = true;
            this.chkHistory_1_show_axis.Image = null;
            this.chkHistory_1_show_axis.Location = new System.Drawing.Point(62, 0);
            this.chkHistory_1_show_axis.Name = "chkHistory_1_show_axis";
            this.chkHistory_1_show_axis.Size = new System.Drawing.Size(15, 14);
            this.chkHistory_1_show_axis.TabIndex = 145;
            this.chkHistory_1_show_axis.UseVisualStyleBackColor = true;
            this.chkHistory_1_show_axis.CheckedChanged += new System.EventHandler(this.chkHistory_1_show_axis_CheckedChanged);
            // 
            this.nudHistory_axis1_max.DecimalPlaces = 1;
            this.nudHistory_axis1_max.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudHistory_axis1_max.Location = new System.Drawing.Point(187, 73);
            this.nudHistory_axis1_max.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudHistory_axis1_max.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            -2147483648});
            this.nudHistory_axis1_max.Name = "nudHistory_axis1_max";
            this.nudHistory_axis1_max.Size = new System.Drawing.Size(56, 20);
            this.nudHistory_axis1_max.TabIndex = 143;
            this.nudHistory_axis1_max.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudHistory_axis1_max, "Reading update and is related to screen update");
            this.nudHistory_axis1_max.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudHistory_axis1_max.ValueChanged += new System.EventHandler(this.nudHistory_axis1_max_ValueChanged);
            // 
            this.labelTS263.Image = null;
            this.labelTS263.Location = new System.Drawing.Point(138, 73);
            this.labelTS263.Name = "labelTS263";
            this.labelTS263.Size = new System.Drawing.Size(43, 16);
            this.labelTS263.TabIndex = 142;
            this.labelTS263.Text = "Max:";
            this.labelTS263.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.nudHistory_axis1_min.DecimalPlaces = 1;
            this.nudHistory_axis1_min.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudHistory_axis1_min.Location = new System.Drawing.Point(73, 73);
            this.nudHistory_axis1_min.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.nudHistory_axis1_min.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.nudHistory_axis1_min.Name = "nudHistory_axis1_min";
            this.nudHistory_axis1_min.Size = new System.Drawing.Size(56, 20);
            this.nudHistory_axis1_min.TabIndex = 141;
            this.nudHistory_axis1_min.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudHistory_axis1_min, "Reading update and is related to screen update");
            this.nudHistory_axis1_min.Value = new decimal(new int[] {
            150,
            0,
            0,
            -2147483648});
            this.nudHistory_axis1_min.ValueChanged += new System.EventHandler(this.nudHistory_axis1_min_ValueChanged);
            // 
            this.labelTS264.Image = null;
            this.labelTS264.Location = new System.Drawing.Point(24, 73);
            this.labelTS264.Name = "labelTS264";
            this.labelTS264.Size = new System.Drawing.Size(43, 16);
            this.labelTS264.TabIndex = 140;
            this.labelTS264.Text = "Min:";
            this.labelTS264.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.chkHistory_auto_1_scale.AutoSize = true;
            this.chkHistory_auto_1_scale.Image = null;
            this.chkHistory_auto_1_scale.Location = new System.Drawing.Point(73, 52);
            this.chkHistory_auto_1_scale.Name = "chkHistory_auto_1_scale";
            this.chkHistory_auto_1_scale.Size = new System.Drawing.Size(78, 17);
            this.chkHistory_auto_1_scale.TabIndex = 139;
            this.chkHistory_auto_1_scale.Text = "Auto Scale";
            this.chkHistory_auto_1_scale.UseVisualStyleBackColor = true;
            this.chkHistory_auto_1_scale.CheckedChanged += new System.EventHandler(this.chkHistory_auto_1_scale_CheckedChanged);
            // 
            this.comboHistory_reading_1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHistory_reading_1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboHistory_reading_1.FormattingEnabled = true;
            this.comboHistory_reading_1.Items.AddRange(new object[] {
            "HERMES",
            "ANAN-10",
            "ANAN-10E",
            "ANAN-100",
            "ANAN-100B",
            "ANAN-100D",
            "ANAN-200D",
            "ANAN-7000DLE",
            "ANAN-8000DLE",
            "ANAN-G2",
            "ANAN-G2-1K"});
            this.comboHistory_reading_1.Location = new System.Drawing.Point(73, 21);
            this.comboHistory_reading_1.Name = "comboHistory_reading_1";
            this.comboHistory_reading_1.Size = new System.Drawing.Size(136, 23);
            this.comboHistory_reading_1.TabIndex = 137;
            this.comboHistory_reading_1.SelectedIndexChanged += new System.EventHandler(this.comboHistory_reading_1_SelectedIndexChanged);
            // 
            this.labelTS265.Image = null;
            this.labelTS265.Location = new System.Drawing.Point(9, 23);
            this.labelTS265.Name = "labelTS265";
            this.labelTS265.Size = new System.Drawing.Size(58, 16);
            this.labelTS265.TabIndex = 138;
            this.labelTS265.Text = "Reading:";
            this.labelTS265.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.groupBoxTS45.Controls.Add(this.clrbtnHistory_colour_0);
            this.groupBoxTS45.Controls.Add(this.nudHistory_axis0_max);
            this.groupBoxTS45.Controls.Add(this.labelTS262);
            this.groupBoxTS45.Controls.Add(this.nudHistory_axis0_min);
            this.groupBoxTS45.Controls.Add(this.labelTS261);
            this.groupBoxTS45.Controls.Add(this.chkHistory_auto_0_scale);
            this.groupBoxTS45.Controls.Add(this.comboHistory_reading_0);
            this.groupBoxTS45.Controls.Add(this.labelTS260);
            this.groupBoxTS45.Location = new System.Drawing.Point(14, 143);
            this.groupBoxTS45.Name = "groupBoxTS45";
            this.groupBoxTS45.Size = new System.Drawing.Size(297, 102);
            this.groupBoxTS45.TabIndex = 143;
            this.groupBoxTS45.TabStop = false;
            this.groupBoxTS45.Text = "Left Axis";
            // 
            this.clrbtnHistory_colour_0.Automatic = "Automatic";
            this.clrbtnHistory_colour_0.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnHistory_colour_0.ForeColor = System.Drawing.Color.Black;
            this.clrbtnHistory_colour_0.Image = null;
            this.clrbtnHistory_colour_0.Location = new System.Drawing.Point(215, 21);
            this.clrbtnHistory_colour_0.MoreColors = "More Colors...";
            this.clrbtnHistory_colour_0.Name = "clrbtnHistory_colour_0";

            this.clrbtnHistory_colour_0.Size = new System.Drawing.Size(40, 23);
            this.clrbtnHistory_colour_0.TabIndex = 130;
            this.toolTip1.SetToolTip(this.clrbtnHistory_colour_0, "Background colour");
            this.clrbtnHistory_colour_0.Changed += new System.EventHandler(this.clrbtnHistory_colour_0_Changed);
            // 
            this.nudHistory_axis0_max.DecimalPlaces = 1;
            this.nudHistory_axis0_max.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudHistory_axis0_max.Location = new System.Drawing.Point(187, 73);
            this.nudHistory_axis0_max.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudHistory_axis0_max.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            -2147483648});
            this.nudHistory_axis0_max.Name = "nudHistory_axis0_max";
            this.nudHistory_axis0_max.Size = new System.Drawing.Size(56, 20);
            this.nudHistory_axis0_max.TabIndex = 143;
            this.nudHistory_axis0_max.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudHistory_axis0_max, "Reading update and is related to screen update");
            this.nudHistory_axis0_max.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudHistory_axis0_max.ValueChanged += new System.EventHandler(this.nudHistory_axis0_max_ValueChanged);
            // 
            this.labelTS262.Image = null;
            this.labelTS262.Location = new System.Drawing.Point(138, 73);
            this.labelTS262.Name = "labelTS262";
            this.labelTS262.Size = new System.Drawing.Size(43, 16);
            this.labelTS262.TabIndex = 142;
            this.labelTS262.Text = "Max:";
            this.labelTS262.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.nudHistory_axis0_min.DecimalPlaces = 1;
            this.nudHistory_axis0_min.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudHistory_axis0_min.Location = new System.Drawing.Point(73, 73);
            this.nudHistory_axis0_min.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.nudHistory_axis0_min.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.nudHistory_axis0_min.Name = "nudHistory_axis0_min";
            this.nudHistory_axis0_min.Size = new System.Drawing.Size(56, 20);
            this.nudHistory_axis0_min.TabIndex = 141;
            this.nudHistory_axis0_min.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudHistory_axis0_min, "Reading update and is related to screen update");
            this.nudHistory_axis0_min.Value = new decimal(new int[] {
            150,
            0,
            0,
            -2147483648});
            this.nudHistory_axis0_min.ValueChanged += new System.EventHandler(this.nudHistory_axis0_min_ValueChanged);
            // 
            this.labelTS261.Image = null;
            this.labelTS261.Location = new System.Drawing.Point(24, 73);
            this.labelTS261.Name = "labelTS261";
            this.labelTS261.Size = new System.Drawing.Size(43, 16);
            this.labelTS261.TabIndex = 140;
            this.labelTS261.Text = "Min:";
            this.labelTS261.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.chkHistory_auto_0_scale.AutoSize = true;
            this.chkHistory_auto_0_scale.Image = null;
            this.chkHistory_auto_0_scale.Location = new System.Drawing.Point(73, 52);
            this.chkHistory_auto_0_scale.Name = "chkHistory_auto_0_scale";
            this.chkHistory_auto_0_scale.Size = new System.Drawing.Size(78, 17);
            this.chkHistory_auto_0_scale.TabIndex = 139;
            this.chkHistory_auto_0_scale.Text = "Auto Scale";
            this.chkHistory_auto_0_scale.UseVisualStyleBackColor = true;
            this.chkHistory_auto_0_scale.CheckedChanged += new System.EventHandler(this.chkHistory_auto_0_scale_CheckedChanged);
            // 
            this.comboHistory_reading_0.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHistory_reading_0.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboHistory_reading_0.FormattingEnabled = true;
            this.comboHistory_reading_0.Items.AddRange(new object[] {
            "HERMES",
            "ANAN-10",
            "ANAN-10E",
            "ANAN-100",
            "ANAN-100B",
            "ANAN-100D",
            "ANAN-200D",
            "ANAN-7000DLE",
            "ANAN-8000DLE",
            "ANAN-G2",
            "ANAN-G2-1K"});
            this.comboHistory_reading_0.Location = new System.Drawing.Point(73, 21);
            this.comboHistory_reading_0.Name = "comboHistory_reading_0";
            this.comboHistory_reading_0.Size = new System.Drawing.Size(136, 23);
            this.comboHistory_reading_0.TabIndex = 137;
            this.comboHistory_reading_0.SelectedIndexChanged += new System.EventHandler(this.comboHistory_reading_0_SelectedIndexChanged);
            // 
            this.labelTS260.Image = null;
            this.labelTS260.Location = new System.Drawing.Point(9, 23);
            this.labelTS260.Name = "labelTS260";
            this.labelTS260.Size = new System.Drawing.Size(58, 16);
            this.labelTS260.TabIndex = 138;
            this.labelTS260.Text = "Reading:";
            this.labelTS260.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.pnlVariableInUse_2_history.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_2_history.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_2_history.BackColor = System.Drawing.Color.Lime;
            this.pnlVariableInUse_2_history.Location = new System.Drawing.Point(284, 79);
            this.pnlVariableInUse_2_history.Name = "pnlVariableInUse_2_history";
            this.pnlVariableInUse_2_history.Size = new System.Drawing.Size(28, 6);
            this.pnlVariableInUse_2_history.TabIndex = 142;
            // 
            this.btnMMIO_variable_2_history.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMMIO_variable_2_history.Image = null;
            this.btnMMIO_variable_2_history.Location = new System.Drawing.Point(284, 52);
            this.btnMMIO_variable_2_history.Name = "btnMMIO_variable_2_history";

            this.btnMMIO_variable_2_history.Size = new System.Drawing.Size(28, 28);
            this.btnMMIO_variable_2_history.TabIndex = 140;
            this.btnMMIO_variable_2_history.Text = "%";
            this.btnMMIO_variable_2_history.UseVisualStyleBackColor = true;
            this.btnMMIO_variable_2_history.Click += new System.EventHandler(this.btnMMIO_variable_2_history_Click);
            // 
            this.btnMMIO_variable_history.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMMIO_variable_history.Image = null;
            this.btnMMIO_variable_history.Location = new System.Drawing.Point(252, 52);
            this.btnMMIO_variable_history.Name = "btnMMIO_variable_history";

            this.btnMMIO_variable_history.Size = new System.Drawing.Size(28, 28);
            this.btnMMIO_variable_history.TabIndex = 139;
            this.btnMMIO_variable_history.Text = "%";
            this.btnMMIO_variable_history.UseVisualStyleBackColor = true;
            this.btnMMIO_variable_history.Click += new System.EventHandler(this.btnMMIO_variable_history_Click);
            // 
            this.pnlVariableInUse_1_history.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_1_history.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlVariableInUse_1_history.BackColor = System.Drawing.Color.Lime;
            this.pnlVariableInUse_1_history.Location = new System.Drawing.Point(252, 79);
            this.pnlVariableInUse_1_history.Name = "pnlVariableInUse_1_history";
            this.pnlVariableInUse_1_history.Size = new System.Drawing.Size(28, 6);
            this.pnlVariableInUse_1_history.TabIndex = 141;
            // 
            this.nudHistory_keep_for.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHistory_keep_for.Location = new System.Drawing.Point(118, 78);
            this.nudHistory_keep_for.Maximum = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            this.nudHistory_keep_for.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHistory_keep_for.Name = "nudHistory_keep_for";
            this.nudHistory_keep_for.Size = new System.Drawing.Size(56, 20);
            this.nudHistory_keep_for.TabIndex = 136;
            this.nudHistory_keep_for.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudHistory_keep_for, "Reading update and is related to screen update");
            this.nudHistory_keep_for.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudHistory_keep_for.ValueChanged += new System.EventHandler(this.nudHistory_keep_for_ValueChanged);
            // 
            this.labelTS259.Image = null;
            this.labelTS259.Location = new System.Drawing.Point(18, 78);
            this.labelTS259.Name = "labelTS259";
            this.labelTS259.Size = new System.Drawing.Size(94, 16);
            this.labelTS259.TabIndex = 135;
            this.labelTS259.Text = "Keep For (secs) :";
            this.labelTS259.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.nudHistory_update.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHistory_update.Location = new System.Drawing.Point(118, 52);
            this.nudHistory_update.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudHistory_update.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nudHistory_update.Name = "nudHistory_update";
            this.nudHistory_update.Size = new System.Drawing.Size(56, 20);
            this.nudHistory_update.TabIndex = 134;
            this.nudHistory_update.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudHistory_update, "Reading update and is related to screen update");
            this.nudHistory_update.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudHistory_update.ValueChanged += new System.EventHandler(this.nudHistory_update_ValueChanged);
            // 
            this.labelTS252.Image = null;
            this.labelTS252.Location = new System.Drawing.Point(41, 52);
            this.labelTS252.Name = "labelTS252";
            this.labelTS252.Size = new System.Drawing.Size(71, 16);
            this.labelTS252.TabIndex = 133;
            this.labelTS252.Text = "Update (ms):";
            this.labelTS252.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            this.nudHistory_vertical_ratio.DecimalPlaces = 3;
            this.nudHistory_vertical_ratio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudHistory_vertical_ratio.Location = new System.Drawing.Point(118, 26);
            this.nudHistory_vertical_ratio.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHistory_vertical_ratio.Minimum = new decimal(new int[] {
            130,
            0,
            0,
            196608});
            this.nudHistory_vertical_ratio.Name = "nudHistory_vertical_ratio";
            this.nudHistory_vertical_ratio.Size = new System.Drawing.Size(56, 20);
            this.nudHistory_vertical_ratio.TabIndex = 132;
            this.nudHistory_vertical_ratio.TinyStep = false;
            this.toolTip1.SetToolTip(this.nudHistory_vertical_ratio, "Vertical size, compared to width");
            this.nudHistory_vertical_ratio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudHistory_vertical_ratio.ValueChanged += new System.EventHandler(this.nudHistory_vertical_ratio_ValueChanged);
            // 
            this.labelTS253.AutoSize = true;
            this.labelTS253.Image = null;
            this.labelTS253.Location = new System.Drawing.Point(45, 28);
            this.labelTS253.Name = "labelTS253";
            this.labelTS253.Size = new System.Drawing.Size(73, 13);
            this.labelTS253.TabIndex = 131;
            this.labelTS253.Text = "Vertical Ratio:";
            // 
            this.labelTS258.AutoSize = true;
            this.labelTS258.Image = null;
            this.labelTS258.Location = new System.Drawing.Point(198, 28);
            this.labelTS258.Name = "labelTS258";
            this.labelTS258.Size = new System.Drawing.Size(68, 13);
            this.labelTS258.TabIndex = 130;
            this.labelTS258.Text = "Background:";
            // 
            this.clrbtnHistory_background.Automatic = "Automatic";
            this.clrbtnHistory_background.Color = System.Drawing.Color.LimeGreen;
            this.clrbtnHistory_background.ForeColor = System.Drawing.Color.Black;
            this.clrbtnHistory_background.Image = null;
            this.clrbtnHistory_background.Location = new System.Drawing.Point(272, 23);
            this.clrbtnHistory_background.MoreColors = "More Colors...";
            this.clrbtnHistory_background.Name = "clrbtnHistory_background";

            this.clrbtnHistory_background.Size = new System.Drawing.Size(40, 23);
            this.clrbtnHistory_background.TabIndex = 129;
            this.toolTip1.SetToolTip(this.clrbtnHistory_background, "Background colour");
            this.clrbtnHistory_background.Changed += new System.EventHandler(this.clrbtnHistory_background_Changed);
            // 
            this.chkHistory_fade_tx.AutoSize = true;
            this.chkHistory_fade_tx.Image = null;
            this.chkHistory_fade_tx.Location = new System.Drawing.Point(229, 116);
            this.chkHistory_fade_tx.Name = "chkHistory_fade_tx";
            this.chkHistory_fade_tx.Size = new System.Drawing.Size(82, 17);
            this.chkHistory_fade_tx.TabIndex = 3;
            this.chkHistory_fade_tx.Text = "Fade on TX";
            this.chkHistory_fade_tx.UseVisualStyleBackColor = true;
            this.chkHistory_fade_tx.CheckedChanged += new System.EventHandler(this.chkHistory_fade_tx_CheckedChanged);
            // 
            this.chkHistory_fade_rx.AutoSize = true;
            this.chkHistory_fade_rx.Image = null;
            this.chkHistory_fade_rx.Location = new System.Drawing.Point(229, 93);
            this.chkHistory_fade_rx.Name = "chkHistory_fade_rx";
            this.chkHistory_fade_rx.Size = new System.Drawing.Size(83, 17);
            this.chkHistory_fade_rx.TabIndex = 2;
            this.chkHistory_fade_rx.Text = "Fade on RX";
            this.chkHistory_fade_rx.UseVisualStyleBackColor = true;
            this.chkHistory_fade_rx.CheckedChanged += new System.EventHandler(this.chkHistory_fade_rx_CheckedChanged);

            this.Controls.Add(this.grpMultiMeterHolder);
            this.Controls.Add(this.grpMeterItemClockSettings); this.grpMeterItemClockSettings.Visible = false;
            this.Controls.Add(this.grpMeterItemVfoDisplaySettings); this.grpMeterItemVfoDisplaySettings.Visible = false;
            this.Controls.Add(this.grpMeterItemSpacerSettings); this.grpMeterItemSpacerSettings.Visible = false;
            this.Controls.Add(this.grpTextOverlay); this.grpTextOverlay.Visible = false;
            this.Controls.Add(this.grpMeterItemDataOutNode); this.grpMeterItemDataOutNode.Visible = false;
            this.Controls.Add(this.grpMeterItemRotator); this.grpMeterItemRotator.Visible = false;
            this.Controls.Add(this.grpLedIndicator); this.grpLedIndicator.Visible = false;
            this.Controls.Add(this.grpWebImage); this.grpWebImage.Visible = false;
            this.Controls.Add(this.grpBandButtons); this.grpBandButtons.Visible = false;
            this.Controls.Add(this.grpHistoryItem); this.grpHistoryItem.Visible = false;
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(724, 410);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "PowerSDR - Meters/Gadgets";
            this.ShowInTaskbar = true;
        }

        private bool _shutdown;
        private void P27ThetisMetersConfigForm_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                try { chkContainerHighlight.Checked = false; } catch { }
                MeterManager.HighlightContainer("");
                try { console.P27SaveMetersConfiguration(); } catch { }
            }
            else if (!_shutdown) updateMeter2Controls("");
        }

        private void P27ThetisMetersConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_shutdown && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            MeterManager.HighlightContainer("");
        }

        internal void CloseForShutdown()
        {
            _shutdown = true;
            try { console.P27SaveMetersConfiguration(); } catch { }
            try { Close(); } catch { }
            try { Dispose(); } catch { }
        }

        // multimeter 2
        private const int MAX_CONTAINERS = 50;

        private class clsContainerComboboxItem
        {
            public string Text { get; set; }
            public string ID { get; set; }

            public override string ToString()
            {
                return Text;
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
            clsContainerComboboxItem cci = comboContainerSelect.SelectedItem as clsContainerComboboxItem;
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
        private void updateMeter2Controls(string sId = "")
        {
            bool bEnableAdd = MeterManager.TotalMeterContainers < MAX_CONTAINERS;

            btnAddRX1Container.Enabled = bEnableAdd;
            btnAddRX2Container.Enabled = bEnableAdd && console.RX2Enabled;

            comboContainerSelect.Text = "";
            comboContainerSelect.Items.Clear();
            int i = 0;
            int nSelect = 0;

            // add the containers to the list
            foreach (KeyValuePair<string, ucMeter> kvp in MeterManager.MeterContainers)
            {
                clsContainerComboboxItem cci = new clsContainerComboboxItem();
                cci.Text = "Container " + (i + 1).ToString() + " TRX" + kvp.Value.RX.ToString();
                cci.ID = kvp.Value.ID;

                comboContainerSelect.Items.Add(cci);

                if (cci.ID == sId && nSelect == 0) nSelect = i;

                i++;
            }

            bool bEnableControls = false;

            if (comboContainerSelect.Items.Count > 0)
            {
                comboContainerSelect.SelectedIndex = nSelect;

                bEnableControls = true;
            }
            else
            {
                comboContainerSelect.Text = "";
            }

            bool locked = chkLockContainer.Checked;

            btnContainerDelete.Enabled = bEnableControls && !locked;
            chkContainerHighlight.Enabled = bEnableControls;
            comboContainerSelect.Enabled = bEnableControls;
            clrbtnContainerBackground.Enabled = bEnableControls;
            chkContainerBorder.Enabled = bEnableControls;
            chkContainerNoTitle.Enabled = bEnableControls;
            chkMultiMeter_auto_container_height.Enabled = bEnableControls;
            chkLockContainer.Enabled = bEnableControls;
            chkContainerShowRX.Enabled = bEnableControls;
            chkContainerShowTX.Enabled = bEnableControls;
            chkContainerMinimises.Enabled = bEnableControls;
            txtContainerNotes.Enabled = bEnableControls;
            lblMMContainerBackground.Enabled = bEnableControls;
            lblMMContainerNotes.Enabled = bEnableControls;
            lstMetersAvailable.Enabled = bEnableControls;
            lstMetersInUse.Enabled = bEnableControls;
            btnAddMeterItem.Enabled = bEnableControls && !locked;
            btnRemoveMeterItem.Enabled = bEnableControls && !locked;
            btnMeterUp.Enabled = bEnableControls && !locked && lstMetersInUse.Items.Count > 0;
            btnMeterDown.Enabled = bEnableControls && !locked && lstMetersInUse.Items.Count > 0;

            btnMeterCopySettings.Enabled = bEnableControls && lstMetersInUse.Items.Count > 0;
            btnMeterPasteSettings.Enabled = bEnableControls && lstMetersInUse.Items.Count > 0;

            if (!bEnableControls) txtContainerNotes.Text = "";
            if (!bEnableControls) comboContainerSelect.Text = "";

            updateMeterLists();
        }
        private void updateMeterLists()
        {
            //lstMetersInUse.BeginUpdate();
            //lstMetersAvailable.BeginUpdate();

            //lstMetersInUse.SuspendLayout();
            //lstMetersAvailable.SuspendLayout();

            lstMetersInUse.Items.Clear();
            lstMetersAvailable.Items.Clear();

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
                //else
                //{
                //if (mt != MeterType.SPACER && mt != MeterType.TEXT_OVERLAY)
                //{
                clsMeterTypeComboboxItem mtci2 = new clsMeterTypeComboboxItem(mt, -1);
                notinuse.Add(mtci2);
                //}
                //}
            }
            //// add spacer and overlay here always to notinuse
            //clsMeterTypeComboboxItem mtci_tmp = new clsMeterTypeComboboxItem(MeterType.SPACER, -1);
            //notinuse.Add(mtci_tmp);
            //mtci_tmp = new clsMeterTypeComboboxItem(MeterType.TEXT_OVERLAY, -1);
            //notinuse.Add(mtci_tmp);

            foreach (clsMeterTypeComboboxItem mtci in notinuse)
            {
                lstMetersAvailable.Items.Add(mtci);
            }
            foreach (clsMeterTypeComboboxItem mtci in inuse.OrderBy(o => o.Order))
            {
                lstMetersInUse.Items.Add(mtci);
            }

            lstMetersAvailable_SelectedIndexChanged(this, EventArgs.Empty);
            lstMetersInUse_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void btnContainerDelete_Click(object sender, EventArgs e)
        {
            if (chkLockContainer.Checked) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;

            if (cci != null)
            {
                MeterManager.RemoveMeterContainer(cci.ID);
                comboContainerSelect.Items.Remove(cci);

                updateMeter2Controls();
            }
        }

        private void comboContainerSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;

            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci == null) return;

            if (chkContainerHighlight.Checked)
            {
                MeterManager.HighlightContainer(cci.ID);
            }

            chkContainerBorder.Checked = MeterManager.ContainerHasBorder(cci.ID);
            clrbtnContainerBackground.Color = MeterManager.GetContainerBackgroundColour(cci.ID);
            chkContainerNoTitle.Checked = MeterManager.ContainerNoTitleBar(cci.ID);

            chkLockContainer.Checked = MeterManager.ContainerLocked(cci.ID);
            chkLockContainer_CheckedChanged(this, EventArgs.Empty); // force it

            chkContainerShowRX.Checked = MeterManager.ContainerShowOnRX(cci.ID);
            chkContainerShowTX.Checked = MeterManager.ContainerShowOnTX(cci.ID);
            chkContainerMinimises.Checked = MeterManager.ContainerMinimises(cci.ID);
            txtContainerNotes.Text = MeterManager.GetContainerNotes(cci.ID);
            chkMultiMeter_auto_container_height.Checked = MeterManager.ContainerAutoHeight(cci.ID);

            updateMeterLists();
        }

        private void chkContainerHighlight_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (chkContainerHighlight.Checked)
            {
                clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
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
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ShowContainerOnRX(cci.ID, chkContainerShowRX.Checked);
            }
        }
        private void chkContainerShowTX_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ShowContainerOnTX(cci.ID, chkContainerShowTX.Checked);
            }
        }
        private void txtContainerNotes_TextChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                string sTmp = MeterManager.GetContainerNotes(cci.ID);
                if (txtContainerNotes.Text != sTmp)
                    MeterManager.ContainerNotes(cci.ID, txtContainerNotes.Text);
            }
        }
        private void btnAddMeterItem_Click(object sender, EventArgs e)
        {
            if (chkLockContainer.Checked) return;
            clsMeterTypeComboboxItem mti = lstMetersAvailable.SelectedItem as clsMeterTypeComboboxItem;
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
            btnAddMeterItem.Enabled = !chkLockContainer.Checked && lstMetersAvailable.SelectedIndex >= 0;
        }

        private void lstMetersInUse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;

            bool bEnabled = lstMetersInUse.SelectedIndex >= 0;

            if (bEnabled)
                updateItemSettingsControlsForSelected();
            else
                setupMMSettingsGroupBoxes(MeterType.NONE);

            btnRemoveMeterItem.Enabled = !chkLockContainer.Checked && bEnabled;
            btnMeterUp.Enabled = !chkLockContainer.Checked && bEnabled;
            btnMeterDown.Enabled = !chkLockContainer.Checked && bEnabled;

            btnMeterCopySettings.Enabled = bEnabled;
            btnMeterPasteSettings.Enabled = bEnabled && canPasteSettings();
        }

        private void btnRemoveMeterItem_Click(object sender, EventArgs e)
        {
            if (chkLockContainer.Checked) return;
            clsMeterTypeComboboxItem mti = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mti == null) return;

            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            m.RemoveMeterType(mti.MeterType, mti.Order, true);

            updateMeterLists();

            lstMetersInUse_SelectedIndexChanged(sender, e);
        }

        private void btnMeterUp_Click(object sender, EventArgs e)
        {
            if (chkLockContainer.Checked) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            int n = lstMetersInUse.SelectedIndex - 1;
            if (n < 0) return;

            m.SetOrderForMeterType(mtci.MeterType, n, true, true, mtci.Order);

            updateMeterLists();

            lstMetersInUse.SelectedIndex = n;
        }

        private void btnMeterDown_Click(object sender, EventArgs e)
        {
            if (chkLockContainer.Checked) return;
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return;

            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return;

            int n = lstMetersInUse.SelectedIndex + 1;
            if (n > lstMetersInUse.Items.Count - 1) return;

            m.SetOrderForMeterType(mtci.MeterType, n, true, false, mtci.Order);

            updateMeterLists();

            lstMetersInUse.SelectedIndex = n;
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

            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return "";
            if (!m.HasMeterType(mtci.MeterType)) return "";

            return m.MeterGroupID(mtci.MeterType, mtci.Order);
        }
        private MeterType meterItemGroupTypefromSelected()
        {
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return MeterType.NONE;

            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return MeterType.NONE;
            if (!m.HasMeterType(mtci.MeterType)) return MeterType.NONE;

            return mtci.MeterType;
        }
        private void chkMeterItemHistory_CheckedChanged(object sender, EventArgs e)
        {
            bool bEnabled = chkMeterItemHistory.Checked;

            updateHistoryControls(bEnabled, Color.Red, false);

            updateMeterType();
        }
        private MeterManager.clsIGSettings updateMeterType()
        {
            if (initializing || _ignoreMeterItemChangeEvents) return null;

            string mgID = meterItemGroupIDfromSelected();
            if (mgID == "") return null;

            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
            if (mtci == null) return null;
            
            MeterManager.clsMeter m = meterFromSelectedContainer();
            if (m == null) return null;

            MeterType mt = meterItemGroupTypefromSelected();
            if (mt == MeterType.NONE) return null;

            MeterManager.clsIGSettings igs = m.GetSettingsForMeterGroup(mt, mtci.Order);
            if (igs == null) return null;

            if (mt == MeterType.HISTORY)
            {
                igs.SetSetting<float>("history_vertical_ratio", (float)nudHistory_vertical_ratio.Value);
                igs.SetSetting<System.Drawing.Color>("history_background_colour", clrbtnHistory_background.Color);
                igs.SetSetting<float>("history_update", (float)nudHistory_update.Value);
                igs.SetSetting<float>("history_keep_for", (float)nudHistory_keep_for.Value);
                
                clsComboHistoryItem chi = comboHistory_reading_0.SelectedItem as clsComboHistoryItem;
                if(chi != null)
                    igs.SetSetting<Reading>("history_reading_0", chi.Reading);
                else
                    igs.SetSetting<Reading>("history_reading_0", Reading.SIGNAL_STRENGTH);

                chi = comboHistory_reading_1.SelectedItem as clsComboHistoryItem;
                if (chi != null)
                    igs.SetSetting<Reading>("history_reading_1", chi.Reading);
                else
                    igs.SetSetting<Reading>("history_reading_1", Reading.SIGNAL_STRENGTH);

                igs.SetSetting<bool>("history_auto_scale_0", chkHistory_auto_0_scale.Checked);
                igs.SetSetting<float>("history_min_0", (float)nudHistory_axis0_min.Value);
                igs.SetSetting<float>("history_max_0", (float)nudHistory_axis0_max.Value);

                igs.SetSetting<bool>("history_show_scale_1", chkHistory_1_show_axis.Checked);
                igs.SetSetting<bool>("history_auto_scale_1", chkHistory_auto_1_scale.Checked);
                igs.SetSetting<float>("history_min_1", (float)nudHistory_axis1_min.Value);
                igs.SetSetting<float>("history_max_1", (float)nudHistory_axis1_max.Value);

                igs.SetSetting<System.Drawing.Color>("history_colour_0", clrbtnHistory_colour_0.Color);
                igs.SetSetting<System.Drawing.Color>("history_colour_1", clrbtnHistory_colour_1.Color);

                igs.SetSetting<System.Drawing.Color>("history_colour_lines", clrbtnHistory_lines.Color);
                igs.SetSetting<System.Drawing.Color>("history_colour_time", clrbtnHistory_time.Color);

                igs.FadeOnRx = chkHistory_fade_rx.Checked;
                igs.FadeOnTx = chkHistory_fade_tx.Checked;
            }
            else if (mt == MeterType.BAND_BUTTONS || mt == MeterType.MODE_BUTTONS || mt == MeterType.FILTER_BUTTONS || mt == MeterType.ANTENNA_BUTTONS || mt == MeterType.TUNESTEP_BUTTONS)
            {
                if(mt == MeterType.TUNESTEP_BUTTONS)
                {
                    int max_buttons = ucTunestepOptionsGrid_buttons.GetCheckedCount();
                    max_buttons = Math.Max(1, max_buttons);
                    if (nudBandButtons_columns.Value > max_buttons) nudBandButtons_columns.Value = max_buttons;
                    if (nudBandButtons_columns.Maximum != max_buttons) nudBandButtons_columns.Maximum = max_buttons;
                }
                else if (mt == MeterType.ANTENNA_BUTTONS)
                {
                    int max_buttons = getTotalColumnsNeededForAntennaButtons();
                    max_buttons = Math.Max(1, max_buttons);
                    if (nudBandButtons_columns.Value > max_buttons) nudBandButtons_columns.Value = max_buttons;
                    if (nudBandButtons_columns.Maximum != max_buttons) nudBandButtons_columns.Maximum = max_buttons;
                }

                igs.SetSetting<int>("buttonbox_columns", (int)nudBandButtons_columns.Value);
                igs.SetSetting<float>("buttonbox_border", (float)nudBandButtons_border.Value);
                igs.SetSetting<float>("buttonbox_margin", (float)nudBandButtons_margin.Value);
                igs.SetSetting<float>("buttonbox_radius", (float)nudBandButtons_radius.Value);
                igs.SetSetting<float>("buttonbox_height_ratio", (float)nudBandButtons_height_ratio.Value);

                igs.SetSetting<bool>("buttonbox_use_indicator", chkBandButtons_use_indicator.Checked);
                igs.SetSetting<float>("buttonbox_indicator_border", (float)nudBandButtons_indicator_border.Value);
                igs.SetSetting<System.Drawing.Color>("buttonbox_on_colour", clrbtnBandButtons_indicator_on.Color);
                igs.SetSetting<System.Drawing.Color>("buttonbox_off_colour", clrbtnBandButtons_indicator_off.Color);

                igs.SetSetting<System.Drawing.Color>("buttonbox_fill_colour", clrbtnBandButtons_fill.Color);
                igs.SetSetting<System.Drawing.Color>("buttonbox_hover_colour", clrbtnBandButtons_hover.Color);
                igs.SetSetting<System.Drawing.Color>("buttonbox_border_colour", clrbtnBandButtons_border.Color);

                igs.SetSetting<bool>("buttonbox_use_off_colour", chkBandButtons_band_inactive_use.Checked);

                igs.SetSetting<MeterManager.clsButtonBox.IndicatorType>("buttonbox_indicator_type", (MeterManager.clsButtonBox.IndicatorType)((int)nudBandButtons_indicator_style.Value));

                igs.SetSetting<float>("buttonbox_font_scale", (float)nudButtonBox_font_scale.Value);
                igs.SetSetting<float>("buttonbox_font_shift_x", (float)nudButtonBox_font_x_shift.Value);
                igs.SetSetting<float>("buttonbox_font_shift_y", (float)nudButtonBox_font_y_shift.Value);

                if(mt == MeterType.TUNESTEP_BUTTONS)
                {
                    igs.SetSetting<int>("buttonbox_tunestep_bitfield", ucTunestepOptionsGrid_buttons.Bitfield);
                }
                else if (mt == MeterType.ANTENNA_BUTTONS)
                {
                    igs.SetSetting<bool>("buttonbox_rx1", chkButtonBox_antenna_rx1.Checked);
                    igs.SetSetting<bool>("buttonbox_rx2", chkButtonBox_antenna_rx2.Checked);
                    igs.SetSetting<bool>("buttonbox_rx3", chkButtonBox_antenna_rx3.Checked);
                    igs.SetSetting<bool>("buttonbox_tx1", chkButtonBox_antenna_tx1.Checked);
                    igs.SetSetting<bool>("buttonbox_tx2", chkButtonBox_antenna_tx2.Checked);
                    igs.SetSetting<bool>("buttonbox_tx3", chkButtonBox_antenna_tx3.Checked);
                    igs.SetSetting<bool>("buttonbox_byp", chkButtonBox_antenna_byp.Checked);
                    igs.SetSetting<bool>("buttonbox_ext1", chkButtonBox_antenna_ext1.Checked);
                    igs.SetSetting<bool>("buttonbox_xvtr", chkButtonBox_antenna_xvtr.Checked);
                    igs.SetSetting<bool>("buttonbox_rxtxant", chkButtonBox_antenna_rxtxant.Checked);
                }

                if (_bandButtons_font != null)
                {
                    igs.FontFamily1 = _bandButtons_font.FontFamily.Name;
                    igs.FontStyle1 = _bandButtons_font.Style;
                    //igs.FontSize1 = _bandButtons_font.Size; size not used
                }

                igs.FadeOnRx = chkBandButtons_fade_rx.Checked;
                igs.FadeOnTx = chkBandButtons_fade_tx.Checked;
            }
            else if (mt == MeterType.WEB_IMAGE)
            {
                igs.UpdateInterval = (int)nudWebImage_update_interval.Value;
                igs.EyeScale = (float)nudWebImage_width_scale.Value;
                igs.FadeOnRx = chkWebImage_fade_rx.Checked;
                igs.FadeOnTx = chkWebImage_fade_tx.Checked;
                igs.Text1 = txtWebImage_url.Text;
                igs.DarkMode = chkWebImage_bypass_cache.Checked;
            }
            else if(mt == MeterType.ROTATOR)
            {
                igs.UpdateInterval = (int)nudMeterItemUpdateRateRotator.Value;
                igs.Colour = Color.FromArgb(255, clrbtnMeterItemHBackgroundRotator.Color);
                igs.TitleColor = clrbtnMeterItemRotatorArrow.Color;
                igs.MarkerColour = clrbtnMeterItemRotatorLargeDot.Color;
                igs.SubMarkerColour = clrbtnMeterItemRotatorSmallDot.Color;
                igs.ShowMarker = chkMeterItemRotatorShowBeamWidth.Checked;
                igs.LowColor = clrbtnMeterItemRotatorBeamWidth.Color;
                igs.HighColor = clrbtnMeterItemRotatorText.Color;
                igs.ShowHistory = chkMeterItemRotatorCardinals.Checked;
                igs.FadeOnRx = chkMeterItemFadeOnRxRotator.Checked;
                igs.FadeOnTx = chkMeterItemFadeOnTxRotator.Checked;
                igs.DarkMode = chkMeterItemDarkModeRotator.Checked;
                igs.AttackRatio = (float)nudMeterItemRotatorBeamWidth.Value;
                igs.EyeScale = (float)nudMeterItemRotator_padding.Value;

                //
                if (radMeterItemRotator_show_az.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsRotatorItem.RotatorMode.AZ;
                else if (radMeterItemRotator_show_ele.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsRotatorItem.RotatorMode.ELE;
                else if (radMeterItemRotator_show_both.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsRotatorItem.RotatorMode.BOTH;
                //

                igs.ShowType = chkMeterItemRotatorAllowControl.Checked;
                igs.HistoryColor = clrbtnMeterItemRotatorControlColour.Color;
                igs.Text1 = txtMeterItemRotatorAZcommand.Text;
                igs.Text2 = txtMeterItemRotatorELEcommand.Text;
                igs.FontFamily1 = txtMeterItemRotatorSTOPcommand.Text;

                Guid guid = MultiMeterIO.GuidfromFourChar(txtRotator_4charID.Text);
                if (guid != Guid.Empty)
                {
                    igs.SetMMIOGuid(2, guid);
                }
                else
                {
                    igs.SetMMIOGuid(2, Guid.Empty);
                }

                igs.SetSetting<float>("rotator_beamwidth_alpha", (float)nudMeterItemRotatorBeamWidth_alpha.Value);
            }
            else if (mt == MeterType.DATA_OUT)
            {
                Guid guid = MultiMeterIO.GuidfromFourChar(txtDataOutNode_4charID.Text);
                if (guid != Guid.Empty)
                {
                    igs.SetMMIOGuid(0, guid);
                    igs.UpdateInterval = (int)nudDataOutNode_sendinterval.Value;
                }
                else
                {
                    igs.SetMMIOGuid(0, Guid.Empty);
                    igs.UpdateInterval = 500;
                }
            }
            else if (mt == MeterType.SIGNAL_TEXT)
            {
                igs.UpdateInterval = (int)nudMeterItemUpdateRate.Value;
                igs.AttackRatio = (float)nudMeterItemAttackRate.Value;
                igs.DecayRatio = (float)nudMeterItemDecayRate.Value;
                igs.FadeOnRx = chkMeterItemFadeOnRx.Checked;
                igs.FadeOnTx = chkMeterItemFadeOnTx.Checked;
                igs.Colour = clrbtnMeterItemHBackground.Color;
                igs.MarkerColour = clrbtnMeterItemIndicator.Color;
                igs.SubMarkerColour = clrbtnMeterItemSubIndicator.Color;
                igs.ShowSubMarker = chkMeterItemShowSubIndicator.Checked;
                igs.PeakValueColour = clrbtnMeterItemPeakValueColour.Color;
                igs.PeakValue = chkMeterItemPeakValue.Checked;
                igs.Average = chkMeterItemSignalAverage.Checked;
                igs.HistoryDuration = (int)nudMeterItemHistoryDuration.Value;
                igs.IgnoreHistoryDuration = (int)nudMeterItemIgnoreHistoryDuration.Value;
            }
            else if (mt == MeterType.VFO_DISPLAY)
            {
                igs.Colour = clrbtnMMVfoDisplayBackground.Color;
                igs.TitleColor = clrbtnMMVfoDisplayTitle.Color;

                //using exisinng igs settings
                igs.MarkerColour = clrbtnMMVfoDisplayFrequency.Color;
                igs.SubMarkerColour = clrbtnMMVfoDisplayMode.Color;
                igs.LowColor = clrbtnMMVfoDisplaySplitBack.Color;
                igs.HighColor = clrbtnMMVfoDisplaySplit.Color;
                igs.PeakValueColour = clrbtnMMVfoDisplayRx.Color;
                igs.PeakHoldMarkerColor = clrbtnMMVfoDisplayTx.Color;
                igs.HistoryColor = clrbtnMMVfoDisplayFilter.Color;
                igs.SegmentedSolidLowColour = clrbtnMMVfoDisplayBand.Color;
                igs.PowerScaleColour = clrbtnMMVfoDigitHighlight.Color;

                igs.SetSetting<bool>("vfo_showbandtext", chkMultiMeter_vfo_show_bandtext.Checked);
                igs.SetSetting<System.Drawing.Color>("vfo_showbandtext_colour", clrbtnMultiMeter_vfo_show_bandtext.Color);
                igs.SetSetting<System.Drawing.Color>("vfo_frequency_small_numbers_colour", clrbtnMMVfoDisplayFrequency_small.Color);

                igs.SetSetting<System.Drawing.Color>("vfo_lock_colour", clrbtnMultiMeter_vfo_lock.Color);
                igs.SetSetting<System.Drawing.Color>("vfo_sync_colour", clrbtnMultiMeter_vfo_sync.Color);

                if (radMultiMeter_vfo_display_both.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsVfoDisplay.VFODisplayMode.VFO_BOTH;
                else if (radMultiMeter_vfo_display_vfoa.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsVfoDisplay.VFODisplayMode.VFO_A;
                else if (radMultiMeter_vfo_display_vfob.Checked)
                    igs.HistoryDuration = (int)MeterManager.clsVfoDisplay.VFODisplayMode.VFO_B;
            }
            else if (mt == MeterType.CLOCK)
            {
                igs.Colour = clrbtnMMClockBackground.Color;
                igs.ShowType = chkMMClockTitle.Checked;
                igs.TitleColor = clrbtnMMClockTitle.Color;
                igs.MarkerColour = clrbtnMMTime.Color;
                igs.SubMarkerColour = clrbtnMMDate.Color;
                igs.ShowMarker = radMM24Clock.Checked; // use the show marker bool for this                
            }
            else if (mt == MeterType.LED)
            {
                igs.FadeOnRx = chkLedIndicator_FadeOnRX.Checked;
                igs.FadeOnTx = chkLedIndicator_FadeOnTX.Checked;
                igs.Colour = clrbtnLedIndicator_true.Color;
                igs.MarkerColour = clrbtnLedIndicator_false.Color;

                igs.TitleColor = clrbtnLedIndicator_PanelBackground.Color;
                igs.HistoryColor = clrbtnLedIndicator_PanelBackgroundTX.Color;
                igs.ShowSubMarker = chkLedIndicator_ShowPanel.Checked;

                igs.EyeScale = (float)nudLedIndicator_xOffset.Value;
                igs.EyeBezelScale = (float)nudLedIndicator_yOffset.Value;
                igs.AttackRatio = (float)nudLedIndicator_xSize.Value;
                igs.DecayRatio = (float)nudLedIndicator_ySize.Value;

                igs.Text1 = txtLedIndicator_condition.Text;

                igs.SpacerPadding = (float)nudLedIndicator_PanelPadding.Value;

                igs.PeakHold = chkLed_show_true.Checked;
                igs.ShowMarker = chkLed_show_false.Checked;
                if (radLed_light_on_off.Checked)
                    igs.IgnoreHistoryDuration = 0;
                else if (radLed_light_blink.Checked)
                    igs.IgnoreHistoryDuration = 1;
                else if (radLed_light_pulsate.Checked)
                    igs.IgnoreHistoryDuration = 2;
                // also showhistory + showtype are return states for valid/error
            }
            else if (mt == MeterType.TEXT_OVERLAY)
            {
                igs.FadeOnRx = chkTextOverlay_FadeOnRX.Checked;
                igs.FadeOnTx = chkTextOverlay_FadeOnTX.Checked;
                igs.Colour = clrbtnTextOverlay_TextColour1.Color;
                igs.MarkerColour = clrbtnTextOverlay_TextColour2.Color;
                igs.SubMarkerColour = clrbtnTextOverlay_TextBackColour1.Color;
                igs.ShowMarker = chkTextOverlay_textback1.Checked;
                igs.PeakValueColour = clrbtnTextOverlay_TextBackColour2.Color;
                igs.ShowType = chkTextOverlay_textback2.Checked;

                igs.TitleColor = clrbtnTextOverlay_PanelBackground.Color;
                igs.HistoryColor = clrbtnTextOverlay_PanelBackgroundTX.Color;
                igs.ShowSubMarker = chkTextOverlay_ShowPanel.Checked;

                igs.EyeScale = (float)nudTextOverlay_RXxOffset.Value;
                igs.EyeBezelScale = (float)nudTextOverlay_RXyOffset.Value;
                igs.AttackRatio = (float)nudTextOverlay_TXxOffset.Value;
                igs.DecayRatio = (float)nudTextOverlay_TXyOffset.Value;

                igs.Text1 = txtTextOverlay_RXText.Text;
                igs.Text2 = txtTextOverlay_TXText.Text;

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

                igs.SpacerPadding = (float)nudTextOverlay_PanelPadding.Value;
            }
            else if (mt == MeterType.SPACER)
            {
                igs.Colour = clrbtnMeterItemHBackgroundSpacerRX.Color;
                igs.MarkerColour = clrbtnMeterItemHBackgroundSpacerTX.Color;
                igs.FadeOnRx = chkMeterItemFadeOnRxSpacer.Checked;
                igs.FadeOnTx = chkMeterItemFadeOnTxSpacer.Checked;
                igs.SpacerPadding = (float)nudMeterItemSpacerPadding.Value;
            }
            else
            {
                igs.LowColor = Color.FromArgb(255, clrbtnMeterItemLow.Color);
                igs.HighColor = Color.FromArgb(255, clrbtnMeterItemHigh.Color);
                igs.MarkerColour = Color.FromArgb(255, clrbtnMeterItemIndicator.Color);
                igs.SubMarkerColour = Color.FromArgb(255, clrbtnMeterItemSubIndicator.Color);
                igs.ShowMarker = chkMeterItemShowIndicator.Checked;
                igs.ShowSubMarker = chkMeterItemShowSubIndicator.Checked;
                igs.Colour = Color.FromArgb(255, clrbtnMeterItemHBackground.Color);
                igs.UpdateInterval = (int)nudMeterItemUpdateRate.Value;
                igs.AttackRatio = (float)nudMeterItemAttackRate.Value;
                igs.DecayRatio = (float)nudMeterItemDecayRate.Value;
                igs.ShowHistory = chkMeterItemHistory.Checked;
                igs.HistoryColor = Color.FromArgb(tbMeterItemHistoryAlpha.Value, clrbtnMeterItemHistory.Color);
                igs.Shadow = chkMeterItemShadow.Checked;
                igs.HistoryDuration = (int)nudMeterItemHistoryDuration.Value;
                igs.IgnoreHistoryDuration = (int)nudMeterItemIgnoreHistoryDuration.Value;

                if (chkMeterItemSegmented.Checked)
                    igs.BarStyle = MeterManager.clsBarItem.BarStyle.Segments;
                else if (chkMeterItemSolid.Checked)
                    igs.BarStyle = MeterManager.clsBarItem.BarStyle.SolidFilled;
                else
                    igs.BarStyle = MeterManager.clsBarItem.BarStyle.Line;

                igs.SegmentedSolidLowColour = clrbtnMeterItemSegmentedSolidColourLow.Color;
                igs.SegmentedSolidHighColour = clrbtnMeterItemSegmentedSolidColourHigh.Color;

                igs.PeakHold = chkMeterItemPeakHold.Checked;
                igs.PeakHoldMarkerColor = Color.FromArgb(255, clrbtnMeterItemPeakHold.Color);
                igs.HistoryDuration = (int)nudMeterItemHistoryDuration.Value;
                igs.FadeOnRx = chkMeterItemFadeOnRx.Checked;
                igs.FadeOnTx = chkMeterItemFadeOnTx.Checked;
                igs.ShowType = chkMeterItemTitle.Checked;
                igs.TitleColor = clrbtnMeterItemMeterTitle.Color;
                igs.PeakValue = chkMeterItemPeakValue.Checked;
                igs.PeakValueColour = clrbtnMeterItemPeakValueColour.Color;
                igs.EyeScale = (float)nudMeterItemEyeScale.Value;
                igs.EyeBezelScale = (float)nudMeterItemEyeBezelScale.Value;
                igs.MaxPower = (float)nudMeterItemsPowerLimit.Value;
                igs.PowerScaleColour = clrbtnMeterItemPowerScale.Color;

                if (mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE) igs.Average = chkMeterItemSignalAverage.Checked;
                if (mt == MeterType.ANANMM || mt == MeterType.CROSS) igs.DarkMode = chkMeterItemDarkMode.Checked;
            }

            m.ApplySettingsForMeterGroup(mt, igs, mtci.Order);

            updateLedValidControls();
            return igs;
        }
        private bool _ignoreMeterItemChangeEvents = false;
        private void updateItemSettingsControlsForSelected()
        {
            if (initializing) return;

            string mgID = meterItemGroupIDfromSelected();
            if (mgID == "") return;

            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
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
                mt != MeterType.HISTORY && mt != MeterType.TUNESTEP_BUTTONS
                )
            {
                switch (m.MeterVariables(mt))
                {
                    case 1:
                        btnMMIO_variable.Enabled = true;
                        btnMMIO_variable_2.Enabled = false;
                        toolTip1.SetToolTip(btnMMIO_variable, m.MeterVariablesReadingString(mt, 0));
                        pnlVariableInUse_1.Visible = variableInUse(0);
                        pnlVariableInUse_2.Visible = false;
                        break;
                    case 2:
                        btnMMIO_variable.Enabled = true;
                        btnMMIO_variable_2.Enabled = true;
                        toolTip1.SetToolTip(btnMMIO_variable, m.MeterVariablesReadingString(mt, 0));
                        toolTip1.SetToolTip(btnMMIO_variable_2, m.MeterVariablesReadingString(mt, 1));
                        pnlVariableInUse_1.Visible = variableInUse(0);
                        pnlVariableInUse_2.Visible = variableInUse(1);
                        break;
                    case 7:
                        //todo? anan mm
                        btnMMIO_variable.Enabled = false;
                        btnMMIO_variable_2.Enabled = false;
                        pnlVariableInUse_1.Visible = false;
                        pnlVariableInUse_2.Visible = false;
                        break;
                    default:
                        btnMMIO_variable.Enabled = false;
                        btnMMIO_variable_2.Enabled = false;
                        pnlVariableInUse_1.Visible = false;
                        pnlVariableInUse_2.Visible = false;
                        break;
                }
            }
            else if (mt == MeterType.ROTATOR)
            {
                // unique controls for rotator as own setting grp
                switch (m.MeterVariables(mt))
                {
                    case 2:
                        btnMMIO_variable_rotator.Enabled = true;
                        btnMMIO_variable_2_rotator.Enabled = true;
                        toolTip1.SetToolTip(btnMMIO_variable_rotator, m.MeterVariablesReadingString(mt, 0));
                        toolTip1.SetToolTip(btnMMIO_variable_2_rotator, m.MeterVariablesReadingString(mt, 1));
                        pnlVariableInUse_1_rotator.Visible = variableInUse(0);
                        pnlVariableInUse_2_rotator.Visible = variableInUse(1);
                        break;
                }
            }
            else if (mt == MeterType.HISTORY)
            {
                // unique controls for history as own setting grp
                switch (m.MeterVariables(mt))
                {
                    case 2:
                        btnMMIO_variable_history.Enabled = true;
                        btnMMIO_variable_2_history.Enabled = true;
                        toolTip1.SetToolTip(btnMMIO_variable_history, m.MeterVariablesReadingString(mt, 0));
                        toolTip1.SetToolTip(btnMMIO_variable_2_history, m.MeterVariablesReadingString(mt, 1));
                        pnlVariableInUse_1_history.Visible = variableInUse(0);
                        pnlVariableInUse_2_history.Visible = variableInUse(1);
                        break;
                }
            }

            if (mt == MeterType.HISTORY)
            {
                nudHistory_vertical_ratio.Value = (decimal)igs.GetSetting<float>("history_vertical_ratio", true, 0.130f, 1f, 0.5f);
                clrbtnHistory_background.Color = igs.GetSetting<System.Drawing.Color>("history_background_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Black);
                nudHistory_update.Value = (decimal)igs.GetSetting<float>("history_update", true, 50f, 10000f, 0.5f);
                nudHistory_keep_for.Value = (decimal)igs.GetSetting<float>("history_keep_for", true, 1f, 86400f, 20f);
                Reading r = igs.GetSetting<Reading>("history_reading_0", false, Reading.NONE, Reading.NONE, Reading.SIGNAL_STRENGTH);
                foreach(clsComboHistoryItem chi in comboHistory_reading_0.Items)
                {
                    if (chi.Reading == r)
                    {
                        comboHistory_reading_0.SelectedItem = chi;
                        break;
                    }
                }
                r = igs.GetSetting<Reading>("history_reading_1", false, Reading.NONE, Reading.NONE, Reading.SIGNAL_STRENGTH);
                foreach (clsComboHistoryItem chi in comboHistory_reading_1.Items)
                {
                    if (chi.Reading == r)
                    {
                        comboHistory_reading_1.SelectedItem = chi;
                        break;
                    }
                }

                chkHistory_auto_0_scale.Checked = igs.GetSetting<bool>("history_auto_scale_0", false, false, false, true);
                nudHistory_axis0_min.Value = (decimal)igs.GetSetting<float>("history_min_0", true, -10000f, 10000f, -150f);
                nudHistory_axis0_max.Value = (decimal)igs.GetSetting<float>("history_max_0", true, -10000f, 10000f, 0f);

                chkHistory_1_show_axis.Checked = igs.GetSetting<bool>("history_show_scale_1", false, false, false, true);
                chkHistory_auto_1_scale.Checked = igs.GetSetting<bool>("history_auto_scale_1", false, false, false, true);
                nudHistory_axis1_min.Value = (decimal)igs.GetSetting<float>("history_min_1", true, -10000f, 10000f, -150f);
                nudHistory_axis1_max.Value = (decimal)igs.GetSetting<float>("history_max_1", true, -10000f, 10000f, 0f);

                clrbtnHistory_colour_0.Color = igs.GetSetting<System.Drawing.Color>("history_colour_0", false, Color.Empty, Color.Empty, System.Drawing.Color.Red);
                clrbtnHistory_colour_1.Color = igs.GetSetting<System.Drawing.Color>("history_colour_1", false, Color.Empty, Color.Empty, System.Drawing.Color.Yellow);

                clrbtnHistory_lines.Color = igs.GetSetting<System.Drawing.Color>("history_colour_lines", false, Color.Empty, Color.Empty, System.Drawing.Color.White);
                clrbtnHistory_time.Color = igs.GetSetting<System.Drawing.Color>("history_colour_time", false, Color.Empty, Color.Empty, System.Drawing.Color.Gray);

                chkHistory_fade_rx.Checked = igs.FadeOnRx;
                chkHistory_fade_tx.Checked = igs.FadeOnTx;
            }
            else if(mt == MeterType.BAND_BUTTONS || mt == MeterType.MODE_BUTTONS || mt == MeterType.FILTER_BUTTONS || mt == MeterType.ANTENNA_BUTTONS || mt == MeterType.TUNESTEP_BUTTONS)
            {
                int columns = 1;
                int max_buttons = 1;
                switch (mt)
                {
                    case MeterType.BAND_BUTTONS:
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, 15, 15);
                        if (nudBandButtons_columns.Value > 15) nudBandButtons_columns.Value = 15;
                        if (nudBandButtons_columns.Maximum != 15) nudBandButtons_columns.Maximum = 15;
                        break;
                    case MeterType.MODE_BUTTONS:
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, 12, 12);
                        if (nudBandButtons_columns.Value > 12) nudBandButtons_columns.Value = 12;
                        if (nudBandButtons_columns.Maximum != 12) nudBandButtons_columns.Maximum = 12;
                        break;
                    case MeterType.FILTER_BUTTONS:
                        max_buttons = m.RX == 1 ? 12 : 9; // rx2 only has 9 filter buttons
                        max_buttons = Math.Max(1, max_buttons);
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (nudBandButtons_columns.Value > max_buttons) nudBandButtons_columns.Value = max_buttons;
                        if (nudBandButtons_columns.Maximum != max_buttons) nudBandButtons_columns.Maximum = max_buttons;
                        break;
                    case MeterType.ANTENNA_BUTTONS:
                        max_buttons = getTotalColumnsNeededForAntennaButtons();
                        max_buttons = Math.Max(1, max_buttons);
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (nudBandButtons_columns.Value > max_buttons) nudBandButtons_columns.Value = max_buttons;
                        if (nudBandButtons_columns.Maximum != max_buttons) nudBandButtons_columns.Maximum = max_buttons;
                        break;
                    case MeterType.TUNESTEP_BUTTONS:
                        max_buttons = ucTunestepOptionsGrid_buttons.GetCheckedCount();
                        max_buttons = Math.Max(1, max_buttons);
                        columns = igs.GetSetting<int>("buttonbox_columns", true, 1, max_buttons, max_buttons);
                        if (nudBandButtons_columns.Value > max_buttons) nudBandButtons_columns.Value = max_buttons;
                        if (nudBandButtons_columns.Maximum != max_buttons) nudBandButtons_columns.Maximum = max_buttons;
                        break;
                }
                nudBandButtons_columns.Value = columns;
                nudBandButtons_border.Value = (decimal)igs.GetSetting<float>("buttonbox_border", true, 0f, 1f, 0.05f);
                nudBandButtons_margin.Value = (decimal)igs.GetSetting<float>("buttonbox_margin", true, 0f, 1f, 0f);
                nudBandButtons_radius.Value = (decimal)igs.GetSetting<float>("buttonbox_radius", true, 0f, 2f, 0f);
                nudBandButtons_height_ratio.Value = (decimal)igs.GetSetting<float>("buttonbox_height_ratio", true, 0.01f, 2f, 0.5f);

                chkBandButtons_use_indicator.Checked = igs.GetSetting<bool>("buttonbox_use_indicator", false, false, false, false);
                nudBandButtons_indicator_border.Value = (decimal)igs.GetSetting<float>("buttonbox_indicator_border", true, 0f, 1f, 0.05f);
                clrbtnBandButtons_indicator_on.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_on_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.CornflowerBlue);
                clrbtnBandButtons_indicator_off.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_off_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LightGray);

                clrbtnBandButtons_fill.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_fill_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Black);
                clrbtnBandButtons_hover.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_hover_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LightGray);
                clrbtnBandButtons_border.Color = igs.GetSetting<System.Drawing.Color>("buttonbox_border_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.White);

                chkBandButtons_band_inactive_use.Checked = igs.GetSetting<bool>("buttonbox_use_off_colour", false, false, false, false);

                nudBandButtons_indicator_style.Value = (decimal)((int)igs.GetSetting<MeterManager.clsButtonBox.IndicatorType>("buttonbox_indicator_type", true, MeterManager.clsButtonBox.IndicatorType.RING, MeterManager.clsButtonBox.IndicatorType.LAST, MeterManager.clsButtonBox.IndicatorType.RING));

                nudButtonBox_font_scale.Value = (decimal)igs.GetSetting<float>("buttonbox_font_scale", true, 0.01f, 2f, 1f);
                nudButtonBox_font_x_shift.Value = (decimal)igs.GetSetting<float>("buttonbox_font_shift_x", true, -0.25f, 0.25f, 0f);
                nudButtonBox_font_y_shift.Value = (decimal)igs.GetSetting<float>("buttonbox_font_shift_y", true, -0.25f, 0.25f, 0f);

                if (mt == MeterType.TUNESTEP_BUTTONS)
                {
                    ucTunestepOptionsGrid_buttons.Bitfield = igs.GetSetting<int>("buttonbox_tunestep_bitfield", true, 0, int.MaxValue, 0);
                }
                else if (mt == MeterType.ANTENNA_BUTTONS)
                {
                    chkButtonBox_antenna_rx1.Checked = igs.GetSetting<bool>("buttonbox_rx1", false, false, false, true);
                    chkButtonBox_antenna_rx2.Checked = igs.GetSetting<bool>("buttonbox_rx2", false, false, false, true);
                    chkButtonBox_antenna_rx3.Checked = igs.GetSetting<bool>("buttonbox_rx3", false, false, false, true);
                    chkButtonBox_antenna_tx1.Checked = igs.GetSetting<bool>("buttonbox_tx1", false, false, false, true);
                    chkButtonBox_antenna_tx2.Checked = igs.GetSetting<bool>("buttonbox_tx2", false, false, false, true);
                    chkButtonBox_antenna_tx3.Checked = igs.GetSetting<bool>("buttonbox_tx3", false, false, false, true);
                    chkButtonBox_antenna_byp.Checked = igs.GetSetting<bool>("buttonbox_byp", false, false, false, true);
                    chkButtonBox_antenna_ext1.Checked = igs.GetSetting<bool>("buttonbox_ext1", false, false, false, true);
                    chkButtonBox_antenna_xvtr.Checked = igs.GetSetting<bool>("buttonbox_xvtr", false, false, false, true);
                    chkButtonBox_antenna_rxtxant.Checked = igs.GetSetting<bool>("buttonbox_rxtxant", false, false, false, true);
                }

                _bandButtons_font = new Font(igs.FontFamily1, igs.FontSize1, igs.FontStyle1);
                chkBandButtons_fade_rx.Checked = igs.FadeOnRx;
                chkBandButtons_fade_tx.Checked = igs.FadeOnTx;

                updateButtonIndicatorControls();
            }
            else if (mt == MeterType.WEB_IMAGE)
            {
                nudWebImage_update_interval.Value = igs.UpdateInterval;
                nudWebImage_width_scale.Value = (decimal)igs.EyeScale;
                chkWebImage_fade_rx.Checked = igs.FadeOnRx;
                chkWebImage_fade_tx.Checked = igs.FadeOnTx;
                txtWebImage_url.Text = igs.Text1;
                chkWebImage_bypass_cache.Checked = igs.DarkMode;
                updateWebImageState((ImageFetcher.State)igs.HistoryDuration);
            }
            else if (mt == MeterType.ROTATOR)
            {
                nudMeterItemUpdateRateRotator.Value = igs.UpdateInterval;
                clrbtnMeterItemHBackgroundRotator.Color = igs.Colour;
                clrbtnMeterItemRotatorArrow.Color = igs.TitleColor;
                clrbtnMeterItemRotatorLargeDot.Color = igs.MarkerColour;
                clrbtnMeterItemRotatorSmallDot.Color = igs.SubMarkerColour;
                chkMeterItemRotatorShowBeamWidth.Checked = igs.ShowMarker;
                clrbtnMeterItemRotatorBeamWidth.Color = igs.LowColor;
                clrbtnMeterItemRotatorText.Color = igs.HighColor;
                chkMeterItemRotatorCardinals.Checked = igs.ShowHistory;
                chkMeterItemFadeOnRxRotator.Checked = igs.FadeOnRx;
                chkMeterItemFadeOnTxRotator.Checked = igs.FadeOnTx;
                chkMeterItemDarkModeRotator.Checked = igs.DarkMode;
                nudMeterItemRotatorBeamWidth.Value = (decimal)igs.AttackRatio;
                nudMeterItemRotator_padding.Value = (decimal)igs.EyeScale;
                updateShowBeamWidthControls();

                chkMeterItemRotatorAllowControl.Checked = igs.ShowType;
                clrbtnMeterItemRotatorControlColour.Color = igs.HistoryColor;
                txtMeterItemRotatorAZcommand.Text = igs.Text1;
                txtMeterItemRotatorELEcommand.Text = igs.Text2;
                txtMeterItemRotatorSTOPcommand.Text = igs.FontFamily1;
                updateRotatorControlControls();

                //
                switch ((MeterManager.clsRotatorItem.RotatorMode)igs.HistoryDuration)
                {
                    case MeterManager.clsRotatorItem.RotatorMode.AZ:
                        radMeterItemRotator_show_az.Checked = true;
                        break;
                    case MeterManager.clsRotatorItem.RotatorMode.ELE:
                        radMeterItemRotator_show_ele.Checked = true;
                        break;
                    case MeterManager.clsRotatorItem.RotatorMode.BOTH:
                        radMeterItemRotator_show_both.Checked = true;
                        break;
                    default:
                        break;
                }
                //

                Guid guid = igs.GetMMIOGuid(2);
                if (MultiMeterIO.Data.ContainsKey(guid))
                {
                    MultiMeterIO.clsMMIO mmio = MultiMeterIO.Data[guid];
                    txtRotator_4charID.Text = mmio.FourChar;
                }
                else
                {
                    txtRotator_4charID.Text = "";
                }

                nudMeterItemRotatorBeamWidth_alpha.Value = (decimal)igs.GetSetting<float>("rotator_beamwidth_alpha", true, 0, 1f, 0.6f);
            }
            else if (mt == MeterType.DATA_OUT)
            {
                Guid guid = igs.GetMMIOGuid(0);
                if (MultiMeterIO.Data.ContainsKey(guid))
                {
                    MultiMeterIO.clsMMIO mmio = MultiMeterIO.Data[guid];
                    txtDataOutNode_4charID.Text = mmio.FourChar;
                    nudDataOutNode_sendinterval.Value = (decimal)igs.UpdateInterval;
                }
                else
                {
                    txtDataOutNode_4charID.Text = "";
                    nudDataOutNode_sendinterval.Value = 500;
                }
            }
            else if (mt == MeterType.SIGNAL_TEXT)
            {
                nudMeterItemUpdateRate.Value = igs.UpdateInterval < nudMeterItemUpdateRate.Minimum ? nudMeterItemUpdateRate.Minimum : igs.UpdateInterval;
                nudMeterItemAttackRate.Value = (decimal)igs.AttackRatio;
                nudMeterItemDecayRate.Value = (decimal)igs.DecayRatio;

                chkMeterItemFadeOnRx.Checked = igs.FadeOnRx;
                chkMeterItemFadeOnTx.Checked = igs.FadeOnTx;
                clrbtnMeterItemHBackground.Color = igs.Colour;
                clrbtnMeterItemIndicator.Color = igs.MarkerColour;
                clrbtnMeterItemSubIndicator.Color = igs.SubMarkerColour;
                clrbtnMeterItemPeakValueColour.Color = igs.PeakValueColour;
                chkMeterItemPeakValue.Checked = igs.PeakValue;
                chkMeterItemSignalAverage.Checked = igs.Average;
                chkMeterItemShowSubIndicator.Checked = igs.ShowSubMarker;

                nudMeterItemHistoryDuration.Value = igs.HistoryDuration < nudMeterItemHistoryDuration.Minimum ? nudMeterItemHistoryDuration.Minimum : igs.HistoryDuration;
                nudMeterItemIgnoreHistoryDuration.Value = igs.IgnoreHistoryDuration;

                lblMMLow.Enabled = false;
                lblMMHigh.Enabled = false;
                clrbtnMeterItemLow.Enabled = false;
                clrbtnMeterItemHigh.Enabled = false;

                lblMMIndicator.Enabled = true;
                clrbtnMeterItemIndicator.Enabled = true;
                chkMeterItemShowIndicator.Enabled = false;

                lblMMIndicatorSub.Enabled = true;
                clrbtnMeterItemSubIndicator.Enabled = true;
                chkMeterItemShowSubIndicator.Enabled = true;

                lblMMBackground.Enabled = true;
                clrbtnMeterItemHBackground.Enabled = true;
                chkMeterItemFadeOnRx.Enabled = true;
                chkMeterItemFadeOnTx.Enabled = true;
                chkMeterItemSegmented.Enabled = false;
                chkMeterItemSolid.Enabled = false;
                lblMMsegSolLow.Enabled = false;
                lblMMsegSolHigh.Enabled = false;
                clrbtnMeterItemSegmentedSolidColourLow.Enabled = false;
                clrbtnMeterItemSegmentedSolidColourHigh.Enabled = false;
                chkMeterItemTitle.Enabled = false;
                clrbtnMeterItemMeterTitle.Enabled = false;
                chkMeterItemPeakValue.Enabled = true;
                updatePeakValueControls();
                lblMMEyeSize.Enabled = false;
                lblMMEyeBezelSize.Enabled = false;
                nudMeterItemEyeScale.Enabled = false;
                nudMeterItemEyeBezelScale.Enabled = false;
                chkMeterItemShadow.Enabled = false;
                lblMMHistory.Enabled = true;
                lblMMHistoryIgnore.Enabled = true;
                nudMeterItemHistoryDuration.Enabled = true;
                nudMeterItemIgnoreHistoryDuration.Enabled = true;
                chkMeterItemHistory.Enabled = false;
                clrbtnMeterItemHistory.Enabled = false;
                chkMeterItemPeakHold.Enabled = false;
                clrbtnMeterItemPeakHold.Enabled = false;
                chkMeterItemSignalAverage.Enabled = true;
                chkMeterItemDarkMode.Enabled = false;
                lblMMPowerLimit.Enabled = false;
                nudMeterItemsPowerLimit.Enabled = false;
                clrbtnMeterItemPowerScale.Enabled = false;
            }
            else if (mt == MeterType.VFO_DISPLAY)
            {
                clrbtnMMVfoDisplayBackground.Color = igs.Colour;
                clrbtnMMVfoDisplayTitle.Color = igs.TitleColor;

                //using exisinng igs settings
                clrbtnMMVfoDisplayFrequency.Color = igs.MarkerColour;
                clrbtnMMVfoDisplayMode.Color = igs.SubMarkerColour;
                clrbtnMMVfoDisplaySplitBack.Color = igs.LowColor;
                clrbtnMMVfoDisplaySplit.Color = igs.HighColor;
                clrbtnMMVfoDisplayRx.Color = igs.PeakValueColour;
                clrbtnMMVfoDisplayTx.Color = igs.PeakHoldMarkerColor;
                clrbtnMMVfoDisplayFilter.Color = igs.HistoryColor;
                clrbtnMMVfoDisplayBand.Color = igs.SegmentedSolidLowColour;
                clrbtnMMVfoDigitHighlight.Color = igs.PowerScaleColour;

                chkMultiMeter_vfo_show_bandtext.Checked = igs.GetSetting<bool>("vfo_showbandtext", false, false, false, false);
                clrbtnMultiMeter_vfo_show_bandtext.Color = igs.GetSetting<System.Drawing.Color>("vfo_showbandtext_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                clrbtnMMVfoDisplayFrequency_small.Color = igs.GetSetting<System.Drawing.Color>("vfo_frequency_small_numbers_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.Orange);

                clrbtnMultiMeter_vfo_lock.Color = igs.GetSetting<System.Drawing.Color>("vfo_lock_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);
                clrbtnMultiMeter_vfo_sync.Color = igs.GetSetting<System.Drawing.Color>("vfo_sync_colour", false, Color.Empty, Color.Empty, System.Drawing.Color.LimeGreen);


                switch ((MeterManager.clsVfoDisplay.VFODisplayMode)igs.HistoryDuration)
                {
                    case MeterManager.clsVfoDisplay.VFODisplayMode.VFO_BOTH:
                        radMultiMeter_vfo_display_both.Checked = true;
                        break;
                    case MeterManager.clsVfoDisplay.VFODisplayMode.VFO_A:
                        radMultiMeter_vfo_display_vfoa.Checked = true;
                        break;
                    case MeterManager.clsVfoDisplay.VFODisplayMode.VFO_B:
                        radMultiMeter_vfo_display_vfob.Checked = true;
                        break;
                }

                updateVfoShowBandtextColour();
            }
            else if (mt == MeterType.CLOCK)
            {
                clrbtnMMClockBackground.Color = igs.Colour;
                chkMMClockTitle.Checked = igs.ShowType;
                clrbtnMMClockTitle.Color = igs.TitleColor;
                clrbtnMMTime.Color = igs.MarkerColour;
                clrbtnMMDate.Color = igs.SubMarkerColour;
                radMM24Clock.Checked = igs.ShowMarker; // use the show marker bool for this
                if (!radMM24Clock.Checked && !radMM12Clock.Checked) radMM12Clock.Checked = true;
                updateTitleControlsClock();
            }
            else if (mt == MeterType.LED)
            {
                chkLedIndicator_FadeOnRX.Checked = igs.FadeOnRx;
                chkLedIndicator_FadeOnTX.Checked = igs.FadeOnTx;
                clrbtnLedIndicator_true.Color = igs.Colour;
                clrbtnLedIndicator_false.Color = igs.MarkerColour;

                clrbtnLedIndicator_PanelBackground.Color = igs.TitleColor;
                clrbtnLedIndicator_PanelBackgroundTX.Color = igs.HistoryColor;
                chkLedIndicator_ShowPanel.Checked = igs.ShowSubMarker;

                nudLedIndicator_xOffset.Value = (decimal)igs.EyeScale;
                nudLedIndicator_yOffset.Value = (decimal)igs.EyeBezelScale;
                nudLedIndicator_xSize.Value = (decimal)igs.AttackRatio;
                nudLedIndicator_ySize.Value = (decimal)igs.DecayRatio;

                txtLedIndicator_condition.Text = igs.Text1;

                nudLedIndicator_PanelPadding.Value = (decimal)igs.SpacerPadding;

                chkLed_show_true.Checked = igs.PeakHold;
                chkLed_show_false.Checked = igs.ShowMarker;
                switch(igs.IgnoreHistoryDuration)
                {
                    case 0:
                        radLed_light_on_off.Checked = true;
                        break;
                    case 1:
                        radLed_light_blink.Checked = true;
                        break;
                    case 2:
                        radLed_light_pulsate.Checked = true;
                        break;
                }

                updateLedIndicatorPanelControls();
                updateLedValidControls();
            }
            else if (mt == MeterType.TEXT_OVERLAY)
            {
                chkTextOverlay_FadeOnRX.Checked = igs.FadeOnRx;
                chkTextOverlay_FadeOnTX.Checked = igs.FadeOnTx;
                clrbtnTextOverlay_TextColour1.Color = igs.Colour;
                clrbtnTextOverlay_TextColour2.Color = igs.MarkerColour;
                clrbtnTextOverlay_TextBackColour1.Color = igs.SubMarkerColour;
                chkTextOverlay_textback1.Checked = igs.ShowMarker;
                clrbtnTextOverlay_TextBackColour2.Color = igs.PeakValueColour;
                chkTextOverlay_textback2.Checked = igs.ShowType;

                clrbtnTextOverlay_PanelBackground.Color = igs.TitleColor;
                clrbtnTextOverlay_PanelBackgroundTX.Color = igs.HistoryColor;
                chkTextOverlay_ShowPanel.Checked = igs.ShowSubMarker;

                nudTextOverlay_RXxOffset.Value = (decimal)igs.EyeScale;
                nudTextOverlay_RXyOffset.Value = (decimal)igs.EyeBezelScale;
                nudTextOverlay_TXxOffset.Value = (decimal)igs.AttackRatio;
                nudTextOverlay_TXyOffset.Value = (decimal)igs.DecayRatio;

                txtTextOverlay_RXText.Text = igs.Text1;
                txtTextOverlay_TXText.Text = igs.Text2;

                _textOverlayFont1 = new Font(igs.FontFamily1, igs.FontSize1, igs.FontStyle1);
                _textOverlayFont2 = new Font(igs.FontFamily2, igs.FontSize2, igs.FontStyle2);

                nudTextOverlay_PanelPadding.Value = (decimal)igs.SpacerPadding;

                updateTextOverlayPanelControls();
                updateTextOverlayBackTextControls();
            }
            else if (mt == MeterType.SPACER)
            {
                clrbtnMeterItemHBackgroundSpacerRX.Color = igs.Colour;
                clrbtnMeterItemHBackgroundSpacerTX.Color = igs.MarkerColour;
                chkMeterItemFadeOnRxSpacer.Checked = igs.FadeOnRx;
                chkMeterItemFadeOnTxSpacer.Checked = igs.FadeOnTx;
                nudMeterItemSpacerPadding.Value = (decimal)igs.SpacerPadding;
            }
            else
            {
                clrbtnMeterItemLow.Color = igs.LowColor;
                clrbtnMeterItemHigh.Color = igs.HighColor;
                clrbtnMeterItemIndicator.Color = igs.MarkerColour;
                clrbtnMeterItemSubIndicator.Color = igs.SubMarkerColour;
                chkMeterItemShowIndicator.Checked = igs.ShowMarker;
                chkMeterItemShowSubIndicator.Checked = igs.ShowSubMarker;
                clrbtnMeterItemHBackground.Color = igs.Colour;
                nudMeterItemUpdateRate.Value = igs.UpdateInterval < nudMeterItemUpdateRate.Minimum ? nudMeterItemUpdateRate.Minimum : igs.UpdateInterval;
                nudMeterItemAttackRate.Value = (decimal)igs.AttackRatio;
                nudMeterItemDecayRate.Value = (decimal)igs.DecayRatio;

                updateHistoryControls(igs.ShowHistory, igs.HistoryColor, igs.ShowHistory);
                nudMeterItemHistoryDuration.Value = igs.HistoryDuration < nudMeterItemHistoryDuration.Minimum ? nudMeterItemHistoryDuration.Minimum : igs.HistoryDuration;

                if (igs.BarStyle == MeterManager.clsBarItem.BarStyle.Segments)
                    chkMeterItemSegmented.Checked = true; // will cause solid to turn off
                else if (igs.BarStyle == MeterManager.clsBarItem.BarStyle.SolidFilled)
                    chkMeterItemSolid.Checked = true; // will cause segment to turn off
                else
                {
                    chkMeterItemSegmented.Checked = false;
                    chkMeterItemSolid.Checked = false;
                }

                clrbtnMeterItemSegmentedSolidColourLow.Color = igs.SegmentedSolidLowColour;
                clrbtnMeterItemSegmentedSolidColourHigh.Color = igs.SegmentedSolidHighColour;
                updateSegmentedSolidControls();

                updatePeakHoldControls(igs.PeakHold, igs.PeakHoldMarkerColor, igs.PeakHold);

                chkMeterItemShadow.Checked = igs.Shadow;
                chkMeterItemFadeOnRx.Checked = igs.FadeOnRx;
                chkMeterItemFadeOnTx.Checked = igs.FadeOnTx;
                chkMeterItemTitle.Checked = igs.ShowType;

                clrbtnMeterItemMeterTitle.Color = igs.TitleColor;
                updateTitleControls();

                chkMeterItemPeakValue.Checked = igs.PeakValue;
                clrbtnMeterItemPeakValueColour.Color = igs.PeakValueColour;
                updatePeakValueControls();

                if (mt == MeterType.CROSS || mt == MeterType.ANANMM || mt == MeterType.PWR || mt == MeterType.REVERSE_PWR) nudMeterItemsPowerLimit.Value = (decimal)igs.MaxPower;
                if (mt == MeterType.CROSS || mt == MeterType.ANANMM) clrbtnMeterItemPowerScale.Color = igs.PowerScaleColour;

                // specific to mt
                bool bMagicEye = mt == MeterType.MAGIC_EYE;
                if (bMagicEye)
                {
                    nudMeterItemEyeScale.Value = (decimal)igs.EyeScale; // prevents setting it to 0 as other items will have 0 //FIX THIS
                    nudMeterItemEyeBezelScale.Value = (decimal)igs.EyeBezelScale;
                }
                nudMeterItemEyeScale.Enabled = bMagicEye;
                nudMeterItemEyeBezelScale.Enabled = bMagicEye;
                lblMMEyeSize.Enabled = bMagicEye;
                lblMMEyeBezelSize.Enabled = bMagicEye;
                lblMMHistory.Enabled = !bMagicEye;
                nudMeterItemHistoryDuration.Enabled = !bMagicEye;
                lblMMHistoryIgnore.Enabled = !bMagicEye;
                nudMeterItemIgnoreHistoryDuration.Enabled = !bMagicEye;
                chkMeterItemHistory.Enabled = !bMagicEye;
                clrbtnMeterItemHistory.Enabled = !bMagicEye && chkMeterItemHistory.Checked;
                chkMeterItemPeakHold.Enabled = !(bMagicEye || mt == MeterType.CROSS);
                clrbtnMeterItemPeakHold.Enabled = !(bMagicEye || mt == MeterType.CROSS) && chkMeterItemPeakHold.Checked;

                chkMeterItemShadow.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;
                chkMeterItemDarkMode.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;

                lblMMPowerLimit.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;
                clrbtnMeterItemPowerScale.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS;

                nudMeterItemsPowerLimit.Enabled = mt == MeterType.ANANMM || mt == MeterType.CROSS || mt == MeterType.PWR || mt == MeterType.REVERSE_PWR;

                bool bEnable = mt == MeterType.ANANMM || mt == MeterType.CROSS || mt == MeterType.MAGIC_EYE;
                chkMeterItemSegmented.Enabled = !bEnable;
                chkMeterItemSolid.Enabled = !bEnable;

                clrbtnMeterItemSegmentedSolidColourLow.Enabled = !bEnable && (chkMeterItemSegmented.Checked || chkMeterItemSolid.Checked);
                clrbtnMeterItemSegmentedSolidColourHigh.Enabled = !bEnable && (chkMeterItemSegmented.Checked || chkMeterItemSolid.Checked);
                lblMMsegSolLow.Enabled = !bEnable && (chkMeterItemSegmented.Checked || chkMeterItemSolid.Checked);
                lblMMsegSolHigh.Enabled = !bEnable && (chkMeterItemSegmented.Checked || chkMeterItemSolid.Checked);

                chkMeterItemTitle.Enabled = !bEnable;
                clrbtnMeterItemMeterTitle.Enabled = !bEnable;
                chkMeterItemPeakValue.Enabled = !bEnable;
                clrbtnMeterItemPeakValueColour.Enabled = !bEnable;
                lblMMLow.Enabled = !bEnable;
                lblMMHigh.Enabled = !bEnable;
                lblMMBackground.Enabled = !bEnable;
                clrbtnMeterItemLow.Enabled = !bEnable;
                clrbtnMeterItemHigh.Enabled = !bEnable;
                clrbtnMeterItemHBackground.Enabled = !bEnable;
                chkMeterItemShowIndicator.Enabled = !bEnable;
                //
                lblMMIndicatorSub.Enabled = igs.SubIndicators;
                clrbtnMeterItemSubIndicator.Enabled = igs.SubIndicators;
                chkMeterItemShowSubIndicator.Enabled = !bEnable && igs.SubIndicators;
                //

                chkMeterItemSignalAverage.Enabled = mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE;
                if (mt == MeterType.ANANMM || mt == MeterType.MAGIC_EYE) chkMeterItemSignalAverage.Checked = igs.Average;
                if (mt == MeterType.ANANMM || mt == MeterType.CROSS) chkMeterItemDarkMode.Checked = igs.DarkMode;
                //
            }
            
            setupMMSettingsGroupBoxes(mt);

            _ignoreMeterItemChangeEvents = false;
        }
        private void updateHistoryControls(bool showHistory, Color c, bool updateColor = false)
        {
            chkMeterItemHistory.Checked = showHistory;
            clrbtnMeterItemHistory.Enabled = showHistory;
            tbMeterItemHistoryAlpha.Enabled = showHistory;

            if (updateColor)
            {
                tbMeterItemHistoryAlpha.Value = c.A;
                clrbtnMeterItemHistory.Color = Color.FromArgb(255, c);
            }
        }
        private void updateSegmentedSolidControls()
        {
            bool bEnabled = chkMeterItemSegmented.Checked || chkMeterItemSolid.Checked;
            clrbtnMeterItemSegmentedSolidColourLow.Enabled = bEnabled;
            clrbtnMeterItemSegmentedSolidColourHigh.Enabled = bEnabled;
            lblMMsegSolLow.Enabled = bEnabled;
            lblMMsegSolHigh.Enabled = bEnabled;
        }
        private void updateTitleControls()
        {
            clrbtnMeterItemMeterTitle.Enabled = chkMeterItemTitle.Checked;
        }
        private void updateTitleControlsClock()
        {
            clrbtnMMClockTitle.Enabled = chkMMClockTitle.Checked;
        }
        private void updatePeakValueControls()
        {
            clrbtnMeterItemPeakValueColour.Enabled = chkMeterItemPeakValue.Checked;
        }
        private void updatePeakHoldControls(bool showPeakHold, Color c, bool updateColor = false)
        {
            chkMeterItemPeakHold.Checked = showPeakHold;
            clrbtnMeterItemPeakHold.Enabled = showPeakHold;

            if (updateColor)
            {
                clrbtnMeterItemPeakHold.Color = Color.FromArgb(255, c);
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
            if (chkMeterItemSegmented.Checked) chkMeterItemSolid.Checked = false; // can only be one

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
            bool bEnabled = chkMeterItemPeakHold.Checked;

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

        private void chkMeterItemSignalAverage_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemDarkMode_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMaintainNFAdjustDeltaRX2_CheckedChanged(object sender, EventArgs e)
        {
            console.MaintainNFAdjustDeltaRX2 = chkMaintainNFAdjustDeltaRX2.Checked;
        }

        private void chkMaintainNFAdjustDeltaRX1_CheckedChanged(object sender, EventArgs e)
        {
            console.MaintainNFAdjustDeltaRX1 = chkMaintainNFAdjustDeltaRX1.Checked;
        }

        private void chkContainerBorder_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ContainerBorder(cci.ID, chkContainerBorder.Checked);
            }
        }

        private void clrbtnContainerBackground_Changed(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ContainerBackgroundColour(cci.ID, clrbtnContainerBackground.Color);
            }
        }

        private void nudMeterItemsPowerLimit_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemSolid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMeterItemSolid.Checked) chkMeterItemSegmented.Checked = false; // can only be one

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
        private void setupMMSettingsGroupBoxes(MeterType mt)
        {
            // grpMeterItemSettings defines the x,y used by all
            Point loc = grpMeterItemSettings.Location;

            grpMeterItemSettings.Visible = false;
            grpMeterItemClockSettings.Visible = false;
            grpMeterItemVfoDisplaySettings.Visible = false;
            grpMeterItemSpacerSettings.Visible = false;
            grpTextOverlay.Visible = false;
            grpMeterItemDataOutNode.Visible = false;
            grpMeterItemRotator.Visible = false;
            grpLedIndicator.Visible = false;
            grpWebImage.Visible = false;
            grpBandButtons.Visible = false;
            pnlButtonBox_antenna_toggles.Visible = false;
            grpHistoryItem.Visible = false;

            switch (mt)
            {
                case MeterType.NONE:
                    break;
                case MeterType.VFO_DISPLAY:
                    grpMeterItemVfoDisplaySettings.Parent = grpMultiMeterHolder;
                    grpMeterItemVfoDisplaySettings.Location = loc;
                    grpMeterItemVfoDisplaySettings.Visible = true;
                    break;
                case MeterType.CLOCK:
                    grpMeterItemClockSettings.Parent = grpMultiMeterHolder;
                    grpMeterItemClockSettings.Location = loc;
                    grpMeterItemClockSettings.Visible = true;
                    break;
                case MeterType.SPACER:
                    grpMeterItemSpacerSettings.Parent = grpMultiMeterHolder;
                    grpMeterItemSpacerSettings.Location = loc;
                    grpMeterItemSpacerSettings.Visible = true;
                    break;
                case MeterType.TEXT_OVERLAY:
                    grpTextOverlay.Parent = grpMultiMeterHolder;
                    grpTextOverlay.Location = loc;
                    grpTextOverlay.Visible = true;
                    break;
                case MeterType.DATA_OUT:
                    grpMeterItemDataOutNode.Parent = grpMultiMeterHolder;
                    grpMeterItemDataOutNode.Location = loc;
                    grpMeterItemDataOutNode.Visible = true;
                    break;
                case MeterType.ROTATOR:
                    grpMeterItemRotator.Parent = grpMultiMeterHolder;
                    grpMeterItemRotator.Location = loc;
                    grpMeterItemRotator.Visible = true;
                    break;
                case MeterType.LED:
                    grpLedIndicator.Parent = grpMultiMeterHolder;
                    grpLedIndicator.Location = loc;
                    grpLedIndicator.Visible = true;
                    break;
                case MeterType.WEB_IMAGE:
                    grpWebImage.Parent = grpMultiMeterHolder;
                    grpWebImage.Location = loc;
                    grpWebImage.Visible = true;
                    comboWebImage_HamQsl.SelectedIndex = 0;
                    comboWebImage_BsdWorld.SelectedIndex = 0;
                    comboWebImage_nasa.SelectedIndex = 0;
                    comboWebImage_noaa.SelectedIndex = 0;
                    break;
                case MeterType.TUNESTEP_BUTTONS:
                case MeterType.ANTENNA_BUTTONS:
                case MeterType.FILTER_BUTTONS:
                case MeterType.MODE_BUTTONS:
                case MeterType.BAND_BUTTONS:
                    {
                        grpBandButtons.Parent = grpMultiMeterHolder;
                        grpBandButtons.Location = loc;
                        grpBandButtons.Visible = true;

                        switch (mt)
                        {
                            case MeterType.ANTENNA_BUTTONS:
                                pnlButtonBox_antenna_toggles.Parent = grpBandButtons;
                                pnlButtonBox_antenna_toggles.Location = new Point(166, 194);
                                pnlButtonBox_antenna_toggles.Visible = true;
                                ucTunestepOptionsGrid_buttons.Visible = false;
                                break;
                            case MeterType.TUNESTEP_BUTTONS:
                                ucTunestepOptionsGrid_buttons.Parent = grpBandButtons;
                                ucTunestepOptionsGrid_buttons.Location = new Point(166, 194);
                                ucTunestepOptionsGrid_buttons.Visible = true;
                                pnlButtonBox_antenna_toggles.Visible = false;
                                if(console != null)
                                {
                                    ucTunestepOptionsGrid_buttons.Init(console.TuneStepList);
                                }
                                break;
                            default:
                                pnlButtonBox_antenna_toggles.Visible = false;
                                ucTunestepOptionsGrid_buttons.Visible = false;
                                break;
                        }
                    }
                    break;
                case MeterType.HISTORY:
                    grpHistoryItem.Parent = grpMultiMeterHolder;
                    grpHistoryItem.Location = loc;
                    grpHistoryItem.Visible = true;
                    break;
                default:
                    grpMeterItemSettings.Parent = grpMultiMeterHolder;
                    grpMeterItemSettings.Visible = true;
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
            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
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

            clsMeterTypeComboboxItem mtci = lstMetersInUse.SelectedItem as clsMeterTypeComboboxItem;
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

                if(mt == MeterType.TUNESTEP_BUTTONS)
                {
                    _itemGroupSettings.SetSetting<int>("buttonbox_tunestep_bitfield", currentSettings.GetSetting<int>("buttonbox_tunestep_bitfield", true, 0, int.MaxValue, 0));
                }
                else if (mt == MeterType.ANTENNA_BUTTONS)
                {
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

                m.ApplySettingsForMeterGroup(mt, _itemGroupSettings, mtci.Order);
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

            // only allow paste into matching
            if (mt == MeterType.MAGIC_EYE || mt == MeterType.CROSS ||
                mt == MeterType.ANANMM || mt == MeterType.SIGNAL_TEXT ||
                mt == MeterType.SPACER || mt == MeterType.TEXT_OVERLAY ||
                mt == MeterType.LED || mt == MeterType.ROTATOR || mt == MeterType.HISTORY ||
                mt == MeterType.VFO_DISPLAY || mt == MeterType.CLOCK
                )
            {
                bPaste = _itemGroupSettingsMeterType == mt;
            }
            else if (_itemGroupSettingsMeterType == MeterType.MAGIC_EYE || _itemGroupSettingsMeterType == MeterType.CROSS ||
                _itemGroupSettingsMeterType == MeterType.ANANMM || _itemGroupSettingsMeterType == MeterType.SIGNAL_TEXT ||
                _itemGroupSettingsMeterType == MeterType.SPACER || _itemGroupSettingsMeterType == MeterType.TEXT_OVERLAY ||
                _itemGroupSettingsMeterType == MeterType.LED || mt == MeterType.ROTATOR || mt == MeterType.HISTORY ||
                _itemGroupSettingsMeterType == MeterType.VFO_DISPLAY || _itemGroupSettingsMeterType == MeterType.CLOCK
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
            if (!String.IsNullOrEmpty(sId))
            {
                for (int i = 0; i < comboContainerSelect.Items.Count; i++)
                {
                    clsContainerComboboxItem cci = comboContainerSelect.Items[i] as clsContainerComboboxItem;
                    if (cci != null && cci.ID == sId) { comboContainerSelect.SelectedIndex = i; break; }
                }
            }
            if (!Visible) Show(console);
            WindowState = FormWindowState.Normal;
            BringToFront();
            Activate();
        }
        private void chkLockContainer_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.LockContainer(cci.ID, chkLockContainer.Checked);
                btnContainerDelete.Enabled = !chkLockContainer.Checked;
                btnAddMeterItem.Enabled = !chkLockContainer.Checked;
                btnRemoveMeterItem.Enabled = !chkLockContainer.Checked;
                btnMeterUp.Enabled = !chkLockContainer.Checked;
                btnMeterDown.Enabled = !chkLockContainer.Checked;
            }
        }

        private void chkMultiMeter_auto_container_height_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.AutoContainerHeight(cci.ID, chkMultiMeter_auto_container_height.Checked);
            }
        }

        private void chkContainerMinimises_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.ContainerMinimises(cci.ID, chkContainerMinimises.Checked);
            }
        }

        private void chkContainerNoTitle_CheckedChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            clsContainerComboboxItem cci = (clsContainerComboboxItem)comboContainerSelect.SelectedItem;
            if (cci != null)
            {
                MeterManager.NoTitle(cci.ID, chkContainerNoTitle.Checked);
            }
        }

        private void btnMMIO_variable_2_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(1);
        }

        private void btnMMIO_variable_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(0);
        }

        private void clrbtnMeterItemSubIndicator_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemIndicator_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMultiMeter_vfo_sync_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMultiMeter_vfo_lock_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnVFOCopyColourFromMainNumbers_Click(object sender, EventArgs e)
        {
            clrbtnMMVfoDisplayFrequency_small.Color = clrbtnMMVfoDisplayFrequency.Color;
        }

        private void clrbtnMMVfoDisplayFrequency_small_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMultiMeter_vfo_show_bandtext_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMultiMeter_vfo_show_bandtext_CheckedChanged(object sender, EventArgs e)
        {
            updateVfoShowBandtextColour();
            updateMeterType();
        }

        private void radMultiMeter_vfo_display_vfob_CheckedChanged(object sender, EventArgs e)
        {
            if (!radMultiMeter_vfo_display_vfob.Checked) return;
            updateMeterType();
        }

        private void radMultiMeter_vfo_display_vfoa_CheckedChanged(object sender, EventArgs e)
        {
            if (!radMultiMeter_vfo_display_vfoa.Checked) return;
            updateMeterType();
        }

        private void radMultiMeter_vfo_display_both_CheckedChanged(object sender, EventArgs e)
        {
            if(!radMultiMeter_vfo_display_both.Checked) return;
            updateMeterType();
        }

        private void clrbtnMMVfoDigitHighlight_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemHBackgroundSpacerTX_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemSpacerPadding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemHBackgroundSpacerRX_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemFadeOnTxSpacer_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemFadeOnRxSpacer_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnTextOverlay_copyfonts_Click(object sender, EventArgs e)
        {
            _textOverlayFont2 = new Font(_textOverlayFont1.FontFamily, _textOverlayFont1.Size, _textOverlayFont1.Style);
            clrbtnTextOverlay_TextColour2.Color = clrbtnTextOverlay_TextColour1.Color;
            clrbtnTextOverlay_TextBackColour2.Color = clrbtnTextOverlay_TextBackColour1.Color;
            chkTextOverlay_textback2.Checked = chkTextOverlay_textback1.Checked;

            updateMeterType();
        }

        private void pbTextOverlay_variables_Click(object sender, EventArgs e)
        {
            toolTip1.Show(toolTip1.GetToolTip(pbTextOverlay_variables), pbTextOverlay_variables, 10 * 1000);
        }

        private void clrbtnTextOverlay_PanelBackgroundTX_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkTextOverlay_textback2_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateTextOverlayBackTextControls();
        }

        private void chkTextOverlay_textback1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateTextOverlayBackTextControls();
        }

        private void clrbtnTextOverlay_TextBackColour2_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnTextOverlay_TextBackColour1_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnTextOverlay_copyoffsets_Click(object sender, EventArgs e)
        {
            nudTextOverlay_TXxOffset.Value = nudTextOverlay_RXxOffset.Value;
            nudTextOverlay_TXyOffset.Value = nudTextOverlay_RXyOffset.Value;
        }

        private void nudTextOverlay_TXyOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudTextOverlay_TXxOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudTextOverlay_RXyOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudTextOverlay_RXxOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnTextOverlay_TextColour2_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnTextOverlay_Font2_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = _textOverlayFont2;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    _textOverlayFont2 = fontDialog.Font;
                    updateMeterType();
                }
            }
        }

        private void txtTextOverlay_TXText_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnTextOverlay_Font1_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = _textOverlayFont1;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    _textOverlayFont1 = fontDialog.Font;
                    updateMeterType();
                }
            }
        }

        private void txtTextOverlay_RXText_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkTextOverlay_ShowPanel_CheckedChanged(object sender, EventArgs e)
        {
            updateTextOverlayPanelControls();
            updateMeterType();
        }

        private void clrbtnTextOverlay_TextColour1_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudTextOverlay_PanelPadding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnTextOverlay_PanelBackground_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkTextOverlay_FadeOnTX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkTextOverlay_FadeOnRX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void txtDataOutNode_4charID_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudDataOutNode_sendinterval_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemRotatorBeamWidth_alpha_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void txtMeterItemRotatorSTOPcommand_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemRotator_padding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void radMeterItemRotator_show_both_CheckedChanged(object sender, EventArgs e)
        {
            if (radMeterItemRotator_show_both.Checked)
            {
                updateMeterType();
                nudMeterItemRotator_padding.Enabled = false;
            }
        }

        private void radMeterItemRotator_show_ele_CheckedChanged(object sender, EventArgs e)
        {
            if (radMeterItemRotator_show_ele.Checked)
            {
                updateMeterType();
                nudMeterItemRotator_padding.Enabled = true;
            }
        }

        private void radMeterItemRotator_show_az_CheckedChanged(object sender, EventArgs e)
        {
            // only do the checked state for rad controls, as all the others in the group will fire as well
            if (radMeterItemRotator_show_az.Checked)
            {
                updateMeterType();
                nudMeterItemRotator_padding.Enabled = true;
            }
        }

        private void txtRotator_4charID_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void bntMultiMeterItemRotator_default_pstRotator_Click(object sender, EventArgs e)
        {
            txtMeterItemRotatorAZcommand.Text = "<PST><AZIMUTH>%AZ%</AZIMUTH></PST>";
            txtMeterItemRotatorELEcommand.Text = "<PST><ELEVATION>%ELE%</ELEVATION></PST>";
            txtMeterItemRotatorSTOPcommand.Text = "<PST><STOP>1</STOP></PST>";
        }

        private void txtMeterItemRotatorELEcommand_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void txtMeterItemRotatorAZcommand_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemRotatorControlColour_Changed(object sender, EventArgs e)
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

        private void clrbtnMeterItemRotatorText_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemRotatorBeamWidth_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnMMIO_variable_2_rotator_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(1);
        }

        private void btnMMIO_variable_rotator_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(0);
        }

        private void chkMeterItemRotatorShowBeamWidth_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
            updateShowBeamWidthControls();
        }

        private void clrbtnMeterItemRotatorBeamWidth_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemDarkModeRotator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudMeterItemUpdateRateRotator_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemHBackgroundRotator_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemRotatorSmallDot_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemRotatorLargeDot_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnMeterItemRotatorArrow_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemFadeOnTxRotator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkMeterItemFadeOnRxRotator_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void radLed_light_pulsate_CheckedChanged(object sender, EventArgs e)
        {
            if(radLed_light_pulsate.Checked)
                updateMeterType();
        }

        private void radLed_light_blink_CheckedChanged(object sender, EventArgs e)
        {
            if(radLed_light_blink.Checked)
                updateMeterType();
        }

        private void radLed_light_on_off_CheckedChanged(object sender, EventArgs e)
        {
            if(radLed_light_on_off.Checked)
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

        private void btnLedIndicator_copy_truefalse_colours_Click(object sender, EventArgs e)
        {
            clrbtnLedIndicator_false.Color = clrbtnLedIndicator_true.Color;
        }

        private void clrbtnLedIndicator_PanelBackgroundTX_Changed(object sender, EventArgs e)
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

        private void btnLedIndicator_copy_sizex_to_y_Click(object sender, EventArgs e)
        {
            nudLedIndicator_ySize.Value = nudLedIndicator_xSize.Value;
        }

        private void nudLedIndicator_ySize_ValueChanged(object sender, EventArgs e)
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

        private void nudLedIndicator_xOffset_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void txtLedIndicator_condition_TextChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkLedIndicator_ShowPanel_CheckedChanged(object sender, EventArgs e)
        {
            updateLedIndicatorPanelControls();
            updateMeterType();
        }

        private void nudLedIndicator_PanelPadding_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnLedIndicator_PanelBackground_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkLedIndicator_FadeOnTX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkLedIndicator_FadeOnRX_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkWebImage_bypass_cache_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void comboWebImage_noaa_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (comboWebImage_noaa.SelectedIndex == -1) return;
            if (comboWebImage_noaa.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _noaa_urls[comboWebImage_noaa.SelectedIndex];
            txtWebImage_url.Text = kvp.Value;

            comboWebImage_noaa.SelectedIndex = 0;
        }

        private void btnWebImage_bsdworld_visit_Click(object sender, EventArgs e)
        {
            Common.OpenUri("https://bsdworld.org/help.html");
        }

        private void comboWebImage_BsdWorld_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (comboWebImage_BsdWorld.SelectedIndex == -1) return;
            if (comboWebImage_BsdWorld.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _bsdworld_urls[comboWebImage_BsdWorld.SelectedIndex];
            txtWebImage_url.Text = kvp.Value;

            comboWebImage_BsdWorld.SelectedIndex = 0;
        }

        private void comboWebImage_nasa_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (comboWebImage_nasa.SelectedIndex == -1) return;
            if (comboWebImage_nasa.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _nasa_urls[comboWebImage_nasa.SelectedIndex];
            txtWebImage_url.Text = kvp.Value;

            comboWebImage_nasa.SelectedIndex = 0;
        }

        private void btnWebImage_hamqsl_donate_Click(object sender, EventArgs e)
        {
            Common.OpenUri("https://www.hamqsl.com/donate.html");
        }

        private void comboWebImage_HamQsl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;
            if (comboWebImage_HamQsl.SelectedIndex == -1) return;
            if (comboWebImage_HamQsl.SelectedIndex == 0) return;

            KeyValuePair<string, string> kvp = _hamqsl_urls[comboWebImage_HamQsl.SelectedIndex];
            txtWebImage_url.Text = kvp.Value;

            comboWebImage_HamQsl.SelectedIndex = 0;
        }

        private void txtWebImage_url_TextChanged(object sender, EventArgs e)
        {
            if (txtWebImage_url.Text.Contains("hamqsl.com", StringComparison.InvariantCultureIgnoreCase) ||
                txtWebImage_url.Text.Contains("bsdworld.org", StringComparison.InvariantCultureIgnoreCase) ||
                //txtWebImage_url.Text.Contains("nascom.nasa.gov", StringComparison.InvariantCultureIgnoreCase) ||
                //txtWebImage_url.Text.Contains("swpc.noaa.gov", StringComparison.InvariantCultureIgnoreCase) ||
                txtWebImage_url.Text.Contains("kc2g.com", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                // lock and set the update interval
                nudWebImage_update_interval.Enabled = false;
                _ignoreMeterItemChangeEvents = true;
                nudWebImage_update_interval.Value = (decimal)600;
                _ignoreMeterItemChangeEvents = false;

                // lock and set the bypass cache
                chkWebImage_bypass_cache.Enabled = false;
                _ignoreMeterItemChangeEvents = true;
                chkWebImage_bypass_cache.Checked = false;
                _ignoreMeterItemChangeEvents = false;
            }
            else
            {
                if(!nudWebImage_update_interval.Enabled)
                    nudWebImage_update_interval.Enabled = true;
                if(!chkWebImage_bypass_cache.Enabled)
                    chkWebImage_bypass_cache.Enabled = true;
            }

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

        private void chkWebImage_fade_tx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkWebImage_fade_rx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_rxtxant_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_xvtr_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_ext1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_byp_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_tx3_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_tx2_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_tx1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_rx3_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_rx2_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkButtonBox_antenna_rx1_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudButtonBox_font_y_shift_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudButtonBox_font_x_shift_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudButtonBox_font_scale_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudBandButtons_indicator_style_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkBandButtons_band_inactive_use_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnBandButtons_hover_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnBandButtons_fill_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnBandButtons_border_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnBandButtons_indicator_off_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudBandButtons_indicator_border_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudBandButtons_height_ratio_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudBandButtons_radius_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudBandButtons_margin_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudBandButtons_border_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnBandButtons_font_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = _bandButtons_font;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    _bandButtons_font = fontDialog.Font;
                    updateMeterType();
                }
            }
        }

        private void chkBandButtons_use_indicator_CheckedChanged(object sender, EventArgs e)
        {
            updateButtonIndicatorControls();
            updateMeterType();
        }

        private void nudBandButtons_columns_ValueChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnBandButtons_indicator_on_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkBandButtons_fade_tx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkBandButtons_fade_rx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnHistory_time_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnHistory_lines_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void clrbtnHistory_colour_1_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void btnHistory_copy_minmax_from_0_Click(object sender, EventArgs e)
        {
            nudHistory_axis1_min.Value = nudHistory_axis0_min.Value;
            nudHistory_axis1_max.Value = nudHistory_axis0_max.Value;
        }

        private void chkHistory_1_show_axis_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudHistory_axis1_max_ValueChanged(object sender, EventArgs e)
        {
            if (nudHistory_axis1_max.Value < nudHistory_axis1_min.Value) nudHistory_axis1_min.Value = nudHistory_axis1_max.Value;
            updateMeterType();
        }

        private void nudHistory_axis1_min_ValueChanged(object sender, EventArgs e)
        {
            if (nudHistory_axis1_min.Value > nudHistory_axis1_max.Value) nudHistory_axis1_max.Value = nudHistory_axis1_min.Value;
            updateMeterType();
        }

        private void chkHistory_auto_1_scale_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void comboHistory_reading_1_SelectedIndexChanged(object sender, EventArgs e)
        {
            clsComboHistoryItem chi = comboHistory_reading_1.SelectedItem as clsComboHistoryItem;
            if (chi == null) return;

            updateMeterType();
        }

        private void clrbtnHistory_colour_0_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void nudHistory_axis0_max_ValueChanged(object sender, EventArgs e)
        {
            if (nudHistory_axis0_max.Value < nudHistory_axis0_min.Value) nudHistory_axis0_min.Value = nudHistory_axis0_max.Value;
            updateMeterType();
        }

        private void nudHistory_axis0_min_ValueChanged(object sender, EventArgs e)
        {
            if (nudHistory_axis0_min.Value > nudHistory_axis0_max.Value) nudHistory_axis0_max.Value = nudHistory_axis0_min.Value;
            updateMeterType();
        }

        private void chkHistory_auto_0_scale_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void comboHistory_reading_0_SelectedIndexChanged(object sender, EventArgs e)
        {
            clsComboHistoryItem chi = comboHistory_reading_0.SelectedItem as clsComboHistoryItem;
            if (chi == null) return;

            updateMeterType();
        }

        private void btnMMIO_variable_2_history_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(1);
        }

        private void btnMMIO_variable_history_Click(object sender, EventArgs e)
        {
            mmioSetupVariable(0);
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

        private void clrbtnHistory_background_Changed(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkHistory_fade_tx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }

        private void chkHistory_fade_rx_CheckedChanged(object sender, EventArgs e)
        {
            updateMeterType();
        }


    }

    sealed unsafe public partial class Console
    {
        private P27ThetisMetersConfigForm p27MetersConfigForm;

        internal void P27ShowMetersConfig()
        {
            if (p27MetersConfigForm == null || p27MetersConfigForm.IsDisposed)
                p27MetersConfigForm = new P27ThetisMetersConfigForm(this);

            if (!p27MetersConfigForm.Visible) p27MetersConfigForm.Show(this);
            else { p27MetersConfigForm.WindowState = FormWindowState.Normal; p27MetersConfigForm.BringToFront(); p27MetersConfigForm.Activate(); }
        }

        internal void P27SaveMetersConfiguration() { P25SaveThetisMeters(); }

        internal void P27CloseMetersConfig()
        {
            P27ThetisMetersConfigForm form = p27MetersConfigForm;
            p27MetersConfigForm = null;
            if (form != null && !form.IsDisposed) form.CloseForShutdown();
        }
    }
}
