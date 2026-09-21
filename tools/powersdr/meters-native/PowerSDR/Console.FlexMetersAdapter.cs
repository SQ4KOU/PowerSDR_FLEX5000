namespace PowerSDR
{
    sealed unsafe public partial class Console
    {
        private const string FlexMetersDatabaseTableName = "FlexMeters";
        private static readonly System.TimeSpan FlexMetersLiveInterval =
            System.TimeSpan.FromMilliseconds(100.0);

        private FlexMeters.DataTableMeterStore flexMetersStore;
        private FlexMeters.MeterWorkspaceRuntimeHost flexMetersWorkspaceHost;
        private FlexMeters.MeterWorkspaceManager flexMetersContainerManager;
        private FlexMeters.WinFormsMeterWindowHost flexMetersWindowHost;

        private sealed class FlexMetersRadioStateAdapter : FlexMeters.IMeterRadioState
        {
            private readonly Console _console;

            public FlexMetersRadioStateAdapter(Console console)
            {
                _console = console;
            }

            public FlexMeters.MeterRadioStateSnapshot CaptureState()
            {
                return new FlexMeters.MeterRadioStateSnapshot
                {
                    VfoAHertz = (long)System.Math.Round(_console.VFOAFreq * 1000000.0),
                    VfoBHertz = (long)System.Math.Round(_console.VFOBFreq * 1000000.0),
                    Rx2Enabled = _console.RX2Enabled
                };
            }
        }

        private sealed class FlexMetersTelemetryAdapter : FlexMeters.IMeterTelemetrySource
        {
            private readonly Console _console;

            public FlexMetersTelemetryAdapter(Console console)
            {
                _console = console;
            }

            public FlexMeters.MeterReadingResult Read(
                FlexMeters.MeterReading reading,
                FlexMeters.MeterReceiver receiver)
            {
                if (receiver != FlexMeters.MeterReceiver.Rx1)
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "RX2 telemetry is not implemented in the RX1 contract stage.");

                if (reading != FlexMeters.MeterReading.SignalStrength)
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "Only FLEX-5000 RX1 SIGNAL_STRENGTH is implemented in this stage.");

                if (_console.CurrentModel != Model.FLEX5000)
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "The current radio is not a FLEX-5000.");

                if (!_console.PowerOn)
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "RX1 SIGNAL_STRENGTH is unavailable while the radio is powered off.");

                float raw = DttSP.CalculateRXMeter(
                    0,
                    0,
                    DttSP.MeterType.SIGNAL_STRENGTH);

                double calibrated = FlexMeters.Flex5000Rx1SignalCalibration.Apply(
                    raw,
                    _console.MultiMeterCalOffset,
                    Display.RX1PreampOffset,
                    _console.RX1FilterSizeCalOffset,
                    _console.RX1PathOffset,
                    _console.RX1XVTRGainOffset,
                    _console.RX1Loop,
                    _console.LoopGain);

                return FlexMeters.MeterReadingResult.Supported(calibrated);
            }
        }

        internal FlexMeters.IMeterTelemetrySource CreateFlexMetersTelemetrySource()
        {
            return new FlexMetersTelemetryAdapter(this);
        }

        internal FlexMeters.IMeterRadioState CreateFlexMetersRadioState()
        {
            return new FlexMetersRadioStateAdapter(this);
        }

        internal FlexMeters.MeterLiveRuntime CreateFlexMetersLiveRuntime(
            FlexMeters.MeterWorkspaceSnapshot workspace)
        {
            return new FlexMeters.MeterLiveRuntime(
                CreateFlexMetersTelemetrySource(),
                workspace);
        }

        private void InitializeFlexMetersWorkspaceRuntime()
        {
            if (flexMetersWorkspaceHost != null)
                return;

            if (DB.ds == null)
                throw new System.InvalidOperationException(
                    "FlexMeters workspace cannot start before DB.Init.");

            System.Data.DataTable table;
            if (DB.ds.Tables.Contains(FlexMetersDatabaseTableName))
            {
                table = DB.ds.Tables[FlexMetersDatabaseTableName];
            }
            else
            {
                table = new System.Data.DataTable(FlexMetersDatabaseTableName);
                DB.ds.Tables.Add(table);
            }

            flexMetersStore = new FlexMeters.DataTableMeterStore(table);
            flexMetersWorkspaceHost = new FlexMeters.MeterWorkspaceRuntimeHost(
                flexMetersStore,
                CreateFlexMetersTelemetrySource());
            flexMetersWorkspaceHost.Start(FlexMetersLiveInterval);
            flexMetersContainerManager = new FlexMeters.MeterWorkspaceManager(
                flexMetersWorkspaceHost,
                delegate { DB.Update(); });

            this.Shown += FlexMetersConsoleShown;
        }

        private void FlexMetersConsoleShown(object sender, System.EventArgs e)
        {
            this.Shown -= FlexMetersConsoleShown;

            if (flexMetersContainerManager == null || flexMetersWindowHost != null)
                return;

            flexMetersWindowHost = new FlexMeters.WinFormsMeterWindowHost(
                this,
                flexMetersContainerManager,
                flexMetersWorkspaceHost,
                CreateFlexMetersRadioState(),
                true);
            flexMetersWindowHost.RestoreWindows(
                flexMetersContainerManager.Snapshot);
        }

        internal void ReplaceFlexMetersWorkspace(
            FlexMeters.MeterWorkspaceSnapshot workspace)
        {
            if (workspace == null)
                throw new System.ArgumentNullException("workspace");

            if (flexMetersWorkspaceHost == null)
                InitializeFlexMetersWorkspaceRuntime();

            flexMetersContainerManager.ReplaceAll(workspace);
        }

        internal void ReloadFlexMetersWorkspaceRuntime()
        {
            if (flexMetersWorkspaceHost == null)
                InitializeFlexMetersWorkspaceRuntime();
            else
                flexMetersContainerManager.ReloadFromStore();
        }

        internal FlexMeters.MeterWorkspaceRuntimeHost FlexMetersWorkspaceHost
        {
            get { return flexMetersWorkspaceHost; }
        }

        internal FlexMeters.MeterWorkspaceManager FlexMetersContainerManager
        {
            get { return flexMetersContainerManager; }
        }

        internal FlexMeters.WinFormsMeterWindowHost FlexMetersWindowHost
        {
            get { return flexMetersWindowHost; }
        }

        internal void AddFlexMetersContainer(FlexMeters.MeterContainerSnapshot container)
        {
            if (flexMetersContainerManager == null)
                InitializeFlexMetersWorkspaceRuntime();

            flexMetersContainerManager.AddContainer(container);
        }

        internal bool RemoveFlexMetersContainer(System.Guid containerId)
        {
            if (flexMetersContainerManager == null)
                InitializeFlexMetersWorkspaceRuntime();

            return flexMetersContainerManager.RemoveContainer(containerId);
        }

        internal void ReplaceFlexMetersContainer(FlexMeters.MeterContainerSnapshot container)
        {
            if (flexMetersContainerManager == null)
                InitializeFlexMetersWorkspaceRuntime();

            flexMetersContainerManager.ReplaceContainer(container);
        }

        private void ShutdownFlexMetersWorkspaceRuntime()
        {
            this.Shown -= FlexMetersConsoleShown;

            FlexMeters.WinFormsMeterWindowHost windowHost = flexMetersWindowHost;
            FlexMeters.MeterWorkspaceRuntimeHost host = flexMetersWorkspaceHost;

            flexMetersWindowHost = null;
            flexMetersContainerManager = null;
            flexMetersWorkspaceHost = null;
            flexMetersStore = null;

            if (windowHost != null)
                windowHost.Dispose();

            if (host != null)
                host.Dispose();
        }
    }
}
