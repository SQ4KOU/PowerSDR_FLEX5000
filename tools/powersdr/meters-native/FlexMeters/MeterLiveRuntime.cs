using System;
using System.Collections.Generic;
using System.Threading;

namespace FlexMeters
{
    public sealed class MeterLiveValue
    {
        internal MeterLiveValue(
            Guid containerId,
            Guid itemId,
            MeterReceiver receiver,
            MeterReading reading,
            MeterReadingResult result)
        {
            ContainerId = containerId;
            ItemId = itemId;
            Receiver = receiver;
            Reading = reading;
            Result = result;
        }

        public Guid ContainerId { get; private set; }
        public Guid ItemId { get; private set; }
        public MeterReceiver Receiver { get; private set; }
        public MeterReading Reading { get; private set; }
        public MeterReadingResult Result { get; private set; }
    }

    public sealed class MeterLiveSnapshot
    {
        private readonly IDictionary<Guid, MeterLiveValue> _values;

        internal MeterLiveSnapshot(long sequence, IDictionary<Guid, MeterLiveValue> values)
        {
            Sequence = sequence;
            _values = new Dictionary<Guid, MeterLiveValue>(values);
        }

        public long Sequence { get; private set; }
        public IDictionary<Guid, MeterLiveValue> Values
        {
            get { return new Dictionary<Guid, MeterLiveValue>(_values); }
        }

        public bool TryGetValue(Guid itemId, out MeterLiveValue value)
        {
            return _values.TryGetValue(itemId, out value);
        }
    }

    public sealed class MeterLiveSnapshotEventArgs : EventArgs
    {
        internal MeterLiveSnapshotEventArgs(MeterLiveSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public MeterLiveSnapshot Snapshot { get; private set; }
    }

    public sealed class MeterLiveRuntime : IDisposable
    {
        private sealed class Binding
        {
            public Guid ContainerId;
            public Guid ItemId;
            public MeterReceiver Receiver;
            public MeterReading Reading;
        }

        private struct RequestKey : IEquatable<RequestKey>
        {
            public MeterReceiver Receiver;
            public MeterReading Reading;

            public bool Equals(RequestKey other)
            {
                return Receiver == other.Receiver && Reading == other.Reading;
            }

            public override bool Equals(object obj)
            {
                return obj is RequestKey && Equals((RequestKey)obj);
            }

            public override int GetHashCode()
            {
                return ((int)Receiver * 397) ^ (int)Reading;
            }
        }

        private readonly object _sync = new object();
        private readonly IMeterTelemetrySource _source;
        private readonly List<Binding> _bindings;
        private Timer _timer;
        private long _sequence;
        private MeterLiveSnapshot _current;
        private bool _disposed;

        public MeterLiveRuntime(IMeterTelemetrySource source, MeterWorkspaceSnapshot workspace)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (workspace == null)
                throw new ArgumentNullException("workspace");

            MeterWorkspaceValidator.Validate(workspace);

            _source = source;
            _bindings = BuildBindings(workspace);
            _current = new MeterLiveSnapshot(0, new Dictionary<Guid, MeterLiveValue>());
        }

        public event EventHandler<MeterLiveSnapshotEventArgs> Updated;

        public MeterLiveSnapshot Current
        {
            get
            {
                lock (_sync)
                {
                    return _current;
                }
            }
        }

        public int BindingCount
        {
            get { return _bindings.Count; }
        }

        public void Start(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("interval");

            lock (_sync)
            {
                ThrowIfDisposed();
                if (_timer != null)
                    throw new InvalidOperationException("Meter live runtime is already started.");

                _timer = new Timer(TimerTick, null, interval, interval);
            }
        }

        public void Stop()
        {
            Timer timer = null;
            lock (_sync)
            {
                if (_timer != null)
                {
                    timer = _timer;
                    _timer = null;
                }
            }

            if (timer != null)
                timer.Dispose();
        }

        public MeterLiveSnapshot RefreshNow()
        {
            ThrowIfDisposed();

            var results = new Dictionary<RequestKey, MeterReadingResult>();
            for (int i = 0; i < _bindings.Count; i++)
            {
                Binding binding = _bindings[i];
                var key = new RequestKey { Receiver = binding.Receiver, Reading = binding.Reading };
                if (!results.ContainsKey(key))
                    results.Add(key, _source.Read(binding.Reading, binding.Receiver));
            }

            var values = new Dictionary<Guid, MeterLiveValue>();
            for (int i = 0; i < _bindings.Count; i++)
            {
                Binding binding = _bindings[i];
                var key = new RequestKey { Receiver = binding.Receiver, Reading = binding.Reading };
                values.Add(
                    binding.ItemId,
                    new MeterLiveValue(
                        binding.ContainerId,
                        binding.ItemId,
                        binding.Receiver,
                        binding.Reading,
                        results[key]));
            }

            MeterLiveSnapshot snapshot;
            EventHandler<MeterLiveSnapshotEventArgs> handler;
            lock (_sync)
            {
                ThrowIfDisposed();
                _sequence++;
                snapshot = new MeterLiveSnapshot(_sequence, values);
                _current = snapshot;
                handler = Updated;
            }

            if (handler != null)
                handler(this, new MeterLiveSnapshotEventArgs(snapshot));

            return snapshot;
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }

            Stop();
        }

        private void TimerTick(object state)
        {
            try
            {
                RefreshNow();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static List<Binding> BuildBindings(MeterWorkspaceSnapshot workspace)
        {
            var bindings = new List<Binding>();

            for (int i = 0; i < workspace.Containers.Count; i++)
            {
                MeterContainerSnapshot container = workspace.Containers[i];

                for (int j = 0; j < container.Items.Count; j++)
                {
                    MeterItemSnapshot item = container.Items[j];
                    MeterReading reading;
                    if (!TryResolveReading(item.Type, out reading))
                        continue;

                    bindings.Add(new Binding
                    {
                        ContainerId = container.Id,
                        ItemId = item.Id,
                        Receiver = container.Receiver,
                        Reading = reading
                    });
                }
            }

            return bindings;
        }

        private static bool TryResolveReading(string itemType, out MeterReading reading)
        {
            MeterItemDescriptor descriptor;
            if (MeterItemCatalog.TryGet(itemType, out descriptor))
            {
                reading = descriptor.Reading;
                return true;
            }

            reading = default(MeterReading);
            return false;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("MeterLiveRuntime");
        }
    }
}
