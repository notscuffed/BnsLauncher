using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using BnsLauncher.Core.Abstractions;

namespace BnsLauncher.Core.Services
{
    public class ProfilesWatcher : IProfilesWatcher
    {
        private readonly IFileSystem _fs;
        private readonly string _profileDirectory;
        private readonly FileSystemWatcher _watcher;

        public ProfilesWatcher(string profileDirectory, IFileSystem fs)
        {
            _fs = fs;
            
            _profileDirectory = fs.Path.GetFullPath(profileDirectory);
            _fs.Directory.CreateDirectory(_profileDirectory);
            
            _watcher = new FileSystemWatcher
            {
                Path = _profileDirectory,
                IncludeSubdirectories = true,
            };

            _watcher.Changed += WatcherOnChanged;
            _watcher.Created += WatcherOnChanged;
            _watcher.Deleted += WatcherOnChanged;
            _watcher.Renamed += WatcherOnRenamed;
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine($"{e.OldFullPath} -> {e.FullPath}");
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            var fullPath = e.FullPath.Substring(_profileDirectory.Length + 1);

            var firstSlash = fullPath.IndexOf('\\');
            if (firstSlash == -1)
                return;

            Debug.WriteLine($"{e.ChangeType} {e.FullPath} {fullPath}");
        }

        public void WatchForChanges()
        {
            _watcher.EnableRaisingEvents = true;
        }
    }
}