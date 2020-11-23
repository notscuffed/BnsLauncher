using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Services
{
    public class GameStarter : IGameStarter
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fs;
        private readonly Dictionary<string, string> _env = new Dictionary<string, string>();

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

                processInfo.Account = account;

                profile.AddProcessInfo(processInfo);

                ClearEnvironmentVariables();
            }
            catch (Exception exception)
            {
                _logger.Log("Exception occured while starting client");
                _logger.Log(exception);
            }
        }

        private ProcessInfo CreateProcess(string clientPath, string arguments, string logPipeName)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = clientPath,
                Arguments = arguments,
                UseShellExecute = false,
            };

            var clientBinDirectory = _fs.Path.GetDirectoryName(_fs.Path.GetFullPath(clientPath));
            if (clientBinDirectory != null)
            {
                processStartInfo.WorkingDirectory = clientBinDirectory;
                _logger.Log($"Setting client working directory to: '{clientBinDirectory}'");
            }
            else
            {
                _logger.Log($"Failed to get client working directory from path: '{clientPath}'");
            }

            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true,
            };

            foreach (var kp in _env)
            {
                processStartInfo.EnvironmentVariables[kp.Key] = kp.Value;
            }

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

        private void ClearEnvironmentVariables()
        {
            _env.Clear();
        }

        private void SetEnvironmentVariables(Profile profile, Account account)
        {
            ClearEnvironmentVariables();

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

            // Set custom environment variables
            foreach (var kp in profile.CustomEnvironmentVariables)
            {
                SetEnvIfNotEmpty(kp.Key, kp.Value);
            }

            // Account stuff
            if (account == null)
                return;

            if (!profile.AllowAccounts)
            {
                _logger.Log("Profile disallows automatic login");
                return;
            }

            if (string.IsNullOrWhiteSpace(account.Username))
            {
                _logger.Log("Account username is empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(account.Password))
            {
                _logger.Log("Account password is empty");
                return;
            }

            SetEnvIfNotEmpty("BNS_PROFILE_USERNAME", account.Username, true);
            SetEnvIfNotEmpty("BNS_PROFILE_PASSWORD", account.Password, true);

            if (!profile.AllowPin)
            {
                _logger.Log("Profile disallows automatic pin");
                return;
            }

            var pin = account.Pin;

            if (string.IsNullOrEmpty(pin))
                return;

            if (pin.Length != 6)
            {
                _logger.Log($"Invalid PIN length: {pin.Length}");
                return;
            }

            if (!pin.All(char.IsDigit))
            {
                _logger.Log("PIN must be a 6 digit number");
                return;
            }

            SetEnvIfNotEmpty("BNS_PINCODE", account.Pin, true);
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

            _env[name] = value;

            _logger.Log($"Set env. variable: {name}={(censor ? "***" : value)}");
        }

        private void ClearEnv(string name)
        {
            _env.Remove(name);
        }
    }
}