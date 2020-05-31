using System.IO;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;
using Newtonsoft.Json;

namespace BnsLauncher.Core.Services
{
    public class JsonGameConfigStorage : IGameConfigStorage
    {
        public string ConfigPath { get; set; }

        public void SaveConfig(GameConfig config)
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config));
        }

        public GameConfig LoadConfig()
        {
            return File.Exists(ConfigPath)
                ? JsonConvert.DeserializeObject<GameConfig>(File.ReadAllText(ConfigPath))
                : new GameConfig();
        }
    }
}