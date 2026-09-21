using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace FlexMeters
{
    public sealed class MeterStoreFormatException : InvalidDataException
    {
        public MeterStoreFormatException(string message) : base(message) { }
        public MeterStoreFormatException(string message, Exception innerException) : base(message, innerException) { }
    }

    public sealed class DataTableMeterStore : IMeterStore
    {
        public const string KeyColumn = "Key";
        public const string ValueColumn = "Value";
        private const string SchemaVersion = "1";

        private readonly DataTable _table;

        public DataTableMeterStore(DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");

            _table = table;
            EnsureSchema(_table);
        }

        public MeterWorkspaceSnapshot Load()
        {
            if (_table.Rows.Count == 0)
                return new MeterWorkspaceSnapshot();

            Dictionary<string, string> map = ReadMap();
            var used = new HashSet<string>(StringComparer.Ordinal);

            string version = Required(map, "schema.version", used);
            if (!String.Equals(version, SchemaVersion, StringComparison.Ordinal))
                throw new MeterStoreFormatException("Unsupported FlexMeters store schema: " + version);

            int containerCount = ParseNonNegativeInt(Required(map, "container.count", used), "container.count");
            var snapshot = new MeterWorkspaceSnapshot();

            for (int i = 0; i < containerCount; i++)
            {
                string containerIdText = Required(map, "container.order." + i, used);
                Guid containerId = ParseGuid(containerIdText, "container.order." + i);
                string cp = "container." + containerId.ToString("D") + ".";

                var container = new MeterContainerSnapshot();
                container.Id = containerId;
                container.Receiver = ParseReceiver(Required(map, cp + "receiver", used), cp + "receiver");
                container.VisibleOnReceive = ParseBool(Required(map, cp + "visible.rx", used), cp + "visible.rx");
                container.VisibleOnTransmit = ParseBool(Required(map, cp + "visible.tx", used), cp + "visible.tx");
                container.Geometry.X = ParseInt(Required(map, cp + "geometry.x", used), cp + "geometry.x");
                container.Geometry.Y = ParseInt(Required(map, cp + "geometry.y", used), cp + "geometry.y");
                container.Geometry.Width = ParseInt(Required(map, cp + "geometry.width", used), cp + "geometry.width");
                container.Geometry.Height = ParseInt(Required(map, cp + "geometry.height", used), cp + "geometry.height");
                container.Geometry.Maximized = ParseBool(Required(map, cp + "geometry.maximized", used), cp + "geometry.maximized");

                int itemCount = ParseNonNegativeInt(Required(map, cp + "item.count", used), cp + "item.count");
                for (int j = 0; j < itemCount; j++)
                {
                    string itemIdText = Required(map, cp + "item.order." + j, used);
                    Guid itemId = ParseGuid(itemIdText, cp + "item.order." + j);
                    string ip = "item." + itemId.ToString("D") + ".";

                    var item = new MeterItemSnapshot();
                    item.Id = itemId;
                    item.Type = Required(map, ip + "type", used);

                    int settingCount = ParseNonNegativeInt(Required(map, ip + "setting.count", used), ip + "setting.count");
                    for (int k = 0; k < settingCount; k++)
                    {
                        string key = Required(map, ip + "setting." + k + ".name", used);
                        string value = Required(map, ip + "setting." + k + ".value", used);
                        if (item.Settings.ContainsKey(key))
                            throw new MeterStoreFormatException("Duplicate setting key '" + key + "' on item " + itemId + ".");
                        item.Settings.Add(key, value);
                    }

                    container.Items.Add(item);
                }

                snapshot.Containers.Add(container);
            }

            if (used.Count != map.Count)
            {
                foreach (string key in map.Keys)
                {
                    if (!used.Contains(key))
                        throw new MeterStoreFormatException("Unexpected/stale FlexMeters store key: " + key);
                }
            }

            try
            {
                MeterWorkspaceValidator.Validate(snapshot);
            }
            catch (Exception ex)
            {
                throw new MeterStoreFormatException("FlexMeters store failed model validation.", ex);
            }

            return snapshot;
        }

        public void ReplaceAll(MeterWorkspaceSnapshot snapshot)
        {
            MeterWorkspaceValidator.Validate(snapshot);

            List<KeyValuePair<string, string>> rows = Encode(snapshot);
            List<object[]> backup = CaptureRows();

            try
            {
                ReplaceRows(rows);
            }
            catch
            {
                RestoreRows(backup);
                throw;
            }
        }

        private static void EnsureSchema(DataTable table)
        {
            if (table.Columns.Count == 0)
            {
                table.Columns.Add(KeyColumn, typeof(string));
                table.Columns.Add(ValueColumn, typeof(string));
            }

            if (table.Columns.Count != 2 ||
                !table.Columns.Contains(KeyColumn) ||
                !table.Columns.Contains(ValueColumn) ||
                table.Columns[KeyColumn].DataType != typeof(string) ||
                table.Columns[ValueColumn].DataType != typeof(string))
            {
                throw new InvalidOperationException("FlexMeters DataTable must contain exactly string Key and Value columns.");
            }

            if (table.PrimaryKey.Length == 0)
                table.PrimaryKey = new DataColumn[] { table.Columns[KeyColumn] };
            else if (table.PrimaryKey.Length != 1 || table.PrimaryKey[0] != table.Columns[KeyColumn])
                throw new InvalidOperationException("FlexMeters DataTable primary key must be the Key column.");
        }

        private Dictionary<string, string> ReadMap()
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (DataRow row in _table.Rows)
            {
                if (row.IsNull(KeyColumn) || row.IsNull(ValueColumn))
                    throw new MeterStoreFormatException("FlexMeters store contains a null Key or Value.");

                string key = (string)row[KeyColumn];
                string value = (string)row[ValueColumn];

                if (String.IsNullOrEmpty(key))
                    throw new MeterStoreFormatException("FlexMeters store contains an empty Key.");
                if (map.ContainsKey(key))
                    throw new MeterStoreFormatException("FlexMeters store contains duplicate Key: " + key);

                map.Add(key, value);
            }

            return map;
        }

        private static List<KeyValuePair<string, string>> Encode(MeterWorkspaceSnapshot snapshot)
        {
            var rows = new List<KeyValuePair<string, string>>();
            var keys = new HashSet<string>(StringComparer.Ordinal);

            Add(rows, keys, "schema.version", SchemaVersion);
            Add(rows, keys, "container.count", snapshot.Containers.Count.ToString(CultureInfo.InvariantCulture));

            for (int i = 0; i < snapshot.Containers.Count; i++)
            {
                MeterContainerSnapshot container = snapshot.Containers[i];
                string id = container.Id.ToString("D");
                string cp = "container." + id + ".";

                Add(rows, keys, "container.order." + i, id);
                Add(rows, keys, cp + "receiver", ((int)container.Receiver).ToString(CultureInfo.InvariantCulture));
                Add(rows, keys, cp + "visible.rx", FormatBool(container.VisibleOnReceive));
                Add(rows, keys, cp + "visible.tx", FormatBool(container.VisibleOnTransmit));
                Add(rows, keys, cp + "geometry.x", container.Geometry.X.ToString(CultureInfo.InvariantCulture));
                Add(rows, keys, cp + "geometry.y", container.Geometry.Y.ToString(CultureInfo.InvariantCulture));
                Add(rows, keys, cp + "geometry.width", container.Geometry.Width.ToString(CultureInfo.InvariantCulture));
                Add(rows, keys, cp + "geometry.height", container.Geometry.Height.ToString(CultureInfo.InvariantCulture));
                Add(rows, keys, cp + "geometry.maximized", FormatBool(container.Geometry.Maximized));
                Add(rows, keys, cp + "item.count", container.Items.Count.ToString(CultureInfo.InvariantCulture));

                for (int j = 0; j < container.Items.Count; j++)
                {
                    MeterItemSnapshot item = container.Items[j];
                    string itemId = item.Id.ToString("D");
                    string ip = "item." + itemId + ".";

                    Add(rows, keys, cp + "item.order." + j, itemId);
                    Add(rows, keys, ip + "type", item.Type);

                    var settingNames = new List<string>(item.Settings.Keys);
                    settingNames.Sort(StringComparer.Ordinal);
                    Add(rows, keys, ip + "setting.count", settingNames.Count.ToString(CultureInfo.InvariantCulture));

                    for (int k = 0; k < settingNames.Count; k++)
                    {
                        string settingName = settingNames[k];
                        Add(rows, keys, ip + "setting." + k + ".name", settingName);
                        Add(rows, keys, ip + "setting." + k + ".value", item.Settings[settingName]);
                    }
                }
            }

            return rows;
        }

        private static void Add(List<KeyValuePair<string, string>> rows, HashSet<string> keys, string key, string value)
        {
            if (!keys.Add(key))
                throw new InvalidOperationException("Generated duplicate FlexMeters key: " + key);
            rows.Add(new KeyValuePair<string, string>(key, value));
        }

        private List<object[]> CaptureRows()
        {
            var backup = new List<object[]>();
            foreach (DataRow row in _table.Rows)
                backup.Add((object[])row.ItemArray.Clone());
            return backup;
        }

        private void ReplaceRows(IList<KeyValuePair<string, string>> rows)
        {
            _table.BeginLoadData();
            try
            {
                _table.Rows.Clear();
                for (int i = 0; i < rows.Count; i++)
                {
                    DataRow row = _table.NewRow();
                    row[KeyColumn] = rows[i].Key;
                    row[ValueColumn] = rows[i].Value;
                    _table.Rows.Add(row);
                }
            }
            finally
            {
                _table.EndLoadData();
            }
        }

        private void RestoreRows(IList<object[]> backup)
        {
            _table.BeginLoadData();
            try
            {
                _table.Rows.Clear();
                for (int i = 0; i < backup.Count; i++)
                    _table.Rows.Add(backup[i]);
            }
            finally
            {
                _table.EndLoadData();
            }
        }

        private static string Required(Dictionary<string, string> map, string key, HashSet<string> used)
        {
            string value;
            if (!map.TryGetValue(key, out value))
                throw new MeterStoreFormatException("Missing FlexMeters store key: " + key);
            used.Add(key);
            return value;
        }

        private static Guid ParseGuid(string value, string key)
        {
            Guid result;
            if (!Guid.TryParseExact(value, "D", out result) || result == Guid.Empty)
                throw new MeterStoreFormatException("Invalid GUID in " + key + ": " + value);
            return result;
        }

        private static int ParseInt(string value, string key)
        {
            int result;
            if (!Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                throw new MeterStoreFormatException("Invalid integer in " + key + ": " + value);
            return result;
        }

        private static int ParseNonNegativeInt(string value, string key)
        {
            int result = ParseInt(value, key);
            if (result < 0)
                throw new MeterStoreFormatException("Negative integer in " + key + ": " + value);
            return result;
        }

        private static MeterReceiver ParseReceiver(string value, string key)
        {
            int raw = ParseInt(value, key);
            if (!Enum.IsDefined(typeof(MeterReceiver), raw))
                throw new MeterStoreFormatException("Unsupported receiver in " + key + ": " + value);
            return (MeterReceiver)raw;
        }

        private static bool ParseBool(string value, string key)
        {
            if (String.Equals(value, "1", StringComparison.Ordinal))
                return true;
            if (String.Equals(value, "0", StringComparison.Ordinal))
                return false;
            throw new MeterStoreFormatException("Invalid boolean in " + key + ": " + value);
        }

        private static string FormatBool(bool value)
        {
            return value ? "1" : "0";
        }
    }
}
