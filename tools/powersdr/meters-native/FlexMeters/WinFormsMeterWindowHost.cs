using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlexMeters
{
    public sealed class WinFormsMeterWindowHost : IMeterWindowHost, IDisposable
    {
        private sealed class MeterContainerForm : Form
        {
            private readonly TableLayoutPanel _itemsPanel;
            private readonly Dictionary<Guid, Control> _itemControls =
                new Dictionary<Guid, Control>();
            private readonly Dictionary<Guid, string> _itemTypes =
                new Dictionary<Guid, string>();
            private readonly Dictionary<Guid, ThetisSignalMeterControl> _renderers =
                new Dictionary<Guid, ThetisSignalMeterControl>();
            private readonly Dictionary<Guid, FlexTxMeterControl> _txRenderers =
                new Dictionary<Guid, FlexTxMeterControl>();

            public MeterContainerForm(MeterContainerSnapshot container)
            {
                ContainerId = container.Id;
                StartPosition = FormStartPosition.Manual;
                MinimumSize = new Size(220, 110);
                ShowInTaskbar = false;

                _itemsPanel = new TableLayoutPanel();
                _itemsPanel.Dock = DockStyle.Fill;
                _itemsPanel.AutoScroll = true;
                _itemsPanel.ColumnCount = 1;
                _itemsPanel.RowCount = 0;
                _itemsPanel.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                Controls.Add(_itemsPanel);

                UpdateDefinition(container);
            }

            public Guid ContainerId { get; private set; }

            public void UpdateDefinition(MeterContainerSnapshot container)
            {
                ApplyContainerAppearance(container);

                if (DefinitionMatches(container))
                {
                    ApplyRendererSettings(container);
                    return;
                }

                _itemsPanel.SuspendLayout();
                try
                {
                    _itemsPanel.Controls.Clear();
                    _itemsPanel.RowStyles.Clear();
                    _itemControls.Clear();
                    _itemTypes.Clear();
                    _renderers.Clear();
                    _txRenderers.Clear();

                    if (container.Items.Count == 0)
                    {
                        var empty = CreateFallbackLabel("(no meter items)");
                        _itemsPanel.Controls.Add(empty, 0, 0);
                        return;
                    }

                    for (int i = 0; i < container.Items.Count; i++)
                    {
                        MeterItemSnapshot item = container.Items[i];
                        Control control = CreateItemControl(item);
                        _itemControls.Add(item.Id, control);
                        _itemTypes.Add(item.Id, item.Type);
                        _itemsPanel.Controls.Add(control, 0, i);
                    }
                }
                finally
                {
                    _itemsPanel.ResumeLayout(true);
                }
            }

            public void UpdateLive(
                MeterContainerSnapshot container,
                MeterLiveSnapshot live,
                bool aboveS9Frequency)
            {
                for (int i = 0; i < container.Items.Count; i++)
                {
                    MeterItemSnapshot item = container.Items[i];
                    MeterLiveValue value = null;
                    bool hasValue = live != null && live.TryGetValue(item.Id, out value);

                    ThetisSignalMeterControl signalRenderer;
                    if (_renderers.TryGetValue(item.Id, out signalRenderer))
                    {
                        signalRenderer.UpdateReading(
                            hasValue
                                ? value.Result
                                : MeterReadingResult.Unsupported("No live reading is available."),
                            aboveS9Frequency);
                        continue;
                    }

                    FlexTxMeterControl txRenderer;
                    if (_txRenderers.TryGetValue(item.Id, out txRenderer))
                    {
                        txRenderer.UpdateReading(
                            hasValue
                                ? value.Result
                                : MeterReadingResult.Unsupported("No live reading is available."));
                    }
                }
            }

            public bool TryGetItemText(Guid itemId, out string text)
            {
                ThetisSignalMeterControl renderer;
                if (_renderers.TryGetValue(itemId, out renderer))
                {
                    text = renderer.DiagnosticText;
                    return true;
                }

                FlexTxMeterControl txRenderer;
                if (_txRenderers.TryGetValue(itemId, out txRenderer))
                {
                    text = txRenderer.DiagnosticText;
                    return true;
                }

                Control control;
                if (_itemControls.TryGetValue(itemId, out control))
                {
                    text = control.Text;
                    return true;
                }

                text = null;
                return false;
            }

            public bool TryGetRendererKind(Guid itemId, out string kind)
            {
                ThetisSignalMeterControl renderer;
                if (_renderers.TryGetValue(itemId, out renderer))
                {
                    kind = renderer.RendererKind;
                    return true;
                }

                FlexTxMeterControl txRenderer;
                if (_txRenderers.TryGetValue(itemId, out txRenderer))
                {
                    kind = txRenderer.RendererKind;
                    return true;
                }

                kind = null;
                return false;
            }

            public bool TryGetNormalizedPosition(Guid itemId, out double position)
            {
                ThetisSignalMeterControl renderer;
                if (_renderers.TryGetValue(itemId, out renderer))
                {
                    position = renderer.NormalizedPosition;
                    return true;
                }

                FlexTxMeterControl txRenderer;
                if (_txRenderers.TryGetValue(itemId, out txRenderer))
                {
                    position = txRenderer.NormalizedPosition;
                    return true;
                }

                position = Double.NaN;
                return false;
            }

            public bool TryGetAboveS9Frequency(Guid itemId, out bool above)
            {
                ThetisSignalMeterControl renderer;
                if (_renderers.TryGetValue(itemId, out renderer))
                {
                    above = renderer.AboveS9Frequency;
                    return true;
                }

                above = false;
                return false;
            }

            private bool DefinitionMatches(MeterContainerSnapshot container)
            {
                if (_itemTypes.Count != container.Items.Count)
                    return false;

                for (int i = 0; i < container.Items.Count; i++)
                {
                    MeterItemSnapshot item = container.Items[i];
                    string type;
                    if (!_itemTypes.TryGetValue(item.Id, out type))
                        return false;
                    if (!String.Equals(type, item.Type, StringComparison.Ordinal))
                        return false;
                }

                return true;
            }

            private void ApplyContainerAppearance(MeterContainerSnapshot container)
            {
                Text = BuildTitle(container);
                BackColor = Color.FromArgb(container.BackgroundArgb);
                _itemsPanel.BackColor = BackColor;

                if (container.NoTitleBar)
                    FormBorderStyle = FormBorderStyle.None;
                else if (container.Locked)
                    FormBorderStyle = FormBorderStyle.FixedSingle;
                else
                    FormBorderStyle = FormBorderStyle.Sizable;

                if (container.Highlight)
                {
                    _itemsPanel.Padding = new Padding(2);
                    _itemsPanel.CellBorderStyle =
                        TableLayoutPanelCellBorderStyle.Single;
                }
                else
                {
                    _itemsPanel.Padding = Padding.Empty;
                    _itemsPanel.CellBorderStyle =
                        container.Border
                        ? TableLayoutPanelCellBorderStyle.Single
                        : TableLayoutPanelCellBorderStyle.None;
                }
            }

            private void ApplyRendererSettings(MeterContainerSnapshot container)
            {
                for (int i = 0; i < container.Items.Count; i++)
                {
                    MeterItemSnapshot item = container.Items[i];

                    ThetisSignalMeterControl signalRenderer;
                    if (_renderers.TryGetValue(item.Id, out signalRenderer))
                        signalRenderer.ApplySettings(item);

                    FlexTxMeterControl txRenderer;
                    if (_txRenderers.TryGetValue(item.Id, out txRenderer))
                        txRenderer.ApplySettings(item);
                }
            }

            private Control CreateItemControl(MeterItemSnapshot item)
            {
                MeterItemDescriptor descriptor;
                if (!MeterItemCatalog.TryGet(item.Type, out descriptor))
                    return CreateFallbackLabel(item.Type + ": renderer not implemented");

                if (descriptor.RendererKind == MeterItemRendererKind.SignalBar ||
                    descriptor.RendererKind == MeterItemRendererKind.SignalText)
                {
                    var renderer = new ThetisSignalMeterControl(item);
                    _renderers.Add(item.Id, renderer);
                    return renderer;
                }

                if (descriptor.RendererKind == MeterItemRendererKind.Linear)
                {
                    var renderer = new FlexTxMeterControl(descriptor, item);
                    _txRenderers.Add(item.Id, renderer);
                    return renderer;
                }

                return CreateFallbackLabel(item.Type + ": renderer not implemented");
            }

            private static Label CreateFallbackLabel(string text)
            {
                var label = new Label();
                label.Dock = DockStyle.Top;
                label.AutoSize = true;
                label.Padding = new Padding(8, 6, 8, 6);
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Text = text;
                return label;
            }

            private static string BuildTitle(MeterContainerSnapshot container)
            {
                return "FlexMeters " + container.Receiver + " [" +
                    container.Id.ToString("D").Substring(0, 8) + "]";
            }
        }

        private readonly object _sync = new object();
        private readonly Form _owner;
        private readonly MeterWorkspaceManager _manager;
        private readonly MeterWorkspaceRuntimeHost _runtimeHost;
        private readonly IMeterRadioState _radioState;
        private readonly bool _showWindows;
        private readonly Dictionary<Guid, MeterContainerForm> _windows =
            new Dictionary<Guid, MeterContainerForm>();

        private bool _suppressWindowEvents;
        private bool _disposed;

        public WinFormsMeterWindowHost(
            Form owner,
            MeterWorkspaceManager manager,
            MeterWorkspaceRuntimeHost runtimeHost)
            : this(owner, manager, runtimeHost, null, true)
        {
        }

        public WinFormsMeterWindowHost(
            Form owner,
            MeterWorkspaceManager manager,
            MeterWorkspaceRuntimeHost runtimeHost,
            bool showWindows)
            : this(owner, manager, runtimeHost, null, showWindows)
        {
        }

        public WinFormsMeterWindowHost(
            Form owner,
            MeterWorkspaceManager manager,
            MeterWorkspaceRuntimeHost runtimeHost,
            IMeterRadioState radioState,
            bool showWindows)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (runtimeHost == null)
                throw new ArgumentNullException("runtimeHost");
            if (!runtimeHost.IsStarted)
                throw new InvalidOperationException(
                    "WinForms meter windows require a started runtime host.");

            _owner = owner;
            _manager = manager;
            _runtimeHost = runtimeHost;
            _radioState = radioState;
            _showWindows = showWindows;

            _manager.WorkspaceChanged += ManagerWorkspaceChanged;
            _runtimeHost.Updated += RuntimeUpdated;
        }

        public int OpenWindowCount
        {
            get
            {
                lock (_sync)
                    return _windows.Count;
            }
        }

        public void RestoreWindows(MeterWorkspaceSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");

            MeterWorkspaceValidator.Validate(snapshot);
            MeterLiveSnapshot live = _runtimeHost.Current;
            ExecuteOnUi(delegate
            {
                ReconcileWindows(snapshot);
                ApplyLiveSnapshot(snapshot, live);
            });
        }

        public void AddWindow(MeterContainerSnapshot container)
        {
            ThrowIfDisposed();
            _manager.AddContainer(container);
        }

        public bool RemoveWindow(Guid containerId)
        {
            ThrowIfDisposed();
            return _manager.RemoveContainer(containerId);
        }

        public bool CloseWindow(Guid containerId)
        {
            ThrowIfDisposed();

            MeterContainerForm form = null;
            lock (_sync)
            {
                if (!_windows.TryGetValue(containerId, out form))
                    return false;
            }

            ExecuteOnUi(delegate { form.Close(); });
            return true;
        }

        public bool SetWindowGeometry(Guid containerId, MeterWindowGeometry geometry)
        {
            ThrowIfDisposed();
            ValidateGeometry(geometry);

            MeterContainerForm form;
            lock (_sync)
            {
                if (!_windows.TryGetValue(containerId, out form))
                    return false;
            }

            ExecuteOnUi(delegate
            {
                _suppressWindowEvents = true;
                try
                {
                    ApplyGeometry(form, geometry);
                }
                finally
                {
                    _suppressWindowEvents = false;
                }
            });

            MeterWorkspaceSnapshot snapshot = _manager.Snapshot;
            MeterContainerSnapshot container = FindContainer(snapshot, containerId);
            if (container == null)
                return false;

            container.Geometry = CloneGeometry(geometry);
            _manager.ReplaceContainer(container);
            return true;
        }

        public bool TryCaptureGeometry(
            Guid containerId,
            out MeterWindowGeometry geometry)
        {
            ThrowIfDisposed();

            MeterContainerForm form;
            lock (_sync)
            {
                if (!_windows.TryGetValue(containerId, out form))
                {
                    geometry = null;
                    return false;
                }
            }

            MeterWindowGeometry captured = null;
            ExecuteOnUi(delegate { captured = CaptureGeometry(form); });
            geometry = captured;
            return captured != null;
        }

        public bool TryGetDisplayedItemText(Guid itemId, out string text)
        {
            ThrowIfDisposed();
            string captured = null;
            bool found = false;

            ExecuteOnUi(delegate
            {
                MeterContainerForm[] forms = SnapshotForms();
                for (int i = 0; i < forms.Length; i++)
                {
                    if (forms[i].TryGetItemText(itemId, out captured))
                    {
                        found = true;
                        return;
                    }
                }
            });

            text = captured;
            return found;
        }

        public bool TryGetRendererKind(Guid itemId, out string kind)
        {
            ThrowIfDisposed();
            string captured = null;
            bool found = false;

            ExecuteOnUi(delegate
            {
                MeterContainerForm[] forms = SnapshotForms();
                for (int i = 0; i < forms.Length; i++)
                {
                    if (forms[i].TryGetRendererKind(itemId, out captured))
                    {
                        found = true;
                        return;
                    }
                }
            });

            kind = captured;
            return found;
        }

        public bool TryGetRenderedNormalizedPosition(Guid itemId, out double position)
        {
            ThrowIfDisposed();
            double captured = Double.NaN;
            bool found = false;

            ExecuteOnUi(delegate
            {
                MeterContainerForm[] forms = SnapshotForms();
                for (int i = 0; i < forms.Length; i++)
                {
                    if (forms[i].TryGetNormalizedPosition(itemId, out captured))
                    {
                        found = true;
                        return;
                    }
                }
            });

            position = captured;
            return found;
        }

        public bool TryGetAboveS9Frequency(Guid itemId, out bool above)
        {
            ThrowIfDisposed();
            bool captured = false;
            bool found = false;

            ExecuteOnUi(delegate
            {
                MeterContainerForm[] forms = SnapshotForms();
                for (int i = 0; i < forms.Length; i++)
                {
                    if (forms[i].TryGetAboveS9Frequency(itemId, out captured))
                    {
                        found = true;
                        return;
                    }
                }
            });

            above = captured;
            return found;
        }

        public void CloseAll()
        {
            if (_disposed)
                return;

            ExecuteOnUi(delegate
            {
                MeterContainerForm[] forms;
                lock (_sync)
                {
                    forms = new MeterContainerForm[_windows.Count];
                    _windows.Values.CopyTo(forms, 0);
                    _windows.Clear();
                }

                _suppressWindowEvents = true;
                try
                {
                    for (int i = 0; i < forms.Length; i++)
                    {
                        forms[i].FormClosing -= WindowFormClosing;
                        forms[i].ResizeEnd -= WindowResizeEnd;
                        forms[i].Close();
                        forms[i].Dispose();
                    }
                }
                finally
                {
                    _suppressWindowEvents = false;
                }
            });
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _runtimeHost.Updated -= RuntimeUpdated;
            _manager.WorkspaceChanged -= ManagerWorkspaceChanged;
            CloseAll();
            _disposed = true;
        }

        private void ManagerWorkspaceChanged(
            object sender,
            MeterWorkspaceChangedEventArgs e)
        {
            if (_disposed)
                return;

            MeterWorkspaceSnapshot snapshot = e.Workspace;
            MeterLiveSnapshot live = _runtimeHost.Current;
            ExecuteOnUi(delegate
            {
                ReconcileWindows(snapshot);
                ApplyLiveSnapshot(snapshot, live);
            });
        }

        private void RuntimeUpdated(
            object sender,
            MeterLiveSnapshotEventArgs e)
        {
            if (_disposed)
                return;

            MeterWorkspaceSnapshot workspace = _manager.Snapshot;
            MeterLiveSnapshot live = e.Snapshot;
            ExecuteOnUi(delegate { ApplyLiveSnapshot(workspace, live); });
        }

        private void ReconcileWindows(MeterWorkspaceSnapshot snapshot)
        {
            ThrowIfDisposed();

            var wanted = new Dictionary<Guid, MeterContainerSnapshot>();
            for (int i = 0; i < snapshot.Containers.Count; i++)
                wanted.Add(snapshot.Containers[i].Id, snapshot.Containers[i]);

            var close = new List<MeterContainerForm>();
            lock (_sync)
            {
                foreach (KeyValuePair<Guid, MeterContainerForm> pair in _windows)
                {
                    if (!wanted.ContainsKey(pair.Key))
                        close.Add(pair.Value);
                }

                for (int i = 0; i < close.Count; i++)
                    _windows.Remove(close[i].ContainerId);
            }

            _suppressWindowEvents = true;
            try
            {
                for (int i = 0; i < close.Count; i++)
                {
                    close[i].FormClosing -= WindowFormClosing;
                    close[i].ResizeEnd -= WindowResizeEnd;
                    close[i].Close();
                    close[i].Dispose();
                }

                for (int i = 0; i < snapshot.Containers.Count; i++)
                {
                    MeterContainerSnapshot container = snapshot.Containers[i];
                    MeterContainerForm form;

                    lock (_sync)
                        _windows.TryGetValue(container.Id, out form);

                    if (form == null)
                    {
                        form = CreateWindow(container);
                        lock (_sync)
                            _windows.Add(container.Id, form);

                        // Visibility is applied from live radio state in ApplyLiveSnapshot.
                        // Do not show here; this avoids an RX/TX visibility flash.
                    }
                    else
                    {
                        form.UpdateDefinition(container);
                        ApplyGeometry(form, container.Geometry);
                    }
                }
            }
            finally
            {
                _suppressWindowEvents = false;
            }
        }

        private void ApplyLiveSnapshot(
            MeterWorkspaceSnapshot workspace,
            MeterLiveSnapshot live)
        {
            if (workspace == null)
                return;

            MeterRadioStateSnapshot radio = _radioState == null
                ? null
                : _radioState.CaptureState();

            for (int i = 0; i < workspace.Containers.Count; i++)
            {
                MeterContainerSnapshot container = workspace.Containers[i];
                MeterContainerForm form;

                lock (_sync)
                    _windows.TryGetValue(container.Id, out form);

                if (form == null)
                    continue;

                bool above = false;
                if (radio != null)
                {
                    if (container.Receiver == MeterReceiver.Rx1)
                    {
                        above =
                            radio.VfoAHertz >=
                            ThetisSignalMeterMath.S9FrequencyThresholdHertz;
                    }
                    else if (container.Receiver == MeterReceiver.Rx2)
                    {
                        above =
                            radio.Rx2Enabled &&
                            radio.VfoBHertz >=
                            ThetisSignalMeterMath.S9FrequencyThresholdHertz;
                    }
                }

                if (_showWindows)
                {
                    bool shouldShow = MeterWindowVisibility.ShouldShow(
                        container,
                        radio);

                    if (shouldShow && !form.Visible)
                    {
                        if (_owner != null)
                            form.Show(_owner);
                        else
                            form.Show();
                    }
                    else if (!shouldShow && form.Visible)
                    {
                        form.Hide();
                    }
                }

                form.UpdateLive(container, live, above);
            }
        }

        private MeterContainerForm[] SnapshotForms()
        {
            lock (_sync)
            {
                var forms = new MeterContainerForm[_windows.Count];
                _windows.Values.CopyTo(forms, 0);
                return forms;
            }
        }

        private MeterContainerForm CreateWindow(MeterContainerSnapshot container)
        {
            var form = new MeterContainerForm(container);
            ApplyGeometry(form, container.Geometry);
            form.FormClosing += WindowFormClosing;
            form.ResizeEnd += WindowResizeEnd;
            return form;
        }

        private void WindowFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_suppressWindowEvents || _disposed)
                return;

            var form = sender as MeterContainerForm;
            if (form == null)
                return;

            // Thetis frmMeterDisplay semantics: clicking X hides a meter
            // container; it does NOT delete the container definition.  Removal
            // is an explicit Setup -> Remove Container operation.
            if (e.CloseReason == CloseReason.UserClosing)
            {
                MeterWorkspaceSnapshot snapshot = _manager.Snapshot;
                MeterContainerSnapshot container =
                    FindContainer(snapshot, form.ContainerId);
                if (container != null)
                {
                    container.Geometry = CaptureGeometry(form);
                    _manager.ReplaceContainer(container);
                }

                form.Hide();
                e.Cancel = true;
                return;
            }

            // Application/owner shutdown must never mutate the authoritative
            // container list.  Geometry is already persisted on ResizeEnd.
        }

        private void WindowResizeEnd(object sender, EventArgs e)
        {
            if (_suppressWindowEvents || _disposed)
                return;

            var form = sender as MeterContainerForm;
            if (form == null)
                return;

            MeterWorkspaceSnapshot snapshot = _manager.Snapshot;
            MeterContainerSnapshot container = FindContainer(
                snapshot,
                form.ContainerId);

            if (container == null)
                return;

            container.Geometry = CaptureGeometry(form);
            _manager.ReplaceContainer(container);
        }

        private static MeterContainerSnapshot FindContainer(
            MeterWorkspaceSnapshot snapshot,
            Guid containerId)
        {
            for (int i = 0; i < snapshot.Containers.Count; i++)
            {
                if (snapshot.Containers[i].Id == containerId)
                    return snapshot.Containers[i];
            }

            return null;
        }

        private static void ApplyGeometry(
            Form form,
            MeterWindowGeometry geometry)
        {
            ValidateGeometry(geometry);

            form.WindowState = FormWindowState.Normal;
            form.Bounds = new Rectangle(
                geometry.X,
                geometry.Y,
                geometry.Width,
                geometry.Height);

            if (geometry.Maximized)
                form.WindowState = FormWindowState.Maximized;
        }

        private static MeterWindowGeometry CaptureGeometry(Form form)
        {
            Rectangle bounds =
                form.WindowState == FormWindowState.Normal
                ? form.Bounds
                : form.RestoreBounds;

            if (bounds.Width <= 0 || bounds.Height <= 0)
                bounds = form.Bounds;

            return new MeterWindowGeometry
            {
                X = bounds.X,
                Y = bounds.Y,
                Width = bounds.Width,
                Height = bounds.Height,
                Maximized = form.WindowState == FormWindowState.Maximized
            };
        }

        private static MeterWindowGeometry CloneGeometry(
            MeterWindowGeometry geometry)
        {
            return new MeterWindowGeometry
            {
                X = geometry.X,
                Y = geometry.Y,
                Width = geometry.Width,
                Height = geometry.Height,
                Maximized = geometry.Maximized
            };
        }

        private static void ValidateGeometry(MeterWindowGeometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");
            if (geometry.Width <= 0 || geometry.Height <= 0)
                throw new ArgumentOutOfRangeException(
                    "geometry",
                    "Meter window width and height must be positive.");
        }

        private void ExecuteOnUi(Action action)
        {
            ThrowIfDisposed();

            if (_owner != null && _owner.InvokeRequired)
                _owner.Invoke(action);
            else
                action();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("WinFormsMeterWindowHost");
        }
    }
}
