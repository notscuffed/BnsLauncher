using System.Collections.Generic;
using System.Linq;

namespace BnsLauncher.Core.Models
{
    public class Profile : PropertyChangedBase
    {
        private readonly object _processesLocker = new object();
        private readonly Dictionary<int, ProcessInfo> _processInfos = new Dictionary<int, ProcessInfo>();

        public string ProfilePath { get; set; }
        public string BnsPatchPath { get; set; }

        public string Name { get; set; }
        public char Initial => string.IsNullOrWhiteSpace(Name) ? ' ' : Name[0];

        public string Background { get; set; }
        public string Foreground { get; set; }
        public string ClientPath { get; set; }
        public string Arguments { get; set; }

        public string Ip { get; set; }
        public ushort Port { get; set; }

        public string IpPort => Port == 0 ? "" : $"{Ip}:{Port}";

        public string BinPath { get; set; }
        public string LocalBinPath { get; set; }
        
        public int Priority { get; set; }

        public bool HasBins =>
            !string.IsNullOrWhiteSpace(BinPath) &&
            !string.IsNullOrWhiteSpace(LocalBinPath);

        public bool AllowAccounts { get; set; }
        public bool AllowPin { get; set; }
        public bool AutopinOnRelog { get; set; }

        public ProcessInfo[] ProcessInfos
        {
            get
            {
                lock (_processesLocker)
                    return _processInfos.Values.ToArray();
            }
        }

        public void AddProcessInfo(ProcessInfo processInfo)
        {
            lock (_processesLocker)
            {
                _processInfos[processInfo.Process.Id] = processInfo;
            }

            OnPropertyChanged(nameof(ProcessInfos));
        }

        public bool HasProcessWithId(int pid)
        {
            lock (_processesLocker)
            {
                return _processInfos.Keys.Contains(pid);
            }
        }

        public void RemoveProcessWithId(int pid)
        {
            lock (_processesLocker)
            {
                if (!_processInfos.Remove(pid))
                    return;
            }

            OnPropertyChanged(nameof(ProcessInfos));
        }
    }
}