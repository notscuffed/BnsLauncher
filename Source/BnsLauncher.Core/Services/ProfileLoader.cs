using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Services
{
    public class ProfileLoader : IProfileLoader
    {
        private readonly ILogger _logger;

        public ProfileLoader(ILogger logger)
        {
            _logger = logger;
        }

        public Task<List<Profile>> LoadProfiles(string sourceDirectory)
        {
            _logger.Log("Loading profiles..");
            
            var profileList = new List<Profile>();

            foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*.xml"))
            {
                try
                {
                    _logger.Log($"Loading xml profile: {file}");

                    var document = new XmlDocument();
                    document.Load(file);

                    var profile = LoadProfileFromXml(document);
                    profile.ProfilePath = Path.GetFullPath(file);
                    profileList.Add(profile);
                }
                catch (Exception exception)
                {
                    _logger.Log("Exception has occured while loading profile:");
                    _logger.Log(exception);
                }
            }

            return Task.FromResult(profileList);
        }

        private Profile LoadProfileFromXml(XmlNode root)
        {
            var profile = new Profile();

            var patches = root.SelectSingleNode("/patches");

            if (patches == null)
            {
                _logger.Log("Failed to find root node: 'patches'");
                return null;
            }

            profile.Name = patches.Attributes?["name"].Value ?? root.Name;
            profile.Background = patches.Attributes?["background"].Value ?? "gray";
            profile.Foreground = patches.Attributes?["foreground"].Value ?? "white";

            (profile.Ip, profile.Port) = GetIpPort(patches);

            return profile;
        }

        private (string, ushort) GetIpPort(XmlNode root)
        {
            var lobbyGateAddress =
                root.SelectSingleNode("//select-node[contains(@query, \"'lobby-gate-address'\")]/set-value");
            var lobbyGatePort =
                root.SelectSingleNode("//select-node[contains(@query, \"'lobby-gate-port'\")]/set-value");

            var address = lobbyGateAddress?.Attributes?["value"].Value;
            var portString = lobbyGatePort?.Attributes?["value"].Value;

            if (lobbyGateAddress == null || string.IsNullOrWhiteSpace(address))
            {
                _logger.Log("Failed to get lobby-gate-address");
                return (null, 0);
            }

            if (!ushort.TryParse(portString, out var port)
                || lobbyGatePort == null
                || string.IsNullOrWhiteSpace(portString))
            {
                _logger.Log("Failed to get lobby-gate-port");
                return (null, 0);
            }

            return (address, port);
        }
    }
}