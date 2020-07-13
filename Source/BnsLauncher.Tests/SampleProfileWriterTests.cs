using System.IO.Abstractions.TestingHelpers;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Services;
using BnsLauncher.Tests.Mocks;
using NUnit.Framework;

namespace BnsLauncher.Tests
{
    [TestFixture]
    public class SampleProfileWriterTests
    {
        private ILogger _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new MockLogger();
        }

        [Test]
        public void WriteSampleProfile()
        {
            var fs = new MockFileSystem();
            var writer = new SampleProfileWriter(fs, _logger);
            writer.WriteSampleProfiles("C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles");

            var profileFileCount = fs.Directory
                .GetFiles("C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles\\PrivateServer").Length;
            
            Assert.AreEqual(4, profileFileCount, "Private server profile folder must contain 4 files after writing");
        }
    }
}