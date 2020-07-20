using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;
using BnsLauncher.Core.Services;
using NUnit.Framework;

namespace BnsLauncher.Tests
{
    [TestFixture]
    public class ProfileAccountsGetterTests
    {
        private IProfileAccountsGetter _profileAccountsGetter;

        [SetUp]
        public void SetUp()
        {
            _profileAccountsGetter = new ProfileAccountsGetter();
        }
        
        [Test]
        public void MatchOneEndWildcard()
        {
            var accounts = new[]
            {
                new Account {ProfilePatterns = "Hello*"},
                new Account {ProfilePatterns = "Dont match"},
            };

            var profile = new Profile {Name = "Hello world"};

            var matchedAccounts = _profileAccountsGetter.GetAccountsForProfile(profile, accounts);

            Assert.AreEqual(1, matchedAccounts.Length, "Only one account should match");
            Assert.AreEqual("Hello*", matchedAccounts[0].ProfilePatterns);
        }

        [Test]
        public void NoMatch()
        {
            var accounts = new[]
            {
                new Account {ProfilePatterns = "*Hello*"},
                new Account {ProfilePatterns = "Dont match"},
            };

            var profile = new Profile {Name = "Wrong"};

            var matchedAccounts = _profileAccountsGetter.GetAccountsForProfile(profile, accounts);

            Assert.IsEmpty(matchedAccounts, "No account should match");
        }

        [Test]
        public void MatchAllFullWildcard()
        {
            var accounts = new[]
            {
                new Account {ProfilePatterns = "*"},
                new Account {ProfilePatterns = "*"},
            };

            var profile = new Profile {Name = "Hello world"};

            var matchedAccounts = _profileAccountsGetter.GetAccountsForProfile(profile, accounts);

            Assert.AreEqual(2, matchedAccounts.Length, "Two accounts must match");
        }
        
        [Test]
        public void EmptyPatternMatchAll()
        {
            var accounts = new[]
            {
                new Account {ProfilePatterns = ""},
                new Account {ProfilePatterns = " "},
            };

            var profile = new Profile {Name = "Hello world"};

            var matchedAccounts = _profileAccountsGetter.GetAccountsForProfile(profile, accounts);

            Assert.AreEqual(2, matchedAccounts.Length, "All accounts must match");
        }
    }
}