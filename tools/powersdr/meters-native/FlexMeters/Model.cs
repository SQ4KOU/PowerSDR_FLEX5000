using System;
using System.Collections.Generic;

namespace FlexMeters
{
    public sealed class MeterWorkspaceSnapshot
    {
        public MeterWorkspaceSnapshot()
        {
            Containers = new List<MeterContainerSnapshot>();
        }

        public IList<MeterContainerSnapshot> Containers { get; private set; }
    }

    public sealed class MeterContainerSnapshot
    {
        public MeterContainerSnapshot()
        {
            Geometry = new MeterWindowGeometry();
            Items = new List<MeterItemSnapshot>();
            VisibleOnReceive = true;
            VisibleOnTransmit = true;
            Border = true;
            Highlight = false;
            Locked = false;
            NoTitleBar = false;
            AutoHeight = false;
            Minimises = true;
            HideWhenReceiverNotUsed = true;
            BackgroundArgb = unchecked((int)0xFF202020);
            Notes = "";
        }

        public Guid Id { get; set; }
        public MeterReceiver Receiver { get; set; }
        public bool VisibleOnReceive { get; set; }
        public bool VisibleOnTransmit { get; set; }

        // Thetis ucMeter container state. These deliberately mirror the
        // container options exposed by Setup -> Appearance -> Meters/Gadgets.
        public bool Border { get; set; }
        public bool Highlight { get; set; }
        public bool Locked { get; set; }
        public bool NoTitleBar { get; set; }
        public bool AutoHeight { get; set; }
        public bool Minimises { get; set; }
        public bool HideWhenReceiverNotUsed { get; set; }
        public int BackgroundArgb { get; set; }
        public string Notes { get; set; }

        public MeterWindowGeometry Geometry { get; set; }
        public IList<MeterItemSnapshot> Items { get; private set; }
    }

    public sealed class MeterWindowGeometry
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Maximized { get; set; }
    }

    public sealed class MeterItemSnapshot
    {
        public MeterItemSnapshot()
        {
            Settings = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public Guid Id { get; set; }
        public string Type { get; set; }
        public IDictionary<string, string> Settings { get; private set; }
    }

    internal static class MeterWorkspaceValidator
    {
        public static void Validate(MeterWorkspaceSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");

            var containerIds = new HashSet<Guid>();
            var itemIds = new HashSet<Guid>();

            for (int i = 0; i < snapshot.Containers.Count; i++)
            {
                MeterContainerSnapshot container = snapshot.Containers[i];
                if (container == null)
                    throw new InvalidOperationException("Container " + i + " is null.");
                if (container.Id == Guid.Empty)
                    throw new InvalidOperationException("Container " + i + " has an empty ID.");
                if (!containerIds.Add(container.Id))
                    throw new InvalidOperationException("Duplicate container ID: " + container.Id);
                if (!Enum.IsDefined(typeof(MeterReceiver), container.Receiver))
                    throw new InvalidOperationException("Unsupported receiver on container " + container.Id + ".");
                if (container.Geometry == null)
                    throw new InvalidOperationException("Container " + container.Id + " has no geometry.");
                if (container.Geometry.Width <= 0 || container.Geometry.Height <= 0)
                    throw new InvalidOperationException("Container " + container.Id + " has invalid geometry.");

                for (int j = 0; j < container.Items.Count; j++)
                {
                    MeterItemSnapshot item = container.Items[j];
                    if (item == null)
                        throw new InvalidOperationException("Container " + container.Id + " contains a null item.");
                    if (item.Id == Guid.Empty)
                        throw new InvalidOperationException("Container " + container.Id + " contains an item with an empty ID.");
                    if (!itemIds.Add(item.Id))
                        throw new InvalidOperationException("Duplicate item ID: " + item.Id);
                    if (String.IsNullOrWhiteSpace(item.Type))
                        throw new InvalidOperationException("Item " + item.Id + " has no type.");

                    foreach (KeyValuePair<string, string> setting in item.Settings)
                    {
                        if (String.IsNullOrWhiteSpace(setting.Key))
                            throw new InvalidOperationException("Item " + item.Id + " has an empty setting key.");
                        if (setting.Value == null)
                            throw new InvalidOperationException("Item " + item.Id + " setting '" + setting.Key + "' has a null value.");
                    }
                }
            }
        }
    }
}
