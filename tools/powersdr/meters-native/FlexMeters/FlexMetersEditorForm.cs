using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FlexMeters
{
    // PowerSDR port of the Thetis Setup -> Appearance -> Meters/Gadgets
    // grpMultiMeterHolder.  The primary 710x395 layout and control semantics
    // are intentionally kept aligned with pinned Thetis.
    public sealed class FlexMetersEditorForm : Form
    {
        private const int MaxContainers = 50;

        private sealed class ContainerChoice
        {
            public int Index;
            public Guid Id;
            public string Text;
            public override string ToString()
            {
                return Index.ToString(CultureInfo.InvariantCulture) + " - " +
                    (String.IsNullOrWhiteSpace(Text) ? "Container" : Text);
            }
        }

        private sealed class MeterTypeChoice
        {
            public string Type;
            public string Text;
            public override string ToString() { return Text; }
        }

        private static readonly MeterTypeChoice[] SupportedTypes =
            BuildSupportedTypes();

        private readonly MeterWorkspaceManager _manager;
        private bool _refreshing;
        private bool _disposedSubscription;

        private GroupBox grpMultiMeterHolder;
        private ComboBox comboContainerSelect;
        private Button btnAddRX1Container;
        private RadioButton radContainer_rx1_data;
        private RadioButton radContainer_rx2_data;
        private CheckBox chkContainerHighlight;
        private CheckBox chkContainerBorder;
        private CheckBox chkContainerNoTitle;
        private CheckBox chkContainerShowRX;
        private CheckBox chkContainerShowTX;
        private CheckBox chkMultiMeter_auto_container_height;
        private CheckBox chkLockContainer;
        private CheckBox chkContainer_hidewhennotused;
        private CheckBox chkContainerMinimises;
        private Label lblMMContainerBackground;
        private Button clrbtnContainerBackground;
        private Label lblMMContainerNotes;
        private TextBox txtContainerNotes;
        private Button btnRecoverContainer;
        private Button btnContainerDelete;
        private ListBox lstMetersAvailable;
        private ListBox lstMetersInUse;
        private Button btnAddMeterItem;
        private Button btnRemoveMeterItem;
        private Button btnMeterUp;
        private Button btnMeterDown;
        private Button btnContainer_dupe;
        private Button btnContainer_load;
        private Button btnContainer_save;
        private Button btnMeterCopySettings;
        private Button btnMeterPasteSettings;
        private GroupBox grpMeterItemSettings;

        private NumericUpDown nudMeterItemUpdateRate;
        private NumericUpDown nudMeterItemAttackRate;
        private NumericUpDown nudMeterItemDecayRate;
        private Button clrbtnMeterItemHBackground;
        private Button clrbtnMeterItemLow;
        private Button clrbtnMeterItemHigh;
        private CheckBox chkMeterItemFadeOnRx;
        private CheckBox chkMeterItemFadeOnTx;
        private CheckBox chkMeterItemHistory;
        private CheckBox chkMeterItemPeakHold;
        private CheckBox chkMeterItemPeakValue;
        private CheckBox chkMeterItemTitle;
        private CheckBox chkMeterItemSolid;
        private CheckBox chkMeterItemShadow;
        private CheckBox chkMeterItemSegmented;
        private CheckBox chkMeterItemDarkMode;

        private Dictionary<string, string> _copiedSettings;

        public FlexMetersEditorForm(MeterWorkspaceManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _manager = manager;

            Text = "Meters/Gadgets";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(726, 411);
            MinimumSize = new Size(742, 450);
            MaximizeBox = true;
            MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            AutoScaleMode = AutoScaleMode.Font;

            BuildThetisLayout();

            for (int i = 0; i < SupportedTypes.Length; i++)
                lstMetersAvailable.Items.Add(SupportedTypes[i]);

            _manager.WorkspaceChanged += ManagerWorkspaceChanged;
            RefreshFromWorkspace(null);
        }

        public int ContainerCount
        {
            get { return _manager.ContainerCount; }
        }

        public Guid? SelectedContainerId
        {
            get
            {
                ContainerChoice choice =
                    comboContainerSelect.SelectedItem as ContainerChoice;
                return choice == null ? (Guid?)null : choice.Id;
            }
        }

        public string[] AvailableItemTypes
        {
            get
            {
                var result = new string[SupportedTypes.Length];
                for (int i = 0; i < SupportedTypes.Length; i++)
                    result[i] = SupportedTypes[i].Type;
                return result;
            }
        }

        public Guid AddContainer(MeterReceiver receiver)
        {
            if (receiver != MeterReceiver.Rx1)
                return Guid.Empty;
            if (_manager.ContainerCount >= MaxContainers)
                return Guid.Empty;

            int offset = _manager.ContainerCount * 24;
            var container = new MeterContainerSnapshot
            {
                Id = Guid.NewGuid(),
                Receiver = MeterReceiver.Rx1,
                VisibleOnReceive = true,
                VisibleOnTransmit = true,
                Border = true,
                Highlight = false,
                Locked = false,
                NoTitleBar = false,
                AutoHeight = false,
                Minimises = true,
                HideWhenReceiverNotUsed = true,
                BackgroundArgb = Color.FromArgb(32, 32, 32).ToArgb(),
                Notes = "",
                Geometry = new MeterWindowGeometry
                {
                    X = 40 + offset,
                    Y = 40 + offset,
                    Width = 420,
                    Height = 180,
                    Maximized = false
                }
            };

            _manager.AddContainer(container);
            RefreshFromWorkspace(container.Id);
            return container.Id;
        }

        public bool RemoveSelectedContainer()
        {
            Guid? selected = SelectedContainerId;
            if (!selected.HasValue)
                return false;

            bool removed = _manager.RemoveContainer(selected.Value);
            RefreshFromWorkspace(null);
            return removed;
        }

        public bool SelectContainer(Guid containerId)
        {
            for (int i = 0; i < comboContainerSelect.Items.Count; i++)
            {
                ContainerChoice choice =
                    comboContainerSelect.Items[i] as ContainerChoice;
                if (choice != null && choice.Id == containerId)
                {
                    comboContainerSelect.SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        public bool SelectItem(int index)
        {
            if (index < 0 || index >= lstMetersInUse.Items.Count)
                return false;
            lstMetersInUse.SelectedIndex = index;
            return true;
        }

        public bool AddItemToSelectedContainer(string itemType)
        {
            if (!MeterItemCatalog.IsSupported(itemType))
                return false;

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null || container.Locked)
                return false;

            var item = new MeterItemSnapshot
            {
                Id = Guid.NewGuid(),
                Type = itemType
            };
            ThetisMeterItemSettings.ApplyDefaults(item);
            container.Items.Add(item);

            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
            lstMetersInUse.SelectedIndex = lstMetersInUse.Items.Count - 1;
            return true;
        }

        public bool RemoveSelectedItem()
        {
            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null || container.Locked)
                return false;

            int index = lstMetersInUse.SelectedIndex;
            if (index < 0 || index >= container.Items.Count)
                return false;

            container.Items.RemoveAt(index);
            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);

            if (lstMetersInUse.Items.Count > 0)
                lstMetersInUse.SelectedIndex =
                    Math.Min(index, lstMetersInUse.Items.Count - 1);
            return true;
        }

        public bool MoveSelectedItem(int delta)
        {
            if (delta != -1 && delta != 1)
                throw new ArgumentOutOfRangeException("delta");

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null || container.Locked)
                return false;

            int index = lstMetersInUse.SelectedIndex;
            int target = index + delta;
            if (index < 0 || target < 0 || target >= container.Items.Count)
                return false;

            MeterItemSnapshot item = container.Items[index];
            container.Items.RemoveAt(index);
            container.Items.Insert(target, item);
            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
            lstMetersInUse.SelectedIndex = target;
            return true;
        }

        public bool SetSelectedContainerVisibility(bool showRx, bool showTx)
        {
            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return false;

            container.VisibleOnReceive = showRx;
            container.VisibleOnTransmit = showTx;
            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
            return true;
        }

        public bool SetSelectedReceiver(MeterReceiver receiver)
        {
            if (receiver != MeterReceiver.Rx1)
                return false;

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return false;

            container.Receiver = receiver;
            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposedSubscription)
            {
                _manager.WorkspaceChanged -= ManagerWorkspaceChanged;
                _disposedSubscription = true;
            }
            base.Dispose(disposing);
        }

        private void BuildThetisLayout()
        {
            grpMultiMeterHolder = new GroupBox();
            grpMultiMeterHolder.Location = new Point(8, 8);
            grpMultiMeterHolder.Size = new Size(710, 395);
            grpMultiMeterHolder.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(grpMultiMeterHolder);

            comboContainerSelect = new ComboBox();
            comboContainerSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            comboContainerSelect.Location = new Point(6, 13);
            comboContainerSelect.Size = new Size(193, 21);
            comboContainerSelect.SelectedIndexChanged +=
                delegate { if (!_refreshing) RefreshSelectedContainer(); };
            grpMultiMeterHolder.Controls.Add(comboContainerSelect);

            btnAddRX1Container = MakeButton("Add\r\nContainer", 209, 13, 71, 44);
            btnAddRX1Container.Click += delegate { AddContainer(MeterReceiver.Rx1); };
            grpMultiMeterHolder.Controls.Add(btnAddRX1Container);

            radContainer_rx1_data = MakeRadio("RX1 data", 290, 14);
            radContainer_rx1_data.CheckedChanged += delegate
            {
                if (!_refreshing && radContainer_rx1_data.Checked)
                    SetSelectedReceiver(MeterReceiver.Rx1);
            };
            grpMultiMeterHolder.Controls.Add(radContainer_rx1_data);

            radContainer_rx2_data = MakeRadio("RX2 data", 290, 33);
            radContainer_rx2_data.Enabled = false;
            grpMultiMeterHolder.Controls.Add(radContainer_rx2_data);

            chkContainerHighlight = MakeCheck("Highlight", 8, 40);
            chkContainerBorder = MakeCheck("Border", 8, 61);
            chkContainerNoTitle = MakeCheck("No title bar", 8, 82);
            chkContainerShowRX = MakeCheck("Show RX", 128, 40);
            chkContainerShowTX = MakeCheck("Show TX", 129, 60);
            chkMultiMeter_auto_container_height = MakeCheck("Auto height", 119, 81);
            chkLockContainer = MakeCheck("Lock", 310, 62);
            chkContainer_hidewhennotused =
                MakeCheck("Hide if RX\r\nnot in use", 286, 85);
            chkContainerMinimises = MakeCheck("Minimise", 294, 121);

            grpMultiMeterHolder.Controls.Add(chkContainerHighlight);
            grpMultiMeterHolder.Controls.Add(chkContainerBorder);
            grpMultiMeterHolder.Controls.Add(chkContainerNoTitle);
            grpMultiMeterHolder.Controls.Add(chkContainerShowRX);
            grpMultiMeterHolder.Controls.Add(chkContainerShowTX);
            grpMultiMeterHolder.Controls.Add(chkMultiMeter_auto_container_height);
            grpMultiMeterHolder.Controls.Add(chkLockContainer);
            grpMultiMeterHolder.Controls.Add(chkContainer_hidewhennotused);
            grpMultiMeterHolder.Controls.Add(chkContainerMinimises);

            chkContainerHighlight.CheckedChanged += ContainerStateChanged;
            chkContainerBorder.CheckedChanged += ContainerStateChanged;
            chkContainerNoTitle.CheckedChanged += ContainerStateChanged;
            chkContainerShowRX.CheckedChanged += ContainerStateChanged;
            chkContainerShowTX.CheckedChanged += ContainerStateChanged;
            chkMultiMeter_auto_container_height.CheckedChanged += ContainerStateChanged;
            chkLockContainer.CheckedChanged += ContainerStateChanged;
            chkContainer_hidewhennotused.CheckedChanged += ContainerStateChanged;
            chkContainerMinimises.CheckedChanged += ContainerStateChanged;

            lblMMContainerBackground = new Label();
            lblMMContainerBackground.AutoSize = true;
            lblMMContainerBackground.Location = new Point(89, 107);
            lblMMContainerBackground.Text = "Background:";
            grpMultiMeterHolder.Controls.Add(lblMMContainerBackground);

            clrbtnContainerBackground = MakeColorButton(159, 102);
            clrbtnContainerBackground.Click += delegate
            {
                if (PickColor(clrbtnContainerBackground))
                    ContainerStateChanged(clrbtnContainerBackground, EventArgs.Empty);
            };
            grpMultiMeterHolder.Controls.Add(clrbtnContainerBackground);

            lblMMContainerNotes = new Label();
            lblMMContainerNotes.AutoSize = true;
            lblMMContainerNotes.Location = new Point(7, 113);
            lblMMContainerNotes.Text = "Notes:";
            grpMultiMeterHolder.Controls.Add(lblMMContainerNotes);

            txtContainerNotes = new TextBox();
            txtContainerNotes.Location = new Point(7, 130);
            txtContainerNotes.Multiline = true;
            txtContainerNotes.Size = new Size(196, 33);
            txtContainerNotes.TextChanged += ContainerStateChanged;
            grpMultiMeterHolder.Controls.Add(txtContainerNotes);

            btnRecoverContainer = MakeButton(
                "Recover Container", 209, 63, 71, 44);
            btnRecoverContainer.Click += delegate { RecoverSelectedContainer(); };
            grpMultiMeterHolder.Controls.Add(btnRecoverContainer);

            btnContainerDelete = MakeButton(
                "Remove Container", 209, 119, 71, 44);
            btnContainerDelete.Click += delegate { RemoveSelectedContainer(); };
            grpMultiMeterHolder.Controls.Add(btnContainerDelete);

            lstMetersAvailable = new ListBox();
            lstMetersAvailable.Location = new Point(7, 174);
            lstMetersAvailable.Size = new Size(140, 212);
            lstMetersAvailable.DoubleClick +=
                delegate { AddSelectedAvailableItem(); };
            lstMetersAvailable.SelectedIndexChanged +=
                delegate { UpdateEnabledState(); };
            grpMultiMeterHolder.Controls.Add(lstMetersAvailable);

            btnAddMeterItem = MakeButton(">", 153, 174, 32, 32);
            btnAddMeterItem.Click += delegate { AddSelectedAvailableItem(); };
            grpMultiMeterHolder.Controls.Add(btnAddMeterItem);

            btnRemoveMeterItem = MakeButton("<", 153, 212, 32, 32);
            btnRemoveMeterItem.Click += delegate { RemoveSelectedItem(); };
            grpMultiMeterHolder.Controls.Add(btnRemoveMeterItem);

            lstMetersInUse = new ListBox();
            lstMetersInUse.Location = new Point(191, 174);
            lstMetersInUse.Size = new Size(140, 212);
            lstMetersInUse.DoubleClick += delegate { RemoveSelectedItem(); };
            lstMetersInUse.SelectedIndexChanged += delegate
            {
                UpdateEnabledState();
                RefreshItemSettings();
            };
            grpMultiMeterHolder.Controls.Add(lstMetersInUse);

            btnMeterUp = MakeButton("↑", 337, 174, 32, 32);
            btnMeterUp.Click += delegate { MoveSelectedItem(-1); };
            grpMultiMeterHolder.Controls.Add(btnMeterUp);

            btnMeterDown = MakeButton("↓", 336, 212, 32, 32);
            btnMeterDown.Click += delegate { MoveSelectedItem(1); };
            grpMultiMeterHolder.Controls.Add(btnMeterDown);

            btnContainer_dupe = MakeButton("D", 153, 278, 32, 32);
            btnContainer_dupe.Click += delegate { DuplicateSelectedContainer(); };
            grpMultiMeterHolder.Controls.Add(btnContainer_dupe);

            btnContainer_load = MakeButton("L", 153, 316, 32, 32);
            btnContainer_load.Click += delegate { LoadContainer(); };
            grpMultiMeterHolder.Controls.Add(btnContainer_load);

            btnContainer_save = MakeButton("S", 153, 354, 32, 32);
            btnContainer_save.Click += delegate { SaveContainer(); };
            grpMultiMeterHolder.Controls.Add(btnContainer_save);

            btnMeterCopySettings = MakeButton("C", 337, 316, 32, 32);
            btnMeterCopySettings.Click += delegate { CopySelectedItemSettings(); };
            grpMultiMeterHolder.Controls.Add(btnMeterCopySettings);

            btnMeterPasteSettings = MakeButton("P", 336, 353, 32, 32);
            btnMeterPasteSettings.Click += delegate { PasteSelectedItemSettings(); };
            grpMultiMeterHolder.Controls.Add(btnMeterPasteSettings);

            BuildThetisSettingsGroup();
        }

        private void BuildThetisSettingsGroup()
        {
            grpMeterItemSettings = new GroupBox();
            grpMeterItemSettings.Location = new Point(374, 15);
            grpMeterItemSettings.Size = new Size(323, 376);
            grpMeterItemSettings.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
            grpMeterItemSettings.Text = "Settings";
            grpMultiMeterHolder.Controls.Add(grpMeterItemSettings);

            AddSettingLabel("Update (ms):", 6, 20, 71, 16);
            nudMeterItemUpdateRate = MakeNumeric(83, 20, 56, 20, 10, 5000, 0, 10);
            grpMeterItemSettings.Controls.Add(nudMeterItemUpdateRate);

            AddSettingLabel("Attack:", 34, 47, 43, 16);
            nudMeterItemAttackRate = MakeNumeric(83, 45, 56, 20, 0, 1, 3, 0.01m);
            grpMeterItemSettings.Controls.Add(nudMeterItemAttackRate);

            AddSettingLabel("Decay:", 27, 71, 50, 16);
            nudMeterItemDecayRate = MakeNumeric(83, 70, 56, 20, 0, 1, 3, 0.01m);
            grpMeterItemSettings.Controls.Add(nudMeterItemDecayRate);

            var bgLabel = new Label();
            bgLabel.AutoSize = true;
            bgLabel.Location = new Point(184, 22);
            bgLabel.Text = "Background:";
            grpMeterItemSettings.Controls.Add(bgLabel);

            clrbtnMeterItemHBackground = MakeColorButton(263, 17);
            grpMeterItemSettings.Controls.Add(clrbtnMeterItemHBackground);

            Panel pnlMeterItemSettings = new Panel();
            pnlMeterItemSettings.Location = new Point(9, 98);
            pnlMeterItemSettings.Size = new Size(308, 273);
            pnlMeterItemSettings.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
            grpMeterItemSettings.Controls.Add(pnlMeterItemSettings);

            var lowLabel = new Label();
            lowLabel.AutoSize = true;
            lowLabel.Location = new Point(19, 9);
            lowLabel.Text = "Low:";
            pnlMeterItemSettings.Controls.Add(lowLabel);
            clrbtnMeterItemLow = MakeColorButton(55, 4);
            pnlMeterItemSettings.Controls.Add(clrbtnMeterItemLow);

            var highLabel = new Label();
            highLabel.AutoSize = true;
            highLabel.Location = new Point(114, 9);
            highLabel.Text = "High:";
            pnlMeterItemSettings.Controls.Add(highLabel);
            clrbtnMeterItemHigh = MakeColorButton(148, 4);
            pnlMeterItemSettings.Controls.Add(clrbtnMeterItemHigh);

            chkMeterItemFadeOnRx = MakeCheck("Fade on RX", 4, 57);
            chkMeterItemFadeOnTx = MakeCheck("Fade on TX", 4, 80);
            chkMeterItemShadow = MakeCheck("Shadow", 4, 103);
            chkMeterItemSegmented = MakeCheck("Segmented", 4, 126);
            chkMeterItemSolid = MakeCheck("Solid", 4, 149);
            chkMeterItemTitle = MakeCheck("Meter Title", 4, 172);
            chkMeterItemPeakValue = MakeCheck("Peak Value", 4, 195);
            chkMeterItemHistory = MakeCheck("Show History", 183, 103);
            chkMeterItemPeakHold = MakeCheck("Show Peak Hold", 183, 149);
            chkMeterItemDarkMode = MakeCheck("Dark Mode", 183, 220);

            pnlMeterItemSettings.Controls.Add(chkMeterItemFadeOnRx);
            pnlMeterItemSettings.Controls.Add(chkMeterItemFadeOnTx);
            pnlMeterItemSettings.Controls.Add(chkMeterItemShadow);
            pnlMeterItemSettings.Controls.Add(chkMeterItemSegmented);
            pnlMeterItemSettings.Controls.Add(chkMeterItemSolid);
            pnlMeterItemSettings.Controls.Add(chkMeterItemTitle);
            pnlMeterItemSettings.Controls.Add(chkMeterItemPeakValue);
            pnlMeterItemSettings.Controls.Add(chkMeterItemHistory);
            pnlMeterItemSettings.Controls.Add(chkMeterItemPeakHold);
            pnlMeterItemSettings.Controls.Add(chkMeterItemDarkMode);

            EventHandler changed = ItemSettingsChanged;
            nudMeterItemUpdateRate.ValueChanged += changed;
            nudMeterItemAttackRate.ValueChanged += changed;
            nudMeterItemDecayRate.ValueChanged += changed;
            chkMeterItemFadeOnRx.CheckedChanged += changed;
            chkMeterItemFadeOnTx.CheckedChanged += changed;
            chkMeterItemShadow.CheckedChanged += changed;
            chkMeterItemSegmented.CheckedChanged += changed;
            chkMeterItemSolid.CheckedChanged += changed;
            chkMeterItemTitle.CheckedChanged += changed;
            chkMeterItemPeakValue.CheckedChanged += changed;
            chkMeterItemHistory.CheckedChanged += changed;
            chkMeterItemPeakHold.CheckedChanged += changed;
            chkMeterItemDarkMode.CheckedChanged += changed;

            clrbtnMeterItemHBackground.Click += delegate
            {
                if (PickColor(clrbtnMeterItemHBackground))
                    ItemSettingsChanged(clrbtnMeterItemHBackground, EventArgs.Empty);
            };
            clrbtnMeterItemLow.Click += delegate
            {
                if (PickColor(clrbtnMeterItemLow))
                    ItemSettingsChanged(clrbtnMeterItemLow, EventArgs.Empty);
            };
            clrbtnMeterItemHigh.Click += delegate
            {
                if (PickColor(clrbtnMeterItemHigh))
                    ItemSettingsChanged(clrbtnMeterItemHigh, EventArgs.Empty);
            };
        }

        private void AddSettingLabel(
            string text,
            int x,
            int y,
            int width,
            int height)
        {
            var label = new Label();
            label.Location = new Point(x, y);
            label.Size = new Size(width, height);
            label.Text = text;
            grpMeterItemSettings.Controls.Add(label);
        }

        private static Button MakeButton(
            string text,
            int x,
            int y,
            int width,
            int height)
        {
            var button = new Button();
            button.Location = new Point(x, y);
            button.Size = new Size(width, height);
            button.Text = text;
            return button;
        }

        private static Button MakeColorButton(int x, int y)
        {
            var button = new Button();
            button.Location = new Point(x, y);
            button.Size = new Size(40, 23);
            button.FlatStyle = FlatStyle.Flat;
            button.Text = "";
            return button;
        }

        private static CheckBox MakeCheck(string text, int x, int y)
        {
            var check = new CheckBox();
            check.AutoSize = true;
            check.Location = new Point(x, y);
            check.Text = text;
            return check;
        }

        private static RadioButton MakeRadio(string text, int x, int y)
        {
            var radio = new RadioButton();
            radio.AutoSize = true;
            radio.Location = new Point(x, y);
            radio.Text = text;
            return radio;
        }

        private static NumericUpDown MakeNumeric(
            int x,
            int y,
            int width,
            int height,
            decimal min,
            decimal max,
            int decimals,
            decimal increment)
        {
            var nud = new NumericUpDown();
            nud.Location = new Point(x, y);
            nud.Size = new Size(width, height);
            nud.Minimum = min;
            nud.Maximum = max;
            nud.DecimalPlaces = decimals;
            nud.Increment = increment;
            return nud;
        }

        private bool PickColor(Button button)
        {
            using (var dialog = new ColorDialog())
            {
                dialog.Color = button.BackColor;
                dialog.FullOpen = true;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return false;
                button.BackColor = dialog.Color;
                return true;
            }
        }

        private void ManagerWorkspaceChanged(
            object sender,
            MeterWorkspaceChangedEventArgs e)
        {
            if (IsDisposed)
                return;

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke((Action)delegate
                {
                    RefreshFromWorkspace(SelectedContainerId);
                });
                return;
            }

            RefreshFromWorkspace(SelectedContainerId);
        }

        private void RefreshFromWorkspace(Guid? requestedSelection)
        {
            _refreshing = true;
            try
            {
                Guid? preserve = requestedSelection ?? SelectedContainerId;
                MeterWorkspaceSnapshot snapshot = _manager.Snapshot;

                comboContainerSelect.Items.Clear();
                for (int i = 0; i < snapshot.Containers.Count; i++)
                {
                    MeterContainerSnapshot container = snapshot.Containers[i];
                    comboContainerSelect.Items.Add(new ContainerChoice
                    {
                        Index = i + 1,
                        Id = container.Id,
                        Text = ContainerName(container)
                    });
                }

                int selectedIndex = -1;
                if (preserve.HasValue)
                {
                    for (int i = 0; i < comboContainerSelect.Items.Count; i++)
                    {
                        ContainerChoice choice =
                            comboContainerSelect.Items[i] as ContainerChoice;
                        if (choice != null && choice.Id == preserve.Value)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }
                }

                if (selectedIndex < 0 && comboContainerSelect.Items.Count > 0)
                    selectedIndex = 0;

                comboContainerSelect.SelectedIndex = selectedIndex;
                RefreshSelectedContainer();
            }
            finally
            {
                _refreshing = false;
            }

            UpdateEnabledState();
        }

        private void RefreshSelectedContainer()
        {
            MeterContainerSnapshot container = GetSelectedContainer();

            _refreshing = true;
            try
            {
                lstMetersInUse.Items.Clear();

                if (container == null)
                {
                    radContainer_rx1_data.Checked = false;
                    radContainer_rx2_data.Checked = false;
                    chkContainerHighlight.Checked = false;
                    chkContainerBorder.Checked = false;
                    chkContainerNoTitle.Checked = false;
                    chkContainerShowRX.Checked = false;
                    chkContainerShowTX.Checked = false;
                    chkMultiMeter_auto_container_height.Checked = false;
                    chkLockContainer.Checked = false;
                    chkContainer_hidewhennotused.Checked = false;
                    chkContainerMinimises.Checked = false;
                    txtContainerNotes.Text = "";
                    clrbtnContainerBackground.BackColor = SystemColors.Control;
                    RefreshItemSettings();
                    return;
                }

                radContainer_rx1_data.Checked =
                    container.Receiver == MeterReceiver.Rx1;
                radContainer_rx2_data.Checked =
                    container.Receiver == MeterReceiver.Rx2;
                chkContainerHighlight.Checked = container.Highlight;
                chkContainerBorder.Checked = container.Border;
                chkContainerNoTitle.Checked = container.NoTitleBar;
                chkContainerShowRX.Checked = container.VisibleOnReceive;
                chkContainerShowTX.Checked = container.VisibleOnTransmit;
                chkMultiMeter_auto_container_height.Checked = container.AutoHeight;
                chkLockContainer.Checked = container.Locked;
                chkContainer_hidewhennotused.Checked =
                    container.HideWhenReceiverNotUsed;
                chkContainerMinimises.Checked = container.Minimises;
                clrbtnContainerBackground.BackColor =
                    Color.FromArgb(container.BackgroundArgb);
                txtContainerNotes.Text = container.Notes ?? "";

                for (int i = 0; i < container.Items.Count; i++)
                {
                    MeterItemSnapshot item = container.Items[i];
                    lstMetersInUse.Items.Add(new MeterTypeChoice
                    {
                        Type = item.Type,
                        Text = DisplayName(item.Type)
                    });
                }

                if (lstMetersInUse.Items.Count > 0)
                    lstMetersInUse.SelectedIndex = 0;

                RefreshItemSettings();
            }
            finally
            {
                _refreshing = false;
            }

            UpdateEnabledState();
        }

        private void ContainerStateChanged(object sender, EventArgs e)
        {
            if (_refreshing)
                return;

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return;

            container.Highlight = chkContainerHighlight.Checked;
            container.Border = chkContainerBorder.Checked;
            container.NoTitleBar = chkContainerNoTitle.Checked;
            container.VisibleOnReceive = chkContainerShowRX.Checked;
            container.VisibleOnTransmit = chkContainerShowTX.Checked;
            container.AutoHeight = chkMultiMeter_auto_container_height.Checked;
            container.Locked = chkLockContainer.Checked;
            container.HideWhenReceiverNotUsed =
                chkContainer_hidewhennotused.Checked;
            container.Minimises = chkContainerMinimises.Checked;
            container.BackgroundArgb = clrbtnContainerBackground.BackColor.ToArgb();
            container.Notes = txtContainerNotes.Text ?? "";

            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
        }

        private void RefreshItemSettings()
        {
            MeterItemSnapshot item = GetSelectedItem();
            bool enabled = item != null;

            grpMeterItemSettings.Enabled = enabled;
            if (!enabled)
                return;

            ThetisMeterItemSettings settings =
                ThetisMeterItemSettings.FromItem(item);

            _refreshing = true;
            try
            {
                nudMeterItemUpdateRate.Value =
                    ClampDecimal(settings.UpdateIntervalMs,
                        nudMeterItemUpdateRate.Minimum,
                        nudMeterItemUpdateRate.Maximum);
                nudMeterItemAttackRate.Value =
                    ClampDecimal((decimal)settings.Attack,
                        nudMeterItemAttackRate.Minimum,
                        nudMeterItemAttackRate.Maximum);
                nudMeterItemDecayRate.Value =
                    ClampDecimal((decimal)settings.Decay,
                        nudMeterItemDecayRate.Minimum,
                        nudMeterItemDecayRate.Maximum);

                chkMeterItemFadeOnRx.Checked = settings.FadeOnRx;
                chkMeterItemFadeOnTx.Checked = settings.FadeOnTx;
                chkMeterItemShadow.Checked = settings.Shadow;
                chkMeterItemSegmented.Checked = settings.Segmented;
                chkMeterItemSolid.Checked = settings.Solid;
                chkMeterItemTitle.Checked = settings.ShowType;
                chkMeterItemPeakValue.Checked = settings.PeakValue;
                chkMeterItemHistory.Checked = settings.ShowHistory;
                chkMeterItemPeakHold.Checked = settings.PeakHold;
                chkMeterItemDarkMode.Checked = settings.DarkMode;
                clrbtnMeterItemHBackground.BackColor =
                    settings.BackgroundColor;
                clrbtnMeterItemLow.BackColor = settings.LowColor;
                clrbtnMeterItemHigh.BackColor = settings.HighColor;
            }
            finally
            {
                _refreshing = false;
            }
        }

        private void ItemSettingsChanged(object sender, EventArgs e)
        {
            if (_refreshing)
                return;

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null || container.Locked)
                return;

            int index = lstMetersInUse.SelectedIndex;
            if (index < 0 || index >= container.Items.Count)
                return;

            MeterItemSnapshot item = container.Items[index];
            ThetisMeterItemSettings settings =
                ThetisMeterItemSettings.FromItem(item);

            settings.UpdateIntervalMs = (int)nudMeterItemUpdateRate.Value;
            settings.Attack = (double)nudMeterItemAttackRate.Value;
            settings.Decay = (double)nudMeterItemDecayRate.Value;
            settings.FadeOnRx = chkMeterItemFadeOnRx.Checked;
            settings.FadeOnTx = chkMeterItemFadeOnTx.Checked;
            settings.Shadow = chkMeterItemShadow.Checked;
            settings.Segmented = chkMeterItemSegmented.Checked;
            settings.Solid = chkMeterItemSolid.Checked;
            settings.ShowType = chkMeterItemTitle.Checked;
            settings.PeakValue = chkMeterItemPeakValue.Checked;
            settings.ShowHistory = chkMeterItemHistory.Checked;
            settings.PeakHold = chkMeterItemPeakHold.Checked;
            settings.DarkMode = chkMeterItemDarkMode.Checked;
            settings.BackgroundColor =
                clrbtnMeterItemHBackground.BackColor;
            settings.LowColor = clrbtnMeterItemLow.BackColor;
            settings.HighColor = clrbtnMeterItemHigh.BackColor;
            settings.ApplyTo(item);

            _manager.ReplaceContainer(container);
        }

        private static decimal ClampDecimal(
            decimal value,
            decimal min,
            decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void AddSelectedAvailableItem()
        {
            MeterTypeChoice choice =
                lstMetersAvailable.SelectedItem as MeterTypeChoice;
            if (choice != null)
                AddItemToSelectedContainer(choice.Type);
        }

        private MeterContainerSnapshot GetSelectedContainer()
        {
            Guid? id = SelectedContainerId;
            if (!id.HasValue)
                return null;

            MeterWorkspaceSnapshot snapshot = _manager.Snapshot;
            for (int i = 0; i < snapshot.Containers.Count; i++)
            {
                if (snapshot.Containers[i].Id == id.Value)
                    return snapshot.Containers[i];
            }
            return null;
        }

        private MeterItemSnapshot GetSelectedItem()
        {
            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return null;

            int index = lstMetersInUse.SelectedIndex;
            if (index < 0 || index >= container.Items.Count)
                return null;

            return container.Items[index];
        }

        private void RecoverSelectedContainer()
        {
            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null || container.Locked)
                return;

            container.Geometry.X = 100;
            container.Geometry.Y = 100;
            container.VisibleOnReceive = true;
            container.VisibleOnTransmit = true;
            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
        }

        private void DuplicateSelectedContainer()
        {
            if (_manager.ContainerCount >= MaxContainers)
                return;

            MeterContainerSnapshot source = GetSelectedContainer();
            if (source == null)
                return;

            MeterContainerSnapshot copy =
                MeterWorkspaceCopy.CloneContainer(source);
            copy.Id = Guid.NewGuid();
            copy.Notes = String.IsNullOrWhiteSpace(copy.Notes)
                ? "Container copy"
                : copy.Notes + " copy";
            copy.Geometry.X += 24;
            copy.Geometry.Y += 24;

            for (int i = 0; i < copy.Items.Count; i++)
                copy.Items[i].Id = Guid.NewGuid();

            _manager.AddContainer(copy);
            RefreshFromWorkspace(copy.Id);
        }

        private void SaveContainer()
        {
            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return;

            using (var dialog = new SaveFileDialog())
            {
                dialog.InitialDirectory =
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments);
                dialog.Filter = "Container Files|*.dat";
                dialog.Title = "Save Container";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog(this) != DialogResult.OK ||
                    String.IsNullOrEmpty(dialog.FileName))
                    return;

                File.WriteAllText(
                    dialog.FileName,
                    SerializeContainer(container),
                    Encoding.UTF8);
            }
        }

        private void LoadContainer()
        {
            if (_manager.ContainerCount >= MaxContainers)
                return;

            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory =
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments);
                dialog.Filter = "Container Files|*.dat";
                dialog.Title = "Load Container";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog(this) != DialogResult.OK ||
                    String.IsNullOrEmpty(dialog.FileName) ||
                    !File.Exists(dialog.FileName))
                    return;

                MeterContainerSnapshot container =
                    DeserializeContainer(
                        File.ReadAllText(dialog.FileName, Encoding.UTF8));

                container.Id = Guid.NewGuid();
                for (int i = 0; i < container.Items.Count; i++)
                    container.Items[i].Id = Guid.NewGuid();

                _manager.AddContainer(container);
                RefreshFromWorkspace(container.Id);
            }
        }

        private static string SerializeContainer(
            MeterContainerSnapshot container)
        {
            var workspace = new MeterWorkspaceSnapshot();
            workspace.Containers.Add(
                MeterWorkspaceCopy.CloneContainer(container));

            var ds = new DataSet("Data");
            var table = new DataTable("Options");
            table.Columns.Add("Key", typeof(string));
            table.Columns.Add("Value", typeof(string));
            ds.Tables.Add(table);

            var store = new ThetisOptionsMeterStore(table);
            store.ReplaceAll(workspace);

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);
                return Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(writer.ToString()));
            }
        }

        private static MeterContainerSnapshot DeserializeContainer(
            string data64)
        {
            try
            {
                string xml = Encoding.UTF8.GetString(
                    Convert.FromBase64String(data64.Trim()));
                var ds = new DataSet("Data");
                using (var reader = new StringReader(xml))
                    ds.ReadXml(reader, XmlReadMode.ReadSchema);

                if (!ds.Tables.Contains("Options"))
                    throw new InvalidDataException(
                        "Container file has no Options table.");

                var store =
                    new ThetisOptionsMeterStore(ds.Tables["Options"]);
                MeterWorkspaceSnapshot workspace = store.Load();
                if (workspace.Containers.Count != 1)
                    throw new InvalidDataException(
                        "Container file must contain exactly one container.");

                return MeterWorkspaceCopy.CloneContainer(
                    workspace.Containers[0]);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    "Container file is not valid Meters/Gadgets data.",
                    ex);
            }
        }

        private void CopySelectedItemSettings()
        {
            MeterItemSnapshot item = GetSelectedItem();
            if (item == null)
                return;

            _copiedSettings =
                new Dictionary<string, string>(
                    item.Settings,
                    StringComparer.Ordinal);
            UpdateEnabledState();
        }

        private void PasteSelectedItemSettings()
        {
            if (_copiedSettings == null)
                return;

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null || container.Locked)
                return;

            int index = lstMetersInUse.SelectedIndex;
            if (index < 0 || index >= container.Items.Count)
                return;

            MeterItemSnapshot item = container.Items[index];
            item.Settings.Clear();
            foreach (KeyValuePair<string, string> pair in _copiedSettings)
                item.Settings.Add(pair.Key, pair.Value);

            _manager.ReplaceContainer(container);
            RefreshItemSettings();
        }

        private void UpdateEnabledState()
        {
            MeterContainerSnapshot container = GetSelectedContainer();
            bool hasContainer = container != null;
            bool locked = hasContainer && container.Locked;
            bool hasItem =
                hasContainer &&
                lstMetersInUse.SelectedIndex >= 0 &&
                lstMetersInUse.SelectedIndex < container.Items.Count;

            btnAddRX1Container.Enabled =
                _manager.ContainerCount < MaxContainers;
            btnContainerDelete.Enabled = hasContainer && !locked;
            btnRecoverContainer.Enabled = hasContainer && !locked;
            btnContainer_dupe.Enabled =
                hasContainer && _manager.ContainerCount < MaxContainers;
            btnContainer_save.Enabled = hasContainer;
            btnContainer_load.Enabled =
                _manager.ContainerCount < MaxContainers;

            comboContainerSelect.Enabled = hasContainer;
            chkContainerHighlight.Enabled = hasContainer;
            chkContainerBorder.Enabled = hasContainer;
            chkContainerNoTitle.Enabled = hasContainer;
            chkContainerShowRX.Enabled = hasContainer;
            chkContainerShowTX.Enabled = hasContainer;
            chkMultiMeter_auto_container_height.Enabled = hasContainer;
            chkLockContainer.Enabled = hasContainer;
            chkContainer_hidewhennotused.Enabled = hasContainer;
            chkContainerMinimises.Enabled = hasContainer;
            clrbtnContainerBackground.Enabled = hasContainer;
            txtContainerNotes.Enabled = hasContainer;
            radContainer_rx1_data.Enabled = hasContainer;
            radContainer_rx2_data.Enabled = false;

            lstMetersAvailable.Enabled = hasContainer;
            lstMetersInUse.Enabled = hasContainer;
            btnAddMeterItem.Enabled =
                hasContainer && !locked &&
                lstMetersAvailable.SelectedIndex >= 0;
            btnRemoveMeterItem.Enabled = hasItem && !locked;
            btnMeterUp.Enabled =
                hasItem && !locked && lstMetersInUse.SelectedIndex > 0;
            btnMeterDown.Enabled =
                hasItem && !locked &&
                lstMetersInUse.SelectedIndex < lstMetersInUse.Items.Count - 1;
            btnMeterCopySettings.Enabled = hasItem;
            btnMeterPasteSettings.Enabled =
                hasItem && !locked && _copiedSettings != null;
            grpMeterItemSettings.Enabled = hasItem && !locked;
        }

        private static string ContainerName(
            MeterContainerSnapshot container)
        {
            string notes = (container.Notes ?? "").Trim();
            int newline = notes.IndexOf('\n');
            if (newline >= 0)
                notes = notes.Substring(0, newline);
            if (notes.Length > 40)
                notes = notes.Substring(0, 40);
            return String.IsNullOrEmpty(notes) ? "Container" : notes;
        }

        private static string DisplayName(string itemType)
        {
            MeterItemDescriptor descriptor;
            return MeterItemCatalog.TryGet(itemType, out descriptor)
                ? descriptor.DisplayName
                : itemType;
        }

        private static MeterTypeChoice[] BuildSupportedTypes()
        {
            MeterItemDescriptor[] descriptors = MeterItemCatalog.All;
            var result = new MeterTypeChoice[descriptors.Length];

            for (int i = 0; i < descriptors.Length; i++)
            {
                result[i] = new MeterTypeChoice
                {
                    Type = descriptors[i].Type,
                    Text = descriptors[i].DisplayName
                };
            }

            return result;
        }
    }
}
