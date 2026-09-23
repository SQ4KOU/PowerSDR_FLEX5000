using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static class P39LegacyItemsState
    {
        internal static bool HideMeters;
        internal static bool HideBands;
        internal static bool HideModes;
        internal static bool HideFilters;
        internal static bool HideVFOA;
        internal static bool HideVFOB;
        internal static bool HideVFOSync;

        private static bool loaded;

        internal static void Load()
        {
            if (loaded) return;
            loaded = true;

            ArrayList stored = DB.GetVars("SQ4KOU_LegacyItems");
            if (stored == null) return;

            foreach (object o in stored)
            {
                string s = o as string;
                if (String.IsNullOrEmpty(s)) continue;
                int slash = s.IndexOf('/');
                if (slash <= 0 || slash >= s.Length - 1) continue;

                string key = s.Substring(0, slash);
                bool value;
                if (!Boolean.TryParse(s.Substring(slash + 1), out value)) continue;

                switch (key)
                {
                    case "HideMeters": HideMeters = value; break;
                    case "HideBands": HideBands = value; break;
                    case "HideModes": HideModes = value; break;
                    case "HideFilters": HideFilters = value; break;
                    case "HideVFOA": HideVFOA = value; break;
                    case "HideVFOB": HideVFOB = value; break;
                    case "HideVFOSync": HideVFOSync = value; break;
                }
            }
        }

        internal static void Save()
        {
            ArrayList a = new ArrayList();
            a.Add("HideMeters/" + HideMeters.ToString());
            a.Add("HideBands/" + HideBands.ToString());
            a.Add("HideModes/" + HideModes.ToString());
            a.Add("HideFilters/" + HideFilters.ToString());
            a.Add("HideVFOA/" + HideVFOA.ToString());
            a.Add("HideVFOB/" + HideVFOB.ToString());
            a.Add("HideVFOSync/" + HideVFOSync.ToString());
            DB.SaveVars("SQ4KOU_LegacyItems", ref a);
        }
    }

    sealed unsafe public partial class Console
    {
        private bool p39LegacyControllerReady;
        private bool p39ApplyingLegacyVisibility;
        private readonly Dictionary<Control, bool> p39NativeVisibility = new Dictionary<Control, bool>();

        private Control[] p39MeterControls;
        private Control[] p39BandControls;
        private Control[] p39ModeControls;
        private Control[] p39FilterControls;
        private Control[] p39VFOAControls;
        private Control[] p39VFOBControls;
        private Control[] p39VFOSyncControls;

        internal void P39InitLegacyItemsController()
        {
            P39LegacyItemsState.Load();

            if (!p39LegacyControllerReady)
            {
                p39MeterControls = new Control[] { grpMultimeter, grpRX2Meter, pwrMstWatts, pwrMstSWR };
                p39BandControls = new Control[] { panelBandHF, panelBandGN, panelBandVHF };
                p39ModeControls = new Control[] { panelMode };
                p39FilterControls = new Control[] { panelFilter };
                p39VFOAControls = new Control[] { grpVFOA, VFODialA, VFODialAA };
                p39VFOBControls = new Control[] { grpVFOB, VFODialB, VFODialBB };
                p39VFOSyncControls = new Control[] { chkVFOSync };

                P39RegisterControls(p39MeterControls);
                P39RegisterControls(p39BandControls);
                P39RegisterControls(p39ModeControls);
                P39RegisterControls(p39FilterControls);
                P39RegisterControls(p39VFOAControls);
                P39RegisterControls(p39VFOBControls);
                P39RegisterControls(p39VFOSyncControls);

                p39LegacyControllerReady = true;
            }

            P39ApplyLegacyItems();
        }

        private void P39RegisterControls(Control[] controls)
        {
            foreach (Control c in controls)
            {
                if (c == null) continue;
                if (!p39NativeVisibility.ContainsKey(c))
                    p39NativeVisibility.Add(c, c.Visible);
                c.VisibleChanged += P39LegacyControlVisibleChanged;
            }
        }

        private void P39LegacyControlVisibleChanged(object sender, EventArgs e)
        {
            if (p39ApplyingLegacyVisibility) return;

            Control c = sender as Control;
            if (c == null) return;

            p39NativeVisibility[c] = c.Visible;

            if (P39ShouldHide(c) && c.Visible)
            {
                p39ApplyingLegacyVisibility = true;
                try { c.Visible = false; }
                finally { p39ApplyingLegacyVisibility = false; }
            }
        }

        private static bool P39Contains(Control[] controls, Control c)
        {
            if (controls == null) return false;
            for (int i = 0; i < controls.Length; i++)
                if (Object.ReferenceEquals(controls[i], c)) return true;
            return false;
        }

        private bool P39ShouldHide(Control c)
        {
            if (P39LegacyItemsState.HideMeters && P39Contains(p39MeterControls, c)) return true;
            if (P39LegacyItemsState.HideBands && P39Contains(p39BandControls, c)) return true;
            if (P39LegacyItemsState.HideModes && P39Contains(p39ModeControls, c)) return true;
            if (P39LegacyItemsState.HideFilters && P39Contains(p39FilterControls, c)) return true;
            if (P39LegacyItemsState.HideVFOA && P39Contains(p39VFOAControls, c)) return true;
            if (P39LegacyItemsState.HideVFOB && P39Contains(p39VFOBControls, c)) return true;
            if (P39LegacyItemsState.HideVFOSync && P39Contains(p39VFOSyncControls, c)) return true;
            return false;
        }

        private void P39ApplyGroup(Control[] controls, bool hide)
        {
            if (controls == null) return;

            foreach (Control c in controls)
            {
                if (c == null) continue;

                bool nativeVisible;
                if (!p39NativeVisibility.TryGetValue(c, out nativeVisible))
                {
                    nativeVisible = c.Visible;
                    p39NativeVisibility[c] = nativeVisible;
                }

                p39ApplyingLegacyVisibility = true;
                try
                {
                    if (hide)
                    {
                        if (c.Visible) p39NativeVisibility[c] = true;
                        c.Visible = false;
                    }
                    else
                    {
                        c.Visible = p39NativeVisibility[c];
                    }
                }
                finally
                {
                    p39ApplyingLegacyVisibility = false;
                }
            }
        }

        internal void P39ApplyLegacyItems()
        {
            if (!p39LegacyControllerReady) return;

            P39ApplyGroup(p39MeterControls, P39LegacyItemsState.HideMeters);
            P39ApplyGroup(p39BandControls, P39LegacyItemsState.HideBands);
            P39ApplyGroup(p39ModeControls, P39LegacyItemsState.HideModes);
            P39ApplyGroup(p39FilterControls, P39LegacyItemsState.HideFilters);
            P39ApplyGroup(p39VFOAControls, P39LegacyItemsState.HideVFOA);
            P39ApplyGroup(p39VFOBControls, P39LegacyItemsState.HideVFOB);
            P39ApplyGroup(p39VFOSyncControls, P39LegacyItemsState.HideVFOSync);

            PerformLayout();
            Invalidate();
        }
    }

    public partial class Setup
    {
        private TabPage p39LegacyItemsTab;
        private CheckBox p39HideMeters;
        private CheckBox p39HideBands;
        private CheckBox p39HideModes;
        private CheckBox p39HideFilters;
        private CheckBox p39HideVFOA;
        private CheckBox p39HideVFOB;
        private CheckBox p39HideVFOSync;
        private bool p39LegacyUiLoading;

        internal void P39InitLegacyItemsUI()
        {
            if (p39LegacyItemsTab != null) return;

            P39LegacyItemsState.Load();

            TabPage appearance = null;
            foreach (TabPage page in tcSetup.TabPages)
            {
                if (page.Text.IndexOf("Appearance", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    appearance = page;
                    break;
                }
            }
            if (appearance == null) return;

            TabControl subTabs = P39FindNestedTabControl(appearance);
            if (subTabs == null) return;

            foreach (TabPage page in subTabs.TabPages)
            {
                if (page.Text.Equals("Legacy Items", StringComparison.OrdinalIgnoreCase))
                {
                    p39LegacyItemsTab = page;
                    return;
                }
            }

            p39LegacyItemsTab = new TabPage("Legacy Items");
            p39LegacyItemsTab.Name = "p39LegacyItemsTab";
            p39LegacyItemsTab.UseVisualStyleBackColor = true;
            subTabs.TabPages.Add(p39LegacyItemsTab);

            GroupBox group = new GroupBox();
            group.Text = "Expanded Console UI";
            group.Location = new Point(20, 18);
            group.Size = new Size(565, 245);
            group.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            p39LegacyItemsTab.Controls.Add(group);

            p39HideMeters = P39MakeCheckBox("Hide legacy meters", 18, 28);
            p39HideBands = P39MakeCheckBox("Hide band button grid", 18, 55);
            p39HideModes = P39MakeCheckBox("Hide mode button grid", 18, 82);
            p39HideFilters = P39MakeCheckBox("Hide filter button grid", 18, 109);

            p39HideVFOA = P39MakeCheckBox("Hide VFO A", 240, 28);
            p39HideVFOB = P39MakeCheckBox("Hide VFO B", 240, 55);
            p39HideVFOSync = P39MakeCheckBox("Hide VFOSync box", 240, 82);

            group.Controls.Add(p39HideMeters);
            group.Controls.Add(p39HideBands);
            group.Controls.Add(p39HideModes);
            group.Controls.Add(p39HideFilters);
            group.Controls.Add(p39HideVFOA);
            group.Controls.Add(p39HideVFOB);
            group.Controls.Add(p39HideVFOSync);

            p39LegacyUiLoading = true;
            try
            {
                p39HideMeters.Checked = P39LegacyItemsState.HideMeters;
                p39HideBands.Checked = P39LegacyItemsState.HideBands;
                p39HideModes.Checked = P39LegacyItemsState.HideModes;
                p39HideFilters.Checked = P39LegacyItemsState.HideFilters;
                p39HideVFOA.Checked = P39LegacyItemsState.HideVFOA;
                p39HideVFOB.Checked = P39LegacyItemsState.HideVFOB;
                p39HideVFOSync.Checked = P39LegacyItemsState.HideVFOSync;
            }
            finally
            {
                p39LegacyUiLoading = false;
            }

        }

        private CheckBox P39MakeCheckBox(string text, int x, int y)
        {
            CheckBox cb = new CheckBox();
            cb.AutoSize = true;
            cb.Text = text;
            cb.Location = new Point(x, y);
            cb.CheckedChanged += P39LegacyItemChanged;
            return cb;
        }

        private TabControl P39FindNestedTabControl(Control root)
        {
            foreach (Control c in root.Controls)
            {
                TabControl tc = c as TabControl;
                if (tc != null) return tc;

                tc = P39FindNestedTabControl(c);
                if (tc != null) return tc;
            }
            return null;
        }

        private void P39LegacyItemChanged(object sender, EventArgs e)
        {
            if (p39LegacyUiLoading) return;

            P39LegacyItemsState.HideMeters = p39HideMeters.Checked;
            P39LegacyItemsState.HideBands = p39HideBands.Checked;
            P39LegacyItemsState.HideModes = p39HideModes.Checked;
            P39LegacyItemsState.HideFilters = p39HideFilters.Checked;
            P39LegacyItemsState.HideVFOA = p39HideVFOA.Checked;
            P39LegacyItemsState.HideVFOB = p39HideVFOB.Checked;
            P39LegacyItemsState.HideVFOSync = p39HideVFOSync.Checked;

            P39LegacyItemsState.Save();

            if (console != null)
            {
                console.P39InitLegacyItemsController();
                console.P39ApplyLegacyItems();
            }
        }
    }
}
