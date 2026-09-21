using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace FlexMeters
{
    // Canonical persistence for the PowerSDR port.  The key families mirror
    // Thetis MeterManager.StoreSettings2/RestoreSettings:
    // meterContData_*, meterData_*, meterIGData_*, meterIGSettings_2_*.
    // The table itself is the native PowerSDR Options table, so meter state
    // participates in the same DB lifecycle as the rest of Setup.
    public sealed class ThetisOptionsMeterStore : IMeterStore
    {
        private const string ContainerPrefix = "meterContData_";
        private const string MeterPrefix = "meterData_";
        private const string GroupPrefix = "meterIGData_";
        private const string GroupSettingsPrefix = "meterIGSettings_2_";
        private const string FormatVersion = "2";

        private readonly DataTable _options;

        public ThetisOptionsMeterStore(DataTable options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            _options = options;
            EnsureSchema(_options);
        }

        public MeterWorkspaceSnapshot Load()
        {
            Dictionary<string, string> map = ReadMap();
            var ordered = new List<ContainerEnvelope>();

            foreach (KeyValuePair<string, string> pair in map)
            {
                if (!pair.Key.StartsWith(ContainerPrefix, StringComparison.Ordinal))
                    continue;

                Guid id;
                if (!Guid.TryParseExact(
                    pair.Key.Substring(ContainerPrefix.Length),
                    "D",
                    out id) ||
                    id == Guid.Empty)
                {
                    throw new MeterStoreFormatException(
                        "Invalid Thetis meter container key: " + pair.Key);
                }

                ContainerEnvelope envelope = ParseContainer(id, pair.Value);
                ordered.Add(envelope);
            }

            ordered.Sort(delegate(ContainerEnvelope a, ContainerEnvelope b)
            {
                int cmp = a.Sequence.CompareTo(b.Sequence);
                if (cmp != 0) return cmp;
                return a.Container.Id.CompareTo(b.Container.Id);
            });

            var workspace = new MeterWorkspaceSnapshot();
            for (int i = 0; i < ordered.Count; i++)
            {
                MeterContainerSnapshot container = ordered[i].Container;
                string meterKey = MeterPrefix + container.Id.ToString("D");
                string meterValue;
                if (!map.TryGetValue(meterKey, out meterValue))
                    throw new MeterStoreFormatException(
                        "Missing Thetis meter data for container " + container.Id + ".");

                Guid[] orderedItems = ParseMeter(container.Id, meterValue);
                for (int itemIndex = 0; itemIndex < orderedItems.Length; itemIndex++)
                {
                    Guid itemId = orderedItems[itemIndex];
                    string groupKey = GroupPrefix + itemId.ToString("D");
                    string groupValue;
                    if (!map.TryGetValue(groupKey, out groupValue))
                        throw new MeterStoreFormatException(
                            "Missing Thetis meter item data: " + groupKey);

                    MeterItemSnapshot item = ParseGroup(
                        container.Id,
                        itemId,
                        itemIndex,
                        groupValue);

                    string settingsKey =
                        GroupSettingsPrefix + itemId.ToString("D");
                    string settingsValue;
                    if (map.TryGetValue(settingsKey, out settingsValue))
                        ParseSettings(item, settingsValue);

                    container.Items.Add(item);
                }

                workspace.Containers.Add(container);
            }

            MeterWorkspaceValidator.Validate(workspace);
            return workspace;
        }

        public void ReplaceAll(MeterWorkspaceSnapshot snapshot)
        {
            MeterWorkspaceValidator.Validate(snapshot);

            _options.BeginLoadData();
            try
            {
                PurgeMeterRows();

                for (int i = 0; i < snapshot.Containers.Count; i++)
                {
                    MeterContainerSnapshot container = snapshot.Containers[i];
                    AddRow(
                        ContainerPrefix + container.Id.ToString("D"),
                        EncodeContainer(container, i));

                    AddRow(
                        MeterPrefix + container.Id.ToString("D"),
                        EncodeMeter(container));

                    for (int j = 0; j < container.Items.Count; j++)
                    {
                        MeterItemSnapshot item = container.Items[j];
                        AddRow(
                            GroupPrefix + item.Id.ToString("D"),
                            EncodeGroup(container.Id, item, j));
                        AddRow(
                            GroupSettingsPrefix + item.Id.ToString("D"),
                            EncodeSettings(item));
                    }
                }
            }
            finally
            {
                _options.EndLoadData();
            }
        }

        public static bool IsMeterKey(string key)
        {
            if (key == null) return false;
            return
                key.StartsWith(ContainerPrefix, StringComparison.Ordinal) ||
                key.StartsWith(MeterPrefix, StringComparison.Ordinal) ||
                key.StartsWith(GroupPrefix, StringComparison.Ordinal) ||
                key.StartsWith(GroupSettingsPrefix, StringComparison.Ordinal);
        }

        private sealed class ContainerEnvelope
        {
            public int Sequence;
            public MeterContainerSnapshot Container;
        }

        private void PurgeMeterRows()
        {
            for (int i = _options.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = _options.Rows[i];
                if (row.RowState == DataRowState.Deleted ||
                    row.RowState == DataRowState.Detached ||
                    row.IsNull("Key"))
                    continue;

                string key = Convert.ToString(row["Key"], CultureInfo.InvariantCulture);
                if (IsMeterKey(key))
                    _options.Rows.RemoveAt(i);
            }
        }

        private Dictionary<string, string> ReadMap()
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (DataRow row in _options.Rows)
            {
                if (row.RowState == DataRowState.Deleted ||
                    row.RowState == DataRowState.Detached)
                    continue;
                if (row.IsNull("Key") || row.IsNull("Value"))
                    continue;

                string key = Convert.ToString(
                    row["Key"],
                    CultureInfo.InvariantCulture);
                if (!IsMeterKey(key))
                    continue;

                string value = Convert.ToString(
                    row["Value"],
                    CultureInfo.InvariantCulture);

                if (map.ContainsKey(key))
                    throw new MeterStoreFormatException(
                        "Duplicate Thetis meter option key: " + key);

                map.Add(key, value);
            }

            return map;
        }

        private void AddRow(string key, string value)
        {
            DataRow row = _options.NewRow();
            row["Key"] = key;
            row["Value"] = value;
            _options.Rows.Add(row);
        }

        private static string EncodeContainer(
            MeterContainerSnapshot container,
            int sequence)
        {
            return String.Join(
                "|",
                new string[]
                {
                    FormatVersion,
                    sequence.ToString(CultureInfo.InvariantCulture),
                    ((int)container.Receiver).ToString(CultureInfo.InvariantCulture),
                    B(container.VisibleOnReceive),
                    B(container.VisibleOnTransmit),
                    container.Geometry.X.ToString(CultureInfo.InvariantCulture),
                    container.Geometry.Y.ToString(CultureInfo.InvariantCulture),
                    container.Geometry.Width.ToString(CultureInfo.InvariantCulture),
                    container.Geometry.Height.ToString(CultureInfo.InvariantCulture),
                    B(container.Geometry.Maximized),
                    B(container.Border),
                    B(container.Highlight),
                    B(container.Locked),
                    B(container.NoTitleBar),
                    B(container.AutoHeight),
                    B(container.Minimises),
                    B(container.HideWhenReceiverNotUsed),
                    container.BackgroundArgb.ToString(CultureInfo.InvariantCulture),
                    Base64(container.Notes ?? "")
                });
        }

        private static ContainerEnvelope ParseContainer(Guid id, string value)
        {
            string[] p = (value ?? "").Split('|');
            if (p.Length != 19 || p[0] != FormatVersion)
                throw new MeterStoreFormatException(
                    "Unsupported Thetis meter container data for " + id + ".");

            int receiverRaw = I(p[2], "receiver");
            if (!Enum.IsDefined(typeof(MeterReceiver), receiverRaw))
                throw new MeterStoreFormatException(
                    "Unsupported receiver on Thetis meter container " + id + ".");

            var container = new MeterContainerSnapshot();
            container.Id = id;
            container.Receiver = (MeterReceiver)receiverRaw;
            container.VisibleOnReceive = Bool(p[3], "show RX");
            container.VisibleOnTransmit = Bool(p[4], "show TX");
            container.Geometry.X = I(p[5], "x");
            container.Geometry.Y = I(p[6], "y");
            container.Geometry.Width = I(p[7], "width");
            container.Geometry.Height = I(p[8], "height");
            container.Geometry.Maximized = Bool(p[9], "maximized");
            container.Border = Bool(p[10], "border");
            container.Highlight = Bool(p[11], "highlight");
            container.Locked = Bool(p[12], "locked");
            container.NoTitleBar = Bool(p[13], "no title");
            container.AutoHeight = Bool(p[14], "auto height");
            container.Minimises = Bool(p[15], "minimises");
            container.HideWhenReceiverNotUsed = Bool(p[16], "hide when RX not used");
            container.BackgroundArgb = I(p[17], "background");
            container.Notes = FromBase64(p[18]);

            return new ContainerEnvelope
            {
                Sequence = I(p[1], "sequence"),
                Container = container
            };
        }

        private static string EncodeMeter(MeterContainerSnapshot container)
        {
            var values = new List<string>();
            values.Add(FormatVersion);
            values.Add(container.Items.Count.ToString(CultureInfo.InvariantCulture));
            for (int i = 0; i < container.Items.Count; i++)
                values.Add(container.Items[i].Id.ToString("D"));
            return String.Join("|", values.ToArray());
        }

        private static Guid[] ParseMeter(Guid containerId, string value)
        {
            string[] p = (value ?? "").Split('|');
            if (p.Length < 2 || p[0] != FormatVersion)
                throw new MeterStoreFormatException(
                    "Unsupported Thetis meter data for " + containerId + ".");

            int count = I(p[1], "item count");
            if (count < 0 || p.Length != count + 2)
                throw new MeterStoreFormatException(
                    "Invalid item count for Thetis meter " + containerId + ".");

            Guid[] ids = new Guid[count];
            for (int i = 0; i < count; i++)
            {
                if (!Guid.TryParseExact(p[i + 2], "D", out ids[i]) ||
                    ids[i] == Guid.Empty)
                    throw new MeterStoreFormatException(
                        "Invalid item ID in Thetis meter " + containerId + ".");
            }

            return ids;
        }

        private static string EncodeGroup(
            Guid containerId,
            MeterItemSnapshot item,
            int order)
        {
            return String.Join(
                "|",
                new string[]
                {
                    FormatVersion,
                    containerId.ToString("D"),
                    item.Id.ToString("D"),
                    Base64(item.Type),
                    order.ToString(CultureInfo.InvariantCulture)
                });
        }

        private static MeterItemSnapshot ParseGroup(
            Guid expectedContainerId,
            Guid expectedItemId,
            int expectedOrder,
            string value)
        {
            string[] p = (value ?? "").Split('|');
            if (p.Length != 5 || p[0] != FormatVersion)
                throw new MeterStoreFormatException(
                    "Unsupported Thetis meter item data for " + expectedItemId + ".");

            Guid containerId;
            Guid itemId;
            if (!Guid.TryParseExact(p[1], "D", out containerId) ||
                containerId != expectedContainerId)
                throw new MeterStoreFormatException(
                    "Container mismatch on Thetis meter item " + expectedItemId + ".");
            if (!Guid.TryParseExact(p[2], "D", out itemId) ||
                itemId != expectedItemId)
                throw new MeterStoreFormatException(
                    "Item ID mismatch on Thetis meter item " + expectedItemId + ".");
            if (I(p[4], "item order") != expectedOrder)
                throw new MeterStoreFormatException(
                    "Item order mismatch on Thetis meter item " + expectedItemId + ".");

            return new MeterItemSnapshot
            {
                Id = itemId,
                Type = FromBase64(p[3])
            };
        }

        private static string EncodeSettings(MeterItemSnapshot item)
        {
            var names = new List<string>(item.Settings.Keys);
            names.Sort(StringComparer.Ordinal);

            var values = new List<string>();
            values.Add(FormatVersion);
            values.Add(names.Count.ToString(CultureInfo.InvariantCulture));

            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                values.Add(Base64(name));
                values.Add(Base64(item.Settings[name] ?? ""));
            }

            return String.Join("|", values.ToArray());
        }

        private static void ParseSettings(
            MeterItemSnapshot item,
            string value)
        {
            string[] p = (value ?? "").Split('|');
            if (p.Length < 2 || p[0] != FormatVersion)
                throw new MeterStoreFormatException(
                    "Unsupported Thetis meter item settings for " + item.Id + ".");

            int count = I(p[1], "settings count");
            if (count < 0 || p.Length != 2 + (count * 2))
                throw new MeterStoreFormatException(
                    "Invalid settings count for Thetis meter item " + item.Id + ".");

            for (int i = 0; i < count; i++)
            {
                string key = FromBase64(p[2 + (i * 2)]);
                string settingValue = FromBase64(p[3 + (i * 2)]);
                if (String.IsNullOrWhiteSpace(key) ||
                    item.Settings.ContainsKey(key))
                    throw new MeterStoreFormatException(
                        "Invalid/duplicate setting on Thetis meter item " + item.Id + ".");
                item.Settings.Add(key, settingValue);
            }
        }

        private static void EnsureSchema(DataTable table)
        {
            if (!table.Columns.Contains("Key") ||
                !table.Columns.Contains("Value"))
                throw new InvalidOperationException(
                    "PowerSDR Options table requires Key and Value columns.");
        }

        private static string B(bool value)
        {
            return value ? "1" : "0";
        }

        private static bool Bool(string value, string label)
        {
            if (value == "1") return true;
            if (value == "0") return false;
            throw new MeterStoreFormatException(
                "Invalid boolean for " + label + ": " + value);
        }

        private static int I(string value, string label)
        {
            int result;
            if (!Int32.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result))
                throw new MeterStoreFormatException(
                    "Invalid integer for " + label + ": " + value);
            return result;
        }

        private static string Base64(string value)
        {
            return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(value ?? ""));
        }

        private static string FromBase64(string value)
        {
            try
            {
                return Encoding.UTF8.GetString(
                    Convert.FromBase64String(value ?? ""));
            }
            catch (Exception ex)
            {
                throw new MeterStoreFormatException(
                    "Invalid base64 in Thetis meter store.",
                    ex);
            }
        }
    }
}
