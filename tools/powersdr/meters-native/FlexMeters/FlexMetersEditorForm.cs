using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlexMeters
{
    public sealed class FlexMetersEditorForm : Form
    {
        private const int MaxContainers = 50;

        private sealed class ContainerChoice
        {
            public int Index;
            public Guid Id;
            public MeterReceiver Receiver;

            public override string ToString()
            {
                return Index.ToString() + " - Container [" + Receiver.ToString() + "]";
            }
        }

        private sealed class MeterTypeChoice
        {
            public string Type;
            public string Text;

            public override string ToString()
            {
                return Text;
            }
        }

        private static readonly MeterTypeChoice[] SupportedTypes =
        {
            new MeterTypeChoice { Type = "SIGNAL_STRENGTH", Text = "Signal Peak" },
            new MeterTypeChoice { Type = "SIGNAL_TEXT", Text = "Signal Text" }
        };

        private readonly MeterWorkspaceManager _manager;
        private readonly ComboBox _comboContainer;
        private readonly Button _btnAddContainer;
        private readonly Button _btnRemoveContainer;
        private readonly RadioButton _radRx1;
        private readonly RadioButton _radRx2;
        private readonly CheckBox _chkShowRx;
        private readonly CheckBox _chkShowTx;
        private readonly ListBox _listAvailable;
        private readonly ListBox _listInUse;
        private readonly Button _btnAddItem;
        private readonly Button _btnRemoveItem;
        private readonly Button _btnUp;
        private readonly Button _btnDown;
        private readonly Label _status;
        private readonly Label _itemStatus;

        private bool _refreshing;
        private bool _disposedSubscription;

        public FlexMetersEditorForm(MeterWorkspaceManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _manager = manager;

            Text = "Meters/Gadgets";
            ClientSize = new Size(724, 410);
            MinimumSize = new Size(740, 449);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            AutoScaleMode = AutoScaleMode.Dpi;

            var holder = new GroupBox();
            holder.Location = new Point(8, 8);
            holder.Size = new Size(710, 395);
            holder.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(holder);

            _comboContainer = new ComboBox();
            _comboContainer.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboContainer.Location = new Point(6, 13);
            _comboContainer.Size = new Size(193, 21);
            _comboContainer.SelectedIndexChanged += ComboContainerSelectedIndexChanged;
            holder.Controls.Add(_comboContainer);

            _btnAddContainer = new Button();
            _btnAddContainer.Location = new Point(209, 13);
            _btnAddContainer.Size = new Size(71, 44);
            _btnAddContainer.Text = "Add\r\nContainer";
            _btnAddContainer.Click += delegate { AddContainer(MeterReceiver.Rx1); };
            holder.Controls.Add(_btnAddContainer);

            _radRx1 = new RadioButton();
            _radRx1.AutoSize = true;
            _radRx1.Location = new Point(290, 14);
            _radRx1.Text = "RX1 data";
            _radRx1.CheckedChanged += ReceiverChanged;
            holder.Controls.Add(_radRx1);

            _radRx2 = new RadioButton();
            _radRx2.AutoSize = true;
            _radRx2.Location = new Point(290, 33);
            _radRx2.Text = "RX2 data";
            _radRx2.Enabled = false;
            holder.Controls.Add(_radRx2);

            var rx2Note = new Label();
            rx2Note.AutoSize = true;
            rx2Note.Location = new Point(374, 35);
            rx2Note.Text = "native RX2 telemetry pending";
            rx2Note.ForeColor = SystemColors.GrayText;
            holder.Controls.Add(rx2Note);

            _chkShowRx = new CheckBox();
            _chkShowRx.AutoSize = true;
            _chkShowRx.Location = new Point(128, 40);
            _chkShowRx.Text = "Show RX";
            _chkShowRx.CheckedChanged += VisibilityChanged;
            holder.Controls.Add(_chkShowRx);

            _chkShowTx = new CheckBox();
            _chkShowTx.AutoSize = true;
            _chkShowTx.Location = new Point(129, 60);
            _chkShowTx.Text = "Show TX";
            _chkShowTx.CheckedChanged += VisibilityChanged;
            holder.Controls.Add(_chkShowTx);

            _btnRemoveContainer = new Button();
            _btnRemoveContainer.Location = new Point(209, 119);
            _btnRemoveContainer.Size = new Size(71, 44);
            _btnRemoveContainer.Text = "Remove\r\nContainer";
            _btnRemoveContainer.Click += delegate { RemoveSelectedContainer(); };
            holder.Controls.Add(_btnRemoveContainer);

            _status = new Label();
            _status.Location = new Point(7, 84);
            _status.Size = new Size(192, 40);
            _status.TextAlign = ContentAlignment.MiddleLeft;
            holder.Controls.Add(_status);

            _listAvailable = new ListBox();
            _listAvailable.Location = new Point(7, 174);
            _listAvailable.Size = new Size(140, 212);
            _listAvailable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            _listAvailable.DoubleClick += delegate { AddSelectedAvailableItem(); };
            _listAvailable.SelectedIndexChanged += delegate { UpdateEnabledState(); };
            holder.Controls.Add(_listAvailable);

            _btnAddItem = new Button();
            _btnAddItem.Location = new Point(153, 174);
            _btnAddItem.Size = new Size(32, 32);
            _btnAddItem.Text = ">";
            _btnAddItem.Click += delegate { AddSelectedAvailableItem(); };
            holder.Controls.Add(_btnAddItem);

            _btnRemoveItem = new Button();
            _btnRemoveItem.Location = new Point(153, 212);
            _btnRemoveItem.Size = new Size(32, 32);
            _btnRemoveItem.Text = "<";
            _btnRemoveItem.Click += delegate { RemoveSelectedItem(); };
            holder.Controls.Add(_btnRemoveItem);

            _listInUse = new ListBox();
            _listInUse.Location = new Point(191, 174);
            _listInUse.Size = new Size(140, 212);
            _listInUse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            _listInUse.DoubleClick += delegate { RemoveSelectedItem(); };
            _listInUse.SelectedIndexChanged += delegate
            {
                UpdateEnabledState();
                UpdateItemStatus();
            };
            holder.Controls.Add(_listInUse);

            _btnUp = new Button();
            _btnUp.Location = new Point(337, 174);
            _btnUp.Size = new Size(32, 32);
            _btnUp.Text = "Up";
            _btnUp.Click += delegate { MoveSelectedItem(-1); };
            holder.Controls.Add(_btnUp);

            _btnDown = new Button();
            _btnDown.Location = new Point(337, 212);
            _btnDown.Size = new Size(32, 32);
            _btnDown.Text = "Dn";
            _btnDown.Click += delegate { MoveSelectedItem(1); };
            holder.Controls.Add(_btnDown);

            var rendererGroup = new GroupBox();
            rendererGroup.Location = new Point(375, 73);
            rendererGroup.Size = new Size(327, 90);
            rendererGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rendererGroup.Text = "Renderer";
            holder.Controls.Add(rendererGroup);

            var rendererInfo = new Label();
            rendererInfo.Dock = DockStyle.Fill;
            rendererInfo.Padding = new Padding(8);
            rendererInfo.Text =
                "Native Thetis signal renderer\r\n" +
                "FLEX-5000 RX1 live telemetry\r\n" +
                "100 ms live update";
            rendererGroup.Controls.Add(rendererInfo);

            var itemGroup = new GroupBox();
            itemGroup.Location = new Point(375, 174);
            itemGroup.Size = new Size(327, 212);
            itemGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemGroup.Text = "Meter item";
            holder.Controls.Add(itemGroup);

            _itemStatus = new Label();
            _itemStatus.Dock = DockStyle.Fill;
            _itemStatus.Padding = new Padding(10);
            _itemStatus.TextAlign = ContentAlignment.TopLeft;
            itemGroup.Controls.Add(_itemStatus);

            for (int i = 0; i < SupportedTypes.Length; i++)
                _listAvailable.Items.Add(SupportedTypes[i]);

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
                ContainerChoice choice = _comboContainer.SelectedItem as ContainerChoice;
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
            for (int i = 0; i < _comboContainer.Items.Count; i++)
            {
                ContainerChoice choice = _comboContainer.Items[i] as ContainerChoice;
                if (choice != null && choice.Id == containerId)
                {
                    _comboContainer.SelectedIndex = i;
                    return true;
                }
            }

            return false;
        }

        public bool SelectItem(int index)
        {
            if (index < 0 || index >= _listInUse.Items.Count)
                return false;

            _listInUse.SelectedIndex = index;
            return true;
        }

        public bool AddItemToSelectedContainer(string itemType)
        {
            if (!IsSupportedItemType(itemType))
                return false;

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return false;

            container.Items.Add(new MeterItemSnapshot
            {
                Id = Guid.NewGuid(),
                Type = itemType
            });

            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
            _listInUse.SelectedIndex = _listInUse.Items.Count - 1;
            return true;
        }

        public bool RemoveSelectedItem()
        {
            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return false;

            int index = _listInUse.SelectedIndex;
            if (index < 0 || index >= container.Items.Count)
                return false;

            container.Items.RemoveAt(index);
            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);

            if (_listInUse.Items.Count > 0)
                _listInUse.SelectedIndex = Math.Min(index, _listInUse.Items.Count - 1);

            return true;
        }

        public bool MoveSelectedItem(int delta)
        {
            if (delta != -1 && delta != 1)
                throw new ArgumentOutOfRangeException("delta");

            MeterContainerSnapshot container = GetSelectedContainer();
            if (container == null)
                return false;

            int index = _listInUse.SelectedIndex;
            int target = index + delta;
            if (index < 0 || target < 0 || target >= container.Items.Count)
                return false;

            MeterItemSnapshot item = container.Items[index];
            container.Items.RemoveAt(index);
            container.Items.Insert(target, item);
            _manager.ReplaceContainer(container);
            RefreshFromWorkspace(container.Id);
            _listInUse.SelectedIndex = target;
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

        private void ComboContainerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_refreshing)
                return;

            RefreshSelectedContainer();
        }

        private void ReceiverChanged(object sender, EventArgs e)
        {
            if (_refreshing || !_radRx1.Checked)
                return;

            SetSelectedReceiver(MeterReceiver.Rx1);
        }

        private void VisibilityChanged(object sender, EventArgs e)
        {
            if (_refreshing)
                return;

            SetSelectedContainerVisibility(
                _chkShowRx.Checked,
                _chkShowTx.Checked);
        }

        private void AddSelectedAvailableItem()
        {
            MeterTypeChoice choice = _listAvailable.SelectedItem as MeterTypeChoice;
            if (choice == null)
                return;

            AddItemToSelectedContainer(choice.Type);
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

                _comboContainer.Items.Clear();
                for (int i = 0; i < snapshot.Containers.Count; i++)
                {
                    MeterContainerSnapshot container = snapshot.Containers[i];
                    _comboContainer.Items.Add(new ContainerChoice
                    {
                        Index = i + 1,
                        Id = container.Id,
                        Receiver = container.Receiver
                    });
                }

                int selectedIndex = -1;
                if (preserve.HasValue)
                {
                    for (int i = 0; i < _comboContainer.Items.Count; i++)
                    {
                        ContainerChoice choice = _comboContainer.Items[i] as ContainerChoice;
                        if (choice != null && choice.Id == preserve.Value)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }
                }

                if (selectedIndex < 0 && _comboContainer.Items.Count > 0)
                    selectedIndex = 0;

                _comboContainer.SelectedIndex = selectedIndex;
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

            _listInUse.Items.Clear();

            if (container == null)
            {
                _radRx1.Checked = false;
                _radRx2.Checked = false;
                _chkShowRx.Checked = false;
                _chkShowTx.Checked = false;
                _status.Text = "No containers";
                _itemStatus.Text = "Select or add a container.";
                UpdateEnabledState();
                return;
            }

            _radRx1.Checked = container.Receiver == MeterReceiver.Rx1;
            _radRx2.Checked = container.Receiver == MeterReceiver.Rx2;
            _chkShowRx.Checked = container.VisibleOnReceive;
            _chkShowTx.Checked = container.VisibleOnTransmit;

            for (int i = 0; i < container.Items.Count; i++)
            {
                MeterItemSnapshot item = container.Items[i];
                _listInUse.Items.Add(new MeterTypeChoice
                {
                    Type = item.Type,
                    Text = DisplayName(item.Type)
                });
            }

            _status.Text =
                "Containers: " + _manager.ContainerCount.ToString() +
                "\r\nItems: " + container.Items.Count.ToString();

            UpdateItemStatus();
            UpdateEnabledState();
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

        private void UpdateItemStatus()
        {
            MeterTypeChoice choice = _listInUse.SelectedItem as MeterTypeChoice;
            if (choice == null)
            {
                _itemStatus.Text =
                    "Available native item types:\r\n" +
                    "Signal Peak - Thetis horizontal S-meter\r\n" +
                    "Signal Text - S unit / dBm / uV";
                return;
            }

            _itemStatus.Text =
                choice.Text + "\r\n\r\n" +
                "Renderer: native Thetis port\r\n" +
                "Telemetry: FLEX-5000 RX1\r\n" +
                "Persistence: authoritative replace-all store";
        }

        private void UpdateEnabledState()
        {
            bool hasContainer = SelectedContainerId.HasValue;
            bool hasItem = _listInUse.SelectedIndex >= 0;

            _btnAddContainer.Enabled = _manager.ContainerCount < MaxContainers;
            _btnRemoveContainer.Enabled = hasContainer;
            _radRx1.Enabled = hasContainer;
            _radRx2.Enabled = false;
            _chkShowRx.Enabled = hasContainer;
            _chkShowTx.Enabled = hasContainer;
            _listAvailable.Enabled = hasContainer;
            _listInUse.Enabled = hasContainer;
            _btnAddItem.Enabled = hasContainer && _listAvailable.SelectedIndex >= 0;
            _btnRemoveItem.Enabled = hasContainer && hasItem;
            _btnUp.Enabled = hasItem && _listInUse.SelectedIndex > 0;
            _btnDown.Enabled =
                hasItem &&
                _listInUse.SelectedIndex < _listInUse.Items.Count - 1;
        }

        private static bool IsSupportedItemType(string itemType)
        {
            for (int i = 0; i < SupportedTypes.Length; i++)
            {
                if (String.Equals(
                    SupportedTypes[i].Type,
                    itemType,
                    StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static string DisplayName(string itemType)
        {
            for (int i = 0; i < SupportedTypes.Length; i++)
            {
                if (String.Equals(
                    SupportedTypes[i].Type,
                    itemType,
                    StringComparison.Ordinal))
                    return SupportedTypes[i].Text;
            }

            return itemType;
        }
    }
}
