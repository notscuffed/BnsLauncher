using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using BnsLauncher.Core.Abstractions;

namespace BnsLauncher.Core.Services
{
    public class SampleProfileWriter : ISampleProfileWriter
    {
        private const string SamplesNamespace = "BnsLauncher.Core.Samples.";
        private static readonly Assembly _assembly = typeof(ProfileLoader).Assembly;
        private readonly IFileSystem _fs;
        private readonly ILogger _logger;

        public SampleProfileWriter(IFileSystem fs, ILogger logger)
        {
            _fs = fs;
            _logger = logger;
        }

        public void WriteSampleProfiles(string profilesDirectory)
        {
            var profiles = _assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(SamplesNamespace))
                .GroupBy(x =>
                {
                    var index = x.IndexOf('.', SamplesNamespace.Length);

                    return index == -1
                        ? null
                        : x.Substring(SamplesNamespace.Length, index - SamplesNamespace.Length);
                });

            foreach (var profile in profiles)
            {
                WriteProfile(profilesDirectory, profile.Key, profile);
            }
        }

        private void WriteProfile(string rootDirectory, string profileName, IEnumerable<string> resources)
        {
            _logger.Log($"Writing sample profile: {profileName}");
            
            var profileDirectory = _fs.Path.Combine(rootDirectory, profileName);
            _fs.Directory.CreateDirectory(profileDirectory);

            foreach (var resource in resources)
            {
                using var resourceStream = _assembly.GetManifestResourceStream(resource);

                if (resourceStream == null)
                {
                    _logger.Log($"Failed to get resourceStream of '{resource}'");
                    return;
                }

                var fileName = resource.Substring(SamplesNamespace.Length + profileName.Length + 1);

                try
                {
                    var filePath = _fs.Path.Combine(profileDirectory, fileName);
                    using var outputStream =
                        _fs.File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

                    resourceStream.CopyTo(outputStream);
                }
                catch (Exception exception)
                {
                    _logger.Log("Exception has occured while copying sample.xml profile");
                    _logger.Log(exception);
                }
            }
        }
    }
}