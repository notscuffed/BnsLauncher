using System;
using System.Collections.Generic;
using System.Windows;
using BnsLauncher.Core.Abstractions;

namespace BnsLauncher.Logging
{
    public class Logger : ILogger
    {
        private readonly ICollection<LogEntry> _logEntriesCollection;

        public Logger(ICollection<LogEntry> logEntriesCollection)
        {
            _logEntriesCollection = logEntriesCollection;
        }

        public void Log(string text)
        {
            Application.Current.Dispatcher.Invoke(() => _logEntriesCollection.Add(
                new LogEntry
                {
                    Text = text
                }
            ));
        }

        public void Log(Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() => _logEntriesCollection.Add(
                new LogEntry
                {
                    Text = exception.ToString(),
                    Color = "#B66"
                }
            ));
        }
    }
}