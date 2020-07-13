using System.ComponentModel;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class SettingsViewModel : Screen
    {
        private readonly IGlobalConfigStorage _globalConfigStorage;
        
        public SettingsViewModel(GlobalConfig globalConfig, IGlobalConfigStorage globalConfigStorage)
        {
            GlobalConfig = globalConfig;
            _globalConfigStorage = globalConfigStorage;
            GlobalConfig.PropertyChanged += GameConfigOnPropertyChanged;
        }

        public GlobalConfig GlobalConfig { get; set; }

        private void GameConfigOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _globalConfigStorage.SaveConfig(GlobalConfig);
        }
    }
}