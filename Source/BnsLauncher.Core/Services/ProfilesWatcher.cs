using System;
using System.IO;
using System.IO.Abstractions;
using BnsLauncher.Core.Abstractions;

namespace BnsLauncher.Core.Services
{
    public class ProfilesWatcher : IProfilesWatcher
    {
        private readonly FileSystemWatcher _watcher;

        public ProfilesWatcher(string profileDirectory, IFileSystem fs)
        {
            profileDirectory = fs.Path.GetFullPath(profileDirectory);
            fs.Directory.CreateDirectory(profileDirectory);

            _watcher = new FileSystemWatcher
            {
                Path = profileDirectory,
                IncludeSubdirectories = true,
            };

            _watcher.Changed += WatcherOnChanged;
            _watcher.Created += WatcherOnChanged;
            _watcher.Deleted += WatcherOnChanged;
            _watcher.Renamed += WatcherOnRenamed;
        }

        public event Action OnProfileChange;

        private void WatcherOnRenamed(object sender, RenamedEventArgs e) => OnProfileChange?.Invoke();
        private void WatcherOnChanged(object sender, FileSystemEventArgs e) => OnProfileChange?.Invoke();

        public void WatchForChanges()
        {
            _watcher.EnableRaisingEvents = true;
        }
    }
}