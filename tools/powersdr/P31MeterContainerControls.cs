using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PowerSDR
{
    // P31 container controls are isolated from the P30 ucMeter source.
    public partial class ucMeter
    {
        private bool p31Locked;
        private bool p31NoControls;
        private bool p31HooksInstalled;

        public bool Locked
        {
            get { return p31Locked; }
            set
            {
                P31EnsureHooks();
                p31Locked = value;
                if (p31Locked)
                {
                    _dragging = false;
                    _resizing = false;
                    pbGrab.Hide();
                }
            }
        }

        public bool NoControls
        {
            get { return p31NoControls; }
            set
            {
                P31EnsureHooks();
                p31NoControls = value;
                if (p31NoControls)
                {
                    pnlBar.Hide();
                    pbGrab.Hide();
                }
            }
        }

        private void P31EnsureHooks()
        {
            if (p31HooksInstalled) return;
            p31HooksInstalled = true;

            pnlBar.MouseDown += P31LockedMouseDown;
            lblRX.MouseDown += P31LockedMouseDown;
            pbGrab.MouseDown += P31LockedMouseDown;
            picContainer.MouseMove += P31NoControlsMouseMove;
            picContainer.MouseLeave += P31NoControlsMouseLeave;
        }

        private void P31LockedMouseDown(object sender, MouseEventArgs e)
        {
            if (!p31Locked) return;
            _dragging = false;
            _resizing = false;
        }

        private void P31NoControlsMouseMove(object sender, MouseEventArgs e)
        {
            if (!p31NoControls) return;

            if ((ModifierKeys & Keys.Shift) != Keys.Shift)
            {
                pnlBar.Hide();
                pbGrab.Hide();
            }
        }

        private void P31NoControlsMouseLeave(object sender, EventArgs e)
        {
            if (!p31NoControls) return;
            pnlBar.Hide();
            pbGrab.Hide();
        }
    }

    sealed unsafe public partial class Console
    {
        private void P31AppendContainerOptions(ArrayList a)
        {
            if (a == null) return;

            foreach (KeyValuePair<string, ucMeter> kvp in MeterManager.MeterContainers)
            {
                if (kvp.Value == null) continue;
                a.Add("P31Locked_" + kvp.Key + "/" + kvp.Value.Locked.ToString());
                a.Add("P31NoControls_" + kvp.Key + "/" + kvp.Value.NoControls.ToString());
            }
        }

        private void P31RestoreContainerOptions(ArrayList stored)
        {
            if (stored == null) return;

            foreach (object o in stored)
            {
                string s = o as string;
                if (String.IsNullOrEmpty(s)) continue;

                int slash = s.IndexOf('/');
                if (slash <= 0 || slash >= s.Length - 1) continue;

                string key = s.Substring(0, slash);
                string value = s.Substring(slash + 1);
                bool b;
                if (!Boolean.TryParse(value, out b)) continue;

                const string lockedPrefix = "P31Locked_";
                const string noControlsPrefix = "P31NoControls_";

                if (key.StartsWith(lockedPrefix, StringComparison.Ordinal))
                {
                    string id = key.Substring(lockedPrefix.Length);
                    if (MeterManager.MeterContainers.ContainsKey(id))
                        MeterManager.MeterContainers[id].Locked = b;
                }
                else if (key.StartsWith(noControlsPrefix, StringComparison.Ordinal))
                {
                    string id = key.Substring(noControlsPrefix.Length);
                    if (MeterManager.MeterContainers.ContainsKey(id))
                        MeterManager.MeterContainers[id].NoControls = b;
                }
            }
        }
    }
}
