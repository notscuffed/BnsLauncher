namespace BnsLauncher.Core.Models
{
    public class GlobalConfig : PropertyChangedBase
    {
        public string ClientPath { get; set; }
        public string Arguments { get; set; }
        public bool Unattended { get; set; } = true;
        public bool NoTextureStreaming { get; set; } = true;
        public bool UseAllCores { get; set; } = true;
    }
}