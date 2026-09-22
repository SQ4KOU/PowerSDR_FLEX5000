using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace PowerSDR
{
    internal static class P24ThetisMetersRuntime
    {
        private sealed class LegacyPersistedState
        {
            public int Version { get; set; }
            public List<string> Containers { get; set; }
            public string MultiMeterIO { get; set; }
        }

        private static readonly object Sync = new object();
        private static bool _initialised;
        private static bool _restoring;
        private static bool _finished;
        private static Console _console;
        private static string _legacyStatePath;
        private static string _diagPath;

        internal static void Init(Console console)
        {
            if (console == null) return;

            lock (Sync)
            {
                if (_initialised) return;
                _console = console;
            }

            try
            {
                string root = console.AppDataPath;
                if (String.IsNullOrWhiteSpace(root))
                    root = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "FlexRadio Systems", "PowerSDR");

                Directory.CreateDirectory(root);
                _legacyStatePath = Path.Combine(root, "P24_Thetis_Meters_Gadgets.json");
                _diagPath = Path.Combine(root, "P24_MetersGadgets_DB.log");

                MeterManager.Init(console, null);

                lock (Sync)
                {
                    _initialised = true;
                }

                Log("Init OK; containers=" + MeterManager.TotalMeterContainers.ToString());

                console.FormClosing += delegate
                {
                    try { MeterManager.Shutdown(); }
                    catch (Exception ex) { Log("Shutdown ERROR: " + SafeException(ex)); }
                };
            }
            catch (Exception ex)
            {
                lock (Sync)
                {
                    _initialised = false;
                }
                Log("Init ERROR: " + SafeException(ex));
                Debug.WriteLine("P24 meter runtime init: " + SafeException(ex));
            }
        }

        internal static void RestoreFromPowerSdrOptions(ArrayList rows)
        {
            if (!_initialised || rows == null) return;

            lock (Sync)
            {
                if (_restoring) return;
                _restoring = true;
            }

            try
            {
                Dictionary<string, string> all = RowsToDictionary(rows);
                Dictionary<string, string> meter = all
                    .Where(kvp => IsMeterKey(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

                RemoveAllContainers();

                bool restored = true;
                if (meter.Count > 0)
                    restored = MeterManager.RestoreSettings(ref meter);

                bool mmioOk = true;
                string mmio;
                if (all.TryGetValue("multimeter_io2", out mmio) && !String.IsNullOrWhiteSpace(mmio))
                {
                    try { mmioOk = MultiMeterIO.RestoreSaveData2(mmio); }
                    catch { mmioOk = false; }
                }

                bool migrated = false;
                if (MeterManager.TotalMeterContainers == 0)
                    migrated = TryMigrateLegacyJson();

                bool bootstrapped = false;
                if (MeterManager.TotalMeterContainers == 0)
                {
                    ucMeter uc = new ucMeter();
                    uc.RX = 1;
                    uc.Floating = false;
                    MeterManager.AddMeterContainer(uc, true);
                    bootstrapped = MeterManager.TotalMeterContainers > 0;
                }

                Log(
                    "Restore Options: meterRows=" + meter.Count.ToString() +
                    " restoreOk=" + restored.ToString() +
                    " mmioOk=" + mmioOk.ToString() +
                    " migrated=" + migrated.ToString() +
                    " bootstrapped=" + bootstrapped.ToString() +
                    " containers=" + MeterManager.TotalMeterContainers.ToString());
            }
            catch (Exception ex)
            {
                Log("Restore Options ERROR: " + SafeException(ex));
                Debug.WriteLine("P24 meter runtime DB restore: " + SafeException(ex));
            }
            finally
            {
                lock (Sync) { _restoring = false; }
            }
        }

        internal static void StoreIntoPowerSdrOptions(ArrayList rows)
        {
            if (!_initialised || _restoring || rows == null) return;

            try
            {
                RemoveMeterRows(rows);

                Dictionary<string, string> meter = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                bool ok = MeterManager.StoreSettings2(ref meter);

                foreach (KeyValuePair<string, string> kvp in meter)
                    rows.Add(kvp.Key + "/" + (kvp.Value ?? String.Empty));

                string mmio = String.Empty;
                try { mmio = MultiMeterIO.GetSaveData(); }
                catch { }

                if (!String.IsNullOrWhiteSpace(mmio))
                    rows.Add("multimeter_io2/" + mmio);

                Log(
                    "Store Options: ok=" + ok.ToString() +
                    " meterRows=" + meter.Count.ToString() +
                    " containers=" + MeterManager.TotalMeterContainers.ToString());
            }
            catch (Exception ex)
            {
                Log("Store Options ERROR: " + SafeException(ex));
                Debug.WriteLine("P24 meter runtime DB store: " + SafeException(ex));
            }
        }

        internal static void FinishSetup()
        {
            if (!_initialised || _finished) return;

            try
            {
                MeterManager.RunAllRendererDisplays();
                MeterManager.FinishSetupAndDisplay();
                _finished = true;
                Log("FinishSetup OK; containers=" + MeterManager.TotalMeterContainers.ToString());
            }
            catch (Exception ex)
            {
                Log("FinishSetup ERROR: " + SafeException(ex));
                Debug.WriteLine("P24 meter runtime finish: " + SafeException(ex));
            }
        }

        internal static int ContainerCount
        {
            get
            {
                try { return MeterManager.TotalMeterContainers; }
                catch { return -1; }
            }
        }

        private static bool TryMigrateLegacyJson()
        {
            if (String.IsNullOrWhiteSpace(_legacyStatePath) || !File.Exists(_legacyStatePath))
                return false;

            try
            {
                LegacyPersistedState state = JsonConvert.DeserializeObject<LegacyPersistedState>(
                    File.ReadAllText(_legacyStatePath, Encoding.UTF8));

                if (state == null) return false;

                if (state.Containers != null)
                {
                    foreach (string encoded in state.Containers)
                    {
                        if (String.IsNullOrWhiteSpace(encoded)) continue;
                        try { MeterManager.ContainerFromString(encoded); }
                        catch { }
                    }
                }

                if (!String.IsNullOrWhiteSpace(state.MultiMeterIO))
                {
                    try { MultiMeterIO.RestoreSaveData2(state.MultiMeterIO); }
                    catch { }
                }

                if (MeterManager.TotalMeterContainers > 0)
                {
                    string migrated = _legacyStatePath + ".migrated";
                    try
                    {
                        if (File.Exists(migrated)) File.Delete(migrated);
                        File.Move(_legacyStatePath, migrated);
                    }
                    catch { }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log("Legacy migration ERROR: " + SafeException(ex));
            }

            return false;
        }

        private static void RemoveAllContainers()
        {
            try
            {
                List<string> ids = MeterManager.MeterContainers.Keys.ToList();
                foreach (string id in ids)
                {
                    try { MeterManager.RemoveMeterContainer(id); }
                    catch { }
                }
            }
            catch { }
        }

        private static Dictionary<string, string> RowsToDictionary(ArrayList rows)
        {
            Dictionary<string, string> data =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (object obj in rows)
            {
                string row = obj as string;
                if (String.IsNullOrEmpty(row)) continue;

                int slash = row.IndexOf('/');
                if (slash <= 0) continue;

                string key = row.Substring(0, slash);
                string value = slash + 1 < row.Length ? row.Substring(slash + 1) : String.Empty;
                data[key] = value;
            }

            return data;
        }

        private static void RemoveMeterRows(ArrayList rows)
        {
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                string row = rows[i] as string;
                if (String.IsNullOrEmpty(row)) continue;

                int slash = row.IndexOf('/');
                string key = slash > 0 ? row.Substring(0, slash) : row;

                if (IsMeterKey(key) ||
                    String.Equals(key, "multimeter_io2", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(key, "multimeter_io", StringComparison.OrdinalIgnoreCase))
                    rows.RemoveAt(i);
            }
        }

        private static bool IsMeterKey(string key)
        {
            if (String.IsNullOrEmpty(key)) return false;

            return key.StartsWith("meterContData_", StringComparison.OrdinalIgnoreCase) ||
                   key.StartsWith("meterData_", StringComparison.OrdinalIgnoreCase) ||
                   key.StartsWith("meterIGData_", StringComparison.OrdinalIgnoreCase) ||
                   key.StartsWith("meterIGSettings_", StringComparison.OrdinalIgnoreCase);
        }

        private static string SafeException(Exception ex)
        {
            if (ex == null) return "<null>";
            try
            {
                string typeName = ex.GetType().FullName ?? ex.GetType().Name;
                string message;
                try { message = ex.Message; }
                catch { message = "<message unavailable>"; }

                string inner = String.Empty;
                try
                {
                    if (ex.InnerException != null)
                    {
                        string innerType = ex.InnerException.GetType().FullName ?? ex.InnerException.GetType().Name;
                        string innerMessage;
                        try { innerMessage = ex.InnerException.Message; }
                        catch { innerMessage = "<message unavailable>"; }
                        inner = " | inner=" + innerType + ": " + innerMessage;
                    }
                }
                catch { }

                return typeName + ": " + message + inner;
            }
            catch
            {
                return "<exception details unavailable>";
            }
        }

        private static void Log(string message)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(_diagPath)) return;
                File.AppendAllText(
                    _diagPath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                    " | " + message + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch { }
        }
    }
}
