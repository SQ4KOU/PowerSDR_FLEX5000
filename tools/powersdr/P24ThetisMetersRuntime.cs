using System;
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
        private sealed class PersistedState
        {
            public int Version { get; set; }
            public List<string> Containers { get; set; }
            public string MultiMeterIO { get; set; }
        }

        private static readonly object Sync = new object();
        private static bool _initialised;
        private static bool _restoring;
        private static Console _console;
        private static string _statePath;

        internal static void Init(Console console)
        {
            if (console == null) return;
            lock (Sync)
            {
                if (_initialised) return;
                _initialised = true;
                _console = console;
            }

            try
            {
                string root = console.AppDataPath;
                if (String.IsNullOrWhiteSpace(root))
                    root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FlexRadio Systems", "PowerSDR");
                Directory.CreateDirectory(root);
                _statePath = Path.Combine(root, "P24_Thetis_Meters_Gadgets.json");

                MeterManager.Init(console, null);
                Restore();

                console.FormClosing += delegate
                {
                    try { Save(); } catch { }
                    try { MeterManager.Shutdown(); } catch { }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("P24 meter runtime init: " + ex);
            }
        }

        internal static void Save()
        {
            if (!_initialised || _restoring || String.IsNullOrWhiteSpace(_statePath)) return;
            try
            {
                PersistedState state = new PersistedState();
                state.Version = 1;
                state.Containers = new List<string>();

                foreach (KeyValuePair<string, ucMeter> kvp in MeterManager.MeterContainers.OrderBy(k => k.Value.Sequence))
                {
                    string encoded = MeterManager.ContainerToString(kvp.Value.ID);
                    if (!String.IsNullOrWhiteSpace(encoded))
                        state.Containers.Add(encoded);
                }

                try { state.MultiMeterIO = MultiMeterIO.GetSaveData(); }
                catch { state.MultiMeterIO = ""; }

                string json = JsonConvert.SerializeObject(state, Formatting.Indented);
                string tmp = _statePath + ".tmp";
                File.WriteAllText(tmp, json, new UTF8Encoding(false));
                if (File.Exists(_statePath))
                {
                    string bak = _statePath + ".bak";
                    try { File.Replace(tmp, _statePath, bak, true); }
                    catch { File.Copy(tmp, _statePath, true); File.Delete(tmp); }
                }
                else File.Move(tmp, _statePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("P24 meter runtime save: " + ex);
            }
        }

        internal static void Restore()
        {
            if (!_initialised || String.IsNullOrWhiteSpace(_statePath) || !File.Exists(_statePath)) return;
            _restoring = true;
            try
            {
                PersistedState state = JsonConvert.DeserializeObject<PersistedState>(File.ReadAllText(_statePath, Encoding.UTF8));
                if (state == null) return;

                MeterScriptEngine.BeginBatch();
                try
                {
                    if (state.Containers != null)
                    {
                        foreach (string encoded in state.Containers)
                        {
                            if (String.IsNullOrWhiteSpace(encoded)) continue;
                            ucMeter ucm = MeterManager.ContainerFromString(encoded);
                            if (ucm == null) continue;
                            MeterManager.RunRendererDisplay(ucm.ID);
                            MeterManager.FinishSetupAndDisplay(ucm.ID);
                        }
                    }
                }
                finally
                {
                    MeterScriptEngine.EndBatch();
                }

                if (!String.IsNullOrWhiteSpace(state.MultiMeterIO))
                {
                    try { MultiMeterIO.RestoreSaveData2(state.MultiMeterIO); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("P24 meter runtime restore: " + ex);
            }
            finally { _restoring = false; }
        }
    }
}
