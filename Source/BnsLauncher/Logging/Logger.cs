using System;
using System.Collections.Generic;
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
            _logEntriesCollection.Add(new LogEntry {Text = text});
        }

        public void Log(Exception exception)
        {
            _logEntriesCollection.Add(new LogEntry {Text = exception.ToString(), Color = "#B66"});
        }
    }
}