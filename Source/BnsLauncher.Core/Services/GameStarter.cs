using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void Start(Profile profile, GlobalConfig globalConfig, Account account)
        {
            try
            {
                if (!GetClientPath(profile, globalConfig, out var clientPath))
                    return;

                if (!ClientExists(clientPath))
                    return;

                var arguments = GetArguments(profile, globalConfig);
                var logPipeName = GenerateLogPipeName();

                SetEnvironmentVariables(profile, account);

                _logger.Log($"Starting game with arguments: '{arguments}'");

                // Start client
                var processInfo = CreateProcess(clientPath, arguments, logPipeName);

                if (processInfo == null)
                    return;

                profile.AddProcessInfo(processInfo);
            }
            catch (Exception exception)
            {
                _logger.Log("Exception occured while starting client");
                _logger.Log(exception);
            }
        }

        private ProcessInfo CreateProcess(string clientPath, string arguments, string logPipeName)
        {
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

            var namedPipeToLog = new NamedPipeToLog(_logger, logPipeName);

            process.Exited += (sender, eventArgs) =>
            {
                OnProcessExit?.Invoke(process);

                namedPipeToLog.Close();

                _logger.Log($"Process {process.Id} has exited");
            };

            if (!process.Start())
            {
                _logger.Log("Failed to start process");
                return null;
            }

            namedPipeToLog.LogPrefix = $"(Pid: {process.Id}) ";
            namedPipeToLog.StartLogging();

            return new ProcessInfo {Process = process};
        }

        private void SetEnvironmentVariables(Profile profile, Account account)
        {
            // Make the game not required to run as admin
            SetEnvIfNotEmpty("__COMPAT_LAYER", "RUNASINVOKER");

            SetEnvIfNotEmpty("BNS_PROFILE_XML", profile.BnsPatchPath);

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

            if (profile.AllowAccounts
                && !string.IsNullOrWhiteSpace(account.Username)
                && !string.IsNullOrWhiteSpace(account.Password))
            {
                SetEnvIfNotEmpty("BNS_PROFILE_USERNAME", account.Username, true);
                SetEnvIfNotEmpty("BNS_PROFILE_PASSWORD", account.Password, true);
            }

            if (profile.AllowPin
                && !string.IsNullOrWhiteSpace(account.Pin))
            {
                SetEnvIfNotEmpty("BNS_PINCODE", account.Pin, true);
            }
        }

        private string GenerateLogPipeName()
        {
            var pipeName = "bns_" + Guid.NewGuid().ToString().Replace("-", "");

            _logger.Log($"Log pipe name: '{pipeName}'");
            SetEnvIfNotEmpty("BNS_LOG", pipeName);

            return pipeName;
        }

        private bool GetClientPath(Profile profile, GlobalConfig globalConfig, out string clientPath)
        {
            if (!string.IsNullOrWhiteSpace(profile.ClientPath))
            {
                _logger.Log($"Using profile client path : '{profile.ClientPath}'");
                clientPath = profile.ClientPath;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(globalConfig.ClientPath))
            {
                _logger.Log($"Using global client path: '{globalConfig.ClientPath}'");
                clientPath = globalConfig.ClientPath;
                return true;
            }

            _logger.Log("Client path is not set in either profile or global config");
            clientPath = null;
            return false;
        }

        private bool ClientExists(string clientPath)
        {
            if (string.IsNullOrWhiteSpace(clientPath))
            {
                _logger.Log("Client path is empty");
                return false;
            }

            if (!_fs.File.Exists(clientPath))
            {
                _logger.Log($"Client doesn't exist at path: '{clientPath}'");
                return false;
            }

            return true;
        }

        private string GetArguments(Profile profile, GlobalConfig globalConfig)
        {
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

            return string.Join(" ", args);
        }
        
        private void SetEnvIfNotEmpty(string name, string value, bool censor = false)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            Environment.SetEnvironmentVariable(name, value);

            _logger.Log($"Set env. variable: {name}={(censor ? "***" : value)}");
        }
    }
}