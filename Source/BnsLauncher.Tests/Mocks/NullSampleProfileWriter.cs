using BnsLauncher.Core.Abstractions;

namespace BnsLauncher.Tests.Mocks
{
    public class NullSampleProfileWriter : ISampleProfileWriter
    {
        public void WriteSampleProfiles(string profilesDirectory)
        {
        }
    }
}