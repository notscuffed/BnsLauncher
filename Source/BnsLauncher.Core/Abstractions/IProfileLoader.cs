using System.Collections.Generic;
using System.Threading.Tasks;
using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Abstractions
{
    public interface IProfileLoader
    {
        Task<List<Profile>> LoadProfiles(string sourceDirectory);
    }
}