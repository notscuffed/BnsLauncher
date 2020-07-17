using System.Diagnostics;

namespace BnsLauncher.Core.Models
{
    public class ProcessInfo : PropertyChangedBase
    {
        public Process Process { get; set; }
        public Account Account { get; set; }
    }
}