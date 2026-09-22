using System;

namespace PowerSDR
{
    // P30: FLEX-5000 TX telemetry adapter for the native Thetis MeterManager.
    // RX telemetry remains in P25ThetisMetersBridge and is intentionally untouched.
    sealed unsafe public partial class Console
    {
        private readonly object p30TxRfLock = new object();
        private int p30TxRfTick;
        private bool p30TxRfValid;
        private int p30TxFwdAdc;
        private int p30TxRevAdc;
        private int p30TxVoltsAdc;

        internal float P30ReadTxDsp(DttSP.MeterType meter)
        {
            if (!PowerOn || !MOX) return -200.0f;

            try
            {
                // PowerSDR's native TX DSP is thread 1 (DSPTX(1)).
                // Legacy DttSP exposes meter magnitudes as positive attenuation;
                // Thetis TX meter scales expect signed dB (-30..+12) or
                // positive gain-reduction values, hence the native PowerSDR sign inversion.
                return -DttSP.CalculateTXMeter(1, meter);
            }
            catch
            {
                return -200.0f;
            }
        }

        private bool P30RefreshTxRfSnapshot()
        {
            if (!PowerOn || !MOX) return false;

            lock (p30TxRfLock)
            {
                int now = Environment.TickCount;
                int elapsed = unchecked(now - p30TxRfTick);
                if (p30TxRfValid && elapsed >= 0 && elapsed < 75)
                    return true;

                int fwd = 0;
                int rev = 0;
                int volts = 0;

                try
                {
                    int rf = FWC.ReadPAADC(5, out fwd);
                    int rr = FWC.ReadPAADC(4, out rev);
                    int rv = FWC.ReadPAADC(2, out volts);

                    p30TxRfTick = now;
                    p30TxRfValid = (rf == 0 && rr == 0);

                    if (p30TxRfValid)
                    {
                        p30TxFwdAdc = fwd;
                        p30TxRevAdc = rev;
                        if (rv == 0) p30TxVoltsAdc = volts;

                        // Keep native PowerSDR PA ADC state coherent with other consumers.
                        pa_fwd_power = fwd;
                        pa_rev_power = rev;
                    }
                }
                catch
                {
                    p30TxRfTick = now;
                    p30TxRfValid = false;
                }

                return p30TxRfValid;
            }
        }

        internal float P30ReadTxForwardWatts()
        {
            if (!P30RefreshTxRfSnapshot()) return 0.0f;
            try { return (float)Math.Max(0.0, FWCPAPower(p30TxFwdAdc)); }
            catch { return 0.0f; }
        }

        internal float P30ReadTxReverseWatts()
        {
            if (!P30RefreshTxRfSnapshot()) return 0.0f;
            try
            {
                double p = FWCPAPower(p30TxRevAdc);
                try { p *= atu_swr_table[(int)TXBand]; } catch { }
                return (float)Math.Max(0.0, p);
            }
            catch { return 0.0f; }
        }

        internal float P30ReadTxSWR()
        {
            if (!P30RefreshTxRfSnapshot()) return 1.0f;
            try
            {
                double swr = FWCSWR(p30TxFwdAdc, p30TxRevAdc);
                if (Double.IsNaN(swr) || Double.IsInfinity(swr) || swr < 1.0) return 1.0f;
                return (float)Math.Min(99.0, swr);
            }
            catch { return 1.0f; }
        }

        internal float P30ReadPaVolts()
        {
            if (!P30RefreshTxRfSnapshot()) return 0.0f;
            try
            {
                return (float)((double)p30TxVoltsAdc / 4096.0 * 2.5 * 11.0);
            }
            catch { return 0.0f; }
        }
    }
}
