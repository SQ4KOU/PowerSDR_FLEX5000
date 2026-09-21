using System;

namespace FlexMeters
{
    public static class MeterWindowVisibility
    {
        public static bool ShouldShow(
            MeterContainerSnapshot container,
            MeterRadioStateSnapshot radioState)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            if (radioState == null)
                return true;

            return radioState.Mox
                ? container.VisibleOnTransmit
                : container.VisibleOnReceive;
        }
    }
}
