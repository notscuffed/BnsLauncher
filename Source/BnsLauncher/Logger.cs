using System;
using System.Diagnostics;
using BnsLauncher.Core.Abstractions;

namespace BnsLauncher
{
    public class Logger : ILogger
    {
        public void Log(string text)
        {
            Debug.WriteLine(text);
        }

        public void Log(Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}