using System;

namespace BnsLauncher.Logging
{
    public class LogEntry
    {
        public string Text { get; set; }
        public string Color { get; set; } = "#AAA";
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}