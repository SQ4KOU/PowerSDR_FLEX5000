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

        [STAThread]
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
            Run("container manager add persists authoritative copy", ContainerManagerAddPersistsAuthoritativeCopy);
            Run("container manager remove purges restart state", ContainerManagerRemovePurgesRestartState);
            Run("container manager replace updates container", ContainerManagerReplaceUpdatesContainer);
            Run("container manager rejects duplicate ID without mutation", ContainerManagerRejectsDuplicateIdWithoutMutation);
            Run("container manager reload follows store", ContainerManagerReloadFollowsStore);
            Run("window host restores exactly persisted containers", WindowHostRestoresExactlyPersistedContainers);
            Run("window host geometry persists through manager", WindowHostGeometryPersistsThroughManager);
            Run("window host manager add/remove reconciles windows", WindowHostManagerAddRemoveReconcilesWindows);
            Run("window host remove stays removed after XML restart", WindowHostRemoveStaysRemovedAfterXmlRestart);
            Run("window host empty workspace creates zero windows", WindowHostEmptyWorkspaceCreatesZeroWindows);
            Run("window host displays live RX1 signal by item ID", WindowHostDisplaysLiveRx1SignalByItemId);
            Run("window host updates same item after refresh", WindowHostUpdatesSameItemAfterRefresh);
            Run("window host preserves unsupported text without zero", WindowHostPreservesUnsupportedTextWithoutZero);

            Console.WriteLine("PASS " + _passed + "/30");
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

        private static void ContainerManagerAddPersistsAuthoritativeCopy()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(new MeterWorkspaceSnapshot());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-88.0));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                int persistCount = 0;
                var manager = new MeterWorkspaceManager(host, delegate { persistCount++; });

                MeterContainerSnapshot container = BuildLiveWorkspace().Containers[0];
                manager.AddContainer(container);

                Equal(1, manager.ContainerCount, "manager add count");
                Equal(1, persistCount, "manager add persist callback");

                // Mutating the caller-owned object after Add must not mutate authoritative state.
                container.Geometry.Width = 9999;
                Equal(320, manager.Snapshot.Containers[0].Geometry.Width, "authoritative copy width");

                MeterWorkspaceSnapshot loaded = store.Load();
                Equal(1, loaded.Containers.Count, "manager add store count");
                Equal(320, loaded.Containers[0].Geometry.Width, "manager add stored width");
            }
        }

        private static void ContainerManagerRemovePurgesRestartState()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot initial = BuildTwoContainerSnapshot();
            store.ReplaceAll(initial);
            Guid removedId = initial.Containers[0].Id;

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx1, MeterReadingResult.Supported(-97.0));
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx2, MeterReadingResult.Unsupported("RX2 not implemented."));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(host, null);

                True(manager.RemoveContainer(removedId), "manager remove returned false");
                Equal(1, manager.ContainerCount, "manager remove count");
                RawTableDoesNotContain(table, removedId.ToString("D"));
            }

            string xml;
            var dataSet = new DataSet("PowerSDR");
            dataSet.Tables.Add(table.Copy());
            using (var writer = new StringWriter())
            {
                dataSet.WriteXml(writer, XmlWriteMode.WriteSchema);
                xml = writer.ToString();
            }

            var restarted = new DataSet("PowerSDR");
            using (var reader = new StringReader(xml))
                restarted.ReadXml(reader, XmlReadMode.ReadSchema);

            var restartedStore = new DataTableMeterStore(restarted.Tables["FlexMeters"]);
            MeterWorkspaceSnapshot loaded = restartedStore.Load();
            Equal(1, loaded.Containers.Count, "restart count after manager remove");
            False(loaded.Containers[0].Id == removedId, "removed container returned after restart");
        }

        private static void ContainerManagerReplaceUpdatesContainer()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot initial = BuildLiveWorkspace();
            store.ReplaceAll(initial);

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-92.0));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(host, null);

                MeterContainerSnapshot replacement = CloneContainer(manager.Snapshot.Containers[0]);
                replacement.Geometry.X = 777;
                replacement.Geometry.Width = 555;
                manager.ReplaceContainer(replacement);

                MeterWorkspaceSnapshot current = manager.Snapshot;
                Equal(777, current.Containers[0].Geometry.X, "manager replace X");
                Equal(555, current.Containers[0].Geometry.Width, "manager replace width");

                MeterWorkspaceSnapshot loaded = store.Load();
                Equal(777, loaded.Containers[0].Geometry.X, "stored replace X");
                Equal(555, loaded.Containers[0].Geometry.Width, "stored replace width");
            }
        }

        private static void ContainerManagerRejectsDuplicateIdWithoutMutation()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot initial = BuildLiveWorkspace();
            store.ReplaceAll(initial);

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-90.0));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(host, null);

                MeterContainerSnapshot duplicate = CloneContainer(manager.Snapshot.Containers[0]);
                Throws<InvalidOperationException>(
                    delegate { manager.AddContainer(duplicate); },
                    "duplicate container ID was accepted");

                Equal(1, manager.ContainerCount, "duplicate changed manager count");
                Equal(1, store.Load().Containers.Count, "duplicate changed store count");
            }
        }

        private static void ContainerManagerReloadFollowsStore()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(BuildLiveWorkspace());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-91.0));

            using (var host = new MeterWorkspaceRuntimeHost(store, fake))
            {
                host.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(host, null);
                Equal(1, manager.ContainerCount, "manager pre-reload count");

                store.ReplaceAll(new MeterWorkspaceSnapshot());
                manager.ReloadFromStore();

                Equal(0, manager.ContainerCount, "manager reload count");
                Equal(0, host.Runtime.BindingCount, "manager reload runtime binding count");
            }
        }

        private static void WindowHostRestoresExactlyPersistedContainers()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot expected = BuildTwoContainerSnapshot();
            store.ReplaceAll(expected);

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx1, MeterReadingResult.Supported(-89.0));
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx2, MeterReadingResult.Unsupported("RX2 not implemented."));

            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(runtimeHost, null);

                using (var windows = new WinFormsMeterWindowHost(null, manager, runtimeHost, false))
                {
                    windows.RestoreWindows(manager.Snapshot);
                    Equal(2, windows.OpenWindowCount, "restored WinForms window count");

                    MeterWindowGeometry geometry;
                    True(windows.TryCaptureGeometry(expected.Containers[0].Id, out geometry), "RX1 geometry missing");
                    Equal(expected.Containers[0].Geometry.X, geometry.X, "restored geometry X");
                    Equal(expected.Containers[0].Geometry.Y, geometry.Y, "restored geometry Y");
                    Equal(expected.Containers[0].Geometry.Width, geometry.Width, "restored geometry width");
                    Equal(expected.Containers[0].Geometry.Height, geometry.Height, "restored geometry height");
                }
            }
        }

        private static void WindowHostGeometryPersistsThroughManager()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot initial = BuildLiveWorkspace();
            store.ReplaceAll(initial);
            Guid id = initial.Containers[0].Id;

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-90.0));

            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                int persistCount = 0;
                var manager = new MeterWorkspaceManager(runtimeHost, delegate { persistCount++; });

                using (var windows = new WinFormsMeterWindowHost(null, manager, false))
                {
                    windows.RestoreWindows(manager.Snapshot);

                    var geometry = new MeterWindowGeometry
                    {
                        X = 321,
                        Y = 222,
                        Width = 640,
                        Height = 240,
                        Maximized = false
                    };

                    True(windows.SetWindowGeometry(id, geometry), "set window geometry returned false");
                    Equal(1, persistCount, "geometry persist callback");

                    MeterWorkspaceSnapshot loaded = store.Load();
                    Equal(321, loaded.Containers[0].Geometry.X, "stored window X");
                    Equal(222, loaded.Containers[0].Geometry.Y, "stored window Y");
                    Equal(640, loaded.Containers[0].Geometry.Width, "stored window width");
                    Equal(240, loaded.Containers[0].Geometry.Height, "stored window height");
                }
            }
        }

        private static void WindowHostManagerAddRemoveReconcilesWindows()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(new MeterWorkspaceSnapshot());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-91.0));

            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(runtimeHost, null);

                using (var windows = new WinFormsMeterWindowHost(null, manager, false))
                {
                    windows.RestoreWindows(manager.Snapshot);
                    Equal(0, windows.OpenWindowCount, "initial empty WinForms count");

                    MeterContainerSnapshot container = BuildLiveWorkspace().Containers[0];
                    manager.AddContainer(container);
                    Equal(1, windows.OpenWindowCount, "WinForms count after manager add");

                    True(manager.RemoveContainer(container.Id), "manager remove failed");
                    Equal(0, windows.OpenWindowCount, "WinForms count after manager remove");
                }
            }
        }

        private static void WindowHostRemoveStaysRemovedAfterXmlRestart()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            MeterWorkspaceSnapshot initial = BuildTwoContainerSnapshot();
            store.ReplaceAll(initial);
            Guid removedId = initial.Containers[0].Id;

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx1, MeterReadingResult.Supported(-92.0));
            fake.Set(MeterReading.SignalStrength, MeterReceiver.Rx2, MeterReadingResult.Unsupported("RX2 not implemented."));

            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(runtimeHost, null);

                using (var windows = new WinFormsMeterWindowHost(null, manager, false))
                {
                    windows.RestoreWindows(manager.Snapshot);
                    Equal(2, windows.OpenWindowCount, "pre-remove window count");
                    True(windows.RemoveWindow(removedId), "window-host remove failed");
                    Equal(1, windows.OpenWindowCount, "post-remove window count");
                }
            }

            var ds = new DataSet("PowerSDR");
            ds.Tables.Add(table.Copy());
            string xml;
            using (var writer = new StringWriter())
            {
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);
                xml = writer.ToString();
            }

            var restarted = new DataSet("PowerSDR");
            using (var reader = new StringReader(xml))
                restarted.ReadXml(reader, XmlReadMode.ReadSchema);

            var restartedStore = new DataTableMeterStore(restarted.Tables["FlexMeters"]);
            MeterWorkspaceSnapshot reloaded = restartedStore.Load();
            Equal(1, reloaded.Containers.Count, "restart window count");
            False(reloaded.Containers[0].Id == removedId, "removed WinForms container returned after restart");
        }

        private static void WindowHostEmptyWorkspaceCreatesZeroWindows()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(new MeterWorkspaceSnapshot());

            var fake = new FakeTelemetrySource();
            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(runtimeHost, null);

                using (var windows = new WinFormsMeterWindowHost(null, manager, false))
                {
                    windows.RestoreWindows(manager.Snapshot);
                    Equal(0, windows.OpenWindowCount, "empty workspace fabricated a default window");
                }
            }
        }

        private static void WindowHostDisplaysLiveRx1SignalByItemId()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(BuildLiveWorkspace());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-104.5));

            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(runtimeHost, null);

                using (var windows = new WinFormsMeterWindowHost(null, manager, runtimeHost, false))
                {
                    windows.RestoreWindows(manager.Snapshot);

                    string text;
                    True(
                        windows.TryGetDisplayedItemText(
                            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            out text),
                        "RX1 signal item label missing");
                    Equal("SIGNAL_STRENGTH: -104.5 dBm", text, "RX1 diagnostic label text");

                    string secondText;
                    True(
                        windows.TryGetDisplayedItemText(
                            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                            out secondText),
                        "RX1 signal text item label missing");
                    Equal("SIGNAL_TEXT: -104.5 dBm", secondText, "RX1 signal-text diagnostic label");
                }
            }
        }

        private static void WindowHostUpdatesSameItemAfterRefresh()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(BuildLiveWorkspace());

            var fake = new FakeTelemetrySource();
            fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-111.0));

            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(runtimeHost, null);

                using (var windows = new WinFormsMeterWindowHost(null, manager, runtimeHost, false))
                {
                    windows.RestoreWindows(manager.Snapshot);
                    Guid itemId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

                    string before;
                    True(windows.TryGetDisplayedItemText(itemId, out before), "live item missing before refresh");
                    Equal("SIGNAL_STRENGTH: -111.0 dBm", before, "live item before refresh");

                    fake.Set(MeterReading.SignalStrength, MeterReadingResult.Supported(-73.25));
                    runtimeHost.RefreshNow();

                    string after;
                    True(windows.TryGetDisplayedItemText(itemId, out after), "live item missing after refresh");
                    Equal("SIGNAL_STRENGTH: -73.3 dBm", after, "live item after refresh");
                    False(before == after, "live item text did not change");
                }
            }
        }

        private static void WindowHostPreservesUnsupportedTextWithoutZero()
        {
            var table = NewTable();
            var store = new DataTableMeterStore(table);
            store.ReplaceAll(BuildLiveWorkspace());

            var fake = new FakeTelemetrySource();
            fake.Set(
                MeterReading.SignalStrength,
                MeterReadingResult.Unsupported("Radio off."));

            using (var runtimeHost = new MeterWorkspaceRuntimeHost(store, fake))
            {
                runtimeHost.Start(TimeSpan.FromHours(1));
                var manager = new MeterWorkspaceManager(runtimeHost, null);

                using (var windows = new WinFormsMeterWindowHost(null, manager, runtimeHost, false))
                {
                    windows.RestoreWindows(manager.Snapshot);

                    string text;
                    True(
                        windows.TryGetDisplayedItemText(
                            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                            out text),
                        "unsupported item label missing");
                    True(text.Contains("UNSUPPORTED"), "unsupported status is not visible");
                    True(text.Contains("Radio off."), "unsupported reason is not visible");
                    False(text.Contains("0.0 dBm"), "unsupported value was fabricated as zero");
                }
            }
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
