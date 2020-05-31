using System;

namespace BnsLauncher.Core.Abstractions
{
    public interface ILogger
    {
        void Log(string text);
        void Log(Exception exception);
    }
}