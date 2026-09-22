using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PowerSDR
{
    public partial class Setup
    {
        // P24 compatibility state copied from the Thetis Setup meter page.
        // These members live in Setup.cs in Thetis, outside the extracted meter block.
        private Font _textOverlayFont1 = null;
        private Font _textOverlayFont2 = null;
        private Font _bandButtons_font = null;
        private int _selected_voice_slot = 0;
        private bool _ignore_slot_count = false;
        private bool _suppressEvents = false;
        private bool _reset_button_map_layout = false;
        private bool _reset_waverecord_order_map = false;
        private bool _setting_globalkeybind = false;
        private Keys _globalPlayRecordInterrupKeybind = Keys.None;
        private bool _listening_for_recording_keycodes = false;
        private readonly System.Windows.Forms.Timer _recording_keybind_timer = new System.Windows.Forms.Timer();
        private bool _alt_pressed = false;
        private bool _shift_pressed = false;
        private bool _ctrl_pressed = false;

        private KeyValuePair<string,string>[] _bsdworld_urls = new KeyValuePair<string,string>[]
        {
            new KeyValuePair<string,string>("select one", "")
        };
        private KeyValuePair<string,string>[] _hamqsl_urls = new KeyValuePair<string,string>[]
        {
            new KeyValuePair<string,string>("select one", "")
        };
        private KeyValuePair<string,string>[] _nasa_urls = new KeyValuePair<string,string>[]
        {
            new KeyValuePair<string,string>("select one", "")
        };
        private KeyValuePair<string,string>[] _noaa_urls = new KeyValuePair<string,string>[]
        {
            new KeyValuePair<string,string>("select one", "")
        };

        private class clsComboHistoryItem
        {
            private readonly string _reading_name;
            private readonly Reading _reading;
            public clsComboHistoryItem(Reading r)
            {
                _reading = r;
                _reading_name = MeterManager.ReadingName(r);
            }
            public Reading Reading { get { return _reading; } }
            public string ReadingName { get { return _reading_name; } }
            public override string ToString() { return _reading_name; }
        }

        private void updateLedValidControls()
        {
            // The validation state is presentation-only. The native MeterManager
            // remains authoritative for LED item configuration and rendering.
        }

        private int getTotalColumnsNeededForAntennaButtons()
        {
            int n = 0;
            if (p24_chkButtonBox_antenna_rx1.Checked) n++;
            if (p24_chkButtonBox_antenna_rx2.Checked) n++;
            if (p24_chkButtonBox_antenna_rx3.Checked) n++;
            if (p24_chkButtonBox_antenna_tx1.Checked) n++;
            if (p24_chkButtonBox_antenna_tx2.Checked) n++;
            if (p24_chkButtonBox_antenna_tx3.Checked) n++;
            if (p24_chkButtonBox_antenna_byp.Checked) n++;
            if (p24_chkButtonBox_antenna_ext1.Checked) n++;
            if (p24_chkButtonBox_antenna_xvtr.Checked) n++;
            if (p24_chkButtonBox_antenna_rxtxant.Checked) n++;
            return Math.Max(1, n);
        }

        private bool preventIfContainerContainsLockedRecordings()
        {
            // Native PowerSDR does not have the Thetis ARP recording store.
            // There are no Thetis-owned recordings to lose on FLEX-5000.
            return false;
        }

        private bool preventIfItemContainsLockedRecordings()
        {
            return false;
        }

        private void updateWebImageState(ImageFetcher.State state, bool checkSelected = false, string id = "")
        {
            if (checkSelected)
            {
                string mgID = meterItemGroupIDfromSelected();
                if (mgID == "" || mgID != id) return;
                MeterType mt = meterItemGroupTypefromSelected();
                if (mt != MeterType.WEB_IMAGE) return;
            }

            string txt;
            switch (state)
            {
                case ImageFetcher.State.IDLE: txt = "idle"; break;
                case ImageFetcher.State.OK: txt = "ok"; break;
                case ImageFetcher.State.ERROR_URL_ISSUE: txt = "url issue"; break;
                case ImageFetcher.State.ERROR_IMAGE_CONVERSION_PROBLEM: txt = "bad image"; break;
                case ImageFetcher.State.ERROR_NO_SUITABLE_IMAGE: txt = "no image"; break;
                case ImageFetcher.State.WAITING: txt = "waiting"; break;
                case ImageFetcher.State.GATHERING_IMAGES: txt = "gathering"; break;
                default: txt = ""; break;
            }
            if (p24_lblWebImage_state != null) p24_lblWebImage_state.Text = txt;
        }
    }
}
