using System;

namespace FlexMeters
{
    // TX meter normalization follows the pinned Thetis meter semantics documented
    // in meters-native/EVIDENCE_AUDIT.md. Raw values are supplied by native KE9NS DttSP.
    public static class ThetisTxMeterMath
    {
        public static double Mic(double raw)
        {
            return Math.Max(-195.0, -raw);
        }

        public static double Stage(double raw)
        {
            return Math.Max(-30.0, -raw);
        }

        public static double LevelerGain(double raw)
        {
            return Math.Max(0.0, raw);
        }

        public static double AlcGain(double raw)
        {
            return Math.Max(0.0, -raw);
        }

        public static double AlcGroup(double alcPeakRaw, double alcGainRaw)
        {
            return Stage(alcPeakRaw) + AlcGain(alcGainRaw);
        }
    }
}
