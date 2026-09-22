using System;

namespace PowerSDR
{
    // Compile-time compatibility only for Thetis meter features tied to HPSDR-specific
    // display/audio services. These are not radio backends and do not touch FLEX hardware.
    internal sealed class P24AdaptorInfo
    {
    }

    public class SpecHPSDR
    {
        public SpecHPSDR(int display) { }
    }

    public sealed class clsAudioRecordPlayback : IDisposable
    {
        public void Dispose() { }
    }
}
