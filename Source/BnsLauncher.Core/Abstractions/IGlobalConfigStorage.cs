using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Abstractions
{
    public interface IGlobalConfigStorage
    {
        void SaveConfig(GlobalConfig config);
        GlobalConfig LoadConfig();
    }
}