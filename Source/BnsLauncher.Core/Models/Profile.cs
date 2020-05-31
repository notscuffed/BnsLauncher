using System.Collections.Generic;
using System.Diagnostics;

namespace BnsLauncher.Core.Models
{
    public class Profile : PropertyChangedBase
    {
        private readonly object _processesLocker = new object();
        private readonly List<Process> _processes = new List<Process>();

        public string ProfilePath { get; set; }
        
        public string Name { get; set; }
        public string Background { get; set; }
        public string Foreground { get; set; }

        public string Ip { get; set; }
        public ushort Port { get; set; }

        public string IpPort => $"{Ip}:{Port}";

        public Process[] Processes
        {
            get
            {
                lock (_processesLocker)
                    return _processes.ToArray();
            }
        }

        public char Initial => string.IsNullOrWhiteSpace(Name) ? ' ' : Name[0];

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