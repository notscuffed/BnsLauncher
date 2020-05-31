using System;
using System.Collections.Generic;
using System.Diagnostics;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Services
{
    public class GameStarter : IGameStarter
    {
        private readonly ILogger _logger;

        public GameStarter(ILogger logger)
        {
            _logger = logger;
        }

        public void Start(Profile profile, GameConfig gameConfig)
        {
            try
            {
                var args = new List<string>
                {
                    "/LaunchByLauncher",
                    "/LoginMode", "2"
                };
                
                if (gameConfig.Unattended)
                    args.Add("-UNATTENDED");
                
                if (gameConfig.NoTextureStreaming)
                    args.Add("-NOTEXTURESTREAMING");
                
                if (gameConfig.UseAllCores)
                    args.Add("-USEALLAVAILABLECORES");

                Environment.SetEnvironmentVariable("__COMPAT_LAYER", "RUNASINVOKER");
                Environment.SetEnvironmentVariable("BNS_PROFILE_XML", profile.ProfilePath);
                
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = gameConfig.ClientPath,
                        Arguments = string.Join(" ", args),
                        UseShellExecute = false
                    },
                    EnableRaisingEvents = true
                };

                process.Exited += (sender, eventArgs) => profile.RemoveProcess(process);

                process.Start();

                profile.AddProcess(process);
            }
            catch (Exception exception)
            {
                _logger.Log("Exception occured while starting process");
                _logger.Log(exception);
            }
        }
    }
}