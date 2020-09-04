using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Services
{
    public class ProfileLoader : IProfileLoader
    {
        private readonly IFileSystem _fs;
        private readonly ILogger _logger;
        private readonly ISampleProfileWriter _sampleProfileWriter;

        public ProfileLoader(IFileSystem fs, ILogger logger, ISampleProfileWriter sampleProfileWriter)
        {
            _fs = fs;
            _logger = logger;
            _sampleProfileWriter = sampleProfileWriter;
        }

        public Task<List<Profile>> LoadProfiles(string sourceDirectory)
        {
            _logger.Log("Loading profiles..");

            var directoryInfo = _fs.DirectoryInfo.FromDirectoryName(sourceDirectory);

            // Copy sample profiles if none found
            if (!directoryInfo.Exists || directoryInfo.GetDirectories().Length == 0)
                _sampleProfileWriter.WriteSampleProfiles(sourceDirectory);

            var profileList = _fs.Directory.EnumerateDirectories(directoryInfo.FullName)
                .Select(LoadProfile)
                .Where(profile => profile != null)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.Name)
                .ToList();

            return Task.FromResult(profileList);
        }

        private Profile LoadProfile(string profileRoot)
        {
            var files = _fs.Directory
                .GetFiles(profileRoot, "*.xml")
                .ToDictionary(_fs.Path.GetFileName, _fs.File.ReadAllText, StringComparer.OrdinalIgnoreCase);

            if (!files.TryGetValue("profile.xml", out var profileXmlContent))
            {
                _logger.Log($"profile.xml is missing from folder: '{profileRoot}'");
                return null;
            }

            _logger.Log($"Loading xml profile: '{profileRoot}'");

            Profile profile;

            try
            {
                var profileDocument = new XmlDocument();
                profileDocument.LoadXml(profileXmlContent);

                profile = LoadProfile(profileDocument);
                profile.ProfilePath = profileRoot;
            }
            catch (Exception exception)
            {
                _logger.Log("Exception has occured while loading profile:");
                _logger.Log(exception);
                return null;
            }

            if (files.TryGetValue("loginhelper.xml", out var loginhelperXmlContent))
            {
                try
                {
                    LoadLoginHelper(profile, loginhelperXmlContent);
                }
                catch (Exception exception)
                {
                    _logger.Log("Exception has occured while reading loginhelper.xml");
                    _logger.Log(exception);
                }
            }

            if (files.TryGetValue("bnspatch.xml", out var bnspatchXmlContent))
            {
                try
                {
                    profile.BnsPatchPath = _fs.Path.Combine(profileRoot, "bnspatch.xml");
                    (profile.Ip, profile.Port) = LoadIpPortFromBnsPatch(bnspatchXmlContent);
                }
                catch (Exception exception)
                {
                    _logger.Log("Exception has occured while reading bnspatch.xml");
                    _logger.Log(exception);
                }
            }

            if (files.TryGetValue("binloader.xml", out var binloaderXmlContent))
            {
                try
                {
                    (profile.BinPath, profile.LocalBinPath) = LoadBinsFromBinLoader(profileRoot, binloaderXmlContent);
                }
                catch (Exception exception)
                {
                    _logger.Log("Exception has occured while reading binloader.xml");
                    _logger.Log(exception);
                }
            }

            return profile;
        }

        private void LoadLoginHelper(Profile profile, string loginhelperXmlContent)
        {
            var root = new XmlDocument();
            root.LoadXml(loginhelperXmlContent);

            var loginhelper = root.SelectSingleNode("/loginhelper");

            if (loginhelper == null)
            {
                _logger.Log("[LoadLoginHelper] Failed to find root node: 'loginhelper'");
                return;
            }

            profile.AllowPin = loginhelper.GetNodeBoolean("./allow-pin", false);
            profile.AllowAccounts = loginhelper.GetNodeBoolean("./allow-accounts", false);
            profile.AutopinOnRelog = loginhelper.GetNodeBoolean("./autopin-on-relog", false);
        }

        private Profile LoadProfile(XmlNode root)
        {
            var profile = new Profile();

            var profileRoot = root.SelectSingleNode("/profile");

            if (profileRoot == null)
            {
                _logger.Log("[LoadProfileFromXml] Failed to find root node: 'profile'");
                return null;
            }

            profile.Name = profileRoot.GetNodeText("./name", "No name");
            profile.Background = profileRoot.GetNodeText("./background", "gray");
            profile.Foreground = profileRoot.GetNodeText("./foreground", "white");
            profile.ClientPath = profileRoot.GetNodeText("./clientpath", null);
            profile.Arguments = profileRoot.GetNodeText("./arguments", null);
            var priorityString = profileRoot.GetNodeText("./priority", "0");

            if (int.TryParse(priorityString, out var priority))
            {
                profile.Priority = priority;
            }
            else
            {
                _logger.Log($"[LoadProfileFromXml] Invalid priority string: '{priorityString}'");
            }


            return profile;
        }

        private (string bin, string localbin) LoadBinsFromBinLoader(string profileRoot, string xml)
        {
            var root = new XmlDocument();
            root.LoadXml(xml);

            var binloaderRoot = root.SelectSingleNode("/binloader");

            if (binloaderRoot == null)
            {
                _logger.Log("[LoadBinsFromBinLoader] Failed to find root node: 'binloader'");
                return (null, null);
            }

            var bin = binloaderRoot.GetNodeText("./bin", null);
            var localBin = binloaderRoot.GetNodeText("./localbin", null);

            if (string.IsNullOrWhiteSpace(bin) || string.IsNullOrWhiteSpace(localBin))
                return (null, null);

            return (
                _fs.Path.Combine(profileRoot, Environment.ExpandEnvironmentVariables(bin)),
                _fs.Path.Combine(profileRoot, Environment.ExpandEnvironmentVariables(localBin))
            );
        }

        private static (string ip, ushort port) LoadIpPortFromBnsPatch(string xml)
        {
            var root = new XmlDocument();
            root.LoadXml(xml);

            var lobbyGateAddress =
                root.SelectSingleNode("//select-node[contains(@query, \"'lobby-gate-address'\")]/set-value");
            var lobbyGatePort =
                root.SelectSingleNode("//select-node[contains(@query, \"'lobby-gate-port'\")]/set-value");

            var address = lobbyGateAddress?.Attributes?["value"].Value;
            var portString = lobbyGatePort?.Attributes?["value"].Value;

            if (lobbyGateAddress == null || string.IsNullOrWhiteSpace(address))
                return (null, 0);

            if (!ushort.TryParse(portString, out var port)
                || lobbyGatePort == null
                || string.IsNullOrWhiteSpace(portString))
                return (null, 0);

            return (address, port);
        }
    }
}