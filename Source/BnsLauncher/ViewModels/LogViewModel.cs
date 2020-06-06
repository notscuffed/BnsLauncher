using System.Collections.ObjectModel;
using BnsLauncher.Logging;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class LogViewModel : Screen
    {
        public ObservableCollection<LogEntry> LogEntries { get; } = new ObservableCollection<LogEntry>();
    }
}