namespace FlexMeters
{
    public static class Flex5000Rx1SignalCalibration
    {
        public static double Apply(
            double rawDspSignal,
            double meterCalibrationOffset,
            double preampOffset,
            double filterSizeCalibrationOffset,
            double pathOffset,
            double xvtrGainOffset,
            bool rx1LoopEnabled,
            double loopGain)
        {
            double value =
                rawDspSignal +
                meterCalibrationOffset +
                preampOffset +
                filterSizeCalibrationOffset +
                pathOffset +
                xvtrGainOffset;

            if (rx1LoopEnabled)
                value += loopGain;

            return value;
        }
    }
}
