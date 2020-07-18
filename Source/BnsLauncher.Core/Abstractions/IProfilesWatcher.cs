using System;

namespace BnsLauncher.Core.Abstractions
{
    public interface IProfilesWatcher
    {
        public event Action OnProfileChange;
        void WatchForChanges();
    }
}