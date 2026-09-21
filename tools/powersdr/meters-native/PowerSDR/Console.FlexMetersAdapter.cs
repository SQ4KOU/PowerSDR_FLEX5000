namespace PowerSDR
{
    sealed unsafe public partial class Console
    {
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
    }
}
