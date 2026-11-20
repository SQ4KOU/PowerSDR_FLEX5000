using System;
using System.Windows.Forms;

namespace PowerSDR
{
    sealed unsafe public partial class Console
    {
        internal bool P32VFOALock
        {
            get { return VFOLock; }
            set { VFOLock = value; }
        }
        internal bool P32VFOBLock
        {
            get { return VFOLockB; }
            set { VFOLockB = value; }
        }

        internal string P32RX1FilterName
        {
            get
            {
                try
                {
                    if (RX1DSPMode == DSPMode.FIRST || RX1DSPMode == DSPMode.LAST ||
                        RX1Filter == Filter.FIRST || RX1Filter == Filter.LAST)
                        return String.Empty;

                    FilterPreset preset = rx1_filters[(int)RX1DSPMode];
                    if (preset == null) return RX1Filter.ToString();

                    string name = preset.GetName(RX1Filter);
                    return String.IsNullOrEmpty(name) ? RX1Filter.ToString() : name;
                }
                catch
                {
                    return RX1Filter.ToString();
                }
            }
        }

        internal void P32SetRX1Band(Band band) { SetCATBand(band); }
        internal void P32SetRX2Band(Band band) { RX2Band = band; }

        internal void P32PopupFilterMenu(int rx)
        {
            // PowerSDR has no Thetis filter-popup API. Filter buttons themselves
            // map directly to RX1Filter; only the Thetis context-popup has no native peer.
        }
    }
}