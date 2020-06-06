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

                var pipeName = "bns_" + Guid.NewGuid().ToString().Replace("-", "");

                Environment.SetEnvironmentVariable("__COMPAT_LAYER", "RUNASINVOKER");
                Environment.SetEnvironmentVariable("BNS_PROFILE_XML", profile.ProfilePath);
                Environment.SetEnvironmentVariable("BNS_LOG", pipeName);

                var arguments = string.Join(" ", args);

                _logger.Log($"Starting game with arguments: {arguments}");
                _logger.Log($"Log pipe name: {pipeName}");

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = gameConfig.ClientPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    },
                    EnableRaisingEvents = true,
                };

                var namedPipeToLog = new NamedPipeToLog(_logger, pipeName);

                process.Exited += (sender, eventArgs) =>
                {
                    profile.RemoveProcess(process);
                    namedPipeToLog.Close();

                    _logger.Log($"Process {process.Id} has exited");
                };
                process.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (string.IsNullOrWhiteSpace(eventArgs.Data))
                        return;

                    _logger.Log(eventArgs.Data);
                };

                if (!process.Start())
                    return;

                namedPipeToLog.LogPrefix = $"(Pid: {process.Id}) ";
                namedPipeToLog.StartLogging();
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