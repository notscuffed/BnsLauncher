using System.ComponentModel;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class SettingsViewModel : Screen
    {
        private readonly IGameConfigStorage _gameConfigStorage;
        
        public SettingsViewModel(GameConfig gameConfig, IGameConfigStorage gameConfigStorage)
        {
            GameConfig = gameConfig;
            _gameConfigStorage = gameConfigStorage;
            GameConfig.PropertyChanged += GameConfigOnPropertyChanged;
        }

        public GameConfig GameConfig { get; set; }

        private void GameConfigOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _gameConfigStorage.SaveConfig(GameConfig);
        }
    }
}