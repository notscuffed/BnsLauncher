using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Services;
using BnsLauncher.Tests.Helpers;
using BnsLauncher.Tests.Mocks;
using NUnit.Framework;

namespace BnsLauncher.Tests
{
    [TestFixture]
    public class ProfileLoaderTests
    {
        private ILogger _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new MockLogger();
        }

        [Test]
        public async Task SamplePrivateServerProfile()
        {
            var fs = new MockFileSystem();
            fs.WriteSampleProfile(
                "C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles\\PrivateServer",
                "PrivateServer");

            var profileLoader = new ProfileLoader(fs, _logger, new NullSampleProfileWriter());
            var profiles = await profileLoader.LoadProfiles(
                "C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles");

            Assert.AreEqual(1, profiles.Count, "Must only load 1 profile");

            var profile = profiles.First();

            Assert.AreEqual(
                "C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles\\PrivateServer",
                profile.ProfilePath,
                "Profile paths must be equal");
            
            Assert.AreEqual(
                "C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles\\PrivateServer\\bnspatch.xml",
                profile.BnsPatchPath,
                "Bns patch paths must be equal");
            
            Assert.AreEqual("Private Server", profile.Name, "Names must be equal");
            Assert.AreEqual('P', profile.Initial, "Initials must be equal");
            Assert.AreEqual("red", profile.Background, "Backgrounds must be equal");
            Assert.AreEqual("white", profile.Foreground, "Foregrounds must be equal");
            Assert.AreEqual(".\\bin\\Client.exe", profile.ClientPath, "Client paths must be equal");
            Assert.AreEqual("/LaunchByLauncher /LoginMode 2", profile.Arguments, "Arguments must be equal");
            
            Assert.AreEqual("127.0.0.1", profile.Ip, "Ips must be equal");
            Assert.AreEqual(10900, profile.Port, "Ports must be equal");
            
            Assert.IsNull(profile.BinPath, "Bin path must be null");
            Assert.IsNull(profile.LocalBinPath, "Local bin path must be null");
        }
        
        [Test]
        public async Task SampleLiveEU32Profile()
        {
            var fs = new MockFileSystem();
            fs.WriteSampleProfile(
                "C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles\\LiveEU32",
                "LiveEU32");

            var profileLoader = new ProfileLoader(fs, _logger, new NullSampleProfileWriter());
            var profiles = await profileLoader.LoadProfiles(
                "C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles");

            Assert.AreEqual(1, profiles.Count, "Must only load 1 profile");

            var profile = profiles.First();

            Assert.AreEqual(
                "C:\\Users\\TestUser\\AppData\\Local\\BnSLauncher\\Profiles\\LiveEU32",
                profile.ProfilePath,
                "Profile paths must be equal");
            
            Assert.IsNull(profile.BnsPatchPath, "Bns patch path must be null");
            
            Assert.AreEqual("Live EU 32-bit", profile.Name, "Names must be equal");
            Assert.AreEqual('L', profile.Initial, "Initials must be equal");
            Assert.AreEqual("green", profile.Background, "Backgrounds must be equal");
            Assert.AreEqual("white", profile.Foreground, "Foregrounds must be equal");
            Assert.AreEqual("C:\\Program Files\\BnS\\bin\\Client.exe", profile.ClientPath, "Client paths must be equal");
            Assert.AreEqual("/sesskey /launchbylauncher -lang:English -region:1", profile.Arguments, "Arguments must be equal");
            
            Assert.IsNull(profile.Ip, "Ip must be null");
            Assert.AreEqual(0, profile.Port, "Ports must be 0");
            
            Assert.IsNull(profile.BinPath, "Bin path must be null");
            Assert.IsNull(profile.LocalBinPath, "Local bin path must be null");
        }
    }
}