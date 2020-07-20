using System.Collections.Generic;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Abstractions
{
    public interface IProfileAccountsGetter
    {
        /// <summary>
        /// Returns list of accounts for specific profile
        /// </summary>
        /// <param name="profile">The profile to get accounts for</param>
        /// <param name="accountsSource">Source of accounts</param>
        /// <returns></returns>
        Account[] GetAccountsForProfile(Profile profile, IEnumerable<Account> accountsSource);
    }
}