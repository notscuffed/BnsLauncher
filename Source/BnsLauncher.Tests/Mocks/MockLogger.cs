using System;
using BnsLauncher.Core.Abstractions;

namespace BnsLauncher.Tests.Mocks
{
    public class MockLogger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }

        public void Log(Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}