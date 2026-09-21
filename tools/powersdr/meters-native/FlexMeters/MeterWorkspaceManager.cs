using System;
using System.Collections.Generic;

namespace FlexMeters
{
    internal static class MeterWorkspaceCopy
    {
        public static MeterWorkspaceSnapshot Clone(MeterWorkspaceSnapshot source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var clone = new MeterWorkspaceSnapshot();

            for (int i = 0; i < source.Containers.Count; i++)
                clone.Containers.Add(CloneContainer(source.Containers[i]));

            return clone;
        }

        public static MeterContainerSnapshot CloneContainer(MeterContainerSnapshot source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

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
                MeterItemSnapshot item = source.Items[i];
                var itemClone = new MeterItemSnapshot
                {
                    Id = item.Id,
                    Type = item.Type
                };

                foreach (KeyValuePair<string, string> setting in item.Settings)
                    itemClone.Settings.Add(setting.Key, setting.Value);

                clone.Items.Add(itemClone);
            }

            return clone;
        }
    }

    public sealed class MeterWorkspaceChangedEventArgs : EventArgs
    {
        internal MeterWorkspaceChangedEventArgs(MeterWorkspaceSnapshot workspace)
        {
            Workspace = MeterWorkspaceCopy.Clone(workspace);
        }

        public MeterWorkspaceSnapshot Workspace { get; private set; }
    }

    public sealed class MeterWorkspaceManager
    {
        private readonly object _sync = new object();
        private readonly MeterWorkspaceRuntimeHost _host;
        private readonly Action _persistCommittedStore;
        private MeterWorkspaceSnapshot _workspace;

        public MeterWorkspaceManager(
            MeterWorkspaceRuntimeHost host,
            Action persistCommittedStore)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            if (!host.IsStarted)
                throw new InvalidOperationException(
                    "Meter workspace manager requires a started runtime host.");

            _host = host;
            _persistCommittedStore = persistCommittedStore;
            _workspace = host.Workspace;
        }

        public event EventHandler<MeterWorkspaceChangedEventArgs> WorkspaceChanged;

        public MeterWorkspaceSnapshot Snapshot
        {
            get
            {
                lock (_sync)
                    return MeterWorkspaceCopy.Clone(_workspace);
            }
        }

        public int ContainerCount
        {
            get
            {
                lock (_sync)
                    return _workspace.Containers.Count;
            }
        }

        public void ReplaceAll(MeterWorkspaceSnapshot workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException("workspace");

            Commit(MeterWorkspaceCopy.Clone(workspace));
        }

        public void AddContainer(MeterContainerSnapshot container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            lock (_sync)
            {
                MeterWorkspaceSnapshot candidate = MeterWorkspaceCopy.Clone(_workspace);
                candidate.Containers.Add(MeterWorkspaceCopy.CloneContainer(container));
                CommitLocked(candidate);
            }
        }

        public bool RemoveContainer(Guid containerId)
        {
            if (containerId == Guid.Empty)
                throw new ArgumentException("Container ID cannot be empty.", "containerId");

            lock (_sync)
            {
                MeterWorkspaceSnapshot candidate = MeterWorkspaceCopy.Clone(_workspace);
                int index = FindContainerIndex(candidate, containerId);
                if (index < 0)
                    return false;

                candidate.Containers.RemoveAt(index);
                CommitLocked(candidate);
                return true;
            }
        }

        public void ReplaceContainer(MeterContainerSnapshot container)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            if (container.Id == Guid.Empty)
                throw new ArgumentException("Container ID cannot be empty.", "container");

            lock (_sync)
            {
                MeterWorkspaceSnapshot candidate = MeterWorkspaceCopy.Clone(_workspace);
                int index = FindContainerIndex(candidate, container.Id);
                if (index < 0)
                    throw new KeyNotFoundException(
                        "Meter container does not exist: " + container.Id);

                candidate.Containers[index] = MeterWorkspaceCopy.CloneContainer(container);
                CommitLocked(candidate);
            }
        }

        public void ReloadFromStore()
        {
            MeterWorkspaceSnapshot changed;
            EventHandler<MeterWorkspaceChangedEventArgs> handler;

            lock (_sync)
            {
                _host.ReloadFromStore();
                _workspace = _host.Workspace;
                changed = MeterWorkspaceCopy.Clone(_workspace);
                handler = WorkspaceChanged;
            }

            if (handler != null)
                handler(this, new MeterWorkspaceChangedEventArgs(changed));
        }

        private void Commit(MeterWorkspaceSnapshot candidate)
        {
            lock (_sync)
                CommitLocked(candidate);
        }

        private void CommitLocked(MeterWorkspaceSnapshot candidate)
        {
            MeterWorkspaceValidator.Validate(candidate);

            _host.ReplaceWorkspace(candidate);
            _workspace = _host.Workspace;

            if (_persistCommittedStore != null)
                _persistCommittedStore();

            EventHandler<MeterWorkspaceChangedEventArgs> handler = WorkspaceChanged;
            if (handler != null)
            {
                MeterWorkspaceSnapshot changed = MeterWorkspaceCopy.Clone(_workspace);
                handler(this, new MeterWorkspaceChangedEventArgs(changed));
            }
        }

        private static int FindContainerIndex(
            MeterWorkspaceSnapshot workspace,
            Guid containerId)
        {
            for (int i = 0; i < workspace.Containers.Count; i++)
            {
                if (workspace.Containers[i].Id == containerId)
                    return i;
            }

            return -1;
        }
    }
}
