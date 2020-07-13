using System;
using System.Diagnostics;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Abstractions
{
    public interface IGameStarter
    {
        void Start(Profile profile, GlobalConfig globalConfig);
        event Action<Process> OnProcessExit;
    }
}