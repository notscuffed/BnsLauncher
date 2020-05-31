using System;

namespace BnsLauncher
{
    public static class Constants
    {
        public static readonly string AppDataPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
            "\\BnSLauncher";

        public static readonly string ConfigPath = AppDataPath + "\\config.json";
        public static readonly string ProfilesPath = AppDataPath + "\\Profiles";
    }
}