using System;

namespace FlexMeters
{
    public sealed class MeterWorkspaceRuntimeHost : IDisposable
    {
        private readonly object _sync = new object();
        private readonly IMeterStore _store;
        private readonly IMeterTelemetrySource _source;

        private MeterLiveRuntime _runtime;
        private MeterWorkspaceSnapshot _workspace;
        private TimeSpan _interval;
        private bool _started;
        private bool _disposed;

        public MeterWorkspaceRuntimeHost(
            IMeterStore store,
            IMeterTelemetrySource source)
        {
            if (store == null)
                throw new ArgumentNullException("store");
            if (source == null)
                throw new ArgumentNullException("source");

            _store = store;
            _source = source;
            _workspace = new MeterWorkspaceSnapshot();
        }

        public event EventHandler<MeterLiveSnapshotEventArgs> Updated;

        public bool IsStarted
        {
            get
            {
                lock (_sync)
                    return _started;
            }
        }

        public MeterWorkspaceSnapshot Workspace
        {
            get
            {
                lock (_sync)
                    return _workspace;
            }
        }

        public MeterLiveRuntime Runtime
        {
            get
            {
                lock (_sync)
                    return _runtime;
            }
        }

        public MeterLiveSnapshot Current
        {
            get
            {
                lock (_sync)
                    return _runtime == null ? null : _runtime.Current;
            }
        }

        public void Start(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("interval");

            lock (_sync)
            {
                ThrowIfDisposed();
                if (_started)
                    throw new InvalidOperationException("Meter workspace runtime host is already started.");

                _interval = interval;
                _started = true;
            }

            try
            {
                ReloadFromStore();
            }
            catch
            {
                lock (_sync)
                    _started = false;
                throw;
            }
        }

        public void ReplaceWorkspace(MeterWorkspaceSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException("snapshot");

            ThrowIfDisposed();
            _store.ReplaceAll(snapshot);

            if (IsStarted)
                ReloadFromStore();
            else
                _workspace = _store.Load();
        }

        public void ReloadFromStore()
        {
            ThrowIfDisposed();

            TimeSpan interval;
            lock (_sync)
            {
                if (!_started)
                    throw new InvalidOperationException("Meter workspace runtime host is not started.");
                interval = _interval;
            }

            MeterWorkspaceSnapshot workspace = _store.Load();
            var replacement = new MeterLiveRuntime(_source, workspace);
            replacement.Updated += RuntimeUpdated;

            try
            {
                replacement.RefreshNow();
                replacement.Start(interval);
            }
            catch
            {
                replacement.Updated -= RuntimeUpdated;
                replacement.Dispose();
                throw;
            }

            MeterLiveRuntime previous;
            lock (_sync)
            {
                if (!_started || _disposed)
                {
                    replacement.Updated -= RuntimeUpdated;
                    replacement.Dispose();
                    ThrowIfDisposed();
                    throw new InvalidOperationException("Meter workspace runtime host stopped during reload.");
                }

                previous = _runtime;
                _runtime = replacement;
                _workspace = workspace;
            }

            if (previous != null)
            {
                previous.Updated -= RuntimeUpdated;
                previous.Dispose();
            }
        }

        public MeterLiveSnapshot RefreshNow()
        {
            MeterLiveRuntime runtime;
            lock (_sync)
            {
                ThrowIfDisposed();
                if (!_started || _runtime == null)
                    throw new InvalidOperationException("Meter workspace runtime host is not started.");
                runtime = _runtime;
            }

            return runtime.RefreshNow();
        }

        public void Stop()
        {
            MeterLiveRuntime runtime;
            lock (_sync)
            {
                if (!_started)
                    return;

                _started = false;
                runtime = _runtime;
                _runtime = null;
            }

            if (runtime != null)
            {
                runtime.Updated -= RuntimeUpdated;
                runtime.Dispose();
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;
            }

            Stop();

            lock (_sync)
                _disposed = true;
        }

        private void RuntimeUpdated(object sender, MeterLiveSnapshotEventArgs e)
        {
            EventHandler<MeterLiveSnapshotEventArgs> handler = Updated;
            if (handler != null)
                handler(this, e);
        }

        private void ThrowIfDisposed()
        {
            lock (_sync)
            {
                if (_disposed)
                    throw new ObjectDisposedException("MeterWorkspaceRuntimeHost");
            }
        }
    }
}
