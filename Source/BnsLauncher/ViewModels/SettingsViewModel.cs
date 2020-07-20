using BnsLauncher.Core.Models;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class SettingsViewModel : Screen
    {
        public SettingsViewModel(GlobalConfig globalConfig)
        {
            GlobalConfig = globalConfig;
        }

        public GlobalConfig GlobalConfig { get; set; }
    }
}