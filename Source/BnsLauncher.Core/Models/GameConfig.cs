namespace BnsLauncher.Core.Models
{
    public class GameConfig : PropertyChangedBase
    {
        public string ClientPath { get; set; }
        public bool Unattended { get; set; }
        public bool NoTextureStreaming { get; set; }
        public bool UseAllCores { get; set; }
    }
}