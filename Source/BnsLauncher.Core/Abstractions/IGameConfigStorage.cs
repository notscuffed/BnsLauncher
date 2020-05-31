using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Abstractions
{
    public interface IGameConfigStorage
    {
        void SaveConfig(GameConfig config);
        GameConfig LoadConfig();
    }
}