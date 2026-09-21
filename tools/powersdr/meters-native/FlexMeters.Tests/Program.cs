using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using FlexMeters;

namespace FlexMeters.Tests
{
    internal static class Program
    {
        private static int _passed;

        private static int Main()
        {
            Run("empty store -> zero containers", EmptyStoreRestoresZeroContainers);
            Run("two containers round-trip exactly", TwoContainersRoundTripExactly);
            Run("delete + replace-all purges stale container", DeleteThenReplaceAllPurgesStaleContainer);
            Run("replace-all with zero containers clears old IDs", ZeroContainersPurgesAllContainerIds);
            Run("unsupported reading has no numeric value", UnsupportedReadingHasNoNumericValue);
            Run("fake telemetry signal changes are observable", FakeTelemetrySignalChangesAreObservable);
            Run("FLEX-5000 RX1 calibration matches native KE9NS sum", Flex5000Rx1CalibrationMatchesNativeSum);
            Run("FLEX-5000 RX1 loop gain is conditional", Flex5000Rx1LoopGainIsConditional);
            Run("live runtime propagates changing RX1 signal", LiveRuntimePropagatesChangingRx1Signal);
            Run("live runtime deduplicates identical telemetry requests", LiveRuntimeDeduplicatesIdenticalRequests);
            Run("live runtime preserves unsupported without zero", LiveRuntimePreservesUnsupportedWithoutZero);
            Run("live runtime ignores unbound item types", LiveRuntimeIgnoresUnboundItemTypes);
            Run("store survives DataSet XML restart", StoreSurvivesDataSetXmlRestart);
            Run("workspace host starts from replace-all store", WorkspaceHostStartsFromStore);
            Run("workspace host replace-all reloads runtime", WorkspaceHostReplaceAllReloadsRuntime);
            Run("workspace host reload sees external store change", WorkspaceHostReloadSeesExternalStoreChange);
            Run("workspace host stop disposes runtime", WorkspaceHostStopDisposesRuntime);

            Console.WriteLine("PASS " + _passed + "/17");
            return 0;
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                _passed++;
                Console.WriteLine("PASS: " + name);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FAIL: " + name);
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }

        private static void EmptyStoreRestoresZeroContainers()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot loaded = store.Load();
            Equal(0, loaded.Containers.Count, "empty store container count");
        }

        private static void TwoContainersRoundTripExactly()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot source = BuildTwoContainerSnapshot();

            store.ReplaceAll(source);
            MeterWorkspaceSnapshot loaded = store.Load();

            AssertWorkspaceEqual(source, loaded);
        }

        private static void DeleteThenReplaceAllPurgesStaleContainer()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot source = BuildTwoContainerSnapshot();
            Guid deletedId = source.Containers[0].Id;
            Guid remainingId = source.Containers[1].Id;

            store.ReplaceAll(source);

            var reduced = new MeterWorkspaceSnapshot();
            reduced.Containers.Add(CloneContainer(source.Containers[1]));
            store.ReplaceAll(reduced);

            MeterWorkspaceSnapshot loaded = store.Load();
            Equal(1, loaded.Containers.Count, "reloaded container count");
            Equal(remainingId, loaded.Containers[0].Id, "remaining container id");
            RawTableDoesNotContain(table, deletedId.ToString("D"));
        }

        private static void ZeroContainersPurgesAllContainerIds()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot source = BuildTwoContainerSnapshot();
            string firstId = source.Containers[0].Id.ToString("D");
            string secondId = source.Containers[1].Id.ToString("D");

            store.ReplaceAll(source);
            store.ReplaceAll(new MeterWorkspaceSnapshot());

            MeterWorkspaceSnapshot loaded = store.Load();
            Equal(0, loaded.Containers.Count, "zero-container reload");
            RawTableDoesNotContain(table, firstId);
            RawTableDoesNotContain(table, secondId);
        }

        private static void UnsupportedReadingHasNoNumericValue()
        {
            MeterReadingResult result = MeterReadingResult.Unsupported("No proven KE9NS source.");
            False(result.IsSupported, "unsupported status");
            False(result.Value.HasValue, "unsupported numeric value must be null");
            Equal("No proven KE9NS source.", result.Reason, "unsupported reason");
        }

        private static void FakeTelemetrySignalChangesAreObservable()
        {
            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-113.5));
            double first = RequireValue(fake.Read(MeterReading.SignalStrength, MeterReceiver.Rx1));

            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-87.25));
            double second = RequireValue(fake.Read(MeterReading.SignalStrength, MeterReceiver.Rx1));

            Equal(-113.5, first, "first signal");
            Equal(-87.25, second, "second signal");
            True(second > first, "signal value did not change");
        }

        private static void Flex5000Rx1CalibrationMatchesNativeSum()
        {
            double value = Flex5000Rx1SignalCalibration.Apply(
                -110.25,
                2.5,
                6.0,
                1.25,
                -0.5,
                10.0,
                false,
                3.75);

            Equal(-91.0, value, "native RX1 calibrated signal");
        }

        private static void Flex5000Rx1LoopGainIsConditional()
        {
            double withoutLoop = Flex5000Rx1SignalCalibration.Apply(
                -110.25, 2.5, 6.0, 1.25, -0.5, 10.0, false, 3.75);
            double withLoop = Flex5000Rx1SignalCalibration.Apply(
                -110.25, 2.5, 6.0, 1.25, -0.5, 10.0, true, 3.75);

            Equal(-91.0, withoutLoop, "RX1 value without loop");
            Equal(-87.25, withLoop, "RX1 value with loop");
        }

        private static void LiveRuntimePropagatesChangingRx1Signal()
        {
            var fake = new FakeTelemetrySource();
            MeterWorkspaceSnapshot workspace = BuildLiveWorkspace();

            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-112.5));
            using (var runtime = new MeterLiveRuntime(fake, workspace))
            {
                int events = 0;
                runtime.Updated += delegate { events++; };

                MeterLiveSnapshot first = runtime.RefreshNow();
                MeterLiveValue firstValue;
                True(first.TryGetValue(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), out firstValue), "first live value missing");
                Equal(-112.5, RequireValue(firstValue.Result), "first live RX1 signal");

                fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-84.75));
                MeterLiveSnapshot second = runtime.RefreshNow();
                MeterLiveValue secondValue;
                True(second.TryGetValue(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), out secondValue), "second live value missing");
                Equal(-84.75, RequireValue(secondValue.Result), "second live RX1 signal");
                Equal(1L, first.Sequence, "first live sequence");
                Equal(2L, second.Sequence, "second live sequence");
                Equal(2, events, "live update event count");
            }
        }

        private static void LiveRuntimeDeduplicatesIdenticalRequests()
        {
            var fake = new FakeTelemetrySource();
            MeterWorkspaceSnapshot workspace = BuildLiveWorkspace();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-93.0));

            using (var runtime = new MeterLiveRuntime(fake, workspace))
            {
                MeterLiveSnapshot snapshot = runtime.RefreshNow();
                Equal(2, runtime.BindingCount, "bound live item count");
                Equal(1, fake.ReadCount(MeterReading.SignalStrength, MeterReceiver.Rx1), "RX1 signal source read count");
                Equal(2, snapshot.Values.Count, "live values count");
            }
        }

        private static void LiveRuntimePreservesUnsupportedWithoutZero()
        {
            var fake = new FakeTelemetrySource();
            MeterWorkspaceSnapshot workspace = BuildLiveWorkspace();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Unsupported("Radio off."));

            using (var runtime = new MeterLiveRuntime(fake, workspace))
            {
                MeterLiveSnapshot snapshot = runtime.RefreshNow();
                MeterLiveValue value;
                True(snapshot.TryGetValue(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), out value), "unsupported live value missing");
                False(value.Result.IsSupported, "unsupported live status");
                False(value.Result.Value.HasValue, "unsupported live value must stay null");
                Equal("Radio off.", value.Result.Reason, "unsupported live reason");
            }
        }

        private static void LiveRuntimeIgnoresUnboundItemTypes()
        {
            var fake = new FakeTelemetrySource();
            var workspace = new MeterWorkspaceSnapshot();
            var container = new MeterContainerSnapshot
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Receiver = MeterReceiver.Rx1,
                Geometry = new MeterWindowGeometry { X = 1, Y = 2, Width = 100, Height = 80 }
            };
            container.Items.Add(new MeterItemSnapshot
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Type = "NOT_IMPLEMENTED"
            });
            workspace.Containers.Add(container);

            using (var runtime = new MeterLiveRuntime(fake, workspace))
            {
                MeterLiveSnapshot snapshot = runtime.RefreshNow();
                Equal(0, runtime.BindingCount, "unbound item binding count");
                Equal(0, snapshot.Values.Count, "unbound item live values count");
                Equal(0, fake.TotalReadCount, "unbound item telemetry calls");
            }
        }

        private static void StoreSurvivesDataSetXmlRestart()
        {
            var first = new DataSet("PowerSDR");
            DataTable table = NewTable();
            first.Tables.Add(table);
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot expected = BuildTwoContainerSnapshot();
            store.ReplaceAll(expected);

            string xml;
            using (var writer = new StringWriter())
            {
                first.WriteXml(writer, XmlWriteMode.WriteSchema);
                xml = writer.ToString();
            }

            var restarted = new DataSet("PowerSDR");
            using (var reader = new StringReader(xml))
                restarted.ReadXml(reader, XmlReadMode.ReadSchema);

            True(restarted.Tables.Contains("FlexMeters"), "FlexMeters table missing after XML restart");
            var restartedStore = new DataTableMeterStore(restarted.Tables["FlexMeters"]);
            MeterWorkspaceSnapshot actual = restartedStore.Load();
            AssertWorkspaceEqual(expected, actual);
        }

        private static void WorkspaceHostStartsFromStore()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(BuildLiveWorkspace());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-101.25));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                True(host.IsStarted, "workspace host did not start");
                Equal(1, host.Workspace.Containers.Count, "workspace host container count");
                Equal(2, host.Runtime.BindingCount, "workspace host binding count");
                Equal(1, fake.ReadCount(MeterReading.SignalStrength, MeterReceiver.Rx1), "initial host RX1 source reads");

                MeterLiveValue value;
                True(host.Current.TryGetValue(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), out value), "initial host live value missing");
                Equal(-101.25, RequireValue(value.Result), "initial host live signal");
            }
        }

        private static void WorkspaceHostReplaceAllReloadsRuntime()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot initial = BuildTwoContainerSnapshot();
            store.ReplaceAll(initial);
            Guid deletedId = initial.Containers[1].Id;

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx1, MeterReadingResult.Supported(-99.0));
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx2, MeterReadingResult.Unsupported("RX2 not implemented."));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                MeterLiveRuntime firstRuntime = host.Runtime;

                MeterWorkspaceSnapshot replacement = BuildLiveWorkspace();
                host.ReplaceWorkspace(replacement);

                False(Object.ReferenceEquals(firstRuntime, host.Runtime), "workspace host did not replace runtime");
                Equal(1, host.Workspace.Containers.Count, "replacement workspace container count");
                Equal(2, host.Runtime.BindingCount, "replacement workspace binding count");
                RawTableDoesNotContain(table, deletedId.ToString("D"));

                Throws<ObjectDisposedException>(
                    delegate { firstRuntime.RefreshNow(); },
                    "replaced runtime was not disposed");
            }
        }

        private static void WorkspaceHostReloadSeesExternalStoreChange()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(BuildLiveWorkspace());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-90.0));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                Equal(1, host.Workspace.Containers.Count, "pre-reload workspace count");

                store.ReplaceAll(new MeterWorkspaceSnapshot());
                host.ReloadFromStore();

                Equal(0, host.Workspace.Containers.Count, "external store reload workspace count");
                Equal(0, host.Runtime.BindingCount, "external store reload binding count");
                Equal(0, host.Current.Values.Count, "external store reload live value count");
            }
        }

        private static void WorkspaceHostStopDisposesRuntime()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(BuildLiveWorkspace());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-95.0));

            var host = new MeterWorkspaceRuntimeHost(store, fake);
            host.Start(TimeSpan.FromHours(1));
            MeterLiveRuntime runtime = host.Runtime;

            host.Stop();
            False(host.IsStarted, "workspace host remained started after Stop");
            True(host.Runtime == null, "workspace host retained runtime after Stop");

            Throws<ObjectDisposedException>(
                delegate { runtime.RefreshNow(); },
                "stopped runtime was not disposed");

            host.Dispose();
        }

        private static MeterWorkspaceSnapshot BuildLiveWorkspace()
        {
            var workspace = new MeterWorkspaceSnapshot();
            var container = new MeterContainerSnapshot
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Receiver = MeterReceiver.Rx1,
                Geometry = new MeterWindowGeometry { X = 10, Y = 20, Width = 320, Height = 160 }
            };
            container.Items.Add(new MeterItemSnapshot
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Type = "SIGNAL_STRENGTH"
            });
            container.Items.Add(new MeterItemSnapshot
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Type = "SIGNAL_TEXT"
            });
            workspace.Containers.Add(container);
            return workspace;
        }

        private static DataTable NewTable()
        {
            return new DataTable("FlexMeters");
        }

        private static MeterWorkspaceSnapshot BuildTwoContainerSnapshot()
        {
            var snapshot = new MeterWorkspaceSnapshot();

            var rx1 = new MeterContainerSnapshot
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Receiver = MeterReceiver.Rx1,
                VisibleOnReceive = true,
                VisibleOnTransmit = false,
                Geometry = new MeterWindowGeometry { X = 25, Y = 35, Width = 420, Height = 180, Maximized = false }
            };
            var rx1Signal = new MeterItemSnapshot
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Type = "SIGNAL_STRENGTH"
            };
            rx1Signal.Settings.Add("style", "analog");
            rx1Signal.Settings.Add("label", "RX1");
            rx1.Items.Add(rx1Signal);

            var rx2 = new MeterContainerSnapshot
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Receiver = MeterReceiver.Rx2,
                VisibleOnReceive = true,
                VisibleOnTransmit = true,
                Geometry = new MeterWindowGeometry { X = 500, Y = 40, Width = 360, Height = 210, Maximized = true }
            };
            var rx2Signal = new MeterItemSnapshot
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Type = "SIGNAL_TEXT"
            };
            rx2Signal.Settings.Add("digits", "1");
            rx2Signal.Settings.Add("units", "dBm");
            rx2.Items.Add(rx2Signal);

            snapshot.Containers.Add(rx1);
            snapshot.Containers.Add(rx2);
            return snapshot;
        }

        private static MeterContainerSnapshot CloneContainer(MeterContainerSnapshot source)
        {
            var clone = new MeterContainerSnapshot
            {
                Id = source.Id,
                Receiver = source.Receiver,
                VisibleOnReceive = source.VisibleOnReceive,
                VisibleOnTransmit = source.VisibleOnTransmit,
                Geometry = new MeterWindowGeometry
                {
                    X = source.Geometry.X,
                    Y = source.Geometry.Y,
                    Width = source.Geometry.Width,
                    Height = source.Geometry.Height,
                    Maximized = source.Geometry.Maximized
                }
            };

            for (int i = 0; i < source.Items.Count; i++)
            {
                MeterItemSnapshot oldItem = source.Items[i];
                var newItem = new MeterItemSnapshot { Id = oldItem.Id, Type = oldItem.Type };
                foreach (KeyValuePair<string, string> setting in oldItem.Settings)
                    newItem.Settings.Add(setting.Key, setting.Value);
                clone.Items.Add(newItem);
            }

            return clone;
        }

        private static void AssertWorkspaceEqual(MeterWorkspaceSnapshot expected, MeterWorkspaceSnapshot actual)
        {
            Equal(expected.Containers.Count, actual.Containers.Count, "container count");

            for (int i = 0; i < expected.Containers.Count; i++)
            {
                MeterContainerSnapshot e = expected.Containers[i];
                MeterContainerSnapshot a = actual.Containers[i];
                Equal(e.Id, a.Id, "container id " + i);
                Equal(e.Receiver, a.Receiver, "receiver " + i);
                Equal(e.VisibleOnReceive, a.VisibleOnReceive, "RX visibility " + i);
                Equal(e.VisibleOnTransmit, a.VisibleOnTransmit, "TX visibility " + i);
                Equal(e.Geometry.X, a.Geometry.X, "geometry X " + i);
                Equal(e.Geometry.Y, a.Geometry.Y, "geometry Y " + i);
                Equal(e.Geometry.Width, a.Geometry.Width, "geometry width " + i);
                Equal(e.Geometry.Height, a.Geometry.Height, "geometry height " + i);
                Equal(e.Geometry.Maximized, a.Geometry.Maximized, "geometry maximized " + i);
                Equal(e.Items.Count, a.Items.Count, "item count " + i);

                for (int j = 0; j < e.Items.Count; j++)
                {
                    MeterItemSnapshot ei = e.Items[j];
                    MeterItemSnapshot ai = a.Items[j];
                    Equal(ei.Id, ai.Id, "item id " + i + "/" + j);
                    Equal(ei.Type, ai.Type, "item type " + i + "/" + j);
                    Equal(ei.Settings.Count, ai.Settings.Count, "setting count " + i + "/" + j);
                    foreach (KeyValuePair<string, string> setting in ei.Settings)
                    {
                        True(ai.Settings.ContainsKey(setting.Key), "missing setting " + setting.Key);
                        Equal(setting.Value, ai.Settings[setting.Key], "setting " + setting.Key);
                    }
                }
            }
        }

        private static void RawTableDoesNotContain(DataTable table, string text)
        {
            foreach (DataRow row in table.Rows)
            {
                string key = (string)row[DataTableMeterStore.KeyColumn];
                string value = (string)row[DataTableMeterStore.ValueColumn];
                False(key.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0, "stale ID remains in key: " + key);
                False(value.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0, "stale ID remains in value: " + value);
            }
        }

        private static double RequireValue(MeterReadingResult result)
        {
            True(result.IsSupported, "reading unexpectedly unsupported");
            True(result.Value.HasValue, "supported reading missing value");
            return result.Value.Value;
        }

        private static void True(bool condition, string message)
        {
            if (!condition)
                throw new Exception(message);
        }

        private static void False(bool condition, string message)
        {
            True(!condition, message);
        }

        private static void Equal<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception(message + ": expected=" + expected + " actual=" + actual);
        }

        private static void Throws<T>(Action action, string message) where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }

            throw new Exception(message + ": expected exception " + typeof(T).Name);
        }

        private sealed class FakeTelemetrySource : IMeterTelemetrySource
        {
            private readonly Dictionary<string, MeterReadingResult> _values =
                new Dictionary<string, MeterReadingResult>();
            private readonly Dictionary<string, int> _reads =
                new Dictionary<string, int>();

            public int TotalReadCount { get; private set; }

            public void Set(MeterReading reading, MeterReadingResult result)
            {
                Set(reading, MeterReceiver.Rx1, result);
            }

            public void Set(MeterReading reading, MeterReceiver receiver, MeterReadingResult result)
            {
                _values[Key(reading, receiver)] = result;
            }

            public int ReadCount(MeterReading reading, MeterReceiver receiver)
            {
                int count;
                return _reads.TryGetValue(Key(reading, receiver), out count) ? count : 0;
            }

            public MeterReadingResult Read(MeterReading reading, MeterReceiver receiver)
            {
                string key = Key(reading, receiver);
                int count;
                _reads.TryGetValue(key, out count);
                _reads[key] = count + 1;
                TotalReadCount++;

                MeterReadingResult result;
                if (_values.TryGetValue(key, out result))
                    return result;

                return MeterReadingResult.Unsupported("Fake source has no configured value for " + reading + " on " + receiver + ".");
            }

            private static string Key(MeterReading reading, MeterReceiver receiver)
            {
                return ((int)receiver).ToString() + ":" + ((int)reading).ToString();
            }
        }
    }
}
