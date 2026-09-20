// SQ4KOU P23 - Setup -> Appearance -> Meters/Gadgets UI
// Control naming/layout follows the Thetis Meters/Gadgets page where practical.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static class SQ4KOUThetisMetersP23
    {
        private const string Marker = "sq4kouP23ThetisMetersPage";
        private static PowerSDR.Console _console;
        private static Setup _setup;
        private static P23MetersSetupPanel _panel;

        internal static void Install(PowerSDR.Console console, Setup setup)
        {
            if (console == null || setup == null) return;
            _console = console;
            _setup = setup;
            P23MeterManager.Initialize(console, setup);

            setup.Shown += delegate
            {
                try
                {
                    setup.BeginInvoke((MethodInvoker)delegate { InstallPage(); });
                }
                catch { }
            };
        }

        private static void InstallPage()
        {
            if (_setup == null || _setup.IsDisposed) return;
            if (_setup.Controls.Find(Marker, true).Length > 0) return;

            TabPage appearance = FindAppearanceTab(_setup);
            if (appearance == null)
            {
                Button fallback = new Button();
                fallback.Name = Marker;
                fallback.Text = "Meters/Gadgets";
                fallback.Size = new Size(120, 28);
                fallback.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                fallback.Location = new Point(Math.Max(4, _setup.ClientSize.Width - 132),
                                              Math.Max(4, _setup.ClientSize.Height - 40));
                fallback.Click += delegate { ShowStandaloneSetup(); };
                _setup.Controls.Add(fallback);
                fallback.BringToFront();
                return;
            }

            TabControl nested = FindBestTabControl(appearance);
            if (nested == null)
            {
                ShowStandaloneSetup();
                return;
            }

            TabPage page = new TabPage();
            page.Name = Marker;
            page.Text = "Meters/Gadgets";
            page.BackColor = SystemColors.Control;
            page.UseVisualStyleBackColor = false;
            _panel = new P23MetersSetupPanel();
            _panel.Dock = DockStyle.Fill;
            page.Controls.Add(_panel);
            nested.TabPages.Add(page);
        }

        private static void ShowStandaloneSetup()
        {
            Form f = new Form();
            f.Text = "PowerSDR - Meters/Gadgets";
            f.StartPosition = FormStartPosition.CenterParent;
            f.ClientSize = new Size(724, 410);
            P23MetersSetupPanel p = new P23MetersSetupPanel();
            p.Dock = DockStyle.Fill;
            f.Controls.Add(p);
            f.Show(_setup);
        }

        internal static void SelectContainer(string id)
        {
            if (_panel != null && !_panel.IsDisposed) _panel.SelectContainer(id);
        }

        private static TabPage FindAppearanceTab(Control root)
        {
            foreach (Control c in root.Controls)
            {
                TabPage tp = c as TabPage;
                if (tp != null)
                {
                    string s = ((tp.Text ?? "") + " " + (tp.Name ?? "")).Replace(" ", "").ToUpperInvariant();
                    if (s.Contains("APPEARANCE")) return tp;
                }
                TabPage nested = FindAppearanceTab(c);
                if (nested != null) return nested;
            }
            return null;
        }

        private static TabControl FindBestTabControl(Control root)
        {
            TabControl best = null;
            foreach (Control c in root.Controls)
            {
                TabControl tc = c as TabControl;
                if (tc != null && (best == null || tc.TabPages.Count > best.TabPages.Count)) best = tc;
                TabControl sub = FindBestTabControl(c);
                if (sub != null && (best == null || sub.TabPages.Count > best.TabPages.Count)) best = sub;
            }
            return best;
        }
    }

    internal sealed class P23MetersSetupPanel : UserControl
    {
        private readonly GroupBox grpMultiMeterHolder;
        private readonly ComboBox comboContainerSelect;
        private readonly Button btnAddRX1Container;
        private readonly Button btnRecoverContainer;
        private readonly Button btnContainerDelete;
        private readonly Button btnContainer_dupe;
        private readonly Button btnContainer_load;
        private readonly Button btnContainer_save;
        private readonly CheckBox chkContainerHighlight;
        private readonly CheckBox chkLockContainer;
        private readonly CheckBox chkContainerBorder;
        private readonly CheckBox chkContainerNoTitle;
        private readonly CheckBox chkContainerShowRX;
        private readonly CheckBox chkContainerShowTX;
        private readonly CheckBox chkContainerMinimises;
        private readonly CheckBox chkContainer_hidewhennotused;
        private readonly CheckBox chkMultiMeter_auto_container_height;
        private readonly RadioButton radContainer_rx1_data;
        private readonly RadioButton radContainer_rx2_data;
        private readonly Button clrbtnContainerBackground;
        private readonly TextBox txtContainerNotes;
        private readonly ListBox lstMetersAvailable;
        private readonly ListBox lstMetersInUse;
        private readonly Button btnAddMeterItem;
        private readonly Button btnRemoveMeterItem;
        private readonly Button btnMeterUp;
        private readonly Button btnMeterDown;
        private readonly Button btnMeterCopySettings;
        private readonly Button btnMeterPasteSettings;
        private readonly PropertyGrid propertyGrid;
        private readonly Button btnMMIO;
        private readonly Button btnSkins;
        private P23MeterItemConfig _clipboard;
        private bool _loading;

        internal P23MetersSetupPanel()
        {
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            MinimumSize = new Size(710, 395);

            grpMultiMeterHolder = new GroupBox();
            grpMultiMeterHolder.Name = "grpMultiMeterHolder";
            grpMultiMeterHolder.Location = new Point(8, 8);
            grpMultiMeterHolder.Size = new Size(710, 395);
            grpMultiMeterHolder.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(grpMultiMeterHolder);

            comboContainerSelect = new ComboBox();
            comboContainerSelect.Name = "comboContainerSelect";
            comboContainerSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            comboContainerSelect.Location = new Point(6, 13);
            comboContainerSelect.Size = new Size(193, 21);
            comboContainerSelect.SelectedIndexChanged += delegate { LoadSelectedContainer(); };
            grpMultiMeterHolder.Controls.Add(comboContainerSelect);

            btnAddRX1Container = MakeButton("btnAddRX1Container", "Add\r\nContainer", 209, 13, 71, 44);
            btnAddRX1Container.Click += delegate { SelectAfter(P23MeterManager.AddContainer()); };
            grpMultiMeterHolder.Controls.Add(btnAddRX1Container);

            btnRecoverContainer = MakeButton("btnRecoverContainer", "Recover\r\nContainer", 209, 63, 71, 44);
            btnRecoverContainer.Click += delegate
            {
                P23MeterContainer c = CurrentContainer();
                if (c != null) P23MeterManager.Recover(c.Config.Id);
                LoadSelectedContainer();
            };
            grpMultiMeterHolder.Controls.Add(btnRecoverContainer);

            btnContainerDelete = MakeButton("btnContainerDelete", "Remove\r\nContainer", 209, 119, 71, 44);
            btnContainerDelete.Click += delegate
            {
                P23MeterContainer c = CurrentContainer();
                if (c != null) P23MeterManager.Remove(c.Config.Id);
            };
            grpMultiMeterHolder.Controls.Add(btnContainerDelete);

            chkContainerHighlight = MakeCheck("chkContainerHighlight", "Highlight", 286, 13);
            chkContainerHighlight.CheckedChanged += delegate
            {
                P23MeterContainer[] cs = P23MeterManager.GetContainers();
                for (int i = 0; i < cs.Length; i++) cs[i].SetHighlight(false);
                P23MeterContainer c = CurrentContainer();
                if (c != null && chkContainerHighlight.Checked) c.SetHighlight(true);
            };
            grpMultiMeterHolder.Controls.Add(chkContainerHighlight);

            radContainer_rx1_data = MakeRadio("radContainer_rx1_data", "RX1 data", 290, 33);
            radContainer_rx1_data.CheckedChanged += delegate { if (!_loading && radContainer_rx1_data.Checked) SetCfg(delegate(P23ContainerConfig c) { c.RX = 1; }); };
            grpMultiMeterHolder.Controls.Add(radContainer_rx1_data);

            radContainer_rx2_data = MakeRadio("radContainer_rx2_data", "RX2 data", 290, 51);
            radContainer_rx2_data.CheckedChanged += delegate { if (!_loading && radContainer_rx2_data.Checked) SetCfg(delegate(P23ContainerConfig c) { c.RX = 2; }); };
            grpMultiMeterHolder.Controls.Add(radContainer_rx2_data);

            chkLockContainer = MakeCheck("chkLockContainer", "Lock", 310, 70);
            chkLockContainer.RightToLeft = RightToLeft.Yes;
            chkLockContainer.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.Locked = chkLockContainer.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkLockContainer);

            chkContainer_hidewhennotused = MakeCheck("chkContainer_hidewhennotused", "Hide if RX\r\nnot in use", 286, 89);
            chkContainer_hidewhennotused.AutoSize = false;
            chkContainer_hidewhennotused.Size = new Size(80, 32);
            chkContainer_hidewhennotused.RightToLeft = RightToLeft.Yes;
            chkContainer_hidewhennotused.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.HideWhenRxNotUsed = chkContainer_hidewhennotused.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkContainer_hidewhennotused);

            chkContainerMinimises = MakeCheck("chkContainerMinimises", "Minimise", 294, 122);
            chkContainerMinimises.RightToLeft = RightToLeft.Yes;
            chkContainerMinimises.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.ContainerMinimises = chkContainerMinimises.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkContainerMinimises);

            chkContainerShowRX = MakeCheck("chkContainerShowRX", "Show RX", 286, 143);
            chkContainerShowRX.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.ShowOnRX = chkContainerShowRX.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkContainerShowRX);

            chkContainerShowTX = MakeCheck("chkContainerShowTX", "Show TX", 286, 160);
            chkContainerShowTX.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.ShowOnTX = chkContainerShowTX.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkContainerShowTX);

            chkContainerBorder = MakeCheck("chkContainerBorder", "Border", 374, 13);
            chkContainerBorder.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.Border = chkContainerBorder.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkContainerBorder);

            chkContainerNoTitle = MakeCheck("chkContainerNoTitle", "No title/controls", 374, 34);
            chkContainerNoTitle.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.NoControls = chkContainerNoTitle.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkContainerNoTitle);

            chkMultiMeter_auto_container_height = MakeCheck("chkMultiMeter_auto_container_height", "Auto height", 374, 55);
            chkMultiMeter_auto_container_height.CheckedChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.AutoHeight = chkMultiMeter_auto_container_height.Checked; }); };
            grpMultiMeterHolder.Controls.Add(chkMultiMeter_auto_container_height);

            Label bgLabel = new Label();
            bgLabel.Text = "Background:";
            bgLabel.AutoSize = true;
            bgLabel.Location = new Point(91, 107);
            grpMultiMeterHolder.Controls.Add(bgLabel);

            clrbtnContainerBackground = MakeButton("clrbtnContainerBackground", "", 159, 102, 40, 23);
            clrbtnContainerBackground.BackColor = Color.Black;
            clrbtnContainerBackground.Click += ChooseBackground;
            grpMultiMeterHolder.Controls.Add(clrbtnContainerBackground);

            Label notesLabel = new Label();
            notesLabel.Text = "Notes:";
            notesLabel.AutoSize = true;
            notesLabel.Location = new Point(7, 113);
            grpMultiMeterHolder.Controls.Add(notesLabel);

            txtContainerNotes = new TextBox();
            txtContainerNotes.Name = "txtContainerNotes";
            txtContainerNotes.Location = new Point(48, 109);
            txtContainerNotes.Size = new Size(104, 48);
            txtContainerNotes.Multiline = true;
            txtContainerNotes.TextChanged += delegate { if (!_loading) SetCfg(delegate(P23ContainerConfig c) { c.Notes = txtContainerNotes.Text; }); };
            grpMultiMeterHolder.Controls.Add(txtContainerNotes);

            btnContainer_dupe = MakeButton("btnContainer_dupe", "Copy", 153, 278, 32, 32);
            btnContainer_dupe.Click += delegate
            {
                P23MeterContainer c = CurrentContainer();
                if (c != null) SelectAfter(P23MeterManager.Duplicate(c.Config.Id));
            };
            grpMultiMeterHolder.Controls.Add(btnContainer_dupe);

            btnContainer_load = MakeButton("btnContainer_load", "L", 153, 316, 32, 32);
            btnContainer_load.Click += LoadContainerFile;
            grpMultiMeterHolder.Controls.Add(btnContainer_load);

            btnContainer_save = MakeButton("btnContainer_save", "S", 153, 354, 32, 32);
            btnContainer_save.Click += SaveContainerFile;
            grpMultiMeterHolder.Controls.Add(btnContainer_save);

            lstMetersAvailable = new ListBox();
            lstMetersAvailable.Name = "lstMetersAvailable";
            lstMetersAvailable.Location = new Point(7, 174);
            lstMetersAvailable.Size = new Size(140, 212);
            lstMetersAvailable.DoubleClick += delegate { AddSelectedItem(); };
            grpMultiMeterHolder.Controls.Add(lstMetersAvailable);

            lstMetersInUse = new ListBox();
            lstMetersInUse.Name = "lstMetersInUse";
            lstMetersInUse.Location = new Point(191, 174);
            lstMetersInUse.Size = new Size(140, 212);
            lstMetersInUse.SelectedIndexChanged += delegate
            {
                propertyGrid.SelectedObject = lstMetersInUse.SelectedItem as P23MeterItemConfig;
            };
            lstMetersInUse.DoubleClick += delegate { RemoveSelectedItem(); };
            grpMultiMeterHolder.Controls.Add(lstMetersInUse);

            btnAddMeterItem = MakeButton("btnAddMeterItem", ">", 153, 174, 32, 32);
            btnAddMeterItem.Click += delegate { AddSelectedItem(); };
            grpMultiMeterHolder.Controls.Add(btnAddMeterItem);

            btnRemoveMeterItem = MakeButton("btnRemoveMeterItem", "<", 153, 212, 32, 32);
            btnRemoveMeterItem.Click += delegate { RemoveSelectedItem(); };
            grpMultiMeterHolder.Controls.Add(btnRemoveMeterItem);

            btnMeterUp = MakeButton("btnMeterUp", "▲", 337, 174, 32, 32);
            btnMeterUp.Click += delegate { MoveItem(-1); };
            grpMultiMeterHolder.Controls.Add(btnMeterUp);

            btnMeterDown = MakeButton("btnMeterDown", "▼", 337, 212, 32, 32);
            btnMeterDown.Click += delegate { MoveItem(1); };
            grpMultiMeterHolder.Controls.Add(btnMeterDown);

            btnMeterCopySettings = MakeButton("btnMeterCopySettings", "Copy item", 337, 250, 76, 26);
            btnMeterCopySettings.Click += delegate
            {
                P23MeterItemConfig i = lstMetersInUse.SelectedItem as P23MeterItemConfig;
                _clipboard = i == null ? null : i.Clone();
            };
            grpMultiMeterHolder.Controls.Add(btnMeterCopySettings);

            btnMeterPasteSettings = MakeButton("btnMeterPasteSettings", "Paste item", 337, 280, 76, 26);
            btnMeterPasteSettings.Click += delegate
            {
                P23MeterContainer c = CurrentContainer();
                int idx = lstMetersInUse.SelectedIndex;
                if (c == null || _clipboard == null || idx < 0 || idx >= c.Config.Items.Count) return;
                P23MeterItemConfig n = _clipboard.Clone();
                n.Id = c.Config.Items[idx].Id;
                c.Config.Items[idx] = n;
                c.NotifyChanged();
                ReloadMeterLists(idx);
            };
            grpMultiMeterHolder.Controls.Add(btnMeterPasteSettings);

            GroupBox grpMeterItemSettings = new GroupBox();
            grpMeterItemSettings.Name = "grpMeterItemSettings";
            grpMeterItemSettings.Text = "Selected item settings";
            grpMeterItemSettings.Location = new Point(419, 10);
            grpMeterItemSettings.Size = new Size(283, 318);
            grpMeterItemSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpMultiMeterHolder.Controls.Add(grpMeterItemSettings);

            propertyGrid = new PropertyGrid();
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.HelpVisible = true;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.PropertyValueChanged += delegate
            {
                P23MeterContainer c = CurrentContainer();
                if (c != null) c.NotifyChanged();
                ReloadMeterLists(lstMetersInUse.SelectedIndex);
            };
            grpMeterItemSettings.Controls.Add(propertyGrid);

            btnMMIO = MakeButton("btnMMIO", "MultiMeter I/O...", 419, 337, 100, 28);
            btnMMIO.Click += delegate
            {
                using (P23MultiMeterIOForm f = new P23MultiMeterIOForm()) f.ShowDialog(FindForm());
            };
            grpMultiMeterHolder.Controls.Add(btnMMIO);

            btnSkins = MakeButton("btnP23Skins", "Skin server...", 526, 337, 92, 28);
            btnSkins.Click += BrowseSkins;
            grpMultiMeterHolder.Controls.Add(btnSkins);

            Button saveAll = MakeButton("btnP23SaveAll", "Save all", 625, 337, 72, 28);
            saveAll.Click += delegate { P23MeterManager.SaveAll(); };
            grpMultiMeterHolder.Controls.Add(saveAll);

            ToolTip tips = new ToolTip();
            tips.SetToolTip(comboContainerSelect, "Selected container. Each floating container has a unique ID in its title.");
            tips.SetToolTip(btnAddRX1Container, "Add a meter item container");
            tips.SetToolTip(btnRecoverContainer, "Recover this container to the console window");
            tips.SetToolTip(btnContainerDelete, "Remove selected container and its meter items");
            tips.SetToolTip(chkLockContainer, "Lock container against move/remove/add/remove. Item properties remain editable.");
            tips.SetToolTip(chkContainerNoTitle, "Hide title/control bar. Hold SHIFT over the container to reveal controls.");
            tips.SetToolTip(btnMeterUp, "Move item up");
            tips.SetToolTip(btnMeterDown, "Move item down");
            tips.SetToolTip(btnAddMeterItem, "Include the item");
            tips.SetToolTip(btnRemoveMeterItem, "Remove the item");

            foreach (P23MeterItemType t in P23MeterItemNames.Available)
                lstMetersAvailable.Items.Add(new P23AvailableItem(t));

            P23MeterManager.ContainersChanged += ManagerContainersChanged;
            P23MeterManager.SelectContainerRequested += ManagerSelectRequested;
            ReloadContainers(null);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                P23MeterManager.ContainersChanged -= ManagerContainersChanged;
                P23MeterManager.SelectContainerRequested -= ManagerSelectRequested;
            }
            base.Dispose(disposing);
        }

        internal void SelectContainer(string id)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { SelectContainer(id); });
                return;
            }
            for (int i = 0; i < comboContainerSelect.Items.Count; i++)
            {
                P23ContainerChoice choice = comboContainerSelect.Items[i] as P23ContainerChoice;
                if (choice != null && String.Equals(choice.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    comboContainerSelect.SelectedIndex = i;
                    return;
                }
            }
        }

        private void ManagerContainersChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke((MethodInvoker)delegate { ReloadContainers(CurrentId()); });
            else ReloadContainers(CurrentId());
        }

        private void ManagerSelectRequested(string id)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke((MethodInvoker)delegate { SelectContainer(id); });
            else SelectContainer(id);
            SQ4KOUThetisMetersP23.SelectContainer(id);
        }

        private string CurrentId()
        {
            P23ContainerChoice c = comboContainerSelect.SelectedItem as P23ContainerChoice;
            return c == null ? null : c.Id;
        }

        private P23MeterContainer CurrentContainer()
        {
            return P23MeterManager.Find(CurrentId());
        }

        private void ReloadContainers(string keepId)
        {
            _loading = true;
            try
            {
                comboContainerSelect.Items.Clear();
                P23MeterContainer[] cs = P23MeterManager.GetContainers();
                int selected = -1;
                for (int i = 0; i < cs.Length; i++)
                {
                    P23ContainerChoice choice = new P23ContainerChoice(cs[i].Config);
                    comboContainerSelect.Items.Add(choice);
                    if (!String.IsNullOrEmpty(keepId) &&
                        String.Equals(keepId, choice.Id, StringComparison.OrdinalIgnoreCase))
                        selected = i;
                }
                if (comboContainerSelect.Items.Count > 0)
                    comboContainerSelect.SelectedIndex = selected >= 0 ? selected : 0;
            }
            finally { _loading = false; }
            LoadSelectedContainer();
        }

        private void LoadSelectedContainer()
        {
            P23MeterContainer c = CurrentContainer();
            _loading = true;
            try
            {
                bool has = c != null;
                btnContainerDelete.Enabled = has;
                btnRecoverContainer.Enabled = has;
                btnContainer_dupe.Enabled = has;
                btnContainer_save.Enabled = has;
                chkContainerHighlight.Enabled = has;
                if (!has)
                {
                    lstMetersInUse.Items.Clear();
                    propertyGrid.SelectedObject = null;
                    return;
                }

                P23ContainerConfig cfg = c.Config;
                radContainer_rx1_data.Checked = cfg.RX != 2;
                radContainer_rx2_data.Checked = cfg.RX == 2;
                chkLockContainer.Checked = cfg.Locked;
                chkContainer_hidewhennotused.Checked = cfg.HideWhenRxNotUsed;
                chkContainerMinimises.Checked = cfg.ContainerMinimises;
                chkContainerShowRX.Checked = cfg.ShowOnRX;
                chkContainerShowTX.Checked = cfg.ShowOnTX;
                chkContainerBorder.Checked = cfg.Border;
                chkContainerNoTitle.Checked = cfg.NoControls;
                chkMultiMeter_auto_container_height.Checked = cfg.AutoHeight;
                txtContainerNotes.Text = cfg.Notes ?? "";
                clrbtnContainerBackground.BackColor = Color.FromArgb(cfg.BackColorArgb);
                ReloadMeterLists(-1);
            }
            finally { _loading = false; }
        }

        private void ReloadMeterLists(int select)
        {
            P23MeterContainer c = CurrentContainer();
            lstMetersInUse.BeginUpdate();
            try
            {
                lstMetersInUse.Items.Clear();
                if (c != null && c.Config.Items != null)
                    for (int i = 0; i < c.Config.Items.Count; i++) lstMetersInUse.Items.Add(c.Config.Items[i]);
                if (select >= 0 && select < lstMetersInUse.Items.Count) lstMetersInUse.SelectedIndex = select;
            }
            finally { lstMetersInUse.EndUpdate(); }
            if (lstMetersInUse.SelectedIndex < 0) propertyGrid.SelectedObject = null;
        }

        private void SetCfg(Action<P23ContainerConfig> action)
        {
            P23MeterContainer c = CurrentContainer();
            if (c == null) return;
            action(c.Config);
            c.NotifyChanged();
            ReloadContainerChoiceText();
        }

        private void ReloadContainerChoiceText()
        {
            string id = CurrentId();
            ReloadContainers(id);
        }

        private void AddSelectedItem()
        {
            P23MeterContainer c = CurrentContainer();
            P23AvailableItem ai = lstMetersAvailable.SelectedItem as P23AvailableItem;
            if (c == null || ai == null || c.Config.Locked) return;
            P23MeterItemConfig item = DefaultsFor(ai.Type);
            c.Config.Items.Add(item);
            c.NotifyChanged();
            ReloadMeterLists(c.Config.Items.Count - 1);
        }

        private void RemoveSelectedItem()
        {
            P23MeterContainer c = CurrentContainer();
            int idx = lstMetersInUse.SelectedIndex;
            if (c == null || c.Config.Locked || idx < 0 || idx >= c.Config.Items.Count) return;
            c.Config.Items.RemoveAt(idx);
            c.NotifyChanged();
            ReloadMeterLists(Math.Min(idx, c.Config.Items.Count - 1));
        }

        private void MoveItem(int delta)
        {
            P23MeterContainer c = CurrentContainer();
            int idx = lstMetersInUse.SelectedIndex;
            if (c == null || c.Config.Locked || idx < 0 || idx >= c.Config.Items.Count) return;
            int n = idx + delta;
            if (n < 0 || n >= c.Config.Items.Count) return;
            P23MeterItemConfig tmp = c.Config.Items[idx];
            c.Config.Items[idx] = c.Config.Items[n];
            c.Config.Items[n] = tmp;
            c.NotifyChanged();
            ReloadMeterLists(n);
        }

        private static P23MeterItemConfig DefaultsFor(P23MeterItemType t)
        {
            P23MeterItemConfig i = new P23MeterItemConfig();
            i.Type = t;
            i.Name = P23MeterItemNames.Display(t);
            switch (t)
            {
                case P23MeterItemType.SIGNAL_STRENGTH:
                case P23MeterItemType.AVG_SIGNAL_STRENGTH:
                case P23MeterItemType.ANANMM:
                    i.Height = 88; i.Minimum = -140; i.Maximum = -20; break;
                case P23MeterItemType.PWR:
                    i.Height = 60; i.Minimum = 0; i.Maximum = 100; break;
                case P23MeterItemType.REVERSE_PWR:
                    i.Height = 60; i.Minimum = 0; i.Maximum = 25; break;
                case P23MeterItemType.SWR:
                    i.Height = 60; i.Minimum = 1; i.Maximum = 5; break;
                case P23MeterItemType.VFO_DISPLAY:
                case P23MeterItemType.DIAL_DISPLAY:
                    i.Height = 92; break;
                case P23MeterItemType.CLOCK:
                    i.Height = 64; break;
                case P23MeterItemType.SPACER:
                    i.Height = 16; break;
                case P23MeterItemType.TEXT_OVERLAY:
                    i.Height = 48; i.Text = "%VFOA%  %BAND%  %MODE%  %SIGNAL% dBm"; break;
                case P23MeterItemType.LED:
                    i.Height = 36; i.Condition = "POWER_ON==true"; i.Text = "RADIO"; break;
                case P23MeterItemType.HISTORY:
                    i.Height = 100; i.Source = "SIGNAL"; i.Minimum = -140; i.Maximum = -20; break;
                case P23MeterItemType.ROTATOR:
                    i.Height = 160; break;
                case P23MeterItemType.WEB_IMAGE:
                    i.Height = 160; break;
                case P23MeterItemType.CUSTOM_METER_BAR:
                    i.Height = 60; i.Source = "SIGNAL"; i.Minimum = -140; i.Maximum = -20; break;
                default:
                    i.Height = 44; break;
            }
            return i;
        }

        private void ChooseBackground(object sender, EventArgs e)
        {
            P23MeterContainer c = CurrentContainer();
            if (c == null) return;
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = Color.FromArgb(c.Config.BackColorArgb);
                if (cd.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    clrbtnContainerBackground.BackColor = cd.Color;
                    c.Config.BackColorArgb = cd.Color.ToArgb();
                    c.NotifyChanged();
                }
            }
        }

        private void SaveContainerFile(object sender, EventArgs e)
        {
            P23MeterContainer c = CurrentContainer();
            if (c == null) return;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.Filter = "Container Files|*.dat";
                sfd.Title = "Save Container";
                sfd.FileName = "MeterContainer-" + c.Config.Id.Substring(0, 5) + ".dat";
                if (sfd.ShowDialog(FindForm()) == DialogResult.OK)
                    P23MeterManager.SaveContainer(c.Config.Id, sfd.FileName);
            }
        }

        private void LoadContainerFile(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.Filter = "Container Files|*.dat|JSON files|*.json|All files|*.*";
                ofd.Title = "Load Container";
                if (ofd.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    try
                    {
                        P23MeterContainer c = P23MeterManager.LoadContainer(ofd.FileName);
                        if (c == null)
                        {
                            MessageBox.Show(FindForm(),
                                "The file is not a P23/Thetis-compatible container understood by this build.",
                                "Meters/Gadgets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else SelectAfter(c);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(FindForm(), ex.Message, "Load Container",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BrowseSkins(object sender, EventArgs e)
        {
            P23MeterContainer c = CurrentContainer();
            if (c == null) return;
            using (OE3IDESkinBrowser f = new OE3IDESkinBrowser(P23MeterManager.Console))
            {
                if (f.ShowDialog(FindForm()) == DialogResult.OK && !String.IsNullOrEmpty(f.SelectedBackground))
                {
                    c.Config.BackgroundImagePath = f.SelectedBackground;
                    c.NotifyChanged();
                }
            }
        }

        private void SelectAfter(P23MeterContainer c)
        {
            if (c == null) return;
            ReloadContainers(c.Config.Id);
        }

        private static Button MakeButton(string name, string text, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Name = name;
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            b.UseVisualStyleBackColor = true;
            return b;
        }

        private static CheckBox MakeCheck(string name, string text, int x, int y)
        {
            CheckBox c = new CheckBox();
            c.Name = name;
            c.Text = text;
            c.AutoSize = true;
            c.Location = new Point(x, y);
            return c;
        }

        private static RadioButton MakeRadio(string name, string text, int x, int y)
        {
            RadioButton r = new RadioButton();
            r.Name = name;
            r.Text = text;
            r.AutoSize = true;
            r.Location = new Point(x, y);
            return r;
        }
    }

    internal sealed class P23AvailableItem
    {
        internal readonly P23MeterItemType Type;
        internal P23AvailableItem(P23MeterItemType t) { Type = t; }
        public override string ToString() { return P23MeterItemNames.Display(Type); }
    }

    internal sealed class P23ContainerChoice
    {
        internal readonly string Id;
        private readonly P23ContainerConfig _cfg;
        internal P23ContainerChoice(P23ContainerConfig c) { _cfg = c; Id = c.Id; }
        public override string ToString() { return _cfg.ToString(); }
    }

    internal sealed class P23MultiMeterIOForm : Form
    {
        private readonly NumericUpDown _udpPort;
        private readonly ComboBox _com;
        private readonly NumericUpDown _baud;
        private readonly ListView _vars;
        private readonly Timer _timer;

        internal P23MultiMeterIOForm()
        {
            Text = "MultiMeter I/O";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(600, 390);
            MinimumSize = new Size(520, 340);

            GroupBox net = new GroupBox();
            net.Text = "UDP input";
            net.Location = new Point(10, 10);
            net.Size = new Size(270, 82);
            Controls.Add(net);

            _udpPort = new NumericUpDown();
            _udpPort.Minimum = 1;
            _udpPort.Maximum = 65535;
            _udpPort.Value = 9000;
            _udpPort.Location = new Point(12, 31);
            _udpPort.Width = 80;
            net.Controls.Add(_udpPort);

            Button startUdp = new Button();
            startUdp.Text = "Start";
            startUdp.Location = new Point(102, 29);
            startUdp.Click += delegate
            {
                try { P23MultiMeterIO.StartUdp((int)_udpPort.Value); }
                catch (Exception ex) { MessageBox.Show(this, ex.Message); }
            };
            net.Controls.Add(startUdp);

            Button stopUdp = new Button();
            stopUdp.Text = "Stop";
            stopUdp.Location = new Point(181, 29);
            stopUdp.Click += delegate { P23MultiMeterIO.StopUdp(); };
            net.Controls.Add(stopUdp);

            GroupBox serial = new GroupBox();
            serial.Text = "Serial input";
            serial.Location = new Point(290, 10);
            serial.Size = new Size(300, 82);
            Controls.Add(serial);

            _com = new ComboBox();
            _com.DropDownStyle = ComboBoxStyle.DropDownList;
            _com.Location = new Point(10, 31);
            _com.Width = 78;
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            _com.Items.AddRange(ports);
            if (_com.Items.Count > 0) _com.SelectedIndex = 0;
            serial.Controls.Add(_com);

            _baud = new NumericUpDown();
            _baud.Minimum = 300;
            _baud.Maximum = 921600;
            _baud.Value = 9600;
            _baud.Increment = 1200;
            _baud.Location = new Point(94, 31);
            _baud.Width = 76;
            serial.Controls.Add(_baud);

            Button startSerial = new Button();
            startSerial.Text = "Start";
            startSerial.Location = new Point(176, 29);
            startSerial.Width = 54;
            startSerial.Click += delegate
            {
                if (_com.SelectedItem == null) return;
                try { P23MultiMeterIO.StartSerial(_com.SelectedItem.ToString(), (int)_baud.Value); }
                catch (Exception ex) { MessageBox.Show(this, ex.Message); }
            };
            serial.Controls.Add(startSerial);

            Button stopSerial = new Button();
            stopSerial.Text = "Stop";
            stopSerial.Location = new Point(236, 29);
            stopSerial.Width = 54;
            stopSerial.Click += delegate { P23MultiMeterIO.StopSerial(); };
            serial.Controls.Add(stopSerial);

            Label info = new Label();
            info.Text = "Accepted input: JSON object or lines KEY=VALUE. Variables are available to Text Overlay, LED, Rotator and Custom Meter Bar.";
            info.Location = new Point(10, 102);
            info.Size = new Size(580, 34);
            Controls.Add(info);

            _vars = new ListView();
            _vars.View = View.Details;
            _vars.FullRowSelect = true;
            _vars.GridLines = true;
            _vars.Location = new Point(10, 140);
            _vars.Size = new Size(580, 205);
            _vars.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _vars.Columns.Add("Variable", 230);
            _vars.Columns.Add("Value", 330);
            Controls.Add(_vars);

            Button close = new Button();
            close.Text = "Close";
            close.Location = new Point(515, 352);
            close.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            close.Click += delegate { Close(); };
            Controls.Add(close);

            _timer = new Timer();
            _timer.Interval = 500;
            _timer.Tick += delegate { RefreshVars(); };
            _timer.Start();
            FormClosed += delegate { _timer.Stop(); _timer.Dispose(); };
        }

        private void RefreshVars()
        {
            Dictionary<string, object> v = P23MultiMeterIO.Snapshot();
            _vars.BeginUpdate();
            try
            {
                _vars.Items.Clear();
                foreach (KeyValuePair<string, object> kv in v)
                {
                    ListViewItem li = new ListViewItem(kv.Key);
                    li.SubItems.Add(kv.Value == null ? "" : Convert.ToString(kv.Value, CultureInfo.InvariantCulture));
                    _vars.Items.Add(li);
                }
            }
            finally { _vars.EndUpdate(); }
        }
    }
}
