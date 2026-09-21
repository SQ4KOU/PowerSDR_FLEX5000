namespace FlexMeters
{
    public static class Flex5000Rx2SignalCalibration
    {
        public static double Apply(
            double rawSignal,
            double meterCalOffset,
            double preampOffset,
            double filterSizeCalOffset,
            double pathOffset,
            double xvtrGainOffset,
            bool loopEnabled,
            double loopGain)
        {
            // KE9NS RX2 uses the same additive calibration structure as RX1.
            // Reuse the proven helper instead of maintaining a second formula.
            return Flex5000Rx1SignalCalibration.Apply(
                rawSignal,
                meterCalOffset,
                preampOffset,
                filterSizeCalOffset,
                pathOffset,
                xvtrGainOffset,
                loopEnabled,
                loopGain);
        }
    }
}
