using System.Collections.Generic;
using System.Diagnostics;

namespace BnsLauncher.Core.Models
{
    public class Profile : PropertyChangedBase
    {
        private readonly object _processesLocker = new object();
        private readonly List<Process> _processes = new List<Process>();

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
        public bool HasBins => 
            !string.IsNullOrWhiteSpace(BinPath) &&
            !string.IsNullOrWhiteSpace(LocalBinPath);

        public Process[] Processes
        {
            get
            {
                lock (_processesLocker)
                    return _processes.ToArray();
            }
        }

        public void AddProcess(Process process)
        {
            lock (_processesLocker)
            {
                _processes.Add(process);
            }
            
            OnPropertyChanged(nameof(Processes));
        }

        public void RemoveProcess(Process process)
        {
            lock (_processesLocker)
            {
                _processes.Remove(process);
            }
            
            OnPropertyChanged(nameof(Processes));
        }
    }
}