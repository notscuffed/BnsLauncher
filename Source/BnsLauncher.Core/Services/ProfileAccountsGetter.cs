using System;
using System.Collections.Generic;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Services
{
    public class ProfileAccountsGetter : IProfileAccountsGetter
    {
        public Account[] GetAccountsForProfile(Profile profile, IEnumerable<Account> accountsSource)
        {
            var matchedAccounts = new List<Account>();
            var profileName = profile.Name;

            foreach (var account in accountsSource)
            {
                if (string.IsNullOrWhiteSpace(account.ProfilePatterns))
                    goto ADD;

                var patterns = account.ProfilePatterns
                    .Split(new []{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var pattern in patterns)
                {
                    if (profileName.WildMatch(pattern))
                        goto ADD;
                }
                
                continue;
                
                ADD:
                matchedAccounts.Add(account);
            }

            return matchedAccounts.ToArray();
        }
    }
}