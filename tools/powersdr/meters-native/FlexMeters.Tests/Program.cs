using System;
using System.Collections.Generic;
using System.Data;
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

            Console.WriteLine("PASS " + _passed + "/8");
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

        private sealed class FakeTelemetrySource : IMeterTelemetrySource
        {
            private readonly Dictionary<MeterReading, MeterReadingResult> _values =
                new Dictionary<MeterReading, MeterReadingResult>();

            public void Set(MeterReading reading, MeterReadingResult result)
            {
                _values[reading] = result;
            }

            public MeterReadingResult Read(MeterReading reading, MeterReceiver receiver)
            {
                MeterReadingResult result;
                if (_values.TryGetValue(reading, out result))
                    return result;

                return MeterReadingResult.Unsupported("Fake source has no configured value for " + reading + ".");
            }
        }
    }
}
