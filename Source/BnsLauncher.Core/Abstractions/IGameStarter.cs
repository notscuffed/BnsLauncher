using BnsLauncher.Core.Models;

namespace BnsLauncher.Core.Abstractions
{
    public interface IGameStarter
    {
        void Start(Profile profile, GameConfig gameConfig);
    }
}