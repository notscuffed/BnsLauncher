using System.IO.Abstractions;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;
using Newtonsoft.Json;

namespace BnsLauncher.Core.Services
{
    public class JsonGlobalConfigStorage : IGlobalConfigStorage
    {
        private readonly IFileSystem _fs;

        public JsonGlobalConfigStorage(IFileSystem fs)
        {
            _fs = fs;
        }

        public string ConfigPath { get; set; }

        public void SaveConfig(GlobalConfig config)
        {
            _fs.File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config));
        }

        public GlobalConfig LoadConfig()
        {
            return _fs.File.Exists(ConfigPath)
                ? JsonConvert.DeserializeObject<GlobalConfig>(_fs.File.ReadAllText(ConfigPath))
                : new GlobalConfig();
        }
    }
}