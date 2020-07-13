using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Services
{
    public class GameStarter : IGameStarter
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fs;

        public GameStarter(ILogger logger, IFileSystem fs)
        {
            _logger = logger;
            _fs = fs;
        }

        public event Action<Process> OnProcessExit;

        public void Start(Profile profile, GlobalConfig globalConfig)
        {
            try
            {
                string clientPath;

                // Get client path
                if (!string.IsNullOrWhiteSpace(profile.ClientPath))
                {
                    _logger.Log($"Using profile client path : '{profile.ClientPath}'");
                    clientPath = profile.ClientPath;
                }
                else if (!string.IsNullOrWhiteSpace(globalConfig.ClientPath))
                {
                    _logger.Log($"Using global client path: '{globalConfig.ClientPath}'");
                    clientPath = globalConfig.ClientPath;
                }
                else
                {
                    _logger.Log("Client path is not set in either profile or global config");
                    return;
                }

                // Make sure client exists
                if (string.IsNullOrWhiteSpace(clientPath))
                {
                    _logger.Log("Client path is empty");
                    return;
                }

                if (!_fs.File.Exists(clientPath))
                {
                    _logger.Log($"Client doesn't exist at path: '{clientPath}'");
                    return;
                }

                // Get arguments
                var args = new List<string>();

                if (profile.Arguments != null)
                {
                    _logger.Log($"Using profile arguments: '{profile.Arguments}'");
                    args.Add(profile.Arguments);
                }
                else if (globalConfig.Arguments != null)
                {
                    _logger.Log($"Using global arguments: '{globalConfig.Arguments}'");
                    args.Add(globalConfig.Arguments);
                }
                else
                {
                    _logger.Log("No arguments found in profile/global config");
                }

                if (globalConfig.Unattended)
                    args.Add("-UNATTENDED");

                if (globalConfig.NoTextureStreaming)
                    args.Add("-NOTEXTURESTREAMING");

                if (globalConfig.UseAllCores)
                    args.Add("-USEALLAVAILABLECORES");

                //  Create log pipe
                var pipeName = "bns_" + Guid.NewGuid().ToString().Replace("-", "");

                SetEnvIfNotEmpty("__COMPAT_LAYER", "RUNASINVOKER");
                SetEnvIfNotEmpty("BNS_PROFILE_XML", profile.BnsPatchPath);
                SetEnvIfNotEmpty("BNS_LOG", pipeName);

                if (profile.HasBins)
                {
                    if (!_fs.File.Exists(profile.BinPath))
                    {
                        _logger.Log($"Bin doesn't exist at path: '{profile.BinPath}'");
                        goto SKIP;
                    }

                    if (!_fs.File.Exists(profile.LocalBinPath))
                    {
                        _logger.Log($"Local bin doesn't exist at path: '{profile.LocalBinPath}'");
                        goto SKIP;
                    }

                    SetEnvIfNotEmpty("BNS_PROFILE_DATAFILE", profile.BinPath);
                    SetEnvIfNotEmpty("BNS_PROFILE_LOCALFILE", profile.LocalBinPath);

                    SKIP: ;
                }

                var arguments = string.Join(" ", args);

                _logger.Log($"Starting game with arguments: '{arguments}'");
                _logger.Log($"Log pipe name: '{pipeName}'");

                // Start client
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = clientPath,
                        Arguments = arguments,
                        UseShellExecute = false
                    },
                    EnableRaisingEvents = true,
                };

                var namedPipeToLog = new NamedPipeToLog(_logger, pipeName);

                process.Exited += (sender, eventArgs) =>
                {
                    OnProcessExit?.Invoke(process);

                    namedPipeToLog.Close();

                    _logger.Log($"Process {process.Id} has exited");
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

        private void SetEnvIfNotEmpty(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            Environment.SetEnvironmentVariable(name, value);
            _logger.Log($"Set env. variable: {name}={value}");
        }
    }
}