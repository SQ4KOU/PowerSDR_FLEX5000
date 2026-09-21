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
                    Mox = _console.MOX,
                    Tune = _console.TUN,
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
                        "RX2 telemetry is not implemented yet.");

                if (_console.CurrentModel != Model.FLEX5000)
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "The current radio is not a FLEX-5000.");

                if (!_console.PowerOn)
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "FlexMeters telemetry is unavailable while the radio is powered off.");

                if (reading == FlexMeters.MeterReading.SignalStrength)
                {
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

                if (!IsFlexMetersTxReading(reading))
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "This FLEX-5000 meter reading is not implemented yet.");

                if (!_console.MOX && !_console.TUN)
                    return FlexMeters.MeterReadingResult.Unsupported(
                        "TX meter reading is unavailable while the radio is not transmitting.");

                switch (reading)
                {
                    case FlexMeters.MeterReading.Mic:
                        return ReadTxDsp(
                            DttSP.MeterType.MIC,
                            FlexMeters.ThetisTxMeterMath.Mic);

                    case FlexMeters.MeterReading.MicPeak:
                        return ReadTxDsp(
                            DttSP.MeterType.MIC_PK,
                            FlexMeters.ThetisTxMeterMath.Mic);

                    case FlexMeters.MeterReading.Eq:
                        return ReadTxDsp(
                            DttSP.MeterType.EQ,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.EqPeak:
                        return ReadTxDsp(
                            DttSP.MeterType.EQ_PK,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.Leveler:
                        return ReadTxDsp(
                            DttSP.MeterType.LEVELER,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.LevelerPeak:
                        return ReadTxDsp(
                            DttSP.MeterType.LEVELER_PK,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.LevelerGain:
                        return ReadTxDsp(
                            DttSP.MeterType.LVL_G,
                            FlexMeters.ThetisTxMeterMath.LevelerGain);

                    case FlexMeters.MeterReading.Compressor:
                        return ReadTxDsp(
                            DttSP.MeterType.COMP,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.CompressorPeak:
                        return ReadTxDsp(
                            DttSP.MeterType.COMP_PK,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.Alc:
                        return ReadTxDsp(
                            DttSP.MeterType.ALC,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.AlcPeak:
                        return ReadTxDsp(
                            DttSP.MeterType.ALC_PK,
                            FlexMeters.ThetisTxMeterMath.Stage);

                    case FlexMeters.MeterReading.AlcGain:
                        return ReadTxDsp(
                            DttSP.MeterType.ALC_G,
                            FlexMeters.ThetisTxMeterMath.AlcGain);

                    case FlexMeters.MeterReading.AlcGroup:
                    {
                        float alcPeak = DttSP.CalculateTXMeter(
                            1,
                            DttSP.MeterType.ALC_PK);
                        float alcGain = DttSP.CalculateTXMeter(
                            1,
                            DttSP.MeterType.ALC_G);

                        return FlexMeters.MeterReadingResult.Supported(
                            FlexMeters.ThetisTxMeterMath.AlcGroup(
                                alcPeak,
                                alcGain));
                    }

                    case FlexMeters.MeterReading.ForwardPower:
                        return FlexMeters.MeterReadingResult.Supported(
                            _console.FWCPAPower(_console.pa_fwd_power));

                    case FlexMeters.MeterReading.ReversePower:
                        return FlexMeters.MeterReadingResult.Supported(
                            _console.FWCPAPower(_console.pa_rev_power) *
                            _console.swr_table[(int)_console.TXBand]);

                    case FlexMeters.MeterReading.Swr:
                        return FlexMeters.MeterReadingResult.Supported(
                            _console.FWCSWR(
                                _console.pa_fwd_power,
                                _console.pa_rev_power));

                    default:
                        return FlexMeters.MeterReadingResult.Unsupported(
                            "This FLEX-5000 TX meter reading is not implemented yet.");
                }
            }

            private delegate double TxMeterTransform(double raw);

            private FlexMeters.MeterReadingResult ReadTxDsp(
                DttSP.MeterType meterType,
                TxMeterTransform transform)
            {
                float raw = DttSP.CalculateTXMeter(1, meterType);
                return FlexMeters.MeterReadingResult.Supported(transform(raw));
            }

            private static bool IsFlexMetersTxReading(
                FlexMeters.MeterReading reading)
            {
                switch (reading)
                {
                    case FlexMeters.MeterReading.Mic:
                    case FlexMeters.MeterReading.MicPeak:
                    case FlexMeters.MeterReading.Eq:
                    case FlexMeters.MeterReading.EqPeak:
                    case FlexMeters.MeterReading.Leveler:
                    case FlexMeters.MeterReading.LevelerPeak:
                    case FlexMeters.MeterReading.LevelerGain:
                    case FlexMeters.MeterReading.Compressor:
                    case FlexMeters.MeterReading.CompressorPeak:
                    case FlexMeters.MeterReading.Alc:
                    case FlexMeters.MeterReading.AlcPeak:
                    case FlexMeters.MeterReading.AlcGain:
                    case FlexMeters.MeterReading.AlcGroup:
                    case FlexMeters.MeterReading.ForwardPower:
                    case FlexMeters.MeterReading.ReversePower:
                    case FlexMeters.MeterReading.Swr:
                        return true;

                    default:
                        return false;
                }
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

        internal FlexMeters.MeterWorkspaceManager EnsureFlexMetersWorkspaceManager()
        {
            if (flexMetersWorkspaceHost == null ||
                flexMetersContainerManager == null)
                InitializeFlexMetersWorkspaceRuntime();

            return flexMetersContainerManager;
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
