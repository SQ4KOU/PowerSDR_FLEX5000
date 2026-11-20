using System;

namespace PowerSDR
{
    // P30: minimal FLEX-5000 TX telemetry adapter for native Thetis MeterManager.
    // Sources/formulas are the same native PowerSDR paths used by its own TX meters.
    sealed unsafe public partial class Console
    {
        internal float P30ReadTxAverage(DttSP.MeterType meter)
        {
            if (!PowerOn || !MOX) return -30.0f;
            try { return (float)Math.Max(-30.0f, -DttSP.CalculateTXMeter(1, meter) + 3.0f); }
            catch { return -30.0f; }
        }

        internal float P30ReadTxPeak(DttSP.MeterType meter)
        {
            if (!PowerOn || !MOX) return -30.0f;
            try { return (float)Math.Max(-30.0f, -DttSP.CalculateTXMeter(1, meter)); }
            catch { return -30.0f; }
        }

        internal float P30ReadAlcGain()
        {
            if (!PowerOn || !MOX) return 0.0f;
            try { return (float)Math.Max(0.0f, -DttSP.CalculateTXMeter(1, DttSP.MeterType.ALC_G)); }
            catch { return 0.0f; }
        }

        internal float P30ReadLevelerGain()
        {
            if (!PowerOn || !MOX) return 0.0f;
            try { return (float)Math.Max(0.0f, DttSP.CalculateTXMeter(1, DttSP.MeterType.LVL_G)); }
            catch { return 0.0f; }
        }

        internal float P30ReadTxForwardWatts()
        {
            if (!PowerOn || !MOX) return 0.0f;
            try
            {
                // FLEX-5000 native PowerSDR updates pa_fwd_power from PA ADC channel 7.
                return (float)Math.Max(0.0, FWCPAPower(pa_fwd_power));
            }
            catch { return 0.0f; }
        }

        internal float P30ReadTxReverseWatts()
        {
            if (!PowerOn || !MOX) return 0.0f;
            try
            {
                // Exact native FLEX-5000 reverse-power calibration path.
                double p = FWCPAPower(pa_rev_power) * swr_table[(int)tx_band];
                return (float)Math.Max(0.0, p);
            }
            catch { return 0.0f; }
        }

        internal float P30ReadTxSWR()
        {
            if (!PowerOn || !MOX) return 1.0f;
            try
            {
                double swr = FWCSWR(pa_fwd_power, pa_rev_power);
                if (swr >= 19.0)
                {
                    for (int q = 0; q < 5; q++)
                    {
                        System.Threading.Thread.Sleep(10);
                        swr = FWCSWR(pa_fwd_power, pa_rev_power);
                        if (swr < 19.0) break;
                    }
                }

                if (Double.IsNaN(swr) || Double.IsInfinity(swr)) return 1.0f;
                if (swr < 1.0 && swr > -1.0) swr = 1.0;
                return (float)swr;
            }
            catch { return 1.0f; }
        }

        internal float P30ReadPaVolts()
        {
            if (!PowerOn) return 0.0f;
            try
            {
                // Native PowerSDR PA-voltage ADC value (channel 2), maintained by its own worker.
                return ((float)Volts_Value / 4096.0f) * 2.5f * 11.0f;
            }
            catch { return 0.0f; }
        }
    }
}
